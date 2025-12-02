using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申辦項目提供檔案上傳種類
    /// </summary>
    public class TblESERVICE_FILE_ITEM : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? srv_file_item_id { get; set; }

        /// <summary>
        /// 案件PK
        /// </summary>
        [Display(Name = "案件PK")]
        public int? srv_id { get; set; }

        /// <summary>
        /// 檔案標題
        /// </summary>
        [Display(Name = "檔案標題")]
        public string filetitle { get; set; }

        /// <summary>
        /// 限制大小
        /// </summary>
        [Display(Name = "限制大小")]
        public int? allowlen { get; set; }

        /// <summary>
        /// 副檔名
        /// </summary>
        [Display(Name = "副檔名")]
        public string allowext { get; set; }

        /// <summary>
        /// 案件參數
        /// </summary>
        [Display(Name = "案件參數")]
        public string other_parm { get; set; }

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
            return StaticCodeMap.TableName.ESERVICE_FILE_ITEM;
        }
    }
}