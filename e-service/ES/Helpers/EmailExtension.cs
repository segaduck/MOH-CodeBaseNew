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
using ES.Models;

namespace ES.Helpers
{
    /// <summary>
    /// HTML Email 輸入框產生輔助方法類別
    /// </summary>
    public static class EmailExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString EmailForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            object htmlAttributes = null,
                bool IsReadOnly = false)
        {

            var DisabledString = IsReadOnly ? "pointer-events:none;background:#DDDDDD" : "";

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            // mail-value 切割
            var mail_1 = "";
            var mail_2 = "";
            if (value.Contains("@"))
            {
                // mail 前半部
                mail_1 = value.ToSplit('@')[0];
                // mail 後半部
                mail_2 = value.ToSplit('@')[1];

            }

            ShareCodeListModel dao = new ShareCodeListModel();
            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append(htmlHelper.HiddenFor(expression).ToHtmlString());

            sb.Append(htmlHelper.TextBox(propertyId + "_1", mail_1, new { size = "20", @class = " form-normal", placeholder = "Email", style = DisabledString }).ToHtmlString());
            sb.Append("@");
            sb.Append(htmlHelper.DropDownList(propertyId + "_2", dao.GetMailDomainList1, new {  @class = "form-normal ", style = DisabledString }).ToHtmlString());
            sb.Append(htmlHelper.TextBox(propertyId + "_3", mail_2, new { size = "40", @class = "form-normal ", placeholder = "EmailAddress", style = DisabledString }).ToHtmlString());


            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "_1').on('blur', do" + EXPname + "NameGet1);");
            sb_script.Append("$('#" + propertyId + "_2').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("$('#" + propertyId + "_3').on('blur', do" + EXPname + "NameGet3);");
            sb_script.Append(" do" + EXPname + "NameGet();");

            sb_script.Append("});");

            //組裝Email
            sb_script.Append("function do" + EXPname + "NameGet() {");
            sb_script.Append("var EmailValue = '';");
            sb_script.Append("var Email1 = $('#" + propertyId + "_1');");
            sb_script.Append("var Email2 = $('#" + propertyId + "_2');");
            sb_script.Append("var Email3 = $('#" + propertyId + "_3');");
            sb_script.Append("var Email = $('#" + propertyId + "');");

            sb_script.Append("if(Email2.val() != '' && (Email2.val() != '0' || Email3.val().trim() != '')){Email3.val(Email2.val());if(Email3.val() == '0'){Email3.val('  ')} }");

            sb_script.Append("if(Email2.val() != '0'){");
            sb_script.Append("Email2.val(Email3.val());");
            sb_script.Append("if(Email2.val() == null){Email2.val('0'); }");
            sb_script.Append("}");

            sb_script.Append("if(Email2.val() == '0'){Email3.removeAttr('disabled')}");
            sb_script.Append("if(Email2.val() != '0'){Email3.attr('disabled','disabled')}");

            sb_script.Append("if(Email1.val().trim() != '' && (Email2.val() != '' || Email3.val().trim() != '')){EmailValue +=Email1.val()+'@'+Email3.val().trim() ;}");
            sb_script.Append("Email.val(EmailValue)");
            sb_script.Append("};");

            //組裝Email1
            sb_script.Append("function do" + EXPname + "NameGet1() {");
            sb_script.Append("var EmailValue = '';");
            sb_script.Append("var Email1 = $('#" + propertyId + "_1');");
            sb_script.Append("var Email3 = $('#" + propertyId + "_3');");
            sb_script.Append("var Email = $('#" + propertyId + "');");
            

            sb_script.Append("if(Email1.val().trim() != '' && Email3.val().trim() != ''){EmailValue +=Email1.val()+'@'+Email3.val().trim() ;}");
            sb_script.Append("Email.val(EmailValue)");
            sb_script.Append("};");
            

            //組裝Email3
            sb_script.Append("function do" + EXPname + "NameGet3() {");
            sb_script.Append("var EmailValue = '';");
            sb_script.Append("var Email1 = $('#" + propertyId + "_1');");
            sb_script.Append("var Email2 = $('#" + propertyId + "_2');");
            sb_script.Append("var Email3 = $('#" + propertyId + "_3');");
            sb_script.Append("var Email = $('#" + propertyId + "');");
            
            sb_script.Append("if(Email1.val().trim() != '' && Email3.val().trim() != ''){EmailValue +=Email1.val()+'@'+Email3.val().trim() ;}");
            sb_script.Append("Email.val(EmailValue)");
            sb_script.Append("};");

            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }

    }
}