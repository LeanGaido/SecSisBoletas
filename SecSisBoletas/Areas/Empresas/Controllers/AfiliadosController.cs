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
    public class AfiliadosController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Afiliados
        public ActionResult Index()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            var empleadosEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.FechaBaja == null).ToList();
            List<Afiliado> afiliados = new List<Afiliado>();

            foreach (var empleado in empleadosEmpresa)
            {
                if (empleado.EsAfiliado)
                {
                    var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleado.idEmpleadoEmpresa && x.FechaBaja == null).FirstOrDefault();
                    if (afiliado != null)
                    {
                        afiliados.Add(afiliado);
                    }
                }
            }

            return View(afiliados.ToList());

        }

        public ActionResult IndexBajas()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            var empleadosEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.FechaBaja == null).ToList();
            List<Afiliado> afiliados = new List<Afiliado>();

            foreach (var empleado in empleadosEmpresa)
            {
                var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleado.idEmpleadoEmpresa && x.FechaBaja != null).FirstOrDefault();
                if (afiliado != null)
                {
                    afiliados.Add(afiliado);
                }
            }

            return View(afiliados.ToList());

        }

        // GET: Afiliados/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Afiliado afiliado = db.Afiliado.Find(id);
            if (afiliado == null)
            {
                return HttpNotFound();
            }
            return View(afiliado);
        }

        //// GET: Afiliados/Create
        //public ActionResult Create(Empleado empleado = null)
        //{
        //    if (empleado.IdEmpleado != 0) ViewBag.IdEmpleado = new SelectList(db.Empleado.Where(x => x.IdEmpleado == empleado.IdEmpleado), "IdEmpleado", "Nombre", empleado.IdEmpleado);
        //    else
        //    {
        //        var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
        //        int IdEmpresa = Convert.ToInt32(claim.Value);
        //        var empleados = /*db.Afiliado.Include(a => a.Empleado).Where(x => x.FechaBaja == null)*/
        //                        from oEmpleado in db.Empleado
        //                        join oEmpEmp in db.EmpleadoEmpresa on oEmpleado.IdEmpleado equals oEmpEmp.idEmpleado
        //                        where oEmpEmp.idEmpresa == IdEmpresa && oEmpEmp.FechaBaja == null
        //                        select oEmpleado;

        //        foreach (var emp in empleados)
        //        {
        //            emp.Nombre = emp.Apellido + ", " + emp.Nombre;
        //        }
        //        ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre");
        //    }
        //    return View();
        //}

        //// POST: Afiliados/Create
        //// Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        //// más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "IdAfiliado,IdEmpleado,CantFamiliaresACargo,FechaAlta")] Afiliado afiliado)
        //{
        //    var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
        //    int IdEmpresa = Convert.ToInt32(claim.Value);

        //    Afiliado af = db.Afiliado.Where(x => x.IdEmpleado == afiliado.IdEmpleado && x.FechaBaja == null).FirstOrDefault();
        //    if (af == null)
        //    {
        //        if (afiliado.FechaAlta == null) afiliado.FechaAlta = DateTime.Today;
        //        if (ModelState.IsValid)
        //        {
        //            EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == afiliado.IdEmpleado && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();

        //            empEmp.EsAfiliado = true;

        //            db.Afiliado.Add(afiliado);
        //            db.SaveChanges();
        //            return RedirectToAction("CreateMessage");
        //        }
        //    }
        //    else
        //    {
        //        if(af.FechaBaja == null)
        //        {
        //            ModelState.AddModelError("IdEmpleado", "Este Empleado ya esta Afiliado");
        //        }
        //        else
        //        {
        //            EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == afiliado.IdEmpleado && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();

        //            empEmp.EsAfiliado = true;
        //            af.FechaBaja = null;

        //            db.SaveChanges();
        //            //return RedirectToAction("Edit", new { id = af.IdAfiliado });
        //        }
        //    }

        //    var empleados = /*db.Afiliado.Include(a => a.Empleado).Where(x => x.FechaBaja == null)*/
        //                    from oEmpleado in db.Empleado
        //                    join oEmpEmp in db.EmpleadoEmpresa on oEmpleado.IdEmpleado equals oEmpEmp.idEmpleado
        //                    where oEmpEmp.idEmpresa == IdEmpresa && oEmpEmp.FechaBaja == null
        //                    select oEmpleado;
        //    foreach (var emp in empleados)
        //    {
        //        emp.Nombre = emp.Apellido + ", " + emp.Nombre;
        //    }
        //    ViewBag.IdEmpleado = new SelectList(empleados, "IdEmpleado", "Nombre", afiliado.IdEmpleado);
        //    return View(afiliado);
        //}

        //public ActionResult CreateMessage()
        //{
        //    return View();
        //}

        // GET: Afiliados/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Afiliado afiliado = db.Afiliado.Find(id);
            if (afiliado == null)
            {
                return HttpNotFound();
            }

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            var empleados = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.FechaBaja == null).Include(t => t.Empleado).ToList();
                            /*db.Afiliado.Include(a => a.Empleado).Where(x => x.FechaBaja == null)*/
                            /*from oEmpleado in db.Empleado
                            join oEmpEmp in db.EmpleadoEmpresa on oEmpleado.IdEmpleado equals oEmpEmp.idEmpleado
                            where oEmpEmp.idEmpresa == IdEmpresa && oEmpEmp.FechaBaja == null
                            select oEmpleado;*/
            foreach (var emp in empleados)
            {
                emp.NombreEmpleado = emp.Empleado.Apellido + ", " + emp.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleados, "IdEmpleadoEmpresa", "NombreEmpleado", afiliado.IdEmpleadoEmpresa);
            return View(afiliado);
        }

        // POST: Afiliados/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdAfiliado,IdEmpleadoEmpresa,CantFamiliaresACargo,FechaAlta,FechaBaja")] Afiliado afiliado)
        {
            if (ModelState.IsValid)
            {
                if (afiliado.FechaAlta > afiliado.FechaBaja || afiliado.FechaBaja > DateTime.Today)
                {
                    if (afiliado.FechaAlta > afiliado.FechaBaja)
                    {
                        ModelState.AddModelError("FechaBaja", "La Fecha de Baja no puede ser menor a la fecha de alta del afiliado");
                    }
                    else
                    {
                        ModelState.AddModelError("FechaBaja", "La Fecha de Baja no puede ser mayor a la fecha de hoy");
                    }

                    return View(afiliado);
                }

                db.Entry(afiliado).State = EntityState.Modified;
                db.SaveChanges();

                if(afiliado.FechaBaja != null)
                {
                    return RedirectToAction("IndexBajas");
                }
                else
                {
                    return RedirectToAction("EditMessage");
                }
            }

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            var empleados = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.FechaBaja == null).Include(t => t.Empleado).ToList();
                            /*db.Afiliado.Include(a => a.Empleado).Where(x => x.FechaBaja == null)*/
                            /*from oEmpleado in db.Empleado
                            join oEmpEmp in db.EmpleadoEmpresa on oEmpleado.IdEmpleado equals oEmpEmp.idEmpleado
                            where oEmpEmp.idEmpresa == IdEmpresa && oEmpEmp.FechaBaja == null
                            select oEmpleado;*/
            foreach (var emp in empleados)
            {
                emp.NombreEmpleado = emp.Empleado.Apellido + ", " + emp.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleados, "IdEmpleadoEmpresa", "NombreEmpleado", afiliado.IdEmpleadoEmpresa);
            return View(afiliado);
        }

        public ActionResult EditMessage()
        {
            return View();
        }

        // GET: Afiliados/ReActivate/5
        public ActionResult ReActivate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Afiliado afiliado = db.Afiliado.Find(id);
            if (afiliado == null)
            {
                return HttpNotFound();
            }

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            EmpleadoEmpresa empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleado == afiliado.IdEmpleadoEmpresa).FirstOrDefault();
            if (empleadoEmpresa == null || empleadoEmpresa.FechaBaja != null)
            {
                return HttpNotFound();
            }

            var empleados = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.FechaBaja == null).Include(t => t.Empleado).ToList();
                            /*db.Afiliado.Include(a => a.Empleado).Where(x => x.FechaBaja == null)*/
                            /*from oEmpleado in db.Empleado
                            join oEmpEmp in db.EmpleadoEmpresa on oEmpleado.IdEmpleado equals oEmpEmp.idEmpleado
                            where oEmpEmp.idEmpresa == IdEmpresa && oEmpEmp.FechaBaja == null
                            select oEmpleado;*/
            foreach (var emp in empleados)
            {
                emp.NombreEmpleado = emp.Empleado.Apellido + ", " + emp.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleados, "IdEmpleadoEmpresa", "NombreEmpleado", afiliado.IdEmpleadoEmpresa);
            return View(afiliado);
        }

        // POST: Afiliados/ReActivate/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReActivate([Bind(Include = "IdAfiliado,IdEmpleadoEmpresa,CantFamiliaresACargo,FechaAlta,ReActivate")] Afiliado afiliado)
        {
            if (ModelState.IsValid)
            {
                if (afiliado.ReActivate)
                {
                    afiliado.FechaBaja = null;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            var empleados = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.FechaBaja == null).Include(t => t.Empleado).ToList();
                            /*db.Afiliado.Include(a => a.Empleado).Where(x => x.FechaBaja == null)*/
                            /*from oEmpleado in db.Empleado
                            join oEmpEmp in db.EmpleadoEmpresa on oEmpleado.IdEmpleado equals oEmpEmp.idEmpleado
                            where oEmpEmp.idEmpresa == IdEmpresa && oEmpEmp.FechaBaja == null
                            select oEmpleado;*/
            foreach (var emp in empleados)
            {
                emp.NombreEmpleado = emp.Empleado.Apellido + ", " + emp.Empleado.Nombre;
            }
            ViewBag.IdEmpleadoEmpresa = new SelectList(empleados, "IdEmpleadoEmpresa", "NombreEmpleado", afiliado.IdEmpleadoEmpresa);
            return View(afiliado);
        }

        public ActionResult ReActivateMessage()
        {
            return View();
        }

        // GET: Afiliados/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Afiliado afiliado = db.Afiliado.Find(id);
            if (afiliado == null)
            {
                return HttpNotFound();
            }
            return View(afiliado);
        }

        // POST: Afiliados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, DateTime FechaBaja)
        {
            bool error = false;
            Afiliado afiliado = db.Afiliado.Find(id);
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == afiliado.IdEmpleadoEmpresa).FirstOrDefault();

            if (afiliado.FechaAlta > FechaBaja)
            {
                error = true;
                ViewBag.MensajeError = "La Fecha de Baja no puede ser menor a la fecha de alta del afiliado";
            }
            if(FechaBaja > DateTime.Today)
            {
                error = true;
                ViewBag.MensajeError = "La Fecha de Baja no puede ser mayor a la fecha de hoy";
            }

            int mesAux = FechaBaja.Month, anioAux  = FechaBaja.Year;

            var DeclaracionJurada = db.DeclaracionJurada.Where(x => x.idEmpresa == empleadoEmpresa.idEmpresa && x.mes >= mesAux && x.anio >= anioAux).FirstOrDefault();

            if(DeclaracionJurada  != null)
            {
                error = true;
                ViewBag.MensajeError = "La Fecha de Baja no puede ser menor a la fecha de la ultima DDJJ generada";
            }

            if (!error)
            {
                afiliado.FechaBaja = FechaBaja;//DateTime.Today;

                var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
                int IdEmpresa = Convert.ToInt32(claim.Value);

                //EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == afiliado.IdEmpleadoEmpresa).FirstOrDefault();
                empleadoEmpresa.EsAfiliado = false;

                db.Entry(afiliado).State = EntityState.Modified;
                db.Entry(empleadoEmpresa).State = EntityState.Modified;

                //db.Afiliado.Remove(afiliado);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return View(afiliado);
            }
        }

        public ActionResult DeleteMessage()
        {
            return View();
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
