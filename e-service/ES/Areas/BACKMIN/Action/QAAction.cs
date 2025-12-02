using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Utils;
using System.Data.SqlClient;
using log4net;
using ES.Areas.Admin.Models;
using ES.Action;
using System.Text;

namespace ES.Areas.Admin.Action
{
    public class QAAction : BaseAction
    {
        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public QAAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public QAAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<QAModel> GetList(QAActionModel model)
        {
            List<QAModel> list = new List<QAModel>();

            StringBuilder countSQL = new StringBuilder(@"SELECT COUNT(1) FROM QA WHERE DEL_MK = 'N' ");

            StringBuilder querySQL = new StringBuilder(@"
    SELECT QAID, TITLE ,CONTENT, SEQ
    FROM QA 
    WHERE DEL_MK = 'N'");

            if (!String.IsNullOrEmpty(model.Title))
            {
                countSQL.Append("AND TITLE LIKE '%' + @TITLE + '%'");
                querySQL.Append("AND TITLE LIKE '%' + @TITLE + '%'");
            }
            querySQL.Append(@" ORDER BY SEQ ");

            // 計算筆數
            SqlCommand cmd1 = new SqlCommand(countSQL.ToString(), conn);

            if (!String.IsNullOrEmpty(model.Title))
            {
                DataUtils.AddParameters(cmd1, "TITLE", model.Title);
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

            if (!String.IsNullOrEmpty(model.Title))
            {
                DataUtils.AddParameters(cmd2, "TITLE", model.Title);
            }
            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    //int n = 0;
                    QAModel item = new QAModel()
                    {
                        QAID = DataUtils.GetDBInt(dr, 0),
                        Title = DataUtils.GetDBString(dr, 1),
                        Content = DataUtils.GetDBString(dr, 2)
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
        /// <param name="QAId"></param>
        /// <returns></returns>
        public QAEditModel GetData(int QAId)
        {
            QAEditModel item = new QAEditModel();

            string sql = @" 
     SELECT QAID, TITLE ,CONTENT, SEQ
    FROM QA 
    WHERE DEL_MK = 'N'
    AND QAID = @QAID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "QAID", QAId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.QAID = DataUtils.GetDBInt(dr, n++);
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.Content = DataUtils.GetDBString(dr, n++);
                    item.SEQ = DataUtils.GetDBInt(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="QAId"></param>
        /// <returns></returns>
        public string GetDataNewID()
        {
            string item = string.Empty;

            string sql = @" 
                    SELECT TOP(1) QAID
                    FROM QA
                    WHERE 1=1
                    and QAID <> @QAID
                    ORDER BY QAID DESC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "QAID", "0");
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
        public bool Insert(QAEditModel model)
        {
            string sql = @" INSERT INTO QA (
                                QAID, TITLE, CONTENT,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC, SEQ
                            ) VALUES (
                                ISNULL((SELECT MAX(QAID) FROM QA), 0)+1,
                                @TITLE, @CONTENT,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC, @SEQ
                            )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "CONTENT", model.Content);
            DataUtils.AddParameters(cmd, "SEQ", model.SEQ);
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-QA");
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
        public bool Update(QAEditModel model)
        {
            string sql = @" UPDATE QA SET
                            TITLE = @TITLE,
                            CONTENT = @CONTENT,
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC,
                            SEQ = @SEQ";

            sql += @" WHERE QAID = @QAID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "CONTENT", model.Content);
            DataUtils.AddParameters(cmd, "SEQ", model.SEQ);
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-QA");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "QAID", model.QAID);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(QAActionModel model)
        {
            string sql = @" UPDATE QA SET
    DEL_MK = 'Y',
    DEL_TIME = GETDATE(),
    DEL_FUN_CD = @FUN_CD,
    DEL_ACC = @UPD_ACC,
    UPD_TIME = GETDATE(),
    UPD_FUN_CD = @FUN_CD,
    UPD_ACC = @UPD_ACC
    WHERE QAID = @QAID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-QA");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "QAID", model.ActionId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}