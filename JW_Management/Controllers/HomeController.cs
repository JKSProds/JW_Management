using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace JW_Management.Controllers
{
    public class HomeController : Controller
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
        public async Task<IActionResult> Login(Publicador p, string ReturnUrl)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;
            List<Publicador> LstUtilizadores = context.ObterPublicadores().Where(u => u.Username == p.Username).ToList();

            foreach (var u in LstUtilizadores)
            {
                var passwordHasher = new PasswordHasher<string>();

                if (passwordHasher.VerifyHashedPassword(null, u.Password!, p.Password!) == PasswordVerificationResult.Success)
                {

                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, u.Id.ToString()),
                            new Claim(ClaimTypes.Role, (u.Id == 49 ? "Master" : u.Id == 41 ? "Territorios" : "")),
                            new Claim(ClaimTypes.GivenName, u.Nome!)
                };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

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

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
