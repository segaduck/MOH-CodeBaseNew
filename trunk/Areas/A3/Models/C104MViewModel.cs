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
    public class C104MViewModel
    {
        public C104MForm1Model Form1 { get; set; }

        public C104MForm2Model Form2 { get; set; }
    }

    public class C104MForm1Model : PagingResultsViewModel
    {
        [Display(Name = "醫院名稱")]
        [Control(Mode = Control.TextBox, maxlength = "20", size = "92", placeholder = "請輸入醫院名稱")]
        public string HospitalText { get; set; }

        [Display(Name = "申請人身分證字號")]
        [Control(Mode = Control.TextBox, maxlength = "10", size = "92", placeholder = "請輸入申請人身分證字號")]
        public string ApplyIdno { get; set; }

        public string ApplyDateS { get; set; }

        [Display(Name = "申請區間起")]
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

        [Display(Name = "申請區間迄")]
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

        public IList<C104MForm1GridModel> Form1Grid { get; set; }
    }

    public class C104MForm2Model : PagingResultsViewModel
    {
        /// <summary>手續費參數設定的動態SQL</summary>
        public string The_CommonType_SQL
        {
            get
            {
                var commonTypes = new FrontDAO().Get_EEC_CommonTypeAll();
                var Result = "";
                var Idx = 0;
                foreach (var row in commonTypes)
                {
                    if (row["type_price"].TONotNullString() == "0") continue;
                    Result = Result + "SUM(IIF(a.his_type_name='" + row["type_name"].ToString() + "', a.price, 0)) AS EEC_CommonType_" + Idx.ToString() + ", ";
                    Idx++;
                }
                return Result;
            }
        }

        [Display(Name = "醫院名稱")]
        [Control(Mode = Control.TextBox, maxlength = "20", size = "92", placeholder = "請輸入醫院名稱")]
        public string HospitalText { get; set; }

        public string ApplyDateS { get; set; }

        [Display(Name = "申請區間起")]
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

        [Display(Name = "申請區間迄")]
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

        public IList<C104MForm2GridModel> Form2Grid { get; set; }
    }

    public class C104MForm1GridModel : TblEEC_ApplyDetail
    {
        private IList<TblEEC_Hospital_Api> HospApiList { get; set; }

        public C104MForm1GridModel()
        {
            A3DAO dao = new A3DAO();
            this.HospApiList = dao.GetRowList(new TblEEC_Hospital_Api());
        }

        [NotDBField]
        public string user_name { get; set; }

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
                return tmpDT.ToString("yyy/MM/dd HH:mm:ss");
            }
        }

        [NotDBField]
        public long? price { get; set; }

        [NotDBField]
        public string price_Text { get { return string.Format("{0:N0}", this.price); } }

        [NotDBField]
        public long NeedUploadNum { get; set; }

        [NotDBField]
        public string NeedUploadNum_Text { get { return (this.NeedUploadNum == 0) ? "否" : "是"; } }

        [NotDBField]
        public string payed_Text { get { return (this.payed == "Y") ? "是" : "否"; } }

        [NotDBField]
        public string user_birthday { get; set; }

        [NotDBField]
        private List<TblEEC_ApplyDetailPrice> TheData_ApplyDetails
        {
            get
            {
                // 是屬於特殊醫院代號，要用他的 API 去抓項目 (現在不動態抓 API 了，直接拿 DB 存的去顯示)
                var Result = new List<TblEEC_ApplyDetailPrice>();
                if (!string.IsNullOrEmpty(this.apply_no_sub))
                {
                    Result = new A3DAO().GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = this.apply_no_sub })
                        .Where(x => x.his_type.TONotNullString() != "").ToList();
                }
                return Result;
            }
        }

        [NotDBField]
        public string his_types_Text
        {
            get
            {
                List<string> rtnList = new List<string>();
                if (this.hospital_code.TONotNullString() == "" || this.his_range1.TONotNullString() == "" || this.his_range2.TONotNullString() == "" || this.his_types.TONotNullString() == "") return "";
                if (this.hospital_code == "1131010011H")
                {
                    // 亞東
                    var findApi = this.HospApiList.Where(x => x.hospital_code == this.hospital_code && x.hospital_apikey == "A1").ToList();
                    if (findApi.ToCount() == 1)
                    {
                        foreach (var row in this.TheData_ApplyDetails)
                        {
                            var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                            tmpObj.InjectFrom(row);
                            tmpObj.ec_no = row.his_type;
                            tmpObj.ec_name = row.his_type_name;
                            rtnList.Add(row.his_type_name + EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj));
                        }
                    }
                }
                else
                if (this.hospital_code == "1317040011H")
                {
                    // 中山醫
                    foreach (var row in this.TheData_ApplyDetails)
                    {
                        var tmpObj = new EECOnline.Utils.Hospital_csh_Api.Api_A1ResultModel();
                        tmpObj.InjectFrom(row);
                        tmpObj.ec_no = row.his_type;
                        tmpObj.ec_name = row.his_type_name;
                        rtnList.Add(row.his_type_name + EECOnline.Utils.Hospital_csh_Api.Api_A1_Remark(tmpObj));
                    }
                }
                else
                if (this.hospital_code == "1317050017H")
                {
                    // 中國醫

                }
                else
                {
                    // 找不到時，才抓自己系統的
                    rtnList.AddRange(
                        new ShareCodeListModel().Get_HIS_Type_AllList()
                        .Where(x => this.his_types.Contains(x.Value))
                        .Select(x => x.Text)
                    );
                }
                return string.Join("<br />", rtnList);
            }
        }
    }

    public class C104MForm2GridModel : TblEEC_ApplyDetail
    {
        //[NotDBField]
        //public long? Other1 { get; set; }

        //[NotDBField]
        //public long? Other2 { get; set; }

        [NotDBField]
        public long? HisType { get; set; }

        [NotDBField]
        public long? SubSum { get; set; }

        [NotDBField]
        public long? ApplyNum { get; set; }

        //[NotDBField]
        //public string Other1_Text { get { return string.Format("{0:N0}", this.Other1); } }

        //[NotDBField]
        //public string Other2_Text { get { return string.Format("{0:N0}", this.Other2); } }

        [NotDBField]
        public string HisType_Text { get { return string.Format("{0:N0}", this.HisType); } }

        [NotDBField]
        public string SubSum_Text { get { return string.Format("{0:N0}", this.SubSum); } }

        // 以下對應動態的 EEC_CommonType，阿不過現在限定使用者只能建 2 筆，所以不夠再開吧！
        [NotDBField] public long? EEC_CommonType_0 { get; set; }
        [NotDBField] public long? EEC_CommonType_1 { get; set; }
        [NotDBField] public string EEC_CommonType_0_Text { get { return string.Format("{0:N0}", this.EEC_CommonType_0); } }
        [NotDBField] public string EEC_CommonType_1_Text { get { return string.Format("{0:N0}", this.EEC_CommonType_1); } }
    }
}