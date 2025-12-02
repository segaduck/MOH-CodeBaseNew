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
    /// APPLY_001008 醫事人員請領英文證明書
    /// </summary>
    public class Apply_001008ViewModel : Apply_001008Model
    {
        public Apply_001008ViewModel()
        {
        }
    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_001008DoneModel
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

    public class Apply_001008FormModel : Apply_001008Model
    {
        public Apply_001008FormModel()
        {
            //醫事人員證書grid
            ME = new GoodsDynamicGrid<Apply_001008_MeViewModel>();
            ME.APP_ID = this.APP_ID;
            ME.model = new Apply_001008_MeViewModel();
            ME.GetGoodsList();
            ME.SourceModelName = "ME";
            ME.IsReadOnly = false;
            ME.IsNewOpen = true;
            ME.IsDeleteOpen = true;
            this.IsNew = true;

            //專科證書grid
            PR = new GoodsDynamicGrid<Apply_001008_PrViewModel>();
            PR.APP_ID = this.APP_ID;
            PR.model = new Apply_001008_PrViewModel();
            PR.GetGoodsList();
            PR.SourceModelName = "PR";
            PR.IsReadOnly = false;
            PR.IsNewOpen = true;
            PR.IsDeleteOpen = true;
            this.IsNew = true;

            //國內寄送地址grid
            TRANS = new GoodsDynamicGrid<Apply_001008_TransViewModel>();
            TRANS.APP_ID = this.APP_ID;
            TRANS.model = new Apply_001008_TransViewModel();
            TRANS.GetGoodsList();
            TRANS.SourceModelName = "TRANS";
            TRANS.IsReadOnly = false;
            TRANS.IsNewOpen = true;
            TRANS.IsDeleteOpen = true;
            this.IsNew = true;

            //國外寄送地址grid
            TRANSF = new GoodsDynamicGrid<Apply_001008_TransFViewModel>();
            TRANSF.APP_ID = this.APP_ID;
            TRANSF.model = new Apply_001008_TransFViewModel();
            TRANSF.GetGoodsList();
            TRANSF.SourceModelName = "TRANSF";
            TRANSF.IsReadOnly = false;
            TRANSF.IsNewOpen = true;
            TRANSF.IsDeleteOpen = true;
            this.IsNew = true;

            //醫事人員中文證書電子檔grid
            //ATH = new GoodsDynamicGrid<Apply_001008_AthViewModel>();
            //ATH.APP_ID = this.APP_ID;
            //ATH.model = new Apply_001008_AthViewModel();
            //ATH.GetGoodsList();
            //ATH.SourceModelName = "ATH";
            //ATH.IsReadOnly = false;
            //ATH.IsNewOpen = true;
            //ATH.IsDeleteOpen = true;
            //this.IsNew = true;
        }

        public Apply_001008FormModel(string APP_ID)
        {
            //醫事人員證書grid
            ME = new GoodsDynamicGrid<Apply_001008_MeViewModel>();
            ME.APP_ID = APP_ID;
            this.APP_ID = APP_ID;
            ME.model = new Apply_001008_MeViewModel();
            ME.GetGoodsList();
            ME.SourceModelName = "ME";
            ME.IsReadOnly = false;
            ME.IsNewOpen = true;
            ME.IsDeleteOpen = true;
            this.IsNew = true;

            //專科證書grid
            PR = new GoodsDynamicGrid<Apply_001008_PrViewModel>();
            PR.APP_ID = APP_ID;
            this.APP_ID = APP_ID;
            PR.model = new Apply_001008_PrViewModel();
            PR.GetGoodsList();
            PR.SourceModelName = "PR";
            PR.IsReadOnly = false;
            PR.IsNewOpen = true;
            PR.IsDeleteOpen = true;
            this.IsNew = true;

            //國內寄送地址grid
            TRANS = new GoodsDynamicGrid<Apply_001008_TransViewModel>();
            TRANS.APP_ID = APP_ID;
            this.APP_ID = APP_ID;
            TRANS.model = new Apply_001008_TransViewModel();
            TRANS.GetGoodsList();
            TRANS.SourceModelName = "TRANS";
            TRANS.IsReadOnly = false;
            TRANS.IsNewOpen = true;
            TRANS.IsDeleteOpen = true;
            this.IsNew = true;

            //國外寄送地址grid
            TRANSF = new GoodsDynamicGrid<Apply_001008_TransFViewModel>();
            TRANSF.APP_ID = APP_ID;
            this.APP_ID = APP_ID;
            TRANSF.model = new Apply_001008_TransFViewModel();
            TRANSF.GetGoodsList();
            TRANSF.SourceModelName = "TRANSF";
            TRANSF.IsReadOnly = false;
            TRANSF.IsNewOpen = true;
            TRANSF.IsDeleteOpen = true;
            this.IsNew = true;

            //醫事人員中文證書電子檔grid
            //ATH = new GoodsDynamicGrid<Apply_001008_AthViewModel>();
            //ATH.APP_ID = APP_ID;
            //this.APP_ID = APP_ID;
            //ATH.model = new Apply_001008_AthViewModel();
            //ATH.GetGoodsList();
            //ATH.SourceModelName = "ATH";
            //ATH.IsReadOnly = false;
            //ATH.IsNewOpen = true;
            //ATH.IsDeleteOpen = true;
            //this.IsNew = true;
        }

        /// <summary>
        /// 申請案主檔
        /// </summary>
        public ApplyModel Apply { get; set; }

        /// <summary>
        /// 案件處理狀態
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string CODE_CD { get; set; }

        //表件內容資訊
        public bool IsNew { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string IsMode { get; set; }

        /// <summary>
        /// 執行流程(1:填寫申報表件並上傳檔案 / 2:預覽申辦表件 / 3:繳費 / 4:完成申報)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string FlowMode { get; set; }

        /// <summary>
        /// 是否上傳附件 (0:不上傳 / 1:上傳 )
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string IsUpLoadFile { get; set; }

        #region 申辦表件填寫
        /*hidden欄位*/

        /// <summary>
        /// 案件編號
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle = true, block_toggle_id = "BaseData", toggle_name = "申辦表件填寫", block_toggle_group = 1)]
        public string APP_ID { get; set; }

        /// <summary>
        /// 申請項目代碼
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRV_ID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRC_SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string ACC_NO { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string UNIT_CD { get; set; }

        //[Control(Mode = Control.Hidden, block_toggle_group = 1)]
        //public string IDN { get; set; }

        /// <summary>
        /// 申辦日期(西元年yyyy/MM/dd)
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string APPLY_DATE { get; set; }

        public DateTime? APP_EXT_DATE { get; set; }

        [Display(Name = "預計完成日")]
        public string APP_EXT_DATE_AD
        {
            get
            {
                return HelperUtil.DateTimeToString(APP_EXT_DATE);     // YYYYMMDD 回傳給系統
            }
        }

        [Display(Name = "預計完成日")]
        public string APP_EXT_DATE_TW
        {
            get
            {
                return HelperUtil.DateTimeToTwString(APP_EXT_DATE);     // YYYYMMDD 回傳給系統
            }
        }

        public string PRO_ACC { get; set; }

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

        //[Display(Name = "系統狀態")]
        //public string APP_STATUS
        //{
        //    get
        //    {
        //        BackApplyDAO dao = new BackApplyDAO();
        //        return dao.GetSchedule(this.APP_ID, "2");
        //    }
        //}

        /*顯示欄位*/
        /// <summary>
        /// 申辦項目(中文顯示)
        /// </summary>
        [Display(Name = "申辦項目")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
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
        //[Display(Name = "承辦單位")]
        //[Control(Mode = Control.Lable, block_toggle_group = 1, group = 1)]
        //public string UNIT_NAME
        //{
        //    get
        //    {
        //        string ret = "衛生福利部醫事司";
        //        DataLayers.ShareDAO dao = new ShareDAO();
        //        //ret = dao.GetServiceUnit(this.SRV_ID); //暫時mark先不從db撈(因為跟雛型的內容不一樣)

        //        return ret;
        //    }
        //}

        /// <summary>
        /// 申辦日期(民國年yyy/MM/dd)
        /// </summary>
        [Display(Name = "申辦日期")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string APPLY_DATE_TW
        {
            get
            {
                string ret = "";

                if (this.APPLY_DATE != null)
                {
                    ret = HelperUtil.TransToTwYear(this.APPLY_DATE);
                }

                return ret;
            }
        }

        [Display(Name = "案件編號")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string APP_ID_TEXT { get; set; }

        [Display(Name = "系統狀態")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string APP_STATUS
        {
            get
            {
                BackApplyDAO dao = new BackApplyDAO();
                return dao.GetSchedule(this.APP_ID, "02");
            }
        }

        /*
        public DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
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
        */
        [Display(Name = "申請人中文姓名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, IsReadOnly = true, block_toggle_group = 1)]
        public string NAME { get; set; }

        [Display(Name = "申請人英文姓名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ENAME { get; set; }

        [Display(Name = "申請人英文別名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ENAME_ALIAS { get; set; }

        [Display(Name = "國民身分證統一編號")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, IsReadOnly = true, block_toggle_group = 1, group = 2)]
        public string IDN { get; set; }

        [Display(Name = "計算金額")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, IsReadOnly = true, block_toggle_group = 1, size = "5")]
        public string TOTAL_MEM { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 1)]
        public string ADDR_ZIP { get; set; }

        public string ADDR_ZIP_ADDR { get; set; }

        public string ADDR_ZIP_DETAIL
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

        public string ADDR { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "聯絡電話")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string TEL { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "行動電話")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string MOBILE { get; set; }

        [Display(Name = "EMAIL")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string EMAIL { get; set; }

        [Display(Name = "備註")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, block_toggle_group = 1, rows = 3, columns = 50)]
        public string REMARK { get; set; }
        #endregion

        #region 醫事人員證書grid
        [Control(Mode = Control.Goods, block_toggle = true, toggle_name = "醫事人員或公共衛生師證書", block_toggle_group = 2, EditorViewName = "GoodsDynamicGrid001008Me")]
        public GoodsDynamicGrid<Apply_001008_MeViewModel> ME { get; set; }
        #endregion

        #region 專科證書grid
        [Control(Mode = Control.Goods, block_toggle = true, block_toggle_id = "PR", toggle_name = "專科證書", block_toggle_group = 3, EditorViewName = "GoodsDynamicGrid001008Pr")]
        public GoodsDynamicGrid<Apply_001008_PrViewModel> PR { get; set; }
        #endregion

        #region 本英文證明書郵寄地址(國內)grid
        [Control(Mode = Control.Goods, block_toggle = true, block_toggle_id = "TRANS", toggle_name = "國內寄送地址", block_toggle_group = 4, EditorViewName = "GoodsDynamicGrid001008Trans")]
        public GoodsDynamicGrid<Apply_001008_TransViewModel> TRANS { get; set; }
        #endregion

        #region 本英文證明書郵寄地址(國外)grid
        [Control(Mode = Control.Goods, block_toggle = true, block_toggle_id = "TRANSF", toggle_name = "國外寄送地址", block_toggle_group = 5, EditorViewName = "GoodsDynamicGrid001008TransF")]
        public GoodsDynamicGrid<Apply_001008_TransFViewModel> TRANSF { get; set; }
        #endregion

        #region 上傳應附檔案
        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        [Required]
        //[Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle = true, block_toggle_id = "FILE", block_toggle_group = 6, toggle_name = "上傳應附檔案")]
        public string MERGEYN { get; set; }

        public IList<SelectListItem> MERGEYN_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.YorN_list;
            }
        }

        /// <summary>
        /// 佐證文件採合併檔案中文描述
        /// </summary>
        public string MERGEYN_TEXT
        {
            get
            {
                return (this.MERGEYN.Equals("Y") ? "是" : "否");
            }
        }

        /// <summary>
        /// 單一檔案(apply_file)
        /// </summary>
        public Apply_001008FileModel FileList { get; set; }

        /// <summary>
        /// 醫事人員中文證書電子檔 Grid
        /// </summary>
        public IList<Apply_001008_AthViewModel> ATHs { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案歷程
        /// </summary>
        //public IList<Apply_001008FileHisModel> filePayRecHisList { get; set; }

        /// <summary>
        /// 護照影本電子檔歷程
        /// </summary>
        //public IList<Apply_001008FileHisModel> filePayPassportHisList { get; set; }

        /// <summary>
        /// 醫事人員中文證書電子檔 file Grid
        /// </summary>
        //public IList<Apply_001008AthFileHisModel> ATHs { get; set; }


        //筆數
        public int? RowCount { get; set; }
        #endregion

        #region 案件歷程
        [Display(Name = "案件進度")]
        //[Control(Mode = Control.Lable, block_toggle = true, block_toggle_id = "StatusData", toggle_name = "案件進度歷程", IsOpenNew = true, block_toggle_group = 9)]
        public string FLOW_CD_STATUS
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                var FLOW_list = dao.GetStatuListForUnitCD8;
                return FLOW_list.Where(m => m.Value == FLOW_CD).Select(m => m.Text).First(); ;
            }
        }

        /// <summary>
        /// 案件狀態
        /// --:已接收，處理中 / 0:完成申請 / 1:新收案件 / 2:通知補件 / 3:補件收件 / 9:逾期未補件而予結案 / 12:核可(發文歸檔)
        /// </summary>
        [Display(Name = "案件狀態修改")]
        [Required]
        [Control(Mode = Control.DropDownList, block_toggle_group = 9)]
        public string FLOW_CD { get; set; }

        /// <summary>
        /// 案件狀態選項
        /// </summary>
        public IList<SelectListItem> FLOW_CD_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.GetStatuListForDEP;
            }
        }

        [Display(Name = "案件歷程紀錄")]
        [Control(Mode = Control.Log, IsOpenNew = true, block_toggle_group = 9, LogSchema = "APPLY_001008,APPLY_001008_TRANS,APPLY_001008_TRANSF,APPLY_PAY")]
        public string FLOW_CD_LOG { get; set; }
        #endregion

        #region 繳費資訊
        /// <summary>
        /// 繳費方式(C:信用卡/D:匯票/T:劃撥/B:臨櫃/S:超商)
        /// </summary>
        public string PAY_METHOD { get; set; }

        /// <summary>
        /// 申請時應繳費用
        /// </summary>
        public int? PAY_A_FEE { get; set; }

        /// <summary>
        /// 持卡人身分證號
        /// </summary>
        public string CARD_IDN { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CLIENT_IP { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SessionTransactionKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ErrorCode { get; set; }


        public string ErrorMessage { get; set; }


        public string TempMessage { get; set; }

        public string STATUS { get; set; }

        public string AppDocCount { get; set; }
        #endregion

        #region 繳費資訊(後台)
        /// <summary>
        /// 線費方式中文
        /// </summary>
        public string PAY_METHOD_NAME { get; set; }

        /// <summary>
        /// 新增時間(西元年yyyy/MM/dd)
        /// </summary>
        public string PAY_ACT_TIME_AC { get; set; }

        /// <summary>
        /// 交易日期
        /// </summary>
        public string PAY_EXT_TIME_AC { get; set; }

        /// <summary>
        /// 異動時間 (西元年yyyy/MM/dd)
        /// </summary>
        public string PAY_INC_TIME_AC { get; set; }

        /// <summary>
        /// 交易金額
        /// </summary>
        public int? PAY_MONEY { get; set; }

        /// <summary>
        /// 是否已繳費
        /// </summary>
        public bool IsPay { get; set; }

        /// <summary>
        /// 是否通知
        /// </summary>
        public string IsNotice { get; set; }


        #endregion

        #region 案件進度歷程
        /// <summary>
        /// 補件項目
        /// </summary>
        public string ISMODIFY { get; set; }

        /// <summary>
        /// 補件項目
        /// </summary>
        public string NoticeCheck { get; set; }

        /// <summary>
        /// 補件內容
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 郵寄日期
        /// </summary>
        public string MAIL_DATE_AD { get; set; }

        /// <summary>
        /// 掛號條碼
        /// </summary>
        public string MAIL_BARCODE { get; set; }

        /// <summary>
        /// 公文文號(apply)
        /// </summary>
        public string MOHW_CASE_NO { get; set; }
        #endregion
    }

    public class Apply_001008FileModel
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public string APP_ID { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案
        /// </summary>
        [Display(Name = "繳費紀錄照片或pdf檔案")]
        public string FILE_PAYRECORD { get; set; }

        public string FILE_PAYRECORD_TEXT { get; set; }

        /// <summary>
        /// 護照影本電子檔
        /// </summary>
        [Display(Name = "護照影本電子檔")]

        public string FILE_PASSPORT { get; set; }

        public string FILE_PASSPORT_TEXT { get; set; }
    }

    public class Apply_001008FileHisModel : Apply_FileModel
    {

        /// <summary>
        /// 上傳檔案資訊
        /// </summary>
        public string FILE_TEXT { get; set; }

        /// <summary>
        /// 檔案上傳時間
        /// </summary>
        public string ADD_TIME_TW
        {
            get
            {
                string ret = "";

                if (this.ADD_TIME != null)
                {
                    ret = HelperUtil.DateTimeToTwFormatLongString(this.ADD_TIME);
                }

                return ret;
            }
        }
    }

    public class Apply_001008AthFileHisModel : Apply_001008_AthModel
    {
        public string ATH_FILE { get; set; }
        public string ATH_FILE_TEXT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Apply_001008_AthViewModel> athList { get; set; }
    }
}