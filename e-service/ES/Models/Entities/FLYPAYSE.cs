using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TblFLYPAYSE
    {
        /// <summary>
        /// 
        /// </summary>
        public int? ID { get; set; }
        public string ECHELON { get; set; }
        public string SECTION { get; set; }
        public string SE_CODE { get; set; }
        public string DAYCODE { get; set; }
        public int? UCNT { get; set; }
    }
}