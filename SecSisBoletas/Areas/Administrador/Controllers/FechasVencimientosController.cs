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
    public class FechasVencimientosController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/FechasVencimientos
        public ActionResult Index(string sortOrder, string currentFilter, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            var fechasVencimiento = db.FechaVencimiento.ToList();

            //foreach (var fechaVencimiento in fechasVencimiento)
            //{
            //    if (fechaVencimiento.FechaVto.Month != 1)
            //    {
            //        fechaVencimiento.mesBoleta = fechaVencimiento.FechaVto.Month - 1;
            //        fechaVencimiento.anioBoleta = fechaVencimiento.FechaVto.Year;
            //    }
            //    else
            //    {
            //        fechaVencimiento.mesBoleta = 12;
            //        fechaVencimiento.anioBoleta = fechaVencimiento.FechaVto.Year - 1;
            //    }
            //}

            switch (sortOrder)
            {
                case "name_desc":
                    fechasVencimiento = fechasVencimiento.OrderBy(x => x.FechaVto).ToList();
                    break;
                default:
                    fechasVencimiento = fechasVencimiento.OrderByDescending(x => x.FechaVto).ToList();
                    break;
            }
            int pageSize = 12;
            int pageNumber = (page ?? 1);

            return View(fechasVencimiento.ToPagedList(pageNumber, pageSize));
        }

        // GET: Administrador/FechasVencimientos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaVencimiento fechaVencimiento = db.FechaVencimiento.Find(id);
            if (fechaVencimiento == null)
            {
                return HttpNotFound();
            }

            if (fechaVencimiento.FechaVto.Month != 1)
            {
                fechaVencimiento.mesBoleta = fechaVencimiento.FechaVto.Month - 1;
                fechaVencimiento.anioBoleta = fechaVencimiento.FechaVto.Year;
            }
            else
            {
                fechaVencimiento.mesBoleta = 12;
                fechaVencimiento.anioBoleta = fechaVencimiento.FechaVto.Year - 1;
            }

            return View(fechaVencimiento);
        }

        // GET: Administrador/FechasVencimientos/Create
        public ActionResult Create()
        {
            ViewBag.mesBoleta = DateTime.Today.Month;
            ViewBag.anioBoleta = DateTime.Today.Year;
            return View();
        }

        // POST: Administrador/FechasVencimientos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdFechaVencimiento,FechaVto,mesBoleta,anioBoleta")] FechaVencimiento fechaVencimiento)
        {
            if (ModelState.IsValid)
            {
                db.FechaVencimiento.Add(fechaVencimiento);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.mesBoleta = fechaVencimiento.mesBoleta;
            ViewBag.anioBoleta = fechaVencimiento.anioBoleta;
            return View(fechaVencimiento);
        }

        // GET: Administrador/FechasVencimientos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaVencimiento fechaVencimiento = db.FechaVencimiento.Find(id);
            if (fechaVencimiento == null)
            {
                return HttpNotFound();
            }
            return View(fechaVencimiento);
        }

        // POST: Administrador/FechasVencimientos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdFechaVencimiento,FechaVto,mesBoleta,anioBoleta")] FechaVencimiento fechaVencimiento)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fechaVencimiento).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fechaVencimiento);
        }

        // GET: Administrador/FechasVencimientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FechaVencimiento fechaVencimiento = db.FechaVencimiento.Find(id);
            if (fechaVencimiento == null)
            {
                return HttpNotFound();
            }
            return View(fechaVencimiento);
        }

        // POST: Administrador/FechasVencimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FechaVencimiento fechaVencimiento = db.FechaVencimiento.Find(id);
            db.FechaVencimiento.Remove(fechaVencimiento);
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
