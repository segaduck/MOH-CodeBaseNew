using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申辦項目分文對應群組
    /// </summary>
    public class TblESERVICE_SEND_GRP : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? srv_send_id { get; set; }

        /// <summary>
        /// 申請項目編碼        
        /// </summary>
        [Display(Name = "申請項目編碼")]
        public int? srv_id { get; set; }

        /// <summary>
        /// 群組編碼
        /// </summary>
        [Display(Name = "群組編碼")]
        public string grp_id { get; set; }

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
            return StaticCodeMap.TableName.ESERVICE_SEND_GRP;
        }
    }
}