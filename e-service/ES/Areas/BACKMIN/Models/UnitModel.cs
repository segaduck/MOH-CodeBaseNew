using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    [Serializable]
    public class UnitModel
    {
        [Display(Name = "單位代碼")]
        public virtual int UnitCode { get; set; }

        [Display(Name = "單位名稱")]
        public virtual string Name { get; set; }

        [Display(Name = "地址")]
        public virtual string Address { get; set; }

        [Display(Name = "父單位代碼")]
        public virtual int ParentCode { get; set; }

        [Display(Name = "層級")]
        public virtual int Level { get; set; }

        [Display(Name = "排序")]
        public virtual int Seq { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }

        [Display(Name = "是否有下層單位")]
        public bool HaveChild { get; set; }

        [Display(Name = "單位代碼")]
        public string UnitSCode { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { get; set; }
    }

    public class UnitEditModel : UnitModel
    {
        [Display(Name = "單位代碼")]
        public override int UnitCode { get; set; }

        [Required(ErrorMessage = "請輸入單位名稱")]
        [Display(Name = "單位名稱")]
        public override string Name { get; set; }

        [Display(Name = "地址")]
        public override string Address { get; set; }

        [Display(Name = "父單位代碼")]
        public override int ParentCode { get; set; }

        [Display(Name = "層級")]
        public override int Level { get; set; }

        [RegularExpression(@"[0-9]*", ErrorMessage = "排序只能輸入數字")]
        [Display(Name = "排序")]
        public override int Seq { get; set; }

        [Display(Name = "父單位名稱")]
        public string ParentUnitName { get; set; }
    }
}