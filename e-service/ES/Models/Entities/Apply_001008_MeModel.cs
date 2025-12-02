using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001008_ME 醫事人員請領英文證明書_醫事人員證書
    /// </summary>
    public class Apply_001008_MeModel
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 申請序號
        /// </summary>
        [Display(Name = "申請序號")]
        public int? SRL_NO { get; set; }
        /// <summary>
        /// 醫事人員證書類型
        /// </summary>
        [Display(Name = "醫事人員或公共衛生師證書類型")]
        public string ME_LIC_TYPE { get; set; }
        /// <summary>
        /// 證書字號
        /// </summary>
        [Display(Name = "證書字號")]
        public string ME_LIC_NUM { get; set; }
        /// <summary>
        /// 核發日期
        /// </summary>
        [Display(Name = "核發日期")]
        public DateTime? ME_ISSUE_DATE { get; set; }
        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        public int? ME_COPIES { get; set; }
        /// <summary>
        /// 開立證明格式
        /// </summary>
        [Display(Name = "開立證明格式")]
        public string ME_TYPE_CD { get; set; }
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