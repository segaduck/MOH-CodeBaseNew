using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using log4net;
using System.IO;
using ES.Utils;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace ES.Action
{
    public class FormAction : BaseAction
    {
        private IFormatProvider cultureStyle = new System.Globalization.CultureInfo("zh-TW", true);

        /// <summary>
        /// 線上申請
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        public FormAction(SqlConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// 線上申請
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <param name="tran">資料庫交易</param>
        public FormAction(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }


        public Dictionary<string, string> GetMember(string account)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string sql = @"
                SELECT ACC_NO, IDN, SEX_CD, CONVERT(CHAR(10), BIRTHDAY, 111) AS BIRTHDAY,
                    NAME, ENAME, CNT_NAME, CNT_ENAME, CHR_NAME, CHR_ENAME,
                    TEL, FAX, CNT_TEL, MAIL, 
                    ISNULL((SELECT CODE_DESC FROM CODE_CD WHERE CODE_KIND = 'TOWN_CD' AND CODE_CD = M.CITY_CD), '') +
                    ISNULL((SELECT CODE_DESC FROM CODE_CD WHERE CODE_KIND = 'TOWN_CD' AND CODE_CD = M.TOWN_CD), '') + ADDR AS ADDR, 
                    EADDR, MEDICO, MOBILE
                FROM MEMBER M
                WHERE M.ACC_NO = @ACC_NO";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("ACC_NO", account);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dict.Add(dr.GetName(i), DataUtils.DecodeValue(dr.GetValue(i).ToString()));
                    }
                }
                dict.Add("APPLY_DATE", DateTime.Now.ToString("yyyy/MM/dd"));
                dr.Close();
            }

            //logger.Debug("ADDR: " + dict["ADDR"]);

            return dict;
        }

        /// <summary>
        /// 取得表單基本資料
        /// </summary>
        /// <param name="account"></param>
        /// <param name="applyId"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetFormBase(string serviceId)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string sql = @"
                SELECT S.SRV_ID, S.NAME, S.SRV_DESC,
                    S.APP_FEE, S.CHK_FEE, S.PAY_POINT, S.PAY_UNIT, S.CHK_PAY_UNIT,
                    S.PAY_DEADLINE, S.BASE_NUM, S.CHK_FEE_EXTRA, S.FEE_EXTRA, S.CHK_BASE_NUM,
                    S.PAY_RULE_FIELD, S.CHK_PAY_RULE_FIELD, S.PAY_METHOD, S.RPT_NAME,
                    S.CA_TYPE, S.APP_TARGET, S.TRAN_ARCHIVE_MK, C.UNIT_CD, S.PAY_ACCOUNT,
                    (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = C.UNIT_CD) AS UNIT_NAME,
                    CONVERT(VARCHAR(MAX), (SELECT COUNT(1) FROM FORM_TABLE WHERE SRV_ID = S.SRV_ID)) AS TABLE_COUNT,
                    ISNULL((SELECT SRV_FIELD FROM SERVICE_FORM WHERE (SRV_ID = S.SRV_ID AND ISNULL(S.FORM_ID, '') = '') OR (SRV_ID = S.FORM_ID)), '') AS SRV_FIELD,
                    ISNULL((SELECT SRV_SCRIPT FROM SERVICE_FORM WHERE (SRV_ID = S.SRV_ID AND ISNULL(S.FORM_ID, '') = '') OR (SRV_ID = S.FORM_ID)), '') AS SRV_SCRIPT,
                    ISNULL((SELECT PRE_SCRIPT FROM SERVICE_FORM WHERE (SRV_ID = S.SRV_ID AND ISNULL(S.FORM_ID, '') = '') OR (SRV_ID = S.FORM_ID)), '') AS PRE_SCRIPT
                FROM SERVICE S, SERVICE_CATE C
                WHERE S.SC_ID = C.SC_ID AND S.SRV_ID = @SRV_ID
            ";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dict.Add(dr.GetName(i), DataUtils.DecodeValue(dr.GetValue(i).ToString()));
                    }
                }
                dr.Close();
            }

            return dict;
        }


        /// <summary>
        /// 取得所有刪除的付款方式
        /// </summary>
        /// <returns></returns>
        public List<string> GetDelPayMethod()
        {
            List<string> list = new List<string>();

            string sql = @"
                select CODE_CD,CODE_DESC from CODE_CD where CODE_KIND = 'PAY_METHOD' and del_mk='Y'
            ";

            SqlCommand cmd = new SqlCommand(sql, conn, tran);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                    list.Add(dr[0].ToString());
                dr.Close();
            }

            return list;
        }

        /// <summary>
        /// 取得管理人員列表
        /// </summary>
        /// <param name="unitCode">單位</param>
        /// <param name="menuId">權限 (151: 分文處理 / 152: 案件處理)</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetAdminList(string serviceId, int menuId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.ACC_NO, A.NAME, A.MAIL
                FROM ADMIN A, UNIT U
                WHERE A.DEL_MK = 'N' AND A.UNIT_CD = U.UNIT_CD
                    AND U.UNIT_CD IN (SELECT UNIT_CD FROM SERVICE_UNIT WHERE SRV_ID = @SRV_ID)
                    AND EXISTS (SELECT ACC_NO FROM ADMIN_LEVEL WHERE ACC_NO = A.ACC_NO AND MN_ID = @MN_ID)
                ORDER BY U.SEQ_NO
            ";

            args.Add("SRV_ID", serviceId);
            args.Add("MN_ID", menuId);

            return GetList(querySQL, args);
        }

        /*
        /// <summary>
        /// 取得表格定義列表
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> GetFormTableList2(string serviceId)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            string sql = @"
                SELECT SRV_ID, TABLE_ID, TABLE_TYPE, TABLE_TITLE, TABLE_DESC, TABLE_DB_NAME
                FROM FORM_TABLE
                WHERE DEL_MK = 'N' AND SRV_ID = @SRV_ID
                ORDER BY SEQ_NO
            ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dict.Add(dr.GetName(i), dr.GetValue(i).ToString());
                    }
                    list.Add(dict);
                }
            }

            //logger.Debug(list.Count);

            return list;
        }
        */
        /*
        /// <summary>
        /// 取得欄位定義資料列表
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public List<Dictionary<string, string>> GetFormFieldList2(string serviceId, string tableId)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            string sql = @"
                SELECT SRV_ID, TABLE_ID, FIELD_ID, FIELD_NAME, FIELD_TITLE,
                    FIELD_LEFT, FIELD_RIGHT, FIELD_DEF, FIELD_STYLE, FIELD_LIMIT,
                    FIELD_ATTR,
                    TITLE_STYLE, TITLE_ATTR, TITLE_LEFT, TITLE_RIGHT,
                    REQ_MK, FORM_FIELD_CD, CODE_CD, REL_1, REL_2, PREVIEW_MK,
                    (SELECT TABLE_TYPE FROM FORM_TABLE WHERE TABLE_ID = F.TABLE_ID AND SRV_ID = F.SRV_ID) AS TABLE_TYPE
                FROM FORM_FIELD F
                WHERE DEL_MK = 'N'
                    AND TABLE_ID = @TABLE_ID
                    AND SRV_ID = @SRV_ID
                ORDER BY SEQ_NO
            ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("SRV_ID", serviceId);
            cmd.Parameters.AddWithValue("TABLE_ID", tableId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dict.Add(dr.GetName(i), dr.GetValue(i).ToString());
                    }
                    if (dict["FORM_FIELD_CD"].Equals("11") && dict["FIELD_DEF"].Equals("TODAY"))
                    {
                        dict["FIELD_DEF"] = DateTime.Now.ToString("yyyy/MM/dd");
                    }
                    list.Add(dict);
                }
            }

            //logger.Debug(list.Count);

            return list;
        }
        */

        /// <summary>
        /// 案件內容
        /// </summary>
        /// <param name="applyId"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetPreview(string applyId)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            string sql = @"
                SELECT A.APP_ID, A.SRV_ID, S.NAME, S.SRV_DESC, S.FORM_ID,
                    (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = A.UNIT_CD) AS UNIT_NAME,
                    ISNULL((SELECT SRV_FIELD FROM SERVICE_FORM WHERE (SRV_ID = S.SRV_ID AND ISNULL(S.FORM_ID, '') = '') OR (SRV_ID = S.FORM_ID)), '') AS SRV_FIELD,
                    ISNULL((SELECT SRV_SCRIPT FROM SERVICE_FORM WHERE (SRV_ID = S.SRV_ID AND ISNULL(S.FORM_ID, '') = '') OR (SRV_ID = S.FORM_ID)), '') AS SRV_SCRIPT,
                    ISNULL((SELECT PRE_SCRIPT FROM SERVICE_FORM WHERE (SRV_ID = S.SRV_ID AND ISNULL(S.FORM_ID, '') = '') OR (SRV_ID = S.FORM_ID)), '') AS PRE_SCRIPT
                FROM APPLY A, SERVICE S
                WHERE A.SRV_ID = S.SRV_ID
                  AND APP_ID = @APP_ID
            ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("APP_ID", applyId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dict.Add(dr.GetName(i), DataUtils.GetDBValue(dr, i));
                    }
                }
                dr.Close();
            }

            return dict;
        }

        /// <summary>
        /// 案件內容 - 會員資料
        /// </summary>
        /// <param name="applyId"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetPreviewMember(string applyId)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();

            string sql = @"
                SELECT ACC_NO, IDN, SEX_CD, CONVERT(CHAR(10), BIRTHDAY, 111) AS BIRTHDAY,
                    NAME, ENAME, CNT_NAME, CNT_ENAME, CHR_NAME, CHR_ENAME,
                    TEL, FAX, CNT_TEL, ADDR, EADDR,
                    (SELECT MAIL FROM MEMBER WHERE ACC_NO = A.ACC_NO) AS MAIL,
                    (SELECT MEDICO FROM MEMBER WHERE ACC_NO = A.ACC_NO) AS MEDICO,
                    CONVERT(CHAR(10), APP_TIME, 111) AS APPLY_DATE,MARITAL_CD,SRV_ID,MOBILE
                FROM APPLY A
                WHERE A.APP_ID = @APP_ID
            ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("APP_ID", applyId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        item.Add(dr.GetName(i), DataUtils.GetDBValue(dr, i));
                    }
                }
                dr.Close();
            }

            return item;
        }

        public Dictionary<string, object> GetPreviewForm(string serviceId, string applyId)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();

            string sql = @"
                SELECT * FROM APPLY_" + serviceId + @"
                WHERE APP_ID = @APP_ID
            ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("APP_ID", applyId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        item.Add(dr.GetName(i), DataUtils.GetDBValue(dr, i));
                    }
                }
                dr.Close();
            }

            return item;
        }

        public List<Dictionary<string, object>> GetPreviewTableList(string tableName, string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string sql = @"
                SELECT * FROM " + tableName + @"
                WHERE APP_ID = @APP_ID
            ";

            args.Add("APP_ID", applyId);

            return GetList(sql, args);
        }

        /// <summary>
        /// 新增申請單資料到暫存表
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="dt"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool InsertTempData(string accountNo, DateTime dt, Dictionary<string, string> data)
        {
            string sql = @"INSERT INTO APPLY_TEMP (ACC_NO, APP_TIME, APP_DATA) VALUES (@ACC_NO, @APP_TIME, @APP_DATA)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "ACC_NO", accountNo);
            DataUtils.AddParameters(cmd, "APP_TIME", dt.ToString("yyyyMMddHHmmssffffff"));

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(ms, data);
                ms.Seek(0, 0);

                DataUtils.AddParameters(cmd, "APP_DATA", ms.ToArray());
            }

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 更新預交易代碼到暫存資料表
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool UpdateTempData(string accountNo, Dictionary<string, string> data)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("ACC_NO", accountNo);
            args.Add("APP_TIME", data["APP_TIME"]);
            args.Add("SESSION_KEY", data["PAY_SESSION_KEY"]);

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(ms, data);
                ms.Seek(0, 0);

                args.Add("APP_DATA", ms.ToArray());
            }

            string sql = @"
                UPDATE APPLY_TEMP SET
                    APP_DATA = @APP_DATA,
                    SESSION_KEY = @SESSION_KEY
                WHERE ACC_NO = @ACC_NO
                  AND APP_TIME = @APP_TIME
            ";

            return (Update(sql, args) == 1);
        }

        /// <summary>
        /// 取得暫存資料
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetTempData(string accountNo, string dt, string serviceId)
        {
            Dictionary<string, string> data = null;

            string sql = @"
                SELECT APP_DATA, (SELECT COMP_DESC FROM SERVICE_NOTICE WHERE SRV_ID = @SRV_ID) AS COMP_DESC
                FROM APPLY_TEMP 
                WHERE APP_TIME = @APP_TIME AND ACC_NO = @ACC_NO";
            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "ACC_NO", accountNo);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);
            DataUtils.AddParameters(cmd, "SRV_ID", serviceId);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    byte[] buffer = (byte[])dr[0];

                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(buffer, 0, buffer.Length);
                        ms.Seek(0, 0);
                        BinaryFormatter b = new BinaryFormatter();
                        data = (Dictionary<string, string>)b.Deserialize(ms);
                    }

                    if (!data.ContainsKey("COMP_DESC"))
                    {
                        data.Add("COMP_DESC", DataUtils.GetDBString(dr, 1));
                    }
                }
                dr.Close();
            }

            return data;
        }

        public Dictionary<string, string> GetTempData(string accountNo, string sessionKey)
        {
            Dictionary<string, string> data = null;

            logger.Debug("GetTempData: ACC_NO: " + accountNo + " / SESSION_KEY: " + sessionKey);

            string sql1 = @"
                SELECT APP_DATA
                FROM APPLY_TEMP 
                WHERE SESSION_KEY = @SESSION_KEY AND ACC_NO = @ACC_NO";
            SqlCommand cmd = new SqlCommand(sql1, conn, tran);
            DataUtils.AddParameters(cmd, "ACC_NO", accountNo);
            DataUtils.AddParameters(cmd, "SESSION_KEY", sessionKey);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    byte[] buffer = (byte[])dr[0];

                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(buffer, 0, buffer.Length);
                        ms.Seek(0, 0);
                        BinaryFormatter b = new BinaryFormatter();
                        data = (Dictionary<string, string>)b.Deserialize(ms);
                    }
                }
                dr.Close();
            }

            //Dictionary<string, object> args = new Dictionary<string, object>();
            //args.Add("SRV_ID", data["SRV_ID"]);

            return data;
        }

        /// <summary>
        /// 刪除暫存資料
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool DeleteTempData(string accountNo, string dt)
        {
            string sql = @"
                DELETE APPLY_TEMP 
                WHERE APP_TIME = @APP_TIME AND ACC_NO = @ACC_NO";
            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            DataUtils.AddParameters(cmd, "ACC_NO", accountNo);
            DataUtils.AddParameters(cmd, "APP_TIME", dt);

            int flag = cmd.ExecuteNonQuery();

            if (flag == 1) return true;

            return false;
        }

        /// <summary>
        /// 取得申請單編號
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="applyDate"></param>
        /// <returns></returns>
        public string GetApplySerial(string serviceId, string applyDate)
        {
            string serial = null;

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SRV_ID", serviceId);
            args.Add("SRV_DATE", applyDate);

            string querySQL = @"SELECT SRL_NO FROM APPLY_SERIAL WHERE SRV_DATE = CONVERT(DATE, @SRV_DATE, 112) AND SRV_ID = @SRV_ID";
            string insertSQL = @"INSERT INTO APPLY_SERIAL (SRV_ID, SRV_DATE, SRL_NO) VALUES (@SRV_ID, CONVERT(DATE, @SRV_DATE, 112), 1)";
            string updateSQL = @"UPDATE APPLY_SERIAL SET SRL_NO = SRL_NO + 1 WHERE SRV_ID = @SRV_ID AND SRV_DATE = CONVERT(DATE, @SRV_DATE, 112) AND SRL_NO = @SRL_NO";

            Dictionary<string, object> data = GetData(querySQL, args);

            if (data == null)
            {
                data = new Dictionary<string, object>();
                data.Add("SRV_ID", serviceId);
                data.Add("SRV_DATE", applyDate);
                data.Add("SRL_NO", 0);

                SqlCommand cmd = new SqlCommand(insertSQL, conn, tran);
                DataUtils.AddParameters(cmd, "SRV_ID", serviceId);
                DataUtils.AddParameters(cmd, "SRV_DATE", applyDate);

                cmd.ExecuteNonQuery();
            }
            else
            {
                SqlCommand cmd = new SqlCommand(updateSQL, conn, tran);
                DataUtils.AddParameters(cmd, "SRV_ID", serviceId);
                DataUtils.AddParameters(cmd, "SRV_DATE", applyDate);
                DataUtils.AddParameters(cmd, "SRL_NO", data["SRL_NO"]);

                cmd.ExecuteNonQuery();
            }

            serial = applyDate.ToString() + serviceId + (((int)data["SRL_NO"]) + 1).ToString("D4");

            return serial;
        }

        /// <summary>
        /// 取得繳費單編號
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="applyDate"></param>
        /// <returns></returns>
        public string GetPaySerial(string serviceId, string applyDate)
        {
            string serial = null;

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("SRV_ID", serviceId);
            args.Add("SRV_DATE", applyDate);

            string querySQL = @"SELECT SRL_NO FROM PAY_SERIAL WHERE SRV_DATE = CONVERT(DATE, @SRV_DATE, 112) AND SRV_ID = @SRV_ID";
            string insertSQL = @"INSERT INTO PAY_SERIAL (SRV_ID, SRV_DATE, SRL_NO) VALUES (@SRV_ID, CONVERT(DATE, @SRV_DATE, 112), 1)";
            string updateSQL = @"UPDATE PAY_SERIAL SET SRL_NO = SRL_NO + 1 WHERE SRV_ID = @SRV_ID AND SRV_DATE = CONVERT(DATE, @SRV_DATE, 112) AND SRL_NO = @SRL_NO";

            Dictionary<string, object> data = GetData(querySQL, args);

            if (data == null)
            {
                data = new Dictionary<string, object>();
                data.Add("SRV_ID", serviceId);
                data.Add("SRV_DATE", applyDate);
                data.Add("SRL_NO", 0);

                SqlCommand cmd = new SqlCommand(insertSQL, conn, tran);
                DataUtils.AddParameters(cmd, "SRV_ID", serviceId);
                DataUtils.AddParameters(cmd, "SRV_DATE", applyDate);

                cmd.ExecuteNonQuery();
            }
            else
            {
                SqlCommand cmd = new SqlCommand(updateSQL, conn, tran);
                DataUtils.AddParameters(cmd, "SRV_ID", serviceId);
                DataUtils.AddParameters(cmd, "SRV_DATE", applyDate);
                DataUtils.AddParameters(cmd, "SRL_NO", data["SRL_NO"]);

                cmd.ExecuteNonQuery();
            }



            serial = applyDate.ToString() + serviceId + (((int)data["SRL_NO"]) + 1).ToString("D4");

            return serial;
        }

        /// <summary>
        /// 取得超商繳費編號
        /// </summary>
        /// <param name="applyYM"></param>
        /// <returns></returns>
        public string GetPayStoreSerial(string applyYM)
        {
            string serial = null;

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("PAY_YM", applyYM);

            string querySQL = @"SELECT SRL_NO FROM PAY_STORE_SERIAL WHERE PAY_YM = @PAY_YM";
            string insertSQL = @"INSERT INTO PAY_STORE_SERIAL (PAY_YM, SRL_NO) VALUES (@PAY_YM, 1)";
            string updateSQL = @"UPDATE PAY_STORE_SERIAL SET SRL_NO = SRL_NO + 1 WHERE PAY_YM = @PAY_YM AND SRL_NO = @SRL_NO";

            Dictionary<string, object> data = GetData(querySQL, args);

            if (data == null)
            {
                data = new Dictionary<string, object>();
                data.Add("PAY_YM", applyYM);
                data.Add("SRL_NO", 0);

                SqlCommand cmd = new SqlCommand(insertSQL, conn, tran);
                DataUtils.AddParameters(cmd, "PAY_YM", applyYM);

                cmd.ExecuteNonQuery();
            }
            else
            {
                SqlCommand cmd = new SqlCommand(updateSQL, conn, tran);
                DataUtils.AddParameters(cmd, "PAY_YM", applyYM);
                DataUtils.AddParameters(cmd, "SRL_NO", data["SRL_NO"]);

                cmd.ExecuteNonQuery();
            }

            serial = applyYM.Substring(2) + (((int)data["SRL_NO"]) + 1).ToString("D4");

            return serial;
        }

        /// <summary>
        /// 取得信用卡錯誤代碼說明
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetPayCodeDesc(string code)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("CODE_CD", code);

            string sql = @"SELECT CODE_DESC FROM CODE_CD WHERE CODE_CD = @CODE_CD AND CODE_KIND = 'PAY_GSP_CD'";

            Dictionary<string, object> data = GetData(sql, args);

            if (data != null && data.ContainsKey("CODE_DESC"))
            {
                return (string)data["CODE_DESC"];
            }

            return "";
        }

        public Dictionary<String, object> GetApplyData(string applyId)
        {
            Dictionary<String, object> args = new Dictionary<string, object>();
            string querySQL = @"
                SELECT A.*, (SELECT NAME FROM SERVICE WHERE SRV_ID = A.SRV_ID) AS SRV_NAME,
                    DATEADD(DAY, (SELECT PAY_DEADLINE FROM SERVICE WHERE SRV_ID = A.SRV_ID), APP_TIME) AS PAY_DEADLINE,
                    (SELECT TOP 1 SESSION_KEY FROM APPLY_PAY WHERE APP_ID = A.APP_ID AND PAY_METHOD = 'S') AS SESSION_KEY
                FROM APPLY A WHERE APP_ID = @APP_ID";
            args.Add("APP_ID", applyId);

            return GetData(querySQL, args);
        }
        public Dictionary<String, object> GetApplyDonateData(string applyId)
        {
            Dictionary<String, object> args = new Dictionary<string, object>();
            string querySQL = @"
                SELECT A.*, (SELECT NAME FROM SERVICE WHERE SRV_ID = A.SRV_ID) AS SRV_NAME,
                    DATEADD(DAY, (SELECT PAY_DEADLINE FROM SERVICE WHERE SRV_ID = '007001'), APP_TIME) AS PAY_DEADLINE,
                    (SELECT TOP 1 SESSION_KEY FROM APPLY_PAY WHERE APP_ID = A.APP_ID AND PAY_METHOD = 'S') AS SESSION_KEY
                FROM APPLY A WHERE APP_ID = @APP_ID";
            args.Add("APP_ID", applyId);

            return GetData(querySQL, args);
        }
        public bool InsertApply(Dictionary<string, string> data, Dictionary<string, string> form)
        {
            int n = 0, flag = 0;


            #region 新增資料至 APPLY TABLE
            string insertSQL = @"
                INSERT INTO APPLY (
                    APP_ID, SRV_ID, SRC_SRV_ID, ACC_NO,
                    IDN, SEX_CD, BIRTHDAY, NAME, ENAME,
                    CNT_NAME, CNT_ENAME, CHR_NAME, CHR_ENAME, TEL,
                    FAX, CNT_TEL, ADDR, EADDR, CARD_IDN,
                    APP_TIME, PAY_POINT, PAY_METHOD, PAY_A_FEE, PAY_A_FEEBK,
                    PAY_A_PAID, PAY_C_FEE, PAY_C_FEEBK, PAY_C_PAID, ATM_VNO, 
                    TRANS_ID, FLOW_CD, LOGIN_TYPE, UNIT_CD, APP_DISP_MK, PRO_UNIT_CD,
                    ADD_TIME, ADD_FUN_CD, ADD_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC, MARITAL_CD,CERT_SN,MOBILE
                )
                SELECT @APP_ID, SRV_ID, SRV_ID AS SRC_SRV_ID, @ACC_NO,
                    @IDN, @SEX_CD, CONVERT(DATE, @BIRTHDAY, 111), @NAME, @ENAME,
                    @CNT_NAME, @CNT_ENAME, @CHR_NAME, @CHR_ENAME, @TEL,
                    @FAX, @CNT_TEL, @ADDR, @EADDR, @CARD_IDN,
                    @APP_TIME, PAY_POINT, @PAY_METHOD, @PAY_A_FEE, @PAY_A_FEEBK,
                    @PAY_A_PAID, @PAY_C_FEE, @PAY_C_FEEBK, @PAY_C_PAID, @ATM_VNO, 
                    @TRANS_ID, @FLOW_CD, @LOGIN_TYPE,
                    ISNULL((
		                SELECT (CASE 
				            WHEN UNIT_PCD = 0 THEN S.FIX_UNIT_CD 
				            WHEN (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = U.UNIT_PCD) = 0 THEN S.FIX_UNIT_CD 
				            ELSE UNIT_PCD  END) 
		                FROM UNIT U WHERE UNIT_CD = S.FIX_UNIT_CD
	                ), S.FIX_UNIT_CD) AS UNIT_CD, @APP_DISP_MK,
                    ISNULL((
		                SELECT (CASE 
				            WHEN UNIT_PCD = 0 THEN S.FIX_UNIT_CD 
				            WHEN (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = U.UNIT_PCD) = 0 THEN S.FIX_UNIT_CD 
				            ELSE UNIT_PCD  END) 
		                FROM UNIT U WHERE UNIT_CD = S.FIX_UNIT_CD
	                ), S.FIX_UNIT_CD) AS PRO_UNIT_CD,
                    GETDATE(), @UPD_FUN_CD, @UPD_ACC, GETDATE(), @UPD_FUN_CD, @UPD_ACC,@MARITAL_CD,@CERT_SN,@MOBILE
                FROM SERVICE S
                WHERE SRV_ID = @SRV_ID
            ";

            using (SqlCommand cmd = new SqlCommand(insertSQL, conn, tran))
            {

                DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                DataUtils.AddParameters(cmd, "ACC_NO", GetData(data, "ACC_NO"));

                DataUtils.AddParameters(cmd, "IDN", GetData(data, "IDN"));
                DataUtils.AddParameters(cmd, "SEX_CD", GetData(data, "SEX_CD"));
                DataUtils.AddParameters(cmd, "MARITAL_CD", GetData(data, "MARITAL_CD"));
                if (GetData(data, "BIRTHDAY").Equals(""))
                {
                    DataUtils.AddParameters(cmd, "BIRTHDAY", null);
                }
                else
                {
                    DataUtils.AddParameters(cmd, "BIRTHDAY", DateTime.ParseExact(GetData(data, "BIRTHDAY"), "yyyy/MM/dd", cultureStyle));
                }
                DataUtils.AddParameters(cmd, "NAME", GetData(data, "NAME"));
                DataUtils.AddParameters(cmd, "ENAME", GetData(data, "ENAME"));

                DataUtils.AddParameters(cmd, "CNT_NAME", GetData(data, "CNT_NAME"));
                DataUtils.AddParameters(cmd, "CNT_ENAME", GetData(data, "CNT_ENAME"));
                DataUtils.AddParameters(cmd, "CHR_NAME", GetData(data, "CHR_NAME"));
                DataUtils.AddParameters(cmd, "CHR_ENAME", GetData(data, "CHR_ENAME"));
                DataUtils.AddParameters(cmd, "TEL", GetData(data, "TEL"));

                DataUtils.AddParameters(cmd, "FAX", GetData(data, "FAX"));
                DataUtils.AddParameters(cmd, "CNT_TEL", GetData(data, "CNT_TEL"));
                DataUtils.AddParameters(cmd, "ADDR", GetData(data, "ADDR"));
                DataUtils.AddParameters(cmd, "EADDR", GetData(data, "EADDR"));
                DataUtils.AddParameters(cmd, "CARD_IDN", GetData(data, "CARD_IDN"));

                DataUtils.AddParameters(cmd, "APP_TIME", DateTime.ParseExact(GetData(data, "APP_TIME"), "yyyyMMddHHmmssffffff", cultureStyle));
                DataUtils.AddParameters(cmd, "PAY_METHOD", GetData(data, "PAY_METHOD"));
                DataUtils.AddParameters(cmd, "PAY_A_FEE", GetData(data, "PAY_A_FEE"));
                DataUtils.AddParameters(cmd, "PAY_A_FEEBK", GetData(data, "PAY_A_FEEBK"));

                DataUtils.AddParameters(cmd, "PAY_A_PAID", GetData(data, "PAY_A_PAID"));
                DataUtils.AddParameters(cmd, "PAY_C_FEE", GetData(data, "PAY_C_FEE"));
                DataUtils.AddParameters(cmd, "PAY_C_FEEBK", GetData(data, "PAY_C_FEEBK"));
                DataUtils.AddParameters(cmd, "PAY_C_PAID", GetData(data, "PAY_C_PAID"));
                DataUtils.AddParameters(cmd, "ATM_VNO", GetData(data, "ATM_VNO"));

                DataUtils.AddParameters(cmd, "TRANS_ID", GetData(data, "TRANS_ID"));
                DataUtils.AddParameters(cmd, "FLOW_CD", "01");
                DataUtils.AddParameters(cmd, "LOGIN_TYPE", GetData(data, "LOGIN_TYPE"));

                DataUtils.AddParameters(cmd, "APP_DISP_MK", (GetData(data, "SRV_ID").Equals("001034") || GetData(data, "SRV_ID").Equals("001035") || GetData(data, "SRV_ID").Equals("001038") || GetData(data, "SRV_ID").StartsWith("005") || GetData(data, "SRV_ID").StartsWith("007")) ? "Y" : "N");

                DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));

                DataUtils.AddParameters(cmd, "SRV_ID", GetData(data, "SRV_ID"));

                DataUtils.AddParameters(cmd, "CERT_SN", GetData(data, "CERT_SN"));
                //DataUtils.AddParameters(cmd, "CERT_IDX", GetData(data, "CERT_IDX"));

                DataUtils.AddParameters(cmd, "MOBILE", GetData(data, "MOBILE"));

                n++;
                flag += cmd.ExecuteNonQuery();
            }
            #endregion

            // DATEADD(DAY, (SELECT PAY_DEADLINE FROM SERVICE WHERE SRV_ID = A.SRV_ID), APP_TIME)

            #region 繳費交易明細檔
            string insertSQL2 = @"
                INSERT INTO APPLY_PAY (
                    APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_METHOD,
                    PAY_ACT_TIME, PAY_EXT_TIME, PAY_INC_TIME, SESSION_KEY,
                    TRANS_RET, AUTH_DATE, AUTH_NO, SETTLE_DATE, OTHER, HOST_TIME, ROC_ID,
                    CLIENT_IP, OID, SID, PAY_STATUS_MK,
                    ADD_TIME, ADD_FUN_CD, ADD_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC
                ) VALUES (
                    @APP_ID, @PAY_ID, @PAY_MONEY, @PAY_PROFEE, @PAY_METHOD,
                    @PAY_ACT_TIME, @PAY_EXT_TIME, @PAY_INC_TIME,
                    @SESSION_KEY,
                    @TRANS_RET, @AUTH_DATE, @AUTH_NO, @SETTLE_DATE, @OTHER, @HOST_TIME, @ROC_ID,
                    @CLIENT_IP, @OID, @SID, @PAY_STATUS_MK,
                    GETDATE(), @UPD_FUN_CD, @UPD_ACC, GETDATE(), @UPD_FUN_CD, @UPD_ACC
                )
            ";

            using (SqlCommand cmd = new SqlCommand(insertSQL2, conn, tran))
            {
                DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                DataUtils.AddParameters(cmd, "PAY_ID", GetData(data, "PAY_ID"));
                DataUtils.AddParameters(cmd, "PAY_MONEY", GetData(data, "PAY_A_FEE"));
                DataUtils.AddParameters(cmd, "PAY_PROFEE", GetData(data, "PAY_A_FEEBK"));
                DataUtils.AddParameters(cmd, "PAY_METHOD", GetData(data, "PAY_METHOD"));

                DataUtils.AddParameters(cmd, "PAY_ACT_TIME", DateTime.Now);

                if (GetData(data, "PAY_METHOD").Equals("C") && GetData(data, "PAY_TRANS_RET").Equals("0000"))
                {
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", DateTime.Now);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", DateTime.ParseExact(GetData(data, "APP_TIME"), "yyyyMMddHHmmssffffff", cultureStyle));
                }
                else
                {
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", null);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", null);
                }

                DataUtils.AddParameters(cmd, "SRV_ID", GetData(data, "SRV_ID"));

                DataUtils.AddParameters(cmd, "SESSION_KEY", GetData(data, "PAY_SESSION_KEY"));

                DataUtils.AddParameters(cmd, "TRANS_RET", GetData(data, "PAY_TRANS_RET"));
                DataUtils.AddParameters(cmd, "AUTH_DATE", GetTime(data, "PAY_AUTH_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "AUTH_NO", GetData(data, "PAY_AUTH_NO"));
                DataUtils.AddParameters(cmd, "SETTLE_DATE", GetTime(data, "PAY_SETTLE_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "OTHER", GetData(data, "PAY_OTHER"));
                DataUtils.AddParameters(cmd, "HOST_TIME", GetTime(data, "PAY_HOST_TIME", "yyyyMMddHHmmss"));

                DataUtils.AddParameters(cmd, "ROC_ID", GetData(data, "PAY_CARD_IDN"));
                DataUtils.AddParameters(cmd, "CLIENT_IP", GetData(data, "PAY_CLIENT_IP"));
                DataUtils.AddParameters(cmd, "OID", GetData(data, "PAY_OID"));
                DataUtils.AddParameters(cmd, "SID", GetData(data, "PAY_SID"));

                DataUtils.AddParameters(cmd, "PAY_STATUS_MK", (GetData(data, "PAY_TRANS_RET").Equals("0000") ? "Y" : "N"));

                DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));

                n++;
                flag += cmd.ExecuteNonQuery();
            }
            #endregion

            if (String.IsNullOrEmpty(form["SRV_FIELD"]))
            {
                #region 組SQL語法
                string querySQL = @"
                    WITH T1 AS (
	                    SELECT DISTINCT TABLE_DB_NAME,
			                    SUBSTRING(TABLE_TYPE, 1, LEN(TABLE_TYPE)-1) AS TABLE_TYPE, (
				                    SELECT FIELD_NAME + ','
				                    FROM FORM_FIELD F
				                    WHERE DB_MK = 'Y'
					                    AND TABLE_ID IN (
						                    SELECT TABLE_ID FROM FORM_TABLE 
						                    WHERE TABLE_DB_NAME = T.TABLE_DB_NAME
							                    AND SRV_ID = T.SRV_ID
					                    )
					                    AND SRV_ID = T.SRV_ID
				                    FOR XML PATH('')
			                    ) AS FIELD_NAME, (
				                    SELECT CONVERT(VARCHAR(MAX), TABLE_ID) + ','
				                    FROM FORM_TABLE
				                    WHERE TABLE_DB_NAME = T.TABLE_DB_NAME
					                    AND SRV_ID = T.SRV_ID
				                    FOR XML PATH('')
			                    ) AS TABLE_ID
	                    FROM FORM_TABLE T
	                    WHERE SRV_ID = @SRV_ID
                    ), T2 AS (
	                    SELECT DISTINCT TABLE_DB_NAME, (
				                    SELECT FIELD_NAME + ','
				                    FROM FORM_FIELD F
				                    WHERE DB_MK = 'Y'
					                    AND FORM_FIELD_CD = '11'
					                    AND TABLE_ID IN (
						                    SELECT TABLE_ID FROM FORM_TABLE 
						                    WHERE TABLE_DB_NAME = T.TABLE_DB_NAME
							                    AND SRV_ID = T.SRV_ID
					                    )
					                    AND SRV_ID = T.SRV_ID
				                    FOR XML PATH('')
			                    ) AS DATE_FIELD_NAME
	                    FROM FORM_TABLE T
	                    WHERE SRV_ID = @SRV_ID
                    )
                    SELECT T1.TABLE_DB_NAME, T1.TABLE_TYPE,
	                    SUBSTRING(T1.TABLE_ID, 1, LEN(T1.TABLE_ID)-1) AS TABLE_ID,
	                    SUBSTRING(T1.FIELD_NAME, 1, LEN(T1.FIELD_NAME)-1) AS FIELD_NAME,
	                    SUBSTRING(T2.DATE_FIELD_NAME, 1, LEN(T2.DATE_FIELD_NAME)-1) AS DATE_FIELD_NAME
                    FROM T1, T2
                    WHERE T1.TABLE_DB_NAME = T2.TABLE_DB_NAME
                ";

                Dictionary<string, object> args = new Dictionary<string, object>();
                args.Add("SRV_ID", data["SRV_ID"]);
                List<Dictionary<string, object>> tableList = GetList(querySQL, args);
                Dictionary<string, string> sql = new Dictionary<string, string>();
                StringBuilder sb = null;
                string[] columns = null;
                string[] dateColumns = null;

                IFormatProvider format = new System.Globalization.CultureInfo("zh-TW", true);

                foreach (Dictionary<string, object> item in tableList)
                {
                    columns = item["FIELD_NAME"].ToString().Split(',');

                    sb = new StringBuilder("INSERT INTO ");
                    sb.Append(item["TABLE_DB_NAME"]).Append(" (");

                    for (int i = 0; i < columns.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(columns[i]))
                        {
                            sb.Append(columns[i]).Append(", ");
                        }
                    }

                    sb.Append("APP_ID, ");
                    if (item["TABLE_TYPE"].Equals("1"))
                    {
                        sb.Append("SRL_NO, ");
                    }
                    sb.Append("ADD_TIME, ADD_FUN_CD, ADD_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC");
                    sb.Append(") VALUES (");

                    for (int i = 0; i < columns.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(columns[i]))
                        {
                            sb.Append("@").Append(columns[i]).Append(", ");
                        }
                    }

                    sb.Append("@APP_ID, ");
                    if (item["TABLE_TYPE"].Equals("1"))
                    {
                        sb.Append("@SRL_NO, ");
                    }
                    sb.Append("GETDATE(), @UPD_FUN_CD, @UPD_ACC, GETDATE(), @UPD_FUN_CD, @UPD_ACC)");

                    //logger.Debug("SQL: " + sb.ToString());

                    sql.Add(item["TABLE_DB_NAME"].ToString(), sb.ToString());
                }
                #endregion

                #region 新增資料至 APPLY_XXX TABLE
                string[] tableId = null;
                foreach (Dictionary<string, object> item in tableList)
                {
                    columns = item["FIELD_NAME"].ToString().Split(',');
                    dateColumns = item["DATE_FIELD_NAME"].ToString().Split(',');

                    if (item["TABLE_TYPE"].Equals("0"))
                    {
                        #region APPLY_XXXXXX
                        using (SqlCommand cmd = new SqlCommand(sql[item["TABLE_DB_NAME"].ToString()], conn, tran))
                        {
                            for (int i = 0; i < columns.Count(); i++)
                            {
                                if (!String.IsNullOrEmpty(columns[i]))
                                {
                                    //logger.Debug("[" + i + "] " + columns[i] + ": " + dateColumns.Contains(columns[i]));
                                    if (dateColumns.Contains(columns[i]))
                                    {
                                        //logger.Debug(columns[i] + ": " + GetData(data, columns[i]));
                                        DataUtils.AddParameters(cmd, columns[i], DateTime.ParseExact(GetData(data, columns[i]), "yyyy/MM/dd", format));
                                    }
                                    else
                                    {
                                        DataUtils.AddParameters(cmd, columns[i], GetData(data, columns[i]));
                                    }
                                }
                            }
                            DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                            DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                            DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));

                            n++;
                            flag += cmd.ExecuteNonQuery();
                        }
                        #endregion
                    }
                    else if (item["TABLE_TYPE"].Equals("1"))
                    {
                        #region APPLY_XXXXXX_XX
                        tableId = item["TABLE_ID"].ToString().Split(',');

                        for (int i = 0; i < tableId.Count(); i++)
                        {
                            int len = Int32.Parse(data["tbody_" + tableId[i] + "_max"]);
                            for (int j = 1; j <= len; j++)
                            {
                                using (SqlCommand cmd = new SqlCommand(sql[item["TABLE_DB_NAME"].ToString()], conn, tran))
                                {
                                    for (int k = 0; k < columns.Count(); k++)
                                    {
                                        //logger.Debug(columns[k] + ": " + dateColumns.Contains(columns[k]));
                                        if (dateColumns.Contains(columns[k]) && !String.IsNullOrEmpty(GetData(data, columns[k] + "_" + j)))
                                        {
                                            //logger.Debug(columns[k] + ": " + GetData(data, columns[k] + "_" + j));
                                            DataUtils.AddParameters(cmd, columns[k], DateTime.ParseExact(GetData(data, columns[k] + "_" + j), "yyyy/MM/dd", format));
                                        }
                                        else
                                        {
                                            DataUtils.AddParameters(cmd, columns[k], GetData(data, columns[k] + "_" + j));
                                        }
                                    }
                                    DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                                    DataUtils.AddParameters(cmd, "SRL_NO", j);
                                    DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                                    DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));

                                    n++;
                                    flag += cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
            else
            {
                #region 組SQL語法
                Dictionary<string, Dictionary<string, string>> tableData = GetFormTableDB(form["SRV_FIELD"], data["SRV_ID"]);
                Dictionary<string, string> item = null;
                Dictionary<string, string> sql = new Dictionary<string, string>();
                StringBuilder sb = null;

                string[] keys = tableData.Keys.ToArray();
                string[] columns = null, dateColumns = null;

                foreach (string key in keys)
                {
                    item = tableData[key];
                    columns = item["FIELD_NAME"].Split(',');

                    sb = new StringBuilder("INSERT INTO ");
                    sb.Append(item["TABLE_DB_NAME"]).Append(" (");

                    for (int i = 0; i < columns.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(columns[i]))
                        {
                            sb.Append(columns[i]).Append(", ");
                        }
                    }

                    sb.Append("APP_ID, ");

                    if (item["TABLE_TYPE"].Equals("1"))
                    {
                        sb.Append("SRL_NO, ");
                    }
                    sb.Append("ADD_TIME, ADD_FUN_CD, ADD_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC");
                    sb.Append(") VALUES (");

                    for (int i = 0; i < columns.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(columns[i]))
                        {
                            sb.Append("@").Append(columns[i]).Append(", ");
                        }
                    }

                    sb.Append("@APP_ID, ");
                    if (item["TABLE_TYPE"].Equals("1"))
                    {
                        sb.Append("@SRL_NO, ");
                    }
                    sb.Append("GETDATE(), @UPD_FUN_CD, @UPD_ACC, GETDATE(), @UPD_FUN_CD, @UPD_ACC)");

                    //logger.Debug("SQL: " + sb.ToString());

                    sql.Add(item["TABLE_DB_NAME"].ToString(), sb.ToString());

                    //logger.Debug(item["TABLE_DB_NAME"] + ": " + sb);
                }
                #endregion

                #region 新增資料至 APPLY_XXX TABLE
                string[] tableId = null;
                IFormatProvider format = new System.Globalization.CultureInfo("zh-TW", true);

                foreach (string key in keys)
                {
                    item = tableData[key];
                    columns = item["FIELD_NAME"].Split(',');

                    dateColumns = item["DATE_FIELD_NAME"].Split(',');

                    if (item["TABLE_TYPE"].Equals("0"))
                    {
                        #region APPLY_XXXXXX
                        logger.Debug(sql[item["TABLE_DB_NAME"].ToString()]);
                        using (SqlCommand cmd = new SqlCommand(sql[item["TABLE_DB_NAME"].ToString()], conn, tran))
                        {
                            for (int i = 0; i < columns.Count(); i++)
                            {
                                if (!String.IsNullOrEmpty(columns[i]))
                                {
                                    if (dateColumns.Contains(columns[i]))
                                    {
                                        logger.Debug("DATE: " + GetData(data, columns[i]));
                                        if (!String.IsNullOrEmpty(GetData(data, columns[i])))
                                            DataUtils.AddParameters(cmd, columns[i], DateTime.ParseExact(GetData(data, columns[i]), "yyyy/MM/dd", format));
                                        else
                                            DataUtils.AddParameters(cmd, columns[i], GetData(data, columns[i]));
                                    }
                                    else
                                    {
                                        logger.Debug(columns[i] + ": " + GetData(data, columns[i]));
                                        DataUtils.AddParameters(cmd, columns[i], GetData(data, columns[i]));
                                    }
                                }
                            }
                            DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                            DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                            DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));
                            logger.Debug("APP_ID:" + GetData(data, "APP_ID"));
                            logger.Debug("UPD_ACC" + GetData(data, "UPD_ACC"));
                            logger.Debug("SqlTxt:" + cmd.CommandText);





                            n++;
                            flag += cmd.ExecuteNonQuery();
                        }
                        #endregion
                    }
                    else if (item["TABLE_TYPE"].Equals("1"))
                    {
                        #region APPLY_XXXXXX_XX
                        tableId = item["TABLE_ID"].ToString().Split(',');

                        for (int i = 0; i < tableId.Count(); i++)
                        {
                            int len = Int32.Parse(data["tbody_" + tableId[i] + "_max"]);
                            for (int j = 1; j <= len; j++)
                            {
                                //logger.Debug(sql[item["TABLE_DB_NAME"].ToString()]);
                                using (SqlCommand cmd = new SqlCommand(sql[item["TABLE_DB_NAME"].ToString()], conn, tran))
                                {
                                    for (int k = 0; k < columns.Count(); k++)
                                    {
                                        //logger.Debug(columns[k] + ": " + dateColumns.Contains(columns[k]));
                                        if (dateColumns.Contains(columns[k]) && !String.IsNullOrEmpty(GetData(data, columns[k] + "_" + j)))
                                        {
                                            //logger.Debug(columns[k] + ": " + GetData(data, columns[k] + "_" + j));
                                            DataUtils.AddParameters(cmd, columns[k], DateTime.ParseExact(GetData(data, columns[k] + "_" + j), "yyyy/MM/dd", format));
                                        }
                                        else
                                        {
                                            //logger.Debug(columns[k] + ": " + GetData(data, columns[k] + "_" + j));
                                            DataUtils.AddParameters(cmd, columns[k], GetData(data, columns[k] + "_" + j));
                                        }
                                    }
                                    DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                                    DataUtils.AddParameters(cmd, "SRL_NO", j);
                                    DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                                    DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));

                                    n++;
                                    flag += cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }

            #region 005001-5 更新公文檔的申請單號及寄送mail
            if (form["SRV_ID"] == "005001" || form["SRV_ID"] == "005002" || form["SRV_ID"] == "005003" || form["SRV_ID"] == "005004" || form["SRV_ID"] == "005005")
            {
                string updateSQL = @"
                UPDATE APPLY_DOCUMENT SET APP_ID=@APP_ID WHERE NO=@NO ";
                using (SqlCommand cmd = new SqlCommand(updateSQL, conn, tran))
                {
                    DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                    DataUtils.AddParameters(cmd, "NO", form["DocumentId"]);
                    flag += cmd.ExecuteNonQuery();
                    n++;
                }
                string deleteSQL = @"DELETE APPLY_DOCUMENT WHERE ISNULL(APP_ID,'')=''";
                using (SqlCommand cmd = new SqlCommand(deleteSQL, conn, tran))
                {
                    cmd.ExecuteNonQuery();
                }

                //寄發Mail通知管理員
                string strSrc = System.Configuration.ConfigurationManager.AppSettings["DocumentSrc"] + "/SCHEDULE/DocSftp/RUN";
                DateTime applyTime = DateTime.ParseExact(GetData(data, "APP_TIME"), "yyyyMMddHHmmssffffff", cultureStyle);
                string body = String.Format(MessageUtils.MAIL_ToADMIN_NEWCASE_BODY_1, GetData(data, "NAME"),
                               applyTime.Year - 1911, applyTime.Month.ToString("D2"), applyTime.Day.ToString("D2"),
                               form["NAME"], GetData(data, "APP_ID"), strSrc);

                MailUtils.SendMail(DataUtils.GetConfig("DOWNLOAD_DOCUMENT_MAIL"), MessageUtils.MAIL_ToADMIN_NEWCASE_SUBJECT, body, form["SRV_ID"]);
            }
            #endregion

            //logger.Debug("flag: " + flag + " / n: " + n);

            if (flag == n)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 新增繳費資訊 (信用卡重新繳費)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool InsertApplyPay(Dictionary<string, string> data)
        {
            string insertSQL = @"
                INSERT INTO APPLY_PAY (
                    APP_ID, PAY_ID, PAY_MONEY, PAY_PROFEE, PAY_METHOD,
                    PAY_ACT_TIME, PAY_EXT_TIME, PAY_INC_TIME, SESSION_KEY,
                    TRANS_RET, AUTH_DATE, AUTH_NO, SETTLE_DATE, OTHER, HOST_TIME, ROC_ID,
                    CLIENT_IP, OID, SID, PAY_STATUS_MK,
                    ADD_TIME, ADD_FUN_CD, ADD_ACC, UPD_TIME, UPD_FUN_CD, UPD_ACC
                ) VALUES (
                    @APP_ID, @PAY_ID, @PAY_MONEY, @PAY_PROFEE, @PAY_METHOD,
                    @PAY_ACT_TIME, @PAY_EXT_TIME, @PAY_INC_TIME,
                    @SESSION_KEY,
                    @TRANS_RET, @AUTH_DATE, @AUTH_NO, @SETTLE_DATE, @OTHER, @HOST_TIME, @ROC_ID,
                    @CLIENT_IP, @OID, @SID, @PAY_STATUS_MK,
                    GETDATE(), @UPD_FUN_CD, @UPD_ACC, GETDATE(), @UPD_FUN_CD, @UPD_ACC
                )
            ";

            using (SqlCommand cmd = new SqlCommand(insertSQL, conn, tran))
            {
                DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                DataUtils.AddParameters(cmd, "PAY_ID", GetData(data, "PAY_ID"));
                DataUtils.AddParameters(cmd, "PAY_MONEY", GetData(data, "PAY_A_FEE"));
                DataUtils.AddParameters(cmd, "PAY_PROFEE", GetData(data, "PAY_A_FEEBK"));
                DataUtils.AddParameters(cmd, "PAY_METHOD", GetData(data, "PAY_METHOD"));

                DataUtils.AddParameters(cmd, "PAY_ACT_TIME", DateTime.Now);

                if (GetData(data, "PAY_METHOD").Equals("C") && GetData(data, "PAY_TRANS_RET").Equals("0000"))
                {
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", DateTime.Now);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", DateTime.ParseExact(GetData(data, "APP_TIME"), "yyyyMMddHHmmssffffff", cultureStyle));
                }
                else
                {
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", null);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", null);
                }

                DataUtils.AddParameters(cmd, "SRV_ID", GetData(data, "SRV_ID"));

                DataUtils.AddParameters(cmd, "SESSION_KEY", GetData(data, "PAY_SESSION_KEY"));

                DataUtils.AddParameters(cmd, "TRANS_RET", GetData(data, "PAY_TRANS_RET"));
                DataUtils.AddParameters(cmd, "AUTH_DATE", GetTime(data, "PAY_AUTH_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "AUTH_NO", GetData(data, "PAY_AUTH_NO"));
                DataUtils.AddParameters(cmd, "SETTLE_DATE", GetTime(data, "PAY_SETTLE_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "OTHER", GetData(data, "PAY_OTHER"));
                DataUtils.AddParameters(cmd, "HOST_TIME", GetTime(data, "PAY_HOST_TIME", "yyyyMMddHHmmss"));

                DataUtils.AddParameters(cmd, "ROC_ID", GetData(data, "PAY_CARD_IDN"));
                DataUtils.AddParameters(cmd, "CLIENT_IP", GetData(data, "PAY_CLIENT_IP"));
                DataUtils.AddParameters(cmd, "OID", GetData(data, "PAY_OID"));
                DataUtils.AddParameters(cmd, "SID", GetData(data, "PAY_SID"));

                DataUtils.AddParameters(cmd, "PAY_STATUS_MK", (GetData(data, "PAY_TRANS_RET").Equals("0000") ? "Y" : "N"));

                DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));

                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 更新繳費交易明細檔
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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
                    PAY_A_PAID = @PAY_A_PAID,
                    UPD_TIME = GETDATE(),
                    UPD_FUN_CD = @UPD_FUN_CD,
                    UPD_ACC = @UPD_ACC
                WHERE APP_ID = @APP_ID
            ";

            int flag = 0;

            using (SqlCommand cmd = new SqlCommand(updateSQL1, conn, tran))
            {

                //logger.Debug("PAY_METHOD: " + GetData(data, "PAY_METHOD") + " / PAY_TRANS_RET: " + GetData(data, "PAY_TRANS_RET"));

                if (GetData(data, "PAY_METHOD").Equals("C") && GetData(data, "PAY_TRANS_RET").Equals("0000"))
                {
                    //logger.Debug("IN");
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", DateTime.Now);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", DateTime.Now);
                }
                else
                {
                    DataUtils.AddParameters(cmd, "PAY_EXT_TIME", null);
                    DataUtils.AddParameters(cmd, "PAY_INC_TIME", null);
                }

                DataUtils.AddParameters(cmd, "TRANS_RET", GetData(data, "PAY_TRANS_RET"));
                //logger.Debug("AUTH_DATE: " + data["PAY_AUTH_DATE"]);
                DataUtils.AddParameters(cmd, "AUTH_DATE", GetTime(data, "PAY_AUTH_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "AUTH_NO", GetData(data, "PAY_AUTH_NO"));
                DataUtils.AddParameters(cmd, "SETTLE_DATE", GetTime(data, "PAY_SETTLE_DATE", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "OTHER", GetData(data, "PAY_OTHER"));
                DataUtils.AddParameters(cmd, "HOST_TIME", GetTime(data, "PAY_HOST_TIME", "yyyyMMddHHmmss"));
                DataUtils.AddParameters(cmd, "PAY_STATUS_MK", (GetData(data, "PAY_TRANS_RET").Equals("0000") ? "Y" : "N"));
                DataUtils.AddParameters(cmd, "UPD_FUN_CD", "WEB-APPLY");
                DataUtils.AddParameters(cmd, "UPD_ACC", GetData(data, "UPD_ACC"));
                DataUtils.AddParameters(cmd, "APP_ID", GetData(data, "APP_ID"));
                DataUtils.AddParameters(cmd, "SESSION_KEY", GetData(data, "PAY_SESSION_KEY"));

                flag += cmd.ExecuteNonQuery();
            }

            using (SqlCommand cmd = new SqlCommand(updateSQL2, conn, tran))
            {
                DataUtils.AddParameters(cmd, "PAY_A_PAID", GetData(data, "PAY_A_PAID"));
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


        //public bool InsertApplyDocu

        public Dictionary<string, object> GetFormField(string xml, string serviceId)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            try
            {
                //logger.Debug(serviceId + " / " + xml);

                List<Dictionary<string, string>> tableList = new List<Dictionary<string, string>>();
                Dictionary<string, List<Dictionary<string, string>>> fields = new Dictionary<string, List<Dictionary<string, string>>>();
                List<Dictionary<string, string>> fieldList = null;
                Dictionary<string, string> item = null;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                //logger.Debug(1);
                XmlNode root = doc.GetElementsByTagName("ROOT")[0];
                //logger.Debug(2);
                XmlNodeList tableNodeList = root.ChildNodes;
                //logger.Debug(4);
                foreach (XmlNode tableNode in tableNodeList)
                {
                    //logger.Debug(4);
                    item = new Dictionary<string, string>();
                    item.Add("SRV_ID", serviceId);
                    item.Add("TABLE_ID", GetNodeText(tableNode.SelectSingleNode("ID")));
                    item.Add("TABLE_TYPE", GetNodeText(tableNode.SelectSingleNode("TYPE")));
                    item.Add("TABLE_TITLE", GetNodeText(tableNode.SelectSingleNode("TITLE")));
                    item.Add("TABLE_DESC", GetNodeText(tableNode.SelectSingleNode("DESC")));
                    item.Add("TABLE_DB_NAME", GetNodeText(tableNode.SelectSingleNode("DB_NAME")));
                    item.Add("TABLE_LEFT", GetNodeText(tableNode.SelectSingleNode("LEFT")));
                    item.Add("TABLE_RIGHT", GetNodeText(tableNode.SelectSingleNode("RIGHT")));
                    //logger.Debug(5);
                    tableList.Add(item);
                    //logger.Debug(6);
                    //logger.Debug("1:" + tableNode.SelectSingleNode("DATA").InnerText);
                    XmlNodeList fieldNodeList = tableNode.SelectSingleNode("DATA").ChildNodes;
                    fieldList = new List<Dictionary<string, string>>();
                    //logger.Debug(7);
                    foreach (XmlNode fieldNode in fieldNodeList)
                    {
                        //logger.Debug(8);
                        //logger.Debug("FIELD: " + fieldNode.InnerXml);
                        XmlNodeList dataNodeList = fieldNode.SelectSingleNode("FIELD").ChildNodes;
                        //logger.Debug(9);
                        bool isFirst = true;
                        foreach (XmlNode dataNode in dataNodeList)
                        {
                            //logger.Debug("X: " + dataNode.InnerXml);
                            //logger.Debug(10);
                            item = new Dictionary<string, string>();
                            item.Add("SRV_ID", serviceId);
                            item.Add("TABLE_ID", tableList.Last()["TABLE_ID"]);
                            item.Add("FIELD_ID", (fieldList.Count() + 1).ToString());
                            item.Add("FIELD_NAME", GetNodeText(dataNode.SelectSingleNode("NAME")));


                            item.Add("FIELD_LEFT", GetNodeText(dataNode.SelectSingleNode("LEFT")));
                            item.Add("FIELD_RIGHT", GetNodeText(dataNode.SelectSingleNode("RIGHT")));
                            item.Add("FIELD_DEF", GetNodeText(dataNode.SelectSingleNode("DEF")));
                            item.Add("FIELD_VAL", GetNodeText(dataNode.SelectSingleNode("VAL")));
                            item.Add("FIELD_STYLE", "");
                            item.Add("FIELD_LIMIT", "");

                            item.Add("FIELD_ATTR", GetNodeText(dataNode.SelectSingleNode("ATTR")));


                            if (isFirst)
                            {
                                item.Add("FIELD_TITLE", GetNodeText(fieldNode.SelectSingleNode("NAME")));
                                item.Add("TITLE_ATTR", GetNodeText(fieldNode.SelectSingleNode("ATTR")));
                                item.Add("TITLE_LEFT", GetNodeText(fieldNode.SelectSingleNode("LEFT")));
                                item.Add("TITLE_RIGHT", GetNodeText(fieldNode.SelectSingleNode("RIGHT")));
                                item.Add("TITLE_STYLE", GetNodeText(fieldNode.SelectSingleNode("TITLE_STYLE")));
                            }
                            else
                            {
                                item.Add("FIELD_TITLE", "");
                                item.Add("TITLE_ATTR", "");
                                item.Add("TITLE_LEFT", "");
                                item.Add("TITLE_RIGHT", "");
                                item.Add("TITLE_STYLE", "");
                            }

                            item.Add("REQ_MK", GetNodeText(fieldNode.SelectSingleNode("REQ_MK"), "N"));
                            item.Add("FORM_FIELD_CD", GetNodeText(dataNode.SelectSingleNode("TYPE_CD"), "0"));
                            item.Add("CODE_CD", GetNodeText(dataNode.SelectSingleNode("CODE_CD")));
                            item.Add("REL_1", GetNodeText(dataNode.SelectSingleNode("REL_1")));
                            item.Add("REL_2", GetNodeText(dataNode.SelectSingleNode("REL_2")));

                            item.Add("PREVIEW_MK", GetNodeText(dataNode.SelectSingleNode("PRE_MK"), "Y"));
                            item.Add("TABLE_TYPE", tableList.Last()["TABLE_TYPE"]);

                            if (item["FORM_FIELD_CD"].Equals("11") && item["FIELD_DEF"].Equals("TODAY"))
                            {
                                item["FIELD_DEF"] = DateTime.Now.ToString("yyyy/MM/dd");
                            }

                            fieldList.Add(item);

                            isFirst = false;
                        }
                    }
                    fields.Add(tableList.Last()["TABLE_ID"], fieldList);
                }

                data.Add("TABLE", tableList);
                data.Add("FIELD", fields);
            }
            catch (Exception e)
            {
                logger.Debug(e.Message, e);
            }

            return data;
        }

        public Dictionary<string, Dictionary<string, string>> GetFormTableDB(string xml, string serviceId)
        {
            Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                List<Dictionary<string, string>> fieldList = null;
                Dictionary<string, string> item = null;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNode root = doc.GetElementsByTagName("ROOT")[0];
                XmlNodeList tableNodeList = root.ChildNodes;

                foreach (XmlNode tableNode in tableNodeList)
                {
                    string tableName = GetNodeText(tableNode.SelectSingleNode("DB_NAME"));
                    if (data.ContainsKey(tableName))
                    {
                        item = data[tableName];
                    }
                    else
                    {
                        item = new Dictionary<string, string>();
                        item.Add("TABLE_ID", GetNodeText(tableNode.SelectSingleNode("ID")));
                        item.Add("TABLE_TYPE", GetNodeText(tableNode.SelectSingleNode("TYPE")).Substring(0, 1));
                        item.Add("TABLE_DB_NAME", tableName);
                        item.Add("FIELD_NAME", "");
                        item.Add("DATE_FIELD_NAME", "");
                    }

                    XmlNodeList fieldNodeList = tableNode.SelectSingleNode("DATA").ChildNodes;
                    fieldList = new List<Dictionary<string, string>>();

                    foreach (XmlNode fieldNode in fieldNodeList)
                    {
                        XmlNodeList dataNodeList = fieldNode.SelectSingleNode("FIELD").ChildNodes;
                        foreach (XmlNode dataNode in dataNodeList)
                        {
                            if (GetNodeText(dataNode.SelectSingleNode("DB_MK"), "N").Equals("Y"))
                            {
                                item["FIELD_NAME"] += GetNodeText(dataNode.SelectSingleNode("NAME")) + ",";
                                if (GetNodeText(dataNode.SelectSingleNode("TYPE_CD")).Equals("11"))
                                {
                                    item["DATE_FIELD_NAME"] += GetNodeText(dataNode.SelectSingleNode("NAME")) + ",";
                                }
                            }
                        }
                    }

                    if (data.ContainsKey(tableName))
                    {
                        data[tableName] = item;
                    }
                    else
                    {
                        data.Add(tableName, item);
                    }
                }

                string[] keys = data.Keys.ToArray();

                //logger.Debug("count: " + data.Count());

                foreach (string key in keys)
                {
                    item = data[key];

                    if (item["FIELD_NAME"].EndsWith(","))
                    {
                        item["FIELD_NAME"] = item["FIELD_NAME"].Substring(0, item["FIELD_NAME"].Length - 1);
                    }
                    if (item["DATE_FIELD_NAME"].EndsWith(","))
                    {
                        item["DATE_FIELD_NAME"] = item["DATE_FIELD_NAME"].Substring(0, item["DATE_FIELD_NAME"].Length - 1);
                    }

                    data[key] = item;

                    //logger.Debug(DataUtils.DictionaryToJsonString(item));
                }
            }
            catch (Exception e)
            {
                logger.Debug(e.Message, e);
            }

            return data;
        }

        public List<Dictionary<string, string>> GetFormTableList(string xml, string serviceId)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
            Dictionary<string, string> item = null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNode root = doc.GetElementsByTagName("ROOT")[0];
                XmlNodeList tableNodeList = root.ChildNodes;

                foreach (XmlNode tableNode in tableNodeList)
                {
                    item = new Dictionary<string, string>();
                    item.Add("SRV_ID", serviceId);
                    item.Add("TABLE_ID", GetNodeText(tableNode.SelectSingleNode("ID")));
                    item.Add("TABLE_TYPE", GetNodeText(tableNode.SelectSingleNode("TYPE")));
                    item.Add("TABLE_TITLE", GetNodeText(tableNode.SelectSingleNode("TITLE")));
                    item.Add("TABLE_DB_NAME", GetNodeText(tableNode.SelectSingleNode("DB_NAME")));

                    list.Add(item);
                }
            }
            catch (Exception e)
            {
                logger.Debug(e.Message, e);
            }

            return list;
        }

        /// <summary>
        /// 取得繳費資訊 (信用卡重新繳費)
        /// Add: 2014-03-04 Jay
        /// </summary>
        /// <param name="applyId"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetPayData(string applyId, string account)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.APP_ID, S.SRV_ID, S.NAME AS SRV_NAME,
                    (CASE 
                        WHEN S.FIX_UNIT_CD < 10 THEN (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = S.FIX_UNIT_CD)
                        ELSE (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = S.FIX_UNIT_CD))
                    END) AS PRO_UNIT_NAME,
                    ISNULL((SELECT NAME FROM ADMIN WHERE ACC_NO = A.PRO_ACC), '尚未分案') AS PRO_ACC_NAME,
                    A.APP_TIME, S.PRO_DEADLINE, S.RPT_NAME, A.PAY_METHOD,
                    (CASE WHEN A.PAY_A_FEE = A.PAY_A_PAID THEN 'Y' ELSE 'N' END) PAY_MK, PAY_A_FEE, PAY_A_FEEBK,
                    (PAY_A_FEE + PAY_A_FEEBK) AS PAY_FEE, S.PAY_ACCOUNT
                FROM APPLY A, SERVICE S
                WHERE A.DEL_MK = 'N'
                    AND A.SRV_ID = S.SRV_ID
                    AND A.ACC_NO = @ACC_NO
                    AND A.APP_ID = @APP_ID
            ";

            args.Add("ACC_NO", account);
            args.Add("APP_ID", applyId);

            return GetData(querySQL, args);
        }

        /// <summary>
        /// 使用SessionKey取得付費資訊
        /// Add: 2014-03-04 Jay
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetPayDataBySessionKey(string sessionKey)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.APP_ID, S.SRV_ID, S.NAME AS SRV_NAME,
                    (CASE 
                        WHEN S.FIX_UNIT_CD < 10 THEN (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = S.FIX_UNIT_CD)
                        ELSE (SELECT UNIT_NAME FROM UNIT WHERE UNIT_CD = (SELECT UNIT_PCD FROM UNIT WHERE UNIT_CD = S.FIX_UNIT_CD))
                    END) AS PRO_UNIT_NAME,
                    ISNULL((SELECT NAME FROM ADMIN WHERE ACC_NO = A.PRO_ACC), '尚未分案') AS PRO_ACC_NAME,
                    A.APP_TIME, S.PRO_DEADLINE, S.RPT_NAME, A.PAY_METHOD, A.NAME,
                    (CASE WHEN A.PAY_A_FEE = A.PAY_A_PAID THEN 'Y' ELSE 'N' END) PAY_MK, PAY_A_FEE, PAY_A_FEEBK,
                    (PAY_A_FEE + PAY_A_FEEBK) AS PAY_FEE, P.SESSION_KEY, S.PAY_ACCOUNT,
                    (SELECT COUNT(1) FROM APPLY_PAY WHERE APP_ID = A.APP_ID ) AS PAY_COUNT,
                    (SELECT COMP_DESC FROM SERVICE_NOTICE WHERE SRV_ID = A.SRV_ID) AS COMP_DESC
                FROM APPLY A, SERVICE S, APPLY_PAY P
                WHERE A.DEL_MK = 'N'
                    AND A.SRV_ID = S.SRV_ID
                    AND A.APP_ID = P.APP_ID
                    AND P.SESSION_KEY = @SESSION_KEY
            ";

            args.Add("SESSION_KEY", sessionKey);

            return GetData(querySQL, args);
        }

        public List<Dictionary<string, object>> GetFileList(string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("APP_ID", applyId);

            string querySQL = @"
                SELECT APP_ID, FILE_NO, FILENAME, SRC_FILENAME
                FROM APPLY_FILE
                WHERE DEL_MK = 'N'
                  AND APP_ID = @APP_ID
                ORDER BY APP_ID
            ";

            return GetList(querySQL, args);
        }

        private string GetNodeText(XmlNode node)
        {
            return (node == null) ? "" : node.InnerText;
        }

        private string GetNodeText(XmlNode node, string def)
        {
            return (node == null) ? def : node.InnerText;
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


        public string CheckExpirDate(string SDate, string EDate, string applyId)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("APP_ID", applyId);
            args.Add("SDate", SDate);
            args.Add("EDate", EDate);

            string querySQL = @"
                SELECT case when COUNT(*) > 0 then 'T' else 'F' end  
                FROM APPLY_005004 WHERE ADD_ACC = @APP_ID
                    AND convert(datetime,ADD_TIME,103) between @SDate AND @EDate AND DEL_MK='N'
            ";

            return GetSingleValue(querySQL, args);
        }

        public string LeadSave(Models.LeadModel.DocumentFormat data)
        {
            string sql = @"INSERT INTO APPLY_DOCUMENT (NO,APP_ID,SRV_NO,DOC_NO,ADDRESS,NAME,TEL,FAX,MAIL,LIC_NUM,DRUG_NAME,APP_COUNT,AMOUNT,PAYMETHOD,OTHER1,OTHER2,OTHER3,OTHER4,OTHER5,UNIT_NAME) 
OUTPUT Inserted.NO
VALUES ((select Convert(varchar(10),Getdate(),112)+ REPLICATE('0',4-Len(substring(isnull(MAX(no),Convert(varchar(10),Getdate(),112)+'0000'),9,4)+1)) + RTRIM(CAST(substring(isnull(MAX(no),Convert(varchar(10),Getdate(),112)+'0000'),9,4)+1 AS CHAR))  from APPLY_DOCUMENT where substring(NO,1,8)=Convert(varchar(10),Getdate(),112))
,@APP_ID,@SRV_NO,@DOC_NO,@ADDRESS,@NAME,@TEL,@FAX,@MAIL,@LIC_NUM,@DRUG_NAME,@APP_COUNT,@AMOUNT,@PAYMETHOD,@OTHER1,@OTHER2,@OTHER3,@OTHER4,@OTHER5,@UNIT_NAME)";
            SqlCommand cmd = new SqlCommand(sql, conn);
            DataUtils.AddParameters(cmd, "APP_ID", "");
            DataUtils.AddParameters(cmd, "SRV_NO", data.SRV_NO);
            DataUtils.AddParameters(cmd, "DOC_NO", "");
            DataUtils.AddParameters(cmd, "ADDRESS", data.Address);
            DataUtils.AddParameters(cmd, "NAME", data.Name);
            DataUtils.AddParameters(cmd, "TEL", data.Tel);
            DataUtils.AddParameters(cmd, "FAX", data.Fax);
            DataUtils.AddParameters(cmd, "MAIL", data.EMail);
            DataUtils.AddParameters(cmd, "LIC_NUM", data.LicNum);
            DataUtils.AddParameters(cmd, "DRUG_NAME", data.DrugName);
            DataUtils.AddParameters(cmd, "APP_COUNT", data.AppCnt);
            DataUtils.AddParameters(cmd, "AMOUNT", data.TotalAmount);
            DataUtils.AddParameters(cmd, "PAYMETHOD", data.PayMethod);
            DataUtils.AddParameters(cmd, "OTHER1", data.Caption4);
            DataUtils.AddParameters(cmd, "OTHER2", data.Caption5);
            DataUtils.AddParameters(cmd, "OTHER3", data.Caption6);
            DataUtils.AddParameters(cmd, "OTHER4", data.Caption7);
            DataUtils.AddParameters(cmd, "OTHER5", data.Caption8);
            DataUtils.AddParameters(cmd, "UNIT_NAME", data.Title);


            return cmd.ExecuteScalar().ToString();

        }

        public Dictionary<string, string> GetDocumentBase(string SRV_ID)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string sql = @"
                SELECT *
                FROM DOCUMENT_BASE
                WHERE SRV_ID = @SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("SRV_ID", SRV_ID);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        dict.Add(dr.GetName(i), DataUtils.DecodeValue(dr.GetValue(i).ToString()));
                    }
                }
                dr.Close();
            }
            return dict;
        }

        public string GetAPP_FEE(string SRV_ID)
        {
            string sql = "select APP_FEE from SERVICE where SRV_ID=@SRV_ID";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("SRV_ID", SRV_ID);

            return cmd.ExecuteScalar().ToString(); ;
        }
    }
}