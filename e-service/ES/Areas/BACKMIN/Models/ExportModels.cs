using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class ExportCase1Models
    {
        [Display(Name = "申辦日期(起)")]
        public DateTime? DateS { get; set; }

        [Display(Name = "申辦日期(迄)")]
        public DateTime? DateE { get; set; }

        [Display(Name = "捐款專案")]
        public string ServiceId { get; set; }
    }
}