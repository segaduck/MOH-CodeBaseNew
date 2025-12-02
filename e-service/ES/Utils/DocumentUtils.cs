using ES.Controllers;
using ES.DataLayers;
using ES.Extensions;
using ES.Services;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace ES.Utils
{
    public class DocumentUtils : BaseController
    {
        string DownLoadPath = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH");//下載路徑           
        public bool DownLoadImage(string SourcePath, string FileName)
        {
            FileStream fs = new FileStream(DownLoadPath + FileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs, System.Text.Encoding.Default);
            try
            {
                byte[] xfile = null;
                System.Net.WebClient wc = new System.Net.WebClient(); //呼叫 webclient 方式做檔案下載                
                xfile = wc.DownloadData(SourcePath);
                bw.Write(xfile);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //清空緩衝區               
                bw.Flush();
                //關閉流
                bw.Close();
                fs.Close();
            }
        }

        public bool DownLoadApplyData(string APP_ID, string FileName, byte[] data)
        {
            FileStream fs = new FileStream(DownLoadPath + FileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs, System.Text.Encoding.Default);
            try
            {
                byte[] xfile = null;
                xfile = WordConverter.Convert(data, ES.Extensions.WordConverter.WordFileFormat.WordDocx, APP_ID);
                bw.Write(xfile);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //清空緩衝區               
                bw.Flush();
                //關閉流
                bw.Close();
                fs.Close();
            }
        }

        public bool DownLoadXml(string data, string FileName)
        {
            FileStream fs;
            if (FileName.IndexOf(".DI") > -1)
                fs = new FileStream(DownLoadPath + "DI\\" + FileName, FileMode.Create);
            else
                fs = new FileStream(DownLoadPath + FileName, FileMode.Create);

            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            try
            {
                sw.Write(data);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                //清空緩衝區               
                sw.Flush();
                //關閉流
                sw.Close();
                fs.Close();
            }
        }
        public Byte[] DownLoadXmlFileStream(string data, string FileName)
        {
            // 判斷資料夾是否存在
            FileDI();

            var filename = string.Empty;
            FileStream fs;
            if (FileName.IndexOf(".di") > -1)
            {
                fs = new FileStream(DownLoadPath + "DI\\" + FileName, FileMode.Create);
                filename = DownLoadPath + "DI\\" + FileName;
            }
            else
            {
                fs = new FileStream(DownLoadPath + FileName, FileMode.Create);
                filename = DownLoadPath + FileName;
            }
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            sw.Write(data);
            //清空緩衝區               
            sw.Flush();
            //關閉流
            sw.Close();
            fs.Close();

            return this.GetBinaryFile(filename);
        }

        public void FileDI()
        {
            //判斷資料夾是否存在
            if (!Directory.Exists(DownLoadPath))
            {
                //資料夾不存在
                Directory.CreateDirectory(DownLoadPath);
            }
            if (!Directory.Exists(DownLoadPath + "DI\\"))
            {
                //建立DI資料夾
                Directory.CreateDirectory(DownLoadPath + "DI\\");
            }
            //else
            //{
            //    try
            //    {
            //        Directory.Delete(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"), true);
            //    }
            //    catch { }

            //    Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"));
            //    //建立DI資料夾
            //    Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            //}
        }
        public void FileDIZIP()
        {
            //判斷資料夾是否存在
            if (!Directory.Exists(DownLoadPath))
            {
                //資料夾不存在
                Directory.CreateDirectory(DownLoadPath);
            }
            if (!Directory.Exists(DownLoadPath + "DIZIP\\"))
            {
                //建立DI資料夾
                Directory.CreateDirectory(DownLoadPath + "DIZIP\\");
            }
        }
        private byte[] GetBinaryFile(string filename)
        {
            byte[] bytes;
            using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
            }
            return bytes;
        }
        /// <summary>
        /// 產壓縮檔 中醫藥司
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Byte[] GetDI_ZIP(string APP_ID, string APP_ID_RAN, byte[] docx, List<Areas.Admin.Models.FileGroupModel> filelist, string srv_id)
        {
            #region 另存檔案至目錄
            var dao = new ShareDAO();
            var FileZipRoute = DownLoadPath + "DIZIP\\" + APP_ID_RAN.Replace(".", "_");
            // 判斷是否有資料夾
            this.FileDI();
            this.FileDIZIP();
            if (!Directory.Exists(FileZipRoute))
            {
                //建立DI資料夾
                Directory.CreateDirectory(FileZipRoute);
            }
            // 複製DI 檔案
            var FilePath = DownLoadPath + "DI\\" + APP_ID_RAN + ".di";
            var TargetPath = FileZipRoute;
            System.IO.File.Copy(FilePath, TargetPath + "\\" + APP_ID_RAN + ".di");
            // 產生 申請書
            var FilePathpdf = DownLoadPath + "DI\\applypdf.pdf";
            var TargetPathpdf = FileZipRoute + "\\" + APP_ID_RAN + "_Attach1.pdf";
            System.IO.File.Copy(FilePathpdf, TargetPathpdf);
            // 產生 切結書
            if (srv_id == "005013")
            {
                var FilePathpdf2 = DownLoadPath + "DI\\affidavit1.pdf";
                var TargetPathpdf2 = FileZipRoute + "\\" + APP_ID_RAN + "_Attach2.pdf";
                System.IO.File.Copy(FilePathpdf2, TargetPathpdf2);
            }
            else if (srv_id == "005014")
            {
                var FilePathpdf2 = DownLoadPath + "DI\\affidavit1.pdf";
                var TargetPathpdf2 = FileZipRoute + "\\" + APP_ID_RAN + "_Attach2.pdf";
                System.IO.File.Copy(FilePathpdf2, TargetPathpdf2);

                var FilePathpdf3 = DownLoadPath + "DI\\affidavit2.pdf";
                var TargetPathpdf3 = FileZipRoute + "\\" + APP_ID_RAN + "_Attach3.pdf";
                System.IO.File.Copy(FilePathpdf3, TargetPathpdf3);
            }
            // 複製 檔案上傳 附件
            if (filelist != null && filelist.Count > 0)
            {
                var seq = 2;
                if (srv_id == "005013")
                {
                    seq = 3;
                }
                else if (srv_id == "005014")
                {
                    seq = 4;
                }
                foreach (var item in filelist)
                {
                    var dot = item.FILE_NAME.ToSplit(".").LastOrDefault();
                    var ItemsPath = dao.getApplyFileRoute(item.SRC);
                    var TargetFileFilePath = FileZipRoute + "\\" + APP_ID_RAN + "_Attach" + seq.ToString() + "." + dot;
                    if (TargetFileFilePath != null)
                    {
                        System.IO.File.Copy(ItemsPath.Replace("/", "\\"), TargetFileFilePath);
                    }
                    seq++;
                }
            }

            #endregion

            string zipFileName = APP_ID_RAN + ".zip";
            logger.Debug($"GetDI_ZIP.zipFileName:{zipFileName}");
            //string[] filenames = Directory.GetFiles(TargetPath);
            //string[] fileattachs = Directory.GetFiles(TargetPath + "\\attach");
            //logger.Debug($"GetDI_ZIP.TargetPath:{TargetPath}");
            var zipFilePath = TargetPath + "\\" + zipFileName;
            var outzip = DownLoadPath + "DIZIP";
            ZipFileDictory(TargetPath, outzip + "\\" + zipFileName, APP_ID_RAN);

            return this.GetBinaryFile(outzip + "\\" + zipFileName);
        }
        /// 遞歸壓縮文件夾方法 
        private bool ZipFileDictory(string FolderToZip, ZipOutputStream s, string ParentFolderName, string APP_ID_RAN)
        {
            bool res = true;
            string[] folders, filenames;
            ZipEntry entry = null;
            FileStream fs = null;
            Crc32 crc = new Crc32();

            try
            {
                ////創建當前文件夾
                //entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/")); //加上 “/” 纔會當成是文件夾創建
                //s.PutNextEntry(entry);
                //s.Flush();

                //先壓縮文件，再遞歸壓縮文件夾 
                filenames = Directory.GetFiles(FolderToZip);
                foreach (string file in filenames)
                {
                    //打開壓縮文件
                    fs = System.IO.File.OpenRead(file);

                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(file)));

                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();

                    crc.Reset();
                    crc.Update(buffer);

                    entry.Crc = crc.Value;

                    s.PutNextEntry(entry);

                    s.Write(buffer, 0, buffer.Length);
                }
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                {
                    entry = null;
                }
                GC.Collect();
                GC.Collect(1);
            }


            folders = Directory.GetDirectories(FolderToZip);
            foreach (string folder in folders)
            {
                if (!ZipFileDictory(folder, s, Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip)), APP_ID_RAN))
                {
                    return false;
                }
            }

            return res;
        }
        /// <summary>
        /// 壓縮目錄
        /// </summary>
        /// <param name="FolderToZip">待壓縮的文件夾，全路徑格式</param>
        /// <param name="ZipedFile">壓縮後的文件名，全路徑格式</param>
        /// <param name="Password"></param>
        /// <returns></returns>
        private bool ZipFileDictory(string FolderToZip, string ZipedFile, string APP_ID_RAN, String Password = "")
        {
            bool res;
            if (!Directory.Exists(FolderToZip))
            {
                return false;
            }

            ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(ZipedFile));
            s.SetLevel(6);
            //s.Password = Password;

            res = ZipFileDictory(FolderToZip, s, "", APP_ID_RAN);

            s.Finish();
            s.Close();

            return res;
        }
        /// <summary>
        /// 產壓縮檔 醫事司
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Byte[] GetDI_ZIP_A08(string APP_ID, string APP_ID_RAN, byte[] docx, List<Areas.Admin.Models.FileGroupModel> filelist)
        {
            #region 另存檔案至目錄
            var dao = new ShareDAO();
            var FileZipRoute = DownLoadPath + "DIZIP\\" + APP_ID_RAN.Replace(".", "_");
            // 判斷是否有資料夾
            this.FileDI();
            this.FileDIZIP();
            if (!Directory.Exists(FileZipRoute))
            {
                //建立DI資料夾
                Directory.CreateDirectory(FileZipRoute);
            }
            // 複製DI 檔案
            var FilePath = DownLoadPath + "DI\\" + APP_ID_RAN + ".di";
            var TargetPath = FileZipRoute;
            System.IO.File.Copy(FilePath, TargetPath + "\\" + APP_ID_RAN + ".di");
            // 複製 檔案上傳 附件
            if (filelist != null && filelist.Count > 0)
            {
                var seq = 1;
                foreach (var item in filelist)
                {
                    var dot = item.FILE_NAME.ToSplit(".").LastOrDefault();
                    var ItemsPath = dao.getApplyFileRoute(item.SRC);
                    var TargetFileFilePath = FileZipRoute + "\\" + APP_ID_RAN + "_Attach" + seq.ToString() + "." + dot;
                    if (TargetFileFilePath != null)
                    {
                        System.IO.File.Copy(ItemsPath.Replace("/", "\\"), TargetFileFilePath);
                    }
                    seq++;
                }
            }

            #endregion

            string zipFileName = APP_ID_RAN + ".zip";
            logger.Debug($"GetDI_ZIP.zipFileName:{zipFileName}");
            //string[] filenames = Directory.GetFiles(TargetPath);
            //string[] fileattachs = Directory.GetFiles(TargetPath + "\\attach");
            //logger.Debug($"GetDI_ZIP.TargetPath:{TargetPath}");
            var zipFilePath = TargetPath + "\\" + zipFileName;
            var outzip = DownLoadPath + "DIZIP";
            ZipFileDictory(TargetPath, outzip + "\\" + zipFileName, APP_ID_RAN);

            return this.GetBinaryFile(outzip + "\\" + zipFileName);
        }
    }
}