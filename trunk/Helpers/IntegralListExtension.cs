using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using EECOnline.Services;
using System.Text.RegularExpressions;
using EECOnline.DataLayers;
using EECOnline.Models;
using Newtonsoft.Json;
using System.Collections;

namespace EECOnline.Helpers
{
    /// <summary>
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class IntegralListExtension
    {
        /// <summary>
        /// 上傳附件(共用)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="IsDisplay">是否顯示</param>
        /// <param name="IsReadOnly">是否唯圖</param>
        /// <param name="htmlAttributes">css設定</param>
        /// <param name="validType">檢核附件型態代碼</param>
        /// <param name="limitSize">上傳容量限制(MB)</param>
        /// <param name="uploadDesc">說明文字</param>
        /// <returns></returns>
        public static MvcHtmlString IntegralListForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            bool IsReadOnly = false,
            object htmlAttributes = null, bool enabled = true)
        {
            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //HTML 標籤的 id 與 name 屬性值
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var di_style = IsReadOnly ? "style='display:none'" : "";

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("<div id='goodsTable' class='table-responsive form-table' "+ di_style + ">");

            sb.Append("<table width='100%' border='0' cellspacing='0' cellpadding='0' class='table-typeA table'>");

            sb.Append("<thead>");

            sb.Append("<tr>");

            sb.Append("<th width='25%'>課程名稱");
            sb.Append("</th>");

            sb.Append("<th width='25%'>積分");
            sb.Append("</th>");

            sb.Append("<th width='25%'>積分有效日期");
            sb.Append("</th>");

            sb.Append("</tr>");

            sb.Append("</thead>");

            sb.Append("<tbody>");
            var text = (List<IntegralModel>)metadata.Model;

            if (text != null)
            {
                var i = 0;
                foreach (var item in text)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>");
                    sb.Append(item.classname.TONotNullString());
                    sb.Append(htmlHelper.Hidden("IntegralList["+i+ "].classname", item.classname.TONotNullString()));
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(item.classgrade.TONotNullString());
                    sb.Append(htmlHelper.Hidden("IntegralList[" + i + "].classgrade", item.classgrade.TONotNullString()));
                    sb.Append("</td>");
                    sb.Append("<td>");
                    sb.Append(item.gradedate.TONotNullString());
                    sb.Append(htmlHelper.Hidden("IntegralList[" + i + "].gradedate", item.gradedate.TONotNullString()));
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    i++;
                }
            }

            sb.Append("</tbody>");

            sb.Append("</table>");

            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}