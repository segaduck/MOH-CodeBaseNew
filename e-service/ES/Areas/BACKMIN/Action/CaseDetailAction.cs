using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using log4net;
using ES.Areas.Admin.Models;
using System.Data.Objects.SqlClient;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Configuration;
using ES.Utils;
using System.Data;
using static ES.Areas.Admin.Models.CaseDetailModel;

namespace ES.Areas.Admin.Action
{
    public class CaseDetailAction : BaseAction
    {
        #region Tran
        public CaseDetailAction()
        {
        }
        public CaseDetailAction(SqlConnection conn)
        {
            this.conn = conn;
        }
        public CaseDetailAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        #endregion

        public DetailViewModel GetAPPLY(string app_id)
        {
            DetailViewModel cqm = new DetailViewModel();
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
                                    select a.APP_ID,a.APP_STR_DATE,a.APP_TIME,a.FLOW_CD,a.IDN,a.MOHW_CASE_NO,
                                        a.NAME,a.PAY_METHOD,a.PAY_POINT,a.PRO_ACC,a.PRO_UNIT_CD,a.SRV_ID,a.PAY_A_FEE,a.PAY_A_PAID,a.PAY_C_FEE,
                                        a.UNIT_CD,ad.NAME as ACC_NAME,s.NAME as SRV_NAME,a.CASE_BACK_MK,a.PAY_BACK_MK, (
                                            SELECT CODE_DESC
                                            FROM CODE_CD
                                            WHERE DEL_MK = 'N'
                                              AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD )
                                              AND CODE_CD = A.FLOW_CD
                                              AND CODE_KIND = 'F_CASE_STATUS'
                                        ) AS CASE_STATUS
                                    from APPLY a 
                                    LEFT JOIN service s on s.SRV_ID = a.SRV_ID
                                    LEFT JOIN ADMIN ad on ad.ACC_NO = a.PRO_ACC
                                    where a.APP_ID=@APP_ID
                                    ";
                        dbc.Parameters.Clear();
                        dbc.CommandText = sql;
                        DataUtils.AddParameters(dbc, "@APP_ID", app_id);
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                cqm.APP_ID = DataUtils.GetDBString(sda, 0);
                                cqm.APP_STR_DATE = DataUtils.GetDBDateTime(sda, 1);
                                cqm.APP_TIME = DataUtils.GetDBDateTime(sda, 2);
                                cqm.FLOW_CD = DataUtils.GetDBString(sda, 3);
                                cqm.IDN = DataUtils.GetDBString(sda, 4);
                                cqm.MOHW_CASE_NO = DataUtils.GetDBString(sda, 5);
                                cqm.NAME = DataUtils.GetDBString(sda, 6);
                                cqm.PAY_METHOD = DataUtils.GetDBString(sda, 7);
                                cqm.PAY_POINT = DataUtils.GetDBString(sda, 8);
                                cqm.PRO_ACC = DataUtils.GetDBString(sda, 9);
                                cqm.PRO_UNIT_CD = DataUtils.GetDBInt(sda, 10).ToString();
                                cqm.SRV_ID = DataUtils.GetDBString(sda, 11);
                                cqm.PAY_A_FEE = DataUtils.GetDBInt(sda, 12).ToString();
                                cqm.PAY_A_PAID = DataUtils.GetDBInt(sda, 13).ToString();
                                cqm.PAY_C_FEE = DataUtils.GetDBInt(sda, 14).ToString();
                                cqm.UNIT_CD = DataUtils.GetDBInt(sda, 15).ToString();
                                cqm.ACC_NAME = DataUtils.GetDBString(sda, 16);
                                cqm.SRV_NAME = DataUtils.GetDBString(sda, 17);
                                cqm.CASE_BACK_MK = DataUtils.GetDBString(sda, 18).Equals("Y");
                                cqm.PAY_BACK_MK = DataUtils.GetDBString(sda, 19).Equals("Y");
                                cqm.CASE_STATUS = DataUtils.GetDBString(sda, 20);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return cqm;
        }

        public Detail001037Model GetDetail001037(string app_id)
        {
            Detail001037Model cqm = new Detail001037Model();
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
                                    select a.APP_ID,a.APP_STR_DATE,a.APP_TIME,a.FLOW_CD,a.IDN,a.MOHW_CASE_NO,
                                        a.NAME,a.PAY_METHOD,a.PAY_POINT,a.PRO_ACC,a.PRO_UNIT_CD,a.SRV_ID,a.PAY_A_FEE,a.PAY_A_PAID,a.PAY_C_FEE,
                                        a.UNIT_CD,ad.NAME as ACC_NAME,s.NAME as SRV_NAME,a.CASE_BACK_MK,a.PAY_BACK_MK, (
                                            SELECT CODE_DESC
                                            FROM CODE_CD
                                            WHERE DEL_MK = 'N'
                                              AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = A.UNIT_CD )
                                              AND CODE_CD = A.FLOW_CD
                                              AND CODE_KIND = 'F_CASE_STATUS'
                                        ) AS CASE_STATUS
                                    from APPLY a 
                                    LEFT JOIN service s on s.SRV_ID = a.SRV_ID
                                    LEFT JOIN ADMIN ad on ad.ACC_NO = a.PRO_ACC
                                    where a.APP_ID=@APP_ID
                                    ";
                        dbc.Parameters.Clear();
                        dbc.CommandText = sql;
                        DataUtils.AddParameters(dbc, "@APP_ID", app_id);
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                cqm.APP_ID = DataUtils.GetDBString(sda, 0);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return cqm;
        }

        public bool UpdateBackAssignApply(string appid, string acc, SqlTransaction tran)
        {
            StringBuilder querySQL = new StringBuilder(@"
        UPDATE APPLY SET
            APP_DISP_ACC = @UPD_ACC,
            APP_DISP_MK = 'N',
            PRO_ACC = NULL,
            PRO_UNIT_CD = NULL,
            UPD_TIME = GETDATE(),
            UPD_FUN_CD = @UPD_FUN_CD,
            UPD_ACC = @UPD_ACC
        WHERE APP_ID = @APP_ID");

            using (SqlCommand com = new SqlCommand(querySQL.ToString(), tran.Connection, tran))
            {
                com.Parameters.AddWithValue("@UPD_ACC", acc);
                com.Parameters.AddWithValue("@UPD_FUN_CD", "SYS-ASSIGN");
                com.Parameters.AddWithValue("@APP_ID", appid);

                int flag = com.ExecuteNonQuery();
                return flag == 1;
            }
        }

    }
}