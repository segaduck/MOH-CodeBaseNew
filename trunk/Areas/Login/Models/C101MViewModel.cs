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

namespace EECOnline.Areas.Login.Models
{
    public class C101MViewModel
    {
        public C101MViewModel()
        {
            this.form = new C101MFormModel();
        }

        public C101MFormModel form { get; set; }

    }

    /// <summary>
    /// 登入表單 Model
    /// </summary>
    public class C101MFormModel
    {
        /// <summary>
        /// 當前是哪一種登入 1: 帳號密碼登入; 2: 醫院授權碼登入;
        /// </summary>
        public string ThePage { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [Required]
        public string UserNo { get; set; }

        /// <summary>
        /// 帳號 的密碼
        /// </summary>
        [Display(Name = "密碼")]
        [Required]
        public string UserPwd { get; set; }

        /// <summary>
        /// 醫院授權碼
        /// </summary>
        [Display(Name = "醫院授權碼")]
        [Required]
        public string AuthCode { get; set; }

        /// <summary>
        /// 醫院授權碼 的密碼
        /// </summary>
        public string AuthCode_Pwd { get; set; }

        /// <summary>
        /// 驗證碼
        /// </summary>
        [Display(Name = "驗證碼")]
        [Required]
        public string ValidateCode { get; set; }

        /// <summary>
        /// 登入失敗的錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 忘記密碼
    /// </summary>
    public class C101MDetailModel
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [Required]
        public string UserNo { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Display(Name = "身分證字號")]
        [Required]
        public string idno { get; set; }

        /// <summary>
        /// 信箱
        /// </summary>
        [Display(Name = "信箱")]
        [Required]
        public string email { get; set; }

        /// <summary>
        /// 驗證碼
        /// </summary>
        [Display(Name = "驗證碼")]
        [Required]
        public string ValidateCode { get; set; }
    }

    /// <summary>
    /// 變更密碼
    /// </summary>
    public class C101MChangeModel : TblAMDBURM
    {
        public C101MChangeModel()
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

    public class C101MHospPwdChangeModel : TblEEC_Hospital
    {
        public C101MHospPwdChangeModel() { }

        public string ErrorMessage { get; set; }

        public string guid { get; set; }

        /// <summary>醫院代號</summary>
        public string code { get; set; }

        /// <summary>醫院名稱</summary>
        public string text { get; set; }

        public string PWD { get; set; }

        public string PWD_REPEAT { get; set; }

        public PasswordCheckModel PWDCheck { get; set; }
    }

    /// <summary>
    /// 忘記密碼
    /// </summary>
    public class C101MForgetModel : TblAMDBURM
    {
        public C101MForgetModel()
        {
        }

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