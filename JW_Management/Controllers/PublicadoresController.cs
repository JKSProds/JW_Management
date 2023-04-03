using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;

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

            return View(context.ObterPublicador(id, true, true));
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
