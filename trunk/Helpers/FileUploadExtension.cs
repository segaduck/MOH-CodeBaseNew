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

namespace EECOnline.Helpers
{
    /// <summary>
    /// HTML 上傳附件 輸入框產生輔助方法類別
    /// </summary>
    public static class FileUploadExtension
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
        public static MvcHtmlString FileUploadForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            bool IsDisplay,
            bool IsReadOnly = false,
            object htmlAttributes = null,
            string limitType = "", string limitSize = "", string UploadDesc = "", string Required = "")
        {
            var disPlay = IsDisplay ? "display:none" : "";
            var DisabledString = IsReadOnly ? "pointer-events:none;" : "";
            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name.Replace("[", "_").Replace("]", "_").Replace(".", "_");
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='custom-file'>");
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "custom-file-input", type = "file", onchange = "doGetFile" + EXPname + "Nam(this)", style = DisabledString }).ToHtmlString());
            sb.Append("<label id='" + propertyId + "_label' class='custom-file-label' for='customFile'>選取檔案</label>");
            sb.Append("</div>");

            if (!string.IsNullOrEmpty(UploadDesc))
            {
                sb.Append(string.Format("<div id='{0}' class='form-text'>{1}</div>", propertyId + "_Msg", UploadDesc));
            }

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");

            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "_CHECK').val(\"\");");
            sb_script.Append("});");
            //File檔名
            sb_script.Append("function doGetFile" + EXPname + "Nam(file) {");                                                                                                       

            if (limitType == "1")
            {
                sb_script.Append(" var validExts = new Array('.PDF', '.JPG', '.BMP', '.PNG', '.GIF', '.TIF');");
            }
            else
            {
                sb_script.Append(" var validExts = new Array('.DOC', '.DOCX', '.ODT', '.ODF', '.ODS', '.XLS', '.XLSX', '.PDF', '.PPT', '.PPTX', '.JPG', '.BMP', '.PNG', '.GIF', '.TIF', '.ZIP');");
            }

            //sb_script.Append("var filename = file.files.item(0).name;");
            sb_script.Append("var fileExt = file.value;");
            sb_script.Append("fileExt = fileExt.substring(fileExt.lastIndexOf('.'));");

            //限制上傳容量
            sb_script.Append("var blOverSize = false;");
            if (!string.IsNullOrEmpty(limitSize))
            {
                sb_script.Append("blOverSize = (file.files.item(0).size / 1024 / 1024 > "+ limitSize + ");");
            }

            sb_script.Append(" var errMsg = '';");
            sb_script.Append(" if (blOverSize) { ");
            sb_script.Append("      errMsg += '檔案大小不可超過" + limitSize + "MB\\n';");
            sb_script.Append(" } ");

            sb_script.Append(" if (validExts.indexOf(fileExt.toUpperCase()) < 0) {");
            sb_script.Append("      errMsg +='檔案類型錯誤，可接受的副檔名有：' + validExts.toString() + '\\n';");
            sb_script.Append(" } ");

            sb_script.Append(" if (errMsg != '') { ");
            sb_script.Append("      blockAlert(errMsg);");
            sb_script.Append("      $('input[name=\"' + file.name + '\"]').val(null);");

            sb_script.Append("      $('#" + propertyId + "_CHECK').val(\"\");");

            sb_script.Append("      return false;");
            sb_script.Append(" } else {");
            sb_script.Append("      $('#" + propertyId + "_CHECK').val(\"Y\");");
            sb_script.Append("      $('#' + file.name + '_TEXT').val(file.files.item(0).name);");
            sb_script.Append("      return true;");
            sb_script.Append("  }");
            sb_script.Append(" }");



            sb_script.Append(" $('#" + propertyId + "').on('change', function() {");
            sb_script.Append("       var fileName = $(this).val().split('\\\\').pop();");
            sb_script.Append("      $(this).siblings('#" + propertyId + "_label').addClass('selected').html(fileName);");
            sb_script.Append(" });");

            sb_script.Append("</script>");
            sb.Append(sb_script);
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}