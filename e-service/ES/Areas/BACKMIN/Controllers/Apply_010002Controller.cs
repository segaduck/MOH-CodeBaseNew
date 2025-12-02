using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using System;
using System.Data;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using System.ComponentModel;
using System.IO;
using Xceed.Words.NET;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_010002Controller : BaseController
    {

        public static string s_SRV_ID = "010002";//011002
        public static string s_SRV_NAME = "低收入戶及中低收入戶之體外受精(俗稱試管嬰兒)補助方案";

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_010002ViewModel model = new Apply_010002ViewModel();
            Apply_010002FormModel form = new Apply_010002FormModel();
            string s_APP_NAME = s_SRV_NAME; //"低收入戶及中低收入戶之體外受精(俗稱試管嬰兒)補助方案";

            srvid = (srvid ?? s_SRV_ID);
            form = new Apply_010002FormModel();
            form.APP_ID = appid;
            form.SRV_ID = srvid;
            try
            {
                form = dao.QueryApply_010002(form);
            }
            catch (Exception) { }
            if (form == null)
            {
                string s_log1 = "##Apply_010002Controller form = dao.QueryApply_010002(form) is null";
                logger.Error(s_log1);
                ViewBag.tempMessage = "查詢資料有誤!";
                sm.LastErrorMessage = ViewBag.tempMessage;
                return View("Index", model);
            }

            DataTable dtPay = dao.QueryPayInfo(appid);
            DataRow drP1 = null;
            if (dtPay != null) { if (dtPay.Rows.Count > 0) { drP1 = dtPay.Rows[0]; } }
            if (drP1 != null)
            {
                form.PAY_STATUS = "N"; //drP1["PAY_STATUS_MK"].TONotNullString();//PAY_STATUS_MK
                form.PAY_EXT_TIME_AD = drP1["PAY_EXT_TIME"].TONotNullString();
                form.PAY_ACT_TIME = drP1["PAY_ACT_TIME"].TONotNullString();
            }

            form.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(form.APPLY_DATE);
            form.APP_NAME = s_APP_NAME;
            if (form.FLOW_CD == "2")
            {
                model.Detail = dao.GetApplyNotice_010002(appid);
            }

            #region 調整資料後帶入欄位

            // 通訊地址
            TblZIPCODE zip = new TblZIPCODE();
            zip.ZIP_CO = form.C_ZIPCODE;
            var getnamC = dao.GetRow(zip);
            if (getnamC != null && form.C_ADDR != null)
            {
                form.C_ZIPCODE_TEXT = (getnamC.CITYNM + getnamC.TOWNNM);
                form.C_ADDR = form.C_ADDR.TONotNullString().Replace(getnamC.CITYNM + getnamC.TOWNNM, "").Replace(getnamC.TOWNNM, "").Replace(getnamC.CITYNM, "");
            }
            // 戶籍地址
            zip = new TblZIPCODE();
            zip.ZIP_CO = form.H_ZIPCODE;
            var getnamH = dao.GetRow(zip);
            if (getnamH != null && form.H_ADDR != null)
            {
                form.H_ZIPCODE_TEXT = (getnamH.CITYNM + getnamH.TOWNNM);
                form.H_ADDR = form.H_ADDR.TONotNullString().Replace(getnamH.CITYNM + getnamH.TOWNNM, "").Replace(getnamH.TOWNNM, "").Replace(getnamH.CITYNM, "");
            }
            #endregion

            #region MyData 資料顯示處理
            // 置換 MyData Json 資料中的換行, 確保前端在 parseJSON 時正常運作
            if (!string.IsNullOrEmpty(form.MYDATA_IDCN))
            {
                form.MYDATA_IDCN = form.MYDATA_IDCN.Replace("\r", "").Replace("\n", "");
            }
            else if (!string.IsNullOrEmpty(form.MYDATA_TX_RESULT_MSG))
            {
                // 取得 MyData /service/data 異常
                form.MYDATA_IDCN = string.Format("{\"code\":\"500\", \"text\":\"{0}\"}", form.MYDATA_TX_RESULT_MSG);
                form.MYDATA_IDCN = string.Format("{0}{1}{2}", "{", form.MYDATA_IDCN, "}");
            }
            else if (!"200".Equals(form.MYDATA_RTN_CODE) && !string.IsNullOrEmpty(form.MYDATA_RTN_CODE))
            {
                // ApiSP 接收到異常
                form.MYDATA_IDCN = string.Format("\"code\":\"{0}\", \"text\":\"{1}\"", form.MYDATA_RTN_CODE, form.MYDATA_RTN_CODE_DESC);
                form.MYDATA_IDCN = string.Format("{0}{1}{2}", "{", form.MYDATA_IDCN, "}");
            }

            if (!string.IsNullOrEmpty(form.MYDATA_LOWREC))
            {
                form.MYDATA_LOWREC = string.IsNullOrEmpty(form.MYDATA_LOWREC) ? "" : form.MYDATA_LOWREC.Replace("\r", "").Replace("\n", "");
            }
            else if (!string.IsNullOrEmpty(form.MYDATA_TX_RESULT_MSG))
            {
                // 取得 MyData /service/data 異常
                form.MYDATA_LOWREC = string.Format("\"code\":\"500\", \"text\":\"{0}\"", form.MYDATA_TX_RESULT_MSG);
                form.MYDATA_LOWREC = string.Format("{0}{1}{2}", "{", form.MYDATA_LOWREC, "}");
            }
            else if (!"200".Equals(form.MYDATA_RTN_CODE) && !string.IsNullOrEmpty(form.MYDATA_RTN_CODE))
            {
                // ApiSP 接收到異常
                form.MYDATA_LOWREC = string.Format("\"code\":\"{0}\", \"text\":\"{1}\"", form.MYDATA_RTN_CODE, form.MYDATA_RTN_CODE_DESC);
                form.MYDATA_LOWREC = string.Format("{0}{1}{2}", "{", form.MYDATA_LOWREC, "}");
            }
            #endregion

            // 取檔案
            form.FileList = dao.GetFileList_010002(form.APP_ID);

            model.Form = form;

            return View("Index", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_010002ViewModel model)
        {
            // Apply_010002ViewModel model
            // Apply_010002FormModel form
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            // 檢核
            ErrorMsg = dao.CheckApply010002(model);
            if (ErrorMsg == "")
            {
                //存檔
                ErrorMsg = dao.AppendApply010002(model);
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
        /// 更新繳費狀態
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <param name="date"></param>
        /// <param name="IS_PAY_STATUS"></param>
        /// <param name="PAY_A_FEE"></param>
        /// <returns></returns>
        public ActionResult UpdatePayType(string APP_ID, DateTime? date, bool IS_PAY_STATUS, int PAY_A_FEE)
        {
            string ErrorMsg = "";
            var result = new AjaxResultStruct();
            BackApplyDAO dao = new BackApplyDAO();
            if (string.IsNullOrWhiteSpace(APP_ID))
            {
                ErrorMsg = "存檔失敗，請聯絡系統管理員 !";
            }
            else
            {
                if (IS_PAY_STATUS)
                {
                    if (!date.HasValue)
                    {
                        ErrorMsg += "繳費日期不得為空!";
                    }
                    else
                    {
                        // 檢核
                        ErrorMsg = dao.UpdatePayInfo_010002(APP_ID, IS_PAY_STATUS, date, PAY_A_FEE);
                    }
                }
            }
            if (string.IsNullOrEmpty(ErrorMsg))
            {
                result.status = true;
                result.message = "繳費資訊更新成功";
            }
            else
            {
                result.message = ErrorMsg;
            }
            return Content(result.Serialize(), "application/json");
        }

        [HttpPost]
        [DisplayName("010002_申請表套表")]
        public void PreviewApplyForm(Apply_010002FormModel form)
        {
            ShareCodeListModel sc = new ShareCodeListModel();
            Apply_010002FormModel fm = form;
            string path = Server.MapPath("~/Sample/apply010002.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //申請日期
                    string YY1 = fm.APPLY_DATE_TW.ToSplit('/')[0];
                    string MM1 = fm.APPLY_DATE_TW.ToSplit('/')[1];
                    string DD1 = fm.APPLY_DATE_TW.ToSplit('/')[2];
                    string YMD1 = string.Format("中華民國{0}年{1}月{2}日", YY1, MM1, DD1);
                    doc.ReplaceText("$APP_TIME_TW$", YMD1, false, System.Text.RegularExpressions.RegexOptions.None);

                    //$APNAME$	
                    doc.ReplaceText("$APNAME$", fm.APNAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$APIDN$	
                    doc.ReplaceText("$APIDN$", fm.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$APBIRTH$	
                    string YY2 = fm.BIRTHDAY_TW.ToSplit('/')[0];
                    string MM2 = fm.BIRTHDAY_TW.ToSplit('/')[1];
                    string DD2 = fm.BIRTHDAY_TW.ToSplit('/')[2];
                    string YMD2 = string.Format("{0}年{1}月{2}日", YY2, MM2, DD2);
                    doc.ReplaceText("$APBIRTH$", YMD2, false, System.Text.RegularExpressions.RegexOptions.None);
                    //$TEL$
                    doc.ReplaceText("$TEL$", fm.TEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$MOBILE$
                    doc.ReplaceText("$MOBILE$", fm.MOBILE.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$EMAIL$
                    doc.ReplaceText("$EMAIL$", fm.EMAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //$SPNAME$
                    doc.ReplaceText("$SPNAME$", fm.SPNAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$SPIDN$
                    doc.ReplaceText("$SPIDN$", fm.SPIDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$SPBIRTH$
                    string YY3 = fm.SPBIRTHDAY_TW.ToSplit('/')[0];
                    string MM3 = fm.SPBIRTHDAY_TW.ToSplit('/')[1];
                    string DD3 = fm.SPBIRTHDAY_TW.ToSplit('/')[2];
                    string YMD3 = string.Format("{0}年{1}月{2}日", YY3, MM3, DD3);
                    doc.ReplaceText("$SPBIRTH$", YMD3, false, System.Text.RegularExpressions.RegexOptions.None);
                    //$SPTEL$
                    doc.ReplaceText("$SPTEL$", fm.SPTEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$SPMOBILE$
                    doc.ReplaceText("$SPMOBILE$", fm.SPMOBILE.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$SPEMAIL$
                    doc.ReplaceText("$SPEMAIL$", fm.SPEMAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$HADDR$
                    doc.ReplaceText("$HADDR$", string.Format("({0}) {1}{2}", fm.H_ZIPCODE, fm.H_ZIPCODE_TEXT, fm.H_ADDR.TONotNullString()), false, System.Text.RegularExpressions.RegexOptions.None);
                    //$CADDR$
                    doc.ReplaceText("$CADDR$", string.Format("({0}) {1}{2}", fm.C_ZIPCODE, fm.C_ZIPCODE_TEXT, fm.C_ADDR.TONotNullString()), false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            string attach_1 = string.Format(@"attachment;  filename={0}.doc", s_SRV_NAME);
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", attach_1);
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }



    }
}
