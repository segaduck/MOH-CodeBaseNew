using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 檔案上傳
    /// </summary>
    public class TblEFILE : IDBRow
    {
        /// <summary>
        /// 
        /// </summary>
        [IdentityDBField]
        public int? efile_id { get; set; }

        /// <summary>
        /// 原始檔名
        /// </summary>
        [Display(Name = "原始檔名")]
        public string srcfilename { get; set; }

        /// <summary>
        /// 修改後檔名
        /// </summary>
        [Display(Name = "修改後檔名")]
        public string filename { get; set; }

        /// <summary>
        /// 副檔名       
        /// </summary>
        [Display(Name = "副檔名")]
        public string extion { get; set; }

        /// <summary>
        /// 檔案容量
        /// </summary>
        [Display(Name = "檔案容量")]
        public int? filesize { get; set; }

        /// <summary>
        /// 檔案路徑
        /// </summary>
        [Display(Name = "檔案路徑")]
        public string filepath { get; set; }

        /// <summary>
        /// 檔案來源
        /// 資料表
        /// </summary>
        [Display(Name = "檔案來源")]
        public string peky1 { get; set; }

        /// <summary>
        /// 檔案來源ID
        /// 資料表PK
        /// </summary>
        [Display(Name = "檔案來源ID")]
        public string peky2 { get; set; }

        /// <summary>
        /// 檔案其他KEY1
        /// </summary>
        [Display(Name = "檔案其他KEY1")]
        public string peky3 { get; set; }

        /// <summary>
        /// 檔案其他KEY2
        /// </summary>
        [Display(Name = "檔案其他KEY2")]
        public string peky4 { get; set; }

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
            return StaticCodeMap.TableName.EFILE;
        }
    }
}