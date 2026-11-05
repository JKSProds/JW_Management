using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

// Cliente do Hourglass só para LEITURA: expõe apenas GET, usa a sessão que o utilizador colou
// e não a guarda nem a regista. Os endereços são os que a própria aplicação web do Hourglass usa.
public record HgUtilizador(int Id, string Nome, string Primeiro, string Ultimo, int Grupo = 0);

public class HourglassContext
{
    private const string Base = "https://app.hourglass-app.com/api/v0.2";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    // O JSON «null» chega como JValue (não como null do C#): aceder a um sub-campo dele lançaria exceção.
    public static JToken? Sub(JToken? t, string campo) => t is JObject o ? o[campo] : null;

    public static bool CookieValida(string? c) =>
        !string.IsNullOrWhiteSpace(c) && c.Contains("hglogin=") && Regex.IsMatch(c, @"X-Hourglass-XSRF-Token=[^;\s]+");

    public async Task<JToken> Get(string cookie, string caminho)
    {
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
            (string?)u["firstname"] ?? "", (string?)u["lastname"] ?? "", (int?)u["group_id"] ?? 0));
        return (users, lgroup);
    }
}
