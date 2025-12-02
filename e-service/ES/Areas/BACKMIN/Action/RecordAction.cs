using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Action;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;
using ES.Utils;

namespace ES.Areas.Admin.Action
{
    public class RecordAction : BaseAction
    {

        public List<Map> GetLOGIN_LOG(LoginRecordQueryModel model)
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
                        String sql = @"select LOGIN_ID,NAME,UNIT_CD,IP_ADDR,STATUS,LOGIN_TIME
                                        from LOGIN_LOG
                                        where 1=1 ";

                        dbc.Parameters.Clear();

                        if (!String.IsNullOrEmpty(model.Account))
                        {
                            sql += " and LOGIN_ID = @LOGIN_ID ";
                            DataUtils.AddParameters(dbc, "LOGIN_ID", model.Account);
                            //dbc.Parameters.Add("@LOGIN_ID", model.Account);
                        }

                        if (!String.IsNullOrEmpty(model.StartDate))
                        {
                            sql += " and LOGIN_TIME>=@TIME_S ";
                            DataUtils.AddParameters(dbc, "TIME_S", model.StartDate);
                            //dbc.Parameters.Add("@TIME_S", model.StartDate);
                        }

                        if (!String.IsNullOrEmpty(model.EndDate))
                        {
                            sql += " and LOGIN_TIME<DateAdd(Day, 1, CONVERT(DATETIME, @TIME_F, 111)) ";
                            DataUtils.AddParameters(dbc, "TIME_F", model.EndDate);
                            //dbc.Parameters.Add("@TIME_F", model.EndDate);
                        }

                        sql += " order by LOGIN_TIME DESC";

                        //logger.Debug("SQL: " + sql + " / " + model.EndDate);

                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("LOGIN_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("UNIT_CD", DataUtils.GetDBInt(sda, 2));
                                map.Add("IP_ADDR", DataUtils.GetDBString(sda, 3));
                                map.Add("STATUS", DataUtils.GetDBString(sda, 4));
                                map.Add("LOGIN_TIME", DataUtils.GetDBDateTime(sda, 5).HasValue ? DataUtils.GetDBDateTime(sda, 5).Value.ToString("yyyy/MM/dd HH:mm:ss") : "");
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


        public List<Map> GetLOGIN_LOGERR(LoginRecordQueryModel model)
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
                        String sql = @"select LOGIN_ID,NAME,UNIT_CD,IP_ADDR,STATUS,LOGIN_TIME,FAIL_COUNT
                                        from LOGIN_LOG
                                        where 1=1 ";

                        dbc.Parameters.Clear();

                        if (!String.IsNullOrEmpty(model.Account))
                        {
                            sql += " and LOGIN_ID = @LOGIN_ID ";
                            DataUtils.AddParameters(dbc, "LOGIN_ID", model.Account);
                            //dbc.Parameters.Add("@LOGIN_ID", model.Account);
                        }

                        if (!String.IsNullOrEmpty(model.StartDate))
                        {
                            sql += " and LOGIN_TIME>=@TIME_S ";
                            DataUtils.AddParameters(dbc, "TIME_S", model.StartDate);
                            //dbc.Parameters.Add("@TIME_S", model.StartDate);
                        }

                        if (!String.IsNullOrEmpty(model.EndDate))
                        {
                            sql += " and LOGIN_TIME< DATEADD(DAY, 1, @TIME_F) ";
                            DataUtils.AddParameters(dbc, "TIME_F", model.EndDate);
                            //dbc.Parameters.Add("@TIME_F", model.EndDate);
                        }

                        sql += " and FAIL_COUNT>=@FAIL_COUNT ";
                        DataUtils.AddParameters(dbc, "FAIL_COUNT", model.ErrTimes);
                        //dbc.Parameters.Add("@FAIL_COUNT", model.ErrTimes);

                        sql += " order by LOGIN_TIME DESC";

                        //logger.Debug("SQL: " + sql + " / ErrTimes: " + model.ErrTimes);

                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("LOGIN_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("UNIT_CD", DataUtils.GetDBInt(sda, 2));
                                map.Add("IP_ADDR", DataUtils.GetDBString(sda, 3));
                                map.Add("STATUS", DataUtils.GetDBString(sda, 4));
                                map.Add("LOGIN_TIME", DataUtils.GetDBDateTime(sda, 5).HasValue ? DataUtils.GetDBDateTime(sda, 5).Value.ToString("yyyy/MM/dd HH:mm:ss") : "");
                                map.Add("FAIL_COUNT", DataUtils.GetDBInt(sda, 6));
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

        public List<Map> ModifySearchQuery(ModifyLogModel model)
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
                        String sql = @"select tl.TX_TIME,tl.TX_TYPE,tl.TX_LOGIN_ID,tl.TX_LOGIN_NAME,tl.TX_DESC,u.UNIT_NAME,tl.TX_CATE_CD
                                        from tx_log tl
                                        LEFT JOIN unit u on tl.TX_UNIT_CD = u.UNIT_CD
                                        where 1=1 ";

                        dbc.Parameters.Clear();

                        if (!String.IsNullOrEmpty(model.TYPE))
                        {
                            sql += " and tl.TX_CATE_CD>=@TX_CATE_CD ";
                            DataUtils.AddParameters(dbc, "TX_CATE_CD", model.TYPE);
                            //dbc.Parameters.Add("@TX_CATE_CD", model.TYPE);
                            if (model.TYPE.Equals("5"))
                            {
                                sql += " and tl.TX_DESC like @TX_DESC ";
                                //dbc.Parameters.Add("@TX_DESC", "%" + model.ID + "%");
                                DataUtils.AddParameters(dbc, "TX_DESC", "%" + model.ID + "%");
                            }
                        }
                        if (!String.IsNullOrEmpty(model.TIME_S))
                        {
                            sql += " and tl.TX_TIME>=@TIME_S ";
                            //dbc.Parameters.Add("@TIME_S", model.TIME_S);
                            DataUtils.AddParameters(dbc, "TIME_S", model.TIME_S);
                        }

                        if (!String.IsNullOrEmpty(model.TIME_F))
                        {
                            sql += " and tl.TX_TIME<DATEADD(DAY, 1, @TIME_F)";
                            //dbc.Parameters.Add("@TIME_F", model.TIME_F);
                            DataUtils.AddParameters(dbc, "TIME_F", model.TIME_F);
                        }

                        sql += " order by tl.TX_TIME desc ";
                        dbc.CommandText = sql;
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("TX_TIME", (DataUtils.GetDBDateTime(sda, 0).HasValue ? DataUtils.GetDBDateTime(sda, 0).Value.ToString("yyyy/MM/dd") : ""));
                                map.Add("TX_TYPE", DataUtils.GetDBInt(sda, 1));
                                map.Add("TX_LOGIN_ID", DataUtils.GetDBString(sda, 2));
                                map.Add("TX_LOGIN_NAME", DataUtils.GetDBString(sda, 3));
                                map.Add("TX_DESC", DataUtils.GetDBString(sda, 4));
                                map.Add("UNIT_NAME", DataUtils.GetDBString(sda, 5));
                                map.Add("TX_CATE_CD", DataUtils.GetDBString(sda, 6));
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


        public List<Map> MailSearchQuery(MailLogModel model)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"select a.SUBJECT,a.BODY,a.SEND_TIME,a.MAIL,a.RESULT_MK,isnull(s.name,'') as APPNAME 
                                        from mail_log a 
                                        left join service s on a.SRV_ID=s.SRV_ID 
                                        where 1 = 1 ";

                    dbc.Parameters.Clear();

                    if (!String.IsNullOrEmpty(model.MAIL))
                    {
                        sql += " and a.mail like @mail ";

                        DataUtils.AddParameters(dbc, "mail", "%" + model.MAIL + "%");
                        //dbc.Parameters.Add("@mail", "%"+model.MAIL+"%");
                    }
                    if (!String.IsNullOrEmpty(model.TIME_S))
                    {
                        sql += " and a.SEND_TIME>=@TIME_S ";
                        DataUtils.AddParameters(dbc, "TIME_S", model.TIME_S);
                        //dbc.Parameters.Add("@TIME_S", model.TIME_S);
                    }
                    if (!String.IsNullOrEmpty(model.TIME_F))
                    {
                        sql += " and a.SEND_TIME < DATEADD(DAY, 1, CONVERT(DATETIME, @TIME_F, 111)) ";
                        DataUtils.AddParameters(dbc, "TIME_F", model.TIME_F);
                        //dbc.Parameters.Add("@TIME_F", model.TIME_F);
                    }
                    if (!String.IsNullOrEmpty(model.RESULT_MK))
                    {
                        sql += " and a.RESULT_MK = @RESULT_MK ";
                        DataUtils.AddParameters(dbc, "RESULT_MK", model.RESULT_MK);
                        //dbc.Parameters.Add("@RESULT_MK", model.RESULT_MK);
                    }
                    if (!String.IsNullOrEmpty(model.SRV_ID))
                    {
                        sql += " and a.SRV_ID = @SRV_ID ";
                        DataUtils.AddParameters(dbc, "SRV_ID", model.SRV_ID);
                        //dbc.Parameters.Add("@SRV_ID", model.SRV_ID);
                    }
                    sql += " order by a." + GetOrderColumn(model.ORDER_FIELD) + " " + GetOrderBy(model.ORDER_BY);
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("SUBJECT", DataUtils.GetDBString(sda, 0));
                            map.Add("BODY", DataUtils.GetDBString(sda, 1));
                            map.Add("SEND_TIME", (DataUtils.GetDBDateTime(sda, 2).HasValue ? DataUtils.GetDBDateTime(sda, 2).Value.ToString("yyyy/MM/dd HH:mm:ss") : ""));
                            map.Add("MAIL", DataUtils.GetDBString(sda, 3));
                            map.Add("RESULT_MK", DataUtils.GetDBString(sda, 4).Equals("Y") ? "成功" : "失敗");
                            map.Add("APPNAME", DataUtils.GetDBString(sda, 5));
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

        private string GetOrderColumn(string column)
        {
            switch (column)
            {
                case "send_time":
                    return "send_time";
                case "mail":
                    return "mail";
            }

            return "send_time";
        }

        private string GetOrderBy(string orderBy)
        {
            switch (orderBy)
            {
                case "asc":
                    return "asc";
                case "desc":
                    return "desc";
            }
            return "desc";
        }
    }
}