using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using System.IO;
using GemBox.Spreadsheet;
using WebUI.CustomClass;
using System.Data;
using System.Web.Routing;
using ES.Commons;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 6;
        }

        #region 申辦統計
        [HttpGet]
        public ActionResult CaseSum()
        {
            CaseSumModel model = new CaseSumModel();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";

            this.SetVisitRecord("Report", "CaseSum", "申辦統計");

            return View(model);
        }

        [HttpPost]
        public ActionResult CaseSum(CaseSumModel model)
        {
            ViewBag.CallFunc = "classpage();";
            return View(model);
        }

        public ActionResult CaseSumReport(string Sdate,string Fdate)
        {
            CaseSumModel model = new CaseSumModel();
            model.Fdate = Fdate;
            model.Sdate = Sdate;
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetCaseSumReport(model);
            return View(model);
        }
        #endregion

        #region 登入統計
        [HttpGet]
        public ActionResult StatisticsLogin()
        {
            StatisticsLoginModel model = new StatisticsLoginModel();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");

            this.SetVisitRecord("Report", "StatisticsLogin", "登入統計");

            return View(model);
        }

        [HttpPost]
        public ActionResult StatisticsLogin(StatisticsLoginModel model)
        {
            ReportAction action = new ReportAction();
            ViewBag.Map = action.GetLoginReport(model);
            return View(model);
        }
        #endregion

        #region 熱門申辦統計
        [HttpGet]
        public ActionResult StatisticsHot()
        {
            StatisticsHotModel model = new StatisticsHotModel();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");

            this.SetVisitRecord("Report", "StatisticsHot", "熱門申辦統計");

            return View(model);
        }

        [HttpPost]
        public ActionResult StatisticsHot(StatisticsHotModel model)
        {
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetHotReport(model);
            return View(model);
        }
        #endregion

        #region 書表下載統計
        [HttpGet]
        public ActionResult StatisticsFile()
        {
            StatisticsFileModel model = new StatisticsFileModel();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");

            this.SetVisitRecord("Report", "StatisticsFile", "書表下載統計");

            return View(model);
        }

        [HttpPost]
        public ActionResult StatisticsFile(StatisticsFileModel model)
        {
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetFileReport(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult StatisticsFileDetail(StatisticsFileModel model)
        {
            string sc_id = String.IsNullOrEmpty(Request.Form["SC_ID"]) ? "" : Request.Form["SC_ID"].ToString();
            string sname = String.IsNullOrEmpty(Request.Form["SNAME"]) ? "" : Request.Form["SNAME"].ToString();
            string uname = String.IsNullOrEmpty(Request.Form["UNAME"]) ? "" : Request.Form["UNAME"].ToString();
            string counter = String.IsNullOrEmpty(Request.Form["COUNTER"]) ? "" : Request.Form["COUNTER"].ToString();
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetFileDetailReport(model, sc_id);
            ViewBag.SNAME = sname;
            ViewBag.UNAME = uname;
            ViewBag.COUNTER = counter;
            return View(model);
        }
        #endregion

        #region 繳費方式統計
        [HttpGet]
        public ActionResult StatisticsPaytype()
        {
            StatisticsPaytypeModel model = new StatisticsPaytypeModel();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");

            this.SetVisitRecord("Report", "StatisticsPaytype", "繳費方式統計");

            return View(model);
        }

        [HttpPost]
         public ActionResult StatisticsPaytype(StatisticsPaytypeModel model)
        {
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetPaytypeReport(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult StatisticsPaytypeDetail(StatisticsPaytypeModel model)
        {
            string sc_id = String.IsNullOrEmpty(Request.Form["SC_ID"]) ? "" : Request.Form["SC_ID"].ToString();
            string name = String.IsNullOrEmpty(Request.Form["NAME"]) ? "" : Request.Form["NAME"].ToString();
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetPaytypeDetailReport(model, sc_id);
            ViewBag.NAME = name;
            return View(model);
        }
        #endregion

        #region 滿意度報表
        [HttpGet]
        public ActionResult SatisfactionScores()
        {
            SatisfactionScoresModel model = new SatisfactionScoresModel();
            model.Rbl_Table = "y";
            model.Sdate = DateTime.Now.AddYears(-1911).ToString("yyy/MM");
            model.Fdate = DateTime.Now.AddYears(-1911).ToString("yyy/MM");

            this.SetVisitRecord("Report", "SatisfactionScores", "滿意度報表");

            return View(model);
        }

        [HttpPost]
        public ActionResult GetExcel(SatisfactionScoresModel model)
        {
            ReportAction action = new ReportAction();
            String bulidSelectSql = "";
            switch (model.Rbl_Table)
            {
                case "y":
                    bulidSelectSql = "SURVEY_ONLINE";
                    break;
                case "n":
                    bulidSelectSql = "SURVEY_OFFLINE";
                    break;
                case "all":
                    bulidSelectSql = "  ";
                    break;
            }
            DataTable dt = null;

            if (model.Table_Type == "1")
            {
                dt = action.CreateExcel(bulidSelectSql, model.Sdate.Remove(3,1).Trim(), model.Fdate.Remove(3,1).Trim());
            }
            else
            {
                dt = action.CreateExcel2(bulidSelectSql, model.Sdate.Remove(3,1).Trim(), model.Fdate.Remove(3,1).Trim());
            }

            // 產生 Excel 資料流。
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt) as MemoryStream;

            return File(ms, "application/unknown", "Download.xls");
        }
        #endregion

        #region 資料驗證函式
        public ActionResult CheckDateIsOver(DateModel model)
        {
            Boolean result = false;
            DateTime sdate = Convert.ToDateTime(model.Sdate);
            DateTime fdate = Convert.ToDateTime(model.Fdate);
            if (fdate.CompareTo(sdate) >= 0)
            {
                result = true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 專科社會工作師
        [HttpGet]
        public ActionResult SocialWorker1()
        {
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";

            this.SetVisitRecord("Report", "SocialWorker1", "專科社會工作師證書清冊");

            return View(model);
        }
        [HttpPost]
        public ActionResult SocialWorker1(SocialWorker model)
        {
            DataTable dt = null;
            ReportAction action = new ReportAction();
            dt = action.GetSocialWork1(model);
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, "專科社會工作師證書清冊");
            return File(ms, "application/unknown", "SocialWork1_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }
        [HttpGet]
        public ActionResult SocialWorker2()
        {
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";

            this.SetVisitRecord("Report", "SocialWorker2", "專科社會工作師分析");

            return View(model);
        }

        [HttpPost]
        public ActionResult SocialWorker2(SocialWorker model)
        {
            DataTable dt = null;
            ReportAction action = new ReportAction();
            dt = action.GetSocialWork2(model);
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, string.Format("申請日期起：{0}～申請日期迄：{1}", HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Sdate,"/")), HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Fdate,"/"))));
            return File(ms, "application/unknown", "SocialWork2_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }
        #endregion 專科社會工作師

        #region 社會工作實務經驗年資審查
        [HttpGet]
        public ActionResult SocialWorker3()
        {
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";

            this.SetVisitRecord("Report", "SocialWorker3", "社會工作實務經驗年資審查清冊");

            return View(model);
        }
        [HttpPost]
        public ActionResult SocialWorker3(SocialWorker model)
        {
            DataTable dt = null;
            ReportAction action = new ReportAction();
            dt = action.GetSocialWork3(model);
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, "社會工作實務經驗年資審查清冊 "+ string.Format("申請日期起：{0}～申請日期迄：{1}", HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Sdate, "/")), HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Fdate, "/"))));
            return File(ms, "application/unknown", "SocialWork3_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }
        #endregion 社會工作實務經驗年資審查

        #region 社會工作師證書匯出
        [HttpGet]
        public ActionResult SocialWorker4()
        {
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";

            this.SetVisitRecord("Report", "SocialWorker4", "社會工作師證書匯出");

            return View(model);
        }

        [HttpPost]
        public ActionResult SocialWorker4(SocialWorker model)
        {
            DataTable dt = null;
            ReportAction action = new ReportAction();
            dt = action.GetSocialWork4(model);
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, string.Format("申請日期起：{0}～申請日期迄：{1}", HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Sdate, "/")), HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Fdate, "/"))));
            return File(ms, "application/unknown", "SocialWork4_"+DateTime.Now.ToString("yyyyMMddHHmmss") +".xls");
        }
        #endregion 社會工作師證書匯出

        #region 全國社會工作專業人員選拔推薦清冊匯出
        [HttpGet]
        public ActionResult SocialWorker5()
        {
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";

            this.SetVisitRecord("Report", "SocialWorker5", "全國社會工作專業人員選拔推薦清冊匯出");

            return View(model);
        }

        [HttpPost]
        public ActionResult SocialWorker5(SocialWorker model)
        {
            DataTable dt = null;
            ReportAction action = new ReportAction();
            dt = action.GetSocialWork5(model);
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, string.Format("申請日期起：{0}～申請日期迄：{1}", HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Sdate, "/")), HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Fdate, "/"))));
            return File(ms, "application/unknown", "SocialWork5_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }
        #endregion 全國社會工作專業人員選拔推薦清冊匯出

        #region 訴願案件申請清冊
        [HttpGet]
        public ActionResult SocialWorker6()
        {
            ReportAction action = new ReportAction();
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";
            model.queryModel.NowPage = 1;
            ViewBag.NowPage = 1;
            ViewBag.TotalCount = 0;
            ViewBag.TotalPage = 0;

            this.SetVisitRecord("Report", "SocialWorker6", "訴願案件申請清冊");

            return View(model);
        }
        [HttpPost]
        public ActionResult SocialWorker6(SocialWorker model)
        {
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetReport6(model);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.queryModel.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }
        //[HttpPost]
        //public ActionResult SocialWorker6(SocialWorker model)
        //{
        //    DataTable dt = null;
        //    ReportAction action = new ReportAction();
        //    dt = action.GetSocialWork6(model);
        //    MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, string.Format("申請日期起：{0}～申請日期迄：{1}", HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Sdate, "/")), HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Fdate, "/"))));
        //    return File(ms, "application/unknown", "SocialWork6_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        //}
        #endregion 訴願案件申請清冊

        #region 爭議案件申請清冊
        [HttpGet]
        public ActionResult SocialWorker7()
        {
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";
            model.queryModel.NowPage = 1;
            ViewBag.NowPage = 1;
            ViewBag.TotalCount = 0;
            ViewBag.TotalPage = 0;

            this.SetVisitRecord("Report", "SocialWorker7", "爭議案件申請清冊");

            return View(model);
        }
        [HttpPost]
        public ActionResult SocialWorker7(SocialWorker model)
        {
            ReportAction action = new ReportAction();
            ViewBag.List = action.GetReport7(model);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.queryModel.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }
        //[HttpPost]
        //public ActionResult SocialWorker7(SocialWorker model)
        //{
        //    DataTable dt = null;
        //    ReportAction action = new ReportAction();
        //    dt = action.GetSocialWork7(model);
        //    MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, string.Format("申請日期起：{0}～申請日期迄：{1}", HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Sdate, "/")), HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Fdate, "/"))));
        //    return File(ms, "application/unknown", "SocialWork7_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        //}
        #endregion 爭議案件申請清冊

        #region 年金案件申請清冊
        [HttpGet]
        public ActionResult SocialWorker8()
        {
            SocialWorker model = new SocialWorker();
            model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.CallFunc = "";

            this.SetVisitRecord("Report", "SocialWorker8", "年金案件申請清冊");

            return View(model);
        }

        [HttpPost]
        public ActionResult SocialWorker8(SocialWorker model)
        {
            DataTable dt = null;
            ReportAction action = new ReportAction();
            dt = action.GetSocialWork8(model);
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt, string.Format("申請日期起：{0}～申請日期迄：{1}", HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Sdate, "/")), HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Fdate, "/"))));
            return File(ms, "application/unknown", "SocialWork8_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
        }
        #endregion 爭議案件申請清冊
    }
}
