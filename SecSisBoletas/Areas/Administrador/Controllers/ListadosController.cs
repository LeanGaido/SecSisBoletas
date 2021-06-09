using DAL;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using PagedList;
using Rotativa;
using DAL.ViewModels;
using SecSisBoletas.Areas.Empresas.Controllers;
using SecSisBoletas.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using ClosedXML.Excel;
using System.IO.Compression;
using System.IO;

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    public class ListadosController : Controller
    {
        private SecModel db = new SecModel();
        private ApplicationDbContext context = new ApplicationDbContext();
        private DeclaracionesJuradasController ddjj = new DeclaracionesJuradasController();
        private static int idDeclaracionJurada;

        #region Boletas
        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Administrador/Listados/BoletaAportes
        public ActionResult IndexBoletaAportes(int? mes, int? anio, int estadoPago = 0, int idEmpresa = 0)
        {
            //if (idEmpresa != 0)
            //{
            var boletaAportes = db.BoletaAportes.Include(t => t.DeclaracionJurada).Where(x => x.DeclaracionJurada.idEmpresa == idEmpresa && x.DeBaja == false);
            //}

            if (estadoPago == 1)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == true);
            }

            if (estadoPago == 2)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == false);
            }

            if (mes != null && anio != null)
            {
                boletaAportes = boletaAportes.Where(x => x.MesBoleta == mes && x.AnioBoleta == anio);
            }

            foreach (var boleta in boletaAportes)
            {
                #region Old
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

                //decimal total2 = (sueldos2 / 100) * 2;
                //decimal total5 = (sueldos5 / 100) * 5;
                #endregion
                
                boleta.CantEmpleados = count2;
                boleta.TotalSueldos2 = sueldos2;
                boleta.Aportes2 = (Math.Truncate(((sueldos2 / 100) * 2) * 100) / 100);

                boleta.CantAfiliados = count5;
                boleta.TotalSueldos5 = sueldos5;
                boleta.Aportes5 = (Math.Truncate(((sueldos5 / 100) * 5) * 100) / 100);
                
                decimal mora = (boleta.RecargoMora != null) ? (decimal)boleta.RecargoMora : 0;

                boleta.TotalDepositado = TruncateFunction(boleta.Aportes2 + boleta.Aportes5 + mora, 2);

                //boleta.TotalDepositado2 = TruncateFunction(boleta.Aportes2, 2);
                //boleta.TotalDepositado5 = TruncateFunction(boleta.Aportes5, 2);
                //boleta.TotalDepositado = TruncateFunction(boleta.Aportes2 + boleta.Aportes5 + mora,2);
            }

            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            List<Empresa> empresas = db.Empresa.OrderBy(x => x.RazonSocial).ToList();
            empresas.Insert(0, new Empresa { IdEmpresa = 0, RazonSocial = "Todas" });
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial", idEmpresa);
            ViewBag.estadoPago = estadoPago;

            return View(boletaAportes.OrderByDescending(x => x.AnioBoleta).ThenByDescending(x => x.MesBoleta).ToList());
        }

        public void ActualizarValores(int? act)
        {
            if(act != null && act == 3443)
            {
                var boletaAportesAux = db.BoletaAportes.Include(b => b.DeclaracionJurada).Where(x => x.DeBaja == false);
                foreach (var boleta in boletaAportesAux)
                {
                    boleta.TotalSueldos5 = 0;

                    int count2 = 0, count5 = 0;
                    decimal sueldos2 = 0, sueldos5 = 0;

                    var detalles = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == boleta.IdDeclaracionJurada).Include(t => t.EmpleadoEmpresa).Include(t => t.EmpleadoEmpresa.Empleado).ToList();
                    foreach (var empleado in detalles)
                    {
                        sueldos2 += empleado.Sueldo;
                        count2++;
                        if (empleado.SueldoBase > 0)
                        {
                            sueldos5 += empleado.SueldoBase;
                            count5++;
                        }
                    }
                    //foreach (var detalle in detalles)
                    //{
                    //    var empEmpAux = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();

                    //    var empleado = db.Empleado.Where(x => x.IdEmpleado == empEmpAux.idEmpleado).FirstOrDefault();

                    //    count2++;
                    //    sueldos2 += detalle.Sueldo;

                    //    var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empEmpAux.idEmpleadoEmpresa).FirstOrDefault();
                    //    if (afiliado != null)
                    //    {
                    //        if (afiliado.FechaAlta.Year < boleta.DeclaracionJurada.anio)
                    //        {
                    //            if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > boleta.DeclaracionJurada.anio || (afiliado.FechaBaja.Value.Year == boleta.DeclaracionJurada.anio && afiliado.FechaBaja.Value.Month >= boleta.DeclaracionJurada.mes))
                    //            {
                    //                count5++;
                    //                sueldos5 += detalle.SueldoBase;
                    //                boleta.TotalSueldos5 += detalle.SueldoBase;
                    //            }
                    //        }
                    //        else if (afiliado.FechaAlta.Year == boleta.DeclaracionJurada.anio && afiliado.FechaAlta.Month <= boleta.DeclaracionJurada.mes)
                    //        {
                    //            if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > boleta.DeclaracionJurada.anio || (afiliado.FechaBaja.Value.Year == boleta.DeclaracionJurada.anio && afiliado.FechaBaja.Value.Month >= boleta.DeclaracionJurada.mes))
                    //            {
                    //                count5++;
                    //                sueldos5 += detalle.SueldoBase;
                    //                boleta.TotalSueldos5 += detalle.SueldoBase;
                    //            }
                    //        }
                    //    }
                    //}

                    boleta.CantEmpleados = count2;
                    boleta.TotalSueldos2 = sueldos2;
                    boleta.Aportes2 = TruncateFunction(((sueldos2 / 100) * 2), 2);

                    boleta.CantAfiliados = count5;
                    boleta.TotalSueldos5 = sueldos5;
                    boleta.Aportes5 = TruncateFunction(((sueldos5 / 100) * 5), 2);

                    boleta.TotalPagado = boleta.Aportes2 + boleta.Aportes5;
                }
                db.SaveChanges();
            }
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Administrador/Listados/DetailsBoletaAportes/5
        public ActionResult DetailsBoletaAportes(int? id)
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
                //                //if (empleado.SueldoBase > 0)
                //                //{
                //                    sueldos5 += empleado.SueldoBase;
                //                //}
                //                //else
                //                //{
                //                //    sueldos5 += empleado.Sueldo;
                //                //}
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
                //                //if (empleado.SueldoBase > 0)
                //                //{
                //                    sueldos5 += empleado.SueldoBase;
                //                //}
                //                //else
                //                //{
                //                //    sueldos5 += empleado.Sueldo;
                //                //}
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

            boletaAportes.CantEmpleados = count2;
            boletaAportes.TotalSueldos2 = sueldos2;
            boletaAportes.Aportes2 = (Math.Truncate(((sueldos2 / 100) * 2) * 100) / 100);

            boletaAportes.CantAfiliados = count5;
            boletaAportes.TotalSueldos5 = sueldos5;
            boletaAportes.Aportes5 = (Math.Truncate(((sueldos5 / 100) * 5) * 100) / 100);

            decimal mora = (boletaAportes.RecargoMora != null) ? (decimal)boletaAportes.RecargoMora : 0;

            boletaAportes.TotalDepositado2 = TruncateFunction(boletaAportes.Aportes2, 2);//Math.Truncate((total2 * 100) / (decimal)100);// Math.Truncate(total2);
            boletaAportes.TotalDepositado5 = TruncateFunction(boletaAportes.Aportes5, 2);//Math.Truncate((total5 * 100) / 100);// Math.Truncate(total5);
            boletaAportes.TotalDepositado = TruncateFunction(boletaAportes.Aportes2 + boletaAportes.Aportes5 + mora, 2);//Math.Truncate(((total2 + total5 + mora) * 100) / 100); //Math.Truncate(total2 + total5 + mora);

            ViewBag.IdEmpresa = ddjj.idEmpresa;

            return View(boletaAportes);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Administrador/Listados/EditBoletaAportes/5
        public ActionResult EditBoletaAportes(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BoletaAportes boletaAportes = db.BoletaAportes.Include(t => t.DeclaracionJurada).Where(x => x.IdBoleta == id).FirstOrDefault();

            if (boletaAportes == null)
            {
                return HttpNotFound();
            }

            if (boletaAportes.BoletaPagada)
            {
                return RedirectToAction("IndexBoletaAportes", new { mes = boletaAportes.MesBoleta, anio = boletaAportes.AnioBoleta });
            }

            ViewBag.Periodo = boletaAportes.MesBoleta + "/" + boletaAportes.AnioBoleta;

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
                //                //if (empleado.SueldoBase > 0)
                //                //{
                //                    sueldos5 += empleado.SueldoBase;
                //                //}
                //                //else
                //                //{
                //                //    sueldos5 += empleado.Sueldo;
                //                //}
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
                //                //if (empleado.SueldoBase > 0)
                //                //{
                //                    sueldos5 += empleado.SueldoBase;
                //                //}
                //                //else
                //                //{
                //                //    sueldos5 += empleado.Sueldo;
                //                //}
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
            boletaAportes.TotalDepositado2 = TruncateFunction(total2, 2);
            boletaAportes.TotalDepositado5 = TruncateFunction(total5, 2);
            boletaAportes.TotalDepositado = TruncateFunction(total2 + total5, 2);

            var declaracionesJuradas = db.DeclaracionJurada.Where(x => x.DeBaja == false).ToList();

            foreach (DeclaracionJurada declaracion in declaracionesJuradas)
            {
                declaracion.MesAnio = declaracion.mes + "/" + declaracion.anio;
            }

            ViewBag.IdDeclaracionJurada = new SelectList(declaracionesJuradas, "IdDeclaracionJurada", "MesAnio", boletaAportes.IdDeclaracionJurada);
            return View(boletaAportes);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // POST: Administrador/Listados/EditBoletaAportes/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBoletaAportes([Bind(Include = "IdBoleta,IdDeclaracionJurada,MesBoleta,AnioBoleta,FechaVencimiento,TotalSueldos2,TotalSueldos5,RecargoMora,BoletaPagada,FechaPago,FechaBoleta,CantEmpleados,TotalSueldos2,Aportes2,CantAfiliados,TotalSueldos5,Aportes5")] BoletaAportes boletaAportes)
        {
            /*
             *CantEmpleados
             *TotalSueldos2
             *Aportes2
             *CantAfiliados
             *TotalSueldos5
             *Aportes5
             */
            if (ModelState.IsValid)
            {
                db.Entry(boletaAportes).State = EntityState.Modified;
                db.SaveChanges();

                var ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == boletaAportes.IdDeclaracionJurada).FirstOrDefault();

                return RedirectToAction("IndexBoletaAportes", new { idEmpresa = ddjj.idEmpresa });
            }

            var declaracionesJuradas = db.DeclaracionJurada.Where(x => x.DeBaja == false).ToList();

            foreach (DeclaracionJurada declaracion in declaracionesJuradas)
            {
                declaracion.MesAnio = declaracion.mes + "/" + declaracion.anio;
            }

            decimal mora = (boletaAportes.RecargoMora != null) ? (decimal)boletaAportes.RecargoMora : 0;
            //(Math.Truncate(((sueldos / 100) * 5) * 100) / 100).ToString();

            boletaAportes.TotalDepositado = TruncateFunction(boletaAportes.Aportes2 + boletaAportes.Aportes5 + mora, 2);

            ViewBag.IdDeclaracionJurada = new SelectList(declaracionesJuradas, "IdDeclaracionJurada", "MesAnio", boletaAportes.IdDeclaracionJurada);
            return View(boletaAportes);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Administrador/Listados/PagarBoletaAportes/5
        public ActionResult PagarBoletaAportes(int? id)
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
            if (boletaAportes.BoletaPagada)
            {
                return RedirectToAction("IndexBoletaAportes", new { mes = boletaAportes.MesBoleta, anio = boletaAportes.AnioBoleta });
            }
            return View(boletaAportes);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // POST: Administrador/Listados/PagarBoletaAportes/5
        [HttpPost, ActionName("PagarBoletaAportes")]
        [ValidateAntiForgeryToken]
        public ActionResult PagarBoletaAportesConfirmed(int id)
        {
            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
            boletaAportes.BoletaPagada = true;
            boletaAportes.FechaPago = DateTime.Today;

            decimal total2 = (boletaAportes.TotalSueldos2 / 100) * 2;
            decimal total5 = (boletaAportes.TotalSueldos5 / 100) * 5;

            boletaAportes.TotalPagado = TruncateFunction(total2 + total5, 2);
            db.SaveChanges();
            return RedirectToAction("IndexBoletaAportes", new { mes = boletaAportes.MesBoleta, anio = boletaAportes.AnioBoleta });
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Administrador/Listados/PagarBoletaAportes/5
        public ActionResult AnularPagoBoletaAportes(int? id)
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
            return View(boletaAportes);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // POST: Administrador/Listados/PagarBoletaAportes/5
        [HttpPost, ActionName("AnularPagoBoletaAportes")]
        [ValidateAntiForgeryToken]
        public ActionResult AnularPagoBoletaAportesConfirmed(int id)
        {
            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
            boletaAportes.BoletaPagada = false;
            boletaAportes.FechaPago = null;
            db.SaveChanges();
            return RedirectToAction("IndexBoletaAportes", new { mes = boletaAportes.MesBoleta, anio = boletaAportes.AnioBoleta });
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Empresas/BoletaAportes/Delete/5
        public ActionResult DeleteBoletaAportes(int? id)
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
            if (boletaAportes.BoletaPagada)
            {
                return RedirectToAction("CantDeleteBoletaAportesMessage");
            }
            return View(boletaAportes);
        }

        // POST: Empresas/BoletaAportes/Delete/5
        [HttpPost, ActionName("DeleteBoletaAportes")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBoletaAportesConfirmed(int id)
        {
            BoletaAportes boletaAportes = db.BoletaAportes.Find(id);
            if (boletaAportes.BoletaPagada)
            {
                return RedirectToAction("CantDeleteBoletaAportesMessage");
            }

            boletaAportes.DeBaja = true;
            boletaAportes.FechaBaja = DateTime.Now;
            boletaAportes.UserId = User.Identity.GetUserId();

            //db.BoletaAportes.Remove(boletaAportes);
            db.SaveChanges();
            return RedirectToAction("DeleteBoletaAportesMessage");
        }

        public ActionResult DeleteBoletaAportesMessage()
        {
            return View();
        }

        public ActionResult CantDeleteBoletaAportesMessage()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ImprimirBoletasPagadas(DateTime? FechaPago, string BoletasPagadas = "null")
        {
            int[] IdsBoletasAportesPagadas;
            List<BoletaAportes> BoletasAportesPagadas = new List<BoletaAportes>();
            if (!string.IsNullOrEmpty(BoletasPagadas))
            {
                string llave = BoletasPagadas.Substring(0, 1);
                if (llave == "[")
                {
                    IdsBoletasAportesPagadas = System.Web.Helpers.Json.Decode<int[]>(BoletasPagadas);
                    ViewBag.BoletasAportesPagadas = IdsBoletasAportesPagadas;
                    BoletasAportesPagadas = db.BoletaAportes.Where(x => IdsBoletasAportesPagadas.Contains(x.IdBoleta)).ToList();
                }
                else
                {
                    IdsBoletasAportesPagadas = new int[1] { System.Web.Helpers.Json.Decode<int>(BoletasPagadas) };
                    ViewBag.BoletasAportesPagadas = IdsBoletasAportesPagadas;
                    BoletasAportesPagadas = db.BoletaAportes.Where(x => IdsBoletasAportesPagadas.Contains(x.IdBoleta)).ToList();
                }
            }

            decimal totalGlobal = 0;

            if(FechaPago == null)
            {
                FechaPago = DateTime.Now;
            }

            foreach (var boleta in BoletasAportesPagadas)
            {
                decimal total2 = (boleta.TotalSueldos2 / 100) * 2;
                decimal total5 = (boleta.TotalSueldos5 / 100) * 5;

                if (boleta.BoletaPagada == false)
                {
                    boleta.BoletaPagada = true;
                    boleta.FechaPago = FechaPago;
                    decimal mora = (boleta.RecargoMora != null) ? (decimal)boleta.RecargoMora : 0;
                    boleta.TotalPagado = TruncateFunction(total2 + total5 + mora, 2);
                    db.SaveChanges();
                }
            }


            List<VmBoletaAportes> boletasDeAportes = new List<VmBoletaAportes>();

            foreach (var boleta in BoletasAportesPagadas)
            {
                DeclaracionJurada declaracion = db.DeclaracionJurada.Find(boleta.IdDeclaracionJurada);

                var detalles = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == declaracion.IdDeclaracionJurada).ToList();
                decimal total2 = 0;
                decimal total5 = 0;
                decimal sueldos2 = 0, sueldos5 = 0;

                foreach (var detalle in detalles)
                {
                    sueldos2 += detalle.Sueldo;
                    if (detalle.SueldoBase > 0)
                    {
                        sueldos5 += detalle.SueldoBase;
                        //count5++;
                    }
                    //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();
                    //if (afiliado != null)
                    //{
                    //    if (afiliado.FechaAlta.Year < declaracion.anio)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracion.anio || (afiliado.FechaBaja.Value.Year == declaracion.anio && afiliado.FechaBaja.Value.Month >= declaracion.mes))
                    //        {
                    //            sueldos5 += detalle.SueldoBase;
                    //        }
                    //    }
                    //    else if (afiliado.FechaAlta.Year == declaracion.anio && afiliado.FechaAlta.Month <= declaracion.mes)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracion.anio || (afiliado.FechaBaja.Value.Year == declaracion.anio && afiliado.FechaBaja.Value.Month >= declaracion.mes))
                    //        {
                    //            sueldos5 += detalle.SueldoBase;
                    //        }
                    //    }
                    //}
                }

                total2 = TruncateFunction((sueldos2 / 100) * 2, 2);

                total5 = TruncateFunction((sueldos5 / 100) * 5, 2);

                decimal mora = (boleta.RecargoMora != null) ? (decimal)boleta.RecargoMora : 0;

                boleta.TotalDepositado = TruncateFunction(total2 + total5 + mora, 2);

                totalGlobal += boleta.TotalPagado;
                boletasDeAportes.Add(new VmBoletaAportes()
                {
                    IdDeclaracionJurada = boleta.IdDeclaracionJurada.ToString(),
                    RazonSocial = boleta.DeclaracionJurada.Empresa.RazonSocial.ToString(),
                    Cuit = boleta.DeclaracionJurada.Empresa.Cuit.ToString(),
                    Mes = boleta.MesBoleta.ToString(),
                    Anio = boleta.AnioBoleta.ToString(),
                    CantEmpleados = detalles.Count().ToString(),
                    TotalSueldos = sueldos2.ToString(),
                    DosPorc = total2.ToString(),
                    CantAfiliados = detalles.Where(x => x.EmpleadoEmpresa.EsAfiliado).Count().ToString(),
                    TotalSueldosAfiliados = sueldos5.ToString(),
                    CincoPorc = total5.ToString(),
                    CantFamiliaresACargo = "",
                    UnPorcFamiliaresACargo = "",
                    RecargoPorMora = boleta.RecargoMora.ToString(),
                    TotalDepositado = boleta.TotalPagado.ToString(),
                    FechaPago = boleta.FechaPago.ToString()
                });
            }

            ViewBag.TotalGlobal = totalGlobal;
            return new ViewAsPdf(boletasDeAportes)
            {
                FileName = "Boletas-Aportes-Pagadas.pdf",
                PageOrientation = Rotativa.Options.Orientation.Landscape
            };
        }

        [AllowAnonymous]
        public ActionResult ImprimirBoletasPagadas(DateTime? Desde, DateTime? Hasta)
        {
            DateTime Start = (Desde != null) ? (DateTime)Desde : DateTime.Today;
            DateTime End = (Hasta != null) ? (DateTime)Hasta : DateTime.Today;
            ViewBag.Start = Start;
            ViewBag.End = End;

            var boletaAportes = db.BoletaAportes.Include(b => b.DeclaracionJurada).Where(x => x.BoletaPagada == true).ToList();

            foreach (var boleta in boletaAportes)
            {
                DeclaracionJurada declaracion = db.DeclaracionJurada.Find(boleta.IdDeclaracionJurada);

                var detalles = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == declaracion.IdDeclaracionJurada).ToList();
                decimal total2 = 0;
                decimal total5 = 0;
                decimal sueldos2 = 0, sueldos5 = 0;

                foreach (var detalle in detalles)
                {
                    sueldos2 += detalle.Sueldo;
                    if (detalle.SueldoBase > 0)
                    {
                        sueldos5 += detalle.SueldoBase;
                    }
                    #region Recheck Afiliado
                    //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();
                    //if (afiliado != null)
                    //{
                    //    if (afiliado.FechaAlta.Year < declaracion.anio)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracion.anio || (afiliado.FechaBaja.Value.Year == declaracion.anio && afiliado.FechaBaja.Value.Month >= declaracion.mes))
                    //        {
                    //            if (detalle.idJornadaLaboral == 1 || detalle.idJornadaLaboral == 2)
                    //            {
                    //                if (detalle.SueldoBase > 0)
                    //                {
                    //                    sueldos5 += detalle.SueldoBase;
                    //                }
                    //                else
                    //                {
                    //                    sueldos5 += detalle.Sueldo;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                sueldos5 += detalle.Sueldo;
                    //            }
                    //        }
                    //    }
                    //    else if (afiliado.FechaAlta.Year == declaracion.anio && afiliado.FechaAlta.Month <= declaracion.mes)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracion.anio || (afiliado.FechaBaja.Value.Year == declaracion.anio && afiliado.FechaBaja.Value.Month >= declaracion.mes))
                    //        {
                    //            if (detalle.idJornadaLaboral == 1 || detalle.idJornadaLaboral == 2)
                    //            {
                    //                if (detalle.SueldoBase > 0)
                    //                {
                    //                    sueldos5 += detalle.SueldoBase;
                    //                }
                    //                else
                    //                {
                    //                    sueldos5 += detalle.Sueldo;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                sueldos5 += detalle.Sueldo;
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
                }

                total2 = (sueldos2 / 100) * 2;

                total5 = (sueldos5 / 100) * 5;

                boleta.TotalDepositado = TruncateFunction(total2 + total5, 2);
            }

            List<VmBoletaAportes> boletasDeAportes = new List<VmBoletaAportes>();

            foreach (var boleta in boletaAportes)
            {
                if(boleta.FechaPago >= Start)
                {
                    if(boleta.FechaPago <= End)
                    {
                        decimal total2 = TruncateFunction((boleta.TotalSueldos2 / 100) * 2, 2);
                        decimal total5 = TruncateFunction((boleta.TotalSueldos5 / 100) * 5, 2);
                        var detalles = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == boleta.IdDeclaracionJurada).ToList();
                        boletasDeAportes.Add(new VmBoletaAportes()
                        {
                            IdDeclaracionJurada = boleta.IdDeclaracionJurada.ToString(),
                            RazonSocial = boleta.DeclaracionJurada.Empresa.RazonSocial.ToString(),
                            Cuit = boleta.DeclaracionJurada.Empresa.Cuit.ToString(),
                            Mes = boleta.MesBoleta.ToString(),
                            Anio = boleta.AnioBoleta.ToString(),
                            CantEmpleados = detalles.Count().ToString(),
                            TotalSueldos = boleta.TotalSueldos2.ToString(),
                            DosPorc = total2.ToString(),
                            CantAfiliados = detalles.Where(x => x.EmpleadoEmpresa.EsAfiliado).Count().ToString(),
                            TotalSueldosAfiliados = boleta.TotalSueldos5.ToString(),
                            CincoPorc = total5.ToString(),
                            CantFamiliaresACargo = "",
                            UnPorcFamiliaresACargo = "",
                            RecargoPorMora = boleta.RecargoMora.ToString(),
                            TotalDepositado = boleta.TotalPagado.ToString(),
                            FechaPago = boleta.FechaPago.ToString()
                        });
                    }
                }
            }

            return new ViewAsPdf(boletasDeAportes)
            {
                FileName = "Boletas-Aportes-Pagadas.pdf",
                PageOrientation = Rotativa.Options.Orientation.Landscape
            };
        }
        #endregion

        #region DDJJ
        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas, Auxiliar")]
        // GET: DeclaracionesJuradas
        public ActionResult IndexDeclaracionesJuradas(int? mes, int? anio, int estadoPago = 0, int idEmpresa = 0)
        {
            var declaracionJurada = db.DeclaracionJurada.Where(x => x.DeBaja == false).Include(d => d.Empresa);

            //if (idEmpresa != 0)
            //{
                declaracionJurada = declaracionJurada.Where(x => x.idEmpresa == idEmpresa);
            //}

            if (estadoPago == 1)
            {
                //Declaraciones juradas Pagadas
            }

            if (estadoPago == 2)
            {
                //Declaraciones juradas Impagas
            }

            if (mes != null && mes != 0 && anio != null && anio != 0)
            {
                declaracionJurada = declaracionJurada.Where(x => x.mes == mes && x.anio == anio);
            }

            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            List<Empresa> empresas = db.Empresa.OrderBy(x => x.RazonSocial).ToList();
            empresas.Insert(0, new Empresa { IdEmpresa = 0, RazonSocial = "Todas" });
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial", idEmpresa);
            ViewBag.estadoPago = estadoPago;

            return View(declaracionJurada.OrderByDescending(x => x.anio).ThenByDescending(x => x.mes).ToList());
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas, Auxiliar")]
        // GET: DeclaracionesJuradas/Details/5
        public ActionResult DetailsDeclaracionesJuradas(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeclaracionJurada declaracionJurada = db.DeclaracionJurada.Find(id);
            if (declaracionJurada == null)
            {
                return HttpNotFound();
            }
            return View(declaracionJurada);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas, Auxiliar")]
        // GET: DetallesDeclaracionJurada
        public ActionResult IndexDetallesDeclaracionJurada(string sortOrder, string currentFilter, string searchString, int? page, int id)
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

            idDeclaracionJurada = id;

            var ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == id).FirstOrDefault();

            ViewBag.Mes = ddjj.mes;
            ViewBag.Anio = ddjj.anio;

            var detalleDeclaracionJurada = (db.DetalleDeclaracionJurada.Include(d => d.Categoria)
                                                                       .Include(d => d.DeclaracionJurada)
                                                                       .Include(d => d.EmpleadoEmpresa)
                                                                       .Include(d => d.Jornada)
                                                                       .Where(x => x.IdDeclaracionJurada == id /*&& x.EmpleadoEmpresa.FechaBaja == null*/));
            ViewBag.IdDeclaracionJurada = id;

            if (!string.IsNullOrEmpty(searchString))
            {
                detalleDeclaracionJurada = detalleDeclaracionJurada.Where(x => x.EmpleadoEmpresa.Empleado.Apellido.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    detalleDeclaracionJurada = detalleDeclaracionJurada.OrderByDescending(x => x.EmpleadoEmpresa.Empleado.Apellido);
                    break;
                default:
                    detalleDeclaracionJurada = detalleDeclaracionJurada.OrderBy(x => x.EmpleadoEmpresa.Empleado.Apellido);
                    break;
            }

            int pageSize = (detalleDeclaracionJurada.Count() > 0)? detalleDeclaracionJurada.Count() : 1;
            int pageNumber = (page ?? 1);

            if (detalleDeclaracionJurada.ToList().Count > 0)
            {
                decimal sueldos2 = 0, sueldos5 = 0;
                foreach (DetalleDeclaracionJurada detalle in detalleDeclaracionJurada)
                {
                    detalle.EsAfiliado = false;
                    sueldos2 += detalle.Sueldo;
                    if (detalle.SueldoBase > 0)
                    {
                        sueldos5 += detalle.SueldoBase;
                        detalle.EsAfiliado = true;
                    }
                    //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();
                    //if (afiliado != null)
                    //{
                    //    if (afiliado.FechaAlta.Year < ddjj.anio)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                    //        {
                    //            sueldos5 += detalle.SueldoBase;
                    //            detalle.EsAfiliado = true;
                    //        }
                    //    }
                    //    else if (afiliado.FechaAlta.Year == ddjj.anio && afiliado.FechaAlta.Month <= ddjj.mes)
                    //    {
                    //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                    //        {
                    //            sueldos5 += detalle.SueldoBase;
                    //            detalle.EsAfiliado = true;
                    //        }
                    //    }
                    //}

                    detalle.LicenciaEmpleado = false;
                    foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa))
                    {
                        #region Old
                        //if (licencia != null && licencia.FechaAltaLicencia.Year == detalle.DeclaracionJurada.anio)
                        //{
                        //    if (licencia.FechaBajaLicencia.Value.Year == detalle.DeclaracionJurada.anio)
                        //    {
                        //        if (licencia.FechaAltaLicencia.Month <= detalle.DeclaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
                        //        {
                        //            detalle.LicenciaEmpleado = true;
                        //        }
                        //    }
                        //    if (licencia.FechaBajaLicencia.Value.Year > detalle.DeclaracionJurada.anio)
                        //    {
                        //        if (licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
                        //        {
                        //            detalle.LicenciaEmpleado = true;
                        //        }
                        //    }
                        //}
                        //if (licencia.FechaBajaLicencia.Value.Year >= detalle.DeclaracionJurada.anio)
                        //{
                        //    if (licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
                        //    {
                        //        detalle.LicenciaEmpleado = true;
                        //    }
                        //}
                        #endregion

                        if (licencia.IdLicenciaLaboral != 3)
                        {
                            if (licencia.FechaAltaLicencia.Year < detalle.DeclaracionJurada.anio ||
                                (licencia.FechaAltaLicencia.Year == detalle.DeclaracionJurada.anio && licencia.FechaAltaLicencia.Month <= detalle.DeclaracionJurada.mes))
                            {
                                if (licencia.FechaBajaLicencia.HasValue)
                                {
                                    if (licencia.FechaBajaLicencia.Value.Year == detalle.DeclaracionJurada.anio)
                                    {
                                        if (licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
                                        {
                                            detalle.LicenciaEmpleado = true;
                                        }
                                    }
                                    else if (licencia.FechaBajaLicencia.Value.Year > detalle.DeclaracionJurada.anio)
                                    {
                                        detalle.LicenciaEmpleado = true;
                                    }
                                }
                                else
                                {
                                    detalle.LicenciaEmpleado = true;
                                }
                            }
                        }
                        else
                        {

                            if (detalle.DeclaracionJurada.mes != 1)
                            {
                                if (licencia.FechaBajaLicencia.HasValue)
                                {
                                    if (licencia.FechaBajaLicencia.Value.Year == detalle.DeclaracionJurada.anio)
                                    {
                                        if (licencia.FechaBajaLicencia.Value.Month == detalle.DeclaracionJurada.mes)
                                        {
                                            detalle.LicenciaEmpleado = true;
                                        }
                                        else if (licencia.FechaBajaLicencia.Value.Month == (detalle.DeclaracionJurada.mes - 1))
                                        {
                                            detalle.LicenciaEmpleado = true;
                                        }
                                    }
                                }
                                else
                                {
                                    detalle.LicenciaEmpleado = true;
                                }
                            }
                            else
                            {
                                if (licencia.FechaBajaLicencia.HasValue)
                                {
                                    if (licencia.FechaBajaLicencia.Value.Year == detalle.DeclaracionJurada.anio)
                                    {
                                        if (licencia.FechaBajaLicencia.Value.Month == 1)
                                        {
                                            detalle.LicenciaEmpleado = true;
                                        }
                                    }
                                    else if (licencia.FechaBajaLicencia.Value.Year == (detalle.DeclaracionJurada.anio - 1))
                                    {
                                        if (licencia.FechaBajaLicencia.Value.Month == 12)
                                        {
                                            detalle.LicenciaEmpleado = true;
                                        }
                                    }
                                }
                                else
                                {
                                    detalle.LicenciaEmpleado = true;
                                }
                            }
                        }
                    }
                    var LiquidacionProporcional = db.LiquidacionProporcionalEmpleado.Where(x => x.IdDetalleDeclaracionJurada == detalle.IdDetalleDeclaracionJurada).FirstOrDefault();
                    detalle.LiquidacionProporcional = (LiquidacionProporcional != null) ? true : false;
                    detalle.IdLiquidacionProporcional = detalle.IdLiquidacionProporcional;
                }
                
                ViewBag.Sueldos2 = sueldos2;
                ViewBag.Sueldos5 = sueldos5;
                return View(detalleDeclaracionJurada.ToPagedList(pageNumber, pageSize));
            }
            return View(detalleDeclaracionJurada.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas, Auxiliar")]
        // GET: DetallesDeclaracionJurada/Details/5
        public ActionResult DetailsDetallesDeclaracionJurada(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DetalleDeclaracionJurada detalleDeclaracionJurada = db.DetalleDeclaracionJurada.Find(id);
            if (detalleDeclaracionJurada == null)
            {
                return HttpNotFound();
            }
            ViewBag.idDeclaracion = idDeclaracionJurada;
            return View(detalleDeclaracionJurada);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Empresas/BoletaAportes/Delete/5
        public ActionResult DeleteDeclaracionJurada(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeclaracionJurada declaracionJurada = db.DeclaracionJurada.Find(id);
            if (declaracionJurada == null)
            {
                return HttpNotFound();
            }

            var boletaAportes = db.BoletaAportes.Where(x => x.IdDeclaracionJurada == id && x.DeBaja == false).FirstOrDefault();

            if (boletaAportes != null)
            {
                return RedirectToAction("CantDeleteDeclaracionJuradaMessage");
            }

            return View(declaracionJurada);
        }

        // POST: Empresas/BoletaAportes/Delete/5
        [HttpPost, ActionName("DeleteDeclaracionJurada")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDeclaracionJuradaConfirmed(int id)
        {
            DeclaracionJurada declaracionJurada = db.DeclaracionJurada.Find(id);

            var boletaAportes = db.BoletaAportes.Where(x => x.IdDeclaracionJurada == id && x.DeBaja == false).FirstOrDefault();

            if (boletaAportes != null)
            {
                return RedirectToAction("CantDeleteDeclaracionJuradaMessage");
            }

            declaracionJurada.DeBaja = true;
            declaracionJurada.FechaBaja = DateTime.Now;
            declaracionJurada.UserId = User.Identity.GetUserId();

            //db.BoletaAportes.Remove(boletaAportes);
            db.SaveChanges();
            return RedirectToAction("DeleteBoletaAportesMessage");
        }

        public ActionResult DeleteDeclaracionJuradaMessage()
        {
            return View();
        }

        public ActionResult CantDeleteDeclaracionJuradaMessage()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ImpresionDDJJPorEmpresas(int id)
        {
            idDeclaracionJurada = id;
            var declaracionJurada = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == id).FirstOrDefault();

            ViewBag.Periodo = declaracionJurada.mes + "/" + declaracionJurada.anio;

            var empresa = db.Empresa.Where(x => x.IdEmpresa == declaracionJurada.idEmpresa).FirstOrDefault();
            ViewBag.RazonSocial = empresa.RazonSocial;
            ViewBag.Nro = empresa.Altura;
            ViewBag.Cuit = empresa.Cuit;
            ViewBag.Calle = empresa.Calle;
            ViewBag.LocalidadEmpresa = empresa.Localidad.Nombre;
            ViewBag.ProvinciaEmpresa = empresa.Localidad.Provincia.Nombre;


            decimal totalSueldos = 0, totalAportes = 0, totalSueldosBase = 0;
            var detalleDeclaracionJurada = (db.DetalleDeclaracionJurada.Include(d => d.Categoria)
                                                                       .Include(d => d.DeclaracionJurada)
                                                                       .Include(d => d.EmpleadoEmpresa)
                                                                       .Include(d => d.Jornada)
                                                                       .Where(x => x.IdDeclaracionJurada == id && x.EmpleadoEmpresa.FechaBaja == null));
            ViewBag.IdDeclaracionJurada = id;

            foreach (DetalleDeclaracionJurada detalle in detalleDeclaracionJurada)
            {
                totalSueldos += Math.Round(detalle.Sueldo, 2);
                if (detalle.SueldoBase > 0)
                {
                    totalAportes += Math.Round(((detalle.SueldoBase / 100) * 5), 2);
                    detalle.EsAfiliado = true;
                }
                //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();
                //if (afiliado != null)
                //{
                //    if (afiliado.FechaAlta.Year < declaracionJurada.anio)
                //    {
                //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
                //        {
                //            if(detalle.idJornadaLaboral == 1 || detalle.idJornadaLaboral == 2)
                //            {
                //                totalAportes += Math.Round(((detalle.SueldoBase / 100) * 5), 2);
                //            }
                //            else
                //            {
                //                totalAportes += Math.Round(((detalle.Sueldo / 100) * 5), 2);
                //            }
                //            totalSueldosBase += detalle.SueldoBase;
                //            detalle.EsAfiliado = true;
                //        }
                //    }
                //    else if (afiliado.FechaAlta.Year == declaracionJurada.anio && afiliado.FechaAlta.Month <= declaracionJurada.mes)
                //    {
                //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
                //        {
                //            if (detalle.idJornadaLaboral == 1 || detalle.idJornadaLaboral == 2)
                //            {
                //                totalAportes += Math.Round(((detalle.SueldoBase / 100) * 5), 2);
                //            }
                //            else
                //            {
                //                totalAportes += Math.Round(((detalle.Sueldo / 100) * 5), 2);
                //            }
                //            totalSueldosBase += detalle.SueldoBase;
                //            detalle.EsAfiliado = true;
                //        }
                //    }
                //}
                //else
                //{
                //    totalAportes += Math.Round(((detalle.Sueldo / 100) * 2), 2);
                //    detalle.EsAfiliado = false;
                //}
                detalle.LicenciaEmpleado = false;
                foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa))
                {
                    if (licencia != null && licencia.FechaAltaLicencia.Year == detalle.DeclaracionJurada.anio)
                    {
                        if (licencia.FechaBajaLicencia.HasValue)
                        {
                            if (licencia.FechaBajaLicencia.Value.Year == detalle.DeclaracionJurada.anio)
                            {
                                if (licencia.FechaAltaLicencia.Month <= detalle.DeclaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
                                {
                                    detalle.LicenciaEmpleado = true;
                                }
                            }
                            if (licencia.FechaBajaLicencia.Value.Year > detalle.DeclaracionJurada.anio)
                            {
                                if (licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
                                {
                                    detalle.LicenciaEmpleado = true;
                                }
                            }
                        }
                        else
                        {
                            detalle.LicenciaEmpleado = true;
                        }
                    }
                    if (licencia.FechaBajaLicencia.HasValue)
                    {
                        if (licencia.FechaBajaLicencia.Value.Year >= detalle.DeclaracionJurada.anio)
                        {
                            if (licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
                            {
                                detalle.LicenciaEmpleado = true;
                            }
                        }
                    }
                    else
                    {
                        detalle.LicenciaEmpleado = true;
                    }
                }
                var LiquidacionProporcional = db.LiquidacionProporcionalEmpleado.Where(x => x.IdDetalleDeclaracionJurada == detalle.IdDetalleDeclaracionJurada).FirstOrDefault();
                detalle.LiquidacionProporcional = (LiquidacionProporcional != null) ? true : false;
                detalle.IdLiquidacionProporcional = detalle.IdLiquidacionProporcional;
            }
            ViewBag.TotalSueldos = totalSueldos;
            ViewBag.TotalSueldosBase = totalSueldosBase;
            ViewBag.TotalAportes = totalAportes;

            return View(detalleDeclaracionJurada.ToList());
        }
        
        [AllowAnonymous]
        public ActionResult ImprimirDDJJPorEmpresas(int id)
        {
            return new ActionAsPdf("ImpresionDDJJPorEmpresas", new { id })
            {
                FileName = "Declaracion-Jurada.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                MinimumFontSize = 12
            };
        }
        
        [AllowAnonymous]
        public ActionResult ImprimirFaecys(int? Mes, int? Anio)
        {
            int MesFaecys = (Mes != null) ? (int)Mes : DateTime.Today.Month;
            int AnioFaecys = (Anio != null) ? (int)Anio : DateTime.Today.Year;
            ViewBag.Mes = Mes;
            ViewBag.Anio = Anio;
            switch (Mes)
            {
                case 1:
                    ViewBag.MesDeclarado = "Enero";
                    break;
                case 2:
                    ViewBag.MesDeclarado = "Febrero";
                    break;
                case 3:
                    ViewBag.MesDeclarado = "Marzo";
                    break;
                case 4:
                    ViewBag.MesDeclarado = "Abril";
                    break;
                case 5:
                    ViewBag.MesDeclarado = "Mayo";
                    break;
                case 6:
                    ViewBag.MesDeclarado = "Junio";
                    break;
                case 7:
                    ViewBag.MesDeclarado = "Julio";
                    break;
                case 8:
                    ViewBag.MesDeclarado = "Agosto";
                    break;
                case 9:
                    ViewBag.MesDeclarado = "Septiembre";
                    break;
                case 10:
                    ViewBag.MesDeclarado = "Octubre";
                    break;
                case 11:
                    ViewBag.MesDeclarado = "Noviembre";
                    break;
                case 12:
                    ViewBag.MesDeclarado = "Diciembre";
                    break;
            };
            ViewBag.AnioDeclarado = AnioFaecys;
            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            var DetallesDeclaracionJuradas = db.DetalleDeclaracionJurada.Include(t => t.EmpleadoEmpresa)
                                                                        .Include(t => t.DeclaracionJurada)
                                                                        .Where(x => x.DeclaracionJurada.mes == Mes &&
                                                                                    x.DeclaracionJurada.anio == Anio)
                                                                        .Select(p =>
                                                                                    new VmEmpleados
                                                                                    {
                                                                                        CuilEmpleado = p.EmpleadoEmpresa.Empleado.Cuil,
                                                                                        NombreEmpleado = p.EmpleadoEmpresa.Empleado.Apellido + " " + p.EmpleadoEmpresa.Empleado.Nombre,
                                                                                        NombreEmpresa = p.EmpleadoEmpresa.Empresa.RazonSocial,
                                                                                        CuitEmpresa = p.EmpleadoEmpresa.Empresa.Cuit
                                                                                    }
                                                                                ).ToList();

            ViewBag.CantidadDeEmpleados = DetallesDeclaracionJuradas.Count();

            return new ViewAsPdf(DetallesDeclaracionJuradas.OrderBy(x => x.NombreEmpresa).ThenBy(x => x.NombreEmpleado))
            {
                FileName = "DDJJ " + MesFaecys + " " + AnioFaecys + " FAECYT.pdf"
            };
        }
        #endregion

        #region Empresas
        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Empresa/Empresas
        public ActionResult IndexEmpresa(string sortOrder, string currentFilter, string searchString, int? page, int searchType = 1, int idLocalidad = 0, int idProvincia = 0, int idActividad = 0, int CantMinEmpleados = 0)
        {
            ViewBag.RazonSocialSortParm = string.IsNullOrEmpty(sortOrder) ? "razonSocial_desc" : "";
            ViewBag.NombreFantasiaSortParm = sortOrder == "nombreFantasia" ? "nombreFantasia_desc" : "nombreFantasia";
            ViewBag.LocalidadSortParm = sortOrder == "localidad" ? "localidad_desc" : "localidad";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.searchString = searchString;
            ViewBag.SearchType = searchType;
            ViewBag.IdLocalidadSeleccionada = idLocalidad;
            ViewBag.IdProvinciaSeleccionada = idProvincia;
            ViewBag.IdActividadSeleccionada = idActividad;
            ViewBag.CantMinEmpleados = CantMinEmpleados;

            #region Provincias
            var provincias = db.Provincia.ToList();

            Provincia provinciaDefault = new Provincia();
            provinciaDefault.IdProvincia = 0;
            provinciaDefault.Nombre = "seleccione una Provincia...";
            provincias.Insert(0, provinciaDefault);

            if (idProvincia != 0)
            {
                ViewBag.idProvincia = new SelectList(provincias, "IdProvincia", "Nombre", idProvincia);
            }
            else
            {
                ViewBag.idProvincia = new SelectList(provincias, "IdProvincia", "Nombre");
            }
            #endregion

            #region Localidades
            var localidades = db.Localidad.ToList();
            if (idProvincia != 0) { localidades = localidades.Where(x => x.IdProvincia == idProvincia).ToList(); }

            Localidad localidadDefault = new Localidad();
            localidadDefault.IdLocalidad = 0;
            localidadDefault.Nombre = "seleccione una ciudad...";
            localidades.Insert(0, localidadDefault);

            foreach (var localidad in localidades)
            {
                if (localidad.IdLocalidad != 0)
                {
                    localidad.Nombre = localidad.Nombre + " (" + localidad.CodPostal + ")";
                }
            }

            if (idLocalidad != 0)
            {
                ViewBag.idLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre", idLocalidad);
            }
            else
            {
                ViewBag.idLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre");
            }
            #endregion

            #region Actividades
            //var actividades = db.Actividad.ToList();

            //Actividad actividadDefault = new Actividad();
            //actividadDefault.IdActividad = 0;
            //actividadDefault.Nombre = "seleccione una Actividad...";
            //actividades.Insert(0, actividadDefault);

            //if (idActividad != 0)
            //{
            //    ViewBag.idActividad = new SelectList(actividades, "IdActividad", "Nombre", idActividad);
            //}
            //else
            //{
            //    ViewBag.idActividad = new SelectList(actividades, "IdActividad", "Nombre");
            //}
            #endregion

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            List<Empresa> listadoEmpresas = new List<Empresa>();
            var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);

            if (idProvincia != 0)
            {
                empresa = empresa.Where(x => x.Localidad.IdProvincia == idProvincia);
            }

            if (idLocalidad != 0)
            {
                empresa = empresa.Where(x => x.IdLocalidad == idLocalidad);
            }

            if(CantMinEmpleados != 0)
            {
                DateTime hoy = DateTime.Today;
                foreach (var empr in empresa)
                {
                    if(db.EmpleadoEmpresa.Where(x => x.idEmpresa == empr.IdEmpresa &&
                                                     (x.FechaBaja == null || x.FechaBaja > hoy)).Count() >= CantMinEmpleados)
                    {
                        listadoEmpresas.Add(empr);
                    }
                }
            }
            else
            {
                listadoEmpresas.AddRange(empresa.ToList());
            }
            //if (idActividad != 0)
            //{
            //    empresa = empresa.Where(x => x.IdActividad == idActividad);
            //}

            if (!string.IsNullOrEmpty(searchString))
            {
                if(searchType == 1)
                {
                    listadoEmpresas = listadoEmpresas.Where(x => x.RazonSocial.ToUpper().Contains(searchString.ToUpper())).ToList();
                }
                else
                {
                    listadoEmpresas = listadoEmpresas.Where(x => x.Cuit.ToUpper().Contains(searchString.ToUpper())).ToList();
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    listadoEmpresas = listadoEmpresas.OrderByDescending(x => x.RazonSocial).ToList();
                    break;
                case "nombreFantasia":
                    listadoEmpresas = listadoEmpresas.OrderBy(s => s.NombreFantasia).ToList();
                    break;
                case "nombreFantasia_desc":
                    listadoEmpresas = listadoEmpresas.OrderByDescending(s => s.NombreFantasia).ToList();
                    break;
                case "localidad":
                    listadoEmpresas = listadoEmpresas.OrderBy(s => s.Localidad.Nombre).ToList();
                    break;
                case "localidad_desc":
                    listadoEmpresas = listadoEmpresas.OrderByDescending(s => s.Localidad.Nombre).ToList();
                    break;
                default:
                    listadoEmpresas = listadoEmpresas.OrderBy(x => x.RazonSocial).ToList();
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(listadoEmpresas.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult IndexEmpresasSinPagos(int? page)
        {
            var IdEmpresasConPagos = (from oEmpresas in db.Empresa
                                      join oDeclaraciones in db.DeclaracionJurada on oEmpresas.IdEmpresa equals oDeclaraciones.idEmpresa
                                      join oBoletas in db.BoletaAportes on oDeclaraciones.IdDeclaracionJurada equals oBoletas.IdDeclaracionJurada
                                      where oBoletas.BoletaPagada == true
                                      select oEmpresas.IdEmpresa).ToList();

            var EmpresasSinPagos = (from oEmpresas in db.Empresa
                                    where !IdEmpresasConPagos.Contains(oEmpresas.IdEmpresa)
                                    select oEmpresas).ToList();

            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(EmpresasSinPagos.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Notificaciones(int? page)
        {
            List<VmListadoNotificaciones> notificaciones = (from oNotificaciones in db.Notificaciones
                                                            join oNotificacionesEmpresa in db.NotificacionesEmpresa on oNotificaciones.IdNotificacion equals oNotificacionesEmpresa.IdNotificacion
                                                            join oEmpresa in db.Empresa on oNotificacionesEmpresa.idEmpresa equals oEmpresa.IdEmpresa
                                                            select new VmListadoNotificaciones
                                                            {
                                                                ID = oNotificaciones.IdNotificacion,
                                                                EmpresaId = oNotificacionesEmpresa.idEmpresa,
                                                                RazonSocial = oEmpresa.RazonSocial,
                                                                Fecha = oNotificaciones.Fecha,
                                                                Titulo = oNotificaciones.Titulo,
                                                                Visto = oNotificacionesEmpresa.Visto,
                                                                FechaVisto = oNotificacionesEmpresa.FechaVisto
                                                            }).ToList();
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(notificaciones.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult DetalleNotificacion(int Id)
        {
            VmNotificacion notificacion = (from oNotificaciones in db.Notificaciones
                                           join oNotificacionesEmpresa in db.NotificacionesEmpresa on oNotificaciones.IdNotificacion equals oNotificacionesEmpresa.IdNotificacion
                                           join oEmpresa in db.Empresa on oNotificacionesEmpresa.idEmpresa equals oEmpresa.IdEmpresa
                                           where oNotificaciones.IdNotificacion == Id
                                           select new VmNotificacion
                                           {
                                               idNotificacion = oNotificaciones.IdNotificacion,
                                               idEmpresa = oNotificacionesEmpresa.idEmpresa,
                                               RazonSocial = oEmpresa.RazonSocial,
                                               Fecha = oNotificaciones.Fecha,
                                               Titulo = oNotificaciones.Titulo,
                                               Descripcion = oNotificaciones.Descripcion,
                                               Visto = oNotificacionesEmpresa.Visto,
                                               FechaVisto = oNotificacionesEmpresa.FechaVisto
                                           }).FirstOrDefault();

            if (notificacion == null)
            {
                return HttpNotFound();
            }

            notificacion.ListadoAdjuntos = db.AdjuntosNotificacion.Where(x => x.idNotificacion == notificacion.idNotificacion).ToList();

            return View(notificacion);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        public ActionResult NotificarEmpresa(int? id)
        {
            string userId = User.Identity.GetUserId();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empresa empresa = db.Empresa.Find(id);
            if (empresa == null)
            {
                return HttpNotFound();
            }

            List<Empresa> empresas = db.Empresa.Where(x => x.IdEmpresa == empresa.IdEmpresa).ToList();
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial");

            VmNotificacion notificacion = new VmNotificacion();

            notificacion.idEmpresa = empresa.IdEmpresa;
            notificacion.Fecha = DateTime.Now;
            notificacion.UserId = userId;
            notificacion.Visto = false;

            return View(notificacion);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotificarEmpresa(VmNotificacion notificacion)
        {
            if (ModelState.IsValid)
            {
                Notificacion nuevaNotificacion = new Notificacion();

                nuevaNotificacion.Fecha = DateTime.Now;
                nuevaNotificacion.UserId = notificacion.UserId;
                nuevaNotificacion.Titulo = notificacion.Titulo;
                nuevaNotificacion.Descripcion = notificacion.Descripcion;

                db.Notificaciones.Add(nuevaNotificacion);
                db.SaveChanges();

                NotificacionEmpresa notificacionEmpresa = new NotificacionEmpresa();

                notificacionEmpresa.IdNotificacion = nuevaNotificacion.IdNotificacion;
                notificacionEmpresa.idEmpresa = notificacion.idEmpresa;
                notificacionEmpresa.Visto = false;

                db.NotificacionesEmpresa.Add(notificacionEmpresa);
                db.SaveChanges();

                foreach (var adjunto in notificacion.Adjuntos)
                {
                    if(adjunto != null)
                    {
                        string fecha = DateTime.Today.ToShortDateString();
                        fecha = fecha.Replace('/', '-');
                        string fileName = nuevaNotificacion.IdNotificacion + " - " + notificacion.Titulo + " - " + adjunto.FileName;
                        string path = Path.Combine(Server.MapPath("~/Areas/Administrador/Content/AdjuntosNotificaciones"),
                                        Path.GetFileName(fileName));
                        if (!System.IO.File.Exists(path))
                        {
                            adjunto.SaveAs(path);
                            db.AdjuntosNotificacion.Add(new AdjuntoNotificacion()
                            {
                                idNotificacion = nuevaNotificacion.IdNotificacion,
                                Adjunto = fileName
                            });

                            db.SaveChanges();
                        }
                    }
                }

                return RedirectToAction("IndexEmpresa");
            }

            List<Empresa> empresas = db.Empresa.Where(x => x.IdEmpresa == notificacion.idEmpresa).ToList();
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial", notificacion.idEmpresa);

            return View(notificacion);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        public ActionResult NotificarEmpresas(string idSeleccionados = "null")
        {
            List<int> id = new List<int>();
            List<BoletaAportes> BoletasAportesPagadas = new List<BoletaAportes>();
            if (!string.IsNullOrEmpty(idSeleccionados))
            {
                string llave = idSeleccionados.Substring(0, 1);
                if (llave == "[")
                {
                    id.AddRange(System.Web.Helpers.Json.Decode<List<int>>(idSeleccionados));
                }
                else
                {
                    id.Add(System.Web.Helpers.Json.Decode<int>(idSeleccionados));
                }
            }

            string userId = User.Identity.GetUserId();
            if (id == null || id.Count < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<Empresa> empresa = db.Empresa.Where(x => id.Contains(x.IdEmpresa)).ToList();
            if (empresa == null || empresa.Count < 1)
            {
                return HttpNotFound();
            }

            VmNotificacionEmpresas notificacion = new VmNotificacionEmpresas();

            notificacion.idEmpresaSeleccionado = idSeleccionados;
            notificacion.idEmpresa = id;
            notificacion.Fecha = DateTime.Now;
            notificacion.UserId = userId;
            notificacion.Visto = false;

            return View(notificacion);
        }

        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotificarEmpresas(VmNotificacionEmpresas notificaciones)
        {
            string userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                notificaciones.idEmpresa = new List<int>();
                string llave = notificaciones.idEmpresaSeleccionado.Substring(0, 1);
                if (llave == "[")
                {
                    notificaciones.idEmpresa.AddRange(System.Web.Helpers.Json.Decode<List<int>>(notificaciones.idEmpresaSeleccionado));
                }
                else
                {
                    notificaciones.idEmpresa.Add(System.Web.Helpers.Json.Decode<int>(notificaciones.idEmpresaSeleccionado));
                }

                Notificacion nuevaNotificacion = new Notificacion();

                nuevaNotificacion.Fecha = DateTime.Now;
                nuevaNotificacion.UserId = userId;
                nuevaNotificacion.Titulo = notificaciones.Titulo;
                nuevaNotificacion.Descripcion = notificaciones.Descripcion;

                db.Notificaciones.Add(nuevaNotificacion);
                db.SaveChanges();

                foreach (var adjunto in notificaciones.Adjuntos)
                {
                    if (adjunto != null)
                    {
                        string fecha = DateTime.Today.ToShortDateString();
                        fecha = fecha.Replace('/', '-');
                        string fileName = nuevaNotificacion.IdNotificacion + " - " + nuevaNotificacion.Titulo + " - " + adjunto.FileName;
                        string path = Path.Combine(Server.MapPath("~/Areas/Administrador/Content/AdjuntosNotificaciones"),
                                        Path.GetFileName(fileName));
                        if (!System.IO.File.Exists(path))
                        {
                            adjunto.SaveAs(path);
                            db.AdjuntosNotificacion.Add(new AdjuntoNotificacion()
                            {
                                idNotificacion = nuevaNotificacion.IdNotificacion,
                                Adjunto = fileName
                            });

                            db.SaveChanges();
                        }
                    }
                }

                foreach (var IdEmpresa in notificaciones.idEmpresa)
                {
                    NotificacionEmpresa notificacionEmpresa = new NotificacionEmpresa();

                    notificacionEmpresa.IdNotificacion = nuevaNotificacion.IdNotificacion;
                    notificacionEmpresa.idEmpresa = IdEmpresa;
                    notificacionEmpresa.Visto = false;

                    db.NotificacionesEmpresa.Add(notificacionEmpresa);
                    db.SaveChanges();
                }

                return RedirectToAction("IndexEmpresa");
            }

            return View(notificaciones);
        }


        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        public ActionResult DetailsEmpresa(int? id)
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

        [Authorize(Roles = "Admin, Fiscalizacion")]
        public ActionResult Usuarios(int? page, string currentFilter, string searchString)
        {
            List<VmUsuario> usuarios = new List<VmUsuario>();
            var users = context.Users;


            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                foreach (var user in users)
                {
                    if (user.Email == "admin@gmail.com")
                    {
                        continue;
                    }
                    var claims = user.Claims.Where(x => x.ClaimType == "IdEmpresa").FirstOrDefault();

                    int idEmpresa = 0;

                    if (claims != null)
                    {
                        int.TryParse(claims.ClaimValue, out idEmpresa);
                    }

                    var empresa = db.Empresa.Find(idEmpresa);

                    if (empresa == null)
                    {
                        continue;
                    }

                    if (empresa.FechaBajaEmpresa != null)
                    {
                        continue;
                    }

                    VmUsuario usuario = new VmUsuario();

                    usuario.Id = user.Id;
                    usuario.IdEmpresa = (empresa != null) ? empresa.IdEmpresa : 0;
                    usuario.RazonSocial = (empresa != null) ? empresa.RazonSocial : "";
                    usuario.Email = user.Email;
                    usuario.Cuit = (empresa != null) ? empresa.Cuit : "";
                    usuario.Telefono = (empresa != null) ? empresa.TelefonoFijo : "";
                    usuario.Celular = (empresa != null) ? empresa.TelefonoCelular : "";

                    usuarios.Add(usuario);
                }

                usuarios = usuarios.Where(x => x.RazonSocial.ToUpper().Contains(searchString.ToUpper().Trim())).ToList();
            }

            int pageSize = 7;
            int pageNumber = (page ?? 1);

            ViewBag.Page = page;
            ViewBag.CurrentFilter = searchString;

            return View(usuarios.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin, Fiscalizacion")]
        public ActionResult ActualizarUsuario(string id, int page, string currentFilter)
        {
            var user = context.Users.Where(x => x.Id == id).FirstOrDefault();

            var claims = user.Claims.Where(x => x.ClaimType == "IdEmpresa").FirstOrDefault();

            int idEmpresa = 0;

            if(claims != null)
            {
                int.TryParse(claims.ClaimValue, out idEmpresa);
            }

            var empresa = db.Empresa.Find(idEmpresa);

            VmUsuario usuario = new VmUsuario();

            usuario.Id = user.Id;
            usuario.IdEmpresa = (empresa != null) ? empresa.IdEmpresa : 0;
            usuario.RazonSocial = (empresa != null) ? empresa.RazonSocial : "";
            usuario.NombreFantasia = (empresa != null) ? empresa.NombreFantasia : "";
            usuario.Email = user.Email;
            usuario.Cuit = (empresa != null) ? empresa.Cuit : "";
            usuario.Telefono = (empresa != null) ? empresa.TelefonoFijo : "";
            usuario.Celular = (empresa != null) ? empresa.TelefonoCelular : "";
            usuario.Page = page;
            usuario.CurrentFilter = currentFilter;

            return View(usuario);
        }

        [Authorize(Roles = "Admin, Fiscalizacion")]
        [HttpPost]
        public ActionResult ActualizarUsuario(VmUsuario usuario)
        {
            if(string.IsNullOrEmpty(usuario.Email.Trim()))
            {
                ModelState.AddModelError("Email", "El EMail no puede estar vacio");
                return View(usuario);
            }
            var user = context.Users.Where(x => x.Id == usuario.Id).FirstOrDefault();

            if(usuario.Email != user.Email)
            {
                user.Email = usuario.Email;
                user.EmailConfirmed = true;
                context.SaveChanges();
            }

            var empresa = db.Empresa.Find(usuario.IdEmpresa);

            if (!string.IsNullOrEmpty(usuario.RazonSocial) && usuario.RazonSocial.Trim() != empresa.RazonSocial.Trim())
            {
                empresa.RazonSocial = usuario.RazonSocial;
            }

            if (!string.IsNullOrEmpty(usuario.NombreFantasia) && usuario.NombreFantasia.Trim() != empresa.NombreFantasia.Trim())
            {
                empresa.NombreFantasia = usuario.NombreFantasia;
            }

            if (!string.IsNullOrEmpty(usuario.Celular) && usuario.Celular.Trim() != empresa.TelefonoCelular.Trim())
            {
                empresa.TelefonoCelular = usuario.Celular;
            }

            if (!string.IsNullOrEmpty(usuario.Telefono) && usuario.Telefono.Trim() != empresa.TelefonoFijo.Trim())
            {
                empresa.TelefonoFijo = usuario.Telefono;
            }

            db.SaveChanges();

            return RedirectToAction("Usuarios", new { page = usuario.Page, currentFilter = usuario.CurrentFilter });
        }
        
        [AllowAnonymous]
        public ActionResult ImpresionEmpresas(string sortOrder, string searchString, int IdLocalidadSeleccionada = 0, int IdProvinciaSeleccionada = 0, int IdActividadSeleccionada = 0, int CantMinEmpleados = 0)
        {
            List<VmEmpresa> empresas = new List<VmEmpresa>();

            VmEmpresa tvm;

            //Datos de la Empresa

            List<Empresa> listadoEmpresas = new List<Empresa>();
            var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);

            if (IdProvinciaSeleccionada != 0)
            {
                empresa.Where(x => x.Localidad.IdProvincia == IdProvinciaSeleccionada);
            }

            if (IdLocalidadSeleccionada != 0)
            {
                empresa = empresa.Where(x => x.IdLocalidad == IdLocalidadSeleccionada);
            }

            //if (IdActividadSeleccionada != 0)
            //{
            //    empresa = empresa.Where(x => x.IdActividad == IdActividadSeleccionada);
            //}

            if (!string.IsNullOrEmpty(searchString))
            {
                empresa = empresa.Where(x => x.RazonSocial.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    empresa = empresa.OrderByDescending(x => x.RazonSocial);
                    break;
                case "nombreFantasia":
                    empresa = empresa.OrderBy(s => s.NombreFantasia);
                    break;
                case "nombreFantasia_desc":
                    empresa = empresa.OrderByDescending(s => s.NombreFantasia);
                    break;
                case "localidad":
                    empresa = empresa.Include(t => t.Localidad).OrderBy(s => s.Localidad.Nombre);
                    break;
                case "localidad_desc":
                    empresa = empresa.Include(t => t.Localidad).OrderByDescending(s => s.Localidad.Nombre);
                    break;
                default:
                    empresa = empresa.OrderBy(x => x.RazonSocial);
                    break;
            }


            if (CantMinEmpleados != 0)
            {
                DateTime hoy = DateTime.Today;
                foreach (var emp in empresa)
                {
                    if (db.EmpleadoEmpresa.Where(x => x.idEmpresa == emp.IdEmpresa &&
                                                      (x.FechaBaja == null || x.FechaBaja > hoy)).Count() >= CantMinEmpleados)
                    {
                        int idLocalidad = emp.IdLocalidad;
                        Localidad localidad = db.Localidad.Where(x => x.IdLocalidad == idLocalidad).FirstOrDefault();

                        int idActividad = emp.IdActividad;
                        Actividad actividad = db.Actividad.Where(x => x.IdActividad == idActividad).FirstOrDefault();

                        tvm = new VmEmpresa();
                        tvm.Cuit = emp.Cuit;
                        tvm.RazonSocial = emp.RazonSocial;
                        tvm.NombreFantasia = emp.NombreFantasia;
                        tvm.Calle = emp.Calle + " " + emp.Altura.ToString();
                        tvm.Altura = emp.Altura.ToString();
                        tvm.Localidad = localidad.Nombre;
                        tvm.Provincia = localidad.Provincia.Nombre;
                        tvm.TelefonoFijo = (emp.TelefonoFijo != null) ? emp.TelefonoFijo.ToString() : "";
                        tvm.TelefonoCelular = (emp.TelefonoCelular != null) ? emp.TelefonoCelular.ToString() : "";
                        tvm.Email = emp.Email;
                        tvm.Actividad = actividad.Nombre;
                        tvm.FechaAltaEmpresa = emp.FechaAltaEmpresa.ToShortDateString();
                        if (emp.FechaBajaEmpresa != null)
                        {
                            tvm.FechaBajaEmpresa = emp.FechaBajaEmpresa.Value.ToShortDateString();
                        }

                        empresas.Add(tvm);
                    }
                }
            }
            else
            {
                foreach (var emp in empresa)
                {
                    int idLocalidad = emp.IdLocalidad;
                    Localidad localidad = db.Localidad.Where(x => x.IdLocalidad == idLocalidad).FirstOrDefault();

                    int idActividad = emp.IdActividad;
                    Actividad actividad = db.Actividad.Where(x => x.IdActividad == idActividad).FirstOrDefault();

                    tvm = new VmEmpresa();
                    tvm.Cuit = emp.Cuit;
                    tvm.RazonSocial = emp.RazonSocial;
                    tvm.NombreFantasia = emp.NombreFantasia;
                    tvm.Calle = emp.Calle + " " + emp.Altura.ToString();
                    tvm.Altura = emp.Altura.ToString();
                    tvm.Localidad = localidad.Nombre;
                    tvm.Provincia = localidad.Provincia.Nombre;
                    tvm.TelefonoFijo = (emp.TelefonoFijo != null) ? emp.TelefonoFijo.ToString() : "";
                    tvm.TelefonoCelular = (emp.TelefonoCelular != null) ? emp.TelefonoCelular.ToString() : "";
                    tvm.Email = emp.Email;
                    tvm.Actividad = actividad.Nombre;
                    tvm.FechaAltaEmpresa = emp.FechaAltaEmpresa.ToShortDateString();
                    if (emp.FechaBajaEmpresa != null)
                    {
                        tvm.FechaBajaEmpresa = emp.FechaBajaEmpresa.Value.ToShortDateString();
                    }

                    empresas.Add(tvm);
                }
            }
            

            //string[] filtros = new string[] { sortOrder, searchString, IdLocalidadSeleccionada.ToString(), IdProvinciaSeleccionada.ToString(), IdActividadSeleccionada.ToString() };

            return View(empresas.ToList());
        }
        
        [AllowAnonymous]
        public ActionResult ImprimirEmpresas(string sortOrder, string searchString, int IdLocalidadSeleccionada = 0, int IdProvinciaSeleccionada = 0, int IdActividadSeleccionada = 0, int CantMinEmpleados = 0)
        {
            return new ActionAsPdf("ImpresionEmpresas", new { sortOrder, searchString, IdLocalidadSeleccionada, IdProvinciaSeleccionada, IdActividadSeleccionada })
            {
                FileName = "Empresas.pdf"
            };
        }
        
        [AllowAnonymous]
        public ActionResult ImpresionPadron2(int? Mes, int? Anio)
        {
            int MesPadron = (Mes != null) ? (int)Mes : DateTime.Today.Month;
            int AnioPadron = (Anio != null) ? (int)Anio : DateTime.Today.Year;
            ViewBag.Mes = Mes;
            ViewBag.Anio = Anio;
            switch (Mes)
            {
                case 1:
                    ViewBag.MesDeclarado = "Enero";
                    break;
                case 2:
                    ViewBag.MesDeclarado = "Febrero";
                    break;
                case 3:
                    ViewBag.MesDeclarado = "Marzo";
                    break;
                case 4:
                    ViewBag.MesDeclarado = "Abril";
                    break;
                case 5:
                    ViewBag.MesDeclarado = "Mayo";
                    break;
                case 6:
                    ViewBag.MesDeclarado = "Junio";
                    break;
                case 7:
                    ViewBag.MesDeclarado = "Julio";
                    break;
                case 8:
                    ViewBag.MesDeclarado = "Agosto";
                    break;
                case 9:
                    ViewBag.MesDeclarado = "Septiembre";
                    break;
                case 10:
                    ViewBag.MesDeclarado = "Octubre";
                    break;
                case 11:
                    ViewBag.MesDeclarado = "Noviembre";
                    break;
                case 12:
                    ViewBag.MesDeclarado = "Diciembre";
                    break;
            };
            ViewBag.AnioDeclarado = AnioPadron;

            List<Empresa> empresas = db.Empresa.ToList();
            List<VmPadron> padron = new List<VmPadron>();

            foreach (var empresa in empresas)
            {
                DateTime hoy = DateTime.Today;
                int cantEmpleados = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == empresa.IdEmpresa && x.FechaBaja < hoy).Count();

                padron.Add(new VmPadron()
                {
                    IdEmpresa = empresa.IdEmpresa,
                    RazonSocial = empresa.RazonSocial,
                    CantEmpleados = cantEmpleados
                });
            }

            return View(padron);
        }

        [AllowAnonymous]
        public ActionResult ImprimirPadron2(int? Mes, int? Anio)
        {
            return new ActionAsPdf("ImpresionPadron2", new { Mes, Anio })
            {
                FileName = "Padron-2%.pdf"
            };
        }


        [AllowAnonymous]
        public ActionResult ImpresionPadron5()
        {
            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            DateTime hoy = DateTime.Now;

            var afiliados = db.Afiliado.Where(x => (x.FechaBaja == null || x.FechaBaja > hoy) && x.FechaAlta < hoy).ToList();


            if (afiliados.Count > 0)
            {
                foreach (var afiliado in afiliados)
                {
                    var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == afiliado.IdEmpleadoEmpresa &&
                                                               (x.FechaBaja == null || x.FechaBaja > hoy) &&
                                                               x.FechaAlta < hoy)
                                                   .FirstOrDefault();
                    if (empEmp != null)
                    {
                        var empleado = db.Empleado.Where(x => x.IdEmpleado == empEmp.idEmpleado).FirstOrDefault();

                        VmEmpleados vmEmp = new VmEmpleados();
                        vmEmp.NombreEmpleado = empleado.Apellido + " " + empleado.Nombre;
                        vmEmp.CuilEmpleado = empleado.Cuil;
                        vmEmp.LocalidadEmpleado = empleado.Localidad.Nombre;
                        vmEmp.ProvinciaEmpleado = empleado.Localidad.Provincia.Nombre;
                        vmEmp.CategoríaEmpleado = empEmp.Categoria.Descripcion;
                        vmEmp.JornadaEmpleado = empEmp.Jornada.Descripcion;

                        listaEmpleados.Add(vmEmp);
                    }
                }
            }
            ViewBag.TotalAfiliados = listaEmpleados.Count;
            return View(listaEmpleados);
        }
        #endregion

        #region Empleados
        [Authorize(Roles = "Admin, Fiscalizacion")]
        // GET: IndexEmpleados
        public ActionResult IndexEmpleados(string sortOrder, string currentFilter, string searchString, int? page, int idEmpresa = 0)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null).OrderBy(x => x.RazonSocial), "IdEmpresa", "RazonSocial");
            ViewBag.idEmpresaSeleccionada = idEmpresa;


            var localidad = db.Localidad.Include(t => t.Provincia).ToList();
            var empresa = db.Empresa.ToList();
            var categoria = db.Categoria.ToList();
            var jornada = db.Jornada.ToList();

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.Page = page;
            ViewBag.CurrentFilter = searchString;

            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();
            //var empleados = from oEmpleados in db.Empleado
            //                join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
            //                join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
            //                where oEmpleadoEmpresa.FechaBaja == null
            //                select new
            //                {
            //                    IdEmpleado = oEmpleados.IdEmpleado,
            //                    IdEmpresa = oEmpleadoEmpresa.idEmpresa,
            //                    Nombre = oEmpleados.Nombre,
            //                    Apellido = oEmpleados.Apellido,
            //                    Cuil = oEmpleados.Cuil,
            //                    Calle = oEmpleados.Calle,
            //                    Altura = oEmpleados.Altura,
            //                    IdLocalidad = oEmpleados.IdLocalidad,
            //                    FechaAlta = oEmpleadoEmpresa.FechaAlta,
            //                    IdCategoria = oEmpleadoEmpresa.IdCategoria,
            //                    IdJornada = oEmpleadoEmpresa.IdJornada
            //                };

            if (idEmpresa != 0 || !string.IsNullOrEmpty(searchString))
            {

                var empleados = from oEmpleados in db.Empleado
                                join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                                where oEmpleadoEmpresa.FechaBaja == null
                                select new
                                {
                                    IdEmpleado = oEmpleados.IdEmpleado,
                                    IdEmpresa = oEmpleadoEmpresa.idEmpresa,
                                    Nombre = oEmpleados.Nombre,
                                    Apellido = oEmpleados.Apellido,
                                    Cuil = oEmpleados.Cuil,
                                    Calle = oEmpleados.Calle,
                                    Altura = oEmpleados.Altura,
                                    IdLocalidad = oEmpleados.IdLocalidad,
                                    FechaAlta = oEmpleadoEmpresa.FechaAlta,
                                    IdCategoria = oEmpleadoEmpresa.IdCategoria,
                                    IdJornada = oEmpleadoEmpresa.IdJornada
                                };

                if(idEmpresa != 0)
                {
                    empleados = empleados.Where(x => x.IdEmpresa == idEmpresa);
                }

                foreach (var empleado in empleados)
                {
                    string nombreLocalidad = localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault().Nombre;
                    string nombreProvincia = localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault().Provincia.Nombre;
                    string nombreEmpresa = empresa.Where(x => x.IdEmpresa == empleado.IdEmpresa).FirstOrDefault().RazonSocial;
                    string cuitEmpresa = empresa.Where(x => x.IdEmpresa == empleado.IdEmpresa).FirstOrDefault().Cuit;
                    string nombreCategoria = categoria.Where(x => x.IdCategoria == empleado.IdCategoria).FirstOrDefault().Descripcion;
                    string nombreJornada = jornada.Where(x => x.IdJornada == empleado.IdJornada).FirstOrDefault().Descripcion;

                    listaEmpleados.Add(new VmEmpleados()
                    {
                        IdEmpleado = empleado.IdEmpleado,
                        NombreEmpleado = empleado.Nombre,
                        ApellidoEmpleado = empleado.Apellido,
                        CuilEmpleado = empleado.Cuil,
                        CalleEmpleado = empleado.Calle,
                        AlturaEmpleado = empleado.Altura.ToString(),
                        LocalidadEmpleado = nombreLocalidad,
                        ProvinciaEmpleado = nombreProvincia,
                        FechaAltaEmpleado = empleado.FechaAlta.ToShortDateString(),
                        CategoríaEmpleado = nombreCategoria,
                        JornadaEmpleado = nombreJornada,
                        NombreEmpresa = nombreEmpresa,
                        CuitEmpresa = cuitEmpresa
                    });
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                listaEmpleados = listaEmpleados.Where(x => x.ApellidoEmpleado.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    listaEmpleados = listaEmpleados.OrderByDescending(x => x.ApellidoEmpleado).ToList();
                    break;
                default:
                    listaEmpleados = listaEmpleados.OrderBy(x => x.ApellidoEmpleado).ToList();
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(listaEmpleados.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin, Fiscalizacion")]
        // GET: IndexEmpleados
        public ActionResult IndexEmpleadosBaja(string sortOrder, string currentFilter, string searchString, int? page, int idEmpresa = 0)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null).OrderBy(x => x.RazonSocial), "IdEmpresa", "RazonSocial");
            ViewBag.idEmpresaSeleccionada = idEmpresa;


            var localidad = db.Localidad.Include(t => t.Provincia).ToList();
            var empresa = db.Empresa.ToList();
            var categoria = db.Categoria.ToList();
            var jornada = db.Jornada.ToList();

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();
            var empleados = from oEmpleados in db.Empleado
                            join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                            join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                            where oEmpleadoEmpresa.FechaBaja != null
                            select new
                            {
                                IdEmpleado = oEmpleados.IdEmpleado,
                                IdEmpresa = oEmpleadoEmpresa.idEmpresa,
                                Nombre = oEmpleados.Nombre,
                                Apellido = oEmpleados.Apellido,
                                Cuil = oEmpleados.Cuil,
                                Calle = oEmpleados.Calle,
                                Altura = oEmpleados.Altura,
                                IdLocalidad = oEmpleados.IdLocalidad,
                                FechaAlta = oEmpleadoEmpresa.FechaAlta,
                                FechaBaja = oEmpleadoEmpresa.FechaBaja,
                                IdCategoria = oEmpleadoEmpresa.IdCategoria,
                                IdJornada = oEmpleadoEmpresa.IdJornada
                            };

            if (idEmpresa != 0)
            {
                empleados = from oEmpleados in empleados
                            join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                            where oEmpleadoEmpresa.idEmpresa == idEmpresa && oEmpleadoEmpresa.FechaBaja != null
                            select new
                            {
                                IdEmpleado = oEmpleados.IdEmpleado,
                                IdEmpresa = oEmpleadoEmpresa.idEmpresa,
                                Nombre = oEmpleados.Nombre,
                                Apellido = oEmpleados.Apellido,
                                Cuil = oEmpleados.Cuil,
                                Calle = oEmpleados.Calle,
                                Altura = oEmpleados.Altura,
                                IdLocalidad = oEmpleados.IdLocalidad,
                                FechaAlta = oEmpleadoEmpresa.FechaAlta,
                                FechaBaja = oEmpleadoEmpresa.FechaBaja,
                                IdCategoria = oEmpleadoEmpresa.IdCategoria,
                                IdJornada = oEmpleadoEmpresa.IdJornada
                            };
            }

            foreach (var empleado in empleados)
            {
                string nombreLocalidad = localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault().Nombre;
                string nombreProvincia = localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault().Provincia.Nombre;
                string nombreEmpresa = empresa.Where(x => x.IdEmpresa == empleado.IdEmpresa).FirstOrDefault().RazonSocial;
                string cuitEmpresa = empresa.Where(x => x.IdEmpresa == empleado.IdEmpresa).FirstOrDefault().Cuit;
                string nombreCategoria = categoria.Where(x => x.IdCategoria == empleado.IdCategoria).FirstOrDefault().Descripcion;
                string nombreJornada = jornada.Where(x => x.IdJornada == empleado.IdJornada).FirstOrDefault().Descripcion;

                listaEmpleados.Add(new VmEmpleados()
                {
                    IdEmpleado = empleado.IdEmpleado,
                    NombreEmpleado = empleado.Nombre,
                    ApellidoEmpleado = empleado.Apellido,
                    CuilEmpleado = empleado.Cuil,
                    CalleEmpleado = empleado.Calle,
                    AlturaEmpleado = empleado.Altura.ToString(),
                    LocalidadEmpleado = nombreLocalidad,
                    ProvinciaEmpleado = nombreProvincia,
                    FechaAltaEmpleado = empleado.FechaAlta.ToShortDateString(),
                    FechaBajaEmpleado = empleado.FechaBaja.Value.ToShortDateString(),
                    CategoríaEmpleado = nombreCategoria,
                    JornadaEmpleado = nombreJornada,
                    NombreEmpresa = nombreEmpresa,
                    CuitEmpresa = cuitEmpresa
                });
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                listaEmpleados = listaEmpleados.Where(x => x.ApellidoEmpleado.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    listaEmpleados = listaEmpleados.OrderByDescending(x => x.ApellidoEmpleado).ToList();
                    break;
                default:
                    listaEmpleados = listaEmpleados.OrderBy(x => x.ApellidoEmpleado).ToList();
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(listaEmpleados.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin, Fiscalizacion")]
        // GET: DetailsEmpleados/5
        public ActionResult DetailsEmpleados(int? id, string sortOrder, string currentFilter, int? page, int idEmpresa = 0, bool deBaja = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.DeBaja = deBaja;

            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresaSeleccionada = idEmpresa;
            ViewBag.Page = page;
            ViewBag.CurrentFilter = currentFilter;

            var empleadoAux = (from oEmpleados in db.Empleado
                                 join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                                 join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                                 where oEmpleadoEmpresa.idEmpleado == id
                                 select new
                                 {
                                     IdEmpleado = oEmpleados.IdEmpleado,
                                     IdEmpresa = oEmpleadoEmpresa.idEmpresa,
                                     Nombre = oEmpleados.Nombre,
                                     Apellido = oEmpleados.Apellido,
                                     Cuil = oEmpleados.Cuil,
                                     Calle = oEmpleados.Calle,
                                     Altura = oEmpleados.Altura,
                                     IdLocalidad = oEmpleados.IdLocalidad,
                                     FechaAlta = oEmpleadoEmpresa.FechaAlta,
                                     IdCategoria = oEmpleadoEmpresa.IdCategoria,
                                     IdJornada = oEmpleadoEmpresa.IdJornada
                                 }).FirstOrDefault();

            var localidad = db.Localidad.Include(t => t.Provincia).Where(x => x.IdLocalidad == empleadoAux.IdLocalidad).FirstOrDefault();
            var empresa = db.Empresa.Where(x => x.IdEmpresa == empleadoAux.IdEmpresa).FirstOrDefault();
            var categoria = db.Categoria.Find(empleadoAux.IdCategoria);
            var jornada = db.Jornada.Find(empleadoAux.IdJornada);

            VmEmpleados empleado = new VmEmpleados() {
                IdEmpleado = empleadoAux.IdEmpleado,
                NombreEmpleado = empleadoAux.Nombre,
                ApellidoEmpleado = empleadoAux.Apellido,
                CuilEmpleado = empleadoAux.Cuil,
                CalleEmpleado = empleadoAux.Calle,
                AlturaEmpleado = empleadoAux.Altura.ToString(),
                LocalidadEmpleado = localidad.Nombre,
                ProvinciaEmpleado = localidad.Provincia.Nombre,
                FechaAltaEmpleado = empleadoAux.FechaAlta.ToShortDateString(),
                CategoríaEmpleado = categoria.Descripcion,
                JornadaEmpleado = jornada.Descripcion,
                NombreEmpresa = empresa.RazonSocial,
                CuitEmpresa = empresa.Cuit
            };

            //empleado.FechaAlta = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado && x.FechaBaja == null).FirstOrDefault().FechaAlta;

            if (empleado == null)
            {
                return HttpNotFound();
            }

            return View(empleado);
        }

        [AllowAnonymous]
        public ActionResult ImpresionResumenEmpleados(string sortOrder, string currentFilter, string searchString, int? Mes, int? Anio, int idEmpresa = 0)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null), "IdEmpresa", "RazonSocial");
            ViewBag.idEmpresaSeleccionada = idEmpresa;

            ViewBag.CurrentFilter = searchString;

            int MesSeleccionado = (Mes != null) ? (int)Mes : DateTime.Today.Month;
            int AnioSeleccionado = (Anio != null) ? (int)Anio : DateTime.Today.Year;
            ViewBag.Mes = MesSeleccionado;
            ViewBag.Anio = AnioSeleccionado;

            #region Empleados
            var empleados = (from oEmpleados in db.Empleado
                             join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                             where (oEmpleadoEmpresa.FechaAlta.Year < AnioSeleccionado || (oEmpleadoEmpresa.FechaAlta.Year == AnioSeleccionado && oEmpleadoEmpresa.FechaAlta.Month <= MesSeleccionado)) && 
                                   (oEmpleadoEmpresa.FechaBaja == null || (oEmpleadoEmpresa.FechaBaja.Value.Year >= AnioSeleccionado || (oEmpleadoEmpresa.FechaBaja.Value.Year == AnioSeleccionado && oEmpleadoEmpresa.FechaBaja.Value.Month >= MesSeleccionado)))
                             select oEmpleados).ToList();

            ViewBag.TotalEmpleados = empleados.Count;
            #endregion

            #region Empleados Alta
            var empleadosAlta = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.FechaAlta.Month == MesSeleccionado && x.FechaAlta.Year == AnioSeleccionado).ToList();

            ViewBag.TotalEmpleadosAlta = empleadosAlta.Count;
            #endregion

            #region Empleados Baja
            var empleadosBaja = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.FechaBaja.HasValue && x.FechaBaja.Value.Month == Mes && x.FechaBaja.Value.Year == AnioSeleccionado).ToList();

            ViewBag.TotalEmpleadosBaja = empleadosBaja.Count;
            #endregion

            #region Afiliados

            DateTime hoy = new DateTime(AnioSeleccionado,MesSeleccionado,(MesSeleccionado == 2)?28:30);
            int totalAfiliados = 0;

            var afiliados = db.Afiliado.Where(x => (x.FechaBaja == null || x.FechaBaja > hoy) && x.FechaAlta < hoy).ToList();

            if (afiliados.Count > 0)
            {
                foreach (var afiliado in afiliados)
                {
                    var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == afiliado.IdEmpleadoEmpresa &&
                                                               (x.FechaBaja == null || x.FechaBaja > hoy) &&
                                                               x.FechaAlta < hoy)
                                                   .FirstOrDefault();
                    if (empEmp != null)
                    {
                        totalAfiliados++;
                    }
                }
            }
            ViewBag.TotalAfiliados = totalAfiliados;
            #endregion

            #region Afiliados Alta
            var afiliadosAlta = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.EsAfiliado && x.FechaAlta.Month == MesSeleccionado && x.FechaAlta.Year == AnioSeleccionado).ToList();

            ViewBag.TotalAfiliadosAlta = afiliadosAlta.Count;
            #endregion

            #region Afiliados Baja
            var afiliadosBaja = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.EsAfiliado && x.FechaBaja.HasValue && x.FechaBaja.Value.Month == MesSeleccionado && x.FechaBaja.Value.Year == AnioSeleccionado).ToList();

            ViewBag.TotalAfiliadosBaja = afiliadosAlta.Count;
            #endregion

            return View();
        }

        [AllowAnonymous]
        public ActionResult ImprimirResumenEmpleados(string sortOrder, string currentFilter, string searchString, int? Mes, int? Anio, int idEmpresa = 0)
        {
            return new ActionAsPdf("ImpresionResumenEmpleados", new { sortOrder, currentFilter, searchString, Mes, Anio, idEmpresa })
            {
                FileName = "ResumenGeneralEmpleados.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                MinimumFontSize = 12
            };
        }

        [AllowAnonymous]
        public ActionResult ImpresionEmpleados(string sortOrder, string currentFilter, string searchString, int idEmpresa = 0)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null), "IdEmpresa", "RazonSocial");
            ViewBag.idEmpresaSeleccionada = idEmpresa;

            ViewBag.CurrentFilter = searchString;

            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            var empleados = (from oEmpleados in db.Empleado
                             join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                             join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                             where oEmpleadoEmpresa.FechaBaja == null
                             select oEmpleados).ToList();

            if (idEmpresa != 0)
            {
                empleados = (from oEmpleados in empleados
                             join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                             join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                             where oEmpleadoEmpresa.idEmpresa == idEmpresa && oEmpleadoEmpresa.FechaBaja == null
                             select oEmpleados).ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(x => x.Apellido.Contains(searchString)).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    empleados = empleados.OrderByDescending(x => x.Apellido).ToList();
                    break;
                default:
                    empleados = empleados.OrderBy(x => x.Apellido).ToList();
                    break;
            }


            if (empleados.Count > 0)
            {
                foreach (var empleado in empleados)
                {
                    var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado &&
                                                               x.FechaBaja == null).FirstOrDefault();
                    if (empEmp != null)
                    {
                        VmEmpleados vmEmp = new VmEmpleados();
                        vmEmp.NombreEmpleado = empleado.Apellido + " " + empleado.Nombre;
                        vmEmp.CuilEmpleado = empleado.Cuil;
                        vmEmp.LocalidadEmpleado = empleado.Localidad.Nombre;
                        vmEmp.ProvinciaEmpleado = empleado.Localidad.Provincia.Nombre;
                        vmEmp.CategoríaEmpleado = empEmp.Categoria.Descripcion;
                        vmEmp.JornadaEmpleado = empEmp.Jornada.Descripcion;

                        listaEmpleados.Add(vmEmp);
                    }
                }
            }

            ViewBag.TotalEmpleados = listaEmpleados.Count;
            return View(listaEmpleados);
        }

        [AllowAnonymous]
        public ActionResult ImprimirEmpleados(string sortOrder, string currentFilter, string searchString, int idEmpresa = 0)
        {
            return new ActionAsPdf("ImpresionEmpleados", new { sortOrder, currentFilter, searchString, idEmpresa })
            {
                FileName = "Empleados.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                MinimumFontSize = 12
            };
        }

        [AllowAnonymous]
        public ActionResult ImpresionEmpleadosNoAfiliados(string sortOrder, string currentFilter, string searchString, int idEmpresa = 0)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null), "IdEmpresa", "RazonSocial");
            ViewBag.idEmpresaSeleccionada = idEmpresa;

            ViewBag.CurrentFilter = searchString;

            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();
            
            var empleados = (from oEmpleados in db.Empleado
                             join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                             join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                             where oEmpleadoEmpresa.FechaBaja == null
                             select oEmpleados).ToList();

            if (idEmpresa != 0)
            {
                empleados = (from oEmpleados in empleados
                             join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                             join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                             where oEmpleadoEmpresa.idEmpresa == idEmpresa && oEmpleadoEmpresa.FechaBaja == null
                             select oEmpleados).ToList();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(x => x.Apellido.Contains(searchString)).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    empleados = empleados.OrderByDescending(x => x.Apellido).ToList();
                    break;
                default:
                    empleados = empleados.OrderBy(x => x.Apellido).ToList();
                    break;
            }


            if (empleados.Count > 0)
            {
                foreach (var empleado in empleados)
                {
                    var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado &&
                                                               x.FechaBaja == null).FirstOrDefault();
                    if (empEmp != null)
                    {
                        if (!empEmp.EsAfiliado)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empleado.Apellido + " " + empleado.Nombre;
                            vmEmp.CuilEmpleado = empleado.Cuil;
                            vmEmp.LocalidadEmpleado = empleado.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empleado.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empEmp.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empEmp.Jornada.Descripcion;

                            listaEmpleados.Add(vmEmp);
                        }
                    }
                }
            }

            ViewBag.TotalEmpleados = listaEmpleados.Count;
            return View(listaEmpleados);
        }

        [AllowAnonymous]
        public ActionResult ImprimirEmpleadosNoAfiliados(string sortOrder, string currentFilter, string searchString, int idEmpresa = 0)
        {
            return new ActionAsPdf("ImpresionEmpleadosNoAfiliados", new { sortOrder, currentFilter, searchString, idEmpresa })
            {
                FileName = "Empleados-No-Afiliados.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                MinimumFontSize = 12
            };
        }

        [AllowAnonymous]
        public ActionResult ImpresionEmpleadosPorEmpresas(string sortOrder, string searchString, int searchType = 1, int IdLocalidadSeleccionada = 0, int IdProvinciaSeleccionada = 0, int IdActividadSeleccionada = 0, int CantMinEmpleados = 0)
        {
            List<VmEmpleadosEmpresas> empleadosEmpresas = new List<VmEmpleadosEmpresas>();

            VmEmpleadosEmpresas tvm;

            //Datos de la Empresa
            var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);

            if (IdProvinciaSeleccionada != 0)
            {
                empresa.Where(x => x.Localidad.IdProvincia == IdProvinciaSeleccionada);
            }

            if (IdLocalidadSeleccionada != 0)
            {
                empresa = empresa.Where(x => x.IdLocalidad == IdLocalidadSeleccionada);
            }

            if (IdActividadSeleccionada != 0)
            {
                empresa = empresa.Where(x => x.IdActividad == IdActividadSeleccionada);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                if (searchType == 1)
                {
                    empresa = empresa.Where(x => x.RazonSocial.ToUpper().Contains(searchString.ToUpper()));
                }
                else
                {
                    empresa = empresa.Where(x => x.Cuit.ToUpper().Contains(searchString.ToUpper()));
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    empresa = empresa.OrderByDescending(x => x.RazonSocial);
                    break;
                case "nombreFantasia":
                    empresa = empresa.OrderBy(s => s.NombreFantasia);
                    break;
                case "nombreFantasia_desc":
                    empresa = empresa.OrderByDescending(s => s.NombreFantasia);
                    break;
                case "localidad":
                    empresa = empresa.Include(t => t.Localidad).OrderBy(s => s.Localidad.Nombre);
                    break;
                case "localidad_desc":
                    empresa = empresa.Include(t => t.Localidad).OrderByDescending(s => s.Localidad.Nombre);
                    break;
                default:
                    empresa = empresa.OrderBy(x => x.RazonSocial);
                    break;
            }

            DateTime hoy = DateTime.Today;
            foreach (var emp in empresa.ToList())
            {
                tvm = new VmEmpleadosEmpresas();
                tvm.RazonSocialEmpresa = emp.RazonSocial;
                tvm.CuitEmpresa = emp.Cuit;
                tvm.CalleEmpresa = emp.Calle + " " + emp.Altura;
                tvm.LocalidadEmpresa = emp.Localidad.Nombre;
                tvm.ProvinciaEmpresa = emp.Localidad.Provincia.Nombre;

                var empleados = (from oEmpleados in db.Empleado
                                 join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                                 join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                                 where oEmpleadoEmpresa.idEmpresa == emp.IdEmpresa && (oEmpleadoEmpresa.FechaBaja == null || oEmpleadoEmpresa.FechaBaja > hoy)
                                 select oEmpleados).ToList();

                if (empleados.Count > 0 && (CantMinEmpleados == 0 || empleados.Count >= CantMinEmpleados))
                {
                    tvm.Empleados = new List<VmEmpleados>();
                    foreach (var empleado in empleados)
                    {
                        var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado &&
                                                                   x.idEmpresa == emp.IdEmpresa &&
                                                                   x.FechaBaja == null).FirstOrDefault();
                        if (empEmp != null)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empleado.Apellido + " " + empleado.Nombre;
                            vmEmp.CuilEmpleado = empleado.Cuil;
                            vmEmp.LocalidadEmpleado = empleado.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empleado.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empEmp.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empEmp.Jornada.Descripcion;
                            vmEmp.CalleEmpleado = empleado.Calle;
                            vmEmp.AlturaEmpleado = empleado.Altura.ToString();

                            tvm.Empleados.Add(vmEmp);
                        }
                    }

                    empleadosEmpresas.Add(tvm);
                }

            }

            //return new ViewAsPdf(empleadosEmpresas.ToList())
            //{
            //    FileName = "Empleados-Por-Empresas.pdf"
            //};
            return View(empleadosEmpresas.ToList());
        }

        [AllowAnonymous]
        public ActionResult ImprimirEmpleadosPorEmpresas(string sortOrder, string searchString, int searchType = 1, int IdLocalidadSeleccionada = 0, int IdProvinciaSeleccionada = 0, int IdActividadSeleccionada = 0, int CantMinEmpleados = 0)
        {
            return new ActionAsPdf("ImpresionEmpleadosPorEmpresas", new { sortOrder, searchString, searchType, IdLocalidadSeleccionada, IdProvinciaSeleccionada, IdActividadSeleccionada, CantMinEmpleados })
            {
                FileName = "Empleados-Por-Empresas.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                MinimumFontSize = 12
            };
        }

        [AllowAnonymous]
        public ActionResult ImprimirAltasEmpleados(DateTime? Desde, DateTime? Hasta)
        {
            DateTime Start = (Desde != null) ? (DateTime)Desde : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime End = (Hasta != null) ? (DateTime)Hasta : DateTime.MaxValue;
            ViewBag.Start = Start;
            ViewBag.End = End;

            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            var empleados = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.FechaAlta > Start && x.FechaAlta < End).ToList();

            if (empleados.Count > 0)
            {
                foreach (var empleado in empleados)
                {
                    var empEmp = db.Empleado.Where(x => x.IdEmpleado == empleado.idEmpleado).FirstOrDefault();
                    if (empEmp != null)
                    {
                        if (!empEmp.EsAfiliado)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empEmp.Apellido + " " + empEmp.Nombre;
                            vmEmp.CuilEmpleado = empEmp.Cuil;
                            vmEmp.LocalidadEmpleado = empEmp.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empEmp.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empleado.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empleado.Jornada.Descripcion;

                            listaEmpleados.Add(vmEmp);
                        }
                    }
                }
            }

            ViewBag.TotalEmpleados = listaEmpleados.Count;
            return new ViewAsPdf(listaEmpleados)
            {
                FileName = "Altas-Empleados.pdf"
            };
        }

        [AllowAnonymous]
        public ActionResult ImprimirBajasEmpleados(DateTime? Desde, DateTime? Hasta)
        {
            DateTime Start = (Desde != null) ? (DateTime)Desde : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime End = (Hasta != null) ? (DateTime)Hasta : DateTime.MaxValue;
            ViewBag.Start = Start;
            ViewBag.End = End;

            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            var empleados = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.FechaBaja > Start && x.FechaBaja < End).ToList();

            if (empleados.Count > 0)
            {
                foreach (var empleado in empleados)
                {
                    var empEmp = db.Empleado.Where(x => x.IdEmpleado == empleado.idEmpleado).FirstOrDefault();
                    if (empEmp != null)
                    {
                        if (!empEmp.EsAfiliado)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empEmp.Apellido + " " + empEmp.Nombre;
                            vmEmp.CuilEmpleado = empEmp.Cuil;
                            vmEmp.LocalidadEmpleado = empEmp.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empEmp.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empleado.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empleado.Jornada.Descripcion;

                            listaEmpleados.Add(vmEmp);
                        }
                    }
                }
            }

            ViewBag.TotalEmpleados = listaEmpleados.Count;
            return new ViewAsPdf(listaEmpleados)
            {
                FileName = "Bajas-Empleados.pdf"
            };
        }

        [AllowAnonymous]
        public ActionResult ImprimirAportesMenoresAlMinimo(DateTime? Desde, DateTime? Hasta)
        {
            DateTime Start = (Desde != null) ? (DateTime)Desde : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime End = (Hasta != null) ? (DateTime)Hasta : DateTime.MaxValue;
            ViewBag.Start = Start;
            ViewBag.End = End;

            var empresas = db.Empresa.ToList();
            List<VmEmpleadosEmpresas> empleadosEmpresas = new List<VmEmpleadosEmpresas>();
            VmEmpleadosEmpresas EmpEmp;

            foreach (var emp in empresas)
            {
                List<DetalleDeclaracionJurada> ListaDetalleDeclaracionJurada = new List<DetalleDeclaracionJurada>();
                List<DetalleDeclaracionJurada> ListaAporteMenorEscalaSalarial = new List<DetalleDeclaracionJurada>();
                List<VmEmpleados> empleados = new List<VmEmpleados>();

                ListaDetalleDeclaracionJurada = db.DetalleDeclaracionJurada.Include(t => t.DeclaracionJurada)
                                                                           .Include(t => t.EmpleadoEmpresa)
                                                                           .Where(x => x.EmpleadoEmpresa.idEmpresa == emp.IdEmpresa)
                                                                           .ToList();


                foreach (var detalleDeclaracionJurada in ListaDetalleDeclaracionJurada)
                {
                    var DeclaracionJurada = detalleDeclaracionJurada.DeclaracionJurada;

                    DateTime fechaDetalleDeclaracionJurada = new DateTime(DeclaracionJurada.anio, DeclaracionJurada.mes, 15);

                    if(fechaDetalleDeclaracionJurada >= Start && fechaDetalleDeclaracionJurada <= End)
                    {
                        var sueldoBasico = db.SueldoBasico.Where(x => x.Desde <= fechaDetalleDeclaracionJurada && x.Hasta >= fechaDetalleDeclaracionJurada).FirstOrDefault();

                        if (sueldoBasico != null)
                        {
                            if (detalleDeclaracionJurada.Sueldo < sueldoBasico.Monto)
                            {
                                ListaAporteMenorEscalaSalarial.Add(detalleDeclaracionJurada);
                            }
                        }
                    }
                }

                if (ListaAporteMenorEscalaSalarial.Count > 0)
                {
                    foreach (var empleado in ListaAporteMenorEscalaSalarial)
                    {
                        var empEmp = db.Empleado.Where(x => x.IdEmpleado == empleado.EmpleadoEmpresa.idEmpleado).FirstOrDefault();
                        if (empEmp != null)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empEmp.Apellido + " " + empEmp.Nombre;
                            vmEmp.CuilEmpleado = empEmp.Cuil;
                            vmEmp.LocalidadEmpleado = empEmp.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empEmp.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empleado.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empleado.Jornada.Descripcion;
                            vmEmp.Sueldo = Decimal.Parse(empleado.Sueldo.ToString());

                            DateTime fechaDetalleDeclaracionJurada = new DateTime(empleado.DeclaracionJurada.anio, empleado.DeclaracionJurada.mes, 15);
                            var licencia_liquidacion = db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleado.EmpleadoEmpresa.idEmpleadoEmpresa &&
                                                                                      x.FechaAltaLicencia <= fechaDetalleDeclaracionJurada &&
                                                                                      x.FechaBajaLicencia >= fechaDetalleDeclaracionJurada)
                                                                          .Include(t => t.LicenciaLaboral)
                                                                          .FirstOrDefault();
                            vmEmp.Licencia_Liquidacion = (licencia_liquidacion != null) ? licencia_liquidacion.LicenciaLaboral.Descripcion : "";

                            var LiquidacionProporcional = db.LiquidacionProporcionalEmpleado.Where(x => x.IdDetalleDeclaracionJurada == empleado.IdDetalleDeclaracionJurada).Include(t => t.LiquidacionProporcional).FirstOrDefault();

                            if (LiquidacionProporcional != null)
                            {
                                vmEmp.Licencia_Liquidacion = LiquidacionProporcional.LiquidacionProporcional.Descripcion;
                            }

                            empleados.Add(vmEmp);
                        }
                    }
                }
                EmpEmp = new VmEmpleadosEmpresas();
                EmpEmp.RazonSocialEmpresa = emp.RazonSocial;
                EmpEmp.CuitEmpresa = emp.Cuit;
                EmpEmp.CalleEmpresa = emp.Calle + " " + emp.Altura;
                EmpEmp.LocalidadEmpresa = emp.Localidad.Nombre;
                EmpEmp.ProvinciaEmpresa = emp.Localidad.Provincia.Nombre;

                if (empleados.Count > 0)
                {
                    EmpEmp.Empleados = new List<VmEmpleados>();
                    foreach (var empleado in empleados)
                    {
                        EmpEmp.Empleados.Add(empleado);
                    }

                    empleadosEmpresas.Add(EmpEmp);
                }

            }

            //return new ViewAsPdf(empleadosEmpresas.ToList())
            //{
            //    FileName = "Empleados-Por-Empresas.pdf"
            //};
            //return View(empleadosEmpresas.ToList());

            return new ViewAsPdf(empleadosEmpresas)
            {
                FileName = "empleados-con-aporte-menor-a-la-escala-salarial.pdf"
            };
        }
        #endregion

        #region Afiliados
        [Authorize(Roles = "Admin, Fiscalizacion")]
        // GET: Administrador/Listados/IndexAfiliados
        public ActionResult IndexAfiliados(string sortOrder, string currentFilter, string searchString, int? page, int idEmpresa = 0)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null).OrderBy(x => x.RazonSocial), "IdEmpresa", "RazonSocial");

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            List<Afiliado> afiliados = new List<Afiliado>();
            var empleadosEmpresa = db.EmpleadoEmpresa.Where(x => x.FechaBaja == null).ToList();
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

            if (idEmpresa != 0)
            {
                afiliados.Clear();
                empleadosEmpresa.Clear();
                empleadosEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpresa == idEmpresa && x.FechaBaja == null).ToList();
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
            
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                afiliados = afiliados.Where(x => x.EmpleadoEmpresa.Empleado.Apellido.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    afiliados = afiliados.OrderByDescending(x => x.EmpleadoEmpresa.Empleado.Apellido).ToList();
                    break;
                default:
                    afiliados = afiliados.OrderBy(x => x.EmpleadoEmpresa.Empleado.Apellido).ToList();
                    break;
            }
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(afiliados.ToPagedList(pageNumber, pageSize));
        }

        [Authorize(Roles = "Admin, Fiscalizacion")]
        // GET: Administrador/Listados/DetailsAfiliados/5
        public ActionResult DetailsAfiliados(int? id)
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

        [AllowAnonymous]
        public ActionResult ImpresionAfiliados(string sortOrder, string currentFilter, string searchString, int idEmpresa = 0)
        {
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.FechaBajaEmpresa == null), "IdEmpresa", "RazonSocial");
            ViewBag.idEmpresaSeleccionada = idEmpresa;

            ViewBag.CurrentFilter = searchString;

            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            DateTime hoy = DateTime.Now;

            var afiliados = db.Afiliado.Where(x => (x.FechaBaja == null || x.FechaBaja > hoy) && x.FechaAlta < hoy).ToList();


            if (afiliados.Count > 0)
            {
                foreach (var afiliado in afiliados)
                {
                    var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == afiliado.IdEmpleadoEmpresa &&
                                                               (x.FechaBaja == null || x.FechaBaja > hoy) && 
                                                               x.FechaAlta < hoy)
                                                   .FirstOrDefault();
                    if (empEmp != null)
                    {
                        //var empleado = db.Empleado.Where(x => x.IdEmpleado == empEmp.idEmpleado).FirstOrDefault();

                        VmEmpleados vmEmp = new VmEmpleados();
                        vmEmp.NombreEmpleado = empEmp.Empleado.Apellido + " " + empEmp.Empleado.Nombre;
                        vmEmp.CalleEmpleado = empEmp.Empleado.Calle + " " + empEmp.Empleado.Altura;
                        vmEmp.CuilEmpleado = empEmp.Empleado.Cuil;
                        vmEmp.LocalidadEmpleado = empEmp.Empleado.Localidad.Nombre;
                        vmEmp.ProvinciaEmpleado = empEmp.Empleado.Localidad.Provincia.Nombre;
                        vmEmp.CategoríaEmpleado = empEmp.Categoria.Descripcion;
                        vmEmp.JornadaEmpleado = empEmp.Jornada.Descripcion;
                        vmEmp.FechaAltaEmpleado = empEmp.FechaAlta.ToShortDateString();
                        vmEmp.NombreEmpresa = empEmp.Empresa.RazonSocial;


                        listaEmpleados.Add(vmEmp);
                    }
                }
            }
            ViewBag.TotalAfiliados = listaEmpleados.Count;
            return View(listaEmpleados.OrderBy(x => x.LocalidadEmpleado).ThenBy(x => x.NombreEmpleado));
        }

        [AllowAnonymous]
        public ActionResult ImprimirAfiliados(string sortOrder, string currentFilter, string searchString, int idEmpresa = 0)
        {
            return new ActionAsPdf("ImpresionAfiliados", new { sortOrder, currentFilter, searchString, idEmpresa })
            {
                FileName = "Afiliados.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                MinimumFontSize = 12
            };
        }

        [AllowAnonymous]
        public FileContentResult ImpresionAfiliadosPorEmpresas(string sortOrder, string searchString, int IdLocalidadSeleccionada = 0, int IdProvinciaSeleccionada = 0, int IdActividadSeleccionada = 0)
        {
            List<VmEmpleadosEmpresas> empleadosEmpresas = new List<VmEmpleadosEmpresas>();

            VmEmpleadosEmpresas tvm;

            string newFile = "";

            //Datos de la Empresa
            var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);

            if (IdProvinciaSeleccionada != 0)
            {
                empresa.Where(x => x.Localidad.IdProvincia == IdProvinciaSeleccionada);
            }

            if (IdLocalidadSeleccionada != 0)
            {
                empresa = empresa.Where(x => x.IdLocalidad == IdLocalidadSeleccionada);
            }

            if (IdActividadSeleccionada != 0)
            {
                empresa = empresa.Where(x => x.IdActividad == IdActividadSeleccionada);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                empresa = empresa.Where(x => x.RazonSocial.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    empresa = empresa.OrderByDescending(x => x.RazonSocial);
                    break;
                case "nombreFantasia":
                    empresa = empresa.OrderBy(s => s.NombreFantasia);
                    break;
                case "nombreFantasia_desc":
                    empresa = empresa.OrderByDescending(s => s.NombreFantasia);
                    break;
                case "localidad":
                    empresa = empresa.Include(t => t.Localidad).OrderBy(s => s.Localidad.Nombre);
                    break;
                case "localidad_desc":
                    empresa = empresa.Include(t => t.Localidad).OrderByDescending(s => s.Localidad.Nombre);
                    break;
                default:
                    empresa = empresa.OrderBy(x => x.RazonSocial);
                    break;
            }

            foreach (var emp in empresa.ToList())
            {
                tvm = new VmEmpleadosEmpresas();
                tvm.RazonSocialEmpresa = emp.RazonSocial;
                tvm.CuitEmpresa = emp.Cuit;
                tvm.CalleEmpresa = emp.Calle + " " + emp.Altura;
                tvm.LocalidadEmpresa = emp.Localidad.Nombre;
                tvm.ProvinciaEmpresa = emp.Localidad.Provincia.Nombre;

                var empleados = (from oEmpleados in db.Empleado
                                 join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                                 join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                                 join oAfiliados in db.Afiliado on oEmpleadoEmpresa.idEmpleadoEmpresa equals oAfiliados.IdEmpleadoEmpresa
                                 where oEmpleadoEmpresa.idEmpresa == emp.IdEmpresa && oAfiliados.FechaBaja == null
                                 select oEmpleados).OrderBy(x => x.Localidad.Nombre).ToList();

                if (empleados.Count > 0)
                {
                    tvm.Empleados = new List<VmEmpleados>();
                    foreach (var empleado in empleados)
                    {
                        var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado &&
                                                                   x.idEmpresa == emp.IdEmpresa &&
                                                                   x.FechaBaja == null).FirstOrDefault();
                        if (empEmp != null)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empEmp.Empleado.Apellido + " " + empEmp.Empleado.Nombre;
                            vmEmp.CalleEmpleado = empEmp.Empleado.Calle + " " + empEmp.Empleado.Altura;
                            vmEmp.CuilEmpleado = empEmp.Empleado.Cuil;
                            vmEmp.LocalidadEmpleado = empEmp.Empleado.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empEmp.Empleado.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empEmp.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empEmp.Jornada.Descripcion;
                            vmEmp.FechaAltaEmpleado = empEmp.FechaAlta.ToShortDateString();
                            vmEmp.NombreEmpresa = empEmp.Empresa.RazonSocial;

                            tvm.Empleados.Add(vmEmp);
                        }
                    }

                    empleadosEmpresas.Add(tvm);
                }

            }

            #region Generacion Excels
            int fila = 1;
            
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Afiliados Por Empresa");
                foreach (var excelSwissMedical in empleadosEmpresas.Where(x => x.Empleados.Count >= 1))
                {
                    //Escriobo en la fila y en la columna que corresponde para cada valor
                    worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString("Razon Social:"));
                    worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(excelSwissMedical.RazonSocialEmpresa));
                    worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString("Cuit:"));
                    worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString(excelSwissMedical.CuitEmpresa));
                    fila++;

                    worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString("Calle: "));
                    worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(excelSwissMedical.CalleEmpresa));
                    worksheet.Cell(fila, 3).SetValue<string>(Convert.ToString("Localidad:"));
                    worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString(excelSwissMedical.LocalidadEmpresa));
                    worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString("Provincia:"));
                    worksheet.Cell(fila, 6).SetValue<string>(Convert.ToString(excelSwissMedical.ProvinciaEmpresa));

                    IXLRange range = worksheet.Range(worksheet.Cell(fila - 1, 1).Address, worksheet.Cell(fila, 6).Address);

                    range.Style.Font.Bold = true;
                    fila++;

                    worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString("Nombre"));
                    worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString("Calle"));
                    worksheet.Cell(fila, 3).SetValue<string>(Convert.ToString("Localidad"));
                    worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString("Cuil"));
                    worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString("Fecha Alta"));
                    worksheet.Cell(fila, 6).SetValue<string>(Convert.ToString("Categoria"));
                    worksheet.Cell(fila, 7).SetValue<string>(Convert.ToString("Nombre de la Empresa"));
                    IXLRange range2 = worksheet.Range(worksheet.Cell(fila, 1).Address, worksheet.Cell(fila, 7).Address);
                    range2.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                    range2.Style.Font.Bold = true;
                    fila++;
                    foreach (var afiliado in excelSwissMedical.Empleados)
                    {
                        worksheet.Cell(fila, 1).SetValue<string>(Convert.ToString(afiliado.NombreEmpleado));
                        worksheet.Cell(fila, 2).SetValue<string>(Convert.ToString(afiliado.CalleEmpleado));
                        worksheet.Cell(fila, 3).SetValue<string>(Convert.ToString(afiliado.LocalidadEmpleado));
                        worksheet.Cell(fila, 4).SetValue<string>(Convert.ToString(afiliado.CuilEmpleado));
                        worksheet.Cell(fila, 5).SetValue<string>(Convert.ToString(afiliado.FechaAltaEmpleado));
                        worksheet.Cell(fila, 6).SetValue<string>(Convert.ToString(afiliado.CategoríaEmpleado));
                        worksheet.Cell(fila, 7).SetValue<string>(Convert.ToString(afiliado.NombreEmpresa));
                        fila++;
                    }
                    fila++;
                }

                worksheet.Columns().AdjustToContents();  // Adjust column width
                worksheet.Rows().AdjustToContents();

                newFile = Server.MapPath("~/Areas/Administrador/Content/Impresiones/AfiliadosPorEmpresa/AfiliadosPorEmpresa.xlsx");
                workbook.SaveAs(newFile);
            }
            
            #endregion

            #region descargar
            String mimeType = MimeMapping.GetMimeMapping(newFile);

            byte[] stream = System.IO.File.ReadAllBytes(newFile);

            return File(stream, mimeType);
            #endregion
        }

        [AllowAnonymous]
        public ActionResult ImprimirAfiliadosPorEmpresas(string sortOrder, string searchString, int IdLocalidadSeleccionada = 0, int IdProvinciaSeleccionada = 0, int IdActividadSeleccionada = 0)
        {
            return new ActionAsPdf("ImpresionAfiliadosPorEmpresas", new { sortOrder, searchString, IdLocalidadSeleccionada, IdProvinciaSeleccionada, IdActividadSeleccionada })
            {
                FileName = "Afiliados-Por-Empresas.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                MinimumFontSize = 12
            };
        }

        [AllowAnonymous]
        public ActionResult ImprimirAltasAfiliados(DateTime? Desde, DateTime? Hasta)
        {
            DateTime Start = (Desde != null) ? (DateTime)Desde : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime End = (Hasta != null) ? (DateTime)Hasta : DateTime.MaxValue;
            ViewBag.Start = Start;
            ViewBag.End = End;
            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            var empleados = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.EsAfiliado && x.FechaAlta > Start && x.FechaAlta < End).ToList();

            if (empleados.Count > 0)
            {
                foreach (var empleado in empleados)
                {
                    var empEmp = db.Empleado.Where(x => x.IdEmpleado == empleado.idEmpleado).FirstOrDefault();
                    if (empEmp != null)
                    {
                        if (!empEmp.EsAfiliado)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empEmp.Apellido + " " + empEmp.Nombre;
                            vmEmp.CuilEmpleado = empEmp.Cuil;
                            vmEmp.LocalidadEmpleado = empEmp.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empEmp.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empleado.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empleado.Jornada.Descripcion;

                            listaEmpleados.Add(vmEmp);
                        }
                    }
                }
            }

            return new ViewAsPdf(listaEmpleados)
            {
                FileName = "Altas-Afiliados.pdf"
            };
        }

        [AllowAnonymous]
        public ActionResult ImprimirBajasAfiliados(DateTime? Desde, DateTime? Hasta)
        {
            DateTime Start = (Desde != null) ? (DateTime)Desde : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime End = (Hasta != null) ? (DateTime)Hasta : DateTime.MaxValue;
            ViewBag.Start = Start;
            ViewBag.End = End;
            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            var empleados = db.EmpleadoEmpresa.Include(t => t.Empleado).Where(x => x.EsAfiliado && x.FechaBaja > Start && x.FechaBaja < End).ToList();

            if (empleados.Count > 0)
            {
                foreach (var empleado in empleados)
                {
                    var empEmp = db.Empleado.Where(x => x.IdEmpleado == empleado.idEmpleado).FirstOrDefault();
                    if (empEmp != null)
                    {
                        if (!empEmp.EsAfiliado)
                        {
                            VmEmpleados vmEmp = new VmEmpleados();
                            vmEmp.NombreEmpleado = empEmp.Apellido + " " + empEmp.Nombre;
                            vmEmp.CuilEmpleado = empEmp.Cuil;
                            vmEmp.LocalidadEmpleado = empEmp.Localidad.Nombre;
                            vmEmp.ProvinciaEmpleado = empEmp.Localidad.Provincia.Nombre;
                            vmEmp.CategoríaEmpleado = empleado.Categoria.Descripcion;
                            vmEmp.JornadaEmpleado = empleado.Jornada.Descripcion;

                            listaEmpleados.Add(vmEmp);
                        }
                    }
                }
            }

            return new ViewAsPdf(listaEmpleados)
            {
                FileName = "Bajas-Afiliados.pdf"
            };
        }
        #endregion

        #region Licencias
        [Authorize(Roles = "Admin, Fiscalizacion")]
        public ActionResult IndexLicenciasEmpleados(int idEmpresa = 0)
        {
            var licenciaEmpleado = db.LicenciaEmpleado.Include(l => l.EmpleadoEmpresa)
                                                      .Include(l => l.LicenciaLaboral);
            if (idEmpresa != 0)
            {
                licenciaEmpleado = licenciaEmpleado.Where(x => x.EmpleadoEmpresa.idEmpresa == idEmpresa);
            }
            return View(licenciaEmpleado.ToList());
        }

        [Authorize(Roles = "Admin, Fiscalizacion")]
        // GET: LicenciasEmpleados/Details/5
        public ActionResult DetailsLicenciasEmpleados(int? id)
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

        [Authorize(Roles = "Admin, Fiscalizacion")]
        public ActionResult ImpresionEmpleadosEnLicenciaPorMaternidad()
        {
            DateTime Hoy = DateTime.Today;
            List<VmEmpleados> listaEmpleados = new List<VmEmpleados>();

            var empleados = (from oEmpleados in db.Empleado
                            join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                            join oLicencias in db.LicenciaEmpleado on oEmpleadoEmpresa.idEmpleadoEmpresa equals oLicencias.IdEmpleadoEmpresa
                            where oEmpleadoEmpresa.FechaBaja == null && oLicencias.IdLicenciaLaboral == 2 && oLicencias.FechaBajaLicencia > Hoy
                            select oEmpleados).ToList();

            if (empleados.Count > 0)
            {
                foreach (var empleado in empleados)
                {
                    var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado &&
                                                               x.FechaBaja == null).FirstOrDefault();
                    if (empEmp != null)
                    {
                        VmEmpleados vmEmp = new VmEmpleados();
                        vmEmp.NombreEmpleado = empleado.Apellido + " " + empleado.Nombre;
                        vmEmp.CuilEmpleado = empleado.Cuil;
                        vmEmp.LocalidadEmpleado = empleado.Localidad.Nombre;
                        vmEmp.ProvinciaEmpleado = empleado.Localidad.Provincia.Nombre;
                        vmEmp.CategoríaEmpleado = empEmp.Categoria.Descripcion;
                        vmEmp.JornadaEmpleado = empEmp.Jornada.Descripcion;

                        listaEmpleados.Add(vmEmp);
                    }
                }
            }

            return new ViewAsPdf(listaEmpleados)
            {
                FileName = "Licencias-Maternidad.pdf"
            };
            //return View(listaEmpleados);
        }
        #endregion

        public decimal TruncateFunction(decimal number, int digits)
        {
            decimal stepper = (decimal)(Math.Pow(10.0, (double)digits));
            int temp = (int)(stepper * number);
            return (decimal)temp / stepper;
        }

        ////Se Puede Mejorar
        //public ActionResult ImpresionEmpresasExt(string sortOrder, string searchString, int IdLocalidadSeleccionada = 0, int IdProvinciaSeleccionada = 0, int IdActividadSeleccionada = 0)
        //{
        //    var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);

        //    if (IdProvinciaSeleccionada != 0)
        //    {
        //        empresa.Where(x => x.Localidad.IdProvincia == IdProvinciaSeleccionada);
        //    }

        //    if (IdLocalidadSeleccionada != 0)
        //    {
        //        empresa = empresa.Where(x => x.IdLocalidad == IdLocalidadSeleccionada);
        //    }

        //    if (IdActividadSeleccionada != 0)
        //    {
        //        empresa = empresa.Where(x => x.IdActividad == IdActividadSeleccionada);
        //    }

        //    if (!string.IsNullOrEmpty(searchString))
        //    {
        //        empresa = empresa.Where(x => x.RazonSocial.Contains(searchString));
        //    }

        //    switch (sortOrder)
        //    {
        //        case "name_desc":
        //            empresa = empresa.OrderByDescending(x => x.RazonSocial);
        //            break;
        //        case "nombreFantasia":
        //            empresa = empresa.OrderBy(s => s.NombreFantasia);
        //            break;
        //        case "nombreFantasia_desc":
        //            empresa = empresa.OrderByDescending(s => s.NombreFantasia);
        //            break;
        //        case "localidad":
        //            empresa = empresa.Include(t => t.Localidad).OrderBy(s => s.Localidad.Nombre);
        //            break;
        //        case "localidad_desc":
        //            empresa = empresa.Include(t => t.Localidad).OrderByDescending(s => s.Localidad.Nombre);
        //            break;
        //        default:
        //            empresa = empresa.OrderBy(x => x.RazonSocial);
        //            break;
        //    }

        //    if (empresa.ToList().Count < 1)
        //    {
        //        ViewBag.CurrentSort = sortOrder;
        //        ViewBag.searchString = searchString;
        //        return View();
        //    }

        //    string[] filtros = new string[] { sortOrder, searchString, IdLocalidadSeleccionada.ToString(), IdProvinciaSeleccionada.ToString(), IdActividadSeleccionada.ToString() };
        //    Session["filtros"] = filtros;

        //    return Redirect("~/Areas/Administrador/Reports/EmpresasExt.aspx");
        //}
    }
}