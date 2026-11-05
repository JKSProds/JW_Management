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
            var especiais = _dbContext.ObterPedidosEspeciais().Where(l => l.Quantidade > 0).ToList();
            var periodicos = _dbContext.ObterPeriodicos().Where(l => l.Quantidade > 0).OrderBy(l => l.Entrega).ToList();

            return Json(new
            {
                titulos = stock.Count,
                unidades = stock.Sum(l => l.Quantidade),
                semStock = stock.Count(l => l.Quantidade <= 0),
                pedidosPendentes = especiais.Count(l => l.EstadoPedido!.Descricao == "Pendente"),
                pedidosEspeciais = especiais.Count,
                periodicosTitulos = periodicos.Count,
                periodicosExemplares = periodicos.Sum(l => l.QuantidadeFalta),
                urgentes = periodicos.Where(l => l.Entrega < 31)
                    .Select(l => new { stamp = l.Stamp, descricao = l.Descricao, geral = l.DescricaoGeral, dias = Math.Max(0, l.Entrega), falta = l.QuantidadeFalta })
            });
        }

        // A lista de congregações é lida uma só vez no arranque. Se a base de dados master ainda não
        // estava disponível nessa altura fica vazia para sempre; por isso volta a tentar aqui.
        // As designações do utilizador autenticado nas próximas semanas. A semana vem como «SEMANA n DE aaaa»
        // (mesma contagem de 7 em 7 dias a partir de 1 de janeiro com que a reunião é criada).
        [Authorize]
        [HttpGet]
        public IActionResult MinhasDesignacoes()
        {
            var hoje = DateTime.Today;
            var lista = _dbContext.ObterDesignacoesPublicador(User.ObterId())
                .Select(d =>
                {
                    var m = System.Text.RegularExpressions.Regex.Match(d.SemanaReuniao ?? "", @"(\d+)\D+(\d{4})");
                    if (!m.Success) return null;
                    var inicio = new DateTime(int.Parse(m.Groups[2].Value), 1, 1).AddDays((int.Parse(m.Groups[1].Value) - 1) * 7);
                    return new { stamp = d.StampReuniao, semana = d.SemanaReuniao, inicio = inicio.ToString("yyyy-MM-dd"), fim = inicio.AddDays(6).ToString("yyyy-MM-dd"),
                                 tipo = d.TipoDesignacao?.Descricao, tema = d.NomeDesignacao, minutos = d.NMin, local = d.Local, ordem = inicio, tid = d.TipoDesignacao?.Id ?? 0 };
                })
                .Where(x => x != null && x.ordem.AddDays(6) >= hoje).OrderBy(x => x!.ordem).ThenBy(x => x!.tid).Take(30);
            return Json(lista);
        }

        private List<Tenant> ObterTenants()
        {
            var ctx = _appContext._tenantContext;
            if (ctx._tenants.Count == 0) ctx._tenants = ctx.ObterListaTenants();
            return ctx._tenants;
        }

        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        // A congregação deduz-se das credenciais: procura-se o utilizador em todas. Se existir em mais
        // do que uma, o ecrã pede para escolher (sem nunca reenviar a palavra-passe).
        [HttpPost]
        public async Task<IActionResult> Login(Publicador p, int t, string? ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;

            if (_configStore.SomenteOidcAtivo(_config))
            {
                ModelState.AddModelError("", "O login com palavra-passe está desativado. Entre com a passkey.");
                return View();
            }

            var username = (p.Username ?? "").ToLower().Trim();
            var password = p.Password ?? "";
            var passwordHasher = new PasswordHasher<string>();

            var encontrados = new List<(Tenant Tenant, Publicador Utilizador)>();
            foreach (var tenant in ObterTenants().Where(x => t == 0 || x.Id == t))
            {
                try
                {
                    _appContext._manualTenant = tenant.Id;
                    var u = _dbContext.ObterPublicadores(tenant.Id, false).FirstOrDefault(u =>
                        u.Username == username && !string.IsNullOrEmpty(u.Password) &&
                        passwordHasher.VerifyHashedPassword(null!, u.Password, password) == PasswordVerificationResult.Success);
                    if (u != null) encontrados.Add((tenant, u));
                }
                catch (Exception e)
                {
                    // Uma congregação indisponível não pode impedir as restantes.
                    Console.WriteLine($"[Login] Falha ao consultar a congregação {tenant.Id}: {e.Message}");
                }
            }
            _appContext._manualTenant = 0;

            if (encontrados.Count == 0)
            {
                // Mensagem única: não revela se o utilizador existe.
                ModelState.AddModelError("", "Utilizador ou palavra-passe incorretos.");
                return View(new Publicador { Username = username });
            }

            if (encontrados.Count > 1)
            {
                ViewBag.Escolher = encontrados.Select(x => new SelectListItem(x.Tenant.Nome, x.Tenant.Id.ToString())).ToList();
                ModelState.AddModelError("", "Esta conta existe em mais do que uma congregação. Escolha uma e volte a introduzir a palavra-passe.");
                return View(new Publicador { Username = username });
            }

            var (escolhida, utilizador) = encontrados[0];
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, utilizador.Id.ToString()),
                new Claim(ClaimTypes.Role, utilizador.Role),
                new Claim(ClaimTypes.GivenName, utilizador.Nome!),
                new Claim(ClaimTypes.Sid, escolhida.Id.ToString()),
                new Claim(ClaimTypes.PrimaryGroupSid, escolhida.NomeCongregacao.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            Console.WriteLine($"[{escolhida.NomeCongregacao}] - Login - Utilizador: {utilizador.Nome}");

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
