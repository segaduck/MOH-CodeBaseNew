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
    /// HTML 標籤 輸入框產生輔助方法類別
    /// </summary>
    public static class LabelExtension
    {
        /// <summary>HTML 標籤單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString LabelForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, object>> expression, object htmlAttributes = null)
        {
            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model);

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("<div>");
            sb.Append("<input id='" + propertyName + "' name='" + propertyName + "' type='hidden' value='" + value + "'>");
            sb.Append("</div>");
            sb.Append("<p class='form-control-plaintext'>" + value);
            sb.Append("</p>");


            return MvcHtmlString.Create(sb.ToString());
        }
    }
}