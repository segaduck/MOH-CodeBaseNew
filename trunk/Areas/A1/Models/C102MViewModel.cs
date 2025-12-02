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
    public class C102MViewModel
    {
        public C102MViewModel() { this.Form = new C102MFormModel(); }

        public C102MFormModel Form { get; set; }
    }

    public class C102MFormModel
    {
        [Display(Name = "名稱")]
        [Control(Mode = Control.DropDownList, group = 1)]
        public string hospital_code { get; set; }

        public IList<SelectListItem> hospital_code_list
        {
            get
            {
                SessionModel sm = SessionModel.Get();
                ShareCodeListModel list = new ShareCodeListModel();
                if (sm.UserInfo.LoginTab == "1") return list.Get_Hospital_list();
                else
                {
                    A1DAO dao = new A1DAO();
                    string tmpCode = dao.GetRow(new TblEEC_Hospital() { AuthCode = sm.UserInfo.UserNo }).code.TONotNullString();
                    return list.Get_Hospital_list().Where(x => x.Value == tmpCode).ToList();
                }
            }
        }

        public string hospital_name
        {
            get
            {
                if (this.hospital_code.TONotNullString() == "") return "";
                return this.hospital_code_list.Where(x => x.Value == this.hospital_code).FirstOrDefault().Text;
            }
        }

        public string AuthCode
        {
            get
            {
                SessionModel sm = SessionModel.Get();
                if (sm.UserInfo.LoginTab == "1")
                {
                    if (this.hospital_code.TONotNullString() == "") return "";
                    A1DAO dao = new A1DAO();
                    return dao.GetRow(new TblEEC_Hospital() { code = this.hospital_code }).AuthCode.TONotNullString();
                }
                else
                {
                    return sm.UserInfo.UserNo;
                }
            }
        }

        public string AuthDate { get; set; }

        [Display(Name = "受理線上申辦起始日")]
        [Control(Mode = Control.TextBox, group = 1, IsReadOnly = true)]
        public string AuthDate_Text
        {
            get
            {
                if (this.AuthDate.TONotNullString() == "") return "";
                return DateTime.ParseExact(this.AuthDate, "yyyyMMddHHmmss", null)
                    .AddYears(-1911)
                    .ToString("yyy 年 MM 月 dd 日");
            }
        }

        [Display(Name = "連絡信箱")]
        [Control(Mode = Control.TextBox, size = "50", IsReadOnly = true)]
        public string Email { get; set; }

        public string DoClearKeyid { get; set; }

        public IList<C102MGridModel> Grid { get; set; }
    }

    public class C102MGridModel : TblEEC_Hospital_SetPrice
    {
    }
}