using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
    public class ReunioesController(DbContext _dbContext, HourglassImportador _hourglass, ProgramaServico _programa, HourglassContext _hg, MailContext _mailContext) : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Progresso = _dbContext.ObterProgressoReunioes();
            var reunioes = _dbContext.ObterReunioes(false);
            var progresso = (Dictionary<string, (int Total, int Atribuidas)>)ViewBag.Progresso;
            foreach (var r in reunioes)
            {
                (r.EventoMeio, r.EventoFim) = _programa.EventoDaSemana(r.SemanaReuniao);
                // Semana com uma das reuniões cancelada: as partes por atribuir dessa reunião não contam.
                if (r.EventoMeio == null && r.EventoFim == null) continue;
                var validas = _dbContext.ObterDesignacoes(r.Stamp!).Where(d => !r.Cancelada(d)).ToList();
                progresso[r.Stamp!] = (validas.Count, validas.Count(d => d.Atribuida));
            }
            return View(reunioes);
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
        private byte[]? GerarPdf(string tipo, int ano, int mes, string? dirigente)
        {
            var cong = User.ObterCong();
            return tipo switch
            {
                "programa" => _programa.ObterPrograma(ano, mes, cong, (dirigente ?? "").Trim()) is { } p ? PdfProgramas.Gerar(p) : null,
                "campo" => _programa.ObterCampo(ano, mes) is { } c ? PdfProgramas.GerarCampo(c, cong, ano, mes) : null,
                "testemunho" => _programa.ObterTestemunho(ano, mes) is { } t ? PdfProgramas.GerarTestemunho(t, cong, ano, mes) : null,
                _ => null
            };
        }

        [HttpGet]
        public IActionResult ProgramaPdf(string tipo, int ano, int mes, string? dirigente)
        {
            var pdf = GerarPdf(tipo, ano, mes, dirigente);
            if (pdf == null) return NotFound("Ainda não há dados guardados para este mês: sincronize-os do Hourglass.");
            return File(pdf, "application/pdf", $"{tipo}_{ano}_{mes:00}.pdf");
        }

        // Envia por email os PDF escolhidos (programa, campo, testemunho) aos publicadores escolhidos: um email por publicador, com os PDF anexados.
        private static readonly Dictionary<string, (string Rotulo, string Ficheiro)> TiposPdf = new()
        {
            ["programa"] = ("Reuniões", "Reunioes"), ["campo"] = ("Serviço de campo", "Servico_de_campo"), ["testemunho"] = ("Testemunho público", "Testemunho_publico")
        };

        [HttpPost]
        public IActionResult ProgramaEnviar(int ano, int mes, string? tipos, string? ids, string? dirigente)
        {
            if (mes is < 1 or > 12 || ano is < 2000 or > 2100) return BadRequest("Mês inválido.");
            var escolhidos = (tipos ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
            if (escolhidos.Count == 0 || escolhidos.Any(t => !TiposPdf.ContainsKey(t))) return BadRequest("Escolha pelo menos um programa.");
            var idsSel = (ids ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(i => int.TryParse(i, out var n) ? n : 0).Where(n => n > 0).ToHashSet();
            if (idsSel.Count == 0) return BadRequest("Escolha pelo menos um publicador.");
            if (idsSel.Count > 300) return BadRequest("Demasiados destinatários num só envio (máximo 300).");

            var pdfs = new List<(string, byte[])>();
            foreach (var t in escolhidos)
            {
                var pdf = GerarPdf(t, ano, mes, dirigente);
                if (pdf == null) return BadRequest($"Ainda não há dados guardados de «{TiposPdf[t].Rotulo}» neste mês: obtenha-os do Hourglass.");
                pdfs.Add(($"{TiposPdf[t].Ficheiro}_{ano}_{mes:00}.pdf", pdf));
            }

            var pubs = _dbContext.ObterPublicadores(false).Where(p => idsSel.Contains(p.Id) && !string.IsNullOrWhiteSpace(p.Email)).ToList();
            if (pubs.Count == 0) return BadRequest("Nenhum dos publicadores escolhidos tem email.");

            var enviados = _mailContext.MailProgramas(pubs, pdfs, $"{ProgramaServico.NomeMes(mes)} {ano}");
            return Json(new { enviados, total = pubs.Count });
        }

        // Só leitura: as designações vêm todas do Hourglass.
        [HttpGet]
        public IActionResult Reuniao(string id)
        {
            var r = _dbContext.ObterReuniao(id, true, true);
            (r.EventoMeio, r.EventoFim) = _programa.EventoDaSemana(r.SemanaReuniao);
            return View(r);
        }

        [HttpDelete]
        public IActionResult Reuniao(string id, bool apagar)
        {
            return _dbContext.ApagarReunioes(new List<string>() { id }) ? StatusCode(200) : StatusCode(500);
        }
    }
}
