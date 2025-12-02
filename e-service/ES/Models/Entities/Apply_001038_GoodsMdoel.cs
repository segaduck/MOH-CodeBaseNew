using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_001038_GoodsModel    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 貨品序號
        /// </summary>
        [Display(Name = "貨品序號")]
        public int? SRL_NO { get; set; }
        /// <summary>
        /// 貨品類別(規格)代碼
        /// </summary>
        [Display(Name = "貨品類別(規格)代碼")]
        public string GOODS_TYPE_ID { get; set; }
        /// <summary>
        /// 貨品類別(規格)名稱
        /// </summary>
        [Display(Name = "貨品類別(規格)名稱")]
        public string GOODS_TYPE { get; set; }
        /// <summary>
        /// 識別碼
        /// </summary>
        [Display(Name = "識別碼")]
        public string GOODS_SID { get; set; }
        /// <summary>
        /// 貨品名稱
        /// </summary>
        [Display(Name = "貨品名稱")]
        public string GOODS_NAME { get; set; }
        /// <summary>
        /// 申請數量
        /// </summary>
        [Display(Name = "申請數量")]
        public int? APPLY_CNT { get; set; }
        /// <summary>
        /// 數量單位代碼
        /// </summary>
        [Display(Name = "數量單位代碼")]
        public string GOODS_UNIT_ID { get; set; }
        /// <summary>
        /// 數量單位名稱
        /// </summary>
        [Display(Name = "數量單位名稱")]
        public string GOODS_UNIT { get; set; }
        /// <summary>
        /// 每單位容量
        /// </summary>
        [Display(Name = "每單位容量")]
        public string GOODS_MODEL { get; set; }
        /// <summary>
        /// 型號一
        /// </summary>
        [Display(Name = "型號一")]
        public string GOODS_SPEC_1 { get; set; }
        /// <summary>
        /// 型號二
        /// </summary>
        [Display(Name = "型號二")]
        public string GOODS_SPEC_2 { get; set; }
        /// <summary>
        /// 牌名
        /// </summary>
        [Display(Name = "牌名")]
        public string GOODS_BRAND { get; set; }
        /// <summary>
        /// 貨品輔助描述
        /// </summary>
        [Display(Name = "貨品輔助描述")]
        public string GOODS_DESC { get; set; }
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