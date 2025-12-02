using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using ES.Models;
using ES.Commons;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    /// <summary>
    /// DonateDetail 線上捐款明細
    /// </summary>
    public class ApplyDonateDetailViewModel
    {
        public ApplyDonateDetailViewModel()
        {
            this.Form = new ApplyDonateDetailFormModel();
            this.Grid = new List<ApplyDonateDetailGridModel>();
            this.Detail = new ApplyDonateDetailModifyModel();
        }
        public int NowPage { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
        public ApplyDonateDetailFormModel Form { get; set; }
        public List<ApplyDonateDetailGridModel> Grid { get; set; }
        public ApplyDonateDetailModifyModel Detail { get; set; }
    }
    public class ApplyDonateDetailFormModel : TblAPPLY_DONATE
    {
        public ApplyDonateDetailFormModel()
        {
        }

        public List<ApplyDonateDetailGridModel> Grid { get; set; }
        [Display(Name = "賑災專案")]
        public string SRV_ID_DONATE { get; set; }
        [Display(Name = "捐款人姓名")]
        public string NAME { get; set; }
        [Display(Name = "捐款方式")]
        public string PAYWAY { get; set; }
        [Display(Name = "是否已捐款")]
        public string ISPAY { get; set; }
        [Display(Name = "是否同意上傳國稅局")]
        public string ISAGREE { get; set; }
        [Display(Name = "CSV年度")]
        public string CSV_YEAR { get; set; }
        public IList<SelectListItem> APPLYDONATE_List
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.APPLYDONATE_list;
            }
        }
        public IList<SelectListItem> DONATE_PAYWAY_List
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                var li = new List<SelectListItem>();
                li.Add(new SelectListItem()
                {
                    Value = "",
                    Text = "請選擇"
                });
                li.AddRange(dao.DONATE_PAY_list);
                return li;
            }
        }
        public IList<SelectListItem> ISPAY_List
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                var li = new List<SelectListItem>();
                li.Add(new SelectListItem()
                {
                    Value = "",
                    Text = "請選擇"
                });
                li.AddRange(dao.YorN_list);
                return li;
            }
        }
        public IList<SelectListItem> ISAGREE_List
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                var li = new List<SelectListItem>();
                li.Add(new SelectListItem()
                {
                    Value = "",
                    Text = "請選擇"
                });
                li.AddRange(dao.YorN_list);
                return li;
            }
        }
    }
    public class ApplyDonateDetailGridModel : ApplyModel
    {
        /// <summary>
        /// 賑災專案名稱
        /// </summary>
        public string NAME_CH { get; set; }
        /// <summary>
        /// 捐款方式中文
        /// </summary>
        public string PAY_METHOD_NAME { get; set; }
        /// <summary>
        /// 是否已捐款
        /// </summary>
        public string PAY_STATUS_MK { get; set; }
        /// <summary>
        /// 同意上傳國稅局(請提供納稅義務人身份證/統一編號)
        /// </summary>
        public string DONOR_IDN { get; set; }
        public string ISDONOR { get; set; }
    }
    public class ApplyDonateDetailModifyModel
    {
        public ApplyDonateDetailModifyModel()
        {

        }
        #region 捐款人捐款資料
        /// <summary>
        /// 案號
        /// </summary>
        [Control(Mode = Control.Hidden, block_toggle = true, block_toggle_id = "BaseData", toggle_name = "捐款人捐款資料", block_toggle_group = 1)]
        public string APP_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRV_ID_DONATE { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string SRC_SRV_ID { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string ACC_NO { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string UNIT_CD { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string REC_MK { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string REC_ADDR_1 { get; set; }

        [Control(Mode = Control.Hidden, block_toggle_group = 1)]
        public string REC_ADDR_2 { get; set; }

        /// <summary>
        /// 個資使用說明
        /// </summary>
        [Display(Name = "個資使用說明")]
        [Control(Mode = Control.CheckBox, block_toggle_group = 1, IsOpenNew = true, IsReadOnly = true, checkBoxWord = "您以了解並同意本部蒐集、處理及使用您的個人資料。", group = 2)]
        [Required]
        public bool USE_MK { get; set; }

        /// <summary>
        /// 捐款形式
        /// </summary>
        [Display(Name = "捐款形式")]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsOpenNew = true, group = 3)]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 1, group = 4)]
        [Required]
        public string AMOUNT { get; set; }

        /// <summary>
        /// 捐款方式
        /// </summary>
        [Display(Name = "捐款方式")]
        [Control(Mode = Control.RadioGroup, block_toggle_group = 1, IsOpenNew = true, group = 5)]
        [Required]
        public string PAY_METHOD { get; set; }

        public IList<SelectListItem> PAY_METHOD_list
        {
            get
            {
                ShareCodeListModel dao = new ShareCodeListModel();
                return dao.DONATE_PAY_list;
            }
        }
        #endregion
        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle = true, block_toggle_id = "BaseData2", toggle_name = "捐款人基本資料", block_toggle_group = 2, group = 6)]
        [Required]
        public string DONOR_NAME { get; set; }

        /// <summary>
        /// 身份證/統一編號
        /// </summary
        [Display(Name = "身份證/統一編號")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 7)]
        [Required]
        public string IDN { get; set; }

        /// <summary>
        /// 電子信箱
        /// </summary>
        [Display(Name = "電子信箱")]
        [Control(Mode = Control.EMAIL, IsOpenNew = true, block_toggle_group = 2, group = 8)]
        [Required]
        public string DONOR_MAIL { get; set; }

        public DateTime? BIRTHDAY { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        [Control(Mode = Control.DatePicker, IsOpenNew = true, block_toggle_group = 2, group = 9)]
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
        [Control(Mode = Control.Tel, IsOpenNew = true, block_toggle_group = 2, group = 10)]
        [Required]
        public string DONOR_TEL { get; set; }

        /// <summary>
        /// 手機號碼
        /// </summary
        [Display(Name = "手機號碼")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 2, group = 11)]
        [Required]
        public string MOBILE { get; set; }

        /// <summary>
        /// 聯絡地址
        /// </summary>
        [Display(Name = "聯絡地址")]
        [Required]
        [Control(Mode = Control.ADDR, IsOpenNew = true, block_toggle_group = 2, group = 12)]
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
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle = true, block_toggle_id = "BaseData3", toggle_name = "收據資訊", block_toggle_group = 3, group = 13)]
        public string REC_TITLE { get; set; }

        /// <summary>
        /// 身份證/統一編號
        /// </summary
        [Display(Name = "同意上傳國稅局(請提供納稅義務人身份證/統一編號)")]
        [Control(Mode = Control.TextBox, IsOpenNew = true, block_toggle_group = 3, group = 14)]
        public string DONOR_IDN { get; set; }

        [Display(Name = "同捐款人基本資料")]
        [Control(Mode = Control.CheckBox, IsOpenNew = true, block_toggle_group = 3, checkBoxWord = "同意收據抬頭、身分證/統一編號同捐款人基本資料", group = 15)]
        public bool REF_MEM_MK { get; set; }

        [Display(Name = "捐款收據寄發方式")]
        [Control(Mode = Control.RadioGroup, IsOpenNew = true, block_toggle_group = 3, group = 16)]
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
    public class ApplyDonateCSVModel
    {
        /// <summary>
        /// 捐贈年度
        /// </summary>
        public string DON_YR { get; set; }
        /// <summary>
        /// 捐贈者身分證統一編號
        /// </summary>
        public string DON_IDN { get; set; }
        /// <summary>
        /// 捐贈者姓名
        /// </summary>
        public string DON_NM { get; set; }
        /// <summary>
        /// 捐款金額
        /// </summary>
        public string DON_AMT { get; set; }
        /// <summary>
        /// 受捐贈者統一編號(扣繳單位統一編號)
        /// </summary>
        public string DON_BAN { get; set; }
        /// <summary>
        /// 捐贈別
        /// </summary>
        public string DON_KD { get; set; }
        /// <summary>
        /// 受捐贈者名稱
        /// </summary>
        public string DONEE_NM { get; set; }
    }
}