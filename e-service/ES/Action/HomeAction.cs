using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Models;
using ES.Utils;

namespace ES.Action
{
    public class HomeAction : BaseAction
    {
        /// <summary>
        /// <summary>
        /// 首頁
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public HomeAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 首頁
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public HomeAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public List<LeftMenuItemModel> GetLeftMenuList()
        {
            List<LeftMenuItemModel> list = new List<LeftMenuItemModel>();

            string sql = @" SELECT UNIT_CD, (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = T.UNIT_CD) AS UNIT_NAME, SUM(COUNT) AS SUM
                            FROM (
	                            SELECT C.SC_ID, C.UNIT_CD, (SELECT COUNT(1) FROM SERVICE WHERE DEL_MK = 'N' AND ONLINE_S_MK = 'Y' AND SC_ID = C.SC_ID) AS COUNT
	                            FROM SERVICE_CATE C
	                            WHERE C.DEL_MK = 'N'
                            ) T
                            GROUP BY UNIT_CD
                            HAVING SUM(COUNT) > 0";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    LeftMenuItemModel item = new LeftMenuItemModel();
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.UnitName = DataUtils.GetDBString(dr, n++);
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 依分類取得申辦案件
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetServiceList(int categoryId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                WITH T AS (
	                SELECT S.SRV_ID, S.SC_ID, S.NAME, S.SEQ_NO AS S_SEQ_NO, (CASE
			                WHEN C.LEVEL = 1 THEN C.SEQ_NO * 1000
			                WHEN C.LEVEL = 2 THEN (SELECT SEQ_NO FROM SERVICE_CATE WHERE SC_ID = C.SC_PID) * 1000 + C.SEQ_NO
		                END) AS C_SEQ_NO
	                FROM SERVICE S, SERVICE_CATE C
	                WHERE S.SC_ID = C.SC_ID
		                AND (C.SC_ID = @SC_ID OR C.SC_PID = @SC_ID)
		                /*AND S.ONLINE_N_MK = 'Y'*/
		                AND S.ONLINE_S_MK = 'Y'
                )
                SELECT SRV_ID, NAME FROM T
                ORDER BY C_SEQ_NO, S_SEQ_NO
            ";

            args.Add("SC_ID", categoryId);

            return GetList(querySQL, args);
        }

        /// <summary>
        /// 熱門申辦
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetHotApplyList()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT TOP 10 S.SRV_ID, S.NAME, COUNT(1) AS TOTAL
                FROM APPLY A, SERVICE S
                WHERE A.SRV_ID = S.SRV_ID
	                AND A.APP_TIME > DATEADD(MONTH, -12 , GETDATE())
                    /*AND S.ONLINE_N_MK = 'Y'*/
                    AND S.ONLINE_S_MK = 'Y'
                GROUP BY S.SRV_ID, S.NAME
                ORDER BY 3 DESC
            ";

            return GetList(querySQL, args);
        }

        public List<Dictionary<string, object>> GetMessageList()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT MSG_ID, TITLE 
                FROM MESSAGE
                WHERE DEL_MK = 'N'
                    AND TIME_S <= GETDATE()
	                AND TIME_E >= GETDATE()
                ORDER BY TIME_S DESC
            ";

            return GetList(querySQL, args);
        }
    }
}