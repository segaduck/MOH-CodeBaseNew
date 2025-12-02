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

namespace ES.Helpers
{
    /// <summary>
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class ImageHoverExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ImageHoverForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            string expression_TEXT,
            int index,
            object htmlAttributes = null, bool enabled = true, string SRV_ID = "")
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();
            if (!string.IsNullOrEmpty(expression_TEXT))
            {
                Regex regex = new Regex(expression.Parameters[0] + ".");
                string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);
                ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
                string labelText = metadata.DisplayName;
                string IdName = propertyName.ToSplit('.')[0] + "[" + index.ToString() + "]." + metadata.PropertyName;
                string functionName = propertyName.ToSplit('.')[0] + index.ToString() + "_" + metadata.PropertyName;
                if (string.IsNullOrEmpty(labelText))
                {
                    labelText = propertyName;
                }
                if (SRV_ID == "011004")
                {
                    // 社工司證書(英文)
                    IdName = propertyName;
                    functionName = propertyName;
                }

                // HTML標籤
                //sb.Append("<span data-hover=\"<img id='" + IdName + "_HoverImg'>\" class=\"hover\" onmouseover=\"Get" + functionName + "ImgHover();\"><p class=\"form-control-static\">" + expression_TEXT + "</p></span>");

                var extname = expression_TEXT.ToSplit('.').LastOrDefault().ToUpper();
                if (extname == "GIF" || extname == "JPG" || extname == "JPEG" || extname == "TIF" || extname == "PNG")
                {
                    sb.Append("<span><p class=\"form-control-static\">");
                    //sb.Append("<span><p class=\"form-control-static\">" + expression_TEXT + "</p></span>");
                    sb.Append("<a target='_blank' id='" + functionName + @"_HoverImg_Link1' onclick='img_blank_dy" + functionName + @"1(this)'>");
                    sb.Append("<img id='" + functionName + "_HoverImg1' width='100px' alt='" + labelText + @"' style='display:none'>");
                    sb.Append(expression_TEXT);
                    sb.Append(" </a>");
                    sb.Append("</p></span>");

                    sb.Append(@"<a target='_blank' id='" + functionName + @"_HoverImg_Link' onclick='img_blank_dy" + functionName + @"(this)'>
                                <img id='" + functionName + "_HoverImg' width='100px' alt='" + labelText + @"'>                               
                            </a>");
                }
                else if (extname == "PDF")
                {
                    var PdfName = "<div class='form-inline'><p class=\"form-control-static\">" + expression_TEXT + "</p></div>";
                    //sb.Append(@"<iframe id='" + IdName + @"_HoverImg' width='200px'></iframe>");
                    sb.Append("<a target='_blank' id='" + functionName + @"_HoverImgButton' onclick='pdf_blank" + functionName + @"1()' style='margin-left:20px'>" + PdfName + "</a>");
                    //sb.Append(@"</div>");

                    //sb.Append("<div class='form-inline'><p class=\"form-control-static\">" + expression_TEXT + "</p>");
                    sb.Append(@"<button id='" + functionName + @"_HoverImgButton1' type='button' onclick='pdf_blank" + functionName + @"()' style='margin-left:20px'>預覽PDF</button>");
                    //sb.Append(@"</div>");
                }
                else
                {
                    var PdfName = "<div class='form-inline'><p class=\"form-control-static\">" + expression_TEXT + "</p></div>";
                    sb.Append("<a target='_blank' id='" + functionName + @"_HoverImgButton' onclick='pdf_blank" + functionName + @"1()' style='margin-left:20px'>" + PdfName + "</a>");
                }
                sb_script.Append(@"<script type='text/javascript'>
                                    function Get" + functionName + @"ImgHover() {
                                        if (window.FileReader) {
                                            var fileElm = $('input[name=""" + IdName + @"""][type=""file""]');
                                            if(fileElm[0]==undefined){return;}
                                            var file = fileElm[0].files[0];
                                            if(file!=undefined){
                                                var filename = fileElm[0].files[0].name;
                                                var fileExt = filename.substring(filename.lastIndexOf('.'));
                                                var validImgExts = new Array('.jpg', '.jpeg', '.bmp', '.png', '.gif');
                                                if (validImgExts.indexOf(fileExt) > -1){
                                                    var fr = new FileReader(); 
				                                    var showimg = document.getElementById('" + functionName + @"_HoverImg');
                                                    var showimg1 = document.getElementById('" + functionName + @"_HoverImg1');
                                                    fr.onloadend = function () {
                                                        showimg.src = this.result;
                                                        showimg1.src = this.result;
                                                    }
                                                    fr.readAsDataURL(file);
                                                }
                                                else if(fileExt.toUpperCase() == '.PDF')
                                                {
                                                    var fr = new FileReader();
                                                    fr.onloadend = function () {
                                                        console.log(this.result);
                                                       // showimg.src = this.result;
                                                        $('#" + functionName + @"_HoverImgButton').attr('onclick',""pdf_blank" + functionName + @"1('""+this.result+""')"");
                                                        $('#" + functionName + @"_HoverImgButton1').attr('onclick',""pdf_blank" + functionName + @"('""+this.result+""')"");
                                                    }
                                                    fr.readAsDataURL(file);
                                                }     
                                            }
                                        }
                                    }

                                    function img_blank_dy" + functionName + @"(img){
                                        var newTab = window.open();
                                        var imgData = img.children[0].src;
                                        newTab.document.body.innerHTML = ""<img src="" + imgData + "">"";
                                    }
                                    function pdf_blank" + functionName + @"(src){
                                        var newTab = window.open();
                                        //var imgData = $('#" + IdName + @"_HoverImg').attr('src');
                                        newTab.document.body.innerHTML = "" <iframe width='100%' height='100%' src = "" + src + ""></iframe> ""; }

                                    function img_blank_dy" + functionName + @"1(img){
                                        var element = document.createElement('a');
                                        var imgData = img.children[0].src;
                                        element.setAttribute('href', encodeURI(imgData));
                                        element.setAttribute('download', '" + expression_TEXT + @"');
                                        element.style.display = 'none';
                                        document.body.appendChild(element);
                                        element.click();
                                        document.body.removeChild(element);
                                    }
                                    function pdf_blank" + functionName + @"1(src){
                                        var element = document.createElement('a');
                                        var imgData =src;
                                        element.setAttribute('href', encodeURI(imgData));
                                        element.setAttribute('download', '" + expression_TEXT + @"');
                                        element.style.display = 'none';
                                        document.body.appendChild(element);
                                        element.click();
                                        document.body.removeChild(element);

                                        //var newTab = window.open();
                                        //var imgData = $('#" + IdName + @"_HoverImg').attr('src');
                                        //newTab.document.body.innerHTML = "" <iframe width='100%' height='100%' src = "" + src + ""></iframe> ""; 
                                    }

                                    Get" + functionName + @"ImgHover();
                               </script>");
            }
            //sb_script.Append("<script type = \"text/javascript\" > \r\n");
            //sb_script.Append("function Get" + functionName + "ImgHover() { \r\n");
            //sb_script.Append(" if (window.FileReader) { \r\n");
            //sb_script.Append("var fileElm = $('input[name=\"" + IdName + "\"][type=\"file\"]') \r\n");
            //sb_script.Append("var file = fileElm[0].files[0]; \r\n");
            //sb_script.Append("if(file!=undefined){ \r\n");
            //sb_script.Append("var filename = fileElm[0].files[0].name; \r\n");
            //sb_script.Append("var fileExt = filename.substring(filename.lastIndexOf('.')); \r\n");
            //sb_script.Append("var validImgExts = new Array(\".jpg\", \".bmp\", \".png\", \".gif\"); \r\n");
            //sb_script.Append("if (validImgExts.indexOf(fileExt) > -1){ \r\n");
            //sb_script.Append("var fr = new FileReader(); \r\n");
            //sb_script.Append(" var showimg = document.getElementById('" + IdName + "_HoverImg'); \r\n");
            //sb_script.Append(" fr.onloadend = function (e) { \r\n");
            //sb_script.Append("showimg.src = e.target.result; \r\n");
            //sb_script.Append("}; \r\n");
            //sb_script.Append(" fr.readAsDataURL(file); \r\n");
            //sb_script.Append("} \r\n");
            //sb_script.Append("} \r\n");
            //sb_script.Append("} \r\n");
            //sb_script.Append("} \r\n");

            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ImageHoverForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            string expression_TEXT,
            object htmlAttributes = null, bool enabled = true)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();
            if (!string.IsNullOrEmpty(expression_TEXT))
            {
                Regex regex = new Regex(expression.Parameters[0] + ".");
                string propertyName = regex.Replace(expression.Body.ToString(), string.Empty, 1);
                ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
                string labelText = metadata.DisplayName;
                string IdName = metadata.PropertyName;
                var value = Convert.ToString(metadata.Model).ToLower();
                var valueRe = value.Replace('\\', '/');
                var valueList = valueRe.Split('/');
                if (string.IsNullOrEmpty(labelText))
                {
                    labelText = propertyName;
                }
                var extname = expression_TEXT.ToSplit('.').LastOrDefault().ToUpper();
                // HTML標籤
                if (extname != "PDF")
                {
                    sb.Append("<span><p class=\"form-control-static\">");
                    //sb.Append("<span><p class=\"form-control-static\">" + expression_TEXT + "</p></span>");
                    sb.Append("<a target='_blank' id='" + IdName + @"_HoverImg_Link' onclick='img_blank" + IdName + @"1()'>");
                    sb.Append(expression_TEXT);
                    sb.Append(" </a>");
                    sb.Append("</p></span>");
                }
                else
                {
                    var PdfName = "<div class='form-inline'><p class=\"form-control-static\">" + expression_TEXT + "</p></div>";
                    //sb.Append(@"<iframe id='" + IdName + @"_HoverImg' width='200px'></iframe>");
                    sb.Append("<a target='_blank' id='" + IdName + @"_HoverImgButton' onclick='pdf_blank" + IdName + @"1()' style='margin-left:20px'>" + PdfName + "</a>");            
                    //sb.Append(@"</div>");


                    //sb.Append("<div class='form-inline'><p class=\"form-control-static\">" + expression_TEXT + "</p>");
                }

                if (extname == "GIF" || extname == "JPG" || extname == "JPEG" || extname == "TIF" || extname == "PNG")
                {
                    sb.Append(@"<a target='_blank' id='" + IdName + @"_HoverImg_Link' onclick='img_blank" + IdName + @"()'>
                                <img id='" + IdName + "_HoverImg' width='100px' alt='" + labelText + @"'>                               
                            </a>");
                }
                else if (extname == "PDF")
                {
                    //sb.Append(@"<iframe id='" + IdName + @"_HoverImg' width='200px'></iframe>");
                    //sb.Append("<br/>");
                    sb.Append(@"<button id='" + IdName + @"_HoverImgButton1' type='button' onclick='pdf_blank" + IdName + @"()' style='margin-left:20px'>預覽PDF</button>");
                    //sb.Append(@"</div>");
                }
                sb_script.Append(@"<script type='text/javascript'>
                                    function Get" + IdName + @"ImgHover() {
                                        if (window.FileReader) {
                                            var fileElm = $('input[id=""" + IdName + @"""][type=""file""]');
                                            if(fileElm[0]==undefined){return;}
                                            var file = fileElm[0].files[0];
                                            if(file!=undefined){
                                                var filename = fileElm[0].files[0].name;
                                                var fileExt = filename.substring(filename.lastIndexOf('.'));
                                                var validImgExts = new Array('.JPG', '.JPEG', '.BMP', '.PNG', '.GIF');
                                                var showimg = document.getElementById('" + IdName + @"_HoverImg');

                                                if (validImgExts.indexOf(fileExt.toUpperCase()) > -1){
                                                    var fr = new FileReader();
                                                    fr.onloadend = function () {
                                                        showimg.src = this.result;
                                                    }
                                                    fr.readAsDataURL(file);
                                                }
                                                else if(fileExt.toUpperCase() == '.PDF')
                                                {
                                                    var fr = new FileReader();
                                                    fr.onloadend = function () {
                                                        console.log(this.result);
                                                       // showimg.src = this.result;
                                                        $('#" + IdName + @"_HoverImgButton').attr('onclick',""pdf_blank" + IdName + @"1('""+this.result+""')"");
                                                        $('#" + IdName + @"_HoverImgButton1').attr('onclick',""pdf_blank" + IdName + @"('""+this.result+""')"");
                                                    }
                                                    fr.readAsDataURL(file);
                                                }                                                  
                                            }
                                            else {
                                                    showimg.innerHTML = '<p class=""form-control-static"">" + expression_TEXT + @"</p>';
                                                }
                                        }
                                    }

                                    function img_blank" + IdName + @"(){
                                        var newTab = window.open();
                                        var imgData = $('#" + IdName + @"_HoverImg').attr('src');
                                        newTab.document.body.innerHTML = ""<img src="" + imgData + "">"";
                                    }
                                    function pdf_blank" + IdName + @"(src){
                                        var newTab = window.open();
                                        //var imgData = $('#" + IdName + @"_HoverImg').attr('src');
                                        newTab.document.body.innerHTML = "" <iframe width='100%' height='100%' src = "" + src + ""></iframe> ""; }

                                    function img_blank" + IdName + @"1(){
                                        var element = document.createElement('a');
                                        var imgData = $('#" + IdName + @"_HoverImg').attr('src');
                                        element.setAttribute('href', encodeURI(imgData));
                                        element.setAttribute('download', '"+ expression_TEXT + @"');
                                        element.style.display = 'none';
                                        document.body.appendChild(element);
                                        element.click();
                                        document.body.removeChild(element);
                                        //var newTab = window.open();
                                        // var imgData = $('#" + IdName + @"_HoverImg').attr('src');
                                        // newTab.document.body.innerHTML = ""<img src="" + imgData + "">"";
                                    }
                                    function pdf_blank" + IdName + @"1(src){
                                        var element = document.createElement('a');
                                        var imgData =src;
                                        element.setAttribute('href', encodeURI(imgData));
                                        element.setAttribute('download', '" + expression_TEXT + @"');
                                        element.style.display = 'none';
                                        document.body.appendChild(element);
                                        element.click();
                                        document.body.removeChild(element);

                                        //var newTab = window.open();
                                        //var imgData = $('#" + IdName + @"_HoverImg').attr('src');
                                        //newTab.document.body.innerHTML = "" <iframe width='100%' height='100%' src = "" + src + ""></iframe> ""; 
                                    }

                                    Get" + IdName + @"ImgHover();
                                </script>");
            }


            //sb_script.Append("<script type = \"text/javascript\" > \r\n");

            ////sb_script.Append("$(document).ready(function () { \r\n");
            ////sb_script.Append("$('[data-hover!=\"\"]').hover(); \r\n");
            ////sb_script.Append("}); \r\n");

            //sb_script.Append("function Get" + IdName + "ImgHover() { \r\n");
            //sb_script.Append(" if (window.FileReader) { \r\n");
            //sb_script.Append("var fileElm = $('input[id=\"" + IdName + "\"][type=\"file\"]') \r\n");
            //sb_script.Append("var file = fileElm[0].files[0]; \r\n");
            //sb_script.Append("if(file!=undefined){ \r\n");
            //sb_script.Append("var filename = fileElm[0].files[0].name; \r\n");
            //sb_script.Append("var fileExt = filename.substring(filename.lastIndexOf('.')); \r\n");
            //sb_script.Append("var validImgExts = new Array(\".jpg\", \".bmp\", \".png\", \".gif\"); \r\n");
            //sb_script.Append("if (validImgExts.indexOf(fileExt) > -1){ \r\n");
            //sb_script.Append("var fr = new FileReader(); \r\n");
            //sb_script.Append(" var showimg = document.getElementById('" + IdName + "_HoverImg'); \r\n");
            //sb_script.Append(" fr.onloadend = function (e) { \r\n");
            //sb_script.Append("showimg.src = e.target.result; \r\n");
            //sb_script.Append("}; \r\n");
            //sb_script.Append(" fr.readAsDataURL(file); \r\n");
            //sb_script.Append("} \r\n");
            //sb_script.Append("} \r\n");
            //sb_script.Append("} \r\n");
            //sb_script.Append("} \r\n");

            //sb_script.Append("</script> \r\n");
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}