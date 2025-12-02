using DocumentFormat.OpenXml.EMMA;
using ES.Action.Form;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ES.Utils;
using Omu.ValueInjecter;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_040001Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_040001ViewModel model = new Apply_040001ViewModel();
            string s_APP_NAME = "衛生福利部訴願案件";

            model.Form = new Apply_040001FormModel();
            model.Form.APP_ID = appid;
            model.Form = dao.QueryApply_040001(model.Form);
            if (model.Form == null)
            {
                string s_log1 = "##Apply_040001Controller dao.QueryApply_040001(model.Form) is null";
                logger.Error(s_log1);
                ViewBag.tempMessage = "查詢資料有誤!";
                sm.LastErrorMessage = ViewBag.tempMessage;
                return View("Index", model);
            }
            model.Form.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(model.Form.APPLY_DATE);
            model.Form.APP_NAME = s_APP_NAME;
            //訴願人 / 申請人
            model.Form.BIRTHDAY_STR = HelperUtil.DateTimeToTwString(model.Form.BIRTHDAY);
            model.Form.H_TEL = model.Form.CNT_TEL;
            //地址
            TblZIPCODE zip = new TblZIPCODE();
            zip.ZIP_CO = model.Form.ADDR_CODE;
            var zipdata = dao.GetRow(zip);
            model.Form.C_ZIPCODE = model.Form.ADDR_CODE;
            model.Form.C_ZIPCODE_TEXT = zipdata.CITYNM + zipdata.TOWNNM;
            model.Form.C_ADDR = model.Form.ADDR;
            // 代表人
            if (model.Form.CHR_BIRTH != null)
            {
                model.Form.CHR_BIRTH_STR = HelperUtil.DateTimeToTwString(model.Form.CHR_BIRTH);
            }
            //地址
            if (!string.IsNullOrEmpty(model.Form.CHR_ADDR_CODE))
            {
                TblZIPCODE zip_chr = new TblZIPCODE();
                zip_chr.ZIP_CO = model.Form.CHR_ADDR_CODE;
                var zip_chrdata = dao.GetRow(zip_chr);
                model.Form.CHR_ZIPCODE = model.Form.CHR_ADDR_CODE;
                model.Form.CHR_ZIPCODE_TEXT = zip_chrdata.CITYNM + zip_chrdata.TOWNNM;
                model.Form.CHR_ADDR = model.Form.CHR_ADDR;
            }
            // 代理人
            if (model.Form.R_BIRTH != null)
            {
                model.Form.R_BIRTH_STR = HelperUtil.DateTimeToTwString(model.Form.R_BIRTH);
            }
            //地址
            if (!string.IsNullOrEmpty(model.Form.R_ADDR_CODE))
            {
                TblZIPCODE zip_r = new TblZIPCODE();
                zip_r.ZIP_CO = model.Form.R_ADDR_CODE;
                var zip_rdata = dao.GetRow(zip_r);
                model.Form.R_ZIPCODE = model.Form.R_ADDR_CODE;
                model.Form.R_ZIPCODE_TEXT = zip_rdata.CITYNM + zip_rdata.TOWNNM;
                model.Form.R_ADDR = model.Form.R_ADDR;
            }
            // 申訴案件日期
            if (model.Form.ORG_DATE != null)
            {
                model.Form.ORG_DATE_STR = HelperUtil.DateTimeToTwString(model.Form.ORG_DATE);
            }

            if (model.Form != null)
            {
                //// 取檔案
                model.Form.FileList = dao.GetFileList_040001(model.Form.APP_ID);
                model.Form.FileList.SRVLIST = new List<Apply_040001SRVLSTModel>();
                if (model.Form.FileList.FILE != null && model.Form.FileList.FILE.Count > 0)
                {
                    foreach (var item in model.Form.FileList.FILE)
                    {
                        var insert_data = new Apply_040001SRVLSTModel();
                        insert_data.InjectFrom(item);
                        insert_data.FILE_1_TEXT = item.SRC_FILENAME;
                        model.Form.FileList.SRVLIST.Add(insert_data);
                    }
                }
            }
            return View("Index", model);
        }
        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_040001ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            // 檢核
            ErrorMsg = dao.CheckApply040001(model);

            if (ErrorMsg == "")
            {
                //存檔
                ErrorMsg = dao.AppendApply040001(model);
                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";
                }
                else result.message = ErrorMsg;
            }
            else result.message = ErrorMsg;

            return Content(result.Serialize(), "application/json");
        }
        /// <summary>
        /// 套印申請書
        /// </summary>
        /// <param name="APP_ID"></param>
        public void PreviewApplyForm(string APP_ID)
        {
            string path = Server.MapPath("~/Sample/apply040001.docx");
            var filesStr = string.Empty;
            byte[] buffer = null;
            Apply_040001FormModel vm = new Apply_040001FormModel();
            vm = GetApply040001Data(APP_ID);
            if (vm.FileList != null && vm.FileList.FILE.Count > 0)
            {
                foreach (var item in vm.FileList.FILE)
                {
                    filesStr += item.SRC_FILENAME + ",";
                }
            }
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //訴願人
                    doc.ReplaceText("$ANAME$", vm.NAME.TONotNullString());
                    doc.ReplaceText("$ABIRTH$", vm.BIRTHDAY_STR.TONotNullString());
                    doc.ReplaceText("$AIDN$", vm.IDN.TONotNullString());
                    doc.ReplaceText("$AADDRCODE$", vm.C_ZIPCODE.TONotNullString() + " " + vm.C_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$AADDR$", vm.C_ADDR.TONotNullString());
                    doc.ReplaceText("$AMOBILE$", vm.MOBILE.TONotNullString());
                    doc.ReplaceText("$ATEL$", vm.CNT_TEL.TONotNullString());
                    //代表人
                    doc.ReplaceText("$CHRNAME$", vm.CHR_NAME.TONotNullString());
                    doc.ReplaceText("$CHRBIRTH$", vm.CHR_BIRTH_STR.TONotNullString());
                    doc.ReplaceText("$CHRIDN$", vm.CHR_IDN.TONotNullString());
                    doc.ReplaceText("$CHRADDRCODE$", vm.CHR_ADDR.TONotNullString() == "" ? "" : vm.CHR_ADDR_CODE.TONotNullString() + " " + vm.CHR_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$CHRADDR$", vm.CHR_ADDR.TONotNullString());
                    doc.ReplaceText("$CHRMOBILE$", vm.CHR_MOBILE.TONotNullString());
                    doc.ReplaceText("$CHRTEL$", vm.CHR_TEL.TONotNullString());
                    //代理人
                    doc.ReplaceText("$RNAME$", vm.R_NAME.TONotNullString());
                    doc.ReplaceText("$RBIRTH$", vm.R_BIRTH_STR.TONotNullString());
                    doc.ReplaceText("$RIDN$", vm.R_IDN.TONotNullString());
                    doc.ReplaceText("$RADDRCODE$", vm.R_ADDR.TONotNullString() == "" ? "" : vm.R_ADDR_CODE.TONotNullString() + " " + vm.R_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$RADDR$", vm.R_ADDR.TONotNullString());
                    doc.ReplaceText("$RMOBILE$", vm.R_MOBILE.TONotNullString());
                    doc.ReplaceText("$RTEL$", vm.R_TEL.TONotNullString());
                    //申請內容
                    doc.ReplaceText("$ORGNAME$", vm.ORG_NAME.TONotNullString());
                    doc.ReplaceText("$ORGDATE$", vm.ORG_DATE_STR.TONotNullString());
                    doc.ReplaceText("$ORGMEMO$", vm.ORG_MEMO.TONotNullString());
                    doc.ReplaceText("$ORGFACT$", vm.ORG_FACT.TONotNullString());
                    doc.ReplaceText("$ORGNAME2$", vm.ORG_NAME.TONotNullString());
                    doc.ReplaceText("$ANAME2$", vm.NAME.TONotNullString());
                    doc.ReplaceText("$CHRNAME2$", vm.CHR_NAME.TONotNullString());
                    doc.ReplaceText("$RNAME2$", vm.R_NAME.TONotNullString());
                    doc.ReplaceText("$AYEAR$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[0]);
                    doc.ReplaceText("$AMONTH$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[1]);
                    doc.ReplaceText("$ADAY$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[2]);
                    doc.ReplaceText("$FILES$", filesStr.TONotNullString());
                    doc.ReplaceText("$AYEAR2$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[0]);
                    doc.ReplaceText("$AMONTH2$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[1]);
                    doc.ReplaceText("$ADAY2$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[2]);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=訴願申請書.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// 取得資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <returns></returns>
        public Apply_040001FormModel GetApply040001Data(string app_id)
        {
            Apply_040001FormModel model = new Apply_040001FormModel();
            var APP_ID = app_id;
            BackApplyDAO dao = new BackApplyDAO();

            // 案件基本資訊
            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);
            model.APP_ID = APP_ID;
            model.SRV_ID = "040001";
            model.APP_TIME = alydata.APP_TIME;
            // 訴願人
            model.NAME = alydata.NAME;
            model.BIRTHDAY_STR = HelperUtil.DateTimeToTwString(alydata.BIRTHDAY);
            model.IDN = alydata.IDN;
            // 地址
            TblZIPCODE zip = new TblZIPCODE();
            zip.ZIP_CO = alydata.ADDR_CODE;
            var zipdata = dao.GetRow(zip);
            model.C_ZIPCODE = alydata.ADDR_CODE;
            model.C_ZIPCODE_TEXT = alydata.ADDR_CODE != "" ? zipdata.CITYNM + zipdata.TOWNNM : "";
            model.C_ADDR = alydata.ADDR;
            model.CNT_TEL = alydata.CNT_TEL;
            model.MOBILE = alydata.MOBILE;

            TblAPPLY_040001 where_data = new TblAPPLY_040001();
            where_data.APP_ID = APP_ID;
            var data = dao.GetRow(where_data);
            //代表人
            model.CHR_NAME = data.CHR_NAME;
            model.CHR_BIRTH_STR = HelperUtil.DateTimeToTwString(data.CHR_BIRTH);
            model.CHR_IDN = data.CHR_IDN;
            // 地址
            TblZIPCODE chrzip = new TblZIPCODE();
            chrzip.ZIP_CO = data.CHR_ADDR_CODE;
            var chrzipdata = dao.GetRow(chrzip);
            model.CHR_ZIPCODE = data.CHR_ADDR_CODE;
            model.CHR_ZIPCODE_TEXT = data.CHR_ADDR_CODE != null && data.CHR_ADDR_CODE != "" ? chrzipdata.CITYNM + chrzipdata.TOWNNM : "";
            model.CHR_ADDR = data.CHR_ADDR;
            model.CHR_TEL = data.CHR_TEL;
            model.CHR_MOBILE = data.CHR_MOBILE;
            //代理人
            model.R_NAME = data.R_NAME;
            model.R_BIRTH_STR = HelperUtil.DateTimeToTwString(data.R_BIRTH);
            model.R_IDN = data.R_IDN;
            // 地址
            TblZIPCODE rzip = new TblZIPCODE();
            rzip.ZIP_CO = data.R_ADDR_CODE;
            var rzipdata = dao.GetRow(rzip);
            model.R_ZIPCODE = data.R_ADDR_CODE;
            model.R_ZIPCODE_TEXT = data.R_ADDR_CODE != null && data.R_ADDR_CODE != "" ? rzipdata.CITYNM + rzipdata.TOWNNM : "";
            model.R_ADDR = data.R_ADDR;
            model.R_TEL = data.R_TEL;
            model.R_MOBILE = data.R_MOBILE;
            // 訴願請求
            model.ORG_NAME = data.ORG_NAME;
            model.ORG_DATE_STR = HelperUtil.DateTimeToTwString(data.ORG_DATE);
            model.ORG_MEMO = data.ORG_MEMO;
            model.ORG_FACT = data.ORG_FACT;

            //// 取檔案
            model.FileList = dao.GetFileList_040001(model.APP_ID); //SRC_FILENAME

            return model;
        }
    }
}
