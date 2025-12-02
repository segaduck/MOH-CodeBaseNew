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
    /// APPLY_011009 (衛福部) 社工師證書補發（遺失）
    /// </summary>
    public class Apply_011009ViewModel
    {
        public Apply_011009ViewModel()
        {
            this.Form = new Apply_011009FormModel();
        }

        /// <summary>
        /// FormModel
        /// </summary>
        public Apply_011009FormModel Form { get; set; }

        /// <summary>
        /// 補件表單
        /// </summary>
        public Apply_011009AppDocModel AppDoc { get; set; }
    }


    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011009FormModel : ApplyModel
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

        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string BIRTHDAY_TW
        {
            get
            {
                return (BIRTHDAY.HasValue ? HelperUtil.DateTimeToTwString(BIRTHDAY.Value) : null);
            }
            set
            {
                if (BIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToTwString(BIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    BIRTHDAY = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 申請人出生年月日-西元年
        /// </summary>
        public string BIRTHDAY_AD
        {
            get
            {
                return (BIRTHDAY.HasValue ? HelperUtil.DateTimeToString(BIRTHDAY.Value) : null);
            }
            set
            {
                if (BIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToString(BIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    BIRTHDAY = HelperUtil.TransToDateTime(value);
            }
        }

        #region 電話(公)
        /// <summary>
        /// 電話(公) 00-00000000#000
        /// </summary>
        public string W_TEL { get; set; }
        /// <summary>
        /// 電話(公)區碼
        /// </summary>
        public string W_TEL_0 { get; set; }
        /// <summary>
        /// 電話(公)8碼
        /// </summary>
        public string W_TEL_1 { get; set; }
        /// <summary>
        /// 電話分機
        /// </summary>
        public string W_TEL_2 { get; set; }
        #endregion 電話(公)

        #region 電話(宅)
        /// <summary>
        /// 電話(宅) 00-00000000#000
        /// </summary>
        public string H_TEL { get; set; }
        /// <summary>
        /// 電話(宅)區碼
        /// </summary>
        public string H_TEL_0 { get; set; }
        /// <summary>
        /// 電話(宅)8碼
        /// </summary>
        public string H_TEL_1 { get; set; }
        /// <summary>
        /// 電話(宅) 分機
        /// </summary>
        public string H_TEL_2 { get; set; }
        #endregion 電話(宅)

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
        public string EMAIL_0 { get; set; }
        public string EMAIL_1 { get; set; }
        public string EMAIL_2 { get; set; }
        public string EMAIL_3 { get; set; }
        #endregion EMAIL

        #region 地址
        /// <summary>
        /// 通訊地址郵遞區號
        /// </summary>
        [Display(Name = "通訊地址(郵遞區號)")]
        [Required]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 通訊地址(縣市、鄉鎮市區)
        /// </summary>
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 通訊地址-路巷弄號
        /// </summary>
        [Display(Name = "通訊地址(道路或街名)")]
        [Required]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 戶籍地址郵遞區號
        /// </summary>
        [Display(Name = "戶籍地址(郵遞區號)")]
        [Required]
        public string H_ZIPCODE { get; set; }

        /// <summary>
        /// 戶籍地址(縣市、鄉鎮市區)
        /// </summary>
        public string H_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 戶籍地址-路巷弄號
        /// </summary>
        [Display(Name = "戶籍地址(道路或街名)")]
        [Required]
        public string H_ADDR { get; set; }

        /// <summary>
        /// 同通訊地址
        /// </summary>
        public string H_EQUAL { get; set; }
        #endregion

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string TEST_YEAR { get; set; }

        /// <summary>
        /// 考試名稱類科
        /// </summary>
        [Display(Name = "考試名稱類科")]
        [Required]
        public string TEST_CATEGORY { get; set; }

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }



        #region 上傳檔案

        /// <summary>
        /// 申請人-考試院考試及格證書影本或電子證書 
        /// </summary>
        [Display(Name = "考試院考試及格證書影本或電子證書")]
        public HttpPostedFileBase FILE_PASSCOPY { get; set; }
        public string FILE_PASSCOPY_TEXT { get; set; }
        public string FILE_PASSCOPY_FILENAME { get; set; }

        /// <summary>
        /// 申請人-身分證正面影本
        /// </summary>
        [Display(Name = "身分證正面影本")]
        public HttpPostedFileBase FILE_IDNF { get; set; }
        public string FILE_IDNF_TEXT { get; set; }
        public string FILE_IDNF_FILENAME { get; set; }
        /// <summary>
        /// 申請人-身分證反面影本
        /// </summary>
        [Display(Name = "身分證反面影本")]
        public HttpPostedFileBase FILE_IDNB { get; set; }
        public string FILE_IDNB_TEXT { get; set; }
        public string FILE_IDNB_FILENAME { get; set; }

        /// <summary>
        /// 申請人-大頭貼-一年內2吋正面脫帽半身照片
        /// </summary>
        [Display(Name = "一年內2吋正面脫帽半身照片")]
        public HttpPostedFileBase FILE_PHOTO { get; set; }
        public string FILE_PHOTO_TEXT { get; set; }
        public string FILE_PHOTO_FILENAME { get; set; }

        #endregion 上傳檔案


        /// <summary>
        /// 考試年度清單
        /// </summary>
        public IList<SelectListItem> TEST_YEAR_LIST
        {
            get
            {
                IDictionary<string, string> IDic = new Dictionary<string, string>();
                Int32 NowYear = HelperUtil.TransToTwYear(DateTime.Now, "").Substring(0, 3).TOInt32();
                for (var i = 0; i < 16; i++)
                {
                    IDic.Add((NowYear - i).ToString(), (NowYear - i).ToString());
                }
                return MyCommonUtil.ConvertSelItems(IDic);
            }
        }

        ///// <summary>
        ///// PreView
        ///// </summary>
        //public Apply_001034ViewModel PreView { get; set; }
    }
    
    /// <summary>
    /// 補件表單
    /// </summary>
    public class Apply_011009AppDocModel : ApplyModel
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

        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string BIRTHDAY_TW
        {
            get
            {
                return (BIRTHDAY.HasValue ? HelperUtil.DateTimeToTwString(BIRTHDAY) : null);
            }
            set
            {
                if (BIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToTwString(BIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    BIRTHDAY = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 申請人出生年月日-西元年
        /// </summary>
        public string BIRTHDAY_AD
        {
            get
            {
                return (BIRTHDAY.HasValue ? HelperUtil.DateTimeToString(BIRTHDAY) : null);
            }
            set
            {
                if (BIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToString(BIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    BIRTHDAY = HelperUtil.TransToDateTime(value);
            }
        }
        #region 電話(公)
        /// <summary>
        /// 電話(公) 00-00000000#000
        /// </summary>
        public string W_TEL
        {
            get
            {
                if (string.IsNullOrWhiteSpace(W_TEL_0) && string.IsNullOrWhiteSpace(W_TEL_1) && string.IsNullOrWhiteSpace(W_TEL_2))
                {
                    return null;
                }
                else
                {
                    var result = string.Format("{0}-{1}#{2}", W_TEL_0, W_TEL_1, W_TEL_2);
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    //var result = value.ToSplit('-');
                    W_TEL_0 = value.ToSplit('-').FirstOrDefault();
                    W_TEL_1 = value.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                    W_TEL_2 = value.ToSplit('#').LastOrDefault();
                }
            }
        }
        /// <summary>
        /// 電話(公)區碼
        /// </summary>
        public string W_TEL_0 { get; set; }
        /// <summary>
        /// 電話(公)8碼
        /// </summary>
        public string W_TEL_1 { get; set; }
        /// <summary>
        /// 電話分機
        /// </summary>
        public string W_TEL_2 { get; set; }
        #endregion 電話(公)

        #region 電話(宅)
        /// <summary>
        /// 電話(宅) 00-00000000#000
        /// </summary>
        public string H_TEL
        {
            get
            {
                if (string.IsNullOrWhiteSpace(H_TEL_0) && string.IsNullOrWhiteSpace(H_TEL_1) && string.IsNullOrWhiteSpace(H_TEL_2))
                {
                    return null;
                }
                else
                {
                    var result = string.Format("{0}-{1}#{2}", H_TEL_0, H_TEL_1, H_TEL_2);
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    H_TEL_0 = value.ToSplit('-').FirstOrDefault();
                    H_TEL_1 = value.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                    H_TEL_2 = value.ToSplit('#').LastOrDefault();
                }
            }
        }
        /// <summary>
        /// 電話(宅)區碼
        /// </summary>
        public string H_TEL_0 { get; set; }
        /// <summary>
        /// 電話(宅)8碼
        /// </summary>
        public string H_TEL_1 { get; set; }
        /// <summary>
        /// 電話(宅) 分機
        /// </summary>
        public string H_TEL_2 { get; set; }

        #endregion 電話(宅)

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
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string TEST_YEAR { get; set; }

        /// <summary>
        /// 考試名稱類科
        /// </summary>
        [Display(Name = "考試名稱類科")]
        [Required]
        public string TEST_CATEGORY { get; set; }

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

        #region 地址
        /// <summary>
        /// 通訊地址郵遞區號
        /// </summary>
        [Display(Name = "通訊地址(郵遞區號)")]
        [Required]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 通訊地址(縣市、鄉鎮市區)
        /// </summary>
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 通訊地址-路巷弄號
        /// </summary>
        [Display(Name = "通訊地址(道路或街名)")]
        [Required]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 戶籍地址郵遞區號
        /// </summary>
        [Display(Name = "戶籍地址(郵遞區號)")]
        [Required]
        public string H_ZIPCODE { get; set; }

        /// <summary>
        /// 戶籍地址(縣市、鄉鎮市區)
        /// </summary>
        public string H_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 戶籍地址-路巷弄號
        /// </summary>
        [Display(Name = "戶籍地址(道路或街名)")]
        [Required]
        public string H_ADDR { get; set; }

        /// <summary>
        /// 同通訊地址
        /// </summary>
        public string H_EQUAL { get; set; }

        #endregion

        #region 上傳檔案
        /// <summary>
        /// 申請人-身分證正面影本
        /// </summary>
        public HttpPostedFileBase FILE_IDNF { get; set; }
        public string FILE_IDNF_TEXT { get; set; }

        /// <summary>
        /// 申請人-身分證反面影本
        /// </summary>
        public HttpPostedFileBase FILE_IDNB { get; set; }
        public string FILE_IDNB_TEXT { get; set; }

        /// <summary>
        /// 申請人-大頭貼
        /// </summary>
        public HttpPostedFileBase FILE_PHOTO { get; set; }
        public string FILE_PHOTO_TEXT { get; set; }

        /// <summary>
        /// 申請人-考試院考試及格證書影本或電子證書
        /// </summary>
        public HttpPostedFileBase FILE_PASSCOPY { get; set; }
        public string FILE_PASSCOPY_TEXT { get; set; }

        #endregion 上傳檔案

        /// <summary>
        /// 考試年度清單
        /// </summary>
        public IList<SelectListItem> TEST_YEAR_LIST
        {
            get
            {
                IDictionary<string, string> IDic = new Dictionary<string, string>();
                var NowYear = HelperUtil.TransToTwYear(DateTime.Now, "").Substring(0, 3).TOInt32();
                for (var i = 0; i < 16; i++)
                {
                    IDic.Add((NowYear - i).ToString(), (NowYear - i).ToString());
                }
                return MyCommonUtil.ConvertSelItems(IDic);
            }
        }

        /// <summary>
        /// 完成畫面
        /// </summary>
        public class Apply_011009DoneModel
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
    /// 補件表單
    /// </summary>
    public class Apply_011009DetailModel : ApplyModel
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

        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string BIRTHDAY_TW
        {
            get
            {
                return (BIRTHDAY.HasValue ? HelperUtil.DateTimeToTwString(BIRTHDAY.Value) : null);
            }
            set
            {
                if (BIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToTwString(BIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    BIRTHDAY = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 申請人出生年月日-西元年
        /// </summary>
        public string BIRTHDAY_AD
        {
            get
            {
                return (BIRTHDAY.HasValue ? HelperUtil.DateTimeToString(BIRTHDAY.Value) : null);
            }
            set
            {
                if (BIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToString(BIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    BIRTHDAY = HelperUtil.TransToDateTime(value);
            }
        }
        #region 電話(公)
        /// <summary>
        /// 電話(公) 00-00000000#000
        /// </summary>
        public string W_TEL
        {
            get
            {
                if (string.IsNullOrWhiteSpace(W_TEL_0) && string.IsNullOrWhiteSpace(W_TEL_1) && string.IsNullOrWhiteSpace(W_TEL_2))
                {
                    return null;
                }
                else
                {
                    var result = string.Format("{0}-{1}#{2}", W_TEL_0, W_TEL_1, W_TEL_2);
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    //var result = value.ToSplit('-');
                    W_TEL_0 = value.ToSplit('-').FirstOrDefault();
                    W_TEL_1 = value.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                    W_TEL_2 = value.ToSplit('#').LastOrDefault();
                }
            }
        }
        /// <summary>
        /// 電話(公)區碼
        /// </summary>
        public string W_TEL_0 { get; set; }
        /// <summary>
        /// 電話(公)8碼
        /// </summary>
        public string W_TEL_1 { get; set; }
        /// <summary>
        /// 電話分機
        /// </summary>
        public string W_TEL_2 { get; set; }
        #endregion 電話(公)

        #region 電話(宅)
        /// <summary>
        /// 電話(宅) 00-00000000#000
        /// </summary>
        public string H_TEL
        {
            get
            {
                if (string.IsNullOrWhiteSpace(H_TEL_0) && string.IsNullOrWhiteSpace(H_TEL_1) && string.IsNullOrWhiteSpace(H_TEL_2))
                {
                    return null;
                }
                else
                {
                    var result = string.Format("{0}-{1}#{2}", H_TEL_0, H_TEL_1, H_TEL_2);
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    //var result = value.ToSplit('-');
                    H_TEL_0 = value.ToSplit('-').FirstOrDefault();
                    H_TEL_1 = value.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                    H_TEL_2 = value.ToSplit('#').LastOrDefault();
                }
            }
        }
        /// <summary>
        /// 電話(宅)區碼
        /// </summary>
        public string H_TEL_0 { get; set; }
        /// <summary>
        /// 電話(宅)8碼
        /// </summary>
        public string H_TEL_1 { get; set; }
        /// <summary>
        /// 電話(宅) 分機
        /// </summary>
        public string H_TEL_2 { get; set; }

        #endregion 電話(宅)

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
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string TEST_YEAR { get; set; }

        /// <summary>
        /// 考試名稱類科
        /// </summary>
        [Display(Name = "考試名稱類科")]
        [Required]
        public string TEST_CATEGORY { get; set; }

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

        #region 地址
        /// <summary>
        /// 通訊地址郵遞區號
        /// </summary>
        [Display(Name = "通訊地址(郵遞區號)")]
        [Required]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 通訊地址(縣市、鄉鎮市區)
        /// </summary>
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 通訊地址-路巷弄號
        /// </summary>
        [Display(Name = "通訊地址(道路或街名)")]
        [Required]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 戶籍地址郵遞區號
        /// </summary>
        [Display(Name = "戶籍地址(郵遞區號)")]
        [Required]
        public string H_ZIPCODE { get; set; }

        /// <summary>
        /// 戶籍地址(縣市、鄉鎮市區)
        /// </summary>
        public string H_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 戶籍地址-路巷弄號
        /// </summary>
        [Display(Name = "戶籍地址(道路或街名)")]
        [Required]
        public string H_ADDR { get; set; }

        /// <summary>
        /// 同通訊地址
        /// </summary>
        public string H_EQUAL { get; set; }

        #endregion

        #region 上傳檔案
        /// <summary>
        /// 申請人-身分證正面影本
        /// </summary>
        public HttpPostedFileBase FILE_IDNF { get; set; }
        public string FILE_IDNF_TEXT { get; set; }

        /// <summary>
        /// 申請人-身分證反面影本
        /// </summary>
        public HttpPostedFileBase FILE_IDNB { get; set; }
        public string FILE_IDNB_TEXT { get; set; }

        /// <summary>
        /// 申請人-大頭貼
        /// </summary>
        public HttpPostedFileBase FILE_PHOTO { get; set; }
        public string FILE_PHOTO_TEXT { get; set; }

        /// <summary>
        /// 申請人-考試院考試及格證書影本或電子證書
        /// </summary>
        public HttpPostedFileBase FILE_PASSCOPY { get; set; }
        public string FILE_PASSCOPY_TEXT { get; set; }

        #endregion 上傳檔案


        /// <summary>
        /// 考試年度清單
        /// </summary>
        public IList<SelectListItem> TEST_YEAR_LIST
        {
            get
            {
                IDictionary<string, string> IDic = new Dictionary<string, string>();
                var NowYear = HelperUtil.TransToTwYear(DateTime.Now, "").Substring(0, 3).TOInt32();
                for (var i = 0; i < 16; i++)
                {
                    IDic.Add((NowYear - i).ToString(), (NowYear - i).ToString());
                }
                return MyCommonUtil.ConvertSelItems(IDic);
            }
        }

        /// <summary>
        /// 完成畫面
        /// </summary>
        public class Apply_011009DoneModel
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
    public class Apply_011009DoneModel
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