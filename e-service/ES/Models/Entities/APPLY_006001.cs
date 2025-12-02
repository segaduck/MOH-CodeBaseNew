using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPPLY_006001
    {
        /// <summary>
        /// 
        /// </summary>
        public string APP_ID { get; set; }
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }
        [Display(Name = "同上資料（被保險人與申請人為同一人）")]
        public string ISSAME { get; set; }
        [Display(Name = "被保險人姓名")]
        public string R_NAME { get; set; }
        [Display(Name = "被保險人出生年月日")]
        public DateTime? R_BIRTH { get; set; }
        [Display(Name = "被保險人身分證統一編號")]
        public string R_IDN { get; set; }
        [Display(Name = "被保險人地址郵遞區號")]
        public string R_ADDR_CODE { get; set; }
        [Display(Name = "被保險人詳細地址")]
        public string R_ADDR { get; set; }
        [Display(Name = "被保險人手機號碼")]
        public string R_MOBILE { get; set; }
        [Display(Name = "被保險人連絡電話")]
        public string R_TEL { get; set; }
        [Display(Name = "不服勞保局核定文件")]
        public string KINDTYPE { get; set; }
        [Display(Name ="文號日期")]
        public DateTime? LIC_DATE { get; set; }
        [Display(Name ="文號(字)")]
        public string LIC_CD { get; set; }
        [Display(Name ="文號(號)")]
        public string LIC_NUM { get; set; }
        [Display(Name ="繳款單年度")]
        public string PAY_YEAR { get; set; }
        [Display(Name ="繳款單月份")]
        public string PAY_MONTH { get; set; }
        [Display(Name ="繳款單字號")]
        public string PAY_NUM { get; set; }
        [Display(Name ="收受或知悉日期")]
        public DateTime? KNOW_DATE { get; set; }
        [Display(Name ="請求事項")]
        public string KNOW_MEMO { get; set; }
        [Display(Name = "事實及理由")]
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