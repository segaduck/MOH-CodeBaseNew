using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    public class DetialModel : BaseModel
    {
        [Display(Name = "申請日期")]
        public DateTime? ApplyDate { get; set; }
    }

    public class DetialMemberModel : DetialModel
    {
        [Display(Name = "帳號")]
        public virtual string Account { get; set; }

        [Display(Name = "身分證號 / 統一編號")]
        public virtual string Identity { get; set; }

        [Display(Name = "性別")]
        public virtual string SexCode { get; set; }

        [Display(Name = "性別")]
        public virtual string SexDesc { get; set; }

        [Display(Name = "生日")]
        public virtual DateTime? Birthday { get; set; }

        [Display(Name = "中文姓名")]
        public virtual string Name { get; set; }

        [Display(Name = "英文姓名")]
        public virtual string NameEng { get; set; }

        [Display(Name = "聯絡人中文姓名")]
        public virtual string ContactName { get; set; }

        [Display(Name = "聯絡人英文姓名")]
        public virtual string ContactNameEng { get; set; }

        [Display(Name = "負責人中文姓名")]
        public virtual string ChargeName { get; set; }

        [Display(Name = "負責人英文姓名")]
        public virtual string ChargeNameEng { get; set; }

        [Display(Name = "電話號碼")]
        public virtual string Tel { get; set; }

        [Display(Name = "傳真號碼")]
        public virtual string Fax { get; set; }

        [Display(Name = "聯絡人電話號碼")]
        public virtual string ContactTel { get; set; }

        [Display(Name = "電子郵件")]
        public virtual string Mail { get; set; }

        [Display(Name = "國別")]
        public virtual string Country { get; set; }

        [Display(Name = "縣市")]
        public virtual string CityCode { get; set; }

        [Display(Name = "鄉鎮市區")]
        public virtual string TownCode { get; set; }

        [Display(Name = "中文地址")]
        public virtual string Address { get; set; }

        [Display(Name = "英文地址")]
        public virtual string AddressEng { get; set; }

        [Display(Name = "藥商許可執照編號")]
        public virtual string Medico { get; set; }

        [Display(Name = "是否要收到最新消息郵件")]
        public virtual bool MailMark { get; set; }

        [Display(Name = "是否為個人")]
        public virtual bool IsPerson { get; set; }
    }

    public class DetialDefinitionModel
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 父欄位編號
        /// </summary>
        public int FiledParentId { get; set; }

        /// <summary>
        /// 欄位編號
        /// </summary>
        public int FiledId { get; set; }

        /// <summary>
        /// 欄位名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 欄位標題
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 欄位左邊說明
        /// </summary>
        public string DescriptionL { get; set; }

        /// <summary>
        /// 欄位右邊說明
        /// </summary>
        public string DescriptionR { get; set; }

        /// <summary>
        /// 欄位輸入類別
        /// </summary>
        public int TypeInput { get; set; }

        /// <summary>
        /// 資料來源類別
        /// </summary>
        public string Code { get; set; }
    }
}