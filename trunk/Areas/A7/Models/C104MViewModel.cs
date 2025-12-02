using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Commons;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using Turbo.Commons;
using Turbo.DataLayer;

namespace EECOnline.Areas.A7.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class C104MViewModel
    {
        public C104MViewModel()
        {
            this.form = new C104MFormModel();
        }

        /// <summary>
        /// 
        /// </summary>
        public C104MFormModel form { get; set; }

    }

    /// <summary>
    /// 查詢
    /// </summary>
    public class C104MFormModel : PagingResultsViewModel
    {

        /// <summary>
        /// 常見問題類型
        /// </summary>
        [Display(Name = "問題類型")]
        [Control(Mode = Control.DropDownList, col = "2")]
        public string code_name { get; set; }

        /// <summary>
        /// 常見問題_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> code_name_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.code_name_list;
            }
        }

        /// <summary>
        /// 主旨
        /// </summary>
        [Display(Name = "問題主旨")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入問題主旨")]
        public string question { get; set; }

        /// 回覆
        /// </summary>
        [Display(Name = "問題回覆")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入問題回覆")]
        public string answer { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        [Display(Name = "是否上架")]
        [Control(Mode = Control.DropDownList, col = "2")]
        public string status { get; set; }

        /// <summary>
        /// 是否上架_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> status_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.status_list;
            }
        }

        /// <summary>
        /// 查詢結果 
        /// </summary>
        public IList<C104MGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 常見問題清單
    /// </summary>
    public class C104MGridModel : TblEFAQ
    {
        /// <summary>
        /// 序號
        /// </summary>       
        public int? row_id { get; set; }

        /// <summary>
        /// 常見問題類別
        /// </summary>       
        public string code_name { get; set; }
    }
    /// <summary>
    /// 新增 / 編輯 常見問題
    /// </summary>
    public class C104MDetailModel : TblEFAQ
    {
        /// <summary>
        /// 
        /// </summary>
        public C104MDetailModel()
        {
            this.IsNew = true;
        }

        /// <summary>
        /// Detail必要控件(Hidden)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// PK
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string efaq_id { get; set; }

        /// <summary>
        /// 常見問題類型
        /// </summary>
        [Display(Name = "問題類型")]
        [Required]
        [Control(Mode = Control.DropDownList)]
        public string code_name { get; set; }

        /// <summary>
        /// 常見問題_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> code_name_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.code_name_list1;
            }
        }

        /// <summary>
        /// 主旨
        /// </summary>
        [Display(Name = "問題主旨")]
        [Required]
        [Control(Mode = Control.TextBox, size = "48", maxlength = "32", placeholder = "請輸入問題主旨")]
        public string question { get; set; }

        /// <summary>
        /// 問題回覆
        /// </summary>
        [AllowHtml]
        [Display(Name = "問題回覆")]
        [Required]
        [Control(Mode = Control.TextArea, columns = 80, rows = 10, placeholder = "請輸入問題回覆內容")]
        public string answer { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        [Display(Name = "是否上架")]
        [Required]
        [Control(Mode = Control.RadioGroup)]
        public string status { get; set; }

        /// <summary>
        /// 是否上架_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> status_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.YorN_list;
            }
        }

    }

    
}