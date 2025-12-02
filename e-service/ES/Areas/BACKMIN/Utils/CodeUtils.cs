using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Mvc;
using ES.Utils;
using ES.Areas.Admin.Models;
using System.Text;

namespace ES.Areas.Admin.Utils
{
    public class CodeUtils : ES.Utils.CodeUtils
    {
        /// <summary>
        /// 取得單位下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="empty">是否產生空項目</param>
        /// <returns></returns>
        public static List<SelectListItem> GetUnitList(SqlConnection conn, bool empty)
        {
            return GetUnitList(conn, null, empty);
        }

        /// <summary>
        /// 取得單位下拉列表
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="value">預設選項</param>
        /// <param name="empty">是否產生空項目</param>
        /// <returns></returns>
        public static List<SelectListItem> GetUnitList(SqlConnection conn, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT UNIT_CD, UNIT_NAME FROM (
                            SELECT (CASE
                                    WHEN UNIT_LEVEL = 0 THEN SEQ_NO * 1000000
                                    WHEN UNIT_LEVEL = 1 THEN (SELECT SEQ_NO * 1000000 FROM UNIT WHERE UNIT_CD = U.UNIT_PCD) + SEQ_NO * 1000
                                    WHEN UNIT_LEVEL = 2 THEN (SELECT (SELECT SEQ_NO * 1000000 FROM UNIT WHERE UNIT_CD = PU.UNIT_PCD) + SEQ_NO * 1000 FROM UNIT AS PU WHERE UNIT_CD = U.UNIT_PCD) + SEQ_NO
                                END) AS SEQ, UNIT_CD, ISNULL((CASE
                                    WHEN UNIT_LEVEL = 1 THEN '　'
                                    WHEN UNIT_LEVEL = 2 THEN '　　'
                                END), '') + UNIT_NAME AS UNIT_NAME
                            FROM UNIT AS U WHERE DEL_MK = 'N'
                        ) T ORDER BY SEQ";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetInt32(0).ToString() };
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

        public static List<SelectListItem> GetUnitList(SqlConnection conn, int mainUnitCode, string[] values, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"
                SELECT UNIT_CD, UNIT_NAME 
                FROM UNIT AS U 
                WHERE DEL_MK = 'N' 
                    AND UNIT_PCD = @UNIT_CD
                UNION
                SELECT UNIT_CD, UNIT_NAME 
                FROM UNIT AS U 
                WHERE DEL_MK = 'N' 
                    AND U.UNIT_CD = @UNIT_CD and U.UNIT_PCD= '2'";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", mainUnitCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetInt32(0).ToString() };

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

        public static List<SelectListItem> GetUnitList(SqlConnection conn, int mainUnitCode, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT UNIT_CD, UNIT_NAME FROM (
                            SELECT (CASE
                                    WHEN UNIT_LEVEL = 0 THEN SEQ_NO * 1000000
                                    WHEN UNIT_LEVEL = 1 THEN (SELECT SEQ_NO * 1000000 FROM UNIT WHERE UNIT_CD = U.UNIT_PCD) + SEQ_NO * 1000
                                    WHEN UNIT_LEVEL = 2 THEN (SELECT (SELECT SEQ_NO * 1000000 FROM UNIT WHERE UNIT_CD = PU.UNIT_PCD) + SEQ_NO * 1000 FROM UNIT AS PU WHERE UNIT_CD = U.UNIT_PCD) + SEQ_NO
                                END) AS SEQ, UNIT_CD, ISNULL((CASE
                                    WHEN UNIT_LEVEL = 2 THEN '　'
                                END), '') + UNIT_NAME AS UNIT_NAME
                            FROM UNIT AS U WHERE DEL_MK = 'N' AND (UNIT_CD = @UNIT_CD OR UNIT_PCD = @UNIT_CD)
                        ) T ORDER BY SEQ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", mainUnitCode);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetInt32(0).ToString() };
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

        public static List<SelectListItem> GetUnitAdminSelectList(SqlConnection conn, int unitCode, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT A.ACC_NO, U.UNIT_NAME + ' - ' + A.NAME AS NAME
                            FROM ADMIN A, UNIT U
                            WHERE A.DEL_MK = 'N' AND A.UNIT_CD = U.UNIT_CD
                                AND (U.UNIT_CD = @UNIT_CD OR U.UNIT_PCD = @UNIT_CD)
                            ORDER BY U.SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", unitCode);
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

        public static List<SelectListItem> GetUnitAdminSelectList(SqlConnection conn, int unitCode, int menuId, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT A.ACC_NO, U.UNIT_NAME + ' - ' + A.NAME AS NAME
                            FROM ADMIN A, UNIT U
                            WHERE A.DEL_MK = 'N' AND A.UNIT_CD = U.UNIT_CD
                                AND (U.UNIT_CD = @UNIT_CD OR U.UNIT_PCD = @UNIT_CD)
                                AND EXISTS (SELECT ACC_NO FROM ADMIN_LEVEL WHERE ACC_NO = A.ACC_NO AND MN_ID = @MN_ID)
                            ORDER BY U.SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", unitCode);
            DataUtils.AddParameters(cmd, "MN_ID", menuId);
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

        public static List<SelectListItem> GetMCaseClassSelectList(SqlConnection conn, string unitSCode, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT CLASS_CD, CLASS_NAME FROM M_CASE_CLASS WHERE UNIT_SCD = @UNIT_SCD ORDER BY SEQ_NO, CLASS_CD";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_SCD", unitSCode);

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

        public static Dictionary<string, string[]> GetLevelItemList(SqlConnection conn)
        {
            Dictionary<string, string[]> d = new Dictionary<string, string[]>();

            List<string> code = new List<string>();
            List<string> text = new List<string>();

            string sql = @"SELECT MN_CD, MN_TEXT FROM MENU WHERE DEL_MK = 'N' AND LEN(MN_CD) > 0";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    code.Add(DataUtils.GetDBString(dr, 0));
                    text.Add(DataUtils.GetDBString(dr, 1));
                }

                d.Add("CODE", code.ToArray());
                d.Add("TEXT", text.ToArray()); 
                dr.Close();
            }

            return d;
        }

        /// <summary>
        /// 信用卡付款帳號
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static Dictionary<string, string[]> GetPayAccountList2(SqlConnection conn)
        {
            Dictionary<string, string[]> d = new Dictionary<string, string[]>();

            List<string> code = new List<string>();
            List<string> text = new List<string>();

            string sql = @"SELECT SRL_NO, PAY_DESC FROM PAY_ACCOUNT WHERE DEL_MK = 'N'";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    code.Add(DataUtils.GetDBString(dr, 0));
                    text.Add(DataUtils.GetDBString(dr, 1));
                }

                d.Add("CODE", code.ToArray());
                d.Add("TEXT", text.ToArray());
                dr.Close();
            }

            return d;
        }


        public static List<SelectListItem> GetPayAccountList(SqlConnection conn, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT SRL_NO, PAY_DESC FROM PAY_ACCOUNT WHERE DEL_MK = 'N'";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    SelectListItem item = new SelectListItem() { Text = dr.GetString(1), Value = dr.GetInt32(0).ToString() };
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

        public static List<SelectListItem> GetSharedList(SqlConnection conn, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT SRV_ID, NAME FROM SERVICE WHERE DEL_MK = 'N' AND SHARED_MK = 'Y'";

            SqlCommand cmd = new SqlCommand(sql, conn);

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

        public static List<SelectListItem> GetCase1List(SqlConnection conn, string value, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"SELECT SRV_ID_DONATE, NAME_CH FROM APPLY_DONATE WHERE DEL_MK = 'N'";

            SqlCommand cmd = new SqlCommand(sql, conn);

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

        public static List<Dictionary<String, Object>> GetMenuList(SqlConnection conn, int parentId, string account)
        {
            List<Dictionary<String, Object>> list = new List<Dictionary<string, object>>();
            Dictionary<String, Object> item = null;

            StringBuilder querySQL = new StringBuilder(@"
                SELECT MN_ID, MN_TYPE, MN_TEXT, MN_URL, MN_TARGET
                FROM ADMIN_MENU M
                WHERE MN_PID = @MN_PID
                  AND DEL_MK = 'N'");

            if (!String.IsNullOrEmpty(account))
            {
                querySQL.Append("AND (EXISTS (SELECT MN_ID FROM ADMIN_LEVEL WHERE ACC_NO = @ACC_NO AND MN_ID = M.MN_ID ) OR MN_ID IN (995, 1000))");
            }

            querySQL.Append("ORDER BY SEQ_NO");

            SqlCommand cmd = new SqlCommand(querySQL.ToString(), conn);
            DataUtils.AddParameters(cmd, "MN_PID", parentId);
            if (!String.IsNullOrEmpty(account))
            {
                DataUtils.AddParameters(cmd, "ACC_NO", account);
            }

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    int n = 0;
                    item = new Dictionary<string, object>();
                    item.Add("id", DataUtils.GetDBInt(dr, n++));
                    item.Add("isParent", DataUtils.GetDBString(dr, n++).Equals("F"));
                    item.Add("name", DataUtils.GetDBString(dr, n++));

                    if (!Convert.IsDBNull(DataUtils.GetDBString(dr, n + 1)))
                    {
                        item.Add("url", DataUtils.GetDBString(dr, n++));
                        item.Add("target", DataUtils.GetDBString(dr, n++));
                    }

                    list.Add(item);
                }
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得選單列表
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static List<MenuModel> GetMenuList(SqlConnection conn)
        {
            List<MenuModel> list = new List<MenuModel>();

            string sql = @"SELECT * FROM (
                            SELECT ISNULL((SELECT SEQ_NO FROM MENU WHERE MN_ID = MN.MN_PID) * 1000 + SEQ_NO, SEQ_NO * 1000) AS SEQ_NO,
                                MN_ID, MN_TEXT, MN_PID, MN_TYPE, MN_TARGET, MN_CONTROL, MN_ACTION, MN_CD
                            FROM MENU MN
                        ) T ORDER BY SEQ_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    MenuModel model = new MenuModel();
                    model.Id = DataUtils.GetDBInt(dr, 1);
                    model.Text = DataUtils.GetDBString(dr, 2);
                    model.ParentId = DataUtils.GetDBInt(dr, 3);
                    model.IsFolder = DataUtils.GetDBString(dr, 4).Equals("F");
                    model.Target = DataUtils.GetDBString(dr, 5);
                    model.Control = DataUtils.GetDBString(dr, 6);
                    model.Action = DataUtils.GetDBString(dr, 7);
                    model.Code = DataUtils.GetDBString(dr, 8);

                    list.Add(model);
                }
                dr.Close();
            }

            return list;
        }

        public static List<SelectListItem> GetUnitACCList(SqlConnection conn, int mainUnitCode, string[] values, bool empty)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            if (empty)
            {
                SelectListItem item = new SelectListItem() { Text = "--請選擇--", Value = "" };
                list.Add(item);
            }

            string sql = @"
SELECT CONVERT(NVARCHAR,ADMIN.ACC_NO) AS CODE_ID, CONVERT(NVARCHAR,ADMIN.NAME) AS CODE_TEXT
FROM ADMIN
LEFT JOIN UNIT AS U ON U.UNIT_CD = ADMIN.UNIT_CD
WHERE ADMIN.DEL_MK = 'N' 
AND UNIT_PCD = @UNIT_CD
UNION
SELECT CONVERT(NVARCHAR,ADMIN.ACC_NO) AS CODE_ID, CONVERT(NVARCHAR,ADMIN.NAME) AS CODE_TEXT
FROM ADMIN
LEFT JOIN UNIT AS U ON U.UNIT_CD = ADMIN.UNIT_CD
WHERE ADMIN.DEL_MK = 'N' 
AND U.UNIT_CD = @UNIT_CD AND U.UNIT_PCD= '2'";

            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "UNIT_CD", mainUnitCode);

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
    }
}