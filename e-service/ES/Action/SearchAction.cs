using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Models;

namespace ES.Action
{
    public class SearchAction : BaseAction
    {
        /// <summary>
        /// <summary>
        /// 案件搜尋
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public SearchAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 案件搜尋
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public SearchAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public List<Dictionary<string, object>> GetList(SearchModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("NAME", model.Name);

            string querySQL = @"
                SELECT T.*, ROW_NUMBER() OVER (ORDER BY C_SEQ_NO, S_SEQ_NO) AS _ROW_NO
                FROM (
	                SELECT S.SRV_ID, S.NAME, S.SC_ID, S.SC_PID, S.SEQ_NO AS S_SEQ_NO, (CASE
			                WHEN C.LEVEL = 1 THEN C.SEQ_NO * 1000
			                WHEN C.LEVEL = 2 THEN (SELECT SEQ_NO FROM SERVICE_CATE WHERE SC_ID = C.SC_PID) * 1000 + C.SEQ_NO
		                END) AS C_SEQ_NO,
		                (CASE WHEN S.SC_ID <> S.SC_PID THEN (SELECT NAME + ' / ' FROM SERVICE_CATE WHERE SC_ID = S.SC_PID) ELSE '' END) + C.NAME AS CATE_NAME
	                FROM SERVICE S, SERVICE_CATE C
	                WHERE S.DEL_MK = 'N'
		                AND S.SC_ID = C.SC_ID
		                AND S.ONLINE_S_MK = 'Y'
                        AND S.NAME LIKE '%' + @NAME + '%'
                ) T
            ";

            return GetList(querySQL, model.NowPage, args);
        }
    }
}