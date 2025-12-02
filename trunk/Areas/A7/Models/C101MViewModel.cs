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
    public class C101MViewModel
    {
        public C101MViewModel()
        {
            this.form = new C101MFormModel();
        }

        /// <summary>
        /// 
        /// </summary>
        public C101MFormModel form { get; set; }

    }

    /// <summary>
    /// 查詢最新消息維護
    /// </summary>
    public class C101MFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 主旨
        /// </summary>
        [Display(Name = "主旨")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入查詢主旨")]
        public string subject { get; set; }


        /// <summary>
        /// 公告類型
        /// </summary>
        [Display(Name = "公告類型")]
        [Control(Mode = Control.DropDownList)]
        public string enews { get; set; }

        /// <summary>
        /// 公告類型_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> enews_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.enews_list;
            }
        }

        /// <summary>
        /// 置頂
        /// </summary>
        [Display(Name = "是否置頂")]
        [Control(Mode = Control.DropDownList)]
        public string totop { get; set; }

        //// <summary>
        /// 置頂_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> totop_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                SelectListItem item = new SelectListItem();
                item.Text = "請選擇";
                item.Value = "";
                var newlist = list.YorN_list;
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.Value).ToList();

                return newlist;
            }
        }

        /// <summary>
        /// 上架日期起
        /// </summary>
        public string showdates { get; set; }

        [Display(Name = "上架日期起")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        [NotDBField]
        public string showdates_AD
        {
            get
            {
                if (string.IsNullOrEmpty(showdates))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(showdates, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                showdates = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 上架日期迄
        /// </summary>
        public string showdatee { get; set; }

        [Display(Name = "上架日期迄")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        [NotDBField]
        public string showdatee_AD
        {
            get
            {
                if (string.IsNullOrEmpty(showdatee))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(showdatee, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                showdatee = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }


        /// <summary>
        /// 查詢結果 
        /// </summary>
        public IList<C101MGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 最新消息清單
    /// </summary>
    public class C101MGridModel : TblENEWS
    {
        /// <summary>
        /// 序號
        /// </summary>       
        public int? row_id { get; set; }

        /// <summary>
        /// 最新消息顯示日起迄
        /// </summary>
        public string showdate { get; set; }

        /// <summary>
        /// 公告類別
        /// </summary>
        public string code_name { get; set; }

        /// <summary>
        /// 公告編碼
        /// </summary>
        public string code_cd { get; set; }

        /// <summary>
        /// 上架
        /// </summary>
        public string showyn { get; set; }

        /// <summary>
        /// 狀態
        /// 0 停用
        /// 1 使用
        /// </summary>       
        public string status_nm { get; set; }

    }

    /// <summary>
    /// 新增 / 編輯 單位
    /// </summary>
    public class C101MDetailModel : TblENEWS
    {
        /// <summary>
        /// 
        /// </summary>
        public C101MDetailModel()
        {
            this.IsNew = true;
            // 檔案上傳/下載元件
            Upload = new DynamicEFileGrid();
  
        }

        /// <summary>
        /// 設定上傳參數
        /// </summary>
        public void SetUploadParm()
        {
            Upload.ShowFileUpload = true;
            Upload.ShowDelete = true;
            Upload.peky1 = "ENEWS";
            if (!IsNew) Upload.peky2 = enews_id.TONotNullString();
            Upload.GetFileGrid();
        }

        /// <summary>
        /// Detail必要控件(Hidden)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// 公告編碼
        /// </summary>
        public string code_cd { get; set; }

        /// <summary>
        /// PK
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string enews_id { get; set; }


        /// <summary>
        /// 公告類型
        /// </summary>
        [Display(Name = "公告類型")]
        [Control(Mode = Control.DropDownList)]
        [Required]
        public string enews { get; set; }

        //// <summary>
        /// 公告類型_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> enews_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.enews_list;
            }
        }

        /// <summary>
        /// 主旨
        /// </summary>
        [Display(Name = "主旨")]
        [Control(Mode = Control.TextBox, size = "48", maxlength = "50", placeholder = "請輸入主旨")]
        [Required]
        public string subject { get; set; }

        /// <summary>
        /// 公告內容
        /// </summary>
        [Display(Name = "公告內容")]
        [AllowHtml]
        [Control(Mode = Control.TextArea, columns = 80, rows = 10, placeholder = "請輸入公告內容")]
        [Required]
        public string body { get; set; }

        /// <summary>
        /// 上架日期起
        /// </summary>
        [Display(Name = "上架日期起")]
        [Required]
        public string showdates { get; set; }

        [Display(Name = "上架日期起")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        [NotDBField]
        public string showdates_AD
        {
            get
            {
                if (string.IsNullOrEmpty(showdates))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(showdates, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                showdates = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 上架日期迄
        /// </summary>
        [Required]
        [Display(Name = "上架日期迄")]
        public string showdatee { get; set; }

        [Display(Name = "上架日期迄")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        [NotDBField]
        public string showdatee_AD
        {
            get
            {
                if (string.IsNullOrEmpty(showdatee))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(showdatee, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                showdatee = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 是否上架
        /// </summary>
        [Display(Name = "是否上架")]
        [Control(Mode = Control.RadioGroup)]
        [Required]
        public string showyn { get; set; }

        //// <summary>
        /// 置頂_選項
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> showyn_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.YorN_list;
            }
        }

        /// <summary>
        /// 置頂狀態
        /// 0 否
        /// 1 是
        /// </summary>
        [Display(Name = "是否置頂")]
        [Control(Mode = Control.RadioGroup)]
        [Required]
        public string totop { get; set; }

        /// <summary>
        /// 置頂_選項
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> totop_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.YorN_list;
            }
        }

        /// <summary>
        /// 檔案上傳套件
        /// </summary>
        [NotDBField]
        [Control(Mode = Control.EFILE)]
        public DynamicEFileGrid Upload { get; set; }

       
    }

}