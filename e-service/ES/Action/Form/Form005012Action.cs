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
    public class Form005012Action : FormBaseAction
    {
        public Form005012Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form005012Action(SqlConnection conn, SqlTransaction tran)
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
                    B.MF_CD, B.MF_NAME, B.MF_ZIP, B.MF_ADDR, B.MF_LIC_CD,
                    B.MF_CHR_NAME, B.MF_LIC_NUM, B.MF_IDN, 
                    B.MF_CD_2, B.MF_NAME_2, B.MF_ZIP_2, B.MF_CHR_NAME_2,
                    B.MF_ADDR_2, B.MF_LIC_CD_2, B.MF_LIC_NUM_2, B.MF_UNI_CD_2, 
                    B.ATTACH_1, B.ATTACH_2, B.ATTACH_3, B.ATTACH_4, B.ATTACH_5 
                FROM APPLY A, APPLY_005012 B 
                WHERE A.APP_ID = B.APP_ID
                  AND A.APP_ID = @APP_ID
            ";

            return GetData(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("APP_ID", applyId);

            string querySQL = @"
                SELECT APP_ID, SRL_NO,
                    L_LIC_CD, L_LIC_NUM, L_NAME, L_CH_NAME, L_AUTH_DATE
                FROM APPLY_005012_LIST
                WHERE APP_ID = @APP_ID
            ";

            return GetList(querySQL, args);
        }

        public byte[] GetXML(Dictionary<string, object> item, List<Dictionary<string, object>> list)
        {
            string[] mainKeys = new string[] {
                "APP_ID", "APP_TIME",
                "MF_CD", "MF_IDN", "MF_NAME", "MF_CHR_NAME", "MF_ADDR", "MF_LIC_NUM",
                "MF_CD_2", "MF_UNI_CD_2", "MF_NAME_2", "MF_CHR_NAME_2", "MF_ADDR_2", "MF_LIC_NUM_2",
                "ATTACH_1", "ATTACH_2", "ATTACH_3", "ATTACH_4", "ATTACH_5"
            };

            string[] mainTags = new string[] {
                "AppID", "ApplyDate",
                "DetApp", "DetAppUN", "DetAppName", "DetUserName", "DetAddress", "DetLicenceNo", 
                "BeDetApp", "BeDetAppUN", "BeDetAppName", "BeDetUserName", "BeDetAddress", "BeDetLicenceNo",
                "AttachFile1", "AttachFile2", "AttachFile3", "AttachFile4", "AttachFile5",
            };

            string[] detailKeys = new string[] {
                "APP_ID", "SRL_NO", "L_LIC_CD", "L_AUTH_DATE", "L_NAME"
            };

            string[] detailTags = new string[] { 
                "AppID", "SEQ", "Liceid", "ApproveDate", "ItemName"
            };

            MemoryStream ms = new MemoryStream();
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            // 建立根結點物件
            XmlElement root = doc.CreateElement("root");
            doc.AppendChild(root);

            // 主檔
            XmlElement main = doc.CreateElement("MainData");
            root.AppendChild(main);

            item["APP_TIME"] = ((DateTime)item["APP_TIME"]).ToString("yyyyMMdd");

            for (int i = 1; i <= 5; i++)
            {
                string file = (string)item["ATTACH_" + i];
                if (file.LastIndexOf("\\") >= 0)
                {
                    file = file.Substring(file.LastIndexOf("\\") + 1);
                }
                item["ATTACH_" + i] = file;
            }

            for (int i = 0; i < mainKeys.Length; i++)
            {
                //logger.Debug("key: " + mainKeys[i]);
                if (!Convert.IsDBNull(item[mainKeys[i]]) && !String.IsNullOrEmpty((string)item[mainKeys[i]]))
                {
                    //logger.Debug(mainKeys[i]);
                    XmlElement e = doc.CreateElement(mainTags[i]);
                    e.InnerText = (string)item[mainKeys[i]];
                    main.AppendChild(e);
                }
            }

            XmlElement detail = doc.CreateElement("ApplyDetail");
            root.AppendChild(detail);

            foreach (Dictionary<string, object> data in list)
            {
                data["SRL_NO"] = data["SRL_NO"].ToString();
                data["L_AUTH_DATE"] = ((DateTime)data["L_AUTH_DATE"]).ToString("yyyyMMdd");
                
                XmlElement detail2 = doc.CreateElement("LicenceDetail");
                detail.AppendChild(detail2);

                for (int i = 0; i < detailKeys.Length; i++)
                {
                    XmlElement e = doc.CreateElement(detailTags[i]);
                    e.InnerText = (string)data[detailKeys[i]];
                    detail2.AppendChild(e);
                }
            }

            doc.Save(ms);

            return ms.ToArray();
        }
    }
}