using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography.Xml;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura")]
    public class LiteraturaController(DbContext _dbContext, FileContext _fileContext, JWApiContext _jwApiContext) : Controller
    {
        public IActionResult Index(string filtro, int tipo)
        {
            if (string.IsNullOrEmpty(filtro)) filtro = "";
            ViewData["filtro"] = filtro;
            ViewData["tipo"] = tipo;

            List<TipoLiteratura> LstTiposLiteratura = _dbContext.ObterTiposLiteratura();
            LstTiposLiteratura.Insert(0, new TipoLiteratura() { Id = 0, Descricao = "Todos" });
            ViewBag.Tipos = LstTiposLiteratura.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Descricao });

            if (tipo == 0) { return View("Index", _dbContext.ObterLiteraturas(filtro, 0).OrderBy(l => l.Referencia)); }
            return View("Index", _dbContext.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id == tipo).OrderBy(l => l.Referencia));
        }

        [HttpGet]
        public IActionResult Movimentos(int id, string data)
        {
            DateOnly.TryParse(data, out DateOnly d);
            if (d == DateOnly.MinValue) d = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.Publicadores = _dbContext.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });
            ViewBag.Periodicos = _dbContext.ObterTipoPeriodicos().Select(l => new SelectListItem() { Value = l.Referencia, Text = l.Descricao });
            ViewData["Data"] = d;

            if (id == 1) return View("Entradas", _dbContext.ObterMovimentos(true, false, 0, d));
            return View("Saidas", _dbContext.ObterMovimentos(false, true, 0, d));
        }

        [HttpPost]
        public IActionResult Movimentos(string stamp, int qtd, int idpub)
        {
            Literatura l = _dbContext.ObterLiteratura(stamp);
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);

            Movimentos m = new Movimentos()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Literatura = l,
                Quantidade = qtd,
                DataMovimento = DateTime.Now,
                Publicador = new Publicador() { Id = idpub }
            };

            return _dbContext.AdicionarMovimento(m) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Movimentos(string stamp)
        {
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);
            return Json(_dbContext.ApagarMovimento(stamp) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Literaturas(string filtro, bool periodico)
        {
            if (periodico) return Json(_dbContext.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id == 7));
            return Json(_dbContext.ObterLiteraturas(filtro, 0).Where(l => l.Tipo.Id != 7));
        }

        [HttpGet]
        public IActionResult Literatura(string id)
        {
            return Json(_dbContext.ObterLiteratura(id));
        }


        [HttpPost]
        public IActionResult Literatura(string referencia, string descricao, string data, int tipo)
        {
            if (string.IsNullOrEmpty(referencia) || string.IsNullOrEmpty(descricao)) return StatusCode(500);
            Literatura l = new Literatura()
            {
                Descricao = descricao,
                Referencia = referencia,
                Data = data,
                Tipo = _dbContext.ObterTiposLiteratura().Where(t => t.Id == tipo).DefaultIfEmpty(_dbContext.ObterTiposLiteratura().Last()).First(),
                Stamp = DateTime.Now.Ticks.ToString()
            };

            return Json(_dbContext.AdicionarLiteratura(l) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Literatura(string stamp, bool apagar)
        {
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);

            return Json(_dbContext.ApagarLiteratura(stamp) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Periodicos()
        {
            return View(_dbContext.ObterPeriodicos().Where(l => l.Quantidade > 0).OrderBy(l => l.DescricaoGeral));
        }

        [HttpPost]
        public IActionResult Periodicos(string Email, string id)
        {
            List<Literatura> LstLiteratura = _dbContext.ObterPedidosPeriodico().Where(l => l.Referencia == id).OrderBy(l => l.Publicador.Nome).ToList();
            
            return Json(MailContext.MailPedidosPeriodicosGrupo(LstLiteratura, Email) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Periodico(string id)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return View(_dbContext.ObterPeriodicos(id).OrderBy(p => p.Publicador.Nome));
        }

        [HttpPost]
        public IActionResult Periodico(string referencia, int idpub, int qtd)
        {
            if (string.IsNullOrEmpty(referencia) || idpub == 0 || qtd == 0) return StatusCode(500);

            Literatura l = new Literatura()
            {
                Referencia = referencia,
                Publicador = _dbContext.ObterPublicador(idpub, false, false, false, false),
                Quantidade = qtd,
                Stamp = DateTime.Now.Ticks.ToString()
            };

            return Json(_dbContext.AdicionarPedidoPeriodico(l) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Periodico(string id, bool apagar)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return Json(_dbContext.ApagarPedidoPeriodico(id) ? StatusCode(200) : StatusCode(500));
        }

        [HttpGet]
        public IActionResult Pedidos()
        {
            ViewBag.Publicadores = _dbContext.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });
            ViewBag.Estados = _dbContext.ObterEstadosPedido().Select(l => new SelectListItem() { Value = l.Descricao, Text = l.Descricao });
            ViewBag.Periodicos = _dbContext.ObterTipoPeriodicos().Select(l => new SelectListItem() { Value = l.Referencia, Text = l.Descricao });

            List<Literatura> LstPedidos = _dbContext.ObterPedidosPeriodico().Where(l => l.Quantidade > 0).OrderBy(l => l.Publicador.Nome).OrderBy(l => l.Referencia).ToList();
            LstPedidos.AddRange(_dbContext.ObterPedidosEspeciais().Where(l => l.Quantidade > 0).OrderBy(l => l.Publicador.Nome));

            return View(LstPedidos);
        }

        [HttpPost]
        public IActionResult Pedidos(string Email)
        {
            List<Literatura> LstLiteratura = _dbContext.ObterPedidosEspeciais().Where(l => l.EstadoPedido.Descricao == "Pendente").ToList();

            foreach (var l in LstLiteratura)
            {
                _dbContext.AtualizarEstadoPedidoEspecial(l, new EstadoPedido("Enviado Email"));
            }

            return Json(MailContext.MailPedidosEspeciaisPendentes(LstLiteratura, Email) ? StatusCode(200) : StatusCode(500));
        }

        [HttpPost]
        public IActionResult Pedido(string stamp, int idpub, int qtd)
        {
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);

            Literatura l = new Literatura()
            {
                Publicador = _dbContext.ObterPublicador(idpub, false, false, false, false),
                Quantidade = qtd,
                Stamp = stamp
            };

            return Json(_dbContext.AdicionarPedidoEspecial(l, new EstadoPedido()) ? l.Stamp : StatusCode(500));
        }

        [HttpPut]
        public IActionResult Pedido(string stamp, int qtd, string estado)
        {
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);

            Literatura l = _dbContext.ObterPedidoEspecial(stamp);
            l.Quantidade = qtd;
            EstadoPedido e = new EstadoPedido(estado);

            return Json(_dbContext.AtualizarEstadoPedidoEspecial(l, e) ? l.Stamp : StatusCode(500));
        }

        [HttpDelete]
        public IActionResult Pedido(string id, bool apagar)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return Json(_dbContext.ApagarPedidoEspecial(id) ? StatusCode(200) : StatusCode(500));
        }


        [HttpGet]
        public IActionResult Formulario()
        {
            var file = _fileContext.PreencherFormularioS28(_dbContext!).ToArray();
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
            _jwApiContext.Cookies = id;
            var i = _jwApiContext.ObterInventario();
            
            if ((i.DataInventario.Month == DateTime.Now.Month && i.DataInventario.Year == DateTime.Now.Year) || string.IsNullOrEmpty(i.StampInventario)) return BadRequest();
            List<Literatura> lstLiteraturas = _dbContext.ObterLiteraturas(i.DataInventario.Month, i.DataInventario.Year).Where(o => o.Tipo.Id != 7).ToList();

            foreach (var l in i.Literatura)
            {
                l.Quantidade = lstLiteraturas.Where(o => o.Referencia == l.Referencia).DefaultIfEmpty(new Literatura()).First().Quantidade;
                _jwApiContext.AtualizarLiteratura(i, l);
            }
           
            return Ok(_jwApiContext.FecharInventario(i));
        }

        [HttpPost]
        public IActionResult Imagem(string id, IFormFile file)
        {
            if (string.IsNullOrEmpty(id) || file == null) return StatusCode(500);

            Literatura l = _dbContext.ObterLiteratura(id);
            string FileName = l.Referencia + "_" + DateTime.Now.Ticks.ToString() + "." + file.FileName.Split(".").Last();

            if (_fileContext.GuardarImagemLiteratura(file, FileName)) return _dbContext.AdicionarImagemLiteratura(l, FileName) ? StatusCode(200) : StatusCode(500);

            return StatusCode(500);
        }
    }
}
