using ES.Action.Form;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Areas.BACKMIN.Utils;
using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace ES.Areas.Admin.Controllers
{
    // 以文件為主 此程式不可參考
    public class DocSftpController : BaseController
    {
        //
        // GET: /Schedule/DocSftp/

        public ActionResult Run()
        {
            ViewBag.Message = Export();
            return View("Message");
        }

        public string Export()
        {
            DocumentExportAction DEAction = new DocumentExportAction();
            List<DocumentExportModel.DocumentModel> model = DEAction.GetDocumentData();//取得尚未匯出的公文申請資料
            DocumentUtils du = new DocumentUtils();
            int FileSn = DEAction.GetDocumentId();
            string FileName = "", Attach1Name = "", Attach2Name = "";
            StringBuilder sb = new StringBuilder();
            int Count = 1;
            int ResultCnt = 0;
            string AppNo = "", DocumentNo = "";
            string SuccessDocNo = "";
            string Message = "";

            //判斷資料夾是否存在
            if (!Directory.Exists(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH")))
            {
                //資料夾不存在
                Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"));
                //建立DI資料夾
                Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            }
            else
            {
                try
                {
                    Directory.Delete(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"), true);
                }
                catch { }

                Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"));
                //建立DI資料夾
                Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            }

            foreach (var item in model)
            {
                FileSn = FileSn + 1;
                FileName = "A10_" + (System.DateTime.Today.Year - 1911).ToString() + FileSn.ToString("0000000");//String.Format("{0:D7}", FileSn.ToString());//FileSn.ToString("0000000");
                string ATTACH = DEAction.GetATTACH_1(item.APP_ID, item.SRV_ID);
                string SourceFile = DataUtils.GetConfig("FOLDER_APPLY_FILE") + ATTACH;
                string extension = Path.GetExtension(SourceFile);//副檔名 

                int FileCount = 0;

                try
                {
                    //申請資料
                    Attach1Name = FileName + "-" + Count.ToString() + ".pdf";
                    byte[] b = null;
                    using (SqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        switch (item.SRV_ID)
                        {
                            case "005001":
                                Form005001Action action1 = new Form005001Action(conn);
                                b = action1.GetApplyWord(item.APP_ID);
                                break;
                            case "005002":
                                Form005002Action action2 = new Form005002Action(conn);
                                b = action2.GetApplyWord(item.APP_ID);
                                break;
                            case "005003":
                                Form005003Action action3 = new Form005003Action(conn);
                                b = action3.GetApplyWord(item.APP_ID);
                                break;
                            case "005004":
                                Form005004Action action4 = new Form005004Action(conn);
                                b = action4.GetApplyWord(item.APP_ID);
                                break;
                            case "005005":
                                Form005005Action action5 = new Form005005Action(conn);
                                b = action5.GetApplyWord(item.APP_ID);
                                break;
                        }
                        conn.Close();
                        conn.Dispose();
                    }
                    du.DownLoadApplyData(item.APP_ID, Attach1Name, b);
                    //上傳的附件
                    Count++;
                    FileCount++;
                    Attach2Name = FileName + "-" + Count.ToString() + extension;
                    du.DownLoadImage(SourceFile, Attach2Name);
                    FileCount++;
                    //產生公文DI檔
                    du.DownLoadXml(GenerateDIXml(item, Attach1Name, Attach2Name, FileName, extension), FileName + ".DI");
                    FileCount++;
                    //產生公文SW檔
                    du.DownLoadXml(GenerateSWXml(FileName), FileName + ".SW");
                    FileCount++;
                    //更新公文案號
                    DEAction.UpdataDocumentId(FileName, item.APP_ID);
                    //記錄產生的案號及公文號
                    AppNo = item.APP_ID + ",";
                    DocumentNo += FileName + "-" + Count.ToString() + ",";

                    Count = 1; //下一個檔，附件序號歸1
                    ResultCnt += 1;
                }
                catch
                {
                    Message = "發生不可知的錯誤！";
                    //TempData["message"] = "發生不可知的錯誤！";
                    //return RedirectToAction("Index");
                }
            }

            string[] files = System.IO.Directory.GetFiles(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"));
            string[] DIfiles = System.IO.Directory.GetFiles(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            int FileCnt = files.Length + DIfiles.Length;
            if (FileCnt == 0 || (FileCnt % 4) != 0 || ((model.Count * 4) != FileCnt))
            {
                if (FileCnt == 0)
                    return "目前無檔案可傳送！";
                else
                    return "產生檔案時發生不可知的錯誤！";


                //return RedirectToAction("Index");
            }
            else
            {
                string fileName = "";
                SFTPUtils sftp = null;
                try
                {
                    logger.Debug("公文系統SFTP上傳排程開始");
                    //上傳至sftp                    
                    sftp = new SFTPUtils(DataUtils.GetConfig("DOCUMENT_SFTP_IP"), DataUtils.GetConfig("DOCUMENT_SFTP_PORT"), DataUtils.GetConfig("DOCUMENT_SFTP_ACC"), DataUtils.GetConfig("DOCUMENT_SFTP_PWD"));
                    //先上傳DI檔
                    logger.Debug("sfpt：連線成功，公文內容：" + DIfiles.Count() + "，DI以外文件檔：" + files.Count());
                    foreach (var item in DIfiles)
                    {
                        fileName = item.Substring(item.LastIndexOf("\\") + 1);
                        logger.Debug(item + "傳送至：" + "/" + fileName);
                        sftp.Put(item, "/" + fileName);
                        logger.Debug(item + "傳送至：" + "/" + fileName + "/傳送成功");
                    }

                    //上傳DI以外公文所需的檔案
                    foreach (var item in files)
                    {
                        fileName = item.Substring(item.LastIndexOf("\\") + 1);
                        logger.Debug(item + "傳送至：" + DataUtils.GetConfig("DOCUMENT_SFTP_FOLDER") + "/" + fileName);
                        sftp.Put(item, DataUtils.GetConfig("DOCUMENT_SFTP_FOLDER") + "/" + fileName);
                        logger.Debug(item + "傳送至：" + DataUtils.GetConfig("DOCUMENT_SFTP_FOLDER") + "/" + fileName + "/傳送成功");
                    }

                    sftp.Disconnect();
                    TempData["message"] = "檔案傳送成功：一共有【" + model.Count + "】件申請案件，傳送至公文系統共有【" + FileCnt + "】個檔案。";

                    //寄發Mail通知管理員．批次時要MAIL通知
                    //MailUtils.SendMail(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_MAIL"), MessageUtils.MAIL_ToADMIN_SEND_DOCUMENTFILE_SUBJECT, String.Format(MessageUtils.MAIL_ToADMIN_SEND_DOCUMENTFILE_BODY_1,model.Count, files.Length));
                    logger.Debug("公文系統SFTP上傳排程結束");
                    Message = "檔案傳送成功";//：一共有【" + model.Count + "】件申請案件，傳送至公文系統共有【" + FileCnt + "】個檔案。";

                }
                catch
                {
                    Message = "檔案在傳送過程中發生不明原因失敗！";
                    if (!String.IsNullOrEmpty(DocumentNo))
                    {
                        DocumentNo = DocumentNo.Substring(0, DocumentNo.Length - 1);
                        string[] tmpArray = DocumentNo.Split(',');
                        foreach (var item in tmpArray)
                        {
                            //傳送成功，則不清空值
                            if (SuccessDocNo.IndexOf(item) == -1)
                            {
                                DEAction.UpdataClearDocumentId(item.Substring(0, 14));
                            }
                        }
                    }
                    logger.Debug("公文系統SFTP上傳排程失敗");
                }
                finally
                {
                    if (sftp != null && sftp.Connected) sftp.Disconnect();
                }
            }
            return Message;
            //return RedirectToAction("Index");
        }

        private string GenerateDIXml(DocumentExportModel.DocumentModel model, string attFileName1, string attFileName2, string DocumentId, string extension)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            // 附件檔名，每個附件一筆
            sb.Append("<!ENTITY 名單 SYSTEM \"" + DocumentId + ".SW\" NDATA DI>").Append("\n");
            sb.Append("<!NOTATION DI SYSTEM \"\">").Append("\n");
            sb.Append("<!ENTITY 附件一 SYSTEM \"" + attFileName1 + "\" NDATA pdf>").Append("\n");
            sb.Append("<!NOTATION  " + extension.Substring(1) + " SYSTEM \"\">").Append("\n");
            sb.Append("<!ENTITY 附件二 SYSTEM \"" + attFileName2 + "\" NDATA " + extension.Substring(1) + ">").Append("\n");
            sb.Append("<!NOTATION  pdf SYSTEM \"\">");


            XmlElement xe1, xe2, xe3;
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xd);

            doc.CreateComment("<!--  A10_1050000001.DI     2016.06.03  -->");


            XmlDocumentType type = doc.CreateDocumentType("函", null, "104_2_utf8.dtd", sb.ToString());
            doc.AppendChild(type);


            // 建立根結點物件
            XmlElement root = doc.CreateElement("函");
            doc.AppendChild(root);


            // 發文機關
            xe1 = doc.CreateElement("發文機關");
            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = model.UNIT_NAME;     // 固定寫入 "行政院衛生福利部"
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("機關代碼");    // 固定寫入 "327220000I"
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("函類別");
            xe1.SetAttribute("代碼", "函");
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("地址");
            xe1.InnerText = model.ADDRESS;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = "傳　　真：" + model.FAX;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = "聯絡人及電話：" + model.NAME + model.TEL;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = "電子郵件信箱：" + model.MAIL;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("受文者");
            xe2 = doc.CreateElement("交換表");
            xe2.SetAttribute("交換表單", "名單");
            xe2.InnerText = "衛生福利部中醫藥司";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("發文日期");
            xe2 = doc.CreateElement("年月日");
            xe2.InnerText = "中華民國" + (System.DateTime.Today.Year - 1911).ToString() + "年" + System.DateTime.Today.Month + "月" + System.DateTime.Today.Day + "日";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("發文字號");
            xe2 = doc.CreateElement("字");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("文號");
            xe3 = doc.CreateElement("年度");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);
            xe3 = doc.CreateElement("流水號");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);


            xe1 = doc.CreateElement("速別");
            xe1.SetAttribute("代碼", "普通件");
            root.AppendChild(xe1);


            xe1 = doc.CreateElement("附件");
            xe2 = doc.CreateElement("文字");
            if (model.SRV_ID == "005004" || model.SRV_ID == "005005")
                xe2.InnerText = "證明書擬稿、查廠核備函影印本";
            else
                xe2.InnerText = "證明書擬稿、許可證影本";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("附件檔名");
            xe2.SetAttribute("附件名", attFileName1 + "," + attFileName2);
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);


            xe1 = doc.CreateElement("主旨");
            xe2 = doc.CreateElement("文字");
            xe2.InnerText = model.SUBJECT;
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);


            xe1 = doc.CreateElement("段落");
            xe1.SetAttribute("段名", "說明");
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "一、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = model.CAPTION1;
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "二、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = model.CAPTION2;
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "三、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = model.CAPTION3;
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);

            if (!String.IsNullOrEmpty(model.CAPTION4))
            {
                xe2 = doc.CreateElement("條列");
                xe2.SetAttribute("序號", "四、");
                xe3 = doc.CreateElement("文字");
                xe3.InnerText = model.CAPTION4;
                xe2.AppendChild(xe3);
                xe1.AppendChild(xe2);
            }

            if (!String.IsNullOrEmpty(model.CAPTION5))
            {
                xe2 = doc.CreateElement("條列");
                xe2.SetAttribute("序號", "五、");
                xe3 = doc.CreateElement("文字");
                xe3.InnerText = model.CAPTION5;
                xe2.AppendChild(xe3);
                xe1.AppendChild(xe2);
            }

            if (!String.IsNullOrEmpty(model.CAPTION6))
            {
                xe2 = doc.CreateElement("條列");
                xe2.SetAttribute("序號", "六、");
                xe3 = doc.CreateElement("文字");
                xe3.InnerText = model.CAPTION6;
                xe2.AppendChild(xe3);
                xe1.AppendChild(xe2);
            }

            if (!String.IsNullOrEmpty(model.CAPTION7))
            {
                xe2 = doc.CreateElement("條列");
                xe2.SetAttribute("序號", "七、");
                xe3 = doc.CreateElement("文字");
                xe3.InnerText = model.CAPTION7;
                xe2.AppendChild(xe3);
                xe1.AppendChild(xe2);
            }

            if (!String.IsNullOrEmpty(model.CAPTION8))
            {
                xe2 = doc.CreateElement("條列");
                xe2.SetAttribute("序號", "八、");
                xe3 = doc.CreateElement("文字");
                xe3.InnerText = model.CAPTION8;
                xe2.AppendChild(xe3);
                xe1.AppendChild(xe2);
            }
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("正本");
            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "衛生福利部中醫藥司";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("署名");
            root.AppendChild(xe1);

            return doc.OuterXml;
        }

        private string GenerateSWXml(string DocumentId)
        {
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xd);
            doc.XmlResolver = null;

            XmlDocumentType type = doc.CreateDocumentType("交換表單", null, "104_Roster_utf8.dtd", null);
            doc.AppendChild(type);

            // 建立根節點
            XmlElement root = doc.CreateElement("交換表單");
            root.AppendChild(doc.CreateElement("單位名")).InnerText = "衛生福利部中醫藥司";
            root.AppendChild(doc.CreateElement("機關代碼")).InnerText = "A21000000I";
            root.AppendChild(doc.CreateElement("單位代碼")).InnerText = "UA10000";
            doc.AppendChild(root);

            return doc.OuterXml;
        }



    }
}
