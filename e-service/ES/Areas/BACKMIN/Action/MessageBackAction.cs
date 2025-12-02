using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using ES.Areas.Admin.Models;
using System.Data.SqlClient;
using System.Text;
using ES.Utils;
using log4net;

namespace ES.Areas.Admin.Action
{
    public class MessageBackAction : BaseAction
    {
        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public MessageBackAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public MessageBackAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<MessageBackModel> GetList(MessageBackActionModel model)
        {
            List<MessageBackModel> list = new List<MessageBackModel>();

            StringBuilder countSQL = new StringBuilder(@"SELECT COUNT(1) FROM MESSAGEBack WHERE DEL_MK = 'N' ");

            StringBuilder querySQL = new StringBuilder(@"
    SELECT MSG_ID, TITLE, TIME_S, TIME_E
    ,SEND_MAIL_MK
    ,MESSAGE_TYPE
    FROM MESSAGEBack 
    WHERE DEL_MK = 'N' 
    AND MSG_ID IN (
    SELECT TOP @PAGE_SIZE MSG_ID FROM (
    SELECT TOP @END_COUNT MSG_ID FROM MESSAGEBack
    WHERE DEL_MK = 'N' ");

            if (!String.IsNullOrEmpty(model.Title))
            {
                countSQL.Append("AND TITLE LIKE '%' + @TITLE + '%'");
                querySQL.Append("AND TITLE LIKE '%' + @TITLE + '%'");
            }


            if (model.StartTime != null || model.EndTime != null)
            {
                countSQL.Append("AND ( 1=2 OR ");
                querySQL.Append("AND ( 1=2 OR ");
                if (model.StartTime != null)
                {
                    //CAST(FLOOR(CAST(TIME_E AS FLOAT)) AS DATETIME) 
                    countSQL.Append(" CAST(FLOOR(CAST(TIME_E AS FLOAT)) AS DATETIME) >= @TIME_S ");
                    querySQL.Append(" CAST(FLOOR(CAST(TIME_E AS FLOAT)) AS DATETIME) >= @TIME_S ");
                }
                if (model.EndTime != null)
                {
                    if (model.StartTime != null)
                    {
                        countSQL.Append("AND  CAST(FLOOR(CAST(TIME_S AS FLOAT)) AS DATETIME)  <= @TIME_E ");
                        querySQL.Append("AND  CAST(FLOOR(CAST(TIME_S AS FLOAT)) AS DATETIME)  <= @TIME_E ");
                    }
                    else
                    {
                        countSQL.Append(" CAST(FLOOR(CAST(TIME_S AS FLOAT)) AS DATETIME) <= @TIME_E ");
                        querySQL.Append(" CAST(FLOOR(CAST(TIME_S AS FLOAT)) AS DATETIME) <= @TIME_E ");
                    }
                }
                countSQL.Append(") ");
                querySQL.Append(") ");
            }

            querySQL.Append(@" ORDER BY MSG_ID DESC ) T ORDER BY MSG_ID ASC
                    ) ORDER BY MSG_ID DESC ");

            // 計算筆數
            SqlCommand cmd1 = new SqlCommand(countSQL.ToString(), conn);

            if (!String.IsNullOrEmpty(model.Title))
            {
                DataUtils.AddParameters(cmd1, "TITLE", model.Title);
            }
            if (model.StartTime != null)
            {
                DataUtils.AddParameters(cmd1, "TIME_S", (DateTime)model.StartTime);
            }
            if (model.EndTime != null)
            {
                DataUtils.AddParameters(cmd1, "TIME_E", (DateTime)model.EndTime);
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
            if (model.StartTime != null)
            {
                DataUtils.AddParameters(cmd2, "TIME_S", (DateTime)model.StartTime);
            }
            if (model.EndTime != null)
            {
                DataUtils.AddParameters(cmd2, "TIME_E", (DateTime)model.EndTime);
            }

            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    //int n = 0;
                    MessageBackModel item = new MessageBackModel()
                    {
                        MSG_ID = DataUtils.GetDBInt(dr, 0),
                        Title = DataUtils.GetDBString(dr, 1),
                        StartTime = DataUtils.GetDBDateTime(dr, 2),
                        EndTime = DataUtils.GetDBDateTime(dr, 3),
                        //SendMailMark = DataUtils.GetDBString(dr, 4).Equals("Y")
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
        /// <param name="messageId"></param>
        /// <returns></returns>
        public MessageBackEditModel GetData(int messageId)
        {
            MessageBackEditModel item = new MessageBackEditModel();

            string sql = @" 
    SELECT MSG_ID, UNIT_CD, CATEGORY, TITLE, CONTENT, TIME_S, TIME_E,
    FILENAME_1, FILENAME_2, FILENAME_3, SEND_MAIL_MK,
    CLS_SUB_CD, CLS_ADM_CD, CLS_SRV_CD, KEYWORD, MESSAGE_TYPE
    FROM MESSAGEBack
    WHERE DEL_MK = 'N'
    AND MSG_ID = @MSG_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "MSG_ID", messageId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.MSG_ID = DataUtils.GetDBInt(dr, n++);
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.Category = DataUtils.GetDBStringArray(dr, n++, ", ");
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.Content = DataUtils.GetDBString(dr, n++);
                    item.StartTime = DataUtils.GetDBDateTime(dr, n++);
                    item.EndTime = DataUtils.GetDBDateTime(dr, n++);
                    item.FileName1 = DataUtils.GetDBString(dr, n++);
                    item.FileName2 = DataUtils.GetDBString(dr, n++);
                    item.FileName3 = DataUtils.GetDBString(dr, n++);
                    //item.SendMailMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    //item.ClassSubCode = DataUtils.GetDBString(dr, n++);
                    //item.ClassAdmCode = DataUtils.GetDBString(dr, n++);
                    //item.ClassSrvCode = DataUtils.GetDBString(dr, n++);
                    //item.KeyWord = DataUtils.GetDBString(dr, n++);
                    item.MESSAGE_TYPE = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return item;
        }
        public MessageBackShowModel GetDataToShow(int messageId)
        {
            MessageBackShowModel item = new MessageBackShowModel();

            string sql = @" 
    SELECT MSG_ID, UNIT_CD, CATEGORY, TITLE, CONTENT, TIME_S, TIME_E,
    FILENAME_1, FILENAME_2, FILENAME_3, SEND_MAIL_MK,
    CLS_SUB_CD, CLS_ADM_CD, CLS_SRV_CD, KEYWORD, MESSAGE_TYPE
    FROM MESSAGEBack
    WHERE DEL_MK = 'N'
    AND TIME_S<=GETDATE()
	AND TIME_E>=GETDATE()	
    AND MSG_ID = @MSG_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "MSG_ID", messageId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.MSG_ID = DataUtils.GetDBInt(dr, n++);
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.Category = DataUtils.GetDBStringArray(dr, n++, ", ");
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.Content = DataUtils.GetDBString(dr, n++);
                    item.StartTime = DataUtils.GetDBDateTime(dr, n++);
                    item.EndTime = DataUtils.GetDBDateTime(dr, n++);
                    item.FileName1 = DataUtils.GetDBString(dr, n++);
                    item.FileName2 = DataUtils.GetDBString(dr, n++);
                    item.FileName3 = DataUtils.GetDBString(dr, n++);            
                    item.MESSAGE_TYPE = DataUtils.GetDBString(dr, n++);
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
        public bool Insert(MessageBackEditModel model)
        {
            string sql = @" INSERT INTO MESSAGEBack (
                                MSG_ID, UNIT_CD, CATEGORY, TITLE, CONTENT, TIME_S, TIME_E,
                                FILENAME_1, FILENAME_2, FILENAME_3, SEND_MAIL_MK,
                                CLS_SUB_CD, CLS_ADM_CD, CLS_SRV_CD, KEYWORD, MESSAGE_TYPE,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                ISNULL((SELECT MAX(MSG_ID) FROM MESSAGEBack), 0)+1,
                                @UNIT_CD, @CATEGORY, @TITLE, @CONTENT, @TIME_S, @TIME_E,
                                @FILENAME_1, @FILENAME_2, @FILENAME_3, @SEND_MAIL_MK,
                                @CLS_SUB_CD, @CLS_ADM_CD, @CLS_SRV_CD, @KEYWORD, @MESSAGE_TYPE,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);
            DataUtils.AddParameters(cmd, "CATEGORY", DataUtils.StringArrayToString(model.Category, ", "));
            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "CONTENT", model.Content);
            DataUtils.AddParameters(cmd, "TIME_S", model.StartTime);
            DataUtils.AddParameters(cmd, "TIME_E", model.EndTime);

            DataUtils.AddParameters(cmd, "FILENAME_1", model.FileName1);
            DataUtils.AddParameters(cmd, "FILENAME_2", model.FileName2);
            DataUtils.AddParameters(cmd, "FILENAME_3", model.FileName3);
            DataUtils.AddParameters(cmd, "SEND_MAIL_MK", "N");

            DataUtils.AddParameters(cmd, "CLS_SUB_CD", null);
            DataUtils.AddParameters(cmd, "CLS_ADM_CD", null);
            DataUtils.AddParameters(cmd, "CLS_SRV_CD", null);
            DataUtils.AddParameters(cmd, "KEYWORD", null);
            DataUtils.AddParameters(cmd, "MESSAGE_TYPE", model.MESSAGE_TYPE);

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
        public bool Update(MessageBackEditModel model)
        {
            string sql = @" UPDATE MESSAGEBack SET
    UNIT_CD = @UNIT_CD,
    CATEGORY = @CATEGORY,
    TITLE = @TITLE,
    CONTENT = @CONTENT,
    TIME_S = @TIME_S,
    TIME_E = @TIME_E,
    FILENAME_1 = @FILENAME_1,
    FILENAME_2 = @FILENAME_2,
    FILENAME_3 = @FILENAME_3,
    SEND_MAIL_MK = @SEND_MAIL_MK,
    CLS_SUB_CD = @CLS_SUB_CD,
    CLS_ADM_CD = @CLS_ADM_CD,
    CLS_SRV_CD = @CLS_SRV_CD,
    KEYWORD = @KEYWORD,
    MESSAGE_TYPE = @MESSAGE_TYPE,
    UPD_TIME = GETDATE(),
    UPD_FUN_CD = @FUN_CD,
    UPD_ACC = @UPD_ACC
    WHERE MSG_ID = @MSG_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);
            DataUtils.AddParameters(cmd, "CATEGORY", DataUtils.StringArrayToString(model.Category, ", "));
            DataUtils.AddParameters(cmd, "TITLE", model.Title);
            DataUtils.AddParameters(cmd, "CONTENT", model.Content);
            DataUtils.AddParameters(cmd, "TIME_S", model.StartTime);
            DataUtils.AddParameters(cmd, "TIME_E", model.EndTime);

            DataUtils.AddParameters(cmd, "FILENAME_1", model.FileName1);
            DataUtils.AddParameters(cmd, "FILENAME_2", model.FileName2);
            DataUtils.AddParameters(cmd, "FILENAME_3", model.FileName3);
            DataUtils.AddParameters(cmd, "SEND_MAIL_MK", "N");

            DataUtils.AddParameters(cmd, "CLS_SUB_CD", null);
            DataUtils.AddParameters(cmd, "CLS_ADM_CD", null);
            DataUtils.AddParameters(cmd, "CLS_SRV_CD", null);
            DataUtils.AddParameters(cmd, "KEYWORD", null);
            DataUtils.AddParameters(cmd, "MESSAGE_TYPE", model.MESSAGE_TYPE);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-MSG");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "MSG_ID", model.MSG_ID);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(MessageBackActionModel model)
        {
            string sql = @" UPDATE MESSAGEBack SET
    DEL_MK = 'Y',
    DEL_TIME = GETDATE(),
    DEL_FUN_CD = @FUN_CD,
    DEL_ACC = @UPD_ACC,
    UPD_TIME = GETDATE(),
    UPD_FUN_CD = @FUN_CD,
    UPD_ACC = @UPD_ACC
    WHERE MSG_ID = @MSG_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-MSG");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "MSG_ID", model.ActionId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}