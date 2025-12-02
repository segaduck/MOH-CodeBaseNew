using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblESERVICE : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? srv_id { get; set; }

        /// <summary>
        /// 申辦項目ID
        /// </summary>
        [Display(Name = "申辦項目ID")]
        public string srv_cd { get; set; }

        /// <summary>
        /// 申辦項目縣市
        /// </summary>
        [Display(Name = "申辦項目縣市")]
        public string srv_city { get; set; }

        /// <summary>
        /// 申辦項目名稱
        /// </summary>
        [Display(Name = "申辦項目名稱")]
        public string srv_name { get; set; }

        /// <summary>
        /// 是否需服務同意
        /// </summary>
        [Display(Name = "是否需服務同意")]
        public string srv_agree { get; set; }

        /// <summary>
        /// 服務同意需知文字
        /// </summary>
        [Display(Name = "服務同意需知文字")]        
        public string srv_agree_body { get; set; }

        /// <summary>
        /// 是否可線上申辦
        /// </summary>
        [Display(Name = "是否可線上申辦")]
        public string srv_online { get; set; }

        /// <summary>
        /// 是否可表單下載
        /// </summary>
        [Display(Name = "是否可表單下載")]
        public string srv_download { get; set; }

        /// <summary>
        /// 是否需繳費規費
        /// </summary>
        [Display(Name = "是否需繳費規費")]
        public string srv_pay { get; set; }

        /// <summary>
        /// 是否需領取
        /// </summary>
        [Display(Name = "是否需領取")]
        public string srv_get { get; set; }

        /// <summary>
        /// 是否需要點收
        /// </summary>
        [Display(Name = "是否需要點收")]
        public string srv_chk { get; set; }

        /// <summary>
        /// 是否需要分文
        /// </summary>
        [Display(Name = "是否需要分文")]
        public string srv_send { get; set; }

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

        /// <summary>
        /// 刪除註記
        /// </summary>
        [Display(Name = "刪除註記")]
        public string del_mk { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.ESERVICE;
        }
    }
}