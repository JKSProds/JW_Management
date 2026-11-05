using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

// Associa os publicadores da aplicação aos utilizadores do Hourglass (só leitura no Hourglass) e propõe
// atualizar email, telemóvel, morada e grupo, ou acrescentar quem ainda não existe. Não altera funções (permissões).
public class HourglassSincronizador(HourglassContext _hg, DbContext _db)
{
    public async Task<SyncPreview> Pre(string cookie)
    {
        var tUsers = _hg.Get(cookie, "/fsreport/users");
        var tGrupos = _hg.Get(cookie, "/fsreport/fsgroups");
        var tMoradas = _hg.Get(cookie, "/fsreport/addresses");
        await Task.WhenAll(tUsers, tGrupos, tMoradas);

        var locais = _db.ObterPublicadores(true).Where(p => p.Id > 0).ToList();
        var gruposLocais = _db.ObterGrupos().Where(g => g.Id > 0).ToList();
        var hgUsers = (tUsers.Result["users"] as JArray ?? new()).OfType<JObject>().ToList();
        var moradas = (tMoradas.Result as JArray ?? new()).ToDictionary(a => (int)a["id"]!, a => Morada(a));
        var hgGrupos = (tGrupos.Result as JArray ?? new()).ToDictionary(g => (int)g["id"]!, g => g);

        string NomeDe(JObject u) => $"{(string?)u["firstname"]} {(string?)u["lastname"]}".Trim();
        Publicador? Achar(JObject u)
        {
            var email = ((string?)u["email"] ?? "").Trim();
            if (email != "") { var porMail = locais.Where(p => string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase)).ToList(); if (porMail.Count == 1) return porMail[0]; }
            var desc = (string?)u["descriptor"];
            return HourglassImportador.Corresponder((string?)u["firstname"] ?? "", (string?)u["lastname"] ?? "", NomeDe(u), locais)
                ?? (string.IsNullOrWhiteSpace(desc) ? null : locais.FirstOrDefault(p => HourglassImportador.Norm(p.Nome) == HourglassImportador.Norm(desc)));
        }

        // Cada utilizador do Hourglass -> publicador local (ou nenhum)
        var par = hgUsers.ToDictionary(u => (int)u["id"]!, u => Achar(u));
        // Grupo local equivalente: pelo responsável do grupo; senão pelo número no nome («Grupo 5»)
        int GrupoLocal(int hgGrupoId)
        {
            if (!hgGrupos.TryGetValue(hgGrupoId, out var g)) return 0;
            var resp = par.TryGetValue((int?)g["overseer_id"] ?? 0, out var pr) ? pr : null;
            var porResp = resp == null ? null : gruposLocais.FirstOrDefault(x => x.Responsavel?.Id == resp.Id);
            if (porResp != null) return porResp.Id;
            var n = Regex.Match((string?)g["name"] ?? "", @"\d+").Value;
            return n == "" ? 0 : gruposLocais.FirstOrDefault(x => Regex.IsMatch(x.Nome ?? "", @"(?<!\d)" + n + @"(?!\d)"))?.Id ?? 0;
        }

        var preview = new SyncPreview();
        var usadas = new HashSet<int>();
        foreach (var u in hgUsers.OrderBy(NomeDe))
        {
            var local = par[(int)u["id"]!];
            if (local != null && !usadas.Add(local.Id)) local = null;     // dois do Hourglass para o mesmo local: o segundo é tratado como novo
            var email = ((string?)u["email"] ?? "").Trim();
            var tel = ((string?)u["cellphone"] ?? "").Trim();
            var mor = moradas.TryGetValue((int?)u["address_id"] ?? 0, out var m) ? m : "";
            var grupo = GrupoLocal((int?)u["group_id"] ?? 0);
            var nome = string.IsNullOrWhiteSpace((string?)u["descriptor"]) ? NomeDe(u) : ((string)u["descriptor"]!).Trim();
            var linha = new SyncLinha
            {
                HgId = (int)u["id"]!, HgNome = nome, Cargo = string.Join(" · ", new[] { (string?)u["appt"], (string?)u["status"] }.Where(x => !string.IsNullOrWhiteSpace(x))),
                PubId = local?.Id ?? 0, PubNome = local?.Nome ?? "", Email = email, Telemovel = tel, Morada = mor, GrupoId = grupo, GrupoNome = gruposLocais.FirstOrDefault(g => g.Id == grupo)?.Nome ?? "", Aplicar = local == null
            };

            if (local != null)
            {
                void Prop(string campo, string atual, string novo, bool igual, int gid = 0)
                {
                    if (novo == "" || igual) return;
                    linha.Campos.Add(new SyncCampo { Campo = campo, Atual = atual, Novo = novo, GrupoId = gid, Aplicar = atual == "" });   // só preenche por omissão o que está vazio
                }
                Prop("Email", local.Email ?? "", email, string.Equals(local.Email, email, StringComparison.OrdinalIgnoreCase));
                Prop("Telemovel", local.Telemovel ?? "", tel, Digitos(local.Telemovel) == Digitos(tel) || Digitos(local.Telemovel).EndsWith(Digitos(tel)) && Digitos(tel).Length >= 9);
                Prop("Morada", local.Morada ?? "", mor, HourglassImportador.Norm(local.Morada) == HourglassImportador.Norm(mor));
                if (grupo > 0 && local.Grupo?.Id != grupo)
                    linha.Campos.Add(new SyncCampo { Campo = "Grupo", Atual = local.Grupo?.Id > 0 ? local.Grupo.Nome ?? "" : "", Novo = gruposLocais.First(g => g.Id == grupo).Nome ?? "", GrupoId = grupo, Aplicar = (local.Grupo?.Id ?? 0) == 0 });
                if (linha.Campos.Count == 0) { preview.SemAlteracoes++; continue; }
                linha.Aplicar = true;
            }
            preview.Linhas.Add(linha);
        }

        preview.SoLocal = locais.Where(p => !usadas.Contains(p.Id)).OrderBy(p => p.Nome).Select(p => new SyncPessoa { Id = p.Id, Nome = p.Nome ?? "" }).ToList();
        return preview;
    }

    public (int atualizados, int criados) Aplicar(SyncPreview p)
    {
        var locais = _db.ObterPublicadores(true);
        int upd = 0, novos = 0, proximo = locais.Select(x => x.Id).DefaultIfEmpty(0).Max() + 1;
        var usernames = locais.Select(x => x.Username ?? "").ToHashSet();

        foreach (var l in p.Linhas.Where(x => x.Aplicar))
        {
            if (l.PubId > 0)
            {
                var pub = locais.FirstOrDefault(x => x.Id == l.PubId); if (pub == null) continue;
                var alterou = false;
                foreach (var c in l.Campos.Where(c => c.Aplicar))
                {
                    switch (c.Campo)
                    {
                        case "Email": pub.Email = c.Novo; alterou = true; break;
                        case "Telemovel": pub.Telemovel = c.Novo; alterou = true; break;
                        case "Morada": pub.Morada = c.Novo; alterou = true; break;
                        case "Grupo": pub.Grupo = new Grupo { Id = c.GrupoId }; alterou = true; break;
                    }
                }
                if (alterou && _db.AdicionarPublicador(Escapar(pub))) upd++;
            }
            else
            {
                var username = Username(l.HgNome, usernames);
                var novo = new Publicador { Id = proximo, Nome = l.HgNome, Username = username, Password = "", Email = l.Email, Telemovel = l.Telemovel, Morada = l.Morada, Grupo = new Grupo { Id = l.GrupoId } };
                if (_db.AdicionarPublicador(Escapar(novo))) { proximo++; novos++; usernames.Add(username); }
            }
        }
        return (upd, novos);
    }

    // AdicionarPublicador grava TODOS os campos de texto concatenados: escapam-se todos (inclui os que já estavam na base de dados).
    private static Publicador Escapar(Publicador p)
    {
        p.Nome = S(p.Nome); p.Username = S(p.Username); p.Password = S(p.Password); p.Email = S(p.Email); p.Telemovel = S(p.Telemovel); p.Morada = S(p.Morada);
        return p;
    }

    private static string Morada(JToken a)
    {
        var cp = string.Join(" ", new[] { (string?)a["postalcode"], (string?)a["city"] }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.Join(", ", new[] { (string?)a["line1"], (string?)a["line2"], cp }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string Digitos(string? s) => Regex.Replace(s ?? "", @"\D", "");

    private static string Username(string nome, HashSet<string> existentes)
    {
        var d = nome.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in d) if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
        var baseU = sb.Length == 0 ? "utilizador" : sb.ToString(); var u = baseU; int i = 2;
        while (existentes.Contains(u)) u = baseU + i++;
        return u;
    }

    // O DbContext monta SQL por texto: o que vem de fora escreve-se escapado.
    private static string S(string? s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "''");
}
