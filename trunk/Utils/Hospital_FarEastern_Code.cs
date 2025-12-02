using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json;
using log4net;
using System.Net;

namespace EECOnline.Utils
{
    /// <summary>
    /// 亞東醫院 API 專用 Code
    /// </summary>
    public class Hospital_FarEastern_Code
    {
        private static readonly ILog LOG = LogUtils.GetLogger();

        private class PubKey
        {
            public string Pubkey { get; set; }
        }

        private class Hash
        {
            public static string GetHash(HashAlgorithm hashAlgorithm, string input)
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();
                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                // Return the hexadecimal string.
                return sBuilder.ToString();
            }

            // Verify a hash against a string.
            private static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
            {
                // Hash the input.
                var hashOfInput = GetHash(hashAlgorithm, input);
                // Create a StringComparer an compare the hashes.
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                return comparer.Compare(hashOfInput, hash) == 0;
            }
        }

        public class AesCrypto
        {
            public static string AesKey = "隨便輸入一組字串"; //密鑰
            public static string AesIv = "也是隨便輸入一組字串"; //密鑰向量

            /// <summary>
            /// AES 加密字串
            /// </summary>
            /// <param name="original">原始字串</param>
            /// <param name="key">自訂金鑰</param>
            /// <param name="iv">自訂向量</param>
            /// <returns></returns>
            public static string AesEncrypt(string original, string key = null, string iv = null)
            {
                key = string.IsNullOrEmpty(key) ? AesKey : key;
                iv = string.IsNullOrEmpty(iv) ? AesIv : iv;

                string encrypt = "";
                try
                {
                    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    byte[] dataByteArray = Encoding.UTF8.GetBytes(original);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (
                            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(keyData, ivData), CryptoStreamMode.Write)
                        )
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                            encrypt = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo...
                }

                return encrypt;
            }

            /// <summary>
            /// AES 解密字串
            /// </summary>
            /// <param name="hexString">已加密字串</param>
            /// <param name="key">自訂金鑰</param>
            /// <param name="iv">自訂向量</param>
            /// <returns></returns>
            public static string AesDecrypt(string hexString, string key = null, string iv = null)
            {
                key = string.IsNullOrEmpty(key) ? AesKey : key;
                iv = string.IsNullOrEmpty(iv) ? AesIv : iv;

                string decrypt = hexString;
                try
                {
                    SymmetricAlgorithm aes = new AesCryptoServiceProvider();
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    byte[] dataByteArray = Convert.FromBase64String(hexString);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (
                            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(keyData, ivData), CryptoStreamMode.Write)
                        )
                        {
                            cs.Write(dataByteArray, 0, dataByteArray.Length);
                            cs.FlushFinalBlock();
                            decrypt = Encoding.UTF8.GetString(ms.ToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo...
                }

                return decrypt;
            }

            public static bool TryAesDecrypt(string hexString, out string original, string key = null, string iv = null)
            {
                return hexString != (original = AesDecrypt(hexString, key, iv));
            }

            /// <summary>
            /// AES 加密檔案
            /// </summary>
            /// <param name="sourceFile">原始檔案路徑</param>
            /// <param name="encryptFile">加密後檔案路徑</param>
            /// <param name="key">自訂金鑰</param>
            /// <param name="iv">自訂向量</param>
            public static bool AesEncryptFile(string sourceFile, string encryptFile, string key = null, string iv = null)
            {
                key = string.IsNullOrEmpty(key) ? AesKey : key;
                iv = string.IsNullOrEmpty(iv) ? AesIv : iv;

                if (string.IsNullOrEmpty(sourceFile) || string.IsNullOrEmpty(encryptFile) || !File.Exists(sourceFile))
                {
                    return false;
                }

                try
                {
                    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    aes.Key = keyData;
                    aes.IV = ivData;

                    using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                    {
                        using (FileStream encryptStream = new FileStream(encryptFile, FileMode.Create, FileAccess.Write))
                        {
                            //檔案加密
                            byte[] dataByteArray = new byte[sourceStream.Length];
                            sourceStream.Read(dataByteArray, 0, dataByteArray.Length);

                            using (CryptoStream cs = new CryptoStream(encryptStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(dataByteArray, 0, dataByteArray.Length);
                                cs.FlushFinalBlock();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo...
                    return false;
                }

                return true;
            }

            /// <summary>
            /// AES 解密檔案
            /// </summary>
            /// <param name="encryptFile"></param>
            /// <param name="decryptFile"></param>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <returns></returns>
            public static bool AesDecryptFile(string encryptFile, string decryptFile, string key = null, string iv = null)
            {
                key = string.IsNullOrEmpty(key) ? AesKey : key;
                iv = string.IsNullOrEmpty(iv) ? AesIv : iv;

                if (string.IsNullOrEmpty(encryptFile) || string.IsNullOrEmpty(decryptFile) || !File.Exists(encryptFile))
                {
                    return false;
                }

                try
                {
                    AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                    byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                    byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));
                    aes.Key = keyData;
                    aes.IV = ivData;

                    using (FileStream encryptStream = new FileStream(encryptFile, FileMode.Open, FileAccess.Read))
                    {
                        using (FileStream decryptStream = new FileStream(decryptFile, FileMode.Create, FileAccess.Write))
                        {
                            byte[] dataByteArray = new byte[encryptStream.Length];
                            encryptStream.Read(dataByteArray, 0, dataByteArray.Length);
                            using (CryptoStream cs = new CryptoStream(decryptStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(dataByteArray, 0, dataByteArray.Length);
                                cs.FlushFinalBlock();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo...
                    return false;
                }

                return true;
            }
        }

        private static string getPubKEY()
        {
            string Result = "";
            try
            {
                string hash = "";
                string source = "ZHQuZQRCgk5TW2qE";
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    Hash Hash = new Hash();
                    hash = Hash.GetHash(sha256Hash, source);
                }

                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var postJSON = JsonConvert.SerializeObject(new PubKey()
                        {
                            Pubkey = hash
                        });
                        string webApiUrl = "https://emrdmz.femh.org.tw:9001/api/KeysController1";
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
                            PubKey objResult = JsonConvert.DeserializeObject<PubKey>(strResult);
                            Result = objResult.Pubkey;
                            sr.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Code.getPubKEY() Error: " + ex.Message);
                return "";
            }
            return Result;
        }

        public static string GetToken()
        {
            try
            {
                PubKey PubKey = new PubKey();
                PubKey.Pubkey = getPubKEY();
                AesCrypto AesCrypto = new AesCrypto();
                AesCrypto.AesKey = "kXt8er28Dn6QfSt4qAFx8UmhmCUKx7MCybwr76WXetYeWXzztehdPEB7ByUbzpga";
                AesCrypto.AesIv = "8Xuqn4fUwuBZDR2mEHyqDCupEp9DSQxbhKbNaEBReeeTQHVGGZKErybCAVcAmy7h";

                string Token = AesCrypto.AesEncrypt(PubKey.Pubkey, AesCrypto.AesKey, AesCrypto.AesIv);
                return Token;
            }
            catch (Exception ex)
            {
                LOG.Debug("EECOnline.Utils.Hospital_FarEastern_Code.GetToken() Error: " + ex.Message);
                return "";
            }
        }
    }
}