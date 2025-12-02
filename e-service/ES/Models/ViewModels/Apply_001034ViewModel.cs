using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using System.Web.Mvc;

namespace ES.Models.ViewModels
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
        public string APPLY_DATE { get; set; }

        /// <summary>
        /// 起始日期(西元)
        /// </summary>
        public string DATE_S_AC { get; set; }

        /// <summary>
        /// 終止日期(西元)
        /// </summary>
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
        /// PreView
        /// </summary>
        public Apply_001034ViewModel PreView { get; set; }

        /// <summary>
        /// Field notice columns
        /// </summary>
        public string FieldList { get; set; }

        /// <summary>
        /// Good notice columns
        /// </summary>
        public string GoodFieldList { get; set; }

        /// <summary>
        /// 狀態
        /// </summary>
        public string STATUS { get; set; }

        /// <summary>
        /// 補件件數
        /// </summary>
        public int AppDocCount { get; set; }

    }

    /// <summary>
    /// APPLY_001034_GOODS 危險性醫療儀器進口申請作業 貨品資料
    /// </summary>
    public class Apply_001034_GoodsViewModel : Apply_001034_GoodsModel
    {

        /// <summary>
        /// 貨品類別（規格）文字
        /// </summary>
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
        public string DOC_TYP_01_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源01
        /// </summary>
        public string DOC_TYP_01_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型02
        /// </summary>
        public HttpPostedFileBase DOC_TYP_02_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型02
        /// </summary>
        public string DOC_TYP_02_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源02
        /// </summary>
        public string DOC_TYP_02_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型03
        /// </summary>
        public HttpPostedFileBase DOC_TYP_03_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型03
        /// </summary>
        public string DOC_TYP_03_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源03
        /// </summary>
        public string DOC_TYP_03_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型04
        /// </summary>
        public HttpPostedFileBase DOC_TYP_04_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型04
        /// </summary>
        public string DOC_TYP_04_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源04
        /// </summary>
        public string DOC_TYP_04_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型05
        /// </summary>
        public HttpPostedFileBase DOC_TYP_05_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型05
        /// </summary>
        public string DOC_TYP_05_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源05
        /// </summary>
        public string DOC_TYP_05_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型06
        /// </summary>
        public HttpPostedFileBase DOC_TYP_06_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型06
        /// </summary>
        public string DOC_TYP_06_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源06
        /// </summary>
        public string DOC_TYP_06_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型07
        /// </summary>
        public HttpPostedFileBase DOC_TYP_07_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型07
        /// </summary>
        public string DOC_TYP_07_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源07
        /// </summary>
        public string DOC_TYP_07_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型08
        /// </summary>
        public HttpPostedFileBase DOC_TYP_08_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型08
        /// </summary>
        public string DOC_TYP_08_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源08
        /// </summary>
        public string DOC_TYP_08_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型09
        /// </summary>
        public HttpPostedFileBase DOC_TYP_09_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型09
        /// </summary>
        public string DOC_TYP_09_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源09
        /// </summary>
        public string DOC_TYP_09_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型10
        /// </summary>
        public HttpPostedFileBase DOC_TYP_10_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型10
        /// </summary>
        public string DOC_TYP_10_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源10
        /// </summary>
        public string DOC_TYP_10_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型11
        /// </summary>
        public HttpPostedFileBase DOC_TYP_11_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型11
        /// </summary>
        public string DOC_TYP_11_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源11
        /// </summary>
        public string DOC_TYP_11_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型12
        /// </summary>
        public HttpPostedFileBase DOC_TYP_12_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型12
        /// </summary>
        public string DOC_TYP_12_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源12
        /// </summary>
        public string DOC_TYP_12_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型13
        /// </summary>
        public HttpPostedFileBase DOC_TYP_13_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型13
        /// </summary>
        public string DOC_TYP_13_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源13
        /// </summary>
        public string DOC_TYP_13_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型14
        /// </summary>
        public HttpPostedFileBase DOC_TYP_14_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型14
        /// </summary>
        public string DOC_TYP_14_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源14
        /// </summary>
        public string DOC_TYP_14_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型15
        /// </summary>
        public HttpPostedFileBase DOC_TYP_15_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型15
        /// </summary>
        public string DOC_TYP_15_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源15
        /// </summary>
        public string DOC_TYP_15_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型16
        /// </summary>
        public HttpPostedFileBase DOC_TYP_16_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型16
        /// </summary>
        public string DOC_TYP_16_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源16
        /// </summary>
        public string DOC_TYP_16_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型17
        /// </summary>
        public HttpPostedFileBase DOC_TYP_17_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型17
        /// </summary>
        public string DOC_TYP_17_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源17
        /// </summary>
        public string DOC_TYP_17_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型18
        /// </summary>
        public HttpPostedFileBase DOC_TYP_18_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型18
        /// </summary>
        public string DOC_TYP_18_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源18
        /// </summary>
        public string DOC_TYP_18_FILE_SRC { get; set; }

        /// <summary>
        /// 檢附文件類型19
        /// </summary>
        public HttpPostedFileBase DOC_TYP_19_FILE { get; set; }

        /// <summary>
        /// 檢附文件類型19
        /// </summary>
        public string DOC_TYP_19_FILE_NAME { get; set; }

        /// <summary>
        /// 檢附文件類型檔案來源19
        /// </summary>
        public string DOC_TYP_19_FILE_SRC { get; set; }
    }

}