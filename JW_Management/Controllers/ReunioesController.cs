using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
    public class ReunioesController : Controller
    {
        public IActionResult Index(string filtro, int tipo)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterReunioes(false));
        }

        [HttpPost]
        public IActionResult Reunioes(IFormFile file)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext f = new FileContext();

            List<Designacao> LstD = f.ImportarDesignacoes(file);

            return context.ApagarDesignacoes(LstD.Select(d => d.SemanaReuniao.ToString()).Distinct().ToList()) ? context.AdicionarDesignacoes(LstD) ? StatusCode(200): StatusCode(500) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Reuniao(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            
            return View(context.ObterReuniao(id, true));
        }
    }
}
