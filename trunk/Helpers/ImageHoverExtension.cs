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
using EECOnline.Models;

namespace EECOnline.Helpers
{
    /// <summary>
    /// HTML 附件顯示 輸入框產生輔助方法類別
    /// </summary>
    public static class ImageHoverExtension
    {
        /// <summary>HTML 附件顯示單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ImageHoverForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,

            object htmlAttributes = null, bool enabled = true, string SRV_ID = "")
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();
            string[] validExts = new string[] { "pdf" };
            String funcId = Guid.NewGuid().ToString().Replace("-", "");
            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model);
            var filename = value.SubstringTo(value.LastIndexOf("/") + 1);

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            if (System.IO.File.Exists(value))
            {
                sb.Append("<input style=\"display:none;\" id=\"" + propertyId + "\" name=\"" + propertyName + "\" value=\"" + value + "\">");

                sb.Append("<p class=\"form-control-plaintext\">");
                sb.Append("<a href=\"/GetZip/GetFile1?filename=" + filename + "\" target=\"_blank\">");
                sb.Append(filename);
                sb.Append("</a>");
                sb.Append("</p>");
                if (MimeMapping.GetMimeMapping(value) == "image/jpeg" || MimeMapping.GetMimeMapping(value) == "image/png")
                {
                    byte[] imageArray = System.IO.File.ReadAllBytes(value);
                    string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                    sb.Append("<a target='_blank' id='" + propertyId + @"_HoverImg_Link' onclick='img_blank_dy" + propertyId + @"(this)'> ");
                    //圖檔
                    var showContent = "<img id='{0}_HoverImg' width='100px' src='data:{1};base64,{2}' />";
                    sb.Append(string.Format(showContent, propertyId, MimeMapping.GetMimeMapping(value).Split('/'), base64ImageRepresentation));
                    sb.Append("</a>");
                    sb_script = new StringBuilder();
                    sb_script.Append(@"<script type='text/javascript'>
                                        function img_blank_dy" + propertyId + @"(img){
                                        var newTab = window.open();
                                        var imgData = img.children[0].src;
                                        newTab.document.body.innerHTML = ""<img src="" + imgData + "">"";
                                    }
                                 </script>");
                    sb.Append(sb_script);
                    sb.Append("<br/>");
                }
                else
                {
                    //一般檔案
                    foreach (var item in validExts)
                    {
                        if (string.Compare(filename.SubstringTo(filename.LastIndexOf(".") + 1), item, true) == 0)
                        {
                            byte[] pdfArray = System.IO.File.ReadAllBytes(value);
                            string base64PdfRepresentation = Convert.ToBase64String(pdfArray);
                            sb.AppendFormat("<button type='button' onclick='doPreViewShow{0} (\"{1}\")' style='margin-left:20px'>PDF預覽</button>", propertyId, base64PdfRepresentation);
                            sb.Append(" <br/>");
                            sb_script = new StringBuilder();
                            sb_script.Append("<script type = \"text/javascript\" > \r\n");

                            sb_script.Append("function doPreViewShow" + propertyId + "(pdf) { \r\n");
                            sb_script.Append("var newTab = window.open(); \r\n newTab.document.body.innerHTML = \"<iframe width='100%' height='100%' src ='data:application/pdf;base64, \" + pdf + \"'></iframe> \"; \r\n}");

                            sb_script.Append("</script> \r\n");

                            sb.Append(sb_script);
                        }
                    }
                }
            }
            return MvcHtmlString.Create(sb.ToString());
        }

        public static MvcHtmlString ImageHoverListForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,

            object htmlAttributes = null, bool enabled = true, string SRV_ID = "")
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();
            string[] validExts = new string[] { "pdf" };
            String funcId = Guid.NewGuid().ToString().Replace("-", "");
            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var text = (List<OtherFile>)metadata.Model;
            if (text != null)
            {
                var i = 0;
                foreach (var item in text)
                {
                    var propertyName = "Upload[" + i.TONotNullString() + "].FilePath";
                    var propertyId = "Upload_" + i.TONotNullString() + "FilePath";

                    sb.Append("<div class=\"col-sm-2\">");
                    sb.Append("<label class=\"form-label\" for=\"" + propertyName + "\">其他_" + (i + 1).TONotNullString() + "</label>");
                    sb.Append("</div>");
                    sb.Append("<div class='col-sm-10'>");

                    var value = item.FilePath;
                    var filename = value.SubstringTo(value.TONotNullString().LastIndexOf("/") + 1);

                    if (System.IO.File.Exists(value))
                    {

                        sb.Append("<input style=\"display:none;\" id=\"" + propertyId + "\" name=\"" + propertyName + "\" value=\"" + value + "\">");

                        sb.Append("<p class=\"form-control-plaintext\">");
                        sb.Append("<a href=\"/GetZip/GetFile1?filename=" + filename + "\" target=\"_blank\">");
                        sb.Append(filename);
                        sb.Append("</a>");
                        sb.Append("</p>");
                        if (MimeMapping.GetMimeMapping(value) == "image/jpeg" || MimeMapping.GetMimeMapping(value) == "image/png")
                        {
                            byte[] imageArray = System.IO.File.ReadAllBytes(value);
                            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                            sb.Append("<a target='_blank' id='" + propertyId + @"_HoverImg_Link' onclick='img_blank_dy" + propertyId + @"(this)'> ");
                            //圖檔
                            var showContent = "<img id='{0}_HoverImg' width='100px' src='data:{1};base64,{2}' />";
                            sb.Append(string.Format(showContent, propertyId, MimeMapping.GetMimeMapping(value).Split('/'), base64ImageRepresentation));
                            sb.Append("</a>");
                            sb_script = new StringBuilder();
                            sb_script.Append(@"<script type='text/javascript'>
                                        function img_blank_dy" + propertyId + @"(img){
                                        var newTab = window.open();
                                        var imgData = img.children[0].src;
                                        newTab.document.body.innerHTML = ""<img src="" + imgData + "">"";
                                    }
                                 </script>");
                            sb.Append(sb_script);
                            sb.Append("<br/>");
                        }
                        else
                        {
                            //一般檔案
                            foreach (var item1 in validExts)
                            {
                                if (string.Compare(filename.SubstringTo(filename.LastIndexOf(".") + 1), item1, true) == 0)
                                {
                                    byte[] pdfArray = System.IO.File.ReadAllBytes(value);
                                    string base64PdfRepresentation = Convert.ToBase64String(pdfArray);
                                    sb.AppendFormat("<button type='button' onclick='doPreViewShow{0} (\"{1}\")' style='margin-left:20px'>PDF預覽</button>", propertyId, base64PdfRepresentation);
                                    sb.Append(" <br/>");
                                    sb_script = new StringBuilder();
                                    sb_script.Append("<script type = \"text/javascript\" > \r\n");

                                    sb_script.Append("function doPreViewShow" + propertyId + "(pdf) { \r\n");
                                    sb_script.Append("var newTab = window.open(); \r\n newTab.document.body.innerHTML = \"<iframe width='100%' height='100%' src ='data:application/pdf;base64, \" + pdf + \"'></iframe> \"; \r\n}");

                                    sb_script.Append("</script> \r\n");

                                    sb.Append(sb_script);
                                }
                            }
                        }
                        i++;
                    }
                    sb.Append("</div>");
                }
            }
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}