using ES.Action;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.Models;
using ES.Models.Entities;
using ES.Service.GSP1;
using ES.Service.GSP2;
using ES.Services;
using ES.Utils;
using HppApi;
using log4net;
using Omu.ValueInjecter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ES.DataLayers
{
    public class PayDAO : BaseAction
    {
        protected static readonly ILog logger = LogUtils.GetLogger();

        #region Tran
        /// <summary>
        /// 資料庫連線
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        public void Tran(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }
        #endregion


        #region 更新繳費資料
        /// <summary>
        /// 更新信用卡繳費回傳相關資料
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool UpdateApplyPay(Dictionary<string, string> data, bool isBackmin = false, string ECORDERID = "")
        {
            bool isUpdate = false;
            bool isSend = false;
            APPLY_PAY where_pay = new APPLY_PAY();
            APPLY_PAY update_pay = new APPLY_PAY();
            APPLY_PAY where_opay = new APPLY_PAY();
            APPLY_PAY qry_opay = new APPLY_PAY();
            ApplyModel where_apply = new ApplyModel();
            ApplyModel update_apply = new ApplyModel();
            ApplyModel where_oapply = new ApplyModel();
            ApplyModel qry_oapply = new ApplyModel();
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", data["APP_ID"]);
            dict2.Add("SRV_ID", data["APP_ID"].Substring(8, 6));
            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                FormAction action = new FormAction(conn, tran);

                try
                {
                    // 查詢原繳費狀態
                    where_opay.APP_ID = data["APP_ID"];
                    qry_opay = this.GetRow<APPLY_PAY>(where_opay);
                    // 查詢原主案件檔
                    where_oapply.APP_ID = data["APP_ID"];
                    qry_oapply = this.GetRow<ApplyModel>(where_oapply);

                    // 更新繳費
                    where_pay.APP_ID = data["APP_ID"];
                    if (/*data["PAY_METHOD"].Equals("C") &&*/ data["PAY_TRANS_RET"].Equals("0000") || data["PAY_TRANS_RET"].Equals("00"))
                    {
                        update_pay.PAY_EXT_TIME = DateTime.Now;//繳費時間
                        update_pay.PAY_INC_TIME = DateTime.Now;//異動時間
                    }
                    else
                    {
                        update_pay.PAY_EXT_TIME = null;
                        update_pay.PAY_INC_TIME = DateTime.Now;//異動時間
                    }
                    update_pay.TRANS_RET = data["PAY_TRANS_RET"].TONotNullString();
                    string formatString = "yyyyMMddHHmmss";
                    string PAY_AUTH_DATE = string.Empty;
                    string PAY_SETTLE_DATE = string.Empty;
                    string PAY_HOST_TIME = string.Empty;
                    if (update_pay.TRANS_RET == "0000" || update_pay.TRANS_RET == "00")
                    {
                        PAY_AUTH_DATE = data["PAY_AUTH_DATE"];
                        PAY_SETTLE_DATE = data["PAY_SETTLE_DATE"];
                        PAY_HOST_TIME = data["PAY_HOST_TIME"];
                        if (PAY_AUTH_DATE == null)
                        { update_pay.AUTH_DATE = DateTime.ParseExact(PAY_AUTH_DATE, formatString, null); }
                        if (PAY_SETTLE_DATE == null)
                        { update_pay.SETTLE_DATE = DateTime.ParseExact(PAY_SETTLE_DATE, formatString, null); }
                        if (PAY_HOST_TIME == null)
                        { update_pay.HOST_TIME = DateTime.ParseExact(PAY_HOST_TIME, formatString, null); }
                        update_pay.AUTH_NO = data["PAY_AUTH_NO"].TONotNullString();
                        update_pay.OTHER = data["PAY_OTHER"].TONotNullString();
                        update_pay.ECORDERID = ECORDERID.TONotNullString();
                    }
                    else
                    {
                        // 重新查詢交易結果，因有案例為第一次接收為交易成功後，實際上經E政府排程/檢核後，二次回傳結果為交易失敗
                    }
                    update_pay.PAY_RET_CD = data["PAY_TRANS_RET"].TONotNullString();
                    update_pay.PAY_RET_MSG = action.GetPayCodeDesc(update_pay.PAY_RET_CD);
                    update_pay.PAY_STATUS_MK = data["PAY_TRANS_RET"].TONotNullString().Equals("0000") ? "Y" : data["PAY_TRANS_RET"].TONotNullString().Equals("00") ? "Y" : "N";
                    update_pay.UPD_FUN_CD = "WEB-APPLY";

                    update_pay.UPD_ACC = data["UPD_ACC"].TONotNullString();
                    update_pay.APP_ID = data["APP_ID"].TONotNullString();
                    update_pay.SESSION_KEY = data["PAY_SESSION_KEY"].TONotNullString();


                    Update2(update_pay, where_pay, dict2, isBackmin);

                    // 繳費主檔更新 PAY_A_PAID == PAY_A_FEE 時狀態會顯示已繳費
                    where_apply.APP_ID = data["APP_ID"].TONotNullString();
                    update_apply.PAY_A_PAID = Convert.ToInt32(data["PAY_A_PAID"]);
                    update_apply.UPD_FUN_CD = "WEB-APPLY";
                    update_apply.UPD_ACC = data["UPD_ACC"].TONotNullString();
                    update_apply.APP_ID = data["APP_ID"].TONotNullString();

                    Update2(update_apply, where_apply, dict2, isBackmin);
                    isUpdate = true;
                    if (qry_opay != null && qry_opay.PAY_STATUS_MK == "Y" && update_pay.PAY_STATUS_MK == "N")
                    {
                        isSend = true;
                    }
                    tran.Commit();

                    // 交易結果失敗通知
                    if (isSend)
                    {
                        var mailrev = DataUtils.GetConfig("SERVICEMAIL"); // 系統收信服務信箱
                        var subject = "衛福部人民線上案件編號" + data["APP_ID"].TONotNullString() + "繳費成功異動";
                        var mailbody = $"您好，<br>案件編號:{data["APP_ID"].TONotNullString()}重新勾稽我的E政府繳費平台為交易失敗。<br>";
                        mailbody += "此封為繳費狀態更新通知信件，請勿直接回覆。";
                        if (!string.IsNullOrEmpty(mailrev))
                        {
                            foreach (var mailer in mailrev.ToSplit(','))
                            {
                                this.SendMail(mailer, subject, mailbody);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    logger.Error(ex.Message, ex);
                    isUpdate = false;
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return isUpdate;
        }

        /// <summary>
        /// 更新繳費日期
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public bool UpdateAPPLY_PAY(String[] APP_ID, string account)
        {
            bool result = false;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                try
                {
                    foreach (string temp in APP_ID)
                    {
                        if (!String.IsNullOrEmpty(temp))
                        {
                            //增加歷程，需要下列參數
                            Dictionary<string, object> dict2 = new Dictionary<string, object>();
                            dict2.Add("APP_ID", temp);
                            dict2.Add("SRV_ID", temp.Substring(8, 6));
                            dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
                            APPLY_PAY where = new APPLY_PAY();
                            where.APP_ID = temp;
                            APPLY_PAY data = new APPLY_PAY();
                            data.APP_ID = temp;
                            data.SETTLE_DATE = DateTime.Now;
                            data.UPD_ACC = account;
                            data.UPD_FUN_CD = "WEB-PAY";
                            data.UPD_TIME = DateTime.Now;
                            Update2(data, where, dict2, true);
                        }
                    }
                    result = true;
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 更新繳費方式
        /// </summary>
        /// <param name="paymethod_org"></param>
        /// <param name="app_id"></param>
        /// <param name="tx_id"></param>
        /// <returns></returns>
        public bool UpdateAPPLY_PAY_PAYMETHOD(String paymethod_org, string app_id, string tx_id, string account)
        {
            bool result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                try
                {
                    //增加歷程，需要下列參數
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", app_id);
                    dict2.Add("SRV_ID", app_id.Substring(8, 6));
                    dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    APPLY_PAY where = new APPLY_PAY();
                    where.APP_ID = app_id;
                    APPLY_PAY data = new APPLY_PAY();
                    data.PAY_METHOD = paymethod_org;
                    data.PAY_INC_TIME = DateTime.Now;
                    data.APP_ID = app_id;
                    data.PAY_ID = tx_id;
                    data.UPD_ACC = account;
                    data.UPD_FUN_CD = "WEB-PAY";
                    data.UPD_TIME = DateTime.Now;
                    Update2(data, where, dict2, true);

                    tran.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 新增繳費資料
        /// </summary>
        /// <param name="app_id"></param>
        /// <param name="tx_id"></param>
        /// <param name="pay_money_new"></param>
        /// <param name="paymethod_new"></param>
        /// <returns></returns>
        public bool InsertAPPLY_PAY(string app_id, string tx_id, string pay_money_new, string paymethod_new, string account)
        {
            bool result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                try
                {
                    APPLY_PAY data = new APPLY_PAY();
                    data.APP_ID = app_id;
                    data.PAY_ID = tx_id;
                    data.PAY_MONEY = pay_money_new.TOInt32();
                    data.PAY_ACT_TIME = DateTime.Now;
                    data.PAY_EXT_TIME = DateTime.Now;
                    data.PAY_INC_TIME = DateTime.Now;
                    data.PAY_STATUS_MK = "Y";
                    data.PAY_METHOD = paymethod_new;
                    data.UPD_ACC = account;
                    data.UPD_FUN_CD = "WEB-PAY";
                    data.UPD_TIME = DateTime.Now;
                    data.ADD_ACC = account;
                    data.ADD_FUN_CD = "WEB-PAY";
                    data.ADD_TIME = DateTime.Now;
                    Insert(data);

                    tran.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 判斷審核後已繳金額是否異動
        /// </summary>
        /// <param name="app_id"></param>
        /// <param name="pay_cexpect_db"></param>
        /// <returns></returns>
        public bool UpdateAPPLY_PAY_C_PAID(string app_id, string pay_cexpect_db, string account)
        {
            bool result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                try
                {
                    //增加歷程，需要下列參數
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", app_id);
                    dict2.Add("SRV_ID", app_id.Substring(8, 6));
                    dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    ApplyModel where = new ApplyModel();
                    where.APP_ID = app_id;
                    ApplyModel data = new ApplyModel();
                    data.PAY_C_PAID = pay_cexpect_db.TOInt32();
                    data.APP_ID = app_id;
                    data.UPD_ACC = account;
                    data.UPD_FUN_CD = "WEB-PAY";
                    data.UPD_TIME = DateTime.Now;

                    Update2(data, where, dict2, true);
                    tran.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// 判斷審核前已繳金額是否異動
        /// </summary>
        /// <param name="app_id"></param>
        /// <param name="pay_aactual_new"></param>
        /// <returns></returns>
        public bool UpdateAPPLY_PAY_A_PAID(string app_id, int pay_aactual_new, string account)
        {
            bool result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);

                try
                {
                    //增加歷程，需要下列參數
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2.Add("APP_ID", app_id);
                    dict2.Add("SRV_ID", app_id.Substring(8, 6));
                    dict2.Add("LastMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    ApplyModel where = new ApplyModel();
                    where.APP_ID = app_id;
                    ApplyModel data = new ApplyModel();
                    data.PAY_A_PAID = pay_aactual_new.TOInt32();
                    data.APP_ID = app_id;
                    data.UPD_ACC = account;
                    data.UPD_FUN_CD = "WEB-PAY";
                    data.UPD_TIME = DateTime.Now;

                    Update2(data, where, dict2, true);
                    tran.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        #endregion

        #region 信用卡重新繳費
        public APPLY_PAY GetBeforRePaySessionKey(RePayCreditFormModel model)
        {
            var result = new APPLY_PAY();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = GetRow<APPLY_PAY>(new APPLY_PAY() { APP_ID = model.APP_ID });
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        public string CheckCreditStatus(RePayCreditFormModel model, APPLY_PAY payModel)
        {
            var results = string.Empty;
            MapUtils data = new MapUtils();
            PayDAO dao = new PayDAO();
            // 付款帳號
            Dictionary<string, string> pay = DataUtils.GetPayAccountOID(payModel.OID);
            if (pay == null)
            {
                logger.Warn("CheckPayCard:pay is null");
                results = "1000";
            }
            else
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    FormAction action = new FormAction(conn);
                    logger.Debug("GetAccountingService:" + payModel.SESSION_KEY);
                    ConfirmResponse res = CardUtils.GetAccountingService(payModel.SESSION_KEY, pay);
                    logger.Debug("res.ResultInfo:" + res.ResultInfo);
                    data.Put("PAY_TRANS_RET", res.ResultInfo);
                    data.Put("APP_ID", model.APP_ID);
                    // ResultInfo1002:無交易紀錄
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

                        data.Put("PAY_TRANS_RET", result[0].TransactionResult);
                        data.Put("PAY_TRANS_MSG", action.GetPayCodeDesc(data.Get("PAY_TRANS_RET")));
                    }
                    logger.Debug("PASS_result_PAY_TRANS_RET:" + data.Get("PAY_TRANS_RET"));
                    data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("0000")) ? data.Get("PAY_A_FEE") : "0");
                    data.Put("ACC_NO", model.ACC_NO);
                    data.Put("UPD_ACC", string.IsNullOrEmpty(model.UPD_ACC) ? "" : model.UPD_ACC);

                    if (dao.UpdateApplyPay(data.GetItem()))
                    {
                        logger.Debug("UpdateApplyPay_Success.APP_ID:" + model.APP_ID);
                        results = data.Get("PAY_TRANS_RET");
                    }
                    else
                    {
                        logger.Debug("UpdateApplyPay_Failure.APP_ID:" + model.APP_ID);
                        results = data.Get("PAY_TRANS_RET");
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }

            return results;
        }
        public string CheckCreditStatusEC(RePayCreditFormModel model, APPLY_PAY payModel)
        {
            var results = string.Empty;
            MapUtils data = new MapUtils();
            PayDAO dao = new PayDAO();
            ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;
            if (string.IsNullOrEmpty(payModel.SESSION_KEY))
            {
                logger.Warn("CheckPayCard:SESSION_KEY is null");
                results = "1000";
            }
            else
            {
                CreditHPPModel ECmodel = new CreditHPPModel();
                ECmodel.EncModel.MerchantID = DataUtils.GetConfig("PAY_EC_MERCHANTID");
                if (model.APP_ID.Substring(8, 6).Equals("001036"))
                {
                    ECmodel.EncModel.TerminalID = DataUtils.GetConfig("PAY_EC_TRMINALID2");
                }
                else
                {
                    ECmodel.EncModel.TerminalID = DataUtils.GetConfig("PAY_EC_TRMINALID");
                }
                ECmodel.EncModel.OrderID = payModel.ECORDERID;
                ECmodel.ECConnetModel.DomainName = DataUtils.GetConfig("PAY_EC_DOMAINNAME");
                ECmodel.ECConnetModel.RequestURL = DataUtils.GetConfig("PAY_EC_REQUESTURL");
                // 查詢狀態
                ApiClient apiClient = CardUtils.GetpostQueryEC(ECmodel, payModel.SESSION_KEY);

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    FormAction action = new FormAction(conn);
                    data.Put("PAY_TRANS_RET", apiClient.ResponseCode);
                    data.Put("APP_ID", model.APP_ID);
                    if (apiClient != null)
                    {
                        logger.Debug("PASS_result_PAY_AUTH_DATE:" + apiClient.getTRANSDATE() + " " + apiClient.getTRANSTIME());
                        data.Put("PAY_AUTH_DATE", apiClient.getTRANSDATE());
                        data.Put("PAY_AUTH_NO", apiClient.getAPPROVECODE());
                        data.Put("PAY_SETTLE_DATE", apiClient.getTRANSDATE());
                        data.Put("PAY_HOST_TIME", apiClient.getTRANSTIME());
                        data.Put("PAY_INFO_NO", apiClient.getPAN());
                        data.Put("PAY_OTHER", apiClient.getORDERID());

                        data.Put("PAY_TRANS_RET", apiClient.getRESPONSECODE());
                        data.Put("PAY_TRANS_MSG", apiClient.getRESPONSEMSG());
                        data.Put("PAY_SESSION_KEY", apiClient.getKEY());
                    }
                    logger.Debug("PASS_result_PAY_TRANS_RET:" + data.Get("PAY_TRANS_RET"));
                    data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("00")) ? data.Get("PAY_A_FEE") : "0");
                    data.Put("ACC_NO", model.ACC_NO);
                    data.Put("UPD_ACC", string.IsNullOrEmpty(model.UPD_ACC) ? "" : model.UPD_ACC);

                    if (dao.UpdateApplyPay(data.GetItem()))
                    {
                        logger.Debug("UpdateApplyPay_Success.APP_ID:" + model.APP_ID);
                        results = data.Get("PAY_TRANS_RET");
                    }
                    else
                    {
                        logger.Debug("UpdateApplyPay_Failure.APP_ID:" + model.APP_ID);
                        results = data.Get("PAY_TRANS_RET");
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }

            return results;
        }
        public string RePayUpdateApplyPay(RePayCreditFormModel model)
        {
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo;
            var result = string.Empty;
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", model.SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    FormAction action = new FormAction(conn, tran);
                    model.PAY_A_FEE = GetRow<ApplyModel>(new ApplyModel() { APP_ID = model.APP_ID })?.PAY_A_FEE;
                    Dictionary<string, string> form = action.GetFormBase(model.SRV_ID);
                    MapUtils data = new MapUtils();

                    data.Put("PAY_METHOD", "C");
                    data.Put("PAY_CARD_IDN", model.CARD_IDN == null ? "" : model.CARD_IDN);
                    data.Put("PAY_A_FEEBK", "0");
                    data.Put("APP_FEE", model.PAY_A_FEE.ToString());
                    data.Put("PAY_ID", model.APP_ID);
                    data.Put("PAY_CLIENT_IP", model.CLIENT_IP);

                    Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(form["PAY_ACCOUNT"]));
                    data.Put("PAY_OID", pay["OID"]);
                    data.Put("PAY_SID", pay["SID"]);

                    SessionKeyResponse res = CardUtils.GetTransactionKey(data.GetItem(), pay);
                    logger.Debug("RePayUpdateApplyPay.CardUtils.GetTransactionKey:APP_ID" + model.APP_ID + "CARDIDN:" + model.CARD_IDN);
                    if (res != null)
                    {
                        model.ErrorCode = res.ResultInfo;
                        model.ErrorMessage = action.GetPayCodeDesc(res.ResultInfo);
                        if (res.ResultInfo.Equals("0000"))
                        {
                            string sessionKey = res.SessionTransactionKey;
                            data.Put("PAY_SESSION_KEY", sessionKey);
                            logger.Debug("SessionTransactionKey: " + sessionKey);
                            model.SessionTransactionKey = sessionKey;
                            ApplyModel where_app = new ApplyModel();
                            where_app.APP_ID = model.APP_ID;
                            ApplyModel apply = new ApplyModel();
                            apply.CARD_IDN = model.CARD_IDN;
                            apply.UPD_TIME = DateTime.Now;
                            apply.UPD_FUN_CD = "WEB-APPLY";
                            apply.UPD_ACC = UserInfo.UserNo.TONotNullString();
                            this.Update2(apply, where_app, dict2);

                            APPLY_PAY where_pay = new APPLY_PAY();
                            where_pay.APP_ID = model.APP_ID;

                            APPLY_PAY applyPay = new APPLY_PAY();
                            applyPay.APP_ID = model.APP_ID;
                            applyPay.OID = data.GetItem()["PAY_OID"];   // 機關代碼
                            applyPay.SID = data.GetItem()["PAY_SID"];   // 服務代碼
                            applyPay.PAY_ID = model.APP_ID;
                            applyPay.PAY_METHOD = "C";
                            applyPay.PAY_STATUS_MK = "N";
                            applyPay.PAY_MONEY = model.PAY_A_FEE;
                            applyPay.PAY_PROFEE = 0;
                            applyPay.SESSION_KEY = model.SessionTransactionKey;
                            //applyPay.PAY_ACT_TIME = DateTime.Now;//新增時間
                            //applyPay.PAY_EXT_TIME = DateTime.Now;//繳費時間
                            applyPay.PAY_INC_TIME = DateTime.Now;//異動時間
                            applyPay.CLIENT_IP = model.CLIENT_IP;
                            //applyPay.ADD_TIME = DateTime.Now;
                            //applyPay.ADD_FUN_CD = "WEB-APPLY";
                            //applyPay.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                            applyPay.UPD_TIME = DateTime.Now;
                            applyPay.UPD_FUN_CD = "WEB-APPLY";
                            applyPay.UPD_ACC = UserInfo.UserNo.TONotNullString();
                            applyPay.DEL_MK = "N";
                            this.Update2(applyPay, where_pay, dict2);
                            result += $"{model.ErrorCode}-{model.SessionTransactionKey}";
                            tran.Commit();
                        }
                        else
                        {
                            logger.Error($"{model.ErrorCode}-{model.ErrorMessage}");
                            result += $"{model.ErrorCode}-{model.ErrorMessage}";
                            tran.Rollback();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    logger.Debug(ex.Message, ex);
                    result += ex.Message;
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }
        public string RePayUpdateApplyPayEC(RePayCreditFormModel model)
        {
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo;
            var result = string.Empty;
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", model.APP_ID);
            dict2.Add("SRV_ID", model.SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);
            ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    FormAction action = new FormAction(conn, tran);

                    MapUtils data = new MapUtils();
                    Dictionary<string, string> form = action.GetFormBase(model.SRV_ID);
                    Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(form["PAY_ACCOUNT"]));
                    data.Put("PAY_OID", pay["OID"]);
                    data.Put("PAY_SID", pay["SID"]);
                    data.Put("PAY_METHOD", "C");
                    data.Put("PAY_CARD_IDN", model.CARD_IDN == null ? "" : model.CARD_IDN);
                    data.Put("PAY_A_FEEBK", "0");
                    data.Put("APP_FEE", model.PAY_A_FEE.ToString());
                    data.Put("PAY_ID", model.APP_ID);
                    data.Put("PAY_CLIENT_IP", model.CLIENT_IP);

                    model.PAY_A_FEE = GetRow<ApplyModel>(new ApplyModel() { APP_ID = model.APP_ID })?.PAY_A_FEE;
                    var ECmodel = new CreditHPPModel();
                    ECmodel.EncModel.MerchantID = DataUtils.GetConfig("PAY_EC_MERCHANTID");
                    ECmodel.EncModel.OrderID = "";
                    if (model.SRV_ID == "001036")
                    {
                        ECmodel.EncModel.TerminalID = DataUtils.GetConfig("PAY_EC_TRMINALID2");
                    }
                    else
                    {
                        ECmodel.EncModel.TerminalID = DataUtils.GetConfig("PAY_EC_TRMINALID");
                    }
                    ECmodel.EncModel.TransAmt = Convert.ToString(model.PAY_A_FEE);
                    ECmodel.EncModel.TransMode = "0";
                    ECmodel.EncModel.Template = "BOTH";
                    ECmodel.EncModel.CardholderName = "";
                    ECmodel.EncModel.CardholderEmailAddress = "";
                    ECmodel.EncModel.IDNUMBER = Convert.ToString(model.CARD_IDN); //持卡人身份證字號
                    ECmodel.EncModel.PrivateData = "ClientIP=" + model.CLIENT_IP + "&APPID=" + model.APP_ID + "&SRVID=" + model.SRV_ID; //自訂資料
                    ECmodel.ECConnetModel.DomainName = DataUtils.GetConfig("PAY_EC_DOMAINNAME");
                    ECmodel.ECConnetModel.RequestURL = DataUtils.GetConfig("PAY_EC_REQUESTURL");
                    //*回應網址
                    ECmodel.EncModel.NotifyURL = DataUtils.GetConfig("PAY_EC_NOTIFYURL");
                    logger.Debug("RePayUpdateApplyPayEC.NotifyURL" + ECmodel.EncModel.NotifyURL);

                    //聯合信用卡
                    ApiClient resEC = CardUtils.GetTransactionKeyEC(ECmodel);
                    logger.Debug("RePayUpdateApplyPayEC.CardUtils.GetTransactionKey:APP_ID" + model.APP_ID + "CARDIDN:" + model.CARD_IDN);
                    if (resEC != null)
                    {
                        model.ErrorCode = resEC.ResponseCode;
                        model.ErrorMessage = resEC.ResponseMsg;
                        if (resEC.ResponseCode.Equals("00"))
                        {
                            string sessionKey = resEC.KEY;
                            logger.Debug("SessionTransactionKey: " + sessionKey);
                            model.SessionTransactionKey = sessionKey;
                            ApplyModel where_app = new ApplyModel();
                            where_app.APP_ID = model.APP_ID;
                            ApplyModel apply = new ApplyModel();
                            apply.CARD_IDN = model.CARD_IDN;
                            apply.UPD_TIME = DateTime.Now;
                            apply.UPD_FUN_CD = "WEB-APPLY";
                            apply.UPD_ACC = UserInfo.UserNo.TONotNullString();
                            this.Update2(apply, where_app, dict2);

                            APPLY_PAY where_pay = new APPLY_PAY();
                            where_pay.APP_ID = model.APP_ID;

                            APPLY_PAY applyPay = new APPLY_PAY();
                            applyPay.APP_ID = model.APP_ID;
                            applyPay.OID = data.GetItem()["PAY_OID"];   // 機關代碼
                            applyPay.SID = data.GetItem()["PAY_SID"];   // 服務代碼
                            applyPay.PAY_ID = model.APP_ID;
                            applyPay.PAY_METHOD = "C";
                            applyPay.PAY_STATUS_MK = "N";
                            applyPay.PAY_MONEY = model.PAY_A_FEE;
                            applyPay.PAY_PROFEE = 0;
                            applyPay.SESSION_KEY = sessionKey;
                            //applyPay.PAY_ACT_TIME = DateTime.Now;//新增時間
                            //applyPay.PAY_EXT_TIME = DateTime.Now;//繳費時間
                            applyPay.PAY_INC_TIME = DateTime.Now;//異動時間
                            applyPay.CLIENT_IP = model.CLIENT_IP;
                            //applyPay.ADD_TIME = DateTime.Now;
                            //applyPay.ADD_FUN_CD = "WEB-APPLY";
                            //applyPay.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                            applyPay.UPD_TIME = DateTime.Now;
                            applyPay.UPD_FUN_CD = "WEB-APPLY";
                            applyPay.UPD_ACC = UserInfo.UserNo.TONotNullString();
                            applyPay.DEL_MK = "N";
                            applyPay.PAY_BANK = "EC"; //聯合信用
                            applyPay.ECORDERID = resEC.OrderID; // EC datetime.tick
                            this.Update2(applyPay, where_pay, dict2);
                            result += $"{model.ErrorCode}-{resEC.KEY}";
                            tran.Commit();
                        }
                        else
                        {
                            logger.Error($"{model.ErrorCode}-{model.ErrorMessage}");
                            result += $"{model.ErrorCode}-{model.ErrorMessage}";
                            tran.Rollback();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message, ex);
                    result += ex.Message;
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }

        #endregion

        #region 通用寄信功能

        /// <summary>
        /// 通用寄信功能        
        /// </summary>
        /// <param name="Mail">信箱</param>
        /// <param name="Subject">主旨</param>
        /// <param name="Body">內容</param>
        /// 
        public void SendMail(string Mail, string Subject, string Body, string ServiceId = "")
        {
            ShareDAO dao = new ShareDAO();
            // 寄信LOG
            TblMAIL_LOG log = new TblMAIL_LOG();
            log.MAIL = Mail;
            log.SUBJECT = Subject;
            log.BODY = Body;
            log.SEND_TIME = DateTime.Now;
            log.SRV_ID = ServiceId;

            if (this.conn == null)
            {
                this.conn = DataUtils.GetConnection();
                this.conn.Open();
            }

            try
            {
                if (ConfigModel.MailRevTest == "1")
                {
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, ConfigModel.MailRevAddr1, Subject, Body);
                    mailMessage.IsBodyHtml = true;
                    CommonsServices.SendMail(mailMessage);
                    if (ConfigModel.MailRevIsTwo == "1")
                    {
                        var recList = ConfigModel.MailRevAddr2.ToSplit(',');
                        foreach (var rec in recList)
                        {
                            mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, rec, Subject, Body);
                            mailMessage.IsBodyHtml = true;
                            CommonsServices.SendMail(mailMessage);
                        }
                    }
                }
                else
                {
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Mail, Subject, Body);
                    mailMessage.IsBodyHtml = true;
                    CommonsServices.SendMail(mailMessage);
                }

                // 寄信成功
                log.RESULT_MK = "Y";
                Insert(log);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
                // 寄信失敗
                log.RESULT_MK = "N";
                Insert(log);
            }

        }

        /// <summary>
        /// 通用寄信功能        
        /// </summary>
        /// <param name="Mail">信箱</param>
        /// <param name="Subject">主旨</param>
        /// <param name="Body">內容</param>
        /// 
        public void SendMailAttch(string Mail, string Subject, string Body, MemoryStream stream)
        {
            ShareDAO dao = new ShareDAO();
            // 寄信LOG
            TblMAIL_LOG log = new TblMAIL_LOG();
            log.MAIL = Mail;
            log.SUBJECT = Subject;
            log.BODY = Body;
            log.SEND_TIME = DateTime.Now;

            if (this.conn == null)
            {
                this.conn = DataUtils.GetConnection();
                this.conn.Open();
            }

            try
            {
                if (ConfigModel.MailRevTest == "1")
                {
                    // 測試機
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, "預設信箱", Subject, Body);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Attachments.Add(new Attachment(stream, $"2022春節專案每日名額及分配-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xls", "application/unknown"));
                    CommonsServices.SendMail(mailMessage);
                }
                else
                {
                    // 正式機
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, "預設信箱", Subject, Body);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Attachments.Add(new Attachment(stream, $"2022春節專案每日名額及分配-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xls", "application/unknown"));
                    CommonsServices.SendMail(mailMessage);
                    // 專案經理
                    MailMessage mailMessage2 = CommonsServices.NewMail(ConfigModel.MailSenderAddr, "預設信箱", Subject, Body);
                    mailMessage2.IsBodyHtml = true;
                    mailMessage2.Attachments.Add(new Attachment(stream, $"2022春節專案每日名額及分配-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.xls", "application/unknown"));
                    CommonsServices.SendMail(mailMessage2);
                }

                // 寄信成功
                log.RESULT_MK = "Y";
                Insert(log);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
                // 寄信失敗
                log.RESULT_MK = "N";
                Insert(log);
            }

        }
        #endregion

        #region 玉山 單筆查詢更新繳費資料
        /// <summary>
        /// 更新繳費資料ono, record, rrn, air, GUID, ltd, ltt, Convert.ToDateTime(ltd + " " + ltt)
        /// "回覆碼:{rc},特店代碼:{mid},訂單編號:{ono},收單交易日期:{ltd},收單交易時間:{ltt},
        /// 簽帳單序號:{rrn},授權碼:{air},交易金額:{txnamount},剩餘消費金額:{settleamount},訂單狀態:{settlestatus},卡號:{an}"
        /// </summary>
        /// <param name="orderNo">訂單編號 ono</param>
        /// <param name="URLEnc">回傳字串</param>
        /// <param name="XID">簽帳單序號</param>
        /// <param name="traceNO">授權碼</param>
        /// <param name="Guid">訂房編號</param>
        /// <param name="LTD">訂單日期</param>
        /// <param name="LTT">訂單時間</param>
        /// <param name="paydate"></param>
        /// <returns></returns>
        public bool UpdateCreditSuccessRecvGuid(string orderNo, string URLEnc, string XID, string traceNO, string Guid, string LTD, string LTT, DateTime? paydate, string sprcode = "")
        {
            var isStatus = string.IsNullOrEmpty(Guid) ? "N" : "Y";
            bool isSuccess = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo;
            var username = string.Empty;
            username = UserInfo == null ? "SYSTEM" : UserInfo.UserNo;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    TblFLYPAYBASIC where = new TblFLYPAYBASIC();
                    where.PAYRESULT = orderNo;
                    TblFLYPAYBASIC data = new TblFLYPAYBASIC();
                    data.PAYDATE = paydate;
                    data.PAYRETURN = URLEnc;
                    data.XID = XID;
                    data.TRACENO = traceNO;
                    data.STATUS = isStatus;
                    data.GUID = Guid;
                    data.RC = "00";
                    data.UPD_TIME = DateTime.Now;
                    data.UPD_ACC = username;
                    data.SPRCODE = sprcode;
                    Update<TblFLYPAYBASIC>(data, where);
                    isSuccess = true;
                    logger.Debug($"UpdateCreditSuccessRecvGuid:isSuccess:true");
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error($"UpdateCreditSuccessRecvGuid:orderNo_{ex.Message}", ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return isSuccess;
        }
        /// <summary>
        /// 防疫旅館春節專案查詢
        /// </summary>
        /// <returns></returns>
        public List<string> QueryEsunPayResultList_SPR()
        {
            var list = new List<string>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    // 查詢未繳費的訂單編號
                    TblFLYPAYBASIC where = new TblFLYPAYBASIC();
                    where.BANKTYPE = "2";
                    where.STATUS = "N";
                    IList<TblFLYPAYBASIC> tbl = this.GetRowList<TblFLYPAYBASIC>(where);
                    if (tbl != null && tbl.Count > 0)
                    {
                        var newtbl = tbl.Where(x => x.FLYTYPE.TOInt32() > 3 && x.GUID.TONotNullString() == "").ToList();
                        foreach (var item in newtbl)
                        {
                            if (DateTime.Now >= item.ADD_TIME.Value.AddHours(1))
                            {
                                // 逾時操作60分鐘
                                // 春節專案
                                //TblFLYPAYBASICSPR whereSpr = new TblFLYPAYBASICSPR();
                                //whereSpr.FLY_ID = item.FLY_ID;
                                //whereSpr.ISUSE = "Y";
                                //var sprData = this.GetRow<TblFLYPAYBASICSPR>(whereSpr);
                                //if (sprData != null && sprData.SPR_ID > 0)
                                {
                                    // 準備釋放的名額資料
                                    list.Add(item.FLY_ID.ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"QueryEsunPayResultList_SPR:{ex.Message}", ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return list;
        }


        /// <summary>
        /// 防疫旅館一般訂房查詢
        /// </summary>
        /// <returns></returns>
        public List<string> QueryEsunPayResultList()
        {
            var list = new List<string>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    // 查詢未繳費的訂單編號
                    TblFLYPAYBASIC where = new TblFLYPAYBASIC();
                    where.BANKTYPE = "2";
                    where.FLYTYPE = "3";
                    where.STATUS = "N";
                    IList<TblFLYPAYBASIC> tbl = this.GetRowList<TblFLYPAYBASIC>(where);
                    if (tbl != null && tbl.Count > 0)
                    {
                        foreach (var item in tbl.Where(x => (Convert.ToInt32(x.FLIGHTDATE.Replace("-", "").Replace("/", "")) >= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")))).ToList())
                        {
                            // 一般訂房
                            list.Add(item.PAYRESULT);
                        }
                        logger.Debug("QueryEsunPayResultList:Count" + list.Count);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"QueryEsunPayResultList:{ex.Message}", ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return list;
        }
        #endregion

        #region  春節專案 查詢預定名額
        /// <summary>
        /// 春節專案 釋放額度
        /// </summary>
        /// <param name="fly_id"></param>
        /// <returns></returns>
        public bool UpdateSPR_isUse(string fly_id)
        {
            bool isSuccess = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo;
            var username = string.Empty;
            username = UserInfo == null ? "SYSTEM" : UserInfo.UserNo;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    TblFLYPAYBASICSPR where = new TblFLYPAYBASICSPR();
                    where.FLY_ID = fly_id.TOInt32();
                    TblFLYPAYBASICSPR data = new TblFLYPAYBASICSPR();
                    data.ISUSE = "N";
                    data.UPD_TIME = DateTime.Now;
                    data.UPD_ACC = username;
                    Update<TblFLYPAYBASICSPR>(data, where);
                    isSuccess = true;
                    logger.Debug($"UpdateSPR_isUse:ISUSE:N,FLY_ID:{fly_id}");
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error($"UpdateSPR_isUse:fly_id:{fly_id}_{ex.Message}", ex);
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return isSuccess;
        }

        public string UpdateSPRNum(FlyPaySPRViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            var sprcode = string.Empty;
            var UserInfo = sm.UserInfo;
            var username = string.Empty;
            username = UserInfo == null ? "SYSTEM" : UserInfo.UserNo;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    // 子項 集檢所
                    var secList = new List<TblFLYPAYROOMS_DE>();
                    TblFLYPAYROOMS_DE where = new TblFLYPAYROOMS_DE();
                    where.SEC = model.SECTION;
                    where.SE_DAY = model.DAYCODE;
                    secList = GetRowList<TblFLYPAYROOMS_DE>(where).ToList();
                    if (secList.ToCount() > 0)
                    {
                        foreach (var item in secList)
                        {
                            if (string.IsNullOrEmpty(sprcode))
                            {
                                TblFLYPAYSE where_se = new TblFLYPAYSE();
                                where_se.ECHELON = model.ECHELON;
                                where_se.DAYCODE = item.SE_DAY;
                                where_se.SECTION = model.SECTION;
                                where_se.SE_CODE = item.SE_CODE;
                                var ucnt = this.GetRow<TblFLYPAYSE>(where_se);
                                if (ucnt == null || ucnt.ID.TOInt32() <= 0)
                                {
                                    TblFLYPAYSE insData = new TblFLYPAYSE();
                                    insData.DAYCODE = item.SE_DAY;
                                    insData.ECHELON = model.ECHELON;
                                    insData.SECTION = model.SECTION;
                                    insData.SE_CODE = item.SE_CODE;
                                    insData.UCNT = 1;
                                    this.Insert<TblFLYPAYSE>(insData);
                                    sprcode = $"{item.SEC}{item.SE_CODE}{model.ECHELON.PadLeft(2, '0')}{item.SE_DAY}{insData.UCNT.TONotNullString().PadLeft(2, '0')}";
                                }
                                else if (ucnt != null && ucnt.UCNT >= item.ROOMS)
                                {
                                    // 已超過
                                    continue;
                                }
                                else if (ucnt != null && ucnt.UCNT > 0)
                                {
                                    TblFLYPAYSE where_nse = new TblFLYPAYSE();
                                    where_nse.ID = ucnt.ID;
                                    TblFLYPAYSE upData = new TblFLYPAYSE();
                                    upData.UCNT = ucnt.UCNT + 1;
                                    this.Update<TblFLYPAYSE>(upData, where_nse);
                                    sprcode = $"{item.SEC}{item.SE_CODE}{model.ECHELON.PadLeft(2, '0')}{item.SE_DAY}{upData.UCNT.TONotNullString().PadLeft(2, '0')}";
                                }
                            }
                        }

                        TblFLYPAYBASIC where_bas = new TblFLYPAYBASIC();
                        where_bas.FLY_ID = model.FLY_ID;
                        TblFLYPAYBASIC updBas = new TblFLYPAYBASIC();
                        updBas.SPRCODE = sprcode;
                        this.Update<TblFLYPAYBASIC>(updBas, where_bas);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Error($"UpdateSPRNum:sprcode:{sprcode}:{ex.Message}", ex);
                    tran.Rollback();
                    sprcode = string.Empty;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return sprcode;
        }

        #endregion

        /// <summary>
        /// 取得 額滿資訊
        /// </summary>
        /// <returns></returns>
        public List<Hashtable> getAddtime(string ono)
        {
            List<Hashtable> result = new List<Hashtable>();
            StringBuilder querySQL = new StringBuilder(@"
SELECT CONVERT(varchar,ADD_TIME,20) as ADD_TIME FROM FLYPAYBASIC
WHERE PAYRESULT = @PAYRESULT

            ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            //成功後回寫繳費時間
            DataUtils.AddParameters(com, "PAYRESULT", ono);

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    Hashtable ht = new Hashtable();

                    ht["ADD_TIME"] = Convert.ToString(sr["ADD_TIME"]);
                    result.Add(ht);
                }
                sr.Close();
            }
            return result;
        }

        #region 聯合信用卡
        /// <summary>
        /// 取得案件編號
        /// </summary>
        /// <param name="SessionKey"></param>
        /// <returns></returns>
        public string GetAPPbySessionKey(string SessionKey)
        {
            var result = string.Empty;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                var model = GetRow<APPLY_PAY>(new APPLY_PAY() { SESSION_KEY = SessionKey });
                if (model != null && !string.IsNullOrEmpty(model.APP_ID))
                {
                    result = model.APP_ID;
                }
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        /// <summary>
        /// 更新繳費狀態
        /// </summary>
        /// <param name="model"></param>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public bool UpdatePayResultEC(CreditHPPModel model, string KEY, bool isBackmin = false)
        {
            bool result = false;
            SessionModel sm = SessionModel.Get();
            var UserInfo = sm.UserInfo;
            string LastMODTIME = DateTime.Now.ToString("yyyyMMddHHmmss");
            var listPrivateData = model.DecModel.PrivateData.ToSplit("&"); // ClientIP,APPID,SRVID
            var CLIENT_IP = listPrivateData[0].ToUpper().Replace("CLIENTIP=", "");
            var APP_ID = listPrivateData[1].ToUpper().Replace("APPID=", "");
            var SRV_ID = listPrivateData[2].ToUpper().Replace("SRVID=", "");
            ApplyModel where_apply = new ApplyModel();
            ApplyModel update_apply = new ApplyModel();
            //增加歷程，需要下列參數
            Dictionary<string, object> dict2 = new Dictionary<string, object>();
            dict2.Add("APP_ID", APP_ID);
            dict2.Add("SRV_ID", SRV_ID);
            dict2.Add("LastMODTIME", LastMODTIME);

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                this.Tran(conn, tran);
                try
                {
                    FormAction action = new FormAction(conn, tran);

                    if (model.DecModel.RESPONSECODE.Equals("00"))
                    {
                        APPLY_PAY where_pay = new APPLY_PAY();
                        where_pay.APP_ID = APP_ID;

                        APPLY_PAY applyPay = new APPLY_PAY();
                        applyPay.APP_ID = APP_ID;
                        //applyPay.OID = data.GetItem()["PAY_OID"];   // 機關代碼
                        //applyPay.SID = data.GetItem()["PAY_SID"];   // 服務代碼
                        applyPay.PAY_ID = APP_ID;
                        applyPay.PAY_METHOD = "C";
                        applyPay.PAY_STATUS_MK = "Y";
                        applyPay.PAY_MONEY = model.DecModel.TRANSAMT.TOInt32();
                        applyPay.PAY_PROFEE = 0;
                        applyPay.SESSION_KEY = KEY;
                        applyPay.ECORDERID = model.DecModel.OrderID;
                        applyPay.PAY_RET_CD = model.DecModel.RESPONSECODE;
                        applyPay.PAY_RET_MSG = model.DecModel.RESPONSEMSG;
                        applyPay.TRANS_RET = model.DecModel.TRANSCODE;
                        applyPay.CARD_NO = model.DecModel.PAN;
                        applyPay.AUTH_NO = model.DecModel.APPROVECODE;
                        applyPay.AUTH_DATE = DateTime.ParseExact($"{model.DecModel.TRANSDATE}{model.DecModel.TRANSTIME}", "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        //applyPay.PAY_ACT_TIME = DateTime.Now;//新增時間

                        applyPay.PAY_EXT_TIME = DateTime.ParseExact($"{model.DecModel.TRANSDATE}{model.DecModel.TRANSTIME}", "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);//繳費時間
                        applyPay.PAY_INC_TIME = DateTime.Now;//異動時間
                        applyPay.CLIENT_IP = CLIENT_IP;
                        //applyPay.ADD_TIME = DateTime.Now;
                        //applyPay.ADD_FUN_CD = "WEB-APPLY";
                        //applyPay.ADD_ACC = UserInfo.ACC_NO.TONotNullString();
                        applyPay.UPD_TIME = DateTime.Now;
                        applyPay.UPD_FUN_CD = "WEB-APPLY";
                        applyPay.UPD_ACC = UserInfo.UserNo.TONotNullString();
                        applyPay.DEL_MK = "N";
                        this.Update2(applyPay, where_pay, dict2);

                        // 繳費主檔更新 PAY_A_PAID == PAY_A_FEE 時狀態會顯示已繳費
                        where_apply.APP_ID = APP_ID;
                        update_apply.PAY_A_PAID = model.DecModel.TRANSAMT.TOInt32();
                        update_apply.UPD_FUN_CD = "WEB-APPLY";
                        update_apply.UPD_ACC = UserInfo.UserNo.TONotNullString();
                        update_apply.APP_ID = APP_ID;
                        Update2(update_apply, where_apply, dict2, isBackmin);

                        result = true;
                        tran.Commit();
                    }
                    else
                    {
                        logger.Error($"{model.DecModel.RESPONSECODE}{model.DecModel.RESPONSEMSG}-{model.DecModel.OrderID}-{APP_ID}");
                        result = false;
                        tran.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug(ex.Message, ex);
                    result = false;
                    tran.Rollback();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }
        #endregion
    }
}
