using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ES.Helpers
{
    /// <summary>
    /// 支援動態設定 Readonly 的 TextBoxFor helper 擴充
    /// </summary>
    public static class ExtTextBoxExtension
    {

        /// <summary>
        /// 支援動態條件 Readonly 的 TextBoxFor Html Helper
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="isReadonly"></param>
        /// <returns></returns>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                    Expression<Func<TModel, TProperty>> expression, bool isReadonly)
        {
            return TextBoxFor<TModel, TProperty>(htmlHelper, expression, new Dictionary<string, object>(), isReadonly);
        }

        /// <summary>
        /// 支援動態條件 Readonly 的 TextBoxFor Html Helper
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="isReadonly"></param>
        /// <returns></returns>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                    Expression<Func<TModel, TProperty>> expression,
                    object htmlAttributes, bool isReadonly)
        {
            IDictionary<string, object> htmlAttrs = (IDictionary < string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return TextBoxFor<TModel, TProperty>(htmlHelper, expression, htmlAttrs, isReadonly);
        }

        /// <summary>
        /// 支援動態條件 Readonly 的 TextBoxFor Html Helper
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="isReadonly"></param>
        /// <returns></returns>
        public static MvcHtmlString TextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, 
                    Expression<Func<TModel, TProperty>> expression,
                    IDictionary<string, object> htmlAttributes, bool isReadonly)
        {
            if(isReadonly)
            {
                htmlAttributes.Add("readonly", "readonly");
            }
            else
            {
                htmlAttributes.Remove("readonly");
            }

            return htmlHelper.TextBoxFor(expression, htmlAttributes);
        }
    }
}