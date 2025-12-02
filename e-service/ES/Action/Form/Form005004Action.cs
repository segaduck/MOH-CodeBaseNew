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
    public class Form005004Action : FormBaseWordAction
    {
        public Form005004Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form005004Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(String id) 
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.SRV_ID, A.APP_TIME, A.NAME AS MF_NAME, A.ENAME AS MF_ENAME,
                    B.MF_ADDR, B.LIC_NUM,DATEPART(year,B.EXPIR_DATE)-1911 as ED_YEAR,RIGHT(REPLICATE('0', 2) + CAST(DATEPART(month,B.EXPIR_DATE) as NVARCHAR), 2) as ED_MONTH,RIGHT(REPLICATE('0', 2) + CAST(DATEPART(day,B.EXPIR_DATE) as NVARCHAR), 2) as ED_DAY
                FROM APPLY A, APPLY_005004 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.APP_ID = @APP_ID
            ";

            args.Add("APP_ID", id);

            return GetData(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList1(string id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"SELECT (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = A.TRA_FORMS AND CODE_KIND = 'F5_DOSAGE_FORM' ) AS TRA_FORMS FROM APPLY_005004_TRA A WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetList(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList2(string id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"SELECT (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = A.CON_FORMS AND CODE_KIND = 'F5_DOSAGE_FORM' ) AS CON_FORMS FROM APPLY_005004_CON A WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetList(querySQL, args);
        }

        public byte[] GetApplyWord(string id)
        {
            Dictionary<string, object> data = GetData(id);
            List<Dictionary<string, object>> list1 = GetList1(id);
            List<Dictionary<string, object>> list2 = GetList2(id);

            string TRA_FORMS = "", CON_FORMS = "";
            if (list1.Count > 0)
            {
                TRA_FORMS = "<w:br />";
                TRA_FORMS += list1.Count > 0 && list2.Count > 0 ? "1.傳統劑型：" : "傳統劑型：";
                for (int i = 0; i < list1.Count; i++)
                {
                    TRA_FORMS += list1[i]["TRA_FORMS"];
                    if (i < list1.Count - 1) TRA_FORMS += "、";
                }
            }
            
            if (list2.Count > 0)
            {
                CON_FORMS = "<w:br />";
                CON_FORMS += list1.Count > 0 && list2.Count > 0 ? "2.濃縮劑型：" : "濃縮劑型：";
                for (int i = 0; i < list2.Count; i++)
                {
                    CON_FORMS += list2[i]["CON_FORMS"];

                    if (i < list2.Count - 1) CON_FORMS += "、";
                }
            }
            

            data.Add("TRA_FORMS", TRA_FORMS);
            data.Add("CON_FORMS", CON_FORMS);

            string path = DataUtils.GetConfig("FOLDER_TEMPLATE") + data["SRV_ID"];

            string templateFile = path + "/中文GMP證明書.docx";
            string tempFile = path + "/" + new DateTime().ToString("yyyyMMddHHmmssfff") + ".docx";

            return MarkDocx(tempFile, templateFile, data);
        }
    }
}