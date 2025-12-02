using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Areas.Admin.Models;
using System.Data.SqlClient;

namespace ES.Utils
{
    /**                 前言
     * 這是個獨立客製化計算坑爹的案件費用
     * 由不知道哪個廠商製作後，公司來接維護又接改版案，改制而成
     * By Nick 翻譯
     **/
    public class Calculatefee
    {
        public CaseQueryModel CalculateFee(CaseQueryModel model)
        {
            Map SRV_MAP = GetServiceStatus(model.SRV_ID);
            if (SRV_MAP == null)
                return model;
            if (model.SRV_ID.Equals("001008"))
            {
                model.PAY_A_FEE = getValue_001008(model.APP_ID);
                return model;
            }else{
                //if (!String.IsNullOrEmpty(model.APP_ID)) { } 
                //    //getMasterTableName(org_uid, app_table);
                if (model.PAY_POINT.Equals("A"))
                {
                    model.PAY_A_FEE = 0;
                    model.PAY_C_FEE = 0;
                }
                if (model.PAY_POINT.Equals("B") || model.PAY_POINT.Equals("D"))
                {
                    if (model.PAY_POINT.Equals("B"))
                    {
                        model.PAY_C_FEE = 0;
                    }

                    if (SRV_MAP.GetString("PAY_UNIT").Equals("A"))
                    {
                        model.PAY_A_FEE = SRV_MAP.GetInt("APP_FEE");
                    }
                    else if (SRV_MAP.GetString("PAY_UNIT").Equals("B"))
                    {
                        model.PAY_A_FEE = getValueB(model.SRV_ID, model.APP_ID, SRV_MAP.GetInt("APP_FEE"), SRV_MAP.GetInt("BASE_NUM"), SRV_MAP.GetInt("FEE_EXTRA") , model.PAY_A_FEE.Value);
                    }
                    else if (SRV_MAP.GetString("PAY_UNIT").Equals("C"))
                    {
                        //Do Nothing
                    }
                    else if (SRV_MAP.GetString("PAY_UNIT").Equals("D"))
                    {
                        //Do Nothing
                    }
                }
                if (model.PAY_POINT.Equals("C") || model.PAY_POINT.Equals("D"))
                {
                    if (model.PAY_POINT.Equals("C"))
                    {
                        model.PAY_A_FEE = 0;
                    }

                    if (SRV_MAP.GetString("CHK_PAY_UNIT").Equals("A"))
                    {
                        model.PAY_C_FEE = SRV_MAP.GetInt("CHK_FEE");
                    }
                    else if (SRV_MAP.GetString("CHK_PAY_UNIT").Equals("B"))
                    {
                        model.PAY_A_FEE = getValueB(model.SRV_ID, model.APP_ID, SRV_MAP.GetInt("APP_FEE"), SRV_MAP.GetInt("BASE_NUM"), SRV_MAP.GetInt("FEE_EXTRA"), model.PAY_A_FEE.Value);
                    }
                    else if (SRV_MAP.GetString("CHK_PAY_UNIT").Equals("C"))
                    {
                        //Do Nothing
                    }
                }
            }
            return model;
        }

        public int getValueB(string srv_id, string app_id, int app_fee, int basenum, int fee_extra, int fee)
        {
            int data = 0;
            if (!String.IsNullOrEmpty(app_id))
            {
                data = GetAPPLY_COPIES(srv_id, app_id);
            }
            //比較申請份數是否大於基本份數
            int base_num = basenum;
            int intfee = 0;
            if (base_num >= data)
                intfee = data * app_fee;     //沒有大於基本份數
            else
                intfee = base_num * app_fee + (data - base_num) * fee_extra;
            fee = intfee;
            return fee;
        }

        //public int getValueC(string app_table, string s_uid, string org_uid, string app_id, string app_num_field, string app_fee, string basenum, string fee_extra, string value, string fee)
        //{
        //    int data = 0;
        //    if (String.IsNullOrEmpty(app_id))
        //    {
        //        data = int.Parse(value);
        //    }
        //    else
        //    {
        //        data = GetAPPLY_COPIES(s_uid, app_id);
        //    }
        //    //比較申請份數是否大於基本份數
        //    //printf("bm=%s,data=%s",basenum,data);
        //    int base_num = int.Parse(basenum);
        //    int intfee = 0;
        //    if (base_num >= data)
        //        intfee = data * int.Parse(app_fee);     //沒有大於基本份數
        //    else
        //        intfee = base_num * int.Parse(app_fee) + (data - base_num) * int.Parse(fee_extra);
        //    fee = intfee.ToString();
        //    return 0;
        //}

        private int getValue_001008(string app_id)
        {
            int count =  GetAPPLY_001008_CountType01(app_id) + GetAPPLY_001008_CountType02(app_id);
            return count;
        }

        private int GetAPPLY_COPIES(string srv_id,string app_id)
        {
            int tatol = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = "select COPIES from APPLY_" + srv_id + " where APP_ID = @APP_ID";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            tatol = DataUtils.GetDBInt(sda, 0);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return tatol;
        }

        private int GetAPPLY_001008_CountType01(string app_id)
        {
            int tatol = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"with ap as (
                                    select SRL_NO,ME_LIC_TYPE as LIC_TYPE,ME_COPIES as COPIES,ME_TYPE_CD as TYPE_CD
                                    from APPLY_001008_ME where APP_ID = @APP_ID
                                    UNION
                                    select SRL_NO,PR_LIC_TYPE as LIC_TYPE,PR_COPIES as COPIES,PR_TYPE_CD as TYPE_CD
                                    from APPLY_001008_PR where APP_ID = @APP_ID
                                    )
                                    select LIC_TYPE,(500 + (sum(COPIES)-1)*200) as money from ap where TYPE_CD = 1
                                    GROUP BY LIC_TYPE
                                ";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            tatol += DataUtils.GetDBInt(sda, 2);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return tatol;
        }

        private int GetAPPLY_001008_CountType02(string app_id)
        {
            int tatol = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"with ap as (
                                    select SRL_NO,ME_LIC_TYPE as LIC_TYPE,ME_COPIES as COPIES,ME_TYPE_CD as TYPE_CD
                                    from APPLY_001008_ME where APP_ID = @APP_ID
                                    UNION
                                    select SRL_NO,PR_LIC_TYPE as LIC_TYPE,PR_COPIES as COPIES,PR_TYPE_CD as TYPE_CD
                                    from APPLY_001008_PR where APP_ID = @APP_ID
                                    )
                                    select top 1 (500 + (COPIES-1)*200) as money from  ap where TYPE_CD = 2 ORDER BY SRL_NO DESC
                                ";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            tatol += DataUtils.GetDBInt(sda, 0);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return tatol;
        }

        private Map GetServiceStatus(string srv_id)
        {
            Map map = null;
            //int d = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"
                                select PAY_POINT,PAY_UNIT,APP_FEE,BASE_NUM,FEE_EXTRA,APP_NUM_FIELD,
                                PAY_RULE_FIELD,CHK_PAY_UNIT,CHK_FEE,CHK_BASE_NUM,CHK_FEE_EXTRA,
                                CHK_NUM_FIELD,CHK_PAY_RULE_FIELD
                                from service where SRV_ID = @SRV_ID
                                ";
                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    //dbc.Parameters.Add("@SRV_ID", srv_id);
                    DataUtils.AddParameters(dbc, "SRV_ID", srv_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            map = new Map();
                            map.Add("PAY_POINT", DataUtils.GetDBString(sda, 0));
                            map.Add("PAY_UNIT", DataUtils.GetDBString(sda, 1));
                            map.Add("APP_FEE", DataUtils.GetDBInt(sda, 2));
                            map.Add("BASE_NUM", DataUtils.GetDBInt(sda, 3));
                            map.Add("FEE_EXTRA", DataUtils.GetDBInt(sda, 4));
                            map.Add("APP_NUM_FIELD", DataUtils.GetDBString(sda, 5));
                            map.Add("PAY_RULE_FIELD", DataUtils.GetDBString(sda, 6));
                            map.Add("CHK_PAY_UNIT", DataUtils.GetDBString(sda, 7));
                            map.Add("CHK_FEE", DataUtils.GetDBInt(sda, 8));
                            map.Add("CHK_BASE_NUM", DataUtils.GetDBInt(sda, 9));
                            map.Add("CHK_FEE_EXTRA", DataUtils.GetDBInt(sda, 10));
                            map.Add("CHK_NUM_FIELD", DataUtils.GetDBString(sda, 11));
                            map.Add("CHK_PAY_RULE_FIELD", DataUtils.GetDBString(sda, 12));
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return map;
        }
    }
}