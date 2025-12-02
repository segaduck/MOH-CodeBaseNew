using ES.Action.Form;
using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using Ionic.Zip;
using ES.Areas.BACKMIN.Utils;
using ES.Commons;

namespace ES.Areas.Admin.Controllers
{
    public class DocumentExportController : BaseController
    {
        //
        // GET: /BACKMIN/DocumentExport/
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            DocumentExportAction DEAction = new DocumentExportAction();
            DocumentExportModel.DocumentSet dsModel = new DocumentExportModel.DocumentSet();
            dsModel.path = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH");
            dsModel.mail = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_MAIL");
            List<DocumentExportModel.DocumentModel> model = DEAction.GetDocumentData();//取得尚未匯出的公文申請資料
            ViewBag.tempMessage = TempData["message"];
            ViewBag.ApplyCount = "共有申請案件【" + model.Count + "】件";

            this.SetVisitRecord("DocumentExport", "Index", "中醫藥司公文匯出");

            return View(dsModel);
        }

        [HttpPost]
        public ActionResult Save(DocumentExportModel.DocumentSet model)
        {
            DocumentExportAction DEAction = new DocumentExportAction();
            if (DEAction.UpdataDocumentId(model) == 2)
                TempData["message"] = "儲存成功";
            DataUtils.ClearConfig();
            return RedirectToAction("Index");
        }


        public ActionResult Export()
        {
            DocSftpController doc = new DocSftpController();
            TempData["message"] = doc.Export();
            return RedirectToAction("Index");

            #region === 原功能在這，但後追加mail連結執行，故改呼叫排程那隻，這邊mark掉   ===
            //DocumentExportAction DEAction = new DocumentExportAction();
            //List<DocumentExportModel.DocumentModel> model = DEAction.GetDocumentData();//取得尚未匯出的公文申請資料
            //DocumentUtils du = new DocumentUtils();
            //int FileSn = DEAction.GetDocumentId();
            //string FileName = "", Attach1Name = "", Attach2Name = "";
            //StringBuilder sb = new StringBuilder();
            //int Count = 1;
            //int ResultCnt = 0;
            //string AppNo = "", DocumentNo = "";
            //string SuccessDocNo = "";

            ////判斷資料夾是否存在
            //if (!Directory.Exists(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH")))
            //{
            //    //資料夾不存在
            //    Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"));
            //    //建立DI資料夾
            //    Directory.CreateDirectory(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            //}
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

            //foreach (var item in model)
            //{
            //    FileSn = FileSn + 1;
            //    FileName = "A10_" + (System.DateTime.Today.Year - 1911).ToString() + FileSn.ToString("0000000");//String.Format("{0:D7}", FileSn.ToString());//FileSn.ToString("0000000");
            //    string ATTACH = DEAction.GetATTACH_1(item.APP_ID, item.SRV_ID);
            //    string SourceFile = DataUtils.GetConfig("FOLDER_APPLY_FILE") + ATTACH;
            //    string extension = Path.GetExtension(SourceFile);//副檔名 

            //    int FileCount = 0;

            //    try
            //    {
            //        //申請資料
            //        Attach1Name = FileName + "-" + Count.ToString() + ".pdf";
            //        byte[] b = null;
            //        using (SqlConnection conn = GetConnection())
            //        {
            //            conn.Open();
            //            switch (item.SRV_ID)
            //            {
            //                case "005001":
            //                    Form005001Action action1 = new Form005001Action(conn);
            //                    b = action1.GetApplyWord(item.APP_ID);
            //                    break;
            //                case "005002":
            //                    Form005002Action action2 = new Form005002Action(conn);
            //                    b = action2.GetApplyWord(item.APP_ID);
            //                    break;
            //                case "005003":
            //                    Form005003Action action3 = new Form005003Action(conn);
            //                    b = action3.GetApplyWord(item.APP_ID);
            //                    break;
            //                case "005004":
            //                    Form005004Action action4 = new Form005004Action(conn);
            //                    b = action4.GetApplyWord(item.APP_ID);
            //                    break;
            //                case "005005":
            //                    Form005005Action action5 = new Form005005Action(conn);
            //                    b = action5.GetApplyWord(item.APP_ID);
            //                    break;
            //            }

            //        }
            //        du.DownLoadApplyData(item.APP_ID, Attach1Name, b);
            //        //上傳的附件
            //        Count++;
            //        FileCount++;
            //        Attach2Name = FileName + "-" + Count.ToString() + extension;
            //        du.DownLoadImage(SourceFile, Attach2Name);
            //        FileCount++;
            //        //產生公文DI檔
            //        du.DownLoadXml(GenerateDIXml(item, Attach1Name, Attach2Name, FileName, extension), FileName + ".DI");
            //        FileCount++;
            //        //產生公文SW檔
            //        du.DownLoadXml(GenerateSWXml(FileName), FileName + ".SW");
            //        FileCount++;
            //        //更新公文案號
            //        DEAction.UpdataDocumentId(FileName, item.APP_ID);
            //        //記錄產生的案號及公文號
            //        AppNo = item.APP_ID + ",";
            //        DocumentNo += FileName + "-" + Count.ToString() + ",";

            //        Count = 1; //下一個檔，附件序號歸1
            //        ResultCnt += 1;
            //    }
            //    catch
            //    {
            //        TempData["message"] = "發生不可知的錯誤！";
            //        return RedirectToAction("Index");
            //    }
            //}

            //string[] files = System.IO.Directory.GetFiles(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH"));
            //string[] DIfiles = System.IO.Directory.GetFiles(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            ////int FileCnt = files.Length + DIfiles.Length;
            ////if (FileCnt == 0 || (FileCnt % 4) != 0 || ((model.Count * 4) != FileCnt))
            ////{
            ////    if (FileCnt == 0)
            ////        TempData["message"] = "目前無檔案可傳送！";
            ////    else
            ////        TempData["message"] = "產生檔案時發生不可知的錯誤！";

            ////    return RedirectToAction("Index");
            ////}
            ////else
            ////{
            ////    string fileName = "";
            ////    SFTPUtils sftp = null;
            ////    try
            ////    {
            ////        logger.Debug("公文系統SFTP上傳排程開始");
            ////        //上傳至sftp                    
            ////        sftp = new SFTPUtils(DataUtils.GetConfig("DOCUMENT_SFTP_IP"), DataUtils.GetConfig("DOCUMENT_SFTP_PORT"), DataUtils.GetConfig("DOCUMENT_SFTP_ACC"), DataUtils.GetConfig("DOCUMENT_SFTP_PWD"));

            ////        //先上傳DI檔
            ////        foreach (var item in DIfiles)
            ////        {
            ////            sftp.Put(item, "/" + fileName);
            ////            logger.Debug(fileName + "傳送至：" + "/" + fileName);
            ////        }

            ////        //上傳DI以外公文所需的檔案
            ////        foreach (var item in files)
            ////        {
            ////            sftp.Put(item, DataUtils.GetConfig("DOCUMENT_SFTP_FOLDER") + "/" + fileName);
            ////            logger.Debug(fileName + "傳送至：" + DataUtils.GetConfig("DOCUMENT_SFTP_FOLDER") + "/" + fileName);
            ////        }

            ////        sftp.Disconnect();
            ////        TempData["message"] = "檔案傳送成功：一共有【" + model.Count + "】件申請案件，傳送至公文系統共有【" + FileCnt + "】個檔案。";

            ////        //寄發Mail通知管理員．批次時要MAIL通知
            ////        //MailUtils.SendMail(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_MAIL"), MessageUtils.MAIL_ToADMIN_SEND_DOCUMENTFILE_SUBJECT, String.Format(MessageUtils.MAIL_ToADMIN_SEND_DOCUMENTFILE_BODY_1,model.Count, files.Length));
            ////        logger.Debug("公文系統SFTP上傳排程結束");

            ////    }
            ////    catch
            ////    {
            ////        TempData["message"] = "檔案在傳送過程中發生不明原因失敗！";
            ////        if (!String.IsNullOrEmpty(DocumentNo))
            ////        {
            ////            DocumentNo = DocumentNo.Substring(0, DocumentNo.Length - 1);
            ////            string[] tmpArray = DocumentNo.Split(',');
            ////            foreach (var item in tmpArray)
            ////            {
            ////                //傳送成功，則不清空值
            ////                if (SuccessDocNo.IndexOf(item) == -1)
            ////                {
            ////                    DEAction.UpdataClearDocumentId(item.Substring(0, 14));
            ////                }
            ////            }
            ////        }
            ////        logger.Debug("公文系統SFTP上傳排程失敗");
            ////    }
            ////    finally
            ////    {
            ////        if (sftp != null && sftp.Connected) sftp.Disconnect();
            ////    }
            ////}

            //return RedirectToAction("Index");
            #endregion
        }

        #region
        public string GenerateDIXml(DocumentExportModel.DocumentModel model, string attFileName1, string attFileName2, string DocumentId, string extension)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            // 附件檔名，每個附件一筆
            sb.Append("<!ENTITY 名單 SYSTEM \"" + DocumentId + ".SW\" NDATA DI>").Append("\n");
            sb.Append("<!NOTATION DI SYSTEM \"\">").Append("\n");
            sb.Append("<!ENTITY 附件一 SYSTEM \"" + attFileName1 + "\" NDATA pdf>").Append("\n");
            sb.Append("<!NOTATION  " + "." + " SYSTEM \"\">").Append("\n");
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
            xe2.InnerText = "衛生福利部";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("署名");
            root.AppendChild(xe1);

            return doc.OuterXml;
        }

        public string GenerateSWXml(string DocumentId)
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

            var str = doc.OuterXml;
            var path = System.IO.Directory.GetFiles(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            doc.Save("C:\\e-service\\Document\\DI\\temp.sw");
            doc.Save($"C:\\e-service\\Document\\DI\\{DocumentId}.sw");
            //doc.Save($"{path}{DocumentId}.sw");
            return str;
        }
        #endregion
        /// <summary>
        /// 中醫藥司
        /// </summary>
        /// <param name="model"></param>
        /// <param name="files"></param>
        /// <param name="exportfile"></param>
        /// <returns></returns>
        public string GenerateDIXml(DocumentExportModel.DocumentModel model, List<FileGroupModel> files, string exportfile, string APP_ID_RAN)
        {
            var APP_ID = model.APP_ID;
            List<string> ext = new List<string>();
            ext.Add("pdf");
            if (model.SRV_ID == "005013")
            {
                ext.Add("pdf");
            }else if (model.SRV_ID == "005014")
            {
                ext.Add("pdf");
                ext.Add("pdf");
            }
            if (files != null && files.Count > 0)
            {
                foreach (var item in files)
                {
                    if (!ext.Contains(item.FILE_NAME.Split('.').LastOrDefault()))
                    {
                        ext.Add(item.FILE_NAME.Split('.').LastOrDefault());
                    }
                }
            }
            var filedims = "附件一";
            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            // 附件檔名，每個附件一筆
            sb.Append("<!ENTITY 附件一 SYSTEM \"" + APP_ID_RAN + "_Attach1.pdf" + "\" NDATA " + "_PDF" + ">").Append("\n");
            //切結書 005013 1
            if (model.SRV_ID == "005013")
            {
                filedims += " 附件二";
                sb.Append("<!ENTITY 附件二 SYSTEM \"" + APP_ID_RAN + "_Attach2.pdf" + "\" NDATA " + "_PDF" + ">").Append("\n");
                if (files != null & files.Count > 0)
                {
                    ulong i = 3;
                    foreach(var item in files)
                    {
                        var itxt= HelperUtil.NumberFormatZH(i, true);
                        filedims += " 附件" + itxt;
                        sb.Append("<!ENTITY 附件"+itxt+" SYSTEM \"" + APP_ID_RAN + "_Attach"+i+"" + "." + item.FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + item.FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                        i++;
                    } 
                }
            }
            else
            {
                //切結書 005014 1,2
                filedims += " 附件二";
                sb.Append("<!ENTITY 附件二 SYSTEM \"" + APP_ID_RAN + "_Attach2.pdf" + "\" NDATA " + "_PDF" + ">").Append("\n");
                filedims += " 附件三";
                sb.Append("<!ENTITY 附件三 SYSTEM \"" + APP_ID_RAN + "_Attach3.pdf" + "\" NDATA " + "_PDF" + ">").Append("\n");
                if (files != null & files.Count > 0)
                {
                    ulong i = 4;
                    foreach (var item in files)
                    {
                        var itxt = HelperUtil.NumberFormatZH(i, true);
                        filedims += " 附件" + itxt;
                        sb.Append("<!ENTITY 附件" + itxt + " SYSTEM \"" + APP_ID_RAN + "_Attach" + i + "" + "." + item.FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + item.FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                        i++;
                    }
                }
            }
        
            foreach (var item in ext)
            {
                if (item == "pdf")
                {
                    sb.Append("<!NOTATION " + "_" + item.ToUpper() + " SYSTEM \"AdobeReader\">").Append("\n");
                }
                else
                {
                    sb.Append("<!NOTATION " + "_" + item.ToUpper() + " SYSTEM \"\">").Append("\n");
                }
            }

            XmlElement xe1, xe2, xe3, xe4;
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
            xe2.InnerText = model.UNIT_NAME;
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("機關代碼");
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
            xe1.InnerText = "傳真：" + model.FAX;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = "聯絡人：" + model.NAME;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = "聯絡電話：" + model.TEL;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");
            xe1.InnerText = "電子郵件：" + model.MAIL;
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("受文者");
            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "衛生福利部";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("機關代碼");
            xe2.InnerText = "A21000000I";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

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
            xe3 = doc.CreateElement("支號");
            xe3.InnerText = "";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("速別");
            xe1.SetAttribute("代碼", "普通件");
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("密等及解密條件或保密期限");
            xe2 = doc.CreateElement("密等");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("解密條件或保密期限");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("附件");
            xe2 = doc.CreateElement("文字");
            xe2.InnerText = "附件";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("附件檔名");
            xe2.SetAttribute("附件名", filedims);
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
            xe3.InnerText = "申請人資料：";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(一)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = model.SRV_ID == "005013" ? "申請人姓名：" : "公司名稱：";
            xe3.InnerText += $"{model.applyData.Apply_NAME}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(二)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = model.SRV_ID == "005013" ? "身分證字號：" : "統一編號：";
            xe3.InnerText += $"{model.applyData.Apply_IDN}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(三)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = $"地址：{model.applyData.Apply_ADDR}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(四)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = $"電話：{model.applyData.CNT_TEL}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "二、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = model.SRV_ID == "005013" ? "申請自用中藥資料：" : "申請進口貨品資料：";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(一)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = model.SRV_ID == "005013" ? "申請藥品：" : "進口貨品名稱：";
            xe3.InnerText += $"{model.applyData.PRODUCTNAME}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(二)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = $"申請數量：{model.applyData.PRODUCTUNIT}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(三)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = $"生產國別：{model.applyData.PRODUCTION_COUNTRY}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(四)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = $"起運口岸：{model.applyData.TRANSFER_COUNTRY}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("條列");
            xe2.SetAttribute("序號", "(五)、");
            xe3 = doc.CreateElement("文字");
            xe3.InnerText = $"賣方國家：{model.applyData.SELL_COUNTRY}\n";
            xe2.AppendChild(xe3);
            xe1.AppendChild(xe2);

            if (model.SRV_ID == "005014")
            {
                model.CAPTION3 = "";
                model.CAPTION3 += $"聯絡人及方式：{model.NAME}、{model.TEL}、{model.MAIL}";

                xe2 = doc.CreateElement("條列");
                xe2.SetAttribute("序號", "三、");
                xe3 = doc.CreateElement("文字");
                xe3.InnerText = model.CAPTION3;
                xe2.AppendChild(xe3);
                xe1.AppendChild(xe2);
            }

            root.AppendChild(xe1);

            xe1 = doc.CreateElement("正本");
            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "衛生福利部";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            var str = doc.OuterXml;
            var path = System.IO.Directory.GetFiles(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            using (var writer = new XmlTextWriter("C:\\e-service\\Document\\DI\\temp.di", new UTF8Encoding(false)))
            {
                doc.Save(writer);
            }
            using (var writer = new XmlTextWriter($"C:\\e-service\\Document\\DI\\{APP_ID_RAN}.xml", new UTF8Encoding(false)))
            {
                doc.Save(writer);
            }
            using (var writer = new XmlTextWriter($"C:\\e-service\\Document\\DI\\{APP_ID_RAN}.di", new UTF8Encoding(false)))
            {
                doc.Save(writer);
            }
            //doc.Save("C:\\e-service\\Document\\DI\\temp.di");
            //doc.Save($"C:\\e-service\\Document\\DI\\{APP_ID_RAN}.xml");
            //doc.Save($"C:\\e-service\\Document\\DI\\{APP_ID_RAN}.di");
            //doc.Save($"{path}{model.APP_ID}.di");

            return str;
        }
        /// <summary>
        /// 醫事司
        /// </summary>
        /// <param name="model"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public string GenerateDIXml_A08(DocumentExportModel.DocumentModel model, List<FileGroupModel> files, string APP_ID_RAN)
        {
            var APP_ID = model.APP_ID;
            List<string> ext = new List<string>();
            if (files != null && files.Count > 0)
            {
                foreach (var item in files)
                {
                    if (!ext.Contains(item.FILE_NAME.Split('.').LastOrDefault()))
                    {
                        ext.Add(item.FILE_NAME.Split('.').LastOrDefault());
                    }
                }
            }
            var filedims = "";
            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            // 附件檔名，每個附件一筆
            if (files != null & files.Count > 0)
            {
                filedims += " 附件一";
                sb.Append("<!ENTITY 附件一 SYSTEM \"" + APP_ID_RAN + "_Attach1" + "." + files[0].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[0].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                if (files.Count > 1)
                {
                    filedims += " 附件二";
                    sb.Append("<!ENTITY 附件二 SYSTEM \"" + APP_ID_RAN + "_Attach2" + "." + files[1].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[1].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                    if (files.Count > 2)
                    {
                        filedims += " 附件三";
                        sb.Append("<!ENTITY 附件三 SYSTEM \"" + APP_ID_RAN + "_Attach3" + "." + files[2].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[2].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                        if (files.Count > 3)
                        {
                            filedims += " 附件四";
                            sb.Append("<!ENTITY 附件四 SYSTEM \"" + APP_ID_RAN + "_Attach4" + "." + files[3].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[3].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                            if (files.Count > 4)
                            {
                                filedims += " 附件五";
                                sb.Append("<!ENTITY 附件五 SYSTEM \"" + APP_ID_RAN + "_Attach5" + "." + files[4].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[4].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                                if (files.Count > 5)
                                {
                                    filedims += " 附件六";
                                    sb.Append("<!ENTITY 附件六 SYSTEM \"" + APP_ID + "_Attach6" + "." + files[5].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[5].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                                    if (files.Count > 6)
                                    {
                                        filedims += " 附件七";
                                        sb.Append("<!ENTITY 附件七 SYSTEM \"" + APP_ID_RAN + "_Attach7" + "." + files[6].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[6].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                                        if (files.Count > 7)
                                        {
                                            filedims += " 附件八";
                                            sb.Append("<!ENTITY 附件八 SYSTEM \"" + APP_ID_RAN + "_Attach8" + "." + files[7].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[7].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                                            if (files.Count > 8)
                                            {
                                                filedims += " 附件九";
                                                sb.Append("<!ENTITY 附件九 SYSTEM \"" + APP_ID_RAN + "_Attach9" + "." + files[8].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[8].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                                                if (files.Count > 9)
                                                {
                                                    filedims += " 附件十";
                                                    sb.Append("<!ENTITY 附件十 SYSTEM \"" + APP_ID_RAN + "_Attach10" + "." + files[9].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[9].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                                                    if (files.Count > 10)
                                                    {
                                                        filedims += " 附件十一";
                                                        sb.Append("<!ENTITY 附件十一 SYSTEM \"" + APP_ID_RAN + "_Attach11" + "." + files[10].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[10].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");
                                                        if (files.Count > 11)
                                                        {
                                                            filedims += " 附件十二";
                                                            sb.Append("<!ENTITY 附件十二 SYSTEM \"" + APP_ID_RAN + "_Attach12" + "." + files[11].FILE_NAME.Split('.').LastOrDefault().ToLower() + "\" NDATA " + "_" + files[11].FILE_NAME.Split('.').LastOrDefault().ToUpper() + ">").Append("\n");

                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var item in ext)
            {
                if (item == "pdf")
                {
                    sb.Append("<!NOTATION " + "_" + item.ToUpper() + " SYSTEM \"AdobeReader\">").Append("\n");
                }
                else
                {
                    sb.Append("<!NOTATION " + "_" + item.ToUpper() + " SYSTEM \"\">").Append("\n");
                }
            }

            XmlElement xe1, xe2, xe3, xe4;
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null;
            XmlDeclaration xd = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xd);

            doc.CreateComment("<!--  基本標籤集   104_basic_utf8.ent   1999.12.1  修改日期:2020.10.12  -->");
            doc.CreateComment("<!--  電子交換標籤集   104_exchange_utf8.ent    1999.12.1  修改日期:2020.10.12  -->");

            XmlDocumentType type = doc.CreateDocumentType("簽", null, "104_5_utf8.dtd", sb.ToString());
            doc.AppendChild(type);

            // 建立根結點物件
            XmlElement root = doc.CreateElement("簽");
            doc.AppendChild(root);

            // 發文機關
            xe1 = doc.CreateElement("發文機關");
            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "醫事司";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("機關代碼");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("文號");
            xe2 = doc.CreateElement("年度");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("流水號");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("支號");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("速別");
            xe1.SetAttribute("代碼", "普通件");
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("密等及解密條件或保密期限");
            xe2 = doc.CreateElement("密等");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            xe2 = doc.CreateElement("解密條件或保密期限");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            if (model.SRV_ID == "001039")
            {
                xe1 = doc.CreateElement("擬辦方式");
                xe1.SetAttribute("代碼", "2");
                root.AppendChild(xe1);
            }

            xe1 = doc.CreateElement("附件");
            xe2 = doc.CreateElement("文字");
            xe2.InnerText = "";
            xe1.AppendChild(xe2);
            if (!string.IsNullOrEmpty(filedims))
            {
                xe2 = doc.CreateElement("附件檔名");
                xe2.SetAttribute("附件名", filedims);
                xe1.AppendChild(xe2);
            }
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
            if (model.SRV_ID == "001039")
            {
                xe2 = doc.CreateElement("條列");
                xe2.SetAttribute("序號", "三、");
                xe3 = doc.CreateElement("文字");
                xe3.InnerText = model.CAPTION6;
                xe2.AppendChild(xe3);
                xe1.AppendChild(xe2);
            }
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("段落");
            xe1.SetAttribute("段名", "擬辦");
            xe2 = doc.CreateElement("文字");
            xe2.InnerText = model.CAPTION3; //擬辦
            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("署名");
            xe1.InnerText = model.CAPTION4; //07-011 or 07-028
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("年月日");
            xe1.InnerText = model.CAPTION5; // 年月日
            root.AppendChild(xe1);

            var str = doc.OuterXml;
            var path = System.IO.Directory.GetFiles(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH") + "DI\\");
            using (var writer = new XmlTextWriter("C:\\e-service\\Document\\DI\\temp.di", new UTF8Encoding(false)))
            {
                doc.Save(writer);
            }
            using (var writer = new XmlTextWriter($"C:\\e-service\\Document\\DI\\{APP_ID_RAN}.xml", new UTF8Encoding(false)))
            {
                doc.Save(writer);
            }
            using (var writer = new XmlTextWriter($"C:\\e-service\\Document\\DI\\{APP_ID_RAN}.di", new UTF8Encoding(false)))
            {
                doc.Save(writer);
            }

            return str;
        }
    }
}
