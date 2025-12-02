using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblELINKS : IDBRow
    {
        /// <summary>
        /// 網站連結編碼
        /// </summary>
        [IdentityDBField]
        public int? elinks_id { get; set; }

        /// <summary>
        /// 連結說明
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 連結
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 連結類型
        /// </summary>
        public string linkstype { get; set; }

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
        /// 刪除註記
        /// </summary>
        public string del_mk { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.ELINKS;
        }
    }
}