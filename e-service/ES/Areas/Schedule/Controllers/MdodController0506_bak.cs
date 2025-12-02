using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using ES.Utils;
using System.IO;
using log4net;
using ES.Controllers;
using System.Net;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using System.Text;
using ES.Areas.BACKMIN.Utils;

namespace ES.Areas.Admin.Controllers
{
    public class MdodController : BaseController
    {
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleMdodLogger");
        private static bool isLock = false;

        public ActionResult CreateXML()
        {
            logger.Debug("Client IP: " + GetClientIP());
            if (isLock)
            {
                ViewBag.Message = "程式重複執行";
                logger.Debug(ViewBag.Message);
                return View("Message");
            }

            isLock = true;

            logger.Debug("醫事簽審排程開始");
            try
            {
                filename[0] = dt.ToString("yyyyMMddHHmm") + "APP110.xml";
                filename[1] = dt.ToString("yyyyMMddHHmm") + "APP111.xml";
                filename[2] = dt.ToString("yyyyMMddHHmm") + "APP111_ADD.xml";
                filename[3] = dt.ToString("yyyyMMddHHmm") + "APP112.xml";

                List<Dictionary<string, string>> list1, list2, list3, list4;


                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MdodAction action = new MdodAction(conn);

                    list1 = action.Get001035Data(dt);
                    list1.AddRange(action.Get001034Data(dt));
                    list2 = action.Get001035Goods(dt);
                    list2.AddRange(action.Get001034Goods(dt));
                    list3 = action.Get001035Doc(dt);
                    list3.AddRange(action.Get001034Doc(dt));
                    list4 = action.Get001035File(dt);
                }

                if (list1.Count > 0 || list2.Count > 0 || list3.Count > 0)
                {
                    CreateAPP110(list1);
                    CreateAPP111(list2);
                    CreateAPP111Add(list2);
                    CreateAPP112(list3);
                }

                try
                {
                    ftp(list4);
                    using (SqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        MdodAction action = new MdodAction(conn);

                        action.UpdateFlag(list1);
                    }

                    ViewBag.Message = "XML產生完成，FTP上傳成功";
                }
                catch (Exception e)
                {
                    ViewBag.Message = "FTP傳輸失敗，錯誤訊息：" + e;
                    SendMail(e);
                    logger.Warn(ViewBag.Message, e);
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = "XML產生失敗，錯誤訊息：" + e;
                SendMail(e);
                logger.Warn(ViewBag.Message, e);
            }
            finally
            {
                logger.Debug("醫事簽審排程結束");
                isLock = false;
            }

            return View("Message");
        }

        public ActionResult CreateXML2()
        {
            if (isLock)
            {
                ViewBag.Message = "程式重複執行";
                logger.Debug(ViewBag.Message);
                return View("Message");
            }

            isLock = true;

            logger.Debug("醫事簽審排程開始");
            try
            {
                filename[0] = dt.ToString("yyyyMMddHHmm") + "APP110.xml";
                filename[1] = dt.ToString("yyyyMMddHHmm") + "APP111.xml";
                filename[2] = dt.ToString("yyyyMMddHHmm") + "APP111_ADD.xml";
                filename[3] = dt.ToString("yyyyMMddHHmm") + "APP112.xml";

                List<Dictionary<string, string>> list1, list2, list3, list4;


                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MdodAction action = new MdodAction(conn);

                    list1 = action.Get001035Data(dt);
                    list1.AddRange(action.Get001034Data(dt));
                    list2 = action.Get001035Goods(dt);
                    list2.AddRange(action.Get001034Goods(dt));
                    list3 = action.Get001035Doc(dt);
                    list3.AddRange(action.Get001034Doc(dt));
                    list4 = action.Get001035File(dt);
                }

                if (list1.Count > 0 || list2.Count > 0 || list3.Count > 0)
                {
                    CreateAPP110(list1);
                    CreateAPP111(list2);
                    CreateAPP111Add(list2);
                    CreateAPP112(list3);
                }
            }
            catch (Exception e)
            {
                ViewBag.Message = "XML產生失敗，錯誤訊息：" + e;
                SendMail(e);
                logger.Warn(ViewBag.Message, e);
            }
            finally
            {
                logger.Debug("醫事簽審排程結束");
                isLock = false;
            }

            return View("Message");
        }

        private void SendMail(Exception ex)
        {
            try
            {
                //寄管理者信箱
                string sendTo = "eservice@iscom.com.tw";
                //string sendTo = "jay@thinkon.com.tw";
                string subject = "線上申辦系統排程異常通知";
                string body = @"
                    時間："+ DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") +@"
                    <br/>訊息：" + ex.StackTrace.Replace("\r\n", "<br />") + @"
                ";

                MailUtils.SendMailNoLog(sendTo, subject, body);
            }
            catch (Exception e)
            {
                logger.Warn("發送錯誤信件失敗", e);
            }
        }

        private void CreateAPP110(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));

            // 建立根結點物件
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);

            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);

                foreach (string key in app110)
                {
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }

            if (!Directory.Exists(xmlPath))
            {
                Directory.CreateDirectory(xmlPath);
            }

            doc.Save(xmlPath + filename[0]);
        }

        private void CreateAPP111(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));

            // 建立根結點物件
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);

            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);

                foreach (string key in app111)
                {
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }

            if (!Directory.Exists(xmlPath))
            {
                Directory.CreateDirectory(xmlPath);
            }

            doc.Save(xmlPath + filename[1]);
        }

        private void CreateAPP111Add(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));

            // 建立根結點物件
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);

            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);

                foreach (string key in app111add)
                {
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }

            if (!Directory.Exists(xmlPath))
            {
                Directory.CreateDirectory(xmlPath);
            }

            doc.Save(xmlPath + filename[2]);
        }

        private void CreateAPP112(List<Dictionary<string, string>> list)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "big5", null));

            // 建立根結點物件
            XmlElement root = doc.CreateElement("RecvData");
            doc.AppendChild(root);

            foreach (Dictionary<string, string> item in list)
            {
                XmlElement rec = doc.CreateElement("REC");
                root.AppendChild(rec);

                foreach (string key in app112)
                {
                    XmlElement e = doc.CreateElement(key);
                    if (item.ContainsKey(key))
                    {
                        e.InnerText = item[key];
                    }
                    rec.AppendChild(e);
                }
            }

            if (!Directory.Exists(xmlPath))
            {
                Directory.CreateDirectory(xmlPath);
            }

            doc.Save(xmlPath + filename[3]);
        }

        private void ftp(List<Dictionary<string, string>> list)
        {
            if (System.IO.File.Exists(xmlPath + filename[0]))
            {
                UploadFile(xmlPath + filename[0], filename[0]);
            }
            if (System.IO.File.Exists(xmlPath + filename[1]))
            {
                UploadFile(xmlPath + filename[1], filename[1]);
            }
            if (System.IO.File.Exists(xmlPath + filename[2]))
            {
                UploadFile(xmlPath + filename[2], filename[2]);
            }
            if (System.IO.File.Exists(xmlPath + filename[3]))
            {
                UploadFile(xmlPath + filename[3], filename[3]);
            }

            foreach (Dictionary<string, string> item in list)
            {
                if (!String.IsNullOrEmpty(item["FILE"]))
                {
                    string file1 = DataUtils.GetConfig("FOLDER_APPLY_FILE") + item["FILE"];
                    string file2 = item["APPNO"] + "-" + item["SEQNO"] + Path.GetExtension(file1);


                    if (System.IO.File.Exists(file1))
                    {
                        UploadFile(file1, file2);
                        //wc.UploadFile(ftpServer + file2, file1);
                    }
                    else
                    {
                        logger.Warn("附件不存在：" + file1);
                    }
                }
            }
        }

        //private void UploadFile(string file1, string file2)
        //{


        //    FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create(ftpServer + file2);

        //    ftp.Method = WebRequestMethods.Ftp.UploadFile;
        //    ftp.UseBinary = true;
        //    ftp.UsePassive = false;
        //    ftp.Credentials = new NetworkCredential(ftpAcc, ftpPwd);
        //    //ftp.KeepAlive = true;
        //    ftp.Timeout = 300000;

        //    FileStream fs = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read);
        //    byte[] file = new byte[fs.Length];
        //    fs.Read(file, 0, (int)fs.Length);
        //    fs.Close();
        //    //logger.Debug("file: " + file2 + "");
        //    ftp.ContentLength = file.Length;
        //    //logger.Debug("file: " + file2 + " (2)");
        //    Stream rs = ftp.GetRequestStream();
        //    //logger.Debug("file: " + file2 + " (3)");
        //    rs.Write(file, 0, file.Length);
        //    //logger.Debug("file: " + file2 + " (4)");
        //    rs.Close();
        //    //logger.Debug("file: " + file2 + " (5)");
        //    FtpWebResponse res = (FtpWebResponse)ftp.GetResponse();
        //    logger.Debug("file: " + file2 + " / res: " + res.StatusDescription);
        //    res.Close();

        //    /*
        //    StreamReader sr = new StreamReader(file1, Encoding.GetEncoding("big5"));
        //    byte[] file = Encoding.GetEncoding("big5").GetBytes(sr.ReadToEnd());

        //    sr.Close();
        //    ftp.ContentLength = file.Length;

        //    Stream rs = ftp.GetRequestStream();
        //    rs.Write(file, 0, file.Length);
        //    rs.Close();

        //    FtpWebResponse res = (FtpWebResponse)ftp.GetResponse();
        //    logger.Debug("res: " + res.StatusDescription);
        //    res.Close();
        //    */
        //}

        private void UploadFile(string file1, string file2)//20190506 使用SFTP
        {
            SFTPUtils sftp = null;
            sftp = new SFTPUtils(this.ftpServer, "22", this.ftpAcc, this.ftpPwd);
            MdodController.logger.Debug(string.Concat("sfpt：連線成功，開始上傳文件：", file1));
            sftp.Put(file1, string.Concat("/", Path.GetFileName(file2)));
            sftp.Disconnect();
        }

        private DateTime dt = DateTime.Now;
        private string xmlPath = DataUtils.GetConfig("SCHEDULE_MDOD_XML_PATH") + DateTime.Now.Year.ToString("D4") + "\\" + DateTime.Now.Month.ToString("D2") + "\\";
        private string ftpServer = "ftp://" + DataUtils.GetConfig("SCHEDULE_MDOD_FTP_SERVER") + "/";
        private string ftpAcc = DataUtils.GetConfig("SCHEDULE_MDOD_FTP_ACCOUNT");
        private string ftpPwd = DataUtils.GetConfig("SCHEDULE_MDOD_FTP_PASSWORD");
        private string[] filename = new string[4];

        private string[] app110 = new string[] {
            "APPNO", "APPTYP", "CHECKTYP", "ORGAN", "ORGCHNNAM",
            "EnglishName", "ORGADD", "ORGEADD", "ORGNAM", "ORGTEL",
            "ORGMAIL", "ContactFaxNo", "GETTYP", "BEGDTE", "ENDDTE",
            "LINTYP", "COUCOD", "BUYCOU", "TRNPORT", "BEGPORT",
            "BUYNAM", "BUYADD", "CHGCOD", "CHGREMARK", "ConfirmType",
            "APPDTE", "APPTIME", "REMARK", "CREDTE", "CRETIME",
            "CREUSR", "MODDTE", "MODTIME", "MODUSR", "CLODTE",
            "CLOTIME", "CLOUSR", "AgentID", "AgentChineseName", "AgentEnglishName",
            "AgentChineseAddr", "AgentEnglishAddr", "AgentContactPerson", "AgentTelNo", "AgentFaxNo",
            "AgentEmail"
        };

        private string[] app111 = new string[] {
            "APPNO", "SEQNO", "ITEMNO", "APPNUM", "BRAND",
            "OTHITEMNO", "OTHORGAN", "OTHNUM"
        };

        private string[] app111add = new string[] {
            "APPNO", "SEQNO", "GOODNAME", "UNIT", "Spec",
            "AdditionalGoodsDesc", "Model", "OriginCountry"
        };

        private string[] app112 = new string[] {
            "APPNO", "SEQNO", "DOCTYP", "DOCCOD", "DOCTXT"
        };
    }
}
