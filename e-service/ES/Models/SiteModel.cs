using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    public class SiteModel
    {
        [Display(Name = "網站名稱")]
        public virtual string Name { get; set; }

        [Display(Name = "網址")]
        public virtual string Url { get; set; }

        [Display(Name = "排序")]
        public virtual int Seq { get; set; }
    }
}