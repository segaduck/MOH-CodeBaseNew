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
    public class Apply_005001_DiViewModel:Apply_005001_DiModel
    {
        /// <summary>
        /// 成分內容
        /// </summary>
        [Display(Name = "成分內容 成分內容(中文)")]
        [Required]
        public string DI_NAME { get; set; }

        /// <summary>
        /// 生藥名
        /// </summary>
        [Display(Name = "成分內容 生藥名")]
        [Required]
        public string DI_ENAME { get; set; }
    }
}