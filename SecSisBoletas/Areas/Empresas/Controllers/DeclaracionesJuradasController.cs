using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using DAL;
using DAL.Models;
using Rotativa;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class DeclaracionesJuradasController : Controller
    {
        private SecModel db = new SecModel();
        private EmpleadosController empl = new EmpleadosController();

        // GET: DeclaracionesJuradas
        public ActionResult Index(int? mes, int? anio, int estadoPago = 0)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            var declaracionJurada = db.DeclaracionJurada.Include(d => d.Empresa);
            int IdEmpresa = Convert.ToInt32(claim.Value);
            declaracionJurada = db.DeclaracionJurada.Include(d => d.Empresa).Where(x => x.idEmpresa == IdEmpresa && x.DeBaja == false).OrderByDescending(x => x.fecha);

            if (estadoPago == 0)
            {
                if(mes != null && anio != null)
                {
                    declaracionJurada = declaracionJurada.Where(x => x.mes == mes && x.anio == anio);
                }
            }
            else
            {
                if (estadoPago == 1)
                {
                    //Declaraciones Juradas Pagadas
                }
                else if (estadoPago == 2)
                {
                    //Declaraciones Juradas Impagas
                }
            }

            foreach (var ddjj in declaracionJurada)
            {
                switch(ddjj.mes)
                {
                    case 1:
                        ddjj.NombreMes = "Enero";
                        break;
                    case 2:
                        ddjj.NombreMes = "Febrero";
                        break;
                    case 3:
                        ddjj.NombreMes = "Marzo";
                        break;
                    case 4:
                        ddjj.NombreMes = "Abril";
                        break;
                    case 5:
                        ddjj.NombreMes = "Mayo";
                        break;
                    case 6:
                        ddjj.NombreMes = "Junio";
                        break;
                    case 7:
                        ddjj.NombreMes = "Julio";
                        break;
                    case 8:
                        ddjj.NombreMes = "Agosto";
                        break;
                    case 9:
                        ddjj.NombreMes = "Septiembre";
                        break;
                    case 10:
                        ddjj.NombreMes = "Octubre";
                        break;
                    case 11:
                        ddjj.NombreMes = "Noviembre";
                        break;
                    case 12:
                        ddjj.NombreMes = "Diciembre";
                        break;
                }
            }

            ViewBag.Mes = mes;
            ViewBag.Anio = anio;
            List<Empresa> empresas = db.Empresa.ToList();
            empresas.Insert(0, new Empresa { IdEmpresa = 0, RazonSocial = "Todas" });
            ViewBag.IdEmpresa = new SelectList(empresas, "IdEmpresa", "RazonSocial", IdEmpresa);
            ViewBag.estadoPago = estadoPago;

            return View(declaracionJurada.OrderByDescending(x => x.anio).ThenByDescending(x => x.mes).ToList());
        }

        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file, int mes, int anio)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            int errorRenglon = 1;
            int idDeclaracion = 0;

            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    DeclaracionJurada ddjj = db.DeclaracionJurada.Where(x => x.idEmpresa == IdEmpresa &&
                                                                             x.mes == mes &&
                                                                             x.anio == anio &&
                                                                             x.DeBaja == false).FirstOrDefault();
                    if(ddjj == null)
                    {
                        ddjj = new DeclaracionJurada();
                        ddjj.anio = anio;
                        ddjj.mes = mes;
                        ddjj.idEmpresa = IdEmpresa;
                        ddjj.fecha = DateTime.Now;
                        db.DeclaracionJurada.Add(ddjj);
                        db.SaveChanges();
                        idDeclaracion = ddjj.IdDeclaracionJurada;
                        bool error = false;
                        List<string> rows = new List<string>();
                        List<DetalleDeclaracionJurada> detallesDeclaracionJurada = new List<DetalleDeclaracionJurada>();
                        StreamReader fileContent = new StreamReader(file.InputStream);
                        do
                        {
                            rows.Add(fileContent.ReadLine());
                        }
                        while (!fileContent.EndOfStream);
                        foreach (var row in rows)
                        {
                            string[] detalles = row.Split(';');
                            if (detalles.Length == 4)
                            {
                                DetalleDeclaracionJurada detalle = new DetalleDeclaracionJurada();
                                detalle.IdDeclaracionJurada = idDeclaracion;
                                EmpleadoEmpresa empEmp = new EmpleadoEmpresa();
                                string cuil = detalles[0];
                                Empleado emp = (from oEmpleado in db.Empleado
                                                    join oEmpEmp in db.EmpleadoEmpresa on oEmpleado.IdEmpleado equals oEmpEmp.idEmpleado
                                                where oEmpEmp.idEmpresa == IdEmpresa && 
                                                      //oEmpEmp.FechaBaja == null &&
                                                      oEmpleado.Cuil == cuil
                                                select oEmpleado).FirstOrDefault();
                                
                                if (emp != null)
                                {
                                    empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == emp.IdEmpleado && x.idEmpresa == IdEmpresa 
                                                                        & (x.FechaAlta.Year < ddjj.anio || (x.FechaAlta.Year == ddjj.anio && x.FechaAlta.Month <= ddjj.mes)))
                                                               .FirstOrDefault();
                                    if(emp == null || empEmp == null)
                                    {
                                        ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon + Environment.NewLine + "Empleado no Encontrado!!";
                                        error = true;
                                        break;
                                    }
                                    if (empEmp.FechaBaja != null && empEmp.FechaBaja.Value.Year < ddjj.anio)
                                    {
                                        ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon + Environment.NewLine + "Empleado esta de baja!!";
                                        error = true;
                                        break;
                                    }
                                    else if (empEmp.FechaBaja != null && empEmp.FechaBaja.Value.Year == ddjj.anio && empEmp.FechaBaja.Value.Month < ddjj.mes)
                                    {
                                        ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon + Environment.NewLine + "Empleado esta de baja!!";
                                        error = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon + Environment.NewLine + "Empleado no Encontrado!!";
                                    error = true;
                                    break;
                                }
                                detalle.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                                detalle.idCategoria = empEmp.IdCategoria;
                                detalle.idJornadaLaboral = empEmp.IdJornada;
                                if (int.TryParse(detalles[1], out int idliquidacion))
                                {
                                    detalle.IdLiquidacionProporcional = int.Parse(detalles[1]);
                                }
                                else
                                {
                                    error = true; break;
                                }
                                decimal sueldo = 0;
                                if (decimal.TryParse(detalles[2], out sueldo))
                                {
                                    if (comprobarSueldoBasico(empEmp.idEmpleadoEmpresa, detalle.IdDeclaracionJurada, decimal.Parse(detalles[2])))
                                    {
                                        detalle.Sueldo = decimal.Parse(detalles[2]);
                                    }
                                    else
                                    {
                                        if(detalle.IdLiquidacionProporcional != 1)
                                        {
                                            detalle.Sueldo = decimal.Parse(detalles[2]);
                                        }
                                        else
                                        {
                                            ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon + Environment.NewLine + "Sueldo 2% Menor al minimo, corrija el sueldo y vuelva a intentar!!";
                                            error = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon + Environment.NewLine + "Sueldo 2% no Valido!!";
                                    error = true;
                                    break;
                                }
                                decimal sueldoBase = 0;
                                if (decimal.TryParse(detalles[3], out sueldoBase))
                                {
                                    detalle.SueldoBase = decimal.Parse(detalles[3]);
                                }
                                detallesDeclaracionJurada.Add(detalle);
                            }
                            else
                            {
                                error = true;
                            }
                            errorRenglon++;
                        }
                        if (error == false)
                        {
                            //db.DeclaracionJurada.Add(ddjj);
                            //db.SaveChanges();
                            foreach (DetalleDeclaracionJurada detalle in detallesDeclaracionJurada)
                            {
                                detalle.IdDeclaracionJurada = ddjj.IdDeclaracionJurada;
                                db.DetalleDeclaracionJurada.Add(detalle);
                                db.SaveChanges();

                                var liquidacionProporcional = db.LiquidacionProporcionalEmpleado.Where(x => x.IdDetalleDeclaracionJurada == detalle.IdDetalleDeclaracionJurada).FirstOrDefault();
                                if (detalle.IdLiquidacionProporcional != null && detalle.IdLiquidacionProporcional != 1)
                                {
                                    if (liquidacionProporcional != null)
                                    {
                                        if (liquidacionProporcional.IdLiquidacionProporcional != detalle.IdLiquidacionProporcional)
                                        {
                                            liquidacionProporcional.IdLiquidacionProporcional = (int)detalle.IdLiquidacionProporcional;
                                        }
                                    }
                                    else
                                    {
                                        db.LiquidacionProporcionalEmpleado.Add(new LiquidacionProporcionalEmpleado()
                                        {
                                            IdLiquidacionProporcional = (int)detalle.IdLiquidacionProporcional,
                                            IdDetalleDeclaracionJurada = detalle.IdDetalleDeclaracionJurada
                                        });
                                    }
                                }
                                else
                                {
                                    if (liquidacionProporcional != null)
                                    {
                                        db.LiquidacionProporcionalEmpleado.Remove(liquidacionProporcional);
                                    }
                                }
                            }
                            db.SaveChanges();

                            ViewBag.Message = "Empleados Importados Correctamente!!";
                            return View();
                        }
                        else
                        {
                            //ViewBag.Message = (!string.IsNullOrEmpty(ViewBag.Message)) ? ViewBag.Message + "Error al importar la Declaracion Jurada!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon : "Error al importar la Declaracion Jurada!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon;
                            //ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon;
                            RollBackDeclaracion(idDeclaracion);
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Ya existe una declaracion Jurada para la fecha Seleccionada!!";
                        RollBackDeclaracion(idDeclaracion);
                        return View();
                    }
                }
                else
                {
                    ViewBag.Message = "Error, Archivo vacio o no valido!!";
                    RollBackDeclaracion(idDeclaracion);
                    return View();
                }
            }
            catch (Exception e)
            {
                RollBackDeclaracion(idDeclaracion);
                ViewBag.Message = (!string.IsNullOrEmpty(ViewBag.Message)) ? ViewBag.Message + "Error al importar la Declaracion Jurada!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon : "Error al importar la Declaracion Jurada!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon;
                return View();
            }
        }


        public void RollBackDeclaracion(int idDeclaracion)
        {
            if (idDeclaracion != 0)
            {
                var detalles = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == idDeclaracion).ToList();
                if (detalles.Count > 0)
                {
                    List<LiquidacionProporcionalEmpleado> liquidaciones = new List<LiquidacionProporcionalEmpleado>();
                    foreach (var item in detalles)
                    {
                        var liquidacion = db.LiquidacionProporcionalEmpleado.Where(x => x.IdDetalleDeclaracionJurada == item.IdDetalleDeclaracionJurada).FirstOrDefault();
                        if (liquidacion != null)
                        {
                            liquidaciones.Add(liquidacion);
                        }
                    }
                    if (liquidaciones.Count > 0)
                    {
                        db.LiquidacionProporcionalEmpleado.RemoveRange(liquidaciones);
                    }
                    db.DetalleDeclaracionJurada.RemoveRange(detalles);
                }
                db.DeclaracionJurada.Remove(db.DeclaracionJurada.Find(idDeclaracion));
                db.SaveChanges();
            }
        }
        // GET: DeclaracionesJuradas/Details/5
        public ActionResult Details(int? id)
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

        // GET: DeclaracionesJuradas/Create
        public ActionResult Create()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.IdEmpresa == IdEmpresa), "IdEmpresa", "Cuit", IdEmpresa);
            return View();
        }

        // POST: DeclaracionesJuradas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdDeclaracionJurada,idEmpresa,mes,anio,fecha")] DeclaracionJurada declaracionJurada)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            if (ModelState.IsValid)
            {
                if (db.DeclaracionJurada.Where(x => x.idEmpresa == declaracionJurada.idEmpresa &&
                                                    x.mes == declaracionJurada.mes &&
                                                    x.anio == declaracionJurada.anio && 
                                                    x.DeBaja == false).FirstOrDefault() != null)
                {
                    ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.IdEmpresa == IdEmpresa), "IdEmpresa", "Cuit", IdEmpresa);
                    ModelState.AddModelError("Mes", "ya existe una declaracion Jurada para este Mes y Año");
                    return View(declaracionJurada);
                }
                declaracionJurada.fecha = DateTime.Now;
                db.DeclaracionJurada.Add(declaracionJurada);
                db.SaveChanges();
                return RedirectToAction("CreateMessage", "DetallesDeclaracionJurada", new { idDeclaracionJurada = declaracionJurada.IdDeclaracionJurada });
            }

            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.IdEmpresa == IdEmpresa), "IdEmpresa", "Cuit", IdEmpresa);
            return View(declaracionJurada);
        }

        public ActionResult CreateMessage(int idDeclaracionJurada)
        {
            ViewBag.idDeclaracion = idDeclaracionJurada;
            return View();
        }

        // GET: DeclaracionesJuradas/Edit/5
        public ActionResult Edit(int? id)
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
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.IdEmpresa == IdEmpresa), "IdEmpresa", "Cuit", IdEmpresa);
            return View(declaracionJurada);
        }

        // POST: DeclaracionesJuradas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdDeclaracionJurada,idEmpresa,mes,anio,fecha")] DeclaracionJurada declaracionJurada)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            ViewBag.idEmpresa = new SelectList(db.Empresa.Where(x => x.IdEmpresa == IdEmpresa), "IdEmpresa", "Cuit", IdEmpresa);
            if (ModelState.IsValid)
            {
                if (db.DeclaracionJurada.Where(x => x.idEmpresa == declaracionJurada.idEmpresa &&
                                                    x.mes == declaracionJurada.mes &&
                                                    x.anio == declaracionJurada.anio &&
                                                    x.DeBaja == false &&
                                                    x.IdDeclaracionJurada != declaracionJurada.IdDeclaracionJurada).FirstOrDefault() != null)
                {
                    ModelState.AddModelError("Mes", "ya existe una declaracion Jurada para este Mes y Año");
                    return View(declaracionJurada);
                }
                db.Entry(declaracionJurada).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EditMessage");
            }
            return View(declaracionJurada);
        }

        public ActionResult EditMessage()
        {
            return View();
        }

        //// GET: DeclaracionesJuradas/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    DeclaracionJurada declaracionJurada = db.DeclaracionJurada.Find(id);
        //    if (declaracionJurada == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if(db.BoletaAportes.Where(x => x.IdDeclaracionJurada == id).FirstOrDefault() != null)
        //    {
        //        return RedirectToAction("CantDeleteMessage");
        //    }
        //    return View(declaracionJurada);
        //}

        //// POST: DeclaracionesJuradas/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    if (db.BoletaAportes.Where(x => x.IdDeclaracionJurada == id).FirstOrDefault() != null)
        //    {
        //        return RedirectToAction("CantDeleteMessage");
        //    }
        //    var detalles = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == id).ToList();
        //    foreach (DetalleDeclaracionJurada detalle in detalles)
        //    {
        //        if (detalle != null)
        //        {
        //            var liquidacion = db.LiquidacionProporcionalEmpleado.Where(x => x.IdDetalleDeclaracionJurada == detalle.IdDetalleDeclaracionJurada).FirstOrDefault();
        //            if(liquidacion != null)
        //            {
        //                db.LiquidacionProporcionalEmpleado.Remove(liquidacion);
        //                db.SaveChanges();
        //            }
        //            db.DetalleDeclaracionJurada.Remove(detalle);
        //        }
        //    }
        //    db.SaveChanges();
        //    DeclaracionJurada declaracionJurada = db.DeclaracionJurada.Find(id);
        //    db.DeclaracionJurada.Remove(declaracionJurada);
        //    db.SaveChanges();
        //    return RedirectToAction("DeleteMessage");
        //}

        //public ActionResult DeleteMessage()
        //{
        //    return View();
        //}

        public ActionResult CantDeleteMessage()
        {
            return View();
        }

        public bool comprobarSueldoBasico(int IdEmpleadoEmpresa, int IdDeclaracionJurada, decimal sueldoIngresado)
        {
            #region Old
            ////var declaracionJurada = db.DeclaracionJurada.Find(IdDeclaracionJurada);
            //var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == IdEmpleadoEmpresa).FirstOrDefault();
            //decimal sueldo = 1;

            ///*
            // * Establece el sueldo minimo basandose en la categoria y jornada del empleado
            // */
            //DateTime fechaSueldo = new DateTime(anio, mes, DateTime.Today.Day); 
            //var sueldosBasicos = db.SueldoBasico.Where(x => x.IdCategoria == empleadoEmpresa.IdCategoria &&
            //                                                x.Desde <= fechaSueldo &&
            //                                                x.Hasta >= fechaSueldo).ToList();
            //if(sueldosBasicos != null && sueldosBasicos.Count > 0)
            //{
            //    foreach (var sueldos in sueldosBasicos)
            //    {
            //        if (sueldos != null && sueldos.Desde.Year == anio)
            //        {
            //            if (sueldos.Hasta.Year == anio)
            //            {
            //                if (sueldos.Desde.Month <= mes && sueldos.Hasta.Month >= mes)
            //                {
            //                    if (empleadoEmpresa.IdJornada == 2)
            //                    {
            //                        sueldo = (sueldos.Monto / 2);
            //                    }
            //                    else if (empleadoEmpresa.IdJornada == 3)
            //                    {
            //                        sueldo = sueldos.Monto;
            //                    }
            //                }
            //            }
            //            if (sueldos.Hasta.Year > anio)
            //            {
            //                if (empleadoEmpresa.IdJornada == 1)
            //                {
            //                    sueldo = 1;
            //                }
            //                else if (empleadoEmpresa.IdJornada == 2)
            //                {
            //                    sueldo = (sueldos.Monto / 2);
            //                }
            //                else if (empleadoEmpresa.IdJornada == 3)
            //                {
            //                    sueldo = sueldos.Monto;
            //                }
            //            }
            //        }
            //    }

            //    /*
            //     * Sueldo basico mas antiguedad
            //     */
            //    decimal antiguedad = MonthDifference(new DateTime(anio, mes, 1), empleadoEmpresa.FechaAlta);
            //    antiguedad = antiguedad / 12;
            //    sueldo += ((sueldo / 100) * Math.Truncate(antiguedad));

            //    /*
            //     * Sueldo basico mas 
            //     */
            //    sueldo += ((sueldo / 100) * (decimal)8.33);

            //    /*
            //     * Verifica si el empleado esta en licencia en el momento de la declaracion jurada, si esta de licencia, el sistema
            //     * le permite ingresar un sueldo por debajo del minimo
            //     */
            //    bool enLicencia = false;

            //    foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa))
            //    {
            //        if (licencia != null)
            //        {
            //            if(licencia.IdLicenciaLaboral != 3)
            //            {
            //                if (licencia.FechaBajaLicencia.Value.Year == anio)
            //                {
            //                    if (licencia.FechaAltaLicencia.Month <= mes && licencia.FechaBajaLicencia.Value.Month >= mes)
            //                    {
            //                        enLicencia = true;
            //                        sueldo = 0;
            //                    }
            //                }
            //                if (licencia.FechaBajaLicencia.Value.Year > anio)
            //                {
            //                    if (licencia.FechaAltaLicencia.Year <= anio && licencia.FechaAltaLicencia.Month <= mes)
            //                    {
            //                        enLicencia = true;
            //                        sueldo = 0;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if ((licencia.FechaBajaLicencia.Value.Year == anio &&
            //                    licencia.FechaBajaLicencia.Value.Month == mes) ||
            //                    licencia.FechaBajaLicencia.Value.Year == anio &&
            //                    licencia.FechaBajaLicencia.Value.Month == mes -1)
            //                {
            //                    enLicencia = true;
            //                }
            //                else if (licencia.FechaBajaLicencia.Value.Year == (anio - 1) &&
            //                    licencia.FechaBajaLicencia.Value.Month == 12 && mes == 1)
            //                {
            //                    enLicencia = true;
            //                }
            //            }

            //        }
            //        if (enLicencia)
            //        {
            //            switch (licencia.IdLicenciaLaboral)
            //            {
            //                case 1://Enfermedad
            //                    if ((licencia.FechaBajaLicencia - licencia.FechaAltaLicencia) > new TimeSpan(365, 0, 0, 0))
            //                    {
            //                        sueldo = 1;
            //                        DateTime fecha = new DateTime(anio, mes, 1);
            //                        if (MonthDifference(fecha, licencia.FechaAltaLicencia) >= 12)
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    break;
            //                case 2://Maternidad
            //                    if (licencia.FechaAltaLicencia.Month == mes)
            //                    {
            //                        //Si el/la empleado/a entro en licencia por maternidad habiendo trabajado
            //                        //almenos un dia el sistema solicita ingresar un sueldo mayor a 1
            //                        if (licencia.FechaAltaLicencia.Day > 1)
            //                        {
            //                            sueldo = 1;
            //                        }
            //                    }
            //                    if (licencia.FechaBajaLicencia.Value.Month == mes)
            //                    {
            //                        //Si el/la empleado/a volvio de licencia por maternidad antes de terminar el mes
            //                        //el sistema solicita ingresar un sueldo mayor a 1
            //                        if (licencia.FechaAltaLicencia.Day < 31)
            //                        {
            //                            sueldo = 1;
            //                        }
            //                    }
            //                    break;
            //                case 3://Vacaciones
            //                       //Si el/la empleado/a estuvo de vacaciones el mes anterior a la declaracion jurada
            //                       //el sistema permite ingresar un sueldo menor que el minimo
            //                    if (licencia.FechaBajaLicencia.Value.Year == anio)
            //                    {
            //                        if ((licencia.FechaBajaLicencia.Value.Month == (mes)) ||
            //                            (licencia.FechaBajaLicencia.Value.Month == (mes - 1)))
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    if (licencia.FechaBajaLicencia.Value.Year == (anio - 1) &&
            //                        licencia.FechaBajaLicencia.Value.Month == 12 && mes == 1)
            //                    {
            //                        sueldo = 0;
            //                    }
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }

            //    }
            //}
            //else
            //{
            //    //"No Hay Sueldos Basicos Cargados Para Esta Fecha"
            //}

            //sueldo = TruncateFunction(sueldo, 2);
            #endregion

            decimal sueldo = empl.obtenerSueldo2(IdEmpleadoEmpresa, IdDeclaracionJurada, true);

            return ((sueldo - 0.05M) <= sueldoIngresado) ? true : false;
        }

        public bool comprobarSueldoBase(int IdEmpleadoEmpresa, int IdDeclaracionJurada, decimal sueldoIngresado)
        {
            #region Old
            ////var declaracionJurada = db.DeclaracionJurada.Find(IdDeclaracionJurada);
            //var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == IdEmpleadoEmpresa).FirstOrDefault();
            //decimal sueldo = 1;

            ///*
            // * Establece el sueldo minimo basandose en la categoria y jornada del empleado
            // */
            //DateTime fechaSueldo = new DateTime(anio, mes, DateTime.Today.Day);
            //var sueldosBasicos = db.SueldoBasico.Where(x => x.IdCategoria == empleadoEmpresa.IdCategoria &&
            //                                                x.Desde <= fechaSueldo &&
            //                                                x.Hasta >= fechaSueldo).ToList();
            //if (sueldosBasicos != null && sueldosBasicos.Count > 0)
            //{
            //    foreach (var sueldos in sueldosBasicos)
            //    {
            //        if (sueldos != null && sueldos.Desde.Year == anio)
            //        {
            //            if (sueldos.Hasta.Year == anio)
            //            {
            //                if (sueldos.Desde.Month <= mes && sueldos.Hasta.Month >= mes)
            //                {
            //                    sueldo = sueldos.Monto;
            //                }
            //            }
            //            if (sueldos.Hasta.Year > anio)
            //            {
            //                sueldo = sueldos.Monto;
            //            }
            //        }
            //    }

            //    /*
            //     * Sueldo basico mas antiguedad
            //     */
            //    decimal antiguedad = MonthDifference(new DateTime(anio, mes, 1), empleadoEmpresa.FechaAlta);
            //    antiguedad = antiguedad / 12;
            //    sueldo += ((sueldo / 100) * Math.Truncate(antiguedad));

            //    /*
            //     * Sueldo basico mas 
            //     */
            //    sueldo += ((sueldo / 100) * (decimal)8.33);

            //    /*
            //     * Verifica si el empleado esta en licencia en el momento de la declaracion jurada, si esta de licencia, el sistema
            //     * le permite ingresar un sueldo por debajo del minimo
            //     */
            //    bool enLicencia = false;

            //    foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa))
            //    {
            //        if (licencia != null)
            //        {
            //            if (licencia.IdLicenciaLaboral != 3)
            //            {
            //                if (licencia.FechaBajaLicencia.Value.Year == anio)
            //                {
            //                    if (licencia.FechaAltaLicencia.Month <= mes && licencia.FechaBajaLicencia.Value.Month >= mes)
            //                    {
            //                        enLicencia = true;
            //                        sueldo = 0;
            //                    }
            //                }
            //                if (licencia.FechaBajaLicencia.Value.Year > anio)
            //                {
            //                    if (licencia.FechaAltaLicencia.Year <= anio && licencia.FechaAltaLicencia.Month <= mes)
            //                    {
            //                        enLicencia = true;
            //                        sueldo = 0;
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if ((licencia.FechaBajaLicencia.Value.Year == anio &&
            //                    licencia.FechaBajaLicencia.Value.Month == mes) ||
            //                    licencia.FechaBajaLicencia.Value.Year == anio &&
            //                    licencia.FechaBajaLicencia.Value.Month == mes - 1)
            //                {
            //                    enLicencia = true;
            //                }
            //                else if (licencia.FechaBajaLicencia.Value.Year == (anio - 1) &&
            //                    licencia.FechaBajaLicencia.Value.Month == 12 && mes == 1)
            //                {
            //                    enLicencia = true;
            //                }
            //            }
            //        }

            //        if (enLicencia)
            //        {
            //            switch (licencia.IdLicenciaLaboral)
            //            {
            //                case 1://Enfermedad
            //                    if ((licencia.FechaBajaLicencia - licencia.FechaAltaLicencia) > new TimeSpan(365, 0, 0, 0))
            //                    {
            //                        sueldo = 1;
            //                        DateTime fecha = new DateTime(anio, mes, 1);
            //                        if (MonthDifference(fecha, licencia.FechaAltaLicencia) >= 12)
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    break;
            //                case 2://Maternidad
            //                    if (licencia.FechaAltaLicencia.Month == mes)
            //                    {
            //                        //Si el/la empleado/a entro en licencia por maternidad habiendo trabajado
            //                        //almenos un dia el sistema solicita ingresar un sueldo mayor a 1
            //                        if (licencia.FechaAltaLicencia.Day > 1)
            //                        {
            //                            sueldo = 1;
            //                        }
            //                    }
            //                    if (licencia.FechaBajaLicencia.Value.Month == mes)
            //                    {
            //                        //Si el/la empleado/a volvio de licencia por maternidad antes de terminar el mes
            //                        //el sistema solicita ingresar un sueldo mayor a 1
            //                        if (licencia.FechaAltaLicencia.Day < 31)
            //                        {
            //                            sueldo = 1;
            //                        }
            //                    }
            //                    break;
            //                case 3://Vacaciones
            //                       //Si el/la empleado/a estuvo de vacaciones el mes anterior a la declaracion jurada
            //                       //el sistema permite ingresar un sueldo menor que el minimo
            //                    if (licencia.FechaBajaLicencia.Value.Year == anio)
            //                    {
            //                        if ((licencia.FechaBajaLicencia.Value.Month == (mes)) ||
            //                            (licencia.FechaBajaLicencia.Value.Month == (mes - 1)))
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    if (licencia.FechaBajaLicencia.Value.Year == (anio - 1) &&
            //                        licencia.FechaBajaLicencia.Value.Month == 12 && mes == 1)
            //                    {
            //                        sueldo = 0;
            //                    }
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }

            //    }
            //}
            //else
            //{
            //    //"No Hay Sueldos Basicos Cargados Para Esta Fecha"
            //}
            #endregion

            decimal sueldo = empl.obtenerSueldo5(IdEmpleadoEmpresa, IdDeclaracionJurada, true);

            sueldo = TruncateFunction(sueldo, 2);

            return ((sueldo - 0.05M) <= sueldoIngresado) ? true : false;
        }

        public static decimal MonthDifference(DateTime FechaFin, DateTime FechaInicio)
        {
            return Math.Abs((FechaFin.Month - FechaInicio.Month) + 12 * (FechaFin.Year - FechaInicio.Year));
        }


        [AllowAnonymous]
        public ActionResult ImpresionDDJJPorEmpresas(int id)
        {
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
                    totalSueldosBase += detalle.SueldoBase;
                    detalle.EsAfiliado = true;
                }
                //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa).FirstOrDefault();
                //if (afiliado != null)
                //{
                //    if (afiliado.FechaAlta.Year < declaracionJurada.anio)
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

        //[AllowAnonymous]
        //public ActionResult ImpresionDDJJPorEmpresas(int id)
        //{
        //    var declaracionJurada = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == id).FirstOrDefault();

        //    var empresa = db.Empresa.Where(x => x.IdEmpresa == declaracionJurada.idEmpresa).FirstOrDefault();
        //    ViewBag.RazonSocial = empresa.RazonSocial;
        //    ViewBag.Nro = empresa.Altura;
        //    ViewBag.Cuit = empresa.Cuit;
        //    ViewBag.Calle = empresa.Calle;
        //    ViewBag.LocalidadEmpresa = empresa.Localidad.Nombre;
        //    ViewBag.ProvinciaEmpresa = empresa.Localidad.Provincia.Nombre;


        //    decimal totalSueldos = 0, totalAportes = 0, totalSueldosBase = 0;
        //    var detalleDeclaracionJurada = (db.DetalleDeclaracionJurada.Include(d => d.Categoria)
        //                                                               .Include(d => d.DeclaracionJurada)
        //                                                               .Include(d => d.EmpleadoEmpresa)
        //                                                               .Include(d => d.Jornada)
        //                                                               .Where(x => x.IdDeclaracionJurada == id && x.EmpleadoEmpresa.FechaBaja == null));
        //    ViewBag.IdDeclaracionJurada = id;

        //    foreach (DetalleDeclaracionJurada detalle in detalleDeclaracionJurada)
        //    {
        //        totalSueldos += Math.Round(detalle.Sueldo, 2);
        //        var afiliado = db.Afiliado.Where(x => x.IdEmpleado == detalle.EmpleadoEmpresa.idEmpleado).FirstOrDefault();
        //        if (afiliado != null)
        //        {
        //            if (afiliado.FechaAlta.Year < declaracionJurada.anio)
        //            {
        //                if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
        //                {
        //                    if(detalle.idJornadaLaboral == 1 || detalle.idJornadaLaboral == 2)
        //                    {
        //                        totalAportes += Math.Round(((detalle.SueldoBase / 100) * 5), 2);
        //                    }
        //                    else
        //                    {
        //                        totalAportes += Math.Round(((detalle.Sueldo / 100) * 5), 2);
        //                    }
        //                    totalSueldosBase += detalle.SueldoBase;
        //                    detalle.EsAfiliado = true;
        //                }
        //            }
        //            else if (afiliado.FechaAlta.Year == declaracionJurada.anio && afiliado.FechaAlta.Month <= declaracionJurada.mes)
        //            {
        //                if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
        //                {
        //                    if (detalle.idJornadaLaboral == 1 || detalle.idJornadaLaboral == 2)
        //                    {
        //                        totalAportes += Math.Round(((detalle.SueldoBase / 100) * 5), 2);
        //                    }
        //                    else
        //                    {
        //                        totalAportes += Math.Round(((detalle.Sueldo / 100) * 5), 2);
        //                    }
        //                    totalSueldosBase += detalle.SueldoBase;
        //                    detalle.EsAfiliado = true;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            totalAportes += Math.Round(((detalle.Sueldo / 100) * 2), 2);
        //            detalle.EsAfiliado = false;
        //        }
        //        detalle.LicenciaEmpleado = false;
        //        foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == detalle.IdEmpleadoEmpresa))
        //        {
        //            if (licencia != null && licencia.FechaAltaLicencia.Year == detalle.DeclaracionJurada.anio)
        //            {
        //                if (licencia.FechaBajaLicencia.Value.Year == detalle.DeclaracionJurada.anio)
        //                {
        //                    if (licencia.FechaAltaLicencia.Month <= detalle.DeclaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
        //                    {
        //                        detalle.LicenciaEmpleado = true;
        //                    }
        //                }
        //                if (licencia.FechaBajaLicencia.Value.Year > detalle.DeclaracionJurada.anio)
        //                {
        //                    if (licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
        //                    {
        //                        detalle.LicenciaEmpleado = true;
        //                    }
        //                }
        //            }
        //            if (licencia.FechaBajaLicencia.Value.Year >= detalle.DeclaracionJurada.anio)
        //            {
        //                if (licencia.FechaBajaLicencia.Value.Month >= detalle.DeclaracionJurada.mes)
        //                {
        //                    detalle.LicenciaEmpleado = true;
        //                }
        //            }
        //        }
        //        var LiquidacionProporcional = db.LiquidacionProporcionalEmpleado.Where(x => x.IdDetalleDeclaracionJurada == detalle.IdDetalleDeclaracionJurada).FirstOrDefault();
        //        detalle.LiquidacionProporcional = (LiquidacionProporcional != null) ? true : false;
        //        detalle.IdLiquidacionProporcional = detalle.IdLiquidacionProporcional;
        //    }
        //    ViewBag.TotalSueldos = totalSueldos;
        //    ViewBag.TotalSueldosBase = totalSueldosBase;
        //    ViewBag.TotalAportes = totalAportes;

        //    return View(detalleDeclaracionJurada.ToList());
        //}

        //[AllowAnonymous]
        //public ActionResult ImprimirDDJJPorEmpresas(int id)
        //{
        //    return new ActionAsPdf("ImpresionDDJJPorEmpresas", new { id })
        //    {
        //        FileName = "Declaracion-Jurada.pdf",
        //        PageSize = Rotativa.Options.Size.A4,
        //        PageOrientation = Rotativa.Options.Orientation.Portrait,
        //        MinimumFontSize = 12
        //    };
        //}


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
