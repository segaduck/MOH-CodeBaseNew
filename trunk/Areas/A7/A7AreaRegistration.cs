using System.Web.Mvc;

namespace EECOnline.Areas.A7
{
    public class A7AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "A7";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "A7_default",
                "A7/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}