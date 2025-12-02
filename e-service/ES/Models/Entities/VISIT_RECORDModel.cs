using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// 最近使用功能
    /// </summary>
    public class VISIT_RECORDModel
    {
        /// <summary>
        /// 
        /// </summary>
        public double? TAG { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CONTROL_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ACTION_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string APP_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ACC_NO { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? IS_EXPIRED { get; set; }

    }
}