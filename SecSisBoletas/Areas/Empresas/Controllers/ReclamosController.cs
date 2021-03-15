using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DAL;
using DAL.Models;
using SecSisBoletas.Models.Utilities;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class ReclamosController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Empresas/Reclamos
        public ActionResult Index()
        {
            return View(db.Reclamo.ToList().OrderByDescending(x => x.IdReclamo));
        }

        // GET: Empresas/Reclamos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamo.Find(id);
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            return View(reclamo);
        }

        // GET: Empresas/Reclamos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Empresas/Reclamos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IdReclamo,Nombre,Empresa,Telefono,Email,Mensaje")] Reclamo reclamo)
        {
            if (ModelState.IsValid)
            {
                db.Reclamo.Add(reclamo);
                db.SaveChanges();
                //db.Reclamo.Add(new Reclamo()
                //{
                //    Nombre = Nombre,
                //    Empresa = Empresa,
                //    Telefono = Telefono,
                //    Email = Email,
                //    Mensaje = Mensaje
                //});
                //db.SaveChanges();

                var emailBody = "<b>Nombre</b>: " + reclamo.Nombre + "<br/>" +
                                "<b>Empresa</b>: " + reclamo.Empresa + "<br/>" +
                                "<b>Telefono</b>: " + reclamo.Telefono + "<br/>" +
                                "<b>Email</b>: " + reclamo.Email + "<br/>" +
                                "<b>Mensaje</b>: <br/>" + reclamo.Mensaje + "<br/>";

                //Configuring webMail class to send emails  
                //gmail smtp server  
                WebMail.SmtpServer = "mail.xindicoweb.com.ar";
                //gmail port to send emails  
                WebMail.SmtpPort = 587;
                WebMail.SmtpUseDefaultCredentials = true;
                //sending emails with secure protocol  
                //WebMail.EnableSsl = true;
                //EmailId used to send emails from application  
                WebMail.UserName = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";
                WebMail.Password = "B0L3ZC1ho9vMepG";

                //Sender email address.  
                WebMail.From = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";

                //Send email  
                WebMail.Send(to: "reclamos@xindicoweb.com.ar", subject: "Reclamo - XindicoWeb", body: emailBody, isBodyHtml: true);

                //respuesta.resultado = true;
                //respuesta.mensaje = "El Reclamo se envio correctamente.";

                /*reclamos@secsanfrancisco.org.ar*/

                return RedirectToAction("ReclamoEnviado");
            }

            return View(reclamo);
        }

        public ActionResult ReclamoEnviado()
        {
            return View();
        }

        //public JsonResult EnviarReclamo(string Nombre, string Empresa, string Telefono, string Email, string Mensaje)
        //{
        //    RespuestaJson respuesta = new RespuestaJson(false, "Ocurrio un Error, por Favor intente mas tarde");
        //    try
        //    {
        //        db.Reclamo.Add(new Reclamo()
        //        {
        //            Nombre = Nombre,
        //            Empresa = Empresa,
        //            Telefono = Telefono,
        //            Email = Email,
        //            Mensaje = Mensaje
        //        });
        //        db.SaveChanges();

        //        var emailBody = "<b>Nombre</b>: " + Nombre + "<br/>" +
        //                        "<b>Empresa</b>: " + Empresa + "<br/>" +
        //                        "<b>Telefono</b>: " + Telefono + "<br/>" +
        //                        "<b>Email</b>: " + Email + "<br/>" +
        //                        "<b>Mensaje</b>: <br/>" + Mensaje + "<br/>";

        //        //Configuring webMail class to send emails  
        //        //gmail smtp server  
        //        WebMail.SmtpServer = "mail.xindicoweb.com.ar";
        //        //gmail port to send emails  
        //        WebMail.SmtpPort = 587;
        //        WebMail.SmtpUseDefaultCredentials = true;
        //        //sending emails with secure protocol  
        //        //WebMail.EnableSsl = true;
        //        //EmailId used to send emails from application  
        //        WebMail.UserName = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";
        //        WebMail.Password = "B0L3ZC1ho9vMepG";

        //        //Sender email address.  
        //        WebMail.From = "no-reply@xindicoweb.com.ar";//"no-responder@xindicoweb.com.ar";

        //        //Send email  
        //        WebMail.Send(to: "reclamos@xindicoweb.com.ar", subject: "Reclamo - XindicoWeb", body: emailBody, isBodyHtml: true);

        //        respuesta.resultado = true;
        //        respuesta.mensaje = "El Reclamo se envio correctamente.";
        //    }
        //    catch (Exception)
        //    {

        //    }


        //    return Json(respuesta, JsonRequestBehavior.AllowGet);
        //}

        // GET: Empresas/Reclamos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamo.Find(id);
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            return View(reclamo);
        }

        // POST: Empresas/Reclamos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IdReclamo,Nombre,Empresa,Telefono,Email,Mensaje")] Reclamo reclamo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reclamo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(reclamo);
        }

        // GET: Empresas/Reclamos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reclamo reclamo = db.Reclamo.Find(id);
            if (reclamo == null)
            {
                return HttpNotFound();
            }
            return View(reclamo);
        }

        // POST: Empresas/Reclamos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reclamo reclamo = db.Reclamo.Find(id);
            db.Reclamo.Remove(reclamo);
            db.SaveChanges();
            return RedirectToAction("Index");
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
