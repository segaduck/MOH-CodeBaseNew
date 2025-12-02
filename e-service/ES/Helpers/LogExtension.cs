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
using ES.Models;

namespace ES.Helpers
{
    /// <summary>
    /// HTML 顏色 輸入框產生輔助方法類別
    /// </summary>
    public static class LogExtension
    {
        /// <summary>HTML 顏色單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        public static MvcHtmlString LogForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper, string APP_ID, string TableN)
        {
            BackApplyDAO dao = new BackApplyDAO();
            var TableC = TableN.ToSplit(',');
            var TableD = TableC.Select(m => m + "_LOG").ToList();
            IList<LogModel> LogDB = dao.GetLog(APP_ID, TableD);
            IList<TransLogModel> TransLog = dao.GetTransLog(APP_ID);
            IList<TransLogModel> NoticeLog = dao.GetNoticeLog(APP_ID);

            StringBuilder sb = new StringBuilder();
            foreach (var item in LogDB)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + item.CODE_DESC + "(" + item.MODTIME.TransDateTime() + ")</td>");
                foreach (var t_item in TransLog.Where(t => t.MODTIME == item.MODTIME))
                {
                    sb.Append("<td>" + t_item.DESC1 + "</td>");
                }
                foreach (var t_item in NoticeLog.Where(t => t.MODTIME == item.MODTIME))
                {
                    sb.Append("<td>" + t_item.DESC1 + "</td>");
                }
                sb.Append("</tr><br/>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

    }
}