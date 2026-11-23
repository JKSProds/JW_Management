using System.Net;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JW_Management.Controllers;

[Authorize(Roles = "Admin")]
[Route("configuracao")]
public class ConfiguracaoController(ConfigStore _store, TenantContext _tenants) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(Load());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Index(ConfiguracaoModel m)
    {
        var atual = Load();
        var secretDefinido = atual.ClientSecretDefinido || !string.IsNullOrEmpty(m.ClientSecret);

        if (!string.IsNullOrWhiteSpace(m.Authority) &&
            !(Uri.TryCreate(m.Authority, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps))
            ModelState.AddModelError(nameof(m.Authority), "O Authority tem de ser um URL https válido.");
        if (!string.IsNullOrWhiteSpace(m.KnownProxy) && !IPAddress.TryParse(m.KnownProxy, out _))
            ModelState.AddModelError(nameof(m.KnownProxy), "IP inválido.");
        if (m.SomenteOidc && (string.IsNullOrWhiteSpace(m.Authority) || string.IsNullOrWhiteSpace(m.ClientId) || !secretDefinido))
            ModelState.AddModelError(nameof(m.SomenteOidc), "Só OAuth exige Authority, ClientId e ClientSecret definidos.");

        // Email: ou está tudo vazio (envio desligado) ou tem de estar completo
        var tenant = _tenants._tenants.FirstOrDefault(t => t.Id == User.ObterTenant());
        var senhaDefinida = atual.EmailPasswordDefinida || !string.IsNullOrEmpty(m.EmailPassword);
        var algumEmail = !string.IsNullOrWhiteSpace(m.EmailRemetente) || !string.IsNullOrWhiteSpace(m.EmailHost) || !string.IsNullOrWhiteSpace(m.EmailPorta) || !string.IsNullOrEmpty(m.EmailPassword);
        if (tenant == null && algumEmail) ModelState.AddModelError(nameof(m.EmailRemetente), "Não foi possível identificar a congregação atual.");
        if (!string.IsNullOrWhiteSpace(m.EmailRemetente) && !System.Net.Mail.MailAddress.TryCreate(m.EmailRemetente.Trim(), out _))
            ModelState.AddModelError(nameof(m.EmailRemetente), "Email do remetente inválido.");
        if (!string.IsNullOrWhiteSpace(m.EmailPorta) && !(int.TryParse(m.EmailPorta.Trim(), out var porta) && porta is > 0 and < 65536))
            ModelState.AddModelError(nameof(m.EmailPorta), "A porta tem de ser um número entre 1 e 65535 (465 ou 587).");
        if (algumEmail && (string.IsNullOrWhiteSpace(m.EmailRemetente) || string.IsNullOrWhiteSpace(m.EmailHost) || string.IsNullOrWhiteSpace(m.EmailPorta) || !senhaDefinida))
            ModelState.AddModelError(nameof(m.EmailRemetente), "Para enviar emails indique o remetente, o servidor, a porta e a palavra-passe.");
        if (!string.IsNullOrWhiteSpace(m.MailBcc) && !System.Net.Mail.MailAddress.TryCreate(m.MailBcc.Trim(), out _))
            ModelState.AddModelError(nameof(m.MailBcc), "Email da cópia (Bcc) inválido.");

        if (!ModelState.IsValid)
        {
            m.ClientSecretDefinido = atual.ClientSecretDefinido;
            m.EmailPasswordDefinida = atual.EmailPasswordDefinida;
            return View(m);
        }

        _store.Set(ConfigStore.Authority, m.Authority?.Trim());
        _store.Set(ConfigStore.ClientId, m.ClientId?.Trim());
        if (!string.IsNullOrEmpty(m.ClientSecret)) _store.Set(ConfigStore.ClientSecret, m.ClientSecret);
        _store.Set(ConfigStore.KnownProxy, m.KnownProxy?.Trim());
        _store.Set(ConfigStore.SomenteOidc, m.SomenteOidc ? "true" : null);
        _store.Set(ConfigStore.MailNome, m.MailNome?.Trim());
        _store.Set(ConfigStore.MailBcc, m.MailBcc?.Trim());
        if (tenant != null) _tenants.AtualizarEmail(tenant.Id, m.EmailRemetente, m.EmailHost, m.EmailPorta, m.EmailPassword);

        TempData["Guardado"] = true;
        return RedirectToAction(nameof(Index));
    }

    private ConfiguracaoModel Load()
    {
        var v = _store.Load();
        var t = _tenants._tenants.FirstOrDefault(x => x.Id == User.ObterTenant());
        return new ConfiguracaoModel
        {
            EmailRemetente = t?.Email, EmailHost = t?.EmailHost, EmailPorta = t?.EmailSMTPPort,
            EmailPasswordDefinida = !string.IsNullOrEmpty(t?.EmailPassword),
            MailNome = v.GetValueOrDefault(ConfigStore.MailNome), MailBcc = v.GetValueOrDefault(ConfigStore.MailBcc),
            Authority = v.GetValueOrDefault(ConfigStore.Authority),
            ClientId = v.GetValueOrDefault(ConfigStore.ClientId),
            ClientSecretDefinido = !string.IsNullOrEmpty(v.GetValueOrDefault(ConfigStore.ClientSecret)),
            KnownProxy = v.GetValueOrDefault(ConfigStore.KnownProxy),
            SomenteOidc = v.GetValueOrDefault(ConfigStore.SomenteOidc) == "true"
        };
    }
}
