using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{

    public class PublicadoresController(DbContext _dbContext) : Controller
    {
        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpGet]
        public IActionResult Index()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterPublicadores(false).OrderBy(p => p.Nome));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpGet]
        public IActionResult Publicador(int id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            List<Grupo> LstGrupos = context.ObterGrupos();
            LstGrupos.Insert(0, new Grupo() { Id = 0, Nome = "N/D", Responsavel = new Publicador() { Nome = "N/D" } });
            ViewBag.Grupos = LstGrupos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome + " (" + l.Responsavel.Nome + ")" });

            return View(context.ObterPublicador(id, true, true, true, false));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura, Territorios")]
        public IActionResult ObterPublicador(int id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return Json(context.ObterPublicador(id, false, false, false, false));
        }


        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura, Territorios")]
        [HttpGet]
        public IActionResult ObterPublicadores()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return Json(context.ObterPublicadores(false));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
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

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpDelete]
        public IActionResult Publicador(int id, bool delete)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (!delete) return StatusCode(403);

            return context.ApagarPublicador(id) ? StatusCode(200) : StatusCode(500);
        }

        public IActionResult Senha(int id)
        {
            string s = "";
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!#$%&/()=?-_,.;:|^~";

            using var rng = RandomNumberGenerator.Create();
            while (s.Length != 12)
            {
                byte[] oneByte = new byte[1];
                rng.GetBytes(oneByte);
                char character = (char)oneByte[0];
                if (valid.Contains(character))
                {
                    s += character;
                }
            }
            
            var p = _dbContext.ObterPublicador(id, false, false, false, false);
            p.Password = s;
            _dbContext.AdicionarPublicador(p);

            MailContext.MailSenhaPublicador(p);
            
            return RedirectToAction("Publicador", new { id = p.Id });
        }
    }
}
