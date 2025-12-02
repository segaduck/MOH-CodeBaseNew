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
    /// HTML 地址 輸入框產生輔助方法類別
    /// </summary>
    public static class AddrExtension
    {
        /// <summary>HTML 地址 單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString AddrForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            Expression<Func<TModel, object>> expressionTEXT,
            Expression<Func<TModel, object>> expressionDetail,
             bool IsReadOnly = false,
            object htmlAttributes = null
           )
        {

            var DisabledString = IsReadOnly ? "pointer-events:none;background:#EEEEEE" :"" ;

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name.Replace("[","").Replace("]", "").Replace(".", "").Replace("_", "");
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(name);
            
            //HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionTEXT);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionTEXT, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = templateInfo.GetFullHtmlFieldId(name_text);

            //HTML 標籤的 id 與 name 屬性值
            var name_detail = ExpressionHelper.GetExpressionText(expressionDetail);
            var metadata_detail = ModelMetadata.FromLambdaExpression(expressionDetail, htmlHelper.ViewData);
            var templateInfo_detail = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_detail = Convert.ToString(metadata.Model).ToLower();

            var propertyName_detail = templateInfo.GetFullHtmlFieldName(name_detail);
            var propertyId_detail = templateInfo.GetFullHtmlFieldId(name_detail);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-10 form-control", size = 10, maxlength = 10, placeholder = "郵遞區號" }, IsReadOnly).ToHtmlString());
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT, new { size = "20", @class = "form-normal", placeholder = "郵遞區號名稱" }, true).ToHtmlString());
            //if (!IsReadOnly)
            //{
                sb.Append("<span class='input-group-btn'>");
                sb.Append("<button type='button' class='btn btn-info' onclick='do" + EXPname + "Select()'> ");
                sb.Append("<i class='fa fa-search' aria-hidden='true'></i>");
                sb.Append("</button>");
                sb.Append("</span>");
            //}
            sb.Append("</div>");
            sb.Append(htmlHelper.TextBoxFor(expressionDetail, new { size = "59", maxlength = "64", @class = "form-normal", placeholder = "請輸入地址" }, IsReadOnly).ToHtmlString());

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");

            //組裝Tel
            sb_script.Append("  function loadDialog"+ EXPname + "CodeMapName(data) {");
            sb_script.Append("ajaxLoadMore(data.url,");
            sb_script.Append("data.arg,");
            sb_script.Append("function (resp) {");
            sb_script.Append("if (resp === undefined) data.box.val('');");
            sb_script.Append("else {");
            sb_script.Append("if (resp.data != '') data.box.val(resp.data);");
            sb_script.Append("else {");
            sb_script.Append("data.box.val('');");
            sb_script.Append("data.add.val('');");
            sb_script.Append(" blockAlert(data.msg);");
            sb_script.Append(" }");
            sb_script.Append(" }");
            sb_script.Append(" });");
            sb_script.Append("  };");

            sb_script.Append("function do" + EXPname + "Select() {");
            sb_script.Append("  var url = '/ZIP_CO';");
            sb_script.Append("  var title = '郵遞區號查詢';");
            sb_script.Append("  popupWindow({ 'width': 900 },");
            sb_script.Append("   url,");
            sb_script.Append("  title,");
            sb_script.Append("  null,");
            sb_script.Append("  function (retData, doc) {");
            sb_script.Append("   if (retData != null) {");
            sb_script.Append("   $('#"+ propertyId + "').val(retData.Id);");
            sb_script.Append("  $('#" + propertyId_text + "').val(retData.Name);");
            sb_script.Append("  $('#" + propertyId_detail + "').val(retData.Name);");
            sb_script.Append("  }");
            sb_script.Append(" });");
            sb_script.Append("  }");

            sb_script.Append("  function do"+ EXPname + "NameGet() {");
            sb_script.Append("   var code = $('#"+ propertyId + "');");
            sb_script.Append("  console.log('"+ propertyId + "');");
            sb_script.Append("   var data = {");
            sb_script.Append("  'url': '/Ajax/GetCityTown',");
            sb_script.Append("  'msg': '查無該郵遞區號!!',");
            sb_script.Append("  'arg': { 'CODE': code.val().toUpperCase() },");
            sb_script.Append("  'box': $('#"+ propertyId_text+ "'),");
            sb_script.Append("  'add': $('#"+ propertyId + "')");
            sb_script.Append("   };");
            sb_script.Append("  if ($.trim(data.arg.CODE) == '') data.box.val('');");
            sb_script.Append("  else {");
            sb_script.Append("  code.val(data.arg.CODE);");
            sb_script.Append("   loadDialog"+ EXPname + "CodeMapName(data);");
            sb_script.Append("  }");
            sb_script.Append("  }");



            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }


        /// <summary>HTML 地址 單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString AddrDynamicForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            Expression<Func<TModel, object>> expressionTEXT,
            Expression<Func<TModel, object>> expressionDetail,
            string DynamicName,
             bool IsReadOnly = false,
            object htmlAttributes = null
           )
        {

            var DisabledString = IsReadOnly ? "pointer-events:none;background:#DDDDDD" : "";

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name.Replace("[", "").Replace("]", "").Replace(".", "").Replace("_", "");
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = DynamicName + "_" + templateInfo.GetFullHtmlFieldId(name);

            //HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionTEXT);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionTEXT, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = DynamicName + "_" + templateInfo.GetFullHtmlFieldId(name_text);

            //HTML 標籤的 id 與 name 屬性值
            var name_detail = ExpressionHelper.GetExpressionText(expressionDetail);
            var metadata_detail = ModelMetadata.FromLambdaExpression(expressionDetail, htmlHelper.ViewData);
            var templateInfo_detail = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_detail = Convert.ToString(metadata.Model).ToLower();

            var propertyName_detail = templateInfo.GetFullHtmlFieldName(name_detail);
            var propertyId_detail = DynamicName + "_" +  templateInfo.GetFullHtmlFieldId(name_detail);


            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-10 form-control", size = 10, maxlength = 10, placeholder = "郵遞區號" }, IsReadOnly).ToHtmlString());
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT, new { size = "20", @class = "form-normal", placeholder = "郵遞區號名稱" }, true).ToHtmlString());
            if (!IsReadOnly)
            {
                sb.Append("<span class='input-group-btn'>");
                sb.Append("<button type='button' class='btn btn-info' onclick='do" + EXPname + "Select()'>");
                sb.Append("<i class='fa fa-search' aria-hidden='true'></i>");
                sb.Append("</button>");
                sb.Append("</span>");
            }
            sb.Append("</div>");
            sb.Append(htmlHelper.TextBoxFor(expressionDetail, new { size = "59", maxlength = "64", @class = "form-normal", placeholder = "請輸入地址" }, IsReadOnly).ToHtmlString());

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");

            //組裝Tel
            sb_script.Append("  function loadDialog" + EXPname + "CodeMapName(data) {");
            sb_script.Append("ajaxLoadMore(data.url,");
            sb_script.Append("data.arg,");
            sb_script.Append("function (resp) {");
            sb_script.Append("if (resp === undefined) data.box.val('');");
            sb_script.Append("else {");
            sb_script.Append("if (resp.data != '') data.box.val(resp.data);");
            sb_script.Append("else {");
            sb_script.Append("data.box.val('');");
            sb_script.Append("data.add.val('');");
            sb_script.Append(" blockAlert(data.msg);");
            sb_script.Append(" }");
            sb_script.Append(" }");
            sb_script.Append(" });");
            sb_script.Append("  };");

            sb_script.Append("function do" + EXPname + "Select() {");
            sb_script.Append("  var url = '/ZIP_CO';");
            sb_script.Append("  var title = '郵遞區號查詢';");
            sb_script.Append("  popupWindow({ 'width': 900 },");
            sb_script.Append("   url,");
            sb_script.Append("  title,");
            sb_script.Append("  null,");
            sb_script.Append("  function (retData, doc) {");
            sb_script.Append("   if (retData != null) {");
            sb_script.Append("   $('#" + propertyId + "').val(retData.Id);");
            sb_script.Append("  $('#" + propertyId_text + "').val(retData.Name);");
            sb_script.Append("  $('#" + propertyId_detail + "').val(retData.Name);");
            sb_script.Append("  }");
            sb_script.Append(" });");
            sb_script.Append("  }");

            sb_script.Append("  function do" + EXPname + "NameGet() {");
            sb_script.Append("   var code = $('#" + propertyId + "');");
            sb_script.Append("  console.log('" + propertyId + "');");
            sb_script.Append("   var data = {");
            sb_script.Append("  'url': '/Ajax/GetCityTown',");
            sb_script.Append("  'msg': '查無該郵遞區號!!',");
            sb_script.Append("  'arg': { 'CODE': code.val() },");
            sb_script.Append("  'box': $('#" + propertyId_text + "'),");
            sb_script.Append("  'add': $('#" + propertyId + "')");
            sb_script.Append("   };");
            sb_script.Append("  if ($.trim(data.arg.CODE) == '') data.box.val('');");
            sb_script.Append("  else {");
            sb_script.Append("  code.val(data.arg.CODE);");
            sb_script.Append("   loadDialog" + EXPname + "CodeMapName(data);");
            sb_script.Append("  }");
            sb_script.Append("  }");



            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}