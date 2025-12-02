using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Utils;
using ES.Service.GSP1;

namespace ES.Areas.Admin.Controllers
{
    public class ResetConfigController : BaseController
    {
        public ActionResult Index()
        {
            DataUtils.ClearConfig();

            return View();
        }

        public ActionResult Test()
        {
            SessionKeyResponse res = GetTransactionKey();



            if (res != null)
            {
                logger.Debug("ResultInfo: " + res.ResultInfo);
                logger.Debug("SessionTransactionKey: " + res.SessionTransactionKey);
            }
            else
            {
                logger.Debug("res is null");
            }
             
            return View("Message");
        }

        /// <summary>
        /// 取得E政府預約交易代碼
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private SessionKeyResponse GetTransactionKey()
        {
            try
            {

                // 建立WS 連線 Instance
                TransactionServiceSoapClient ws = new TransactionServiceSoapClient();

                /*
                string connectionAccount = DataUtils.GetConfig("PAY_CARD_ACCOUNT");    // 連線平台帳號
                string connectionPwd = DataUtils.GetConfig("PAY_CARD_PASSWORD");       // 連線平台密碼
                */
                string connectionAccount = "mohw2351";    // 連線平台帳號
                string connectionPwd = "DOSAASWmohw@0723";       // 連線平台密碼
                string clientIP = GetClientIP();        // 使用者IP
                string ROCID = "Z100000084";    // 使用者身分證號

                // 付費資訊清單
                // 假設：設定交易資料, 單筆付費資訊
                PaymentInfo info = new PaymentInfo();
                info.DepartmentID = "2.16.886.101.999990264";   // 機關代碼
                info.SOID = "2.16.886.101.999990264.2097156.1.1";           // 服務代碼
                info.InfoNO = "201410031101";          // 機關銷帳編號
                info.Amount = 1000; // 民眾繳費金額

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
    }
}
