using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

// Programa mensal: lê do Hourglass (só GET) e guarda o resultado na base de dados (r_programa);
// depois os PDF e o ecrã do programa saem dessa cópia, sem voltar a pedir a cookie.
public class ProgramaServico(HourglassContext _hg, DbContext _db)
{
    private static readonly CultureInfo Pt = new("pt-PT");
    public static string NomeMes(int mes) => Pt.TextInfo.ToTitleCase(Pt.DateTimeFormat.GetMonthName(mes));

    // Segundas-feiras das semanas do mês (as semanas pertencem ao mês da sua segunda-feira).
    public static List<DateTime> Segundas(int ano, int mes)
    {
        var primeira = new DateTime(ano, mes, 1);
        var inicio = primeira.AddDays(-(((int)primeira.DayOfWeek + 6) % 7));
        var l = Enumerable.Range(0, 6).Select(i => inicio.AddDays(7 * i)).Where(d => d.Month == mes).ToList();
        return l.Count > 0 ? l : new() { inicio };
    }

    // ---------- Ler da base de dados ----------
    public ProgramaEstado Estado(int ano, int mes)
    {
        var seg = Segundas(ano, mes);
        var semanas = _db.ObterProgramaDocs("semana", seg.Select(d => d.ToString("yyyy-MM-dd")));
        var campo = _db.ObterProgramaDocs("campo", new[] { $"{ano}-{mes:00}" });
        var test = _db.ObterProgramaDocs("testemunho", new[] { $"{ano}-{mes:00}" });
        var datas = semanas.Values.Select(v => v.Atualizado).Concat(campo.Values.Select(v => v.Atualizado)).Concat(test.Values.Select(v => v.Atualizado)).ToList();
        return new ProgramaEstado { Semanas = semanas.Count, SemanasEsperadas = seg.Count, Campo = campo.Count > 0, Testemunho = test.Count > 0, Atualizado = datas.Count > 0 ? datas.Max() : null };
    }

    public ProgramaMensal? ObterPrograma(int ano, int mes, string congregacao, string dirigenteSentinela = "")
    {
        var seg = Segundas(ano, mes);
        var docs = _db.ObterProgramaDocs("semana", seg.Select(d => d.ToString("yyyy-MM-dd")));
        if (docs.Count == 0) return null;
        var rot = _db.ObterProgramaDocs("config", new[] { "rotulos" });
        var rotulos = rot.TryGetValue("rotulos", out var r) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(r.Dados) ?? new() : new();

        var p = new ProgramaMensal { Congregacao = congregacao, Mes = $"{NomeMes(mes)} {ano}", Titulo = $"{congregacao} - Programa de Atividades - {NomeMes(mes)}", Secoes = ProgramaMensal.CriarSecoes(rotulos) };
        foreach (var d in seg)
        {
            if (!docs.TryGetValue(d.ToString("yyyy-MM-dd"), out var doc)) continue;
            var s = JsonConvert.DeserializeObject<ProgramaSemana>(doc.Dados);
            if (s == null) continue;
            if (!string.IsNullOrWhiteSpace(dirigenteSentinela) && string.IsNullOrWhiteSpace(s.Valores.GetValueOrDefault("std", ""))) s.Valores["std"] = dirigenteSentinela;
            p.Semanas.Add(s);
        }
        return p;
    }

    public List<CampoEntrada>? ObterCampo(int ano, int mes) =>
        _db.ObterProgramaDocs("campo", new[] { $"{ano}-{mes:00}" }).TryGetValue($"{ano}-{mes:00}", out var d) ? JsonConvert.DeserializeObject<List<CampoEntrada>>(d.Dados) : null;

    public List<TestemunhoEntrada>? ObterTestemunho(int ano, int mes) =>
        _db.ObterProgramaDocs("testemunho", new[] { $"{ano}-{mes:00}" }).TryGetValue($"{ano}-{mes:00}", out var d) ? JsonConvert.DeserializeObject<List<TestemunhoEntrada>>(d.Dados) : null;

    // ---------- Ler do Hourglass e guardar ----------
    public async Task<ProgramaEstado> Sincronizar(string cookie, int ano, int mes)
    {
        var seg = Segundas(ano, mes);
        var intervalo = $"{seg[0]:yyyy-MM-dd}_{seg[^1].AddDays(6):yyyy-MM-dd}";
        var mesIni = new DateTime(ano, mes, 1); var mesFim = mesIni.AddMonths(1).AddDays(-1);

        var (users, lgroup) = await _hg.Utilizadores(cookie);
        var tMeeting = _hg.Get(cookie, $"/scheduling/mm/meeting/{intervalo}?lgroup={lgroup}&no_subs=true");
        var tMm = _hg.Get(cookie, $"/scheduling/mm/schedule/{intervalo}?lgroup={lgroup}");
        var tWm = _hg.Get(cookie, $"/scheduling/wm/schedule/{intervalo}?lgroup={lgroup}");
        var tAv = _hg.Get(cookie, $"/scheduling/av_attendant/{intervalo}?lgroup={lgroup}");
        var tSet = _hg.Get(cookie, $"/scheduling/congregation/settings?lgroup={lgroup}");
        var tLimp = _hg.Get(cookie, $"/scheduling/other/clean?fromDate={seg[0]:yyyy-MM-dd}&toDate={seg[^1].AddDays(6):yyyy-MM-dd}");
        var tGrupos = _hg.Get(cookie, "/fsreport/fsgroups");
        var tFm = _hg.Get(cookie, $"/scheduling/other/fm?fromDate={mesIni:yyyy-MM-dd}&toDate={mesFim:yyyy-MM-dd}");
        var tFmLoc = _hg.Get(cookie, $"/scheduling/fsmeetings/locations?lgroup={lgroup}");
        var tPw = _hg.Get(cookie, $"/scheduling/pw/schedule/{mesIni:yyyy-MM-dd}_{mesFim:yyyy-MM-dd}?lgroup={lgroup}");
        // «disabled=true» inclui os locais desativados, que ainda podem aparecer em turnos antigos
        var tPwLoc = _hg.Get(cookie, "/scheduling/pw/locations?disabled=true");
        await Task.WhenAll(tMeeting, tMm, tWm, tAv, tSet, tLimp, tGrupos, tFm, tFmLoc, tPw, tPwLoc);

        JObject Mapa(JToken t) => new(((t as JArray) ?? new()).Select(x => new JProperty((string)x["date"]!, x)));
        var partes = Mapa(tMeeting.Result); var mm = Mapa(tMm.Result); var wm = Mapa(tWm.Result);
        var av = (tAv.Result as JArray ?? new()).GroupBy(a => (string)a["date"]!).ToDictionary(g => g.Key, g => g.ToList());
        var cfg = tSet.Result as JObject ?? new();
        string Cfg(string chave, string padrao) { var v = (string?)cfg[chave]; return string.IsNullOrWhiteSpace(v) ? padrao : v; }
        string Nome(JToken? id) => id != null && id.Type == JTokenType.Integer && users.TryGetValue((int)id, out var u) ? u.Preferido : "";
        string Par(params string[] n) => string.Join(" - ", n.Where(x => x != ""));

        // Rótulos configuráveis do quadro (indicadores, sistema sonoro, limpeza)
        var rotulos = new Dictionary<string, string>
        {
            ["ind1"] = Cfg("attendant_label_1", "Receção"), ["ind2"] = Cfg("attendant_label_2", "Auditório"), ["ind3"] = Cfg("security_attendant_label_1", "Estacionamento"),
            ["som"] = Cfg("console_label_1", "Som"), ["tri"] = Cfg("stage_label_1", "Tribuna"), ["limp"] = Cfg("cleaning_label_1", "Limpeza")
        };
        _db.GuardarProgramaDoc("config", "rotulos", JsonConvert.SerializeObject(rotulos));

        // Limpeza: atribuída ao responsável de um grupo numa data; mostra-se o grupo dele («Grupo 1»).
        var grupoDe = (tGrupos.Result as JArray ?? new()).ToDictionary(g => (int)g["id"]!, g => System.Text.RegularExpressions.Regex.Match((string?)g["name"] ?? "", @"^\s*Grupo\s*\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase) is { Success: true } m ? m.Value.Trim() : ((string?)g["name"] ?? ""));
        var limpeza = new Dictionary<string, string>();
        foreach (var l in tLimp.Result as JArray ?? new())
        {
            if (!DateTime.TryParse((string?)l["date"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dl)) continue;
            var k = dl.AddDays(-(((int)dl.DayOfWeek + 6) % 7)).ToString("yyyy-MM-dd");
            var uid = (int?)l["assigneeUserId"] ?? 0;
            var texto = users.TryGetValue(uid, out var u) && grupoDe.TryGetValue(u.Grupo, out var gn) ? gn : (string?)l["assigneeText"] ?? "";
            if (texto != "") limpeza[k] = texto;
        }

        var canticos = new List<(int, string)>();
        foreach (var d in seg)
        {
            var k = d.ToString("yyyy-MM-dd"); var fim = d.AddDays(6); var v = new Dictionary<string, string>();
            var sem = new ProgramaSemana { Inicio = d, Valores = v, Titulo = d.Month == fim.Month ? $"Semana de {d.Day} a {fim.Day} de {NomeMes(d.Month)}" : $"Semana de {d.Day} de {NomeMes(d.Month)} a {fim.Day} de {NomeMes(fim.Month)}" };

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
                if (m != null)
                {
                    var nums = new List<string>();
                    foreach (var sk in new[] { "song1", "song2", "song3" })
                        if ((int?)HourglassContext.Sub(m[sk], "number") is > 0 and var n) { nums.Add(n.ToString()); canticos.Add((n, (string?)HourglassContext.Sub(m[sk], "title") ?? "")); }
                    if (nums.Count > 0) v["cmm"] = string.Join(" - ", nums);
                }
            }

            if (wm.TryGetValue(k, out var wTok) && wTok is JObject w)
            {
                v["wpr"] = Nome(w["wm_chairman"]); v["std"] = Nome(w["wt_conductor"]); v["stl"] = Nome(w["wm_reader"]); v["sto"] = Nome(w["closeprayer"]);
                if (w["speaker"] is JObject sp)
                {
                    v["wor"] = $"{(string?)sp["firstname"]} {(string?)sp["lastname"]}".Trim();
                    v["wco"] = (string?)HourglassContext.Sub(sp["congregation"], "name") ?? (string?)HourglassContext.Sub(w["temp_cong"], "name") ?? "";
                }
                v["wte"] = (string?)HourglassContext.Sub(w["public_talk"], "title") ?? "";
                sem.Evento = Evento(w["event"] as JObject) ?? sem.Evento;
                var wt = w["wt"] as JObject; var nw = new List<string>();
                foreach (var sk in new[] { "song1", "song2" })
                    if ((int?)HourglassContext.Sub(wt?[sk], "number") is > 0 and var n) { nw.Add(n.ToString()); canticos.Add((n, (string?)HourglassContext.Sub(wt?[sk], "title") ?? "")); }
                if (nw.Count > 0) v["cwm"] = string.Join(" - ", nw);
            }
            if (sem.Evento.Count == 0 && mm.TryGetValue(k, out var e2) && e2["event"] is JObject ev) sem.Evento = Evento(ev) ?? sem.Evento;

            if (av.TryGetValue(k, out var lista))
            {
                string Av(string tipo, int slot) => Nome(lista.FirstOrDefault(x => (string?)x["type"] == tipo && (int?)x["slot"] == slot)?["assignee"]);
                v["ind1"] = Av("attendant", 0); v["ind2"] = Av("attendant", 1); v["ind3"] = Av("security_attendant", 0);
                v["som"] = Av("console", 0); v["mic"] = Par(Av("mics", 0), Av("mics", 1)); v["tri"] = Av("stage", 0);
            }
            if (limpeza.TryGetValue(k, out var gl)) v["limp"] = gl;
            // a dirigente de A Sentinela vazio fica vazio: o valor por omissão aplica-se ao gerar, não ao guardar
            _db.GuardarProgramaDoc("semana", k, JsonConvert.SerializeObject(sem));
        }
        _db.GuardarCanticos(canticos);

        // Serviço de campo
        var locFm = (tFmLoc.Result as JArray ?? new()).ToDictionary(l => (int)l["id"]!, l => (string?)l["name"] ?? "");
        var campo = (tFm.Result as JArray ?? new()).Where(x => !string.IsNullOrEmpty((string?)x["date"]))
            .Select(x => new CampoEntrada { Data = (string)x["date"]!, Hora = (string?)x["time"] ?? "", Local = locFm.GetValueOrDefault((int?)x["fsMeetingLocationId"] ?? 0, ""), Dirigente = Nome(x["assigneeUserId"]) is { Length: > 0 } n ? n : (string?)x["assigneeText"] ?? "" })
            .OrderBy(c => c.Data).ThenBy(c => c.Hora).ThenBy(c => c.Local).ToList();
        _db.GuardarProgramaDoc("campo", $"{ano}-{mes:00}", JsonConvert.SerializeObject(campo));

        // Testemunho público: os instantes vêm em UTC; mostram-se na hora de Lisboa.
        var locPw = (tPwLoc.Result as JArray ?? new()).ToDictionary(l => (int)l["id"]!, l => (string?)l["name"] ?? "");
        TimeZoneInfo tz; try { tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon"); } catch { tz = TimeZoneInfo.Utc; }
        var turnos = (tPw.Result as JArray ?? new()).Select(x =>
        {
            var utc = DateTime.SpecifyKind(DateTime.Parse((string)x["datetime"]!, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal), DateTimeKind.Utc);
            var ini = TimeZoneInfo.ConvertTimeFromUtc(utc, tz); var horas = double.TryParse((string?)x["duration"], NumberStyles.Any, CultureInfo.InvariantCulture, out var h) ? h : 1;
            return (Ini: ini, Fim: ini.AddHours(horas), Loc: locPw.GetValueOrDefault((int?)x["location_id"] ?? 0, ""), Pessoa: Nome(x["assignee_id"]));
        }).GroupBy(t => (t.Ini, t.Fim, t.Loc)).Select(g => new TestemunhoEntrada
        {
            Data = g.Key.Ini.ToString("yyyy-MM-dd"), Inicio = g.Key.Ini.ToString("HH:mm"), Fim = g.Key.Fim.ToString("HH:mm"), Local = g.Key.Loc, Pessoas = g.Select(t => t.Pessoa).Where(n => n != "").ToList()
        }).OrderBy(t => t.Data).ThenBy(t => t.Inicio).ThenBy(t => t.Local).ToList();
        _db.GuardarProgramaDoc("testemunho", $"{ano}-{mes:00}", JsonConvert.SerializeObject(turnos));

        return Estado(ano, mes);
    }

    // Eventos que substituem as reuniões aparecem destacados; a visita do superintendente de circuito não.
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
