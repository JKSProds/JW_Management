using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    public class LiteraturaController : Controller
    {
        public IActionResult Index(string filtro)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            if (string.IsNullOrEmpty(filtro)) filtro = "";
            ViewData["filtro"] = filtro;

            return View("Index", context.ObterLiteraturas(filtro, 0));
        }

        [HttpGet]
        public IActionResult Movimentos(int id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            if (id == 1) return View("Entradas", context.ObterMovimentos(id));
            return View("Saidas", context.ObterMovimentos(id));
        }

        [HttpPost]
        public IActionResult Movimentos(string stamp, int qtd, int idpub)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            Literatura l = context.ObterLiteratura(stamp);
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);

            Movimentos m = new Movimentos()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Literatura = l,
                Quantidade = qtd,
                DataMovimento = DateTime.Now,
                Publicador = new Publicador() { Id = idpub }
            };

            return context.AdicionarMovimento(m) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Movimentos(string stamp)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return Json(context.ApagarMovimento(stamp) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Literatura(string filtro, bool periodico)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            if (periodico) return Json(context.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id == 7));
            return Json(context.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id != 7));
        }

        [HttpPost]
        public IActionResult Literatura(string referencia, string descricao, int tipo)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            Literatura l = new Literatura()
            {
                Descricao = descricao,
                Referencia = referencia,
                Tipo = context.ObterTiposLiteratura().Where(t => t.Id == tipo).DefaultIfEmpty(context.ObterTiposLiteratura().Last()).First(),
                Stamp = DateTime.Now.Ticks.ToString()
            };

            return Json(context.NovaLiteratura(l) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Literatura(string stamp)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return Json(context.ApagarLiteratura(stamp) ? StatusCode(200) : StatusCode(500));
        }
    }
}
