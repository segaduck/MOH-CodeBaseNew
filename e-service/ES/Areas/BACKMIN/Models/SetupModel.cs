using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class SetupModel
    {
        [Required(ErrorMessage = "請填寫參數名稱")]
        [Display(Name = "參數名稱")]
        public string SetupCode { get; set; }

        [Required(ErrorMessage = "請填寫參數說明")]
        [Display(Name = "參數說明")]
        public string SetupDesc { get; set; }

        [Display(Name = "參數值")]
        public string SetupValue { get; set; }

        [Display(Name = "異動者")]
        public string UpdateAccount { get; set; }
    }
}