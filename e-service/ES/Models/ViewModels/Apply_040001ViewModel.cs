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
    public class Apply_040001ViewModel
    {
        public Apply_040001ViewModel()
        {
        }

        /// <summary>
        /// 表單填寫
        /// </summary>
        public Apply_040001FormModel Form { get; set; }

        public Apply_040001AppDocModel AppDoc { get; set; }

    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_040001FormModel : ApplyModel
    {
        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_DATE { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "訴願人E-MAIL")]
        [Required]
        public string EMAIL { get; set; }
        /// <summary>
        /// 訴願人姓名
        /// </summary>
        [Display(Name = "訴願人中文姓名")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 訴願人出生年月日_字串
        /// </summary>
        [Display(Name = "訴願人出生年月日")]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// 訴願人身分證明文件字號
        /// </summary>
        [Display(Name = "訴願人身分證明文件字號")]
        [Required]
        public string IDN { get; set; }

        /// <summary>
        /// 訴願人訴願人連絡電話
        /// </summary>
        [Display(Name = "訴願人連絡電話")]
        public string H_TEL { get; set; }

        /// <summary>
        /// 訴願人手機號碼
        /// </summary>
        [Display(Name = "訴願人手機號碼")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 訴願人通訊地址
        /// </summary>
        [Display(Name = "訴願人住所或居所(含郵遞區號)")]
        [Required]
        public string C_ZIPCODE { get; set; }

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
        [Display(Name ="原行政處分機關")]
        [Required]
        public string ORG_NAME { get; set; }
        /// <summary>
        /// 訴願人收受或知悉行政處分之年月日
        /// </summary>
        [Display(Name ="訴願人收受或知悉行政處分之年月日")]
        [Required]
        public string ORG_DATE { get; set; }
        /// <summary>
        /// 訴願請求
        /// </summary>
        [Display(Name ="訴願請求")]
        [Required]
        public string ORG_MEMO { get; set; }
        /// <summary>
        /// 事實與理由
        /// </summary>
        [Display(Name ="事實與理由")]
        [Required]
        public string ORG_FACT { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE { get; set; }

        /// <summary>
        /// 佐證檔案_清單
        /// </summary>
        public IList<Apply_040001SRVLSTModel> SRVLIST { get; set; }

    }

    public class Apply_040001SRVLSTModel : Apply_FileModel
    {
        public string SEQ_NO { get; set; }
        /// <summary>
        /// 佐證檔案
        /// </summary>
        [Display(Name = "佐證檔案")]
        public HttpPostedFileBase FILE_1 { get; set; }
        public string FILE_1_TEXT { get; set; }
    }
    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_040001DoneModel
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
    public class Apply_040001AppDocModel : ApplyModel
    {
        public Apply_040001AppDocModel()
        {
        }
        ///// <summary>
        ///// 補件狀態
        ///// </summary>
        //public string APPSTATUS { get; set; }
        ///// <summary>
        ///// 補件欄位字串
        ///// </summary>
        //[Display(Name = "補件欄位字串")]
        //public string FieldStr { get; set; }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_DATE { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "訴願人E-MAIL")]
        public string EMAIL { get; set; }
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
        public string ORG_DATE { get; set; }
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
        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE { get; set; }

        /// <summary>
        /// 推薦表(PDF檔)_清單
        /// </summary>
        public IList<Apply_040001SRVLSTModel> SRVLIST { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_040001FILEModel> FILE { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Apply_040001FILEModel : Apply_FileModel
    {
    }
}