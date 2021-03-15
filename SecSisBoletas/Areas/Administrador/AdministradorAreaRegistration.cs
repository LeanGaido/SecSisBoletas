using System.Web.Mvc;

namespace SecSisBoletas.Areas.Administrador
{
    public class AdministradorAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Administrador";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Administrador_default",
                "Administrador/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "SecSisBoletas.Areas.Administrador.Controllers" }
            );
        }
    }
}