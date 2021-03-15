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

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class LocalidadesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Localidades
        public ActionResult Index()
        {
            var localidad = db.Localidad.Include(l => l.Provincia).OrderBy(x => x.Nombre);
            return View(localidad.ToList());
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

        [AllowAnonymous]
        public JsonResult GetCodPostal(int IdLocalidad)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var localidad = db.Localidad.Where(x => x.IdLocalidad == IdLocalidad).FirstOrDefault();
            return Json(localidad.CodPostal, JsonRequestBehavior.AllowGet);
        }

        // GET: Localidades/Details/5
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

        // GET: Localidades/Create
        public ActionResult Create()
        {
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre");
            return View();
        }

        // POST: Localidades/Create
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

        // GET: Localidades/Edit/5
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

        // POST: Localidades/Edit/5
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

        // GET: Localidades/Delete/5
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

        // POST: Localidades/Delete/5
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
