using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using System.Data;
using ES.Areas.Admin.Models;

namespace ES.Areas.Admin.Action
{
    public class ExportAction : BaseAction
    {
        /// <summary>
        /// 社救司案件匯出
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ExportAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 社救司案件匯出
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ExportAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public DataTable GetCase1(ExportCase1Models model)
        {
            string querySQL = @"
                SELECT A.APP_ID, CONVERT(VARCHAR(10), A.APP_TIME, 111) AS APP_TIME, 
		            (SELECT NAME FROM SERVICE WHERE SRV_ID = A.SRV_ID) AS SRV_NAME,
		            B.DONOR_NAME, B.DONOR_IDN, 
                    (CASE B.PUBLIC_MK WHEN 'Y' THEN '是' ELSE '否' END) AS PUBLIC_MK,
                    B.DONOR_MAIL, B.DONOR_TEL, B.AMOUNT,
		            (CASE C.REC_MK WHEN 'Y' THEN '需要收據' ELSE '不需要收據' END) AS REC_MK,
	                (CASE C.REC_MK WHEN 'Y' THEN C.REC_TITLE_CD ELSE '' END) AS REC_TITLE_CD,
	                (CASE C.REC_MK WHEN 'Y' THEN C.REC_TITLE ELSE '' END) AS REC_TITLE,
	                (CASE C.REC_MK WHEN 'Y' THEN C.REC_ADDR_1 ELSE '' END) AS REC_ADDR_1,
	                (CASE C.REC_MK WHEN 'Y' THEN C.REC_ADDR_2 ELSE '' END) AS REC_ADDR_2
                FROM APPLY A, APPLY_007001 B, APPLY_007001_DATA C
                WHERE A.APP_ID = B.APP_ID
	                AND B.APP_ID = C.APP_ID
            ";
            if (model.DateS.HasValue)
            {
                querySQL += "AND A.APP_TIME >=  @APP_TIME_S ";
            }

            if (model.DateE.HasValue)
            {
                querySQL += "AND A.APP_TIME <  @APP_TIME_E ";
            }

            if (!String.IsNullOrEmpty(model.ServiceId))
            {
                querySQL += "AND A.SRV_ID = @SRV_ID ";
            }

            SqlCommand cmd = new SqlCommand(querySQL, conn);

            if (model.DateS.HasValue)
            {
                cmd.Parameters.AddWithValue("APP_TIME_S", model.DateS.Value);
            }

            if (model.DateE.HasValue)
            {
                cmd.Parameters.AddWithValue("APP_TIME_E", model.DateS.Value.AddDays(1));
            }

            if (!String.IsNullOrEmpty(model.ServiceId))
            {
                cmd.Parameters.AddWithValue("SRV_ID", model.ServiceId);
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("案件編號", typeof(string));
            dt.Columns.Add("申請日期", typeof(string));
            dt.Columns.Add("捐款專案", typeof(string));
            dt.Columns.Add("捐款人姓名", typeof(string));
            dt.Columns.Add("身分證字號", typeof(string));
            dt.Columns.Add("公開姓名", typeof(string));
            dt.Columns.Add("E-mail", typeof(string));
            dt.Columns.Add("連絡電話", typeof(string));
            dt.Columns.Add("捐款金額", typeof(int));
            dt.Columns.Add("收據", typeof(string));
            dt.Columns.Add("收據抬頭", typeof(string));
            dt.Columns.Add("另開立收據抬頭", typeof(string));
            dt.Columns.Add("郵遞區號", typeof(string));
            dt.Columns.Add("收據地址", typeof(string));

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    dt.Rows.Add(
                        dr.GetValue(0),
                        dr.GetValue(1),
                        dr.GetValue(2),
                        dr.GetValue(3),
                        dr.GetValue(4),
                        dr.GetValue(5),
                        dr.GetValue(6),
                        dr.GetValue(7),
                        dr.GetValue(8),
                        dr.GetValue(9),
                        dr.GetValue(10),
                        dr.GetValue(11),
                        dr.GetValue(12),
                        dr.GetValue(13)
                    );
                }
                dr.Close();
            }

            return dt;
        }
    }
}