using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Utils;
using log4net;
using System.Data.SqlClient;
using System.Text;
using ES.Areas.Admin.Models;
using ES.Action;
using GemBox.Spreadsheet;


namespace ES.Areas.Admin.Action
{
    public class AccountAction : BaseAction
    {
        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public AccountAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 單位管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public AccountAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得列表
        /// </summary>
        /// <returns></returns>
        public List<AccountModel> GetList(AccountQueryModel model)
        {
            List<AccountModel> list = new List<AccountModel>();

            StringBuilder countSQL = new StringBuilder(@"SELECT COUNT(1) FROM ADMIN A WHERE DEL_MK = 'N' ");


            StringBuilder querySQL = new StringBuilder();

            querySQL.Append(@"SELECT ACC_NO, NAME, (
                                SELECT UNIT_NAME + (CASE 
                                    WHEN UNIT_LEVEL = 2 THEN (SELECT ' (' + UNIT_NAME + ')' FROM UNIT WHERE UNIT_CD = U.UNIT_PCD) 
                                    ELSE '' END) 
                                FROM UNIT U WHERE UNIT_CD = A.UNIT_CD) AS UNIT_NAME, case A.DEL_MK when 'Y' then '(已停用)' else '(啟用中)' end as Stops
                            FROM ADMIN A WHERE DEL_MK = 'N' AND ACC_NO IN (
                                SELECT TOP @PAGE_SIZE ACC_NO FROM (
                                    SELECT TOP @END_COUNT ACC_NO FROM ADMIN A WHERE DEL_MK = 'N' ");

            if (!String.IsNullOrEmpty(model.Account))
            {
                querySQL.Append("AND ACC_NO = @ACC_NO ");
                countSQL.Append("AND ACC_NO = @ACC_NO ");
            }

            if (!String.IsNullOrEmpty(model.Name))
            {
                querySQL.Append("AND NAME = @NAME ");
                countSQL.Append("AND NAME = @NAME ");
            }

            if (!String.IsNullOrEmpty(model.UnitCode))
            {
                querySQL.Append("AND UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE UNIT_CD = @UNIT_CD OR UNIT_PCD = @UNIT_CD) ");
                countSQL.Append("AND UNIT_CD IN (SELECT UNIT_CD FROM UNIT WHERE UNIT_CD = @UNIT_CD OR UNIT_PCD = @UNIT_CD) ");
            }


            querySQL.Append(@"      ORDER BY ACC_NO ASC
                                ) T
                                ORDER BY ACC_NO DESC
                            )
                            ORDER BY ACC_NO ASC");

            // 計算筆數
            SqlCommand cmd1 = new SqlCommand(countSQL.ToString(), conn);

            if (!String.IsNullOrEmpty(model.Account))
            {
                DataUtils.AddParameters(cmd1, "ACC_NO", model.Account);
            }

            if (!String.IsNullOrEmpty(model.Name))
            {
                DataUtils.AddParameters(cmd1, "NAME", model.Name);
            }

            if (!String.IsNullOrEmpty(model.UnitCode))
            {
                DataUtils.AddParameters(cmd1, "UNIT_CD", model.UnitCode);
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

            //logger.Debug("nowPage: " + model.NowPage + " / total: " + this.totalCount + " / pageSize: " + GetPageSize(model.NowPage) + " / endCount: " + GetEndCount(model.NowPage));

            SqlCommand cmd2 = new SqlCommand(querySQL.ToString(), conn);

            if (!String.IsNullOrEmpty(model.Account))
            {
                DataUtils.AddParameters(cmd2, "ACC_NO", model.Account);
            }

            if (!String.IsNullOrEmpty(model.Name))
            {
                DataUtils.AddParameters(cmd2, "NAME", model.Name);
            }

            if (!String.IsNullOrEmpty(model.UnitCode))
            {
                DataUtils.AddParameters(cmd2, "UNIT_CD", model.UnitCode);
            }

            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                while (dr.Read())
                {
                    AccountModel item = new AccountModel();
                    item.Account = DataUtils.GetDBString(dr, 0);
                    item.Name = DataUtils.GetDBString(dr, 1);
                    item.UnitName = DataUtils.GetDBString(dr, 2);
                    item.Stops = DataUtils.GetDBString(dr, 3);
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        public AccountEditModel GetAccount(string account)
        {
            AccountEditModel model = new AccountEditModel();

            string sql = @"SELECT ACC_NO, NAME, UNIT_CD, TEL, MAIL,
                                ADMIN_SCOPE, 
                                CONVERT(CHAR(10), LEVEL_UPD_TIME, 111) + ' ' +  CONVERT(CHAR(8), LEVEL_UPD_TIME, 108) AS LEVEL_UPD_TIME,
                                CONVERT(CHAR(10), UPD_TIME, 111) + ' ' +   CONVERT(CHAR(8), UPD_TIME, 108) AS UPD_TIME,
                                UPD_ACC
                            FROM ADMIN WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);

            DataUtils.AddParameters(cmd, "ACC_NO", account);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;
                    model.Account = DataUtils.GetDBString(dr, n++);
                    model.Name = DataUtils.GetDBString(dr, n++);
                    model.UnitCode = DataUtils.GetDBInt(dr, n++).ToString();
                    model.Tel = DataUtils.GetDBString(dr, n++);
                    model.Mail = DataUtils.GetDBString(dr, n++);

                    model.Scope = DataUtils.GetDBInt(dr, n++);
                    model.LevelUpdateTime = DataUtils.GetDBString(dr, n++);
                    model.UpdateTime = DataUtils.GetDBString(dr, n++);
                    model.UpdateAccount = DataUtils.GetDBString(dr, n++);
                }
                dr.Close();
            }

            return model;
        }

        public bool AccountExists(string account)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("ACC_NO", account);
            string sql = @"SELECT ACC_NO FROM ADMIN WHERE ACC_NO = @ACC_NO ";

            return (GetList(sql, args).Count() == 1);
        }
        public bool AccountStops(string account)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("ACC_NO", account);
            string sql = @"SELECT ACC_NO FROM ADMIN WHERE ACC_NO = @ACC_NO and isnull(DEL_MK,'N')='Y' ";

            return (GetList(sql, args).Count() == 1);
        }

        public bool CDCExists(string account, string cdcpass)
        {
            var psd256 = DataUtils.Crypt256(cdcpass);
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("ACC_NO", account);
            args.Add("ACC_PSWD", psd256);
            string sql = @"SELECT ACC_NO FROM ADMIN_CDC WHERE ACC_NO = @ACC_NO AND ACC_PSWD = @ACC_PSWD";

            return (GetList(sql, args).Count() == 1);
        }

        public bool AccountisLock(string account)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("@LOGIN_ID", account);
            string _sql = @"select top 1 *
                            from LOGIN_LOG
                            where 1 = 1";
            _sql += " and LOGIN_ID = @LOGIN_ID";
            _sql += " and status = 'A' and fail_count >= 5 and getdate() < DATEADD(minute,15,login_time) ";
            _sql += " order by LOGIN_TIME desc";

            return (GetList(_sql, args).Count() == 1);
        }

        /// <summary>
        /// 取得登入者帳號資訊
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public AccountModel GetAccountModel(string account)
        {
            AccountModel model = new AccountModel();

            string querySQL1 = @"
                SELECT ACC_NO, NAME, UNIT_CD, ADMIN_SCOPE,
                    (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = A.UNIT_CD) AS UNIT_NAME,
                    (SELECT (CASE WHEN A.ADMIN_SCOPE = 0 AND UNIT_LEVEL = 2 THEN UNIT_PCD ELSE UNIT_CD END) FROM UNIT WHERE UNIT_CD =  A.UNIT_CD) AS SCOPE_UNIT_CD,
                    (SELECT (CASE WHEN UNIT_LEVEL = 2 THEN UNIT_PCD ELSE UNIT_CD END) FROM UNIT WHERE UNIT_CD =  A.UNIT_CD) AS SRV_UNIT_CD
                FROM ADMIN A
                WHERE ACC_NO = @ACC_NO
            ";

            SqlCommand cmd1 = new SqlCommand(querySQL1, conn, tran);
            DataUtils.AddParameters(cmd1, "ACC_NO", account);
            using (SqlDataReader dr = cmd1.ExecuteReader())
            {
                if (dr.Read())
                {
                    int n = 0;

                    model.Account = DataUtils.GetDBString(dr, n++);
                    model.Name = DataUtils.GetDBString(dr, n++);
                    model.UnitCode = DataUtils.GetDBInt(dr, n++).ToString();
                    model.Scope = DataUtils.GetDBInt(dr, n++);
                    model.UnitName = DataUtils.GetDBString(dr, n++);
                    model.ScopeUnitCode = DataUtils.GetDBInt(dr, n++);
                    model.ServiceUnitCode = DataUtils.GetDBInt(dr, n++);
                }
                dr.Close();
            }

            string querySQL2 = @"SELECT MN_ID FROM ADMIN_LEVEL WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd2 = new SqlCommand(querySQL2, conn, tran);
            DataUtils.AddParameters(cmd2, "ACC_NO", account);
            using (SqlDataReader dr = cmd2.ExecuteReader())
            {
                List<int> list = new List<int>();

                while (dr.Read())
                {
                    list.Add(DataUtils.GetDBInt(dr, 0));
                }

                model.LevelList = list;
                dr.Close();
            }

            return model;
        }

        /// <summary>
        /// 新增管理帳號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Insert(AccountNewModel model)
        {
            string deleteSQL = @"
                DELETE ADMIN WHERE DEL_MK = 'Y' AND ACC_NO = @ACC_NO
            ";

            using (SqlCommand cmd = new SqlCommand(deleteSQL, conn, tran))
            {
                DataUtils.AddParameters(cmd, "ACC_NO", model.Account);
                cmd.ExecuteNonQuery();
            }


            string sql = @"INSERT INTO ADMIN (
                            ACC_NO, NAME, UNIT_CD, TEL, MAIL, ADMIN_SCOPE,
                            UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                        ) VALUES (
                            @ACC_NO, @NAME, @UNIT_CD, @TEL, @MAIL, 0,
                            GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                        )";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {

                DataUtils.AddParameters(cmd, "ACC_NO", model.Account);
                DataUtils.AddParameters(cmd, "NAME", model.Name);
                DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);
                DataUtils.AddParameters(cmd, "TEL", model.Tel);
                DataUtils.AddParameters(cmd, "MAIL", model.Mail);

                DataUtils.AddParameters(cmd, "FUN_CD", "ADM-ACC");
                DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

                int flag = cmd.ExecuteNonQuery();

                if (flag == 1) return true;
            }

            return false;
        }

        /// <summary>
        /// 修改管理帳號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(AccountEditModel model)
        {
            string sql = @"UPDATE ADMIN SET 
                            NAME = @NAME,
                            UNIT_CD = @UNIT_CD,
                            TEL = @TEL,
                            MAIL = @MAIL,
                            ADMIN_SCOPE = @ADMIN_SCOPE,
                            LEVEL_UPD_TIME = (SELECT MAX(UPD_TIME) FROM ADMIN_LEVEL WHERE ACC_NO = ADMIN.ACC_NO),
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC
                        WHERE ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "NAME", model.Name);
            DataUtils.AddParameters(cmd, "UNIT_CD", model.UnitCode);
            DataUtils.AddParameters(cmd, "TEL", model.Tel);
            DataUtils.AddParameters(cmd, "MAIL", model.Mail);
            DataUtils.AddParameters(cmd, "ADMIN_SCOPE", model.Scope);

            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-ACC");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "ACC_NO", model.Account);

            int flag = cmd.ExecuteNonQuery();

            logger.Debug("flag: " + model.UpdateAccount);

            if (flag == 1) return true;

            return false;
        }

        public bool UpdateLevel(AccountEditModel model)
        {
            try
            {
                Dictionary<string, object> args = new Dictionary<string, object>();

                string deleteSQL1 = String.Format(@"
                    UPDATE ADMIN_LEVEL SET
                        DEL_MK = 'Y',
                        DEL_TIME = GETDATE(),
                        DEL_FUN_CD = @FUN_CD,
                        DEL_ACC = @UPD_ACC,
                        UPD_TIME = GETDATE(),
                        UPD_FUN_CD = @FUN_CD,
                        UPD_ACC = @UPD_ACC
                    WHERE ACC_NO = @ACC_NO
                        AND MN_ID NOT IN ({0})
                ", model.Level);

                args.Add("ACC_NO", model.Account);
                args.Add("FUN_CD", "ADM-ACC");
                args.Add("UPD_ACC", model.UpdateAccount);
                Update(deleteSQL1, args);

                string deleteSQL2 = @"DELETE ADMIN_LEVEL WHERE DEL_MK = 'Y' AND ACC_NO = @ACC_NO";

                args.Clear();
                args.Add("ACC_NO", model.Account);
                Update(deleteSQL2, args);

                string insertSQL = String.Format(@"
                    INSERT INTO ADMIN_LEVEL (
                        ACC_NO, MN_ID,
                        UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                    )
                    SELECT @ACC_NO, MN_ID,
                        GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                    FROM ADMIN_MENU M
                    WHERE DEL_MK = 'N'
                        AND MN_ID IN ({0})
                        AND MN_ID NOT IN (SELECT MN_ID FROM ADMIN_LEVEL WHERE DEL_MK = 'N' AND ACC_NO = @ACC_NO)
                ", model.Level);

                args.Clear();
                args.Add("ACC_NO", model.Account);
                args.Add("FUN_CD", "ADM-ACC");
                args.Add("UPD_ACC", model.UpdateAccount);
                Update(insertSQL, args);

                logger.Debug("UpdateLevel: " + insertSQL);
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 停用管理帳號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Delete(AccountQueryModel model)
        {
            string updateSQL1 = @"
                UPDATE ADMIN SET
                    DEL_MK = 'Y',
                    DEL_TIME = GETDATE(),
                    DEL_FUN_CD = @FUN_CD,
                    DEL_ACC = @UPD_ACC,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE ACC_NO = @ACC_NO";

            string deleteSQL1 = @"
                DELETE ADMIN WHERE DEL_MK = 'Y' AND ACC_NO = @ACC_NO
            ";

            string updateSQL2 = @"
                UPDATE ADMIN_LEVEL SET
                    DEL_MK = 'Y',
                    DEL_TIME = GETDATE(),
                    DEL_FUN_CD = @FUN_CD,
                    DEL_ACC = @UPD_ACC,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE ACC_NO = @ACC_NO";

            string deleteSQL2 = @"
                DELETE ADMIN_LEVEL WHERE DEL_MK = 'Y' AND ACC_NO = @ACC_NO
            ";

            using (SqlCommand cmd = new SqlCommand(updateSQL1, conn, tran))
            {

                DataUtils.AddParameters(cmd, "FUN_CD", "ADM-ACC");
                DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

                DataUtils.AddParameters(cmd, "ACC_NO", model.ActionId);

                cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(updateSQL2, conn, tran))
            {

                DataUtils.AddParameters(cmd, "FUN_CD", "ADM-ACC");
                DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);

                DataUtils.AddParameters(cmd, "ACC_NO", model.ActionId);

                cmd.ExecuteNonQuery();
            }

            //using (SqlCommand cmd = new SqlCommand(deleteSQL2, conn, tran))
            //{
            //    DataUtils.AddParameters(cmd, "ACC_NO", model.ActionId);

            //    cmd.ExecuteNonQuery();
            //}

            //using (SqlCommand cmd = new SqlCommand(deleteSQL1, conn, tran))
            //{
            //    DataUtils.AddParameters(cmd, "ACC_NO", model.ActionId);

            //    int flag = cmd.ExecuteNonQuery();
            //    if (flag == 1) return true;
            //}

            return true;
        }

        public List<MemberBaseModel> GetMemberList(MemberQueryModel model)
        {
            List<MemberBaseModel> list = new List<MemberBaseModel>();

            string sql = @"SELECT ACC_NO, IDN, MAIL FROM MEMBER WHERE DEL_MK = 'N' ";

            if (model.QueryType.Equals("I"))
            {
                sql += "AND IDN = @QUERY";
            }
            else if (model.QueryType.Equals("A"))
            {
                sql += "AND ACC_NO = @QUERY";
            }
            else if (model.QueryType.Equals("E"))
            {
                sql += "AND MAIL = @QUERY";
            }

            logger.Debug("QueryId: " + model.QueryId + " / " + sql);

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "QUERY", model.QueryId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    MemberBaseModel item = new MemberBaseModel();
                    item.Account = DataUtils.GetDBString(dr, 0);
                    item.Identity = DataUtils.GetDBString(dr, 1);
                    item.Mail = DataUtils.GetDBString(dr, 2);
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        public bool ResetMemberPassword(MemberQueryModel model)
        {
            string sql = @"UPDATE MEMBER SET
                            PSWD = @PSWD,
                            UPD_TIME = GETDATE(),
                            UPD_FUN_CD = @FUN_CD,
                            UPD_ACC = @UPD_ACC
                        WHERE ACC_NO = @ACC_NO";

            logger.Debug("SQL: " + sql);
            logger.Debug("PSWD: " + DataUtils.Crypt256(model.ActionId));
            logger.Debug("UPD_ACC: " + model.UpdateAccount);
            logger.Debug("ACC_NO: " + model.ActionId);

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            DataUtils.AddParameters(cmd, "PSWD", DataUtils.Crypt256(model.ActionId));
            DataUtils.AddParameters(cmd, "FUN_CD", "ADM-RSTPWD");
            DataUtils.AddParameters(cmd, "UPD_ACC", model.UpdateAccount);
            DataUtils.AddParameters(cmd, "ACC_NO", model.ActionId);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        public List<Dictionary<String, Object>> GetLevelList(string accountNo)
        {
            List<Dictionary<String, Object>> list = new List<Dictionary<string, object>>();
            Dictionary<String, Object> item = null;

            string querySQL = @"
                SELECT MN_ID, MN_PID, MN_TYPE, MN_TEXT,
                    (CASE WHEN (SELECT COUNT(1) FROM ADMIN_LEVEL WHERE DEL_MK = 'N' AND MN_ID = M.MN_ID AND ACC_NO = @ACC_NO) = 0 THEN 'N' ELSE 'Y' END) AS CHK,
                    SEQ_NO * 10000 + ISNULL((SELECT SEQ_NO FROM ADMIN_MENU WHERE MN_ID = M.MN_PID), 0) AS SEQ
                FROM ADMIN_MENU M
                WHERE DEL_MK = 'N' AND MN_ID NOT IN (995, 1000)
                ORDER BY 6
            ";

            SqlCommand cmd = new SqlCommand(querySQL, conn);
            DataUtils.AddParameters(cmd, "ACC_NO", accountNo);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    item = new Dictionary<string, object>();
                    item.Add("id", DataUtils.GetDBInt(dr, n++));
                    item.Add("pId", DataUtils.GetDBInt(dr, n++));
                    item.Add("isParent", DataUtils.GetDBString(dr, n++).Equals("F"));
                    item.Add("name", DataUtils.GetDBString(dr, n++));
                    item.Add("checked", DataUtils.GetDBString(dr, n++).Equals("Y"));

                    item.Add("open", item["isParent"]);

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }
    }
}