using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace ES.Models
{
    /// <summary>
    /// FTP
    /// </summary>
    public class FtpModel
    {
        /// <summary>
        /// 預設建構子
        /// </summary>
        /// <returns></returns>
        public FtpModel() { }

        /// <summary>
        /// 預設建構子
        /// </summary>
        /// <returns></returns>
        public FtpModel(string FullSubPath, string FileName, string FtpUser, string FtpPassWord)
        {
            this.FullSubPath = FullSubPath;
            this.FileName = FileName;
            this.FtpUser = FtpUser;
            this.FtpPassWord = FtpPassWord;
        }

        /// <summary>
        /// FTP路徑
        /// </summary>
        public string FullSubPath { get; set; }

        /// <summary>
        /// FTP檔案
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// FTP-帳號
        /// </summary>
        public string FtpUser { get; set; }

        /// <summary>
        /// FTP-密碼
        /// </summary>
        public string FtpPassWord { get; set; }

        /// <summary>
        /// 從ftp伺服器上獲得檔案列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetFileList()
        {
            List<string> strs = new List<string>();
            try
            {
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(FullSubPath));
                // ftp使用者名稱和密碼
                reqFTP.Credentials = new NetworkCredential(FtpUser, FtpPassWord);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream()); //中文檔名

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (!line.Contains("<DIR>"))
                    {
                        string msg = line.Substring(39).Trim();
                        strs.Add(msg);
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
                response.Close();
                return strs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("獲取檔案出錯：" + ex.Message);
            }
            return strs;
        }

    }
}