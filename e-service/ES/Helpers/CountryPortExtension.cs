using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ES.Services;
using System.Text.RegularExpressions;

namespace ES.Helpers
{
    /// <summary>
    /// HTML 港口-岸口 輸入框產生輔助方法類別
    /// </summary>
    public static class CountryPortExtension
    {
        /// <summary>HTML 港口-岸口 單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString CountryPortForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            IList<SelectListItem> expressionList,
            Expression<Func<TModel, object>> expressionPort,
            IList<SelectListItem> expressionPortList,
            bool IsReadOnly = false,
            object htmlAttributes = null
           )
        {

            var DisabledString = IsReadOnly ? "pointer-events:none;background:#DDDDDD" :"" ;

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);
            
            //HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionPort);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionPort, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata_text.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = templateInfo.GetFullHtmlFieldId(propertyName_text);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.DropDownListFor(expression, expressionList, new { @class = "form-normal form-inline",style= DisabledString }).ToHtmlString());
            sb.Append(htmlHelper.DropDownListFor(expressionPort, expressionPortList, new { @class = "form-normal form-inline", style = DisabledString }).ToHtmlString());
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#"+ propertyId + "').change(function () {");
            sb_script.Append("GetHarbor"+ EXPname + "List();");
            sb_script.Append("});");
            sb_script.Append("GetHarbor" + EXPname + "List();");
            sb_script.Append("});");

            //組裝Tel
            sb_script.Append("  function GetHarbor"+ EXPname + "List() {");
            sb_script.Append(" var port = $('#"+ propertyId + "'); ");
            sb_script.Append("var data = {");
            sb_script.Append("'url': '/Ajax/GetHarborList', 'msg': '查無該港口!!', 'arg': { 'port': port.val() }, 'result': $('#" + propertyId + "')");
            sb_script.Append("};");

            sb_script.Append("if ($.trim(data.arg.port) == '')");
            sb_script.Append("data.result.val('');");
            sb_script.Append("else {");
            sb_script.Append("port.val(data.arg.port);");
            sb_script.Append("loadHarbor"+ EXPname + "List(data);");
            sb_script.Append("}");
            sb_script.Append("};");

            sb_script.Append("function loadHarbor"+ EXPname + "List(data) {");
            sb_script.Append("ajaxLoadMore(data.url, data.arg, function (resp) {");
            sb_script.Append("if (resp === undefined) {");
            sb_script.Append("data.result.val('');");
            sb_script.Append("}");
            sb_script.Append("else {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("$('#"+ propertyId_text + "').empty();");
            sb_script.Append("$.each(resp.data, function (i, item) {");
            sb_script.Append("$('#"+ propertyId_text + "').append('<option value=' + item.CODE + '>' + item.TEXT + '</option>');");
            sb_script.Append("});");
            sb_script.Append("$('#" + propertyId_text + "').val('"+ value_text.ToUpper() + "')");
            sb_script.Append("}");
            sb_script.Append("else {");
            sb_script.Append("data.result.val('');");
            sb_script.Append("blockAlert(data.msg);");
            sb_script.Append(" }");
            sb_script.Append("}");
            sb_script.Append("});");
            sb_script.Append("};");
            



            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }

    }
}