using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class ServiceDateModel
    {
        [Display(Name = "消息編號")]
        public int ServiceDateID { get; set; }

        [Display(Name = "標題")]
        public virtual string Title { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "開始時間")]
        public virtual DateTime? StartTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "結束時間")]
        public virtual DateTime? EndTime { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }

    public class ServiceDateActionModel : ServiceDateModel
    {
        [Display(Name = "頁數")]
        public int NowPage { get; set; }

        [Display(Name = "動作流水號")]
        public int ActionId { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [Display(Name = "開始時間")]
        public override DateTime? StartTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [Display(Name = "結束時間")]
        public override DateTime? EndTime { get; set; }
    }

    public class ServiceDateEditModel : ServiceDateModel
    {
        [AllowHtml]
        [Display(Name = "標題")]
        public override string Title { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "上線時間（起）")]
        public override DateTime? StartTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "上線時間（迄）")]
        public override DateTime? EndTime { get; set; }
    }

}