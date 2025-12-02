using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using ES.Utils;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using log4net;
using System.IO.Compression;

namespace ES.Commons
{
    /// <summary>
    /// 串接「MyData 數位服務個人化網站」相關 
    /// </summary>
    public class MyData
    {
        protected static readonly ILog logger = LogManager.GetLogger(typeof(MyData));

        public MyData()
        {

        }

        /// <summary>
        /// 以指定的 key(clientSecret) 及 iv(cbcIv) 對 plainText(通常是身分證號) 進行加密
        /// <para>這是跟 java AES/CBC/PKCS5PADDING 等效的加密方式</para>
        /// <para>回傳加密後資料經 Base64 encoding 的字串</para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string AESEncrypt(string key, string iv, string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return null;

            using (AesManaged aes = new AesManaged())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);

                if (key.Length == 16)
                {
                    // AES256 key 為 256bits (32 bytes)
                    key = key + key;
                }

                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        byte[] bytes = (byte[])ms.ToArray();
                        return Convert.ToBase64String(bytes);
                    }
                }
            }
        }

        /// <summary>
        /// 以指定的 key(clientSecret) 及 iv(cbcIv) 對 encryptStr 進行解密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="encryptStr"></param>
        /// <returns></returns>
        public string AESDecrypt(string key, string iv, string encryptStr)
        {
            if (string.IsNullOrEmpty(encryptStr))
                return null;

            return AESDecrypt(key, iv, Encoding.UTF8.GetBytes(encryptStr));
        }

        /// <summary>
        /// 以指定的 key(clientSecret) 及 iv(cbcIv) 對 encryptStr 進行解密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="encryptStr"></param>
        /// <returns></returns>
        public string AESDecrypt(string key, string iv, byte[] encryptData)
        {
            if (encryptData == null || encryptData.Length == 0)
                return null;

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptData, 0, encryptData.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }


        /// <summary>
        /// 取得 MyData ReturnUrl 帶回狀態碼說明文字
        /// <para>參考 MyData「服務提供者技術文件」</para>
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetReturnCodeDesc(string code)
        {
            Dictionary<string, string> descs = new Dictionary<string, string>();

            descs.Add("200", "OK");
            descs.Add("205", "User 不同意傳送資料給 SP");
            descs.Add("206", "超過 DP 資料集當日請求之上限");
            descs.Add("400", "無法順利解析 SP 帶入的 path parameter");
            descs.Add("401", "權限錯誤、不允許此 IP 連線、SP 所請求的 resoruce_id 不屬於 MyData 管理後臺中所登錄的設定、未完成身分驗證或身分驗證失敗、無法順利解密");
            descs.Add("403", "拒絕存取、參數（tx_id 或 client_id）不存在");
            descs.Add("404", "sp_return_url 不符合 MyData 管理後臺 中所登錄的設定");
            descs.Add("408", "交易逾時");
            descs.Add("409", "身分衝突、用戶身分證字號檢核失敗、SP 傳送的 pid 與用戶於 MyData 填寫的身分證字號不符");
            descs.Add("410", "SP-API 呼叫失敗");
            descs.Add("423", "服務已中止、暫時無法開放服務");
            descs.Add("501", "SP 請求的 DP 資料集之系統已停止服務");
            descs.Add("504", "SP 請求的 DP 資料集之系統異常，無法傳送DP資料集");

            if (descs.ContainsKey(code))
            {
                return code + " " + descs[code];
            }
            else
            {
                return code + " (沒有定義)";
            }
        }

        /// <summary>
        /// 取得 MyData /service/data 回應 HTTP Status 狀態碼說明文字
        /// <para>參考 MyData「服務提供者技術文件」</para>
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public string GetServiceDataStatusDesc(string statusCode)
        {
            Dictionary<string, string> descs = new Dictionary<string, string>();

            descs.Add("200", "OK");
            descs.Add("201", "RESOURCE CREATED");
            descs.Add("302", "此IP未加入白名單，無法成功連線至MyData");
            descs.Add("400", "參數格式或內容不正確，或是缺少必要參數");
            descs.Add("401", "權限錯誤、不允許此 IP 連線");
            descs.Add("403", "拒絕存取、參數（permission_ticket）不存在");
            descs.Add("408", "交易逾時");
            descs.Add("429", "TOO_MANY_REQUESTS");
            descs.Add("504", "SP 請求的 DP 資料集之系統異常，無法傳送DP資料集");

            if (descs.ContainsKey(statusCode))
            {
                return statusCode + " " + descs[statusCode];
            }
            else
            {
                return statusCode + " (沒有定義)";
            }
        }

        /// <summary>
        /// 以傳入的 permission_ticket 從 MyData-API Endpoint /service/data 取回資料
        /// <para>實際取回資料的邏輯是在獨立的 thread 中運作，這裡只是啟動而已，啟動後會立即返回。</para>
        /// </summary>
        /// <param name="myDataHost"></param>
        /// <param name="permission_ticket"></param>
        /// <param name="secret_key"></param>
        /// <param name="savePath"></param>
        /// <param name="callback"></param>
        public void GetServiceData(string myDataHost, string tx_id, string permission_ticket, string secret_key, string savePath, IMyDataServiceCallback callback)
        {
            if(string.IsNullOrEmpty(myDataHost))
            {
                throw new ArgumentNullException("myDataHost");
            }
            if (string.IsNullOrEmpty(tx_id))
            {
                throw new ArgumentNullException("tx_id");
            }
            if (string.IsNullOrEmpty(permission_ticket))
            {
                throw new ArgumentNullException("permission_ticket");
            }
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            try
            {
                //  啟動背景處理程序
                Action<CancellationToken> action = token => DoGetServiceData(token, myDataHost, tx_id, permission_ticket, secret_key, savePath, callback);
                HostingEnvironment.QueueBackgroundWorkItem(action);
            }
            catch (Exception ex)
            {
                logger.Error("GetServiceData: " + ex.Message, ex);
            }
        }


        private void DoGetServiceData(CancellationToken cancellationToken, string myDataHost, string tx_id, string permission_ticket, string secret_key, string savePath, IMyDataServiceCallback callback)
        {
            string info = "tx_id=" + tx_id + ", permission_ticket=" + permission_ticket;
            logger.Info("DoGetServiceData(" + info + "): INIT.");

            try
            {
                logger.Info("DoGetServiceData(" + info + "): START.");

                string apiUrl = myDataHost + "/service/data";
                Hashtable headers = new Hashtable();
                headers.Add("permission_ticket", permission_ticket);

                bool retry = false;
                do
                {
                    MyHttpRequest http = new MyHttpRequest();
                    string mydataJwe = http.request("GET", apiUrl, headers, null, "application/json");
                    string statusCode = http.GetStatusCode();
                    string statusCodeDesc = this.GetServiceDataStatusDesc(statusCode);
                    Dictionary<string, string> responseHeaders = http.GetResponseHeaders();

                    int retryAfter = 0;
                    retry = false;

                    if ("429".Equals(statusCode))  // TOO_MANY_REQUESTS
                    {
                        // 讀取 Retry-After header, 等待後重新發出 Request
                        retryAfter = 60;
                        if (responseHeaders.ContainsKey("Retry-After"))
                        {
                            if (!int.TryParse(responseHeaders["Retry-After"], out retryAfter))
                            {
                                retryAfter = 60;
                            }
                        }

                        logger.Info("DoGetServiceData(" + info + "): TOO_MANY_REQUESTS, Retry-After: " + retryAfter + " Seconds.");

                        if (retryAfter > 0)
                        {
                            retry = true;
                            Thread.Sleep(retryAfter * 1000);
                        }
                    }
                    else if ("200".Equals(statusCode))  // OK  
                    {
                        // 交易請求成功
                        // content 是 MyData 回傳的 JWE(JSON Web Encryption) compact serialization 格式資料 (RFC7516)
                        // 裝內容加密金鑰 (Content Encryption Key, CEK) 使用 A256KW（AES Key Wrap using 256-bit key）演算法，
                        // 加密內容使用 A256CBC-HS512（AES_256_CBC_HMAC_SHA_512）演算法

                        logger.Info("DoGetServiceData(" + info + "): mydataJwe [" + mydataJwe + "]");

                        // 解密 JWE 資料
                        string jsonData = Jose.JWT.Decode(mydataJwe, Encoding.UTF8.GetBytes(secret_key) );
                        logger.Debug("DoGetServiceData(" + info + "): Decrypted: \n" + jsonData);

                        string realSavePath = ProcessPackJsonData(savePath, tx_id, jsonData);

                        callback.OnServiceDataDone(tx_id, statusCode, statusCodeDesc, realSavePath);
                    }
                    else
                    {
                        // 交易失敗
                        logger.Info("DoGetServiceData(" + info + "): " + statusCodeDesc);
                        callback.OnServiceDataDone(tx_id, statusCode, statusCodeDesc, null);
                    }
                }
                while (retry);

            }
            catch(Exception ex)
            {
                logger.Error("DoGetServiceData(" + info + "): ERROR, " + ex.Message, ex);
                callback.OnServiceDataDone(tx_id, "500", "系統異常: " + ex.Message, null);
            }
            finally
            {
                logger.Info("DoGetServiceData(" + info + "): END.");
            }

        }

        /// <summary>
        /// 取出及儲存 MyData zip 打包檔案，並將其中的 zip 打包檔案逐一解壓縮
        /// <para>作業完成回傳實際存檔路徑</para>
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="tx_id"></param>
        /// <param name="jsonData"></param>
        public string ProcessPackJsonData(string savePath, string tx_id, string jsonData)
        {
            MyDataSevicePayload payload = Newtonsoft.Json.JsonConvert.DeserializeObject<MyDataSevicePayload>(jsonData);

            // 取出 MyData zip 打包檔案資料
            string dataBase64 = payload.data.Substring("application/zip;data:".Length);
            // 因為 MyData 系統是 java base, 
            // 對於 Base64 encode 的邏輯跟 C# 有點差異, 要進行下列置換, C# 才能正確 decode
            dataBase64 = dataBase64.Replace('-', '+').Replace('_', '/');
            byte[] data = Convert.FromBase64String(dataBase64);

            if(!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string packZipFile = Path.Combine(savePath, tx_id + ".zip");

            if(File.Exists(packZipFile))
            {
                throw new Exception("MyData zip 打包檔案 " + packZipFile + " 已存在");
            }

            logger.Debug("ProcessPackJsonData: " + packZipFile);
            using (FileStream fs = File.Create(packZipFile))
            {
                fs.Write(data, 0, data.Length);
            }

            // 解壓縮打包檔案
            string unzipPath = Path.Combine(savePath, tx_id);
            ZipFile.ExtractToDirectory(packZipFile, unzipPath);

            // 找出打包中的 resource zip 檔案, 並逐一解壓縮
            string[] fileEntries = Directory.GetFiles(unzipPath);
            for(int i=0; i<fileEntries.Length; i++)
            {
                if(fileEntries[i].EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    string filename = Path.GetFileName(fileEntries[i]);
                    string subPath = Path.Combine(unzipPath, filename.Substring(0, filename.LastIndexOf(".")));
                    ZipFile.ExtractToDirectory(fileEntries[i], subPath);
                }
            }

            return unzipPath;
        }

    }

    /// <summary>
    /// 從 MyData  /service/data 取回 JWE 資料解密後字串, 
    /// 經 JSON Deserialize 得到的 payload 資料物件  
    /// </summary>
    public class MyDataSevicePayload
    {
        public string filename { get; set; }

        public string data { get; set; }
    }

    /// <summary>
    /// 當呼叫 GetServiceData() 完成 MyData /service/data 的呼叫並取得回應後, 
    /// 會透過這個 interface callback 回傳 MyData 回應的資料 
    /// </summary>
    public interface IMyDataServiceCallback
    {
        void OnServiceDataDone(string tx_id, string statusCode, string statusCodeDesc, string dataSavePath);
    }
}