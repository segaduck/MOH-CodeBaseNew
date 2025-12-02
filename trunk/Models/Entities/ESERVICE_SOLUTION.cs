using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 申辦項目說明
    /// </summary>
    public class TblESERVICE_SOLUTION : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? srv_soltion_id { get; set; }

        /// <summary>
        /// 順序
        /// </summary>
        [Display(Name = "順序")]
        public string seq { get; set; }

        /// <summary>
        /// 申請項目編碼
        /// </summary>
        [Display(Name = "申請項目編碼")]
        public string srv_id { get; set; }

        /// <summary>
        /// 項目類型
        /// 1: 網路申辦 
        /// 2: 臨櫃申辦 
        /// 3: 書表下載
        /// </summary>
        [Display(Name = "項目類型")]
        public string slt_type { get; set; }

        /// <summary>
        /// 主題
        /// </summary>
        [Display(Name = "主題")]
        public string subject { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        [Display(Name = "內容")]
        public string body { get; set; }

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
            return StaticCodeMap.TableName.ESERVICE_SOLUTION;
        }
    }
}