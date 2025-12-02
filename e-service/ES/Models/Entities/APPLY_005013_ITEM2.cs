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
    public class APPLY_005013_ITEM2
    {
        /// <summary>
        /// 
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
        /// 申請貨品名稱
        /// </summary>
        [Display(Name = "申請貨品名稱")]
        public string ITEMNAME { get; set; }

        /// <summary>
        /// 申請中藥用法
        /// </summary>
        [Display(Name = "申請中藥用法")]
        public string USAGE { get; set; }

        /// <summary>
        /// 申請中藥總數量
        /// </summary>
        [Display(Name = "申請中藥總數量")]
        public string ALLQTY { get; set; }

        /// <summary>
        /// 申請數量
        /// </summary>
        [Display(Name = "申請數量")]
        public string QTY { get; set; }
        /// <summary>
        /// 申請數量單位
        /// </summary>
        [Display(Name ="申請數量單位")]
        public string UNIT { get; set; }
        /// <summary>
        /// 規格數量
        /// </summary>
        [Display(Name ="規格數量")]
        public string SPECQTY { get; set; }
        /// <summary>
        /// 規劃數量單位
        /// </summary>
        [Display(Name = "規格數量單位")]
        public string SPECUNIT { get; set; }
    }
}