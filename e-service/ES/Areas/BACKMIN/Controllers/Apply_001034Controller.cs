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

namespace ES.Areas.Admin.Controllers
{
    public class Apply_001034Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_001034ViewModel model = new Apply_001034ViewModel();
            model = new Apply_001034ViewModel();
            model.APP_ID = appid;
            model = dao.QueryApply_001034(model);
            
            return View("Index", model);
        }

        /// <summary>
        /// 產壓縮檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void GetZIP(string APP_ID)
        {
            #region 另存檔案至目錄     
            FileStreamResult file;

            // 判斷是否有資料夾
            if (!Directory.Exists(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP")))
            {
                Directory.CreateDirectory(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
            }

            BackApplyDAO dao = new BackApplyDAO();
            ShareDAO sharedao = new ShareDAO();

            // 取檔案後排序
            Apply_FileModel fm = new Apply_FileModel();
            fm.APP_ID = APP_ID;
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
                    var FilePath = Server.MapPath(@"../../Template/" + APP_ID + "_ZIP") + "/" + item.SRC_FILENAME;
                    var dbyte = sharedao.sftpDownload(item.FILENAME);
                    System.IO.File.WriteAllBytes(FilePath, dbyte);
                }
            }
            #endregion

            string zipFileName = "危險性醫療儀器進口申請_" + APP_ID + ".zip";
            string[] filenames = Directory.GetFiles(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP/"));
            byte[] buffer = new byte[4096];

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP/" + zipFileName))))
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
            Response.Headers["Content-Disposition"] = "attachment; filename=" + zipFileName;
            Response.TransmitFile(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP/" + zipFileName));
            Response.Flush();
            Response.Close();
            // 刪除資料夾
            if (Directory.Exists(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP")))
            {
                string[] files = Directory.GetFiles(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
                string[] dirs = Directory.GetDirectories(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
                foreach (string item in files)
                {
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }
                Directory.Delete(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
            }
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001034ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                ErrorMsg = dao.AppendApply001034(model);
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
    }
}
