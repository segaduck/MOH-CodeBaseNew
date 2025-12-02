using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A6.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;
using OfficeOpenXml;

namespace EECOnline.Areas.A6.Controllers
{
    public class C103MController : BaseController
    {
        /// <summary>
        /// 查詢帳號
        /// </summary>
        public ActionResult Index(C103MFormModel form)
        {
            A6DAO dao = new A6DAO();
            ActionResult rtn = View(form);
            SessionModel sm = SessionModel.Get();
            form.unit_cd = sm.UserInfo.User.unit_cd;
            form.IsDealWith = "0";

            // 帳號額外審核機制 
            var Hd_grpname = "";
            TblAMUROLE tr = new TblAMUROLE();
            tr.userno = sm.UserInfo.UserNo;
            var trlist = dao.GetRowList(tr);

            var usernotemp = form.userno.TONotNullString(); //20211209新增 暫存帳號

            // 20211209 note:僅案件處理 身分者 限制僅查詢本人的資料
            if (trlist.ToCount() == 1)
            {
                TblAMGRP tg = new TblAMGRP();
                tg.grp_id = trlist.FirstOrDefault().grp_id;
                var tgdata = dao.GetRow(tg);
                Hd_grpname = tgdata.grpname;
                if (Hd_grpname.Contains("案件處理"))
                {
                    form.userno = sm.UserInfo.UserNo;
                    form.IsDealWith = form.userno.Contains(usernotemp) ? "1" : "2";
                }
            }

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                // 查詢結果
                form.Grid = dao.QueryC103MGrid(form);

                //if (Hd_grpname.TONotNullString() != "") form.userno = "";
                if (form.IsDealWith == "1" || form.IsDealWith == "2") form.userno = usernotemp; //20211209新增 代回暫存帳號

                // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    rtn = PartialView("_GridRows", form);
                }
                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            return rtn;
        }

        /// <summary>
        /// 新增帳號
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            A6DAO dao = new A6DAO();
            SessionModel sm = SessionModel.Get();
            C103MDetailModel model = new C103MDetailModel();
            model.modtime = HelperUtil.DateTimeToTwString(DateTime.Now, "");
            model.authdates = HelperUtil.DateTimeToTwString(DateTime.Now, "");
            model.authdatee = HelperUtil.DateTimeToTwString(DateTime.Now.AddYears(99), "");
            model.authstatus = "2"; //需更新密碼

            if (sm.UserInfo.User.unit_cd != "00" && sm.UserInfo.User.unit_cd != "01")
            {
                model.unit_cd = sm.UserInfo.User.unit_cd;
            }

            return View("Detail", model);
        }

        /// <summary>
        /// 編輯帳號
        /// </summary>
        /// <param name="userno"></param>
        /// <returns></returns>
        public ActionResult Modify(string userno)
        {
            A6DAO dao = new A6DAO();
            SessionModel sm = SessionModel.Get();
            C103MDetailModel model = new C103MDetailModel();
            model = dao.QueryC103MDetail(userno);

            if (model == null)
            { sm.LastErrorMessage = "找不到指定的資料!"; model = new C103MDetailModel(); }

            model.IsNew = false;

            return View("Detail", model);
        }

        /// <summary>
        /// 儲存群組
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(C103MDetailModel detail)
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            SessionModel sm = SessionModel.Get();
            A6DAO dao = new A6DAO();
            ActionResult rtn = View("Detail", detail);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                // 檢核
                string ErrorMsg = dao.CheckC103M(detail);
                if (ErrorMsg == "")
                {
                    if (detail.IsNew)
                    {
                        dao.AppendC103MDetail(detail);
                        sm.LastResultMessage = "帳號新增成功";
                    }
                    else
                    {
                        dao.UpdateC103MDetail(detail);
                        sm.LastResultMessage = "帳號更新成功";
                    }
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C103M", new { area = "A6", useCache = "2" });
                }
                else
                {
                    sm.LastErrorMessage = ErrorMsg;
                }
            }
            return rtn;
        }

        /// <summary>
        /// 刪除帳號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(C103MDetailModel model)
        {
            if (string.IsNullOrEmpty(model.userno.TONotNullString()))
            {
                throw new ArgumentNullException("Detail.USERNO");
            }
            SessionModel sm = SessionModel.Get();
            A6DAO dao = new A6DAO();
            dao.DeleteC103MDetail(model);

            sm.LastResultMessage = "該帳號已刪除";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C103M", new { area = "A6", useCache = "2" });

            return View("Detail", model);
        }

        /// <summary>
        /// 選擇功能群組
        /// </summary>
        /// <param name="userno"></param>
        /// <returns></returns>
        public ActionResult Grp(string userno)
        {
            A6DAO dao = new A6DAO();
            SessionModel sm = SessionModel.Get();
            C103MGrpModel model = new C103MGrpModel();
            model.userno = userno;
            var unitcd = sm.UserInfo.User.unit_cd;
            model.unit_cd = unitcd == "00" || unitcd == "01" ? "" : unitcd;
            model.Grid = dao.QueryC103MGrp(model);
            return View("Grp", model);
        }

        /// <summary>
        /// 儲存群組
        /// </summary>
        /// <param name="grp"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GrpSave(C103MGrpModel model)
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            SessionModel sm = SessionModel.Get();
            A6DAO dao = new A6DAO();
            ActionResult rtn = View("Grp", model);
            string Msg = dao.CheckC103M(model);
            if (Msg != "") ModelState.AddModelError("model.Grid", Msg);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                dao.SaveC103MGrp(model);
                sm.LastResultMessage = "功能群組設定成功";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "C103M", new { area = "A6", useCache = "2" });
            }

            return rtn;
        }

        #region 匯出帳號清單

        /// <summary>
        /// 匯出Excel
        /// </summary>
        /// <param name="form"></param>
        /// <param name="unitnm">服務單位</param>
        /// <param name="userno">使用者帳號</param>
        /// <param name="username">使用者姓名</param>
        /// <param name="isdealwith">僅案件處理群組 0否 1是 2是(非查詢本人)</param>
        public void ApplyPrint(string unitnm, string userno, string username, string isdealwith)
        {
            A6DAO dao = new A6DAO();
            //ShareCodeListModel sc = new ShareCodeListModel();
            SessionModel sm = SessionModel.Get();
            C103MFormModel form = new C103MFormModel();
            form.unit_cd = sm.UserInfo.User.unit_cd;

            form.unitnm = unitnm;
            form.userno = userno;
            form.username = username;
            form.IsDealWith = isdealwith;
            if (isdealwith == "1" || isdealwith == "2")
            {
                form.userno = sm.UserInfo.User.userno;
                form.IsDealWith = sm.UserInfo.User.userno.Contains(userno) ? "1" : "2";
            }

            form.GridAll = dao.QueryC103MGridAll(form);

            //群組
            //TblAMGRP grp = new TblAMGRP();
            //var group = dao.GetRowList(grp);
            form.GmapmGrid = dao.QueryC103MGmapmGrid(form);

            //權限_功能名稱
            TblAMFUNCM af = new TblAMFUNCM();
            var funcm = dao.GetRowList(af).OrderBy(m => m.prgid);

            string path = Server.MapPath("~/Template/Accountlist.xlsx");
            FileInfo newFile = new FileInfo(path);

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (ExcelPackage excel = new ExcelPackage(newFile))
                {
                    ExcelWorksheet excelSheet = null;
                    excelSheet = excel.Workbook.Worksheets.FirstOrDefault();

                    //人員清單
                    int row = 2;
                    foreach (var item in form.GridAll)
                    {
                        var col = 1;
                        excelSheet.Cells[row, col++].Value = item.insert_unit_nm;
                        excelSheet.Cells[row, col++].Value = item.unit_nm;
                        excelSheet.Cells[row, col++].Value = item.userno;
                        excelSheet.Cells[row, col++].Value = item.username;
                        excelSheet.Cells[row, col++].Value = item.grp_nm;
                        excelSheet.Cells[row, col++].Value = item.authstatus_nm;

                        row++;
                    }

                    //群組清單
                    if (form.GmapmGrid != null)
                    {
                        row += 2;

                        excelSheet.Cells[row, 1].Value = "群組名稱";
                        excelSheet.Cells[row, 2].Value = "現有權限";

                        row += 1;
                        foreach (var item in form.GmapmGrid)
                        {
                            var col = 1;
                            excelSheet.Cells[row, col++].Value = item.grpname;
                            excelSheet.Cells[row, col++].Value = item.prgname;

                            row++;
                        }
                    }

                    //權限功能
                    row += 2;

                    excelSheet.Cells[row, 1].Value = "權限列表";

                    row += 1;
                    foreach (var item in funcm)
                    {
                        var col = 1;
                        if (item.prgid.TONotNullString() == "" || item.prgid.TONotNullString() == " ")
                        {
                            excelSheet.Cells[row, col++].Value = item.sysid + "：" + item.prgname;
                        }
                        else
                        {
                            excelSheet.Cells[row, col++].Value = item.prgid + "：" + item.prgname;
                        }
                        row++;
                    }

                    excel.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            string dt = DateTime.Now.ToString("yyyyMMdd");
            dt = (dt.SubstringTo(0, 4).TOInt32()) - 1911 + dt.Substring(4);
            string attach_1 = string.Format(@"attachment;  filename={0}.xlsx", dt + "帳號權限清查資料");
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", attach_1);
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }
        #endregion
    }
}