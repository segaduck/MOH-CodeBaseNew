using CTCB.Crypto;
using ES.Action;
using ES.DataLayers;
using ES.Models;
using ES.Models.ViewModels;
using ES.Utils;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Web.Mvc;
using System.Security.Cryptography;
using ES.Services;
using System.Web;
using ES.Commons;
using System.Text;
using ES.Models.Entities;
using ES.Service.GSP2;
using System.Collections.Generic;

namespace ES.Controllers
{
    public class ApplyController : BaseNoMemberController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View("Index");
        }
        /// <summary>
        /// 我的E政府線上繳費 成功轉導
        /// </summary>
        /// <returns></returns>
        public ActionResult PASS(Dictionary<string, object> item)
        {
            logger.Debug("ApplyController_PASS");
            return RedirectToAction("Index", "History");
            //MapUtils data = new MapUtils();
            //PayDAO dao = new PayDAO();
            //APPLY_PAY model = new APPLY_PAY();
            //Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(item["PAY_ACCOUNT"]));
            //using (SqlConnection conn = GetConnection())
            //{
            //    conn.Open();
            //    // 查詢
            //    APPLY_PAY where = new APPLY_PAY();
            //    where.SESSION_KEY = data.Get("PAY_SESSION_KEY");
            //    model = dao.GetRow<APPLY_PAY>(where);
            //    // 查無該筆sessionKey
            //    if (model == null)
            //    {
            //        logger.Error($"我的E政府繳費ResultFailure_SESSION_KEY:{data.Get("PAY_SESSION_KEY")}查無該筆sessionKey");
            //        return View("ResultFailure");
            //    }
            //}
            //using (SqlConnection conn = GetConnection())
            //{
            //    conn.Open();
            //    FormAction action = new FormAction(conn);

            //    logger.Debug("PASS_PAY_SESSINO_KEY:" + data.Get("PAY_SESSION_KEY"));
            //    ConfirmResponse res = CardUtils.GetAccountingService(data.Get("PAY_SESSION_KEY"), pay);
            //    data.Put("PAY_TRANS_RET", res.ResultInfo);
            //    data.Put("APP_ID", model.APP_ID);

            //    ConfirmResult[] result = res.ConfirmResults;
            //    if (result.Length > 0)
            //    {
            //        logger.Debug("PASS_result_PAY_AUTH_DATE:" + result[0].AuthDate);
            //        data.Put("PAY_AUTH_DATE", result[0].AuthDate);
            //        data.Put("PAY_AUTH_NO", result[0].ApproveNo);
            //        data.Put("PAY_SETTLE_DATE", result[0].SettleDate);
            //        data.Put("PAY_HOST_TIME", result[0].HostTime);
            //        data.Put("PAY_INFO_NO", result[0].InfoNO);
            //        data.Put("PAY_OTHER", result[0].Others);

            //        data.Put("PAY_TRANS_RET", result[0].TransactionResult);
            //        data.Put("PAY_TRANS_MSG", action.GetPayCodeDesc(data.Get("PAY_TRANS_RET")));
            //    }
            //    logger.Debug("PASS_result_PAY_TRANS_RET:" + data.Get("PAY_TRANS_RET"));
            //    data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("0000")) ? data.Get("PAY_A_FEE") : "0");
            //    data.Put("ACC_NO", GetAccount());
            //    data.Put("UPD_ACC", GetAccount());

            //    if (dao.UpdateApplyPay(data.GetItem()))
            //    {
            //        return View("ResultSuccess");
            //    }
            //    else
            //    {
            //        return View("ResultFailure");
            //    }
            //    //data.Put("APP_FEE", (Int32.Parse(data.Get("PAY_A_FEE")) + Int32.Parse(data.Get("PAY_A_FEEBK"))).ToString());
            //    //data.Put("NAME", item["NAME"].ToString());
            //    //data.Put("COMP_DESC", item["COMP_DESC"].ToString());
            //    //data.Put("APPLY_DATE", ((DateTime)item["APP_TIME"]).ToString("yyyy/MM/dd"));
            //}
            //return View("ResultSuccess");
        }
        /// <summary>
        /// 我的E政府線上繳費 失敗轉導
        /// </summary>
        /// <returns></returns>
        public ActionResult ERROR(Dictionary<string, object> item)
        {
            logger.Debug("ApplyController_ERROR");
            return RedirectToAction("Index", "History");
        }
    }
}
