using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Service.GSP1;
using log4net;
using ES.Service.GSP2;
using System.Net;
using HppApi;
using ES.Models;

namespace ES.Utils
{
    public class CardUtils
    {
        /// <summary>
        /// 取得E政府預約交易代碼
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static SessionKeyResponse GetTransactionKey(Dictionary<string, string> data, Dictionary<string, string> pay)
        {
            try
            {
                ServicePointManager.SecurityProtocol =
                   SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                // 建立WS 連線 Instance
                TransactionServiceSoapClient ws = new TransactionServiceSoapClient();

                /*
                string connectionAccount = DataUtils.GetConfig("PAY_CARD_ACCOUNT");    // 連線平台帳號
                string connectionPwd = DataUtils.GetConfig("PAY_CARD_PASSWORD");       // 連線平台密碼
                */
                string connectionAccount = pay["ACCOUNT"];    // 連線平台帳號
                string connectionPwd = pay["PSWD"];       // 連線平台密碼
                string clientIP = data["PAY_CLIENT_IP"];        // 使用者IP
                string ROCID = data["PAY_CARD_IDN"];    // 使用者身分證號

                // 付費資訊清單
                // 假設：設定交易資料, 單筆付費資訊
                PaymentInfo info = new PaymentInfo();
                info.DepartmentID = data["PAY_OID"];   // 機關代碼
                info.SOID = data["PAY_SID"];           // 服務代碼
                info.InfoNO = data["PAY_ID"];          // 機關銷帳編號
                info.Amount = Int32.Parse(data["APP_FEE"]); // 民眾繳費金額

                logger.Debug("connectionAccount: " + connectionAccount);
                logger.Debug("connectionPwd: " + connectionPwd);
                logger.Debug("clientIP: " + clientIP);
                logger.Debug("ROCID: " + ROCID);
                logger.Debug("DepartmentID: " + info.DepartmentID);
                logger.Debug("SOID: " + info.SOID);
                logger.Debug("InfoNO: " + info.InfoNO);
                logger.Debug("Amount: " + info.Amount);


                // 取得電子案號
                SessionKeyResponse res = ws.GetSessionTransactionKey(connectionAccount, connectionPwd, clientIP, ROCID, new PaymentInfo[] { info });

                return res;
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }

            return null;
        }

        /// <summary>
        /// 確認交易結果
        /// </summary>
        /// <param name="sessionTransactionKey"></param>
        /// <param name="pay"></param>
        /// <returns></returns>
        public static ConfirmResponse GetAccountingService(string sessionTransactionKey, Dictionary<string, string> pay)
        {
            string connectionAccount = null;  // 連線平台帳號
            string connectionPwd = null;      // 連線平台密碼
            try
            {
                ServicePointManager.SecurityProtocol =
                       SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                // 建立WS 連線 Instance
                AccountingServiceSoapClient ws = new AccountingServiceSoapClient();
                if (pay == null)
                {
                    logger.Warn("##GetAccountingService ,(Dictionary<string, string> pay) is null!!! ");
                    return null;
                }
                if (ws == null)
                {
                    logger.Warn("##GetAccountingService ,(AccountingServiceSoapClient ws) is null!!! ");
                    return null;
                }
                if (sessionTransactionKey == null || string.IsNullOrEmpty(sessionTransactionKey))
                {
                    logger.Warn("##GetAccountingService ,sessionTransactionKey is null!!! ");
                    return null;
                }

                // 主機連線資訊
                //string connectionAccount = DataUtils.GetConfig("PAY_CARD_ACCOUNT");    // 連線平台帳號
                //string connectionPwd = DataUtils.GetConfig("PAY_CARD_PASSWORD");       // 連線平台密碼
                connectionAccount = (pay["ACCOUNT"] ?? "");  // 連線平台帳號
                connectionPwd = (pay["PSWD"] ?? "");         // 連線平台密碼

                // 確認交易結果
                ConfirmResponse res = ws.ConfirmTransaction(connectionAccount, connectionPwd, sessionTransactionKey);
                if (res != null)
                {
                    logger.Debug("[1] SESSION_KEY: " + sessionTransactionKey + " / PAY_TRANS_RET: " + res.ResultInfo);
                }

                return res;

            }
            catch (Exception e)
            {
                string s_Err1 = "";
                s_Err1 += string.Format("\n ##GetAccountingService {0}", e.Message);
                s_Err1 += string.Format("\n connectionAccount:{0}", connectionAccount);
                s_Err1 += string.Format("\n connectionPwd:{0}", connectionPwd);
                logger.Warn(s_Err1, e);
            }
            return null;
        }

        /// <summary>
        /// 確認交易結果
        /// </summary>
        /// <param name="sessionTransactionKey"></param>
        /// <param name="connectionAccount"></param>
        /// <param name="connectionPwd"></param>
        /// <returns></returns>
        public static MapUtils GetAccountingService(string sessionTransactionKey, string connectionAccount, string connectionPwd)
        {
            MapUtils data = new MapUtils();

            try
            {
                ServicePointManager.SecurityProtocol =
          SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
          SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                // 建立WS 連線 Instance
                AccountingServiceSoapClient ws = new AccountingServiceSoapClient();

                // 確認交易結果
                ConfirmResponse res = ws.ConfirmTransaction(connectionAccount, connectionPwd, sessionTransactionKey);

                if (res != null)
                {
                    logger.Debug("[2] SESSION_KEY: " + sessionTransactionKey + " / PAY_TRANS_RET: " + res.ResultInfo);

                    data.Put("PAY_TRANS_RET", res.ResultInfo);

                    ConfirmResult[] result = res.ConfirmResults;

                    if (result != null && result.Length > 0)
                    {
                        if (result[0] != null)
                        {
                            logger.Debug("PAY_AUTH_DATE" + result[0].AuthDate);
                            logger.Debug("PAY_AUTH_NO" + result[0].ApproveNo);
                            logger.Debug("PAY_SETTLE_DATE" + result[0].SettleDate);
                            logger.Debug("PAY_HOST_TIME" + result[0].HostTime);
                            logger.Debug("PAY_INFO_NO" + result[0].InfoNO);
                            logger.Debug("PAY_OTHER" + result[0].Others);
                            logger.Debug("PAY_TRANS_RET" + result[0].TransactionResult);
                        }
                        else
                        {
                            logger.Error("result[0] is null");
                        }
                        data.Put("PAY_AUTH_DATE", result[0].AuthDate);
                        data.Put("PAY_AUTH_NO", result[0].ApproveNo);
                        data.Put("PAY_SETTLE_DATE", result[0].SettleDate);
                        data.Put("PAY_HOST_TIME", result[0].HostTime);
                        data.Put("PAY_INFO_NO", result[0].InfoNO);
                        data.Put("PAY_OTHER", result[0].Others);

                        data.Put("PAY_TRANS_RET", result[0].TransactionResult);
                    }

                    return data;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }

            return null;
        }

        /// <summary>
        /// 聯合信用卡處理中心 取得交易金鑰
        /// </summary>
        public static ApiClient GetTransactionKeyEC(CreditHPPModel apimodel)
        {
            ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;

            //var apimodel = new CreditHPPModel();
            var result = "";
            String orderid = "EC" + DateTime.Now.Ticks;
            apimodel.EncModel.OrderID = orderid;
            ApiClient apiClient = new ApiClient();
            apiClient.setMERCHANTID(apimodel.EncModel.MerchantID);
            apiClient.setTERMINALID(apimodel.EncModel.TerminalID);
            apiClient.setORDERID(apimodel.EncModel.OrderID);
            apiClient.setIDNUMBER(apimodel.EncModel.IDNUMBER);
            apiClient.setTRANSMODE("0");
            apiClient.setINSTALLMENT("0");
            apiClient.setTRANSAMT(apimodel.EncModel.TransAmt);
            apiClient.setNotifyURL(apimodel.EncModel.NotifyURL);
            apiClient.setURL(apimodel.ECConnetModel.DomainName, apimodel.ECConnetModel.RequestURL);
            apiClient.setPrivateData(apimodel.EncModel.PrivateData); // ClientIP,APPID,SRVID
           
            // 當"00".Equals(responseCode))時,可以取得”交易金鑰”否則為錯誤碼
            // 取得交易金鑰 apiClient.getKEY())再使用 3.2 節方式呼叫 hpp 程式
            logger.Debug("apiClient.postTransaction.");
            apiClient.postTransaction();
            var responseCode = apiClient.getRESPONSECODE();
            logger.Debug("responseCode:" + responseCode + ",orderid:" + orderid);
            if ("00".Equals(responseCode))
            { // 作業執行成功
                result = apiClient.getKEY(); //交易金鑰
                logger.Debug("ORDERID=" + orderid + " RESPONSECODE=" + apiClient.getRESPONSECODE() + " RESPONSEMSG=" + apiClient.getRESPONSEMSG());
                apiClient.UrlEncode(result);
            }
            else
            {
                result = apiClient.getRESPONSECODE();
                apiClient.log("ORDERID=" + orderid + " RESPONSECODE=" + apiClient.getRESPONSECODE() + " RESPONSEMSG=" + apiClient.getRESPONSEMSG());
                logger.Debug("ORDERID=" + orderid + " RESPONSECODE=" + apiClient.getRESPONSECODE() + " RESPONSEMSG=" + apiClient.getRESPONSEMSG());
            }
            //儲存回傳編號用
            apiClient.setORDERID(orderid);
            logger.Debug("apiClient.postTransaction. End.");
            //// log檔案路徑
            //apiClient.setLogMaskFlag(false);
            //apiClient.setLogFile(DataUtils.GetConfig("PAY_EC_LOGFILE"));
            return apiClient;
        }
        /// <summary>
        /// 聯合 查詢
        /// </summary>
        /// <param name="model">主要用 特店代碼</param>
        /// <param name="orderid">ECORDERID</param>
        /// <param name="transkey">SESSIONKEY</param>
        /// <returns></returns>
        public static ApiClient GetpostQueryEC(CreditHPPModel model,string transkey="")
        {
            ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;
            var result = "";
            ApiClient apiClient = new ApiClient();
            if (!string.IsNullOrWhiteSpace(transkey))
            {
                apiClient.setKEY(transkey);
            }
            else
            {
                apiClient.setMERCHANTID(model.EncModel.MerchantID);
                apiClient.setORDERID(model.EncModel.OrderID);
            }
            apiClient.setURL(model.ECConnetModel.DomainName, model.ECConnetModel.RequestURL);
            logger.Debug("GetpostQueryEC.postQuery");
            var rtnCode = apiClient.postQuery();
            var responseCode = apiClient.getRESPONSECODE();
            logger.Debug("responseCode:" + responseCode + ",orderid:" + model.EncModel.OrderID);
            if ("00".Equals(responseCode))
            { // 作業執行成功
                result = apiClient.getKEY(); //交易金鑰
                logger.Debug("ORDERID=" + model.EncModel.OrderID + " RESPONSECODE=" + apiClient.getRESPONSECODE() + " RESPONSEMSG=" + apiClient.getRESPONSEMSG());
            }
            else
            {
                result = apiClient.getRESPONSECODE();
                apiClient.log("ORDERID=" + model.EncModel.OrderID + " RESPONSECODE=" + apiClient.getRESPONSECODE() + " RESPONSEMSG=" + apiClient.getRESPONSEMSG());
                logger.Debug("ORDERID=" + model.EncModel.OrderID + " RESPONSECODE=" + apiClient.getRESPONSECODE() + " RESPONSEMSG=" + apiClient.getRESPONSEMSG());
            }
            //logger.Debug("apiClient.postTransaction. End.");
            //// log檔案路徑
            //apiClient.setLogMaskFlag(false);
            //apiClient.setLogFile(DataUtils.GetConfig("PAY_EC_LOGFILE"));
            return apiClient;
        }



        protected static readonly ILog logger = LogUtils.GetLogger();
    }
}