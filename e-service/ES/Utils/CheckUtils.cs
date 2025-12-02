using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using log4net;

namespace ES.Utils
{
    public class CheckUtils
    {
        private static readonly ILog logger = LogUtils.GetLogger();

        /// <summary>
        /// 是否為身分證號或統一證號
        /// </summary>
        /// <param name="id">身分證號或統一證號</param>
        /// <returns></returns>
        public static bool IsIdentity(string id)
        {

            if (id == null || "".Equals(id))
            {
                return false;
            }

            char[] pidCharArray = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

            // 原身分證英文字應轉換為10~33，這裡直接作個位數*9+10 
            int[] pidIDInt = new int[] { 1, 10, 19, 28, 37, 46, 55, 64, 39, 73, 82, 2, 11, 20, 48, 29, 38, 47, 56, 65, 74, 83, 21, 3, 12, 30 };

            // 原居留證第一碼英文字應轉換為10~33，十位數*1，個位數*9，這裡直接作[(十位數*1) mod 10] + [(個位數*9) mod 10] 
            int[] pidResidentFirstInt = new int[] { 1, 10, 9, 8, 7, 6, 5, 4, 9, 3, 2, 2, 11, 10, 8, 9, 8, 7, 6, 5, 4, 3, 11, 3, 12, 10 };

            // 原居留證第二碼英文字應轉換為10~33，並僅取個位數*6，這裡直接取[(個位數*6) mod 10] 
            int[] pidResidentSecondInt = new int[] { 0, 8, 6, 4, 2, 0, 8, 6, 2, 4, 2, 0, 8, 6, 0, 4, 2, 0, 8, 6, 4, 2, 6, 0, 8, 4 };

            id = id.ToUpper(); // 轉換大寫
            
            char[] strArr = id.ToCharArray(); // 字串轉成char陣列
            int verifyNum = 0;

            //判斷是身份證還是統一證號
            Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Z]+$");
            Regex reg_new = new System.Text.RegularExpressions.Regex(@"^[8-9]+$");
            if (reg.IsMatch(id.Substring(1, 1)) || reg_new.IsMatch(id.Substring(1,1)))
            {
                if (reg.IsMatch(id.Substring(1, 1)))
                {
                    /* 檢查統一證(居留證)編號 舊*/
                    Regex re2 = new Regex(@"^[A-Z]{1}[A-D]{1}[0-9]{8}$");
                    Match m2 = re2.Match(id);
                    verifyNum = 0;
                    if (m2.Success)
                    {
                        // 第一碼 
                        verifyNum += pidResidentFirstInt[Array.BinarySearch(pidCharArray, strArr[0])];
                        // 第二碼 
                        verifyNum += pidResidentSecondInt[Array.BinarySearch(pidCharArray, strArr[1])]; 
                        // 第三~八碼 
                        for (int i = 2, j = 7; i < 9; i++, j--)
                        {
                            verifyNum += int.Parse(strArr[i].ToString()) * j;
                        }
                        // 檢查碼 
                        verifyNum = (10 - (verifyNum % 10)) % 10;

                        return verifyNum == int.Parse(strArr[9].ToString()) ? true : false;
                    }
                    else
                        return false;
                }else if (reg_new.IsMatch(id.Substring(1, 1)))
                {
                    /* 檢查統一證(居留證)編號 新*/
                    Regex re2_new = new Regex(@"^[A-Z]{1}[8-9]{1}[0-9]{8}$");
                    Match m2_new = re2_new.Match(id);
                    verifyNum = 0;
                    if (m2_new.Success)
                    {
                        // 第一碼 
                        verifyNum += pidResidentFirstInt[Array.BinarySearch(pidCharArray, strArr[0])];
                        // 第二~九碼 
                        for (int i = 1, j = 8; i < 9; i++, j--)
                        {
                            verifyNum += int.Parse(strArr[i].ToString()) * j;
                        }
                        // 檢查碼 
                        verifyNum = (10 - (verifyNum % 10)) % 10;

                        return verifyNum == int.Parse(strArr[9].ToString()) ? true : false;
                    }
                    else
                        return false;
                }
                return false;
            }
            else
            {
                /* 檢查身分證字號 */
                Regex re1 = new Regex(@"^[A-Z]{1}[1-2]{1}[0-9]{8}$");
                Match m1 = re1.Match(id);
                if (m1.Success)
                {
                    // 第一碼 
                    verifyNum = verifyNum + pidIDInt[Array.BinarySearch(pidCharArray, strArr[0])];
                    // 第二~九碼 
                    for (int i = 1, j = 8; i < 9; i++, j--)
                    {
                        verifyNum += int.Parse(strArr[i].ToString()) * j;
                    }
                    // 檢查碼 
                    verifyNum = (10 - (verifyNum % 10)) % 10;

                    //return verifyNum == (char)strArr[9];
                    return verifyNum == int.Parse(strArr[9].ToString()) ? true : false;
                }
                else
                    return false;
            }

            ////-------------------------------------------------

            //if (id == null) return false;

            //id = id.ToUpper();

            //Regex re1 = new Regex(@"^[A-Z][CD]\d{8}$");
            //Match m1 = re1.Match(id);

            //if (m1.Success) return true;

            //Regex re = new Regex(@"^[A-Z][12A-D]\d{8}$");
            //Match m = re.Match(id);

            //if (!m.Success) return false;

            //string strA = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            //string strB = "0123456789";

            //int index = 0;
            //int iChkNum = 0;

            //// 第1碼
            //index = strA.IndexOf(id.Substring(0, 1)) + 10;
            //iChkNum += Convert.ToInt16(Math.Floor((double)index / 10)) + (index % 10 * 9);

            //// 第2碼
            //if ("ABCD".IndexOf(id.Substring(1, 1)) < 0) // 一般身分證號
            //{
            //    index = strB.IndexOf(id.Substring(1, 1));
            //}
            //else // 統一證號
            //{
            //    index = strB.IndexOf(id.Substring(1, 1)) + 10;
            //}
            //iChkNum += index % 10 * 8;

            //// 第3~9碼
            //for (int i = 2; i < id.Length - 1; i++)
            //{
            //    index = strB.IndexOf(id.Substring(i, 1));
            //    iChkNum += index * (9 - i);
            //}

            //// 第10碼
            //index = strB.IndexOf(id.Substring(9, 1));
            //iChkNum += index;

            //if (iChkNum % 10 == 0)
            //{
            //    return true;
            //}

            //return false;
        }

        public static string GetFormId(string value)
        {
            Regex re = new Regex(@"^\d{1,3}$");

            if (re.IsMatch(value))
            {
                return value;
            }
            return "";
        }

        public static string GetHtmlEncode(string value)
        {
            value = HttpUtility.HtmlEncode(value);

            value = value.Replace("&lt;font ", "<font ");
            value = value.Replace("&lt;br", "<br");

            int idx = value.IndexOf("<");

            while (idx >= 0)
            {
                idx = value.IndexOf("&gt;", idx);
                value = value.Substring(0, idx) + ">" + value.Substring(idx + 4);
                idx = value.IndexOf("<", idx);
            }

            value = value.Replace("&lt;/font&gt;", "</font>");

            return value;
        }

        public static string GetServiceId(string value)
        {
            Regex re = new Regex(@"^\d{6}$");

            return (re.IsMatch(value) ? value : "");
        }
        

        /// <summary>
        /// 是否為日期格式 (yyyy/MM/dd)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDate(string value)
        {
            try
            {
                logger.Debug("value: " + value);
                DateTime NewDate = DateTime.ParseExact(value, "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces);

                logger.Debug("true");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 檢核中華民國外僑及大陸人士在台居留證(舊式+新式)
        /// </summary>
        /// <param name="idNo">身分證</param>
        /// <returns></returns>
        public static bool CheckResidentID(string idNo)
        {
            if (idNo == null)
            {
                return false;
            }
            idNo = idNo.ToUpper();
            Regex regex = new Regex(@"^([A-Z])(A|B|C|D|8|9)(\d{8})$");
            Match match = regex.Match(idNo);
            if (!match.Success)
            {
                return false;
            }

            if ("ABCD".IndexOf(match.Groups[2].Value) >= 0)
            {
                //舊式
                return CheckOldResidentID(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
            }
            else
            {
                //新式(2021/01/02)正式生效
                return CheckNewResidentID(match.Groups[1].Value, match.Groups[2].Value + match.Groups[3].Value);
            }
        }

        /// <summary>
        /// 舊式檢核
        /// </summary>
        /// <param name="firstLetter">第1碼英文字母(區域碼)</param>
        /// <param name="secondLetter">第2碼英文字母(性別碼)</param>
        /// <param name="num">第3~9流水號 + 第10碼檢查碼</param>
        /// <returns></returns>
        private static bool CheckOldResidentID(string firstLetter, string secondLetter, string num)
        {
            ///建立字母對應表(A~Z)
            ///A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
            ///P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35 
            string alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            string transferIdNo =
                $"{alphabet.IndexOf(firstLetter) + 10}" +
                $"{(alphabet.IndexOf(secondLetter) + 10) % 10}" +
                $"{num}";
            int[] idNoArray = transferIdNo.ToCharArray()
                                          .Select(c => Convert.ToInt32(c.ToString()))
                                          .ToArray();

            int sum = idNoArray[0];
            int[] weight = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            for (int i = 0; i < weight.Length; i++)
            {
                sum += weight[i] * idNoArray[i + 1];
            }
            return (sum % 10 == 0);
        }

        /// <summary>
        /// 新式檢核
        /// </summary>
        /// <param name="firstLetter">第1碼英文字母(區域碼)</param>
        /// <param name="num">第2碼(性別碼) + 第3~9流水號 + 第10碼檢查碼</param>
        /// <returns></returns>
        private static bool CheckNewResidentID(string firstLetter, string num)
        {
            ///建立字母對應表(A~Z)
            ///A=10 B=11 C=12 D=13 E=14 F=15 G=16 H=17 J=18 K=19 L=20 M=21 N=22
            ///P=23 Q=24 R=25 S=26 T=27 U=28 V=29 X=30 Y=31 W=32  Z=33 I=34 O=35 
            string alphabet = "ABCDEFGHJKLMNPQRSTUVXYWZIO";
            string transferIdNo = $"{(alphabet.IndexOf(firstLetter) + 10)}" +
                                  $"{num}";
            int[] idNoArray = transferIdNo.ToCharArray()
                                          .Select(c => Convert.ToInt32(c.ToString()))
                                          .ToArray();

            int sum = idNoArray[0];
            int[] weight = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
            for (int i = 0; i < weight.Length; i++)
            {
                sum += (weight[i] * idNoArray[i + 1]) % 10;
            }
            return (sum % 10 == 0);
        }
    }
}