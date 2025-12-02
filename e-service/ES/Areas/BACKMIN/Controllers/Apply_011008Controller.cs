using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using System;
using System.Data;
using System.Web.Mvc;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_011008Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_011008ViewModel model = new Apply_011008ViewModel();
            string s_APP_NAME = "社工師證書換發（更名或汙損）";
            model.Form = new Apply_011008FormModel();
            model.Form.APP_ID = appid;
            model.Form = dao.QueryApply_011008(model.Form);
            if (model.Form == null)
            {
                string s_log1 = "##Apply_011008Controller dao.QueryApply_011008(model.Form) is null";
                logger.Error(s_log1);
                ViewBag.tempMessage = "查詢資料有誤!";
                sm.LastErrorMessage = ViewBag.tempMessage;
                return View("Index", model);
            }
            //ViewBag.tempMessage = "查詢資料有誤!";
            //sm.LastErrorMessage = ViewBag.tempMessage;
            //return View("Index", model);

            DataTable dtPay = dao.QueryPayInfo(appid);
            DataRow drP1 = null;
            if (dtPay != null) { if (dtPay.Rows.Count > 0) { drP1 = dtPay.Rows[0]; } }
            if (drP1 != null)
            {
                model.Form.PAY_STATUS = drP1["PAY_STATUS_MK"].TONotNullString();//PAY_STATUS_MK
                model.Form.PAY_EXT_TIME_AD = drP1["PAY_EXT_TIME"].TONotNullString();
                model.Form.PAY_ACT_TIME = drP1["PAY_ACT_TIME"].TONotNullString();
                //model.Form.PAY_EXT_TIME = HelperUtil.TransToDateTime(drP1["PAY_EXT_TIME"].TONotNullString());
                //model.Form.PAY_EXT_TIME_TW = HelperUtil.DateTimeToTwString(model.Form.PAY_EXT_TIME);
            }
            //APPLY_TYPE 1:更名 2:汙損
            string s_APPLY_TYPE_TXT = "";
            if (model.Form.APPLY_TYPE == "1") { s_APPLY_TYPE_TXT = "更名"; }
            if (model.Form.APPLY_TYPE == "2") { s_APPLY_TYPE_TXT = "汙損"; }
            model.Form.APPLY_TYPE_TXT = s_APPLY_TYPE_TXT;

            model.Form.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(model.Form.APPLY_DATE);
            model.Form.APP_NAME = s_APP_NAME;

            if (model.Form != null)
            {
                if (model.Form.FLOW_CD == "2")
                {
                    model.Detail = dao.GetApplyNotice_011008(appid);
                }

                #region 調整資料後帶入欄位

                // 通訊地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = model.Form.C_ZIPCODE;
                var getnam = dao.GetRow(zip);
                if (getnam != null && model.Form.C_ADDR != null)
                {
                    model.Form.C_ADDR = model.Form.C_ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                }
                // 戶籍地址
                zip = new TblZIPCODE();
                zip.ZIP_CO = model.Form.H_ZIPCODE;
                getnam = null;
                getnam = dao.GetRow(zip);
                if (getnam != null && model.Form.H_ADDR != null)
                {
                    model.Form.H_ADDR = model.Form.H_ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                }

                #endregion

                // 取檔案
                model.Form.FileList = dao.GetFileList_011008(model.Form.APP_ID);
            }

            return View("Index", model);
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011008ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            // 檢核
            ErrorMsg = dao.CheckApply011008(model);
            if (ErrorMsg == "")
            {
                //存檔
                ErrorMsg = dao.AppendApply011008(model);
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
                        ErrorMsg = dao.UpdatePayInfo_011008(APP_ID, IS_PAY_STATUS, date, PAY_A_FEE);
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

    }
}
