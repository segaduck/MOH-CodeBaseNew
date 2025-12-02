using ES.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace ES.Services
{
    public static class CommonsServices
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region 基底擴充

        /// <summary>
        /// 設定屬性資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="target"></param>
        /// <param name="memberLamda"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue<T, TValue, TModel>(this Expression<Func<T, TValue>> memberLamda, TValue value, TModel model)
        {
            T rtn = (T)Activator.CreateInstance(typeof(T));

            var memberSelectorExpression = memberLamda.Body as MemberExpression;

            if (memberSelectorExpression != null)
            {
                var modelproperty = memberSelectorExpression.Expression as MemberExpression;
                foreach (PropertyInfo pi in rtn.GetType().GetProperties())
                {
                    if (pi.Name == modelproperty.Member.Name)
                    {
                        var property = memberSelectorExpression.Member as PropertyInfo;
                        foreach (PropertyInfo pj in model.GetType().GetProperties())
                        {
                            if (property != null)
                                if (property.Name == pj.Name)
                                    pj.SetValue(model, value);
                        }
                        pi.SetValue(rtn, model);
                    }
                }
            }
        }

        /// <summary>
        /// 正規畫-保留篩選文字
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string splitHTML(this object html, string SaveReg)
        {
            string split = Regex.Replace(html.TONotNullString(), SaveReg, "");
            return split;
        }

        /// <summary>
        /// 取得地址後半部(前半部)
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static string getAddrBack(this string addr)
        {
            string BlackAddress = "";
            if (!string.IsNullOrEmpty(addr))
            {
                string RealType = "";
                char[] AdrType = { '區', '鄉', '鎮', '市' };
                IList<string> Adr_list = new List<string>();
                foreach (var AdrItem in AdrType)
                {
                    if (addr.Contains(AdrItem))
                    {
                        Adr_list = addr.ToSplit(AdrItem);
                        RealType = AdrItem.TONotNullString();
                        break;
                    }
                }
                if (Adr_list.ToCount() > 0)
                {
                    BlackAddress = Adr_list[0];
                    if (Adr_list.ToCount() > 1)
                    {
                        BlackAddress = Adr_list[1];
                        // 在後置地址又有'區', '鄉', '鎮', '市'等文字
                        if (Adr_list.ToCount() > 2)
                        {
                            for (int j = 1; j < Adr_list.ToCount(); j++)
                            {
                                BlackAddress += Adr_list[j];
                            }
                        }
                    }
                }
                else
                {
                    BlackAddress = addr;
                }
            }

            return BlackAddress;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string SubstringTo(this string str, int start)
        {
            // 字串空的回傳空值
            if (string.IsNullOrEmpty(str)) return "";
            // 字串長度未達起始位置
            if (str.Length < start) return str;

            return str.Substring(start);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separator"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string SubstringTo(this string str, int start, int end)
        {
            // 字串空的回傳空值
            if (string.IsNullOrEmpty(str)) return "";
            // 字串長度未達起始位置
            if (str.Length < start) return str;
            // 字串起始位置到結束位置長度超過字串長度
            if (str.Substring(start).Length < end) return str;

            return str.Substring(start, end);
        }

        /// <summary>
        /// 將民國時間轉換為民國證照模式(民國 yyy 年 MM 月 dd 日)
        /// </summary>
        public static string TransDateTime(this string TwDate)
        {
            if (TwDate == null)
            {
                return "";
            }
            var Year = (TwDate.SubstringTo(0, 4).TOInt32() - 1911) + "年";
            var Month = TwDate.SubstringTo(4, 2).TONotNullString() + "月";
            var Day = TwDate.SubstringTo(6, 2).TONotNullString() + "日";
            var Hour = TwDate.SubstringTo(8, 2).TONotNullString() + "點";
            var Minu = TwDate.SubstringTo(10, 2).TONotNullString() + "分";
            var Sec = TwDate.SubstringTo(12, 2).TONotNullString() + "秒";
            return Year + Month + Day + Hour + Minu + Sec;
        }

        /// <summary>
        /// 將民國時間轉換為民國證照模式(民國 yyy 年 MM 月 dd 日)
        /// </summary>
        public static string TransDateTimeTw(this DateTime TwDate)
        {
            if (TwDate == null)
            {
                return "";
            }
            return TwDate.AddYears(-1911).ToString("民國 yyy 年 MM 月 dd 日", new System.Globalization.CultureInfo("zh-TW"));
        }



        /// <summary>
        /// 將民國時間轉換為西元證照模式(MM dd,yyyy)
        /// </summary>
        public static string TransDateTime(this DateTime TwDate)
        {
            if (TwDate == null)
            {
                return "";
            }
            return TwDate.ToString("MMMM dd, yyyy", new System.Globalization.CultureInfo("en-US"));
        }

        /// <summary>
        /// 將民國時間轉換為民國模式(yyy/MM/dd/)
        /// S:年月日
        /// </summary>
        public static string TransDateTimeTw(this string TwDate, string type = "")
        {
            if (TwDate == null)
            {
                return "";
            }
            if (type == "S")
            {
                return TwDate.SubstringTo(0, 3) + "年" + TwDate.SubstringTo(3, 2) + "月" + TwDate.SubstringTo(5, 2) + "日";
            }
            return TwDate.SubstringTo(0, 3) + "/" + TwDate.SubstringTo(3, 2) + "/" + TwDate.SubstringTo(5, 2);
        }

        /// <summary>
        /// Split進階版(可以將文字分割為IList)
        /// 若傳入NULL則傳回0的IList
        /// 若該字串沒有分割符號，則回傳該字串單筆IList
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static IList<string> ToSplit(this string str, char sp)
        {
            IList<string> splitlist = new List<string>();

            if (!string.IsNullOrEmpty(str))
            {
                if (str.IndexOf(sp) > -1)
                {
                    string[] strsplit = str.Split(sp);
                    for (int i = 0; i < strsplit.Count(); i++)
                    {
                        splitlist.Add(strsplit[i]);
                    }
                }
                else
                {
                    splitlist.Add(str);
                }
            }

            return splitlist;
        }

        public static IList<string> ToSplit(this string str, string sp)
        {
            IList<string> splitlist = new List<string>();

            if (string.IsNullOrEmpty(str)) { return splitlist; }

            if (str.IndexOf(sp) == -1)
            {
                splitlist.Add(str);
                return splitlist;
            }

            string[] strsplit = str.Split(new string[] { sp }, StringSplitOptions.None);
            //string[] strsplit = str.Split(sp);
            for (int i = 0; i < strsplit.Count(); i++)
            {
                splitlist.Add(strsplit[i]);
            }
            return splitlist;
        }

        /* eric,
         * 不可以用這樣的方式,
         * 分母回傳1會造成原本的除式結果邏輯上的錯誤
         *
        /// <summary>
        /// 數字-除法分母用(分母不可為0)
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double ToDivInt(this object num)
        {
            if (Convert.ToDouble(num) != 0)
            {
                return Convert.ToDouble(num);
            }
            return 1;
        }
        */

        /// <summary>
        /// 轉成浮點數
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static double ToDouble(this object num)
        {
            if (Convert.ToDouble(num) != 0)
            {
                return Convert.ToDouble(num);
            }
            return 0;
        }

        /// <summary>
        /// 轉成浮點數
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static float ToSingle(this object num)
        {
            if (Convert.ToSingle(num) != 0)
            {
                return Convert.ToSingle(num);
            }
            return 0;
        }

        /// <summary>
        /// 字串-傳回千分位字串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TOTranThousandString(this object str)
        {
            string str1 = str.TONotNullString();
            str1 = (str1 == "" || str1 == null) ? "0" : str1;
            return Convert.ToInt64(str1).ToString("#,0");
        }

        /// <summary>
        /// 是否符合規則
        /// </summary>
        /// <param name="CString"></param>
        /// <returns></returns>
        public static bool IsMatch(string _value, string RegularExpressions)
        {
            Match m = Regex.Match(_value, RegularExpressions);

            if (m.Success)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 檢測是否為中文
        /// </summary>
        /// <param name="CString"></param>
        /// <returns></returns>
        public static bool IsSpecial(string _value)
        {
            var RegularExpressions = "(?=.*[@#$%^&+=.*?])";

            Match m = Regex.Match(_value, RegularExpressions);

            if (m.Success)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 字串-字串NULL回傳""
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TONotNullString(this object str)
        {
            return Convert.ToString(str) == "" ? "" : Convert.ToString(str);
        }

        /// <summary>
        /// 字串-字串空白" "轉為""
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SpaceTranNullString(this object str)
        {
            return Convert.ToString(str) == " " ? "" : Convert.ToString(str);
        }

        /// <summary>
        /// 字串-字串NULL回傳"-"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TONotDashString(this object str)
        {
            return string.IsNullOrEmpty(Convert.ToString(str)) ? "--" : Convert.ToString(str);
        }

        /// <summary>
        /// 字串-字串NULL回傳" "(空白字串)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TONotSpaceString(this object str)
        {
            return Convert.ToString(str) == "" ? " " : "";
        }

        /// <summary>
        /// 字串-字串回傳數字int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int TOInt32(this object str)
        {
            int tryP = 0;
            if (string.IsNullOrEmpty(Convert.ToString(str)) || !int.TryParse(str.TONotNullString(), out tryP))
            {
                return 0;
            }
            return Convert.ToInt32(str);
        }

        /// <summary>
        /// 字串-字串回傳數字long
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long TOInt64(this object str)
        {
            long tryP = 0;
            if (string.IsNullOrEmpty(Convert.ToString(str)) || !long.TryParse(str.TONotNullString(), out tryP))
            {
                return 0;
            }
            return Convert.ToInt64(str);
        }

        /// <summary>
        /// 字串-字串回傳數字long
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long DoubleTOInt64(this double str)
        {
            if (string.IsNullOrEmpty(Convert.ToString(str)))
            {
                return 0;
            }
            return Convert.ToInt64(str);
        }

        /// <summary>
        /// 從右邊取得對應字數
        /// </summary>
        /// <returns></returns>
        public static string ToRight(this string str, int n)
        {
            return str.SubstringTo(str.Length - n, n);
        }

        /// <summary>
        /// 從左邊取得對應字數
        /// </summary>
        /// <returns></returns>
        public static string ToLeft(this string str, int n)
        {
            return str.SubstringTo(0, n);
        }

        /// <summary>
        /// Count進階版
        /// 若傳入NULL則傳回0
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static int ToCount<T>(this ICollection<T> list)
        {
            if (list == null) { return 0; }
            return list.Count;
        }

        /// <summary>
        /// Count進階版
        /// 若傳入NULL則傳回0
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static int ToCount<T>(this IEnumerable<T> list)
        {
            if (list == null)
            {
                return 0;
            }
            return list.Count();
        }

        /// <summary>
        /// 進階Trim，能將字串所有空白都削掉
        /// S:年月日
        /// </summary>
        public static string ToTrim(this string str)
        {
            if (str == null)
            {
                return "";
            }

            return str.Replace(" ", "");
        }

        /// <summary>
        /// Contains進階版
        /// 直接針對傳進的List判斷兩個List是否有相同元素
        /// 若有傳回True，反之False
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static bool ToContains(this ICollection<string> list, ICollection<string> list2)
        {
            foreach (var item in list)
            {
                if (list2.Contains(item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 陸生報名-100年以上陸生報名應該都是100年以上
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ROC2W(this string str)
        {
            if (str.Length == 7)
            {
                return (str.SubstringTo(0, 3).TOInt32() + 1911) + "-" + str.SubstringTo(3, 2) + "-" + str.SubstringTo(5, 2);
            }
            else
            {
                return (str.SubstringTo(0, 2).TOInt32() + 1911) + "-" + str.SubstringTo(2, 2) + "-" + str.SubstringTo(4, 2);
            }
        }

        /// <summary>
        ///  判斷是否為英文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNatural_Eng(this string str)
        {
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[A-Za-z]+$");

            return reg1.IsMatch(str);
        }

        /// <summary>
        ///  判斷是否為數字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNatural_Num(this string str)
        {
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");

            return reg1.IsMatch(str);
        }

        #endregion 基底擴充

        #region HTML 標籤

        /// <summary>
        /// 取得Model物件在HTML的ID
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetpropertyId<TModel>(HtmlHelper<TModel> htmlHelper,
         Expression<Func<TModel, string>> expression)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            return propertyId;
        }

        /// <summary>
        /// 取得Model物件在HTML的NAME
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetpropertyName<TModel>(HtmlHelper<TModel> htmlHelper,
         Expression<Func<TModel, string>> expression)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            return propertyName;
        }

        /// <summary>
        /// 取得Model物件在HTML的VALUE
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetpropertyValue<TModel>(HtmlHelper<TModel> htmlHelper,
         Expression<Func<TModel, string>> expression)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            return value;
        }

        #endregion HTML 標籤

        #region 共用函數

        /// <summary>
        /// 取得連續數字的分組集合
        /// </summary>
        /// <param name="NUMBERlist"></param>
        /// <returns></returns>
        public static List<List<int>> GetContinuousNUMBER(List<int> NUMBERlist)
        {
            NUMBERlist = NUMBERlist.OrderBy(m => m).ToList();
            var ConNUMBERGroup = new List<List<int>>();
            var ConNUMBERlist = new List<int>();
            for (var i = 0; i < NUMBERlist.ToCount(); i++)
            {
                var item = NUMBERlist[i];
                var dif = 0;
                if (i == 0) dif = 1;
                else dif = (item - NUMBERlist[i - 1]);

                if (dif == 1)
                {
                    ConNUMBERlist.Add(item);
                }
                else
                {
                    var Temp = new List<int>();
                    Temp = ConNUMBERlist;
                    ConNUMBERGroup.Add(Temp);
                    ConNUMBERlist = new List<int>();
                }
                // 最後一筆直接增加
                if (i + 1 == NUMBERlist.ToCount())
                {
                    if (ConNUMBERlist.ToCount() == 0) ConNUMBERlist.Add(item);
                    var Temp = new List<int>();
                    Temp = ConNUMBERlist;
                    ConNUMBERGroup.Add(Temp);
                }
            }

            return ConNUMBERGroup;
        }

        /// <summary>
        /// 取得現在民國時間到秒數(yyymmddhhmmss)
        /// </summary>
        /// <param name="time"></param>
        /// <param name="sp">true代表yyymmdd,false代表yyy/mm/dd</param>
        /// <returns></returns>
        public static string ToTwNowTime(this DateTime time)
        {
            return (time.Year - 1911)
                + time.Month.TONotNullString().PadLeft(2, '0')
                + time.Day.TONotNullString().PadLeft(2, '0')
                + time.Hour.TONotNullString().PadLeft(2, '0')
                + time.Minute.TONotNullString().PadLeft(2, '0')
                + time.Second.TONotNullString().PadLeft(2, '0');
        }

        /// <summary>
        /// 取得現在民國時間到日期(民國yyy年MM月dd日)
        /// </summary>
        /// <param name="time"></param>
        /// <param name="sp">true代表yyymmdd,false代表yyy/mm/dd</param>
        /// <returns></returns>
        public static string ToTwNowDate(this DateTime time)
        {
            var dt = $"民國{(time.Year - 1911)}年{time.Month.TONotNullString().PadLeft(2, '0')}月{time.Day.TONotNullString().PadLeft(2, '0')}日";
            return dt;
        }

        ///<summary>
        ///ASCII字串全形轉半形
        ///</summary>
        ///<paramname="input">全形字元串</param>
        ///<returns>半形字元串</returns>
        public static string ToNarrow(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                //全形空格的 UNICODE 數值為12288，半形空格為32
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                //其他字元半形(33-126)與全形(65281-65374)的對應關係是：均相差65248
                if (c[i] > 65280 && c[i] < 65375)
                {
                    c[i] = (char)(c[i] - 65248);
                }
            }
            return new string(c);
        }

        /// <summary>
        /// 將HTML標籤取代並替換掉，例:段落P AAA /段落p 轉換成AAA
        /// ，但HTML的空格符號需自行用REPLACE取代掉
        /// </summary>
        /// <param name="html">含html的文字編碼</param>
        /// <returns></returns>
        public static string HtmlStrippedText(string html)
        {
            html = html.Replace("(<style)+[^<>]*>[^\0]*(</style>)+", "");
            html = html.Replace(@"\<img[^\>] \>", "");
            html = html.Replace(@"<p>", "\r\n");
            html = html.Replace(@"</p>", "");
            System.Text.RegularExpressions.Regex regex0 = new System.Text.RegularExpressions.Regex("(<style)+[^<>]*>[^\0]*(</style>)+", System.Text.RegularExpressions.RegexOptions.Multiline);
            System.Text.RegularExpressions.Regex regex1 = new System.Text.RegularExpressions.Regex(@"<script[\s\S] </script *>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex2 = new System.Text.RegularExpressions.Regex(@" href *= *[\s\S]*script *:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex3 = new System.Text.RegularExpressions.Regex(@" on[\s\S]*=", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex4 = new System.Text.RegularExpressions.Regex(@"<iframe[\s\S] </iframe *>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex5 = new System.Text.RegularExpressions.Regex(@"<frameset[\s\S] </frameset *>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex6 = new System.Text.RegularExpressions.Regex(@"\<img[^\>] \>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex7 = new System.Text.RegularExpressions.Regex(@"</p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex8 = new System.Text.RegularExpressions.Regex(@"<p>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Regex regex9 = new System.Text.RegularExpressions.Regex(@"<[^>]*>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = regex1.Replace(html, ""); //過濾<script></script>標記
            html = regex2.Replace(html, ""); //過濾href=javascript: (<A>) 屬性
            html = regex0.Replace(html, ""); //過濾href=javascript: (<A>) 屬性           //html = regex10.Replace(html, "");
            html = regex3.Replace(html, "");// _disibledevent="); //過濾其他操控on...事件
            html = regex4.Replace(html, ""); //過濾iframe
            html = regex5.Replace(html, ""); //過濾frameset
            html = regex6.Replace(html, ""); //過濾frameset
            html = regex7.Replace(html, ""); //過濾frameset
            html = regex8.Replace(html, ""); //過濾frameset
            html = regex9.Replace(html, "");        //html = html.Replace(" ", "");
            html = html.Replace("</strong>", "");
            html = html.Replace("<strong>", "");
            html = html.Replace(" ", "");
            return html;
        }

        #endregion 共用函數

        #region 共用檢查

        /// <summary>
        /// 檢查電話是否合法
        /// </summary>
        /// <returns></returns>
        public static bool ToCheckPhone(this string str)
        {
            str = str.Replace("-", "0");
            str = str.Replace("#", "0");
            long Num = 0;
            return long.TryParse(str, out Num);
        }

        /// <summary>
        /// 電子郵件信箱位址是否合法。
        /// </summary>
        /// <param name="email">電子郵件信箱位址。</param>
        /// <returns>True表示合法，False表示不合法。</returns>
        public static bool CheckEMail(string email)
        {
            /*照1.0版本邏輯(clsUtility.vb內sChk_EMail函數)*/
            if (string.IsNullOrEmpty(email)) return false;
            else
            {
                string[] arrStr1 = email.Split('@');
                string[] arrStr2 = email.Split('.');
                if (arrStr1.Length < 2 || string.IsNullOrEmpty(arrStr1[0].Trim())) return false;
                if (arrStr2.Length < 1) return false;
                if (arrStr2.Length == 2 && string.IsNullOrEmpty(arrStr2[1].Trim())) return false;
                return true;
            }
        }

        #endregion 共用檢查

        #region 電子郵件傳送

        /// <summary>將電子郵件「寄件者」設定為系統預設值</summary>
        /// <param name="message">電子郵件訊息</param>
        public static void SetDefaultMailSender(MailMessage message)
        {
            message.From = new MailAddress(ConfigModel.MailSenderAddr);
        }

        /// <summary>在電子郵件內加入附件檔案</summary>
        /// <param name="message">電子郵件</param>
        /// <param name="attachmentPath">附件檔案實體路徑。輸入範例： "C:\Temp\AAA.xls"</param>
        private static void AddAttachment(MailMessage message, string attachmentPath)
        {
            if (!string.IsNullOrEmpty(attachmentPath))
            {
                var data = new Attachment(attachmentPath, MediaTypeNames.Application.Octet);
                var disposition = data.ContentDisposition;
                message.Attachments.Add(data);
            }
        }

        /// <summary>在電子郵件內加入附件檔案</summary>
        /// <param name="message">電子郵件</param>
        /// <param name="attachmentPath">附件檔案實體路徑。輸入範例： "C:\Temp\AAA.xls", "C:\Temp\BBB.pdf"</param>
        private static void AddAttachment(MailMessage message, params string[] attachmentPath)
        {
            if (attachmentPath != null && attachmentPath.Length > 0)
            {
                foreach (var path in attachmentPath)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        var data = new Attachment(path, MediaTypeNames.Application.Octet);
                        var disposition = data.ContentDisposition;
                        message.Attachments.Add(data);
                    }
                }
            }
        }

        /// <summary>建立新的電子郵件</summary>
        /// <param name="from">寄件者電子郵件信箱位址</param>
        /// <param name="to"> 收件者電子郵件信箱位址</param>
        /// <param name="subject">郵件主旨</param>
        /// <param name="body">郵件內容</param>
        /// <param name="attachmentPaths">附件檔案實體路徑。輸入範例： new string[] { "C:\Temp\AAA.xls", "C:\Temp\BBB.pdf" }。輸入 null 表示沒有附件檔案。</param>
        /// <param name="isBodyHtml">郵件內容是否為 HTML 格式。（true: HTML 格式，false: 純文字）</param>
        /// <returns></returns>
        public static MailMessage NewMail(string from, string to, string subject, string body,
                                          string[] attachmentPaths = null, bool isBodyHtml = false)
        {
            if (string.IsNullOrEmpty(from)) from = ConfigModel.MailSenderAddr;
            if (string.IsNullOrEmpty(from)) throw new ArgumentNullException("寄件者電子郵件位址不可為空。");
            if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("收件者電子郵件位址不可為空。");
            if (!string.IsNullOrEmpty(to) && !to.Contains('@'))
            {
                string s_err1 = string.Format("收件者電子郵件有誤。{0}", to);
                throw new ArgumentNullException(s_err1);
            }
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException("郵件主旨不可為空。");

            MailMessage message = null;
            try
            {
                message = new MailMessage(from, to, subject, body);
            }
            catch (Exception ex)
            {
                string s_err = ex.Message;
                s_err += string.Format("\nfrom: {0}", from);
                s_err += string.Format("\nto: {0}", to);
                s_err += string.Format("\nsubject: {0}", subject);
                s_err += string.Format("\nbody: {0}", body);
                logger.Error(s_err, ex);
                throw ex;
            }
            message.IsBodyHtml = isBodyHtml;
            AddAttachment(message, attachmentPaths);
            return message;
        }

        /// <summary>建立新的電子郵件</summary>
        /// <param name="from">寄件者電子郵件信箱位址。輸入 null 表示使用系統預設值</param>
        /// <param name="to"> 收件者電子郵件信箱位址</param>
        /// <param name="subject">郵件主旨</param>
        /// <param name="body">郵件內容</param>
        /// <param name="attachmentPath">附件檔案實體路徑。輸入範例： "C:\Temp\AAA.xls"。輸入 null 表示沒有附件檔案。</param>
        /// <param name="isBodyHtml">郵件內容是否為 HTML 格式。（true: HTML 格式，false: 純文字）</param>
        /// <returns></returns>
        public static MailMessage NewMail(string from, string to, string subject, string body,
                                          string attachmentPath, bool isBodyHtml = false)
        {
            if (string.IsNullOrEmpty(from)) from = ConfigModel.MailSenderAddr;
            if (string.IsNullOrEmpty(from)) throw new ArgumentNullException("寄件者電子郵件位址不可為空。");
            if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("收件者電子郵件位址不可為空。");
            if (!string.IsNullOrEmpty(to) && !to.Contains('@'))
            {
                string s_err1 = string.Format("收件者電子郵件有誤。{0}", to);
                throw new ArgumentNullException(s_err1);
            }
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException("郵件主旨不可為空。");

            MailMessage message = null;
            try
            {
                message = new MailMessage(from, to, subject, body);
            }
            catch (Exception ex)
            {
                string s_err = ex.Message;
                s_err += string.Format("\nfrom: {0}", from);
                s_err += string.Format("\nto: {0}", to);
                s_err += string.Format("\nsubject: {0}", subject);
                s_err += string.Format("\nbody: {0}", body);
                logger.Error(s_err, ex);
                throw ex;
            }
            message.IsBodyHtml = isBodyHtml;
            AddAttachment(message, attachmentPath);
            return message;
        }

        /// <summary>執行電子郵件傳送</summary>
        /// <param name="message">要處理的電子郵件</param>
        /// <param name="credential">（非必要）電子郵件寄件者帳號與密碼。輸入 null 表示使用系統預設值。</param>
        /// <param name="mailServer">（非必要）電子郵件服務主機 IP。輸入 null 表示使用系統預設值。</param>
        /// <returns>電子郵件傳送處理結果</returns>
        public static MailSentResult SendMail(MailMessage message, NetworkCredential credential = null, string mailServer = null)
        {
            SmtpClient client = null;
            MailSentResult result = null;
            try
            {
                result = new MailSentResult(message);
                result.Start = DateTime.Now;
                if (result.TriedTimes < int.MaxValue) result.TriedTimes++;

                if (message == null) throw new ArgumentNullException("電子郵件內容不可為空。");
                if (message.From == null) throw new ArgumentNullException("寄件者電子郵件位址不可為空。");
                if (message.To == null || message.To.Count == 0) throw new ArgumentNullException("收件者電子郵件位址不可為空。");
                if (string.IsNullOrEmpty(message.Subject)) throw new ArgumentNullException("郵件主旨不可為空。");

                //取得系統郵件服務主機 IP
                if (string.IsNullOrEmpty(mailServer))
                {
                    mailServer = ConfigModel.MailServer;
                }

                //取得系統寄件者電子郵件地址與密碼
                if (credential == null)
                {
                    // ConfigModel.MailSenderAcc
                    // ConfigModel.MailSenderAddr
                    credential = new NetworkCredential(ConfigModel.MailSenderAcc, ConfigModel.MailSenderPwd);
                }

                //使用 SMTP 協定傳送電子郵件
                client = new SmtpClient(mailServer);
                client.Credentials = (client.Credentials == null) ? credential : CredentialCache.DefaultNetworkCredentials;

                string s_MailServerPort = ConfigModel.MailServerPort ?? "";
                if (!string.IsNullOrEmpty(s_MailServerPort)) { client.Port = Convert.ToInt32(s_MailServerPort); }

                string s_EnableSsl = ConfigModel.MailEnableSsl ?? "";
                if (s_EnableSsl.Equals("Y")) { client.EnableSsl = true; }

                client.Send(message);

                result.SetSuccessResult();
                return result;
            }
            catch (SmtpFailedRecipientsException ex)
            {
                logger.Warn(ex.Message, ex);
                string errText = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
                int errCode = (ex.InnerException == null) ? ex.HResult : ex.InnerException.HResult;
                result.SetFailureResult(errText, errCode.ToString());
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
                if (result == null) throw ex;
                else
                {
                    string errText = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
                    int errCode = (ex.InnerException == null) ? ex.HResult : ex.InnerException.HResult;
                    errText = (errCode == -2146233079) ? "系統郵件服務主機無法連線。" : errText;
                    result.SetFailureResult(errText, errCode.ToString());
                    return result;
                }
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
        }

        #endregion 電子郵件傳送

        #region LinePay加密

        public static string HMACSHA256_Base64(string message, string key)
        {
            key = key ?? "";
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }
        #endregion
    }
}