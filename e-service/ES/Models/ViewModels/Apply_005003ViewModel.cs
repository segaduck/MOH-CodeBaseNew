using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Commons;
using ES.Models.Entities;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_005003ViewModel
    {
        //Apply_005003Model

        /// <summary>
        /// 案號
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle = true, block_toggle_id = "BaseData", toggle_name = "申辦表件填寫", block_toggle_group = 1)]
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        public Apply_005003ViewModel()
        {
            this.Form = new Apply_005003FormModel();
            this.FormBack = new Apply_005003AppDocModel();
            this.Detail = new Apply_005003AppDocModel();

            F11 = new GoodsDynamicGrid<Apply_005003_F11ViewModel>();
            F11.APP_ID = this.APP_ID ?? "";
            F11.model = new Apply_005003_F11ViewModel();
            F11.GetGoodsList();
            F11.SourceModelName = "F11";
            F11.IsReadOnly = false;
            F11.IsNewOpen = true;
            F11.IsDeleteOpen = true;
        }

        public Apply_005003ViewModel(string APP_ID)
        {
            this.Form = new Apply_005003FormModel();
            this.FormBack = new Apply_005003AppDocModel();
            this.Detail = new Apply_005003AppDocModel();

            this.APP_ID = APP_ID;
            F11 = new GoodsDynamicGrid<Apply_005003_F11ViewModel>();
            F11.APP_ID = this.APP_ID ?? "";
            F11.model = new Apply_005003_F11ViewModel();
            F11.GetGoodsList();
            F11.SourceModelName = "F11";
            F11.IsReadOnly = false;
            F11.IsNewOpen = true;
            F11.IsDeleteOpen = true;

        }
        public Apply_005003FormModel Form { get; set; }
        public Apply_005003AppDocModel FormBack { get; set; }
        public Apply_005003AppDocModel Detail { get; set; }

        /// <summary>
        /// 成分內容
        /// </summary>
        public GoodsDynamicGrid<Apply_005003_F11ViewModel> F11 { get; set; }

    }

    public class Apply_005003FormModel : ApplyModel
    {

        /// <summary>
        /// 是否上傳附件 (0:不上傳 / 1:上傳 )
        /// </summary>
        public string IsUpLoadFile { get; set; }

        public Apply_005003FormModel()
        {
            F11 = new GoodsDynamicGrid<Apply_005003_F11ViewModel>();
            if (this.APP_ID != null) { F11.APP_ID = this.APP_ID; }
            F11.model = new Apply_005003_F11ViewModel();
            F11.GetGoodsList();
            F11.SourceModelName = "F11";
            F11.IsReadOnly = false;
            F11.IsNewOpen = true;
            F11.IsDeleteOpen = true;
        }

        public Apply_005003FormModel(string APP_ID)
        {
            this.APP_ID = APP_ID;
            F11 = new GoodsDynamicGrid<Apply_005003_F11ViewModel>();
            if (this.APP_ID != null) { F11.APP_ID = this.APP_ID; }
            F11.model = new Apply_005003_F11ViewModel();
            F11.GetGoodsList();
            F11.SourceModelName = "F11";
            F11.IsReadOnly = false;
            F11.IsNewOpen = true;
            F11.IsDeleteOpen = true;
        }

        /// <summary>
        /// 補件欄位字串
        /// </summary>
        [Display(Name = "補件欄位字串")]
        public string FieldStr { get; set; }

        /// <summary>
        /// 申請日期(民國)
        /// </summary>
        [Display(Name = "申請日期(民國)")]
        public string APP_TIME_TW
        {
            get { return (APP_TIME.HasValue ? HelperUtil.DateTimeToTwString(APP_TIME) : null); }
            set { APP_TIME = HelperUtil.TransTwToDateTime(value); }
        }

        /// <summary>
        /// 申請份數
        /// </summary>
        [Display(Name = "申請份數")]
        public string PAYCOUNT { get; set; }

        /// <summary>
        /// 繳費金額
        /// </summary>
        [Display(Name = "繳費金額")]
        [Required]
        public int PAYAMOUNT { get; set; }

        /// <summary>
        /// 公司名稱
        /// </summary>
        [Display(Name = "公司名稱")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 聯絡人姓名
        /// </summary>
        [Display(Name = "聯絡人姓名")]
        [Required]
        public string CNT_NAME { get; set; }


        /// <summary>
        /// 連絡電話(區域號碼)
        /// </summary>
        [Display(Name = "連絡電話-區域號碼")]
        public string TEL_0 { get; set; }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        public string TEL_1 { get; set; }

        /// <summary>
        /// 分機
        /// </summary>
        [Display(Name = "連絡電話-分機")]
        public string TEL_2 { get; set; }

        //[Display(Name = "連絡電話")]
        //[Required]
        //public string TEL { get; set; }


        /// <summary>
        /// 傳真電話(區域號碼)
        /// </summary>
        [Display(Name = "傳真-區域號碼")]
        public string FAX_0 { get; set; }

        /// <summary>
        /// 傳真電話
        /// </summary>
        [Display(Name = "傳真")]
        public string FAX_1 { get; set; }

        /// <summary>
        /// 傳真分機
        /// </summary>
        [Display(Name = "傳真-分機")]
        public string FAX_2 { get; set; }

        /// <summary>
        /// 傳真電話
        /// </summary>
        //public string FAX { get; set; }

        #region EMAIL

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_0 { get; set; }

        /// <summary>
        /// EMAIL 其他MAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_1 { get; set; }

        /// <summary>
        /// EMAIL網域
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_2 { get; set; }

        /// <summary>
        /// EMAIL網域
        /// </summary>
        [Display(Name = "EMAIL")]
        public string EMAIL_3 { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        [Required]
        public string EMAIL { get; set; }
        #endregion

        //[Display(Name = "案件號碼")]
        //public string APP_ID { get; set; }

        /// <summary>
        /// *申辦份數
        /// </summary>
        [Display(Name = "申辦份數")]
        public int? COPIES { get; set; }
        /// <summary>
        /// ??地址-外銷國家-(暫用)
        /// </summary>
        [Display(Name = "外銷國家")]
        public string MF_ADDR { get; set; }
        /// <summary>
        /// *2A.1藥品許可證字號-字
        /// </summary>
        [Display(Name = "*2A.1藥品許可證字號")]
        public string F_2A_1_WORD { get; set; }

        //[Display(Name = "*2A.1藥品許可證字號")]
        //public string F_2A_1_WORD_TEXT { get; set; }

        /// <summary>
        /// *2A.1藥品許可證字號-號
        /// </summary>
        [Display(Name = "*2A.1藥品許可證字號")]
        public string F_2A_1_NUM { get; set; }
        /// <summary>
        /// 1.2本產品是否獲准在國內販售？
        /// </summary>
        [Display(Name = "1.2本產品是否獲准在國內販售？")]
        public string F_1_2 { get; set; }
        /// <summary>
        /// 1.藥品名稱
        /// </summary>
        [Display(Name = "1.藥品名稱")]
        public string F_1 { get; set; }
        /// <summary>
        /// *劑型
        /// </summary>
        [Display(Name = "劑型")]
        public string F_1_DF { get; set; }
        /// <summary>
        /// *1.1處方說明
        /// </summary>
        [Display(Name = "1.1處方說明")]
        public string F_1_1 { get; set; }

        /// <summary>
        /// 1.3本產品是否有在國內販售？
        /// </summary>
        [Display(Name = "1.3本產品是否有在國內販售？")]
        public string F_1_3 { get; set; }

        /// <summary>
        /// *2A.1核准日期
        /// </summary>
        [Display(Name = "2A.1核准日期")]
        public DateTime? F_2A_1_DATE { get; set; }

        /// <summary>
        ///  2A.1核准日期
        /// </summary>
        public string F_2A_1_DATE_AD
        {
            get
            {
                return (F_2A_1_DATE.HasValue ? HelperUtil.DateTimeToString(F_2A_1_DATE.Value) : null);
            }
            set
            {
                if (F_2A_1_DATE.HasValue)
                    value = HelperUtil.DateTimeToString(F_2A_1_DATE);
                if (!string.IsNullOrWhiteSpace(value))
                    F_2A_1_DATE = HelperUtil.TransToDateTime(value);
            }
        }

        /// <summary>
        /// 2A.1核准日期
        /// </summary>
        [Display(Name = "2A.1核准日期")]
        public string F_2A_1_DATE_TW
        {
            get
            {
                return (F_2A_1_DATE.HasValue ? HelperUtil.DateTimeToTwString(F_2A_1_DATE.Value) : null);
            }
            set
            {
                if (F_2A_1_DATE.HasValue)
                    value = HelperUtil.DateTimeToTwString(F_2A_1_DATE);
                if (!string.IsNullOrWhiteSpace(value))
                    F_2A_1_DATE = HelperUtil.TransTwToDateTime(value);
            }
        }

        /// <summary>
        /// 2A.2/2A.3.1/2B.2.1製造廠名稱
        /// </summary>
        [Display(Name = "2A.2/2A.3.1/2B.2.1製造廠名稱")]
        public string F_2A_3_1_NAME { get; set; }
        /// <summary>
        /// *2A.2/2A.3.1/2B.2.1製造廠地址
        /// </summary>
        [Display(Name = "2A.2/2A.3.1/2B.2.1製造廠地址")]
        public string F_2A_3_2_ADDR { get; set; }
        /// <summary>
        /// *是否為委託製造？
        /// </summary>
        [Display(Name = "是否為委託製造？")]
        public string F_2A_2_COMM { get; set; }
        /// <summary>
        /// *2A.2藥商名稱
        /// </summary>
        [Display(Name = "2A.2藥商名稱")]
        public string F_2A_2 { get; set; }
        /// <summary>
        /// *2A.2藥商地址
        /// </summary>
        [Display(Name = "2A.2藥商地址")]
        public string F_2A_2_ADDR { get; set; }

        /// <summary>
        /// *2A.3藥品許可證持有者之類別
        /// </summary>
        [Display(Name = "2A.3藥品許可證持有者之類別")]
        public string F_2A_3 { get; set; }
        public bool F_2A_3_CHK_A { get; set; }
        public bool F_2A_3_CHK_B { get; set; }
        public bool F_2A_3_CHK_C { get; set; }

        /// <summary>
        /// *2A.4該藥品許可證是否有經認可之試驗佐證？
        /// </summary>
        [Display(Name = "")]
        public string F_2A_4 { get; set; }
        /// <summary>
        /// *2A.5所附產品資訊是否完整且與藥品許可證一致？
        /// </summary>
        [Display(Name = "*2A.5所附產品資訊是否完整且與藥品許可證一致？")]
        public string F_2A_5 { get; set; }

        /// <summary>
        /// *2A.6申請者是否為藥品許可證持有者？
        /// </summary>
        [Display(Name = "*2A.6申請者是否為藥品許可證持有者？")]
        public string F_2A_6 { get; set; }
        /// <summary>
        /// 2A.6/2B.1申請者之公司名稱
        /// </summary>
        [Display(Name = "2A.6/2B.1申請者之公司名稱")]
        public string F_2A_6_NAME { get; set; }
        /// <summary>
        /// 2A.6/2B.1申請者之公司地址
        /// </summary>
        [Display(Name = "2A.6/2B.1申請者之公司地址")]
        public string F_2A_6_ADDR { get; set; }

        public string F_2B_1_NAME { get; set; }
        public string F_2B_2_ADDR { get; set; }

        /// <summary>
        /// *2B.2申請者之類別
        /// </summary>
        [Display(Name = "2B.2申請者之類別")]
        public string F_2B_2 { get; set; }
        public bool F_2B_2_CHK_A { get; set; }
        public bool F_2B_2_CHK_B { get; set; }
        public bool F_2B_2_CHK_C { get; set; }


        /// <summary>
        /// *2B.3僅供外銷專用之原因
        /// </summary>
        [Display(Name = "2B.3僅供外銷專用之原因")]
        public string F_2B_3 { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        [Display(Name = "備註")]
        public string F_2B_3_REMARKS { get; set; }

        /// <summary>
        /// *3.製造廠是否定期接受本部之GMP查核？
        /// </summary>
        [Display(Name = "3.製造廠是否定期接受本部之GMP查核？")]
        public string F_3_0 { get; set; }
        /// <summary>
        /// *3.1接受定期查核之週期為何？
        /// </summary>
        [Display(Name = "*3.1接受定期查核之週期為何？")]
        public string F_3_1 { get; set; }
        /// <summary>
        /// 3.2申請案藥品許可證之劑型，是否經過本部查核？
        /// </summary>
        [Display(Name = "3.2申請案藥品許可證之劑型，是否經過本部查核？")]
        public string F_3_2 { get; set; }
        /// <summary>
        /// *3.3申請案藥品許可證之製造設備及製程，是否符合WHO建議之GMP規範？
        /// </summary>
        [Display(Name = "*3.3申請案藥品許可證之製造設備及製程，是否符合WHO建議之GMP規範？")]
        public string F_3_3 { get; set; }
        /// <summary>
        /// *4.申請者所提供之資訊，是否符合外銷對象對產品製造所有方面的標準？
        /// </summary>
        [Display(Name = "*4.申請者所提供之資訊，是否符合外銷對象對產品製造所有方面的標準？")]
        public string F_4 { get; set; }

        /// <summary>
        /// 附件名稱 500
        /// </summary>
        [Display(Name = "附件名稱")]
        public string ATTACH_1 { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; } //varchar 1 允許

        #region 上傳檔案   
        /// <summary>
        /// 藥品許可證影本(正面)
        /// </summary>
        [Display(Name = "藥品許可證影本(正面)")]
        public HttpPostedFileBase FILE_LICF { get; set; }
        public string FILE_LICF_TEXT { get; set; }

        /// <summary>
        /// 藥品許可證影本(反面)
        /// </summary>
        [Display(Name = "藥品許可證影本(反面)")]
        public HttpPostedFileBase FILE_LICB { get; set; }
        public string FILE_LICB_TEXT { get; set; }

        /// <summary>
        /// 處方之中藥材中英文對照表。
        /// </summary>
        [Display(Name = "處方之中藥材中英文對照表。")]
        public HttpPostedFileBase FILE_CHART { get; set; }
        public string FILE_CHART_TEXT { get; set; }

        #endregion

        /// <summary>
        /// 成分內容
        /// </summary>
        public GoodsDynamicGrid<Apply_005003_F11ViewModel> F11 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_005003_IcViewModel> IC { get; set; }


        /// <summary>
        /// 補件狀態
        /// </summary>
        public string DOCYN { get; set; }

        /// <summary>
        /// 是否為預覽
        /// </summary>
        public bool IS_PREVIEW { get; set; }

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
        /// CODE_CD=0 結案(回函核准)，CODE_CD=1 收件，CODE_CD=2 申請資料待確認，CODE_CD=3 審查中，CODE_CD=4 申請案補件中
        /// CODE_CD=10 已收案，處理中，CODE_CD=20 結案(歉難同意)，$("#Form_IS_CASE_LOCK").val() == "True"=>案件過期前台鎖定，後台開啟修改
        /// </summary>
        public string CODE_CD { get; set; }

        /// <summary>
        /// 案件狀態
        /// CODE_CD=0 結案(回函核准)，CODE_CD=1 收件，CODE_CD=2 申請資料待確認，CODE_CD=3 審查中，CODE_CD=4 申請案補件中
        /// CODE_CD=10 已收案，處理中，CODE_CD=20 結案(歉難同意)，$("#Form_IS_CASE_LOCK").val() == "True"=>案件過期前台鎖定，後台開啟修改
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
    }


    public class Apply_005003AppDocModel : Apply_005003FormModel
    {
        /// <summary>
        /// 成分內容(補件)
        /// </summary>
        public string F11_CONTENT { get; set; }
        public string FILE_LICF_NAME { get; set; }
        public string FILE_LICB_NAME { get; set; }
        public string FILE_CHART_NAME { get; set; }
    }

    public class Apply_005003RemoteDataModel
    {
        public Apply_005003RespModel WHOPMPE { get; set; }

        public string STATUS { get; set; }
    }

    public class Apply_005003RespModel
    {
        public string 製造廠名稱 { get; set; }

        public string 製造廠地址 { get; set; }

        public string 中文品名 { get; set; }

        public string 英文品名 { get; set; }

        public string 藥品劑型 { get; set; }

        public string 發證日期 { get; set; }

        public string 發證日期_英文 { get; set; }

        public string 有效日期 { get; set; }

        public string 處方說明 { get; set; }

        public string 效能 { get; set; }

        public string 適應症 { get; set; }

        public string 限制1 { get; set; }

        public string 限制2 { get; set; }

        public string 限制3 { get; set; }

        public string 限制4 { get; set; }

        public Apply_005003DRUGNAMEModel[] 外銷品名 { get; set; }

        public Apply_005003DIModel[] 成分說明 { get; set; }
    }

    public class Apply_005003DRUGNAMEModel
    {
        public string 外銷中文品名 { get; set; }

        public string 外銷英文品名 { get; set; }
    }

    public class Apply_005003DIModel
    {
        public string 成分內容 { get; set; }

        public string 份量 { get; set; }

        public string 單位 { get; set; }
    }
}
