using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_001038_GoodsViewModel : Apply_001038_GoodsModel
    {
        /// <summary>
        /// 貨品類別(規格)代碼
        /// </summary>
        [Required]
        [Display(Name = "貨品類別(規格)")]
        public string GOODS_TYPE_ID { get; set; }

        /// <summary>
        /// 貨品名稱
        /// </summary>
        [Required]
        [Display(Name = "貨品名稱")]
        public string GOODS_NAME { get; set; }

        /// <summary>
        /// 申請數量
        /// </summary>
        [Required]
        [Display(Name = "申請數量")]
        public int? APPLY_CNT { get; set; }

        /// <summary>
        /// 數量單位代碼
        /// </summary>
        [Required]
        [Display(Name = "數量單位代碼")]
        public string GOODS_SPEC_1 { get; set; }

        /// <summary>
        /// 每單位容量
        /// </summary>
        [Required]
        [Display(Name = "每單位容量")]
        public string GOODS_SPEC_2 { get; set; }

        /// <summary>
        /// 型號一
        /// </summary>
        [Display(Name = "型號")]
        public string GOODS_MODEL { get; set; }


        /// <summary>
        /// 牌名
        /// </summary>
        [Display(Name = "牌名")]
        public string GOODS_BRAND { get; set; }

        /// <summary>
        /// 貨品輔助描述
        /// </summary>
        [Required]
        [Display(Name = "貨品輔助描述")]
        public string GOODS_DESC { get; set; }
    }
}