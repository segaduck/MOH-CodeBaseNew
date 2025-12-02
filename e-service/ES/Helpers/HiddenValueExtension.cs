using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ES.Services;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Utils;

namespace ES.Helpers
{
    /// <summary>
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class HiddenValueExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        public static MvcHtmlString HiddenValueForTurbo<TModel, T>(this HtmlHelper<TModel> htmlHelper, T Model)
        {
            StringBuilder sb = new StringBuilder();

            var t = Model.GetType();
            PropertyInfo[] props = t.GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetValue(Model) != null)
                {
                    sb.Append("<input type='hidden' id='" + prop.Name + "' name='" + prop.Name + "' value='" + prop.GetValue(Model) + "'>\r\n");
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}