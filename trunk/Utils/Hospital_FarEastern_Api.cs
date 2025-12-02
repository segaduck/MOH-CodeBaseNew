using System;
using System.Collections.Generic;
using Turbo.Commons;
using EECOnline.Services;
using log4net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.IO;
using System.Collections;
using Omu.ValueInjecter;
using System.Linq;

namespace EECOnline.Utils
{
    /// <summary>
    /// 亞東醫院 API
    /// </summary>
    public class Hospital_FarEastern_Api
    {
        private static readonly ILog LOG = LogUtils.GetLogger();

        private class Api_A1Model
        {
            public string idno { get; set; }
            public string birth { get; set; }
            public string ec_sdate { get; set; }
            public string ec_edate { get; set; }
            public string Token { get; set; }
        }

        public class Api_A1ResultModel
        {
            public string ec_no { get; set; }
            public string ec_name { get; set; }
            public string ec_price { get; set; }
            public string ec_date { get; set; }
            public string ec_note { get; set; }
            public string ec_online { get; set; }
            public string ec_dept { get; set; }
            public string ec_doctor { get; set; }
            public string ec_dateText { get; set; }
            public string ec_docType { get; set; }
            public string ec_system { get; set; }
        }

        /// <summary>
        /// 取得 病歷類型 列表
        /// </summary>
        public static IList<Api_A1ResultModel> Api_A1(string domainName, string idno, string birth, string ec_sdate, string ec_edate)
        {
            LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A1() Called.");
            var Result = new List<Api_A1ResultModel>();

            // 參數基本檢查
            if (domainName.TONotNullString() == "") return Result;
            if (idno.TONotNullString() == "") return Result;
            if (birth.TONotNullString() == "") return Result;
            if (ec_sdate.TONotNullString() == "") return Result;
            if (ec_edate.TONotNullString() == "") return Result;

            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var postJSON = JsonConvert.SerializeObject(new Api_A1Model()
                        {
                            idno = idno,          // 身分證 必填
                            birth = birth,        // 生日
                            ec_sdate = ec_sdate,  // 病歷區間起 必填
                            ec_edate = ec_edate,  // 病歷區間訖 必填
                            Token = Hospital_FarEastern_Code.GetToken(),  // 傳入亞東醫院的 Token
                        });
                        string webApiUrl = domainName;
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
                            //var tmpResult = JsonConvert.DeserializeObject<Hashtable>(strResult);
                            List<Api_A1ResultModel> objResult = JsonConvert.DeserializeObject<List<Api_A1ResultModel>>(strResult/*tmpResult["result"].TONotNullString()*/);
                            Result.Clear();
                            Result.AddRange(objResult.Where(x => x.ec_online == "Y").ToList());
                            sr.Close();
                        }
                        LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A1() OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A1() Error: " + ex.Message);
            }
            return Result;
        }

        public static string Api_A1_Remark(Api_A1ResultModel model)
        {
            var Result = "";
            if (!string.IsNullOrEmpty(model.ec_dept)) Result = Result + "_" + model.ec_dept;
            if (!string.IsNullOrEmpty(model.ec_doctor)) Result = Result + "_" + model.ec_doctor;
            if (!string.IsNullOrEmpty(model.ec_dateText)) Result = Result + "_" + model.ec_dateText;
            if (!string.IsNullOrEmpty(model.ec_date)) Result = Result + ":" + model.ec_date;
            return Result;
        }

        public class Api_A2_1_MainParamsModel
        {
            public string idno { get; set; }
            public string birth { get; set; }
            public string caseNo { get; set; }
            public string Token { get; set; }
            public IList<Api_A2_1_ParamsModel> data { get; set; }
        }

        public class Api_A2_1_ParamsModel
        {
            public string ec_no { get; set; }
            public string ec_name { get; set; }
            public int? ec_price { get; set; }
            public string ec_date { get; set; }
            public string ec_docType { get; set; }
            public string ec_system { get; set; }
            public string ec_fileName { get; set; }
        }

        public class Api_A2_1_ResultModel
        {
            public string ec_no { get; set; }
            public string ec_name { get; set; }
            public string ec_price { get; set; }
            public string ec_date { get; set; }
            public string ec_success { get; set; }
            public string ec_reason { get; set; }
        }

        /// <summary>
        /// 取得 匯入申請病歷資料
        /// </summary>
        public static IList<Api_A2_1_ResultModel> Api_A2_1(string domainName, string idno, string birth, string caseNo,
            List<Api_A2_1_ParamsModel> data)
        {
            LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_1() Called.");
            var Result = new List<Api_A2_1_ResultModel>();

            // 參數基本檢查
            if (domainName.TONotNullString() == "") return Result;
            if (idno.TONotNullString() == "") return Result;
            if (birth.TONotNullString() == "") return Result;
            if (caseNo.TONotNullString() == "") return Result;
            if (data.ToCount() <= 0) return Result;

            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var postJSON = JsonConvert.SerializeObject(new Api_A2_1_MainParamsModel()
                        {
                            idno = idno,
                            birth = birth,
                            caseNo = caseNo,
                            Token = Hospital_FarEastern_Code.GetToken(),
                            data = data,
                        });
                        string webApiUrl = domainName;
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
                            List<Api_A2_1_ResultModel> objResult = JsonConvert.DeserializeObject<List<Api_A2_1_ResultModel>>(strResult);
                            Result.Clear();
                            Result.AddRange(objResult);
                            sr.Close();
                        }
                        LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_1() OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_1() Error: " + ex.Message);
            }
            return Result;
        }

        public class Api_A2_2_Login_ParamsModel
        {
            public string user_name { get; set; }
            public string user_pwd { get; set; }
        }

        public class Api_A2_2_Login_ResultModel
        {
            public string token { get; set; }
        }

        public class Api_A2_2_ParamsModel
        {
            public string token { get; set; }
            public string caseNo { get; set; }
            public IList<Api_A2_2_ParamsSubModel> data { get; set; }
        }

        public class Api_A2_2_ParamsSubModel
        {
            public string ec_no { get; set; }
            public string ec_name { get; set; }
            public string ec_price { get; set; }
            public string ec_date { get; set; }
            public string ec_success { get; set; }
            public string ec_reason { get; set; }

            public string ec_fileBase64 { get; set; }
        }

        public class Api_A2_2_ResultModel
        {
            public string ec_no { get; set; }
            public string ec_name { get; set; }
            public string ec_price { get; set; }
            public string ec_date { get; set; }
            public string ec_success { get; set; }
            public string ec_reason { get; set; }
        }

        public class Api_A4_MainParamsModel
        {
            public string idno { get; set; }
            public string birth { get; set; }
            public string caseNo { get; set; }
            public string Token { get; set; }
            public IList<Api_A4_ParamsModel> data2 { get; set; }
        }

        public class Api_A4_ParamsModel
        {
            public string ec_no { get; set; }
        }

        public static void Api_A4(string domainName, string idno, string birth, string caseNo, List<Api_A4_ParamsModel> data)
        {
            LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A4() Called.");

            // 參數基本檢查
            if (domainName.TONotNullString() == "") return;
            if (idno.TONotNullString() == "") return;
            if (birth.TONotNullString() == "") return;
            if (caseNo.TONotNullString() == "") return;
            if (data.ToCount() <= 0) return;

            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var postJSON = JsonConvert.SerializeObject(new Api_A4_MainParamsModel()
                        {
                            idno = idno,
                            birth = birth,
                            caseNo = caseNo,
                            Token = Hospital_FarEastern_Code.GetToken(),
                            data2 = data,
                        });
                        string webApiUrl = domainName;
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
                            Hashtable objResult = JsonConvert.DeserializeObject<Hashtable>(strResult);
                            LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A4() ec_success: '" + objResult["ec_success"].TONotNullString() + "'.");
                            if (objResult["ec_success"].TONotNullString() != "成功")
                            {
                                LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A4() ec_reason: '" + objResult["ec_reason"].TONotNullString() + "'.");
                            }
                            sr.Close();
                        }
                        LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A4() OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Api.Api_A4() Error: " + ex.Message);
            }
        }
    }
}