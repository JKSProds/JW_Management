using JW_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JW_Management.Controllers;

[Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
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
}