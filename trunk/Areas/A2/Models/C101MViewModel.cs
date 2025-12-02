using System;
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
using Omu.ValueInjecter;
using Turbo.Commons;
using Turbo.DataLayer;

namespace EECOnline.Areas.A2.Models
{
    public class C101MViewModel
    {
        public C101MViewModel() { this.Form = new C101MFormModel(); }

        public C101MFormModel Form { get; set; }

        public C101MDetailModel Detail { get; set; }
    }

    public class C101MFormModel : PagingResultsViewModel
    {
        /// <summary>訂單編號</summary>
        [Display(Name = "訂單編號")]
        [Control(Mode = Control.TextBox, maxlength = "50", size = "92", placeholder = "請輸入訂單編號")]
        public string apply_no_sub { get; set; }

        /// <summary>申請人姓名</summary>
        [Display(Name = "申請人姓名")]
        [Control(Mode = Control.TextBox, maxlength = "20", size = "92", placeholder = "請輸入申請人姓名")]
        public string user_name { get; set; }

        /// <summary>申請區間起</summary>
        public string his_range1 { get; set; }

        [Display(Name = "申請區間起")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        public string his_range1_AD
        {
            get
            {
                if (string.IsNullOrEmpty(his_range1))
                    return null;
                else
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(his_range1, ""));  // YYYYMMDD 回傳給系統
            }
            set
            {
                his_range1 = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>申請區間迄</summary>
        public string his_range2 { get; set; }

        [Display(Name = "申請區間迄")]
        [Control(Mode = Control.DatePicker, group = 1, col = "2")]
        public string his_range2_AD
        {
            get
            {
                if (string.IsNullOrEmpty(his_range2))
                    return null;
                else
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(his_range2, ""));  // YYYYMMDD 回傳給系統
            }
            set
            {
                his_range2 = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>醫院登入時，只能看得到自己的資料</summary>
        public string HospCode { get; set; }

        public IList<C101MGridModel> Grid { get; set; }
    }

    public class C101MGridModel : TblEEC_ApplyDetail
    {
        private IList<TblEEC_Hospital_Api> HospApiList { get; set; }

        public C101MGridModel()
        {
            FrontDAO dao = new FrontDAO();
            this.HospApiList = dao.GetRowList(new TblEEC_Hospital_Api());
        }

        [NotDBField]
        public string user_name { get; set; }

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
                    Result = new A2DAO().GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = this.apply_no_sub })
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
                return tmpDT.ToString("yyy/MM/dd HH:mm:ss");
            }
        }

        [NotDBField]
        public string provide_datetime { get; set; }

        [NotDBField]
        public string provide_datetime_Text
        {
            get
            {
                if (this.provide_datetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.provide_datetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd HH:mm:ss");
            }
        }

        [NotDBField]
        public string TheStatus
        {
            get
            {
                return (this.payed == "Y") ? "已繳費" : "未繳費";
            }
        }
    }

    public class C101MDetailModel
    {
        public string apply_no_sub { get; set; }

        public string user_name { get; set; }

        public string user_birthday { get; set; }

        public string user_idno { get; set; }

        public string his_range1 { get; set; }

        public string his_range2 { get; set; }

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

        public IList<C101MDetailGridModel> DetailGrid { get; set; }
    }

    public class C101MDetailGridModel : TblEEC_ApplyDetailPrice
    {
        public string Get_Api_A1_Remark
        {
            get
            {
                var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                tmpObj.InjectFrom(this);
                return EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj);
            }
        }

        [NotDBField]
        public string IsGetReport { get; set; }

        [NotDBField]
        public string TheStatus
        {
            get
            {
                var Result = "";
                if (this.payed != "Y") Result = "未繳費";
                if (this.payed == "Y") Result = "已繳費";
                if (this.hospital_code == "1131010011H")
                {
                    // 是 亞東醫院
                    if (ec_success_yn == "成功") Result = "檔案已上傳";
                    if (ec_success_yn != "成功") Result = "檔案未上傳";
                }
                else
                if (this.hospital_code == "1317040011H")
                {
                    // 中山醫
                    Result = "";
                }
                else
                if (this.hospital_code == "1317050017H")
                {
                    // 中國醫
                    Result = "";
                }
                else
                {
                    // 是 一般 EEC
                    if (this.IsGetReport == "1") Result = "檔案已上傳";
                    if (this.IsGetReport == "0") Result = "檔案未上傳";
                }
                if (this.download_count > 0) Result = "檔案已下載";
                return Result;
            }
        }
    }
}