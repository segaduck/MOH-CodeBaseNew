using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class SiteModel
    {
        [Display(Name = "網站編號")]
        public virtual int SiteId { get; set; }

        [Display(Name = "網站名稱")]
        public virtual string Name { get; set; }

        [Display(Name = "網址")]
        public virtual string Url { get; set; }

        [Display(Name = "排序")]
        public virtual int Seq { get; set; }
    }

    public class SiteEditModel : SiteModel
    {
        [Required(ErrorMessage="請輸入網站名稱")]
        public override string Name { get; set; }

        [Required(ErrorMessage = "請輸入網址")]
        [Display(Name = "網址")]
        public override string Url { get; set; }

        [Required(ErrorMessage = "請輸入排序")]
        [Display(Name = "排序")]
        public override int Seq { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }
}