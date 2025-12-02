using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using System.Data.SqlClient;
using ES.Utils;
using ES.Models;
using ES.Action;
using System.Web.Security;

namespace ES.Filter
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private static readonly ILog logger = LogUtils.GetLogger();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                if (filterContext.HttpContext.User.IsInRole("Member"))
                {
                    if (filterContext.HttpContext.Session["Member"] == null)
                    {
                        using (SqlConnection conn = DataUtils.GetConnection())
                        {
                            FormsIdentity id = (FormsIdentity)System.Web.HttpContext.Current.User.Identity;
                            FormsAuthenticationTicket ticket = id.Ticket;
                            Dictionary<string, string> userData = ES.Utils.DataUtils.JsonStringToDictionary(ticket.UserData);

                            conn.Open();
                            MemberAction action = new MemberAction(conn);
                            MemberModel member = action.GetMember(userData["MemberAccount"]);
                            filterContext.HttpContext.Session.Add("Member", member);
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                }
                
                if (filterContext.HttpContext.User.IsInRole("Admin"))
                {
                }
            }
        }
    }
}