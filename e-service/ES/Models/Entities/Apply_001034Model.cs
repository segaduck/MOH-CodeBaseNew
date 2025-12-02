using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001034 危險性醫療儀器進口申請作業
    /// </summary>
    public class Apply_001034Model
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 納稅人身分證字號/統一編號
        /// </summary>
        [Display(Name ="身分證字號/統一編號")]
        public string TAX_ORG_ID { get; set; }
        /// <summary>
        /// 納稅人姓名/公司名稱
        /// </summary>
        [Display(Name = "姓名/公司名稱")]
        public string TAX_ORG_NAME { get; set; }
        /// <summary>
        /// 納稅人英文姓名/英文公司名稱
        /// </summary>
        [Display(Name = "英文姓名/英文公司名稱")]
        public string TAX_ORG_ENAME { get; set; }
        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Display(Name = "郵遞區號")]
        public string TAX_ZIP_CODE { get; set; }
        /// <summary>
        /// 聯絡地址
        /// </summary>
        [Display(Name = "聯絡地址")]
        public string TAX_ORG_ADDR { get; set; }
        /// <summary>
        /// 聯絡英文地址
        /// </summary>
        [Display(Name = "聯絡英文地址")]
        public string TAX_ORG_EADDR { get; set; }
        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Display(Name = "聯絡人姓名")]
        public string TAX_ORG_MAN { get; set; }
        /// <summary>
        /// 聯絡人電話
        /// </summary>
        [Display(Name = "聯絡人電話")]
        public string TAX_ORG_TEL { get; set; }
        /// <summary>
        /// 聯絡人電子信箱
        /// </summary>
        [Display(Name = "聯絡人e-mail")] 
        public string TAX_ORG_EMAIL { get; set; }
        /// <summary>
        /// 聯絡人傳真
        /// </summary>
        [Display(Name = "聯絡人傳真")]
        public string TAX_ORG_FAX { get; set; }
        /// <summary>
        /// 進出口別
        /// </summary>
        [Display(Name = "進出口別")]
        public string IM_EXPORT { get; set; }
        /// <summary>
        /// 起始日期
        /// </summary>
        [Display(Name = "起始日期")]
        public DateTime? DATE_S { get; set; }
        /// <summary>
        /// 終止日期
        /// </summary>
        [Display(Name = "終止日期")]
        public DateTime? DATE_E { get; set; }
        /// <summary>
        /// 生產國家代號
        /// </summary>
        [Display(Name = "生產國家")]
        public string DEST_STATE_ID { get; set; }
        /// <summary>
        /// 生產國家名稱
        /// </summary>
        [Display(Name = "生產國家名稱")]
        public string DEST_STATE { get; set; }
        /// <summary>
        /// 賣方國家代號
        /// </summary>
        [Display(Name = "賣方國家")]
        public string SELL_STATE_ID { get; set; }
        /// <summary>
        /// 賣方國家名稱
        /// </summary>
        [Display(Name = "賣方國家名稱")]
        public string SELL_STATE { get; set; }
        /// <summary>
        /// 轉口港代號
        /// </summary>
        [Display(Name = "轉口港")]
        public string TRN_PORT_ID { get; set; }
        /// <summary>
        /// 轉口港名稱
        /// </summary>
        [Display(Name = "轉口港名稱")]
        public string TRN_PORT { get; set; }
        /// <summary>
        /// 起運口岸代號
        /// </summary>
        [Display(Name = "起運口岸")]
        public string BEG_PORT_ID { get; set; }
        /// <summary>
        /// 起運口岸名稱
        /// </summary>
        [Display(Name = "起運口岸名稱")]
        public string BEG_PORT { get; set; }
        /// <summary>
        /// 賣方英文名稱
        /// </summary>
        [Display(Name = "賣方英文名稱")]
        public string SELL_NAME { get; set; }
        /// <summary>
        /// 賣方英文地址
        /// </summary>
        [Display(Name = "賣方英文地址")]
        public string SELL_ADDR { get; set; }
        /// <summary>
        /// 用途代號
        /// </summary>
        [Display(Name = "申請用途")] 
        public string APP_USE_ID { get; set; }
        /// <summary>
        /// 用途代號
        /// </summary>
        [Display(Name = "用途代號")]
        public string APP_USE { get; set; }
        /// <summary>
        /// 用途說明
        /// </summary>
        [Display(Name = "用途說明")]
        public string USE_MARK { get; set; }
        /// <summary>
        /// 核發方式代號
        /// </summary>
        [Display(Name = "核發方式代號")] 
        public string CONF_TYPE_ID { get; set; }
        /// <summary>
        /// 核發方式名稱
        /// </summary>
        [Display(Name = "核發方式名稱")]
        public string CONF_TYPE { get; set; }
        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        public int? COPIES { get; set; }
        /// <summary>
        /// 公文文號
        /// </summary>
        [Display(Name = "公文文號")]
        public string MDOD_APPNO { get; set; }
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