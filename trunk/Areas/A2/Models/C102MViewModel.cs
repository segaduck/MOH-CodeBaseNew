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
using Omu.ValueInjecter;
using Turbo.Commons;
using Turbo.DataLayer;

namespace EECOnline.Areas.A2.Models
{
    public class C102MViewModel
    {
        public C102MViewModel() { this.Form = new C102MFormModel(); }

        public C102MFormModel Form { get; set; }

        public C102MUploadModel Upload { get; set; }
    }

    public class C102MFormModel //: PagingResultsViewModel
    {
        /// <summary>醫院登入時，只能看得到自己的資料</summary>
        public string HospCode { get; set; }

        public IList<C102MGridModel> Grid { get; set; }

        public IList<C102MLogGridModel> LogGrid { get; set; }
    }

    public class C102MGridModel : TblEEC_ApplyDetailPrice
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
                return tmpDT.ToString("yyy/MM/dd HH:mm:ss");
            }
        }

        public string Get_Api_A1_Remark
        {
            get
            {
                var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                tmpObj.InjectFrom(this);
                return EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj);
            }
        }
    }

    public class C102MLogGridModel : TblEEC_ApplyDetailUploadLog
    {
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

        public string Get_Api_A1_Remark
        {
            get
            {
                var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                tmpObj.InjectFrom(this);
                return EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj);
            }
        }
    }

    public class C102MUploadModel
    {
        public string apply_no_sub { get; set; }
        public string his_type { get; set; }
        public HttpPostedFileBase UploadFILE { get; set; }
    }
}