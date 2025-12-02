using log4net;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace ES.Helpers
{
    /// <summary>
    /// 用來顯示 客制化 LABEL 的 HtmlHelper 擴充, 輸出內容採用 Bootstrap 3 的樣式,
    /// 這個 Helper 輸出的 Label 會自動判斷 Property 是否標示 Required,
    /// 額外輸出一個 class="mark-red" 的星號 span
    /// </summary>
    public static class CustomLabelExtension
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static object GetPropertyValue(object src, string propName)
        {

            if (propName.Contains("."))//complex type nested
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                return GetPropertyValue(GetPropertyValue(src, temp[0]), temp[1]);
            }
            else
            {
                var prop = src.GetType().GetProperty(propName);
                return prop != null ? prop.GetValue(src, null) : null;
            }
        }

        /// <summary>
        /// 產生 Custom Label (Bootstrap style) 的 HtmlHelper,
        /// 並會根據 Property 是否有標定 Required 自動加上必填欄位樣式
        /// (客制化版面樣式)
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString CustomLabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            return CustomLabelFor(htmlHelper, expression, false, htmlAttributes);
        }

        /// <summary>
        /// 產生 Custom Label (Bootstrap style) 的 HtmlHelper,
        /// 並會根據 Property 是否有標定 Required 自動加上必填欄位樣式
        /// <para>
        /// 若 forceMark = true 則, 不論 Property 是否有標定 Required, 
        /// 都會加上星號必填樣式
        /// </para>
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="forceMark">forceMark</param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString CustomLabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool forceMark = false, object htmlAttributes = null)
        {
            // 從 expression 中取得 property name for CheckBoxListFor
            // expression.Body 是 MemberExpression,  toString() 原始格式會類似: m.Form.FIELD_NAME
            // 要去掉最前面的 "m." 成為 Form.FIELD_NAME
            Regex regex = new Regex(expression.Parameters[0] + ".");
            string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);

            //取得欄位 PropertyInfo, 判斷是否有標示 RequiredAttribute 
            bool required = forceMark;
            if(!required)
            {
                PropertyInfo pInfo = null;
                if (expression.Body.NodeType == ExpressionType.MemberAccess)
                {
                    pInfo = (PropertyInfo)((MemberExpression)expression.Body).Member;
                    Attribute requiredAttr = pInfo.GetCustomAttribute(typeof(RequiredAttribute));
                    if (requiredAttr != null)
                    {
                        required = true;
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }            

            // 取得 Label Text, from Display(Name=*)
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string labelText = metadata.DisplayName;
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = propertyName;
            }

            logger.Debug("CustomLabelFor: " + expression.Body);
            logger.Debug("CustomLabelFor(" + propertyName + "): labelText=" + labelText + ", required=" + required);

            // Create label element
            TagBuilder labelTag = new TagBuilder("label");
            labelTag.AddCssClass("control-label");
            //labelTag.AddCssClass("col-sm-2");
            //labelTag.AddCssClass("label-set");

            //補加 label 物件 for 的屬性設定(for 無障礙檢測)
            labelTag.Attributes.Add("for", propertyName.Replace('.','_'));

            labelTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            // 判斷加上必填欄位的客制化樣式
            if (required)
            {
                labelTag.InnerHtml = "<span style=\"color:red\" sr-only=\"必填\">＊</span>";
            }

            labelTag.InnerHtml += labelText;

            return MvcHtmlString.Create(labelTag.ToString());
        }

    }
}