using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申辦項目提供領取項目
    /// </summary>
    public class TblESERVICE_GET : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? srv_get_id { get; set; }

        /// <summary>
        /// 案件PK
        /// </summary>
        [Display(Name = "案件PK")]
        public int? srv_id { get; set; }

        /// <summary>
        /// 領取名目
        /// </summary>
        [Display(Name = "領取名目")]
        public string subject { get; set; }

        /// <summary>
        /// 領取詳細規則
        /// </summary>
        [Display(Name = "領取詳細規則")]
        public string body { get; set; }

        /// <summary>
        /// 輸入方式
        /// </summary>
        [Display(Name = "輸入方式")]
        public string gettype { get; set; }

        /// <summary>
        /// 顯示流程_填寫申辦
        /// </summary>
        [Display(Name = "領取_填寫申辦")]
        public string write { get; set; }

        /// <summary>
        /// 顯示流程_申辦查詢
        /// </summary>
        [Display(Name = "領取_申辦查詢")]
        public string preview { get; set; }

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
            return StaticCodeMap.TableName.ESERVICE_GET;
        }
    }
}