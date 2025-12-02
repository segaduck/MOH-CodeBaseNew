using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 信件LOG
    /// </summary>
    public class TblMAILLOG : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? srl_no { get; set; }

        /// <summary>
        /// 案件代碼
        /// </summary>
        [Display(Name = "案件代碼")]
        public string srv_id { get; set; }

        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string apy_id { get; set; }

        /// <summary>
        /// 主旨
        /// </summary>
        [Display(Name = "主旨")]
        public string subject { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        [Display(Name = "內容")]
        public string body { get; set; }

        /// <summary>
        /// 信箱
        /// </summary>
        [Display(Name = "信箱")]
        public string mail { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        [Display(Name = "狀態")]
        public string status { get; set; }

        /// <summary>
        /// 發送時間
        /// </summary>
        [Display(Name = "發送時間")]
        public DateTime? send_time { get; set; }
        
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.MAILLOG;
        }
    }
}