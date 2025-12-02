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
using System.Collections;
using EECOnline.Models;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML 附件 輸入框產生輔助方法類別
    /// </summary>
    public static class FileDownloadExtension
    {
        /// <summary>HTML 附件單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="IsBackmin">前後台判斷(Y:後台,N:前台)</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString FileDLForTurbo<TModel, T>(this HtmlHelper<TModel> htmlHelper, T Model, string apy_id, string apy_main_key, string apy_src_key, string apy_other_key="")
        {
            FrontDAO dao = new FrontDAO();
            var IdName = "file_" + apy_main_key + "_" + apy_src_key;

            string[] validExts = new string[] { "pdf"};
            // HTML標籤
            StringBuilder sb = new StringBuilder();
            StringBuilder sb_script = new StringBuilder();

            String funcId = Guid.NewGuid().ToString().Replace("-", "");
            Hashtable parm = new Hashtable();
            parm["apy_id"] = apy_id;
            parm["apy_main_key"] = apy_main_key;
            parm["apy_src_key"] = apy_src_key;
            parm["apy_other_key"] = apy_other_key;
            var result = dao.QueryForObject<PreviewFileModel>("Front.queryPreviewFile", parm);
            if (result != null)
            {
                if (System.IO.File.Exists(result.apy_file_path))
                {
                    sb.Append("<p class=\"form-control-plaintext\">");
                    sb.Append("<a href=\"/GetZip/GetFile?apy_id="+ apy_id + "&apy_main_key="+ apy_main_key + "&apy_src_key="+ apy_src_key + "\" target=\"_blank\"> ");
                    sb.Append(result.apy_filename+"."+ result.apy_src_extion);
                    sb.Append("</a>");
                    sb.Append("<img src=\"/images/icon-check.svg\" alt=\"check\" style=\"width:20px;margin-left: 10px;\">");
                    sb.Append("</p>");
                    if (MimeMapping.GetMimeMapping(result.apy_file_path) == "image/jpeg"||MimeMapping.GetMimeMapping(result.apy_file_path) == "image/png")
                    {
                        byte[] imageArray = System.IO.File.ReadAllBytes(result.apy_file_path);
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        sb.Append("<a target='_blank' id='" + IdName + @"_HoverImg_Link' onclick='img_blank_dy" + IdName + @"(this)'> ");
                        //圖檔
                        var showContent = "<img id='{0}_HoverImg' width='100px' src='data:{1};base64,{2}' />";
                        sb.Append(string.Format(showContent, IdName, MimeMapping.GetMimeMapping(result.apy_file_path).Split('/'), base64ImageRepresentation));
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
                            if (string.Compare(result.apy_src_extion, item, true) == 0)
                            {
                                byte[] pdfArray = System.IO.File.ReadAllBytes(result.apy_file_path);
                                string base64PdfRepresentation = Convert.ToBase64String(pdfArray);
                                sb.AppendFormat("<button type='button' onclick='doPreViewShow{0} (\"{1}\")' style='margin-left:20px'>PDF預覽</button>", apy_id, base64PdfRepresentation);
                                sb.Append(" <br/>");
                                sb_script = new StringBuilder();
                                sb_script.Append("<script type = \"text/javascript\" > \r\n");
                                //sb_script.Append("function doPreView"+ newvalue[2] + "() { \r\n");
                                sb_script.Append("function doPreView" + funcId + "() { \r\n");
                                sb_script.Append("var data ={ \r\n");
                                sb_script.Append("'apy_id':'" + apy_id + "', \r\n");
                                sb_script.Append("'apy_main_key':'" + apy_main_key + "', \r\n");
                                sb_script.Append("'apy_src_key':'" + apy_src_key + "', \r\n");
                                sb_script.Append("} \r\n");
                                sb_script.Append("var url =\"/PreView/PreView?FILENAME=" + "\" \r\n");
                                sb_script.Append(" window.open (url, '_blank'); \r\n");
                                sb_script.Append("} \r\n");

                                sb_script.Append("function doPreViewShow" + apy_id + "(pdf) { \r\n");
                                sb_script.Append("var newTab = window.open(); \r\n newTab.document.body.innerHTML = \"<iframe width='100%' height='100%' src ='data:application/pdf;base64, \" + pdf + \"'></iframe> \"; \r\n}");

                                sb_script.Append("</script> \r\n");

                                sb.Append(sb_script);
                            }
                        }
                    }

                }
            }
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}