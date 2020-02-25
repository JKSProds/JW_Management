using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JW_Management.Controllers
{
    public class DiscursosController : Controller
    {
        // GET: Discursos
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Calendario()
        {
            return View();
        }


        public JsonResult ObterDiscursos()
        {
            JW_ManagementContext context = HttpContext.RequestServices.GetService(typeof(JW_ManagementContext)) as JW_ManagementContext;
            return new JsonResult(context.ConverterDiscursosEventos(context.ObterListaDiscursos()).ToList());

        }


        
        // GET: Discursos/Create
        public ActionResult Novo(DateTime Data)
        {
            JW_ManagementContext context = HttpContext.RequestServices.GetService(typeof(JW_ManagementContext)) as JW_ManagementContext;

            ViewBag.Temas = context.ObterListaTemas().Select(c => new SelectListItem()
            { Text = "N. " + c.Id.ToString().PadLeft(3, '0') + " - " + c.Tema, Value = c.Id.ToString() }).ToList();

            Discurso discurso = new Discurso();

            discurso.DataDiscurso = Data != new DateTime() ? Data : DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy hh:mm"));

            return View(discurso);
        }

        // POST: Discursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Novo(Discurso discurso)
        {
            try
            {
                JW_ManagementContext context = HttpContext.RequestServices.GetService(typeof(JW_ManagementContext)) as JW_ManagementContext;

                context.CriarDiscurso(discurso);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Discursos/Edit/5
        public ActionResult Editar(int id)
        {
            JW_ManagementContext context = HttpContext.RequestServices.GetService(typeof(JW_ManagementContext)) as JW_ManagementContext;
            
            ViewBag.Temas = context.ObterListaTemas().Select(c => new SelectListItem()
            { Text = "#" + c.Id.ToString().PadLeft(3, '0') + " - " + c.Tema, Value = c.Id.ToString() }).ToList();

            return View(context.ObterDiscurso(id));
        }

        // POST: Discursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, Discurso discurso)
        {
            try
            {
                JW_ManagementContext context = HttpContext.RequestServices.GetService(typeof(JW_ManagementContext)) as JW_ManagementContext;

                context.AtualizarDiscurso(discurso, id);

                return RedirectToAction(nameof(Editar), id);
            }
            catch
            {
                return View();
            }
        }


        public ActionResult Apagar(int id)
        {
            try
            {
                JW_ManagementContext context = HttpContext.RequestServices.GetService(typeof(JW_ManagementContext)) as JW_ManagementContext;
                context.ApagarDiscurso(id);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}