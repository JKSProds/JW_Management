using System.Security.Cryptography;
using JW_Management.Component.DB;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppCtx = JW_Management.Models.AppContext;

namespace JW_Management.Controllers;

// Gestão de congregações: só Admins do tenant principal (ver TenantContext.IdPrincipal).
[Authorize(Roles = "Admin")]
public class TenantsController(TenantContext _tenants, IConfiguration _cfg) : Controller
{
    private bool Permitido => _tenants.IdPrincipal(_cfg) == User.ObterTenant();

    [HttpGet]
    public IActionResult Index() => !Permitido ? Forbid() : View(_tenants._tenants.OrderBy(t => t.Id).ToList());

    [HttpGet]
    public IActionResult Editar(int id = 0)
    {
        if (!Permitido) return Forbid();
        if (id == 0) return View(new TenantForm());
        var t = _tenants._tenants.FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound();
        return View(new TenantForm { Id = t.Id, Nome = t.Nome, NomeCongregacao = t.NomeCongregacao, Telefone = t.TelefoneCongregacao, Morada = t.MoradaCongregacao, JWApiId = t.JWApiIdCongregacao, Email = t.Email, EmailSMTPPort = t.EmailSMTPPort, EmailHost = t.EmailHost });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Editar(TenantForm f)
    {
        if (!Permitido) return Forbid();
        var novo = f.Id == 0;
        if (string.IsNullOrWhiteSpace(f.Nome)) ModelState.AddModelError(nameof(f.Nome), "Indique o nome.");
        if (string.IsNullOrWhiteSpace(f.NomeCongregacao)) ModelState.AddModelError(nameof(f.NomeCongregacao), "Indique o nome da congregação.");
        if (novo)
        {
            if (string.IsNullOrWhiteSpace(f.AdminNome)) ModelState.AddModelError(nameof(f.AdminNome), "Indique o nome do primeiro administrador.");
            if (string.IsNullOrWhiteSpace(f.AdminEmail) || !f.AdminEmail.Contains('@')) ModelState.AddModelError(nameof(f.AdminEmail), "Indique um email válido.");
            // sem o envio de email configurado o administrador inicial não recebia a palavra-passe
            if (string.IsNullOrWhiteSpace(f.Email) || string.IsNullOrEmpty(f.EmailPassword) || string.IsNullOrWhiteSpace(f.EmailHost) || string.IsNullOrWhiteSpace(f.EmailSMTPPort))
                ModelState.AddModelError(nameof(f.Email), "Para criar o tenant preencha o email, a palavra-passe, o servidor e a porta de envio.");
        }
        if (!ModelState.IsValid) return View(f);

        if (!novo)
        {
            if (!_tenants._tenants.Any(t => t.Id == f.Id)) return NotFound();
            _tenants.Atualizar(f); _tenants.Recarregar();
            TempData["Msg"] = "Congregação guardada.";
            return RedirectToAction(nameof(Index));
        }

        var id = _tenants.Criar(f, _tenants.IdPrincipal(_cfg));
        _tenants.Recarregar();
        new DbService(new AppCtx(_tenants)); // cria as tabelas do tenant novo (e confirma as dos outros)

        var enviado = false;
        try { enviado = NovaSenhaAdmin(id, f.AdminNome!, f.AdminEmail!, criar: true); }
        catch (Exception e) { Console.WriteLine($"[Tenants] Falha ao criar o administrador inicial de {id}: {e.Message}"); }
        TempData["Msg"] = enviado
            ? "Congregação criada. O administrador inicial recebeu a palavra-passe por email."
            : "Congregação criada, mas o administrador inicial não foi criado ou o email não foi enviado. Confirme as definições de email e use «Reenviar acesso».";
        return RedirectToAction(nameof(Index));
    }

    // Gera nova palavra-passe para o primeiro administrador do tenant e envia-a por email (nunca a mostra).
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ReenviarAdmin(int id)
    {
        if (!Permitido) return Forbid();
        if (!_tenants._tenants.Any(t => t.Id == id)) return NotFound();
        var c = new AppCtx(_tenants, id);
        var a = c._dbContext.ObterPublicadores(false).Where(p => p.Id > 0 && p.TipoUtilizador == 0 && !string.IsNullOrWhiteSpace(p.Email)).OrderBy(p => p.Id).FirstOrDefault();
        TempData["Msg"] = a != null && NovaSenhaAdmin(id, a.Nome!, a.Email!, criar: false, a.Id) ? "Acesso reenviado ao administrador." : "Não foi possível reenviar o acesso (sem administrador com email, ou falha no envio).";
        return RedirectToAction(nameof(Index));
    }

    private bool NovaSenhaAdmin(int tenantId, string nome, string email, bool criar, int idExistente = 0)
    {
        var c = new AppCtx(_tenants, tenantId);
        var senha = RandomNumberGenerator.GetString("abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!#$%&", 12);
        var username = nome.Replace(" ", "").ToLower();
        var p = new Publicador
        {
            Id = criar ? c._dbContext.ObterPublicadores(false).Select(x => x.Id).DefaultIfEmpty(0).Max() + 1 : idExistente,
            Nome = nome, Username = username, Email = email, Grupo = new Grupo { Id = 0, Nome = "N/D" },
            Password = new PasswordHasher<string>().HashPassword(username, senha)
        };
        if (!c._dbContext.AdicionarPublicador(p)) return false;
        if (criar && !c._dbContext.AtualizarTipoUtilizador(p.Id, 0)) return false;
        p.Password = senha; // em claro só para o email
        return c._mailContext.MailSenhaPublicador(p);
    }
}
