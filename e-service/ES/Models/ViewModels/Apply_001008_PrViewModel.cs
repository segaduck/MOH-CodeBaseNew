using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_001008_PR 醫事人員請領英文證明書_專科證書
    /// </summary>
    public class Apply_001008_PrViewModel : Apply_001008_PrModel
    {
        /// <summary>
        /// 專科人員證書類別
        /// </summary>
        //[Required]
        [Display(Name = "專科人員證書")]
        public string PR_LIC_TYPE { get; set; }

        /// <summary>
        /// 證書字號
        /// </summary>
        //[Required]
        [Display(Name = "證書字號")]
        public string PR_LIC_NUM { get; set; }

        /// <summary>
        /// 核發日期
        /// </summary>
        //[Required]
        [Display(Name = "核發日期")]
        public string PR_ISSUE_DATE_AD { get; set; }

        /// <summary>
        /// 有效日期起
        /// </summary>
        //[Required]
        [Display(Name = "有效日期起")]
        public string PR_EF_DATE_S_AD { get; set; }

        /// <summary>
        /// 有效日期迄
        /// </summary>
        //[Required]
        [Display(Name = "有效日期迄")]
        public string PR_EF_DATE_E_AD { get; set; }

        /// <summary>
        /// 申請份數
        /// </summary>
        //[Required]
        [Display(Name = "申請份數")]
        public int? PR_COPIES { get; set; }

        /// <summary>
        /// 開立證明格式
        /// </summary>
        //[Required]
        [Display(Name = "開立證明格式")]
        public string PR_TYPE_CD { get; set; }
    }
}