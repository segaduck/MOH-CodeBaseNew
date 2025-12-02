using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using System.Web.Security;
using System.Web.Configuration;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;

namespace ES.Areas.Admin.Controllers
{
    public class LogoutController : BaseController
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                Dictionary<string, string> userData = GetUserData();
                List<string> roles = ES.Utils.DataUtils.StringToList(userData["Roles"]);

                // LoginLog
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    LoginAction action = new LoginAction(conn);
                    action.InsertLoginLog(GetAccount(), GetClientIP(), "O");
                    conn.Close();
                    conn.Dispose();
                }

                roles.Remove("Admin");

                if (roles.Count == 0)
                {
                    // 清除表單驗證Cookie
                    FormsAuthentication.SignOut();

                    // 清除Session資料
                    Session.Clear();
                }
                else
                {
                    // 將管理者登入的 Cookie 設定成 Session Cookie
                    bool isPersistent = ES.Utils.DataUtils.GetConfig("LOGIN_PERSISTENT_MK").Equals("Y");
                    int timeout = Int32.Parse(ES.Utils.DataUtils.GetConfig("LOGIN_TIMEOUT"));

                    userData.Remove("AdminAccount");
                    userData["Roles"] = ES.Utils.DataUtils.StringArrayToString(roles.ToArray(), ",");

                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userData["Id"], DateTime.Now, DateTime.Now.AddMinutes(timeout), isPersistent, ES.Utils.DataUtils.DictionaryToJsonString(userData));

                    string encTicket = FormsAuthentication.Encrypt(ticket);

                    HttpCookie cookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
                    if (cookie == null)
                    {
                        cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
                    }
                    if (isPersistent)
                    {
                        cookie.Expires = ticket.Expiration;
                    }
                    cookie.Value = encTicket;
                    HttpContext.Response.AppendCookie(cookie);
                }
            }

            return RedirectToAction("Index", "Login");
        }
    }
}
