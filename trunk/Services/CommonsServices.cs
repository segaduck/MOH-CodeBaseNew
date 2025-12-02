using OfficeOpenXml;
using OfficeOpenXml.Style;
using Omu.ValueInjecter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using EECOnline.DataLayers;
using EECOnline.Commons;
using EECOnline.Models;
using System.Reflection;
using System.Xml;
using EECOnline.Areas.A3.Models;
using HppApi;
using System.Diagnostics;

namespace EECOnline.Services
{
    public static class CommonsServices
    {
        #region 基底擴充

        public static string GetPiValue<T>(this T Model, string piName)
        {
            var _pi = Model.GetType().GetProperties().Where(m => m.Name == piName);
            if (_pi.ToCount() > 0)
            {
                return _pi.FirstOrDefault().GetValue(Model).TONotNullString();
            }
            return null;
        }

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
            TwDate = TwDate.Replace("/", "");
            if (TwDate == null)
            {
                return "";
            }
            if (type == "S")
            {
                if (TwDate.Length == 6) TwDate = "0" + TwDate;
                return TwDate.SubstringTo(0, 3) + "年" + TwDate.SubstringTo(3, 2) + "月" + TwDate.SubstringTo(5, 2) + "日";
            }
            return TwDate.SubstringTo(0, 3) + "/" + TwDate.SubstringTo(3, 2) + "/" + TwDate.SubstringTo(5, 2);
        }

        /// <summary>
        /// 將西元時間字串(yyyymmdd)轉換為民國模式(yyy/MM/dd/)
        /// S:年月日
        /// </summary>
        public static string TransDateTw(this string TwDate)
        {
            TwDate = TwDate.Replace("/", "");
            if (TwDate == null)
            {
                return "";
            }
            return (TwDate.SubstringTo(0, 4).TOInt32()-1911).TONotNullString() + "/" + TwDate.SubstringTo(4, 2) + "/" + TwDate.SubstringTo(6, 2);
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
        /// 字串-傳回千分位字串(小數點)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TOTranDoubleThousandString(this object str)
        {
            string str1 = str.TONotNullString();
            str1 = (str1 == "" || str1 == null) ? "0" : str1;
            return Convert.ToDouble(str1).ToString("#,0.#");
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
        /// 字串-字串NULL回傳""
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int dateTOint(this string str)
        {
            return str.Replace("/", "").TOInt32();
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
        /// 數字-傳回金額數字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="Format">true:傳回一百零六,false:傳回壹佰零陸</param>
        /// <returns></returns>
        public static string TOTwZh(this int number, bool Format = true)
        {
            return HelperUtil.NumberFormatZH(Convert.ToUInt64(number), Format);
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
            if (list == null)
            {
                return 0;
            }
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
        /// 取得現在民國時間(yyymmdd)
        /// </summary>
        /// <param name="time"></param>
        /// <param name="sp">true代表yyymmdd,false代表yyy/mm/dd</param>
        /// <returns></returns>
        public static string ToTwNow(this DateTime time, bool sp = true)
        {
            return HelperUtil.DateTimeToTwString(time, sp ? "" : "/");
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
        /// 日期自行計算
        /// </summary>
        public static string DateAdd_YMD(string mDate, string YMD, long n)
        {
            DateTime tempDate = new DateTime();
            if (!DateTime.TryParse(mDate, out tempDate))
            {
                return (mDate.TOInt32() + n).TONotNullString();
            }
            tempDate = HelperUtil.TransTwToDateTime(mDate, "").Value;
            switch (YMD)
            {
                case "Y":
                    tempDate.AddYears((int)n);
                    break;

                case "M":
                    tempDate.AddMonths((int)n);
                    break;

                case "D":
                    tempDate.AddDays(n);
                    break;
            }
            return tempDate.TONotNullString();
        }

        /*Gary Lu:下方的函數原為Cla8Service專屬，基於A7/C108T報送流程中算試科編組表報送管制日時，使用上方的函數計算會有得到錯誤結果的危險
         (上方DateAdd_YMD函數內mDate參數傳入yyyMMdd格式民國年月日會Parse失敗) Ex:1080310減10天會變成1080300不正確
         在此提出共用*/

        /// <summary>
        /// 計算出新的民國年月日。
        /// </summary>
        /// <param name="OldMinguoDate">舊的民國年月日，套用yyyMMdd格式。</param>
        /// <param name="YMD">要加減年請傳入Y,加減月請傳入M,加減日請傳入D。</param>
        /// <param name="Numbers">要加減的年數或月數或天數。</param>
        /// <returns>新的民國年月日。</returns>
        public static string CountMinguoDate(string OldMinguoDate, string YMD, int Numbers)
        {
            /*Gary:翻寫舊程式的clsUtility.Utl_DateAdd_YMD函數*/
            DateTime OldDate = HelperUtil.TransTwToDateTime(OldMinguoDate, "").Value;
            DateTime NewDate = OldDate;
            switch (YMD.ToUpper().Trim())
            {
                case "Y":
                    return HelperUtil.DateTimeToTwString(OldDate.AddYears(Numbers), "");

                case "M":
                    return HelperUtil.DateTimeToTwString(OldDate.AddMonths(Numbers), "");

                case "D":
                    return HelperUtil.DateTimeToTwString(OldDate.AddDays(Numbers), "");

                default:
                    throw new ArgumentOutOfRangeException("YMD", "不支援的加減設定");
            }
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
        /// 動態EXCEL-儲存格型態
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <returns></returns>
        public static void CellStyle(ExcelRange cell, string VALUE, int width = 30)
        {
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            cell.AutoFitColumns(width);
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <returns></returns>
        public static void CellStyle(ExcelRange cell, string VALUE)
        {
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-數字型態數值
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <returns></returns>
        public static void CellStyle(ExcelRange cell, int VALUE)
        {
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        #region (Gary Lu 問題單673調整而新增的Overload函數)

        /// <summary>
        /// 動態EXCEL-儲存格型態-數字型態數值
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <param name="Numberformat">指定的數字格式</param>
        /// <returns></returns>
        public static void CellStyle(ExcelRange cell, int VALUE, string Numberformat, ExcelBorderStyle SetBorderStyle = ExcelBorderStyle.Medium)
        {
            /*Gary Lu 20180823時處理問題單號673時加上(許多數字欄位都要加千分號之故)*/
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(SetBorderStyle, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cell.Style.Numberformat.Format = Numberformat;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M-指定邊框樣式
        /// </summary>
        /// <param name="cell">儲存格</param>
        /// <param name="VALUE">要寫入該儲存格的字串值</param>
        /// <param name="SetBorderStyle">儲存格邊框樣式</param>
        /// <returns></returns>
        public static void CellStyle(ExcelRange cell, string VALUE, ExcelBorderStyle SetBorderStyle)
        {
            /*Gary Lu 20180904問題單673調A5/C810R框線用*/
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(SetBorderStyle, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        }

        /// <summary>
        /// 動態EXCEL-合併儲存格並給值,使用指定的框線樣式,文字不自動換行
        /// </summary>
        /// <param name="excelsheet"></param>
        /// <param name="cellCol1">起</param>
        /// <param name="cellCol2">迄</param>
        /// <param name="cellRow1">起</param>
        /// <param name="cellRow2">迄</param>
        /// <param name="VALUE"></param>
        /// <param name="SetBorderStyle">指定的框線樣式</param>
        /// <param name="Wraptext">文字是否自動換行</param>
        /// <returns></returns>
        ///
        public static void CellMergeStyle(ExcelWorksheet excelsheet, int cellRow1, int cellCol1, int cellRow2, int cellCol2, string VALUE, ExcelBorderStyle SetBorderStyle, bool Wraptext = false)
        {
            /*Gary Lu 20180904問題單673調A5/C810R框線用*/
            ExcelRange range = excelsheet.Cells[cellRow1, cellCol1, cellRow2, cellCol2];
            range.Merge = true;
            range.Value = VALUE;
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            range.Style.Border.BorderAround(SetBorderStyle, System.Drawing.Color.Black);
            range.Style.WrapText = Wraptext;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M(border(True)表示要框線),依照format方式顯示數字
        /// </summary>
        /// <param name="cell">儲存格</param>
        /// <param name="Formula">要套用到儲存格內的公式</param>
        /// <param name="border">是否要框線</param>
        /// <param name="SetBorderStyle">若有需要框線時的框線樣式</param>
        /// <param name="format">儲存格數值呈現格式</param>
        /// <returns></returns>
        public static void CellFormulaStyle(ExcelRange cell, string Formula, bool border, ExcelBorderStyle SetBorderStyle, string format = null)
        {
            cell.Formula = Formula;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            if (border)
                cell.Style.Border.BorderAround(SetBorderStyle, System.Drawing.Color.Black);
            if (!string.IsNullOrEmpty(format))
                cell.Style.Numberformat.Format = format;
        }

        #endregion (Gary Lu 問題單673調整而新增的Overload函數)

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M,傳入所需的字型名稱(例:標楷體)
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <param name="FontFamilyName"></param>
        /// <returns></returns>
        public static void CellFontFamilyStyle(ExcelRange cell, string VALUE, string FontFamilyName)
        {
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Font.Name = FontFamilyName;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M,字體不加粗,自動換列,預設不帶框線,水平置左:1,中:2,右:4
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <param name="HorizontalAlignment"></param>
        /// <param name="border"></param>
        /// <returns></returns>
        public static void CellNoBoldFontStyle(ExcelRange cell, string VALUE, int HorizontalAlignment, bool border = false)
        {
            cell.Value = VALUE;
            cell.Style.Font.Size = 12;
            if (border)
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
            cell.Style.WrapText = true;
            switch (HorizontalAlignment)
            {
                case 1:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    break;

                case 2:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    break;

                case 3:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    break;

                case 4:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M,字體不加粗,自動換列,預設不帶框線,水平置左:1,中:2,右:4,依照format方式顯示數字
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <param name="HorizontalAlignment"></param>
        /// <param name="border"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static void CellNoBoldFontStyle(ExcelRange cell, int VALUE, int HorizontalAlignment, bool border = false, string format = null)
        {
            cell.Value = VALUE;
            cell.Style.Font.Size = 12;
            cell.Style.WrapText = true;
            if (border)
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
            switch (HorizontalAlignment)
            {
                case 1:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    break;

                case 2:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    break;

                case 3:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    break;

                case 4:
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    break;

                default:
                    break;
            }
            if (!string.IsNullOrEmpty(format))
            {
                cell.Style.Numberformat.Format = format;
            }
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-Double型態(依照傳入的Number決定百分比取小數幾位)
        /// 需傳入像是0.000的格式，請勿自行乘於100
        /// </summary>
        /// <param name="str"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static void CellPercentStyle(ExcelRange cell, double VALUE, int number)
        {
            if (double.IsNaN(VALUE)) VALUE = 0;
            cell.Value = VALUE;
            if (number != 0)
            {
                string formatstr = "#0.".PadRight(number + 3, '0');
                cell.Style.Numberformat.Format = formatstr + "%";
            }
            else
            {
                string formatstr = "#0";
                cell.Style.Numberformat.Format = formatstr + "%";
            }
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-Double型態(依照傳入的Number決定百分比取小數幾位)
        /// 需傳入像是0.000的格式，請勿自行乘於100, ZToZ 為True時 可顯示0.00%
        /// </summary>
        /// <param name="str"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static void CellPercentStyle(ExcelRange cell, double VALUE, int number, bool ZToZ)
        {
            if (double.IsNaN(VALUE)) VALUE = 0;
            cell.Value = VALUE;
            string formatstr = "";
            if (number > 0)
            {
                formatstr = "#0.".PadRight(number + 3, '0');
            }
            else
            {
                formatstr = "#0";
            }
            cell.Style.Numberformat.Format = formatstr + "%";
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        /// <summary>
        /// 動態EXCEL-輸出頁首頁尾文字
        /// 頁尾不輸出頁數 請輸入文字 HealderPlace或FooterPlace(0:左;1:中;2:右)
        /// IsPageNumber表示頁尾是否輸出頁碼 false時，FooterValue為必填欄位
        /// 第幾頁 寫法  "第"+ ExcelHeaderFooter.PageNumber+"頁"
        /// 第幾頁，共幾頁 寫法 "第" + ExcelHeaderFooter.PageNumber + "頁，共"+ExcelHeaderFooter.NumberOfPages+"頁"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static void XlsHeaderFooter(ExcelWorksheet excelsheet, int HeaderPlace, int FooterPlace, bool IsPageNumber = true, string healderValue = null, string FooterValue = null)
        {
            var footString = "";
            if (!IsPageNumber) footString = FooterValue;
            else footString = ExcelHeaderFooter.PageNumber;
            if (!string.IsNullOrWhiteSpace(healderValue))
            {
                switch (HeaderPlace)
                {
                    case 0:
                        excelsheet.HeaderFooter.OddHeader.LeftAlignedText = string.Format("{0}", healderValue);
                        break;

                    case 1:
                        excelsheet.HeaderFooter.OddHeader.CenteredText = string.Format("{0}", healderValue);
                        break;

                    default:
                        excelsheet.HeaderFooter.OddHeader.RightAlignedText = string.Format("{0}", healderValue);
                        break;
                }
            }
            switch (FooterPlace)
            {
                case 0:
                    excelsheet.HeaderFooter.OddFooter.LeftAlignedText = string.Format("{0}", footString);
                    break;

                case 1:
                    excelsheet.HeaderFooter.OddFooter.CenteredText = string.Format("{0}", footString);
                    break;

                default:
                    excelsheet.HeaderFooter.OddFooter.RightAlignedText = string.Format("{0}", footString);
                    break;
            }
        }

        /// <summary>
        /// 動態EXCEL-輸出頁首頁尾文字
        /// 頁尾不輸出頁數 請輸入文字 HealderPlace或FooterPlace(0:左;1:中;2:右
        /// healderValue 表示列印頁首文字
        /// footerValueStyle 表示頁尾輸出(1為 第X頁;;2 為 第X頁，共T頁;;3為單純頁碼)
        /// </summary>
        /// <param name="excelsheet"></param>
        /// <param name="HeaderPlace"></param>
        /// <param name="FooterPlace"></param>
        /// <param name="healderValue"></param>
        /// <param name="footerValueStyle"></param>
        /// <returns></returns>
        public static void XlsHeaderFooter(ExcelWorksheet excelsheet, int HeaderPlace, int FooterPlace, string healderValue = null, int? footerValueStyle = null)
        {
            var footString = "";
            switch (footerValueStyle)
            {
                case 1:
                    footString = "第" + ExcelHeaderFooter.PageNumber + "頁";
                    break;

                case 2:
                    footString = "第" + ExcelHeaderFooter.PageNumber + "頁，共" + ExcelHeaderFooter.NumberOfPages + "頁";
                    break;

                default:
                    footString = ExcelHeaderFooter.PageNumber;
                    break;
            }
            if (!string.IsNullOrWhiteSpace(healderValue))
            {
                switch (HeaderPlace)
                {
                    case 0:
                        excelsheet.HeaderFooter.OddHeader.LeftAlignedText = string.Format("{0}", healderValue);
                        break;

                    case 1:
                        excelsheet.HeaderFooter.OddHeader.CenteredText = string.Format("{0}", healderValue);
                        break;

                    default:
                        excelsheet.HeaderFooter.OddHeader.RightAlignedText = string.Format("{0}", healderValue);
                        break;
                }
            }
            if (!string.IsNullOrWhiteSpace(footString))
            {
                switch (FooterPlace)
                {
                    case 0:
                        excelsheet.HeaderFooter.OddFooter.LeftAlignedText = string.Format("{0}", footString);
                        break;

                    case 1:
                        excelsheet.HeaderFooter.OddFooter.CenteredText = string.Format("{0}", footString);
                        break;

                    default:
                        excelsheet.HeaderFooter.OddFooter.RightAlignedText = string.Format("{0}", footString);
                        break;
                }
            }
        }

        /// <summary>
        /// 動態EXCEL-合併儲存格並給值,預設不帶框線,文字不自動換行
        /// </summary>
        /// <param name="excelsheet"></param>
        /// <param name="cellCol1">起</param>
        /// <param name="cellCol2">迄</param>
        /// <param name="cellRow1">起</param>
        /// <param name="cellRow2">迄</param>
        /// <param name="VALUE"></param>
        /// <param name="Wraptext">文字是否自動換行</param>
        /// <param name="border">是否帶框線</param>
        /// <returns></returns>
        ///
        public static void CellMergeStyle(ExcelWorksheet excelsheet, int cellRow1, int cellCol1, int cellRow2, int cellCol2, string VALUE
            , bool Wraptext = false
            , bool border = false
            , bool left_border = false
            , bool right_border = false
            , bool top_border = false
            , bool bottom_border = false
            , bool font_border = false
            , bool border_thin = false
            , bool Merge = true)
        {
            ExcelRange range = excelsheet.Cells[cellRow1, cellCol1, cellRow2, cellCol2];
            range.Merge = Merge;
            range.Value = VALUE == "0" ? "-" : VALUE;
            var borderStyle = border_thin ? ExcelBorderStyle.Thin : ExcelBorderStyle.Medium;
            if (font_border)
            {
                range.Style.Font.Bold = true;
            }
            range.Style.Font.Size = 12;
            if (border)
            {
                // 四邊黑線
                range.Style.Border.BorderAround(borderStyle, System.Drawing.Color.Black);
            }
            else
            {
                // 指定 黑線
                if (left_border)
                {
                    range.Style.Border.Left.Style = borderStyle;
                    range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                }
                if (right_border)
                {
                    range.Style.Border.Right.Style = borderStyle;
                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }
                if (top_border)
                {
                    range.Style.Border.Top.Style = borderStyle;
                    range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                }
                if (bottom_border)
                {
                    range.Style.Border.Bottom.Style = borderStyle;
                    range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                }
            }
            range.Style.WrapText = Wraptext;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        /// <summary>
        /// 動態EXCEL-合併儲存格並給值,預設不帶框線
        /// </summary>
        /// <param name="excelsheet"></param>
        /// <param name="cellCol1">起</param>
        /// <param name="cellCol2">迄</param>
        /// <param name="cellRow1">起</param>
        /// <param name="cellRow2">迄</param>
        /// <param name="VALUE"></param>
        /// <param name="border">是否帶框線</param>
        /// <returns></returns>
        ///
        public static void CellMergeStyle(ExcelWorksheet excelsheet, int cellRow1, int cellCol1, int cellRow2, int cellCol2, string VALUE, bool border = false)
        {
            ExcelRange range = excelsheet.Cells[cellRow1, cellCol1, cellRow2, cellCol2];
            range.Merge = true;
            range.Value = VALUE;
            range.Style.Font.Bold = true;
            range.Style.Font.Size = 12;
            if (border)
                range.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        /// <summary>
        /// 動態EXCEL-合併儲存格並給值,文字,字型,大小,水平對齊:1左 2中 4右,垂直對齊:0上 1中 2下,是否粗體,是否框線,是否自動換行
        /// </summary>
        /// <param name="excelsheet"></param>
        /// <param name="cellCol1">起</param>
        /// <param name="cellCol2">迄</param>
        /// <param name="cellRow1">起</param>
        /// <param name="cellRow2">迄</param>
        /// <param name="VALUE"></param>
        /// <param name="FontName">字型</param>
        /// <param name="Size">文字大小</param>
        /// <param name="HorizontalAlignment">水平對齊</param>
        /// <param name="VerticalAlignment">垂直對齊</param>
        /// <param name="Bold">是否粗體字</param>
        /// <param name="border">是否帶框線</param>
        /// <param name="Wraptext">文字是否自動換行</param>
        /// <returns></returns>
        ///
        public static void CellMergeStyle(ExcelWorksheet excelsheet, int cellRow1, int cellCol1, int cellRow2, int cellCol2, string VALUE, string FontName, int Size, int HorizontalAlignment, int VerticalAlignment, bool Bold = false, bool border = false, bool Wraptext = false)
        {
            ExcelRange range = excelsheet.Cells[cellRow1, cellCol1, cellRow2, cellCol2];
            //合併
            range.Merge = true;

            //值
            range.Value = VALUE;

            //文字大小
            range.Style.Font.Size = Size;
            //字形
            range.Style.Font.Name = FontName;
            //垂直對齊
            switch (HorizontalAlignment)
            {
                case 1:
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    break;

                case 2:
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    break;

                case 3:
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    break;

                case 4:
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    break;

                default:
                    break;
            }

            //水平對齊
            switch (VerticalAlignment)
            {
                case 0:
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    break;

                case 1:
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    break;

                case 2:
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
                    break;

                default:
                    break;
            }

            //是否粗體
            range.Style.Font.Bold = Bold;

            //是否框線
            if (border)
                range.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            // range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //文字是否自動換行

            range.Style.WrapText = Wraptext;
        }

        /// <summary>
        /// 動態EXCEL-列印邊界,預設為公分(是否為英寸,上 右 下 左 頁首 頁尾)
        /// </summary>
        /// <param name="excelsheet"></param>
        /// <param name="Top"></param>
        /// <param name="Right"></param>
        /// <param name="Bottom"></param>
        /// <param name="Left"></param>
        /// <param name="Header"></param>
        /// <param name="Footer"></param>
        /// <param name="inches"></param>
        /// <returns></returns>
        public static void SheetPrintMargin(ExcelWorksheet excelsheet, bool inches = false, double? Top = null, double? Right = null, double? Bottom = null, double? Left = null, double? Header = null, double? Footer = null)
        {
            double centerMeterToInch = 0.3937007874;
            if (inches)
            {
                if (Top.HasValue)
                    excelsheet.PrinterSettings.TopMargin = Convert.ToDecimal(Top);
                if (Right.HasValue)
                    excelsheet.PrinterSettings.RightMargin = Convert.ToDecimal(Right);
                if (Bottom.HasValue)
                    excelsheet.PrinterSettings.BottomMargin = Convert.ToDecimal(Bottom);
                if (Left.HasValue)
                    excelsheet.PrinterSettings.LeftMargin = Convert.ToDecimal(Left);
                if (Header.HasValue)
                    excelsheet.PrinterSettings.HeaderMargin = Convert.ToDecimal(Header);
                if (Footer.HasValue)
                    excelsheet.PrinterSettings.FooterMargin = Convert.ToDecimal(Footer);
            }
            else
            {
                if (Top.HasValue)
                    excelsheet.PrinterSettings.TopMargin = Convert.ToDecimal(Top * centerMeterToInch);
                if (Right.HasValue)
                    excelsheet.PrinterSettings.RightMargin = Convert.ToDecimal(Right * centerMeterToInch);
                if (Bottom.HasValue)
                    excelsheet.PrinterSettings.BottomMargin = Convert.ToDecimal(Bottom * centerMeterToInch);
                if (Left.HasValue)
                    excelsheet.PrinterSettings.LeftMargin = Convert.ToDecimal(Left * centerMeterToInch);
                if (Header.HasValue)
                    excelsheet.PrinterSettings.HeaderMargin = Convert.ToDecimal(Header * centerMeterToInch);
                if (Footer.HasValue)
                    excelsheet.PrinterSettings.FooterMargin = Convert.ToDecimal(Footer * centerMeterToInch);
            }
        }

        /// <summary>
        /// 動態EXCEL-列印邊界,預設為公分(是否為英寸,上 右 下 左 頁首 頁尾 縮放 列印水平置中 列印垂直置中)
        /// scale列印縮放比例，可依需求自行縮放
        /// 列印版面設定 置中方式(printHorCenter水平// printVertiCenter垂直) TRUE 表示置中,預設FALSE
        /// </summary>
        /// <param name="excelsheet"></param>
        /// <param name="Top"></param>
        /// <param name="Right"></param>
        /// <param name="Bottom"></param>
        /// <param name="Left"></param>
        /// <param name="Header"></param>
        /// <param name="Footer"></param>
        /// <param name="inches"></param>
        /// <param name="printHorCenter"></param>
        /// <param name="printVertiCenter"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static void SheetPrintMargin(ExcelWorksheet excelsheet, bool inches = false, double? Top = null, double? Right = null, double? Bottom = null, double? Left = null, double? Header = null, double? Footer = null, int scale = 100, bool printHorCenter = false, bool printVertiCenter = false)
        {
            double centerMeterToInch = 0.3937007874;
            if (inches)
            {
                if (Top.HasValue)
                    excelsheet.PrinterSettings.TopMargin = Convert.ToDecimal(Top);
                if (Right.HasValue)
                    excelsheet.PrinterSettings.RightMargin = Convert.ToDecimal(Right);
                if (Bottom.HasValue)
                    excelsheet.PrinterSettings.BottomMargin = Convert.ToDecimal(Bottom);
                if (Left.HasValue)
                    excelsheet.PrinterSettings.LeftMargin = Convert.ToDecimal(Left);
                if (Header.HasValue)
                    excelsheet.PrinterSettings.HeaderMargin = Convert.ToDecimal(Header);
                if (Footer.HasValue)
                    excelsheet.PrinterSettings.FooterMargin = Convert.ToDecimal(Footer);
            }
            else
            {
                if (Top.HasValue)
                    excelsheet.PrinterSettings.TopMargin = Convert.ToDecimal(Top * centerMeterToInch);
                if (Right.HasValue)
                    excelsheet.PrinterSettings.RightMargin = Convert.ToDecimal(Right * centerMeterToInch);
                if (Bottom.HasValue)
                    excelsheet.PrinterSettings.BottomMargin = Convert.ToDecimal(Bottom * centerMeterToInch);
                if (Left.HasValue)
                    excelsheet.PrinterSettings.LeftMargin = Convert.ToDecimal(Left * centerMeterToInch);
                if (Header.HasValue)
                    excelsheet.PrinterSettings.HeaderMargin = Convert.ToDecimal(Header * centerMeterToInch);
                if (Footer.HasValue)
                    excelsheet.PrinterSettings.FooterMargin = Convert.ToDecimal(Footer * centerMeterToInch);
            }

            //列印水平置中
            excelsheet.PrinterSettings.HorizontalCentered = printHorCenter;
            //列印垂直置中
            excelsheet.PrinterSettings.VerticalCentered = printVertiCenter;
            //設定列印時的縮放比例
            excelsheet.PrinterSettings.Scale = scale;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態TH
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <returns></returns>
        public static void CellThStyle(ExcelRange cell, string VALUE, int width = 30)
        {
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.AutoFitColumns(width);
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <returns></returns>
        public static void CellFormulaStyle(ExcelRange cell, string VALUE)
        {
            cell.Formula = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M(border(True)表示要框線)
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <param name="border"></param>
        /// <returns></returns>
        public static void CellFormulaStyle(ExcelRange cell, string Formula, bool border)
        {
            cell.Formula = Formula;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            if (border)
                cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M(border(True)表示要框線),依照format方式顯示數字
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <param name="border"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static void CellFormulaStyle(ExcelRange cell, string Formula, bool border, string format = null)
        {
            cell.Formula = Formula;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            if (border)
                cell.Style.Border.BorderAround(ExcelBorderStyle.Medium, System.Drawing.Color.Black);
            if (!string.IsNullOrEmpty(format))
                cell.Style.Numberformat.Format = format;
        }

        /// <summary>
        /// 動態EXCEL-儲存格型態-C212M
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="VALUE"></param>
        /// <returns></returns>
        public static void CellStyle(ExcelRange cell, string VALUE, System.Drawing.Color Color, string ColorType = "1")
        {
            cell.Value = VALUE;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            switch (ColorType)
            {
                case "1":
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color);
                    break;

                case "2":
                    cell.Style.Font.Color.SetColor(Color);
                    break;
            }
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
        /// 檢查身分證號格式。若身分證號格式正確傳回""，否則傳回錯誤訊息。
        /// </summary>
        /// <param name="IDNO">身分證號(可以是外籍人士身分證號、居留證號)</param>
        /// <param name="rule">要套用的檢核規則，(1:只檢查中華民國身分證號,2:檢查中華民國身分證號與外籍人士身分證號,3:只檢查外籍人士身分證號)</param>
        /// <param name="allowEmpty">是否允許身分證字號為空值，(true:允許空值，false:不允許空值)</param>
        /// <returns></returns>
        public static string CheckIDNO(string IDNO, int rule = 2, bool allowEmpty = false)
        {
            if (allowEmpty == true)
            {
                if (string.IsNullOrEmpty(IDNO)) return "";
            }
            if (IDNO.Trim().Length != 10)
            {
                return "國民身分證統一編號長度不是10碼";
            }
            bool r = false;

            Regex regex = null;
            string ruleForeigner = "[a-zA-Z]{1}[a-dA-D]{1}[0-9]{8}";
            switch (rule)
            {
                case 1:
                    r = MyHelperUtil.IsIDNO(IDNO);
                    return (r ? "" : "國民身分證統一編號格式有誤");

                case 3:
                    regex = new Regex(ruleForeigner);
                    r = regex.IsMatch(IDNO);
                    return (r ? "" : "外籍身份證號格式有誤");

                default:
                    regex = new Regex("[a-zA-Z]{2}");
                    string first2 = IDNO.Substring(0, 2);
                    if (regex.IsMatch(first2))
                    {
                        regex = new Regex(ruleForeigner);
                        r = regex.IsMatch(IDNO);
                        return (r ? "" : "外籍身份證號格式有誤");
                    }
                    else
                    {
                        r = MyHelperUtil.IsIDNO(IDNO);
                        return (r ? "" : "國民身分證統一編號格式有誤");
                    }
            }
        }

        /// <summary>
        /// 檢查時間
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ToCheckTime(this string str)
        {
            str = str.Replace("：", ":");
            if (!str.Contains(":"))
            { return false; }
            if (!HelperUtil.IsNumber(str.SubstringTo(0, 2)) || !HelperUtil.IsNumber(str.SubstringTo(3, 2)))
            { return false; }
            if (str.SubstringTo(0, 2).TOInt32() > 24 || str.SubstringTo(3, 2).TOInt32() > 60)
            { return false; }

            return true;
        }

        /// <summary>
        /// 檢查時間區間(hh:mm-hh:mm)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ToCheckTimePeriod(this string str)
        {
            if (str.Length != 11)
            { return false; }
            if (str[5] != '-')
            { return false; }
            if (!str.Substring(0, 5).ToCheckTime())
            { return false; }
            if (!str.Substring(6, 5).ToCheckTime())
            { return false; }

            return true;
        }

        /// <summary>
        /// 檢查日期
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ToCheckDate(this string str)
        {
            if (!HelperUtil.IsNumber(str))
            { return false; }
            if (str.Length != 7)
            { return false; }
            //大於12
            if (str.SubstringTo(3, 2).CompareTo("12") == 1)
            { return false; }
            //大於31
            if (str.SubstringTo(5, 2).CompareTo("31") == 1)
            { return false; }
            if (str.SubstringTo(3, 2) == "00")
            { return false; }
            if (str.SubstringTo(5, 2) == "00")
            { return false; }
            if (str.SubstringTo(0, 3) == "000")
            { return false; }

            return true;
        }

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
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException("郵件主旨不可為空。");

            MailMessage message = new MailMessage(from, to, subject, body);
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
            if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException("郵件主旨不可為空。");

            MailMessage message = new MailMessage(from, to, subject, body);
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
                ServicePointManager.SecurityProtocol =
                         SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                         SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;

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
                    credential = new NetworkCredential(ConfigModel.MailSenderUserNo, ConfigModel.MailSenderPwd);
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
                string errText = (ex.InnerException == null) ? ex.Message : ex.InnerException.Message;
                int errCode = (ex.InnerException == null) ? ex.HResult : ex.InnerException.HResult;
                result.SetFailureResult(errText, errCode.ToString());
                return result;
            }
            catch (Exception ex)
            {
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

        #region 簡訊傳送

        /// <summary>執行簡訊傳送</summary>
        /// <param name="message">簡訊訊息</param>
        /// <returns>簡訊傳送處理結果</returns>
        public static SMSSentResult SendSMS(SMSMessage message)
        {
            SMSSentResult result = null;
            try
            {
                result = new SMSSentResult(message);
                result.Start = DateTime.Now;
                if (result.TriedTimes < int.MaxValue) result.TriedTimes++;

                if (message == null) throw new ArgumentNullException("簡訊訊息不可為空。");
                if (string.IsNullOrEmpty(message.Phone)) throw new ArgumentNullException("行動電話號碼不可為空。");
                if (string.IsNullOrEmpty(message.Text)) throw new ArgumentNullException("簡訊內容文字不可為空。");

                //由於系統尚未提供簡訊如何發送（流程），因此目前僅能先提供「空殼」方法
                //TODO: 撰寫簡訊實際發送程式碼
                //=====================================
                // 發送簡訊
                //=====================================
                //準備發送內容
                var smsModel = new SmsMsgModel();
                //電話號碼
                smsModel.DEST = result.Phone;
                //訊息
                smsModel.MSG = result.Text;
                //發送
                var sendmsg = new SmsMsgUtil().SendSms(smsModel);

                if (string.IsNullOrEmpty(sendmsg))
                {
                    result.SetSuccessResult();
                }
                else
                {
                    result.SetFailureResult(sendmsg);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (result == null) throw ex;
                result.SetFailureResult(ex);
                return result;
            }
        }

        /// <summary>執行簡訊傳送</summary>
        /// <param name="phone">行動電話號碼</param>
        /// <param name="text">簡訊內容文字</param>
        /// <param name="name">（非必要）簡訊接收者名稱</param>
        /// <returns>簡訊傳送處理結果</returns>
        public static SMSSentResult SendSMS(string phone, string text, string name = null)
        {
            var message = new SMSMessage();
            message.Phone = phone;
            message.Text = text;
            message.Name = name;
            return SendSMS(message);
        }

        /// <summary>執行簡訊傳送</summary>
        /// <param name="list">簡訊訊息傳送清單</param>
        /// <returns>簡訊傳送處理結果</returns>
        public static void SendSMS(IList<SMSMessage> list)
        {
            if (list == null) throw new ArgumentNullException("list");
            SMSSentResult result = null;
            foreach (var message in list)
            {
                try
                {
                    result = SendSMS(message);
                }
                catch (Exception ex)
                {
                    if (result == null) throw ex;
                    result.SetFailureResult(ex);
                }
            }
        }

        /// <summary>執行簡訊傳送</summary>
        /// <param name="list">簡訊訊息傳送清單</param>
        /// <param name="messageCallback">每封簡訊處理之後要執行的自訂處理方法。輸入 null 表示不需要</param>
        /// <returns>簡訊傳送處理結果</returns>
        public static void SendSMS(IList<SMSMessage> list, Action<SMSSentResult> messageCallback)
        {
            if (list == null) throw new ArgumentNullException("list");
            SMSSentResult result = null;
            foreach (var message in list)
            {
                try
                {
                    result = SendSMS(message);
                    if (messageCallback != null) messageCallback(result);
                }
                catch (Exception ex)
                {
                    if (result == null) throw ex;
                    result.SetFailureResult(ex);
                }
            }
        }

        #endregion 簡訊傳送

        #region 產生隨機碼

        /// <summary>
        /// 產生一個字串，長度是指定長度，內容由指定的可用字元當中隨機組合而成。
        /// </summary>
        /// <param name="AvailableCharSet">可用字元。</param>
        /// <param name="NeededLength">所需字串長度。</param>
        /// <returns></returns>
        public static string GetRandomString(string AvailableCharSet, uint NeededLength)
        {
            /*
             * Gary Lu 20180705:原為AS/M01M"忘記密碼"用產生隨機新密碼用函數，AT/C201隨機產生進入碼也要用，提出共用
             */
            if (AvailableCharSet.Length == 0)
            {
                throw new ArgumentNullException("AvailableCharSet", "必須有可用字元。");
            }
            if (NeededLength < 1)
            {
                throw new ArgumentException("NeededLength", "所需字串長度必須大於0");
            }
            //隨機產生一個正整數,對應到AvailableCharSet的各字元位置
            int x = 0;
            Random rand = new Random();
            string result = string.Empty;
            while (result.Length < NeededLength)
            {
                //取個亂數
                x = rand.Next(0, AvailableCharSet.Length);
                //對應到AvailableCharSet哪個字元就該字元加入result
                result += AvailableCharSet[x];
            }
            return result;
        }

        #endregion 產生隨機碼

        public static List<string> ExportDat(string Merchant, string Terminal, IList<ExportDatGridModel> Grid)
        {
            if (string.IsNullOrEmpty(Merchant)) return null;
            if (string.IsNullOrEmpty(Terminal)) return null;
            var resFile = new List<string>();
            // 檔頭資料
            var a1 = "H";                                                           // HEADER   長度 1
            var a2 = DateTime.Now.ToString("yyyyMMdd");                             // 送檔日期  長度 8
            var a3 = DateTime.Now.ToString("HHmmssffff");                           // 序號      長度 10
            var a4 = Grid.ToCount().ToString().PadLeft(12, '0');                    // 總筆數    長度 12
            var a5 = "+";                                                           // 金額正負號 長度 1
            var a6 = Grid.Sum(x => x.price.TOInt64()).ToString().PadLeft(12, '0');  // 總金額    長度 12
            var a7 = "".PadLeft(216, ' ');                                          // 空白      長度 216
            resFile.Add(a1 + Merchant + a2 + a3 + a4 + a5 + a6 + a7);
            // 明細資料
            foreach (var row in Grid)
            {
                if (string.IsNullOrEmpty(row.payed_orderid) || string.IsNullOrEmpty(row.payed_sessionkey)) continue;
                var b1 = row.payed_orderid.TONotNullString().PadRight(40, ' ');     // 訂單編號  長度 40
                var b2 = "".PadLeft(19, ' ');                                       // 空白      長度 19
                var b3 = row.price.TONotNullString().PadLeft(8, '0');               // 交易金額  長度 8
                var b4 = row.payed_approvecode.TONotNullString().PadRight(8, ' ');  // 授權碼    長度 8
                var b5 = "02";                                                      // 交易碼    長度 2
                var b6 = row.payed_transdate.TONotNullString();                     // 交易日期  長度 8
                if (string.IsNullOrEmpty(row.payed_approvecode) || string.IsNullOrEmpty(row.payed_transdate))
                {
                    var obj = ReGetNCCCApiDatas(row.payed_sessionkey);
                    if (obj == null || string.IsNullOrEmpty(obj.getAPPROVECODE())) continue;
                    b4 = obj.getAPPROVECODE().TONotNullString().PadRight(8, ' ');
                    b6 = obj.getTRANSDATE();
                }
                var b7 = "".PadLeft(16, ' ');   // 使用者自訂欄位 長度 16
                var b8Utf8 = "".PadLeft(20, '\u3000');  // 卡人資訊       長度 20個全形空白
                Encoding big5 = Encoding.GetEncoding("big5");
                byte[] big5Bytes = big5.GetBytes(b8Utf8);
                string b8 = big5.GetString(big5Bytes);
                //var b9 = DateTime.Now.ToString("YYMMDD");   // 帳單處理日期
                //var b10 = "00 ";   // 回應碼 固定三碼“00 ” (左靠右補空白) 為請款成功
                //var b10 = "00 ";   // 帳單處理日期
                var b9 = "".PadLeft(111, ' ');  // 其他欄位補滿
                resFile.Add(Merchant + Terminal + b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8 + b9);
            }
            return resFile;
        }

        private static ApiClient ReGetNCCCApiDatas(string Key)
        {
            if (Key.TONotNullString() == "") return null;
            // 取 DB 參數設定
            var dao = new FrontDAO();
            var GetSetups = dao.QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);
            // 去 API 抓資料
            ApiClient apiClient = new ApiClient();
            apiClient.setKEY(Key);
            apiClient.setURL(
                dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_DOMAINNAME"),  // 聯合正式或測試DomainName
                dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_REQUESTURL")   // 聯合正式或測試RequestUrl
            );
            int res = apiClient.postQuery();
            return apiClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="parameters"></param>
        public static void ProcessCmd(string filename, string parameters)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + parameters + filename);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
        }
    }
}