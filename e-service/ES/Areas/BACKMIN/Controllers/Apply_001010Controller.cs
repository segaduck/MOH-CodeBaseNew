using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Omu.ValueInjecter;
using ES.Models;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Xceed.Words.NET;
using System.Data.SqlClient;
using ES.Utils;
using Dapper;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_001010Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_001010ViewModel model = new Apply_001010ViewModel();
            model.APP_ID = appid;
            model = dao.QueryApply_001010(model);
            model.Apply.NAME = System.Net.WebUtility.HtmlDecode(model.Apply.NAME.TONotNullString());
            if (model.PAY_EXT_TIME_AC != null)
            {
                model.PAY_MONEY = model.Apply.PAY_A_PAID;
            }
            return View("Index", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001010ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                ErrorMsg = dao.SaveApply_001010(model);
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
        public void Print(string APP_ID)
        {
            Apply_001010ViewModel model = new Apply_001010ViewModel();
            string path = Server.MapPath("~/Sample/apply001010.docx");
            byte[] buffer = null;

            model.APP_ID = APP_ID;
            BackApplyDAO dao = new BackApplyDAO();
            model = dao.PrintApply_001010(model);

            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    // 姓名
                    doc.ReplaceText("$NAME", model.Apply.NAME);
                    // 核發單位
                    var dept = string.Empty;
                    switch (model.ISSUE_DEPT.TONotNullString())
                    {
                        case "1":
                            dept = "內政部";
                            break;
                        case "2":
                            dept = "行政院衛生署";
                            break;
                        case "3":
                            dept = "衛生福利部";
                            break;
                    }
                    doc.ReplaceText("$ISSUE_DEPT", dept);
                    // 核發日期
                    doc.ReplaceText("$YEAR", (model.ISSUE_DATE?.ToString("yyyy").TOInt32() -1911).ToString());
                    doc.ReplaceText("$MON", model.ISSUE_DATE?.ToString("MM"));
                    doc.ReplaceText("$DAY", model.ISSUE_DATE?.ToString("dd"));
                    // 證書字號
                    doc.ReplaceText("$LIC_TXT", model.LIC_CD);
                    // 證書字號
                    doc.ReplaceText("$LIC_NUM", model.LIC_NUM);
                    //證書類別
                    string sql = "";
                    var str = "";
                    sql += " SELECT top 1 CODE_DESC AS TEXT ";
                    sql += " FROM CODE_CD ";
                    sql += " WHERE 1 = 1 ";
                    sql += " AND CODE_KIND ='F1_LICENSE_CD_1'";
                    sql += " AND CODE_PCD=''";
                    sql += " AND ISNULL(CODE_PCD,'')='' ";
                    sql += " AND CODE_CD='" + model.DIVISION + "' ";
                    sql += " ORDER BY SEQ_NO,CODE_CD ";

                    using (SqlConnection conn = DataUtils.GetConnection())
                    {
                        conn.Open();
                        str = conn.Query<string>(sql.ToString()).FirstOrDefault();
                        conn.Close();
                        conn.Dispose();
                    }

                    doc.ReplaceText("$DIVISION ", str.TONotNullString());

                    // 申請份數
                    doc.ReplaceText("$COPIES", model.COPIES.ToString());

                    // 身份證字號
                    doc.ReplaceText("$IDNO", model.Apply.IDN);

                    // 收件地址
                    doc.ReplaceText("$ADDR_CODE", model.Apply.ADDR_CODE);
                    doc.ReplaceText("$ADDR", model.Apply.ADDR);
                    
                    // 電話
                    doc.ReplaceText("$MOBILE", model.Apply.MOBILE);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=醫事人員或公共衛生師證書影本申請書.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }
    }
}
