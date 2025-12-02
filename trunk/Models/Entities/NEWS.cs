using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 最新消息
    /// </summary>
    public class TblNEWS : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? news_id { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        [Display(Name = "內容")]
        public string body { get; set; }

        /// <summary>
        /// 是否顯示
        /// 0:否 1:是
        /// </summary>
        [Display(Name = "是否顯示")]
        public string status { get; set; }

        /// <summary>
        /// 最新消息顯示日起
        /// </summary>
        [Display(Name = "最新消息顯示日起")]
        public string showdates { get; set; }

        /// <summary>
        /// 最新消息顯示日迄
        /// </summary>
        [Display(Name = "最新消息顯示日迄")]
        public string showdatee { get; set; }

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
            return StaticCodeMap.TableName.NEWS;
        }
    }
}