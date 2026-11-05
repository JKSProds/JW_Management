using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
    public class ReunioesController(DbContext _dbContext, HourglassImportador _hourglass, ProgramaServico _programa) : Controller
    {
        // O DbContext monta SQL por texto: o texto livre escreve-se escapado.
        private static string Esc(string? s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "''");

        public IActionResult Index()
        {
            ViewBag.Progresso = _dbContext.ObterProgressoReunioes();
            return View(_dbContext.ObterReunioes(false));
        }

        // Importação do Hourglass: só leitura no Hourglass; a cookie é usada neste pedido e não é guardada.
        [HttpPost]
        public async Task<IActionResult> HourglassPreview(string cookie, DateTime de, DateTime ate)
        {
            if (!HourglassContext.CookieValida(cookie)) return BadRequest("Cookie inválida: falta «hglogin» ou o token XSRF.");
            if (ate < de || (ate - de).TotalDays > 120) return BadRequest("Indique um intervalo de datas até 120 dias.");
            try { return Json(await _hourglass.Pre(cookie, de, ate)); }
            catch (UnauthorizedAccessException e) { return Unauthorized(e.Message); }
            catch (HttpRequestException) { return StatusCode(502, "Não foi possível obter os dados do Hourglass."); }
        }

        [HttpPost]
        public IActionResult HourglassImportar([FromBody] HgPreview dados)
        {
            try
            {
                var (semanas, designacoes, discursos) = _hourglass.Importar(dados);
                return Json(new { semanas, designacoes, discursos });
            }
            catch (InvalidOperationException e) { return StatusCode(500, e.Message); }
        }

        // O antigo «Calendário» (vazio) deu lugar ao Programa.
        [HttpGet]
        public IActionResult Calendario() => RedirectToActionPermanent("Programa");

        // Programa do mês (reuniões, serviço de campo e testemunho público), lido da cópia guardada na base de dados.
        [HttpGet]
        public IActionResult Programa(int? ano, int? mes)
        {
            var hoje = DateTime.Today;
            int a = ano ?? hoje.Year, m = mes ?? hoje.Month;
            if (m is < 1 or > 12 || a is < 2000 or > 2100) { a = hoje.Year; m = hoje.Month; }

            return View(new ProgramaPagina
            {
                Ano = a, Mes = m,
                Estado = _programa.Estado(a, m),
                Programa = _programa.ObterPrograma(a, m, User.ObterCong()),
                Campo = _programa.ObterCampo(a, m),
                Testemunho = _programa.ObterTestemunho(a, m)
            });
        }

        // Lê o mês do Hourglass (só GET) e guarda-o; a cookie é usada neste pedido e não é guardada.
        [HttpPost]
        public async Task<IActionResult> ProgramaSincronizar(string cookie, int ano, int mes)
        {
            if (!HourglassContext.CookieValida(cookie)) return BadRequest("Cookie inválida: falta «hglogin» ou o token XSRF.");
            if (mes is < 1 or > 12 || ano is < 2000 or > 2100) return BadRequest("Mês ou ano inválido.");
            try { return Json(await _programa.Sincronizar(cookie, ano, mes)); }
            catch (UnauthorizedAccessException e) { return Unauthorized(e.Message); }
            catch (HttpRequestException) { return StatusCode(502, "Não foi possível obter os dados do Hourglass."); }
        }

        // PDF a partir dos dados já guardados: não precisa da cookie. tipo = programa | campo | testemunho.
        [HttpGet]
        public IActionResult ProgramaPdf(string tipo, int ano, int mes, string? dirigente)
        {
            var cong = User.ObterCong();
            byte[]? pdf = tipo switch
            {
                "programa" => _programa.ObterPrograma(ano, mes, cong, (dirigente ?? "").Trim()) is { } p ? PdfProgramas.Gerar(p) : null,
                "campo" => _programa.ObterCampo(ano, mes) is { } c ? PdfProgramas.GerarCampo(c, cong, ano, mes) : null,
                "testemunho" => _programa.ObterTestemunho(ano, mes) is { } t ? PdfProgramas.GerarTestemunho(t, cong, ano, mes) : null,
                _ => null
            };
            if (pdf == null) return NotFound("Ainda não há dados guardados para este mês: sincronize-os do Hourglass.");
            return File(pdf, "application/pdf", $"{tipo}_{ano}_{mes:00}.pdf");
        }

        [HttpGet]
        public IActionResult Reuniao(string id)
        {
            ViewBag.Publicadores = _dbContext.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Nome });
            ViewBag.Tipos = _dbContext.ObterTiposDesignacao().OrderBy(p => p.Id).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Descricao });
            ViewBag.Canticos = _dbContext.ObterCanticos().OrderBy(p => p.Id).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Id + " - " + l.Nome });
            ViewBag.Grupos = _dbContext.ObterGrupos().OrderBy(p => p.Id).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Nome });

            return View(_dbContext.ObterReuniao(id, true, true));
        }
        [HttpPost]
        public IActionResult Reuniao(DateTime data)
        {
            string semana = "Semana " + ((data.DayOfYear - 1) / 7 + 1) + " de " + data.Year.ToString();
            string stamp = DateTime.Now.Ticks.ToString();

            if (!_dbContext.AdicionarReuniao(semana.ToUpper(), stamp)) return StatusCode(500);

            return RedirectToAction("Reuniao", new { id = stamp });
        }
        [HttpDelete]
        public IActionResult Reuniao(string id, bool apagar)
        {
            return _dbContext.ApagarReunioes(new List<string>() { id }) ? StatusCode(200) : StatusCode(500);
        }

        [HttpPost]
        public IActionResult Designacao(string id, int tipo, string local)
        {
             TipoDesignacao t = _dbContext.ObterTiposDesignacao().Where(t => t.Id == tipo).DefaultIfEmpty(new TipoDesignacao()).First();

            Designacao d = new Designacao()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                StampReuniao = id,
                SemanaReuniao = _dbContext.ObterDesignacoes(id).DefaultIfEmpty(new Models.Designacao()).First().SemanaReuniao,
                NomePublicador = "",
                NomeDesignacao = t.Descricao,
                Local = local,
                Publicador = _dbContext.ObterPublicador(0, false, false, false, false),
                TipoDesignacao = t,
            };

            return _dbContext.AdicionarDesignacao(d) ? StatusCode(200) : StatusCode(500);
        }

        [HttpPut]
        public IActionResult Designacao(string id, int pub, int minutos, string tema)
        {
            Designacao d = _dbContext.ObterDesignacao(id);

            d.Publicador = _dbContext.ObterPublicador(pub, false, false, false, false);
            d.NMin = minutos;
            d.NomeDesignacao = Esc(tema);

            return _dbContext.AtualizarDesignacao(d) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Designacao(string id, bool apagar)
        {
            return _dbContext.ApagarDesignacoes(id.Split(",").ToList()) ? StatusCode(200) : StatusCode(500);
        }

        [HttpPost]
        public IActionResult Publicador(string id, int pub)
        {
            Designacao d = _dbContext.ObterDesignacao(id);
            Publicador p = _dbContext.ObterPublicador(pub, false, false, false, false);

            d.Stamp = DateTime.Now.Ticks.ToString();
            d.Publicador = p;
            d.NomePublicador = p.Nome;
            return _dbContext.AdicionarDesignacao(d) ? StatusCode(200) : StatusCode(500);
        }


        [HttpPut]
        public IActionResult Cantico(int id, string stamp)
        {
            Cantico c = new Models.Cantico() { Id = id, Stamp = stamp };

            return _dbContext.AtualizarCantico(c) ? StatusCode(200) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Discursos()
        {
            ViewBag.Publicadores = _dbContext.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Nome });
            ViewBag.Discursos = _dbContext.ObterDiscursoTemas().OrderBy(p => p.NumDiscurso).Select(l => new SelectListItem() { Value = l.NumDiscurso.ToString(), Text = l.NumDiscurso + " - " + l.TemaDiscurso });

            return View(_dbContext.ObterDiscursos());
        }

        [HttpPost]
        public IActionResult Discurso(int txtIdPub, string txtNomePub, string txtCongPub, int txtIdDiscurso, DateTime txtData)
        {
            txtNomePub = Esc(txtNomePub); txtCongPub = Esc(txtCongPub);
            Publicador p = _dbContext.ObterPublicador(txtIdPub, false, false, false, false);
            Discurso t = _dbContext.ObterDiscursoTema(txtIdDiscurso);

            if (txtIdPub > 0) txtCongPub = "INTERNO";
            if (txtIdPub > 0) txtNomePub = p.Nome;

            Discurso d = new Discurso()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Pub = p,
                NomePublicador = txtNomePub,
                Congregacao = txtCongPub,
                NumDiscurso = t.NumDiscurso,
                TemaDiscurso = t.TemaDiscurso,
                DataDiscurso = txtData
            };

            _dbContext.CriarDiscurso(d);

            return RedirectToAction("Discursos");
        }

        [HttpDelete]
        public IActionResult Discurso(string id, bool apagar)
        {
            return _dbContext.ApagarDiscurso(_dbContext.ObterDiscurso(id)) ? StatusCode(200) : StatusCode(500);
        }
    }
}
