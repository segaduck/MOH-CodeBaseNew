using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Models;
using ES.Commons;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// Donate 線上捐款專案
    /// </summary>
    public class ApplyDonateViewModel
    {
        public ApplyDonateViewModel()
        {
            this.Form = new ApplyDonateFormModel();
            this.Detail = new ApplyDonateDetailModel();
            this.GridDetail = new List<ApplyDonateGridDetailModel>();
        }

        public ApplyDonateFormModel Form { get; set; }
        public List<ApplyDonateGridModel> Grid { get; set; }
        public ApplyDonateDetailModel Detail { get; set; }
        public HttpPostedFileBase File_1 { get; set; }
        public List<ApplyDonateGridDetailModel> GridDetail { get; set; }
    }

    /// <summary>
    /// 查詢條件
    /// </summary>
    public class ApplyDonateFormModel
    {

    }
    /// <summary>
    /// 查詢列表
    /// </summary>
    public class ApplyDonateGridModel : TblAPPLY_DONATE
    {
        /// <summary>
        /// 捐款總金額
        /// </summary>
        public string DonateAmt { get; set; }
    }

    /// <summary>
    /// 賑災專戶明細
    /// </summary>
    public class ApplyDonateDetailModel : TblAPPLY_DONATE
    {
        /// <summary>
        /// PK
        /// </summary>
        public string SRV_ID_DONATE { get; set; }

        [Required]
        [Display(Name = "賑災專戶名稱(中文)")]
        public string NAME_CH { get; set; }
        [Display(Name = "賑災專戶名稱(英文)")]
        public string NAME_ENG { get; set; }
        [Required]
        [Display(Name = "起始日")]
        public string START_DATE { get; set; }

        public string START_DATE_AD
        {
            get
            {
                if (string.IsNullOrEmpty(START_DATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(START_DATE, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                START_DATE = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }
        [Required]
        [Display(Name = "結束日")]
        public string END_DATE { get; set; }

        public string END_DATE_AD
        {
            get
            {
                if (string.IsNullOrEmpty(END_DATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(END_DATE, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                END_DATE = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 捐款方式
        /// </summary>
        public bool PAY_WAY_C { get; set; }
        public bool PAY_WAY_S { get; set; }
        public bool PAY_WAY_T { get; set; }
        public bool PAY_WAY_L { get; set; }

        /// <summary>
        /// 銀行代碼
        /// </summary>
        public string BANK_CODE { get; set; }
        /// <summary>
        /// 銀行帳號
        /// </summary>
        public string BANK_ACCOUNT { get; set; }
        /// <summary>
        /// 戶頭名稱
        /// </summary>
        public string BANK_NAME { get; set; }
        /// <summary>
        /// 專戶說明
        /// </summary>
        [Required]
        [Display(Name = "專戶說明(中文)")]
        public string DESC_CH { get; set; }

        [Display(Name = "專戶說明(英文)")]
        public string DESC_ENG { get; set; }

        /// <summary>
        /// 檔案上傳
        /// </summary>
        public List<string> FileList { get; set; }
    }

    /// <summary>
    /// 查詢列表
    /// </summary>
    public class ApplyDonateGridDetailModel : APPLY_PAY
    {
        /// <summary>
        /// 項次
        /// </summary>
        public string SEQ { get; set; }
        /// <summary>
        /// 收據編號
        /// </summary>
        public string REC_NO { get; set; }
        /// <summary>
        /// 捐贈者名稱或姓名
        /// </summary>
        public string D_NAME { get; set; }
        /// <summary>
        /// 捐贈用途
        /// </summary>
        public string D_USE { get; set; }
        /// <summary>
        /// 指定用途
        /// </summary>
        public string D_DUSE { get; set; }
        /// <summary>
        /// 說明
        /// </summary>
        public string D_REMARK { get; set; }
    }
}