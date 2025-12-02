using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 忘記密碼修改歷程
    /// </summary>
    public class TblAMCHANGEPWD_LOG : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? amlog_id { get; set; }

        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string userno { get; set; }

        /// <summary>
        /// 唯一識別碼
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 使用者密碼
        /// </summary>
        public string pwd { get; set; }

        /// <summary>
        /// 編輯者帳號
        /// </summary>
        public string moduserid { get; set; }

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
            return StaticCodeMap.TableName.AMCHANGEPWD_LOG;
        }
    }
}