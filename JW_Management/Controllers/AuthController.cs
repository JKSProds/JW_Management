using JW_Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace JW_Management.Controllers;

public class AuthController(TenantContext _tenantContext, DbContext _dbContext, IConfiguration _config) : Controller
{
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl)
    {
        returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"; // só caminhos locais: evita redirecionamento aberto

        // Sem OIDC configurado, mantém-se o login antigo.
        if (string.IsNullOrEmpty(_config["Oidc:ClientId"]))
            return RedirectToAction("Login", "Home", new { ReturnUrl = returnUrl });

        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OidcLogin.PendingScheme);
        return Redirect("/");
    }

    [HttpGet("/acesso-negado")]
    public IActionResult AcessoNegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [HttpGet("/oidc/escolher")]
    public async Task<IActionResult> Escolher()
    {
        var pending = await HttpContext.AuthenticateAsync(OidcLogin.PendingScheme);
        if (!pending.Succeeded) return Redirect("/acesso-negado");

        var ids = pending.Principal.FindAll(OidcLogin.MatchClaim).Select(c => c.Value).ToList();
        return View(_tenantContext._tenants.Where(t => ids.Contains(t.Id.ToString())).ToList());
    }

    [HttpPost("/oidc/escolher"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Escolher(int tenantId)
    {
        var pending = await HttpContext.AuthenticateAsync(OidcLogin.PendingScheme);
        if (!pending.Succeeded || !pending.Principal.HasClaim(OidcLogin.MatchClaim, tenantId.ToString()))
            return Redirect("/acesso-negado");

        // Repete a pesquisa só nesse tenant: o email vem do utilizador autenticado, não do pedido.
        var tenant = _tenantContext._tenants.First(t => t.Id == tenantId);
        var email = pending.Principal.FindFirst("email")?.Value;
        var found = OidcLogin.Match([tenant], t => _dbContext.ObterPublicadores(t.Id, false), email);
        if (found.Count != 1) return Redirect("/acesso-negado");

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            OidcLogin.ToPrincipal(found[0].Tenant, found[0].User));
        await HttpContext.SignOutAsync(OidcLogin.PendingScheme);

        var back = pending.Properties?.RedirectUri;
        return LocalRedirect(Url.IsLocalUrl(back) ? back : "/");
    }
}
