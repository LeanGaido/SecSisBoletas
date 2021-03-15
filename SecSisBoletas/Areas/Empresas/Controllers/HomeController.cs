using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SecSisBoletas.Areas.Empresas.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class HomeController : Controller
    {
        // GET: Empresas/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}