using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class ServiceFormModel
    {
        [Display(Name = "案件編號")]
        public virtual string ServiceId { get; set; }

        [AllowHtml]
        [Display(Name = "表單設定")]
        public virtual string ServiceField { get; set; }

        [AllowHtml]
        [Display(Name = "表單驗證")]
        public virtual string ServiceScript { get; set; }

        [AllowHtml]
        [Display(Name = "預覽驗證")]
        public virtual string PreviewScript { get; set; }
    }

    public class ServiceFormEditModel : ServiceFormModel
    {
        [Display(Name = "案件名稱")]
        public string ServiceName { get; set; }

        [Display(Name = "分類ID")]
        public int CategoryId { set; get; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }
}