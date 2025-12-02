using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq; 
using ES.Models.Entities;
using System.Web;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_001009 醫事人員資格英文求證
    /// </summary>
    public class Apply_001009ViewModel : Apply_001009Model
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
        /// 傳真號碼(全)
        /// </summary>
        public string FAX { get; set; }

        /// <summary>
        /// 傳真區碼
        /// </summary>
        public string FAX_SEC { get; set; }

        /// <summary>
        /// 傳真號碼
        /// </summary>
        public string FAX_NO { get; set; }

        /// <summary>
        /// 傳真分機
        /// </summary>
        public string FAX_EXT { get; set; }

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

        /// <summary>
        /// Mail Domain
        /// </summary>
        public string DOMAINList { get; set; }

        #region 地址

        public string FULL_ADDRESS { get; set; }
        public string CITY_CODE { get; set; }
        public string CITY_TEXT { get; set; }
        public string CITY_DETAIL { get; set; }

        #endregion

        /// <summary>
        /// 系統狀態
        /// </summary>
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 出生年月日 
        /// </summary>
        public string BIRTHDAY_AC { get; set; }

        /// 證書字號
        /// </summary>
        public string LIC_CD_TEXT { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string STATUS { get; set; }

        public string IsNotice { get; set; }

        /// <summary>
        /// 補件內容
        /// </summary>
        public string MAILBODY { get; set; }

        /// <summary>
        /// 申請案主檔
        /// </summary>
        public ApplyModel Apply { get; set; }

        /// <summary>
        /// 醫事人員證書日期
        /// </summary>
        public string CERT_APPROVED_DATE_AC { get; set; }

        /// <summary>
        /// 護照影本
        /// </summary>
        public HttpPostedFileBase FILE1 { get; set; }
        public string FILE1_NAME { get; set; }
        public string FILE1_CHECK { get; set; }

        /// <summary>
        /// 醫事人員或公共衛生師中文證書影本
        /// </summary>
        public HttpPostedFileBase FILE2 { get; set; }
        public string FILE2_NAME { get; set; }
        public string FILE2_CHECK { get; set; }

        /// <summary>
        /// 考照時之畢業證書影本
        /// </summary>
        public HttpPostedFileBase FILE3 { get; set; }
        public string FILE3_NAME { get; set; }
        public string FILE3_CHECK { get; set; }

        /// <summary>
        /// 考試及格證書影本
        /// </summary>
        public HttpPostedFileBase FILE4 { get; set; }
        public string FILE4_NAME { get; set; }
        public string FILE4_CHECK { get; set; }

        /// <summary>
        /// 對方機構求證表格
        /// </summary>
        public HttpPostedFileBase FILE5 { get; set; }
        public string FILE5_NAME { get; set; }
        public string FILE5_CHECK { get; set; }

        public string YEAR1 { get; set; }
        public string MONTH1 { get; set; }
        public string YEAR2 { get; set; }
        public string MONTH2 { get; set; }
        public AdminModel Admin { get; set; }

        public List<TblAPPLY_NOTICE> ApplyNoticeList { get; set; }
        public string NoticeList { get; set; }
        public  int? FREQUENCY { get; set; }
        public List<Apply_FileModel> FileList { get; set; }

        public string APPLY_CERT_CATE_TEXT { get; set; } 
        public string COUNTRY_TEXT { get; set; }
    }
}