using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class ServiceFormAction : ServiceAction
    {
        /// <summary>
        /// 表單設定
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceFormAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 表單設定
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceFormAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public ServiceFormEditModel GetEdit(string serviceId)
        {

            ServiceFormEditModel item = new ServiceFormEditModel();

            string sql = @"
                SELECT S.SRV_ID, S.SC_ID, S.NAME, F.SRV_FIELD, F.SRV_SCRIPT, F.PRE_SCRIPT
                FROM SERVICE S
                LEFT OUTER JOIN SERVICE_FORM F ON S.SRV_ID = F.SRV_ID
                WHERE S.SRV_ID = @SRV_ID
            ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                int n = 0;
                if (dr.Read())
                {
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.CategoryId = DataUtils.GetDBInt(dr, n++);
                    item.ServiceName = DataUtils.GetDBString(dr, n++);
                    item.ServiceField = DataUtils.GetDBString(dr, n++);
                    item.ServiceScript = DataUtils.GetDBString(dr, n++);
                    item.PreviewScript = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        public bool Update(ServiceFormEditModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string updateSQL = @"
                if exists (select * from service_form where srv_id = @SRV_ID)
                begin
                    update service_form set 
                        srv_field = @SRV_FIELD,
                        srv_script = @SRV_SCRIPT,
                        pre_script = @PRE_SCRIPT,
                        upd_time = GETDATE(),
                        upd_fun_cd = @FUN_CD,
                        upd_acc = @UPD_ACC
                    where srv_id = @SRV_ID
                end
                else
                begin
                    insert into service_form (
                        srv_id, srv_field, srv_script, pre_script,
                        upd_time, upd_fun_cd, upd_acc, add_time, add_fun_cd, add_acc
                    ) values (
                        @SRV_ID, @SRV_FIELD, @SRV_SCRIPT, @PRE_SCRIPT,
                        GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                    )
                end
            ";

            args.Add("SRV_ID", model.ServiceId);
            args.Add("SRV_FIELD", model.ServiceField);
            args.Add("SRV_SCRIPT", model.ServiceScript);
            args.Add("PRE_SCRIPT", model.PreviewScript);
            args.Add("FUN_CD", "ADM_SRVFORM");
            args.Add("UPD_ACC", model.UpdateAccount);

            int flag = Update(updateSQL, args);

            if (flag == 1) return true;

            return false;
        }
    }
}