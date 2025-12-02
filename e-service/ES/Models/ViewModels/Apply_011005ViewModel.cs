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
    /// APPLY_011005 專科社會工作師證書補發（遺失或污損）
    /// </summary>
    public class Apply_011005ViewModel
    {
        public Apply_011005ViewModel()
        {
            this.Form = new Apply_011005FormModel();
        }

        /// <summary>
        /// FormModel
        /// </summary>
        public Apply_011005FormModel Form { get; set; }

        /// <summary>
        /// 補件表單
        /// </summary>
        public Apply_011005AppDocModel AppDoc { get; set; }
    }
    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011005FormModel : ApplyModel
    {
        /// <summary>
        /// 申請日期
        /// </summary>
        public string APP_TIME_SHOW { get; set; }

        /// <summary>
        /// 專科類別
        /// </summary>
        [Display(Name = "專科類別")]
        [Required]
        public string SPECIALIST_TYPE { get; set; }

        /// <summary>
        /// 申請用途
        /// </summary>
        [Display(Name = "申請用途")]
        [Required]
        public string APPLY_TYPE { get; set; }



        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string BIRTHDAY_AD_TW
        {
            get
            {
                if (!BIRTHDAY.HasValue)
                    return null;
                else
                {
                    return HelperUtil.DateTimeToTwString(BIRTHDAY);
                }
            }
            set
            {
                if (BIRTHDAY.HasValue)
                {
                    value = HelperUtil.DateTimeToTwString(BIRTHDAY);
                }
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
                if (!BIRTHDAY.HasValue)
                    return null;
                else
                {
                    return HelperUtil.DateTimeToString(BIRTHDAY);
                }
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
        /// 申請人執業處所
        /// </summary>
        [Display(Name = "執業處所")]
        [Required]
        public string PRACTICE_PLACE { get; set; }

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string TEST_YEAR { get; set; }

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
        /// 申請人-戶籍謄本或戶口名簿影本
        /// </summary>
        public HttpPostedFileBase FILE_HOUSEHOLD { get; set; }
        public string FILE_HOUSEHOLD_TEXT { get; set; }

        #endregion 上傳檔案

        /// <summary>
        /// 專科類別清單
        /// </summary>
        public IList<SelectListItem> SPECIALIST_TYPE_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.SPECIALIST_TYPE_list;
            }
        }

        /// <summary>
        /// 申請狀態清單
        /// </summary>
        public IList<SelectListItem> APPLY_TYPE_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.APPLY_TYPE_list;
            }
        }
        /// <summary>
        /// 考試年度清單
        /// </summary>
        public IList<SelectListItem> TEST_YEAR_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.TEST_YEAR_list;
            }
        }

        ///// <summary>
        ///// PreView
        ///// </summary>
        //public Apply_001034ViewModel PreView { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public IList<SelectListItem> SEX_CD_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.SEX_CD_list;

            }
        }
    }



    /// <summary>
    /// 補件表單
    /// </summary>
    public class Apply_011005AppDocModel : ApplyModel
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

        /// <summary>
        /// 案號
        /// </summary>
        [Display(Name = "案號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public string APP_TIME_SHOW { get; set; }

        /// <summary>
        /// 專科類別
        /// </summary>
        [Display(Name = "專科類別")]
        [Required]
        public string SPECIALIST_TYPE { get; set; }

        /// <summary>
        /// 申請用途
        /// </summary>
        [Display(Name = "申請用途")]
        [Required]
        public string APPLY_TYPE { get; set; }



        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string BIRTHDAY_AD_TW
        {
            get
            {
                if (!BIRTHDAY.HasValue)
                    return null;
                else
                {
                    return HelperUtil.DateTimeToTwString(BIRTHDAY);
                }
            }
            set
            {
                if (BIRTHDAY.HasValue)
                {
                    value = HelperUtil.DateTimeToTwString(BIRTHDAY);
                }
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
                if (!BIRTHDAY.HasValue)
                    return null;
                else
                {
                    return HelperUtil.DateTimeToString(BIRTHDAY);
                }
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
                    var result = "";
                    if (!string.IsNullOrWhiteSpace(W_TEL_0))
                    {
                        result += W_TEL_0;
                    }
                    result += "-";
                    if (!string.IsNullOrWhiteSpace(W_TEL_1))
                    {
                        result += W_TEL_1;
                    }
                    if (!string.IsNullOrWhiteSpace(W_TEL_2))
                    {
                        result += "#" + W_TEL_2;
                    }
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var result = value.ToSplit('-');
                    W_TEL_0 = result[0];

                    if (result.Count > 1)
                    {
                        if (!string.IsNullOrWhiteSpace(result[1]))
                        {
                            var tel1_2 = result[1].ToSplit('#');
                            W_TEL_1 = tel1_2[0];
                            if (tel1_2.Count > 1)
                            {
                                W_TEL_2 = tel1_2[0];
                            }
                        }
                    }

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
                    var result = "";
                    if (!string.IsNullOrWhiteSpace(H_TEL_0))
                    {
                        result += H_TEL_0;
                    }
                    result += "-";
                    if (!string.IsNullOrWhiteSpace(H_TEL_1))
                    {
                        result += H_TEL_1;
                    }
                    if (!string.IsNullOrWhiteSpace(H_TEL_2))
                    {
                        result += "#" + H_TEL_2;
                    }
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var result = value.ToSplit('-');
                    H_TEL_0 = result[0];

                    if (result.Count > 1)
                    {
                        if (!string.IsNullOrWhiteSpace(result[1]))
                        {
                            var tel1_2 = result[1].ToSplit('#');
                            H_TEL_1 = tel1_2[0];
                            if (tel1_2.Count > 1)
                            {
                                H_TEL_2 = tel1_2[0];
                            }
                        }
                    }

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
        /// 申請人執業處所
        /// </summary>
        [Display(Name = "執業處所")]
        [Required]
        public string PRACTICE_PLACE { get; set; }

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string TEST_YEAR { get; set; }

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
        /// 申請人-戶籍謄本或戶口名簿影本
        /// </summary>
        public HttpPostedFileBase FILE_HOUSEHOLD { get; set; }
        public string FILE_HOUSEHOLD_TEXT { get; set; }

        #endregion 上傳檔案

        /// <summary>
        /// 專科類別清單
        /// </summary>
        public IList<SelectListItem> SPECIALIST_TYPE_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.SPECIALIST_TYPE_list;
            }
        }

        /// <summary>
        /// 申請狀態清單
        /// </summary>
        public IList<SelectListItem> APPLY_TYPE_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.APPLY_TYPE_list;
            }
        }
        /// <summary>
        /// 考試年度清單
        /// </summary>
        public IList<SelectListItem> TEST_YEAR_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.TEST_YEAR_list;
            }
        }

        /// <summary>
        /// 完成畫面
        /// </summary>
        public class Apply_011005DoneModel
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
    public class Apply_011005DetailModel : ApplyModel
    {
        /// <summary>
        /// 補件欄位字串
        /// </summary>
        [Display(Name = "補件欄位字串")]
        public string FieldStr { get; set; }

        /// <summary>
        /// 案號
        /// </summary>
        [Display(Name = "案號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public string APP_TIME_SHOW { get; set; }

        /// <summary>
        /// 專科類別
        /// </summary>
        [Display(Name = "專科類別")]
        [Required]
        public string SPECIALIST_TYPE { get; set; }

        /// <summary>
        /// 申請用途
        /// </summary>
        [Display(Name = "申請用途")]
        [Required]
        public string APPLY_TYPE { get; set; }



        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string BIRTHDAY_AD_TW
        {
            get
            {
                if (!BIRTHDAY.HasValue)
                    return null;
                else
                {
                    return HelperUtil.DateTimeToTwString(BIRTHDAY);
                }
            }
            set
            {
                if (BIRTHDAY.HasValue)
                {
                    value = HelperUtil.DateTimeToTwString(BIRTHDAY);
                }
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
                if (!BIRTHDAY.HasValue)
                    return null;
                else
                {
                    return HelperUtil.DateTimeToString(BIRTHDAY);
                }
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
                    var result = "";
                    if (!string.IsNullOrWhiteSpace(W_TEL_0))
                    {
                        result += W_TEL_0;
                    }
                    result += "-";
                    if (!string.IsNullOrWhiteSpace(W_TEL_1))
                    {
                        result += W_TEL_1;
                    }
                    if (!string.IsNullOrWhiteSpace(W_TEL_2))
                    {
                        result += "#" + W_TEL_2;
                    }
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var result = value.ToSplit('-');
                    W_TEL_0 = result[0];

                    if (result.Count > 1)
                    {
                        if (!string.IsNullOrWhiteSpace(result[1]))
                        {
                            var tel1_2 = result[1].ToSplit('#');
                            W_TEL_1 = tel1_2[0];
                            if (tel1_2.Count > 1)
                            {
                                W_TEL_2 = tel1_2[0];
                            }
                        }
                    }

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
                    var result = "";
                    if (!string.IsNullOrWhiteSpace(H_TEL_0))
                    {
                        result += H_TEL_0;
                    }
                    result += "-";
                    if (!string.IsNullOrWhiteSpace(H_TEL_1))
                    {
                        result += H_TEL_1;
                    }
                    if (!string.IsNullOrWhiteSpace(H_TEL_2))
                    {
                        result += "#" + H_TEL_2;
                    }
                    return result;
                }
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var result = value.ToSplit('-');
                    H_TEL_0 = result[0];

                    if (result.Count > 1)
                    {
                        if (!string.IsNullOrWhiteSpace(result[1]))
                        {
                            var tel1_2 = result[1].ToSplit('#');
                            H_TEL_1 = tel1_2[0];
                            if (tel1_2.Count > 1)
                            {
                                H_TEL_2 = tel1_2[0];
                            }
                        }
                    }

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
        /// 申請人執業處所
        /// </summary>
        [Display(Name = "執業處所")]
        [Required]
        public string PRACTICE_PLACE { get; set; }

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string TEST_YEAR { get; set; }

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
        /// 申請人-戶籍謄本或戶口名簿影本
        /// </summary>
        public HttpPostedFileBase FILE_HOUSEHOLD { get; set; }
        public string FILE_HOUSEHOLD_TEXT { get; set; }

        #endregion 上傳檔案

        /// <summary>
        /// 專科類別清單
        /// </summary>
        public IList<SelectListItem> SPECIALIST_TYPE_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.SPECIALIST_TYPE_list;
            }
        }

        /// <summary>
        /// 申請狀態清單
        /// </summary>
        public IList<SelectListItem> APPLY_TYPE_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.APPLY_TYPE_list;
            }
        }
        /// <summary>
        /// 考試年度清單
        /// </summary>
        public IList<SelectListItem> TEST_YEAR_LIST
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.TEST_YEAR_list;
            }
        }

        /// <summary>
        /// 完成畫面
        /// </summary>
        public class Apply_011005DoneModel
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
    public class Apply_011005DoneModel
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