using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001005 醫事人員證書補(換)發
    /// </summary>
    public class Apply_001005Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 補(換)發
        /// </summary>
        [Display(Name = "補(換)發")]
        public string ACTION_TYPE { get; set; }
        /// <summary>
        /// 核發單位
        /// </summary>
        [Display(Name = "核發單位")]
        public string ISSUE_DEPT { get; set; }
        /// <summary>
        /// 核發日期
        /// </summary>
        [Display(Name = "核發日期")]
        public DateTime? ISSUE_DATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LIC_TYPE { get; set; }
        /// <summary>
        /// 證書類型
        /// </summary>
        [Display(Name = "證書類型")]
        public string LIC_CD { get; set; }
        /// <summary>
        /// 證書字號
        /// </summary>
        [Display(Name = "證書字號")]
        public string LIC_NUM { get; set; }
        /// <summary>
        /// 換證原因
        /// </summary>
        [Display(Name = "換證原因")]
        public string ACTION_RES { get; set; }
        /// <summary>
        /// 換證其他原因
        /// </summary>
        [Display(Name = "換證其他原因")]
        public string OTHER_RES { get; set; }
        /// <summary>
        /// 科別
        /// </summary>
        [Display(Name = "科別")]
        public string DIVISION { get; set; }
        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }
        /// <summary>
        /// 郵寄日期
        /// </summary>
        [Display(Name = "郵寄日期")]
        public DateTime? MAIL_DATE { get; set; }
        /// <summary>
        /// 掛號條碼
        /// </summary>
        [Display(Name = "掛號條碼")]
        public string MAIL_BARCODE { get; set; }
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