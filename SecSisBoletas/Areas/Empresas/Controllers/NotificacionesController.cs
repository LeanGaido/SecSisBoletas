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
                                                            join oNotificacionesEmpresa in db.NotificacionesEmpresa on oNotificaciones.IdNotificacion equals oNotificacionesEmpresa.IdNotificacion
                                                            where oNotificacionesEmpresa.idEmpresa == IdEmpresa
                                                            select new VmListadoNotificaciones
                                                            {
                                                                ID = oNotificaciones.IdNotificacion,
                                                                EmpresaId = oNotificacionesEmpresa.idEmpresa,
                                                                Fecha = oNotificaciones.Fecha,
                                                                Titulo = oNotificaciones.Titulo,
                                                                Visto = oNotificacionesEmpresa.Visto,
                                                                FechaVisto = oNotificacionesEmpresa.FechaVisto
                                                            }).OrderByDescending(x => x.Fecha).ToList();

            return View(notificaciones);
        }

        public ActionResult Detalle(int Id)
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            VmNotificacion notificacion = (from oNotificaciones in db.Notificaciones
                                           join oNotificacionesEmpresa in db.NotificacionesEmpresa on oNotificaciones.IdNotificacion equals oNotificacionesEmpresa.IdNotificacion
                                           where oNotificaciones.IdNotificacion == Id && oNotificacionesEmpresa.idEmpresa == IdEmpresa
                                           select new VmNotificacion
                                           {
                                               idNotificacion = oNotificaciones.IdNotificacion,
                                               idEmpresa = oNotificacionesEmpresa.idEmpresa,
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

            NotificacionEmpresa notificacionEmpresa = db.NotificacionesEmpresa.Where(x => x.IdNotificacion == notificacion.idNotificacion && x.idEmpresa == notificacion.idEmpresa).FirstOrDefault();

            notificacionEmpresa.Visto = true;
            notificacionEmpresa.FechaVisto = DateTime.Now;

            db.SaveChanges();

            notificacion.ListadoAdjuntos = db.AdjuntosNotificacion.Where(x => x.idNotificacion == notificacion.idNotificacion).ToList();

            return View(notificacion);
        }

        public JsonResult GetNotificaciones()
        {
            var claim = ((ClaimsIdentity)User.Identity).FindFirst("IdEmpresa");
            int IdEmpresa = Convert.ToInt32(claim.Value);

            List<VmPreviewNotificacion> notificaciones = (from oNotificaciones in db.Notificaciones
                                                          join oNotificacionesEmpresa in db.NotificacionesEmpresa on oNotificaciones.IdNotificacion equals oNotificacionesEmpresa.IdNotificacion
                                                          where oNotificacionesEmpresa.idEmpresa == IdEmpresa && !oNotificacionesEmpresa.Visto
                                                          select new VmPreviewNotificacion
                                                          {
                                                              ID = oNotificaciones.IdNotificacion,
                                                              EmpresaId = oNotificacionesEmpresa.idEmpresa,
                                                              FechaAux = oNotificaciones.Fecha,
                                                              Titulo = oNotificaciones.Titulo
                                                          }).ToList();

            notificaciones.ForEach(s => s.Fecha = s.FechaAux.ToString("dd/MM/yyyy HH:mm"));

            return Json(notificaciones, JsonRequestBehavior.AllowGet);
        }
    }
}