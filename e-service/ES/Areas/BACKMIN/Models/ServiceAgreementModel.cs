using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class ServiceAgreementModel
    {
        [Display(Name = "案件編號")]
        public virtual string ServiceId { get; set; }

        [Display(Name = "標題")]
        public virtual string Title { get; set; }

        [Display(Name = "內容")]
        public virtual string Content { get; set; }
    }

    public class ServiceAgreementEditModel : ServiceAgreementModel
    {
        [Display(Name = "案件名稱")]
        public string ServiceName { get; set; }

        [Display(Name = "分類ID")]
        public int CategoryId { set; get; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }
}