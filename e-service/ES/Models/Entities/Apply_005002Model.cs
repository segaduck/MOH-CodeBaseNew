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
    public class Apply_005002Model
    {
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "製造廠名稱")]
        public string MF_CNT_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "製造廠名稱(英文)")]
        public string MF_CNT_NAME_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "製造廠連絡電話")]
        public string MF_CNT_TEL { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "製造廠地址")]
        public string MF_ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "製造廠英文地址")]
        public string MF_ADDR_E { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "藥品名稱")]
        public string DRUG_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "藥品英文名稱")]
        public string DRUG_NAME_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "外銷品名")]
        public string DRUG_ABROAD_NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "外銷品名(英文)")]
        public string DRUG_ABROAD_NAME_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "劑型")]
        public string DOSAGE_FORM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "劑型(英文)")]
        public string DOSAGE_FORM_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "藥品許可證名稱")]
        public string LIC_CD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "藥品許可證字號")]
        public string LIC_NUM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "藥品許可證英文名稱")]
        public string LIC_CD_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "藥品許可證英文字號")]
        public string LIC_NUM_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "核准日期")]
        public DateTime? ISSUE_DATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "有效日期")]
        public DateTime? EXPIR_DATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "處方說明")]
        public string MF_CONT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "處方說明(英文)")]
        public string MF_CONT_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "生藥與浸膏比例-1")]
        public string PC_SCALE_1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "生藥與浸膏比例-2")]
        public string PC_SCALE_1E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "生藥與浸膏比例-3")]
        public string PC_SCALE_2E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "生藥與浸膏比例-4")]
        public string PC_SCALE_21 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "生藥與浸膏比例-5")]
        public string PC_SCALE_22 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "生藥與浸膏比例-6")]
        public string PC_SCALE_23 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "生藥與浸膏比例-7")]
        public string PC_SCALE_24 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "適應症中文")]
        public string INDIOCATION { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "適應症英文")]
        public string INDIOCATION_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "效能中文")]
        public string EFFICACY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "效能英文")]
        public string EFFICACY_E { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ATTACH_1 { get; set; }

        /// <summary>
        /// 
        /// </summary>

        [Display(Name = "備註")]
        public string MF_REMARK { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "申請份數")]
        public int? MF_COPIES { get; set; }

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
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        public string MERGE_YN { get; set; }

    }
}