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
    /// APPLY_001037 醫事人員請領無懲戒紀錄證明申請書
    /// </summary>
    public class Apply_001037ViewModel : Apply_001037Model
    {
        /// <summary>
        /// 申請人
        /// </summary>
        public string APPLY_NAME { get; set; }

        /// <summary>
        /// 申請人身份證字號
        /// </summary>
        public string APPLY_PID { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public string APPLY_DATE { get; set; }

        /// <summary>
        /// 預計完成日期
        /// </summary>
        public string APP_EXT_DATE { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        public string MOBILE { get; set; }

        /// <summary>
        /// 電話號碼(全)
        /// </summary>
        public string TEL { get; set; }

        /// <summary>
        /// 電話區碼
        /// </summary>
        public string TEL_SEC { get; set; }

        /// <summary>
        /// 電話號碼
        /// </summary>
        public string TEL_NO { get; set; }

        /// <summary>
        /// 電話分機
        /// </summary>
        public string TEL_EXT { get; set; }

        /// <summary>
        /// Mail
        /// </summary>
        public string MAIL { get; set; }

        /// <summary>
        /// Mail帳號
        /// </summary>
        public string MAIL_ACCOUNT { get; set; }

        /// <summary>
        /// Mail Domain
        /// </summary>
        public string MAIL_DOMAIN { get; set; }

        #region 地址

        public string FULL_ADDRESS { get; set; }
        public string CITY_CODE { get; set; }
        public string CITY_TEXT { get; set; }
        public string CITY_DETAIL { get; set; }

        public string MAIL_FULL_ADDRESS { get; set; }
        public string MAIL_CITY_CODE { get; set; }
        public string MAIL_CITY_TEXT { get; set; }
        public string MAIL_CITY_DETAIL { get; set; }

        #endregion

        /// <summary>
        /// Mail Domain
        /// </summary>
        public string DOMAINList { get; set; }

        /// <summary>
        /// 護照影本電子檔
        /// </summary>
        public HttpPostedFileBase FILE1 { get; set; }
        public string FILE1_NAME { get; set; }
        public string FILE1_CHECK { get; set; }

        /// <summary>
        /// 醫事人員中文證書電子檔
        /// </summary>
        public HttpPostedFileBase FILE2 { get; set; }
        public string FILE2_NAME { get; set; }
        public string FILE2_CHECK { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案
        /// </summary>
        public HttpPostedFileBase FILE3 { get; set; }
        public string FILE3_NAME { get; set; }
        public string FILE3_CHECK { get; set; }

        /// <summary>
        /// 系統狀態
        /// </summary>
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 出生年月日 
        /// </summary>
        public string BIRTHDAY_AC { get; set; }

        /// <summary>
        /// 核發日期
        /// </summary>
        public string ISSUE_DATE_AC { get; set; }


        /// 證書字號
        /// </summary>
        public string LIC_CD_TEXT { get; set; }

        /// <summary>
        /// 申請案主檔
        /// </summary>
        public ApplyModel Apply { get; set; }

        /// <summary>
        /// Field notice columns
        /// </summary>
        public string FieldList { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string STATUS { get; set; }

        /// <summary>
        /// Apply-信用卡身分證字號
        /// </summary>
        public string CARD_IDN { get; set; }

        /// <summary>
        /// Apply-申請需繳費(B)
        /// </summary>
        public string PAY_POINT { get; set; }

        /// <summary>
        /// Apply-繳費方式
        /// </summary>
        public string PAY_METHOD { get; set; }

        /// <summary>
        /// CLIENT_IP
        /// </summary>
        public string CLIENT_IP { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string SessionTransactionKey { get; set; }

        public string TempMessage { get; set; }

        /// <summary>
        /// 申請案主檔
        /// </summary>
        public AdminModel Admin { get; set; }

        /// <summary>
        /// 申請案件附檔
        /// </summary>
        public List<Apply_FileModel> FileList { get; set; }
        public List<TblAPPLY_NOTICE> ApplyNoticeList { get; set; }
        public string NoticeList { get; set; }
        public int? FREQUENCY { get; set; }

        public APPLY_PAY ApplyPay { get; set; }

        public string IsNotice { get; set; }

        /// <summary>
        /// 補件內容
        /// </summary>
        public string MAILBODY { get; set; }

        /// 證書類別
        /// </summary>
        public string DIVISION { get; set; }
        public string DIVISION_TEXT { get; set; }

        /// <summary>
        /// PreView
        /// </summary>
        public Apply_001037ViewModel PreView { get; set; }

        /// <summary>
        /// AuxView
        /// </summary>
        public Apply_001037ViewModel AuxView { get; set; }

        public string REASON_1_TEXT { get; set; }
        public string REASON_2_TEXT { get; set; }
        public string REASON_3_TEXT { get; set; }
        public string REASON_4_TEXT { get; set; }
        public string REASON_5_TEXT { get; set; }
        public string COUNTRY_TEXT { get; set; }
        public int AppDocCount { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// 新增時間
        /// </summary>
        public string PAY_ACT_TIME_AC { get; set; }

        /// <summary>
        /// 繳費日期
        /// </summary>
        public string PAY_EXT_TIME_AC { get; set; }

        /// <summary>
        /// 異動時間
        /// </summary>
        public string PAY_INC_TIME_AC { get; set; }

        /// <summary>
        /// 繳費方式
        /// </summary>
        public string PAY_METHOD_NAME { get; set; }

        /// <summary>
        /// 是否已繳費
        /// </summary>
        public bool IsPay { get; set; }

        public int? PAY_MONEY { get; set; }

        public string MAIL_DATE_AC { get; set; }

    }
}