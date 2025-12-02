using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    /// <summary>
    /// 同意書管理
    /// </summary>
    public class ServiceAgreementAction : ServiceAction
    {
        /// <summary>
        /// 同意書管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceAgreementAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 同意書管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceAgreementAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得同意書資料
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceAgreementEditModel GetEdit(string serviceId)
        {

            ServiceAgreementEditModel item = new ServiceAgreementEditModel();

            string sql = @"SELECT S.SRV_ID, S.SC_ID, S.NAME, A.TITLE, A.CONTENT
                            FROM SERVICE S
                            LEFT OUTER JOIN SERVICE_AGREE A ON S.SRV_ID = A.SRV_ID
                            WHERE S.SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item.ServiceId = DataUtils.GetDBString(dr, 0);
                    item.CategoryId = DataUtils.GetDBInt(dr, 1);
                    item.ServiceName = DataUtils.GetDBString(dr, 2);
                    item.Title = DataUtils.GetDBString(dr, 3);
                    item.Content = DataUtils.GetDBString(dr, 4);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 更新同意書資料
        /// </summary>
        /// <param name="model"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        public bool Update(ServiceAgreementEditModel model)
        {
            string sql1 = @"SELECT SRV_ID FROM SERVICE_AGREE WHERE SRV_ID = @SRV_ID";

            string sql2 = @"UPDATE SERVICE_AGREE SET 
                                TITLE = @TITLE,
                                CONTENT = @CONTENT,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE SRV_ID = @SRV_ID";

            string sql3 = @"INSERT INTO SERVICE_AGREE (
                                SRV_ID, TITLE, CONTENT,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                @SRV_ID, @TITLE, @CONTENT,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";

            SqlCommand cmd1 = new SqlCommand(sql1, conn, tran);
            DataUtils.AddParameters(cmd1, "SRV_ID", model.ServiceId);

            bool isExists = false;
            int flag = -1;

            using (SqlDataReader dr = cmd1.ExecuteReader())
            {
                if (dr.Read())
                {
                    isExists = true;
                }
                dr.Close();
            }

            cmd1.Dispose();

            if (isExists)
            {
                SqlCommand cmd2 = new SqlCommand(sql2, conn, tran);
                DataUtils.AddParameters(cmd2, "TITLE", model.Title);
                DataUtils.AddParameters(cmd2, "CONTENT", model.Content);
                DataUtils.AddParameters(cmd2, "FUN_CD", "ADM-AGREE");
                DataUtils.AddParameters(cmd2, "UPD_ACC", model.UpdateAccount);
                DataUtils.AddParameters(cmd2, "SRV_ID", model.ServiceId);
                flag = cmd2.ExecuteNonQuery();
            }
            else
            {
                SqlCommand cmd3 = new SqlCommand(sql3, conn, tran);
                DataUtils.AddParameters(cmd3, "SRV_ID", model.ServiceId);
                DataUtils.AddParameters(cmd3, "TITLE", model.Title);
                DataUtils.AddParameters(cmd3, "CONTENT", model.Content);
                DataUtils.AddParameters(cmd3, "FUN_CD", "ADM-AGREE");
                DataUtils.AddParameters(cmd3, "UPD_ACC", model.UpdateAccount);
                flag = cmd3.ExecuteNonQuery();
            }

            if (flag == 1) return true;

            return false;
        }
    }
}