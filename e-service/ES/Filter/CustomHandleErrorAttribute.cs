using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using System.Web.Configuration;
using System.IO;
using System.Runtime.Remoting;
using System.Text;

namespace ES.Filter
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        private static readonly ILog logger = ES.Utils.LogUtils.GetLogger();

        /// <summary>
        /// 異常處理
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnException(ExceptionContext filterContext)
        {
            //使用log4net或其他記錄錯誤消息
            Exception Error = filterContext.Exception;
            string Message = Error.Message;//錯誤信息
            string Url = HttpContext.Current.Request.RawUrl;//錯誤發生地址
            string s_err1 = string.Format("\nMessage: {0}\nUrl: {1}", Message, Url);
            logger.Error(s_err1, Error);
            s_err1 = string.Format("\nMessage: {0}\nUrl: {1}\nException:{2}", Message, Url, Error.ToString());

            filterContext.Controller.ViewBag.Message = Message;

            string WebTestEnvir = WebConfigurationManager.AppSettings["WebTestEnvir"];
            bool flag_error_show = false;
            if (WebTestEnvir != null && WebTestEnvir.Equals("Y")) { flag_error_show = true; }
            if (HttpContext.Current.Request.IsLocal) { flag_error_show = true; }
            if (flag_error_show)
            {
                filterContext.Controller.TempData["show"] = true;
                filterContext.Controller.TempData["Message"] = Message.Replace("\n", "<br/>");
                filterContext.Controller.TempData["ErrLogMsg"] = s_err1.Replace("\n", "<br/>");
            }

            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult() { ViewName = "Error", TempData = filterContext.Controller.TempData };

            // 檢查是否已經存在 reset.okgo 檔案
            string resetFilePath = @"C:\eWeb\SRC\rcReset\reset.okgo";
            if (Message.Contains("執行逾時到期"))
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
        }
    }
}