using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Master")]
    public class PublicadoresController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterPublicadores().OrderBy(p => p.Nome));
        }

        [HttpGet]
        public IActionResult Publicador(int id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            List<Grupo> LstGrupos = context.ObterGrupos();
            LstGrupos.Insert(0, new Grupo() { Id = 0, Nome = "N/D", Responsavel = new Publicador() { Nome = "N/D" } });
            ViewBag.Grupos = LstGrupos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome + " (" + l.Responsavel.Nome + ")" });

            return View(context.ObterPublicador(id, true, true, true, true));
        }

        [HttpGet]
        public IActionResult ObterPublicador(int id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return Json(context.ObterPublicador(id, false, false, false, false));
        }

        [HttpGet]
        public IActionResult ObterPublicadores()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return Json(context.ObterPublicadores());
        }

        [HttpPost]
        public IActionResult Publicador(Publicador p)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (p.Id == 0) return StatusCode(500);
            if (!string.IsNullOrEmpty(p.Nome) && string.IsNullOrEmpty(p.Username)) p.Username = p.Nome.Replace(" ", "").ToLower();

            Publicador pOriginal = context.ObterPublicador(p.Id, false, false, false, false);

            var passwordHasher = new PasswordHasher<string>();
            if (pOriginal.Password != p.Password && !string.IsNullOrEmpty(p.Password))
            {
                p.Password = passwordHasher.HashPassword(p.Username!, p.Password);
            }
            else
            {
                p.Password = pOriginal.Password;
            }

            return context.AdicionarPublicador(p) ? RedirectToAction("Publicador", p.Id) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Publicador(int id, bool delete)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (!delete) return StatusCode(403);

            return context.ApagarPublicador(id) ? StatusCode(200) : StatusCode(500);
        }
    }
}
