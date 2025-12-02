using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblAPPLY_012001_APPFIL
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        [Display(Name = "序號")]
        public int? SEQ_NO { get; set; }

        /// <summary>
        /// 檔號及文號
        /// </summary>
        [Display(Name = "檔號及文號")]
        public string FILENUM { get; set; }

        /// <summary>
        /// 檔案名稱或內容要旨
        /// </summary>
        [Display(Name = "檔案名稱或內容要旨")]
        public string FILENAME { get; set; }

        /// <summary>
        /// 件數
        /// </summary>
        [Display(Name = "件數")]
        public string NUMCNT { get; set; }

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