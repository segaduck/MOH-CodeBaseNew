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
    public class Apply_041001ViewModel
    {
        public Apply_041001ViewModel()
        {
        }

        /// <summary>
        /// 表單填寫
        /// </summary>
        public Apply_041001FormModel Form { get; set; }

        public Apply_041001AppDocModel AppDoc { get; set; }

    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_041001FormModel : ApplyModel
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
        [Display(Name = "申請人姓名/單位名稱")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 申請人身分證統一編號
        /// </summary>
        [Display(Name = "申請人身分證統一編號\r\n（或投保單位統一編號、\r\n醫事服務機構代號）")]
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
        [Display(Name = "申請人地址(含郵遞區號)")]
        [Required]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人地址")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人地址")]
        public string C_ADDR { get; set; }
      
        /// <summary>
        /// 代理人姓名
        /// </summary>
        [Display(Name = "代理人姓名\r\n（請附委託書正本，倘為法定代理人，\r\n請檢附關係證明文件影本）")]
        public string R_NAME { get; set; }

        /// <summary>
        /// 代理人身分證統一編號
        /// </summary>
        [Display(Name = "代理人身分證統一編號")]
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
        [Display(Name = "代理人地址(含郵遞區號)")]
        public string R_ZIPCODE { get; set; }

        /// <summary>
        /// 代理人住所或居所
        /// </summary>
        [Display(Name = "代理人地址")]
        public string R_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 代理人住所或居所
        /// </summary>
        [Display(Name = "代理人地址")]
        public string R_ADDR { get; set; }
        /// <summary>
        /// 核定文件 1
        /// </summary>
        [Display(Name = "衛生福利部\r\n中央健康保險署\r\n核定文件\r\n(請檢附影本)\r\n")]

        public string KIND1 { get; set; }
        public bool KIND1_CHK { get; set; }
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
        /// 核定文件 2 繳款單
        /// </summary>
        public string KIND2 { get; set; }
        public bool KIND2_CHK { get; set; }

        /// <summary>
        /// 核定文件 3 其他
        /// </summary>
        public string KIND3 { get; set; }
        public bool KIND3_CHK { get; set; }

        /// <summary>
        /// 收受或知悉日期
        /// </summary>
        public DateTime? KNOW_DATE { get; set; }
        [Display(Name = "收受或知悉日期")]
        public string KNOW_DATE_STR { get; set; }
        /// <summary>
        /// 請求事項
        /// </summary>
        [Display(Name ="請求事項")]
        public string KNOW_MEMO { get; set; }
        /// <summary>
        /// 事實及理由
        /// </summary>
        [Display(Name ="事實及理由(請以條例方式，簡要敘明)")]
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
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
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
        /// 核定文件(繳款單)
        /// </summary>
        [Display(Name = "核定文件(繳款單)")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
        public HttpPostedFileBase FILE_2 { get; set; }

        public string FILE_2_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_2_TEXT { get; set; }

        /// <summary>
        /// 核定文件(繳款單)-檔案類型
        /// </summary>
        public string FILE_2_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_2 != null)
                {
                    ret = this.FILE_2.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 核定文件(繳款單)
        /// </summary>
        [Display(Name = "核定文件(繳款單)")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
        public HttpPostedFileBase FILE_3 { get; set; }

        public string FILE_3_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_3_TEXT { get; set; }

        /// <summary>
        /// 核定文件(繳款單)-檔案類型
        /// </summary>
        public string FILE_3_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_3 != null)
                {
                    ret = this.FILE_3.ContentType;
                }
                return ret;
            }
        }
        /// <summary>
        /// 核定文件(繳款單)
        /// </summary>
        [Display(Name = "核定文件(繳款單)")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
        public HttpPostedFileBase FILE_4 { get; set; }

        public string FILE_4_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_4_TEXT { get; set; }

        /// <summary>
        /// 核定文件(繳款單)-檔案類型
        /// </summary>
        public string FILE_4_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_4 != null)
                {
                    ret = this.FILE_4.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 佐證檔案_清單
        /// </summary>
        public IList<Apply_041001SRVLSTModel> SRVLIST { get; set; }

    }

    public class Apply_041001SRVLSTModel : Apply_FileModel
    {
        public string SEQ_NO { get; set; }
    
        /// <summary>
        /// 佐證檔案
        /// </summary>
        [Display(Name = "其他")]
        public HttpPostedFileBase FILE_5 { get; set; }
        public string FILE_5_TEXT { get; set; }
    }
    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_041001DoneModel
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
    public class Apply_041001AppDocModel : ApplyModel
    {
        public Apply_041001AppDocModel()
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
        [Display(Name = "申請人姓名/單位名稱")]
        public string NAME { get; set; }

        /// <summary>
        /// 申請人身分證統一編號
        /// </summary>
        [Display(Name = "申請人身分證統一編號\r\n（或投保單位統一編號、\r\n醫事服務機構代號）")]
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
        [Display(Name = "申請人地址(含郵遞區號)")]
        public string C_ZIPCODE { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人地址")]
        public string C_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 申請人通訊地址
        /// </summary>
        [Display(Name = "申請人地址")]
        public string C_ADDR { get; set; }

        /// <summary>
        /// 代理人姓名
        /// </summary>
        [Display(Name = "代理人姓名\r\n（請附委託書正本，倘為法定代理人，\r\n請檢附關係證明文件影本）")]
        public string R_NAME { get; set; }

        /// <summary>
        /// 代理人身分證統一編號
        /// </summary>
        [Display(Name = "代理人身分證統一編號")]
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
        [Display(Name = "代理人地址(含郵遞區號)")]
        public string R_ZIPCODE { get; set; }

        /// <summary>
        /// 代理人住所或居所
        /// </summary>
        [Display(Name = "代理人地址")]
        public string R_ZIPCODE_TEXT { get; set; }

        /// <summary>
        /// 代理人住所或居所
        /// </summary>
        [Display(Name = "代理人地址")]
        public string R_ADDR { get; set; }

        /// <summary>
        /// 核定文件 1
        /// </summary>
        [Display(Name = "衛生福利部\r\n中央健康保險署\r\n核定文件\r\n(請檢附影本)\r\n")]

        public string KIND1 { get; set; }
        public bool KIND1_CHK { get; set; }
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
        /// 核定文件 2 繳款單
        /// </summary>
        public string KIND2 { get; set; }
        public bool KIND2_CHK { get; set; }

        /// <summary>
        /// 核定文件 3 其他
        /// </summary>
        public string KIND3 { get; set; }
        public bool KIND3_CHK { get; set; }

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
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
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
        /// 核定文件(繳款單)
        /// </summary>
        [Display(Name = "核定文件")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
        public HttpPostedFileBase FILE_2 { get; set; }

        public string FILE_2_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_2_TEXT { get; set; }

        /// <summary>
        /// 核定文件(繳款單)-檔案類型
        /// </summary>
        public string FILE_2_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_2 != null)
                {
                    ret = this.FILE_2.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 核定文件(繳款單)
        /// </summary>
        [Display(Name = "核定文件")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
        public HttpPostedFileBase FILE_3 { get; set; }

        public string FILE_3_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_3_TEXT { get; set; }

        /// <summary>
        /// 核定文件(繳款單)-檔案類型
        /// </summary>
        public string FILE_3_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_3 != null)
                {
                    ret = this.FILE_3.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 核定文件(繳款單)
        /// </summary>
        [Display(Name = "核定文件")]
        [Control(Mode = Control.FileUpload, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(總檔案大小5MB以下，請附上圖檔，例如：PDF、JPG、JPEG、BMP、PNG、GIF、TIF)")]
        public HttpPostedFileBase FILE_4 { get; set; }

        public string FILE_4_FILENAME { get; set; }

        [Control(Mode = Control.Hidden)]
        public string FILE_4_TEXT { get; set; }

        /// <summary>
        /// 核定文件(繳款單)-檔案類型
        /// </summary>
        public string FILE_4_MIME
        {
            get
            {
                string ret = null;
                if (this.FILE_4 != null)
                {
                    ret = this.FILE_4.ContentType;
                }
                return ret;
            }
        }
        /// <summary>
        /// 推薦表(PDF檔)_清單
        /// </summary>
        public IList<Apply_041001SRVLSTModel> SRVLIST { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_041001FILEModel> FILE { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Apply_041001FILEModel : Apply_FileModel
    {
    }
}