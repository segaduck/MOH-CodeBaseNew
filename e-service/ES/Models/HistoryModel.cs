using System;
using System.Collections.Generic;
using ES.Models.Entities;

namespace ES.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class HistoryModel
    {
        /// <summary>
        /// 
        /// </summary>
        public HistoryFormModel Form { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HistoryFormModel : ApplyModel
    {
        /// <summary>
        /// 
        /// </summary>
        public IList<HistoryGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HistoryGridModel : ApplyModel
    {
        /// <summary>
        /// 申請案件
        /// </summary>
        public string SRV_ID_NAM { get; set; }

        /// <summary>
        /// 繳費狀態
        /// </summary>
        public string PAY_STATUS { get; set; }

        /// <summary>
        /// 進度
        /// </summary>
        public string FLOW_NAME { get; set; }

        /// <summary>
        /// 上傳繳費按鈕與對應的申辦案件代號
        /// </summary>
        public string Upload_PayFile { get; set; }

        /// <summary>
        /// 列印超商繳費單
        /// </summary>
        public string Download_PayFile { get; set; }

        /// <summary>
        /// 信用卡重新繳費
        /// </summary>
        public string RePayCard { get; set; }
    }
}