using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using ES.Utils;
using ES.Areas.Admin.Models;
using System.Text;
using log4net.Util.TypeConverters;
using System.Data;
using DocumentFormat.OpenXml.EMMA;

namespace ES.Areas.Admin.Action
{
    public class PayAction : BaseAction
    {
        public PayAction()
        {
        }

        public PayAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        public PayAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }
        /// <summary>
        /// 繳費維護功能 查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<Map> MaintainQuery(MaintainModel model)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = SqlModel.GetMaintainSQL(GetPayMethod(model.PAY_WAY), model.PAY_STATUS);

                        dbc.Parameters.Clear();
                        if (!String.IsNullOrEmpty(model.APP_ID_BEGIN))
                        {
                            sql += "and a.APP_ID>=@APP_ID_BEGIN ";
                            DataUtils.AddParameters(dbc, "APP_ID_BEGIN", model.APP_ID_BEGIN);
                        }
                        if (!String.IsNullOrEmpty(model.APP_ID_END))
                        {
                            sql += "and a.APP_ID<=@APP_ID_END ";
                            DataUtils.AddParameters(dbc, "APP_ID_END", model.APP_ID_END);
                        }
                        if (model.APP_TIME_BEGIN.HasValue)
                        {
                            sql += "and a.APP_TIME>=@APP_TIME_BEGIN ";
                            DataUtils.AddParameters(dbc, "APP_TIME_BEGIN", model.APP_TIME_BEGIN);
                        }
                        if (model.APP_TIME_BEGIN.HasValue)
                        {
                            sql += "and a.APP_TIME<DATEADD(DAY, 1, @APP_TIME_END) ";
                            DataUtils.AddParameters(dbc, "APP_TIME_END", model.APP_TIME_END);
                        }
                        if (!String.IsNullOrEmpty(model.UNIT_TYPE))
                        {
                            sql += "and a.UNIT_CD= @UNIT_CD ";
                            DataUtils.AddParameters(dbc, "UNIT_CD", model.UNIT_TYPE);
                        }
                        if (!String.IsNullOrEmpty(model.SC_PID))
                        {
                            sql += "and s.SC_PID=@SC_PID ";
                            DataUtils.AddParameters(dbc, "SC_PID", model.SC_PID);
                        }
                        if (!String.IsNullOrEmpty(model.SRV_ID))
                        {
                            sql += "and a.SRV_ID=@SRV_ID ";
                            DataUtils.AddParameters(dbc, "SRV_ID", model.SRV_ID);
                        }
                        if (!String.IsNullOrEmpty(model.IDN))
                        {
                            sql += "and a.IDN=@IDN ";
                            DataUtils.AddParameters(dbc, "IDN", model.IDN);
                        }
                        sql += " order by a." + GetOrderColumn(model.OderByCol) + " " + GetOrderBy(model.SortAZ);
                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("SRV_NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("APP_TIME", (DataUtils.GetDBDateTime(sda, 2).HasValue ? DataUtils.GetDBDateTime(sda, 2).Value.ToString("yyyy/MM/dd") : ""));
                                map.Add("FLOW_NAME", DataUtils.GetDBString(sda, 3));
                                map.Add("MSC_CLOSE_MK", DataUtils.GetDBString(sda, 4));
                                map.Add("APP_EXT_DATE", DataUtils.GetDBDateTime(sda, 5));
                                map.Add("NAME", DataUtils.GetDBString(sda, 6));
                                map.Add("PAY_POINT", DataUtils.GetDBString(sda, 7));
                                map.Add("PAY_A_FEE", DataUtils.GetDBInt(sda, 8));
                                map.Add("PAY_A_PAID", DataUtils.GetDBInt(sda, 9));
                                map.Add("PAY_C_FEE", DataUtils.GetDBInt(sda, 10));
                                map.Add("PAY_C_PAID", DataUtils.GetDBInt(sda, 11));
                                map.Add("CLOSE_MK", DataUtils.GetDBString(sda, 12));
                                map.Add("PAY_BACK_MK", DataUtils.GetDBString(sda, 13));
                                map.Add("CASE_BACK_MK", DataUtils.GetDBString(sda, 14));
                                map.Add("PAY_METHOD", DataUtils.GetDBString(sda, 15));
                                map.Add("ISSETTLE", DataUtils.GetDBDateTime(sda, 16).HasValue);
                                map.Add("PAY_STATUS_MK", DataUtils.GetDBString(sda, 17));
                                map.Add("PAY_BANK", DataUtils.GetDBString(sda, 18));
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
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }
        /// <summary>
        /// 聯合信用卡 查詢繳費
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<Map> MaintainQuery(MaintainECModel model)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = SqlModel.GetMaintainECSQL();

                        dbc.Parameters.Clear();
                        if (!String.IsNullOrEmpty(model.APP_ID_BEGIN))
                        {
                            sql += "and a.APP_ID>=@APP_ID_BEGIN ";
                            DataUtils.AddParameters(dbc, "APP_ID_BEGIN", model.APP_ID_BEGIN);
                        }
                        if (!String.IsNullOrEmpty(model.APP_ID_END))
                        {
                            sql += "and a.APP_ID<=@APP_ID_END ";
                            DataUtils.AddParameters(dbc, "APP_ID_END", model.APP_ID_END);
                        }
                        if (model.APP_TIME_BEGIN.HasValue)
                        {
                            sql += "and a.APP_TIME>=@APP_TIME_BEGIN ";
                            DataUtils.AddParameters(dbc, "APP_TIME_BEGIN", model.APP_TIME_BEGIN);
                        }
                        if (model.APP_TIME_BEGIN.HasValue)
                        {
                            sql += "and a.APP_TIME<DATEADD(DAY, 1, @APP_TIME_END) ";
                            DataUtils.AddParameters(dbc, "APP_TIME_END", model.APP_TIME_END);
                        }
                        if (!String.IsNullOrEmpty(model.SC_PID))
                        {
                            sql += "and s.SC_PID=@SC_PID ";
                            DataUtils.AddParameters(dbc, "SC_PID", model.SC_PID);
                        }
                        if (!String.IsNullOrEmpty(model.SRV_ID))
                        {
                            sql += "and a.SRV_ID=@SRV_ID ";
                            DataUtils.AddParameters(dbc, "SRV_ID", model.SRV_ID);
                        }
                        if (!string.IsNullOrEmpty(model.RSP_STATUS))
                        {
                            //請款狀態
                            if (model.RSP_STATUS == "Y")
                            {
                                sql += "and RSP.L_RESPCODE='00' ";
                            }
                            else if (model.RSP_STATUS == "N")
                            {
                                sql += "and RSP.L_RESPCODE<>'00' ";
                            }
                        }

                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("SRV_NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("APP_TIME", (DataUtils.GetDBDateTime(sda, 2).HasValue ? DataUtils.GetDBDateTime(sda, 2).Value.ToString("yyyy/MM/dd") : ""));
                                map.Add("FLOW_NAME", DataUtils.GetDBString(sda, 3));
                                map.Add("MSC_CLOSE_MK", DataUtils.GetDBString(sda, 4));
                                map.Add("APP_EXT_DATE", DataUtils.GetDBDateTime(sda, 5));
                                map.Add("NAME", DataUtils.GetDBString(sda, 6));
                                map.Add("PAY_POINT", DataUtils.GetDBString(sda, 7));
                                map.Add("PAY_A_FEE", DataUtils.GetDBInt(sda, 8));
                                map.Add("PAY_A_PAID", DataUtils.GetDBInt(sda, 9));
                                map.Add("PAY_C_FEE", DataUtils.GetDBInt(sda, 10));
                                map.Add("PAY_C_PAID", DataUtils.GetDBInt(sda, 11));
                                map.Add("CLOSE_MK", DataUtils.GetDBString(sda, 12));
                                map.Add("PAY_BACK_MK", DataUtils.GetDBString(sda, 13));
                                map.Add("CASE_BACK_MK", DataUtils.GetDBString(sda, 14));
                                map.Add("PAY_METHOD", DataUtils.GetDBString(sda, 15));
                                map.Add("ISSETTLE", DataUtils.GetDBDateTime(sda, 16).HasValue);
                                map.Add("PAY_STATUS_MK", DataUtils.GetDBString(sda, 17));
                                map.Add("PAY_BANK", DataUtils.GetDBString(sda, 18));
                                map.Add("RSP_NAME", DataUtils.GetDBString(sda, 19));
                                map.Add("ECORDERID", DataUtils.GetDBString(sda, 20));
                                map.Add("L_RESPCODE", DataUtils.GetDBString(sda, 21));
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
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }
        public Boolean UpdateAPPLY_PAY(String[] APP_ID)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        dbc.CommandText = @"update APPLY_PAY set SETTLE_DATE = @SETTLE_DATE where APP_ID = @APP_ID ";
                        try
                        {
                            foreach (string temp in APP_ID)
                            {
                                if (!String.IsNullOrEmpty(temp))
                                {
                                    dbc.Parameters.Clear();
                                    DataUtils.AddParameters(dbc, "SETTLE_DATE", DateTime.Now);
                                    DataUtils.AddParameters(dbc, "APP_ID", temp);
                                    //dbc.Parameters.Add("@SETTLE_DATE", DateTime.Now);
                                    //dbc.Parameters.Add("@APP_ID", temp);
                                    dbc.ExecuteNonQuery();
                                }
                            }
                            dbc.Transaction.Commit();
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.Message, ex);
                            dbc.Transaction.Rollback();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public List<OptionModel> GetSERVICE_CATEoption(String UNIT_CD)
        {
            List<OptionModel> li = new List<OptionModel>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = @"SELECT NAME,SC_ID
                                            FROM SERVICE_CATE
                                            WHERE UNIT_CD = @UNIT_CD";
                        dbc.Parameters.Clear();
                        //dbc.Parameters.Add("@UNIT_CD", UNIT_CD);
                        DataUtils.AddParameters(dbc, "UNIT_CD", UNIT_CD);
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                OptionModel om = new OptionModel();
                                om.NAME = DataUtils.GetDBString(sda, 0);
                                om.VALUE = DataUtils.GetDBInt(sda, 1).ToString();
                                li.Add(om);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<OptionModel> GetSERVICEoption(String SC_ID)
        {
            List<OptionModel> li = new List<OptionModel>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = @"select NAME,SRV_ID
                                            FROM SERVICE
                                            where  (SC_PID = @SC_ID or SC_ID = @SC_ID) and ONLINE_N_MK='Y' ";
                        dbc.Parameters.Clear();
                        //dbc.Parameters.Add("@SC_ID", SC_ID);
                        DataUtils.AddParameters(dbc, "SC_ID", SC_ID);
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                OptionModel om = new OptionModel();
                                om.NAME = DataUtils.GetDBString(sda, 0);
                                om.VALUE = DataUtils.GetDBString(sda, 1);
                                li.Add(om);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public MaintainViewModel MaintainViewQuery(string app_id)
        {
            MaintainViewModel mvm = new MaintainViewModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = SqlModel.GetMaintainViewSQL();
                        dbc.CommandText = sql;
                        dbc.Parameters.Clear();
                        //dbc.Parameters.Add("@APP_ID", app_id);
                        DataUtils.AddParameters(dbc, "APP_ID", app_id);

                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                mvm.SRV_ID = DataUtils.GetDBString(sda, 0);
                                mvm.IDN = DataUtils.GetDBString(sda, 1);
                                mvm.CARD_IDN = DataUtils.GetDBString(sda, 2);
                                mvm.APP_TIME = (DataUtils.GetDBDateTime(sda, 3).HasValue ? DataUtils.GetDBDateTime(sda, 3).Value.ToString("yyyy/MM/dd") : "");
                                mvm.PAY_POINT = DataUtils.GetDBString(sda, 4);
                                mvm.PAY_A_FEE = DataUtils.GetDBInt(sda, 5);
                                mvm.PAY_A_PAID = DataUtils.GetDBInt(sda, 6);
                                mvm.PAY_C_FEE = DataUtils.GetDBInt(sda, 7);
                                mvm.PAY_C_PAID = DataUtils.GetDBInt(sda, 8);
                                mvm.SRV_NAME = DataUtils.GetDBString(sda, 9);
                                mvm.MOHW_CASE_NO = DataUtils.GetDBString(sda, 10);
                                mvm.FLOW_NAME = DataUtils.GetDBString(sda, 11);
                                mvm.UNIT_CD = DataUtils.GetDBInt(sda, 12);
                                mvm.FLOW_CD = DataUtils.GetDBString(sda, 13);
                                mvm.NAME = DataUtils.GetDBString(sda, 14);
                                mvm.SRC_SRV_ID = DataUtils.GetDBString(sda, 15);
                                mvm.APP_STR_DATE = DataUtils.GetDBDateTime(sda, 16).HasValue ? DataUtils.GetDBDateTime(sda, 16).Value.ToString("yyyy/MM/dd") : "";
                                mvm.CASE_BACK_MK = DataUtils.GetDBString(sda, 17);
                                mvm.PAY_BACK_MK = DataUtils.GetDBString(sda, 18);
                                mvm.CLOSE_MK = DataUtils.GetDBString(sda, 19);
                                mvm.PAY_WAY = DataUtils.GetDBString(sda, 20);
                                mvm.SETTLE_DATE = DataUtils.GetDBDateTime(sda, 21);
                                mvm.APP_ID = DataUtils.GetDBString(sda, 22);
                                mvm.SESSION_KEY = DataUtils.GetDBString(sda, 23);
                                mvm.PAY_STATUS = DataUtils.GetDBString(sda, 24);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return mvm;
        }

        public List<Map> MaintainViewDetailQuery(string app_id)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = @"select PAY_ID,PAY_METHOD,PAY_MONEY,PAY_STATUS_MK,PAY_ACT_TIME,PAY_DESC,SESSION_KEY
                                        from APPLY_PAY
                                        where APP_ID=@APP_ID order by PAY_ID ASC";
                        dbc.CommandText = sql;
                        dbc.Parameters.Clear();
                        //dbc.Parameters.Add("@APP_ID", app_id);
                        DataUtils.AddParameters(dbc, "APP_ID", app_id);

                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("PAY_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("PAY_METHOD", DataUtils.GetDBString(sda, 1));
                                map.Add("PAY_MONEY", DataUtils.GetDBInt(sda, 2));
                                map.Add("PAY_STATUS_MK", DataUtils.GetDBString(sda, 3));
                                map.Add("PAY_ACT_TIME", DataUtils.GetDBDateTime(sda, 4));
                                map.Add("PAY_DESC", DataUtils.GetDBString(sda, 5));
                                map.Add("SESSION_KEY", DataUtils.GetDBString(sda, 6));
                                li.Add(map);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public Map MaintainViewDetailMailDataQuery(string app_id)
        {
            Map map = new Map();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = @"select a.APP_TIME,s.NAME as SRV_NAME,IsNull(a.NAME,''),m.MAIL,u.UNIT_NAME
                                        from APPLY a
                                        LEFT JOIN service s on a.SRV_ID = s.SRV_ID
                                        LEFT JOIN MEMBER m on a.ACC_NO = m.ACC_NO
                                        LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
                                        where a.APP_ID=@APP_ID";
                        dbc.CommandText = sql;
                        dbc.Parameters.Clear();

                        DataUtils.AddParameters(dbc, "APP_ID", app_id);

                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {

                            if (sda.Read())
                            {
                                map.Add("APP_TIME", DataUtils.GetDBDateTime(sda, 0));
                                map.Add("SRV_NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("NAME", DataUtils.GetDBString(sda, 2));
                                map.Add("MAIL", DataUtils.GetDBString(sda, 3));
                                map.Add("UNIT_NAME", DataUtils.GetDBString(sda, 4));
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return map;
        }

        public int MaintainViewTotalPayQuery(string app_id)
        {
            int total_pay = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = @"select IsNull(sum(PAY_MONEY),0)
                                        from APPLY_PAY
                                        where APP_ID=@APP_ID and PAY_STATUS_MK='Y' ";
                        dbc.CommandText = sql;
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "APP_ID", app_id);

                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                total_pay = DataUtils.GetDBInt(sda, 0);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return total_pay;
        }

        public string NewPayID(string app_id)
        {
            string newid = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = @"select max(PAY_ID) as PAY_ID from APPLY_PAY where app_id = @APP_ID";
                        dbc.CommandText = sql;
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "APP_ID", app_id);

                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            if (sda.Read())
                            {
                                string temp = DataUtils.GetDBString(sda, 0);
                                newid = temp.Substring(0, 14);
                                int number = int.Parse(temp.Substring(14, 4));
                                newid = newid + ES.Utils.WebUtils.DefaultStringNumber(4, number + 1);
                            }
                            else
                            {
                                int year = int.Parse(app_id.Substring(0, 4));
                                newid = app_id.Substring(4, 10);
                                newid = ES.Utils.WebUtils.DefaultStringNumber(4, year - 1911) + newid + "0001";
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return newid;
        }




        public Boolean InsertAPPLY_PAY(string app_id, string tx_id, string pay_money_new, string paymethod_new)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        dbc.CommandText = @"insert into APPLY_PAY(APP_ID,PAY_ID,PAY_MONEY,PAY_ACT_TIME,PAY_EXT_TIME,PAY_INC_TIME,PAY_STATUS_MK,PAY_METHOD) values(
@APP_ID,@PAY_ID,@PAY_MONEY,@PAY_ACT_TIME,@PAY_EXT_TIME,@PAY_INC_TIME,@PAY_STATUS_MK,@PAY_METHOD)";
                        try
                        {
                            dbc.Parameters.Clear();
                            DataUtils.AddParameters(dbc, "APP_ID", app_id);
                            DataUtils.AddParameters(dbc, "PAY_ID", tx_id);
                            DataUtils.AddParameters(dbc, "PAY_MONEY", pay_money_new);
                            DataUtils.AddParameters(dbc, "PAY_ACT_TIME", DateTime.Now);
                            DataUtils.AddParameters(dbc, "PAY_EXT_TIME", DateTime.Now);
                            DataUtils.AddParameters(dbc, "PAY_INC_TIME", DateTime.Now);
                            DataUtils.AddParameters(dbc, "PAY_STATUS_MK", "Y");
                            DataUtils.AddParameters(dbc, "PAY_METHOD", paymethod_new);
                            /*
                            dbc.Parameters.Add("@APP_ID", app_id);
                            dbc.Parameters.Add("@PAY_ID", tx_id);
                            dbc.Parameters.Add("@PAY_MONEY", pay_money_new);
                            dbc.Parameters.Add("@PAY_ACT_TIME", DateTime.Now);
                            dbc.Parameters.Add("@PAY_EXT_TIME", DateTime.Now);
                            dbc.Parameters.Add("@PAY_INC_TIME", DateTime.Now);
                            dbc.Parameters.Add("@PAY_STATUS_MK", "Y");
                            dbc.Parameters.Add("@PAY_METHOD", paymethod_new);
                            */
                            dbc.ExecuteNonQuery();
                            dbc.Transaction.Commit();
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.Message, ex);
                            dbc.Transaction.Rollback();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public Boolean UpdateAPPLY_PAY_PAYMETHOD(String paymethod_org, string app_id, string tx_id)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        dbc.CommandText = @"update APPLY_PAY set PAY_METHOD=@PAY_METHOD,PAY_STATUS_MK='Y'
                                            ,PAY_EXT_TIME=@PAY_INC_TIME
                                            ,PAY_INC_TIME=@PAY_INC_TIME where APP_ID=@APP_ID and PAY_ID=@PAY_ID";
                        try
                        {
                            dbc.Parameters.Clear();

                            DataUtils.AddParameters(dbc, "PAY_METHOD", paymethod_org);
                            DataUtils.AddParameters(dbc, "PAY_INC_TIME", DateTime.Now);
                            DataUtils.AddParameters(dbc, "APP_ID", app_id);
                            DataUtils.AddParameters(dbc, "PAY_ID", tx_id);
                            /*
                            dbc.Parameters.Add("@PAY_METHOD", paymethod_org);
                            dbc.Parameters.Add("@PAY_INC_TIME", DateTime.Now);
                            dbc.Parameters.Add("@APP_ID", app_id);
                            dbc.Parameters.Add("@PAY_ID", tx_id);
                            */
                            dbc.ExecuteNonQuery();
                            dbc.Transaction.Commit();
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.Message, ex);
                            dbc.Transaction.Rollback();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public Boolean UpdateAPPLY_PAY_C_PAID(string app_id, string pay_cexpect_db)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        dbc.CommandText = @"update APPLY set PAY_C_PAID=@PAY_C_PAID where APP_ID=@APP_ID";
                        try
                        {
                            dbc.Parameters.Clear();
                            DataUtils.AddParameters(dbc, "PAY_C_PAID", pay_cexpect_db);
                            DataUtils.AddParameters(dbc, "APP_ID", app_id);
                            /*
                            dbc.Parameters.Add("@PAY_C_PAID", pay_cexpect_db);
                            dbc.Parameters.Add("@APP_ID", app_id);
                            */
                            dbc.ExecuteNonQuery();
                            dbc.Transaction.Commit();
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.Message, ex);
                            dbc.Transaction.Rollback();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public Boolean UpdateAPPLY_PAY_A_PAID(string app_id, int pay_aactual_new)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        dbc.CommandText = @"update APPLY set PAY_A_PAID=@PAY_A_PAID where APP_ID=@APP_ID";
                        try
                        {
                            dbc.Parameters.Clear();
                            DataUtils.AddParameters(dbc, "PAY_A_PAID", pay_aactual_new);
                            DataUtils.AddParameters(dbc, "APP_ID", app_id);
                            /*
                            dbc.Parameters.Add("@PAY_A_PAID", pay_aactual_new);
                            dbc.Parameters.Add("@APP_ID", app_id);
                            */
                            dbc.ExecuteNonQuery();
                            dbc.Transaction.Commit();
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.Message, ex);
                            dbc.Transaction.Rollback();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public List<Map> FormatReportOKQuery(string Sdate, string Fdate, string pay_status, string unitcd, string[] paymethod, string sort)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = SqlModel.GetFormatReportOKSQL();
                        dbc.Parameters.Clear();
                        if (!String.IsNullOrEmpty(Sdate))
                        {
                            sql += " and ap.PAY_INC_TIME >= @PAY_INC_TIME_S ";
                            DataUtils.AddParameters(dbc, "PAY_INC_TIME_S", Convert.ToDateTime(Sdate));
                            //dbc.Parameters.Add("@PAY_INC_TIME_S", Convert.ToDateTime(Sdate));
                            //logger.Debug("PAY_INC_TIME_S: " + Convert.ToDateTime(Sdate));
                        }
                        if (!String.IsNullOrEmpty(Fdate))
                        {
                            sql += " and ap.PAY_INC_TIME < DATEADD(DAY, 1, @PAY_INC_TIME_F)";
                            DataUtils.AddParameters(dbc, "PAY_INC_TIME_F", Convert.ToDateTime(Fdate));
                            //dbc.Parameters.Add("@PAY_INC_TIME_F", Convert.ToDateTime(Fdate));
                            //logger.Debug("PAY_INC_TIME_F: " + Convert.ToDateTime(Fdate));
                        }
                        if (pay_status.Equals("1"))
                        {
                            sql += " and ap.SETTLE_DATE is null ";
                        }
                        else if (pay_status.Equals("2"))
                        {
                            sql += " and ap.SETTLE_DATE is not null ";
                        }
                        if (!String.IsNullOrEmpty(unitcd))
                        {
                            sql += " and a.UNIT_CD = @UNIT_CD ";
                            DataUtils.AddParameters(dbc, "UNIT_CD", unitcd);
                            //dbc.Parameters.Add("@UNIT_CD", unitcd);
                            //logger.Debug("UNIT_CD: " + unitcd);
                        }
                        sql += " and (";
                        for (int i = 0; i < paymethod.Length; i++)
                        {
                            if (i == 0)
                            {
                                sql += " ap.PAY_METHOD = @PAY_METHOD" + i;
                                DataUtils.AddParameters(dbc, "PAY_METHOD" + i, paymethod[i]);
                                //dbc.Parameters.Add("@PAY_METHOD" + i, paymethod[i]);
                                //logger.Debug("PAY_METHOD: " + paymethod[i]);
                            }
                            else
                            {
                                sql += " or ap.PAY_METHOD = @PAY_METHOD" + i;
                                DataUtils.AddParameters(dbc, "PAY_METHOD" + i, paymethod[i]);
                                //dbc.Parameters.Add("@PAY_METHOD" + i, paymethod[i]);
                                //logger.Debug("PAY_METHOD: " + paymethod[i]);
                            }
                        }
                        sql += " ) ";

                        if (sort.Equals("1"))
                        {
                            sql += "order by a.APP_TIME";
                        }
                        else if (sort.Equals("2"))
                        {
                            sql += "order by ap.PAY_ACT_TIME";
                        }
                        else if (sort.Equals("3"))
                        {
                            sql += "order by ap.AUTH_DATE";
                        }
                        else
                        {
                            sql += "order by ap.PAY_INC_TIME";
                        }
                        logger.Debug("SQL: " + sql);

                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                int idx = 0;
                                Map map = new Map();
                                map.Add("APP_ID", DataUtils.GetDBString(sda, idx++));
                                map.Add("SRV_NAME", DataUtils.GetDBString(sda, idx++));
                                map.Add("APP_TIME", DataUtils.GetDBDateTime(sda, idx++));
                                map.Add("NAME", DataUtils.GetDBString(sda, idx++));
                                map.Add("PAY_POINT", DataUtils.GetDBString(sda, idx++));
                                map.Add("CLOSE_MK", DataUtils.GetDBString(sda, idx++));
                                map.Add("PAY_BACK_MK", DataUtils.GetDBString(sda, idx++));
                                map.Add("CASE_BACK_MK", DataUtils.GetDBString(sda, idx++));
                                map.Add("UNIT_NAME", DataUtils.GetDBString(sda, idx++));
                                map.Add("PAY_METHOD", DataUtils.GetDBString(sda, idx++));
                                map.Add("PAY_MONEY", DataUtils.GetDBInt(sda, idx++));
                                map.Add("PAY_PROFEE", DataUtils.GetDBInt(sda, idx++));
                                map.Add("PAY_DESC", DataUtils.GetDBString(sda, idx++));
                                map.Add("PAY_INC_TIME", DataUtils.GetDBDateTime(sda, idx++));
                                map.Add("PAY_ID", DataUtils.GetDBString(sda, idx++));
                                map.Add("SESSION_KEY", DataUtils.GetDBString(sda, idx++));
                                map.Add("ACCOUNT_NAME", DataUtils.GetDBString(sda, idx++));
                                map.Add("ACCOUNT_CD", DataUtils.GetDBString(sda, idx++));
                                map.Add("AUTH_NO", DataUtils.GetDBString(sda, idx++));
                                map.Add("AUTH_DATE", DataUtils.GetDBDateTime(sda, idx++));
                                map.Add("PAY_ACT_TIME", DataUtils.GetDBDateTime(sda, idx++));
                                map.Add("SETTLE_DATE", DataUtils.GetDBDateTime(sda, idx++));
                                map.Add("ENAME", DataUtils.GetDBString(sda, idx++));
                                li.Add(map);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<Map> FormatReportERRQuery(string Sdate, string Fdate, string unitcd, string[] ispay)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = SqlModel.GetFormatReportERRSQL();
                        dbc.Parameters.Clear();
                        if (!String.IsNullOrEmpty(Sdate))
                        {
                            sql += " and a.APP_TIME >= @APP_TIME_S ";
                            DataUtils.AddParameters(dbc, "APP_TIME_S", Convert.ToDateTime(Sdate));
                            //dbc.Parameters.Add("@APP_TIME_S", Convert.ToDateTime(Sdate));
                        }
                        if (!String.IsNullOrEmpty(Fdate))
                        {
                            sql += " and a.APP_TIME < DATEADD(DAY, 1, @APP_TIME_F) ";
                            DataUtils.AddParameters(dbc, "APP_TIME_F", Convert.ToDateTime(Fdate));
                            //dbc.Parameters.Add("@APP_TIME_F", Convert.ToDateTime(Fdate));
                        }
                        if (!String.IsNullOrEmpty(unitcd))
                        {
                            sql += " and a.UNIT_CD = @UNIT_CD ";
                            DataUtils.AddParameters(dbc, "UNIT_CD", unitcd);
                            //dbc.Parameters.Add("@UNIT_CD", unitcd);
                        }
                        if (ispay.Length == 1)
                        {
                            if (ispay[0].Equals("0"))
                            {
                                sql += " and a.PAY_A_FEE>a.PAY_A_PAID";
                            }
                            else
                            {
                                sql += " and a.PAY_A_FEE<a.PAY_A_PAID";
                            }
                        }
                        else if (ispay.Length == 2)
                        {
                            sql += " and a.PAY_A_FEE!=a.PAY_A_PAID ";
                        }

                        sql += " order by a.APP_ID Asc ";
                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("SRV_NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("APP_TIME", DataUtils.GetDBDateTime(sda, 2));
                                map.Add("NAME", DataUtils.GetDBString(sda, 3));
                                map.Add("PAY_POINT", DataUtils.GetDBString(sda, 4));
                                map.Add("PAY_A_FEE", DataUtils.GetDBInt(sda, 5));
                                map.Add("PAY_A_PAID", DataUtils.GetDBInt(sda, 6));
                                map.Add("PAY_C_FEE", DataUtils.GetDBInt(sda, 7));
                                map.Add("PAY_C_PAID", DataUtils.GetDBInt(sda, 8));
                                map.Add("CLOSE_MK", DataUtils.GetDBString(sda, 9));
                                map.Add("PAY_BACK_MK", DataUtils.GetDBString(sda, 10));
                                map.Add("CASE_BACK_MK", DataUtils.GetDBString(sda, 11));
                                map.Add("PAY_METHOD", DataUtils.GetDBString(sda, 12));
                                map.Add("UNIT_NAME", DataUtils.GetDBString(sda, 13));
                                map.Add("ENAME", DataUtils.GetDBString(sda, 14));
                                li.Add(map);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<Map> PayStoreFileSearchQuery(string Text_PayNumber, string Text_Trandate_S, string Text_Trandate_F, string Text_Paydate_S, string Text_Paydate_F)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = "select APP_ID,PAY_ACT_TIME,PAY_INC_TIME, PAY_MONEY from APPLY_PAY where 1=1 ";
                        dbc.Parameters.Clear();
                        if (!String.IsNullOrEmpty(Text_PayNumber))
                        {
                            sql += " and APP_ID like @APP_ID ";
                            DataUtils.AddParameters(dbc, "APP_ID", "%" + Text_PayNumber + "%");
                            //dbc.Parameters.Add("@APP_ID", "%" + Text_PayNumber + "%");
                        }

                        if (!String.IsNullOrEmpty(Text_Trandate_S))
                        {
                            sql += " and PAY_ACT_TIME >= @Text_Trandate_S ";
                            DataUtils.AddParameters(dbc, "Text_Trandate_S", Convert.ToDateTime(Text_Trandate_S));
                            //dbc.Parameters.Add("Text_Trandate_S", Convert.ToDateTime(Text_Trandate_S));
                        }
                        if (!String.IsNullOrEmpty(Text_Trandate_F))
                        {
                            sql += " and PAY_ACT_TIME < DATEADD(DAY, 1, @Text_Trandate_F)";
                            DataUtils.AddParameters(dbc, "Text_Trandate_F", Convert.ToDateTime(Text_Trandate_F));
                            //dbc.Parameters.Add("Text_Trandate_F", Convert.ToDateTime(Text_Trandate_F));
                        }

                        if (!String.IsNullOrEmpty(Text_Paydate_S))
                        {
                            sql += " and PAY_INC_TIME >= @Text_Paydate_S ";
                            DataUtils.AddParameters(dbc, "Text_Paydate_S", Convert.ToDateTime(Text_Paydate_S));
                            //dbc.Parameters.Add("Text_Paydate_S", Convert.ToDateTime(Text_Paydate_S));
                        }
                        if (!String.IsNullOrEmpty(Text_Paydate_F))
                        {
                            sql += " and PAY_INC_TIME <= @Text_Paydate_F ";
                            DataUtils.AddParameters(dbc, "Text_Paydate_F", Convert.ToDateTime(Text_Paydate_F));
                            //dbc.Parameters.Add("@Text_Paydate_F", Convert.ToDateTime(Text_Paydate_F));
                        }


                        sql += " order by PAY_INC_TIME desc ";
                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("PAY_ACT_TIME", DataUtils.GetDBDateTime(sda, 1));
                                map.Add("PAY_INC_TIME", DataUtils.GetDBDateTime(sda, 2));
                                map.Add("PAY_MONEY", DataUtils.GetDBInt(sda, 3));
                                li.Add(map);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public int UpdateStoreFile(List<Map> list, string updateAccount)
        {
            int count = 0;
            Dictionary<string, object> args = new Dictionary<string, object>();
            /*PAY_ACT_TIME 新增 PAY_EXT_TIME 繳費 PAY_INC_TIME 異動*/
            string updateSQL1 = @"
                UPDATE APPLY_PAY SET
                    PAY_STATUS_MK = 'Y',
                    PAY_EXT_TIME = CONVERT(DATETIME, @PAY_EXT_TIME, 112),
                    PAY_INC_TIME = GETDATE(),
                    AUTH_DATE = CONVERT(DATETIME, @AUTH_DATE, 112),
                    PAY_DESC = @PAY_DESC,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE DEL_MK = 'N'
                  AND PAY_METHOD = 'S'
                  AND PAY_MONEY = @PAY_MONEY
                  AND PAY_PROFEE = @PAY_PROFEE
                  AND SESSION_KEY = @SESSION_KEY
            ";

            string updateSQL2 = @"
                UPDATE APPLY SET
                    PAY_A_PAID = @PAY_A_PAID,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE APP_ID = (SELECT APP_ID FROM APPLY_PAY WHERE SESSION_KEY = @SESSION_KEY)
            ";

            try
            {
                foreach (Map item in list)
                {
                    logger.Debug("CLIENT_NO: " + item.GetString("CLIENT_NO"));

                    args.Clear();
                    args.Add("PAY_EXT_TIME", (item.GetInt("PAYMENT_DATE") + 19110000).ToString());
                    args.Add("AUTH_DATE", (item.GetInt("POST_DATE") + 19110000).ToString());
                    args.Add("PAY_DESC", item.GetString("CHANNEL"));
                    args.Add("PAY_MONEY", item.GetInt("COLLECTION_AMOUNT") - item.GetInt("FCC"));
                    args.Add("PAY_PROFEE", item.GetInt("FCC"));
                    args.Add("SESSION_KEY", item.GetString("CLIENT_NO"));
                    args.Add("UPD_FUN_CD", "ADM-STORE");
                    args.Add("UPD_ACC", updateAccount);

                    count += Update(updateSQL1, args);

                    args.Clear();
                    args.Add("PAY_A_PAID", item.GetInt("COLLECTION_AMOUNT") - item.GetInt("FCC"));
                    args.Add("SESSION_KEY", item.GetString("CLIENT_NO"));
                    args.Add("UPD_FUN_CD", "ADM-STORE");
                    args.Add("UPD_ACC", updateAccount);

                    Update(updateSQL2, args);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }

            return count;
        }

        private string GetOrderColumn(string column)
        {
            switch (column)
            {
                case "APP_ID":
                    return "APP_ID";
                case "APP_TIME":
                    return "APP_TIME";
                case "UNIT_CD":
                    return "UNIT_CD";
                case "SRV_ID":
                    return "SRV_ID";
            }

            return "";
        }

        private string GetOrderBy(string orderBy)
        {
            switch (orderBy)
            {
                case "ASC":
                    return "ASC";
                case "DESC":
                    return "DESC";
            }
            return "DESC";
        }


        public string GetPayMethod(string payMethod)
        {
            if (String.IsNullOrEmpty(payMethod)) return "";

            string[] t = payMethod.Split(',');

            StringBuilder sb = new StringBuilder();
            if (t.Contains("'C'")) sb.Append("'C',");
            if (t.Contains("'D'")) sb.Append("'D',");
            if (t.Contains("'T'")) sb.Append("'T',");
            if (t.Contains("'B'")) sb.Append("'B',");
            if (t.Contains("'S'")) sb.Append("'S',");


            if (sb.Length > 0)
            {
                return sb.ToString().Substring(0, sb.Length - 1);
            }

            return "";
        }

        public List<Dictionary<string, object>> GetCardList(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT P.SESSION_KEY, PA.ACCOUNT, PA.PSWD,
                    P.APP_ID, A.PAY_A_FEE,P.ECORDERID 
                FROM APPLY_PAY P
				LEFT JOIN APPLY A ON A.APP_ID = P.APP_ID
				LEFT JOIN SERVICE S ON S.SRV_ID = A.SRV_ID
                LEFT JOIN PAY_ACCOUNT PA ON S.PAY_ACCOUNT = PA.SRL_NO
                WHERE P.PAY_METHOD = 'C'
                  AND P.PAY_STATUS_MK = 'N'
                  AND P.APP_ID = @APP_ID
            ";

            args.Add("APP_ID", applyId);


            return GetList(querySQL, args);
        }

        public bool UpdateApplyPay(Dictionary<string, string> data)
        {
            String updateSQL1 = @"
                UPDATE APPLY_PAY SET
                    PAY_EXT_TIME = @PAY_EXT_TIME,
                    PAY_INC_TIME = @PAY_INC_TIME,
                    TRANS_RET = @TRANS_RET,
                    AUTH_DATE = @AUTH_DATE,
                    AUTH_NO = @AUTH_NO,
                    SETTLE_DATE = @SETTLE_DATE,
                    HOST_TIME = @HOST_TIME,
                    OTHER = @OTHER,
                    PAY_STATUS_MK = @PAY_STATUS_MK,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE APP_ID = @APP_ID
                  AND SESSION_KEY = @SESSION_KEY
            ";

            String updateSQL2 = @"
                UPDATE APPLY SET
                    PAY_A_PAID = PAY_A_PAID + @PAY_A_PAID,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE APP_ID = @APP_ID
            ";

            int flag = 0;

            using (SqlCommand cmd = new SqlCommand(updateSQL1, conn, tran))
            {

                if (GetData(data, "PAY_TRANS_RET").Equals("0000"))
                {
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", DateTime.Now);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", DateTime.Now);
                }
                else
                {
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", null);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", null);
                }

                DataUtils.AddParameters(cmd, "TRANS_RET", GetData(data, "PAY_TRANS_RET"));
                DataUtils.AddParameters(cmd, "AUTH_DATE", GetTime(data, "PAY_AUTH_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "AUTH_NO", GetData(data, "PAY_AUTH_NO"));
                DataUtils.AddParameters(cmd, "SETTLE_DATE", GetTime(data, "PAY_SETTLE_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "OTHER", GetData(data, "PAY_OTHER"));
                DataUtils.AddParameters(cmd, "HOST_TIME", GetTime(data, "PAY_HOST_TIME", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "PAY_STATUS_MK", (GetData(data, "PAY_TRANS_RET").Equals("0000") ? "Y" : "N"));
                DataUtils.AddParameters(cmd, "UPD_FUN_CD", "ADM-PAY");
                DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));
                DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                DataUtils.AddParameters(cmd, "SESSION_KEY", GetData(data, "PAY_SESSION_KEY"));

                flag += cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(updateSQL2, conn, tran))
            {
                DataUtils.AddParameters(cmd, "PAY_A_PAID", GetData(data, "ADM-PAY"));
                DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));
                DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));

                flag += cmd.ExecuteNonQuery();
            }

            if (flag == 2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 聯合信用卡 產生請款資料 明細
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<Map> exportECDetailList(MaintainECModel model)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        String sql = SqlModel.exportECDetailListSQL();

                        dbc.Parameters.Clear();
                        if (!String.IsNullOrEmpty(model.APP_ID_BEGIN))
                        {
                            sql += "and a.APP_ID>=@APP_ID_BEGIN ";
                            DataUtils.AddParameters(dbc, "APP_ID_BEGIN", model.APP_ID_BEGIN);
                        }
                        if (!String.IsNullOrEmpty(model.APP_ID_END))
                        {
                            sql += "and a.APP_ID<=@APP_ID_END ";
                            DataUtils.AddParameters(dbc, "APP_ID_END", model.APP_ID_END);
                        }
                        if (model.APP_TIME_BEGIN.HasValue)
                        {
                            sql += "and a.APP_TIME>=@APP_TIME_BEGIN ";
                            DataUtils.AddParameters(dbc, "APP_TIME_BEGIN", model.APP_TIME_BEGIN);
                        }
                        if (model.APP_TIME_BEGIN.HasValue)
                        {
                            sql += "and a.APP_TIME<DATEADD(DAY, 1, @APP_TIME_END) ";
                            DataUtils.AddParameters(dbc, "APP_TIME_END", model.APP_TIME_END);
                        }
                        if (!String.IsNullOrEmpty(model.SC_PID))
                        {
                            sql += "and s.SC_PID=@SC_PID ";
                            DataUtils.AddParameters(dbc, "SC_PID", model.SC_PID);
                        }
                        if (!String.IsNullOrEmpty(model.SRV_ID))
                        {
                            sql += "and a.SRV_ID=@SRV_ID ";
                            DataUtils.AddParameters(dbc, "SRV_ID", model.SRV_ID);
                        }
                        sql += "order by a.app_id ";
                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("A_MERCHANTID", DataUtils.GetConfig("PAY_EC_MERCHANTID"));
                                map.Add("B_TRMINALID", DataUtils.GetConfig("PAY_EC_TRMINALID"));
                                map.Add("C_ECORDERID", DataUtils.GetDBString(sda, 0));
                                map.Add("D_EMPTY", DataUtils.GetDBString(sda, 1));
                                map.Add("E_MOENY", DataUtils.GetDBString(sda, 2));
                                map.Add("F_AUTHNO", DataUtils.GetDBString(sda, 3));
                                map.Add("G_AUTHCODE", DataUtils.GetDBString(sda, 4));
                                map.Add("H_AUTHDATE", DataUtils.GetDBString(sda, 5));
                                map.Add("I_PRIVATE", DataUtils.GetDBString(sda, 6));
                                map.Add("J_EMPTY", DataUtils.GetDBString(sda, 7));
                                map.Add("PAY_MONEY", DataUtils.GetDBInt(sda, 8));
                                map.Add("K_EMPTY_RETURN", DataUtils.GetDBString(sda, 9));
                                li.Add(map);
                            }
                            sda.Close();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        /// <summary>
        /// 聯合信用卡 儲存請款回傳資料 明細
        /// </summary>
        /// <param name="list"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int UpdateECDetailFile(List<RspModel> list, string updateAccount)
        {
            int count = 0;
            Dictionary<string, object> args = new Dictionary<string, object>();
            string insertSQL = @"
                INSERT INTO [dbo].[PAY_ECRSP]([ADD_TIME],[ADD_FUN_CD],[ADD_ACC],[DEL_MK],[DEL_TIME],[DEL_ACC],[A_MERCHANTID],[B_TRMINALID],[C_ECID],[D_EMPTY],[E_MONEY]
           ,[F_TRACECODE],[G_TRADETYPE],[H_TRADEDATE],[I_CUSTOMIZE],[J_CARDINFO],[K_PAYDEALDATE],[L_RESPCODE],[M_RESPMSG],[N_BATCH],[O_REDTYPE],[P_REDINSTALL]
           ,[Q_REDFIRSTPAY],[R_REDEACHPAY],[S_REDEACHFEE],[T_REDPOINT],[U_PLUS],[V_REDPOINTB],[W_REDPAY],[X_PAYDAY],[Y_3DRESULT],[Z_FORG],[ZA_SYSFILE],[ZB_EMPTY])
     VALUES
           (getdate(),'ADM-EC',@ADD_ACC,'N',null,null,@A_MERCHANTID,@B_TRMINALID,@C_ECID,@D_EMPTY,@E_MONEY,@F_TRACECODE,@G_TRADETYPE,@H_TRADEDATE
           ,@I_CUSTOMIZE,@J_CARDINFO,@K_PAYDEALDATE,@L_RESPCODE,@M_RESPMSG,@N_BATCH,@O_REDTYPE
           ,@P_REDINSTALL,@Q_REDFIRSTPAY,@R_REDEACHPAY,@S_REDEACHFEE,@T_REDPOINT,@U_PLUS,@V_REDPOINTB
           ,@W_REDPAY,@X_PAYDAY,@Y_3DRESULT,@Z_FORG,@ZA_SYSFILE,@ZB_EMPTY)
            ";

            /*PAY_ACT_TIME 新增 PAY_EXT_TIME 繳費 PAY_INC_TIME 異動*/
            string updateSQL1 = @"
                UPDATE APPLY_PAY SET
                    PAY_STATUS_MK = 'Y',
                    PAY_EXT_TIME = CONVERT(DATETIME, @PAY_EXT_TIME, 112),
                    PAY_INC_TIME = GETDATE(),
                    AUTH_DATE = CONVERT(DATETIME, @AUTH_DATE, 112),
                    PAY_DESC = @PAY_DESC,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE DEL_MK = 'N'
                  AND PAY_METHOD = 'S'
                  AND PAY_MONEY = @PAY_MONEY
                  AND PAY_PROFEE = @PAY_PROFEE
                  AND SESSION_KEY = @SESSION_KEY
            ";

            string updateSQL2 = @"
                UPDATE APPLY SET
                    PAY_A_PAID = @PAY_A_PAID,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE APP_ID = (SELECT APP_ID FROM APPLY_PAY WHERE SESSION_KEY = @SESSION_KEY)
            ";

            try
            {
                foreach (var item in list)
                {
                    logger.Debug("UpdateECDetailFile: start");

                    args.Clear();
                    args.Add("@ADD_ACC", updateAccount);
                    args.Add("@A_MERCHANTID", Convert.ToString(item.A_001_010_10));
                    args.Add("@B_TRMINALID", Convert.ToString(item.B_011_018_8));
                    args.Add("@C_ECID", Convert.ToString(item.C_019_058_40));
                    args.Add("@D_EMPTY", Convert.ToString(item.D_059_077_19));
                    args.Add("@E_MONEY", Convert.ToString(item.E_078_085_8));
                    args.Add("@F_TRACECODE", Convert.ToString(item.F_086_093_8));
                    args.Add("@G_TRADETYPE", Convert.ToString(item.G_094_095_2));
                    args.Add("@H_TRADEDATE", Convert.ToString(item.H_096_103_8));
                    args.Add("@I_CUSTOMIZE", Convert.ToString(item.I_104_119_16));
                    args.Add("@J_CARDINFO", Convert.ToString(item.J_120_159_40));
                    args.Add("@K_PAYDEALDATE", Convert.ToString(item.K_160_165_6));
                    args.Add("@L_RESPCODE", Convert.ToString(item.L_166_168_3));
                    args.Add("@M_RESPMSG", Convert.ToString(item.M_169_184_16));
                    args.Add("@N_BATCH", Convert.ToString(item.N_185_190_6));
                    args.Add("@O_REDTYPE", Convert.ToString(item.O_191_191_1));
                    args.Add("@P_REDINSTALL", Convert.ToString(item.P_192_193_2));
                    args.Add("@Q_REDFIRSTPAY", Convert.ToString(item.Q_194_201_8));
                    args.Add("@R_REDEACHPAY", Convert.ToString(item.R_202_209_8));
                    args.Add("@S_REDEACHFEE", Convert.ToString(item.S_210_215_6));
                    args.Add("@T_REDPOINT", Convert.ToString(item.T_216_223_8));
                    args.Add("@U_PLUS", Convert.ToString(item.U_224_224_1));
                    args.Add("@V_REDPOINTB", Convert.ToString(item.V_225_232_8));
                    args.Add("@W_REDPAY", Convert.ToString(item.W_233_242_10));
                    args.Add("@X_PAYDAY", Convert.ToString(item.X_243_250_8));
                    args.Add("@Y_3DRESULT", Convert.ToString(item.Y_251_251_1));
                    args.Add("@Z_FORG", Convert.ToString(item.Z_252_252_1));
                    args.Add("@ZA_SYSFILE", Convert.ToString(item.ZA_253_253_1));
                    args.Add("@ZB_EMPTY", Convert.ToString(item.ZB_254_270_17));

                    count += Update(insertSQL, args);

                    //args.Clear();
                    //args.Add("PAY_EXT_TIME", (item.GetInt("PAYMENT_DATE") + 19110000).ToString());
                    //args.Add("AUTH_DATE", (item.GetInt("POST_DATE") + 19110000).ToString());
                    //args.Add("PAY_DESC", item.GetString("CHANNEL"));
                    //args.Add("PAY_MONEY", item.GetInt("COLLECTION_AMOUNT") - item.GetInt("FCC"));
                    //args.Add("PAY_PROFEE", item.GetInt("FCC"));
                    //args.Add("SESSION_KEY", item.GetString("CLIENT_NO"));
                    //args.Add("UPD_FUN_CD", "ADM-STORE");
                    //args.Add("UPD_ACC", updateAccount);

                    //count += Update(updateSQL1, args);

                    //args.Clear();
                    //args.Add("PAY_A_PAID", item.GetInt("COLLECTION_AMOUNT") - item.GetInt("FCC"));
                    //args.Add("SESSION_KEY", item.GetString("CLIENT_NO"));
                    //args.Add("UPD_FUN_CD", "ADM-STORE");
                    //args.Add("UPD_ACC", updateAccount);

                    //Update(updateSQL2, args);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }

            return count;
        }

        /// <summary>
        /// EXCEL 匯出 簡易專案
        /// </summary>
        /// <param name="strTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable CreateExcelPayEC(PayECModel form)
        {
            var model = form.queryModel;

            DataTable resultTable = new DataTable();


            string s_col_c1 = "APP_ID,SRV_NAME,APP_TIME,NAME,PAY_METHOD,RSP_NAME,PAY_A_PAID,PAID_FEE";
            string[] sa_col_c1 = s_col_c1.Split(',');
            string s_col_c2 = "案件編號,申辦項目,申辦日期,申請人姓名,繳費方式,請款狀態,金額,手續費";
            string[] sa_col_c2 = s_col_c2.Split(',');

            StringBuilder sb = new StringBuilder();
            try
            {
                List<Map> li = new List<Map>();
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    using (SqlTransaction st = conn.BeginTransaction())
                    {
                        using (SqlCommand dbc = conn.CreateCommand())
                        {
                            dbc.Transaction = st;
                            String sql = @"SELECT a.APP_ID,s.NAME as SRV_NAME,convert(varchar,a.APP_TIME,111) as APP_TIME
,a.NAME,a.PAY_A_PAID,case when a.PAY_METHOD='C' and PAY_BANK='EC' then '信用卡(EC)' else a.PAY_METHOD end as PAY_METHOD
,case WHEN rsp.L_RESPCODE IS NULL THEN '尚未請款' WHEN RSP.L_RESPCODE='00' THEN '成功' ELSE '失敗' END AS RSP_NAME
,case when PAY_A_PAID <1000 AND rsp.L_RESPCODE='00' then '2' when PAY_A_PAID >=1000 AND rsp.L_RESPCODE='00' then 7 end as PAID_FEE
from APPLY a
LEFT JOIN APPLY_PAY ap on ap.APP_ID = a.APP_ID
LEFT JOIN SERVICE s on a.SRV_ID = s.SRV_ID
LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
LEFT JOIN M_CASE_STATUS msc on msc.UNIT_SCD = u.UNIT_SCD and a.FLOW_CD = msc.FLOW_CD
LEFT JOIN PAY_ECRSP RSP ON RSP.C_ECID=AP.ECORDERID
where /*msc.CASE_CAT = 4 and*/ a.PAY_POINT in ('B','C','D') and a.PAY_METHOD ='C' 
and A.PAY_A_FEE <= A.PAY_A_PAID AND (SELECT MAX(SETTLE_DATE) FROM APPLY_PAY WHERE APP_ID = A.APP_ID) IS NULL 
and PAY_BANK='EC' AND ECORDERID IS NOT NULL AND AUTH_NO IS NOT NULL AND AUTH_DATE IS NOT NULL  ";

                            dbc.Parameters.Clear();
                            if (!String.IsNullOrEmpty(model.APP_ID_BEGIN))
                            {
                                sql += "and a.APP_ID>=@APP_ID_BEGIN ";
                                DataUtils.AddParameters(dbc, "APP_ID_BEGIN", model.APP_ID_BEGIN);
                            }
                            if (!String.IsNullOrEmpty(model.APP_ID_END))
                            {
                                sql += "and a.APP_ID<=@APP_ID_END ";
                                DataUtils.AddParameters(dbc, "APP_ID_END", model.APP_ID_END);
                            }
                            if (model.APP_TIME_BEGIN.HasValue)
                            {
                                sql += "and a.APP_TIME>=@APP_TIME_BEGIN ";
                                DataUtils.AddParameters(dbc, "APP_TIME_BEGIN", model.APP_TIME_BEGIN);
                            }
                            if (model.APP_TIME_BEGIN.HasValue)
                            {
                                sql += "and a.APP_TIME<DATEADD(DAY, 1, @APP_TIME_END) ";
                                DataUtils.AddParameters(dbc, "APP_TIME_END", model.APP_TIME_END);
                            }
                            if (!String.IsNullOrEmpty(model.SC_PID))
                            {
                                sql += "and s.SC_PID=@SC_PID ";
                                DataUtils.AddParameters(dbc, "SC_PID", model.SC_PID);
                            }
                            if (!String.IsNullOrEmpty(model.SRV_ID))
                            {
                                sql += "and a.SRV_ID=@SRV_ID ";
                                DataUtils.AddParameters(dbc, "SRV_ID", model.SRV_ID);
                            }
                            if (!string.IsNullOrEmpty(model.RSP_STATUS))
                            {
                                //請款狀態
                                if (model.RSP_STATUS == "Y")
                                {
                                    sql += "and RSP.L_RESPCODE='00' ";
                                }
                                else if (model.RSP_STATUS == "N")
                                {
                                    sql += "and RSP.L_RESPCODE<>'00' ";
                                }
                            }
                            sql += "order by a.app_id ";
                            dbc.CommandText = sql;
                            resultTable.Load(dbc.ExecuteReader());
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                }

                //new 一個新的table來render to excel
                foreach (DataColumn column in resultTable.Columns.Cast<DataColumn>().AsQueryable().ToList())
                {
                    for (int i = 0; i < sa_col_c1.Length; i++)
                    {
                        if (column.ColumnName.Equals(sa_col_c1[i])) { column.ColumnName = sa_col_c2[i]; }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }

            return resultTable;
        }

        private string GetData(Dictionary<string, string> data, string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            return "";
        }

        private DateTime? GetTime(Dictionary<string, string> data, string key, string format)
        {
            if (data.ContainsKey(key) && !String.IsNullOrEmpty(data[key]))
            {
                return DateTime.ParseExact(data[key], format, cultureStyle);
            }
            return null;
        }

        private IFormatProvider cultureStyle = new System.Globalization.CultureInfo("zh-TW", true);
    }
}