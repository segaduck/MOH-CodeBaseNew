using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class APPLY_PAY
    {
        /// <summary>
        /// 
        /// </summary>
        public string APP_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PAY_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? PAY_MONEY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? PAY_PROFEE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? PAY_ACT_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "繳費時間")]
        public DateTime? PAY_EXT_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "最後異動時間")]
        public DateTime? PAY_INC_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PAY_METHOD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "繳費狀態")]
        public string PAY_STATUS_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PAY_RET_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PAY_RET_MSG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BATCH_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string APPROVAL_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PAY_RET_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string INVOICE_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PAY_DESC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CARD_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? HOST_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TRANS_RET { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SESSION_KEY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? AUTH_DATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AUTH_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? SETTLE_DATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OTHER { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ROC_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CLIENT_IP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SID { get; set; }
        /// <summary>
        /// 
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
        /// <summary>
        /// 繳費銀行代號
        /// </summary>
        public string PAY_BANK { get; set; }
        /// <summary>
        /// 繳費銀行訂單編號
        /// </summary>
        public string ECORDERID { get; set; }

    }
}