using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq.Expressions;
using System.Reflection;
using Turbo.Commons;
using log4net;
using System.Text.RegularExpressions;

namespace EECOnline.Helpers
{
    /// <summary>
    /// 用來顯示 Checkbox Group/List 的 HtmlHelper 擴充, 輸出內容採用 Bootstrap 3 的樣式 
    /// </summary>
    public static class ERadioGroupExtension
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 產生 Radio Group (Bootstrap style) 的 HtmlHelper
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="listItems"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString ERadioGroupFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> listItems, object htmlAttributes = null)
        {
            // 2017.07.13, eric
            // 參考 mvc3-rtm-source 中的 InputExtensions.cs 中的寫法
            // 修改取得 property full name 的方式, 
            // 以解決 SubModel 是以 IList<> 方式存在時, 無法正確取得 sub model 前置詞的問題  
            string expressionText = ExpressionHelper.GetExpressionText(expression);
            string propertyName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expressionText);
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("expression");
            }

            //Get currently select values from the ViewData model
            TProperty property = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            //Convert selected value list to a List<string> for easy manipulation
            string selectedValue = (property != null) ? property.ToString() : string.Empty;

            logger.Debug("RadioGroupFor: " + expression.Body);
            logger.Debug("RadioGroupFor(" + propertyName + "): selectedValue=" + selectedValue + "");

            // Create outer div of checkbox group
            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            // Add checkboxes
            int idx = 0;
            foreach (SelectListItem item in listItems)
            {
                logger.Debug("RadioGroupFor: " + item.ToString());
                bool isChecked = selectedValue.Equals(item.Value);

                divTag.InnerHtml += String.Format("<div class=\"form-check form-check-inline\" id=\"radio_{1}_{2}\">" +
                    "<label for=\"{1}_{2}\">" +
                    "<input type=\"radio\" name=\"{0}\" id=\"{1}_{2}\" value=\"{2}\" {3} />{4}</label></div>",
                                                    propertyName,
                                                    propertyName.Replace('.','_'),
                                                    item.Value,
                                                    isChecked ? "checked=\"checked\"" : "",
                                                    item.Text,
                                                    idx);
                idx++;
            }

            return MvcHtmlString.Create(divTag.ToString());
        }

    }
}