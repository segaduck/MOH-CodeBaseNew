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
    public class Apply_005004Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請類別
        /// </summary>
        [Display(Name = "申請類別")]
        public string APPLY_TYPE { get; set; }

        /// <summary>
        /// 製造廠地址
        /// </summary>
        [Display(Name = "製造廠地址")]
        public string MF_ADDR { get; set; }

        /// <summary>
        /// 製造廠許可編號
        /// </summary>
        [Display(Name = "製造廠許可編號")]
        public string LIC_NUM { get; set; }

        /// <summary>
        /// 查廠日期
        /// </summary>
        [Display(Name = "查廠日期")]
        public DateTime? ISSUE_DATE { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        [Display(Name = "有效日期")]
        public DateTime? EXPIR_DATE { get; set; }

        /// <summary>
        /// 核備函(最近一期查廠之核備函公文)
        /// </summary>
        [Display(Name = "核備函(最近一期查廠之核備函公文)")]
        public string ATTACH_1 { get; set; }

        /// <summary>
        /// 藥品優良製造證明書申請表用印之掃描檔
        /// </summary>
        [Display(Name = "藥品優良製造證明書申請表用印之掃描檔")]
        public string ATTACH_2 { get; set; }

        /// <summary>
        /// 本部核發藥物製造或展延許可函影本
        /// </summary>
        [Display(Name = "本部核發藥物製造或展延許可函影本")]
        public string ATTACH_3 { get; set; }

        /// <summary>
        /// 製造業藥商許可執照影本
        /// </summary>
        [Display(Name = "製造業藥商許可執照影本")]
        public string ATTACH_4 { get; set; }

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
        /// 藥商許可執照字號
        /// </summary>
        [Display(Name = "藥商許可執照字號藥商許可執照字號")]
        public string PL_CD { get; set; }

        /// <summary>
        /// 藥商許可執照字號
        /// </summary>
        [Display(Name = "藥商許可執照字號")]
        public string PL_NUM { get; set; }

        /// <summary>
        /// 監製藥師
        /// </summary>
        [Display(Name = "監製藥師")]
        public string PP_NAME { get; set; }

        /// <summary>
        /// 工廠登記證字號
        /// </summary>
        [Display(Name = "工廠登記證字號")]
        public string FRC_NUM { get; set; }

        /// <summary>
        /// 原料藥勾選
        /// </summary>
        [Display(Name = "原料藥勾選")]
        public string TRA_CHECK { get; set; }

        /// <summary>
        /// 製劑勾選
        /// </summary>
        [Display(Name = "製劑勾選")]
        public string CON_CHECK { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string RADIOYN { get; set; }

        /// <summary>
        /// 公文文號(申請人填寫)
        /// </summary>
        [Display(Name = "公文文號(申請人填寫)")]
        public string MOHW_CASE_NO_SELF { get; set; }

    }
}