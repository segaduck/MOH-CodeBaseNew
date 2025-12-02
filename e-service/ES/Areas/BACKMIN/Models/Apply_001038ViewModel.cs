using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// APPLY_001038 生殖細胞及胚胎輸入輸出申請作業
    /// </summary>
    public class Apply_001038ViewModel : Apply_001038Model
    {
        public Apply_001038ViewModel()
        {
        }
    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_001038DoneModel
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

    public class Apply_001038FormModel : Apply_001038Model
    {
        public Apply_001038FormModel()
        {
            GOODS = new GoodsDynamicGrid<Apply_001038_GoodsViewModel>();
            //GOODS.APP_ID = "1111";
            GOODS.APP_ID = this.APP_ID;
            GOODS.model = new Apply_001038_GoodsViewModel();
            GOODS.GetGoodsList();
            GOODS.SourceModelName = "GOODS";
            GOODS.IsReadOnly = false;
            GOODS.IsNewOpen = true;
            GOODS.IsDeleteOpen = true;
            this.IsNew = true;
        }

        public Apply_001038FormModel(string APP_ID)
        {
            GOODS = new GoodsDynamicGrid<Apply_001038_GoodsViewModel>();
            //GOODS.APP_ID = "1111";
            GOODS.APP_ID = APP_ID;
            this.APP_ID = APP_ID;
            GOODS.model = new Apply_001038_GoodsViewModel();
            GOODS.GetGoodsList();
            GOODS.SourceModelName = "GOODS";
            GOODS.IsReadOnly = false;
            GOODS.IsNewOpen = true;
            GOODS.IsDeleteOpen = true;
            this.IsNew = true;
        }

        public bool IsNew { get; set; }
        [Control(Mode = Control.Hidden)]
        public string IsMode { get; set; }

        #region 申辦表件填寫
        /// <summary>
        /// 案號
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle = true, block_toggle_id = "BaseData",toggle_name = "申辦表件填寫", block_toggle_group = 1)]
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRC_SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string ACC_NO { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string UNIT_CD { get; set; }


        public DateTime? APP_EXT_DATE { get; set; }

        [Display(Name = "預計完成日")]
        public string APP_EXT_DATE_AD
        {
            get
            {
                return HelperUtil.DateTimeToString(APP_EXT_DATE);     // YYYYMMDD 回傳給系統
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
                return dao.GetSchedule(this.APP_ID, "11");
            }
        }

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
        ///  承辦單位
        /// </summary>
        [Display(Name = "承辦單位")]
        [Control(Mode = Control.Lable, block_toggle_group = 1, group = 1)]
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
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        [Control(Mode = Control.DatePicker, block_toggle_group = 1, group = 1, IsOpenNew = true)]
        [Required]
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
                    return HelperUtil.DateTimeToString(APP_TIME);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                APP_TIME = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }
        

        /// <summary>
        /// 進出口別
        /// </summary>
        [Display(Name = "申請類型")]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsOpenNew = true,group =2)]
        [Required]
        public string IM_EXPORT { get; set; }

        public IList<SelectListItem> IM_EXPORT_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.IM_EXPORT_list;
            }
        }

        /// <summary>
        ///  統一編號
        /// </summary>
        [Display(Name = "統一編號")]
        [Control(Mode = Control.Lable, block_toggle_group = 1, group = 2)]
        public string IDN { get; set; }


        #endregion


        #region 輸入
        [Display(Name = "原儲存機構名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_BIG_id = "IMPORT",  block_toggle = true, toggle_name = "生殖細胞及胚胎輸入", block_toggle_group = 2, group = 2, block_toggle_id = "DestIn")]
        public string Dest_ORG_UNITNAME { get; set; }

        [Display(Name = "原儲存機構國別")]
        [Required]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_group = 2, group = 3)]
        public string Dest_DEST_STATE_ID { get; set; }

        public IList<SelectListItem> Dest_DEST_STATE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.CountryList;
            }
        }

        [Display(Name = "原儲存機構聯絡人")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 3)]
        public string Dest_ORG_NAME { get; set; }

        [Display(Name = "電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 2, group = 4)]
        public string Dest_ORG_TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 2, group = 5)]
        public string Dest_ORG_EMAIL { get; set; }

        [Display(Name = "原儲存機構之地址")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 6, size = "59")]
        public string Dest_TAI_ADDR { get; set; }

        [Display(Name = "我國承接之人工生殖機構名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 7, size = "59")]
        public string Dest_TAI_UNITNAME { get; set; }

        [Display(Name = "我國承接機構之聯絡人")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 8)]
        public string Dest_TAI_NAME { get; set; }

        [Display(Name = "人工生殖機構許可文號")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 9)]
        public string Dest_LIC_NUM { get; set; }

        [Display(Name = "承接機構電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 2, group = 10)]
        public string Dest_TAI_TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 2, group = 11)]
        public string Dest_TAI_EMAIL { get; set; }

        [Display(Name = "輸入之目的")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 12)]
        public string Dest_USE_MARK { get; set; }

        [Display(Name = "輸入生殖細胞或胚胎數量")]
        [Required]
        [Control(Mode = Control.ABCNUM, IsOpenNew = true, block_toggle_group = 2, group = 13, fontWord = "Dest_")]
        public string Dest_ABC_CHECK
        {
            get
            {
                return Dest_A_IsCheck || Dest_B_IsCheck || Dest_C_IsCheck ? "Y" : null;
            }
        }

        public bool Dest_A_IsCheck
        {
            get
            {
                return Dest_A_NAME.TONotNullString() != "";
            }
        }

        public string Dest_A_NAME { get; set; }
        public string Dest_A_NUM1 { get; set; }
        public DateTime? Dest_A_DATE { get; set; }

        public string Dest_A_DATE_AD
        {
            get
            {
                if (Dest_A_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(Dest_A_DATE);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                Dest_A_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        public bool Dest_B_IsCheck
        {
            get
            {
                return Dest_B_NAME.TONotNullString() != "";
            }
        }
        public string Dest_B_NAME { get; set; }
        public string Dest_B_NUM1 { get; set; }
        public string Dest_B_NUM2 { get; set; }
        public DateTime? Dest_B_DATE { get; set; }
        public string Dest_B_DATE_AD
        {
            get
            {
                if (Dest_B_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(Dest_B_DATE);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                Dest_B_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }


        public bool Dest_C_IsCheck
        {
            get
            {
                return Dest_C_NAME1.TONotNullString() != "";
            }
        }
        public string Dest_C_NAME1 { get; set; }
        public string Dest_C_NAME2 { get; set; }
        public string Dest_C_NUM1 { get; set; }
        public DateTime? Dest_C_DATE { get; set; }
        public string Dest_C_DATE_AD
        {
            get
            {
                if (Dest_C_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(Dest_C_DATE);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                Dest_C_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }
        public string Dest_C_DAY { get; set; }
        #endregion


        #region 輸出
        [Display(Name = "申請機構名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_BIG_id = "EXPORT",  block_toggle = true, toggle_name = "生殖細胞及胚胎輸出", block_toggle_group = 3, group = 13, block_toggle_id = "SellOut")]
        public string Sell_ORG_UNITNAME { get; set; }

        [Display(Name = "人工生殖機構聯絡人")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 3, group = 14)]
        public string Sell_ORG_NAME { get; set; }

        [Display(Name = "人工生殖機構許可文號")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 3, group = 14)]
        public string Sell_ORG_LIC_NUM { get; set; }

        [Display(Name = "電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 3, group = 15)]
        public string Sell_ORG_TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 3, group = 16)]
        public string Sell_ORG_EMAIL { get; set; }

        [Display(Name = "輸出國(地)")]
        [Required]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_group = 3, group = 17)]
        public string Sell_SELL_STATE_ID { get; set; }

        public IList<SelectListItem> Sell_SELL_STATE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.CountryList;
            }
        }

        [Display(Name = "輸出國(地)承接機構名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 3, group = 18)]
        public string Sell_OTH_UNITNAME { get; set; }

        [Display(Name = "電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 3, group = 19)]
        public string Sell_OTH_TEL { get; set; }

        [Display(Name = "E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 3, group = 20)]
        public string Sell_OTH_EMAIL { get; set; }


        [Display(Name = "地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 3, group = 23)]
        public string Sell_OTH_ZIP { get; set; }

        public string Sell_OTH_ZIP_ADDR { get; set; }

        public string Sell_OTH_ZIP_DETAIL
        {
            get
            {
                return Sell_OTH_ADDR;
            }
            set
            {
                Sell_OTH_ADDR = value;
            }
        }
        public string Sell_OTH_ADDR { get; set; }

        [Display(Name = "輸出之目的")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 3, group = 21)]
        public string Sell_USE_MARK { get; set; }

        [Display(Name = "輸出生殖細胞或胚胎數量")]
        [Required]
        [Control(Mode = Control.ABCNUM, IsOpenNew = true, block_toggle_group = 3, group = 22, fontWord = "Sell_")]
        public string Sell_ABC_CHECK
        {
            get
            {
                return Sell_A_IsCheck || Sell_B_IsCheck || Sell_C_IsCheck ? "Y" : null;
            }
        }

        public bool Sell_A_IsCheck
        {
            get
            {
                return Sell_A_NAME.TONotNullString() != "";
            }
        }
        public string Sell_A_NAME { get; set; }
        public string Sell_A_NUM1 { get; set; }
        public DateTime? Sell_A_DATE { get; set; }

        public string Sell_A_DATE_AD
        {
            get
            {
                if (Sell_A_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(Sell_A_DATE);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                Sell_A_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        public bool Sell_B_IsCheck
        {
            get
            {
                return Sell_B_NAME.TONotNullString() != "";
            }
        }
        public string Sell_B_NAME { get; set; }
        public string Sell_B_NUM1 { get; set; }
        public string Sell_B_NUM2 { get; set; }
        public DateTime? Sell_B_DATE { get; set; }

        public string Sell_B_DATE_AD
        {
            get
            {
                if (Sell_B_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(Sell_B_DATE);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                Sell_B_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }


        public bool Sell_C_IsCheck
        {
            get
            {
                return Sell_C_NAME1.TONotNullString() != "";
            }
        }
        public string Sell_C_NAME1 { get; set; }
        public string Sell_C_NAME2 { get; set; }
        public string Sell_C_NUM1 { get; set; }
        public DateTime? Sell_C_DATE { get; set; }
        public string Sell_C_DATE_AD
        {
            get
            {
                if (Sell_C_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(Sell_C_DATE);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                Sell_C_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }
        public string Sell_C_DAY { get; set; }
        #endregion


        #region 委託人資料
        /// <summary>
        /// 身份證字號
        /// </summary
        [Display(Name = "身份證字號")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle = true, block_toggle_id = "ApplyData", toggle_name = "委託人資料", block_toggle_group = 4, group = 30)]
        public string TAX_ORG_ID { get; set; }
        /// <summary>
        /// 委託人姓名
        /// </summary>
        [Display(Name = "委託人姓名")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4, group = 31)]
        public string TAX_ORG_NAME { get; set; }
        /// <summary>
        /// 委託人姓名(英文)
        /// </summary>
        //[Display(Name = "委託人姓名(英文)")]
        //[Required]
        //[Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4, group = 31)]
        //public string TAX_ORG_ENAME { get; set; }

        /// <summary>
        /// 委託人聯絡地址
        /// </summary>
        //[Display(Name = "委託人聯絡地址中文")]
        //[Required]
        //[Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 4, group = 32)]
        //public string TAX_ORG_ZIP { get; set; }

        //public string TAX_ORG_ZIP_ADDR { get; set; }

        //public string TAX_ORG_ZIP_DETAIL
        //{
        //    get
        //    {
        //        return TAX_ORG_ADDR;
        //    }
        //    set
        //    {
        //        TAX_ORG_ADDR = value;
        //    }
        //}
        [Display(Name = "委託人聯絡地址")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4, group = 32)]
        public string TAX_ORG_ADDR { get; set; }

        /// <summary>
        /// 委託人聯絡地址(英文)
        /// </summary>
        //[Display(Name = "委託人聯絡地址英文")]
        //[Required]
        //[Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4, group = 33)]
        //public string TAX_ORG_EADDR { get; set; }

        /// <summary>
        /// 委託人二，姓名(籍配偶，胚胎輸出入時填寫)
        /// </summary>
        [Display(Name = "委託人二，姓名(籍配偶，胚胎輸出入時填寫)")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4, group = 34)]
        public string TAX_ORG_MAN { get; set; }

        /// <summary>
        /// 委託人二，身分證字號或護照號碼
        /// </summary>
        [Display(Name = "委託人二，身分證字號或護照號碼")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4, group = 34)]
        public string TAX_ORG_TID { get; set; }
        /// <summary>
        /// 委託人連絡電話
        /// </summary>
        [Display(Name = "委託人聯絡電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 4, group = 35)]
        public string TAX_ORG_TEL { get; set; }
        /// <summary>
        /// 委託人聯絡email
        /// </summary>
        [Display(Name = "委託人E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 4, group = 36)]
        public string TAX_ORG_EMAIL { get; set; }

        /// <summary>
        /// 委託人聯絡傳真號碼
        /// </summary>
        [Display(Name = "委託人傳真")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 4, group = 37)]
        public string TAX_ORG_FAX { get; set; }

        [Display(Name = "起始日期")]
        [Required]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 4, group = 38)]
        public string DATE_S_AD
        {
            get
            {
                if (DATE_S == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(DATE_S);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                DATE_S = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }
        [Display(Name = "終止日期")]
        [Required]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 4, group = 38)]
        public string DATE_E_AD
        {
            get
            {
                if (DATE_E == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(DATE_E);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                DATE_E = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }
        /// <summary>
        /// 轉口港代碼
        /// </summary>
        [Display(Name = "轉口岸")]
        [Control(Mode = Control.CountryPort, IsOpenNew = true, block_toggle_group = 4, group = 39)]
        public string TRN_COUNTRY_ID { get; set; }

        public IList<SelectListItem> TRN_COUNTRY_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.PortList;
            }
        }

        public string TRN_COUNTRY_ID_PORT
        {
            get
            {
                return TRN_PORT_ID;
            }
            set
            {
                TRN_PORT_ID = value;
            }
        }

        public IList<SelectListItem> TRN_COUNTRY_ID_PORT_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.HarborList;
            }
        }

        public string TRN_PORT_ID { get; set; }

        /// <summary>
        /// 起運口岸代碼
        /// </summary>
        [Display(Name = "起運口岸")]
        [Control(Mode = Control.CountryPort, IsOpenNew = true, block_toggle_group = 4, group = 40)]
        public string BEG_COUNTRY_ID { get; set; }

        public IList<SelectListItem> BEG_COUNTRY_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.PortList;
            }
        }

        public string BEG_COUNTRY_ID_PORT
        {
            get
            {
                return BEG_PORT_ID;
            }
            set
            {
                BEG_PORT_ID = value;
            }
        }

        public IList<SelectListItem> BEG_COUNTRY_ID_PORT_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.HarborList;
            }
        }

        public string BEG_PORT_ID { get; set; }
        #endregion


        #region 貨品資料
        [Control(Mode = Control.Goods, block_toggle_group = 4, EditorViewName = "GoodsDynamicGrid001038")]
        public GoodsDynamicGrid<Apply_001038_GoodsViewModel> GOODS { get; set; }
        #endregion


        #region 其他資料
        /// <summary>
        /// 申請用途
        /// </summary
        [Display(Name = "申請用途")]
        [Required]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_id = "OtherData", block_toggle_group = 6, block_toggle = true, toggle_name = "其他資料<span style='color:red'>*當次全部檔案傳輸上傳最大容量允許為5MB</span>")]
        public string APP_USE_ID { get; set; }

        public IList<SelectListItem> APP_USE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.GetAPP_USE3List;
            }
        }

        /// <summary>
        /// 核發方式
        /// </summary>
        [Display(Name = "核發方式")]
        [Required]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_group = 6)]
        public string CONF_TYPE_ID { get; set; }

        public IList<SelectListItem> CONF_TYPE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.GetCONF_TYPEList;
            }
        }

        ///// <summary>
        ///// 佐證文件採合併檔案
        ///// </summary>
        //[Display(Name = "佐證文件採合併檔案")]
        //[Required]
        //[Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle_group = 6)]
        //public string DOC_TYP_CHK { get; set; }

        //public IList<SelectListItem> DOC_TYP_CHK_list
        //{
        //    get
        //    {
        //        ShareCodeListModel dao = new ShareCodeListModel();
        //        return dao.YorN_list;
        //    }
        //}

        /// <summary>
        /// 檢附文件類型一
        /// </summary>
        [Display(Name = "檢附文件類型一")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "委託人身分證明文件影本(如:國民身分證統一編號或護照)", block_toggle_group = 6)]
        public Boolean DOC_TYP_01_SHOW
        {
            get
            {
                return DOC_TYP_01.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_01 = value ? "B11" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號一
        /// </summary>
        [Display(Name = "檢附文件字號一")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 6)]
        public string DOC_COD_01 { get; set; }
        /// <summary>
        /// 檢附文件說明一
        /// </summary>
        [Display(Name = "檢附文件說明一")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 6)]
        public string DOC_TXT_01 { get; set; }

        /// <summary>
        /// 檢附文件說明一
        /// </summary>
        [Display(Name = "檢附文件類型一(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型一", block_toggle_group = 6)]
        public HttpPostedFileBase DOC_FILE_01 { get; set; }

        public string DOC_FILE_01_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型二
        /// </summary>
        [Display(Name = "檢附文件類型二")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "輸入或輸出申請書", block_toggle_group = 6)]
        public Boolean DOC_TYP_02_SHOW
        {
            get
            {
                return DOC_TYP_02.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_02 = value ? "B12" : null;
            }
        }
        /// <summary>
        /// 檢附文件字號二
        /// </summary>
        [Display(Name = "檢附文件字號二")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 6)]
        public string DOC_COD_02 { get; set; }
        /// <summary>
        /// 檢附文件說明二
        /// </summary>
        [Display(Name = "檢附文件說明二")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 6)]
        public string DOC_TXT_02 { get; set; }

        /// <summary>
        /// 檢附文件說明二
        /// </summary>
        [Display(Name = "檢附文件類型二(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型二", block_toggle_group = 6)]
        public HttpPostedFileBase DOC_FILE_02 { get; set; }

        public string DOC_FILE_02_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型三
        /// </summary>
        [Display(Name = "檢附文件類型三")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "委託書", block_toggle_group = 6)]
        public Boolean DOC_TYP_03_SHOW
        {
            get
            {
                return DOC_TYP_03.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_03 = value ? "B13" : null;
            }
        }
        /// <summary>
        /// 檢附文件字號三
        /// </summary>
        [Display(Name = "檢附文件字號三")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 6)]
        public string DOC_COD_03 { get; set; }
        /// <summary>
        /// 檢附文件說明三
        /// </summary>
        [Display(Name = "檢附文件說明三")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 6)]
        public string DOC_TXT_03 { get; set; }

        /// <summary>
        /// 檢附文件說明三
        /// </summary>
        [Display(Name = "檢附文件類型三(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型三", block_toggle_group = 6)]
        public HttpPostedFileBase DOC_FILE_03 { get; set; }

        public string DOC_FILE_03_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型四
        /// </summary>
        [Display(Name = "檢附文件類型四")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "切結書", block_toggle_group = 6)]
        public Boolean DOC_TYP_04_SHOW
        {
            get
            {
                return DOC_TYP_04.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_04 = value ? "B14" : null;
            }
        }
        /// <summary>
        /// 檢附文件字號四
        /// </summary>
        [Display(Name = "檢附文件字號四")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 6)]
        public string DOC_COD_04 { get; set; }
        /// <summary>
        /// 檢附文件說明四
        /// </summary>
        [Display(Name = "檢附文件說明四")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 6)]
        public string DOC_TXT_04 { get; set; }

        /// <summary>
        /// 檢附文件說明四
        /// </summary>
        [Display(Name = "檢附文件類型四(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型四", block_toggle_group = 6)]
        public HttpPostedFileBase DOC_FILE_04 { get; set; }

        public string DOC_FILE_04_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型五
        /// </summary>
        [Display(Name = "檢附文件類型五")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "輸出或輸入國(地)主管機關開立，並經我國駐外單位或其委託之機構驗證之輸出或輸入許可證明，或足以證明輸出或輸入國(地)未管制輸出或輸入文件", block_toggle_group = 6)]
        public Boolean DOC_TYP_05_SHOW
        {
            get
            {
                return DOC_TYP_05.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_05 = value ? "B15" : null;
            }
        }
        /// <summary>
        /// 檢附文件字號五
        /// </summary>
        [Display(Name = "檢附文件字號五")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 6)]
        public string DOC_COD_05 { get; set; }
        /// <summary>
        /// 檢附文件說明五
        /// </summary>
        [Display(Name = "檢附文件說明五")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 6)]
        public string DOC_TXT_05 { get; set; }

        /// <summary>
        /// 檢附文件說明五
        /// </summary>
        [Display(Name = "檢附文件類型五(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型五", block_toggle_group = 6)]
        public HttpPostedFileBase DOC_FILE_05 { get; set; }

        public string DOC_FILE_05_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型六
        /// </summary>
        [Display(Name = "檢附文件類型六")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "輸入國(地)承接儲存生殖細胞或胚胎之機構開立，並經我國駐外單位或其委託之機構驗證之承接同意書。", block_toggle_group = 6)]
        public Boolean DOC_TYP_06_SHOW
        {
            get
            {
                return DOC_TYP_06.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_06 = value ? "B16" : null;
            }
        }
        /// <summary>
        /// 檢附文件字號六
        /// </summary>
        [Display(Name = "檢附文件字號六")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 6)]
        public string DOC_COD_06 { get; set; }
        /// <summary>
        /// 檢附文件說明六
        /// </summary>
        [Display(Name = "檢附文件說明六")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 6)]
        public string DOC_TXT_06 { get; set; }

        /// <summary>
        /// 檢附文件說明六
        /// </summary>
        [Display(Name = "檢附文件類型六(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型六", block_toggle_group = 6)]
        public HttpPostedFileBase DOC_FILE_06 { get; set; }

        public string DOC_FILE_06_FILENAME { get; set; }




        /// <summary>
        /// 檔案打包下載
        /// </summary>
        [Display(Name = "檔案打包下載")]
        [Control(Mode = Control.ZipButton,CaseName = "生殖細胞及胚胎輸入輸出申請作業", block_toggle_group = 6)]
        public string ZipButton { get; set; }
        
        #endregion

        #region 案件歷程
        [Display(Name = "案件進度")]
        [Control(Mode = Control.Lable, block_toggle = true, block_toggle_id = "StatusData", toggle_name = "案件進度歷程", IsOpenNew = true, block_toggle_group = 7)]
        public string FLOW_CD_STATUS
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                var FLOW_list = dao.GetStatuListForUnitCD8;
                return FLOW_list.Where(m => m.Value == FLOW_CD).Select(m => m.Text).First(); ;
            }
        }

        [Display(Name = "案件狀態修改")]
        [Required]
        [Control(Mode = Control.DropDownList,  block_toggle_group = 7)]
        public string FLOW_CD { get; set; }


        public IList<SelectListItem> FLOW_CD_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.GetStatuListForUnitCD11;
            }
        }

        [Display(Name = "案件歷程紀錄")]
        [Control(Mode = Control.Log, IsOpenNew = true, block_toggle_group = 7, LogSchema= "APPLY_001038_GOODS,APPLY_001038_DEST,APPLY_001038_SELL")]
        public string FLOW_CD_LOG { get; set; }
        #endregion

        #region 檔案上傳
        /// <summary>
        /// 檔案上傳
        /// </summary>
        public string FileSave()
        {
            var ErrorMsg = "";
            ShareDAO dao = new ShareDAO();
            if (this.IsMode == "1")
            {
                // B10(教學)：B11、B12、B13
                if (this.APP_USE_ID == "B10")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\r\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\r\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\r\n"; }
                }
                // B20(研究)：B11、B12、B13
                if (this.APP_USE_ID == "B20")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\r\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\r\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\r\n"; }
                }
                // B30(檢查/檢驗)：B11、B12、B13
                if (this.APP_USE_ID == "B30")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\r\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\r\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\r\n"; }
                }
                // B40(人體醫療移植使用)：B11、B12、B13、B14、B15、B18
                if (this.APP_USE_ID == "B40")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\r\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\r\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\r\n"; }
                    if (!this.DOC_TYP_04_SHOW) { ErrorMsg += "檢附文件類型四為必填!\r\n"; }
                    if (!this.DOC_TYP_05_SHOW) { ErrorMsg += "檢附文件類型五為必填!\r\n"; }
                    //if (!this.DOC_TYP_08_SHOW) { ErrorMsg += "檢附文件類型八為必填!\r\n"; }
                }
                //// B41(細胞治療技術製程使用)：B31、B32
                //if (this.APP_USE_ID == "B41")
                //{
                //    if (!this.DOC_TYP_21_SHOW) { ErrorMsg += "檢附文件類型二十一為必填!\r\n"; }
                //    if (!this.DOC_TYP_22_SHOW) { ErrorMsg += "檢附文件類型二十二為必填!\r\n"; }
                //}
                // B50(保存)：B11、B12、B13
                if (this.APP_USE_ID == "B50")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\r\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\r\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\r\n"; }
                }
                if (this.APP_USE_ID == "B99")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\r\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\r\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\r\n"; }
                }

                if (ErrorMsg == "")
                {
                    if (this.DOC_TYP_01_SHOW)
                    {
                        if (DOC_FILE_01 == null) { ErrorMsg += "檢附文件類型一、"; }
                        else
                        { this.DOC_FILE_01_FILENAME = dao.PutFile("001038", this.DOC_FILE_01, "1").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_02_SHOW)
                    {
                        if (DOC_FILE_02 == null) { ErrorMsg += "檢附文件類型二、"; }
                        else
                        { this.DOC_FILE_02_FILENAME = dao.PutFile("001038", this.DOC_FILE_02, "2").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_03_SHOW)
                    {
                        if (DOC_FILE_03 == null) { ErrorMsg += "檢附文件類型三、"; }
                        else
                        { this.DOC_FILE_03_FILENAME = dao.PutFile("001038", this.DOC_FILE_03, "3").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_04_SHOW)
                    {
                        if (DOC_FILE_04 == null) { ErrorMsg += "檢附文件類型四、"; }
                        else
                        { this.DOC_FILE_04_FILENAME = dao.PutFile("001038", this.DOC_FILE_04, "4").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_05_SHOW)
                    {
                        if (DOC_FILE_05 == null) { ErrorMsg += "檢附文件類型五、"; }
                        else
                        { this.DOC_FILE_05_FILENAME = dao.PutFile("001038", this.DOC_FILE_05, "5").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_06_SHOW)
                    {
                        if (DOC_FILE_06 == null) { ErrorMsg += "檢附文件類型六、"; }
                        else
                        { this.DOC_FILE_06_FILENAME = dao.PutFile("001038", this.DOC_FILE_06, "6").Replace("\\", "/"); }
                    }
                    #region 20201210 JIRA 787

                    //if (this.DOC_TYP_07_SHOW)
                    //{
                    //    if (DOC_FILE_07 == null) { ErrorMsg += "檢附文件類型七、"; }
                    //    else
                    //    { this.DOC_FILE_07_FILENAME = dao.PutFile("001038", this.DOC_FILE_07, "7").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_08_SHOW)
                    //{
                    //    if (DOC_FILE_08 == null) { ErrorMsg += "檢附文件類型八、"; }
                    //    else
                    //    { this.DOC_FILE_08_FILENAME = dao.PutFile("001038", this.DOC_FILE_08, "8").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_09_SHOW)
                    //{
                    //    if (DOC_FILE_09 == null) { ErrorMsg += "檢附文件類型九、"; }
                    //    else
                    //    { this.DOC_FILE_09_FILENAME = dao.PutFile("001038", this.DOC_FILE_09, "9").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_10_SHOW)
                    //{
                    //    if (DOC_FILE_10 == null) { ErrorMsg += "檢附文件類型十、"; }
                    //    else
                    //    { this.DOC_FILE_10_FILENAME = dao.PutFile("001038", this.DOC_FILE_10, "10").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_11_SHOW)
                    //{
                    //    if (DOC_FILE_11 == null) { ErrorMsg += "檢附文件類型十一、"; }
                    //    else
                    //    { this.DOC_FILE_11_FILENAME = dao.PutFile("001038", this.DOC_FILE_11, "11").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_12_SHOW)
                    //{
                    //    if (DOC_FILE_12 == null) { ErrorMsg += "檢附文件類型十二、"; }
                    //    else
                    //    { this.DOC_FILE_12_FILENAME = dao.PutFile("001038", this.DOC_FILE_12, "12").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_13_SHOW)
                    //{
                    //    if (DOC_FILE_13 == null) { ErrorMsg += "檢附文件類型十三、"; }
                    //    else
                    //    { this.DOC_FILE_13_FILENAME = dao.PutFile("001038", this.DOC_FILE_13, "13").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_14_SHOW)
                    //{
                    //    if (DOC_FILE_14 == null) { ErrorMsg += "檢附文件類型十四、"; }
                    //    else
                    //    { this.DOC_FILE_14_FILENAME = dao.PutFile("001038", this.DOC_FILE_14, "14").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_15_SHOW)
                    //{
                    //    if (DOC_FILE_15 == null) { ErrorMsg += "檢附文件類型十五、"; }
                    //    else
                    //    { this.DOC_FILE_15_FILENAME = dao.PutFile("001038", this.DOC_FILE_15, "15").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_16_SHOW)
                    //{
                    //    if (DOC_FILE_16 == null) { ErrorMsg += "檢附文件類型十六、"; }
                    //    else
                    //    { this.DOC_FILE_16_FILENAME = dao.PutFile("001038", this.DOC_FILE_16, "16").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_17_SHOW)
                    //{
                    //    if (DOC_FILE_17 == null) { ErrorMsg += "檢附文件類型十七、"; }
                    //    else
                    //    { this.DOC_FILE_17_FILENAME = dao.PutFile("001038", this.DOC_FILE_17, "17").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_18_SHOW)
                    //{
                    //    if (DOC_FILE_18 == null) { ErrorMsg += "檢附文件類型十八、"; }
                    //    else
                    //    { this.DOC_FILE_18_FILENAME = dao.PutFile("001038", this.DOC_FILE_18, "18").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_19_SHOW)
                    //{
                    //    if (DOC_FILE_19 == null) { ErrorMsg += "檢附文件類型十九、"; }
                    //    else
                    //    { this.DOC_FILE_19_FILENAME = dao.PutFile("001038", this.DOC_FILE_19, "19").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_20_SHOW)
                    //{
                    //    if (DOC_FILE_20 == null) { ErrorMsg += "檢附文件類型二十、"; }
                    //    else
                    //    { this.DOC_FILE_20_FILENAME = dao.PutFile("001038", this.DOC_FILE_20, "20").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_21_SHOW)
                    //{
                    //    if (DOC_FILE_21 == null) { ErrorMsg += "檢附文件類型二十一、"; }
                    //    else
                    //    { this.DOC_FILE_21_FILENAME = dao.PutFile("001038", this.DOC_FILE_21, "21").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_22_SHOW)
                    //{
                    //    if (DOC_FILE_22 == null) { ErrorMsg += "檢附文件類型二十二、"; }
                    //    else
                    //    { this.DOC_FILE_22_FILENAME = dao.PutFile("001038", this.DOC_FILE_22, "22").Replace("\\", "/"); }
                    //}
                    //if (this.DOC_TYP_23_SHOW)
                    //{
                    //    if (DOC_FILE_23 == null) { ErrorMsg += "檢附文件類型二十三、"; }
                    //    else
                    //    { this.DOC_FILE_23_FILENAME = dao.PutFile("001038", this.DOC_FILE_23, "23").Replace("\\", "/"); }
                    //}

                    #endregion

                    if (ErrorMsg != "") { ErrorMsg += "未上傳檔案"; }
                }

            }
            return ErrorMsg;
        }
        #endregion
    }

    //public class Apply_001038_GoodsViewModel : Apply_001038_GoodsModel
    //{

    //    /// <summary>
    //    /// 貨品類別(規格)代碼
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "貨品類別(規格)")]
    //    public string GOODS_TYPE_ID { get; set; }

    //    /// <summary>
    //    /// 貨品名稱
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "貨品名稱")]
    //    public string GOODS_NAME { get; set; }

    //    /// <summary>
    //    /// 申請數量
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "申請數量")]
    //    public int? APPLY_CNT { get; set; }

    //    /// <summary>
    //    /// 數量單位代碼
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "單位")]
    //    public string GOODS_UNIT_ID { get; set; }

    //    /// <summary>
    //    /// 每單位容量
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "每單位容量")]
    //    public string GOODS_MODEL { get; set; }

    //    /// <summary>
    //    /// 型號一
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "型號一")]
    //    public string GOODS_SPEC_1 { get; set; }

    //    /// <summary>
    //    /// 型號二
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "型號一")]
    //    public string GOODS_SPEC_2 { get; set; }

    //    /// <summary>
    //    /// 牌名
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "牌名")]
    //    public string GOODS_BRAND { get; set; }

    //    /// <summary>
    //    /// 貨品輔助描述
    //    /// </summary>
    //    [Required]
    //    [Display(Name = "貨品輔助描述")]
    //    public string GOODS_DESC { get; set; }
    //}

    /// <summary>
    /// 
    /// </summary>
    public class Apply_001038_FileViewModel : Apply_001038_FileModel
    {

    }
}