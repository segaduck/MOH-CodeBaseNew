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
using ES.Models.ViewModels;

namespace ES.Areas.BACKMIN.Models
{
    public class Apply_005001ViewModel
    {
        public Apply_005001ViewModel()
        {
            this.Form = new Apply_005001FormModel();
            this.Detail = new Apply_005001FormModel();

            DI = new GoodsDynamicGrid<Apply_005001_DiViewModel>();
            DI.APP_ID = Form.APP_ID;
            DI.model = new Apply_005001_DiViewModel();
            DI.GetGoodsList();
            DI.SourceModelName = "DI";
            DI.IsReadOnly = false;
            DI.IsNewOpen = true;
            DI.IsDeleteOpen = true;

            PC = new GoodsDynamicGrid<Apply_005001_PcViewModel>();
            PC.APP_ID = Form.APP_ID;
            PC.model = new Apply_005001_PcViewModel();
            PC.GetGoodsList();
            PC.SourceModelName = "PC";
            PC.IsReadOnly = false;
            PC.IsNewOpen = true;
            PC.IsDeleteOpen = true;
        }

        public Apply_005001ViewModel(string APP_ID)
        {
            this.Form = new Apply_005001FormModel();
            this.Detail = new Apply_005001FormModel();

            DI = new GoodsDynamicGrid<Apply_005001_DiViewModel>();
            DI.APP_ID = APP_ID;
            DI.model = new Apply_005001_DiViewModel();
            DI.GetGoodsList();
            DI.SourceModelName = "DI";
            DI.IsReadOnly = false;
            DI.IsNewOpen = true;
            DI.IsDeleteOpen = true;

            PC = new GoodsDynamicGrid<Apply_005001_PcViewModel>();
            PC.APP_ID = APP_ID;
            PC.model = new Apply_005001_PcViewModel();
            PC.GetGoodsList();
            PC.SourceModelName = "PC";
            PC.IsReadOnly = false;
            PC.IsNewOpen = true;
            PC.IsDeleteOpen = true;
        }

        public Apply_005001FormModel Form { get; set; }

        public Apply_005001FormModel Detail { get; set; }

        public Apply_005001PreviewModel Preview { get; set; }

        /// <summary>
        /// 成分內容
        /// </summary>
        public GoodsDynamicGrid<Apply_005001_DiViewModel> DI { get; set; }

        /// <summary>
        /// 賦形劑
        /// </summary>
        public GoodsDynamicGrid<Apply_005001_PcViewModel> PC { get; set; }
    }

    public class Apply_005001FormModel : ApplyModel
    {
        /// <summary>
        /// 申請日期(民國)
        /// </summary>
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
        /// 申請份數
        /// </summary>
        public string PAYCOUNT { get; set; }

        /// <summary>
        /// 繳費金額
        /// </summary>
        [Display(Name = "繳費金額")]
        public int PAYAMOUNT { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        [Display(Name = "公司名稱")]
        public string NAME { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Display(Name = "聯絡人姓名")]
        public string CNT_NAME { get; set; }

        #region 連絡電話

        /// <summary>
        /// 連絡電話(區域號碼)
        /// </summary>
        [Display(Name = "連絡電話區域號碼")]
        public string TEL_BEFORE { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL_AFTER { get; set; }

        /// <summary>
        /// 分機
        /// </summary>
        public string TEL_Extension { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL { get; set; }

        #endregion

        #region 傳真

        /// <summary>
        /// 傳真電話(區域號碼)
        /// </summary>
        public string FAX_BEFORE { get; set; }

        /// <summary>
        /// 傳真電話
        /// </summary>
        public string FAX_AFTER { get; set; }

        /// <summary>
        /// 傳真分機
        /// </summary>
        public string FAX_Extension { get; set; }

        /// <summary>
        /// 傳真電話
        /// </summary>
        public string FAX { get; set; }

        #endregion

        #region EMAIL

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_BEFORE { get; set; }

        /// <summary>
        /// EMAIL 其他MAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_CUSTOM { get; set; }

        /// <summary>
        /// EMAIL網域
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_ADDR { get; set; }

        /// <summary>
        /// EMAIL網域
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_ADDR_TEXT { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL { get; set; }

        #endregion

        #region 藥商許可執照字號

        /// <summary>
        /// 藥商許可執照字號
        /// </summary>
        [Display(Name = "藥商許可執照字號(中文)")]
        public string PL_CD { get; set; }

        /// <summary>
        /// 藥商許可執照字號
        /// </summary>
        public string PL_CD_TEXT { get; set; }

        /// <summary>
        /// 藥商許可執照字號
        /// </summary>
        [Display(Name = "第")]
        public string PL_Num { get; set; }

        /// <summary>
        /// 藥商許可執照字號(英文)
        /// </summary>
        [Display(Name = "藥商許可執照字號(英文)")]
        public string PL_CD_E { get; set; }

        /// <summary>
        /// 藥商許可執照字號(英文)
        /// </summary>
        public string PL_CD_E_TEXT { get; set; }

        /// <summary>
        /// 藥商許可執照字號(英文)
        /// </summary>
        [Display(Name = "第")]
        public string PL_Num_E { get; set; }

        #endregion

        /// <summary>
        /// 製造廠名稱(中文)
        /// </summary>
        [Display(Name = "製造廠名稱(中文)")]
        public string MF_CNT_NAME { get; set; }

        /// <summary>
        /// 製造廠名稱(英文)
        /// </summary>
        [Display(Name = "製造廠名稱(英文)")]
        public string MF_CNT_NAME_E { get; set; }

        #region 製造廠地址(中文)
        [Display(Name = "製造廠地址(中文)")]
        public string TAX_ORG_CITY_CODE { get; set; }

        [Display(Name = "製造廠地址區域名稱")]
        public string TAX_ORG_CITY_TEXT { get; set; }

        [Display(Name = "製造廠地址")]
        public string TAX_ORG_CITY_DETAIL { get; set; }
        #endregion 地址

        /// <summary>
        /// 製造廠地址(中文)
        /// </summary>
        [Display(Name = "製造廠地址(中文)")]
        public string MF_ADDR { get; set; }

        /// <summary>
        /// 製造廠地址(英文)
        /// </summary>
        [Display(Name = "製造廠地址(英文)")]
        public string MF_ADDR_E { get; set; }

        /// <summary>
        /// 藥品名稱(中文)
        /// </summary>
        [Display(Name = "藥品名稱(中文)")]
        public string DRUG_NAME { get; set; }

        /// <summary>
        /// 藥品名稱(英文)
        /// </summary>
        [Display(Name = "藥品名稱(英文)")]
        public string DRUG_NAME_E { get; set; }

        /// <summary>
        /// 外銷品名勾選
        /// </summary>
        [Display(Name = "登載外銷品名")]
        public string DRUG_ABROAD_CHECK { get; set; }

        /// <summary>
        /// 外銷品名勾選
        /// </summary>
        public bool IS_DRUG_ABROAD_CHECK
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.DRUG_ABROAD_CHECK) ? this.DRUG_ABROAD_CHECK.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    DRUG_ABROAD_CHECK = "Y";
                }
                else
                {
                    DRUG_ABROAD_CHECK = "N";
                }
            }
        }

        /// <summary>
        /// 外銷品名(中文)
        /// </summary>
        [Display(Name = "外銷品名(中文)")]
        public string DRUG_ABROAD_NAME { get; set; }

        /// <summary>
        /// 外銷品名(英文)
        /// </summary>
        [Display(Name = "外銷品名(英文)")]
        public string DRUG_ABROAD_NAME_E { get; set; }

        /// <summary>
        /// 劑型(中文)
        /// </summary>
        [Display(Name = "劑型(中文)")]
        public string DOSAGE_FORM { get; set; }

        /// <summary>
        /// 劑型(英文)
        /// </summary>
        [Display(Name = "劑型(英文)")]
        public string DOSAGE_FORM_E { get; set; }

        /// <summary>
        /// 核准日期(中文)
        /// </summary>
        [Display(Name = "核准日期(中文)")]
        public string ISSUE_DATE { get; set; }

        public string ISSUE_DATE_TW
        {
            get
            {
                if (string.IsNullOrEmpty(ISSUE_DATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(ISSUE_DATE));
                }
            }
            set
            {
                ISSUE_DATE = HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value));
            }
        }

        /// <summary>
        /// 有效日期勾選
        /// </summary>
        [Display(Name = "顯示有效日期")]
        public string EXPIR_DATE_CHECK { get; set; }

        /// <summary>
        /// 有效日期勾選
        /// </summary>
        public bool IS_EXPIR_DATE_CHECK
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.EXPIR_DATE_CHECK) ? this.EXPIR_DATE_CHECK.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    EXPIR_DATE_CHECK = "Y";
                }
                else
                {
                    EXPIR_DATE_CHECK = "N";
                }
            }
        }

        /// <summary>
        /// 有效日期(中文)
        /// </summary>
        [Display(Name = "有效日期(中文)")]
        public string EXPIR_DATE { get; set; }

        public string EXPIR_DATE_TW
        {
            get
            {
                if (string.IsNullOrEmpty(EXPIR_DATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(EXPIR_DATE));
                }
            }
            set
            {
                EXPIR_DATE = HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(value));
            }
        }

        /// <summary>
        /// 處方說明(中文)
        /// </summary>
        [Display(Name = "處方說明(中文)")]
        public string MF_CONT { get; set; }

        /// <summary>
        /// 處方說明(英文)
        /// </summary>
        [Display(Name = "處方說明(英文)")]
        public string MF_CONT_E { get; set; }

        /// <summary>
        /// 是否為濃縮製劑勾選
        /// </summary>
        [Display(Name = "是否為濃縮製劑勾選")]
        public string Concentrate_CHECK { get; set; }

        /// <summary>
        /// 是否為濃縮製劑勾選
        /// </summary>
        public bool IS_Concentrate_CHECK
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.Concentrate_CHECK) ? this.Concentrate_CHECK.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    Concentrate_CHECK = "Y";
                }
                else
                {
                    Concentrate_CHECK = "N";
                }
            }
        }

        /// <summary>
        /// 生藥與浸膏比例(中文)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(中文)")]
        public string PC_SCALE { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(英文)
        /// </summary>
        [Display(Name = "生藥與浸膏比例(英文)")]
        public string PC_SCALE_EN { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(分量)
        /// </summary>
        public string PC_SCALE_1 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(中文單位)
        /// </summary>
        public string PC_SCALE_1E { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(英文單位)
        /// </summary>
        public string PC_SCALE_2E { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例1)
        /// </summary>
        public string PC_SCALE_21 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例2)
        /// </summary>
        public string PC_SCALE_22 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例3)
        /// </summary>
        public string PC_SCALE_23 { get; set; }

        /// <summary>
        /// 生藥與浸膏比例(比例4)
        /// </summary>
        public string PC_SCALE_24 { get; set; }

        /// <summary>
        /// 適應症勾選
        /// </summary>
        public string INDIOCATION_CHECK { get; set; }

        /// <summary>
        /// 適應症勾選
        /// </summary>
        public bool IS_INDIOCATION_CHECK
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.INDIOCATION_CHECK) ? this.INDIOCATION_CHECK.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    INDIOCATION_CHECK = "Y";
                }
                else
                {
                    INDIOCATION_CHECK = "N";
                }
            }
        }

        /// <summary>
        /// 效能勾選
        /// </summary>
        public string EFFICACY_CHECK { get; set; }

        /// <summary>
        /// 效能勾選
        /// </summary>
        public bool IS_EFFICACY_CHECK
        {
            get { return ("Y".Equals(!string.IsNullOrEmpty(this.EFFICACY_CHECK) ? this.EFFICACY_CHECK.ToUpper() : "N") ? true : false); }
            set
            {
                if (value)
                {
                    EFFICACY_CHECK = "Y";
                }
                else
                {
                    EFFICACY_CHECK = "N";
                }
            }
        }

        /// <summary>
        /// 登載效能、適應症
        /// </summary>
        [Display(Name = "登載效能、適應症")]
        public string INDIOCATION_AND_EFFICACY { get; set; }

        /// <summary>
        /// (中文)
        /// </summary>
        [Display(Name = "適應症(中文)")]
        public string INDIOCATION { get; set; }

        /// <summary>
        /// 適應症(英文)
        /// </summary>
        [Display(Name = "適應症(英文)")]
        public string INDIOCATION_E { get; set; }

        /// <summary>
        /// 效能(中文)
        /// </summary>
        [Display(Name = "效能(中文)")]
        public string EFFICACY { get; set; }

        /// <summary>
        /// 效能(英文)
        /// </summary>
        [Display(Name = "效能(英文)")]
        public string EFFICACY_E { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string RADIOYN { get; set; }

        #region 藥品許可證影本(正面)

        /// <summary>
        /// 藥品許可證影本(正面)
        /// </summary>
        [Display(Name = "藥品許可證影本(正面)")]
        public HttpPostedFileBase File_1 { get; set; }

        /// <summary>
        /// 藥品許可證影本(正面)
        /// </summary>
        [Display(Name = "藥品許可證影本(正面)")]
        public string FILE1 { get; set; }
        public string FILE1_TEXT { get; set; }

        [Display(Name = "藥品許可證影本(正面)")]
        public string File_1_Name { get; set; }

        #endregion

        #region 藥品許可證影本(反面)

        /// <summary>
        /// 藥品許可證影本(反面)
        /// </summary>
        [Display(Name = "藥品許可證影本(反面)")]
        public HttpPostedFileBase File_2 { get; set; }

        /// <summary>
        /// 藥品許可證影本(反面)
        /// </summary>
        [Display(Name = "藥品許可證影本(反面)")]
        public string FILE2 { get; set; }
        public string FILE2_TEXT { get; set; }

        [Display(Name = "藥品許可證影本(反面)")]
        public string File_2_Name { get; set; }

        #endregion

        #region 處方之中藥材中英文對照表

        /// <summary>
        /// 處方之中藥材中英文對照表
        /// </summary>
        [Display(Name = "處方之中藥材中英文對照表")]
        public HttpPostedFileBase File_3 { get; set; }

        /// <summary>
        /// 處方之中藥材中英文對照表
        /// </summary>
        [Display(Name = "處方之中藥材中英文對照表")]
        public string FILE3 { get; set; }
        public string FILE3_TEXT { get; set; }

        [Display(Name = "處方之中藥材中英文對照表")]
        public string File_3_Name { get; set; }

        #endregion

        /// <summary>
        /// 補件狀態
        /// </summary>
        public bool IS_CASE_LOCK { get; set; }

        /// <summary>
        /// 案件歷程
        /// </summary>
        public string APPLY_STATUS { get; set; }

        /// <summary>
        /// 案件狀態
        /// </summary>
        public string CODE_CD { get; set; }

        /// <summary>
        /// 案件狀態
        /// </summary>
        public string CODE_CD_TEXT { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        public string ADMIN_NAME { get; set; }

        /// <summary>
        /// 繳費日期
        /// </summary>
        [Display(Name = "繳費日期")]
        public string PAY_ACT_TIME { get; set; }

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
                if (value)
                {
                    PAY_STATUS = "Y";
                }
                else
                {
                    PAY_STATUS = "N";
                }
            }
        }

        /// <summary>
        /// 成分內容
        /// </summary>
        [Display(Name = "成分內容")]
        public string INGREDIENT_CONTENT { get; set; }

        /// <summary>
        /// 賦形劑
        /// </summary>
        [Display(Name = "賦形劑")]
        public string EXCIPIENT { get; set; }

        [Display(Name = "公文日期")]
        public string MOHW_CASE_DATE { get; set; }
    }

    /// <summary>
    /// 檔案預覽
    /// </summary>
    public class Apply_005001PreviewModel : ApplyModel
    {
        public string FILENAME { get; set; }

        public string APP_ID { get; set; }

        public string FILE_NO { get; set; }

        public string SRC_NO { get; set; }
    }
}