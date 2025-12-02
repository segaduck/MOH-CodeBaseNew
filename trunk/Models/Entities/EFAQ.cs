using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 常見問題
    /// </summary>
    public class TblEFAQ : IDBRow
    {
        /// <summary>
        /// IDK
        /// </summary>
        [IdentityDBField]
        public int? efaq_id { get; set; }

        /// <summary>
        /// 問題
        /// </summary>
        public string question { get; set; }

        /// <summary>
        /// 回答
        /// </summary>
        public string answer { get; set; }

        /// <summary>
        /// 常見問題類型
        /// </summary>
        public string faqtype { get; set; }

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
        /// 刪除註解
        /// </summary>
        public string del_mk { get; set; }

        /// <summary>
        /// 上架狀態 0否;1是
        /// </summary>
        public string status { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.EFAQ;
        }
    }
}