using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ES.Areas.Admin.Models;
using System.Data.SqlClient;

namespace ES.Utils
{
    public class XMLToDOCUtils
    {
        public String InitFileXML(String path, String srv_id, string app_id)
        {
            String filepath = "";
            String temp = "";
            String XmlStr = "";
            switch (srv_id)
            {
                case "001037":
                    filepath = path + "\\Utils\\XML\\001037.xml";
                    break;
                case "001008_ME_1":
                    filepath = path + "\\Utils\\XML\\001008_prsn_separate.xml";
                    break;
                case "001008_ME_2":
                    filepath = path + "\\Utils\\XML\\001008_prsn_merge.xml";
                    break;
                case "001008_PR_1":
                    filepath = path + "\\Utils\\XML\\001008_docu_separate.xml";
                    break;
                case "001008_PR_2":
                    filepath = path + "\\Utils\\XML\\001008_docu_merge.xml";
                    break;
            }
            try
            {
                StreamReader sr = File.OpenText(filepath);
                while ((temp = sr.ReadLine()) != null)
                {
                    XmlStr += temp;
                }
            }
            catch (Exception ex)
            {
                XmlStr = ex.Message;
            }
            return XmlStr;
        }

        //還是拆報這個FUCTION叫好
        public String DoReplaceMark(String XmlStr, String srv_id, Map map)
        {
            string eng1, eng2;
            string gender_1, gender_2, gender_3;//(MR.,MALE,He)
            string app_time = "";
            string birthday = "";
            string issue_date = "";
            if (srv_id.Equals("001037"))
            {
                //轉換英文證明書
                ParseCodeCD(map.GetString("CODE_CD"), out eng1, out eng2);
                //轉換性別 稱謂 性別 人稱
                ParseSEXCD(map.GetString("SEX_CD"), out gender_1, out gender_2, out gender_3);
                //轉換時間格式
                app_time = ParseDate("1", map.GetDateTime("APP_TIME").Value);
                birthday = ParseDate("2", map.GetDateTime("BIRTHDAY").Value);
                issue_date = ParseDate("2", map.GetDateTime("ISSUE_DATE").Value);
                XmlStr = XmlStr.Replace("$A01$", app_time);
                XmlStr = XmlStr.Replace("$A02$", gender_1);
                XmlStr = XmlStr.Replace("$A03$", map.GetString("ENAME").Replace(" ", "  "));
                XmlStr = XmlStr.Replace("$A04$", map.GetString("NAME"));
                XmlStr = XmlStr.Replace("$A05$", gender_2);
                XmlStr = XmlStr.Replace("$A06$", birthday);
                XmlStr = XmlStr.Replace("$A07$", eng1);
                XmlStr = XmlStr.Replace("$A08$", map.GetString("LIC_NUM"));
                XmlStr = XmlStr.Replace("$A09$", eng2);
                XmlStr = XmlStr.Replace("$A10$", issue_date);
                XmlStr = XmlStr.Replace("$A11$", gender_3);
                XmlStr = XmlStr.Replace("$A12$", gender_3);
                XmlStr = XmlStr.Replace("$A13$", eng2);
            }
            if (srv_id.Equals("001008_ME_1"))
            {
                string eng, chi;
                Parse_ME_LIC_TYPE(map.GetString("LIC_TYPE"), out eng, out chi);
                //轉換性別 稱謂 性別 人稱
                ParseSEXCD(map.GetString("SEX_CD"), out gender_1, out gender_2, out gender_3);
                //轉換時間格式
                String now = ParseDate("1", DateTime.Now);
                if (map.GetDateTime("BIRTHDAY").HasValue)
                    birthday = ParseDate("2", map.GetDateTime("BIRTHDAY").Value);
                if (map.GetDateTime("ISSUE_DATE").HasValue)
                    issue_date = ParseDate("2", map.GetDateTime("ISSUE_DATE").Value);
                XmlStr = XmlStr.Replace("$A01$", chi);
                XmlStr = XmlStr.Replace("$A02$", map.GetString("ENAME"));
                XmlStr = XmlStr.Replace("$A03$", map.GetString("NAME"));
                XmlStr = XmlStr.Replace("$A04$", gender_2);
                XmlStr = XmlStr.Replace("$A05$", birthday);
                XmlStr = XmlStr.Replace("$A06$", eng);
                XmlStr = XmlStr.Replace("$A07$", map.GetString("SRL_NO"));
                XmlStr = XmlStr.Replace("$A08$", issue_date);
                XmlStr = XmlStr.Replace("$A09$", now);
            }
            if (srv_id.Equals("001008_ME_2_1"))
            {
                string eng, chi;
                Parse_ME_LIC_TYPE(map.GetString("LIC_TYPE"), out eng, out chi);
                //轉換性別 稱謂 性別 人稱
                ParseSEXCD(map.GetString("SEX_CD"), out gender_1, out gender_2, out gender_3);
                //轉換時間格式
                String now = ParseDate("1", DateTime.Now);
                if (map.GetDateTime("BIRTHDAY").HasValue)
                    birthday = ParseDate("2", map.GetDateTime("BIRTHDAY").Value);
                if (map.GetDateTime("ISSUE_DATE").HasValue)
                    issue_date = ParseDate("2", map.GetDateTime("ISSUE_DATE").Value);

                XmlStr = XmlStr.Replace("$A01$", now);
                XmlStr = XmlStr.Replace("$A02$", map.GetString("ENAME"));
                XmlStr = XmlStr.Replace("$A03$", map.GetString("NAME"));
                XmlStr = XmlStr.Replace("$A04$", gender_2);
                XmlStr = XmlStr.Replace("$A05$", birthday);
                XmlStr = XmlStr.Replace("$A06$", eng);
                XmlStr = XmlStr.Replace("$A07$", map.GetString("SRL_NO"));
                XmlStr = XmlStr.Replace("$A08$", issue_date);
                //XmlStr = XmlStr.Replace("$A09$", eng);
                //XmlStr = XmlStr.Replace("$A10$", map.GetString("SRL_NO"));
                //XmlStr = XmlStr.Replace("$A11$", issue_date);
                //XmlStr = XmlStr.Replace("$A12$", eng);
                //XmlStr = XmlStr.Replace("$A13$", map.GetString("SRL_NO"));
                //XmlStr = XmlStr.Replace("$A14$", issue_date);
                
            }
            if (srv_id.Equals("001008_ME_2_2"))
            {
                string eng, chi;
                Parse_ME_LIC_TYPE(map.GetString("LIC_TYPE"), out eng, out chi);
                if (map.GetDateTime("ISSUE_DATE").HasValue)
                    issue_date = ParseDate("2", map.GetDateTime("ISSUE_DATE").Value);
                XmlStr = XmlStr.Replace("$A09$", eng);
                XmlStr = XmlStr.Replace("$A10$", map.GetString("SRL_NO"));
                XmlStr = XmlStr.Replace("$A11$", issue_date);

            }
            if (srv_id.Equals("001008_ME_2_3"))
            {
                string eng, chi;
                Parse_ME_LIC_TYPE(map.GetString("LIC_TYPE"), out eng, out chi);
                if (map.GetDateTime("ISSUE_DATE").HasValue)
                    issue_date = ParseDate("2", map.GetDateTime("ISSUE_DATE").Value);
                XmlStr = XmlStr.Replace("$A12$", eng);
                XmlStr = XmlStr.Replace("$A13$", map.GetString("SRL_NO"));
                XmlStr = XmlStr.Replace("$A14$", issue_date);
            }
            return XmlStr;
        }

        public String DoReplaceMark2(String XmlStr, Map map_me, Map map_pr)
        {
            //string eng1, eng2;
            string gender_1, gender_2, gender_3;//(MR.,MALE,He)
            //string app_time = "";
            string birthday = "";
            string issue_date = "";

            string eng, chi;
            Parse_ME_LIC_TYPE(map_me.GetString("LIC_TYPE"), out eng, out chi);
            //轉換性別 稱謂 性別 人稱
            ParseSEXCD(map_me.GetString("SEX_CD"), out gender_1, out gender_2, out gender_3);
            //轉換時間格式
            String now = ParseDate("1", DateTime.Now);
            if (map_me.GetDateTime("BIRTHDAY").HasValue)
                birthday = ParseDate("2", map_me.GetDateTime("BIRTHDAY").Value);
            if (map_me.GetDateTime("ISSUE_DATE").HasValue)
                issue_date = ParseDate("2", map_me.GetDateTime("ISSUE_DATE").Value);

            string dates = "";
            if (map_pr.GetDateTime("EF_DATE_S").HasValue)
                dates = ParseDate("2", map_pr.GetDateTime("EF_DATE_S").Value);

            string datee = "";
            if (map_pr.GetDateTime("EF_DATE_E").HasValue)
                datee = ParseDate("2", map_pr.GetDateTime("EF_DATE_E").Value);
            string issue_date2 = "";
            if (map_pr.GetDateTime("ISSUE_DATE").HasValue)
                issue_date2 = ParseDate("2", map_pr.GetDateTime("ISSUE_DATE").Value);
            string eng3="";
            Parse_PR_LIC_TYPE(map_pr.GetString("LIC_TYP"), out eng3);

            XmlStr = XmlStr.Replace("$A01$", chi);
            XmlStr = XmlStr.Replace("$A02$", map_me.GetString("ENAME"));
            XmlStr = XmlStr.Replace("$A03$", map_me.GetString("NAME"));
            XmlStr = XmlStr.Replace("$A04$", gender_2);
            XmlStr = XmlStr.Replace("$A05$", birthday);
            XmlStr = XmlStr.Replace("$A06$", eng);
            XmlStr = XmlStr.Replace("$A07$", map_me.GetString("SRL_NO"));
            XmlStr = XmlStr.Replace("$A08$", issue_date);
            XmlStr = XmlStr.Replace("$A09$", eng3);
            XmlStr = XmlStr.Replace("$A10$", map_pr.GetString("SRL_NO"));
            XmlStr = XmlStr.Replace("$A11$", issue_date2);
            XmlStr = XmlStr.Replace("$A12$", dates + " to " + datee);
            XmlStr = XmlStr.Replace("$A13$", now);
            return XmlStr;
        }

        public String DoReplaceMark3(String XmlStr,int i, Map map_me, Map map_pr)
        {
            //string eng1, eng2;
            string gender_1, gender_2, gender_3;//(MR.,MALE,He)
            //string app_time = "";
            string birthday = "";
            string issue_date = "";
            string eng, chi;
            Parse_ME_LIC_TYPE(map_me.GetString("LIC_TYPE"), out eng, out chi);
            //轉換性別 稱謂 性別 人稱
            ParseSEXCD(map_me.GetString("SEX_CD"), out gender_1, out gender_2, out gender_3);
            //轉換時間格式
            String now = ParseDate("1", DateTime.Now);
            if (map_me.GetDateTime("BIRTHDAY").HasValue)
                birthday = ParseDate("2", map_me.GetDateTime("BIRTHDAY").Value);
            if (map_me.GetDateTime("ISSUE_DATE").HasValue)
                issue_date = ParseDate("2", map_me.GetDateTime("ISSUE_DATE").Value);


            string dates = "";
            if (map_pr.GetDateTime("EF_DATE_S").HasValue)
                dates = ParseDate("2", map_pr.GetDateTime("EF_DATE_S").Value);

            string datee = "";
            if (map_pr.GetDateTime("EF_DATE_E").HasValue)
                datee = ParseDate("2", map_pr.GetDateTime("EF_DATE_E").Value);
            string issue_date2 = "";
            if (map_pr.GetDateTime("ISSUE_DATE").HasValue)
                issue_date2 = ParseDate("2", map_pr.GetDateTime("ISSUE_DATE").Value);
            string eng3 = "";
            Parse_PR_LIC_TYPE(map_pr.GetString("LIC_TYP"), out eng3);

            switch (i)
            {
                case 1:
                    XmlStr = XmlStr.Replace("$A01$", chi);
                    XmlStr = XmlStr.Replace("$A02$", now);
                    XmlStr = XmlStr.Replace("$A03$", map_me.GetString("ENAME"));
                    XmlStr = XmlStr.Replace("$A04$", map_me.GetString("NAME"));
                    XmlStr = XmlStr.Replace("$A05$", gender_2);
                    XmlStr = XmlStr.Replace("$A06$", birthday);
                    XmlStr = XmlStr.Replace("$A07$", eng);
                    XmlStr = XmlStr.Replace("$A08$", map_me.GetString("SRL_NO"));
                    XmlStr = XmlStr.Replace("$A09$", issue_date);
                    XmlStr = XmlStr.Replace("$A10$", eng3);
                    XmlStr = XmlStr.Replace("$A11$", map_pr.GetString("SRL_NO"));
                    XmlStr = XmlStr.Replace("$A12$", issue_date2);
                    XmlStr = XmlStr.Replace("$A13$", dates + " to " + datee);
                     XmlStr = XmlStr.Replace("$A14$", "");
                    XmlStr = XmlStr.Replace("$A08$", map_pr.GetString("SRL_NO"));
                    XmlStr = XmlStr.Replace("$A09$", issue_date2);
                    break;
                case 2:
                    XmlStr = XmlStr.Replace("$A14$", eng3);
                    XmlStr = XmlStr.Replace("$A15$", map_pr.GetString("SRL_NO"));
                    XmlStr = XmlStr.Replace("$A16$", issue_date2);
                    XmlStr = XmlStr.Replace("$A17$", dates + " to " + datee);
                    break;
            }
            
            return XmlStr;
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public Map GetAPPLY_001037Data(string app_id)
        {
            Map map = new Map();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"SELECT d.APP_TIME ,d.NAME,d.ENAME,d.SEX_CD,d.BIRTHDAY,
                                    c.CODE_CD,a.LIC_NUM,a.ISSUE_DATE
                                    FROM APPLY_001037 a
                                    LEFT JOIN CODE_CD b on b.code_kind = 'F_LICENSE_CD_1' and a.LIC_CD = b.CODE_CD
                                    LEFT JOIN CODE_CD c on c.code_kind = 'F_LICENSE_CD_1' and c.CODE_CD = b.CODE_PCD
                                    LEFT JOIN APPLY d on a.APP_ID = d.APP_ID
                                    WHERE a.APP_ID = @APP_ID
                                        ";
                    dbc.Parameters.Clear();
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            map.Add("APP_TIME", DataUtils.GetDBDateTime(sda, 0));
                            map.Add("NAME", DataUtils.GetDBString(sda, 1));
                            map.Add("ENAME", DataUtils.GetDBString(sda, 2));
                            map.Add("SEX_CD", DataUtils.GetDBString(sda, 3));
                            map.Add("BIRTHDAY", DataUtils.GetDBDateTime(sda, 4));
                            map.Add("CODE_CD", DataUtils.GetDBString(sda, 5));
                            map.Add("LIC_NUM", DataUtils.GetDBString(sda, 6));
                            map.Add("ISSUE_DATE", DataUtils.GetDBDateTime(sda, 7));
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return map;
        }

        //private Map GetAPPLY_001008Data(string app_id)
        //{
        //    Map map = new Map();
        //    using (SqlConnection conn = DataUtils.GetConnection())
        //    {
        //        conn.Open();
        //        using (SqlCommand dbc = conn.CreateCommand())
        //        {
        //            String sql = @"SELECT NAME,ENAME,SEC_CD,BIRTHDAY FROM APPLY WHERE APP_ID = @APP_ID";
        //            dbc.Parameters.Clear();
        //            dbc.Parameters.Add("@APP_ID", app_id);
        //            dbc.CommandText = sql;
        //            using (SqlDataReader sda = dbc.ExecuteReader())
        //            {
        //                if (sda.Read())
        //                {
        //                    map.Add("NAME", DataUtils.GetDBString(sda, 0));
        //                    map.Add("ENAME", DataUtils.GetDBString(sda, 1));
        //                    map.Add("SEX_CD", DataUtils.GetDBString(sda, 2));
        //                    map.Add("BIRTHDAY", DataUtils.GetDBDateTime(sda, 3));
        //                }
        //            }
        //        }
        //    }
        //    return map;
        //}

        public int GetAPPLY_001008Type(string target, string app_id, string type_cd)
        {
            int count = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = @"select count(*) as cont from APPLY_001008_" + target + " where APP_ID=@APP_ID and " + target + "_TYPE_CD !=@" + target + "_TYPE_CD";
                    dbc.Parameters.Clear();
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    DataUtils.AddParameters(dbc, target + "_TYPE_CD", type_cd);
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    //dbc.Parameters.Add("@" + target + "_TYPE_CD", type_cd);
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        if (sda.Read())
                        {
                            count = DataUtils.GetDBInt(sda, 0);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return count;
        }

        public List<Map> GetAPPLY_001008ME(string app_id)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = "select ME_LIC_TYPE,SRL_NO,ME_ISSUE_DATE,a.NAME,a.ENAME,a.SEX_CD,a.BIRTHDAY " +
                    "from APPLY_001008_ME aa left join apply a on a.app_id =aa.app_id where aa.APP_ID=@APP_ID";
                    dbc.Parameters.Clear();
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("LIC_TYPE", DataUtils.GetDBString(sda, 0));
                            map.Add("SRL_NO", DataUtils.GetDBInt(sda, 1));
                            map.Add("ISSUE_DATE", DataUtils.GetDBDateTime(sda, 2));
                            map.Add("NAME", DataUtils.GetDBString(sda, 3));
                            map.Add("ENAME", DataUtils.GetDBString(sda, 4));
                            map.Add("SEX_CD", DataUtils.GetDBString(sda, 5));
                            map.Add("BIRTHDAY", DataUtils.GetDBDateTime(sda, 6));
                            li.Add(map);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        public List<Map> GetAPPLY_001008PR(string app_id)
        {
            List<Map> li = new List<Map>();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand dbc = conn.CreateCommand())
                {
                    String sql = "select PR_LIC_TYPE,SRL_NO,PR_ISSUE_DATE,PR_EF_DATE_S,PR_EF_DATE_E from APPLY_001008_PR where APP_ID=@APP_ID ";
                    dbc.Parameters.Clear();
                    //dbc.Parameters.Add("@APP_ID", app_id);
                    DataUtils.AddParameters(dbc, "APP_ID", app_id);
                    dbc.CommandText = sql;
                    using (SqlDataReader sda = dbc.ExecuteReader())
                    {
                        while (sda.Read())
                        {
                            Map map = new Map();
                            map.Add("LIC_TYPE", DataUtils.GetDBString(sda, 0));
                            map.Add("SRL_NO", DataUtils.GetDBInt(sda, 1));
                            map.Add("ISSUE_DATE", DataUtils.GetDBDateTime(sda, 2));
                            map.Add("EF_DATE_S", DataUtils.GetDBDateTime(sda, 3));
                            map.Add("EF_DATE_E", DataUtils.GetDBDateTime(sda, 4));
                            li.Add(map);
                        }
                        sda.Close();
                    }
                }
                conn.Close();
                conn.Dispose();
            }
            return li;
        }

        private void ParseCodeCD(string val, out string eng1, out string eng2)
        {
            eng1 = "";
            eng2 = "";
            switch (val)
            {
                case "A":
                    eng1 = "Physician";
                    eng2 = "Physicians  Act";
                    break;
                case "B":
                    eng1 = "Doctor  of  Chinese  Medicine";
                    eng2 = "Physicians  Act";
                    break;
                case "C":
                    eng1 = "Physician";
                    eng2 = "Physicians  Act";
                    break;
                case "D":
                    eng1 = "Pharmacist";
                    eng2 = "Pharmacist  Act";
                    break;
                case "F":
                    eng1 = "Registered  Professional  Nurse";
                    eng2 = "Nursing  Personnel  Law";
                    break;
                case "G":
                    eng1 = "Registered  Nurse";
                    eng2 = "Nursing  Personnel  Law";
                    break;
            }
        }

        private void ParseSEXCD(string val, out string gender_1, out string gender_2, out string gender_3)
        {
            gender_1 = "";
            gender_2 = "";
            gender_3 = "";
            switch (val)
            {
                case "M":
                    gender_1 = "Mr.";
                    gender_2 = "male";
                    gender_3 = "He";
                    break;
                case "F":
                    gender_1 = "Mrs.";
                    gender_2 = "female";
                    gender_3 = "She";
                    break;
            }
        }

        private string ParseDate(string val, DateTime dt)
        {
            string temp = "";
            switch (val)
            {
                case "1":
                    temp = dt.ToString("M", System.Globalization.CultureInfo.CreateSpecificCulture("en-us"))
                        + " " + dt.ToString("dd yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                    break;
                case "2":
                    temp = dt.ToString("M", System.Globalization.CultureInfo.CreateSpecificCulture("en-us")).Substring(0, 3)
                        + ". " + dt.ToString("dd yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-us"));
                    break;
            }
            return temp;
        }

        private void Parse_ME_LIC_TYPE(string val, out string eng1, out string eng2)
        {
            eng1 = "";
            eng2 = "";
            switch (val)
            {
                case "A":
                    eng1 = "Physician";
                    eng2 = "醫師";
                    break;
                case "B":
                    eng1 = "Doctor  of  Chinese  Medicine";
                    eng2 = "中醫師";
                    break;
                case "C":
                    eng1 = "Dentist";
                    eng2 = "牙醫師";
                    break;
                case "D":
                    eng1 = "Pharmacist";
                    eng2 = "藥師";
                    break;
                case "E":
                    eng1 = "Assistant Pharmacist";
                    eng2 = "藥劑生";
                    break;
                case "F":
                    eng1 = "Registered Professional Nurse";
                    eng2 = "護理師";
                    break;
                case "G":
                    eng1 = "Registered Nurse";
                    eng2 = "護士";
                    break;
                case "H":
                    eng1 = "Midwife";
                    eng2 = "助產士";
                    break;
                case "I":
                    eng1 = "Registered Professional Midwife";
                    eng2 = "助產師";
                    break;
                case "J":
                    eng1 = "Medical Technologist";
                    eng2 = "醫事檢驗師";
                    break;
                case "K":
                    eng1 = "Medical Tecnician";
                    eng2 = "醫事檢驗生";
                    break;
                case "O":
                    eng1 = "Occupational Therapy Technician";
                    eng2 = "專科護理師";
                    break;
                case "Q":
                    eng1 = "Physical Therapist";
                    eng2 = "物理治療師";
                    break;
                case "U":
                    eng1 = "Physical Therapy Technician";
                    eng2 = "物理治療生";
                    break;
                case "R":
                    eng1 = "Occupational Therapist";
                    eng2 = "職能治療師";
                    break;
                case "W":
                    eng1 = "Occupational Therapy Technician";
                    eng2 = "職能治療生";
                    break;
                case "S":
                    eng1 = "Registered Professional Nurse";
                    eng2 = "醫事放射師";
                    break;
                case "T":
                    eng1 = "Medical Radiological Technician";
                    eng2 = "醫事放射士";
                    break;
                case "V":
                    eng1 = "Respiratory Therapist";
                    eng2 = "呼吸治療師";
                    break;
                case "X":
                    eng1 = "Counseling Psychologist";
                    eng2 = "諮商心理師";
                    break;
                case "Y":
                    eng1 = "Clinical Psychologist";
                    eng2 = "臨床心理師";
                    break;
                case "Z":
                    eng1 = "Dietitian";
                    eng2 = "營養師";
                    break;
                case "28":
                    eng1 = "Speech Therapist";
                    eng2 = "護理師";
                    break;
                case "29":
                    eng1 = "Dental Technician";
                    eng2 = "牙體技術師";
                    break;
                case "30":
                    eng1 = "Audiologist";
                    eng2 = "聽力師";
                    break;
            }
        }


        private void Parse_PR_LIC_TYPE(string val, out string eng)
        {
            eng = "";
            switch (val)
            {
                case "A0100":
                    eng = "Family Medicine";
                    break;
                case "A0200":
                    eng = "Internal Medicine";
                    break;
                case "A0300":
                    eng = "Surgery";
                    break;
                case "A0301":
                    eng = "Plastic Surgery";
                    break;
                case "A0400":
                    eng = "Pediatrics";
                    break;
                case "A0500":
                    eng = "Obstetrics & Gynecology";
                    break;
                case "A0600":
                    eng = "Orthopedics";
                    break;
                case "A0700":
                    eng = "Neurology";
                    break;
                case "A0800":
                    eng = "Neurosurgery";
                    break;
                case "A0900":
                    eng = "Urology";
                    break;
                case "A1000":
                    eng = "Otolaryngology";
                    break;
                case "A1100":
                    eng = "Ophthalmology";
                    break;
                case "A1200":
                    eng = "Dermatology";
                    break;
                case "A1300":
                    eng = "Psychiatry";
                    break;
                case "A1400":
                    eng = "Rehabilitation Medicine";
                    break;
                case "A1500":
                    eng = "Anesthesiology";
                    break;
                case "A1600":
                    eng = "Diagnostic Radiology";
                    break;
                case "A1601":
                    eng = "Radiation Oncology";
                    break;
                case "A2000":
                    eng = "Anatomical Pathology";
                    break;
                case "A2010":
                    eng = "Clinical Pathology";
                    break;
                case "A2100":
                    eng = "Nuclear Medicine";
                    break;
                case "A2200":
                    eng = "Emergency Medicine";
                    break;
                case "A2400":
                    eng = "Occupational Medicine";
                    break;
                case "C0700":
                    eng = "Oral-Maxillofacial Surgery";
                    break;
                case "C0900":
                    eng = "Oral Pathology";
                    break;
                case "A0302":
                    eng = "Surgery Professional Nurse";
                    break;
                case "A0201":
                    eng = "Internal Medicine Registered Professional Nurse";
                    break;
            }
        }

    }
}