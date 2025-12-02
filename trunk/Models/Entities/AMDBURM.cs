using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 使用者資料
    /// </summary>
    public class TblAMDBURM : IDBRow
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        public string userno { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Display(Name = "密碼")]
        public string pwd { get; set; }

        /// <summary>
        /// 使用者姓名
        /// </summary>
        [Display(Name = "使用者姓名")]
        public string username { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Display(Name = "身分證字號")]
        public string idno { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        public string birthday { get; set; }

        /// <summary>
        /// 單位代號
        /// </summary>
        [Display(Name = "單位代號")]
        public string unit_cd { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        [Display(Name = "電話")]
        public string tel { get; set; }

        /// <summary>
        /// 手機
        /// </summary>
        [Display(Name = "手機")]
        public string phone { get; set; }

        /// <summary>
        /// 傳真
        /// </summary>
        [Display(Name = "傳真")]
        public string fax { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        public string email { get; set; }

        /// <summary>
        /// 帳號狀態
        /// </summary>
        [Display(Name = "帳號狀態")]
        public string authstatus { get; set; }

        /// <summary>
        /// 帳號有效起日
        /// </summary>
        [Display(Name = "帳號有效起日")]
        public string authdates { get; set; }

        /// <summary>
        /// 帳號有效迄日
        /// </summary>
        [Display(Name = "帳號有效迄日")]
        public string authdatee { get; set; }

        /// <summary>
        /// 錯誤次數
        /// </summary>
        [Display(Name = "錯誤次數")]
        public int? errct { get; set; }

        /// <summary>
        /// 自然人憑證序號
        /// </summary>
        [Display(Name = "自然人憑證序號")]
        public string ssokey { get; set; }

        /// <summary>
        /// 醫師憑證序號
        /// </summary>
        [Display(Name = "醫師憑證序號")]
        public string dockey { get; set; }

        /// <summary>
        /// 是否系統登入
        /// </summary>
        [Display(Name = "是否系統登入")]
        public string login_yn { get; set; }

        /// <summary>
        /// 是否自然人憑證登入
        /// </summary>
        [Display(Name = "是否自然人憑證登入")]
        public string sso_yn { get; set; }

        /// <summary>
        /// 是否醫師憑證登入
        /// </summary>
        [Display(Name = "是否醫師憑證登入")]
        public string doc_yn { get; set; }

        /// <summary>
        /// 編輯者帳號
        /// </summary>
        [Display(Name = "編輯者帳號")]
        public string moduserid { get; set; }

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

        /// <summary>
        /// 刪除註記
        /// </summary>
        [Display(Name = "刪除註記")]
        public string del_mk { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.AMDBURM;
        }

    }
}