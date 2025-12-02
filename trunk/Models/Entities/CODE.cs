using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 共用代碼
    /// </summary>
    public class TblCODE : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? code_id { get; set; }

        /// <summary>
        /// 代碼編號
        /// </summary>
        [Display(Name = "代碼編號")]
        public string code_cd { get; set; }

        /// <summary>
        /// 代碼名稱       
        /// </summary>
        [Display(Name = "代碼名稱")]
        public string code_name { get; set; }

        /// <summary>
        /// 代碼種類
        /// </summary>
        [Display(Name = "代碼種類")]
        public string code_type { get; set; }

        /// <summary>
        /// 代碼排序
        /// </summary>
        [Display(Name = "代碼排序")]
        public string code_seq { get; set; }

        /// <summary>
        /// 代碼註記
        /// </summary>
        [Display(Name = "代碼註記")]
        public string code_desc { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        [Display(Name = "狀態")]
        public string status { get; set; }
        
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
            return StaticCodeMap.TableName.CODE;
        }
    }
}