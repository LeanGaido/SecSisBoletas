using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SecSisBoletas.Areas.Empresas.Controllers;
using System.IO;

namespace SecSisBoletas.Areas.Empresas.Reports
{
    public partial class ImpresionBoleta : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ReportViewer1.LocalReport.EnableExternalImages = true;
        }

        protected void Page_SaveStateComplete(object sender, EventArgs e)
        {
            ImageClickEventArgs a = new ImageClickEventArgs(1, 1);
            imgImprimir_Click(this, a);
        }

        protected void ReportViewer1_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            ReportViewer1.LocalReport.EnableExternalImages = true;

            int idBoleta = Convert.ToInt32(Session["IdBoleta"]);
            ImpresionBoletaController bc = new ImpresionBoletaController();
            ReportDataSource data = new ReportDataSource("dsBoleta", bc.GetBoletas(idBoleta));
            e.DataSources.Add(data);

        }

        protected void imgImprimir_Click(object sender, ImageClickEventArgs e)
        {
            Warning[] warnings;
            string[] streamids;
            string mimeType = "";
            string encoding = "";
            string extension = "";
            string deviceInfo = "";

            byte[] bytes = ReportViewer1.LocalReport.Render("Pdf", deviceInfo, out mimeType, out encoding, out extension, out streamids, out warnings);

            FileStream fs = new FileStream(Server.MapPath("~/App_Data/") + Session["IdBoleta"].ToString()
                            + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".pdf", FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();

            string fullpath = System.IO.Path.GetFullPath(Server.MapPath("~/App_Data/") + Session["IdBoleta"].ToString()
                            + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + ".pdf");

            string name = System.IO.Path.GetFileName(fullpath);
            string ext = System.IO.Path.GetExtension(fullpath);

            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Length", bytes.Length.ToString());
            Response.AddHeader("Content-Disposition", "attachment; filename=" + name);
            Response.BinaryWrite(bytes);
            Response.End();
            Response.Close();
        }
    }
}