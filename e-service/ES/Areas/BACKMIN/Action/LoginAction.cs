using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;

namespace ES.Areas.Admin.Action
{
    public class LoginAction : BaseAction
    {
        /// <summary>
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public LoginAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public LoginAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public bool InsertLoginLog(string accountNo, string ip, string status)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("ACC_NO", accountNo);
            args.Add("IP_ADDR", ip);
            args.Add("STATUS", status);

            string sql = @"
                INSERT INTO LOGIN_LOG (
                    LOGIN_ID, LOGIN_TIME, NAME, UNIT_CD, IP_ADDR, STATUS, FAIL_TOTAL, FAIL_COUNT
                ) VALUES (
                    @ACC_NO, GETDATE(), 
                    (SELECT NAME FROM ADMIN WHERE ACC_NO = @ACC_NO),
                    (SELECT UNIT_CD FROM ADMIN WHERE ACC_NO = @ACC_NO),
                    @IP_ADDR, @STATUS, (
                        CASE
                            WHEN @STATUS = 'A' THEN ISNULL((SELECT MAX(FAIL_TOTAL) FROM LOGIN_LOG WHERE LOGIN_ID = @ACC_NO), 0) + 1
                            ELSE NULL
                        END
                    ), (
                        CASE
                            WHEN @STATUS = 'A' THEN ISNULL((SELECT TOP 1 FAIL_COUNT FROM LOGIN_LOG WHERE LOGIN_ID = @ACC_NO ORDER BY LOGIN_TIME DESC), 0) + 1
                            ELSE NULL
                        END
                    )
                )
            ";

            try
            {
                if (Update(sql, args) == 1)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Warn("LoginAction_InsertLoginLog: " + e.Message, e);
            }

            return false;
        }
    }
}