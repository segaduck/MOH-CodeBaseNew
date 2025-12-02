using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using log4net;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class CertSftpAction : BaseAction
    {
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleSystemLogger");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public CertSftpAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 定期刪除資料
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public CertSftpAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public List<Dictionary<string, string>> GetCertUploadData()
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            #region SQL語法
            string querySQL = @"
                select a.CERT_SN,m.CARD_IDX,replace(replace(replace(convert(varchar,a.ADD_TIME,120),'-',''),':',''),' ','') as ADD_TIME,(select SETUP_VAL from SETUP where SETUP_CD='CERT_SYSTEM_ID') as SYSTEM_ID  from APPLY a inner join MEMBER m on a.ACC_NO=m.ACC_NO where LOGIN_TYPE='MOEACA' and convert(varchar,a.add_time,111) between @SDate and @EDate order by a.add_time
            ";
            #endregion

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "SDate", System.DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01"));
            DataUtils.AddParameters(cmd, "EDate", DateTime.Parse(System.DateTime.Now.ToString("yyyy/MM/01")).AddDays(-1).ToString("yyyy/MM/dd"));
            //DataUtils.AddParameters(cmd, "SDate", "2017/02/01");
            //DataUtils.AddParameters(cmd, "EDate", "2017/02/28");

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    #region 取資料
                    int n = 0;
                    item = new Dictionary<string, string>();

                    item.Add("CERT_SN", DataUtils.GetDBString(dr, n++)); // 憑證序號
                    item.Add("CARD_IDX", DataUtils.GetDBString(dr, n++)); // 用戶端識別碼(統一編號)
                    item.Add("ADD_TIME", DataUtils.GetDBString(dr, n++)); // 時間
                    item.Add("SYSTEM_ID", DataUtils.GetDBString(dr, n++));//系統代碼

                    list.Add(item);
                    #endregion
                }
                dr.Close();
            }

            return list;
        }

    }
}