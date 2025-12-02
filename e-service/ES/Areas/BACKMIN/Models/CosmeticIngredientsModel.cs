using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class CosmeticIngredientsModel
    {
        [Display(Name = "流水號")]
        public virtual int CosmeticId { get; set; }

        [Display(Name = "成分")]
        public virtual string Content { get; set; }

        [Display(Name = "用途")]
        public virtual string Used { get; set; }

        [Display(Name = "濃度類別")]
        public virtual string Type { get; set; }

        [Display(Name = "濃度值1")]
        public virtual decimal Number1 { get; set; }

        [Display(Name = "濃度值2")]
        public virtual decimal Number2 { get; set; }

        [Display(Name = "備註")]
        public virtual string Note { get; set; }

        [Display(Name = "異動者帳號")]
        public virtual string UpdateAccount { set; get; }
    }

    public class CosmeticIngredientsActionModel : CosmeticIngredientsModel
    {
        [Display(Name = "一般化妝品成分")]
        public override string Content { get; set; }

        [Display(Name = "頁數")]
        public int NowPage { get; set; }

        [Display(Name = "動作流水號")]
        public int ActionId { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }
    }

    public class CosmeticIngredientsEditModel : CosmeticIngredientsModel
    {
        [Display(Name = "濃度值")]
        public string NumberS1 { get; set; }

        [Display(Name = "濃度值1")]
        public string NumberS2 { get; set; }

        [Display(Name = "濃度值2")]
        public string NumberS3 { get; set; }

        [Display(Name = "最後修改時間")]
        public string UpdateTime { get; set; }

        [Display(Name = "最後修改帳號")]
        public override string UpdateAccount { get; set; }

        public void InitSet()
        {
            if (this.Type.Equals("A"))
            {
                this.NumberS1 = this.Number1.ToString();
            }
            else
            {
                this.NumberS2 = this.Number1.ToString();
                this.NumberS3 = this.Number2.ToString();
            }
        }

        public void InitGet()
        {
            if (this.Type.Equals("A"))
            {
                this.Number1 = Decimal.Parse(this.NumberS1);
            }
            else
            {
                this.Number1 = Decimal.Parse(this.NumberS2);
                this.Number2 = Decimal.Parse(this.NumberS3);
            }
        }
    }
}