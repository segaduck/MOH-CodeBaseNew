using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel
    /// </summary>
    public class Apply_011003ViewModel
    {
        public Apply_011003ViewModel()
        {
        }

        /// <summary>
        /// 表單填寫
        /// </summary>
        public Apply_011003FormModel Form { get; set; }

    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011003FormModel : ApplyModel
    {
        /// <summary>
        /// 補件內容
        /// </summary>
        [Display(Name = "補件內容")]
        public string NOTE { get; set; }

        /// <summary>
        /// 申請種類
        /// </summary>
        [Display(Name = "申請種類：依「專門職業及技術人員高等考試社會工作師考試規則」第六條申請部分科目免試")]
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
        public string BIRTHDAY { get; set; }

        public string BIRTHDAY_AD
        {
            get
            {
                if (string.IsNullOrEmpty(BIRTHDAY))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(BIRTHDAY, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                BIRTHDAY = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

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
        public string TEL_0 { get; set; }
        public string TEL_1 { get; set; }

        /// <summary>
        /// 電話(宅)
        /// </summary>
        [Display(Name = "電話(宅)")]
        public string FAX { get; set; }
        public string FAX_0 { get; set; }
        public string FAX_1 { get; set; }


        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "行動電話")]
        public string MOBILE { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址郵遞區號")]
        public string ADDR { get; set; }

        public string ADDR_TEXT { get; set; }

        [Display(Name = "通訊地址")]
        public string ADDR_DETAIL { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        [Display(Name = "E-MAIL")]
        public string MAIL { get; set; }
        public string MAIL_0 { get; set; }
        public string MAIL_1 { get; set; }
        public string EMAIL { get; set; }
        /// <summary>
        /// 學歷證明
        /// </summary>
        [Display(Name = "學歷證明")]
        public string EDUCATION { get; set; }

        /// <summary>
        /// 學歷-學校名稱
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
        public string GRADUATION_MONTH { get; set; }

        /// <summary>
        /// 考試通知書-通知年月
        /// </summary>
        [Display(Name = "考試通知書-通知年月")]
        public string NOTICEDAY { get; set; }
        public string NOTICEDAY_YEAR { get; set; }
        public string NOTICEDAY_MONTH { get; set; }


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
        public string FILE_FIDC_TEXT { get; set; }

        /// <summary>
        /// 國民身分證背面影本
        /// </summary>
        [Display(Name = "國民身分證背面影本")]
        public string FILE_BIDC { get; set; }
        public string FILE_BIDC_TEXT { get; set; }

        /// <summary>
        /// 最近一年內半身脫帽照片一張
        /// </summary>
        [Display(Name = "最近一年內半身脫帽照片一張")]
        public string FILE_PIC { get; set; }
        public string FILE_PIC_TEXT { get; set; }

        /// <summary>
        /// 中華民國一百零二年起，經考選部依「專技人員社會工作師考試應考資格第五條審議通過並公告名單」所列學校之畢業證書影本
        /// </summary>
        [Display(Name = "中華民國一百零二年起，經考選部依「專技人員社會工作師考試應考資格第五條審議通過並公告名單」所列學校之畢業證書影本")]
        public string FILE_GRAD { get; set; }
        public string FILE_GRAD_TEXT { get; set; }

        /// <summary>
        /// 非考選部公告之學校系所(報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本)")]
        public string FILE_NOTI { get; set; }
        public string FILE_NOTI_TEXT { get; set; }

        /// <summary>
        /// 非考選部公告之學校系所(畢業證書影本)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(畢業證書影本)")]
        public string FILE_NOTI2 { get; set; }
        public string FILE_NOTI2_TEXT { get; set; }
        /// <summary>
        /// 非考選部公告之學校系所(學分證明)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(學分證明)")]
        public string FILE_NOTI3 { get; set; }
        public string FILE_NOTI3_TEXT { get; set; }
        /// <summary>
        /// 非考選部公告之學校系所(實習證明)
        /// </summary>
        [Display(Name = "非考選部公告之學校系所(實習證明)")]
        public string FILE_NOTI4 { get; set; }
        public string FILE_NOTI4_TEXT { get; set; }

        /// <summary>
        /// 經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本
        /// </summary>
        [Display(Name = "經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本")]
        public string FILE_GRADOFF { get; set; }
        public string FILE_GRADOFF_TEXT { get; set; }

        /// <summary>
        /// 國外社會工作師資格證明文件
        /// </summary>
        [Display(Name = "國外社會工作師資格證明文件")]
        public string FILE_FONSRV { get; set; }
        public string FILE_FONSRV_TEXT { get; set; }

        /// <summary>
        /// 戶籍謄本或戶口名簿(改過姓名者須附)
        /// </summary>
        [Display(Name = "戶籍謄本或戶口名簿(改過姓名者須附)")]
        public string FILE_DOMICILE { get; set; }
        public string FILE_DOMICILE_TEXT { get; set; }

        /// <summary>
        /// 公立或依法立案之私立專科以上學校教學之證明文件
        /// </summary>
        [Display(Name = "公立或依法立案之私立專科以上學校教學之證明文件")]
        public string FILE_SCHOOL { get; set; }
        public string FILE_SCHOOL_TEXT { get; set; }

        /// <summary>
        /// 其他（申請人自行列舉）
        /// </summary>
        [Display(Name = "其他（申請人自行列舉）")]
        public string FILE_OTHER { get; set; }
        public string FILE_OTHER_TEXT { get; set; }

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
        /// 系統狀態
        /// </summary>
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

        /// <summary>
        /// 案件進度
        /// </summary>
        [Display(Name = "案件進度")]
        public string FLOW_CD_TEXT { get; set; }

        /// <summary>
        /// 案件狀態修改
        /// </summary>
        [Display(Name = "案件狀態修改")]
        public string FLOW_CD { get; set; }

        /// <summary>
        /// 服務經歷_清單
        /// </summary>
        public IList<Apply_011003SRVLSTModel> SRVLIST { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_011003FILEModel> FILE { get; set; }

        public string FileCheck { get; set; }

        public string IsNotice { get; set; }        

    }

    /// <summary>
    /// 服務經歷
    /// </summary>
    public class Apply_011003SRVLSTModel : Apply_FileModel
    {
        /// <summary>
        /// 序號
        /// </summary>
        public string SEQ_NO { get; set; }

        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 服務經歷-服務單位名稱
        /// </summary>
        [Display(Name = "服務經歷-服務單位名稱")]
        public string SRV_NAM { get; set; }

        /// <summary>
        /// 服務經歷-職稱
        /// </summary>
        [Display(Name = "服務經歷-職稱")]
        public string SRV_TITLE { get; set; }

        /// <summary>
        /// 服務經歷-服務年資(起)
        /// </summary>
        [Display(Name = "服務經歷-服務年資(起)")]
        public string SRV_SYEAR { get; set; }

        public string SRV_SYEAR_AD
        {
            get
            {
                if (string.IsNullOrEmpty(SRV_SYEAR))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(SRV_SYEAR, "/"));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                SRV_SYEAR = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value), "/");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 服務經歷-服務年資(迄)
        /// </summary>
        [Display(Name = "服務經歷-服務年資(迄)")]
        public string SRV_EYEAR { get; set; }

        public string SRV_EYEAR_AD
        {
            get
            {
                if (string.IsNullOrEmpty(SRV_EYEAR))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(SRV_EYEAR, "/"));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                SRV_EYEAR = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value), "/");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 服務證明正本彩色檔
        /// </summary>
        [Display(Name = "服務證明正本彩色檔")]
        public string FILE_PICRGB { get; set; }
        public string FILE_PICRGB_TEXT { get; set; }

        /// <summary>
        /// 服務單位認可有案證明文件
        /// </summary>
        [Display(Name = "服務單位認可有案證明文件")]
        public string FILE_SRVPROVE { get; set; }
        public string FILE_SRVPROVE_TEXT { get; set; }

        /// <summary> 
        /// 勞工保險被保險人投保明細表影本(服務證明為團體或私立機構開具者需附)
        /// </summary>
        [Display(Name = "勞工保險被保險人投保明細表影本(服務證明為團體或私立機構開具者需附)")]
        public string FILE_LABOR { get; set; }
        public string FILE_LABOR_TEXT { get; set; }

        /// <summary>
        /// 特殊需求
        /// </summary>
        [Display(Name = "特殊需求")]
        public string SRVLST_DEMAND_1 { get; set; }
        public string SRVLST_DEMAND_1_TEXT { get; set; }

        /// <summary>
        /// 服務單位立案或法人登記證書影本(服務證明為團體或私立機構開具者需附)
        /// </summary>
        [Display(Name = "服務單位立案或法人登記證書影本(服務證明為團體或私立機構開具者需附)")]
        public string FILE_LEGAL { get; set; }
        public string FILE_LEGAL_TEXT { get; set; }

        /// <summary>
        /// 特殊需求
        /// </summary>
        [Display(Name = "特殊需求")]
        public string SRVLST_DEMAND_2 { get; set; }
        public string SRVLST_DEMAND_2_TEXT { get; set; }

        /// <summary>
        /// 服務單位章程影本(服務證明為團體或私立機構開具者否需附)
        /// </summary>
        [Display(Name = "服務單位章程影本(服務證明為團體或私立機構開具者否需附)")]
        public string FILE_CHARTER { get; set; }
        public string FILE_CHARTER_TEXT { get; set; }        
    }

    /// <summary>
    /// 
    /// </summary>
    public class Apply_011003FILEModel : Apply_FileModel
    {
    }
}