using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    public class FlyPayBasicModel
    {
        [Display(Name = "抵達日期")]
        public virtual string FLIGHTDATE { get; set; }

        [Display(Name = "護照號碼")]
        public virtual string MAINDOCNO { get; set; }

        [Display(Name = "識別碼")]
        public virtual string GUID { get; set; }
    }
}