using DAL;
using DAL.Models;
using DAL.ViewModels;
using System;
using System.Web.Mvc;
using System.Data.Entity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Rotativa;
using System.Net;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
    public class RendicionController : Controller
    {
        private SecModel db = new SecModel();

        #region BoletaAportes
        [Authorize(Roles = "Admin, Fiscalizacion, Finanzas")]
        // GET: Administrador/Listados/BoletaAportes
        public ActionResult IndexBoletaAportes(int? mes, int? anio, int estadoPago = 0, int idEmpresa = 0)
        {
            var boletaAportes = db.BoletaAportes.Include(b => b.DeclaracionJurada).Where(x => x.DeBaja == false).ToList();

            if (idEmpresa != 0)
            {
                boletaAportes = boletaAportes.Where(x => x.DeclaracionJurada.idEmpresa == idEmpresa).ToList();
            }

            if (estadoPago == 1)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == true).ToList();
            }

            if (estadoPago == 2)
            {
                boletaAportes = boletaAportes.Where(x => x.BoletaPagada == false).ToList();
            }

            if (mes != null && anio != null)
            {
                boletaAportes = boletaAportes.Where(x => x.MesBoleta == mes && x.AnioBoleta == anio).ToList();
            }

            foreach (var boleta in boletaAportes)
            {
                #region Old
                //DeclaracionJurada ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == boleta.IdDeclaracionJurada).FirstOrDefault();

                //var empleados = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == boleta.IdDeclaracionJurada).ToList();

                //int count2 = 0, count5 = 0;
                //decimal sueldos2 = 0, sueldos5 = 0;
                //foreach (var empleado in empleados)
                //{
                //    sueldos2 += empleado.Sueldo;
                //    count2++;
                //    var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleado.IdEmpleadoEmpresa).FirstOrDefault();
                //    if (afiliado != null)
                //    {
                //        if (afiliado.FechaAlta.Year < boleta.DeclaracionJurada.anio)
                //        {
                //            if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > boleta.DeclaracionJurada.anio || (afiliado.FechaBaja.Value.Year == boleta.DeclaracionJurada.anio && afiliado.FechaBaja.Value.Month >= boleta.DeclaracionJurada.mes))
                //            {
                //                sueldos5 += empleado.SueldoBase;
                //                count5++;
                //            }
                //        }
                //        else if (afiliado.FechaAlta.Year == boleta.DeclaracionJurada.anio && afiliado.FechaAlta.Month <= boleta.DeclaracionJurada.mes)
                //        {
                //            if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > boleta.DeclaracionJurada.anio || (afiliado.FechaBaja.Value.Year == boleta.DeclaracionJurada.anio && afiliado.FechaBaja.Value.Month >= boleta.DeclaracionJurada.mes))
                //            {
                //                sueldos5 += empleado.SueldoBase;
                //                count5++;
                //            }
                //        }
                //    }
                //}

                //decimal total2 = (sueldos2 / 100) * 2;
                //decimal total5 = (sueldos5 / 100) * 5;

                //decimal mora = (boleta.RecargoMora != null) ? (decimal)boleta.RecargoMora : 0;
                //(Math.Truncate(((sueldos / 100) * 5) * 100) / 100).ToString();
                #endregion

                boleta.TotalDepositado2 = TruncateFunction(boleta.Aportes2, 2);
                boleta.TotalDepositado5 = TruncateFunction(boleta.Aportes5, 2);
                boleta.TotalDepositado = TruncateFunction(boleta.Aportes2 + boleta.Aportes5 + ((boleta.RecargoMora != null) ? (decimal)boleta.RecargoMora : 0), 2);
            }

            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            List<Empresa> empresas = db.Empresa.OrderBy(x => x.RazonSocial).ToList();
            empresas.Insert(0, new Empresa { IdEmpresa = 0, RazonSocial = "Todas" });
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial", idEmpresa);
            ViewBag.estadoPago = estadoPago;

            return View(boletaAportes.ToList());
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
                var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleado.IdEmpleadoEmpresa).FirstOrDefault();
                if (afiliado != null)
                {
                    if (afiliado.FechaAlta.Year < ddjj.anio)
                    {
                        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                        {
                            //if (empleado.idJornadaLaboral == 1 || empleado.idJornadaLaboral == 2)
                            //{
                            //if (empleado.SueldoBase > 0)
                            //{
                            sueldos5 += empleado.SueldoBase;
                            //}
                            //else
                            //{
                            //    sueldos5 += empleado.Sueldo;
                            //}
                            //}
                            //else
                            //{
                            //    sueldos5 += empleado.Sueldo;
                            //}
                            count5++;
                        }
                    }
                    else if (afiliado.FechaAlta.Year == ddjj.anio && afiliado.FechaAlta.Month <= ddjj.mes)
                    {
                        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > ddjj.anio || (afiliado.FechaBaja.Value.Year == ddjj.anio && afiliado.FechaBaja.Value.Month >= ddjj.mes))
                        {
                            //if (empleado.idJornadaLaboral == 1 || empleado.idJornadaLaboral == 2)
                            //{
                            //if (empleado.SueldoBase > 0)
                            //{
                            sueldos5 += empleado.SueldoBase;
                            //}
                            //else
                            //{
                            //    sueldos5 += empleado.Sueldo;
                            //}
                            //}
                            //else
                            //{
                            //    sueldos5 += empleado.Sueldo;
                            //}
                            count5++;
                        }
                    }
                }
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

            return View(boletaAportes);
        }
        #endregion

        // GET: Administrador/Rendicion/RendicionBcoNacion
        public ActionResult RendicionBcoNacion()
        {
            return View();
        }

        // GET: Administrador/Rendicion/RendicionBcoHipotecario
        public ActionResult RendicionBcoHipotecario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RendicionBcoNacion(HttpPostedFileBase fileBancoNacion)
        {
            List<BoletaAportes> boletasPagadas = new List<BoletaAportes>();
            List<MessageVm> errores = new List<MessageVm>();
            try
            {
                if (fileBancoNacion != null && fileBancoNacion.ContentLength > 0)
                {                  
                    List<string> rows = new List<string>();
                    StreamReader fileContent = new StreamReader(fileBancoNacion.InputStream);
                    do
                    {
                        rows.Add(fileContent.ReadLine());
                    }
                    while (!fileContent.EndOfStream);
                    foreach (var row in rows)
                    {
                        if(row.Length == 160)
                        {
                            string datosRecaudacion = row.Substring(58, 79);
                            int idDeclaracion = int.Parse(datosRecaudacion.Substring(4, 8));
                            string _fechaPago = row.Substring(18, 8);
                            int _anio = int.Parse(_fechaPago.Substring(0, 4));
                            int _mes = int.Parse(_fechaPago.Substring(4, 2));
                            int _dia = int.Parse(_fechaPago.Substring(6, 2));
                            int importe = int.Parse(datosRecaudacion.Substring(23,6));
                            int importeDecimal = int.Parse(datosRecaudacion.Substring(29,2));
                            decimal monto = importe + (decimal)importeDecimal/100;

                            DateTime fechaPago = new DateTime(_anio, _mes, _dia);

                            BoletaAportes boleta = db.BoletaAportes.Where(x => x.IdDeclaracionJurada == idDeclaracion).FirstOrDefault();
                            if (boleta != null)
                            {
                                boletasPagadas.Add(boleta);
                                if (!boleta.BoletaPagada)
                                {
                                    boleta.BoletaPagada = true;
                                    boleta.FechaPago = fechaPago;
                                    boleta.TotalPagado = monto;
                                    db.SaveChanges();
                                    errores.Add(new MessageVm()
                                    {
                                        Type = "alert-success",
                                        Message = "Boleta en linea: " + (rows.IndexOf(row) + 1) + ", marcada como pagada.",
                                        Dismissible = true
                                    });
                                }
                                else
                                {
                                    errores.Add(new MessageVm(){
                                        Type = "alert-danger",
                                        Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Boleta pagada.",
                                        Dismissible = true
                                    });
                                }
                            }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Boleta no existente.",
                                    Dismissible = true
                                });
                            }
                        }
                        else
                        {
                            errores.Add(new MessageVm()
                            {
                                Type = "alert-danger",
                                Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Largo de linea no es valido.",
                                Dismissible = true
                            });
                        }
                    }
                }
                else
                {
                    errores.Add(new MessageVm()
                    {
                        Type = "alert-danger",
                        Message = "Error, Archivo vacio o no valido!!",
                        Dismissible = true
                    });
                }
            }
            catch (Exception e)
            {
                errores.Add(new MessageVm()
                {
                    Type = "alert-danger",
                    Message = "A Ocurrido un Error, por favor intente nuevamente!!",
                    Dismissible = true
                });
                //log
                string log = e.Message;
            }

            ViewBag.BoletasPagadas = boletasPagadas.Select(x => x.IdBoleta).ToArray();
            ViewBag.ErroresBcoNacion = errores;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RendicionBcoHipotecario(HttpPostedFileBase fileBancoHipotecario)
        {
            List<BoletaAportes> boletasPagadas = new List<BoletaAportes>();
            List<MessageVm> errores = new List<MessageVm>();
            try
            {
                if (fileBancoHipotecario != null && fileBancoHipotecario.ContentLength > 0)
                {
                    List<string> rows = new List<string>();
                    StreamReader fileContent = new StreamReader(fileBancoHipotecario.InputStream);
                    do
                    {
                        rows.Add(fileContent.ReadLine());
                    }
                    while (!fileContent.EndOfStream);
                    foreach (var row in rows)
                    {
                        if(rows.IndexOf(row) != 0 && rows.IndexOf(row) != (rows.Count() - 1))
                        {
                            if (row.Length == 775)
                            {
                                int idDeclaracion = int.Parse(row.Substring(696, 8));
                                string _fechaPago = row.Substring(516, 8);
                                int _anio = int.Parse(_fechaPago.Substring(0, 4));
                                int _mes = int.Parse(_fechaPago.Substring(4, 2));
                                int _dia = int.Parse(_fechaPago.Substring(6, 2));
                                int importe = int.Parse(row.Substring(715, 6));
                                int importeDecimal = int.Parse(row.Substring(721, 2));
                                decimal monto = importe + (decimal)importeDecimal / 100;

                                DateTime fechaPago = new DateTime(_anio, _mes, _dia);

                                BoletaAportes boleta = db.BoletaAportes.Where(x => x.IdDeclaracionJurada == idDeclaracion).FirstOrDefault();
                                if (boleta != null)
                                {
                                    boletasPagadas.Add(boleta);
                                    if (!boleta.BoletaPagada)
                                    {
                                        boleta.BoletaPagada = true;
                                        boleta.FechaPago = fechaPago;
                                        boleta.TotalPagado = monto;
                                        db.SaveChanges();
                                        errores.Add(new MessageVm()
                                        {
                                            Type = "alert-success",
                                            Message = "Boleta en linea: " + (rows.IndexOf(row) + 1) + ", marcada como pagada.",
                                            Dismissible = true
                                        });
                                    }
                                    else
                                    {
                                        errores.Add(new MessageVm()
                                        {
                                            Type = "alert-danger",
                                            Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Boleta pagada.",
                                            Dismissible = true
                                        });
                                    }
                                }
                                else
                                {
                                    errores.Add(new MessageVm()
                                    {
                                        Type = "alert-danger",
                                        Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Boleta no existente.",
                                        Dismissible = true
                                    });
                                }
                            }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Largo de linea no es valido.",
                                    Dismissible = true
                                });
                            }
                            
                        }
                    }
                }
                else
                {
                    errores.Add(new MessageVm()
                    {
                        Type = "alert-danger",
                        Message = "Error, Archivo vacio o no valido!!",
                        Dismissible = true
                    });
                }
            }
            catch (Exception e)
            {
                errores.Add(new MessageVm()
                {
                    Type = "alert-danger",
                    Message = "A Ocurrido un Error, por favor intente nuevamente!!",
                    Dismissible = true
                });
                //log
                string log = e.Message;
            }

            ViewBag.BoletasPagadas = boletasPagadas.Select(x => x.IdBoleta).ToArray();
            ViewBag.ErroresBcoHipotecario = errores;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PagarImprimirBoletas(int[] IdsBoletas)
        {
            string BoletasPagadas = JsonConvert.SerializeObject(IdsBoletas);

            foreach (var IdBoleta in IdsBoletas)
            {
                BoletaAportes boletaAportes = db.BoletaAportes.Find(IdsBoletas);
                boletaAportes.BoletaPagada = true;
                boletaAportes.FechaPago = DateTime.Today;
                decimal total2 = (boletaAportes.TotalSueldos2 / 100) * 2;
                decimal total5 = (boletaAportes.TotalSueldos5 / 100) * 5;
                boletaAportes.TotalPagado = Math.Truncate(total2 + total5);

                db.SaveChanges();
            }

            return RedirectToAction("ImprimirBoletasPagadas", "Rendicion", new { BoletasPagadas = BoletasPagadas });
        }

        [AllowAnonymous]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ImprimirBoletasPagadas(string BoletasPagadas = "null")
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


            foreach (var boleta in BoletasAportesPagadas)
            {
                decimal total2 = (boleta.TotalSueldos2 / 100) * 2;
                decimal total5 = (boleta.TotalSueldos5 / 100) * 5;

                if(boleta.BoletaPagada == false)
                {
                    boleta.BoletaPagada = true;
                    boleta.FechaPago = DateTime.Today;
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
                    var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();
                    if (afiliado != null)
                    {
                        if (afiliado.FechaAlta.Year < declaracion.anio)
                        {
                            if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracion.anio || (afiliado.FechaBaja.Value.Year == declaracion.anio && afiliado.FechaBaja.Value.Month >= declaracion.mes))
                            {
                                sueldos5 += detalle.SueldoBase;
                            }
                        }
                        else if (afiliado.FechaAlta.Year == declaracion.anio && afiliado.FechaAlta.Month <= declaracion.mes)
                        {
                            if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracion.anio || (afiliado.FechaBaja.Value.Year == declaracion.anio && afiliado.FechaBaja.Value.Month >= declaracion.mes))
                            {
                                sueldos5 += detalle.SueldoBase;
                            }
                        }
                    }
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

        public decimal TruncateFunction(decimal number, int digits)
        {
            decimal stepper = (decimal)(Math.Pow(10.0, (double)digits));
            int temp = (int)(stepper * number);
            return (decimal)temp / stepper;
        }
    }
}