using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Commons;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using Turbo.Commons;
using Turbo.DataLayer;

namespace EECOnline.Areas.A6.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class C103MViewModel
    {
        public C103MViewModel()
        {
            this.form = new C103MFormModel();
        }

        /// <summary>
        /// 
        /// </summary>
        public C103MFormModel form { get; set; }

    }

    /// <summary>
    /// 首頁
    /// </summary>
    public class C103MFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 服務單位
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string unit_cd { get; set; }

        /// <summary>
        /// 僅案件處理身份者
        /// 0不是 1是 2是，輸入非自身帳號
        /// </summary>
        [Control(Mode = Control.Hidden)]
        [NotDBField]
        public string IsDealWith { get; set; }

        /// <summary>
        /// 服務單位
        /// </summary>
        [Display(Name = "服務單位")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入所屬單位")]
        public string unitnm { get; set; }

        /// <summary>
        /// 使用者帳號
        /// </summary>
        [Display(Name = "使用者帳號")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入帳號")]
        public string userno { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入姓名")]
        public string username { get; set; }

        /// <summary>
        /// 查詢結果 
        /// 若SQL有更動，請同步更新 GridAll
        /// </summary>
        public IList<C103MGridModel> Grid { get; set; }

        /// <summary>
        /// 查詢結果_匯出Excel 
        /// </summary>
        public IList<C103MGridAllModel> GridAll { get; set; }

        /// <summary>
        /// 群組權限一覽_匯出Excel 
        /// </summary>
        public IList<C103MGmapmGridModel> GmapmGrid { get; set; }
    }
    
    /// <summary>
    /// 帳號清單
    /// </summary>
    public class C103MGridModel : TblAMDBURM
    {
        /// <summary>
        /// 0 停用
        /// 1 使用
        /// </summary>       
        public string authstatus_nm { get; set; }

        /// <summary>
        /// 服務單位
        /// </summary>
        public string unit_nm { get; set; }

        /// <summary>
        /// 登入模式
        /// </summary>
        public string logintype { get; set; }

        /// <summary>
        /// 群組ID
        /// </summary>
        public int grp_id { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        public string grp_nm { get; set; }
    }

    /// <summary>
    /// 新增 / 編輯 群組
    /// </summary>
    public class C103MDetailModel : TblAMDBURM
    {
        /// <summary>
        /// 
        /// </summary>
        public C103MDetailModel()
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
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [Required]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "30", IsOpenNew =true)]
        public string userno { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        [Required]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "16", placeholder = "請輸入姓名")]
        public string username { get; set; }

        /// <summary>
        /// 服務單位
        /// </summary>
        [Display(Name = "服務單位")]
        [Required]
        [Control(Mode = Control.DropDownList)]
        public string unit_cd { get; set; }

        /// <summary>
        /// 服務狀態_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> unit_cd_list
        {
            get
            {
                A2DAO dao = new A2DAO();
                SessionModel sm = SessionModel.Get();
                ShareCodeListModel list = new ShareCodeListModel();
                Hashtable parms = new Hashtable();
                var utlist = list.unit_list1(parms);

                if (sm.UserInfo.User.unit_cd != "00" && sm.UserInfo.User.unit_cd != "01")
                {
                    TblAMUROLE ar = new TblAMUROLE();
                    ar.userno = sm.UserInfo.User.userno;
                    var data = dao.GetRowList(ar);

                    var datalist = new List<string>();
                    foreach (var item in data)
                    {
                        TblAMGRP ag = new TblAMGRP();
                        ag.grp_id = item.grp_id;
                        var agdata = dao.GetRow(ag);

                        if (agdata != null)
                        {
                            TblUNIT ut = new TblUNIT();
                            ut.unit_cd = agdata.unit_cd;
                            var utdata = dao.GetRow(ut);

                            if (utdata != null)
                            {
                                TblUNIT utc = new TblUNIT();
                                utc.unit_city = utdata.unit_city;
                                var utcdata = dao.GetRowList(utc);

                                foreach (var itemutc in utcdata)
                                {
                                    datalist.Add(itemutc.unit_nm);
                                }
                            }
                        }
                    }

                    var newlist = new List<SelectListItem>();
                    foreach (var item in utlist)
                    {
                        if (datalist.Contains(item.Text))
                        {
                            newlist.Add(item);
                        }
                    }

                    utlist = newlist.Distinct().ToList();
                }

                return utlist;
            }
        }

        /// <summary>
        ///<身分證後四碼>
        /// </summary>
        [Display(Name = "身分證後四碼")]
        [Required]
        [Control(Mode = Control.TextBox, size = "26", maxlength = "4")]
        public string idno { get; set; }

        /// <summary>
        /// 電子郵件
        /// </summary>
        [Display(Name = "電子郵件")]
        [Required]
        [Control(Mode = Control.TextBox, size = "46", maxlength = "100")]
        public string email { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public string modtime { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        [Display(Name = "申請日期")]
        [Required]
        [Control(Mode = Control.DatePicker, col = "3", IsReadOnly = true)]
        [NotDBField]
        public string modtime_AD
        {
            get
            {
                if (string.IsNullOrEmpty(modtime))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(modtime, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                modtime = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 啟用日期
        /// </summary>
        public string authdates { get; set; }

        /// <summary>
        /// 啟用日期
        /// </summary>
        [Display(Name = "啟用日期")]
        [Required]
        [Control(Mode = Control.DatePicker, col = "3")]
        [NotDBField]
        public string authdates_AD
        {
            get
            {
                if (string.IsNullOrEmpty(authdates))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(authdates, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                authdates = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 停用日期
        /// </summary>
        public string authdatee { get; set; }

        /// <summary>
        /// 停用日期
        /// </summary>
        [Display(Name = "停用日期")]
        [Required]
        [Control(Mode = Control.DatePicker,col ="3")]
        [NotDBField]
        public string authdatee_AD
        {
            get
            {
                if (string.IsNullOrEmpty(authdatee))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransTwToDateTime(authdatee, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                authdatee = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 帳號使用狀態
        /// </summary>
        [Display(Name = "帳號使用狀態")]
        [Required]
        [Control(Mode = Control.DropDownList)]
        public string authstatus { get; set; }

        /// <summary>
        /// 帳號使用狀態_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> authstatus_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.StoporStart_list;
            }
        }

        #region 登入模式

        //[Display(Name = "登入模式")]
        //[Required]
        //public string loginstatus { get; set; }

        //[Display(Name = "登入模式")]
        //[Required]
        //[Control(Mode = Control.CheckBoxList)]
        //public string[] loginstatus_show
        //{
        //    get
        //    {
        //        if (this.loginstatus != null)
        //        {
        //            return this.loginstatus.Replace("'", "").Split(',');
        //        }
        //        else
        //        {
        //            return new string[0];
        //        }
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            this.loginstatus = string.Join(",", value.ToList());
        //        }
        //    }
        //}

        ///// <summary>
        ///// 登入模式
        ///// </summary>
        //public IList<CheckBoxListItem> loginstatus_show_list
        //{
        //    get
        //    {
        //        ShareCodeListModel list = new ShareCodeListModel();
        //        return list.LoginStatus_chk_list;
        //    }
        //}

        #endregion 登入模式

    }

    /// <summary>
    /// 群組權限
    /// </summary>
    public class C103MGrpModel :TblAMGRP
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string userno { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        [Control(Mode = Control.Hidden)]
        public string unit_cd { get; set; }

        /// <summary>
        /// 查詢結果 GridModel
        /// </summary>
        public IList<C103MGrpGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 群組權限
    /// </summary>
    public class C103MGrpGridModel : TblAMGRP
    {
        /// <summary>
        /// 帳號
        /// </summary>
        public string userno { get; set; }

        ///// <summary>
        ///// 群組名稱
        ///// 0 停用
        ///// 1 啟用
        ///// </summary>
        //[Display(Name = "群組名稱")]
        //[Required]
        //[Control(Mode = Control.DropDownList)]
        //public string grp_id { get; set; }

        ///// <summary>
        ///// 群組名稱_清單
        ///// </summary>
        //[NotDBField]
        //public IList<SelectListItem> grp_id_list
        //{
        //    get
        //    {
        //        ShareCodeListModel list = new ShareCodeListModel();
        //        return list.amgrp_list;
        //    }
        //}

        /// <summary>
        /// 勾選框
        /// </summary>
        [NotDBField]
        public string IsCheck { get; set; }

        /// <summary>
        /// 勾選框(Show)
        /// </summary>
        [NotDBField]
        public bool Is_Check
        {
            get
            {
                return IsCheck == "1";
            }
            set
            {
                IsCheck = value ? "1" : "0";
            }
        }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [NotDBField]
        public string grpname { get; set; }
    }

    /// <summary>
    /// 帳號清單_匯出Excel
    /// </summary>
    public class C103MGridAllModel : TblAMDBURM
    {
        /// <summary>
        /// 0 停用
        /// 1 使用
        /// 2 帳號須變更密碼
        /// </summary>       
        public string authstatus_nm { get; set; }

        /// <summary>
        /// 服務單位
        /// </summary>
        public string unit_nm { get; set; }

        /// <summary>
        /// 登入模式
        /// </summary>
        public string logintype { get; set; }

        /// <summary>
        /// 群組ID
        /// </summary>
        public int grp_id { get; set; }

        /// <summary>
        /// 群組名稱_全部
        /// </summary>
        public string grp_nm { get; set; }

        /// <summary>
        /// 原建檔單位
        /// </summary>
        public string insert_unit_nm { get; set; }
    }

    /// <summary>
    /// 所屬群組權限_匯出Excel
    /// </summary>
    public class C103MGmapmGridModel : TblAMGRP
    {
        /// <summary>
        /// 群組ID
        /// </summary>
        public int grp_id { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        public string grpname { get; set; }
        
        /// <summary>
        /// 權限一覽_代碼
        /// </summary>
        public string prgname { get; set; }
    }
}