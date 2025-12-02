using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.DataLayers;
using Turbo.Commons;
using log4net;
using RSACSP;
using System.Text;
using System.Text.RegularExpressions;

namespace EECOnline.Services
{
    public class ClamService
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 傳入使用者帳號及輸入的密碼明文, 加密後回傳
        /// </summary>
        /// <param name="userNo"></param>
        /// <param name="userPwd"></param>
        /// <returns></returns>
        public string EncPassword(string userNo, string userPwd)
        {
            string plain = userPwd;
            if (!string.IsNullOrWhiteSpace(userNo))
            {
                plain = userNo + plain;
            }
            return this.EncPassword(plain);
        }

        /// <summary>
        /// 傳入使用者輸入的密碼明文, 加密後回傳
        /// </summary>
        /// <param name="usePwd"></param>
        /// <returns></returns>
        private string EncPassword(string userPwd)
        {
            if (string.IsNullOrWhiteSpace(userPwd))
            {
                throw new ArgumentNullException("userPwd");
            }
            //TODO: 置換 RSACSP 改成不可逆的 Hash 方法
            RSACSP.RSACSP rsa = new RSACSP.RSACSP();
            return rsa.Utl_Encrypt(userPwd);
        }

        /// <summary>
        /// 回傳指定帳號的預設密碼加密字串
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        private string GetDefaultPasswordEnc(string userNo)
        {
            if (!string.IsNullOrWhiteSpace(userNo))
            {
                AMDAO dao = new AMDAO();
                TblAMDBURM amdwhere = new TblAMDBURM();
                amdwhere.userno = userNo;
                var data = dao.GetRowList(amdwhere);
                if (data.Count() > 0)
                {
                    return data[0].pwd;
                }
                else
                {
                    return "";
                }

            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 使用者登入帳密檢核, 檢核結果以 LoginUserInfo 返回, 
        /// 應檢查 LoginUserInfo.LoginSuccess 判斷登入是否成功
        /// </summary>
        /// <param name="userNo">使用者ID</param>
        /// <param name="userPwd">使用者登入密碼(明碼)</param>
        /// <returns></returns>
        public LoginUserInfo LoginValidate(string userNo, string userPwd, string IP)
        {
            LoginUserInfo userInfo = new LoginUserInfo();
            userInfo.UserNo = userNo;
            LoginDAO dao = new LoginDAO();

            // 取得使用者帳號資料
            ClamUser dburm = dao.GetUser(userNo);
            if (dburm == null)
            {
                userInfo.LoginErrMessage = "帳號不存在，請檢查 !!";
            }
            else if (dburm.UNIT_NAME.TONotNullString() == "" && !"2".Equals(dburm.authstatus))
            {
                userInfo.LoginErrMessage = "尚未設定使用權限，請先聯絡管理員設定完成再使用 ! ";
            }
            else
            {
                bool accValid = false;

                #region Step.1 帳號有效性檢查

                //DateTime? authDateS = HelperUtil.TransToDateTime(dburm.authdates, "");
                //DateTime? authDateE = HelperUtil.TransToDateTime(dburm.authdatee, "");

                if ("0".Equals(dburm.authstatus))
                {
                    userInfo.LoginErrMessage = "您目前帳號無效，尚無法使用!!";
                    // 登入紀錄
                    TblLOGINLOG llog = new TblLOGINLOG();
                    llog.moduser = userNo;
                    llog.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    llog.status = "0";
                    llog.modusername = "";
                    llog.modip = IP;
                    dao.Insert(llog);
                }
                //else if (authDateE != null
                //    && authDateE.Value.Date.CompareTo(DateTime.Now.Date) < 0)
                //{
                //    userInfo.LoginErrMessage = "您的帳號有效日期已過，無法繼續使用本系統!!";
                //}
                else if ("1".Equals(dburm.authstatus))
                {
                    // 帳號有效
                    accValid = true;
                }
                else if ("2".Equals(dburm.authstatus))
                {
                    // 帳號有效
                    accValid = true;
                }
                else
                {
                    userInfo.LoginErrMessage = "您目前帳號無效，尚無法使用!!";
                }

                #endregion

                if (accValid)
                {
                    //需要變更密碼
                    if ("2".Equals(dburm.authstatus))
                    {
                        userInfo.LoginSuccess = false;
                        //userInfo.ChangePwdRequired = true;
                        userInfo.LoginErrMessage = "您的帳號為初次使用，需先至信箱開啟驗證信啟用！";
                    }
                    else
                    {
                        #region Step.2 密碼檢核

                        // 產生密碼加密字串
                        string encPass = CypherText(userPwd);

                        //依帳號查詢 並帶出加密後的密碼
                        string encDefsultPass = this.GetDefaultPasswordEnc(userNo);

                        long errct = dburm.errct != null ? dburm.errct.Value : 0;
                        if (!encPass.Equals(encDefsultPass))
                        {
                            // 密碼錯誤
                            userInfo.LoginErrMessage = "密碼錯誤";
                            errct++;

                            // 記錄密碼錯誤次數
                            dburm.errct = errct.TOInt32();
                            dao.UpdateUserErrCount(dburm);
                            // 2020.06.29 登入紀錄
                            TblLOGINLOG llog = new TblLOGINLOG();
                            llog.moduser = userNo;
                            llog.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            llog.status = "0";
                            llog.modusername = dburm.username;
                            llog.modip = IP;
                            dao.Insert(llog);

                        }
                        else
                        {
                            //需要變更密碼
                            if ("2".Equals(dburm.authstatus))
                            {
                                userInfo.LoginSuccess = false;
                                //userInfo.ChangePwdRequired = true;
                                userInfo.LoginErrMessage = "您的帳號為初次使用，需先至信箱開啟驗證信啟用！";
                            }
                            else
                            {
                                // 密碼檢核通過
                                userInfo.LoginSuccess = true;
                            }

                        }
                        #endregion

                        #region Step.3 明細資料未填寫檢核

                        //if (dburm.birthday.TONotNullString() == "" || dburm.email.TONotNullString() == "")
                        //{
                        //    userInfo.ChangeDetailRequired = true;
                        //    userInfo.LoginErrMessage = "您目前必填明細資料尚未全部填寫完畢，為了安全起見請先變更明細資料再用本系統！";
                        //}

                        #endregion
                    }
                }
            }

            if (userInfo.LoginSuccess)
            {
                userInfo.User = dburm;

                userInfo.LoginAuth = "1";

                // 登入紀錄
                TblLOGINLOG where = new TblLOGINLOG();
                where.moduser = userNo;
                where.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
                where.status = "1";
                where.modusername = dburm.username;
                where.modip = IP;
                dao.Insert(where);

                if (!"2".Equals(dburm.authstatus))
                {
                    // 抓取使用者群組清單
                    userInfo.Groups = dao.GetUserGroup(userNo);
                    if (userInfo.Groups.Count == 0)
                    {
                        userInfo.LoginSuccess = false;
                        userInfo.LoginErrMessage = "您的權限群組尚未設定，請聯絡系統管理員!!";
                    }

                    // 還原密碼錯誤次數
                    if (userInfo.LoginSuccess)
                    {
                        dburm.errct = 0;
                        dburm.authstatus = "2".Equals(dburm.authstatus) ? "2" : "1";
                        dao.UpdateUserErrCount(dburm);
                    }
                }

            }
            return userInfo;
        }

        public LoginUserInfo LoginValidate_Hosp(string userNo, string userPwd, string IP)
        {
            LoginUserInfo userInfo = new LoginUserInfo();
            userInfo.UserNo = userNo;
            LoginDAO dao = new LoginDAO();

            // 取得使用者帳號資料
            ClamUser dburm = dao.GetUser_Hosp(userNo);
            if (dburm == null)
            {
                userInfo.LoginErrMessage = "醫院授權碼不存在，請檢查 !!";
            }
            else if (dburm.UNIT_NAME.TONotNullString() == "" && !"2".Equals(dburm.authstatus))
            {
                userInfo.LoginErrMessage = "尚未設定使用權限，請先聯絡管理員設定完成再使用 ! ";
            }
            else
            {
                bool accValid = false;

                #region Step.1 醫院授權碼有效性檢查

                //DateTime? authDateS = HelperUtil.TransToDateTime(dburm.authdates, "");
                //DateTime? authDateE = HelperUtil.TransToDateTime(dburm.authdatee, "");

                if ("0".Equals(dburm.authstatus))
                {
                    userInfo.LoginErrMessage = "您目前醫院授權碼無效，尚無法使用!!";
                    // 登入紀錄
                    TblLOGINLOG llog = new TblLOGINLOG();
                    llog.moduser = userNo + "(HAC)";  // EEC_Hospital.AuthCode
                    llog.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
                    llog.status = "0";
                    llog.modusername = "";
                    llog.modip = IP;
                    dao.Insert(llog);
                }
                //else if (authDateE != null
                //    && authDateE.Value.Date.CompareTo(DateTime.Now.Date) < 0)
                //{
                //    userInfo.LoginErrMessage = "您的醫院授權碼有效日期已過，無法繼續使用本系統!!";
                //}
                else if ("1".Equals(dburm.authstatus))
                {
                    // 醫院授權碼有效
                    accValid = true;
                }
                else if ("2".Equals(dburm.authstatus))
                {
                    // 醫院授權碼有效
                    accValid = true;
                }
                else
                {
                    userInfo.LoginErrMessage = "您目前醫院授權碼無效，尚無法使用!!";
                }

                #endregion

                if (accValid)
                {
                    //需要變更密碼
                    if ("2".Equals(dburm.authstatus))
                    {
                        userInfo.LoginSuccess = false;
                        //userInfo.ChangePwdRequired = true;
                        userInfo.LoginErrMessage = "您的帳號為初次使用，需先至信箱開啟驗證信啟用！";
                    }
                    else
                    {
                        #region Step.2 密碼檢核

                        // 產生密碼加密字串
                        string encPass = CypherText(userPwd);

                        long errct = dburm.errct != null ? dburm.errct.Value : 0;
                        if (!encPass.Equals(dburm.pwd))
                        {
                            // 密碼錯誤
                            userInfo.LoginErrMessage = "密碼錯誤";
                            errct++;

                            // 記錄密碼錯誤次數
                            dburm.errct = errct.TOInt32();
                            dao.UpdateUserErrCount_Hosp(dburm);
                            // 2020.06.29 登入紀錄
                            TblLOGINLOG llog = new TblLOGINLOG();
                            llog.moduser = userNo + "(HAC)";  // EEC_Hospital.AuthCode
                            llog.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
                            llog.status = "0";
                            llog.modusername = dburm.username;
                            llog.modip = IP;
                            dao.Insert(llog);
                        }
                        else
                        {
                            //需要變更密碼
                            if ("2".Equals(dburm.authstatus))
                            {
                                userInfo.LoginSuccess = false;
                                //userInfo.ChangePwdRequired = true;
                                userInfo.LoginErrMessage = "您的帳號為初次使用，需先至信箱開啟驗證信啟用！";
                            }
                            else
                            {
                                // 密碼檢核通過
                                userInfo.LoginSuccess = true;
                            }
                        }
                        #endregion

                        #region Step.3 明細資料未填寫檢核

                        //if (dburm.birthday.TONotNullString() == "" || dburm.email.TONotNullString() == "")
                        //{
                        //    userInfo.ChangeDetailRequired = true;
                        //    userInfo.LoginErrMessage = "您目前必填明細資料尚未全部填寫完畢，為了安全起見請先變更明細資料再用本系統！";
                        //}

                        #endregion
                    }
                }
            }

            if (userInfo.LoginSuccess)
            {
                userInfo.User = dburm;

                userInfo.LoginAuth = "1";

                // 登入紀錄
                TblLOGINLOG where = new TblLOGINLOG();
                where.moduser = userNo + "(HAC)";  // EEC_Hospital.AuthCode
                where.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
                where.status = "1";
                where.modusername = dburm.username;
                where.modip = IP;
                dao.Insert(where);

                if (!"2".Equals(dburm.authstatus))
                {
                    // 抓取使用者群組清單
                    userInfo.Groups = dao.GetUserGroup_Hosp(userNo);
                    if (userInfo.Groups.Count == 0)
                    {
                        userInfo.LoginSuccess = false;
                        userInfo.LoginErrMessage = "您的權限群組尚未設定，請聯絡系統管理員!!";
                    }

                    // 還原密碼錯誤次數
                    if (userInfo.LoginSuccess)
                    {
                        dburm.errct = 0;
                        dburm.authstatus = "2".Equals(dburm.authstatus) ? "2" : "1";
                        dao.UpdateUserErrCount_Hosp(dburm);
                    }
                }
            }
            return userInfo;
        }

        /// <summary>
        /// 取得群組權限功能清單
        /// </summary>
        /// <param name="role"></param>
        /// <param name="userNo"></param>
        /// <param name="netId"></param>
        /// <returns></returns>
        public IList<ClamRoleFunc> GetUserRoleFuncs(LoginUserInfo user)
        {
            LoginDAO dao = new LoginDAO();
            return dao.GetRoleFuncs(user.UserNo);
        }

        public IList<ClamRoleFunc> GetUserRoleFuncs_Hosp(LoginUserInfo user)
        {
            LoginDAO dao = new LoginDAO();
            return dao.GetRoleFuncs_Hosp(user.UserNo);
        }

        /// <summary>
        /// （自動依據當前內網環境、外網環境密碼規則）檢查使用者密碼長度是否符合系統規定。若符合系統規定時傳回 ture，否則傳回 false。
        /// </summary>
        /// <param name="password">使用者密碼（必須是明文密碼，請勿傳入加密之後的密碼）</param>
        /// <returns></returns>
        public bool IsMatchPwdLength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            else
            {
                int len = password.Length;
                return (len >= ConfigModel.PwdLenMin && len <= ConfigModel.PwdLenMax);
            }
        }

        /// <summary>
        /// SHA512加密法
        /// </summary>
        /// <param name="originText">加密前字串（必須是明文密碼，請勿傳入加密之後的密碼）</param>
        /// <returns></returns>
        public static string CypherText(string originText)
        {
            string strRtn = "";
            using (System.Security.Cryptography.SHA512CryptoServiceProvider sha512 = new System.Security.Cryptography.SHA512CryptoServiceProvider())
            {
                byte[] dataToHash = System.Text.Encoding.UTF8.GetBytes(originText);
                byte[] hashvalue = sha512.ComputeHash(dataToHash);
                strRtn = Convert.ToBase64String(hashvalue);
            }
            return strRtn;
        }
    }
}