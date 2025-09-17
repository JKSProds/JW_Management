using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;

namespace JW_Management.Controllers
{
    //[Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Territorios")]
    [Authorize(Roles = "Admin")]
    public class TerritoriosController(DbContext _dbContext, FileContext _fileContext, MailContext _mailContext) : Controller
    {
        [HttpGet]
        public IActionResult Index(string filtro)
        {
            filtro = string.IsNullOrEmpty(filtro) ? "" : filtro;
            ViewData["filtro"] = filtro;
            ViewBag.Publicadores = _dbContext.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome });

            return View(_dbContext.ObterTerritorios(filtro, true, false, false, false).OrderBy(p => p.Id));
        }

        [HttpGet]
        public IActionResult Territorio(string id)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            return View(_dbContext.ObterTerritorio(id, true, true, true, true));
        }

        [HttpPost]
        public IActionResult Territorio(Territorio t)
        {
            _dbContext.AdicionarTerritorio(t);

            return View(_dbContext.ObterTerritorio(t!.Stamp, true, true, true, true));
        }

        [HttpPut]
        public IActionResult Territorio(string id, string nome, int dificuldade, string descricao, string url)
        {
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

            return _dbContext.AdicionarTerritorio(t) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Territorio(string stamp, bool apagar)
        {
            if (string.IsNullOrEmpty(stamp)) return StatusCode(500);

            return _dbContext.ApagarTerritorio(stamp) ? StatusCode(200) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Registros()
        {
           return View(_dbContext.ObterMovimentosTerritorios().OrderByDescending(m => m.Entrada.Stamp));
        }

        [HttpPost]
        public IActionResult Movimento(string id, int idpub, string email, string telemovel, int tipo, DateTime data)
        {
            if (string.IsNullOrEmpty(id) || tipo < 0) return StatusCode(500);

            Territorio t = _dbContext.ObterTerritorio(id, false, false, true, false);
            Publicador p = _dbContext.ObterPublicador(idpub, false, false, true, false);

            if (p.Email != email && !string.IsNullOrEmpty(email) && p.Id > 0)
            {
                p.Email = email;
                _dbContext.AdicionarPublicador(p);
            }
            if (p.Telemovel != telemovel && !string.IsNullOrEmpty(telemovel) && p.Id > 0)
            {
                p.Telemovel = telemovel;
                _dbContext.AdicionarPublicador(p);
            }

            MovimentosTerritorio m = new MovimentosTerritorio
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                Territorio = t,
                Publicador = p,
                DataMovimento = data,
                Tipo = tipo == 1 ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA
            };

            if (tipo == 1 && !string.IsNullOrEmpty(email)) _mailContext.MailTerritorioAtribuido(t, email);
            if (tipo == 2 && !string.IsNullOrEmpty(email)) _mailContext.MailTerritorioDevolver(t, email);

            return _dbContext.AdicionarMovimentoTerritorio(m) ? StatusCode(200) : StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Registros(string stampentrada, string stampsaida)
        {
            if (string.IsNullOrEmpty(stampentrada)) return StatusCode(500);
            if (string.IsNullOrEmpty(stampsaida)) stampsaida = "";

            return _dbContext.ApagarMovimentoTerritorio(stampentrada) ? _dbContext.ApagarMovimentoTerritorio(stampsaida) ? StatusCode(200) : StatusCode(500) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Anexo(string id)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            AnexosTerritorio a = _dbContext.ObterAnexo(id);
            byte[] file = _fileContext.ObterFicheiro(a.CaminhoFicheiro + a.NomeFicheiro);
            if (file == null) return Forbid();

            if (new FileExtensionContentTypeProvider().TryGetContentType(a.Extensao!, out var mimeType)) return File(file, mimeType);
            return File(file, "application/octet-stream", a.Extensao);
        }

        [HttpPost]
        public IActionResult Anexo(string id, IFormFile file)
        {
            if (string.IsNullOrEmpty(id) || file == null) return StatusCode(500);

            Territorio t = _dbContext.ObterTerritorio(id, false, false, false, false);

            AnexosTerritorio a = new AnexosTerritorio()
            {
                Stamp = DateTime.Now.Ticks.ToString(),
                NomeFicheiro = "T_" + t.Id.ToUpper() + "_" + DateTime.Now.Ticks.ToString() + "." + file.FileName.Split(".").Last(),
                CaminhoFicheiro = _fileContext.ObterCaminhoTerritorios(),
                Extensao = "." + file.FileName.Split(".").Last(),
                Descricao = "Anexo de Território (" + t.Nome + ")",
                Territorio = t
            };

            if (_fileContext.GuardarAnexoTerritorio(file, a.NomeFicheiro)) return _dbContext.AdicionarAnexoTerritorio(a) ? StatusCode(200) : StatusCode(500);

            return StatusCode(500);
        }

        [HttpDelete]
        public IActionResult Anexo(string id, bool apgar)
        {
            if (string.IsNullOrEmpty(id)) return StatusCode(500);

            AnexosTerritorio a = _dbContext.ObterAnexo(id);
            return _fileContext.ApagarFicheiro(a.CaminhoFicheiro + a.NomeFicheiro) ? (_dbContext.ApagarAnexoTerritorio(a.Stamp!) ? StatusCode(200) : StatusCode(500)) : StatusCode(500);
        }

        [HttpGet]
        public IActionResult Formulario()
        {
            var t = _dbContext.ObterTerritorios("", false, false, false, true);
            if (!t.Any()) return RedirectToAction("Index");
            
            var file = _fileContext.PreencherFormularioS13(_dbContext!, t, DateTime.Now.AddDays(-365), DateTime.Now.AddDays(360)).ToArray();
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
