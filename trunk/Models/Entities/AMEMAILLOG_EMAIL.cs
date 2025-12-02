using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAMEMAILLOG_EMAIL : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? emaillog_id { get; set; }

        /// <summary>
        /// 功能
        ///  1 忘記密碼
        ///  2 前台案件申辦
        ///  3 後台案件變更
        ///  4 排程
        /// </summary>
        public string mail_type { get; set; }

        /// <summary>
        /// 申辦項目ID
        /// </summary>
        public string eservice_id { get; set; }

        /// <summary>
        /// 郵件主旨
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        /// 信件內容
        /// </summary>
        public string body { get; set; }

        /// <summary>
        /// 寄發時間
        /// </summary>
        public string send_time { get; set; }

        /// <summary>
        /// 寄發信箱
        /// </summary>
        public string mail { get; set; }

        /// <summary>
        /// 寄發狀態
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 編輯者帳號
        /// </summary>
        public string moduser { get; set; }

        /// <summary>
        /// 編輯者姓名
        /// </summary>
        public string modusername { get; set; }

        /// <summary>
        /// 編輯時間
        /// </summary>
        public string modtime { get; set; }

        /// <summary>
        /// 編輯IP
        /// </summary>
        public string modip { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.AMEMAILLOG_EMAIL;
        }
    }
}