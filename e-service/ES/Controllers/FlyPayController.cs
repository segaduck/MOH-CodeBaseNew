using CTCB.Crypto;
using CTCBAPI;
using ES.Action;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ES.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ES.Controllers
{
    public class FlyPayController : BaseNoMemberController
    {
        //[HttpGet] 截驗收報告用
        //public ActionResult ResultView()
        //{
        //    FlyPayModel model = new FlyPayModel();
        //    DateTime dt = DateTime.Now;
        //    return View("ResultView", model);
        //}

        /// <summary>
        /// 防疫旅館繳費
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(string ArDate, string RAN)
        {
            //ViewBag.CloseMsg = "已關閉";
            //// 關閉視窗
            //return View("CloseView");

            FlyPayModel model = new FlyPayModel();
            model.payBasic = new UserInfoPayBasicModel();
            model.payBasic.isUsing = "N";
            model.paylist = new List<UserInfoPayModel>();
            // 存IP位置
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                var ClientIP = GetClientIP();

                FlyPayAction action = new FlyPayAction(conn);
                action.InsertFlyPayBasicIP(ClientIP, "2");
                conn.Close();
                conn.Dispose();
            }

            if (string.IsNullOrEmpty(ArDate) || string.IsNullOrEmpty(RAN) || ArDate.TONotNullString().Length != 8)
            {
                ViewBag.CloseMsg = "請輸入正確的Requset參數!";
                // 關閉視窗
                return View("CloseView");
            }
            else if (!string.IsNullOrEmpty(ArDate) && !string.IsNullOrEmpty(RAN))
            {
                if (!RAN.ToUpper().Contains("SIM"))
                {
                    ViewBag.CloseMsg = "請輸入正確Request參數!";
                    return View("CloseView");
                }
                model.payBasic.RAN = RAN;
                model.payBasic.isUsing = "Y";
                // 抵達日期預設當日
                model.payBasic.ArDate = DateTime.Now.ToString("yyyy/MM/dd");
                // 簡易專案
                model.flytype = "11";
                // 最多四筆入住者資料
                for (var i = 0; i < 4; i++)
                {
                    model.paylist.Add(new UserInfoPayModel());
                }
            }

            model.crdtype = "A";//預設信用卡
            model.language = "3";//預設英文
            // 使用玉山銀行繳費
            model.isEsun = true;
            return View("Index", model);
        }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Search()
        {
            //ViewBag.CloseMsg = "已關閉";
            //return View("CloseView");
            ES.Models.FlyPayModel fModel = new FlyPayModel();
            return View("Search", fModel);
        }
        /// <summary>
        /// 繳費查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Result(FlyPayModel model)
        {
            var result = new AjaxResultStruct();

            ES.Models.SessionModel sm = ES.Models.SessionModel.Get();
            if (sm.LoginValidateCode != model.ValidateCode)
            {
                result.status = false;
                result.message = "驗證碼輸入錯誤，請檢查!";
            }
            else
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    FlyPayAction action = new FlyPayAction(conn);

                    var mainNo = model.payBasic.mainno;
                    var birTh = model.payBasic.birth;
                    if (mainNo.TONotNullString() == "" || birTh.TONotNullString() == "")
                    {
                        result.status = false;
                        result.message = "護照號碼、出生年月日為必填，請填寫!";
                    }
                    else
                    {
                        birTh = birTh.Replace("-", "").Replace("/", "");
                        var htList = action.GetPayBasicList(mainNo, birTh, "");
                        if (htList.ToCount() > 0)
                        {
                            result.status = true;
                            result.data = htList.FirstOrDefault();
                        }
                        else
                        {
                            result.status = false;
                            result.message = "未查詢到繳費成功識別碼資料，請檢查填寫資料是否正確!";
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Content(result.Serialize(), "application/json");
        }

        [HttpPost]
        public ActionResult Index(FlyPayModel model)
        {
            var result = new AjaxResultStruct();
            var rtn_type = string.Empty;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                var isTest = false;
                var ErrorMsg = action.CheckPayList(model);
                if (ErrorMsg == "" && model.payBasic.isUsing == "Y")
                {
                    var peopleCount = action.CountPay(model); // 12歲以下無須付費
                    model.payBasic.livedays = model.payBasic.livedays;
                    model.payBasic.sprlevel = "2000";
                    // 統計須繳費金額(依據護照號碼查詢尚未繳費之總金額)
                    model.amountall = peopleCount * Convert.ToInt32(model.payBasic.livedays) * 2000;
                    // 系統訂單編號
                    var orderNo = DateTime.Now.Ticks.ToString() + Guid.NewGuid().ToString("N").SubstringTo(0, 1).Replace("-", "_").ToUpper();
                    if (model.crdtype == "A")
                    {
                        if (Convert.ToDateTime(DataUtils.GetConfig("CreditBankDate")) <= DateTime.Now)
                        {
                            #region 玉山銀行 信用卡
                            // 玉山銀行 信用卡 banktype = 2
                            var basedata = new Dictionary<string, string>();
                            basedata.Add("ONO", orderNo);
                            basedata.Add("U", DataUtils.GetConfig("FLYPAY_ESUN_TYPE4URL"));
                            basedata.Add("MID", DataUtils.GetConfig("FLYPAY_ESUN_MID"));
                            basedata.Add("TA", model.amountall.TONotNullString());
                            basedata.Add("TID", DataUtils.GetConfig("FLYPAY_ESUN_TID"));
                            model.es.data = JsonConvert.SerializeObject(basedata);
                            model.es.ONO = orderNo;
                            var cry256 = model.es.data + DataUtils.GetConfig("FLYPAY_ESUN_MAC");
                            model.es.mac = DataUtils.Crypt256BitConverter(cry256.ToTrim());
                            model.es.ksn = "1";
                            model.es.postUrl = DataUtils.GetConfig("FLYPAY_ESUN_TYPE4POST");
                            logger.Debug($"OrderNo:{orderNo},TA:{model.amountall.ToString()},mac:{model.es.mac}");
                            action.InsertFlyPayBasicOrderNo(model, orderNo, isTest, "2");

                            rtn_type = "1";
                            #endregion
                        }
                        else
                        {
                            #region 中國信託 一般信用卡
                            model.UrlmerId = DataUtils.GetConfig("FLYPAY_VISA_MERID");
                            // 中國信託 信用卡url繳費使用
                            Encrypt enc = new CTCB.Crypto.Encrypt();
                            enc.MerchantID = DataUtils.GetConfig("FLYPAY_VISA_MERCHANTID");
                            enc.TerminalID = DataUtils.GetConfig("FLYPAY_VISA_TERMAINALID");
                            enc.OrderNo = $"{orderNo}";
                            enc.AuthAmt = model.amountall.ToString();
                            enc.TxType = "0";
                            enc.AuthResURL = DataUtils.GetConfig("FLYPAY_VISA_AUTHRESURL").Contains("e-service") ? "https://eservice.mohw.gov.tw/FlyPay/CreditSuccessRecv" : DataUtils.GetConfig("FLYPAY_VISA_AUTHRESURL");
                            enc.Key = DataUtils.GetConfig("FLYPAY_VISA_KEY");
                            enc.MerchantName = DataUtils.GetConfig("FLYPAY_VISA_MERCHANTNAME");
                            enc.AutoCap = "1";
                            enc.Customize = model.language;
                            model.enc = enc;
                            action.InsertFlyPayBasicOrderNo(model, orderNo, isTest, "1");
                            rtn_type = "2";
                            #endregion
                        }
                    }
                    else
                    {
                        // 銀聯卡api繳費使用
                        if (Convert.ToDateTime(DataUtils.GetConfig("CreditBankDate")) <= DateTime.Now)
                        {
                            #region 玉山銀行 銀聯卡
                            // 玉山銀行 銀聯卡 banktype = 2
                            model.es.MID = DataUtils.GetConfig("FLYPAY_ESUN_MID");
                            model.es.CID = string.Empty;
                            model.es.ONO = orderNo;
                            model.es.TA = model.amountall.TONotNullString();
                            model.es.TT = "01";// 01消費 04退貨 31取消 00查詢
                            model.es.U = DataUtils.GetConfig("FLYPAY_ESUN_UURL");
                            model.es.TXNNO = string.Empty;
                            model.es.M = DataUtils.CryptMD5($"{model.es.MID}&{model.es.CID}&{model.es.ONO}&{model.es.TA}&{model.es.TT}&{model.es.U}&{model.es.TXNNO}&" + DataUtils.GetConfig("FLYPAY_ESUN_MAC"));
                            model.es.postUrl = DataUtils.GetConfig("FLYPAY_ESUN_UPOST");
                            logger.Debug($"OrderNo:{model.es.ONO},TA:{model.amountall.ToString()},M:{model.es.M}");
                            action.InsertFlyPayBasicOrderNo(model, orderNo, isTest, "2");

                            rtn_type = "3";
                            #endregion
                        }
                        else
                        {
                            #region 中國信託 銀聯卡
                            // 中國信託 銀聯卡
                            UnionPayMac uc = new UnionPayMac();
                            uc.inPara.merId = DataUtils.GetConfig("FLYPAY_UP_MERID");
                            uc.inPara.lidm = orderNo;
                            uc.inPara.purchAmt = model.amountall.ToString();
                            uc.inPara.macKey = DataUtils.GetConfig("FLYPAY_UP_MACKEY");
                            uc.inPara.inMac = uc.getMacValue();
                            model.uc = uc;

                            action.InsertFlyPayBasicOrderNo(model, orderNo, isTest, "1");
                            rtn_type = "4";
                            #endregion
                        }
                    }
                }
                else
                {
                    model.ErrMsg = ErrorMsg;
                    TempData["tempMessage"] = "請確認下列資訊 Please confirm that the correct information has been entered.<br>";
                    ViewBag.tempMessage = TempData["tempMessage"] + "\r\n" + ErrorMsg;
                    string resultMsg = Convert.ToString("" + ViewBag.tempMessage);
                    result.status = false;
                    result.message = resultMsg;
                    rtn_type = "5";
                    //return View("Index", model);
                }
                conn.Close();
                conn.Dispose();
            }
            switch (rtn_type)
            {
                case "1":
                    return PartialView("_CreditEsun", model);
                    break;
                case "2":
                    return PartialView("_Credit", model);
                    break;
                case "3":
                    return PartialView("_ChBankEsun", model);
                    break;
                case "4":
                    return PartialView("_ChBank", model);
                    break;
                default:
                    return Content(result.Serialize(), "application/json");
                    break;
            }
        }

        #region 中國信託 線上刷卡成功後重導回來
        /// <summary>
        /// 用來接收中國信託線上刷卡成功後重導回來的 handle action
        /// </summary>
        /// <param name="version">版本</param>
        /// <param name="charset">字元</param>
        /// <param name="xid">訂單識別碼(中信)</param>
        /// <param name="lidm">OrderNo</param>
        /// <param name="purchAmt">總金額</param>
        /// <param name="orderStatus">狀態代碼</param>
        /// <param name="respCode">回應碼</param>
        /// <param name="respMsg">回應訊息</param>
        /// <param name="traceNumber">系統跟蹤號，對帳時使用</param>
        /// <param name="traceTime">系統追蹤時間</param>
        /// <param name="qid">交易流水號(銀聯)</param>
        /// <param name="settleAmount">清算金額</param>
        /// <param name="settleCurrency">清算幣別</param>
        /// <param name="settleDate">清算日期</param>
        /// <param name="exchangeRate">清算匯率</param>
        /// <param name="exchangeDate">清算匯率換算日</param>
        /// <param name="inMac">壓碼值</param>
        /// <returns></returns>
        public ActionResult CreditSuccess(string version, string charset, string xid, string lidm, string purchAmt,
            string orderStatus, string respCode, string respMsg, string traceNumber, string traceTime, string qid,
            string settleAmount, string settleCurrency, string settleDate, string exchangeRate, string exchangeDate, string inMac)
        {
            // id 值為申請中國信託信用卡繳款服務時所填的(自行編號)代碼, 對應系統的 form id 值
            // 銀聯卡api繳費使用
            FlyPayModel model = new FlyPayModel();
            DateTime dt = DateTime.Now;

            if (string.IsNullOrEmpty(respCode))
            {
                model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "-";
                logger.Warn("\n##CreditSuccess(string version  respCode IsNullOrEmpty!: " + inMac + "\nFailMsg:" + respCode + respMsg);
                logger.Warn("\n##PayFailOrderNo:" + lidm + ",Message:" + respCode + ",ErrDesc:" + respMsg);
                return View("ResultView", model);
            }
            if (!respCode.Equals("00"))
            {
                model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "Pay Fail OrderNo:" + lidm + ",MessageCode:" + respCode + respMsg;
                logger.Warn("\n##CreditSuccess(string version  UpdateCreditSuccessRecvUnionPay: " + inMac + "\nFailMsg:" + respCode + respMsg);
                logger.Warn("\n##PayFailOrderNo:" + lidm + ",Message:" + respCode + ",ErrDesc:" + respMsg);
                return View("ResultView", model);
            }

            logger.Debug("CreditSuccess.traceNumber:" + traceNumber);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                logger.Debug("UpdateCreditSuccessRecvUnionPay:" + lidm);
                action.UpdateCreditSuccessRecvUnionPay(lidm, inMac, xid, qid, traceNumber, dt);
                if (action.CheckHasPayBasicGUID(lidm))
                {
                    model.ErrMsg += "很抱歉，此訂單已成功獲得訂房識別碼，請勿重複送出。";
                    logger.Warn(model.ErrMsg + ",ORDERNO:" + lidm);
                }
                //else if (action.CheckPayBasic())
                //{
                //    model.ErrMsg += "很抱歉，您指定的期間已無集檢所空房，此筆訂單費用會取消授權收款，謝謝。<br><br><br>We apologize all government quarantine facility rooms are sold out. Your charge will be refunded.";
                //    logger.Warn(model.ErrMsg + ",ORDERNO:" + lidm);
                //    // 防疫旅館(NEW)
                //    action.UpdateCreditSuccessRecvUnionPayGuid(lidm, inMac, xid, qid, traceNumber, "XXXXXXXXXX", dt);
                //    return View("ResultView", model);
                //}
                else
                {
                    model.Guid = RanDomNum();
                    logger.Debug("OrderNo:" + lidm + "RanDomNum():" + model.Guid);
                    // 防疫旅館(NEW)
                    action.UpdateCreditSuccessRecvUnionPayGuid(lidm, inMac, xid, qid, traceNumber, model.Guid, dt);
                    logger.Debug("UpdateCreditSuccessRecvUnionPayGuid:" + lidm);
                    model.succDate = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    model.payOrder = xid;
                    model.authCode = traceNumber;
                    logger.Info("\n##CreditSuccess(string version  UpdateCreditSuccessRecvUnionPay: " + inMac + "\nSuccessOrderNo:" + lidm + ",XID:" + xid + ",traceNumber:" + traceNumber);
                }
                conn.Close();
                conn.Dispose();
            }
            return View("ResultView", model);
        }
        #endregion

        /// <summary>
        /// 用來接收玉山銀行線上刷卡成功後重導回來的 handle action
        /// </summary>
        /// <param name="RC">回覆碼</param>
        /// <param name="MID">特店代碼</param>
        /// <param name="ONO">訂單編號</param>
        /// <param name="LTD">交易完成日期</param>
        /// <param name="LTT">交易完成時間</param>
        /// <param name="TRACENUMBER">系統跟蹤號</param>
        /// <param name="TRACETIME">系統跟蹤時間</param>
        /// <param name="TXNNO">交易序號</param>
        /// <param name="M">押碼</param>
        /// <returns></returns>
        public ActionResult CreditSuccessEsun(string RC, string MID, string ONO, string LTD, string LTT, string TRACENUMBER, string TRACETIME, string TXNNO, string M)
        {
            // 銀聯卡api繳費使用
            FlyPayModel model = new FlyPayModel();
            DateTime dt = DateTime.Now;
            var data = $"RC:{RC},MID:{MID},ONO:{ONO},LTD:{LTD},LTT:{LTT},TRACEENUMBER:{TRACENUMBER},TRACETIME:{TRACETIME},TXNNO:{TXNNO},M:{M}";
            logger.Debug($"CreditSuccessEsun:{data}");
            var mdata = DataUtils.CryptMD5($"{RC}&{MID}&{ONO}&{LTD}&{LTT}&{TRACENUMBER}&{TRACETIME}&{TXNNO}&" + DataUtils.GetConfig("FLYPAY_ESUN_MAC"));
            if (mdata == M)
            {
                logger.Debug("回傳值與系統押碼結果相符");
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    FlyPayAction action = new FlyPayAction(conn);
                    logger.Debug("UpdateCreditSuccessRecv:" + ONO);
                    if (action.CheckHasPayBasicGUID(ONO.TONotNullString()))
                    {
                        model.ErrMsg += "很抱歉，此訂單已成功獲得繳費成功識別碼，請勿重複送出。";
                        logger.Warn(model.ErrMsg + ",ORDERNO:" + ONO);
                        return View("ResultView", model);
                    }
                    else if (RC.TONotNullString() == "00")
                    {
                        var paydate = $"{LTD.Substring(0, 4)}-{LTD.Substring(4, 2)}-{LTD.Substring(6, 2)} {LTT.Substring(0, 2)}:{LTT.Substring(2, 2)}:{LTT.Substring(4, 2)}";
                        var adtime = action.getAddtime(ONO);
                        logger.Debug($"paydate:{paydate}, adtime:{adtime}");
                        if (Convert.ToDateTime(paydate) > Convert.ToDateTime(adtime).AddHours(1))
                        {
                            model.ErrMsg += "很抱歉，此訂單已操作逾時，請重新申請。此筆繳款紀錄會有專人通知進行退費流程。";
                            logger.Warn(model.ErrMsg + ",ORDERNO:" + ONO);
                            logger.Warn("action.NEEDBACK");
                            action.UpdateCreditSuccessNeedBack(ONO);
                            action.UpdateCreditSuccessRecvGuid(ONO.TONotNullString(), data.TONotNullString(), TXNNO.TONotNullString(), TRACENUMBER.TONotNullString(), "", paydate);
                        }
                        else
                        {
                            model.Guid = RanDomNum();
                            logger.Debug("OrderNo:" + ONO + "RanDomNum():" + model.Guid);
                            // 防疫旅館(NEW)
                            action.UpdateCreditSuccessRecvGuid(ONO.TONotNullString(), data.TONotNullString(), TXNNO.TONotNullString(), TRACENUMBER.TONotNullString(), model.Guid, paydate);
                            logger.Debug("UpdateCreditSuccessRecvGuid:" + ONO + "LTD&LTT:" + LTD + " " + LTT);
                            model.succDate = dt.ToString("yyyy/MM/dd HH:mm:ss");
                            model.payOrder = TXNNO.TONotNullString();
                            model.authCode = TRACENUMBER.TONotNullString();
                            logger.Info("\n##CreditSuccessRecvEsun(string data) UpdateCreditSuccessRecv: \nSuccessOrderNo:" + ONO + ",XID:" + TRACENUMBER + ",traceNumber:" + TXNNO);
                        }
                    }
                    else
                    {
                        ShareDAO dao = new ShareDAO();
                        var errDesc = dao.GetESUN_ERR_CODE(RC.TONotNullString());
                        model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "Pay Fail OrderNo:" + ONO + ",Message:" + RC + errDesc;
                        logger.Warn("\n##CreditSuccessRecvEsun(string data, string MACD) UpdateCreditSuccessRecv: " + data + "\nFailMsg:" + RC);
                        logger.Warn("\n##PayFailOrderNo:" + ONO + ",Message:" + RC + ",ErrDesc:" + errDesc);
                        // 交易狀態
                        action.UpdateCreditRC(ONO, RC);
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                logger.Debug("mdata:" + mdata);
                logger.Debug("M:" + M);
                ShareDAO dao = new ShareDAO();
                var errDesc = dao.GetESUN_ERR_CODE_U(RC.TONotNullString());
                model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "Pay Fail OrderNo:" + ONO + ",Message:" + RC + errDesc;
                logger.Warn("\n##CreditSuccessEsun(string RC, string MID, string ONO, string LTD, string LTT, string TRACENUMBER, string TRACETIME, string TXNNO, string M) \nFailMsg:" + RC);
                logger.Warn("\n##PayFailOrderNo:" + ONO + ",Message:" + RC + ",ErrDesc:" + errDesc);
            }
            return View("ResultView", model);
        }

        #region 中國信託 線上刷卡成功後重導回來
        /// <summary>
        /// 用來接收中國信託線上刷卡成功後重導回來的 handle action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CreditSuccessRecv(string URLResEnc)
        {
            // id 值為申請中國信託信用卡繳款服務時所填的(自行編號)代碼, 對應系統的 form id 值
            // 將中國信託 POST 回來的URLResEnc參數解密取出各欄位值
            FlyPayModel model = new FlyPayModel();
            DateTime dt = DateTime.Now;

            if (string.IsNullOrEmpty(URLResEnc))
            {
                model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "-";
                logger.Warn("\n##CreditSuccessRecv(string URLResEnc) URLResEnc IsNullOrEmpty!");
                return View("ResultView", model);
            }
            Decrypt dec = new Decrypt();
            dec.Key = DataUtils.GetConfig("FLYPAY_VISA_KEY");
            dec.EncRes = URLResEnc;

            if (dec.LastError != 0)
            {
                //dt.ToString("yyyy/MM/dd HH:mm:ss") +
                model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "Pay Fail OrderNo:" + dec.OrderNo + ",Message:" + dec.LastError;
                logger.Warn("\n##CreditSuccessRecv(string URLResEnc) UpdateCreditSuccessRecv: " + URLResEnc + "\nFailMsg:" + dec.LastError + "\nStatus:" + dec.Status + "\nerrCode:" + dec.ErrCode);
                logger.Warn("\n##PayFailOrderNo:" + dec.OrderNo + ",Message:" + dec.LastError + ",ErrDesc:" + dec.ErrDesc);
                return View("ResultView", model);
            }

            if (!string.IsNullOrEmpty(dec.ErrDesc))
            {
                model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "Pay Fail OrderNo:" + dec.OrderNo + ",Message:" + dec.ErrDesc;
                logger.Warn("\n##CreditSuccessRecv(string URLResEnc) UpdateCreditSuccessRecv: " + URLResEnc + "\nFailMsg:" + dec.LastError + "\nStatus:" + dec.Status + "\nerrCode:" + dec.ErrCode);
                logger.Warn("\n##PayFailOrderNo:" + dec.OrderNo + ",Message:" + dec.LastError + ",ErrDesc:" + dec.ErrDesc);
                return View("ResultView", model);
            }

            string traceNumber = dec.AuthCode;
            logger.Debug("CreditSuccessRecv.traceNumber:" + traceNumber);
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FlyPayAction action = new FlyPayAction(conn);
                logger.Debug("UpdateCreditSuccessRecv:" + dec.OrderNo);
                action.UpdateCreditSuccessRecv(dec.OrderNo, URLResEnc, dec.XID, traceNumber, dt);
                if (action.CheckHasPayBasicGUID(dec.OrderNo))
                {
                    model.ErrMsg += "很抱歉，此訂單已成功獲得訂房識別碼，請勿重複送出。";
                    logger.Warn(model.ErrMsg + ",ORDERNO:" + dec.OrderNo);
                }
                //else if (action.CheckPayBasic())
                //{
                //    model.ErrMsg += "很抱歉，您指定的期間已無集檢所空房，此筆訂單費用會取消授權收款，謝謝。<br><br><br>We apologize all government quarantine facility rooms are sold out. Your charge will be refunded.";
                //    logger.Warn(model.ErrMsg + ",ORDERNO:" + dec.OrderNo);
                //    // 防疫旅館(NEW)
                //    action.UpdateCreditSuccessRecvGuid(dec.OrderNo, URLResEnc, dec.XID, traceNumber, "XXXXXXXXXX", dt);
                //    return View("ResultView", model);
                //}
                else
                {
                    model.Guid = RanDomNum();
                    logger.Debug("OrderNo:" + dec.OrderNo + "RanDomNum():" + model.Guid);
                    // 防疫旅館(NEW)
                    action.UpdateCreditSuccessRecvGuid(dec.OrderNo, URLResEnc, dec.XID, traceNumber, model.Guid, dt.ToShortDateString());
                    logger.Debug("UpdateCreditSuccessRecvGuid:" + dec.OrderNo);
                    model.succDate = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    model.payOrder = dec.XID;
                    model.authCode = traceNumber;
                    logger.Info("\n##CreditSuccessRecv(string URLResEnc) UpdateCreditSuccessRecv: " + URLResEnc + "\nSuccessOrderNo:" + dec.OrderNo + ",XID:" + dec.XID + ",traceNumber:" + traceNumber);
                }
                conn.Close();
                conn.Dispose();
            }
            return View("ResultView", model);
        }

        #endregion

        /// <summary>
        /// 用來接收玉山銀行線上刷卡成功後重導回來的 handle action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CreditSuccessRecvEsun(string DATA, string MACD)
        {
            FlyPayModel model = new FlyPayModel();
            DateTime dt = DateTime.Now;
            // 玉山測試用
            logger.Debug($"CreditSuccessRecvEsun.DATA:{DATA}\n MACD:{MACD}");
            var dataList = DATA.TONotNullString().ToSplit(',');
            logger.Debug("dataToSplit:" + dataList.ToCount());
            var dic = new Dictionary<string, string>();
            foreach (var item in dataList)
            {
                logger.Debug(item);
                var dicsplit = item.ToSplit('=');
                dic.Add(dicsplit.FirstOrDefault(), dicsplit.LastOrDefault());
            }
            // MACD : SHA256( DATA + , + MacKey)
            var matchMacd = DataUtils.Crypt256BitConverter(DATA + "," + DataUtils.GetConfig("FLYPAY_ESUN_MAC"));
            logger.Debug("matchMacd:" + matchMacd);

            if (matchMacd == MACD)
            {
                logger.Debug("回傳值與系統押碼結果相符");
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    FlyPayAction action = new FlyPayAction(conn);
                    logger.Debug("UpdateCreditSuccessRecv:" + dic["ONO"]);
                    if (action.CheckHasPayBasicGUID(dic["ONO"]))
                    {
                        model.ErrMsg += "很抱歉，此訂單已成功獲得繳費成功識別碼，請勿重複送出。";
                        logger.Warn(model.ErrMsg + ",ORDERNO:" + dic["ONO"]);
                        return View("ResultView", model);
                    }
                    else if (dic["RC"] == "00")
                    {
                        var LTD = dic["LTD"];
                        var LTT = dic["LTT"];
                        var paydate = $"{LTD.Substring(0, 4)}-{LTD.Substring(4, 2)}-{LTD.Substring(6, 2)} {LTT.Substring(0, 2)}:{LTT.Substring(2, 2)}:{LTT.Substring(4, 2)}";
                        var adtime = action.getAddtime(dic["ONO"]);
                        logger.Debug($"paydate:{paydate}, adtime:{adtime}");
                        if (Convert.ToDateTime(paydate) > Convert.ToDateTime(adtime).AddHours(1))
                        {
                            model.ErrMsg += "很抱歉，此訂單已操作逾時，請重新申請。此筆繳款紀錄會有專人通知進行退費流程。";
                            logger.Warn(model.ErrMsg + ",ORDERNO:" + dic["ONO"]);
                            logger.Warn("action.NEEDBACK");
                            action.UpdateCreditSuccessNeedBack(dic["ONO"]);
                            action.UpdateCreditSuccessRecvGuid(dic["ONO"], DATA.TONotNullString(), dic["RRN"], dic["AIR"], "", paydate);
                        }
                        else
                        {
                            model.Guid = RanDomNum();
                            logger.Debug("OrderNo:" + dic["ONO"] + "RanDomNum():" + model.Guid);
                            // 防疫旅館(NEW)
                            action.UpdateCreditSuccessRecvGuid(dic["ONO"], DATA.TONotNullString(), dic["RRN"], dic["AIR"], model.Guid, paydate);

                            logger.Debug("UpdateCreditSuccessRecvGuid:" + dic["ONO"] + "AN:" + dic["AN"] + "LTD&LTT:" + dic["LTD"] + " " + dic["LTT"]);
                            model.succDate = dt.ToString("yyyy/MM/dd HH:mm:ss");
                            model.payOrder = dic["RRN"];
                            model.authCode = dic["AIR"];
                            logger.Info("\n##CreditSuccessRecvEsun(string data) UpdateCreditSuccessRecv: \nSuccessOrderNo:" + dic["ONO"] + ",XID:" + dic["RRN"] + ",traceNumber:" + dic["AIR"]);

                        }
                    }
                    else
                    {
                        ShareDAO dao = new ShareDAO();
                        var errDesc = dao.GetESUN_ERR_CODE(dic["RC"]);
                        model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "Pay Fail OrderNo:" + dic["ONO"] + ",Message:" + dic["RC"] + errDesc;
                        logger.Warn("\n##CreditSuccessRecvEsun(string data, string MACD) UpdateCreditSuccessRecv: " + DATA.TONotNullString() + "\nFailMsg:" + dic["RC"]);
                        logger.Warn("\n##PayFailOrderNo:" + dic["ONO"] + ",Message:" + dic["RC"] + ",ErrDesc:" + errDesc);
                        // 交易狀態
                        action.UpdateCreditRC(dic["ONO"], dic["RC"]);
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                ShareDAO dao = new ShareDAO();
                var errDesc = dao.GetESUN_ERR_CODE(dic["RC"]);
                model.ErrMsg = dt.ToString("yyyy/MM/dd HH:mm:ss") + "Pay Fail OrderNo:" + dic["ONO"] + ",Message:" + dic["RC"] + errDesc;
                logger.Warn("\n##CreditSuccessRecvEsun(string data, string MACD) UpdateCreditSuccessRecv: " + DATA.TONotNullString() + "\nFailMsg:" + dic["RC"]);
                logger.Warn("\n##PayFailOrderNo:" + dic["ONO"] + ",Message:" + dic["RC"] + ",ErrDesc:" + errDesc);
            }
            #region 錯誤代碼
            // 錯誤代碼 CODE_CD ESUN_ERR_CODE
            /*
             00 核准
             01請查詢銀行,05請查詢銀行,14卡號錯誤,54卡片過期,62尚未開卡,
             L1產品代碼錯誤,L2期數錯誤,L3不支援分期(他行卡),L4產品代碼過期,L5金額無效,
             L6不支援分期,L7非限定卡別交易,XA紅利自付額有誤,XB紅利商品數量有誤,XC紅利商品數量超過可折抵上限,
             XD紅利商品折抵點數超過最高折,XE紅利商品傳入之固定點數有誤,XF紅利折抵金額超過消費金額,
             X1不允許使用紅利折抵現金功能,X2點數未達可折抵點數下限,X3他行卡不支援紅利折抵,X4此活動已逾期,
             X5金額未超過限額不允許使用,X6特店不允許紅利交易,X7點數不足,X8非正卡持卡人,X9紅利商品編號有誤或空白,
             ET銀聯卡自訂回覆碼,UE請聯繫收單銀行,TEToken格式錯誤,TI無效Token,G0系統功能有誤,G1交易逾時,
             G2資料格式錯誤,G3非使用中特店,G4特店交易類型不合,G5連線IP不合,G6訂單編號重複,G7使用未定義之紅利點數進行交易,
             G8押碼錯誤,G9Session檢查有誤,GA無效的持卡人資料,GB不允許執行授權取消交易,GC退貨期限逾期,GD查無訂單編號,
             GE查無交易明細,GF交易資料狀態不符,GG交易失敗,GH訂單編號重複送出交易,GI銀行紅利狀態不福,
             GJ出團日期不合法,GK延後出團天數超過限定天數,GL非限定特店，不可使用[玉山卡]參數,
             GM限定特店，必須傳送[玉山卡]參數,GN該卡號非玉山卡所屬,GP銀行紅利與分期只能二選一,
             GR使用者取消刷卡頁面,GS系統暫停服務,GT交易時間逾時,GU預先授權重覆交易.
             GV無預先授權成功交易紀錄,GW無預先授權交易紀錄,GX3D交易異常,GY3D交易異常,GZ3D交易異常,
             V03D驗證失敗,V13D交易異常,V2發卡行(ACS)系統異常,VA取消交易金額有誤,VD取消交易時間不合法(日期),
             VT取消交易時間不合法(時間),RA退貨交易金額有誤,RD超過退貨交易期限,RP退貨交易不符
             N1紅利點數與金額不符,NM未設定特店類型或英文名稱,NR不允許退貨交易,NT未設定為銀聯卡交易特店,
             NV不允許取消交易,Z0URL錯誤,Z1不允許銀聯交易,Z2無法進行完全3D交易,QQ不允許DebitCard交易,其他 拒絕交易
             */
            #endregion
            return View("ResultView", model);
        }

        /// <summary>
        /// 訂單編號
        /// </summary>
        /// <returns></returns>
        public string RanDomNum()
        {
            Random crandom = new Random();
            var Cra = crandom.Next(100000);
            var CraString = Cra.TONotNullString().PadLeft(5, '0');


            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                var GuidCount = action.GetOneGuidDataList(CraString);

                while (GuidCount > 0)
                {
                    Cra = crandom.Next(100000);
                    CraString = Cra.TONotNullString().PadLeft(5, '0');
                    GuidCount = action.GetOneGuidDataList(CraString);
                }
                conn.Close();
                conn.Dispose();
            }

            return CraString;
        }
        /// <summary>
        /// 訂單編號 春節專案
        /// </summary>
        /// <returns></returns>
        public string GetSPRCODE(string PAYRESULT)
        {
            var result = string.Empty;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FlyPayAction action = new FlyPayAction(conn);
                result = action.GetSprProjectNum(PAYRESULT);
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        public ActionResult GetValidateCode()
        {
            Commons.ValidateCode vc = new Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(4);
            ES.Models.SessionModel.Get().LoginValidateCode = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        public ActionResult RenewInformation()
        {
            var result = new AjaxResultStruct();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                var htList = action.GetRoomsFullList();
                if (htList.ToCount() > 0)
                {
                    var secA = "北區:";
                    var secB = "中區:";
                    var secC = "南區:";
                    foreach (var item in htList)
                    {
                        if (Convert.ToString(item["sectionName"]) == "北區")
                        {
                            secA += Convert.ToString(item["fDayName"]) + "、";
                        }
                        else if (Convert.ToString(item["sectionName"]) == "中區")
                        {
                            secB += Convert.ToString(item["fDayName"]) + "、";
                        }
                        else if (Convert.ToString(item["sectionName"]) == "南區")
                        {
                            secC += Convert.ToString(item["fDayName"]) + "、";
                        }
                    }
                    var lastList = secA + "||" + secB + "||" + secC;
                    //fDayName sectionName
                    result.status = true;
                    result.data = lastList;
                }
                else
                {
                    result.status = false;
                }
                conn.Close();
                conn.Dispose();
            }
            result.message = "";
            return Content(result.Serialize(), "application/json");
        }
    }
}
