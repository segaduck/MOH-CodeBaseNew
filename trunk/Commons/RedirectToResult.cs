using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EECOnline.Commons
{
    /// <summary>將 HTTP 請求導向到另一個 MVC 控制器方法結果</summary>
    /// <remarks>
    /// 本類別用來解決將 HTTP 請求導向到另一個 MVC 控制器動作方法時，瀏覽器網址列會顯示另一個 MVC 控制器動作網址。
    /// 在 MVC 控制器動作之間的參數值傳遞一律使用 MVC 控制器的 TempData 物件，以避免在瀏覽器網址列上顯示參數名稱與參數值無法通過網站安全性檢測。
    /// </remarks>
    public class RedirectToResult : ActionResult
    {
        #region 靜態屬性與方法
        /// <summary>HTTP Cookie/Header 名稱：HTTP 請求導向來源功能編號</summary>
        private static string XRedirectFromLabel {
            get { return "X-RedirectFrom"; }
        }

        /// <summary>取得在 HTTP Header/Cookie 內的「HTTP 請求導向來源功能編號」。</summary>
        /// <param name="request">HTTP 請求物件</param>
        /// <returns></returns>
        public static string GetRedirectFromLabel(HttpRequestBase request)
        {
            string v = request.Headers[XRedirectFromLabel];
            if (string.IsNullOrEmpty(v))
            {
                var cookie = request.Cookies[XRedirectFromLabel];
                if (cookie != null) v = cookie.Value;
            }
            return v;
        }

        /// <summary>
        /// 將「HTTP 請求導向來源功能編號」暫存在 HTTP Cookie 內
        /// </summary>
        /// <param name="response">HTTP 回應物件</param>
        /// <param name="prgid">HTTP 請求導向來源功能編號</param>
        public static void KeepRedirectFromLabel(HttpResponseBase response, string prgid)
        {
            response.Cookies.Set(new HttpCookie(RedirectToResult.XRedirectFromLabel, prgid));
        }

        /// <summary>
        /// 將「HTTP 請求導向來源功能編號」從 HTTP Cookie 內移除
        /// </summary>
        /// <param name="response">HTTP 回應物件</param>
        /// <param name="prgid">HTTP 請求導向來源功能編號</param>
        public static void RemoveRedirectFromLabel(HttpResponseBase response)
        {
            response.Cookies.Remove(XRedirectFromLabel);
        }
        #endregion

        #region 物件屬性
        /// <summary>畫面導向來源功能編號。</summary>
        public string Caller { get; set; } = "";

        /// <summary>MVC Area 名稱。</summary>
        public string AreaName { get; set; } = "";

        /// <summary>MVC Action 動作名稱。</summary>
        public string ActionName { get; set; } = "";

        /// <summary>MVC Controller 控制器名稱。</summary>
        public string ControllerName { get; set; } = "";

        /// <summary>
        /// 是否使用 HTTP POST 方式執行請求導向。
        /// （null: 依照原先 HTTP 方式，true: 使用 HTTP POST 方式，false: 使用 HTTP GET 方式 (可能會無法通過網站安全性檢測)）
        /// </summary>
        public bool? UseHttpPost { get; set; }

        /// <summary>要傳給另一個 MVC 控制器動作的路由資料。</summary>
        public object RouteValues { get; set; }
        #endregion

        #region 預設建構子
        /// <summary>
        /// 將 HTTP 請求導向到另一個 MVC 控制器方法結果。
        /// 本方式會固定傳送兩個參數（isFromRedirect: 是否為別支功能導向來的（Y: 是, "": 否），caller: HTTP 請求導向來源功能編號）給目的地 Controller 方法。
        /// 請在目的地 Controller 方法接收這兩個傳入參數（範例：Index(string isFromRedirect = "", string caller = "")）以便在程式判斷應用。
        /// </summary>
        /// <param name="actionName">MVC Action 動作名稱</param>
        /// <param name="controllerName">MVC Controller 控制器名稱</param>
        /// <param name="routeValues">（非必要）MVC 控制器動作的路由資料</param>
        /// <param name="useHttpPost">（非必要）是否使用 HTTP POST 方式執行導向。（null: 依照原先請求方式，true: 使用 HTTP POST 方式，false: 使用 HTTP GET 方式 (可能會無法通過網站安全性檢測)）</param>
        public RedirectToResult(string actionName, string controllerName, object routeValues = null, bool? useHttpPost = null) : base()
        {
            ActionName = actionName;
            ControllerName = controllerName;
            RouteValues = routeValues;
            UseHttpPost = useHttpPost;
        }
        
        /// <summary>
        /// 將 HTTP 請求導向到另一個 MVC 控制器方法結果。
        /// 本方式會固定傳送兩個參數（isFromRedirect: 是否為別支功能導向來的（Y: 是, "": 否），caller: HTTP 請求導向來源功能編號）給目的地 Controller 方法。
        /// 請在目的地 Controller 方法接收這兩個傳入參數（範例：Index(string isFromRedirect = "", string caller = "")）以便在程式判斷應用。
        /// </summary>
        /// <param name="actionName">MVC Action 動作名稱</param>
        /// <param name="controllerName">MVC Controller 控制器名稱</param>
        /// <param name="areaName">MVC Area 領域名稱</param>
        /// <param name="routeValues">（非必要）MVC 控制器動作的路由資料</param>
        /// <param name="useHttpPost">（非必要）是否使用 HTTP POST 方式執行導向。（null: 依照原先請求方式，true: 使用 HTTP POST 方式，false: 使用 HTTP GET 方式 (可能會無法通過網站安全性檢測)）</param>
        public RedirectToResult(string actionName, string controllerName, string areaName, 
                                    object routeValues = null, bool? useHttpPost = null) : base()
        {
            ActionName = actionName;
            ControllerName = controllerName;
            AreaName = areaName;
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
                Caller = Turbo.Commons.ControllerContextHelper.GetActionPath(context);
                string[] tokens = Caller.Split('/');
                if(tokens.Length > 2)
                {
                    Caller = tokens[0] + "/" + tokens[1];
                }
            }

            //取得「目的功能編號」的 MVC 連結網址
            var routeData = new RouteValueDictionary(RouteValues);
            if (!string.IsNullOrEmpty(AreaName)) routeData.Add("area", AreaName);
            routeData.Add("isFromRedirect", "Y");
            routeData.Add("caller", Caller);

            var url = urlHelper.Action(ActionName, ControllerName, routeData);
            if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("目的功能編號網址不可為空字串。");

            //執行 HTTP 請求轉向
            string method = (UseHttpPost.HasValue) ? ((UseHttpPost == true) ? "POST" : "GET") : null;
            var httpContext = context.HttpContext;
            httpContext.Request.Headers.Set(XRedirectFromLabel, Caller);
            httpContext.Request.Cookies.Set(new HttpCookie(XRedirectFromLabel, Caller));
            httpContext.Server.TransferRequest(url, true, method, null);
        }
    }
}