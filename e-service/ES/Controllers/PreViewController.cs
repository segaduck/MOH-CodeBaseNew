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

namespace ES.Controllers
{
    public class PreViewController : Controller
    {
        /// <summary>
        /// 預覽
        /// </summary>
        /// <param name="FILENAME"></param>
        /// <param name="APP_ID"></param>
        /// <param name="FILE_NO"></param>
        /// <param name="SRC_NO"></param>
        /// <returns></returns>
        public ActionResult PreView(string FILENAME, string APP_ID, string FILE_NO, string SRC_NO)
        {
            ShareDAO dao = new ShareDAO();
            Apply_FileModel fileWhere = new Apply_FileModel();
            PreViewModel model = new PreViewModel();
            model.FILENAME = FILENAME;
            model.APP_ID = APP_ID;
            model.FILE_NO = FILE_NO;
            model.SRC_NO = SRC_NO;

            //20201104 修正：因為醫事人員請領英文證明書的「醫事人員/專科中文證書電子檔」上傳附件資料是存在既有的APPLY_001008_ATH，而不是通用的APPLY_FILE，所以額外再多加以服務項目(APPLY.SRV_ID)判斷抓資料名稱的來源資料表
            string srvID = "";
            ApplyModel applywhere = new ApplyModel() { APP_ID = APP_ID };
            ApplyModel apply = dao.GetRow(applywhere);
            byte[] dbyte = null;
            int iPos = -1;

            if (apply != null)
            {
                srvID = apply.SRV_ID;
            }

            if (srvID == "001008" && FILENAME.IndexOf("_ATH_UP_") > 0)
            {
                //add for 醫事人員請領英文證明書(001008 醫事人員/專科中文證書電子檔附件資料 apply_001008_ath )
                Apply_001008_AthModel athwhere = new Apply_001008_AthModel()
                {
                    APP_ID = APP_ID
                    , SRL_NO = FILE_NO.TOInt32()
                };

                if (!string.IsNullOrEmpty(SRC_NO))
                {
                    athwhere.NOTICE_NO = SRC_NO.TOInt32();
                }

                Apply_001008_AthModel ath = dao.GetRow(athwhere);
                if (ath != null)
                {
                    iPos = ath.ATH_UP.LastIndexOf("\\");
                    if (ath.ATH_UP.Substring(iPos + 1, ath.ATH_UP.Length - iPos -1) == FILENAME)
                    {
                        dbyte = dao.sftpDownload(ath.ATH_UP);
                    }
                }
            }
            else
            {
                fileWhere.APP_ID = APP_ID;
                fileWhere.FILE_NO = FILE_NO.TOInt32();
                //if (!string.IsNullOrEmpty(SRC_NO) && SRC_NO.TONotNullString() != "0")
                //{
                //    fileWhere.SRC_NO = SRC_NO.TOInt32();
                //}
                var files = dao.GetRowList(fileWhere);
                foreach (var item in files)
                {
                    // 取得符合編譯後的檔案名稱
                    var itemNum = item.FILENAME.LastIndexOf('\\');
                    if (item.FILENAME.Substring(itemNum + 1, item.FILENAME.Length - itemNum - 1) == FILENAME)
                    {
                        dbyte = dao.sftpDownload(item.FILENAME);
                    }
                }
            }
            
            // 判斷是否有資料夾
            if (!Directory.Exists(Server.MapPath(@"../Template/" + APP_ID)))
            {
                Directory.CreateDirectory(Server.MapPath(@"../Template/" + APP_ID));
            }
            var FilePath = Server.MapPath(@"../Template/" + APP_ID) + "/" + FILENAME;

            // 另存檔案至目錄
            System.IO.File.WriteAllBytes(FilePath, dbyte);

            return View("PreView", model);
        }

        /// <summary>
        /// 預覽
        /// </summary>
        /// <param name="FILENAME"></param>
        /// <param name="APP_ID"></param>
        /// <param name="FILE_NO"></param>
        /// <param name="SRC_NO"></param>
        /// <returns></returns>
        public ActionResult FrontPreView(string FILENAME)
        {
            ShareDAO dao = new ShareDAO();
            Apply_FileModel fileWhere = new Apply_FileModel();
            PreViewModel model = new PreViewModel();
            model.FILENAME = FILENAME;

            var dbyte = dao.sftpDownload(FILENAME);

            // 判斷是否有資料夾
            var FileList = FILENAME.ToSplit('/');
            var FileName = FileList[FileList.ToCount() - 1];
            var dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            var FilePathFULL= @"../Template/" + dt + "/" + FILENAME.Replace(FileName, "");
            var FIleDirectory = Server.MapPath(@"../Template/" + dt) + "/"+ FILENAME.Replace(FileName, "");

            if (!Directory.Exists(FIleDirectory))
            {
                Directory.CreateDirectory(FIleDirectory);
            }
            var FilePath = FIleDirectory + FileName;
            using (FileStream
            fileStream = new FileStream(FilePath, FileMode.Create))
            {
                // Write the data to the file, byte by byte.
                for (int i = 0; i < dbyte.Length; i++)
                {
                    fileStream.WriteByte(dbyte[i]);
                }

                // Set the stream position to the beginning of the file.
                fileStream.Seek(0, SeekOrigin.Begin);
                
            }

            model.FILEPATHFULL = FilePathFULL;


            //FileStream fs = System.IO.File.Create(FilePath);

            // 另存檔案至目錄
            //System.IO.File.WriteAllBytes(FilePath, dbyte);
            //fs.Close();

            return View("PreView", model);
        }


    }
}
