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
    /// 中山醫院<br/>
    /// 備註：他們是走 Web Service
    /// </summary>
    public class Hospital_csh_Api
    {
        private static readonly ILog LOG = LogUtils.GetLogger();
        private static readonly string PKey = "hu_MRNxRzvVYruvjOnpWP3WkIyTtET1YbJ9j7LVqIfSqtrkN4c1sSGredxFNHfBVlARbaoM3cZH4A_yUCsXht2lba614FMZoWSr9cyTT4nHl0hcCeOKkXnR3KI17LR5HGO33RQoQNYqbItOMTo9kfOzozX5vk4a2BhwdyoAl1h8MERcWwMoqLtVfMI5UV6ed99DdMUNjBoS3ED_QjbZMaz1t7lyhN0_R1U6gTxwSOzIKWNBCPie2lNzHFuLstLGu24bnZ4F5ZZz9_k0lhWhrPyxB2helQF3D9gqlmm9hUb95iuFSyDkpaJjthE6nNyygBeA5iyQV5Mw_St9q5Hzm5O8y+EN190YclbNKKNm_DA89n9xhoBPtt8F9lOhlDAtdcA1gbPwToiFIZR3MOyep3GWy9D+xDAIYMY0olORK6vxXcfWPmb4UrovVwS86OgPCVb1wFSzCvIfZxVHGGFGukTunRZy+iydbItYQ2QDC+Nvez43+7ICwrTR20oxFCPJ0BRtLclUueGGhVniUz+B0pCwN_nnH41BFQkOHFs6VkrUU2Hdzw3lLrtEP1Re0+CUidJTnUobtEgLv553wwNW9ABHyqK50N7b4ZA+VEdwAqzaIo2eS7TKHANoD_Ja5Og4r9mFWYzH8GTJQk3LO7x4gmqz8FrJ1WwxclmpBSWhVQqsrTCaT5mivYx1yuqLGhT0MBtvu03g0jQ+2OliDjGD9jeQxQUo8AJCUmFwazSqXLRwCdS2QfxQZXVHHToFl6IwQ3C22C3zlMgUJJAJOcH5I78u3qWnsZdqRQYmPQvTOIBMmuKv6wVOoGtffPX1IvcbqYG6TibogR1KfcMqfnoNaFHq5VwNifLZuQ8FctP9IwdXJtNT4wggYrpYI0a6S75_XB9oGB7jCdcGNYNBwQw9yqCqEhvAKMTp7e_1o2Aqt6nvHFDcVCJRcytC96HLuIF1tGRTBwgXPES9cmFVGryCC3E6xtchyZtJ81qi4hzl_ImU+zo9f3O9MqiKDK4gTLnX83jxEUMG6_yCUlccfytE0Ki8LTDqUHzWeOqZ90YT6xIc+KPFw01t1Ufm64yRhYQdUH_ZFnz1WlKL7s2EJHgo1JL_ElNZIuVNJ8cdlR5RjjlZ1ql4hkU7eIaDpZWMJ7Kjv4p8ySNiJ0rV6l5i+yz+y1NqCgpXI9jHsGmgyrt+LVox+3XiTx+Ohx8c1vFU9uYo17grKAQmfT2kuWiCUyfgUZ+CYHWKXGNSE_46W_swgfsjOI5pztJZaYwsNwGQKOrSpFRCBpQRp_CGYypofD7V+LiJGn2p_3_qrUUZwBGd2xFuagji1VLBjiTUrYb06eFa1SXOHQFmmjoU_kncFXTgZVkq40gVrPXWw5M1xp4es3C4XtQjGY073PHxcHImqpiZFSE4vy2n2yjQRll8u6kK1TLbhT474SxZDLDZQU9V7blVggah6hpH_liIeaCzzkC9Xru7__mDmaYofhgWoxwZFQE_gu61GiusLEVXb+dVlhtCXh09mohEUe_VXgYXORVlhkIPs6+XFTPl84YnoI7t+bCkMLp4EBo9QTEaZYhU59q2zF6dwfFL6hRRTYg0TG0_9jBdXwVJYXOCUMUKedaRtizzJe6o5JKhUZOgwG401iIxlGu0JLo3TLnkWDH6Pa0rKMkAnOpPPgZ8zRGnkFLg7lHk_2iiSnN51icOSYdf_8vo+vrF9IY7d4GiWbcboFdLII7jSzSQD+17x2TdfOM_OjptGzJd1V+2y+COxwGSvZtZ1QFRPjCJuDqaPsQV6HSRI";

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

        /// <summary>
        /// 中山醫的 GetToken
        /// </summary>
        public static string GetToken()
        {
            try
            {
                LOG.Info("Hospital_csh_Api.GetToken() Begin.");

                ServicePointManager.SecurityProtocol =
                         SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                         SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;

                tw.org.csh.sysint.MedRecApply wsObj = new tw.org.csh.sysint.MedRecApply();
                var wsRes = wsObj.GetToken(PKey);

                LOG.Info("Hospital_csh_Api.GetToken() Result: " + wsRes);
                return wsRes;  // 直接回傳整坨

                //if (string.IsNullOrEmpty(wsRes))
                //{
                //    LOG.Info("Hospital_csh_Api.GetToken() Get Null Or Empty.");
                //    return "";
                //}

                //var wsRes_Hash = JsonConvert.DeserializeObject<Hashtable>(wsRes);

                //LOG.Info("Hospital_csh_Api.GetToken() Get result Message: " + wsRes_Hash["Message"].TONotNullString());

                //if (wsRes_Hash["Message"].TONotNullString() == "成功")
                //{
                //    var wsRes_Data = JsonConvert.DeserializeObject<Hashtable>(wsRes_Hash["Data"].TONotNullString());
                //    return wsRes_Data["AccessToken"].TONotNullString();
                //}
                //else
                //{
                //    return "";
                //}
            }
            catch (Exception ex)
            {
                LOG.Info("Hospital_csh_Api.GetToken() Error: " + ex.ToString());
            }
            finally
            {
                LOG.Info("Hospital_csh_Api.GetToken() End.");
            }
            return "";
        }

        /// <summary>
        /// 取得 病歷類型 列表<br/>
        /// 民眾查詢可申請病歷(A1)
        /// </summary>
        public static IList<Api_A1ResultModel> Api_A1(string idno, string birth, string ec_sdate, string ec_edate)
        {
            var Result = new List<Api_A1ResultModel>();
            try
            {
                LOG.Info("Hospital_csh_Api.Api_A1() Begin.");

                ServicePointManager.SecurityProtocol =
                         SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                         SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;

                var wsToken = Hospital_csh_Api.GetToken();

                if (!string.IsNullOrEmpty(wsToken))
                {
                    // 中山要丟有斜線的日期格式
                    var date1 = DateTime.ParseExact(birth, "yyyyMMdd", null).ToString("yyyy/MM/dd");
                    var date2 = DateTime.ParseExact(ec_sdate, "yyyyMMdd", null).ToString("yyyy/MM/dd");
                    var date3 = DateTime.ParseExact(ec_edate, "yyyyMMdd", null).ToString("yyyy/MM/dd");

                    // 測試用
                    if (Models.ConfigModel.level1OnOrOff != "1")
                    {
                        //date1 = "1966/03/02";
                        //date2 = "2024/07/05";
                        //date3 = "2024/07/11";
                    }

                    tw.org.csh.sysint.MedRecApply wsObj = new tw.org.csh.sysint.MedRecApply();
                    var wsRes = wsObj.GetPtEMRList(wsToken, idno, date1, date2, date3);

                    var wsRes_Hash = JsonConvert.DeserializeObject<Hashtable>(wsRes);
                    if (wsRes_Hash["Message"].TONotNullString() == "成功")
                    {
                        Result = JsonConvert.DeserializeObject<List<Api_A1ResultModel>>(wsRes_Hash["Data"].TONotNullString());
                        return Result;
                    }
                    LOG.Info("Hospital_csh_Api.Api_A1() Get result Message: " + wsRes_Hash["Message"].TONotNullString());
                }
                else
                {
                    LOG.Info("Hospital_csh_Api.Api_A1() Token Is Null Or Empty.");
                }
            }
            catch (Exception ex)
            {
                LOG.Info("Hospital_csh_Api.Api_A1() Error: " + ex.ToString());
            }
            finally
            {
                LOG.Info("Hospital_csh_Api.Api_A1() End.");
            }
            return Result;
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

        public static string Api_A1_Remark(Api_A1ResultModel model)
        {
            var Result = "";
            if (!string.IsNullOrEmpty(model.ec_dept)) Result = Result + "_" + model.ec_dept;
            if (!string.IsNullOrEmpty(model.ec_doctor)) Result = Result + "_" + model.ec_doctor;
            if (!string.IsNullOrEmpty(model.ec_dateText)) Result = Result + "_" + model.ec_dateText;
            if (!string.IsNullOrEmpty(model.ec_date)) Result = Result + ":" + model.ec_date;
            return Result;
        }

        public static IList<Api_A2_1_ResultModel> Api_A2_1(string idno, string birth, string caseNo, List<Api_A2_1_ParamsModel> data)
        {
            var Result = new List<Api_A2_1_ResultModel>();
            try
            {
                LOG.Info("Hospital_csh_Api.Api_A2_1() Begin.");

                ServicePointManager.SecurityProtocol =
                         SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                         SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;

                var wsToken = Hospital_csh_Api.GetToken();

                if (!string.IsNullOrEmpty(wsToken))
                {
                    tw.org.csh.sysint.MedRecApply wsApply = new tw.org.csh.sysint.MedRecApply();
                    var postJSON = JsonConvert.SerializeObject(data);
                    var wsRes = wsApply.EMRApply(wsToken, idno, birth, caseNo, postJSON);

                    var wsRes_Hash = JsonConvert.DeserializeObject<Hashtable>(wsRes);
                    if (wsRes_Hash["Message"].TONotNullString() == "成功")
                    {
                        Result = JsonConvert.DeserializeObject<List<Api_A2_1_ResultModel>>(wsRes_Hash["Data"].TONotNullString());
                        return Result;
                    }
                    else
                    {
                        // 失敗時？
                    }
                    LOG.Info("Hospital_csh_Api.Api_A2_1() Get result Message: " + wsRes_Hash["Message"].TONotNullString());
                }
                else
                {
                    LOG.Info("Hospital_csh_Api.Api_A2_1() Token Is Null Or Empty.");
                }
            }
            catch (Exception ex)
            {
                LOG.Info("Hospital_csh_Api.Api_A2_1() Error: " + ex.ToString());
            }
            finally
            {
                LOG.Info("Hospital_csh_Api.Api_A2_1() End.");
            }
            return Result;
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
    }
}