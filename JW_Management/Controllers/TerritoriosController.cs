using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{
    [Authorize]
    public class TerritoriosController : Controller
    {
        [HttpGet]
        public IActionResult Index(string filtro)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            filtro = string.IsNullOrEmpty(filtro) ? "" : filtro;
            ViewData["filtro"] = filtro;
            ViewBag.Publicadores = context.ObterPublicadores().Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });

            return View(context.ObterTerritorios().OrderBy(p => p.Id));
        }

        [HttpPost]
        public IActionResult Territorio(string id, string nome, int dificuldade, string descricao, string url)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(nome) || dificuldade < 0) return StatusCode(500);

            Territorio t = new Territorio
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Id = id,
                Nome = nome,
                Dificuldade = dificuldade == 1 ? DificuldadeTerritorio.FACIL : dificuldade == 2 ? DificuldadeTerritorio.MODERADO : DificuldadeTerritorio.DIFICIL,
                Descricao = descricao,
                Url = url
            };

            return context.AdicionarTerritorio(t) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Territorio(string stamp, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);

            return context.ApagarTerritorio(stamp) ? StatusCode(200) : StatusCode(500);
        }


        [HttpPost]
        public IActionResult Movimento(string id, int idpub, string email, int tipo)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id) || tipo < 0) return StatusCode(500);

            Territorio t = context.ObterTerritorio(id);
            Publicador p = context.ObterPublicador(idpub, false, false, true);

            if (p.Email != email && !string.IsNullOrEmpty(email))
            {
                p.Email = email;
                context.AdicionarPublicador(p);
            }

            MovimentosTerritorio m = new MovimentosTerritorio
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Territorio = t,
                Publicador = p,
                DataMovimento = DateTime.Now,
                Tipo = tipo == 1 ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA
            };

            if (tipo == 1 && !string.IsNullOrEmpty(email)) MailContext.MailTerritorioAtribuido(t, email);
            if (tipo == 2 && !string.IsNullOrEmpty(email)) MailContext.MailTerritorioDevolver(t, email);

            return context.AdicionarMovimentoTerritorio(m) ? StatusCode(200) : StatusCode(500);
        }
    }
}
