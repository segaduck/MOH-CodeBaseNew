using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Models;
using ES.Commons;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// APPLY_001034 危險性醫療儀器進口申請作業
    /// </summary>
    public class Apply_001034ViewModel : Apply_001034Model
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
        [Display(Name = "申請日期")]
        public string APPLY_DATE { get; set; }

        /// <summary>
        /// 預計完成日期
        /// </summary>
        public string APP_EXT_DATE { get; set; }

        /// <summary>
        /// 起始日期(西元)
        /// </summary>
        [Display(Name = "起始日期")] 
        public string DATE_S_AC { get; set; }

        /// <summary>
        /// 終止日期(西元)
        /// </summary>
        [Display(Name = "終止日期")]
        public string DATE_E_AC { get; set; }

        /// <summary>
        /// 轉口港(國家)
        /// </summary>
        public string TRN_PORT_COUNTRY { get; set; }

        /// <summary>
        /// 轉口港
        /// </summary>
        public string TRN_PORT_ID2 { get; set; }

        /// <summary>
        /// 起運口岸(國家)
        /// </summary>
        public string BEG_PORT_COUNTRY { get; set; }

        /// <summary>
        /// 起運口岸
        /// </summary>
        public string BEG_PORT_ID2 { get; set; }

        /// <summary>
        /// 申請用途(Hidden)
        /// </summary> 
        public string APP_USE_TEXT { get; set; }

        /// <summary>
        /// 核發方式(Hidden)
        /// </summary> 
        public string CONF_TYPE_TEXT { get; set; }

        /// <summary>
        /// 電話號碼(全)
        /// </summary>
        public string TEL { get; set; }

        /// <summary>
        /// 電話區碼
        /// </summary>
        [Display(Name = "聯絡人電話")]
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

        /// <summary>
        /// 傳真號碼(全)
        /// </summary>
        public string FAX { get; set; }

        /// <summary>
        /// 傳真區碼
        /// </summary>
        [Display(Name = "聯絡人傳真")]
        public string FAX_SEC { get; set; }

        /// <summary>
        /// 傳真號碼
        /// </summary>
        public string FAX_NO { get; set; }

        /// <summary>
        /// 傳真分機
        /// </summary>
        public string FAX_EXT { get; set; }

        #region 地址

        public string FULL_ADDRESS { get; set; }
        public string CITY_CODE { get; set; }
        public string CITY_TEXT { get; set; }
        public string CITY_DETAIL { get; set; }

        #endregion

        #region 聯絡地址

        public string TAX_FULL_ADDRESS { get; set; }
        [Display(Name = "聯絡地址中文")]
        public string TAX_ORG_CITY_CODE { get; set; }
        public string TAX_ORG_CITY_TEXT { get; set; }
        public string TAX_ORG_CITY_DETAIL { get; set; }

        #endregion

        /// <summary>
        /// Mail Domain
        /// </summary>
        public string DOMAINList { get; set; }

        /// <summary>
        /// 系統狀態
        /// </summary>
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public string GENDER { get; set; } 

        /// <summary>
        /// 生日
        /// </summary>
        public string BIRTHDAY_AC { get; set; }

        /// <summary>
        /// 貨品資料
        /// </summary>
        public List<Apply_001034_GoodsViewModel> Goods { get; set; }

        /// <summary>
        /// 申請案件附檔
        /// </summary>
        public List<Apply_FileModel> Files { get; set; }

        /// <summary>
        /// AuxView
        /// </summary>
        public Apply_001034ViewModel AuxView { get; set; }

        /// <summary>
        /// 申請案主檔
        /// </summary>
        public ApplyModel Apply { get; set; }

        /// <summary>
        /// 申請案主檔
        /// </summary>
        public AdminModel Admin { get; set; }

        /// <summary>
        /// 預覽檔案
        /// </summary>
        public Apply_FileModel PreViewFile { get; set; }

        /// <summary>
        /// 申請案件附檔
        /// </summary>
        public List<TblAPPLY_NOTICE> Notices { get; set; }

        public string MAIL_DATE_AC { get; set; }

        public string Note { get; set; }

    }

    /// <summary>
    /// APPLY_001034_GOODS 危險性醫療儀器進口申請作業 貨品資料
    /// </summary>
    public class Apply_001034_GoodsViewModel : Apply_001034_GoodsModel
    {

        /// <summary>
        /// 貨品類別（規格）文字
        /// </summary>
        [Display(Name = "貨品類別（規格）")]
        public string GOODS_TYPE_TEXT { get; set; }

        /// <summary>
        /// 貨品單位
        /// </summary>
        public string GOODS_UNIT_TEXT { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態1)
        /// </summary>
        public bool DOC_TYP_01_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態2)
        /// </summary>
        public bool DOC_TYP_02_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態3)
        /// </summary>
        public bool DOC_TYP_03_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態4)
        /// </summary>
        public bool DOC_TYP_04_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態5)
        /// </summary>
        public bool DOC_TYP_05_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態6)
        /// </summary>
        public bool DOC_TYP_06_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態7)
        /// </summary>
        public bool DOC_TYP_07_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態8)
        /// </summary>
        public bool DOC_TYP_08_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態9)
        /// </summary>
        public bool DOC_TYP_09_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態10)
        /// </summary>
        public bool DOC_TYP_10_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態11)
        /// </summary>
        public bool DOC_TYP_11_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態12)
        /// </summary>
        public bool DOC_TYP_12_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態13)
        /// </summary>
        public bool DOC_TYP_13_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態14)
        /// </summary>
        public bool DOC_TYP_14_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態15)
        /// </summary>
        public bool DOC_TYP_15_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態16)
        /// </summary>
        public bool DOC_TYP_16_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態17)
        /// </summary>
        public bool DOC_TYP_17_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態18)
        /// </summary>
        public bool DOC_TYP_18_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型(狀態19)
        /// </summary>
        public bool DOC_TYP_19_CHK { get; set; }

        /// <summary>
        /// 檢附文件類型01
        /// </summary>
        public HttpPostedFileBase DOC_TYP_01_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型01
        /// </summary>
        [Display(Name = "檢附文件類型一")]
        public string DOC_TYP_01_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型02
        /// </summary>
        public HttpPostedFileBase DOC_TYP_02_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型02
        /// </summary>
        [Display(Name = "檢附文件類型二")]
        public string DOC_TYP_02_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型03
        /// </summary>
        public HttpPostedFileBase DOC_TYP_03_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型03
        /// </summary>
        [Display(Name = "檢附文件類型三")]
        public string DOC_TYP_03_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型04
        /// </summary>
        public HttpPostedFileBase DOC_TYP_04_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型04
        /// </summary>
        [Display(Name = "檢附文件類型四")]
        public string DOC_TYP_04_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型05
        /// </summary>
        public HttpPostedFileBase DOC_TYP_05_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型05
        /// </summary>
        [Display(Name = "檢附文件類型五")]
        public string DOC_TYP_05_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型06
        /// </summary>
        public HttpPostedFileBase DOC_TYP_06_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型06
        /// </summary>
        [Display(Name = "檢附文件類型六")]
        public string DOC_TYP_06_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型07
        /// </summary>
        public HttpPostedFileBase DOC_TYP_07_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型07
        /// </summary>
        [Display(Name = "檢附文件類型七")]
        public string DOC_TYP_07_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型08
        /// </summary>
        public HttpPostedFileBase DOC_TYP_08_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型08
        /// </summary>
        [Display(Name = "檢附文件類型八")]
        public string DOC_TYP_08_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型09
        /// </summary>
        public HttpPostedFileBase DOC_TYP_09_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型09
        /// </summary>
        [Display(Name = "檢附文件類型九")]
        public string DOC_TYP_09_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型10
        /// </summary>
        public HttpPostedFileBase DOC_TYP_10_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型10
        /// </summary>
        [Display(Name = "檢附文件類型十")]
        public string DOC_TYP_10_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型11
        /// </summary>
        public HttpPostedFileBase DOC_TYP_11_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型11
        /// </summary>
        [Display(Name = "檢附文件類型十一")]
        public string DOC_TYP_11_FILE_NAME { get; set; }
        
        /// <summary>
        /// 檢附文件類型12
        /// </summary>
        public HttpPostedFileBase DOC_TYP_12_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型12
        /// </summary>
        [Display(Name = "檢附文件類型十二")]
        public string DOC_TYP_12_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型13
        /// </summary>
        public HttpPostedFileBase DOC_TYP_13_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型13
        /// </summary>
        [Display(Name = "檢附文件類型十三")]
        public string DOC_TYP_13_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型14
        /// </summary>
        public HttpPostedFileBase DOC_TYP_14_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型14
        /// </summary>
        [Display(Name = "檢附文件類型十四")]
        public string DOC_TYP_14_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型15
        /// </summary>
        public HttpPostedFileBase DOC_TYP_15_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型15
        /// </summary>
        [Display(Name = "檢附文件類型十五")]
        public string DOC_TYP_15_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型16
        /// </summary>
        public HttpPostedFileBase DOC_TYP_16_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型16
        /// </summary>
        [Display(Name = "檢附文件類型十六")]
        public string DOC_TYP_16_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型17
        /// </summary>
        public HttpPostedFileBase DOC_TYP_17_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型17
        /// </summary>
        [Display(Name = "檢附文件類型十七")]
        public string DOC_TYP_17_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型18
        /// </summary>
        public HttpPostedFileBase DOC_TYP_18_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型18
        /// </summary>
        [Display(Name = "檢附文件類型十八")]
        public string DOC_TYP_18_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型19
        /// </summary>
        public HttpPostedFileBase DOC_TYP_19_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型19
        /// </summary>
        [Display(Name = "檢附文件類型十九")]
        public string DOC_TYP_19_FILE_NAME { get; set; }

        public Apply_001034_GoodsViewModel GoodsAuxView { get; set; }

        /// <summary>
        /// 申請數量
        /// </summary>
        [Display(Name = "申請數量")]
        public string APPLY_CNT { get; set; }
    }

}