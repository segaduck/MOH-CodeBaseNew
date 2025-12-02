using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;
using ES.Models;
using ES.Models.Entities;

namespace ES.Models
{
    /// <summary>
    /// 系統登入使用者類型
    /// </summary>
    public class LoginUserType
    {
        /// <summary>
        /// 
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
        /// 登入驗證方式: 
        /// MEMBER.會員
        /// MOICA 自然人憑證
        /// MOEACA 工商
        /// HCA0 醫事憑證
        /// HCA1 醫事憑證
        /// </summary>
        public string LoginAuth { get; set; }

        /// <summary>
        /// 登入來源IP
        /// </summary>
        public string LoginIP { get; set; }

        /// <summary>
        /// 登入的使用者類型
        /// </summary>
        public LoginUserType UserType { get; set; }

        /// <summary>
        /// 前台使用者
        /// </summary>
        public ClamMember Member { get; set; }

        /// <summary>
        /// 後台使用者
        /// </summary>
        public ClamAdmin Admin { get; set; }
    }

    public class CaseQryModel
    {
        public CaseQryModel()
        {
        }
         
        public  int NowPage { get; set; }

        //[Display(Name = "申辦編號")]
        public  String APP_ID_BEGIN { get; set; }

        //[Display(Name = "申辦編號")]
        public String APP_ID_END { get; set; }

        public DateTime? APP_TIME_BEGIN { get; set; }
        //[Display(Name = "申辦日期")]
        public string APP_TIME_BEGIN_AD { get; set; }

        public DateTime? APP_TIME_END { get; set; }
        //[Display(Name = "申辦日期")]
        public string APP_TIME_END_AD { get; set; }

        //[Display(Name = "公文文號")]
        public string MOHW_CASE_NO { get; set; }

        //[Display(Name = "處理進度")]
        public string FLOW_CD { get; set; }

        //[Display(Name = "身份字號/統編")]
        public string IDN { get; set; }

        //[Display(Name = "排序欄位")]
        public string OderByCol { get; set; }

        //[Display(Name = "排序方法")]
        public string SortAZ { get; set; }

        //[Display(Name = "結案狀態")]
        public string CLOSE_MK { get; set; }

        //[Display(Name = "許可證字號(字)")]
        public string LIC_CD { get; set; }

        //[Display(Name = "許可證字號(號)")]
        public string LIC_NUM { get; set; }

        //[Display(Name = "藥廠編號")]
        public string MF_CD { get; set; }

        //[Display(Name = "製造廠名稱")]
        public string MF_NAME { get; set; }

        //[Display(Name = "品名(中英文)")]
        public string DRUG_NAME { get; set; }

        //[Display(Name = "成分")]
        public string INGR_NAME { get; set; }

        //[Display(Name = "效能")]
        public string EFFICACY { get; set; }

        //[Display(Name = "適應症")]
        public string INDIOCATION { get; set; }

        //[Display(Name = "劑型")]
        public string DOSAGE_FORM { get; set; }

        //[Display(Name = "承辦人")]
        public string PRO_ACC { get; set; }


        //[Display(Name = "申請項目")]
        public string Apply_Item { get; set; }

        public string IS_HOME_PAGE { get; set; }

        public string UNIT_CD { get; set; }

        public string FLOW_CD_ITEM { get; set; }

        //[Display(Name = "申請人姓名")]
        public virtual string AP_NAME { get; set; }
    }
}