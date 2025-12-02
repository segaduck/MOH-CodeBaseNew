using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    public class QuestWebModel : BaseModel
    {

        [Display(Name = "網路申請案號")]
        public string ApplyId { get; set; }

        [Display(Name = "承辦機關")]
        public int UnitCode { get; set; }

        [Display(Name = "案件編號")]
        public string ServiceId { get; set; }

        [Display(Name = "承辦機關名稱")]
        public string UnitName { get; set; }

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

        /// <summary>
        /// 整體滿意度
        /// </summary>
        [Display(Name = "整體滿意度")]
        public int Satisfied { get; set; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }

        [Display(Name = "是否填寫過調查表")]
        public bool ExistsMark { set; get; }

        /// <summary>
        /// ACC_NO	VARCHAR(30)	會員帳號
        /// </summary>
        [Display(Name = "會員帳號")]
        public string ACC_NO { set; get; }

    }
}