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
    public class LocalidadesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/Localidades
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

            var localidades = db.Localidad.Include(t => t.Provincia);

            if (!string.IsNullOrEmpty(searchString))
            {
                localidades = localidades.Where(x => x.Nombre.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    localidades = localidades.OrderByDescending(x => x.Nombre);
                    break;
                default:
                    localidades = localidades.OrderBy(x => x.Nombre);
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(localidades.ToPagedList(pageNumber, pageSize));
        }

        [AllowAnonymous]
        public JsonResult GetLocalidades(int IdProvincia)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var localidades = db.Localidad.Where(x => x.IdProvincia == IdProvincia).OrderBy(x => x.Nombre);
            foreach (var localidad in localidades)
            {
                localidad.Nombre = localidad.Nombre + " (" + localidad.CodPostal + ")";
            }
            return Json(localidades, JsonRequestBehavior.AllowGet);
        }

        // GET: Administrador/Localidades/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Localidad localidad = db.Localidad.Find(id);
            if (localidad == null)
            {
                return HttpNotFound();
            }
            return View(localidad);
        }

        // GET: Administrador/Localidades/Create
        public ActionResult Create()
        {
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre");
            return View();
        }

        // POST: Administrador/Localidades/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdLocalidad,Nombre,CodPostal,IdProvincia")] Localidad localidad)
        {
            if (ModelState.IsValid)
            {
                db.Localidad.Add(localidad);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            return View(localidad);
        }

        // GET: Administrador/Localidades/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Localidad localidad = db.Localidad.Find(id);
            if (localidad == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            return View(localidad);
        }

        // POST: Administrador/Localidades/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdLocalidad,Nombre,CodPostal,IdProvincia")] Localidad localidad)
        {
            if (ModelState.IsValid)
            {
                db.Entry(localidad).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            return View(localidad);
        }

        // GET: Administrador/Localidades/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Localidad localidad = db.Localidad.Find(id);
            if (localidad == null)
            {
                return HttpNotFound();
            }
            return View(localidad);
        }

        // POST: Administrador/Localidades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Localidad localidad = db.Localidad.Find(id);
            db.Localidad.Remove(localidad);
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
