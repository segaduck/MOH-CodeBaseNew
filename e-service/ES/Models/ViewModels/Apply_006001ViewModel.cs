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
    public class Apply_006001ViewModel
    {
        public Apply_006001ViewModel()
        {
        }

        /// <summary>
        /// 表單填寫
        /// </summary>
        public Apply_006001FormModel Form { get; set; }

        public Apply_006001AppDocModel AppDoc { get; set; }

    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_006001FormModel : ApplyModel
    {
        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_DATE { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "申請人E-MAIL")]
        [Required]
        public string EMAIL { get; set; }

        /// <summary>
        /// 申請人姓名
        /// </summary>
        [Display(Name = "申請人姓名")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 申請人出生年月日_字串
        /// </summary>
        [Display(Name = "申請人出生年月日")]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// 申請人身分證統一編號
        /// </summary>
        [Display(Name = "申請人身分證統一編號")]
        [Required]
        public string IDN { get; set; }

        /// <summary>
        /// 申請人連絡電話
        /// </summary>
        [Display(Name = "申請人連絡電話")]
        public string H_TEL { get; set; }

        /// <summary>
        /// 申請人手機號碼
        /// </summary>
        [Display(Name = "申請人手機號碼")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人通訊地址(含郵遞區號)")]
        [Required]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人通訊地址")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人通訊地址")]
        public string C_ADDR { get; set; }
        /// <summary>
        /// 同上資料（被保險人與申請人為同一人）
        /// </summary>
        [Display(Name = "同上資料（被保險人與申請人為同一人）")]
        public string ISSAME { get; set; }
        public bool ISSAME_CHK { get; set; }
        /// <summary>
        /// 被保險人姓名
        /// </summary>
        [Display(Name = "被保險人姓名")]
        public string R_NAME { get; set; }

        /// <summary>
        /// 被保險人出生年月日_字串
        /// </summary>
        [Display(Name = "被保險人出生年月日")]
        public string R_BIRTH_STR { get; set; }

        /// <summary>
        /// 被保險人身分證統一編號
        /// </summary>
        [Display(Name = "被保險人身分證統一編號")]
        public string R_IDN { get; set; }

        /// <summary>
        /// 被保險人連絡電話
        /// </summary>
        [Display(Name = "被保險人連絡電話")]
        public string R_TEL { get; set; }

        /// <summary>
        /// 被保險人手機號碼
        /// </summary>
        [Display(Name = "被保險人手機號碼")]
        public string R_MOBILE { get; set; }

        /// <summary>
        /// 被保險人地址(含郵遞區號)
        /// </summary>
        [Display(Name = "被保險人地址(含郵遞區號)")]
        public string R_ZIPCODE { get; set; }

        /// <summary>
        /// 被保險人詳細地址
        /// </summary>
        [Display(Name = "被保險人詳細地址")]
        public string R_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 被保險人詳細地址
        /// </summary>
        [Display(Name = "被保險人詳細地址")]
        public string R_ADDR { get; set; }
        /// <summary>
        /// 核定文件 1
        /// </summary>
        [Display(Name = "不服勞保局核定文件(請檢附影本)")]

        public string KINDTYPE { get; set; }
        public bool KIND1_CHK { get; set; }
        public bool KIND2_CHK { get; set; }
        /// <summary>
        /// 核定文號日期
        /// </summary>
        public DateTime? LIC_DATE { get; set; }
        public string LIC_DATE_STR { get; set; }
        /// <summary>
        /// 核定文號 字
        /// </summary>
        public string LIC_CD { get; set; }
        /// <summary>
        /// 核定文號 號
        /// </summary>
        public string LIC_NUM { get; set; }
        /// <summary>
        /// 繳款單年度
        /// </summary>
        public string PAY_YEAR { get; set; }
        /// <summary>
        /// 繳款單月份
        /// </summary>
        public string PAY_MONTH { get; set; }
        /// <summary>
        /// 繳款單文號
        /// </summary>
        public string PAY_NUM { get; set; }
        /// <summary>
        /// 收受或知悉日期
        /// </summary>
        public DateTime? KNOW_DATE { get; set; }
        [Display(Name = "收受或知悉日期")]
        public string KNOW_DATE_STR { get; set; }
        /// <summary>
        /// 請求事項
        /// </summary>
        [Display(Name = "請求事項")]
        public string KNOW_MEMO { get; set; }
        /// <summary>
        /// 事實及理由
        /// </summary>
        [Display(Name = "事實及理由(請以條例方式，簡要敘明)")]
        public string KNOW_FACT { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE { get; set; }

        /// <summary>
        /// 勞保局核定文件影本
        /// </summary>
        [Display(Name = "勞保局核定文件影本")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(檔案大小5MB以下，可以接受副檔名：PDF、JPG、JPEG、BMP、PNG、GIF、TIF) ")]
        public HttpPostedFileBase FILE_1 { get; set; }

        public string FILE_1_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_1_TEXT { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案_檔案類型
        /// </summary>
        public string FILE_1_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_1 != null)
                {
                    ret = this.FILE_1.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 佐證檔案_清單
        /// </summary>
        public IList<Apply_006001SRVLSTModel> SRVLIST { get; set; }

    }

    public class Apply_006001SRVLSTModel : Apply_FileModel
    {
        public string SEQ_NO { get; set; }

        /// <summary>
        /// 佐證檔案
        /// </summary>
        [Display(Name = "其他")]
        public HttpPostedFileBase FILE_2 { get; set; }
        public string FILE_2_TEXT { get; set; }
    }
    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_006001DoneModel
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
    public class Apply_006001AppDocModel : ApplyModel
    {
        public Apply_006001AppDocModel()
        {
        }
        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_DATE { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "申請人E-MAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 申請人姓名
        /// </summary>
        [Display(Name = "申請人姓名")]
        public string NAME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name ="申請人出生年月日")]
        public string BIRTHDAY_STR { get; set; }

        /// <summary>
        /// 申請人身分證統一編號
        /// </summary>
        [Display(Name = "申請人身分證統一編號")]
        public string IDN { get; set; }

        /// <summary>
        /// 申請人連絡電話
        /// </summary>
        [Display(Name = "申請人連絡電話")]
        public string H_TEL { get; set; }

        /// <summary>
        /// 申請人手機號碼
        /// </summary>
        [Display(Name = "申請人手機號碼")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人通訊地址(含郵遞區號)")]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人通訊地址")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人通訊地址")]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 同上資料（被保險人與申請人為同一人）
        /// </summary>
        [Display(Name = "同上資料（被保險人與申請人為同一人）")]
        public string ISSAME { get; set; }
        public bool ISSAME_CHK { get; set; }

        /// <summary>
        /// 被保險人姓名
        /// </summary>
        [Display(Name = "被保險人姓名")]
        public string R_NAME { get; set; }

        /// <summary>
        /// 被保險人出生年月日_字串
        /// </summary>
        [Display(Name = "被保險人出生年月日")]
        public string R_BIRTH_STR { get; set; }

        /// <summary>
        /// 被保險人身分證統一編號
        /// </summary>
        [Display(Name = "被保險人身分證統一編號")]
        public string R_IDN { get; set; }

        /// <summary>
        /// 被保險人連絡電話
        /// </summary>
        [Display(Name = "被保險人連絡電話")]
        public string R_TEL { get; set; }

        /// <summary>
        /// 被保險人手機號碼
        /// </summary>
        [Display(Name = "被保險人手機號碼")]
        public string R_MOBILE { get; set; }

        /// <summary>
        /// 被保險人通訊地址
        /// </summary>
        [Display(Name = "被保險人地址(含郵遞區號)")]
        public string R_ZIPCODE { get; set; }

        /// <summary>
        /// 被保險人詳細地址
        /// </summary>
        [Display(Name = "被保險人詳細地址")]
        public string R_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 被保險人詳細地址
        /// </summary>
        [Display(Name = "被保險人詳細地址")]
        public string R_ADDR { get; set; }

        /// <summary>
        /// 核定文件 1
        /// </summary>
        [Display(Name = "不服勞保局核定文件(請檢附影本)")]

        public string KINDTYPE { get; set; }
        public bool KIND1_CHK { get; set; }
        public bool KIND2_CHK { get; set; }
        /// <summary>
        /// 核定文號日期
        /// </summary>
        public DateTime? LIC_DATE { get; set; }
        public string LIC_DATE_STR { get; set; }
        /// <summary>
        /// 核定文號 字
        /// </summary>
        public string LIC_CD { get; set; }
        /// <summary>
        /// 核定文號 號
        /// </summary>
        public string LIC_NUM { get; set; }
        /// <summary>
        /// 繳款單年度
        /// </summary>
        public string PAY_YEAR { get; set; }
        /// <summary>
        /// 繳款單月份
        /// </summary>
        public string PAY_MONTH { get; set; }
        /// <summary>
        /// 繳款單文號
        /// </summary>
        public string PAY_NUM { get; set; }

        /// <summary>
        /// 收受或知悉日期
        /// </summary>
        public DateTime? KNOW_DATE { get; set; }
        [Display(Name = "收受或知悉日期")]
        public string KNOW_DATE_STR { get; set; }
        /// <summary>
        /// 請求事項
        /// </summary>
        [Display(Name = "請求事項")]
        public string KNOW_MEMO { get; set; }
        /// <summary>
        /// 事實及理由
        /// </summary>
        [Display(Name = "事實及理由(請以條例方式，簡要敘明)")]
        public string KNOW_FACT { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string IS_MERGE { get; set; }


        /// <summary>
        /// 代理人委託書
        /// </summary>
        [Display(Name = "代理人委託書")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(檔案大小5MB以下，可以接受副檔名：PDF、JPG、JPEG、BMP、PNG、GIF、TIF) ")]
        public HttpPostedFileBase FILE_1 { get; set; }

        public string FILE_1_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_1_TEXT { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案_檔案類型
        /// </summary>
        public string FILE_1_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_1 != null)
                {
                    ret = this.FILE_1.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 推薦表(PDF檔)_清單
        /// </summary>
        public IList<Apply_006001SRVLSTModel> SRVLIST { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_006001FILEModel> FILE { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Apply_006001FILEModel : Apply_FileModel
    {
    }
}