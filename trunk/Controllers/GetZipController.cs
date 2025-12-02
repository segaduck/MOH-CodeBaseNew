using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using EECOnline.Commons;
using EECOnline.DataLayers;
using EECOnline.Models.Entities;
using EECOnline.Services;
using Omu.ValueInjecter;
using EECOnline.Models;
using System.Net.Mail;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Turbo.Commons;

namespace EECOnline.Controllers
{
    public class GetZIPController : Controller
    {
        /// <summary>
        /// 產壓縮檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void GetZIP(string apy_id, string srv_id,string forap008)
        {
            #region 另存檔案至目錄

            var dtnow = DateTime.Now.ToString("yyyyMMddHHmmss");
            // 判斷是否有資料夾
            if (!Directory.Exists(Server.MapPath(@"../Template/"+ dtnow + apy_id + "_ZIP")))
            {
                Directory.CreateDirectory(Server.MapPath(@"../Template/"+ dtnow + apy_id + "_ZIP"));
            }

            FrontDAO dao = new FrontDAO();

            // 取檔案後排序
            TblAPPLY_FILE af = new TblAPPLY_FILE();
            af.apy_id = apy_id;
            if (forap008.TONotNullString()!="")
            {
                af.apy_other_key = forap008;
            } 
            var fmlst = dao.GetRowList(af);
            var newfmlst = from a in fmlst
                           orderby a.modtime descending
                           select a;

            // 不同檔案項目分別群組，取得最新的一筆下載
            var grpfmlst = newfmlst.GroupBy(x => new { x.apy_main_key, x.apy_src_key }).ToList();
            foreach (var gfmlst in grpfmlst)
            {
                var fli = gfmlst.Where(x => x.apy_filename != null || x.apy_filename == "").OrderByDescending(x => x.modtime).FirstOrDefault();
                // 紀錄FILE_NO 已避免重複
                string newFileName = "";
                int iPos = -1;
                if (!string.IsNullOrEmpty(fli.apy_filename))
                {
                    var dt = (DateTime)HelperUtil.TransTwLongToDateTime(fli.modtime);
                    var fileRename = fli.apy_filename.Replace("/", "\\");
                    iPos = fileRename.LastIndexOf("\\");
                    newFileName = fli.apy_filename.Substring(iPos + 1, fli.apy_filename.Length - iPos - 1);

                    var FilePath = Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP") + "/" + newFileName + "." + fli.apy_src_extion;
                    var dbyte = dao.sftpDownload(fli.apy_filename + "." + fli.apy_src_extion, dt);
                    System.IO.File.WriteAllBytes(FilePath, dbyte);
                }
            }

            #endregion

            string zipFileName = srv_id + "_" + apy_id + ".zip";
            string[] filenames = Directory.GetFiles(Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP/"));
            //byte[] buffer = new byte[409600];

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP/" + zipFileName))))
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
            FileInfo file = new FileInfo(Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP/" + zipFileName));
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
            if (Directory.Exists(Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP")))
            {
                string[] files = Directory.GetFiles(Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP"));
                string[] dirs = Directory.GetDirectories(Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP"));
                foreach (string item in files)
                {
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }
                Directory.Delete(Server.MapPath(@"../Template/" + dtnow + apy_id + "_ZIP"));
            }
        }

        /// <summary>
        /// 產壓縮檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult GetFile(string apy_id, string apy_main_key, string apy_src_key)
        {
            SHAREDAO dao = new SHAREDAO();
            TblAPPLY_FILE fileWhere = new TblAPPLY_FILE();
            fileWhere.apy_id = apy_id;
            fileWhere.apy_main_key = apy_main_key;
            fileWhere.apy_src_key = apy_src_key;
            var file = dao.GetRow(fileWhere);
            if (file == null) { return null; }

            string path = file.apy_file_path;
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(stream, "application/octet-stream", file.apy_filename+"."+file.apy_src_extion);
        }

        /// <summary>
        /// 產壓縮檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult GetFile1(string filename)
        {
            SHAREDAO dao = new SHAREDAO();
            var localPath = dao.GetServerLocalPath("APPLY_FILE"); ;
            string path = localPath+ filename;
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(stream, "application/octet-stream", filename);
        }
    }
}