using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using System.Web.Mvc;
using ES.Services;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_011010 全國社會工作專業人員選拔推薦
    /// </summary>
    public class Apply_011010ViewModel
    {
        public Apply_011010ViewModel()
        {
            this.Form = new Apply_011010FormModel();
        }

        /// <summary>
        /// FormModel
        /// </summary>
        public Apply_011010FormModel Form { get; set; }

        /// <summary>
        /// 補件表單
        /// </summary>
        public Apply_011010AppDocModel AppDoc { get; set; }
    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011010FormModel : ApplyModel
    {
        /// <summary>
        /// 是否上傳附件 (0:不上傳 / 1:上傳 )
        /// </summary>
        public string IsUpLoadFile { get; set; }

        public DateTime? APPLY_DATE { get; set; }

        /// <summary>
        /// 申請日期-TW
        /// </summary>
        public string APPLY_DATE_TW
        {
            get
            {
                return (APPLY_DATE.HasValue ? HelperUtil.DateTimeToTwString(APPLY_DATE.Value) : null);
            }
            set
            {
                if (APPLY_DATE.HasValue)
                    value = HelperUtil.DateTimeToTwString(APPLY_DATE);
                if (!string.IsNullOrWhiteSpace(value))
                    APPLY_DATE = HelperUtil.TransTwToDateTime(value);
            }
        }

        [Display(Name="單位類型")]
        public string UNIT_TYPE { get; set; }

        [Display(Name ="單位類型(私部門)")]
        public string UNIT_SUBTYPE { get; set; }

        [Display(Name ="單位名稱")]
        public string UNIT_NAME { get; set;}

        [Display(Name ="單位聯絡人局處/部門")]
        public string UNIT_DEPART { get; set; }

        [Display(Name ="單位聯絡人職稱")]
        public string UNIT_TITLE { get; set; }

        [Display(Name ="單位聯絡人姓名")]
        public string UNIT_CNAME { get; set; }

        [Display(Name ="連絡電話")]
        public string UNIT_TEL { get; set; }

        #region UNIT_EMAIL
        [Display(Name = "E-MAIL")]
        [Required]
        public string EMAIL
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EMAIL_1))
                    return EMAIL_0 + "@" + ((string.IsNullOrWhiteSpace(EMAIL_1) || EMAIL_1 == "0") ? (string.IsNullOrWhiteSpace(EMAIL_2) ? EMAIL_3 : EMAIL_2) : new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == EMAIL_1).FirstOrDefault().Text);

                else
                {
                    return null;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = EMAIL_0 + "@" + ((string.IsNullOrWhiteSpace(EMAIL_1) || EMAIL_1 == "0") ? (string.IsNullOrWhiteSpace(EMAIL_2) ? EMAIL_3 : EMAIL_2) : new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == EMAIL_1).FirstOrDefault().Text);
                }
                else
                {
                    var emailArr = value.ToSplit('@');
                    EMAIL_0 = emailArr[0];
                    if (emailArr.ToCount() > 1)
                    {
                        var email1List = new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == emailArr[1]).ToList();
                        if (email1List.ToCount() != 0)
                        {
                            EMAIL_1 = email1List.FirstOrDefault().Value;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(emailArr[1]))
                            {
                                EMAIL_1 = "0";
                                EMAIL_2 = emailArr[1];
                            }
                        }
                    }
                }
            }
        }
        public string EMAIL_0 { get; set; }
        public string EMAIL_1 { get; set; }
        public string EMAIL_2 { get; set; }
        public string EMAIL_3 { get; set; }
        #endregion UNIT_EMAIL

        [Display(Name = "單位社工人員總數")]
        public string CNT_TYPE { get; set; }

        [Display(Name ="總會人數")]
        public string CNT_A { get; set; }

        [Display(Name ="分會人數")]
        public string CNT_B { get; set; }

        [Display(Name = "由總會(事務所)統一推薦人數")]
        public string CNT_C { get; set; }

        [Display(Name = "單位類型非協會(事務所)人數")]
        public string CNT_D { get; set;}

        [Display(Name = "績優社工獎推薦人數")]
        public string CNT_E { get; set;}

        [Display(Name = "績優社工督導獎推薦人數")]
        public string CNT_F { get; set;}

        [Display(Name = "資深敬業獎推薦人數")]
        public string CNT_G { get; set;}

        [Display(Name = "特殊貢獻獎推薦人數")]
        public string CNT_H { get; set;}

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

        #region 上傳檔案

        /// <summary>
        /// 彙整表(EXCEL檔)
        /// </summary>
        [Display(Name = "彙整表(EXCEL檔)")]
        public HttpPostedFileBase FILE_EXCEL { get; set; }
        public string FILE_EXCEL_TEXT { get; set; }

        /// <summary>
        /// 檢核表(PDF檔)
        /// </summary>
        [Display(Name = "檢核表(PDF檔)")]
        public HttpPostedFileBase FILE_PDF { get; set; }
        public string FILE_PDF_TEXT { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)
        /// </summary>
        [Display(Name = "推薦表(PDF檔)")]
        public HttpPostedFileBase FILE_3 { get; set; }
        public string FILE_3_TEXT { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)_清單
        /// </summary>
        public IList<Apply_011010SRVLSTModel> SRVLIST { get; set; }


        #endregion 上傳檔案
    }
    public class Apply_011010SRVLSTModel : Apply_FileModel
    {
        public string SEQ_NO { get; set; }
        /// <summary>
        /// 推薦表(PDF檔)
        /// </summary>
        [Display(Name = "推薦表(PDF檔)")]
        public HttpPostedFileBase FILE_3 { get; set; }
        public string FILE_3_TEXT { get; set; }
    }
    /// <summary>
    /// 補件表單
    /// </summary>
    public class Apply_011010AppDocModel : ApplyModel
    {
  
        /// <summary>
        /// 補件狀態
        /// </summary>
        public string APPSTATUS { get; set; }

        /// <summary>
        /// 補件欄位字串
        /// </summary>
        [Display(Name = "補件欄位字串")]
        public string FieldStr { get; set; }

        //APP_TIME_TW
        public string APP_TIME_TW
        {
            get
            {
                return (APP_TIME.HasValue ? HelperUtil.DateTimeToTwString(APP_TIME) : null);
            }
            set
            {
                if (APP_TIME.HasValue)
                    value = HelperUtil.DateTimeToTwString(APP_TIME);
                if (!string.IsNullOrWhiteSpace(value))
                    APP_TIME = HelperUtil.TransTwToDateTime(value);
            }
        }

        public DateTime? APPLY_DATE { get; set; }

        //APPLY_DATE_TW
        public string APPLY_DATE_TW
        {
            get
            {
                return (APPLY_DATE.HasValue ? HelperUtil.DateTimeToTwString(APPLY_DATE) : null);
            }
            set
            {
                if (APPLY_DATE.HasValue)
                    value = HelperUtil.DateTimeToTwString(APPLY_DATE);
                if (!string.IsNullOrWhiteSpace(value))
                    APPLY_DATE = HelperUtil.TransTwToDateTime(value);
            }
        }


        [Display(Name = "單位類型")]
        public string UNIT_TYPE { get; set; }

        [Display(Name = "單位類型(私部門)")]
        public string UNIT_SUBTYPE { get; set; }

        [Display(Name = "單位名稱")]
        public string UNIT_NAME { get; set; }

        [Display(Name = "單位聯絡人局處/部門")]
        public string UNIT_DEPART { get; set; }

        [Display(Name = "單位聯絡人職稱")]
        public string UNIT_TITLE { get; set; }

        [Display(Name = "單位聯絡人姓名")]
        public string UNIT_CNAME { get; set; }

        [Display(Name = "連絡電話")]
        public string UNIT_TEL { get; set; }

        #region UNIT_MAIL
        [Display(Name = "E-MAIL")]
        [Required]
        public string EMAIL
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EMAIL_1))
                    return EMAIL_0 + "@" + ((string.IsNullOrWhiteSpace(EMAIL_1) || EMAIL_1 == "0") ? (string.IsNullOrWhiteSpace(EMAIL_2) ? EMAIL_3 : EMAIL_2) : new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == EMAIL_1).FirstOrDefault().Text);

                else
                {
                    return null;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = EMAIL_0 + "@" + ((string.IsNullOrWhiteSpace(EMAIL_1) || EMAIL_1 == "0") ? (string.IsNullOrWhiteSpace(EMAIL_2) ? EMAIL_3 : EMAIL_2) : new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == EMAIL_1).FirstOrDefault().Text);
                }
                else
                {
                    var emailArr = value.ToSplit('@');
                    EMAIL_0 = emailArr[0];
                    if (emailArr.ToCount() > 1)
                    {
                        var email1List = new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == emailArr[1]).ToList();
                        if (email1List.ToCount() != 0)
                        {
                            EMAIL_1 = email1List.FirstOrDefault().Value;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(emailArr[1]))
                            {
                                EMAIL_1 = "0";
                                EMAIL_2 = emailArr[1];
                            }
                        }
                    }
                }
            }
        }
        public string EMAIL_0
        {
            get;
            set;
        }
        public string EMAIL_1
        {
            get;
            set;
        }
        public string EMAIL_2
        {
            get;
            set;
        }
        public string EMAIL_3 { get; set; }
        #endregion UNIT_MAIL

        [Display(Name = "單位社工人員總數")]
        public string CNT_TYPE { get; set; }

        [Display(Name = "總會人數")]
        public string CNT_A { get; set; }

        [Display(Name = "分會人數")]
        public string CNT_B { get; set; }

        [Display(Name = "由總會(事務所)統一推薦人數")]
        public string CNT_C { get; set; }

        [Display(Name = "單位類型非協會(事務所)人數")]
        public string CNT_D { get; set; }

        [Display(Name = "績優社工獎推薦人數")]
        public string CNT_E { get; set; }

        [Display(Name = "績優社工督導獎推薦人數")]
        public string CNT_F { get; set; }

        [Display(Name = "資深敬業獎推薦人數")]
        public string CNT_G { get; set; }

        [Display(Name = "特殊貢獻獎推薦人數")]
        public string CNT_H { get; set; }

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

        #region 上傳檔案
        /// <summary>
        /// 彙整表(EXCEL檔)
        /// </summary>
        public HttpPostedFileBase FILE_EXCEL { get; set; }
        public string FILE_EXCEL_TEXT { get; set; }

        /// <summary>
        /// 檢核表(PDF檔)
        /// </summary>
        public HttpPostedFileBase FILE_PDF { get; set; }
        public string FILE_PDF_TEXT { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)
        /// </summary>
        public HttpPostedFileBase FILE_3 { get; set; }
        public string FILE_3_TEXT { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)_清單
        /// </summary>
        public IList<Apply_011010SRVLSTModel> SRVLIST { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_011010FILEModel> FILE { get; set; }

        #endregion 上傳檔案

        /// <summary>
        /// 完成畫面
        /// </summary>
        public class Apply_011010DoneModel
        {
            /// <summary>
            /// 狀態
            /// </summary>
            public string status { get; set; }

            /// <summary>
            /// 件數
            /// </summary>
            public string Count { get; set; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Apply_011010FILEModel : Apply_FileModel
    {
    }

    /// <summary>
    /// 補件表單
    /// </summary>
    public class Apply_011010DetailModel : ApplyModel
    {

        /// <summary>
        /// 申請日期-TW
        /// </summary>
        public string APP_TIME_TW
        {
            get
            {
                return (APP_TIME.HasValue ? HelperUtil.DateTimeToTwString(APP_TIME.Value) : null);
            }
            set
            {
                if (APP_TIME.HasValue)
                    value = HelperUtil.DateTimeToTwString(APP_TIME);
                if (!string.IsNullOrWhiteSpace(value))
                    APP_TIME = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 補件欄位字串
        /// </summary>
        [Display(Name = "補件欄位字串")]
        public string FieldStr { get; set; }

        #region EMAIL
        [Display(Name = "E-MAIL")]
        [Required]
        public string EMAIL
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EMAIL_1))
                    return EMAIL_0 + "@" + ((string.IsNullOrWhiteSpace(EMAIL_1) || EMAIL_1 == "0") ? (string.IsNullOrWhiteSpace(EMAIL_2) ? EMAIL_3 : EMAIL_2) : new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == EMAIL_1).FirstOrDefault().Text);

                else
                {
                    return null;
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = EMAIL_0 + "@" + ((string.IsNullOrWhiteSpace(EMAIL_1) || EMAIL_1 == "0") ? (string.IsNullOrWhiteSpace(EMAIL_2) ? EMAIL_3 : EMAIL_2) : new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == EMAIL_1).FirstOrDefault().Text);
                }
                else
                {
                    var emailArr = value.ToSplit('@');
                    EMAIL_0 = emailArr[0];
                    if (emailArr.ToCount() > 1)
                    {
                        var email1List = new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == emailArr[1]).ToList();
                        if (email1List.ToCount() != 0)
                        {
                            EMAIL_1 = email1List.FirstOrDefault().Value;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(emailArr[1]))
                            {
                                EMAIL_1 = "0";
                                EMAIL_2 = emailArr[1];
                            }
                        }
                    }
                }
            }
        }
        public string EMAIL_0
        {
            get;
            set;
        }
        public string EMAIL_1
        {
            get;
            set;
        }
        public string EMAIL_2
        {
            get;
            set;
        }
        public string EMAIL_3 { get; set; }
        #endregion EMAIL

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

        #region 上傳檔案

        /// <summary>
        /// 彙整表(EXCEL檔)
        /// </summary>
        public HttpPostedFileBase FILE_EXCEL{ get; set; }
        public string FILE_EXCEL_TEXT { get; set; }

        /// <summary>
        /// 檢核表(PDF檔)
        /// </summary>
        public HttpPostedFileBase FILE_PDF { get; set; }
        public string FILE_PDF_TEXT { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)
        /// </summary>
        public HttpPostedFileBase FILE_3 { get; set; }
        public string FILE_3_TEXT { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)_清單
        /// </summary>
        public IList<Apply_011010SRVLSTModel> SRVLIST { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_011010FILEModel> FILE { get; set; }

        #endregion 上傳檔案

        /// <summary>
        /// 完成畫面
        /// </summary>
        public class Apply_011010DoneModel
        {
            /// <summary>
            /// 狀態
            /// </summary>
            public string status { get; set; }

            /// <summary>
            /// 件數
            /// </summary>
            public string Count { get; set; }
        }
    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_011010DoneModel
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 件數
        /// </summary>
        public string Count { get; set; }
    }
}