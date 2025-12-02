using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAMGMAPM : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? gmapm_id { get; set; }

        /// <summary>
        /// 群組編碼
        /// </summary>
        [Display(Name = "群組編碼")]
        public int? grp_id { get; set; }

        /// <summary>
        /// 清單第一層
        /// </summary>
        [Display(Name = "清單第一層")]

        public string sysid { get; set; }

        /// <summary>
        /// 清單第二層
        /// </summary>
        [Display(Name = "清單第二層")]
        public string modules { get; set; }

        /// <summary>
        /// 清單第三層
        /// </summary>
        [Display(Name = "清單第三層")]
        public string submodules { get; set; }

        /// <summary>
        /// 編號
        /// </summary>
        [Display(Name = "編號")]
        public string prgid { get; set; }

        /// <summary>
        /// 新增權限
        /// </summary>
        [Display(Name = "新增權限")]
        public string prg_i { get; set; }

        /// <summary>
        /// 修改權限
        /// </summary>
        [Display(Name = "修改權限")]
        public string prg_u { get; set; }

        /// <summary>
        /// 刪除權限
        /// </summary>
        [Display(Name = "刪除權限")]
        public string prg_d { get; set; }

        /// <summary>
        /// 查詢權限
        /// </summary>
        [Display(Name = "查詢權限")]
        public string prg_q { get; set; }

        /// <summary>
        /// 列印權限
        /// </summary>
        [Display(Name = "列印權限")]
        public string prg_p { get; set; }

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
            return StaticCodeMap.TableName.AMGMAPM;
        }
    }
}