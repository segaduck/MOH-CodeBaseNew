using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Data.SqlClient;
using ES.Models;
using ES.Utils;

namespace ES.Action
{
    public class SiteAction : BaseAction
    {
        /// <summary>
        /// 相關網站
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public SiteAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 相關網站
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public SiteAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得列表
        /// </summary>
        /// <returns></returns>
        public List<SiteModel> GetList()
        {
            List<SiteModel> list = new List<SiteModel>();

            string sql = @" SELECT SITE_NAME, SITE_URL, SEQ_NO
                            FROM SITE
                            WHERE DEL_MK = 'N'
                            ORDER BY SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SiteModel item = new SiteModel()
                    {
                        Name = DataUtils.GetDBString(dr, 0),
                        Url = DataUtils.GetDBString(dr, 1),
                        Seq = DataUtils.GetDBInt(dr, 2)
                    };

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }
    }
}