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
    public class C103MViewModel
    {
        public C103MViewModel() { this.Form = new C103MFormModel(); }
        public C103MFormModel Form { get; set; }
    }

    public class C103MFormModel
    {
        [Display(Name = "登入類別")]
        [Control(Mode = Control.DropDownList, group = 1)]
        public string login_type { get; set; }

        public IList<SelectListItem> login_type_list
        {
            get
            {
                var sclm = new ShareCodeListModel();
                return sclm.Get_login_type_list(true);
            }
        }

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

        public IList<C103MGridModel> Grid { get; set; }
    }

    public class C103MGridModel
    {
        public string LoginTypeText { get; set; }

        public int? CountNum { get; set; }
    }
}