using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Models.Entities
{
    /// <summary>
    /// 貨品進口專案申請資料表-申請項目
    /// </summary>
    public class Apply_005014_Item
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請項目編號 12,3,...N
        /// </summary>
        public int? ITEM { get; set; }

        /// <summary>
        /// 類型 
        /// </summary>
        public int? ITEM_TYPE { get; set; }

        /// <summary>
        /// 類型-其它
        /// </summary>
        public string ITEM_TYPE_ANOTHER { get; set; }

        /// <summary>
        /// 類型補正
        /// </summary>
        public string ITEM_TYPE_E { get; set; }

        /// <summary>
        /// 貨名 
        /// </summary>
        public string COMMODITIES { get; set; }

        /// <summary>
        /// 貨名 補正
        /// </summary>
        public string COMMODITIES_E { get; set; }

        /// <summary>
        /// 申請數量 
        /// </summary>
        public string  QTY { get; set; }

        /// <summary>
        /// 申請數量 補正
        /// </summary>
        public string QTY_E { get; set; }

        /// <summary>
        /// 申請數量單位 
        /// </summary>
        public string UNIT { get; set; }

        /// <summary>
        /// 單位 補正
        /// </summary>
        public string UNIT_E { get; set; }
        /// <summary>
        /// 規格數量
        /// </summary>
        public string SPECQTY { get; set; }
        /// <summary>
        /// 規格數量 補正
        /// </summary>
        public string SPECQTY_E { get; set; }
        /// <summary>
        /// 規格數量單位
        /// </summary>
        public string SPECUNIT { get; set; }
        /// <summary>
        /// 規格數量單位 補正
        /// </summary>
        public string SPECUNIT_E { get; set; }
        /// <summary>
        /// 產品類別
        /// </summary>
        public string PORCTYPE { get; set; }
        /// <summary>
        /// 產品類別 補正
        /// </summary>
        public string PORCTYPE_E { get; set; }
        /// <summary>
        /// 劑型
        /// </summary>
        public string COMMODTYPE { get; set; }
        /// <summary>
        /// 劑型 補正
        /// </summary>
        public string COMMODTYPE_E { get; set; }
        /// <summary>
        /// 出口國
        /// </summary>
        public string AFF1_EXPORT_COUNTRY { get; set; }
        /// <summary>
        /// 出口國 補正
        /// </summary>
        public string AFF1_EXPORT_COUNTRY_E { get; set; }
        /// <summary>
        /// 報單號碼
        /// </summary>
        public string AFF1_SHEET_NO { get; set; }
        /// <summary>
        /// 報單號碼 補正
        /// </summary>
        public string AFF1_SHEET_NO_E { get; set; }
        /// <summary>
        /// 進口關
        /// </summary>
        public string AFF1_IMPORT_COUNTRY { get; set; }
        /// <summary>
        /// 進口關 補正
        /// </summary>
        public string AFF1_IMPORT_COUNTRY_E { get; set; }
        /// <summary>
        /// 出口國
        /// </summary>
        public string AFF2_EXPORT_COUNTRY { get; set; }
        /// <summary>
        /// 出口國 補正
        /// </summary>
        public string AFF2_EXPORT_COUNTRY_E { get; set; }
        /// <summary>
        /// 原中藥材含量名稱
        /// </summary>
        public string AFF2_AMOUNT_NAME { get; set; }
        /// <summary>
        /// 原中藥材含量名稱 補正
        /// </summary>
        public string AFF2_AMOUNT_NAME_E { get; set; }
        /// <summary>
        /// 原中藥材含量數量
        /// </summary>
        public string AFF2_AMOUNT { get; set; }
        /// <summary>
        /// 原中藥材含量數量 補正
        /// </summary>
        public string AFF2_AMOUNT_E { get; set; }
        /// <summary>
        /// 報單號碼
        /// </summary>
        public string AFF2_SHEET_NO { get; set; }
        /// <summary>
        /// 報單號碼 補正
        /// </summary>
        public string AFF2_SHEET_NO_E { get; set; }
        /// <summary>
        /// 進口關
        /// </summary>
        public string AFF2_IMPORT_COUNTRY { get; set; }
        /// <summary>
        /// 進口關 補正
        /// </summary>
        public string AFF2_IMPORT_COUNTRY_E { get; set; }
    }
}