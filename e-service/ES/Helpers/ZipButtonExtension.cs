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
using ES.DataLayers;

namespace ES.Helpers
{
    /// <summary>
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class ZipButtonExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        public static MvcHtmlString ZipButtonForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper, string APP_ID,string CaseNAME)
        {
            BackApplyDAO dao = new BackApplyDAO();
            StringBuilder sb = new StringBuilder();


            sb.Append("  <div class='btnbar'>");

            sb.Append("  <button type='button' onclick='GetZip"+ APP_ID + "(\""+ APP_ID + "\")' class='btn btn-blue'>檔案打包下載</button>");

            sb.Append("  </div>");

            sb.Append("  <script type='text/javascript'>");
            sb.Append("   function GetZip"+ APP_ID + "(APP_ID) {");
            sb.Append("    var data = {");
            sb.Append("    'APP_ID': APP_ID, ");
            sb.Append("    'CASENAME': '"+ CaseNAME + "' ");
            sb.Append("    }; ");
            sb.Append("   var url = '/GetZIP/GetZIP?' + $.param(data);");
            sb.Append("    location.href = url;");
            sb.Append("   }");
            sb.Append("  </script>");
            return MvcHtmlString.Create(sb.ToString());
        }
    }
}