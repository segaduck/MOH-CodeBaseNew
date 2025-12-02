using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ES.Commons;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using System.Globalization;

namespace ES.Areas.BACKMIN.Models
{
    public class Apply_005005ViewModel
    {
        public Apply_005005ViewModel()
        {
            this.Form = new Apply_005005FormModel();
            this.Detail = new Apply_005005FormModel();
        }

        public Apply_005005FormModel Form { get; set; }

        public Apply_005005FormModel Detail { get; set; }

        public Apply_005005PreviewModel Preview { get; set; }
    }

    public class Apply_005005FormModel : ApplyModel
    {
        /// <summary>
        /// 申請狀態
        /// </summary>
        [Display(Name = "申請狀態")]
        public string APPLY_STATUS { get; set; }

        /// <summary>
        /// 申請日期(民國)
        /// </summary>
        public string APP_TIME_TW
        {
            get
            {
                if (string.IsNullOrEmpty(APP_TIME.ToString()))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(APP_TIME);
                }
            }
            set { APP_TIME = HelperUtil.TransTwToDateTime(value); }
        }

        /// <summary>
        /// 申請份數
        /// </summary>
        public string PAYCOUNT { get; set; }

        /// <summary>
        /// 繳費金額
        /// </summary>
        [Display(Name = "繳費金額")]
        public int PAYAMOUNT { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        [Display(Name = "公司名稱")]
        public string NAME { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Display(Name = "聯絡人姓名")]
        public string CNT_NAME { get; set; }

        /// <summary>
        /// 連絡電話(區域號碼)
        /// </summary>
        [Display(Name = "連絡電話區域號碼")]
        public string TEL_BEFORE { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL_AFTER { get; set; }

        /// <summary>
        /// 分機
        /// </summary>
        [Display(Name = "連絡電話分機")]
        public string TEL_Extension { get; set; }

        /// <summary>
        /// 傳真電話(區域號碼)
        /// </summary>
        public string FAX_BEFORE { get; set; }

        /// <summary>
        /// 傳真電話
        /// </summary>
        public string FAX_AFTER { get; set; }

        /// <summary>
        /// 傳真分機
        /// </summary>
        public string FAX_Extension { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_BEFORE { get; set; }

        /// <summary>
        /// EMAIL 其他MAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_CUSTOM { get; set; }

        /// <summary>
        /// EMAIL網域
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_ADDR { get; set; }

        /// <summary>
        /// EMAIL網域
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_ADDR_TEXT { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 外銷國家
        /// </summary>
        [Display(Name = "外銷國家")]
        public string IMP_COUNTRY { get; set; }

        /// <summary>
        /// 製造廠許可編號
        /// </summary>
        [Display(Name = "製造廠許可編號")]
        public string LIC_NUM { get; set; }

        /// <summary>
        /// 製造廠名稱
        /// </summary>
        [Display(Name = "製造廠名稱")]
        public string MF_CNT_NAME { get; set; }

        /// <summary>
        /// 製造廠地址
        /// </summary>
        [Display(Name = "製造廠地址")]
        public string MF_ADDR { get; set; }

        /// <summary>
        /// 查廠日期
        /// </summary>
        [Display(Name = "最近一次GMP查廠日期")]
        public string ISSUE_DATE { get; set; }

        public string ISSUE_DATE_TW
        {
            get
            {
                if (string.IsNullOrEmpty(ISSUE_DATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(ISSUE_DATE));
                }
            }
            set { ISSUE_DATE = HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value)); }
        }

        /// <summary>
        /// GMP有效日期
        /// </summary>
        [Display(Name = "GMP有效日期")]
        public string EXPIR_DATE { get; set; }

        public string EXPIR_DATE_TW
        {
            get
            {
                if (string.IsNullOrEmpty(EXPIR_DATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(EXPIR_DATE));
                }
            }
            set { EXPIR_DATE = HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value)); }
        }

        /// <summary>
        /// 預計完成日
        /// </summary>
        public string APP_EXT_DATE_TW
        {
            get
            {
                if (string.IsNullOrEmpty(APP_EXT_DATE.ToString()))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(APP_EXT_DATE);
                }
            }
            set { APP_EXT_DATE = HelperUtil.TransTwToDateTime(value); }
        }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        public string RADIOYN { get; set; }

        /// <summary>
        /// 本部核發藥物製造或展延許可函影本
        /// </summary>
        public HttpPostedFileBase File_1 { get; set; }

        [Display(Name = "本部核發藥物製造或展延許可函影本")]
        public string Name_File_1 { get; set; }

        public string Name_File_1_TEXT { get; set; }

        /// <summary>
        /// 製造業藥商許可執照影本
        /// </summary>
        public HttpPostedFileBase File_2 { get; set; }

        [Display(Name = "製造業藥商許可執照影本")]
        public string Name_File_2 { get; set; }

        public string Name_File_2_TEXT { get; set; }

        /// <summary>
        /// 補件狀態
        /// </summary>
        public string DOCYN { get; set; }

        /// <summary>
        /// 案件狀態
        /// </summary>
        public string CODE_CD { get; set; }

        /// <summary>
        /// 案件是否鎖定
        /// </summary>
        public bool IS_CASE_LOCK { get; set; }

        /// <summary>
        /// 繳費日期
        /// </summary>
        public string PAY_ACT_TIME { get; set; }

        /// <summary>
        /// 繳費狀態
        /// </summary>
        public string PAY_STATUS { get; set; }

        /// <summary>
        /// 繳費狀態YN
        /// </summary>
        public bool IS_PAY_STATUS
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.PAY_STATUS) ? this.PAY_STATUS.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    PAY_STATUS = "Y";
                }
                else
                {
                    PAY_STATUS = "N";
                }
            }
        }

        /// <summary>
        /// 案件承辦人姓名
        /// </summary>
        public string ADMIN_NAME { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        public string COMP_NAME { get; set; }

        /// <summary>
        /// 申辦份數
        /// </summary>
        public string COPIES { get; set; }

        /// <summary>
        /// 當前案件進度
        /// </summary>
        public string CODE_CD_TEXT { get; set; }

        [Display(Name ="公文日期")]
        public string MOHW_CASE_DATE { get; set; }
    }

    /// <summary>
    /// 檔案預覽
    /// </summary>
    public class Apply_005005PreviewModel : ApplyModel
    {
        public string FILENAME { get; set; }

        public string APP_ID { get; set; }

        public string FILE_NO { get; set; }

        public string SRC_NO { get; set; }
    }
}