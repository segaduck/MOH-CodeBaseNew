using System.Web.Mvc;

namespace EECOnline.Areas.A6
{
    public class A6AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "A6";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "A6_default",
                "A6/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}