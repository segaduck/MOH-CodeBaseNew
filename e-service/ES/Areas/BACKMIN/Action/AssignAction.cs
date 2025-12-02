using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using log4net;
using ES.Areas.Admin.Models;
using System.Data.SqlClient;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class AssignAction : BaseAction
    {
        public AssignAction()
        {
        }
        public AssignAction(SqlConnection conn)
        {
            this.conn = conn;
        }
        public AssignAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }
        public List<Map> GetAPPLY(AssignModel model, AccountModel account)
        {
            //string s_log1 = "";
            //s_log1 += string.Format("\n GetAPPLY:model.CaseType::{0}", model.CaseType);
            //s_log1 += string.Format("\n GetAPPLY:model.CaseAccount::{0}", model.CaseAccount);
            //logger.Debug(s_log1);

            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                //using (SqlTransaction st = conn.BeginTransaction()) { }
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    //dbc.Transaction = st;
                    //FIX 須加上登入者單位分類判斷
                    String sql = @"
                    SELECT A.APP_ID,S.NAME AS SRV_NAME,A.APP_TIME
                    ,ISNULL(A.PRO_ACC,'') PRO_ACC,AD.NAME,S.RPT_NAME,A.SRV_ID,
                    (SELECT DISTINCT CODE_DESC FROM CODE_CD WHERE CODE_KIND='F_CASE_STATUS' AND CODE_CD=A.FLOW_CD
                    AND CODE_PCD IN (SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD=@UNIT_CD))) FLOW_CD_NAME
                    FROM APPLY A 
                    LEFT JOIN SERVICE S ON S.SRV_ID = A.SRV_ID
                    LEFT JOIN ADMIN AD ON AD.ACC_NO = A.ACC_NO
                    WHERE 1=1
                    AND A.DEL_MK = 'N'
                    AND A.CLOSE_MK='N' 
                    AND A.APP_DISP_MK='N'";

                    dbc.Parameters.Clear();

                    DataUtils.AddParameters(dbc, "UNIT_CD", account.UnitCode);

                    if (account.Scope == 1) // 所有單位
                    {

                    }
                    else if (account.Scope == 0) // 所屬單位
                    {
                        sql += " AND A.SRV_ID IN (SELECT SRV_ID FROM SERVICE_UNIT WHERE UNIT_CD = @UNIT_CD) ";
                    }
                    else // 隸屬單位
                    {
                        sql += " AND A.PRO_ACC = @PRO_ACC ";
                        DataUtils.AddParameters(dbc, "PRO_ACC", account.Account);
                    }
                    if (!string.IsNullOrEmpty(model.CaseType))
                    {
                        sql += " AND A.SRV_ID = @SRV_ID ";
                        DataUtils.AddParameters(dbc, "SRV_ID", model.CaseType);
                    }
                    //if (!string.IsNullOrEmpty(model.CaseAccount))
                    //{
                    //    sql += " AND CONCAT(A.PRO_ACC,'/',A.PRO_UNIT_CD) = @PRO_ACC_UNIT ";
                    //    DataUtils.AddParameters(dbc, "PRO_ACC_UNIT", model.CaseAccount);
                    //}
                    sql += " ORDER BY A.APP_TIME DESC ";

                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                            map.Add("SRV_NAME", DataUtils.GetDBString(sda, 1));
                            map.Add("APP_TIME", (DataUtils.GetDBDateTime(sda, 2).HasValue ? DataUtils.GetDBDateTime(sda, 2).Value.ToString("yyyy/MM/dd") : ""));

                            map.Add("PRO_ACC", DataUtils.GetDBString(sda, 3));
                            map.Add("NAME", DataUtils.GetDBString(sda, 4));
                            map.Add("RPT_NAME", DataUtils.GetDBString(sda, 5));
                            map.Add("SRV_ID", DataUtils.GetDBString(sda, 6));
                            map.Add("FLOW_CD_NAME", DataUtils.GetDBString(sda, 7));
                            li.Add(map);
                        }
                        sda.Close();
                    }
                    if (li != null)
                    {
                        this.totalCount = li.Count();
                        li = li.Skip((model.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<Map> SearchAPPLY(AssignModel model, AccountModel account)
        {

            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                //using (SqlTransaction st = conn.BeginTransaction()) { }
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    //dbc.Transaction = st;
                    //FIX 須加上登入者單位分類判斷

                    dbc.Parameters.Clear();

                    String sql = @"
                    SELECT A.APP_ID,S.NAME AS SRV_NAME,A.APP_TIME,
                    ISNULL(A.PRO_ACC,'') PRO_ACC,AD.NAME,S.RPT_NAME,A.SRV_ID,
                    (SELECT DISTINCT CODE_DESC FROM CODE_CD WHERE CODE_KIND='F_CASE_STATUS' AND CODE_CD=A.FLOW_CD
                    AND CODE_PCD IN (SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD=@UNIT_CD))) FLOW_CD_NAME
                    FROM APPLY A 
                    JOIN SERVICE S ON S.SRV_ID = A.SRV_ID
                    LEFT JOIN ADMIN AD ON AD.ACC_NO = A.ACC_NO
                    WHERE 1=1 AND A.APP_DISP_MK='N' ";
                    //AND A.CLOSE_MK='N' ";

                    if (string.IsNullOrEmpty(model.FLOW_CD) || model.FLOW_CD == "-1")
                    {
                        sql += " AND A.APP_DISP_MK = 'N'";
                    }

                    DataUtils.AddParameters(dbc, "UNIT_CD", account.UnitCode);

                    if (account.Scope == 1) // 所有單位
                    {

                    }
                    else if (account.Scope == 0) // 所屬單位
                    {
                        sql += " AND A.SRV_ID IN (SELECT SRV_ID FROM SERVICE_UNIT WHERE UNIT_CD = @UNIT_CD) ";
                        sql += " AND EXISTS (SELECT ACC_NO FROM SERVICE_CASE WHERE SRV_ID=A.SRV_ID AND ACC_NO=@PRO_ACC) ";
                        //sql += " AND A.PRO_ACC = @PRO_ACC ";
                        DataUtils.AddParameters(dbc, "PRO_ACC", account.Account);
                    }
                    else // 隸屬單位
                    {
                        sql += " AND EXISTS (SELECT ACC_NO FROM SERVICE_CASE WHERE SRV_ID=A.SRV_ID AND ACC_NO=@PRO_ACC) ";
                        DataUtils.AddParameters(dbc, "PRO_ACC", account.Account);
                    }
                    if (!string.IsNullOrEmpty(model.CaseType))
                    {
                        sql += " AND A.SRV_ID = @SRV_ID ";
                        DataUtils.AddParameters(dbc, "SRV_ID", model.CaseType);
                    }
                    if (!string.IsNullOrEmpty(model.CaseAccount))
                    {
                        sql += " AND CONCAT(A.PRO_ACC,'/',A.PRO_UNIT_CD) = @PRO_ACC_UNIT ";
                        DataUtils.AddParameters(dbc, "PRO_ACC_UNIT", model.CaseAccount);
                    }

                    if (!string.IsNullOrEmpty(model.FLOW_CD) && model.FLOW_CD != "-1")
                    {
                        sql += " AND A.FLOW_CD = @FLOW_CD  ";
                        DataUtils.AddParameters(dbc, "FLOW_CD", model.FLOW_CD);
                    }

                    sql += " ORDER BY A.APP_TIME DESC ";

                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                            map.Add("SRV_NAME", DataUtils.GetDBString(sda, 1));
                            map.Add("APP_TIME", (DataUtils.GetDBDateTime(sda, 2).HasValue ? DataUtils.GetDBDateTime(sda, 2).Value.ToString("yyyy/MM/dd") : ""));

                            map.Add("PRO_ACC", DataUtils.GetDBString(sda, 3));
                            map.Add("NAME", DataUtils.GetDBString(sda, 4));
                            map.Add("RPT_NAME", DataUtils.GetDBString(sda, 5));
                            map.Add("SRV_ID", DataUtils.GetDBString(sda, 6));
                            map.Add("FLOW_CD_NAME", DataUtils.GetDBString(sda, 7));
                            li.Add(map);
                        }
                        sda.Close();
                    }
                    if (li != null)
                    {
                        this.totalCount = li.Count();
                        li = li.Skip((model.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<Dictionary<string, object>> GetCaseTypeList(AccountModel account, SqlConnection conn)
        {
            this.conn = conn;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
 SELECT DISTINCT S.SRV_ID, S.NAME AS SRV_NAME /*,ISNULL(PRO_ACC,'') AS PRO_ACC*/ ,
 (SELECT DISTINCT CODE_DESC FROM CODE_CD WHERE CODE_KIND='F_CASE_STATUS' AND CODE_CD=A.FLOW_CD
    AND CODE_PCD IN (SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD=31))) AS FLOW_CD_NAME
 FROM SERVICE S, APPLY A
 WHERE S.SRV_ID = A.SRV_ID
    AND A.CLOSE_MK = 'N'
    AND A.APP_DISP_MK = 'N'
  AND A.DEL_MK = 'N'
            ";

            args.Add("UNIT_CD", account.UnitCode);

            if (account.Scope == 1) // 所有單位
            {

            }
            else if (account.Scope == 0) // 所屬單位
            {
                querySQL += "AND A.SRV_ID IN (SELECT SRV_ID FROM SERVICE_UNIT WHERE UNIT_CD = @UNIT_CD) ";
            }
            else // 隸屬單位
            {
                querySQL += "AND A.PRO_ACC = @PRO_ACC ";
                args.Add("PRO_ACC", account.Account);
            }

            querySQL += "ORDER BY S.SRV_ID";

            return GetList(querySQL, args);
        }

        public List<Dictionary<string, object>> GetAdminList(string serviceId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.ACC_NO, A.NAME, C.UNIT_CD, C.UNIT_NAME
                FROM ADMIN A, ADMIN_LEVEL B, UNIT C
                WHERE A.ACC_NO = B.ACC_NO
                  AND A.UNIT_CD = C.UNIT_CD
                  AND A.UNIT_CD IN (SELECT UNIT_CD FROM SERVICE_UNIT WHERE SRV_ID = @SRV_ID AND DEL_MK = 'N')
                  AND B.MN_ID = 152
                ORDER BY C.UNIT_PCD, C.SEQ_NO
            ";

            args.Add("SRV_ID", serviceId);

            return GetList(querySQL, args);
        }

        public AssignEditModel GetAPPLY(String id)
        {
            AssignEditModel aem = new AssignEditModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        //FIX 須加上登入者單位分類判斷
                        String sql = @"
                                    select a.app_id,a.SRV_ID,s.NAME as SRV_NAME,a.APP_TIME,a.PRO_ACC,a.TO_ARCHIVE_MK,a.FLOW_CD
                                    from APPLY a 
                                    LEFT JOIN service s on s.SRV_ID = a.SRV_ID
                                    where a.app_id = @app_id
                                    ";
                        dbc.Parameters.Clear();
                        //dbc.Parameters.Add("@app_id", id);
                        DataUtils.AddParameters(dbc, "app_id", id);
                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            if (sda.Read())
                            {
                                aem.APP_ID = DataUtils.GetDBString(sda, 0);
                                aem.SRV_ID = DataUtils.GetDBString(sda, 1);
                                aem.SRV_NAME = DataUtils.GetDBString(sda, 2);
                                aem.APP_TIME = DataUtils.GetDBDateTime(sda, 3).HasValue ? DataUtils.GetDBDateTime(sda, 3).Value.ToString("yyyy/MM/dd") : "";
                                aem.PRO_ACC = DataUtils.GetDBString(sda, 4);
                                aem.TO_ARCHIVE_MK = DataUtils.GetDBString(sda, 5);
                                aem.FLOW_CD = DataUtils.GetDBString(sda, 6);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return aem;
        }

        public List<Dictionary<string, object>> GetBatchList(Dictionary<string, object> item)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SRV_ID", item["SRV_ID"]);

            string querySQL = @"
                SELECT APP_ID
                FROM APPLY
                WHERE DEL_MK = 'N'
                AND CLOSE_MK = 'N'
                AND isnull(APP_DISP_MK,'N') = 'N'
                AND SRV_ID = @SRV_ID
            ";

            return GetList(querySQL, args);
        }

        public bool BatchUpdateApply(Dictionary<string, object> args)
        {
            string updateSQL = @"
                UPDATE APPLY SET
                    APP_DISP_ACC = @UPD_ACC,
                    APP_DISP_MK = 'Y',
                    PRO_ACC = @PRO_ACC,
                    PRO_UNIT_CD = @PRO_UNIT_CD,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE DEL_MK = 'N'
                  AND CLOSE_MK = 'N'
                  AND APP_DISP_MK = 'N'
                  AND SRV_ID = @SRV_ID
            ";

            Update(updateSQL, args);

            return true;
        }

        public Boolean UpdateAPPLY(AssignEditModel aem, String Account)
        {
            String[] admin = aem.PRO_ACC.Split('/');
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    dbc.CommandText = @"update APPLY set 
                                        APP_DISP_ACC = @APP_DISP_ACC ,
                                        APP_DISP_MK = 'Y',
                                        PRO_ACC = @PRO_ACC,
                                        PRO_UNIT_CD = @PRO_UNIT_CD
                                        where APP_ID = @APP_ID ";

                    dbc.Parameters.Clear();
                    DataUtils.AddParameters(dbc, "APP_DISP_ACC", Account);
                    DataUtils.AddParameters(dbc, "PRO_ACC", admin[0]);
                    DataUtils.AddParameters(dbc, "PRO_UNIT_CD", admin[1]);
                    DataUtils.AddParameters(dbc, "APP_ID", aem.APP_ID);
                    //dbc.Parameters.Add("@APP_DISP_ACC", Account);
                    //dbc.Parameters.Add("@PRO_ACC", admin[0]);
                    //dbc.Parameters.Add("@PRO_UNIT_CD", admin[1]);
                    //dbc.Parameters.Add("@APP_ID", aem.APP_ID);
                    dbc.ExecuteNonQuery();
                    result = true;
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }
    }
}