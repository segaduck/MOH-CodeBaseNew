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
    public class Apply_005004ViewModel
    {
        public Apply_005004ViewModel()
        {
            this.Form = new Apply_005004FormModel();
            this.Detail = new Apply_005004FormModel();
        }

        public Apply_005004FormModel Form { get; set; }

        public Apply_005004FormModel Detail { get; set; }

        public Apply_005004PreviewModel Preview { get; set; }
    }

    public class Apply_005004FormModel : ApplyModel
    {
        /// <summary>
        /// 申請狀態
        /// </summary>
        [Display(Name = "申請狀態")]
        public string APPLY_STATUS { get; set; }

        /// <summary>
        /// 申請類別
        /// </summary>
        [Display(Name = "申請類別")]
        public string APPLY_TYPE { get; set; }

        /// <summary>
        /// 製造廠地址
        /// </summary>
        [Display(Name = "製造廠地址")]
        public string MF_ADDR { get; set; }

        #region 地址
        [Display(Name = "製造廠地址")]
        public string TAX_ORG_CITY_CODE { get; set; }

        [Display(Name = "製造廠地址區域")]
        public string TAX_ORG_CITY_TEXT { get; set; }

        [Display(Name = "製造廠地址")]
        public string TAX_ORG_CITY_DETAIL { get; set; }
        #endregion 地址


        /// <summary>
        /// 製造廠名稱
        /// </summary>
        [Display(Name = "製造廠名稱")]
        public string NAME { get; set; }

        /// <summary>
        /// 製造廠許可編號
        /// </summary>
        [Display(Name = "製造廠許可編號")]
        public string LIC_NUM { get; set; }

        /// <summary>
        /// 工廠登記證字號
        /// </summary>
        [Display(Name = "工廠登記證字號")]
        public string FRC_NUM { get; set; }

        /// <summary>
        /// 核備函(最近一期查廠之核備函公文)
        /// </summary>
        [Display(Name = "核備函(最近一期查廠之核備函公文)")]
        public string ATTACH_1 { get; set; }

        /// <summary>
        /// 藥品優良製造證明書申請表用印之掃描檔
        /// </summary>
        [Display(Name = "藥品優良製造證明書申請表用印之掃描檔")]
        public string ATTACH_2 { get; set; }

        /// <summary>
        /// 本部核發藥物製造或展延許可函影本
        /// </summary>
        [Display(Name = "本部核發藥物製造或展延許可函影本")]
        public string ATTACH_3 { get; set; }

        /// <summary>
        /// 製造業藥商許可執照影本
        /// </summary>
        [Display(Name = "製造業藥商許可執照影本")]
        public string ATTACH_4 { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        [Display(Name = "公司名稱")]
        public string COMP_NAME { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 藥商許可執照字號
        /// </summary>
        [Display(Name = "藥商許可執照字號")]
        public string PL_CD { get; set; }

        /// <summary>
        /// 藥商許可執照字號
        /// </summary>
        [Display(Name = "第")]
        public string PL_Num { get; set; }

        /// <summary>
        /// 監製藥師
        /// </summary>
        [Display(Name = "監製藥師")]
        public string PP_NAME { get; set; }

        /// <summary>
        /// 原料藥勾選
        /// </summary>
        [Display(Name = "原料藥勾選")]
        public string TRA_CHECK { get; set; }

        /// <summary>
        /// 製劑勾選
        /// </summary>
        [Display(Name = "製劑勾選")]
        public string CON_CHECK { get; set; }

        /// <summary>
        /// 查廠日期
        /// </summary>
        [Display(Name = "查廠日期")]
        public string ISSUE_DATE { get; set; }

        [Display(Name = "查廠日期")]
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
            set
            {
                ISSUE_DATE = HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value));
            }
        }

        /// <summary>
        /// 申請日期(民國)
        /// </summary>
        [Display(Name = "申請日期")]
        public string APP_TIME_TW {
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
            set { APP_TIME = Convert.ToDateTime(HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value))); }
        }

        /// <summary>
        /// 連絡電話(區域號碼)
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL_BEFORE { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL_AFTER { get; set; }

        /// <summary>
        /// 分機
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL_Extension { get; set; }

        /// <summary>
        /// 傳真電話(區域號碼)
        /// </summary>
        [Display(Name = "傳真電話")]
        public string FAX_BEFORE { get; set; }

        /// <summary>
        /// 傳真電話
        /// </summary>
        [Display(Name = "傳真電話")]
        public string FAX_AFTER { get; set; }

        /// <summary>
        /// 傳真分機
        /// </summary>
        [Display(Name = "傳真分機")]
        public string FAX_Extension { get; set; }

        /// <summary>
        /// 預計完成日(民國)
        /// </summary>
        [Display(Name = "預計完成日")]
        public string APP_EXT_DATE_TW
        {
            get
            {
                if (string.IsNullOrEmpty(APP_EXT_DATE.TONotNullString()))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(APP_EXT_DATE);
                }
            }
            set { APP_EXT_DATE = Convert.ToDateTime(HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value))); }
        }

        /// <summary>
        /// 承辦人
        /// </summary>
        [Display(Name = "承辦人")]
        public string ADMIN_NAME { get; set; }

        /// <summary>
        /// 負責人姓名
        /// </summary>
        [Display(Name = "負責人姓名")]
        public string CHR_NAME { get; set; }

        /// <summary>
        /// 負責人身分證字號
        /// </summary>
        [Display(Name = "負責人身分證字號")]
        public string IDN { get; set; }
        
        /// <summary>
        /// 繳費日期
        /// </summary>
        [Display(Name = "繳費日期")]
        public string PAY_ACT_TIME { get; set; }

        /// <summary>
        /// 繳費狀態
        /// </summary>
        [Display(Name = "繳費狀態")]
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
        /// 原料藥
        /// </summary>
        public bool IS_TRA_CHECK
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.TRA_CHECK) ? this.TRA_CHECK.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    TRA_CHECK = "Y";
                }
                else
                {
                    TRA_CHECK = "N";
                }
            }
        }

        /// <summary>
        /// 製劑
        /// </summary>
        public bool IS_CON_CHECK
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.CON_CHECK) ? this.CON_CHECK.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    CON_CHECK = "Y";
                }
                else
                {
                    CON_CHECK = "N";
                }
            }
        }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string RADIOYN { get; set; }

        /// <summary>
        /// 最近一次GMP查廠日期(證明文件)
        /// </summary>
        [Display(Name = "最近一次GMP查廠日期(證明文件)")]
        public string FILE1 { get; set; }
        public string FILE1_TEXT { get; set; }

        /// <summary>
        /// 藥品優良製造證明書申請表用印之掃描檔
        /// </summary>
        [Display(Name = "藥品優良製造證明書申請表用印之掃描檔")]
        public string FILE2 { get; set; }
        public string FILE2_TEXT { get; set; }

        /// <summary>
        /// 本部核發藥物製造或展延許可函影本
        /// </summary>
        [Display(Name = "本部核發藥物製造或展延許可函影本")]
        public string FILE3 { get; set; }
        public string FILE3_TEXT { get; set; }

        /// <summary>
        /// 製造業藥商許可執照影本
        /// </summary>
        [Display(Name = "製造業藥商許可執照影本")]
        public string FILE4 { get; set; }
        public string FILE4_TEXT { get; set; }

        public string PL_CD_TEXT { get; set; }

        /// <summary>
        /// 產品類別
        /// </summary>
        [Display(Name = "產品類別")]
        public string TRA_CON_CHECK { get; set; }

        /// <summary>
        /// 案件進度
        /// </summary>
        [Display(Name = "案件進度")]
        public string CODE_CD { get; set; }

        /// <summary>
        /// 案件原始進度
        /// </summary>
        [Display(Name = "案件原始進度")]
        public string CODE_CD_TEXT { get; set; }

        /// <summary>
        /// 案件是否鎖定
        /// </summary>
        public bool IS_CASE_LOCK { get; set; }

        /// <summary>
        /// 公文文號(申請人填寫)
        /// </summary>
        [Display(Name = "公文文號")]
        public string MOHW_CASE_NO_SELF { get; set; }

        [Display(Name = "公文日期")]
        public string MOHW_CASE_DATE { get; set; }
    }

    /// <summary>
    /// 檔案預覽
    /// </summary>
    public class Apply_005004PreviewModel : ApplyModel
    {
        public string FILENAME { get; set; }

        public string APP_ID { get; set; }

        public string FILE_NO { get; set; }

        public string SRC_NO { get; set; }
    }
}