using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPPLY_041001
    {
        /// <summary>
        /// 
        /// </summary>
        public string APP_ID { get; set; }
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }
        [Display(Name = "代理人姓名(單位名稱)")]
        public string R_NAME { get; set; }
        [Display(Name = "代理人身分證統一編號(或投保單位統一編號、醫事服務機構代號)")]
        public string R_IDN { get; set; }
        [Display(Name = "代理人地址郵遞區號")]
        public string R_ADDR_CODE { get; set; }
        [Display(Name = "代理人詳細地址")]
        public string R_ADDR { get; set; }
        [Display(Name = "代理人手機號碼")]
        public string R_MOBILE { get; set; }
        [Display(Name = "代理人連絡電話")]
        public string R_TEL { get; set; }
        [Display(Name ="文號日期")]
        public DateTime? LIC_DATE { get; set; }
        [Display(Name ="文號(字)")]
        public string LIC_CD { get; set; }
        [Display(Name ="文號(號)")]
        public string LIC_NUM { get; set; }
        [Display(Name ="核定文件(文號)")]
        public string KIND1 { get; set; }
        [Display(Name ="核定文件(繳款單)")]
        public string KIND2 { get; set; }
        [Display(Name ="核定文件(其他)")]
        public string KIND3 { get; set; }
        [Display(Name ="收受或知悉日期")]
        public DateTime? KNOW_DATE { get; set; }
        [Display(Name ="請求事項")]
        public string KNOW_MEMO { get; set; }
        [Display(Name ="事實及理由(請以條例方式，簡要敘明)")]
        public string KNOW_FACT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DEL_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_ACC { get; set; }

     
    }
}