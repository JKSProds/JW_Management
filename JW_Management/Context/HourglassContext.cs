using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

// Cliente do Hourglass só para LEITURA: expõe apenas GET e usa a sessão que o utilizador colou.
// A cookie fica só em memória (nunca na base de dados nem nos registos), por congregação, durante algumas horas. Os endereços são os que a própria aplicação web do Hourglass usa.
public record HgUtilizador(int Id, string Nome, string Primeiro, string Ultimo, int Grupo = 0, string Preferido = "");

public class HourglassContext
{
    private const string Base = "https://app.hourglass-app.com/api/v0.2";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    // O JSON «null» chega como JValue (não como null do C#): aceder a um sub-campo dele lançaria exceção.
    public static JToken? Sub(JToken? t, string campo) => t is JObject o ? o[campo] : null;

    private static readonly TimeSpan Duracao = TimeSpan.FromHours(6);
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<int, (string Cookie, DateTime Expira)> Sessoes = new();

    // Cookie colada válida: guarda-a e usa-a. Sem cookie colada: usa a guardada, se ainda não expirou. null = não há sessão.
    public string? Resolver(int tenant, string? colada)
    {
        // O pacote (exportação) nunca fica guardado: usa-se só nesse pedido, e tem de ser colado de cada vez.
        if (CookieValida(colada)) { if (!EhPacote(colada)) Sessoes[tenant] = (colada!, DateTime.UtcNow + Duracao); return colada; }
        return Sessoes.TryGetValue(tenant, out var s) && s.Expira > DateTime.UtcNow ? s.Cookie : null;
    }
    public DateTime? SessaoAte(int tenant) => Sessoes.TryGetValue(tenant, out var s) && s.Expira > DateTime.UtcNow ? s.Expira.ToLocalTime() : null;
    public void Terminar(int tenant) => Sessoes.TryRemove(tenant, out _);

    public static bool CookieValida(string? c) =>
        !string.IsNullOrWhiteSpace(c) && (EhPacote(c) || (c.Contains("hglogin=") && Regex.IsMatch(c, @"X-Hourglass-XSRF-Token=[^;\s]+")));

    // «Pacote»: JSON com os dados já lidos dentro da página do Hourglass (atalho do iPad), sem cookie nenhuma. Fica em memória como a sessão.
    public static bool EhPacote(string? c) => c != null && c.TrimStart().StartsWith('{') && c.Contains("\"hgdata\"");

    // Responde aos mesmos pedidos que a rede, mas a partir do pacote colado. Pedidos com intervalo: o pacote traz uma janela larga e devolve-se só o intervalo pedido.
    // shortcut: o pacote só tem o que a importação de reuniões e o programa mensal pedem; o resto falha com 502 (alargar o atalho e esta tabela quando for preciso).
    private static readonly (string Prefixo, string Chave, string Campo)[] ComIntervalo =
    [
        ("/scheduling/mm/meeting/", "mmMeeting", "date"), ("/scheduling/mm/schedule/", "mm", "date"), ("/scheduling/wm/schedule/", "wm", "date"),
        ("/scheduling/av_attendant/", "av", "date"), ("/scheduling/other/clean", "clean", "date"), ("/scheduling/other/fm", "fm", "date"),
        ("/scheduling/pw/schedule/", "pw", "datetime")
    ];
    private static readonly (string Prefixo, string Chave)[] SemIntervalo =
        [("/scheduling/congregation/settings", "settings"), ("/fsreport/fsgroups", "fsgroups"), ("/scheduling/fsmeetings/locations", "fmLoc"), ("/scheduling/pw/locations", "pwLoc")];

    private static JToken Pacote(string pacote, string caminho)
    {
        var b = JObject.Parse(pacote);
        JToken Tem(string chave) => b[chave] ?? throw new HttpRequestException($"A exportação não tem «{chave}»: gere-a de novo com o atalho atual.");
        if (caminho == "/fsreport/users") return new JObject { ["users"] = Tem("users") };
        foreach (var (prefixo, chave) in SemIntervalo) if (caminho.StartsWith(prefixo)) return Tem(chave);
        foreach (var (prefixo, chave, campo) in ComIntervalo)
        {
            if (!caminho.StartsWith(prefixo)) continue;
            var m = Regex.Match(caminho, @"(\d{4}-\d\d-\d\d)(?:_|&toDate=)(\d{4}-\d\d-\d\d)");
            if (!m.Success) break;
            // datas ISO comparam-se como texto; «datetime» (UTC) usa só a parte da data. O JSON já lê «datetime» como data, tal como na rede.
            string D(JToken x) => x[campo] is JValue { Value: DateTime dt } ? dt.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : ((string?)x[campo] ?? "").PadRight(10)[..10];
            return new JArray(((Tem(chave) as JArray) ?? new()).Where(x => string.CompareOrdinal(D(x), m.Groups[1].Value) >= 0 && string.CompareOrdinal(D(x), m.Groups[2].Value) <= 0));
        }
        throw new HttpRequestException("A exportação não tem estes dados: " + caminho);
    }

    public async Task<JToken> Get(string cookie, string caminho)
    {
        if (EhPacote(cookie)) return Pacote(cookie, caminho);
        using var req = new HttpRequestMessage(HttpMethod.Get, Base + caminho);
        req.Headers.TryAddWithoutValidation("Cookie", cookie);
        req.Headers.TryAddWithoutValidation("X-Hourglass-XSRF-Token", Regex.Match(cookie, @"X-Hourglass-XSRF-Token=([^;\s]+)").Groups[1].Value);
        req.Headers.TryAddWithoutValidation("Accept", "application/json");

        using var resp = await Http.SendAsync(req);
        if (resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden) throw new UnauthorizedAccessException("Sessão do Hourglass inválida ou expirada.");
        resp.EnsureSuccessStatusCode();
        var texto = await resp.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(texto) ? new JArray() : JToken.Parse(texto);
    }

    // Lista de utilizadores (id e nome) e id da congregação. Só se aproveitam id e nome;
    // os restantes dados pessoais devolvidos não são lidos nem guardados.
    public async Task<(Dictionary<int, HgUtilizador> Users, int Lgroup)> Utilizadores(string cookie)
    {
        var d = await Get(cookie, "/fsreport/users");
        var lista = (d["users"] as JArray ?? new()).ToList();
        var lgroup = lista.GroupBy(u => (int?)u["congregations_id"] ?? 0).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
        var users = lista.ToDictionary(u => (int)u["id"]!, u => new HgUtilizador((int)u["id"]!,
            string.Join(" ", new[] { (string?)u["firstname"], (string?)u["middlename"], (string?)u["lastname"] }.Where(x => !string.IsNullOrWhiteSpace(x))),
            (string?)u["firstname"] ?? "", (string?)u["lastname"] ?? "", (int?)u["group_id"] ?? 0,
            // «descriptor» é o nome preferido no Hourglass; sem ele usa-se primeiro e último nome (sem nomes do meio).
            string.IsNullOrWhiteSpace((string?)u["descriptor"]) ? $"{(string?)u["firstname"]} {(string?)u["lastname"]}".Trim() : ((string)u["descriptor"]!).Trim()));
        return (users, lgroup);
    }
}
