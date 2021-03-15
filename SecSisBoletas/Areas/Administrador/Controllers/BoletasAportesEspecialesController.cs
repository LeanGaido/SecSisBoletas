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
    [Authorize]
    public class BoletasAportesEspecialesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/BoletasAportesEspeciales
        public ActionResult Index(int? mes, int? anio, int estadoPago = 0, int idEmpresa = 0)
        {
            List<BoletaAportesEspecial> boletaAportes = new List<BoletaAportesEspecial>();

            if(idEmpresa != 0)
            {
                boletaAportes = db.BoletaAportesEspeciales.AsNoTracking().Where(x => x.IdEmpresa == idEmpresa).ToList();
            }
            else
            {
                boletaAportes = db.BoletaAportesEspeciales.AsNoTracking().ToList();
            }

            if (estadoPago == 1)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == true).ToList();
            }

            if (estadoPago == 2)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == false).ToList();
            }

            List<Empresa> empresas = db.Empresa.AsNoTracking().OrderBy(x => x.RazonSocial).ToList();

            empresas.Insert(0, new Empresa { IdEmpresa = 0, RazonSocial = "Empresas No Registradas" });

            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial", idEmpresa);
            ViewBag.estadoPago = estadoPago;

            return View(boletaAportes.OrderByDescending(x => x.FechaBoleta));
        }

        // GET: Administrador/BoletasAportesEspeciales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportesEspecial boletaAportesEspecial = db.BoletaAportesEspeciales.Find(id);
            if (boletaAportesEspecial == null)
            {
                return HttpNotFound();
            }
            return View(boletaAportesEspecial);
        }

        // GET: Administrador/BoletasAportesEspeciales/Create
        public ActionResult Create()
        {
            List<Empresa> empresas = db.Empresa.OrderBy(x => x.RazonSocial).ToList();
            empresas.Insert(0, new Empresa { IdEmpresa = 0, RazonSocial = "Nueva Empresa" });
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial");

            BoletaAportesEspecial boletaAportesEspecial = new BoletaAportesEspecial();

            boletaAportesEspecial.RecargoMora = 0;

            return View(boletaAportesEspecial);
        }

        // POST: Administrador/BoletasAportesEspeciales/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdBoleta,FechaBoleta,Observaciones,BoletaPagada,FechaPago,IdEmpresa,Cuit,RazonSocial,NombreFantasia,Calle,Altura,Localidad,CodPostal,TelefonoFijo,TelefonoCelular,Periodo,CantEmpleados,TotalSueldos2,Aportes2,CantAfiliados,TotalSueldos5,Aportes5,RecargoMora,Total,TotalDepositado,FechaVencimiento")] BoletaAportesEspecial boletaAportesEspecial)
        {
            //Cuit,RazonSocial,NombreFantasia,Calle,Altura,Localidad,CodPostal,TelefonoFijo,TelefonoCelular
            if(boletaAportesEspecial.IdEmpresa != 0)
            {
                var Empresa = db.Empresa.Where(x => x.IdEmpresa == boletaAportesEspecial.IdEmpresa).FirstOrDefault();

                if (Empresa == null)
                {
                    //Error
                }

                boletaAportesEspecial.Cuit = Empresa.Cuit;
                boletaAportesEspecial.RazonSocial = Empresa.RazonSocial;
                boletaAportesEspecial.NombreFantasia = Empresa.NombreFantasia;
                boletaAportesEspecial.Calle = Empresa.Calle;
                boletaAportesEspecial.Altura = Empresa.Altura;
                boletaAportesEspecial.Localidad = Empresa.Localidad.Nombre;
                boletaAportesEspecial.CodPostal = Empresa.Localidad.CodPostal.ToString();
                boletaAportesEspecial.TelefonoFijo = Empresa.TelefonoFijo;
                boletaAportesEspecial.TelefonoCelular = Empresa.TelefonoCelular;
            }

            boletaAportesEspecial.FechaBoleta = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.BoletaAportesEspeciales.Add(boletaAportesEspecial);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            List<Empresa> empresas = db.Empresa.OrderBy(x => x.RazonSocial).ToList();
            empresas.Insert(0, new Empresa { IdEmpresa = 0, RazonSocial = "Nueva Empresa" });
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial", boletaAportesEspecial.IdEmpresa);

            return View(boletaAportesEspecial);
        }

        // GET: Administrador/BoletasDeAportesEspeciales/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportesEspecial boletaAportesEspecial = db.BoletaAportesEspeciales.Find(id);
            if (boletaAportesEspecial == null)
            {
                return HttpNotFound();
            }
            return View(boletaAportesEspecial);
        }

        // POST: Administrador/BoletasDeAportesEspeciales/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdBoleta,FechaBoleta,Observaciones,BoletaPagada,FechaPago,IdEmpresa,Cuit,RazonSocial,NombreFantasia,Calle,Altura,Localidad,CodPostal,TelefonoFijo,TelefonoCelular,Periodo,CantEmpleados,TotalSueldos2,Aportes2,CantAfiliados,TotalSueldos5,Aportes5,RecargoMora,Total,TotalDepositado,FechaVencimiento")] BoletaAportesEspecial boletaAportesEspecial)
        {
            if (ModelState.IsValid)
            {
                db.Entry(boletaAportesEspecial).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(boletaAportesEspecial);
        }

        // GET: Administrador/BoletasAportesEspeciales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportesEspecial boletaAportesEspecial = db.BoletaAportesEspeciales.Find(id);
            if (boletaAportesEspecial == null)
            {
                return HttpNotFound();
            }
            return View(boletaAportesEspecial);
        }

        // POST: Administrador/BoletasAportesEspeciales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BoletaAportesEspecial boletaAportesEspecial = db.BoletaAportesEspeciales.Find(id);
            db.BoletaAportesEspeciales.Remove(boletaAportesEspecial);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize]
        // GET: Administrador/Listados/PagarBoletaAportes/5
        public ActionResult Pagar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportesEspecial boletaAportes = db.BoletaAportesEspeciales.Find(id);
            if (boletaAportes == null)
            {
                return HttpNotFound();
            }
            if (boletaAportes.BoletaPagada)
            {
                return RedirectToAction("Index");
            }
            return View(boletaAportes);
        }

        [Authorize]
        // POST: Administrador/Listados/PagarBoletaAportes/5

        [HttpPost, ActionName("Pagar")]
        [ValidateAntiForgeryToken]
        public ActionResult PagarConfirmed(int id)
        {
            BoletaAportesEspecial boletaAportes = db.BoletaAportesEspeciales.Find(id);
            boletaAportes.BoletaPagada = true;
            boletaAportes.FechaPago = DateTime.Today;

            boletaAportes.TotalDepositado = boletaAportes.Total;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize]
        // GET: Administrador/Listados/PagarBoletaAportes/5
        public ActionResult AnularPago(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportesEspecial boletaAportes = db.BoletaAportesEspeciales.Find(id);
            if (boletaAportes == null)
            {
                return HttpNotFound();
            }
            return View(boletaAportes);
        }

        [Authorize]
        // POST: Administrador/Listados/PagarBoletaAportes/5
        [HttpPost, ActionName("AnularPago")]
        [ValidateAntiForgeryToken]
        public ActionResult AnularPagoConfirmed(int id)
        {
            BoletaAportesEspecial boletaAportes = db.BoletaAportesEspeciales.Find(id);
            boletaAportes.BoletaPagada = false;
            boletaAportes.FechaPago = null;

            boletaAportes.TotalDepositado = 0;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public decimal TruncateFunction(decimal number, int digits)
        {
            decimal stepper = (decimal)(Math.Pow(10.0, (double)digits));
            int temp = (int)(stepper * number);
            return (decimal)temp / stepper;
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
