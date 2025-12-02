using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPIRESULT_DONATE
    {
        public Int32? API_ID { get; set; }
        public string APP_ID { get; set; }
        public Int32? AMOUNT { get; set; }
        public string RETURNCODE { get; set; }
        public string RETURNMESSAGE { get; set; }
        public string TRANSACTIONID { get; set; }
        public string PAYMENTACCESSTOKEN { get; set; }
        public string PAYMENTURLAPP { get; set; }
        public string PAYMENTURLWEB { get; set; }
        public DateTime? ADD_TIME { get; set; }
        public string ADD_ACC { get; set; }
        public string ADD_FUN_CD { get; set; }

    }
}