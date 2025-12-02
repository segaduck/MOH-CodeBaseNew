using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001009 醫事人員資格英文求證
    /// </summary>
    public class Apply_001009Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 申請事由
        /// </summary>
        [Display(Name = "申請事由")]
        public string APPLY_CAUSE { get; set; }
        /// <summary>
        /// 申請事由說明
        /// </summary>
        [Display(Name = "申請事由說明")]
        public string APPLY_CAUSE_TEXT { get; set; }
        /// <summary>
        /// 申請證明類別
        /// </summary>
        [Display(Name = "申請證明類別")]
        public string APPLY_CERT_CATE { get; set; }
        /// <summary>
        /// 醫事人員證書代號
        /// </summary>
        [Display(Name = "醫事人員或公共衛生師證書代號")]
        public string LIC_TYPE { get; set; }
        /// <summary>
        /// 醫事人員證書類型
        /// </summary>
        [Display(Name = "醫事人員或公共衛生師證書類型")]
        public string LIC_CD { get; set; }
        /// <summary>
        /// 醫事人員證書字號
        /// </summary>
        [Display(Name = "醫事人員或公共衛生師證書字號")]
        public string LIC_NUM { get; set; }
        /// <summary>
        /// 醫事人員證書日期
        /// </summary>
        [Display(Name = "醫事人員或公共衛生師證書日期")]
        public DateTime? CERT_APPROVED_DATE { get; set; }
        /// <summary>
        /// 畢業學校名稱及所在縣市(醫事證照核發時之畢業學校名稱)中文
        /// </summary>
        [Display(Name = "畢業學校名稱及所在縣市(醫事證照核發時之畢業學校名稱)中文")]
        public string C_SCHOOL_NAME { get; set; }
        /// <summary>
        /// 畢業學校名稱及所在縣市(醫事證照核發時之畢業學校名稱)英文
        /// </summary>
        [Display(Name = "畢業學校名稱及所在縣市(醫事證照核發時之畢業學校名稱)英文")]
        public string E_SCHOOL_NAME { get; set; }
        /// <summary>
        /// 修業年限起
        /// </summary>
        [Display(Name = "修業年限起")]
        public string STUDY_START_YM { get; set; }
        /// <summary>
        /// 修業年限迄
        /// </summary>
        [Display(Name = "修業年限迄")]
        public string STUDY_END_YM { get; set; }
        /// <summary>
        /// 本求證表郵寄機構地址(含國別及收件者)
        /// </summary>
        [Display(Name = "本求證表郵寄機構地址(含國別及收件者)")]
        public string VERIFY_ADDRESS { get; set; }

        /// <summary>
        /// VERIFY_NAME
        /// </summary>
        [Display(Name = "機構名稱")]
        public string VERIFY_NAME { get; set; }
        
        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE_FILE { get; set; }
        /// <summary>
        /// 聯絡人電子郵件
        /// </summary>
        [Display(Name = "聯絡人電子郵件")]
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

        /// <summary>
        /// 國別
        /// </summary>
        [Display(Name = "國別")]
        public string MAIL_COUNTRY { get; set; }

        /// <summary>
        /// 收件者
        /// </summary>
        [Display(Name = "收件者")]
        public string RECEIVER { get; set; }

        /// <summary>
        /// 檔案上傳一
        /// </summary>
        [Display(Name = "檔案上傳一")]
        public string IS_FILE1 { get; set; }

        /// <summary>
        /// 檔案上傳二
        /// </summary>
        [Display(Name = "檔案上傳二")]
        public string IS_FILE2 { get; set; }

        /// <summary>
        /// 檔案上傳三
        /// </summary>
        [Display(Name = "檔案上傳三")]
        public string IS_FILE3 { get; set; }

        /// <summary>
        /// 檔案上傳四
        /// </summary>
        [Display(Name = "檔案上傳四")]
        public string IS_FILE4 { get; set; }

        /// <summary>
        /// 檔案上傳五
        /// </summary>
        [Display(Name = "檔案上傳五")]
        public string IS_FILE5 { get; set; }

    }
}