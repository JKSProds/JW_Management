using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Pqc.Crypto.Falcon;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
    public class ReunioesController : Controller
    {
        public IActionResult Index(string filtro, int tipo)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterReunioes(false));
        }

        [HttpPost]
        public IActionResult Reunioes(IFormFile file)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext f = new FileContext();

            List<Designacao> LstD = f.ImportarDesignacoes(file, context!);

            return context.ApagarReunioes(LstD.Select(d => d.SemanaReuniao.ToString()).Distinct().ToList()) ? context.AdicionarDesignacoes(LstD) ? StatusCode(200): StatusCode(500) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Reuniao(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            ViewBag.Publicadores = context.ObterPublicadores().OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Nome });
            ViewBag.Tipos = context.ObterTiposDesignacao().OrderBy(p => p.Id).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Descricao });

            return View(context.ObterReuniao(id, true));
        }
        [HttpPost]
        public IActionResult Reuniao(DateTime data)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            string semana = "Semana " + ((data.DayOfYear - 1) / 7 + 1) + " de " + data.Year.ToString();

            context.AdicionarReuniao(semana);
            
            ViewBag.Publicadores = context.ObterPublicadores().OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Nome });
            return View(context.ObterReuniao(semana, true));
        }
        [HttpDelete]
        public IActionResult Reuniao(string id, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return context.ApagarReunioes(new List<string>() {id}) ? StatusCode(200) : StatusCode(500);
        }
        [HttpPost]
        public IActionResult Designacao(string id, int tipo, string local)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            TipoDesignacao t = context.ObterTiposDesignacao().Where(t => t.Id == tipo).DefaultIfEmpty(new TipoDesignacao()).First();

            Designacao d = new Designacao() {
                Stamp = DateTime.Now.Ticks.ToString(),
                SemanaReuniao = id, 
                NomePublicador = "",
                NomeDesignacao = t.Descricao,
                Local = local,
                Publicador = context.ObterPublicador(0, false, false, false, false),
                TipoDesignacao = t,
            };

            return context.AdicionarDesignacao(d) ? StatusCode(200) : StatusCode(500);
        }

        [HttpPut]
        public IActionResult Designacao(string id, int pub)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return context.AtualizarDesignacao(id, context.ObterPublicador(pub, false, false, false, false)) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Designacao(string id, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            return context.ApagarDesignacoes(id.Split(",").ToList()) ? StatusCode(200) : StatusCode(500);
        }

        [HttpPost]
        public IActionResult Publicador(string id, int pub)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            Designacao d = context.ObterDesignacao(id);
            Publicador p = context.ObterPublicador(pub, false, false, false, false);

            d.Stamp = DateTime.Now.Ticks.ToString();
            d.Publicador = p;
            d.NomePublicador = p.Nome;
            return context.AdicionarDesignacao(d) ? StatusCode(200) : StatusCode(500);
        }


        [HttpGet]
        public IActionResult Formulario(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext fileContext = new FileContext();

            var file = fileContext.PreencherFormularioS140(context.ObterReuniao(id, true)).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "S-140_TPO_Preenchido.pdf",
                Inline = true,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
    }
}
