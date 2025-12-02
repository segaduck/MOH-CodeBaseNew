using System.Web.Mvc;

namespace ES.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "BACKMIN";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "BACKMIN_default",
                "BACKMIN/{controller}/{action}/{id}",
                //new { action = "Index", id = UrlParameter.Optional }
                new { controller = "Main", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
