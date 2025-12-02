using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models
{
    /// <summary>
    /// 系統會員
    /// </summary>
    public class AdminModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string ACC_NO { get; set; }
        /// <summary>
        /// 單位代碼
        /// </summary>
        public int? UNIT_CD { get; set; }
        /// <summary>
        /// 帳號群組
        /// </summary>
        public int? ADMIN_SCOPE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADMIN_LEVEL { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string NAME { get; set; }
        /// <summary>
        /// 電話
        /// </summary>
        public string TEL { get; set; }
        /// <summary>
        /// 電子郵件
        /// </summary>
        public string MAIL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AD_OU { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SSO_KEY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IDN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LEVEL_UPD_TIME { get; set; }
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