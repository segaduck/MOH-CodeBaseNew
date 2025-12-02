using ES.Action.Form;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Areas.BACKMIN.Utils;
using ES.Controllers;
using ES.Utils;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using WebUI.CustomClass;

namespace ES.Areas.Admin.Controllers
{
    public class FlyPaySftpController : BaseNoMemberController
    {
        //
        // GET: /Schedule/FlyPaySftp/

        public ActionResult Run()
        {
            List<string> fileNameList = DownFileName();
            List<string> fileDownLoadList = new List<string>();
            try
            {
                foreach (var filename in fileNameList)
                {
                    //確認是否已匯入過該航班資料
                    var file = this.CheckFlyPayFile(filename);
                    if (string.IsNullOrEmpty(file))
                    {
                        fileDownLoadList.Add(filename);
                    }
                }

                // 下載檔案
                if (fileDownLoadList != null && fileDownLoadList.Count > 0)
                {
                    DownFile(fileDownLoadList);
                }

                foreach (var download in fileDownLoadList)
                {
                    // 寫回FLYPAY_FILE表
                    var fileurl = this.InsertFlyPayFile(download);
                    // 寫入航班資料
                    var success = this.InsertFlyPayExcels(fileurl);
                    // 刪除SFTP上的檔案
                    if (success) { this.DeleteSFTPfile(download); }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                ViewBag.Message = "發生錯誤，請洽系統管理員。";
            }
            ViewBag.Message = "下載完成。";
            return Content("FLYPAYSFTP:" + ViewBag.Message);
        }

        /// <summary>
        /// 取得資料夾所有檔名
        /// </summary>
        /// <returns></returns>
        public List<string> DownFileName()
        {
            List<string> fileNameList = new List<string>();

            try
            {
                //實務應用時，帳號密碼應加密儲存於設定檔
                string host = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_SERVER");
                string username = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_ACCOUNT");
                string password = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_PASSWORD");
                var port = 22;
                string remoteDirectory = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_FILEURL");
                string localDirectory = DataUtils.GetConfig("SCHEDULE_MOHW_DOWNLOAD_PATH");

                using (var sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();
                    var files = sftp.ListDirectory(remoteDirectory);
                    foreach (var file in files)
                    {
                        string remoteFileName = file.Name;
                        if (!file.Name.StartsWith(".")/* && file.LastWriteTime.Date == DateTime.Today*/)
                        {
                            fileNameList.Add(remoteFileName);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ViewBag.Message = exception.Message;
                logger.Error(exception.Message, exception);
            }

            return fileNameList;
        }
        /// <summary>
        /// 下載符合資格的檔案
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        public List<string> DownFile(List<string> fileNameList)
        {
            try
            {
                //實務應用時，帳號密碼應加密儲存於設定檔
                string host = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_SERVER");
                string username = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_ACCOUNT");
                string password = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_PASSWORD");
                var port = 22;
                string remoteDirectory = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_FILEURL");
                string localDirectory = DataUtils.GetConfig("SCHEDULE_MOHW_DOWNLOAD_PATH");

                using (var sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();
                    var files = sftp.ListDirectory(remoteDirectory);

                    foreach (var file in files)
                    {
                        string remoteFileName = file.Name;
                        if (!file.Name.StartsWith(".") && fileNameList.Contains(remoteFileName)/* && file.LastWriteTime.Date == DateTime.Today*/)
                        {
                            using (Stream file1 = System.IO.File.Create(localDirectory + remoteFileName))
                            {
                                // 下載檔案
                                sftp.DownloadFile(remoteDirectory + remoteFileName, file1);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ViewBag.Message = exception.Message;
                logger.Error(exception.Message, exception);
            }

            return fileNameList;
        }

        /// <summary>
        /// 刪除Sftp上已匯入的檔案
        /// </summary>
        /// <param name="fileNameList"></param>
        /// <returns></returns>
        public void DeleteSFTPfile(string fileName)
        {
            string err_msg = "";
            try
            {
                //實務應用時，帳號密碼應加密儲存於設定檔
                string host = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_SERVER");
                string username = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_ACCOUNT");
                string password = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_PASSWORD");
                err_msg += string.Format("\n.{0}={1}", "host", host);
                err_msg += string.Format("\n.{0}={1}", "username", username);
                err_msg += string.Format("\n.{0}={1}", "password", password);
                var port = 22;
                string remoteDirectory = DataUtils.GetConfig("SCHEDULE_MOHW_FTP_FILEURL");
                err_msg += string.Format("\n.{0}={1}", "remoteDirectory", remoteDirectory);
                err_msg += string.Format("\n.{0}={1}", "fileName", fileName);
                using (SftpClient sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();
                    sftp.DeleteFile(remoteDirectory + fileName);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(err_msg, ex);
                ViewBag.Message = ex.Message;
                logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// 確認是否已下載過該檔
        /// </summary>
        /// <param name="remoteFileName"></param>
        /// <returns></returns>
        public string CheckFlyPayFile(string remoteFileName)
        {
            var filename = string.Empty;
            //確認是否已匯入過該航班資料
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                FlyPayAction action = new FlyPayAction(conn);
                filename = action.CheckFlyPayFile(remoteFileName);
                conn.Close();
                conn.Dispose();
            }
            return filename;
        }

        /// <summary>
        /// 寫回FLYPAY_FILE表
        /// </summary>
        /// <param name="remoteFileName"></param>
        public string InsertFlyPayFile(string remoteFileName)
        {
            var fileurl = string.Empty;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                FlyPayAction action = new FlyPayAction(conn, tran);
                fileurl = action.InsertFlyPayFile(remoteFileName);
                tran.Commit();
                conn.Close();
                conn.Dispose();
            }
            return fileurl;
        }
        /// <summary>
        /// 寫入航班資料
        /// </summary>
        public bool InsertFlyPayExcels(string url)
        {
            var Success = false;
            // 實體檔案路逕
            try
            {
                List<Map> list = null;
                FileStream fsSource = new FileStream(url, FileMode.Open, FileAccess.Read);
                // xlsx 解密
                NPOI.XSSF.UserModel.XSSFWorkbook wb = null;
                NPOI.Util.FileInputStream in1 = null;

                in1 = new NPOI.Util.FileInputStream(fsSource);
                NPOI.POIFS.FileSystem.POIFSFileSystem poifsFileSystem = new NPOI.POIFS.FileSystem.POIFSFileSystem(in1);
                NPOI.POIFS.Crypt.EncryptionInfo encInfo = new NPOI.POIFS.Crypt.EncryptionInfo(poifsFileSystem);
                NPOI.POIFS.Crypt.Decryptor decryptor = NPOI.POIFS.Crypt.Decryptor.GetInstance(encInfo);
                decryptor.VerifyPassword("1922");
                wb = new NPOI.XSSF.UserModel.XSSFWorkbook(decryptor.GetDataStream(poifsFileSystem));

                list = FlyPayUtils.readEXCEL(wb);

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    FlyPayAction action = new FlyPayAction(conn, tran);
                    var InsertList = action.GetInsertDataList(list);
                    int count = action.InsertFile(InsertList, GetAccount());

                    tran.Commit();
                    Success = true;
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                ViewBag.tempMessage = ex.Message;
            }
            return Success;
        }

        public ActionResult GetEsunData()
        {
            ActionResult actionResult;
            Dictionary<string, string> list = new Dictionary<string, string>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                FlyPayAction action = new FlyPayAction(conn);
                var datas = action.GetEsunPayData();
                if (datas != null && datas.Count > 0)
                {
                    // 未完成
                }
                conn.Close();
                conn.Dispose();
            }
            actionResult = base.View("Message");
            return actionResult;
        }
    }
}


