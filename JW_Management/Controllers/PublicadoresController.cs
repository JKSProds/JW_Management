using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{

    public class PublicadoresController(DbContext _dbContext, MailContext _mailContext) : Controller
    {
        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpGet]
        public IActionResult Index()
        {
            return View(_dbContext.ObterPublicadores(false).OrderBy(p => p.Nome));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpGet]
        public IActionResult Publicador(int id)
        {
            List<Grupo> LstGrupos = _dbContext.ObterGrupos();
            LstGrupos.Insert(0, new Grupo() { Id = 0, Nome = "N/D", Responsavel = new Publicador() { Nome = "N/D" } });
            ViewBag.Grupos = LstGrupos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome + " (" + l.Responsavel.Nome + ")" });

            return View(_dbContext.ObterPublicador(id, true, true, true, false));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura, Territorios")]
        public IActionResult ObterPublicador(int id)
        {
            return Json(_dbContext.ObterPublicador(id, false, false, false, false));
        }


        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura, Territorios")]
        [HttpGet]
        public IActionResult ObterPublicadores()
        {
            return Json(_dbContext.ObterPublicadores(false));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpPost]
        public IActionResult Publicador(Publicador p)
        {
            if (p.Id == 0) return StatusCode(500);
            if (!string.IsNullOrEmpty(p.Nome) && string.IsNullOrEmpty(p.Username)) p.Username = p.Nome.Replace(" ", "").ToLower();

            Publicador pOriginal = _dbContext.ObterPublicador(p.Id, false, false, false, false);

            var passwordHasher = new PasswordHasher<string>();
            if (pOriginal.Password != p.Password && !string.IsNullOrEmpty(p.Password))
            {
                p.Password = passwordHasher.HashPassword(p.Username!, p.Password);
            }
            else
            {
                p.Password = pOriginal.Password;
            }

            return _dbContext.AdicionarPublicador(p) ? RedirectToAction("Publicador", p.Id) : StatusCode(500);
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpDelete]
        public IActionResult Publicador(int id, bool delete)
        {
            if (!delete) return StatusCode(403);

            return _dbContext.ApagarPublicador(id) ? StatusCode(200) : StatusCode(500);
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
            _mailContext.MailSenhaPublicador(p);
            
            return Publicador(p);
        }
    }
}
