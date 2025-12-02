using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 訊息管理
    /// </summary>
    public class TblMESSAGEBack
    {
        /// <summary>
        /// 訊息代碼
        /// </summary>
        public int? MSG_ID { get; set; }
        /// <summary>
        /// 單位代碼
        /// </summary>
        public int? UNIT_CD { get; set; }
        /// <summary>
        /// 分類
        /// </summary>
        public string CATEGORY { get; set; }
        /// <summary>
        /// 標題
        /// </summary>
        public string TITLE { get; set; }
        /// <summary>
        /// 內容
        /// </summary>
        public string CONTENT { get; set; }
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime? TIME_S { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? TIME_E { get; set; }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FILENAME_1 { get; set; }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FILENAME_2 { get; set; }
        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FILENAME_3 { get; set; }
        /// <summary>
        /// 寄送通知 1: Y/N
        /// </summary>
        public string SEND_MAIL_MK { get; set; }
        /// <summary>
        /// 寄送時間
        /// </summary>
        public DateTime? SEND_MAIL_TIME { get; set; }
        /// <summary>
        /// 寄送時間
        /// </summary>
        public string CLS_SUB_CD { get; set; }
        /// <summary>
        /// 主題分類代碼
        /// </summary>
        public string CLS_ADM_CD { get; set; }
        /// <summary>
        /// 施政分類代碼
        /// </summary>
        public string CLS_SRV_CD { get; set; }
        /// <summary>
        /// 服務分類代碼
        /// </summary>
        public string KEYWORD { get; set; }
        /// <summary>
        /// 關鍵字
        /// </summary>
        public int? SEQ_NO { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public string DEL_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DEL_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_ACC { get; set; }
    }
}