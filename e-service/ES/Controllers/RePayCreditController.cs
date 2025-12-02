using System;
using System.Linq;
using System.Web.Mvc;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ES.Utils;
using HppApi;

namespace ES.Controllers
{
    public class RePayCreditController : BaseController
    {
        #region 上傳繳費資料檔案
        public ActionResult Index(string APP_ID, string SRV_ID)
        {
            SessionModel sm = SessionModel.Get();
            logger.Debug($"RePayCredit_APPl_ID:{APP_ID},SRV_ID:{SRV_ID}");
            if (string.IsNullOrWhiteSpace(APP_ID) || string.IsNullOrWhiteSpace(SRV_ID))
            {
                sm.LastErrorMessage = "您無權限使用此功能 !";
                return null;
            }
            if (sm.UserInfo.Member == null)
            {
                sm.LastErrorMessage = "請重新登入系統 !";
                return null;
            }
            RePayCreditFormModel model = new RePayCreditFormModel();
            model.APP_ID = APP_ID;
            model.SRV_ID = SRV_ID;
            return View("RePayCredit", model);
        }
        public ActionResult RePayCreditSave(RePayCreditFormModel model)
        {
            var result = new AjaxResultStruct();
            var Msg = string.Empty;
            PayDAO dao = new PayDAO();
            SessionModel sm = SessionModel.Get();
            model.ERRMSG = "";
            logger.Debug($"RePayCreditSave_APP_ID:{model.APP_ID},CARD_IDN:{model.CARD_IDN}");
            if (sm.UserInfo.Member == null)
            {
                model.ERRMSG += "請重新登入系統<br/>";
                ModelState.AddModelError("LOGIN_ERR", "請重新登入系統");
            }
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                try
                {
                    // 檢查信用卡繳費狀態
                    logger.Debug("RePayCredit信用卡重複繳費，檢查信用卡繳費狀態" + model.APP_ID);
                    var oldApplyPay = dao.GetBeforRePaySessionKey(model);
                    var isUpdate = string.Empty;
                    if (DataUtils.GetConfig("PAY_EC_OPEN").ToUpper() == "Y" && DateTime.Now >= Convert.ToDateTime(DataUtils.GetConfig("PAY_EC_SDATE")))
                    {
                        //聯合信用卡中心
                        isUpdate = dao.CheckCreditStatusEC(model, oldApplyPay);
                    }
                    else
                    {
                        // 我的E政府
                        isUpdate = dao.CheckCreditStatus(model, oldApplyPay);
                    }
                    logger.Debug("RePayCredit檢查信用卡繳費狀態:" + isUpdate);
                    // 0000已繳費, 1000無sessionKey, 9999更新失敗, 其他
                    if (isUpdate == "0000" || isUpdate == "00")
                    {
                        sm.LastResultMessage = "此筆已繳費成功，請勿重新繳納";
                        model.STATUS = "N";
                        model.ERRMSG = "此筆已繳費成功，請勿重新繳納";
                        logger.Debug(model.ERRMSG);
                    }
                    else
                    {
                        logger.Debug($"RePayCreditSave_CLIENT_IP:{GetClientIP()}");
                        //更新APPLY APPLY_PAY
                        model.CLIENT_IP = GetClientIP();
                        logger.Debug("RePayUpdateApplyPay");
                        if (DataUtils.GetConfig("PAY_EC_OPEN").ToUpper() == "Y" && DateTime.Now >= Convert.ToDateTime(DataUtils.GetConfig("PAY_EC_SDATE")))
                        {
                            // 聯合信用卡中心
                            Msg = dao.RePayUpdateApplyPayEC(model);
                        }
                        else
                        {
                            // 我的E政府
                            Msg = dao.RePayUpdateApplyPay(model);
                        }
                        logger.Debug("RePayUpdateApplyPay_Msg:" + Msg);
                        if (!string.IsNullOrEmpty(Msg))
                        {
                            var dataMsg = Msg.ToSplit('-');
                            if (dataMsg[0] == "0000" || dataMsg[0] == "00")
                            {
                                model.STATUS = "Y";
                                model.SessionTransactionKey = dataMsg[1];
                            }
                            else
                            {
                                model.STATUS = "N";
                                model.ERRMSG = $"繳費失敗，{Msg}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                    sm.LastResultMessage = "發生錯誤，請重新操作或電洽服務人員";
                    model.STATUS = "N";
                    model.ERRMSG = "發生錯誤，請重新操作或電洽服務人員";
                }
            }

            return View("RePayCredit", model);
        }
        #endregion 上傳繳費資料檔案

        /// <summary>
        /// 聯合信用卡中心 交易後結果回傳
        /// </summary>
        /// <returns></returns>
        public ActionResult SuccessEC(string KEY)
        {
            PayDAO dao = new PayDAO();
            SuccessECModel result = new SuccessECModel();
            ApiClient apiClient = new ApiClient();
            apiClient.setKEY(KEY);
            apiClient.setURL(DataUtils.GetConfig("PAY_EC_DOMAINNAME"), DataUtils.GetConfig("PAY_EC_REQUESTURL"));
            apiClient.postQuery();
            //查詢結果
            CreditHPPModel model = new CreditHPPModel();
            model.DecModel.MerchantID = apiClient.getMERCHANTID();
            model.DecModel.TerminalID = apiClient.getTERMINALID();
            model.DecModel.OrderID = apiClient.getORDERID();
            var merchant = DataUtils.GetConfig("PAY_EC_MERCHANTID"); // 特店
            var trminal = DataUtils.GetConfig("PAY_EC_TRMINALID"); // 終端機
            if (model.DecModel.MerchantID == merchant && model.DecModel.TerminalID == trminal)
            {
                var appstr = dao.GetAPPbySessionKey(KEY);
                if (string.IsNullOrEmpty(appstr))
                {
                    result.ErrorMessage = "無對應之案件編號，請洽管理人員";
                    result.OrderID = model.DecModel.OrderID;
                }
                else
                {
                    model.DecModel.MemberToken = apiClient.getMemberToken();
                    model.DecModel.MemberID = apiClient.getMemberID();
                    model.DecModel.PAN = apiClient.getPAN();
                    model.DecModel.EXPIREDATE = apiClient.getEXPIREDATE();
                    model.DecModel.TRANSCODE = apiClient.getTRANSCODE();
                    model.DecModel.TRANSMODE = apiClient.getTRANSMODE();
                    model.DecModel.TRANSAMT = apiClient.getTRANSAMT();
                    model.DecModel.TRANSDATE = apiClient.getTRANSDATE();
                    model.DecModel.TRANSTIME = apiClient.getTRANSTIME();
                    model.DecModel.APPROVECODE = apiClient.getAPPROVECODE();
                    model.DecModel.RESPONSECODE = apiClient.getRESPONSECODE();
                    model.DecModel.RESPONSEMSG = apiClient.getRESPONSEMSG();
                    model.DecModel.FIRSTAMT = apiClient.getFIRSTAMT();
                    model.DecModel.FEE = apiClient.getFEE();
                    model.DecModel.CREDITAMT = apiClient.getCREDITAMT();
                    model.DecModel.RiskMark = apiClient.getRiskMark();
                    model.DecModel.Foreign = apiClient.getForeign();
                    model.DecModel.SecureStatus = apiClient.getSecureStatus();
                    model.DecModel.PrivateData = apiClient.getPrivateData();

                    // 更新繳費狀態
                    var UpdateBool = dao.UpdatePayResultEC(model, KEY);
                    if (UpdateBool)
                    {
                        result.ErrorMessage = "繳費成功。";
                        result.OrderID = appstr;
                    }
                    else
                    {
                        result.ErrorMessage = "繳費失敗。請洽管理人員。";
                        result.OrderID = appstr + ";" + model.DecModel.OrderID;
                    }
                }
            }
            else
            {
                result.ErrorMessage = "特店代碼或終端機代碼不相符。";
                result.OrderID = KEY;
            }

            logger.Debug("SuccessEC" + KEY);
            return View("~/Views/Shared/ResponseEC.cshtml", result);
        }
    }
}
