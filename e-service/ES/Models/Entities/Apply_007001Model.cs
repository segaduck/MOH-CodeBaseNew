using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_007001Model
    {
        /// <summary>
        /// 
        /// </summary>
        public string APP_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string REF_MEM_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DONOR_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DONOR_IDN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PUBLIC_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DONOR_MAIL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DONOR_TEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? AMOUNT { get; set; }
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

    }
}