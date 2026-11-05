using System.Net;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JW_Management.Controllers;

[Authorize(Roles = "Admin")]
[Route("configuracao")]
public class ConfiguracaoController(ConfigStore _store) : Controller
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

        if (!ModelState.IsValid)
        {
            m.ClientSecretDefinido = atual.ClientSecretDefinido;
            return View(m);
        }

        _store.Set(ConfigStore.Authority, m.Authority?.Trim());
        _store.Set(ConfigStore.ClientId, m.ClientId?.Trim());
        if (!string.IsNullOrEmpty(m.ClientSecret)) _store.Set(ConfigStore.ClientSecret, m.ClientSecret);
        _store.Set(ConfigStore.KnownProxy, m.KnownProxy?.Trim());
        _store.Set(ConfigStore.SomenteOidc, m.SomenteOidc ? "true" : null);

        TempData["Guardado"] = true;
        return RedirectToAction(nameof(Index));
    }

    private ConfiguracaoModel Load()
    {
        var v = _store.Load();
        return new ConfiguracaoModel
        {
            Authority = v.GetValueOrDefault(ConfigStore.Authority),
            ClientId = v.GetValueOrDefault(ConfigStore.ClientId),
            ClientSecretDefinido = !string.IsNullOrEmpty(v.GetValueOrDefault(ConfigStore.ClientSecret)),
            KnownProxy = v.GetValueOrDefault(ConfigStore.KnownProxy),
            SomenteOidc = v.GetValueOrDefault(ConfigStore.SomenteOidc) == "true"
        };
    }
}
