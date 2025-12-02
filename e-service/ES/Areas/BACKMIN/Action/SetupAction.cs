using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;

namespace ES.Areas.Admin.Action
{
    /// <summary>
    /// 參數管理
    /// </summary>
    public class SetupAction : BaseAction
    {
        /// <summary>
        /// 參數管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public SetupAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 參數管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public SetupAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 取得列表
        /// </summary>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetList()
        {
            string querySQL = @"SELECT * FROM SETUP ORDER BY SETUP_CD ";

            return GetList(querySQL, null);
        }

        public SetupModel GetData(string id)
        {
            SetupModel model = new SetupModel();

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SETUP_CD", id);

            string querySQL = @"SELECT * FROM SETUP WHERE SETUP_CD = @SETUP_CD ";

            Dictionary<string, object> data = GetData(querySQL, args);

            model.SetupCode = Convert.IsDBNull(data["SETUP_CD"])?"":(string)data["SETUP_CD"];
            model.SetupDesc = Convert.IsDBNull(data["SETUP_DESC"]) ? "" : (string)data["SETUP_DESC"];
            model.SetupValue = Convert.IsDBNull(data["SETUP_VAL"]) ? "" : (string)data["SETUP_VAL"];

            return model;
        }

        public bool Insert(SetupModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SETUP_CD", model.SetupCode);
            args.Add("SETUP_DESC", model.SetupDesc);
            args.Add("SETUP_VAL", model.SetupValue);
            args.Add("FUN_CD", "ADM-SETUP");
            args.Add("UPD_ACC", model.UpdateAccount);

            string insertSQL = @"
                INSERT INTO SETUP (
                    SETUP_CD, SETUP_DESC, SETUP_VAL,
                    UPD_TIME, UPD_FUN_CD, UPD_ACC, ADD_TIME, ADD_FUN_CD, ADD_ACC
                ) VALUES (
                    @SETUP_CD, @SETUP_DESC, @SETUP_VAL,
                    GETDATE(), @FUN_CD, @UPD_ACC, GETDATE(), @FUN_CD, @UPD_ACC
                )
            ";

            return (Update(insertSQL, args) == 1);
        }

        public bool Update(SetupModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SETUP_CD", model.SetupCode);
            args.Add("SETUP_DESC", model.SetupDesc);
            args.Add("SETUP_VAL", model.SetupValue);
            args.Add("FUN_CD", "ADM-SETUP");
            args.Add("UPD_ACC", model.UpdateAccount);

            string updateSQL = @"
                UPDATE SETUP SET
                    SETUP_DESC = @SETUP_DESC,
                    SETUP_VAL = @SETUP_VAL,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE SETUP_CD = @SETUP_CD
            ";

            return (Update(updateSQL, args) == 1);
        }

        public bool Delete(SetupModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SETUP_CD", model.SetupCode);
            args.Add("FUN_CD", "ADM-SETUP");
            args.Add("UPD_ACC", model.UpdateAccount);

            string updateSQL = @"
                UPDATE SETUP SET
                    DEL_MK = 'Y',
                    DEL_TIME = GETDATE(),
                    DEL_FUN_CD = @FUN_CD,
                    DEL_ACC = @UPD_ACC,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE SETUP_CD = @SETUP_CD
            ";

            string deleteSQL = @"DELETE SETUP WHERE DEL_MK = 'Y' AND SETUP_CD = @SETUP_CD";

            if (Update(updateSQL, args) == 1)
            {
                args.Clear();
                args.Add("SETUP_CD", model.SetupCode);

                return (Update(deleteSQL, args) == 1);
            }

            return false;
        }
    }
}