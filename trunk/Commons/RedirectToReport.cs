using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Turbo.ReportTK;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EECOnline.Commons
{
    /// <summary>將 HTTP 請求導向到另一個 MVC 控制器方法結果</summary>
    /// <remarks>
    /// 本類別用來解決將 HTTP 請求導向到另一個 MVC 控制器動作方法時，瀏覽器網址列會顯示另一個 MVC 控制器動作網址。
    /// 在 MVC 控制器動作之間的參數值傳遞一律使用 MVC 控制器的 TempData 物件，以避免在瀏覽器網址列上顯示參數名稱與參數值無法通過網站安全性檢測。
    /// </remarks>
    public class RedirectToReport : ActionResult
    {
        #region 物件屬性
        /// <summary>報表請求來源功能編號。</summary>
        public string Caller { get; set; } = "";

        /// <summary>在 Report.config 檔案內的報表設定項目 id</summary>
        public string ReportId { get; set; } = "";

        /// <summary>報表類型。（PDF: PDF 格式報表，XLS: Excel 格式報表，WORD: Word 格式報表）</summary>
        public string ReportType { get; set; } = "";

        /// <summary>在產出報表之前要呼叫的自訂報表資料加工處理方法。null 表示不需要</summary>
        public Action<Report> CustomDataHandler { get; set; }

        /// <summary>報表結果檔案自訂處理方法。null 表示不需要</summary>
        public Func<ActionResult, ActionResult> ResultFileHandler { get; set; }

        /// <summary>
        /// 是否使用 HTTP POST 方式執行請求導向。
        /// （null: 依照原先 HTTP 方式，true: 使用 HTTP POST 方式，false: 使用 HTTP GET 方式 (可能會無法通過網站安全性檢測)）
        /// </summary>
        public bool? UseHttpPost { get; set; } = true;

        /// <summary>要傳給另一個 MVC 控制器動作的路由資料。</summary>
        public object RouteValues { get; set; }
        #endregion

        #region 預設建構子
        /// <summary>
        /// 將 HTTP 請求導向到系統報表生成方法結果。
        /// 本方式會固定傳送兩個參數（isFromRedirect: 是否為別支功能導向來的（Y: 是, "": 否），caller: HTTP 請求導向來源功能編號）給目的地 Controller 方法。
        /// 請在目的地 Controller 方法接收這兩個傳入參數（範例：Index(string isFromRedirect = "", string caller = "")）以便在程式判斷應用。
        /// </summary>
        /// <param name="reportId">在 Report.config 檔案內的報表設定項目 id</param>
        /// <param name="reportType">報表類型。（PDF: PDF 格式報表，XLS: Excel 格式報表，WORD: Word 格式報表）</param>
        /// <param name="customDataHandler">（非必要）在產出報表之前要呼叫的自訂報表資料加工處理方法。null 表示不需要</param>
        /// <param name="routeValues">（非必要）MVC 控制器動作的路由資料</param>
        /// <param name="useHttpPost">（非必要）是否使用 HTTP POST 方式執行導向。（null: 依照原先請求方式，true: 使用 HTTP POST 方式，false: 使用 HTTP GET 方式 (可能會無法通過網站安全性檢測)）</param>
        public RedirectToReport(string reportId, string reportType, Action<Report> customDataHandler = null, object routeValues = null, bool? useHttpPost = null) : base()
        {
            ReportId = reportId;
            CustomDataHandler = customDataHandler;
            ResultFileHandler = null;
            ReportType = reportType;
            RouteValues = routeValues;
            UseHttpPost = useHttpPost;
        }

        /// <summary>
        /// 將 HTTP 請求導向到系統報表生成方法結果。
        /// 本方式會固定傳送兩個參數（isFromRedirect: 是否為別支功能導向來的（Y: 是, "": 否），caller: HTTP 請求導向來源功能編號）給目的地 Controller 方法。
        /// 請在目的地 Controller 方法接收這兩個傳入參數（範例：Index(string isFromRedirect = "", string caller = "")）以便在程式判斷應用。
        /// </summary>
        /// <param name="reportId">在 Report.config 檔案內的報表設定項目 id</param>
        /// <param name="reportType">報表類型。（PDF: PDF 格式報表，XLS: Excel 格式報表，WORD: Word 格式報表）</param>
        /// <param name="customDataHandler">在產出報表之前要呼叫的自訂報表資料加工處理方法。null 表示不需要</param>
        /// <param name="resultFileHandler">報表結果檔案自訂處理方法。null 表示不需要</param>
        /// <param name="routeValues">（非必要）MVC 控制器動作的路由資料</param>
        /// <param name="useHttpPost">（非必要）是否使用 HTTP POST 方式執行導向。（null: 依照原先請求方式，true: 使用 HTTP POST 方式，false: 使用 HTTP GET 方式 (可能會無法通過網站安全性檢測)）</param>
        public RedirectToReport(string reportId, string reportType, Action<Report> customDataHandler, Func<ActionResult, ActionResult> resultFileHandler, object routeValues = null, bool? useHttpPost = null) : base()
        {
            ReportId = reportId;
            CustomDataHandler = customDataHandler;
            ResultFileHandler = resultFileHandler;
            ReportType = reportType;
            RouteValues = routeValues;
            UseHttpPost = useHttpPost;
        }
        #endregion

        /// <summary>執行 HTTP 請求導向到另一個 MVC 控制器動作</summary>
        /// <param name="context">MVC 控制器物件。</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            var urlHelper = new UrlHelper(context.RequestContext);

            //取得「畫面導向來源功能編號」
            if (string.IsNullOrEmpty(Caller))
            {
                var actName = Convert.ToString(context.RouteData.Values["action"]);
                var ctlName = Convert.ToString(context.RouteData.Values["controller"]);
                Caller = urlHelper.Action(actName, ctlName);
                if (Caller[0] == '/') Caller = Caller.Substring(1);
            }

            //取得「目的功能編號」的 MVC 連結網址
            var routeData = new RouteValueDictionary(RouteValues);
            routeData.Add("area", "");
            routeData.Add("isFromRedirect", "Y");
            routeData.Add("caller", Caller);
            routeData.Add("rptId", ReportId);
            routeData.Add("reportType", ReportType);

            var url = urlHelper.Action("ReportFromRedirect", "Report", routeData);
            if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("目的功能編號網址不可為空字串。");

            //將「自訂報表資料處理方法」暫存到 Session 內
            var httpContext = context.HttpContext;
            var itemKey = string.Concat("_", Caller, ".RPT.CustomDataHandler_");
            httpContext.Session[itemKey] = CustomDataHandler;

            //將「報表結果檔案自訂處理方法」暫存到 Session 內
            itemKey = string.Concat("_", Caller, ".RPT.ResultFileHandler_");
            httpContext.Session[itemKey] = ResultFileHandler;

            //執行 HTTP 請求轉向
            string method = (UseHttpPost.HasValue) ? ((UseHttpPost == true) ? "POST" : "GET") : null;
            httpContext.Server.TransferRequest(url, true, method, null);
        }
    }
}