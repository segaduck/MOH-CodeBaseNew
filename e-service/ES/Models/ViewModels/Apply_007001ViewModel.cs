using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Commons;
using ES.DataLayers;
using System.Web.Mvc;
using System.Web.Configuration;
using CTCB.Crypto;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// APPLY_007001 線上捐款 APPLY_007001_DATA 收據資料
    /// </summary>
    public class Apply_007001ViewModel : Apply_007001Model
    {
        public Apply_007001ViewModel()
        {
        }
    }

    /// <summary>
    /// 完成畫面
    /// </summary>
    public class Apply_007001DoneModel
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

    public class Apply_007001FormModel : Apply_007001Model
    {
        public Apply_007001FormModel()
        {
            this.IsNew = true;
            this.USE_MK = true;
            this.PayModel = new Apply_007001PayViewModel();
            this.LinePayModel = new LinePayViewModel();
        }
        
        /// <summary>
        /// 繳費
        /// </summary>
        public Apply_007001PayViewModel PayModel { get; set; }
        public LinePayViewModel LinePayModel { get; set; }
        /// <summary>
        /// 新增
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// 繳費
        /// </summary>
        public string PAY_POINT { get; set; }
        public string ReturnMsg { get; set; }

        #region 捐款人捐款資料
        /// <summary>
        /// 案號
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle = true, toggle_name = "捐款人捐款資料", block_toggle_group = 1)]
        public string APP_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRV_ID_DONATE { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRC_SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string ACC_NO { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string UNIT_CD { get; set; }

        /// <summary>
        /// 個資使用說明
        /// </summary>
        [Display(Name = "個資使用說明")]
        [Control(Mode = Control.CheckBox, block_toggle_group = 1, IsOpenNew = true, IsReadOnly = true, checkBoxWord = "您以了解並同意本部蒐集、處理及使用您的個人資料。")]
        [Required]
        public bool USE_MK { get; set; }

        /// <summary>
        /// 捐款形式
        /// </summary>
        [Display(Name = "捐款形式")]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsOpenNew = true)]
        [Required]
        public string PUBLIC_MK { get; set; }

        public IList<SelectListItem> PUBLIC_MK_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.PUBLIC_MK_list;
            }
        }
        /// <summary>
        /// 捐款金額
        /// </summary
        [Display(Name = "捐款金額")]
        [Control(Mode = Control.DonateAmount, IsOpenNew = true, block_toggle_group = 1)]
        [Required]
        public string AMOUNT { get; set; }

        /// <summary>
        /// 捐款方式
        /// </summary>
        [Display(Name = "捐款方式")]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsOpenNew = true)]
        [Required]
        public string PAY_METHOD { get; set; }

        public IList<SelectListItem> PAY_METHOD_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                var list = dao.DONATE_PAY_list;
                var newlist = new List<SelectListItem>(list);
                foreach (var item in list)
                {
                    if (!PAY_METHOD.Contains(item.Value))
                    {
                        newlist.Remove(item);
                    }
                }
                return newlist;
            }
        }
        #endregion
        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle = true, toggle_name = "捐款人基本資料", block_toggle_group = 2)]
        [Required]
        public string DONOR_NAME { get; set; }

        /// <summary>
        /// 身份證/統一編號
        /// </summary
        [Display(Name = "身份證/統一編號")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2)]
        [Required]
        public string IDN { get; set; }

        /// <summary>
        /// 電子信箱
        /// </summary>
        [Display(Name = "電子信箱")]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 2)]
        [Required]
        public string DONOR_MAIL { get; set; }

        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 2)]
        [Required]
        public string BIRTHDAY_AD
        {
            get
            {
                if (BIRTHDAY == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(BIRTHDAY);     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                BIRTHDAY = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 連絡電話
        /// </summary>
        [Display(Name = "連絡電話")]
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 2)]
        [Required]
        public string DONOR_TEL { get; set; }

        /// <summary>
        /// 手機號碼
        /// </summary
        [Display(Name = "手機號碼")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2)]
        [Required]
        public string MOBILE { get; set; }

        /// <summary>
        /// 聯絡地址
        /// </summary>
        [Display(Name = "聯絡地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 2)]
        public string ADDR_CODE { get; set; }

        public string ADDR_CODE_ADDR { get; set; }

        public string ADDR_CODE_DETAIL
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
        /// 收據抬頭
        /// </summary>
        [Display(Name = "收據(徵信)抬頭")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle = true, toggle_name = "收據資訊", block_toggle_group = 3)]
        public string REC_TITLE { get; set; }

        /// <summary>
        /// 身份證/統一編號
        /// </summary
        [Display(Name = "同意上傳國稅局(請提供納稅義務人身份證/統一編號)")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 3)]
        public string DONOR_IDN { get; set; }

        [Display(Name = "同捐款人基本資料")]
        [Control(Mode = Control.CheckBox, IsOpenNew = true, block_toggle_group = 3, checkBoxWord = "同意收據抬頭、身分證/統一編號同捐款人基本資料")]
        public bool REF_MEM_MK { get; set; }

        [Display(Name = "捐款收據寄發方式")]
        [Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle_group = 3)]
        public string REC_TITLE_CD { get; set; }

        public IList<SelectListItem> REC_TITLE_CD_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.REC_TITLE_CD_list;
            }
        }

    }

    public class Apply_007001TitleModel : Apply_007001_DataModel
    {

    }

    public class Apply_007001PayViewModel
    {
        public Apply_007001PayViewModel()
        {
            this.enc = new Encrypt();
        }
        public string ClientIp { get; set; }
        public string PostWay { get; set; }
        public string UrlmerId { get; set; }
        public Encrypt enc { get; set; }
        public string IsWebTestEnvir1
        {
            get
            {
                return WebConfigurationManager.AppSettings["WebTestEnvir"];
            }
        }
    }   
}