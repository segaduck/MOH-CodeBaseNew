using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using iTextSharp.text;
using log4net;
using iTextSharp.text.pdf;
using System.Data.SqlClient;
using ES.Utils;
using System.Text;

namespace ES.Action.Form
{
    public class Form001010Action : FormBaseAction
    {
        public Form001010Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form001010Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.SRV_ID, A.APP_ID, A.APP_TIME, A.IDN, A.NAME, A.ADDR, A.TEL, B.ISSUE_DATE, B.LIC_NUM,
                    B.ACTION_TYPE, B.ISSUE_DEPT, B.ACTION_RES, B.OTHER_RES,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ACTION_TYPE AND CODE_KIND = 'F1_ACTION_TYPE_1') AS ACTION_TYPE_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ISSUE_DEPT AND CODE_KIND = 'F1_ISSUE_DEPT_1') AS ISSUE_DEPT_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_TYPE AND CODE_KIND = 'F1_LICENSE_CD_1') AS LIC_TYPE_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_CD AND CODE_KIND = 'F1_LICENSE_CD_1') AS LIC_CD_DESC,
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = B.ACTION_RES AND CODE_KIND = 'F1_ACTION_RES_1') AS ACTION_RES_DESC
                FROM APPLY A, APPLY_001010 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.APP_ID = @APP_ID
            ";

            args.Add("APP_ID", applyId);

            return GetData(querySQL, args);
        }
    }
}