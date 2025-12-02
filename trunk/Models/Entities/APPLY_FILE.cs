using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申辦案件檔案
    /// </summary>
    public class TblAPPLY_FILE : IDBRow
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string apy_id { get; set; }

        /// <summary>
        /// 檔案名稱
        /// </summary>
        [Display(Name = "檔案名稱")]
        public string apy_filename { get; set; }

        /// <summary>
        /// 原始檔案
        /// </summary>
        [Display(Name = "原始檔案")]
        public string apy_srcfilename { get; set; }

        /// <summary>
        /// 檔案大小
        /// </summary>
        [Display(Name = "檔案大小")]
        public string apy_src_filesize { get; set; }

        /// <summary>
        /// 副檔名
        /// </summary>
        [Display(Name = "副檔名")]
        public string apy_src_extion { get; set; }

        /// <summary>
        /// 申請項目編碼
        /// </summary>
        [Display(Name = "申請項目編碼")]
        public string apy_main_key { get; set; }

        /// <summary>
        /// 動態項目KEY值
        /// </summary>
        [Display(Name = "動態項目KEY值")]
        public string apy_src_key { get; set; }

        /// <summary>
        /// 其他變數
        /// </summary>
        [Display(Name = "其他變數")]
        public string apy_other_key { get; set; }

        /// <summary>
        /// 檔案路徑
        /// </summary>
        [Display(Name = "檔案路徑")]
        public string apy_file_path { get; set; }

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
            return StaticCodeMap.TableName.APPLY_FILE;
        }
    }
}