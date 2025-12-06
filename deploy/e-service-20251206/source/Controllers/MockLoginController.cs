using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Dapper;

namespace ES.Controllers
{
    /// <summary>
    /// Mock Login Controller for e-service testing
    /// Allows admin to login as any member using either password or hashed password from database
    /// </summary>
    public class MockLoginController : Controller
    {
        protected static readonly ILog logger = LogUtils.GetLogger();

        /// <summary>
        /// Check if Mock Login is enabled (reads from Web.config appSettings)
        /// </summary>
        private bool IsMockLoginEnabled()
        {
            string devMode = ConfigurationManager.AppSettings["DevMode"] ?? "";
            string mockLoginEnabled = ConfigurationManager.AppSettings["DevMode_AllowMockLogin"] ?? "";
            return devMode == "1" && mockLoginEnabled == "1";
        }

        /// <summary>
        /// Mock Login Page - GET
        /// </summary>
        public ActionResult Index()
        {
            if (!IsMockLoginEnabled())
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        /// <summary>
        /// Mock Login - POST
        /// Accepts either plain password or hashed password (from database)
        /// </summary>
        [HttpPost]
        public ActionResult MockLogin(string userNo, string userPwd)
        {
            var result = new AjaxResultStruct();
            result.status = false;

            // Check if Mock Login is enabled
            if (!IsMockLoginEnabled())
            {
                result.message = "Mock Login is disabled. Please enable DevMode and DevMode_AllowMockLogin in Web.config.";
                return Content(result.Serialize(), "application/json");
            }

            if (string.IsNullOrEmpty(userNo))
            {
                result.message = "請輸入帳號";
                return Content(result.Serialize(), "application/json");
            }

            if (string.IsNullOrEmpty(userPwd))
            {
                result.message = "請輸入密碼或密碼雜湊值";
                return Content(result.Serialize(), "application/json");
            }

            try
            {
                LoginDAO dao = new LoginDAO();
                LoginUserInfo userInfo = null;

                // First try: use input as plain password (hash it first)
                string hashedPwd = DataUtils.Crypt256(userPwd);
                userInfo = dao.LoginValidate(userNo, hashedPwd);

                // Second try: use input directly as hashed password (from database)
                if (!userInfo.LoginSuccess)
                {
                    userInfo = dao.LoginValidate(userNo, userPwd);
                }

                if (!userInfo.LoginSuccess)
                {
                    result.message = "帳號或密碼/雜湊值錯誤，請確認輸入的密碼或資料庫中的 PSWD 欄位值";
                    return Content(result.Serialize(), "application/json");
                }

                // Set login info
                userInfo.LoginAuth = "MOCK_LOGIN";
                userInfo.LoginIP = HttpContext.Request.UserHostAddress;

                // Save to session
                SessionModel sm = SessionModel.Get();
                sm.UserInfo = userInfo;

                // Log mock login
                LogMockLogin(userInfo);

                result.status = true;
                result.message = $"Mock 登入成功！歡迎 {userInfo.Member.NAME}";
            }
            catch (Exception ex)
            {
                logger.Error("MockLogin failed: " + ex.Message, ex);
                result.message = "Mock 登入失敗: " + ex.Message;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// Search members for Mock Login
        /// </summary>
        [HttpPost]
        public ActionResult SearchMembers(string keyword)
        {
            var result = new AjaxResultStruct();
            result.status = false;

            if (!IsMockLoginEnabled())
            {
                result.message = "Mock Login is disabled.";
                return Content(result.Serialize(), "application/json");
            }

            if (string.IsNullOrEmpty(keyword) || keyword.Length < 2)
            {
                result.message = "請輸入至少2個字元進行搜尋";
                return Content(result.Serialize(), "application/json");
            }

            try
            {
                string sql = @"
                    SELECT TOP 20 
                        ACC_NO, 
                        IDN, 
                        NAME, 
                        MAIL, 
                        (SELECT COUNT(*) FROM APPLY WHERE APPLY.ACC_NO = MEMBER.ACC_NO) AS CaseCount
                    FROM MEMBER 
                    WHERE ISNULL(DEL_MK, 'N') = 'N'
                      AND (ACC_NO LIKE @keyword 
                           OR IDN LIKE @keyword 
                           OR NAME LIKE @keyword
                           OR MAIL LIKE @keyword)
                    ORDER BY 
                        (SELECT COUNT(*) FROM APPLY WHERE APPLY.ACC_NO = MEMBER.ACC_NO) DESC,
                        ACC_NO";

                var parameters = new DynamicParameters();
                parameters.Add("@keyword", "%" + keyword + "%");

                List<MemberSearchResult> members;
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    members = conn.Query<MemberSearchResult>(sql, parameters).ToList();
                    conn.Close();
                }

                result.status = true;
                result.data = members;
                result.message = $"找到 {members.Count} 筆會員資料";
            }
            catch (Exception ex)
            {
                logger.Error("SearchMembers failed: " + ex.Message, ex);
                result.message = "搜尋失敗: " + ex.Message;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// Get top members with most cases for testing
        /// </summary>
        [HttpPost]
        public ActionResult GetTopMembers()
        {
            var result = new AjaxResultStruct();
            result.status = false;

            if (!IsMockLoginEnabled())
            {
                result.message = "Mock Login is disabled.";
                return Content(result.Serialize(), "application/json");
            }

            try
            {
                string sql = @"
                    SELECT TOP 10 
                        m.ACC_NO, 
                        m.IDN, 
                        m.NAME, 
                        m.MAIL, 
                        COUNT(a.APP_ID) AS CaseCount
                    FROM MEMBER m
                    LEFT JOIN APPLY a ON m.ACC_NO = a.ACC_NO
                    WHERE ISNULL(m.DEL_MK, 'N') = 'N'
                    GROUP BY m.ACC_NO, m.IDN, m.NAME, m.MAIL
                    ORDER BY COUNT(a.APP_ID) DESC";

                List<MemberSearchResult> members;
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    members = conn.Query<MemberSearchResult>(sql).ToList();
                    conn.Close();
                }

                result.status = true;
                result.data = members;
                result.message = $"找到 {members.Count} 筆會員資料";
            }
            catch (Exception ex)
            {
                logger.Error("GetTopMembers failed: " + ex.Message, ex);
                result.message = "查詢失敗: " + ex.Message;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// Log mock login for audit
        /// </summary>
        private void LogMockLogin(LoginUserInfo userInfo)
        {
            try
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        LoginDAO dao = new LoginDAO();
                        dao.Tran(conn, tran);

                        TblLOGIN_LOG llog = new TblLOGIN_LOG();
                        llog.LOGIN_ID = userInfo.UserNo;
                        llog.LOGIN_TIME = DateTime.Now;
                        llog.NAME = userInfo.Member?.NAME ?? "MOCK";
                        llog.UNIT_CD = 0;
                        llog.IP_ADDR = userInfo.LoginIP;
                        llog.STATUS = "M"; // M for Mock Login
                        llog.FAIL_COUNT = 0;

                        dao.Insert(llog);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("LogMockLogin failed: " + ex.Message, ex);
                        tran.Rollback();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("LogMockLogin connection failed: " + ex.Message, ex);
            }
        }
    }

    /// <summary>
    /// Member search result model for Mock Login
    /// </summary>
    public class MemberSearchResult
    {
        public string ACC_NO { get; set; }
        public string IDN { get; set; }
        public string NAME { get; set; }
        public string MAIL { get; set; }
        public int CaseCount { get; set; }
    }
}
