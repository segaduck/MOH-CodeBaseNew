using ES.Commons;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace ES.Helpers
{
    /// <summary>
    /// 民國年日曆元件 html helper 擴充
    /// </summary>
    public static class DatePickerACExtension
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// 民國年日曆元件 Html Helper
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerAC(this HtmlHelper helper,
                    string fieldName, string fieldValue)
        {
            return _DatePickerAC(helper, fieldName, fieldValue, new Dictionary<string, object>());
        }

        /// <summary>
        /// 民國年日曆元件 Html Helper
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerAC(this HtmlHelper helper,
                    string fieldName, string fieldValue,
                    object htmlAttributes)
        {
            return _DatePickerAC(helper, fieldName, fieldValue, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        /// <summary>
        /// 民國年日曆元件 Html Helper, 
        /// 目標欄位的日期格式必須為西元日期字串: yyyy/MM/DD
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerAC(this HtmlHelper helper,
                    string fieldName, string fieldValue,
                    IDictionary<string, object> htmlAttributes)
        {
            return _DatePickerAC(helper, fieldName, fieldValue, htmlAttributes);
        }


        /// <summary>
        /// 民國年日曆元件 Html Helper, 
        /// 目標欄位的日期格式必須為西元日期字串: yyyy/MM/DD
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="isReadOnly"></param>
        /// <returns></returns>
        private static MvcHtmlString _DatePickerAC(this HtmlHelper helper,
                    string fieldName, string fieldValue,
                    IDictionary<string, object> htmlAttributes, bool isReadOnly = false)
        {
            /*
             * 產生如下的 HTML block: 
             */
            /*
            <div class="date datepicker-outline input-group">
                            <input size="10" class="form-control" id="START_DATE_TW" name="START_DATE_TW" type="text"></input>
                            <input id="START_DATE" name="START_DATE" type="hidden" value=""></input>
                            <span class="input-group-btn">
                                <a class="open-datepicker btn-info btn" role="button"><i class="fa fa-calendar" aria-hidden="true"></i><span class="sr-only">calendar</span></a>
                            </span>
                        </div>
            */

            // Create wraper div element of input 
            TagBuilder divTag = new TagBuilder("div");
            divTag.AddCssClass("input-group");
            divTag.AddCssClass("datepicker-outline");
            divTag.AddCssClass("date1");

            // Create input element to show TW Date
            string fieldNameTW = fieldName;
            int pDot = fieldName.LastIndexOf(".");
            if(pDot > -1)
            {
                // fieldName 包含前置詞, 
                // 如: Detail.ADATE
                //   Detail.ADATE[0]
                //   Detail[0].ADATE
                fieldNameTW = fieldName.Substring(pDot + 1);
            }

            int p = fieldNameTW.LastIndexOf("[");
            if (p > -1)
            {
                // 若 fieldName 為List Index形式, _TW 要加在 index 之前
                // 如: TDATES[1] ==>  TDATES_TW[1]
                fieldNameTW = fieldNameTW.Substring(0, p)
                    + "_TW" + fieldNameTW.Substring(p);
            }
            else
            {
                fieldNameTW += "_TW";
            }
            // 將前置詞加回去
            if(pDot > -1)
            {
                fieldNameTW = fieldName.Substring(0, pDot + 1) + fieldNameTW;
            }

            TagBuilder inputTag = new TagBuilder("input");
            inputTag.AddCssClass("form-control");
            inputTag.Attributes.Add("type", "text");
            inputTag.Attributes.Add("size", "10");
            inputTag.Attributes.Add("name", fieldNameTW);
            inputTag.Attributes.Add("id", fieldNameTW.Replace('.', '_'));
            if (isReadOnly) inputTag.Attributes.Add("disabled", "disabled");
            DateTime? dd = HelperUtil.TransToDateTime(fieldValue);
            if (dd != null)
            {
                // 20191111 Johnnydai加入，加入htmlAttributes>noTranTW則ControlTW顯示數值為西元年
                var noTranTWint = htmlAttributes.Keys.ToList().IndexOf("noTranTW");
                if (noTranTWint > 0)
                {
                    inputTag.Attributes.Add("value", fieldValue);
                }
                else
                {
                    inputTag.Attributes.Add("value", HelperUtil.DateTimeToTwString(dd));
                }
            }

            //20191101 在最外圍 div 加上一個 tagid 屬性，以便能辨識是哪一個資料欄位的日期控制項
            divTag.Attributes.Add("tagid", inputTag.Attributes["id"]);

            // Create hiddent element to keep real Date value
            TagBuilder hiddenTag = new TagBuilder("input");
            hiddenTag.AddCssClass("datepickerinput");   // 重要: bootstrap-datetimepicker 辨識用
            hiddenTag.Attributes.Add("type", "hidden");
            hiddenTag.Attributes.Add("name", fieldName);
            hiddenTag.Attributes.Add("id", fieldName.Replace('.', '_'));
            hiddenTag.Attributes.Add("value", fieldValue );

            // Create span element for input-group-btn
            TagBuilder spanTag = new TagBuilder("span");
            spanTag.AddCssClass("input-group-btn");

            // Create A/button element as calander icon
            TagBuilder aTag = new TagBuilder("a");
            aTag.AddCssClass("btn");
            aTag.AddCssClass("btn-info");
            aTag.AddCssClass("open-datepicker1");
            aTag.Attributes.Add("role", "button");
            aTag.InnerHtml = "<i class=\"fa fa-calendar\" style=\"color: white\" aria-hidden=\"true\"></i>"
                 + "<span class=\"sr-only\">calendar</span>";



            // Add / Merge HtmlAttributes to inputTag        
            if (htmlAttributes != null)
            {
                foreach (var entry in htmlAttributes)
                {
                    // 額外的 htmlAttributes
                    // 只有 placeholder, readonly 會作用於 input 元件
                    // ---2017/05/17 群鈞修改  readonly屬性也可以套用到input 元件上
                    // 其餘都作用在 wrap div 以符合 bootstrap 的行為
                    if ("placeholder".Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        inputTag.Attributes[entry.Key] = Convert.ToString(entry.Value);
                    }
                    //=2017/05/17 群鈞修改 readonly屬性套用到input 元件上
                    else if ("readonly".Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        inputTag.Attributes[entry.Key] = Convert.ToString(entry.Value);
                    }
                    //=====================================================
                    else if ("class".Equals(entry.Key, StringComparison.OrdinalIgnoreCase) )
                    {
                        divTag.AddCssClass(Convert.ToString(entry.Value));
                    }
                    else if ("title".Equals(entry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        //修改:日曆物件的textbox & a tag 補加 title 提示字（for無障礙檢測用）
                        aTag.Attributes[entry.Key] = "請選擇" + Convert.ToString(entry.Value);
                        inputTag.Attributes[entry.Key] = "請輸入" + Convert.ToString(entry.Value);
                    }
                    else
                    {
                        divTag.Attributes[entry.Key] = Convert.ToString(entry.Value);
                    }
                }
            }

            if (!isReadOnly) spanTag.InnerHtml = aTag.ToString();

            // concate all together
            divTag.InnerHtml =
                hiddenTag.ToString(TagRenderMode.SelfClosing) + "\n"     // 西元年日期字串
                + inputTag.ToString(TagRenderMode.SelfClosing) + "\n"    // 顯示用民國年日期字串
                + spanTag.ToString();

            return MvcHtmlString.Create(divTag.ToString());
        }


        /// <summary>
        /// 民國年日曆元件 Html Helper
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerACFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                    Expression<Func<TModel, TProperty>> expression)
        {
            return DatePickerACFor<TModel, TProperty>(htmlHelper, expression, new Dictionary<string, object>());
        }

        /// <summary>
        /// 民國年日曆元件 Html Helper
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="isReadOnly"></param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerACFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper,
                    Expression<Func<TModel, TProperty>> expression,
                    object htmlAttributes,bool isReadOnly = false)
        {
            IDictionary<string, object> htmlAttrs = (IDictionary < string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return DatePickerACFor<TModel, TProperty>(htmlHelper, expression, htmlAttrs, isReadOnly);
        }

        /// <summary>
        /// 民國年日曆元件 Html Helper
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="isReadOnly"></param>
        /// <returns></returns>
        public static MvcHtmlString DatePickerACFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, 
                    Expression<Func<TModel, TProperty>> expression,
                    IDictionary<string, object> htmlAttributes, bool isReadOnly = false)
        {
            // 2017.06.12, eric
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
            string value = (property != null) ? property.ToString() : string.Empty;

            //logger.Debug("DatePickerACFor: " + expression.Body);
            logger.Debug("DatePickerACFor(" + propertyName + "): value=" + value + "");

            return _DatePickerAC(htmlHelper, propertyName, value, htmlAttributes, isReadOnly);
        }
    }
}