using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class ServiceApplyOpenModel
    {
        [Display(Name = "申辦項目代碼")]
        public virtual string SRV_ID { get; set; }

        [Display(Name = "申辦項目名稱")]
        public virtual string SRV_NAME { get; set; }

        [Required(ErrorMessage = "請輸入開放申請起日")]
        [Display(Name = "開放申請起日")]
        public virtual string APP_SDATE { get; set; }

        [Required(ErrorMessage = "請輸入開放申請迄日")]
        [Display(Name = "開放申請迄日")]
        public virtual string APP_EDATE { get; set; }

        [Display(Name = "異動者")]
        public string UpdateAccount { get; set; }
    }
}