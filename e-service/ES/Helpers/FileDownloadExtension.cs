using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using ES.Services;
using System.Text.RegularExpressions;
using ES.DataLayers;
using ES.Areas.Admin.Models;
using ES.Models.ViewModels;

namespace ES.Helpers
{
    /// <summary>
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class FileDownloadExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="IsBackmin">前後台判斷(Y:後台,N:前台)</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString FileDLForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            string expressionTEXT, string IsBackmin = "Y",
            object htmlAttributes = null, bool enabled = true)
        {
            Regex regex = new Regex(expression.Parameters[0] + ".");
            string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string labelText = metadata.DisplayName;
            string IdName = metadata.PropertyName;
            var value = string.IsNullOrEmpty(expressionTEXT) ? "" : expressionTEXT;

            String funcId = Guid.NewGuid().ToString().Replace("-", "");

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();

            if (value != "")
            {
                var newvalue = value.Split(',');
                var filename = newvalue[0];
                var appID = newvalue[1];
                var fileNO = newvalue[2];
                var srcNO = newvalue[3];

                if (filename.LastIndexOf(".") > 0)
                {
                    var fileExtname = filename.SubstringTo(filename.LastIndexOf("."));
                    string[] validExts = new string[] { ".PDF", ".JPG", ".JPEG", ".BMP", ".PNG", ".GIF",".ODS",".DOCX",".DOC" };
                    ShareDAO dao = new ShareDAO();
                    var filelist = new List<FileGroupModel>();
                    if (newvalue.Count() < 5)
                    {
                        if (filename.IndexOf("_ATH_UP_") > 0)
                        {
                            //20201109 醫事司-醫事人員請領英文證明書(001008)-醫事人員/專科中文證書電子檔(apply_001008_ath)
                            filelist = dao.GetAthFileGridList(appID, fileNO, IsBackmin);
                        }
                        else
                        {
                            // APP_ID, FILE_NO
                            filelist = dao.GetFileGridList(appID, fileNO, IsBackmin);
                        }
                    }
                    else
                    {
                        // APP_ID, FILE_NO, SRC_NO, BITCH_INDEX
                        filelist = dao.GetFileGridList(appID, fileNO, srcNO, newvalue[4], IsBackmin);
                    }

                    string fileDesc = "";
                    string showContent = "";
                    foreach (var fileOne in filelist)
                    {
                        var fileStr = fileOne.FILE_NAME_TEXT;
                        var fileVal = fileStr.Split(',');
                        var file1 = fileVal[0];
                        var file2 = fileVal[1];
                        var file3 = fileVal[2];
                        var file4 = fileVal[3];

                        showContent = "{0}<a href=\"#\" title=\"{1}\" style=\"line-height: 3;\" onclick=\"doDownLoadFILE('{2}','{3}','{4}','{5}');\">{6}</a><br/>";

                        if ("Y".Equals(IsBackmin))
                        {
                            fileDesc = "第" + fileOne.NOTICE_NO + "次補件上傳";
                        }

                        sb.Append(string.Format(showContent, fileDesc, file1, file1, file2, file3, file4, file1));

                        var localPath = dao.GetServerLocalPath();
                        if (System.IO.File.Exists(localPath + fileOne.SRC.Replace("\\", "/")))
                        {
                            if (MimeMapping.GetMimeMapping(fileOne.SRC).Split('/')[0] == "image")
                            {
                                byte[] imageArray = System.IO.File.ReadAllBytes(localPath + fileOne.SRC.Replace("\\", "/"));
                                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                                sb.Append("<a target='_blank' id='" + IdName + @"_HoverImg_Link' onclick='img_blank_dy" + IdName + @"(this)'> ");
                                //圖檔
                                showContent = "<img id='{0}_HoverImg' width='100px' src='data:{1};base64,{2}' />";
                                sb.Append(string.Format(showContent, IdName, MimeMapping.GetMimeMapping(fileOne.SRC).Split('/'), base64ImageRepresentation));
                                sb.Append("</a>");
                                sb_script = new StringBuilder();
                                sb_script.Append(@"<script type='text/javascript'>
                                        function img_blank_dy" + IdName + @"(img){
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
                                    if (string.Compare(fileExtname.ToUpper(), item, true) == 0)
                                    {
                                        //sb.Append(" <button type=\"button\" onclick =\"doPreView"+ newvalue[2] + "() \"> 預覽 </button>");
                                        //sb.Append(" <button type=\"button\" onclick =\"doPreView" + funcId + "() \"> 預覽 </button><br/>");
                                        byte[] pdfArray = System.IO.File.ReadAllBytes(localPath + fileOne.SRC.Replace("\\", "/"));
                                        string base64PdfRepresentation = Convert.ToBase64String(pdfArray);
                                        sb.AppendFormat("<button type='button' onclick='doPreViewShow{0} (\"{1}\")' style='margin-left:20px'>PDF預覽</button>", newvalue[2],base64PdfRepresentation);
                                        //sb.AppendFormat("<button type='button' onclick='doPreViewShow{0}_blank()'>PDF預覽2</button>", newvalue[2]);
                                        sb.Append(" <br/>");
                                        sb_script = new StringBuilder();
                                        sb_script.Append("<script type = \"text/javascript\" > \r\n");
                                        //sb_script.Append("function doPreView"+ newvalue[2] + "() { \r\n");
                                        sb_script.Append("function doPreView" + funcId + "() { \r\n");
                                        sb_script.Append("var data ={ \r\n");
                                        sb_script.Append("'FILENAME':'" + file1 + "', \r\n");
                                        sb_script.Append("'APP_ID':'" + file2 + "', \r\n");
                                        sb_script.Append("'FILE_NO':'" + file3 + "', \r\n");
                                        sb_script.Append("'SRC_NO':'" + file4 + "', \r\n");
                                        sb_script.Append("} \r\n");
                                        sb_script.Append("var url =\"/PreView/PreView?FILENAME=" + file1 + "&APP_ID=" + file2 + "&FILE_NO=" + file3 + "&SRC_NO=" + file4 + "\" \r\n");
                                        sb_script.Append(" window.open (url, '_blank'); \r\n");
                                        sb_script.Append("} \r\n");



                                        sb_script.Append("function doPreViewShow" + newvalue[2] + "(pdf) { \r\n");
                                        sb_script.Append("var newTab = window.open(); \r\n newTab.document.body.innerHTML = \"<iframe width='100%' height='100%' src ='data:application/pdf;base64, \" + pdf + \"'></iframe> \"; \r\n}");

                                        //sb_script.Append("function doPreViewShow" + newvalue[2] +"_blank() { \r\n");
                                        //sb_script.Append("var data ={ \r\n");
                                        //sb_script.Append("'FILENAME':'" + file1 + "', \r\n");
                                        //sb_script.Append("'APP_ID':'" + file2 + "', \r\n");
                                        //sb_script.Append("'FILE_NO':'" + file3 + "', \r\n");
                                        //sb_script.Append("'SRC_NO':'" + file4 + "', \r\n");
                                        //sb_script.Append("} \r\n");
                                        //sb_script.Append("var url =\"/PreView/PreViewShow?FILENAME=" + file1 + "&APP_ID=" + file2 + "&FILE_NO=" + file3 + "&SRC_NO=" + file4 + "\" \r\n");
                                        //sb_script.Append(" window.open (url, '_blank'); \r\n");
                                        //sb_script.Append("} \r\n");

                                        sb_script.Append("</script> \r\n");

                                        sb.Append(sb_script);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString FrontFileDLForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            object htmlAttributes = null, bool enabled = true)
        {
            Regex regex = new Regex(expression.Parameters[0] + ".");
            string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string labelText = metadata.DisplayName;
            string IdName = metadata.PropertyName;
            var value = Convert.ToString(metadata.Model).ToLower();
            var valueRe = value.Replace('\\', '/');
            var valueList = valueRe.Split('/');
            // HTML標籤
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();
            ShareDAO dao = new ShareDAO();
            var funcId = Guid.NewGuid().ToString().Replace("-", "");

            if (value != "")
            {
                var filename = valueList[(valueList.Length - 1)];
                if (filename.LastIndexOf(".") > 0)
                {
                    var fileExtname = filename.SubstringTo(filename.LastIndexOf("."));
                    string[] validExts = new string[] { ".PDF", ".JPG", ".JPEG", ".BMP", ".PNG", ".GIF" };

                    sb.Append("<a href=\"#\" title=\"" + filename + "\" style=\"line-height: 3;\" onclick=\"doFrontDownLoadFILE('" + value + "');\">" + filename + "</a>");
                    sb.Append(htmlHelper.HiddenFor(expression));
                    UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    foreach (var item in validExts)
                    {
                        if (string.Compare(fileExtname.ToUpper(), item, true) == 0)
                        {
                            var localPath = dao.GetServerLocalPath();
                            if (System.IO.File.Exists(localPath + value.Replace("\\", "/")))
                            {
                                if (MimeMapping.GetMimeMapping(value).Split('/')[0] == "image")
                                {
                                    byte[] imageArray = System.IO.File.ReadAllBytes(localPath + value.Replace("\\", "/"));
                                    string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                                    //圖檔
                                    var showContent = "<img id='{0}_HoverImg' width='100px' src='data:{1};base64,{2}'><br/>";
                                    sb.Append("<a target='_blank' id='" + IdName + @"_HoverImg_Link' onclick='img_blank_dy" + IdName + @"(this)'> ");
                                    sb.Append(string.Format(showContent, IdName, MimeMapping.GetMimeMapping(value).Split('/'), base64ImageRepresentation));
                                    sb.Append("</a>");
                                    sb_script = new StringBuilder();
                                    sb_script.Append(@"<script type='text/javascript'>
                                        function img_blank_dy" + IdName + @"(img){
                                        var newTab = window.open();
                                        var imgData = img.children[0].src;
                                        newTab.document.body.innerHTML = ""<img src="" + imgData + "">"";
                                    }
                                 </script>");
                                    sb.Append(sb_script);
                                    //sb_script.Append("<script type = \"text/javascript\" > \r\n");
                                    //sb_script.Append("function doPreView" + funcId + "() { \r\n");
                                    //sb_script.Append("var data ={ \r\n");
                                    //sb_script.Append("'FILENAME':'" + valueRe + "', \r\n");
                                    //sb_script.Append("} \r\n");
                                    //sb_script.Append("var url ='/PreView/FrontPreView?FILENAME=" + valueRe + "';");
                                    //sb_script.Append(" window.open (url, '_blank'); \r\n");
                                    //sb_script.Append("} \r\n");
                                    //sb_script.Append("</script> \r\n");

                                    //sb.Append(sb_script);
                                }
                                else
                                {
                                    //一般檔案
                                    if (string.Compare(fileExtname, item, true) == 0)
                                    {
                                        //sb.Append(" <button type=\"button\" onclick =\"doPreView"+ newvalue[2] + "() \"> 預覽 </button>");
                                        //sb.Append(" <button type=\"button\" onclick =\"doPreView" + funcId + "() \"> 預覽 </button><br/>");
                                        byte[] pdfArray = System.IO.File.ReadAllBytes(localPath + value.Replace("\\", "/"));
                                        string base64PdfRepresentation = Convert.ToBase64String(pdfArray);
                                        sb.AppendFormat("<button type='button' onclick='doPreViewShow{0} (\"{1}\")' style='margin-left:20px'>PDF預覽</button>", IdName, base64PdfRepresentation);
                                        //sb.AppendFormat("<button type='button' onclick='doPreViewShow{0}_blank()'>PDF預覽2</button>", newvalue[2]);
                                        sb.Append(" <br/>");
                                        sb_script = new StringBuilder();
                                        sb_script.Append("<script type = \"text/javascript\" > \r\n");

                                        sb_script.Append("function doPreViewShow" + IdName + "(pdf) { \r\n");
                                        sb_script.Append("var newTab = window.open(); \r\n newTab.document.body.innerHTML = \"<iframe width='100%' height='100%' src ='data:application/pdf;base64, \" + pdf + \"'></iframe> \"; \r\n}");

                                        sb_script.Append("</script> \r\n");
                                        sb.Append(sb_script);
                                    }
                                }
                            }

                        }
                    }
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="IsBackmin">前後台判斷(Y:後台,N:前台)</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString FontFileDLForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            string IsBackmin = "Y",
            object htmlAttributes = null, bool enabled = true)
        {
            Regex regex = new Regex(expression.Parameters[0] + ".");
            string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            string labelText = metadata.DisplayName;
            string IdName = metadata.PropertyName;
            var value = Convert.ToString(metadata.Model).ToLower();

            String funcId = Guid.NewGuid().ToString().Replace("-", "");

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();

            if (value != "")
            {
                var newvalue = value.Split(',');
                var filename = newvalue[0];
                var appID = newvalue[1];
                var fileNO = newvalue[2];
                var srcNO = newvalue[3];

                if (filename.LastIndexOf(".") > 0)
                {
                    var fileExtname = filename.SubstringTo(filename.LastIndexOf("."));
                    string[] validExts = new string[] { ".PDF", ".JPG", ".JPEG", ".BMP", ".PNG", ".GIF" };
                    ShareDAO dao = new ShareDAO();
                    var filelist = new List<FileGroupModel>();
                    if (newvalue.Count() < 5)
                    {
                        if (filename.IndexOf("_ATH_UP_") > 0)
                        {
                            //20201109 醫事司-醫事人員請領英文證明書(001008)-醫事人員/專科中文證書電子檔(apply_001008_ath)
                            filelist = dao.GetAthFileGridList(appID, fileNO, IsBackmin);
                        }
                        else
                        {
                            // APP_ID, FILE_NO
                            filelist = dao.GetFileGridList(appID, fileNO, IsBackmin);
                        }
                    }
                    else
                    {
                        // APP_ID, FILE_NO, SRC_NO, BITCH_INDEX
                        filelist = dao.GetFileGridList(appID, fileNO, srcNO, newvalue[4], IsBackmin);
                    }

                    string fileDesc = "";
                    string showContent = "";
                    foreach (var fileOne in filelist)
                    {
                        var fileStr = fileOne.FILE_NAME_TEXT;
                        var fileVal = fileStr.Split(',');
                        var file1 = fileVal[0];
                        var file2 = fileVal[1];
                        var file3 = fileVal[2];
                        var file4 = fileVal[3];

                        showContent = "{0}<a href=\"#\" title=\"{1}\" style=\"line-height: 3;\" onclick=\"doDownLoadFILE('{2}','{3}','{4}','{5}');\">{6}</a>";

                        if ("Y".Equals(IsBackmin))
                        {
                            fileDesc = "第" + fileOne.NOTICE_NO + "次補件上傳";
                        }
                        sb.Append(string.Format(showContent, fileDesc, file1, file1, file2, file3, file4, file1));
                       
                        var localPath = dao.GetServerLocalPath();
                        if (System.IO.File.Exists(localPath + fileOne.SRC.Replace("\\", "/")))
                        {
                            if (MimeMapping.GetMimeMapping(fileOne.SRC).Split('/')[0] == "image")
                            {
                                byte[] imageArray = System.IO.File.ReadAllBytes(localPath + fileOne.SRC.Replace("\\", "/"));
                                string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                                //圖檔
                                showContent = "<img id='{0}_HoverImg' width='100px' src='data:{1};base64,{2}'><br/>";
                                sb.Append("<a target='_blank' id='" + IdName + @"_HoverImg_Link' onclick='img_blank_dy" + IdName + @"(this)'> ");
                                sb.Append(string.Format(showContent, IdName, MimeMapping.GetMimeMapping(fileOne.SRC).Split('/'), base64ImageRepresentation));
                                sb.Append("</a>");
                                sb_script = new StringBuilder();
                                sb_script.Append(@"<script type='text/javascript'>
                                        function img_blank_dy" + IdName + @"(img){
                                        var newTab = window.open();
                                        var imgData = img.children[0].src;
                                        newTab.document.body.innerHTML = ""<img src="" + imgData + "">"";
                                    }
                                 </script>");
                                sb.Append(sb_script);
                            }
                            else
                            {
                                //一般檔案
                                foreach (var item in validExts)
                                {
                                    if (string.Compare(fileExtname.ToUpper(), item, true) == 0)
                                    {
                                        //sb.Append(" <button type=\"button\" onclick =\"doPreView"+ newvalue[2] + "() \"> 預覽 </button>");
                                        //sb.Append(" <button type=\"button\" onclick =\"doPreView" + funcId + "() \"> 預覽 </button><br/>");
                                        byte[] pdfArray = System.IO.File.ReadAllBytes(localPath + fileOne.SRC.Replace("\\", "/"));
                                        string base64PdfRepresentation = Convert.ToBase64String(pdfArray);
                                        sb.AppendFormat("<button type='button' onclick='doPreViewShow{0} (\"{1}\")' style='margin-left:20px'>PDF預覽</button>", newvalue[2], base64PdfRepresentation);
                                        //sb.AppendFormat("<button type='button' onclick='doPreViewShow{0}_blank()'>PDF預覽2</button>", newvalue[2]);
                                        sb.Append(" <br/>");
                                        sb_script = new StringBuilder();
                                        sb_script.Append("<script type = \"text/javascript\" > \r\n");
                                        //sb_script.Append("function doPreView"+ newvalue[2] + "() { \r\n");
                                        sb_script.Append("function doPreView" + funcId + "() { \r\n");
                                        sb_script.Append("var data ={ \r\n");
                                        sb_script.Append("'FILENAME':'" + file1 + "', \r\n");
                                        sb_script.Append("'APP_ID':'" + file2 + "', \r\n");
                                        sb_script.Append("'FILE_NO':'" + file3 + "', \r\n");
                                        sb_script.Append("'SRC_NO':'" + file4 + "', \r\n");
                                        sb_script.Append("} \r\n");
                                        sb_script.Append("var url =\"/PreView/PreView?FILENAME=" + file1 + "&APP_ID=" + file2 + "&FILE_NO=" + file3 + "&SRC_NO=" + file4 + "\" \r\n");
                                        sb_script.Append(" window.open (url, '_blank'); \r\n");
                                        sb_script.Append("} \r\n");



                                        sb_script.Append("function doPreViewShow" + newvalue[2] + "(pdf) { \r\n");
                                        sb_script.Append("var newTab = window.open(); \r\n newTab.document.body.innerHTML = \"<iframe width='100%' height='100%' src ='data:application/pdf;base64, \" + pdf + \"'></iframe> \"; \r\n}");

                                        //sb_script.Append("function doPreViewShow" + newvalue[2] +"_blank() { \r\n");
                                        //sb_script.Append("var data ={ \r\n");
                                        //sb_script.Append("'FILENAME':'" + file1 + "', \r\n");
                                        //sb_script.Append("'APP_ID':'" + file2 + "', \r\n");
                                        //sb_script.Append("'FILE_NO':'" + file3 + "', \r\n");
                                        //sb_script.Append("'SRC_NO':'" + file4 + "', \r\n");
                                        //sb_script.Append("} \r\n");
                                        //sb_script.Append("var url =\"/PreView/PreViewShow?FILENAME=" + file1 + "&APP_ID=" + file2 + "&FILE_NO=" + file3 + "&SRC_NO=" + file4 + "\" \r\n");
                                        //sb_script.Append(" window.open (url, '_blank'); \r\n");
                                        //sb_script.Append("} \r\n");

                                        sb_script.Append("</script> \r\n");

                                        sb.Append(sb_script);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}