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
    public static class TelExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString TelForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            object htmlAttributes = null,
                bool IsReadOnly = false)
        {

            var DisabledString = IsReadOnly ? "pointer-events:none;background:#DDDDDD" : "" ;

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name.Replace(".","");
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            // tel-value 切割
            var zip = "";
            var phone = "";
            var num = "";
            if (value.Contains("-"))
            {
                // tel 區號
                zip = value.ToSplit('-')[0];
                // tel 電話
                phone = value.ToSplit('-')[1];
                if (phone.Contains("#"))
                {
                    // tel 分機
                    num = phone.ToSplit('#')[1];

                    phone = phone.ToSplit('#')[0];
                }
            }
            else phone = value;


            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append(htmlHelper.HiddenFor(expression).ToHtmlString());

            sb.Append(htmlHelper.TextBox(propertyId + "_Zip", zip, new { size = "10", @class = " form-normal ", placeholder = "區碼,ex:02", style = DisabledString }).ToHtmlString());
            sb.Append("-");
            sb.Append(htmlHelper.TextBox(propertyId + "_Phone", phone, new { size = "30", @class = "form-normal  ", placeholder = "電話,ex:23125555", style = DisabledString }).ToHtmlString());
            sb.Append("#");
            sb.Append(htmlHelper.TextBox(propertyId + "_Num", num, new { size = "10", @class = "form-normal  ", placeholder = "分機,ex:22", style = DisabledString }).ToHtmlString());


            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "_Zip').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("$('#" + propertyId + "_Phone').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("$('#" + propertyId + "_Num').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");

            //組裝Tel
            sb_script.Append("function do" + EXPname + "NameGet() {");
            sb_script.Append("var TelValue = '';");
            sb_script.Append("var ZipValue = $('#" + propertyId + "_Zip').val();");
            sb_script.Append("var PhoneValue = $('#" + propertyId + "_Phone').val();");
            sb_script.Append("var NumValue = $('#" + propertyId + "_Num').val();");
            sb_script.Append("var Tel = $('#" + propertyId + "');");

            sb_script.Append("if(ZipValue != '' && PhoneValue != ''){TelValue +=ZipValue+'-' ;}");
            sb_script.Append("if(PhoneValue != ''&& ZipValue!= ''){TelValue +=PhoneValue ;}");
            sb_script.Append("if(NumValue != '' && PhoneValue != ''&& ZipValue!= ''){TelValue +='#'+NumValue; }");
            sb_script.Append("Tel.val(TelValue)");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }

    }
}