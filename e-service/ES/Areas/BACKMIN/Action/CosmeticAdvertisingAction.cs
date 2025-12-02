using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Areas.Admin.Models;
using System.Text;
using System.Data.SqlClient;
using ES.Utils;
using ES.Action;

namespace ES.Areas.Admin.Action
{
    public class CosmeticAdvertisingAction : BaseAction
    {
        /// <summary>
        /// 化妝品廣告內容管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public CosmeticAdvertisingAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 化妝品廣告內容管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public CosmeticAdvertisingAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<CosmeticAdvertisingModel> GetList(CosmeticAdvertisingActionModel model)
        {
            List<CosmeticAdvertisingModel> list = new List<CosmeticAdvertisingModel>();

            StringBuilder countSQL = new StringBuilder(@"SELECT COUNT(1) FROM COSMETIC_ADV A WHERE DEL_MK = 'N' ");

            StringBuilder querySQL = new StringBuilder(@"
                    SELECT ADV_ID, ADV_WORD
                    FROM COSMETIC_ADV WHERE DEL_MK = 'N' AND ADV_ID IN (
                        SELECT TOP @PAGE_SIZE ADV_ID FROM (
                            SELECT TOP @END_COUNT ADV_ID FROM COSMETIC_ADV
                            WHERE DEL_MK = 'N' ");

            if (!String.IsNullOrEmpty(model.Word))
            {
                countSQL.Append("AND ADV_WORD LIKE '%' + @ADV_WORD + '%'");
                querySQL.Append("AND ADV_WORD LIKE '%' + @ADV_WORD + '%'");
            }

            querySQL.Append(@" ORDER BY ADV_ID ASC ) T ORDER BY ADV_ID DESC
                    ) ORDER BY ADV_ID ASC ");

            // 計算筆數
            SqlCommand cmd1 = new SqlCommand(countSQL.ToString(), conn);

            if (!String.IsNullOrEmpty(model.Word))
            {
                DataUtils.AddParameters(cmd1, "ADV_WORD", model.Word);
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

            if (!String.IsNullOrEmpty(model.Word))
            {
                DataUtils.AddParameters(cmd2, "ADV_WORD", model.Word);
            }

            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    CosmeticAdvertisingModel item = new CosmeticAdvertisingModel();
                    item.AdvId = DataUtils.GetDBInt(dr, 0);
                    item.Word = DataUtils.GetDBString(dr, 1);
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        public CosmeticAdvertisingEditModel GetData(int advId)
        {
            CosmeticAdvertisingEditModel item = new CosmeticAdvertisingEditModel();

            string sql = @" SELECT ADV_ID, ADV_WORD
                            FROM COSMETIC_ADV
                            WHERE DEL_MK = 'N'
                                AND ADV_ID = @ADV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "ADV_ID", advId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.AdvId = DataUtils.GetDBInt(dr, n++);
                    item.Word = DataUtils.GetDBString(dr, n++);
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
        public bool Insert(CosmeticAdvertisingEditModel model)
        {
            string sql = @" INSERT INTO COSMETIC_ADV (
                                ADV_ID, ADV_WORD,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                ISNULL((SELECT MAX(ADV_ID) FROM COSMETIC_ADV), 0)+1, @ADV_WORD,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "ADV_WORD", model.Word);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-COSADV");
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
        public bool Update(CosmeticAdvertisingEditModel model)
        {
            string sql = @" UPDATE COSMETIC_ADV SET
                                ADV_WORD = @ADV_WORD,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE ADV_ID = @ADV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "ADV_WORD", model.Word);
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-COSADV");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "ADV_ID", model.AdvId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(CosmeticAdvertisingActionModel model)
        {
            string sql = @" UPDATE COSMETIC_ADV SET
                                DEL_MK = 'Y',
                                DEL_TIME = GETDATE(),
                                DEL_FUN_CD = @FUN_CD,
                                DEL_ACC = @UPD_ACC,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE ADV_ID = @ADV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-COSADV");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "ADV_ID", model.ActionId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}