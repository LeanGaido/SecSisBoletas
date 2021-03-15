using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using DAL;
using DAL.Models;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class EmpresasController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Empresa/Empresas
        public ActionResult Index()
        {
            var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);
            return View(empresa.ToList());
        }

        // GET: Empresa/Empresas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            Empresa empresa = db.Empresa.Find(IdEmpresa); 
            if (empresa == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdActividad = new SelectList(db.Actividad, "IdActividad", "Nombre", empresa.IdActividad);
            ViewBag.IdLocalidad = new SelectList(db.Localidad, "IdLocalidad", "Nombre", empresa.IdLocalidad);
            return View(empresa);
        }

        // GET: Empresa/Empresas/Create
        public ActionResult Create()
        {
            ViewBag.IdActividad = new SelectList(db.Actividad, "IdActividad", "Nombre");
            ViewBag.IdLocalidad = new SelectList(db.Localidad, "IdLocalidad", "Nombre");
            return View();
        }

        // POST: Empresa/Empresas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdEmpresa,Cuit,RazonSocial,NombreFantasia,Calle,Altura,IdLocalidad,TelefonoFijo,TelefonoCelular,Email,IdActividad,FechaAltaEmpresa,FechaBajaEmpresa")] Empresa empresa)
        {
            if (ModelState.IsValid)
            {
                db.Empresa.Add(empresa);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IdActividad = new SelectList(db.Actividad, "IdActividad", "Nombre", empresa.IdActividad);
            ViewBag.IdLocalidad = new SelectList(db.Localidad, "IdLocalidad", "Nombre", empresa.IdLocalidad);
            return View(empresa);
        }

        // GET: Empresa/Empresas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            Empresa empresa = db.Empresa.Find(IdEmpresa);
            if (empresa == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdActividad = new SelectList(db.Actividad, "IdActividad", "Nombre", empresa.IdActividad);
            ViewBag.IdLocalidad = new SelectList(db.Localidad, "IdLocalidad", "Nombre", empresa.IdLocalidad);
            return View(empresa);
        }

        // POST: Empresa/Empresas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdEmpresa,Cuit,RazonSocial,NombreFantasia,Calle,Altura,IdLocalidad,TelefonoFijo,TelefonoCelular,Email,IdActividad,FechaAltaEmpresa,FechaBajaEmpresa")] Empresa empresa)
        {
            if (ModelState.IsValid)
            {
                db.Entry(empresa).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Manage", new { Area = "" });
            }
            ViewBag.IdActividad = new SelectList(db.Actividad, "IdActividad", "Nombre", empresa.IdActividad);
            ViewBag.IdLocalidad = new SelectList(db.Localidad, "IdLocalidad", "Nombre", empresa.IdLocalidad);
            return View(empresa);
        }

        // GET: Empresa/Empresas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empresa empresa = db.Empresa.Find(id);
            if (empresa == null)
            {
                return HttpNotFound();
            }
            return View(empresa);
        }

        // POST: Empresa/Empresas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Empresa empresa = db.Empresa.Find(id);
            db.Empresa.Remove(empresa);
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
