using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Commons;
using ES.DataLayers;
using ES.Services;
using System.Web.Mvc;
using ES.Models;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// APPLY_010001 生殖細胞及胚胎輸入輸出申請作業
    /// </summary>
    public class Apply_010001ViewModel : TblAPPLY_010001
    {
        public Apply_010001ViewModel()
        {
        }
    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_010001DoneModel
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
    /// 表單
    /// </summary>
    public class Apply_010001FormModel : TblAPPLY_010001
    {
        public Apply_010001FormModel()
        {
        }

        public bool IsNew { get; set; }
        [Control(Mode = Control.Hidden)]
        public string IsMode { get; set; }

        #region 申辦表件填寫

        #region Hidden

        public string APP_ID { get; set; }
        public string SRV_ID { get; set; }
        public string SRC_SRV_ID { get; set; }
        public string ACC_NO { get; set; }
        public string UNIT_CD { get; set; }

        public DateTime? APP_EXT_DATE { get; set; }

        [Display(Name = "預計完成日")]
        public string APP_EXT_DATE_AD
        {
            get
            {
                return HelperUtil.DateTimeToTwString(APP_EXT_DATE);     // YYYYMMDD 回傳給系統
            }
        }

        /// <summary>
        /// 案件承辦人姓名
        /// </summary>
        [Display(Name = "案件承辦人姓名")]
        public string PRO_ACC_NAME
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                return dao.GetAdmin(this.PRO_ACC);
            }
        }

        [Display(Name = "系統狀態")]
        public string APP_STATUS
        {
            get
            {
                BackApplyDAO dao = new BackApplyDAO();
                return dao.GetSchedule(this.APP_ID, "10");
            }
        }

        /// <summary>
        ///  承辦單位
        /// </summary>
        [Display(Name = "承辦單位")]
        public string UNIT_NAME
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                return dao.GetServiceUnit(this.SRV_ID);
            }
        }

        public string PRO_ACC { get; set; }

        public DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 申辦項目(中文顯示)
        /// </summary>
        [Display(Name = "申辦項目")]
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
        public string APP_UNIT
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                return dao.GetServiceUnit(this.SRV_ID);
            }
        }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_TIME_AD
        {
            get
            {
                if (APP_TIME == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToTwString(APP_TIME);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                APP_TIME = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        #endregion

        #region 申請表件

        [Display(Name = "申請角色")]
        [Required]
        [Control(Mode = Control.RadioGroup, block_toggle = true, block_toggle_id = "BaseData", toggle_name = "基本資料檔", block_toggle_group = 1, IsReadOnly = true)]
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
        [Control(Mode = Control.TextBox, block_toggle_group = 1, IsReadOnly = true)]
        public string NAME { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        [Control(Mode = Control.DatePicker, block_toggle_group = 1, IsReadOnly = true)]
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
        [Control(Mode = Control.TextBox, block_toggle_group = 1, IsReadOnly = true)]
        public string IDN { get; set; }


        /// <summary>
        /// 地址
        /// </summary>
        [Display(Name = "地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsReadOnly = true, block_toggle_group = 1)]
        public string ADDR_CODE { get; set; }

        public string ADDR_CODE_ADDR
        {
            get
            {
                var dao = new MyKeyMapDAO();
                KeyMapModel kmm = dao.GetCityTownName(ADDR_CODE);
                return (kmm != null ? kmm.TEXT : "");
            }
        }

        public string ADDR_CODE_DETAIL
        {
            get
            {
                return ADDR;
            }
            set
            {
                ADDR = value;
            }
        }

        [Display(Name = "電話")]
        [Control(Mode = Control.Tel, IsReadOnly = true, block_toggle_group = 1)]
        public string TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsReadOnly = true, block_toggle_group = 1)]
        public string MAIL { get; set; }

        [Display(Name = "代理人")]
        [Required]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsReadOnly = true)]
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
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_BIG_id = "NP", block_toggle = true, toggle_name = "自然人資料", block_toggle_group = 2, block_toggle_id = "NPIN")]
        public string NPIN_E_NAME { get; set; }

        [Display(Name = "與申請人關係")]
        [Required]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 2)]
        public string NPIN_AE_RELATION { get; set; }

        [Display(Name = "出生年月日")]
        [Required]
        [Control(Mode = Control.DatePicker, IsReadOnly = true, block_toggle_group = 2)]
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
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 2)]
        public string NPIN_E_IDN { get; set; }

        [Display(Name = "地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsReadOnly = true, block_toggle_group = 2)]
        public string NPIN_E_ADDR_CODE { get; set; }
        public string NPIN_E_ADDR_CODE_ADDR
        {
            get
            {
                var dao = new MyKeyMapDAO();
                KeyMapModel kmm = dao.GetCityTownName(ADDR_CODE);
                return (kmm != null ? kmm.TEXT : "");
            }
        }
        public string NPIN_E_ADDR { get; set; }

        public string NPIN_E_ADDR_CODE_DETAIL
        {
            get
            {
                return NPIN_E_ADDR;
            }
            set
            {
                NPIN_E_ADDR = value;
            }
        }

        [Display(Name = "電話")]
        [Required]
        [Control(Mode = Control.Tel, IsReadOnly = true, block_toggle_group = 2)]
        public string NPIN_E_TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsReadOnly = true, block_toggle_group = 2)]
        public string NPIN_E_MAIL { get; set; }

        //[Display(Name = "代理上傳委任書電子檔")]
        //[Control(Mode = Control.FileUpload, IsReadOnly = true, block_toggle_group = 2, HoverFileName = "代理上傳委任書電子檔")]
        //public HttpPostedFileBase NPIN_FILE_01 { get; set; }
        //public string NPIN_FILE_01_FILENAME { get; set; }

        #endregion

        #region 法人

        [Display(Name = "法人、團體、事務所或營業所名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_BIG_id = "LP", block_toggle = true, toggle_name = "管理人或代表人資料", block_toggle_group = 3, block_toggle_id = "LPIN")]
        public string LPIN_E_UNIT_NAME { get; set; }

        [Display(Name = "地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsReadOnly = true, block_toggle_group = 3)]
        public string LPIN_E_UNIT_ADDR_CODE { get; set; }

        public string LPIN_E_UNIT_ADDR_CODE_ADDR
        {
            get
            {
                var dao = new MyKeyMapDAO();
                KeyMapModel kmm = dao.GetCityTownName(ADDR_CODE);
                return (kmm != null ? kmm.TEXT : "");
            }
        }
        public string LPIN_E_UNIT_ADDR { get; set; }
        public string LPIN_E_UNIT_ADDR_CODE_DETAIL
        {
            get
            {
                return LPIN_E_UNIT_ADDR;
            }
            set
            {
                LPIN_E_UNIT_ADDR = value;
            }
        }

        //[Display(Name = "代理<法人>上傳登記證影本")]
        //[Control(Mode = Control.FileUpload, IsReadOnly = true, block_toggle_group = 3, HoverFileName = "代理<法人>上傳登記證影本")]
        //public HttpPostedFileBase LPIN_FILE_02 { get; set; }
        //public string LPIN_FILE_02_FILENAME { get; set; }

        #endregion

        #region 申請事由

        //[Display(Name = "使用檔卷原件事由")]
        //public string APP_REASON { get; set; }

        [Display(Name = "申請目的及用途")]
        public string CHECKNO_ITEMS { get; set; }

        #endregion

        #endregion

        #region 案件歷程
        [Display(Name = "案件進度")]
        public string FLOW_CD_STATUS
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                var FLOW_list = dao.GetStatuListForUnitCD8_2;
                return FLOW_list.Where(m => m.Value == FLOW_CD).Select(m => m.Text).First(); ;
            }
        }

        [Display(Name = "案件狀態修改")]
        [Required]
        public string FLOW_CD { get; set; }


        public IList<SelectListItem> FLOW_CD_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.GetStatuListForUnitCD8_2;
            }
        }

        [Display(Name = "案件歷程紀錄")]
        public string FLOW_CD_LOG { get; set; }
        #endregion


        public IList<APPLY_010001_APPFILModel> APPFIL { get; set; }

        public string FileCheck { get; set; }

        public string IsNotice { get; set; }

        /// <summary>
        /// 補件內容
        /// </summary>
        [Display(Name = "補件內容")]
        public string NOTE { get; set; }

        public string MOHW_CASE_NO { get; set; }

        public Apply_010001FileModel FileList { get; set; }

        /// <summary>
        /// EMAIL確認
        /// </summary>
        [Display(Name = "EMAIL確認")]
        public string CHECK_FLAG { get; set; }
    }

    #region 申請檔案VIEW

    /// <summary>
    /// 申請檔案
    /// </summary>
    public class APPLY_010001_APPFILModel : TblAPPLY_010001_APPFIL
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

    }

    #endregion

    #region
    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_010001FileModel
    {
        public Apply_010001FileModel()
        {
            this.FILE_1 = new FileGroupModel();
            this.FILE_2 = new FileGroupModel();
        }
        public string APP_ID { get; set; }

        /// <summary>
        /// 自然人委任書影本
        /// </summary>
        [Display(Name = "自然人委任書影本")]
        public FileGroupModel FILE_1 { get; set; }

        /// <summary>
        /// 法人登記證影本
        /// </summary>
        [Display(Name = "法人登記證影本")]
        public FileGroupModel FILE_2 { get; set; }

    }
    #endregion
}