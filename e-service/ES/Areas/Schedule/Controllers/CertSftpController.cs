using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Action;
using System.Data.SqlClient;
using System.IO;
using ES.Utils;
using log4net;
using System.Security.Cryptography;
using System.Text;
using ES.Areas.BACKMIN.Utils;


namespace ES.Areas.Admin.Controllers
{
    public class CertSftpController :BaseController
    {
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleCertLogger");
        private string path = DataUtils.GetConfig("CERT_PATH") + System.DateTime.Now.Year + "\\" + System.DateTime.Now.Month.ToString("00")+"\\";
        private string FileName = "CUL" + DataUtils.GetConfig("CERT_SYSTEM_ID") + "-" + System.DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
        //
        // GET: /Schedule/CertSftp/

        public ActionResult Run()
        {
            try
            {
                //產生憑證應用紀錄檔
                CreateFile();
                //上傳SFTP
                ftp();
            }catch (Exception ex)
            {
                logger.Debug("憑證上傳排程失敗："+ex.ToString());
            }

            return View("Message");
        }

        public void CreateFile()
        {
            //string FileName = "CUL" + DataUtils.GetConfig("CERT_SYSTEM_ID") + "-" + System.DateTime.Now.ToString("yyyyMMdd");
            string strTxt = "";
            List<Dictionary<string, string>> list;            
            try
            {
                logger.Debug("產生憑證應用紀錄檔開始");
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    CertSftpAction action = new CertSftpAction(conn);
                    list = action.GetCertUploadData();
                    conn.Close();
                    conn.Dispose();
                }

                foreach (Dictionary<string, string> item in list)
                {
                    strTxt += "MOEACA\t";
                    strTxt += item.FirstOrDefault(r => r.Key == "CERT_SN").Value + "\t";
                    strTxt += item.FirstOrDefault(r => r.Key == "CARD_IDX").Value + "\t";
                    strTxt += item.FirstOrDefault(r => r.Key == "ADD_TIME").Value + "\t";
                    strTxt += DataUtils.GetConfig("CERT_SYSTEM_ID") + "\t";
                    strTxt += "00\r\n";
                }

                //檢查資料夾是否存在
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //寫檔案
                using (StreamWriter sw = new StreamWriter(path + FileName + ".txt"))
                {
                    sw.Write(strTxt);
                }

                logger.Debug("產生憑證應用紀錄檔結束");

                //產生sha1碼認證檔
                CreateSha1(FileName,strTxt);
               
            }
            catch (Exception ex)
            {
                logger.Debug("產生憑證應用紀錄檔失敗：" + ex.ToString());
                throw ex;
            }

            
        }

        public void CreateSha1(string FileName,string strTxt)
        {
            try
            {
                logger.Debug("產生SHA1碼認證檔-開始");
                string mySha1 = "";
                //建立一個SHA1
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] txtData = Encoding.Default.GetBytes(strTxt);
                byte[] myHash = sha1.ComputeHash(txtData);
                StringBuilder NewHashCode = new StringBuilder(myHash.Length);
                //轉換成加密的Code
                foreach (byte AddByte in myHash)
                {
                    NewHashCode.AppendFormat("{0:X2}", AddByte);
                }
                mySha1 = NewHashCode.ToString();

                //寫檔案
                using (StreamWriter sw = new StreamWriter(path + FileName + ".sha1"))
                {
                    sw.Write(mySha1);
                }
                logger.Debug("產生SHA1碼認證檔-結束");
            }
            catch (Exception ex)
            {
                logger.Debug("產生SHA1碼認證檔-失敗");
                throw ex;
            }
        }

        public void ftp()
        {
            try
            {
                SFTPUtils sftp = null;
               
                logger.Debug("憑證上傳排程開始");                
                //上傳至sftp                    
                sftp = new SFTPUtils(DataUtils.GetConfig("CERT_SFTP_IP"), DataUtils.GetConfig("CERT_SFTP_PORT"), DataUtils.GetConfig("CERT_SFTP_ACC"), DataUtils.GetConfig("CERT_SFTP_PWD"));
                logger.Debug("sfpt：連線成功，開始上傳文件：" + FileName + ".txt");
                sftp.Put(path + FileName + ".txt", "/" + FileName + ".txt");
                logger.Debug("上傳文件：" + FileName + ".sha1");
                sftp.Put(path + FileName + ".sha1", "/" + FileName + ".sha1");
                sftp.Disconnect();
                logger.Debug("憑證上傳排程結束");

            }catch (Exception ex)
            {
                logger.Debug("憑證上傳排程錯誤："+ex.ToString());
            }
        }

    }
}
