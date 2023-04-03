using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography.Xml;

namespace JW_Management.Controllers
{
    public class LiteraturaController : Controller
    {
        public IActionResult Index(string filtro, int tipo)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            if (string.IsNullOrEmpty(filtro)) filtro = "";
            ViewData["filtro"] = filtro;
            ViewData["tipo"] = tipo;

            List<TipoLiteratura> LstTiposLiteratura = context.ObterTiposLiteratura();
            LstTiposLiteratura.Insert(0, new TipoLiteratura() { Id = 0, Descricao = "Todos" });
            ViewBag.Tipos = LstTiposLiteratura.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Descricao });

            if (tipo == 0) { return View("Index", context.ObterLiteraturas(filtro, 0)); }
            return View("Index", context.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id == tipo));
        }

        [HttpGet]
        public IActionResult Movimentos(int id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            ViewBag.Publicadores = context.ObterPublicadores().Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });
            ViewBag.Periodicos = context.ObterTipoPeriodicos().Select(l => new SelectListItem() { Value = l.Referencia, Text = l.Descricao });

            if (id == 1) return View("Entradas", context.ObterMovimentos(true, false, 0));
            return View("Saidas", context.ObterMovimentos(false, true, 0));
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
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);
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
            if (string.IsNullOrEmpty(referencia) || string.IsNullOrEmpty(descricao)) return StatusCode(500);
            Literatura l = new Literatura()
            {
                Descricao = descricao,
                Referencia = referencia,
                Tipo = context.ObterTiposLiteratura().Where(t => t.Id == tipo).DefaultIfEmpty(context.ObterTiposLiteratura().Last()).First(),
                Stamp = DateTime.Now.Ticks.ToString()
            };

            return Json(context.AdicionarLiteratura(l) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Literatura(string stamp)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);

            return Json(context.ApagarLiteratura(stamp) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Periodicos()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterPeriodicos().Where(l => l.Quantidade > 0));
        }

        [HttpGet]
        public IActionResult Periodico(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return View(context.ObterPeriodicos(id));
        }

        [HttpPost]
        public IActionResult Periodico(string referencia, int idpub, int qtd)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(referencia) || idpub == 0 || qtd == 0) return StatusCode(500);

            Literatura l = new Literatura()
            {
                Referencia = referencia,
                Publicador = context.ObterPublicador(idpub, false,false),
                Quantidade = qtd,
                Stamp = DateTime.Now.Ticks.ToString()
            };

            return Json(context.AdicionarPedidoPeriodico(l) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Periodico(string id, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return Json(context.ApagarPedidoPeriodico(id) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Pedidos()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            ViewBag.Publicadores = context.ObterPublicadores().Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });
            ViewBag.Periodicos = context.ObterTipoPeriodicos().Select(l => new SelectListItem() { Value = l.Referencia, Text = l.Descricao });

            List<Literatura> LstPedidos = context.ObterPedidosPeriodico().Where(l => l.Quantidade > 0).ToList();
            LstPedidos.AddRange(context.ObterPedidosEspeciais().Where(l => l.Quantidade > 0));

            return View(LstPedidos);
        }

        [HttpPost]
        public IActionResult Pedido(string stamp, int idpub, int qtd)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stamp) || idpub == 0 || qtd == 0) return StatusCode(500);

            Literatura l = new Literatura()
            {
                Publicador = context.ObterPublicador(idpub, false,false),
                Quantidade = qtd,
                Stamp = stamp
            };

            return Json(context.AdicionarPedidoEspecial(l, new EstadoPedido()) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Pedido(string id, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return Json(context.ApagarPedidoEspecial(id) ? StatusCode(200) : StatusCode(500));
        }
    }
}
