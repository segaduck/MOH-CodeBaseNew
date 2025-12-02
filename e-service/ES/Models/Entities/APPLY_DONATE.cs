using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 信用卡線上捐款
    /// </summary>
    public class TblAPPLY_DONATE
    {
        /// <summary>
        /// 捐款專戶 SRV_ID
        /// </summary>
        public string SRV_ID_DONATE { get; set; }
        /// <summary>
        /// 專戶名稱 中文
        /// </summary>
        public string NAME_CH { get; set; }
        /// <summary>
        /// 專戶名稱 英文
        /// </summary>
        public string NAME_ENG { get; set; }
        /// <summary>
        /// 起始日
        /// </summary>
        public DateTime? START_DATE { get; set; }
        /// <summary>
        /// 結束日
        /// </summary>
        public DateTime? END_DATE { get; set; }
        /// <summary>
        /// 繳費方式
        /// </summary>
        public string PAY_WAY { get; set; }
        /// <summary>
        /// 銀行代碼
        /// </summary>
        public string BANK_CODE { get; set; }
        /// <summary>
        /// 銀行帳號
        /// </summary>
        public string BANK_ACCOUNT { get; set; }
        /// <summary>
        /// 戶名
        /// </summary>
        public string BANK_NAME { get; set; }
        /// <summary>
        /// 專戶說明 中文
        /// </summary>
        public string DESC_CH { get; set; }
        /// <summary>
        /// 專戶說明 英文
        /// </summary>
        public string DESC_ENG { get; set; }
        /// <summary>
        /// 是否開啟
        /// </summary>
        public string ISOPEN { get; set; }
        /// <summary>
        /// 是否為草稿儲存
        /// </summary>
        public string ISDRAFT { get; set; }
        public string DEL_MK { get; set; }
        public DateTime? DEL_TIME { get; set; }
        public string DEL_FUN_CD { get; set; }
        public string DEL_ACC { get; set; }
        public DateTime? UPD_TIME { get; set; }
        public string UPD_FUN_CD { get; set; }
        public string UPD_ACC { get; set; }
        public DateTime? ADD_TIME { get; set; }
        public string ADD_FUN_CD { get; set; }
        public string ADD_ACC { get; set; }
    }
}