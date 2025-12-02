using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Utils;
using ES.Models;
using ES.Action;
using log4net;

namespace ES.Controllers
{
    /// <summary>
    /// 非線上案件滿意度調查
    /// </summary>
    public class QuestNonWebController : BaseNoMemberController
    {
        [HttpGet]
        public ActionResult Index()
        {
            QuestNonWebModel model = new QuestNonWebModel();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                model.UnitCodeSelList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", null, false);
                model.ServiceIdSelList = CodeUtils.GetServiceLst3(conn, model.UnitCode, model.ServiceId);
                //ViewBag.UnitCodeSelList = model.UnitCodeSelList;
                //ViewBag.ServiceIdSelList = model.ServiceIdSelList;
                //ViewBag.UnitList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", null, false);
                //ViewBag.ServiceList = CodeUtils.GetCodeSelectList(conn, "SURVEY_SRV", "", null, false);
                //ViewBag.ServiceList = CodeUtils.GetServiceLst1("1", null);
                //ViewBag.ServiceList = CodeUtils.GetServiceLst1(conn, "1", null);
                conn.Close();
                conn.Dispose();
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(QuestNonWebModel model)
        {
            //資安檢核
            model.ValidateCode = model.ValidateCode ?? "";
            model.UnitCode = model.UnitCode ?? "";
            model.ServiceId = model.ServiceId ?? "";
            model.ApplyId = model.ApplyId ?? "";

            if (model.ValidateCode.Contains("alert")) { model.ValidateCode = ""; }
            if (model.UnitCode.Contains("alert")) { model.UnitCode = ""; }
            if (model.ServiceId.Contains("alert")) { model.ServiceId = ""; }
            if (model.ApplyId.Contains("alert")) { model.ApplyId = ""; }
            int iVid = 0;
            if (model.ValidateCode.Length > 4) { model.ValidateCode = ""; }
            else if (!int.TryParse(model.ValidateCode, out iVid)) { model.ValidateCode = ""; }
            int iUid = 0;
            if (model.UnitCode.Length > 2) { model.UnitCode = ""; }
            else if (!int.TryParse(model.UnitCode, out iUid)) { model.UnitCode = ""; }
            int iSid = 0;
            if (model.ServiceId.Length > 6) { model.ServiceId = ""; }
            else if (!int.TryParse(model.ServiceId, out iSid)) { model.ServiceId = ""; }
            Int64 iAid = 0;
            if (model.ApplyId.Length > 20) { model.ApplyId = ""; }
            else if (!Int64.TryParse(model.ApplyId, out iAid)) { model.ApplyId = ""; }

            SessionModel sm = SessionModel.Get();
            bool flag_ValidateCode = false;
            string s_err2 = "";
            if (!flag_ValidateCode && (sm.LoginValidateCode == null || string.IsNullOrEmpty(sm.LoginValidateCode)))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸出有誤";
            }
            if (!flag_ValidateCode && (model.ValidateCode == null || string.IsNullOrEmpty(model.ValidateCode)))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸入有誤";
            }

            if (!flag_ValidateCode && (!model.ValidateCode.Equals(sm.LoginValidateCode)))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸入錯誤";
            }
            if (flag_ValidateCode && s_err2 != "")
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    model.UnitCodeSelList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", model.UnitCode, false);
                    model.ServiceIdSelList = CodeUtils.GetServiceLst3(conn, model.UnitCode, model.ServiceId);
                    //ViewBag.UnitList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", null, false);
                    //ViewBag.ServiceList = CodeUtils.GetCodeSelectList(conn, "SURVEY_SRV", "", null, false);
                    conn.Close();
                    conn.Dispose();
                }
                sm.LastErrorMessage = s_err2;
                return View(model);
            }
            //清空驗證碼
            sm.LoginValidateCode = "";

            bool flag_error = false;
            //logger.Debug("##ActionResult return Index()");
            if (model == null) { return Index(); }

            //string s_log1 = "";
            //s_log1 += string.Format("\n model.LST_ID:{0}", model.LST_ID ?? "[null]");
            //s_log1 += string.Format("\n model.UnitCode:{0}", model.UnitCode ?? "[null]");//UNID_CD-1.承辦機關
            //s_log1 += string.Format("\n model.ServiceId:{0}", model.ServiceId ?? "[null]");//SRV_ID-2.案件名稱
            //s_log1 += string.Format("\n model.ApplyId:{0}", model.ApplyId ?? "[null]");//案號-請輸入案號
            //logger.Debug(s_log1);

            string tMsg = null;

            if (!ModelState.IsValid)
            {
                //ViewBag.tempMessage = "欄位驗證錯誤";
                flag_error = true;
                sm.LastErrorMessage = "欄位驗證錯誤!";
                //logger.Debug("##ActionResult Index(QuestNonWebModel 欄位驗證錯誤!");
                //return Index();
                //return View(model);
            }


            if (!flag_error)
            {
                tMsg = CheckApplyId(model);
                if (tMsg != null)
                {
                    flag_error = true;
                    sm.LastErrorMessage = tMsg;
                    //return View(model);
                }
            }

            //if (!flag_error && string.IsNullOrEmpty(model.UnitCode))
            //{
            //    flag_error = true;
            //    sm.LastErrorMessage = "請檢查1.承辦機關!!";
            //}

            ////model.ServiceId = model.ServiceId ?? "";
            //if (!flag_error && string.IsNullOrEmpty(model.ServiceId))
            //{
            //    flag_error = true;
            //    sm.LastErrorMessage = "請檢查2.案件名稱!!";
            //}

            if (!flag_error && string.IsNullOrEmpty(model.ApplyId))
            {
                flag_error = true;
                sm.LastErrorMessage = "請檢查3.申請案號!!";
            }

            if (!flag_error)
            {
                //string tMsg = null;
                List<int> score = new List<int>();
                score.Add(model.Q1);
                score.Add(model.Q2);
                score.Add(model.Q3);
                score.Add(model.Q4);
                score.Add(model.Q5);
                if (score.Count(n => n == 0) > 0)
                {
                    flag_error = true;
                    sm.LastErrorMessage = "請檢查案件處理情形-滿意度輸入項目!!";
                    //return View(model);
                }
            }


            if (flag_error)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    model.UnitCodeSelList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", model.UnitCode, false);
                    model.ServiceIdSelList = CodeUtils.GetServiceLst3(conn, model.UnitCode, model.ServiceId);
                    //ViewBag.UnitList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", null, false);
                    //ViewBag.ServiceList = CodeUtils.GetCodeSelectList(conn, "SURVEY_SRV", "", null, false);
                    conn.Close();
                    conn.Dispose();
                }
                return View(model);
            }


            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                QuestNonWebAction action = new QuestNonWebAction(conn);
                bool flag_double = action.CheckApplyIdExists(model);
                if (flag_double)
                {
                    tMsg = "您己填寫滿意度調查表，同一案件僅能填寫一次";
                    sm.LastErrorMessage = tMsg;
                }
                else
                {
                    SqlTransaction tran = conn.BeginTransaction();
                    action = new QuestNonWebAction(conn, tran);
                    model.Satisfied = GetSatisfied(model);
                    model.UpdateAccount = GetAccount();
                    model.ServiceId = model.ApplyId.Substring(4, 6);
                    model.UnitCode = GetUnitCD(model.ServiceId);
                    bool ret = action.Insert(model);
                    if (ret)
                    {
                        tran.Commit();
                        sm.LastErrorMessage = "感謝您的填寫!";
                    }
                    else
                    {
                        tran.Rollback();
                        tMsg = "存檔失敗";
                        sm.LastErrorMessage = tMsg;
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return RedirectToAction("Index", "QuestNonWeb");
        }

        private void SendMail(QuestNonWebModel model)
        {
            List<int> score = new List<int>();
            score.Add(model.Q1);
            score.Add(model.Q2);
            score.Add(model.Q3);
            score.Add(model.Q4);
            score.Add(model.Q5);

            //logger.Debug("count: " + score.Count(n => n == 1));

            if (score.Count(n => n == 1) > 0 || !String.IsNullOrEmpty(model.Recommend.Trim()))
            {
                try
                {
                    string sendTo = DataUtils.GetConfig("SURVEY_MAIL_1");
                    string subject = MessageUtils.MAIL_SURVEY_SUBJECT;

                    string userMail = "";
                    if (Session["Member"] != null)
                    {
                        userMail = ((MemberModel)Session["Member"]).Mail;
                    }

                    string body = String.Format(MessageUtils.MAIL_SURVEY_BODY, userMail, model.ApplyId,
                        GetSatisfied(model.Q1), GetSatisfied(model.Q2), GetSatisfied(model.Q3), GetSatisfied(model.Q4), GetSatisfied(model.Q5),
                        String.IsNullOrEmpty(model.Recommend) ? "無" : model.Recommend.Replace("\n", "<br/>"));

                    MailUtils.SendMail(sendTo, subject, body, model.ServiceId);
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                }
            }
        }

        private string GetSatisfied(int satisfied)
        {
            switch (satisfied)
            {
                case 1:
                    return "非常不滿意";
                case 2:
                    return "不太滿意";
                case 3:
                    return "無意見";
                case 4:
                    return "還算滿意";
                case 5:
                    return "非常滿意";
            }
            return "";
        }

        /// <summary>
        /// 取得整體滿意度
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int GetSatisfied(QuestNonWebModel model)
        {
            List<int> score = new List<int>();
            score.Add(model.Q1);
            score.Add(model.Q2);
            score.Add(model.Q3);
            score.Add(model.Q4);
            score.Add(model.Q5);

            if (score.Count(n => n == 1) == 5)
            {
                return 1; // 非常不滿意
            }
            if (score.Count(n => n == 3) == 5)
            {
                return 3; // 無意見
            }
            if (score.Count(n => n == 5) == 5)
            {
                return 5; // 非常滿意
            }

            if (score.Contains(2) || score.Contains(1))
            {
                return 2; // 不滿意
            }

            if (score.Contains(4))
            {
                return 4; // 滿意
            }

            if (score.Count(n => n == 1) > 0)
            {
                return 1; // 非常不滿意
            }
            if (score.Count(n => n == 3) > 0)
            {
                return 3; // 無意見
            }
            if (score.Count(n => n == 5) > 0)
            {
                return 5; // 非常滿意
            }

            return -1;
        }

        /// <summary>
        /// 檢查申請案號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string CheckApplyId(QuestNonWebModel model)
        {
            string rst = null;
            if (string.IsNullOrEmpty(model.ApplyId)) { rst = "申請案號有誤!!"; return rst; }

            int year = Int32.Parse(model.ApplyId.Substring(0, 4));
            int yearTw = year - 1911;
            int y1 = DateTime.Now.Year - 1911;
            int y2 = y1 - 1;
            if (yearTw != y1 && yearTw != y2)
            {
                rst = string.Format("【申請案號有誤，限填寫{0}、{1}年結案之案號】", y2.ToString(), y1.ToString());
                return rst;
            }
            return rst;
        }

        private string GetUnitCD(string srv_id)
        {
            string rst = "";
            switch (srv_id)
            {

                // 醫事司
                case "001004":
                case "001005":
                case "001007":
                case "001008":
                    rst = "4";
                    break;
                // 護理及健康照護司
                case "001036":
                    rst = "55";
                    break;
                // 食藥署
                case "001035":
                case "001034":
                    rst = "6";
                    break;
                // 中醫藥司
                case "005001":
                case "005002":
                case "005003":
                case "005004":
                case "005005":
                case "005013":
                case "005014":
                    rst = "7";
                    break;
                // 國健署
                case "010001":
                case "010002":
                    rst = "69";
                    break;
                // 社工司
                case "011001":
                case "011002":
                case "011003":
                case "011004":
                case "011005":
                case "011006":
                case "011007":
                case "011008":
                case "011009":
                    rst = "8";
                    break;
                // 秘書處
                case "012001":
                    rst = "52";
                    break;
            }

            return rst;
        }
    }
}
