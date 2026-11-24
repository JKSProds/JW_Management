using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace JW_Management.Models;

public static class OidcLogin
{
    public const string PendingScheme = "OidcPendente";
    public const string MatchClaim = "oidc_match";

    // Procura o email (sem distinguir maiúsculas) em todos os tenants; email vazio nunca corresponde.
    public static List<(Tenant Tenant, Publicador User)> Match(IEnumerable<Tenant> tenants,
        Func<Tenant, IEnumerable<Publicador>> users, string? email)
    {
        email = email?.Trim();
        if (string.IsNullOrEmpty(email)) return [];

        return tenants
            .SelectMany(t => users(t)
                .Where(u => string.Equals(u.Email?.Trim(), email, StringComparison.OrdinalIgnoreCase))
                .Select(u => (t, u)))
            .ToList();
    }

    // Mesmos claims do login antigo (HomeController.Login), para o resto da app não notar a diferença.
    public static ClaimsPrincipal ToPrincipal(Tenant t, Publicador u, bool master = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, u.Id.ToString()),
            new(ClaimTypes.Role, u.Role),
            new(ClaimTypes.GivenName, u.Nome!),
            new(ClaimTypes.Sid, t.Id.ToString()),
            new(ClaimTypes.PrimaryGroupSid, t.NomeCongregacao.ToString())
        };
        // O master entra em qualquer congregação sem ter registo nela: guarda-se onde está o seu registo (congregação e id).
        if (master) claims.AddRange([new Claim("master", "1"), new Claim("master_home", t.Id.ToString()), new Claim("master_id", u.Id.ToString())]);
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static async Task OnTokenValidated(TokenValidatedContext ctx)
    {
        var sp = ctx.HttpContext.RequestServices;
        var db = sp.GetRequiredService<DbContext>();
        var email = ctx.Principal?.FindFirst("email")?.Value;

        var found = Match(sp.GetRequiredService<TenantContext>()._tenants, t => db.ObterPublicadores(t.Id, false), email);

        // Sem email, sem conta, ou várias contas com o mesmo email na mesma congregação: nunca se adivinha.
        if (found.Count == 0 || found.GroupBy(f => f.Tenant.Id).Any(g => g.Count() > 1))
        {
            ctx.Fail("Sem conta local única para este utilizador.");
            return;
        }

        if (found.Count == 1)
        {
            // O handler ainda compara o "sub" com o do userinfo, por isso mantém-se.
            var principal = ToPrincipal(found[0].Tenant, found[0].User, db.EhMaster(found[0].User));
            var sub = ctx.Principal!.FindFirst("sub");
            if (sub != null) ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("sub", sub.Value));
            ctx.Principal = principal;
            return;
        }

        // Várias congregações: cookie temporário só com os candidatos; o utilizador escolhe em /oidc/escolher.
        var pending = new ClaimsPrincipal(new ClaimsIdentity(
            found.Select(f => new Claim(MatchClaim, f.Tenant.Id.ToString())).Append(new Claim("email", email!)),
            PendingScheme));
        await ctx.HttpContext.SignInAsync(PendingScheme, pending,
            new AuthenticationProperties { RedirectUri = ctx.Properties?.RedirectUri });
        ctx.Response.Redirect("/oidc/escolher");
        ctx.HandleResponse();
    }
}
