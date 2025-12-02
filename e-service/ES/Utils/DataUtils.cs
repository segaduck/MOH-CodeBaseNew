using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using log4net;
using System.Web.Script.Serialization;
using System.Data;

namespace ES.Utils
{
    public class DataUtils
    {
        private static Dictionary<string, string> config;
        private static readonly ILog logger = LogUtils.GetLogger();

        public static string GetConfig(string key)
        {
            if (config == null)
            {
                using (SqlConnection conn = GetConnection())
                {
                    OpenDbConn(conn); //conn.Open();

                    string querySQL = "SELECT SETUP_CD, SETUP_VAL FROM SETUP WHERE DEL_MK = 'N'";

                    SqlCommand cmd = new SqlCommand(querySQL, conn);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        config = new Dictionary<string, string>();
                        while (dr.Read())
                        {
                            config.Add(GetDBString(dr, 0), GetDBString(dr, 1));
                        }
                        dr.Close();
                    }
                    CloseDbConn(conn);
                }
            }

            if (config.ContainsKey(key)) { return config[key]; }

            // 20200504, eric, add logging information
            logger.Debug("GetConfig: " + key + " NOT exists.");
            return "";
        }

        /// <summary>
        /// 取得信用卡付費帳號
        /// </summary>
        /// <param name="serialNo"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPayAccount(int serialNo)
        {
            Dictionary<string, string> item = new Dictionary<string, string>();

            using (SqlConnection conn = GetConnection())
            {
                OpenDbConn(conn); //conn.Open();

                string querySQL = "SELECT ACCOUNT, PSWD, OID, SID, PAY_DESC FROM PAY_ACCOUNT WHERE SRL_NO = @SRL_NO ";

                SqlCommand cmd = new SqlCommand(querySQL, conn);
                AddParameters(cmd, "SRL_NO", serialNo);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        item.Add("ACCOUNT", GetDBString(dr, 0));
                        item.Add("PSWD", GetDBString(dr, 1));
                        item.Add("OID", GetDBString(dr, 2));
                        item.Add("SID", GetDBString(dr, 3));
                        item.Add("PAY_DESC", GetDBString(dr, 4));
                    }
                    dr.Close();
                }
                CloseDbConn(conn);
            }

            return item;
        }

        public static Dictionary<string, string> GetPayAccountOID(string OID)
        {
            Dictionary<string, string> item = new Dictionary<string, string>();

            using (SqlConnection conn = GetConnection())
            {
                OpenDbConn(conn); //conn.Open();

                string querySQL = "SELECT ACCOUNT, PSWD, OID, SID, PAY_DESC FROM PAY_ACCOUNT WHERE OID = @OID ";

                SqlCommand cmd = new SqlCommand(querySQL, conn);
                AddParameters(cmd, "OID", OID);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        item.Add("ACCOUNT", GetDBString(dr, 0));
                        item.Add("PSWD", GetDBString(dr, 1));
                        item.Add("OID", GetDBString(dr, 2));
                        item.Add("SID", GetDBString(dr, 3));
                        item.Add("PAY_DESC", GetDBString(dr, 4));
                    }
                    dr.Close();
                }
                CloseDbConn(conn);
            }

            return item;
        }

        public static void ClearConfig()
        {
            config = null;
        }

        /// <summary>
        /// 設定SQL參數
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddParameters(SqlCommand cmd, string key, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                cmd.Parameters.AddWithValue(key, DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue(key, EncodeValue(value.Trim()));
            }
        }

        /// <summary>
        /// 設定SQL參數
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddParameters(SqlCommand cmd, string key, bool value)
        {
            cmd.Parameters.AddWithValue(key, value ? "Y" : "N");
        }

        /// <summary>
        /// 設定SQL參數
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddParameters(SqlCommand cmd, string key, object value)
        {
            if (value == null)
            {
                cmd.Parameters.AddWithValue(key, DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue(key, value);
            }
        }

        /// <summary>
        /// 取得資料庫欄位值，NULL時回傳空字串
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetDBString(SqlDataReader dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return "";
            }
            else
            {
                return DecodeValue(dr.GetString(i));
            }
        }

        public static string[] GetDBStringArray(SqlDataReader dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return StringToArray("");
            }
            else
            {
                return StringToArray(dr.GetString(i));
            }
        }

        public static string[] GetDBStringArray(SqlDataReader dr, int i, string split)
        {
            if (dr.IsDBNull(i))
            {
                return StringToArray("");
            }
            else
            {
                return dr.GetString(i).Split(split.ToCharArray());
            }
        }

        public static int GetDBInt(SqlDataReader dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return 0;
            }
            return dr.GetInt32(i);
        }

        public static decimal GetDBDecimal(SqlDataReader dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return 0;
            }
            return dr.GetDecimal(i);
        }

        public static DateTime? GetDBDateTime(SqlDataReader dr, int i)
        {
            if (dr.IsDBNull(i))
            {
                return null;
            }
            return dr.GetDateTime(i);
        }

        public static object GetDBValue(SqlDataReader dr, int i)
        {
            if (dr.GetValue(i) != null && dr.GetValue(i).GetType() == "".GetType())
            {
                return DataUtils.GetDBString(dr, i);
            }

            return dr.GetValue(i);

        }

        public static string[] StringToArray(string value)
        {
            char[] c = value.ToCharArray();
            string[] values = new string[c.Length];

            for (int i = 0; i < c.Length; i++)
            {
                values[i] = c[i].ToString();
            }

            return values;
        }

        public static string StringArrayToString(string[] array)
        {
            if (array == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (string value in array)
            {
                sb.Append(value);
            }
            return sb.ToString();
        }

        public static string StringArrayToString(string[] array, string split)
        {
            if (array == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (string value in array)
            {
                sb.Append(value).Append(split);
            }

            if (sb.Length >= split.Length)
            {
                return sb.ToString().Substring(0, sb.Length - split.Length);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 使用SHA256加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Crypt256(string textToEncrypt)
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider(); // 建立一個SHA256
            byte[] source = Encoding.Default.GetBytes(textToEncrypt); // 將字串轉為Byte[]
            byte[] crypto = sha256.ComputeHash(source);// 進行SHA256加密
            string result = Convert.ToBase64String(crypto);//把加密後的字串從Byte[]轉為字串

            return result;
        }
        public static string Crypt256BitConverter(string textToEncrypt)
        {
            byte[] source = Encoding.Default.GetBytes(textToEncrypt); // 將字串轉為Byte[]
            string result = string.Empty;//把加密後的字串從Byte[]轉為字串
            using (SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider())
            {
                byte[] hashMessage = csp.ComputeHash(source);// 進行SHA256加密
                result = BitConverter.ToString(hashMessage).Replace("-", string.Empty).ToLower();
            }
            return result;
        }
        public static string CryptHMAC256(string textToEncrypt, string key)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(textToEncrypt);
            using (var hmacSHA256 = new HMACSHA256(keyByte))
            {
                byte[] hashMessage = hmacSHA256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
            }
        }
        //MD5加密方法寫法
        public static string CryptMD5(string str)
        {
            //建立MD5對像
            MD5 md5 = MD5.Create();
            //將字串轉換成陣列
            byte[] ba = Encoding.Default.GetBytes(str);
            //將陣列加密 成  加密陣列
            byte[] md55 = md5.ComputeHash(ba);
            //將加密陣列編譯成字串
            // return Encoding.Default.GetString(md55);
            //
            string STR = "";
            //便利陣列中元素轉化成字元並拼接
            for (int I = 0; I < md55.Length; I++)
            {
                //X 表是10進位制,X2表示16進位制
                STR += md55[I].ToString("x2").ToLower();
            }
            return STR;
        }
        /// <summary>
        /// 使用Unix Crypt加密
        /// </summary>
        /// <param name="textToEncrypt"></param>
        /// <returns></returns>
        public static string Crypt(string textToEncrypt)
        {
            return UnixCrypt.Crypt(textToEncrypt);
        }

        /// <summary>
        /// 使用Unix Crypt加密
        /// </summary>
        /// <param name="encryptionSalt"></param>
        /// <param name="textToEncrypt"></param>
        /// <returns></returns>
        public static string Crypt(string encryptionSalt, string textToEncrypt)
        {
            return UnixCrypt.Crypt(encryptionSalt, textToEncrypt);
        }

        /// <summary>
        /// 取得資料庫連線
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            SqlConnection conn = new SqlConnection(connectionString);
            return conn;
        }

        /// <summary>
        /// 字串列表轉字串
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ListToString(List<string> list)
        {
            return StringArrayToString(list.ToArray(), ",");
        }

        /// <summary>
        /// 字串轉字串列表
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<string> StringToList(string value)
        {

            return value.Split(',').ToList();
        }

        /// <summary>
        /// Json字串 轉 Dictionary
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, string> JsonStringToDictionary(string json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Deserialize<Dictionary<string, string>>(json);
        }

        /// <summary>
        /// Dictionary 轉 Json字串
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string DictionaryToJsonString(Dictionary<string, string> json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(json);
        }

        /// <summary>
        /// Base64編碼轉成明文
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FromBase64String(string text)
        {
            string rst = null;
            if (String.IsNullOrEmpty(text)) { return ""; }
            try
            {
                rst = Encoding.UTF8.GetString(Convert.FromBase64String(text));
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);/* throw; */
            }
            return rst;
        }

        /// <summary>
        /// 明文轉成Base64編碼
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToBase64String(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        public static String EncodeValue(String value)
        {
            int unicode;
            char[] src = value.ToCharArray();
            Encoding big5 = Encoding.GetEncoding("big5");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < src.Length; i++)
            {

                unicode = (int)src[i];

                if (src[i].ToString().Equals(big5.GetString(big5.GetBytes(src[i].ToString()))))
                {
                    sb.Append(src[i]);
                }
                else
                {
                    sb.Append("&#" + unicode + ";");
                }
            }

            return sb.ToString();
        }

        public static String DecodeValue(String value)
        {
            if (value.IndexOf("&#") < 0 || value.IndexOf(";") < 0)
            {
                return value;
            }

            StringBuilder sb = new StringBuilder();
            //以;將文字拆成陣列
            string[] tmp = value.Split(';');
            //檢查最後一個字元是否為【;】，因為有【英文】、【阿拉伯數字】、【&#XXXX;】
            //若最後一個要處理的字並非HTML UNICODE則不進行處理
            bool Process_last = value.Substring(value.Length - 1, 1).Equals(";");
            //Debug.WriteLine(tmp.Length + "");
            for (int i = 0; i < tmp.Length; i++)
            {
                //以&#將文字拆成陣列
                string[] tmp2 = tmp[i].Split(new string[] { "&#" }, StringSplitOptions.RemoveEmptyEntries);
                if (tmp2.Length == 1)
                {
                    //如果長度為1則試圖轉換UNICODE回字符，若失敗則使用原本的字元
                    if (i != tmp.Length - 1)
                    {
                        try
                        {
                            sb.Append(Convert.ToChar(Convert.ToInt32(int.Parse(tmp2[0]))).ToString());
                        }
                        catch
                        {
                            sb.Append(tmp2[0]);
                        }
                    }
                    else
                    {
                        sb.Append(tmp2[0]);
                    }
                }
                if (tmp2.Length == 2)
                {
                    //若長度為2，則第一項不處理，只處理第二項即可
                    sb.Append(tmp2[0]);
                    sb.Append(Convert.ToChar(Convert.ToInt32(int.Parse(tmp2[1]))).ToString());
                }
            }


            return sb.ToString();
        }

        /// <summary> 開啟測試連線資料庫 </summary>
        /// <param name="objconn"></param>
        /// <returns></returns>
        public static bool OpenDbConn(SqlConnection objconn)
        {
            bool rst = true;
            if (objconn == null) { return false; }
            if (objconn.State == ConnectionState.Closed) { objconn.Open(); }
            return rst;
        }

        /// <summary> 關閉測試連線資料庫 </summary>
        /// <param name="objconn"></param>
        public static void CloseDbConn(SqlConnection objconn)
        {
            if (objconn == null) { return; }
            try
            {
                if (objconn.State == ConnectionState.Open) { objconn.Close(); }
            }
            finally { };
            objconn.Dispose();
            objconn = null;
            return;
        }

    }
}