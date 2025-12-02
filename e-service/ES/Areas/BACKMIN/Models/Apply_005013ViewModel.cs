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
    public class Apply_005013ViewModel
    {
        public Apply_005013ViewModel()
        {
            this.Form = new Apply_005013FormModel();
            this.Detail = new Apply_005013FormModel();
        }

        public Apply_005013FormModel Form { get; set; }

        public Apply_005013FormModel Detail { get; set; }

        public Apply_005013PreviewModel Preview { get; set; }
    }

    public class Apply_005013FormModel : ApplyModel
    {
        /// <summary>
        /// 案件類型
        /// </summary>
        [Display(Name = "案件類型")]
        public string CSEE_TYPE { get; set; }

        /// <summary>
        /// 申辦日期(民國)
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_TIME_TW
        {
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
            set { APP_TIME = HelperUtil.TransTwToDateTime(value); }
        }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        public string NAME { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Display(Name = "身分證字號")]
        public string IDN { get; set; }

        #region 地址
        [Display(Name = "地址區碼")]
        public string TAX_ORG_CITY_CODE { get; set; }

        [Display(Name = "地址區域")]
        public string TAX_ORG_CITY_TEXT { get; set; }

        [Display(Name = "地址")]
        public string TAX_ORG_CITY_DETAIL { get; set; }

        [Display(Name = "地址")]
        public string ADDR { get; set; }
        #endregion 地址

        #region 電話
        /// <summary>
        /// 連絡電話(區域號碼)
        /// </summary>
        [Display(Name = "電話區域號碼")]
        public string TEL_BEFORE { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL_AFTER { get; set; }

        /// <summary>
        /// 分機
        /// </summary>
        [Display(Name = "分機")]
        public string TEL_Extension { get; set; }

        [Display(Name = "電話")]
        public string TEL { get; set; }
        #endregion

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name ="行動電話")]
        public string MOBILE { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }

        /// <summary>
        /// 生產國別
        /// </summary>
        [Display(Name = "生產國別")]
        public string ProductionCountry { get; set; }

        /// <summary>
        /// 生產國別
        /// </summary>
        [Display(Name = "生產國別")]
        public string ProductionCountry_TEXT { get; set; }

        /// <summary>
        /// 賣方國家
        /// </summary>
        [Display(Name = "賣方國家")]
        public string SellerCountry { get; set; }

        /// <summary>
        /// 賣方國家
        /// </summary>
        [Display(Name = "賣方國家")]
        public string SellerCountry_TEXT { get; set; }

        /// <summary>
        /// 起運國家
        /// </summary>
        [Display(Name = "起運國家")]
        public string ShippingPort { get; set; }

        /// <summary>
        /// 起運國家
        /// </summary>
        [Display(Name = "起運國家")]
        public string ShippingPort_TEXT { get; set; }

        /// <summary>
        /// 個人用途
        /// </summary>
        [Display(Name = "個人用途")]
        public string RADIOUSAGE { get; set; }

        /// <summary>
        /// 個人用途_說明
        /// </summary>
        [Display(Name = "個人用途_說明")]
        public string RADIOUSAGE_TEXT { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string RADIOYN { get; set; }

        /// <summary>
        /// 國際包裹招領單或海關補辦驗關通關手續通知書(事先提出申請無須檢附)之正面影本
        /// </summary>
        [Display(Name = "國際包裹招領單或海關補辦驗關通關手續通知書(事先提出申請無須檢附)之正面影本")]
        public HttpPostedFileBase File_1 { get; set; }

        [Display(Name = "國際包裹招領單或海關補辦驗關通關手續通知書(事先提出申請無須檢附)之正面影本")]
        public string File_1_Name { get; set; }

        public string File_1_TEXT { get; set; }

        /// <summary>
        /// 國際包裹招領單或海關補辦驗關通關手續通知書(事先提出申請無須檢附)之反面影本
        /// </summary>
        [Display(Name = "國際包裹招領單或海關補辦驗關通關手續通知書(事先提出申請無須檢附)之反面影本")]
        public HttpPostedFileBase File_2 { get; set; }

        [Display(Name = "國際包裹招領單或海關補辦驗關通關手續通知書(事先提出申請無須檢附)之反面影本")]
        public string File_2_Name { get; set; }

        public string File_2_TEXT { get; set; }

        /// <summary>
        /// 藥品數量超過限量表規定時需檢附國內外醫療機構出具之醫療證明文件
        /// </summary>
        [Display(Name = "醫療用途、其他用途或藥品數量超過限量表規定時需檢附國內外醫療機構出具之醫療證明文件")]
        public HttpPostedFileBase File_3 { get; set; }

        [Display(Name = "醫療用途、其他用途或藥品數量超過限量表規定時需檢附國內外醫療機構出具之醫療證明文件")]
        public string File_3_Name { get; set; }

        public string File_3_TEXT { get; set; }

        /// <summary>
        /// 產品外盒、仿單、說明書
        /// </summary>
        [Display(Name = "產品外盒、仿單、說明書")]
        public HttpPostedFileBase File_4 { get; set; }

        [Display(Name = "產品外盒、仿單、說明書")]
        public string File_4_Name { get; set; }

        public string File_4_TEXT { get; set; }

        /// <summary>
        /// 身分證影本(正面)
        /// </summary>
        [Display(Name = "身分證影本(正面)")]
        public HttpPostedFileBase File_5 { get; set; }

        [Display(Name = "身分證影本(正面)")]
        public string File_5_Name { get; set; }

        public string File_5_TEXT { get; set; }

        /// <summary>
        /// 身分證影本(反面)
        /// </summary>
        [Display(Name = "身分證影本(反面)")]
        public HttpPostedFileBase File_6 { get; set; }

        [Display(Name = "身分證影本(反面)")]
        public string File_6_Name { get; set; }

        public string File_6_TEXT { get; set; }

        /// <summary>
        /// 是否鎖定
        /// </summary>
        public bool IS_CASE_LOCK { get; set; }

        /// <summary>
        /// 案件狀態
        /// </summary>
        public string APPLY_STATUS { get; set; }

        /// <summary>
        /// 預計完成日
        /// </summary>
        public string APP_EXT_DATE_TW
        {
            get
            {
                if (string.IsNullOrEmpty(APP_EXT_DATE.ToString()))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(APP_EXT_DATE);
                }
            }
            set { APP_EXT_DATE = HelperUtil.TransTwToDateTime(value); }
        }

        /// <summary>
        /// 案件承辦人姓名
        /// </summary>
        public string ADMIN_NAME { get; set; }

        /// <summary>
        /// 案件進度
        /// </summary>
        public string CODE_CD { get; set; }

        /// <summary>
        /// 案件進度
        /// </summary>
        public string CODE_CD_TEXT { get; set; }

        /// <summary>
        /// 其他證明文件
        /// </summary>
        public IList<ApplyFileItem4Model> File_005013 { get; set; }

        /// <summary>
        /// 民眾輸入自用中藥切結書申請項目
        /// </summary>
        [Display(Name = "民眾輸入自用中藥切結書申請項目")]
        public IList<ApplyItem2Model> ApplyItem2 { get; set; }

        /// <summary>
        /// 申請項目
        /// </summary>
        [Display(Name = "申請項目")]
        public IList<ApplyItemModel> Item_005013 { get; set; }
    }

    /// <summary>
    /// 檔案預覽
    /// </summary>
    public class Apply_005013PreviewModel : ApplyModel
    {
        public string FILENAME { get; set; }

        public string APP_ID { get; set; }

        public string FILE_NO { get; set; }

        public string SRC_NO { get; set; }
    }

    /// <summary>
    /// 申請項目
    /// </summary>
    public class ApplyItemModel
    {
        /// <summary>
        /// 項次
        /// </summary>
        [Display(Name = "項次")]
        public string ItemNum { get; set; }

        /// <summary>
        /// 產品類別
        /// </summary>
        [Display(Name = "產品類別")]
        public string PorcType { get; set; }
        /// <summary>
        /// 貨名
        /// </summary>
        [Display(Name = "貨名")]
        public string Commodities { get; set; }
        /// <summary>
        /// 貨名
        /// </summary>
        [Display(Name = "備註")]
        public string CommodMemo { get; set; }
        /// <summary>
        /// 劑型
        /// </summary>
        [Display(Name = "劑型")]
        public string CommodType { get; set; }
        /// <summary>
        /// 劑型
        /// </summary>
        [Display(Name = "劑型")]
        public string CommodType_TEXT { get; set; }
        /// <summary>
        /// 規格
        /// </summary>
        [Display(Name = "規格")]
        public string Spec { get; set; }

        /// <summary>
        /// 申請數量
        /// </summary>
        [Display(Name = "申請數量")]
        public string Qty { get; set; }

        /// <summary>
        /// 申請數量單位
        /// </summary>
        [Display(Name = "申請數量單位")]
        public string Unit { get; set; }

        /// <summary>
        /// 申請數量單位
        /// </summary>
        [Display(Name = "申請數量單位")]
        public string Unit_TEXT { get; set; }
        /// <summary>
        /// 規格數量
        /// </summary>
        [Display(Name = "規格數量")]
        public string SpecQty { get; set; }

        /// <summary>
        /// 規格數量單位
        /// </summary>
        [Display(Name = "規格數量單位")]
        public string SpecUnit { get; set; }

        /// <summary>
        /// 規格數量單位
        /// </summary>
        [Display(Name = "規格數量單位")]
        public string SpecUnit_TEXT { get; set; }
    }

    /// <summary>
    /// 其他證明文件
    /// </summary>
    public class ApplyFileItem4Model
    {
        [Display(Name = "其他證明文件")]
        public HttpPostedFileBase File { get; set; }

        [Display(Name = "其他證明文件")]
        public string FileName { get; set; }

        public string FileName_TEXT { get; set; }
    }

    /// <summary>
    /// 民眾輸入自用中藥切結書申請項目
    /// </summary>
    public class ApplyItem2Model
    {
        /// <summary>
        /// 項次
        /// </summary>
        [Display(Name = "項次")]
        public string ItemNum { get; set; }

        /// <summary>
        /// 藥品名稱
        /// </summary>
        [Display(Name = "藥品名稱")]
        public string ItemName { get; set; }

        /// <summary>
        /// 用法
        /// </summary>
        [Display(Name = "用法")]
        public string Usage { get; set; }
        public string ItemQty { get; set; }
        public string ItemQtyUnit { get; set; }
        public string ItemQtyUnit2 { get; set; }
        public string ItemSpecQty { get; set; }
        public string ItemSpecQtyUnit { get; set; }
        /// <summary>
        /// 申請數量單位代號
        /// </summary>
        public string UNIT { get; set; }
        /// <summary>
        /// 規格數量單位代號
        /// </summary>
        public string SPECUNIT { get; set; }
        /// <summary>
        /// 總數量
        /// </summary>
        [Display(Name = "總數量")]
        public string AllQty { get; set; }
    }
}