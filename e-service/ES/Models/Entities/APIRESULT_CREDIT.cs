using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 線上捐款 信用卡/ATM回傳結果
    /// </summary>
    public class TblAPIRESULT_CREDIT
    {
        public string APP_ID { get; set; }
        public Int32? API_ID { get; set; }
        public string EncRes { get; set; }
        public string Status { get; set; }
        public string ErrCode { get; set; }
        public string AuthCode { get; set; }
        public string AuthAmt { get; set; }
        public string OrderNo { get; set; }
        public string OffsetAmt { get; set; }
        public string OriginalAmt { get; set; }
        public string UtilizedPoint { get; set; }
        public string txType { get; set; }
        public string Last4digitPAN { get; set; }
        public string ErrDesc { get; set; }
        public string AuthResURL { get; set; }
        public string XID { get; set; }
        public string AwardedPoint { get; set; }
        public string PointBalance { get; set; }
        public string ProdCode { get; set; }
        public string NumberOfPay { get; set; }
        public string LastError { get; set; }
        public string CardNumber { get; set; }
        public string PidResult { get; set; }
        public string CardNo { get; set; }
        public string EInovice { get; set; }
        public DateTime? ADD_TIME { get; set; }
        public string ADD_FUN_CD { get; set; }
        public string ADD_ACC { get; set; }
        public string WebATMAcct { get; set; }
        public string FeeCharge { get; set; }
        public string PayerBankId { get; set; }
    }
}