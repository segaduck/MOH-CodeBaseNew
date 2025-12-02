using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class CosmeticAdvertisingModel
    {
        [Display(Name = "流水號")]
        public virtual int AdvId { get; set; }

        [Display(Name = "廣告字句")]
        public virtual string Word { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }

    public class CosmeticAdvertisingActionModel : CosmeticAdvertisingModel
    {
        [Display(Name = "不得刊登之廣告字句")]
        public override string Word { get; set; }

        [Display(Name = "頁數")]
        public int NowPage { get; set; }

        [Display(Name = "動作流水號")]
        public int ActionId { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }
    }

    public class CosmeticAdvertisingEditModel : CosmeticAdvertisingModel
    {
        [Required(ErrorMessage = "請輸入廣告字句")]
        [Display(Name = "廣告字句")]
        public override string Word { get; set; }
    }
}