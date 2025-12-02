using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ES.Utils;
using System.Data.SqlClient;
using ES.Controllers;

namespace ES.Models
{
    public class QuestNonWebModel : BaseModel
    {
        //public Data.PERSONAL_EDUCATION PersonalEdu { get; set; }

        public string ValidateCode { get; set; }

        [Display(Name = "1. 案件名稱")]
        public string ServiceId { get; set; }

        [Display(Name = "2. 承辦機關")]
        public string UnitCode { get; set; }

        [Required(ErrorMessage = "請輸入申請案號")]
        [Display(Name = "3. 申請案號")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "申請案號長度錯誤")]
        [RegularExpression(@"^[\d]*$", ErrorMessage = "申請案號格式錯誤")]
        public string ApplyId { get; set; }

        [Required(ErrorMessage = "請選擇案件處理情形(1)滿意度")]
        [Display(Name = "問題一")]
        public int Q1 { get; set; }

        [Required(ErrorMessage = "請選擇案件處理情形(2)滿意度")]
        [Display(Name = "問題二")]
        public int Q2 { get; set; }

        [Required(ErrorMessage = "請選擇案件處理情形(3)滿意度")]
        [Display(Name = "問題三")]
        public int Q3 { get; set; }

        [Required(ErrorMessage = "請選擇案件處理情形(4)滿意度")]
        [Display(Name = "問題四")]
        public int Q4 { get; set; }

        [Required(ErrorMessage = "請選擇案件處理情形(5)滿意度")]
        [Display(Name = "問題五")]
        public int Q5 { get; set; }

        [Display(Name = "建議")]
        [StringLength(300, ErrorMessage = "最多輸入300個中文字")]
        public string Recommend { get; set; }

        [Display(Name = "整體滿意度")]
        public int Satisfied { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { get; set; }

        /// <summary>
        /// 承辦機關-種類
        /// </summary>
        [Display(Name = "承辦機關-種類-代碼")]
        public string LST_ID { get; set; }

        /// <summary>
        /// 承辦機關-種類
        /// </summary>
        [Display(Name = "承辦機關-種類")]
        public List<SelectListItem> UnitCodeSelList { set; get; }

        /// <summary>
        /// 案件名稱-種類
        /// </summary>
        [Display(Name = "案件名稱-種類")]
        public List<SelectListItem> ServiceIdSelList { get; set; }

    }
}