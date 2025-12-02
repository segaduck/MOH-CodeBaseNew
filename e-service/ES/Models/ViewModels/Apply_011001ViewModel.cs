using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using ES.DataLayers;
using System.Web.Mvc;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_011001ViewModel
    {
        public Apply_011001ViewModel()
        {
        }

        /// <summary>
        /// 表單填寫
        /// </summary>
        public Apply_011001FormModel Form { get; set; }

        /// <summary>
        /// 補件表單
        /// </summary>
        public Apply_011001DetailModel Detail { get; set; }

    }


    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011001FormModel : APPLY_011001
    {
        public Apply_011001FormModel()
        {
            this.IsNew = true;
        }

        public bool IsNew { get; set; }

        [Control(Mode = Control.Hidden)]
        public string IsMode { get; set; }

        #region 申辦表件填寫
        /// <summary>
        /// 案號
        /// </summary>
        [Display(Name = "案號")]
        [Control(Mode = Control.Hidden)]
        public string APP_ID { get; set; }

        [Control(Mode = Control.Hidden)]
        public string SRV_ID { get; set; }

        [Control(Mode = Control.Hidden)]
        public string SRC_SRV_ID { get; set; }

        /// <summary>
        /// 申辦項目(中文顯示)
        /// </summary>
        [Display(Name = "申辦項目")]
        [Control(Mode = Control.Lable, block_toggle = true, toggle_name = "申辦表件填寫", block_toggle_group = 1,group = 1)]
        public string SRV_ID_NAME
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                return dao.GetServiceName(this.SRV_ID);
            }
        }
        
        public DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        [Control(Mode = Control.Lable, block_toggle_group = 1, group = 1, IsOpenNew = true)]
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
                    return HelperUtil.DateTimeToTwString(APP_TIME);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                APP_TIME = HelperUtil.TransTwToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 運用單位登入帳號
        /// </summary>
        [Required]
        [Display(Name = "運用單位登入帳號")]
        [Control(Mode = Control.Lable, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NO { get; set; }

        /// <summary>
        /// 運用單位名稱
        /// </summary>
        [Required]
        [Display(Name = "運用單位名稱")]
        [Control(Mode = Control.Lable, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NAM { get; set; }

        [Display(Name = "運用單位電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_TEL { get; set; }

        [Display(Name = "運用單位地址(含郵遞區號)")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_ADDR_CODE { get; set; }

        public string ACC_ADDR_CODE_ADDR { get; set; }

        public string ACC_ADDR_CODE_DETAIL
        {
            get
            {
                return ACC_ADDR;
            }
            set
            {
                ACC_ADDR = value;
            }
        }
        
        public string ACC_ADDR { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        [Required]
        [Display(Name = "承辦人姓名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_NAM { get; set; }

        /// <summary>
        /// 承辦人行動電話
        /// </summary>
        [Display(Name = "承辦人行動電話")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_MOBILE { get; set; }

        [Required]
        [Display(Name = "承辦人E-MAIL")]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_MAIL { get; set; }

        public string ACC_SDATE { get; set; }

        [Required]
        [Display(Name = "運用單位發文日期")]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_SDATE_AD
        {
            get
            {
                if (string.IsNullOrEmpty(ACC_SDATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(ACC_SDATE, "/"));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                ACC_SDATE = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "/");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 運用單位發文字號
        /// </summary>
        [Required]
        [Display(Name = "運用單位發文字號")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NUM { get; set; }

        #endregion

        #region 佐證文件檔案上傳
        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle =true,toggle_name = "佐證文件檔案上傳", block_toggle_group = 2,form_id = "FID_MERGEYN")]
        [Display(Name = "佐證文件採合併檔案")]
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
        /// 志願服務運用計畫
        /// </summary>
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "志願服務運用計畫", block_toggle_group = 2)]
        [Display(Name = "志願服務運用計畫")]
        [Required]
        public HttpPostedFileBase FILE_1 { get; set; }
        public string FILE_1_FILENAME { get; set; }

        /// <summary>
        /// 運用單位組織章程
        /// </summary>
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "運用單位組織章程", block_toggle_group = 2)]
        [Display(Name = "運用單位組織章程")]
        [Required]
        public HttpPostedFileBase FILE_2 { get; set; }
        public string FILE_2_FILENAME { get; set; }

        /// <summary>
        /// 單位立案登記證書影本
        /// </summary>
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "單位立案登記證書影本", block_toggle_group = 2)]
        [Display(Name = "單位立案登記證書影本")]
        [Required]
        public HttpPostedFileBase FILE_3 { get; set; }
        public string FILE_3_FILENAME { get; set; }

        /// <summary>
        /// 志工基本資料清冊
        /// </summary>
        [Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "志工基本資料清冊", block_toggle_group = 2)]
        [Display(Name = "志工基本資料清冊")]
        public HttpPostedFileBase FILE_4 { get; set; }
        public string FILE_4_FILENAME { get; set; }
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
                
                if (ErrorMsg == "")
                {
                    if (MERGEYN == "Y")
                    {
                        if (this.FILE_1 == null && this.FILE_2 == null && this.FILE_3 == null && this.FILE_4 == null)
                        {
                            ErrorMsg += "至少上傳一個檔案，";
                        }
                        else
                        {
                            if (this.FILE_1 != null)
                            { this.FILE_1_FILENAME = dao.PutFile("011001", this.FILE_1, "1").Replace("\\", "/"); }

                            if (this.FILE_2 != null)
                            { this.FILE_2_FILENAME = dao.PutFile("011001", this.FILE_2, "2").Replace("\\", "/"); }

                            if (this.FILE_3 != null)
                            { this.FILE_3_FILENAME = dao.PutFile("011001", this.FILE_3, "3").Replace("\\", "/"); }

                            if (this.FILE_4 != null)
                            { this.FILE_4_FILENAME = dao.PutFile("011001", this.FILE_4, "4").Replace("\\", "/"); }
                        }

                    }
                    else
                    {
                        if (this.FILE_1 == null )
                        {ErrorMsg += "志願服務運用計畫、";  }
                        else
                        { this.FILE_1_FILENAME = dao.PutFile("011001", this.FILE_1, "1").Replace("\\", "/"); }

                        if (this.FILE_2 == null)
                        {ErrorMsg += "運用單位組織章程、";  }
                        else
                        { this.FILE_2_FILENAME = dao.PutFile("011001", this.FILE_2, "2").Replace("\\", "/"); }

                        if (this.FILE_3 == null)
                        {  ErrorMsg += "單位立案登記證書影本、";  }
                        else
                        { this.FILE_3_FILENAME = dao.PutFile("011001", this.FILE_3, "3").Replace("\\", "/"); }

                        if (this.FILE_4 != null)
                        { this.FILE_4_FILENAME = dao.PutFile("011001", this.FILE_4, "4").Replace("\\", "/"); }
                    }

                    if (ErrorMsg != "") { ErrorMsg += "未上傳檔案"; }
                }

            }
            return ErrorMsg;
        }
        #endregion

    }

    /// <summary>
    /// 案件明細
    /// </summary>
    public class Apply_011001DetailModel : ApplyModel
    {
    
        public Apply_011001DetailModel()
        {
            this.IsNew = true;
        }

        public bool IsNew { get; set; }

        [Control(Mode = Control.Hidden)]
        public string IsMode { get; set; }

        /// <summary>
        /// 補件欄位字串
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string FieldStr { get; set; }
        
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string APPSTATUS { get; set; }

        #region 申辦表件填寫
        /// <summary>
        /// 案號
        /// </summary>
        [Display(Name = "案號")]
        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string APP_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRC_SRV_ID { get; set; }

        /// <summary>
        /// 申辦項目(中文顯示)
        /// </summary>
        [Display(Name = "申辦項目")]
        [Control(Mode = Control.Lable, block_toggle = true, toggle_name = "申辦表件填寫", block_toggle_group = 1, group = 1)]
        public string SRV_ID_NAME
        {
            get
            {
                ShareDAO dao = new ShareDAO();
                return dao.GetServiceName(this.SRV_ID);
            }
        }

        public DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        [Control(Mode = Control.Lable, block_toggle_group = 1, group = 1, IsOpenNew = true)]
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
        }

        /// <summary>
        /// 運用單位登入帳號
        /// </summary>
        [Required]
        [Display(Name = "運用單位登入帳號")]
        [Control(Mode = Control.Lable, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NO { get; set; }

        /// <summary>
        /// 運用單位名稱
        /// </summary>
        [Required]
        [Display(Name = "運用單位名稱")]
        [Control(Mode = Control.Lable, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NAM { get; set; }

        [Display(Name = "運用單位電話")]
        [Required]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_TEL { get; set; }

        [Display(Name = "運用單位地址(含郵遞區號)")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_ADDR_CODE { get; set; }

        public string ACC_ADDR_CODE_ADDR { get; set; }

        public string ACC_ADDR_CODE_DETAIL
        {
            get
            {
                return ACC_ADDR;
            }
            set
            {
                ACC_ADDR = value;
            }
        }

        public string ACC_ADDR { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        [Required]
        [Display(Name = "承辦人姓名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_NAM { get; set; }

        /// <summary>
        /// 承辦人行動電話
        /// </summary>
        [Display(Name = "承辦人行動電話")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_MOBILE { get; set; }

        [Required]
        [Display(Name = "承辦人E-MAIL")]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_MAIL { get; set; }

        public string ACC_SDATE { get; set; }

        [Required]
        [Display(Name = "運用單位發文日期")]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_SDATE_AD
        {
            get
            {
                if (string.IsNullOrEmpty(ACC_SDATE))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(ACC_SDATE, "/"));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                ACC_SDATE = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "/");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 運用單位發文字號
        /// </summary>
        [Required]
        [Display(Name = "運用單位發文字號")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NUM { get; set; }

        #endregion

        #region 佐證文件檔案上傳

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle = true, toggle_name = "佐證文件檔案上傳", block_toggle_group = 2,form_id = "FID_MERGEYN")]
        [Display(Name = "佐證文件採合併檔案")]
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
        /// 志願服務運用計畫
        /// </summary>
        [Control(Mode = Control.FileUploadAppDoc, IsOpenNew = true, HoverFileName = "志願服務運用計畫", block_toggle_group = 2)]
        [Display(Name = "志願服務運用計畫")]
        [Required]
        public HttpPostedFileBase FILE_1 { get; set; }
        public string FILE_1_FILENAME { get; set; }

        /// <summary>
        /// 運用單位組織章程
        /// </summary>
        [Control(Mode = Control.FileUploadAppDoc, IsOpenNew = true, HoverFileName = "運用單位組織章程", block_toggle_group = 2)]
        [Display(Name = "運用單位組織章程")]
        [Required]
        public HttpPostedFileBase FILE_2 { get; set; }
        public string FILE_2_FILENAME { get; set; }

        /// <summary>
        /// 單位立案登記證書影本
        /// </summary>
        [Control(Mode = Control.FileUploadAppDoc, IsOpenNew = true, HoverFileName = "單位立案登記證書影本", block_toggle_group = 2)]
        [Display(Name = "單位立案登記證書影本")]
        [Required]
        public HttpPostedFileBase FILE_3 { get; set; }
        public string FILE_3_FILENAME { get; set; }

        /// <summary>
        /// 志工基本資料清冊
        /// </summary>
        [Control(Mode = Control.FileUploadAppDoc, IsOpenNew = true, HoverFileName = "志工基本資料清冊", block_toggle_group = 2)]
        [Display(Name = "志工基本資料清冊")]
        public HttpPostedFileBase FILE_4 { get; set; }
        public string FILE_4_FILENAME { get; set; }
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

                if (ErrorMsg == "")
                {
                    if (MERGEYN == "Y")
                    {
                        if (this.FILE_1 == null && this.FILE_2 == null && this.FILE_3 == null && this.FILE_4 == null)
                        {
                            ErrorMsg += "至少上傳一個檔案，";
                        }
                        else
                        {
                            if (this.FILE_1 != null)
                            { this.FILE_1_FILENAME = dao.PutFile("011001", this.FILE_1, "1").Replace("\\", "/"); }

                            if (this.FILE_2 != null)
                            { this.FILE_2_FILENAME = dao.PutFile("011001", this.FILE_2, "2").Replace("\\", "/"); }

                            if (this.FILE_3 != null)
                            { this.FILE_3_FILENAME = dao.PutFile("011001", this.FILE_3, "3").Replace("\\", "/"); }

                            if (this.FILE_4 != null)
                            { this.FILE_4_FILENAME = dao.PutFile("011001", this.FILE_4, "4").Replace("\\", "/"); }
                        }

                    }
                    else
                    {
                        if (this.FILE_1 == null)
                        { ErrorMsg += "志願服務運用計畫、"; }
                        else
                        { this.FILE_1_FILENAME = dao.PutFile("011001", this.FILE_1, "1").Replace("\\", "/"); }

                        if (this.FILE_2 == null)
                        { ErrorMsg += "運用單位組織章程、"; }
                        else
                        { this.FILE_2_FILENAME = dao.PutFile("011001", this.FILE_2, "2").Replace("\\", "/"); }

                        if (this.FILE_3 == null)
                        { ErrorMsg += "單位立案登記證書影本、"; }
                        else
                        { this.FILE_3_FILENAME = dao.PutFile("011001", this.FILE_3, "3").Replace("\\", "/"); }

                        if (this.FILE_4 != null)
                        { this.FILE_4_FILENAME = dao.PutFile("011001", this.FILE_4, "4").Replace("\\", "/"); }
                    }

                    if (ErrorMsg != "") { ErrorMsg += "未上傳檔案"; }
                }

            }
            return ErrorMsg;
        }
        #endregion



    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_011001DoneModel
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

}