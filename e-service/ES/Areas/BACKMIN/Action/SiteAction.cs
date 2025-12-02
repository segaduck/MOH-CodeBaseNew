using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Data.SqlClient;
using ES.Action;
using ES.Areas.Admin.Models;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class SiteAction : BaseAction
    {
        /// <summary>
        /// 相關網站連結
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public SiteAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 相關網站連結
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

            string sql = @" SELECT SITE_ID, SITE_NAME, SITE_URL, SEQ_NO
                            FROM SITE
                            WHERE DEL_MK = 'N'
                            ORDER BY SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    SiteModel item = new SiteModel();
                    item.SiteId = DataUtils.GetDBInt(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.Url = DataUtils.GetDBString(dr, n++);
                    item.Seq = DataUtils.GetDBInt(dr, n++);

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 新增用
        /// </summary>
        /// <returns></returns>
        public SiteEditModel GetNewModel()
        {
            SiteEditModel item = new SiteEditModel();

            string sql = @"SELECT ISNULL((MAX(SEQ_NO)/10)*10,0)+10 FROM SITE WHERE DEL_MK = 'N'";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.Seq = DataUtils.GetDBInt(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public SiteEditModel GetData(int siteId)
        {
            SiteEditModel item = new SiteEditModel();

            string sql = @" SELECT SITE_ID, SITE_NAME, SITE_URL, SEQ_NO
                            FROM SITE WHERE SITE_ID = @SITE_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SITE_ID", siteId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.SiteId = DataUtils.GetDBInt(dr, n++);
                    item.Name = DataUtils.GetDBString(dr, n++);
                    item.Url = DataUtils.GetDBString(dr, n++);
                    item.Seq = DataUtils.GetDBInt(dr, n++);
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
        public bool Insert(SiteEditModel model)
        {
            string sql = @" INSERT INTO SITE (
                                SITE_ID, SITE_NAME, SITE_URL, SEQ_NO,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                ISNULL((SELECT MAX(SITE_ID) FROM SITE), 0)+1,
                                @SITE_NAME, @SITE_URL, @SEQ_NO,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "SITE_NAME", model.Name);
            DataUtils.AddParameters(cmd, "SITE_URL", model.Url);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SITE");
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
        public bool Update(SiteEditModel model)
        {
            string sql = @" UPDATE SITE SET
                                SITE_NAME = @SITE_NAME,
                                SITE_URL = @SITE_URL,
                                SEQ_NO = @SEQ_NO,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE SITE_ID = @SITE_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "SITE_NAME", model.Name);
            DataUtils.AddParameters(cmd, "SITE_URL", model.Url);
            DataUtils.AddParameters(cmd, "SEQ_NO", model.Seq);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SITE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "SITE_ID", model.SiteId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(SiteEditModel model)
        {
            string sql = @" UPDATE SITE SET
                                DEL_MK = 'Y',
                                DEL_TIME = GETDATE(),
                                DEL_FUN_CD = @FUN_CD,
                                DEL_ACC = @UPD_ACC,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE SITE_ID = @SITE_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-SITE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "SITE_ID", model.SiteId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}