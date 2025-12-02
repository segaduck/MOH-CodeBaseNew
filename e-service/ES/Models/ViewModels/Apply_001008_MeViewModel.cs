using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_001008_ME 醫事人員請領英文證明書_醫事人員證書
    /// </summary>
    public class Apply_001008_MeViewModel : Apply_001008_MeModel
    {
        /// <summary>
        /// 醫事人員證書類別
        /// </summary>
        //[Required]
        [Display(Name = "醫事人員或公共衛生師證書")]
        public string ME_LIC_TYPE { get; set; }

        /// <summary>
        /// 證書字號
        /// </summary>
        //[Required]
        [Display(Name = "證書字號")]
        public string ME_LIC_NUM { get; set; }

        /// <summary>
        /// 核發日期
        /// </summary>
        //[Required]
        [Display(Name = "核發日期")]
        public string ME_ISSUE_DATE_AD { get; set; }

        /// <summary>
        /// 申請份數
        /// </summary>
        //[Required]
        [Display(Name = "申請份數")]
        public int? ME_COPIES { get; set; }

        /// <summary>
        /// 開立證明格式(1:分別開立 / 2:整併一張)
        /// </summary>
        //[Required]
        [Display(Name = "開立證明格式")]
        public string ME_TYPE_CD { get; set; }
    }
}