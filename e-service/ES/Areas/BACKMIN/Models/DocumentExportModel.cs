using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Areas.Admin.Models
{
    public class DocumentExportModel
    {
        public class DocumentModel
        {

            public string APP_ID { get; set; }

            public string SRV_ID { get; set; }

            public string UNIT_NAME { get; set; }

            public string ADDRESS { get; set; }

            public string NAME { get; set; }

            public string TEL { get; set; }

            public string FAX { get; set; }

            public string MAIL { get; set; }

            public string SUBJECT { get; set; }

            public string CAPTION1 { get; set; }

            public string CAPTION2 { get; set; }

            public string CAPTION3 { get; set; }

            public string CAPTION4 { get; set; }

            public string CAPTION5 { get; set; }

            public string CAPTION6 { get; set; }

            public string CAPTION7 { get; set; }

            public string CAPTION8 { get; set; }
            public Document005 applyData { get; set; }
            // 貨品進口
            //public Document005014 Data005014 { get; set; }
            // 民眾少量自用中藥貨品進口
            //public Document005013 Data005013 { get; set; }
        }

        public class DocumentSet
        {
            [Required(ErrorMessage = "請輸入路徑")]
            public string path { set; get; }

            [Required(ErrorMessage = "請輸入Mail")]
            public string mail { set; get; }
        }
    }
    /// <summary>
    /// 民眾少量自用中藥貨品進口
    /// </summary>
    //public class Document005013
    //{
    //    // 姓名
    //    public string Apply_NAME { get; set; }
    //    // 身分證字號ID
    //    public string Apply_IDN { get; set; }
    //    // 地址
    //    public string Apply_ADDR { get; set; }
    //    // 電話
    //    public string CNT_TEL { get; set; }
    //    // 申請自用中藥資料
    //    // 申請藥品 〔項次〕+產品類別+貨名 ,多筆
    //    public string PRODUCTNAME { get; set; }
    //    // 申請數量 〔項次〕+申請數量+單位 ,多筆
    //    public string PRODUCTUNIT { get; set; }
    //    // 生產國別
    //    public string PRODUCTION_COUNTRY { get; set; }
    //    // 起運口岸
    //    public string TRANSFER_COUNTRY { get; set; }
    //    // 賣方國家
    //    public string SELL_COUNTRY { get; set; }
    //}
    /// <summary>
    /// 貨品進口
    /// </summary>
    public class Document005
    {
        // 公司名稱
        public string Apply_NAME { get; set; }
        // 統一編號
        public string Apply_IDN { get; set; }
        // 地址
        public string Apply_ADDR { get; set; }
        // 電話
        public string CNT_TEL { get; set; }
        // 申請進口貨品資料
        // 進口貨品名稱 產品類別 ITEM_TYPE +貨名 COMMODITIES ,多筆
        public string PRODUCTNAME { get; set; }
        // 申請數量 QTY +單位 UNIT ,多筆
        public string PRODUCTUNIT { get; set; }
        // 生產國別
        public string PRODUCTION_COUNTRY { get; set; }
        // 起運口岸
        public string TRANSFER_COUNTRY { get; set; }
        // 賣方國家
        public string SELL_COUNTRY { get; set; }
        // 承辦人姓名+電話+EMAIL CNT_NAME+CNT_TEL+CNT_EMAIL
        public string CNT_INFO { get; set; }
    }
}