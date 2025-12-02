using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Text;
using ES.Extensions;

namespace System.Web.Mvc.Html
{
    public static partial class HtmlHelperExtensions
    {

        public static string CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<string> values, object htmlAttributes)
        {
            return CheckBoxList(htmlHelper, name, values, values, null, htmlAttributes);
        }

        public static string CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<string> values, IEnumerable<string> labels, string[] checks, object htmlAttributes)
        {
            return CheckBoxList(htmlHelper, name, values, labels, checks, false, htmlAttributes);
        }

        public static string CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<string> values, IEnumerable<string> labels, string[] checks, bool line, object htmlAttributes)
        {
            // No creamos ningun CheckBox si no hay valores
            if (values == null)
            {
                return "";
            }

            if (labels == null)
            {
                labels = new List<string>();
            }

            RouteValueDictionary attributes = htmlAttributes == null ? new RouteValueDictionary() : new RouteValueDictionary(htmlAttributes);
            attributes.Remove("checked");

            StringBuilder sb = new StringBuilder();

            string[] modelValues = new string[] { };

            ModelState modelState;
            
            if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState))
            {
                //modelValues = ((string[])modelState.Value.RawValue);
            }
            
            if (checks != null)
            {
                modelValues = checks;
            }

            // Por cada valor pasado generamos un CheckBox

            IEnumerator<string> labelEnumerator = labels.GetEnumerator();
            foreach (string s in values)
            {
                // Si el array contiene el valor correspondiente a este checkbox, entonces fue chequeado
                bool isChecked = modelValues.Contains(s);
                sb.Append(CrearCheckBox(name, s, isChecked, attributes));

                labelEnumerator.MoveNext();
                if (labelEnumerator.Current != null)
                {
                    sb.AppendLine(labelEnumerator.Current + "<br/>");
                }
            }

            // Creamos el div contenedor
            TagBuilder divTag = new TagBuilder("div");
            divTag.InnerHtml = sb.ToString();

            // No nos olvidemos de indicar si hay un error en alguno de los checks
            if (modelState != null && modelState.Errors.Count > 0)
            {
                divTag.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }

            return divTag.ToString(TagRenderMode.Normal);
        }

        public static string CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<CheckBoxListItem> list)
        {
            return CheckBoxList(htmlHelper, name, list, -1, null);
        }

        public static string CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<CheckBoxListItem> list, int lineCount)
        {
            return CheckBoxList(htmlHelper, name, list, lineCount, null);
        }

        public static string CheckBoxList(this HtmlHelper htmlHelper, string name, IEnumerable<CheckBoxListItem> list, int lineCount, object htmlAttributes)
        {
            // No creamos ningun CheckBox si no hay valores
            if (list == null)
            {
                return "";
            }

            RouteValueDictionary attributes = htmlAttributes == null ? new RouteValueDictionary() : new RouteValueDictionary(htmlAttributes);
            attributes.Remove("checked");

            StringBuilder sb = new StringBuilder();

            string[] modelValues = new string[] { };

            ModelState modelState;

            if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState))
            {
                //modelValues = ((string[])modelState.Value.RawValue);
            }

            // Por cada valor pasado generamos un CheckBox

            int n=0;
            foreach (CheckBoxListItem item in list) {
                n++;
                sb.Append(CrearCheckBox(name, n, item.Value, item.Checked, attributes));

                if (lineCount > 0 && n % lineCount == 0)
                {
                    sb.AppendLine(item.Text + "<br/>");
                }
                else
                {
                    sb.AppendLine(item.Text);
                }
            }

            // Creamos el div contenedor
            TagBuilder divTag = new TagBuilder("div");
            divTag.InnerHtml = sb.ToString();

            // No nos olvidemos de indicar si hay un error en alguno de los checks
            if (modelState != null && modelState.Errors.Count > 0)
            {
                divTag.AddCssClass(HtmlHelper.ValidationInputCssClassName);
            }

            return divTag.ToString(TagRenderMode.Normal);
        }

        private static string CrearCheckBox(string name, string value, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", "checkbox");
            tagBuilder.MergeAttribute("name", name, true);

            tagBuilder.GenerateId(name);

            if (isChecked)
            {
                tagBuilder.MergeAttribute("checked", "checked");
            }

            if (value != null)
            {
                tagBuilder.MergeAttribute("value", value, true);
            }

            return tagBuilder.ToString(TagRenderMode.SelfClosing);
        }

        private static string CrearCheckBox(string name, int serialNo, string value, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", "checkbox");
            tagBuilder.MergeAttribute("name", name, true);
            tagBuilder.MergeAttribute("id", name + "_" + serialNo, true);

            tagBuilder.GenerateId(name);

            if (isChecked)
            {
                tagBuilder.MergeAttribute("checked", "checked");
            }

            if (value != null)
            {
                tagBuilder.MergeAttribute("value", value, true);
            }

            return tagBuilder.ToString(TagRenderMode.SelfClosing);
        }

        public static IHtmlString ShowGridPage(this HtmlHelper htmlHelper, int nowPage, int totalPage, int totalCount)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class='form-inline' style=\"text-align: right;\">");
            sb.Append("總共 " + totalCount + " 筆，第 ");
            sb.Append("<select class='form-control' id=\"gNowPage\" onchange=\"doQuery(this.value);$('#ActionForm').submit();\">");
            for (int i = 1; i <= totalPage; i++)
            {
                if (nowPage == i)
                {
                    sb.Append("<option value=\"" + i + "\" selected=\"selected\">" + i + "</option>");
                }
                else
                {
                    sb.Append("<option value=\"" + i + "\" >" + i + "</option>");
                }
            }
            sb.Append("</select>");
            sb.Append("頁，共 " + totalPage + " 頁");
            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }

        public static string GetDateTime(this HtmlHelper htmlHelper, object obj, string format)
        {
            StringBuilder sb = new StringBuilder();

            if (obj != null && obj.GetType().ToString().Equals("System.DateTime"))
            {
                sb.Append(((DateTime)obj).ToString(format));
            }

            return sb.ToString();
        }

        public static MvcHtmlString ShowRequired(this HtmlHelper htmlHelper)
        {
            return MvcHtmlString.Create("<span class='formRequired'>*</span>");
        }
    }
}