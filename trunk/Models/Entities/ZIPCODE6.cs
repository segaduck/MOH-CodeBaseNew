using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 6碼郵遞區號
    /// </summary>
    public class TblZIPCODE6 : IDBRow
    {
        /// <summary>
        /// 自動遞增主鍵
        /// </summary>
        [IdentityDBField]
        public int? ID { get; set; }

        /// <summary>
        /// 6碼郵遞區號
        /// </summary>
        [Display(Name = "6碼郵遞區號")]
        [StringLength(6)]
        public string ZIP_CO { get; set; }

        /// <summary>
        /// 3碼郵遞區號 (對應舊制)
        /// </summary>
        [Display(Name = "3碼郵遞區號")]
        [StringLength(3)]
        public string ZIP3 { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        [Display(Name = "縣市")]
        [StringLength(20)]
        public string CITYNM { get; set; }

        /// <summary>
        /// 鄉鎮市區名稱
        /// </summary>
        [Display(Name = "鄉鎮市區")]
        [StringLength(20)]
        public string TOWNNM { get; set; }

        /// <summary>
        /// 路段名稱
        /// </summary>
        [Display(Name = "路段")]
        [StringLength(100)]
        public string ROADNM { get; set; }

        /// <summary>
        /// 範圍說明
        /// </summary>
        [Display(Name = "範圍")]
        [StringLength(100)]
        public string SCOOP { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime? CREATE_DT { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime? UPDATE_DT { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.ZIPCODE6;
        }
    }
}
