using System.Web.Mvc;

namespace EECOnline.Areas.A1
{
    public class A1AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "A1";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "A1_default",
                "A1/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}