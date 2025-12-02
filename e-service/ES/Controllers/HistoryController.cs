using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models;
using System.Data.SqlClient;
using ES.Action;
using ES.Utils;
using System.IO;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Service.GSP2;
using HppApi;

namespace ES.Controllers
{
    public class HistoryController : BaseController
    {
        /// <summary>
        /// 案件紀錄查詢
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            HistoryFormModel model = new HistoryFormModel();
            SessionModel sm = SessionModel.Get();
            WebDAO dao = new WebDAO();
            try
            {
                if (sm.UserInfo == null)
                {
                    sm.LastErrorMessage = "您尚未登入會員或無權限使用此功能 !";
                    return RedirectToAction("Index", "Login");
                }
                model.Grid = dao.GetCase(sm.UserInfo.UserNo);
                if (model.Grid != null && model.Grid.Count > 0)
                {
                    // 信用卡勾稽
                    foreach (var item in model.Grid)
                    {
                        if (item.PAY_METHOD == "C")
                        {
                            var status = false;
                            if (DataUtils.GetConfig("PAY_EC_OPEN").ToUpper() == "Y" && DateTime.Now >= Convert.ToDateTime(DataUtils.GetConfig("PAY_EC_SDATE")))
                            {
                                // 聯合信用中心
                                status = CheckPayCardEC(item.APP_ID);
                            }
                            else
                            {
                                // 我的E政府
                                status = CheckPayCard(item.APP_ID);
                            }
                        }
                    }
                }
                // 更新狀態
                model.Grid = dao.GetCase(sm.UserInfo.UserNo);

                return View(model);
            }
            catch (Exception ex)
            {
                sm.LastErrorMessage = "系統發生錯誤，請洽系統管理員";
                logger.Error(ex.Message, ex);
                return RedirectToAction("Index", "Login");
            }
        }

        /// <summary>
        /// 列印超商繳費單
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public ActionResult DownloadPayFile(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            return File(dao.ExportPayPDF(APP_ID), "application/pdf", "Pay" + APP_ID + ".pdf");
        }

        /// <summary>
        /// 我的E政府信用卡勾稽
        /// </summary>
        /// <param name="APP_ID"></param>
        public bool CheckPayCard(string APP_ID)
        {
            bool rst = false;
            logger.Debug("HistoryController.Index.Model.Grid.APP_ID:" + APP_ID);
            MapUtils data = new MapUtils();
            PayDAO dao = new PayDAO();
            APPLY_PAY model = new APPLY_PAY();
            try
            {
                var isFalse = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    // 查詢
                    APPLY_PAY where = new APPLY_PAY();
                    where.APP_ID = APP_ID;
                    model = dao.GetRow<APPLY_PAY>(where);
                    // 查無該筆APP_ID
                    if (model == null)
                    {
                        logger.Debug($"APPLY_PAY查無該筆APP_ID:{APP_ID}");
                        isFalse = true;
                    }
                    else if (model.PAY_METHOD == "C"/* && model.PAY_STATUS_MK == "Y"*/)
                    {
                        logger.Debug($"SessionKEY:{model.SESSION_KEY}");
                        isFalse = true;
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isFalse)
                {
                    return false;
                }
                // 付款帳號
                logger.Debug("GetPayAccountOID:" + model.OID);

                Dictionary<string, string> pay = DataUtils.GetPayAccountOID(model.OID);
                if (pay == null || pay.Count == 0)
                {
                    logger.Warn("CheckPayCard:pay is null");
                    return false;
                }

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    FormAction action = new FormAction(conn);
                    if (model.SESSION_KEY == null) { logger.Warn("##CheckPayCard, model.SESSION_KEY is Null!!"); }
                    if (model.SESSION_KEY != null) { logger.Debug("GetAccountingService:" + model.SESSION_KEY); }

                    ConfirmResponse res = CardUtils.GetAccountingService(model.SESSION_KEY, pay);
                    if (res == null)
                    {
                        logger.Warn("##CheckPayCard, res is Null!!");
                        isFalse = true;
                    }
                    else if (res.ResultInfo == null)
                    {
                        logger.Warn("##CheckPayCard, res.ResultInfo is Null!!");
                        isFalse = true;
                    }
                    if (isFalse)
                    {

                    }
                    else
                    {
                        logger.Debug("res.ResultInfo:" + res.ResultInfo);
                        data.Put("PAY_TRANS_RET", res.ResultInfo);
                        data.Put("APP_ID", model.APP_ID);
                        data.Put("PAY_SESSION_KEY", model.SESSION_KEY);

                        ConfirmResult[] result = res.ConfirmResults;
                        if (result != null && result.Length > 0)
                        {
                            logger.Debug("PASS_result_PAY_AUTH_DATE:" + result[0].AuthDate);
                            data.Put("PAY_AUTH_DATE", result[0].AuthDate);
                            data.Put("PAY_AUTH_NO", result[0].ApproveNo);
                            data.Put("PAY_SETTLE_DATE", result[0].SettleDate);
                            data.Put("PAY_HOST_TIME", result[0].HostTime);
                            data.Put("PAY_INFO_NO", result[0].InfoNO);
                            data.Put("PAY_OTHER", result[0].Others);
                            data.Put("PAY_TRANS_MSG", action.GetPayCodeDesc(data.Get("PAY_TRANS_RET")));
                        }
                        logger.Debug("PASS_result_PAY_TRANS_RET:" + data.Get("PAY_TRANS_RET"));
                        logger.Debug("PAY_A_FEE:" + model.PAY_MONEY.ToString());
                        data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("0000")) ? model.PAY_MONEY.ToString() : "0");
                        var upd_acc = GetAccount();
                        data.Put("ACC_NO", string.IsNullOrEmpty(upd_acc) ? "" : upd_acc);
                        data.Put("UPD_ACC", string.IsNullOrEmpty(upd_acc) ? "" : upd_acc);

                        if (dao.UpdateApplyPay(data.GetItem()))
                        {
                            logger.Debug("UpdateApplyPay_Success.APP_ID:" + APP_ID);
                            rst = true;
                        }
                        else
                        {
                            logger.Debug("UpdateApplyPay_Failure.APP_ID:" + APP_ID);
                            rst = false;
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isFalse)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new Exception("CheckPayCard failed:" + ex.Message, ex);
            }
            return rst;
        }

        /// <summary>
        /// 聯合信用卡中心信用卡勾稽
        /// </summary>
        /// <param name="APP_ID"></param>
        public bool CheckPayCardEC(string APP_ID)
        {
            bool rst = false;
            logger.Debug("HistoryController.Index.Model.Grid.APP_ID:" + APP_ID);
            MapUtils data = new MapUtils();
            PayDAO dao = new PayDAO();
            APPLY_PAY model = new APPLY_PAY();
            try
            {
                var isFalse = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    // 查詢
                    APPLY_PAY where = new APPLY_PAY();
                    where.APP_ID = APP_ID;
                    model = dao.GetRow<APPLY_PAY>(where);
                    // 查無該筆APP_ID
                    if (model == null)
                    {
                        logger.Debug($"APPLY_PAY查無該筆APP_ID:{APP_ID}");
                        isFalse = true;
                    }
                    else if (model.PAY_METHOD == "C" && model.PAY_STATUS_MK == "Y")
                    {
                        logger.Debug($"SessionKEY:{model.SESSION_KEY}");
                        isFalse = true;
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isFalse)
                {
                    return false;
                }
                // 付款帳號
                logger.Debug("GetPayAccountOID:" + model.OID);

                Dictionary<string, string> pay = DataUtils.GetPayAccountOID(model.OID);
                if (pay == null || pay.Count == 0)
                {
                    logger.Warn("CheckPayCardEC:pay is null");
                    return false;
                }

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    FormAction action = new FormAction(conn);
                    if (string.IsNullOrEmpty(model.SESSION_KEY)) { logger.Warn("##CheckPayCard, model.SESSION_KEY is Null!!"); }

                    CreditHPPModel ECmodel = new CreditHPPModel();
                    ECmodel.EncModel.MerchantID = DataUtils.GetConfig("PAY_EC_MERCHANTID");
                    ECmodel.EncModel.OrderID = model.ECORDERID;
                    ECmodel.ECConnetModel.DomainName = DataUtils.GetConfig("PAY_EC_DOMAINNAME");
                    ECmodel.ECConnetModel.RequestURL = DataUtils.GetConfig("PAY_EC_REQUESTURL");
                    ApiClient resEC = CardUtils.GetpostQueryEC(ECmodel, model.SESSION_KEY);
                    if (resEC == null)
                    {
                        logger.Warn("##CheckPayCardEC, res is Null!!");
                        isFalse = true;
                    }
                    if (isFalse)
                    {

                    }
                    else
                    {
                        data.Put("PAY_TRANS_RET", resEC.getRESPONSECODE());
                        data.Put("APP_ID", model.APP_ID);
                        data.Put("PAY_SESSION_KEY", model.SESSION_KEY);
                        data.Put("PAY_AUTH_DATE", resEC.getTRANSDATE());
                        data.Put("PAY_AUTH_NO", resEC.getAPPROVECODE());
                        data.Put("PAY_SETTLE_DATE", resEC.getTRANSDATE());
                        data.Put("PAY_HOST_TIME", resEC.getTRANSTIME());
                        data.Put("PAY_INFO_NO", resEC.getPAN());
                        data.Put("PAY_OTHER", resEC.getORDERID());
                        data.Put("PAY_TRANS_MSG", action.GetPayCodeDesc(data.Get("PAY_TRANS_RET")));
                        data.Put("PAY_ECORDERID", resEC.getORDERID());

                        logger.Debug("PASS_result_PAY_TRANS_RET:" + data.Get("PAY_TRANS_RET"));
                        logger.Debug("PAY_A_FEE:" + model.PAY_MONEY.ToString());
                        data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("00")) ? model.PAY_MONEY.ToString() : "0");

                        var upd_acc = GetAccount();
                        data.Put("ACC_NO", string.IsNullOrEmpty(upd_acc) ? "" : upd_acc);
                        data.Put("UPD_ACC", string.IsNullOrEmpty(upd_acc) ? "" : upd_acc);
                        // 適用已繳費且系統繳費狀態尚未更新
                        if (resEC.getRESPONSECODE() == "00" && resEC.getTRANSCODE() == "00")
                        {
                            if (dao.UpdateApplyPay(data.GetItem()))
                            {
                                logger.Debug("UpdateApplyPay_Success.APP_ID:" + APP_ID);
                                rst = true;
                            }
                            else
                            {
                                logger.Debug("UpdateApplyPay_Failure.APP_ID:" + APP_ID);
                                rst = false;
                            }
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isFalse)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new Exception("CheckPayCard failed:" + ex.Message, ex);
            }
            return rst;
        }
    }
}
