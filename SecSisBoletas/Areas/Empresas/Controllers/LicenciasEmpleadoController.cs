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
    public class LicenciasEmpleadosController : Controller
    {
        private SecModel db = new SecModel();

        // GET: LicenciasEmpleados
        public ActionResult Index()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int idEmpresa = Convert.ToInt32(claim.Value);

            var licenciaEmpleado = db.LicenciaEmpleado.Include(l => l.EmpleadoEmpresa)
                                                      .Include(l => l.LicenciaLaboral)
                                                      .Where(x => x.EmpleadoEmpresa.idEmpresa == idEmpresa);
            return View(licenciaEmpleado.ToList());
        }

        // GET: LicenciasEmpleados/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LicenciaEmpleado licenciaEmpleado = db.LicenciaEmpleado.Find(id);
            if (licenciaEmpleado == null)
            {
                return HttpNotFound();
            }
            return View(licenciaEmpleado);
        }

        // GET: LicenciasEmpleados/Create
        public ActionResult Create()
        {
            Claim claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var empleadoEmpresa = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa).ToList();
            foreach (var empleado in empleadoEmpresa)
            {
                empleado.Empleado.Nombre = empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleadoEmpresa, "idEmpleadoEmpresa", "Empleado.Nombre");
            ViewBag.IdLicenciaLaboral = new SelectList(db.LicenciaLaboral, "IdLicenciaLaboral", "Descripcion");
            return View();
        }

        // POST: LicenciasEmpleados/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdLicenciaEmpleado,IdLicenciaLaboral,IdEmpleadoEmpresa,FechaAltaLicencia,FechaBajaLicencia")] LicenciaEmpleado licenciaEmpleado)
        {
            Claim claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            var empleadoEmpresa = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa).ToList();
            foreach (var empleado in empleadoEmpresa)
            {
                empleado.Empleado.Nombre = empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleadoEmpresa, "idEmpleadoEmpresa", "Empleado.Nombre", licenciaEmpleado.IdEmpleadoEmpresa);
            ViewBag.IdLicenciaLaboral = new SelectList(db.LicenciaLaboral, "IdLicenciaLaboral", "Descripcion", licenciaEmpleado.IdLicenciaLaboral);
            if (ModelState.IsValid)
            {
                if(licenciaEmpleado.IdLicenciaLaboral == 3 || licenciaEmpleado.IdLicenciaLaboral == 2)
                {
                    if(db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == licenciaEmpleado.IdEmpleadoEmpresa && 
                                                   x.IdLicenciaLaboral == 3 &&
                                                   x.FechaAltaLicencia.Year == licenciaEmpleado.FechaAltaLicencia.Year).FirstOrDefault() != null)
                    {
                        if (licenciaEmpleado.IdLicenciaLaboral == 3)
                        {
                            ModelState.AddModelError("IdLicenciaLaboral", "Solo se Puede Declarar una \"Licencia por Vacaciones\" por año");
                        }
                        else
                        {
                            ModelState.AddModelError("IdLicenciaLaboral", "Solo se Puede Declarar una \"Licencia por Maternidad\" por año");
                        }
                        return View(licenciaEmpleado);
                    }
                }
                db.LicenciaEmpleado.Add(licenciaEmpleado);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(licenciaEmpleado);
        }

        // GET: LicenciasEmpleados/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LicenciaEmpleado licenciaEmpleado = db.LicenciaEmpleado.Find(id);
            if (licenciaEmpleado == null)
            {
                return HttpNotFound();
            }
            Claim claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var empleadoEmpresa = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa).ToList();
            foreach (var empleado in empleadoEmpresa)
            {
                empleado.Empleado.Nombre = empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleadoEmpresa, "idEmpleadoEmpresa", "Empleado.Nombre", licenciaEmpleado.IdEmpleadoEmpresa);
            ViewBag.IdLicenciaLaboral = new SelectList(db.LicenciaLaboral, "IdLicenciaLaboral", "Descripcion", licenciaEmpleado.IdLicenciaLaboral);
            return View(licenciaEmpleado);
        }

        // POST: LicenciasEmpleados/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdLicenciaEmpleado,IdLicenciaLaboral,IdEmpleadoEmpresa,FechaAltaLicencia,FechaBajaLicencia")] LicenciaEmpleado licenciaEmpleado)
        {
            Claim claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var empleadoEmpresa = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa).ToList();
            foreach (var empleado in empleadoEmpresa)
            {
                empleado.Empleado.Nombre = empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleadoEmpresa, "idEmpleadoEmpresa", "Empleado.Nombre", licenciaEmpleado.IdEmpleadoEmpresa);
            ViewBag.IdLicenciaLaboral = new SelectList(db.LicenciaLaboral, "IdLicenciaLaboral", "Descripcion", licenciaEmpleado.IdLicenciaLaboral);
            if (ModelState.IsValid)
            {
                if (licenciaEmpleado.IdLicenciaLaboral == 3)
                {
                    if (db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == licenciaEmpleado.IdEmpleadoEmpresa &&
                                                    x.IdLicenciaLaboral == 3 &&
                                                    x.FechaAltaLicencia.Year == licenciaEmpleado.FechaAltaLicencia.Year &&
                                                    x.IdLicenciaEmpleado != licenciaEmpleado.IdLicenciaEmpleado).FirstOrDefault() != null)
                    {
                        ModelState.AddModelError("IdLicenciaLaboral", "Solo se Puede Declarar una \"Licencia por Vacaciones\" por año");
                        return View(licenciaEmpleado);
                    }
                }
                db.Entry(licenciaEmpleado).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            return View(licenciaEmpleado);
        }

        // GET: LicenciasEmpleados/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LicenciaEmpleado licenciaEmpleado = db.LicenciaEmpleado.Find(id);
            if (licenciaEmpleado == null)
            {
                return HttpNotFound();
            }
            return View(licenciaEmpleado);
        }

        // POST: LicenciasEmpleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LicenciaEmpleado licenciaEmpleado = db.LicenciaEmpleado.Find(id);
            db.LicenciaEmpleado.Remove(licenciaEmpleado);
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
