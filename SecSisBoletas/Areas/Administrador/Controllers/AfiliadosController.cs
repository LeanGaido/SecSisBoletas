using DAL;
using DAL.Models;
using DAL.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SecSisBoletas.Areas.Administrador.Controllers
{
    public class AfiliadosController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Administrador/Afiliados
        public ActionResult ImportarAfiliados()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportarAfiliados(HttpPostedFileBase fileAfiliados)
        {
            List<Afiliado> afiliados = new List<Afiliado>();
            List<MessageVm> errores = new List<MessageVm>();
            try
            {
                if (fileAfiliados != null && fileAfiliados.ContentLength > 0)
                {
                    List<string> rows = new List<string>();
                    StreamReader fileContent = new StreamReader(fileAfiliados.InputStream);
                    do
                    {
                        rows.Add(fileContent.ReadLine());
                    }
                    while (!fileContent.EndOfStream);
                    foreach (var row in rows)
                    {
                        string[] datosAfiliado = row.Split(';');
                        if (datosAfiliado.Length == 5)
                        {
                            string cuitEmpresa = datosAfiliado[0];
                            string razonSocial = datosAfiliado[1];
                            string cuilEmpleado = datosAfiliado[2];
                            string dniEmpleado = datosAfiliado[3];
                            string fechaAlta = datosAfiliado[4];

                            DateTime fechaAltaAfiliado = new DateTime();

                            bool resultado = DateTime.TryParse(fechaAlta, out fechaAltaAfiliado);

                            if(resultado)
                            {
                                var empresa = db.Empresa.AsNoTracking().Where(x => x.Cuit == cuitEmpresa).FirstOrDefault();
                                if(empresa == null)
                                {
                                    errores.Add(new MessageVm()
                                    {
                                        Type = "alert-danger",
                                        Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", No existe empresa con cuit: " + cuitEmpresa + ", razon social: " + razonSocial + ".",
                                        Dismissible = true
                                    });
                                    continue;
                                }

                                string busqueda = (dniEmpleado == "00000000000") ? cuilEmpleado : dniEmpleado;

                                var empleado = db.EmpleadoEmpresa.Where(x => x.Empleado.Cuil.Contains(busqueda)).FirstOrDefault();
                                if (empleado == null)
                                {
                                    errores.Add(new MessageVm()
                                    {
                                        Type = "alert-danger",
                                        Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", No existe empleado con dni/cuil: " + busqueda + ".",
                                        Dismissible = true
                                    });
                                    continue;
                                }

                                Afiliado afiliado = db.Afiliado.Where(x => x.IdEmpleadoEmpresa == empleado.idEmpleadoEmpresa).FirstOrDefault();
                                if(afiliado == null)
                                {
                                    afiliado = new Afiliado();
                                    //errores.Add(new MessageVm()
                                    //{
                                    //    Type = "alert-danger",
                                    //    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", El empleado con nombre: " + empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre + " y dni/cuil: " + busqueda + ", no esta cargado como afiliado.",
                                    //    Dismissible = true
                                    //});
                                    //continue;
                                }

                                //if(fechaAltaAfiliado < empleado.FechaAlta)
                                //{
                                //    errores.Add(new MessageVm()
                                //    {
                                //        Type = "alert-danger",
                                //        Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", La Fecha de alta del empleado con nombre: " + empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre + " y dni/cuil: " + busqueda + ", es menor a la fecha de alta del empleado.",
                                //        Dismissible = true
                                //    });
                                //    continue;
                                //}

                                if (fechaAltaAfiliado > DateTime.Today)
                                {
                                    errores.Add(new MessageVm()
                                    {
                                        Type = "alert-danger",
                                        Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", La Fecha de alta para el empleado con nombre: " + empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre + " y dni/cuil: " + busqueda + ", es mayor al dia de hoy .",
                                        Dismissible = true
                                    });
                                    continue;
                                }

                                if (afiliado.FechaBaja != null)
                                {
                                    errores.Add(new MessageVm()
                                    {
                                        Type = "alert-danger",
                                        Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", El empleado con dni/cuil: " + busqueda + ", esta de baja en el Sitio web xindocoweb.com.ar/secsanfrancisco.",
                                        Dismissible = true
                                    });
                                    continue;
                                }

                                empleado.EsAfiliado = true;
                                afiliado.FechaAlta = fechaAltaAfiliado;
                                afiliado.FechaBaja = null;
                                db.SaveChanges();

                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-success",
                                    Message = "La Fecha de alta para el empleado con nombre: " + empleado.Empleado.Apellido + ", " + empleado.Empleado.Nombre + " y dni/cuil: " + busqueda + ", fue actualizada correctamente. fecha Anterior: " + afiliado.FechaAlta.ToShortDateString() + ", fecha Nueva: " + fechaAltaAfiliado.ToShortDateString() + ".",
                                    Dismissible = true
                                });
                            }
                            else
                            {
                                errores.Add(new MessageVm()
                                {
                                    Type = "alert-danger",
                                    Message = "Error en linea: " + (rows.IndexOf(row) + 1) + ", Fecha de Alta para el empleado " + dniEmpleado + " no Valida.",
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

            ViewBag.afiliados = afiliados.Select(x => x.IdAfiliado).ToArray();
            ViewBag.ErroresAfiliados = errores;
            return View();
        }
    }
}