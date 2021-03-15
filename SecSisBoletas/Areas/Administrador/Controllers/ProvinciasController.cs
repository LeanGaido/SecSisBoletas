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
    public class ProvinciasController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/Provincias
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

            var provincias = from oProvincia in db.Provincia
                             select oProvincia;

            if (!string.IsNullOrEmpty(searchString))
            {
                provincias = provincias.Where(x => x.Nombre.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    provincias = provincias.OrderByDescending(x => x.Nombre);
                    break;
                default:
                    provincias = provincias.OrderBy(x => x.Nombre);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(provincias.ToPagedList(pageNumber, pageSize));
        }

        // GET: Administrador/Provincias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Provincia provincia = db.Provincia.Find(id);
            if (provincia == null)
            {
                return HttpNotFound();
            }
            return View(provincia);
        }

        // GET: Administrador/Provincias/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrador/Provincias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdProvincia,Nombre")] Provincia provincia)
        {
            if (ModelState.IsValid)
            {
                db.Provincia.Add(provincia);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(provincia);
        }

        // GET: Administrador/Provincias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Provincia provincia = db.Provincia.Find(id);
            if (provincia == null)
            {
                return HttpNotFound();
            }
            return View(provincia);
        }

        // POST: Administrador/Provincias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdProvincia,Nombre")] Provincia provincia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(provincia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(provincia);
        }

        // GET: Administrador/Provincias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Provincia provincia = db.Provincia.Find(id);
            if (provincia == null)
            {
                return HttpNotFound();
            }
            return View(provincia);
        }

        // POST: Administrador/Provincias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Provincia provincia = db.Provincia.Find(id);
            db.Provincia.Remove(provincia);
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
