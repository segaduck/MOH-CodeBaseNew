using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models.Entities
{
    /// <summary>
    /// 貨品進口專案申請資料表
    /// </summary>
    public class Apply_005014
    {
        /// <summary>
        /// 案件編號 
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        public string EMAIL_E { get; set; }

         /// <summary>
         ///生產國別
         /// </summary>
        [Display(Name = "生產國別")]
        public string PRODUCTION_COUNTRY { get; set; }


        public string PRODUCTION_COUNTRY_E { get; set; }

        // 賣方國家
        [Display(Name = "賣方國家")]
        public string SELL_COUNTRY { get; set; }

        public string SELL_COUNTRY_E { get; set; }

        // 起運國家
        [Display(Name = "起運國家")]
        public string TRANSFER_COUNTRY { get; set; }

        public string TRANSFER_COUNTRY_E { get; set; }

        // 起運口岸
        [Display(Name = "起運口岸")]
        public string TRANSFER_PORT { get; set; }

        public string TRANSFER_PORT_E { get; set; }

        // 貨品名稱
        [Display(Name = "貨品名稱")]
        public string AFF1_COMMODITIES { get; set; }

        public string AFF1_COMMODITIES_E { get; set; }

        // 貨品數量
        [Display(Name = "貨品數量")]
        public int? AFF1_QTY { get; set; }


        public string AFF1_QTY_E { get; set; }
   
        // 報單號碼項次
        [Display(Name = "報單號碼項次")]
        public string AFF1_NO { get; set; }

        public string AFF1_NO_E { get; set; }
        
        /// <summary>
        /// AFFIDAVIT2  Y|N 萃取物(提取物)切結書  勾選
        /// </summary>
        [Display(Name = "萃取物切結書")]
        public string AFFIDAVIT2 { get; set; }

        // 貨品名稱
        [Display(Name = "貨品名稱")]
        public string AFF2_COMMODITIES { get; set; }

        public string AFF2_COMMODITIES_E { get; set; }

        // 貨品數量
        [Display(Name = "貨品數量")]
        public int? AFF2_QTY { get; set; }

        public string AFF2_QTY_E { get; set; }

        // 報單號碼項次
        [Display(Name = "報單號碼項次")]
        public string AFF2_NO { get; set; }

        public string AFF2_NO_E { get; set; }

        /// <summary>
        /// 是否合併檔案
        /// </summary>
        [Display(Name = "是否合併檔案")]
        public string IS_MERGE_FILE { get; set; }

    }
}