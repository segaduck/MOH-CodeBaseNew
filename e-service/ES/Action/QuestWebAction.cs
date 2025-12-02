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
    public class QuestWebAction : BaseAction
    {
        /// <summary>
        /// 線上滿意度調查
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public QuestWebAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 線上滿意度調查
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public QuestWebAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public bool Insert(QuestWebModel model)
        {
            string sql = @"
    INSERT INTO SURVEY_ONLINE (
        SRL_NO, 
        UNIT_NAME, 
        SRV_NAME, 
        SATISFIED, 
        UNIT_CD, SRV_ID, APPLY_ID, RECOMMEND, SATISFIED_SCORE,
        Q1, Q2, Q3, Q4, Q5,
        UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
    ) VALUES (
        (SELECT @DATE + REPLICATE('0',5-LEN(SRL)) + SRL
         FROM (
	        SELECT CAST(ISNULL(CAST(SUBSTRING(MAX(SRL_NO), 6, 10)+1 AS INT), 1) AS VARCHAR(5)) AS SRL
	        FROM SURVEY_ONLINE WHERE SRL_NO LIKE @DATE + '%'
         ) T),
        (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = @UNIT_CD),
        (SELECT NAME FROM SERVICE WHERE SRV_ID = @SRV_ID),
        (SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = @SATISFIED_SCORE AND CODE_KIND = 'SURVEY_SCORE'),
        @UNIT_CD, @SRV_ID, @APPLY_ID, @RECOMMEND, @SATISFIED_SCORE,
        @Q1, @Q2, @Q3, @Q4, @Q5,
        GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @ADD_ACC
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

            DataUtils.AddParameters(cmd, "FUN_CD", "WEB-QWEB");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "ADD_ACC", model.UpdateAccount);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        public QuestWebModel GetData(QuestWebModel model)
        {
            QuestWebModel item = null;

            string sql = @"
    SELECT APP_ID, SRV_ID, UNIT_CD
    ,(SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = A.UNIT_CD) AS UNIT_NAME
    ,(CASE WHEN (SELECT COUNT(1) FROM SURVEY_ONLINE WHERE APPLY_ID = A.APP_ID) = 0 THEN 'N' ELSE 'Y' END) AS EXISTS_MK
    ,ACC_NO
    FROM APPLY A
    WHERE DEL_MK = 'N'
    AND ACC_NO = @ACC_NO
    AND APP_ID = @APP_ID ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ACC_NO", model.ACC_NO);
            cmd.Parameters.AddWithValue("APP_ID", model.ApplyId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    item = new QuestWebModel();
                    int n = 0;
                    item.ApplyId = DataUtils.GetDBString(dr, n++);
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.UnitName = DataUtils.GetDBString(dr, n++);
                    item.ExistsMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.ACC_NO = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        public bool CheckApplyIdExists(QuestWebModel model)
        {
            string sql = @"SELECT APPLY_ID FROM SURVEY_ONLINE WHERE APPLY_ID = @APPLY_ID";
            var rst = false;
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("APPLY_ID", model.ApplyId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    rst= true;
                }
                dr.Close();
            }

            return rst;
        }
    }
}