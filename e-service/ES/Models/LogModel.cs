using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models
{
    public class LeadModel
    {
        public class DocumentFormat
        {
            [Display(Name = "發函單位：")]
            public string Title { get; set; }
            
            [Display(Name = "地　　址：")]
            [Required(ErrorMessage = "請輸入地址")]
            public string Address { get; set; }

            [Display(Name = "承辦人　：")]
            [Required(ErrorMessage = "請輸入承辦人")]
            public string Name { get; set; }

            [Display(Name = "電　　話：")]
            [Required(ErrorMessage = "請輸入電話")]
            public string Tel { get; set; }

            [Display(Name = "傳　　真：")]
            [Required(ErrorMessage = "請輸入傳真")]
            public string Fax { get; set; }

            [Display(Name = "電子郵件：")]
            [Required(ErrorMessage = "請輸入電子郵件")]
            public string EMail { get; set; }

                      
            [Display(Name = "中文品名：")]
            [Required(ErrorMessage = "請輸入中文品名")]
            public string DrugName { get; set; }

            [Display(Name = "許可證字號：")]
            [Required(ErrorMessage = "請輸入許可證字號")]
            public string LicNum { get; set; }

            [Display(Name = "主　　旨：")]
            public string SubjectText { get; set; }

            [Display(Name = "說明一")]
            public string Caption1 { get; set; }

            [Display(Name = "說明二")]
            public string Caption2 { get; set; }

            [Display(Name = "說明三")]
            public string Caption3 { get; set; }

            [Display(Name = "說明四")]
            public string Caption4 { get; set; }

            [Display(Name = "說明五")]
            public string Caption5 { get; set; }

            [Display(Name = "說明六")]
            public string Caption6 { get; set; }

            [Display(Name = "說明七")]
            public string Caption7 { get; set; }

            [Display(Name = "說明八")]
            public string Caption8 { get; set; } 

            [Display(Name = "申請書份數")]
            public string AppCnt { get; set; }

            [Display(Name = "總金額")]
            [Required(ErrorMessage = "請輸入總金額")]
            public string TotalAmount { get; set; }

            [Display(Name = "付款方式")]
            public string PayMethod { get; set; }

            [Display(Name = "申請基本費用")]
            public int APP_FEE { get; set; }

            [Display(Name = "表單類型")]
            public string SRV_NO { get; set; }

            public string APP_ID { get; set; }
            public string NO { get; set; }
        }
    }
}