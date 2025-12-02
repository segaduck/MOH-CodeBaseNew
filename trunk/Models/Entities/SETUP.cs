using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblSETUP : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? setup_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string setup_cd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string setup_desc { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string setup_val { get; set; }

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

        /// <summary>
        /// 
        /// </summary>
        public string del_mk { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.SETUP;
        }
    }
}