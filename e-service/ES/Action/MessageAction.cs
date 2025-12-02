using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using log4net;
using ES.Models;
using System.Text;
using ES.Utils;
using ES.Commons;
namespace ES.Action
{
    public class MessageAction : BaseAction
    {
        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public MessageAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public MessageAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetList(MessageActionModel model)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            Dictionary<string, object> args = new Dictionary<string, object>();

            string _sql = @"
    SELECT A.MSG_ID, A.TITLE, A.TIME_S, A.TIME_E,
    ROW_NUMBER() OVER (ORDER BY A.MSG_ID DESC) AS _ROW_NO,
    A.MESSAGE_TYPE
    FROM   MESSAGE A
    WHERE  A.DEL_MK = 'N' and A.TIME_E > GETDATE()";

            model.Title = MyCommonUtil.SqlInjection(model.Title);
            if (!String.IsNullOrEmpty(model.Title))
            {
                _sql += " AND A.TITLE LIKE '%" + model.Title + "%'";
            }

            if (model.StartTime != null || model.EndTime != null)
            {
                if (model.StartTime != null)
                {
                    _sql += " AND convert(varchar, A.TIME_S, 111) >= '" + model.StartTime + "'";
                }

                if (model.EndTime != null)
                {
                    _sql += " AND convert(varchar, A.TIME_E, 111) <= '" + model.EndTime + "'";
                }
            }
            if (!String.IsNullOrEmpty(model.MESSAGE_TYPE))
            {
                switch (model.MESSAGE_TYPE)
                {
                    case "P":
                        _sql += " AND A.MESSAGE_TYPE = 'P'";
                        break;
                    case "I":
                        _sql += " AND A.MESSAGE_TYPE = 'I'";
                        break;
                    case "G":
                        _sql += " AND A.MESSAGE_TYPE = 'G'";
                        break;
                    case "S":
                        _sql += " AND A.MESSAGE_TYPE = 'S'";
                        break;
                }
            }

            list = GetList(_sql.ToString(), model.NowPage, args);

            return list;
        }

        /// <summary>
        /// 取得單筆資料
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public MessageShowModel GetData(int messageId)
        {
            MessageShowModel item = new MessageShowModel();

            string sql = @" 
    SELECT MSG_ID, UNIT_CD, CATEGORY, TITLE, CONTENT, TIME_S, TIME_E
    ,FILENAME_1 as FileName1, FILENAME_2 as FileName2, FILENAME_3 as FileName3, SEND_MAIL_MK
    ,CLS_SUB_CD, CLS_ADM_CD, CLS_SRV_CD, KEYWORD
    ,(SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = M.UNIT_CD) AS UNIT_NAME
    ,MESSAGE_TYPE
    FROM MESSAGE M
    WHERE DEL_MK = 'N'
    AND MSG_ID = @MSG_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "MSG_ID", messageId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.MessageID = DataUtils.GetDBInt(dr, n++);
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.Category = DataUtils.GetDBStringArray(dr, n++, ", ");
                    item.Title = DataUtils.GetDBString(dr, n++);
                    item.Content = DataUtils.GetDBString(dr, n++);
                    item.StartTime = DataUtils.GetDBDateTime(dr, n++);
                    item.EndTime = DataUtils.GetDBDateTime(dr, n++);
                    item.FileName1 = DataUtils.GetDBString(dr, n++);
                    item.FileName2 = DataUtils.GetDBString(dr, n++);
                    item.FileName3 = DataUtils.GetDBString(dr, n++);
                    item.SendMailMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.ClassSubCode = DataUtils.GetDBString(dr, n++);
                    item.ClassAdmCode = DataUtils.GetDBString(dr, n++);
                    item.ClassSrvCode = DataUtils.GetDBString(dr, n++);
                    item.KeyWord = DataUtils.GetDBString(dr, n++);
                    item.UnitName = DataUtils.GetDBString(dr, n++);
                    item.MESSAGE_TYPE = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return item;
        }
    }
}