using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Controllers;
using System.Xml;
using System.Text;

namespace ES.Areas.Admin.Controllers
{
    public class DocumentController : BaseController
    {
        //
        // GET: /Schedule/Document/

        public ActionResult Run()
        {
            CreateDI();

            ViewBag.Message = "DI產生完成，FTP上傳成功";

            return View("Message");
        }

        private void CreateDI()
        {
            XmlElement xe1, xe2, xe3;

            StringBuilder sb = new StringBuilder();
            //sb.Append("<!DOCTYPE 函 SYSTEM \"99_2_utf8.dtd\" [").Append("\n");
            sb.Append("\n");

            // 附件檔名，每個附件一筆
            sb.Append("<!ENTITY ATTCH01 SYSTEM \"20111108020007000101.doc\" NDATA _X>").Append("\n");
            sb.Append("<!ENTITY ATTCH02 SYSTEM \"20111108020007000102.doc\" NDATA _X>").Append("\n");

            // 固定文字，其中，
            // "201111080200070001.sw"：雙引號中的放的是案件編號，副檔固定為sw (但無實際附件檔)
            sb.Append("<!ENTITY 表單 SYSTEM \"201111080200070001.sw\" NDATA DI>").Append("\n");

            // 表頭固定文字
            sb.Append("<!NOTATION DI SYSTEM \"\">").Append("\n");
            sb.Append("<!NOTATION _X SYSTEM \"\">").Append("\n");
            //sb.Append("]>").Append("\n");

            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = null; // 忽略DTD驗證
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            XmlDocumentType docType;
            docType = doc.CreateDocumentType("函", null, "99_2_utf8.dtd", sb.ToString());
            doc.AppendChild(docType);

            // 建立根結點物件
            XmlElement root = doc.CreateElement("函");
            doc.AppendChild(root);

            // 發文機關
            xe1 = doc.CreateElement("發文機關");

            xe2 = doc.CreateElement("全銜");
            xe2.InnerText = "行政院衛生福利部";     // 固定寫入 "行政院衛生福利部"
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("機關代碼");    // 固定寫入 "327220000I"
            xe2.InnerText = "327220000I";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 函類別
            xe1 = doc.CreateElement("函類別");     // 固定文字
            xe1.SetAttribute("代碼", "函");
            root.AppendChild(xe1);

            // 地址
            xe1 = doc.CreateElement("地址");      // 固定文字
            xe1.InnerText = "10341台北市大同區塔城街36號";
            root.AppendChild(xe1);

            // 聯絡方式
            xe1 = doc.CreateElement("聯絡方式");    // 固定文字
            xe1.InnerText = "傳真：(02)85906041";
            root.AppendChild(xe1);

            xe1 = doc.CreateElement("聯絡方式");    // 固定文字
            xe1.InnerText = "電話：(02)85906666";
            root.AppendChild(xe1);

            // 受文者
            xe1 = doc.CreateElement("受文者");     // 固定寫入 "行政院衛生福利部"
            xe1.InnerText = "行政院衛生福利部";
            root.AppendChild(xe1);

            // 發文日期
            xe1 = doc.CreateElement("發文日期");

            xe2 = doc.CreateElement("年月日");     // 寫入申辦日期，格式固定為：中華民國XX年XX月XX日
            xe2.InnerText = "中華民國100年11月16日";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 發文字號
            xe1 = doc.CreateElement("發文字號");

            xe2 = doc.CreateElement("字");       // 固定為 "線上申辦系統"
            xe2.InnerText = "線上申辦系統";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("文號");

            xe3 = doc.CreateElement("年度");      // 寫入申辦年度民國年。例如：100
            xe3.InnerText = "100";
            xe2.AppendChild(xe3);

            xe3 = doc.CreateElement("流水號");     // 寫入案件申請單號
            xe3.InnerText = "201111080200070001";
            xe2.AppendChild(xe3);

            xe1.AppendChild(xe2);
            root.AppendChild(xe1);

            // 速別
            xe1 = doc.CreateElement("速別");     // 固定文字
            xe1.SetAttribute("代碼", "普通件");
            root.AppendChild(xe1);

            // 密等及解密條件或保密期限
            xe1 = doc.CreateElement("密等及解密條件或保密期限");    // 密等固定文字

            xe2 = doc.CreateElement("密等");
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("解密條件或保密期限");
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 附件
            xe1 = doc.CreateElement("附件");      // 無附件時，就不要出現這個Tag

            xe2 = doc.CreateElement("文字");
            xe2.InnerText = "申請公文、工廠英文名稱及地址資料影本各1份";
            xe1.AppendChild(xe2);

            xe2 = doc.CreateElement("附件檔名");
            xe2.SetAttribute("附件名", "ATTCH01 ATTCH02");
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 主旨
            xe1 = doc.CreateElement("主旨");

            xe2 = doc.CreateElement("文字");      // 寫入申辦表單名稱，例如：產銷證明書
            xe2.InnerText = "產銷證明書";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            // 正本
            xe1 = doc.CreateElement("正本");

            xe2 = doc.CreateElement("全銜");      // 固定寫入 "行政院衛生福利部"
            xe2.InnerText = "行政院衛生福利部";
            xe1.AppendChild(xe2);

            root.AppendChild(xe1);

            doc.Save("C:\\e-service\\test.xml");
        }
    }
}
