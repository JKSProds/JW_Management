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
            @ViewData["filtro"] = filtro;

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
    }
}
