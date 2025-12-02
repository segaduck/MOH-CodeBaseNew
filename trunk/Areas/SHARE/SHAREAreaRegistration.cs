using System.Web.Mvc;

namespace EECOnline.Areas.SHARE
{
    public class SHAREAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SHARE";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SHARE_default",
                "SHARE/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}