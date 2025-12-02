using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Principal;
using ES.Utils.Schedule;
using log4net;
using System.Text.RegularExpressions;
using ES.Services;
using System.IO;
using System.Text;

namespace ES
{
    // 注意: 如需啟用 IIS6 或 IIS7 傳統模式的說明，
    // 請造訪 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // 2018.11.05, Tony,比照舊網調整// 2018.02.05, Eric, Injection 檢查
            // 偵測到時直接回應: 404.19 Denied by filtering rule
            //if (SecurityHelper.HasInjection(Request))
            //{
            //    //下面這行需要程式集區設定為 Integrated Mode 才能運作
            //    //避免正式環境困擾, 去掉這一個設定
            //    //Response.SubStatusCode = 19; 
            //    Response.StatusCode = 404;
            //    Response.StatusDescription = "Denied by filtering rule";
            //    Response.End();
            //    return;
            //}
            //2021黑箱問題 (url為資料夾路徑時，強制轉為404)
            Regex rx = new Regex("^(/[s|S]cripts|/[l|L]ogin|/[w|W]eb References|/[v|V]iews|/[s|S]qlMaps|/[o|O]dsFile|/[m|M]odels|/[l|L]GIS|/[f|F]onts|/[t|T]est|/[s|S]ervices|/[l|L]ogs|/[h|H]ome|/[e|E]xcelConfig|/[d|D]oc|/[i|I]mages|/[c|C]ss|/[c|C]ontent|/[d|D]ownloads|[a|A]ssets|/[f|F]onts|/files|/[r|R]eport)[|/]$");
            if (rx.IsMatch(Request.Path))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            if (!Context.Request.IsSecureConnection)
            {
                if (Request.ServerVariables["SERVER_PORT"].Contains("80") || Request.ServerVariables["SERVER_PORT"].Contains("443"))
                {
                    string rqstr = "";
                    foreach (string key in Request.QueryString)
                    {
                        if (rqstr.Length > 0) { rqstr += "&"; }
                        rqstr += string.Format("{0}={1}", key, HttpUtility.UrlEncode(Request.QueryString[key]));
                    }
                    Response.Redirect("https://" + Request.Url.Host + Request.Url.AbsolutePath + (rqstr == "" ? "" : ("?" + rqstr)));
                    //Response.Redirect(Context.Request.Url.ToString().Trim().Replace("http://", "https://"));
                }
            }
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //讀取日誌  如果使用log4net,應用程序一開始的時候，都要進行初始化配置
            log4net.Config.XmlConfigurator.Configure();

            logger.Info("Application_Start");

            //20210223 新增  Hangfire WEB排程機制
            //ES.Models.HangFireTask.Instance.Start();
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            if (User != null && User.Identity.IsAuthenticated && User.Identity is FormsIdentity)
            {
                FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
                FormsAuthenticationTicket ticket = id.Ticket;
                //string userData = ticket.UserData;
                //string[] roles = userData.Split(',');
                Dictionary<string, string> userData = ES.Utils.DataUtils.JsonStringToDictionary(ticket.UserData);
                if (userData != null && userData.ContainsKey("Roles"))
                {
                    string[] roles = userData["Roles"].Split(',');
                    HttpContext.Current.User = new GenericPrincipal(id, roles);
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //發生未處理錯誤時執行的程式碼
            Exception objErr = Server.GetLastError();
            string err = "Application_Error event:\n"
                + "Error in: " + Request.Url.ToString()
                + "\nError Message:" + objErr.Message.ToString() + "\n";
            //+ "\nStack Trace:" + objErr.StackTrace.ToString() + "\n";

            logger.Error(err, objErr);
            // 檢查是否已經存在 reset.okgo 檔案
            string resetFilePath = @"C:\eWeb\SRC\rcReset\reset.okgo";
            if (objErr.Message.Contains("執行逾時到期"))
            {
                if (!File.Exists(resetFilePath))
                {
                    // 創建目錄 (如果不存在)
                    string directoryPath = Path.GetDirectoryName(resetFilePath);
                    Directory.CreateDirectory(directoryPath);
                    // 創建檔案 如果不存在，建立 reset.okgo 檔案
                    using (File.Create(resetFilePath))
                    {
                        FileStream fs = File.OpenRead(resetFilePath);
                        // 寫入空白文字
                        byte[] info = new UTF8Encoding(true).GetBytes("");
                        fs.Write(info, 0, info.Length);
                    }
                }
            }
            
            // 判斷 Request 是否來自 local 端
            // 若不是來自 local 端, 一律顯示
            if (!Request.IsLocal)
            {
                // 要 ClearError 
                // 不然 Asp.Net 底層的 Exception 如: HttpRequestValidationException
                // 還是會丟給 Browser 造成 500 Error
                // Web Server Misconfiguration: Server Error Message ( 10932 ) 
                Server.ClearError();

                try
                {
                    string s_path = Server.MapPath("~/Error");
                    if (!System.IO.File.Exists(s_path)) { logger.Warn("Application_Error: ~/Error 資料檔案不存在: " + s_path); }
                    // 改用 Server Transfer 以便回應 200
                    // Server.Transfer("~/Error");
                }
                catch (Exception ex)
                {
                    logger.Error("Transfer ErrorPage failed: " + ex.Message, ex);
                    Response.StatusCode = 404;
                    Response.StatusDescription = "Transfer ErrorPage failed";
                    Response.End();
                    return;
                }

                Response.StatusCode = 404;
                Response.StatusDescription = "Transfer ErrorPage failed";
                Response.End();
                return;
            }
        }

        protected void Application_EndRequest()
        {
            foreach (var item in Response.Cookies)
            {
                Response.Cookies[item.TONotNullString()].Secure = true;
                Response.Cookies[item.TONotNullString()].HttpOnly = true;
            }
        }
    }
}