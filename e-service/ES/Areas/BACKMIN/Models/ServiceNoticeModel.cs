using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class ServiceNoticeModel
    {
        [Display(Name = "案件編號")]
        public virtual string ServiceId { get; set; }

        [Display(Name = "申辦完成注意事項說明")]
        public virtual string CompleteDesc { get; set; }

        [Display(Name = "標題1")]
        public virtual string Title1 { get; set; }

        [AllowHtml]
        [Display(Name = "內容1")]
        public virtual string Content1 { get; set; }

        [Display(Name = "標題2")]
        public virtual string Title2 { get; set; }

        [AllowHtml]
        [Display(Name = "內容2")]
        public virtual string Content2 { get; set; }

        [Display(Name = "標題3")]
        public virtual string Title3 { get; set; }

        [AllowHtml]
        [Display(Name = "內容3")]
        public virtual string Content3 { get; set; }

        [Display(Name = "標題4")]
        public virtual string Title4 { get; set; }

        [AllowHtml]
        [Display(Name = "內容4")]
        public virtual string Content4 { get; set; }

        [Display(Name = "標題5")]
        public virtual string Title5 { get; set; }

        [AllowHtml]
        [Display(Name = "內容5")]
        public virtual string Content5 { get; set; }

        [Display(Name = "標題6")]
        public virtual string Title6 { get; set; }

        [AllowHtml]
        [Display(Name = "內容6")]
        public virtual string Content6 { get; set; }
    }

    public class ServiceNoticeEditModel : ServiceNoticeModel
    {
        [Display(Name = "案件名稱")]
        public string ServiceName { get; set; }

        [Display(Name = "分類ID")]
        public int CategoryId { set; get; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }
}