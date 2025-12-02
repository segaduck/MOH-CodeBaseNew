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
    public class ServiceHelpAction : ServiceAction
    {
        /// <summary>
        /// 操作輔助說明管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceHelpAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 操作輔助說明管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceHelpAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 操作輔助說明管理 - 檔案列表
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetList(string serviceId)
        {
            List<Dictionary<string, object>> list = null;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string sql = @"
                SELECT SRV_ID, FILE_ID, TITLE, FILENAME, SEQ_NO
                FROM SERVICE_HELP
                WHERE DEL_MK = 'N' AND SRV_ID = @SRV_ID
            ";

            args.Add("SRV_ID", serviceId);

            list = GetList(sql, args);

            return list;
        }

        /// <summary>
        /// 操作輔助說明管理 - 新增 (取得資料)
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceHelpNewModel GetNew(string serviceId)
        {

            ServiceHelpNewModel item = new ServiceHelpNewModel();

            string sql = @"SELECT S.SRV_ID, S.NAME,
                            (SELECT ISNULL((MAX(SEQ_NO)/10)*10,0)+10 FROM SERVICE_HELP WHERE SRV_ID = S.SRV_ID) AS SEQ_NO
                            FROM SERVICE S
                            WHERE S.SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.ServiceName = DataUtils.GetDBString(dr, n++);
                    item.Seq = DataUtils.GetDBInt(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 操作輔助說明管理 - 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(ServiceHelpNewModel model)
        {
            string sql = @"INSERT INTO SERVICE_HELP (
                                SRV_ID, FILE_ID, TITLE, FILENAME, SEQ_NO,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                @SRV_ID, ISNULL((SELECT MAX(FILE_ID) FROM SERVICE_FILE WHERE SRV_ID = @SRV_ID), 0)+1, @TITLE, @FILENAME, @SEQ_NO,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "FILENAME", model.FileName);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-FILE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 操作輔助說明管理 - 修改 (取得資料)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ServiceHelpEditModel GetEdit(ServiceHelpEditModel model)
        {
            ServiceHelpEditModel item = new ServiceHelpEditModel();

            string sql = @"SELECT S.SRV_ID, S.NAME, F.FILE_ID, F.TITLE, F.FILENAME, F.SEQ_NO
                            FROM SERVICE S, SERVICE_HELP F
                            WHERE S.SRV_ID = F.SRV_ID
                                AND S.SRV_ID = @SRV_ID
                                AND F.FILE_ID = @FILE_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd, "FILE_ID", model.FileId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                int n = 0;
                if (dr.Read())
                {
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.ServiceName = DataUtils.GetDBString(dr, n++);
                    item.FileId = DataUtils.GetDBInt(dr, n++);
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.FileName = DataUtils.GetDBString(dr, n++);
                    item.Seq = DataUtils.GetDBInt(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 操作輔助說明管理 - 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(ServiceHelpEditModel model)
        {

            string sql = @"UPDATE SERVICE_HELP SET
                                TITLE = @TITLE,
                                FILENAME = ISNULL(@FILENAME, FILENAME),
                                SEQ_NO = @SEQ_NO,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE SRV_ID = @SRV_ID
                                AND FILE_ID = @FILE_ID";


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "FILENAME", model.FileName);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-FILE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd, "FILE_ID", model.FileId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 操作輔助說明管理 - 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(ServiceHelpEditModel model)
        {

            string sql = @"
                UPDATE SERVICE_HELP SET
                    DEL_MK = 'Y',
                    DEL_TIME = GETDATE(),
                    DEL_FUN_CD = @FUN_CD,
                    DEL_ACC = @UPD_ACC,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE SRV_ID = @SRV_ID
                    AND FILE_ID = @FILE_ID";


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-FILE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd, "FILE_ID", model.FileId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}