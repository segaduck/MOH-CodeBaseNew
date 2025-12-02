using ES.Utils;
using log4net;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ES.Areas.BACKMIN.Utils
{
    public class SFTPUtils
    {
        private SftpClient sftp;

        protected static readonly ILog logger = ES.Utils.LogUtils.GetLogger();
        /// <summary>
        /// SFTP連線狀態
        /// </summary>
        public bool Connected
        {
            get
            {
                return this.sftp.IsConnected;
            }
        }

        static SFTPUtils()
        {
            SFTPUtils.logger = LogUtils.GetLogger();
        }

        /// <summary>
        /// SFTP工具
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        public SFTPUtils(string ip, string port, string user, string pwd)
        {
            ServicePointManager.SecurityProtocol =
                            SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                            SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            this.sftp = new SftpClient(ip, int.Parse(port), user, pwd);
        }
        /// <summary>
        /// 開啟SFTP連線
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            bool flag = true;
            if (this.Connected) { return flag; }
            int max = 3;
            for (int i = 1; i <= max; i++)
            {
                try
                {
                    this.sftp.Connect();
                    break;
                }
                catch (Exception exception)
                {
                    //Exception e = ex;
                    SFTPUtils.logger.Warn(string.Format("SFTP連線失敗第{0}次，錯誤訊息：{1}", i, exception.Message));
                    if (i >= max) { throw new Exception(string.Format("SFTP連線失敗，錯誤訊息：{0}", exception.Message)); }
                }
            }
            return flag;
        }
        /// <summary>
        /// 刪除遠端檔案
        /// </summary>
        /// <param name="remoteFile"></param>
        public void Delete(string remoteFile)
        {
            try
            {
                this.Connect();
                this.sftp.Delete(remoteFile);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("刪除遠端檔案失敗，錯誤訊息：{0}", exception.Message));
            }
        }
        /// <summary>
        /// 中斷SFTP連線
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if ((this.sftp == null ? false : this.Connected))
                {
                    this.sftp.Disconnect();
                }
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("中斷SFTP連線失敗，錯誤訊息：{0}", exception.Message));
            }
        }
        /// <summary>
        /// 取得遠端檔案
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="localPath"></param>
        public void Get(string remotePath, string localPath)
        {
            try
            {
                this.Connect();
                File.WriteAllBytes(localPath, this.sftp.ReadAllBytes(remotePath));
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("取得遠端檔案失敗，錯誤訊息：{0}", exception.Message));
            }
        }
        /// <summary>
        /// 取得遠端檔案列表
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="fileSuffix"></param>
        /// <returns></returns>
        public List<string> GetFileList(string remotePath)
        {
            List<string> strs;
            try
            {
                this.Connect();
                IEnumerable<SftpFile> files = this.sftp.ListDirectory(remotePath, null);
                List<string> list = new List<string>();
                foreach (SftpFile file in files)
                {
                    list.Add(file.Name);
                }
                strs = list;
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("取得遠端檔案列表失敗，錯誤訊息：{0}", exception.Message));
            }
            return strs;
        }
        /// <summary>
        /// 取得遠端檔案列表
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="fileSuffix"></param>
        /// <returns></returns>
        public List<string> GetFileList(string remotePath, string fileSuffix)
        {
            List<string> strs;
            try
            {
                this.Connect();
                IEnumerable<SftpFile> files = this.sftp.ListDirectory(remotePath, null);
                List<string> list = new List<string>();
                foreach (SftpFile file in files)
                {
                    string name = file.Name;
                    if ((name.Length <= fileSuffix.Length + 1 ? false : fileSuffix == name.Substring(name.Length - fileSuffix.Length)))
                    {
                        list.Add(name);
                    }
                }
                strs = list;
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("取得遠端檔案列表失敗，錯誤訊息：{0}", exception.Message));
            }
            return strs;
        }
        /// <summary>
        /// 移動遠端檔案
        /// </summary>
        /// <param name="oldRemotePath"></param>
        /// <param name="newRemotePath"></param>
        public void Move(string oldRemotePath, string newRemotePath)
        {
            try
            {
                this.Connect();
                this.sftp.RenameFile(oldRemotePath, newRemotePath);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("移動遠端檔案失敗，錯誤訊息：{0}", exception.Message));
            }
        }
        /// <summary>
        /// 上傳本地檔案
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="remotePath"></param>
        public void Put(string localPath, string remotePath)
        {
            try
            {
                FileStream file = File.OpenRead(localPath);
                try
                {
                    this.Connect();
                    this.sftp.UploadFile(file, remotePath, null);
                }
                finally
                {
                    if (file != null)
                    {
                        ((IDisposable)file).Dispose();
                    }
                }
            }
            catch (Exception exception)
            {
                string s_log1 = "\n##Put";
                s_log1 += string.Format("\n ex.Message: {0}", exception.Message);
                s_log1 += string.Format("\n localPath: {0}", localPath);
                s_log1 += string.Format("\n remotePath: {0}", remotePath);
                logger.Error(s_log1, exception);
                throw new Exception(string.Format("上傳本地檔案失敗，錯誤訊息：{0}", exception.Message));
            }
        }
    }
}