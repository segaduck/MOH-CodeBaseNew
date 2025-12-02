using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using log4net;
using log4net.Config;
using System.Web.Security;
using ES.Utils;
using ES.Models;

namespace ES.Controllers
{
    public class BaseNoMemberController : Controller
    {
        protected static readonly ILog logger = LogUtils.GetLogger();

        /// <summary>
        /// 取得資料庫連線
        /// </summary>
        /// <returns></returns>
        protected SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            return conn;
        }

        /// <summary>
        /// 取得資料庫連線
        /// </summary>
        /// <param name="key">連線名稱</param>
        /// <returns></returns>
        protected SqlConnection GetConnection(string key)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            return conn;
        }

        /// <summary>
        /// 取得帳號
        /// </summary>
        /// <returns></returns>
        protected string GetAccount()
        {
            if (Request == null) { return ""; }
            if (Request.IsAuthenticated)
            {
                FormsIdentity id = (FormsIdentity)System.Web.HttpContext.Current.User.Identity;
                FormsAuthenticationTicket ticket = id.Ticket;
                Dictionary<string, string> userData = ES.Utils.DataUtils.JsonStringToDictionary(ticket.UserData);
                if (userData.ContainsKey("MemberAccount")) { return userData["MemberAccount"]; }
            }
            else
            {
                if (!String.IsNullOrEmpty(Request["TestAccount"])) return Request["TestAccount"];
            }
            return "";
        }

        protected Dictionary<string, string> GetUserData()
        {
            if (Request.IsAuthenticated)
            {
                System.Web.Security.FormsIdentity id = (System.Web.Security.FormsIdentity)System.Web.HttpContext.Current.User.Identity;
                System.Web.Security.FormsAuthenticationTicket ticket = id.Ticket;
                Dictionary<string, string> userData = ES.Utils.DataUtils.JsonStringToDictionary(ticket.UserData);
                return userData;
            }
            return null;
        }

        /// <summary>
        /// 取得上傳檔案集合
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, HttpPostedFileBase> GetHttpPostedFileDictionary()
        {
            Dictionary<string, HttpPostedFileBase> item = new Dictionary<string, HttpPostedFileBase>();

            HttpPostedFileBase uploadFile = null;

            foreach (string file in Request.Files)
            {
                uploadFile = Request.Files[file] as HttpPostedFileBase;

                if (uploadFile != null)
                {
                    item.Add(file, uploadFile);
                }
            }

            return item;
        }

        protected void SetSearchCode(SqlConnection conn, int serialNo)
        {
            Dictionary<string, string> item = CodeUtils.GetSearchCode(conn, serialNo);
            ViewBag.DCSubject = item["TITLE"];
            ViewBag.DCType = item["TITLE"];
            ViewBag.DCTheme = item["CLS_SUB_CD"];
            ViewBag.DCCake = item["CLS_ADM_CD"];
            ViewBag.DCService = item["CLS_SRV_CD"];
            ViewBag.DCKeywords = item["KEYWORD"];
        }

        protected string GetClientIP()
        {
            string rst = "::1";
            if (Request == null) { return rst; }
            try
            {
                //判斷client端是否有設定代理伺服器
                if (Request.ServerVariables["HTTP_VIA"] == null)
                {
                    rst = Request.ServerVariables["REMOTE_ADDR"].ToString();
                }
                else
                {
                    rst = Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                rst = Request.UserHostAddress;
            }
            return rst;
        }

        /// <summary>
        /// 每個 action 被執行前會觸發這個 event
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string controllerName = filterContext.Controller.GetType().Name;
            string actionName = filterContext.ActionDescriptor.ActionName;
        }
    }
}
