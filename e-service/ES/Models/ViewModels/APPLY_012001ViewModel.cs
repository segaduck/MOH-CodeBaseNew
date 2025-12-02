using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using ES.DataLayers;
using ES.Services;
using System.Web.Mvc;
using ES.Utils;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_012001 檔案應用申請
    /// </summary>
    public class Apply_012001ViewModel : TblAPPLY_012001
    {
        public Apply_012001ViewModel()
        {
        }
    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_012001DoneModel
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
    /// 表單(新申請)
    /// </summary>
    public class Apply_012001FormModel : TblAPPLY_012001
    {
        public Apply_012001FormModel()
        {
            APPFIL = new GoodsDynamicGrid<APPLY_012001_APPFILModel>();
            APPFIL.APP_ID = this.APP_ID;
            APPFIL.model = new APPLY_012001_APPFILModel();
            APPFIL.GetGoodsList();
            APPFIL.SourceModelName = "APPFIL";
            APPFIL.IsReadOnly = false;
            APPFIL.IsNewOpen = true;
            APPFIL.IsDeleteOpen = true;
            this.IsNew = true;
        }

        public Apply_012001FormModel(string APP_ID)
        {
            APPFIL = new GoodsDynamicGrid<APPLY_012001_APPFILModel>();
            APPFIL.APP_ID = this.APP_ID;
            this.APP_ID = APP_ID;
            APPFIL.model = new APPLY_012001_APPFILModel();
            APPFIL.GetGoodsList();
            APPFIL.SourceModelName = "APPFIL";
            APPFIL.IsReadOnly = false;
            APPFIL.IsNewOpen = true;
            APPFIL.IsDeleteOpen = true;
            this.IsNew = true;
        }

        public bool IsNew { get; set; }
        [Control(Mode = Control.Hidden)]
        public string IsMode { get; set; }

        #region 申辦表件填寫

        #region Hidden

        [Control(Mode = Control.Hidden, block_toggle = true, toggle_name = "申辦表件填寫", block_toggle_group = 1)]
        public string APP_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRC_SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string ACC_NO { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string UNIT_CD { get; set; }

        #endregion

        #region 申請表件

        /// <summary>
        /// 申辦項目(中文顯示)
        /// </summary>
        [Display(Name = "申辦項目")]
        [Control(Mode = Control.Lable, block_toggle_group = 1)]
        public string SRV_ID_NAME
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                return dao.GetServiceName(this.SRV_ID);
            }
        }

        /// <summary>
        ///  申請單位
        /// </summary>
        [Display(Name = "申請單位")]
        [Control(Mode = Control.Lable, block_toggle_group = 1, group = 1)]
        public string APP_UNIT
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                //return dao.GetServiceUnit(this.SRV_ID);
                return "衛生福利部";
            }
        }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        [Control(Mode = Control.DatePicker, block_toggle_group = 1, IsOpenNew = true, IsReadOnly = true)]
        [Required]
        public string APP_TIME_AD
        {
            get
            {
                if (APP_TIME == null)
                {
                    return HelperUtil.DateTimeToTwString(DateTime.Now);
                }
                else
                {
                    return HelperUtil.DateTimeToString(APP_TIME);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                APP_TIME = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        [Display(Name = "申請角色")]
        [Required]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsOpenNew = true)]
        public string APP_ROLE { get; set; }

        public IList<SelectListItem> APP_ROLE_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.APP_ROLEList;
            }
        }

        [Display(Name = "姓名")]
        [Required]
        [Control(Mode = Control.TextBox, block_toggle_group = 1, IsOpenNew = true)]
        public string NAME { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        [Control(Mode = Control.DatePicker, block_toggle_group = 1, IsOpenNew = true)]
        [Required]
        public string BIRTHDAY_AD
        {
            get
            {
                if (BIRTHDAY == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(BIRTHDAY);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                BIRTHDAY = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        [Display(Name = "身分證明文件字號")]
        [Required]
        [Control(Mode = Control.TextBox, block_toggle_group = 1, IsOpenNew = true)]
        public string IDN { get; set; }


        /// <summary>
        /// 地址
        /// </summary>
        [Display(Name = "地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 1)]
        public string ADDR { get; set; }

        public string ADDR_ADDR { get; set; }

        public string ADDR_DETAIL
        {
            get
            {
                return ADDR_ADDR;
            }
            set
            {
                ADDR_ADDR = value;
            }
        }

        [Display(Name = "電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 1)]
        public string TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 1)]
        public string MAIL { get; set; }

        [Display(Name = "代理人")]
        [Required]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsOpenNew = true)]
        public string A_AGENT { get; set; }

        public IList<SelectListItem> A_AGENT_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.A_AGENTList;
            }
        }

        #endregion

        #region 自然人

        [Display(Name = "姓名")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_BIG_id = "NP", block_toggle = true, toggle_name = "自然人資料請填下列申請人欄位", block_toggle_group = 2, block_toggle_id = "NPIN")]
        public string NPIN_E_NAME { get; set; }

        [Display(Name = "與申請人關係")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2)]
        public string NPIN_AE_RELATION { get; set; }

        [Display(Name = "出生年月日")]
        [Required]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 2)]
        public string NPIN_E_BIRTHDAY_AD
        {
            get
            {
                if (E_BIRTHDAY == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(E_BIRTHDAY);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                E_BIRTHDAY = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }


        [Display(Name = "身分證明文件字號")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2)]
        public string NPIN_E_IDN { get; set; }

        [Display(Name = "地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 2)]
        public string NPIN_E_ADDR { get; set; }

        public string NPIN_E_ADDR_ADDR { get; set; }

        public string NPIN_E_ADDR_DETAIL
        {
            get
            {
                return NPIN_E_ADDR_ADDR;
            }
            set
            {
                NPIN_E_ADDR_ADDR = value;
            }
        }

        [Display(Name = "電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 2)]
        public string NPIN_E_TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 2)]
        public string NPIN_E_MAIL { get; set; }

        [Display(Name = "代理上傳委任書電子檔")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, block_toggle_group = 2, HoverFileName = "代理上傳委任書電子檔", form_id = "form_NPIN_FILE_01")]
        public HttpPostedFileBase NPIN_FILE_01 { get; set; }
        public string NPIN_FILE_01_FILENAME { get; set; }

        #endregion

        #region 法人

        [Display(Name = "法人、團體、事務所或營業所名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_BIG_id = "LP", block_toggle = true, toggle_name = "管理人或代表人資料請填下列申請人欄位", block_toggle_group = 3, block_toggle_id = "LPIN")]
        public string LPIN_E_UNIT_NAME { get; set; }

        [Display(Name = "地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 3)]
        public string LPIN_E_UNIT_ADDR { get; set; }

        public string LPIN_E_UNIT_ADDR_ADDR { get; set; }

        public string LPIN_E_UNIT_ADDR_DETAIL
        {
            get
            {
                return LPIN_E_UNIT_ADDR_ADDR;
            }
            set
            {
                LPIN_E_UNIT_ADDR_ADDR = value;
            }
        }

        [Display(Name = "代理<法人>上傳登記證影本")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, block_toggle_group = 3, HoverFileName = "代理<法人>上傳登記證影本", form_id = "form_LPIN_FILE_02")]
        public HttpPostedFileBase LPIN_FILE_02 { get; set; }
        public string LPIN_FILE_02_FILENAME { get; set; }

        #endregion 

        #region 申請檔案

        [Control(Mode = Control.Goods, block_toggle_group = 4, EditorViewName = "GoodsDynamicGrid012001")]
        public GoodsDynamicGrid<APPLY_012001_APPFILModel> APPFIL { get; set; }

        #endregion

        #region 申請事由

        //[Display(Name = "使用檔卷原件事由")]
        //[Required]
        //[Control(Mode = Control.TextArea, IsOpenNew = true, block_toggle = true, toggle_name = "申請事由", block_toggle_group = 5, columns = 80, rows = 10)]
        //public string APP_REASON { get; set; }

        [Control(Mode = Control.Hidden)]
        [Display(Name = "申請目的及用途")]
        public string CHECKNO_ITEMS { get; set; }

        public string CHECKNO { get; set; }

        [Display(Name = "申請目的及用途")]
        [Control(Mode = Control.CheckBoxList, IsOpenNew = true, block_toggle = true, toggle_name = "申請事由", block_toggle_group = 5)]
        [Required]
        public string[] CHECKNO_SHOW
        {
            get
            {
                if (this.CHECKNO != null)
                {
                    return this.CHECKNO.Replace("'", "").Split(',');
                }
                else
                {
                    return new string[0];
                }
            }
            set
            {
                if (value != null)
                {
                    value = CHECKNO_ITEMS.TONotNullString().Split(',').ToArray().Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    this.CHECKNO = string.Join(",", value.ToList());
                }
            }
        }

        /// <summary>
        /// 申請目的及用途
        /// </summary>
        public IList<CheckBoxListItem> CHECKNO_SHOW_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.CHECKNO_checkbox_list1;
            }
        }

        [Display(Name = "申請目的及用途(其他)")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 5, form_id = "dCHECKNO_NOTE")]
        public string CHECKNO_NOTE { get; set; }

        #endregion

        #endregion

        #region 檔案上傳
        /// <summary>
        /// 檔案上傳
        /// </summary>
        public string FileSave()
        {
            ShareDAO dao = new ShareDAO();
            var ErrorMsg = "";

            if (this.CHECKNO_ITEMS != null && this.CHECKNO_ITEMS.Contains("8"))
            {
                if (string.IsNullOrWhiteSpace(this.CHECKNO_NOTE))
                {
                    ErrorMsg += "申請目的及用途(其他) 為必填欄位\n";
                }
            }
            else if (this.CHECKNO_ITEMS == null)
            {
                ErrorMsg += "申請目的及用途 為必填欄位\n";
            }

            if (this.A_AGENT == "0" && this.APP_ROLE == "1")
            {
                if (this.NPIN_FILE_01.TONotNullString().Equals(""))
                {
                    ErrorMsg += "代理上傳委任書電子檔 為必填欄位\n";
                }
                else
                {
                    this.NPIN_FILE_01_FILENAME = dao.PutFile("012001", this.NPIN_FILE_01, "1").Replace("\\", "/");
                }
            }

            if (this.A_AGENT == "1" && this.APP_ROLE == "1")
            {
                if (this.LPIN_FILE_02.TONotNullString().Equals(""))
                {
                    ErrorMsg += "代理<法人>上傳登記證影本 為必填欄位\n";
                }
                else
                {
                    this.LPIN_FILE_02_FILENAME = dao.PutFile("012001", this.LPIN_FILE_02, "2").Replace("\\", "/");
                }
            }
            return ErrorMsg;
        }

        public string APPFILSave()
        {
            var ErrorMsg = string.Empty;
            if (this.APPFIL != null && this.APPFIL.GoodsList != null && this.APPFIL.GoodsList.Count() > 0)
            {
                var i = 1;
                foreach (var goods in this.APPFIL.GoodsList)
                {
                    if (string.IsNullOrWhiteSpace(goods.FILENUM))
                    {
                        ErrorMsg += $"序號{i}檔號及文號 為必填欄位\n";
                    }
                    //if (string.IsNullOrWhiteSpace(goods.FILENAME))
                    //{
                    //    ErrorMsg += $"序號{i}檔案名稱或內容要旨 為必填欄位\n";
                    //}
                    if (string.IsNullOrWhiteSpace(goods.NUMCNT))
                    {
                        ErrorMsg += $"序號{i}件數 為必填欄位\n";
                    }
                    if (string.IsNullOrWhiteSpace(goods.CHECKNO_Lst))
                    {
                        ErrorMsg += $"序號{i}申請項目 為必填欄位\n";
                    }
                    i++;
                }
            }
            return ErrorMsg;
        }

        public string EMAILSave()
        {
            var ErrorMsg = string.Empty;
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            // 自然人
            if (!string.IsNullOrWhiteSpace(this.NPIN_E_MAIL))
            {
                if (!reg3.IsMatch(this.NPIN_E_MAIL))
                {
                    ErrorMsg += "請填入正確的Email格式\n";
                }
            }
            // 自辦
            if (!string.IsNullOrWhiteSpace(this.MAIL))
            {
                if (!reg3.IsMatch(this.MAIL))
                {
                    ErrorMsg += "請填入正確的Email格式\n";
                }
            }
            // 身份證字號 申請人
            if (!string.IsNullOrWhiteSpace(this.IDN))
            {
                if (!CheckUtils.IsIdentity(this.IDN))
                {
                    ErrorMsg += "請輸入正確的國民身分證統一編號\n";
                }
            }
            // 自然人
            if (!string.IsNullOrWhiteSpace(this.NPIN_E_IDN))
            {
                if (!CheckUtils.IsIdentity(this.NPIN_E_IDN))
                {
                    ErrorMsg += "請輸入正確的國民身分證統一編號\n";
                }
            }
            return ErrorMsg;
        }
        #endregion
    }

    #region 申請檔案VIEW

    /// <summary>
    /// 申請檔案
    /// </summary>
    public class APPLY_012001_APPFILModel : TblAPPLY_012001_APPFIL
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        /// <summary>
        /// 序號
        /// </summary>
        [Display(Name = "序號")]
        public int SEQ_NO { get; set; }

        /// <summary>
        /// 檔號及文號
        /// </summary>
        [Display(Name = "檔號及文號")]
        public string FILENUM { get; set; }

        /// <summary>
        /// 檔案名稱或內容要旨
        /// </summary>
        [Display(Name = "檔案名稱或內容要旨")]
        public string FILENAME { get; set; }

        /// <summary>
        /// 件數
        /// </summary>
        [Display(Name = "件數")]
        public string NUMCNT { get; set; }

        /// <summary>
        /// 申請項目
        /// </summary>
        public string CHECKNO_Lst { get; set; }
        public string[] CHECKNO_ChekLst
        {
            get
            {
                if (this.CHECKNO_Lst != null)
                {
                    return this.CHECKNO_Lst.Replace("'", "").Split(',');
                }
                else
                {
                    return new string[0];
                }
            }
            set
            {
                if (CHECKNO_Lst != null)
                {
                    this.CHECKNO_Lst = CHECKNO_Lst;
                }
            }
        }

    }

    #endregion

    /// <summary>
    /// 確認連結
    /// </summary>
    public class Apply_012001CheckModel
    {
        /// <summary>
        /// 狀態
        /// </summary>
        public string status { get; set; }
    }

    /// <summary>
    /// 表單(補件)
    /// </summary>
    public class Apply_012001DocModel : ApplyModel
    {
        public Apply_012001DocModel()
        {
            APPFIL = new GoodsDynamicGrid<APPLY_012001_APPFILModel>();
            APPFIL.APP_ID = this.APP_ID;
            APPFIL.model = new APPLY_012001_APPFILModel();
            APPFIL.GetGoodsList();
            APPFIL.SourceModelName = "APPFIL";
            APPFIL.IsReadOnly = false;
            APPFIL.IsNewOpen = true;
            APPFIL.IsDeleteOpen = true;
        }

        public Apply_012001DocModel(string APP_ID)
        {
            APPFIL = new GoodsDynamicGrid<APPLY_012001_APPFILModel>();
            APPFIL.APP_ID = APP_ID;
            APPFIL.model = new APPLY_012001_APPFILModel();
            APPFIL.GetGoodsList();
            APPFIL.SourceModelName = "APPFIL";
            APPFIL.IsReadOnly = false;
            APPFIL.IsNewOpen = true;
            APPFIL.IsDeleteOpen = true;
        }

        /// <summary>
        /// 申辦日期
        /// </summary>
        public string ADD_TIME { get; set; }

        /// <summary>
        /// 申請角色
        /// </summary>
        public string APP_ROLE { get; set; }

        [Display(Name = "姓名")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        [Required]
        public string BIRTHDAY_AD
        {
            get
            {
                if (BIRTHDAY == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(BIRTHDAY);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                BIRTHDAY = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        [Display(Name = "身分證明文件字號")]
        [Required]
        public string IDN { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        [Required]
        public string TEL { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Required]
        public string MAIL { get; set; }

        public GoodsDynamicGrid<APPLY_012001_APPFILModel> APPFIL { get; set; }

        ///// <summary>
        ///// 使用檔卷原件事由
        ///// </summary>
        //public string APP_REASON { get; set; }

        /// <summary>
        /// 申請目的及用途
        /// </summary>
        public string CHECK_NO_LIST { get; set; }

        public string CHECK_NO_NOTE { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string TAX_ORG_CITY_CODE { get; set; }

        public string TAX_ORG_CITY_TEXT { get; set; }

        public string TAX_ORG_CITY_DETAIL { get; set; }

        /// <summary>
        /// 代理
        /// </summary>
        public string A_AGENT { get; set; }

        #region 自然人
        /// <summary>
        /// 姓名
        /// </summary>
        public string E_NAME { get; set; }

        /// <summary>
        /// 與申請人關係
        /// </summary>
        public string AE_RELATION { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        public DateTime? E_BIRTHDAY { get; set; }

        public string E_BIRTHDAY_AD
        {
            get
            {
                if (E_BIRTHDAY == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(E_BIRTHDAY);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                E_BIRTHDAY = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 身分證明文件字號
        /// </summary>
        public string E_IDN { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string E_ADDR_CODE { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string E_ADDR { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string E_TAX_ORG_CITY_CODE { get; set; }

        public string E_TAX_ORG_CITY_TEXT { get; set; }

        public string E_TAX_ORG_CITY_DETAIL { get; set; }

        /// <summary>
        /// 電話
        /// </summary>
        public string E_TEL { get; set; }

        /// <summary>
        /// E-MAIL
        /// </summary>
        public string E_MAIL { get; set; }

        /// <summary>
        /// 代理上傳委任書電子檔
        /// </summary>
        public string FILE_01 { get; set; }

        public HttpPostedFileBase newFILE_01 { get; set; }
        public string newFILE_01_TEXT { get; set; }

        #endregion

        #region 法人
        /// <summary>
        /// 法人、團體、事務所或營業所名稱
        /// </summary>
        public string E_UNIT_NAME { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string E_UNIT_ADDR_CODE { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string E_UNIT_ADDR { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string E_UNIT_TAX_ORG_CITY_CODE { get; set; }

        public string E_UNIT_TAX_ORG_CITY_TEXT { get; set; }

        public string E_UNIT_TAX_ORG_CITY_DETAIL { get; set; }

        /// <summary>
        /// 代理<法人>上傳登記證影本
        /// </summary>
        public string FILE_02 { get; set; }

        public HttpPostedFileBase newFILE_02 { get; set; }
        public string newFILE_02_TEXT { get; set; }


        #endregion

        /// <summary>
        /// 欄位補件
        /// </summary>
        public string DOC_ITEM { get; set; }

        /// <summary>
        /// 檔案補件
        /// </summary>
        public string DOC_FILE { get; set; }


        public string DocFileSave()
        {
            ShareDAO dao = new ShareDAO();
            var ErrorMsg = "";

            if (this.A_AGENT == "0" && this.APP_ROLE == "1")
            {
                if (this.newFILE_01.TONotNullString().Equals(""))
                {
                    ErrorMsg += "代理上傳委任書電子檔 為必填欄位\n";
                }
                else
                {
                    this.newFILE_01_TEXT = dao.PutFile("012001", this.newFILE_01, "1").Replace("\\", "/");
                }
            }

            if (this.A_AGENT == "1" && this.APP_ROLE == "1")
            {
                if (this.newFILE_02.TONotNullString().Equals(""))
                {
                    ErrorMsg += "代理<法人>上傳登記證影本 為必填欄位\n";
                }
                else
                {
                    this.newFILE_02_TEXT = dao.PutFile("012001", this.newFILE_02, "2").Replace("\\", "/");
                }
            }
            return ErrorMsg;
        }
    }
}