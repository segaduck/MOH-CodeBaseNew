using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ES.Models.ViewModels;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Omu.ValueInjecter;
using ES.Models;
using System.Net.Mail;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace ES.Controllers
{
    public class GetZIPController : Controller
    {
        /// <summary>
        /// 產壓縮檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void GetZIP(string APP_ID, string CASENAME)
        {
            #region 另存檔案至目錄

            // 判斷是否有資料夾
            if (!Directory.Exists(Server.MapPath(@"../Template/" + APP_ID + "_ZIP")))
            {
                Directory.CreateDirectory(Server.MapPath(@"../Template/" + APP_ID + "_ZIP"));
            }

            ShareDAO dao = new ShareDAO();

            // 取檔案後排序
            Apply_FileModel fm = new Apply_FileModel();
            fm.APP_ID = APP_ID;
            var fmlst = dao.GetRowList(fm);
            var newfmlst = from a in fmlst
                           orderby a.UPD_TIME descending
                           select a;
            // 不同檔案項目分別群組，取得最新的一筆下載
            var grpfmlst = newfmlst.GroupBy(x => new { x.FILE_NO, x.SRC_NO, x.BATCH_INDEX }).ToList();
            foreach (var gfmlst in grpfmlst)
            {
                var fli = gfmlst.Where(x => x.FILENAME != null || x.FILENAME == "").OrderByDescending(x => x.UPD_TIME).FirstOrDefault();
                // 紀錄FILE_NO 已避免重複
                string newFileName = "";
                int iPos = -1;
                if (!string.IsNullOrEmpty(fli.FILENAME))
                {
                    var fileRename = fli.FILENAME.Replace("/", "\\");
                    iPos = fileRename.LastIndexOf("\\");
                    newFileName = fli.FILENAME.Substring(iPos + 1, fli.FILENAME.Length - iPos - 1);

                    var FilePath = Server.MapPath(@"../Template/" + APP_ID + "_ZIP") + "/" + newFileName;//item.SRC_FILENAME;
                    var dbyte = dao.sftpDownload(fli.FILENAME);
                    System.IO.File.WriteAllBytes(FilePath, dbyte);
                }
            }          

            if (CASENAME == "001008")
            {
                //20201031 ADD for 醫事人員請領英文證明書用檔案(醫事人員/專科中文證書電子檔apply_001008_ath)
                Apply_001008_AthModel athfm = new Apply_001008_AthModel();
                athfm.APP_ID = APP_ID;
                var athfmlst = dao.GetRowList(athfm);
                var newathfmlst = from a in athfmlst
                                  orderby a.SRL_NO
                                  select a;

                // 紀錄FILE_NO 已避免重複
                foreach (var item in newathfmlst)
                {
                    if (!string.IsNullOrWhiteSpace(item.ATH_UP))
                    {
                        var pos = item.ATH_UP.LastIndexOf("\\");
                        var fname = item.ATH_UP.Substring(pos + 1, item.ATH_UP.Length - pos - 1);
                        var FilePath = Server.MapPath(@"../Template/" + APP_ID + "_ZIP") + "/" + fname;
                        var dbyte = dao.sftpDownload(item.ATH_UP);
                        System.IO.File.WriteAllBytes(FilePath, dbyte);
                    }
                }
            }

            #endregion

            string zipFileName = CASENAME + "_" + APP_ID + ".zip";
            string[] filenames = Directory.GetFiles(Server.MapPath(@"../Template/" + APP_ID + "_ZIP/"));
            //byte[] buffer = new byte[409600];

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(@"../Template/" + APP_ID + "_ZIP/" + zipFileName))))
            {
                // 設定壓縮比
                zp.SetLevel(2);


                // 逐一將資料夾內的檔案抓出來壓縮，並寫入至目的檔(.ZIP)
                foreach (string filename in filenames)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(filename));
                    zp.PutNextEntry(entry);
                    using (FileStream fs = new FileStream(filename, FileMode.Open))
                    {
                        byte[] buffer = new byte[fs.Length];
                        int readLength;
                        do
                        {
                            readLength = fs.Read(buffer, 0, buffer.Length);
                            if (readLength > 0)
                            {
                                zp.Write(buffer, 0, readLength);
                            }
                        } while (readLength > 0);
                    }
                }

            }
            FileInfo file = new FileInfo(Server.MapPath(@"../Template/" + APP_ID + "_ZIP/" + zipFileName));
            // Clear the content of the response
            Response.ClearContent();

            // LINE1: Add the file name and attachment, which will force the open/cance/save dialog to show, to the header
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", file.Name));
            //Response.Headers["Content-Disposition"] = "attachment; filename=" + zipFileName;

            // Add the file size into the response header
            Response.AddHeader("Content-Length", file.Length.ToString());

            // Set the ContentType
            Response.ContentType = "application/zip";
            Response.TransmitFile(file.FullName);
            // End the response
            Response.End();
            //Response.Flush();
            //Response.Close();
            //// 刪除資料夾
            if (Directory.Exists(Server.MapPath(@"../Template/" + APP_ID + "_ZIP")))
            {
                string[] files = Directory.GetFiles(Server.MapPath(@"../Template/" + APP_ID + "_ZIP"));
                string[] dirs = Directory.GetDirectories(Server.MapPath(@"../Template/" + APP_ID + "_ZIP"));
                foreach (string item in files)
                {
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }
                Directory.Delete(Server.MapPath(@"../Template/" + APP_ID + "_ZIP"));
            }
        }
    }
}
