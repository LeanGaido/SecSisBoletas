using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.ViewModels;
using DAL.Models;
using DAL;
using Zen.Barcode;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Reporting.WebForms;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    public class ImpresionBoletaController : Controller
    {
        private SecModel db = new SecModel();
        private static int idDeclaracion;
        private static string banco;

        public RedirectResult ImpresionBoleta(int idBoleta, string _banco)
        {
            banco = _banco;
            Session["IdBoleta"] = idBoleta;
            Session["Banco"] = banco;
            if(banco == "5525")
            {
                return Redirect("~/Areas/Empresas/Reports/ImpresionBoleta.aspx");
            }
            else if(banco == "2354")
            {
                return Redirect("~/Areas/Empresas/Reports/ImpresionBoletaBcoHip.aspx");
            }
            return Redirect("https://xindicoweb.com.ar/SecSanFrancisco");
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<Boleta> GetBoletas(int id)
        {
            List<Boleta> boletas = new List<Boleta>();
            var boletaAux = db.BoletaAportes.Where(x => x.IdBoleta == id).FirstOrDefault();
            if (boletaAux != null)
            {
                Boleta boletaNueva = new Boleta();
                boletaNueva.Banco = banco;

                idDeclaracion = boletaAux.IdDeclaracionJurada;
                DeclaracionJurada ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == idDeclaracion).FirstOrDefault();
                Empresa empresa = db.Empresa.Where(x => x.IdEmpresa == ddjj.idEmpresa).FirstOrDefault();
                Localidad localidad = db.Localidad.Where(x => x.IdLocalidad == empresa.IdLocalidad).FirstOrDefault();

                boletaNueva.RazonSocial = empresa.RazonSocial;
                boletaNueva.Cuit = empresa.Cuit;
                boletaNueva.Domicilio = empresa.Calle + " " + empresa.Altura.ToString();
                boletaNueva.CodPostal = localidad.CodPostal.ToString();
                boletaNueva.Localidad = localidad.Nombre;
                string telefono = (empresa.TelefonoFijo != null) ? empresa.TelefonoFijo.ToString() : "";
                if (telefono == ""){ telefono = (empresa.TelefonoCelular != null) ? empresa.TelefonoCelular.ToString() : ""; }
                boletaNueva.Telefono = (empresa.TelefonoFijo != null) ? empresa.TelefonoFijo.ToString() : "";
                boletaNueva.Mes = ddjj.mes.ToString();
                boletaNueva.Anio = ddjj.anio.ToString();
                DateTime fechaVencimiento = boletaAux.FechaVencimiento;
                boletaNueva.FechaVencimiento = fechaVencimiento.ToShortDateString();
                boletaNueva.RecargoPorMora = (boletaAux.RecargoMora == null) ? "0" : boletaAux.RecargoMora.ToString();

                var empleados = db.DetalleDeclaracionJurada.Where(x => x.IdDeclaracionJurada == idDeclaracion).ToList();

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
                boletaNueva.CantEmpleados = count2.ToString();
                boletaNueva.TotalSueldos = sueldos2.ToString();
                /*Math.Truncate(550.847457 * 100) / 100*/
                boletaNueva.DosPorc = (Math.Truncate(((sueldos2 / 100) * 2) * 100) / 100).ToString();

                boletaNueva.CantAfiliados = count5.ToString();
                boletaNueva.TotalSueldosAfiliados = sueldos5.ToString();
                /*Math.Truncate(550.847457 * 100) / 100*/
                boletaNueva.CincoPorc = (Math.Truncate(((sueldos5 / 100) * 5) * 100) / 100).ToString();
                boletaNueva.Total = (Convert.ToDouble(boletaNueva.UnPorcFamiliaresACargo) + Convert.ToDouble(boletaNueva.RecargoPorMora)).ToString();
                boletaNueva.TotalDepositado = (Convert.ToDouble(boletaNueva.DosPorc) + Convert.ToDouble(boletaNueva.CincoPorc) + Convert.ToDouble(boletaNueva.Total)).ToString();

                string totalDepositadoEntero = boletaNueva.TotalDepositado.Split(new Char[] { ',', '.' })[0];
                string totalDepositadoDecimal = (boletaNueva.TotalDepositado.Split(new Char[] { ',', '.' }).Count() > 1) ? boletaNueva.TotalDepositado.Split(new Char[] { ',', '.' })[1] : "00";

                boletaNueva.CodBarra = boletaNueva.Banco + idDeclaracion.ToString().PadLeft(8, '0') + fechaVencimiento.DayOfYear.ToString().PadLeft(3, '0') + (fechaVencimiento.Year - 2000).ToString().PadLeft(2, '0') + boletaNueva.Anio.PadLeft(4, '0') + boletaNueva.Mes.PadLeft(2, '0') + totalDepositadoEntero.PadLeft(6, '0') + totalDepositadoDecimal.PadLeft(2, '0');

                int nv = 0;
                int resto = 0;
                int resto1 = 0;
                string digi2;

                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(0, 1)) * 7);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(1, 1)) * 6);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(2, 1)) * 5);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(3, 1)) * 4);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(4, 1)) * 3);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(5, 1)) * 2);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(6, 1)) * 7);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(7, 1)) * 6);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(8, 1)) * 5);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(9, 1)) * 4);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(10, 1)) * 3);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(11, 1)) * 2);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(12, 1)) * 7);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(13, 1)) * 6);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(14, 1)) * 5);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(15, 1)) * 4);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(16, 1)) * 3);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(17, 1)) * 2);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(18, 1)) * 7);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(19, 1)) * 6);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(20, 1)) * 5);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(21, 1)) * 4);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(22, 1)) * 3);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(23, 1)) * 2);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(24, 1)) * 7);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(25, 1)) * 6);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(26, 1)) * 5);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(27, 1)) * 4);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(28, 1)) * 3);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(29, 1)) * 2);
                nv = nv + (int.Parse(boletaNueva.CodBarra.Substring(30, 1)) * 7);

                resto = nv / 11;
                resto1 = 11 - (nv - (resto * 11));

                if (resto1 == 10)
                {
                    digi2 = "0";
                }
                else
                {
                    if (resto1 == 11)
                    {
                        digi2 = "1";
                    }
                    else
                    {
                        digi2 = resto1.ToString().Trim();
                    }
                }

                boletaNueva.CodBarra += digi2;
                boletaNueva.BarCode = creabarcode(boletaNueva.CodBarra);


                switch (boletaNueva.Mes)
                {
                    case "1":
                        boletaNueva.Mes = "Enero";
                        break;
                    case "2":
                        boletaNueva.Mes = "Febrero";
                        break;
                    case "3":
                        boletaNueva.Mes = "Marzo";
                        break;
                    case "4":
                        boletaNueva.Mes = "Abril";
                        break;
                    case "5":
                        boletaNueva.Mes = "Mayo";
                        break;
                    case "6":
                        boletaNueva.Mes = "Junio y SAC";
                        break;
                    case "7":
                        boletaNueva.Mes = "Julio";
                        break;
                    case "8":
                        boletaNueva.Mes = "Agosto";
                        break;
                    case "9":
                        boletaNueva.Mes = "Septiembre";
                        break;
                    case "10":
                        boletaNueva.Mes = "Octubre";
                        break;
                    case "11":
                        boletaNueva.Mes = "Noviembre";
                        break;
                    case "12":
                        boletaNueva.Mes = "Diciembre y SAC";
                        break;
                    default:
                        break;
                }
                boletas.Add(boletaNueva);

                return boletas.ToList();
            }

            //string cadena = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //using (var conn = new SqlConnection(cadena))
            //{
            //    bool hayDatos = false;
            //    Boleta tvm = new Boleta();
            //    tvm.Banco = banco;
                
            //    //Datos de la Boleta
            //    string commandTextTramite = @"SELECT * FROM tblBoletaAportes
            //            WHERE (IdBoleta = " + id + ")";
            //    using (var command = new SqlCommand(commandTextTramite, conn))
            //    {
            //        conn.Open();
            //        using (var dr = command.ExecuteReader())
            //        {
            //            while (dr.Read())
            //            {
            //                idDeclaracion = Convert.ToInt32(dr["IdDeclaracionJurada"]);
            //                DeclaracionJurada ddjj = db.DeclaracionJurada.Where(x => x.IdDeclaracionJurada == idDeclaracion).FirstOrDefault();
            //                Empresa empresa = db.Empresa.Where(x => x.IdEmpresa == ddjj.idEmpresa).FirstOrDefault();
            //                Localidad localidad = db.Localidad.Where(x => x.IdLocalidad == empresa.IdLocalidad).FirstOrDefault();

            //                tvm.RazonSocial = empresa.RazonSocial;
            //                tvm.Cuit = empresa.Cuit;
            //                tvm.Domicilio = empresa.Calle + " " + empresa.Altura.ToString();
            //                tvm.CodPostal = localidad.CodPostal.ToString();
            //                tvm.Localidad = localidad.Nombre;
            //                tvm.Telefono = (empresa.TelefonoFijo != null) ? empresa.TelefonoFijo.ToString() : "";
            //                tvm.Mes = ddjj.mes.ToString();
            //                tvm.Anio = ddjj.anio.ToString();
            //                DateTime fechaVencimiento = DateTime.Parse(dr["FechaVencimiento"].ToString());
            //                tvm.FechaVencimiento = fechaVencimiento.ToShortDateString();
            //                tvm.RecargoPorMora = (string.IsNullOrEmpty(dr["RecargoMora"].ToString()))? "0" : dr["RecargoMora"].ToString();
            //                hayDatos = true;
            //            }
            //        }
            //    }

            //    //string commandTextEmpleadosNoAfiliados = @"select dt.Sueldo
            //    //                                           from tblDetallesDeclaracionJurada as dt
            //    //                                            join tblDeclaracionesJuradas as dj on dt.IdDeclaracionJurada = dj.IdDeclaracionJurada
            //    //                                            join tblEmpresas as Empr on dj.idEmpresa = Empr.IdEmpresa
            //    //                                            join tblLocalidades as loc on Empr.IdLocalidad = loc.IdLocalidad
            //    //                                            join tblEmpleadosEmpresas as empEmp on dt.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa
            //    //                                            join tblEmpleados as emp on empEmp.idEmpleado = emp.IdEmpleado
            //    //                                           where dt.IdDeclaracionJurada = " + idDeclaracion + @"and dt.IdDetalleDeclaracionJurada not in (select dt.IdDetalleDeclaracionJurada
            //    //                                                                                                                      from tblDetallesDeclaracionJurada as dt
            //    //                                                                                                                          join tblDeclaracionesJuradas as dj on dt.IdDeclaracionJurada = dj.IdDeclaracionJurada
            //    //                                                                                                                          join tblEmpresas as Empr on dj.idEmpresa = Empr.IdEmpresa
            //    //                                                                                                                          join tblLocalidades as loc on Empr.IdLocalidad = loc.IdLocalidad
            //    //                                                                                                                          join tblEmpleadosEmpresas as empEmp on dt.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa
            //    //                                                                                                                          join tblEmpleados as emp on empEmp.idEmpleado = emp.IdEmpleado
            //    //                                                                                                                          join tblAfiliados as af on emp.idEmpleado = af.IdEmpleado
            //    //                                                                                                                      where af.FechaBaja is NULL)";
            //    //No Afiliados
            //    string commandTextEmpleadosNoAfiliados = @"select EmpEmp.IdJornada, dt.Sueldo, dt.SueldoBase
	           //                                             from tblDetallesDeclaracionJurada as dt
		          //                                              join tblEmpleadosEmpresas as EmpEmp on dt.IdEmpleadoEmpresa = EmpEmp.idEmpleadoEmpresa
	           //                                             where dt.IdDeclaracionJurada = " + idDeclaracion  +
            //                                                ";";

            //    using (var command = new SqlCommand(commandTextEmpleadosNoAfiliados, conn))
            //    {
            //        using (var dr = command.ExecuteReader())
            //        {
            //            int count = 0;
            //            double sueldos = 0;
            //            while (dr.Read())
            //            {
            //                sueldos += double.Parse(dr["Sueldo"].ToString());
            //                count++;
            //            }
            //            tvm.CantEmpleados = count.ToString();
            //            tvm.TotalSueldos = sueldos.ToString();
            //            /*Math.Truncate(550.847457 * 100) / 100*/
            //            tvm.DosPorc = (Math.Truncate(((sueldos / 100) * 2) * 100) / 100).ToString();
            //        }
            //    }

            //    //string commandTextEmpleadosAfiliados = @"select dt.Sueldo, af.CantFamiliaresACargo
            //    //                                         from tblDetallesDeclaracionJurada as dt
            //    //                                          join tblDeclaracionesJuradas as dj on dt.IdDeclaracionJurada = dj.IdDeclaracionJurada
            //    //                                          join tblEmpleadosEmpresas as empEmp on dt.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa
            //    //                                          join tblEmpleados as emp on empEmp.idEmpleado = emp.IdEmpleado
            //    //                                          join tblAfiliados as af on emp.idEmpleado = af.IdEmpleado
            //    //                                         where dt.IdDeclaracionJurada = " + idDeclaracion + @" and dt.IdDetalleDeclaracionJurada in (select dt.IdDetalleDeclaracionJurada
            //    //                                                                                                    from tblDetallesDeclaracionJurada as dt
            //    //                                                                                                     join tblDeclaracionesJuradas as dj on dt.IdDeclaracionJurada = dj.IdDeclaracionJurada
            //    //                                                                                                     join tblEmpleadosEmpresas as empEmp on dt.IdEmpleadoEmpresa = empEmp.idEmpleadoEmpresa
            //    //                                                                                                     join tblEmpleados as emp on empEmp.idEmpleado = emp.IdEmpleado
            //    //                                                                                                     join tblAfiliados as af on emp.idEmpleado = af.IdEmpleado)";
            //    //Afiliados
            //    //string commandTextEmpleadosAfiliados = @"select dt.Sueldo
            //    //                                            from tblDetallesDeclaracionJurada as dt
            //    //                                                join tblEmpleadosEmpresas as EmpEmp on dt.IdEmpleadoEmpresa = EmpEmp.idEmpleadoEmpresa
            //    //                                            where dt.IdDeclaracionJurada = " + idDeclaracion + " and EmpEmp.EsAfiliado = 1" +
            //    //                                            ";";
            //    string commandTextEmpleadosAfiliados = @"select EmpEmp.IdJornada, dt.Sueldo, dt.SueldoBase
	           //                                             from tblDetallesDeclaracionJurada as dt
		          //                                              join tblEmpleadosEmpresas as EmpEmp on dt.IdEmpleadoEmpresa = EmpEmp.idEmpleadoEmpresa
		          //                                              join tblAfiliados as af on EmpEmp.idEmpleado = af.IdEmpleado
	           //                                             where dt.IdDeclaracionJurada = " + idDeclaracion + " and " +
            //                                                      "EmpEmp.EsAfiliado = 1 and af.FechaBaja is null" + ";";

            //    using (var command = new SqlCommand(commandTextEmpleadosAfiliados, conn))
            //    {

            //        using (var dr = command.ExecuteReader())
            //        {
            //            int count = 0;
            //            double sueldos = 0;
            //            while (dr.Read())
            //            {
            //                int jornada = int.Parse(dr["IdJornada"].ToString());
            //                if (jornada == 1 || jornada == 2)
            //                {
            //                    double sueldoAux = double.Parse(dr["SueldoBase"].ToString());
            //                    if(sueldoAux > 0)
            //                    {
            //                        sueldos += double.Parse(dr["SueldoBase"].ToString());
            //                    }
            //                    else
            //                    {
            //                        sueldos += double.Parse(dr["Sueldo"].ToString());
            //                    }
            //                }
            //                else
            //                {
            //                    sueldos += double.Parse(dr["Sueldo"].ToString());
            //                }
            //                count++;
            //                hayDatos = true;
            //            }
            //            tvm.CantAfiliados = count.ToString();
            //            tvm.TotalSueldosAfiliados = sueldos.ToString();
            //            /*Math.Truncate(550.847457 * 100) / 100*/
            //            tvm.CincoPorc = (Math.Truncate(((sueldos / 100) * 5) * 100) / 100).ToString();
            //            tvm.Total = (Convert.ToDouble(tvm.UnPorcFamiliaresACargo) + Convert.ToDouble(tvm.RecargoPorMora)).ToString();
            //            tvm.TotalDepositado = (Convert.ToDouble(tvm.DosPorc) + Convert.ToDouble(tvm.CincoPorc) + Convert.ToDouble(tvm.Total)).ToString();
            //            //barra = "5525" + declaracionid + candp + anb + periodo + impt + dect + digi2;
            //            DateTime fechaVencimiento = DateTime.Parse(tvm.FechaVencimiento);
            //            string totalDepositadoEntero = tvm.TotalDepositado.Split(new Char[] { ',', '.' })[0];
            //            string totalDepositadoDecimal = (tvm.TotalDepositado.Split(new Char[] { ',', '.' }).Count() > 1)? tvm.TotalDepositado.Split(new Char[] { ',', '.' })[1] : "00";

            //            tvm.CodBarra = tvm.Banco + idDeclaracion.ToString().PadLeft(8, '0') + fechaVencimiento.DayOfYear.ToString().PadLeft(3, '0') + (fechaVencimiento.Year - 2000).ToString().PadLeft(2, '0') + tvm.Anio.PadLeft(4, '0') + tvm.Mes.PadLeft(2, '0') + totalDepositadoEntero.PadLeft(6, '0') + totalDepositadoDecimal.PadLeft(2, '0');

            //            int nv = 0;
            //            int resto = 0;
            //            int resto1 = 0;
            //            string digi2;

            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(0, 1)) * 7);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(1, 1)) * 6);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(2, 1)) * 5);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(3, 1)) * 4);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(4, 1)) * 3);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(5, 1)) * 2);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(6, 1)) * 7);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(7, 1)) * 6);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(8, 1)) * 5);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(9, 1)) * 4);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(10, 1)) * 3);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(11, 1)) * 2);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(12, 1)) * 7);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(13, 1)) * 6);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(14, 1)) * 5);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(15, 1)) * 4);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(16, 1)) * 3);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(17, 1)) * 2);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(18, 1)) * 7);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(19, 1)) * 6);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(20, 1)) * 5);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(21, 1)) * 4);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(22, 1)) * 3);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(23, 1)) * 2);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(24, 1)) * 7);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(25, 1)) * 6);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(26, 1)) * 5);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(27, 1)) * 4);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(28, 1)) * 3);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(29, 1)) * 2);
            //            nv = nv + (int.Parse(tvm.CodBarra.Substring(30, 1)) * 7);

            //            resto = nv / 11;
            //            resto1 = 11 - (nv - (resto * 11));

            //            if (resto1 == 10)
            //            {
            //                digi2 = "0";
            //            }
            //            else
            //            {
            //                if (resto1 == 11)
            //                {
            //                    digi2 = "1";
            //                }
            //                else
            //                {
            //                    digi2 = resto1.ToString().Trim();
            //                }
            //            }

            //            tvm.CodBarra += digi2;
            //            tvm.BarCode = creabarcode(tvm.CodBarra);

            //        }
            //    }

            //    if (hayDatos)
            //    {

            //        switch (tvm.Mes)
            //        {
            //            case "1":
            //                tvm.Mes = "Enero";
            //                break;
            //            case "2":
            //                tvm.Mes = "Febrero";
            //                break;
            //            case "3":
            //                tvm.Mes = "Marzo";
            //                break;
            //            case "4":
            //                tvm.Mes = "Abril";
            //                break;
            //            case "5":
            //                tvm.Mes = "Mayo";
            //                break;
            //            case "6":
            //                tvm.Mes = "Junio";
            //                break;
            //            case "7":
            //                tvm.Mes = "Julio";
            //                break;
            //            case "8":
            //                tvm.Mes = "Agosto";
            //                break;
            //            case "9":
            //                tvm.Mes = "Septiembre";
            //                break;
            //            case "10":
            //                tvm.Mes = "Octubre";
            //                break;
            //            case "11":
            //                tvm.Mes = "Noviembre";
            //                break;
            //            case "12":
            //                tvm.Mes = "Diciembre";
            //                break;
            //            default:
            //                break;
            //        }
            //        boletas.Add(tvm);
            //    }

                return boletas.ToList();
            //}
        }


        public RedirectResult ImpresionBoletaEspecial(int idBoletaEspecial, string _banco)
        {
            banco = _banco;
            Session["IdBoletaEspecial"] = idBoletaEspecial;
            Session["Banco"] = banco;
            if (banco == "5525")
            {
                return Redirect("~/Areas/Empresas/Reports/ImpresionBoletaEspecial.aspx");
            }
            else if (banco == "2354")
            {
                return Redirect("~/Areas/Empresas/Reports/ImpresionBoletaEspecial.aspx");
            }
            return Redirect("https://xindicoweb.com.ar/SecSanFrancisco");
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<BoletaEspecial> GetBoletaEspecial(int id)
        {
            List<BoletaEspecial> boletas = new List<BoletaEspecial>();
            var boletaAux = db.BoletaAportesEspeciales.Where(x => x.IdBoleta == id).FirstOrDefault();
            if (boletaAux != null)
            {
                BoletaEspecial boletaNueva = new BoletaEspecial();
                boletaNueva.Banco = banco;

                boletaNueva.RazonSocial = boletaAux.RazonSocial;
                boletaNueva.Cuit = boletaAux.Cuit;
                boletaNueva.Domicilio = boletaAux.Calle + " " + boletaAux.Altura.ToString();
                boletaNueva.CodPostal = boletaAux.CodPostal.ToString();
                boletaNueva.Localidad = boletaAux.Localidad;
                string telefono = (boletaAux.TelefonoFijo != null) ? boletaAux.TelefonoFijo.ToString() : "";
                if (telefono == "") { telefono = (boletaAux.TelefonoCelular != null) ? boletaAux.TelefonoCelular.ToString() : ""; }
                boletaNueva.Telefono = telefono;

                boletaNueva.Periodo = boletaAux.Periodo;

                boletaNueva.CantEmpleados = boletaAux.CantEmpleados.ToString();
                boletaNueva.TotalSueldos = boletaAux.TotalSueldos2.ToString();
                boletaNueva.DosPorc = boletaAux.Aportes2.ToString();

                boletaNueva.CantAfiliados = boletaAux.CantAfiliados.ToString();
                boletaNueva.TotalSueldosAfiliados = boletaAux.TotalSueldos5.ToString();
                boletaNueva.CincoPorc = boletaAux.Aportes5.ToString();

                boletaNueva.FechaVencimiento = boletaAux.FechaVencimiento.ToShortDateString();
                boletaNueva.RecargoPorMora = (boletaAux.RecargoMora == null) ? "0" : boletaAux.RecargoMora.ToString();
                boletaNueva.Total = boletaAux.Total.ToString();
                boletaNueva.TotalDepositado = boletaAux.TotalDepositado.ToString();

                boletas.Add(boletaNueva);
            }

            return boletas.ToList();
        }


        public byte[] creabarcode(String barcode)
        {
            BarcodeMetrics tamccbb = new BarcodeMetrics(2, 90);
            System.Drawing.Image imagen;

            imagen = BarcodeDrawFactory.GetSymbology(BarcodeSymbology.Code128).Draw(barcode, tamccbb);

            ImageFormat format = ImageFormat.Bmp;

            MemoryStream mm = new MemoryStream();
            imagen.Save(mm, format);
            imagen.Dispose();

            byte[] bytearray = mm.ToArray();
            mm.Close();
            mm.Dispose();

            return bytearray;
        }
    }
}