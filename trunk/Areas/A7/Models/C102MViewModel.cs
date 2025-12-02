using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Commons;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using Turbo.Commons;
using Turbo.DataLayer;

namespace EECOnline.Areas.A7.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class C102MViewModel
    {
        public C102MViewModel()
        {
            this.form = new C102MFormModel();
        }

        /// <summary>
        /// 
        /// </summary>
        public C102MFormModel form { get; set; }

    }

    /// <summary>
    /// 查詢
    /// </summary>
    public class C102MFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 連結說明
        /// </summary>
        [Display(Name = "標題")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入查詢標題")]
        public string title { get; set; }

        /// <summary>
        /// 查詢結果 
        /// </summary>
        public IList<C102MGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 類別清單
    /// </summary>
    public class C102MGridModel : TblELINKS
    {
        /// <summary>
        /// 序號
        /// </summary>       
        public int? row_id { get; set; }

        public string FILEPATH { get; set; }
        public string SRCFILENAME { get; set; }
        public string EXTION { get; set; }

        public string IMGFILEPATH
        {
            get
            {
                return FILEPATH.SubstringTo(1) + "/" + SRCFILENAME + "." + EXTION;
            }
        }

    }
    /// <summary>
    /// 新增 / 編輯 友善連結
    /// </summary>
    public class C102MDetailModel : TblELINKS
    {
        /// <summary>
        /// 
        /// </summary>
        public C102MDetailModel()
        {
            this.IsNew = true;
            // 檔案上傳/下載元件
            Upload = new DynamicEFileGrid();
        }
        /// <summary>
        /// 設定上傳參數
        /// </summary>
        public void SetUploadParm()
        {
            Upload.ShowFileUpload = true;
            Upload.ShowDelete = true;
            Upload.peky1 = "ELINKS";
            Upload.limitRow = 1;
            if (!IsNew) Upload.peky2 = elinks_id.TONotNullString();
            Upload.GetFileGrid();
        }

        /// <summary>
        /// Detail必要控件(Hidden)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// PK
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string elinks_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string uptype { get; set; }

        /// <summary>
        /// 標題
        /// </summary>
        [Display(Name = "標題")]
        [Required]
        [Control(Mode = Control.TextBox, size = "48", maxlength = "32", placeholder = "請輸入標題")]
        public string title { get; set; }

        /// <summary>
        /// 超連結
        /// </summary>
        [Display(Name = "超連結")]
        [Required]
        [Control(Mode = Control.TextBox, size = "56", maxlength = "48", placeholder = "請輸入超連結")]
        public string url { get; set; }

        /// <summary>
        /// 檔案上傳套件
        /// </summary>
        [NotDBField]
        [Control(Mode = Control.EFILE)]
        public DynamicEFileGrid Upload { get; set; }
    }


}