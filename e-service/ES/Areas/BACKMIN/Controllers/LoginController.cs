using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.DataLayers;
using ES.Services;
//using ES.Models;
using ES.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace ES.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        protected static readonly ILog logger = LogUtils.GetLogger();
        /// <summary>
        /// 登入
        /// GET /Admin/Main/Login
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            return View();
        }

        /// <summary>
        /// 登入
        /// POST /Admin/Main/Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(LoginModel model, string returnUrl)
        {
            ES.Models.SessionModel sm = ES.Models.SessionModel.Get();
            bool flag_ValidateCode = false;
            string s_err2 = "";

            logger.Debug("LoginValidateCode:" + sm.LoginValidateCode);
            //logger.Debug("ES.Models.SessionModel.Get().LoginValidateCode:" + ES.Models.SessionModel.Get().LoginValidateCode);

            if (sm.LoginValidateCode == null || string.IsNullOrEmpty(sm.LoginValidateCode))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸出有誤";
            }
            else if (model.ValidateCode == null || string.IsNullOrEmpty(model.ValidateCode))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸入有誤";
            }
            else if (model.ValidateCode != sm.LoginValidateCode)
            {
                logger.DebugFormat("#model.ValidateCode: {0}", model.ValidateCode);
                logger.DebugFormat("#sm.LoginValidateCode: {0}", sm.LoginValidateCode);
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸入錯誤";
            }
            if (flag_ValidateCode && s_err2 != "")
            {
                // LoginLog
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn);//conn.Open();
                    LoginAction action = new LoginAction(conn);
                    action.InsertLoginLog(model.Account, GetClientIP(), "A");
                    DataUtils.CloseDbConn(conn);
                }
                TempData["tempMessage"] = s_err2;
                ViewBag.tempMessage = TempData["tempMessage"];
                logger.Debug("return View model");
                return View(model);
            }
            sm.LoginValidateCode = "";

            if (!ModelState.IsValid)
            {
                TempData["tempMessage"] = "欄位驗證錯誤";
                ViewBag.tempMessage = TempData["tempMessage"];
                logger.Debug("ModelState.IsValid:" + TempData["tempMessage"]);
                return View(model);
            }

            bool islock = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn);//conn.Open();
                AccountAction action = new AccountAction(conn);
                islock = action.AccountisLock(model.Account);
                DataUtils.CloseDbConn(conn);
            }

            if (islock)
            {
                var serviceTel = DataUtils.GetConfig("SERVICETEL"); // 系統操作服務諮詢電話
                TempData["tempMessage"] = $"鎖定15分鐘，請諮詢系統操作服務諮詢電話：{serviceTel}。";
                ViewBag.tempMessage = TempData["tempMessage"];
                return View(model);
            }

            //Amos 20190320
            //登入欄位驗證
            bool exists = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn);//conn.Open();
                AccountAction action = new AccountAction(conn);
                exists = action.AccountExists(model.Account);
                DataUtils.CloseDbConn(conn);
            }

            if (!exists)
            {
                //登入失敗 // LoginLog
                logger.Debug("登入失敗,帳號密碼不存在");
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn);//conn.Open();
                    LoginAction action = new LoginAction(conn);
                    action.InsertLoginLog(model.Account, GetClientIP(), "A");
                    DataUtils.CloseDbConn(conn);
                }
                TempData["tempMessage"] = "登入驗證錯誤";
                ViewBag.tempMessage = TempData["tempMessage"];
                return View(model);
            }

            bool stops = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn);//conn.Open();
                AccountAction action = new AccountAction(conn);
                stops = action.AccountStops(model.Account);
                DataUtils.CloseDbConn(conn);
            }

            if (stops)
            {
                //登入失敗 // LoginLog
                logger.Debug("登入失敗,帳號已停用");
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn);//conn.Open();
                    LoginAction action = new LoginAction(conn);
                    action.InsertLoginLog(model.Account, GetClientIP(), "A");
                    DataUtils.CloseDbConn(conn);
                }
                TempData["tempMessage"] = "登入驗證錯誤";
                ViewBag.tempMessage = TempData["tempMessage"];
                return View(model);
            }

            //登入
            bool flag_LoginAD = false;
            if (model.Account.Contains("cdccovid"))
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn);//conn.Open();
                    AccountAction action = new AccountAction(conn);
                    flag_LoginAD = action.CDCExists(model.Account, model.Password);
                    DataUtils.CloseDbConn(conn);
                }
            }
            else
            {
                ADUtils adu = new ADUtils();
                flag_LoginAD = adu.LoginAD(model.Account, model.Password);
            }

            if (!flag_LoginAD)
            {
                //登入失敗
                // LoginLog
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn);//conn.Open();
                    LoginAction action = new LoginAction(conn);
                    action.InsertLoginLog(model.Account, GetClientIP(), "A");
                    DataUtils.CloseDbConn(conn);
                }
                //TempData["tempMessage"] = adu.Errmessage;
                TempData["tempMessage"] = "登入欄位驗證錯誤";
                ViewBag.tempMessage = TempData["tempMessage"];
                return View(model);
            }

            //登入成功
            SetAuthCookie(model.Account);
            //加入Session
            //ES.Models.SessionModel sm = ES.Models.SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            ES.Models.LoginUserInfo userInfo = new ES.Models.LoginUserInfo();
            userInfo.LoginIP = HttpContext.Request.UserHostAddress;
            userInfo = dao.LoginInfo(model.Account);
            sm.UserInfo = userInfo;

            // LoginLog
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                DataUtils.OpenDbConn(conn);//conn.Open();
                LoginAction action = new LoginAction(conn);
                action.InsertLoginLog(model.Account, GetClientIP(), "S");
                DataUtils.CloseDbConn(conn);
            }

            if (String.IsNullOrEmpty(returnUrl)) { return RedirectToAction("Index", "Main", new { area = "BACKMIN" }); }
            return Redirect(returnUrl);
        }

        /// <summary>
        /// 設定登入Cookie
        /// </summary>
        /// <param name="account"></param>
        private void SetAuthCookie(String account)
        {
            // 將管理者登入的 Cookie 設定成 Session Cookie
            bool isPersistent = DataUtils.GetConfig("LOGIN_PERSISTENT_MK").Equals("Y");
            //Amos 20190320
            //int timeout = 30;
            int timeout = Int32.Parse(DataUtils.GetConfig("LOGIN_TIMEOUT"));
            Dictionary<string, string> userData = GetUserData();
            List<string> roles = null;

            if (userData == null)
            {
                userData = new Dictionary<string, string>();
                userData.Add("Roles", null);
                userData.Add("Id", DateTime.Now.ToString("yyyyMMddHHmmssffffff"));
                roles = new List<string>();
            }
            else
            {
                roles = ES.Utils.DataUtils.StringToList(userData["Roles"]);
            }

            if (userData.ContainsKey("AdminAccount"))
            {
                userData["AdminAccount"] = account;
            }
            else
            {
                userData.Add("AdminAccount", account);
                roles.Add("Admin");
            }

            userData["Roles"] = DataUtils.StringArrayToString(roles.ToArray(), ",");

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userData["Id"], DateTime.Now, DateTime.Now.AddMinutes(timeout), isPersistent, DataUtils.DictionaryToJsonString(userData));

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

        protected string GetClientIP()
        {
            try
            {
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
                logger.Debug("GetClientIP failed:" + e.TONotNullString());
                return Request.UserHostAddress;
            }
        }

        /// <summary>
        /// 圖型驗證碼轉語音撥放頁
        /// </summary>
        /// <returns></returns>
        public ActionResult VCodeAudio()
        {
            return View();
        }

        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCode()
        {
            ES.Models.SessionModel sm = ES.Models.SessionModel.Get();
            Commons.ValidateCode vc = new Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(4);
            sm.LoginValidateCode = vCode;
            logger.Debug(vCode);
            logger.Debug("sm.LoginValidateCode:" + sm.LoginValidateCode);
            logger.Debug("ES.Models.SessionModel.Get().LoginValidateCode:" + ES.Models.SessionModel.Get().LoginValidateCode);
            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        /// <summary>
        /// 將當前的驗證碼轉成 Wav audio 輸出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCodeAudio()
        {
            string vCode = ES.Models.SessionModel.Get().LoginValidateCode;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                ES.Commons.ValidateCode vc = new ES.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }

    }
}
