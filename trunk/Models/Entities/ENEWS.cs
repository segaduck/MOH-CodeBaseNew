using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 最新消息
    /// </summary>
    public class TblENEWS : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? enews_id { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        [Display(Name = "標題")]
        public string subject { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        [Display(Name = "內容")]
        public string body { get; set; }

        /// <summary>
        /// 最新消息類型
        /// </summary>
        [Display(Name = "最新消息類型")]
        public string newstype { get; set; }

        /// <summary>
        /// 最新消息類型名稱
        /// </summary>
        [Display(Name = "最新消息類型")]
        [NotDBField]
        public string newstype_nm { get; set; }
        
        /// <summary>
        /// 是否置頂
        /// 0:否 1:是
        /// </summary>
        [Display(Name = "是否置頂")]
        public string totop { get; set; }

        /// <summary>
        /// 最新消息顯示日起
        /// </summary>
        [Display(Name = "最新消息顯示日起")]
        public string showdates { get; set; }

        /// <summary>
        /// 消息公布顯示日迄
        /// </summary>
        [Display(Name = "消息公布顯示日迄")]
        public string showdatee { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        [Display(Name = "是否上架")]
        public string showyn { get; set; }

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
            return StaticCodeMap.TableName.ENEWS;
        }
    }
}