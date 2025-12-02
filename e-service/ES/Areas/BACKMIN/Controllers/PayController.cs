using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;
using System.Web.Routing;
using ES.Utils;
using System.IO;
using WebUI.CustomClass;
using System.Data.SqlClient;
using ES.DataLayers;
using ES.Models;
using HppApi;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PayController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 7;
        }

        //
        // GET: /Admin/Pay/
        [HttpGet]
        public ActionResult Maintain()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            PayModel model = new PayModel();
            model.queryModel.APP_TIME_BEGIN = DateTime.Now;
            model.queryModel.APP_TIME_END = DateTime.Now;
            model.queryModel.NowPage = 1;
            model.queryModel.OderByCol = "APP_ID";
            model.queryModel.SortAZ = "DESC";
            ViewBag.NowPage = 1;
            ViewBag.TotalCount = 0;
            ViewBag.TotalPage = 0;

            this.SetVisitRecord("Pay", "Maintain", "繳費維護");

            return View(model);
        }

        /// <summary>
        /// MaintainViewUpdate 返回 Maintain
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MaintainBack(MaintainModel query)
        {
            PayModel model = new PayModel();
            model.queryModel = query;
            model.ActionType = string.Empty;
            model.APP_ID = string.Empty;
            PayAction action = new PayAction();
            ViewBag.List = action.MaintainQuery(model.queryModel);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.queryModel.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View("Maintain", model);
        }

        [HttpPost]
        public ActionResult Maintain(PayModel model)
        {
            if (model.ActionType == "Open")
            {
                model.queryModel.viewAPP_ID = model.APP_ID;
                return RedirectToAction("MaintainView", model.queryModel);
            }

            PayAction action = new PayAction();
            ViewBag.List = action.MaintainQuery(model.queryModel);
            if (ViewBag.List != null && ViewBag.List.Count > 0)
            {
                foreach (var item in ViewBag.List)
                {
                    if (Convert.ToString(item.GetString("PAY_BANK")) == "EC")
                    {
                        if (item.GetString("APP_ID") != null && item.GetString("PAY_METHOD") == "C" && item.GetString("PAY_STATUS_MK") == "N")
                        {
                            // 聯合信用卡中心
                            logger.Debug("MaintainEC_CheckCardStatus:" + item.GetString("APP_ID"));
                            //信用卡狀態重新勾稽
                            CheckCardStatusEC(item.GetString("APP_ID"));
                        }
                    }
                    else
                    {
                        // 我的E政府
                        if (item.GetString("APP_ID") != null && item.GetString("PAY_METHOD") == "C"/* && item.GetString("PAY_STATUS_MK") == "N"*/)
                        {
                            logger.Debug("Maintain_CheckCardStatus:" + item.GetString("APP_ID"));
                            //信用卡狀態重新勾稽
                            CheckCardStatus(item.GetString("APP_ID"));
                        }
                    }
                }
            }

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.queryModel.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }

        public ActionResult MaintainView(MaintainModel model)
        {
            PayModel result = new PayModel();
            result.queryModel = model;
            PayAction action = new PayAction();
            result.viewModel = action.MaintainViewQuery(model.viewAPP_ID);
            ViewBag.List = action.MaintainViewDetailQuery(model.viewAPP_ID);
            return View(result);
        }

        [HttpPost]
        public ActionResult SetSETTLE_DATE(String APP_IDS)
        {
            String[] APP_ID = APP_IDS.Split(',');
            OptionModel om = new OptionModel();
            //PayAction action = new PayAction();
            PayDAO dao = new PayDAO();

            bool bo = dao.UpdateAPPLY_PAY(APP_ID, GetAccount());
            if (bo)
            {
                om.VALUE = "更新完成！";
            }
            else
            {
                om.VALUE = "更新失敗！";
            }

            return Json(om);
        }

        [HttpPost]
        public ActionResult MaintainViewUpdate(PayModel model, String UNIT_CD)
        {
            if (model.ActionType == "Reback")
            {
                return RedirectToAction("MaintainBack", model.queryModel);
            }
            PayDAO dao = new PayDAO();
            String app_id = Request.Form["app_id"];
            String tx_id_new = Request.Form["tx_id_new"];
            tx_id_new = String.IsNullOrEmpty(tx_id_new) ? "" : tx_id_new;
            String paymethod_new = Request.Form["paymethod_new"];
            String pay_money_new = Request.Form["pay_money_new"];
            String pay_cactual_new = Request.Form["pay_cactual_new"];
            pay_cactual_new = String.IsNullOrEmpty(pay_cactual_new) ? "" : pay_cactual_new;
            String[] tx_id_org = Request.Form.GetValues("tx_id_org") == null ? new String[0] : Request.Form.GetValues("tx_id_org");
            String[] paymethod_org = Request.Form.GetValues("paymethod_org") == null ? new String[0] : Request.Form.GetValues("paymethod_org");
            String service_uid_db = Request.Form["service_uid_db"];
            String pay_cexpect_db = Request.Form["pay_cexpect_db"];
            PayAction action = new PayAction();
            int pay_aactual_new = action.MaintainViewTotalPayQuery(app_id);
            String IsMail = "N";
            String upd_flag = "0";
            bool result = false;
            if (tx_id_org.Length > 0)
            {
                upd_flag = "1";
                string tx_id = "";
                string tx_id_chk = "";
                string pay_money_org = "";
                string paymethod = "";

                foreach (string opt in tx_id_org)
                {
                    // opt = tmep
                    String[] temp = opt.Split('-');
                    if (temp.Length == 2)
                    {
                        tx_id = temp[0];
                        pay_money_org = temp[1];
                    }

                    pay_aactual_new = pay_aactual_new + int.Parse(pay_money_org);

                    foreach (string opt2 in paymethod_org)
                    {
                        String[] temp2 = opt2.Split('-');
                        if (temp2.Length == 2)
                        {
                            tx_id_chk = temp2[0];
                            paymethod = temp2[1];
                        }


                        if (tx_id.Equals(tx_id_chk))
                        {
                            result = dao.UpdateAPPLY_PAY_PAYMETHOD(paymethod, app_id, tx_id, GetAccount());
                        }
                    }
                }
            }

            if (tx_id_new.Equals("newtx"))
            {
                upd_flag = "1";
                pay_aactual_new = pay_aactual_new + int.Parse(pay_money_new);
                string tx_id_db = action.NewPayID(app_id);
                result = dao.InsertAPPLY_PAY(app_id, tx_id_db, pay_money_new, paymethod_new, GetAccount());
            }
            else if (tx_id_new.Equals("nontx"))
            {
                upd_flag = "1";
            }

            //判斷審核後已繳金額是否異動
            if (pay_cactual_new.Equals("1"))
            {
                IsMail = "Y";
                result = dao.UpdateAPPLY_PAY_C_PAID(app_id, pay_cexpect_db, GetAccount());
            }
            if (upd_flag.Equals("1"))
            {
                IsMail = "Y";
                result = dao.UpdateAPPLY_PAY_A_PAID(app_id, pay_aactual_new, GetAccount());
            }
            if (IsMail.Equals("Y"))
            {
                Map map = action.MaintainViewDetailMailDataQuery(app_id);
                String ClientUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/";
                DateTime dt = map.GetDateTime("APP_TIME").HasValue ? map.GetDateTime("APP_TIME").Value : new DateTime();
                String massage = String.Format(MessageUtils.MAIL_MaintainView_BODY, map.GetString("UNIT_NAME"), map.GetString("NAME")
                    , (dt.Year - 1911).ToString(), dt.Month, dt.Day, map.GetString("SRV_NAME"), ClientUrl, app_id, app_id, map.GetString("UNIT_NAME"));
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    MailUtils.SendMail(map.GetString("MAIL"), MessageUtils.MAIL_MaintainView_SUBJECT, massage);
                    //MailUtils.SendMail(conn.BeginTransaction(),"nick@thinkon.com.tw", MessageUtils.MAIL_MaintainView_SUBJECT, massage);
                    conn.Close();
                    conn.Dispose();
                }
            }
            if (upd_flag.Equals("1"))
            {
                if (result)
                {
                    //Log
                    using (SqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        SqlTransaction tran = conn.BeginTransaction();
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "6";
                        log_model.TX_TYPE = 6;
                        log_model.TX_DESC = "案件編號:" + app_id + "<br> 手動入帳";
                        log_action.Insert(log_model, upd_Model);
                        tran.Commit();
                        conn.Close();
                        conn.Dispose();
                    }
                    //log end;
                    TempData["tempMessage"] = "更新成功";
                }
                else
                {
                    TempData["tempMessage"] = "更新失敗";
                }
            }
            else
            {
                TempData["tempMessage"] = "無任何異動";
            }
            return RedirectToAction("MaintainBack", model.queryModel);
        }


        [HttpGet]
        public ActionResult FormatReport()
        {
            FormatReportModel model = new FormatReportModel();
            model.DefaultDay = DateTime.Now.ToString("yyyy/MM/dd");

            this.SetVisitRecord("Pay", "FormatReport", "報表列印");

            return View(model);
        }

        [HttpPost]
        public ActionResult FormatReportPrintOK()
        {
            String Sdate = String.IsNullOrEmpty(Request.Form["Sdate"]) ? "" : Request.Form["Sdate"];
            String Fdate = String.IsNullOrEmpty(Request.Form["Fdate"]) ? "" : Request.Form["Fdate"];
            String unitcd = Request.Form["sel_UNIT"];
            String paystatus = String.IsNullOrEmpty(Request.Form["sel_PAYSTATUS"]) ? "" : Request.Form["sel_PAYSTATUS"];
            String sort = String.IsNullOrEmpty(Request.Form["sel_SORT"]) ? "" : Request.Form["sel_SORT"];
            String[] paymethod = Request.Form.GetValues("paymethod");
            PayAction action = new PayAction();
            ViewBag.List = action.FormatReportOKQuery(Sdate, Fdate, paystatus, unitcd, paymethod, sort);
            ViewBag.Sdate = Sdate;
            ViewBag.Fdate = Fdate;
            return View();
        }

        [HttpPost]
        public ActionResult FormatReportPrintERR()
        {
            String Sdate = String.IsNullOrEmpty(Request.Form["Sdate"]) ? "" : Request.Form["Sdate"];
            String Fdate = String.IsNullOrEmpty(Request.Form["Fdate"]) ? "" : Request.Form["Fdate"];
            String unitcd = Request.Form["sel_UNIT"];
            String[] ispay = Request.Form.GetValues("ispay");
            PayAction action = new PayAction();
            ViewBag.List = action.FormatReportERRQuery(Sdate, Fdate, unitcd, ispay);
            ViewBag.Sdate = Sdate;
            ViewBag.Fdate = Fdate;
            return View();
        }

        [HttpGet]
        public ActionResult PayStoreFile()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            this.SetVisitRecord("Pay", "PayStoreFile", "超商銷帳檔");

            return View();
        }

        [HttpPost]
        public ActionResult PayStoreFileSearch()
        {
            String Text_PayNumber = Request.Form["Text_PayNumber"];
            String Text_Trandate_S = String.IsNullOrEmpty(Request.Form["Text_Trandate_S"]) ? "" : Request.Form["Text_Trandate_S"];
            String Text_Trandate_F = String.IsNullOrEmpty(Request.Form["Text_Trandate_F"]) ? "" : Request.Form["Text_Trandate_F"];
            String Text_Paydate_S = String.IsNullOrEmpty(Request.Form["Text_Paydate_S"]) ? "" : Request.Form["Text_Paydate_S"];
            String Text_Paydate_F = String.IsNullOrEmpty(Request.Form["Text_Paydate_F"]) ? "" : Request.Form["Text_Paydate_F"];

            PayAction action = new PayAction();
            ViewBag.List = action.PayStoreFileSearchQuery(Text_PayNumber, Text_Trandate_S, Text_Trandate_F, Text_Paydate_S, Text_Paydate_F);
            ViewBag.Text_PayNumber = String.IsNullOrEmpty(Text_PayNumber) ? "無" : Text_PayNumber;
            ViewBag.Text_Trandate_S = String.IsNullOrEmpty(Text_Trandate_S) ? "無" : Text_Trandate_S;
            ViewBag.Text_Trandate_F = String.IsNullOrEmpty(Text_Trandate_F) ? "無" : Text_Trandate_F;
            ViewBag.Text_Paydate_S = String.IsNullOrEmpty(Text_Paydate_S) ? "無" : Text_Paydate_S;
            ViewBag.Text_Paydate_F = String.IsNullOrEmpty(Text_Paydate_F) ? "無" : Text_Paydate_F;
            return View();
        }

        [HttpPost]
        public ActionResult PayStoreFileUpload(HttpPostedFileBase Text_UploadFile)
        {
            List<Map> list = null;
            if (Text_UploadFile != null)
            {
                MemoryStream ms = new MemoryStream();

                try
                {
                    if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                    {
                        DateTime now = DateTime.Now;

                        string dir = DataUtils.GetConfig("FOLDER_PAY_STORE") + now.Year.ToString("D4") + "\\" + now.Month.ToString("D2") + "\\";
                        string filename = Path.GetFileName(Request.Files[0].FileName);

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        Request.Files[0].SaveAs(dir + now.ToString("yyyyMMddHHmmss") + "_" + filename);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }

                try
                {
                    byte[] b = new byte[Text_UploadFile.ContentLength];
                    Text_UploadFile.InputStream.Read(b, 0, Text_UploadFile.ContentLength);
                    ms.Write(b, 0, b.Length);
                    list = ReportUtils.readEXCEL(ms);

                    using (SqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        SqlTransaction tran = conn.BeginTransaction();

                        PayAction action = new PayAction(conn, tran);
                        PayDAO dao = new PayDAO();
                        int count = action.UpdateStoreFile(list, GetAccount());

                        if (count > 0)
                        {
                            tran.Commit();
                        }
                        else
                        {
                            tran.Rollback();
                        }

                        TempData["tempMessage"] = "匯入成功，總共 " + list.Count + " 筆，成功匯入 " + count + "筆。";
                        conn.Close();
                        conn.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    if (list != null)
                    {
                        TempData["tempMessage"] = "匯入失敗，總共 " + list.Count + " 筆。";
                    }
                    else
                    {
                        TempData["tempMessage"] = "匯入失敗。";
                    }
                    logger.Warn(ex.Message, ex);
                }
                finally
                {
                    ms.Dispose();
                    ms.Close();
                }
            }
            return RedirectToAction("PayStoreFile");
        }

        [HttpPost]
        public ActionResult GetSC_IDoption(String UNIT_CD)
        {
            PayAction action = new PayAction();
            List<OptionModel> li = action.GetSERVICE_CATEoption(UNIT_CD);
            return Json(li);
        }

        [HttpPost]
        public ActionResult GetSRV_IDoption(String SC_ID)
        {
            PayAction action = new PayAction();
            List<OptionModel> li = action.GetSERVICEoption(SC_ID);
            return Json(li);
        }

        /// <summary>
        /// 檢查信用卡繳費狀態 我的E政府
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCardStatus(string applyId)
        {
            PayDAO dao = new PayDAO();
            List<Dictionary<string, object>> list = null;
            var appId = applyId;
            if (!string.IsNullOrEmpty(Request["applyId"]))
            {
                appId = Request["applyId"];
            }
            if (string.IsNullOrEmpty(appId))
            {
                return Json(true);
            }
            // 實際上不需要手動勾稽，畫面載入會自動勾稽信用卡繳費狀態
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                PayAction action = new PayAction(conn);
                list = action.GetCardList(appId);
                logger.Debug($"CheckCardStatus_applyId:{Request["applyId"]},appId:{appId}");
                conn.Close();
                conn.Dispose();
            }

            if (list != null)
            {
                List<MapUtils> result = new List<MapUtils>();
                MapUtils map = null;

                foreach (Dictionary<string, object> item in list)
                {
                    map = CardUtils.GetAccountingService(item["SESSION_KEY"].ToString(), item["ACCOUNT"].ToString(), item["PSWD"].ToString());

                    if (map != null)
                    {
                        // 重新查詢交易結果，因有案例為第一次接收為交易成功後，實際上經E政府排程/檢核後，二次回傳結果為交易失敗
                        map.Put("PAY_SESSION_KEY", item["SESSION_KEY"].ToString());
                        map.Put("PAY_A_PAID", (map.Get("PAY_TRANS_RET").Equals("0000")) ? item["PAY_A_FEE"].ToString() : "0");
                        map.Put("APP_ID", item["APP_ID"].ToString());
                        var upd_acc = GetAccount();
                        map.Put("UPD_ACC", string.IsNullOrEmpty(upd_acc) ? "" : upd_acc);
                        //map.Put("PAY_METHOD", item["PAY_METHOD"].ToString());

                        logger.Debug($"CheckCardStatus_MAP:{item["SESSION_KEY"].ToString()},PAY_TRANS_RET {map.Get("PAY_TRANS_RET")}");
                        logger.Debug("CardPay");
                        logger.Debug($"PAY_A_FEE:{Convert.ToString(item["PAY_A_FEE"])}");
                        result.Add(map);
                    }
                    else
                    {
                        logger.Error($"CheckCardStatus_SESSION_KEY:{item["SESSION_KEY"].ToString()},map is NULL");
                    }
                }

                if (result != null && result.Count > 0)
                {
                    using (SqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        PayAction action = new PayAction(conn);
                        foreach (MapUtils res in result)
                        {
                            dao.UpdateApplyPay(res.GetItem(), true);
                            logger.Debug($"CheckCardStatus_applyId_UpdateApplyPay:{Request["applyId"]},appId:{appId}");
                        }
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return Json(true);
        }

        /// <summary>
        /// 檢查信用卡繳費狀態 聯合信用卡中心
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckCardStatusEC(string applyId)
        {
            PayDAO dao = new PayDAO();
            List<Dictionary<string, object>> list = null;
            var appId = applyId;
            if (!string.IsNullOrEmpty(Request["applyId"]))
            {
                appId = Request["applyId"];
            }
            if (string.IsNullOrEmpty(appId))
            {
                return Json(true);
            }
            // 實際上不需要手動勾稽，畫面載入會自動勾稽信用卡繳費狀態
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                PayAction action = new PayAction(conn);
                list = action.GetCardList(appId);
                logger.Debug($"CheckCardStatusEC_applyId:{Request["applyId"]},appId:{appId}");
                conn.Close();
                conn.Dispose();
            }

            if (list != null)
            {
                List<MapUtils> result = new List<MapUtils>();
                MapUtils map = null;

                foreach (Dictionary<string, object> item in list)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(item["SESSION_KEY"])))
                    {
                        continue;
                    }
                    else
                    {
                        CreditHPPModel ECmodel = new CreditHPPModel();
                        ECmodel.EncModel.MerchantID = DataUtils.GetConfig("PAY_EC_MERCHANTID");
                        ECmodel.EncModel.TerminalID = DataUtils.GetConfig("PAY_EC_TRMINALID");
                        ECmodel.EncModel.OrderID = Convert.ToString(item["ECORDERID"]);
                        ECmodel.ECConnetModel.DomainName = DataUtils.GetConfig("PAY_EC_DOMAINNAME");
                        ECmodel.ECConnetModel.RequestURL = DataUtils.GetConfig("PAY_EC_REQUESTURL");
                        // 查詢狀態
                        ApiClient apiClient = CardUtils.GetpostQueryEC(ECmodel, Convert.ToString(item["SESSION_KEY"]));

                        if (apiClient != null && !string.IsNullOrEmpty(apiClient.KEY))
                        {
                            map.Put("PAY_AUTH_DATE", apiClient.getTRANSDATE());
                            map.Put("PAY_AUTH_NO", apiClient.getAPPROVECODE());
                            map.Put("PAY_SETTLE_DATE", apiClient.getTRANSDATE());
                            map.Put("PAY_HOST_TIME", apiClient.getTRANSTIME());
                            map.Put("PAY_INFO_NO", apiClient.getPAN());
                            map.Put("PAY_OTHER", apiClient.getORDERID());

                            map.Put("PAY_TRANS_RET", apiClient.getRESPONSECODE());
                            map.Put("PAY_TRANS_MSG", apiClient.getRESPONSEMSG());
                            map.Put("PAY_SESSION_KEY", apiClient.getKEY());

                            map.Put("PAY_A_PAID", (map.Get("PAY_TRANS_RET").Equals("00")) ? map.Get("PAY_A_FEE") : "0");
                            var upd_acc = GetAccount();
                            map.Put("UPD_ACC", string.IsNullOrEmpty(upd_acc) ? "" : upd_acc);

                            logger.Debug($"CheckCardStatusEC_MAP:{Convert.ToString(item["SESSION_KEY"])},PAY_TRANS_RET {map.Get("PAY_TRANS_RET")}");
                            logger.Debug("CardPay");
                            logger.Debug($"PAY_A_FEE:{Convert.ToString(item["PAY_A_FEE"])}");
                            result.Add(map);
                        }
                        else
                        {
                            logger.Error($"CheckCardStatusEC_SESSION_KEY:{item["SESSION_KEY"].ToString()},map is NULL");
                        }

                    }
                }

                if (result != null && result.Count > 0)
                {
                    using (SqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        PayAction action = new PayAction(conn);
                        foreach (MapUtils res in result)
                        {
                            dao.UpdateApplyPay(res.GetItem(), true);
                            logger.Debug($"CheckCardStatus_applyId_UpdateApplyPay:{Request["applyId"]},appId:{appId}");
                        }
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return Json(true);
        }
    }
}
