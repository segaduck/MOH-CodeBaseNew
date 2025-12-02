using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ES.Helpers
{
    /// <summary>
    /// 新的流水序號動態產生輔助方法類別。流水序號從 1 到 9223372036854775807，到達最大值時再從 1 開始。
    /// </summary>
    public static class SequenceIdExtension
    {
        /// <summary>執行緒資源鎖物件</summary>
        private readonly static object _SnoLock = new object();
        
        /// <summary>上次使用的序號</summary>
        private static long _LastSno = 0;

        /// <summary>傳回新的流水序號（注意！流水序號從 1 到 9223372036854775807，到達最大值時再從 1 開始）。</summary>
        /// <param name="html">HTML 控制項輔助物件</param>
        /// <param name="idPrefix">（非必填）流水序號的前置字串。預設 "__tmp"。</param>
        /// <returns></returns>
        public static string NewSequenceId(this HtmlHelper html, string idPrefix = "__tmp")
        {
            lock (_SnoLock) {
                if (_LastSno == long.MaxValue) _LastSno = 0;
                _LastSno++;
            }
            return string.Concat(idPrefix, _LastSno.ToString());
        }

        /// <summary>
        /// 傳回本次頁面內容是否是使用 Html.Partial() 方法輸出的。若是使用 Html.Partial() 方法輸出時傳回 true，否則傳回 false。 
        /// </summary>
        /// <param name="html">HTML 控制項輔助物件</param>
        /// <returns></returns>
        public static bool IsPartialOutput(this HtmlHelper html)
        {
            bool result = false;
            if (html != null)
            {
                var page = html.ViewDataContainer as WebViewPage;
                return (page != null && string.IsNullOrEmpty(page.Layout));
            }
            return result;
        }
    }
}