using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using ES.Utils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ES.Areas.Admin.Action
{
    public class ApplyTempAction : BaseAction
    {

        public ApplyTempAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        public ApplyTempAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public List<Dictionary<string, object>> GetList(int nowPage)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            string querySQL = @"
                SELECT 	ACC_NO, APP_TIME, SESSION_KEY,
                    ROW_NUMBER() OVER (ORDER BY APP_TIME DESC) AS _ROW_NO
                FROM APPLY_TEMP
            ";

            return GetList(querySQL, nowPage, args);
        }

        public Dictionary<string, string> GetTempData(string time)
        {
            Dictionary<string, string> data = null;

            string sql1 = @"
                SELECT APP_DATA
                FROM APPLY_TEMP 
                WHERE APP_TIME = @APP_TIME
            ";

            SqlCommand cmd = new SqlCommand(sql1, conn, tran);
            DataUtils.AddParameters(cmd, "APP_TIME", time);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    byte[] buffer = (byte[])dr[0];

                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(buffer, 0, buffer.Length);
                        ms.Seek(0, 0);
                        BinaryFormatter b = new BinaryFormatter();
                        data = (Dictionary<string, string>)b.Deserialize(ms);
                    }
                }
                dr.Close();
            }

            return data;
        }
    }
}