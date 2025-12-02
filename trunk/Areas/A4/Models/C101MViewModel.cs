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

namespace EECOnline.Areas.A4.Models
{
    public class C101MViewModel
    {
        public C101MViewModel() { this.Form = new C101MFormModel(); }
        public C101MFormModel Form { get; set; }
    }

    public class C101MFormModel
    {
        [Display(Name = "繳費情形")]
        [Control(Mode = Control.DropDownList, group = 1)]
        public string payed { get; set; }

        public IList<SelectListItem> payed_list
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

        public IList<C101MGridModel> Grid { get; set; }
    }

    public class C101MGridModel
    {
        public string TypeName { get; set; }

        public int? SubNoCount { get; set; }
    }
}