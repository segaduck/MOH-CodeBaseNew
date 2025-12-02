using System;
using System.Collections.Generic;
using EECOnline.Models;
using EECOnline.Models.Entities;

namespace EECOnline.Models
{
    /// <summary>
    /// 系統登入使用者類型
    /// </summary>
    public class DoctorCertType
    {
        /// <summary>
        /// 系統使用者
        /// </summary>
        public static DoctorCertType User = new DoctorCertType("User");

        #region LoginUserType 實作內容
        private string _type;

        private DoctorCertType() { }

        /// <summary>
        /// 以使用者類型字串建構
        /// </summary>
        /// <param name="type"></param>
        public DoctorCertType(string type)
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
    public class DoctorCert
    {
        public DoctorCert()
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
        public string idno { get; set; }

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
        /// 登入的使用者類型(預設為: USER)
        /// </summary>
        public DoctorCertType UserType { get; set; }

    }
}