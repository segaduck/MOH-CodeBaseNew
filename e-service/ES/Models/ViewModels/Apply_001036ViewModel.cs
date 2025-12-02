using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;

namespace ES.Models.ViewModels

{
    /// <summary>
    /// APPLY_001036 專科護理師證書補(換)發
    /// </summary>
    public class Apply_001036ViewModel : Apply_001036Model
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

        #endregion

        /// <summary>
        /// Mail Domain
        /// </summary>
        public string DOMAINList { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案
        /// </summary>
        public HttpPostedFileBase ATTACH_FILE { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf
        /// </summary>
        public string ATTACH_FILE_NAME { get; set; }

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

        /// 科別
        /// </summary>
        public string DIVISION_TEXT { get; set; }

        /// 補(換)發
        /// </summary>
        public string ACTION_TYPE_TEXT { get; set; }

        /// 證書字號
        /// </summary>
        public string LIC_CD_TEXT { get; set; }

        /// 補(換)發原因
        /// </summary>
        public string ACTION_RES_TEXT { get; set; }

        /// <summary>
        /// PreView
        /// </summary>
        public Apply_001036ViewModel PreView { get; set; }

        /// <summary>
        /// AuxView
        /// </summary>
        public Apply_001036ViewModel AuxView { get; set; }

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
        /// 補件件數
        /// </summary>
        public int AppDocCount { get; set; }

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
        /// Apply-申請時應繳費用
        /// </summary>
        public int? PAY_A_FEE { get; set; }

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
        public Apply_FileModel File { get; set; }

        public APPLY_PAY ApplyPay { get; set; }

        public string IsNotice { get; set; }

        /// <summary>
        /// 是否使用聯合信用中心
        /// </summary>
        public string ISEC { get; set; }
    }

}