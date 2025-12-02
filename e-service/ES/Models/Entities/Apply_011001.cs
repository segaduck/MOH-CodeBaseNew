using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class APPLY_011001
    {
        /// <summary>
        /// 
        /// </summary>
        [Display(Name ="案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "運用單位登入帳號")]
        public string ACC_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "運用單位名稱")]
        public string ACC_NAM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "運用單位電話")]
        public string ACC_TEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "運用單位地址(郵遞區號)")]
        public string ACC_ADDR_CODE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "運用單位地址")]
        public string ACC_ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "承辦人姓名")]
        public string ADM_NAM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "承辦人行動電話")]
        public string ADM_MOBILE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "承辦人E-MAIL")]
        public string ADM_MAIL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "運用單位發文日期")]
        public DateTime? ACC_SDATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "運用單位發文字號")]
        public string ACC_NUM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MERGEYN { get; set; }
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
        /// <summary>
        /// 
        /// </summary>
        public string FILE_SERVICE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FILE_UNIT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FILE_CERTIFICATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FILE_BASIC { get; set; }
    }
}