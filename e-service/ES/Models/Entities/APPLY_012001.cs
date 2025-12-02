using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPPLY_012001
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請單位
        /// </summary>
        [Display(Name = "申請單位")]
        public int? APP_UNIT { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Display(Name = "申請日期")]
        public DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 申請角色
        /// </summary>
        [Display(Name = "申請角色")]
        public string APP_ROLE { get; set; }

        /// <summary>
        /// 申請人帳號
        /// </summary>
        [Display(Name = "申請人帳號")]
        public string ACC_NO { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        public string NAME { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 國民身分證統一編號
        /// </summary>
        [Display(Name = "國民身分證統一編號")]
        public string IDN { get; set; }

        /// <summary>
        /// 地址郵遞區號
        /// </summary>
        [Display(Name = "地址郵遞區號")]
        public string ADDR_CODE { get; set; }

        /// <summary>
        /// 地址詳細
        /// </summary>
        [Display(Name = "地址詳細")]
        public string ADDR { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        [Display(Name = "電話")]
        public string TEL { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string MAIL { get; set; }
        /// <summary>
        /// 確認連結
        /// </summary>
        [Display(Name = "是否點選EMAIL確認連結")]
        public string CHECK_FLAG { get; set; }
        /// <summary>
        /// 代理人
        /// </summary>
        [Display(Name = "代理人")]
        public string A_AGENT { get; set; }

        /// <summary>
        /// 代理人姓名
        /// </summary>
        [Display(Name = "代理人姓名")]
        public string E_NAME { get; set; }

        /// <summary>
        /// 代理人與申請人關係
        /// </summary>
        [Display(Name = "代理人與申請人關係")]
        public string AE_RELATION { get; set; }

        /// <summary>
        /// 代理人出生年月日
        /// </summary>
        [Display(Name = "代理人出生年月日")]
        public DateTime? E_BIRTHDAY { get; set; }

        /// <summary>
        /// 代理人身分證明文件字號
        /// </summary>
        [Display(Name = "代理人身分證明文件字號")]
        public string E_IDN { get; set; }

        /// <summary>
        /// 代理人地址郵遞區號
        /// </summary>
        [Display(Name = "代理人地址郵遞區號")]
        public string E_ADDR_CODE { get; set; }

        /// <summary>
        /// 代理人地址詳細
        /// </summary>
        [Display(Name = "代理人地址詳細")]
        public string E_ADDR { get; set; }

        /// <summary>
        /// 代理人電話
        /// </summary>
        [Display(Name = "代理人電話")]
        public string E_TEL { get; set; }

        /// <summary>
        /// 代理人E-MAIL
        /// </summary>
        [Display(Name = "代理人E-MAIL")]
        public string E_MAIL { get; set; }

        /// <summary>
        /// 代理上傳委任書電子檔
        /// </summary>
        [Display(Name = "代理上傳委任書電子檔")]
        public string FILE_01 { get; set; }

        /// <summary>
        /// 法人、團體、事務所或營業所名稱
        /// </summary>
        [Display(Name = "法人、團體、事務所或營業所名稱")]
        public string E_UNIT_NAME { get; set; }

        /// <summary>
        /// 法人、團體、事務所或營業所地址郵遞區號
        /// </summary>
        [Display(Name = "法人、團體、事務所或營業所地址郵遞區號")]
        public string E_UNIT_ADDR_CODE { get; set; }

        /// <summary>
        /// 法人、團體、事務所或營業所地址詳細
        /// </summary>
        [Display(Name = "法人、團體、事務所或營業所地址詳細")]
        public string E_UNIT_ADDR { get; set; }

        /// <summary>
        /// 代理<法人>上傳登記證影本
        /// </summary>
        [Display(Name = "代理<法人>上傳登記證影本")]
        public string FILE_02 { get; set; }

        /// <summary>
        /// 申請目的及用途
        /// </summary>
        [Display(Name = "申請目的及用途")]
        public string APP_REASON { get; set; }

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