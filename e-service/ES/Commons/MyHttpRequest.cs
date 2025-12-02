using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Xml;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using log4net;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;

namespace ES.Commons
{
    /// <summary>
    /// 對後端服務提供者發出 HTTP Request 並取回結果內容的工具類
    /// </summary>
    public class MyHttpRequest
    {

        protected static readonly ILog logger = LogManager.GetLogger(typeof(MyHttpRequest));

        private const string MyUserAgent = "MOWH.EService.HttpRequest";
        //private WebClient webClient;
        private int requestTimeout = 100000;        // default set to 10 seconds
        private string statusCode = "";
        private string statusDescription = "";
        private Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

        /// <summary>
        /// HttpRequest 的建構子，HTTP request timeout 預設為10秒
        /// </summary>
        public MyHttpRequest()
        {
            statusCode = "";
            statusDescription = "";
            responseHeaders.Clear();

            //參考：如何在 .NET Framework 4.0, 4.5 以上的程式支援 TLS 1.2
            //https://blogs.msdn.microsoft.com/jchiou/2016/05/27/如何在-net-framework-4-0-4-5-以上的程式支援-tls-1-2/
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            //重點是修改這行
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //SecurityProtocolType.Tls1.2;
        }

        /// <summary>
        /// 指定 HTTP request timeout 毫秒數的 HttpRequest 的建構子
        /// </summary>
        /// <param name="requestTimeout"></param>
        public MyHttpRequest(int requestTimeout) : this()
        {
            this.requestTimeout = requestTimeout;
        }

        public string GetStatusCode()
        {
            return statusCode;
        }

        public string GetStatusDescription()
        {
            return statusDescription;
        }

        public Dictionary<string, string> GetResponseHeaders()
        {
            return this.responseHeaders;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }


        /// <summary>
        /// 依傳入的 url 以及各項參數, 實際發出 HTTP Get Request，並回傳一個內容字串，
        /// </summary>
        /// <param name="requestMethod">本次 Request 所要採用的 Method 字串: GET/POST</param>
        /// <param name="requestUrl">本次 Request 的目的 url</param>
        /// <param name="headers">要附加的 Http header 參數</param>
        /// <param name="parms">各項 Http Get/Post 參數</param>
        /// <param name="contentType">Get/Post 的 Content-Type 值, 預設是 application/x-www-form-urlencoded</param>
        /// <returns></returns>
        public string request(string requestMethod, string requestUrl, Hashtable headers, Hashtable parms, string contentType = "application/x-www-form-urlencoded")
        {

            // 將各個參數組成 QueryString / Post Body
            string reqParms = "";
            if(parms != null && parms.Count > 0)
            {
                if ("application/json".Equals(contentType, StringComparison.OrdinalIgnoreCase) )
                {
                    reqParms = new JavaScriptSerializer().Serialize(parms);
                }
                else
                {
                    foreach (string key in (parms.Keys))
                    {
                        if (!"".Equals(reqParms))
                        {
                            reqParms += "&";
                        }
                        reqParms += key + "=" + HttpUtility.UrlEncode((string)parms[key], Encoding.UTF8);
                    }
                }
            }

            string reqUrl = requestUrl;

            // 依不同的 HTTP method 採用不同傳參數的方式
            if ("GET".Equals(requestMethod, StringComparison.OrdinalIgnoreCase))
            {
                if(!string.IsNullOrEmpty(reqParms))
                {
                    reqUrl = requestUrl + ((requestUrl.IndexOf("?") > -1) ? "?" : "&")
                        + reqParms;
                }
            }

            string resData = null;
            byte[] responseArray;
            try
            {
                logger.Info(requestMethod + " " + requestUrl);
                logger.Info("Content-Type: " + contentType);

                MyWebClient webClient = new MyWebClient(requestTimeout);
                webClient.Credentials = CredentialCache.DefaultCredentials;
                webClient.Headers.Set("Content-Type", contentType);

                // 附加 HTTP Headers
                if (headers != null && headers.Count > 0)
                {
                    logger.Info("===REQUEST_HEADERS===");
                    foreach(string key in headers.Keys)
                    {
                        logger.Info(key + "=" + (string)headers[key]);
                        webClient.Headers.Set(key, (string)headers[key]);
                    }
                }

                if ("POST".Equals(requestMethod, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Info("POST CONTENT: " + reqParms);

                    byte[] postBytes = new ASCIIEncoding().GetBytes(reqParms);
                    responseArray = webClient.UploadData(requestUrl, "POST", postBytes);
                    resData = Encoding.UTF8.GetString(responseArray);
                }
                else
                {
                    responseArray = webClient.DownloadData(requestUrl);
                    resData = Encoding.UTF8.GetString(responseArray);
                }

                this.statusCode = webClient.GetStatusCode();
                this.statusDescription = webClient.GetStatusDescription();

                // 取得 response headers
                logger.Info("===RESPONSE_HEADERS===");
                WebHeaderCollection resHeaders = webClient.ResponseHeaders;
                for (int i = 0; i < resHeaders.Count; i++)
                {
                    logger.Info(resHeaders.Keys[i] + "=" + resHeaders[i]);
                    responseHeaders.Add(resHeaders.Keys[i], resHeaders[i]);
                }
            }
            catch (Exception ex)
            {
                string err = requestMethod + " " + reqUrl + " 失敗, " + ex.Message;
                logger.Error("request: " + err, ex);
                throw new ArgumentException(err, ex);
            }

            logger.Info("RESPONSE DATA: \n" + resData);
            return resData;
        }

    }

    /// <summary>
    /// 繼承 WebClient 加上 timeout 設定的支援
    /// </summary>
    public class MyWebClient : WebClient
    {
        private int timeout;
        private WebRequest webReq;

        public MyWebClient(int timeout)
        {
            this.timeout = timeout;
        }

        public string GetStatusCode()
        {
            if(webReq != null)
            {
                HttpWebResponse webRes = (HttpWebResponse)base.GetWebResponse(webReq);
                return "" + (int)webRes.StatusCode;
            }
            else
            {
                return "";
            }
        }

        public string GetStatusDescription()
        {
            if (webReq != null)
            {
                HttpWebResponse webRes = (HttpWebResponse)base.GetWebResponse(webReq);
                return webRes.StatusDescription;
            }
            else
            {
                return "";
            }
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            webReq = base.GetWebRequest(uri);
            webReq.Timeout = this.timeout * 1000;
            return webReq;
        }
    }
}