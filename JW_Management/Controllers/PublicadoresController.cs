using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    public class PublicadoresController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterPublicadores().OrderBy(p => p.Id));
        }

        [HttpGet]
        public IActionResult Publicador(int id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            List<Grupo> LstGrupos = context.ObterGrupos();
            LstGrupos.Insert(0, new Grupo() { Id = 0, Nome = "N/D", Responsavel = new Publicador() { Nome = "N/D" } });
            ViewBag.Grupos = LstGrupos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome + " (" + l.Responsavel.Nome + ")" });

            return View(context.ObterPublicador(id, true, true, true));
        }

        [HttpPost]
        public IActionResult Publicador(Publicador p)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return context.AdicionarPublicador(p) ? RedirectToAction("Publicador", p.Id) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Publicador(int id, bool delete)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (!delete) return StatusCode(403);

            return View(context.ApagarPublicador(id));
        }
    }
}
