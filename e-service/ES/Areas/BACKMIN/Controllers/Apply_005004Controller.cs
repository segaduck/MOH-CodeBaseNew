using ES.Areas.BACKMIN.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ICSharpCode.SharpZipLib.Zip;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_005004Controller : BaseController
    {
        [DisplayName("005004_案件審理")]
        public ActionResult Index(string appid, string srvid)
        {
            BackApplyDAO dao = new BackApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005004ViewModel form = new Apply_005004ViewModel();

            form.Form = dao.QueryApply_005004(appid);
            if (form.Form.CODE_CD == "2")
            {
                form.Detail = dao.GetApplyNotice_005004(appid);
            }
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

            //案件是否過期
            form.Form.IS_CASE_LOCK = shareDao.CalculationDocDate("005004", appid);

            return View(form);
        }

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

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(@"../../Template/" + app_id + "_ZIP/中藥GMP廠證明文件(中文).ZIP"))))
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
            Response.Headers["Content-Disposition"] = "attachment; filename=中藥GMP廠證明文件(中文).ZIP";
            Response.TransmitFile(Server.MapPath(@"../../Template/" + app_id + "_ZIP/中藥GMP廠證明文件(中文).ZIP"));
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

        [HttpPost]
        public ActionResult Save(Apply_005004ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if ((model.Form.CODE_CD == "4" || model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20") && string.IsNullOrEmpty(model.Form.MOHW_CASE_NO))
                {
                    ErrorMsg += "請取得公文文號。";
                    if (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE))
                    {
                        ErrorMsg += "請輸入公文日期。";
                    }
                }
                else if (!string.IsNullOrEmpty(model.Form.MOHW_CASE_NO) && (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE) || !model.Form.MOHW_CASE_DATE.Contains("/")))
                {
                    ErrorMsg += "請輸入公文日期。";
                }
                if (model.Form.CODE_CD == "2")
                {
                    var t = 0;
                    foreach (var item in model.Detail.GetType().GetProperties())
                    {
                        if (item.GetValue(model.Detail) != null && (item.GetValue(model.Detail)).ToString() != "False" && (item.GetValue(model.Detail)).ToString() != "True")
                        {
                            t++;
                        }
                    }
                    if (t == 0)
                    {
                        ErrorMsg += "請選擇補件項目並輸入備註說明。";
                    }
                }

                if (ErrorMsg == "")
                {
                    //一般存檔
                    ErrorMsg = dao.AppendApply005004(model);

                    if (ErrorMsg == "")
                    {
                        if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                        {
                            //補件存檔
                            ErrorMsg = dao.AppendApplyDoc005004(model);
                        }
                    }
                }

                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";

                    if (model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20")
                    {
                        dao.CaseFinishMail_005004(model);
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
        [DisplayName("005004_申請單套表")]
        public void PreviewApplyForm(Apply_005004ViewModel model)
        {
            string path = Server.MapPath("~/Sample/apply005004.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //申請日期
                    doc.ReplaceText("$APP_TIME_TW", "中華民國" + model.Form.APP_TIME_TW.Split('/')[0] + "年" + model.Form.APP_TIME_TW.Split('/')[1] + "月" + model.Form.APP_TIME_TW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);

                    //申請類別
                    if (model.Form.APPLY_TYPE == "新申請")
                    {
                        doc.ReplaceText("$APPLY_TYPE", "■新申請       □遺失補發      □汙損換發", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (model.Form.APPLY_TYPE == "遺失補發")
                    {
                        doc.ReplaceText("$APPLY_TYPE", "□新申請       ■遺失補發      □汙損換發", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (model.Form.APPLY_TYPE == "汙損換發")
                    {
                        doc.ReplaceText("$APPLY_TYPE", "□新申請       □遺失補發      ■汙損換發", false, System.Text.RegularExpressions.RegexOptions.None);
                    }

                    //製造廠名稱/藥廠名稱
                    doc.ReplaceText("$MF_CNT_NAME", model.Form.NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //藥商許可執照字號
                    doc.ReplaceText("$PL_CD_TEXT", model.Form.PL_CD.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("$PL_Num", model.Form.PL_Num.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠名稱/藥廠地址
                    doc.ReplaceText("$TAX_ORG_CITY_CODE", model.Form.TAX_ORG_CITY_CODE.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("$TAX_ORG_CITY_TEXT", model.Form.TAX_ORG_CITY_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("$TAX_ORG_CITY_DETAIL", model.Form.TAX_ORG_CITY_DETAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //聯絡人
                    doc.ReplaceText("$CNT_NAME", model.Form.CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //電話
                    doc.ReplaceText("$TELALL", model.Form.TEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //傳真
                    doc.ReplaceText("$FAXALL", string.IsNullOrEmpty(model.Form.FAX) ? "" : model.Form.FAX, false, System.Text.RegularExpressions.RegexOptions.None);

                    //EMAIL
                    doc.ReplaceText("$EMALALL", model.Form.EMAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //負責人身分證統一編號
                    doc.ReplaceText("$IDN", model.Form.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //負責人姓名
                    doc.ReplaceText("$CHR_NAME", model.Form.CHR_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //■製劑    □原料藥$PROCESS_TYPE
                    if (model.Form.CON_CHECK == "Y" && model.Form.TRA_CHECK == "Y")
                    {
                        doc.ReplaceText("$PROCESS_TYPE", "■製劑    ■原料藥", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (model.Form.CON_CHECK == "Y")
                    {
                        doc.ReplaceText("$PROCESS_TYPE", "■製劑    □原料藥", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (model.Form.TRA_CHECK == "Y")
                    {
                        doc.ReplaceText("$PROCESS_TYPE", "□製劑    ■原料藥", false, System.Text.RegularExpressions.RegexOptions.None);
                    }

                    //監製藥師
                    doc.ReplaceText("$PP_NAME", model.Form.PP_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //工廠登記證字號
                    doc.ReplaceText("$FRC_Num", model.Form.FRC_NUM.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //最近一次GMP查廠日期
                    doc.ReplaceText("$GMP_TIME_TW", "中華民國 " + model.Form.ISSUE_DATE_TW.Split('/')[0] + " 年 " + model.Form.ISSUE_DATE_TW.Split('/')[1] + " 月 " + model.Form.ISSUE_DATE_TW.Split('/')[2] + " 日 ", false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=中藥GMP廠證明文件申請單.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

    }
}
