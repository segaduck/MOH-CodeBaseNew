using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Action;
using ES.Areas.Admin.Models;
using System.Text;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class CosmeticIngredientsAction : BaseAction
    {
        /// <summary>
        /// 一般化妝品成分管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public CosmeticIngredientsAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 一般化妝品成分管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public CosmeticIngredientsAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<CosmeticIngredientsModel> GetList(CosmeticIngredientsActionModel model)
        {
            List<CosmeticIngredientsModel> list = new List<CosmeticIngredientsModel>();

            StringBuilder countSQL = new StringBuilder(@"SELECT COUNT(1) FROM COSMETIC_ING A WHERE DEL_MK = 'N' ");

            StringBuilder querySQL = new StringBuilder(@"
                    SELECT COS_ID, COS_CONTENT, COS_TYPE, COS_NUM_1, COS_NUM_2 
                    FROM COSMETIC_ING WHERE DEL_MK = 'N' AND COS_ID IN (
                        SELECT TOP @PAGE_SIZE COS_ID FROM (
                            SELECT TOP @END_COUNT COS_ID FROM COSMETIC_ING
                            WHERE DEL_MK = 'N' ");

            if (!String.IsNullOrEmpty(model.Content))
            {
                countSQL.Append("AND COS_CONTENT LIKE '%' + @ADV_WORD + '%'");
                querySQL.Append("AND COS_CONTENT LIKE '%' + @ADV_WORD + '%'");
            }

            querySQL.Append(@" ORDER BY COS_ID ASC ) T ORDER BY COS_ID DESC
                    ) ORDER BY COS_ID ASC ");

            // 計算筆數
            SqlCommand cmd1 = new SqlCommand(countSQL.ToString(), conn);

            if (!String.IsNullOrEmpty(model.Content))
            {
                DataUtils.AddParameters(cmd1, "COS_CONTENT", model.Content);
            }

            using (SqlDataReader dr = cmd1.ExecuteReader())
            {
                if (dr.Read())
                {
                    this.totalCount = DataUtils.GetDBInt(dr, 0);
                }
                dr.Close();
            }

            // 查詢資料
            querySQL.Replace("@PAGE_SIZE", GetPageSize(model.NowPage));
            querySQL.Replace("@END_COUNT", GetEndCount(model.NowPage));

            SqlCommand cmd2 = new SqlCommand(querySQL.ToString(), conn);

            if (!String.IsNullOrEmpty(model.Content))
            {
                DataUtils.AddParameters(cmd2, "ADV_WORD", model.Content);
            }

            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    CosmeticIngredientsModel item = new CosmeticIngredientsModel();
                    item.CosmeticId = DataUtils.GetDBInt(dr, n++);
                    item.Content = DataUtils.GetDBString(dr, n++);
                    item.Type = DataUtils.GetDBString(dr, n++);
                    item.Number1 = DataUtils.GetDBDecimal(dr, n++);
                    item.Number2 = DataUtils.GetDBDecimal(dr, n++);

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="cosmeticId"></param>
        /// <returns></returns>
        public CosmeticIngredientsEditModel GetData(int cosmeticId)
        {
            CosmeticIngredientsEditModel item = new CosmeticIngredientsEditModel();

            string sql = @" SELECT COS_ID, COS_CONTENT, COS_USED, COS_TYPE, COS_NUM_1, COS_NUM_2, COS_NOTE,
                                CONVERT(CHAR(10), UPD_TIME, 111) + ' ' +   CONVERT(CHAR(8), UPD_TIME, 108) AS UPD_TIME, UPD_ACC
                            FROM COSMETIC_ING
                            WHERE DEL_MK = 'N'
                                AND COS_ID = @COS_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "COS_ID", cosmeticId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.CosmeticId = DataUtils.GetDBInt(dr, n++);
                    item.Content = DataUtils.GetDBString(dr, n++);
                    item.Used = DataUtils.GetDBString(dr, n++);
                    item.Type = DataUtils.GetDBString(dr, n++);
                    item.Number1 = DataUtils.GetDBDecimal(dr, n++);
                    item.Number2 = DataUtils.GetDBDecimal(dr, n++);
                    item.Note = DataUtils.GetDBString(dr, n++);
                    item.UpdateTime = DataUtils.GetDBString(dr, n++);
                    item.UpdateAccount = DataUtils.GetDBString(dr, n++);
                    item.InitSet();
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
        public bool Insert(CosmeticIngredientsEditModel model)
        {
            string sql = @" INSERT INTO COSMETIC_ING (
                                COS_ID, COS_CONTENT, COS_USED, COS_TYPE, COS_NUM_1, COS_NUM_2, COS_NOTE,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                ISNULL((SELECT MAX(COS_ID) FROM COSMETIC_ING), 0)+1,
                                @COS_CONTENT, @COS_USED, @COS_TYPE, @COS_NUM_1, @COS_NUM_2, @COS_NOTE,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "COS_CONTENT", model.Content);
            DataUtils.AddParameters(cmd, "COS_USED", model.Used);
            DataUtils.AddParameters(cmd, "COS_TYPE", model.Type);
            DataUtils.AddParameters(cmd, "COS_NUM_1", model.Number1);
            DataUtils.AddParameters(cmd, "COS_NUM_2", model.Number2);
            DataUtils.AddParameters(cmd, "COS_NOTE", model.Note);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-COSING");
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
        public bool Update(CosmeticIngredientsEditModel model)
        {
            string sql = @" UPDATE COSMETIC_ING SET
                                COS_CONTENT = @COS_CONTENT,
                                COS_USED = @COS_USED,
                                COS_TYPE = @COS_TYPE,
                                COS_NUM_1 = @COS_NUM_1,
                                COS_NUM_2 = @COS_NUM_2,
                                COS_NOTE = @COS_NOTE,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE COS_ID = @COS_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "COS_CONTENT", model.Content);
            DataUtils.AddParameters(cmd, "COS_USED", model.Used);
            DataUtils.AddParameters(cmd, "COS_TYPE", model.Type);
            DataUtils.AddParameters(cmd, "COS_NUM_1", model.Number1);
            DataUtils.AddParameters(cmd, "COS_NUM_2", model.Number2);
            DataUtils.AddParameters(cmd, "COS_NOTE", model.Note);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-COSING");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "COS_ID", model.CosmeticId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(CosmeticIngredientsActionModel model)
        {
            string sql = @" UPDATE COSMETIC_ING SET
                                DEL_MK = 'Y',
                                DEL_TIME = GETDATE(),
                                DEL_FUN_CD = @FUN_CD,
                                DEL_ACC = @UPD_ACC,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE COS_ID = @COS_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-COSING");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "COS_ID", model.ActionId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}