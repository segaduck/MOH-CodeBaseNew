using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class ServiceRuleModel
    {
        [Display(Name = "案件編號")]
        public string ServiceId { get; set; }

        [Display(Name = "申請後通知")]
        public bool RuleAMark { get; set; }

        [Display(Name = "申請後通知期限")]
        public int RuleADay { get; set; }

        [Display(Name = "分文後通知")]
        public bool RuleBMark { get; set; }

        [Display(Name = "分文後通知期限")]
        public int RuleBDay { get; set; }

        [Display(Name = "處理後逾期通知")]
        public bool RuleCMark { get; set; }

        [Display(Name = "通知次數")]
        public int NotifyCount { get; set; }

        [Display(Name = "是否還原繼續通知")]
        public bool ResetMark { get; set; }

        [Display(Name = "是否通知主管")]
        public bool HeadMark { get; set; }

        [Display(Name = "主管帳號")]
        public string HeadAccount { get; set; }
    }

    public class ServiceRuleEditModel : ServiceRuleModel
    {
        [Display(Name = "分類ID")]
        public int CategoryId { set; get; }

        [Display(Name = "案件名稱")]
        public string ServiceName { get; set; }

        [Display(Name = "承辦單位")]
        public int UnitCode { get; set; }

        [Display(Name = "申請後通知期限")]
        public string RuleADayS { get; set; }

        [Display(Name = "分文後通知期限")]
        public string RuleBDayS { get; set; }

        [Display(Name = "通知次數")]
        public string NotifyCountS { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { get; set; }

        public void InitSet()
        {
            if (this.RuleAMark)
            {
                this.RuleADayS = this.RuleADay.ToString();
            }
            if (this.RuleBMark)
            {
                this.RuleBDayS = this.RuleBDay.ToString();
            }
            this.NotifyCountS = this.NotifyCount.ToString();
        }

        public void InitGet()
        {
            if (this.RuleAMark && !String.IsNullOrEmpty(this.RuleADayS))
            {
                this.RuleADay = Int32.Parse(this.RuleADayS);
            }
            if (this.RuleBMark && !String.IsNullOrEmpty(this.RuleBDayS))
            {
                this.RuleBDay = Int32.Parse(this.RuleBDayS);
            }
            if (!String.IsNullOrEmpty(this.NotifyCountS))
            {
                this.NotifyCount = Int32.Parse(this.NotifyCountS);
            }
        }
    }
}