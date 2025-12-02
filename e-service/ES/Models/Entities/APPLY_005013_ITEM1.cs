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
    public class APPLY_005013_ITEM1
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
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
        /// 申請項目項次
        /// </summary>
        [Display(Name = "申請項目項次")]
        public string ITEMNUM { get; set; }

        /// <summary>
        /// 申請貨品
        /// </summary>
        [Display(Name = "申請貨品")]
        public string COMMODITIES { get; set; }

        /// <summary>
        /// 申請貨品備註
        /// </summary>
        [Display(Name = "申請貨品備註")]
        public string COMMODMEMO { get; set; }

        /// <summary>
        /// 申請規格
        /// </summary>
        [Display(Name = "申請規格")]
        public string SPEC { get; set; }

        /// <summary>
        /// 申請數量
        /// </summary>
        [Display(Name = "申請數量")]
        public string QTY { get; set; }

        /// <summary>
        /// 申請數量單位代碼
        /// </summary>
        [Display(Name = "申請數量單位代碼")]
        public string UNIT { get; set; }

        /// <summary>
        /// 申請數量單位名稱
        /// </summary>
        [Display(Name = "申請數量單位名稱")]
        public string UNIT_TEXT { get; set; }

        /// <summary>
        /// 規格數量
        /// </summary>
        [Display(Name = "規格數量")]
        public string SPECQTY { get; set; }
        /// <summary>
        /// 規格數量單位
        /// </summary>
        [Display(Name = "規格數量單位代碼")]
        public string SPECUNIT { get; set; }
        /// <summary>
        /// 規格數量單位名稱
        /// </summary>
        [Display(Name = "規格數量單位名稱")]
        public string SPECUNIT_TEXT { get; set; }
        /// <summary>
        /// 產品類別
        /// </summary>
        [Display(Name = "產品類別")]
        public string PORCTYPE { get; set; }
        /// <summary>
        /// 劑型
        /// </summary>
        [Display(Name = "劑型")]
        public string COMMODTYPE { get; set; }
    }
}