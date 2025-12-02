using System.Web.Mvc;

namespace EECOnline.Areas.A8
{
    public class A8AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "A8";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "A8_default",
                "A8/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}