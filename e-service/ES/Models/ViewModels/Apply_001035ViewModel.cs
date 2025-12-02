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

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_001035 非感染性人體器官、組織及細胞進出口申請作業
    /// </summary>
    public class Apply_001035ViewModel : Apply_001035Model
    {
        public Apply_001035ViewModel()
        {
        }
    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_001035DoneModel
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

    public class Apply_001035FormModel : Apply_001035Model
    {
        public Apply_001035FormModel()
        {
            GOODS = new GoodsDynamicGrid<Apply_001035_GoodsModel>();
            //GOODS.APP_ID = "1111";
            GOODS.APP_ID = this.APP_ID;
            GOODS.model = new Apply_001035_GoodsModel();
            GOODS.GetGoodsList();
            GOODS.SourceModelName = "GOODS";
            GOODS.IsReadOnly = false;
            GOODS.IsNewOpen = true;
            GOODS.IsDeleteOpen = true;
            this.IsNew = true;
        }

        public Apply_001035FormModel(string APP_ID)
        {
            GOODS = new GoodsDynamicGrid<Apply_001035_GoodsModel>();
            //GOODS.APP_ID = "1111";
            GOODS.APP_ID = APP_ID;
            this.APP_ID = APP_ID;
            GOODS.model = new Apply_001035_GoodsModel();
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

        public DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        [Control(Mode = Control.DatePicker, block_toggle_group = 1, group = 1, IsOpenNew = true, IsReadOnly = true)]
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

        [Display(Name = "申請人")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1, group = 2, IsReadOnly = true)]
        public string NAME { get; set; }

        [Display(Name = "申請人身分證字號")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1, group = 2, IsReadOnly = true)]
        public string IDN { get; set; }

        #endregion

        #region 納稅義務人資料
        /// <summary>
        /// 身份證字號/統一編號
        /// </summary
        [Display(Name = "身份證字號/統一編號")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle = true, toggle_name = "納稅義務人資料", block_toggle_group = 2)]
        public string TAX_ORG_ID { get; set; }
        /// <summary>
        /// 姓名/公司名稱中文
        /// </summary>
        [Display(Name = "姓名/公司名稱中文")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, placeholder = "請擇一填寫姓名/公司名稱中文或英文", size = "60", Notes = "請擇一填寫姓名/公司名稱中文或英文")]
        public string TAX_ORG_NAME { get; set; }
        /// <summary>
        /// 姓名/公司名稱英文
        /// </summary>
        [Display(Name = "姓名/公司名稱英文")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, placeholder = "請擇一填寫姓名/公司名稱中文或英文", size = "60", Notes = "請擇一填寫姓名/公司名稱中文或英文")]
        public string TAX_ORG_ENAME { get; set; }

        /// <summary>
        /// 聯絡地址中文
        /// </summary>
        [Display(Name = "聯絡地址中文")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 2, placeholder = "請擇一填寫委託人聯絡地址中文或英文", Notes = "請擇一填寫委託人聯絡地址中文或英文")]
        public string TAX_ORG_ZIP { get; set; }

        public string TAX_ORG_ZIP_ADDR { get; set; }

        public string TAX_ORG_ZIP_DETAIL
        {
            get
            {
                return TAX_ORG_ADDR;
            }
            set
            {
                TAX_ORG_ADDR = value;
            }
        }

        public string TAX_ORG_ADDR { get; set; }

        /// <summary>
        /// 委託人聯絡地址(英文)
        /// </summary>
        [Display(Name = "委託人聯絡地址英文")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, placeholder = "請擇一填寫委託人聯絡地址中文或英文", size = "60", Notes = "請擇一填寫委託人聯絡地址中文或英文")]
        public string TAX_ORG_EADDR { get; set; }
        #endregion

        #region 申辦資料
        /// <summary>
        /// 聯絡人姓名
        /// </summary
        [Display(Name = "聯絡人姓名")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle = true, toggle_name = "申辦資料", block_toggle_group = 3)]
        public string TAX_ORG_MAN { get; set; }
        /// <summary>
        /// 聯絡人電話
        /// </summary>
        [Display(Name = "聯絡人電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 3)]
        public string TAX_ORG_TEL { get; set; }
        /// <summary>
        /// 聯絡人E-MAIL
        /// </summary>
        [Display(Name = "聯絡人E-MAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 3)]
        public string TAX_ORG_EMAIL { get; set; }
        /// <summary>
        /// 聯絡人傳真
        /// </summary>
        [Display(Name = "聯絡人傳真")]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 3)]
        public string TAX_ORG_FAX { get; set; }


        /// <summary>
        /// 進出口別
        /// </summary>
        [Display(Name = "進出口別")]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 3, IsOpenNew = true)]
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
        [Display(Name = "起始日期")]
        [Required]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 3, group = 10)]
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
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 3, group = 10)]
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
        #endregion

        #region 進口
        [Display(Name = "生產國家\n(貨品來源國家)")]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_BIG_id = "IMPORT", block_toggle = true, toggle_name = "進口資料", block_toggle_group = 4, group = 10, block_toggle_id = "DestIn")]
        public string Dest_DEST_STATE_ID { get; set; }

        public IList<SelectListItem> Dest_DEST_STATE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.CountryList;
            }
        }

        [Display(Name = "賣方國家\n(出口國)")]
        [Required]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_group = 4)]
        public string Dest_SELL_STATE_ID { get; set; }

        public IList<SelectListItem> Dest_SELL_STATE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.CountryList;
            }
        }

        /// <summary>
        /// 起運口岸代碼
        /// </summary>
        [Display(Name = "起運口岸")]
        [Control(Mode = Control.CountryPort, IsOpenNew = true, block_toggle_group = 4)]
        public string Dest_BEG_COUNTRY_ID { get; set; }

        public IList<SelectListItem> Dest_BEG_COUNTRY_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.PortList;
            }
        }

        public string Dest_BEG_COUNTRY_ID_PORT
        {
            get
            {
                return Dest_BEG_PORT_ID;
            }
            set
            {
                Dest_BEG_PORT_ID = value;
            }
        }

        public IList<SelectListItem> Dest_BEG_COUNTRY_ID_PORT_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.HarborList;
            }
        }

        public string Dest_BEG_PORT_ID { get; set; }

        [Display(Name = "賣方英文名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4)]
        public string Dest_SELL_NAME { get; set; }

        [Display(Name = "賣方英文地址")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 4)]
        public string Dest_SELL_ADDR { get; set; }
        #endregion

        #region 出口
        [Display(Name = "目的地國家\n(貨品送達國家)")]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_BIG_id = "EXPORT", block_toggle = true, toggle_name = "出口資料", block_toggle_group = 5, group = 20, block_toggle_id = "SellOut")]
        public string Sell_DEST_STATE_ID { get; set; }

        public IList<SelectListItem> Sell_DEST_STATE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.CountryList;
            }
        }

        [Display(Name = "買方國家\n(進口國)")]
        [Required]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_group = 5)]
        public string Sell_SELL_STATE_ID { get; set; }

        public IList<SelectListItem> Sell_SELL_STATE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.CountryList;
            }
        }

        /// <summary>
        /// 轉口港代碼
        /// </summary>
        [Display(Name = "轉口港")]
        [Control(Mode = Control.CountryPort, IsOpenNew = true, block_toggle_group = 5)]
        public string Sell_TRN_COUNTRY_ID { get; set; }

        public IList<SelectListItem> Sell_TRN_COUNTRY_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.PortList;
            }
        }

        public string Sell_TRN_COUNTRY_ID_PORT
        {
            get
            {
                return Sell_TRN_PORT_ID;
            }
            set
            {
                Sell_TRN_PORT_ID = value;
            }
        }

        public IList<SelectListItem> Sell_TRN_COUNTRY_ID_PORT_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.HarborList;
            }
        }

        public string Sell_TRN_PORT_ID { get; set; }

        /// <summary>
        /// 起運口岸
        /// </summary>
        [Display(Name = "起運口岸")]
        [Control(Mode = Control.CountryPort, IsOpenNew = true, block_toggle_group = 5)]
        public string Sell_BEG_COUNTRY_ID { get; set; }

        public IList<SelectListItem> Sell_BEG_COUNTRY_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.PortList;
            }
        }

        public string Sell_BEG_COUNTRY_ID_PORT
        {
            get
            {
                return Sell_BEG_PORT_ID;
            }
            set
            {
                Sell_BEG_PORT_ID = value;
            }
        }

        public IList<SelectListItem> Sell_BEG_COUNTRY_ID_PORT_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.HarborList;
            }
        }

        public string Sell_BEG_PORT_ID { get; set; }

        [Display(Name = "買方英文名稱")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 5)]
        public string Sell_SELL_NAME { get; set; }

        [Display(Name = "買方英文地址")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 5)]
        public string Sell_SELL_ADDR { get; set; }
        #endregion

        #region 貨品資料
        [Control(Mode = Control.Goods, block_toggle_group = 6, EditorViewName = "GoodsDynamicGrid001035")]
        public GoodsDynamicGrid<Apply_001035_GoodsModel> GOODS { get; set; }
        #endregion

        #region 其他資料
        /// <summary>
        /// 申請用途
        /// </summary
        [Display(Name = "申請用途")]
        [Required]
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_group = 7, block_toggle = true, toggle_name = "其他資料")]
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
        [Control(Mode = Control.DropDownList, IsOpenNew = true, block_toggle_group = 7)]
        public string CONF_TYPE_ID { get; set; }

        public IList<SelectListItem> CONF_TYPE_ID_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.GetCONF_TYPEList;
            }
        }


        /// <summary>
        /// 檢附文件類型一
        /// </summary>
        [Display(Name = "檢附文件類型一")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "申請單位同意試驗〈使用〉之文件或計畫摘要", block_toggle_group = 7, form_id = "form_DOC_TYP_01_SHOW")]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_01")]
        public string DOC_COD_01 { get; set; }
        /// <summary>
        /// 檢附文件說明一
        /// </summary>
        [Display(Name = "檢附文件說明一")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_01")]
        public string DOC_TXT_01 { get; set; }

        /// <summary>
        /// 檢附文件說明一
        /// </summary>
        [Display(Name = "檢附文件類型一(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型一", block_toggle_group = 7, form_id = "form_DOC_FILE_01")]
        public HttpPostedFileBase DOC_FILE_01 { get; set; }

        public string DOC_FILE_01_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型二
        /// </summary>
        [Display(Name = "檢附文件類型二")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "輸出國主管機關同意輸出文件或足以證明輸出國未管制輸出文件", block_toggle_group = 7, form_id = "form_DOC_TYP_02_SHOW")]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_02")]
        public string DOC_COD_02 { get; set; }
        /// <summary>
        /// 檢附文件說明二
        /// </summary>
        [Display(Name = "檢附文件說明二")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_02")]
        public string DOC_TXT_02 { get; set; }

        /// <summary>
        /// 檢附文件說明二
        /// </summary>
        [Display(Name = "檢附文件類型二(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型二", block_toggle_group = 7, form_id = "form_DOC_FILE_02")]
        public HttpPostedFileBase DOC_FILE_02 { get; set; }

        public string DOC_FILE_02_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型三
        /// </summary>
        [Display(Name = "檢附文件類型三")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "貨品之檢驗證明文件", block_toggle_group = 7, form_id = "form_DOC_TYP_03_SHOW")]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_03")]
        public string DOC_COD_03 { get; set; }
        /// <summary>
        /// 檢附文件說明三
        /// </summary>
        [Display(Name = "檢附文件說明三")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_03")]
        public string DOC_TXT_03 { get; set; }

        /// <summary>
        /// 檢附文件說明三
        /// </summary>
        [Display(Name = "檢附文件類型三(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型三", block_toggle_group = 7, form_id = "form_DOC_FILE_03")]
        public HttpPostedFileBase DOC_FILE_03 { get; set; }

        public string DOC_FILE_03_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型四
        /// </summary>
        [Display(Name = "檢附文件類型四")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "來源單位合法設立證明文件", block_toggle_group = 7, form_id = "form_DOC_TYP_04_SHOW")]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_04")]
        public string DOC_COD_04 { get; set; }
        /// <summary>
        /// 檢附文件說明四
        /// </summary>
        [Display(Name = "檢附文件說明四")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_04")]
        public string DOC_TXT_04 { get; set; }

        /// <summary>
        /// 檢附文件說明四
        /// </summary>
        [Display(Name = "檢附文件類型四(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型四", block_toggle_group = 7, form_id = "form_DOC_FILE_04")]
        public HttpPostedFileBase DOC_FILE_04 { get; set; }

        public string DOC_FILE_04_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型五
        /// </summary>
        [Display(Name = "檢附文件類型五")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "來源單位證明捐贈者同意捐贈之文件", block_toggle_group = 7, form_id = "form_DOC_TYP_05_SHOW")]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_05")]
        public string DOC_COD_05 { get; set; }
        /// <summary>
        /// 檢附文件說明五
        /// </summary>
        [Display(Name = "檢附文件說明五")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_05")]
        public string DOC_TXT_05 { get; set; }

        /// <summary>
        /// 檢附文件說明五
        /// </summary>
        [Display(Name = "檢附文件類型五(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型五", block_toggle_group = 7, form_id = "form_DOC_FILE_05")]
        public HttpPostedFileBase DOC_FILE_05 { get; set; }

        public string DOC_FILE_05_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型六
        /// </summary>
        [Display(Name = "檢附文件類型六")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "與貨品輸入單位合作之相關證明文件", block_toggle_group = 7, form_id = "form_DOC_TYP_06_SHOW")]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_06")]
        public string DOC_COD_06 { get; set; }
        /// <summary>
        /// 檢附文件說明六
        /// </summary>
        [Display(Name = "檢附文件說明六")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_06")]
        public string DOC_TXT_06 { get; set; }

        /// <summary>
        /// 檢附文件說明六
        /// </summary>
        [Display(Name = "檢附文件類型六(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型六", block_toggle_group = 7, form_id = "form_DOC_FILE_06")]
        public HttpPostedFileBase DOC_FILE_06 { get; set; }

        public string DOC_FILE_06_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型七
        /// </summary>
        [Display(Name = "檢附文件類型七")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "來本署藥政處藥品臨床試驗許可函(如進行藥物臨床試驗者必備)", block_toggle_group = 7, form_id = "form_DOC_TYP_07_SHOW")]
        public Boolean DOC_TYP_07_SHOW
        {
            get
            {
                return DOC_TYP_07.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_07 = value ? "B17" : null;
            }
        }
        /// <summary>
        /// 檢附文件字號七
        /// </summary>
        [Display(Name = "檢附文件字號七")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_07")]
        public string DOC_COD_07 { get; set; }
        /// <summary>
        /// 檢附文件說明七
        /// </summary>
        [Display(Name = "檢附文件說明七")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_07")]
        public string DOC_TXT_07 { get; set; }

        /// <summary>
        /// 檢附文件說明七
        /// </summary>
        [Display(Name = "檢附文件類型七(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型七", block_toggle_group = 7, form_id = "form_DOC_FILE_07")]
        public HttpPostedFileBase DOC_FILE_07 { get; set; }

        public string DOC_FILE_07_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型八
        /// </summary>
        [Display(Name = "檢附文件類型八")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "捐贈者年齡、器官、組織或細胞摘取時間等資料(進口眼角膜得於輸入後一個月內補正)", block_toggle_group = 7, form_id = "form_DOC_TYP_08_SHOW")]
        public Boolean DOC_TYP_08_SHOW
        {
            get
            {
                return DOC_TYP_08.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_08 = value ? "B18" : null;
            }
        }
        /// <summary>
        /// 檢附文件字號八
        /// </summary>
        [Display(Name = "檢附文件字號八")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_08")]
        public string DOC_COD_08 { get; set; }
        /// <summary>
        /// 檢附文件說明八
        /// </summary>
        [Display(Name = "檢附文件說明八")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_08")]
        public string DOC_TXT_08 { get; set; }

        /// <summary>
        /// 檢附文件說明八
        /// </summary>
        [Display(Name = "檢附文件類型八(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型八", block_toggle_group = 7, form_id = "form_DOC_FILE_08")]
        public HttpPostedFileBase DOC_FILE_08 { get; set; }

        public string DOC_FILE_08_FILENAME { get; set; }


        /// <summary>
        /// 檢附文件類型九
        /// </summary>
        [Display(Name = "檢附文件類型九")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "教學課程計畫書", block_toggle_group = 7, form_id = "form_DOC_TYP_09_SHOW")]
        public Boolean DOC_TYP_09_SHOW
        {
            get
            {
                return DOC_TYP_09.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_09 = value ? "B19" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號九
        /// </summary>
        [Display(Name = "檢附文件字號九")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_09")]
        public string DOC_COD_09 { get; set; }
        /// <summary>
        /// 檢附文件說明九
        /// </summary>
        [Display(Name = "檢附文件說明九")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_09")]
        public string DOC_TXT_09 { get; set; }

        /// <summary>
        /// 檢附文件說明九
        /// </summary>
        [Display(Name = "檢附文件類型九(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型九", block_toggle_group = 7, form_id = "form_DOC_FILE_09")]
        public HttpPostedFileBase DOC_FILE_09 { get; set; }

        public string DOC_FILE_09_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十
        /// </summary>
        [Display(Name = "檢附文件類型十")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "藥物臨床試驗許可函", block_toggle_group = 7, form_id = "form_DOC_TYP_10_SHOW")]
        public Boolean DOC_TYP_10_SHOW
        {
            get
            {
                return DOC_TYP_10.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_10 = value ? "B20" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十
        /// </summary>
        [Display(Name = "檢附文件字號十")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_10")]
        public string DOC_COD_10 { get; set; }
        /// <summary>
        /// 檢附文件說明十
        /// </summary>
        [Display(Name = "檢附文件說明十")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_10")]
        public string DOC_TXT_10 { get; set; }

        /// <summary>
        /// 檢附文件說明十
        /// </summary>
        [Display(Name = "檢附文件類型十(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十", block_toggle_group = 7, form_id = "form_DOC_FILE_10")]
        public HttpPostedFileBase DOC_FILE_10 { get; set; }

        public string DOC_FILE_10_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十一
        /// </summary>
        [Display(Name = "檢附文件類型十一")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "臨床試驗摘要", block_toggle_group = 7, form_id = "form_DOC_TYP_11_SHOW")]
        public Boolean DOC_TYP_11_SHOW
        {
            get
            {
                return DOC_TYP_11.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_11 = value ? "B21" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十一
        /// </summary>
        [Display(Name = "檢附文件字號十一")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_11")]
        public string DOC_COD_11 { get; set; }
        /// <summary>
        /// 檢附文件說明十一
        /// </summary>
        [Display(Name = "檢附文件說明十一")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_11")]
        public string DOC_TXT_11 { get; set; }

        /// <summary>
        /// 檢附文件說明十一
        /// </summary>
        [Display(Name = "檢附文件類型十一(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十一", block_toggle_group = 7, form_id = "form_DOC_FILE_11")]
        public HttpPostedFileBase DOC_FILE_11 { get; set; }

        public string DOC_FILE_11_FILENAME { get; set; }


        /// <summary>
        /// 檢附文件類型十二
        /// </summary>
        [Display(Name = "檢附文件類型十二")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "檢體數量概估說明(表)", block_toggle_group = 7, form_id = "form_DOC_TYP_12_SHOW")]
        public Boolean DOC_TYP_12_SHOW
        {
            get
            {
                return DOC_TYP_12.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_12 = value ? "B22" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十二
        /// </summary>
        [Display(Name = "檢附文件字號十二")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_12")]
        public string DOC_COD_12 { get; set; }
        /// <summary>
        /// 檢附文件說明十二
        /// </summary>
        [Display(Name = "檢附文件說明十二")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_12")]
        public string DOC_TXT_12 { get; set; }

        /// <summary>
        /// 檢附文件說明十二
        /// </summary>
        [Display(Name = "檢附文件類型十二(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十二", block_toggle_group = 7, form_id = "form_DOC_FILE_12")]
        public HttpPostedFileBase DOC_FILE_12 { get; set; }

        public string DOC_FILE_12_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十三
        /// </summary>
        [Display(Name = "檢附文件類型十三")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "IRB審核通過之國外擔保書", block_toggle_group = 7, form_id = "form_DOC_TYP_13_SHOW")]
        public Boolean DOC_TYP_13_SHOW
        {
            get
            {
                return DOC_TYP_13.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_13 = value ? "B23" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十三
        /// </summary>
        [Display(Name = "檢附文件字號十三")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_13")]
        public string DOC_COD_13 { get; set; }
        /// <summary>
        /// 檢附文件說明十三
        /// </summary>
        [Display(Name = "檢附文件說明十三")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_13")]
        public string DOC_TXT_13 { get; set; }

        /// <summary>
        /// 檢附文件說明十三
        /// </summary>
        [Display(Name = "檢附文件類型十三(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十三", block_toggle_group = 7, form_id = "form_DOC_FILE_13")]
        public HttpPostedFileBase DOC_FILE_13 { get; set; }

        public string DOC_FILE_13_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十四
        /// </summary>
        [Display(Name = "檢附文件類型十四")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "捐贈者同意文件（或來源單位證明捐者同意捐贈之文件）", block_toggle_group = 7, form_id = "form_DOC_TYP_14_SHOW")]
        public Boolean DOC_TYP_14_SHOW
        {
            get
            {
                return DOC_TYP_14.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_14 = value ? "B24" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十四
        /// </summary>
        [Display(Name = "檢附文件字號十四")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_14")]
        public string DOC_COD_14 { get; set; }
        /// <summary>
        /// 檢附文件說明十四
        /// </summary>
        [Display(Name = "檢附文件說明十四")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_14")]
        public string DOC_TXT_14 { get; set; }

        /// <summary>
        /// 檢附文件說明十四
        /// </summary>
        [Display(Name = "檢附文件類型十四(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十四", block_toggle_group = 7, form_id = "form_DOC_FILE_14")]
        public HttpPostedFileBase DOC_FILE_14 { get; set; }

        public string DOC_FILE_14_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十五
        /// </summary>
        [Display(Name = "檢附文件類型十五")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "捐贈者同意範例文件", block_toggle_group = 7, form_id = "form_DOC_TYP_15_SHOW")]
        public Boolean DOC_TYP_15_SHOW
        {
            get
            {
                return DOC_TYP_15.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_15 = value ? "B25" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十五
        /// </summary>
        [Display(Name = "檢附文件字號十五")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_15")]
        public string DOC_COD_15 { get; set; }
        /// <summary>
        /// 檢附文件說明十五
        /// </summary>
        [Display(Name = "檢附文件說明十五")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_15")]
        public string DOC_TXT_15 { get; set; }

        /// <summary>
        /// 檢附文件說明十五
        /// </summary>
        [Display(Name = "檢附文件類型十五(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十五", block_toggle_group = 7, form_id = "form_DOC_FILE_15")]
        public HttpPostedFileBase DOC_FILE_15 { get; set; }

        public string DOC_FILE_15_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十六
        /// </summary>
        [Display(Name = "檢附文件類型十六")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "與單位合作之相關證明文件", block_toggle_group = 7, form_id = "form_DOC_TYP_16_SHOW")]
        public Boolean DOC_TYP_16_SHOW
        {
            get
            {
                return DOC_TYP_16.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_16 = value ? "B26" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十六
        /// </summary>
        [Display(Name = "檢附文件字號十六")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_16")]
        public string DOC_COD_16 { get; set; }
        /// <summary>
        /// 檢附文件說明十六
        /// </summary>
        [Display(Name = "檢附文件說明十六")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_16")]
        public string DOC_TXT_16 { get; set; }

        /// <summary>
        /// 檢附文件說明十六
        /// </summary>
        [Display(Name = "檢附文件類型十六(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十六", block_toggle_group = 7, form_id = "form_DOC_FILE_16")]
        public HttpPostedFileBase DOC_FILE_16 { get; set; }

        public string DOC_FILE_16_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十七
        /// </summary>
        [Display(Name = "檢附文件類型十七")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "研究單位同意試驗文件（如：IRB許可函）", block_toggle_group = 7, form_id = "form_DOC_TYP_17_SHOW")]
        public Boolean DOC_TYP_17_SHOW
        {
            get
            {
                return DOC_TYP_17.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_17 = value ? "B27" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十七
        /// </summary>
        [Display(Name = "檢附文件字號十七")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_17")]
        public string DOC_COD_17 { get; set; }
        /// <summary>
        /// 檢附文件說明十七
        /// </summary>
        [Display(Name = "檢附文件說明十七")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_17")]
        public string DOC_TXT_17 { get; set; }

        /// <summary>
        /// 檢附文件說明十七
        /// </summary>
        [Display(Name = "檢附文件類型十七(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十七", block_toggle_group = 7, form_id = "form_DOC_FILE_17")]
        public HttpPostedFileBase DOC_FILE_17 { get; set; }

        public string DOC_FILE_17_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十八
        /// </summary>
        [Display(Name = "檢附文件類型十八")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "輸入貨品之檢驗證明文件或無污染證明文件", block_toggle_group = 7, form_id = "form_DOC_TYP_18_SHOW")]
        public Boolean DOC_TYP_18_SHOW
        {
            get
            {
                return DOC_TYP_18.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_18 = value ? "B28" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十八
        /// </summary>
        [Display(Name = "檢附文件字號十八")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_18")]
        public string DOC_COD_18 { get; set; }
        /// <summary>
        /// 檢附文件說明十八
        /// </summary>
        [Display(Name = "檢附文件說明十八")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_18")]
        public string DOC_TXT_18 { get; set; }

        /// <summary>
        /// 檢附文件說明十八
        /// </summary>
        [Display(Name = "檢附文件類型十八(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十八", block_toggle_group = 7, form_id = "form_DOC_FILE_18")]
        public HttpPostedFileBase DOC_FILE_18 { get; set; }

        public string DOC_FILE_18_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型十九
        /// </summary>
        [Display(Name = "檢附文件類型十九")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "藥物臨床試驗許可函及/或試驗計畫摘要", block_toggle_group = 7, form_id = "form_DOC_TYP_19_SHOW")]
        public Boolean DOC_TYP_19_SHOW
        {
            get
            {
                return DOC_TYP_19.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_19 = value ? "B29" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號十九
        /// </summary>
        [Display(Name = "檢附文件字號十九")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_19")]
        public string DOC_COD_19 { get; set; }
        /// <summary>
        /// 檢附文件說明十九
        /// </summary>
        [Display(Name = "檢附文件說明十九")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_19")]
        public string DOC_TXT_19 { get; set; }

        /// <summary>
        /// 檢附文件說明十九
        /// </summary>
        [Display(Name = "檢附文件類型十九(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型十九", block_toggle_group = 7, form_id = "form_DOC_FILE_19")]
        public HttpPostedFileBase DOC_FILE_19 { get; set; }

        public string DOC_FILE_19_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型二十
        /// </summary>
        [Display(Name = "檢附文件類型二十")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "原核准輸出同意函", block_toggle_group = 7, form_id = "form_DOC_TYP_20_SHOW")]
        public Boolean DOC_TYP_20_SHOW
        {
            get
            {
                return DOC_TYP_20.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_20 = value ? "B30" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號二十
        /// </summary>
        [Display(Name = "檢附文件字號二十")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "20", block_toggle_group = 7, form_id = "form_DOC_COD_20")]
        public string DOC_COD_20 { get; set; }
        /// <summary>
        /// 檢附文件說明二十
        /// </summary>
        [Display(Name = "檢附文件說明二十")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_20")]
        public string DOC_TXT_20 { get; set; }

        /// <summary>
        /// 檢附文件說明二十
        /// </summary>
        [Display(Name = "檢附文件類型二十(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型二十", block_toggle_group = 7, form_id = "form_DOC_FILE_20")]
        public HttpPostedFileBase DOC_FILE_20 { get; set; }

        public string DOC_FILE_20_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型二十一
        /// </summary>
        [Display(Name = "檢附文件類型二十一")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "細胞治療技術施行計畫核准函", block_toggle_group = 7, form_id = "form_DOC_TYP_21_SHOW")]
        public Boolean DOC_TYP_21_SHOW
        {
            get
            {
                return DOC_TYP_21.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_21 = value ? "B31" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號二十一
        /// </summary>
        [Display(Name = "檢附文件字號二十一")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "21", block_toggle_group = 7, form_id = "form_DOC_COD_21")]
        public string DOC_COD_21 { get; set; }
        /// <summary>
        /// 檢附文件說明二十一
        /// </summary>
        [Display(Name = "檢附文件說明二十一")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_21")]
        public string DOC_TXT_21 { get; set; }

        /// <summary>
        /// 檢附文件說明二十一
        /// </summary>
        [Display(Name = "檢附文件類型二十一(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型二十一", block_toggle_group = 7, form_id = "form_DOC_FILE_21")]
        public HttpPostedFileBase DOC_FILE_21 { get; set; }

        public string DOC_FILE_21_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型二十二
        /// </summary>
        [Display(Name = "檢附文件類型二十二")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "細胞製備場所GTP認可函", block_toggle_group = 7, form_id = "form_DOC_TYP_22_SHOW")]
        public Boolean DOC_TYP_22_SHOW
        {
            get
            {
                return DOC_TYP_22.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_22 = value ? "B32" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號二十二
        /// </summary>
        [Display(Name = "檢附文件字號二十二")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "22", block_toggle_group = 7, form_id = "form_DOC_COD_22")]
        public string DOC_COD_22 { get; set; }
        /// <summary>
        /// 檢附文件說明二十二
        /// </summary>
        [Display(Name = "檢附文件說明二十二")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_22")]
        public string DOC_TXT_22 { get; set; }

        /// <summary>
        /// 檢附文件說明二十二
        /// </summary>
        [Display(Name = "檢附文件類型二十二(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型二十二", block_toggle_group = 7, form_id = "form_DOC_FILE_22")]
        public HttpPostedFileBase DOC_FILE_22 { get; set; }

        public string DOC_FILE_22_FILENAME { get; set; }

        /// <summary>
        /// 檢附文件類型二十三
        /// </summary>
        [Display(Name = "檢附文件類型二十三")]
        [Control(Mode = Control.CheckBox, onChangeFun = "doDocCheck($(this))", IsOpenNew = true, checkBoxWord = "其他", block_toggle_group = 7, form_id = "form_DOC_TYP_23_SHOW")]
        public Boolean DOC_TYP_23_SHOW
        {
            get
            {
                return DOC_TYP_23.TONotNullString() != "";
            }
            set
            {
                DOC_TYP_23 = value ? "B99" : null;
            }
        }

        /// <summary>
        /// 檢附文件字號二十三
        /// </summary>
        [Display(Name = "檢附文件字號二十三")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, size = "23", block_toggle_group = 7, form_id = "form_DOC_COD_23")]
        public string DOC_COD_23 { get; set; }
        /// <summary>
        /// 檢附文件說明二十三
        /// </summary>
        [Display(Name = "檢附文件說明二十三")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, columns = 30, rows = 5, block_toggle_group = 7, form_id = "form_DOC_TXT_23")]
        public string DOC_TXT_23 { get; set; }

        /// <summary>
        /// 檢附文件說明二十三
        /// </summary>
        [Display(Name = "檢附文件類型二十三(檔案)")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "檢附文件類型二十三", block_toggle_group = 7, form_id = "form_DOC_FILE_23")]
        public HttpPostedFileBase DOC_FILE_23 { get; set; }

        public string DOC_FILE_23_FILENAME { get; set; }
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
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\n"; }
                }
                // B20(研究)：B11、B12、B13
                if (this.APP_USE_ID == "B20")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\n"; }
                }
                // B30(檢查/檢驗)：B11、B12、B13
                if (this.APP_USE_ID == "B30")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\n"; }
                }
                // B40(人體醫療移植使用)：B11、B12、B13、B14、B15、B18
                if (this.APP_USE_ID == "B40")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\n"; }
                    if (!this.DOC_TYP_04_SHOW) { ErrorMsg += "檢附文件類型四為必填!\n"; }
                    if (!this.DOC_TYP_05_SHOW) { ErrorMsg += "檢附文件類型五為必填!\n"; }
                    if (!this.DOC_TYP_08_SHOW) { ErrorMsg += "檢附文件類型八為必填!\n"; }
                }
                // B41(細胞治療技術製程使用)：B31、B32
                if (this.APP_USE_ID == "B41")
                {
                    if (!this.DOC_TYP_21_SHOW) { ErrorMsg += "檢附文件類型二十一為必填!\n"; }
                    if (!this.DOC_TYP_22_SHOW) { ErrorMsg += "檢附文件類型二十二為必填!\n"; }
                }
                // B50(保存)：B11、B12、B13
                if (this.APP_USE_ID == "B50")
                {
                    if (!this.DOC_TYP_01_SHOW) { ErrorMsg += "檢附文件類型一為必填!\n"; }
                    if (!this.DOC_TYP_02_SHOW) { ErrorMsg += "檢附文件類型二為必填!\n"; }
                    if (!this.DOC_TYP_03_SHOW) { ErrorMsg += "檢附文件類型三為必填!\n"; }
                }



                if (ErrorMsg == "")
                {
                    if (this.DOC_TYP_01_SHOW)
                    {
                        if (DOC_FILE_01 == null) { ErrorMsg += "檢附文件類型一、"; }
                        else
                        { this.DOC_FILE_01_FILENAME = dao.PutFile("001035", this.DOC_FILE_01, "1").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_02_SHOW)
                    {
                        if (DOC_FILE_02 == null) { ErrorMsg += "檢附文件類型二、"; }
                        else
                        { this.DOC_FILE_02_FILENAME = dao.PutFile("001035", this.DOC_FILE_02, "2").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_03_SHOW)
                    {
                        if (DOC_FILE_03 == null) { ErrorMsg += "檢附文件類型三、"; }
                        else
                        { this.DOC_FILE_03_FILENAME = dao.PutFile("001035", this.DOC_FILE_03, "3").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_04_SHOW)
                    {
                        if (DOC_FILE_04 == null) { ErrorMsg += "檢附文件類型四、"; }
                        else
                        { this.DOC_FILE_04_FILENAME = dao.PutFile("001035", this.DOC_FILE_04, "4").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_05_SHOW)
                    {
                        if (DOC_FILE_05 == null) { ErrorMsg += "檢附文件類型五、"; }
                        else
                        { this.DOC_FILE_05_FILENAME = dao.PutFile("001035", this.DOC_FILE_05, "5").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_06_SHOW)
                    {
                        if (DOC_FILE_06 == null) { ErrorMsg += "檢附文件類型六、"; }
                        else
                        { this.DOC_FILE_06_FILENAME = dao.PutFile("001035", this.DOC_FILE_06, "6").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_07_SHOW)
                    {
                        if (DOC_FILE_07 == null) { ErrorMsg += "檢附文件類型七、"; }
                        else
                        { this.DOC_FILE_07_FILENAME = dao.PutFile("001035", this.DOC_FILE_07, "7").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_08_SHOW)
                    {
                        if (DOC_FILE_08 == null) { ErrorMsg += "檢附文件類型八、"; }
                        else
                        { this.DOC_FILE_08_FILENAME = dao.PutFile("001035", this.DOC_FILE_08, "8").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_09_SHOW)
                    {
                        if (DOC_FILE_09 == null) { ErrorMsg += "檢附文件類型九、"; }
                        else
                        { this.DOC_FILE_09_FILENAME = dao.PutFile("001035", this.DOC_FILE_09, "9").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_10_SHOW)
                    {
                        if (DOC_FILE_10 == null) { ErrorMsg += "檢附文件類型十、"; }
                        else
                        { this.DOC_FILE_10_FILENAME = dao.PutFile("001035", this.DOC_FILE_10, "10").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_11_SHOW)
                    {
                        if (DOC_FILE_11 == null) { ErrorMsg += "檢附文件類型十一、"; }
                        else
                        { this.DOC_FILE_11_FILENAME = dao.PutFile("001035", this.DOC_FILE_11, "11").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_12_SHOW)
                    {
                        if (DOC_FILE_12 == null) { ErrorMsg += "檢附文件類型十二、"; }
                        else
                        { this.DOC_FILE_12_FILENAME = dao.PutFile("001035", this.DOC_FILE_12, "12").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_13_SHOW)
                    {
                        if (DOC_FILE_13 == null) { ErrorMsg += "檢附文件類型十三、"; }
                        else
                        { this.DOC_FILE_13_FILENAME = dao.PutFile("001035", this.DOC_FILE_13, "13").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_14_SHOW)
                    {
                        if (DOC_FILE_14 == null) { ErrorMsg += "檢附文件類型十四、"; }
                        else
                        { this.DOC_FILE_14_FILENAME = dao.PutFile("001035", this.DOC_FILE_14, "14").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_15_SHOW)
                    {
                        if (DOC_FILE_15 == null) { ErrorMsg += "檢附文件類型十五、"; }
                        else
                        { this.DOC_FILE_15_FILENAME = dao.PutFile("001035", this.DOC_FILE_15, "15").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_16_SHOW)
                    {
                        if (DOC_FILE_16 == null) { ErrorMsg += "檢附文件類型十六、"; }
                        else
                        { this.DOC_FILE_16_FILENAME = dao.PutFile("001035", this.DOC_FILE_16, "16").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_17_SHOW)
                    {
                        if (DOC_FILE_17 == null) { ErrorMsg += "檢附文件類型十七、"; }
                        else
                        { this.DOC_FILE_17_FILENAME = dao.PutFile("001035", this.DOC_FILE_17, "17").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_18_SHOW)
                    {
                        if (DOC_FILE_18 == null) { ErrorMsg += "檢附文件類型十八、"; }
                        else
                        { this.DOC_FILE_18_FILENAME = dao.PutFile("001035", this.DOC_FILE_18, "18").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_19_SHOW)
                    {
                        if (DOC_FILE_19 == null) { ErrorMsg += "檢附文件類型十九、"; }
                        else
                        { this.DOC_FILE_19_FILENAME = dao.PutFile("001035", this.DOC_FILE_19, "19").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_20_SHOW)
                    {
                        if (DOC_FILE_20 == null) { ErrorMsg += "檢附文件類型二十、"; }
                        else
                        { this.DOC_FILE_20_FILENAME = dao.PutFile("001035", this.DOC_FILE_20, "20").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_21_SHOW)
                    {
                        if (DOC_FILE_21 == null) { ErrorMsg += "檢附文件類型二十一、"; }
                        else
                        { this.DOC_FILE_21_FILENAME = dao.PutFile("001035", this.DOC_FILE_21, "21").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_22_SHOW)
                    {
                        if (DOC_FILE_22 == null) { ErrorMsg += "檢附文件類型二十二、"; }
                        else
                        { this.DOC_FILE_22_FILENAME = dao.PutFile("001035", this.DOC_FILE_22, "22").Replace("\\", "/"); }
                    }
                    if (this.DOC_TYP_23_SHOW)
                    {
                        if (DOC_FILE_23 == null) { ErrorMsg += "檢附文件類型二十三、"; }
                        else
                        { this.DOC_FILE_23_FILENAME = dao.PutFile("001035", this.DOC_FILE_23, "23").Replace("\\", "/"); }
                    }


                    if (ErrorMsg != "") { ErrorMsg += "未上傳檔案\n"; }
                }

            }
            return ErrorMsg;
        }
        #endregion
    }

}