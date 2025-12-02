using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Commons;
using EECOnline.DataLayers;
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
    public class C103MViewModel
    {
        public C103MViewModel()
        {
            this.form = new C103MFormModel();
        }

        /// <summary>
        /// 
        /// </summary>
        public C103MFormModel form { get; set; }

    }

    /// <summary>
    /// 查詢
    /// </summary>
    public class C103MFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 單位名稱
        /// </summary>
        [Display(Name = "單位名稱")]
        [Control(Mode = Control.TextBox, size = "26",placeholder = "請輸入單位名稱")]
        public string con_name { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        [Display(Name = "標題")]
        [Control(Mode = Control.TextBox, placeholder = "請輸入標題")]
        public string title { get; set; }

        ///// <summary>
        ///// 內容
        ///// </summary>
        //[Display(Name = "內容")]
        //[Control(Mode = Control.TextBox, size = "48", placeholder = "請輸入內容關鍵字")]
        //public string body { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        [Display(Name = "是否上架")]
        [Control(Mode = Control.DropDownList)]
        public string status { get; set; }

        //// <summary>
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
        public IList<C103MGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 清單
    /// </summary>
    public class C103MGridModel : TblCONTACT
    {
        /// <summary>
        /// 項次
        /// </summary>
        public int? row_id { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        public string con_name { get; set; }

        /// <summary>
        /// 聯絡我們標題
        /// </summary>
        public string  title { get; set; }

        /// <summary>
        /// 聯絡我們內容
        /// </summary>
        public string body{ get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        public string unit_nm { get; set; }
    }

    /// <summary>
    /// 新增 / 編輯 
    /// </summary>
    public class C103MDetailModel : TblCONTACT
    {
        public C103MDetailModel()
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
        public string con_id { get; set; }

        /// <summary>
        /// 單位編號
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string con_cd { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        [Display(Name = "單位名稱")]
        [Required]
        [Control(Mode = Control.DropDownList)]
        public string con_name { get; set; }

        /// <summary>
        /// 單位名稱_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> con_name_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.unit_list;
            }
        }

        /// <summary>
        /// 標題
        /// </summary>
        [Display(Name = "標題")]
        [Required]
        [Control(Mode = Control.TextBox, placeholder = "請輸入標題")]
        public string title { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        [AllowHtml]
        [Display(Name = "內容")]
        [Required]
        [Control(Mode = Control.TextArea, columns = 80, rows = 10, placeholder = "請輸入內容")]
        public string body { get; set; }

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

        /// <summary>
        /// 單位名稱
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string unit_nm { get; set; }

    }
}