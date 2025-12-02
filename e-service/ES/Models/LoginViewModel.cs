using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using Omu.ValueInjecter;
using ES.Models.Share;
using ES.Models.Entities;
using ES.Commons;

namespace ES.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            this.Form = new LoginFormModel();
        }

        /// <summary>
        /// 查詢條件 FromModel
        /// </summary>
        public LoginFormModel Form { get; set; }

        /// <summary>
        /// 新增會員
        /// </summary>
        public LoginDetailModel Detail { get; set; }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        public LoginForgetPWDModel forgetPWD { get; set; }

        /// <summary>
        /// 憑證類別 1.自然人憑證 2.工商憑證 3.醫事人員憑證  4.數位身分證
        /// </summary>
        public string Hide_loginType { get; set; }
        public string Hide_PinVerify { get; set; }
        public string Hide_enccert { get; set; }
        public string Hide_cadata { get; set; }
        public string Hide_sign { get; set; }
        public string Hid_RPCTYPE { get; set; }

        public bool lock_RPCTYPE { get; set; }
        /// <summary>
        /// 登入失敗的錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 登入失敗的錯誤訊息
        /// </summary>
        public string BlockMessage { get; set; }
    }

    /// <summary>
    /// 規章
    /// </summary>
    public class RegulationsModel : PaginationInfo
    {

    }

    /// <summary>
    /// 登入表單
    /// </summary>
    public class LoginFormModel : PaginationInfo
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [Required]
        public string UserNo { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Display(Name = "密碼")]
        [Required]
        public string UserPwd { get; set; }

        /// <summary>
        /// 驗證碼
        /// </summary>
        [Display(Name = "驗證碼")]
        [Required]
        public string ValidateCode { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class LoginDetailModel : TblMEMBER
    {
        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "帳號")]
        [Required]
        public string ACC_NO { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Display(Name = "密碼")]
        //[Required]
        public string PSWD { get; set; }

        /// <summary>
        /// 確認密碼
        /// </summary>
        [Display(Name = "確認密碼")]
        //[Required]
        public string PSWD_CHK { get; set; }

        /// <summary>
        /// 身分證字號
        /// </summary>
        [Display(Name = "身分證字號")]
        [Required]
        public string IDN { get; set; }

        /// <summary>
        /// 信箱
        /// </summary>
        [Display(Name = "信箱")]
        [Required]
        public string MAIL { get; set; }

        /// <summary>
        /// 中文姓名
        /// </summary>
        [Display(Name = "中文姓名")]
        [Required]
        public string NAME { get; set; }

        /// <summary>
        /// 英文姓名
        /// </summary>
        [Display(Name = "英文姓名")]
        public string ENAME { get; set; }

        /// <summary>
        /// 原住民姓名
        /// </summary>
        [Display(Name="原住民姓名")]
        public string ONAME { get; set; }

        /// <summary>
        /// 電話號碼
        /// </summary>
        [Display(Name = "電話號碼")]
        [Required]
        public string TEL { get; set; }

        /// <summary>
        /// 手機
        /// </summary>
        [Display(Name = "手機")]
        public string MOBILE { get; set; }

        #region 地址

        /// <summary>
        /// 郵遞區號
        /// </summary>
        [Display(Name = "郵遞區號")]
        [Required]
        public string ADDR_CODE { get; set; }
        public string ADDR_TEXT { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [Display(Name = "地址")]
        [Required]
        public string ADDR_DETAIL { get; set; }

        #endregion

        /// <summary>
        /// 性別
        /// </summary>
        [Display(Name = "性別")]
        public string SEX_CD { get; set; }

        /// <summary>
        /// 出生年月日
        /// </summary>
        [Display(Name = "出生年月日")]
        public string BIRTHDAY { get; set; }

        public string BIRTHDAY_AD
        {
            get
            {
                if (string.IsNullOrEmpty(BIRTHDAY))
                {
                    return null;
                }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(BIRTHDAY, ""));     // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                BIRTHDAY = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");         // YYYMMDD 民國年 使用者看到
            }
        }

        public bool Mailmark { get; set; }
        public bool NoKeyPxasWrod { get; set; }
        public bool IsUpdata { get; set; }
        public string Hide_loginType { get; set; }
        public string Hide_PinVerify { get; set; }
        public string Hide_enccert { get; set; }
        public string Hide_cadata { get; set; }
        public string Hide_sign { get; set; }
        public string Hid_RPCTYPE { get; set; }

    }

    /// <summary>
    /// 忘記密碼表單
    /// </summary>
    public class LoginForgetPWDModel
    {

    }

    public class QAViewModel
    {
        public IList<TblQA> grids { get; set; }
    }

    #region 舊系統

    ///// <summary>
    ///// 會員資料
    ///// </summary>
    //public class MemberModel : BaseModel
    //{
    //    [Display(Name = "帳號")]
    //    public virtual string Account { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "密碼")]
    //    public virtual string Password { get; set; }

    //    [Display(Name = "身分證號 / 統一編號")]
    //    public virtual string Identity { get; set; }

    //    [Display(Name = "性別")]
    //    public virtual string SexCode { get; set; }

    //    [Display(Name = "生日")]
    //    public virtual DateTime? Birthday { get; set; }

    //    [Display(Name = "中文姓名")]
    //    public virtual string Name { get; set; }

    //    [Display(Name = "英文姓名")]
    //    public virtual string NameEng { get; set; }

    //    [Display(Name = "聯絡人中文姓名")]
    //    public virtual string ContactName { get; set; }

    //    [Display(Name = "聯絡人英文姓名")]
    //    public virtual string ContactNameEng { get; set; }

    //    [Display(Name = "負責人中文姓名")]
    //    public virtual string ChargeName { get; set; }

    //    [Display(Name = "負責人英文姓名")]
    //    public virtual string ChargeNameEng { get; set; }

    //    [Display(Name = "電話號碼")]
    //    public virtual string Tel { get; set; }

    //    [Display(Name = "手機")]
    //    public virtual string Mobile { get; set; }

    //    [Display(Name = "傳真號碼")]
    //    public virtual string Fax { get; set; }

    //    [Display(Name = "聯絡人電話號碼")]
    //    public virtual string ContactTel { get; set; }

    //    [Display(Name = "電子郵件")]
    //    public virtual string Mail { get; set; }

    //    [Display(Name = "國別")]
    //    public virtual string Country { get; set; }

    //    [Display(Name = "縣市")]
    //    public virtual string CityCode { get; set; }

    //    [Display(Name = "鄉鎮市區")]
    //    public virtual string TownCode { get; set; }

    //    [Display(Name = "中文地址")]
    //    public virtual string Address { get; set; }

    //    [Display(Name = "英文地址")]
    //    public virtual string AddressEng { get; set; }

    //    [Display(Name = "藥商許可執照編號")]
    //    public virtual string Medico { get; set; }

    //    [Display(Name = "是否要收到最新消息郵件")]
    //    public virtual bool MailMark { get; set; }

    //    [Display(Name = "是否為個人")]
    //    public virtual bool IsPerson { get; set; }
    //}

    ///// <summary>
    ///// 會員資料 (個人)
    ///// </summary>
    //public class MemberPModel : MemberModel
    //{
    //    [Display(Name = "身分證號")]
    //    public override string Identity { get; set; }

    //    [Required(ErrorMessage = "請輸入電子郵件")]
    //    [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+[A-Za-z]{2,4}", ErrorMessage = "電子郵件格式錯誤")]
    //    [Display(Name = "電子郵件")]
    //    public override string Mail { get; set; }

    //    [Required(ErrorMessage = "請輸入中文姓名")]
    //    [Display(Name = "中文姓名")]
    //    public override string Name { get; set; }

    //    [Display(Name = "英文姓名")]
    //    public override string NameEng { get; set; }

    //    [Required(ErrorMessage = "請輸入電話號碼")]
    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "電話號碼格式錯誤")]
    //    [Display(Name = "電話號碼")]
    //    public override string Tel { get; set; }

    //    [Required(ErrorMessage = "請輸入手機")]
    //    [RegularExpression(@"([0][0-9]{3}[-]?[0-9]{6})", ErrorMessage = "手機格式錯誤")]
    //    [Display(Name = "手機")]
    //    public override string Mobile { get; set; }

    //    [Required(ErrorMessage = "請輸入通訊地址")]
    //    [Display(Name = "通訊地址")]
    //    public override string Address { get; set; }

    //    [Display(Name = "通訊地址縣市")]
    //    public override string CityCode { get; set; }

    //    [Display(Name = "通訊地址鄉鎮市區")]
    //    public override string TownCode { get; set; }

    //    [Required(ErrorMessage = "請選擇性別")]
    //    [Display(Name = "性別")]
    //    public override string SexCode { get; set; }

    //    [Remote("IsDate", "AJAX", ErrorMessage = "日期格式錯誤")]
    //    [DataType(DataType.Date, ErrorMessage = "日期格式錯誤")]
    //    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
    //    [Display(Name = "出生日期")]
    //    public override DateTime? Birthday { get; set; }

    //    [DataType(DataType.Password)]
    //    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼至少8個字元，最多20個字元")]
    //    //[RegularExpression(@"^[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [RegularExpression(@"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [Display(Name = "密碼")]
    //    public override string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼至少8個字元，最多20個字元")]
    //    //[RegularExpression(@"^[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [RegularExpression(@"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "密碼與確認密碼不同")]
    //    [Display(Name = "確認密碼")]
    //    public string ConfirmPassword { get; set; }
    //}

    ///// <summary>
    ///// 會員資料 (公司)
    ///// </summary>
    //public class MemberCModel : MemberModel
    //{
    //    [Display(Name = "帳號")]
    //    public override string Account { get; set; }

    //    [Display(Name = "電子郵件")]
    //    public override string Mail { get; set; }

    //    [Display(Name = "統一編號")]
    //    public override string Identity { get; set; }

    //    [Required(ErrorMessage = "請輸入公司（商號）中文名稱")]
    //    [Display(Name = "公司（商號）中文名稱")]
    //    public override string Name { get; set; }

    //    [Display(Name = "公司（商號）英文名稱")]
    //    public override string NameEng { get; set; }

    //    [Required(ErrorMessage = "請輸入公司（商號）電話")]
    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "電話格式錯誤")]
    //    [Display(Name = "公司（商號）電話")]
    //    public override string Tel { get; set; }

    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "傳真格式錯誤")]
    //    [Display(Name = "公司（商號）傳真")]
    //    public override string Fax { get; set; }

    //    [Required(ErrorMessage = "請輸入公司（商號）地址")]
    //    [Display(Name = "公司（商號）地址")]
    //    public override string Address { get; set; }

    //    [Display(Name = "公司（商號）地址縣市")]
    //    public override string CityCode { get; set; }

    //    [Display(Name = "公司（商號）地址鄉鎮市區")]
    //    public override string TownCode { get; set; }

    //    [Display(Name = "公司（商號）英文地址")]
    //    public override string AddressEng { get; set; }

    //    [Display(Name = "藥商許可執照編號")]
    //    public override string Medico { get; set; }

    //    [Display(Name = "負責人中文姓名")]
    //    public override string ChargeName { get; set; }

    //    [Display(Name = "負責人英文姓名")]
    //    public override string ChargeNameEng { get; set; }

    //    [Required(ErrorMessage = "請輸入聯絡人（承辦人）中文姓名")]
    //    [Display(Name = "聯絡人（承辦人）中文姓名")]
    //    public override string ContactName { get; set; }

    //    [Display(Name = "聯絡人（承辦人）英文姓名")]
    //    public override string ContactNameEng { get; set; }

    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "電話格式錯誤")]
    //    [Display(Name = "聯絡人（承辦人）電話")]
    //    public override string ContactTel { get; set; }

    //    [DataType(DataType.Password)]
    //    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼至少8個字元，最多20個字元")]
    //    //[RegularExpression(@"^[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [RegularExpression(@"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [Display(Name = "密碼")]
    //    public override string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼至少8個字元，最多20個字元")]
    //    //[RegularExpression(@"^[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [RegularExpression(@"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "密碼與確認密碼不同")]
    //    [Display(Name = "確認密碼")]
    //    public string ConfirmPassword { get; set; }
    //}

    ///// <summary>
    ///// 加入會員 (步驟二)
    ///// </summary>
    //public class MemberRModel : MemberModel
    //{
    //    [Remote("CheckAccountExists", "AJAX", ErrorMessage = "帳號已存在")]
    //    [Required(ErrorMessage = "請輸入帳號")]
    //    [StringLength(20, MinimumLength = 6, ErrorMessage = "帳號至少6個字元，最多20個字元")]
    //    [RegularExpression(@"^(?!.*[A-Za-z]+[0-9]{9})[A-Za-z]+[A-Za-z0-9_]*$", ErrorMessage = "帳號格式錯誤/帳號不得為身分證")]
    //    [Display(Name = "帳號")]
    //    public override string Account { get; set; }

    //    [Required(ErrorMessage = "請輸入密碼")]
    //    [DataType(DataType.Password)]
    //    [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼至少8個字元，最多20個字元")]
    //    //[RegularExpression(@"^[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [RegularExpression(@"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$", ErrorMessage = "密碼格式錯誤")]
    //    [Display(Name = "密碼")]
    //    public override string Password { get; set; }

    //    [Remote("CheckIdentityExists", "AJAX", ErrorMessage = "身分證號已存在")]
    //    [Required(ErrorMessage = "請輸入身分證號 / 統一編號")]
    //    [Display(Name = "身分證號 / 統一編號")]
    //    public override string Identity { get; set; }

    //    [Required(ErrorMessage = "請輸入電子郵件")]
    //    [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+[A-Za-z]{2,4}", ErrorMessage = "電子郵件格式錯誤")]
    //    [Display(Name = "電子郵件")]
    //    public override string Mail { get; set; }
    //}

    ///// <summary>
    ///// 加入會員 (步驟三 - 個人)
    ///// </summary>
    //public class MemberRPModel : MemberModel
    //{

    //    public MemberRPModel() { }

    //    public MemberRPModel(MemberRModel model)
    //    {
    //        this.Account = model.Account;
    //        this.Password = model.Password;
    //        this.Identity = model.Identity;
    //        this.Mail = model.Mail;
    //    }

    //    [Display(Name = "帳號")]
    //    public override string Account { get; set; }

    //    [Display(Name = "密碼")]
    //    public override string Password { get; set; }

    //    [Display(Name = "電子郵件")]
    //    public override string Mail { get; set; }

    //    [Display(Name = "身分證號")]
    //    public override string Identity { get; set; }

    //    [Required(ErrorMessage = "請輸入中文姓名")]
    //    [Display(Name = "中文姓名")]
    //    public override string Name { get; set; }

    //    [Display(Name = "英文姓名")]
    //    public override string NameEng { get; set; }

    //    [Required(ErrorMessage = "請輸入電話號碼")]
    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "電話號碼格式錯誤")]
    //    [Display(Name = "電話號碼")]
    //    public override string Tel { get; set; }

    //    [Required(ErrorMessage = "請輸入通訊地址")]
    //    [Display(Name = "通訊地址")]
    //    public override string Address { get; set; }

    //    [Display(Name = "通訊地址縣市")]
    //    public override string CityCode { get; set; }

    //    [Display(Name = "通訊地址鄉鎮市區")]
    //    public override string TownCode { get; set; }

    //    [Required(ErrorMessage = "請選擇性別")]
    //    [Display(Name = "性別")]
    //    public override string SexCode { get; set; }

    //    [DataType(DataType.Date, ErrorMessage = "日期格式錯誤")]
    //    [Display(Name = "出生日期")]
    //    public override DateTime? Birthday { get; set; }
    //}

    ///// <summary>
    ///// 加入會員 (步驟三 - 公司)
    ///// </summary>
    //public class MemberRCModel : MemberModel
    //{

    //    public MemberRCModel() { }

    //    public MemberRCModel(MemberRModel model)
    //    {
    //        this.Account = model.Account;
    //        this.Password = model.Password;
    //        this.Identity = model.Identity;
    //        this.Mail = model.Mail;
    //    }

    //    [Display(Name = "帳號")]
    //    public override string Account { get; set; }

    //    [Display(Name = "密碼")]
    //    public override string Password { get; set; }

    //    [Display(Name = "電子郵件")]
    //    public override string Mail { get; set; }

    //    [Display(Name = "統一編號")]
    //    public override string Identity { get; set; }

    //    [Required(ErrorMessage = "請輸入公司（商號）中文名稱")]
    //    [Display(Name = "公司（商號）中文名稱")]
    //    public override string Name { get; set; }

    //    [Display(Name = "公司（商號）英文名稱")]
    //    public override string NameEng { get; set; }

    //    [Required(ErrorMessage = "請輸入公司（商號）電話")]
    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "電話格式錯誤")]
    //    [Display(Name = "公司（商號）電話")]
    //    public override string Tel { get; set; }

    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "傳真格式錯誤")]
    //    [Display(Name = "公司（商號）傳真")]
    //    public override string Fax { get; set; }

    //    [Required(ErrorMessage = "請輸入公司（商號）地址")]
    //    [Display(Name = "公司（商號）地址")]
    //    public override string Address { get; set; }

    //    [Display(Name = "公司（商號）地址縣市")]
    //    public override string CityCode { get; set; }

    //    [Display(Name = "公司（商號）地址鄉鎮市區")]
    //    public override string TownCode { get; set; }

    //    [Display(Name = "公司（商號）英文地址")]
    //    public override string AddressEng { get; set; }

    //    [Display(Name = "藥商許可執照編號")]
    //    public override string Medico { get; set; }

    //    [Display(Name = "負責人中文姓名")]
    //    public override string ChargeName { get; set; }

    //    [Display(Name = "負責人英文姓名")]
    //    public override string ChargeNameEng { get; set; }

    //    [Required(ErrorMessage = "請輸入聯絡人（承辦人）中文姓名")]
    //    [Display(Name = "聯絡人（承辦人）中文姓名")]
    //    public override string ContactName { get; set; }

    //    [Display(Name = "聯絡人（承辦人）英文姓名")]
    //    public override string ContactNameEng { get; set; }

    //    [RegularExpression(@"([0][0-9]{1,3}[-])?[0-9]{5,8}([#][0-9]{1,5})?", ErrorMessage = "電話格式錯誤")]
    //    [Display(Name = "聯絡人（承辦人）電話")]
    //    public override string ContactTel { get; set; }
    //}

    ///// <summary>
    ///// 忘記密碼
    ///// </summary>
    //public class MemberForgetModel
    //{
    //    [Required(ErrorMessage = "請填寫帳號")]
    //    [Display(Name = "帳號")]
    //    public string Account { get; set; }

    //    [Required(ErrorMessage = "身分證字號(統一編號)")]
    //    [Display(Name = "身分證字號(統一編號)")]
    //    public string Identity { get; set; }

    //    [Required(ErrorMessage = "請填寫電子郵件")]
    //    [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+[A-Za-z]{2,4}", ErrorMessage = "電子郵件格式錯誤")]
    //    [Display(Name = "電子郵件")]
    //    public string Mail { get; set; }
    //}

    #endregion
}