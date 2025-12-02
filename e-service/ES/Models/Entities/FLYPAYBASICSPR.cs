using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblFLYPAYBASICSPR
    {
        /// <summary>
        /// 
        /// </summary>
        public int? SPR_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? FLY_ID { get; set; }
        /// <summary>
        /// 選擇地區 北1 中2 南3
        /// </summary>
        public string SECTION { get; set; }
        /// <summary>
        /// 選擇價錢 2000A, 1500 B
        /// </summary>
        public string PLEVEL { get; set; }
        public DateTime? ADD_TIME { get; set; }
        public string ADD_FUN_CD { get; set; }
        public string ADD_ACC { get; set; }
        public DateTime? UPD_TIME { get; set; }
        public string UPD_FUN_CD { get; set; }
        public string UPD_ACC { get; set; }

        /// <summary>
        /// 是否計算
        /// </summary>
        public string ISUSE { get; set; }
    }
}