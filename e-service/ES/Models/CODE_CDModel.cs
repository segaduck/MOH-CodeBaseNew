using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    /// <summary>
    /// 選單管理
    /// </summary>
    public class CODE_CDModel
    {
        /// <summary>
        /// 選單群組
        /// </summary>
        public string CODE_KIND { get; set; }
        /// <summary>
        /// 選項代碼
        /// </summary>
        public string CODE_CD { get; set; }
        /// <summary>
        /// 選項代碼2
        /// </summary>
        public string CODE_PCD { get; set; }
        /// <summary>
        /// 選項名稱
        /// </summary>
        public string CODE_DESC { get; set; }
        /// <summary>
        /// 選項備註
        /// </summary>
        public string CODE_MEMO { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int? SEQ_NO { get; set; }
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