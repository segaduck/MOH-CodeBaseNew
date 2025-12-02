using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Commons;

namespace ES.Areas.Admin.Models
{
    public class CaseDetailModel
    {
        public CaseDetailModel()
        {
            this.detail_base = new DetailViewModel();
            this.detail_001037 = new Detail001037Model();
        }

        /// <summary>
        /// 申請案件
        /// </summary>
        public DetailViewModel detail_base { get; set; }

        /// <summary>
        /// 申請案件 醫事人員請領無懲戒紀錄證明申請書
        /// </summary>
        public Detail001037Model detail_001037 { get; set; }

    }
    public class DetailViewModel
    {

        //[Display(Name = "申請項目")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true, group = 1, form_id = "C101MA", block_BIG_id = "A", block_toggle_group = 1, block_toggle_id = "A1_toggle", block_toggle = true, toggle_name = "基本資料檔")]
        public string SRV_ID { get; set; }

        //[Display(Name = "申請項目名稱")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true, group = 1, block_toggle_group = 1)]
        public string SRV_NAME { get; set; }
        public DateTime? APP_TIME { get; set; }

        //[Display(Name = "申辦日期")]
        //[Control(Mode = Control.TextBox, HtmlAttribute = "dateformat='yyyy/MM/dd'", IsReadOnly = true, block_toggle_group = 1)]
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
                    return HelperUtil.DateTimeToString(APP_TIME);     // YYYY/MM/DD 回傳給系統
                }
            }
            set
            {
                APP_TIME = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        //[Display(Name = "案件進度")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true, group = 2, block_toggle_group = 1)]
        public string FLOW_CD { get; set; }

        //[Display(Name = "案件狀態")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true, group = 2, block_toggle_group = 1)]
        public string CASE_STATUS { get; set; }

        //[Display(Name = "申辦編號")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APP_ID { get; set; }

        //[Display(Name = "申請人身分證號/統編")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string IDN { get; set; }

        //[Display(Name = "公文文號")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string MOHW_CASE_NO { get; set; }

        //[Display(Name = "申請人姓名")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string NAME { get; set; }

        //[Display(Name = "繳費方式")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string PAY_METHOD { get; set; }

        //[Display(Name = "繳費時機")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string PAY_POINT { get; set; }

        public string PRO_ACC { get; set; }

        public string PRO_UNIT_CD { get; set; }



        public string PAY_A_FEE { get; set; }

        public string PAY_A_PAID { get; set; }

        public string PAY_C_FEE { get; set; }

        //[Display(Name = "承辦單位")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string UNIT_CD { get; set; }

        //[Display(Name = "承辦人員")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string ACC_NAME { get; set; }

        //[Display(Name = "退件註記")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public bool CASE_BACK_MK { get; set; }

        //[Display(Name = "退款註記")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public bool PAY_BACK_MK { get; set; }

        public DateTime? APP_STR_DATE { get; set; }
        //[Display(Name = "處理起算日")]
        //[Control(Mode = Control.TextBox, HtmlAttribute = "dateformat='yyyy/MM/dd'", IsReadOnly = true)]
        public string APP_STR_DATE_AD
        {
            get
            {
                if (APP_STR_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(APP_STR_DATE);     // YYYY/MM/DD 回傳給系統
                }
            }
            set
            {
                APP_STR_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }
    }

    public class Detail001037Model
    {
        [Display(Name = "申請項目")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, group = 1, form_id = "C101MA", block_BIG_id = "A", block_toggle_group = 1, block_toggle_id = "A1_toggle", block_toggle = true, toggle_name = "基本資料檔")]
        public string SRV_ID { get; set; }

        [Display(Name = "申請項目名稱")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, group = 1, block_toggle_group = 1)]
        public string SRV_NAME { get; set; }
        public DateTime? APP_TIME { get; set; }

        [Display(Name = "申辦日期")]
        [Control(Mode = Control.TextBox, HtmlAttribute = "dateformat='yyyy/MM/dd'", IsReadOnly = true, block_toggle_group = 1)]
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
                    return HelperUtil.DateTimeToString(APP_TIME);     // YYYY/MM/DD 回傳給系統
                }
            }
            set
            {
                APP_TIME = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }

        [Display(Name = "案件進度")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, group = 2, block_toggle_group = 1)]
        public string FLOW_CD { get; set; }

        [Display(Name = "申辦編號")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APP_ID { get; set; }

        [Display(Name = "申請人姓名")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_NAME { get; set; }

        [Display(Name = "出生年月日")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_BIR { get; set; }

        [Display(Name = "國民身分證統一編號")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_IDNO { get; set; }

        [Display(Name = "戶籍地址")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_ADD { get; set; }

        [Display(Name = "連絡電話")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_TEL { get; set; }

        [Display(Name = "行動電話")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_MOBIL { get; set; }

        [Display(Name = "E-MAIL")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_EMAIL { get; set; }

        [Display(Name = "醫事人員或公共衛生師證書字號")]
        [Control(Mode = Control.TextBox, IsReadOnly = true,group =3, block_toggle_group = 1)]
        public string APPLY_NUM { get; set; }

        [Display(Name = "證書發給日期")]
        [Control(Mode = Control.TextBox, HtmlAttribute = "dateformat='yyyy/MM/dd'", IsReadOnly = true,group =3, block_toggle_group = 1)]
        public string APPLY_NUMDATE { get; set; }

        [Display(Name = "申請份數")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, group = 4, block_toggle_group = 1)]
        public string APPLY_CNT { get; set; }

        [Display(Name = "金額 (NTD)")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, group = 4, block_toggle_group = 1)]
        public string APPLY_NTD { get; set; }

        [Display(Name = "本證明書郵寄地址(收件者)")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, group = 5, block_toggle_group = 1)]
        public string APPLY_SEND { get; set; }

        [Display(Name = "本證明書郵寄地址(國別)")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, group = 5, block_toggle_group = 1)]
        public string APPLY_SCOUNTRY { get; set; }

        [Display(Name = "本證明書郵寄地址")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_SADD { get; set; }

        [Display(Name = "申請理由")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_REASON { get; set; }

        [Display(Name = "出國使用")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_OUTREASON { get; set; }

        [Display(Name = "前往國家")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 1)]
        public string APPLY_OUTCOUNTRY { get; set; }

        [Display(Name = "上傳檔案")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, form_id = "C101MB", block_BIG_id = "B", block_toggle_group = 2, block_toggle_id = "A2_toggle", block_toggle = true, toggle_name = "上傳應附檔案")]
        public string FILEGROUP { get; set; }

        [Display(Name = "完成繳/退費")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, form_id = "C101MC", block_BIG_id = "C", block_toggle_group = 3, block_toggle_id = "A3_toggle", block_toggle = true, toggle_name = "繳費情形")]
        public string PAY_STATUS { get; set; }

        [Display(Name = "繳費方式")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group =3)]
        public string PAY_WAY { get; set; }

        [Display(Name = "交易日期")]
        [Control(Mode = Control.TextBox, HtmlAttribute = "dateformat='yyyy/MM/dd'", IsReadOnly = true, block_toggle_group = 3)]
        public string PAY_CDATE { get; set; }

        [Display(Name = "繳費金額")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 3)]
        public string PAY_MONEY { get; set; }

        [Display(Name = "交易金額")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 3)]
        public string PAY_AMOUNT { get; set; }

        [Display(Name = "交易狀態")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group = 3)]
        public string PAY_CONDITION { get; set; }

        [Display(Name = "繳費日期")]
        [Control(Mode = Control.TextBox, HtmlAttribute = "dateformat='yyyy/MM/dd'", IsReadOnly = true, block_toggle_group = 3)]
        public string PAY_DATE { get; set; }

        [Display(Name = "已繳費")]
        [Control(Mode = Control.CheckBox, IsReadOnly = true, block_toggle_group = 3)]
        public string PAY_IS { get; set; }

        [Display(Name = "案件進度")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, form_id = "C101MD", block_BIG_id = "D", block_toggle_group = 4, block_toggle_id = "A4_toggle", block_toggle = true, toggle_name = "案件進度歷程")]
        public string CASE_STATUS { get; set; }

        [Display(Name = "案件狀態修改")]
        [Control(Mode = Control.TextBox, IsReadOnly = true, block_toggle_group =4)]
        public string CASE_UPDATE { get; set; }

        [Display(Name = "案件歷程記錄")]
        [Control(Mode = Control.TextArea, IsReadOnly = true, block_toggle_group = 4)]
        public string CASE_HISTORY { get; set; }

        public DateTime? APP_STR_DATE { get; set; }
        //[Display(Name = "處理起算日")]
        //[Control(Mode = Control.TextBox, HtmlAttribute = "dateformat='yyyy/MM/dd'", IsReadOnly = true)]
        public string APP_STR_DATE_AD
        {
            get
            {
                if (APP_STR_DATE == null)
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(APP_STR_DATE);     // YYYY/MM/DD 回傳給系統
                }
            }
            set
            {
                APP_STR_DATE = HelperUtil.TransToDateTime(value);         // YYYMMDD 民國年 使用者看到
            }
        }
        //[Display(Name = "承辦人員")]
        //[Control(Mode = Control.TextBox, IsReadOnly = true)]
        public string ACC_NAME { get; set; }
    }
}