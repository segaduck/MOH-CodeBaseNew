using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class ServiceRuleAction : ServiceAction
    {
        /// <summary>
        /// 案件稽催設定
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceRuleAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 案件稽催設定
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceRuleAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得角色列表
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetRuleList(int categoryId)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string,object>>();
            Dictionary<String, Object> args = new Dictionary<string, object>();

            string sql = @"
                SELECT S.SRV_ID, S.SC_ID, S.NAME, R.SRV_ID AS SRV_RULE_ID,
                    R.RULE_A_MK, R.RULE_A_DAY, R.RULE_B_MK, R.RULE_B_DAY,
                    R.RULE_C_MK, R.NOTIFY_COUNT, R.RESET_MK, R.HEAD_MK, R.HEAD_ACC
                FROM SERVICE S
                    LEFT JOIN SERVICE_RULE R ON S.SRV_ID = R.SRV_ID AND R.DEL_MK = 'N'
                WHERE S.DEL_MK = 'N' AND S.SC_ID = @SC_ID";

            args.Add("SC_ID", categoryId);

            return GetList(sql, args);
        }

        /// <summary>
        /// 取得新增資訊
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceRuleEditModel GetNew(string serviceId)
        {
            ServiceRuleEditModel item = new ServiceRuleEditModel();

            string sql = @" 
                SELECT S.SRV_ID, S.NAME, S.SC_ID, C.UNIT_CD
                FROM SERVICE S, SERVICE_CATE C
                WHERE S.SC_ID = C.SC_ID
                    AND S.SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.ServiceName = DataUtils.GetDBString(dr, n++);
                    item.CategoryId = DataUtils.GetDBInt(dr, n++);
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 取得修改資訊
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceRuleEditModel GetEdit(string serviceId)
        {
            ServiceRuleEditModel item = new ServiceRuleEditModel();

            string sql = @"
                SELECT S.SRV_ID, S.NAME, S.SC_ID, C.UNIT_CD,
                    RULE_A_MK, RULE_A_DAY, RULE_B_MK, RULE_B_DAY,
                    RULE_C_MK, NOTIFY_COUNT, RESET_MK, HEAD_MK, HEAD_ACC
                FROM SERVICE S, SERVICE_CATE C, SERVICE_RULE R
                WHERE S.SC_ID = C.SC_ID
                    AND S.SRV_ID = R.SRV_ID
                    AND S.SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.ServiceId = DataUtils.GetDBString(dr, n++);
                    item.ServiceName = DataUtils.GetDBString(dr, n++);
                    item.CategoryId = DataUtils.GetDBInt(dr, n++);
                    item.UnitCode = DataUtils.GetDBInt(dr, n++);
                    item.RuleAMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.RuleADay = DataUtils.GetDBInt(dr, n++);
                    item.RuleBMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.RuleBDay = DataUtils.GetDBInt(dr, n++);
                    item.RuleCMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.NotifyCount = DataUtils.GetDBInt(dr, n++);
                    item.ResetMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.HeadMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.HeadAccount = DataUtils.GetDBString(dr, n++);
                    item.InitSet();
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 取得預設稽催設定
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ServiceRuleEditModel GetSetup(string serviceId)
        {
            ServiceRuleEditModel item = new ServiceRuleEditModel();

            string sql = @" SELECT RULE_A_MK, RULE_A_DAY, RULE_B_MK, RULE_B_DAY,
                                RULE_C_MK, NOTIFY_COUNT, RESET_MK, HEAD_MK, HEAD_ACC
                            FROM SERVICE_RULE R
                            WHERE SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    item.ServiceId = serviceId;
                    item.RuleAMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.RuleADay = DataUtils.GetDBInt(dr, n++);
                    item.RuleBMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.RuleBDay = DataUtils.GetDBInt(dr, n++);
                    item.RuleCMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.NotifyCount = DataUtils.GetDBInt(dr, n++);
                    item.ResetMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.HeadMark = DataUtils.GetDBString(dr, n++).Equals("Y");
                    item.HeadAccount = DataUtils.GetDBString(dr, n++);
                    item.InitSet();
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 稽催設定 - 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(ServiceRuleEditModel model)
        {
            string sql1 = @"DELETE SERVICE_RULE WHERE DEL_MK = 'Y' AND SRV_ID = @SRV_ID";


            string sql2 = @" INSERT INTO SERVICE_RULE (
                                SRV_ID, RULE_A_MK, RULE_A_DAY, RULE_B_MK, RULE_B_DAY,
                                RULE_C_MK, NOTIFY_COUNT, RESET_MK, HEAD_MK, HEAD_ACC,
                                UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                            ) VALUES (
                                @SRV_ID, @RULE_A_MK, @RULE_A_DAY, @RULE_B_MK, @RULE_B_DAY,
                                @RULE_C_MK, @NOTIFY_COUNT, @RESET_MK, @HEAD_MK, @HEAD_ACC,
                                GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                            )";

            SqlCommand cmd1 = new SqlCommand(sql1, conn, tran);
            DataUtils.AddParameters(cmd1, "SRV_ID", model.ServiceId);
            cmd1.ExecuteNonQuery();

            SqlCommand cmd2 = new SqlCommand(sql2, conn, tran);

            DataUtils.AddParameters(cmd2, "SRV_ID", model.ServiceId);
            DataUtils.AddParameters(cmd2, "RULE_A_MK", model.RuleAMark);
            DataUtils.AddParameters(cmd2, "RULE_A_DAY", model.RuleADay);
            DataUtils.AddParameters(cmd2, "RULE_B_MK", model.RuleBMark);
            DataUtils.AddParameters(cmd2, "RULE_B_DAY", model.RuleBDay);

            DataUtils.AddParameters(cmd2, "RULE_C_MK", model.RuleCMark);
            DataUtils.AddParameters(cmd2, "NOTIFY_COUNT", model.NotifyCount);
            DataUtils.AddParameters(cmd2, "RESET_MK", model.ResetMark);
            DataUtils.AddParameters(cmd2, "HEAD_MK", model.HeadMark);
            DataUtils.AddParameters(cmd2, "HEAD_ACC", model.HeadAccount);

            DataUtils.AddParameters(cmd2, "FUN_CD", "ADM-RULE");
            DataUtils.AddParameters(cmd2, "UPD_ACC", model.UpdateAccount);

            int flag = cmd2.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 稽催設定 - 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(ServiceRuleEditModel model)
        {

            string sql = @" UPDATE SERVICE_RULE SET
                                RULE_A_MK = @RULE_A_MK,
                                RULE_A_DAY = @RULE_A_DAY,
                                RULE_B_MK = @RULE_B_MK,
                                RULE_B_DAY = @RULE_B_DAY,
                                RULE_C_MK = @RULE_C_MK,
                                NOTIFY_COUNT = @NOTIFY_COUNT,
                                RESET_MK = @RESET_MK,
                                HEAD_MK = @HEAD_MK,
                                HEAD_ACC = @HEAD_ACC,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "RULE_A_MK", model.RuleAMark);
            DataUtils.AddParameters(cmd, "RULE_A_DAY", model.RuleADay);
            DataUtils.AddParameters(cmd, "RULE_B_MK", model.RuleBMark);
            DataUtils.AddParameters(cmd, "RULE_B_DAY", model.RuleBDay);

            DataUtils.AddParameters(cmd, "RULE_C_MK", model.RuleCMark);
            DataUtils.AddParameters(cmd, "NOTIFY_COUNT", model.NotifyCount);
            DataUtils.AddParameters(cmd, "RESET_MK", model.ResetMark);
            DataUtils.AddParameters(cmd, "HEAD_MK", model.HeadMark);
            DataUtils.AddParameters(cmd, "HEAD_ACC", model.HeadAccount);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-FILE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 稽催設定 - 刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(ServiceRuleEditModel model)
        {

            string sql = @" UPDATE SERVICE_RULE SET
                                DEL_MK = 'Y',
                                DEL_TIME = GETDATE(),
                                DEL_FUN_CD = @FUN_CD,
                                DEL_ACC = @UPD_ACC,
                                UPD_TIME = GETDATE(),
                                UPD_FUN_CD = @FUN_CD,
                                UPD_ACC = @UPD_ACC
                            WHERE SRV_ID = @SRV_ID";


            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-FILE");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

            DataUtils.AddParameters(cmd, "SRV_ID", model.ServiceId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }
    }
}