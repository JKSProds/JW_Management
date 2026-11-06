using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace JW_Management.Controllers
{

    public class PublicadoresController(DbContext _dbContext, MailContext _mailContext, HourglassSincronizador _sync, HourglassContext _hg, FileContext _file) : Controller
    {
        private bool OutroAdmin(int id) => _dbContext.ObterPublicadores(false).Any(x => x.Id != id && x.Id > 0 && x.TipoUtilizador == 0);
        private static readonly int[] Funcoes = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 99 };

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpGet]
        public IActionResult Index()
        {
            return View(_dbContext.ObterPublicadores(true).OrderBy(p => p.Nome));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpGet]
        public IActionResult Publicador(int id)
        {
            List<Grupo> LstGrupos = _dbContext.ObterGrupos();
            //LstGrupos.Insert(0, new Grupo() { Id = 0, Nome = "N/D", Responsavel = new Publicador() { Nome = "N/D" } });
            ViewBag.Grupos = LstGrupos.Select(l => new SelectListItem() { Value = l.Id.ToString(), Text = l.Nome + " (" + l.Responsavel.Nome + ")" });

            ViewBag.Funcoes = Funcoes.Select(t => new SelectListItem(new Publicador { TipoUtilizador = t }.Role, t.ToString()));
            return View(_dbContext.ObterPublicador(id, true, true, true, false));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura, Territorios")]
        public IActionResult ObterPublicador(int id)
        {
            return Json(_dbContext.ObterPublicador(id, false, false, false, false));
        }


        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico, Literatura, Territorios")]
        [HttpGet]
        public IActionResult ObterPublicadores()
        {
            return Json(_dbContext.ObterPublicadores(false));
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpPost]
        public IActionResult Publicador(Publicador p)
        {
            if (p.Id == 0) return StatusCode(500);
            if (!string.IsNullOrEmpty(p.Nome) && string.IsNullOrEmpty(p.Username)) p.Username = p.Nome.Replace(" ", "").ToLower();

            Publicador pOriginal = _dbContext.ObterPublicador(p.Id, false, false, false, false);

            var passwordHasher = new PasswordHasher<string>();
            if (pOriginal.Password != p.Password && !string.IsNullOrEmpty(p.Password))
            {
                p.Password = passwordHasher.HashPassword(p.Username!, p.Password);
            }
            else
            {
                p.Password = pOriginal.Password;
            }

            // A função (permissão) só muda se quem grava for Admin e o valor for uma das funções existentes.
            if (User.IsInRole("Admin") && int.TryParse(Request.Form["TipoUtilizador"], out var tipo) && Funcoes.Contains(tipo) && tipo != pOriginal.TipoUtilizador)
            {
                // Um admin só retira a função de admin a si próprio, e só se existir outro admin.
                if (pOriginal.TipoUtilizador == 0 && p.Id != User.ObterId()) return BadRequest("Um administrador só pode retirar a sua própria função de administrador.");
                if (pOriginal.TipoUtilizador == 0 && !OutroAdmin(p.Id)) return BadRequest("Tem de existir outro administrador antes de deixar de o ser.");
                _dbContext.AtualizarTipoUtilizador(p.Id, tipo);
            }

            return _dbContext.AdicionarPublicador(p) ? RedirectToAction("Publicador", new { id = p.Id }) : StatusCode(500);
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpDelete]
        public IActionResult Publicador(int id, bool delete)
        {
            if (!delete) return StatusCode(403);
            var p = _dbContext.ObterPublicador(id, false, false, false, false);
            var u = _dbContext.ObterPublicador(User.ObterId(), false, false, false, false);
            if (p.TipoUtilizador < u.TipoUtilizador) return Forbid();
            // Mesma regra: um admin só se apaga a si próprio e só havendo outro admin.
            if (p.TipoUtilizador == 0 && (p.Id != u.Id || !OutroAdmin(p.Id))) return Forbid();

            return _dbContext.ApagarPublicador(id) ? StatusCode(200) : StatusCode(500);
        }

        // ---------- Grupos (só Admin) ----------
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Grupos()
        {
            ViewBag.Publicadores = _dbContext.ObterPublicadores(false).Where(p => p.Id > 0).OrderBy(p => p.Nome).ToList();
            return View(_dbContext.ObterGrupos().Where(g => g.Id > 0).OrderBy(g => g.Nome));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult GrupoGuardar(int id, string? nome, int idResponsavel, int idAjudante)
        {
            if (string.IsNullOrWhiteSpace(nome)) return BadRequest("O grupo precisa de um nome.");
            return _dbContext.GuardarGrupo(id, nome, idResponsavel, idAjudante) ? RedirectToAction("Grupos") : StatusCode(500);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult GrupoApagar(int id) => id > 0 && _dbContext.ApagarGrupo(id) ? RedirectToAction("Grupos") : StatusCode(500);

        // ---------- Fotografia do utilizador (só a própria) ----------
        [Authorize]
        [HttpGet]
        public IActionResult Foto(int id)
        {
            var caminho = _file.ObterCaminhoPublicadores() + id + ".jpg";
            if (!System.IO.File.Exists(caminho)) return NotFound();
            Response.Headers.CacheControl = "private, no-cache"; // revalida sempre (304) para a troca de foto aparecer logo
            return PhysicalFile(caminho, "image/jpeg", new DateTimeOffset(System.IO.File.GetLastWriteTimeUtc(caminho)), null);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FotoGuardar(IFormFile? ficheiro)
        {
            // cabeçalho que um formulário de outro site não consegue enviar
            if (Request.Headers["X-Requested-With"] != "fetch") return BadRequest();
            var id = User.ObterId();
            if (id <= 0 || ficheiro == null || ficheiro.Length is 0 or > 1_000_000) return BadRequest("Imagem inválida ou demasiado grande.");
            using var ms = new MemoryStream(); await ficheiro.CopyToAsync(ms);
            var b = ms.ToArray();
            if (b.Length < 3 || b[0] != 0xFF || b[1] != 0xD8 || b[2] != 0xFF) return BadRequest("A imagem tem de ser JPEG."); // o browser converte antes de enviar
            await System.IO.File.WriteAllBytesAsync(_file.ObterCaminhoPublicadores() + id + ".jpg", b);
            return Ok();
        }

        // Sincronização com o Hourglass: só leitura no Hourglass; a sessão fica só em memória durante algumas horas.
        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpPost]
        public async Task<IActionResult> HourglassPreview(string cookie)
        {
            cookie = _hg.Resolver(User.ObterTenant(), cookie)!;
            if (cookie == null) return BadRequest("Sem sessão do Hourglass: cole o pedido copiado (falta «hglogin» ou o token XSRF).");
            try { return Json(await _sync.Pre(cookie)); }
            catch (UnauthorizedAccessException e) { _hg.Terminar(User.ObterTenant()); return Unauthorized(e.Message); }
            catch (HttpRequestException) { return StatusCode(502, "Não foi possível obter os dados do Hourglass."); }
        }

        [Authorize(Roles = "Admin, Assistente, Coordenador, Secretario, Servico")]
        [HttpPost]
        public IActionResult HourglassAplicar([FromBody] SyncPreview dados)
        {
            var (atualizados, criados) = _sync.Aplicar(dados);
            return Json(new { atualizados, criados });
        }

        public IActionResult Senha(int id)
        {
            string s = "";
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!#$%&/()=?-_,.;:|^~";

            using var rng = RandomNumberGenerator.Create();
            while (s.Length != 12)
            {
                byte[] oneByte = new byte[1];
                rng.GetBytes(oneByte);
                char character = (char)oneByte[0];
                if (valid.Contains(character))
                {
                    s += character;
                }
            }
            
            var p = _dbContext.ObterPublicador(id, false, false, true, false);
            p.Password = s;
            _mailContext.MailSenhaPublicador(p);
            
            return Publicador(p);
        }
    }
}
