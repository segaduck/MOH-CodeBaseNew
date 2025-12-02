using System;
using System.ComponentModel.DataAnnotations;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class APPLY_011003
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請種類
        /// </summary>
        [Display(Name = "申請種類")]
        public string ISMEET { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        public string NAME { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 國民身分證統一編號
        /// </summary>
        [Display(Name = "國民身分證統一編號")]
        public string IDN { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [Display(Name = "性別")]
        public string SEX_CD { get; set; }

        /// <summary>
        /// 電話(公)
        /// </summary>
        [Display(Name = "電話(公)")]
        public string TEL { get; set; }

        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string FAX { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "行動電話")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 通訊地址(郵遞區號)
        /// </summary>
        [Display(Name = "通訊地址(郵遞區號)")]
        public string ADDR_CODE { get; set; }

        /// <summary>
        /// 通訊地址(不含郵遞區號，請填寫目前服務單位或可收掛號信之地址)
        /// </summary>
        [Display(Name = "通訊地址(不含郵遞區號，請填寫目前服務單位或可收掛號信之地址)")]
        public string ADDR { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string MAIL { get; set; }

        /// <summary>
        /// 學歷證明
        /// </summary>
        [Display(Name = "學歷證明")]
        public string EDUCATION { get; set; }

        /// <summary>
        /// 學歷-學校名稱(請填寫全銜)
        /// </summary>
        [Display(Name = "學歷-學校名稱(請填寫全銜)")]
        public string SCHOOL { get; set; }

        /// <summary>
        /// 學歷-系(組)所名稱
        /// </summary>
        [Display(Name = "學歷-系(組)所名稱")]
        public string OFFICE { get; set; }

        /// <summary>
        /// 學歷-畢業年月
        /// </summary>
        [Display(Name = "學歷-畢業年月")]
        public string GRADUATION { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

        /// <summary>
        /// 國民身分證正面影本
        /// </summary>
        [Display(Name = "國民身分證正面影本")]
        public string FILE_FIDC { get; set; }

        /// <summary>
        /// 國民身分證背面影本
        /// </summary>
        [Display(Name = "國民身分證背面影本")]
        public string FILE_BIDC { get; set; }

        /// <summary>
        /// 最近一年內半身脫帽照片一張
        /// </summary>
        [Display(Name = "最近一年內半身脫帽照片一張")]
        public string FILE_PIC { get; set; }

        /// <summary>
        /// 中華民國一百零二年起，經考選部依「專技人員社會工作師考試應考資格第五條審議通過並公告名單」所列學校之畢業證書影本
        /// </summary>
        [Display(Name = "中華民國一百零二年起，經考選部依「專技人員社會工作師考試應考資格第五條審議通過並公告名單」所列學校之畢業證書影本")]
        public string FILE_GRAD { get; set; }

        /// <summary>
        /// 非考選部公告之學校系所(報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本)")]
        public string FILE_NOTI { get; set; }
        /// <summary>
        /// 非考選部公告之學校系所(畢業證書影本)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(畢業證書影本)")]
        public string FILE_NOTI2 { get; set; }
        /// <summary>
        /// 非考選部公告之學校系所(學分證明)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(學分證明)")]
        public string FILE_NOTI3 { get; set; }
        /// <summary>
        /// 非考選部公告之學校系所(實習證明)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(實習證明)")]
        public string FILE_NOTI4 { get; set; }

        /// <summary>
        /// 經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本
        /// </summary>
        [Display(Name = "經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本")]
        public string FILE_GRADOFF { get; set; }

        /// <summary>
        /// 國外社會工作師或相當資格證明文件
        /// </summary>
        [Display(Name = "國外社會工作師或相當資格證明文件")]
        public string FILE_FONSRV { get; set; }

        /// <summary>
        /// 戶籍謄本或戶口名簿
        /// </summary>
        [Display(Name = "戶籍謄本或戶口名簿")]
        public string FILE_DOMICILE { get; set; }

        /// <summary>
        /// 公立或立案之私立專科以上學校教學之證明文件
        /// </summary>
        [Display(Name = "公立或立案之私立專科以上學校教學之證明文件")]
        public string FILE_SCHOOL { get; set; }

        /// <summary>
        /// 其他附件(申請人自行列舉)
        /// </summary>
        [Display(Name = "其他附件(申請人自行列舉)")]
        public string FILE_OTHER { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_MK { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? DEL_TIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_FUN_CD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DEL_ACC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPD_TIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UPD_FUN_CD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UPD_ACC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADD_TIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ADD_FUN_CD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ADD_ACC { get; set; }

        /// <summary>
        /// 考試通知書-通知年月
        /// </summary>
        public string NOTICEDAY { get; set; }
    }
}