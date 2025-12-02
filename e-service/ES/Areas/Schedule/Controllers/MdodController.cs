using ES.Areas.Admin.Action;
using ES.Areas.BACKMIN.Utils;
using ES.Controllers;
using ES.Utils;
using log4net;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using System.Web.Mvc;
using System.Xml;

namespace ES.Areas.Admin.Controllers
{
    public class MdodController : BaseNoMemberController
    {
        //private readonly static ILog logger;
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleMdodLogger");

        private static bool isLock;

        private DateTime dt = DateTime.Now;

        private string xmlPath;

        private string ftpServer;

        private string ftpAcc;

        private string ftpPwd;

        private string[] filename;

        private string[] app110;

        private string[] app111;

        private string[] app111add;

        private string[] app112;

        static MdodController()
        {
            MdodController.logger = LogUtils.GetLogger("ScheduleMdodLogger");
            MdodController.isLock = false;
        }

        public MdodController()
        {
            this.dt = DateTime.Now;
            string[] config = new string[] { DataUtils.GetConfig("SCHEDULE_MDOD_XML_PATH"), null, null, null, null };
            int year = DateTime.Now.Year;
            config[1] = year.ToString("D4");
            config[2] = "\\";
            year = DateTime.Now.Month;
            config[3] = year.ToString("D2");
            config[4] = "\\";
            this.xmlPath = string.Concat(config);
            this.ftpServer = DataUtils.GetConfig("SCHEDULE_MDOD_FTP_SERVER");
            this.ftpAcc = DataUtils.GetConfig("SCHEDULE_MDOD_FTP_ACCOUNT");
            this.ftpPwd = DataUtils.GetConfig("SCHEDULE_MDOD_FTP_PASSWORD");
            this.filename = new string[4];
            config = new string[] { "APPNO", "APPTYP", "CHECKTYP", "ORGAN", "ORGCHNNAM", "EnglishName"
                , "ORGADD", "ORGEADD", "ORGNAM", "ORGTEL", "ORGMAIL", "ContactFaxNo", "GETTYP"
                , "BEGDTE", "ENDDTE", "LINTYP", "COUCOD", "BUYCOU", "TRNPORT", "BEGPORT", "BUYNAM"
                , "BUYADD", "CHGCOD", "CHGREMARK", "ConfirmType", "APPDTE", "APPTIME", "REMARK"
                , "CREDTE", "CRETIME", "CREUSR", "MODDTE", "MODTIME", "MODUSR", "CLODTE", "CLOTIME"
                , "CLOUSR", "AgentID", "AgentChineseName", "AgentEnglishName", "AgentChineseAddr"
                , "AgentEnglishAddr", "AgentContactPerson", "AgentTelNo", "AgentFaxNo", "AgentEmail" };
            this.app110 = config;
            config = new string[] { "APPNO", "SEQNO", "ITEMNO", "APPNUM", "BRAND", "OTHITEMNO", "OTHORGAN", "OTHNUM" };
            this.app111 = config;
            config = new string[] { "APPNO", "SEQNO", "GOODNAME", "UNIT", "Spec", "AdditionalGoodsDesc", "Model", "OriginCountry" };
            this.app111add = config;
            config = new string[] { "APPNO", "SEQNO", "DOCTYP", "DOCCOD", "DOCTXT" };
            this.app112 = config;
            //Base();
        }

        private void CreateAPP110(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);
            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);
                string[] strArrays = this.app110;
                for (int i = 0; i < (int)strArrays.Length; i++)
                {
                    string key = strArrays[i];
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }
            if (!Directory.Exists(this.xmlPath))
            {
                Directory.CreateDirectory(this.xmlPath);
            }
            doc.Save(string.Concat(this.xmlPath, this.filename[0]));
        }

        private void CreateAPP111(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);
            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);
                string[] strArrays = this.app111;
                for (int i = 0; i < (int)strArrays.Length; i++)
                {
                    string key = strArrays[i];
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }
            if (!Directory.Exists(this.xmlPath))
            {
                Directory.CreateDirectory(this.xmlPath);
            }
            doc.Save(string.Concat(this.xmlPath, this.filename[1]));
        }

        private void CreateAPP111Add(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);
            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);
                string[] strArrays = this.app111add;
                for (int i = 0; i < (int)strArrays.Length; i++)
                {
                    string key = strArrays[i];
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }
            if (!Directory.Exists(this.xmlPath))
            {
                Directory.CreateDirectory(this.xmlPath);
            }
            doc.Save(string.Concat(this.xmlPath, this.filename[2]));
        }

        private void CreateAPP112(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);
            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);
                string[] strArrays = this.app112;
                for (int i = 0; i < (int)strArrays.Length; i++)
                {
                    string key = strArrays[i];
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }
            if (!Directory.Exists(this.xmlPath))
            {
                Directory.CreateDirectory(this.xmlPath);
            }
            doc.Save(string.Concat(this.xmlPath, this.filename[3]));
        }

        public ActionResult CreateXML()
        {
            List<Dictionary<string, string>> list1 = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> list2 = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> list3 = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> list4 = new List<Dictionary<string, string>>();
            MdodAction action;
            Exception e;
            ActionResult actionResult;
            MdodController.logger.Debug(string.Concat("Client IP: ", base.GetClientIP()));
            if (!MdodController.isLock)
            {
                MdodController.isLock = true;
                MdodController.logger.Debug("醫事簽審排程開始");
                try
                {
                    try
                    {
                        this.filename[0] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP110.xml");
                        this.filename[1] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP111.xml");
                        this.filename[2] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP111_ADD.xml");
                        this.filename[3] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP112.xml");
                        SqlConnection conn = base.GetConnection();
                        try
                        {
                            conn.Open();
                            action = new MdodAction(conn);
                            list1 = action.Get001035Data(this.dt);
                            list1.AddRange(action.Get001038Data(this.dt));
                            list1.AddRange(action.Get001034Data(this.dt));
                            list2 = action.Get001035Goods(this.dt);
                            list2.AddRange(action.Get001038Goods(this.dt));
                            list2.AddRange(action.Get001034Goods(this.dt));
                            list3 = action.Get001035Doc(this.dt);
                            list3.AddRange(action.Get001038Doc(this.dt));
                            list3.AddRange(action.Get001034Doc(this.dt));
                            list4 = action.Get001035File(this.dt);
                        }
                        finally
                        {
                            if (conn != null)
                            {
                                ((IDisposable)conn).Dispose();
                            }
                        }
                        if ((list1.Count > 0 || list2.Count > 0 ? true : list3.Count > 0))
                        {
                            this.CreateAPP110(list1);
                            this.CreateAPP111(list2);
                            this.CreateAPP111Add(list2);
                            this.CreateAPP112(list3);
                        }
                        try
                        {
                            this.ftp(list4);
                            conn = base.GetConnection();
                            try
                            {
                                conn.Open();
                                action = new MdodAction(conn);
                                action.UpdateFlag(list1);
                            }
                            finally
                            {
                                if (conn != null)
                                {
                                    ((IDisposable)conn).Dispose();
                                }
                            }
                            ((dynamic)base.ViewBag).Message = "XML產生完成，FTP上傳成功";
                        }
                        catch (Exception exception)
                        {
                            e = exception;
                            ((dynamic)base.ViewBag).Message = string.Concat("FTP傳輸失敗，錯誤訊息：", e);
                            this.SendMail(e);
                            MdodController.logger.Warn(((dynamic)base.ViewBag).Message, e);
                        }
                    }
                    catch (Exception exception1)
                    {
                        e = exception1;
                        ((dynamic)base.ViewBag).Message = string.Concat("XML產生失敗，錯誤訊息：", e);
                        this.SendMail(e);
                        MdodController.logger.Warn(((dynamic)base.ViewBag).Message, e);
                    }
                }
                finally
                {
                    MdodController.logger.Debug("醫事簽審排程結束");
                    MdodController.isLock = false;
                }
                actionResult = base.View("Message");
            }
            else
            {
                ((dynamic)base.ViewBag).Message = "程式重複執行";
                MdodController.logger.Debug(((dynamic)base.ViewBag).Message);
                actionResult = base.View("Message");
            }
            return actionResult;
        }

        public ActionResult CreateXML2()
        {
            List<Dictionary<string, string>> list1;
            List<Dictionary<string, string>> list2;
            List<Dictionary<string, string>> list3;
            ActionResult actionResult;
            if (!MdodController.isLock)
            {
                MdodController.isLock = true;
                MdodController.logger.Debug("醫事簽審排程開始");
                try
                {
                    try
                    {
                        this.filename[0] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP110.xml");
                        this.filename[1] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP111.xml");
                        this.filename[2] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP111_ADD.xml");
                        this.filename[3] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP112.xml");
                        SqlConnection conn = base.GetConnection();
                        try
                        {
                            conn.Open();
                            MdodAction action = new MdodAction(conn);
                            list1 = action.Get001035Data(this.dt);
                            list1.AddRange(action.Get001038Data(this.dt));
                            list1.AddRange(action.Get001034Data(this.dt));
                            list2 = action.Get001035Goods(this.dt);
                            list2.AddRange(action.Get001038Goods(this.dt));
                            list2.AddRange(action.Get001034Goods(this.dt));
                            list3 = action.Get001035Doc(this.dt);
                            list3.AddRange(action.Get001038Doc(this.dt));
                            list3.AddRange(action.Get001034Doc(this.dt));
                            action.Get001035File(this.dt);
                        }
                        finally
                        {
                            if (conn != null)
                            {
                                ((IDisposable)conn).Dispose();
                            }
                        }
                        if ((list1.Count > 0 || list2.Count > 0 ? true : list3.Count > 0))
                        {
                            this.CreateAPP110(list1);
                            this.CreateAPP111(list2);
                            this.CreateAPP111Add(list2);
                            this.CreateAPP112(list3);
                        }
                    }
                    catch (Exception exception)
                    {
                        Exception e = exception;
                        ((dynamic)base.ViewBag).Message = string.Concat("XML產生失敗，錯誤訊息：", e);
                        this.SendMail(e);
                        MdodController.logger.Warn(((dynamic)base.ViewBag).Message, e);
                    }
                }
                finally
                {
                    MdodController.logger.Debug("醫事簽審排程結束");
                    MdodController.isLock = false;
                }
                actionResult = base.View("Message");
            }
            else
            {
                ((dynamic)base.ViewBag).Message = "程式重複執行";
                MdodController.logger.Debug(((dynamic)base.ViewBag).Message);
                actionResult = base.View("Message");
            }
            return actionResult;
        }
        /// <summary>
        /// 不上傳實體檔案到sftp
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateXML3()
        {
            List<Dictionary<string, string>> list1 = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> list2 = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> list3 = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> list4 = new List<Dictionary<string, string>>();
            MdodAction action;
            Exception e;
            ActionResult actionResult;
            MdodController.logger.Debug(string.Concat("Client IP: ", base.GetClientIP()));
            if (!MdodController.isLock)
            {
                MdodController.isLock = true;
                MdodController.logger.Debug("醫事簽審排程開始");
                try
                {
                    try
                    {
                        this.filename[0] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP110.xml");
                        this.filename[1] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP111.xml");
                        this.filename[2] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP111_ADD.xml");
                        this.filename[3] = string.Concat(this.dt.ToString("yyyyMMddHHmm"), "APP112.xml");
                        SqlConnection conn = base.GetConnection();
                        try
                        {
                            conn.Open();
                            action = new MdodAction(conn);
                            list1 = action.Get001035Data(this.dt);
                            list1.AddRange(action.Get001038Data(this.dt));
                            list1.AddRange(action.Get001034Data(this.dt));
                            list2 = action.Get001035Goods(this.dt);
                            list2.AddRange(action.Get001038Goods(this.dt));
                            list2.AddRange(action.Get001034Goods(this.dt));
                            list3 = action.Get001035Doc(this.dt);
                            list3.AddRange(action.Get001038Doc(this.dt));
                            list3.AddRange(action.Get001034Doc(this.dt));
                            list4 = action.Get001035File(this.dt);
                        }
                        finally
                        {
                            if (conn != null)
                            {
                                ((IDisposable)conn).Dispose();
                            }
                        }
                        if ((list1.Count > 0 || list2.Count > 0 ? true : list3.Count > 0))
                        {
                            this.CreateAPP110(list1);
                            this.CreateAPP111(list2);
                            this.CreateAPP111Add(list2);
                            this.CreateAPP112(list3);
                        }
                        try
                        {
                            this.ftp(list4, "N");
                            conn = base.GetConnection();
                            try
                            {
                                conn.Open();
                                action = new MdodAction(conn);
                                action.UpdateFlag(list1);
                            }
                            finally
                            {
                                if (conn != null)
                                {
                                    ((IDisposable)conn).Dispose();
                                }
                            }
                            ((dynamic)base.ViewBag).Message = "XML產生完成，FTP上傳成功";
                        }
                        catch (Exception exception)
                        {
                            e = exception;
                            ((dynamic)base.ViewBag).Message = string.Concat("FTP傳輸失敗，錯誤訊息：", e);
                            this.SendMail(e);
                            MdodController.logger.Warn(((dynamic)base.ViewBag).Message, e);
                        }
                    }
                    catch (Exception exception1)
                    {
                        e = exception1;
                        ((dynamic)base.ViewBag).Message = string.Concat("XML產生失敗，錯誤訊息：", e);
                        this.SendMail(e);
                        MdodController.logger.Warn(((dynamic)base.ViewBag).Message, e);
                    }
                }
                finally
                {
                    MdodController.logger.Debug("醫事簽審排程結束");
                    MdodController.isLock = false;
                }
                actionResult = base.View("Message");
            }
            else
            {
                ((dynamic)base.ViewBag).Message = "程式重複執行";
                MdodController.logger.Debug(((dynamic)base.ViewBag).Message);
                actionResult = base.View("Message");
            }
            return actionResult;
        }
        private void ftp(List<Dictionary<string, string>> list, string isFileUpload = "Y")
        {
            List<string> file1s = new List<string>();
            List<string> file2s = new List<string>();
            logger.Debug($"MdodController_ftp_listCount:{list?.Count}");
            logger.Debug($"MdodController_xmlPath:{this.xmlPath},filename[0]:{this.filename[0]},filename[1]:{this.filename[1]},filename[2]:{this.filename[2]},filename[3]:{this.filename[3]}");
            if (System.IO.File.Exists(string.Concat(this.xmlPath, this.filename[0])))
            {
                logger.Debug($"MdodController_thisUploadFile_0:{string.Concat(this.xmlPath, this.filename[0])},{this.filename[0]}");
                //this.UploadFile(string.Concat(this.xmlPath, this.filename[0]), this.filename[0]);
                file1s.Add(string.Concat(this.xmlPath, this.filename[0]));
                file2s.Add(this.filename[0]);
            }
            if (System.IO.File.Exists(string.Concat(this.xmlPath, this.filename[1])))
            {
                logger.Debug($"MdodController_thisUploadFile_1:{string.Concat(this.xmlPath, this.filename[1])},{this.filename[1]}");
                //this.UploadFile(string.Concat(this.xmlPath, this.filename[1]), this.filename[1]);
                file1s.Add(string.Concat(this.xmlPath, this.filename[1]));
                file2s.Add(this.filename[1]);
            }
            if (System.IO.File.Exists(string.Concat(this.xmlPath, this.filename[2])))
            {
                logger.Debug($"MdodController_thisUploadFile_2:{string.Concat(this.xmlPath, this.filename[2])},{this.filename[2]}");
                //this.UploadFile(string.Concat(this.xmlPath, this.filename[2]), this.filename[2]);
                file1s.Add(string.Concat(this.xmlPath, this.filename[2]));
                file2s.Add(this.filename[2]);
            }
            if (System.IO.File.Exists(string.Concat(this.xmlPath, this.filename[3])))
            {
                logger.Debug($"MdodController_thisUploadFile_3:{string.Concat(this.xmlPath, this.filename[3])},{this.filename[3]}");
                //this.UploadFile(string.Concat(this.xmlPath, this.filename[3]), this.filename[3]);
                file1s.Add(string.Concat(this.xmlPath, this.filename[3]));
                file2s.Add(this.filename[3]);
            }
            if (isFileUpload == "Y")
            {
                logger.Debug("MdodController_foreach_Dictionary<string, string> item in list");
                foreach (Dictionary<string, string> item in list)
                {
                    if (!string.IsNullOrEmpty(item["FILE"]))
                    {
                        logger.Debug($"MdodController_item[FILE]:{item["FILE"]}");
                        string file1 = string.Concat(DataUtils.GetConfig("FOLDER_APPLY_FILE"), item["FILE"]);
                        logger.Debug($"MdodController_file1{file1}");
                        string file2 = string.Concat(item["APPNO"], "-", item["SEQNO"], Path.GetExtension(file1));
                        logger.Debug($"MdodController_file2{file2}");
                        if (!System.IO.File.Exists(file1))
                        {
                            MdodController.logger.Warn(string.Concat("附件不存在：", file1));
                        }
                        else
                        {
                            //logger.Debug($"MdodController_thisUploadFile:file1,file2");
                            //this.UploadFile(file1, file2);
                            file1s.Add(file1);
                            file2s.Add(file2);
                        }
                    }
                }
            }
            if (file1s != null && file1s.Count > 0 && file2s != null && file2s.Count > 0)
            {
                this.UploadFile(file1s, file2s);
            }
        }

        private void SendMail(Exception ex)
        {
            try
            {
                //產生錯誤訊息
                string ErrMsg = ex.Message + "\r\n" + ex.StackTrace;
                ErrMsg = ErrMsg.Replace("\r\n", "<br />");
                string sendTo = DataUtils.GetConfig("ESAPI_SendMail");
                if (string.IsNullOrEmpty(sendTo))
                {
                    sendTo = "eservice@turbotech.com.tw";
                }
                string subject = "線上申辦系統排程異常通知";
                string[] str = new string[] { "\r\n                    時間：", null, null, null, null };
                str[1] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                str[2] = "\r\n                    <br/>訊息：";
                str[3] = ErrMsg;
                str[4] = "\r\n                ";
                string body = string.Concat(str);
                foreach(var item in sendTo.Split(','))
                {
                    MailUtils.SendMailNoLog(item, subject, body);
                }
            }
            catch (Exception exception)
            {
                MdodController.logger.Warn("發送錯誤信件失敗", exception);
            }
        }

        private void UploadFile(string file1, string file2)
        {
            SFTPUtils sftp = null;
            sftp = new SFTPUtils(this.ftpServer, "22", this.ftpAcc, this.ftpPwd);
            MdodController.logger.Debug(string.Concat("sftp：連線成功，開始上傳文件：", file1));
            sftp.Put(file1, string.Concat("/", Path.GetFileName(file2)));
            sftp.Disconnect();
        }

        private SFTPUtils ConnSftp()
        {
            SFTPUtils sftp = null;
            sftp = new SFTPUtils(this.ftpServer, "22", this.ftpAcc, this.ftpPwd);
            MdodController.logger.Debug("sftp：連線成功");
            return sftp;
        }
        private void UploadFile(List<string> file1, List<string> file2)
        {
            // 連線次數
            var sche = file1.Count / 10;

            for (var i = 0; i < (sche + 1); i++)
            {
                SFTPUtils sftp = this.ConnSftp();
                for (var j = 0; j < 10; j++)
                {
                    var num = j + (10 * i);
                    if (num < file1.Count)
                    {
                        MdodController.logger.Debug(string.Concat("開始上傳文件：", file1[num]));
                        sftp.Put(file1[num], string.Concat("/", Path.GetFileName(file2[num])));
                        MdodController.logger.Debug(string.Concat("sftp：上傳成功:", file1[num], ",檔案名稱:", file2[num]));
                    }
                    else
                    {
                        // 超出陣列
                        MdodController.logger.Debug("結束傳送。");
                        break;
                    }
                }
                this.DisSftp(sftp);
            }
        }
        private void DisSftp(SFTPUtils sftp)
        {
            sftp.Disconnect();
            MdodController.logger.Debug("sftp：斷線成功");
        }
    }
}