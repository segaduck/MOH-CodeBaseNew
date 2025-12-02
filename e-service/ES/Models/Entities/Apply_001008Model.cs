using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001008 醫事人員請領英文證明書
    /// </summary>
    public class Apply_001008Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 代號
        /// </summary>
        [Display(Name = "代號")]
        public string CATEGORY { get; set; }
        /// <summary>
        /// 英文姓名
        /// </summary>
        [Display(Name = "申請人英文別名")]
        public string ENAME_ALIAS { get; set; }
        /// <summary>
        /// 畢業學校名稱及所在地中文
        /// </summary>
        [Display(Name = "畢業學校名稱及所在地中文")]
        public string SCHOOL_C { get; set; }
        /// <summary>
        /// 畢業學校名稱及所在地英文
        /// </summary>
        [Display(Name = "畢業學校名稱及所在地英文")]
        public string SCHOOL_E { get; set; }
        /// <summary>
        /// 畢業年度
        /// </summary>
        [Display(Name = "畢業年度")]
        public string GRADUATE_YM { get; set; }
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
        public DateTime? LIC_DATE { get; set; }
        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        public int? COPIES { get; set; }
        /// <summary>
        /// 郵寄地址
        /// </summary>
        [Display(Name = "郵寄地址")]
        public string MAIL_ADDR { get; set; }
        /// <summary>
        /// 附件檔名1
        /// </summary>
        [Display(Name = "附件檔名1")]
        public string ATTACH_1 { get; set; }
        /// <summary>
        /// 附件檔名2
        /// </summary>
        [Display(Name = "附件檔名2")]
        public string ATTACH_2 { get; set; }
        /// <summary>
        /// 附件檔名3
        /// </summary>
        [Display(Name = "附件檔名3")]
        public string ATTACH_3 { get; set; }
        /// <summary>
        /// 應付金額
        /// </summary>
        [Display(Name = "應付金額")]
        public int? TOTAL_MEM { get; set; }
        /// <summary>
        /// 已繳費金額
        /// </summary>
        [Display(Name = "已繳費金額")]
        public int? TOTAL_SYS { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        [Display(Name = "備註")]
        public string REMARK { get; set; }
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
        /// 申請人電子郵件
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 佐證文件是否採合併檔案（Y:是 / N:否）
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

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
    }
}