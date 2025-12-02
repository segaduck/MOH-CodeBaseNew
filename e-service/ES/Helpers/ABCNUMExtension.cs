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
    /// HTML 生殖細胞或胚胎數量 輸入框產生輔助方法類別
    /// </summary>
    public static class ABCNUMExtension
    {
        /// <summary>HTML 生殖細胞或胚胎數量 單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ABCNUMForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression_aName,
            Expression<Func<TModel, object>> expression_aNum1,
            Expression<Func<TModel, object>> expression_aDate,
            Expression<Func<TModel, object>> expression_bName,
            Expression<Func<TModel, object>> expression_bNum1,
            Expression<Func<TModel, object>> expression_bNum2,
            Expression<Func<TModel, object>> expression_bDate,
            Expression<Func<TModel, object>> expression_cName1,
            Expression<Func<TModel, object>> expression_cName2,
            Expression<Func<TModel, object>> expression_cNum1,
            Expression<Func<TModel, object>> expression_cDate,
            Expression<Func<TModel, object>> expression_cDay,
            object htmlAttributes = null,
                bool IsReadOnly = false)
        {

            var DisabledString = IsReadOnly ? "pointer-events:none;background:#DDDDDD" :"" ;

           

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='input-group'>");
            sb.Append("取自");
            sb.Append(htmlHelper.TextBoxFor(expression_aName, new { size = "15", @class = " form-normal", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("之精子");
            sb.Append(htmlHelper.TextBoxFor(expression_aNum1, new { size = "15", @class = "form-normal ", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("(管)，取得日期為");
            sb.Append(htmlHelper.DatePickerTWFor(expression_aDate, new { placeholder = "年/月/日", style = DisabledString }).ToHtmlString());
            sb.Append("</div>");

            sb.Append("<div class='input-group'>");
            sb.Append("取自");
            sb.Append(htmlHelper.TextBoxFor(expression_bName, new { size = "15", @class = "form-normal ", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("之卵子");
            sb.Append(htmlHelper.TextBoxFor(expression_bNum1, new { size = "15", @class = " form-normal", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("(個)，每支");
            sb.Append(htmlHelper.TextBoxFor(expression_bNum2, new { size = "15", @class = "form-normal ", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("卵子，取得日期為");
            sb.Append(htmlHelper.DatePickerTWFor(expression_bDate, new { placeholder = "年/月/日", style = DisabledString }).ToHtmlString());
            sb.Append("</div>");

            sb.Append("<div class='input-group'>");
            sb.Append("取自");
            sb.Append(htmlHelper.TextBoxFor(expression_cName1, new { size = "15", @class = "form-normal ", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("及配偶");
            sb.Append(htmlHelper.TextBoxFor(expression_cName2, new { size = "15", @class = "form-normal ", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("之生殖細胞所形成之胚胎");
            sb.Append(htmlHelper.TextBoxFor(expression_cNum1, new { size = "15", @class = "form-normal ", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("(個)，形成日期為");
            sb.Append(htmlHelper.DatePickerTWFor(expression_cDate, new { placeholder = "年/月/日", style = DisabledString }).ToHtmlString());
            sb.Append("，胚胎日齡為");
            sb.Append(htmlHelper.TextBoxFor(expression_cDay, new { size = "15", @class = "form-normal  ", placeholder = "", style = DisabledString }).ToHtmlString());
            sb.Append("日");
            sb.Append("</div>");
            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("});");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }

    }
}