using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;

namespace EECOnline.Commons
{
    /// <summary>
    /// 用來處理全域 Unhandled Exception 的 logging 及 routing 的類
    /// </summary>
    public class ExceptionHandler
    {
        protected ILog LOG = LogManager.GetLogger(typeof(ExceptionHandler));

        private Controller ErrorPageController = null;
        private string ControllerName = "ErrorPage";
        private string DefaultAction = "Index";

        public ExceptionHandler(Controller controller)
        {
            ErrorPageController = controller;
            ControllerName = controller.GetType().Name;
        }

        public void RouteErrorPage(HttpContext context, Exception lastError)
        {

            int statusCode = 0;
            if (lastError == null)
            {
                lastError = new System.ArgumentNullException("沒有Exception資訊");
            }
            if (lastError.GetType() == typeof(HttpException))
            {
                statusCode = ((HttpException)lastError).GetHttpCode();
            }
            else
            {
                // Not an HTTP related error so this is a problem in our code, set status to
                // 500 (internal server error)
                statusCode = 500;
            }

            statusCode = 404;

            // logging current RouteData and lastError
            HttpContextWrapper contextWrapper = new HttpContextWrapper(context);
            RouteData thisRouteData = RouteTable.Routes.GetRouteData(contextWrapper);
            string thisArea = (string)thisRouteData.DataTokens["area"];
            string thisController = (string)thisRouteData.Values["controller"];
            string thisAction = (string)thisRouteData.Values["action"];
            thisArea = (thisArea != null) ? "~/" + thisArea : "~";

            string exMessage = lastError.GetType().FullName + ": " + lastError.Message;
            LOG.Error(thisArea + "/" + thisController + "/" + thisAction + ": " + context.Request.UserHostAddress + " " + statusCode + " " + exMessage);
            LOG.Error(">>", lastError);

            // keep lastError to Session, that ErrorPageController can read it
            try
            {
                if(context.Session != null)
                {
                    context.Session["LastException"] = lastError;
                }
            }
            catch (Exception e)
            {
                LOG.Error("Session 存取異常: " + e.Message, e);
                exMessage = lastError.GetType().FullName + ": " + lastError.Message;
            }
            

            // pass exception to ErrorsPageController
            RouteData newRouteData = new RouteData();
            newRouteData.Values.Add("controller", ControllerName);
            newRouteData.Values.Add("action", DefaultAction);
            newRouteData.Values.Add("statusCode", statusCode);
            newRouteData.Values.Add("exMessage", exMessage);
            newRouteData.Values.Add("isAjaxRequet", contextWrapper.Request.IsAjaxRequest());

            IController controller = this.ErrorPageController;

            RequestContext requestContext = new RequestContext(contextWrapper, newRouteData);

            controller.Execute(requestContext);
            context.Response.End();

        }


    }

}