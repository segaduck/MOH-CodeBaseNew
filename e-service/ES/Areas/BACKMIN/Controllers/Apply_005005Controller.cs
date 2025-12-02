using ES.Areas.BACKMIN.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_005005Controller : BaseController
    {
        [DisplayName("005005_案件審理")]
        public ActionResult Index(string appid, string srvid)
        {
            BackApplyDAO dao = new BackApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005005ViewModel form = new Apply_005005ViewModel();

            form.Form = dao.QueryApply_005005(appid);
            if (form.Form.CODE_CD == "2")
            {
                form.Detail = dao.GetApplyNotice_005005(appid);
            }

            //案件是否過期
            form.Form.IS_CASE_LOCK = shareDao.CalculationDocDate("005005", appid);

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

        [HttpPost]
        public ActionResult Save(Apply_005005ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\'\.\-\,\s\(\)]+$");
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if ((model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20" || model.Form.CODE_CD == "4") && string.IsNullOrEmpty(model.Form.MOHW_CASE_NO))
                {
                    ErrorMsg += "請取得公文文號。";
                    if (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE))
                    {
                        ErrorMsg += "請輸入公文日期。";
                    }
                }

                if (!string.IsNullOrEmpty(model.Form.IMP_COUNTRY))
                {
                    if (model.Form.IMP_COUNTRY.Trim().Substring(model.Form.IMP_COUNTRY.Trim().Length - 1, 1) != ".")
                    {
                        ErrorMsg += "外銷國家最後請以「.」結尾。\r\n";
                    }

                    if (!reg.IsMatch(model.Form.IMP_COUNTRY))
                    {
                        ErrorMsg += "外銷國家請以英文填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(model.Form.LIC_NUM))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(model.Form.LIC_NUM.ToUpper(), @"^\(C\)[0-9]{7}$"))
                    {
                        ErrorMsg += "製造廠許可編號輸入錯誤。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(model.Form.MF_CNT_NAME))
                {
                    if (!reg.IsMatch(model.Form.MF_CNT_NAME))
                    {
                        ErrorMsg += "製造廠名稱請以英文或數字填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(model.Form.MF_ADDR))
                {
                    if (!reg.IsMatch(model.Form.MF_ADDR))
                    {
                        ErrorMsg += "製造廠地址請以英文或數字填寫。\r\n";
                    }
                }

                if (ErrorMsg == "")
                {
                    //一般存檔
                    ErrorMsg = dao.AppendApply005005(model);
                    if (ErrorMsg == "")
                    {
                        if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                        {
                            //補件存檔
                            ErrorMsg = dao.AppendApplyDoc005005(model);
                        }
                    }
                }

                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";

                    if (model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20")
                    {
                        dao.CaseFinishMail_005005(model);
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
        [DisplayName("005005_申請單套表")]
        public void PreviewApplyForm(Apply_005005ViewModel model)
        {
            string path = Server.MapPath("~/Sample/apply005005.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //申請日期
                    //doc.ReplaceText("$APP_TIME_TW", "中華民國" + Form.APP_TIME_TW.Split('/')[0] + "年" + Form.APP_TIME_TW.Split('/')[1] + "月" + Form.APP_TIME_TW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);

                    //外銷國家
                    doc.ReplaceText("$IMP_COUNTRY", model.Form.IMP_COUNTRY.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠名稱
                    doc.ReplaceText("$MF_CNT_NAME", model.Form.MF_CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠地址
                    doc.ReplaceText("$MF_ADDR", model.Form.MF_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠許可編號
                    doc.ReplaceText("$LIC_NUM", model.Form.LIC_NUM.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠名稱
                    doc.ReplaceText("$ISSUE_DATE", Convert.ToDateTime(model.Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠名稱
                    doc.ReplaceText("$EXPIR_DATE", Convert.ToDateTime(model.Form.EXPIR_DATE).ToString("MMM. dd, yyyy.", CultureInfo.CreateSpecificCulture("en-US")), false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=GMP英文證明書.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

    }
}
