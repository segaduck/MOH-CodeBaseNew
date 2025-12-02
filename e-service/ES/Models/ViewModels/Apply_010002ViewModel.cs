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
    /// APPLY_010002 低收入戶及中低收入戶之體外受精（俗稱試管嬰兒）補助方案
    /// </summary>
    public class Apply_010002ViewModel
    {
        public Apply_010002ViewModel()
        {
            this.Form = new Apply_010002FormModel();
        }

        /// <summary>
        /// FormModel
        /// </summary>
        public Apply_010002FormModel Form { get; set; }

        /// <summary>
        /// 補件表單
        /// </summary>
        public Apply_010002AppDocModel AppDoc { get; set; }
    }


    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_010002FormModel : ApplyModel
    {
        /// <summary>
        /// 申請日期
        /// </summary>
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
        /// 承辦單位
        /// </summary>
        public string ORG_NAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人
        /// </summary>
        public string APNAME { get; set; } //nvarchar 60     

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

        /// <summary>
        /// *申請人電話 TEL
        /// </summary>
        public string TEL_0 { get; set; } //varchar 30 允許
        public string TEL_1 { get; set; } //varchar 30 允許
        public string TEL_2 { get; set; } //varchar 30 允許

        /// <summary>
        /// *申請人手機 MOBILE
        /// </summary>
        //public string MOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 申請人E-MAIL EMAIL
        /// </summary>
        public string EMAIL { get; set; } //varchar 60 允許
        public string EMAIL_0 { get; set; }
        public string EMAIL_1 { get; set; }
        public string EMAIL_2 { get; set; }


        /// <summary>
        /// *配偶姓名 SPNAME
        /// </summary>
        public string SPNAME { get; set; } //nvarchar 60    
        /// <summary>
        /// *配偶身份證統一編號/外籍統一證號或護照號碼 SPIDN/SPIDNS
        /// </summary>
        public string SPIDN { get; set; } //varchar 20     
        /// <summary>
        /// *配偶出生年月日 SPBIRTH
        /// </summary>
        public DateTime? SPBIRTHDAY { get; set; } //date   允許
        public string SPBIRTHDAY_TW
        {
            get
            {
                return (SPBIRTHDAY.HasValue ? HelperUtil.DateTimeToTwString(SPBIRTHDAY) : null);
            }
            set
            {
                if (SPBIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToTwString(SPBIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    SPBIRTHDAY = HelperUtil.TransTwToDateTime(value);
            }
        }
        public string SPBIRTHDAY_AD
        {
            get
            {
                return (SPBIRTHDAY.HasValue ? HelperUtil.DateTimeToString(SPBIRTHDAY) : null);
            }
            set
            {
                if (SPBIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToString(SPBIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    SPBIRTHDAY = HelperUtil.TransToDateTime(value);
            }
        }

        /// <summary>
        /// *配偶電話 SPTEL
        /// </summary>
        public string SPTEL { get; set; } //varchar 30 允許
        public string SPTEL_0 { get; set; } //varchar 30 允許
        public string SPTEL_1 { get; set; } //varchar 30 允許
        public string SPTEL_2 { get; set; } //varchar 30 允許


        /// <summary>
        /// *配偶手機 SPMOBILE
        /// </summary>
        public string SPMOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 配偶E-MAIL SPEMAIL
        /// </summary>
        public string SPEMAIL { get; set; } //varchar 60 允許

        public string SPEMAIL_0 { get; set; }
        public string SPEMAIL_1 { get; set; }
        public string SPEMAIL_2 { get; set; }

        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ZIPCODE { get; set; } //varchar 5 允許
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ADDR { get; set; } //nvarchar 300 允許

        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ZIPCODE { get; set; } //varchar 5 允許
        public string H_ZIPCODE_TEXT { get; set; }
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 同戶籍地址
        /// </summary>
        public string H_EQUAL { get; set; } //varchar 1 允許
        public bool H_EQUAL_FG { get; set; }

        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "國民身分證影印本正反面-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public string MYDATA_GET1 { get; set; } //varchar 30 允許
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "國民身分證影印本正反面-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public bool MYDATA_GET_FG1 { get; set; }
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public string MYDATA_GET2 { get; set; } //varchar 30 允許
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public bool MYDATA_GET_FG2 { get; set; }

        /// <summary>
        /// 佐證文件檔案上傳--佐證文件採合併檔案是 否
        /// </summary>
        public string MERGEYN { get; set; } //varchar 1 允許

        #region  上傳檔案

        /// <summary>
        /// 國民身分證影印本(正面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(正面) ")]
        public HttpPostedFileBase FILE_IDCNF { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(反面) ")]
        public HttpPostedFileBase FILE_IDCNB { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        [Display(Name = "人工生殖機構開立之不孕症診斷證明書")]
        public HttpPostedFileBase FILE_DISEASE { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件")]
        public HttpPostedFileBase FILE_LOWREC { get; set; } //varchar 20 允許

        /// <summary>
        ///  國民身分證影印本(正面) 
        /// </summary>
        public string FILE_IDCNF_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        public string FILE_IDCNB_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        public string FILE_DISEASE_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        public string FILE_LOWREC_TEXT { get; set; } //varchar 20 允許

        #endregion

    }

    /// <summary>
    /// 補件表單
    /// </summary>
    public class Apply_010002AppDocModel : ApplyModel
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

        public string APP_TIME_AD
        {
            get
            {
                return (APP_TIME.HasValue ? HelperUtil.DateTimeToString(APP_TIME.Value) : null);
            }
            set
            {
                if (APP_TIME.HasValue)
                    value = HelperUtil.DateTimeToString(APP_TIME);
                if (!string.IsNullOrWhiteSpace(value))
                    APP_TIME = HelperUtil.TransToDateTime(value);
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

        public string APPLY_DATE_AD
        {
            get
            {
                return (APPLY_DATE.HasValue ? HelperUtil.DateTimeToString(APPLY_DATE.Value) : null);
            }
            set
            {
                if (APPLY_DATE.HasValue)
                    value = HelperUtil.DateTimeToString(APPLY_DATE);
                if (!string.IsNullOrWhiteSpace(value))
                    APPLY_DATE = HelperUtil.TransToDateTime(value);
            }
        }

        /// <summary>
        /// 承辦單位
        /// </summary>
        public string ORG_NAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人
        /// </summary>
        public string APNAME { get; set; } //nvarchar 60     

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

        public string TEL_0 { get; set; }
        public string TEL_1 { get; set; }
        public string TEL_2 { get; set; }

        /// <summary>
        /// *申請人手機 MOBILE
        /// </summary>
        //public string MOBILE { get; set; } //varchar 30 允許

        /// <summary>
        /// 申請人E-MAIL EMAIL
        /// </summary>
        public string EMAIL { get; set; } //varchar 60 允許
        public string EMAIL_0 { get; set; }
        public string EMAIL_1 { get; set; }
        public string EMAIL_2 { get; set; }
        //public string EMAIL_3 { get; set; }

        /// <summary>
        /// *配偶姓名 SPNAME
        /// </summary>
        public string SPNAME { get; set; } //nvarchar 60    
        /// <summary>
        /// *配偶身份證統一編號/外籍統一證號或護照號碼 SPIDN/SPIDNS
        /// </summary>
        public string SPIDN { get; set; } //varchar 20     
        /// <summary>
        /// *配偶出生年月日 SPBIRTH
        /// </summary>
        public DateTime? SPBIRTHDAY { get; set; } //date   允許

        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string SPBIRTHDAY_TW
        {
            get
            {
                return (SPBIRTHDAY.HasValue ? HelperUtil.DateTimeToTwString(SPBIRTHDAY.Value) : null);
            }
            set
            {
                if (SPBIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToTwString(SPBIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    SPBIRTHDAY = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 申請人出生年月日-西元年
        /// </summary>
        public string SPBIRTHDAY_AD
        {
            get
            {
                return (SPBIRTHDAY.HasValue ? HelperUtil.DateTimeToString(SPBIRTHDAY.Value) : null);
            }
            set
            {
                if (SPBIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToString(SPBIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    SPBIRTHDAY = HelperUtil.TransToDateTime(value);
            }
        }

        /// <summary>
        /// *配偶電話 SPTEL
        /// </summary>
        public string SPTEL { get; set; } //varchar 30 允許
        public string SPTEL_0 { get; set; }
        public string SPTEL_1 { get; set; }
        public string SPTEL_2 { get; set; }

        /// <summary>
        /// *配偶手機 SPMOBILE
        /// </summary>
        public string SPMOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 配偶E-MAIL SPEMAIL
        /// </summary>
        public string SPEMAIL { get; set; } //varchar 60 允許
        public string SPEMAIL_0 { get; set; }
        public string SPEMAIL_1 { get; set; }
        public string SPEMAIL_2 { get; set; }
        //public string SPEMAIL_3 { get; set; }

        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ZIPCODE { get; set; } //varchar 5 允許
        public string C_ZIPCODE_TEXT { get; set; }
        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ZIPCODE { get; set; } //varchar 5 允許
        public string H_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 同戶籍地址
        /// </summary>
        public string H_EQUAL { get; set; } //varchar 1 允許

        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "國民身分證影印本正反面-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public string MYDATA_GET1 { get; set; } //varchar 30 允許
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "國民身分證影印本正反面-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public bool MYDATA_GET_FG1 { get; set; }
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public string MYDATA_GET2 { get; set; } //varchar 30 允許
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件-同意自Mydata平台取得資料(需使用自然人憑證驗證身分)")]
        public bool MYDATA_GET_FG2 { get; set; }

        /// <summary>
        /// 佐證文件檔案上傳--佐證文件採合併檔案是 否
        /// </summary>
        public string MERGEYN { get; set; } //varchar 1 允許

        #region  上傳檔案

        /// <summary>
        /// 國民身分證影印本(正面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(正面) ")]
        public HttpPostedFileBase FILE_IDCNF { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(反面) ")]
        public HttpPostedFileBase FILE_IDCNB { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        [Display(Name = "人工生殖機構開立之不孕症診斷證明書")]
        public HttpPostedFileBase FILE_DISEASE { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件")]
        public HttpPostedFileBase FILE_LOWREC { get; set; } //varchar 20 允許

        /// <summary>
        ///  國民身分證影印本(正面) 
        /// </summary>
        public string FILE_IDCNF_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        public string FILE_IDCNB_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        public string FILE_DISEASE_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        public string FILE_LOWREC_TEXT { get; set; } //varchar 20 允許

        #endregion

    }

    public class Apply_010002DetailModel : ApplyModel
    {

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

        /// <summary>
        /// 承辦單位
        /// </summary>
        public string ORG_NAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人
        /// </summary>
        public string APNAME { get; set; } //nvarchar 60     

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

        /// <summary>
        /// *申請人電話 TEL
        /// </summary>
        public string TEL_0 { get; set; } //varchar 30 允許
        public string TEL_1 { get; set; } //varchar 30 允許
        public string TEL_2 { get; set; } //varchar 30 允許

        /// <summary>
        /// *申請人手機 MOBILE
        /// </summary>
        //public string MOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 申請人E-MAIL EMAIL
        /// </summary>
        public string EMAIL { get; set; } //varchar 60 允許
        public string EMAIL_0 { get; set; }
        public string EMAIL_1 { get; set; }
        public string EMAIL_2 { get; set; }


        /// <summary>
        /// *配偶姓名 SPNAME
        /// </summary>
        public string SPNAME { get; set; } //nvarchar 60    
        /// <summary>
        /// *配偶身份證統一編號/外籍統一證號或護照號碼 SPIDN/SPIDNS
        /// </summary>
        public string SPIDN { get; set; } //varchar 20     
        /// <summary>
        /// *配偶出生年月日 SPBIRTH
        /// </summary>
        public DateTime? SPBIRTHDAY { get; set; } //date   允許

        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        public string SPBIRTHDAY_TW
        {
            get
            {
                return (SPBIRTHDAY.HasValue ? HelperUtil.DateTimeToTwString(SPBIRTHDAY.Value) : null);
            }
            set
            {
                if (SPBIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToTwString(SPBIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    SPBIRTHDAY = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 申請人出生年月日-西元年
        /// </summary>
        public string SPBIRTHDAY_AD
        {
            get
            {
                return (SPBIRTHDAY.HasValue ? HelperUtil.DateTimeToString(SPBIRTHDAY.Value) : null);
            }
            set
            {
                if (SPBIRTHDAY.HasValue)
                    value = HelperUtil.DateTimeToString(SPBIRTHDAY);
                if (!string.IsNullOrWhiteSpace(value))
                    SPBIRTHDAY = HelperUtil.TransToDateTime(value);
            }
        }


        /// <summary>
        /// *配偶電話 SPTEL
        /// </summary>
        public string SPTEL { get; set; } //varchar 30 允許
        public string SPTEL_0 { get; set; } //varchar 30 允許
        public string SPTEL_1 { get; set; } //varchar 30 允許
        public string SPTEL_2 { get; set; } //varchar 30 允許


        /// <summary>
        /// *配偶手機 SPMOBILE
        /// </summary>
        public string SPMOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 配偶E-MAIL SPEMAIL
        /// </summary>
        public string SPEMAIL { get; set; } //varchar 60 允許

        public string SPEMAIL_0 { get; set; }
        public string SPEMAIL_1 { get; set; }
        public string SPEMAIL_2 { get; set; }


        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ZIPCODE { get; set; } //varchar 5 允許
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ZIPCODE { get; set; } //varchar 5 允許
        public string H_ZIPCODE_TEXT { get; set; }
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 同戶籍地址
        /// </summary>
        public string H_EQUAL { get; set; } //varchar 1 允許

        public string MERGEYN { get; set; } //varchar 1 允許

        #region  上傳檔案

        /// <summary>
        /// 國民身分證影印本(正面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(正面) ")]
        public HttpPostedFileBase FILE_IDCNF { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(反面) ")]
        public HttpPostedFileBase FILE_IDCNB { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        [Display(Name = "人工生殖機構開立之不孕症診斷證明書")]
        public HttpPostedFileBase FILE_DISEASE { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件")]
        public HttpPostedFileBase FILE_LOWREC { get; set; } //varchar 20 允許

        /// <summary>
        ///  國民身分證影印本(正面) 
        /// </summary>
        public string FILE_IDCNF_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        public string FILE_IDCNB_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        public string FILE_DISEASE_TEXT { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        public string FILE_LOWREC_TEXT { get; set; } //varchar 20 允許

        #endregion

    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_010002DoneModel
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 件數
        /// </summary>
        public string Count { get; set; }

        /// <summary>
        /// 額外的訊息
        /// </summary>
        public string Msg { get; set; }
    }

}