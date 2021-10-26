using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using DAL;
using DAL.Models;
using DAL.ViewModels;
using PagedList;
using SecSisBoletas.Models.Utilities;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class EmpleadosController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Empleados
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var empleados = from oEmpleados in db.Empleado
                            join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                            join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                            where oEmpleadoEmpresa.idEmpresa == IdEmpresa && oEmpleadoEmpresa.FechaBaja == null
                            select oEmpleados;

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(x => x.Apellido.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    empleados = empleados.OrderByDescending(x => x.Apellido);
                    break;
                default:
                    empleados = empleados.OrderBy(x => x.Apellido);
                    break;
            }

            foreach (var emp in empleados)
            {
                var empEmp = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa &&
                                                                          x.idEmpleado == emp.IdEmpleado &&
                                                                          x.FechaBaja == null).FirstOrDefault();
                if(empEmp != null)
                {

                    emp.Jornada = empEmp.Jornada.Descripcion;
                    emp.Categoria = empEmp.Categoria.Descripcion;
                    emp.EsAfiliado = empEmp.EsAfiliado;
                    emp.FechaAlta = empEmp.FechaAlta;
                }
            }

            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(empleados.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult IndexBajas(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CurrentSort = sortOrder;//Para mantener el orden(asc o desc)

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var empleados = from oEmpleados in db.Empleado
                            join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                            join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                            where oEmpleadoEmpresa.idEmpresa == IdEmpresa && oEmpleadoEmpresa.FechaBaja != null
                            select oEmpleados;

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(x => x.Apellido.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    empleados = empleados.OrderByDescending(x => x.Apellido);
                    break;
                default:
                    empleados = empleados.OrderBy(x => x.Apellido);
                    break;
            }

            foreach (var emp in empleados)
            {
                var empEmp = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa &&
                                                                          x.idEmpleado == emp.IdEmpleado &&
                                                                          x.FechaBaja != null).FirstOrDefault();
                if (empEmp != null)
                {

                    emp.Jornada = empEmp.Jornada.Descripcion;
                    emp.Categoria = empEmp.Categoria.Descripcion;
                    emp.EsAfiliado = empEmp.EsAfiliado;
                    emp.FechaAlta = empEmp.FechaAlta;
                }
            }

            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(empleados.ToPagedList(pageNumber, pageSize));
        }

        public JsonResult GetEmpleadoJornadaCategoria(int? id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var empleadoEmpresa = (from oEmpleadoEmpresa in db.EmpleadoEmpresa
                                  join oCategoria in db.Categoria on oEmpleadoEmpresa.IdCategoria equals oCategoria.IdCategoria
                                  join oJornada in db.Jornada on oEmpleadoEmpresa.IdJornada equals oJornada.IdJornada
                                  where oEmpleadoEmpresa.idEmpleadoEmpresa == id
                                  select new
                                  {
                                      IdCategoria = oEmpleadoEmpresa.IdCategoria,
                                      NombreCategoria = oEmpleadoEmpresa.Categoria.Descripcion,
                                      IdJornada = oEmpleadoEmpresa.IdJornada,
                                      NombreJornada = oEmpleadoEmpresa.Jornada.Descripcion
                                  });

            if(empleadoEmpresa.FirstOrDefault() != null)
            {
                return Json(empleadoEmpresa, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(empleadoEmpresa.FirstOrDefault(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CheckAfiliado(int IdEmpleadoEmpresa, int IdDeclaracionJurada)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var declaracionJurada = db.DeclaracionJurada.Find(IdDeclaracionJurada);
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == IdEmpleadoEmpresa).FirstOrDefault();

            empleadoEmpresa.EsAfiliado = false;
            var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa).FirstOrDefault();
            if (afiliado != null)
            {
                if (afiliado.FechaAlta.Year < declaracionJurada.anio)
                {
                    if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
                    {
                        empleadoEmpresa.EsAfiliado = true;
                    }
                }
                else if (afiliado.FechaAlta.Year == declaracionJurada.anio && afiliado.FechaAlta.Month <= declaracionJurada.mes)
                {
                    if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
                    {
                        empleadoEmpresa.EsAfiliado = true;
                    }
                }
            }

            if (empleadoEmpresa != null)
            {
                return Json(empleadoEmpresa.EsAfiliado, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(empleadoEmpresa.EsAfiliado, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetSueldoBasico(int IdEmpleadoEmpresa, int IdDeclaracionJurada)
        {
            #region Old
            //var declaracionJurada = db.DeclaracionJurada.Find(IdDeclaracionJurada);
            //var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == IdEmpleadoEmpresa).FirstOrDefault();
            //int mesAnterior = 0;
            //int anioAnterior = 0;

            //if(declaracionJurada.mes == 1)
            //{
            //    mesAnterior = 11;//12;
            //    anioAnterior = declaracionJurada.anio - 1;
            //}
            //else if(declaracionJurada.mes == 7)
            //{
            //    mesAnterior = declaracionJurada.mes - 2;
            //    anioAnterior = declaracionJurada.anio;
            //}
            //else
            //{
            //    mesAnterior = declaracionJurada.mes - 1;
            //    anioAnterior = declaracionJurada.anio;
            //}

            //var detalleDeclaracionAnterior = db.DetalleDeclaracionJurada.Include(e => e.DeclaracionJurada)
            //                                                            .Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa &&
            //                                                                        x.DeclaracionJurada.mes == mesAnterior &&
            //                                                                        x.DeclaracionJurada.anio == anioAnterior).FirstOrDefault();

            //decimal sueldo = 1, sueldoAnterior = 1;

            //if (detalleDeclaracionAnterior != null)
            //{
            //    sueldoAnterior = detalleDeclaracionAnterior.Sueldo;
            //    //return Json(sueldo, JsonRequestBehavior.AllowGet);
            //}

            ///*
            // * Establece el sueldo minimo basandose en la categoria y jornada del empleado
            // */
            //foreach (var sueldos in db.SueldoBasico.Where(x => x.IdCategoria == empleadoEmpresa.IdCategoria))
            //{
            //    if(sueldos == null)
            //    {
            //        sueldo = -1;
            //        return Json(sueldo, JsonRequestBehavior.AllowGet);
            //    }
            //    if (sueldos != null && sueldos.Desde.Year == declaracionJurada.anio)
            //    {
            //        if (sueldos.Hasta.Year == declaracionJurada.anio)
            //        {
            //            if (sueldos.Desde.Month <= declaracionJurada.mes && sueldos.Hasta.Month >= declaracionJurada.mes)
            //            {
            //                if (empleadoEmpresa.IdJornada == 2)
            //                {
            //                    sueldo = (sueldos.Monto / 2);
            //                }
            //                else if (empleadoEmpresa.IdJornada == 3)
            //                {
            //                    sueldo = sueldos.Monto;
            //                }
            //            }
            //        }
            //        if (sueldos.Hasta.Year > declaracionJurada.anio)
            //        {
            //            if (empleadoEmpresa.IdJornada == 2)
            //            {
            //                sueldo = (sueldos.Monto / 2);
            //            }
            //            else if (empleadoEmpresa.IdJornada == 3)
            //            {
            //                sueldo = sueldos.Monto;
            //            }
            //        }
            //    }
            //}

            ///*
            // * Sueldo basico mas antiguedad
            // */
            //decimal antiguedad = MonthDifference(new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1), empleadoEmpresa.FechaAlta);
            //antiguedad = antiguedad / 12;
            //sueldo += ((sueldo / 100) * Math.Truncate(antiguedad));

            ///*
            // * Sueldo basico mas presentismo
            // */
            //sueldo += ((sueldo / 100) * (decimal)8.33);

            ///*
            // * Verifica si el empleado esta en licencia en el momento de la declaracion jurada, si esta de licencia, el sistema
            // * le permite ingresar un sueldo por debajo del minimo
            // */
            //bool enLicencia = false;
            //foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa))
            //{
            //    if (licencia != null && licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
            //    {
            //        if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
            //        {
            //            if (licencia.FechaAltaLicencia.Month <= declaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
            //            {
            //                enLicencia = true;
            //            }
            //        }
            //        if (licencia.FechaBajaLicencia.Value.Year > declaracionJurada.anio)
            //        {
            //            if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
            //            {
            //                enLicencia = true;
            //            }
            //        }
            //    }
            //    if (licencia.FechaBajaLicencia.Value.Year >= declaracionJurada.anio)
            //    {
            //        if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
            //        {
            //            enLicencia = true;
            //        }
            //    }
            //    if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1))
            //    {
            //        if (licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
            //        {
            //            enLicencia = true;
            //        }
            //    }
            //    if (enLicencia)
            //    {
            //        switch (licencia.IdLicenciaLaboral)
            //        {
            //            case 1://Enfermedad
            //                if ((licencia.FechaBajaLicencia - licencia.FechaAltaLicencia) > new TimeSpan(365, 0, 0, 0))
            //                {
            //                    sueldo = 1;
            //                    DateTime fecha = new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1);
            //                    if (MonthDifference(fecha, licencia.FechaAltaLicencia) >= 12)
            //                    {
            //                        sueldo = 0;
            //                    }
            //                }
            //                break;
            //            case 2://Maternidad
            //                if (licencia.FechaAltaLicencia.Month == declaracionJurada.mes)
            //                {
            //                    //Si el/la empleado/a entro en licencia por maternidad habiendo trabajado
            //                    //almenos un dia el sistema solicita ingresar un sueldo mayor a 1
            //                    if (licencia.FechaAltaLicencia.Day > 1)
            //                    {
            //                        sueldo = 1;
            //                    }
            //                    //Si el/la empleado/a entro en licencia por maternidad el dia 1
            //                    //el sistema le permite ingresar 0
            //                    else
            //                    {
            //                        sueldo = 0;
            //                    }
            //                }
            //                if (licencia.FechaBajaLicencia.Value.Month == declaracionJurada.mes)
            //                {
            //                    //Si el/la empleado/a volvio de licencia por maternidad antes de terminar el mes
            //                    //el sistema solicita ingresar un sueldo mayor a 1
            //                    if (licencia.FechaAltaLicencia.Day < 31)
            //                    {
            //                        sueldo = 1;
            //                    }
            //                    else
            //                    {
            //                        sueldo = 0;
            //                    }
            //                }
            //                break;
            //            case 3://Vacaciones
            //                //Si el/la empleado/a estuvo de vacaciones el mes anterior a la declaracion jurada
            //                //el sistema permite ingresar un sueldo menor que el minimo
            //                if (licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
            //                {
            //                    if (declaracionJurada.mes == 1 &&
            //                        licencia.FechaAltaLicencia.Month == (declaracionJurada.mes - 1))
            //                    {
            //                        sueldo = 0;
            //                    }
            //                }
            //                if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1) &&
            //                    licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
            //                {
            //                    sueldo = 0;
            //                }
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //}

            //if(sueldoAnterior > sueldo)
            //{
            //    sueldo = sueldoAnterior;
            //}
            #endregion
            db.Configuration.ProxyCreationEnabled = false;

            decimal sueldo = obtenerSueldo2(IdEmpleadoEmpresa,IdDeclaracionJurada);

            return Json(TruncateFunction(sueldo, 2), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSueldoBase(int IdEmpleadoEmpresa, int IdDeclaracionJurada)
        {
            #region Old
            //var declaracionJurada = db.DeclaracionJurada.Find(IdDeclaracionJurada);
            //var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == IdEmpleadoEmpresa).FirstOrDefault();
            //int mesAnterior = 0;
            //int anioAnterior = 0;

            ///*
            // * Sueldo 5% Anterior
            // */
            //if(declaracionJurada.mes == 1)
            //{
            //    mesAnterior = 11;//12;
            //    anioAnterior = declaracionJurada.anio - 1;
            //}
            //else if(declaracionJurada.mes == 7)
            //{
            //    mesAnterior = declaracionJurada.mes - 2;
            //    anioAnterior = declaracionJurada.anio;
            //}
            //else
            //{
            //    mesAnterior = declaracionJurada.mes - 1;
            //    anioAnterior = declaracionJurada.anio;
            //}

            //var detalleDeclaracionAnterior = db.DetalleDeclaracionJurada.Include(e => e.DeclaracionJurada)
            //                                                            .Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa &&
            //                                                                        x.DeclaracionJurada.mes == mesAnterior &&
            //                                                                        x.DeclaracionJurada.anio == anioAnterior).FirstOrDefault();
            //empleadoEmpresa.EsAfiliado = false;
            //var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa).FirstOrDefault();
            //if (afiliado != null)
            //{
            //    if (afiliado.FechaAlta.Year < declaracionJurada.anio)
            //    {
            //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
            //        {
            //            empleadoEmpresa.EsAfiliado = true;
            //        }
            //    }
            //    else if (afiliado.FechaAlta.Year == declaracionJurada.anio && afiliado.FechaAlta.Month <= declaracionJurada.mes)
            //    {
            //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
            //        {
            //            empleadoEmpresa.EsAfiliado = true;
            //        }
            //    }
            //}

            //decimal sueldo = 0, sueldoAnterior = 0;

            //if(empleadoEmpresa.EsAfiliado)
            //{
            //    if(detalleDeclaracionAnterior != null)
            //    {
            //        sueldoAnterior = detalleDeclaracionAnterior.SueldoBase;
            //    }


            //    /*
            //     * Establece el sueldo minimo basandose en la categoria y jornada del empleado
            //     */
            //    foreach (var sueldos in db.SueldoBasico.Where(x => x.IdCategoria == empleadoEmpresa.IdCategoria))
            //    {
            //        if (sueldos == null)
            //        {
            //            sueldo = -1;
            //            return Json(sueldo, JsonRequestBehavior.AllowGet);
            //        }
            //        if (sueldos != null && sueldos.Desde.Year == declaracionJurada.anio)
            //        {
            //            if (sueldos.Hasta.Year == declaracionJurada.anio)
            //            {
            //                if (sueldos.Desde.Month <= declaracionJurada.mes && sueldos.Hasta.Month >= declaracionJurada.mes)
            //                {
            //                    //if (empleadoEmpresa.IdJornada == 2)
            //                    //{
            //                    //    sueldo = (sueldos.Monto / 2);
            //                    //}
            //                    //else if (empleadoEmpresa.IdJornada == 3)
            //                    //{
            //                        sueldo = sueldos.Monto;
            //                    //}
            //                }
            //            }
            //            if (sueldos.Hasta.Year > declaracionJurada.anio)
            //            {
            //                //if (empleadoEmpresa.IdJornada == 2)
            //                //{
            //                //    sueldo = (sueldos.Monto / 2);
            //                //}
            //                //else if (empleadoEmpresa.IdJornada == 3)
            //                //{
            //                    sueldo = sueldos.Monto;
            //                //}
            //            }
            //        }
            //    }

            //    /*
            //     * Sueldo basico mas antiguedad
            //     */
            //    int antiguedad = Convert.ToInt32(MonthDifference(new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1), empleadoEmpresa.FechaAlta));
            //    antiguedad = antiguedad / 12;
            //    sueldo += ((sueldo / 100) * antiguedad);

            //    /*
            //     * Sueldo basico mas Presentismo
            //     */
            //    sueldo += ((sueldo / 100) * (decimal)8.33);

            //    /*
            //     * Verifica si el empleado esta en licencia en el momento de la declaracion jurada, si esta de licencia, el sistema
            //     * le permite ingresar un sueldo por debajo del minimo
            //     */
            //    bool enLicencia = false;
            //    foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa))
            //    {
            //        if (licencia != null && licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
            //        {
            //            if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
            //            {
            //                if (licencia.FechaAltaLicencia.Month <= declaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
            //                {
            //                    enLicencia = true;
            //                }
            //            }
            //            if (licencia.FechaBajaLicencia.Value.Year > declaracionJurada.anio)
            //            {
            //                if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
            //                {
            //                    enLicencia = true;
            //                }
            //            }
            //        }
            //        if (licencia.FechaBajaLicencia.Value.Year >= declaracionJurada.anio)
            //        {
            //            if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
            //            {
            //                enLicencia = true;
            //            }
            //        }
            //        if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1))
            //        {
            //            if (licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
            //            {
            //                enLicencia = true;
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
            //                        DateTime fecha = new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1);
            //                        if (MonthDifference(fecha, licencia.FechaAltaLicencia) >= 12)
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    break;
            //                case 2://Maternidad
            //                    if (licencia.FechaAltaLicencia.Month == declaracionJurada.mes)
            //                    {
            //                        //Si el/la empleado/a entro en licencia por maternidad habiendo trabajado
            //                        //almenos un dia el sistema solicita ingresar un sueldo mayor a 1
            //                        if (licencia.FechaAltaLicencia.Day > 1)
            //                        {
            //                            sueldo = 1;
            //                        }
            //                        //Si el/la empleado/a entro en licencia por maternidad el dia 1
            //                        //el sistema le permite ingresar 0
            //                        else
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    if (licencia.FechaBajaLicencia.Value.Month == declaracionJurada.mes)
            //                    {
            //                        //Si el/la empleado/a volvio de licencia por maternidad antes de terminar el mes
            //                        //el sistema solicita ingresar un sueldo mayor a 1
            //                        if (licencia.FechaAltaLicencia.Day < 31)
            //                        {
            //                            sueldo = 1;
            //                        }
            //                        else
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    break;
            //                case 3://Vacaciones
            //                    //Si el/la empleado/a estuvo de vacaciones el mes anterior a la declaracion jurada
            //                    //el sistema permite ingresar un sueldo menor que el minimo
            //                    if (licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
            //                    {
            //                        if (declaracionJurada.mes == 1 &&
            //                            licencia.FechaAltaLicencia.Month == (declaracionJurada.mes - 1))
            //                        {
            //                            sueldo = 0;
            //                        }
            //                    }
            //                    if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1) &&
            //                        licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
            //                    {
            //                        sueldo = 0;
            //                    }
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }

            //    if (sueldoAnterior > sueldo)
            //    {
            //        sueldo = sueldoAnterior;
            //    }
            //}
            #endregion
            db.Configuration.ProxyCreationEnabled = false;

            decimal sueldo = obtenerSueldo5(IdEmpleadoEmpresa, IdDeclaracionJurada);

            return Json(TruncateFunction(sueldo, 2), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            int errorRenglon = 1;
            List<MessageVm> errores = new List<MessageVm>();

            try
            {
                //solo entra si hay un documento y este no esta vacio
                if (file != null && file.ContentLength > 0)
                {
                    bool error = false; //seteo una variable de error en false
                    List<string> rows = new List<string>(); //lista de string que va a contener las x filas que tenga el documento
                    List<Empleado> empleados = new List<Empleado>(); //lista de Empleado que va a contener los empleados en el documento
                    StreamReader fileContent = new StreamReader(file.InputStream, System.Text.Encoding.Default); //lleno la lista de string con cada renglon que tenga el documento
                    do
                    {
                        rows.Add(fileContent.ReadLine());
                    }
                    while (!fileContent.EndOfStream); //continua en loop hasta que el final del documento
                    foreach (var row in rows) //recorro la lista de string
                    {
                        string[] emp = row.Split(';'); //divido cada elemento string de la lista por medio de ; para obtener lo que serian los datos de cada empleado
                        if(emp.Length == 9) //solo entra si en el renglon hay exacta cantidad de datos
                        {
                            Empleado empleado = new Empleado(); //creo un objetp Empleado en blanco y le asigno toda la informacion
                            empleado.Nombre = emp[0].Trim();
                            empleado.Apellido = emp[1].Trim();
                            if (emp[2].Length > 11 || emp[2].Length < 9)
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Cuil No Valido.",
                                    Dismissible = true
                                });
                                error = true;  break;
                            }
                            empleado.Cuil = emp[2].Trim();
                            empleado.Calle = emp[3].Trim();
                            if (int.TryParse(emp[4].Trim(), out int altura)) { empleado.Altura = int.Parse(emp[4].Trim()); }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Numero de calle No Valido.",
                                    Dismissible = true
                                });
                                error = true; break;
                            }
                            if(db.Localidad.Find(int.Parse(emp[5].Trim())) != null) { empleado.IdLocalidad = int.Parse(emp[5].Trim()); }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Id de Localidad No Valido.",
                                    Dismissible = true
                                });
                                error = true; break;
                            }
                            if (db.Categoria.Find(int.Parse(emp[6].Trim())) != null) { empleado.IdCategoria = int.Parse(emp[6].Trim()); }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Id de Categoria No Valido.",
                                    Dismissible = true
                                });
                                error = true; break;
                            }
                            if (db.Jornada.Find(int.Parse(emp[7].Trim())) != null) { empleado.IdJornada = int.Parse(emp[7].Trim()); }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Id de Jornada No Valido.",
                                    Dismissible = true
                                });
                                error = true; break;
                            }
                            if(DateTime.TryParse(emp[8].Trim(), out DateTime fechaAlta)) { empleado.FechaAlta = DateTime.Parse(emp[8].Trim()); }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Fecha de Alta No Valida.",
                                    Dismissible = true
                                });
                                error = true; break;
                            }
                            //si no hay errores se agrega el empleado a la lista de empleados
                            empleados.Add(empleado);
                        }
                        else
                        {
                            error = true;
                            break;
                        }
                        errorRenglon++;
                    }
                    if(error == false) //solo entra si no hay errores
                    {
                        foreach (Empleado empleado in empleados)
                        {
                            EmpleadoEmpresa empleadoEmpresa = new EmpleadoEmpresa(); //creo un objeto EmpleadoEmpresa donde guardo relacion entre el empleado y la empresa
                            Empleado emp = db.Empleado.AsNoTracking().Where(x => x.Cuil == empleado.Cuil).FirstOrDefault(); //busco si hay un empleado con el cuil dado
                            if (emp == null)//si no existe el empleado, se guarda
                            {
                                db.Empleado.Add(empleado);
                                db.SaveChanges();
                            }
                            else//si existe no lo guardo y obtengo su id
                            {
                                empleado.IdEmpleado = emp.IdEmpleado;
                            }
                            List<EmpleadoEmpresa> empEmp = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpleado == empleado.IdEmpleado).ToList(); //busco alguna relacion del empleado con alguna empresa

                            if (empEmp.Count() < 1) //si no existe, significa que no trabaja para ninguna empresa, asi que creo la relacion del empleado con esta empresa
                            {
                                empleadoEmpresa.idEmpleado = empleado.IdEmpleado;
                                empleadoEmpresa.idEmpresa = IdEmpresa;
                                empleadoEmpresa.IdCategoria = empleado.IdCategoria;
                                empleadoEmpresa.IdJornada = empleado.IdJornada;
                                empleadoEmpresa.EsAfiliado = empleado.EsAfiliado;
                                empleadoEmpresa.FechaAlta = empleado.FechaAlta;
                                db.EmpleadoEmpresa.Add(empleadoEmpresa);
                                db.SaveChanges();

                                if (empleadoEmpresa.IdJornada == 2) //si es a empleado a media jornada se le asigna un turno(Default)
                                {
                                    TurnoEmpleado turnoEmpleado = new TurnoEmpleado();
                                    turnoEmpleado.IdEmpleadoEmpresa = empleadoEmpresa.idEmpleadoEmpresa;
                                    turnoEmpleado.Turno = "Mañana";
                                    db.TurnoEmpleado.Add(turnoEmpleado);
                                    db.SaveChanges();
                                }
                            }
                            else //si existe relacion con alguna empresa
                            {
                                //Solo entra si esta relacionado con esta empresa
                                if (db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa && x.idEmpleado == empleado.IdEmpleado).FirstOrDefault() != null)
                                {
                                    //si esta relacionado y aun trabajando con esta empresa doy mensaje de error y continuo con el siguinte empleado en la lista
                                    if (db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpresa == IdEmpresa && x.idEmpleado == empleado.IdEmpleado && x.FechaBaja == null).FirstOrDefault() != null)
                                    {
                                        errores.Add(new MessageVm()
                                        {
                                            Type = "alert-danger",
                                            Message = "Error con empleado con cuil: " + empleado.Cuil + ", El Empleado ya se encuentra relacionado a esta empresa.",
                                            Dismissible = true
                                        });
                                        //ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a esta empresa");
                                        continue;
                                    }
                                    // entra si esta relacionado y pero no esta trabajando con esta empresa
                                    else
                                    {
                                        var empleadoDespedido = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.idEmpleado == empleado.IdEmpleado && x.FechaBaja != null).FirstOrDefault();
                                        if (empleadoDespedido != null)
                                        {
                                            //si el empleado se encuentra trabajando en una empresa a tiempo completo, doy mensaje de error
                                            if (db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpleado == empleado.IdEmpleado && x.IdJornada == 3 && x.FechaBaja == null).ToList().Count() >= 1) 
                                            {
                                                errores.Add(new MessageVm()
                                                {
                                                    Type = "alert-danger",
                                                    Message = "Error con empleado con cuil: " + empleado.Cuil + ", El Empleado ya se encuentra relacionado a otra empresa.",
                                                    Dismissible = true
                                                });
                                                continue;
                                            }
                                            else
                                            {
                                                int limite = 0;
                                                if(empleado.IdJornada == 1 || empleado.IdJornada == 2)
                                                {
                                                    limite = 1;
                                                }
                                                int cant = db.EmpleadoEmpresa.AsNoTracking().Where(x => x.idEmpleado == empleado.IdEmpleado && (x.IdJornada == 2 || x.IdJornada == 1)).ToList().Count;
                                                if (cant <= limite)
                                                {
                                                    //recontrato al empleado
                                                    empleadoDespedido.idEmpleado = empleado.IdEmpleado;
                                                    empleadoDespedido.idEmpresa = IdEmpresa;
                                                    empleadoDespedido.IdCategoria = empleado.IdCategoria;
                                                    empleadoDespedido.IdJornada = empleado.IdJornada;
                                                    empleadoDespedido.EsAfiliado = empleado.EsAfiliado;
                                                    empleadoDespedido.FechaAlta = empleado.FechaAlta;
                                                    empleadoDespedido.FechaBaja = null;
                                                    db.SaveChanges();

                                                    if (empleadoEmpresa.IdJornada == 2)
                                                    {

                                                        TurnoEmpleado turnoEmpleado = db.TurnoEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoDespedido.idEmpleadoEmpresa).FirstOrDefault();
                                                        if (turnoEmpleado == null) turnoEmpleado = new TurnoEmpleado();
                                                        turnoEmpleado.IdEmpleadoEmpresa = empleadoEmpresa.idEmpleadoEmpresa;
                                                        turnoEmpleado.Turno = "Mañana";
                                                        if (turnoEmpleado.IdTurnoEmpleado == 0) db.TurnoEmpleado.Add(turnoEmpleado);
                                                        db.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    errores.Add(new MessageVm()
                                                    {
                                                        Type = "alert-danger",
                                                        Message = "Error con empleado con cuil: " + empleado.Cuil + ", El Empleado ya se encuentra relacionado a 2(Dos) Empresas, no es posible que inicie una relacion con otra empresa.",
                                                        Dismissible = true
                                                    });
                                                    continue;
                                                }
                                            }
                                        }

                                    }
                                }

                                //valido si el empleado esta trabajando para alguna empresa
                                empEmp = empEmp.Where(x => x.FechaBaja == null).ToList();
                                if (empEmp.Count() > 0) //entra solo si se encuentra en relacion y trabajando para una empresa
                                {
                                    if (empleado.IdJornada == 2 || empleado.IdJornada == 1) //Solo entra si el empleado tiene un trabaja de media jornada o jornada por horas
                                    {
                                        if (empEmp.Where(x => x.IdJornada == 3).ToList().Count() >= 1) //si el empleado se encuentra trabajando en una empresa a tiempo completo, doy mensaje de error
                                        {
                                            errores.Add(new MessageVm()
                                            {
                                                Type = "alert-danger",
                                                Message = "Error con empleado con cuil: " + empleado.Cuil + ", El Empleado ya se encuentra relacionado a otra empresa.",
                                                Dismissible = true
                                            });
                                            //ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a otra empresa");
                                        }
                                        else
                                        {
                                            empEmp = empEmp.Where(x => x.IdJornada == 2 || x.IdJornada == 1).ToList();
                                            if (empEmp.Count() < 2)
                                            {
                                                empleadoEmpresa.idEmpleado = empleado.IdEmpleado;
                                                empleadoEmpresa.idEmpresa = IdEmpresa;
                                                empleadoEmpresa.IdCategoria = empleado.IdCategoria;
                                                empleadoEmpresa.IdJornada = empleado.IdJornada;
                                                empleadoEmpresa.EsAfiliado = empleado.EsAfiliado;
                                                empleadoEmpresa.FechaAlta = empleado.FechaAlta;
                                                db.EmpleadoEmpresa.Add(empleadoEmpresa);
                                                db.SaveChanges();

                                                if (empleadoEmpresa.IdJornada == 2)
                                                {
                                                    TurnoEmpleado turnoEmpleado = new TurnoEmpleado();
                                                    turnoEmpleado.IdEmpleadoEmpresa = empleadoEmpresa.idEmpleadoEmpresa;
                                                    turnoEmpleado.Turno = "Mañana";
                                                    db.TurnoEmpleado.Add(turnoEmpleado);
                                                    db.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                                errores.Add(new MessageVm()
                                                {
                                                    Type = "alert-danger",
                                                    Message = "Error con empleado con cuil: " + empleado.Cuil + ", El Empleado ya se encuentra relacionado a 2(Dos) Empresas, no es posible que inicie una relacion con otra empresa.",
                                                    Dismissible = true
                                                });
                                                //ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a 2(Dos) Empresas, no es posible que inicie una relacion con otra empresa");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        errores.Add(new MessageVm()
                                        {
                                            Type = "alert-danger",
                                            Message = "Error con empleado con cuil: " + empleado.Cuil + ", El Empleado ya se encuentra relacionado a otra empresa.",
                                            Dismissible = true
                                        });
                                        //ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a otra empresa");
                                    }
                                }
                                else
                                {
                                    empleadoEmpresa.idEmpleado = empleado.IdEmpleado;
                                    empleadoEmpresa.idEmpresa = IdEmpresa;
                                    empleadoEmpresa.IdCategoria = empleado.IdCategoria;
                                    empleadoEmpresa.IdJornada = empleado.IdJornada;
                                    empleadoEmpresa.EsAfiliado = empleado.EsAfiliado;
                                    empleadoEmpresa.FechaAlta = empleado.FechaAlta;
                                    db.EmpleadoEmpresa.Add(empleadoEmpresa);
                                    db.SaveChanges();

                                    if (empleadoEmpresa.IdJornada == 2)
                                    {
                                        TurnoEmpleado turnoEmpleado = new TurnoEmpleado();
                                        turnoEmpleado.IdEmpleadoEmpresa = empleadoEmpresa.idEmpleadoEmpresa;
                                        turnoEmpleado.Turno = "Mañana";
                                        db.TurnoEmpleado.Add(turnoEmpleado);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        errores.Add(new MessageVm()
                        {
                            Type = "alert-success",
                            Message = "Empleados Importados Correctamente!!.",
                            Dismissible = true
                        });
                        //ViewBag.Message = "Empleados Importados Correctamente!!";
                    }
                    else
                    {
                        errores.Add(new MessageVm()
                        {
                            Type = "alert-danger",
                            Message = "Error al importar los Empleados!!.",
                            Dismissible = true
                        });
                        //ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon;
                    }
                }
                else
                {
                    errores.Add(new MessageVm()
                    {
                        Type = "alert-danger",
                        Message = "Error, Archivo vacio o no valido!!.",
                        Dismissible = true
                    });
                    ViewBag.Message = "Error, Archivo vacio o no valido!!";
                }
                ViewBag.ErroresImportacionEmpleados = errores;
                return View();
            }
            catch (Exception e)
            {
                errores.Add(new MessageVm()
                {
                    Type = "alert-danger",
                    Message = "Error al importar los Empleados!!!!.",
                    Dismissible = true
                });
                ViewBag.ErroresImportacionEmpleados = errores;
                //ViewBag.Message = "Error al importar los Empleados!!" + Environment.NewLine + "Error en el renglon Nro: " + errorRenglon;
                return View();
            }
        }

        // GET: Empleados/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            Empleado empleado = db.Empleado.Where(x => x.IdEmpleado == id).Include(t => t.Localidad).FirstOrDefault();

            if (empleado == null)
            {
                return HttpNotFound();
            }

            var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == id &&
                                           x.idEmpresa == IdEmpresa)
                               .Include(t => t.Jornada)
                               .Include(t => t.Categoria)
                               .FirstOrDefault();

            empleado.Jornada = empEmp.Jornada.Descripcion;
            empleado.Categoria = empEmp.Categoria.Descripcion;
            empleado.EsAfiliado = empEmp.EsAfiliado;
            empleado.FechaAlta = empEmp.FechaAlta;
            empleado.FechaBaja = (empEmp.FechaBaja != null) ? empEmp.FechaBaja.Value: DateTime.MinValue;

            //if (empEmp.IdJornada == 2)
            //{
            //    empleado.Turno = db.TurnoEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault().Turno;
            //}
            //else if (empEmp.IdJornada == 1)
            //{
            //    var horarios = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).ToList();
            //    foreach (var horario in horarios)
            //    {
            //        switch (horario.Dia)
            //        {
            //            case "Lunes":
            //                if(horario.Turno == "Manana")
            //                {
            //                    empleado.desdeLunesManana = horario.Entrada;
            //                    empleado.hastaLunesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeLunesTarde = horario.Entrada;
            //                    empleado.hastaLunesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Martes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeMartesManana = horario.Entrada;
            //                    empleado.hastaMartesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeMartesManana = horario.Entrada;
            //                    empleado.hastaMartesManana = horario.Salida;
            //                }
            //                break;
            //            case "Miercoles":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeMiercolesManana = horario.Entrada;
            //                    empleado.hastaMiercolesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeMiercolesTarde = horario.Entrada;
            //                    empleado.hastaMiercolesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Jueves":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeJuevesManana = horario.Entrada;
            //                    empleado.hastaJuevesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeJuevesTarde = horario.Entrada;
            //                    empleado.hastaJuevesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Viernes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeViernesManana = horario.Entrada;
            //                    empleado.hastaViernesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeViernesTarde = horario.Entrada;
            //                    empleado.hastaViernesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Sabado":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeSabadoManana = horario.Entrada;
            //                    empleado.hastaSabadoManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeSabadoTarde = horario.Entrada;
            //                    empleado.hastaSabadoTarde = horario.Salida;
            //                }
            //                break;
            //            case "Domingo":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeDomingoManana = horario.Entrada;
            //                    empleado.hastaDomingoManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeDomingoTarde = horario.Entrada;
            //                    empleado.hastaDomingoTarde = horario.Salida;
            //                }
            //                break;
            //        }
            //    }
            //}

            return View(empleado);
        }

        // GET: Empleados/Create
        public ActionResult Create()
        {
            var localidades = db.Localidad;
            foreach (var localidad in localidades)
            {
                localidad.Nombre = localidad.Nombre + " (" + localidad.CodPostal + ")";
            }
            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre");
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre",5);
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion");
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", 3);
            return View();
        }

        // POST: Empleados/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Nombre,Apellido,Dni,Cuil,Calle,Altura,IdLocalidad,FechaAlta,IdCategoria,IdJornada,EsAfiliado"/*,Turno,desdeLunesManana,desdeLunesTarde,hastaLunesManana,hastaLunesTarde,desdeMartesManana,desdeMartesTarde,hastaMartesManana,hastaMartesTarde,desdeMiercolesManana,desdeMiercolesTarde,hastaMiercolesManana,hastaMiercolesTarde,desdeJuevesManana,desdeJuevesTarde,hastaJuevesManana,hastaJuevesTarde,desdeViernesManana,desdeViernesTarde,hastaViernesManana,hastaViernesTarde,desdeSabadoManana,desdeSabadoTarde,hastaSabadoManana,hastaSabadoTarde,desdeDomingoManana,desdeDomingoTarde,hastaDomingoManana,hastaDomingoTarde"*/)] Empleado empleado)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", empleado.IdProvincia);
            var localidades = db.Localidad;
            foreach (var localidad in localidades)
            {
                localidad.NombreCodPostal = localidad.Nombre + " (" + localidad.CodPostal + ")";
            }
            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "NombreCodPostal", empleado.IdLocalidad);
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", empleado.IdCategoria);
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", empleado.IdJornada);

            if (ModelState.IsValid)
            {
                CUIT cuil = new CUIT(empleado.Cuil);
                if (cuil.EsValido)
                {
                    if (db.Empleado.Where(x => x.Cuil == empleado.Cuil).FirstOrDefault() == null)
                    {
                        db.Empleado.Add(empleado);
                        db.SaveChanges();
                    }
                    else
                    {
                        empleado.IdEmpleado = db.Empleado.Where(x => x.Cuil == empleado.Cuil).FirstOrDefault().IdEmpleado;
                    }
                    int IdEmpresa = Convert.ToInt32(claim.Value);
                    //Identifico si el empleado ya esta en una relacion de trabajo con alguna empresa
                    List<EmpleadoEmpresa> empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado).ToList();
                    if (empEmp.Count() < 1)
                    {
                        ContratarEmpleado(empleado, IdEmpresa);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        if (db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.idEmpleado == empleado.IdEmpleado && x.FechaBaja == null).FirstOrDefault() != null)
                        {
                            ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a esta empresa");
                        }
                        else
                        {
                            empEmp = empEmp.Where(x => x.FechaBaja == null).ToList();
                            if (empEmp.Count() > 0)
                            {
                                if (empleado.IdJornada == 2 || empleado.IdJornada == 1)
                                {
                                    if (empEmp.Where(x => x.IdJornada == 3).ToList().Count() >= 1)
                                    {
                                        ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a otra empresa");
                                    }
                                    else
                                    {
                                        empEmp = empEmp.Where(x => x.IdJornada == 2 || x.IdJornada == 1).ToList();
                                        if (empEmp.Count() < 2)
                                        {
                                            ContratarEmpleado(empleado, IdEmpresa);
                                            return RedirectToAction("Index");
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a 2(Dos) Empresas, no es posible que inicie una relacion con otra empresa");
                                        }
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("Nombre", "El Empleado ya se encuentra relacionado a otra empresa");
                                }
                            }
                            else
                            {
                                ContratarEmpleado(empleado, IdEmpresa);
                                return RedirectToAction("Index");
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("Cuil", "El Cuil ingresado no es valido");
                }
            }

            return View(empleado);
        }

        public ActionResult EditFechaBaja(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Empleado empleado = db.Empleado.Find(id);

            if (empleado == null)
            {
                return HttpNotFound();
            }

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == id && x.idEmpresa == IdEmpresa && x.FechaBaja != null).FirstOrDefault();

            empleado.Jornada = empEmp.Jornada.Descripcion;
            empleado.Categoria = empEmp.Categoria.Descripcion;
            empleado.EsAfiliado = empEmp.EsAfiliado;
            empleado.FechaAlta = empEmp.FechaAlta;
            empleado.FechaBaja = empEmp.FechaBaja.Value;

            //if (empEmp.IdJornada == 2)
            //{
            //    var turno = db.TurnoEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault();
            //    empleado.Turno = (turno != null) ? turno.Turno : "Mañana";
            //}
            //else if (empEmp.IdJornada == 1)
            //{
            //    var horarios = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).ToList();
            //    foreach (var horario in horarios)
            //    {
            //        switch (horario.Dia)
            //        {
            //            case "Lunes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeLunesManana = horario.Entrada;
            //                    empleado.hastaLunesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeLunesTarde = horario.Entrada;
            //                    empleado.hastaLunesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Martes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeMartesManana = horario.Entrada;
            //                    empleado.hastaMartesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeMartesManana = horario.Entrada;
            //                    empleado.hastaMartesManana = horario.Salida;
            //                }
            //                break;
            //            case "Miercoles":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeMiercolesManana = horario.Entrada;
            //                    empleado.hastaMiercolesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeMiercolesTarde = horario.Entrada;
            //                    empleado.hastaMiercolesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Jueves":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeJuevesManana = horario.Entrada;
            //                    empleado.hastaJuevesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeJuevesTarde = horario.Entrada;
            //                    empleado.hastaJuevesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Viernes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeViernesManana = horario.Entrada;
            //                    empleado.hastaViernesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeViernesTarde = horario.Entrada;
            //                    empleado.hastaViernesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Sabado":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeSabadoManana = horario.Entrada;
            //                    empleado.hastaSabadoManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeSabadoTarde = horario.Entrada;
            //                    empleado.hastaSabadoTarde = horario.Salida;
            //                }
            //                break;
            //            case "Domingo":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeDomingoManana = horario.Entrada;
            //                    empleado.hastaDomingoManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeDomingoTarde = horario.Entrada;
            //                    empleado.hastaDomingoTarde = horario.Salida;
            //                }
            //                break;
            //        }
            //    }
            //}

            var localidades = db.Localidad;
            foreach (var loc in localidades)
            {
                loc.Nombre = loc.Nombre + " (" + loc.CodPostal + ")";
            }
            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre", empleado.IdLocalidad);

            var localidad = db.Localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault();
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);

            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado && x.idEmpresa == IdEmpresa).FirstOrDefault();
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", empleadoEmpresa.IdCategoria);
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", empleadoEmpresa.IdJornada);
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFechaBaja([Bind(Include = "IdEmpleado,Nombre,Apellido,Dni,Cuil,Calle,Altura,IdLocalidad,FechaAlta,FechaBaja,IdCategoria,IdJornada,EsAfiliado"/*,Turno,desdeLunesManana,desdeLunesTarde,hastaLunesManana,hastaLunesTarde,desdeMartesManana,desdeMartesTarde,hastaMartesManana,hastaMartesTarde,desdeMiercolesManana,desdeMiercolesTarde,hastaMiercolesManana,hastaMiercolesTarde,desdeJuevesManana,desdeJuevesTarde,hastaJuevesManana,hastaJuevesTarde,desdeViernesManana,desdeViernesTarde,hastaViernesManana,hastaViernesTarde,desdeSabadoManana,desdeSabadoTarde,hastaSabadoManana,hastaSabadoTarde,desdeDomingoManana,desdeDomingoTarde,hastaDomingoManana,hastaDomingoTarde"*/)] Empleado empleado)
        {
            var localidades = db.Localidad;
            foreach (var item in localidades)
            {
                item.NombreCodPostal = item.Nombre + " (" + item.CodPostal + ")";
            }

            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre", empleado.IdLocalidad);
            var localidad = db.Localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault();
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado).FirstOrDefault();
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", empleadoEmpresa.IdCategoria);
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", empleadoEmpresa.IdJornada);

            if (ModelState.IsValid)
            {
                var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
                int IdEmpresa = Convert.ToInt32(claim.Value);

                EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado && x.idEmpresa == IdEmpresa && x.FechaBaja != null).FirstOrDefault();
                //empEmp.IdJornada = empleado.IdJornada;
                //empEmp.IdCategoria = empleado.IdCategoria;
                //empEmp.FechaAlta = empleado.FechaAlta;


                if (empleado.FechaAlta > empleado.FechaBaja || empleado.FechaBaja > DateTime.Today)
                {
                    if (empleado.FechaAlta > empleado.FechaBaja)
                    {
                        ModelState.AddModelError("FechaBaja", "La Fecha de Baja no puede ser menor a la fecha de alta del afiliado");
                    }
                    else
                    {
                        ModelState.AddModelError("FechaBaja", "La Fecha de Baja no puede ser mayor a la fecha de hoy");
                    }

                    return View(empleado);
                }

                empEmp.FechaBaja = empleado.FechaBaja;
                db.SaveChanges();

                return RedirectToAction("IndexBajas");
            }
            return View(empleado);
        }

        // GET: Empleados/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empleado empleado = db.Empleado.Find(id);
            if (empleado == null)
            {
                return HttpNotFound();
            }

            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == id && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();

            empleado.Jornada = empEmp.Jornada.Descripcion;
            empleado.Categoria = empEmp.Categoria.Descripcion;
            var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault();
            var hoy = DateTime.Today;
            //empleado.EsAfiliado = false;
            //if (afiliado != null)
            //{
            //    if (afiliado.FechaAlta.Year < hoy.Year)
            //    {
            //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > hoy.Year || (afiliado.FechaBaja.Value.Year == hoy.Year && afiliado.FechaBaja.Value.Month >= hoy.Month))
            //        {
            //            empleado.EsAfiliado = true;
            //        }
            //    }
            //    else if (afiliado.FechaAlta.Year == hoy.Year && afiliado.FechaAlta.Month <= hoy.Month)
            //    {
            //        if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > hoy.Year || (afiliado.FechaBaja.Value.Year == hoy.Year && afiliado.FechaBaja.Value.Month >= hoy.Month))
            //        {
            //            empleado.EsAfiliado = true;
            //        }
            //    }
            //}
            empleado.EsAfiliado = empEmp.EsAfiliado;
            empleado.FechaAlta = empEmp.FechaAlta;

            if (empEmp.IdJornada == 2)
            {
                var turno = db.TurnoEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault();
                empleado.Turno = (turno != null) ? turno.Turno : "Mañana";
            }
            else if(empEmp.IdJornada == 1)
            {
                var horarios = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).ToList();
                foreach (var horario in horarios)
                {
                    switch (horario.Dia)
                    {
                        case "Lunes":
                            if (horario.Turno == "Manana")
                            {
                                empleado.desdeLunesManana = horario.Entrada;
                                empleado.hastaLunesManana = horario.Salida;
                            }
                            if (horario.Turno == "Tarde")
                            {
                                empleado.desdeLunesTarde = horario.Entrada;
                                empleado.hastaLunesTarde = horario.Salida;
                            }
                            break;
                        case "Martes":
                            if (horario.Turno == "Manana")
                            {
                                empleado.desdeMartesManana = horario.Entrada;
                                empleado.hastaMartesManana = horario.Salida;
                            }
                            if (horario.Turno == "Tarde")
                            {
                                empleado.desdeMartesManana = horario.Entrada;
                                empleado.hastaMartesManana = horario.Salida;
                            }
                            break;
                        case "Miercoles":
                            if (horario.Turno == "Manana")
                            {
                                empleado.desdeMiercolesManana = horario.Entrada;
                                empleado.hastaMiercolesManana = horario.Salida;
                            }
                            if (horario.Turno == "Tarde")
                            {
                                empleado.desdeMiercolesTarde = horario.Entrada;
                                empleado.hastaMiercolesTarde = horario.Salida;
                            }
                            break;
                        case "Jueves":
                            if (horario.Turno == "Manana")
                            {
                                empleado.desdeJuevesManana = horario.Entrada;
                                empleado.hastaJuevesManana = horario.Salida;
                            }
                            if (horario.Turno == "Tarde")
                            {
                                empleado.desdeJuevesTarde = horario.Entrada;
                                empleado.hastaJuevesTarde = horario.Salida;
                            }
                            break;
                        case "Viernes":
                            if (horario.Turno == "Manana")
                            {
                                empleado.desdeViernesManana = horario.Entrada;
                                empleado.hastaViernesManana = horario.Salida;
                            }
                            if (horario.Turno == "Tarde")
                            {
                                empleado.desdeViernesTarde = horario.Entrada;
                                empleado.hastaViernesTarde = horario.Salida;
                            }
                            break;
                        case "Sabado":
                            if (horario.Turno == "Manana")
                            {
                                empleado.desdeSabadoManana = horario.Entrada;
                                empleado.hastaSabadoManana = horario.Salida;
                            }
                            if (horario.Turno == "Tarde")
                            {
                                empleado.desdeSabadoTarde = horario.Entrada;
                                empleado.hastaSabadoTarde = horario.Salida;
                            }
                            break;
                        case "Domingo":
                            if (horario.Turno == "Manana")
                            {
                                empleado.desdeDomingoManana = horario.Entrada;
                                empleado.hastaDomingoManana = horario.Salida;
                            }
                            if (horario.Turno == "Tarde")
                            {
                                empleado.desdeDomingoTarde = horario.Entrada;
                                empleado.hastaDomingoTarde = horario.Salida;
                            }
                            break;
                    }
                }
            }

            var localidades = db.Localidad;
            foreach (var loc in localidades)
            {
                loc.Nombre = loc.Nombre + " (" + loc.CodPostal + ")";
            }
            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre", empleado.IdLocalidad);

            var localidad = db.Localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault();
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);

            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", empleadoEmpresa.IdCategoria);
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", empleadoEmpresa.IdJornada);
            return View(empleado);
        }

        // POST: Empleados/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdEmpleado,Nombre,Apellido,Dni,Cuil,Calle,Altura,IdLocalidad,FechaAlta,IdCategoria,IdJornada,EsAfiliado"/*,Turno,desdeLunesManana,desdeLunesTarde,hastaLunesManana,hastaLunesTarde,desdeMartesManana,desdeMartesTarde,hastaMartesManana,hastaMartesTarde,desdeMiercolesManana,desdeMiercolesTarde,hastaMiercolesManana,hastaMiercolesTarde,desdeJuevesManana,desdeJuevesTarde,hastaJuevesManana,hastaJuevesTarde,desdeViernesManana,desdeViernesTarde,hastaViernesManana,hastaViernesTarde,desdeSabadoManana,desdeSabadoTarde,hastaSabadoManana,hastaSabadoTarde,desdeDomingoManana,desdeDomingoTarde,hastaDomingoManana,hastaDomingoTarde"*/)] Empleado empleado)
        {
            var localidades = db.Localidad;
            foreach (var item in localidades)
            {
                item.NombreCodPostal = item.Nombre + " (" + item.CodPostal + ")";
            }

            ViewBag.IdLocalidad = new SelectList(localidades, "IdLocalidad", "Nombre", empleado.IdLocalidad);
            var localidad = db.Localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault();
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado).FirstOrDefault();
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", empleadoEmpresa.IdCategoria);
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", empleadoEmpresa.IdJornada);

            if (ModelState.IsValid)
            {
                CUIT cuil = new CUIT(empleado.Cuil);
                if (cuil.EsValido)
                {
                    db.Entry(empleado).State = EntityState.Modified;
                    db.SaveChanges();

                    var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
                    int IdEmpresa = Convert.ToInt32(claim.Value);

                    EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();
                    empEmp.IdJornada = empleado.IdJornada;
                    empEmp.IdCategoria = empleado.IdCategoria;
                    empEmp.FechaAlta = empleado.FechaAlta;
                    db.SaveChanges();

                    //if (empEmp.IdJornada == 2)
                    //{
                    //    TurnoEmpleado turno;
                    //    if (db.TurnoEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault() == null)
                    //    {
                    //        turno = new TurnoEmpleado();
                    //        turno.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    //        turno.Turno = empleado.Turno;
                    //        db.TurnoEmpleado.Add(turno);
                    //    }
                    //    else
                    //    {
                    //        turno = db.TurnoEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault();
                    //        turno.Turno = empleado.Turno;
                    //    }
                    //    db.SaveChanges();
                    //}

                    //if (empEmp.IdJornada == 1)
                    //{
                    //    CargarHorarios(empleado);
                    //}

                    if (!empleado.EsAfiliado)
                    {
                        Afiliado afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.FechaBaja == null).FirstOrDefault();
                        if (afiliado != null)
                        {
                            empEmp.EsAfiliado = false;
                            afiliado.FechaBaja = DateTime.Today;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        Afiliado af = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault();

                        if(af == null || af.FechaBaja != null)
                        {
                            if(af == null)
                            {
                                af = new Afiliado();
                                af.FechaAlta = DateTime.Today;
                            }
                            af.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                            af.FechaBaja = null;

                            empEmp.EsAfiliado = true;

                            if (af.IdAfiliado == 0)
                            {
                                db.Afiliado.Add(af);
                            }
                        }

                        db.SaveChanges();
                    }
                }
                else
                {
                    ModelState.AddModelError("Cuil", "El Cuil ingresado no es valido");
                    
                    return View(empleado);
                }
                return RedirectToAction("Index");
            }
            return View(empleado);
        }

        // GET: Afiliados/ReActivate/5
        public ActionResult ReActivate(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Empleado empleado = db.Empleado.Find(id);

            if (empleado == null)
            {
                return HttpNotFound();
            }
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var localidad = db.Localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault();
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado).FirstOrDefault();

            ViewBag.IdLocalidad = new SelectList(db.Localidad, "IdLocalidad", "Nombre", empleado.IdLocalidad);
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", empleadoEmpresa.IdCategoria);
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", empleadoEmpresa.IdJornada);

            return View(empleado);
        }

        // POST: Afiliados/ReActivate/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReActivate([Bind(Include = "IdEmpleado,Nombre,Apellido,Dni,Cuil,Calle,Altura,IdLocalidad,FechaAlta,IdCategoria,IdJornada,EsAfiliado,ReActivate,Turno")] Empleado empleado)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            if (ModelState.IsValid)
            {
                if (empleado.ReActivate)
                {
                    EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado &&
                                                                           x.idEmpresa == IdEmpresa).FirstOrDefault();
                    if (empEmp == null)
                    {
                        return HttpNotFound();
                    }
                    empEmp.FechaBaja = null;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            var localidad = db.Localidad.Where(x => x.IdLocalidad == empleado.IdLocalidad).FirstOrDefault();
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado).FirstOrDefault();

            ViewBag.IdLocalidad = new SelectList(db.Localidad, "IdLocalidad", "Nombre", empleado.IdLocalidad);
            ViewBag.IdProvincia = new SelectList(db.Provincia, "IdProvincia", "Nombre", localidad.IdProvincia);
            ViewBag.IdCategoria = new SelectList(db.Categoria, "IdCategoria", "Descripcion", empleadoEmpresa.IdCategoria);
            ViewBag.IdJornada = new SelectList(db.Jornada, "IdJornada", "Descripcion", empleadoEmpresa.IdJornada);

            return View(empleado);
        }

        // GET: Empleados/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Empleado empleado = (from oEmpleados in db.Empleado
                                 join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
                                 join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
                                 where oEmpleadoEmpresa.idEmpleado == id
                                 select oEmpleados).FirstOrDefault();

            if (empleado == null)
            {
                return HttpNotFound();
            }
            
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == id &&
                                           x.idEmpresa == IdEmpresa &&
                                           x.FechaBaja == null)
                               .Include(t => t.Jornada)
                               .Include(t => t.Categoria)
                               .FirstOrDefault();

            empleado.Jornada = empEmp.Jornada.Descripcion;
            empleado.Categoria = empEmp.Categoria.Descripcion;
            empleado.EsAfiliado = empEmp.EsAfiliado;
            empleado.FechaBaja = empEmp.FechaAlta;

            //if (empEmp.IdJornada == 2)
            //{
            //    empleado.Turno = db.TurnoEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).FirstOrDefault().Turno;
            //}
            //else if (empEmp.IdJornada == 1)
            //{
            //    var horarios = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa).ToList();
            //    foreach (var horario in horarios)
            //    {
            //        switch (horario.Dia)
            //        {
            //            case "Lunes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeLunesManana = horario.Entrada;
            //                    empleado.hastaLunesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeLunesTarde = horario.Entrada;
            //                    empleado.hastaLunesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Martes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeMartesManana = horario.Entrada;
            //                    empleado.hastaMartesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeMartesManana = horario.Entrada;
            //                    empleado.hastaMartesManana = horario.Salida;
            //                }
            //                break;
            //            case "Miercoles":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeMiercolesManana = horario.Entrada;
            //                    empleado.hastaMiercolesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeMiercolesTarde = horario.Entrada;
            //                    empleado.hastaMiercolesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Jueves":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeJuevesManana = horario.Entrada;
            //                    empleado.hastaJuevesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeJuevesTarde = horario.Entrada;
            //                    empleado.hastaJuevesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Viernes":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeViernesManana = horario.Entrada;
            //                    empleado.hastaViernesManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeViernesTarde = horario.Entrada;
            //                    empleado.hastaViernesTarde = horario.Salida;
            //                }
            //                break;
            //            case "Sabado":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeSabadoManana = horario.Entrada;
            //                    empleado.hastaSabadoManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeSabadoTarde = horario.Entrada;
            //                    empleado.hastaSabadoTarde = horario.Salida;
            //                }
            //                break;
            //            case "Domingo":
            //                if (horario.Turno == "Manana")
            //                {
            //                    empleado.desdeDomingoManana = horario.Entrada;
            //                    empleado.hastaDomingoManana = horario.Salida;
            //                }
            //                if (horario.Turno == "Tarde")
            //                {
            //                    empleado.desdeDomingoTarde = horario.Entrada;
            //                    empleado.hastaDomingoTarde = horario.Salida;
            //                }
            //                break;
            //        }
            //    }
            //}

            return View(empleado);
        }

        // POST: Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, DateTime FechaBaja)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);
            Empleado empleado = db.Empleado.Find(id);
            EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == id && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();

            if (empEmp.FechaAlta > FechaBaja || FechaBaja > DateTime.Today)
            {
                empleado.Jornada = empEmp.Jornada.Descripcion;
                empleado.Categoria = empEmp.Categoria.Descripcion;
                empleado.EsAfiliado = empEmp.EsAfiliado;
                empleado.FechaBaja = empEmp.FechaAlta;

                if(empEmp.FechaAlta > FechaBaja)
                {
                    ViewBag.MensajeError = "La Fecha de Baja no puede ser menor a la fecha de alta del empleado";
                }
                else
                {
                    ViewBag.MensajeError = "La Fecha de Baja no puede ser mayor a la fecha de hoy";
                }

                return View(empleado);
            }

            empEmp.FechaBaja = FechaBaja;
            db.SaveChanges();
            Afiliado afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == id).FirstOrDefault();
            if (afiliado != null)
            {
                empEmp.EsAfiliado = false;
                afiliado.FechaBaja = DateTime.Today;
            }
            db.SaveChanges();
            return RedirectToAction("Deleted");
        }

        public ActionResult Deleted()
        {
            return View();
        }

        public decimal obtenerSueldo2(int IdEmpleadoEmpresa, int IdDeclaracionJurada, bool basico = false)
        {
            var declaracionJurada = db.DeclaracionJurada.Find(IdDeclaracionJurada);
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == IdEmpleadoEmpresa).FirstOrDefault();
            int mesAnterior = 0;
            int anioAnterior = 0;

            if (declaracionJurada.mes == 1)
            {
                mesAnterior = 11;//12;
                anioAnterior = declaracionJurada.anio - 1;
            }
            else if (declaracionJurada.mes == 7)
            {
                mesAnterior = declaracionJurada.mes - 2;
                anioAnterior = declaracionJurada.anio;
            }
            else
            {
                mesAnterior = declaracionJurada.mes - 1;
                anioAnterior = declaracionJurada.anio;
            }

            var detalleDeclaracionAnterior = db.DetalleDeclaracionJurada.Include(e => e.DeclaracionJurada)
                                                                        .Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa &&
                                                                                    x.DeclaracionJurada.mes == mesAnterior &&
                                                                                    x.DeclaracionJurada.anio == anioAnterior).FirstOrDefault();

            decimal sueldo = 1, sueldoAnterior = 1;

            if (detalleDeclaracionAnterior != null)
            {
                sueldoAnterior = detalleDeclaracionAnterior.Sueldo;
                //return Json(sueldo, JsonRequestBehavior.AllowGet);
            }

            /*
             * Establece el sueldo minimo basandose en la categoria y jornada del empleado
             */
            foreach (var sueldos in db.SueldoBasico.Where(x => x.IdCategoria == empleadoEmpresa.IdCategoria))
            {
                if (sueldos == null)
                {
                    sueldo = -1;
                    return sueldo;
                }

                if (sueldos.Desde.Year == declaracionJurada.anio)
                {
                    if(sueldos.Hasta.Year >= declaracionJurada.anio)
                    {
                        if (sueldos.Desde.Month <= declaracionJurada.mes && sueldos.Hasta.Month >= declaracionJurada.mes)
                        {
                            if (empleadoEmpresa.IdJornada == 2)
                            {
                                sueldo = (sueldos.Monto / 2);
                            }
                            else if (empleadoEmpresa.IdJornada == 3)
                            {
                                sueldo = sueldos.Monto;
                            }
                        }
                    }
                }
                else if (sueldos.Desde.Year < declaracionJurada.anio)
                {
                    if (sueldos.Hasta.Year >= declaracionJurada.anio)
                    {
                        if (sueldos.Hasta.Month >= declaracionJurada.mes)
                        {
                            if (empleadoEmpresa.IdJornada == 2)
                            {
                                sueldo = (sueldos.Monto / 2);
                            }
                            else if (empleadoEmpresa.IdJornada == 3)
                            {
                                sueldo = sueldos.Monto;
                            }
                        }
                    }
                }

                #region Old
                //if (sueldos != null && sueldos.Desde.Year == declaracionJurada.anio)
                //{
                //    if (sueldos.Hasta.Year == declaracionJurada.anio)
                //    {
                //        if (sueldos.Desde.Month <= declaracionJurada.mes && sueldos.Hasta.Month >= declaracionJurada.mes)
                //        {
                //            if (empleadoEmpresa.IdJornada == 2)
                //            {
                //                sueldo = (sueldos.Monto / 2);
                //            }
                //            else if (empleadoEmpresa.IdJornada == 3)
                //            {
                //                sueldo = sueldos.Monto;
                //            }
                //        }
                //    }
                //    if (sueldos.Hasta.Year > declaracionJurada.anio)
                //    {
                //        if (empleadoEmpresa.IdJornada == 2)
                //        {
                //            sueldo = (sueldos.Monto / 2);
                //        }
                //        else if (empleadoEmpresa.IdJornada == 3)
                //        {
                //            sueldo = sueldos.Monto;
                //        }
                //    }
                //}
                #endregion
            }

            /*
             * Sueldo basico mas antiguedad
             */
            decimal antiguedad = MonthDifference(new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1), empleadoEmpresa.FechaAlta);
            antiguedad = antiguedad / 12;
            sueldo += ((sueldo / 100) * Math.Truncate(antiguedad));

            /*
             * Sueldo basico mas presentismo
             */
            sueldo += ((sueldo / 100) * (decimal)8.33);

            /*
             * Verifica si el empleado esta en licencia en el momento de la declaracion jurada, si esta de licencia, el sistema
             * le permite ingresar un sueldo por debajo del minimo
             */
            bool enLicencia = false;
            /*
            where YEAR(FechaAltaLicencia) < @AÑO and (YEAR(FechaBajaLicencia) > @AÑO or (YEAR(FechaBajaLicencia) > @AÑO or (YEAR(FechaBajaLicencia) = @AÑO and MONTH(FechaAltaLicencia) >= @MES)) or
	        YEAR(FechaAltaLicencia) = @AÑO and MONTH(FechaAltaLicencia) <= @MES and     (YEAR(FechaBajaLicencia) > @AÑO or (YEAR(FechaBajaLicencia) = @AÑO and MONTH(FechaAltaLicencia) >= @MES))
            */

            var Mes = declaracionJurada.mes;
            var Año = declaracionJurada.anio;

            var licencias = db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa &&
                                                           (x.FechaAltaLicencia.Year < Año && (x.FechaBajaLicencia.Value.Year > Año || (x.FechaBajaLicencia.Value.Year == Año && x.FechaBajaLicencia.Value.Month >= Mes)) ||
                                                           x.FechaAltaLicencia.Year == Año && x.FechaAltaLicencia.Month <= Mes && (x.FechaBajaLicencia.Value.Year > Año || (x.FechaBajaLicencia.Value.Year == Año && x.FechaBajaLicencia.Value.Month >= Mes)))).ToList();
            foreach (var licencia in licencias)
            {
                //if(licencia.IdLicenciaLaboral != 3)
                //{
                //    if(licencia.FechaAltaLicencia.Year < declaracionJurada.anio || 
                //        (licencia.FechaAltaLicencia.Year == declaracionJurada.anio && licencia.FechaAltaLicencia.Month <= declaracionJurada.mes))
                //    {
                //        if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                //        {
                //            if(licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                //            {
                //                enLicencia = true;
                //                sueldo = 0;
                //            }
                //        }
                //        else if (licencia.FechaBajaLicencia.Value.Year > declaracionJurada.anio)
                //        {
                //            enLicencia = true;
                //            sueldo = 0;
                //        }
                //    }
                //}
                //else
                //{
                //    enLicencia = true;
                //}

                #region Old
                //if (licencia != null && licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
                //{
                //    if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                //    {
                //        if (licencia.FechaAltaLicencia.Month <= declaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                //        {
                //            enLicencia = true;
                //        }
                //    }
                //    if (licencia.FechaBajaLicencia.Value.Year > declaracionJurada.anio)
                //    {
                //        if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                //        {
                //            enLicencia = true;
                //        }
                //    }
                //}
                //if (licencia.FechaBajaLicencia.Value.Year >= declaracionJurada.anio)
                //{
                //    if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                //    {
                //        enLicencia = true;
                //    }
                //}
                //if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1))
                //{
                //    if (licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
                //    {
                //        enLicencia = true;
                //    }
                //}
                #endregion

                //if (enLicencia)
                //{
                    switch (licencia.IdLicenciaLaboral)
                    {
                        case 1://Enfermedad
                        {
                            if ((licencia.FechaBajaLicencia - licencia.FechaAltaLicencia) > new TimeSpan(365, 0, 0, 0))
                            {
                                sueldo = 1;
                                DateTime fecha = new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1);
                                if (MonthDifference(fecha, licencia.FechaAltaLicencia) >= 12)
                                {
                                    sueldo = 0;
                                }
                            }
                            break;
                        }
                        case 2://Maternidad
                        {
                            if (licencia.FechaAltaLicencia.Month == declaracionJurada.mes)
                            {
                                //Si el/la empleado/a entro en licencia por maternidad habiendo trabajado
                                //almenos un dia el sistema solicita ingresar un sueldo mayor a 1
                                if (licencia.FechaAltaLicencia.Day > 1)
                                {
                                    sueldo = 1;
                                }
                                //Si el/la empleado/a entro en licencia por maternidad el dia 1
                                //el sistema le permite ingresar 0
                                else
                                {
                                    sueldo = 0;
                                }
                            }
                            else if (licencia.FechaBajaLicencia.Value.Month == declaracionJurada.mes)
                            {
                                //Si el/la empleado/a volvio de licencia por maternidad antes de terminar el mes
                                //el sistema solicita ingresar un sueldo mayor a 1

                                #region DiasDelMes
                                int diasDelMes = 31;
                                switch (declaracionJurada.mes)
                                {
                                    case 2:
                                        diasDelMes = 30;
                                        break;
                                    case 4:
                                        diasDelMes = 30;
                                        break;
                                    case 6:
                                        diasDelMes = 30;
                                        break;
                                    case 9:
                                        diasDelMes = 30;
                                        break;
                                    case 11:
                                        diasDelMes = 30;
                                        break;
                                }
                                #endregion

                                if (licencia.FechaAltaLicencia.Day < diasDelMes)
                                {
                                    sueldo = 1;
                                }
                                else
                                {
                                    sueldo = 0;
                                }
                            }
                            else
                            {
                                sueldo = 0;
                            }
                            break;
                        }
                        case 3://Vacaciones
                        {
                            //Si el/la empleado/a estuvo de vacaciones el mes anterior a la declaracion jurada
                            //el sistema permite ingresar un sueldo menor que el minimo

                            if (declaracionJurada.mes != licencia.FechaAltaLicencia.Month)
                            {
                                sueldo = 0;
                            }
                            else if((licencia.FechaAltaLicencia.Month == licencia.FechaBajaLicencia.Value.Month) && licencia.FechaAltaLicencia.Month == declaracionJurada.mes)
                            {
                                sueldo = 0;
                            }

                            //if(declaracionJurada.mes != 1)
                            //{
                            //    if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                            //    {
                            //        if (licencia.FechaBajaLicencia.Value.Month == declaracionJurada.mes)
                            //        {
                            //            sueldo = 0;
                            //        }
                            //        else if (licencia.FechaBajaLicencia.Value.Month == (declaracionJurada.mes - 1))
                            //        {
                            //            sueldo = 0;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    if(licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                            //    {
                            //        if (licencia.FechaBajaLicencia.Value.Month == 1)
                            //        {
                            //            sueldo = 0;
                            //        }
                            //    }
                            //    else if(licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1))
                            //    {
                            //        if(licencia.FechaBajaLicencia.Value.Month == 12)
                            //        {
                            //            sueldo = 0;
                            //        }
                            //    }
                            //}

                            #region Old
                            //if (licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
                            //{
                            //    if (declaracionJurada.mes == 1 &&
                            //        licencia.FechaAltaLicencia.Month == (declaracionJurada.mes - 1))
                            //    {
                            //        sueldo = 0;
                            //    }
                            //}
                            //if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1) &&
                            //    licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
                            //{
                            //    sueldo = 0;
                            //}
                            #endregion

                            break;
                        }
                        case 5://Art. 211 – Conservación de Empleo 
                        {
                            //
                            sueldo = 0;
                            break;
                        }
                        case 6://Art. 217 – Fuero Sindical
                        {
                            //
                            sueldo = 0;
                            break;
                        }
                        case 7://Cargos Representativos
                        {
                            //
                            sueldo = 0;
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                //}
            }

            if (!basico && sueldoAnterior > sueldo)
            {
                sueldo = sueldoAnterior;
            }

            return sueldo;
        }

        public decimal obtenerSueldo5(int IdEmpleadoEmpresa, int IdDeclaracionJurada, bool basico = false)
        {
            var declaracionJurada = db.DeclaracionJurada.Find(IdDeclaracionJurada);
            var empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == IdEmpleadoEmpresa).FirstOrDefault();
            int mesAnterior = 0;
            int anioAnterior = 0;

            /*
             * Sueldo 5% Anterior
             */
            if (declaracionJurada.mes == 1)
            {
                mesAnterior = 11;//12;
                anioAnterior = declaracionJurada.anio - 1;
            }
            else if (declaracionJurada.mes == 7)
            {
                mesAnterior = declaracionJurada.mes - 2;
                anioAnterior = declaracionJurada.anio;
            }
            else
            {
                mesAnterior = declaracionJurada.mes - 1;
                anioAnterior = declaracionJurada.anio;
            }

            var detalleDeclaracionAnterior = db.DetalleDeclaracionJurada.Include(e => e.DeclaracionJurada)
                                                                        .Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa &&
                                                                                    x.DeclaracionJurada.mes == mesAnterior &&
                                                                                    x.DeclaracionJurada.anio == anioAnterior).FirstOrDefault();
            empleadoEmpresa.EsAfiliado = false;
            var afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa).FirstOrDefault();
            if (afiliado != null)
            {
                if (afiliado.FechaAlta.Year < declaracionJurada.anio)
                {
                    if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
                    {
                        empleadoEmpresa.EsAfiliado = true;
                    }
                }
                else if (afiliado.FechaAlta.Year == declaracionJurada.anio && afiliado.FechaAlta.Month <= declaracionJurada.mes)
                {
                    if (afiliado.FechaBaja == null || afiliado.FechaBaja.Value.Year > declaracionJurada.anio || (afiliado.FechaBaja.Value.Year == declaracionJurada.anio && afiliado.FechaBaja.Value.Month >= declaracionJurada.mes))
                    {
                        empleadoEmpresa.EsAfiliado = true;
                    }
                }
            }

            decimal sueldo = 0, sueldoAnterior = 0;

            if (empleadoEmpresa.EsAfiliado)
            {
                if (detalleDeclaracionAnterior != null)
                {
                    sueldoAnterior = detalleDeclaracionAnterior.SueldoBase;
                }


                /*
                 * Establece el sueldo minimo basandose en la categoria y jornada del empleado
                 */
                var sueldos = db.SueldoBasico.Where(x => x.IdCategoria == empleadoEmpresa.IdCategoria &&
                                                         (x.Hasta.Year > declaracionJurada.anio ||
                                                         (x.Hasta.Month >= declaracionJurada.mes && x.Hasta.Year == declaracionJurada.anio))).ToList();
                foreach (var sueldoCat in sueldos)
                {
                    if (sueldoCat == null)
                    {
                        sueldo = -1;
                        return sueldo;
                    }
                    if (sueldoCat != null && sueldoCat.Desde.Year <= declaracionJurada.anio)
                    {
                        if (sueldoCat.Hasta.Year == declaracionJurada.anio)
                        {
                            if (sueldoCat.Hasta.Month >= declaracionJurada.mes)
                            {
                                //if (empleadoEmpresa.IdJornada == 2)
                                //{
                                //    sueldo = (sueldos.Monto / 2);
                                //}
                                //else if (empleadoEmpresa.IdJornada == 3)
                                //{
                                sueldo = sueldoCat.Monto;
                                //}
                            }
                        }
                        if (sueldoCat.Hasta.Year > declaracionJurada.anio)
                        {
                            //if (empleadoEmpresa.IdJornada == 2)
                            //{
                            //    sueldo = (sueldos.Monto / 2);
                            //}
                            //else if (empleadoEmpresa.IdJornada == 3)
                            //{
                            sueldo = sueldoCat.Monto;
                            //}
                        }
                    }
                }

                /*
                 * Sueldo basico mas antiguedad
                 */
                int antiguedad = Convert.ToInt32(MonthDifference(new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1), empleadoEmpresa.FechaAlta));
                antiguedad = antiguedad / 12;
                sueldo += ((sueldo / 100) * antiguedad);

                /*
                 * Sueldo basico mas Presentismo
                 */
                sueldo += ((sueldo / 100) * (decimal)8.33);

                /*
                 * Verifica si el empleado esta en licencia en el momento de la declaracion jurada, si esta de licencia, el sistema
                 * le permite ingresar un sueldo por debajo del minimo
                 */
                bool enLicencia = false;
                foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa))
                {
                    if(licencia.IdLicenciaLaboral != 3)
                    {
                        if(licencia.FechaAltaLicencia.Year < declaracionJurada.anio || 
                            (licencia.FechaAltaLicencia.Year == declaracionJurada.anio && licencia.FechaAltaLicencia.Month <= declaracionJurada.mes))
                        {
                            if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                            {
                                if(licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                                {
                                    enLicencia = true;
                                }
                            }
                            else if (licencia.FechaBajaLicencia.Value.Year > declaracionJurada.anio)
                            {
                                enLicencia = true;
                            }
                        }
                    }
                    else
                    {
                        enLicencia = true;
                    }

                    #region Old
                    //if (licencia != null && licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
                    //{
                    //    if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                    //    {
                    //        if (licencia.FechaAltaLicencia.Month <= declaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                    //        {
                    //            enLicencia = true;
                    //        }
                    //    }
                    //    if (licencia.FechaBajaLicencia.Value.Year > declaracionJurada.anio)
                    //    {
                    //        if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                    //        {
                    //            enLicencia = true;
                    //        }
                    //    }
                    //}
                    //if (licencia.FechaBajaLicencia.Value.Year >= declaracionJurada.anio)
                    //{
                    //    if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                    //    {
                    //        enLicencia = true;
                    //    }
                    //}
                    //if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1))
                    //{
                    //    if (licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
                    //    {
                    //        enLicencia = true;
                    //    }
                    //}
                    #endregion

                    if (enLicencia)
                    {
                        switch (licencia.IdLicenciaLaboral)
                        {
                            case 1://Enfermedad
                                if ((licencia.FechaBajaLicencia - licencia.FechaAltaLicencia) > new TimeSpan(365, 0, 0, 0))
                                {
                                    sueldo = 1;
                                    DateTime fecha = new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1);
                                    if (MonthDifference(fecha, licencia.FechaAltaLicencia) >= 12)
                                    {
                                        sueldo = 0;
                                    }
                                }
                                break;
                            case 2://Maternidad
                                if (licencia.FechaAltaLicencia.Month == declaracionJurada.mes)
                                {
                                    //Si el/la empleado/a entro en licencia por maternidad habiendo trabajado
                                    //almenos un dia el sistema solicita ingresar un sueldo mayor a 1
                                    if (licencia.FechaAltaLicencia.Day > 1)
                                    {
                                        sueldo = 1;
                                    }
                                    //Si el/la empleado/a entro en licencia por maternidad el dia 1
                                    //el sistema le permite ingresar 0
                                    else
                                    {
                                        sueldo = 0;
                                    }
                                }
                                if (licencia.FechaBajaLicencia.Value.Month == declaracionJurada.mes)
                                {
                                    //Si el/la empleado/a volvio de licencia por maternidad antes de terminar el mes
                                    //el sistema solicita ingresar un sueldo mayor a 1

                                    #region DiasDelMes
                                    int diasDelMes = 31;
                                    switch (declaracionJurada.mes)
                                    {
                                        case 2:
                                            diasDelMes = 30;
                                            break;
                                        case 4:
                                            diasDelMes = 30;
                                            break;
                                        case 6:
                                            diasDelMes = 30;
                                            break;
                                        case 9:
                                            diasDelMes = 30;
                                            break;
                                        case 11:
                                            diasDelMes = 30;
                                            break;
                                    }
                                    #endregion

                                    if (licencia.FechaAltaLicencia.Day < diasDelMes)
                                    {
                                        sueldo = 1;
                                    }
                                    else
                                    {
                                        sueldo = 0;
                                    }
                                }
                                break;
                            case 3://Vacaciones
                                //Si el/la empleado/a estuvo de vacaciones el mes anterior a la declaracion jurada
                                //el sistema permite ingresar un sueldo menor que el minimo

                                if(declaracionJurada.mes != 1)
                                {
                                    if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                                    {
                                        if (licencia.FechaBajaLicencia.Value.Month == declaracionJurada.mes)
                                        {
                                            sueldo = 0;
                                        }
                                        else if (licencia.FechaBajaLicencia.Value.Month == (declaracionJurada.mes - 1))
                                        {
                                            sueldo = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    if(licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                                    {
                                        if (licencia.FechaBajaLicencia.Value.Month == 1)
                                        {
                                            sueldo = 0;
                                        }
                                    }
                                    else if(licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1))
                                    {
                                        if(licencia.FechaBajaLicencia.Value.Month == 12)
                                        {
                                            sueldo = 0;
                                        }
                                    }
                                }

                                #region Old
                                //if (licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
                                //{
                                //    if (declaracionJurada.mes == 1 &&
                                //        licencia.FechaAltaLicencia.Month == (declaracionJurada.mes - 1))
                                //    {
                                //        sueldo = 0;
                                //    }
                                //}
                                //if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1) &&
                                //    licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
                                //{
                                //    sueldo = 0;
                                //}
                                #endregion

                                break;
                            case 5://Art. 211 – Conservación de Empleo 
                                //
                                sueldo = 0;
                                break;
                            case 6://Art. 217 – Fuero Sindical
                                //
                                sueldo = 0;
                                break;
                            case 7://Cargos Representativos
                                //
                                sueldo = 0;
                                break;
                            default:
                                break;
                        }
                    }
                }

                #region Old
                /*
                 * Verifica si el empleado esta en licencia en el momento de la declaracion jurada, si esta de licencia, el sistema
                 * le permite ingresar un sueldo por debajo del minimo
                 */
                //bool enLicencia = false;
                //foreach (var licencia in db.LicenciaEmpleado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa))
                //{
                //    if (licencia != null && licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
                //    {
                //        if (licencia.FechaBajaLicencia.Value.Year == declaracionJurada.anio)
                //        {
                //            if (licencia.FechaAltaLicencia.Month <= declaracionJurada.mes && licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                //            {
                //                enLicencia = true;
                //            }
                //        }
                //        if (licencia.FechaBajaLicencia.Value.Year > declaracionJurada.anio)
                //        {
                //            if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                //            {
                //                enLicencia = true;
                //            }
                //        }
                //    }
                //    if (licencia.FechaBajaLicencia.Value.Year >= declaracionJurada.anio)
                //    {
                //        if (licencia.FechaBajaLicencia.Value.Month >= declaracionJurada.mes)
                //        {
                //            enLicencia = true;
                //        }
                //    }
                //    if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1))
                //    {
                //        if (licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
                //        {
                //            enLicencia = true;
                //        }
                //    }
                //    if (enLicencia)
                //    {
                //        switch (licencia.IdLicenciaLaboral)
                //        {
                //            case 1://Enfermedad
                //                if ((licencia.FechaBajaLicencia - licencia.FechaAltaLicencia) > new TimeSpan(365, 0, 0, 0))
                //                {
                //                    sueldo = 1;
                //                    DateTime fecha = new DateTime(declaracionJurada.anio, declaracionJurada.mes, 1);
                //                    if (MonthDifference(fecha, licencia.FechaAltaLicencia) >= 12)
                //                    {
                //                        sueldo = 0;
                //                    }
                //                }
                //                break;
                //            case 2://Maternidad
                //                if (licencia.FechaAltaLicencia.Month == declaracionJurada.mes)
                //                {
                //                    //Si el/la empleado/a entro en licencia por maternidad habiendo trabajado
                //                    //almenos un dia el sistema solicita ingresar un sueldo mayor a 1
                //                    if (licencia.FechaAltaLicencia.Day > 1)
                //                    {
                //                        sueldo = 1;
                //                    }
                //                    //Si el/la empleado/a entro en licencia por maternidad el dia 1
                //                    //el sistema le permite ingresar 0
                //                    else
                //                    {
                //                        sueldo = 0;
                //                    }
                //                }
                //                if (licencia.FechaBajaLicencia.Value.Month == declaracionJurada.mes)
                //                {
                //                    //Si el/la empleado/a volvio de licencia por maternidad antes de terminar el mes
                //                    //el sistema solicita ingresar un sueldo mayor a 1
                //                    if (licencia.FechaAltaLicencia.Day < 31)
                //                    {
                //                        sueldo = 1;
                //                    }
                //                    else
                //                    {
                //                        sueldo = 0;
                //                    }
                //                }
                //                break;
                //            case 3://Vacaciones
                //                //Si el/la empleado/a estuvo de vacaciones el mes anterior a la declaracion jurada
                //                //el sistema permite ingresar un sueldo menor que el minimo
                //                if (licencia.FechaAltaLicencia.Year == declaracionJurada.anio)
                //                {
                //                    if (declaracionJurada.mes == 1 &&
                //                        licencia.FechaAltaLicencia.Month == (declaracionJurada.mes - 1))
                //                    {
                //                        sueldo = 0;
                //                    }
                //                }
                //                if (licencia.FechaBajaLicencia.Value.Year == (declaracionJurada.anio - 1) &&
                //                    licencia.FechaBajaLicencia.Value.Month == 12 && declaracionJurada.mes == 1)
                //                {
                //                    sueldo = 0;
                //                }
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}
                #endregion

                if (!basico && sueldoAnterior > sueldo)
                {
                    sueldo = sueldoAnterior;
                }
            }

            return sueldo;

        }

        public static decimal MonthDifference(DateTime FechaFin, DateTime FechaInicio)
        {
            return Math.Abs((FechaFin.Month - FechaInicio.Month) + 12 * (FechaFin.Year - FechaInicio.Year));
        }

        private ActionResult ContratarEmpleado(Empleado empleado, int IdEmpresa)
        {
            EmpleadoEmpresa empleadoEmpresa = db.EmpleadoEmpresa.Where(x => x.idEmpresa == IdEmpresa && x.idEmpleado == empleado.IdEmpleado).FirstOrDefault();
            if(empleadoEmpresa == null)
            {
                empleadoEmpresa = new EmpleadoEmpresa();
            }
            empleadoEmpresa.idEmpleado = empleado.IdEmpleado;
            empleadoEmpresa.idEmpresa = IdEmpresa;
            empleadoEmpresa.IdCategoria = empleado.IdCategoria;
            empleadoEmpresa.IdJornada = empleado.IdJornada;
            empleadoEmpresa.EsAfiliado = empleado.EsAfiliado;
            empleadoEmpresa.FechaAlta = empleado.FechaAlta;
            empleadoEmpresa.FechaBaja = null;
            if(empleadoEmpresa.idEmpleadoEmpresa == 0)
            {
                db.EmpleadoEmpresa.Add(empleadoEmpresa);
            }
            db.SaveChanges();

            //if(empleadoEmpresa.IdJornada == 1)
            //{
            //    CargarHorarios(empleado);
            //}

            //if(empleadoEmpresa.IdJornada == 2)
            //{
            //    TurnoEmpleado turnoEmpleado = new TurnoEmpleado();
            //    turnoEmpleado.IdEmpleadoEmpresa = empleadoEmpresa.idEmpleadoEmpresa;
            //    turnoEmpleado.Turno = empleado.Turno;
            //    db.TurnoEmpleado.Add(turnoEmpleado);
            //    db.SaveChanges();
            //}

            if (empleado.EsAfiliado)
            {
                Afiliado af = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleadoEmpresa.idEmpleadoEmpresa).FirstOrDefault();
                if (af == null)
                {
                    af = new Afiliado();
                }

                af.IdEmpleadoEmpresa = empleadoEmpresa.idEmpleadoEmpresa;
                af.FechaAlta = DateTime.Today;
                af.FechaBaja = null;

                EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleadoEmpresa == af.IdEmpleadoEmpresa && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();
                empEmp.EsAfiliado = true;

                if (af.IdAfiliado == 0)
                {
                    db.Afiliado.Add(af);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private void CargarHorarios(Empleado empleado)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            EmpleadoEmpresa empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado && x.idEmpresa == IdEmpresa && x.FechaBaja == null).FirstOrDefault();

            if (empleado.desdeLunesManana != null)
            {
                HorarioEmpleado lunes;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Lunes" && x.Turno == "Manana").FirstOrDefault() == null)
                {
                    lunes = new HorarioEmpleado();
                    lunes.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    lunes.Dia = "Lunes";
                    lunes.Turno = "Manana";
                    lunes.Entrada = empleado.desdeLunesManana;
                    lunes.Salida = empleado.hastaLunesManana;
                    db.HorarioEmpleado.Add(lunes);
                }
                else
                {
                    lunes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Lunes" && x.Turno == "Manana").FirstOrDefault();
                    lunes.Entrada = empleado.desdeLunesManana;
                    lunes.Salida = empleado.hastaLunesManana;
                }
            }
            else
            {
                var lunes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Lunes" && x.Turno == "Manana").FirstOrDefault();
                if (lunes != null)
                {
                    db.HorarioEmpleado.Remove(lunes);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeLunesTarde != null)
            {
                HorarioEmpleado lunes;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Lunes" && x.Turno == "Tarde").FirstOrDefault() == null)
                {
                    lunes = new HorarioEmpleado();
                    lunes.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    lunes.Dia = "Lunes";
                    lunes.Turno = "Tarde";
                    lunes.Entrada = empleado.desdeLunesTarde;
                    lunes.Salida = empleado.hastaLunesTarde;
                    db.HorarioEmpleado.Add(lunes);
                }
                else
                {
                    lunes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Lunes" && x.Turno == "Tarde").FirstOrDefault();
                    lunes.Entrada = empleado.desdeLunesTarde;
                    lunes.Salida = empleado.hastaLunesTarde;
                }
            }
            else
            {
                var lunes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Lunes" && x.Turno == "Tarde").FirstOrDefault();
                if (lunes != null)
                {
                    db.HorarioEmpleado.Remove(lunes);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeMartesManana != null)
            {
                HorarioEmpleado martes;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Martes" && x.Turno == "Manana").FirstOrDefault() == null)
                {
                    martes = new HorarioEmpleado();
                    martes.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    martes.Dia = "Martes";
                    martes.Turno = "Manana";
                    martes.Entrada = empleado.desdeMartesManana;
                    martes.Salida = empleado.hastaMartesManana;
                    db.HorarioEmpleado.Add(martes);
                }
                else
                {
                    martes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Martes" && x.Turno == "Manana").FirstOrDefault();
                    martes.Entrada = empleado.desdeMartesManana;
                    martes.Salida = empleado.hastaMartesManana;
                }
            }
            else
            {
                var martes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Martes" && x.Turno == "Manana").FirstOrDefault();
                if (martes != null)
                {
                    db.HorarioEmpleado.Remove(martes);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeMartesTarde != null)
            {
                HorarioEmpleado martes;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Martes" && x.Turno == "Tarde").FirstOrDefault() == null)
                {
                    martes = new HorarioEmpleado();
                    martes.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    martes.Dia = "Martes";
                    martes.Turno = "Tarde";
                    martes.Entrada = empleado.desdeMartesTarde;
                    martes.Salida = empleado.hastaMartesTarde;
                    db.HorarioEmpleado.Add(martes);
                }
                else
                {
                    martes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Martes" && x.Turno == "Tarde").FirstOrDefault();
                    martes.Entrada = empleado.desdeMartesTarde;
                    martes.Salida = empleado.hastaMartesTarde;
                }
            }
            else
            {
                var martes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Martes" && x.Turno == "Tarde").FirstOrDefault();
                if (martes != null)
                {
                    db.HorarioEmpleado.Remove(martes);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeMiercolesManana != null)
            {
                HorarioEmpleado miercoles;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Miercoles" && x.Turno == "Manana").FirstOrDefault() == null)
                {
                    miercoles = new HorarioEmpleado();
                    miercoles.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    miercoles.Dia = "Miercoles";
                    miercoles.Turno = "Manana";
                    miercoles.Entrada = empleado.desdeMiercolesManana;
                    miercoles.Salida = empleado.hastaMiercolesManana;
                    db.HorarioEmpleado.Add(miercoles);
                }
                else
                {
                    miercoles = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Miercoles" && x.Turno == "Manana").FirstOrDefault();
                    miercoles.Entrada = empleado.desdeMiercolesManana;
                    miercoles.Salida = empleado.hastaMiercolesManana;
                }
            }
            else
            {
                var miercoles = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Miercoles" && x.Turno == "Manana").FirstOrDefault();
                if (miercoles != null)
                {
                    db.HorarioEmpleado.Remove(miercoles);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeMiercolesTarde != null)
            {
                HorarioEmpleado miercoles;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Miercoles" && x.Turno == "Tarde").FirstOrDefault() == null)
                {
                    miercoles = new HorarioEmpleado();
                    miercoles.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    miercoles.Dia = "Miercoles";
                    miercoles.Turno = "Tarde";
                    miercoles.Entrada = empleado.desdeMiercolesTarde;
                    miercoles.Salida = empleado.hastaMiercolesTarde;
                    db.HorarioEmpleado.Add(miercoles);
                }
                else
                {
                    miercoles = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Miercoles" && x.Turno == "Tarde").FirstOrDefault();
                    miercoles.Entrada = empleado.desdeMiercolesTarde;
                    miercoles.Salida = empleado.hastaMiercolesTarde;
                }
            }
            else
            {
                var miercoles = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Miercoles" && x.Turno == "Tarde").FirstOrDefault();
                if (miercoles != null)
                {
                    db.HorarioEmpleado.Remove(miercoles);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeJuevesManana != null)
            {
                HorarioEmpleado jueves;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Jueves" && x.Turno == "Manana").FirstOrDefault() == null)
                {
                    jueves = new HorarioEmpleado();
                    jueves.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    jueves.Dia = "Jueves";
                    jueves.Turno = "Manana";
                    jueves.Entrada = empleado.desdeJuevesManana;
                    jueves.Salida = empleado.hastaJuevesManana;
                    db.HorarioEmpleado.Add(jueves);
                }
                else
                {
                    jueves = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Jueves" && x.Turno == "Manana").FirstOrDefault();
                    jueves.Entrada = empleado.desdeJuevesManana;
                    jueves.Salida = empleado.hastaJuevesManana;
                }
            }
            else
            {
                var jueves = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Jueves" && x.Turno == "Manana").FirstOrDefault();
                if (jueves != null)
                {
                    db.HorarioEmpleado.Remove(jueves);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeJuevesTarde != null)
            {
                HorarioEmpleado jueves;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Jueves" && x.Turno == "Tarde").FirstOrDefault() == null)
                {
                    jueves = new HorarioEmpleado();
                    jueves.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    jueves.Dia = "Jueves";
                    jueves.Turno = "Tarde";
                    jueves.Entrada = empleado.desdeJuevesTarde;
                    jueves.Salida = empleado.hastaJuevesTarde;
                    db.HorarioEmpleado.Add(jueves);
                }
                else
                {
                    jueves = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Jueves" && x.Turno == "Tarde").FirstOrDefault();
                    jueves.Entrada = empleado.desdeJuevesTarde;
                    jueves.Salida = empleado.hastaJuevesTarde;
                }
            }
            else
            {
                var jueves = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Jueves" && x.Turno == "Tarde").FirstOrDefault();
                if (jueves != null)
                {
                    db.HorarioEmpleado.Remove(jueves);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeViernesManana != null)
            {
                HorarioEmpleado viernes;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Viernes" && x.Turno == "Manana").FirstOrDefault() == null)
                {
                    viernes = new HorarioEmpleado();
                    viernes.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    viernes.Dia = "Viernes";
                    viernes.Turno = "Manana";
                    viernes.Entrada = empleado.desdeViernesManana;
                    viernes.Salida = empleado.hastaViernesManana;
                    db.HorarioEmpleado.Add(viernes);
                }
                else
                {
                    viernes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Viernes" && x.Turno == "Manana").FirstOrDefault();
                    viernes.Entrada = empleado.desdeViernesManana;
                    viernes.Salida = empleado.hastaViernesManana;
                }
            }
            else
            {
                var viernes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Viernes" && x.Turno == "Manana").FirstOrDefault();
                if (viernes != null)
                {
                    db.HorarioEmpleado.Remove(viernes);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeViernesTarde != null)
            {
                HorarioEmpleado viernes;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Viernes" && x.Turno == "Tarde").FirstOrDefault() == null)
                {
                    viernes = new HorarioEmpleado();
                    viernes.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    viernes.Dia = "Viernes";
                    viernes.Turno = "Tarde";
                    viernes.Entrada = empleado.desdeViernesTarde;
                    viernes.Salida = empleado.hastaViernesTarde;
                    db.HorarioEmpleado.Add(viernes);
                }
                else
                {
                    viernes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Viernes" && x.Turno == "Tarde").FirstOrDefault();
                    viernes.Entrada = empleado.desdeViernesTarde;
                    viernes.Salida = empleado.hastaViernesTarde;
                }
            }
            else
            {
                var viernes = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Viernes" && x.Turno == "Tarde").FirstOrDefault();
                if (viernes != null)
                {
                    db.HorarioEmpleado.Remove(viernes);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeSabadoManana != null)
            {
                HorarioEmpleado sabado;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Sabado" && x.Turno == "Manana").FirstOrDefault() == null)
                {
                    sabado = new HorarioEmpleado();
                    sabado.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    sabado.Dia = "Sabado";
                    sabado.Turno = "Manana";
                    sabado.Entrada = empleado.desdeSabadoManana;
                    sabado.Salida = empleado.hastaSabadoManana;
                    db.HorarioEmpleado.Add(sabado);
                }
                else
                {
                    sabado = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Sabado" && x.Turno == "Manana").FirstOrDefault();
                    sabado.Entrada = empleado.desdeSabadoManana;
                    sabado.Salida = empleado.hastaSabadoManana;
                }
            }
            else
            {
                var sabado = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Sabado" && x.Turno == "Manana").FirstOrDefault();
                if (sabado != null)
                {
                    db.HorarioEmpleado.Remove(sabado);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeSabadoTarde != null)
            {
                HorarioEmpleado sabado;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Sabado" && x.Turno == "Tarde").FirstOrDefault() == null)
                {
                    sabado = new HorarioEmpleado();
                    sabado.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    sabado.Dia = "Sabado";
                    sabado.Turno = "Tarde";
                    sabado.Entrada = empleado.desdeSabadoTarde;
                    sabado.Salida = empleado.hastaSabadoTarde;
                    db.HorarioEmpleado.Add(sabado);
                }
                else
                {
                    sabado = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Sabado" && x.Turno == "Tarde").FirstOrDefault();
                    sabado.Entrada = empleado.desdeSabadoTarde;
                    sabado.Salida = empleado.hastaSabadoTarde;
                }
            }
            else
            {
                var sabado = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Sabado" && x.Turno == "Tarde").FirstOrDefault();
                if (sabado != null)
                {
                    db.HorarioEmpleado.Remove(sabado);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeDomingoManana != null)
            {
                HorarioEmpleado domingo;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Domingo" && x.Turno == "Manana").FirstOrDefault() == null)
                {
                    domingo = new HorarioEmpleado();
                    domingo.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    domingo.Dia = "Domingo";
                    domingo.Turno = "Manana";
                    domingo.Entrada = empleado.desdeDomingoManana;
                    domingo.Salida = empleado.hastaDomingoManana;
                    db.HorarioEmpleado.Add(domingo);
                }
                else
                {
                    domingo = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Domingo" && x.Turno == "Manana").FirstOrDefault();
                    domingo.Entrada = empleado.desdeDomingoManana;
                    domingo.Salida = empleado.hastaDomingoManana;
                }
            }
            else
            {
                var domingo = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Domingo" && x.Turno == "Manana").FirstOrDefault();
                if (domingo != null)
                {
                    db.HorarioEmpleado.Remove(domingo);
                    db.SaveChanges();
                }
            }

            if (empleado.desdeDomingoTarde != null)
            {
                HorarioEmpleado domingo;
                if (db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Domingo" && x.Turno == "Tarde").FirstOrDefault() == null)
                {
                    domingo = new HorarioEmpleado();
                    domingo.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa;
                    domingo.Dia = "Domingo";
                    domingo.Turno = "Tarde";
                    domingo.Entrada = empleado.desdeDomingoTarde;
                    domingo.Salida = empleado.hastaDomingoTarde;
                    db.HorarioEmpleado.Add(domingo);
                }
                else
                {
                    domingo = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Domingo" && x.Turno == "Tarde").FirstOrDefault();
                    domingo.Entrada = empleado.desdeDomingoTarde;
                    domingo.Salida = empleado.hastaDomingoTarde;
                }
            }
            else
            {
                var domingo = db.HorarioEmpleado.Where(x => x.IdEmpleadoEmpresa == empEmp.idEmpleadoEmpresa && x.Dia == "Domingo" && x.Turno == "Tarde").FirstOrDefault();
                if (domingo != null)
                {
                    db.HorarioEmpleado.Remove(domingo);
                    db.SaveChanges();
                }
            }

            db.SaveChanges();
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
