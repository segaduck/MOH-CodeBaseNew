using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using log4net;
using ES.Areas.Admin.Models;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using ES.Utils;
using System.Text;

namespace ES.Areas.Admin.Action
{
    public class FlySwipeAction : BaseAction
    {
        public FlySwipeAction()
        {
        }

        public FlySwipeAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        public FlySwipeAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }
 
        public List<FlySwipeSearchGridModel> GetFlySwipeList(FlySwipeModel form)
        {
            List<FlySwipeSearchGridModel> result = new List<FlySwipeSearchGridModel>();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
    SELECT   a.STORECODE
    ,a.TRACENO  /*PK*/ 
    ,a.PAYDATE  /*PK*/ 
    ,a.PAYMONEY
    ,a.BILLINGDATE
    ,a.GRANTDATE
    ,a.DEL_MK    ,a.DEL_TIME    ,a.DEL_FUN_CD    ,a.DEL_ACC
    ,a.UPD_TIME    ,a.UPD_FUN_CD    ,a.UPD_ACC
    ,a.ADD_TIME    ,a.ADD_FUN_CD    ,a.ADD_ACC
    FROM FLYSWIPE a
    WHERE 1=1
    AND a.DEL_MK = 'N' ");

                //TRACENO 授權碼
                if (!String.IsNullOrEmpty(form.QRY_TRACENO)) { sb.Append(" and a.TRACENO = @TRACENO "); }
                //[PAYDATE] 交易日期 	
                if (form.QRY_PAYDATE != null) { sb.Append(" and a.PAYDATE = @PAYDATE "); }

                sb.Append(@" ORDER BY PAYDATE, TRACENO");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);

                //TRACENO 授權碼
                if (!String.IsNullOrEmpty(form.QRY_TRACENO)) { DataUtils.AddParameters(com, "TRACENO", form.QRY_TRACENO); }
                //[PAYDATE] 交易日期 	
                if (form.QRY_PAYDATE != null)
                {
                    string s_QRY_PAYDATE = form.QRY_PAYDATE.Value.ToString("yyyy-MM-dd");
                    DataUtils.AddParameters(com, "PAYDATE", s_QRY_PAYDATE);
                }

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        FlySwipeSearchGridModel cqm = new FlySwipeSearchGridModel()
                        {
                            STORECODE = Convert.ToString(sr["PAYMONEY"]),
                            TRACENO = Convert.ToString(sr["TRACENO"]),
                            PAYDATE = Convert.ToDateTime(sr["PAYDATE"]),
                            PAYMONEY = Convert.ToString(sr["PAYMONEY"]),
                            BILLINGDATE = Convert.ToDateTime(sr["BILLINGDATE"]),
                            GRANTDATE = Convert.ToDateTime(sr["GRANTDATE"])
                        };

                        result.Add(cqm);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }

            return result == null ? new List<FlySwipeSearchGridModel>() : result;

        }

        /// <summary>
        /// 匯入檔案
        /// </summary>
        /// <param name="list"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int InsertFlySweipFile(string rFileName, string rFilePath)
        {
            int count = 0;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string updateSQL1 = @"
    INSERT INTO [dbo].[FLYSWIPE_FILE]([FILE_NAME] ,[FILE_URL] ,[ADD_TIME] ,[STATUS])
    VALUES (@FILE_NAME ,@FILE_URL ,GETDATE() ,@STATUS) ";

            //logger.Debug("FILE_NAME: " + remoteFileName);
            //var fileurl = DataUtils.GetConfig("SCHEDULE_MOHW_DOWNLOAD_PATH") + remoteFileName;
            args.Clear();
            args.Add("FILE_NAME", rFileName);
            args.Add("FILE_URL", rFilePath);
            args.Add("STATUS", "Y");
            //args.Add("ADD_TIME", DateTime.Now);
            count += Update(updateSQL1, args);

            return count;
        }

        /// <summary>
        /// EXCEL 匯入
        /// </summary>
        /// <param name="list"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int InsertFile(List<ImpFlySwipeModel> list, string updateAccount, string BANKTYPE)
        {
            int i_count = 0;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string selectSQL1 = @"
    SELECT 'X' FROM FLYSWIPE WHERE 1=1
    AND TRACENO=@TRACENO AND PAYDATE=@PAYDATE ";

            string insertSQL1 = @"
    INSERT INTO [dbo].[FLYSWIPE](
    STORECODE,TRACENO  ,PAYDATE ,PAYMONEY,BILLINGDATE,GRANTDATE,DEL_MK
    ,UPD_TIME,UPD_FUN_CD,UPD_ACC
    ,ADD_TIME,ADD_FUN_CD,ADD_ACC
    ,TRACENO_QID
    ,BANKTYPE
    )
    VALUES(
    @STORECODE,@TRACENO  ,@PAYDATE ,@PAYMONEY,@BILLINGDATE,@GRANTDATE,'N'
    ,GETDATE(),@UPD_FUN_CD,@UPD_ACC
    ,GETDATE(),@ADD_FUN_CD,@ADD_ACC
    ,@TRACENO_QID
    ,@BANKTYPE
    ) ";

            try
            {
                foreach (ImpFlySwipeModel item in list)
                {
                    if (string.IsNullOrEmpty(item.STORECODE)) { continue; }
                    if (string.IsNullOrEmpty(item.TRACENO)) { continue; }
                    if (!item.PAYDATE.HasValue) { continue; }

                    args.Clear();
                    //args.Add("STORECODE", item.STORECODE);
                    args.Add("TRACENO", item.TRACENO);
                    args.Add("PAYDATE", item.PAYDATE);
                    List<Dictionary<string, object>> listdt1 = GetList(selectSQL1, args);
                    if (listdt1 != null && listdt1.Count > 0) { continue; }
                    logger.Debug(string.Format("TRACENO: {0}", item.TRACENO) + string.Format(",PAYDATE: {0}", item.PAYDATE.Value));
                    logger.Debug(string.Format("TRACENO_QID: {0}", item.TRACENO_QID));

                    args.Clear();
                    args.Add("STORECODE", item.STORECODE);
                    args.Add("TRACENO", item.TRACENO);
                    args.Add("PAYDATE", item.PAYDATE);
                    args.Add("PAYMONEY", item.PAYMONEY);
                    args.Add("BILLINGDATE", item.BILLINGDATE);
                    args.Add("GRANTDATE", item.GRANTDATE);
                    //args.Add("UPD_TIME", DateTime.Now);
                    args.Add("UPD_FUN_CD", "ADM-FLYPAY");
                    args.Add("UPD_ACC", updateAccount);
                    //args.Add("ADD_TIME", DateTime.Now);
                    args.Add("ADD_FUN_CD", "ADM-FLYPAY");
                    args.Add("ADD_ACC", updateAccount);
                    args.Add("TRACENO_QID", item.TRACENO_QID);
                    args.Add("BANKTYPE", BANKTYPE);
                    i_count += Update(insertSQL1, args);
                    //logger.Debug(string.Format("i_count :{0}", i_count));
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }

            return i_count;
        }

    }
}