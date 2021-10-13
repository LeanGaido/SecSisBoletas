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
    public class BoletaAportesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Empresas/BoletaAportes
        public ActionResult Index(int? mes, int? anio, int estadoPago = 0)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            var boletaAportes = db.BoletaAportes.Include(b => b.DeclaracionJurada)
                                                .Where(x => x.DeclaracionJurada.idEmpresa == IdEmpresa && x.DeBaja == false);

            if (estadoPago == 1)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == true);
            }
            if (estadoPago == 2)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == false);
            }

            if (mes != null && mes != 0 && anio != null && anio != 0)
            {
                boletaAportes = boletaAportes.Where(x => x.MesBoleta == mes && x.AnioBoleta == anio);
            }

            foreach (var boleta in boletaAportes)
            {

                DeclaracionJurada ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == boleta.IdDeclaracionJurada).FirstOrDefault();

                var empleados = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == ddjj.IdDeclaracionJurada).ToList();

                int count2 = 0, count5 = 0;
                decimal sueldos2 = 0, sueldos5 = 0;
                foreach (var empleado in empleados)
                {
                    sueldos2 += empleado.Sueldo;
                    count2++;
                    if (empleado.SueldoBase > 0)
                    {
                        sueldos5 += empleado.SueldoBase;
                        count5++;
                    }

                    //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleado.IdEmpleadoEmpresa).FirstOrDefault();
                    //if (afiliado != null)
                    //{
                    //    if (afiliado.FechaAlta.Year < ddjj.anio)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                    //        {
                    //            //if (empleado.idJornadaLaboral == 1 || empleado.idJornadaLaboral == 2)
                    //            //{
                    //            //if (empleado.SueldoBase > 0)
                    //            //{
                    //            sueldos5 += empleado.SueldoBase;
                    //            //}
                    //            //else
                    //            //{
                    //            //    sueldos5 += empleado.Sueldo;
                    //            //}
                    //            //}
                    //            //else
                    //            //{
                    //            //    sueldos5 += empleado.Sueldo;
                    //            //}
                    //            count5++;
                    //        }
                    //    }
                    //    else if (afiliado.FechaAlta.Year == ddjj.anio && afiliado.FechaAlta.Month <= ddjj.mes)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                    //        {
                    //            //if (empleado.idJornadaLaboral == 1 || empleado.idJornadaLaboral == 2)
                    //            //{
                    //            //if (empleado.SueldoBase > 0)
                    //            //{
                    //            sueldos5 += empleado.SueldoBase;
                    //            //}
                    //            //else
                    //            //{
                    //            //    sueldos5 += empleado.Sueldo;
                    //            //}
                    //            //}
                    //            //else
                    //            //{
                    //            //    sueldos5 += empleado.Sueldo;
                    //            //}
                    //            count5++;
                    //        }
                    //    }
                    //}
                }

                decimal total2 = (sueldos2 / 100) * 2;
                decimal total5 = (sueldos5 / 100) * 5;

                decimal mora = (boleta.RecargoMora != null) ? (decimal)boleta.RecargoMora : 0;
                //(Math.Truncate(((sueldos / 100) * 5) * 100) / 100).ToString();
                boleta.TotalDepositado2 = TruncateFunction(total2, 2);
                boleta.TotalDepositado5 = TruncateFunction(total5, 2);
                boleta.TotalDepositado = TruncateFunction(total2 + total5 + mora, 2);
            }

            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            ViewBag.estadoPago = estadoPago;

            return View(boletaAportes.OrderByDescending(x => x.AnioBoleta).ThenByDescending(x => x.MesBoleta).ToList());
        }

        public ActionResult BoletasEspeciales(int? mes, int? anio, int estadoPago = 0)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            List<BoletaAportesEspecial> boletaAportes = new List<BoletaAportesEspecial>();

            boletaAportes = db.BoletaAportesEspeciales.AsNoTracking().Where(x => x.IdEmpresa == IdEmpresa && x.BoletaPagada).ToList();
            

            if (estadoPago == 1)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == true).ToList();
            }

            if (estadoPago == 2)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == false).ToList();
            }

            ViewBag.estadoPago = estadoPago;

            return View(boletaAportes.OrderByDescending(x => x.FechaBoleta));
        }

        // GET: Empresas/BoletaAportes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);

            if (boletaAportes == null)
            {
                return HttpNotFound();
            }

            DeclaracionJurada ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == boletaAportes.IdDeclaracionJurada).FirstOrDefault();

            var empleados = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == ddjj.IdDeclaracionJurada).ToList();

            int count2 = 0, count5 = 0;
            decimal sueldos2 = 0, sueldos5 = 0;
            foreach (var empleado in empleados)
            {
                sueldos2 += empleado.Sueldo;
                count2++;
                if (empleado.SueldoBase > 0)
                {
                    sueldos5 += empleado.SueldoBase;
                    count5++;
                }

                //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleado.IdEmpleadoEmpresa).FirstOrDefault();
                //if (afiliado != null)
                //{
                //    if (afiliado.FechaAlta.Year < ddjj.anio)
                //    {
                //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                //        {
                //            //if (empleado.idJornadaLaboral == 1 || empleado.idJornadaLaboral == 2)
                //            //{
                //            //if (empleado.SueldoBase > 0)
                //            //{
                //            sueldos5 += empleado.SueldoBase;
                //            //}
                //            //else
                //            //{
                //            //    sueldos5 += empleado.Sueldo;
                //            //}
                //            //}
                //            //else
                //            //{
                //            //    sueldos5 += empleado.Sueldo;
                //            //}
                //            count5++;
                //        }
                //    }
                //    else if (afiliado.FechaAlta.Year == ddjj.anio && afiliado.FechaAlta.Month <= ddjj.mes)
                //    {
                //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                //        {
                //            //if (empleado.idJornadaLaboral == 1 || empleado.idJornadaLaboral == 2)
                //            //{
                //            //if (empleado.SueldoBase > 0)
                //            //{
                //            sueldos5 += empleado.SueldoBase;
                //            //}
                //            //else
                //            //{
                //            //    sueldos5 += empleado.Sueldo;
                //            //}
                //            //}
                //            //else
                //            //{
                //            //    sueldos5 += empleado.Sueldo;
                //            //}
                //            count5++;
                //        }
                //    }
                //}
            }

            boletaAportes.TotalSueldos2 = TruncateFunction(sueldos2, 2);

            boletaAportes.TotalSueldos5 = TruncateFunction(sueldos5, 2);

            decimal total2 = (sueldos2 / 100) * 2;
            decimal total5 = (sueldos5 / 100) * 5;

            decimal mora = (boletaAportes.RecargoMora != null) ? (decimal)boletaAportes.RecargoMora : 0;
            //(Math.Truncate(((sueldos / 100) * 5) * 100) / 100).ToString();
            boletaAportes.TotalDepositado2 = TruncateFunction(total2, 2);//Math.Truncate((total2 * 100) / (decimal)100);// Math.Truncate(total2);
            boletaAportes.TotalDepositado5 = TruncateFunction(total5, 2);//Math.Truncate((total5 * 100) / 100);// Math.Truncate(total5);
            boletaAportes.TotalDepositado = TruncateFunction(total2 + total5 + mora, 2);//Math.Truncate(((total2 + total5 + mora) * 100) / 100); //Math.Truncate(total2 + total5 + mora);

            ViewBag.IdEmpresa = ddjj.idEmpresa;

            return View(boletaAportes);
        }

        public ActionResult DetailsBoletaEspecial(int? id)
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

        // GET: Empresas/BoletaAportes/Create
        public ActionResult Create()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var declaracionesJuradas = db.DeclaracionJurada.Where(x => x.idEmpresa == IdEmpresa && x.DeBaja == false).OrderByDescending(x => x.anio).ThenByDescending(x => x.mes).ToList();
            foreach (DeclaracionJurada declaracion in declaracionesJuradas)
            {
                declaracion.MesAnio = declaracion.mes + "/" + declaracion.anio;
            }
            ViewBag.TotalSueldo = 0;

            ViewBag.IdDeclaracionJurada = new SelectList(declaracionesJuradas, "IdDeclaracionJurada", "MesAnio");
            return View();
        }

        // POST: Empresas/BoletaAportes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdBoleta,IdDeclaracionJurada,MesBoleta,AnioBoleta")] BoletaAportes boletaAportes)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var declaracionesJuradas = db.DeclaracionJurada.Where(x => x.idEmpresa == IdEmpresa && x.DeBaja == false).OrderByDescending(x => x.anio).ThenByDescending(x => x.mes).ToList();

            foreach (DeclaracionJurada declaracion in declaracionesJuradas)
            {
                declaracion.MesAnio = declaracion.mes + "/" + declaracion.anio;
            }

            ViewBag.IdDeclaracionJurada = new SelectList(declaracionesJuradas, "IdDeclaracionJurada", "MesAnio", boletaAportes.IdDeclaracionJurada);

            int cantEmpleados = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa &&
                                                              (x.FechaAlta.Year < boletaAportes.AnioBoleta ||
                                                              (x.FechaAlta.Year == boletaAportes.AnioBoleta && x.FechaAlta.Month <= boletaAportes.MesBoleta)) &&
                                                              (x.FechaBaja.Value == null || 
                                                              (x.FechaBaja.Value.Year > boletaAportes.AnioBoleta || 
                                                              (x.FechaBaja.Value.Year == boletaAportes.AnioBoleta && x.FechaBaja.Value.Month > boletaAportes.MesBoleta)))).Count();

            int cantDetalleDeclaracion = db.DetalleDeclaracionJurada.Include(t => t.DeclaracionJurada)
                                                                    .Include(t => t.EmpleadoEmpresa)
                                                                    .Where(x => x.EmpleadoEmpresa.idEmpresa == IdEmpresa &&
                                                                                x.DeclaracionJurada.mes == boletaAportes.MesBoleta &&
                                                                                x.DeclaracionJurada.anio == boletaAportes.AnioBoleta).Count();

            if(cantDetalleDeclaracion < cantEmpleados)
            {
                ModelState.AddModelError("IdDeclaracionJurada", "No todos los empleados estan declarados en la declararion jurada");
                return View(boletaAportes);
            }

            if (db.BoletaAportes.Where(x => x.IdDeclaracionJurada == boletaAportes.IdDeclaracionJurada && x.DeBaja == false).FirstOrDefault() != null)
            {
                ModelState.AddModelError("IdDeclaracionJurada", "Ya Existe una boleta Generada para este mes y año");
                return View(boletaAportes);
            }

            var ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == boletaAportes.IdDeclaracionJurada).FirstOrDefault();

            boletaAportes.TotalSueldos2 = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == boletaAportes.IdDeclaracionJurada).Sum(x => x.Sueldo);
            var detalles = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == boletaAportes.IdDeclaracionJurada).ToList();

            int count2 = 0, count5 = 0;
            decimal sueldos2 = 0, sueldos5 = 0;

            foreach (var detalle in detalles)
            {
                var empEmpAux = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();

                var empleado = db.Empleado.Where(x => x.IdEmpleado == empEmpAux.idEmpleado).FirstOrDefault();

                count2++;
                sueldos2 += detalle.Sueldo;
                if (detalle.SueldoBase > 0)
                {
                    sueldos5 += detalle.SueldoBase;
                    count5++;
                }
            }

            boletaAportes.CantEmpleados = count2;
            boletaAportes.TotalSueldos2 = sueldos2;
            boletaAportes.Aportes2 = TruncateFunction((sueldos2/100)*2,2);

            boletaAportes.CantAfiliados = count5;
            boletaAportes.TotalSueldos5 = sueldos5;
            boletaAportes.Aportes5 = TruncateFunction((sueldos5 / 100) * 5, 2);

            boletaAportes.FechaBoleta = DateTime.Today;
            boletaAportes.FechaVencimiento = GenerarVencimiento(boletaAportes.MesBoleta, boletaAportes.AnioBoleta);
            boletaAportes.BoletaPagada = false;
            boletaAportes.RecargoMora = 0;

            if (ModelState.IsValid)
            {
                db.BoletaAportes.Add(boletaAportes);
                db.SaveChanges();
                return RedirectToAction("CreateMessage");
            }
            
            return View(boletaAportes);
        }

        public ActionResult CreateMessage()
        {
            return View();
        }

        //// GET: Empresas/BoletaAportes/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
        //    if (boletaAportes == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    if (boletaAportes.BoletaPagada)
        //    {
        //        return RedirectToAction("Index", new { mes = boletaAportes.MesBoleta, anio = boletaAportes.AnioBoleta });
        //    }

        //    var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
        //    int IdEmpresa = Convert.ToInt32(claim.Value);

        //    var declaracionesJuradas = db.DeclaracionJurada.Where(x => x.idEmpresa == IdEmpresa).ToList();

        //    foreach (DeclaracionJurada declaracion in declaracionesJuradas)
        //    {
        //        declaracion.MesAnio = declaracion.mes + "/" + declaracion.anio;
        //    }

        //    ViewBag.IdDeclaracionJurada = new SelectList(declaracionesJuradas, "IdDeclaracionJurada", "MesAnio", boletaAportes.IdDeclaracionJurada);
        //    return View(boletaAportes);
        //}

        //// POST: Empresas/BoletaAportes/Edit/5
        //// Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        //// más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "IdBoleta,IdDeclaracionJurada,MesBoleta,AnioBoleta,FechaVencimiento,TotalSueldos,RecargoMora,BoletaPagada,FechaPago,FechaBoleta")] BoletaAportes boletaAportes)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(boletaAportes).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
        //    int IdEmpresa = Convert.ToInt32(claim.Value);

        //    var declaracionesJuradas = db.DeclaracionJurada.Where(x => x.idEmpresa == IdEmpresa).ToList();

        //    foreach (DeclaracionJurada declaracion in declaracionesJuradas)
        //    {
        //        declaracion.MesAnio = declaracion.mes + "/" + declaracion.anio;
        //    }

        //    ViewBag.IdDeclaracionJurada = new SelectList(declaracionesJuradas, "IdDeclaracionJurada", "MesAnio", boletaAportes.IdDeclaracionJurada);
        //    return View(boletaAportes);
        //}


        //// GET: Empresas/BoletaAportes/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
        //    if (boletaAportes == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if (boletaAportes.BoletaPagada)
        //    {
        //        return RedirectToAction("Index", new { mes = boletaAportes.MesBoleta, anio = boletaAportes.AnioBoleta } );
        //    }
        //    return View(boletaAportes);
        //}

        //// POST: Empresas/BoletaAportes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
        //    if (boletaAportes.BoletaPagada)
        //    {
        //        return RedirectToAction("CantDeleteMessage", new { mes = boletaAportes.MesBoleta, anio = boletaAportes.AnioBoleta });
        //    }
        //    db.BoletaAportes.Remove(boletaAportes);
        //    db.SaveChanges();
        //    return RedirectToAction("DeleteMessage");
        //}

        public ActionResult DeleteMessage()
        {
            return View();
        }

        public ActionResult CantDeleteMessage()
        {
            return View();
        }

        public DateTime GenerarVencimiento(int mes, int anio)
        {
            //if(mes != 12)
            //{
                return db.FechaVencimiento.Where(x => x.mesBoleta == mes && x.anioBoleta == anio).FirstOrDefault().FechaVto;
            //}
            //else
            //{
            //    return db.FechaVencimiento.Where(x => x.FechaVto.Month == 1 && x.FechaVto.Year == (anio + 1)).FirstOrDefault().FechaVto;
            //}
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
