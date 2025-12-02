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
    public class Apply_041001Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_041001ViewModel model = new Apply_041001ViewModel();
            string s_APP_NAME = "全民健康保險爭議案件(權益案件及特約管理案件)線上申辦";

            model.Form = new Apply_041001FormModel();
            model.Form.APP_ID = appid;
            model.Form = dao.QueryApply_041001(model.Form);
            if (model.Form == null)
            {
                string s_log1 = "##Apply_041001Controller dao.QueryApply_041001(model.Form) is null";
                logger.Error(s_log1);
                ViewBag.tempMessage = "查詢資料有誤!";
                sm.LastErrorMessage = ViewBag.tempMessage;
                return View("Index", model);
            }
            model.Form.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(model.Form.APPLY_DATE);
            model.Form.APP_NAME = s_APP_NAME;
            //申請人
            model.Form.H_TEL = model.Form.CNT_TEL;
            //地址
            TblZIPCODE zip = new TblZIPCODE();
            zip.ZIP_CO = model.Form.ADDR_CODE;
            var zipdata = dao.GetRow(zip);
            model.Form.C_ZIPCODE = model.Form.ADDR_CODE;
            model.Form.C_ZIPCODE_TEXT = zipdata.CITYNM + zipdata.TOWNNM;
            model.Form.C_ADDR = model.Form.ADDR;

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
            // 收受或知悉日期
            if (model.Form.KNOW_DATE != null)
            {
                model.Form.KNOW_DATE_STR = HelperUtil.DateTimeToTwString(model.Form.KNOW_DATE);
            }
            //核定文件
            if (model.Form.LIC_DATE != null)
            {
                model.Form.LIC_DATE_STR = HelperUtil.DateTimeToTwString(model.Form.LIC_DATE);
            }
            model.Form.KIND1_CHK = model.Form.KIND1 == "Y" ? true : false;
            model.Form.KIND2_CHK = model.Form.KIND2 == "Y" ? true : false;
            model.Form.KIND3_CHK = model.Form.KIND3 == "Y" ? true : false;

            if (model.Form != null)
            {
                //// 取檔案
                model.Form.FileList = new Apply_041001FileModel();
                model.Form.FileList = dao.GetFileList_041001(model.Form.APP_ID);
                model.Form.FileList.SRVLIST = new List<Apply_041001SRVLSTModel>();
                if (model.Form.FileList.FILE != null && model.Form.FileList.FILE.Count > 0)
                {
                    foreach (var item in model.Form.FileList.FILE)
                    {
                        var insert_data = new Apply_041001SRVLSTModel();
                        insert_data.InjectFrom(item);
                        switch (item.FILE_NO)
                        {
                            case 1:
                                model.Form.FILE_1 = item.FILENAME;
                                model.Form.FILE_1_TEXT = item.SRC_FILENAME;
                                break;
                            case 2:
                                model.Form.FILE_2 = item.FILENAME;
                                model.Form.FILE_2_TEXT = item.SRC_FILENAME;
                                break;
                            case 3:
                                model.Form.FILE_3 = item.FILENAME;
                                model.Form.FILE_3_TEXT = item.SRC_FILENAME;
                                break;
                            case 4:
                                model.Form.FILE_4 = item.FILENAME;
                                model.Form.FILE_4_TEXT = item.SRC_FILENAME;
                                break;
                            default:
                                insert_data.FILE_5_TEXT = item.SRC_FILENAME;
                                model.Form.FileList.SRVLIST.Add(insert_data);
                                break;
                        }
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
        public ActionResult Save(Apply_041001ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            // 檢核
            ErrorMsg = dao.CheckApply041001(model);

            if (ErrorMsg == "")
            {
                //存檔
                ErrorMsg = dao.AppendApply041001(model);
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
            string path = Server.MapPath("~/Sample/apply041001.docx");
            var filesStr = string.Empty;
            byte[] buffer = null;
            Apply_041001FormModel vm = new Apply_041001FormModel();
            vm = GetApply041001Data(APP_ID);
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
                    doc.ReplaceText("$AIDN$", vm.IDN.TONotNullString());
                    doc.ReplaceText("$AADDRCODE$", vm.C_ZIPCODE.TONotNullString() + " " + vm.C_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$AADDR$", vm.C_ADDR.TONotNullString());
                    doc.ReplaceText("$AMOBILE$", vm.MOBILE.TONotNullString());
                    doc.ReplaceText("$ATEL$", vm.CNT_TEL.TONotNullString());
                    //代理人
                    doc.ReplaceText("$RNAME$", vm.R_NAME.TONotNullString());
                    doc.ReplaceText("$RIDN$", vm.R_IDN.TONotNullString());
                    doc.ReplaceText("$RADDRCODE$", vm.R_ADDR.TONotNullString() == "" ? "" : vm.R_ADDR_CODE.TONotNullString() + " " + vm.R_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$RADDR$", vm.R_ADDR.TONotNullString());
                    doc.ReplaceText("$RMOBILE$", vm.R_MOBILE.TONotNullString());
                    doc.ReplaceText("$RTEL$", vm.R_TEL.TONotNullString());
                    //申請內容
                    doc.ReplaceText("$KIND1$", vm.KIND1 == "Y" ? "█" : "□");

                    if (!string.IsNullOrEmpty(vm.LIC_DATE_STR))
                    {
                        doc.ReplaceText("$LYEAR$", vm.LIC_DATE_STR.Split('/')[0]);
                        doc.ReplaceText("$LMONTH$", vm.LIC_DATE_STR.Split('/')[1]);
                        doc.ReplaceText("$LDAY$", vm.LIC_DATE_STR.Split('/')[2]);
                        doc.ReplaceText("$LICCD$", vm.LIC_CD.TONotNullString());
                        doc.ReplaceText("$LICNUM$", vm.LIC_NUM.TONotNullString());
                    }
                    else
                    {
                        doc.ReplaceText("$LYEAR$", "");
                        doc.ReplaceText("$LMONTH$", "");
                        doc.ReplaceText("$LDAY$", "");
                        doc.ReplaceText("$LICCD$", "");
                        doc.ReplaceText("$LICNUM$", "");
                    }

                    doc.ReplaceText("$KIND2$", vm.KIND2 == "Y" ? "█" : "□");
                    doc.ReplaceText("$KIND3$", vm.KIND3 == "Y" ? "█" : "□");
                    if (!string.IsNullOrEmpty(vm.KNOW_DATE_STR))
                    {
                        doc.ReplaceText("$KYEAR$", vm.KNOW_DATE_STR.Split('/')[0]);
                        doc.ReplaceText("$KMONTH$", vm.KNOW_DATE_STR.Split('/')[1]);
                        doc.ReplaceText("$KDAY$", vm.KNOW_DATE_STR.Split('/')[2]);
                    }
                    else
                    {
                        doc.ReplaceText("$KYEAR$", "");
                        doc.ReplaceText("$KMONTH$", "");
                        doc.ReplaceText("$KDAY$", "");
                    }

                    doc.ReplaceText("$KNOWMEMO$", vm.KNOW_MEMO.TONotNullString());
                    doc.ReplaceText("$KNOWFACT$", vm.KNOW_FACT.TONotNullString());
                    doc.ReplaceText("$FILES$", filesStr.TONotNullString());
                    doc.ReplaceText("$AYEAR$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[0]);
                    doc.ReplaceText("$AMONTH$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[1]);
                    doc.ReplaceText("$ADAY$", HelperUtil.TransToTwYear(Convert.ToDateTime(vm.APP_TIME)).Split('/')[2]);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=全民健康保險爭議審議申請書.docx");
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
        public Apply_041001FormModel GetApply041001Data(string app_id)
        {
            Apply_041001FormModel model = new Apply_041001FormModel();
            var APP_ID = app_id;
            BackApplyDAO dao = new BackApplyDAO();

            // 案件基本資訊
            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);
            model.APP_ID = APP_ID;
            model.SRV_ID = "041001";
            model.APP_TIME = alydata.APP_TIME;
            // 申請人
            model.NAME = alydata.NAME;
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
            TblAPPLY_041001 where_data = new TblAPPLY_041001();
            where_data.APP_ID = APP_ID;
            var data = dao.GetRow(where_data);
            //代理人
            model.R_NAME = data.R_NAME;
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
            // 申請內容
            model.LIC_DATE_STR = HelperUtil.DateTimeToTwString(data.LIC_DATE);
            model.LIC_CD = data.LIC_CD;
            model.LIC_NUM = data.LIC_NUM;
            model.KNOW_DATE_STR = HelperUtil.DateTimeToTwString(data.KNOW_DATE);
            model.KNOW_MEMO = data.KNOW_MEMO;
            model.KNOW_FACT = data.KNOW_FACT;
            model.KIND1_CHK = data.KIND1 == "Y" ? true : false;
            model.KIND2_CHK = data.KIND2 == "Y" ? true : false;
            model.KIND3_CHK = data.KIND3 == "Y" ? true : false;
            model.KIND1 = data.KIND1;
            model.KIND2 = data.KIND2;
            model.KIND3 = data.KIND3;
            // 取檔案
            model.FileList = dao.GetFileList_041001(model.APP_ID); //SRC_FILENAME

            return model;
        }
    }
}
