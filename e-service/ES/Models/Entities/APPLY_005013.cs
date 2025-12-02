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
    public class APPLY_005013
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
        /// 生產國別代碼
        /// </summary>
        [Display(Name = "生產國別代碼")]
        public string ORIGIN { get; set; }

        /// <summary>
        /// 生產國別
        /// </summary>
        [Display(Name = "生產國別")]
        public string ORIGIN_TEXT { get; set; }

        /// <summary>
        /// 賣方國家代碼
        /// </summary>
        [Display(Name = "賣方國家代碼")]
        public string SELLER { get; set; }

        /// <summary>
        /// 賣方國家
        /// </summary>
        [Display(Name = "賣方國家")]
        public string SELLER_TEXT { get; set; }

        /// <summary>
        /// 起運國家代碼
        /// </summary>
        [Display(Name = "起運國家代碼")]
        public string SHIPPINGPORT { get; set; }

        /// <summary>
        /// 起運國家
        /// </summary>
        [Display(Name = "起運國家")]
        public string SHIPPINGPORT_TEXT { get; set; }

        /// <summary>
        /// 案件類型
        /// </summary>
        [Display(Name = "案件類型")]
        public string APP_TYPE { get; set; }

        /// <summary>
        /// 個人用途
        /// </summary>
        [Display(Name = "個人用途")]
        public string RADIOUSAGE { get; set; }

        /// <summary>
        /// 個人用途說明
        /// </summary>
        [Display(Name = "個人用途說明")]
        public string RADIOUSAGE_TEXT { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string RADIOYN { get; set; }
        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }

    }
}