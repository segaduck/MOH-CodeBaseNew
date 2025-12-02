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
using System.Collections;
using EECOnline.Services;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Utils;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using Turbo.Commons;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML 歷程 產生輔助方法類別
    /// </summary>
    public static class LogExtension
    {
        /// <summary>HTML 歷程產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        public static MvcHtmlString LogForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper, string apy_id, string num = null)
        {
            BackApplyDAO dao = new BackApplyDAO();

            Hashtable parm = new Hashtable();
            parm["apy_id"] = apy_id;
            var result = dao.QueryForList<Hashtable>("BackApply.queryLog", parm);
            StringBuilder sb = new StringBuilder();
            var result1 = dao.QueryForObject<Hashtable>("BackApply.queryLogNow", parm);

            if (apy_id.Substring(8,6) == "001008")
            {
                //判斷歇業或執業
               if (num == "1")
                {
                    result = dao.QueryForList<Hashtable>("BackApply.queryLog_BE", parm);
                    result1 = dao.QueryForObject<Hashtable>("BackApply.queryLogNow_BE", parm);
                }
               else
                {
                    result = dao.QueryForList<Hashtable>("BackApply.queryLog_AF", parm);
                    result1 = dao.QueryForObject<Hashtable>("BackApply.queryLogNow_AF", parm);
                }
            }
            
            // 現狀
            if (result1 != null)
            {
                var time1 = (DateTime)HelperUtil.TransTwLongToDateTime(result1["modtime"].TONotNullString());
                var year1 = (time1.Year - 1911).ToString("000");
                var dt1 = time1.ToString("年MM月dd日");
                var tm1 = time1.ToString("HH:mm");
                var statusname1 = result1["statusname"];
                var modusername = result1["modusername"];
                if(modusername != null)
                {
                    sb.Append("<p class=\"form-control-plaintext\">" + statusname1 + "(" + year1 + dt1 + "  " + tm1+ " -" + modusername + ")" + "</p>");
                }
                else {sb.Append("<p class=\"form-control-plaintext\">" + statusname1 + "(" + year1 + dt1 + "  " + tm1 + ")" + "</p>"); }
            }
            // 歷程
            foreach (var item in result)
            {
                var time = (DateTime)HelperUtil.TransTwLongToDateTime(item["modtime"].TONotNullString());
                var year = (time.Year - 1911).ToString("000");
                var dt = time.ToString("年MM月dd日");
                var tm = time.ToString("HH:mm");
                var statusname = item["statusname"];
                var modusername = item["modusername"];
                if (modusername != null)
                {
                    sb.Append("<p class=\"form-control-plaintext\">" + statusname + "(" + year + dt + "  " + tm + " -" + modusername + ")" + "</p>");
                }
                else { sb.Append("<p class=\"form-control-plaintext\">" + statusname + "(" + year + dt + "  " + tm +")" + "</p>"); }
            }
 
            return MvcHtmlString.Create(sb.ToString());
        }

    }
}