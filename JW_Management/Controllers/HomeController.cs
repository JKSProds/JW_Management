using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;

namespace JW_Management.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
           
            return View();
        }
    }
}
