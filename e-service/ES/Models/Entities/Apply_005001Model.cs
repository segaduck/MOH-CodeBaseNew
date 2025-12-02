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
    public class Apply_005001Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 製造廠名稱
        /// </summary>
        [Display(Name = "製造廠名稱")]
        public string MF_CNT_NAME { get; set; }

        /// <summary>
        /// 製造廠英文名稱
        /// </summary>
        [Display(Name = "製造廠英文名稱")]
        public string MF_CNT_NAME_E { get; set; }

        /// <summary>
        /// 製造廠連絡電話
        /// </summary>
        [Display(Name = "製造廠連絡電話")]
        public string MF_CNT_TEL { get; set; }

        /// <summary>
        /// 製造廠地址
        /// </summary>
        [Display(Name = "製造廠地址")]
        public string MF_ADDR { get; set; }

        /// <summary>
        /// 製造廠英文地址
        /// </summary>
        [Display(Name = "製造廠英文地址")]
        public string MF_ADDR_E { get; set; }

        /// <summary>
        /// 藥品名稱
        /// </summary>
        [Display(Name = "藥品名稱")]
        public string DRUG_NAME { get; set; }

        /// <summary>
        /// 藥品英文名稱
        /// </summary>
        [Display(Name = "藥品英文名稱")]
        public string DRUG_NAME_E { get; set; }

        /// <summary>
        /// 外銷品名(中文)
        /// </summary>
        [Display(Name = "外銷品名(中文)")]
        public string DRUG_ABROAD_NAME { get; set; }

        /// <summary>
        /// 外銷品名(英文)
        /// </summary>
        [Display(Name = "外銷品名(英文)")]
        public string DRUG_ABROAD_NAME_E { get; set; }

        /// <summary>
        /// 劑型(中文)
        /// </summary>
        [Display(Name = "劑型(中文)")]
        public string DOSAGE_FORM { get; set; }

        /// <summary>
        /// 劑型(英文)
        /// </summary>
        [Display(Name = "劑型(英文)")]
        public string DOSAGE_FORM_E { get; set; }

        /// <summary>
        /// 許可證名稱
        /// </summary>
        [Display(Name = "許可證名稱")]
        public string LIC_CD { get; set; }

        /// <summary>
        /// 許可證字號
        /// </summary>
        [Display(Name = "許可證字號")]
        public string LIC_NUM { get; set; }

        /// <summary>
        /// 英文許可證名稱
        /// </summary>
        [Display(Name = "英文許可證名稱")]
        public string LIC_CD_E { get; set; }

        /// <summary>
        /// 英文許可證字號
        /// </summary>
        [Display(Name = "英文許可證字號")]
        public string LIC_NUM_E { get; set; }

        /// <summary>
        /// 核准日期
        /// </summary>
        [Display(Name = "核准日期")]
        public DateTime? ISSUE_DATE { get; set; }

        /// <summary>
        /// 有效日期
        /// </summary>
        [Display(Name = "有效日期")]
        public DateTime? EXPIR_DATE { get; set; }

        /// <summary>
        /// 處方說明中文
        /// </summary>
        [Display(Name = "處方說明中文")]
        public string MF_CONT { get; set; }

        /// <summary>
        /// 處方說明英文
        /// </summary>
        [Display(Name = "處方說明英文")]
        public string MF_CONT_E { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(分量)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(分量)")]
        public string PC_SCALE_1 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(中文單位)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(中文單位)")]
        public string PC_SCALE_1E { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(英文單位)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(英文單位)")]
        public string PC_SCALE_2E { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例1)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(比例1)")]
        public string PC_SCALE_21 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例2)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(比例2)")]
        public string PC_SCALE_22 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例3)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(比例3)")]
        public string PC_SCALE_23 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例4)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(比例4)")]
        public string PC_SCALE_24 { get; set; }

        /// <summary>
        /// 適應症中文
        /// </summary>
        [Display(Name = "適應症中文")]
        public string INDIOCATION { get; set; }

        /// <summary>
        /// 適應症英文
        /// </summary>
        [Display(Name = "適應症英文")]
        public string INDIOCATION_E { get; set; }

        /// <summary>
        /// 效能中文
        /// </summary>
        [Display(Name = "效能中文")]
        public string EFFICACY { get; set; }

        /// <summary>
        /// 效能英文
        /// </summary>
        [Display(Name = "效能英文")]
        public string EFFICACY_E { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        [Display(Name = "附件")]
        public string ATTACH_1 { get; set; }

        /// <summary>
        /// 製造商備註
        /// </summary>
        [Display(Name = "製造商備註")]
        public string MF_REMARK { get; set; }

        /// <summary>
        /// 申請份數
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
        /// 外銷
        /// </summary>
        [Display(Name = "外銷")]
        public string DRUG_ABROAD_CHECK { get; set; }

        /// <summary>
        /// 顯示有效日期
        /// </summary>
        [Display(Name = "顯示有效日期")]
        public string EXPIR_DATE_CHECK { get; set; }

        /// <summary>
        /// 是否為濃縮製劑
        /// </summary>
        [Display(Name = "是否為濃縮製劑")]
        public string Concentrate_CHECK { get; set; }

        /// <summary>
        /// 適應症
        /// </summary>
        [Display(Name = "適應症")]
        public string INDIOCATION_CHECK { get; set; }

        /// <summary>
        /// 效能
        /// </summary>
        [Display(Name = "效能")]
        public string EFFICACY_CHECK { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }
    }
}