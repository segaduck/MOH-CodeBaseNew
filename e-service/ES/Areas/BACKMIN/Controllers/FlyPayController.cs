using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WebUI.CustomClass;
using NPOI.Util;
using ES.DataLayers;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FlyPayController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 7;
        }

        [HttpGet]
        public ActionResult Index()
        {
            this.SetVisitRecord("FlyPay", "Index", "航班資料查詢");
            FlyPayModel model = new FlyPayModel();
            FlyPayAction action = new FlyPayAction();
            model.QRY_PAY_STATUSList = action.GetPAY_STATUS();
            model.QRY_BANKTYPEList = action.GetBANKTYPE();
            model.QRY_SECTIONList = action.GetSECTION();
            model.QRY_NEED_BACKList = action.GetNEEDBACK();
            return View(model);
        }
        /// <summary>
        /// 資料查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(FlyPayModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["tempMessage"] = "查詢驗證有誤!!";
                ViewBag.tempMessage = TempData["tempMessage"];
                return Index();
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);

                if (form.ProjectType == "2")
                {
                    form.grids = action.GetFlyPayBasicList(form);
                }
                else if (form.ProjectType == "11")
                {
                    form.grids = action.GetFlyPaySimList(form);
                }
                form.QRY_PAY_STATUSList = action.GetPAY_STATUS();
                form.QRY_BANKTYPEList = action.GetBANKTYPE();
                form.QRY_SECTIONList = action.GetSECTION();
                form.QRY_NEED_BACKList = action.GetNEEDBACK();

                conn.Close();
                conn.Dispose();
            }
            return View(form);
        }
        /// <summary>
        /// 編輯 菲律賓專案
        /// </summary>
        /// <param name="maindocno">護照號碼</param>
        /// <param name="flightno">航班代碼</param>
        /// <param name="flightdate">航班日期</param>
        /// <param name="edit"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(string maindocno, string flightno, string flightdate, string edit)
        {
            FlyPayModel model = new FlyPayModel();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                model.detail = action.GetFlyPayDetail(maindocno, flightno, flightdate);

                model.QRY_PAY_STATUSList = action.GetPAY_STATUS();

                model.QRY_CanEdit = false;
                if (!string.IsNullOrEmpty(edit) && edit.Equals("9")) { model.QRY_CanEdit = true; }

                conn.Close();
                conn.Dispose();
            }
            return View(model);
        }
        /// <summary>
        /// 編輯 防疫旅館專案
        /// </summary>
        /// <param name="flyid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditBasic(string flyid)
        {
            FlyPayModel model = new FlyPayModel();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                model.detail = action.GetFlyPayDetail(flyid);

                model.QRY_PAY_STATUSList = action.GetPAY_STATUS();
                model.QRY_BANKTYPEList = action.GetBANKTYPE();
                model.QRY_SECTIONList = action.GetSECTION();
                model.QRY_NEED_BACKList = action.GetNEEDBACK();

                model.QRY_CanEdit = false;

                conn.Close();
                conn.Dispose();
            }
            return View(model);
        }
        /// <summary>
        /// 編輯 簡易專案
        /// </summary>
        /// <param name="flyid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditSIM(string flyid)
        {
            FlyPayModel model = new FlyPayModel();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                model.detail = action.GetFlyPayDetailSIM(flyid);

                model.QRY_PAY_STATUSList = action.GetPAY_STATUS();
                model.QRY_BANKTYPEList = action.GetBANKTYPE();
                model.QRY_SECTIONList = action.GetSECTION();
                model.QRY_NEED_BACKList = action.GetNEEDBACK();

                model.QRY_CanEdit = false;

                conn.Close();
                conn.Dispose();
            }
            return View(model);
        }
        /// <summary>
        /// 編輯 菲律賓專案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(FlyPayModel model)
        {
            try
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    FlyPayAction action = new FlyPayAction(conn, tran);

                    int count = action.UpdateDetail(model, GetAccount());

                    tran.Commit();
                    //if (count > 0) { tran.Commit(); } else { tran.Rollback(); }
                    TempData["tempMessage"] = "存檔成功";
                    //model.detail = action.GetFlyPayDetail(model.detail.MAINDOCNO, model.detail.FLIGHTNO);

                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                TempData["tempMessage"] = "存檔失敗。";
                logger.Warn(ex.Message, ex);
            }
            ViewBag.tempMessage = TempData["tempMessage"];
            return View("Index", model);
        }
        /// <summary>
        /// 編輯 防疫旅館專案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditBasic(FlyPayModel model)
        {
            try
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    FlyPayAction action = new FlyPayAction(conn, tran);

                    int count = action.UpdateFlyPayBasicDetail(model, GetAccount());

                    tran.Commit();
                    //if (count > 0) { tran.Commit(); } else { tran.Rollback(); }
                    TempData["tempMessage"] = "存檔成功";
                    //model.detail = action.GetFlyPayDetail(model.detail.MAINDOCNO, model.detail.FLIGHTNO);

                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                TempData["tempMessage"] = "存檔失敗。";
                logger.Warn(ex.Message, ex);
            }
            ViewBag.tempMessage = TempData["tempMessage"];
            return View("Index", model);
        }
        /// <summary>
        /// 編輯 簡易專案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditSIM(FlyPayModel model)
        {
            try
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    FlyPayAction action = new FlyPayAction(conn, tran);

                    int count = action.UpdateFlyPayBasicDetailSIM(model, GetAccount());

                    tran.Commit();
                    TempData["tempMessage"] = "存檔成功";

                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                TempData["tempMessage"] = "存檔失敗。";
                logger.Warn(ex.Message, ex);
            }
            ViewBag.tempMessage = TempData["tempMessage"];
            return View("Index", model);
        }
        /// <summary>
        /// 還原GUID 防疫旅館專案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RESAVE(FlyPayModel model)
        {
            try
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    FlyPayAction action = new FlyPayAction(conn, tran);

                    int count = action.ClearFlyPayBasicGUID(model, GetAccount());

                    tran.Commit();
                    //if (count > 0) { tran.Commit(); } else { tran.Rollback(); }
                    TempData["tempMessage"] = "清空GUID成功";
                    //model.detail = action.GetFlyPayDetail(model.detail.MAINDOCNO, model.detail.FLIGHTNO);

                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                TempData["tempMessage"] = "清空失敗。";
                logger.Warn(ex.Message, ex);
            }
            ViewBag.tempMessage = TempData["tempMessage"];
            return View("Index", model);
        }

        #region 航班資料匯入

        [HttpPost]
        public ActionResult FlyImport(HttpPostedFileBase Text_UploadFile)
        {
            List<Map> list = null;
            string s_Upload_FileName = "";

            if (Text_UploadFile == null) { TempData["tempMessage"] = "請選擇檔案。"; }

            if (Text_UploadFile != null)
            {
                //MemoryStream ms = new MemoryStream();

                try
                {
                    if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                    {
                        DateTime now = DateTime.Now;

                        string dir = DataUtils.GetConfig("FOLDER_TEMPLATE") + now.Year.ToString("D4") + "\\" + now.Month.ToString("D2") + "\\";

                        string filename = Path.GetFileName(Request.Files[0].FileName);

                        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

                        s_Upload_FileName = string.Format("{0}{1}_{2}", dir, now.ToString("yyyyMMddHHmmss"), filename);

                        Request.Files[0].SaveAs(s_Upload_FileName);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }

                try
                {
                    // xlsx 開啟
                    NPOI.Util.FileInputStream in1 = new NPOI.Util.FileInputStream(Text_UploadFile.InputStream);
                    NPOI.HSSF.UserModel.HSSFWorkbook wb = new NPOI.HSSF.UserModel.HSSFWorkbook(in1);

                    list = FlyPayUtils.readEXCEL_backmin(wb);

                    using (SqlConnection conn = DataUtils.GetConnection())
                    {
                        conn.Open();

                        SqlTransaction tran = conn.BeginTransaction();

                        FlyPayAction action = new FlyPayAction(conn, tran);

                        Dictionary<string, object> ret = null;
                        ret = action.InsertFileBackmin(list, GetAccount());
                        int i_count = (int)ret["count"];
                        int i_errcnt = (int)ret["errcnt"];

                        tran.Commit();
                        //if (count > 0) { tran.Commit(); } else { tran.Rollback(); }
                        string s_msg2 = "";
                        //if (i_count != list.Count) { s_msg2 = "(重複的資料不再更新)"; }
                        if (i_errcnt > 0) { s_msg2 += string.Format("(失敗 {0}筆)", i_errcnt); }
                        TempData["tempMessage"] = string.Format("更新成功，總共 {0} 筆，成功 {1}筆。{2}", list.Count, i_count, s_msg2);

                        conn.Close();
                        conn.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    TempData["tempMessage"] = "更新失敗。";
                    if (list != null) { TempData["tempMessage"] = "更新失敗，總共 " + list.Count + " 筆。"; }
                    logger.Error(ex.Message, ex);
                }
                finally
                {
                    //ms.Dispose();
                    //ms.Close();
                }
            }
            ViewBag.tempMessage = TempData["tempMessage"];

            FlyPayModel model = new FlyPayModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                //SqlTransaction tran = conn.BeginTransaction();
                FlyPayAction action = new FlyPayAction(conn);

                model.QRY_PAY_STATUSList = action.GetPAY_STATUS();

                conn.Close();
                conn.Dispose();
            }
            return View("Index", model);
        }
        #endregion

        [HttpPost]
        public ActionResult ExportSIM(FlyPayModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["tempMessage"] = "查詢驗證有誤!!";
                ViewBag.tempMessage = TempData["tempMessage"];
                return Index();
            }

            FlyPayAction action = new FlyPayAction();
            DataTable dt = action.CreateExcelBasicSIM(form);

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelFlyPay(dt);

            return File(ms, "application/unknown", $"FlyPayBasicSIMList-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xls");
        }


        #region 防疫旅館繳費匯出

        [HttpPost]
        public ActionResult Export(FlyPayModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["tempMessage"] = "查詢驗證有誤!!";
                ViewBag.tempMessage = TempData["tempMessage"];
                return Index();
            }

            FlyPayAction action = new FlyPayAction();
            DataTable dt = action.CreateExcel(form);

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelFlyPay(dt);

            return File(ms, "application/unknown", $"FlyPayList-{DateTime.Now.ToString("yyyyMMdd-hhmmss")}.xls");
        }
        [HttpPost]
        public ActionResult ExportBasic(FlyPayModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["tempMessage"] = "查詢驗證有誤!!";
                ViewBag.tempMessage = TempData["tempMessage"];
                return Index();
            }

            FlyPayAction action = new FlyPayAction();
            DataTable dt = action.CreateExcelBasic(form);

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelFlyPay(dt);

            return File(ms, "application/unknown", $"FlyPayBasicList-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xls");
        }

        [HttpPost]
        public ActionResult ExportSPR(FlyPayModel form)
        {
            // 先給予春節流水號後統計
            FlySwipeController swControl = new FlySwipeController();
            swControl.SetSPRNum();
            // 統計
            FlyPayAction action = new FlyPayAction();
            DataTable dt = action.CreateExcelBasicSPR();

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelFlyPaySPR(dt);

            return File(ms, "application/vnd.ms-excel", $"2022春節專案每日名額及分配-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xls");
        }
        /// <summary>
        /// 排程用 每日報表
        /// </summary>
        public void ExportSPR1()
        {
            // 先給予春節流水號後統計
            FlySwipeController swControl = new FlySwipeController();
            swControl.SetSPRNum();
            // 統計
            FlyPayAction action = new FlyPayAction();
            DataTable dt = action.CreateExcelBasicSPR();

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelFlyPaySPR(dt);

            PayDAO dao = new PayDAO();
            dao.SendMailAttch("", "衛福部_防疫旅館_春節專案每日報表_上午9點", "如標題附件", ms);
        }
        public void ExportSPR2()
        {
            // 先給予春節流水號後統計
            FlySwipeController swControl = new FlySwipeController();
            swControl.SetSPRNum();
            // 統計
            FlyPayAction action = new FlyPayAction();
            DataTable dt = action.CreateExcelBasicSPR();

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelFlyPaySPR(dt);

            PayDAO dao = new PayDAO();
            dao.SendMailAttch("", "衛福部_防疫旅館_春節專案每日報表_下午1點", "如標題附件", ms);
        }
        public void ExportSPR3()
        {
            // 先給予春節流水號後統計
            FlySwipeController swControl = new FlySwipeController();
            swControl.SetSPRNum();
            // 統計
            FlyPayAction action = new FlyPayAction();
            DataTable dt = action.CreateExcelBasicSPR();

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelFlyPaySPR(dt);

            PayDAO dao = new PayDAO();
            dao.SendMailAttch("", "衛福部_防疫旅館_春節專案每日報表_下午9點", "如標題附件", ms);
        }
        #endregion


        /// <summary>
        /// 銀聯卡api繳費使用
        /// </summary>
        /// <param name="inMac"></param>
        /// <returns></returns>
        public ActionResult CreditSuccessRecv(string inMac)
        {
            return Content(inMac);
        }
    }
}
