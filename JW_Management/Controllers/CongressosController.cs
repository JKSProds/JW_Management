using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JW_Management.Controllers;

[Authorize(Roles = "Admin, Congresso")]
public class CongressosController(DbContext _dbContext, FileContext _fileContext) : Controller
{
    // GET
    public IActionResult Index()
    {
        return View(_dbContext.ObterCongressos());
    }
    
    public IActionResult Congresso(int id)
    {
        if (id==0) return NotFound();
        
        return View(_dbContext.ObterCongresso(id, true));
    }
    
    public IActionResult Congregacoes(int id)
    {
        if (id==0) return NotFound();
        
        return Ok(_dbContext.ObterLocais(id));
    }
    
    public IActionResult Paragens(int id)
    {
        if (id==0) return NotFound();
        Congresso c = _dbContext.ObterCongresso(id);
        return Ok(_dbContext.ObterParagens(c));
    }
    
    [HttpPost("Congressos/Importar/Rotas/{id}")]
    public IActionResult Rotas(IFormFile file, int id, int idTipo)
    {
        if (id==0 || idTipo == 0) return NotFound();
        Congresso c = _dbContext.ObterCongresso(id);
        var d = _fileContext.ConvertCsv(file);
        
        return Ok(_dbContext.CriarRotas(d, c, idTipo));
    }    
    [HttpPost("Congressos/Importar/Viagens/{id}")]
    public IActionResult Viagens(IFormFile file, int id)
    {
        if (id==0) return NotFound();
        Congresso c = _dbContext.ObterCongresso(id);
        var d = _fileContext.ConvertCsv(file);
        
        return Ok(_dbContext.CriarViagens(d, c));
    }    
    [HttpPost("Congressos/Importar/Paragens/{id}")]
    public IActionResult Paragens(IFormFile file, int id, int idTipo)
    {
        if (id==0 || idTipo == 0) return NotFound();
        Congresso c = _dbContext.ObterCongresso(id);
        var d = _fileContext.ConvertCsv(file);
        
        return Ok(_dbContext.CriarParagens(d, c, idTipo));
    }    
    
    [HttpPost("Congressos/Importar/ViagensParagens/{id}")]
    public IActionResult ViagensParagens(IFormFile file, int id)
    {
        if (id==0) return NotFound();
        Congresso c = _dbContext.ObterCongresso(id);
        var d = _fileContext.ConvertCsv(file);
        
        return Ok(_dbContext.CriarViagensParagens(d, c));
    }
    
    
    [HttpGet("Congressos/{IdIC}/Paragem/{id}")]
    public IActionResult Paragem(string id, int IdIC)
    {
        if (id==String.Empty) return NotFound();
        var p = _dbContext.ObterParagem(id, IdIC, true);
        return View(p);
    }
    
    [HttpGet("Congressos/{IdIC}/Viagem/{id}")]
    public IActionResult Viagem(string id, int IdIC)
    {
        if (id==String.Empty) return NotFound();
        var p = _dbContext.ObterViagem(id, IdIC, true);
        return View(p);
    }
    
    [HttpGet("Congressos/{IdIC}/Congregacao/{id}")]
    public IActionResult Congregacao(int id, int IdIC)
    {
        if (id==0) return NotFound();
        var c = _dbContext.ObterCongresso(IdIC);
        
        var cng = _dbContext.ObterCongregacao(id, c.Id);
        cng.Paragens = _dbContext.ObterParagens(cng, c.Id);
        cng.Recomendacoes = _dbContext.ObterRecomendacoes(cng);
        
        return View(cng);
    }
    
    
    [HttpPost("Congressos/{IdIC}/Congregacao/{id}/Recomendacao")]
    public IActionResult Recomendacao(Recomendacao r, int IdIC, int id)
    {
        if (IdIC==0) return NotFound();
        var c = _dbContext.ObterCongresso(IdIC);
        var cng = _dbContext.ObterCongregacao(id, IdIC);
        cng.Recomendacoes = _dbContext.ObterRecomendacoes(cng);

        r.Congresso = c;
        r.IdCongregacao = cng.Id;
        
        if (r.Linhas == null || r.Linhas.Count == 0)
        {
            r.Linhas.Add(new Recomendacao_Linhas()
            {
                Manual = true,
                Rota = new Rota() {Nome = "Manual"},
                Viagem_Paragem = new Viagem_Paragem() {Viagem = new  Viagem() {Destino = "Manual"}, Paragem = new Paragem() {NomeParagem = "Manual"}},
                TipoTransporte = new TipoTransporte() {Nome = "Manual"},
                Sequencia = 1
            });
        }
        
        foreach (var o in cng.Recomendacoes)
        {
            if (o.Sequencia == r.Sequencia) _dbContext.ApagarRecomendacao(o, c, cng);
        }
        
        r.Id = _dbContext.CriarRecomendacao(r, c, cng);
        foreach (var l in r.Linhas)
        {
           if (l.TipoTransporte.Id > 0) l.TipoTransporte = _dbContext.ObterTiposTransporte().First(t => t.Id == l.TipoTransporte.Id);
            l.Manual = true;
            l.Sequencia = 1;
          l.Id = _dbContext.CriarRecomendacaoLinha(r, l, c, cng);
        }
        
        return RedirectToAction("Congregacao", new { id = cng.Id, IdIC = c.Id });
    }
    
    [HttpDelete("Congressos/{IdIC}/Congregacao/{idC}/Recomendacao/{id}")]
    public IActionResult Recomendacao(string id, int idC, int IdIC)
    {
        var c = _dbContext.ObterCongresso(IdIC);
        var cng = _dbContext.ObterCongregacao(idC, IdIC);
 
        return _dbContext.ApagarRecomendacao(new Recomendacao() {Id = id}, c, cng) ? Ok() : BadRequest();
    }
    
    [HttpGet("Congressos/{IdIC}/Tipo/{id}/Rotas")]
    public IActionResult Rotas(int id, int IdIC)
    {
        if (IdIC==0) return NotFound();
        var c = _dbContext.ObterCongresso(IdIC);
        
        return Ok(_dbContext.ObterRotas(c, _dbContext.ObterTiposTransporte().First(o => o.Id == id)).DistinctBy(o => o.Nome).ToList().OrderBy(o => o.Nome));
    }
    
    [HttpGet("Congressos/{IdIC}/Tipo/{IdTipo}/Rotas/{NomeRota}/Viagens")]
    public IActionResult Viagens(string NomeRota, int IdIC, int IdTipo)
    {
        if (IdIC==0) return NotFound();
        var c = _dbContext.ObterCongresso(IdIC);
        var v = _dbContext.ObterViagens(c).Where(o => o.Rota.Nome == NomeRota && o.Rota.Tipo.Id == IdTipo).DistinctBy(o => o.Destino).ToList().OrderBy(o => o.Destino);
        return Ok(v);
    }
    
    [HttpGet("Congressos/{IdIC}/Tipo/{IdTipo}/Rotas/{NomeRota}/Viagens/{NomeDestino}/Paragens")]
    public IActionResult Paragens(string NomeRota, string NomeDestino, int IdTipo, int IdIC)
    {
        if (IdIC==0) return NotFound();
        var c = _dbContext.ObterCongresso(IdIC);
        var p = _dbContext.ObterParagens(c, NomeRota, NomeDestino, IdTipo).DistinctBy(o => o.NomeParagem).ToList().OrderBy(o => o.NomeParagem);
        return Ok(p);
    }
    
    [HttpGet("Congressos/{IdIC}/Tipo/{IdTipo}/Rotas/{NomeRota}/Viagens/{NomeDestino}/Horarios/{NomeParagem}")]
    public IActionResult Horarios(string NomeRota, string NomeDestino, string NomeParagem, int IdTipo, int IdIC)
    {
        if (IdIC==0) return NotFound();
        var c = _dbContext.ObterCongresso(IdIC);
        var h = _dbContext.ObterViagensParagem(c, NomeRota, NomeDestino, NomeParagem, IdTipo);
        return Ok(h);
    }
}