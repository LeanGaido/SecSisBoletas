using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SecSisBoletas.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.IsInRole("Empresa"))
            {
                return RedirectToAction("Index", "Home", new { area = "Empresas" });
            }
            else if (User.IsInRole("Admin") || User.IsInRole("Fiscalizacion") || User.IsInRole("Finanzas") || User.IsInRole("Auxiliar"))
            {
                return RedirectToAction("Index", "Home", new { area = "Administrador" });
            }
            return RedirectToAction("Index", "Home", new { area = "Empresas" });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            if (User.IsInRole("Empresa"))
            {
                return RedirectToAction("Index", "Home", new { area = "Empresas" });
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home", new { area = "Administrador" });
            }
            return RedirectToAction("Index", "Home", new { area = "Empresas" });
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            if (User.IsInRole("Empresa"))
            {
                return RedirectToAction("Index", "Home", new { area = "Empresas" });
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home", new { area = "Administrador" });
            }
            return RedirectToAction("Index", "Home", new { area = "Empresas" });
        }
    }
}