using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    public class ServiceModel : BaseModel
    {
        [Display(Name = "案件編號")]
        public string ServiceId { get; set; }

        [Display(Name = "案件名稱")]
        public string Name { get; set; }

        [Display(Name = "是否上線")]
        public bool OnlineSMark { get; set; }

        [Display(Name = "是否開放線上申辦")]
        public bool OnlineNMark { get; set; }

        [Display(Name = "承辦單位代碼")]
        public int UnitCode { get; set; }

        /// <summary>
        /// 聯絡窗口-承辦單位
        /// </summary>
        [Display(Name = "聯絡窗口")]
        public string UnitName { get; set; }

        [Display(Name = "是否提供書表下載")]
        public bool FileMark { get; set; }

        [Display(Name = "是否提供輔助說明下載")]
        public bool HelpMark { get; set; }

        [Display(Name = "是否提供相關規範下載")]
        public bool NormMark { get; set; }

        [Display(Name = "是否公文介接")]
        public bool LeadMark { get; set; }
    }

    public class ServiceNoticeModel : ServiceModel
    {
        [Display(Name = "標題1")]
        public string Title1 { get; set; }

        [Display(Name = "內容1")]
        public string Content1 { get; set; }

        [Display(Name = "標題2")]
        public string Title2 { get; set; }

        [Display(Name = "內容2")]
        public string Content2 { get; set; }

        [Display(Name = "標題3")]
        public string Title3 { get; set; }

        [Display(Name = "內容3")]
        public string Content3 { get; set; }

        [Display(Name = "標題4")]
        public string Title4 { get; set; }

        [Display(Name = "內容4")]
        public string Content4 { get; set; }

        [Display(Name = "標題5")]
        public string Title5 { get; set; }

        [Display(Name = "內容5")]
        public string Content5 { get; set; }

        [Display(Name = "標題6")]
        public string Title6 { get; set; }

        [Display(Name = "內容6")]
        public string Content6 { get; set; }

        [Display(Name = "業務單位名稱")]
        public string PayUnit { get; set; }

        [Display(Name = "申請費用")]
        public int ApplyFee { get; set; }

        [Display(Name = "處理期限")]
        public int ProDeadline { get; set; }

        [Display(Name = "單位地址")]
        public string UnitAddress { get; set; }

        [Display(Name = "數位簽章")]
        public string CAType { get; set; }

        public string LST_ID { get; set; }
        public string ACT_TYPE { get; set; }
    }

    public class ServiceFileModel : ServiceModel
    {
        [Display(Name = "檔案編號")]
        public int FileId { get; set; }

        [Display(Name = "標題")]
        public string Title { get; set; }

        [Display(Name = "檔案名稱")]
        public string FileName { get; set; }

        [Display(Name = "類型")]
        public string FileType { get; set; }

        [Display(Name = "網址")]
        public string FileUrl { get; set; }
    }
}