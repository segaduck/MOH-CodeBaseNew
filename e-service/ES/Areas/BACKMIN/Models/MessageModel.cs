using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ES.Areas.Admin.Models
{
    public class MessageModel
    {
        [Display(Name = "消息編號")]
        public int MessageID { get; set; }

        [Display(Name = "標題")]
        public virtual string Title { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "開始時間")]
        public virtual DateTime? StartTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "結束時間")]
        public virtual DateTime? EndTime { get; set; }

        /// <summary>
        /// 是否寄送 SEND_MAIL_MK	Varchar(1)	寄送通知 SendMailMark Y/N
        /// </summary>
        [Display(Name = "是否寄送")]
        public bool SendMailMark { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }

        /// <summary>
        /// MESSAGE_TYPE nvarchar(1)	公告種類 S:停機 P:繳費 I:重要
        /// </summary>
        [Display(Name = "公告種類")]
        public string MESSAGE_TYPE { set; get; }
    }

    public class MessageActionModel : MessageModel
    {
        [Display(Name = "頁數")]
        public int NowPage { get; set; }

        [Display(Name = "動作流水號")]
        public int ActionId { get; set; }

        [Display(Name = "動作類型")]
        public string ActionType { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [Display(Name = "開始時間")]
        public override DateTime? StartTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [Display(Name = "結束時間")]
        public override DateTime? EndTime { get; set; }
    }

    public class MessageEditModel : MessageModel
    {
        [Display(Name = "發布單位")]
        public int UnitCode;

        [Required(ErrorMessage = "請選擇標題")]
        [Display(Name = "類別")]
        public string[] Category { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "請輸入標題")]
        [Display(Name = "標題")]
        public override string Title { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "請輸入內容")]
        [Display(Name = "內容")]
        public string Content { get; set; }

        [Required(ErrorMessage = "上線時間（起）")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "上線時間（起）")]
        public override DateTime? StartTime { get; set; }

        [Required(ErrorMessage = "上線時間（迄）")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mi}")]
        [Display(Name = "上線時間（迄）")]
        public override DateTime? EndTime { get; set; }

        [Display(Name = "附件檔案一")]
        public string FileName1 { get; set; }
        public HttpPostedFileBase UpdateFile1 { get; set; }
        

        [Display(Name = "附件檔案二")]
        public string FileName2 { get; set; }
        public HttpPostedFileBase UpdateFile2 { get; set; }

        [Display(Name = "附件檔案三")]
        public string FileName3 { get; set; }
        public HttpPostedFileBase UpdateFile3 { get; set; }

        //[Display(Name = "是否寄送")]
        //public bool SendMailMark { get; set; }

        [Display(Name = "主題分類代碼")]
        public string ClassSubCode { get; set; }

        [Display(Name = "施政分類代碼")]
        public string ClassAdmCode { get; set; }

        [Display(Name = "服務分類代碼")]
        public string ClassSrvCode { get; set; }

        [Display(Name = "關鍵字")]
        public string KeyWord { get; set; }
    }

    public class MessageShowModel : MessageModel
    {
        [Display(Name = "發布單位代碼")]
        public int UnitCode { get; set; }

        [Display(Name = "發布單位")]
        public string UnitName { get; set; }

        [Display(Name = "類別")]
        public string[] Category { get; set; }

        [Display(Name = "上線時間（起）")]
        public override DateTime? StartTime { get; set; }

        [Display(Name = "上線時間（迄）")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "附件檔案一")]
        public string FileName1 { get; set; }

        [Display(Name = "附件檔案二")]
        public string FileName2 { get; set; }

        [Display(Name = "附件檔案三")]
        public string FileName3 { get; set; }

        [Display(Name = "是否寄送")]
        public bool SendMailMark { get; set; }

        [Display(Name = "主題分類代碼")]
        public string ClassSubCode { get; set; }

        [Display(Name = "施政分類代碼")]
        public string ClassAdmCode { get; set; }

        [Display(Name = "服務分類代碼")]
        public string ClassSrvCode { get; set; }

        [Display(Name = "關鍵字")]
        public string KeyWord { get; set; }
 

    }
}