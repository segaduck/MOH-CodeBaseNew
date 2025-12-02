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
    /// HTML 電話 輸入框產生輔助方法類別
    /// </summary>
    public static class DonateExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString DonateAmountForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            object htmlAttributes = null,
                bool IsReadOnly = false)
        {

            var DisabledString = IsReadOnly ? "pointer-events:none;background:#DDDDDD" : "";

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("新台幣");
            sb.Append(htmlHelper.TextBox(propertyId, value, new { size = "10", @class = " form-normal ", placeholder = "請輸入數值", style = DisabledString }).ToHtmlString());
            sb.Append("元<br/>");
            sb.Append("<div>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"300\")'>NT.300</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"500\")'>NT.500</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"1000\")'>NT.1000</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"2000\")'>NT.2000</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"3000\")'>NT.3000</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"5000\")'>NT.5000</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"8000\")'>NT.8000</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"10000\")'>NT.10,000</a>");
            sb.Append("<a href = 'javascript:void(0)' class='btn btn-add' onclick='onChangeAmount(\"20000\")'>NT.20,000</a>");
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("});");

            //組裝DonateAmount
            sb_script.Append("function onChangeAmount(item) {");
            sb_script.Append("var DAmount = $('#" + propertyId + "');");
            sb_script.Append("DAmount.val(item);");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}