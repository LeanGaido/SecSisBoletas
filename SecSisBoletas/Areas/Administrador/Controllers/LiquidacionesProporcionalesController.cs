using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DAL;
using DAL.Models;
using PagedList;

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    [Authorize(Roles = "Admin, Fiscalizacion")]
    public class LiquidacionesProporcionalesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/LiquidacionesProporcionales
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var liquidaciones = from oLiquidacionProporcional in db.LiquidacionProporcional
                                select oLiquidacionProporcional;

            if (!string.IsNullOrEmpty(searchString))
            {
                liquidaciones = liquidaciones.Where(x => x.Descripcion.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    liquidaciones = liquidaciones.OrderByDescending(x => x.Descripcion);
                    break;
                default:
                    liquidaciones = liquidaciones.OrderBy(x => x.Descripcion);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(liquidaciones.ToPagedList(pageNumber, pageSize));
        }

        // GET: Administrador/LiquidacionesProporcionales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LiquidacionProporcional liquidacionProporcional = db.LiquidacionProporcional.Find(id);
            if (liquidacionProporcional == null)
            {
                return HttpNotFound();
            }
            return View(liquidacionProporcional);
        }

        // GET: Administrador/LiquidacionesProporcionales/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrador/LiquidacionesProporcionales/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdLiquidacionProporcional,Descripcion")] LiquidacionProporcional liquidacionProporcional)
        {
            if (ModelState.IsValid)
            {
                db.LiquidacionProporcional.Add(liquidacionProporcional);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(liquidacionProporcional);
        }

        // GET: Administrador/LiquidacionesProporcionales/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LiquidacionProporcional liquidacionProporcional = db.LiquidacionProporcional.Find(id);
            if (liquidacionProporcional == null)
            {
                return HttpNotFound();
            }
            return View(liquidacionProporcional);
        }

        // POST: Administrador/LiquidacionesProporcionales/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdLiquidacionProporcional,Descripcion")] LiquidacionProporcional liquidacionProporcional)
        {
            if (ModelState.IsValid)
            {
                db.Entry(liquidacionProporcional).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(liquidacionProporcional);
        }

        // GET: Administrador/LiquidacionesProporcionales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LiquidacionProporcional liquidacionProporcional = db.LiquidacionProporcional.Find(id);
            if (liquidacionProporcional == null)
            {
                return HttpNotFound();
            }
            return View(liquidacionProporcional);
        }

        // POST: Administrador/LiquidacionesProporcionales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LiquidacionProporcional liquidacionProporcional = db.LiquidacionProporcional.Find(id);
            db.LiquidacionProporcional.Remove(liquidacionProporcional);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
