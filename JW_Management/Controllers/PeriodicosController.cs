using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    public class PeriodicosController : Controller
    {
        public IActionResult Index()
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View("Index", context.ObterPeriodicos());
        }

        public IActionResult Periodicos(string id)
        {
            DbContext context = HttpContext.RequestServices.GetService(typeof(DbContext)) as DbContext;

            return View("Saidas", context.ObterPeriodicos(id));
        }

    }
}
