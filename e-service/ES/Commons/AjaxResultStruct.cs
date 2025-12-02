using System;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using log4net;
using log4net.Config;

namespace ES.Commons
{
    /// <summary>
    /// 針對異動作業類的 AJAX 呼叫, 封裝要回應的 JSON 格式
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AjaxResultStruct
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(AjaxResultStruct));

        /// <summary>
        /// 
        /// </summary>
        public AjaxResultStruct()
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //
            this.status = false;
        }

        /// <summary>
        /// JsonResultStruct的建構子
        /// </summary>
        /// <param name="isSuccess">異動作業的結果{true/false}</param>
        public AjaxResultStruct(bool isSuccess)
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //

            this.status = isSuccess;
        }

        /// <summary>
        /// 作業結果, true: 成功, false: 失敗
        /// </summary>
        [JsonProperty]
        public bool status { get; set; }

        /// <summary>
        /// 額外的結果訊息, 錯誤時的原因資訊
        /// </summary>
        [JsonProperty]
        //[JsonConverter(typeof(COMMON.jsStringEncodeConverter))]
        public string message { get; set; }

        /// <summary>
        /// 作業回應時間
        /// </summary>
        [JsonProperty]
        public DateTime time { get { return DateTime.Now; } }


        /// <summary>
        /// 用來輸出單筆結果資料
        /// </summary>
        [JsonProperty]
        public object data { get; set; }


        /// <summary>
        /// Serialize this AjaxResultStruct to JSON string
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            // 建立客制化的 DateTime 格式
            MyJsonDateTimeConverter dateTimeConvert = new MyJsonDateTimeConverter("yyyy'/'MM'/'dd' 'HH':'mm':'ss");
            
            // Serialize JsonResultStruct 本身並指定套用在 DateTime 物件的格式
            string jsonStr = JsonConvert.SerializeObject(this, dateTimeConvert);

            return jsonStr;
        }
    }
}