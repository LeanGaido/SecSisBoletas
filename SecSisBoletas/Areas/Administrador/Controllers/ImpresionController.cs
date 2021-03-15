using DAL;
using DAL.Models;
using DAL.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Rotativa;

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    public class ImpresionController : Controller
    {
        private SecModel db = new SecModel();

        //Get
        public ActionResult ImprimirFaecys()
        {
            ViewBag.Mes = DateTime.Today.Month - 1;
            ViewBag.Anio = DateTime.Today.Year;
            return View();
        }

        [HttpPost]
        public ActionResult ImprimirFaecys(int mes, int anio)
        {
            List<DetalleDeclaracionJurada> detalles = db.DetalleDeclaracionJurada.Include(e => e.DeclaracionJurada)
                                                                                 .Include(e => e.EmpleadoEmpresa)
                                                                                 .Where(x => x.DeclaracionJurada.mes == mes &&
                                                                                              x.DeclaracionJurada.anio == anio).ToList();

            if (detalles.Count < 1)
            {
                ViewBag.Mes = mes;
                ViewBag.Anio = anio;
                return View();
            }

            int[] periodo = new int[2] { mes, anio };
            Session["periodo"] = periodo;

            return Redirect("~/Areas/Administrador/Reports/Faecys.aspx");
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<VmFaecys> GetFaecys(int[] periodo)
        {
            List<VmFaecys> faecys = new List<VmFaecys>();

            bool hayDatos = false;
            VmFaecys tvm;

            //Datos de la Empresa
            int mes = periodo[0];
            int anio = periodo[1];
            var detalleDeclaracionJurada = db.DetalleDeclaracionJurada.Include(e => e.DeclaracionJurada)
                                                                       .Include(e => e.EmpleadoEmpresa)
                                                                       .Where(x => x.DeclaracionJurada.mes == mes &&
                                                                                   x.DeclaracionJurada.anio == anio).ToList();

            foreach (var item in detalleDeclaracionJurada)
            {
                tvm = new VmFaecys();
                int idEmpresa = item.EmpleadoEmpresa.idEmpresa;
                Empresa empresa = db.Empresa.Where(x => x.IdEmpresa == idEmpresa).FirstOrDefault();
                int idEmpleado = item.EmpleadoEmpresa.idEmpleado;
                Empleado empleado = db.Empleado.Where(x => x.IdEmpleado == idEmpleado).FirstOrDefault();

                tvm.Periodo = periodo[0].ToString() + " de " + periodo[1].ToString();
                //tvm.CantEmpleados = detalleDeclaracionJurada.Count().ToString();
                tvm.NombreEmpleado = (empleado.Apellido + " " + empleado.Nombre);
                tvm.CuilEmpleado = empleado.Cuil;
                tvm.Empresa = empresa.NombreFantasia;
                tvm.CuitEmpresa = empresa.Cuit;

                hayDatos = true;

                if (hayDatos)
                {
                    faecys.Add(tvm);
                }
            }

            return faecys.ToList();
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<VmEmpresa> GetEmpresas(string[] filtros)
        {
            List<VmEmpresa> empresas = new List<VmEmpresa>();

            VmEmpresa tvm;

            //Datos de la Empresa

            var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);

            if (int.Parse(filtros[3]) != 0)
            {
                int idProvincia = int.Parse(filtros[3]);
                empresa.Where(x => x.Localidad.IdProvincia == idProvincia);
            }

            if (int.Parse(filtros[2]) != 0)
            {
                int idLocalidad = int.Parse(filtros[2]);
                empresa = empresa.Where(x => x.IdLocalidad == idLocalidad);
            }

            if (int.Parse(filtros[4]) != 0)
            {
                int idActividad = int.Parse(filtros[4]);
                empresa = empresa.Where(x => x.IdActividad == idActividad);
            }

            if (!string.IsNullOrEmpty(filtros[1]))
            {
                string searchString = filtros[1];
                empresa = empresa.Where(x => x.RazonSocial.Contains(searchString));
            }

            switch (filtros[0])
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


            return empresas.ToList();
        }

        //[DataObjectMethod(DataObjectMethodType.Select)]
        //public List<VmEmpleadosEmpresas> GetEmpleadosEmpresas(string[] filtros)
        //{
        //    List<VmEmpleadosEmpresas> empleadosEmpresas = new List<VmEmpleadosEmpresas>();

        //    VmEmpleadosEmpresas tvm;

        //    //Datos de la Empresa

        //    var empresa = db.Empresa.Include(e => e.Actividad).Include(e => e.Localidad);

        //    //if (int.Parse(filtros[3]) != 0)
        //    //{
        //    //    int idProvincia = int.Parse(filtros[3]);
        //    //    empresa.Where(x => x.Localidad.IdProvincia == idProvincia);
        //    //}

        //    //if (int.Parse(filtros[2]) != 0)
        //    //{
        //    //    int idLocalidad = int.Parse(filtros[2]);
        //    //    empresa = empresa.Where(x => x.IdLocalidad == idLocalidad);
        //    //}

        //    //if (int.Parse(filtros[4]) != 0)
        //    //{
        //    //    int idActividad = int.Parse(filtros[4]);
        //    //    empresa = empresa.Where(x => x.IdActividad == idActividad);
        //    //}

        //    //if (!string.IsNullOrEmpty(filtros[1]))
        //    //{
        //    //    string searchString = filtros[1];
        //    //    empresa = empresa.Where(x => x.RazonSocial.Contains(searchString));
        //    //}

        //    //switch (filtros[0])
        //    //{
        //    //    case "name_desc":
        //    //        empresa = empresa.OrderByDescending(x => x.RazonSocial);
        //    //        break;
        //    //    case "nombreFantasia":
        //    //        empresa = empresa.OrderBy(s => s.NombreFantasia);
        //    //        break;
        //    //    case "nombreFantasia_desc":
        //    //        empresa = empresa.OrderByDescending(s => s.NombreFantasia);
        //    //        break;
        //    //    case "localidad":
        //    //        empresa = empresa.Include(t => t.Localidad).OrderBy(s => s.Localidad.Nombre);
        //    //        break;
        //    //    case "localidad_desc":
        //    //        empresa = empresa.Include(t => t.Localidad).OrderByDescending(s => s.Localidad.Nombre);
        //    //        break;
        //    //    default:
        //    //        empresa = empresa.OrderBy(x => x.RazonSocial);
        //    //        break;
        //    //}

        //    foreach (var emp in empresa)
        //    {
        //        var empleados = (from oEmpleados in db.Empleado
        //                            join oLocalidades in db.Localidad on oEmpleados.IdLocalidad equals oLocalidades.IdLocalidad
        //                            join oEmpleadoEmpresa in db.EmpleadoEmpresa on oEmpleados.IdEmpleado equals oEmpleadoEmpresa.idEmpleado
        //                        where oEmpleadoEmpresa.idEmpresa == emp.IdEmpresa
        //                        select oEmpleados).ToList();

        //        foreach (var empleado in empleados)
        //        {
        //            var empEmp = db.EmpleadoEmpresa.Where(x => x.idEmpleado == empleado.IdEmpleado &&
        //                                                       x.idEmpresa == emp.IdEmpresa &&
        //                                                       x.FechaBaja == null).FirstOrDefault();
        //            if(empEmp != null)
        //            {
        //                tvm = new VmEmpleadosEmpresas();
        //                tvm.RazonSocialEmpresa = emp.RazonSocial;
        //                tvm.CuitEmpresa = emp.Cuit;
        //                tvm.CalleEmpresa = emp.Calle + " " + emp.Altura;
        //                tvm.LocalidadEmpresa = emp.Localidad.Nombre;
        //                tvm.ProvinciaEmpresa = emp.Localidad.Provincia.Nombre;
        //                tvm.NombreEmpleado = empleado.Apellido + " " + empleado.Nombre;
        //                tvm.CuilEmpleado = empleado.Cuil;
        //                tvm.LocalidadEmpleado = empleado.Localidad.Nombre;
        //                tvm.ProvinciaEmpleado = empleado.Localidad.Provincia.Nombre;
        //                tvm.CategoríaEmpleado = empEmp.Categoria.Descripcion;
        //                tvm.JornadaEmpleado = empEmp.Jornada.Descripcion;

        //                empleadosEmpresas.Add(tvm);
        //            }
        //        }

        //    }

        //    return empleadosEmpresas.ToList();
        //}
    }
}