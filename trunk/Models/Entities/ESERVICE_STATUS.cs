using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申辦項目案件申請狀態
    /// </summary>
    public class TblESERVICE_STATUS : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? srv_status_id { get; set; }

        /// <summary>
        /// 案件PK
        /// </summary>
        [Display(Name = "案件PK")]
        public int? srv_id { get; set; }

        /// <summary>
        /// 案件申請狀態代碼
        /// </summary>
        [Display(Name = "案件申請狀態代碼")]
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

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.ESERVICE_STATUS;
        }
    }
}