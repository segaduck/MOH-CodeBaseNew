using ES.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models.ViewModels
{
    public class Apply_001008_TransFViewModel : Apply_001008_TransFModel
    {
        /// <summary>
        /// 寄送地址
        /// </summary>
        [Display(Name = "寄送地址")]
        public string TRANSF_ADDR { get; set; }

        /// <summary>
        /// 寄送份數
        /// </summary>
        [Display(Name = "寄送份數")]
        public int? TRANSF_COPIES { get; set; }
    }
}