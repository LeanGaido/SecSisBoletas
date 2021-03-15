using System.Web.Mvc;

namespace SecSisBoletas.Areas.Empresas
{
    public class EmpresasAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Empresas";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Empresas_default",
                "Empresas/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "SecSisBoletas.Areas.Empresas.Controllers" }
            );
        }
    }
}