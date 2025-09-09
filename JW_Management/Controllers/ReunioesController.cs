using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Pqc.Crypto.Falcon;

namespace JW_Management.Controllers
{
    //[Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
    [Authorize(Roles = "Admin")]
    public class ReunioesController(DbContext _dbContext, FileContext _fileContext) : Controller
    {
        public IActionResult Index(string filtro, int tipo)
        {
            return View(_dbContext.ObterReunioes(false));
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

            _dbContext.AdicionarReuniao(semana.ToUpper(), stamp);

            ViewBag.Publicadores = _dbContext.ObterPublicadores(false).OrderBy(p => p.Nome).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Nome });
            ViewBag.Tipos = _dbContext.ObterTiposDesignacao().OrderBy(p => p.Id).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Descricao });
            ViewBag.Canticos = _dbContext.ObterCanticos().OrderBy(p => p.Id).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Id + " - " + l.Nome });
            ViewBag.Grupos = _dbContext.ObterGrupos().OrderBy(p => p.Id).Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Id == 0 ? "Não Definido" : l.Nome });

            return View(_dbContext.ObterReuniao(stamp, true, true));
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
            d.NomeDesignacao = tema;

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

        [HttpGet]
        public IActionResult Formulario(string id)
        {
            var file = _fileContext.PreencherFormularioS140(_dbContext.ObterReuniao(id, true, true)).ToArray();
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
