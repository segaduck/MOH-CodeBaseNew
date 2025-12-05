using EECOnline.Controllers;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EECOnline.Commons;
using Turbo.Commons;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using EECOnline.DataLayers;

namespace EECOnline
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected static readonly ILog LOG = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch
                      (new System.IO.FileInfo(Server.MapPath("~/log4net.config")));

            LOG.Info("Application_Start");

            // 加入自定義的 View/PartialView Location 設定
            ExtendedRazorViewEngine engine = ExtendedRazorViewEngine.Instance();
            // 報表模組-客制化報表結果顯示用的 PartialViews
            engine.AddPartialViewLocationFormat("~/Views/Report/Custom/{0}.cshtml");
            engine.Register();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //加入技檢系統共用的資料 Model 綁定
            //ModelBinders.Binders.Add(typeof(ExamUnitModel), new ExamUnitModelBinder());

            //啟動 BackgroundJob
            HangfireBootstrapper.Instance.Start();
        }

        protected void Application_EndRequest()
        {
            foreach (var item in Response.Cookies)
            {
                Response.Cookies[item.TONotNullString()].Secure = true;
                Response.Cookies[item.TONotNullString()].HttpOnly = true;
            }
        }

        protected void Application_Error()
        {
            // Code that runs when an unhandled error occurs
            Exception lastError = Server.GetLastError();
            Server.ClearError();
            
            // Log the full exception details for debugging
            LOG.Error("Application_Error: " + lastError.Message, lastError);

            //// 紀錄異常
            //LoginDAO dao = new LoginDAO();
            //if (lastError.Message.Contains("找不到路徑"))
            //{
            //    TblVISIT_RECORD vr = new TblVISIT_RECORD();
            //    vr.modtime = DateTime.Now;
            //    vr.use_type = "-1";
            //    vr.status = "0";
            //    vr.note = lastError.Message;

            //    dao.Insert(vr);
            //}

            // handled the exception and route to error page   
            // ExceptionHandler handler = new ExceptionHandler(new ErrorPageController());
            //handler.RouteErrorPage(this.Context, lastError);

            // Return appropriate HTTP status code based on exception type
            var httpException = lastError as HttpException;
            if (httpException != null)
            {
                Response.StatusCode = httpException.GetHttpCode();
            }
            else
            {
                // Default to 500 Internal Server Error for non-HTTP exceptions
                Response.StatusCode = 500;
            }
        }
    }
}
