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
using ES.Models.Entities;
using ES.Services;

namespace ES.Areas.Admin.Action
{
    public class FlyPayAction : BaseAction
    {
        public FlyPayAction()
        {
        }

        public FlyPayAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        public FlyPayAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// EXCEL 匯出
        /// </summary>
        /// <param name="strTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable CreateExcel(FlyPayModel form)
        {
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            //DataSet dataSet = new DataSet();
            DataTable resultTable = new DataTable();

            string s_group_1 = @" 
  a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[GENDER]
 ,a.[MAINDOCNO],a.[NATIONALITY],a.[FLIGHTDATE],a.[FLIGHTNO],a.[DEPARTAIRPORT],a.[PASSENGERTYPE]
 ,a.[PLACE],a.[ISPAY],a.[STATUS] ,a.[PAYDATE],a.[PAYMONEY],format(b.[GRANTDATE],'yyyy-MM-dd') GRANTDATE,a.TRACENO
 ,a.[PAYRESULT],a.[XID],a.QID";
            string s_group_1b = @" 
  a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[GENDER]
 ,a.[MAINDOCNO],a.[NATIONALITY],a.[FLIGHTDATE],a.[FLIGHTNO],a.[DEPARTAIRPORT],a.[PASSENGERTYPE]
 ,a.[PLACE],a.[ISPAY],a.[STATUS] ,a.[PAYDATE],a.[PAYMONEY],format(b.[GRANTDATE],'yyyy-MM-dd'),a.TRACENO
 ,a.[PAYRESULT],a.[XID],a.QID";

            string s_col_c1 = "FIRSTNAME,MIDDLENAME,LASTNAME,BIRTH,GENDER,MAINDOCNO,NATIONALITY,FLIGHTDATE,FLIGHTNO,DEPARTAIRPORT,PASSENGERTYPE,PLACE,ISPAY,STATUS,PAYDATE,PAYMONEY,GRANTDATE,TRACENO,PAYRESULT,XID,QID";
            string[] sa_col_c1 = s_col_c1.Split(',');
            string s_col_c2 = "FisrtName,MiddleName,LastName,Birth,Gender,MainDocNo,Nationality,FlightDate,FlightNo,DepartAirport,PassengerType,防疫所別,為應繳費人員,繳費狀態,繳費日期,繳費金額,撥款日期,授權碼,繳費編號,中信繳費編號,銀聯繳費編號,識別碼,驗證碼,資料行";
            string[] sa_col_c2 = s_col_c2.Split(',');

            StringBuilder sb = new StringBuilder();
            try
            {
                bool flag_use_leftjoin = true;
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { flag_use_leftjoin = false; }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { flag_use_leftjoin = false; }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { flag_use_leftjoin = false; }

                sb.Append(@" SELECT " + s_group_1);
                sb.Append(@" FROM FLYPAY a");
                if (flag_use_leftjoin) { sb.Append(@" LEFT"); }
                sb.Append(@" JOIN FLYSWIPE b on b.TRACENO=a.TRACENO AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE) AND b.DEL_MK = 'N'");
                sb.Append(@" WHERE a.DEL_MK = 'N'");

                //航班號碼
                if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { sb.Append(" and a.FLIGHTNO = @FLIGHTNO "); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { sb.Append(" and a.MAINDOCNO = @MAINDOCNO"); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { sb.Append(" and a.PAYRESULT = @PAYRESULT"); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { sb.Append(" and a.STATUS = @STATUS"); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { sb.Append(" and a.NEEDBACK = @NEEDBACK"); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue && form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue)
                {
                    sb.Append(" and replace(FLIGHTDATE,'-','') BETWEEN @FLIGHTDATE AND @FLIGHTDATE_END ");
                }
                else
                {
                    if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue) { sb.Append(" and replace(FLIGHTDATE,'-','') >= @FLIGHTDATE"); }
                    //航班日期
                    if (form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue) { sb.Append(" and replace(FLIGHTDATE,'-','') <= @FLIGHTDATE_END"); }
                }

                //防疫旅館
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { sb.Append(" and a.PLACE LIKE '%' + @PLACE + '%'"); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { sb.Append(" and a.TRACENO= @TRACENO"); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue && form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue)
                {
                    sb.Append(" and  convert(varchar, b.GRANTDATE, 23)  between @GRANTDATE1 and @GRANTDATE2 ");
                }
                else
                {
                    if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { sb.Append(" and b.GRANTDATE >= @GRANTDATE1"); }
                    //撥款日期區間2
                    if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { sb.Append(" and b.GRANTDATE <= @GRANTDATE2"); }
                }

                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue && form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue)
                {
                    sb.Append(" and convert(varchar, a.ADD_TIME, 23) between @ADDTIMES and @ADDTIMEE ");
                }
                else
                {
                    if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue) { sb.Append(" and a.ADD_TIME >= @ADDTIMES"); }
                    //新增日期迄
                    if (form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue) { sb.Append(" and a.ADD_TIME <= @ADDTIMEE"); }
                }
                sb.Append(@" GROUP BY " + s_group_1b);
                sb.Append(@" ORDER BY a.FLIGHTDATE, a.FLIGHTNO, a.FIRSTNAME");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);
                //航班號碼
                if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { DataUtils.AddParameters(com, "FLIGHTNO", form.QRY_FLIGHTNO); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { DataUtils.AddParameters(com, "MAINDOCNO", form.QRY_MAINDOCNO); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { DataUtils.AddParameters(com, "PAYRESULT", form.QRY_PAYRESULT); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { DataUtils.AddParameters(com, "STATUS", form.QRY_STATUS); }
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { DataUtils.AddParameters(com, "NEEDBACK", form.QRY_NEEDBACK); }

                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE", form.QRY_FLIGHTDATE.Value.ToString("yyyyMMdd")); }
                if (form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE_END", form.QRY_FLIGHTDATE_END.Value.ToString("yyyyMMdd")); }
                //防疫所別
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { DataUtils.AddParameters(com, "PLACE", form.QRY_PLACE); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { DataUtils.AddParameters(com, "TRACENO", form.QRY_TRACENO); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { DataUtils.AddParameters(com, "GRANTDATE1", form.QRY_GRANTDATE1.Value.ToString("yyyy-MM-dd")); }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { DataUtils.AddParameters(com, "GRANTDATE2", form.QRY_GRANTDATE2.Value.ToString("yyyy-MM-dd")); }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue) { DataUtils.AddParameters(com, "ADDTIMES", form.QRY_ADDTIMES.Value.ToString("yyyy-MM-dd")); }
                //新增日期迄
                if (form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue) { DataUtils.AddParameters(com, "ADDTIMEE", form.QRY_ADDTIMEE.Value.ToString("yyyy-MM-dd")); }

                resultTable.Load(com.ExecuteReader());

                //SqlDataAdapter sda = new SqlDataAdapter(dbc);
                //sda.Fill(dataSet, "report");
                //new 一個新的table來render to excel
                foreach (DataColumn column in resultTable.Columns.Cast<DataColumn>().AsQueryable().ToList())
                {
                    for (int i = 0; i < sa_col_c1.Length; i++)
                    {
                        if (column.ColumnName.Equals(sa_col_c1[i])) { column.ColumnName = sa_col_c2[i]; }
                    }
                }

                //resultTable.Columns.Add("FisrtName");
                //resultTable.Columns.Add("MiddleName");
                //resultTable.Columns.Add("LastName");
                //resultTable.Columns.Add("Birth");
                //resultTable.Columns.Add("Gender");
                //resultTable.Columns.Add("MainDocNo");
                //resultTable.Columns.Add("Nationality");
                //resultTable.Columns.Add("FlightDate");
                //resultTable.Columns.Add("FlightNo");
                //resultTable.Columns.Add("DepartAirport");
                //resultTable.Columns.Add("PassengerType");
                //resultTable.Columns.Add("繳費日期");
                //resultTable.Columns.Add("繳費金額");
                //resultTable.Columns.Add("繳費編號");
                //resultTable.Columns.Add("防疫所別");
                //resultTable.Columns.Add("為應繳費人員");
                //resultTable.Columns.Add("繳費狀態");
                //resultTable.Columns.Add("中信繳費編號");
                //resultTable.Columns.Add("銀聯追蹤碼");
                //resultTable.Columns.Add("銀聯繳費編號");

                //int row_index = 0;
                ////將select結果塞進新table
                //foreach (DataRow row in dataSet.Tables["report"].Rows)
                //{
                //    resultTable.Rows.Add();
                //    resultTable.Rows[row_index]["FisrtName"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["FIRSTNAME"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["FIRSTNAME"]);
                //    resultTable.Rows[row_index]["MiddleName"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["MIDDLENAME"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["MIDDLENAME"]);
                //    resultTable.Rows[row_index]["LastName"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["LASTNAME"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["LASTNAME"]);
                //    resultTable.Rows[row_index]["Birth"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["BIRTH"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["BIRTH"]);
                //    resultTable.Rows[row_index]["Gender"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["GENDER"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["GENDER"]);
                //    resultTable.Rows[row_index]["MainDocNo"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["MAINDOCNO"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["MAINDOCNO"]);
                //    resultTable.Rows[row_index]["Nationality"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["NATIONALITY"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["NATIONALITY"]);
                //    resultTable.Rows[row_index]["FlightDate"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["FLIGHTDATE"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["FLIGHTDATE"]);
                //    resultTable.Rows[row_index]["FlightNo"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["FLIGHTNO"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["FLIGHTNO"]);
                //    resultTable.Rows[row_index]["DepartAirport"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["DEPARTAIRPORT"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["DEPARTAIRPORT"]);
                //    resultTable.Rows[row_index]["PassengerType"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["PASSENGERTYPE"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["PASSENGERTYPE"]);
                //    resultTable.Rows[row_index]["繳費日期"] = Convert.ToString(dataSet.Tables["report"].Rows[row_index]["PAYDATE"]);
                //    resultTable.Rows[row_index]["繳費金額"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["PAYMONEY"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["PAYMONEY"]);
                //    resultTable.Rows[row_index]["繳費編號"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["PAYRESULT"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["PAYRESULT"]);
                //    resultTable.Rows[row_index]["防疫所別"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["PLACE"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["PLACE"]);
                //    resultTable.Rows[row_index]["為應繳費人員"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["ISPAY"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["ISPAY"]);
                //    resultTable.Rows[row_index]["繳費狀態"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["STATUS"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["STATUS"]);
                //    resultTable.Rows[row_index]["中信繳費編號"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["XID"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["XID"]);
                //    resultTable.Rows[row_index]["銀聯追蹤碼"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["TRACENO"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["TRACENO"]);
                //    resultTable.Rows[row_index]["銀聯繳費編號"] = Convert.IsDBNull(dataSet.Tables["report"].Rows[row_index]["QID"]) ? "" : DataUtils.DecodeValue((string)dataSet.Tables["report"].Rows[row_index]["QID"]);

                //    ++row_index;
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }

            return resultTable;
        }

        /// <summary>
        /// EXCEL 匯出 防疫旅館
        /// </summary>
        /// <param name="strTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable CreateExcelBasic(FlyPayModel form)
        {
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            //DataSet dataSet = new DataSet();
            DataTable resultTable = new DataTable();

            string s_group_1 = @" 
  a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[GENDER]
 ,a.[MAINDOCNO],a.[NATIONALITY],a.[FLIGHTDATE],a.[FLIGHTNO],a.[DEPARTAIRPORT],a.[PASSENGERTYPE]
 ,a.[PLACE],a.[ISPAY],a.[STATUS] ,a.[PAYDATE],a.[PAYMONEY],format(b.[GRANTDATE],'yyyy-MM-dd') GRANTDATE
 ,CASE WHEN isnull(b.TRACENO_QID,'') <> '' THEN b.TRACENO_QID+' (銀聯)' ELSE b.TRACENO END AS TRACENO                                                     
 ,a.[PAYRESULT],a.[XID],a.QID,a.[GUID],a.[ADD_ACC],a.[FLY_ID]
, CASE spr.[SECTION] WHEN '1' THEN '北' WHEN '2' THEN '中' WHEN '3' THEN '南' END + '-' + isnull(de.se_val,'') AS SECTION_TEXT,a.[SPRCODE] ";
            string s_group_2 = @"
    , ISNULL(cd.[name],'') AS FLYCONTRY
    ,isnull(concat(cd2.[name],(SELECT  ',' + cast(flycountry.[name] AS VARCHAR ) from flypaybasiccon
	left join flycountry on CONCAT(flycountry.code1,'-',flycountry.code2,'-',flycountry.code3) = flypaybasiccon.[COUNTRY_ID]
	where flypaybasiccon.fly_id = a.fly_id
	FOR XML PATH(''))),'') AS FLYCONTRYTR";
            string s_group_1b = @" 
  a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[GENDER]
 ,a.[MAINDOCNO],a.[NATIONALITY],a.[FLIGHTDATE],a.[FLIGHTNO],a.[DEPARTAIRPORT],a.[PASSENGERTYPE]
 ,a.[PLACE],a.[ISPAY],a.[STATUS] ,a.[PAYDATE],a.[PAYMONEY],format(b.[GRANTDATE],'yyyy-MM-dd')
    ,b.[TRACENO],b.[TRACENO_QID]
 ,a.[PAYRESULT],a.[XID],a.QID,a.[GUID],a.[ADD_ACC],a.[FLY_ID]";
            string s_group_2b = @",cd.[name],cd2.[name],spr.[SECTION],a.[SPRCODE],de.[SE_VAL] ";

            string s_col_c1 = "FIRSTNAME,MIDDLENAME,LASTNAME,BIRTH,GENDER,MAINDOCNO,NATIONALITY,FLIGHTDATE,FLIGHTNO,DEPARTAIRPORT,PASSENGERTYPE,PLACE,ISPAY,STATUS,PAYDATE,PAYMONEY,GRANTDATE,TRACENO,PAYRESULT,XID,QID,GUID,ADD_ACC,FLY_ID,FLYCONTRY,FLYCONTRYTR,SECTION_TEXT,SPRCODE";
            string[] sa_col_c1 = s_col_c1.Split(',');
            string s_col_c2 = "FisrtName,MiddleName,LastName,Birth,Gender,MainDocNo,Nationality,FlightDate,FlightNo,DepartAirport,PassengerType,防疫所別,為應繳費人員,繳費狀態,繳費日期,繳費金額,撥款日期,授權碼,繳費編號,中信繳費編號,銀聯繳費編號,識別碼,驗證碼,資料行,出境國家,轉機國家,地區(春節專案),春節專案流水號";
            string[] sa_col_c2 = s_col_c2.Split(',');

            StringBuilder sb = new StringBuilder();
            try
            {
                bool flag_use_leftjoin = true;
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { flag_use_leftjoin = false; }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { flag_use_leftjoin = false; }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { flag_use_leftjoin = false; }

                sb.Append(@" SELECT " + s_group_1 + s_group_2);
                sb.Append(@" FROM FLYPAYBASIC a");
                sb.Append(@" left join flycountry cd on CONCAT(cd.code1,'-',cd.code2,'-',cd.code3) = a.flycontry ");
                sb.Append(@" left join flycountry cd2 on CONCAT(cd2.code1,'-',cd2.code2,'-',cd2.code3) = a.flycontrytr ");
                sb.Append(@" left join flypaybasicspr spr on spr.fly_id = a.fly_id ");
                sb.Append(@" left join flypayrooms_de de on de.se_code = substring(a.sprcode,2,2) ");
                if (flag_use_leftjoin) { sb.Append(@" LEFT"); }
                sb.Append(@" JOIN FLYSWIPE b on (b.TRACENO=a.TRACENO OR b.TRACENO_QID=a.TRACENO) AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE) AND b.DEL_MK = 'N'");
                sb.Append(@" WHERE a.DEL_MK = 'N'");

                //航班號碼
                //if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { sb.Append(" and a.FLIGHTNO = @FLIGHTNO "); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { sb.Append(" and a.MAINDOCNO LIKE '%' + @MAINDOCNO  + '%' "); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { sb.Append(" and a.PAYRESULT = @PAYRESULT"); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { sb.Append(" and a.STATUS = @STATUS"); }
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { sb.Append(" and a.NEEDBACK = @NEEDBACK"); }

                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue && form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue)
                {
                    sb.Append(" and replace(FLIGHTDATE,'-','') BETWEEN @FLIGHTDATE AND @FLIGHTDATE_END ");
                }

                //防疫旅館
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { sb.Append(" and a.PLACE LIKE '%' + @PLACE + '%'"); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { sb.Append(" and a.TRACENO= @TRACENO"); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue && form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue)
                {
                    sb.Append(" and  convert(varchar, b.GRANTDATE, 23)  between @GRANTDATE1 and @GRANTDATE2 ");
                }

                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue && form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue)
                {
                    sb.Append(" and convert(varchar, a.ADD_TIME, 23) between @ADDTIMES and @ADDTIMEE ");
                }

                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { sb.Append(" and cd.NAME LIKE '%' + @FLYCONTRY + '%'"); }
                //銀行別
                if (!String.IsNullOrEmpty(form.QRY_BANKTYPE))
                {
                    if (form.QRY_BANKTYPE == "2") { sb.Append(" and a.BANKTYPE = @BANKTYPE"); } //玉山
                    else { sb.Append(" and (a.BANKTYPE IS NULL or a.BANKTYPE <> '2')"); } //中信
                }
                //春節專案流水號
                if (!String.IsNullOrEmpty(form.QRY_SPRCODE)) { sb.Append(" and a.SPRCODE LIKE '%' + @SPRCODE  + '%' "); }
                //訂房識別碼
                if (!String.IsNullOrEmpty(form.QRY_GUID)) { sb.Append(" and a.GUID = @GUID "); }
                //地區(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_SECTION)) { sb.Append(" and spr.SECTION = @SECTION"); }
                //價錢(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_PLEVEL)) { sb.Append(" and spr.PLEVEL LIKE '%' + @PLEVEL  + '%' "); }

                sb.Append(@" GROUP BY " + s_group_1b + s_group_2b);
                sb.Append(@" ORDER BY a.FLIGHTDATE, a.FIRSTNAME");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);
                //航班號碼
                //if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { DataUtils.AddParameters(com, "FLIGHTNO", form.QRY_FLIGHTNO); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { DataUtils.AddParameters(com, "MAINDOCNO", form.QRY_MAINDOCNO); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { DataUtils.AddParameters(com, "PAYRESULT", form.QRY_PAYRESULT); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { DataUtils.AddParameters(com, "STATUS", form.QRY_STATUS); }
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { DataUtils.AddParameters(com, "NEEDBACK", form.QRY_NEEDBACK); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE", form.QRY_FLIGHTDATE.Value.ToString("yyyyMMdd")); }
                if (form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE_END", form.QRY_FLIGHTDATE_END.Value.ToString("yyyyMMdd")); }
                //防疫所別
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { DataUtils.AddParameters(com, "PLACE", form.QRY_PLACE); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { DataUtils.AddParameters(com, "TRACENO", form.QRY_TRACENO); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { DataUtils.AddParameters(com, "GRANTDATE1", form.QRY_GRANTDATE1.Value.ToString("yyyy-MM-dd")); }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { DataUtils.AddParameters(com, "GRANTDATE2", form.QRY_GRANTDATE2.Value.ToString("yyyy-MM-dd")); }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue) { DataUtils.AddParameters(com, "ADDTIMES", form.QRY_ADDTIMES.Value.ToString("yyyy-MM-dd")); }
                //新增日期迄
                if (form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue) { DataUtils.AddParameters(com, "ADDTIMEE", form.QRY_ADDTIMEE.Value.ToString("yyyy-MM-dd")); }

                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { DataUtils.AddParameters(com, "FLYCONTRY", form.QRY_FLYCONTRY); }
                //銀行別
                if (!String.IsNullOrEmpty(form.QRY_BANKTYPE)) { DataUtils.AddParameters(com, "BANKTYPE", form.QRY_BANKTYPE); }
                //春節專案流水號
                if (!String.IsNullOrEmpty(form.QRY_SPRCODE)) { DataUtils.AddParameters(com, "SPRCODE", form.QRY_SPRCODE); }
                //訂房識別碼
                if (!String.IsNullOrEmpty(form.QRY_GUID)) { DataUtils.AddParameters(com, "GUID", form.QRY_GUID); }
                //地區(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_SECTION)) { DataUtils.AddParameters(com, "SECTION", form.QRY_SECTION); }
                //價錢(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_PLEVEL)) { DataUtils.AddParameters(com, "PLEVEL", form.QRY_PLEVEL); }

                resultTable.Load(com.ExecuteReader());

                //SqlDataAdapter sda = new SqlDataAdapter(dbc);
                //sda.Fill(dataSet, "report");
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

        /// <summary>
        /// EXCEL 匯出 簡易專案
        /// </summary>
        /// <param name="strTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable CreateExcelBasicSIM(FlyPayModel form)
        {
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            //DataSet dataSet = new DataSet();
            DataTable resultTable = new DataTable();

            string s_group_1 = @" a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[MAINDOCNO]
 ,a.[FIRSTNAME2],a.[MIDDLENAME2],a.[LASTNAME2],a.[BIRTH2],a.[MAINDOCNO2]
 ,a.[FIRSTNAME3],a.[MIDDLENAME3],a.[LASTNAME3],a.[BIRTH3],a.[MAINDOCNO3]
 ,a.[FIRSTNAME4],a.[MIDDLENAME4],a.[LASTNAME4],a.[BIRTH4],a.[MAINDOCNO4]
 ,a.[FLIGHTDATE],a.[STATUS] ,a.[PAYDATE],a.[PAYMONEY],format(b.[GRANTDATE],'yyyy-MM-dd') GRANTDATE
 ,CASE WHEN isnull(b.TRACENO_QID,'') <> '' THEN b.TRACENO_QID+' (銀聯)' ELSE b.TRACENO END AS TRACENO 
 ,a.[PAYRESULT],a.[XID],a.QID,a.[GUID],a.[ADD_ACC],a.[FLY_ID] ";
            string s_group_2 = @" ,ISNULL(cd.[name],'') AS FLYCONTRY
    ,isnull(concat(cd2.[name],(SELECT  ',' + cast(flycountry.[name] AS VARCHAR ) from flypaybasiccon
	left join flycountry on CONCAT(flycountry.code1,'-',flycountry.code2,'-',flycountry.code3) = flypaybasiccon.[COUNTRY_ID]
	where flypaybasiccon.fly_id = a.fly_id
	FOR XML PATH(''))),'') AS FLYCONTRYTR ";
            string s_group_1b = @" a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[MAINDOCNO]
 ,a.[FIRSTNAME2],a.[MIDDLENAME2],a.[LASTNAME2],a.[BIRTH2],a.[MAINDOCNO2]
 ,a.[FIRSTNAME3],a.[MIDDLENAME3],a.[LASTNAME3],a.[BIRTH3],a.[MAINDOCNO3]
 ,a.[FIRSTNAME4],a.[MIDDLENAME4],a.[LASTNAME4],a.[BIRTH4],a.[MAINDOCNO4]
 ,a.[FLIGHTDATE],a.[STATUS] ,a.[PAYDATE],a.[PAYMONEY]
 ,a.[PAYRESULT],a.[XID],a.QID,a.[GUID],a.[ADD_ACC],a.[FLY_ID] ";
            string s_group_2b = @" ,format(b.[GRANTDATE],'yyyy-MM-dd'),b.[TRACENO],b.[TRACENO_QID],cd.[name],cd2.[name] ";

            string s_col_c1 = "FIRSTNAME,MIDDLENAME,LASTNAME,BIRTH,MAINDOCNO,FIRSTNAME2,MIDDLENAME2,LASTNAME2,BIRTH2,MAINDOCNO2,FIRSTNAME3,MIDDLENAME3,LASTNAME3,BIRTH3,MAINDOCNO3,FIRSTNAME4,MIDDLENAME4,LASTNAME4,BIRTH4,MAINDOCNO4,FLIGHTDATE,STATUS,PAYDATE,PAYMONEY,GRANTDATE,TRACENO,PAYRESULT,XID,QID,GUID,ADD_ACC,FLY_ID,FLYCONTRY,FLYCONTRYTR";
            string[] sa_col_c1 = s_col_c1.Split(',');
            string s_col_c2 = "FisrtName(1),MiddleName(1),LastName(1),Birth(1),MainDocNo(1),FisrtName(2),MiddleName(2),LastName(2),Birth(2),MainDocNo(2),FisrtName(3),MiddleName(3),LastName(3),Birth(3),MainDocNo(3),FisrtName(4),MiddleName(4),LastName(4),Birth(4),MainDocNo(4),抵達日期,繳費狀態,繳費日期,繳費金額,撥款日期,授權碼,繳費編號,中信繳費編號,銀聯繳費編號,識別碼,驗證碼,資料行,出境國家,轉機國家";
            string[] sa_col_c2 = s_col_c2.Split(',');

            StringBuilder sb = new StringBuilder();
            try
            {
                bool flag_use_leftjoin = true;
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { flag_use_leftjoin = false; }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { flag_use_leftjoin = false; }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { flag_use_leftjoin = false; }

                sb.Append(@" SELECT " + s_group_1 + s_group_2);
                sb.Append(@" FROM FLYPAYBASIC a");
                sb.Append(@" left join flycountry cd on CONCAT(cd.code1,'-',cd.code2,'-',cd.code3) = a.flycontry ");
                sb.Append(@" left join flycountry cd2 on CONCAT(cd2.code1,'-',cd2.code2,'-',cd2.code3) = a.flycontrytr ");
                if (flag_use_leftjoin) { sb.Append(@" LEFT"); }
                sb.Append(@" JOIN FLYSWIPE b on (b.TRACENO=a.TRACENO OR b.TRACENO_QID=a.TRACENO) AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE) AND b.DEL_MK = 'N'");
                sb.Append(@" WHERE a.DEL_MK = 'N' and a.FLYTYPE='11' ");

                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { sb.Append(" and (a.MAINDOCNO LIKE '%' + @MAINDOCNO  + '%' or a.MAINDOCNO2 LIKE '%' + @MAINDOCNO  + '%' or a.MAINDOCNO3 LIKE '%' + @MAINDOCNO  + '%' or a.MAINDOCNO4 LIKE '%' + @MAINDOCNO  + '%') "); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { sb.Append(" and a.PAYRESULT = @PAYRESULT"); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { sb.Append(" and a.STATUS = @STATUS"); }
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { sb.Append(" and a.NEEDBACK = @NEEDBACK"); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue && form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue)
                {
                    sb.Append(" and replace(FLIGHTDATE,'-','') BETWEEN @FLIGHTDATE AND @FLIGHTDATE_END ");
                }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { sb.Append(" and a.TRACENO= @TRACENO"); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue && form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue)
                {
                    sb.Append(" and  convert(varchar, b.GRANTDATE, 23)  between @GRANTDATE1 and @GRANTDATE2 ");
                }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue && form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue)
                {
                    sb.Append(" and convert(varchar, a.ADD_TIME, 23) between @ADDTIMES and @ADDTIMEE ");
                }
                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { sb.Append(" and cd.NAME LIKE '%' + @FLYCONTRY + '%'"); }
                sb.Append(@" GROUP BY " + s_group_1b + s_group_2b);
                sb.Append(@" ORDER BY a.FLIGHTDATE, a.FIRSTNAME");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { DataUtils.AddParameters(com, "MAINDOCNO", form.QRY_MAINDOCNO); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { DataUtils.AddParameters(com, "PAYRESULT", form.QRY_PAYRESULT); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { DataUtils.AddParameters(com, "STATUS", form.QRY_STATUS); }
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { DataUtils.AddParameters(com, "NEEDBACK", form.QRY_NEEDBACK); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE", form.QRY_FLIGHTDATE.Value.ToString("yyyyMMdd")); }
                if (form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE_END", form.QRY_FLIGHTDATE_END.Value.ToString("yyyyMMdd")); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { DataUtils.AddParameters(com, "TRACENO", form.QRY_TRACENO); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { DataUtils.AddParameters(com, "GRANTDATE1", form.QRY_GRANTDATE1.Value.ToString("yyyy-MM-dd")); }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { DataUtils.AddParameters(com, "GRANTDATE2", form.QRY_GRANTDATE2.Value.ToString("yyyy-MM-dd")); }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue) { DataUtils.AddParameters(com, "ADDTIMES", form.QRY_ADDTIMES.Value.ToString("yyyy-MM-dd")); }
                //新增日期迄
                if (form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue) { DataUtils.AddParameters(com, "ADDTIMEE", form.QRY_ADDTIMEE.Value.ToString("yyyy-MM-dd")); }
                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { DataUtils.AddParameters(com, "FLYCONTRY", form.QRY_FLYCONTRY); }
                resultTable.Load(com.ExecuteReader());

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


        /// <summary>
        /// EXCEL 匯出 春節專案
        /// </summary>
        /// <param name="strTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public DataTable CreateExcelBasicSPR()
        {
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            DataTable resultTable = new DataTable();

            StringBuilder sb = new StringBuilder();
            try
            {
                #region TSQL

                sb.Append(@"
     if object_id('tempdb..##resultSPR') is not null 
        begin
	        drop table ##resultSPR
        end
        declare @ord int
        declare @unit_id varchar(10)
		declare @unit_code varchar(10)
		declare @unit_sec varchar(10)
		declare @unit_secC varchar(10)
		
        declare @unit_name nvarchar(50)
        declare @row_num int = 1
        declare @ap1101214  varchar(10)
        declare @ap1101215  varchar(10)
        declare @ap1101216  varchar(10)
        declare @ap1101217  varchar(10)
        declare @ap1101218  varchar(10)
        declare @ap1101219  varchar(10)
        declare @ap1101220  varchar(10)
        declare @ap1101221  varchar(10)
        declare @ap1101222  varchar(10)

        declare @ap1101223  varchar(10)
        declare @ap1101224  varchar(10)
        declare @ap1101225  varchar(10)
        declare @ap1101226  varchar(10)
        declare @ap1101227  varchar(10)
        declare @ap1101228  varchar(10)
        declare @ap1101229  varchar(10)
        declare @ap1101230  varchar(10)
        declare @ap1101231  varchar(10)

        declare @ap1110101  varchar(10)
        declare @ap1110102  varchar(10)
        declare @ap1110103  varchar(10)
        declare @ap1110104  varchar(10)
        declare @ap1110105  varchar(10)
        declare @ap1110106  varchar(10)
        declare @ap1110107  varchar(10)
		declare @ap1110108  varchar(10)
		declare @ap1110109  varchar(10)

		declare @ap1110110  varchar(10)
		declare @ap1110111  varchar(10)
		declare @ap1110112  varchar(10)
		declare @ap1110113  varchar(10)
		declare @ap1110114  varchar(10)
		declare @ap1110115  varchar(10)
		declare @ap1110116  varchar(10)
		declare @ap1110117  varchar(10)
		declare @ap1110118  varchar(10)

		declare @ap1110119  varchar(10)
		declare @ap1110120  varchar(10)
		declare @ap1110121  varchar(10)
		declare @ap1110122  varchar(10)
		declare @ap1110123  varchar(10)
		declare @ap1110124  varchar(10)
		declare @ap1110125  varchar(10)
		declare @ap1110126  varchar(10)
		declare @ap1110127  varchar(10)

		declare @ap1110128  varchar(10)
		declare @ap1110129  varchar(10)
		declare @ap1110130  varchar(10)
		declare @ap1110131  varchar(10)
		declare @ap1110201  varchar(10)
		declare @ap1110202  varchar(10)
		declare @ap1110203  varchar(10)
		declare @ap1110204  varchar(10)
		declare @ap1110205  varchar(10)

		declare @apTotal int /*總計*/

        create table ##resultSPR
        (
			row_index int,
			unit_sec varchar(10),
			unit_code varchar(10),
	        unit_name nvarchar(50),
	        ap1101214  varchar(10),
	        ap1101215  varchar(10),
			ap1101216  varchar(10),
			ap1101217  varchar(10),
			ap1101218  varchar(10),
			ap1101219  varchar(10),
			ap1101220  varchar(10),
			ap1101221  varchar(10),
			ap1101222  varchar(10),
			ap1101223  varchar(10),
			ap1101224  varchar(10),
			ap1101225  varchar(10),
			ap1101226  varchar(10),
			ap1101227  varchar(10),
			ap1101228  varchar(10),
			ap1101229  varchar(10),
			ap1101230  varchar(10),
			ap1101231  varchar(10),
			ap1110101  varchar(10),
			ap1110102  varchar(10),
			ap1110103  varchar(10),
			ap1110104  varchar(10),
			ap1110105  varchar(10),
			ap1110106  varchar(10),
			ap1110107  varchar(10),
			ap1110108  varchar(10),
			ap1110109  varchar(10),
			ap1110110  varchar(10),
			ap1110111  varchar(10),
			ap1110112  varchar(10),
			ap1110113  varchar(10),
			ap1110114  varchar(10),
			ap1110115  varchar(10),
			ap1110116  varchar(10),
			ap1110117  varchar(10),
			ap1110118  varchar(10),
			ap1110119  varchar(10),
			ap1110120  varchar(10),
			ap1110121  varchar(10),
			ap1110122  varchar(10),
			ap1110123  varchar(10),
			ap1110124  varchar(10),
			ap1110125  varchar(10),
			ap1110126  varchar(10),
			ap1110127  varchar(10),
			ap1110128  varchar(10),
			ap1110129  varchar(10),
			ap1110130  varchar(10),
			ap1110131  varchar(10),
			ap1110201  varchar(10),
			ap1110202  varchar(10),
			ap1110203  varchar(10),
			ap1110204  varchar(10),
			ap1110205  varchar(10)

        )  

        begin
		        declare unit_cursor cursor for
		        select ord,unit_id, unit_name,unit_secC from (
					        values
							(1,'統計日期','春節專案集中檢疫所訂房情形','TT'),
							(2,'','春節專案集中檢疫所訂房情形','TP'),
					(9, '', '每日上限總數','TA'),
					(10,'A','北區每日上限','A'),
					(11,'小計','北區','A'),
					  (1001,'01','烏來','A'),
		              (1002,'02','陽明山','A'),  
		              (1003,'03','林口','A'),
		              (1004,'04','湖口','A'),
		              (1005,'05','大崗','A'),
		              (1006,'06','汐止','A'), 
		              (1007,'07','竹林','A'),  
		              (1008,'08','桂山','A'),  
		              (1009,'09','楊梅','A'),  
		              (1010,'10','萬里','A'), 
		              (1011,'11','士林','A'),
		              (1012,'12','龜山','A'),
		              (1013,'13','中正','A'),
		              (1014,'14','桃園二','A'),
		              (1015,'15','八德','A'),
		              (1016,'16','蘭陽','A'),
		              (1017,'17','礁溪','A'),
					  (2010,'B','中區每日上限','B'),
					  (2011,'小計','中區','B'),
		              (2018,'18','中興','B'),
		              (2019,'19','彰化','B'),
					  (2020,'20','草屯','B'),
					  (2021,'21','豐原','B'),
					  (2022,'22','花壇','B'),
					  (2023,'23','臺中東區','B'),
					  (2024,'24','竹山','B'),
					  (2025,'25','臺中二中','B'),
					  (3010,'C','南區每日上限','C'),
					  (3011,'小計','南區','C'),
					  (3026,'26','大寮','C'),
					  (3027,'27','官田','C'),
					  (3028,'28','新化1','C'),
					  (3029,'29','新化2','C'),
					  (3030,'30','新化3','C'),
					  (3031,'31','長治','C'),
					  (3032,'32','大林','C'),
					  (3033,'33','燕巢','C'),
					  (3034,'34','太保','C'),
					  (3035,'35','朴子','C'),
					  (3036,'36','麻豆','C'),
					  (3037,'37','高埕-1','C'),
					  (3038,'38','高埕-2','C'),
					  (3039,'39','高新','C'),
					  (3040,'40','高前','C'),
					  (3041,'41','中埔','C'),
					  (3042,'42','嘉義','C')
					        ) AS UnitMap(ord,unit_id, unit_name,unit_secC) 
					        order by ord 

		        open unit_cursor  
  
		        fetch next from unit_cursor into @ord,@unit_id, @unit_name,@unit_secC  
  
		        while @@FETCH_STATUS = 0  
		        begin 
				/*初始化*/
					set @unit_sec = @unit_secC
					set @unit_code = @unit_id
					set @ap1101214 = '-'
					set @ap1101215 = '-'
					set @ap1101216 = '-'
					set @ap1101217 = '-'
					set @ap1101218 = '-'
					set @ap1101219 = '-'
					set @ap1101220 = '-'
					set @ap1101221 = '-'
					set @ap1101222 = '-'
					set @ap1101223 = '-'
					set @ap1101224 = '-'
					set @ap1101225 = '-'
					set @ap1101226 = '-'
					set @ap1101227 = '-'
					set @ap1101228 = '-'
					set @ap1101229 = '-'
					set @ap1101230 = '-'
					set @ap1101231 = '-'
					set @ap1110101 = '-'
					set @ap1110102 = '-'
					set @ap1110103 = '-'
					set @ap1110104 = '-'
					set @ap1110105 = '-'
					set @ap1110106 = '-'
					set @ap1110107 = '-'
					set @ap1110108 = '-'
					set @ap1110109 = '-'
					set @ap1110110 = '-'
					set @ap1110111 = '-'
					set @ap1110112 = '-'
					set @ap1110113 = '-'
					set @ap1110114 = '-'
					set @ap1110115 = '-'
					set @ap1110116 = '-'
					set @ap1110117 = '-'
					set @ap1110118 = '-'
					set @ap1110119 = '-'
					set @ap1110120 = '-'
					set @ap1110121 = '-'
					set @ap1110122 = '-'
					set @ap1110123 = '-'
					set @ap1110124 = '-'
					set @ap1110125 = '-'
					set @ap1110126 = '-'
					set @ap1110127 = '-'
					set @ap1110128 = '-'
					set @ap1110129 = '-'
					set @ap1110130 = '-'
					set @ap1110131 = '-'
					set @ap1110201 = '-'
					set @ap1110202 = '-'
					set @ap1110203 = '-'
					set @ap1110204 = '-'
					set @ap1110205 = '-'
					/*春節專案集中檢疫所訂房情形*/
					if @unit_sec = 'TT'
					begin 
						set @unit_name = FORMAT (getdate(), 'yyyy-MM-dd HH:mm:ss')
						set @ap1101214 = '可預約房數'
						set @ap1101215 = case when GETDATE() > '2021-11-23 10:00:00' then 3600*6 else '7200' end 
						set @ap1101216 = '目前空房數'
						set @ap1101218 = '已預約房數'
						select @ap1101219 = isnull(ucnt,'0') from ( select count(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
						WHERE ISNULL(B.SPRCODE,'') <> '' AND B.STATUS = 'Y') A
						set @ap1101217 = convert(int,@ap1101215) - convert(int,@ap1101219)
						end
					else if @unit_sec = 'TP'
					begin
						set @unit_name = '各區已預約'
						set @ap1101214 = '北區'
						select @ap1101215 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASICSPR B
						JOIN FLYPAYBASIC A ON B.FLY_ID = A.FLY_ID
						where B.SECTION = '1' AND A.STATUS = 'Y' AND ISNULL(A.SPRCODE,'') <> '') A
						set @ap1101216 = '中區'
						select @ap1101217 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASICSPR B
						JOIN FLYPAYBASIC A ON B.FLY_ID = A.FLY_ID
						where B.SECTION = '2' AND A.STATUS = 'Y' AND ISNULL(A.SPRCODE,'') <> '') A
						set @ap1101218 = '南區'
						select @ap1101219 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASICSPR B
						JOIN FLYPAYBASIC A ON B.FLY_ID = A.FLY_ID
						where B.SECTION = '3' AND A.STATUS = 'Y' AND ISNULL(A.SPRCODE,'') <> '') A
						end
					/*集檢所每日數量*/
					else if @unit_secC = 'TA'
					begin
						/****D1 1-6梯次計算****/
						select @ap1101214 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-14' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-14' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-14' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101223 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-23' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-23' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-23' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110101 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-01' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-01' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-01' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110110 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-10' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-10' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-10' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110119 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-19' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-19' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-19' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D1' and ECHELON = '1' group by DAYVAL) A) AA
						
						select @ap1110128 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-28' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-28' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-28' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D1' and ECHELON = '1' group by DAYVAL) A) AA

						/**** D2 1-6梯計算****/
						select @ap1101215 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-15' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-15' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-15' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D2' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101224 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-24' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-24' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-24' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D2' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110102 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-02' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-02' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-02' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D2' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110111 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-11' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-11' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-11' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D2' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110120 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-20' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-20' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-20' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D2' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110129 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-29' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-29' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-29' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D2' and ECHELON = '1' group by DAYVAL) A) AA

						/**** D3 1-6梯計算****/
						select @ap1101216 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D3' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101225 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-25' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-25' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-25' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D3' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110103 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-03' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-03' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-03' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D3' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110112 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-12' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-12' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-12' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D3' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110121 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-21' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-21' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-21' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D3' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110130 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-30' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-30' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-30' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D3' and ECHELON = '1' group by DAYVAL) A) AA

						/**** D4 1-6梯計算****/
						select @ap1101217 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-17' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-17' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-17' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D4' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101226 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-26' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-26' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-26' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D4' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110104 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-04' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-04' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-04' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D4' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110113 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-13' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-13' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-13' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D4' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110122 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-22' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-22' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-22' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D4' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110131 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-31' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-31' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-31' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D4' and ECHELON = '1' group by DAYVAL) A) AA

						/**** D5 1-6梯計算****/
						select @ap1101218 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-18' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-18' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-18' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D5' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101227 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-27' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-27' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-27' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D5' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110105 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-05' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-05' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-05' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D5' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110114 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-14' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-14' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-14' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D5' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110123 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-23' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-23' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-23' AND ISNULL(B.SPRCODE,'') <> '')A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D5' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110201 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-01' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-01' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-01' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D5' and ECHELON = '1' group by DAYVAL) A) AA

						/**** D6 1-6梯計算****/
						select @ap1101219 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-19' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-19' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-19' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D6' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101228 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-28' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-28' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-28' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D6' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110106 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-06' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-06' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-06' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D6' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110115 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-15' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-15' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-15' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D6' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110124 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-24' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-24' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-24' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D6' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110202 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-02' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-02' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-02' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D6' and ECHELON = '1' group by DAYVAL) A) AA

						
						/**** D7 1-6梯計算****/
						select @ap1101220 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-20' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-20' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-20' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D7' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101229 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-29' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-29' AND ISNULL(B.SPRCODE,'') <> '')A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-29' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D7' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110107 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-07' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-07' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-07' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D7' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110116 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-16' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-16' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-16' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D7' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110125 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-25' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-25' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-25' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D7' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110203 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-03' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-03' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-03' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D7' and ECHELON = '1' group by DAYVAL) A) AA

						/**** D8 1-6梯計算****/
						select @ap1101221 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-21' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-21' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-21' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D8' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101230 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D8' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110108 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-08' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-08' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-08' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D8' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110117 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-17' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-17' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-17' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D8' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110126 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-26' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-26' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-26' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D8' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110204 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-04' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-04' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-04' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'D8' and ECHELON = '1' group by DAYVAL) A) AA

						
						/**** P1 1-6梯計算****/
						select @ap1101222 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-22' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-22' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-22' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'P1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1101231 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-31' AND ISNULL(B.SPRCODE,'') <> '')A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-31' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-31' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'P1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110109 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-09' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-09' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-09' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'P1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110118 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-18' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-18' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-18' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'P1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110127 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-27' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-27' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-27' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'P1' and ECHELON = '1' group by DAYVAL) A) AA

						select @ap1110205 = convert(varchar,roomCnt) + '('+ convert(varchar,sprCnt) + ')' from (
						select roomCnt, isnull(roomCnt,'0') - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '1' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-05' AND ISNULL(B.SPRCODE,'') <> '') A1) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '2' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-05' AND ISNULL(B.SPRCODE,'') <> '') A2) - (select isnull(ucnt,'0') 
						from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where A.SECTION = '3' AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-05' AND ISNULL(B.SPRCODE,'') <> '') A3) as sprCnt
						from (select sum(convert(int, room)) as roomCnt from FLYPAYROOMS 
						where DAYCODE = 'P1' and ECHELON = '1' group by DAYVAL) A) AA
						
					end
					else if @unit_id = 'A' or @unit_id = 'B' or @unit_id = 'C'
					begin
						/*各區每日上限*/
						/****D1 1-6梯次計算****/
						select @ap1101214 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-14' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D1' and ECHELON = '1') A ) AA

						select @ap1101223 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID)  as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-23' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D1' and ECHELON = '1') A ) AA

						select @ap1110101 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-01' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D1' and ECHELON = '1') A ) AA

						select @ap1110110 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-10' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D1' and ECHELON = '1') A ) AA

						select @ap1110119 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-19' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D1' and ECHELON = '1') A ) AA

						select @ap1110128 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-28' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D1' and ECHELON = '1') A ) AA

						/****D2 1-6梯次計算****/
						select @ap1101215 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-15' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D2' and ECHELON = '1') A ) AA

						select @ap1101224 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-24' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D2' and ECHELON = '1') A ) AA

						select @ap1110102 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-02' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D2' and ECHELON = '1') A ) AA

						select @ap1110111 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-11' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D2' and ECHELON = '1') A ) AA

						select @ap1110120 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-20' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D2' and ECHELON = '1') A ) AA

						select @ap1110129 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-29' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D2' and ECHELON = '1') A ) AA

						/****D3 1-6梯次計算****/
						select @ap1101216 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-16' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D3' and ECHELON = '1') A ) AA
						
						select @ap1101225 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-25' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D3' and ECHELON = '1') A ) AA
						
						select @ap1110103 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-03' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D3' and ECHELON = '1') A ) AA
						
						select @ap1110112 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-12' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D3' and ECHELON = '1') A ) AA
						
						select @ap1110121 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-21' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D3' and ECHELON = '1') A ) AA
						
						select @ap1110130 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-30' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D3' and ECHELON = '1') A ) AA
						
						/****D4 1-6梯次計算****/
						select @ap1101217 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-17' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D4' and ECHELON = '1') A ) AA
						
						select @ap1101226 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-26' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D4' and ECHELON = '1') A ) AA
						
						select @ap1110104 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-04' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D4' and ECHELON = '1') A ) AA
						
						select @ap1110113 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-13' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D4' and ECHELON = '1') A ) AA
						
						select @ap1110122 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-22' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D4' and ECHELON = '1') A ) AA
						
						select @ap1110131 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-31' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D4' and ECHELON = '1') A ) AA
						
						/****D5 1-6梯次計算****/
						select @ap1101218 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-18' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D5' and ECHELON = '1') A ) AA
						
						select @ap1101227 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-27' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D5' and ECHELON = '1') A ) AA
						
						select @ap1110105 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-05' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D5' and ECHELON = '1') A ) AA
						
						select @ap1110114 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-14' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D5' and ECHELON = '1') A ) AA
						
						select @ap1110123 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-23' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D5' and ECHELON = '1') A ) AA
						
						select @ap1110201 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-01' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D5' and ECHELON = '1') A ) AA
						
						/****D6 1-6梯次計算****/
						select @ap1101219 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-19' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D6' and ECHELON = '1') A ) AA
						
						select @ap1101228 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-28' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D6' and ECHELON = '1') A ) AA
						
						select @ap1110106 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-06' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D6' and ECHELON = '1') A ) AA
						
						select @ap1110115 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-15' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D6' and ECHELON = '1') A ) AA
						
						select @ap1110124 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-24' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D6' and ECHELON = '1') A ) AA
						
						select @ap1110202 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-02' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D6' and ECHELON = '1') A ) AA
						
						/****D7 1-6梯次計算****/
						select @ap1101220 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-20' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D7' and ECHELON = '1') A ) AA
						
						select @ap1101229 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-29' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D7' and ECHELON = '1') A ) AA
						
						select @ap1110107 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-07' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D7' and ECHELON = '1') A ) AA
						
						select @ap1110116 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-16' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D7' and ECHELON = '1') A ) AA
						
						select @ap1110125 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-25' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D7' and ECHELON = '1') A ) AA
						
						select @ap1110203 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-03' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D7' and ECHELON = '1') A ) AA
					
						/****D8 1-6梯次計算****/
						select @ap1101221 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-21' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D8' and ECHELON = '1') A ) AA
						
						select @ap1101230 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D8' and ECHELON = '1') A ) AA
						
						select @ap1110108 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-08' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D8' and ECHELON = '1') A ) AA
						
						select @ap1110117 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-17' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D8' and ECHELON = '1') A ) AA
						
						select @ap1110126 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-26' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D8' and ECHELON = '1') A ) AA
						
						select @ap1110204 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-04' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'D8' and ECHELON = '1') A ) AA

						/****P1 1-6梯次計算****/
						select @ap1101222 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-22' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'P1' and ECHELON = '1') A ) AA
						
						select @ap1101231 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-31' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'P1' and ECHELON = '1') A ) AA
						
						select @ap1110109 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-09' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'P1' and ECHELON = '1') A ) AA
						
						select @ap1110118 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-18' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'P1' and ECHELON = '1') A ) AA
						
						select @ap1110127 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-27' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'P1' and ECHELON = '1') A ) AA
						
						select @ap1110205 = convert(varchar,roomCnt) + '(' + convert(varchar,sprCnt) + ')' from (
						select roomCnt, convert(int,roomCnt) - (select isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
							from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-05' AND ISNULL(B.SPRCODE,'') <> '') A) as sprCnt
						from (select ROOM as roomCnt from FLYPAYROOMS 
						where case SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_id
						and DAYCODE = 'P1' and ECHELON = '1') A ) AA
						
					end
					else if @unit_id = '小計'
					begin
						select @ap1101214 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-14' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101215 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
					from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-15' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101216 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-16' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101217 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-17' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101218 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-18' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101219 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-19' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101220 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-20' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101221 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-21' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101222 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-22' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101223 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-23' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101224 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-24' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101225 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-25' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101226 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-26' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101227 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-27' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101228 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-28' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101229 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-29' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101230 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-30' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1101231 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2021-12-31' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110101 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-01' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110102 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-02' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110103 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-03' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110104 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-04' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110105 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-05' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110106 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
					from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-06' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110107 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-07' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110108 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-08' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110109 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-09' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110110 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-10' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110111 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-11' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110112 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-12' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110113 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-13' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110114 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-14' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110115 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-15' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110116 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-16' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110117 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-17' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110118 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-18' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110119 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-19' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110120 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-20' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110121 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-21' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110122 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-22' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110123 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-23' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110124 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-24' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110125 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-25' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110126 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-26' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110127 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-27' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110128 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-28' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110129 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-29' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110130 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-30' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110131 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-01-31' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110201 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-01' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110202 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-02' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110203 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-03' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110204 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-04' AND ISNULL(B.SPRCODE,'') <> '') A
						select @ap1110205 = isnull(ucnt,'0') from ( select COUNT(B.FLY_ID) as ucnt
						from FLYPAYBASIC B
							join FLYPAYBASICSPR A on B.FLY_ID = A.FLY_ID
							where case A.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end = @unit_secC 
							AND B.STATUS = 'Y' AND FLIGHTDATE = '2022-02-05' AND ISNULL(B.SPRCODE,'') <> '') A
					end
					else
					begin
				/*各區集檢所每日數量*/
				/*第一波*/
					select @ap1101214 = isnull(ucnt,'0') from ( select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D1' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-14'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

			        select @ap1101215= isnull(ucnt,'0') from ( select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D2' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-15'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101216= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D3' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-16'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101217= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D4' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-17'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101218= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D5' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-18'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101219= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D6' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-19'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101220= isnull(ucnt,'0') from ( select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D7' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-20'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101221= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D8' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-21'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101222= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'P1' and a.ECHELON = 1
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-22'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A
				/*第二波*/
					select @ap1101223= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D1' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-23'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

			        select @ap1101224= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D2' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-24'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101225= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D3' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-25'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101226= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D4' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-26'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101227= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D5' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-27'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101228= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D6' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-28'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101229= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D7' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-29'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101230= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D8' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-30'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1101231= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'P1' and a.ECHELON = 2
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2021-12-31'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A
				/*第三波*/
					select @ap1110101= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D1' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-01'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

			        select @ap1110102= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D2' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-02'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110103= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D3' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-03'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110104= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D4' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-04'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110105= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D5' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-05'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110106= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D6' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-06'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110107= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D7' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-07'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110108= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D8' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-08'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110109= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'P1' and a.ECHELON = 3
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-09'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A
				/*第四波*/
					select @ap1110110= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D1' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-10'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

			        select @ap1110111= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D2' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-11'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110112= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D3' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-12'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110113= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D4' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-13'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110114= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D5' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-14'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110115= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D6' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-15'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110116= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D7' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-16'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110117= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D8' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-17'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110118= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'P1' and a.ECHELON = 4
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-18'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A
				/*第五波*/
					select @ap1110119= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D1' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-19'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

			        select @ap1110120= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D2' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-20'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110121= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D3' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-21'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110122= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D4' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-22'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110123= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D5' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-23'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110124= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D6' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-24'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110125= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D7' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-25'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110126= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D8' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-26'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110127= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'P1' and a.ECHELON = 5
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-27'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A
				/*第六波*/
					select @ap1110128= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D1' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-28'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

			        select @ap1110129= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D2' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-29'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110130= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D3' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-30'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110131= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D4' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-01-31'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110201= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D5' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-02-01'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110202= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D6' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-02-02'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110203= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D7' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-02-03'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110204= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'D8' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-02-04'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					select @ap1110205= isnull(ucnt,'0') from (select SUBSTRING(B.SPRCODE,2,2) as SE_CODE, count(B.FLY_ID) as ucnt from FLYPAYBASIC B
					left join FLYPAYSE a on SUBSTRING(B.SPRCODE,2,2) = a.SE_CODE and a.DAYCODE = 'P1' and a.ECHELON = 6
					where isnull(B.sprcode,'') <> '' and B.STATUS = 'Y' and B.FLIGHTDATE = '2022-02-05'
					and SE_CODE = @unit_id group by SUBSTRING(B.SPRCODE,2,2)) A

					end

			        insert into ##resultSPR (row_index,unit_sec,unit_code, unit_name
					,ap1101214,ap1101215,ap1101216,ap1101217
					,ap1101218,ap1101219,ap1101220,ap1101221,ap1101222
					,ap1101223,ap1101224,ap1101225,ap1101226,ap1101227
					,ap1101228,ap1101229,ap1101230,ap1101231,ap1110101
					,ap1110102,ap1110103,ap1110104,ap1110105,ap1110106
					,ap1110107,ap1110108,ap1110109,ap1110110,ap1110111
					,ap1110112,ap1110113,ap1110114,ap1110115,ap1110116
					,ap1110117,ap1110118,ap1110119,ap1110120,ap1110121
					,ap1110122,ap1110123,ap1110124,ap1110125,ap1110126
					,ap1110127,ap1110128,ap1110129,ap1110130,ap1110131
					,ap1110201,ap1110202,ap1110203,ap1110204,ap1110205)
			        values(@ord, @unit_sec,@unit_code,@unit_name
					,isnull(@ap1101214,''),isnull(@ap1101215,''),isnull(@ap1101216,''),isnull(@ap1101217,'')
					,isnull(@ap1101218,''),isnull(@ap1101219,''),isnull(@ap1101220,''),isnull(@ap1101221,''),isnull(@ap1101222,'')
					,isnull(@ap1101223,''),isnull(@ap1101224,''),isnull(@ap1101225,''),isnull(@ap1101226,''),isnull(@ap1101227,'')
					,isnull(@ap1101228,''),isnull(@ap1101229,''),isnull(@ap1101230,''),isnull(@ap1101231,''),isnull(@ap1110101,'')
					,isnull(@ap1110102,''),isnull(@ap1110103,''),isnull(@ap1110104,''),isnull(@ap1110105,''),isnull(@ap1110106,'')
					,isnull(@ap1110107,''),isnull(@ap1110108,''),isnull(@ap1110109,''),isnull(@ap1110110,''),isnull(@ap1110111,'')
					,isnull(@ap1110112,''),isnull(@ap1110113,''),isnull(@ap1110114,''),isnull(@ap1110115,''),isnull(@ap1110116,'')
					,isnull(@ap1110117,''),isnull(@ap1110118,''),isnull(@ap1110119,''),isnull(@ap1110120,''),isnull(@ap1110121,'')
					,isnull(@ap1110122,''),isnull(@ap1110123,''),isnull(@ap1110124,''),isnull(@ap1110125,''),isnull(@ap1110126,'')
					,isnull(@ap1110127,''),isnull(@ap1110128,''),isnull(@ap1110129,''),isnull(@ap1110130,''),isnull(@ap1110131,'')
					,isnull(@ap1110201,''),isnull(@ap1110202,''),isnull(@ap1110203,''),isnull(@ap1110204,''),isnull(@ap1110205,'')
				          )
			        set @row_num=@row_num+1

			        fetch next from unit_cursor into @ord,@unit_id, @unit_name,@unit_secC  

		        end
		        close unit_cursor;  
		        deallocate unit_cursor; 
        end 

        select unit_sec,unit_code, unit_name
					,ap1101214,ap1101215,ap1101216,ap1101217
					,ap1101218,ap1101219,ap1101220,ap1101221,ap1101222
					,ap1101223,ap1101224,ap1101225,ap1101226,ap1101227
					,ap1101228,ap1101229,ap1101230,ap1101231,ap1110101
					,ap1110102,ap1110103,ap1110104,ap1110105,ap1110106
					,ap1110107,ap1110108,ap1110109,ap1110110,ap1110111
					,ap1110112,ap1110113,ap1110114,ap1110115,ap1110116
					,ap1110117,ap1110118,ap1110119,ap1110120,ap1110121
					,ap1110122,ap1110123,ap1110124,ap1110125,ap1110126
					,ap1110127,ap1110128,ap1110129,ap1110130,ap1110131
					,ap1110201,ap1110202,ap1110203,ap1110204,ap1110205
        from ##resultSPR order by row_index

        drop table ##resultSPR 
");

                #endregion

                SqlCommand com = new SqlCommand(sb.ToString(), conn);
                resultTable.Load(com.ExecuteReader());
                // sql 欄位名稱
                string s_col_c1 = "unit_sec,unit_code,unit_name,ap1101214,ap1101215,ap1101216,ap1101217,ap1101218,ap1101219,ap1101220,ap1101221";
                s_col_c1 += ",ap1101222,ap1101223,ap1101224,ap1101225,ap1101226,ap1101227,ap1101228,ap1101229,ap1101230,ap1101231";
                s_col_c1 += ",ap1110101,ap1110102,ap1110103,ap1110104,ap1110105,ap1110106,ap1110107,ap1110108,ap1110109,ap1110110";
                s_col_c1 += ",ap1110111,ap1110112,ap1110113,ap1110114,ap1110115,ap1110116,ap1110117,ap1110118,ap1110119,ap1110120";
                s_col_c1 += ",ap1110121,ap1110122,ap1110123,ap1110124,ap1110125,ap1110126,ap1110127,ap1110128,ap1110129,ap1110130";
                s_col_c1 += ",ap1110131,ap1110201,ap1110202,ap1110203,ap1110204,ap1110205";
                string[] sa_col_c1 = s_col_c1.Split(',');
                // excel顯示文字
                string s_col_c2 = "區域,代碼,集檢所,110/12/14,110/12/15,110/12/16,110/12/17,110/12/18,110/12/19,110/12/20,110/12/21";
                s_col_c2 += ",110/12/22,110/12/23,110/12/24,110/12/25,110/12/26,110/12/27,110/12/28,110/12/29,110/12/30,110/12/31";
                s_col_c2 += ",111/01/01,111/01/02,111/01/03,111/01/04,111/01/05,111/01/06,111/01/07,111/01/08,111/01/09,111/01/10";
                s_col_c2 += ",111/01/11,111/01/12,111/01/13,111/01/14,111/01/15,111/01/16,111/01/17,111/01/18,111/01/19,111/01/20";
                s_col_c2 += ",111/01/21,111/01/22,111/01/23,111/01/24,111/01/25,111/01/26,111/01/27,111/01/28,111/01/29,111/01/30";
                s_col_c2 += ",111/01/31,111/02/01,111/02/02,111/02/03,111/02/04,111/02/05";
                string[] sa_col_c2 = s_col_c2.Split(',');
                //SqlDataAdapter sda = new SqlDataAdapter(dbc);
                //sda.Fill(dataSet, "report");
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
        /// <summary>
        /// EXCEL 匯入
        /// </summary>
        /// <param name="list"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int InsertFile(List<Map> list, string updateAccount)
        {
            int i_count = 0;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string selectSQL1 = @"SELECT 'X' FROM FLYPAY WHERE 1=1
    AND FLIGHTNO=@FLIGHTNO AND MAINDOCNO=@MAINDOCNO AND FLIGHTDATE=@FLIGHTDATE";

            string insertSQL1 = @"
    INSERT INTO [dbo].[FLYPAY]
           ([FIRSTNAME]           ,[MIDDLENAME]           ,[LASTNAME]           ,[BIRTH]
           ,[GENDER]           ,[MAINDOCNO]           ,[NATIONALITY]           ,[FLIGHTDATE]
           ,[FLIGHTNO]           ,[DEPARTAIRPORT]           ,[PASSENGERTYPE]           ,[DEL_MK]
           ,[DEL_TIME]           ,[DEL_FUN_CD]           ,[DEL_ACC]           ,[UPD_TIME]
           ,[UPD_FUN_CD]           ,[UPD_ACC]           ,[ADD_TIME]           ,[ADD_FUN_CD]
           ,[ADD_ACC]           ,[PAYDATE]           ,[PAYMONEY]           ,[PAYRESULT]
           ,[PLACE]           ,[ISPAY]           ,[STATUS]           ,[PAYRETURN]
            ,[BANKTYPE]        ,[RC])
     VALUES
           (@FIRSTNAME            ,@MIDDLENAME           ,@LASTNAME           ,@BIRTH
           ,@GENDER           ,@MAINDOCNO           ,@NATIONALITY           ,@FLIGHTDATE
           ,@FLIGHTNO           ,@DEPARTAIRPORT           ,@PASSENGERTYPE           ,@DEL_MK
           ,NULL        ,@DEL_FUN_CD           ,@DEL_ACC           ,GETDATE()
           ,@UPD_FUN_CD           ,@UPD_ACC           ,GETDATE()         ,@ADD_FUN_CD
           ,@ADD_ACC           ,@PAYDATE           ,@PAYMONEY           ,@PAYRESULT
           ,@PLACE           ,@ISPAY           ,@STATUS           ,@PAYRETURN
            ,@BANKTYPE      ,@RC) ";

            try
            {
                // ImportModel model = new ImportModel(); 
                foreach (Map item in list)
                {
                    if (string.IsNullOrEmpty(item.GetString("MainDocNo"))) { continue; }
                    if (string.IsNullOrEmpty(item.GetString("FlightDate"))) { continue; }
                    if (string.IsNullOrEmpty(item.GetString("FlightNo"))) { continue; }

                    args.Clear();
                    args.Add("MAINDOCNO", item.GetString("MainDocNo"));
                    args.Add("FLIGHTDATE", item.GetString("FlightDate"));
                    args.Add("FLIGHTNO", item.GetString("FlightNo"));
                    List<Dictionary<string, object>> listdt1 = GetList(selectSQL1, args);
                    if (listdt1 != null && listdt1.Count > 0) { continue; }

                    logger.Debug("FLIGHTNO: " + item.GetString("FLIGHTNO"));
                    args.Clear();
                    args.Add("FIRSTNAME", item.GetString("FirstName"));
                    args.Add("MIDDLENAME", item.GetString("MiddleName"));
                    args.Add("LASTNAME", item.GetString("LastName"));
                    args.Add("BIRTH", item.GetString("Birth"));
                    args.Add("GENDER", item.GetString("Gender"));
                    args.Add("MAINDOCNO", item.GetString("MainDocNo"));
                    args.Add("NATIONALITY", item.GetString("Nationality"));
                    args.Add("FLIGHTDATE", item.GetString("FlightDate"));
                    args.Add("FLIGHTNO", item.GetString("FlightNo"));
                    args.Add("DEPARTAIRPORT", item.GetString("DepartAirport"));
                    args.Add("PASSENGERTYPE", item.GetString("PassengerType"));

                    args.Add("DEL_MK", "N");
                    //args.Add("DEL_TIME", null);
                    args.Add("DEL_FUN_CD", null);
                    args.Add("DEL_ACC", null);
                    //args.Add("UPD_TIME", DateTime.Now);
                    args.Add("UPD_FUN_CD", "ADM-FLYPAY");
                    args.Add("UPD_ACC", updateAccount);
                    //args.Add("ADD_TIME", DateTime.Now);
                    args.Add("ADD_FUN_CD", "ADM-FLYPAY");
                    args.Add("ADD_ACC", updateAccount);
                    args.Add("PLACE", null);
                    args.Add("PAYDATE", null);
                    args.Add("PAYMONEY", null);
                    args.Add("PAYRESULT", null);
                    args.Add("PAYPLACE", null);
                    args.Add("ISPAY", "N");
                    args.Add("STATUS", "N");
                    args.Add("PAYRETURN", null);
                    args.Add("BANKTYPE", null);
                    args.Add("RC", null);
                    i_count += Update(insertSQL1, args);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }

            return i_count;
        }

        /// <summary>
        /// EXCEL 匯入
        /// </summary>
        /// <param name="list"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public Dictionary<string, object> InsertFileBackmin(List<Map> list, string updateAccount)
        {
            int i_count = 0; //更新筆數
            int i_errcnt = 0; //錯誤筆數
            Dictionary<string, object> rst = new Dictionary<string, object>();

            Dictionary<string, object> args = new Dictionary<string, object>();
            Dictionary<string, object> args2 = new Dictionary<string, object>();

            string updateSQL1 = @"
    UPDATE FLYPAY 
    SET PLACE = @PLACE 
    ,ISPAY = @ISPAY 
    ,UPD_TIME = GETDATE()
    ,UPD_FUN_CD = @UPD_FUN_CD 
    ,UPD_ACC = @UPD_ACC
    WHERE FLIGHTDATE = @FLIGHTDATE AND FLIGHTNO = @FLIGHTNO AND MAINDOCNO = @MAINDOCNO";

            string updateSQL2 = @"
    UPDATE FLYPAYBASIC
    SET PLACE = @PLACE 
    ,UPD_TIME = GETDATE()
    ,UPD_FUN_CD = @UPD_FUN_CD 
    ,UPD_ACC = @UPD_ACC
    WHERE FLY_ID = @FLY_ID AND ADD_ACC = @ADD_ACC";

            int i_row = 0;
            // ImportModel model = new ImportModel(); 
            foreach (Map item in list)
            {
                i_row++;
                if (!string.IsNullOrWhiteSpace(item.GetString("資料行")) && !string.IsNullOrWhiteSpace(item.GetString("驗證碼")))
                {
                    // 防疫旅館專案
                    string s_log2 = "";
                    s_log2 += string.Format("i_row:{0} \t", i_row);
                    s_log2 += ($"抵達日期:{item.GetString("FlightDate")}\t MainDocNo:{item.GetString("MainDocNo")}\t");
                    s_log2 += ($"防疫所別:{item.GetString("防疫所別")}\t資料行:{item.GetString("資料行")}\tupdateAccount:{updateAccount}\t");
                    logger.Debug(s_log2);

                    // 略過表頭
                    if (item.GetString("MainDocNo") == "MainDocNo") { continue; }

                    args2.Clear();
                    args2.Add("PLACE", item.GetString("防疫所別"));
                    args2.Add("UPD_FUN_CD", "ADM-FLYPAY");
                    args2.Add("UPD_ACC", updateAccount);

                    args2.Add("FLY_ID", item.GetString("資料行"));
                    args2.Add("ADD_ACC", item.GetString("驗證碼"));
                }
                else
                {
                    // 菲律賓專案
                    string s_log1 = "";
                    s_log1 += string.Format("i_row:{0} \t", i_row);
                    //s_log1 += ($"FlightDate:{item.GetString("FlightDate")}\tFlightNo:{item.GetString("FlightNo")}\tMainDocNo:{item.GetString("MainDocNo")}\t");
                    s_log1 += ($"FlightDate:{item.GetString("FlightDate")}\t MainDocNo:{item.GetString("MainDocNo")}\t");
                    s_log1 += ($"防疫所別:{item.GetString("防疫所別")}\t為應繳費人員:{item.GetString("為應繳費人員")}\tupdateAccount:{updateAccount}\t");
                    logger.Debug(s_log1);

                    // 略過表頭
                    if (item.GetString("MainDocNo") == "MainDocNo") { continue; }

                    args.Clear();
                    //args.Add("UPD_TIME", DateTime.Now);
                    args.Add("PLACE", item.GetString("防疫所別"));
                    args.Add("ISPAY", item.GetString("為應繳費人員"));
                    args.Add("UPD_FUN_CD", "ADM-FLYPAY");
                    args.Add("UPD_ACC", updateAccount);

                    args.Add("FLIGHTDATE", item.GetString("FlightDate"));
                    args.Add("FLIGHTNO", item.GetString("FlightNo"));
                    args.Add("MAINDOCNO", item.GetString("MainDocNo"));
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(item.GetString("資料行")) && !string.IsNullOrWhiteSpace(item.GetString("驗證碼")))
                    {
                        i_count += Update(updateSQL2, args2);
                    }
                    else
                    {
                        i_count += Update(updateSQL1, args);
                    }
                }
                catch (Exception ex)
                {
                    i_errcnt += 1;
                    logger.Error(ex.Message, ex);
                }
            }

            rst.Clear();
            rst.Add("count", i_count);
            rst.Add("errcnt", i_errcnt);
            return rst;
        }
        /// <summary>
        /// 繳費狀態下拉式選單
        /// </summary>
        /// <returns></returns>
        public List<Map> GetPAY_STATUS()
        {
            List<Map> li = new List<Map>();
            Map mapY = new Map();
            mapY.Add("QRY_PAY_STATUS", "Y");
            mapY.Add("QRY_PAY_STATUS_NAME", "已繳費");
            li.Add(mapY);
            Map mapN = new Map();
            mapN.Add("QRY_PAY_STATUS", "N");
            mapN.Add("QRY_PAY_STATUS_NAME", "未繳費");
            li.Add(mapN);
            return li;
        }

        /// <summary>
        /// 銀行別下拉式選單
        /// </summary>
        /// <returns></returns>
        public List<Map> GetBANKTYPE()
        {
            List<Map> li = new List<Map>();
            Map map1 = new Map();
            map1.Add("QRY_BANKTYPE", "1");
            map1.Add("QRY_BANKTYPE_NAME", "中信");
            li.Add(map1);
            Map map2 = new Map();
            map2.Add("QRY_BANKTYPE", "2");
            map2.Add("QRY_BANKTYPE_NAME", "玉山");
            li.Add(map2);
            return li;
        }

        /// <summary>
        /// 地區(春節專案)下拉式選單
        /// </summary>
        /// <returns></returns>
        public List<Map> GetSECTION()
        {
            List<Map> li = new List<Map>();
            Map map1 = new Map();
            map1.Add("QRY_SECTION", "1");
            map1.Add("QRY_SECTION_NAME", "北");
            li.Add(map1);
            Map map2 = new Map();
            map2.Add("QRY_SECTION", "2");
            map2.Add("QRY_SECTION_NAME", "中");
            li.Add(map2);
            Map map3 = new Map();
            map3.Add("QRY_SECTION", "3");
            map3.Add("QRY_SECTION_NAME", "南");
            li.Add(map3);
            return li;
        }
        /// <summary>
        /// 是否需要退款狀態下拉式選單
        /// </summary>
        /// <returns></returns>
        public List<Map> GetNEEDBACK()
        {
            List<Map> li = new List<Map>();
            Map mapY = new Map();
            mapY.Add("QRY_NEED_BACK", "Y");
            mapY.Add("QRY_NEED_BACK_NAME", "是");
            li.Add(mapY);
            Map mapN = new Map();
            mapN.Add("QRY_NEED_BACK", "N");
            mapN.Add("QRY_NEED_BACK_NAME", "否");
            li.Add(mapN);
            return li;
        }
        /// <summary>
        /// 航班資料查詢 菲律賓專案
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<FlySearchGridModel> GetFlyPayList(FlyPayModel form)
        {
            List<FlySearchGridModel> result = new List<FlySearchGridModel>();

            try
            {
                StringBuilder sb = new StringBuilder();

                string s_select_1 = @" 
    a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[GENDER]
    ,a.[MAINDOCNO],a.[NATIONALITY],a.[FLIGHTDATE],a.[FLIGHTNO],a.[DEPARTAIRPORT],a.[PASSENGERTYPE]
    ,a.[PAYDATE],a.[PAYMONEY],a.[PAYRESULT],a.[PLACE],a.[ISPAY],a.[STATUS],a.[XID]
    ,b.TRACENO,b.[GRANTDATE]
    ,case a.[BANKTYPE] WHEN '2' THEN '玉山' ELSE '中信' END AS BANKTYPE_TEXT";
                string s_group_1 = @" 
    a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH],a.[GENDER]
    ,a.[MAINDOCNO],a.[NATIONALITY],a.[FLIGHTDATE],a.[FLIGHTNO],a.[DEPARTAIRPORT],a.[PASSENGERTYPE]
    ,a.[PAYDATE],a.[PAYMONEY],a.[PAYRESULT],a.[PLACE],a.[ISPAY],a.[STATUS],a.[XID]
    ,b.TRACENO,b.[GRANTDATE] ,a.[BANKTYPE] ";

                bool flag_use_leftjoin = true;
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { flag_use_leftjoin = false; }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { flag_use_leftjoin = false; }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { flag_use_leftjoin = false; }

                sb.Append(@" SELECT " + s_select_1);
                sb.Append(@" FROM FLYPAY a");
                if (flag_use_leftjoin) { sb.Append(@" LEFT"); }
                sb.Append(@" JOIN FLYSWIPE b on b.TRACENO=a.TRACENO AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE) AND b.DEL_MK = 'N'");
                sb.Append(@" WHERE a.DEL_MK = 'N'");

                //航班號碼
                if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { sb.Append(" and a.FLIGHTNO = @FLIGHTNO "); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { sb.Append(" and a.MAINDOCNO LIKE '%' + @MAINDOCNO  + '%' "); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { sb.Append(" and a.PAYRESULT = @PAYRESULT"); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { sb.Append(" and a.STATUS = @STATUS"); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue && form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue)
                {
                    sb.Append(" and replace(FLIGHTDATE,'-','') BETWEEN @FLIGHTDATE AND @FLIGHTDATE_END ");
                }

                //防疫旅館
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { sb.Append(" and a.PLACE LIKE '%' + @PLACE + '%'"); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { sb.Append(" and a.TRACENO= @TRACENO"); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue && form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue)
                {
                    sb.Append(" and  convert(varchar, b.GRANTDATE, 23)  between @GRANTDATE1 and @GRANTDATE2 ");
                }

                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue && form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue)
                {
                    sb.Append(" and convert(varchar, a.ADD_TIME, 23) between @ADDTIMES and @ADDTIMEE ");
                }

                sb.Append(@" GROUP BY " + s_group_1);
                sb.Append(@" ORDER BY a.FLIGHTDATE, a.FLIGHTNO, a.FIRSTNAME");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);

                //航班號碼
                if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { DataUtils.AddParameters(com, "FLIGHTNO", form.QRY_FLIGHTNO); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { DataUtils.AddParameters(com, "MAINDOCNO", form.QRY_MAINDOCNO); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { DataUtils.AddParameters(com, "PAYRESULT", form.QRY_PAYRESULT); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { DataUtils.AddParameters(com, "STATUS", form.QRY_STATUS); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE", form.QRY_FLIGHTDATE.Value.ToString("yyyyMMdd")); }
                if (form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE_END", form.QRY_FLIGHTDATE_END.Value.ToString("yyyyMMdd")); }
                //防疫所別
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { DataUtils.AddParameters(com, "PLACE", form.QRY_PLACE); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { DataUtils.AddParameters(com, "TRACENO", form.QRY_TRACENO); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { DataUtils.AddParameters(com, "GRANTDATE1", form.QRY_GRANTDATE1.Value.ToString("yyyy-MM-dd")); }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { DataUtils.AddParameters(com, "GRANTDATE2", form.QRY_GRANTDATE2.Value.ToString("yyyy-MM-dd")); }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue) { DataUtils.AddParameters(com, "ADDTIMES", form.QRY_ADDTIMES.Value.ToString("yyyy-MM-dd")); }
                //新增日期迄
                if (form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue) { DataUtils.AddParameters(com, "ADDTIMEE", form.QRY_ADDTIMEE.Value.ToString("yyyy-MM-dd")); }

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        FlySearchGridModel cqm = new FlySearchGridModel();
                        cqm.BIRTH = Convert.ToString(sr["BIRTH"]);
                        cqm.DEPARTAIRPORT = Convert.ToString(sr["DEPARTAIRPORT"]);
                        cqm.FIRSTNAME = Convert.ToString(sr["FIRSTNAME"]);
                        cqm.FLIGHTNO = Convert.ToString(sr["FLIGHTNO"]);
                        cqm.FLIGHTDATE = Convert.ToString(sr["FLIGHTDATE"]);
                        cqm.GENDER = Convert.ToString(sr["GENDER"]);
                        cqm.ISPAY = Convert.ToString(sr["ISPAY"]);
                        cqm.LASTNAME = Convert.ToString(sr["LASTNAME"]);
                        cqm.MAINDOCNO = Convert.ToString(sr["MAINDOCNO"]);
                        cqm.MIDDLENAME = Convert.ToString(sr["MIDDLENAME"]);
                        cqm.NATIONALITY = Convert.ToString(sr["NATIONALITY"]);
                        cqm.PASSENGERTYPE = Convert.ToString(sr["PASSENGERTYPE"]);
                        cqm.PAYDATE = string.IsNullOrEmpty(Convert.ToString(sr["PAYDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["PAYDATE"]);
                        cqm.PAYMONEY = Convert.ToString(sr["PAYMONEY"]);
                        cqm.PAYRESULT = Convert.ToString(sr["PAYRESULT"]);
                        cqm.PLACE = Convert.ToString(sr["PLACE"]);
                        cqm.XID = Convert.ToString(sr["XID"]);
                        cqm.STATUS = Convert.ToString(sr["STATUS"]);
                        cqm.TRACENO = Convert.ToString(sr["TRACENO"]);
                        cqm.GRANTDATE = string.IsNullOrEmpty(Convert.ToString(sr["GRANTDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["GRANTDATE"]);
                        cqm.BANKTYPE_TEXT = Convert.ToString(sr["BANKTYPE_TEXT"]);
                        result.Add(cqm);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                //String a = ex.Message;
                logger.Warn(ex.Message, ex);
                //throw ex;
            }

            return result == null ? new List<FlySearchGridModel>() : result;
        }
        /// <summary>
        /// 航班資料查詢 防疫旅館專案
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<FlySearchGridModel> GetFlyPayBasicList(FlyPayModel form)
        {
            List<FlySearchGridModel> result = new List<FlySearchGridModel>();

            try
            {
                StringBuilder sb = new StringBuilder();

                string s_group_1 = @" 
     a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH]
    ,a.[MAINDOCNO],a.[FLIGHTDATE],a.[PLACE]
    ,a.[PAYDATE],a.[PAYMONEY],a.[PAYRESULT],a.[STATUS],a.[XID]
    ,b.[GRANTDATE],a.[FLY_ID],a.[GUID],a.[SPRCODE],spr.[PLEVEL] ";
                string s_group_2 = @"
    ,CASE WHEN isnull(b.TRACENO_QID,'') <> '' THEN b.TRACENO_QID +' (銀聯)' ELSE b.TRACENO END AS TRACENO
    ,CASE a.[BANKTYPE] WHEN '2' THEN '玉山' ELSE '中信' END AS BANKTYPE_TEXT                                              
    , ISNULL(cd.[name],'') AS FLYCONTRY
    ,isnull(concat(cd2.[name],(SELECT ',' + cast(flycountry.[name] AS VARCHAR) from flypaybasiccon
	    left join flycountry on CONCAT(flycountry.code1,'-',flycountry.code2,'-',flycountry.code3) = flypaybasiccon.[COUNTRY_ID]
	    where flypaybasiccon.fly_id = a.fly_id
	    FOR XML PATH(''))),'') AS FLYCONTRYTR
    ,CASE spr.[SECTION] WHEN '1' THEN '北' WHEN '2' THEN '中' WHEN '3' THEN '南' END + '-' + isnull(de.se_val,'') AS SECTION_TEXT";
                string s_group_3 = @",cd.[name],cd2.[name],b.[TRACENO],b.[TRACENO_QID],a.[BANKTYPE],a.[SPRCODE],spr.[SECTION],de.se_val ";
                bool flag_use_leftjoin = true;
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { flag_use_leftjoin = false; }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { flag_use_leftjoin = false; }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { flag_use_leftjoin = false; }

                sb.Append(@" SELECT " + s_group_1 + s_group_2);
                sb.Append(@" FROM FLYPAYBASIC a");
                sb.Append(@" left join flycountry cd on CONCAT(cd.code1,'-',cd.code2,'-',cd.code3) = a.flycontry ");
                sb.Append(@" left join flycountry cd2 on CONCAT(cd2.code1,'-',cd2.code2,'-',cd2.code3) = a.flycontrytr ");
                sb.Append(@" left join flypaybasicspr spr on spr.fly_id = a.fly_id ");
                sb.Append(@" left join flypayrooms_de de on de.se_code = substring(a.sprcode,2,2) ");
                if (flag_use_leftjoin) { sb.Append(@" LEFT"); }
                sb.Append(@" JOIN FLYSWIPE b on (b.TRACENO=a.TRACENO or b.TRACENO_QID = a.TRACENO) AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE) AND b.DEL_MK = 'N'");
                sb.Append(@" WHERE a.DEL_MK = 'N' and isnull(a.flytype,'0') <> '11' ");

                //航班號碼
                //if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { sb.Append(" and a.FLIGHTNO = @FLIGHTNO "); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { sb.Append(" and a.MAINDOCNO LIKE '%' + @MAINDOCNO  + '%' "); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { sb.Append(" and a.PAYRESULT = @PAYRESULT"); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { sb.Append(" and a.STATUS = @STATUS"); }
                //是否需要退費狀態
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { sb.Append(" and a.NEEDBACK = @NEEDBACK"); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue && form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue)
                {
                    sb.Append(" and replace(FLIGHTDATE,'-','') BETWEEN @FLIGHTDATE AND @FLIGHTDATE_END ");
                }

                //防疫旅館
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { sb.Append(" and a.PLACE LIKE '%' + @PLACE + '%'"); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { sb.Append(" and a.TRACENO= @TRACENO"); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue && form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue)
                {
                    sb.Append(" and  convert(varchar, b.GRANTDATE, 23)  between @GRANTDATE1 and @GRANTDATE2 ");
                }

                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue && form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue)
                {
                    sb.Append(" and convert(varchar, a.ADD_TIME, 23) between @ADDTIMES and @ADDTIMEE ");
                }

                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { sb.Append(" and cd.NAME LIKE '%' + @FLYCONTRY + '%'"); }
                //銀行別
                if (!String.IsNullOrEmpty(form.QRY_BANKTYPE))
                {
                    if (form.QRY_BANKTYPE == "2") { sb.Append(" and a.BANKTYPE = @BANKTYPE"); } //玉山
                    else { sb.Append(" and (a.BANKTYPE IS NULL or a.BANKTYPE <> '2')"); } //中信
                }
                //春節專案流水號
                if (!String.IsNullOrEmpty(form.QRY_SPRCODE)) { sb.Append(" and a.SPRCODE LIKE '%' + @SPRCODE  + '%' "); }
                //訂房識別碼
                if (!String.IsNullOrEmpty(form.QRY_GUID)) { sb.Append(" and a.GUID = @GUID "); }
                //地區(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_SECTION)) { sb.Append(" and spr.SECTION = @SECTION"); }
                //價錢(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_PLEVEL)) { sb.Append(" and spr.PLEVEL LIKE '%' + @PLEVEL  + '%' "); }

                sb.Append(@" GROUP BY " + s_group_1 + s_group_3);
                sb.Append(@" ORDER BY a.FLIGHTDATE, a.FIRSTNAME");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);

                //航班號碼
                //if (!String.IsNullOrEmpty(form.QRY_FLIGHTNO)) { DataUtils.AddParameters(com, "FLIGHTNO", form.QRY_FLIGHTNO); }
                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { DataUtils.AddParameters(com, "MAINDOCNO", form.QRY_MAINDOCNO); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { DataUtils.AddParameters(com, "PAYRESULT", form.QRY_PAYRESULT); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { DataUtils.AddParameters(com, "STATUS", form.QRY_STATUS); }
                //是否需要退費狀態
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { DataUtils.AddParameters(com, "NEEDBACK", form.QRY_NEEDBACK); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE", form.QRY_FLIGHTDATE.Value.ToString("yyyyMMdd")); }
                if (form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE_END", form.QRY_FLIGHTDATE_END.Value.ToString("yyyyMMdd")); }

                //防疫所別
                if (!String.IsNullOrEmpty(form.QRY_PLACE)) { DataUtils.AddParameters(com, "PLACE", form.QRY_PLACE); }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { DataUtils.AddParameters(com, "TRACENO", form.QRY_TRACENO); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { DataUtils.AddParameters(com, "GRANTDATE1", form.QRY_GRANTDATE1.Value.ToString("yyyy-MM-dd")); }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { DataUtils.AddParameters(com, "GRANTDATE2", form.QRY_GRANTDATE2.Value.ToString("yyyy-MM-dd")); }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue) { DataUtils.AddParameters(com, "ADDTIMES", form.QRY_ADDTIMES.Value.ToString("yyyy-MM-dd")); }
                //新增日期迄
                if (form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue) { DataUtils.AddParameters(com, "ADDTIMEE", form.QRY_ADDTIMEE.Value.ToString("yyyy-MM-dd")); }

                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { DataUtils.AddParameters(com, "FLYCONTRY", form.QRY_FLYCONTRY); }
                //銀行別
                if (!String.IsNullOrEmpty(form.QRY_BANKTYPE)) { DataUtils.AddParameters(com, "BANKTYPE", form.QRY_BANKTYPE); }
                //春節專案流水號
                if (!String.IsNullOrEmpty(form.QRY_SPRCODE)) { DataUtils.AddParameters(com, "SPRCODE", form.QRY_SPRCODE); }
                //訂房識別碼
                if (!String.IsNullOrEmpty(form.QRY_GUID)) { DataUtils.AddParameters(com, "GUID", form.QRY_GUID); }
                //地區(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_SECTION)) { DataUtils.AddParameters(com, "SECTION", form.QRY_SECTION); }
                //價錢(春節專案)
                if (!String.IsNullOrEmpty(form.QRY_PLEVEL)) { DataUtils.AddParameters(com, "PLEVEL", form.QRY_PLEVEL); }

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        FlySearchGridModel cqm = new FlySearchGridModel();
                        cqm.FLY_ID = Convert.ToString(sr["FLY_ID"]);
                        cqm.GUID = Convert.ToString(sr["GUID"]);
                        cqm.PLACE = Convert.ToString(sr["PLACE"]);
                        cqm.BIRTH = Convert.ToString(sr["BIRTH"]);
                        cqm.FIRSTNAME = Convert.ToString(sr["FIRSTNAME"]);
                        cqm.FLIGHTDATE = Convert.ToString(sr["FLIGHTDATE"]);
                        cqm.LASTNAME = Convert.ToString(sr["LASTNAME"]);
                        cqm.MAINDOCNO = Convert.ToString(sr["MAINDOCNO"]);
                        cqm.MIDDLENAME = Convert.ToString(sr["MIDDLENAME"]);
                        cqm.PAYDATE = string.IsNullOrEmpty(Convert.ToString(sr["PAYDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["PAYDATE"]);
                        cqm.PAYMONEY = Convert.ToString(sr["PAYMONEY"]);
                        cqm.PAYRESULT = Convert.ToString(sr["PAYRESULT"]);
                        cqm.XID = Convert.ToString(sr["XID"]);
                        cqm.STATUS = Convert.ToString(sr["STATUS"]);
                        cqm.TRACENO = Convert.ToString(sr["TRACENO"]);
                        cqm.GRANTDATE = string.IsNullOrEmpty(Convert.ToString(sr["GRANTDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["GRANTDATE"]);
                        cqm.FLYCONTRY = Convert.ToString(sr["FLYCONTRY"]);
                        cqm.FLYCONTRYTR = Convert.ToString(sr["FLYCONTRYTR"]);
                        cqm.BANKTYPE_TEXT = Convert.ToString(sr["BANKTYPE_TEXT"]);
                        cqm.SPRCODE = Convert.ToString(sr["SPRCODE"]);
                        cqm.SECTION_TEXT = Convert.ToString(sr["SECTION_TEXT"]);
                        cqm.PLEVEL = Convert.ToString(sr["PLEVEL"]);

                        result.Add(cqm);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                //String a = ex.Message;
                logger.Warn(ex.Message, ex);
                //throw ex;
            }

            return result == null ? new List<FlySearchGridModel>() : result;
        }
        /// <summary>
        /// 航班資料查詢 菲律賓專案
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ExportModel GetFlyPayDetail(string MAINDOCNO, string FLIGHTNO, string FLIGHTDATE)
        {
            ExportModel result = new ExportModel();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
    SELECT a.FIRSTNAME,a.MIDDLENAME,a.LASTNAME
    ,a.BIRTH,a.GENDER
    ,a.MAINDOCNO  ,a.NATIONALITY
    ,a.FLIGHTDATE ,a.FLIGHTNO  
    ,a.DEPARTAIRPORT,a.PASSENGERTYPE
    ,a.PAYDATE,a.PAYMONEY,a.PAYRESULT
    ,a.PLACE,a.ISPAY,a.STATUS
    ,a.PAYRETURN,a.XID,a.TRACENO,a.QID
    ,b.GRANTDATE
    FROM FLYPAY a
    LEFT JOIN FLYSWIPE b on (b.TRACENO=a.TRACENO OR b.TRACENO_QID = a.TRACENO) AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE)
    WHERE a.DEL_MK = 'N' 
    AND a.MAINDOCNO = @MAINDOCNO AND a.FLIGHTNO = @FLIGHTNO AND a.FLIGHTDATE = @FLIGHTDATE
                ");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);

                DataUtils.AddParameters(com, "FLIGHTNO", FLIGHTNO);
                DataUtils.AddParameters(com, "MAINDOCNO", MAINDOCNO);
                DataUtils.AddParameters(com, "FLIGHTDATE", FLIGHTDATE);

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        result = new ExportModel();

                        result.BIRTH = Convert.ToString(sr["BIRTH"]);
                        result.DEPARTAIRPORT = Convert.ToString(sr["DEPARTAIRPORT"]);
                        result.FIRSTNAME = Convert.ToString(sr["FIRSTNAME"]);
                        result.FLIGHTNO = Convert.ToString(sr["FLIGHTNO"]);
                        result.FLIGHTDATE = Convert.ToString(sr["FLIGHTDATE"]);
                        result.GENDER = Convert.ToString(sr["GENDER"]);
                        result.ISPAY = Convert.ToString(sr["ISPAY"]);
                        result.LASTNAME = Convert.ToString(sr["LASTNAME"]);
                        result.MAINDOCNO = Convert.ToString(sr["MAINDOCNO"]);
                        result.MIDDLENAME = Convert.ToString(sr["MIDDLENAME"]);
                        result.NATIONALITY = Convert.ToString(sr["NATIONALITY"]);
                        result.PASSENGERTYPE = Convert.ToString(sr["PASSENGERTYPE"]);
                        result.PAYDATE = string.IsNullOrEmpty(Convert.ToString(sr["PAYDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["PAYDATE"]);
                        result.PAYMONEY = Convert.ToString(sr["PAYMONEY"]);
                        result.PAYRESULT = Convert.ToString(sr["PAYRESULT"]);
                        result.PLACE = Convert.ToString(sr["PLACE"]);
                        result.PAYRETURN = Convert.ToString(sr["PAYRETURN"]);
                        result.STATUS = Convert.ToString(sr["STATUS"]);
                        result.XID = Convert.ToString(sr["XID"]);
                        result.TRACENO = Convert.ToString(sr["TRACENO"]);
                        result.QID = Convert.ToString(sr["QID"]);
                        result.GRANTDATE = string.IsNullOrEmpty(Convert.ToString(sr["GRANTDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["GRANTDATE"]);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }

            return result;
        }
        /// <summary>
        /// 航班資料查詢 防疫旅館專案
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ExportModel GetFlyPayDetail(string FLY_ID)
        {
            ExportModel result = new ExportModel();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
   	  SELECT a.FIRSTNAME,a.MIDDLENAME,a.LASTNAME,a.ADD_TIME
    ,a.BIRTH
    ,a.MAINDOCNO
    ,a.FLIGHTDATE
    ,a.PAYDATE,a.PAYMONEY,a.PAYRESULT
    ,a.STATUS,a.PLACE
    ,a.PAYRETURN,a.XID
    ,CASE WHEN isnull(b.TRACENO_QID,'') <> '' THEN b.TRACENO_QID+' (銀聯)' ELSE b.TRACENO END AS TRACENO
    ,a.QID
    ,b.GRANTDATE
    ,a.GUID,a.FLY_ID,a.ADD_ACC AS VCODE
    ,spr.SECTION, spr.PLEVEL, a.SPRCODE
	, ISNULL(cd.[name],'') AS FLYCONTRY
    ,isnull(concat(cd2.[name],',',(SELECT cast(flycountry.[name] AS VARCHAR ) + ','  from flypaybasiccon
	left join flycountry on CONCAT(flycountry.code1,'-',flycountry.code2,'-',flycountry.code3) = flypaybasiccon.[COUNTRY_ID]
	where flypaybasiccon.fly_id = a.fly_id	--把name一樣的加起來
	FOR XML PATH(''))),'') AS FLYCONTRYTR
    FROM FLYPAYBASIC a
    LEFT JOIN FLYSWIPE b on (b.TRACENO=a.TRACENO OR b.TRACENO_QID = a.TRACENO) AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE)
    left join flycountry cd on CONCAT(cd.code1,'-',cd.code2,'-',cd.code3) = a.flycontry
    left join flycountry cd2 on CONCAT(cd2.code1,'-',cd2.code2,'-',cd2.code3) = a.flycontrytr
    left join flypaybasicspr spr on spr.fly_id = a.fly_id
    WHERE a.DEL_MK = 'N' 
    AND a.FLY_ID = @FLY_ID
                ");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);

                DataUtils.AddParameters(com, "FLY_ID", FLY_ID);

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        result = new ExportModel();
                        result.FLY_ID = Convert.ToString(sr["FLY_ID"]);
                        result.GUID = Convert.ToString(sr["GUID"]);
                        result.ADD_ACC = Convert.ToString(sr["VCODE"]);
                        result.BIRTH = Convert.ToString(sr["BIRTH"]);
                        result.FIRSTNAME = Convert.ToString(sr["FIRSTNAME"]);
                        result.FLIGHTDATE = Convert.ToString(sr["FLIGHTDATE"]);
                        result.LASTNAME = Convert.ToString(sr["LASTNAME"]);
                        result.MAINDOCNO = Convert.ToString(sr["MAINDOCNO"]);
                        result.MIDDLENAME = Convert.ToString(sr["MIDDLENAME"]);
                        result.PAYDATE = string.IsNullOrEmpty(Convert.ToString(sr["PAYDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["PAYDATE"]);
                        result.PAYMONEY = Convert.ToString(sr["PAYMONEY"]);
                        result.PAYRESULT = Convert.ToString(sr["PAYRESULT"]);
                        result.PAYRETURN = Convert.ToString(sr["PAYRETURN"]);
                        result.PLACE = Convert.ToString(sr["PLACE"]);
                        result.STATUS = Convert.ToString(sr["STATUS"]);
                        result.XID = Convert.ToString(sr["XID"]);
                        result.TRACENO = Convert.ToString(sr["TRACENO"]);
                        result.QID = Convert.ToString(sr["QID"]);
                        result.GRANTDATE = string.IsNullOrEmpty(Convert.ToString(sr["GRANTDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["GRANTDATE"]);
                        result.FLYCONTRYTR = Convert.ToString(sr["FLYCONTRYTR"]);
                        result.FLYCONTRY = Convert.ToString(sr["FLYCONTRY"]);
                        result.SECTION = Convert.ToString(sr["SECTION"]);
                        result.PLEVEL = Convert.ToString(sr["PLEVEL"]);
                        result.SPRCODE = Convert.ToString(sr["SPRCODE"]);
                        result.ADD_TIME = string.IsNullOrEmpty(Convert.ToString(sr["ADD_TIME"])) ? (DateTime?)null : Convert.ToDateTime(sr["ADD_TIME"]);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }

            return result;
        }

        /// <summary>
        /// 航班資料查詢 簡易專案
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ExportModel GetFlyPayDetailSIM(string FLY_ID)
        {
            ExportModel result = new ExportModel();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
   	  SELECT a.FIRSTNAME,a.MIDDLENAME,a.LASTNAME,a.ADD_TIME
    ,a.BIRTH
    ,a.MAINDOCNO
    ,a.FLIGHTDATE
    ,a.PAYDATE,a.PAYMONEY,a.PAYRESULT
    ,a.STATUS,a.PLACE
    ,a.PAYRETURN,a.XID
    ,CASE WHEN isnull(b.TRACENO_QID,'') <> '' THEN b.TRACENO_QID+' (銀聯)' ELSE b.TRACENO END AS TRACENO
    ,a.QID
    ,b.GRANTDATE
    ,a.GUID,a.FLY_ID,a.ADD_ACC AS VCODE
	, ISNULL(cd.[name],'') AS FLYCONTRY
    ,isnull(concat(cd2.[name],',',(SELECT cast(flycountry.[name] AS VARCHAR ) + ','  from flypaybasiccon
	left join flycountry on CONCAT(flycountry.code1,'-',flycountry.code2,'-',flycountry.code3) = flypaybasiccon.[COUNTRY_ID]
	where flypaybasiccon.fly_id = a.fly_id	--把name一樣的加起來
	FOR XML PATH(''))),'') AS FLYCONTRYTR
    ,a.[FIRSTNAME2],a.[MIDDLENAME2],a.[LASTNAME2],a.[BIRTH2],a.[MAINDOCNO2] 
    ,a.[FIRSTNAME3],a.[MIDDLENAME3],a.[LASTNAME3],a.[BIRTH3],a.[MAINDOCNO3]
    ,a.[FIRSTNAME4],a.[MIDDLENAME4],a.[LASTNAME4],a.[BIRTH4],a.[MAINDOCNO4]
    FROM FLYPAYBASIC a
    LEFT JOIN FLYSWIPE b on (b.TRACENO=a.TRACENO OR b.TRACENO_QID = a.TRACENO) AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE)
    left join flycountry cd on CONCAT(cd.code1,'-',cd.code2,'-',cd.code3) = a.flycontry
    left join flycountry cd2 on CONCAT(cd2.code1,'-',cd2.code2,'-',cd2.code3) = a.flycontrytr
    WHERE a.DEL_MK = 'N' 
    AND a.FLY_ID = @FLY_ID
                ");
                SqlCommand com = new SqlCommand(sb.ToString(), conn);
                DataUtils.AddParameters(com, "FLY_ID", FLY_ID);
                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        result = new ExportModel();
                        result.FLY_ID = Convert.ToString(sr["FLY_ID"]);
                        result.GUID = Convert.ToString(sr["GUID"]);
                        result.ADD_ACC = Convert.ToString(sr["VCODE"]);
                        result.BIRTH = Convert.ToString(sr["BIRTH"]);
                        result.FIRSTNAME = Convert.ToString(sr["FIRSTNAME"]);
                        result.FLIGHTDATE = Convert.ToString(sr["FLIGHTDATE"]);
                        result.LASTNAME = Convert.ToString(sr["LASTNAME"]);
                        result.MAINDOCNO = Convert.ToString(sr["MAINDOCNO"]);
                        result.MIDDLENAME = Convert.ToString(sr["MIDDLENAME"]);
                        result.PAYDATE = string.IsNullOrEmpty(Convert.ToString(sr["PAYDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["PAYDATE"]);
                        result.PAYMONEY = Convert.ToString(sr["PAYMONEY"]);
                        result.PAYRESULT = Convert.ToString(sr["PAYRESULT"]);
                        result.PAYRETURN = Convert.ToString(sr["PAYRETURN"]);
                        result.PLACE = Convert.ToString(sr["PLACE"]);
                        result.STATUS = Convert.ToString(sr["STATUS"]);
                        result.XID = Convert.ToString(sr["XID"]);
                        result.TRACENO = Convert.ToString(sr["TRACENO"]);
                        result.QID = Convert.ToString(sr["QID"]);
                        result.GRANTDATE = string.IsNullOrEmpty(Convert.ToString(sr["GRANTDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["GRANTDATE"]);
                        result.FLYCONTRYTR = Convert.ToString(sr["FLYCONTRYTR"]);
                        result.FLYCONTRY = Convert.ToString(sr["FLYCONTRY"]);
                        result.ADD_TIME = string.IsNullOrEmpty(Convert.ToString(sr["ADD_TIME"])) ? (DateTime?)null : Convert.ToDateTime(sr["ADD_TIME"]);
                        result.BIRTH2 = Convert.ToString(sr["BIRTH2"]);
                        result.FIRSTNAME2 = Convert.ToString(sr["FIRSTNAME2"]);
                        result.LASTNAME2 = Convert.ToString(sr["LASTNAME2"]);
                        result.MAINDOCNO2 = Convert.ToString(sr["MAINDOCNO2"]);
                        result.MIDDLENAME2 = Convert.ToString(sr["MIDDLENAME2"]);
                        result.BIRTH3 = Convert.ToString(sr["BIRTH3"]);
                        result.FIRSTNAME3 = Convert.ToString(sr["FIRSTNAME3"]);
                        result.LASTNAME3 = Convert.ToString(sr["LASTNAME3"]);
                        result.MAINDOCNO3 = Convert.ToString(sr["MAINDOCNO3"]);
                        result.MIDDLENAME3 = Convert.ToString(sr["MIDDLENAME3"]);
                        result.BIRTH4 = Convert.ToString(sr["BIRTH4"]);
                        result.FIRSTNAME4 = Convert.ToString(sr["FIRSTNAME4"]);
                        result.LASTNAME4 = Convert.ToString(sr["LASTNAME4"]);
                        result.MAINDOCNO4 = Convert.ToString(sr["MAINDOCNO4"]);
                        result.MIDDLENAME4 = Convert.ToString(sr["MIDDLENAME4"]);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }

            return result;
        }

        /// <summary>
        /// EXCEL 匯入 菲律賓專案
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int UpdateDetail(FlyPayModel model, string updateAccount)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            int count = 0;
            string updateSQL1 = @"
                UPDATE FLYPAY 
                SET ISPAY = @ISPAY, PLACE = @PLACE
                , UPD_TIME = GETDATE()
                , UPD_FUN_CD = @UPD_FUN_CD
                , UPD_ACC = @UPD_ACC 
                WHERE FLIGHTNO = @FLIGHTNO AND MAINDOCNO = @MAINDOCNO AND FLIGHTDATE = @FLIGHTDATE ";

            try
            {
                logger.Debug("FLIGHTNO: " + model.detail.FLIGHTNO + "/" + model.detail.MAINDOCNO);

                args.Clear();
                args.Add("MAINDOCNO", model.detail.MAINDOCNO);
                args.Add("FLIGHTDATE", model.detail.FLIGHTDATE);
                args.Add("FLIGHTNO", model.detail.FLIGHTNO);
                //args.Add("UPD_TIME", DateTime.Now);
                args.Add("UPD_FUN_CD", "ADM-FLYPAY");
                args.Add("UPD_ACC", updateAccount);
                args.Add("ISPAY", model.detail.ISPAY);
                args.Add("PLACE", model.detail.PLACE);
                count += Update(updateSQL1, args);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }

            if (model.QRY_CanEdit)
            {
                string updateSQL2 = @"
    UPDATE FLYPAY 
    SET PAYDATE = @PAYDATE,PAYMONEY = @PAYMONEY
    ,STATUS = @STATUS,TRACENO = @TRACENO
    WHERE FLIGHTNO = @FLIGHTNO AND MAINDOCNO = @MAINDOCNO AND FLIGHTDATE = @FLIGHTDATE ";
                try
                {
                    args.Clear();
                    args.Add("PAYDATE", model.detail.PAYDATE);
                    args.Add("PAYMONEY", model.detail.PAYMONEY);
                    args.Add("STATUS", model.detail.STATUS);
                    args.Add("TRACENO", model.detail.TRACENO);

                    args.Add("FLIGHTNO", model.detail.FLIGHTNO);
                    args.Add("MAINDOCNO", model.detail.MAINDOCNO);
                    args.Add("FLIGHTDATE", model.detail.FLIGHTDATE);
                    count += Update(updateSQL2, args);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                }
            }

            return count;
        }

        /// <summary>
        /// 確認是否已匯入過該航班資料
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public string CheckFlyPayFile(string remoteFileName)
        {
            string result = "";

            StringBuilder sb = new StringBuilder();
            sb.Append(@"
                    SELECT FILE_NAME
                    FROM FLYPAY_FILE WHERE 1=1 and FILE_NAME = @FILE_NAME
                ");
            SqlCommand com = new SqlCommand(sb.ToString(), conn);

            //檔案名稱
            DataUtils.AddParameters(com, "FILE_NAME", remoteFileName);
            using (SqlDataReader sr = com.ExecuteReader(CommandBehavior.CloseConnection))
            {
                while (sr.Read())
                {
                    result = Convert.ToString(sr["FILE_NAME"]);
                }
                sr.Close();
            }
            return result;
        }

        /// <summary>
        /// Sftp排程下載檔案
        /// </summary>
        /// <param name="list"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public string InsertFlyPayFile(string remoteFileName)
        {
            int count = 0;
            Dictionary<string, object> args = new Dictionary<string, object>();

            string updateSQL1 = @"
    INSERT INTO [dbo].[FLYPAY_FILE] ([FILE_NAME],[FILE_URL],[ADD_TIME],[STATUS])    
    VALUES (@FILE_NAME,@FILE_URL,GETDATE(),@STATUS) ";
            logger.Debug("FILE_NAME: " + remoteFileName);
            var fileurl = DataUtils.GetConfig("SCHEDULE_MOHW_DOWNLOAD_PATH") + remoteFileName;

            args.Clear();
            args.Add("FILE_NAME", remoteFileName);
            args.Add("FILE_URL", fileurl);
            args.Add("STATUS", "Y");
            //args.Add("ADD_TIME", DateTime.Now);
            count += Update(updateSQL1, args);

            return fileurl;
        }

        /// <summary>
        /// 航班資料查詢(確認是否為既有資料)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<Map> GetInsertDataList(List<Map> list)
        {
            List<Map> result = new List<Map>();

            Dictionary<string, object> args = new Dictionary<string, object>();

            string selectSQL1 = @"
    SELECT * FROM FLYPAY 
    WHERE 1=1 
    AND FIRSTNAME = @FIRSTNAME 
    AND MIDDLENAME = @MIDDLENAME
    AND LASTNAME = @LASTNAME 
    AND BIRTH = @BIRTH 
    AND GENDER = @GENDER 
    AND MAINDOCNO = @MAINDOCNO 
    AND NATIONALITY = @NATIONALITY 
    AND FLIGHTDATE = @FLIGHTDATE 
    AND FLIGHTNO = @FLIGHTNO 
    AND DEPARTAIRPORT = @DEPARTAIRPORT 
    AND PASSENGERTYPE = @PASSENGERTYPE";
            try
            {
                foreach (Map item in list)
                {
                    // 略過表頭
                    if (item.GetString("MainDocNo") == "MainDocNo") { continue; }

                    args.Clear();
                    args.Add("FIRSTNAME", item.GetString("FirstName"));
                    args.Add("MIDDLENAME", item.GetString("MiddleName"));
                    args.Add("LASTNAME", item.GetString("LastName"));
                    args.Add("BIRTH", item.GetString("Birth"));
                    args.Add("GENDER", item.GetString("Gender"));
                    args.Add("MAINDOCNO", item.GetString("MainDocNo"));
                    args.Add("NATIONALITY", item.GetString("Nationality"));
                    args.Add("FLIGHTDATE", item.GetString("FlightDate"));
                    args.Add("FLIGHTNO", item.GetString("FlightNo"));
                    args.Add("DEPARTAIRPORT", item.GetString("DepartAirport"));
                    args.Add("PASSENGERTYPE", item.GetString("PassengerType"));
                    var data = GetData(selectSQL1, args);
                    if (data == null) { result.Add(item); }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            return result;

        }

        /// <summary>
        /// 編輯 防疫旅館專案 防疫所別
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int UpdateFlyPayBasicDetail(FlyPayModel model, string updateAccount)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            int count = 0;
            string updateSQL1 = @"
                UPDATE FLYPAYBASIC
                SET FIRSTNAME = @FIRSTNAME
                , MIDDLENAME = @MIDDLENAME
                , LASTNAME = @LASTNAME
                , BIRTH = @BIRTH
                , MAINDOCNO = @MAINDOCNO
                , FLIGHTDATE = @FLIGHTDATE
                , PLACE = @PLACE
                , UPD_TIME = GETDATE()
                , UPD_FUN_CD = @UPD_FUN_CD
                , UPD_ACC = @UPD_ACC 
                , SPRCODE = @SPRCODE
                WHERE FLY_ID = @FLYID ";
            try
            {
                logger.Debug("FLYID: " + model.detail.FLY_ID + "/MAINDOCNO: " + model.detail.MAINDOCNO + "/FIRSTNAME: " + model.detail.FIRSTNAME + "/MIDDLENAME: " + model.detail.MIDDLENAME + "/LASTNAME: " + model.detail.LASTNAME + "/BIRTH: " + model.detail.BIRTH + "/FLIGHTDATE: " + model.detail.FLIGHTDATE);

                args.Clear();
                args.Add("FIRSTNAME", model.detail.FIRSTNAME);
                args.Add("MIDDLENAME", model.detail.MIDDLENAME);
                args.Add("LASTNAME", model.detail.LASTNAME);
                args.Add("BIRTH", model.detail.BIRTH);
                args.Add("MAINDOCNO", model.detail.MAINDOCNO);
                args.Add("FLIGHTDATE", model.detail.FLIGHTDATE);
                args.Add("PLACE", model.detail.PLACE);
                args.Add("FLYID", model.detail.FLY_ID);
                args.Add("UPD_FUN_CD", "ADM-FLYPAY");
                args.Add("UPD_ACC", updateAccount);
                args.Add("SPRCODE", model.detail.SPRCODE);
                count += Update(updateSQL1, args);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }
            return count;
        }

        /// <summary>
        /// 編輯 簡易專案
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int UpdateFlyPayBasicDetailSIM(FlyPayModel model, string updateAccount)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            int count = 0;
            string updateSQL1 = @"
                UPDATE FLYPAYBASIC
                SET FIRSTNAME = @FIRSTNAME
                , MIDDLENAME = @MIDDLENAME
                , LASTNAME = @LASTNAME
                , BIRTH = @BIRTH
                , MAINDOCNO = @MAINDOCNO
                , FLIGHTDATE = @FLIGHTDATE
                , UPD_TIME = GETDATE()
                , UPD_FUN_CD = @UPD_FUN_CD
                , UPD_ACC = @UPD_ACC 
,FIRSTNAME2 = @FIRSTNAME2
                , MIDDLENAME2 = @MIDDLENAME2
                , LASTNAME2 = @LASTNAME2
                , BIRTH2 = @BIRTH2
                , MAINDOCNO2 = @MAINDOCNO2
,FIRSTNAME3 = @FIRSTNAME3
                , MIDDLENAME3 = @MIDDLENAME3
                , LASTNAME3 = @LASTNAME3
                , BIRTH3 = @BIRTH3
                , MAINDOCNO3 = @MAINDOCNO3
,FIRSTNAME4 = @FIRSTNAME4
                , MIDDLENAME4 = @MIDDLENAME4
                , LASTNAME4 = @LASTNAME4
                , BIRTH4 = @BIRTH4
                , MAINDOCNO4 = @MAINDOCNO4
                WHERE FLY_ID = @FLYID ";
            try
            {
                logger.Debug("FLYID: " + model.detail.FLY_ID + "/MAINDOCNO: " + model.detail.MAINDOCNO + "/FIRSTNAME: " + model.detail.FIRSTNAME + "/MIDDLENAME: " + model.detail.MIDDLENAME + "/LASTNAME: " + model.detail.LASTNAME + "/BIRTH: " + model.detail.BIRTH + "/FLIGHTDATE: " + model.detail.FLIGHTDATE);
                logger.Debug("FLYID: " + model.detail.FLY_ID + "/MAINDOCNO2: " + model.detail.MAINDOCNO2 + "/FIRSTNAME2: " + model.detail.FIRSTNAME2 + "/MIDDLENAME2: " + model.detail.MIDDLENAME2 + "/LASTNAME2: " + model.detail.LASTNAME2 + "/BIRTH2: " + model.detail.BIRTH2 + "/FLIGHTDATE: " + model.detail.FLIGHTDATE);
                logger.Debug("FLYID: " + model.detail.FLY_ID + "/MAINDOCNO3: " + model.detail.MAINDOCNO3 + "/FIRSTNAME3: " + model.detail.FIRSTNAME3 + "/MIDDLENAME3: " + model.detail.MIDDLENAME3 + "/LASTNAME3: " + model.detail.LASTNAME3 + "/BIRTH: " + model.detail.BIRTH3 + "/FLIGHTDATE: " + model.detail.FLIGHTDATE);
                logger.Debug("FLYID: " + model.detail.FLY_ID + "/MAINDOCNO4: " + model.detail.MAINDOCNO4 + "/FIRSTNAME4: " + model.detail.FIRSTNAME4 + "/MIDDLENAME4: " + model.detail.MIDDLENAME4 + "/LASTNAME4: " + model.detail.LASTNAME4 + "/BIRTH: " + model.detail.BIRTH4 + "/FLIGHTDATE: " + model.detail.FLIGHTDATE);
                args.Clear();
                args.Add("FIRSTNAME", model.detail.FIRSTNAME);
                args.Add("MIDDLENAME", model.detail.MIDDLENAME);
                args.Add("LASTNAME", model.detail.LASTNAME);
                args.Add("BIRTH", model.detail.BIRTH);
                args.Add("MAINDOCNO", model.detail.MAINDOCNO);
                args.Add("FLIGHTDATE", model.detail.FLIGHTDATE);
                args.Add("FLYID", model.detail.FLY_ID);
                args.Add("UPD_FUN_CD", "ADM-FLYPAY");
                args.Add("UPD_ACC", updateAccount);
                args.Add("FIRSTNAME2", model.detail.FIRSTNAME2);
                args.Add("MIDDLENAME2", model.detail.MIDDLENAME2);
                args.Add("LASTNAME2", model.detail.LASTNAME2);
                args.Add("BIRTH2", model.detail.BIRTH2);
                args.Add("MAINDOCNO2", model.detail.MAINDOCNO2);
                args.Add("FIRSTNAME3", model.detail.FIRSTNAME3);
                args.Add("MIDDLENAME3", model.detail.MIDDLENAME3);
                args.Add("LASTNAME3", model.detail.LASTNAME3);
                args.Add("BIRTH3", model.detail.BIRTH3);
                args.Add("MAINDOCNO3", model.detail.MAINDOCNO3);
                args.Add("FIRSTNAME4", model.detail.FIRSTNAME4);
                args.Add("MIDDLENAME4", model.detail.MIDDLENAME4);
                args.Add("LASTNAME4", model.detail.LASTNAME4);
                args.Add("BIRTH4", model.detail.BIRTH4);
                args.Add("MAINDOCNO4", model.detail.MAINDOCNO4);
                count += Update(updateSQL1, args);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }
            return count;
        }
        /// <summary>
        /// 清空 防疫旅館專案 GUID
        /// </summary>
        /// <param name="model"></param>
        /// <param name="updateAccount"></param>
        /// <returns></returns>
        public int ClearFlyPayBasicGUID(FlyPayModel model, string updateAccount)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            int count = 0;
            string updateSQL1 = @"
                UPDATE FLYPAYBASIC
                SET GUID = ''
                , STATUS = 'N'
                , TRACENO = ''
                , PAYDATE = ''
                , UPD_TIME = GETDATE()
                , UPD_ACC = @UPD_ACC
                WHERE FLY_ID = @FLYID ";
            try
            {
                logger.Debug("ClearFlyPayBasicGUID:FLYID: " + model.detail.FLY_ID + "/MAINDOCNO: " + model.detail.MAINDOCNO + "/FIRSTNAME: " + model.detail.FIRSTNAME + "/MIDDLENAME: " + model.detail.MIDDLENAME + "/LASTNAME: " + model.detail.LASTNAME + "/BIRTH: " + model.detail.BIRTH + "/FLIGHTDATE: " + model.detail.FLIGHTDATE);

                args.Clear();
                args.Add("FLYID", model.detail.FLY_ID);
                args.Add("UPD_ACC", updateAccount);
                count += Update(updateSQL1, args);
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }
            return count;
        }

        /// <summary>
        /// 檢測1/12-1/26 僅1500名額可申請 依據繳費成功的名額 
        /// </summary>
        /// <returns></returns>
        public int CountPayBasic()
        {
            StringBuilder querySQL = new StringBuilder(@"
    SELECT COUNT(MAINDOCNO) AS CNT FROM FLYPAYBASIC 
    WHERE 1=1 AND FLIGHTDATE BETWEEN '20210112' AND '20210126' AND STATUS='Y'");

            SqlCommand com = new SqlCommand("", conn);
            com.CommandText = querySQL.ToString();
            var cnt = 0;
            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    cnt = Convert.ToInt32(sr["CNT"]);
                }
                sr.Close();
            }
            return cnt;
        }

        /// <summary>
        /// 檢測1/12-1/26 僅1500名額可申請 依據繳費成功的名額 
        /// </summary>
        /// <returns></returns>
        public int CountPayBasic0630()
        {
            StringBuilder querySQL = new StringBuilder(@"
    SELECT COUNT(FLY_ID) AS CNT FROM FLYPAYBASIC 
    WHERE 1=1 AND replace(flightdate,'-','') BETWEEN '20211214' AND '20220205' AND STATUS='Y' AND ISNULL(GUID,'') <> '' ");

            SqlCommand com = new SqlCommand("", conn);
            com.CommandText = querySQL.ToString();
            var cnt = 0;
            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    cnt = Convert.ToInt32(sr["CNT"]);
                }
                sr.Close();
            }
            return cnt;
        }

        /// <summary>
        /// 取得玉山未繳費資料
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<string> GetEsunPayData()
        {
            List<string> result = new List<string>();

            StringBuilder sb = new StringBuilder();
            sb.Append(@"
                    SELECT PAYRESULT
                    FROM FLYPAYBASIC WHERE 1=1 and BANKTYPE = @BANKTYPE AND STATUS = 'N'
                ");
            SqlCommand com = new SqlCommand(sb.ToString(), conn);

            //檔案名稱
            DataUtils.AddParameters(com, "BANKTYPE", "2");
            using (SqlDataReader sr = com.ExecuteReader(CommandBehavior.CloseConnection))
            {
                if (sr.HasRows)
                {
                    while (sr.Read())
                    {
                        var payresult = sr.GetString(sr.GetOrdinal("PAYRESULT"));
                        result.Add(payresult);
                    }
                }
                sr.Close();
            }
            return result;
        }

        /// <summary>
        /// 防疫旅館春節專案查詢
        /// </summary>
        /// <returns></returns>
        public List<ES.Models.FlyPaySPRViewModel> QuerySPRNumIsNull()
        {
            var list = new List<ES.Models.FlyPaySPRViewModel>();

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                    SELECT A.SPRCODE,A.FLY_ID,B.ISUSE, A.STATUS, A.FLIGHTDATE, C.CD_ID, C.ECHELON, 
                    case C.SECTION when '1' then 'A' when '2' then 'B' when '3' then 'C' end as SECTION, C.DAYCODE, C.ROOM, C.DAYVAL FROM FLYPAYBASIC A
                    JOIN FLYPAYBASICSPR B ON B.FLY_ID = A.FLY_ID
                    JOIN FLYPAYROOMS C ON REPLACE(C.DAYVAL,'-','') = REPLACE(A.FLIGHTDATE,'-','') AND C.SECTION = B.SECTION
                    WHERE B.ISUSE = @ISUSE AND A.STATUS = @STATUS AND ISNULL(A.SPRCODE,'') =''
                ");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);

                DataUtils.AddParameters(com, "ISUSE", "Y");
                DataUtils.AddParameters(com, "STATUS", "Y");

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        var result = new ES.Models.FlyPaySPRViewModel();
                        result.SPRCODE = Convert.ToString(sr["SPRCODE"]);
                        result.CD_ID = Convert.ToInt32(sr["CD_ID"]);
                        result.DAYCODE = Convert.ToString(sr["DAYCODE"]);
                        result.DAYVAL = Convert.ToString(sr["DAYVAL"]);
                        result.ECHELON = Convert.ToString(sr["ECHELON"]);
                        result.FLIGHTDATE = Convert.ToString(sr["FLIGHTDATE"]);
                        result.FLY_ID = Convert.ToInt32(sr["FLY_ID"]);
                        result.ISUSE = Convert.ToString(sr["ISUSE"]);
                        result.ROOM = Convert.ToString(sr["ROOM"]);
                        result.SECTION = Convert.ToString(sr["SECTION"]);
                        result.STATUS = Convert.ToString(sr["STATUS"]);
                        list.Add(result);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"QuerySPRNumIsNull:{ex.Message}", ex);
            }
            return list;
        }
        /// <summary>
        /// 航班資料查詢 防疫旅館專案
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<FlySearchGridModel> GetFlyPaySimList(FlyPayModel form)
        {
            List<FlySearchGridModel> result = new List<FlySearchGridModel>();
            try
            {
                StringBuilder sb = new StringBuilder();
                string s_group_1 = @" 
     a.[FIRSTNAME],a.[MIDDLENAME],a.[LASTNAME],a.[BIRTH]
    ,a.[MAINDOCNO],a.[FLIGHTDATE],a.[PLACE]
    ,a.[PAYDATE],a.[PAYMONEY],a.[PAYRESULT],a.[STATUS],a.[XID]
    ,b.[GRANTDATE],a.[FLY_ID],a.[GUID]
    ,a.[FIRSTNAME2],a.[MIDDLENAME2],a.[LASTNAME2],a.[BIRTH2],a.[MAINDOCNO2] 
    ,a.[FIRSTNAME3],a.[MIDDLENAME3],a.[LASTNAME3],a.[BIRTH3],a.[MAINDOCNO3]
    ,a.[FIRSTNAME4],a.[MIDDLENAME4],a.[LASTNAME4],a.[BIRTH4],a.[MAINDOCNO4] ";
                string s_group_2 = @"
    ,CASE WHEN isnull(b.TRACENO_QID,'') <> '' THEN b.TRACENO_QID +' (銀聯)' ELSE b.TRACENO END AS TRACENO
    ,CASE a.[BANKTYPE] WHEN '2' THEN '玉山' ELSE '中信' END AS BANKTYPE_TEXT                                              
    , ISNULL(cd.[name],'') AS FLYCONTRY
    ,isnull(concat(cd2.[name],(SELECT ',' + cast(flycountry.[name] AS VARCHAR) from flypaybasiccon
	    left join flycountry on CONCAT(flycountry.code1,'-',flycountry.code2,'-',flycountry.code3) = flypaybasiccon.[COUNTRY_ID]
	    where flypaybasiccon.fly_id = a.fly_id
	    FOR XML PATH(''))),'') AS FLYCONTRYTR ";
                string s_group_3 = @",cd.[name],cd2.[name],b.[TRACENO],b.[TRACENO_QID],a.[BANKTYPE] ";
                bool flag_use_leftjoin = true;
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { flag_use_leftjoin = false; }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { flag_use_leftjoin = false; }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { flag_use_leftjoin = false; }

                sb.Append(@" SELECT " + s_group_1 + s_group_2);
                sb.Append(@" FROM FLYPAYBASIC a ");
                sb.Append(@" left join flycountry cd on CONCAT(cd.code1,'-',cd.code2,'-',cd.code3) = a.flycontry ");
                sb.Append(@" left join flycountry cd2 on CONCAT(cd2.code1,'-',cd2.code2,'-',cd2.code3) = a.flycontrytr ");
                if (flag_use_leftjoin) { sb.Append(@" LEFT"); }
                sb.Append(@" JOIN FLYSWIPE b on (b.TRACENO=a.TRACENO or b.TRACENO_QID = a.TRACENO) AND convert(date,b.PAYDATE)=convert(date,a.PAYDATE) AND b.DEL_MK = 'N' ");
                sb.Append(@" WHERE a.DEL_MK = 'N' AND a.FLYTYPE='11' ");

                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { sb.Append(" and (a.MAINDOCNO LIKE '%' + @MAINDOCNO  + '%' or a.MAINDOCNO2 LIKE '%' + @MAINDOCNO  + '%' or a.MAINDOCNO3 LIKE '%' + @MAINDOCNO  + '%' or a.MAINDOCNO4 LIKE '%' + @MAINDOCNO  + '%') "); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { sb.Append(" and a.PAYRESULT = @PAYRESULT"); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { sb.Append(" and a.STATUS = @STATUS"); }
                //是否需要退費狀態
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { sb.Append(" and a.NEEDBACK = @NEEDBACK"); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue && form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue)
                {
                    sb.Append(" and replace(FLIGHTDATE,'-','') BETWEEN @FLIGHTDATE AND @FLIGHTDATE_END ");
                }
                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { sb.Append(" and a.TRACENO= @TRACENO"); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue && form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue)
                {
                    sb.Append(" and  convert(varchar, b.GRANTDATE, 23)  between @GRANTDATE1 and @GRANTDATE2 ");
                }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue && form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue)
                {
                    sb.Append(" and convert(varchar, a.ADD_TIME, 23) between @ADDTIMES and @ADDTIMEE ");
                }
                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { sb.Append(" and cd.NAME LIKE '%' + @FLYCONTRY + '%'"); }
                //銀行別
                if (!String.IsNullOrEmpty(form.QRY_BANKTYPE))
                {
                    if (form.QRY_BANKTYPE == "2") { sb.Append(" and a.BANKTYPE = @BANKTYPE"); } //玉山
                    else { sb.Append(" and (a.BANKTYPE IS NULL or a.BANKTYPE <> '2')"); } //中信
                }
                //識別碼
                if (!String.IsNullOrEmpty(form.QRY_GUID)) { sb.Append(" and a.GUID = @GUID "); }
                sb.Append(@" GROUP BY " + s_group_1 + s_group_3);
                sb.Append(@" ORDER BY a.FLIGHTDATE, a.FIRSTNAME");

                SqlCommand com = new SqlCommand(sb.ToString(), conn);

                //證照號碼
                if (!String.IsNullOrEmpty(form.QRY_MAINDOCNO)) { DataUtils.AddParameters(com, "MAINDOCNO", form.QRY_MAINDOCNO); }
                //繳費編號
                if (!String.IsNullOrEmpty(form.QRY_PAYRESULT)) { DataUtils.AddParameters(com, "PAYRESULT", form.QRY_PAYRESULT); }
                //繳費狀態
                if (!String.IsNullOrEmpty(form.QRY_STATUS)) { DataUtils.AddParameters(com, "STATUS", form.QRY_STATUS); }
                //是否需要退費狀態
                if (!String.IsNullOrEmpty(form.QRY_NEEDBACK)) { DataUtils.AddParameters(com, "NEEDBACK", form.QRY_NEEDBACK); }
                //航班日期
                if (form.QRY_FLIGHTDATE != null && form.QRY_FLIGHTDATE.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE", form.QRY_FLIGHTDATE.Value.ToString("yyyyMMdd")); }
                if (form.QRY_FLIGHTDATE_END != null && form.QRY_FLIGHTDATE_END.HasValue) { DataUtils.AddParameters(com, "FLIGHTDATE_END", form.QRY_FLIGHTDATE_END.Value.ToString("yyyyMMdd")); }

                //授權碼
                if (!string.IsNullOrEmpty(form.QRY_TRACENO)) { DataUtils.AddParameters(com, "TRACENO", form.QRY_TRACENO); }
                //撥款日期區間1
                if (form.QRY_GRANTDATE1 != null && form.QRY_GRANTDATE1.HasValue) { DataUtils.AddParameters(com, "GRANTDATE1", form.QRY_GRANTDATE1.Value.ToString("yyyy-MM-dd")); }
                //撥款日期區間2
                if (form.QRY_GRANTDATE2 != null && form.QRY_GRANTDATE2.HasValue) { DataUtils.AddParameters(com, "GRANTDATE2", form.QRY_GRANTDATE2.Value.ToString("yyyy-MM-dd")); }
                //新增日期起
                if (form.QRY_ADDTIMES != null && form.QRY_ADDTIMES.HasValue) { DataUtils.AddParameters(com, "ADDTIMES", form.QRY_ADDTIMES.Value.ToString("yyyy-MM-dd")); }
                //新增日期迄
                if (form.QRY_ADDTIMEE != null && form.QRY_ADDTIMEE.HasValue) { DataUtils.AddParameters(com, "ADDTIMEE", form.QRY_ADDTIMEE.Value.ToString("yyyy-MM-dd")); }
                //出境國家
                if (!String.IsNullOrEmpty(form.QRY_FLYCONTRY)) { DataUtils.AddParameters(com, "FLYCONTRY", form.QRY_FLYCONTRY); }
                //銀行別
                if (!String.IsNullOrEmpty(form.QRY_BANKTYPE)) { DataUtils.AddParameters(com, "BANKTYPE", form.QRY_BANKTYPE); }
                //識別碼
                if (!String.IsNullOrEmpty(form.QRY_GUID)) { DataUtils.AddParameters(com, "GUID", form.QRY_GUID); }

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        FlySearchGridModel cqm = new FlySearchGridModel();
                        cqm.FLY_ID = Convert.ToString(sr["FLY_ID"]);
                        cqm.GUID = Convert.ToString(sr["GUID"]);
                        cqm.PLACE = Convert.ToString(sr["PLACE"]);
                        cqm.BIRTH = Convert.ToString(sr["BIRTH"]);
                        cqm.FIRSTNAME = Convert.ToString(sr["FIRSTNAME"]);
                        cqm.FLIGHTDATE = Convert.ToString(sr["FLIGHTDATE"]);
                        cqm.LASTNAME = Convert.ToString(sr["LASTNAME"]);
                        cqm.MAINDOCNO = Convert.ToString(sr["MAINDOCNO"]);
                        cqm.MIDDLENAME = Convert.ToString(sr["MIDDLENAME"]);
                        cqm.PAYDATE = string.IsNullOrEmpty(Convert.ToString(sr["PAYDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["PAYDATE"]);
                        cqm.PAYMONEY = Convert.ToString(sr["PAYMONEY"]);
                        cqm.PAYRESULT = Convert.ToString(sr["PAYRESULT"]);
                        cqm.XID = Convert.ToString(sr["XID"]);
                        cqm.STATUS = Convert.ToString(sr["STATUS"]);
                        cqm.TRACENO = Convert.ToString(sr["TRACENO"]);
                        cqm.GRANTDATE = string.IsNullOrEmpty(Convert.ToString(sr["GRANTDATE"])) ? (DateTime?)null : Convert.ToDateTime(sr["GRANTDATE"]);
                        cqm.FLYCONTRY = Convert.ToString(sr["FLYCONTRY"]);
                        cqm.FLYCONTRYTR = Convert.ToString(sr["FLYCONTRYTR"]);
                        cqm.BANKTYPE_TEXT = Convert.ToString(sr["BANKTYPE_TEXT"]);

                        cqm.FIRSTNAME2 = Convert.ToString(sr["FIRSTNAME2"]);
                        cqm.MIDDLENAME2 = Convert.ToString(sr["MIDDLENAME2"]);
                        cqm.LASTNAME2 = Convert.ToString(sr["LASTNAME2"]);
                        cqm.MAINDOCNO2 = Convert.ToString(sr["MAINDOCNO2"]);
                        cqm.BIRTH2 = Convert.ToString(sr["BIRTH2"]);
                        cqm.FIRSTNAME3 = Convert.ToString(sr["FIRSTNAME3"]);
                        cqm.MIDDLENAME3 = Convert.ToString(sr["MIDDLENAME3"]);
                        cqm.LASTNAME3 = Convert.ToString(sr["LASTNAME3"]);
                        cqm.MAINDOCNO3 = Convert.ToString(sr["MAINDOCNO3"]);
                        cqm.BIRTH3 = Convert.ToString(sr["BIRTH3"]);
                        cqm.FIRSTNAME4 = Convert.ToString(sr["FIRSTNAME4"]);
                        cqm.MIDDLENAME4 = Convert.ToString(sr["MIDDLENAME4"]);
                        cqm.LASTNAME4 = Convert.ToString(sr["LASTNAME4"]);
                        cqm.MAINDOCNO4 = Convert.ToString(sr["MAINDOCNO4"]);
                        cqm.BIRTH4 = Convert.ToString(sr["BIRTH4"]);

                        result.Add(cqm);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
            }

            return result == null ? new List<FlySearchGridModel>() : result;
        }
    }
}