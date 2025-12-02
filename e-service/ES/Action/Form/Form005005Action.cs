using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ES.Action.Form
{
    public class Form005005Action : FormBaseWordAction
    {
        public Form005005Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form005005Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(String id) 
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.SRV_ID, A.APP_TIME, A.NAME AS MF_NAME, A.ENAME AS MF_ENAME,
                    B.MF_ADDR, B.LIC_NUM,
                    CONVERT(VARCHAR, ISSUE_DATE, 107) AS ISSUE_DATE,
                    CONVERT(VARCHAR, EXPIR_DATE, 107) AS EXPIR_DATE
                FROM APPLY A, APPLY_005005 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.APP_ID = @APP_ID
            ";

            args.Add("APP_ID", id);

            return GetData(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList1(string id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"SELECT * FROM APPLY_005005_IC WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetList(querySQL, args);
        }

        public byte[] GetApplyWord(string id)
        {
            Dictionary<string, object> data = GetData(id);
            List<Dictionary<string, object>> list1 = GetList1(id);

            string IMP_COUNTRY = "";
            for (int i = 0; i < list1.Count; i++)
            {
                IMP_COUNTRY += list1[i]["IMP_COUNTRY"];

                if (i < list1.Count - 1) IMP_COUNTRY += ", ";
            }

            data.Add("IMP_COUNTRY", IMP_COUNTRY);

            string path = DataUtils.GetConfig("FOLDER_TEMPLATE") + data["SRV_ID"];

            string templateFile = path + "/GMP_certificate空白證書格式_0030833001.docx";
            string tempFile = path + "/" + new DateTime().ToString("yyyyMMddHHmmssfff") + ".docx";

            return MarkDocx(tempFile, templateFile, data);
        }
    }
}