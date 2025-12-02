using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPPLY_011004
    {
        /// <summary>
        /// 
        /// </summary>
        public string APP_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "聯絡人E-MAIL")]
        public string EMAIL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "電話(公司)")]
        public string C_TEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "手機號碼")]
        public string MOBILE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "通訊地址(郵遞區號)")]
        public string C_ZIP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "")]
        public string H_ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "申請用途")]
        public string APPLY_FOR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "申請份數")]
        public string APPLY_NUM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "考試年度")]
        public string YEAR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "考試名稱類科")]
        public string TYPE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IS_MERGE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FILE_NAME { get; set; }
    }
}