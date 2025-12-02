using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace EECOnline.Models
{
    /// <summary>
    /// 簡訊王發送 Model
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SmsMsgModel
    {
        public SmsMsgModel()
        {
            this.UID = "eisapi";
            this.PWD = "eisapi7654321";
        }

        /// <summary>
        ///  帳號
        /// </summary>
        [JsonProperty]
        public string UID { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [JsonProperty]
        public string PWD { get; set; }

        /// <summary>
        /// 目的門號
        /// </summary>
        [JsonProperty]
        public string DEST { get; set; }

        /// <summary>
        /// 簡訊主旨(不會隨著簡訊內容發送出，註記用)
        /// </summary>
        [JsonProperty]
        public string SB { get; set; }

        /// <summary>
        /// 簡訊發送內容
        /// ※利用api傳送資料時，請將所有傳送資料都以url編碼，可避免因特殊字元，使得傳送裁斷或顯示異常。
        /// </summary>
        [JsonProperty]
        public string MSG { get; set; }

        /// <summary>
        /// 簡訊預定發送時間。
        /// -立即發送：請傳入空字串
        /// -預約發送：請傳入預計發送時間，若傳送時間⼩於系統接單時間，將不予傳送。
        /// 格式為yyyyMMddHHmnss；例如:預約2009/01/31 15:30:00發送，則傳入 20090131153000。若傳遞時間已逾現在之時間，將立即發送。
        /// </summary>
        [JsonProperty]
        public string ST { get; set; }

        /// <summary>
        /// 簡訊有效期限
        /// 單位: 分鐘
        /// 若未指定, 則以該簡訊平台之預設1440分鐘帶出。
        /// 當接收手機未開機時,系統會重複再次發送簡訊給接收門號，若您的簡訊內容有時效性，您可設定若超過設定時間後就不再重複發送。
        /// </summary>
        [JsonProperty]
        public string RETRYTIME { get; set; }

        /// <summary>
        /// 容錯機制碼
        /// </summary>
        [JsonProperty]
        public string ftcode { get; set; }

        /// <summary>
        /// 發送簡訊是否成功的狀態回報網址, 若不宣告此參數時為不回報。
        /// </summary>
        [JsonProperty]
        public string Response { get; set; }

        /// <summary>
        /// 將內部參數轉為 Uri
        /// </summary>
        /// <returns></returns>
        public string GetParamsUriString()
        {
            var paramDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(this));

            var paramStr = "";
            foreach (var key in paramDic.Keys)
            {
                paramStr += "&" + key + "=" + HttpUtility.UrlEncode(paramDic[key], Encoding.GetEncoding(65001));    //utf8      //950:big5
            }
            return "?" + paramStr.Substring(1);
        }

        /// <summary>
        /// 將內部參數轉為 Uri username ,password
        /// </summary>
        /// <returns></returns>
        public string GetParamsUriString2()
        {
            var paramDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(this));

            var paramStr = "";
            foreach (var key in paramDic.Keys)
            {
                if (key == "UID" || key == "PWD")
                {
                    paramStr += "&" + key + "=" + HttpUtility.UrlEncode(paramDic[key], Encoding.GetEncoding(65001));    //utf8      //950:big5
                }
            }
            return "?" + paramStr.Substring(1);
        }
    }
}