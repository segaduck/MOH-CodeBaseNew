using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Data.SqlClient;
using System.IO;
using System.Xml;

namespace ES.Action.Form
{
    public class Form005011Action : FormBaseAction
    {
        public Form005011Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form005011Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("APP_ID", applyId);

            string querySQL = @"
                SELECT A.APP_ID, A.APP_TIME,
                    B.MF_NAME, B.MF_CD, B.MF_IDN, B.MF_ZIP_1, B.MF_ADDR_1,
                    B.MF_ZIP_2, B.MF_ADDR_2, B.MF_TEL, B.MF_FAX, B.MF_CHR_NAME,
                    B.ATTACH_1, B.ATTACH_2
                FROM APPLY A, APPLY_005011 B 
                WHERE A.APP_ID = B.APP_ID
                  AND A.APP_ID = @APP_ID
            ";

            return GetData(querySQL, args);
        }

        public byte[] GetXML(Dictionary<string, object> item)
        {
            MemoryStream ms = new MemoryStream();
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            // 建立根結點物件
            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            // 主檔
            XmlElement main = doc.CreateElement("MainData");
            root.AppendChild(main);

            if (!String.IsNullOrEmpty((string)item["APP_ID"]))
            {
                XmlElement e = doc.CreateElement("AppID");
                e.InnerText = (string) item["APP_ID"];
                main.AppendChild(e);
            }

            if (item["APP_TIME"] != null)
            {
                XmlElement e = doc.CreateElement("ApplyDate");
                e.InnerText = ((DateTime)item["APP_TIME"]).ToString("yyyyMMdd");
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_NAME"]))
            {
                XmlElement e = doc.CreateElement("ApplyName");
                e.InnerText = (string)item["MF_NAME"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_CHR_NAME"]))
            {
                XmlElement e = doc.CreateElement("UserName");
                e.InnerText = (string)item["MF_CHR_NAME"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_IDN"]))
            {
                XmlElement e = doc.CreateElement("UniteNum");
                e.InnerText = (string)item["MF_IDN"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_ZIP_1"]))
            {
                XmlElement e = doc.CreateElement("DeliveryNum");
                e.InnerText = (string)item["MF_ZIP_1"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_ADDR_1"]))
            {
                XmlElement e = doc.CreateElement("BusinessAddress");
                e.InnerText = (string)item["MF_ADDR_1"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_TEL"]))
            {
                XmlElement e = doc.CreateElement("Tel");
                e.InnerText = (string)item["MF_TEL"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_FAX"]))
            {
                XmlElement e = doc.CreateElement("Fax");
                e.InnerText = (string)item["MF_FAX"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_CD"]))
            {
                XmlElement e = doc.CreateElement("OrganizationID");
                e.InnerText = (string)item["MF_CD"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["MF_ADDR_2"]))
            {
                XmlElement e = doc.CreateElement("MailAddress");
                e.InnerText = (string)item["MF_ADDR_2"];
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["ATTACH_1"]))
            {
                XmlElement e = doc.CreateElement("AttachFile1");
                string file = (string)item["ATTACH_1"];
                if (file.LastIndexOf("\\") >= 0)
                {
                    file = file.Substring(file.LastIndexOf("\\")+1);
                }
                e.InnerText = file;
                main.AppendChild(e);
            }

            if (!String.IsNullOrEmpty((string)item["ATTACH_2"]))
            {
                XmlElement e = doc.CreateElement("AttachFile2");
                string file = (string)item["ATTACH_2"];
                if (file.LastIndexOf("\\") >= 0)
                {
                    file = file.Substring(file.LastIndexOf("\\")+1);
                }
                e.InnerText = file;
                main.AppendChild(e);
            }

            doc.Save(ms);

            return ms.ToArray();
        }
    }
}