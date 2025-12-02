using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Models.Share;
using Omu.ValueInjecter;
using System.Collections;
using ES.Models;
using System.Configuration;
using System.Data.SqlClient;
using ES.Action;
using ES.Utils;
using System.Text;
using System.Data;
using ES.Models.ChineseMedicineAPI;
using ES.Services;
using Dapper;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ES.DataLayers
{
    public class APIDAO : BaseAction
    {
        //private readonly string PATH = "http://localhost:12595/WebService_SaleLicense.json";
        #region GMP
        public GMPAPI GetGMPLicense(string LIC_NUM)
        {

            var data = new GMPAPI();
            try
            {
                // API網址
                string url = "https://chinesemedicinelicensesystem.mohw.gov.tw/API/WebService/GMPLicense";
                //string url = "http://localhost/WebService/GMPLicense";
                ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
               (sender, cert, chain, sslPolicyErrors) => true;

                NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("LICNO", LIC_NUM);
                byte[] byteArray = Encoding.UTF8.GetBytes(postParams.ToString());
                logger.Debug("GetGMPLicense:" + LIC_NUM);

                var webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.UserAgent = "Nothing";
                webRequest.Method = "POST";
                using (Stream reqStream = webRequest.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var contributorsAsJson = sr.ReadToEnd();
                        dynamic temp = JsonConvert.DeserializeObject(contributorsAsJson);
                        data.STATUS = Convert.ToString(temp["STATUS"]);
                        if (Convert.ToString(temp["STATUS"]) ==  "成功")
                        {
                            var record = Convert.ToString(temp["中藥GMP證明書"]);
                            var tempFinal = JsonConvert.DeserializeObject(record);

                            data.GMPResp.名稱 = Convert.ToString(tempFinal["名稱"]);
                            data.GMPResp.醫事機構代碼 = Convert.ToString(tempFinal["醫事機構代碼"]);
                            data.GMPResp.郵遞區號 = Convert.ToString(tempFinal["郵遞區號"]);
                            data.GMPResp.營業地址 = Convert.ToString(tempFinal["營業地址"]);
                            data.GMPResp.負責人 = Convert.ToString(tempFinal["負責人"]);
                            data.GMPResp.查廠日期 = Convert.ToString(tempFinal["查廠日期"]);
                            data.GMPResp.有效期限 = Convert.ToString(tempFinal["有效期限"]);
                            logger.Debug("GetGMPLicense:Success");
                            logger.Debug($"名稱:{data.GMPResp.名稱};醫事機構代碼:{data.GMPResp.醫事機構代碼};郵遞區號:{data.GMPResp.郵遞區號};營業地址:{data.GMPResp.營業地址};負責人:{data.GMPResp.負責人};查廠日期:{data.GMPResp.查廠日期};有效期限:{data.GMPResp.有效期限}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetGMPLicense failed" + ex.TONotNullString());
                data.STATUS = "失敗";
            }

            return data;
        }
        #endregion

        #region SALE
        public SALEAPI GetSALELicense(string LICWORD,string LIC_NUM)
        {
            var data = new SALEAPI();
            try
            {
                // API網址
                string url = "https://chinesemedicinelicensesystem.mohw.gov.tw/API/WebService/SaleLicense";
                //string url = "http://localhost/WebService/SaleLicense";
                ServicePointManager.SecurityProtocol =
                                  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                  SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback +=
               (sender, cert, chain, sslPolicyErrors) => true;
                ShareCodeListModel dropdown = new ShareCodeListModel();
                var LICWORD_LIST = dropdown.PList.Where(m => m.Value == LICWORD);
                var LICWORD_TEXT = "";
                if(LICWORD_LIST.ToCount() > 0)
                {
                    LICWORD_TEXT = LICWORD_LIST.FirstOrDefault().Text.ToLeft(4);
                }

                NameValueCollection postParams = System.Web.HttpUtility.ParseQueryString(string.Empty);
                postParams.Add("LICWORD", LICWORD_TEXT);
                postParams.Add("LICNO", LIC_NUM);
                byte[] byteArray = Encoding.UTF8.GetBytes(postParams.ToString());
                logger.Debug($"GetSALELicense:{LICWORD_TEXT}.{LIC_NUM}");

                var webRequest = WebRequest.Create(url) as HttpWebRequest;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.UserAgent = "Nothing";
                webRequest.Method = "POST";
                using (Stream reqStream = webRequest.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var contributorsAsJson = sr.ReadToEnd();
                        dynamic temp = JsonConvert.DeserializeObject(contributorsAsJson);
                        data.STATUS = Convert.ToString(temp["STATUS"]);
                        if (Convert.ToString(temp["STATUS"]) == "成功")
                        {
                            var record = Convert.ToString(temp["產銷證明書"]);
                            var tempFinal = JsonConvert.DeserializeObject(record);
                            data.SALEResp.製造廠名稱 = Convert.ToString( tempFinal["製造廠名稱"]);
                            data.SALEResp.製造廠地址 = Convert.ToString(tempFinal["製造廠地址"]);
                            data.SALEResp.中文品名 = Convert.ToString(tempFinal["中文品名"]);
                            data.SALEResp.英文品名 = Convert.ToString(tempFinal["英文品名"]);
                            logger.Debug($"製造廠名稱:{data.SALEResp.製造廠名稱};製造廠地址:{data.SALEResp.製造廠地址};中文品名:{data.SALEResp.中文品名};英文品名:{data.SALEResp.英文品名}");

                            var tempOutList = (JArray)JsonConvert.DeserializeObject(Convert.ToString(tempFinal["外銷品名"]));
                            data.SALEResp.外銷品名 = new List<SALEAPI_DRUGNAME>();
                            foreach (var item in tempOutList)
                            {
                                SALEAPI_DRUGNAME drug = new SALEAPI_DRUGNAME();
                                drug.外銷中文品名 = Convert.ToString(item["外銷中文品名"]);
                                drug.外銷英文品名 = Convert.ToString(item["外銷英文品名"]);
                                data.SALEResp.外銷品名.Add(drug);
                                logger.Debug($"外銷品名:{drug.外銷中文品名}.{drug.外銷英文品名}");
                            }

                            //data.SALEResp.外銷品名 = tempFinal["外銷品名"].TONotNullString();

                            data.SALEResp.藥品劑型 = Convert.ToString(tempFinal["藥品劑型"]);
                            data.SALEResp.發證日期 = Convert.ToString(tempFinal["發證日期"]);
                            data.SALEResp.有效日期 = Convert.ToString(tempFinal["有效日期"]);
                            data.SALEResp.處方說明 = Convert.ToString(tempFinal["處方說明"]);
                            logger.Debug($"藥品劑型:{data.SALEResp.藥品劑型};發證日期:{data.SALEResp.發證日期};有效日期:{data.SALEResp.有效日期};處方說明:{data.SALEResp.處方說明}");

                            var tempSolList = (JArray)JsonConvert.DeserializeObject(Convert.ToString(tempFinal["成分說明"]));
                            data.SALEResp.成分說明 = new List<SALEAPI_DI>();
                            foreach (var item in tempSolList)
                            {
                                SALEAPI_DI di = new SALEAPI_DI();
                                di.成分內容 = Convert.ToString(item["成分內容"]);
                                di.份量 = Convert.ToString(item["份量"]);
                                di.單位 = Convert.ToString(item["單位"]);
                                data.SALEResp.成分說明.Add(di);
                                logger.Debug($"成分內容:{di.成分內容};份量:{di.份量};單位:{di.單位}");
                            }

                           // data.SALEResp.成分說明 = tempFinal["成分說明"].TONotNullString();

                            data.SALEResp.效能 = Convert.ToString(tempFinal["效能"]);
                            data.SALEResp.適應症 = Convert.ToString(tempFinal["適應症"]);
                            data.SALEResp.限制1 = Convert.ToString(tempFinal["限制1"]);
                            data.SALEResp.限制2 = Convert.ToString(tempFinal["限制2"]);
                            data.SALEResp.限制3 = Convert.ToString(tempFinal["限制3"]);
                            data.SALEResp.限制4 = Convert.ToString(tempFinal["限制4"]);
                            logger.Debug($"效能:{data.SALEResp.效能}");
                            logger.Debug($"適應症:{data.SALEResp.適應症}");
                            logger.Debug($"限制1:{data.SALEResp.限制1}");
                            logger.Debug($"限制2:{data.SALEResp.限制2}");
                            logger.Debug($"限制3:{data.SALEResp.限制3}");
                            logger.Debug($"限制4:{data.SALEResp.限制4}");

                            if ((data.SALEResp.限制1 + "_" + data.SALEResp.限制2 + "_" + data.SALEResp.限制3 + "_" + data.SALEResp.限制4).Contains("外銷專用"))
                            {
                                data.STATUS = "外銷專用";
                            }
                       
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetSALELicense failed" + ex.TONotNullString());
                data.STATUS = "失敗";
            }

            return data;
        }
        #endregion
    }
}
