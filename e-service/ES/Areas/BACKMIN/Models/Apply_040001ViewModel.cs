using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using System.Web.Mvc;
using ES.Services;
using ES.Models;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel
    /// </summary>
    public class Apply_040001ViewModel
    {
        public Apply_040001ViewModel()
        {
        }

        /// <summary>
        /// Form
        /// </summary>
        public Apply_040001FormModel Form { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Apply_040001FormModel Detail { get; set; }

    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_040001FormModel : ApplyModel
    {
        /// <summary>
        /// 申辦日期西元YYYY/MM/DD
        /// </summary>
        [Display(Name = "申辦日期")]
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
        /// 承辦人姓名
        /// </summary>
        [Display(Name = "承辦人姓名")]
        public string PRO_NAM { get; set; }

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
        /// 系統狀態
        /// </summary>
        [Display(Name = "系統狀態")]
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [Display(Name = "案件備註")]
        public string NOTE { get; set; }

        #region EMAIL

        [Display(Name = "訴願人E-MAIL")]
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
        [Display(Name = "E-MAIL")]
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
        /// 訴願人姓名
        /// </summary>
        [Display(Name = "訴願人中文姓名")]
        public string NAME { get; set; }

        /// <summary>
        /// 訴願人出生年月日_字串
        /// </summary>
        [Display(Name = "訴願人出生年月日")]
        public string BIRTHDAY_STR { get; set; }
        public DateTime? BIRTHDAY { get; set; }
        /// <summary>
        /// 訴願人身分證明文件字號
        /// </summary>
        [Display(Name = "訴願人身分證明文件字號")]
        public string IDN { get; set; }

        /// <summary>
        /// 訴願人訴願人連絡電話
        /// </summary>
        [Display(Name = "訴願人連絡電話")]
        public string H_TEL { get; set; }
        public string CNT_TEL { get; set; }
        /// <summary>
        /// 訴願人手機號碼
        /// </summary>
        [Display(Name = "訴願人手機號碼")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 訴願人通訊地址
        /// </summary>
        [Display(Name = "訴願人住所或居所(含郵遞區號)")]
        public string C_ZIPCODE { get; set; }
        public string ADDR_CODE { get; set; }
        /// <summary>
        /// 訴願人住所或居所
        /// </summary>
        [Display(Name = "訴願人住所或居所")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 訴願人住所或居所
        /// </summary>
        [Display(Name = "訴願人住所或居所")]
        public string C_ADDR { get; set; }
        public string ADDR { get; set; }
        /// <summary>
        /// 代表人姓名
        /// </summary>
        [Display(Name = "代表人姓名")]
        public string CHR_NAME { get; set; }

        /// <summary>
        /// 代表人出生年月日_字串
        /// </summary>
        [Display(Name = "代表人出生年月日")]
        public string CHR_BIRTH_STR { get; set; }
        public DateTime? CHR_BIRTH { get; set; }
        /// <summary>
        /// 代表人身分證明文件字號
        /// </summary>
        [Display(Name = "代表人身分證明文件字號")]
        public string CHR_IDN { get; set; }

        /// <summary>
        /// 代表人連絡電話
        /// </summary>
        [Display(Name = "代表人連絡電話")]
        public string CHR_TEL { get; set; }

        /// <summary>
        /// 代表人手機號碼
        /// </summary>
        [Display(Name = "代表人手機號碼")]
        public string CHR_MOBILE { get; set; }

        /// <summary>
        /// 代表人通訊地址
        /// </summary>
        [Display(Name = "代表人住所或居所(含郵遞區號)")]
        public string CHR_ZIPCODE { get; set; }
        public string CHR_ADDR_CODE { get; set; }
        /// <summary>
        /// 代表人住所或居所
        /// </summary>
        [Display(Name = "代表人住所或居所")]
        public string CHR_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 代表人住所或居所
        /// </summary>
        [Display(Name = "代表人住所或居所")]
        public string CHR_ADDR { get; set; }

        /// <summary>
        /// 代理人姓名
        /// </summary>
        [Display(Name = "代理人姓名")]
        public string R_NAME { get; set; }

        /// <summary>
        /// 代理人出生年月日_字串
        /// </summary>
        [Display(Name = "代理人出生年月日")]
        public string R_BIRTH_STR { get; set; }
        public DateTime? R_BIRTH { get; set; }
        /// <summary>
        /// 代理人身分證明文件字號
        /// </summary>
        [Display(Name = "代理人身分證明文件字號")]
        public string R_IDN { get; set; }

        /// <summary>
        /// 代理人連絡電話
        /// </summary>
        [Display(Name = "代理人連絡電話")]
        public string R_TEL { get; set; }

        /// <summary>
        /// 代理人手機號碼
        /// </summary>
        [Display(Name = "代理人手機號碼")]
        public string R_MOBILE { get; set; }

        /// <summary>
        /// 代理人通訊地址
        /// </summary>
        [Display(Name = "代理人住所或居所(含郵遞區號)")]
        public string R_ZIPCODE { get; set; }
        public string R_ADDR_CODE { get; set; }
        /// <summary>
        /// 代理人住所或居所
        /// </summary>
        [Display(Name = "代理人住所或居所")]
        public string R_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 代理人住所或居所
        /// </summary>
        [Display(Name = "代理人住所或居所")]
        public string R_ADDR { get; set; }
        /// <summary>
        /// 原行政處分機關
        /// </summary>
        [Display(Name = "原行政處分機關")]
        public string ORG_NAME { get; set; }
        /// <summary>
        /// 訴願人收受或知悉行政處分之年月日
        /// </summary>
        [Display(Name = "訴願人收受或知悉行政處分之年月日")]
        public string ORG_DATE_STR { get; set; }
        public DateTime? ORG_DATE { get; set; }
        /// <summary>
        /// 訴願請求
        /// </summary>
        [Display(Name = "訴願請求")]
        public string ORG_MEMO { get; set; }
        /// <summary>
        /// 事實與理由
        /// </summary>
        [Display(Name = "事實與理由")]
        public string ORG_FACT { get; set; }


        public Apply_040001FileModel FileList { get; set; }

    }

    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_040001FileModel
    {
        public string APP_ID { get; set; }

        /// <summary>
        /// 服務經歷_清單
        /// </summary>
        public IList<Apply_040001SRVLSTModel> SRVLIST { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_040001FILEModel> FILE { get; set; }

    }

    /// <summary>
    /// 服務經歷
    /// </summary>
    public class Apply_040001SRVLSTModel : Apply_FileModel
    {
        /// <summary>
        /// 序號
        /// </summary>
        public string SEQ_NO { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "佐證文件")]
        public string FILE_1 { get; set; }
        public string FILE_1_TEXT { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Apply_040001FILEModel : Apply_FileModel
    {
    }
}