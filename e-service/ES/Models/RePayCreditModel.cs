using System;
using System.Collections.Generic;
using ES.Models.Entities;
using System.Web;

namespace ES.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class RePayCreditModel
    {
        /// <summary>
        /// 
        /// </summary>
        public RePayCreditFormModel Form { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RePayCreditFormModel : ApplyModel
    {
        public string CLIENT_IP { get; set; }
        public string ERRMSG { get; set; }
        public string STATUS { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string SessionTransactionKey { get; set; }
    }

    public class SuccessECModel
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string OrderID { get; set; }
    }
}