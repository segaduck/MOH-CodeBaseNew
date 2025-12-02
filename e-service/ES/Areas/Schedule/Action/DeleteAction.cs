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
    public class DeleteAction : BaseAction
    {
        private static readonly new ILog logger = LogUtils.GetLogger("ScheduleSystemLogger");

        /// <summary>
        /// 定期刪除資料
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public DeleteAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 定期刪除資料
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public DeleteAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 刪除申請單暫存資料
        /// </summary>
        public void DeleteApplyTemp()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("TIME", Int32.Parse(DateTime.Now.AddYears(-1).ToString("yyyyMMdd")));

            string sql = @"DELETE APPLY_TEMP WHERE CONVERT(INT, SUBSTRING(APP_TIME, 1, 8)) < @TIME";

            Update(sql, args);
        }
    }
}