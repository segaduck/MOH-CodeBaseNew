using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class AccountModel
    {
        [Display(Name = "帳號")]
        public virtual string Account { get; set; }

        [Display(Name = "姓名")]
        public virtual string Name { get; set; }

        [Display(Name = "單位")]
        public virtual string UnitCode { get; set; }

        [Display(Name = "單位名稱")]
        public virtual string UnitName { get; set; }

        [Display(Name = "權限範圍單位")]
        public virtual int ScopeUnitCode { get; set; }

        [Display(Name = "服務範圍單位")]
        public virtual int ServiceUnitCode { get; set; }

        [Display(Name = "權限範圍")]
        public virtual int Scope { get; set; }

        [Display(Name = "等級權限")]
        public virtual List<int> LevelList { get; set; }

        [Display(Name = "異動者帳號")]
        public virtual string UpdateAccount { get; set; }

        [Display(Name = "停用狀態")]
        public virtual string Stops { get; set; }
    }

    public class AccountQueryModel : AccountModel
    {
        [Display(Name = "帳號")]
        public override string Account { get; set; }

        [Display(Name = "姓名")]
        public override string Name { get; set; }

        [Display(Name = "單位")]
        public override string UnitCode { get; set; }

        [Display(Name = "頁數")]
        public int NowPage { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }

        [Display(Name = "帳號")]
        public string ActionId { get; set; }

        [Required(ErrorMessage = "請選擇單位")]
        [Display(Name = "權限範圍")]
        public string Permission { get; set; }
    }

    public class AccountNewModel : AccountModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        [Display(Name = "帳號")]
        public override string Account { get; set; }

        [Display(Name = "姓名")]
        public override string Name { get; set; }

        [Required(ErrorMessage = "請選擇單位")]
        [Display(Name = "單位")]
        public override string UnitCode { get; set; }

        [Display(Name = "電話")]
        public string Tel { get; set; }

        [Display(Name = "電子郵件")]
        public string Mail { get; set; }
    }

    public class AccountNewJsonModel : AccountNewModel
    {
        public string result { get; set; }

        public string message { get; set; }
    }

    public class AccountEditModel : AccountModel
    {
        [Display(Name = "帳號")]
        public override string Account { get; set; }

        [Display(Name = "姓名")]
        public override string Name { get; set; }

        [Required(ErrorMessage = "請選擇單位")]
        [Display(Name = "單位")]
        public override string UnitCode { get; set; }

        [Display(Name = "電話")]
        public string Tel { get; set; }

        [Display(Name = "電子郵件")]
        public string Mail { get; set; }

        [Display(Name = "權限範圍")]
        public override int Scope { get; set; }

        [Display(Name = "等級權限")]
        public string Level { get; set; }

        [Display(Name = "帳號權限最後更新時間")]
        public string LevelUpdateTime { get; set; }

        [Display(Name = "最後修改時間(含屬性等)")]
        public string UpdateTime { get; set; }

        [Display(Name = "最後修改帳號")]
        public override string UpdateAccount { get; set; }
    }

    public class MemberBaseModel
    {
        public string Identity { get; set; }

        public string Account { get; set; }

        public string Mail { get; set; }
    }

    public class MemberQueryModel
    {
        [Display(Name = "動作類型")]
        public string ActionType { get; set; }

        [Display(Name = "輸入")]
        public string ActionId { get; set; }

        [Display(Name = "選項")]
        public string QueryType { get; set; }

        [Required(ErrorMessage = "輸入欄位必填")]
        [Display(Name = "輸入")]
        public string QueryId { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { get; set; }
    }
}