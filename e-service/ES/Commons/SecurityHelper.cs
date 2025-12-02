using log4net;
using Microsoft.Security.Application;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// SecurityHelper 的摘要描述
/// </summary>
public class SecurityHelper
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityHelper));

    private static string[] strSkipArray = { };
    private static string[] strSkipUrls = { };
    private static string url = "";
    /// <summary>
    /// 2018.02.05, Eric,
    /// 偵測前端輸入的參數字串是否含有 Injection 攻擊的情況
    /// </summary>
    /// <param name="strParameter">輸入的參數字串</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static bool HasInjection(System.Web.HttpRequest Request)
    {
        string tmpSKip = ConfigurationManager.AppSettings["SkipArray"];
        string tmpSkipUrls = ConfigurationManager.AppSettings["SkipUrls"];

        if (tmpSKip != null) { strSkipArray = tmpSKip.Split(';'); }
        if (tmpSkipUrls != null) { strSkipUrls = tmpSkipUrls.Split(';'); }

        bool injected = false;
        url = Request.Url.AbsolutePath;

        if (_HttpMethodOverride(Request))
        {
            injected = true;
            return injected;
        }
        if (_ContentTypeCheck(Request))
        {
            injected = true;
            return injected;
        }

        foreach (string key in Request.QueryString)
        {
            //掃描進行Injection攻擊.asmx，在服務描述(WSDL)後加入Query參數導致伺服器錯誤回應 ( 10932 )。針對此攻擊進行過濾動作
            if (Request.CurrentExecutionFilePathExtension == ".asmx"
                && !string.IsNullOrEmpty(key) && key.ToLower() == "wsdl")
            {
                injected = true;
                Log.Warn("HasInjection 【服務描述(WSDL) Injection攻擊】 " + key + "=" + Request.QueryString[key]);
                break;
            }

            if (_HasInjection(key, Request.QueryString[key]))
            {
                injected = true;
                break;
            }
        }
        foreach (string key in Request.Form)
        {
            if (_HasInjection(key, Request.Form[key]))
            {
                injected = true;
                break;
            }
        }
        //掃描針對Cookie進行的Injection攻擊
        foreach (string key in Request.Cookies)
        {
            if (_HasInjection(key, Request.Cookies[key].Value))
            {
                injected = true;
                break;
            }
        }
        return injected;
    }

    /// <summary>
    /// 排除 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static bool isExclude(string key, string[] strArray)
    {
        bool rst = false;
        foreach (string str in strArray)
        {
            //Console.WriteLine(str);
            //if (str.Equals(key)) { rst = true; }
            if (!string.IsNullOrWhiteSpace(key) && key.EndsWith(str, StringComparison.OrdinalIgnoreCase)) { rst = true; return rst; }
        }
        return rst;
    }

    /// <summary>
    /// Injection 檢查判斷邏輯
    /// </summary>
    /// <param name="strKey"></param>
    /// <param name="strParameter"></param>
    /// <returns></returns>
    private static bool _HasInjection(string strKey, string strParameter)
    {
        bool injected = false;
        if (!string.IsNullOrEmpty(strParameter))
        {
            strParameter = ParameterDecode(strParameter);
            //Regex for detection of SQL meta-characters:
            if (Regex.Match(strParameter, @"/(\%27)|(\')|(\-\-)|(\%23)|(#)/ix", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【SQL meta-characters-1】 " + strKey + "=" + strParameter);
            }

            //Modified regex for detection of SQL meta-characters:
            if (Regex.Match(strParameter, @"/((\%3D)|(=))[^\n]*((\%27)|(\')|(\-\-)|(\%3B)|(;))/i", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【SQL meta-characters-2】 " + strKey + "=" + strParameter);
            }

            //Regex for typical SQL Injection attacks:
            if (Regex.Match(strParameter, @"/\w*((\%27)|(\'))((\%6F)|o|(\%4F))((\%72)|r|(\%52))/ix", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【SQL Injection】 " + strKey + "=" + strParameter);
            }

            //Regex for detecting SQL Injection with the UNION keyword:
            if (Regex.Match(strParameter, @"/((\%27)|(\'))union/ix", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【SQL Injection with the UNION】 " + strKey + "=" + strParameter);
            }

            //Regex for detecting SQL Injection attacks on a MS SQL Server:
            if (Regex.Match(strParameter, @"/exec(\s|\+)+(s|x)p\w+/ix", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【SQL Injecttion attacks on a MS SQL】 " + strKey + "=" + strParameter);
            }

            //Regex for a simple XSS attack:
            if (Regex.Match(strParameter, @"/((\%3C)|<)((\%2F)|\/)*[a-z0-9\%]+((\%3E)|>)/ix", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【simple XSS】 " + strKey + "=" + strParameter);
            }

            //Regex for "<img src" CSS attack:
            if (Regex.Match(strParameter, @"/((\%3C)|<)((\%69)|i|(\%49))((\%6D)|m|(\%4D))((\%67)|g|(\%47))[^\n]+((\%3E)|>)/I", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【\"<img src\" CSS attack】 " + strKey + "=" + strParameter);
            }

            //Paranoid regex for XSS attacks:
            if (Regex.Match(strParameter, @"/((\%3C)|<)[^\n]+((\%3E)|>)/I", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【XSS】 " + strKey + "=" + strParameter);
            }

            //根據黑箱掃描攻擊新增比對 Regex for mysql sleep() 、 benchmark()、ASCII(SUBSTR:
            if (Regex.Match(strParameter, @"(sleep|benchmark)([ \p{Z}\\t\\r\n\\v\\f])*(\()|(ASCII)([ \p{Z}\\t\\r\n\\v\\f])*(\()([ \p{Z}\\t\\r\n\\v\\f])*SUBSTR", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【mysql sleep()、 benchmark()、ASCII(SUBSTR)】 " + strKey + "=" + strParameter);
            }

            //根據黑箱掃描攻擊新增比對 Regex for global_name、sysmaster、scat.datatypes、all_users、syscolumns、pg_aggregate:
            if (Regex.Match(strParameter, @"(global_name)|(sysmaster)|(scat\.datatypes)|(all_users)|(syscolumns)|(pg_aggregate)", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【global_name、sysmaster、scat.datatypes、all_users、syscolumns、pg_aggregate】 " + strKey + "=" + strParameter);
            }
            //根據黑箱掃描攻擊新增比對 Regex for '/*'  '*/'
            if (Regex.Match(strParameter, @"(\/\s*\*|\*\s*\/)", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【'/*'  '*/'】 " + strKey + "=" + strParameter);
            }
            //根據黑箱掃描攻擊新增比對 Regex for if() 、mongodb : db.getName()、node.js : require('fs')
            if (Regex.Match(strParameter, @"(if\(.*\)| require\(\'fs\'\)|db\.getname\(\))", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【if() 、mongodb : db.getName()、node.js : require('fs')】 " + strKey + "=" + strParameter);
            }
            //根據黑箱掃描攻擊新增比對 Regex for LDAP Injection :  )(|  )(&  )(uid=*
            if (Regex.Match(strParameter, @"\)\((\||\&|uid=\*)", RegexOptions.IgnoreCase).Success)
            {
                injected = true;
                Log.Warn("_HasInjection 【 LDAP Injection :  )(|  )(&  )(uid=* 】 " + strKey + "=" + strParameter);
            }

            //108.12.11 顧問回覆
            //LDAP Injection(11597) 過濾的字元應該是不太夠，
            //請再加入以下字元在過濾程序內: & ! | = < > , + - " '
            if (Regex.Match(strParameter, @"[\&\!\|=<>,\+""']", RegexOptions.IgnoreCase).Success && !isExclude(strKey, strSkipArray) && !isExclude(url, strSkipUrls))
            {
                injected = true;
                Log.Warn("_HasInjection 【 & ! | = < > , + - \" ' 】 " + strKey + "=" + strParameter);
            }
        }

        return injected;
    }

    /// <summary>
    /// LDAP Injection(11597) 會過濾的字元【 & ! | = < > , + - \" ' 】
    /// </summary>
    static string[] filterChars = { "&", "!", "|", "=", "<", ">", ",", "+", "-", "\"", "'" };
    /// <summary>
    /// LDAP Injection(11597) 會過濾的字元【 & ! | = < > , + - \" ' 】
    /// 經過 EscapeInjectionChars() 處理後對應的置換字元
    /// </summary>
    static string[] escapeChars = { "＆", "！", "｜", "＝", "＜", "＞", "，", "＋", "—", "”", "’" };

    /// <summary>
    /// 針對 LDAP Injection(11597) 會過濾的字元【 & ! | = < > , + - \" ' 】進行置換處理
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string EscapeInjectionChars(string source)
    {
        if (!string.IsNullOrWhiteSpace(source))
        {
            for (int i = 0; i < filterChars.Length; i++)
            {
                source = source.Replace(filterChars[i], escapeChars[i]);
            }
        }
        return source;
    }
    /// <summary>
    /// 針對 UnescapeInjectionChars() 置換處理過的字串, 進行還原置換處理
    /// </summary>
    /// <param name="escapeStr"></param>
    /// <returns></returns>
    public static string UnescapeInjectionChars(string escapeStr)
    {
        if (!string.IsNullOrWhiteSpace(escapeStr))
        {
            for (int i = 0; i < filterChars.Length; i++)
            {
                escapeStr = escapeStr.Replace(escapeChars[i], filterChars[i]);
            }
        }
        return escapeStr;
    }

    /// <summary>
    /// 過濾字串
    /// </summary>
    /// <param name="strInput">輸入值</param>
    /// <returns></returns>
    public static string GetSafeHtml(string strInput)
    {
        return ParseChineseWords(Sanitizer.GetSafeHtml(strInput));
    }

    /// <summary>
    /// 過濾字串
    /// </summary>
    /// <param name="strInput">輸入值</param>
    /// <returns></returns>
    public static string GetSafeHtmlFragment(string strInput)
    {
        return ParseChineseWords(Sanitizer.GetSafeHtmlFragment(strInput));
    }

    /// <summary>
    /// 解決微軟AntiXSS 4.0 Parse中文字錯誤問題
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string ParseChineseWords(string str)
    {
        //str = str.Replace("\r\n", "");

        if (str.Contains("&#"))
        {
            Dictionary<string, string> hbjDictionaryFX = new Dictionary<string, string>();
            hbjDictionaryFX.Add("&#20028;", "丼");
            hbjDictionaryFX.Add("&#20284;", "似");
            hbjDictionaryFX.Add("&#20540;", "值");
            hbjDictionaryFX.Add("&#20796;", "儼");
            hbjDictionaryFX.Add("&#21052;", "刼");
            hbjDictionaryFX.Add("&#21308;", "匼");
            hbjDictionaryFX.Add("&#21564;", "吼");
            hbjDictionaryFX.Add("&#21820;", "唼");
            hbjDictionaryFX.Add("&#22076;", "嘼");
            hbjDictionaryFX.Add("&#22332;", "圼");
            hbjDictionaryFX.Add("&#22588;", "堼");
            hbjDictionaryFX.Add("&#23612;", "尼");
            hbjDictionaryFX.Add("&#26684;", "格");
            hbjDictionaryFX.Add("&#22844;", "夼");
            hbjDictionaryFX.Add("&#23100;", "娼");
            hbjDictionaryFX.Add("&#23356;", "嬼");
            hbjDictionaryFX.Add("&#23868;", "崼");
            hbjDictionaryFX.Add("&#24124;", "帼");
            hbjDictionaryFX.Add("&#24380;", "弼");
            hbjDictionaryFX.Add("&#24636;", "怼");
            hbjDictionaryFX.Add("&#24892;", "愼");
            hbjDictionaryFX.Add("&#25148;", "戼");
            hbjDictionaryFX.Add("&#25404;", "挼");
            hbjDictionaryFX.Add("&#25660;", "搼");
            hbjDictionaryFX.Add("&#25916;", "攼");
            hbjDictionaryFX.Add("&#26172;", "昼");
            hbjDictionaryFX.Add("&#26428;", "朼");
            hbjDictionaryFX.Add("&#26940;", "椼");
            hbjDictionaryFX.Add("&#27196;", "樼");
            hbjDictionaryFX.Add("&#27452;", "欼");
            hbjDictionaryFX.Add("&#27708;", "氼");
            hbjDictionaryFX.Add("&#27964;", "洼");
            hbjDictionaryFX.Add("&#28220;", "渼");
            hbjDictionaryFX.Add("&#28476;", "漼");
            hbjDictionaryFX.Add("&#28732;", "瀼");
            hbjDictionaryFX.Add("&#28988;", "焼");
            hbjDictionaryFX.Add("&#29244;", "爼");
            hbjDictionaryFX.Add("&#29500;", "猼");
            hbjDictionaryFX.Add("&#29756;", "琼");
            hbjDictionaryFX.Add("&#30012;", "甼");
            hbjDictionaryFX.Add("&#30268;", "瘼");
            hbjDictionaryFX.Add("&#30524;", "眼");
            hbjDictionaryFX.Add("&#30780;", "砼");
            hbjDictionaryFX.Add("&#31036;", "礼");
            hbjDictionaryFX.Add("&#31292;", "稼");
            hbjDictionaryFX.Add("&#31548;", "笼");
            hbjDictionaryFX.Add("&#31804;", "簼");
            hbjDictionaryFX.Add("&#32060;", "紼");
            hbjDictionaryFX.Add("&#32316;", "縼");
            hbjDictionaryFX.Add("&#32572;", "缼");
            hbjDictionaryFX.Add("&#32828;", "耼");
            hbjDictionaryFX.Add("&#33084;", "脼");
            hbjDictionaryFX.Add("&#33340;", "舼");
            hbjDictionaryFX.Add("&#33596;", "茼");
            hbjDictionaryFX.Add("&#33852;", "萼");
            hbjDictionaryFX.Add("&#34108;", "蔼");
            hbjDictionaryFX.Add("&#36156;", "贼");
            hbjDictionaryFX.Add("&#39740;", "鬼");

            foreach (string key in hbjDictionaryFX.Keys)
            {
                if (str.Contains(key))
                {
                    str = str.Replace(key, hbjDictionaryFX[key]);
                }
            }
        }

        return str;
    }

    /// <summary>
    /// 處理查詢字串內有多重編碼問題
    /// </summary>
    /// <param name="strParameter"></param>
    /// <returns></returns>
    public static string ParameterDecode(string strParameter)
    {
        if (strParameter.IndexOf("%25") > -1)
        {
            strParameter = HttpUtility.UrlDecode(strParameter);
            strParameter = ParameterDecode(strParameter);
        }
        else
        {
            strParameter = HttpUtility.UrlDecode(strParameter);
        }
        return strParameter;
    }

    /// <summary>
    /// 處理 Often Misused: HTTP Method Override ( 11534 ) 問題
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    private static bool _HttpMethodOverride(System.Web.HttpRequest Request)
    {
        bool flag = false;
        //預計排除的HTTP METHOD
        string[] http_Method = { "PUT", "DELETE", "OPTIONS", "PATCH" };
        //針對get post傳入參數_method的值檢驗
        string rq_method = string.IsNullOrEmpty(Request.QueryString["_method"]) ? "" : Request.QueryString["_method"].ToUpper();
        string rf_method = string.IsNullOrEmpty(Request.Form["_method"]) ? "" : Request.Form["_method"].ToUpper();
        //針對Headers中X_HTTP_METHOD、X_HTTP_Method_Override、X_METHOD_OVERRIDE的值檢驗
        string X_HTTP_METHOD = string.IsNullOrEmpty(Request.Headers["X-HTTP-METHOD"]) ? "" : Request.Headers["X-HTTP-METHOD"].ToUpper();
        string X_HTTP_Method_Override = string.IsNullOrEmpty(Request.Headers["X-HTTP-Method-Override"]) ? "" : Request.Headers["X-HTTP-Method-Override"].ToUpper();
        string X_METHOD_OVERRIDE = string.IsNullOrEmpty(Request.Headers["X-METHOD-OVERRIDE"]) ? "" : Request.Headers["X-METHOD-OVERRIDE"].ToUpper();

        if (X_HTTP_METHOD != "" || X_HTTP_Method_Override != "" || X_METHOD_OVERRIDE != "" || rq_method != "" || rf_method != "")
        {
            string[] http_Method_Check_Item = { X_HTTP_METHOD, X_HTTP_Method_Override, X_METHOD_OVERRIDE, rq_method, rf_method };

            if (http_Method_Check_Item.Any(http_Method.Contains))
            {
                Log.Warn(string.Format("Has_Http_Method_Override : X_HTTP_METHOD={0},X_HTTP_Method_Override={1},X_METHOD_OVERRIDE={2},rq_method={3},rf_method={4}", X_HTTP_METHOD, X_HTTP_Method_Override, X_METHOD_OVERRIDE, rq_method, rf_method));
                return true;
            }
        }
        return flag;
    }
    /// <summary>
    /// 處理 ContentType是JSON但傳送進來的內容不是JSON格式造成WebService 500 ERROR問題
    /// </summary>
    /// <param name="Request"></param>
    /// <returns></returns>
    private static bool _ContentTypeCheck(System.Web.HttpRequest Request)
    {
        bool flag = false;

        if (Request.ContentType.ToLower().Contains("application/json"))
        {
            Stream s = Request.InputStream;
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            s.Seek(0, SeekOrigin.Begin);
            string strInputStream = Encoding.UTF8.GetString(b);
            try
            {
                JsonConvert.DeserializeObject<JObject>(strInputStream);
            }
            catch (Exception ex)
            {
                Log.Warn(string.Format("ContentType-application/json,Parameter:{0},ex:{1}", strInputStream, ex));
                return true;
            }
        }
        return flag;
    }
}