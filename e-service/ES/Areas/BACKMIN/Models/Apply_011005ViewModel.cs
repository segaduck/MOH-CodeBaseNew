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
    public class Apply_011005ViewModel
    {
        public Apply_011005ViewModel()
        {
        }

        /// <summary>
        /// 通知補件是否僅通知需回寄項目
        /// </summary>
        public string isOnlyNoticePaper { get; set; }

        /// <summary>
        /// Form
        /// </summary>
        public Apply_011005FormModel Form { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Apply_011005FormModel Detail { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //public Apply_011005PreviewModel Preview { get; set; }

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
    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011005FormModel : ApplyModel
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
        public string APP_TIME { get; set; }

        /// <summary>
        /// 申請日期(民國YYY/MM/DD)
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_TIME_SHOW { get; set; }

        ///<summary>
        /// 申辦項目
        /// </summary>
        [Display(Name = "申辦項目")]
        public string APP_NAME { get; set; }

        /// <summary>
        /// 案號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 案件進度
        /// </summary>
        [Display(Name = "案件進度")]
        public string FLOW_CD_TEXT { get; set; }


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
        /// 申請人姓名
        /// </summary>
        [Display(Name = "姓名")]
        [Required]
        public string NAME { get; set; }



        /// <summary>
        /// 申請人出生年月日-民國年
        /// </summary>
        [Display(Name = "出生年月日")]
        [Required]
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

        /// <summary>
        /// 申請人身分證字號
        /// </summary>
        [Display(Name = "國民身分證統一編號")]
        [Required]
        public string IDN { get; set; }

        #region 電話(公)
        /// <summary>
        /// 電話(公) 00-00000000#000
        /// </summary>
        [Display(Name = "電話(公)")]
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
        [Display(Name = "電話(宅)")]
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

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "行動電話")]
        public string MOBILE { get; set; }

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

        #region 地址
        /// <summary>
        /// 通訊地址郵遞區號
        /// </summary>
        [Display(Name = "通訊地址(含郵遞區號)")]
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
        [Display(Name = "戶籍地址(含郵遞區號)")]
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
        #endregion

        /// <summary>
        /// 同通訊地址
        /// </summary>
        [Display(Name = "同通訊地址")]
        public string H_EQUAL { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        [Display(Name = "承辦人姓名")]
        public string ADM_NAM { get; set; }

        /// <summary>
        /// 承辦人行動電話
        /// </summary>
        [Display(Name = "承辦人行動電話")]
        public string ADM_MOBILE { get; set; }

        #region 承辦人E-MAIL
        public string ADM_MAIL { get; set; }

        [Display(Name = "承辦人E-MAIL")]
        public string ADM_MAIL_0 { get; set; }

        [Display(Name = "承辦人E-MAIL")]
        public string ADM_MAIL_1 { get; set; }

        public string ADM_MAIL_2 { get; set; }

        public string ADM_MAIL_3 { get; set; }

        #endregion

        [Display(Name = "運用單位發文日期")]
        public string ACC_SDATE { get; set; }

        public string ACC_SDATE_AD
        {
            get
            {
                if (string.IsNullOrEmpty(ACC_SDATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(ACC_SDATE, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                ACC_SDATE = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 運用單位發文字號
        /// </summary>
        [Display(Name = "運用單位發文字號")]
        public string ACC_NUM { get; set; }

        /// <summary>
        /// 是否合併上傳
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        [Required]
        public string MERGEYN { get; set; }


        /// <summary>
        /// 繳費日期
        /// </summary>
        [Display(Name = "繳費日期")]
        public string PAY_EXT_TIME { get; set; }

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
                if (string.IsNullOrEmpty(PAY_STATUS))
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
                else
                {
                    value = "Y".Equals(!string.IsNullOrEmpty(this.PAY_STATUS) ? this.PAY_STATUS.ToUpper() : "N") ? true : false;
                }

            }
        }

        /// <summary>
        /// 刪除註記
        /// </summary>
        public string DEL_MK { get; set; }

        /// <summary>
        /// 刪除時間
        /// </summary>
        public string DEL_TIME { get; set; }

        /// <summary>
        /// 刪除位置
        /// </summary>
        public string DEL_FUN_CD { get; set; }

        /// <summary>
        /// 刪除人帳號
        /// </summary>
        public string DEL_ACC { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public string UPD_TIME { get; set; }

        /// <summary>
        /// 更新位置
        /// </summary>
        public string UPD_FUN_CD { get; set; }

        /// <summary>
        /// 更新人帳號
        /// </summary>
        public string UPD_ACC { get; set; }

        /// <summary>
        /// 新增時間
        /// </summary>
        public string ADD_TIME { get; set; }

        /// <summary>
        /// 新增位置
        /// </summary>
        public string ADD_FUN_CD { get; set; }

        /// <summary>
        /// 新增人帳號
        /// </summary>
        public string ADD_ACC { get; set; }

        /// <summary>
        /// 身分證正面影本
        /// </summary>
        [Display(Name = "身分證正面影本")]
        public string FILE_1 { get; set; }
        public string FILE_1_TEXT { get; set; }

        /// <summary>
        /// 身分證反面影本
        /// </summary>
        [Display(Name = "身分證反面影本")]
        public string FILE_2 { get; set; }
        public string FILE_2_TEXT { get; set; }

        /// <summary>
        /// 相片-大頭照
        /// </summary>
        [Display(Name = "相片")]
        public string FILE_3 { get; set; }
        public string FILE_3_TEXT { get; set; }

        /// <summary>
        /// 戶籍謄本或戶口名簿影本
        /// </summary
        [Display(Name = "戶籍謄本或戶口名簿影本")]
        public string FILE_4 { get; set; }
        public string FILE_4_TEXT { get; set; }

        /// <summary>
        /// 系統狀態
        /// </summary>
        [Display(Name = "系統狀態")]
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 預計完成日期
        /// </summary>
        [Display(Name = "預計完成日期")]
        public string APP_EXT_DATE { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        [Display(Name = "承辦人姓名")]
        public string PRO_NAM { get; set; }

        public Apply_011005FileModel FileList { get; set; }
    }

    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_011005FileModel
    {
        public Apply_011005FileModel()
        {
            this.FILENAM = new List<FileGroupModel>();
        }
        public string APP_ID { get; set; }

        /// <summary>
        /// 身分證正面影本
        /// </summary>
        [Display(Name = "身分證正面影本")]
        public string FILE_IDNF { get; set; }
        public string FILE_IDNF_TEXT { get; set; }

        /// <summary>
        /// 身分證反面影本
        /// </summary>
        [Display(Name = "身分證反面影本")]
        public string FILE_IDNB { get; set; }
        public string FILE_IDNB_TEXT { get; set; }

        /// <summary>
        /// 相片-大頭照
        /// </summary>
        [Display(Name = "照片(規格應同護照照片)")]
        public string FILE_PHOTO { get; set; }
        public string FILE_PHOTO_TEXT { get; set; }

        /// <summary>
        /// 戶籍謄本或戶口名簿影本
        /// </summary
        [Display(Name = "戶籍謄本或戶口名簿影本")]
        public string FILE_HOUSEHOLD { get; set; }
        public string FILE_HOUSEHOLD_TEXT { get; set; }

        public List<FileGroupModel> FILENAM { get; set; }
    }
}