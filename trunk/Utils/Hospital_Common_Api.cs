using System;
using System.Collections.Generic;
using EECOnline.Services;
using EECOnline.DataLayers;
using EECOnline.Models.Entities;
using log4net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace EECOnline.Utils
{
    /// <summary>
    /// 醫院共用 API
    /// </summary>
    public class Hospital_Common_Api
    {
        private static readonly ILog LOG = LogUtils.GetLogger();

        private static string GetApiUrl()
        {
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetApiUrl() Called.");
            FrontDAO dao = new FrontDAO();
            var tmpObj = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_Common_Api", del_mk = "N" });
            return (tmpObj == null) ? "" : tmpObj.setup_val.TONotNullString();
        }

        private class apiLoginModel
        {
            public string LoginUser { get; set; }
            public string LoginPwd { get; set; }
            public string ResultToken { get; set; }
        }

        /// <summary>1.1 登入<br/>登入取得 Token</summary>
        public static string GetLoginToken(string LoginUser, string LoginPwd)
        {
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetLoginToken() Called.");
            const string ApiPath = "/api/login";
            string ApiUrl = GetApiUrl();
            string Result = "";
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var postJSON = JsonConvert.SerializeObject(new apiLoginModel()
                        {
                            LoginUser = LoginUser,
                            LoginPwd = LoginPwd,
                        });
                        string webApiUrl = ApiUrl + ApiPath;
                        if (System.Web.Configuration.WebConfigurationManager.AppSettings["BliWsUrl"] != null)
                        {
                            webApiUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["BliWsUrl"].ToString();
                        }
                        // 處理呼叫完成 Web API 之後的回報結果
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        HttpWebRequest request = HttpWebRequest.Create(webApiUrl) as HttpWebRequest;
                        request.ContentType = "application/json";
                        request.Method = "POST";  // 方法
                        request.KeepAlive = true; // 是否保持連線
                        byte[] bs = Encoding.UTF8.GetBytes(postJSON);
                        using (Stream reqStream = request.GetRequestStream())
                        {
                            reqStream.Write(bs, 0, bs.Length);
                        }
                        using (WebResponse webResponse = request.GetResponse())
                        {
                            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                            var strResult = sr.ReadToEnd();
                            apiLoginModel objResult = JsonConvert.DeserializeObject<apiLoginModel>(strResult);
                            Result = objResult.ResultToken;
                            sr.Close();
                        }
                        LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetLoginToken() OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetLoginToken() Error: " + ex.Message);
            }
            return Result;
        }

        public class apiQueryIndexModel
        {
            public string PatientIdNo { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Token { get; set; }
            public string PatientId { get; set; }
            public string AccessionNum { get; set; }
            public string HospitalId { get; set; }
            public string StudyUid { get; set; }
            public string Guid { get; set; }
            public string TemplateId { get; set; }
            public string CreatedDateTime { get; set; }
            public string Modality { get; set; }
            public string BodyPart { get; set; }
            public string PerfrmdStartDate { get; set; }
            public string HospitalName { get; set; }
            public string PerfrmdItem { get; set; }
            public string VerificationDate { get; set; }
        }

        /// <summary>1.2 查詢索引<br/>使用時間區間和病患身份證號來查詢索引</summary>
        public static IList<apiQueryIndexModel> GetQueryIndex(string PatientIdNo, string StartDate, string EndDate, string Token)
        {
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryIndex() Called.");
            const string ApiPath = "/api/queryindex";
            string ApiUrl = GetApiUrl();
            var Result = new List<apiQueryIndexModel>();
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var postJSON = JsonConvert.SerializeObject(new apiQueryIndexModel()
                        {
                            PatientIdNo = PatientIdNo,
                            StartDate = StartDate,
                            EndDate = EndDate,
                            Token = Token,
                        });
                        string webApiUrl = ApiUrl + ApiPath;
                        if (System.Web.Configuration.WebConfigurationManager.AppSettings["BliWsUrl"] != null)
                        {
                            webApiUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["BliWsUrl"].ToString();
                        }
                        // 處理呼叫完成 Web API 之後的回報結果
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        HttpWebRequest request = HttpWebRequest.Create(webApiUrl) as HttpWebRequest;
                        request.ContentType = "application/json";
                        request.Method = "POST";  // 方法
                        request.KeepAlive = true; // 是否保持連線
                        byte[] bs = Encoding.UTF8.GetBytes(postJSON);
                        using (Stream reqStream = request.GetRequestStream())
                        {
                            reqStream.Write(bs, 0, bs.Length);
                        }
                        using (WebResponse webResponse = request.GetResponse())
                        {
                            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                            var strResult = sr.ReadToEnd();
                            List<apiQueryIndexModel> objResult = JsonConvert.DeserializeObject<List<apiQueryIndexModel>>(strResult);
                            Result.Clear();
                            Result.AddRange(objResult);
                            sr.Close();
                        }
                        LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryIndex() OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryIndex() Error: " + ex.Message);
            }
            return Result;
        }

        public class apiQueryContentModel
        {
            public string Token { get; set; }
            public string GuId { get; set; }
            public string PatientIdNo { get; set; }
            public string AccessionNum { get; set; }
            public string HospitalId { get; set; }
            public string TemplateId { get; set; }
            public string Report { get; set; }
        }

        /// <summary>1.4 匯入報告<br/>下載報告</summary>
        /// <returns>base64 字串的報告本文</returns>
        public static string GetQueryContent(string Token, string GuId, string PatientIdNo, string AccessionNum, string HospitalId, string TemplateId)
        {
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryContent() Called.");
            const string ApiPath = "/api/querycontent";
            string ApiUrl = GetApiUrl();
            var Result = "";
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var postJSON = JsonConvert.SerializeObject(new apiQueryContentModel()
                        {
                            Token = Token,
                            GuId = GuId,
                            PatientIdNo = PatientIdNo,
                            AccessionNum = AccessionNum,
                            HospitalId = HospitalId,
                            TemplateId = TemplateId,
                        });
                        string webApiUrl = ApiUrl + ApiPath;
                        if (System.Web.Configuration.WebConfigurationManager.AppSettings["BliWsUrl"] != null)
                        {
                            webApiUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["BliWsUrl"].ToString();
                        }
                        // 處理呼叫完成 Web API 之後的回報結果
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        HttpWebRequest request = HttpWebRequest.Create(webApiUrl) as HttpWebRequest;
                        request.ContentType = "application/json";
                        request.Method = "POST";  // 方法
                        request.KeepAlive = true; // 是否保持連線
                        byte[] bs = Encoding.UTF8.GetBytes(postJSON);
                        using (Stream reqStream = request.GetRequestStream())
                        {
                            reqStream.Write(bs, 0, bs.Length);
                        }
                        using (WebResponse webResponse = request.GetResponse())
                        {
                            StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                            var strResult = sr.ReadToEnd();
                            apiQueryContentModel objResult = JsonConvert.DeserializeObject<apiQueryContentModel>(strResult);
                            Result = objResult.Report;
                            sr.Close();
                        }
                        LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryContent() OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryContent() Error: " + ex.Message);
            }
            return Result;
        }

        /// <summary>
        /// 1.4 匯入報告<br/>
        /// 下載報告<br/>
        /// 呼叫此函數，會直接將結果 base64 存入 DB 資料表 EEC_ApplyDetailPrice_ApiData
        /// </summary>
        public static void GetQueryContent_SaveIntoDB(string Token, string GuId, string PatientIdNo, string AccessionNum, string HospitalId, string TemplateId)
        {
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryContent_SaveIntoDB() Called.");

            var base64Str = GetQueryContent(Token, GuId, PatientIdNo, AccessionNum, HospitalId, TemplateId);
            if (base64Str.TONotNullString() == "")
            {
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryContent_SaveIntoDB() The GetQueryContent return is Null or Empty.");
                return;
            }

            FrontDAO dao = new FrontDAO();
            int res = dao.Update(
                new TblEEC_ApplyDetailPrice_ApiData()
                {
                    Report_XML = base64Str,
                },
                new TblEEC_ApplyDetailPrice_ApiData()
                {
                    Guid = GuId,
                    PatientIdNo = PatientIdNo,
                    AccessionNum = AccessionNum,
                    HospitalId = HospitalId,
                    TemplateId = TemplateId,
                }
            );
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.GetQueryContent_SaveIntoDB() Update-rows: " + res.ToString());
        }

        /// <summary>
        /// 抓出 DB 的 病歷檔案 base64 (是一個 XML 檔) <br />
        /// 然後把這個 XML 透過 XSLT <br />
        /// 轉成 HTML 然後再存入 DB 資料表 EEC_ApplyDetailPrice_ApiData
        /// </summary>
        public static void TransXMLtoHTML(string HisType, string GuId, string PatientIdNo, string AccessionNum, string HospitalId, string TemplateId)
        {
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML() Called.");
            var theKey = new TblEEC_ApplyDetailPrice_ApiData()
            {
                Guid = GuId,
                PatientIdNo = PatientIdNo,
                AccessionNum = AccessionNum,
                HospitalId = HospitalId,
                TemplateId = TemplateId,
            };
            // 檢查
            var dao = new FrontDAO();
            var apiData = dao.GetRow(theKey);
            if (apiData == null)
            {
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML() API-Data not found.");
                return;
            }
            if (string.IsNullOrEmpty(apiData.Report_XML))
            {
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML() Report_XML is Null or Empty.");
                return;
            }
            // 分類
            //var pathXSL = @"..\XSLTemplate\";
            //var pathXSL = System.Web.HttpContext.Current.Server.MapPath(@"~\Uploads\XSLTemplate\");
            var pathXSL = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Uploads\XSLTemplate\");
            switch (HisType)
            {
                case "121": pathXSL = pathXSL + "121ClinicalMedical門診病歷.xsl"; break;
                case "114": pathXSL = pathXSL + "114OutPatientMedication門診用藥.xsl"; break;
                case "113": pathXSL = pathXSL + "113BloodTest血液.xsl"; break;
                case "004": pathXSL = pathXSL + "ImageReport醫療影像.xsl"; break;
                case "115": pathXSL = pathXSL + "115Discharge出院病摘.xsl"; break;
                default:
                    LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML() HisType not found.");
                    return;
            }
            if (!File.Exists(pathXSL))
            {
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML() XSL is not Exists.");
                return;
            }
            var htmlContent = TransformBase64ToHtmlAndUpdateDatabase(apiData.Report_XML, pathXSL);
            var resUpd = dao.Update(new TblEEC_ApplyDetailPrice_ApiData() { Report_HTML = htmlContent }, theKey);
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML() Done.");
        }

        /// <summary>
        /// 傳入 XML 的 base64 資料<br />
        /// 透過 XSL 去轉換成 HTML 然後回傳
        /// </summary>
        /// <param name="base64Content">XML 的 base64 資料</param>
        /// <param name="xslFilePath">XSL 的 檔案路徑</param>
        /// <returns>Html Content</returns>
        public static string TransformBase64ToHtmlAndUpdateDatabase(string base64Content, string xslFilePath)
        {
            LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransformBase64ToHtmlAndUpdateDatabase() Called.");
            // Step 1: Convert Base64 to XML
            byte[] xmlBytes = Convert.FromBase64String(base64Content);
            string xmlString = Encoding.UTF8.GetString(xmlBytes);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            // Step 2: Transform XML to HTML using XSL
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xslFilePath);
            using (StringWriter stringWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
            {
                xslt.Transform(xmlDoc, xmlWriter);
                string htmlContent = stringWriter.ToString();
                LOG.Debug("EECOnline.Utils.Hospital_Common_Api.TransformBase64ToHtmlAndUpdateDatabase() Done.");
                return htmlContent;
            }
        }
    }
}