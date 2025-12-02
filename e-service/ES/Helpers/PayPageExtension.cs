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
    public static class PayPageExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString PayForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            string Service_ID
        )
        {
            ShareDAO dao = new ShareDAO();
            var data = dao.getPayInfo(Service_ID);

            //// HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='form-group'>");
            sb.Append("<label class='step-label col-sm-2'>繳費方式</label>");
            sb.Append("<div class='col-sm-10'>");
            sb.Append("<input id='PAY_POINT' name='PAY_POINT' type='hidden' value='" + data.Rows[0][2].ToString() + "'>");
            for (int i = 0; i < data.Rows[0][1].ToString().Length; i++)
            {
                if (data.Rows[0][1].ToString().Substring(i, 1) == "C")
                {
                    //信用卡
                    sb.Append("<input id='PAY_METHOD' name='PAY_METHOD' type='radio' value='C'>信用卡<br>");
                }

                if (data.Rows[0][1].ToString().Substring(i, 1) == "D")
                {
                    //匯票
                    sb.Append("<input id='PAY_METHOD' name='PAY_METHOD' type='radio' value='D'>匯票(抬頭：衛生福利部）<br>");
                }

                if (data.Rows[0][1].ToString().Substring(i, 1) == "T")
                {
                    //劃撥
                    sb.Append("<input id='PAY_METHOD' name='PAY_METHOD' type='radio' value='T'>劃撥<br>");
                }

                if (data.Rows[0][1].ToString().Substring(i, 1) == "B")
                {
                    //現金
                    sb.Append("<input id='PAY_METHOD' name='PAY_METHOD' type='radio' value='B'>現金<br>");
                }

                if (data.Rows[0][1].ToString().Substring(i, 1) == "S")
                {
                    //超商
                    sb.Append("<input id='PAY_METHOD' name='PAY_METHOD' type='radio' value='S'>超商<br>");
                }
            }
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<div class='form-group'>");
            sb.Append("<label class='step-label col-sm-2'>申請時應繳費用</label>");
            sb.Append("<div class='col-sm-10' style='margin-top:10px'>" + data.Rows[0][0].ToString() + "元");
            sb.Append("<input id='PAY_A_FEE' name='PAY_A_FEE' type='hidden' value='" + data.Rows[0][0].ToString() + "'>");
            sb.Append("</div>");
            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
