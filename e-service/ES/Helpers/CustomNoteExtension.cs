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
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class CustomNoteExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString NoteLabelForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            object htmlAttributes = null, bool enabled = true,int index=1)
        {
            Regex regex = new Regex(expression.Parameters[0] + ".");
            string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string labelText = metadata.DisplayName;
            string IdName = metadata.PropertyName+"_" + index.ToString();
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = propertyName;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("<label class=\"main-label col-sm-2\"> \r\n");
            sb.Append("<input id=\"Chk_" + IdName + "\" type=\"checkbox\" onclick=\""+ IdName + "SH(this)\"> \r\n ");
            sb.Append(labelText);
            sb.Append("</label \r\n>");

            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type = \"text/javascript\"  \r\n>");
            sb_script.Append("function "+ IdName + "SH(i){ \r\n");
            sb_script.Append("var CHK_ID = i.id; \r\n");
            sb_script.Append("var status = document.getElementById(CHK_ID).checked; \r\n");
            sb_script.Append("if(status == true){ \r\n");
            sb_script.Append("$('#DIV_" + IdName + "').show();");       
            sb_script.Append("}");
            sb_script.Append("else{ \r\n");
            sb_script.Append("$('#DIV_" + IdName + "').hide(); \r\n");
            sb_script.Append("$('#" + IdName + "_Note').val(''); \r\n");
            sb_script.Append("} \r\n");
            sb_script.Append("} \r\n");
            sb_script.Append("</script> \r\n");

            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }


        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString NoteLabelForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression, string REDMEMO,
            object htmlAttributes = null, bool enabled = true, int index = 1)
        {
            Regex regex = new Regex(expression.Parameters[0] + ".");
            string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string labelText = metadata.DisplayName;
            string IdName = metadata.PropertyName + "_" + index.ToString();
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = propertyName;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("<label class=\"main-label col-sm-2\"> \r\n");
            sb.Append("<input id=\"Chk_" + IdName + "\" type=\"checkbox\" onclick=\"" + IdName + "SH(this)\"> \r\n ");
            sb.Append(@"<span style='color:red'>*" + REDMEMO + "</span>" + labelText);
            sb.Append("</label \r\n>");

            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type = \"text/javascript\"  \r\n>");
            sb_script.Append("function " + IdName + "SH(i){ \r\n");
            sb_script.Append("var CHK_ID = i.id; \r\n");
            sb_script.Append("var status = document.getElementById(CHK_ID).checked; \r\n");
            sb_script.Append("if(status == true){ \r\n");
            sb_script.Append("$('#DIV_" + IdName + "').show();");
            sb_script.Append("}");
            sb_script.Append("else{ \r\n");
            sb_script.Append("$('#DIV_" + IdName + "').hide(); \r\n");
            sb_script.Append("$('#" + IdName + "_Note').val(''); \r\n");
            sb_script.Append("} \r\n");
            sb_script.Append("} \r\n");
            sb_script.Append("</script> \r\n");

            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="checkexpression">註記欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString NoteForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> checkexpression,
            object htmlAttributes = null, bool enabled = true,int index=1)
        {
            Regex regex = new Regex(checkexpression.Parameters[0] + ".");
            string checkpropertyName = regex.Replace(checkexpression.Body.ToString(), string.Empty, 1);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(checkexpression, htmlHelper.ViewData);
            string labelText = metadata.DisplayName;
            string IdName = metadata.PropertyName + "_" + index.ToString(); ;
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = checkpropertyName;
            }

            StringBuilder sb = new StringBuilder();

            // 備註欄位
            sb.Append("<div class=\"col-sm-12\" id=\"DIV_" + IdName + "\"> \r\n");
            sb.Append("<label class=\"main-label col-sm-2\"> \r\n");
            sb.Append("補件備註");
            sb.Append("</label> \r\n");
            sb.Append("<div class=\"col-sm-10\" > \r\n");
            sb.Append(htmlHelper.TextBoxFor(checkexpression, new { @id = IdName + "_Note", @class = "form-control", placeholder = "" }));
            sb.Append("</div> \r\n");
            sb.Append("</div> \r\n");

            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type = \"text/javascript\" > \r\n");
            sb_script.Append("$(document).ready(function () { \r\n");
            sb_script.Append("$('#DIV_" + IdName + "').hide(); \r\n");
            sb_script.Append("}); \r\n");
            sb_script.Append("</script> \r\n");

            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}