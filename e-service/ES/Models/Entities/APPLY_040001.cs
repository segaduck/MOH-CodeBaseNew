using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPPLY_040001
    {
        /// <summary>
        /// 
        /// </summary>
        public string APP_ID { get; set; }
        /// <summary>
        /// 代表人姓名
        /// </summary>
        [Display(Name = "代表人姓名")]
        public string CHR_NAME { get; set; }
        [Display(Name = "代表人出生年月日")]
        public DateTime? CHR_BIRTH { get; set; }
        [Display(Name = "代表人身分證明文件字號")]
        public string CHR_IDN { get; set; }
        [Display(Name = "代表人地址郵遞區號")]
        public string CHR_ADDR_CODE { get; set; }
        [Display(Name = "代表人詳細地址")]
        public string CHR_ADDR { get; set; }
        [Display(Name = "代表人手機號碼")]
        public string CHR_MOBILE { get; set; }
        [Display(Name = "代表人連絡電話")]
        public string CHR_TEL { get; set; }
        /// <summary>
        /// 代理人姓名
        /// </summary>
        [Display(Name = "代理人姓名")]
        public string R_NAME { get; set; }
        [Display(Name = "代理人出生年月日")]
        public DateTime? R_BIRTH { get; set; }
        [Display(Name = "代理人身分證明文件字號")]
        public string R_IDN { get; set; }
        [Display(Name = "代理人地址郵遞區號")]
        public string R_ADDR_CODE { get; set; }
        [Display(Name = "代理人詳細地址")]
        public string R_ADDR { get; set; }
        [Display(Name = "代理人手機號碼")]
        public string R_MOBILE { get; set; }
        [Display(Name = "代理人連絡電話")]
        public string R_TEL { get; set; }
        /// <summary>
        /// 原行政處分機關
        /// </summary>
        [Display(Name = "原行政處分機關")]
        public string ORG_NAME { get; set; }
        [Display(Name = "訴願人收受或知悉行政處分之年月日")]
        public DateTime? ORG_DATE { get; set; }
        [Display(Name = "訴願請求(即請求撤銷之行政處分書發文日期、文號或其他)")]
        public string ORG_MEMO { get; set; }
        [Display(Name = "事實與理由")]
        public string ORG_FACT { get; set; }

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

        [Display(Name ="EMAIL")]
        public string EMAIL { get; set; }
    }
}