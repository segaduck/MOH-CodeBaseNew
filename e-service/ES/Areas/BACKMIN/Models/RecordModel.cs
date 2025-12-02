using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class RecordModel
    {
        
    }

    public class LoginRecordQueryModel
    {
        [Display(Name = "帳號")]
        public virtual string Account { get; set; }

        [Display(Name = "姓名")]
        public virtual string Name { get; set; }

        [Display(Name = "單位名稱")]
        public virtual string UnitName { get; set; }

        [Display(Name = "異動者帳號")]
        public virtual string UpdateAccount { get; set; }

        [Display(Name = "起始日期")]
        public string StartDate { get; set; }

        [Display(Name = "結束日期")]
        public string EndDate { get; set; }

        [Display(Name = "異常次數")]
        public int ErrTimes { get; set; }

        [Display(Name = "頁數")]
        public int NowPage { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }
    }

    public class ModifyLogModel
    {
        public virtual int NowPage { get; set; }

        [Display(Name = "異動區間(起)")]
        public virtual String TIME_S { get; set; }

        [Display(Name = "異動區間(迄)")]
        public virtual String TIME_F { get; set; }

        [Display(Name = "異動類別")]
        public virtual String TYPE { get; set; }

        [Display(Name = "申辦編號")]
        public virtual String ID { get; set; }
    }

    public class MailLogModel
    {
        public virtual int NowPage { get; set; }

        [Display(Name = "異動區間(起)")]
        public virtual String TIME_S { get; set; }

        [Display(Name = "異動區間(迄)")]
        public virtual String TIME_F { get; set; }

        public virtual String SRV_ID { get; set; }

        public virtual String MAIL { get; set; }

        public virtual String RESULT_MK { get; set; }

        public virtual String ORDER_FIELD { get; set; }

        public virtual String ORDER_BY { get; set; }
    }
}