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
using NPOI.OpenXmlFormats.Dml.ChartDrawing;
using ES.Commons;

namespace ES.Areas.Admin.Action
{
    public class ReportAction : BaseAction
    {
        #region 使用者帳號清單
        public DataTable AccountReport(string permission)
        {
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand dbc = conn.CreateCommand();
            SqlDataAdapter sda = new SqlDataAdapter(dbc);
            DataSet dataSet = new DataSet();
            DataTable resultTable = new DataTable();
            try
            {
                dbc.CommandText = @"SELECT 
	(SELECT UNIT_NAME + (CASE WHEN UNIT_LEVEL = 2 THEN (SELECT ' (' + UNIT_NAME + ')' FROM UNIT 
	WHERE UNIT_CD = U.UNIT_PCD) ELSE '' END) 
    FROM UNIT U WHERE UNIT_CD = A.UNIT_CD) AS UNIT_NAME
	, NAME
	, ACC_NO
	, (CASE WHEN  ADMIN_SCOPE = '0' THEN '所屬單位/一般使用者' 
			WHEN  ADMIN_SCOPE = '1' THEN '所有單位/管理者'
			WHEN  ADMIN_SCOPE = '2' THEN '隸屬單位/一般使用者' 
            ELSE '不明' END) AS ADMIN_SCOPE
	, UPD_TIME
	, '□續用□不再使用□其他___________' as viewresult
	, '' as usersign
    FROM ADMIN A 
	WHERE DEL_MK = 'N'
";
                switch (permission)
                {
                    case "all":
                        break;
                    case "0":  //所屬單位
                        dbc.CommandText += @"AND ADMIN_SCOPE = '0' ";
                        break;
                    case "1":  //所有單位
                        dbc.CommandText += @"AND ADMIN_SCOPE = '1' ";
                        break;
                    case "2":  //隸屬單位
                        dbc.CommandText += @"AND ADMIN_SCOPE = '2' ";
                        break;
                }


                sda.Fill(dataSet, "report");

                resultTable.Columns.Add("單位");
                resultTable.Columns.Add("姓名");
                resultTable.Columns.Add("帳號名稱");
                resultTable.Columns.Add("使用者權限/使用者群組");
                resultTable.Columns.Add("最後更新日期");
                resultTable.Columns.Add("檢視結果");
                resultTable.Columns.Add("檢視人員核章");

                int row_index = 0;
                //將select結果塞進新table
                foreach (DataRow row in dataSet.Tables["report"].Rows)
                {
                    resultTable.Rows.Add();
                    resultTable.Rows[row_index]["單位"] = dataSet.Tables["report"].Rows[row_index]["UNIT_NAME"];
                    resultTable.Rows[row_index]["姓名"] = dataSet.Tables["report"].Rows[row_index]["NAME"];
                    resultTable.Rows[row_index]["帳號名稱"] = dataSet.Tables["report"].Rows[row_index]["ACC_NO"];
                    resultTable.Rows[row_index]["使用者權限/使用者群組"] = dataSet.Tables["report"].Rows[row_index]["ADMIN_SCOPE"];
                    resultTable.Rows[row_index]["最後更新日期"] = dataSet.Tables["report"].Rows[row_index]["UPD_TIME"];
                    resultTable.Rows[row_index]["檢視結果"] = dataSet.Tables["report"].Rows[row_index]["viewresult"];
                    resultTable.Rows[row_index]["檢視人員核章"] = dataSet.Tables["report"].Rows[row_index]["usersign"];

                    ++row_index;
                }
            }
            finally
            {
                sda.Dispose();
                dbc.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return resultTable;
        }

        #endregion

        #region 申辦統計
        public List<CaseSumResultModel> GetCaseSumReport(CaseSumModel model)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).ToString("yyyy/MM/dd");
            List<CaseSumResultModel> li = new List<CaseSumResultModel>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = LongWithSql.GetCaseSumResultSql();
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", Convert.ToDateTime(model.Fdate).AddDays(1));
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                CaseSumResultModel crm = new CaseSumResultModel();
                                crm.name = sda["UNIT_NAME"].ToString();
                                crm.newcase = String.IsNullOrEmpty(sda["newcase"].ToString()) ? 0 : int.Parse(sda["newcase"].ToString());
                                crm.procase = String.IsNullOrEmpty(sda["procase"].ToString()) ? 0 : int.Parse(sda["procase"].ToString());
                                crm.okcase = String.IsNullOrEmpty(sda["okcase"].ToString()) ? 0 : int.Parse(sda["okcase"].ToString());
                                crm.ok_delay = String.IsNullOrEmpty(sda["ok_delay"].ToString()) ? 0 : int.Parse(sda["ok_delay"].ToString());
                                crm.unokcase = String.IsNullOrEmpty(sda["unokcase"].ToString()) ? 0 : int.Parse(sda["unokcase"].ToString());
                                crm.unok_delay = String.IsNullOrEmpty(sda["unok_delay"].ToString()) ? 0 : int.Parse(sda["unok_delay"].ToString());
                                crm.moica = String.IsNullOrEmpty(sda["moica"].ToString()) ? 0 : int.Parse(sda["moica"].ToString());
                                crm.moeaca = String.IsNullOrEmpty(sda["moeaca"].ToString()) ? 0 : int.Parse(sda["moeaca"].ToString());
                                crm.hca0 = String.IsNullOrEmpty(sda["hca0"].ToString()) ? 0 : int.Parse(sda["hca0"].ToString());
                                crm.hca1 = String.IsNullOrEmpty(sda["hca1"].ToString()) ? 0 : int.Parse(sda["hca1"].ToString());
                                crm.NEWEID = String.IsNullOrEmpty(sda["NEWEID"].ToString()) ? 0 : int.Parse(sda["NEWEID"].ToString());
                                crm.sumCase = crm.newcase + crm.procase;
                                crm.sumOk = crm.okcase + crm.ok_delay;
                                crm.sumUnok = crm.sumCase - crm.sumOk;
                                crm.member = crm.newcase - crm.moica - crm.moeaca - crm.hca0 - crm.hca1-crm.NEWEID;

                                li.Add(crm);
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
        #endregion

        #region 登入統計
        public Map GetLoginReport(StatisticsLoginModel model)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).ToString("yyyy/MM/dd");
            Map map = new Map();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = "select sum(member), sum(moica), sum(moeaca), sum(hca0), sum(hca1),sum(NEWEID) from LOGIN_STATISTICS where LOGIN_DATE >= @Sdate and LOGIN_DATE <= @Fdate";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", Convert.ToDateTime(model.Fdate).AddDays(1));
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            if (sda.Read())
                            {
                                map.Add("member", DataUtils.GetDBInt(sda, 0));
                                map.Add("moica", DataUtils.GetDBInt(sda, 1));
                                map.Add("moeaca", DataUtils.GetDBInt(sda, 2));
                                map.Add("hca0", DataUtils.GetDBInt(sda, 3));
                                map.Add("hca1", DataUtils.GetDBInt(sda, 4));
                                map.Add("NEWEID", DataUtils.GetDBInt(sda, 5));
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
        #endregion

        #region 熱門申辦統計
        public List<Map> GetHotReport(StatisticsHotModel model)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).AddDays(1).ToString("yyyy/MM/dd");
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = LongWithSql.GetHotSql();
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", Convert.ToDateTime(model.Fdate).AddDays(1));
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("SCNAME", DataUtils.GetDBString(sda, 0));
                                map.Add("SRV_ID", DataUtils.GetDBString(sda, 1));
                                map.Add("NAME", DataUtils.GetDBString(sda, 2));
                                map.Add("TIMES", DataUtils.GetDBInt(sda, 3));
                                map.Add("MEMBER", DataUtils.GetDBInt(sda, 4));
                                map.Add("MOICA", DataUtils.GetDBInt(sda, 5));
                                map.Add("MOEACA", DataUtils.GetDBInt(sda, 6));
                                map.Add("HCA0", DataUtils.GetDBInt(sda, 7));
                                map.Add("HCA1", DataUtils.GetDBInt(sda, 8));
                                map.Add("NEWEID", DataUtils.GetDBInt(sda, 9));
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
        #endregion

        #region 書表下載統計
        public List<Map> GetFileReport(StatisticsFileModel model)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).ToString("yyyy/MM/dd");
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = LongWithSql.GetFileSql();
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", Convert.ToDateTime(model.Fdate).AddDays(1));
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {

                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("SC_ID", DataUtils.GetDBInt(sda, 0));
                                map.Add("SNAME", DataUtils.GetDBString(sda, 1));
                                map.Add("UNAME", DataUtils.GetDBString(sda, 2));
                                map.Add("COUNTER", DataUtils.GetDBInt(sda, 3));
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

        public List<Map> GetFileDetailReport(StatisticsFileModel model, String sc_id)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).ToString("yyyy/MM/dd");
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = LongWithSql.GetFileDetailSql();
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "SC_ID", sc_id);
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", Convert.ToDateTime(model.Fdate).AddDays(1));
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {

                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("SRV_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("TITLE", DataUtils.GetDBString(sda, 2));
                                map.Add("total_count", DataUtils.GetDBInt(sda, 3));
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
        #endregion

        #region 繳費方式統計
        public List<Map> GetPaytypeReport(StatisticsPaytypeModel model)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).ToString("yyyy/MM/dd");
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = LongWithSql.GetPaytypeSql();
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", Convert.ToDateTime(model.Fdate).AddDays(1));
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("SC_ID", DataUtils.GetDBInt(sda, 0));
                                map.Add("NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("ATM", DataUtils.GetDBInt(sda, 2));
                                map.Add("CREDITCARD", DataUtils.GetDBInt(sda, 3));
                                map.Add("DRAFT", DataUtils.GetDBInt(sda, 4));
                                map.Add("TRANSFER", DataUtils.GetDBInt(sda, 5));
                                map.Add("COUNTER", DataUtils.GetDBInt(sda, 6));
                                map.Add("STORE", DataUtils.GetDBInt(sda, 7));
                                map.Add("TATOL", DataUtils.GetDBInt(sda, 8));
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

        public List<Map> GetPaytypeDetailReport(StatisticsPaytypeModel model, String sc_id)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).ToString("yyyy/MM/dd");
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;

                        dbc.CommandText = LongWithSql.GetPaytypeDetailSql();
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "SC_ID", sc_id);
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", Convert.ToDateTime(model.Fdate).AddDays(1));
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {

                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("SC_ID", DataUtils.GetDBInt(sda, 0));
                                map.Add("NAME", DataUtils.GetDBString(sda, 1));
                                map.Add("ATM", DataUtils.GetDBInt(sda, 2));
                                map.Add("CREDITCARD", DataUtils.GetDBInt(sda, 3));
                                map.Add("DRAFT", DataUtils.GetDBInt(sda, 4));
                                map.Add("TRANSFER", DataUtils.GetDBInt(sda, 5));
                                map.Add("COUNTER", DataUtils.GetDBInt(sda, 6));
                                map.Add("STORE", DataUtils.GetDBInt(sda, 7));
                                map.Add("TATOL", DataUtils.GetDBInt(sda, 8));
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
        #endregion

        #region 滿意度報表
        public DataTable CreateExcel(string tableName, string strTime, string endTime)
        {
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand dbc = conn.CreateCommand();
            SqlDataAdapter sda = new SqlDataAdapter(dbc);
            DataSet dataSet = new DataSet();
            DataTable resultTable = new DataTable();
            try
            {
                dbc.CommandText = "select * from " + tableName + " where substring(SRL_NO,1,5) between @strTime and @endTime and DEL_MK = 'N' order by SRL_NO";
                DataUtils.AddParameters(dbc, "strTime", strTime);
                DataUtils.AddParameters(dbc, "endTime", endTime);

                sda.Fill(dataSet, "report");

                //new 一個新的table來render to excel

                resultTable.Columns.Add("問卷編號");
                resultTable.Columns.Add("機關別");
                resultTable.Columns.Add("案件名稱");
                resultTable.Columns.Add("案件編號");
                resultTable.Columns.Add("服務親切度");
                resultTable.Columns.Add("業務熟悉度");
                resultTable.Columns.Add("時間合理度");
                resultTable.Columns.Add("流程合宜度");
                resultTable.Columns.Add("補正告知度");
                resultTable.Columns.Add("滿意程度");
                resultTable.Columns.Add("具體建議");

                int row_index = 0;
                //將select結果塞進新table
                foreach (DataRow row in dataSet.Tables["report"].Rows)
                {
                    resultTable.Rows.Add();
                    resultTable.Rows[row_index]["問卷編號"] = dataSet.Tables["report"].Rows[row_index]["SRL_NO"];
                    resultTable.Rows[row_index]["機關別"] = dataSet.Tables["report"].Rows[row_index]["UNIT_NAME"];
                    resultTable.Rows[row_index]["案件名稱"] = dataSet.Tables["report"].Rows[row_index]["SRV_NAME"];
                    resultTable.Rows[row_index]["案件編號"] = dataSet.Tables["report"].Rows[row_index]["APPLY_ID"];
                    resultTable.Rows[row_index]["服務親切度"] = dataSet.Tables["report"].Rows[row_index]["Q1"];
                    resultTable.Rows[row_index]["業務熟悉度"] = dataSet.Tables["report"].Rows[row_index]["Q2"];
                    resultTable.Rows[row_index]["時間合理度"] = dataSet.Tables["report"].Rows[row_index]["Q3"];
                    resultTable.Rows[row_index]["流程合宜度"] = dataSet.Tables["report"].Rows[row_index]["Q4"];
                    resultTable.Rows[row_index]["補正告知度"] = dataSet.Tables["report"].Rows[row_index]["Q5"];
                    resultTable.Rows[row_index]["滿意程度"] = dataSet.Tables["report"].Rows[row_index]["SATISFIED"];
                    resultTable.Rows[row_index]["具體建議"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["RECOMMEND"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["RECOMMEND"]);
                    ++row_index;
                }
            }
            finally
            {
                sda.Dispose();
                dbc.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return resultTable;
        }

        public DataTable CreateExcel2(string tableName, string strTime, string endTime)
        {
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand dbc = conn.CreateCommand();
            SqlDataAdapter sda = new SqlDataAdapter(dbc);
            DataSet dataSet = new DataSet();
            //new 一個新的table來render to excel
            DataTable resultTable2 = new DataTable();
            try
            {
                dbc.CommandText = "select * from " + tableName + " where substring(SRL_NO,1,5) between @strTime and @endTime and DEL_MK = 'N' order by SRL_NO";
                DataUtils.AddParameters(dbc, "strTime", strTime);
                DataUtils.AddParameters(dbc, "endTime", endTime);

                sda.Fill(dataSet, "report");

                DataTable resultTable = new DataTable();
                resultTable.Columns.Add("問卷編號");
                resultTable.Columns.Add("機關別");
                resultTable.Columns.Add("滿意程度");
                //resultTable.Columns.Add("類別");

                int row_index = 0;

                //將select結果塞進新table
                foreach (DataRow row in dataSet.Tables["report"].Rows)
                {
                    resultTable.Rows.Add();
                    //resultTable.Rows[row_index]["類別"] = "類別";
                    resultTable.Rows[row_index]["問卷編號"] = dataSet.Tables["report"].Rows[row_index]["SRL_NO"];
                    resultTable.Rows[row_index]["機關別"] = dataSet.Tables["report"].Rows[row_index]["UNIT_NAME"];
                    resultTable.Rows[row_index]["滿意程度"] = dataSet.Tables["report"].Rows[row_index]["SATISFIED"];
                    ++row_index;
                }



                //resultTable2.Columns.Add("級別");
                resultTable2.Columns.Add("機關別");

                resultTable2.Columns.Add("回收合計");


                resultTable2.Columns.Add("不合計");
                resultTable2.Columns.Add("不百分比");


                resultTable2.Columns.Add("滿合計");
                resultTable2.Columns.Add("滿百分比");
                DataTable tempTable = new DataTable();

                //第一行做法較特殊
                #region

                row_index = 0;
                resultTable2.Rows.Add();

                // =================全部==================
                double total = 0;

                total = resultTable.Rows.Count;

                resultTable2.Rows[row_index]["機關別"] = "總計";

                resultTable2.Rows[row_index]["回收合計"] = string.Format("{0:0}", total);


                // =================不滿意==================

                var tempResult = from myrow in resultTable.AsEnumerable()
                                 where (myrow.Field<string>("滿意程度") == "不滿意" || myrow.Field<string>("滿意程度") == "非常不滿意")
                                 select myrow;

                resultTable2.Rows[row_index]["不合計"] = string.Format("{0:0}", tempResult.AsDataView().Count);
                resultTable2.Rows[row_index]["不百分比"] = string.Format("{0:0.0%}", (tempResult.AsDataView().Count) / total);

                // =================滿意==================

                tempResult = from myrow in resultTable.AsEnumerable()
                             where (myrow.Field<string>("滿意程度") == "滿意" || myrow.Field<string>("滿意程度") == "非常滿意")
                             select myrow;

                resultTable2.Rows[row_index]["滿合計"] = string.Format("{0:0}", (tempResult.AsDataView().Count));
                resultTable2.Rows[row_index]["滿百分比"] = string.Format("{0:0.0%}", (tempResult.AsDataView().Count) / total);

                #endregion



                //第二行後前置作業

                DataView dv = new DataView();

                dv = resultTable.DefaultView;
                tempTable = dv.ToTable("result", true, "機關別");
                List<string> facultyList = new List<string>(); //所有案件出現的機關
                for (int i = 0; i < tempTable.Rows.Count; i++)
                {
                    facultyList.Add(tempTable.Rows[i]["機關別"].ToString());
                }
                tempTable.Clear();

                foreach (string facu in facultyList)
                {

                    tempResult = from myrow in resultTable.AsEnumerable()
                                 where myrow.Field<string>("機關別") == facu
                                 select myrow;

                    total = tempResult.AsDataView().Count;


                    if (total > 0)
                    {
                        ++row_index;
                        resultTable2.Rows.Add();

                        resultTable2.Rows[row_index]["機關別"] = facu;

                        // =================全部==================

                        resultTable2.Rows[row_index]["回收合計"] = string.Format("{0:0}", total);


                        // =================不滿意==================

                        tempResult = from myrow in resultTable.AsEnumerable()
                                     where (myrow.Field<string>("滿意程度") == "不滿意" || myrow.Field<string>("滿意程度") == "非常不滿意") && myrow.Field<string>("機關別") == facu
                                     select myrow;

                        resultTable2.Rows[row_index]["不合計"] = string.Format("{0:0}", (tempResult.AsDataView().Count));
                        resultTable2.Rows[row_index]["不百分比"] = string.Format("{0:0.0%}", (tempResult.AsDataView().Count) / total);


                        // =================滿意==================

                        tempResult = from myrow in resultTable.AsEnumerable()
                                     where (myrow.Field<string>("滿意程度") == "滿意" || myrow.Field<string>("滿意程度") == "非常滿意") && myrow.Field<string>("機關別") == facu
                                     select myrow;

                        resultTable2.Rows[row_index]["滿合計"] = string.Format("{0:0}", (tempResult.AsDataView().Count));
                        resultTable2.Rows[row_index]["滿百分比"] = string.Format("{0:0.0%}", (tempResult.AsDataView().Count) / total);
                    }
                }
            }
            finally
            {
                sda.Dispose();
                dbc.Dispose();
                conn.Close();
                conn.Dispose();
            }
            return resultTable2;
        }
        #endregion

        #region 專科社會工作師
        public DataTable GetSocialWork1(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        dbc.Transaction = st;

                        dbc.CommandText = @"
	 SELECT CASE WHEN APPLY_TYPE='1' then '核發' WHEN APPLY_TYPE = '2' THEN '補發(遺失、汙損等)' ELSE '換發(更名)' END APPLY_TYPEN, 
	 CASE WHEN SPECIALIST_TYPE = '1' then '醫務專科' 
	 WHEN SPECIALIST_TYPE = '2' THEN '心理衛生專科'
	 WHEN SPECIALIST_TYPE = '3' THEN '兒童、少年、婦女及家庭專科' 
	 WHEN SPECIALIST_TYPE = '4' THEN '老人專科' ELSE '身心障礙專科' END SPECIALIST_TYPEN, 
	 NAME, CONVERT(VARCHAR(10), BIRTHDAY, 111) BIRTHDAY, IDN, 
	 CASE WHEN SEX_CD = 'M' then '男' WHEN SEX_CD = 'F' THEN '女' ELSE '其他' END SEX_CDN,
	 '電話(公)：'+ISNULL(W_TEL,'')+'<br>'+'電話(宅)：'+ISNULL(H_TEL,'')+'<br>手機:'+ISNULL(TELM,'') TEL,
	 '(' + C_ZIPCODE + ') ' + CZIPN + C_ADDR C_ADDRN ,
	 '(' + H_ZIPCODE + ') ' + HZIPN + H_ADDR H_ADDRN,PRACTICE_PLACE 
	 FROM (
    SELECT APPLY_011002.*, APPLY.NAME, APPLY.BIRTHDAY, APPLY.IDN, APPLY.SEX_CD,
CZIP.CITYNM + CZIP.TOWNNM CZIPN, HZIP.CITYNM + HZIP.TOWNNM HZIPN, APPLY.APP_TIME, APPLY.MOBILE AS TELM FROM APPLY_011002 LEFT JOIN APPLY ON APPLY.APP_ID = APPLY_011002.APP_ID LEFT JOIN ZIPCODE CZIP ON CZIP.ZIP_CO = APPLY_011002.C_ZIPCODE LEFT JOIN ZIPCODE HZIP ON HZIP.ZIP_CO = APPLY_011002.H_ZIPCODE
    UNION
    SELECT APPLY_011005.*, APPLY.NAME, APPLY.BIRTHDAY, APPLY.IDN, APPLY.SEX_CD, CZIP.CITYNM + CZIP.TOWNNM CZIPN, HZIP.CITYNM + HZIP.TOWNNM HZIPN, APPLY.APP_TIME, APPLY.MOBILE AS TELM FROM APPLY_011005 LEFT JOIN APPLY ON APPLY.APP_ID = APPLY_011005.APP_ID LEFT JOIN ZIPCODE CZIP ON CZIP.ZIP_CO = APPLY_011005.C_ZIPCODE LEFT JOIN ZIPCODE HZIP ON HZIP.ZIP_CO = APPLY_011005.H_ZIPCODE
    UNION
    SELECT APPLY_011006.*, APPLY.NAME, APPLY.BIRTHDAY, APPLY.IDN, APPLY.SEX_CD, CZIP.CITYNM + CZIP.TOWNNM CZIPN, HZIP.CITYNM + HZIP.TOWNNM HZIPN, APPLY.APP_TIME, APPLY.MOBILE AS TELM FROM APPLY_011006 LEFT JOIN APPLY ON APPLY.APP_ID = APPLY_011006.APP_ID LEFT JOIN ZIPCODE CZIP ON CZIP.ZIP_CO = APPLY_011006.C_ZIPCODE LEFT JOIN ZIPCODE HZIP ON HZIP.ZIP_CO = APPLY_011006.H_ZIPCODE
    ) A WHERE Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        dt.Columns[0].ColumnName = "申請用途";
                        dt.Columns[1].ColumnName = "專科類別";
                        dt.Columns[2].ColumnName = "姓名";
                        dt.Columns[3].ColumnName = "出生年月日";
                        dt.Columns[4].ColumnName = "國民身分證統一編號";
                        dt.Columns[5].ColumnName = "性別";
                        dt.Columns[6].ColumnName = "電話號碼";
                        dt.Columns[7].ColumnName = "通訊地址(含郵遞區號)";
                        dt.Columns[8].ColumnName = "戶籍地址(含郵遞區號)";
                        dt.Columns[9].ColumnName = "執業處所";
                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                dr[i] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        dtTemp.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }

        public DataTable GetSocialWork2(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        dbc.Transaction = st;
                        dbc.CommandText = @" WITH A AS (
    SELECT APPLY_011002.*,APPLY.APP_TIME FROM APPLY_011002 LEFT JOIN APPLY ON APPLY.APP_ID=APPLY_011002.APP_ID
    WHERE Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate
    UNION 
    SELECT APPLY_011005.*,APPLY.APP_TIME FROM APPLY_011005 LEFT JOIN APPLY ON APPLY.APP_ID=APPLY_011005.APP_ID
    WHERE Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate
    UNION 
    SELECT APPLY_011006.*,APPLY.APP_TIME FROM APPLY_011006 LEFT JOIN APPLY ON APPLY.APP_ID=APPLY_011006.APP_ID
    WHERE Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate
)
    SELECT 1 as ID, '受理件數'+'<br>'+'(總計)' TITLE,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='1') SPEICIALIST_N1,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='2') SPEICIALIST_N2,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='3') SPEICIALIST_N3,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='4') SPEICIALIST_N4,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='5') SPEICIALIST_N5
    UNION
    SELECT 2 as ID, '申請核發'+'<br>'+'件數' TITLE,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='1' AND APPLY_TYPE='1') SPEICIALIST_N1,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='2' AND APPLY_TYPE='1') SPEICIALIST_N2,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='3' AND APPLY_TYPE='1') SPEICIALIST_N3,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='4' AND APPLY_TYPE='1') SPEICIALIST_N4,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='5' AND APPLY_TYPE='1') SPEICIALIST_N5
    UNION
    SELECT 3 as ID, '申請補發'+'<br>'+'件數' TITLE,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='1' AND APPLY_TYPE='2') SPEICIALIST_N1,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='2' AND APPLY_TYPE='2') SPEICIALIST_N2,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='3' AND APPLY_TYPE='2') SPEICIALIST_N3,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='4' AND APPLY_TYPE='2') SPEICIALIST_N4,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='5' AND APPLY_TYPE='2') SPEICIALIST_N5
    UNION
    SELECT 4 as ID, '申請換發'+'<br>'+'件數' TITLE,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='1' AND APPLY_TYPE in ('3','4')) SPEICIALIST_N1,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='2' AND APPLY_TYPE in ('3','4')) SPEICIALIST_N2,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='3' AND APPLY_TYPE in ('3','4')) SPEICIALIST_N3,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='4' AND APPLY_TYPE in ('3','4')) SPEICIALIST_N4,
(SELECT COUNT(*) FROM A WHERE SPECIALIST_TYPE='5' AND APPLY_TYPE in ('3','4')) SPEICIALIST_N5
";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        var alColumns = dt.Columns.Count;
                        dt.Columns.Remove("ID");
                        dt.Columns[0].ColumnName = "    ";
                        dt.Columns[1].ColumnName = "醫務專科";
                        dt.Columns[2].ColumnName = "心理衛生<br>專科";
                        dt.Columns[3].ColumnName = "兒童、少<br>年婦女及<br>家庭專科";
                        dt.Columns[4].ColumnName = "老人專科 ";
                        dt.Columns[5].ColumnName = "身心障礙<br>專科";
                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 1; i < alColumns; i++)
                            {
                                dr[i - 1] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        dtTemp.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }
        #endregion 專科社會工作師

        #region 社會工作實務經驗年資審查匯出
        public DataTable GetSocialWork3(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        var dtTemp1 = new DataTable();
                        dbc.Transaction = st;

                        dbc.CommandText = @"
           SELECT APPLY_011003.APP_ID,
                /*APPLY_011003.ISMEET,*/ 
            CASE APPLY_011003.ISMEET WHEN 1 then '符合第一款(國內社會工作實務經驗五年以上，有中央主管機關審查合格之證明文件。)'
                WHEN 2 then '符合第二款(外國社會工作師證書及國內社會工作實務經驗一年以上，有中央主管機關審查合格之證明文件。)'
                when 3 then '符合第三款(曾任公立或依法立案之私立專科以上學校講師三年以上、助理教授或副教授二年以上、教授一年以上，<br>   講授前條第一項所列學科至少二科，並有國內社會工作實務經驗一年以上，有中央主管機關審查合格之證明文件。)'
                else ''
            end ISMEET_TEXT,
            APPLY.NAME,
            ISNULL(CONVERT(VARCHAR(3),CONVERT(VARCHAR(4),APPLY.BIRTHDAY,20) - 1911) + '/' +SUBSTRING(CONVERT(VARCHAR(10),APPLY.BIRTHDAY,20),6,2) + '/' +SUBSTRING(CONVERT(VARCHAR(10),APPLY.BIRTHDAY,20),9,2),'') AS BIRTHDAY, APPLY.IDN, 
            CASE APPLY.SEX_CD WHEN 'M' then '男' WHEN 'F' THEN '女' ELSE '其他' END SEX_CDN,
            '電話(公)：'+ISNULL(APPLY.TEL,'')+'<br>電話(宅)'+ISNULL(APPLY_011003.FAX,'')+'<br>行動電話'+ISNULL(APPLY_011003.MOBILE,''),/*, '('+APPLY.ADDR_CODE +') ' +APPLY.ADDR AS ADDR,*/ APPLY.ADDR_CODE,APPLY.ADDR,
            /*ISNULL(APPLY_011003.SCHOOL,'')+'<br>'+ISNULL(APPLY_011003.OFFICE,'')+'<br>'+ISNULL(APPLY_011003.GRADUATION,'') SCHOOL_EDU,*/
            ISNULL(APPLY_011003.SCHOOL,'')+'<br>'+ISNULL(APPLY_011003.OFFICE,'')  SCHOOL_EDU,APPLY_011003.GRADUATION,
            /*APPLY_011003_SRVLST.SRV_NAM,APPLY_011003_SRVLST.SRV_TITLE,APPLY_011003_SRVLST.SRV_SYEAR,APPLY_011003_SRVLST.SRV_EYEAR,*/
            /*(select D.SRV_NAM+','+D.SRV_TITLE+','+CONVERT(VARCHAR(10),D.SRV_SYEAR, 111)+','+CONVERT(VARCHAR(10),D.SRV_EYEAR, 111) + ';' from APPLY_011003_SRVLST as D where  D.APP_ID=APPLY_011003.APP_ID FOR XML PATH('')) as SRV_LIST*/
            (select ISNULL(D.SRV_NAM,'') + ';' from APPLY_011003_SRVLST as D where  D.APP_ID=APPLY_011003.APP_ID ORDER BY APP_ID,SEQ_NO FOR XML PATH('')) as SRV_NAM_LIST,
			(select ISNULL(D.SRV_TITLE,'')+ ';' from APPLY_011003_SRVLST as D where  D.APP_ID=APPLY_011003.APP_ID ORDER BY APP_ID,SEQ_NO  FOR XML PATH('')) as SRV_TITLE_LIST,
			(select ISNULL(CONVERT(VARCHAR(3),CONVERT(VARCHAR(4),D.SRV_SYEAR,20) - 1911) + '/' +SUBSTRING(CONVERT(VARCHAR(10),D.SRV_SYEAR,20),6,2) + '/' +SUBSTRING(CONVERT(VARCHAR(10),D.SRV_SYEAR,20),9,2),'') + ';' from APPLY_011003_SRVLST as D where  D.APP_ID=APPLY_011003.APP_ID  ORDER BY APP_ID,SEQ_NO  FOR XML PATH('')) as SRV_SYEAR_LIST,
			(select ISNULL(CONVERT(VARCHAR(3),CONVERT(VARCHAR(4),D.SRV_EYEAR,20) - 1911) + '/' +SUBSTRING(CONVERT(VARCHAR(10),D.SRV_EYEAR,20),6,2) + '/' +SUBSTRING(CONVERT(VARCHAR(10),D.SRV_EYEAR,20),9,2),'') + ';' from APPLY_011003_SRVLST as D where  D.APP_ID=APPLY_011003.APP_ID  ORDER BY APP_ID,SEQ_NO  FOR XML PATH('')) as SRV_EYEAR_LIST
            
            FROM APPLY_011003
            LEFT JOIN APPLY ON  APPLY_011003.APP_ID=APPLY.APP_ID
            /*LEFT JOIN APPLY_011003_SRVLST ON APPLY_011003.APP_ID=APPLY_011003_SRVLST.APP_ID*/
            WHERE Convert(date,APPLY.APP_TIME) BETWEEN @Sdate AND @Fdate";

                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        //dt.Columns.Remove("APP_ID");
                        dt.Columns[0].ColumnName = "案件編號";
                        dt.Columns[1].ColumnName = "申請種類";
                        dt.Columns[2].ColumnName = "姓名";
                        dt.Columns[3].ColumnName = "出生年月日";
                        dt.Columns[4].ColumnName = "國民身分證統一編號";
                        dt.Columns[5].ColumnName = "性別";
                        dt.Columns[6].ColumnName = "電話號碼     ";
                        dt.Columns[7].ColumnName = "郵遞區號";
                        dt.Columns[8].ColumnName = "通訊地址        ";
                        dt.Columns[9].ColumnName = "學歷     ";
                        dt.Columns[10].ColumnName = "畢業年月";
                        dt.Columns[11].ColumnName = "服務單位     ";
                        dt.Columns[12].ColumnName = "職稱      ";
                        dt.Columns[13].ColumnName = "服務年資起";
                        dt.Columns[14].ColumnName = "服務年資迄";
                        #region 橫向動態用
                        //dbc.CommandText = @"select A.APP_ID,A.SRV_NAM,A.SRV_TITLE,CONVERT(VARCHAR(10),A.SRV_SYEAR, 111) SRV_SYEAR,CONVERT(VARCHAR(10),A.SRV_EYEAR, 111) SRV_EYEAR,SEQ_NO  FROM APPLY_011003_SRVLST A LEFT JOIN APPLY on apply.APP_ID=A.APP_ID and APPLY.SRV_ID='011003'  WHERE Convert(date,APPLY.APP_TIME) BETWEEN @Sdate AND @Fdate order by A.APP_ID";
                        //SqlDataReader sda1 = dbc.ExecuteReader();
                        //dtTemp1.Load(sda1);
                        ////取得資料中最多服務經歷的筆數
                        //var max4Time = Convert.ToInt32(dtTemp1.Compute("Max([SEQ_NO])", string.Empty));
                        ////動態產生EXCEL報表欄位標題LIST
                        //var list = new List<string>() { "服務單位{0}     ", "職稱{0}      ", "服務年資{0}起", "服務年資{0}迄" };
                        ////動態生成LIST清單中欄位標題 max4Time * list元素數量
                        //for (var i = 0; i < max4Time; i++)
                        //{
                        //    for (var j = 0; j < list.Count(); j++)
                        //    {
                        //        if (dt.Columns.Count > 9 + i * 4 + j+1)
                        //        {
                        //            dt.Columns[9 + i * 4 + j+1].ColumnName = string.Format(list[j],(i + 1).ToString());
                        //        }
                        //        else
                        //        {
                        //            dt.Columns.Add(new DataColumn(string.Format(list[j], (i + 1).ToString()), typeof(string)));
                        //        }
                        //    }
                        //}

                        //foreach (DataRow drTemp in dtTemp.Rows)
                        //{
                        //    var dr = dt.NewRow();
                        //    for (var i = 0; i < dt.Columns.Count; i++)
                        //    {
                        //        //前9筆不是動態產生
                        //        if (i < 10)
                        //        {
                        //            if (drTemp[i].ToString().Contains(';'))
                        //            {
                        //                dr[i] = drTemp[i].ToString().Substring(0, drTemp[i].ToString().Length - 1).Replace(";", "<br>");
                        //            }
                        //            else
                        //                dr[i] = drTemp[i];
                        //        }
                        //        else
                        //        {
                        //            //取出要列印的資料內容
                        //            var dtSelect = dtTemp1.Select("APP_ID='" + drTemp[14] + "' and SEQ_NO='" + ((i - 10) / 4 + 1).ToString() + "'");
                        //            //輸出經歷資料
                        //            if (dtSelect.Length > 0)
                        //            {
                        //                switch ((i - 10) % 4)
                        //                {
                        //                    case 0:
                        //                        dr[i] = dtSelect[0][1];
                        //                        break;
                        //                    case 1:
                        //                        dr[i] = dtSelect[0][2];
                        //                        break;
                        //                    case 2:
                        //                        dr[i] = dtSelect[0][3];
                        //                        break;
                        //                    case 3:
                        //                        dr[i] = dtSelect[0][4];
                        //                        break;
                        //                    default: break;
                        //                }
                        //            }
                        //        }
                        //    }
                        //    dt.Rows.Add(dr);
                        //}
                        //dtTemp1.Clear();
                        #endregion 橫向動態用
                        #region 工作經歷直向顯示用
                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                if (drTemp[i].ToString().Contains(';'))
                                {
                                    dr[i] = drTemp[i].ToString().Substring(0, drTemp[i].ToString().Length - 1).Replace(";", "<br>");
                                }
                                else
                                    dr[i] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        #endregion 工作經歷直向顯示用
                        dtTemp.Clear();

                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }
        #endregion 社會工作實務經驗年資審查匯出

        #region 社會工作師
        public DataTable GetSocialWork4(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        dbc.Transaction = st;
                        dbc.CommandText = @"
	SELECT /* CONVERT(VARCHAR(10),SYSDATETIME(), 111) EXPORTDATE,*/ ROW_NUMBER()  OVER (Order by SRV_ID,APPLY_TYPE,APP_ID) RNUM, CASE APPLY_TYPE WHEN '7' then '核發(中文)' WHEN '81' THEN '更名' WHEN '82' THEN '汙損' WHEN '9' THEN '遺失'  ELSE '核發(英文)' END APPLY_TYPEN,/*APPLY_TYPE,*/ 
	    NAME, CASE WHEN SEX_CD = 'M' then '男' WHEN SEX_CD = 'F' THEN '女' ELSE '其他' END SEX_CDN,
        CONVERT(VARCHAR(10), BIRTHDAY, 111) BIRTHDAY,
        /*民國年月日(YYY/MM/DD)*/
        /*CONVERT(VARCHAR(3),CONVERT(INT,SUBSTRING(CONVERT(VARCHAR(10), BIRTHDAY, 111),1,4))-1911)+SUBSTRING(CONVERT(VARCHAR(10), BIRTHDAY, 111),5,6)  BIRTHDAY_TW,*/    
	    IDN,TEST_YEAR,
	    /*'(' + C_ZIPCODE + ') ' + C_ADDR C_ADDRN,*/
        C_ZIPCODE,C_ADDRN,
        /*011004 沒有公司地址先註解*/
	    /*'(' + H_ZIPCODE + ') ' + H_ADDR H_ADDRN,*/
	    '電話(公)：'+ISNULL(W_TEL,'')+'<br>'+'電話(宅)：'+ISNULL(H_TEL,'')+'<br>手機:'+ISNULL(MOBILE,'') TEL
	FROM (
            SELECT DISTINCT A.*,CZIP.CITYNM+CZIP.TOWNNM+C_ADDR C_ADDRN ,HZIP.CITYNM+HZIP.TOWNNM+H_ADDR H_ADDRN
	        FROM (
	                SELECT APPLY.APP_ID,APPLY.SRV_ID,APPLY.APP_TIME,'7' APPLY_TYPE,''CHG_NAME , APPLY.NAME, APPLY.BIRTHDAY, APPLY.IDN, APPLY.SEX_CD,APPLY_DATE,EMAIL,W_TEL,H_TEL,APPLY.MOBILE,C_ZIPCODE,C_ADDR,
	                    H_ZIPCODE,H_ADDR,TEST_YEAR,TEST_CATEGORY,APPLY.FLOW_CD  
	                    FROM APPLY_011007
	                    JOIN APPLY ON APPLY.APP_ID = APPLY_011007.APP_ID  AND APPLY.SRV_ID='011007'
	                UNION
	                SELECT APPLY.APP_ID,APPLY.SRV_ID,APPLY.APP_TIME ,'8'+APPLY_TYPE APPLY_TYPE,CHG_NAME, APPLY.NAME, APPLY.BIRTHDAY, APPLY.IDN, APPLY.SEX_CD ,APPLY_DATE,EMAIL,W_TEL,H_TEL,APPLY.MOBILE,C_ZIPCODE,C_ADDR,
	                    H_ZIPCODE,H_ADDR,TEST_YEAR,TEST_CATEGORY,APPLY.FLOW_CD  
	                    FROM APPLY_011008
	                    JOIN APPLY ON APPLY.APP_ID = APPLY_011008.APP_ID AND APPLY.SRV_ID='011008'
	                UNION
	                SELECT APPLY.APP_ID,APPLY.SRV_ID,APPLY.APP_TIME,'9' APPLY_TYPE,'' CHG_NAME, APPLY.NAME, APPLY.BIRTHDAY, APPLY.IDN, APPLY.SEX_CD, APPLY_DATE,EMAIL,W_TEL,H_TEL,APPLY.MOBILE,C_ZIPCODE,
	                    C_ADDR,H_ZIPCODE,H_ADDR,TEST_YEAR,TEST_CATEGORY,APPLY.FLOW_CD  
	                    FROM APPLY_011009
	                    JOIN APPLY ON APPLY.APP_ID = APPLY_011009.APP_ID  AND APPLY.SRV_ID='011009'
	                UNION
	                SELECT APPLY.APP_ID,APPLY.SRV_ID,APPLY.APP_TIME,'4' APPLY_TYPE,'' CHG_NAME , APPLY.NAME, APPLY.BIRTHDAY, APPLY.IDN, APPLY.SEX_CD, 
	                    (SELECT APP_TIME FROM APPLY WHERE APPLY.APP_ID=APPLY_011004.APP_ID) APPLY_DATE,EMAIL,C_TEL,H_TEL,APPLY.MOBILE,C_ZIP C_ZIPCODE,C_ADDR,
	                    '' H_ZIP, H_ADDR,YEAR,TYPE AS TEST_CATEGORY,APPLY.FLOW_CD  
	                    FROM APPLY_011004
	                    JOIN APPLY ON APPLY.APP_ID = APPLY_011004.APP_ID  AND APPLY.SRV_ID='011004'
            ) A 
            LEFT JOIN ZIPCODE CZIP ON CZIP.ZIP_CO = A.C_ZIPCODE 
		    LEFT JOIN ZIPCODE HZIP ON HZIP.ZIP_CO = A.H_ZIPCODE 
            WHERE Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate 
        ) B
	ORDER BY SRV_ID,APPLY_TYPE,APP_ID";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        dt.Columns[0].ColumnName = "編號";
                        dt.Columns[1].ColumnName = "申請類別";
                        dt.Columns[2].ColumnName = "姓名";
                        dt.Columns[3].ColumnName = "性別";
                        dt.Columns[4].ColumnName = "出生年月日";
                        dt.Columns[5].ColumnName = "國民身分證統一編號";
                        dt.Columns[6].ColumnName = "考試及格";
                        dt.Columns[7].ColumnName = "郵遞區號";
                        dt.Columns[8].ColumnName = "聯絡地址       ";
                        dt.Columns[9].ColumnName = "電話號碼       ";
                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                dr[i] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        dtTemp.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }
        #endregion 社會工作師

        #region 全國社會工作專業人員選拔推薦清冊
        public DataTable GetSocialWork5(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        dbc.Transaction = st;
                        dbc.CommandText = @"
	SELECT 
    ROW_NUMBER() OVER (ORDER BY app.APP_TIME) AS RowIndex,
	ser.NAME,
	CONVERT(varchar, app.APP_TIME, 111) AS APPLY_DATE,
    app.APP_ID,
	case A2.UNIT_TYPE 
when '1' then '1.公部門〔直轄市、縣（市）政府（含社會局處、勞工局處、教育局處及衛生局等相關單位）及所屬之社會福利機構、本部等其他中央部會及其所屬社會福利機構、公立大專校院等〕' 
when '2' then '2.私部門〔各私立社會福利機構、長期照顧服務機構、團體、大專校院〕' 
when '3' then '3.公私立醫事機構' else '' end as UNIT_TYPE,
	case A2.UNIT_SUBTYPE when '1' then '身心障礙福利機構/團體' when '2' then '老人及長期照顧福利機構/團體' when '3' then '兒少、婦女及家庭福利機構/團體' when '4' then '學校' when '5' then '矯正機關' when '6' then '社工師事務所及公會/協會/學會' when '7' then '其他（非屬上述類別）' else '' end as UNIT_SUBTYPE,
	A2.UNIT_NAME,
	A2.UNIT_DEPART,
	A2.UNIT_TITLE,
	A2.UNIT_CNAME,
	A2.UNIT_TEL,
	A2.UNIT_EMAIL,
	convert(varchar,isnull(A2.CNT_D,'0')) CNT_D,
    convert(varchar,isnull(A2.CNT_E,'0')) CNT_E,
    convert(varchar,isnull(A2.CNT_F,'0')) CNT_F,
    convert(varchar,isnull(A2.CNT_G,'0')) CNT_G,
    convert(varchar,isnull(A2.CNT_H,'0')) CNT_H
FROM 
    APPLY app
LEFT JOIN ADMIN ad ON ad.ACC_NO = app.PRO_ACC
LEFT JOIN APPLY_011010 A2 ON A2.APP_ID = app.APP_ID
LEFT JOIN SERVICE ser on ser.SRV_ID=app.SRV_ID
WHERE 1 = 1
and app.srv_id='011010'
and Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate 
";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        dt.Columns[0].ColumnName = "編號";
                        dt.Columns[1].ColumnName = "申辦項目";
                        dt.Columns[2].ColumnName = "申辦日期";
                        dt.Columns[3].ColumnName = "案件編號";
                        dt.Columns[4].ColumnName = "單位類型";
                        dt.Columns[5].ColumnName = "單位類型(私部門";
                        dt.Columns[6].ColumnName = "單位名稱";
                        dt.Columns[7].ColumnName = "單位連絡人局處/部門";
                        dt.Columns[8].ColumnName = "單位連絡人職稱";
                        dt.Columns[9].ColumnName = "單位聯絡人姓名";
                        dt.Columns[10].ColumnName = "連絡電話";
                        dt.Columns[11].ColumnName = "E-MAIL";
                        dt.Columns[12].ColumnName = "單位社工人員總數類型";
                        dt.Columns[13].ColumnName = "績優社工獎推薦人數";
                        dt.Columns[14].ColumnName = "績優社工督導獎推薦人數";
                        dt.Columns[15].ColumnName = "資深敬業獎推薦人數";
                        dt.Columns[16].ColumnName = "特殊貢獻獎推薦人數";

                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                dr[i] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        dtTemp.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }
        #endregion 全國社會工作專業人員選拔推薦清冊

        #region 訴願案件申請清冊
        public DataTable GetSocialWork6(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        dbc.Transaction = st;
                        dbc.CommandText = @"
SELECT ROW_NUMBER() OVER (ORDER BY app.APP_TIME) AS RowIndex,ser.NAME as SRVNAME
,CONVERT(varchar, app.APP_TIME, 111) AS APPLY_DATE,app.APP_ID
,app.NAME,convert(varchar,app.BIRTHDAY,111) BIRTHDAY,app.IDN
,convert(varchar,app.ADDR_CODE)+z1.CITYNM+z1.TOWNNM+REPLACE(convert(varchar,app.ADDR),z1.CITYNM+z1.TOWNNM,'') C_ADDR
,app.CNT_TEL,app.MOBILE
,A2.CHR_NAME,convert(varchar,A2.CHR_BIRTH,111) CHR_BIRTH,A2.CHR_IDN
,convert(varchar,A2.CHR_ADDR_CODE)+z2.CITYNM+z2.TOWNNM+REPLACE(convert(varchar,A2.CHR_ADDR),z2.CITYNM+z2.TOWNNM,'') CHR_ADDR
,A2.CHR_TEL,A2.CHR_MOBILE
,A2.R_NAME,convert(varchar,A2.R_BIRTH,111) R_BIRTH,A2.R_IDN
,convert(varchar,A2.R_ADDR_CODE)+z3.CITYNM+z3.TOWNNM+REPLACE(convert(varchar,A2.R_ADDR),z3.CITYNM+z3.TOWNNM,'') R_ADDR
,A2.R_TEL,A2.R_MOBILE
,A2.ORG_NAME,convert(varchar,A2.ORG_DATE,111) ORG_DATE
,A2.ORG_MEMO,A2.ORG_FACT
FROM 
    APPLY app
LEFT JOIN ADMIN ad ON ad.ACC_NO = app.PRO_ACC
LEFT JOIN APPLY_040001 A2 ON A2.APP_ID = app.APP_ID
LEFT JOIN SERVICE ser ON ser.SRV_ID=app.SRV_ID
LEFT JOIN (SELECT DISTINCT ZIP_CO,CITYNM,TOWNNM FROM ZIPCODE) z1 ON z1.ZIP_CO=app.ADDR_CODE
LEFT JOIN (SELECT DISTINCT ZIP_CO,CITYNM,TOWNNM FROM ZIPCODE) z2 on z2.ZIP_CO=A2.CHR_ADDR_CODE
LEFT JOIN (SELECT DISTINCT ZIP_CO,CITYNM,TOWNNM FROM ZIPCODE) z3 on z3.ZIP_CO=A2.R_ADDR_CODE
WHERE 1 = 1
and app.srv_id='040001'
and Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate 
";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        dt.Columns[0].ColumnName = "編號";
                        dt.Columns[1].ColumnName = "申辦項目";
                        dt.Columns[2].ColumnName = "申辦日期";
                        dt.Columns[3].ColumnName = "案件編號";
                        dt.Columns[4].ColumnName = "訴願人姓名";
                        dt.Columns[5].ColumnName = "訴願人出生年月日";
                        dt.Columns[6].ColumnName = "訴願人身分證明文件字號";
                        dt.Columns[7].ColumnName = "訴願人住所或居所";
                        dt.Columns[8].ColumnName = "訴願人聯絡電話";
                        dt.Columns[9].ColumnName = "訴願人聯絡電話2";
                        dt.Columns[10].ColumnName = "代表人姓名";
                        dt.Columns[11].ColumnName = "代表人出生年月日";
                        dt.Columns[12].ColumnName = "代表人身分證明文件字號";
                        dt.Columns[13].ColumnName = "代表人住所或居所";
                        dt.Columns[14].ColumnName = "代表人聯絡電話";
                        dt.Columns[15].ColumnName = "代表人聯絡電話2";
                        dt.Columns[16].ColumnName = "代理人姓名";
                        dt.Columns[17].ColumnName = "代理人出生年月日";
                        dt.Columns[18].ColumnName = "代理人身分證明文件字號";
                        dt.Columns[19].ColumnName = "代理人住所或居所";
                        dt.Columns[20].ColumnName = "代理人聯絡電話";
                        dt.Columns[21].ColumnName = "代理人聯絡電話2";
                        dt.Columns[22].ColumnName = "原行政處分機關";
                        dt.Columns[23].ColumnName = "訴願人收受或知悉行政處分之年月日";
                        dt.Columns[24].ColumnName = "訴願請求(即請求撤銷之行政處分書發文日期、文號或其他)";
                        dt.Columns[25].ColumnName = "事實與理由";

                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                dr[i] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        dtTemp.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }

        #region 訴願案件清冊
        public List<Map> GetReport6(SocialWorker model)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).AddDays(1).ToString("yyyy/MM/dd");
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        dbc.CommandText = @"
                        SELECT app.APP_ID,CONVERT(varchar, app.APP_TIME, 111) AS APP_TIME,app.NAME,A2.ORG_NAME,A2.ORG_MEMO
                        FROM APPLY app
                        LEFT JOIN ADMIN ad ON ad.ACC_NO = app.PRO_ACC
                        LEFT JOIN APPLY_040001 A2 ON A2.APP_ID = app.APP_ID
                        WHERE 1 = 1
                        and app.srv_id='040001'
                        and Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate 
                        ";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("APP_TIME", HelperUtil.DateTimeToTwString(Convert.ToDateTime(DataUtils.GetDBString(sda, 1))));
                                map.Add("NAME", DataUtils.GetDBString(sda, 2));
                                map.Add("ORG_NAME", DataUtils.GetDBString(sda, 3));
                                map.Add("ORG_MEMO", DataUtils.GetDBString(sda, 4));
                                li.Add(map);
                            }
                            sda.Close();
                        }
                        if (li != null)
                        {
                            this.totalCount = li.Count();
                            li = li.Skip((model.queryModel.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }
        #endregion
        #endregion 訴願案件申請清冊

        #region 爭議案件申請清冊
        public DataTable GetSocialWork7(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        dbc.Transaction = st;
                        dbc.CommandText = @"
SELECT ROW_NUMBER() OVER (ORDER BY app.APP_TIME) AS RowIndex,ser.NAME as SRVNAME
,CONVERT(varchar, app.APP_TIME, 111) AS APPLY_DATE,app.APP_ID,app.NAME,app.IDN
,convert(varchar,app.ADDR_CODE)+z1.CITYNM+z1.TOWNNM+REPLACE(convert(varchar,app.ADDR),z1.CITYNM+z1.TOWNNM,'') C_ADDR,app.CNT_TEL,app.MOBILE,A2.R_NAME,A2.R_IDN
,convert(varchar,A2.R_ADDR_CODE)+z3.CITYNM+z3.TOWNNM+REPLACE(convert(varchar,A2.R_ADDR),z3.CITYNM+z3.TOWNNM,'') R_ADDR,A2.R_TEL,A2.R_MOBILE
,Convert(varchar,A2.KNOW_DATE,111) KNOW_DATE
,case A2.KIND1 when 'Y' then '是' else '否' end KIND1
,Convert(varchar,A2.LIC_DATE,111) LIC_DATE,A2.LIC_CD,A2.LIC_NUM
,case A2.KIND2 when 'Y' then '是' else '否' end KIND2
,case A2.KIND3 when 'Y' then '是' else '否' end KIND3
,A2.KNOW_MEMO,A2.KNOW_FACT
FROM 
    APPLY app
LEFT JOIN ADMIN ad ON ad.ACC_NO = app.PRO_ACC
LEFT JOIN APPLY_041001 A2 ON A2.APP_ID = app.APP_ID
LEFT JOIN SERVICE ser ON ser.SRV_ID=app.SRV_ID
LEFT JOIN (SELECT DISTINCT ZIP_CO,CITYNM,TOWNNM FROM ZIPCODE) z1 ON z1.ZIP_CO=app.ADDR_CODE
LEFT JOIN (SELECT DISTINCT ZIP_CO,CITYNM,TOWNNM FROM ZIPCODE) z3 on z3.ZIP_CO=A2.R_ADDR_CODE
WHERE 1 = 1
and app.srv_id='041001'
and Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate 
";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        dt.Columns[0].ColumnName = "編號";
                        dt.Columns[1].ColumnName = "申辦項目";
                        dt.Columns[2].ColumnName = "申辦日期";
                        dt.Columns[3].ColumnName = "案件編號";
                        dt.Columns[4].ColumnName = "申請人姓名/單位名稱";
                        dt.Columns[5].ColumnName = "申請人身分證統一編號(或投保單位統一編號、醫事服務機構代號)";
                        dt.Columns[6].ColumnName = "申請人地址";
                        dt.Columns[7].ColumnName = "申請人電話";
                        dt.Columns[8].ColumnName = "申請人電話2";
                        dt.Columns[9].ColumnName = "代理人姓名/單位名稱";
                        dt.Columns[10].ColumnName = "代理人身分證統一編號(或投保單位統一編號、醫事服務機構代號)";
                        dt.Columns[11].ColumnName = "代理人地址";
                        dt.Columns[12].ColumnName = "代理人電話";
                        dt.Columns[13].ColumnName = "代理人電話2";
                        dt.Columns[14].ColumnName = "收受或知悉日期";
                        dt.Columns[15].ColumnName = "核定文件1.文號";
                        dt.Columns[16].ColumnName = "文號日期";
                        dt.Columns[17].ColumnName = "字";
                        dt.Columns[18].ColumnName = "號";
                        dt.Columns[19].ColumnName = "核定文件2.繳款單";
                        dt.Columns[20].ColumnName = "核定文件3.其他";
                        dt.Columns[21].ColumnName = "請求事項";
                        dt.Columns[22].ColumnName = "事實及理由";

                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                dr[i] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        dtTemp.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }

        #region 爭議案件清冊
        public List<Map> GetReport7(SocialWorker model)
        {
            if (String.IsNullOrEmpty(model.Sdate))
                model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
            if (String.IsNullOrEmpty(model.Fdate))
                model.Fdate = Convert.ToDateTime(model.Sdate).AddDays(1).ToString("yyyy/MM/dd");
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        dbc.Transaction = st;
                        dbc.CommandText = @"
                        SELECT app.APP_ID,CONVERT(varchar, app.APP_TIME, 111) AS APP_TIME,app.NAME,app.IDN
                        ,Convert(varchar,ROW_NUMBER() OVER (ORDER BY app.APP_TIME ASC)) AS ROW_NUM
                        FROM APPLY app
                        LEFT JOIN ADMIN ad ON ad.ACC_NO = app.PRO_ACC
                        LEFT JOIN APPLY_041001 A2 ON A2.APP_ID = app.APP_ID
                        WHERE 1 = 1
                        and app.srv_id='041001'
                        and Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate 
                        ";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        using (SqlDataReader sda = dbc.ExecuteReader())
                        {
                            while (sda.Read())
                            {
                                Map map = new Map();
                                map.Add("ROW_NUM", DataUtils.GetDBString(sda, 4));
                                map.Add("APP_ID", DataUtils.GetDBString(sda, 0));
                                map.Add("APP_TIME", HelperUtil.DateTimeToTwString(Convert.ToDateTime(DataUtils.GetDBString(sda, 1))));
                                map.Add("NAME", DataUtils.GetDBString(sda, 2));
                                map.Add("IDN", DataUtils.GetDBString(sda, 3));
                                li.Add(map);
                            }
                            sda.Close();
                        }
                        if (li != null)
                        {
                            this.totalCount = li.Count();
                            li = li.Skip((model.queryModel.NowPage - 1) * this.pageSize).Take(this.pageSize).ToList();
                        }
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }
        #endregion
        #endregion 爭議案件申請清冊

        #region 年金案件申請清冊
        public DataTable GetSocialWork8(SocialWorker model)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlTransaction st = conn.BeginTransaction())
                {
                    using (SqlCommand dbc = conn.CreateCommand())
                    {
                        var dtTemp = new DataTable();
                        dbc.Transaction = st;
                        dbc.CommandText = @"
SELECT ROW_NUMBER() OVER (ORDER BY app.APP_TIME) AS RowIndex,ser.NAME as SRVNAME
,CONVERT(varchar, app.APP_TIME, 111) AS APPLY_DATE,app.APP_ID,app.NAME,app.IDN,convert(varchar,app.birthday,111) BIRTHDAY
,convert(varchar,app.ADDR_CODE)+z1.CITYNM+z1.TOWNNM+REPLACE(convert(varchar,app.ADDR),z1.CITYNM+z1.TOWNNM,'') C_ADDR,app.CNT_TEL,app.MOBILE
,A2.R_NAME,A2.R_IDN,CONVERT(VARCHAR,A2.R_BIRTH,111) R_BIRTH
,convert(varchar,A2.R_ADDR_CODE)+z3.CITYNM+z3.TOWNNM+REPLACE(convert(varchar,A2.R_ADDR),z3.CITYNM+z3.TOWNNM,'') R_ADDR,A2.R_TEL,A2.R_MOBILE
,Replace(Replace(A2.KINDTYPE,'1','核定文號'),'2','繳款單') KINDTYPE
,CONVERT(VARCHAR,A2.LIC_DATE,111) LIC_DATE,A2.LIC_CD,A2.LIC_NUM
,A2.PAY_YEAR,A2.PAY_MONTH,A2.PAY_NUM
,Convert(varchar,A2.KNOW_DATE,111) KNOW_DATE
,A2.KNOW_MEMO,A2.KNOW_FACT
FROM 
    APPLY app
LEFT JOIN ADMIN ad ON ad.ACC_NO = app.PRO_ACC
LEFT JOIN APPLY_006001 A2 ON A2.APP_ID = app.APP_ID
LEFT JOIN SERVICE ser ON ser.SRV_ID=app.SRV_ID
LEFT JOIN (SELECT DISTINCT ZIP_CO,CITYNM,TOWNNM FROM ZIPCODE) z1 ON z1.ZIP_CO=app.ADDR_CODE
LEFT JOIN (SELECT DISTINCT ZIP_CO,CITYNM,TOWNNM FROM ZIPCODE) z3 on z3.ZIP_CO=A2.R_ADDR_CODE
WHERE 1 = 1
and app.srv_id='006001'
and Convert(date,APP_TIME) BETWEEN @Sdate AND @Fdate 
";
                        dbc.Parameters.Clear();
                        DataUtils.AddParameters(dbc, "Sdate", model.Sdate);
                        DataUtils.AddParameters(dbc, "Fdate", model.Fdate);
                        SqlDataReader sda = dbc.ExecuteReader();
                        dtTemp.Load(sda);
                        dt = dtTemp.Clone();
                        dt.Columns[0].ColumnName = "編號";
                        dt.Columns[1].ColumnName = "申辦項目";
                        dt.Columns[2].ColumnName = "申辦日期";
                        dt.Columns[3].ColumnName = "案件編號";
                        dt.Columns[4].ColumnName = "申請人姓名";
                        dt.Columns[5].ColumnName = "申請人身分證統一編號";
                        dt.Columns[6].ColumnName = "申請人出生年月日";
                        dt.Columns[7].ColumnName = "申請人通訊地址(含郵遞區號)";
                        dt.Columns[8].ColumnName = "申請人連絡電話";
                        dt.Columns[9].ColumnName = "申請人手機號碼";
                        dt.Columns[10].ColumnName = "被保險人姓名";
                        dt.Columns[11].ColumnName = "被保險人身分證統一編號";
                        dt.Columns[12].ColumnName = "被保險人出生年月日";
                        dt.Columns[13].ColumnName = "被保險人地址";
                        dt.Columns[14].ColumnName = "被保險人連絡電話";
                        dt.Columns[15].ColumnName = "被保險人手機號碼";
                        dt.Columns[16].ColumnName = "不服勞保局核定文件";
                        dt.Columns[17].ColumnName = "核定文件日期";
                        dt.Columns[18].ColumnName = "核定文件字";
                        dt.Columns[19].ColumnName = "核定文件號";
                        dt.Columns[20].ColumnName = "繳款單年度";
                        dt.Columns[21].ColumnName = "繳款單月份";
                        dt.Columns[22].ColumnName = "繳款單字號";
                        dt.Columns[23].ColumnName = "收受或知悉日期";
                        dt.Columns[24].ColumnName = "請求事項";
                        dt.Columns[25].ColumnName = "事實與理由";

                        foreach (DataRow drTemp in dtTemp.Rows)
                        {
                            var dr = dt.NewRow();
                            for (var i = 0; i < dt.Columns.Count; i++)
                            {
                                dr[i] = drTemp[i];
                            }
                            dt.Rows.Add(dr);
                        }
                        dtTemp.Clear();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return dt;
        }
        #endregion 年金案件申請清冊
    }
}