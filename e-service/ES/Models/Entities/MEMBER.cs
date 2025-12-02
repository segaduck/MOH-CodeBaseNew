using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblMEMBER
    {
        /// <summary>
        /// 
        /// </summary>
        public string ACC_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PSWD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IDN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SEX_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? BIRTHDAY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ENAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CNT_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CNT_ENAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CHR_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CHR_ENAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FAX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CNT_TEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MAIL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CITY_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TOWN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MEDICO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MAIL_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CARD_TYPE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CARD_INFO { get; set; }
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
        /// 
        /// </summary>
        public string CARD_IDX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MOBILE { get; set; }

        public string SERIALNO { get; set; }

        public DateTime? LAST_LOGINDATE { get; set; }

        /// <summary>
        /// 原住民羅馬拼音
        /// </summary>
        public string ONAME { get; set; }

    }
}