using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using ES.Models.Entities;
using ES.Commons;
using ES.DataLayers;
using System.Web.Mvc;
using System.Web;
using ES.Services;

namespace ES.Models.ViewModels
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

        //表件內容資訊
        public bool IsNew { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string IsMode { get; set; }

        /// <summary>
        /// 是否允許開放補件(Y:是 / N:否)
        /// </summary>
        public string IsNotice { get; set; }
                
        #region 申辦表件填寫
        /*hidden欄位*/

        /// <summary>
        /// 案件編號
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle = true, toggle_name = "申辦表件填寫", block_toggle_group = 1)]
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

        /// <summary>
        /// 案件處理狀態(--:已接收，處理中 / 0:完成申請 / 1:新收案件 / 2:通知補件 / 3:補件收件 / 9:逾期未補件而予結案 / 12:核可(發文歸檔))
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string CODE_CD { get; set; }

        /// <summary>
        /// 執行流程(1:填寫申報表件並上傳檔案 / 2:預覽申辦表件 / 3:繳費 / 4:完成申報)
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string FlowMode { get; set; }

        /// <summary>
        /// 是否上傳附件 (0:不上傳 / 1:上傳 )
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string IsUpLoadFile { get; set; }

        /// <summary>
        /// 案件處理狀態名稱
        /// </summary>
        public string CODE_TEXT { get; set; }

        /*顯示欄位*/
        /// <summary>
        /// 申辦項目(中文顯示)
        /// </summary>
        [Display(Name = "申辦項目")]
        [Control(Mode = Control.Lable, block_toggle_group = 1, group = 1)]
        public string SRV_ID_NAME
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                string ret = "";

                if (!string.IsNullOrWhiteSpace(this.SRV_ID))
                {
                    ret = dao.GetServiceName(this.SRV_ID);
                }

                return ret;
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
                string ret = "衛生福利部醫事司";
                DataLayers.ShareDAO dao = new ShareDAO();
                //ret = dao.GetServiceUnit(this.SRV_ID); //暫時mark先不從db撈(因為跟雛型的內容不一樣)

                return ret;
            }
        }

        /// <summary>
        /// 申辦日期(民國年yyy/MM/dd)
        /// </summary>
        [Display(Name = "申辦日期")]
        [Control(Mode = Control.Lable, block_toggle_group = 1)]
        [Required]
        public string APPLY_DATE_TW {
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
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, IsReadOnly = true, block_toggle_group = 1,group = 2)]
        public string NAME { get; set; }

        [Display(Name = "身分證編號/居留證號")] //20201221 fix [MOHES-917]欄位更名
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, IsReadOnly = true, block_toggle_group = 1, group = 2)]
        public string IDN { get; set; }

        [Display(Name = "申請人英文姓名")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ENAME { get; set; }

        [Display(Name = "申請人英文別名(有英文別名者請附護照影本)")]
        //[Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1, placeholder = "請輸入申請人英文別名")]
        public string ENAME_ALIAS { get; set; }

        [Display(Name = "備註")]
        [Control(Mode = Control.TextArea, IsOpenNew = true, block_toggle_group = 1,rows = 3, columns = 50,maxlength = "50", placeholder = "請輸入備註",Notes = "(最多填寫50個字)")]
        public string REMARK { get; set; }

        [Display(Name = "計算金額")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, IsReadOnly = true, block_toggle_group = 1,size = "5",Link = "瀏覽計算方式範例", LinkHref = "PayMemo")]
        public string TOTAL_MEM { get; set; }

        /// <summary>
        /// 通訊地址
        /// </summary>
        [Display(Name = "通訊地址")]
        [Required]
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
        /// 完整通訊地址(純文字顯示用)
        /// </summary>
        public string ADDR_FULLTEXT
        {
            get
            {
                string ret = "";

                if (this.ADDR_ZIP_ADDR != null)
                {
                    ret = this.ADDR_ZIP + this.ADDR_ZIP_ADDR + this.ADDR_ZIP_DETAIL.Replace(this.ADDR_ZIP_ADDR, "");
                }

                return ret;
            }
        }

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "電話")]
        //[Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 1)]
        public string TEL { get; set; }

        public string TEL_Zip { get; set; }
        public string TEL_Phone { get; set; }
        public string TEL_Num { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        [Display(Name = "行動電話")]
        [Required]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string MOBILE { get; set; }

        [Display(Name = "EMAIL")]
        [Required]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 1)]
        public string EMAIL { get; set; }

        public string EMAIL_1 { get; set; }

        public string EMAIL_2 { get; set; }

        public string EMAIL_3 { get; set; }
        #endregion

        #region 醫事人員證書grid
        /// <summary>
        /// 醫事人員證書
        /// </summary>
        [Control(Mode = Control.Goods, block_toggle_group = 4, EditorViewName = "GoodsDynamicGrid001008Me")]
        public GoodsDynamicGrid<Apply_001008_MeViewModel> ME { get; set; }
        #endregion

        #region 專科證書grid
        /// <summary>
        /// 專科證書
        /// </summary>
        [Control(Mode = Control.Goods, block_toggle_group = 5, EditorViewName = "GoodsDynamicGrid001008Pr")]
        public GoodsDynamicGrid<Apply_001008_PrViewModel> PR { get; set; }
        #endregion

        #region 本英文證明書郵寄地址(國內)grid
        /// <summary>
        /// 本英文證明書-國內郵寄地址
        /// </summary>
        [Control(Mode = Control.Goods, block_toggle_group = 6, EditorViewName = "GoodsDynamicGrid001008Trans")]
        public GoodsDynamicGrid<Apply_001008_TransViewModel> TRANS { get; set; }
        #endregion

        #region 本英文證明書郵寄地址(國外)grid
        /// <summary>
        /// 本英文證明書-國外郵寄地址
        /// </summary>
        [Control(Mode = Control.Goods, block_toggle_group = 7, EditorViewName = "GoodsDynamicGrid001008TransF")]
        public GoodsDynamicGrid<Apply_001008_TransFViewModel> TRANSF { get; set; }
        #endregion

        #region 佐證文件檔案上傳
        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        [Required]
        [Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle = true, block_toggle_group = 8, toggle_name = "佐證文件檔案上傳",Notes = "(若您是將檔案掃描後，於同一份文件內上傳，請選擇佐證文件採合併檔案選\"是\"，檔案分開上傳請選擇\"否\")")]
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
        public string MERGEYN_TEXT {
            get
            {
                string ret = "";

                if (!string.IsNullOrWhiteSpace(this.MERGEYN))
                {
                    ret = (this.MERGEYN.Equals("Y") ? "是" : "否");
                }

                return ret;
            }
        }

        /// <summary>
        /// 醫事人員中文證書電子檔
        /// </summary>
        //[Display(Name = "醫事人員中文證書電子檔")]
        //[Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "醫事人員中文證書電子檔", block_toggle_group = 9)]
        //public HttpPostedFileBase FILE_CHN_CERTIFICATE { get; set; }
        //public string FILE_CHN_CERTIFICATE_FILENAME { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案
        /// </summary>
        [Display(Name = "繳費紀錄照片或pdf檔案")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "繳費紀錄照片或pdf檔案", block_toggle_group = 8,LimitFileType = "1", MaxFileSize = "5" , UploadDesc = "(檔案大小5MB以下，可以接受副檔名：PDF、JPG、BMP、PNG、GIF、TIF) ")]
        public HttpPostedFileBase FILE_PAYRECORD { get; set; }

        public string FILE_PAYRECORD_FILENAME { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 8)]
        public string FILE_PAYRECORD_TEXT { get; set; }

        /// <summary>
        /// 繳費紀錄照片或pdf檔案_檔案類型
        /// </summary>
        public string FILE_PAYRECORD_MIME {
            get
            {
                string ret = null;
                if (this.FILE_PAYRECORD != null) {
                    ret = this.FILE_PAYRECORD.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 是否已上傳過護照影本電子檔
        /// </summary>
        public string HAS_OFILE_PAYRECORD { get; set; }

        /// <summary>
        /// 護照影本電子檔
        /// </summary>
        [Display(Name = "護照影本電子檔")]
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "護照影本電子檔", block_toggle_group = 8, LimitFileType = "1", MaxFileSize = "5", UploadDesc = "(檔案大小5MB以下，可以接受副檔名：PDF、JPG、BMP、PNG、GIF、TIF) ")]
        [Required]
        public HttpPostedFileBase FILE_PASSPORT { get; set; }

        public string FILE_PASSPORT_FILENAME { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 8)]
        public string FILE_PASSPORT_TEXT { get; set; }

        /// <summary>
        /// 護照影本電子檔-檔案類型
        /// </summary>
        public string FILE_PASSPORT_MIME {
            get
            {
                string ret = null;
                if (this.FILE_PASSPORT != null)
                {
                    ret = this.FILE_PASSPORT.ContentType;
                }
                return ret;
            }
        }

        /// <summary>
        /// 是否已上傳過護照影本電子檔
        /// </summary>
        public string HAS_OFILE_PAYPASSPORT { get; set; }

        ///// <summary>
        ///// 醫事人員中文證書電子檔 Grid
        ///// </summary>
        //[Control(Mode = Control.Goods, block_toggle_group = 8, EditorViewName = "GoodsDynamicGrid001008Ath")]
        //public GoodsDynamicGrid<Apply_001008_AthViewModel> ATH { get; set; }

        public IList<Apply_001008_AthViewModel> ATHs { get; set; }

        //筆數
        public int? RowCount { get; set; }

        /// <summary>
        /// 是否開放補件修改-繳費紀錄照片或pdf檔案
        /// </summary>
        public bool IsUpdPayRecord {
            get
            {
                bool ret = false;

                if (!string.IsNullOrWhiteSpace(this.FieldStr))
                {
                    ret = (this.FieldStr.IndexOf("FILE_0") >= 0 || this.FieldStr.IndexOf("ALL_3") >= 0);
                }

                return ret;
            }
        }

        /// <summary>
        /// 是否開放補件修改-護照影本電子檔
        /// </summary>
        public bool IsUpdPassPort
        {
            get
            {
                bool ret = false;

                if (!string.IsNullOrWhiteSpace(this.FieldStr))
                {
                    ret = (this.FieldStr.IndexOf("FILE_1") >= 0 || this.FieldStr.IndexOf("ALL_3") >= 0);
                }

                return ret;
            }
        }

        /// <summary>
        /// 是否開放補件修改-護照影本電子檔
        /// </summary>
        public bool IsUpdATH
        {
            get
            {
                bool ret = false;

                if (!string.IsNullOrWhiteSpace(this.FieldStr))
                {
                    ret = (this.FieldStr.IndexOf("FILE_2") >= 0 || this.FieldStr.IndexOf("ALL_3") >= 0);
                }

                return ret;
            }
        }

        /// <summary>
        /// 是否開放補件修改-其它
        /// </summary>
        public bool IsUpdApply
        {
            get
            {
                bool ret = false;

                if (!string.IsNullOrWhiteSpace(this.FieldStr))
                {
                    ret = (this.FieldStr.IndexOf("ALL_3") >= 0);
                }

                return ret;
            }
        }

        #endregion

        #region 檔案上傳檢核
        /// <summary>
        /// 檔案上傳(檢核)
        /// </summary>
        public string FileSave()
        {
            var ErrorMsg = "";
            bool blFlag = false;

            ShareDAO dao = new ShareDAO();

            if (this.IsMode == "1")
            {
                if (this.FILE_PASSPORT == null)
                {
                    //ErrorMsg = "請上傳護照影本電子檔。";
                }
                else  if (MERGEYN == "Y")
                {
                    
                    if (this.FILE_PAYRECORD == null && this.FILE_PASSPORT == null)
                    {
                        ErrorMsg += "佐證文件檔案上傳選【是】時，請至少上傳一個檔案。";
                    }
                    else
                    {
                        //if (this.FILE_CHN_CERTIFICATE != null)
                        //{ this.FILE_CHN_CERTIFICATE_FILENAME = dao.PutFile("001008", this.FILE_CHN_CERTIFICATE, "1").Replace("\\", "/"); }

                        if (this.FILE_PAYRECORD != null && this.IsUpLoadFile.Equals("1") )
                        {
                            this.FILE_PAYRECORD_FILENAME = dao.PutFile("001008", this.FILE_PAYRECORD, "1").Replace("\\", "/");
                        }

                        if (this.FILE_PASSPORT != null && this.IsUpLoadFile.Equals("1"))
                        {
                            this.FILE_PASSPORT_FILENAME = dao.PutFile("001008", this.FILE_PASSPORT, "2").Replace("\\", "/");
                        }
                    }
                }
                else
                {
                    //if (this.FILE_CHN_CERTIFICATE == null)
                    //{ ErrorMsg += "醫事人員中文證書電子檔、"; }
                    //else
                    //{ this.FILE_CHN_CERTIFICATE_FILENAME = dao.PutFile("001008", this.FILE_CHN_CERTIFICATE, "1").Replace("\\", "/"); }

                    if (this.FILE_PAYRECORD == null)
                    {
                        //ErrorMsg += "繳費紀錄照片或pdf檔案、"; 
                    }
                    else 
                    {
                        this.FILE_PAYRECORD_FILENAME = dao.PutFile("001008", this.FILE_PAYRECORD, "1").Replace("\\", "/"); 
                    }

                    if (this.FILE_PASSPORT == null)
                    {
                        //ErrorMsg += "護照影本電子檔、"; 
                    }
                    else if (this.IsUpLoadFile.Equals("1"))
                    {
                        this.FILE_PASSPORT_FILENAME = dao.PutFile("001008", this.FILE_PASSPORT, "2").Replace("\\", "/");
                    }
                }

                if (ErrorMsg != "") { ErrorMsg += ""; }

                if (this.ATHs != null)
                {
                    var idx = 1;
                    foreach (var item in this.ATHs)
                    {
                        if (item.FILE_1 != null)
                        {
                            blFlag = true;

                            item.FILE_1_FILENAME = dao.PutFile("001008","", item.FILE_1, (idx++).ToString(),"_ATH_UP_").Replace("\\", "/");
                            item.FILE_1_FULLNAME = item.FILE_1_FILENAME;
                        }
                    }

                    if (!blFlag)
                    {
                        ErrorMsg += "請至少上傳一筆醫事人員或公共衛生師/專科中文證書電子檔。";
                    }
                }

                bool blChkAth = false;
                if (this.ATHs != null)
                {
                    for (int i = 0; i< this.ATHs.Count(); i++)
                    {
                        if (this.ATHs[i].FILE_1 != null)
                        {
                            blChkAth = true;
                            break;
                        }
                    }
                }

                //處理醫事人員/專科中文證書電子檔
                if (!blChkAth)
                {
                    ErrorMsg += (ErrorMsg.Equals("") ? "" : "<br>") ;
                }
            }

            return ErrorMsg;
        }

        /// <summary>
        /// 檢核醫事人員/專科中文證書電子檔附件資料（補件）
        /// </summary>
        /// <returns></returns>
        public string chkAthFile()
        {
            string ret = "";
            bool blFlag = false;

            if (ATHs != null)
            {
                for (int i = 0; i < ATHs.Count(); i++)
                {
                    //20201106要求再補件，若已有舊檔且沒再上傳新檔，目前先允許可以送出
                    //if (!string.IsNullOrEmpty(ATHs[i].ATH_UP) || ATHs[i].FILE_1 != null)
                    if (ATHs[i].HAS_OFILE.Equals("Y") || ATHs[i].FILE_1 != null)
                    {
                        blFlag = true;
                        break;
                    }
                }
            }

            if (!blFlag)
            {
                ret = "請至少上傳一筆醫事人員或公共衛生師/專科中文證書電子檔！";
            }
           
            return ret;
        }
        #endregion

        #region 繳費資訊
        /// <summary>
        /// 繳費方式(C:信用卡線上刷卡（電子化政府網路付費服務） / D:匯票(抬頭：衛生福利部）/ T:劃撥 / B:臨櫃（現金） / S:超商)
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

        #region 通知補件
        /// <summary>
        /// 補件欄位字串
        /// </summary>
        public string FieldStr { get; set; }

        /// <summary>
        /// 補件資訊
        /// </summary>
        public string MAILBODY { get; set; }

        public int? FREQUENCY { get; set; }
        #endregion

        /// <summary>
        /// 是否使用聯合信用中心
        /// </summary>
        public string ISEC { get; set; }
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
        /// 申請送件時是否已上傳繳費紀錄照片或pdf檔案
        /// </summary>
        public string HAS_OFILE_PAYRECORD { get; set; }

        /// <summary>
        /// 護照影本電子檔
        /// </summary>
        [Display(Name = "護照影本電子檔")]

        public string FILE_PASSPORT { get; set; }

        public string FILE_PASSPORT_TEXT { get; set; }

        /// <summary>
        /// 申請送件時是否已上傳護照影本電子檔
        /// </summary>
        public string HAS_OFILE_PAYPASSPORT { get; set; }
    }
}