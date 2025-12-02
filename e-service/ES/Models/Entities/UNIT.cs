using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TblUNIT
    {
        /// <summary>
        /// 
        /// </summary>
        public int? UNIT_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UNIT_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UNIT_ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? UNIT_PCD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? UNIT_LEVEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UNIT_SCD { get; set; }
        /// <summary>
        /// 
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