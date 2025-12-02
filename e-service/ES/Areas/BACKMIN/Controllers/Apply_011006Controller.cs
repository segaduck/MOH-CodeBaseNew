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
    public class Apply_011006Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_011006ViewModel model = new Apply_011006ViewModel();
            model.Form = new Apply_011006FormModel();
            model.Form.APP_ID = appid;
            model.Form = dao.QueryApply_011006(model.Form);
            DataTable dtPay = dao.QueryPayInfo_011006(appid);

            if (dtPay != null)
            {
                if (dtPay.Rows.Count > 0)
                {
                    model.Form.PAY_STATUS = dtPay.Rows[0][0].TONotNullString();
                    model.Form.PAY_EXT_TIME = dtPay.Rows[0][1].TONotNullString();
                }
            }
            model.Form.APP_TIME_SHOW = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Form.APP_TIME));
            switch (model.Form.APPLY_TYPE)
            {
                case "1":
                    model.Form.APP_NAME = "專科社會工作師證書核發";
                    break;
                case "2":
                    model.Form.APP_NAME = "專科社會工作師證書補發（遺失）";
                    break;
                case "3":
                    model.Form.APP_NAME = "專科社會工作師證書換發（更名或污損）";

                    break;
                default:
                    model.Form.APP_NAME = "專科社會工作師證書";
                    break;
            }
            if (model.Form != null)
            {
                if (model.Form.FLOW_CD == "2")
                {
                    model.Detail = dao.GetApplyNotice_011006(appid);
                    model.isOnlyNoticePaper = new ShareDAO().GetApplyNoticePaper(appid, "OTHER_5,OTHER_6");
                }

                #region 調整資料後帶入欄位

                // 通訊地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = model.Form.C_ZIPCODE;
                var getnam = dao.GetRow(zip);
                if (getnam != null)
                {
                    model.Form.C_ADDR = model.Form.C_ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                }
                // 戶籍地址
                zip = new TblZIPCODE();
                zip.ZIP_CO = model.Form.H_ZIPCODE;
                getnam = null;
                getnam = dao.GetRow(zip);
                if (getnam != null)
                {
                    model.Form.H_ADDR = model.Form.H_ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                }

                #endregion

                // 取檔案
                model.Form.FileList = dao.GetFileList_011006(model.Form.APP_ID);
            }

            return View("Index", model);
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011006ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            // 檢核
            ErrorMsg = dao.CheckApply011006(model);
            if (ErrorMsg == "")
            {
                ErrorMsg = dao.AppendApply011006(model);
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
                        ErrorMsg = dao.UpdatePayInfo_011002(APP_ID, IS_PAY_STATUS, date, PAY_A_FEE, "011006");
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
