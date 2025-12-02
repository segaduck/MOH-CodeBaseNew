using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申辦項目案件申請狀態對應信件內容
    /// </summary>
    public class TblESERVICE_STATUS_MAIL : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? srv_status_mail_id { get; set; }

        /// <summary>
        /// 案件PK
        /// </summary>
        [Display(Name = "案件PK")]
        public int? srv_id { get; set; }

        /// <summary>
        /// 案件申請狀態代碼
        /// </summary>
        [Display(Name = "案件申請狀態代碼")]
        public string srv_status_id { get; set; }

        /// <summary>
        /// 信件標題
        /// </summary>
        [Display(Name = "信件標題")]
        public string subject { get; set; }

        /// <summary>
        /// 信件內容
        /// </summary>
        [Display(Name = "信件內容")]
        public string body { get; set; }

        /// <summary>
        /// 寄送身分
        /// 承辦人 0
        /// 申請人 1
        /// </summary>
        [Display(Name = "寄送身分")]
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
            return StaticCodeMap.TableName.ESERVICE_STATUS_MAIL;
        }
    }
}