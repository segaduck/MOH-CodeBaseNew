using System;
using System.Net;
using log4net;
using HppApi;
using EECOnline.Models;

namespace EECOnline.Utils
{
    public class CardUtils
    {
        /// <summary>
        /// 聯合信用卡處理中心 取得交易金鑰
        /// </summary>
        public static ApiClient GetTransactionKeyEC(CreditHPPModel apimodel, string apply_no_sub)
        {
            ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;

            //var apimodel = new CreditHPPModel();
            var result = "";
            //String orderid = "EEC" + DateTime.Now.Ticks;
            String orderid = apply_no_sub + DateTime.Now.ToString("MMddHHmmss");
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
            var res = apiClient.postTransaction();
            var responseCode = apiClient.getRESPONSECODE();
            logger.Debug("responseCode:" + responseCode + ",orderid:" + orderid);
            if ("00".Equals(responseCode))
            {   // 作業執行成功
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

        protected static readonly ILog logger = LogUtils.GetLogger();
    }
}