using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using System.Web.Security;
using System.Data.SqlClient;
using ES.Areas.Admin.Utils;
using ES.Areas.Admin.Action;
using log4net;
using ES.Models;
using ES.DataLayers;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MainController : BaseController
    {
        /// <summary>
        /// 管理頁首頁
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            if (!Request.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Login");
            }
            AccountModel account = GetAccountModel();
            ViewBag.Account = GetAccount();
            Dictionary<string, object> data = new Dictionary<string, object>();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                MainAction action = new MainAction(conn);
                data = action.GetData(account.Account);
                conn.Close();
                conn.Dispose();
            }
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                MainAction action = new MainAction(conn);
                ViewBag.LastLoginTime = "您是第一次進入本系統";
                bool fg_show = true;
                if (data["LAST_LOGIN_TIME"] == null || string.IsNullOrEmpty(data["LAST_LOGIN_TIME"].ToString()) || Convert.ToString(data["LAST_LOGIN_TIME"]).Length == 0) { fg_show = false; }
                if (fg_show)
                {
                    ViewBag.LastLoginTime = string.Format("上次進入時間：{0}", ((DateTime)data["LAST_LOGIN_TIME"]).ToString("yyyy/MM/dd HH:mm:ss"));
                }
                if (account.LevelList.Contains(143)) // 是否有分文權限
                {
                    ViewBag.List = action.GetList(account.Scope, account.ServiceUnitCode, Int32.Parse(account.UnitCode), sm.UserInfo.Admin.ACC_NO);
                }
                conn.Close();
                conn.Dispose();
            }
            if (sm.UserInfo != null && sm.UserInfo.UserNo.Contains("cdccovid"))
            {
                return RedirectToAction("Index", "FlyPay", new { area = "BACKMIN" });
            }
            BackApplyDAO backDao = new BackApplyDAO();
            MainModel model = new MainModel();
            model = backDao.GetLatestMessages();
            model.barChartData = backDao.GetBarChartData(account.UnitCode).Replace("STAT_MONTH", "name").Replace("CASE_COUNT", "value");
            model.pieChartData = backDao.GetPieChartData(account.UnitCode).Replace("SERVICE_NAME", "label").Replace("CASE_COUNT", "count").Replace("SERVICE_STATUS", "enabled");
            model.VisitRecords = this.GetVisitRecord();

            return View(model);
        }

        /// <summary>
        /// 申辦情形統計
        /// </summary>
        /// <returns></returns>
        public ActionResult Menu()
        {
            List<Dictionary<String, Object>> list = null;

            int parentId = 0;

            if (Request.Form["id"] != null)
            {
                parentId = Int32.Parse(Request.Form["id"]);
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                list = CodeUtils.GetMenuList(conn, parentId, GetAccount());
                conn.Close();
                conn.Dispose();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CaseSearch()
        {
            SessionModel sm = SessionModel.Get();
            AccountModel account = GetAccountModel();
            MainModel model = new MainModel();
            BackApplyDAO backDao = new BackApplyDAO();
            model.START_DATE_AC = DateTime.Now.ToString("yyy/01/01");
            model.END_DATE_AC = DateTime.Now.AddMonths(1).AddDays(-DateTime.Now.AddMonths(1).Day).ToString("yyyy/MM/dd");

            model.CaseList = backDao.GetCaseList(account.UnitCode, DateTime.Now.ToString("yyy0101"), DateTime.Now.AddMonths(1).AddDays(-DateTime.Now.AddMonths(1).Day).ToString("yyyyMMdd"));

            return View("CaseSearch", model);

        }

        public ActionResult CaseSearchQuery(MainModel model)
        {

            SessionModel sm = SessionModel.Get();
            AccountModel account = GetAccountModel();
            BackApplyDAO backDao = new BackApplyDAO();

            if (model.START_DATE_AC != null)
            {
                string start_date = model.START_DATE_AC;
                string end_date = model.END_DATE_AC;
                model.START_DATE_AC = model.START_DATE_AC.Replace("/", "");
                model.END_DATE_AC = model.END_DATE_AC.Replace("/", "");

                model.CaseList = backDao.GetCaseList(account.UnitCode, model.START_DATE_AC, model.END_DATE_AC);
                model.START_DATE_AC = start_date;
                model.END_DATE_AC = end_date;
                return View("CaseSearch", model);
            }
            else
            {
                return CaseSearch();
            }


        }

        public ActionResult CaseSearchDetail(string ym)
        {
            SessionModel sm = SessionModel.Get();
            AccountModel account = GetAccountModel();
            BackApplyDAO backDao = new BackApplyDAO();
            MainModel model = new MainModel();
            ym = (Convert.ToInt32(ym.Replace("/", "")) + 191100).ToString();
            model.CaseList = backDao.GetCaseDetailList(account.UnitCode, ym);

            return View(model);
        }


    }
}
