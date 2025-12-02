using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class AssignModel
    {
        public virtual int NowPage { get; set; }

        public virtual string CaseType { get; set; }

        public virtual string CaseAccount { get; set; }

        public string SRV_ID { get; set; }

        public string FLOW_CD { get; set; } 

        public string IS_HOME_PAGE { get; set; }
    }

    public class AssignEditModel
    {
        [Display(Name = "申辦編號")]
        public virtual String APP_ID { get; set; }

        [Display(Name = "申辦項目")]
        public virtual String SRV_ID { get; set; }

        [Display(Name = "申辦日期")]
        public virtual String APP_TIME { get; set; }

        [Display(Name="承辦人員")]
        [Required(ErrorMessage="請選擇承辦人員!")]
        public virtual String PRO_ACC { get; set; }

        public virtual String SRV_NAME { get; set; }

        public virtual String TO_ARCHIVE_MK { get; set; }

        public string FLOW_CD { get; set; }

        public string IS_HOME_PAGE { get; set; }
    }
}