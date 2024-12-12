using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography.Xml;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura")]
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
        public IActionResult Movimentos(int id, string data)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            DateOnly.TryParse(data, out DateOnly d);
            if (d == DateOnly.MinValue) d = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.Publicadores = context.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });
            ViewBag.Periodicos = context.ObterTipoPeriodicos().Select(l => new SelectListItem() { Value = l.Referencia, Text = l.Descricao });
            ViewData["Data"] = d;

            if (id == 1) return View("Entradas", context.ObterMovimentos(true, false, 0, d));
            return View("Saidas", context.ObterMovimentos(false, true, 0, d));
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
        public IActionResult Literaturas(string filtro, bool periodico)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            if (periodico) return Json(context.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id == 7));
            return Json(context.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id != 7));
        }

        [HttpGet]
        public IActionResult Literatura(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;


            return Json(context.ObterLiteratura(id));
        }


        [HttpPost]
        public IActionResult Literatura(string referencia, string descricao, string data, int tipo)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(referencia) || string.IsNullOrEmpty(descricao)) return StatusCode(500);
            Literatura l = new Literatura()
            {
                Descricao = descricao,
                Referencia = referencia,
                Data = data,
                Tipo = context.ObterTiposLiteratura().Where(t => t.Id == tipo).DefaultIfEmpty(context.ObterTiposLiteratura().Last()).First(),
                Stamp = DateTime.Now.Ticks.ToString()
            };

            return Json(context.AdicionarLiteratura(l) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Literatura(string stamp, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);

            return Json(context.ApagarLiteratura(stamp) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Periodicos()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterPeriodicos().Where(l => l.Quantidade > 0).OrderBy(l => l.DescricaoGeral));
        }

        [HttpPost]
        public IActionResult Periodicos(string Email, string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            List<Literatura> LstLiteratura = context.ObterPedidosPeriodico().Where(l => l.Referencia == id).OrderBy(l => l.Publicador.Nome).ToList();


            return Json(MailContext.MailPedidosPeriodicosGrupo(LstLiteratura, Email) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Periodico(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return View(context.ObterPeriodicos(id).OrderBy(p => p.Publicador.Nome));
        }

        [HttpPost]
        public IActionResult Periodico(string referencia, int idpub, int qtd)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(referencia) || idpub == 0 || qtd == 0) return StatusCode(500);

            Literatura l = new Literatura()
            {
                Referencia = referencia,
                Publicador = context.ObterPublicador(idpub, false, false, false, false),
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

            ViewBag.Publicadores = context.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });
            ViewBag.Estados = context.ObterEstadosPedido().Select(l => new SelectListItem() { Value = l.Descricao, Text = l.Descricao });
            ViewBag.Periodicos = context.ObterTipoPeriodicos().Select(l => new SelectListItem() { Value = l.Referencia, Text = l.Descricao });

            List<Literatura> LstPedidos = context.ObterPedidosPeriodico().Where(l => l.Quantidade > 0).OrderBy(l => l.Publicador.Nome).OrderBy(l => l.Referencia).ToList();
            LstPedidos.AddRange(context.ObterPedidosEspeciais().Where(l => l.Quantidade > 0).OrderBy(l => l.Publicador.Nome));

            return View(LstPedidos);
        }

        [HttpPost]
        public IActionResult Pedidos(string Email)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            List<Literatura> LstLiteratura = context.ObterPedidosEspeciais().Where(l => l.EstadoPedido.Descricao == "Pendente").ToList();

            foreach (var l in LstLiteratura)
            {
                context.AtualizarEstadoPedidoEspecial(l, new EstadoPedido("Enviado Email"));
            }

            return Json(MailContext.MailPedidosEspeciaisPendentes(LstLiteratura, Email) ? StatusCode(200) : StatusCode(500));
        }

        [HttpPost]
        public IActionResult Pedido(string stamp, int idpub, int qtd)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);

            Literatura l = new Literatura()
            {
                Publicador = context.ObterPublicador(idpub, false, false, false, false),
                Quantidade = qtd,
                Stamp = stamp
            };

            return Json(context.AdicionarPedidoEspecial(l, new EstadoPedido()) ? l.Stamp : StatusCode(500));
        }

        [HttpPut]
        public IActionResult Pedido(string stamp, int qtd, string estado)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);

            Literatura l = context.ObterPedidoEspecial(stamp);
            l.Quantidade = qtd;
            EstadoPedido e = new EstadoPedido(estado);

            return Json(context.AtualizarEstadoPedidoEspecial(l, e) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Pedido(string id, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return Json(context.ApagarPedidoEspecial(id) ? StatusCode(200) : StatusCode(500));
        }


        [HttpGet]
        public IActionResult Formulario()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext fileContext = new FileContext();

            var file = fileContext.PreencherFormularioS28(context!).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "S-28_TPO_Preenchido.pdf",
                Inline = true,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
        
        [HttpGet]
        public IActionResult API(string id)
        {
            JWApi api = HttpContext.RequestServices.GetService(typeof(JWApi)) as JWApi;
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            api.XSRFToken = id;
            var i = api.ObterInventario();
            
            if ((i.DataInventario.Month == DateTime.Now.Month && i.DataInventario.Year == DateTime.Now.Year) || string.IsNullOrEmpty(i.StampInventario)) return BadRequest();
            List<Literatura> lstLiteraturas = context.ObterLiteraturas(i.DataInventario.Month, i.DataInventario.Year).Where(o => o.Tipo.Id != 7).ToList();

            foreach (var l in i.Literatura)
            {
                l.Quantidade = lstLiteraturas.Where(o => o.Referencia == l.Referencia).DefaultIfEmpty(new Literatura()).First().Quantidade;
                api.AtualizarLiteratura(i, l);
            }
           
            return Ok(api.FecharInventario(i));
        }

        [HttpPost]
        public IActionResult Imagem(string id, IFormFile file)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext f = new();
            if (string.IsNullOrEmpty(id) || file == null) return StatusCode(500);

            Literatura l = context.ObterLiteratura(id);
            string FileName = l.Referencia + "_" + DateTime.Now.Ticks.ToString() + "." + file.FileName.Split(".").Last();

            if (f.GuardarImagemLiteratura(file, FileName)) return context.AdicionarImagemLiteratura(l, FileName) ? StatusCode(200) : StatusCode(500);

            return StatusCode(500);
        }
    }
}
