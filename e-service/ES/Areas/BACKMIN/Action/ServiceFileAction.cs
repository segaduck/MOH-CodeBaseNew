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
    public class ServiceFileAction : ServiceAction
    {
        /// <summary>
        /// 書表下載管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceFileAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 書表下載管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceFileAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 書表下載管理 - 檔案列表
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetList(string serviceId)
        {
            List<Dictionary<string, object>> list = null;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string sql = @"
                SELECT SRV_ID, FILE_ID, TITLE, FILENAME, SEQ_NO
                FROM SERVICE_FILE
                WHERE DEL_MK = 'N' AND SRV_ID = @SRV_ID
            ";

            args.Add("SRV_ID", serviceId);

            list = GetList(sql, args);

            return list;
        }

        /// <summary>
        /// 書表下載管理 - 新增 (取得資料)
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceFileNewModel GetNew(string serviceId)
        {

            ServiceFileNewModel item = new ServiceFileNewModel();

            string sql = @"SELECT S.SRV_ID, S.NAME,
                            (SELECT ISNULL((MAX(SEQ_NO)/10)*10,0)+10 FROM SERVICE_FILE WHERE SRV_ID = S.SRV_ID) AS SEQ_NO
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
                    item.FileType = "F";
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 書表下載管理 - 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(ServiceFileNewModel model)
        {
            string sql = @"INSERT INTO SERVICE_FILE (
                                SRV_ID, FILE_ID, TITLE, FILENAME, SEQ_NO,
                                FILE_TYPE_CD, FILE_URL,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                @SRV_ID, ISNULL((SELECT MAX(FILE_ID) FROM SERVICE_FILE WHERE SRV_ID = @SRV_ID), 0)+1, @TITLE, @FILENAME, @SEQ_NO,
                                @FILE_TYPE_CD, @FILE_URL,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "FILENAME", model.FileName);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FILE_TYPE_CD", model.FileType);
            DataUtils.AddParameters(cmd, "FILE_URL", model.FileUrl);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-FILE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 書表下載管理 - 修改 (取得資料)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ServiceFileEditModel GetEdit(ServiceFileEditModel model)
        {
            ServiceFileEditModel item = new ServiceFileEditModel();

            string sql = @"
                SELECT S.SRV_ID, S.NAME, F.FILE_ID, F.TITLE, F.FILENAME, F.SEQ_NO,
                    F.FILE_TYPE_CD, F.FILE_URL
                FROM SERVICE S, SERVICE_FILE F
                WHERE S.SRV_ID = F.SRV_ID
                    AND S.SRV_ID = @SRV_ID
                    AND F.FILE_ID = @FILE_ID
            ";

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
                    item.FileType = DataUtils.GetDBString(dr, n++);
                    item.FileUrl = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 書表下載管理 - 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(ServiceFileEditModel model)
        {

            string sql = @"
                UPDATE SERVICE_FILE SET
                    TITLE = @TITLE,                    
                    SEQ_NO = @SEQ_NO,
                    FILE_TYPE_CD = @FILE_TYPE_CD,
                    FILE_URL = @FILE_URL,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC";

            if (model.FileName != null) sql += @" ,FILENAME = @FILENAME";
            sql += @" WHERE SRV_ID = @SRV_ID
                      AND FILE_ID = @FILE_ID ";


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            if (model.FileName != null) DataUtils.AddParameters(cmd, "FILENAME", model.FileName);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FILE_TYPE_CD", model.FileType);
            DataUtils.AddParameters(cmd, "FILE_URL", model.FileUrl);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-FILE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd, "FILE_ID", model.FileId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 書表下載管理 - 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(ServiceFileEditModel model)
        {

            string sql = @"
                UPDATE SERVICE_FILE SET
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