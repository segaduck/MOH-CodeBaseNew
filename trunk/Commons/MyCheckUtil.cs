using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EECOnline.Commons
{
    /// <summary>
    /// 系統常用規則驗證檢查方法
    /// </summary>
    public static class MyCheckUtil
    {
        #region 傳回是否為中華民國身分證號（註：本方法不檢查身分證號有效性）
        /// <summary>傳回是否為中華民國身分證號（註：本方法不檢查身分證號有效性）。若通過檢查時傳回 true，反之傳回 false。</summary>
        /// <param name="idno">身分證號</param>
        /// <returns></returns>
        public static bool IsTaiwanIDNO(string idno)
        {
            bool res = false;
            if (!string.IsNullOrEmpty(idno) && idno.Length == 10)
            {
                //檢查第 1 個字是否為 A-Z 或是 a-z
                char ch = idno[0];
                res = (ch >= 0x41 && ch <= 0x5A) || (ch >= 0x61 && ch <= 0x7A);
                if (res == false) return res;

                //檢查第 2 個字是否為 1 或 2
                ch = idno[1];
                res = (ch == '1' || ch == '2');
                if (res == false) return res;
                
                //檢查第 3 - 9 個字是否為數字
                int i = 0;
                res = int.TryParse(idno.Substring(2), out i);
            }
            return res;
        }
        #endregion

        #region 檢查中華民國身分證號是否有效
        /// <summary>檢查中華民國身分證號是否有效。若通過檢查時傳回 true，反之傳回 false。</summary>
        /// <param name="idno">身分證號</param>
        /// <returns></returns>
        public static bool IsValidIDNO(string idno)
        {
            bool res = false;
            if (!string.IsNullOrEmpty(idno) && idno.Length == 10)
            {
                idno = idno.ToUpper();
                char ch = idno[0];
                if (ch >= 0x41 && ch <= 0x5A)  //檢查第一個字是否為 A-Z 英文字母
                {
                    int idx = ((ch) - 65);
                    var a = new [] { 10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33 };
                    var b = new int[11];
                    b[0] = a[idx] / 10;
                    b[1] = a[idx] % 10;
                    int c = b[0];
                    
                    for (int i = 1; i <= 9; i++)
                    {
                        b[i + 1] = idno[i] - 48;
                        c += b[i] * (10 - i);
                    }

                    if (((c % 10) + b[10]) % 10 == 0)
                    {
                        res = true;
                    }
                }
            }
            return res;
        }
        #endregion

        #region 檢查在中華民國身分證號內的性別與選取的性別是否一致
        /// <summary>檢查在中華民國身分證號內的性別與選取的性別是否一致。若通過檢查時傳回 true，反之傳回 false。</summary>
        /// <param name="idno">身分證號</param>
        /// <param name="">性別代碼。(1: 男，2: 女) 或是 (M: 男，F: 女)</param>
        /// <param name="errorMsg">（非必要）當檢查不通過時要傳回的自訂誤錯訊息。</param>
        /// <returns></returns>
        public static bool IsSameSexInIDNO(string idno, string sex)
        {
            if (!string.IsNullOrEmpty(idno) && idno.Length == 10)
            {
                char ch = idno[1];
                switch (sex)
                {
                    case "1": return (ch == '1');
                    case "2": return (ch == '2');
                    case "M": return (ch == '1');
                    case "F": return (ch == '2');
                    default: throw new Exception(string.Concat("無法判斷性別代碼 \"", sex , "\" 是男或是女。"));
                }
            }
            return false;
        }
        #endregion
    }
}