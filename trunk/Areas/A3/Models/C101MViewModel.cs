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

namespace EECOnline.Areas.A3.Models
{
    public class C101MViewModel
    {
        public C101MFormModel Form { get; set; }
    }

    public class C101MFormModel : PagingResultsViewModel
    {
        [Display(Name = "訂單編號")]
        [Control(Mode = Control.TextBox, maxlength = "20", size = "92", placeholder = "請輸入訂單編號")]
        public string apply_no_sub { get; set; }

        [Display(Name = "醫院名稱")]
        [Control(Mode = Control.TextBox, maxlength = "20", size = "92", placeholder = "請輸入醫院名稱")]
        public string HospitalText { get; set; }

        public string ApplyDateS { get; set; }

        [Display(Name = "申請日起")]
        [Control(Mode = Control.DatePicker, group = 2, col = "2")]
        public string ApplyDateS_AD
        {
            get
            {
                if (string.IsNullOrEmpty(ApplyDateS))
                    return null;
                else
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(ApplyDateS, ""));  // YYYYMMDD 回傳給系統
            }
            set
            {
                ApplyDateS = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        public string ApplyDateE { get; set; }

        [Display(Name = "申請日迄")]
        [Control(Mode = Control.DatePicker, group = 2, col = "2")]
        public string ApplyDateE_AD
        {
            get
            {
                if (string.IsNullOrEmpty(ApplyDateE))
                    return null;
                else
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(ApplyDateE, ""));  // YYYYMMDD 回傳給系統
            }
            set
            {
                ApplyDateE = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        [Display(Name = "繳費情形")]
        [Control(Mode = Control.DropDownList)]
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

        public IList<C101MGridModel> Grid { get; set; }
    }

    public class C101MGridModel : TblEEC_ApplyDetailPrice
    {
        [NotDBField]
        public string user_name { get; set; }

        [NotDBField]
        public string his_range1 { get; set; }

        [NotDBField]
        public string his_range1_Text
        {
            get
            {
                if (this.his_range1.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.his_range1.TONotNullString(), "yyyyMMdd", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd");
            }
        }

        [NotDBField]
        public string his_range2 { get; set; }

        [NotDBField]
        public string his_range2_Text
        {
            get
            {
                if (this.his_range2.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.his_range2.TONotNullString(), "yyyyMMdd", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd");
            }
        }

        [NotDBField]
        public string createdatetime { get; set; }

        [NotDBField]
        public string createdatetime_Text
        {
            get
            {
                if (this.createdatetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.createdatetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd");
            }
        }

        [NotDBField]
        public string payed_Text { get { return (this.payed == "Y") ? "已繳費" : "尚未繳費"; } }

        [NotDBField]
        public string price_Text { get { return string.Format("{0:N0}", this.price); } }

        [NotDBField]
        public int? price2 { get; set; }

        [NotDBField]
        public string price2_Text { get { return string.Format("{0:N0}", this.price2); } }
    }

    public class C101MExportDatModel
    {
        [Display(Name = "訂單編號")]
        [Control(Mode = Control.TextBox, maxlength = "20", size = "92", placeholder = "請輸入訂單編號")]
        public string apply_no_sub { get; set; }

        [Display(Name = "醫院名稱")]
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

        public string ApplyDateS { get; set; }

        [Display(Name = "申請日起")]
        [Control(Mode = Control.DatePicker, group = 2, col = "2")]
        public string ApplyDateS_AD
        {
            get
            {
                if (string.IsNullOrEmpty(ApplyDateS))
                    return null;
                else
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(ApplyDateS, ""));  // YYYYMMDD 回傳給系統
            }
            set
            {
                ApplyDateS = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        public string ApplyDateE { get; set; }

        [Display(Name = "申請日迄")]
        [Control(Mode = Control.DatePicker, group = 2, col = "2")]
        public string ApplyDateE_AD
        {
            get
            {
                if (string.IsNullOrEmpty(ApplyDateE))
                    return null;
                else
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(ApplyDateE, ""));  // YYYYMMDD 回傳給系統
            }
            set
            {
                ApplyDateE = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        [Display(Name = "繳費情形")]
        [Control(Mode = Control.DropDownList)]
        public string PayStatus { get; set; }

        public IList<SelectListItem> PayStatus_list
        {
            get
            {
                var rtnList = new List<SelectListItem>();
                rtnList.Add(new SelectListItem() { Value = "", Text = "<請選擇>" });
                rtnList.Add(new SelectListItem() { Value = "Y", Text = "已繳費" });
                rtnList.Add(new SelectListItem() { Value = "N", Text = "未繳費" });
                rtnList.Add(new SelectListItem() { Value = "D", Text = "請款成功" });  // 特殊狀態，需組合SQL
                return rtnList;
            }
        }

        public string PayStatus_forSQL { get; set; }

        public IList<ExportDatGridModel> Grid { get; set; }
    }

    public class ExportDatGridModel
    {
        public string apply_no { get; set; }
        public string apply_no_sub { get; set; }
        public string hospital_code { get; set; }
        public string price { get; set; }
        public string payed_orderid { get; set; }
        public string payed_sessionkey { get; set; }
        public string payed_transdate { get; set; }
        public string payed_approvecode { get; set; }
    }

    public class C101MImportDatModel
    {
        public HttpPostedFileBase UploadFILE { get; set; }
        public IList<C101MImportDatGridModel> Grid { get; set; }
    }

    public class C101MImportDatGridModel
    {
        public string A_001_010_10 { get; set; }
        public string B_011_018_8 { get; set; }
        public string C_019_058_40 { get; set; }
        public string D_059_077_19 { get; set; }
        public string E_078_085_8 { get; set; }
        public string F_086_093_8 { get; set; }
        public string G_094_095_2 { get; set; }
        public string H_096_103_8 { get; set; }
        public string I_104_119_16 { get; set; }
        public string J_120_159_40 { get; set; }
        public string K_160_165_6 { get; set; }
        public string L_166_168_3 { get; set; }
        public string M_169_184_16 { get; set; }
        public string N_185_190_6 { get; set; }
        public string O_191_191_1 { get; set; }
        public string P_192_193_2 { get; set; }
        public string Q_194_201_8 { get; set; }
        public string R_202_209_8 { get; set; }
        public string S_210_215_6 { get; set; }
        public string T_216_223_8 { get; set; }
        public string U_224_224_1 { get; set; }
        public string V_225_232_8 { get; set; }
        public string W_233_242_10 { get; set; }
        public string X_243_250_8 { get; set; }
        public string Y_251_251_1 { get; set; }
        public string Z_252_252_1 { get; set; }
        public string ZA_253_253_1 { get; set; }
        public string ZB_254_270_17 { get; set; }
    }
}