using ES.Commons;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace ES.Helpers
{
    /// <summary>
    /// 用來顯示 Checkbox Group/List 的 HtmlHelper 擴充, 
    /// 輸出內容採用 Bootstrap 3 的樣式,
    /// 並需要 Font Awesome 4.7 以顯示 check/uncheck 的視覺效果
    /// </summary>
    public static class CheckboxListExtension
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 產生 Checkbox Group (Bootstrap style) 的 HtmlHelper
        /// 輸出內容採用 Bootstrap 3 的樣式,
        /// 並需要 Font Awesome 4.7 以顯示 check/uncheck 的視覺效果
        /// <para>
        /// 注意: 需配合 global.js 中一段 checkbox change event handler 勾選的動作才能正常運作
        /// </para>
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="listItems"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, IList<TProperty>>> expression, IEnumerable<CheckBoxListItem> listItems, object htmlAttributes = null)
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
            IList<TProperty> list = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            //Convert selected value list to a List<string> for easy manipulation
            List<string> selectedValues = new List<string>();

            if (list != null)
            {
                selectedValues = new List<TProperty>(list)
                    .ConvertAll<string>(delegate (TProperty i)
                    {
                        return (i != null) ? i.ToString() : null;
                    });
            }
            logger.Debug("CheckBoxListFor: " + expression.Body);
            logger.Debug("CheckBoxListFor(" + propertyName + "): selectedValues=[" + string.Join(",", selectedValues.ToArray()) + "]");

            //20171225 加入用來判斷是否使用 Html.Partial() 方式輸出的程式碼
            string divId = htmlHelper.IsPartialOutput() ? htmlHelper.NewSequenceId() : null;

            // Create outer div of checkbox group
            TagBuilder divTag = new TagBuilder("div");
            divTag.Attributes.Add("class", "form-group");
            //20171225 當 page.Layout 為空值時必須加入 id 屬性值
            if (divId != null) divTag.Attributes.Add("id", divId);

            //divTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            // Add / Merge HtmlAttributes
            string strReadonly = "";
            if (htmlAttributes != null)
            {
                foreach (var entry in new RouteValueDictionary(htmlAttributes))
                {
                    // 額外的 htmlAttributes
                    // 只有 readonly 會作用於 input 元件
                    // 其餘都作用在 wrap div 以符合 bootstrap 的行為
                    if ("readonly".Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        strReadonly = Convert.ToString(entry.Value);
                    }
                    //=====================================================
                    else if ("class".Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        divTag.AddCssClass(Convert.ToString(entry.Value));

                    }
                    else
                    {
                        divTag.Attributes[entry.Key] = Convert.ToString(entry.Value);
                    }
                }
            }


            // Add checkboxes
            int idx = 0;
            foreach (CheckBoxListItem item in listItems)
            {
                logger.Debug("CheckBoxListFor: " + item.ToString());
                bool isChecked = false;
                if (selectedValues.Count > 0)
                {
                    // Form Model binding
                    isChecked = selectedValues.Contains(item.Value);
                }
                else
                {
                    // Default status
                    isChecked = item.Checked;
                }
                // 在 input 之後加上一個空的 <i> 用 Font Awesome 顯示 check 狀態
                // 用來替代 ::before, ::after 的CSS應用, 
                // 以解決 onclick event 無法 binding 在 pseudo-elements 的問題
                //divTag.InnerHtml += String.Format("<div class=\"checkbox\" id=\"checkbox_{1}_{2}\">" +
                //    "<label for=\"{1}_{2}\">" +
                //    "<input type=\"checkbox\" name=\"{0}\" id=\"{1}_{2}\" value=\"{2}\" {3} {6}/>{5}{4}</label></div>",
                //                                    propertyName,
                //                                    propertyName.Replace('.', '_'),
                //                                    item.Value,
                //                                    isChecked ? "checked=\"checked\"" : "",
                //                                    item.DisplayText,
                //                                    string.Format("<i class=\"fa fa-{0}\" aria-hidden=\"true\"></i>", isChecked ? "check-square-o" : "square-o"),
                //                                    (strReadonly == "" ? "" : "readonly=\"" + strReadonly + "\"")
                //                                    );


                divTag.InnerHtml += String.Format("<div id=\"checkbox_{1}_{2}\" style='float:left;width:150px'>" +
                  "<input type=\"checkbox\" name=\"{0}\" id=\"{1}_{2}\" value=\"{2}\" {3} {6} style='zoom:1.5'/>{5}{4}</div>",
                                                  propertyName,
                                                  propertyName.Replace('.', '_'),
                                                  item.Value,
                                                  isChecked ? "checked=\"checked\"" : "",
                                                  item.DisplayText,
                                                  string.Format("", isChecked ? "check-square-o" : "square-o"),
                                                  (strReadonly == "" ? "" : "readonly=\"" + strReadonly + "\"")
                                                  );
                idx++;
            }

            //20171222 解決 CheckBoxList CSS 在以 Html.Partial() 方式輸出內容時，會導致勾選與取消勾選視覺圖示沒有變化問題。
            if (divId != null)
            {
                //20171225 當使用 Html.Partial() 方法輸出勾選框時，必須動態呼叫一次 bindEventCheckBoxListChange() 方法，繫結 checkbox change 事件。
                divTag.InnerHtml += string.Concat("<script type=\"text/javascript\">if (bindEventCheckBoxListChange) bindEventCheckBoxListChange(\"", divId, "\");</script>");
            }

            return MvcHtmlString.Create(divTag.ToString());
        }

    }
}