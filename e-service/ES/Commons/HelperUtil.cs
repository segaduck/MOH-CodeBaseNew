using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace ES.Commons
{
    /// <summary>
    /// 定義一些在 Controller 或 ViewModel 中會用到 Helper method
    /// </summary>
    public class HelperUtil
    {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>純文字的 ContentType MIME</summary>
        public static string TEXTContentType
        {
            get { return "text/plain"; }
        }

        /// <summary>XML 的 ContentType MIME</summary>
        public static string XMLContentType
        {
            /* 請勿使用 text/xml。
             * 參考下列文章說明：https://www.iteye.com/blog/songyishan-1073969
             */
            get { return "application/xml"; }
        }

        /// <summary>
        /// Word 2007/2010(.docx) 的 ContentType MIME 
        /// </summary>
        public static string DOCXContentType
        {
            get
            {
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }
        }

        /// <summary>
        /// Excel 2007(.xls) 的 ContentType MIME 
        /// </summary>
        public static string XLSContentType
        {
            get
            {
                return "application/vnd.ms-excel";
            }
        }

        /// <summary>
        /// Excel 2010(.xlsx) 的 ContentType MIME 
        /// </summary>
        public static string XLSXContentType
        {
            get
            {
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
        }

        /// <summary>
        /// ODS 試算表的 ContentType MIME (OpenOffice 2.0/StarOffice 8 and later)
        /// </summary>
        public static string ODSContentType
        {
            get
            {
                return "application/vnd.oasis.opendocument.spreadsheet";
            }
        }

        /// <summary>
        /// ODT Writer 的 ContentType MIME (OpenOffice 2.0/StarOffice 8 and later)
        /// </summary>
        public static string ODTContentType
        {
            get
            {
                return "application/vnd.oasis.opendocument.text";
            }
        }

        /// <summary>
        /// PDF 的 ContentType MIME 
        /// </summary>
        public static string PDFContentType
        {
            get
            {
                return "application/pdf";
            }
        }

        /// <summary>ZIP 的 ContentType MIME</summary>
        public static string ZIPContentType
        {
            get
            {
                return "application/x-zip-compressed";
            }
        }

        /// <summary>未知檔案類型的 ContentType MIME</summary>
        public static string OctetStreamContentType
        {
            get
            {
                return "application/octet-stream";
            }
        }

        /// <summary>
        /// 將 DateTime 轉換為 民國年日期字串(YYY/MM/dd), 若date為Null則回傳 null,
        /// delimiter 參數用來指定民國年的日期的分隔字元, 預設是 '/'
        /// </summary>
        public static string TransToTwYear(DateTime? date, string delimiter = "/")
        {
            string rtn = null;
            if (date == null) { return rtn; }
            if (!date.HasValue) { return rtn; }
            if (((DateTime)date).Year < 1753) { return rtn; }

            int twYear = date.Value.Year - 1911;
            rtn = string.Format("{0}{1}{2}{1}{3}", twYear, delimiter, date.Value.ToString("MM"), date.Value.ToString("dd"));
            return rtn;
        }

        /// <summary>
        /// 將 yyyy/MM/dd 格式的字串轉換為 民國年日期字串(YYY/MM/dd), 
        /// 若strDate為Null或格式不合法則回傳 null,
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/'
        /// </summary>
        public static string TransToTwYear(string strDate, string delimiter = "/")
        {
            string TwY = null;
            if (string.IsNullOrEmpty(strDate)) { return TwY; }
            DateTime? dt = null;

            try
            {
                dt = TransToDateTime(strDate, delimiter);
                TwY = TransToTwYear(dt);
            }
            catch (FormatException ex)
            {
                LOG.Warn("TransToTwYear('" + strDate + "'): " + ex.Message);
            }
            return TwY;
        }

        /// <summary>
        /// 將西元年日期格式的字串轉為 DateTime, 
        /// 若 strDate 為 Null 或 Empty 則回傳 null,
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/'
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static DateTime? TransToDateTime(string strDate, string delimiter = "/")
        {
            DateTime? date = null;
            //LOG.Warn("TransToDateTime: strDate is Null or Empty");
            if (string.IsNullOrEmpty(strDate)) { return date; }

            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                string format = string.Format("yyyy{0}MM{0}dd", delimiter);
                date = DateTime.ParseExact(strDate, format, provider);
            }
            catch (Exception ex)
            {
                LOG.Warn("TransToDateTime(" + strDate + ", " + delimiter + "): " + ex.Message, ex);
            }
            return date;
        }

        /// <summary>
        /// 將西元年日期格式的字串轉為 DateTime, 
        /// 若 strDate 為 Null 或 Empty 則回傳 null,
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/'
        /// </summary>
        /// <param name="strDate"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static DateTime? TransToDateTimeReal(string strDate, string delimiter = "/")
        {
            //LOG.Warn("TransToDateTime: strDate is Null or Empty");
            if (string.IsNullOrEmpty(strDate)) { return null; }

            DateTime? date = null;
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                string format = string.Format("yyyy{0}MM{1}dd HH:mm:ss", delimiter, delimiter);
                date = DateTime.ParseExact(strDate, format, provider);
            }
            catch (Exception ex)
            {
                LOG.Warn("TransTwToDateTime(" + strDate + ", " + delimiter + "): " + ex.Message, ex);
            }
            return date;
        }

        /// <summary>
        /// 將民國年日期格式的字串(YYY/MM/DD or YYYMMDD)轉為 DateTime, 
        /// 若 strDate 為 Null 或 Empty 則回傳 null,
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/', 
        /// 若 delimiter 為空字串, 則以固定格式 YYYMMDD 進行轉換
        /// </summary>
        /// <param name="strTwDate"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static DateTime? TransTwToDateTime(string strTwDate, string delimiter = "/")
        {
            if (string.IsNullOrEmpty(strTwDate))
            {
                //LOG.Warn("TransTwToDateTime: strTwDate is Null or Empty");
                return null;
            }

            DateTime? date = null;
            try
            {
                string strDate = TransTwToAdYear(strTwDate, delimiter);
                if (strDate != null)
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    string format = string.Format("yyyy{0}MM{1}dd", delimiter, delimiter);
                    date = DateTime.ParseExact(strDate, format, provider);
                }
            }
            catch (Exception ex)
            {
                LOG.Warn("TransTwToDateTime(" + strTwDate + "): " + ex.Message, ex);
            }
            return date;
        }

        /// <summary>
        /// 將民國年日期加時間(yyyMMddHHmmss)格式的字串轉為 DateTime, 
        /// 若 strDate 為 Null 或 Empty 則回傳 null
        /// </summary>
        /// <param name="strTwDate"></param>
        /// <returns></returns>
        public static DateTime? TransTwLongToDateTime(string strTwDate)
        {
            if (string.IsNullOrEmpty(strTwDate))
            {
                //LOG.Warn("TransTwLongToDateTime: strTwDate is Null or Empty");
                return null;
            }
            if (strTwDate.Trim().Length != 13)
            {
                LOG.Warn("TransTwLongToDateTime(" + strTwDate + "): strTwDate 字串格式長度不等於 13");
                return null;
            }

            DateTime? date = null;

            try
            {
                string sDate = strTwDate.Trim().Substring(0, 7);
                string sTime = strTwDate.Trim().Substring(7, 6);

                string strDate = TransTwToAdYear(sDate, "") + sTime;
                if (strDate != null)
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    string format = string.Format("yyyyMMddHHmmss");
                    date = DateTime.ParseExact(strDate, format, provider);
                }
            }
            catch (Exception ex)
            {
                LOG.Warn("TransTwLongToDateTime(" + strTwDate + "): " + ex.Message, ex);
            }
            return date;
        }

        /// <summary>
        /// 將西元年日期加時間(yyyyMMddHHmmss)格式的字串轉為 DateTime, 
        /// 若 strDateTime 為 Null 或 Empty 則回傳 null
        /// </summary>
        /// <param name="strDateTime"></param>
        /// <returns></returns>
        public static DateTime? TransLongToDateTime(string strDateTime)
        {
            if (string.IsNullOrEmpty(strDateTime))
            {
                //LOG.Warn("TransLongToDateTime: strDateTime is Null or Empty");
                return null;
            }
            if (strDateTime.Trim().Length != 14)
            {
                LOG.Warn("TransLongToDateTime(" + strDateTime + "): strDateTime 字串格式長度不等於 14");
                return null;
            }

            DateTime? date = null;
            try
            {
                //string sDate = strDateTime.Trim().Substring(0, 8);
                //string sTime = strDateTime.Trim().Substring(8, 6);

                CultureInfo provider = CultureInfo.InvariantCulture;
                string format = string.Format("yyyyMMddHHmmss");
                date = DateTime.ParseExact(strDateTime, format, provider);
            }
            catch (Exception ex)
            {
                LOG.Warn("TransLongToDateTime(" + strDateTime + "): " + ex.Message, ex);
            }
            return date;
        }

        /// <summary>
        /// 將 DateTime 轉換為西元年日期字串 (yyyy/MM/dd), 
        /// 若date為null則回傳null,
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/'
        /// </summary>
        /// <param name="date"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string DateTimeToString(DateTime? date, string delimiter = "/")
        {
            string result = null;
            if (date == null) { return result; }
            if (!date.HasValue) { return result; }
            if (((DateTime)date).Year < 1753) { return result; }

            result = ((DateTime)date).ToString(string.Format("yyyy{0}MM{0}dd", delimiter));
            return result;
        }

        public static DateTime? ConvertDateTime(object oDateTime)
        {
            DateTime? nulldate = null;
            if (oDateTime == null) { return nulldate; }
            else nulldate = (DateTime)oDateTime;
            if (nulldate.ToString().Length > 0) { nulldate = ConvertDateTime(nulldate.ToString()); }
            return nulldate;
        }

        public static DateTime? ConvertDateTime(string sDateTime)
        {
            DateTime? nulldate = null;
            DateTime date;
            if (!DateTime.TryParse(sDateTime, out date))
            {
                return nulldate;
            }
            nulldate = date;
            if (((DateTime)date).Year < 1753) { nulldate = null; }
            return nulldate;
        }

        /// <summary>
        /// 將 DateTime 轉換為西元年日期加時間字串 (yyyyMMddHHmmss), 
        /// 若date為null則回傳null
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateTimeToLongString(DateTime? date)
        {
            string result = null;
            if (date == null) { return result; }
            if (!date.HasValue) { return result; }
            if (((DateTime)date).Year < 1753) { return result; }

            result = ((DateTime)date).ToString(string.Format("yyyyMMddHHmmss"));
            return result;
        }

        /// <summary>
        /// 將 DateTime 轉換為民國年日期加時間字串 (yyyMMddHHmmss), 
        /// 若date為null則回傳null
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateTimeToLongTwString(DateTime? date)
        {
            string result = null;
            if (date == null) { return result; }
            if (!date.HasValue) { return result; }
            if (((DateTime)date).Year < 1753) { return result; }

            string twYr = (((DateTime)date).Year - 1911).ToString("000");
            result = twYr + ((DateTime)date).ToString(string.Format("MMddHHmmss"));
            return result;
        }

        /// <summary>
        /// 將 DateTime 轉換為民國年日期字串 (yyy/MM/dd), 
        /// 若date為null則回傳null,
        /// delimiter 參數用來指定日期的分隔字元 (預設是 '/', 年度小於100不會補0), 
        /// 若分隔字元為 "", 則回傳固定長度(7碼)的 yyyMMdd 字串(若年度小於100前面會補0)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string DateTimeToTwString(DateTime? date, string delimiter = "/")
        {
            string result = null;
            if (date == null) { return result; }
            if (!date.HasValue) { return result; }
            if (((DateTime)date).Year < 1753) { return result; }

            string twYr = (((DateTime)date).Year - 1911).ToString("".Equals(delimiter) ? "000" : "###");
            result = string.Format("{1}{0}{2}", delimiter, twYr, ((DateTime)date).ToString(string.Format("MM{0}dd", delimiter)));
            return result;
        }

        /// <summary>
        /// 將 DateTime 轉換為民國年日期加時間字串 (yyy/MM/dd HH:mm:ss), 
        /// 若date為null則回傳null,
        /// delimiter 參數用來指定日期的分隔字元 (預設是 '/', 年度小於100不會補0), 
        /// 若分隔字元為 "", 則回傳固定長度(7碼)的 yyyMMdd 字串(若年度小於100前面會補0)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string DateTimeToTwFormatLongString(DateTime? date, string delimiter = "/")
        {
            string result = null;
            if (date == null) { return result; }
            if (!date.HasValue) { return result; }
            if (((DateTime)date).Year < 1753) { return result; }

            string twDate = DateTimeToTwString(date, delimiter);
            result = twDate + " " + ((DateTime)date).ToString("HH:mm:ss");
            return result;
        }

        /// <summary>
        /// 將 DateTime 轉換為西元年日期加時間字串 (yyyy/MM/dd HH:mm:ss), 
        /// 若date為null則回傳null,
        /// delimiter 參數用來指定日期的分隔字元 (預設是 '/', 年度小於100不會補0), 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string DateTimeToADFormatLongString(DateTime? date, string delimiter = "/")
        {
            string result = null;
            if (date == null) { return result; }
            if (!date.HasValue) { return result; }
            if (((DateTime)date).Year < 1753) { return result; }

            string twDate = DateTimeToString(date, delimiter);
            result = twDate + " " + ((DateTime)date).ToString("HH:mm:ss");
            return result;
        }

        /// <summary>
        /// 將 西元件年月字串(yyyy/MM) 轉成 民國年月字串(YYY/MM),
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/',
        /// 若 delimiter 為 null 或空字串, 則以固定格式 YYYYMM 進行轉換
        /// </summary>
        /// <param name="date"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string TransToTwYearMonth(string date, string delimiter = "/")
        {
            string rtn = null;
            if (string.IsNullOrEmpty(date)) { return rtn; }
            delimiter = delimiter ?? "";

            try
            {
                if (!string.IsNullOrEmpty(date))
                {
                    string[] arr = null;
                    if ("".Equals(delimiter))
                    {
                        // 無分隔符號, 固定長度格式: YYYYMM
                        if (date.Length == 6)
                        {
                            arr = new string[2];
                            arr[0] = date.Substring(0, 4);
                            arr[1] = date.Substring(4, 2);
                        }
                    }
                    else
                    {
                        arr = date.Split(delimiter[0]);
                    }

                    if (arr.Length == 2)
                    {
                        int year;
                        if (int.TryParse(arr[0], out year))
                        {
                            rtn = string.Format("{0}{1}{2}", (year - 1911), delimiter, arr[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Warn("TransToTwYearMonth(" + date + ", " + delimiter + "): " + ex.Message, ex);
            }
            return rtn;
        }

        /// <summary>
        /// 將 民國年月字串(YYY/MM) 轉成 西元件年月字串(yyyy/MM), 
        /// 若 date字串為null, 則回傳 null,
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/',
        /// 若 delimiter 為 null 或空字串, 則以固定格式 YYYMM 進行轉換
        /// </summary>
        /// <param name="date"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string TransToAdYearMonth(string date, string delimiter = "/")
        {
            string rtn = null;
            if (string.IsNullOrEmpty(date)) { return rtn; }
            delimiter = delimiter ?? "";

            try
            {
                if (!string.IsNullOrEmpty(date))
                {
                    string[] arr = null;
                    if ("".Equals(delimiter))
                    {
                        // 無分隔符號, 固定長度格式: YYYMM
                        if (date.Length == 5)
                        {
                            arr = new string[2];
                            arr[0] = date.Substring(0, 3);
                            arr[1] = date.Substring(3, 2);
                        }
                    }
                    else
                    {
                        arr = date.Split(delimiter[0]);
                    }

                    if (arr != null && arr.Length == 2)
                    {
                        int year;
                        if (int.TryParse(arr[0], out year))
                        {
                            rtn = string.Format("{0}{1}{2}", (year + 1911), delimiter, arr[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Warn("TransToAdYearMonth(" + date + ", " + delimiter + "): " + ex.Message, ex);
            }

            return rtn;
        }

        /// <summary>
        /// 將 民國年格式日期字串(YYY/MM/DD) 轉成 西元年格式日期字串(yyyy/MM/DD), 
        /// 若 strTwDate 字串為 null 或格式不符, 則回傳 null,
        /// delimiter 參數用來指定日期的分隔字元, 預設是 '/',
        /// 若 delimiter 為 null 或空字串, 則以固定格式 YYYMMDD 進行轉換
        /// </summary>
        /// <param name="strTwDate"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string TransTwToAdYear(string strTwDate, string delimiter = "/")
        {
            string rtn = null;
            if (string.IsNullOrEmpty(strTwDate)) { return rtn; }
            delimiter = delimiter ?? "";

            if (!string.IsNullOrEmpty(strTwDate))
            {
                string[] arr = null;
                if ("".Equals(delimiter))
                {
                    if (strTwDate.Length == 6)
                    {
                        // 無分隔符號, 固定長度格式: YYMMDD
                        strTwDate = "0" + strTwDate;
                    }

                    // 無分隔符號, 固定長度格式: YYYMMDD
                    if (strTwDate.Length == 7)
                    {
                        arr = new string[3];
                        arr[0] = strTwDate.Substring(0, 3);
                        arr[1] = strTwDate.Substring(3, 2);
                        arr[2] = strTwDate.Substring(5, 2);
                    }
                }
                else
                {
                    arr = strTwDate.Split(delimiter[0]);
                }

                if (arr != null && arr.Length == 3)
                {
                    int year;
                    if (int.TryParse(arr[0], out year))
                    {
                        rtn = string.Format("{0}{1}{2}{3}{4}", (year + 1911), delimiter, arr[1], delimiter, arr[2]);
                    }
                }
            }
            return rtn;
        }


        /// <summary>
        /// 將 yyyy/MM 或 yyy/MM 日期年月文字格式字串 分割(split) 為2個元素的字串陣列, 
        /// 如果 yrMon 為 null 則回傳2個元素的空字串陣列
        /// </summary>
        /// <param name="yrMon"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string[] SplitYearMonth(string yrMon, char delimiter = '/')
        {
            string[] parts = new string[2];
            if (!string.IsNullOrEmpty(yrMon))
            {
                string[] t = yrMon.Split(delimiter);
                for (int i = 0; i < 2 && i < t.Length; i++)
                {
                    parts[i] = t[i];
                }
            }

            return parts;
        }

        /// <summary>
        /// 取得code book對應的文字
        /// </summary>
        public static string GetMapText(Dictionary<string, string> codeMap, string code)
        {
            string rtn = string.Empty;
            if (codeMap != null && !string.IsNullOrEmpty(code) && codeMap.ContainsKey(code))
            {
                rtn = codeMap[code];
            }
            return rtn;
        }

        /// <summary>
        /// 在代碼清單 list 中取得指定 code 對應的文字
        /// </summary>
        /// <param name="list"></param>
        /// <param name="codeField"></param>
        /// <param name="textField"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetListCodeText(IList<Hashtable> list, string codeField, string textField, string code)
        {
            string rtn = string.Empty;
            if (list != null
                && !string.IsNullOrEmpty(codeField)
                && !string.IsNullOrEmpty(textField)
                && !string.IsNullOrEmpty(code))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Hashtable row = list[i];
                    if (code.Equals(row[codeField]))
                    {
                        rtn = (string)row[textField];
                        break;
                    }
                }
            }
            return rtn;
        }

        /// <summary>
        /// 取得利用分隔符號連接之字串
        /// </summary>
        /// <param name="codeList"></param>
        /// <param name="cutChar"></param>
        /// <returns></returns>
        public static IList<string> GetSplitedItemList(string codeList, char cutChar)
        {
            List<string> rtn = new List<string>();
            if (!string.IsNullOrEmpty(codeList))
            {
                string[] arr = codeList.Split(new char[] { cutChar });
                if (arr.Count() > 0)
                {
                    rtn = arr.ToList();
                }
            }
            return rtn;
        }

        /// <summary>
        /// 檢核指定的 value 是否存在於 list 中
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsContains(string[] list, string value)
        {
            if (list == null || value == null)
            {
                return false;
            }

            bool found = false;
            for (int i = 0; i < list.Length; i++)
            {
                if (value.Equals(list[i]))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        /// <summary>
        /// 判斷是否為正整數(>=0)
        /// </summary>
        /// <param name="strNumber"></param>
        /// <returns></returns>
        public static bool IsNumber(string strNumber)
        {
            string strValue = @"^\d+$";//非負整數
            if (strNumber == null)
            { return false; }
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(strValue);
            return r.IsMatch(strNumber);
        }
        /// <summary>
        /// 判斷是否為正整、浮點數(>=0)
        /// </summary>
        /// <param name="strDouble"></param>
        /// <returns></returns>
        public static bool IsDouble(string strDouble)
        {
            string strValue = @"^\d+(\.\d+)?$";//非負浮點數
            if (strDouble == null)
            { return false; }
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(strValue);
            return r.IsMatch(strDouble);
        }

        /// <summary>
        /// 轉換換行符號
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string ChgBreakLine(String msg)
        {
            string Message = "";

            if (!string.IsNullOrEmpty(msg))
            {
                Message = Convert.ToString(msg).Replace(System.Environment.NewLine, "<br>");
            }

            return Message;
        }


        /// <summary>
        /// 計自傳入的出生日期, 當前(系統日)的年齡
        /// </summary>
        /// <param name="birthDate"></param>
        /// <returns></returns>
        public static int CalculateAge(DateTime birthDate)
        {
            DateTime now = DateTime.Today;
            int age = now.Year - birthDate.Year;

            if (now.Month < birthDate.Month
                || (now.Month == birthDate.Month && now.Day < birthDate.Day))
                age--;

            return age;
        }

        /// <summary>
        /// 將阿拉伯數字金額, 轉成中文大寫金額字串,
        /// 例: 1100 ==> 壹仟壹佰元整,
        /// (TODO: 大於1萬的數值轉換, 未經完整驗測)
        /// </summary>
        /// <param name="number"></param>
        /// <param name="zhLower">國字大小寫, true.大寫(壹,貳,參,...), false.小寫(一,二,三,...)</param>
        /// <returns></returns>
        public static string NumberFormatZH(ulong number, bool zhLower = false)
        {
            return NumberFormatZH(number, "", zhLower);
        }

        /// <summary>
        /// 將阿拉伯數字金額, 轉成中文大寫金額字串,
        /// 例: 1100 ==> 壹仟壹佰元整,
        /// (TODO: 大於1萬的數值轉換, 未經完整驗測)
        /// </summary>
        /// <param name="number"></param>
        /// <param name="lastDW">最後金額單位</param>
        /// <param name="zhLower">國字大小寫, true.大寫(壹,貳,參,...), false.小寫(一,二,三,...)</param>
        /// <returns></returns>
        private static string NumberFormatZH(ulong number, string lastDW, bool zhLower = false)
        {
            // 中文金額單位
            string[] NTD = { "零", "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖" };
            string[] NTDL = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] DW = { "萬", "仟", "佰", "拾", "" };
            string[] DWL = { "萬", "千", "百", "十", "" };

            ulong BILLION = 100000000;
            ulong TEN_THOUNDS = 10000;

            if (number >= BILLION)
            {
                // >= 1億
                return NumberFormatZH(number / BILLION, "億") + NumberFormatZH(number % BILLION, lastDW);
            }
            if (number >= TEN_THOUNDS)
            {
                // >= 1萬
                return NumberFormatZH(number / TEN_THOUNDS, "萬") + NumberFormatZH(number % TEN_THOUNDS, lastDW);
            }

            if (zhLower)
            {
                NTD = NTDL;
                DW = DWL;
            }

            // < 1萬
            bool has0 = false;
            //ulong idx = TEN_THOUNDS;
            string strNumber = number.ToString("D5");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < DW.Length; ++i)
            {
                // 目前位數值
                string digit = strNumber.Substring(i, 1);
                int n = int.Parse(digit);
                if (n == 0)
                {
                    if (sb.Length > 0 && !has0)
                    {
                        // 補 "零"
                        sb.Append(NTD[n]);
                        has0 = true;
                    }
                }
                else
                {
                    sb.Append(NTD[n]);
                    sb.Append(DW[i]);
                }
                /*
                if (number < 10)
                {
                    number = 0;
                }
                else
                {
                    number = number % idx;
                }
                idx = idx / 10;
                */
            }
            string final = "";
            if (sb.Length > 0)
                final = sb.ToString(sb.Length - 1, 1) == "零" ? sb.ToString(0, sb.Length - 1) : sb.ToString();
            else if (sb.Length == 1)
                final = sb.ToString();
            else
                final = "零";
            return final + lastDW;
        }

        // =============================================================================
        // Trim
        // =============================================================================
        /// <summary>
        /// trim 字串 (避免傳入值為null)
        /// </summary>
        /// <param name="s">
        /// @return
        /// </param>
        public static string SafeTrim(object s)
        {
            return SafeTrim(s, "");
        }

        /// <summary>
        /// trim 字串 (避免傳入值為null)
        /// </summary>
        /// <param name="s"> 傳入字 </param>
        /// <param name="defaultStr">
        /// 預設字串
        /// @return
        /// </param>
        public static string SafeTrim(object s, string defaultStr)
        {
            if (s == null || IsEmpty(s))
            {
                return defaultStr;
            }
            return s.ToString().Trim();
        }

        // =============================================================================
        // is 判斷
        // =============================================================================
        /// <summary>
        /// 檢核字串是否為空
        /// </summary>
        /// <param name="object"> 傳入物件 </param>
        /// <returns> true or false </returns>
        public static bool IsEmpty(object @object)
        {
            if (@object == null)
            {
                return true;
            }
            if (!"".Equals(@object.ToString().Trim()))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 檢核身份證號格式
        /// </summary>
        /// <param name="arg_Identify"></param>
        /// <returns></returns>
        public static bool IsIDNO(object arg)
        {
            var arg_Identify = SafeTrim(arg);
            var d = false;
            if (arg_Identify.Length == 10)
            {
                arg_Identify = arg_Identify.ToUpper();
                if (arg_Identify[0] >= 0x41 && arg_Identify[0] <= 0x5A)
                {
                    var a = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33 };
                    var b = new int[11];
                    b[1] = a[(arg_Identify[0]) - 65] % 10;
                    var c = b[0] = a[(arg_Identify[0]) - 65] / 10;
                    for (var i = 1; i <= 9; i++)
                    {
                        b[i + 1] = arg_Identify[i] - 48;
                        c += b[i] * (10 - i);
                    }
                    if (((c % 10) + b[10]) % 10 == 0)
                    {
                        d = true;
                    }
                }
            }
            return d;
        }

        /// <summary>
        /// 檢核email格式
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(string email)
        {
            return Regex.IsMatch(email, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", RegexOptions.IgnoreCase);
        }
    }
}