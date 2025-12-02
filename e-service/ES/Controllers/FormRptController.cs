using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Action.Form;
using System.IO;
using ES.Extensions;
using System.Data.SqlClient;
using log4net;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Omu.ValueInjecter;
using ES.Models;
using Xceed.Words.NET;

namespace ES.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FormRptController : BaseController
    {
        /// <summary>
        /// 醫事人員證書補(換)發
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Apply001005(string id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form001005Action action = new Form001005Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                b = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return File(b, "application/pdf", "Apply" + id + ".pdf");
        }

        /// <summary>
        /// 專科醫師證書補(換)發
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Apply001007(string id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form001007Action action = new Form001007Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                b = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return File(b, "application/pdf", "Apply" + id + ".pdf");
        }

        /// <summary>
        /// 醫事人員請領英文證明申請書
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Apply001008(string id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form001008Action action = new Form001008Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                data.Add("LIST_ME", action.GetListME(id));
                data.Add("LIST_PR", action.GetListPR(id));
                b = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return File(b, "application/pdf", "Apply" + id + ".pdf");
        }

        /// <summary>
        /// 專科護理師證書補(換)發
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Apply001036(string id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form001036Action action = new Form001036Action(conn);
                Dictionary<string, object> data = action.GetData(id);

                b = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return File(b, "application/pdf", "Apply" + id + ".pdf");
        }

        /// <summary>
        /// 醫事人員請領無懲戒紀錄證明申請書
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Apply001037(string id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form001037Action action = new Form001037Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                b = action.GetApplyPDF(data);
                conn.Close();
                conn.Dispose();
            }

            return File(b, "application/pdf", "Apply" + id + ".pdf");
        }

        /// <summary>
        /// 醫事人員資格英文求證
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void Apply001009(string id)
        {
            string path = Server.MapPath("~/Sample/apply001009.docx");
            byte[] buffer = null;

            Apply_001009ViewModel model = new Apply_001009ViewModel();
            SessionModel sm = SessionModel.Get();

            model.APP_ID = id;
            model.Apply = new ApplyModel();
            model.Apply.PRO_ACC = sm.UserInfo.Admin.ACC_NO;
            BackApplyDAO dao = new BackApplyDAO();
            model = dao.QueryApply_001009RPT(model);

            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //中文
                    doc.ReplaceText("$NAME", model.Apply.NAME);
                    //英文
                    doc.ReplaceText("$ENAME", model.Apply.ENAME);
                    //英文別名
                    doc.ReplaceText("$E_ALIAS_NAME", model.Apply.E_ALIAS_NAME.TONotNullString());
                    //申請人出生年月日
                    doc.ReplaceText("$BYY", model.Apply.BIRTHDAY?.ToString("yyyy"));
                    doc.ReplaceText("$BMM", model.Apply.BIRTHDAY?.ToString("MM"));
                    doc.ReplaceText("$BDD", model.Apply.BIRTHDAY?.ToString("dd"));
                    //申請事由：□任職-機構：　　　 　　　□其他　　  　
                    if (model.APPLY_CAUSE == "1")
                    {
                        doc.ReplaceText("$AC1", "■任職-機構");
                        doc.ReplaceText("$AC2", "□其他");
                        doc.ReplaceText("$ACT1", model.APPLY_CAUSE_TEXT);
                        doc.ReplaceText("$ACT2", "      ");
                    }
                    else
                    {
                        doc.ReplaceText("$AC1", "□任職-機構");
                        doc.ReplaceText("$AC2", "■其他");
                        doc.ReplaceText("$ACT1", "      ");
                        doc.ReplaceText("$ACT2", model.APPLY_CAUSE_TEXT);
                    }
                    //申請證明類別
                    doc.ReplaceText("$APP_CERT_CATE", model.APPLY_CERT_CATE);

                    //醫事人員證書字號
                    doc.ReplaceText("$LIC_NUM", model.LIC_CD + " 字 " + model.LIC_NUM);

                    //醫事人員證書日期
                    doc.ReplaceText("$CYY", model.CERT_APPROVED_DATE?.ToString("yyyy"));
                    doc.ReplaceText("$CMM", model.CERT_APPROVED_DATE?.ToString("MM"));
                    doc.ReplaceText("$CDD", model.CERT_APPROVED_DATE?.ToString("dd"));

                    //畢業學校名稱及所在縣市
                    doc.ReplaceText("$C_SCHOOL_NAME", model.C_SCHOOL_NAME);
                    doc.ReplaceText("$E_SCHOOL_NAME", model.E_SCHOOL_NAME);

                    //修業年限
                    doc.ReplaceText("$YY1", model.STUDY_START_YM.Substring(0, 4));
                    doc.ReplaceText("$MM1", model.STUDY_START_YM.Substring(4, 2));
                    doc.ReplaceText("$YY2", model.STUDY_END_YM.Substring(0, 4));
                    doc.ReplaceText("$MM2", model.STUDY_END_YM.Substring(4, 2));

                    //通訊處地址
                    doc.ReplaceText("$ADDR", model.Apply.ADDR);

                    //電話
                    doc.ReplaceText("$TEL", model.Apply.TEL);

                    //國別及收件者
                    doc.ReplaceText("$V_MR", model.MAIL_COUNTRY + "、" + model.RECEIVER);

                    //本求證表郵寄地址（含國別及收件者）
                    doc.ReplaceText("$V_ADDR", model.VERIFY_ADDRESS);

                    //套列日期
                    doc.ReplaceText("$MYY", HelperUtil.DateTimeToTwString(DateTime.Now).Substring(0, 3));
                    doc.ReplaceText("$MMM", HelperUtil.DateTimeToTwString(DateTime.Now).Substring(4, 2));
                    doc.ReplaceText("$MDD", HelperUtil.DateTimeToTwString(DateTime.Now).Substring(7, 2));

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=醫事人員或公共衛生師資格英文求證.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
            RedirectToAction("BACKMIN", "Assign");
        }

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

            return File(b, "application/octet-stream", "Apply" + id + ".docx");
        }

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

            return File(b, "application/octet-stream", "Apply" + id + ".docx");
        }

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
            if (b == null) { return HttpNotFound(); }
            return File(b, "application/octet-stream", "Apply" + id + ".docx");
        }


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

            return File(b, "application/octet-stream", "Apply" + id + ".docx");
        }


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

            return File(b, "application/octet-stream", "Apply" + id + ".docx");
        }

        public ActionResult Apply005011XML(string id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form005011Action action = new Form005011Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                b = action.GetXML(data);
                conn.Close();
                conn.Dispose();
            }

            return File(b, "text/xml");
        }

        public ActionResult Apply005012XML(string id)
        {
            byte[] b = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                Form005012Action action = new Form005012Action(conn);
                Dictionary<string, object> data = action.GetData(id);
                List<Dictionary<string, object>> list = action.GetList(id);
                b = action.GetXML(data, list);
                conn.Close();
                conn.Dispose();
            }

            return File(b, "text/xml");
        }
    }
}
