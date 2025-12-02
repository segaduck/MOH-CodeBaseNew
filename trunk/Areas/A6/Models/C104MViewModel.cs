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

namespace EECOnline.Areas.A6.Models
{
    public class C104MViewModel
    {
        public C104MViewModel() { this.Form = new C104MFormModel(); }

        public C104MFormModel Form { get; set; }
    }

    public class C104MFormModel
    {
        public IList<C104MGridModel> Grid { get; set; }
    }

    public class C104MGridModel : TblEEC_CommonType
    {
        [NotDBField]
        public string type_date1_Text
        {
            get
            {
                if (this.type_date1.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.type_date1.TONotNullString(), "yyyyMMdd", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd");
            }
        }

        [NotDBField]
        public string type_date2_Text
        {
            get
            {
                if (this.type_date2.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.type_date2.TONotNullString(), "yyyyMMdd", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd");
            }
        }
    }

    public class C104MDetailModel : TblEEC_CommonType
    {
        public C104MDetailModel() { this.IsNew = true; }

        [NotDBField]
        [Control(Mode = Control.Hidden)]
        public bool IsNew { get; set; }

        [Required]
        [Display(Name = "參數名稱")]
        [Control(Mode = Control.TextBox, size = "50", maxlength = "16", placeholder = "請輸入參數名稱", IsOpenNew = true)]
        public string type_name { get; set; }

        [Required]
        [Display(Name = "收費金額")]
        [Control(Mode = Control.TextBox, size = "50", maxlength = "16", placeholder = "請輸入收費金額")]
        public string type_price { get; set; }

        [Required]
        [NotDBField]
        [Display(Name = "啟用日起")]
        [Control(Mode = Control.DatePicker, group = 1)]
        public string type_date1_AD
        {
            get
            {
                if (string.IsNullOrEmpty(this.type_date1))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(this.type_date1, ""));  // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                this.type_date1 = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        [Required]
        [NotDBField]
        [Display(Name = "啟用日迄")]
        [Control(Mode = Control.DatePicker, group = 1)]
        public string type_date2_AD
        {
            get
            {
                if (string.IsNullOrEmpty(this.type_date2))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(this.type_date2, ""));  // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                this.type_date2 = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }
    }
}