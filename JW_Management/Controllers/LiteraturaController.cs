using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    public class LiteraturaController : Controller
    {
        public IActionResult Index(int grupo, string filtro)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            if (grupo == 0) grupo = 100;
            if (string.IsNullOrEmpty(filtro)) filtro = "";

            ViewData["grupo"] = grupo;
            ViewData["filtro"] = filtro;

            ViewBag.Grupos = context.ObterGrupos().Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });

            return View("Index", context.ObterLiteratura(grupo, filtro));
        }

        public JsonResult AtualizarStock(string stamp, int qtd)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            Literatura l = context.ObterLiteratura(stamp);
            l.Quantidade = qtd;

            return Json(context.AtualizarLiteratura(l));
        }
    }
}
