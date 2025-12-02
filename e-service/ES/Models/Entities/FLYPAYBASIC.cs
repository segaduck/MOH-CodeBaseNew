using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblFLYPAYBASIC
    {
        public int? FLY_ID { get; set; }
        public string GUID { get; set; }
        public string FIRSTNAME { get; set; }
        public string MIDDLENAME { get; set; }
        public string LASTNAME { get; set; }
        public string BIRTH { get; set; }
        public string GENDER { get; set; }
        public string MAINDOCNO { get; set; }
        public string NATIONALITY { get; set; }
        public string FLIGHTDATE { get; set; }
        public string FLIGHTNO { get; set; }
        public string DEPARTAIRPORT { get; set; }
        public string PASSENGERTYPE { get; set; }
        public DateTime? ADD_TIME { get; set; }
        public string ADD_FUN_CD { get; set; }
        public string ADD_ACC { get; set; }
        public DateTime? PAYDATE { get; set; }
        public string PAYMONEY { get; set; }
        public string PAYRESULT { get; set; }
        public string PLACE { get; set; }
        public string ISPAY { get; set; }
        public string STATUS { get; set; }
        public string PAYRETURN { get; set; }
        public string XID { get; set; }
        public string TRACENO { get; set; }
        public string QID { get; set; }
        public DateTime? BPAYDATE { get; set; }
        public DateTime? UPD_TIME { get; set; }
        public string UPD_FUN_CD { get; set; }
        public string UPD_ACC { get; set; }
        public string DEL_MK { get; set; }
        public DateTime? DEL_TIME { get; set; }
        public string DEL_FUN_CD { get; set; }
        public string DEL_ACC { get; set; }
        public string FLYTYPE { get; set; }
        public string FLYCONTRYTR { get; set; }
        public string BANKTYPE { get; set; }
        public string RC { get; set; }
        public string SPRCODE { get; set; }
        public string NEEDBACK { get; set; }
    }
}