using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using System.Web.Mvc;
using ES.Models;
using ES.DataLayers;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel
    /// </summary>
    public class Apply_011001ViewModel
    {
        public Apply_011001ViewModel()
        {
        }

        /// <summary>
        /// Form
        /// </summary>
        public Apply_011001FormModel Form { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Apply_011001FormModel Detail { get; set; }


    }

    /// <summary>
    /// 表單填寫
    /// </summary>
    public class Apply_011001FormModel : ApplyModel
    {
        public Apply_011001FormModel()
        {
            this.IsNew = true;
            this.fileMode = new Apply_011001FileModel();
        }

        public bool IsNew { get; set; }

        /// <summary>
        /// 申辦日期
        /// </summary>
        [Display(Name = "申辦日期")]
        public string APP_TIME { get; set; }

        /// <summary>
        /// 案號
        /// </summary>
        [Display(Name = "案號")]
        public string APP_ID { get; set; }

        #region 基本資料檔
        /// <summary>
        /// 運用單位登入帳號
        /// </summary>
        [Display(Name = "運用單位登入帳號")]
        [Control(Mode = Control.Lable, block_toggle = true, block_toggle_id = "BaseData", toggle_name = "基本資料檔(點擊展開)", block_toggle_group = 1, group = 1)]
        public string ACC_NO { get; set; }

        /// <summary>
        /// 運用單位名稱
        /// </summary>
        [Display(Name = "運用單位名稱")]
        [Control(Mode = Control.Lable, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NAM { get; set; }

        [Display(Name = "運用單位電話")]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_TEL { get; set; }

        [Display(Name = "運用單位地址(含郵遞區號)")]
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
        [Display(Name = "承辦人姓名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_NAM { get; set; }

        /// <summary>
        /// 承辦人行動電話
        /// </summary>
        [Display(Name = "承辦人行動電話")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_MOBILE { get; set; }

        [Display(Name = "承辦人E-MAIL")]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 1)]
        public string ADM_MAIL { get; set; }


        public DateTime? ACC_SDATE { get; set; }

        [Display(Name = "運用單位發文日期")]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_SDATE_AD
        {
            get
            {
                if (ACC_SDATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(ACC_SDATE,"/");     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                ACC_SDATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 運用單位發文字號
        /// </summary>
        [Display(Name = "運用單位發文字號")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1)]
        public string ACC_NUM { get; set; }

        #endregion

        #region 佐證文件檔案上傳
        ///// <summary>
        ///// 佐證文件採合併檔案
        ///// </summary>
        //[Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle_id = "FileData", block_toggle = true, toggle_name = "佐證文件檔案上傳", block_toggle_group = 2)]
        //[Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; }

        //public IList<SelectListItem> MERGEYN_list
        //{
        //    get
        //    {
        //        ShareCodeListModel dao = new ShareCodeListModel();
        //        return dao.YorN_list;
        //    }
        //}

        ///// <summary>
        ///// 志願服務運用計畫
        ///// </summary>
        //[Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "志願服務運用計畫", block_toggle_group = 2)]
        //[Display(Name = "志願服務運用計畫")]
        ////[Required]
        //public HttpPostedFileBase FILE_SERVICE { get; set; }
        //public string FILE_SERVICE_FILENAME { get; set; }

        ///// <summary>
        ///// 運用單位組織章程
        ///// </summary>
        //[Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "運用單位組織章程", block_toggle_group = 2)]
        //[Display(Name = "運用單位組織章程")]
        ////[Required]
        //public HttpPostedFileBase FILE_UNIT { get; set; }
        //public string FILE_UNIT_FILENAME { get; set; }

        ///// <summary>
        ///// 單位立案登記證書影本
        ///// </summary>
        //[Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "單位立案登記證書影本", block_toggle_group = 2)]
        //[Display(Name = "單位立案登記證書影本")]
        ////[Required]
        //public HttpPostedFileBase FILE_CERTIFICATE { get; set; }
        //public string FILE_CERTIFICATE_FILENAME { get; set; }

        ///// <summary>
        ///// 志工基本資料清冊
        ///// </summary>
        //[Control(Mode = Control.FileUpload, IsOpenNew = true, HoverFileName = "志工基本資料清冊", block_toggle_group = 2)]
        //[Display(Name = "志工基本資料清冊")]
        //public HttpPostedFileBase FILE_BASIC { get; set; }
        //public string FILE_BASIC_FILENAME { get; set; }


        ///// <summary>
        ///// 檔案打包下載
        ///// </summary>
        //[Display(Name = "檔案打包下載")]
        //[Control(Mode = Control.ZipButton, CaseName = "志願服務計畫核備", block_toggle_group = 6)]
        //public string ZipButton { get; set; }
        #endregion


        /// <summary>
        /// 系統狀態
        /// </summary>
        public string APP_STATUS { get; set; }

        /// <summary>
        /// 預計完成日期
        /// </summary>
        [Display(Name = "預計完成日期")]
        public string APP_EXT_DATE { get; set; }

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        [Display(Name = "承辦人姓名")]
        public string PRO_NAM { get; set; }

        /// <summary>
        /// 案件進度
        /// </summary>
        [Display(Name = "案件進度")]
        public string FLOW_CD_TEXT { get; set; }

        /// <summary>
        /// 案件狀態修改
        /// </summary>
        [Display(Name = "案件狀態修改")]
        public string FLOW_CD { get; set; }
        
        public string FileCheck { get; set; }

        public string NOTE { get; set; }

        public Apply_011001FileModel fileMode { get; set; }
    }

    /// <summary>
    /// 檔案下載
    /// </summary>
    public class Apply_011001FileModel
    {
        public string APP_ID { get; set; }

        /// <summary>
        /// 志願服務運用計畫
        /// </summary>
        [Display(Name = "志願服務運用計畫")]
        public string FILE_1 { get; set; }
        public string FILE_1_TEXT { get; set; }

        /// <summary>
        /// 運用單位組織章程
        /// </summary>
        [Display(Name = "運用單位組織章程")]
        public string FILE_2 { get; set; }
        public string FILE_2_TEXT { get; set; }

        /// <summary>
        /// 單位立案登記證書影本
        /// </summary>
        [Display(Name = "單位立案登記證書影本")]
        public string FILE_3 { get; set; }
        public string FILE_3_TEXT { get; set; }

        /// <summary>
        /// 志工基本資料清冊
        /// </summary
        [Display(Name = "志工基本資料清冊")]
        public string FILE_4 { get; set; }
        public string FILE_4_TEXT { get; set; }

        public List<FileGroupModel> FILENAM { get; set; }
    }

}