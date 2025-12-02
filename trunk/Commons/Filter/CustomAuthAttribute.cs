using EECOnline.Models;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using Turbo.Commons;
using EECOnline.Models.Entities;

namespace EECOnline.Commons.Filter
{
    /// <summary>
    /// 同時判斷 登入狀態 及 角色執行權限 的 AuthorizeAttribute
    /// </summary>
    public class LoginRequired : AuthorizeAttribute
    {
        /// <summary>
        /// 預設系統登入頁
        /// </summary>
        public static string LOGIN_PAGE = "~/Login/C101M";
        /// <summary>
        /// 沒有權限的訊息頁面
        /// </summary>
        public static string UNAUTH_PAGE = "~/ErrorPage/UnAuth";

        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// web.config appSettings  設定是否停用 Authorize 權限檢核 (測試開發用):
        /// <para>2:  當系統管理者角色時停用, AuthorizeRequired 沒作用直接 bypass</para>
        /// <para>1:  全部停用, AuthorizeRequired 沒作用直接 bypass</para>
        /// <para>0:  未停用</para>
        /// </summary>
        private string disableAuth = System.Configuration.ConfigurationManager.AppSettings["DisableAuthorize"];

        /// <summary>
        /// LoginRequired 登入 Session 檢核 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string actionPath = ControllerContextHelper.GetActionPath(filterContext);
            string verb = filterContext.HttpContext.Request.HttpMethod;

            // 要把 actionPath 中的 action method 部份去掉
            // 只留下 Area/Controller 以用來比對 CLAMFUNCM.PRGID2
            string funcPath = actionPath;
            string[] tokens = actionPath.Split('/');
            if (tokens.Length >= 3)
            {
                funcPath = tokens[0] + "/" + tokens[1];
            }

            SessionModel sm = SessionModel.Get();
            sm.LastActionPath = actionPath;
            sm.LastActionFunc = null;

            ActionDescriptor desc = filterContext.ActionDescriptor;
            var allowAnonymous = desc.IsDefined(typeof(AllowAnonymousAttribute), true);
            var isByPassAuth = desc.IsDefined(typeof(BypassAuthorize), true)
                                || desc.ControllerDescriptor.IsDefined(typeof(BypassAuthorize), true);

            string userNo = (sm.UserInfo != null) ? sm.UserInfo.UserNo : null;
            LOG.Info("OnAuthorization[" + userNo + "] " + verb + " " + actionPath + " (allowAnonymous=" + allowAnonymous + ", isByPassAuth=" + isByPassAuth + ")");

            bool isLogin = false;
            if (sm.UserInfo != null
                && (sm.UserInfo.UserType.Equals(this.Roles)
                    || (string.IsNullOrEmpty(this.Roles)
                        && sm.UserInfo.UserType.Equals(LoginUserType.SKILL_USER)))
                )
            {
                isLogin = true;
            }

            if (!isLogin)
            {
                // 未登入
                if (!allowAnonymous)
                {
                    // 根據 LoginRequired.Roles 決定, 登入頁面
                    string loginPage = LOGIN_PAGE;
                    LOG.Info("OnAuthorization: redirect to Login page: " + loginPage);

                    //測試 醫事系統連結登入後重導，目前僅抓apy_id、GUID，其餘參數畫面皆顯示操作異常
                    filterContext.Controller.TempData["RedirectPath"] = actionPath; 
                    filterContext.Controller.TempData["RedirectApyId"] = HttpContext.Current.Request.QueryString["apy_id"];
                    filterContext.Controller.TempData["RedirectGUID"] = HttpContext.Current.Request.QueryString["GUID"];

                    filterContext.Result = new RedirectResult(loginPage);
                }
            }
            else
            {
                // 比對系統 TblCLAMFUNCM.PRGID2 以取得功能名稱

                // 取得系統中已啟用的全部 Action Function 定義
                // 以比對取得當前 action path 對應的 功能名稱 並記錄在 SessionModel.LastActionFunc 
                // 比對範圍: 以 area/controller 為準, 同一個 controller 下不再區分子功能
                IList<TblAMFUNCM> allFuncs = ApplicationModel.GetClamFuncsAll();
                sm.LastActionFunc = null;
                for (int i = 0; i < allFuncs.Count; i++)
                {
                    TblAMFUNCM item = allFuncs[i];
                    if (funcPath.Equals(item.prgid))
                    {
                        /* 找到 action 對應功能, 
                         * 包括同一Controller中的相關子功能, 
                         * 例: AM/SE11/Index, AM/SE11/Modify 
                         *    都對應至 AM/SE11 這個功能
                         */
                        sm.LastActionFunc = item;
                        break;
                    }
                }

                // 權限檢核
                if (sm.LastActionFunc == null && !isByPassAuth)
                {
                    LOG.Warn("功能路徑 [" + funcPath + "] 找不到 CLAMFUNCM 定義");
                }

                if (!isByPassAuth)
                {
                    // 角色權限檢核
                    IList<ClamRoleFunc> funcs = sm.RoleFuncs;
                    bool isAuth = false;
                    if (funcs != null)
                    {
                        TblAMFUNCM func = sm.LastActionFunc;
                        for (int i = 0; func != null && i < funcs.Count; i++)
                        {
                            ClamRoleFunc item = funcs[i];
                            if (item.sysid.Equals(func.sysid)
                                && item.modules.Equals(func.modules)
                                && item.submodules.Equals(func.submodules)
                                )
                            {
                                isAuth = true;
                                break;
                            }
                        }
                    }

                    if (!isAuth)
                    {
                        if ("1".Equals(disableAuth)
                            || ("2".Equals(disableAuth) && ConfigModel.Admin.Equals(sm.UserInfo.UserNo)))
                        {
                            //停用權限檢核
                            LOG.Info("OnAuthorization[" + sm.UserInfo.UserNo + "] " + verb + " " + sm.LastActionPath + ", -- AUTHORIZATION CHECK DISABLED --");
                        }
                        else
                        {
                            // 使用者試圖執行未授權的 Action, 導向 UnAuth 頁面
                            LOG.Info("UNAUTHORIZED [" + sm.UserInfo.UserNo + "] " + verb + " " + sm.LastActionPath + ", redirect to UNAUTH_PAGE page");
                            string funcName = (sm.LastActionFunc != null) ? " " + sm.LastActionFunc.prgname + "(" + sm.LastActionFunc.prgid + ") " : "";
                            sm.LastErrorMessage = "您沒有執行" + funcName + "權限!";
                            filterContext.Result = new RedirectResult(UNAUTH_PAGE);

                            //TODO: 寫入未授權執行記錄
                        }

                    }
                }
            }

            //base.OnAuthorization(filterContext);
            return;
        }
    }

    /// <summary>
    /// 用來標示特定 controlle 或 action 將略過權限檢核
    /// <para>例如: /Login/Role 不應該進行權限檢核</para>
    /// </summary>
    public class BypassAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // 不作任何動作, 直接 return
            // 重要!! 不然會回應 401.0 - Unauthorized
            return;
        }
    }
}