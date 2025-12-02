using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_005001_PcViewModel:Apply_005001_PcModel
    {
        /// <summary>
        /// 成分內容
        /// </summary>
        [Display(Name = "賦形劑 成分內容(中文)")]
        //[Required]
        public string PC_NAME { get; set; }

        /// <summary>
        /// 生藥名
        /// </summary>
        [Display(Name = "賦形劑 生藥名")]
        //[Required]
        public string PC_ENAME { get; set; }
    }
}