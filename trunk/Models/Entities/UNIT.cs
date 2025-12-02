using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 單位資料
    /// </summary>
    public class TblUNIT : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? unit_id { get; set; }

        /// <summary>
        /// 單位代碼
        /// </summary>
        [Display(Name = "單位代碼")]
        public string unit_cd { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        [Display(Name = "單位名稱")]
        public string unit_nm { get; set; }

        /// <summary>
        /// 單位縣市
        /// </summary>
        [Display(Name = "單位縣市")]
        public string unit_city { get; set; }

        /// <summary>
        /// 單位縣市(轄區)
        /// </summary>
        [Display(Name = "單位縣市(轄區)")]
        public string unit_city_s { get; set; }

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

        /// <summary>
        /// 單位狀態
        /// 0 停用
        /// 1 啟用
        /// </summary>
        [Display(Name = "單位狀態")]
        public string status { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.UNIT;
        }
    }
}