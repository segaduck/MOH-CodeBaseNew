using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    /// <summary>
    /// 基本模型
    /// </summary>
    public class BaseModel
    {
    }

    /// <summary>
    /// 帳號登入
    /// </summary>
    public class LoginModel
    {
        [Required(ErrorMessage="請輸入帳號")]
        [Display(Name = "帳號")]
        public string LoginAccount { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string LoginPassword { get; set; }
    }

    /// <summary>
    /// 憑證登入
    /// </summary>
    public class CertLoginModel
    {
        [Required(ErrorMessage = "請輸入Pin Code")]
        [Display(Name = "Pin Code")]
        public string PinCode { get; set; }

        public string CardType { get; set; }

        public string CardInfo { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string pSignedData { get; set; }
    }

    /// <summary>
    /// 連結元件
    /// </summary>
    public class LeftMenuItemModel
    {
        public int UnitCode { get; set; }
        public string UnitName { get; set; }
    }
}