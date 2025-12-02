using ES.Action;
using ES.Areas.Admin.Models;
using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace ES.Areas.Admin.Action
{
    public class ServiceDateAction : BaseAction
    {
        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceDateAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceDateAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<ServiceDateModel> GetList(ServiceDateActionModel model)
        {
            List<ServiceDateModel> list = new List<ServiceDateModel>();
            StringBuilder querySQL = new StringBuilder(@"
    SELECT SD_ID, TITLE, TIME_S, TIME_E
    FROM SERVICE_DATE 
    WHERE DEL_MK = 'N' "); 
            // 查詢資料
            SqlCommand cmd2 = new SqlCommand(querySQL.ToString(), conn);
            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    //int n = 0;
                    ServiceDateModel item = new ServiceDateModel()
                    {
                        ServiceDateID = DataUtils.GetDBInt(dr, 0),
                        Title = DataUtils.GetDBString(dr, 1),
                        StartTime = DataUtils.GetDBDateTime(dr, 2),
                        EndTime = DataUtils.GetDBDateTime(dr, 3),
                    };
                    list.Add(item);
                }
                dr.Close();
            }
            return list;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="ServiceDateId"></param>
        /// <returns></returns>
        public ServiceDateEditModel GetData(int ServiceDateId)
        {
            ServiceDateEditModel item = new ServiceDateEditModel();
            string sql = @" 
    SELECT SD_ID, TITLE, TIME_S, TIME_E
    FROM SERVICE_DATE
    WHERE DEL_MK = 'N'
    AND SD_ID = @SD_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SD_ID", ServiceDateId);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.ServiceDateID = DataUtils.GetDBInt(dr, n++);
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.StartTime = DataUtils.GetDBDateTime(dr, n++);
                    item.EndTime = DataUtils.GetDBDateTime(dr, n++);
                }
                dr.Close();
            }
            return item;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="SD_ID"></param>
        /// <returns></returns>
        public string GetDataNewID()
        {
            string item = string.Empty;
            string sql = @" 
                    SELECT TOP(1) SD_ID
                    FROM SERVICE_DATE
                    WHERE 1=1
                    and SD_ID <> @SD_ID
                    ORDER BY SD_ID DESC";
            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SD_ID", "0");
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    var orgId = DataUtils.GetDBInt(dr, n++);
                    item = (orgId + 1).ToString();
                }
                dr.Close();
            }
            return item;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(ServiceDateEditModel model)
        {
            string sql = @" INSERT INTO SERVICE_DATE (
                                SD_ID, TITLE, TIME_S, TIME_E,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                ISNULL((SELECT MAX(SD_ID) FROM ServiceDate), 0)+1,
                                @TITLE, @TIME_S, @TIME_E,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";
            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "TIME_S", model.StartTime);
            DataUtils.AddParameters(cmd, "TIME_E", model.EndTime);
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-MSG");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            int flag = cmd.ExecuteNonQuery();
            if (flag == 1) return true;
            return false;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(ServiceDateEditModel model)
        {
            string sql = @" UPDATE SERVICE_DATE SET
                            TITLE = @TITLE,
                            TIME_S = @TIME_S,
                            TIME_E = @TIME_E,    
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC";

            sql += @" WHERE SD_ID = @SD_ID";
            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "TIME_S", model.StartTime);
            DataUtils.AddParameters(cmd, "TIME_E", model.EndTime);
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-MSG");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "SD_ID", model.ServiceDateID);
            int flag = cmd.ExecuteNonQuery();
            if (flag == 1) return true;
            return false;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(ServiceDateActionModel model)
        {
            string sql = @" UPDATE SERVICE_DATE SET
    DEL_MK = 'Y',
    DEL_TIME = GETDATE(),
    DEL_FUN_CD = @FUN_CD,
    DEL_ACC = @UPD_ACC,
    UPD_TIME = GETDATE(),
    UPD_FUN_CD = @FUN_CD,
    UPD_ACC = @UPD_ACC
    WHERE SD_ID = @SD_ID";
            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-MSG");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "SD_ID", model.ActionId);
            int flag = cmd.ExecuteNonQuery();
            if (flag == 1) return true;
            return false;
        }
    }
}