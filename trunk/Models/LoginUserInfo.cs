using System;
using System.Collections.Generic;
using EECOnline.Models;
using EECOnline.Models.Entities;

namespace EECOnline.Models
{
    /// <summary>
    /// 系統登入使用者類型
    /// </summary>
    public class LoginUserType
    {
        /// <summary>
        /// 技檢系統使用者(來自CLAMDBURM)
        /// </summary>
        public static LoginUserType SKILL_USER = new LoginUserType("SKILL_USER");

        #region LoginUserType 實作內容
        private string _type;

        private LoginUserType() { }

        /// <summary>
        /// 以使用者類型字串建構
        /// </summary>
        /// <param name="type"></param>
        public LoginUserType(string type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type can't be null");
            }
            this._type = type;
        }

        /// <summary>
        /// 取得使用者類型字串
        /// </summary>
        public string Type
        {
            get { return this._type; }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is LoginUserType)
            {
                return this.Equals((LoginUserType)obj);
            }
            else
            {
                throw new ArgumentException("obj must be LoginUserType");
            }
        }

        /// <summary>
        /// 傳入的 LoginUserType 是否為相同的使用者類型
        /// </summary>
        /// <param name="userType"></param>
        /// <returns></returns>
        public bool Equals(LoginUserType userType)
        {
            return (userType != null) && this._type.Equals(userType.Type);
        }

        public bool Equals(string type)
        {
            return this._type.Equals(type);
        }

        public override string ToString()
        {
            return this.Type;
        }
        #endregion
    }

    /// <summary>
    /// 系統登入使用者資訊
    /// </summary>
    public class LoginUserInfo
    {
        public LoginUserInfo()
        {
            LoginSuccess = false;
        }

        /// <summary>
        /// 登入成功與否
        /// </summary>
        public bool LoginSuccess { get; set; }

        /// <summary>
        /// 登入失敗時的錯誤訊息
        /// </summary>
        public string LoginErrMessage { get; set; }


        /// <summary>
        /// 登人時輸入的帳號
        /// </summary>
        public string UserNo { get; set; }

        /// <summary>
        /// 醫院授權碼	
        /// </summary>
        //public string Code { get; set; }

        /// <summary>
        /// 登入方式 <br />
        /// 1: 帳號密碼登入 <br />
        /// 2: 醫院授權碼登入 <br />
        /// </summary>
        public string LoginTab { get; set; }

        /// <summary>醫院代號 (僅供醫院授權碼登入時使用)</summary>
        public string HospitalCode { get; set; }

        /// <summary>
        /// 是否須強制變更密碼
        /// </summary>
        public bool ChangePwdRequired { get; set; }

        /// <summary>
        /// 是否須強制變更密碼
        /// </summary>
        public bool ChangeDetailRequired { get; set; }


        /// <summary>
        /// 使用者登入區域: 1.內綱,  2.外網
        /// </summary>
        public string NetID { get; set; }

        /// <summary>
        /// 登入驗證方式: 1.一般帳密登入, 2.憑證登入
        /// </summary>
        public string LoginAuth { get; set; }

        /// <summary>
        /// 登入來源IP
        /// </summary>
        public string LoginIP { get; set; }

        /// <summary>
        /// 登入成功與否
        /// </summary>
        public DateTime? PWDMODTIME { get; set; }

        /// <summary>
        /// 登入的使用者類型(預設為: SKILL_USER)
        /// </summary>
        public LoginUserType UserType { get; set; }

        /// <summary>
        /// 使用者帳號資料
        /// </summary>
        public ClamUser User { get; set; }

        /// <summary>
        /// 使用者群組清單
        /// </summary>
        public IList<ClamUserGroup> Groups { get; set; }
    }
}