using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using log4net;
using ES.Models;
using System.Text;
using ES.Utils;

using ES.Services;
using System.Collections;
using System.Collections.Specialized;
using ES.Models.Entities;

namespace ES.Action
{
    public class FlyPayAction : BaseAction
    {
        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public FlyPayAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public FlyPayAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        /// <summary>
        /// 訂房編號檢測是否有重複
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int GetOneGuidDataList(string seq)
        {
            var result = new List<FlightModel>();
            StringBuilder querySQL = new StringBuilder(@"
    SELECT GUID
    FROM FLYPAYBASIC 
    WHERE 1=1
    AND GUID = @GUID
");


            SqlCommand com = new SqlCommand("", conn);
            DataUtils.AddParameters(com, "GUID", seq);

            com.CommandText = querySQL.ToString();
            var i = 0;
            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    i++;
                }
                sr.Close();
            }
            return i;
        }

        /// <summary>
        /// 取得所有信用卡的資料(要給資拓宏宇的)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<DictionaryEntry[]> GetAllGuidDataList(List<FlyPayBasicModel> param)
        {
            List<DictionaryEntry[]> dcts = new List<DictionaryEntry[]>();


            foreach (var item in param)
            {

                //var FlightdateAndGuid  = string.Join(",", param.Select(m => "'" + m.FLIGHTDATE + "-" + m.GUID + "'").ToList());

                var FlightdateAndGuid = "";

                if (item.FLIGHTDATE.TONotNullString() != "")
                {
                    FlightdateAndGuid += " AND replace(FLIGHTDATE,'-','') =replace('" + item.FLIGHTDATE.TONotNullString() + "','-','') ";
                }
                if (item.MAINDOCNO.TONotNullString() != "")
                {
                    FlightdateAndGuid += " AND MAINDOCNO LIKE '" + item.MAINDOCNO.TONotNullString() + "%' ";
                }
                if (item.GUID.TONotNullString() != "")
                {
                    FlightdateAndGuid += " AND GUID='" + item.GUID.TONotNullString() + "' ";
                }

                StringBuilder querySQL = new StringBuilder(@"
    SELECT TOP 1 GUID,MAINDOCNO,TRACENO,CASE WHEN ISNULL(FLYTYPE,'0') > 3 THEN '1' ELSE '0' END AS FLYTYPE
    ,ISNULL(SPRCODE,'') AS SPRCODE,replace(FLIGHTDATE,'-','') as FLIGHTDATE
    FROM FLYPAYBASIC 
    WHERE 1=1
    AND STATUS = 'Y'
    " + FlightdateAndGuid + " ORDER BY ADD_TIME DESC ");

                SqlCommand com = new SqlCommand("", conn);

                com.CommandText = querySQL.ToString();

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        ListDictionary dct = new ListDictionary();
                        dct.Add("識別碼", sr["GUID"].TONotNullString());
                        dct.Add("護照號碼", sr["MAINDOCNO"].TONotNullString());
                        dct.Add("信用卡授權碼", sr["TRACENO"].TONotNullString());
                        dct.Add("專案類別", sr["FLYTYPE"].TONotNullString());
                        dct.Add("春節專案編號", sr["SPRCODE"].TONotNullString());
                        dct.Add("抵達日期", sr["FLIGHTDATE"].TONotNullString());
                        DictionaryEntry[] result = new DictionaryEntry[6];
                        dct.CopyTo(result, 0);
                        dcts.Add(result);
                    }
                    sr.Close();
                }
            }



            return dcts;

        }

        /// <summary>
        /// 取得所有信用卡的資料(要給資拓宏宇的)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<DictionaryEntry[]> GetAllGuidDataListAll()
        {
            List<DictionaryEntry[]> dcts = new List<DictionaryEntry[]>();
            Encoding utf8 = Encoding.UTF8;


            StringBuilder querySQL = new StringBuilder(@"
	SELECT GUID,FIRSTNAME,MIDDLENAME,LASTNAME,BIRTH,MAINDOCNO,FLIGHTDATE,ADD_TIME,PAYMONEY,TRACENO,PAYDATE
        ,CASE WHEN ISNULL(FLYTYPE,'0') > 3 THEN '1' ELSE '0' END AS FLYTYPE
    ,ISNULL(SPRCODE,'') AS SPRCODE
    FROM FLYPAYBASIC 
    WHERE 1=1
    AND STATUS = 'Y'
    ");


            SqlCommand com = new SqlCommand("", conn);

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    ListDictionary dct = new ListDictionary();
                    dct.Add("成功訂房碼", utf8.GetBytes(sr["GUID"].TONotNullString()));
                    dct.Add("姓名首", utf8.GetBytes(sr["FIRSTNAME"].TONotNullString()));
                    dct.Add("姓名末", utf8.GetBytes(sr["MIDDLENAME"].TONotNullString()));
                    dct.Add("姓氏", utf8.GetBytes(sr["LASTNAME"].TONotNullString()));
                    dct.Add("出生年月日", utf8.GetBytes(sr["BIRTH"].TONotNullString()));
                    dct.Add("護照號碼", utf8.GetBytes(sr["MAINDOCNO"].TONotNullString()));
                    dct.Add("抵達日期", utf8.GetBytes(sr["FLIGHTDATE"].TONotNullString()));
                    dct.Add("申請日期", utf8.GetBytes(sr["ADD_TIME"].TONotNullString()));
                    dct.Add("付款金額", utf8.GetBytes(sr["PAYMONEY"].TONotNullString()));
                    dct.Add("信用卡授權碼", utf8.GetBytes(sr["TRACENO"].TONotNullString()));
                    dct.Add("繳費時間", utf8.GetBytes(sr["PAYDATE"].TONotNullString()));
                    dct.Add("專案類別", sr["FLYTYPE"].TONotNullString());
                    dct.Add("春節專案編號", sr["SPRCODE"].TONotNullString());
                    DictionaryEntry[] result = new DictionaryEntry[13];
                    dct.CopyTo(result, 0);
                    dcts.Add(result);
                }
                sr.Close();
            }
            return dcts;
        }

        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<FlightModel> GetOneDataList(FlyPayModel model, int seq)
        {
            var result = new List<FlightModel>();
            StringBuilder querySQL = new StringBuilder(@"
    SELECT distinct MAINDOCNO,FLIGHTDATE,FLIGHTNO,STATUS
    FROM FLYPAY 
    WHERE 1=1
    AND MAINDOCNO = @MAINDOCNO 
    AND BIRTH = @BIRTH
    AND FIRSTNAME = @FIRSTNAME
    AND LASTNAME = @LASTNAME ");

            var item = model.paylist[seq];
            SqlCommand com = new SqlCommand("", conn);

            var birStr = string.Empty;
            if (!string.IsNullOrEmpty(item.birth) && item.birth.Length == 8) { birStr = $"{item.birth.Substring(0, 4)}-{item.birth.Substring(4, 2)}-{item.birth.Substring(6, 2)}"; }
            var fn = string.Empty;
            if (!string.IsNullOrEmpty(item.firstname)) { fn = item.firstname.ToUpper(); }

            var mn = string.Empty;
            if (!string.IsNullOrEmpty(item.middlename))
            {
                querySQL.Append(@" AND MIDDLENAME = @MIDDLENAME ");
                mn = item.middlename.ToUpper();
                DataUtils.AddParameters(com, "MIDDLENAME", mn);
            }
            var ln = string.Empty;
            if (!string.IsNullOrEmpty(item.lastname)) { ln = item.lastname.ToUpper(); }

            DataUtils.AddParameters(com, "MAINDOCNO", item.mainno);
            DataUtils.AddParameters(com, "BIRTH", birStr);
            DataUtils.AddParameters(com, "FIRSTNAME", fn);
            DataUtils.AddParameters(com, "LASTNAME", ln);
            querySQL.Append(@" ORDER BY FLIGHTDATE DESC ");

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    var data = new FlightModel();
                    data.mainno = Convert.ToString(sr["MAINDOCNO"]);
                    data.flightDate = Convert.ToDateTime(sr["FLIGHTDATE"]);
                    data.flightNo = Convert.ToString(sr["FLIGHTNO"]);
                    data.status = Convert.ToString(sr["STATUS"]);
                    result.Add(data);
                }
                sr.Close();
            }

            return result;
        }


        /// <summary>
        /// 取得查詢結果(尚未繳費資料)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<FlightModel> GetOneDataList(string mainno)
        {
            var result = new List<FlightModel>();
            StringBuilder querySQL = new StringBuilder(@"
    SELECT firstname,middlename,lastname,birth,gender,maindocno,nationality
    ,flightdate,flightno,departairport,passengertype,status
    FROM FLYPAY 
    WHERE 1=1
    AND MAINDOCNO = @MAINDOCNO 
    AND STATUS = @STATUS
    group by firstname,middlename,lastname,birth,gender,maindocno,nationality
    ,flightdate,flightno,departairport,passengertype,status ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            DataUtils.AddParameters(com, "MAINDOCNO", mainno);
            DataUtils.AddParameters(com, "STATUS", "N");

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    var data = new FlightModel();
                    data.mainno = Convert.ToString(sr["MAINDOCNO"]);
                    data.flightDate = Convert.ToDateTime(sr["FLIGHTDATE"]);
                    data.flightNo = Convert.ToString(sr["FLIGHTNO"]);
                    data.status = Convert.ToString(sr["STATUS"]);
                    result.Add(data);
                }
                sr.Close();
            }

            return result;
        }

        public string CheckPayList(FlyPayModel model)
        {
            var dta = DateTime.Now.ToString("yyyyMMdd");
            var dt = DateTime.Now;
            var msg = string.Empty;
            var peopleCnt = 0;

            // 一般專案 春節專案
            var item = model.payBasic;
            if (string.IsNullOrEmpty(item.livedays))
            {
                if (model.language != "1")
                {
                    msg += "Please enter the Selection Of Packages.";
                }
                else
                {
                    msg += "選擇方案為必填，請輸入!<br>Please enter the Selection Of Packages.<br>";
                }
            }
            else if (Convert.ToInt32(item.livedays) != 3 && Convert.ToInt32(item.livedays) != 7)
            {
                // 避免被竄改入住天數
                if (model.language != "1")
                {
                    msg += "Please enter the Selection Of Packages.";
                }
                else
                {
                    msg += "只限3天方案或7天方案，請重新輸入!<br>";
                }
            }
            if (string.IsNullOrEmpty(item.dirct))
            {
                if (model.language != "1")
                {
                    msg += "Please enter the correct Country of departure.";
                }
                else
                {
                    msg += "出境國家為必填，請輸入!<br>";
                }
            }

            if (string.IsNullOrEmpty(item.ArDate))
            {
                //item.ArDate = DateTime.Now.ToString("yyyy-MM-dd");
                if (model.language != "1")
                {
                    msg += "Please enter the correct Arrival Date at the Collective Quarantine Facilities.";
                }
                else
                {
                    msg += "抵達檢疫所日期為必填，請輸入!<br>";
                }
            }
            else
            {
                var ardate_new = item.ArDate.Replace("-", "").Replace("/", "");
                if (ardate_new.TOInt32() < dta.TOInt32())
                {
                    if (model.language != "1")
                    {
                        msg += "Please enter the correct Arrival Date at the Collective Quarantine Facilities.";

                    }
                    else
                    {
                        msg += "抵達檢疫所日期僅限當日及未來日期申請，請輸入!<br>";
                    }
                }
            }
            // 入住者資料
            if (string.IsNullOrEmpty(model.paylist[0].firstname) || string.IsNullOrEmpty(model.paylist[0].lastname) || string.IsNullOrEmpty(model.paylist[0].birth) || string.IsNullOrEmpty(model.paylist[0].mainno))
            {
                msg += "第一筆入住者資訊不完整，請輸入!<br>";
            }
            else
            {
                var birth = Convert.ToString(model.paylist[0].birth).Replace("-", "").Replace("/", "");
                if (birth.Length < 8 && birth.TOInt32() > dta.TOInt32())
                {
                    msg += "第1筆入住者出生年月日格式不符合，請輸入!<br>";
                }
                else
                {
                    var age = ToAgeAccToDay(Convert.ToDateTime(model.paylist[0].birth));
                    if (age >= 12)
                    {
                        peopleCnt++;
                    }
                }
            }
            for (var i = 1; i < 4; i++)
            {
                if ((Convert.ToString(model.paylist[i].firstname).TONotNullString() != "" || Convert.ToString(model.paylist[i].lastname).TONotNullString() != "" || Convert.ToString(model.paylist[i].birth).TONotNullString() != "" || Convert.ToString(model.paylist[i].mainno).TONotNullString() != "")
                && (string.IsNullOrEmpty(model.paylist[i].firstname) || string.IsNullOrEmpty(model.paylist[i].lastname) || string.IsNullOrEmpty(model.paylist[i].birth) || string.IsNullOrEmpty(model.paylist[i].mainno)))
                {
                    msg += $"第{i + 1}筆入住者資訊不完整，請輸入!<br>";
                }
                else if (!string.IsNullOrEmpty(model.paylist[i].firstname) && !string.IsNullOrEmpty(model.paylist[i].lastname) && !string.IsNullOrEmpty(model.paylist[i].birth) && !string.IsNullOrEmpty(model.paylist[i].mainno))
                {
                    var birth = Convert.ToString(model.paylist[i].birth).Replace("-", "").Replace("/", "");
                    if (birth.Length < 8 && birth.TOInt32() > dta.TOInt32())
                    {
                        msg += $"第{i + 1}筆入住者出生年月日格式不符合，請輸入!<br>";
                    }
                    else
                    {
                        var age = ToAgeAccToDay(Convert.ToDateTime(model.paylist[i].birth));
                        if (age >= 12)
                        {
                            peopleCnt++;
                        }
                    }
                }
            }
            if (peopleCnt <= 0)
            {
                msg += "至少其中1人必須為12歲以上需付費的年齡，請輸入!<br>";
            }

            return msg;
        }
        /// <summary>
        /// 計算應繳費人數
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int CountPay(FlyPayModel model)
        {
            var peopleCnt = 0;
            // 入住者資料
            if (string.IsNullOrEmpty(model.paylist[0].firstname) || string.IsNullOrEmpty(model.paylist[0].lastname) || string.IsNullOrEmpty(model.paylist[0].birth) || string.IsNullOrEmpty(model.paylist[0].mainno))
            {
            }
            else
            {
                var birth = model.paylist[0].birth.Replace("-", "").Replace("/", "");
                var age = ToAgeAccToDay(Convert.ToDateTime(model.paylist[0].birth));
                if (age >= 12)
                {
                    peopleCnt++;
                }
            }
            for (var i = 1; i < 4; i++)
            {
                if ((Convert.ToString(model.paylist[i].firstname).TONotNullString() != "" || Convert.ToString(model.paylist[i].lastname).TONotNullString() != "" || Convert.ToString(model.paylist[i].birth).TONotNullString() != "" || Convert.ToString(model.paylist[i].mainno).TONotNullString() != "")
                && (string.IsNullOrEmpty(model.paylist[i].firstname) || string.IsNullOrEmpty(model.paylist[i].lastname) || string.IsNullOrEmpty(model.paylist[i].birth) || string.IsNullOrEmpty(model.paylist[i].mainno)))
                {
                }
                else if (!string.IsNullOrEmpty(model.paylist[i].firstname) && !string.IsNullOrEmpty(model.paylist[i].lastname) && !string.IsNullOrEmpty(model.paylist[i].birth) && !string.IsNullOrEmpty(model.paylist[i].mainno))
                {
                    var birth = model.paylist[i].birth.Replace("-", "").Replace("/", "");
                    var age = ToAgeAccToDay(Convert.ToDateTime(model.paylist[i].birth));
                    if (age >= 12)
                    {
                        peopleCnt++;
                    }
                }
            }
            return peopleCnt;
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
        public bool CheckPayBasic()
        {
            var result = false;
            StringBuilder querySQL = new StringBuilder(@"
    SELECT COUNT(MAINDOCNO) AS CNT FROM FLYPAYBASIC 
    WHERE 1=1 AND FLIGHTDATE BETWEEN '20210112' AND '20210126' AND STATUS='Y'");

            SqlCommand com = new SqlCommand("", conn);
            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    var cnt = Convert.ToInt32(sr["CNT"]);
                    if (cnt >= 1500)
                    {
                        // 名額已滿
                        result = true;
                    }
                }
                sr.Close();
            }

            return result;
        }
        /// <summary>
        /// 檢測12/14-2/14 僅1800名額可申請 依據繳費成功的名額 
        /// (A1共218個號碼、A2共67個號碼、A3共181個號碼、B1共416個號碼、B2共272個號碼、B3共646個號碼)
        /// </summary>
        /// <returns></returns>
        public bool CheckPayBasic4()
        {
            var result = false;
            StringBuilder querySQL = new StringBuilder(@"
    SELECT COUNT(MAINDOCNO) AS CNT FROM FLYPAYBASIC 
    WHERE 1=1 AND replace(FLIGHTDATE,'-','') BETWEEN '20211214' AND '20211217' AND STATUS='Y'");

            SqlCommand com = new SqlCommand("", conn);
            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    var cnt = Convert.ToInt32(sr["CNT"]);
                    if (cnt >= 1800)
                    {
                        // 名額已滿
                        result = true;
                    }
                }
                sr.Close();
            }

            return result;
        }
        /// <summary>
        /// 檢測春節專案 依據已註記FLAG, [繳費成功]的名額 
        /// A + 北中南(123) + 流水號001; 
        /// </summary>
        /// <returns></returns>
        public bool CheckPayBasic4part1(TblFLYPAYROOMS model)
        {
            var result = false;
            StringBuilder querySQL = new StringBuilder(@"
      SELECT COUNT(MAINDOCNO) AS CNT FROM FLYPAYBASIC 
  LEFT JOIN FLYPAYBASICSPR ON FLYPAYBASICSPR.FLY_ID = FLYPAYBASIC.FLY_ID
    WHERE 1=1 
	AND replace(FLIGHTDATE,'-','') = @LIMDATE
	AND FLYPAYBASICSPR.ISUSE = 'Y' 
	AND FLYPAYBASICSPR.SECTION = @SECTION
	AND FLYPAYBASICSPR.PLEVEL = @PLEVEL");

            SqlCommand com = new SqlCommand("", conn);
            DataUtils.AddParameters(com, "SECTION", model.SECTION);
            DataUtils.AddParameters(com, "PLEVEL", "1500");
            DataUtils.AddParameters(com, "LIMDATE", model.DAYVAL.TONotNullString().Replace("-", "").Replace("/", ""));
            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    var cnt = Convert.ToInt32(sr["CNT"]);
                    if (cnt >= model.ROOM.TOInt32())
                    {
                        // 已達限額
                        result = true;
                    }
                }
                sr.Close();
            }

            return result;
        }

        /// <summary>
        /// 檢測1/12-1/26 僅1500名額可申請 依據繳費成功的名額 
        /// </summary>
        /// <returns></returns>
        public bool CheckHasPayBasicGUID(string OrderNo)
        {
            List<Hashtable> result = new List<Hashtable>();

            StringBuilder querySQL = new StringBuilder(@"
    SELECT GUID FROM FLYPAYBASIC 
    WHERE 1=1 
    AND PAYRESULT = @ORDERNO
    AND isnull(GUID,'') <> ''
    ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            //成功後回寫繳費時間
            //DataUtils.AddParameters(com, "ARDATE", ArDate);
            DataUtils.AddParameters(com, "ORDERNO", OrderNo);

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    Hashtable ht = new Hashtable();

                    ht["GUID"] = Convert.ToString(sr["GUID"]);

                    result.Add(ht);
                }
                sr.Close();
            }
            return result.ToCount() > 0;
        }
        /// <summary>
        /// 檢測
        /// </summary>
        /// <returns></returns>
        public bool CheckHasPayBasicOVERTIME(string OrderNo)
        {
            List<Hashtable> result = new List<Hashtable>();

            StringBuilder querySQL = new StringBuilder(@"
    SELECT GUID FROM FLYPAYBASIC 
    LEFT JOIN FLYPAYBASICSPR ON FLYPAYBASICSPR.FLY_ID = FLYPAYBASIC.FLY_ID
    WHERE 1=1 
    AND PAYRESULT = @ORDERNO
    AND ISUSE = 'N'
    AND ISNULL(FLYTYPE,'0') > 3
    ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            //成功後回寫繳費時間
            //DataUtils.AddParameters(com, "ARDATE", ArDate);
            DataUtils.AddParameters(com, "ORDERNO", OrderNo);

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    Hashtable ht = new Hashtable();

                    ht["GUID"] = Convert.ToString(sr["GUID"]);

                    result.Add(ht);
                }
                sr.Close();
            }
            return result.ToCount() > 0;
        }
        /// <summary>
        /// 檢測是否有重複 春節專案
        /// </summary>
        /// <returns></returns>
        public int CheckPayBasicOne(string MainDocNo, string FLIGHTDATE)
        {
            var ardate = FLIGHTDATE.Replace("-", "").Replace("/", "").TOInt32();
            var sdate = 20211214;
            var edate = 20220205;
            //if (ardate >= 20211214 && ardate <= 20211222)
            //{
            //    // 第一波 春節專案
            //    sdate = "20211214";
            //    edate = "20211222";
            //}
            //else if (ardate >= 20211223 && ardate <= 20211231)
            //{
            //    // 第二波 春節專案
            //    sdate = "20211223";
            //    edate = "20211231";
            //}
            //else if (ardate >= 20220101 && ardate <= 20220109)
            //{
            //    // 第三波 春節專案
            //    sdate = "20220101";
            //    edate = "20220109";
            //}
            //else if (ardate >= 20220110 && ardate <= 20220118)
            //{
            //    // 第四波 春節專案
            //    sdate = "20220110";
            //    edate = "20220118";
            //}
            //else if (ardate >= 20220119 && ardate <= 20220127)
            //{
            //    // 第五波 春節專案
            //    sdate = "20220119";
            //    edate = "20220127";
            //}
            //else if (ardate >= 20220128 && ardate <= 20220205)
            //{
            //    // 第六波 春節專案
            //    sdate = "20220128";
            //    edate = "20220205";
            //}
            // 第二次開放，僅查詢2021-11-15後是否有重複申請
            StringBuilder querySQL = new StringBuilder(@"
    SELECT * FROM FLYPAYBASIC 
    WHERE 1=1  
    AND MAINDOCNO=@MAINDOCNO 
    AND STATUS='Y' 
    AND ISNULL(GUID,'') <> ''
    AND replace(FLIGHTDATE,'-','') between @SDATE and @EDATE
    ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            //成功後回寫繳費時間
            DataUtils.AddParameters(com, "MAINDOCNO", MainDocNo);
            DataUtils.AddParameters(com, "SDATE", sdate);
            DataUtils.AddParameters(com, "EDATE", edate);
            com.CommandText = querySQL.ToString();
            var cnt = 0;
            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    cnt++;
                }
            }
            return cnt;
        }

        /// <summary>
        /// 取得
        /// </summary>
        /// <returns></returns>
        public List<Hashtable> GetPayBasicList(string mainno, string birth, string ardate)
        {
            List<Hashtable> result = new List<Hashtable>();
            StringBuilder querySQL = new StringBuilder(@"
     SELECT top 1 FIRSTNAME, MIDDLENAME, LASTNAME,FLIGHTDATE, GUID
	,FIRSTNAME2,MIDDLENAME2,LASTNAME2,FIRSTNAME3,MIDDLENAME3,LASTNAME3,FIRSTNAME4,MIDDLENAME4,LASTNAME4
    FROM FLYPAYBASIC 
    WHERE 1=1  
    AND ((replace(BIRTH,'-','')=@BIRTH AND MAINDOCNO LIKE ''+@MAINDOCNO+'%') 
or (replace(birth2,'-','')=@BIRTH and maindocno2 like ''+@MAINDOCNO+'%') 
or (replace(birth3,'-','')=@BIRTH and MAINDOCNO3 like ''+@MAINDOCNO+'%') 
or (replace(birth4,'-','')=@BIRTH and maindocno4 like ''+@MAINDOCNO+'%'))
    AND STATUS='Y' 
    and flytype='11'
    ORDER BY FLYPAYBASIC.ADD_TIME DESC
            ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            //成功後回寫繳費時間
            DataUtils.AddParameters(com, "MAINDOCNO", mainno);
            DataUtils.AddParameters(com, "BIRTH", birth);

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    Hashtable ht = new Hashtable();

                    ht["fmlName"] = Convert.ToString(sr["FIRSTNAME"]) + " " + Convert.ToString(sr["MIDDLENAME"]) + " " + Convert.ToString(sr["LASTNAME"]);
                    ht["fmlName2"] = Convert.ToString(sr["FIRSTNAME2"]) + " " + Convert.ToString(sr["MIDDLENAME2"]) + " " + Convert.ToString(sr["LASTNAME2"]);
                    ht["fmlName3"] = Convert.ToString(sr["FIRSTNAME3"]) + " " + Convert.ToString(sr["MIDDLENAME3"]) + " " + Convert.ToString(sr["LASTNAME3"]);
                    ht["fmlName4"] = Convert.ToString(sr["FIRSTNAME4"]) + " " + Convert.ToString(sr["MIDDLENAME4"]) + " " + Convert.ToString(sr["LASTNAME4"]);
                    ht["fliGHTdate"] = Convert.ToString(sr["FLIGHTDATE"]);
                    ht["Guid"] = Convert.ToString(sr["GUID"]);
                    result.Add(ht);
                }
                sr.Close();
            }
            return result;
        }
        /// <summary>
        /// 取得查詢結果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool InsertFlyPayBasicOrderNo(FlyPayModel model, string orderNo, bool isTest = false, string banktype = null)
        {
            var result = string.Empty;
            var paydate = DateTime.Today.ToShortDateString().Replace('/', '-');
            var paymoney = model.amountall.ToString();
            var payresult = orderNo;

            StringBuilder querySQLB = new StringBuilder(@"
        INSERT INTO [dbo].[FLYPAYBASIC]
           ([GUID],[FIRSTNAME],[MIDDLENAME],[LASTNAME],[BIRTH]
           ,[GENDER],[MAINDOCNO],[NATIONALITY],[FLIGHTDATE],[FLIGHTNO]
           ,[DEPARTAIRPORT],[PASSENGERTYPE],[ADD_TIME],[ADD_FUN_CD],[ADD_ACC]
           ,[PAYDATE],[PAYMONEY],[PAYRESULT],[PLACE],[ISPAY]
           ,[STATUS],[PAYRETURN],[XID],[TRACENO],[QID]
           ,[BPAYDATE],[UPD_TIME],[UPD_FUN_CD],[UPD_ACC],[DEL_MK]
           ,[DEL_TIME],[DEL_FUN_CD],[DEL_ACC] ,[FLYTYPE] ,[FLYCONTRY] 
           ,[FLYCONTRYTR],[BANKTYPE],[RC],[SPRCODE],[NEEDBACK]
           ,[FIRSTNAME2],[MIDDLENAME2],[LASTNAME2],[BIRTH2],[MAINDOCNO2]
           ,[FIRSTNAME3],[MIDDLENAME3],[LASTNAME3],[BIRTH3],[MAINDOCNO3]
           ,[FIRSTNAME4],[MIDDLENAME4],[LASTNAME4],[BIRTH4],[MAINDOCNO4])
      VALUES (@GUID ,@FIRSTNAME, @MIDDLENAME, @LASTNAME, @BIRTH
           ,@GENDER, @MAINDOCNO, @NATIONALITY, @FLIGHTDATE, @FLIGHTNO
           ,@DEPARTAIRPORT, @PASSENGERTYPE, GETDATE(), 'WEB-FLY', @ADD_ACC
           ,NULL, @PAYMONEY, @PAYRESULT, NULL,'N'
		   , 'N', NULL, NULL, NULL ,NULL
		   ,NULL, GETDATE(), 'WEB-FLY', '', 'N'
           ,NULL, NULL, NULL, @FLYTYPE, @FLYCONTRY
           ,@FLYCONTRYTR,@BANKTYPE,'','',''
           ,@FIRSTNAME2,@MIDDLENAME2,@LASTNAME2,@BIRTH2,@MAINDOCNO2
           ,@FIRSTNAME3,@MIDDLENAME3,@LASTNAME3,@BIRTH3,@MAINDOCNO3
           ,@FIRSTNAME4,@MIDDLENAME4,@LASTNAME4,@BIRTH4,@MAINDOCNO4)
       SELECT SCOPE_IDENTITY() 
   ");
            SqlCommand com = new SqlCommand(querySQLB.ToString(), conn, tran);
            //成功後回寫繳費時間
            DataUtils.AddParameters(com, "GUID", model.Guid);
            DataUtils.AddParameters(com, "FIRSTNAME", model.paylist[0].firstname.TONotNullString());
            DataUtils.AddParameters(com, "MIDDLENAME", model.paylist[0].middlename.TONotDashString());
            DataUtils.AddParameters(com, "LASTNAME", model.paylist[0].lastname.TONotNullString());
            DataUtils.AddParameters(com, "BIRTH", model.paylist[0].birth.TONotNullString());
            DataUtils.AddParameters(com, "GENDER", "");
            DataUtils.AddParameters(com, "MAINDOCNO", model.paylist[0].mainno.TONotNullString());
            DataUtils.AddParameters(com, "NATIONALITY", "");
            DataUtils.AddParameters(com, "FLIGHTDATE", model.payBasic.ArDate);
            DataUtils.AddParameters(com, "FLIGHTNO", "");
            DataUtils.AddParameters(com, "DEPARTAIRPORT", "");
            DataUtils.AddParameters(com, "PASSENGERTYPE", "");
            DataUtils.AddParameters(com, "ADD_ACC", model.payBasic.RAN);
            DataUtils.AddParameters(com, "PAYMONEY", paymoney);
            DataUtils.AddParameters(com, "PAYRESULT", payresult);
            DataUtils.AddParameters(com, "FLYTYPE", "11"); //簡易專案
            DataUtils.AddParameters(com, "FLYCONTRY", model.payBasic.dirct.TONotNullString());
            DataUtils.AddParameters(com, "FLYCONTRYTR", model.payBasic.dirct_tr.TONotNullString());
            // 銀行來源
            DataUtils.AddParameters(com, "BANKTYPE", banktype.TONotNullString());
            // 第2,3,4入住者資料
            DataUtils.AddParameters(com, "FIRSTNAME2", model.paylist[1].firstname.TONotNullString());
            DataUtils.AddParameters(com, "MIDDLENAME2", model.paylist[1].middlename.TONotDashString());
            DataUtils.AddParameters(com, "LASTNAME2", model.paylist[1].lastname.TONotNullString());
            DataUtils.AddParameters(com, "BIRTH2", model.paylist[1].birth.TONotNullString());
            DataUtils.AddParameters(com, "MAINDOCNO2", model.paylist[1].mainno.TONotNullString());
            DataUtils.AddParameters(com, "FIRSTNAME3", model.paylist[2].firstname.TONotNullString());
            DataUtils.AddParameters(com, "MIDDLENAME3", model.paylist[2].middlename.TONotDashString());
            DataUtils.AddParameters(com, "LASTNAME3", model.paylist[2].lastname.TONotNullString());
            DataUtils.AddParameters(com, "BIRTH3", model.paylist[2].birth.TONotNullString());
            DataUtils.AddParameters(com, "MAINDOCNO3", model.paylist[2].mainno.TONotNullString());
            DataUtils.AddParameters(com, "FIRSTNAME4", model.paylist[3].firstname.TONotNullString());
            DataUtils.AddParameters(com, "MIDDLENAME4", model.paylist[3].middlename.TONotDashString());
            DataUtils.AddParameters(com, "LASTNAME4", model.paylist[3].lastname.TONotNullString());
            DataUtils.AddParameters(com, "BIRTH4", model.paylist[3].birth.TONotNullString());
            DataUtils.AddParameters(com, "MAINDOCNO4", model.paylist[3].mainno.TONotNullString());

            string FLY_ID = com.ExecuteScalar().TONotNullString();

            if (model.payBasic.dirct_tr_list.ToCount() > 0)
            {
                foreach (var di in model.payBasic.dirct_tr_list)
                {
                    if (di.TONotNullString() != "")
                    {
                        StringBuilder querySQLB_FLYPAYBASICCON = new StringBuilder(@"
INSERT INTO [dbo].[FLYPAYBASICCON]
           ([FLY_ID]           ,[COUNTRY_ID]          )
      VALUES (@FLY_ID ,@COUNTRY_ID)
   ");
                        SqlCommand com_FLYPAYBASICCON = new SqlCommand(querySQLB_FLYPAYBASICCON.ToString(), conn, tran);
                        //成功後回寫繳費時間
                        DataUtils.AddParameters(com_FLYPAYBASICCON, "FLY_ID", FLY_ID);
                        DataUtils.AddParameters(com_FLYPAYBASICCON, "COUNTRY_ID", di.TONotNullString());

                        int flagG = com_FLYPAYBASICCON.ExecuteNonQuery();
                    }

                }
            }

            return true;
        }

        /// <summary>
        /// 紀錄IP(GOTYPE:0>轉導紀錄 1>查詢紀錄)
        /// </summary>
        /// <returns></returns>
        public bool InsertFlyPayBasicIP(string IP, string GOTYPE)
        {
            StringBuilder querySQL = new StringBuilder(@"
    INSERT INTO FLYPAYBASICIP 
    (FLYPAYBASICIP,FLYPAYBASICMODTIME,GOTYPE) 
    values 
    (@FLYPAYBASICIP,@FLYPAYBASICMODTIME,@GOTYPE)");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            DataUtils.AddParameters(com, "FLYPAYBASICIP", IP);
            DataUtils.AddParameters(com, "FLYPAYBASICMODTIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            DataUtils.AddParameters(com, "GOTYPE", GOTYPE);
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }

        /// <summary>
        /// 一般信用卡回傳
        /// </summary>
        /// <param name="orderNo">訂單編號</param>
        /// <param name="URLEnc">壓碼值</param>
        /// <param name="XID">中國信託回傳編號</param>
        /// <returns></returns>
        public bool UpdateCreditSuccessRecv(string orderNo, string URLEnc, string XID, string traceNO, DateTime? paydate)
        {
            StringBuilder querySQL = new StringBuilder(@"
    UPDATE FLYPAY 
    SET PAYRETURN = @PAYRETURN,PAYDATE = @PAYDATE
    ,XID = @XID, UPD_TIME = GETDATE()
    ,TRACENO = @TRACENO
    ,STATUS = @STATUS
    WHERE PAYRESULT = @PAYRESULT");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn, tran);
            DataUtils.AddParameters(com, "PAYDATE", paydate);
            DataUtils.AddParameters(com, "PAYRETURN", URLEnc);
            DataUtils.AddParameters(com, "PAYRESULT", orderNo);
            DataUtils.AddParameters(com, "XID", XID);
            DataUtils.AddParameters(com, "TRACENO", traceNO);
            DataUtils.AddParameters(com, "STATUS", "Y");
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }

        /// <summary>
        /// 一般信用卡回傳
        /// </summary>
        /// <param name="orderNo">訂單編號</param>
        /// <param name="URLEnc">壓碼值</param>
        /// <param name="XID">中國信託回傳編號</param>
        /// <returns></returns>
        public bool UpdateCreditSuccessRecvB(string orderNo, string URLEnc, string XID, string traceNO, DateTime? paydate = null)
        {
            StringBuilder querySQL = new StringBuilder(@"
    UPDATE FLYPAYBASIC 
    SET PAYRETURN = @PAYRETURN,PAYDATE = @PAYDATE
    ,XID = @XID, UPD_TIME = GETDATE()
    ,TRACENO = @TRACENO
    ,STATUS = @STATUS
    WHERE PAYRESULT = @PAYRESULT");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn, tran);
            DataUtils.AddParameters(com, "PAYDATE", paydate);
            DataUtils.AddParameters(com, "PAYRETURN", URLEnc);
            DataUtils.AddParameters(com, "PAYRESULT", orderNo);
            DataUtils.AddParameters(com, "XID", XID);
            DataUtils.AddParameters(com, "TRACENO", traceNO);
            DataUtils.AddParameters(com, "STATUS", "Y");
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }
        /// <summary>
        /// 一般信用卡回傳
        /// </summary>
        /// <param name="orderNo">訂單編號</param>
        /// <param name="URLEnc">壓碼值</param>
        /// <param name="XID">中國信託回傳編號</param>
        /// <returns></returns>
        public bool UpdateCreditSuccessRecvGuid(string orderNo, string URLEnc, string XID, string traceNO, string Guid, string paydate)
        {
            var isStatus = string.IsNullOrEmpty(Guid) ? "N" : "Y";
            var NewGuid = string.Empty;
            if (!string.IsNullOrEmpty(Guid))
            {
                logger.Debug("UpdateCreditSuccessRecvGuid.ReNewGuid then Update.");
                NewGuid = this.RanDomNumAgain(Guid);
            }
            StringBuilder querySQL = new StringBuilder(@"
    UPDATE FLYPAYBASIC 
    SET PAYRETURN = @PAYRETURN,PAYDATE = @PAYDATE
    ,XID = @XID, UPD_TIME = GETDATE()
    ,TRACENO = @TRACENO
    ,STATUS = @STATUS
    ,GUID=@GUID
    WHERE PAYRESULT = @PAYRESULT");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn, tran);
            DataUtils.AddParameters(com, "PAYDATE", paydate);
            DataUtils.AddParameters(com, "PAYRETURN", URLEnc);
            DataUtils.AddParameters(com, "PAYRESULT", orderNo);
            DataUtils.AddParameters(com, "XID", XID);
            DataUtils.AddParameters(com, "TRACENO", traceNO);
            DataUtils.AddParameters(com, "STATUS", isStatus);
            DataUtils.AddParameters(com, "GUID", NewGuid);
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }

        public bool UpdateCreditSuccessRecvUnionPay(string orderNo, string URLEnc, string XID, string QID, string traceNO, DateTime? paydate)
        {
            StringBuilder querySQL = new StringBuilder(@"
    UPDATE FLYPAY 
    SET PAYRETURN = @PAYRETURN, PAYDATE = @PAYDATE
    ,XID = @XID, UPD_TIME = GETDATE()
    ,QID = @QID, TRACENO = @TRACENO
    ,STATUS = @STATUS
    WHERE PAYRESULT = @PAYRESULT ");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn, tran);
            DataUtils.AddParameters(com, "PAYDATE", paydate);
            DataUtils.AddParameters(com, "PAYRETURN", URLEnc);
            DataUtils.AddParameters(com, "PAYRESULT", orderNo);
            DataUtils.AddParameters(com, "XID", XID);
            DataUtils.AddParameters(com, "QID", QID);
            DataUtils.AddParameters(com, "TRACENO", traceNO);
            DataUtils.AddParameters(com, "STATUS", "Y");
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }

        public bool UpdateCreditSuccessRecvUnionPayGuid(string orderNo, string URLEnc, string XID, string QID, string traceNO, string Guid, DateTime? paydate)
        {
            StringBuilder querySQL = new StringBuilder(@"
    UPDATE FLYPAYBASIC 
    SET PAYRETURN = @PAYRETURN, PAYDATE = @PAYDATE
    ,XID = @XID, UPD_TIME = GETDATE()
    ,QID = @QID, TRACENO = @TRACENO
    ,STATUS = @STATUS,GUID=@GUID
    WHERE PAYRESULT = @PAYRESULT ");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn, tran);
            DataUtils.AddParameters(com, "PAYDATE", paydate);
            DataUtils.AddParameters(com, "PAYRETURN", URLEnc);
            DataUtils.AddParameters(com, "PAYRESULT", orderNo);
            DataUtils.AddParameters(com, "XID", XID);
            DataUtils.AddParameters(com, "QID", QID);
            DataUtils.AddParameters(com, "TRACENO", traceNO);
            DataUtils.AddParameters(com, "STATUS", "Y");
            DataUtils.AddParameters(com, "GUID", Guid);
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }

        /// <summary>
        /// 一般信用卡回傳
        /// </summary>
        /// <param name="orderNo">訂單編號</param>
        /// <param name="URLEnc">壓碼值</param>
        /// <param name="XID">中國信託回傳編號</param>
        /// <returns></returns>
        public bool UpdateCreditRC(string orderNo, string RC)
        {
            StringBuilder querySQL = new StringBuilder(@"
    UPDATE FLYPAYBASIC 
    SET RC = @RC, UPD_TIME = GETDATE()
    WHERE PAYRESULT = @PAYRESULT");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn, tran);
            DataUtils.AddParameters(com, "PAYRESULT", orderNo);
            DataUtils.AddParameters(com, "RC", RC);
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }

        public string GetSprProjectNum(string payresult)
        {
            var result = string.Empty;
            var cnt = 0;
            var section = string.Empty;
            var plevel = string.Empty;
            var data = this.GetRow<TblFLYPAYBASIC>(new TblFLYPAYBASIC() { PAYRESULT = payresult });
            if (data != null)
            {
                var spr = this.GetRow<TblFLYPAYBASICSPR>(new TblFLYPAYBASICSPR() { FLY_ID = data.FLY_ID });
                if (spr != null)
                {
                    var fcnt = string.Empty;
                    StringBuilder querySQL = new StringBuilder(@"
    SELECT COUNT(MAINDOCNO) AS FCNT
    FROM FLYPAYBASIC 
    JOIN FLYPAYBASICSPR ON FLYPAYBASICSPR.FLY_ID = FLYPAYBASIC.FLY_ID
    WHERE 1=1
    AND replace(FLYPAYBASIC.FLIGHTDATE,'-','') = @FLIGHTDATE
    AND FLYPAYBASIC.STATUS = 'Y'
    AND FLYPAYBASICSPR.ISUSE = 'Y'
    AND FLYPAYBASICSPR.SECTION = @SECTION
");
                    SqlCommand com = new SqlCommand("", conn);
                    DataUtils.AddParameters(com, "FLIGHTDATE", data.FLIGHTDATE.Replace("-", ""));
                    DataUtils.AddParameters(com, "SECTION", spr.SECTION);
                    com.CommandText = querySQL.ToString();
                    using (SqlDataReader sr = com.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            fcnt = Convert.ToString(sr["FCNT"]);
                        }
                        sr.Close();
                    }
                    var splitSectionList = new List<FlyPayEchelonViewModel>();
                    StringBuilder querySQL2 = new StringBuilder(@"
select a.echelon, a.section, a.daycode, a.room, a.dayval, b.se_code, b.rooms, b.sec 
from flypayrooms a
left join flypayrooms_de b on a.daycode= b.se_day and  case a.section when '1' then 'A' when '2' then 'B' when '3' then 'C' end = b.sec
where 1=1
and replace(a.dayval,'-','') = @FLIGHTDATE
and a.section = @SECTION
order by b.se_code
");
                    SqlCommand com2 = new SqlCommand("", conn);
                    DataUtils.AddParameters(com2, "FLIGHTDATE", data.FLIGHTDATE.TONotNullString().Replace("-", "").Replace("/", ""));
                    DataUtils.AddParameters(com2, "SECTION", spr.SECTION);
                    com2.CommandText = querySQL2.ToString();
                    using (SqlDataReader sr = com2.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            splitSectionList.Add(new FlyPayEchelonViewModel()
                            {
                                echelon = sr["echelon"].TONotNullString(),
                                daycode = sr["daycode"].TONotNullString(),
                                dayval = sr["dayval"].TONotNullString(),
                                room = sr["room"].TONotNullString(),
                                rooms = sr["rooms"].TONotNullString(),
                                sec = sr["sec"].TONotNullString(),
                                section = sr["section"].TONotNullString(),
                                se_code = sr["se_code"].TONotNullString()
                            });
                        }
                        sr.Close();
                    }
                    // 地區代碼
                    var sec = splitSectionList.FirstOrDefault().sec;
                    // 集檢所代碼
                    var se_cd = string.Empty;
                    // 梯次
                    var po = splitSectionList.FirstOrDefault().echelon.PadLeft(2, '0');
                    // 天代碼
                    var day_cd = splitSectionList.FirstOrDefault().daycode;
                    // 流水號
                    var wcnt = string.Empty;

                    var num = 0;
                    foreach (var item in splitSectionList)
                    {
                        num = fcnt.TOInt32() - item.rooms.TOInt32();
                        if (num <= 0)
                        {
                            se_cd = item.se_code;
                            wcnt = (num + item.rooms.TOInt32()).ToString().PadLeft(2, '0');
                        }
                    }

                    // 北中南 + 梯次 + 流水號 C 30 04 D3 01

                }
            }

            return result;
        }
        /// <summary>
        /// 一般信用卡回傳
        /// </summary>
        /// <param name="orderNo">訂單編號</param>
        /// <param name="URLEnc">壓碼值</param>
        /// <param name="XID">中國信託回傳編號</param>
        /// <returns></returns>
        public bool UpdateCreditSuccessNeedBack(string orderNo)
        {
            StringBuilder querySQL = new StringBuilder(@"
    UPDATE FLYPAYBASIC 
    SET UPD_TIME = GETDATE(), NEEDBACK= 'Y'
    WHERE PAYRESULT = @PAYRESULT");
            SqlCommand com = new SqlCommand(querySQL.ToString(), conn, tran);
            DataUtils.AddParameters(com, "PAYRESULT", orderNo);
            int flag = com.ExecuteNonQuery();

            if (flag == 1) return true;
            return false;
        }

        /// <summary>
        /// 取得 額滿資訊
        /// </summary>
        /// <returns></returns>
        public List<Hashtable> GetRoomsFullList()
        {
            List<Hashtable> result = new List<Hashtable>();
            StringBuilder querySQL = new StringBuilder(@"
SELECT CASE FLYPAYROOMS.SECTION WHEN '1' THEN '北區' WHEN '2' THEN '中區' WHEN '3' THEN '南區' END AS SECTION, 
SUBSTRING(cast(FDAY as varchar(8)),1,4)+'-'
+SUBSTRING(cast(FDAY as varchar(8)),5,2)+'-'
+SUBSTRING(cast(FDAY as varchar(8)),7,2) AS
FDAY FROM FLYPAYROOMS 
JOIN (
SELECT COUNT(FLYPAYBASICSPR.FLY_ID) AS UCNT, SECTION, REPLACE(FLYPAYBASIC.FLIGHTDATE, '-','') AS FDAY  FROM FLYPAYBASICSPR
JOIN FLYPAYBASIC ON FLYPAYBASIC.FLY_ID = FLYPAYBASICSPR.FLY_ID
WHERE ISUSE = @ISUSE AND FLYPAYBASIC.GUID IS NOT NULL
GROUP BY SECTION, REPLACE(FLYPAYBASIC.FLIGHTDATE,'-','')
) A ON REPLACE(A.FDAY,'-','') = REPLACE(FLYPAYROOMS.DAYVAL,'-','') AND A.SECTION = FLYPAYROOMS.SECTION
WHERE A.UCNT >= FLYPAYROOMS.ROOM
            ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            //成功後回寫繳費時間
            DataUtils.AddParameters(com, "ISUSE", "Y");

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    Hashtable ht = new Hashtable();

                    ht["sectionName"] = Convert.ToString(sr["SECTION"]);
                    ht["fDayName"] = Convert.ToString(sr["FDAY"]);
                    result.Add(ht);
                }
                sr.Close();
            }
            return result;
        }

        /// <summary>
        /// 取得 額滿資訊
        /// </summary>
        /// <returns></returns>
        public string getAddtime(string ono)
        {
            var result = string.Empty;
            StringBuilder querySQL = new StringBuilder(@"
SELECT CONVERT(varchar,ADD_TIME,20) as ADD_TIME FROM FLYPAYBASIC
WHERE PAYRESULT = @PAYRESULT

            ");

            SqlCommand com = new SqlCommand(querySQL.ToString(), conn);
            //成功後回寫繳費時間
            DataUtils.AddParameters(com, "PAYRESULT", ono);

            com.CommandText = querySQL.ToString();

            using (SqlDataReader sr = com.ExecuteReader())
            {
                while (sr.Read())
                {
                    Hashtable ht = new Hashtable();

                    ht["ADD_TIME"] = Convert.ToString(sr["ADD_TIME"]);
                    result = Convert.ToString(sr["ADD_TIME"]);
                }
                sr.Close();
            }
            return result;
        }
        /// <summary>
        /// 訂單編號 訂房識別碼
        /// </summary>
        /// <returns></returns>
        public string RanDomNumAgain(string GuidOld)
        {
            Random crandom = new Random();
            var Cra = crandom.Next(100000);
            var CraString = GuidOld.TONotNullString().PadLeft(5, '0');
            var GuidCount = GetOneGuidDataList(CraString);
            while (GuidCount > 0)
            {
                Cra = crandom.Next(100000);
                CraString = Cra.TONotNullString().PadLeft(5, '0');
                GuidCount = GetOneGuidDataList(CraString);
            }
            return CraString;
        }

        /// <summary>
        /// 將出生日期轉換為年齡
        /// <para>精確到天</para>
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int ToAgeAccToDay(DateTime target)
        {
            DateTime a = target; //小日期
            DateTime b = DateTime.Now; //大日期
            if (a > b)
            {
                return 0;
            }
            else
            {
                return (((b.Year - a.Year) * 12) + (b.Month - a.Month)) / 12 + ((b.Year == a.Year && b.Month == a.Month && b.Day < a.Day) ? -1 : 0);
            }
        }
    }
}