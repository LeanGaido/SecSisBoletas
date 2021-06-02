using DAL;
using DAL.Models;
using DAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    public class NotificacionesController : Controller
    {
        private SecModel db = new SecModel();

        // GET: Empresas/Notificaciones
        public ActionResult Index()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            List<VmListadoNotificaciones> notificaciones = (from oNotificaciones in db.Notificaciones
                                                          where oNotificaciones.idEmpresa == IdEmpresa
                                                          select new VmListadoNotificaciones
                                                          {
                                                              ID = oNotificaciones.IdNotificacion,
                                                              EmpresaId = oNotificaciones.idEmpresa,
                                                              Fecha = oNotificaciones.Fecha,
                                                              Titulo = oNotificaciones.Titulo,
                                                              Visto = oNotificaciones.Visto,
                                                              FechaVisto = oNotificaciones.FechaVisto
                                                          }).ToList();

            return View(notificaciones);
        }

        public ActionResult Detalle(int Id)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            Notificacion notificacion = db.Notificaciones.Where(x => x.IdNotificacion == Id && x.idEmpresa == IdEmpresa).FirstOrDefault();

            if(notificacion == null)
            {
                return HttpNotFound();
            }

            notificacion.Visto = true;
            notificacion.FechaVisto = DateTime.Now;

            db.SaveChanges();

            notificacion.ListadoAdjuntos = db.AdjuntosNotificacion.Where(x => x.idNotificacion == notificacion.IdNotificacion).ToList();

            return View(notificacion);
        }

        public JsonResult GetNotificaciones()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            List<VmPreviewNotificacion> notificaciones = (from oNotificaciones in db.Notificaciones
                                                          where oNotificaciones.idEmpresa == IdEmpresa && !oNotificaciones.Visto
                                                          select new VmPreviewNotificacion
                                                          {
                                                              ID = oNotificaciones.IdNotificacion,
                                                              EmpresaId = oNotificaciones.idEmpresa,
                                                              FechaAux = oNotificaciones.Fecha,
                                                              Titulo = oNotificaciones.Titulo
                                                          }).ToList();

            notificaciones.ForEach(s => s.Fecha = s.FechaAux.ToString("dd/MM/yyyy HH:mm"));

            return Json(notificaciones, JsonRequestBehavior.AllowGet);
        }
    }
}