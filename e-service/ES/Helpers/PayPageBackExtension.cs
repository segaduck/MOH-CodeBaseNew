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
using ES.DataLayers;

namespace ES.Helpers
{
    /// <summary>
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class PayPageBackExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString PayBackForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            string pay_method, string pay_money
        )
        {
            //// HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='form-group'>");
            sb.Append("<label class='main-label col-sm-2'>繳費方式</label>");
            sb.Append("<div class='col-sm-4'>");
            if (pay_method == "C")
            {
                sb.Append("<div class='form-control-static'>信用卡</div>");
            }
            else if(pay_method == "D")
            {
                sb.Append("<div class='form-control-static'>匯票</div>");
            }
            else if (pay_method == "T")
            {
                sb.Append("<div class='form-control-static'>劃撥</div>");
            }
            else if (pay_method == "B")
            {
                sb.Append("<div class='form-control-static'>現金</div>");
            }
            else if (pay_method == "S")
            {
                sb.Append("<div class='form-control-static'>超商</div>");
            }
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("<div class='form-group'>");
            sb.Append("<label class='main-label col-sm-2'>繳費金額</label>");
            sb.Append("<div class='col-sm-4'>");
            sb.Append("<div class='form-control-static'>" + pay_money + "</div>");
            sb.Append("</div>");
            sb.Append("</div>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
