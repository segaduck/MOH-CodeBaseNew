using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001037 醫事人員請領無懲戒紀錄證明申請書
    /// </summary>
    public class Apply_001037Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
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
        /// 發證日期
        /// </summary>
        [Display(Name = "發證日期")]
        public DateTime? ISSUE_DATE { get; set; }
        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        public int? COPIES { get; set; }
        /// <summary>
        /// 應付金額
        /// </summary>
        [Display(Name = "應付金額")]
        public int? TOTAL { get; set; }
        /// <summary>
        /// 收件者
        /// </summary>
        [Display(Name = "收件者")]
        public string MAIL_REC { get; set; }
        /// <summary>
        /// 郵寄地址
        /// </summary>
        [Display(Name = "郵寄地址")]
        public string MAIL_ADDR { get; set; }
        /// <summary>
        /// 郵寄國別
        /// </summary>
        [Display(Name = "郵寄國別")]
        public string MAIL_COUNTRY { get; set; }
        /// <summary>
        /// 申請理由
        /// </summary>
        [Display(Name = "申請理由")]
        public string REASON_1 { get; set; }
        /// <summary>
        /// 出國使用
        /// </summary>
        [Display(Name = "出國使用")]
        public string REASON_2 { get; set; }
        /// <summary>
        /// 出國其他原因說明
        /// </summary>
        [Display(Name = "出國其他原因說明")]
        public string REASON_2_DESC { get; set; }
        /// <summary>
        /// 前往國家
        /// </summary>
        [Display(Name = "前往國家")]
        public string REASON_3 { get; set; }
        /// <summary>
        /// 其他國家說明
        /// </summary>
        [Display(Name = "其他國家說明")]
        public string REASON_3_DESC { get; set; }
        /// <summary>
        /// 國內使用
        /// </summary>
        [Display(Name = "國內使用")]
        public string REASON_4 { get; set; }
        /// <summary>
        /// 國內其他原因說明
        /// </summary>
        [Display(Name = "國內其他原因說明")]
        public string REASON_4_DESC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[Display(Name = "")]
        public string REASON_5 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[Display(Name = "")]
        public string REASON_5_DESC { get; set; }
        /// <summary>
        /// 附檔名稱
        /// </summary>
        [Display(Name = "附檔名稱1")]
        public string ATTACH_1 { get; set; }
        /// <summary>
        /// 附檔名稱
        /// </summary>
        [Display(Name = "附檔名稱2")]
        public string ATTACH_2 { get; set; }
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
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE_FILE { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Display(Name = "郵遞區號")]
        public string ADDR_CODE { get; set; }

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
    }
}