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
    public class Apply_005005Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 製造廠地址
        /// </summary>
        [Display(Name = "製造廠地址")]
        public string MF_ADDR { get; set; }

        /// <summary>
        /// 衛部中藥廠證號
        /// </summary>
        [Display(Name = "衛部中藥廠證號")]
        public string LIC_NUM { get; set; }

        /// <summary>
        /// 查廠日期
        /// </summary>
        [Display(Name = "最近一次GMP查廠日期")]
        public DateTime? ISSUE_DATE { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        [Display(Name = "GMP有效日期")]
        public DateTime? EXPIR_DATE { get; set; }

        /// <summary>
        /// 核備函(最近一期查廠之核備函公文)
        /// </summary>
        [Display(Name = "本部核發藥物製造或展延許可函影本")]
        public string ATTACH_1 { get; set; }

        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        public int? COPIES { get; set; }

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
        /// 製造業藥商許可執照影本
        /// </summary>
        [Display(Name = "製造業藥商許可執照影本")]
        public string ATTACH_2 { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        [Display(Name = "公司名稱")]
        public string COMP_NAME { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string RADIOYN { get; set; }

        /// <summary>
        /// 製造商名稱
        /// </summary>
        [Display(Name = "製造商名稱")]
        public string MF_CNT_NAME { get; set; }
        
    }
}