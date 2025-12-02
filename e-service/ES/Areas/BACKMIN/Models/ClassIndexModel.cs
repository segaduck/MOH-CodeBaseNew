using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{

    public class ClassIndexModel
    {
        [Display(Name = "查詢頁名稱")]
        public virtual string PageName { get; set; }

        [Display(Name = "查詢頁編號")]
        public virtual int PageType { get; set; }

        [Display(Name = "分類名稱")]
        public virtual string ClassName { get; set; }

        [Display(Name = "查詢呈現方式")]
        public virtual string ShowWay { get; set; }

        [Display(Name = "目標欄位名稱")]
        public virtual string Target { get; set; }
    }

    //public class ClassDetailModel
    //{
    //    [Display(Name = "細項代碼")]
    //    public virtual string DetailCode { get; set; }

    //    [Display(Name = "細項名稱")]
    //    public virtual string DetailName { get; set; }
    //}

    [Serializable]
    public class ClassEditModel
    {
        [Display(Name = "分類編號")]
        public virtual int SRL_NO { get; set; }

        [Display(Name = "分類名稱")]
        public virtual string TITLE { get; set; }

        [Display(Name = "主題分類代碼")]
        public virtual string CLS_SUB_CD { get; set; }

        [Display(Name = "施政分類代碼")]
        public virtual string CLS_ADM_CD { get; set; }

        [Display(Name = "服務分類代碼")]
        public virtual string CLS_SRV_CD { get; set; }

        [Display(Name = "關鍵字")]
        public virtual string KEYWORD { get; set; }

        [Display(Name = "異動者")]
        public virtual string UPD_ACC { get; set; }
    }

    [Serializable]
    //BuildTree Data To JSON
    public class ClassNodeModel
    {
        public virtual string id { get; set; }
        public virtual string pId { get; set; }
        public virtual string name { get; set; }
        public virtual int clevel { get; set; }
        public virtual string cname { get; set; }
        public virtual string ccd { get; set; }
        public virtual string cpcd { get; set; }
        public virtual Boolean open { get; set; }
    }

    [Serializable]
    public class ClassPageModel
    {
        public virtual String ActionModel { get; set; }

        public virtual string TargetTable { get; set; }

        public virtual string ParentClassid { get; set; }

        [Display(Name = "分類代碼")]
        [Remote("CheckIsExist", "ClassIndex", AdditionalFields ="ActionModel,TargetTable", ErrorMessage = "此代碼已使用!")]
        [Required(ErrorMessage = "分類代碼必填!")]
        public virtual string Classid { get; set; }

        [Display(Name = "分類名稱")]
        [Required(ErrorMessage = "分類名稱必填!")]
        public virtual string Classname { get; set; }

        public virtual string Clevel { get; set; }

        public virtual string BeforeClassid { get; set; }

        public virtual string scollBarPostion { get; set; }
        
    }
}