using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EECOnline.Services;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML 行政區 輸入框產生輔助方法類別
    /// </summary>
    public static class ZIP_COExtension
    {
        /// <summary>HTML 行政區單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ZIP_COForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            Expression<Func<TModel, object>> expressionTEXT,
            bool IsReadOnly = false,
            object htmlAttributes = null, bool enabled = true)
        {
            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionTEXT);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionTEXT, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = templateInfo.GetFullHtmlFieldId(propertyName_text);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-normal", size = 10, maxlength = 5, placeholder = "郵遞區號" }, IsReadOnly).ToHtmlString());            
           
            if (!IsReadOnly)
            {
                sb.Append("<span class='input-group-btn mr-2'>");
                sb.Append("<button type='button' class='btn btn-info' onclick='do" + EXPname + "Select()'>");
                sb.Append("<i class='fa fa-search' aria-hidden='true'></i>");
                sb.Append("</button>");
                sb.Append("</span>");
            }
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT, new { size = "40", @class = "form-normal", placeholder = "郵遞區號詳細資料" }, true).ToHtmlString());
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");
            //先取得資料代碼對應的顯示名稱，再將顯示名稱設定到唯讀文字框。
            //data 參數物件必須要有 url、msg、arg、box 四個屬性。
            sb_script.Append("function loadDialog" + EXPname + "CodeMapName(data) {");
            sb_script.Append("ajaxLoadMore(data.url, data.arg, function(resp) {");
            sb_script.Append("if (resp === undefined) data.box.val('');");
            sb_script.Append("else {");
            sb_script.Append("if (resp.data != '') data.box.val(resp.data);");
            //sb_script.Append("else {");
            //sb_script.Append("data.box.val('');");
            //sb_script.Append("blockAlert(data.msg);");
            //sb_script.Append("}");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("};");
            //顯示行政區代碼(ZIP_CO)資料選取對話框
            sb_script.Append("function do" + EXPname + "Select() {");
            sb_script.Append("var url = '" + url.Action("Index", "ZIP_CO", new { area = "Share" }) + "';");
            sb_script.Append("var title = '郵遞區號查詢';");
            sb_script.Append("popupWindow({ 'width': 900 }, url, title, null, function (retData, doc) {");
            sb_script.Append("if (retData != null)");
            sb_script.Append("{");
            sb_script.Append("$('#" + propertyId + "').val(retData.Id);");
            sb_script.Append("$('#" + propertyId_text + "').val(retData.Name);");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("}");

            //取得行政區代碼對應的規格名稱。
            sb_script.Append("function do" + EXPname + "NameGet() {");
            sb_script.Append("var code = $('#" + propertyId + "');");
            sb_script.Append("var data = { 'url': '" + url.Action("GetZIP_CO", "Ajax", new { area = "" }) + "',");
            sb_script.Append("'msg': '查無該郵遞區號詳細資料!!',");
            sb_script.Append("'arg': { 'CODE': code.val().toUpperCase() },");
            sb_script.Append("'box': $('#" + propertyId_text + "') };");
            sb_script.Append(" if ($.trim(data.arg.CODE) == '') data.box.val('');");
            sb_script.Append(" else {");
            sb_script.Append("code.val(data.arg.CODE);");
            sb_script.Append("loadDialog" + EXPname + "CodeMapName(data);");
            sb_script.Append("}");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>HTML 行政區加完整地址單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionInput">輸入欄位2(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ZIP_CO_FULLForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            Expression<Func<TModel, object>> expressionTEXT,
            Expression<Func<TModel, object>> expressionInput,
            bool IsReadOnly = false,
            object htmlAttributes = null, bool enabled = true)
        {
            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionTEXT);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionTEXT, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = templateInfo.GetFullHtmlFieldId(propertyName_text);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-normal", size = 10, maxlength = 5, placeholder = "郵遞區號" }, IsReadOnly).ToHtmlString());

            if (!IsReadOnly)
            {
                sb.Append("<span class='input-group-btn mr-2'>");
                sb.Append("<button type='button' class='btn btn-info' onclick='do" + EXPname + "Select()'>");
                sb.Append("<i class='fa fa-search' aria-hidden='true'></i>");
                sb.Append("</button>");
                sb.Append("</span>");
            }
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT, new { size = "40", @class = "form-normal", placeholder = "郵遞區號詳細資料" }, true).ToHtmlString()+"  ");
            sb.Append(htmlHelper.TextBoxFor(expressionInput, new { size = "40", @class = "form-normal" }, IsReadOnly).ToHtmlString());
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");
            //先取得資料代碼對應的顯示名稱，再將顯示名稱設定到唯讀文字框。
            //data 參數物件必須要有 url、msg、arg、box 四個屬性。
            sb_script.Append("function loadDialog" + EXPname + "CodeMapName(data) {");
            sb_script.Append("ajaxLoadMore(data.url, data.arg, function(resp) {");
            sb_script.Append("if (resp === undefined) data.box.val('');");
            sb_script.Append("else {");
            sb_script.Append("if (resp.data != '') data.box.val(resp.data);");
            //sb_script.Append("else {");
            //sb_script.Append("data.box.val('');");
            //sb_script.Append("blockAlert(data.msg);");
            //sb_script.Append("}");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("};");
            //顯示行政區代碼(ZIP_CO)資料選取對話框
            sb_script.Append("function do" + EXPname + "Select() {");
            sb_script.Append("var url = '" + url.Action("Index", "ZIP_CO", new { area = "Share" }) + "';");
            sb_script.Append("var title = '行政區代碼查詢';");
            sb_script.Append("popupWindow({ 'width': 900 }, url, title, null, function (retData, doc) {");
            sb_script.Append("if (retData != null)");
            sb_script.Append("{");
            sb_script.Append("$('#" + propertyId + "').val(retData.Id);");
            sb_script.Append("$('#" + propertyId_text + "').val(retData.Name);");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("}");

            //取得行政區代碼對應的規格名稱。
            sb_script.Append("function do" + EXPname + "NameGet() {");
            sb_script.Append("var code = $('#" + propertyId + "');");
            sb_script.Append("var data = { 'url': '" + url.Action("GetZIP_CO", "Ajax", new { area = "" }) + "',");
            sb_script.Append("'msg': '查無該行政區資訊!!',");
            sb_script.Append("'arg': { 'CODE': code.val().toUpperCase() },");
            sb_script.Append("'box': $('#" + propertyId_text + "') };");
            sb_script.Append(" if ($.trim(data.arg.CODE) == '') data.box.val('');");
            sb_script.Append(" else {");
            sb_script.Append("code.val(data.arg.CODE);");
            sb_script.Append("loadDialog" + EXPname + "CodeMapName(data);");
            sb_script.Append("}");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}