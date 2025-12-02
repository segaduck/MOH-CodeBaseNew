using Dapper;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Services;
using ES.Utils;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_001009Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_001009ViewModel model = new Apply_001009ViewModel();
            model = new Apply_001009ViewModel();
            model.APP_ID = appid;
            model = dao.QueryApply_001009(model);
            //model.LIC_TYPE = model.APPLY_CERT_CATE;
            return View("Index", model);
        } 

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001009ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                ErrorMsg = dao.SaveApply_001009(model);
                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";
                }
                else
                {
                    result.status = false;
                    result.message = ErrorMsg;
                }
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 套印申請書
        /// </summary>
        [HttpPost]
        public void Print(Apply_001009ViewModel model)
        {
            string path = Server.MapPath("~/Sample/apply001009.docx");
            byte[] buffer = null;

            model.APP_ID = model.Apply.APP_ID;
            BackApplyDAO dao = new BackApplyDAO();
            model = dao.QueryApply_001009(model);

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
                    if (model.APPLY_CAUSE=="1")
                    {
                        doc.ReplaceText("$AC1", "■任職-機構");
                        doc.ReplaceText("$AC2", "□其他");
                        doc.ReplaceText("$ACT1", model.APPLY_CAUSE_TEXT);
                        doc.ReplaceText("$ACT2", "");
                    }
                    else
                    {
                        doc.ReplaceText("$AC1", "□任職-機構");
                        doc.ReplaceText("$AC2", "■其他");
                        doc.ReplaceText("$ACT1", "");
                        doc.ReplaceText("$ACT2", model.APPLY_CAUSE_TEXT);
                    }
                    //申請證明類別
                    string sql = "";
                    var str = "";
                    sql +=" SELECT top 1 CODE_DESC AS TEXT ";
                    sql += " FROM CODE_CD ";
                    sql += " WHERE 1 = 1 ";
                    sql += " AND CODE_KIND ='F1_LICENSE_CD_1'";
                    sql += " AND CODE_PCD=''";
                    sql += " AND ISNULL(CODE_PCD,'')='' ";
                    sql += " AND CODE_CD='"+model.APPLY_CERT_CATE + "' ";
                    sql += " ORDER BY SEQ_NO,CODE_CD ";

                    using (SqlConnection conn = DataUtils.GetConnection())
                    {
                        conn.Open();
                        str = conn.Query<string>(sql.ToString()).FirstOrDefault();
                        conn.Close();
                        conn.Dispose();
                    }

                    doc.ReplaceText("$APP_CERT_CATE", str.TONotNullString());

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
                    var yymmdd = $"西元{model.STUDY_START_YM.Substring(0, 4)}年{model.STUDY_START_YM.Substring(4, 2)}月至西元{model.STUDY_END_YM.Substring(0, 4)}年{model.STUDY_END_YM.Substring(4, 2)}月";
                    doc.ReplaceText("$YYMMDD", yymmdd);
                    //doc.ReplaceText("$YY1", model.STUDY_START_YM.Substring(0,4));
                    //doc.ReplaceText("$MM1", model.STUDY_START_YM.Substring(4, 2));
                    //doc.ReplaceText("$YY2", model.STUDY_END_YM.Substring(0, 4));
                    //doc.ReplaceText("$MM2", model.STUDY_END_YM.Substring(4, 2));

                    //通訊處地址
                    doc.ReplaceText("$ADDR", model.Apply.ADDR);

                    //電話
                    doc.ReplaceText("$TEL", model.Apply.TEL);

                    //國別及收件者
                    doc.ReplaceText("$V_MR", model.MAIL_COUNTRY+"、"+model.RECEIVER);

                    //本求證表郵寄地址（含國別及收件者）
                    doc.ReplaceText("$V_ADDR", model.VERIFY_ADDRESS);

                    //套列日期
                    doc.ReplaceText("$MYY", HelperUtil.DateTimeToTwString(DateTime.Now).Substring(0,3));
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
        }
    }
}
