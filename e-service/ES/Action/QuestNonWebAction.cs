using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Data.SqlClient;
using ES.Models;
using ES.Utils;

namespace ES.Action
{
    public class QuestNonWebAction : BaseAction
    {
        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public QuestNonWebAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public QuestNonWebAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public bool Insert(QuestNonWebModel model)
        {
            string sql = @"INSERT INTO SURVEY_OFFLINE (
                    SRL_NO, UNIT_NAME, SRV_NAME, SATISFIED,
                    UNIT_CD, SRV_ID, APPLY_ID, RECOMMEND, SATISFIED_SCORE,
                    Q1, Q2, Q3, Q4, Q5,
                    UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                ) VALUES (
                    (SELECT ISNULL(MAX(Convert(varchar,Convert(int,SRL_NO)+1)), @DATE + '00001') FROM SURVEY_OFFLINE WHERE SRL_NO LIKE @DATE + '%'),
                    (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = @UNIT_CD),
                    (SELECT NAME FROM SERVICE WHERE SRV_ID = @SRV_ID),
                    (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = @SATISFIED_SCORE AND CODE_KIND = 'SURVEY_SCORE'),
                    @UNIT_CD, @SRV_ID, @APPLY_ID, @RECOMMEND, @SATISFIED_SCORE,
                    @Q1, @Q2, @Q3, @Q4, @Q5,
                    GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                )";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "DATE", (DateTime.Now.Year - 1911).ToString() + DateTime.Now.Month.ToString("D2"));
            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);
            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd, "SATISFIED_SCORE", model.Satisfied);
            DataUtils.AddParameters(cmd, "APPLY_ID", model.ApplyId);
            DataUtils.AddParameters(cmd, "RECOMMEND", model.Recommend);

            DataUtils.AddParameters(cmd, "Q1", model.Q1);
            DataUtils.AddParameters(cmd, "Q2", model.Q2);
            DataUtils.AddParameters(cmd, "Q3", model.Q3);
            DataUtils.AddParameters(cmd, "Q4", model.Q4);
            DataUtils.AddParameters(cmd, "Q5", model.Q5);

            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-QNON");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        public bool CheckApplyIdExists(QuestNonWebModel model)
        {
            string sql = @"SELECT APPLY_ID FROM SURVEY_OFFLINE WHERE APPLY_ID = @APPLY_ID";
            var rst = false;
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("APPLY_ID", model.ApplyId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    rst = true;
                }
                dr.Close();
            }
            return rst;
        }
    }
}