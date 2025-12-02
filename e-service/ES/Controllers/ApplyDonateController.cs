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

namespace ES.Controllers
{
    public class ApplyDonateController : BaseNoMemberController
    {
        /// <summary>
        /// 線上捐款專案列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ApplyDonateViewModel model = new ApplyDonateViewModel();
            using (SqlConnection conn = GetConnection())
            {
                ApplyDonateAction action = new ApplyDonateAction(conn);
                conn.Open();
                model.Grid = action.GetApplyDonateList();
                if (model.Grid != null && model.Grid.Count > 0)
                {
                    foreach (var item in model.Grid)
                    {
                        item.FileGrid = action.GetApplyDonateFile(item.SRV_ID_DONATE);
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return View("Index", model);
        }
        /// <summary>
        /// 線上捐款專案明細
        /// </summary>
        /// <param name="srv_id_donate"></param>
        /// <returns></returns>
        public ActionResult Detail(string srv_id_donate)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDonateViewModel model = new ApplyDonateViewModel();
            using (SqlConnection conn = GetConnection())
            {
                ApplyDonateAction action = new ApplyDonateAction(conn);
                conn.Open();
                model.Detail = action.GetApplyDonate(srv_id_donate);
                if (model.Detail == null)
                {
                    sm.LastErrorMessage = "查無資料。";
                    return RedirectToAction("Index");
                }
                model.Detail.FileList = action.GetApplyDonateFile(srv_id_donate);
                conn.Close();
                conn.Dispose();
            }
            return View("Detail", model);
        }
        /// <summary>
        /// 同意個資使用說明 線上捐款
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Detail(ApplyDonateViewModel form)
        {
            SessionModel sm = SessionModel.Get();
            if (form.Detail != null)
            {
                if (form.Detail.START_DATE <= DateTime.Now && form.Detail.END_DATE.Value.AddDays(1) >= DateTime.Now)
                {
                    //符合捐款期限
                }
                else
                {
                    sm.LastErrorMessage = $"{form.Detail.NAME_CH}尚未開放或已截止捐款";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
            ApplyDonateDAO dao = new ApplyDonateDAO();
            Apply_007001FormModel model = new Apply_007001FormModel();
            model.SRV_ID_DONATE = form.Detail.SRV_ID_DONATE;
            TblAPPLY_DONATE ad = new TblAPPLY_DONATE();
            ad.SRV_ID_DONATE = form.Detail.SRV_ID_DONATE;
            var data = dao.GetRow(ad);
            model.PAY_METHOD = data.PAY_WAY;

            return View("Donate", model);
        }
        /// <summary>
        /// 線上捐款
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Donate(Apply_007001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            var APP_ID = "";
            if (ModelState.IsValid)
            {
                ApplyDAO dao = new ApplyDAO();
                // 存檔
                model.PayModel.ClientIp = GetClientIP();
                APP_ID = dao.AppendApply007001(model);
                model.APP_ID = APP_ID;
                if (model.PAY_METHOD == "C")
                {
                    model.PayModel.UrlmerId = DataUtils.GetConfig("DONATE_VISA_MERID");
                    // 信用卡url繳費使用
                    Encrypt enc = new CTCB.Crypto.Encrypt();
                    enc.MerchantID = DataUtils.GetConfig("DONATE_VISA_MERCHANTID");
                    enc.TerminalID = DataUtils.GetConfig("DONATE_VISA_TERMAINALID");
                    enc.OrderNo = $"{APP_ID}";
                    enc.AuthAmt = model.AMOUNT;
                    // 信用卡識別
                    enc.TxType = "0";
                    enc.AuthResURL = DataUtils.GetConfig("DONATE_VISA_AUTHRESURL");
                    enc.Key = DataUtils.GetConfig("DONATE_VISA_KEY");
                    enc.MerchantName = DataUtils.GetConfig("DONATE_VISA_MERCHANTNAME");
                    enc.AutoCap = "1";
                    model.PayModel.enc = enc;
                    logger.Debug("DonateEncodeData: " + enc.EncodeData);
                    logger.Debug("DonatePostWay: " + enc.TxType);
                    return PartialView("_Credit", model);
                }
                else if (model.PAY_METHOD == "T")
                {
                    model.PayModel.UrlmerId = DataUtils.GetConfig("DONATE_VISA_MERID");
                    // WebATM url繳費使用
                    Encrypt enc = new CTCB.Crypto.Encrypt();
                    enc.MerchantID = DataUtils.GetConfig("DONATE_VISA_MERCHANTID");
                    enc.TerminalID = DataUtils.GetConfig("DONATE_VISA_TERMAINALID");
                    enc.OrderNo = $"{APP_ID}";
                    enc.AuthAmt = model.AMOUNT;
                    // WebATM 識別
                    enc.TxType = "9";
                    // 轉入帳號
                    enc.WebATMAcct = PayUtils.GetVirtualAccount(APP_ID, 0);
                    enc.AuthResURL = DataUtils.GetConfig("DONATE_VISA_AUTHRESURL");
                    enc.Key = DataUtils.GetConfig("DONATE_VISA_KEY");
                    enc.StoreName = DataUtils.GetConfig("DONATE_VISA_MERCHANTNAME");
                    enc.BillShortDesc = DataUtils.GetConfig("DONATE_VISA_MERCHANTNAME");
                    enc.AutoCap = "1";
                    model.PayModel.enc = enc;
                    logger.Debug("DonateEncodeData: " + enc.EncodeData);
                    logger.Debug("DonatePostWay: " + enc.TxType);
                    return PartialView("_Credit", model);
                }
                else if (model.PAY_METHOD == "S")
                {
                    // 超商條碼下載
                    logger.Debug("DonatePostWay: _StoreBar" + APP_ID);
                    return PartialView("_StoreBar", model);
                }
                else if (model.PAY_METHOD == "L")
                {
                    // LinePay
                    logger.Debug("DonatePostWay: _LinePay" + APP_ID);
                    model.LinePayModel.orderId = APP_ID;
                    return PartialView("_LinePay", model);
                }
            }
            else
            {
                var result = new AjaxResultStruct();
                var ErrorMsg = string.Empty;
                result.status = false;
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg = ErrorMsg + item.Errors[0].ErrorMessage + "\r\n";
                    }
                }
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }
            return View("Donate", model);
        }
        /// <summary>
        /// 抓取API POST 介接資料
        /// </summary>
        public ActionResult postLinePay(Apply_007001FormModel model, string orderId)
        {
            ApplyDAO dao = new ApplyDAO();
            model.LinePayModel.amount = model.AMOUNT.TOInt32();
            model.LinePayModel.currency = "TWD";
            model.LinePayModel.orderId = orderId;
            model.LinePayModel.packages.Add(new LinePayPackages()
            {
                id = orderId,
                amount = model.AMOUNT.TOInt32(),
                //userFee = 0,
                name = "線上捐款"
            });
            model.LinePayModel.packages[0].products.Add(new LinePayProducts()
            {
                id = "0070010001",
                name = "線上捐款專案",
                quantity = 1,
                price = model.AMOUNT.TOInt32(),
                //originalPrice = model.AMOUNT.TOInt32()
            });
            try
            {
                string ValueInToken = JsonConvert.SerializeObject(model.LinePayModel);
                string apiurl = "/v3/payments/request";
                string ChannelSecret = DataUtils.GetConfig("LINEPAY_SECRETKEY");
                string ChannelId = DataUtils.GetConfig("LINEPAY_CHANNELID");
                string nonce = Guid.NewGuid().ToString();
                string Signature = CommonsServices.HMACSHA256_Base64((ChannelSecret + apiurl + ValueInToken + nonce), ChannelSecret);
                string httpUrl = DataUtils.GetConfig("LINEPAY_APIURL") + apiurl;

                // 底下建立連線
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("X-LINE-ChannelId", ChannelId);
                httpWebRequest.Headers.Add("X-LINE-Authorization-Nonce", nonce);
                httpWebRequest.Headers.Add("X-LINE-Authorization", Signature);

                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                //將傳送的字串轉為 byte array
                byte[] byteArray = Encoding.UTF8.GetBytes(ValueInToken);
                using (var stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                    stream.Flush();
                    stream.Close();
                }

                //取得回覆資訊
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                //解讀回覆資訊
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var responseJSON = streamReader.ReadToEnd();
                    //將 JSON 轉為物件
                    LinePayResponseViewModel oReturnObj = (LinePayResponseViewModel)Newtonsoft.Json.JsonConvert.DeserializeObject(responseJSON, typeof(LinePayResponseViewModel));
                    //紀錄回傳資訊
                    dao.InsertLinePayResponseData(oReturnObj, model);
                    logger.Info("Apply007001LinePay:" + oReturnObj.returnCode + ",orderId:" + model.APP_ID);
                    if (oReturnObj.returnCode == "0000")
                    {
                        //成功
                        dao.UpdateApply007001LinePay(oReturnObj, model);
                        model.ReturnMsg = "付款成功。感謝您的捐款！";
                        return View("DonateResult", model);
                    }
                    else
                    {
                        //失敗
                        model.ReturnMsg = $"付款失敗。錯誤訊息：{oReturnObj.returnCode}/{oReturnObj.returnMessage}";
                        return View("DonateResult", model);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                model.ReturnMsg = $"付款失敗。錯誤訊息：{ex.Message}";
            }
            return View("DonateResult", model);
        }
        /// <summary>
        /// 下載超商繳費條碼
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult PayPDF(string id)
        {
            ApplyDAO dao = new ApplyDAO();
            return File(dao.ExportPayDonatePDF(id), "application/pdf", "Donate" + id + ".pdf");
        }

        /// <summary>
        /// 用來接收中國信託線上刷卡成功後重導回來的 handle action(FormController)
        /// 從FormControll CreditSuccessRecv 轉址過來的
        /// </summary>
        /// <param name="URLResEnc"></param>
        /// <returns></returns>
        public ActionResult CreditSuccessRecv(string URLResEnc)
        {
            // id 值為申請中國信託信用卡繳款服務時所填的(自行編號)代碼, 對應系統的 form id 值
            // 將中國信託 POST 回來的URLResEnc參數解密取出各欄位值
            Apply_007001FormModel model = new Apply_007001FormModel();
            ApplyDAO dao = new ApplyDAO();
            var dt = DateTime.Now;
            Decrypt dec = new Decrypt();
            dec.Key = DataUtils.GetConfig("DONATE_VISA_KEY");
            dec.EncRes = URLResEnc;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ApplyDonateAction action = new ApplyDonateAction(conn);
                // api 紀錄
                dao.InsertCreditResponseData(dec, model);
                if (dec.LastError == 0)
                {
                    if (!string.IsNullOrEmpty(dec.ErrDesc))
                    {
                        // 失敗
                        logger.Error("\nUpdateDonateCreditSuccessRecv: " + URLResEnc + "\nFailMsg:" + dec.LastError + "\nStatus:" + dec.Status + "\nerrCode:" + dec.ErrCode);
                        model.ReturnMsg = $"付款失敗。錯誤訊息：{dec.ErrCode}/{dec.ErrDesc}";
                    }
                    else
                    {
                        // 成功
                        dao.UpdateApply007001Credit(dec, model);
                        logger.Info("\nUpdateDonateCreditSuccessRecv: " + URLResEnc + "\nSuccessOrderNo:" + dec.OrderNo + ",XID:" + dec.XID);
                        model.ReturnMsg = "付款成功。感謝您的捐款！";
                    }
                }
                else
                {
                    // 失敗
                    logger.Error("\nUpdateDonateCreditSuccessRecv: " + URLResEnc + "\nFailMsg:" + dec.LastError + "\nStatus:" + dec.Status + "\nerrCode:" + dec.ErrCode);
                    model.ReturnMsg = $"付款失敗。錯誤訊息：{dec.ErrCode}/{dec.ErrDesc}";

                }
                conn.Close();
                conn.Dispose();
            }
            return View("DonateResult", model);
        }
    }
}
