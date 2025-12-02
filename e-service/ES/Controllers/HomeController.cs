using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using System.Data.SqlClient;
using ES.Action;
using ES.Utils;

namespace ES.Controllers
{
    public class HomeController : BaseNoMemberController
    {
        /// <summary>
        /// 首頁
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Login");
        }

        /// <summary>
        /// 隱私權政策及安全宣告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Privacy()
        {
            return View();
        }
        /// <summary>
        /// 政府網站資料開放宣告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PublicPrivacy()
        {
            return View();
        }
        #region 舊程式碼
        ///// <summary>
        ///// 首頁
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult Index()
        //{
        //    if (DataUtils.GetConfig("HOST_STOP_MK").Equals("Y"))
        //    {
        //        return View("Stop");
        //    }

        //    ViewBag.tempMessage = TempData["tempMessage"];

        //    //logger.Debug("TEST");

        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();

        //        SetSearchCode(conn, 1);

        //        HomeAction action = new HomeAction(conn);
        //        ViewBag.List1 = action.GetServiceList(1); //醫事司
        //        ViewBag.List5 = action.GetServiceList(5); //中醫藥司
        //        ViewBag.List6 = action.GetServiceList(6); //國民年金監理會
        //        ViewBag.List7 = action.GetServiceList(7); //社會救助及社工司
        //        ViewBag.List8 = action.GetServiceList(8); //護理及健康照護司
        //        ViewBag.ListH = action.GetHotApplyList();
        //        ViewBag.ListM = action.GetMessageList();
        //    }

        //    return View();
        //}

        ///// <summary>
        ///// 登入頁
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult Login(string returnUrl)
        //{
        //    ViewBag.tempMessage = TempData["tempMessage"];

        //    //logger.Debug("returnUrl: " + returnUrl);

        //    //if (returnUrl != null && returnUrl.StartsWith("/Admin/"))
        //    if (returnUrl != null && returnUrl.StartsWith("/BACKMIN/"))
        //    {
        //        //return RedirectToAction("Index", "Login", new { area = "Admin", ReturnUrl = returnUrl });
        //        return RedirectToAction("Index", "Login", new { area = "BACKMIN", ReturnUrl = returnUrl });
        //    }

        //    return View();
        //}

        ///// <summary>
        ///// 主頁面按鈕瀏覽
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult LeftMenu()
        //{
        //    if (Session["LeftMenuList"] == null)
        //    {
        //        using (SqlConnection conn = GetConnection())
        //        {
        //            conn.Open();

        //            HomeAction action = new HomeAction(conn);
        //            Session.Add("LeftMenuList", action.GetLeftMenuList());
        //        }
        //    }

        //    ViewBag.List = Session["LeftMenuList"];

        //    return View();
        //}

        ///// <summary>
        ///// 帳號登入
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult LoginM()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// 憑證登入
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult LoginC()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// 憑證登入 (ActiveX)
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult LoginCF()
        //{
        //    return View();
        //}

        ///// <summary>
        ///// 登入後會員資訊
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult MemberInfo()
        //{
        //    return View();
        //}

        //public ActionResult Frame()
        //{
        //    return View();
        //}
        #endregion
    }
}
