using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Turbo.DataLayer;
using Turbo.ReportTK;
using Turbo.ReportTK.Excel;
using EECOnline.Services;
using 即測即評加解密模組;
using Turbo.Commons;
using OfficeOpenXml;
using System.Reflection;

namespace EECOnline.Commons
{
    /// <summary>
    /// 放置 EECOnline 專用的 CommonUtil 擴充,
    /// 只有當 Turbo.Commons.CommonUtil 不足時才寫新的
    /// </summary>
    public class MyCommonUtil : Turbo.Commons.CommonUtil
    {
        #region 將 IList 集合項目轉換成在 HTML select 內 <option> 標籤 HTML 字串
        /// <summary>
        /// 組成在 HTML &lt;select&gt; 標籤內使用的 &lt;option&gt; 標籤 HTML 字串
        /// </summary>
        /// <param name="list">「代碼-名稱」項目集合</param>
        /// <param name="selectValue">預設選取的項目值。輸入 null 表示沒有預設選取項目</param>
        /// <param name="blankOptionCode">空白項目值。輸入 null 表示不需要加入空白項目。預設空白字元。</param>
        /// <param name="blankOptionText">空白項目顯示名稱。</param>
        /// <param name="TextWithCode">顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <returns></returns>
        public static string BuildOptionHtml(IList<KeyMapModel> list, string selectValue = "",
                                             string blankOptionCode = " ", string blankOptionText = "", bool textWithCode = false)
        {
            var sb = new StringBuilder();
            //空白選項
            if (!string.IsNullOrEmpty(blankOptionCode))
            {
                string optCode = HttpUtility.HtmlDecode(blankOptionCode);
                sb.Append("<option value=\"");
                sb.Append(optCode);
                sb.Append("\"");
                if (selectValue != null && selectValue == blankOptionCode) sb.Append(" selected");
                sb.Append(">");
                if (textWithCode)
                {
                    sb.Append(optCode);
                    sb.Append(" ");
                }
                sb.Append(HttpUtility.HtmlDecode(blankOptionText));
                sb.Append("</option>");
            }
            //項目集合選項
            if (list != null && list.Count > 0)
            {
                string optCode = null;
                foreach (var n in list)
                {
                    optCode = HttpUtility.HtmlDecode(n.CODE);
                    sb.Append("<option value=\"");
                    sb.Append(optCode);
                    sb.Append("\"");
                    if (selectValue != null && n.CODE == selectValue) sb.Append(" selected");
                    sb.Append(">");
                    if (textWithCode)
                    {
                        sb.Append(optCode);
                        sb.Append(" ");
                    }
                    sb.Append(HttpUtility.HtmlDecode(n.TEXT));
                    sb.Append("</option>");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 傳回在 HTML &lt;select&gt; 標籤內具有「請選擇」項目的 &lt;option&gt; 標籤 HTML 的 AJAX 非同步作業結果。
        /// 注意！本方法只能與網頁端 Javascript 的 ajaxLoadMore() 方法搭配使用。
        /// </summary>
        /// <param name="list">「代碼-名稱」項目集合</param>
        /// <param name="selectValue">預設選取的項目值。輸入 null 表示沒有預設選取項目</param>
        /// <param name="blankOptionCode">空白項目值。</param>
        /// <param name="blankOptionText">（非必要）空白項目顯示名稱。</param>
        /// <param name="TextWithCode">（非必要）顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <returns></returns>
        public static ContentResult BuildOptionHtmlPleaseAjaxResult(IList<KeyMapModel> list, string selectValue, string blankOptionCode, string blankOptionText = "請選擇", bool textWithCode = false)
        {
            var sb = new StringBuilder();
            //請選擇項目
            string optCode = HttpUtility.HtmlDecode(blankOptionCode);
            sb.Append("<option value=\"");
            sb.Append(optCode);
            sb.Append("\"");
            if (selectValue != null && selectValue == blankOptionCode) sb.Append(" selected");
            sb.Append(">");
            if (textWithCode)
            {
                sb.Append(optCode);
                sb.Append(" ");
            }
            sb.Append(HttpUtility.HtmlDecode(blankOptionText));
            sb.Append("</option>");

            //項目集合選項
            if (list != null && list.Count > 0)
            {
                foreach (var n in list)
                {
                    optCode = HttpUtility.HtmlDecode(n.CODE);
                    sb.Append("<option value=\"");
                    sb.Append(optCode);
                    sb.Append("\"");
                    if (selectValue != null && n.CODE == selectValue) sb.Append(" selected");
                    sb.Append(">");
                    if (textWithCode)
                    {
                        sb.Append(optCode);
                        sb.Append(" ");
                    }
                    sb.Append(HttpUtility.HtmlDecode(n.TEXT));
                    sb.Append("</option>");
                }
            }

            var bag = new Turbo.Commons.AjaxResultStruct();
            bag.data = sb.ToString();
            bag.status = true;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回在 HTML &lt;select&gt; 標籤內使用的 &lt;option&gt; 標籤 HTML 的 AJAX 非同步作業結果。
        /// 注意！本方法只能與網頁端 Javascript 的 ajaxLoadMore() 方法搭配使用。
        /// </summary>
        /// <param name="list">「代碼-名稱」項目集合</param>
        /// <param name="selectValue">預設選取的項目值。輸入 null 表示沒有預設選取項目</param>
        /// <param name="blankOptionCode">空白項目值。輸入 null 表示不需要加入空白項目。預設空白字元。</param>
        /// <param name="blankOptionText">空白項目顯示名稱。</param>
        /// <param name="TextWithCode">顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <returns></returns>
        public static ContentResult BuildOptionHtmlAjaxResult(IList<KeyMapModel> list, string selectValue = "",
                                                              string blankOptionCode = " ", string blankOptionText = "", bool textWithCode = false)
        {
            var bag = new Turbo.Commons.AjaxResultStruct();
            bag.data = MyCommonUtil.BuildOptionHtml(list, selectValue, blankOptionCode, blankOptionText, textWithCode);
            bag.status = true;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回在 HTML &lt;select&gt; 標籤內使用的 &lt;option&gt; 標籤 HTML 的 AJAX 非同步作業結果。
        /// 注意！本方法只能與網頁端 Javascript 的 ajaxLoadMore() 方法搭配使用。
        /// </summary>
        /// <param name="list">「代碼-名稱」項目集合</param>
        /// <param name="selectValue">預設選取的項目值。輸入 null 表示沒有預設選取項目</param>
        /// <param name="blankOptionCode">空白項目值。輸入 null 表示不需要加入空白項目。預設空白字元。</param>
        /// <param name="blankOptionText">空白項目顯示名稱。</param>
        /// <param name="TextWithCode">顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <returns></returns>
        public static ContentResult BuildOptionHtmlAjaxResult(IList<SelectListItem> list, string selectValue = "",
                                                              string blankOptionCode = " ", string blankOptionText = "", bool textWithCode = false)
        {
            string defaultSelect = string.IsNullOrEmpty(selectValue) ? "" : selectValue;
            var mapList = new List<KeyMapModel>();
            if (list != null)
            {
                KeyMapModel opt = null;
                foreach (var item in list)
                {
                    opt = new KeyMapModel();
                    opt.CODE = item.Value;
                    opt.TEXT = item.Text;
                    if (item.Selected && defaultSelect.Length == 0) defaultSelect = item.Value;
                    mapList.Add(opt);
                }
            }

            var bag = new Turbo.Commons.AjaxResultStruct();
            bag.data = MyCommonUtil.BuildOptionHtml(mapList, defaultSelect, blankOptionCode, blankOptionText, textWithCode);
            bag.status = true;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }
        #endregion

        #region 將傳入的代碼清單轉換成 IList 集合
        /// <summary>將傳入的代碼清單轉換成 IList&lt;SelectListItem&gt 集合</summary>
        /// <param name="keyMapList">代碼-名稱項目集合。</param>
        /// <param name="TextWithCode">顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <param name="selectedCode">預設選取的項目代碼（null 表示不選取任何項目）。</param>
        /// <param name="addBlankItem">指示是否要自動加入一個空項目。(true: 加入，false: 不加入)。預設 false。</param>
        /// <param name="blankItemCode">當 addBlankItem 參數等於 true 時，自訂的空項目代碼。預設 ""。</param>
        /// <param name="blankItemText">當 addBlankItem 參數等於 true 時，自訂的空項目代碼對應顯示文字。預設 ""。</param>
        /// <returns></returns>
        public static IList<SelectListItem> ConvertSelItems(IList<KeyMapModel> keyMapList, bool TextWithCode = false,
                                                            string selectedCode = "", bool addBlankItem = false, string blankItemCode = "", string blankItemText = "")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (keyMapList != null)
            {
                if (addBlankItem)
                {
                    var item = new SelectListItem() { Text = blankItemText, Value = blankItemCode };
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    if (TextWithCode) item.Text = string.Concat(item.Value, " ", item.Text);
                    list.Add(item);
                }

                foreach (KeyMapModel hash in keyMapList)
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = (TextWithCode ? hash.CODE + "." : "") + hash.TEXT;
                    item.Value = hash.CODE;
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    list.Add(item);
                }
            }

            return list;
        }

        /// <summary>將傳入的代碼清單轉換成 IList&lt;SelectListItem&gt 集合</summary>
        /// <param name="keyMapList">代碼-名稱項目集合。</param>
        /// <param name="TextWithCode">顯示項目名稱時，是否也要顯示項目代碼。(true: 顯示，false: 不顯示)。預設 false。</param>
        /// <param name="selectedCode">預設選取的項目代碼（null 表示不選取任何項目）。</param>
        /// <param name="addBlankItem">指示是否要自動加入一個空項目。(true: 加入，false: 不加入)。預設 false。</param>
        /// <param name="blankItemCode">當 addBlankItem 參數等於 true 時，自訂的空項目代碼。預設 ""。</param>
        /// <param name="blankItemText">當 addBlankItem 參數等於 true 時，自訂的空項目代碼對應顯示文字。預設 ""。</param>
        public static IList<SelectListItem> ConvertSelItems(IDictionary<string, string> dictionary, bool TextWithCode = false,
                                                            string selectedCode = "", bool addBlankItem = false, string blankItemCode = "", string blankItemText = "")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (dictionary != null)
            {
                if (addBlankItem)
                {
                    var item = new SelectListItem() { Text = blankItemText, Value = blankItemCode };
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    if (TextWithCode) item.Text = string.Concat(item.Value, " ", item.Text);
                    list.Add(item);
                }

                foreach (var pair in dictionary)
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = (TextWithCode ? pair.Key + "." : "") + pair.Value;
                    item.Value = pair.Key;
                    item.Selected = (selectedCode != null && item.Value == selectedCode);
                    list.Add(item);
                }
            }

            return list;
        }
        #endregion

        #region 移除在「代碼-名稱項目」集合內的代碼項目
        /// <summary>
        /// 移除在代碼-名稱項目集合內的代碼項目。
        /// </summary>
        /// <param name="list">代碼-名稱項目集合。</param>
        /// <param name="itemCode">要移除的項目代碼。</param>
        public static void RemoveKeyMapItem(IList<KeyMapModel> list, string itemCode)
        {
            if (list != null)
            {
                int found = -1;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].CODE == itemCode) found = i;
                }

                if (found >= 0) list.RemoveAt(found);
            }
        }

        /// <summary>
        /// 在「代碼-名稱項目集合」內移除指定的代碼項目。
        /// </summary>
        /// <param name="list">代碼-名稱項目集合。</param>
        /// <param name="itemCodes">要移除的項目代碼字串陣列。</param>
        public static void RemoveKeyMapItem(IList<KeyMapModel> list, params string[] itemCodes)
        {
            if (list != null && itemCodes != null)
            {
                var ub = list.Count - 1;
                var found = new List<KeyMapModel>();
                foreach (var code in itemCodes)
                {
                    foreach (var item in list)
                    {
                        if (item.CODE == code)
                        {
                            found.Add(item);
                            break;
                        }
                    }
                }

                foreach (var item in found)
                {
                    list.Remove(item);
                }
            }
        }

        /// <summary>
        /// 在「代碼-名稱項目集合」內保留指定的代碼項目，不是保留的代碼項目一律移除。
        /// </summary>
        /// <param name="list">代碼-名稱項目集合。</param>
        /// <param name="itemCodes">要保留的項目代碼字串陣列。</param>
        public static void ReserveKeyMapItem(IList<KeyMapModel> list, params string[] itemCodes)
        {
            if (list != null && itemCodes != null)
            {
                var ub = list.Count - 1;
                var found = new List<KeyMapModel>();
                foreach (var code in itemCodes)
                {
                    foreach (var item in list)
                    {
                        if (item.CODE != code)
                        {
                            found.Add(item);
                        }
                    }
                }

                foreach (var item in found)
                {
                    list.Remove(item);
                }
            }
        }
        #endregion

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
                foreach (ModelState n in modelState.Values)
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

        /// <summary>移除在 ModelState 內的錯誤訊息</summary>
        /// <param name="modelState">ModelState 物件</param>
        /// <param name="fieldName">（非必要）在 Model 內的資料欄位名稱。輸入 null 表示移除 ModelState 所有錯誤訊息，輸入非 null 值時表示移除指定的資料欄位錯誤訊息。</param>
        public static void RemoveModelStateErrors(ModelStateDictionary modelState, string fieldName = null)
        {
            if (modelState != null && modelState.Values != null)
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    foreach (var obj in modelState.Values) obj.Errors.Clear();
                }
                else
                {
                    if (modelState.ContainsKey(fieldName)) modelState[fieldName].Errors.Clear();
                }
            }
        }
        #endregion

        #region 傳回 AJAX 非同步作業結果相關方法
        /// <summary>
        /// 傳回 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="result">非同步作業結果。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResult(Turbo.Commons.AjaxResultStruct result)
        {
            if (result == null) throw new ArgumentNullException("result");
            else
            {
                var ret = new ContentResult();
                ret.Content = result.Serialize();
                ret.ContentType = "application/json";
                return ret;
            }
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="ex">要回傳的異常例外錯誤訊息。</param>
        /// <param name="data">（非必要）要回傳的資料物件。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResult(Exception ex, object data = null)
        {
            if (ex == null) throw new ArgumentNullException("請指定異常例外錯誤訊息");
            else
            {
                var bag = new Turbo.Commons.AjaxResultStruct(false);
                bag.message = ex.Message;
                bag.data = data;
                var result = new ContentResult();
                result.Content = bag.Serialize();
                result.ContentType = "application/json";
                return result;
            }
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業成功結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="message">要回傳的訊息文字。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResultSuccess(string message)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(true);
            bag.message = message;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業成功結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="message">要回傳的訊息文字。</param>
        /// <param name="data">要回傳的自訂資料物件。建議使用 var data = new { UID = "9999", UNAME = "王小明" }; 格式程式寫法，在前端網頁才好利用 JSON 語法引用回傳資料。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResultSuccess(string message, object data)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(true);
            bag.message = message;
            bag.data = data;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業失敗結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="message">要回傳的失敗訊息文字。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResultFailure(string message)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(false);
            bag.message = message;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業失敗結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="message">要回傳的失敗訊息文字。</param>
        /// <param name="data">要回傳的自訂資料物件。建議使用 var data = new { UID = "9999", UNAME = "王小明" }; 格式程式寫法，在前端網頁才好利用 JSON 語法引用回傳資料。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResultFailure(string message, object data)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(false);
            bag.message = message;
            bag.data = data;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="message">要回傳的訊息文字。</param>
        /// <param name="isSuccess">（非必要）非同步作業結果。（true: 成功，false: 失敗）。預設 true。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResult(string message, bool isSuccess = true)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(isSuccess);
            bag.message = message;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="data">要回傳的自訂資料物件。</param>
        /// <param name="message">要回傳的訊息文字。</param>
        /// <param name="isSuccess">（非必要）非同步作業結果。（true: 成功，false: 失敗）。預設 true。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResult(object data, string message, bool isSuccess = true)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(isSuccess);
            bag.message = message;
            bag.data = data;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="data">要回傳的自訂資料物件。</param>
        /// <param name="isSuccess">作業執行結果是否為成功，（true: 成功，false: 失敗）。預設為 true。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResult(object data, bool isSuccess = true)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(isSuccess);
            bag.data = data;
            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }

        /// <summary>
        /// 傳回 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxResult() 方法搭配使用。
        /// </summary>
        /// <param name="modelState">驗證檢核訊息集合物件。</param>
        /// <param name="delimiter">在驗證檢核訊息之間的分隔字元。(null： 使用 &lt;ul&gt;...&lt;/ul&gt; HTML 元素樣式傳回驗證檢核訊息，不是 null： 將每則驗證檢核訊息以指定的分隔字元串接起來)。預設 null</param>
        /// <param name="isSuccess">（非必要）非同步作業結果。（true: 成功，false: 失敗）。預設 false。</param>
        /// <returns></returns>
        public static ContentResult BuildAjaxResult(ModelStateDictionary modelState, string delimiter = null, bool isSuccess = false)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(isSuccess);
            if (modelState != null && modelState.Count > 0)
            {
                if (delimiter != null)
                {
                    bag.message = MyCommonUtil.GetModelStateErrors(modelState, delimiter);
                }
                else
                {
                    var list = MyCommonUtil.GetModelStateErrors(modelState);
                    var liTag = new TagBuilder("li");
                    var sb = new StringBuilder();
                    sb.Append("<ul>");
                    foreach (var msg in list)
                    {
                        if (!string.IsNullOrEmpty(msg))
                        {
                            liTag.InnerHtml = msg.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
                            sb.Append(liTag.ToString());
                            sb.Append("<br/>");
                        }
                    }
                    sb.Append("</ul>");
                    bag.message = sb.ToString();
                }
            }

            var result = new ContentResult();
            result.Content = bag.Serialize();
            result.ContentType = "application/json";
            return result;
        }
        #endregion

        #region 傳回 AJAX 非同步產生報表作業結果相關方法
        /// <summary>
        /// 傳回系統報表檔案生成 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxTurboReportResult() 方法搭配使用。
        /// </summary>
        /// <param name="ex">要回傳的異常例外錯誤訊息。</param>
        /// <param name="data">（非必要）要回傳的資料物件。</param>
        /// <returns></returns>
        public static ContentResult BuildTurboReportAjaxResult(Exception ex, object data = null)
        {
            if (ex == null) throw new ArgumentNullException("請指定異常例外錯誤訊息");
            else
            {
                var bag = new Turbo.Commons.AjaxResultStruct(false);

                //組成最終要回傳資料
                bag.data = new
                {
                    data = data,
                    turboFile = CreateTurboFileData(null, data, null)
                };

                bag.message = ex.Message;
                var result = new ContentResult();
                result.Content = bag.Serialize();
                result.ContentType = "application/json";
                return result;
            }
        }

        /// <summary>
        /// 傳回系統報表檔案生成 AJAX 非同步作業結果。注意！本方法只能與網頁端 Javascript 的 ajaxTurboReportResult() 方法搭配使用。
        /// </summary>
        /// <param name="reportResult">報表生成結果物件。</param>
        /// <param name="data">要回傳的自訂資料物件。</param>
        /// <param name="isSuccess">作業執行結果是否為成功，（true: 成功，false: 失敗）。</param>
        /// <param name="fileDownloadName">自訂的下載檔案名稱。</param>
        /// <returns></returns>
        public static ContentResult BuildTurboReportAjaxResult(ActionResult reportResult, object data = null, bool isSuccess = true, string fileDownloadName = null)
        {
            var bag = new Turbo.Commons.AjaxResultStruct(isSuccess);

            //組成最終要回傳資料
            bag.data = new
            {
                data = data,
                turboFile = CreateTurboFileData(reportResult, data, fileDownloadName)
            };

            var ret = new ContentResult();
            ret.Content = bag.Serialize();
            ret.ContentType = "application/json";
            return ret;
        }
        #endregion

        /// <summary>
        /// 建立「系統報表檔案生成結果」資料物件。
        /// </summary>
        /// <param name="reportResult">報表生成結果物件。</param>
        /// <param name="data">要回傳的自訂資料物件。</param>
        /// <param name="fileDownloadName">自訂的下載檔案名稱。</param>
        /// <returns></returns>
        private static object CreateTurboFileData(ActionResult reportResult, object data, string fileDownloadName)
        {
            string mimeContentType = "";
            string base64Content = "";

            var result1 = reportResult as FileContentResult;

            if (result1 == null)
            {
                var result2 = reportResult as ContentResult;
                if (result2 != null)
                {
                    //當 result 為 ContentResult 型別時...
                    mimeContentType = string.IsNullOrEmpty(result2.ContentType) ? "text/plain" : result2.ContentType;
                    base64Content = System.Convert.ToBase64String(result2.ContentEncoding.GetBytes(result2.Content));
                }
            }
            else
            {
                //當 result 為 FileContentResult 型別時...
                mimeContentType = string.IsNullOrEmpty(result1.ContentType) ? "application/octet-stream" : result1.ContentType;
                base64Content = System.Convert.ToBase64String(result1.FileContents);
                if (string.IsNullOrEmpty(fileDownloadName)) fileDownloadName = result1.FileDownloadName;
            }

            //組成業界標準的 Data URI Schema 字串
            StringBuilder sb = null;
            if (reportResult != null)
            {
                sb = new StringBuilder();
                sb.Append("data:");
                sb.Append(mimeContentType);
                sb.Append(";base64,");
                sb.Append(base64Content);
            }

            var ret = new
            {
                contentType = mimeContentType,
                fileName = (reportResult == null) ? "" : (string.IsNullOrEmpty(fileDownloadName) ? "Report" : fileDownloadName),
                dataURI = (sb == null) ? "" : sb.ToString()
            };

            return ret;
        }

        #region 系統報表資料查詢相關方法
        /// <summary>
        /// 將報表查詢結果資料轉換成可操作的 Excel 物件。
        /// </summary>
        /// <param name="report">報表查詢結果資料物件</param>
        /// <returns>ExcelPackge 型別物件</returns>
        public static ExcelPackage TransReportDataToExcel(Turbo.ReportTK.Report report)
        {
            if (report == null)
            {
                return new ExcelPackage();
            }
            else
            {
                var content = new ReportExcel().Create(report);
                var stream = new MemoryStream(content);
                return new ExcelPackage(stream);
            }
        }

        /// <summary>
        /// 執行報表資料查詢。當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時，錯誤訊息會存放在 ModelState 內。
        /// 請在每次執行 QueryReportData() 後，立即檢查 ModelState.IsValid 屬性值以便處理非預期狀況。
        /// 註：若想要將「報表查詢結果資料」直接轉換成可操作的 Excel 物件時，請使用 TransReportDataToExcel() 方法。
        /// </summary>
        /// <param name="reportService">系統報表服務物件。</param>
        /// <param name="report">報表查詢結果資料物件</param>
        /// <param name="modelState">用來存放當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時的錯誤訊息。</param>
        private static void QueryReportData(ReportService reportService, Turbo.ReportTK.Report report, ModelStateDictionary modelState)
        {
            try
            {
                // 檢核必填欄位是否已有輸入
                string val = null;
                foreach (Condition condition in report.Query.Conditions)
                {
                    if (condition.Required)
                    {
                        if (report.QueryParms == null)
                        {
                            modelState.AddModelError(condition.Id, condition.Name + " 欄位為必填");
                        }
                        else
                        {
                            val = report.QueryParms[condition.Id] as string;
                            if (string.IsNullOrEmpty(val))
                            {
                                modelState.AddModelError(condition.Id, condition.Name + " 欄位為必填");
                            }
                        }
                    }
                }

                if (modelState.IsValid)
                {
                    // 執行報表資料查詢
                    if (!reportService.ExecuteQuery(report))
                    {
                        modelState.AddModelError("", "SQL查詢逾時（資料太多），請縮小查詢範圍!");
                    }
                    else if (report.Results.Count == 0)
                    {
                        modelState.AddModelError("", "查無資料!");
                    }
                }
            }
            catch (Exception e)
            {
                if (modelState == null) throw e;
                else modelState.AddModelError("", e.Message);
            }
        }

        /// <summary>
        /// 執行報表資料查詢。當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時，錯誤訊息會存放在 ModelState 內。
        /// 請在每次執行 QueryReportData() 後，立即檢查 ModelState.IsValid 屬性值以便處理非預期狀況。
        /// 註：若想要將「報表查詢結果資料」直接轉換成可操作的 Excel 物件時，請使用 TransReportDataToExcel() 方法。
        /// </summary>
        /// <param name="reportId">在 Report.config 檔案內的報表設定項目 id。</param>
        /// <param name="params">報表查詢條件值。</param>
        /// <param name="modelState">用來存放當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時的錯誤訊息。輸入 null 表示不需要。</param>
        /// <returns>系統報表物件</returns>
        public static Turbo.ReportTK.Report QueryReportData(string reportId, Hashtable @params, ModelStateDictionary modelState = null)
        {
            if (string.IsNullOrEmpty(reportId)) throw new ArgumentNullException("reportId");

            //從Config找報表資訊
            var reportService = new ReportService();
            var report = reportService.GetReportConfig().GetReport(reportId);
            if (report == null)
            {
                throw new NotImplementedException("找不到 Report '" + reportId + "' 的設定資訊!");
            }

            report.QueryParms = @params;
            if (modelState != null)
            {
                QueryReportData(reportService, report, modelState);
            }
            else
            {
                modelState = new ModelStateDictionary();
                QueryReportData(reportService, report, modelState);
                var msg = string.Join("; ", GetModelStateErrors(modelState));
                if (!string.IsNullOrEmpty(msg)) throw new Exception(msg);
            }

            return report;
        }

        /// <summary>將來源 Hashtable 內容複製到目的 Hashtable </summary>
        /// <param name="src">來源 Hashtable</param>
        /// <param name="des">目的 Hashtable</param>
        public static void CopyHashtable(Hashtable src, Hashtable des)
        {
            if (src != null && des != null)
            {
                foreach (var key in src.Keys)
                {
                    if (des.ContainsKey(key)) des[key] = src[key];
                    else des.Add(key, src[key]);
                }
            }
        }

        /// <summary>
        /// 執行報表資料查詢。當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時，錯誤訊息會存放在 ModelState 內。
        /// 請在每次執行 QueryReportData() 後，立即檢查 ModelState.IsValid 屬性值以便處理非預期狀況。
        /// 註：若想要將「報表查詢結果資料」直接轉換成可操作的 Excel 物件時，請使用 TransReportDataToExcel() 方法。
        /// </summary>
        /// <param name="reportId">在 Report.config 檔案內的報表設定項目 id。</param>
        /// <param name="controller">ModelState 所在的 Controller 物件（用來存放當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時的錯誤訊息）。</param>
        /// <param name="params">（非必要）報表查詢參數值集合。</param>
        /// <param name="paramsTexts">（非必要）報表查詢參數的中文名稱對應字串集合</param>
        /// <returns>系統報表物件</returns>
        public static Turbo.ReportTK.Report QueryReportData(string reportId, Controller controller, Hashtable @params, Hashtable paramsTexts = null)
        {
            if (string.IsNullOrEmpty(reportId)) throw new ArgumentNullException("reportId");
            if (controller == null) throw new ArgumentNullException("controller");

            //從Config找報表資訊
            var reportService = new ReportService();
            var report = reportService.GetReportConfig().GetReport(reportId);
            if (report == null)
            {
                throw new NotImplementedException("找不到 Report '" + reportId + "' 的設定資訊!");
            }

            //20180810 修正傳入的 params 方法參數沒有作用問題
            if (@params != null)
            {
                //20180810 步驟一：先讓 params 方法參數內容讓系統報表輸入檢核程式早先查出不對之處
                report.QueryParms = new Hashtable();
                MyCommonUtil.CopyHashtable(@params, report.QueryParms);
            }

            //20180810 修正傳入的 paramsTexts 方法參數沒有作用問題
            if (paramsTexts != null)
            {
                //20180810 步驟一
                report.QueryParmTexts = new Hashtable();
                MyCommonUtil.CopyHashtable(paramsTexts, report.QueryParmTexts);
            }

            // 將 Request.Form 的報表查詢條件值轉成 SqlMap 查詢用的 Hashtable 參數
            reportService.ProcessQueryParameters(controller.Request, report);

            //20180810 步驟二：由於 reportService.ProcessQueryParameters() 執行完後會清空 report.QueryParms 集合
            //       因此要再複製一次 params 方法參數內容，否則會導致 QueryReportData() 查詢不到資料
            if (@params != null) MyCommonUtil.CopyHashtable(@params, report.QueryParms);
            if (paramsTexts != null) MyCommonUtil.CopyHashtable(paramsTexts, report.QueryParmTexts);

            //20180810 步驟三：取回報表資料
            MyCommonUtil.QueryReportData(reportService, report, controller.ModelState);
            return report;
        }

        /// <summary>
        /// 執行報表資料查詢。
        /// 當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時，在 MVC Controller 的 ModelState 物件會加入錯誤訊息。
        /// 請在每次執行 QueryReportData() 後，立即檢查 ModelState.IsValid 屬性值以便處理非預期狀況。
        /// 註：若想要將「報表查詢結果資料」直接轉換成可操作的 Excel 物件時，請使用 TransReportDataToExcel() 方法。
        /// </summary>
        /// <param name="reportId">在 Report.config 檔案內的報表設定項目 id。</param>
        /// <param name="controller">MVC 控制器物件。</param>
        /// <returns>系統報表物件</returns>
        public static Turbo.ReportTK.Report QueryReportData(string reportId, Controller controller)
        {
            if (string.IsNullOrEmpty(reportId)) throw new ArgumentNullException("reportId");
            if (controller == null) throw new ArgumentNullException("controller");

            //從Config找報表資訊
            var reportService = new ReportService();
            var report = reportService.GetReportConfig().GetReport(reportId);
            if (report == null)
            {
                throw new NotImplementedException("找不到 Report '" + reportId + "' 的設定資訊!");
            }

            // 將 Request.Form 的報表查詢條件值轉成 SqlMap 查詢用的 Hashtable 參數
            reportService.ProcessQueryParameters(controller.Request, report);
            QueryReportData(reportService, report, controller.ModelState);
            return report;
        }

        /// <summary>
        /// 執行報表資料查詢，並將查詢結果資料轉換成可操作的 Excel 物件。
        /// 當報表查詢條件無法通過輸入查檢或是報表資料查詢有異常狀況時，會在 MVC Controller 的 ModelState 物件加入錯誤訊息。
        /// 請在每次執行 QueryReportDataToExcel() 後，立即檢查 ModelState.IsValid 屬性值以便處理非預期狀況。
        /// </summary>
        /// <param name="reportId">在 Report.config 檔案內的報表設定項目 id。</param>
        /// <param name="controller">MVC 控制器物件。</param>
        /// <returns>ExcelPackge 型別物件</returns>
        public static ExcelPackage QueryReportDataToExcel(string reportId, Controller controller)
        {
            // 執行報表資料查詢
            var report = QueryReportData(reportId, controller);
            // 將查詢結果資料轉換成 Excel 操作物件
            return TransReportDataToExcel(report);
        }
        #endregion

        #region 取得在 MVC Controller/Action 之間使用的暫存參數資料值相關方法
        /// <summary>
        /// 取得在 MVC Controller/Action 之間使用的暫存參數值
        /// </summary>
        /// <param name="controller">MVC 控制器物件</param>
        /// <param name="paramName">參數名稱（區分大小寫英文）</param>
        /// <param name="returnIfParamAbsent">（非必要）若暫存參數不存在時所要傳回的值。</param>
        /// <returns></returns>
        public static string GetTempData(Controller controller, string paramName, string returnIfParamAbsent = "")
        {
            if (controller == null) throw new ArgumentNullException("controller");
            if (string.IsNullOrEmpty(paramName)) throw new ArgumentNullException("paramName");

            if (controller.TempData.ContainsKey(paramName))
            {
                return Convert.ToString(controller.TempData[paramName]);
            }
            else
            {
                return returnIfParamAbsent;
            }
        }

        /// <summary>
        /// 取得在 MVC Controller/Action 之間使用的暫存參數值
        /// </summary>
        /// <param name="controller">MVC 控制器物件</param>
        /// <param name="paramName">參數名稱（區分大小寫英文）</param>
        /// <param name="returnIfParamAbsent">（非必要）若暫存參數不存在時所要傳回的值。</param>
        /// <returns></returns>
        public static int GetTempData(Controller controller, string paramName, int returnIfParamAbsent)
        {
            if (controller == null) throw new ArgumentNullException("controller");
            if (string.IsNullOrEmpty(paramName)) throw new ArgumentNullException("paramName");

            if (controller.TempData.ContainsKey(paramName))
            {
                return Convert.ToInt32(controller.TempData[paramName]);
            }
            else
            {
                return returnIfParamAbsent;
            }
        }

        /// <summary>
        /// 取得在 MVC Controller/Action 之間使用的暫存參數值
        /// </summary>
        /// <param name="controller">MVC 控制器物件</param>
        /// <param name="paramName">參數名稱（區分大小寫英文）</param>
        /// <param name="returnIfParamAbsent">（非必要）若暫存參數不存在時所要傳回的值。</param>
        /// <returns></returns>
        public static object GetTempData(Controller controller, string paramName, object returnIfParamAbsent)
        {
            if (controller == null) throw new ArgumentNullException("controller");
            if (string.IsNullOrEmpty(paramName)) throw new ArgumentNullException("paramName");

            if (controller.TempData.ContainsKey(paramName))
            {
                return controller.TempData[paramName];
            }
            else
            {
                return returnIfParamAbsent;
            }
        }
        #endregion

        #region 民國日期與西元日期互轉方法
        /// <summary>將西元年轉換成民國年字串，並自動補滿成 3 碼民國年</summary>
        /// <param name="year">西元年</param>
        /// <returns></returns>
        public static string TransToTwYear(int year)
        {
            int yr = year - 1911;
            string s = yr.ToString();
            return (yr < 0) ? s : s.PadLeft(3, '0');
        }

        /// <summary>
        /// 將 DateTime 型別值轉換成只有西元日期部份字串（YYYY/MM/DD）
        /// </summary>
        /// <param name="date">西元日期時間</param>
        /// <param name="delimiter">（非必要）要顯示在結果字串內的年月日分隔字元。預設為 "/"</param>
        public static string TransToYYYYMMDD(DateTime? date, string delimiter = "/")
        {
            string ret = null;
            if (date != null && date.HasValue)
            {
                var sb = new StringBuilder();
                sb.Append(date.Value.Year.ToString().PadLeft(3, '0'));
                sb.Append(delimiter);
                sb.Append(date.Value.Month.ToString().PadLeft(2, '0'));
                sb.Append(delimiter);
                sb.Append(date.Value.Day.ToString().PadLeft(2, '0'));
                ret = sb.ToString();
            }
            return ret;
        }

        /// <summary>
        /// 將 DateTime 型別值轉換成僅有時間部份字串（hh:mm:ss）
        /// </summary>
        /// <param name="date">西元日期時間</param>
        /// <param name="needSeconds">（非必要）指示是否要包含秒數，（true: 包含，false: 不包含）。預設 true。</param>
        /// <param name="delimiter">（非必要）要顯示在結果字串內的時分秒分隔字元。預設 ":"</param>
        public static string TransToHHMMSS(DateTime? date, bool needSeconds = true, string delimiter = ":")
        {
            string ret = null;
            if (date != null && date.HasValue)
            {
                var sb = new StringBuilder();
                sb.Append(" ");
                sb.Append(date.Value.Hour.ToString().PadLeft(2, '0'));
                sb.Append(delimiter);
                sb.Append(date.Value.Minute.ToString().PadLeft(2, '0'));
                if (needSeconds == true)
                {
                    sb.Append(delimiter);
                    sb.Append(date.Value.Second.ToString().PadLeft(2, '0'));
                }
                ret = sb.ToString();
            }
            return ret;
        }

        /// <summary>
        /// 將 DateTime 型別值轉換成民國日期字串（YYY/MM/DD）
        /// </summary>
        /// <param name="date">西元日期時間</param>
        /// <param name="delimiter">（非必要）要顯示在結果字串內的年月日分隔字元。預設為 "/"</param>
        public static string TransToTwYYYMMDD(DateTime? date, string delimiter = "/")
        {
            string rtn = null;
            if (date != null && date.HasValue)
            {
                int yr = date.Value.Year - 1911;
                var sb = new StringBuilder();
                sb.Append(yr.ToString().PadLeft(3, '0'));
                sb.Append(delimiter);
                sb.Append(date.Value.Month.ToString().PadLeft(2, '0'));
                sb.Append(delimiter);
                sb.Append(date.Value.Day.ToString().PadLeft(2, '0'));
                rtn = sb.ToString();
            }
            return rtn;
        }

        /// <summary>
        /// 將 DateTime 型別值轉換成民國日期時間字串（YYY/MM/DD hh:mm，注意不包含秒數）
        /// </summary>
        /// <param name="date">西元日期時間</param>
        /// <param name="dateDelimiter">（非必要）要顯示在結果字串內的年月日分隔字元。預設 "/"</param>
        /// <param name="timeDelimiter">（非必要）要顯示在結果字串內的時分秒分隔字元。預設 ":"</param>
        /// <param name="halfDelimiter">（非必要）要顯示在結果字串內的日期與時間之間分隔字元。預設 " "</param>
        public static string TransToTwYYYMMDDHHMM(DateTime? date, string dateDelimiter = "/", string timeDelimiter = ":", string halfDelimiter = " ")
        {
            string rtn = null;
            if (date != null && date.HasValue)
            {
                int yr = date.Value.Year - 1911;
                var sb = new StringBuilder();
                //輸出日期
                sb.Append(yr.ToString().PadLeft(3, '0'));
                sb.Append(dateDelimiter);
                sb.Append(date.Value.Month.ToString().PadLeft(2, '0'));
                sb.Append(dateDelimiter);
                sb.Append(date.Value.Day.ToString().PadLeft(2, '0'));
                //輸出時間
                sb.Append(halfDelimiter);
                sb.Append(date.Value.Hour.ToString().PadLeft(2, '0'));
                sb.Append(timeDelimiter);
                sb.Append(date.Value.Minute.ToString().PadLeft(2, '0'));
                rtn = sb.ToString();
            }
            return rtn;
        }

        /// <summary>
        /// 將 DateTime 型別值轉換成民國日期時間字串（YYY/MM/DD hh:mm:ss）
        /// </summary>
        /// <param name="date">西元日期時間</param>
        /// <param name="needSeconds">（非必要）指示是否要包含秒數，（true: 包含，false: 不包含）。預設 true。</param>
        /// <param name="dateDelimiter">（非必要）要顯示在結果字串內的年月日分隔字元。預設 "/"</param>
        /// <param name="timeDelimiter">（非必要）要顯示在結果字串內的時分秒分隔字元。預設 ":"</param>
        /// <param name="halfDelimiter">（非必要）要顯示在結果字串內的日期與時間之間分隔字元。預設 " "</param>
        public static string TransToTwYYYMMDDHHMMSS(DateTime? date, bool needSeconds = true, string dateDelimiter = "/", string timeDelimiter = ":", string halfDelimiter = " ")
        {
            string rtn = null;
            if (date != null && date.HasValue)
            {
                int yr = date.Value.Year - 1911;
                var sb = new StringBuilder();
                //輸出日期
                sb.Append(yr.ToString().PadLeft(3, '0'));
                sb.Append(dateDelimiter);
                sb.Append(date.Value.Month.ToString().PadLeft(2, '0'));
                sb.Append(dateDelimiter);
                sb.Append(date.Value.Day.ToString().PadLeft(2, '0'));
                //輸出時間
                sb.Append(halfDelimiter);
                sb.Append(date.Value.Hour.ToString().PadLeft(2, '0'));
                sb.Append(timeDelimiter);
                sb.Append(date.Value.Minute.ToString().PadLeft(2, '0'));
                if (needSeconds == true)
                {
                    sb.Append(timeDelimiter);
                    sb.Append(date.Value.Second.ToString().PadLeft(2, '0'));
                }
                rtn = sb.ToString();
            }
            return rtn;
        }

        /// <summary>將 DateTime 型別值轉換成民國日期時間字串（YYYMMDDhhmmss）</summary>
        /// <param name="date">西元日期時間</param>
        /// <param name="needSeconds">（非必要）指示是否要包含秒數，（true: 包含，false: 不包含）。預設 true。</param>
        public static string TransToTwYYYMMDDHHMMSS2(DateTime? date, bool needSeconds = true)
        {
            string rtn = null;
            if (date != null && date.HasValue)
            {
                int yr = date.Value.Year - 1911;
                var sb = new StringBuilder();
                //輸出日期
                sb.Append(yr.ToString().PadLeft(3, '0'));
                sb.Append(date.Value.Month.ToString().PadLeft(2, '0'));
                sb.Append(date.Value.Day.ToString().PadLeft(2, '0'));
                //輸出時間
                sb.Append(date.Value.Hour.ToString().PadLeft(2, '0'));
                sb.Append(date.Value.Minute.ToString().PadLeft(2, '0'));
                if (needSeconds == true) sb.Append(date.Value.Second.ToString().PadLeft(2, '0'));
                rtn = sb.ToString();
            }
            return rtn;
        }

        /// <summary>
        /// 將西元日期字串（YYYY/MM/DD）轉換成 DateTime? 型別值
        /// </summary>
        /// <param name="adDate">西元年日期字串（YYYY/MM/DD）</param>
        /// <param name="delimiter">（非必要）要顯示在結果字串內的年月日分隔字元。預設 "/"</param>
        /// <returns></returns>
        public static DateTime? TransToDateTime(string adDate, string delimiter = "/")
        {
            DateTime? ret = null;
            if (!string.IsNullOrEmpty(adDate))
            {
                System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
                string format = string.Format("yyyy{0}MM{1}dd", delimiter, delimiter);
                ret = DateTime.ParseExact(adDate, format, provider);
            }
            return ret;
        }

        /// <summary>
        /// 將民國日期字串（YYY/MM/DD、YYY-MM-DD 或是 YYYMMDD）轉換成 DateTime? 型別值
        /// </summary>
        /// <param name="twDate">民國日期字串（YYY/MM/DD、YYY-MM-DD 或是 YYYMMDD）</param>
        /// <returns></returns>
        public static DateTime? TransTwToDateTime(string twDate)
        {
            try
            {
                DateTime? ret = null;
                if (!string.IsNullOrEmpty(twDate))
                {
                    string v = twDate.Replace("/", "").Replace("-", "");
                    if (v.Length >= 6)
                    {
                        int d = int.Parse(v.Substring(5, 2));
                        int m = int.Parse(v.Substring(3, 2));
                        int y = int.Parse(v.Substring(0, v.Length - 4)) + 1911;
                        ret = new DateTime?(new DateTime(y, m, d));
                    }
                    else
                    {
                        throw new Exception("民國日期字串長度必須在 6-9 個字之間。");
                    }
                }
                return ret;
            }
            catch
            {
                throw new Exception("無法將民國日期字串轉換成 DateTime? 型別值。");
            }
        }

        /// <summary>
        /// 將民國日期字串（YYY/MM/DD、YYY-MM-DD 或是 YYYMMDD）轉換成 DateTime 型別值
        /// </summary>
        /// <param name="twDate">民國日期字串（YYY/MM/DD、YYY-MM-DD 或是 YYYMMDD）</param>
        /// <returns></returns>
        public static DateTime TransTwToDateTime2(string twDate)
        {
            var dt = MyCommonUtil.TransTwToDateTime(twDate);
            if (dt.HasValue) return dt.Value;
            else throw new Exception("無法將民國日期字串轉換成 DateTime 型別值。");
        }

        /// <summary>
        /// 將時間字串（HH:MM:SS 或是 HHMMSS）轉換成 TimeSpan 型別值
        /// </summary>
        /// <param name="hhmmss">時間字串（HH:MM:SS 或是 HHMMSS）</param>
        /// <returns></returns>
        public static TimeSpan? TransHHMMSSToTimeSpan(string hhmmss)
        {
            try
            {
                TimeSpan? ret = null;
                if (!string.IsNullOrEmpty(hhmmss))
                {
                    string v = hhmmss.Replace(":", "");
                    if (v.Length == 6)
                    {
                        int s = int.Parse(v.Substring(4, 2));
                        int m = int.Parse(v.Substring(2, 2));
                        int h = int.Parse(v.Substring(0, 2));
                        ret = new TimeSpan?(new TimeSpan(h, m, s));
                    }
                    else
                    {
                        throw new Exception("時間字串長度必須在 6-8 個字之間。");
                    }
                }
                return ret;
            }
            catch
            {
                throw new Exception("無法將時間字串轉換成 TimeSpan? 型別值。");
            }
        }

        /// <summary>將西元日期轉換成「民國 yyy 年 MM 月 dd 日」格式的民國日期字串</summary>
        public static string TransDateTimeTw(DateTime date)
        {
            if (date == DateTime.MinValue) return "";
            else return date.ToString("民國 yyy 年 MM 月 dd 日", new System.Globalization.CultureInfo("zh-TW"));
        }

        /// <summary>將 7 碼民國日期字串轉換成「有斜線分隔字元 YYY/MM/DD」或是「有年、月、日分隔字」的民國日期字串</summary>
        /// <param name="twDate">民國年日期（範例：1070815）</param>
        /// <param name="type">（非必要）分隔字元類型 ("": 斜線分隔字元，S: 年月日分隔字)</param>
        /// <param name="prefix">（非必要）要放置在民國日期字串前面的文字（例如：民國）</param>
        public static string TransDateTimeTw(string twDate, string type = "", string prefix = "")
        {
            if (string.IsNullOrEmpty(twDate)) return "";
            if (twDate.Length != 7) throw new ArgumentException("民國日期必須是 7 碼純數字。");

            string yr = twDate.SubstringTo(0, 3);
            string mn = twDate.SubstringTo(3, 2);
            string dy = twDate.SubstringTo(5, 2);

            var sb = new StringBuilder();
            sb.Append(prefix);
            sb.Append(yr);
            if (type == "S")
            {
                sb.Append("年");
                sb.Append(mn);
                sb.Append("月");
                sb.Append(dy);
                sb.Append("日");
            }
            else
            {
                sb.Append("/");
                sb.Append(mn);
                sb.Append("/");
                sb.Append(dy);
            }

            return sb.ToString();
        }
        #endregion

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
            var dt = MyCommonUtil.GetLastDateOfMonth(date);
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
            var dt = MyCommonUtil.GetLastDateOfMonth(year, month);
            var sb = new StringBuilder();
            sb.Append((dt.Year - 1911).ToString().PadLeft(3, '0'));
            sb.Append(dt.Month.ToString().PadLeft(2, '0'));
            sb.Append(dt.Day.ToString().PadLeft(2, '0'));
            return sb.ToString();
        }
        #endregion

        #region CSF 電腦科學基金會提供的解密元件方法
        /// <summary>解密電腦科學基金會的資料檔案</summary>
        /// <param name="filePath">資料檔案路徑</param>
        /// <returns></returns>
        public static void DecryptCSFFile(string filePath)
        {
            //請將 CSF 電腦科學基金會提供給技檢 1.0 使用的解密元件 (CSF_Encrypt.dll) 以 COM+ 元件服務安裝。
            //請將專案檔 (EECOnline.csproj) 的 References 加入 "Interop.即測即評加解密模組.dll" 引用。
            //完成上述動作之後，這樣子才能在 64 位元環境網站程式內成功執行古老的 32 位元 dll 程式。

            //即測即評加解密模組.clsEncryp Encrypt = new clsEncryp();
            var com = new clsEncryp();
            //解密資料檔案（沒錯，它的方法名稱就是 Encrypt，真是○○XX，造物者當年唸英文沒認真在學...）。
            com.EncryptFile(filePath);
        }
        #endregion

        #region 產生隨機碼字串 方法
        /// <summary>
        /// 產生指定長度及複雜度的隨機碼字串
        /// <para>此功能是基於 RNGCryptoServiceProvider 而非傳統的 Random 值,
        /// <see cref="System.Security.Cryptography.RNGCryptoServiceProvider"/>
        /// </para>
        /// </summary>
        /// <param name="len">要產生的字元數長度</param>
        /// <param name="complexity">複雜度: 0.全數字, 1.全大寫英文字(預設值), 2.大寫英文字+數字, 3.大小寫英文字+數字</param>
        /// <returns></returns>
        public static string GetRandomString(int length, int complexity = 1)
        {
            // 用來產生 RandomString 的字元陣列: 0-9
            char[] NUMERIC = "1234567890".ToCharArray();
            // 用來產生 RandomString 的字元陣列: A-Z
            char[] ALPHA_CAPS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            // 用來產生 RandomString 的字元陣列: A-Z, 1-9
            char[] ALPHANUMERIC_CAPS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789".ToCharArray();
            // 用來產生 RandomString 的字元陣列: A-Z, a-z, 1-9
            char[] ALPHANUMERIC_CIS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopgrstuvwxyz123456789".ToCharArray();

            char[] chars = ALPHA_CAPS;
            if (complexity == 3)
            {
                chars = ALPHANUMERIC_CIS;
            }
            else if (complexity == 2)
            {
                chars = ALPHANUMERIC_CAPS;
            }
            else if (complexity == 0)
            {
                chars = NUMERIC;
            }

            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            string s = "";
            for (int i = 0; i < length; i++)
            {
                byte[] intBytes = new byte[4];
                rand.GetBytes(intBytes);
                uint randomInt = BitConverter.ToUInt32(intBytes, 0);
                s += chars[randomInt % chars.Length];
            }
            return s;
        }

        #endregion

        #region 依據檢定類別、單位層級、學科術科類別代碼決定適當的「XX單位」的顯示字串
        /// <summary>
        /// 依據檢定類別、單位層級、學科術科類別代碼決定適當的「XX單位」的顯示字串
        /// </summary>
        /// <param name="textToSearch">要被搜尋更換的單位欄位原始顯示名稱</param>
        /// <param name="examkind">檢定類別</param>
        /// <param name="district">單位層級(1:主辦單位,2:報名單位,3:學術科承辦單位,其他值:其他層級)</param>
        /// <param name="areatype">學科術科類別代碼(W:學科,O:術科,"":不需要)</param>
        public static string DetermineOPCDTitle(string textToSearch, string examkind, string district, string areatype = "")
        {
            if (string.IsNullOrEmpty(textToSearch)) throw new Exception("textToSearch");
            if (string.IsNullOrEmpty(examkind)) throw new Exception("examkind");
            if (string.IsNullOrEmpty(district)) throw new Exception("district");
            if (string.IsNullOrEmpty(areatype) && district == "3") throw new Exception("當DISTRICT=3時需要指定areatype");

            string OPCDTITLE = "";
            switch (district)
            {
                case "1":
                    switch (examkind)
                    {
                        case "1":
                            OPCDTITLE = "主辦單位";
                            break;
                        case "3":
                            OPCDTITLE = "單位";
                            break;
                        default:
                            OPCDTITLE = "承辦單位";
                            break;
                    }
                    break;
                case "2":
                    switch (examkind)
                    {
                        case "1":
                            OPCDTITLE = "主辦單位";
                            break;
                        case "3":
                            OPCDTITLE = "分召學校";
                            break;
                        default:
                            OPCDTITLE = "辦理單位";
                            break;
                    }
                    break;
                case "3":
                    switch (examkind)
                    {
                        case "1":
                        case "3":
                            OPCDTITLE = ("O".Equals(areatype.ToUpper()) ? "術科單位" : "承辦單位");
                            break;
                        default:
                            OPCDTITLE = "承辦單位";
                            break;
                    }
                    break;
                default:
                    OPCDTITLE = "單位";
                    break;
            }
            return OPCDTITLE;
        }

        /// <summary>
        /// 更換在ModelState內的單位欄位錯誤訊息的欄位顯示名稱
        /// </summary>
        /// <param name="modelState">ModelState</param>
        /// <param name="fieldKeyInModelState">ModelState內的單位欄位鍵名</param>
        /// <param name="textToSearch">要被搜尋更換的單位欄位原始顯示名稱</param>
        /// <param name="examkind">檢定類別</param>
        /// <param name="district">單位層級(1:主辦單位,2:報名單位,3:學術科承辦單位,其他值:其他層級)</param>
        /// <param name="areatype">學科術科類別代碼(W:學科,O:術科,"":不需要)</param>
        public static void DetermineOPCDTitle(ModelStateDictionary modelState, string fieldKeyInModelState, string textToSearch, string examkind, string district, string areatype = "")
        {
            if (modelState == null) throw new Exception("modelState");
            if (string.IsNullOrWhiteSpace(fieldKeyInModelState)) throw new Exception("fieldKeyInModelState");
            if (string.IsNullOrEmpty(textToSearch)) throw new Exception("textToSearch");
            if (string.IsNullOrEmpty(examkind)) throw new Exception("examkind");
            if (string.IsNullOrEmpty(district)) throw new Exception("district");
            if (string.IsNullOrEmpty(areatype) && district == "3") throw new Exception("當DISTRICT=3時需要指定areatype");
            var obj = modelState[fieldKeyInModelState];
            if (obj != null && obj.Errors.Count > 0)
            {
                string OPCDTITLE = MyCommonUtil.DetermineOPCDTitle(textToSearch, examkind, district, areatype);
                string Errors = obj.Errors[0].ErrorMessage;
                obj.Errors.Clear();
                obj.Errors.Add(Errors.Replace(textToSearch, OPCDTITLE));
            }
        }
        #endregion

        #region 產生可讓瀏覽器自動下載的 MVC 檔案回應結果物件相關方法
        /// <summary>組成系統一致性的報表下載檔案名稱（檔案名稱格式固定為 "報表ID"+"報表名稱"+"-"+"目前日期時間"+"延伸檔名"）。</summary>
        /// <param name="report">系統報表結構物件</param>
        /// <param name="extensionName">檔案的延伸檔名（範例：.doc、.docx、.pdf、.xls、.xlsx）</param>
        /// <returns></returns>
        private static string BuildTurboReportFileName(Turbo.ReportTK.Report report, string extensionName)
        {
            var sb = new StringBuilder();
            sb.Append(report.Id);
            sb.Append(report.Name);
            sb.Append("-");
            sb.Append(HelperUtil.DateTimeToLongTwString(DateTime.Now));
            sb.Append(extensionName);
            return sb.ToString();
        }

        /// <summary>建立可讓瀏覽器自動下載的 Excel 2007 格式系統報表回應結果。</summary>
        /// <param name="report">系統報表結構物件</param>
        /// <param name="fileContent">報表檔案內容</param>
        /// <returns></returns>
        public static FileContentResult BuildTurboReportResultXlsx(Turbo.ReportTK.Report report, byte[] fileContent)
        {
            if (report == null) throw new ArgumentNullException("report");
            if (fileContent == null) throw new ArgumentNullException("fileContent");
            var contentType = HelperUtil.XLSXContentType;
            var ret = new FileContentResult(fileContent, contentType);
            ret.FileDownloadName = MyCommonUtil.BuildTurboReportFileName(report, ".xlsx");
            return ret;
        }

        /// <summary>建立可讓瀏覽器自動下載的 Excel 2007 格式系統報表回應結果。</summary>
        /// <param name="report">系統報表結構物件</param>
        /// <param name="fileContent">報表檔案內容</param>
        /// <returns></returns>
        public static FileContentResult BuildTurboReportResultXlsx(Turbo.ReportTK.Report report, ExcelPackage fileContent)
        {
            if (report == null) throw new ArgumentNullException("report");
            if (fileContent == null) throw new ArgumentNullException("fileContent");
            var contentType = HelperUtil.XLSXContentType;
            var ret = new FileContentResult(fileContent.GetAsByteArray(), contentType);
            ret.FileDownloadName = MyCommonUtil.BuildTurboReportFileName(report, ".xlsx");
            return ret;
        }
        #endregion

        #region 取得兩數相除之後的結果百分比字串（四捨五入、取到指定位數小數）相關方法
        /// <summary>傳回兩數相除之後的結果百分比結果字串。若除數等於 0 時一律傳回空字串，否則傳回百分比字串。</summary>
        /// <param name="v1">數值 1（被除數）</param>
        /// <param name="v2">數值 2（除數）</param>
        /// <param name="decimals">要取到的小數位數</param>
        /// <param name="need4out5in">（非必要）是否需要四捨五入。(true: 需要四捨五入，false: 不要四捨五入)</param>
        /// <param name="tailText">（非必要）百分比字串結尾字元。預設值為 % 符號字元。</param>
        /// <param name="decimalPadding">（非必要）小數部份補齊位數。輸入 0 表示不需要補齊位數。</param>
        /// <returns></returns>
        public static string ComputePercent(long v1, long v2, int decimals, bool need4out5in = false, string tailText = "%", int decimalPadding = 0)
        {
            if (v2 == 0) return "";
            else
            {
                var r = decimal.Divide(v1, v2) * 100;
                var sb = new System.Text.StringBuilder();
                if (need4out5in)
                {
                    r = Math.Round(r, decimals, MidpointRounding.AwayFromZero);
                    var raw = r.ToString();
                    //若不需要小數部份補齊位數時
                    if (decimalPadding <= 0) sb.Append(raw);
                    else
                    {
                        int i = raw.IndexOf(".");
                        if (i >= 0)
                        {
                            var s = raw.Substring(i + 1);  //取得小數部份（不包含小數點）
                            if (s.Length < decimalPadding)
                            {
                                sb.Append(raw);
                                sb.Append("0".PadLeft(decimalPadding - s.Length, '0'));
                            }
                            else
                            {
                                sb.Append(raw.Substring(0, i + decimalPadding));
                            }
                        }
                        else
                        {
                            sb.Append(raw);
                            sb.Append(".");
                            sb.Append("0".PadLeft(decimalPadding, '0'));
                        }
                    }
                }
                else
                {
                    if (decimals <= 0)
                    {
                        sb.Append(Math.Floor(r).ToString());
                        //若需要小數部份補齊位數時
                        if (decimalPadding > 0)
                        {
                            sb.Append(".");
                            sb.Append("0".PadLeft(decimalPadding, '0'));
                        }
                    }
                    else
                    {
                        var partI = Math.Floor(r);           //整數部份
                        var partD = (r - partI).ToString();  //小數部份
                        sb.Append(partI.ToString());

                        int i = partD.IndexOf(".");
                        if (i >= 0)
                        {
                            var s = (partD.Length > (decimals + 2)) ? partD.Substring(i + 1, decimals) : partD.Substring(i + 1);

                            //若需要小數部份補齊位數時
                            if (decimalPadding > 0)
                            {
                                if (s.Length < decimalPadding) s = s.PadLeft(decimalPadding, '0');
                            }

                            if (s.Length > 0)
                            {
                                sb.Append(".");
                                sb.Append(s);
                            }
                        }
                        else
                        {
                            //若需要小數部份補齊位數時
                            if (decimalPadding > 0)
                            {
                                sb.Append(".");
                                sb.Append("0".PadLeft(decimalPadding, '0'));
                            }
                        }
                    }
                }
                sb.Append(tailText);
                return sb.ToString();
            }
        }
        #endregion

        #region 將 HTTP 用戶端上傳檔案轉換成 HttpPostedFileWrapper 型別物件
        /// <summary>將 HttpPostedFileBase 物件轉成 HttpPostedFileWrapper 物件</summary>
        /// <param name="postedFile">來源 HttpPostedFileBase 物件</param>
        /// <returns></returns>
        private static HttpPostedFileWrapper TransPostedFileWrapper(HttpPostedFileBase postedFile)
        {
            if (postedFile == null) { throw new ArgumentNullException("postedFile"); }
            var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            var obj = (HttpPostedFile)constructorInfo.Invoke(new object[] { postedFile.FileName, postedFile.ContentType, postedFile.InputStream });
            return new HttpPostedFileWrapper(obj);
        }

        /// <summary>取得 HTTP 用戶端上傳檔案，並將它轉成 HttpPostedFileWrapper 物件。若無法取得上傳檔案時會傳回 null。</summary>
        /// <param name="mvcController">當時的 MVC 控制器物件</param>
        /// <param name="postedFileIndex">（非必要）在 HTTP Request 上傳檔案集合內的檔案項目索引。預設值為 0</param>
        /// <returns></returns>
        public static HttpPostedFileWrapper GetHttpPostedFile(Controller mvcController, int postedFileIndex = 0)
        {
            if (mvcController == null) throw new ArgumentNullException("mvcController");
            if (postedFileIndex < 0) throw new ArgumentException("postedFileIndex 參數值不可小於 0。");
            if (mvcController.Request != null && mvcController.Request.Files != null && mvcController.Request.Files.Count > 0)
            {
                var files = mvcController.Request.Files;
                if (postedFileIndex >= files.Count)
                {
                    string msg = string.Concat("postedFileIndex 索引值不可大於已接收檔案數量 ", files.Count.ToString(), "。");
                    throw new InvalidOperationException(msg);
                }
                else
                {
                    var file = files[postedFileIndex];
                    return MyCommonUtil.TransPostedFileWrapper(file);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>取得 HTTP 用戶端上傳檔案，並將它轉成 HttpPostedFileWrapper 物件。若無法取得上傳檔案時會傳回 null。</summary>
        /// <param name="mvcController">當時的 MVC 控制器物件</param>
        /// <param name="postedFileKey">在 HTTP Request 上傳檔案集合內的檔案項目鍵名。</param>
        /// <returns></returns>
        public static HttpPostedFileWrapper GetHttpPostedFile(Controller mvcController, string postedFileKey)
        {
            if (mvcController == null) throw new ArgumentNullException("mvcController");
            if (string.IsNullOrEmpty(postedFileKey)) throw new ArgumentException("postedFileIndex 參數值不可為空字串。");
            if (mvcController.Request != null && mvcController.Request.Files != null && mvcController.Request.Files.Count > 0)
            {
                var files = mvcController.Request.Files;
                var file = files[postedFileKey];
                if (file != null) return MyCommonUtil.TransPostedFileWrapper(file);
                else
                {
                    string msg = string.Concat("在使用者上傳檔案內找不到檔案項目鍵名為 \"", postedFileKey, "\" 的檔案。");
                    throw new InvalidOperationException(msg);
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region AES加密
        /// <summary>
        /// 以指定的 key(clientSecret) 及 iv(cbcIv) 對 plainText(通常是身分證號) 進行加密
        /// <para>這是跟 java AES/CBC/PKCS5PADDING 等效的加密方式</para>
        /// <para>回傳加密後資料經 Base64 encoding 的字串</para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string AESEncrypt(string key, string iv, string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            using (AesManaged aes = new AesManaged())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);

                if (key.Length == 16)
                {
                    // AES256 key 為 256bits (32 bytes)
                    key = key + key;
                }

                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        byte[] bytes = (byte[])ms.ToArray();
                        return Convert.ToBase64String(bytes);
                    }
                }
            }
        }

        /// <summary>
        /// 以指定的 key(clientSecret) 及 iv(cbcIv) 對 encryptStr 進行解密
        /// </summary>
        /// <param name="key">加密金鑰</param>
        /// <param name="iv">加密向量</param>
        /// <param name="encryptStr"></param>
        /// <returns></returns>
        public string AESDecrypt(string key, string iv, string encryptStr)
        {
            if (string.IsNullOrEmpty(encryptStr))
                return null;

            return AESDecrypt(key, iv, Encoding.UTF8.GetBytes(encryptStr));
        }

        /// <summary>
        /// 以指定的 key(clientSecret) 及 iv(cbcIv) 對 encryptStr 進行解密
        /// </summary>
        /// <param name="key">加密金鑰</param>
        /// <param name="iv">加密向量</param>
        /// <param name="encryptStr"></param>
        /// <returns></returns>
        public string AESDecrypt(string key, string iv, byte[] encryptData)
        {
            if (encryptData == null || encryptData.Length == 0)
                return null;

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptData, 0, encryptData.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
        #endregion
    }
}