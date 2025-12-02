using System.Web.Mvc;

namespace EECOnline.Areas.A3
{
    public class A3AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "A3";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "A3_default",
                "A3/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}