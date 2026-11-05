using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

// Lê o programa das reuniões do Hourglass (só GET) e grava-o nas tabelas de reuniões que já existem,
// criando cada semana a partir da reunião-base (AdicionarReuniao) e preenchendo-a.
public class HourglassImportador(HourglassContext _hg, DbContext _db)
{
    private long _seq;

    // ---------- Pré-visualização ----------
    public async Task<HgPreview> Pre(string cookie, DateTime de, DateTime ate)
    {
        var tipos = _db.ObterTiposDesignacao();
        var pubs = _db.ObterPublicadores(false).Where(p => p.Id > 0).ToList();
        var existentes = _db.ObterReunioes(false);

        var (utilizadores, lgroup) = await _hg.Utilizadores(cookie);
        var intervalo = $"{de:yyyy-MM-dd}_{ate:yyyy-MM-dd}";
        var tMeeting = _hg.Get(cookie, $"/scheduling/mm/meeting/{intervalo}?lgroup={lgroup}&no_subs=true");
        var tMm = _hg.Get(cookie, $"/scheduling/mm/schedule/{intervalo}?lgroup={lgroup}");
        var tWm = _hg.Get(cookie, $"/scheduling/wm/schedule/{intervalo}?lgroup={lgroup}");
        await Task.WhenAll(tMeeting, tMm, tWm);

        var partes = (tMeeting.Result as JArray ?? new()).ToDictionary(m => (string)m["date"]!, m => (JObject)m);
        var mm = (tMm.Result as JArray ?? new()).ToDictionary(m => (string)m["date"]!, m => (JObject)m);
        var wm = (tWm.Result as JArray ?? new()).ToDictionary(m => (string)m["date"]!, m => (JObject)m);

        var preview = new HgPreview { Tipos = tipos.Select(t => new HgTipo { Id = t.Id, Descricao = t.Descricao ?? "", Adicional = t.DescricaoAdicional ?? "" }).ToList() };

        foreach (var datas in partes.Keys.Union(mm.Keys).Union(wm.Keys).OrderBy(d => d))
        {
            var d = DateTime.ParseExact(datas, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var semana = $"SEMANA {(d.DayOfYear - 1) / 7 + 1} DE {d.Year}";
            var s = new HgSemana { Data = datas, Semana = semana, Stamp = semana.Replace(" ", ""), };
            s.Existe = existentes.Any(r => r.SemanaReuniao == semana || r.Stamp == s.Stamp);

            HgPessoa? P(JToken? t) => Pessoa(t, utilizadores, pubs);
            void Add(string grupo, string chave, string titulo, JToken? quem, JToken? ajudante = null, int min = 0, string tema = "", int sala = 0, int @base = 0, bool obrigatorio = false)
            {
                var tipoHg = chave.Split(':').Last();
                var it = new HgItem
                {
                    Grupo = grupo, Chave = chave, Titulo = titulo, Tema = tema, Minutos = min, Sala = sala == 0 ? "Auditório" : "Sala " + (sala + 1),
                    Rotulo = Rotulo(chave, titulo), Base = @base, Salas = ComSala.Contains(tipoHg)
                };
                foreach (var p in new[] { P(quem), P(ajudante) }) if (p != null) it.Pessoas.Add(p);
                // uma parte sem ninguém atribuído existe na mesma nessa semana: entra «por atribuir»
                if (it.Pessoas.Count == 0 && obrigatorio) it.Pessoas.Add(new HgPessoa());
                if (it.Pessoas.Count > 0) s.Itens.Add(it);
            }

            if (mm.TryGetValue(datas, out var a))
            {
                partes.TryGetValue(datas, out var m);
                Add("mm", "mm:chairman", "Presidente", a["chairman"], obrigatorio: true);
                Add("mm", "mm:openprayer", "Oração inicial", a["openprayer"], obrigatorio: true);
                foreach (var sec in new[] { "tgw", "fm", "lac" })
                    foreach (var e in a[sec] as JArray ?? new())
                    {
                        var parte = (m?[sec] as JArray)?.FirstOrDefault(p => (int?)p["id"] == (int?)e["part"]);
                        if (parte == null) continue;
                        var tipo = (string?)parte["type"] ?? "";
                        Add("mm", $"mm:{sec}:{tipo}", LimparNumero((string?)parte["title"]), e["assignee"], e["assistant"], Minutos((string?)parte["time"]), LimparNumero((string?)parte["title"]), (int?)e["classroom"] ?? 0, sec == "fm" ? 100 : sec == "lac" ? 200 : 0, true);
                    }
                Add("mm", "mm:cbsreader", "Leitor do estudo bíblico", a["cbs_reader"], @base: 200, obrigatorio: true);
                Add("mm", "mm:closeprayer", "Oração final", a["closeprayer"], @base: 200, obrigatorio: true);
                if (m != null)
                {
                    var ks = new[] { "song1", "song2", "song3" };
                    s.Canticos = ks.Select(k => (int?)HourglassContext.Sub(m[k], "number") ?? 0).ToList();
                    s.CanticoTitulos = ks.Select(k => (string?)HourglassContext.Sub(m[k], "title") ?? "").ToList();
                }
            }

            if (wm.TryGetValue(datas, out var w))
            {
                Add("wm", "wm:chairman", "Presidente (fim de semana)", w["wm_chairman"], @base: 300, obrigatorio: true);
                Add("wm", "wm:conductor", "Dirigente de A Sentinela", w["wt_conductor"], @base: 400);
                Add("wm", "wm:reader", "Leitor de A Sentinela", w["wm_reader"], @base: 400, obrigatorio: true);
            }

            // Assembleia de circuito, congresso e memorial: nessa semana não há cânticos da reunião
            var evSemana = (mm.TryGetValue(datas, out var e1) ? e1["event"] as JObject : null) ?? (wm.TryGetValue(datas, out var e2) ? e2["event"] as JObject : null);
            if (evSemana != null && ((string?)evSemana["event"] is "ca" or "rc" or "mmem" || !string.IsNullOrWhiteSpace((string?)evSemana["custom_title"]))) { s.Canticos = new(); s.CanticoTitulos = new(); }

            if (s.Itens.Count == 0) continue;

            var usados = new HashSet<(int, string)>();
            foreach (var it in s.Itens)
            {
                it.TipoId = Sugerir(it, tipos, usados);
                if (it.TipoId > 0) usados.Add((it.TipoId, it.Sala));
            }
            preview.Semanas.Add(s);
        }
        return preview;
    }

    // ---------- Importação ----------
    public (int semanas, int designacoes) Importar(HgPreview p)
    {
        int ns = 0, nd = 0;
        var tipos = _db.ObterTiposDesignacao();

        // 1) Cria os tipos em falta (a tabela r_tipos pode estar vazia).
        var criados = new Dictionary<(int, string), int>();
        foreach (var it in p.Semanas.Where(x => x.Importar).SelectMany(x => x.Itens).Where(i => i.TipoId == -1))
        {
            var chave = (it.Base, Norm(it.Rotulo));
            if (!criados.TryGetValue(chave, out var id))
            {
                var igual = tipos.FirstOrDefault(t => t.Id > it.Base && t.Id < it.Base + 100 && Norm(t.Descricao) == chave.Item2);
                if (igual != null) id = igual.Id;
                else
                {
                    id = Math.Max(it.Base, tipos.Where(t => t.Id > it.Base && t.Id < it.Base + 100).Select(t => t.Id).DefaultIfEmpty(it.Base).Max()) + 1;
                    var novo = new TipoDesignacao { Id = id, Descricao = Cortar(it.Rotulo), DescricaoAdicional = Cortar(it.Rotulo), NMin = it.Minutos, Salas = it.Salas ? 2 : 1 };
                    if (!_db.CriarTipoDesignacao(novo)) throw new InvalidOperationException($"Não foi possível criar o tipo «{it.Rotulo}».");
                    tipos.Add(novo);
                }
                criados[chave] = id;
            }
            it.TipoId = id;
        }

        foreach (var s in p.Semanas.Where(x => x.Importar))
        {
            // Substitui por completo o que a semana tinha. Só se criam as designações que o Hourglass tem nessa semana
            // (não a reunião-base com todos os tipos), por isso não ficam designações que nessa semana não existem.
            var antigos = _db.ObterReunioes(false).Where(r => r.SemanaReuniao == s.Semana || r.Stamp == s.Stamp).Select(r => r.Stamp!).Distinct().ToList();
            if (antigos.Count > 0) _db.ApagarReunioes(antigos);
            ns++;

            var novas = new List<Designacao>();
            foreach (var it in s.Itens.Where(i => i.TipoId > 0))
            {
                var tipo = tipos.FirstOrDefault(t => t.Id == it.TipoId);
                if (tipo == null) continue;
                foreach (var pessoa in it.Pessoas)
                {
                    var pub = new Publicador { Id = pessoa.PubId, Nome = S(pessoa.PubId > 0 ? pessoa.PubNome : pessoa.Nome) };
                    novas.Add(new Designacao
                    {
                        Stamp = (DateTime.Now.Ticks + _seq++).ToString(), StampReuniao = s.Stamp, SemanaReuniao = S(s.Semana),
                        NomeDesignacao = S(it.Tema != "" ? it.Tema : tipo.DescricaoAdicional ?? tipo.Descricao ?? ""), NomePublicador = pub.Nome, Publicador = pub,
                        TipoDesignacao = tipo, Local = it.Sala, NMin = it.Minutos > 0 ? it.Minutos : tipo.NMin
                    });
                }
            }
            if (novas.Count > 0 && !_db.AdicionarDesignacoes(novas)) throw new InvalidOperationException($"Não foi possível gravar as designações de «{s.Semana}».");
            nd += novas.Count;

            // Cânticos pela ordem do programa (início, meio, fim); o catálogo local fica com os títulos do Hourglass.
            _db.GuardarCanticos(s.Canticos.Select((n, i) => (n, i < s.CanticoTitulos.Count ? s.CanticoTitulos[i] : "")));
            foreach (var n in s.Canticos.Where(n => n > 0))
                _db.CriarCantico(s.Stamp, new Cantico { Id = n, Stamp = (DateTime.Now.Ticks + _seq++).ToString() });
        }
        return (ns, nd);
    }

    // ---------- Auxiliares ----------
    private static HgPessoa? Pessoa(JToken? id, Dictionary<int, HgUtilizador> users, List<Publicador> pubs)
    {
        if (id == null || id.Type != JTokenType.Integer || (int)id == 0 || !users.TryGetValue((int)id, out var u)) return null;
        var local = Corresponder(u.Primeiro, u.Ultimo, u.Preferido, pubs) ?? Corresponder(u.Primeiro, u.Ultimo, u.Nome, pubs);
        return new HgPessoa { Nome = u.Preferido, PubId = local?.Id ?? 0, PubNome = local?.Nome ?? "" };
    }

    internal static Publicador? Corresponder(string primeiro, string ultimo, string completo, List<Publicador> pubs)
    {
        var c = Norm(completo); if (c == "") return null;
        var exato = pubs.FirstOrDefault(p => Norm(p.Nome) == c); if (exato != null) return exato;
        var f = Norm(primeiro); var l = Norm(ultimo);
        var cand = pubs.Where(p => { var n = " " + Norm(p.Nome) + " "; return n.Contains(" " + f + " ") && (l == "" || n.Contains(" " + l + " ")); }).ToList();
        return cand.Count == 1 ? cand[0] : cand.FirstOrDefault(p => Norm(p.Nome).StartsWith(f + " ") && Norm(p.Nome).EndsWith(" " + l));
    }

    // Sugere o tipo local: primeiro pelo título exato, depois por palavras-chave. Meio de semana = Ids < 300; fim de semana = 300-499.
    private static readonly Dictionary<string, string[]> Palavras = new()
    {
        ["chairman"] = ["presidente"], ["openprayer"] = ["oracao inicial", "primeira oracao"], ["closeprayer"] = ["oracao final", "ultima oracao"],
        ["cbsreader"] = ["leitor"], ["reader"] = ["leitor"], ["conductor"] = ["dirigente", "sentinela"],
        ["treasures"] = ["tesouros", "discurso"], ["dfg"] = ["perolas"], ["reading"] = ["leitura"], ["initcall"] = ["iniciar"],
        ["return"] = ["revisita", "voltar"], ["study"] = ["discipulos", "estudo"], ["stutalk"] = ["discurso"],
        ["localneeds"] = ["necessidades"], ["cbs"] = ["estudo biblico de congregacao", "estudo biblico"]
    };

    private static int Sugerir(HgItem it, List<TipoDesignacao> tipos, HashSet<(int, string)> usados)
    {
        var cand = tipos.Where(t => t.Id > 0 && (it.Grupo == "mm" ? t.Id < 300 : t.Id is >= 300 and < 500) && !usados.Contains((t.Id, it.Sala))).ToList();
        var rotulo = Norm(it.Rotulo);
        var exato = cand.FirstOrDefault(t => Norm(t.Descricao) == rotulo || Norm(t.DescricaoAdicional) == rotulo);
        if (exato != null) return exato.Id;
        var tipo = it.Chave.Split(':').Last();
        if (Palavras.TryGetValue(tipo, out var kws))
            foreach (var kw in kws)
            {
                var c = cand.FirstOrDefault(t => (Norm(t.Descricao) + " " + Norm(t.DescricaoAdicional)).Contains(kw));
                if (c != null) return c.Id;
            }
        return -1; // não existe: será criado a partir do Hourglass
    }

    internal static string Norm(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var d = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in d) if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : ' ');
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }

    // Tipos do Hourglass cujas partes têm sala adicional.
    private static readonly HashSet<string> ComSala = ["reading", "initcall", "return", "study", "stutalk"];

    private static readonly Dictionary<string, string> Rotulos = new()
    {
        ["mm:chairman"] = "Presidente", ["mm:openprayer"] = "Oração inicial", ["mm:closeprayer"] = "Oração final", ["mm:cbsreader"] = "Leitor do estudo bíblico",
        ["wm:chairman"] = "Presidente (fim de semana)", ["wm:conductor"] = "Dirigente de A Sentinela", ["wm:reader"] = "Leitor de A Sentinela",
        ["treasures"] = "Tesouros da Palavra de Deus", ["dfg"] = "Pérolas espirituais", ["reading"] = "Leitura da Bíblia", ["initcall"] = "Iniciar conversas",
        ["return"] = "Cultivar o interesse", ["study"] = "Fazer discípulos", ["stutalk"] = "Discurso", ["localneeds"] = "Necessidades locais",
        ["cbs"] = "Estudo bíblico de congregação"
    };

    // Nome estável do tipo: o título de cada semana muda, o tipo não. Tipos desconhecidos usam o título.
    private static string Rotulo(string chave, string titulo) =>
        Rotulos.TryGetValue(chave, out var r) ? r : Rotulos.TryGetValue(chave.Split(':').Last(), out r) ? r : titulo;

    private static string LimparNumero(string? t) => Regex.Replace(t ?? "", @"^\s*\d+\.\s*", "").Trim();
    private static int Minutos(string? t) => int.TryParse(Regex.Match(t ?? "", @"\d+").Value, out var m) ? m : 0;

    // Os dados vêm de fora e o DbContext monta SQL por texto: escapar antes de gravar.
    private static string S(string? s) => Cortar(s).Replace("\\", "\\\\").Replace("'", "''");
    private static string Cortar(string? s) => (s ?? "").Length > 50 ? (s ?? "")[..50] : s ?? "";
}
