using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 郵遞區號地址
    /// </summary>
    public class TblZIPCODE : IDBRow
    {
        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Display(Name = "郵遞區號")]
        public string ZIP_CO { get; set; }

        /// <summary>
        /// 縣市
        /// </summary>
        [Display(Name = "縣市")]
        public string CITYNM { get; set; }

        /// <summary>
        /// 區
        /// </summary>
        [Display(Name = "區")]
        public string TOWNNM { get; set; }

        /// <summary>
        /// 路段
        /// </summary>
        [Display(Name = "路段")]
        public string ROADNM { get; set; }

        /// <summary>
        /// 範圍
        /// </summary>
        [Display(Name = "範圍")]
        public string ROUND { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.ZIPCODE;
        }
    }
}