using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Pqc.Crypto.Falcon;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
    public class ReunioesController(DbContext _dbContext, FileContext _fileContext, HourglassImportador _hourglass, HourglassPrograma _programa) : Controller
    {
        // O DbContext monta SQL por texto: o texto livre escreve-se escapado.
        private static string Esc(string? s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "''");

        public IActionResult Index(string filtro, int tipo)
        {
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

        [HttpGet]
        public IActionResult Calendario()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Reunioes(IFormFile file)
        {

            List<Designacao> LstD = _fileContext.ImportarDesignacoes(file, _dbContext!);

            return _dbContext.ApagarReunioes(LstD.Select(d => d.StampReuniao.ToString()).Distinct().ToList()) ? _dbContext.AdicionarDesignacoes(LstD) ? StatusCode(200) : StatusCode(500) : StatusCode(500);
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

        // Programa mensal de atividades (PDF) lido do Hourglass; a cookie é usada neste pedido e não é guardada.
        [HttpPost]
        public async Task<IActionResult> Programa(string cookie, int ano, int mes, string? dirigente)
        {
            if (!HourglassContext.CookieValida(cookie)) return BadRequest("Cookie inválida: falta «hglogin» ou o token XSRF.");
            if (mes is < 1 or > 12 || ano is < 2000 or > 2100) return BadRequest("Mês ou ano inválido.");
            try
            {
                var programa = await _programa.Obter(cookie, ano, mes, User.ObterCong(), (dirigente ?? "").Trim());
                return File(ProgramaPdf.Gerar(programa), "application/pdf", $"Programa_{ano}_{mes:00}.pdf");
            }
            catch (UnauthorizedAccessException e) { return Unauthorized(e.Message); }
            catch (HttpRequestException) { return StatusCode(502, "Não foi possível obter os dados do Hourglass."); }
        }
    }
}
