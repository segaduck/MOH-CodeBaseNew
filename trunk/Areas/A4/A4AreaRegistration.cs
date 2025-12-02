using System.Web.Mvc;

namespace EECOnline.Areas.A4
{
    public class A4AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "A4";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "A4_default",
                "A4/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}