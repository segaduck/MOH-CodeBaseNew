using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class QAModel
    {
        [Display(Name = "編號")]
        public int QAID { get; set; }

        [Display(Name = "標題")]
        public virtual string Title { get; set; }

        [Display(Name = "內容")]
        public string Content { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }

    public class QAActionModel : QAModel
    {
        [Display(Name = "頁數")]
        public int NowPage { get; set; }

        [Display(Name = "動作流水號")]
        public int ActionId { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }
    }

    public class QAEditModel : QAModel
    {
        [AllowHtml]
        [Required(ErrorMessage = "請輸入標題")]
        [Display(Name = "標題")]
        public override string Title { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "請輸入內容")]
        [Display(Name = "內容")]
        public string Content { get; set; }

        [Required(ErrorMessage = "請輸入排序")]
        [Display(Name = "排序")]
        public int SEQ { get; set; }
    }
}