using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using JW_Management.Models;

namespace JW_Management.Controllers
{
    public class DiscursosController : Controller
    {
        // GET: Discursos
        public ActionResult Index()
        {
            JW_ManagementContext context = HttpContext.RequestServices.GetService(typeof(JW_ManagementContext)) as JW_ManagementContext;

            return View(context.ObterListaDiscursos());
        }

        // GET: Discursos/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Discursos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Discursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Discursos/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Discursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Discursos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Discursos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}