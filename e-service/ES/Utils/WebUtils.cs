using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using ES.Areas.Admin.Models;

namespace ES.Utils
{
    public static class WebUtils
    {

        /// <summary>
        /// 依據單位代碼 取得承辦人 案件處理的代碼為'152'
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static List<Map> GetSrvUnitADMIN(String srv_id)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"select a.ACC_NO,a.NAME,u.UNIT_NAME,a.UNIT_CD
                                FROM ADMIN a 
                                LEFT JOIN UNIT u on a.UNIT_CD = u.UNIT_CD
                                LEFT JOIN ADMIN_LEVEL al on a.ACC_NO = al.ACC_NO
                                where a.UNIT_CD in (select UNIT_CD FROM SERVICE_UNIT WHERE srv_id = @srv_id and del_mk = 'N')
                                and al.MN_ID = 152
                                order by u.UNIT_PCD,u.SEQ_NO
                                ";
                com.CommandText = sql;
                DataUtils.AddParameters(com, "srv_id", (String.IsNullOrEmpty(srv_id) ? "" : srv_id));
                //com.Parameters.Add("@srv_id", (String.IsNullOrEmpty(srv_id) ? "" : srv_id));
                SqlDataReader sr = com.ExecuteReader();
                while (sr.Read())
                {
                    Map map = new Map();
                    map.Add("ACC_NO", sr["ACC_NO"].ToString());
                    map.Add("NAME", sr["NAME"].ToString());
                    map.Add("UNIT_NAME", sr["UNIT_NAME"].ToString());
                    map.Add("UNIT_CD", sr["UNIT_CD"].ToString());
                    li.Add(map);
                }
                sr.Close();
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        /// <summary>
        /// 依據單位代碼 取得案件進度
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static string GetFLOWCDName(int unit_cd, string flow_cd)
        {
            string temp = null;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"
                            select FLOW_NAME from M_CASE_STATUS 
                            where flow_cd=@flow_cd 
                            and unit_scd in (select unit_scd from unit where unit_cd=@unit_cd)";
                com.CommandText = sql;
                DataUtils.AddParameters(com, "flow_cd", (String.IsNullOrEmpty(flow_cd) ? "" : flow_cd));
                DataUtils.AddParameters(com, "unit_cd", unit_cd);
                //com.Parameters.Add("@flow_cd", (String.IsNullOrEmpty(flow_cd) ? "" : flow_cd));
                //com.Parameters.Add("@unit_cd", unit_cd);

                SqlDataReader sr = com.ExecuteReader();
                if (sr.Read())
                {
                    temp = sr["FLOW_NAME"].ToString();
                }
                sr.Close();
                conn.Close();
                conn.Dispose();
            }
            return temp;
        }

        public static string GetFLOWCDName(string app_id, string flow_cd)
        {
            string temp = "";
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    /*
                    String sql = @"
                                    select m.FLOW_NAME 
                                    from M_CASE_STATUS m
                                    where FLOW_CD=@FLOW_CD 
                                    and unit_scd in (
                                        select unit_scd from unit where unit_cd in (
                                            select unit_cd from apply where APP_ID = @APP_ID
                                        )
                                    )
                                    ";
                    */

                    String sql = @"
                        SELECT CODE_DESC AS FLOW_NAME
                        FROM CODE_CD
                        WHERE DEL_MK = 'N'
                          AND CODE_PCD IN ( SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD IN (SELECT UNIT_CD FROM APPLY WHERE APP_ID = @APP_ID) )
                          AND CODE_CD = @FLOW_CD
                          AND CODE_KIND = 'F_CASE_STATUS'
                        ORDER BY SEQ_NO
                    ";

                    dbc.Parameters.Clear();
                    dbc.CommandText = sql;
                    DataUtils.AddParameters(dbc, "FLOW_CD", flow_cd);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    //dbc.Parameters.Add("@FLOW_CD", flow_cd);
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            temp = DataUtils.GetDBString(sda, 0);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return temp;
        }

        public static List<Map> GetServiceAll()
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"select distinct s.NAME,s.SRV_ID
                                FROM service s
                                inner join mail_log m on m.srv_id = s.srv_id
                                order by s.name 
                                ";
                com.CommandText = sql;
                SqlDataReader sr = com.ExecuteReader();
                while (sr.Read())
                {
                    Map map = new Map();
                    map.Add("NAME", sr["NAME"].ToString());
                    map.Add("SRV_ID", sr["SRV_ID"].ToString());
                    li.Add(map);
                }
                sr.Close();
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        /// <summary>
        /// 依據單位代碼 取得案件類別
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static List<Map> GetSERVICEName(int unit_cd)
        {
            Map map = null;
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"
                            select SRV_ID,NAME from SERVICE where SC_ID in 
                            (select SC_ID from SERVICE_CATE where UNIT_CD=@UNIT_CD) order by SRV_ID ";
                com.CommandText = sql;
                //com.Parameters.Add("@unit_cd", unit_cd);
                DataUtils.AddParameters(com, "unit_cd", unit_cd);

                SqlDataReader sr = com.ExecuteReader();
                while (sr.Read())
                {
                    map = new Map();
                    map.Add("SRV_ID", sr["SRV_ID"].ToString());
                    map.Add("NAME", sr["NAME"].ToString());
                    li.Add(map);
                }
                sr.Close();
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        /// <summary>
        /// 取得案件分類的LEVEL 0 層級
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static List<Map> GetServiceLCate()
        {
            Map map = null;
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"
                        select SC_ID,NAME from SERVICE_CATE 
                        where SC_PID = 0 and LEVEL = 1
                        order by SC_ID ";
                com.CommandText = sql;

                SqlDataReader sr = com.ExecuteReader();
                while (sr.Read())
                {
                    map = new Map();
                    map.Add("SC_ID", sr["SC_ID"].ToString());
                    map.Add("NAME", sr["NAME"].ToString());
                    li.Add(map);
                }
                sr.Close();
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        /// <summary>
        /// 取得案件分類的LEVEL 0 層級
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static Boolean GetServiceRPT(String srv_id)
        {
            Boolean result = false;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"
                        select * from SERVICE 
                        where online_n_mk = 'Y' and rpt_name like @rpt_name ";
                com.CommandText = sql;
                com.Parameters.Clear();
                //com.Parameters.Add("rpt_name", "%" + srv_id + "%");
                DataUtils.AddParameters(com, "rpt_name", "%" + srv_id + "%");

                SqlDataReader sr = com.ExecuteReader();
                result = sr.Read();
                sr.Close();
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        /// <summary>
        /// 依據單位代碼 取得案件總類
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static List<Map> GetFLOWCDAll(int unit_cd)
        {
            Map map = null;
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                /*
                String sql = @"
                        select FLOW_CD,FLOW_NAME from M_CASE_STATUS 
                        where CASE_CAT=4 
                        and UNIT_SCD in (select UNIT_SCD from UNIT where UNIT_CD=@UNIT_CD) 
                        and FLOW_CD<>'00' order by FLOW_CD ";
                */

                String sql = @"
                    SELECT CODE_CD AS FLOW_CD, CODE_DESC AS FLOW_NAME
                    FROM CODE_CD
                    WHERE DEL_MK = 'N'
                      AND CODE_PCD IN (SELECT UNIT_SCD FROM UNIT WHERE UNIT_CD = @UNIT_CD)
                      AND CODE_KIND = 'F_CASE_STATUS'
                    ORDER BY SEQ_NO
                ";

                com.CommandText = sql;
                //com.Parameters.Add("@unit_cd", unit_cd);
                DataUtils.AddParameters(com, "UNIT_CD", unit_cd);

                SqlDataReader sr = com.ExecuteReader();
                while (sr.Read())
                {
                    map = new Map();
                    map.Add("FLOW_CD", sr["FLOW_CD"].ToString());
                    map.Add("FLOW_NAME", sr["FLOW_NAME"].ToString());
                    li.Add(map);
                }
                sr.Close();
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        /// <summary>
        /// 取得單位名稱
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static List<Map> GetUnitAll()
        {
            Map map = null;
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"select UNIT_CD, UNIT_NAME from UNIT where UNIT_LEVEL=1 Order By SEQ_NO ";
                com.CommandText = sql;

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        map = new Map();
                        map.Add("UNIT_CD", sr["UNIT_CD"].ToString());
                        map.Add("UNIT_NAME", sr["UNIT_NAME"].ToString());
                        li.Add(map);
                    }
                    sr.Close();
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        /// <summary>
        /// 取得單位名稱
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static String GetUnitName(int unit_cd)
        {
            String temp = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                String sql = @"select UNIT_NAME from UNIT where UNIT_CD=@UNIT_CD ";
                com.CommandText = sql;
                //com.Parameters.Add("@UNIT_CD", unit_cd);
                DataUtils.AddParameters(com, "UNIT_CD", unit_cd);
                using (SqlDataReader sr = com.ExecuteReader())
                {
                    if (sr.Read())
                    {
                        temp = sr["UNIT_NAME"].ToString();
                    }
                    sr.Close();
                }
                conn.Close();
                conn.Dispose();
            }
            return temp;
        }

        /// <summary>
        /// 取得登入狀態
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static String GetLoginStatusName(String code)
        {
            String temp = "";
            switch (code)
            {
                case "O":
                    temp = "登出";
                    break;
                case "A":
                    temp = "登入失敗";
                    break;
                case "S":
                    temp = "登入成功";
                    break;
                case "H":
                    temp = "登入失敗";
                    break;
                case "X":
                    temp = "公衛登入";
                    break;
            }
            return temp;
        }

        /// <summary>
        /// 回傳繳費方式
        /// </summary>
        /// <param name="unit_cd"></param>
        /// <returns></returns>
        public static String GetPay_Method(String code)
        {
            String temp = "";
            switch (code)
            {
                case "C":
                    temp = "信用卡";
                    break;
                case "D":
                    temp = "匯(支)票";
                    break;
                case "T":
                    temp = "劃撥";
                    break;
                case "B":
                    temp = "臨櫃(現金)";
                    break;
                case "S":
                    temp = "超商";
                    break;
            }
            return temp;
        }

        public static String GetPayWord(Map m)
        {
            String PAY_POINT = m.GetString("PAY_POINT");
            int PAY_A_FEE = m.GetInt("PAY_A_FEE");
            int PAY_A_PAID = m.GetInt("PAY_A_PAID");
            int PAY_C_FEE = m.GetInt("PAY_C_FEE");
            int PAY_C_PAID = m.GetInt("PAY_C_FEE");
            bool ISSETTLE = m.GetBool("ISSETTLE");
            String CASE_BACK_MK = m.GetString("CASE_BACK_MK");
            String PAY_BACK_MK = m.GetString("PAY_BACK_MK");

            /*申請時繳費*/
            String temp = "";
            if (PAY_POINT.Equals("B") || PAY_POINT.Equals("D"))
            {//這個D 怪怪a
                if (PAY_A_FEE == PAY_A_PAID)
                {
                    if (ISSETTLE)
                    {
                        temp = "已入帳(申)";
                    }
                    else
                    {
                        temp = "已繳費(申)";
                    }
                }
                else
                {
                    if (PAY_A_PAID == 0)
                    {
                        if (CASE_BACK_MK.Equals("N"))
                        {
                            temp = "未繳費(申)";
                        }
                        else
                        {
                            temp = "未繳費卻退件(申)";
                        }
                    }
                    else
                    {
                        if (PAY_A_FEE > PAY_A_PAID)
                        {
                            temp = "補繳(申)";
                        }
                        else
                        {
                            if (CASE_BACK_MK.Equals("N"))
                            {
                                temp = "溢繳退費(申)";
                            }
                            else
                            {
                                if (PAY_BACK_MK.Equals("Y"))
                                    temp = "退件退費(申)";
                                else
                                    temp = "退件不退費(申)";
                            }
                        }
                    }
                }
            }
            /*審核後繳費*/
            if (PAY_POINT.Equals("C") || PAY_POINT.Equals("D"))
            {
                if (PAY_C_FEE == PAY_C_PAID)
                {
                    if (ISSETTLE)
                    {
                        temp += "已入帳(審)";
                    }
                    else
                    {
                        temp += "已繳費(審)";
                    }
                }
                else
                {
                    if (PAY_C_PAID == 0)
                    {
                        if (CASE_BACK_MK.Equals("N"))
                            temp += "未繳費(審)";
                        else
                            temp += "未繳費卻退件(審)";
                    }
                    else
                    {
                        if (PAY_C_FEE > PAY_C_PAID)
                        {
                            temp += "補繳(審)";
                        }
                        else
                        {
                            if (CASE_BACK_MK.Equals("N"))
                            {
                                temp += "溢繳退費(審)";
                            }
                            else
                            {
                                if (PAY_BACK_MK.Equals("Y"))
                                    temp += "退件退費(審)";
                                else
                                    temp += "退件不退費(審)";
                            }
                        }
                    }
                }
            }
            return temp;
        }

        public static String GetMaintainView_Paypoint(String paypoint, int pay_a_fee, int pay_c_fee)
        {
            string temp = "";
            switch (paypoint)
            {
                case "A":
                    temp = "不需繳費";
                    break;
                case "B":
                    temp = "申請時繳費：金額<font color=red>" + pay_a_fee + "</font>元整";
                    break;
                case "C":
                    temp = "審核後繳費：金額<font color=red>" + pay_c_fee + "</font>元整";
                    break;
                case "D":
                    temp = "申請時繳費：金額<font color=red>" + pay_a_fee + "</font>元整<br>";
                    temp += "審核後繳費：金額<font color=red>" + pay_c_fee + "</font>元整";
                    break;
            }
            return temp;
        }
        public static String GetMaintainView_PaidStatus(String paypoint, int pay_a_fee, int pay_c_fee, int pay_a_paid, int pay_c_paid, string case_back_mk, string pay_back_mk, out string paystatus)
        {
            String temp = "";
            paystatus = "";
            if (paypoint.Equals("B") || paypoint.Equals("D"))
            {
                if (pay_a_fee == pay_a_paid)
                {
                    temp = "申請時繳費：已繳費";
                    paystatus = "已繳費";
                }
                else
                {
                    if (pay_a_paid == 0)
                    {
                        if (case_back_mk.Equals("N"))
                        {
                            temp = "申請時繳費：未繳費";
                            paystatus = "未繳費";
                        }
                        else
                        {
                            temp = "申請時繳費：未繳費（退件）";
                        }
                    }
                    else
                    {
                        if (pay_a_fee > pay_a_paid)
                        {
                            temp = "申請時繳費：補繳";
                        }
                        else
                        {
                            if (case_back_mk.Equals("N"))
                            {
                                temp = "申請時繳費：溢繳退費";
                            }
                            else
                            {
                                if (pay_back_mk.Equals("Y"))
                                    temp = "申請時繳費：退件退費";
                                else
                                    temp = "申請時繳費：退件不退費";
                            }
                        }
                    }
                }
                if (paypoint.Equals("D")) temp += "<br>";
            }
            if (paypoint.Equals("C") || paypoint.Equals("D"))
            {
                if (pay_c_fee == pay_c_paid)
                {
                    temp += "審核後繳費：已繳費";
                    paystatus = "已繳費";
                }
                else
                {
                    if (pay_c_paid == 0)
                    {
                        if (case_back_mk.Equals("N"))
                            temp += "審核後繳費：未繳費";
                        else
                            temp += "審核後繳費：未繳費（退件）";
                    }
                    else
                    {
                        if (pay_c_fee > pay_c_paid)
                        {
                            temp += "審核後繳費：補繳";
                        }
                        else
                        {
                            if (case_back_mk.Equals("N"))
                            {
                                temp += "審核後繳費：溢繳退費";
                            }
                            else
                            {
                                if (pay_back_mk.Equals("Y"))
                                    temp += "審核後繳費：退件退費";
                                else
                                    temp += "審核後繳費：退件不退費";
                            }
                        }
                    }
                }
            }

            return temp;
        }

        public static String DefaultStringNumber(int bit, int number)
        {
            String temp = "";
            for (int i = number.ToString().Length; i < bit; i++)
            {
                temp += "0";
            }
            temp = temp + number.ToString();
            return temp;
        }


        public static String GetFormatReportPrintERR(String app_paypoint, int pay_a_fee, int pay_c_fee, int pay_a_paid, int pay_c_paid, string case_back_mk, string pay_back_mk, out int sum_expect, out int sum_diff)
        {
            string data = "";
            sum_expect = 0;
            sum_diff = 0;
            if (app_paypoint.Equals("B"))
            {
                sum_expect = pay_a_fee;
                if (pay_a_fee > pay_a_paid)
                {
                    sum_diff = pay_a_fee - pay_a_paid;
                    if (sum_diff == pay_a_fee)
                    {
                        if (case_back_mk.Equals("N"))
                            data = "未繳費";
                        else
                            data = "未繳費（退件）";
                    }
                    else
                        data = "補繳";
                }
                else
                {
                    sum_diff = pay_a_paid - pay_a_fee;
                    if (case_back_mk.Equals("N"))
                        data = "溢繳退費";
                    else
                    {
                        if (pay_back_mk.Equals("Y"))
                            data = "退件退費";
                        else
                            data = "退件不退費";
                    }
                }
            }
            else if (app_paypoint.Equals("C"))
            {
                sum_expect = pay_c_fee;
                if (pay_c_fee > pay_c_paid)
                {
                    sum_diff = pay_c_fee - pay_c_paid;
                    if (sum_diff == pay_c_fee)
                    {
                        if (case_back_mk.Equals("N"))
                            data = "未繳費";
                        else
                            data = "未繳費（退件）";
                    }
                    else
                        data = "補繳";
                }
                else
                {
                    sum_diff = pay_c_paid - pay_c_fee;
                    if (case_back_mk.Equals("N"))
                        data = "溢繳退費";
                    else
                    {
                        if (pay_back_mk.Equals("Y"))
                            data = "退件退費";
                        else
                            data = "退件不退費";
                    }
                }
            }
            else if (app_paypoint.Equals("D"))
            {
                sum_expect = pay_a_fee + pay_c_fee;
                sum_diff = pay_a_paid + pay_c_paid;
                if (sum_expect > sum_diff)
                {
                    sum_diff = sum_expect - sum_diff;
                    if (sum_diff == sum_expect)
                    {
                        if (case_back_mk.Equals("N"))
                            data = "未繳費";
                        else
                            data = "未繳費（退件）";
                    }
                    else
                        data = "補繳";
                }
                else
                {
                    sum_diff = sum_expect - sum_expect;
                    if (case_back_mk.Equals("N"))
                        data = "溢繳退費";
                    else
                    {
                        if (pay_back_mk.Equals("Y"))
                            data = "退件退費";
                        else
                            data = "退件不退費";
                    }
                }
            }
            else
            {
                sum_expect = pay_a_fee;
                if (pay_a_fee > pay_a_paid)
                {
                    sum_diff = pay_a_fee - pay_a_paid;
                    if (sum_diff == pay_a_fee)
                    {
                        if (case_back_mk.Equals("N"))
                            data = "未繳費";
                        else
                            data = "未繳費（退件）";
                    }
                    else
                        data = "補繳";
                }
                else
                {
                    sum_diff = pay_a_paid - pay_a_fee;
                    if (case_back_mk.Equals("N"))
                        data = "溢繳退費";
                    else
                    {
                        if (pay_back_mk.Equals("Y"))
                            data = "退件退費";
                        else
                            data = "退件不退費";
                    }
                }
            }
            return data;
        }

        public static String GetModifySearchType(int tx_type, string tx_cate_cd)
        {
            string temp = "";
            if (tx_type == 1)
                temp = "新增";
            else if (tx_type == 2)
                temp = "修改";
            else if (tx_type == 3)
                temp = "刪除";
            else if (tx_type == 4)
                temp = "退回重新分文";
            else if (tx_type == 5)
                temp = "修改Service_uid";
            else if (tx_type == 6)
            {
                if (tx_cate_cd.Equals("6"))
                {
                    temp = "異動繳費狀態";
                }
                else
                {
                    temp = "異動處理進度";
                }
            }
            else if (tx_type == 7)
                temp = "加入公衛平台";
            else if (tx_type == 8)
                temp = "取消公衛平台";
            return temp;
        }


        public static void Apply_Mail_Send(String app_id, string ClientUrl, string recommTex, int type)
        {
            Map map = null;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand com = conn.CreateCommand())
                {
                    String sql = @"select a.ACC_NO,a.APP_TIME,a.FLOW_CD,a.SRV_ID,s.NAME as SRV_NAME, 
                                    IsNull(a.NAME,'')+IsNull(a.CNT_NAME,'') as NAME,m.MAIL,IsNull(a.SEX_CD,''),mcs.FLOW_NAME,u.UNIT_NAME
                                    from APPLY a
                                    LEFT JOIN SERVICE s on s.SRV_ID = a.SRV_ID
                                    LEFT JOIN MEMBER m on m.ACC_NO = a.ACC_NO
                                    LEFT JOIN UNIT u on u.UNIT_CD = a.UNIT_CD
                                    LEFT JOIN M_CASE_STATUS mcs on mcs.UNIT_SCD = u.UNIT_SCD and a.FLOW_CD = mcs.FLOW_CD
                                    where mcs.CASE_CAT='4' and a.APP_ID = @APP_ID ";
                    com.CommandText = sql;
                    com.Parameters.Clear();
                    //com.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(com, "APP_ID", app_id);
                    using (SqlDataReader sr = com.ExecuteReader())
                    {
                        if (sr.Read())
                        {
                            map = new Map();
                            map.Add("ACC_NO", DataUtils.GetDBString(sr, 0));
                            map.Add("APP_TIME", DataUtils.GetDBDateTime(sr, 1));
                            map.Add("FLOW_CD", DataUtils.GetDBString(sr, 2));
                            map.Add("SRV_ID", DataUtils.GetDBString(sr, 3));
                            map.Add("SRV_NAME", DataUtils.GetDBString(sr, 4));
                            map.Add("NAME", DataUtils.GetDBString(sr, 5));
                            map.Add("MAIL", DataUtils.GetDBString(sr, 6));
                            map.Add("SEX_CD", DataUtils.GetDBString(sr, 7));
                            map.Add("FLOW_NAME", DataUtils.GetDBString(sr, 8));
                            map.Add("UNIT_NAME", DataUtils.GetDBString(sr, 9));
                        }
                        sr.Close();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (map != null)
                {
                    String body = "";
                    String subject = "";
                    switch (type)
                    {
                        case 0:
                            body = String.Format(MessageUtils.MAIL_ApplyNotification_BODY, map.Get("NAME"), map.GetDateTime("APP_TIME").Value.Year,
                                map.GetDateTime("APP_TIME").Value.Month, map.GetDateTime("APP_TIME").Value.Day,
                                map.GetString("SRV_NAME"),
                                ClientUrl, app_id, app_id, map.GetString("FLOW_NAME"), map.GetString("UNIT_NAME"));
                            subject = String.Format(MessageUtils.MAIL_ApplyNotification_SUBJECT, map.GetString("UNIT_NAME"));
                            break;
                        case 1:
                            body = String.Format(MessageUtils.MAIL_ApplyNoticeClose_BODY, map.Get("NAME"), map.GetDateTime("APP_TIME").Value.Year,
                                map.GetDateTime("APP_TIME").Value.Month, map.GetDateTime("APP_TIME").Value.Day, map.GetString("SRV_NAME"),
                                ClientUrl, app_id, app_id, map.GetString("FLOW_NAME"), recommTex, map.GetString("UNIT_NAME"));
                            subject = String.Format(MessageUtils.MAIL_ApplyNoticeClose_SUBJECT, map.GetString("UNIT_NAME"));
                            break;
                        case 2:
                            body = String.Format(MessageUtils.MAIL_ApplyNotice_BODY, map.Get("NAME"), map.GetDateTime("APP_TIME").Value.Year,
                                map.GetDateTime("APP_TIME").Value.Month, map.GetDateTime("APP_TIME").Value.Day, map.GetString("SRV_NAME"),
                                ClientUrl, app_id, app_id, map.GetString("FLOW_NAME"), recommTex, map.GetString("UNIT_NAME"));
                            subject = String.Format(MessageUtils.MAIL_ApplyNotice_SUBJECT, map.GetString("UNIT_NAME"));
                            break;
                        case 3:
                            body = String.Format(MessageUtils.MAIL_ApplyQUES_BODY, map.Get("NAME"), map.GetDateTime("APP_TIME").Value.Year,
                                map.GetDateTime("APP_TIME").Value.Month, map.GetDateTime("APP_TIME").Value.Day, map.GetString("SRV_NAME"),
                                ClientUrl, app_id, app_id, map.GetString("FLOW_NAME"), recommTex, ClientUrl, app_id, map.GetString("UNIT_NAME"));
                            subject = String.Format(MessageUtils.MAIL_ApplyQUES_SUBJECT, map.GetString("UNIT_NAME"));
                            break;

                    }
                    //MailUtils.SendMail(conn.BeginTransaction(), "nick@thinkon.com.tw", subject, body);
                    MailUtils.SendMail(map.GetString("MAIL"), subject, body);
                }
            }
        }

        public static void Dispatch_Mail_Send(String app_id)
        {
            Map map = null;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand com = conn.CreateCommand())
                {
                    String sql = @"select ad.NAME as DO_NAME,ad.MAIL as DO_MAIL,a.NAME as MEM_NAME,s.NAME as SRV_NAME,
                                    a.APP_TIME,addisp.NAME,a.SRV_ID
                                    from apply a
                                    LEFT JOIN ADMIN ad on a.PRO_ACC = ad.ACC_NO
                                    LEFT JOIN SERVICE s on a.SRV_ID = s.SRV_ID
                                    LEFT JOIN ADMIN addisp on a.APP_DISP_ACC = addisp.ACC_NO
                                    where a.APP_ID = @APP_ID ";
                    com.CommandText = sql;
                    com.Parameters.Clear();
                    //com.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(com, "APP_ID", app_id);
                    using (SqlDataReader sr = com.ExecuteReader())
                    {
                        if (sr.Read())
                        {
                            map = new Map();
                            map.Add("DO_NAME", DataUtils.GetDBString(sr, 0));
                            map.Add("DO_MAIL", DataUtils.GetDBString(sr, 1));
                            map.Add("MEM_NAME", DataUtils.GetDBString(sr, 2));
                            map.Add("SRV_NAME", DataUtils.GetDBString(sr, 3));
                            map.Add("APP_TIME", DataUtils.GetDBDateTime(sr, 4).HasValue ? DataUtils.GetDBDateTime(sr, 4).Value.ToString("yyyy/MM/dd") : "");
                            map.Add("NAME", DataUtils.GetDBString(sr, 5));
                            map.Add("SRV_ID", DataUtils.GetDBString(sr, 6));
                        }
                        sr.Close();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (map != null)
                {
                    String body = String.Format(MessageUtils.MAIL_Dispatch_BODY, map.GetString("DO_NAME"), map.GetString("MEM_NAME"),
                        map.GetString("APP_TIME"), map.GetString("SRV_NAME"), map.GetString("NAME")
                        );
                    String subject = MessageUtils.MAIL_Dispatch_SUBJECT;
                    //MailUtils.SendMail(conn.BeginTransaction(), "nick@thinkon.com.tw", subject, body);
                    MailUtils.SendMail(map.GetString("DO_MAIL"), subject, body, true);
                }
            }
        }

        public static void News_Mail_Send(String msg_id)
        {
            Map map = null;
            List<String> li = new List<String>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand com = conn.CreateCommand())
                {
                    String sql = @"
    select m.TITLE,m.CONTENT 
    ,case when u.UNIT_LEVEL = 1 THEN u.UNIT_NAME ELSE (select UNIT_NAME from UNIT where UNIT_CD = u.UNIT_PCD) end as NAME
    from MESSAGE m
    LEFT JOIN UNIT u on u.UNIT_CD = m.UNIT_CD
    where m.MSG_ID=@MSG_ID ";

                    com.Parameters.Clear();
                    if (msg_id.Equals("0"))
                    {
                        sql = sql.Replace("@MSG_ID", "ISNULL((SELECT MAX(MSG_ID) FROM MESSAGE), 0)");
                    }
                    else
                    {
                        //com.Parameters.Add("@MSG_ID", msg_id);
                        DataUtils.AddParameters(com, "MSG_ID", msg_id);
                    }
                    com.CommandText = sql;
                    using (SqlDataReader sr = com.ExecuteReader())
                    {
                        if (sr.Read())
                        {
                            map = new Map();
                            map.Add("TITLE", DataUtils.GetDBString(sr, 0));
                            map.Add("CONTENT", DataUtils.GetDBString(sr, 1));
                            map.Add("NAME", DataUtils.GetDBString(sr, 2));
                        }
                        sr.Close();
                    }

                    sql = @"select MAIL from MEMBER where MAIL_MK='Y' ";
                    com.CommandText = sql;
                    com.Parameters.Clear();
                    using (SqlDataReader sr = com.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            li.Add(DataUtils.GetDBString(sr, 0));
                        }
                        sr.Close();
                    }
                }
                if (map != null)
                {
                    String body = String.Format(MessageUtils.MAIL_NEWS_BODY, map.GetString("CONTENT"), map.GetString("NAME"));
                    String subject = String.Format(MessageUtils.MAIL_NEWS_SUBJECT, map.GetString("TITLE"));
                    //MailUtils.SendMail(conn.BeginTransaction(), "nick@thinkon.com.tw", subject, body);
                    foreach (String email in li)
                    {
                        MailUtils.SendMail(email, subject, body);
                    }
                }
                conn.Close();
                conn.Dispose();
            }
        }


    }
}