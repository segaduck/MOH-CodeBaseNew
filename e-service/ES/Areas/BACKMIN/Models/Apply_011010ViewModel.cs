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
    public class Apply_011010ViewModel
    {
        public Apply_011010ViewModel()
        {
        }

        /// <summary>
        /// Form
        /// </summary>
        public Apply_011010FormModel Form { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Apply_011010FormModel Detail { get; set; }

    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011010FormModel : ApplyModel
    {
        /// <summary>
        /// 
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
        /// 單位類型
        /// </summary>
        [Display(Name = "單位類型")]
        [Required]
        public string UNIT_TYPE { get; set; }

        /// <summary>
        /// 單位名稱(私部門)
        /// 1.身心障礙福利機構/團體
        /// 2.老人及長期照顧福利機構/團體
        /// 3.兒少、婦女及家庭福利機構/團體
        /// 4.學校
        /// 5.矯正機關
        /// 6.社工師事務所及公會/協會/學會
        /// 7.其他(非屬上述類別)
        /// </summary>
        [Display(Name = "單位類型(私部門")]
        public string UNIT_SUBTYPE { get; set; }

        /// <summary>
        /// 單位名稱
        /// </summary>
        [Display(Name = "單位名稱")]
        [Required]
        public string UNIT_NAME { get; set; }

        /// <summary>
        /// 單位連絡人局處/部門
        /// </summary>
        [Display(Name = "單位連絡人局處/部門")]
        [Required]
        public string UNIT_DEPART { get; set; }

        /// <summary>
        /// 單位連絡人職稱
        /// </summary>
        [Display(Name = "單位連絡人職稱")]
        [Required]
        public string UNIT_TITLE { get; set; }

        /// <summary>
        /// 單位聯絡人姓名
        /// </summary>
        [Display(Name = "單位聯絡人姓名")]
        [Required]
        public string UNIT_CNAME { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        [Required]
        public string UNIT_TEL { get; set; }

        #region EMAIL

        /// <summary>
        /// EMAIL
        /// </summary>
        public string UNIT_MAIL { get; set; }

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
        /// 單位社工人員總數類型
        /// </summary>
        [Display(Name = "單位社工人員總數類型")]
        [Control(Mode=Control.Hidden)]
        public string CNT_TYPE { get; set; }

        /// <summary>
        /// 總會人數
        /// </summary>
        [Display(Name ="總會人數")]
        [Control(Mode = Control.Hidden)]
        public string CNT_A { get; set; }

        /// <summary>
        /// 分會人數
        /// </summary>
        [Display(Name ="分會人數")]
        [Control(Mode = Control.Hidden)]
        public string CNT_B { get; set; }

        /// <summary>
        /// 總會與分會加總人數
        /// </summary>
        [Display(Name ="總會與分會加總人數")]
        [Control(Mode = Control.Hidden)]
        public string CNT_C { get; set; }

        /// <summary>
        /// 單位社工人員總數類型
        /// </summary>
        [Display(Name = "單位社工人員總數類型")]
        public string CNT_D { get; set; }

        /// <summary>
        /// 績優社工獎推薦人數
        /// </summary>
        [Display(Name ="績優社工獎推薦人數")]
        [Required]
        public string CNT_E { get; set; }

        /// <summary>
        /// 績優社工督導獎推薦人數
        /// </summary>
        [Display(Name ="績優社工督導獎推薦人數")]
        [Required]
        public string CNT_F { get; set; }

        /// <summary>
        /// 資深敬業獎推薦人數
        /// </summary>
        [Display(Name ="資深敬業獎推薦人數")]
        [Required]
        public string CNT_G { get; set; }

        /// <summary>
        /// 特殊貢獻獎推薦人數
        /// </summary>
        [Display(Name ="特殊貢獻獎推薦人數")]
        [Required]
        public string CNT_H { get; set; }

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        [Required]
        public string MERGEYN { get; set; }

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
        /// 彙整表(EXCEL檔)
        /// </summary>
        [Display(Name = "彙整表(EXCEL檔)")]
        public string FILE_EXCLE { get; set; }
        public string FILE_EXCLE_TEXT { get; set; }

        /// <summary>
        /// 檢核表(PDF檔)
        /// </summary>
        [Display(Name = "檢核表(PDF檔)")]
        public string FILE_PDF { get; set; }
        public string FILE_PDF_TEXT { get; set; }

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

        public Apply_011010FileModel FileList { get; set; }

    }

    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_011010FileModel
    {
        public string APP_ID { get; set; }

        /// <summary>
        /// 彙整表(EXCEL檔)
        /// </summary>
        [Display(Name = "彙整表(EXCEL檔)")]
        public string FILE_EXCEL { get; set; }
        public string FILE_EXCEL_TEXT { get; set; }

        /// <summary>
        /// 檢核表(PDF檔)
        /// </summary>
        [Display(Name = "檢核表(PDF檔)")]
        public string FILE_PDF { get; set; }
        public string FILE_PDF_TEXT { get; set; }


        /// <summary>
        /// 服務經歷_清單
        /// </summary>
        public IList<Apply_011010SRVLSTModel> SRVLIST { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_011010FILEModel> FILE { get; set; }

    }

    /// <summary>
    /// 服務經歷
    /// </summary>
    public class Apply_011010SRVLSTModel : Apply_FileModel
    {
        /// <summary>
        /// 序號
        /// </summary>
        public string SEQ_NO { get; set; }

        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)
        /// </summary>
        [Display(Name = "推薦表(PDF檔)")]
        public string FILE_3 { get; set; }
        public string FILE_3_TEXT { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Apply_011010FILEModel : Apply_FileModel
    {
    }
}