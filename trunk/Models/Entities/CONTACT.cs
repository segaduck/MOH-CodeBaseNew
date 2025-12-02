using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 共用代碼
    /// </summary>
    public class TblCONTACT : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? con_id { get; set; }

        /// <summary>
        /// 單位編號
        /// </summary>
        [Display(Name = "單位編號")]
        public string con_cd { get; set; }

        /// <summary>
        /// 單位名稱       
        /// </summary>
        [Display(Name = "單位名稱")]
        public string con_name { get; set; }

        /// <summary>
        /// 聯絡我們標題
        /// </summary>
        [Display(Name = "聯絡我們標題")]
        public string title { get; set; }

        /// <summary>
        /// 聯絡我們內容
        /// </summary>
        [Display(Name = "聯絡我們內容")]
        public string body { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        [Display(Name = "狀態")]
        public string status { get; set; }
        
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
            return StaticCodeMap.TableName.CONTACT;
        }
    }
}