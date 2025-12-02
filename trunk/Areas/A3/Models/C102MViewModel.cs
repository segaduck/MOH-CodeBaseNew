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
using Omu.ValueInjecter;

namespace EECOnline.Areas.A3.Models
{
    public class C102MViewModel
    {
        public C102MFormModel Form { get; set; }
    }

    public class C102MFormModel : PagingResultsViewModel
    {
        [Display(Name = "帳務月份")]
        [Control(Mode = Control.DropDownList, group = 1, col = "2")]
        public string AccountingYM { get; set; }

        public IList<SelectListItem> AccountingYM_list
        {
            get
            {
                var sclm = new ShareCodeListModel();
                return sclm.Get_AccountingYM_list(true);
            }
        }

        [Display(Name = "繳費情形")]
        [Control(Mode = Control.DropDownList, group = 1, col = "2")]
        public string PayStatus { get; set; }

        public IList<SelectListItem> PayStatus_list
        {
            get
            {
                var rtnList = new List<SelectListItem>();
                rtnList.Add(new SelectListItem() { Value = "", Text = "全部" });
                rtnList.Add(new SelectListItem() { Value = "Y", Text = "已繳費" });
                rtnList.Add(new SelectListItem() { Value = "N", Text = "未繳費" });
                return rtnList;
            }
        }

        [Display(Name = "醫院名稱")]
        [Control(Mode = Control.TextBox, maxlength = "20", size = "77", placeholder = "請輸入醫院名稱")]
        public string HospitalText { get; set; }

        public IList<C102MGridModel> Grid { get; set; }
    }

    public class C102MGridModel
    {
        public string AccountingYM { get; set; }

        public string AccountingYM_Text { get; set; }

        public string hospital_code { get; set; }

        public string hospital_name { get; set; }

        public int? price { get; set; }

        public string price_Text { get { return string.Format("{0:N0}", this.price); } }
    }
}