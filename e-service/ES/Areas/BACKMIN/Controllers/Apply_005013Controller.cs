using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Commons;
using ES.DataLayers;
using ES.Services;
using ES.Areas.BACKMIN.Models;
using ES.Models.Entities;
using ICSharpCode.SharpZipLib.Zip;
using Xceed.Words.NET;
using Xceed.Document.NET;
using ES.Models;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_005013Controller : BaseController
    {
        [DisplayName("005013_案件審理")]
        public ActionResult Index(string appid, string srvid)
        {
            BackApplyDAO dao = new BackApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005013ViewModel form = new Apply_005013ViewModel();

            form.Form = dao.QueryApply_005013(appid);
            //地址切割
            form.Form.TAX_ORG_CITY_CODE = form.Form.ADDR_CODE;
            form.Form.TAX_ORG_CITY_DETAIL = form.Form.ADDR;
            TblZIPCODE zip = new TblZIPCODE();
            zip.ZIP_CO = form.Form.ADDR_CODE;
            var getnam = dao.GetRow(zip);
            if (getnam != null)
            {
                form.Form.TAX_ORG_CITY_DETAIL = form.Form.ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                form.Form.TAX_ORG_CITY_TEXT = getnam.CITYNM + getnam.TOWNNM;
            }
            //電話切割
            form.Form.TEL_BEFORE = form.Form.TEL.Split('-')[0];
            var temp = form.Form.TEL.Split('-')[1].Split('#');
            form.Form.TEL_AFTER = temp[0];
            if (temp.Length > 1)
            {
                form.Form.TEL_Extension = temp[1];
            }
            if (form.Form.CODE_CD == "2")
            {
                form.Detail = dao.GetApplyNotice_005013(appid);
            }

            ////案件是否過期
            form.Form.IS_CASE_LOCK = shareDao.CalculationDocDate("005013", appid);

            return View(form);
        }

        [DisplayName("005013_下載壓縮檔")]
        [HttpPost]
        public void GetZipFile(string app_id)
        {
            #region 另存檔案至目錄     
            FileStreamResult file;

            // 判斷是否有資料夾
            if (!Directory.Exists(Server.MapPath(@"../../Template/" + app_id + "_ZIP")))
            {
                Directory.CreateDirectory(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
            }

            BackApplyDAO dao = new BackApplyDAO();
            ShareDAO sharedao = new ShareDAO();

            // 取檔案後排序
            Apply_FileModel fm = new Apply_FileModel();
            fm.APP_ID = app_id;
            var fmlst = dao.GetRowList(fm);
            var newfmlst = from a in fmlst
                           orderby a.ADD_TIME, a.FILE_NO descending
                           select a;

            // 紀錄FILE_NO 已避免重複
            var i = 0;
            foreach (var item in newfmlst)
            {
                if (i != item.FILE_NO.TOInt32())
                {
                    i = item.FILE_NO.TOInt32();
                    var FilePath = Server.MapPath(@"../../Template/" + app_id + "_ZIP") + "/" + item.SRC_FILENAME;
                    var dbyte = sharedao.sftpDownload(item.FILENAME);
                    System.IO.File.WriteAllBytes(FilePath, dbyte);
                }
            }
            #endregion

            string[] filenames = Directory.GetFiles(Server.MapPath(@"../../Template/" + app_id + "_ZIP/"));
            byte[] buffer = new byte[4096];

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(@"../../Template/" + app_id + "_ZIP/中藥GMP廠證明文件(英文).ZIP"))))
            {
                // 設定壓縮比
                zp.SetLevel(0);

                // 逐一將資料夾內的檔案抓出來壓縮，並寫入至目的檔(.ZIP)
                foreach (string filename in filenames)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(filename));
                    zp.PutNextEntry(entry);
                }

            }

            Response.ContentType = "application/zip";
            Response.Headers["Content-Disposition"] = "attachment; filename=中藥GMP廠證明文件(英文).ZIP";
            Response.TransmitFile(Server.MapPath(@"../../Template/" + app_id + "_ZIP/中藥GMP廠證明文件(英文).ZIP"));
            Response.Flush();
            Response.Close();
            // 刪除資料夾
            if (Directory.Exists(Server.MapPath(@"../../Template/" + app_id + "_ZIP")))
            {
                string[] files = Directory.GetFiles(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
                string[] dirs = Directory.GetDirectories(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
                foreach (string item in files)
                {
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }

                Directory.Delete(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
            }
        }

        [DisplayName("005013_案件儲存")]
        [HttpPost]
        public ActionResult Save(Apply_005013ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if ((model.Form.CODE_CD == "4" || model.Form.CODE_CD == "20" || model.Form.CODE_CD == "0") && string.IsNullOrEmpty(model.Form.MOHW_CASE_NO))
                {
                    ErrorMsg += "請輸入公文文號。";
                }

                if (ErrorMsg == "")
                {
                    //一般存檔
                    ErrorMsg += dao.AppendApply005013(model);
                    if (ErrorMsg == "")
                    {
                        model.Form.ADDR = model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL;
                        if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                        {
                            //補件存檔
                            ErrorMsg += dao.AppendApplyDoc005013(model);
                        }
                    }
                }

                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";

                    if (model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20")
                    {
                        dao.CaseFinishMail_005013(model);
                    }
                }
                else
                {
                    result.status = false;
                    result.message = ErrorMsg;
                }
            }

            return Content(result.Serialize(), "application/json");
        }

        [HttpPost]
        [DisplayName("005013_申請單套表")]
        public void PreviewApplyForm1(Apply_005013ViewModel model)
        {
            BackApplyDAO dao = new BackApplyDAO();
            string path = Server.MapPath("~/Sample/apply005013_1.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.Form.APP_ID;
                    var applydata = dao.GetRow(apply);
                    APPLY_005013 ap005013 = new APPLY_005013();
                    ap005013.APP_ID = model.Form.APP_ID;
                    var ap005013data = dao.GetRow(ap005013);

                    var APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                    var APPLY_TYPE2 = "□保健養生";
                    var APPLY_TYPE3 = "□其他：(請說明)";
                    switch (model.Form.RADIOUSAGE.TONotNullString())
                    {
                        case "1":
                            APPLY_TYPE1 = "■疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "□保健養生";
                            APPLY_TYPE3 = "□其他：(請說明)";
                            break;
                        case "2":
                            APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "■保健養生";
                            APPLY_TYPE3 = "□其他：(請說明)";
                            break;
                        case "3":
                            APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "□保健養生";
                            APPLY_TYPE3 = "■其他：(請說明)" + model.Form.RADIOUSAGE_TEXT.TONotNullString();
                            break;
                        default:
                            break;
                    }

                    // 替換文字
                    doc.ReplaceText("[$YEAR]", ((((DateTime)applydata.APP_TIME).Year) - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)applydata.APP_TIME).Month).TONotNullString().Length < 2 ? "0" + (((DateTime)applydata.APP_TIME).Month).TONotNullString() : (((DateTime)applydata.APP_TIME).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)applydata.APP_TIME).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$NAME]", applydata.NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE1]", APPLY_TYPE1, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE2]", APPLY_TYPE2, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE3]", APPLY_TYPE3, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", applydata.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$PHONE]", applydata.TEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$ADDR]", applydata.ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$NAME1]", applydata.NAME.TONotNullString() + "線上申請案，案件編號:" + applydata.APP_ID.TONotNullString().TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);


                    // 動態表格
                    var tb = doc.AddTable(model.Form.ApplyItem2.ToCount() + 1, 4);
                    tb.Paragraphs[0].FontSize(11);
                    tb.Paragraphs[0].Font("標楷體");
                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Rows[0].Cells[0].Width = 50;
                    tb.Rows[0].Cells[1].Width = 120;
                    tb.Rows[0].Cells[2].Width = 160;
                    tb.Rows[0].Cells[3].Width = 160;
                    tb.Rows[0].Cells[0].Paragraphs[0].Append("").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[1].Paragraphs[0].Append("藥品名稱").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[2].Paragraphs[0].Append("用法").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[3].Paragraphs[0].Append("總數量").Font("標楷體").Alignment = Alignment.center;
                    var q = 1;
                    foreach (var item in model.Form.ApplyItem2)
                    {
                        tb.Rows[q].Cells[0].Width = 50;
                        tb.Rows[q].Cells[1].Width = 120;
                        tb.Rows[q].Cells[2].Width = 160;
                        tb.Rows[q].Cells[3].Width = 160;
                        tb.Rows[q].Cells[0].Paragraphs[0].Append(q.TONotNullString()).Font("標楷體").Alignment = Alignment.center;
                        tb.Rows[q].Cells[1].Paragraphs[0].Append(item.ItemName.TONotNullString()).Font("標楷體");
                        tb.Rows[q].Cells[2].Paragraphs[0].Append(item.Usage.TONotNullString()).Font("標楷體");
                        tb.Rows[q].Cells[3].Paragraphs[0].Append(item.AllQty.TONotNullString()).Font("標楷體");
                        q++;
                    }
                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.SaveAs(ms);

                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=個人少量自用貨品進口切結書.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        [HttpPost]
        [DisplayName("005013_申請單套表")]
        public void PreviewApplyForm2(Apply_005013ViewModel model)
        {
            BackApplyDAO dao = new BackApplyDAO();
            string path = Server.MapPath("~/Sample/apply005013_2.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = model.Form.APP_ID;
                    var applydata = dao.GetRow(apply);
                    APPLY_005013 ap005013 = new APPLY_005013();
                    ap005013.APP_ID= model.Form.APP_ID;
                    var ap005013data = dao.GetRow(ap005013);
                    doc.ReplaceText("[$YEAR]", (((DateTime)applydata.APP_TIME).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)applydata.APP_TIME).Month).TONotNullString().Length < 2 ? "0" + (((DateTime)applydata.APP_TIME).Month).TONotNullString() : (((DateTime)applydata.APP_TIME).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)applydata.APP_TIME).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLICANT]", applydata.NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", applydata.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$AATO]", applydata.ADDR.TONotNullString() + "\n" + model.Form.TEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COO]", ap005013data.ORIGIN_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SP]", ap005013data.SHIPPINGPORT_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COS]", ap005013data.SELLER_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLYMAN]", applydata.NAME + "線上申請案\n案件編號:" + applydata.APP_ID.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    var tb = doc.AddTable(model.Form.Item_005013.ToCount() + 1, 5);
                    tb.Paragraphs[0].FontSize(11);
                    tb.Paragraphs[0].Font("標楷體");
                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Rows[0].Cells[0].Paragraphs[0].Append("⑦項次\n Item").FontSize(11).Font("標楷體"); 
                    tb.Rows[0].Cells[1].Paragraphs[0].Append("⑧貨名、規格、廠牌及製造廠名稱\n Description of Commodities Spec.and Brand or Maker, etc.").FontSize(11).Font("標楷體") ;
                    tb.Rows[0].Cells[2].Paragraphs[0].Append("⑨貨品分類稅則號列\n C.C.C.Code").FontSize(11).Font("標楷體");
                    tb.Rows[0].Cells[3].Paragraphs[0].Append("⑩數量\n Q'ty").FontSize(11).Font("標楷體") ;
                    tb.Rows[0].Cells[4].Paragraphs[0].Append("⑪單位\n Unit").FontSize(11).Font("標楷體");
                    var q = 1;
                    foreach (var item in model.Form.Item_005013)
                    {
                        tb.Rows[q].Cells[0].Paragraphs[0].Append(q.TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[1].Paragraphs[0].Append(item.Commodities.TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[2].Paragraphs[0].Append("").FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[3].Paragraphs[0].Append(item.Qty.TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[4].Paragraphs[0].Append(item.Unit_TEXT.TONotNullString()+"\n"+ item.SpecQty.TONotNullString()+item.SpecUnit_TEXT.TONotNullString()).FontSize(12).Font("標楷體");
                        q++;
                    }
                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=民眾少量自用中藥貨品進口申請單.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }
    }
}
