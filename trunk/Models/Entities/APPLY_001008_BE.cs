using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申請案件-醫事人員-歇業後執業-歇業
    /// </summary>
    public class TblAPPLY_001008_BE : IDBRow
    {
        /// <summary>
        /// 案件編碼
        /// </summary>
        [Display(Name = "案件編碼")]
        public string apy_not_deta_id { get; set; }

        /// <summary>
        /// 申請項目編碼
        /// </summary>
        [Display(Name = "申請項目編碼")]
        public string srv_id { get; set; }

        /// <summary>
        /// 申辦縣市
        /// </summary>
        [Display(Name = "申請歇業縣市")]
        public string apy_city { get; set; }

        /// <summary>
        /// 申請歇業縣市轄區
        /// </summary>
        [Display(Name = "申請歇業縣市轄區")]
        public string apy_city_s { get; set; }

        /// <summary>
        /// 案件申請人電話
        /// </summary>
        [Display(Name = "案件申請人電話")]
        public string apy_tel_be { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        public string apy_mail_be { get; set; }
        
        /// <summary>
        /// 離職日期
        /// </summary>
        [Display(Name = "離職日期")]
        public string apy_Resign_date { get; set; }

        /// <summary>
        /// 歇業日期
        /// </summary>
        [Display(Name = "歇業日期")]
        public string apy_close_date { get; set; }

        /// <summary>
        /// 歇業事由
        /// </summary>
        [Display(Name = "歇業事由")]
        public string apy_cause { get; set; }

        /// <summary>
        /// 服務機關是否停業狀態
        /// </summary>
        [Display(Name = "服務機關是否停業狀態")]
        public string apy_close { get; set; }

        /// <summary>
        /// 領取項目編碼
        /// </summary>
        [Display(Name = "領取項目編碼")]
        public string apy_get_id { get; set; }

        /// <summary>
        /// 領取郵遞區號
        /// </summary>
        [Display(Name = "領取郵遞區號")]
        public string apy_get_addr { get; set; }

        /// <summary>
        /// 領取地址
        /// </summary>
        [Display(Name = "領取地址")]
        public string apy_get_addr_ADDR { get; set; }

        /// <summary>
        /// 案件完成狀態
        /// </summary>
        [Display(Name = "案件完成狀態")]
        public string apy_status { get; set; }

        /// <summary>
        /// 分文承辦人員
        /// </summary>
        [Display(Name = "分文承辦人員")]
        public string apy_undertaker { get; set; }
        
        /// <summary>
        /// 案件狀態說明
        /// </summary>
        [Display(Name = "案件狀態說明")]
        public string returndesc_be { get; set; }

        /// <summary>
        /// 編輯者帳號
        /// </summary>
        [Display(Name = "編輯者帳號")]
        public string moduser { get; set; }

        /// <summary>
        /// 編輯者姓名
        /// </summary>
        [Display(Name = "編輯者姓名")]
        public string modusername { get; set; }

        /// <summary>
        /// 編輯時間
        /// </summary>
        [Display(Name = "編輯時間")]
        public string modtime { get; set; }

        /// <summary>
        /// 編輯IP
        /// </summary>
        [Display(Name = "編輯IP")]
        public string modip { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.APPLY_001008_BE;
        }
    }
}