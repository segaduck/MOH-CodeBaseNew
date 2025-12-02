using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using System.Web.Mvc;
using ES.Services;

namespace ES.Areas.Admin.Models
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
        /// FormModel
        /// </summary>
        public Apply_010002FormModel Detail { get; set; }

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
        /// 申辦日期西元YYYY/MM/DD
        /// </summary>
        public DateTime? APPLY_DATE { get; set; }

        /// <summary>
        /// 申辦日期-YYY/MM/DD-TW
        /// </summary>
        [Display(Name = "申辦日期")]
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
        /// 預計完成日期
        /// </summary>
        [Display(Name = "預計完成日期")]
        public DateTime? APP_EXT_DATE { get; set; }

        /// <summary>
        /// 預計完成日
        /// </summary>
        public string APP_EXT_DATE_TW
        {
            get
            {
                return (APP_EXT_DATE.HasValue ? HelperUtil.DateTimeToTwString(APP_EXT_DATE.Value) : null);
            }
            set
            {
                if (APP_EXT_DATE.HasValue)
                    value = HelperUtil.DateTimeToTwString(APP_EXT_DATE);
                if (!string.IsNullOrWhiteSpace(value))
                    APP_EXT_DATE = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 承辦單位
        /// </summary>
        public string ORG_NAME { get; set; } //nvarchar 60   

        /// <summary>
        /// *申請人
        /// </summary>
        [Display(Name = "申請人")]
        public string APNAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人身份證統一編號/外籍統一證號或護照號碼
        /// </summary>
        [Display(Name = "申請人身份證統一編號/外籍統一證號或護照號碼")]
        public string IDN { get; set; } //varchar 20     
        /// <summary>
        /// *出生年月日BIRTH 年 /月/日
        /// </summary>
        [Display(Name = "出生年月日")]
        public DateTime? BIRTHDAY { get; set; } //date   允許

        /// <summary>
        /// 出生年月日-YYY/MM/DD-TW
        /// </summary>
        [Display(Name = "出生年月日")]
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

        [Display(Name = "出生年月日")]
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
        [Display(Name = "申請人電話")]
        public string TEL { get; set; } //varchar 30 允許
        /// <summary>
        /// *申請人手機 MOBILE
        /// </summary>
        [Display(Name = "申請人手機")]
        public string MOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 申請人E-MAIL EMAIL
        /// </summary>
        [Display(Name = "申請人E-MAIL")]
        public string EMAIL { get; set; } //varchar 60 允許
        /// <summary>
        /// *配偶姓名 SPNAME
        /// </summary>
        [Display(Name = "配偶姓名")]
        public string SPNAME { get; set; } //nvarchar 60    
        /// <summary>
        /// *配偶身份證統一編號/外籍統一證號或護照號碼 SPIDN/SPIDNS
        /// </summary>
        [Display(Name = "配偶身份證統一編號/外籍統一證號或護照號碼")]
        public string SPIDN { get; set; } //varchar 20     
        /// <summary>
        /// *配偶出生年月日 SPBIRTH
        /// </summary>
        [Display(Name = "配偶出生年月日")]
        public DateTime? SPBIRTHDAY { get; set; } //date   允許

        /// <summary>
        /// *配偶出生年月日 SPBIRTH-YYY/MM/DD-TW
        /// </summary>
        [Display(Name = "配偶出生年月日")]
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


        [Display(Name = "配偶出生年月日")]
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
        [Display(Name = "配偶電話")]
        public string SPTEL { get; set; } //varchar 30 允許
        /// <summary>
        /// *配偶手機 SPMOBILE
        /// </summary>
        [Display(Name = "配偶手機")]
        public string SPMOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 配偶E-MAIL SPEMAIL
        /// </summary>
        [Display(Name = "配偶E-MAIL")]
        public string SPEMAIL { get; set; } //varchar 60 允許
        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        [Display(Name = "現居地")]
        public string C_ZIPCODE { get; set; } //varchar 5 允許
        public string C_ZIPCODE_TEXT { get; set; } //varchar 5 允許
        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        [Display(Name = "現居地")]
        public string C_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        public string H_ZIPCODE { get; set; } //varchar 5 允許
        public string H_ZIPCODE_TEXT { get; set; } //varchar 5 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        public string H_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 同戶籍地址
        /// </summary>
        [Display(Name = "同戶籍地址")]
        public string H_EQUAL { get; set; } //varchar 1 允許

        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "國民身分證影印本正反面-同意自「個人化資料自主運用(MyData)平臺」取得資料(需使用自然人憑證驗證身分)")]
        public string MYDATA_GET1 { get; set; } //varchar 30 允許
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "國民身分證影印本正反面-同意自「個人化資料自主運用(MyData)平臺」取得資料(需使用自然人憑證驗證身分)")]
        public bool MYDATA_GET_FG1 { get; set; }
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件-同意自「個人化資料自主運用(MyData)平臺」取得資料(需使用自然人憑證驗證身分)")]
        public string MYDATA_GET2 { get; set; } //varchar 30 允許
        /// <summary>
        /// 上傳檔案或民眾授權同意自Mydata平台取得資料 
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件-同意自「個人化資料自主運用(MyData)平臺」取得資料(需使用自然人憑證驗證身分)")]
        public bool MYDATA_GET_FG2 { get; set; }

        /// <summary>
        /// MyData ReturnUrl 帶回狀態碼
        /// </summary>
        public string MYDATA_RTN_CODE { get; set; }

        /// <summary>
        /// MyData ReturnUrl 帶回狀態碼說明文字
        /// </summary>
        public string MYDATA_RTN_CODE_DESC { get; set; }

        /// <summary>
        /// MyData 交易回應時間
        /// </summary>
        public DateTime? MYDATA_TX_TIME { get; set; }

        /// <summary>
        /// MyData 個人戶籍資料 (json)
        /// </summary>
        public string MYDATA_IDCN { get; set; }

        /// <summary>
        /// MyData 低收入戶及中低收入戶證明 (json)
        /// </summary>
        public string MYDATA_LOWREC { get; set; }

        /// <summary>
        /// MyData 交易異常訊息(成功時為空白)
        /// </summary>
        public string MYDATA_TX_RESULT_MSG { get; set; }

        /// <summary>
        /// 佐證文件檔案上傳--佐證文件採合併檔案 是 否
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; } //varchar 1 允許
        /// <summary>
        /// 國民身分證影印本(正面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(正面)")]
        public string FILE_IDCNF { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(反面)")]
        public string FILE_IDCNB { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        [Display(Name = "人工生殖機構開立之不孕症診斷證明書")]
        public string FILE_DISEASE { get; set; } //varchar 20 允許

        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        [Display(Name = "中低收入戶或低收入戶證明文件")]
        public string FILE_LOWREC { get; set; } //varchar 20 允許

        /// <summary>
        /// 補件勾選
        /// </summary>
        public string FileCheck { get; set; }

        /// <summary>
        /// 補件內容
        /// </summary>
        [Display(Name = "補件內容")]
        public string NOTE { get; set; }

        /// <summary>
        /// 補件勾選內容
        /// </summary>
        [Display(Name = "補件勾選內容")]
        public string chkNotice { get; set; }

        ///<summary>
        /// 申辦項目
        /// </summary>
        [Display(Name = "申辦項目")]
        public string APP_NAME { get; set; }

        /// <summary>
        /// 案件進度
        /// </summary>
        [Display(Name = "案件進度")]
        public string FLOW_CD_TEXT { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        [Display(Name = "交易日期")]
        public string PAY_ACT_TIME { get; set; }
        /// <summary>
        /// 繳費日期
        /// </summary>
        [Display(Name = "繳費日期")]
        public DateTime? PAY_EXT_TIME { get; set; }
        public string PAY_EXT_TIME_AD
        {
            get
            {
                // YYYYMMDD 回傳給系統
                return (PAY_EXT_TIME.HasValue ? HelperUtil.DateTimeToString(PAY_EXT_TIME) : null);
            }
            set
            {
                // YYYMMDD 民國年 使用者看到
                if (PAY_EXT_TIME.HasValue)
                    value = HelperUtil.DateTimeToString(PAY_EXT_TIME);
                if (!string.IsNullOrWhiteSpace(value))
                    PAY_EXT_TIME = HelperUtil.TransToDateTime(value);
            }
        }

        /// <summary>
        /// 繳費狀態YN
        /// </summary>
        [Display(Name = "繳費狀態")]
        public string PAY_STATUS { get; set; }

        /// <summary>
        /// 繳費狀態 bool
        /// </summary>
        public bool IS_PAY_STATUS
        {
            get
            {
                return ("Y".Equals(!string.IsNullOrEmpty(this.PAY_STATUS) ? this.PAY_STATUS.ToUpper() : "N") ? true : false);
            }
            set
            {
                //設定
                PAY_STATUS = (value ? "Y" : "N");
                //if (string.IsNullOrEmpty(PAY_STATUS))
                //{
                //    PAY_STATUS = (value ? "Y" : "N");
                //}
                //else
                //{
                //    value = "Y".Equals(!string.IsNullOrEmpty(this.PAY_STATUS) ? this.PAY_STATUS.ToUpper() : "N") ? true : false;
                //}
            }
        }

        /// <summary>
        /// 系統狀態
        /// </summary>
        [Display(Name = "系統狀態")]
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        [Display(Name = "承辦人姓名")]
        public string PRO_NAM { get; set; }

        public Apply_010002FileModel FileList { get; set; }

    } // Apply_010002FormModel

    /// <summary>
    /// 補件表單
    /// </summary>
    public class Apply_010002AppDocModel : ApplyModel
    {
        public DateTime? APPLY_DATE { get; set; }

        /// <summary>
        /// 承辦單位
        /// </summary>
        public string ORG_NAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人
        /// </summary>
        public string APNAME { get; set; } //nvarchar 60     
        /// <summary>
        /// *申請人身份證統一編號/外籍統一證號或護照號碼
        /// </summary>
        public string IDN { get; set; } //varchar 20     
        /// <summary>
        /// *出生年月日BIRTH 年 /月/日
        /// </summary>
        public DateTime? BIRTHDAY { get; set; } //date   允許
        /// <summary>
        /// *申請人電話 TEL
        /// </summary>
        public string TEL { get; set; } //varchar 30 允許
        /// <summary>
        /// *申請人手機 MOBILE
        /// </summary>
        public string MOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 申請人E-MAIL EMAIL
        /// </summary>
        public string EMAIL { get; set; } //varchar 60 允許
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
        /// *配偶電話 SPTEL
        /// </summary>
        public string SPTEL { get; set; } //varchar 30 允許
        /// <summary>
        /// *配偶手機 SPMOBILE
        /// </summary>
        public string SPMOBILE { get; set; } //varchar 30 允許
        /// <summary>
        /// 配偶E-MAIL SPEMAIL
        /// </summary>
        public string SPEMAIL { get; set; } //varchar 60 允許
        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ZIPCODE { get; set; } //varchar 5 允許
        /// <summary>
        /// *現居地-通訊地
        /// </summary>
        public string C_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ZIPCODE { get; set; } //varchar 5 允許
        /// <summary>
        /// *戶籍地址
        /// </summary>
        public string H_ADDR { get; set; } //nvarchar 300 允許
        /// <summary>
        /// 同戶籍地址
        /// </summary>
        public string H_EQUAL { get; set; } //varchar 1 允許
        /// <summary>
        /// 佐證文件檔案上傳--佐證文件採合併檔案是 否
        /// </summary>
        public string MERGEYN { get; set; } //varchar 1 允許
        /// <summary>
        /// 國民身分證影印本(正面) 
        /// </summary>
        public string FILE_IDCNF { get; set; } //varchar 20 允許
        /// <summary>
        /// 國民身分證影印本(反面) 
        /// </summary>
        public string FILE_IDCNB { get; set; } //varchar 20 允許
        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        public string FILE_DISEASE { get; set; } //varchar 20 允許
        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary>
        public string FILE_LOWREC { get; set; } //varchar 20 允許

    }

    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_010002FileModel
    {
        public string APP_ID { get; set; }

        /// <summary>
        /// 國民身分證影印本(正面) 
        /// </summary>
        [Display(Name = "國民身分證影印本(正面) ")]
        public string FILE_IDCNF { get; set; }
        public string FILE_IDCNF_TEXT { get; set; }

        /// <summary>
        ///  國民身分證影印本(反面) 
        /// </summary>
        [Display(Name = " 國民身分證影印本(反面) ")]
        public string FILE_IDCNB { get; set; }
        public string FILE_IDCNB_TEXT { get; set; }

        /// <summary>
        /// 人工生殖機構開立之不孕症診斷證明書
        /// </summary>
        [Display(Name = "人工生殖機構開立之不孕症診斷證明書")]
        public string FILE_DISEASE { get; set; }
        public string FILE_DISEASE_TEXT { get; set; }

        /// <summary>
        /// 中低收入戶或低收入戶證明文件
        /// </summary
        [Display(Name = "中低收入戶或低收入戶證明文件")]
        public string FILE_LOWREC { get; set; }
        public string FILE_LOWREC_TEXT { get; set; }

    }


}