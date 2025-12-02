using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using ES.Services;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_011004ViewModel
    {
        public Apply_011004ViewModel()
        {
        }

        /// <summary>
        /// 表單填寫
        /// </summary>
        public Apply_011004FormModel Form { get; set; }

        public Apply_011004AppDocModel AppDoc { get; set; }

    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011004FormModel : ApplyModel
    {
        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_DATE { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "聯絡人E-MAIL")]
        [Required]
        public string EMAIL { get; set; }
        /// <summary>
        /// 中文姓名
        /// </summary>
        [Display(Name = "中文姓名")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 英文姓名
        /// </summary>
        [Display(Name = "英文姓名")]
        [Required]
        public string ENAME { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Display(Name = "出生年月日")]
        [Required]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [Display(Name = "性別")]
        [Required]
        public string SEX_CD { get; set; }

        /// <summary>
        /// 國民身分證統一編號
        /// </summary>
        [Display(Name = "國民身分證統一編號")]
        [Required]
        public string IDN { get; set; }

        /// <summary>
        /// 電話(公司)
        /// </summary>
        [Display(Name = "電話(公)")]
        public string C_TEL { get; set; }
        
        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL { get; set; }
        
        /// <summary>
        /// 手機號碼
        /// </summary>
        [Display(Name = "手機號碼")]
        //[Required]
        public string MOBILE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址(含郵遞區號)")]
        [Required]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        //[Required]
        public string H_ZIPCODE { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        public string H_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        public string H_ADDR { get; set; }

        /// <summary>
        /// 申請用途
        /// </summary>
        [Display(Name = "申請用途")]
        [Required]
        public string APPLY_FOR { get; set; }

        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        [Required]
        public string APPLY_NUM { get; set; }

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string TYEAR { get; set; }

        /// <summary>
        /// 考試類別
        /// </summary>
        [Display(Name = "考試名稱類科")]
        [Required]
        public string TYPE { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE { get; set; }

        /// <summary>
        /// 社會工作師證書影本
        /// </summary>
        [Display(Name = "社會工作師證書影本")]
        public HttpPostedFileBase FILE_0 { get; set; }
        public string FILE_0_TEXT { get; set; }
    }


    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_011004DoneModel
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 件數
        /// </summary>
        public string Count { get; set; }
    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011004AppDocModel : ApplyModel
    {
        public Apply_011004AppDocModel()
        {
            this.NG_ITEM = string.Empty;
        }
        /// <summary>
        /// 補件狀態
        /// </summary>
        public string APPSTATUS { get; set; }
        /// <summary>
        /// 補件欄位字串
        /// </summary>
        [Display(Name = "補件欄位字串")]
        public string FieldStr { get; set; }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_DATE { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        [Required]
        public string EMAIL { get; set; }
        
        /// <summary>
        /// 中文姓名
        /// </summary>
        [Display(Name = "中文姓名")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 英文姓名
        /// </summary>
        [Display(Name = "英文姓名")]
        [Required]
        public string ENAME { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Display(Name = "申請人出生年月日")]
        [Required]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// 電話(公司)
        /// </summary>
        [Display(Name = "電話(公司)")]
        public string C_TEL { get; set; }
        
        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string H_TEL { get; set; }
        
        /// <summary>
        /// 手機號碼
        /// </summary>
        [Display(Name = "手機號碼")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        [Required]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 戶籍地址
        /// </summary>
        [Display(Name = "戶籍地址")]
        public string H_ADDR { get; set; }

        /// <summary>
        /// 申請用途
        /// </summary>
        [Display(Name = "申請用途")]
        [Required]
        public string APPLY_FOR { get; set; }

        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        [Required]
        public string APPLY_NUM { get; set; }

        /// <summary>
        /// 考試年度
        /// </summary>
        [Display(Name = "考試年度")]
        [Required]
        public string YEAR { get; set; }

        /// <summary>
        /// 考試類別
        /// </summary>
        [Display(Name = "考試類別")]
        [Required]
        public string TYPE { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE { get; set; }

        /// <summary>
        /// 社會工作師證書影本
        /// </summary>
        [Display(Name = "社會工作師證書影本")]
        public HttpPostedFileBase FILE_0 { get; set; }
        public string FILE_0_TEXT { get; set; }

        /// <summary>
        /// 補件項目
        /// </summary>
        public string NG_ITEM { get; set; }
    }
}