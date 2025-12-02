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

namespace EECOnline.Areas.A1.Models
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
    /// 首頁
    /// </summary>
    public class C101MFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 服務單位
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string unit_cd { get; set; }

        /// <summary>
        /// 僅案件處理身份者
        /// 0不是 1是 2是，輸入非自身帳號
        /// </summary>
        //[Control(Mode = Control.Hidden)]
        //[NotDBField]
        //public string IsDealWith { get; set; }

        private string _cityText;

        /// <summary>
        /// 城市 
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string cityName
        {
            get; set;
        }

        /// <summary>
        /// 城市
        /// </summary>
        [Display(Name = "城市")]
        [Control(Mode = Control.DropDownList, size = "26", maxlength = "16")]
        public string city
        {
            //get
            //{
            //    return _cityText;
            //}
            //set
            //{
            //    var selectedItem = city_list.FirstOrDefault(item => item.Value == value);
            //    if (selectedItem != null)
            //    {
            //        _cityText = selectedItem.Text;
            //    }

            //}
            get; set;
        }

        /// <summary>
        /// 城市_清單
        /// </summary>
        public IList<SelectListItem> city_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                var cityList = list.srv_city_list;

                //if (!string.IsNullOrEmpty(_cityText))
                //{
                //    foreach (var item in cityList)
                //    {
                //        item.Selected = item.Text == _cityText;
                //    }
                //}

                return cityList;
            }
        }


        /// <summary>
        /// 醫院代碼
        /// </summary>
        [Display(Name = "醫院代碼")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入醫院代碼")]
        public string code { get; set; }

        /// <summary>
        /// 醫院名稱
        /// </summary>
        [Display(Name = "醫院名稱")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入醫院名稱")]
        public string text { get; set; }

        /// <summary>
        /// 醫院授權碼
        /// </summary>
        [Display(Name = "醫院授權碼")]
        [Control(Mode = Control.Hidden)]
        public string AuthCode { get; set; }

        /// <summary>
        /// 查詢結果 
        /// 若SQL有更動，請同步更新 GridAll
        /// </summary>
        public IList<C101MGridModel> Grid { get; set; }

    }

    /// <summary>
    /// 帳號清單
    /// </summary>
    public class C101MGridModel : TblEEC_Hospital  //TblAMDBURM
    {
        //public int keyid { get; set; }
        //public string code { get; set; }
        //public string text { get; set; }
        //public string cityName { get; set; }
        //public string city { get; set; }
        //public int orderby { get; set; }
        //public string AuthCode { get; set; }
    }

    /// <summary>
    /// 新增 / 編輯 群組
    /// </summary>
    public class C101MDetailModel : TblEEC_Hospital  //TblAMDBURM
    {
        public C101MDetailModel() { this.IsNew = true; }

        /// <summary>
        /// Detail必要控件(Hidden)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public bool IsNew { get; set; }

        private string _cityText;
        private string _city;
        /// <summary>
        /// 城市(Text)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string cityName
        {
            get
            {
                return _cityText;
            }
            set
            {
                var selectedItem = city_list.FirstOrDefault(item => item.Text == value);
                if (selectedItem != null)
                {
                    _cityText = selectedItem.Text;
                }

            }
        }

        /// <summary>
        /// 醫院代碼
        /// </summary>
        [Display(Name = " 醫院代碼 ")]
        [Required]
        [Control(Mode = Control.TextBox, size = "20", maxlength = "30", IsOpenNew = true)]
        public string code { get; set; }

        /// <summary>
        /// 醫院名稱
        /// </summary>
        [Display(Name = " 醫院名稱 ")]
        [Required]
        [Control(Mode = Control.TextBox, size = "50", maxlength = "16", placeholder = "請輸入醫院名稱")]
        public string text { get; set; }

        /// <summary>
        /// 城市(Text)
        /// </summary>
        [Display(Name = " 城市 ")]
        [Required]
        [Control(Mode = Control.DropDownList)]
        public string city
        {
            get
            {
                return _city;
            }
            set
            {
                _city = value;
                var selectedItem = city_list.FirstOrDefault(item => item.Value == value);
                if (selectedItem != null)
                {
                    _cityText = selectedItem.Text;
                }

            }
        }

        /// <summary>
        /// 城市_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> city_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                var cityList = list.srv_city_list;

                //foreach (var item in cityList)
                //{
                //    item.Selected = false;
                //}

                //if (!string.IsNullOrEmpty(_cityText))
                //{
                //    var selectedItem = cityList.FirstOrDefault(item => item.Text == _cityText);
                //    if (selectedItem != null)
                //    {
                //        selectedItem.Selected = true;
                //    }
                //}

                return cityList;
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        [Display(Name = "排序")]
        [Required]
        [Control(Mode = Control.TextBox, size = "10", maxlength = "16", placeholder = "請輸入排序")]
        public string orderby { get; set; }

        [Required]
        [Display(Name = "電子郵件")]
        [Control(Mode = Control.TextBox, size = "50", maxlength = "50")]
        [RegularExpression(@"^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,5})*$", ErrorMessage = "電子郵件格式有誤(example@gmail.com)")]
        public string Email { get; set; }

        /// <summary>
        /// 醫院授權碼
        /// </summary>
        [Display(Name = "醫院授權碼")]
        [Control(Mode = Control.TextBox, size = "10", maxlength = "6", IsReadOnly = true)]
        public string AuthCode { get; set; }
    }
}