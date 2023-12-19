using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;

namespace JW_Management.Controllers
{
    [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Territorios")]
    public class TerritoriosController : Controller
    {
        [HttpGet]
        public IActionResult Index(string filtro)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            filtro = string.IsNullOrEmpty(filtro) ? "" : filtro;
            ViewData["filtro"] = filtro;
            ViewBag.Publicadores = context.ObterPublicadores().OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });

            return View(context.ObterTerritorios(filtro, true, false, false, false).OrderBy(p => p.Id));
        }

        [HttpGet]
        public IActionResult Territorio(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return View(context.ObterTerritorio(id, true, true, true,true));
        }

        [HttpPost]
        public IActionResult Territorio(Territorio t)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            context.AdicionarTerritorio(t);

            return View(context.ObterTerritorio(t!.Stamp, true, true, true, true));
        }

        [HttpPut]
        public IActionResult Territorio(string id, string nome, int dificuldade, string descricao, string url)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(nome)) return StatusCode(500);

            Territorio t = new Territorio
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Id = id,
                Nome = nome,
                Dificuldade = dificuldade == 1 ? DificuldadeTerritorio.FACIL : dificuldade == 2 ? DificuldadeTerritorio.MODERADO : DificuldadeTerritorio.DIFICIL,
                Descricao = descricao,
                Url = url
            };

            return context.AdicionarTerritorio(t) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Territorio(string stamp, bool apagar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);

            return context.ApagarTerritorio(stamp) ? StatusCode(200) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Movimentos()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View(context.ObterMovimentosTerritorios().OrderByDescending(m => m.Entrada.Stamp));
        }

        [HttpPost]
        public IActionResult Movimento(string id, int idpub, string email, string telemovel, int tipo, DateTime data)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(id) || tipo < 0) return StatusCode(500);

            Territorio t = context.ObterTerritorio(id, false, false, true, false);
            Publicador p = context.ObterPublicador(idpub, false, false, true, false);

            if (p.Email != email && !string.IsNullOrEmpty(email) && p.Id > 0)
            {
                p.Email = email;
                context.AdicionarPublicador(p);
            }
            if (p.Telemovel != telemovel && !string.IsNullOrEmpty(telemovel) && p.Id > 0)
            {
                p.Telemovel = telemovel;
                context.AdicionarPublicador(p);
            }

            MovimentosTerritorio m = new MovimentosTerritorio
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Territorio = t,
                Publicador = p,
                DataMovimento = data,
                Tipo = tipo == 1 ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA
            };

            if (tipo == 1 && !string.IsNullOrEmpty(email)) MailContext.MailTerritorioAtribuido(t, email);
            if (tipo == 2 && !string.IsNullOrEmpty(email)) MailContext.MailTerritorioDevolver(t, email);

            return context.AdicionarMovimentoTerritorio(m) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Movimentos(string stampentrada, string stampsaida)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            if (string.IsNullOrEmpty(stampentrada)) return StatusCode(500);
            if (string.IsNullOrEmpty(stampsaida)) stampsaida = "";

            return context.ApagarMovimentoTerritorio(stampentrada) ? context.ApagarMovimentoTerritorio(stampsaida) ? StatusCode(200) : StatusCode(500) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Anexo(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext f = new();
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            AnexosTerritorio a = context.ObterAnexo(id);
            byte[] file = f.ObterFicheiro(a.CaminhoFicheiro + a.NomeFicheiro);
            if (file == null) return Forbid();

            if (new FileExtensionContentTypeProvider().TryGetContentType(a.Extensao!, out var mimeType)) return File(file, mimeType);
            return File(file, "application/octet-stream", a.Extensao);
        }

        [HttpPost]
        public IActionResult Anexo(string id, IFormFile file)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext f = new();
            if (string.IsNullOrEmpty(id) || file == null) return StatusCode(500);

            Territorio t = context.ObterTerritorio(id, false, false, false, false);

            AnexosTerritorio a = new AnexosTerritorio()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                NomeFicheiro = "T_" + t.Id.ToUpper() + "_" + DateTime.Now.Ticks.ToString() + "." + file.FileName.Split(".").Last(),
                CaminhoFicheiro = f.ObterCaminhoTerritorios(),
                Extensao = "." + file.FileName.Split(".").Last(),
                Descricao = "Anexo de Território (" + t.Nome + ")",
                Territorio = t
            };

            if (f.GuardarAnexoTerritorio(file, a.NomeFicheiro)) return context.AdicionarAnexoTerritorio(a) ? StatusCode(200) : StatusCode(500);

            return StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Anexo(string id, bool apgar)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext f = new();
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            AnexosTerritorio a = context.ObterAnexo(id);
            return f.ApagarFicheiro(a.CaminhoFicheiro + a.NomeFicheiro) ? (context.ApagarAnexoTerritorio(a.Stamp!) ? StatusCode(200) : StatusCode(500)) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Formulario()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            FileContext fileContext = new FileContext();

            var file = fileContext.PreencherFormularioS13(context!).ToArray();
            var output = new MemoryStream();
            output.Write(file, 0, file.Length);
            output.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "S-13_TPO_Preenchido.pdf",
                Inline = true,
                Size = file.Length,
                CreationDate = DateTime.Now,

            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            return new FileContentResult(output.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
    }
}
