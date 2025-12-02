using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Commons;

namespace ES.Models
{
    public class MessageModel : BaseModel
    {
        [Display(Name = "消息編號")]
        public int MessageID { get; set; }

        [Display(Name = "標題")]
        public string Title { get; set; }

        [Display(Name = "內容")]
        public string Content { get; set; }

        [Display(Name = "公告時間")]
        public virtual DateTime? StartTime { get; set; }
    }

    public class MessageActionModel : MessageModel
    {
        //[Display(Name = "開始時間")]
        //public override DateTime? StartTime { get; set; }

        //[Display(Name = "結束時間")]
        //public DateTime? EndTime { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        [Display(Name = "開始時間")]
        public string StartTime { get; set; }

        public string StartTime_AD
        {
            get
            {
                if (string.IsNullOrEmpty(StartTime))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(StartTime, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                StartTime = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        /// <summary>
        /// 公告種類
        /// </summary>
    [Display(Name = "公告種類")]
        public string MESSAGE_TYPE { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        [Display(Name = "結束時間")]
        public string EndTime { get; set; }

        public string EndTime_AD
        {
            get
            {
                if (string.IsNullOrEmpty(EndTime))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(EndTime, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                EndTime = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

       

        [Display(Name = "頁數")]
        public int NowPage { get; set; }
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

        /// <summary>
        /// MESSAGE_TYPE	nvarchar(1)	公告種類 S:停機 P:繳費 I:重要
        /// </summary>
        [Display(Name = "公告種類")]
        public string MESSAGE_TYPE { get; set; }

    }
}