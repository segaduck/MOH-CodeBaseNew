using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EECOnline.Commons;
using EECOnline.Models.Entities;
using Turbo.Commons;
using Turbo.DataLayer;
using EECOnline.Models;

namespace EECOnline.Areas.A5.Models
{
    public class C102MViewModel
    {
        public C102MViewModel()
        {
            this.form = new C102MFormModel();
        }

        public C102MFormModel form { get; set; }
    }

    /// <summary>
    /// 變更密碼
    /// </summary>
    public class C102MChangeModel : TblAMDBURM
    {
        public C102MChangeModel()
        {
        }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 忘記密碼驗證碼
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        public string userno { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        public string pwd { get; set; }

        /// <summary>
        /// 重複密碼
        /// </summary>
        public string pwd_REPEAT { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PasswordCheckModel PWDCheck { get; set; }
    }

    /// <summary>
    /// 忘記密碼
    /// </summary>
    public class C102MFormModel : PagingResultsViewModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [Required]
        [Control(Mode = Control.TextBox, size = "36", maxlength = "16", placeholder = "請輸入帳號")]
        public string userno { get; set; }

        /// <summary>
        /// 身分證
        /// </summary>
        [Display(Name = "身分證後四碼")]
        [Required]
        [Control(Mode = Control.TextBox, size = "36", maxlength = "4", placeholder = "請輸入身分證後四碼")]
        public string idno { get; set; }

        /// <summary>
        /// 信箱
        /// </summary>
        [Display(Name = "信箱")]
        [Required]
        [Control(Mode = Control.TextBox, size = "36", placeholder = "請輸入信箱")]
        public string email { get; set; }

        
    }
}