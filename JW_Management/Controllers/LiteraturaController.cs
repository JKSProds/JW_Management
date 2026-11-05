using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura")]
    public class LiteraturaController(DbContext _dbContext, FileContext _fileContext, JWApiContext _jwApiContext, MailContext _mailContext) : Controller
    {
        private static readonly string[] ExtensoesImagem = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

        // Resposta uniforme: 200 em caso de sucesso (com valor opcional), 500 caso contrário.
        private IActionResult Resultado(bool ok) => ok ? Ok() : StatusCode(500);
        private IActionResult Resultado(bool ok, object valor) => ok ? Json(valor) : StatusCode(500);

        private IEnumerable<SelectListItem> ListaPublicadores() =>
            _dbContext.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });

        private IEnumerable<SelectListItem> ListaPeriodicos() =>
            _dbContext.ObterTipoPeriodicos().Select(l => new SelectListItem() { Value = l.Referencia, Text = l.Descricao });

        // A pesquisa e o filtro por tipo são feitos no browser (sem recarregar); os parâmetros só pré-selecionam o estado inicial.
        public IActionResult Index(string filtro, int tipo)
        {
            ViewData["filtro"] = filtro ?? "";
            ViewData["tipo"] = tipo;

            List<TipoLiteratura> LstTiposLiteratura = _dbContext.ObterTiposLiteratura();
            LstTiposLiteratura.Insert(0, new TipoLiteratura() { Id = 0, Descricao = "Todos" });
            ViewBag.Tipos = LstTiposLiteratura.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Descricao });

            // Periódicos sem stock não aparecem na listagem.
            return View("Index", _dbContext.ObterLiteraturas("", 0)
                .Where(l => !(l.Tipo!.Id == 7 && l.Quantidade <= 0))
                .OrderBy(l => l.Referencia));
        }

        [HttpGet]
        public IActionResult Movimentos(int id, string data)
        {
            DateOnly.TryParse(data, out DateOnly d);
            if (d == DateOnly.MinValue) d = DateOnly.FromDateTime(DateTime.Now);
            ViewBag.Publicadores = ListaPublicadores();
            ViewBag.Periodicos = ListaPeriodicos();
            ViewData["Data"] = d;

            if (id == 1) return View("Entradas", _dbContext.ObterMovimentos(true, false, 0, d));
            return View("Saidas", _dbContext.ObterMovimentos(false, true, 0, d));
        }

        [HttpPost]
        public IActionResult Movimentos(string stamp, int qtd, int idpub)
        {
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);
            Literatura l = _dbContext.ObterLiteratura(stamp);

            Movimentos m = new Movimentos()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Literatura = l,
                Quantidade = qtd,
                DataMovimento = DateTime.Now,
                Publicador = new Publicador() { Id = idpub }
            };

            return Resultado(_dbContext.AdicionarMovimento(m));
        }

        [HttpDelete]
        public IActionResult Movimentos(string stamp)
        {
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);
            return Resultado(_dbContext.ApagarMovimento(stamp));
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
            
            // Um periódico já existente (mesma referência e data) é reutilizado.
            var existente = _dbContext.ObterPeriodicosData(data, referencia).FirstOrDefault();
            if (existente != null) return Json(existente.Stamp);

            var tipos = _dbContext.ObterTiposLiteratura();
            Literatura l = new Literatura()
            {
                Descricao = descricao,
                Referencia = referencia,
                Data = data,
                Tipo = tipos.FirstOrDefault(t => t.Id == tipo) ?? tipos.Last(),
                Stamp = DateTime.Now.Ticks.ToString()
            };

            return Resultado(_dbContext.AdicionarLiteratura(l), l.Stamp!);
        }

        [HttpDelete]
        public IActionResult Literatura(string stamp, bool apagar)
        {
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);

            return Resultado(_dbContext.ApagarLiteratura(stamp));
        }
        
        [HttpGet]
        public IActionResult PesquisarPeriodicos(string ano, string tipo)
        {
            var t = _dbContext.ObterTipoPeriodicos().FirstOrDefault(tp => tp.Referencia == tipo);
            if (t == null) return BadRequest();
            return Json(_jwApiContext.ObterPeriodicos(ano, t));
        }

        [HttpGet]
        public IActionResult Periodicos()
        {
            return View(_dbContext.ObterPeriodicos().Where(l => l.Quantidade > 0).OrderBy(l => l.DescricaoGeral));
        }

        [HttpPost]
        public IActionResult Periodicos(string Email, string id)
        {
            List<Literatura> LstLiteratura = _dbContext.ObterPedidosPeriodico().OrderBy(l => l.Publicador.Nome).ToList();
            if (id != "0")
            {
                LstLiteratura = LstLiteratura.Where(l => l.Referencia == id).OrderBy(l => l.Publicador.Nome).ToList();
            }
            
            return Resultado(_mailContext.MailPedidosPeriodicosGrupo(LstLiteratura, Email));
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

            return Resultado(_dbContext.AdicionarPedidoPeriodico(l), l.Stamp!);
        }

        [HttpDelete]
        public IActionResult Periodico(string id, bool apagar)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return Resultado(_dbContext.ApagarPedidoPeriodico(id));
        }

        [HttpGet]
        public IActionResult Pedidos()
        {
            ViewBag.Publicadores = ListaPublicadores();
            ViewBag.Estados = _dbContext.ObterEstadosPedido().Select(l => new SelectListItem() { Value = l.Descricao, Text = l.Descricao });
            ViewBag.Periodicos = ListaPeriodicos();

            List<Literatura> LstPedidos = _dbContext.ObterPedidosPeriodico().Where(l => l.Quantidade > 0).OrderBy(l => l.Referencia).ThenBy(l => l.Publicador!.Nome).ToList();
            LstPedidos.AddRange(_dbContext.ObterPedidosEspeciais().Where(l => l.Quantidade > 0).OrderBy(l => l.Publicador!.Nome));

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

            return Resultado(_mailContext.MailPedidosEspeciaisPendentes(LstLiteratura, Email));
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

            return Resultado(_dbContext.AdicionarPedidoEspecial(l, new EstadoPedido()), l.Stamp);
        }

        [HttpPut]
        public IActionResult Pedido(string stamp, int qtd, string estado)
        {
            if (string.IsNullOrEmpty(stamp) || qtd == 0) return StatusCode(500);

            Literatura l = _dbContext.ObterPedidoEspecial(stamp);
            l.Quantidade = qtd;
            EstadoPedido e = new EstadoPedido(estado);

            return Resultado(_dbContext.AtualizarEstadoPedidoEspecial(l, e), l.Stamp!);
        }

        [HttpDelete]
        public IActionResult Pedido(string id, bool apagar)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return Resultado(_dbContext.ApagarPedidoEspecial(id));
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
        
        // POST: a cookie é um segredo e não deve ir no URL (ficaria nos registos do servidor).
        [HttpPost]
        public IActionResult API(string id)
        {
            _jwApiContext.Cookies = id;
            var i = _jwApiContext.ObterInventario();
            
            if ((i.DataInventario.Month == DateTime.Now.Month && i.DataInventario.Year == DateTime.Now.Year) || string.IsNullOrEmpty(i.StampInventario)) return BadRequest();
            List<Literatura> lstLiteraturas = _dbContext.ObterLiteraturas(i.DataInventario.Month, i.DataInventario.Year);

            foreach (var l in i.Literatura.Where(o => string.IsNullOrEmpty(o.Data)))
            {
                l.Quantidade = lstLiteraturas.Where(o => o.Referencia == l.Referencia).DefaultIfEmpty(new Literatura()).First().Quantidade;
                _jwApiContext.AtualizarLiteratura(i, l);
            }
           
            foreach (var l in i.Literatura.Where(o => !string.IsNullOrEmpty(o.Data)))
            {
                l.Quantidade = lstLiteraturas.Where(o => o.Referencia == l.Referencia && l.Data == o.Data).DefaultIfEmpty(new Literatura()).First().Quantidade;
                _jwApiContext.AtualizarLiteratura(i, l);
            }
           
            return Ok(_jwApiContext.FecharInventario(i));
        }

        [HttpPost]
        public IActionResult Imagem(string id, IFormFile file)
        {
            if (string.IsNullOrEmpty(id) || file == null) return StatusCode(500);

            // Só aceita imagens: a extensão vem do cliente e faz parte do nome do ficheiro guardado.
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!ExtensoesImagem.Contains(ext)) return BadRequest();

            Literatura l = _dbContext.ObterLiteratura(id);
            string FileName = l.Referencia + "_" + DateTime.Now.Ticks.ToString() + ext;

            return Resultado(_fileContext.GuardarImagemLiteratura(file, FileName) && _dbContext.AdicionarImagemLiteratura(l, FileName));
        }
    }
}
