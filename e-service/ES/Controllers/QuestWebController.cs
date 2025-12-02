using ES.Action;
using ES.DataLayers;
using ES.Models;
using ES.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ES.Controllers
{
    /// <summary>
    /// 非線上案件滿意度調查
    /// </summary>
    public class QuestWebController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string id)
        {
            SessionModel sm = SessionModel.Get();
            var isHistory = false;
            bool flag_no_id = false;
            if (id == null) { flag_no_id = true; }
            if (string.IsNullOrEmpty(id)) { flag_no_id = true; }
            if (flag_no_id)
            {
                //MessageBoxModel msg = new MessageBoxModel("Index", "History", "申請案號錯誤");
                //TempData["MessageBoxModel"] = msg;
                //return RedirectToAction("Index", "MessageBox");
                sm.LastErrorMessage = "申請案號錯誤!";
                isHistory = true;
            }

            //WebDAO dao = new WebDAO();
            QuestWebModel model = new QuestWebModel();
            model.ApplyId = id;
            //model.UpdateAccount = GetAccount();
            model.UpdateAccount = sm.UserInfo.UserNo;
            model.ACC_NO = sm.UserInfo.UserNo;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                QuestWebAction action = new QuestWebAction(conn);

                model = action.GetData(model);

                if (model == null)
                {
                    //MessageBoxModel msg = new MessageBoxModel("Index", "History", "申請案號錯誤");
                    //TempData["MessageBoxModel"] = msg;
                    //return RedirectToAction("Index", "MessageBox");
                    sm.LastErrorMessage = "申請案號錯誤!";
                    isHistory = true;
                }

                //.Equals("Y")
                if (model.ExistsMark)
                {
                    //MessageBoxModel msg = new MessageBoxModel("Index", "History", "您己填寫滿意度調查表，同一案件僅能填寫一次");
                    //TempData["MessageBoxModel"] = msg;
                    //return RedirectToAction("Index", "MessageBox");
                    sm.LastResultMessage = "您己填寫滿意度調查表，同一案件僅能填寫一次";
                    isHistory = true;
                }

                //ViewBag.ServiceList = CodeUtils.GetCodeSelectList(conn, "SURVEY_SRV", "", null, false);
                //ViewBag.UnitList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", null, false);
                conn.Close();
                conn.Dispose();
            }
            if (isHistory)
            {
                return RedirectToAction("Index", "History");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(QuestWebModel model)
        {
            //string s_msg = "";//s_msg = ""; //string s_msg = "";
            //s_msg += string.Format(" model.ApplyId :{0}", model.ApplyId);
            //logger.Debug(s_msg);
            SessionModel sm = SessionModel.Get();
            var isHistory = false;
            if (ModelState.IsValid)
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
                    sm.LastErrorMessage = "請檢查輸入項目!!";
                    return View(model);
                }

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    QuestWebAction action = new QuestWebAction(conn);
                    if (action.CheckApplyIdExists(model))
                    {
                        //tMsg = "您己填寫滿意度調查表，同一案件僅能填寫一次";
                        sm.LastResultMessage = "您己填寫滿意度調查表，同一案件僅能填寫一次";
                    }
                    else
                    {
                        SqlTransaction tran = conn.BeginTransaction();
                        action = new QuestWebAction(conn, tran);
                        model.Satisfied = GetSatisfied(model);
                        model.UpdateAccount = sm.UserInfo.UserNo;
                        model.ACC_NO = sm.UserInfo.UserNo;
                        //s_msg = ""; //string s_msg = "";
                        //s_msg += string.Format(" model.UpdateAccount:{0}", model.UpdateAccount);
                        //s_msg += string.Format(" model.ACC_NO :{0}", model.ACC_NO);
                        //logger.Debug(s_msg);

                        bool flag_save_ok = action.Insert(model);

                        if (flag_save_ok)
                        {
                            tran.Commit();
                            sm.LastResultMessage = "感謝您的填寫";
                            //MessageBoxModel msg = new MessageBoxModel("Index", "History", "感謝您的填寫");
                            //TempData["MessageBoxModel"] = msg;
                            SendMail(model);
                            //string s_msg1 = ""; //string s_msg = "";
                            //s_msg1 += string.Format("\n QuestWebController :{0}", "感謝您的填寫");
                            //logger.Debug(s_msg1);
                            //return RedirectToAction("Index", "MessageBox");
                            isHistory = true;
                        }
                        else
                        {
                            tran.Rollback();
                            //tMsg = "存檔失敗";
                            sm.LastErrorMessage = "存檔失敗!!";
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isHistory)
                {
                    return RedirectToAction("Index", "History");
                }
                //ViewBag.tempMessage = tMsg;
            }
            else
            {
                //ViewBag.tempMessage = "欄位驗證錯誤";
                sm.LastErrorMessage = "欄位驗證錯誤!!";
            }

            //using (SqlConnection conn = GetConnection())
            //{
            //    conn.Open();
            //    ViewBag.ServiceList = CodeUtils.GetCodeSelectList(conn, "SURVEY_SRV", "", model.ServiceId, false);
            //    ViewBag.UnitList = CodeUtils.GetCodeSelectList(conn, "SURVEY_UNIT", "", model.UnitCode.ToString(), false);
            //}

            //string s_msg = ""; //string s_msg = "";
            //s_msg += string.Format("\n QuestWebController.ViewBag.tempMessage :{0}", ViewBag.tempMessage);
            //logger.Debug(s_msg);

            return View(model);
        }

        private void SendMail(QuestWebModel model)
        {
            List<int> score = new List<int>();
            score.Add(model.Q1);
            score.Add(model.Q2);
            score.Add(model.Q3);
            score.Add(model.Q4);
            score.Add(model.Q5);

            //logger.Debug("count: " + score.Count(n => n == 1));
            SessionModel sm = SessionModel.Get();

            if (model.Recommend != null) { model.Recommend = model.Recommend.Trim(); }

            //string s_msg = ""; //string s_msg = "";
            //s_msg += string.Format("\n model.Recommend :{0}", model.Recommend);
            //s_msg += string.Format("\n score.Count(n => n == 1):{0}", score.Count(n => n == 1));
            //logger.Debug(s_msg);

            if (score.Count(n => n == 1) > 0 || !String.IsNullOrEmpty(model.Recommend))
            {
                try
                {
                    string sendTo = DataUtils.GetConfig("SURVEY_MAIL_1");
                    if (string.IsNullOrEmpty(sendTo)) { return; }
                    string subject = MessageUtils.MAIL_SURVEY_SUBJECT;
                    if (Session["Member"] == null) { return; }
                    string userMail = ((MemberModel)Session["Member"]).Mail;

                    //s_msg = ""; //string s_msg = ""; //string s_msg = "";
                    //s_msg += string.Format("\n sendTo:{0}", sendTo);
                    //s_msg += string.Format("\n subject:{0}", subject);
                    //s_msg += string.Format("\n userMail::((MemberModel)Session[\"Member\"]).Mail:{0}", userMail);
                    //logger.Debug(s_msg);

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
        private int GetSatisfied(QuestWebModel model)
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
    }
}
