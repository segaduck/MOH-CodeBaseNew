using ES.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_011002Model
    {
        /// <summary>
        /// 申請案件編號
        /// </summary>
        [Display(Name = "申請案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請用途
        /// </summary>
        [Display(Name = "申請用途")]
        public string APPLY_TYPE { get; set; }

        /// <summary>
        /// 專科類別
        /// </summary>
        [Display(Name = "專科類別")]
        public string SPECIALIST_TYPE { get; set; }

        /// <summary>
        /// 申請人電話(公)
        /// </summary>
        [Display(Name ="申請人電話(公)")]
        public string W_TEL { get; set; }

        /// <summary>
        /// 申請人電話(宅)
        /// </summary>
        [Display(Name = "申請人電話(宅)")]
        public string H_TEL { get; set; }

        /// <summary>
        /// 申請人通訊郵遞區號
        /// </summary>
        [Display(Name = "申請人通訊郵遞區號")]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人通訊地址")]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 申請人戶籍郵遞區號
        /// </summary>
        [Display(Name = "申請人戶籍郵遞區號")]
        public string H_ZIPCODE { get; set; }

        /// <summary>
        /// 申請人戶籍地址
        /// </summary>
        [Display(Name = "申請人戶籍地址")]
        public string H_ADDR { get; set; }

        /// <summary>
        /// 同通訊地址
        /// </summary>
        [Display(Name = "同通訊地址")]
        public string H_EQUAL { get; set; }

        /// <summary>
        /// 申請人EMAIL
        /// </summary>
        [Display(Name = "申請人EMAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 申請人執業處所
        /// </summary>
        [Display(Name = "申請人執業處所")]
        public string PRACTICE_PLACE { get; set; }

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        public string TEST_YEAR { get; set; }

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        public string MERGEYN { get; set; }

        /// <summary>
        /// 身分證正面影本
        /// </summary>
        public string FILE_IDNF { get; set; }
        
        /// <summary>
        /// 身分證反面影本
        /// </summary>
        public string FILE_IDNB { get; set; }
        
        /// <summary>
        /// 大頭貼相片
        /// </summary>
        public string FILE_PHOTO { get; set; }

        /// <summary>
        /// 戶籍謄本或戶口名簿影本
        /// </summary>
        public string FILE_HOUSEHOLD { get; set; }

        /////<summary>
        ///// 通知補件勾選的清單
        ///// </summary>
        //public string CHKNOTICE { get; set; }

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