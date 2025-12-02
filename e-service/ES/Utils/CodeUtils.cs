using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Extensions;
using log4net;
using System.Text;
using System.Collections;
using System.Data;
using System.Configuration;

namespace ES.Utils
{
    public class CodeUtils
    {
        private static readonly ILog logger = LogUtils.GetLogger();

        public static string GetCodeDesc(SqlConnection conn, string kind, string parentCode, string value)
        {
            StringBuilder sb = new StringBuilder("SELECT ");
            sb.Append("CODE_DESC FROM CODE_CD ");
            sb.Append("WHERE CODE_KIND = @CODE_KIND ");
            if (parentCode != "")
            {
                sb.Append("AND CODE_PCD = @CODE_PCD ");
            }
            sb.Append("AND CODE_CD = @CODE_CD ");

            SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
            cmd.Parameters.AddWithValue("CODE_KIND", kind);
            if (parentCode != "")
            {
                cmd.Parameters.AddWithValue("CODE_PCD", parentCode);
            }
            cmd.Parameters.AddWithValue("CODE_CD", value);
            var result = string.Empty;
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    result= dr.GetString(0);
                }
                dr.Close();
            }

            return result;
        }

        /// <summary>
        /// 取得代碼下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="kind">類型</param>
        /// <param name="value">預設選項</param>
        /// <param name="empty">是否產生空項目</param>
        /// <returns></returns>
        public static List<SelectListItem> GetCodeSelectList(SqlConnection conn, string kind, string parentCode, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"
    SELECT CODE_CD, CODE_DESC 
    FROM CODE_CD 
    WHERE DEL_MK = 'N' 
    AND CODE_KIND = @CODE_KIND 
    AND CODE_PCD = @CODE_PCD 
    ORDER BY SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("CODE_KIND", kind);
            cmd.Parameters.AddWithValue("CODE_PCD", parentCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    //bool flag_Seled = false;, Selected = flag_Seled
                    string s_Val = dr.GetString(0);
                    string s_Txt = dr.GetString(1);
                    SelectListItem item = new SelectListItem() { Text = s_Txt, Value = s_Val };
                    if (value != null && value.Equals(s_Val)) { item.Selected = true; }
                    //logger.Debug("1: " + item.Value);
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得代碼下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="kind">類型</param>
        /// <param name="parentCode">父代碼</param>
        /// <param name="values">預設選項</param>
        /// <returns></returns>
        public static List<SelectListItem> GetCodeSelectList(SqlConnection conn, string kind, string parentCode, string[] values)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            string sql = @"SELECT CODE_CD, CODE_DESC FROM CODE_CD WHERE DEL_MK = 'N' AND CODE_KIND = @CODE_KIND AND CODE_PCD = @CODE_PCD ORDER BY SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("CODE_KIND", kind);
            cmd.Parameters.AddWithValue("CODE_PCD", parentCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetString(0) };
                    if (values != null && values.Contains(item.Value))
                    {
                        item.Selected = true;
                    }
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得數字下拉選單
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetNumSelectList(int start, int end, string values)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            for (int i = start; i <= end; i++)
            {
                SelectListItem item = new SelectListItem() { Text = i.ToString(), Value = i.ToString() };
                if (string.IsNullOrEmpty(values) && values == i.ToString())
                    item.Selected = true;
                list.Add(item);
            }
            return list;
        }


        public static List<SelectListItem> GetPayMethodSelectList(string values)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            string[] PayMethod = { "支票乙紙", "匯票乙紙", "現金" };
            string[] PayMethodValue = { "1", "2", "3" };
            for (int i = 0; i < PayMethod.Length; i++)
            {
                SelectListItem item = new SelectListItem() { Text = PayMethod[i], Value = PayMethodValue[i] };
                if (string.IsNullOrEmpty(values) && values == PayMethodValue[i])
                    item.Selected = true;
                list.Add(item);
            }

            return list;
        }


        public static List<SelectListItem> GetMaritalSelectList(string values)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            string[] PayMethod = { "已婚", "未婚" };
            string[] PayMethodValue = { "T", "F" };
            for (int i = 0; i < PayMethod.Length; i++)
            {
                SelectListItem item = new SelectListItem() { Text = PayMethod[i], Value = PayMethodValue[i] };
                if (string.IsNullOrEmpty(values) && values == PayMethodValue[i])
                    item.Selected = true;
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// 取得代碼CheckBox列表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="kind"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<CheckBoxListItem> GetCodeCheckBoxList(SqlConnection conn, string kind, string parentCode, string[] values)
        {
            List<CheckBoxListItem> list = new List<CheckBoxListItem>();

            string sql = @"SELECT CODE_CD, CODE_DESC FROM CODE_CD WHERE DEL_MK = 'N' AND CODE_KIND = @CODE_KIND AND CODE_PCD = @CODE_PCD ORDER BY SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("CODE_KIND", kind);
            cmd.Parameters.AddWithValue("CODE_PCD", parentCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    CheckBoxListItem item = new CheckBoxListItem() { Text = dr.GetString(1), Value = dr.GetString(0) };
                    if (values != null && values.Contains(item.Value))
                    {
                        item.Checked = true;
                    }
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得線上申辦分類下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="value">預設選項</param>
        /// <param name="empty">是否產生空項目</param>
        /// <returns></returns>
        public static List<SelectListItem> GetServiceCateList(SqlConnection conn, string value, bool empty)
        {
            if (value == null)
            {
                return GetServiceCateList(conn, -1, empty);
            }
            return GetServiceCateList(conn, Int32.Parse(value), empty);
        }

        /// <summary>
        /// 取得線上申辦分類下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="value">預設選項</param>
        /// <param name="empty">是否產生空項目</param>
        /// <returns></returns>
        public static List<SelectListItem> GetServiceCateList(SqlConnection conn, int value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @" SELECT SC_ID, NAME, SEQ, C1, C2
                            FROM (
	                            SELECT (CASE 
			                            WHEN LEVEL = 1 THEN SEQ_NO * 1000
			                            WHEN LEVEL = 2 THEN (SELECT SEQ_NO * 1000 FROM SERVICE_CATE WHERE SC_ID = C.SC_PID) + SEQ_NO
		                            END) AS SEQ, SC_ID, (CASE WHEN LEVEL = 2 THEN '　' ELSE '' END) + NAME AS NAME,
		                            (SELECT COUNT(1) FROM SERVICE WHERE DEL_MK = 'N' AND ONLINE_S_MK = 'Y' AND ONLINE_N_MK = 'Y' AND SC_ID = C.SC_ID) AS C1,
		                            (SELECT COUNT(1) FROM SERVICE WHERE DEL_MK = 'N' AND ONLINE_S_MK = 'Y' AND ONLINE_N_MK = 'Y' AND SC_ID IN (SELECT SC_ID FROM SERVICE_CATE WHERE SC_PID = C.SC_ID)) AS C2
	                            FROM SERVICE_CATE C
	                            WHERE DEL_MK = 'N'
                            ) T
                            WHERE ( C1 > 0 OR C2 > 0 )
                            ORDER BY SEQ";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetInt32(0).ToString() };
                    if (value.ToString().Equals(item.Text))
                    {
                        item.Selected = true;
                    }
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得案件下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="categoryId">分類代碼</param>
        /// <param name="value">預設選項</param>
        /// <param name="empty">是否產生空項目</param>
        /// <returns></returns>
        public static List<SelectListItem> GetServiceList(SqlConnection conn, string categoryId, string value, bool empty)
        {
            //logger.Debug("categoryId: " + categoryId);
            if (categoryId == null)
            {
                return GetServiceList(conn, -1, value, empty);
            }
            return GetServiceList(conn, Int32.Parse(categoryId), value, empty);
        }

        /// <summary>
        /// 取得案件下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="categoryId">分類代碼</param>
        /// <param name="value">預設選項</param>
        /// <param name="empty">是否產生空項目</param>
        /// <returns></returns>
        public static List<SelectListItem> GetServiceList(SqlConnection conn, int categoryId, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"
                SELECT SRV_ID, NAME, ( CASE
                    WHEN SC_ID = @SC_ID THEN SEQ_NO * 10000
                    ELSE ISNULL((SELECT SEQ_NO FROM SERVICE_CATE WHERE SC_ID = S.SC_ID), 0) * 1000 + SEQ_NO
                END) AS SEQ
                FROM SERVICE S
                WHERE DEL_MK = 'N' 
                    AND ONLINE_S_MK = 'Y' 
                    AND ONLINE_N_MK = 'Y'
                    AND (SC_ID = @SC_ID OR SC_PID = @SC_ID)
                ORDER BY 3";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SC_ID", categoryId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetString(0) };
                    if (value != null && value.Equals(item.Text))
                    {
                        item.Selected = true;
                    }
                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        public static Dictionary<string, string> GetSearchCode(SqlConnection conn, int serialNo)
        {
            Dictionary<string, string> item = new Dictionary<string, string>();

            string sql = @"
    SELECT TITLE, CLS_SUB_CD, CLS_ADM_CD, CLS_SRV_CD, KEYWORD 
    FROM CATE_SEARCH
    WHERE SRL_NO = @SRL_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "SRL_NO", serialNo);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        item.Add(dr.GetName(i), dr.GetString(i));
                    }
                }
                else
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        item.Add(dr.GetName(i), "");
                    }
                }
                dr.Close();
            }

            return item;
        }

        /// <summary>
        /// 取得空下拉列表
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetEmptySelectList()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
            list.Add(item);
            return list;
        }

        public static List<SelectListItem> GetServiceLst3(SqlConnection conn, string LST_ID, string VIL)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            //設定預設值
            LST_ID = (LST_ID ?? "4");

            string _sql = @"
    SELECT m.SRV_ID Value,m.NAME Text
    ,lt.ACT_TYPE,lt.LST_ID,m.SEQ_NO
    FROM SERVICE m
    JOIN SERVICE_LST lt on lt.SRV_ID=m.SRV_ID
    WHERE 1=1 
    AND m.DEL_MK = 'N'
    AND lt.DEL_MK = 'N'
    AND lt.ACT_TYPE= 3
    AND lt.LST_ID=@LST_ID
    ORDER BY m.SEQ_NO
    ";
            SqlCommand cmd = new SqlCommand(_sql, conn);
            cmd.Parameters.AddWithValue("LST_ID", LST_ID);
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetString(0), Selected = false };
                    if (VIL != null && VIL.Contains(item.Value)) { item.Selected = true; }
                    list.Add(item);
                }
                dr.Close();
            }
            return list;
        }

        public static string Utl_GetConfigSet(string sKey)
        {
            string rst = "";
            try
            {
                rst = ConfigurationManager.AppSettings[sKey];
            }
            catch (Exception) { }
            rst = rst ?? "";
            return rst;
        }
    }
}