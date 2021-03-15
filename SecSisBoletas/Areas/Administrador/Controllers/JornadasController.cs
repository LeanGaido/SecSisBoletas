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
    public class JornadasController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/Jornadas
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null), "IdEmpresa", "RazonSocial");

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var jornadas = from oJornada in db.Jornada
                              select oJornada;

            if (!string.IsNullOrEmpty(searchString))
            {
                jornadas = jornadas.Where(x => x.Descripcion.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    jornadas = jornadas.OrderByDescending(x => x.Descripcion);
                    break;
                default:
                    jornadas = jornadas.OrderBy(x => x.Descripcion);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(jornadas.ToPagedList(pageNumber, pageSize));
        }

        // GET: Administrador/Jornadas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jornada jornada = db.Jornada.Find(id);
            if (jornada == null)
            {
                return HttpNotFound();
            }
            return View(jornada);
        }

        // GET: Administrador/Jornadas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrador/Jornadas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdJornada,Descripcion")] Jornada jornada)
        {
            if (ModelState.IsValid)
            {
                db.Jornada.Add(jornada);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jornada);
        }

        // GET: Administrador/Jornadas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jornada jornada = db.Jornada.Find(id);
            if (jornada == null)
            {
                return HttpNotFound();
            }
            return View(jornada);
        }

        // POST: Administrador/Jornadas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdJornada,Descripcion")] Jornada jornada)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jornada).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jornada);
        }

        // GET: Administrador/Jornadas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Jornada jornada = db.Jornada.Find(id);
            if (jornada == null)
            {
                return HttpNotFound();
            }
            return View(jornada);
        }

        // POST: Administrador/Jornadas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Jornada jornada = db.Jornada.Find(id);
            db.Jornada.Remove(jornada);
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
