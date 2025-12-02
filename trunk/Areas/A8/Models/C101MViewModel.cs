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

namespace EECOnline.Areas.A8.Models
{
    /// <summary>
    /// 首頁
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
    /// 首頁內容
    /// </summary>
    public class C101MFormModel : PagingResultsViewModel
    {

        /// <summary>
        /// 操作日期起
        /// </summary>
        public string apy_time_st { get; set; }

        [Display(Name = "操作日期起")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        [NotDBField]
        public string apy_time_st_ad
        {
            get
            {
                if (string.IsNullOrEmpty(apy_time_st))
                {
                    return null;
                }
                else
                {
                    return DateTime.Parse(apy_time_st).ToString("yyyy/MM/dd");
                }
            }
            set
            {
                try
                {
                    apy_time_st = DateTime.Parse(value).ToString("yyyy/MM/dd");
                }
                catch(Exception ex)
                {
                    apy_time_st = null;
                }
                
            }
        }

        /// <summary>
        /// 操作日期迄
        /// </summary>
        public string apy_time_ed { get; set; }

        [Display(Name = "操作日期迄")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        [NotDBField]
        public string apy_time_ed_ad
        {
            get
            {
                if (string.IsNullOrEmpty(apy_time_ed))
                {
                    return null;
                }
                else
                {
                    return DateTime.Parse(apy_time_ed).AddDays(-1).ToString("yyyy/MM/dd");
                }
            }
            set
            {
                try
                {
                    // 由於時間會取到小時，所以這邊先加一已防找不到當天的資料
                    apy_time_ed = DateTime.Parse(value).AddDays(1).ToString("yyyy/MM/dd");
                }
                catch (Exception)
                {
                    apy_time_ed = null;
                }

            }
        }

        /// <summary>
        /// 使用者帳號
        /// </summary>
        [Display(Name = "使用者帳號")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入使用者帳號")]
        public string username { get; set; }

        /// <summary>
        /// 使用者姓名
        /// </summary>
        [Display(Name = "使用者姓名")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入使用者姓名")]
        public string usernameText { get; set; }

        /// <summary>
        /// 操作功能
        /// </summary>
        [Display(Name = "操作功能")]
        [Control(Mode = Control.DropDownList, size = "26", maxlength = "16")]
        public string use_type { get; set; }

        [NotDBField]
        public IList<SelectListItem> use_type_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.use_type_list;
            }
        }

        ///<summary>
        /// 功能
        /// </summary>
        [Display(Name = "功能")]
        [Control(Mode = Control.DropDownList, size = "26", maxlength = "16")]
        public string urlwhere { get; set; }

        [NotDBField]
        public IList<SelectListItem> urlwhere_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.urlwhere_list;
            }
        }


        /// <summary>
        /// 查詢結果 
        /// </summary>
        public IList<C101MGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 群組清單
    /// </summary>
    public class C101MGridModel : TblVISIT_RECORD
    {
        /// <summary>
        /// 使用者姓名
        /// </summary>
        public string usernotext { get; set; }

        /// <summary>
        /// 異動時間轉換
        /// </summary>
        public string modtimeTW
        {
            get
            {
                if (modtime == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwFormatLongString(modtime);
                }
            }
        }
    }


    public enum Use_type_Enum
    {
        查詢 = 0,
        新增 = 1,
        修改 = 2,
        刪除 = 3,
    };

}