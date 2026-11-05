using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
    public class ReunioesController(DbContext _dbContext, HourglassImportador _hourglass, ProgramaServico _programa, HourglassContext _hg) : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Progresso = _dbContext.ObterProgressoReunioes();
            return View(_dbContext.ObterReunioes(false));
        }

        // Sessão do Hourglass guardada em memória algumas horas, para não colar o cURL de cada vez.
        [HttpGet]
        public IActionResult HourglassSessao() => Json(new { ativa = _hg.SessaoAte(User.ObterTenant()) is not null, ate = _hg.SessaoAte(User.ObterTenant())?.ToString("HH:mm") });

        [HttpDelete]
        public IActionResult HourglassSessao(bool terminar) { _hg.Terminar(User.ObterTenant()); return Ok(); }

        // Importação do Hourglass: só leitura no Hourglass.
        [HttpPost]
        public async Task<IActionResult> HourglassPreview(string cookie, DateTime de, DateTime ate)
        {
            cookie = _hg.Resolver(User.ObterTenant(), cookie)!;
            if (cookie == null) return BadRequest("Sem sessão do Hourglass: cole o pedido copiado (falta «hglogin» ou o token XSRF).");
            if (ate < de || (ate - de).TotalDays > 120) return BadRequest("Indique um intervalo de datas até 120 dias.");
            try { return Json(await _hourglass.Pre(cookie, de, ate)); }
            catch (UnauthorizedAccessException e) { _hg.Terminar(User.ObterTenant()); return Unauthorized(e.Message); }
            catch (HttpRequestException) { return StatusCode(502, "Não foi possível obter os dados do Hourglass."); }
        }

        [HttpPost]
        public IActionResult HourglassImportar([FromBody] HgPreview dados)
        {
            try
            {
                var (semanas, designacoes) = _hourglass.Importar(dados);
                return Json(new { semanas, designacoes });
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

        // Lê o mês do Hourglass (só GET) e guarda-o; a sessão é a guardada em memória.
        [HttpPost]
        public async Task<IActionResult> ProgramaSincronizar(string cookie, int ano, int mes)
        {
            cookie = _hg.Resolver(User.ObterTenant(), cookie)!;
            if (cookie == null) return BadRequest("Sem sessão do Hourglass: cole o pedido copiado (falta «hglogin» ou o token XSRF).");
            if (mes is < 1 or > 12 || ano is < 2000 or > 2100) return BadRequest("Mês ou ano inválido.");
            try { return Json(await _programa.Sincronizar(cookie, ano, mes)); }
            catch (UnauthorizedAccessException e) { _hg.Terminar(User.ObterTenant()); return Unauthorized(e.Message); }
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

        // Só leitura: as designações vêm todas do Hourglass.
        [HttpGet]
        public IActionResult Reuniao(string id) => View(_dbContext.ObterReuniao(id, true, true));

        [HttpDelete]
        public IActionResult Reuniao(string id, bool apagar)
        {
            return _dbContext.ApagarReunioes(new List<string>() { id }) ? StatusCode(200) : StatusCode(500);
        }
    }
}
