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

namespace EECOnline.Areas.A8.Models
{
    /// <summary>
    /// 首頁
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
    /// 首頁內容
    /// </summary>
    public class C103MFormModel : PagingResultsViewModel
    {

        /// <summary>
        /// 寄信日期起
        /// </summary>
        public string apy_time_st { get; set; }

        [Display(Name = "寄信日期起")]
        [Control(Mode = Control.DatePicker, group = 1)]
        [NotDBField]
        public string apy_time_st_ad
        {
            get
            {
                if (string.IsNullOrEmpty(apy_time_st))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransTwLongToDateTime(apy_time_st));
                }
            }
            set
            {
                apy_time_st = HelperUtil.DateTimeToLongTwString(HelperUtil.TransToDateTime(value));
            }
        }

        /// <summary>
        /// 寄信日期迄
        /// </summary>
        public string apy_time_ed { get; set; }

        [Display(Name = "寄信日期迄")]
        [Control(Mode = Control.DatePicker, group = 1)]
        [NotDBField]
        public string apy_time_ed_ad
        {
            get
            {
                if (string.IsNullOrEmpty(apy_time_ed))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(((DateTime)(HelperUtil.TransTwLongToDateTime(apy_time_ed))).AddDays(-1));
                }
            }
            set
            {
                try
                {
                    // 由於時間會取到小時，所以這邊先加一已防找不到當天的資料
                    apy_time_ed = HelperUtil.DateTimeToLongTwString(((DateTime)HelperUtil.TransToDateTime(value)).AddDays(1));
                }
                catch (Exception)
                {
                    apy_time_ed = null;
                }

            }
        }

        /// <summary>
        /// 收件人帳號
        /// </summary>
        [Display(Name = "收件人帳號")]
        [Control(Mode = Control.TextBox, group =2, placeholder = "請輸入收件人帳號")]
        public string usedMod { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "收件人姓名")]
        [Control(Mode = Control.TextBox, group =2, placeholder = "請輸入收件人姓名")]
        public string usedModname { get; set; }

        /// <summary>
        /// 收件人信箱
        /// </summary>
        [Display(Name = "收件人信箱")]
        [Control(Mode = Control.TextBox, group =3, placeholder = "請輸入收件人信箱")]
        public string usedModmail { get; set; }

        /// <summary>
        /// 功能
        /// </summary>
        [Display(Name = "功能")]
        [Control(Mode = Control.DropDownList, group =3)]
        public string usedfunc { get; set; }

        ///<summary>
        /// 功能_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> usedfunc_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.usedfunc_list;
            }
        }

        /// <summary>
        /// 寄信狀態
        /// </summary>
        [Display(Name = "寄信狀態")]
        [Control(Mode = Control.DropDownList)]
        public string mail_status { get; set; }

        /// <summary>
        /// 寄信狀態_清單
        /// </summary>
        [NotDBField]
        public IList<SelectListItem> mail_status_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.mail_status_list;
            }
        }

        /// <summary>
        /// 單位
        /// </summary>
        public string unit_cd { get; set; }

        /// <summary>
        /// 查詢結果 
        /// </summary>
        public IList<C103MGridModel> Grid { get; set; }
    }
    
    /// <summary>
    /// 群組清單
    /// </summary>
    public class C103MGridModel : TblAMEMAILLOG_EMAIL
    {
        /// <summary>
        /// 1 成功
        /// else 失敗
        /// </summary>       
        public string status { get; set; }
        
    }


    public enum Mail_type_Enum
    {
        忘記密碼 = 1,
        前臺案件申辦 = 2,
        後臺案件變更 = 3,
    }
}