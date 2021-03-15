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

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    [Authorize(Roles = "Admin, Fiscalizacion")]
    public class LicenciasLaboralesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/LicenciasLaborales
        public ActionResult Index()
        {
            return View(db.LicenciaLaboral.ToList());
        }

        // GET: Administrador/LicenciasLaborales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LicenciaLaboral licenciaLaboral = db.LicenciaLaboral.Find(id);
            if (licenciaLaboral == null)
            {
                return HttpNotFound();
            }
            return View(licenciaLaboral);
        }

        // GET: Administrador/LicenciasLaborales/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrador/LicenciasLaborales/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdLicenciaLaboral,Descripcion")] LicenciaLaboral licenciaLaboral)
        {
            if (ModelState.IsValid)
            {
                db.LicenciaLaboral.Add(licenciaLaboral);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(licenciaLaboral);
        }

        // GET: Administrador/LicenciasLaborales/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LicenciaLaboral licenciaLaboral = db.LicenciaLaboral.Find(id);
            if (licenciaLaboral == null)
            {
                return HttpNotFound();
            }
            return View(licenciaLaboral);
        }

        // POST: Administrador/LicenciasLaborales/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdLicenciaLaboral,Descripcion")] LicenciaLaboral licenciaLaboral)
        {
            if (ModelState.IsValid)
            {
                db.Entry(licenciaLaboral).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(licenciaLaboral);
        }

        // GET: Administrador/LicenciasLaborales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LicenciaLaboral licenciaLaboral = db.LicenciaLaboral.Find(id);
            if (licenciaLaboral == null)
            {
                return HttpNotFound();
            }
            return View(licenciaLaboral);
        }

        // POST: Administrador/LicenciasLaborales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LicenciaLaboral licenciaLaboral = db.LicenciaLaboral.Find(id);
            db.LicenciaLaboral.Remove(licenciaLaboral);
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
