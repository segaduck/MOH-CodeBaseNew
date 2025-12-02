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

namespace EECOnline.Areas.A6.Models
{
    /// <summary>
    /// 首頁
    /// </summary>
    public class C101MViewModel
    {
        public C101MViewModel()
        {
            this.form = new C101MFormModel();
        }

        /// <summary>
        /// 
        /// </summary>
        public C101MFormModel form { get; set; }

    }

    /// <summary>
    /// 首頁內容
    /// </summary>
    public class C101MFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 群組名稱
        /// </summary>
        [Display(Name = "群組名稱")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入群組名稱")]
        public string grpname { get; set; }

        /// <summary>
        /// 單位代碼
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string unit_cd { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        [Control(Mode = Control.DropDownList)]
        public string grp_status { get; set; }

        //// <summary>
        /// 是否啟用_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> grp_status_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.UorS_list;
            }
        }

        /// <summary>
        /// 查詢結果 
        /// </summary>
        public IList<C101MGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 群組清單
    /// </summary>
    public class C101MGridModel : TblAMGRP
    {
        /// <summary>
        /// 0 停用
        /// 1 使用
        /// </summary>       
        public string grp_status_nm { get; set; }

        /// <summary>
        /// 歸屬單位名稱
        /// </summary>
        public string unit_nm { get; set; }
    }

    /// <summary>
    /// 新增 / 編輯 群組
    /// </summary>
    public class C101MDetailModel : TblAMGRP
    {
        public C101MDetailModel()
        {
            this.IsNew = true;
        }

        /// <summary>
        /// Detail必要控件(Hidden)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public bool IsNew { get; set; }

        /// <summary>
        /// 群組ID
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string grp_id { get; set; }

        /// <summary>
        /// 群組名稱 (用於編輯時名稱比對)
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public string hdgrpname { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [Display(Name = "群組名稱")]
        [Required]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入群組名稱")]
        public string grpname { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [Display(Name = "群組所屬單位")]
        [Required]
        [Control(Mode = Control.UNIT, size = "26", maxlength = "16", placeholder = "請輸入群組名稱")]
        public string unit_cd { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [NotDBField]
        public string unit_cd_TEXT { get; set; }

        /// <summary>
        /// 群組狀態
        /// 0 停用
        /// 1 啟用
        /// </summary>
        [Display(Name = "群組狀態")]
        [Required]
        [Control(Mode = Control.RadioGroup)]
        public string grp_status { get; set; }

        /// <summary>
        /// 群組狀態_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> grp_status_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.StartorEed_list;
            }
        }
    }

    #region 權限設定

    public class C101MFuncmModel : TblAMFUNCM
    {
        /// <summary>
        /// 群組ID
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public string grp_id { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [Display(Name = "群組名稱")]
        [Control(Mode = Control.TextBox, size = "30", maxlength = "20", IsReadOnly = true)]
        public string grpname { get; set; }

        /// <summary>
        /// 系統名稱
        /// </summary>
        [Display(Name = "系統名稱")]
        [Control(Mode = Control.DropDownList)]
        public string sysid { get; set; }

        /// <summary>
        /// 系統名稱_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> sysid_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                SelectListItem item = new SelectListItem();
                item.Text = "請選擇";
                item.Value = "";
                var newlist = list.sysid_list;
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.Value).ToList();

                return newlist;
            }
        }

        ///// <summary>
        ///// 模組名稱
        ///// </summary>
        //[Display(Name = "模組名稱")]
        //[Control(Mode = Control.DropDownList)]
        //public string modules { get; set; }

        ///// <summary>
        ///// 模組名稱_清單
        ///// </summary>
        //[NotDBField]
        //public IList<SelectListItem> modules_list
        //{
        //    get
        //    {
        //        ShareCodeListModel model = new ShareCodeListModel();
        //        // 跟隨SYSID顯示資料
        //        var parms = new { sysid = sysid };
        //        var list = model.modules_list(parms);
        //        return list;
        //    }
        //}

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<C101MFuncmGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 群組清單
    /// </summary>
    public class C101MFuncmGridModel : TblAMGMAPM
    {
        /// <summary>
        /// 程式名稱
        /// </summary>
        [NotDBField]
        public string prgname { get; set; }

        /// <summary>
        /// 權限-查詢
        /// </summary>
        [NotDBField]
        public bool prg_q_check
        {
            get
            {
                return "1".Equals(this.prg_q) ? true : false;
            }
            set
            {
                this.prg_q = value ? "1" : "0";
            }
        }

        /// <summary>
        /// 權限-新增
        /// </summary>
        [NotDBField]
        public bool prg_i_check
        {
            get
            {
                return "1".Equals(this.prg_i) ? true : false;
            }
            set
            {
                this.prg_i = value ? "1" : "0";
            }
        }

        /// <summary>
        /// 權限-修改
        /// </summary>
        [NotDBField]
        public bool prg_u_check
        {
            get
            {
                return "1".Equals(this.prg_u) ? true : false;
            }
            set
            {
                this.prg_u = value ? "1" : "0";
            }
        }

        /// <summary>
        /// 權限-刪除
        /// </summary>
        [NotDBField]
        public bool prg_d_check
        {
            get
            {
                return "1".Equals(this.prg_d) ? true : false;
            }
            set
            {
                this.prg_d = value ? "1" : "0";
            }
        }

        /// <summary>
        /// 權限-列印
        /// </summary>
        [NotDBField]
        public bool prg_p_check
        {
            get
            {
                return "1".Equals(this.prg_p) ? true : false;
            }
            set
            {
                this.prg_p = value ? "1" : "0";
            }
        }
    }

    #endregion 權限設定
}