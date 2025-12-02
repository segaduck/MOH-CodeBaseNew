using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using EECOnline.Models;
using EECOnline.Services;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML 行政區 輸入框產生輔助方法類別
    /// </summary>
    public static class Select2Extension
    {
        /// <summary>HTML 行政區單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString Select2ForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            IEnumerable<SelectListItem> listItems,
            bool IsReadOnly = false,
            string OnChange = "",
            object htmlAttributes = null, bool enabled = true)
        {
            ShareCodeListModel list = new ShareCodeListModel();

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //HTML 標籤的 id 與 name 屬性值
            //var name_text = ExpressionHelper.GetExpressionText(expressionTEXT);
            //var metadata_text = ModelMetadata.FromLambdaExpression(expressionTEXT, htmlHelper.ViewData);
            //var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            //var value_text = Convert.ToString(metadata.Model).ToLower();

            //var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            //var propertyId_text = templateInfo.GetFullHtmlFieldId(propertyName_text);
            var disabled = "";
            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<div class='input-group'>");
            if (IsReadOnly)
            {
                disabled = "disabled='disabled'";
            }
            sb.Append("\r\n<select "+ disabled + " onchange=" + OnChange + " id=\"" + propertyId + "\" class=\"select2\" style=\"width:300px;\">");
            foreach (var item in listItems)
            {
                sb.Append("\r\n<option value=\"" + item.Value + "\">" + item.Text + "</option>");
            }
            sb.Append("\r\n</select>");
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("\r\n$(\"#"+ propertyId + "\").select2();");

            sb_script.Append("\r\n$(\"#" + propertyId + "\").select2({");
            sb_script.Append("\r\nplaceholder: '請選擇資料'");
            sb_script.Append("\r\n});");
            sb_script.Append("});");

            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }


    }
}