using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;
using log4net;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;

namespace ES.Areas.Admin.Controllers
{
    [HandleError(View = "Error")]
    public class BaseController : Controller
    {
        protected static readonly ILog logger = ES.Utils.LogUtils.GetLogger();
        private static readonly string CACHE_FORM_ACTION = "Index";
        private static readonly int VISIT_RECORD = 10;

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
            if (Request.IsAuthenticated)
            {
                System.Web.Security.FormsIdentity id = (System.Web.Security.FormsIdentity)System.Web.HttpContext.Current.User.Identity;
                System.Web.Security.FormsAuthenticationTicket ticket = id.Ticket;
                Dictionary<string, string> userData = ES.Utils.DataUtils.JsonStringToDictionary(ticket.UserData);

                if (userData.ContainsKey("AdminAccount"))
                {
                    return userData["AdminAccount"];
                }
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
        /// 取得登入者帳號資料
        /// </summary>
        /// <returns></returns>
        protected AccountModel GetAccountModel()
        {
            AccountModel model = null;

            if (!String.IsNullOrEmpty(GetAccount()))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    AccountAction action = new AccountAction(conn);
                    model = action.GetAccountModel(GetAccount());
                    conn.Close();
                    conn.Dispose();
                }
            }

            return model;
        }

        protected string GetClientIP()
        {
            try
            {
                if(Request == null)
                {
                    return "Request is null. Hangfire is using.";
                }
                //判斷client端是否有設定代理伺服器
                if (Request.ServerVariables["HTTP_VIA"] == null)
                {
                    return Request.ServerVariables["REMOTE_ADDR"].ToString();
                }
                else
                {
                    return Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                return Request.UserHostAddress;
            }
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

            var statusId = Request.QueryString["statusId"].TONotNullString();

            SessionModel sm = SessionModel.Get();
            try
            {
                sm.statusId = statusId;

                if (sm.UserInfo == null || sm.UserInfo.Admin == null)
                {
                    filterContext.Result = Redirect("/BACKMIN/Login");
                }
            }
            catch (Exception)
            {
                filterContext.Result = Redirect("/BACKMIN/Login");
            }


        }

        protected void SetVisitRecord(string controllerName,string actionName,string appName)
        {
            SessionModel sm = SessionModel.Get();
            List<VISIT_RECORDModel> visistList = GetVisitRecord();
            double timeStamp = 0.0d;
            Dictionary<double?,string> duplicateList = null;
            List<VISIT_RECORDModel> newVisistList = null;

            if (visistList.Count==10)
            {
                duplicateList = new Dictionary<double?, string>();
                newVisistList = new List<VISIT_RECORDModel>();
                VISIT_RECORDModel oldestRecord = null;
                BackApplyDAO dao=new BackApplyDAO();

                timeStamp = DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                var query = visistList.Where(x => (double?)x.TAG > 0);
                double? visitTimeStamp = query.Any() ? query.Min(o => o.TAG) : 0.0d;
                oldestRecord = visistList.Where(x => (double?)x.TAG == visitTimeStamp).FirstOrDefault();

                if (oldestRecord != null)
                {
                    dao.VisitRecordDelete(oldestRecord);
                }

                visistList= GetVisitRecord();

                foreach (var record in visistList)
                {
                    if (record.APP_NAME == appName)
                    {
                        duplicateList.Add(record?.TAG, record.APP_NAME);
                    }
                }

                foreach (var record in visistList)
                {
                    if (duplicateList.Where(x => x.Key == record.TAG).Count() > 0)
                    {
                        continue;
                    }
                    else
                    {
                        record.ACC_NO = sm.UserInfo.UserNo;
                        record.IS_EXPIRED = false;
                        newVisistList.Add(record);
                    }

                }

                VISIT_RECORDModel newRecord = new VISIT_RECORDModel();
                dao = new BackApplyDAO();
                timeStamp = DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                newRecord.TAG = timeStamp;
                newRecord.CONTROL_NAME = controllerName;
                newRecord.ACTION_NAME = actionName;
                newRecord.APP_NAME = appName;
                newRecord.ACC_NO = sm.UserInfo.UserNo;
                newRecord.IS_EXPIRED = false;
                newVisistList.Add(newRecord);

                dao.VisitRecordUpdate(newVisistList, sm.UserInfo.UserNo);

            }
            else
            { 
                duplicateList = new Dictionary<double?, string>();
                newVisistList = new List<VISIT_RECORDModel>();

                foreach(var record in visistList)
                {
                    if (record.APP_NAME == appName)
                    {
                        duplicateList.Add(record?.TAG,record.APP_NAME);
                    }
                }

                foreach(var record in visistList)
                {
                    if (duplicateList.Where(x => x.Key== record.TAG).Count() > 0)
                    {
                        continue;
                    }
                    else
                    {
                        record.ACC_NO = sm.UserInfo.UserNo;
                        record.IS_EXPIRED = false;
                        newVisistList.Add(record);
                    }

                }

                VISIT_RECORDModel newRecord = new VISIT_RECORDModel();
                BackApplyDAO dao= new BackApplyDAO();
                timeStamp = DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                newRecord.TAG = timeStamp;
                newRecord.CONTROL_NAME = controllerName;
                newRecord.ACTION_NAME = actionName;
                newRecord.APP_NAME = appName;
                newRecord.ACC_NO = sm.UserInfo.UserNo;
                newRecord.IS_EXPIRED = false;
                newVisistList.Add(newRecord);

                dao.VisitRecordUpdate(newVisistList, sm.UserInfo.UserNo);

            }

        }

        protected List<VISIT_RECORDModel> GetVisitRecord()
        {            
            List<VISIT_RECORDModel> visistList = new List<VISIT_RECORDModel>();
            BackApplyDAO dao = new BackApplyDAO(); 
            SessionModel sm = SessionModel.Get();
            return dao.GetVidsitRecord(sm.UserInfo.UserNo);
        }


    }
}
