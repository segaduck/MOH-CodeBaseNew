using System.Web;
using System.Web.Mvc;
using ES.Filter;

namespace ES
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new CustomAuthorizeAttribute());
            filters.Add(new CustomHandleErrorAttribute());
        }
    }
}