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
    /// 申請須知管理
    /// </summary>
    public class ServiceNoticeAction : ServiceAction
    {
        /// <summary>
        /// 申請須知管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceNoticeAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 申請須知管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceNoticeAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得申請須知資料
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceNoticeEditModel GetEdit(string serviceId)
        {

            ServiceNoticeEditModel item = new ServiceNoticeEditModel();

            string sql = @"SELECT S.SRV_ID, S.SC_ID, S.NAME, N.COMP_DESC,
                                N.TITLE_1, N.CONTENT_1, N.TITLE_2, N.CONTENT_2, N.TITLE_3, N.CONTENT_3,
                                N.TITLE_4, N.CONTENT_4, N.TITLE_5, N.CONTENT_5, N.TITLE_6, N.CONTENT_6
                            FROM SERVICE S
                            LEFT OUTER JOIN SERVICE_NOTICE N ON S.SRV_ID = N.SRV_ID
                            WHERE S.SRV_ID = @SRV_ID";

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
                    item.CompleteDesc = DataUtils.GetDBString(dr, n++);
                    item.Title1 = DataUtils.GetDBString(dr, n++);
                    item.Content1 = DataUtils.GetDBString(dr, n++);
                    item.Title2 = DataUtils.GetDBString(dr, n++);
                    item.Content2 = DataUtils.GetDBString(dr, n++);
                    item.Title3 = DataUtils.GetDBString(dr, n++);
                    item.Content3 = DataUtils.GetDBString(dr, n++);
                    item.Title4 = DataUtils.GetDBString(dr, n++);
                    item.Content4 = DataUtils.GetDBString(dr, n++);
                    item.Title5 = DataUtils.GetDBString(dr, n++);
                    item.Content5 = DataUtils.GetDBString(dr, n++);
                    item.Title6 = DataUtils.GetDBString(dr, n++);
                    item.Content6 = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        public bool Update(ServiceNoticeEditModel model)
        {
            string sql1 = @"SELECT SRV_ID FROM SERVICE_NOTICE WHERE SRV_ID = @SRV_ID";

            string sql2 = @"UPDATE SERVICE_NOTICE SET
                                COMP_DESC = @COMP_DESC,
                                TITLE_1 = @TITLE_1,
                                CONTENT_1 = @CONTENT_1,
                                TITLE_2 = @TITLE_2,
                                CONTENT_2 = @CONTENT_2,
                                TITLE_3 = @TITLE_3,
                                CONTENT_3 = @CONTENT_3,
                                TITLE_4 = @TITLE_4,
                                CONTENT_4 = @CONTENT_4,
                                TITLE_5 = @TITLE_5,
                                CONTENT_5 = @CONTENT_5,
                                TITLE_6 = @TITLE_6,
                                CONTENT_6 = @CONTENT_6,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE SRV_ID = @SRV_ID";

            string sql3 = @"INSERT INTO SERVICE_NOTICE (
                                SRV_ID, COMP_DESC,
                                TITLE_1, CONTENT_1, TITLE_2, CONTENT_2, TITLE_3, CONTENT_3,
                                TITLE_4, CONTENT_4, TITLE_5, CONTENT_5, TITLE_6, CONTENT_6,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                @SRV_ID, @COMP_DESC,
                                @TITLE_1, @CONTENT_1, @TITLE_2, @CONTENT_2, @TITLE_3, @CONTENT_3,
                                @TITLE_4, @CONTENT_4, @TITLE_5, @CONTENT_5, @TITLE_6, @CONTENT_6,
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
                DataUtils.AddParameters(cmd2, "COMP_DESC", model.CompleteDesc);
                DataUtils.AddParameters(cmd2, "TITLE_1", model.Title1);
                DataUtils.AddParameters(cmd2, "CONTENT_1", model.Content1);
                DataUtils.AddParameters(cmd2, "TITLE_2", model.Title2);
                DataUtils.AddParameters(cmd2, "CONTENT_2", model.Content2);
                DataUtils.AddParameters(cmd2, "TITLE_3", model.Title3);
                DataUtils.AddParameters(cmd2, "CONTENT_3", model.Content3);
                DataUtils.AddParameters(cmd2, "TITLE_4", model.Title4);
                DataUtils.AddParameters(cmd2, "CONTENT_4", model.Content4);
                DataUtils.AddParameters(cmd2, "TITLE_5", model.Title5);
                DataUtils.AddParameters(cmd2, "CONTENT_5", model.Content5);
                DataUtils.AddParameters(cmd2, "TITLE_6", model.Title6);
                DataUtils.AddParameters(cmd2, "CONTENT_6", model.Content6);
                DataUtils.AddParameters(cmd2, "FUN_CD", "ADM-NOTICE");
                DataUtils.AddParameters(cmd2, "UPD_ACC", model.UpdateAccount);
                DataUtils.AddParameters(cmd2, "SRV_ID", model.ServiceId);
                flag = cmd2.ExecuteNonQuery();
            }
            else
            {
                SqlCommand cmd3 = new SqlCommand(sql3, conn, tran);
                DataUtils.AddParameters(cmd3, "SRV_ID", model.ServiceId);
                DataUtils.AddParameters(cmd3, "COMP_DESC", model.CompleteDesc);
                DataUtils.AddParameters(cmd3, "TITLE_1", model.Title1);
                DataUtils.AddParameters(cmd3, "CONTENT_1", model.Content1);
                DataUtils.AddParameters(cmd3, "TITLE_2", model.Title2);
                DataUtils.AddParameters(cmd3, "CONTENT_2", model.Content2);
                DataUtils.AddParameters(cmd3, "TITLE_3", model.Title3);
                DataUtils.AddParameters(cmd3, "CONTENT_3", model.Content3);
                DataUtils.AddParameters(cmd3, "TITLE_4", model.Title4);
                DataUtils.AddParameters(cmd3, "CONTENT_4", model.Content4);
                DataUtils.AddParameters(cmd3, "TITLE_5", model.Title5);
                DataUtils.AddParameters(cmd3, "CONTENT_5", model.Content5);
                DataUtils.AddParameters(cmd3, "TITLE_6", model.Title6);
                DataUtils.AddParameters(cmd3, "CONTENT_6", model.Content6);
                DataUtils.AddParameters(cmd3, "FUN_CD", "ADM-NOTICE");
                DataUtils.AddParameters(cmd3, "UPD_ACC", model.UpdateAccount);
                flag = cmd3.ExecuteNonQuery();
            }

            if (flag == 1) return true;

            return false;
        }
    }
}