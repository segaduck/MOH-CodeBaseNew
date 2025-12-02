using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ES.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FlySwipeController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 7;
        }

        //
        // GET: /BACKMIN/FlySwipe/
        [HttpGet]
        public ActionResult Index()
        {
            this.SetVisitRecord("FlySwipe", "Index", "刷卡對帳匯入");
            FlySwipeModel model = new FlySwipeModel();
            FlySwipeAction action = new FlySwipeAction();
            return View(model);
        }

        /// <summary>
        /// 資料查詢
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(FlySwipeModel form)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlySwipeAction action = new FlySwipeAction(conn);

                //FlySwipeSearchGridModel
                form.grids = action.GetFlySwipeList(form);

                //form.QRY_PAY_STATUSList = action.GetPAY_STATUS();
                conn.Close();
                conn.Dispose();
            }
            return View(form);
        }


        #region 資料匯入

        [HttpPost]
        public ActionResult FlySwipeImport(HttpPostedFileBase Text_UploadFile, string banktype)
        {
            List<ImpFlySwipeModel> list = new List<ImpFlySwipeModel>();
            string s_Upload_FileName = "";

            FlySwipeModel model = new FlySwipeModel();
            if (Text_UploadFile == null)
            {
                TempData["tempMessage"] = "沒有選擇檔案!";
                ViewBag.tempMessage = TempData["tempMessage"];
                return View("Index", model);
            }
            if (string.IsNullOrEmpty(banktype))
            {
                TempData["tempMessage"] = "請選擇匯入銀行別";
                ViewBag.tempMessage = TempData["tempMessage"];
                return View("Index", model);
            }
            //MemoryStream ms = new MemoryStream();
            string ofilename = "";
            try
            {
                if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                {
                    DateTime now = DateTime.Now;

                    string dir = DataUtils.GetConfig("FOLDER_TEMPLATE") + now.Year.ToString("D4") + "\\" + now.Month.ToString("D2") + "\\";

                    ofilename = Path.GetFileName(Request.Files[0].FileName);
                    //oExtension = Path.GetExtension(Request.Files[0].FileName);

                    if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

                    s_Upload_FileName = string.Format("{0}{1}_{2}", dir, now.ToString("yyyyMMddHHmmss"), ofilename);

                    Request.Files[0].SaveAs(s_Upload_FileName);
                }

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    FlySwipeAction action = new FlySwipeAction(conn);
                    int count1 = action.InsertFlySweipFile(ofilename, s_Upload_FileName);
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }

            logger.Debug(string.Format("s_Upload_FileName :{0}", s_Upload_FileName));
            // csv 開啟
            StreamReader srr = new StreamReader(s_Upload_FileName);
            //logger.Debug(string.Format("reader is null? :{0}", (reader == null)));
            try
            {
                int i_row = 0;
                int i_count = 0;
                // 中國信託
                if (banktype == "1")
                {
                    //string s_col1 = "商店代號,商店名稱,卡號,授權碼,交易日,交易金額,帳務日,撥款日,交易類型,產品別,EC端末機代號,EC代碼,銀聯卡日期,銀聯卡時間,銀聯卡清算日,銀聯卡追蹤碼,簽單序號後六碼,卡別,類別";
                    //string[] sa_col1 = s_col1.Split(',');
                    const int cst_col_商店代號 = 0;
                    const int cst_col_授權碼 = 3;
                    const int cst_col_交易日 = 4;
                    const int cst_col_交易金額 = 5;
                    const int cst_col_帳務日 = 6;
                    const int cst_col_撥款日 = 7;
                    const int cst_col_銀聯卡追蹤碼 = 15;

                    //!reader.EndOfStream
                    //(srr.Peek() > -1)
                    while (!srr.EndOfStream)
                    {
                        string line = srr.ReadLine();
                        string[] values = line.Split(',');
                        i_row += 1;
                        if (i_row == 1) { continue; }

                        if (values != null && values.Length > 7)
                        {
                            //string s_data_show = string.Format("i_row:{0}", i_row);
                            //s_data_show += string.Format(",STORECODE(商店代號):{0}", values[cst_col_商店代號]);
                            //s_data_show += string.Format(",TRACENO(授權碼):{0}", values[cst_col_授權碼]);
                            //s_data_show += string.Format(",PAYDATE(交易日):{0}", values[cst_col_交易日]);
                            //s_data_show += string.Format(",PAYMONEY(交易金額):{0}", values[cst_col_交易金額]);
                            //s_data_show += string.Format(",BILLINGDATE(帳務日):{0}", values[cst_col_帳務日]);
                            //s_data_show += string.Format(",GRANTDATE(撥款日):{0}", values[cst_col_撥款日]);
                            //s_data_show += string.Format(",GRANTDATE(銀聯卡追蹤碼):{0}", values[cst_col_銀聯卡追蹤碼]);
                            //logger.Debug(s_data_show);

                            // i_row:2,STORECODE(商店代號):="013002864",TRACENO(授權碼):="755527",TRACENO_QID(銀聯卡追蹤碼):="755527"
                            //PAYDATE(交易日):="2020/09/21",PAYMONEY(交易金額):-1,BILLINGDATE(帳務日):="2020/09/22",GRANTDATE(撥款日):="2020/09/23"
                            string s_STORECODE = values[cst_col_商店代號].Replace("=\"", "").Replace("\"", "");
                            string s_TRACENOE = values[cst_col_授權碼].Replace("=\"", "").Replace("\"", "");
                            DateTime? dt_PAYDATE = Convert.ToDateTime(values[cst_col_交易日].Replace("=\"", "").Replace("\"", ""));
                            string s_PAYMONEY = values[cst_col_交易金額];
                            DateTime? dt_BILLINGDATE = Convert.ToDateTime(values[cst_col_帳務日].Replace("=\"", "").Replace("\"", ""));
                            DateTime? dt_GRANTDATE = Convert.ToDateTime(values[cst_col_撥款日].Replace("=\"", "").Replace("\"", ""));
                            string s_TRACENO_QID = values[cst_col_銀聯卡追蹤碼].Replace("=\"", "").Replace("\"", "");
                            // 發卡銀行可能不會給予授權碼
                            if (string.IsNullOrEmpty(s_TRACENOE))
                            {
                                s_TRACENOE = s_TRACENO_QID;
                            }
                            ImpFlySwipeModel fs1 = new ImpFlySwipeModel()
                            {
                                STORECODE = s_STORECODE,
                                TRACENO = s_TRACENOE,
                                PAYDATE = dt_PAYDATE,
                                PAYMONEY = s_PAYMONEY,
                                BILLINGDATE = dt_BILLINGDATE,
                                GRANTDATE = dt_GRANTDATE,
                                TRACENO_QID = s_TRACENO_QID
                            };
                            list.Add(fs1);
                        }
                    }

                }
                else if (banktype == "2")
                {
                    // 玉山銀行
                    //string s_col1 = "商店代號	商店名稱 卡號(前6後4) 授權碼 交易日 交易金額 請款日 撥款日 交易類型(請款/退貨) 
                    //EC端末機代號	銀聯卡日期 銀聯卡時間 銀聯卡清算日 銀聯卡追蹤碼 簽單序號後六碼 卡別(自行/他行/國外/銀聯) 類別(V/J/M/C) EC訂單編號";
                    //string[] sa_col1 = s_col1.Split(',');
                    const int cst_col_商店代號 = 0;
                    const int cst_col_商店名稱 = 1;
                    const int cst_col_卡號 = 2;
                    const int cst_col_授權碼 = 3;
                    const int cst_col_交易日 = 4;
                    const int cst_col_交易金額 = 5;
                    const int cst_col_請款日 = 6;
                    const int cst_col_撥款日 = 7;
                    const int cst_col_交易類型 = 8;
                    const int cst_col_EC端末機代號 = 9;
                    const int cst_col_銀聯卡日期 = 10;
                    const int cst_col_銀聯卡時間 = 11;
                    const int cst_col_銀聯卡清算日 = 12;
                    const int cst_col_銀聯卡追蹤碼 = 13;
                    const int cst_col_簽單序號後六碼 = 14;
                    const int cst_col_卡別 = 15;
                    const int cst_col_類別 = 16;
                    const int cst_col_EC訂單編號 = 17;

                    while (!srr.EndOfStream)
                    {
                        string line = srr.ReadLine();
                        string[] values = line.Split(',');
                        i_row += 1;
                        if (i_row == 1) { continue; }

                        if (values != null && values.Length > 17)
                        {
                            if (string.IsNullOrEmpty(values[cst_col_請款日]) || values[cst_col_請款日].TONotNullString().ToTrim() == "請款日")
                            {
                                continue;
                            }
                            string s_STORECODE = values[cst_col_商店代號].TONotNullString().Replace("=\"", "").Replace("\"", "");
                            string s_TRACENOE = values[cst_col_授權碼].TONotNullString().Replace("=\"", "").Replace("\"", "");
                            DateTime? dt_PAYDATE = Convert.ToDateTime(values[cst_col_交易日].TONotNullString().Replace("=\"", "").Replace("\"", ""));
                            string s_PAYMONEY = values[cst_col_交易金額].TONotNullString();
                            DateTime? dt_BILLINGDATE = Convert.ToDateTime(values[cst_col_請款日].TONotNullString().Replace("=\"", "").Replace("\"", ""));
                            DateTime? dt_GRANTDATE = Convert.ToDateTime(values[cst_col_撥款日].TONotNullString().Replace("=\"", "").Replace("\"", ""));
                            string s_TRACENO_QID = values[cst_col_銀聯卡追蹤碼].TONotNullString().Replace("=\"", "").Replace("\"", "");
                            // 發卡銀行可能不會給予授權碼
                            if (string.IsNullOrEmpty(s_TRACENOE))
                            {
                                s_TRACENOE = s_TRACENO_QID;
                            }
                            ImpFlySwipeModel fs1 = new ImpFlySwipeModel()
                            {
                                STORECODE = s_STORECODE,
                                TRACENO = s_TRACENOE,
                                PAYDATE = dt_PAYDATE,
                                PAYMONEY = s_PAYMONEY,
                                BILLINGDATE = dt_BILLINGDATE,
                                GRANTDATE = dt_GRANTDATE,
                                TRACENO_QID = s_TRACENO_QID
                            };
                            list.Add(fs1);
                        }
                        else
                        {
                            logger.Warn("匯入資料第" + i_row + "行匯入失敗，欄位數不符合。");
                        }
                    }
                }
                else
                {
                    TempData["tempMessage"] = "銀行別參數錯誤!";
                    ViewBag.tempMessage = TempData["tempMessage"];
                    logger.Warn("銀行別參數錯誤");
                    return View("Index", model);
                }

                if (list != null && list.Count > 0)
                {
                    using (SqlConnection conn = DataUtils.GetConnection())
                    {
                        conn.Open();
                        SqlTransaction tran = conn.BeginTransaction();

                        FlySwipeAction action = new FlySwipeAction(conn, tran);

                        i_count = action.InsertFile(list, GetAccount(), banktype);

                        tran.Commit();
                        conn.Close();
                        conn.Dispose();
                    }
                }

                string s_msg2 = "";
                if (i_count != list.Count) { s_msg2 = "(重複的資料不再匯入)"; }
                TempData["tempMessage"] = string.Format("匯入結束，總共 {0} 筆，成功匯入 {1}筆。{2}", list.Count, i_count, s_msg2);
            }
            catch (Exception ex)
            {
                TempData["tempMessage"] = "匯入失敗。";
                if (list != null && list.Count > 0) { TempData["tempMessage"] = "匯入失敗，總共 " + list.Count + " 筆。"; }
                logger.Error(ex.Message, ex);
            }
            srr.Close(); srr.Dispose(); srr = null;


            ViewBag.tempMessage = TempData["tempMessage"] ?? "";
            return View("Index", model);
        }
        #endregion

        #region 玉山單筆查詢
        [HttpPost]
        public ActionResult Condition(FlySwipeModel form)
        {
            SessionModel sm = SessionModel.Get();
            PayDAO dao = new PayDAO();
            var result = new AjaxResultStruct();
            if (string.IsNullOrEmpty(form.ONO))
            {
                result.status = false;
                result.message = "請輸入訂單編號";
                return Content(result.Serialize(), "application/json");
            }

            try
            {
                result = this.EsunAPIQuery(form.ONO);
                // try end
            }
            catch (Exception ex)
            {
                logger.Error("玉山一般卡單筆查詢 failed" + ex.TONotNullString());
                result.message = "系統發生錯誤，請電洽管理員:" + ex.Message;
                result.status = false;
            }
            return Content(result.Serialize(), "application/json");
        }
        [HttpPost]
        public ActionResult ConditionUnion(FlySwipeModel form)
        {
            SessionModel sm = SessionModel.Get();
            PayDAO dao = new PayDAO();
            var result = new AjaxResultStruct();
            if (string.IsNullOrEmpty(form.ONO))
            {
                result.status = false;
                result.message = "請輸入訂單編號";
                return Content(result.Serialize(), "application/json");
            }

            try
            {
                // 銀聯卡
                result = this.EsunUAPIQuery(form.ONO);
                // try end
            }
            catch (Exception ex)
            {
                logger.Error("玉山銀聯卡單筆查詢 failed" + ex.TONotNullString());
                result.message = "系統發生錯誤，請電洽管理員:" + ex.Message;
                result.status = false;
            }
            return Content(result.Serialize(), "application/json");
        }
        /// <summary>
        /// 訂單編號
        /// </summary>
        /// <returns></returns>
        public string RanDomNum()
        {
            Random crandom = new Random();
            var Cra = crandom.Next(100000000);
            var CraString = Cra.TONotNullString().SubstringTo(0, 5).PadLeft(5, '0');

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                FlyPayAction action = new FlyPayAction(conn);
                var GuidCount = this.GetOneGuidDataList(CraString);

                while (GuidCount > 0)
                {
                    Cra = crandom.Next(100000000);
                    CraString = Cra.TONotNullString().SubstringTo(0, 5).PadLeft(5, '0');
                    GuidCount = this.GetOneGuidDataList(CraString);
                }
                conn.Close();
                conn.Dispose();
            }

            return CraString;
        }

        /// <summary>
        /// 訂房編號檢測是否有重複
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int GetOneGuidDataList(string seq)
        {
            PayDAO dao = new PayDAO();
            TblFLYPAYBASIC where = new TblFLYPAYBASIC();
            where.GUID = seq;
            var data = dao.GetRow<TblFLYPAYBASIC>(where);
            if (data != null && !string.IsNullOrEmpty(data.GUID))
            {
                return 1;
            }
            return 0;
        }
        /// <summary>
        /// 防疫旅館 一般訂房查詢
        /// </summary>
        [HttpPost]
        public void ConditionSchedual()
        {
            var result = new AjaxResultStruct();
            PayDAO dao = new PayDAO();
            var payresultList = new List<string>();

            try
            {

                payresultList = dao.QueryEsunPayResultList();

                if (payresultList != null && payresultList.Count > 0)
                {
                    foreach (var item in payresultList)
                    {
                        logger.Debug("玉山排程查詢，訂單編號:" + item);
                        var resultApi = new AjaxResultStruct();
                        // 一般卡
                        resultApi = this.EsunAPIQuery(item);
                        // 交易失敗，GD 查無訂單編號
                        if (resultApi.status == false && resultApi.message.Contains("GD"))
                        {
                            // 銀聯卡
                            resultApi = this.EsunUAPIQuery(item);
                        }
                        logger.Debug("玉山排程查詢結果:" + resultApi.status + "，訂單編號:" + item);
                    }
                }
                logger.Debug("玉山排程查詢結束");
                // try end
            }
            catch (Exception ex)
            {
                logger.Error("玉山排程查詢 failed" + ex.TONotNullString());
            }
        }
        /// <summary>
        /// 防疫旅館 春節專案查詢
        /// </summary>
        [HttpPost]
        public void ApplySPRProject()
        {
            var result = new AjaxResultStruct();
            PayDAO dao = new PayDAO();
            var payresultList = new List<string>();

            try
            {
                // 超過60分鐘即釋放
                payresultList = dao.QueryEsunPayResultList_SPR();
                if (payresultList != null && payresultList.Count > 0)
                {
                    foreach (var item in payresultList)
                    {
                        var resultApi = new AjaxResultStruct();
                        var isSuccess = dao.UpdateSPR_isUse(item);
                        logger.Debug("春節專案訂單編號:fly_id:" + item + "釋放預定名額");
                    }
                }
                logger.Debug("春節專案查詢排程結束");
                // try end
            }
            catch (Exception ex)
            {
                logger.Error("春節專案查詢排程 failed" + ex.TONotNullString());
            }
        }
        /// <summary>
        /// 一般卡查詢
        /// </summary>
        /// <param name="payresult"></param>
        /// <returns></returns>
        public AjaxResultStruct EsunAPIQuery(string payresult)
        {
            FlySwipeModel form = new FlySwipeModel();
            form.ONO = payresult;
            SessionModel sm = SessionModel.Get();
            PayDAO dao = new PayDAO();
            var message = string.Empty;
            var result = new AjaxResultStruct();
            try
            {
                // 原始訂單
                TblFLYPAYBASIC where = new TblFLYPAYBASIC();
                where.PAYRESULT = form.ONO;
                var dataBasic = dao.GetRow(where);
                // 加密查詢條件
                var basedata = new Dictionary<string, string>();
                basedata.Add("MID", DataUtils.GetConfig("FLYPAY_ESUN_MID"));
                basedata.Add("ONO", form.ONO);
                form.es.data = JsonConvert.SerializeObject(basedata);
                var cry256 = form.es.data + DataUtils.GetConfig("FLYPAY_ESUN_MAC");
                form.es.mac = DataUtils.Crypt256BitConverter(cry256.ToTrim());
                form.es.ksn = "1";
                form.es.postUrl = DataUtils.GetConfig("FLYPAY_ESUN_TYPEQRY");
                result.status = true;
                result.data = form.es;

                // API網址
                string url = form.es.postUrl;
                ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
               (sender, cert, chain, sslPolicyErrors) => true;
                // 傳送內容
                NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("data", form.es.data);
                postParams.Add("mac", form.es.mac);
                postParams.Add("ksn", form.es.ksn);

                byte[] byteArray = Encoding.UTF8.GetBytes(postParams.ToString());
                logger.Debug("玉山一般卡單筆查詢:" + form.ONO);

                var webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.UserAgent = "Nothing";
                webRequest.Method = "POST";
                using (Stream reqStream = webRequest.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        // 回傳資料
                        var contributorsAsJson = sr.ReadToEnd();
                        var reContri = contributorsAsJson.TONotNullString().Replace("DATA=", "");
                        dynamic temp = JsonConvert.DeserializeObject(reContri);
                        var returnCode = Convert.ToString(temp["returnCode"]);
                        if (Convert.ToString(temp["returnCode"]) == "00")
                        {
                            var record = Convert.ToString(temp["txnData"]);
                            var tempFinal = JsonConvert.DeserializeObject(record);
                            logger.Debug("temp['txnDAta']:" + record);
                            var rc = Convert.ToString(tempFinal["RC"]);
                            var ono = Convert.ToString(tempFinal["ONO"]);
                            var mid = Convert.ToString(tempFinal["MID"]);
                            var ltd = Convert.ToString(tempFinal["LTD"]);
                            var ltt = Convert.ToString(tempFinal["LTT"]);
                            var rrn = Convert.ToString(tempFinal["RRN"]);
                            var air = Convert.ToString(tempFinal["AIR"]);
                            var txnamount = Convert.ToString(tempFinal["TXNAMOUNT"]);
                            var settleamount = Convert.ToString(tempFinal["SETTLEAMOUNT"]);
                            var settlestatus = Convert.ToString(tempFinal["SETTLESTATUS"]);
                            var an = Convert.ToString(tempFinal["AN"]);
                            logger.Debug("玉山單筆查詢:Success");
                            logger.Debug($"回覆碼:{rc},特店代碼:{mid},訂單編號:{ono},收單交易日期:{ltd},收單交易時間:{ltt},簽帳單序號:{rrn},授權碼:{air},交易金額:{txnamount},剩餘消費金額:{settleamount},訂單狀態:{settlestatus},卡號:{an}");
                            if (rc == "00" || rc == "59")// 交易成功
                            {
                                if (dataBasic != null && dataBasic.STATUS == "Y")
                                {
                                    // 已繳費資料 略過更新
                                    message = $"查詢結果；訂單編號:{ono} 已繳費";
                                    result.status = true;
                                }
                                else if (dataBasic != null && dataBasic.STATUS == "N")
                                {
                                    if (dataBasic.FLIGHTDATE.Replace("-", "").Replace("/", "").TOInt32() >= 20211214)
                                    {
                                        logger.Debug($"春節專案，未完成操作取得訂房識別碼。繳費狀態；訂單編號:{ono} 已繳費");
                                        var dt = ltd.Substring(0, 4) + "-" + ltd.Substring(4, 2) + "-" + ltd.Substring(6, 2);
                                        var tt = ltt.Substring(0, 2) + ":" + ltt.Substring(2, 2) + ":" + ltt.Substring(4, 2);
                                        //var adtime = dao.getAddtime(ono);
                                        var isSuccess = false;
                                        var guid = string.Empty;
                                        //if (Convert.ToDateTime(dt + " " + tt) > Convert.ToDateTime(Convert.ToString(adtime[0])).AddHours(1))
                                        //{
                                        //    // 逾時
                                        //    isSuccess = true;
                                        //}
                                        //else
                                        //{
                                        //    // 時間內
                                        //    //guid = RanDomNum();
                                        //}
                                        dao.UpdateCreditSuccessRecvGuid(ono, record, rrn, air, guid, ltd, ltt, Convert.ToDateTime(dt + " " + tt));
                                        if (isSuccess)
                                        {
                                            message = $"春節專案，未完成操作取得訂房識別碼。繳費狀態；訂單編號:{ono} 已繳費；逾時完成繳費。";
                                            result.status = true;
                                        }
                                        else
                                        {
                                            message = $"春節專案，未完成操作取得訂房識別碼。繳費狀態；訂單編號:{ono} 已繳費；符合繳費時間內完成動作，需另外給予訂房編號。";
                                            result.status = true;
                                        }
                                        logger.Debug(message);
                                    }
                                    else
                                    {
                                        // 更新未繳費
                                        var GUID = RanDomNum();
                                        logger.Debug("OrderNo:" + ono + "RanDomNum():" + GUID);
                                        // 防疫旅館(NEW)
                                        logger.Debug("UpdateCreditSuccessRecvGuid:" + ono + "AN:" + an + "LTD&LTT:" + ltd + " " + ltt);
                                        var dt = ltd.Substring(0, 4) + "-" + ltd.Substring(4, 2) + "-" + ltd.Substring(6, 2);
                                        var tt = ltt.Substring(0, 2) + ":" + ltt.Substring(2, 2) + ":" + ltt.Substring(4, 2);
                                        var isSuccess = dao.UpdateCreditSuccessRecvGuid(ono, record, rrn, air, GUID, ltd, ltt, Convert.ToDateTime(dt + " " + tt));
                                        if (isSuccess)
                                        {
                                            message = $"繳費狀態已更新；訂單編號:{ono} 已繳費";
                                            result.status = true;
                                        }
                                    }
                                }
                                else
                                {
                                    result.status = false;
                                    message = $"查無資料；訂單編號:{ono}";
                                }
                            }
                            else
                            {
                                var rcmsg = string.Empty;
                                #region rc原因
                                if (rc == "10")
                                {
                                    rcmsg = "授權中";
                                }
                                else if (rc == "11")
                                {
                                    rcmsg = "授權失敗";
                                }
                                else if (rc == "19")
                                {
                                    rcmsg = "授權成功(可請款)";
                                }
                                else if (rc == "40")
                                {
                                    rcmsg = "授權取消中";
                                }
                                else if (rc == "41")
                                {
                                    rcmsg = "授權取消失敗(可請款)";
                                }
                                else if (rc == "49")
                                {
                                    rcmsg = "授權取消成功";
                                }
                                else if (rc == "50")
                                {
                                    rcmsg = "請款中";
                                }
                                else if (rc == "51")
                                {
                                    rcmsg = "請款失敗(可請款)";
                                }
                                else if (rc == "59")
                                {
                                    rcmsg = "請款成功(可退貨)";
                                }
                                else if (rc == "60")
                                {
                                    rcmsg = "退貨中";
                                }
                                else if (rc == "61")
                                {
                                    rcmsg = "退貨失敗(可退貨)";
                                }
                                else if (rc == "69")
                                {
                                    rcmsg = "退貨成功";
                                }
                                else if (rc == "70")
                                {
                                    rcmsg = "退貨授權中";
                                }
                                else if (rc == "71")
                                {
                                    rcmsg = "退貨授權失敗";
                                }
                                else if (rc == "79")
                                {
                                    rcmsg = "退貨授權成功";
                                }
                                else
                                {
                                    rcmsg = "無對應代碼";
                                }
                                #endregion
                                // RC != 00
                                message = $"交易失敗，原因:{rc},{rcmsg}，訂單編號:{ono}";
                                result.status = false;
                                if (dataBasic != null && dataBasic.STATUS == "Y")
                                {
                                    message += "\n 請手動收回訂房碼";
                                    var mailer = DataUtils.GetConfig("SERVICEMAIL");
                                    var subject = $"衛福部人民線上玉山銀行單筆查詢交易失敗通知:{rc}";
                                    var mailbody = $"您好，已繳費訂單編號:{ono}，經查詢交易失敗原因為:{rc}，需手動收回訂房碼";
                                    dao.SendMail(mailer, subject, mailbody);
                                }
                            }
                        }
                        else
                        {
                            message = $"查詢失敗，ReturnCode:{returnCode}";
                            result.status = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("玉山一般卡單筆查詢 failed" + ex.TONotNullString());
                message = "系統發生錯誤，請電洽管理員:" + ex.Message;
                result.status = false;
            }
            result.message = message;
            return result;
        }
        /// <summary>
        /// 銀聯卡查詢
        /// </summary>
        /// <param name="payresult"></param>
        /// <returns></returns>
        public AjaxResultStruct EsunUAPIQuery(string payresult)
        {
            FlySwipeModel form = new FlySwipeModel();
            form.ONO = payresult;
            SessionModel sm = SessionModel.Get();
            PayDAO dao = new PayDAO();
            var message = string.Empty;
            var result = new AjaxResultStruct();
            try
            {
                // 原始訂單
                TblFLYPAYBASIC where = new TblFLYPAYBASIC();
                where.PAYRESULT = form.ONO;
                var dataBasic = dao.GetRow(where);
                // 加密查詢條件
                form.esu.MID = DataUtils.GetConfig("FLYPAY_ESUN_MID");
                form.esu.CID = string.Empty;
                form.esu.ONO = form.ONO;
                form.esu.TA = dataBasic.PAYMONEY;
                form.esu.TT = "00";
                form.esu.U = string.Empty;
                form.esu.TXNNO = string.Empty;
                form.esu.M = DataUtils.CryptMD5($"{form.esu.MID}&{form.esu.CID}&{form.esu.ONO}&{form.esu.TA}&{form.esu.TT}&{form.esu.U}&{form.esu.TXNNO}&" + DataUtils.GetConfig("FLYPAY_ESUN_MAC"));
                form.esu.postUrl = DataUtils.GetConfig("FLYPAY_ESUN_UPOST");
                result.status = true;
                result.data = form.esu;

                // API網址
                string url = form.esu.postUrl;
                ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
               (sender, cert, chain, sslPolicyErrors) => true;
                // 傳送內容
                NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("MID", form.esu.MID);
                postParams.Add("CID", form.esu.CID);
                postParams.Add("ONO", form.esu.ONO);
                postParams.Add("TA", form.esu.TA);
                postParams.Add("TT", form.esu.TT);
                postParams.Add("U", form.esu.U);
                postParams.Add("TXNNO", form.esu.TXNNO);
                postParams.Add("M", form.esu.M);
                logger.Debug("MID:" + form.esu.MID + "CID:" + form.esu.CID + "ONO:" + form.esu.ONO + "TA:" + form.esu.TA + "TT:" + form.esu.TT + "U:" + form.esu.U + "txnno:" + form.esu.TXNNO + "M:" + form.esu.M);
                byte[] byteArray = Encoding.UTF8.GetBytes(postParams.ToString());
                logger.Debug("玉山銀聯卡單筆查詢:" + form.ONO);

                var webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.UserAgent = "Nothing";
                webRequest.Method = "POST";
                using (Stream reqStream = webRequest.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        // 回傳資料
                        var contributorsAsJson = sr.ReadToEnd();
                        var reContri = contributorsAsJson.TONotNullString();
                        var splt = reContri.ToSplit(',');
                        Dictionary<string, string> temp = new Dictionary<string, string>();
                        foreach (var item in splt)
                        {
                            var keyname = item.ToSplit('=').FirstOrDefault();
                            var keyvalue = item.ToSplit('=').LastOrDefault();
                            temp.Add(keyname, keyvalue);
                        }
                        var record = reContri;
                        var returnCode = Convert.ToString(temp["RC"]);
                        if (Convert.ToString(temp["RC"]) == "00")
                        {
                            logger.Debug("temp['txnDAta']:" + reContri);
                            var rc = temp["RC"].TONotNullString();
                            var ono = temp["ONO"].TONotNullString();
                            var mid = temp["MID"].TONotNullString();
                            var ltd = temp["LTD"].TONotNullString();
                            var ltt = temp["LTT"].TONotNullString();
                            var tracenumber = temp["TRACENUMBER"].TONotNullString();
                            var tracetime = temp["TRACETIME"].TONotNullString();
                            var txnno = temp["TXNNO"].TONotNullString();
                            logger.Debug("玉山銀聯卡單筆查詢:Success");
                            logger.Debug($"回覆碼:{rc},特店代碼:{mid},訂單編號:{ono},收單交易日期:{ltd},收單交易時間:{ltt},系統跟蹤號:{tracenumber},系統跟蹤時間:{tracetime},交易序號:{txnno}");
                            if (rc == "00" || rc == "59")// 交易成功
                            {
                                if (dataBasic != null && dataBasic.STATUS == "Y")
                                {
                                    // 已繳費資料 略過更新
                                    message = $"查詢結果；訂單編號:{ono} 已繳費";
                                    result.status = true;
                                }
                                else if (dataBasic != null && dataBasic.STATUS == "N")
                                {
                                    if (dataBasic.FLIGHTDATE.Replace("-", "").Replace("/", "").TOInt32() >= 20211214)
                                    {
                                        logger.Debug($"春節專案，未完成操作取得訂房識別碼。繳費狀態；訂單編號:{ono} 已繳費");
                                        var dt = ltd.Substring(0, 4) + "-" + ltd.Substring(4, 2) + "-" + ltd.Substring(6, 2);
                                        var tt = ltt.Substring(0, 2) + ":" + ltt.Substring(2, 2) + ":" + ltt.Substring(4, 2);
                                        //var adtime = dao.getAddtime(ono);
                                        var isSuccess = false;
                                        var guid = string.Empty;
                                        //if (Convert.ToDateTime(dt + " " + tt) > Convert.ToDateTime(Convert.ToString(adtime[0])).AddHours(1))
                                        //{
                                        //    // 逾時
                                        //    isSuccess = true;
                                        //}
                                        //else
                                        //{
                                        //    // 時間內
                                        //    //guid = RanDomNum();
                                        //}
                                        dao.UpdateCreditSuccessRecvGuid(ono, record, txnno, tracenumber, guid, ltd, ltt, Convert.ToDateTime(dt + " " + tt));
                                        if (isSuccess)
                                        {
                                            message = $"春節專案，未完成操作取得訂房識別碼。繳費狀態；訂單編號:{ono} 已繳費；逾時完成繳費。";
                                            result.status = true;
                                        }
                                        else
                                        {
                                            message = $"春節專案，未完成操作取得訂房識別碼。繳費狀態；訂單編號:{ono} 已繳費；符合繳費時間內完成動作，需另外給予訂房編號。";
                                            result.status = true;
                                        }
                                        logger.Debug(message);

                                    }
                                    else
                                    {
                                        // 更新未繳費
                                        var GUID = RanDomNum();
                                        logger.Debug("OrderNo:" + ono + "RanDomNum():" + GUID);
                                        // 防疫旅館(NEW)
                                        logger.Debug("UpdateCreditSuccessRecvGuid:" + ono + "TRACENUMBER:" + tracenumber + "LTD&LTT:" + ltd + " " + ltt);
                                        var dt = ltd.Substring(0, 4) + "-" + ltd.Substring(4, 2) + "-" + ltd.Substring(6, 2);
                                        var tt = ltt.Substring(0, 2) + ":" + ltt.Substring(2, 2) + ":" + ltt.Substring(4, 2);
                                        var isSuccess = dao.UpdateCreditSuccessRecvGuid(ono, record, txnno, tracenumber, GUID, ltd, ltt, Convert.ToDateTime(dt + " " + tt));
                                        if (isSuccess)
                                        {
                                            message = $"繳費狀態已更新；訂單編號:{ono} 已繳費";
                                            result.status = true;
                                        }
                                    }
                                }
                                else
                                {
                                    result.status = false;
                                    message = $"查無資料；訂單編號:{ono}";
                                }
                            }
                        }
                        else
                        {
                            // RC != 00
                            message = $"查詢失敗，ReturnCode:{returnCode}";
                            result.status = false;
                            if (dataBasic != null && dataBasic.STATUS == "Y" && dataBasic.PAYRETURN.Contains("TXNNO"))
                            {
                                message += "\n 請手動收回訂房碼";
                                var mailer = DataUtils.GetConfig("SERVICEMAIL");
                                var subject = $"衛福部人民線上玉山銀行單筆查詢交易失敗通知:{Convert.ToString(temp["RC"])}";
                                var mailbody = $"您好，已繳費訂單編號:{Convert.ToString(temp["ONO"])}，經查詢交易失敗原因為:{Convert.ToString(temp["RC"])}，需手動收回訂房碼";
                                dao.SendMail(mailer, subject, mailbody);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("玉山銀聯單筆查詢 failed" + ex.TONotNullString());
                message = "系統發生錯誤，請電洽管理員:" + ex.Message;
                result.status = false;
            }
            result.message = message;
            return result;
        }
        /// <summary>
        /// 防疫旅館 春節專案流水號
        /// </summary>
        [HttpPost]
        public void SetSPRNum()
        {
            var result = new AjaxResultStruct();
            PayDAO dao = new PayDAO();
            var resultList = new List<FlyPaySPRViewModel>();
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    FlyPayAction action = new FlyPayAction(conn);
                    resultList = action.QuerySPRNumIsNull();
                    conn.Close();
                    conn.Dispose();
                }
                if (resultList != null && resultList.Count > 0)
                {
                    foreach (var item in resultList)
                    {
                        logger.Debug("春節專案流水號");
                        var resultApi = new AjaxResultStruct();
                        var sprcode = dao.UpdateSPRNum(item);
                        logger.Debug("春節專案流水號:fly_id:" + item.FLY_ID + ", sprcode:" + sprcode);
                    }
                }
                logger.Debug("春節專案流水號排程結束");
                // try end
            }
            catch (Exception ex)
            {
                logger.Error("春節專案流水號排程 failed" + ex.TONotNullString());
            }
        }
        #endregion

    }
}
