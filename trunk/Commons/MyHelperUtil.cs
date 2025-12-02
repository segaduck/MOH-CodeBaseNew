using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.DataLayers;
using Turbo.DataLayer;
using System.Collections;
using System.Text;
using Turbo.Commons;

namespace EECOnline.Commons
{
    /// <summary>
    /// 放置 EECOnline 專用的 HelperUtil 擴充,
    /// 只有當 Turbo.Commons.HelperUtil 不足時才寫新的
    /// </summary>
    public class MyHelperUtil: Turbo.Commons.HelperUtil
    {
        #region 取得在 ModelState 內所有的錯誤訊息相關方法
        /// <summary>
        /// 取得在 ModelState 內所有的錯誤訊息。
        /// </summary>
        /// <param name="modelState">ModelState 物件</param>
        /// <returns>錯誤訊息集合物件。</returns>
        public static IList<string> GetModelStateErrors(ModelStateDictionary modelState)
        {
            var list = new List<string>();
            if (modelState != null)
            {
                string msg = null;
                foreach(ModelState n in modelState.Values)
                {
                    msg = string.Join("; ", n.Errors.Select(x => x.ErrorMessage));
                    list.Add(msg);
                }
            }
            return list;
        }

        /// <summary>
        /// 取得在 ModelState 內所有的錯誤訊息。
        /// </summary>
        /// <param name="modelState">ModelState 物件</param>
        /// <param name="delimeter">（非必要）每個錯誤訊息之間的分隔字元。範例："\r\n"、"&lt;br&gt;"</param>
        /// <returns>錯誤訊息字串。</returns>
        public static string GetModelStateErrors(ModelStateDictionary modelState, string delimeter)
        {
            string msg = "";
            if (modelState != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (ModelState n in modelState.Values)
                {
                    sb.Append(string.Join("; ", n.Errors.Select(x => x.ErrorMessage)));
                    if (n.Errors.Count > 0) sb.Append(delimeter);
                }
                if (sb.Length > 0)
                {
                    int deliLen = (delimeter == null) ? 0 : delimeter.Length;
                    msg = sb.ToString();
                    msg = msg.Substring(0, msg.Length - deliLen);
                }
            }
            return msg;
        }
        #endregion

        /// <summary>
        /// 安全的 string.Trim() 包裝, 自動判斷 string 是否為 null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Trim(string str)
        {
            if(str != null)
            {
                return str.Trim();
            }
            else
            {
                return null;
            }
        }

        // =============================================================================
        // is 判斷
        // =============================================================================
        /// <summary>
        ///     檢核字串是否為空
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
        /// 檢核字串是否為空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsEmpty(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return true;
            }
            if (!"".Equals(str.Trim()))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     檢核List是否為空
        /// </summary>
        /// <param name="list"> 傳入物件 </param>
        /// <returns> true or false </returns>
        public static bool IsEmpty(IList list)
        {
            if (list.Count == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///  檢核是否為統一編號
        /// </summary>
        /// <param name="strCardno"></param>
        /// <returns></returns>
        public static bool IsUniformNo(object arg)
        {
            var strCardno = SafeTrim(arg);

            if (strCardno.Trim().Length < 8)
            {
                return false;
            }
            else
            {
                int[] intTmpVal = new int[6];
                int intTmpSum = 0;
                for (int i = 0; i < 6; i++)
                {
                    //位置在奇數位置的*2，偶數位置*1，位置計算從0開始
                    if (i % 2 == 1)
                        intTmpVal[i] = overTen(int.Parse(strCardno[i].ToString()) * 2);
                    else
                        intTmpVal[i] = overTen(int.Parse(strCardno[i].ToString()));

                    intTmpSum += intTmpVal[i];
                }
                intTmpSum += overTen(int.Parse(strCardno[6].ToString()) * 4); //第6碼*4
                intTmpSum += overTen(int.Parse(strCardno[7].ToString())); //第7碼*1

                if (intTmpSum % 10 != 0) //除以10後餘0表示正確，反之則錯誤
                    return false;
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="intVal"></param>
        /// <returns></returns>
        private static int overTen(int intVal) //超過10則十位數與個位數相加，直到相加後小於10
        {
            if (intVal >= 10)
                intVal = overTen((intVal / 10) + (intVal % 10));
            return intVal;
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

        // =============================================================================
        // Trim
        // =============================================================================
        /// <summary>
        ///     trim 字串 (避免傳入值為null)
        /// </summary>
        /// <param name="s">
        ///     @return
        /// </param>
        public static string SafeTrim(object s)
        {
            return SafeTrim(s, "");
        }

        /// <summary>
        ///     trim 字串 (避免傳入值為null)
        /// </summary>
        /// <param name="s"> 傳入字 </param>
        /// <param name="defaultStr">
        ///     預設字串
        ///     @return
        /// </param>
        public static string SafeTrim(object s, string defaultStr)
        {
            if (s == null || IsEmpty(s))
            {
                return defaultStr;
            }
            return s.ToString().Trim();
        }

        /// <summary>
        /// 半型數字轉國字大寫數字, 
        /// 12345 => 一二三四五 OR  壹貳參肆伍 OR １２３４５ OR 12345
        /// <para>轉換種類(type)如下:</para>
        /// <para>ctype = "S" 轉簡單國字 Ex:一二三四五</para>
        /// <para>ctype = "C" 轉複雜國字 Ex:壹貳參肆伍</para>
        /// <para>ctype = "F" 轉全形數字 Ex:１２３４５</para>
        /// <para>ctype = 其他 不轉換, 為原來的半形數字(適用於橫式的報表列印)</para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ctype"></param>
        /// <returns></returns>
        public static string Utl_ChineseValue(string value, string ctype)
        {
            string[] TypeS = { "○", "一", "二", "三", "四", "五", "六", "七", "八", "九"};
            string[] TypeC = { "零", "壹", "貳", "參", "肆", "伍", "陸", "柒", "捌", "玖" };
            string[] TypeF = { "０", "１", "２", "３", "４", "５", "６", "７", "８", "９" };

            if (string.IsNullOrEmpty(value)) {
                return "";
            }

            StringBuilder sb = new StringBuilder();

            foreach(byte ch in Encoding.ASCII.GetBytes(value))
            {
                // ch 是 value 字串每一個字元的 ASCII 碼
                // ch - 48(Ascii of '0') = array index
                int idx = ch - 48;

                if ("S".Equals(ctype))
                {
                    sb.Append(TypeS[idx]);
                }
                else if ("C".Equals(ctype))
                {
                    sb.Append(TypeC[idx]);
                }
                else if ("F".Equals(ctype))
                {
                    sb.Append(TypeF[idx]);
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 將民國年月日(YYYMMDD)字串, 以 __年__月__日 的格式返回
        /// </summary>
        /// <param name="strTwDate"></param>
        /// <returns></returns>
        public static string FormatZHDate(string strTwDate)
        {
            if(string.IsNullOrEmpty(strTwDate) || strTwDate.Length != 7)
            {
                return "";
            }

            return string.Format("{0}年{1}月{2}日",
                strTwDate.Substring(0, 3),
                strTwDate.Substring(3, 2),
                strTwDate.Substring(5, 2)
                );
        }
    
        /// <summary>將民國年日期字串（格式：YYY/MM/DD 或 YYYMMDD）轉換成西元日期字串。</summary>
        /// <param name="twDate">民國年日期字串（格式：YYY/MM/DD 或 YYYMMDD）。</param>
        /// <param name="delimiter">（非必要）在日期字串內的日期年月日分隔字元。預設為 "" 字元。</param>
        /// <param name="returnAdDateIfEmpty">（非必要）當民國日期字串為空值時，傳回此西元日期字串。預設為 null。</param>
        /// <returns></returns>
        public static string TransTwDateToAdDate(string twDate, string delimiter = "", string returnAdDateIfEmpty = null)
        {
            if (string.IsNullOrEmpty(twDate)) {
                return returnAdDateIfEmpty;
            }
            else {
                return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(twDate, delimiter));
            }
        }

        /// <summary>將西元日期字串（格式：YYYY/MM/DD 或 YYYYMMDD）轉換成民國日期字串。</summary>
        /// <param name="adDate">西元日期字串（格式：YYYY/MM/DD 或 YYYYMMDD）。</param>
        /// <param name="delimiter">（非必要）在日期字串內的年月日分隔字元。預設為 "" 字元。</param>
        /// <param name="returnTwDateIfEmpty">（非必要）當西元日期字串為空值時，傳回此民國日期字串。預設為 null。</param>
        /// <returns></returns>
        public static string TransAdDateToTwDate(string adDate, string delimiter = "", string returnTwDateIfEmpty = null)
        {
            if (string.IsNullOrEmpty(adDate)) {
                return returnTwDateIfEmpty;
            }
            else {
                return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(adDate), delimiter);
            }
        }

        #region 取得當月最後一日相關方法
        /// <summary>傳回「當月最後一日」的 DateTime 型別值</summary>
        /// <param name="date">指定的年月日期</param>
        /// <returns></returns>
        public static DateTime GetLastDateOfMonth(DateTime date)
        {
            var dt = new DateTime(date.Year, date.Month, 1);
            dt = dt.AddMonths(1).AddDays(-1);
            return dt;
        }

        /// <summary>傳回「當月最後一日」的 DateTime 型別值</summary>
        /// <param name="date">指定的年月日期</param>
        /// <returns></returns>
        public static string GetLastDateOfMonthTW(DateTime date)
        {
            var dt = MyHelperUtil.GetLastDateOfMonth(date);
            var sb = new StringBuilder();
            sb.Append((dt.Year - 1911).ToString().PadLeft(3, '0'));
            sb.Append(dt.Month.ToString().PadLeft(2, '0'));
            sb.Append(dt.Day.ToString().PadLeft(2, '0'));
            return sb.ToString();
        }

        /// <summary>傳回「當月最後一日」的 DateTime 型別值</summary>
        /// <param name="year">（非必要）指定的西元年（西元年從 1 開始）。輸入 0 表示目前西元年。</param>
        /// <param name="month">（非必要）指定的月份（月份從 1 開始）。輸入 0 表示目前月份。</param>
        /// <returns></returns>
        public static DateTime GetLastDateOfMonth(int year = 0, int month = 0)
        {
            var ndt = DateTime.Now;
            int yr = (year <= 0) ? ndt.Year : year;
            int mn = (month <= 0) ? ndt.Month : month;
            var dt = new DateTime(yr, mn, 1);
            dt = dt.AddMonths(1).AddDays(-1);
            return dt;
        }

        /// <summary>傳回「當月最後一日」的民國日期字串</summary>
        /// <param name="year">（非必要）指定的西元年（西元年從 1 開始）。輸入 0 表示目前西元年。</param>
        /// <param name="month">（非必要）指定的月份（月份從 1 開始）。輸入 0 表示目前月份。</param>
        /// <returns></returns>
        public static string GetLastDateOfMonthTW(int year = 0, int month = 0)
        {
            var dt = MyHelperUtil.GetLastDateOfMonth(year, month);
            var sb = new StringBuilder();
            sb.Append((dt.Year - 1911).ToString().PadLeft(3, '0'));
            sb.Append(dt.Month.ToString().PadLeft(2, '0'));
            sb.Append(dt.Day.ToString().PadLeft(2, '0'));
            return sb.ToString();
        }
        #endregion
    }
}