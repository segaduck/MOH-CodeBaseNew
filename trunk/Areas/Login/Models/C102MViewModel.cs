using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Commons;
using EECOnline.Services;
using EECOnline.Models;
using EECOnline.Models.Entities;
using Turbo.Commons;
using Turbo.DataLayer;

namespace EECOnline.Areas.Login.Models
{
    public class C102MViewModel
    {
        /// <summary>申辦案件數</summary>
        public string ApplyNoSubNum { get; set; }

        /// <summary>待補上傳件數</summary>
        public string WaitUploadNum { get; set; }

        /// <summary>本月申辦預定收款金額</summary>
        public string MonthMoneySum { get; set; }

        /// <summary>當年度申請情形 (依序存 1~12月)</summary>
        public IList<string> YearApplyNoDatas { get; set; }

        /// <summary>當年度病歷單張類型申請情形</summary>
        public IList<Hashtable> YearHisTypeDatas { get; set; }

        public IList<C102MNewsModel> News { get; set; }
    }

    public class C102MNewsModel
    {
        public int? NewsID { get; set; }
        public string Type { get; set; }
        public string TypeName
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                if (this.Type.TONotNullString() == "") return "";
                return list.enews_list.Where(x => x.Value == this.Type).FirstOrDefault().Text;
            }
        }
        public string Date { get; set; }
        public string Subject { get; set; }
        public string IsTop { get; set; }
    }

    public class C102MNewsDetailModel : TblENEWS
    {
        public string newstype_text
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                if (this.newstype.TONotNullString() == "") return "";
                return list.enews_list.Where(x => x.Value == this.newstype).FirstOrDefault().Text;
            }
        }

        public IList<TblEFILE> Files { get; set; }
    }
}