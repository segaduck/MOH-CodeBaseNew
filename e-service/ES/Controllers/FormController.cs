using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Action;
using ES.Utils;
using log4net;
using ES.Models;
using System.Web.Configuration;
using System.IO;
using ES.Extensions;
using ES.Service.GSP1;
using ES.Service.GSP2;
using Microsoft.Office.Interop.Word;
using ES.Action.Form;

namespace ES.Controllers
{

    public class FormController : BaseController
    {

        //ApplicationClass wordApplication = new ApplicationClass();
        //Document wordDocument = null;
        //object paramSourceDocPath = @"C:\Temp\Test.docx";//Sample Path 
        object paramMissing = Type.Missing;
        /// <summary>
        /// 線上申辦
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[Authorize(Roles = "Member")]
        //public ActionResult Apply(string id)
        //{
        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        FormAction action = new FormAction(conn);
        //        ViewBag.Dict = action.GetFormBase(id);

        //        if (ViewBag.Dict["TABLE_COUNT"].Equals("0") && ViewBag.Dict["SRV_FIELD"].Length < 10)
        //        {
        //            MessageBoxModel msg = new MessageBoxModel("Index", "Service", "查無表單設定資料，請聯絡管理人員設定");
        //            TempData["MessageBoxModel"] = msg;
        //            return RedirectToAction("Index", "MessageBox");
        //        }

        //        // 檢查 登入方式 (CA_TYPE) 及 申辦資格 (APP_TARGET)
        //        Dictionary<string, string> form = ViewBag.Dict;
        //        Dictionary<string, string> member = action.GetMember(GetAccount());

        //        Session["FORM_" + id] = form;

        //        Dictionary<string, string> userData = GetUserData();
        //        string loginType = userData["MemberLoginType"];
        //        if (form["CA_TYPE"].IndexOf(loginType) < 0)
        //        {
        //            MessageBoxModel msg = new MessageBoxModel("Index", "Service", "此申辦項目無法使用「" + GetLoginType(loginType) + "」方式申請。");
        //            TempData["MessageBoxModel"] = msg;
        //            return RedirectToAction("Index", "MessageBox");
        //        }
        //        else if (loginType.Equals("MEMBER")) // 一般登入
        //        {
        //            if (member["IDN"].Length == 10) // 一般會員
        //            {
        //                if (form["APP_TARGET"].IndexOf("N") < 0)
        //                {
        //                    MessageBoxModel msg = new MessageBoxModel("Index", "Service", "此申辦項目「自然人會員」無法申請。");
        //                    TempData["MessageBoxModel"] = msg;
        //                    return RedirectToAction("Index", "MessageBox");
        //                }
        //            }
        //            else // 公司
        //            {
        //                if (form["APP_TARGET"].IndexOf("L") < 0)
        //                {
        //                    MessageBoxModel msg = new MessageBoxModel("Index", "Service", "此申辦項目「法人會員」無法申請。");
        //                    TempData["MessageBoxModel"] = msg;
        //                    return RedirectToAction("Index", "MessageBox");
        //                }
        //            }
        //        }

        //        HistoryAction action2 = new HistoryAction(conn);
        //        action2.setPageSize(10);

        //        HistoryActionModel model2 = new HistoryActionModel();
        //        model2.Account = GetAccount();
        //        model2.NowPage = 1;
        //        model2.ServiceId = id;
        //        model2.ApplyDateS = DateTime.Parse(DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"));

        //        //logger.Debug(model2.Account + " / " + model2.ServiceId + " / " + model2.ApplyDateS + " / " + action2.GetOnlineList(model2).Count);

        //        ViewBag.List = action2.GetOnlineList(model2);

        //        double pageSize = action2.GetPageSize();
        //        double totalCount = action2.GetTotalCount();

        //        ViewBag.NowPage = model2.NowPage;
        //        ViewBag.TotalCount = action2.GetTotalCount();
        //        ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);
        //    }
        //    ViewBag.ServiceId = id;
        //    if (id == "005001" || id == "005002" || id == "005003" || id == "005004" || id == "005005")
        //    {
        //        ViewBag.DocumentId = TempData["DocumentId"];
        //    }                
        //    else
        //        ViewBag.DocumentId = "";

        //    if (id.StartsWith("007"))
        //    {
        //        return View("U7_Apply");
        //    }

        //    return View();
        //}


        /// <summary>
        /// 線上申辦 前置畫面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [Authorize(Roles = "Member")]
        public ActionResult Lead(string id)
        {
            Models.LeadModel.DocumentFormat DFModel = new LeadModel.DocumentFormat();
            DFModel.SRV_NO = id;
            var isMsgBox = false;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);

                ViewBag.Dict = action.GetFormBase(id);
                if (ViewBag.Dict["TABLE_COUNT"].Equals("0") && ViewBag.Dict["SRV_FIELD"].Length < 10)
                {
                    MessageBoxModel msg = new MessageBoxModel("Index", "Service", "查無表單設定資料，請聯絡管理人員設定");
                    TempData["MessageBoxModel"] = msg;
                    isMsgBox = true;
                }
                if (isMsgBox == false)
                {
                    // 檢查 登入方式 (CA_TYPE) 及 申辦資格 (APP_TARGET)
                    Dictionary<string, string> form = ViewBag.Dict;
                    Dictionary<string, string> member = action.GetMember(GetAccount());

                    Session["FORM_" + id] = form;

                    Dictionary<string, string> userData = GetUserData();
                    string loginType = userData["MemberLoginType"];

                    if (form["CA_TYPE"].IndexOf(loginType) < 0)
                    {
                        MessageBoxModel msg = new MessageBoxModel("Index", "Service", "此申辦項目無法使用「" + GetLoginType(loginType) + "」方式申請。");
                        TempData["MessageBoxModel"] = msg;
                        isMsgBox = true;
                    }
                    else if (loginType.Equals("MEMBER")) // 一般登入
                    {
                        if (member["IDN"].Length == 10) // 一般會員
                        {
                            if (form["APP_TARGET"].IndexOf("N") < 0)
                            {
                                MessageBoxModel msg = new MessageBoxModel("Index", "Service", "此申辦項目「自然人會員」無法申請。");
                                TempData["MessageBoxModel"] = msg;
                                isMsgBox = true;
                            }
                        }
                        else // 公司
                        {
                            if (form["APP_TARGET"].IndexOf("L") < 0)
                            {
                                MessageBoxModel msg = new MessageBoxModel("Index", "Service", "此申辦項目「法人會員」無法申請。");
                                TempData["MessageBoxModel"] = msg;
                                isMsgBox = true;
                            }
                        }
                    }

                    Dictionary<string, string> Document = action.GetDocumentBase(id);
                    int APP_FEE = 0;
                    try
                    {
                        APP_FEE = int.Parse(action.GetAPP_FEE(id));
                    }
                    catch
                    { }

                    if (member != null && Document != null)
                    {
                        DFModel.Title = member["NAME"];
                        DFModel.Address = member["ADDR"];
                        DFModel.Name = member["CNT_NAME"];
                        DFModel.Tel = member["TEL"];
                        DFModel.Fax = member["FAX"];
                        DFModel.EMail = member["MAIL"];
                        DFModel.APP_FEE = APP_FEE;
                        DFModel.AppCnt = "1";
                        DFModel.TotalAmount = APP_FEE.ToString();
                        DFModel.SubjectText = Document["SUBJECT"];
                        DFModel.Caption1 = Document["CAPTION1"];
                        DFModel.Caption2 = Document["CAPTION2"];
                        DFModel.Caption3 = Document["CAPTION3"];

                        ViewBag.ApplicationList = CodeUtils.GetNumSelectList(1, 10, DFModel.AppCnt);
                        ViewBag.PayMethod = CodeUtils.GetPayMethodSelectList(DFModel.PayMethod);
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            if (isMsgBox)
            {
                return RedirectToAction("Index", "MessageBox");
            }

            return View(DFModel);
        }

        /// <summary>
        /// 線上申請 前置畫面儲存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Member")]
        public ActionResult LeadSave(Models.LeadModel.DocumentFormat data)
        {
            if (data.SRV_NO == "005004" || data.SRV_NO == "005005")
            {
                ModelState.Remove("DrugName");
                ModelState.Remove("LicNum");

                if (data.SRV_NO == "005004")
                    ModelState.Remove("TotalAmount");
            }

            if (ModelState.IsValid)
            {
                var isLead = false;
                var isApply = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    FormAction action = new FormAction(conn);

                    try
                    {
                        if (int.Parse(data.TotalAmount) != (int.Parse(data.AppCnt) * data.APP_FEE))
                        {
                            isLead = true;
                        }
                    }
                    catch
                    {
                        isLead = true;
                    }

                    if (isLead)
                    {

                    }
                    else
                    {
                        string No = action.LeadSave(data);
                        data.NO = No;
                        if (!String.IsNullOrEmpty(No))
                        {
                            TempData["DocumentId"] = No;
                            isApply = true;
                        }
                    }

                    conn.Close();
                    conn.Dispose();
                }
                if (isLead)
                {
                    return RedirectToAction("Lead", new { id = data.SRV_NO });
                }
                if (isApply)
                {
                    return RedirectToAction("Apply", new { id = data.SRV_NO });
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult CheckExpirDate(DateTime ExpirDate)
        {
            string Result = "";
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                Result = action.CheckExpirDate(ExpirDate.AddYears(-2).ToString("yyyy/MM/dd"), ExpirDate.ToString("yyyy/MM/dd"), GetAccount());
                conn.Close();
                conn.Dispose();
            }

            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 線上申辦預覽
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        /// 

        [HttpPost]
        [Authorize(Roles = "Member")]
        public ActionResult Preview(FormCollection fc)
        {
            string id = Request.Form["SRV_ID"].ToString();
            ViewBag.Type = "F";
            int fileSizeLimit = 5 * 1024 * 1024;
            var isErrorMsg = false;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                Dictionary<string, string> form = null;

                if (Session["FORM_" + id] == null)
                {
                    form = action.GetFormBase(id);
                }
                else
                {
                    form = (Dictionary<string, string>)Session["FORM_" + id];
                }
                ViewBag.Dict = form;
                Session["FORM_" + id] = form;

                List<Dictionary<string, string>> list = action.GetFormTableList(form["SRV_FIELD"], id);
                Dictionary<string, List<SelectListItem>> item = new Dictionary<string, List<SelectListItem>>();
                Dictionary<string, List<Dictionary<string, object>>> dbList = new Dictionary<string, List<Dictionary<string, object>>>();
                Dictionary<string, List<Dictionary<string, string>>> dictList = new Dictionary<string, List<Dictionary<string, string>>>();
                Dictionary<string, HttpPostedFileBase> files = GetHttpPostedFileDictionary();
                Dictionary<string, object> fieldData = action.GetFormField(form["SRV_FIELD"], id);
                list = (List<Dictionary<string, string>>)fieldData["TABLE"];

                foreach (Dictionary<string, string> dict in list)
                {
                    dictList.Add(dict["TABLE_ID"], ((Dictionary<string, List<Dictionary<string, string>>>)fieldData["FIELD"])[dict["TABLE_ID"]]);

                    foreach (Dictionary<string, string> fields in dictList[dict["TABLE_ID"]])
                    {
                        if (!String.IsNullOrEmpty(fields["FORM_FIELD_CD"]))
                        {
                            if (dict["TABLE_TYPE"].StartsWith("1"))
                            {
                                //logger.Debug("tbody_" + dict["TABLE_ID"] + "_max: " + fc["tbody_" + dict["TABLE_ID"] + "_max"]);
                                int max = Int32.Parse(fc["tbody_" + dict["TABLE_ID"] + "_max"]);

                                for (int i = 1; i < max; i++)
                                {
                                    string fieldName = fields["FIELD_NAME"] + "_" + (i + 1);

                                    if (files.ContainsKey(fieldName) && files[fieldName].ContentLength > 0)
                                    {
                                        if (files[fieldName].ContentLength > fileSizeLimit)
                                        {
                                            ViewBag.tempMessage = "上傳檔案大小超過限制<br/>大小限制為：" + (fileSizeLimit / 1024 / 1024) + "MB<br/>目前上傳檔案大小為 " + (files[fieldName].ContentLength / 1024 / 1024) + " MB";
                                            logger.Debug("FormController_Preview: " + ViewBag.tempMessage);
                                            isErrorMsg = true;
                                        }
                                        else if (fields["FORM_FIELD_CD"].Equals("42") && !files[fieldName].ContentType.StartsWith("image/"))
                                        {
                                            ViewBag.tempMessage = "上傳檔案格式錯誤，只能為圖檔<br/>目前上傳之檔案類型為：" + files[fieldName].ContentType;
                                            logger.Debug("FormController_Preview: " + ViewBag.tempMessage);
                                            isErrorMsg = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (files.ContainsKey(fields["FIELD_NAME"]) && files[fields["FIELD_NAME"]].ContentLength > 0)
                                {
                                    //logger.Debug("size: " + files[fields["FIELD_NAME"]].ContentLength);

                                    if (files[fields["FIELD_NAME"]].ContentLength > fileSizeLimit)
                                    {
                                        ViewBag.tempMessage = "上傳檔案大小超過限制<br/>大小限制為：" + (fileSizeLimit / 1024 / 1024) + "MB<br/>目前上傳檔案大小為 " + (files[fields["FIELD_NAME"]].ContentLength / 1024 / 1024) + " MB";
                                        isErrorMsg = true;
                                    }
                                    else if (fields["FORM_FIELD_CD"].Equals("42") && !files[fields["FIELD_NAME"]].ContentType.StartsWith("image/"))
                                    {
                                        ViewBag.tempMessage = "上傳檔案格式錯誤，只能為圖檔<br/>目前上傳之檔案類型為：" + files[fields["FIELD_NAME"]].ContentType;
                                        isErrorMsg = true;
                                    }
                                }
                            }
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            if (isErrorMsg)
            {
                return View("ErrorMessage");
            }
            ViewBag.ServiceId = id;

            if (id == "005001" || id == "005002" || id == "005003" || id == "005004" || id == "005005")
            {
                ViewBag.DocumentId = fc["DocumentId"];
            }

            if (id.StartsWith("007"))
            {
                return View("U7_Preview");
            }

            return View();
        }

        /// <summary>
        /// 選擇繳費方式
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Member")]
        public ActionResult PayMethod(FormCollection fc)
        {
            string id = Request.Form["SRV_ID"].ToString();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                Dictionary<string, string> dict = action.GetFormBase(id);
                Dictionary<string, string> data = action.GetTempData(GetAccount(), fc["APP_TIME"], id);
                List<string> DelPayMethod = action.GetDelPayMethod();
                dict.Add("APP_TIME", fc["APP_TIME"]);
                dict.Add("", "0");

                //暫時取消某種付款方式，1、DB 【CODE_CD】欄位【CODE_KIND = 'PAY_METHOD'】付款方式的Del_MK設定'Y' 2、DB【SETUP】欄位【CanelPayMethod】給起始日
                DateTime StartDate = String.IsNullOrEmpty(DataUtils.GetConfig("CanelPayMethod")) ? DateTime.Parse("2999/01/01") : DateTime.Parse(DataUtils.GetConfig("CanelPayMethod"));
                if (DelPayMethod.Count > 0 && (System.DateTime.Today >= StartDate))
                {
                    foreach (var item in DelPayMethod)
                    {
                        dict["PAY_METHOD"] = dict.FirstOrDefault(x => x.Key == "PAY_METHOD").Value.Replace(item[0].ToString(), "");
                    }
                }
                //--------------------------------------------------------------------------------------

                ViewBag.Dict = dict;
                ViewBag.Data = data;
                conn.Close();
                conn.Dispose();
            }

            if (id == "005001" || id == "005002" || id == "005003" || id == "005004" || id == "005005")
            {
                ViewBag.DocumentId = fc["DocumentId"];
            }

            if (id.StartsWith("007"))
            {
                return View("U7_PayMethod");
            }

            return View();
        }

        /// <summary>
        /// 填寫繳費資料
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Member")]
        public ActionResult Pay()
        {
            string id = Request.Form["SRV_ID"].ToString();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                ViewBag.Dict = action.GetFormBase(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 完成申辦手續
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Member")]
        public ActionResult Success(FormCollection fc)
        {
            string id = CheckUtils.GetServiceId(fc["SRV_ID"]);
            var isErrorMsg = false;
            var isKeyMsg = false;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                FormAction action = new FormAction(conn, tran);

                Dictionary<string, string> form = action.GetFormBase(id);
                if (form["SRV_ID"] == "005001" || form["SRV_ID"] == "005002" || form["SRV_ID"] == "005003" || form["SRV_ID"] == "005004" || form["SRV_ID"] == "005005")
                    form.Add("DocumentId", fc["DocumentId"]);

                MapUtils data = new MapUtils(action.GetTempData(GetAccount(), fc["APP_TIME"], form["SRV_ID"]));

                if (data == null)
                {
                    ViewBag.tempMessage = "此申請單已完成申請，請勿重複申請。";
                    isErrorMsg = true;
                }
                else
                {
                    if (fc["PAY_METHOD"] != null && fc["PAY_METHOD"].Equals("C"))
                    {
                        data.Put("PAY_METHOD", fc["PAY_METHOD"] == null ? "" : fc["PAY_METHOD"]);
                        data.Put("PAY_CARD_IDN", fc["CARD_IDN"] == null ? "" : fc["CARD_IDN"]);
                        data.Put("PAY_A_FEEBK", "0");
                        data.Put("APP_FEE", (Int32.Parse(data.Get("PAY_A_FEE")) + Int32.Parse(data.Get("PAY_A_FEEBK"))).ToString());
                        data.Put("PAY_ID", action.GetPaySerial(form["SRV_ID"], data.Get("APP_TIME").Substring(0, 8)));
                        data.Put("PAY_CLIENT_IP", GetClientIP());
                        //data.Put("PAY_OID", DataUtils.GetConfig("PAY_CARD_OID"));
                        //data.Put("PAY_SID", DataUtils.GetConfig("PAY_CARD_SID"));

                        //logger.Debug("PAY_ACCOUNT: " + form["PAY_ACCOUNT"]);

                        Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(form["PAY_ACCOUNT"]));
                        data.Put("PAY_OID", pay["OID"]);
                        data.Put("PAY_SID", pay["SID"]);

                        SessionKeyResponse res = CardUtils.GetTransactionKey(data.GetItem(), pay);

                        if (res != null)
                        {
                            ViewBag.ErrorCode = res.ResultInfo;
                            ViewBag.ErrorMessage = action.GetPayCodeDesc(res.ResultInfo);
                            if (res.ResultInfo.Equals("0000"))
                            {
                                string sessionKey = res.SessionTransactionKey;
                                data.Put("PAY_SESSION_KEY", sessionKey);
                                logger.Debug("SessionTransactionKey: " + sessionKey);

                                data.Put("PAY_METHOD", fc["PAY_METHOD"] == null ? "" : fc["PAY_METHOD"]);
                                data.Put("PAY_A_FEEBK", data.Get("PAY_METHOD").Equals("S") ? DataUtils.GetConfig("PAY_STORE_FEE") : "0");
                                data.Put("APP_FEE", (Int32.Parse(data.Get("PAY_A_FEE")) + Int32.Parse(data.Get("PAY_A_FEEBK"))).ToString());
                                data.Put("APP_ID", action.GetApplySerial(form["SRV_ID"], data.Get("APP_TIME").Substring(0, 8)));
                                data.Put("PAY_ID", action.GetPaySerial(form["SRV_ID"], data.Get("APP_TIME").Substring(0, 8)));
                                data.Put("PAY_A_PAID", "0");
                                data.Put("ACC_NO", GetAccount());
                                data.Put("UPD_ACC", GetAccount());

                                moveFolder(data.GetItem());

                                if (action.InsertApply(data.GetItem(), form))
                                {
                                    action.DeleteTempData(GetAccount(), data.Get("APP_TIME"));
                                    tran.Commit();
                                }

                                ViewBag.SessionTransactionKey = sessionKey;
                            }
                            else
                            {
                                ViewBag.tempMessage = "取得預約交易代碼失敗(" + res.ResultInfo + ")";
                            }
                        }
                        else
                        {
                            ViewBag.ErrorCode = "-1";
                            ViewBag.ErrorMessage = "連接失敗";
                            ViewBag.tempMessage = "取得預約交易代碼失敗(-1)";
                        }
                        isKeyMsg = true;
                    }
                    if (isKeyMsg)
                    {

                    }
                    else
                    {
                        data.Put("PAY_METHOD", fc["PAY_METHOD"] == null ? "" : fc["PAY_METHOD"]);
                        data.Put("PAY_A_FEEBK", data.Get("PAY_METHOD").Equals("S") ? DataUtils.GetConfig("PAY_STORE_FEE") : "0");
                        data.Put("APP_FEE", (Int32.Parse(data.Get("PAY_A_FEE")) + Int32.Parse(data.Get("PAY_A_FEEBK"))).ToString());
                        data.Put("APP_ID", action.GetApplySerial(form["SRV_ID"], data.Get("APP_TIME").Substring(0, 8)));
                        data.Put("PAY_ID", action.GetPaySerial(form["SRV_ID"], data.Get("APP_TIME").Substring(0, 8)));
                        data.Put("PAY_A_PAID", "0");
                        data.Put("ACC_NO", GetAccount());
                        data.Put("UPD_ACC", GetAccount());

                        if (data.Get("PAY_METHOD").Equals("S"))
                        {
                            string stordId = action.GetPayStoreSerial(data.Get("APP_TIME").Substring(0, 6));
                            data.Put("PAY_SESSION_KEY", PayUtils.GetVirtualAccount(stordId, Int32.Parse(data.Get("APP_FEE"))));
                            // PayUtils.GetVirtualAccount("00000000", (int)data["PAY_A_FEE"])
                        }

                        moveFolder(data.GetItem());

                        if (action.InsertApply(data.GetItem(), form))
                        {
                            action.DeleteTempData(GetAccount(), data.Get("APP_TIME"));
                            tran.Commit();

                            SuccessSendMail(form, data);
                        }
                        else
                        {
                            tran.Rollback();
                        }

                        /*
                        foreach (string key in data.Keys)
                        {
                            logger.Debug(key + ": " + data[key]);
                        }
                        */

                        //logger.Debug("Serial: " + action.GetApplySerial(id, fc["APP_TIME"].Substring(0, 8)));

                        ViewBag.Dict = form;
                        ViewBag.Data = data.GetItem();
                    }
                }
            }
            if (isErrorMsg)
            {
                return View("ErrorMessage");
            }
            if (isKeyMsg)
            {
                return View("TransactionKey");
            }
            if (id.StartsWith("007"))
            {
                return View("U7_Success");
            }

            return View();
        }

        private void SuccessSendMail(Dictionary<string, string> form, MapUtils data)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                IFormatProvider cultureStyle = new System.Globalization.CultureInfo("zh-TW", true);
                List<Dictionary<string, object>> adminList = action.GetAdminList(form["SRV_ID"], 151);
                DateTime applyTime = DateTime.ParseExact(data.Get("APP_TIME"), "yyyyMMddHHmmssffffff", cultureStyle);

                List<string> mailList = new List<string>();

                foreach (Dictionary<string, object> admin in adminList)
                {
                    mailList.Add(admin["MAIL"].ToString());
                }

                try
                {

                    // 新案通知
                    if (form["TRAN_ARCHIVE_MK"].Equals("Y")) // 轉入衛福部公文系統
                    {
                        string body = String.Format(MessageUtils.MAIL_NEWCASE_BODY_2, data.Get("NAME"),
                                applyTime.Year - 1911, applyTime.Month.ToString("D2"), applyTime.Day.ToString("D2"),
                                form["NAME"], data.Get("APP_ID"));

                        MailUtils.SendMail(DataUtils.StringArrayToString(mailList.ToArray(), ";"), MessageUtils.MAIL_NEWCASE_SUBJECT, body, form["SRV_ID"]);
                    }
                    else
                    {
                        string body = String.Format(MessageUtils.MAIL_NEWCASE_BODY_1, data.Get("NAME"),
                                applyTime.Year - 1911, applyTime.Month.ToString("D2"), applyTime.Day.ToString("D2"),
                                form["NAME"], data.Get("APP_ID"));

                        MailUtils.SendMail(DataUtils.StringArrayToString(mailList.ToArray(), ";"), MessageUtils.MAIL_NEWCASE_SUBJECT, body, form["SRV_ID"]);

                        string subject2 = String.Format(MessageUtils.MAIL_ApplyNotification_SUBJECT, form["UNIT_NAME"]);
                        string body2 = String.Format(MessageUtils.MAIL_ApplyNotification_BODY,
                            data.Get("NAME"), applyTime.Year - 1911, applyTime.Month.ToString("D2"), applyTime.Day.ToString("D2"),
                            form["NAME"], DataUtils.GetConfig("HOST_URL"), data.Get("APP_ID"), data.Get("APP_ID"), "新收案件", form["UNIT_NAME"]);

                        MailUtils.SendMail(data.Get("MAIL"), subject2, body2, form["SRV_ID"]);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("發送新案通知信失敗", ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// 申請人資料
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Member")]
        public ActionResult ApplyMember1(string id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                Dictionary<string, string> dict = action.GetMember(GetAccount());
                Dictionary<string, string> userData = GetUserData();
                string loginType = userData["MemberLoginType"];
                dict.Add("LOGIN_TYPE", loginType);
                ViewBag.Dict = dict;
                ViewBag.ServiceID = id;
                ViewBag.MARITAL_CD = CodeUtils.GetMaritalSelectList("");
                conn.Close();
                conn.Dispose();
            }
            return View();
        }

        /// <summary>
        /// 取得表單資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Member")]
        public ActionResult ApplyForm1(string id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                List<Dictionary<string, string>> list = null;
                Dictionary<string, List<SelectListItem>> item = new Dictionary<string, List<SelectListItem>>();
                Dictionary<string, List<Dictionary<string, string>>> dictList = new Dictionary<string, List<Dictionary<string, string>>>();
                Dictionary<string, string> userData = GetUserData();
                Dictionary<string, object> fieldData = null;

                Dictionary<string, string> form = (Dictionary<string, string>)Session["FORM_" + id];

                fieldData = action.GetFormField(form["SRV_FIELD"], form["SRV_ID"]);
                list = (List<Dictionary<string, string>>)fieldData["TABLE"];

                foreach (Dictionary<string, string> dict in list)
                {
                    dictList.Add(dict["TABLE_ID"], ((Dictionary<string, List<Dictionary<string, string>>>)fieldData["FIELD"])[dict["TABLE_ID"]]);

                    foreach (Dictionary<string, string> fields in dictList[dict["TABLE_ID"]])
                    {
                        //logger.Debug("FIELD: " + DataUtils.DictionaryToJsonString(fields));

                        if (!String.IsNullOrEmpty(fields["FORM_FIELD_CD"]))
                        {
                            if (fields["FORM_FIELD_CD"].Equals("1")) // 單選
                            {
                                //logger.Debug(fields["FIELD_NAME"] + ": " + fields["FIELD_DEF"] + " / " + item.ContainsKey(fields["FIELD_NAME"]));
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"], false));
                                //logger.Debug("TEST 1");
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("2")) // 複選
                            {
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"].Split(',')));
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("21")) // 下拉
                            {
                                //logger.Debug(fields["FIELD_NAME"] + ": " + fields["FIELD_DEF"]);
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"], true));
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("22")) // 互動式下拉
                            {
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"], true));
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("23")) // 互動式下拉子項目
                            {
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetEmptySelectList());
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("24")) // 下拉選單 (無空項目)
                            {
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"], false));
                            }

                            if (id.StartsWith("005") && !userData["MemberLoginType"].Equals("MEMBER"))
                            {
                                if (fields["FIELD_NAME"].Equals("MF_IDN"))
                                {
                                    fields["FIELD_DEF"] = userData["MemberCardId"];
                                }
                                else if (fields["FIELD_NAME"].Equals("MF_NAME"))
                                {
                                    fields["FIELD_DEF"] = userData["MemberCardName"];
                                }
                            }

                            //logger.Debug(item.ContainsKey(""));
                        }
                    }
                }

                ViewBag.List = list;
                ViewBag.Item = item;
                ViewBag.Dict = form;
                ViewBag.DictList = dictList;
                conn.Close();
                conn.Dispose();
            }

            ViewBag.ApplyId = id;

            return View();
        }

        /// <summary>
        /// 申請人資料
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Member")]
        public ActionResult PreviewMember1(FormCollection fc)
        {
            string id = Request.Form["SRV_ID"].ToString();
            //logger.Debug("SRV_ID: " + id);
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                Dictionary<string, string> dict = action.GetMember(GetAccount());

                foreach (string key in dict.Keys.ToArray())
                {
                    if (fc[key] != null)
                    {
                        dict[key] = fc[key];
                    }
                }

                if (id == "001038" && fc["MARITAL_CD"] != null)
                {
                    dict["MARITAL_CD"] = fc["MARITAL_CD"];
                }

                ViewBag.Dict = dict;
                ViewBag.ServiceID = id;
                conn.Close();
                conn.Dispose();
            }
            return View();
        }

        /// <summary>
        /// 取得表單資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Member")]
        public ActionResult PreviewForm1(FormCollection fc)
        {
            string id = Request.Form["SRV_ID"].ToString();

            DateTime dt = DateTime.Now;
            string folder1 = DataUtils.GetConfig("FOLDER_APPLY_FILE");
            //string folder = id + "\\" + dt.Year + "\\" + dt.Month.ToString("00") + "\\";
            string folder = "Temp\\" + id + "\\";

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                List<Dictionary<string, string>> list = null;
                Dictionary<string, List<SelectListItem>> item = new Dictionary<string, List<SelectListItem>>();
                Dictionary<string, List<Dictionary<string, object>>> dbList = new Dictionary<string, List<Dictionary<string, object>>>();
                Dictionary<string, List<Dictionary<string, string>>> dictList = new Dictionary<string, List<Dictionary<string, string>>>();
                Dictionary<string, string> parms = new Dictionary<string, string>();
                Dictionary<string, HttpPostedFileBase> files = GetHttpPostedFileDictionary();

                //Dictionary<string, string> data = new Dictionary<string, string>();
                MapUtils data = new MapUtils();

                string updateFiles = "";

                foreach (string key in fc.AllKeys)
                {
                    data.Put(key, fc[key]);
                }

                Dictionary<string, object> fieldData = null;
                Dictionary<string, string> form = (Dictionary<string, string>)Session["FORM_" + id];

                fieldData = action.GetFormField(form["SRV_FIELD"], id);
                list = (List<Dictionary<string, string>>)fieldData["TABLE"];

                foreach (Dictionary<string, string> dict in list)
                {
                    dictList.Add(dict["TABLE_ID"], ((Dictionary<string, List<Dictionary<string, string>>>)fieldData["FIELD"])[dict["TABLE_ID"]]);

                    if (dict["TABLE_TYPE"].StartsWith("1"))
                    {
                        if (!String.IsNullOrEmpty(dict["TABLE_DB_NAME"]))
                        {
                            int max = Int32.Parse(fc["tbody_" + dict["TABLE_ID"] + "_max"]);
                            for (int i = 0; i < max; i++)
                            {
                                if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                {
                                    //dbList.Add(dict["TABLE_DB_NAME"], new List<Dictionary<string, object>>());
                                }
                                else
                                {
                                    dbList.Add(dict["TABLE_DB_NAME"], new List<Dictionary<string, object>>());
                                }
                                dbList[dict["TABLE_DB_NAME"]].Add(new Dictionary<string, object>());
                                dbList[dict["TABLE_DB_NAME"]].Last().Add("SRL_NO", dbList[dict["TABLE_DB_NAME"]].Count());
                            }
                        }

                        //logger.Debug("tbody_" + dict["TABLE_ID"] + "_max: " + fc["tbody_" + dict["TABLE_ID"] + "_max"]);
                    }

                    foreach (Dictionary<string, string> fields in dictList[dict["TABLE_ID"]])
                    {
                        //logger.Debug("TABLE_ID: " + dict["TABLE_ID"] + " / FORM_FIELD_CD: " + fields["FORM_FIELD_CD"]);
                        if (!String.IsNullOrEmpty(fields["FORM_FIELD_CD"]))
                        {

                            if (!String.IsNullOrEmpty(fc[fields["FIELD_NAME"]]))
                            {
                                fields["FIELD_DEF"] = fc[fields["FIELD_NAME"]];
                            }

                            if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                            {
                                //logger.Debug("test 2");
                                int max = Int32.Parse(fc["tbody_" + dict["TABLE_ID"] + "_max"]);
                                //logger.Debug(dict["TABLE_ID"] + ": " + max);
                                for (int i = 0; i < max; i++)
                                {
                                    string fieldName = fields["FIELD_NAME"] + "_" + (i + 1);

                                    if (!String.IsNullOrEmpty(fc[fieldName]))
                                    {
                                        //logger.Debug(fields["FIELD_NAME"] + ": " + fc[fields["FIELD_NAME"] + "_" + (i + 1)]);
                                        dbList[dict["TABLE_DB_NAME"]][i].Add(fields["FIELD_NAME"], fc[fieldName]);
                                        //logger.Debug(fieldName + ": " + fc[fieldName]);
                                    }

                                    //logger.Debug(files.Count());
                                    //logger.Debug(fieldName + ": " + files.ContainsKey(fieldName));

                                    if (files.ContainsKey(fieldName) && files[fieldName].ContentLength > 0)
                                    {
                                        //string filename = Path.GetFileName(files[fieldName].FileName);
                                        string filename = dt.ToString("yyyyMMddHHmmss") + "_" + fieldName + files[fieldName].FileName.Substring(files[fieldName].FileName.LastIndexOf("."));

                                        //logger.Debug("filename: " + filename);

                                        if (!Directory.Exists(folder1 + folder))
                                        {
                                            Directory.CreateDirectory(folder1 + folder);
                                        }

                                        files[fieldName].SaveAs(folder1 + folder + filename);
                                        dbList[dict["TABLE_DB_NAME"]][i].Add(fields["FIELD_NAME"], folder + filename);
                                        data.Put(fieldName, folder + filename);
                                        updateFiles += fieldName + ",";
                                    }
                                }
                            }
                            else
                            {
                                if (files.ContainsKey(fields["FIELD_NAME"]) && files[fields["FIELD_NAME"]].ContentLength > 0)
                                {
                                    //logger.Debug(fields["FIELD_NAME"] + ": " + files[fields["FIELD_NAME"]].ContentLength);
                                    //string filename = Path.GetFileName(files[fieldName].FileName);
                                    string filename = dt.ToString("yyyyMMddHHmmss") + "_" + fields["FIELD_NAME"] + files[fields["FIELD_NAME"]].FileName.Substring(files[fields["FIELD_NAME"]].FileName.LastIndexOf("."));

                                    //logger.Debug("filename: " + filename);

                                    if (!Directory.Exists(folder1 + folder))
                                    {
                                        Directory.CreateDirectory(folder1 + folder);
                                    }

                                    files[fields["FIELD_NAME"]].SaveAs(folder1 + folder + filename);
                                    data.Put(fields["FIELD_NAME"], folder + filename);

                                    //logger.Debug("size: " + files[fields["FIELD_NAME"]].ContentLength);
                                    //logger.Debug("type: " + files[fields["FIELD_NAME"]].ContentType);
                                    //logger.Debug("code: " + fields["FORM_FIELD_CD"]);

                                    fields["FIELD_DEF"] = folder + filename;
                                    updateFiles += fields["FIELD_NAME"] + ",";
                                }
                            }

                            if (fields["FORM_FIELD_CD"].Equals("1")) // 單選
                            {
                                //logger.Debug(fields["FIELD_NAME"] + ": " + fields["FIELD_DEF"]);
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"], false));
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("2")) // 複選
                            {
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"].Split(',')));
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("21")) // 下拉
                            {
                                //logger.Debug("21: " + dict["TABLE_DB_NAME"] + " / " + fields["FIELD_NAME"]);
                                //logger.Debug("FIELD_DEF[1]: " + fields["FIELD_DEF"]);
                                fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", fields["FIELD_DEF"]);
                                //logger.Debug("FIELD_DEF[2]: " + fields["FIELD_DEF"]);
                                if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                {
                                    foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                    {
                                        dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", (string)dbItem[fields["FIELD_NAME"]]);
                                    }
                                }
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("22")) // 互動式下拉
                            {
                                //logger.Debug("22: " + dict["TABLE_DB_NAME"]);
                                parms.Add(fields["REL_1"], fields["FIELD_DEF"]);
                                fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", fields["FIELD_DEF"]);

                                if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                {
                                    foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                    {
                                        dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", (string)dbItem[fields["FIELD_NAME"]]);
                                    }
                                }
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("23")) // 互動式下拉子項目
                            {
                                //logger.Debug("23: " + dict["TABLE_DB_NAME"]);
                                if (parms.ContainsKey(fields["FIELD_NAME"]))
                                {
                                    fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], parms[fields["FIELD_NAME"]], fields["FIELD_DEF"]);

                                    if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                    {
                                        foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                        {
                                            dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], parms[fields["FIELD_NAME"]], (string)dbItem[fields["FIELD_NAME"]]);
                                        }
                                    }
                                }
                                else
                                {
                                    fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], null, fields["FIELD_DEF"]);

                                    if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                    {
                                        foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                        {
                                            dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], null, (string)dbItem[fields["FIELD_NAME"]]);
                                        }
                                    }
                                }
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("24")) // 下拉選單 (無空項目)
                            {
                                fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", fields["FIELD_DEF"]);
                                if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                {
                                    foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                    {
                                        dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", (string)dbItem[fields["FIELD_NAME"]]);
                                    }
                                }
                            }
                        }
                    }
                    /*
                    Dictionary<string, string> data2 = action.GetTempData(GetAccount(), dt.ToString("yyyyMMddHHmmssffffff"));
                    
                    foreach (string key in data.Keys)
                    {
                        logger.Debug(key + ": " + data[key]);
                    }
                    */
                }

                if (updateFiles.EndsWith(","))
                {
                    updateFiles = updateFiles.Substring(0, updateFiles.Length - 1);
                }

                ViewBag.Dict = action.GetFormBase(id);

                data.Put("UP_FILES", updateFiles);
                data.Put("APP_TIME", dt.ToString("yyyyMMddHHmmssffffff"));




                data.Put("PAY_A_FEE", GetPay(ViewBag.Dict, data.GetItem()).ToString());

                data.Put("CERT_SN", Convert.ToString(Session["Cert_Sn"]));

                //logger.Debug("InsertTempData: " + DataUtils.DictionaryToJsonString(data));

                action.InsertTempData(GetAccount(), dt, data.GetItem());

                ViewBag.List = list;
                ViewBag.Item = item;
                ViewBag.DictList = dictList;
                ViewBag.DBList = dbList;
                ViewBag.ApplyTime = dt.ToString("yyyyMMddHHmmssffffff");
                //logger.Debug(ViewBag.ApplyTime);

                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 計算應繳費用
        /// </summary>
        /// <param name="form"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private int GetPay(Dictionary<string, string> form, Dictionary<string, string> data)
        {
            int pay = 0;

            if (form["SRV_ID"].Equals("001008"))
            {
                return Int32.Parse(data["TOTAL_MEM"]);
            }

            if (form["PAY_POINT"].Equals("A")) // 繳費時機 A: 不需繳費
            {
                return 0;
            }

            if (form["PAY_UNIT"].Equals("A")) // 申請時繳費計算方式 A: 固定計價 / B: 以份計價
            {
                return Int32.Parse(form["APP_FEE"]);
            }
            else if (form["PAY_UNIT"].Equals("B"))
            {
                int baseNum = Int32.Parse(form["BASE_NUM"]);

                pay = Int32.Parse(form["APP_FEE"]);

                if (Int32.Parse(data["COPIES"]) - baseNum > 0)
                {
                    pay += ((Int32.Parse(data["COPIES"]) - baseNum) * Int32.Parse(form["FEE_EXTRA"]));
                }
            }
            else if (form["PAY_UNIT"].Equals("C")) // 依欄位繳費
            {
                pay = Convert.ToInt32(data[form["PAY_RULE_FIELD"]]);
            }

            return pay;
        }

        /// <summary>
        /// 案件內容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Member,Admin")]
        [HttpGet]
        public ActionResult Preview(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                MessageBoxModel msg = new MessageBoxModel("Index", "Service", "參數異常");
                TempData["MessageBoxModel"] = msg;
                return RedirectToAction("Index", "MessageBox");
            }
            ViewBag.Type = "H";
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                ViewBag.Dict = action.GetPreview(id);
                ViewBag.FileList = action.GetFileList(id);

                string serviceId = (string)((Dictionary<string, object>)ViewBag.Dict)["SRV_ID"];

                Session["FORM_" + serviceId] = action.GetFormBase(serviceId);
                conn.Close();
                conn.Dispose();
            }
            ViewBag.ServiceId = id;

            if (id.StartsWith("007"))
            {
                return View("U7_Preview");
            }

            return View();
        }

        /// <summary>
        /// 案件內容 - 申請人資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        public ActionResult PreviewMember1(string id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                Dictionary<string, object> dict = action.GetPreviewMember(id);
                ViewBag.Dict = dict;

                var dic = dict.Where(p => p.Key == "SRV_ID").ToDictionary(p => p.Key, p => p.Value);
                ViewBag.ServiceID = dic["SRV_ID"] != null ? dic["SRV_ID"].ToString() : "";
                conn.Close();
                conn.Dispose();
            }
            return View();
        }

        /// <summary>
        /// 案件內容 - 表單資料
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Member,Admin")]
        public ActionResult PreviewForm1(string id)
        {
            //logger.Debug("PreviewForm1 TEST");

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);

                Dictionary<string, object> data = action.GetPreview(id);

                //logger.Debug("id: " + id);

                string serviceId = (string)data["SRV_ID"];
                string formId = (Convert.IsDBNull(data["FORM_ID"]) || String.IsNullOrEmpty((string)data["FORM_ID"])) ? (string)data["SRV_ID"] : (string)data["FORM_ID"];

                Dictionary<string, object> def = action.GetPreviewForm((formId == "001038" ? "001035" : formId), (id == "001038" ? "001035" : id));
                List<Dictionary<string, string>> list = null;
                Dictionary<string, List<SelectListItem>> item = new Dictionary<string, List<SelectListItem>>();
                Dictionary<string, List<Dictionary<string, string>>> dictList = new Dictionary<string, List<Dictionary<string, string>>>();
                Dictionary<string, List<Dictionary<string, object>>> dbList = new Dictionary<string, List<Dictionary<string, object>>>();
                Dictionary<string, string> parms = new Dictionary<string, string>();

                Dictionary<string, object> fieldData = null;

                Dictionary<string, string> form = (Dictionary<string, string>)Session["FORM_" + serviceId];

                fieldData = action.GetFormField(form["SRV_FIELD"], serviceId);
                list = (List<Dictionary<string, string>>)fieldData["TABLE"];

                foreach (Dictionary<string, string> dict in list)
                {
                    dictList.Add(dict["TABLE_ID"], ((Dictionary<string, List<Dictionary<string, string>>>)fieldData["FIELD"])[dict["TABLE_ID"]]);

                    if (!String.IsNullOrEmpty(dict["TABLE_DB_NAME"]) && !dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                    {
                        List<Dictionary<string, object>> tmp = action.GetPreviewTableList(dict["TABLE_DB_NAME"], id);
                        dbList.Add(dict["TABLE_DB_NAME"], tmp);

                        if (dict["TABLE_TYPE"].Equals("02") && tmp.Count > 0)
                        {
                            try
                            {
                                foreach (KeyValuePair<string, object> tmp2 in tmp[0])
                                {
                                    if (!def.ContainsKey(tmp2.Key))
                                    {
                                        def.Add(tmp2.Key, tmp2.Value);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn(e.Message, e);
                            }
                        }
                    }

                    //logger.Debug("TABLE_DB_NAME: " + dict["TABLE_DB_NAME"]);

                    foreach (Dictionary<string, string> fields in dictList[dict["TABLE_ID"]])
                    {
                        if (!String.IsNullOrEmpty(fields["FORM_FIELD_CD"]))
                        {
                            //logger.Debug(fields["FIELD_NAME"] + ": ");
                            if (def.ContainsKey(fields["FIELD_NAME"]))
                            {
                                if (fields["FORM_FIELD_CD"].Equals("11") && !String.IsNullOrEmpty(def[fields["FIELD_NAME"]].ToString()))
                                {
                                    fields["FIELD_DEF"] = ((DateTime)def[fields["FIELD_NAME"]]).ToString("yyyy/MM/dd");
                                }
                                else
                                {
                                    fields["FIELD_DEF"] = def[fields["FIELD_NAME"]].ToString();
                                    //logger.Debug(fields["FIELD_NAME"] + ": " + def[fields["FIELD_NAME"]].ToString());
                                }
                            }

                            if (fields["FORM_FIELD_CD"].Equals("1")) // 單選
                            {
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"], false));
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("2")) // 複選
                            {
                                item.Add(fields["FIELD_NAME"], CodeUtils.GetCodeSelectList(conn, fields["CODE_CD"], "", fields["FIELD_DEF"].Split(',')));
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("21")) // 下拉
                            {
                                //logger.Debug("21: " + dict["TABLE_DB_NAME"]);
                                //logger.Debug("FIELD_DEF[1]: " + fields["FIELD_DEF"]);
                                fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", fields["FIELD_DEF"]);
                                //logger.Debug("FIELD_DEF[2]: " + fields["FIELD_DEF"]);)
                                if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                {
                                    foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                    {
                                        if (!Convert.IsDBNull(dbItem[fields["FIELD_NAME"]]))
                                        {
                                            dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", (string)dbItem[fields["FIELD_NAME"]]);
                                        }
                                    }
                                }
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("22")) // 互動式下拉
                            {
                                //logger.Debug("22: " + dict["TABLE_DB_NAME"]);
                                parms.Add(fields["REL_1"], fields["FIELD_DEF"]);
                                fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", fields["FIELD_DEF"]);

                                if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                {
                                    foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                    {
                                        //logger.Debug("fields[\"FIELD_NAME\"]: " + fields.ContainsKey("FIELD_NAME"));
                                        //logger.Debug("fields[\"CODE_CD\"]: " + fields.ContainsKey("CODE_CD"));

                                        //logger.Debug("fields[\"FIELD_NAME\"]: " + fields["FIELD_NAME"]);
                                        //logger.Debug("fields[\"CODE_CD\"]: " + fields["CODE_CD"]);

                                        if (dbItem.ContainsKey(fields["FIELD_NAME"]))
                                        {
                                            if (!Convert.IsDBNull(dbItem[fields["FIELD_NAME"]]))
                                            {
                                                dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", (string)dbItem[fields["FIELD_NAME"]]);
                                            }
                                        }
                                        else
                                        {
                                            //dbItem.Add(fields["FIELD_NAME"],  CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", (string)dbItem[fields["FIELD_NAME"]]));
                                        }

                                    }
                                }
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("23")) // 互動式下拉子項目
                            {
                                //logger.Debug("23: " + dict["TABLE_DB_NAME"]);
                                if (parms.ContainsKey(fields["FIELD_NAME"]))
                                {
                                    fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], parms[fields["FIELD_NAME"]], fields["FIELD_DEF"]);

                                    if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                    {
                                        foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                        {
                                            if (dbItem.ContainsKey(fields["FIELD_NAME"]) && !Convert.IsDBNull(dbItem[fields["FIELD_NAME"]]))
                                            {
                                                dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], parms[fields["FIELD_NAME"]], (string)dbItem[fields["FIELD_NAME"]]);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], null, fields["FIELD_DEF"]);

                                    if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                    {
                                        foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                        {
                                            if (dbItem.ContainsKey(fields["FIELD_NAME"]))
                                            {
                                                dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], null, (string)dbItem[fields["FIELD_NAME"]]);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (fields["FORM_FIELD_CD"].Equals("24")) // 下拉選單 (無空項目)
                            {
                                fields["FIELD_DEF"] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", fields["FIELD_DEF"]);
                                if (dbList.ContainsKey(dict["TABLE_DB_NAME"]))
                                {
                                    foreach (Dictionary<string, object> dbItem in dbList[dict["TABLE_DB_NAME"]])
                                    {
                                        if (dbItem.ContainsKey(fields["FIELD_NAME"]))
                                        {
                                            dbItem[fields["FIELD_NAME"]] = CodeUtils.GetCodeDesc(conn, fields["CODE_CD"], "", (string)dbItem[fields["FIELD_NAME"]]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ViewBag.List = list;
                ViewBag.Item = item;
                ViewBag.Dict = data;
                ViewBag.DictList = dictList;
                ViewBag.DBList = dbList;

                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        [Authorize(Roles = "Member,Admin")]
        public ActionResult PayPDF(string id)
        {
            Dictionary<string, object> data = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                data = action.GetApplyData(id);
                conn.Close();
                conn.Dispose();
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();

            dict.Add("VirtualAccount", (string)data["SESSION_KEY"]);
            dict.Add("Fee", (Int32.Parse(data["PAY_A_FEE"].ToString()) + Int32.Parse(DataUtils.GetConfig("PAY_STORE_FEE"))).ToString());
            dict.Add("Name", data["NAME"].ToString());
            dict.Add("ServiceName", data["SRV_NAME"].ToString());
            dict.Add("ApplyId", data["APP_ID"].ToString());
            dict.Add("PayDeadline", ((DateTime)data["PAY_DEADLINE"]).ToString("yyyy/MM/dd"));

            DateTime dt = (DateTime)data["PAY_DEADLINE"];

            string[] b1 = PayUtils.GetStore(dt, dict["VirtualAccount"], Int32.Parse(dict["Fee"]));
            string[] b2 = PayUtils.GetPost(dt, dict["VirtualAccount"], Int32.Parse(dict["Fee"]));

            dict.Add("Store1", b1[0]);
            dict.Add("Store2", b1[1]);
            dict.Add("Store3", b1[2]);

            dict.Add("Post1", b2[0]);
            dict.Add("Post2", b2[1]);
            dict.Add("Post3", b2[2]);

            byte[] b = PayUtils.GetPDF(dict);
            //logger.Debug("ms: " + ms.Length);

            return File(b, "application/pdf", "Pay" + id + ".pdf");
        }

        /// <summary>
        /// 檔案下載
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult File(string path)
        {

            string folder1 = DataUtils.GetConfig("FOLDER_APPLY_FILE");
            string s_path = DataUtils.FromBase64String(path);
            if (s_path == null) { return HttpNotFound(); }

            string filePath = folder1 + s_path;
            string fileName = Path.GetFileName(filePath);

            Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(iStream, "application/unknown", fileName);
        }

        public ActionResult Example(string id)
        {
            return View("Example" + id);
        }

        public ActionResult ErrorMessage()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            return View();
        }

        public ActionResult FormJS(string id)
        {

            Dictionary<string, string> form = null;

            if (Session["FORM_" + id] == null)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    FormAction action = new FormAction(conn);
                    form = action.GetFormBase(id);
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                form = (Dictionary<string, string>)Session["FORM_" + id];
            }

            return File(System.Text.Encoding.UTF8.GetBytes(form["SRV_SCRIPT"]), "application/javascript");
        }

        public ActionResult PreviewJS(string id)
        {

            Dictionary<string, string> form = null;

            if (Session["FORM_" + id] == null)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    FormAction action = new FormAction(conn);
                    form = action.GetFormBase(id);
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                form = (Dictionary<string, string>)Session["FORM_" + id];
            }

            return File(System.Text.Encoding.UTF8.GetBytes(form["PRE_SCRIPT"]), "application/javascript");
        }



        public ActionResult PaySuccess()
        {
            try
            {
                Dictionary<string, object> item = null;

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    FormAction action = new FormAction(conn, tran);
                    item = action.GetPayDataBySessionKey(Request["SessionTransactionKey"]);
                    conn.Close();
                    conn.Dispose();
                }

                logger.Debug("SessionTransactionKey: " + Request["SessionTransactionKey"]);

                if (item != null)
                {
                    logger.Debug("PAY_COUNT: " + item["PAY_COUNT"]);

                    if (Convert.ToInt32(item["PAY_COUNT"]) == 1)
                    {
                        Success(item);

                        if (item["SRV_ID"].ToString().StartsWith("007"))
                        {
                            return View("U7_Success");
                        }

                        return View("Success");
                    }
                    else if (Convert.ToInt32(item["PAY_COUNT"]) > 1)
                    {
                        CardPaySuccess(item);

                        if (item["SRV_ID"].ToString().StartsWith("007"))
                        {
                            return View("U7_CardPaySuccess");
                        }

                        return View("CardPaySuccess");
                    }
                }

                return View("Error");

                /*
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    FormAction action = new FormAction(conn, tran);
                    item = action.GetPayDataBySessionKey(Request["SessionTransactionKey"]);
                }
                //logger.Debug("PaySuccess()_3");
                if (item == null)
                {
                    //logger.Debug("PaySuccess()_4");
                    Success();
                    return View("Success");
                }
                else
                {
                    //logger.Debug("PaySuccess()_5");
                    CardPaySuccess(item);
                    return View("CardPaySuccess");
                }
                */
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                //throw e;
                return View("Error3");
            }
        }

        public ActionResult PayFailure()
        {
            return PaySuccess();
        }

        public ActionResult PaySuccess2()
        {
            Success();
            return PaySuccess();
        }

        private void Success(Dictionary<string, object> item)
        {
            MapUtils data = new MapUtils();
            data.Put("PAY_SESSION_KEY", item["SESSION_KEY"].ToString());
            data.Put("APP_ID", item["APP_ID"].ToString());
            data.Put("PAY_A_FEE", item["PAY_A_FEE"].ToString());
            data.Put("PAY_A_FEEBK", item["PAY_A_FEEBK"].ToString());
            data.Put("PAY_METHOD", "C");
            data.Put("SRV_NAME", item["SRV_NAME"].ToString());
            data.Put("APP_TIME", ((DateTime)item["APP_TIME"]).ToString("yyyyMMddHHmmssffffff"));
            data.Put("SRV_ID", data.Get("APP_ID").Substring(8, 6));

            logger.Debug("Success: " + data.Get("SRV_ID"));

            Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(item["PAY_ACCOUNT"]));

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                FormAction action = new FormAction(conn, tran);
                ConfirmResponse res = CardUtils.GetAccountingService(data.Get("PAY_SESSION_KEY"), pay);
                data.Put("PAY_TRANS_RET", res.ResultInfo);
                Dictionary<string, string> form = action.GetFormBase(data.Get("SRV_ID"));

                ConfirmResult[] result = res.ConfirmResults;
                if (result.Length > 0)
                {
                    data.Put("PAY_AUTH_DATE", result[0].AuthDate);
                    data.Put("PAY_AUTH_NO", result[0].ApproveNo);
                    data.Put("PAY_SETTLE_DATE", result[0].SettleDate);
                    data.Put("PAY_HOST_TIME", result[0].HostTime);
                    data.Put("PAY_INFO_NO", result[0].InfoNO);
                    data.Put("PAY_OTHER", result[0].Others);

                    data.Put("PAY_TRANS_RET", result[0].TransactionResult);
                    data.Put("PAY_TRANS_MSG", action.GetPayCodeDesc(data.Get("PAY_TRANS_RET")));
                }

                data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("0000")) ? data.Get("PAY_A_FEE") : "0");
                data.Put("ACC_NO", GetAccount());
                data.Put("UPD_ACC", GetAccount());

                if (action.UpdateApplyPay(data.GetItem()))
                {
                    tran.Commit();

                    SuccessSendMail(form, data);
                }
                else
                {
                    tran.Rollback();
                }

                data.Put("APP_FEE", (Int32.Parse(data.Get("PAY_A_FEE")) + Int32.Parse(data.Get("PAY_A_FEEBK"))).ToString());
                data.Put("NAME", item["NAME"].ToString());
                data.Put("COMP_DESC", item["COMP_DESC"].ToString());
                data.Put("APPLY_DATE", ((DateTime)item["APP_TIME"]).ToString("yyyy/MM/dd"));

                ViewBag.Dict = form;
                ViewBag.Data = data.GetItem();
                conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 舊 信用卡繳費完成，不使用
        /// </summary>
        private void Success()
        {

            string sessionTransactionKey = Request["SessionTransactionKey"];    // 交易電子案號
            string others = Request["Others"];                                  // 備註說明
            string errorCode = Request["ErrorCode"];                            // 錯誤代碼
            string errorMsg = Request["ErrorMsg"];                              // 錯誤訊息

            logger.Debug("sessionTransactionKey: " + sessionTransactionKey);
            logger.Debug("others: " + others);
            logger.Debug("errorCode: " + errorCode);
            logger.Debug("errorMsg: " + errorMsg);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                FormAction action = new FormAction(conn, tran);

                //Dictionary<string, string> data = action.GetTempData(GetAccount(), sessionTransactionKey);
                MapUtils data = new MapUtils(action.GetTempData(GetAccount(), sessionTransactionKey));

                Dictionary<string, string> form = action.GetFormBase(data.Get("SRV_ID"));
                Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(form["PAY_ACCOUNT"]));

                ConfirmResponse res = CardUtils.GetAccountingService(sessionTransactionKey, pay);

                logger.Debug("Success()_1 / isNull: " + (res == null));

                data.Put("PAY_TRANS_RET", res.ResultInfo);

                //logger.Debug("ResultInfo: " + res.ResultInfo);
                logger.Debug("Success()_2");

                ConfirmResult[] result = res.ConfirmResults;
                if (result != null && result.Length > 0)
                {
                    logger.Debug("Success()_3");
                    data.Put("PAY_AUTH_DATE", result[0].AuthDate);
                    logger.Debug("Success()_4");
                    data.Put("PAY_AUTH_NO", result[0].ApproveNo);
                    data.Put("PAY_SETTLE_DATE", result[0].SettleDate);
                    logger.Debug("Success()_5");
                    data.Put("PAY_HOST_TIME", result[0].HostTime);
                    data.Put("PAY_INFO_NO", result[0].InfoNO);
                    data.Put("PAY_OTHER", result[0].Others);
                    logger.Debug("Success()_5");
                    data.Put("PAY_TRANS_RET", result[0].TransactionResult);
                    logger.Debug("Success()_7");
                    data.Put("PAY_TRANS_MSG", action.GetPayCodeDesc(data.Get("PAY_TRANS_RET")));
                    logger.Debug("Success()_8");
                }

                logger.Debug("Success()_9");
                data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("0000")) ? data.Get("PAY_A_FEE") : "0");

                data.Put("APP_ID", action.GetApplySerial(data.Get("SRV_ID"), data.Get("APP_TIME").Substring(0, 8)));
                data.Put("ACC_NO", GetAccount());
                data.Put("UPD_ACC", GetAccount());

                //logger.Debug("SRV_ID: " + data["SRV_ID"] + " / APP_ID: " + data["APP_ID"]);

                moveFolder(data.GetItem());

                logger.Debug("Success()_10");
                if (action.InsertApply(data.GetItem(), form))
                {
                    IFormatProvider cultureStyle = new System.Globalization.CultureInfo("zh-TW", true);
                    List<Dictionary<string, object>> adminList = action.GetAdminList(form["SRV_ID"], 151);
                    DateTime applyTime = DateTime.ParseExact(data.Get("APP_TIME"), "yyyyMMddHHmmssffffff", cultureStyle);

                    List<string> mailList = new List<string>();

                    foreach (Dictionary<string, object> admin in adminList)
                    {
                        mailList.Add(admin["MAIL"].ToString());
                    }
                    logger.Debug("Success()_11");

                    // 新案通知
                    if (form["TRAN_ARCHIVE_MK"].Equals("Y")) // 轉入衛福部公文系統
                    {
                        string body = String.Format(MessageUtils.MAIL_NEWCASE_BODY_2, data.Get("NAME"),
                                applyTime.Year - 1911, applyTime.Month.ToString("D2"), applyTime.Day.ToString("D2"),
                                form["NAME"], data.Get("APP_ID"));

                        MailUtils.SendMail(DataUtils.StringArrayToString(mailList.ToArray(), ";"), MessageUtils.MAIL_NEWCASE_SUBJECT, body, form["SRV_ID"]);
                    }
                    else
                    {
                        string body = String.Format(MessageUtils.MAIL_NEWCASE_BODY_1, data.Get("NAME"),
                                applyTime.Year - 1911, applyTime.Month.ToString("D2"), applyTime.Day.ToString("D2"),
                                form["NAME"], data.Get("APP_ID"));

                        MailUtils.SendMail(DataUtils.StringArrayToString(mailList.ToArray(), ";"), MessageUtils.MAIL_NEWCASE_SUBJECT, body, form["SRV_ID"]);
                    }

                    action.DeleteTempData(GetAccount(), data.Get("APP_TIME"));
                    logger.Debug("Success()_12");
                    tran.Commit();
                }
                else
                {
                    tran.Rollback();
                }

                ViewBag.Dict = form;
                ViewBag.Data = data.GetItem();
                conn.Close();
                conn.Dispose();
            }
        }



        /// <summary>
        /// 搬移暫存檔
        /// </summary>
        /// <param name="data"></param>
        private void moveFolder(Dictionary<string, string> data)
        {
            string[] updateFiles = data["UP_FILES"].Split(',');
            string folder1 = DataUtils.GetConfig("FOLDER_APPLY_FILE");
            string folder2 = data["SRV_ID"] + "\\" + DateTime.Now.Year + "\\" + DateTime.Now.Month.ToString("D2") + "\\" + data["APP_ID"] + "\\";
            string file1, file2;

            for (int i = 0; i < updateFiles.Length; i++)
            {
                if (data.ContainsKey(updateFiles[i]))
                {
                    file1 = folder1 + data[updateFiles[i]];
                    file2 = folder1 + folder2 + Path.GetFileName(file1);

                    try
                    {
                        if (!Directory.Exists(folder1 + folder2))
                        {
                            Directory.CreateDirectory(folder1 + folder2);
                        }

                        System.IO.File.Move(file1, file2);
                        System.IO.File.Delete(file1);
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.Message, e);
                    }

                    data[updateFiles[i]] = folder2 + Path.GetFileName(file1);
                }
            }
        }

        private string GetLoginType(string loginType)
        {
            switch (loginType)
            {
                case "MEMBER":
                    return "會員登入";
                case "MOICA":
                    return "自然人憑證";
                case "MOEACA":
                    return "工商憑證";
                case "HCA0":
                    return "醫事憑證（機構）";
                case "HCA1":
                    return "醫事憑證（人員）";
            }
            return loginType;
        }

        /// <summary>
        /// 信用卡重新繳費
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CardPay(string id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                FormAction action = new FormAction(conn);
                ViewBag.Item = action.GetPayData(id, GetAccount());
                conn.Close();
                conn.Dispose();
            }
            if (ViewBag.Item != null && ViewBag.Item["PAY_MK"].Equals("Y"))
            {
                MessageBoxModel msg = new MessageBoxModel("此案件已完成繳費", -1);
                TempData["MessageBoxModel"] = msg;
                return RedirectToAction("Index", "MessageBox");
            }

            if (ViewBag.Item["SRV_ID"].ToString().StartsWith("007"))
            {
                return View("U7_CardPay");
            }

            return View();
        }

        [HttpPost]
        public ActionResult CardPay(FormCollection fc)
        {

            //Dictionary<string, string> data = new Dictionary<string, string>();

            MapUtils data = new MapUtils();
            data.Put("APP_ID", fc["ApplyId"]);  // 申請編號
            data.Put("PAY_CLIENT_IP", GetClientIP());
            data.Put("PAY_CARD_IDN", fc["CardIDN"].ToUpper());
            //data.Put("PAY_OID", DataUtils.GetConfig("PAY_CARD_OID"));
            //data.Put("PAY_SID", DataUtils.GetConfig("PAY_CARD_SID"));

            data.Put("PAY_METHOD", "C"); // 信用卡付費
            data.Put("UPD_ACC", GetAccount());

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                FormAction action = new FormAction(conn, tran);
                Dictionary<string, object> item = action.GetPayData(data.Get("APP_ID"), GetAccount());
                data.Put("PAY_A_FEE", item["PAY_A_FEE"].ToString());
                data.Put("PAY_A_FEEBK", item["PAY_A_FEEBK"].ToString());
                data.Put("APP_FEE", item["PAY_FEE"].ToString());
                data.Put("PAY_ID", action.GetPaySerial(item["SRV_ID"].ToString(), DateTime.Now.ToString("yyyyMMdd")));

                Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(item["PAY_ACCOUNT"]));
                data.Put("PAY_OID", pay["OID"]);
                data.Put("PAY_SID", pay["SID"]);

                SessionKeyResponse res = CardUtils.GetTransactionKey(data.GetItem(), pay);

                if (res != null)
                {
                    ViewBag.ErrorCode = res.ResultInfo;
                    ViewBag.ErrorMessage = action.GetPayCodeDesc(res.ResultInfo);
                    if (res.ResultInfo.Equals("0000"))
                    {
                        string sessionKey = res.SessionTransactionKey;
                        data.Put("PAY_SESSION_KEY", sessionKey);
                        logger.Debug("CardPay_SessionTransactionKey: " + sessionKey);
                        if (action.InsertApplyPay(data.GetItem()))
                        {
                            tran.Commit();
                        }
                        ViewBag.SessionTransactionKey = sessionKey;
                    }
                    else
                    {
                        ViewBag.tempMessage = "取得預約交易代碼失敗(" + res.ResultInfo + ")";
                    }
                }
                else
                {
                    ViewBag.ErrorCode = "-1";
                    ViewBag.ErrorMessage = "連接失敗";
                    ViewBag.tempMessage = "取得預約交易代碼失敗(-1)";
                }
                conn.Close();
                conn.Dispose();
            }
            return View("TransactionKey");
        }

        /// <summary>
        /// 信用卡重新付費完成
        /// Add: 2014-03-04 Jay
        /// </summary>
        /// <param name="item"></param>
        public void CardPaySuccess(Dictionary<string, object> item)
        {
            MapUtils data = new MapUtils();
            data.Put("PAY_SESSION_KEY", item["SESSION_KEY"].ToString());
            data.Put("APP_ID", item["APP_ID"].ToString());
            data.Put("PAY_A_FEE", item["PAY_A_FEE"].ToString());
            data.Put("PAY_METHOD", "C");
            data.Put("SRV_NAME", item["SRV_NAME"].ToString());
            data.Put("APP_TIME", ((DateTime)item["APP_TIME"]).ToString("yyyy/MM/dd"));

            Dictionary<string, string> pay = DataUtils.GetPayAccount(Convert.ToInt32(item["PAY_ACCOUNT"]));

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                FormAction action = new FormAction(conn, tran);
                ConfirmResponse res = CardUtils.GetAccountingService(data.Get("PAY_SESSION_KEY"), pay);
                data.Put("PAY_TRANS_RET", res.ResultInfo);

                ConfirmResult[] result = res.ConfirmResults;
                if (result.Length > 0)
                {
                    data.Put("PAY_AUTH_DATE", result[0].AuthDate);
                    data.Put("PAY_AUTH_NO", result[0].ApproveNo);
                    data.Put("PAY_SETTLE_DATE", result[0].SettleDate);
                    data.Put("PAY_HOST_TIME", result[0].HostTime);
                    data.Put("PAY_INFO_NO", result[0].InfoNO);
                    data.Put("PAY_OTHER", result[0].Others);

                    data.Put("PAY_TRANS_RET", result[0].TransactionResult);
                    data.Put("PAY_TRANS_MSG", action.GetPayCodeDesc(data.Get("PAY_TRANS_RET")));
                }

                data.Put("PAY_A_PAID", (data.Get("PAY_TRANS_RET").Equals("0000")) ? data.Get("PAY_A_FEE") : "0");
                data.Put("ACC_NO", GetAccount());
                data.Put("UPD_ACC", GetAccount());

                if (action.UpdateApplyPay(data.GetItem()))
                {
                    tran.Commit();
                }
                else
                {
                    tran.Rollback();
                }
                conn.Close();
                conn.Dispose();
            }

            ViewBag.Item = data.GetItem();
        }

        [Authorize(Roles = "Member")]
        public ActionResult Apply005001(String id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form005001Action action = new Form005001Action(conn);
                b = action.GetApplyWord(id);
                conn.Close();
                conn.Dispose();
            }

            byte[] tmpPDF = WordConverter.Convert(b, ES.Extensions.WordConverter.WordFileFormat.WordDocx, id);
            return File(tmpPDF, "application/pdf", "Apply" + id + ".pdf");
        }

        [Authorize(Roles = "Member")]
        public ActionResult Apply005002(String id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form005002Action action = new Form005002Action(conn);
                b = action.GetApplyWord(id);
                conn.Close();
                conn.Dispose();
            }

            byte[] tmpPDF = WordConverter.Convert(b, ES.Extensions.WordConverter.WordFileFormat.WordDocx, id);
            return File(tmpPDF, "application/pdf", "Apply" + id + ".pdf");
        }

        [Authorize(Roles = "Member")]
        public ActionResult Apply005003(String id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form005003Action action = new Form005003Action(conn);
                b = action.GetApplyWord(id);
                conn.Close();
                conn.Dispose();
            }

            byte[] tmpPDF = WordConverter.Convert(b, ES.Extensions.WordConverter.WordFileFormat.WordDocx, id);
            return File(tmpPDF, "application/pdf", "Apply" + id + ".pdf");
        }

        [Authorize(Roles = "Member")]
        public ActionResult Apply005004(String id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form005004Action action = new Form005004Action(conn);
                b = action.GetApplyWord(id);
                conn.Close();
                conn.Dispose();
            }

            byte[] tmpPDF = WordConverter.Convert(b, ES.Extensions.WordConverter.WordFileFormat.WordDocx, id);
            return File(tmpPDF, "application/pdf", "Apply" + id + ".pdf");
        }

        [Authorize(Roles = "Member")]
        public ActionResult Apply005005(String id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form005005Action action = new Form005005Action(conn);
                b = action.GetApplyWord(id);
                conn.Close();
                conn.Dispose();
            }

            byte[] tmpPDF = WordConverter.Convert(b, ES.Extensions.WordConverter.WordFileFormat.WordDocx, id);
            return File(tmpPDF, "application/pdf", "Apply" + id + ".pdf");
        }

        /// <summary>
        /// 用來接收中國信託線上刷卡成功後重導回來的 handle action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CreditSuccessRecv(string URLResEnc)
        {
            // id 值為申請中國信託信用卡繳款服務時所填的(自行編號)代碼, 對應系統的 form id 值
            // 007002: 信用卡線上捐款

            // 將中國信託 POST 回來的URLResEnc參數解密取出各欄位值
            return RedirectToAction("CreditSuccessRecv", "ApplyDonate", URLResEnc);
        }
    }
}
