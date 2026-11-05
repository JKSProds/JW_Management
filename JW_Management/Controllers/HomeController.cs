using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace JW_Management.Controllers
{
    public class HomeController(JW_Management.Models.AppContext _appContext, DbContext _dbContext, ConfigStore _configStore, IConfiguration _config) : Controller
    {
        public IActionResult Index()
        {

            return View();
        }

        // Resumo para o início. Pedido depois de a página carregar, para não atrasar a abertura.
        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura")]
        [HttpGet]
        public IActionResult Resumo()
        {
            var stock = _dbContext.ObterLiteraturas("", 0).Where(l => !(l.Tipo!.Id == 7 && l.Quantidade <= 0)).ToList();
            var pendentes = _dbContext.ObterPedidosEspeciais().Count(l => l.Quantidade > 0 && l.EstadoPedido!.Descricao == "Pendente");

            return Json(new { titulos = stock.Count, unidades = stock.Sum(l => l.Quantidade), pedidosPendentes = pendentes });
        }

        // A lista de congregações é lida uma só vez no arranque. Se a base de dados master ainda não
        // estava disponível nessa altura fica vazia para sempre; por isso volta a tentar aqui.
        private List<Tenant> ObterTenants()
        {
            var ctx = _appContext._tenantContext;
            if (ctx._tenants.Count == 0) ctx._tenants = ctx.ObterListaTenants();
            return ctx._tenants;
        }

        private void PrepararLogin(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Tenants = ObterTenants().Select(t => new SelectListItem(t.Nome, t.Id.ToString())).ToList();
        }

        [HttpGet]
        public IActionResult Login(string ReturnUrl)
        {
            PrepararLogin(ReturnUrl);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Publicador p, int t, string ReturnUrl)
        {
            PrepararLogin(ReturnUrl);

            if (_configStore.SomenteOidcAtivo(_config))
            {
                ModelState.AddModelError("", "O login com palavra-passe está desativado. Use o Pocket ID.");
                return View();
            }

            _appContext._manualTenant = t;
            p.Username = (p.Username ?? "").ToLower().Trim();

            var passwordHasher = new PasswordHasher<string>();
            var u = _dbContext.ObterPublicadores(t, false).FirstOrDefault(u =>
                u.Username == p.Username && !string.IsNullOrEmpty(u.Password) &&
                passwordHasher.VerifyHashedPassword(null!, u.Password, p.Password ?? "") == PasswordVerificationResult.Success);

            if (u == null)
            {
                _appContext._manualTenant = 0;
                // Mensagem única: não revela se o utilizador existe.
                ModelState.AddModelError("", "Utilizador ou palavra-passe incorretos.");
                return View(new Publicador { Username = p.Username });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, u.Id.ToString()),
                new Claim(ClaimTypes.Role, u.Role),
                new Claim(ClaimTypes.GivenName, u.Nome!),
                new Claim(ClaimTypes.Sid, _appContext._currentTenant.Id.ToString()),
                new Claim(ClaimTypes.PrimaryGroupSid, _appContext._currentTenant.NomeCongregacao.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            Console.WriteLine($"[{_appContext._currentTenant.NomeCongregacao}] - Login - Utilizador: {u.Nome}");
            _appContext._manualTenant = 0;

            // Só aceita destinos locais, para o ReturnUrl não servir de redirecionamento aberto.
            return Url.IsLocalUrl(ReturnUrl) ? LocalRedirect(ReturnUrl) : RedirectToAction("Index", "Home");
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Tenants()
        {
            return Json(ObterTenants().Select(t => new Tenant() {Id = t.Id, Nome = t.Nome}));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
