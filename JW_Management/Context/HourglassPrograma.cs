using System.Globalization;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

// Monta o programa mensal de atividades a partir do Hourglass (só leitura, como a importação).
public class HourglassPrograma(HourglassContext _hg)
{
    private static readonly CultureInfo Pt = new("pt-PT");

    public async Task<ProgramaMensal> Obter(string cookie, int ano, int mes, string congregacao, string dirigenteSentinela = "")
    {
        var primeira = new DateTime(ano, mes, 1);
        var inicio = primeira.AddDays(-(((int)primeira.DayOfWeek + 6) % 7));            // segunda-feira
        var segundas = Enumerable.Range(0, 6).Select(i => inicio.AddDays(7 * i)).Where(d => d.Month == mes).ToList();
        if (segundas.Count == 0) segundas.Add(inicio);
        var intervalo = $"{segundas[0]:yyyy-MM-dd}_{segundas[^1].AddDays(6):yyyy-MM-dd}";

        var (users, lgroup) = await _hg.Utilizadores(cookie);
        var tMeeting = _hg.Get(cookie, $"/scheduling/mm/meeting/{intervalo}?lgroup={lgroup}&no_subs=true");
        var tMm = _hg.Get(cookie, $"/scheduling/mm/schedule/{intervalo}?lgroup={lgroup}");
        var tWm = _hg.Get(cookie, $"/scheduling/wm/schedule/{intervalo}?lgroup={lgroup}");
        var tAv = _hg.Get(cookie, $"/scheduling/av_attendant/{intervalo}?lgroup={lgroup}");
        var tSet = _hg.Get(cookie, $"/scheduling/congregation/settings?lgroup={lgroup}");
        var tLimp = _hg.Get(cookie, $"/scheduling/other/clean?fromDate={segundas[0]:yyyy-MM-dd}&toDate={segundas[^1].AddDays(6):yyyy-MM-dd}");
        var tGrupos = _hg.Get(cookie, "/fsreport/fsgroups");
        await Task.WhenAll(tMeeting, tMm, tWm, tAv, tSet, tLimp, tGrupos);

        JObject Mapa(JToken t) => new(((t as JArray) ?? new()).Select(x => new JProperty((string)x["date"]!, x)));
        var partes = Mapa(tMeeting.Result); var mm = Mapa(tMm.Result); var wm = Mapa(tWm.Result);
        var av = (tAv.Result as JArray ?? new()).GroupBy(a => (string)a["date"]!).ToDictionary(g => g.Key, g => g.ToList());
        var cfg = tSet.Result as JObject ?? new();

        // Limpeza: a atribuição é a um utilizador (responsável do grupo) numa data; mostra-se o grupo dele («Grupo 1»).
        var grupoDe = (tGrupos.Result as JArray ?? new()).ToDictionary(g => (int)g["id"]!, g => System.Text.RegularExpressions.Regex.Match((string?)g["name"] ?? "", @"^\s*Grupo\s*\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase) is { Success: true } m ? m.Value.Trim() : ((string?)g["name"] ?? ""));
        var grupoUtilizador = users.ToDictionary(u => u.Key, u => u.Value.Grupo);
        var limpeza = new Dictionary<string, string>();
        foreach (var l in tLimp.Result as JArray ?? new())
        {
            if (!DateTime.TryParse((string?)l["date"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dl)) continue;
            var seg = dl.AddDays(-(((int)dl.DayOfWeek + 6) % 7)).ToString("yyyy-MM-dd");
            var uid = (int?)l["assigneeUserId"] ?? 0;
            var texto = grupoUtilizador.TryGetValue(uid, out var gid) && grupoDe.TryGetValue(gid, out var gn) ? gn : (string?)l["assigneeText"] ?? "";
            if (texto != "") limpeza[seg] = texto;
        }
        string Rot(string chave, string padrao) { var v = (string?)cfg[chave]; return string.IsNullOrWhiteSpace(v) ? padrao : v; }

        string Nome(JToken? id) => id != null && id.Type == JTokenType.Integer && users.TryGetValue((int)id, out var u) ? $"{u.Primeiro} {u.Ultimo}".Trim() : "";
        string Par(params string[] n) => string.Join(" - ", n.Where(x => x != ""));

        var secoes = new List<ProgramaSecao>
        {
            new() { Titulo = "REUNIÃO DE SEMANA", Faixa = true, Linhas = { new() { Id = "pres", Rotulo = "Presidente" }, new() { Id = "orac", Rotulo = "Oração" } } },
            new() { Titulo = "TESOUROS DA PALAVRA DE DEUS", Linhas = { new() { Id = "tes1", Rotulo = "Discurso" }, new() { Id = "tes2", Rotulo = "Em Busca de Pérolas Espirituais" }, new() { Id = "tes3", Rotulo = "Leitura da Bíblia" } } },
            new() { Titulo = "EMPENHE-SE NO MINISTÉRIO", Linhas = { new() { Id = "min1", Rotulo = "Parte 1" }, new() { Id = "min2", Rotulo = "Parte 2" }, new() { Id = "min3", Rotulo = "Parte 3" }, new() { Id = "min4", Rotulo = "Parte 4" } } },
            new() { Titulo = "VIVER COMO CRISTÃOS", Linhas = { new() { Id = "viv1", Rotulo = "Parte 1" }, new() { Id = "viv2", Rotulo = "Parte 2" } } },
            new() { Titulo = "ESTUDO BÍBLICO", Linhas = { new() { Id = "ebd", Rotulo = "Dirigente" }, new() { Id = "ebl", Rotulo = "Leitor" }, new() { Id = "ebo", Rotulo = "Oração final" } } },
            new() { Titulo = "REUNIÃO DE FIM DE SEMANA", Faixa = true, Linhas = { new() { Id = "wpr", Rotulo = "Presidente" }, new() { Id = "wor", Rotulo = "Orador" }, new() { Id = "wco", Rotulo = "Congregação" }, new() { Id = "wte", Rotulo = "Tema" } } },
            new() { Titulo = "ESTUDO DE A SENTINELA", Linhas = { new() { Id = "std", Rotulo = "Dirigente" }, new() { Id = "stl", Rotulo = "Leitor" }, new() { Id = "sto", Rotulo = "Oração final" } } },
            new() { Titulo = "INDICADORES", Linhas = { new() { Id = "ind1", Rotulo = Rot("attendant_label_1", "Receção") }, new() { Id = "ind2", Rotulo = Rot("attendant_label_2", "Auditório") }, new() { Id = "ind3", Rotulo = Rot("security_attendant_label_1", "Estacionamento") } } },
            new() { Titulo = "SISTEMA SONORO", Linhas = { new() { Id = "som", Rotulo = Rot("console_label_1", "Som") }, new() { Id = "mic", Rotulo = "Micros" }, new() { Id = "tri", Rotulo = Rot("stage_label_1", "Tribuna") } } },
            new() { Titulo = "", Linhas = { new() { Id = "limp", Rotulo = Rot("cleaning_label_1", "Limpeza") } } }
        };

        var programa = new ProgramaMensal { Titulo = $"{congregacao} - Programa de Atividades - {Pt.TextInfo.ToTitleCase(Pt.DateTimeFormat.GetMonthName(mes))}", Secoes = secoes };

        foreach (var d in segundas)
        {
            var k = d.ToString("yyyy-MM-dd");
            var v = new Dictionary<string, string>();
            var fim = d.AddDays(6);
            var mesIni = Pt.TextInfo.ToTitleCase(Pt.DateTimeFormat.GetMonthName(d.Month));
            var sem = new ProgramaSemana
            {
                Inicio = d, Valores = v,
                Titulo = d.Month == fim.Month ? $"Semana de {d.Day} a {fim.Day} de {mesIni}" : $"Semana de {d.Day} de {mesIni} a {fim.Day} de {Pt.TextInfo.ToTitleCase(Pt.DateTimeFormat.GetMonthName(fim.Month))}"
            };

            if (mm.TryGetValue(k, out var aTok) && aTok is JObject a)
            {
                partes.TryGetValue(k, out var mTok); var m = mTok as JObject;
                v["pres"] = Nome(a["chairman"]); v["orac"] = Nome(a["openprayer"]); v["ebl"] = Nome(a["cbs_reader"]); v["ebo"] = Nome(a["closeprayer"]);
                int min = 0, viv = 0;
                foreach (var sec in new[] { "tgw", "fm", "lac" })
                    foreach (var e in (a[sec] as JArray ?? new()).Where(e => ((int?)e["classroom"] ?? 0) == 0))
                    {
                        var tipo = (string?)((m?[sec] as JArray)?.FirstOrDefault(p => (int?)p["id"] == (int?)e["part"]))?["type"] ?? "";
                        var quem = Nome(e["assignee"]);
                        switch (sec, tipo)
                        {
                            case ("tgw", "treasures"): v["tes1"] = quem; break;
                            case ("tgw", "dfg"): v["tes2"] = quem; break;
                            case ("tgw", "reading"): v["tes3"] = quem; break;
                            case ("fm", _): if (++min <= 4) v["min" + min] = Par(quem, Nome(e["assistant"])); break;
                            case ("lac", "cbs"): v["ebd"] = quem; break;
                            case ("lac", _): if (++viv <= 2) v["viv" + viv] = quem; break;
                        }
                    }
            }

            if (wm.TryGetValue(k, out var wTok) && wTok is JObject w)
            {
                v["wpr"] = Nome(w["wm_chairman"]); v["std"] = Nome(w["wt_conductor"]) is { Length: > 0 } cond ? cond : dirigenteSentinela; v["stl"] = Nome(w["wm_reader"]); v["sto"] = Nome(w["closeprayer"]);
                if (w["speaker"] is JObject sp)
                {
                    v["wor"] = $"{(string?)sp["firstname"]} {(string?)sp["lastname"]}".Trim();
                    v["wco"] = (string?)HourglassContext.Sub(sp["congregation"], "name") ?? (string?)HourglassContext.Sub(w["temp_cong"], "name") ?? "";
                }
                v["wte"] = (string?)HourglassContext.Sub(w["public_talk"], "title") ?? "";
                sem.Evento = Evento(w["event"] as JObject) ?? sem.Evento;
            }
            if (sem.Evento.Count == 0 && mm.TryGetValue(k, out var e2) && e2["event"] is JObject ev) sem.Evento = Evento(ev) ?? sem.Evento;

            if (av.TryGetValue(k, out var lista))
            {
                string Av(string tipo, int slot) => Nome(lista.FirstOrDefault(x => (string?)x["type"] == tipo && (int?)x["slot"] == slot)?["assignee"]);
                v["ind1"] = Av("attendant", 0); v["ind2"] = Av("attendant", 1); v["ind3"] = Av("security_attendant", 0);
                v["som"] = Av("console", 0); v["mic"] = Par(Av("mics", 0), Av("mics", 1)); v["tri"] = Av("stage", 0);
            }
            if (limpeza.TryGetValue(k, out var gl)) v["limp"] = gl;
            programa.Semanas.Add(sem);
        }
        return programa;
    }

    // Eventos que substituem as reuniões aparecem a amarelo; a visita do superintendente de circuito não.
    private static List<string>? Evento(JObject? e)
    {
        if (e == null) return null;
        var titulo = (string?)e["custom_title"];
        if (!string.IsNullOrWhiteSpace(titulo)) return titulo.ToUpperInvariant().Split(' ', 3).ToList();
        return ((string?)e["event"]) switch
        {
            "ca" => new() { "ASSEMBLEIA", "DE", "CIRCUITO" },
            "rc" => new() { "CONGRESSO", "REGIONAL" },
            "mmem" => new() { "MEMORIAL" },
            _ => null
        };
    }
}
