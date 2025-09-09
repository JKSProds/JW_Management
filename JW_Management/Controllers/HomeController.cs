using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using AppContext = System.AppContext;

namespace JW_Management.Controllers
{
    public class HomeController(JW_Management.Models.AppContext _appContext) : Controller
    {
        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login(string ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Publicador p, int t, string ReturnUrl)
        {
            _appContext._manualTenant = t;
            p.Username = p.Username.ToLower().Trim();
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            List<Publicador> LstUtilizadores = context.ObterPublicadores(t, false).Where(u => u.Username == p.Username).ToList();
            
            foreach (var u in LstUtilizadores)
            {
                var passwordHasher = new PasswordHasher<string>();

                if (passwordHasher.VerifyHashedPassword(null, u.Password!, p.Password!) == PasswordVerificationResult.Success)
                {

                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, u.Id.ToString()),
                            new Claim(ClaimTypes.Role, u.Role),
                            new Claim(ClaimTypes.GivenName, u.Nome!),
                            new Claim(ClaimTypes.Sid, _appContext._currentTenant.Id.ToString()),
                            new Claim(ClaimTypes.PrimaryGroupSid, _appContext._currentTenant.NomeCongregacao.ToString())
                };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    _appContext._manualTenant = 0;
                    
                    if (ReturnUrl != "" && ReturnUrl != null)
                    {
                        Response.Redirect(ReturnUrl, true);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Password errada!");
                }
            }
            ModelState.AddModelError("", "Utilizador não encontrado!");
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
        
        [HttpGet]
        public JsonResult Tenants()
        {
            return Json(_appContext._tenantContext._tenants.Select(t => new Tenant() {Id = t.Id, Nome = t.Nome}));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
