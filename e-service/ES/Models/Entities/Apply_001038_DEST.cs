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
    public class Apply_001038_DEST    {
        public string APP_ID { get; set; }
        public string ORG_UNITNAME { get; set; }
        public string ORG_NAME { get; set; }
        public string ORG_TEL { get; set; }
        public string ORG_EMAIL { get; set; }
        public string TAI_UNITNAME { get; set; }
        public string TAI_NAME { get; set; }
        public string TAI_TEL { get; set; }
        public string TAI_EMAIL { get; set; }
        public string TAI_ADDR { get; set; }        
        public string LIC_NUM { get; set; }
        public string A_NAME { get; set; }
        public string A_NUM1 { get; set; }
        public DateTime? A_DATE { get; set; }
        public string B_NAME { get; set; }

        public string B_NUM1 { get; set; }

        public string B_NUM2 { get; set; }
        public DateTime? B_DATE { get; set; }
        public string C_NAME1 { get; set; }
        public string C_NAME2 { get; set; }
        public string C_NUM1 { get; set; }
        public DateTime? C_DATE { get; set; }
        public string C_DAY { get; set; }

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