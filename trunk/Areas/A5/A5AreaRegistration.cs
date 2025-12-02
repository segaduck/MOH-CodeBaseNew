using System.Web.Mvc;

namespace EECOnline.Areas.A5
{
    public class A5AreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "A5";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "A5_default",
                "A5/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}