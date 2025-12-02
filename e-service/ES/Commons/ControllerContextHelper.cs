using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using System.Text;

namespace ES.Commons
{
    /// <summary>
    /// 封裝一些存取 ControllerContext 相關資訊的 Helper 類
    /// </summary>
    public class ControllerContextHelper
    {
        private static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 從 ControllerContext 中取得 action 名稱
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetAction(ControllerContext context)
        {
            RouteData routeData = context.RouteData;
            if (routeData != null)
            {
                if (routeData.Values.ContainsKey("MS_DirectRouteMatches"))
                {
                    routeData = ((IEnumerable<RouteData>)routeData.Values["MS_DirectRouteMatches"]).First();
                }
            }

            return (string)routeData.Values["action"];
        }

        /// <summary>
        /// 從 ControllerContext 中取得完整 action path, 例: area/controller/action
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetActionPath(ControllerContext context)
        {
            string actionPath = string.Empty;

            /* 
             * 因為有可能透過 RouteConfig 去進行 RouteMap, 
             * Url 跟實際執行的 Controller/Action 可能會有所不同,
             * 所以, 不要由 routeData 中去取值, 一律中 Request.Url 中自行 parse 
             * 但 action 部份則要比對 routeData 中的值, 若為 Index 則要加回去
             * 
            var area = string.Empty;
             */

            RouteData routeData = context.RouteData;
            if (routeData != null)
            {
                if (routeData.Values.ContainsKey("MS_DirectRouteMatches"))
                {
                    routeData = ((IEnumerable<RouteData>)routeData.Values["MS_DirectRouteMatches"]).First();
                }
            }
            /*
            object areaObj;
            if (routeData.Values.TryGetValue("area", out areaObj))
            {
                area = areaObj.ToString();
            }
            var controller = routeData.Values["controller"];
             */
            var action = routeData.Values["action"];

            HttpRequestBase request = context.RequestContext.HttpContext.Request;
            string applicationPath = request.ApplicationPath;
            string path = request.Path;
            // remove ApplicationPath
            int p = path.IndexOf(applicationPath);
            if (p > -1)
            {
                path = path.Substring(p + applicationPath.Length);
            }
            p = path.IndexOf("?");
            if (p > -1)
            {
                path = path.Substring(0, p);
            }
            if(path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            LOG.Debug("GetActionPath: request applicationPath=" + applicationPath + ", path=" + path);

            // 最多取path中的3個部份(eg: area/controller/action)
            string[] tokens = path.Split('/');
            List<string> sbAction = new List<string>();
            for (int i = 0; i < 3 && i < tokens.Length; i++)
            {
                sbAction.Add(tokens[i]);
            }

            actionPath = string.Join("/", sbAction.ToArray());
            if ("Index".Equals(action) && !action.Equals(tokens[tokens.Length - 1]))
            {
                actionPath += "/" + action;
            }

            return actionPath;
        }


        /// <summary>
        /// Parse 由 Request 來的完整 query Path, 找出並回傳 Area 部份字串
        /// </summary>
        /// <param name="requestPath"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static string GetActionArea(string requestPath, string controller = null)
        {
            HttpRequest request = System.Web.HttpContext.Current.Request;

            string applicationPath = request.ApplicationPath;

            // remove ApplicationPath
            int p = requestPath.IndexOf(applicationPath);
            if (p > -1)
            {
                requestPath = requestPath.Substring(p + applicationPath.Length);
            }

            string area = string.Empty;
            string[] tokens = requestPath.Split('/');
            if (controller != null)
            {
                // 有傳入 controller 資訊, 用 controller 的前一個 token 作為 area
                int i = tokens.Length - 1;
                for (; i >= 0; i--)
                {
                    if (tokens[i].Equals(controller))
                    {
                        i--;
                        if (i < 0)
                        {
                            // controller 之前就是 ApplicationPath, 沒有 area 
                            //  例: http://localhost/AppContext/controller/action 或  http://localhost/controller/action
                            // request path 中沒有 area 部份, area 維持 empty
                        }
                        else
                        {
                            // path 中 controller 前一個部份, 視為 area
                            //   例: http://localhost/area/controller/action 或 http://localhost/ApplicationPath/area/controller/action
                            area = tokens[i];
                        }
                        break;
                    }
                }
            }
            else
            {
                // 沒有 controller 資訊, 將 path 視為由 3 個(含)以上的 token 組成
                // 若 token.length > 2, 直接回傳第1個
                if (tokens.Length > 2)
                {
                    area = tokens[0];
                }
            }

            LOG.Debug("GetActionArea: request applicationPath=" + applicationPath + ", path=" + requestPath + ", area=" + area);
            return area;
        }

        /// <summary>
        /// 將給定 ControllerContext 下, 將指定的 Partial viewName 搭配給定的 model 進行 Razor Render, 並返回Render結果string內容.  
        /// </summary>
        /// <param name="context"></param>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static StringBuilder RenderRazorPartialViewToString(ControllerContext context, string viewName, object model)
        {
            context.Controller.ViewData.Model = model;
            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                return sw.GetStringBuilder();
            }
        }

    }
}