using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class ServiceHelpModel
    {
        [Display(Name = "案件編號")]
        public virtual string ServiceId { get; set; }

        [Display(Name = "檔案編號")]
        public virtual int FileId { get; set; }

        [Display(Name = "檔案標題")]
        public virtual string Title { get; set; }

        [Display(Name = "檔案名稱")]
        public virtual string FileName { get; set; }

        [Display(Name = "排序")]
        public virtual int Seq { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }

    public class ServiceHelpNewModel : ServiceFileModel
    {
        [Display(Name = "案件名稱")]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "請填寫檔案標題")]
        [Display(Name = "檔案標題")]
        public override string Title { get; set; }

        [Display(Name = "檔案名稱")]
        public override string FileName { get; set; }

        [Required(ErrorMessage = "請填寫排序")]
        [Display(Name = "排序")]
        public override int Seq { get; set; }
    }

    public class ServiceHelpEditModel : ServiceFileModel
    {
        [Display(Name = "案件名稱")]
        public string ServiceName { get; set; }

        [Required(ErrorMessage = "請填寫檔案標題")]
        [Display(Name = "檔案標題")]
        public override string Title { get; set; }

        [Display(Name = "原有檔案")]
        public override string FileName { get; set; }

        [Required(ErrorMessage = "請填寫排序")]
        [Display(Name = "排序")]
        public override int Seq { get; set; }
    }
}