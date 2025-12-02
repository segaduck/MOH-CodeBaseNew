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
    public class ServiceApplyOpenAction : BaseAction
    {
        /// <summary>
        /// 參數管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public ServiceApplyOpenAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 參數管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public ServiceApplyOpenAction(SqlConnection conn, SqlTransaction tran)
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
            string querySQL = @"SELECT convert(varchar,SRV_ID) SRV_ID,convert(nvarchar,NAME) SRV_NAME
                                ,convert(varchar,APP_SDATE,111) APP_SDATE
                                ,convert(varchar,APP_EDATE,111) APP_EDATE
                                FROM SERVICE where SRV_ID='011010' ";

            return GetList(querySQL, null);
        }

        public ServiceApplyOpenModel GetData(string id)
        {
            ServiceApplyOpenModel model = new ServiceApplyOpenModel();

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SRV_ID", id);

            string querySQL = @"SELECT convert(varchar,SRV_ID) SRV_ID,convert(nvarchar,NAME) SRV_NAME
                                ,convert(varchar,APP_SDATE,111) APP_SDATE
                                ,convert(varchar,APP_EDATE,111) APP_EDATE
                                FROM SERVICE WHERE SRV_ID = @SRV_ID ";

            Dictionary<string, object> data = GetData(querySQL, args);

            model.SRV_ID = id;
            model.SRV_NAME = Convert.IsDBNull(data["SRV_NAME"]) ? "" : (string)data["SRV_NAME"];
            model.APP_SDATE = Convert.IsDBNull(data["APP_SDATE"]) ? "" : (string)data["APP_SDATE"];
            model.APP_EDATE = Convert.IsDBNull(data["APP_EDATE"]) ? "" : (string)data["APP_EDATE"];

            return model;
        }

        public bool Update(ServiceApplyOpenModel model)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("APP_SDATE", model.APP_SDATE);
            args.Add("APP_EDATE", model.APP_EDATE);
            args.Add("FUN_CD", "ADM-SRV");
            args.Add("UPD_ACC", model.UpdateAccount);
            args.Add("SRV_ID", "011010");

            string updateSQL = @"
                UPDATE SETUP SET
                    APP_SDATE = @APP_SDATE,
                    APP_EDATE = @APP_EDATE,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE SRV_ID = @SRV_ID
            ";

            return (Update(updateSQL, args) == 1);
        }

    }
}