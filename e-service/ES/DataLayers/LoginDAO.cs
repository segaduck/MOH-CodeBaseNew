using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ES.Models.Share;
using Omu.ValueInjecter;
using System.Collections;
using ES.Models;
using System.Configuration;
using System.Data.SqlClient;
using ES.Action;
using ES.Utils;
using System.Text;
using System.Data;
using ES.Services;
using Dapper;
using ES.Models.Entities;
using ES.Commons;
using System.Text.RegularExpressions;

namespace ES.DataLayers
{
    public class LoginDAO : BaseAction
    {
        /// <summary>
        /// 資料庫連線
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        public void Tran(SqlConnection conn, SqlTransaction tran)
        {
            object _thisLock = new object();
            lock (_thisLock)
            {
                this.conn = conn;
                this.tran = tran;
            }
        }

        #region 加入會員

        /// <summary>
        /// 帳號檢查
        /// </summary>
        /// <param name="ACC_NO"></param>
        /// <returns></returns>
        public string ChkACC_NO(string ACC_NO)
        {
            var dictionary = new Dictionary<string, object>
                    {
                        { "@ACC_NO",ACC_NO }
                    };
            var parameters = new DynamicParameters(dictionary);

            var result = new List<TblMEMBER>();

            string _sql = @"
    SELECT ACC_NO FROM MEMBER WHERE 1 = 1 AND ACC_NO = @ACC_NO";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<TblMEMBER>(_sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result.ToCount() > 0 ? "帳號重複 \n" : "";
        }

        /// <summary>
        /// 一般自然人／自然人憑證檢核
        /// </summary>
        /// <param name="ACC_NO"></param>
        /// <param name="PSWD"></param>
        /// <returns></returns>
        public IList<ClamMember> ChkACC_NO2(string ACC_NO, string PSWD)
        {
            string PSWD_encry = DataUtils.Crypt256(PSWD); //加密 

            var dictionary = new Dictionary<string, object>
                    {
                        { "@ACC_NO",ACC_NO },{ "@PSWD",PSWD_encry }
                    };
            var parameters = new DynamicParameters(dictionary);

            IList<ClamMember> result = null;

            string _sql = @"
    SELECT ACC_NO, UPPER(IDN) IDN, SEX_CD, NAME, ENAME
    ,CNT_NAME, CNT_ENAME, CNT_TEL
    ,TEL, FAX, MAIL, CITY_CD ,ADDR 
    ,BIRTHDAY
    ,EADDR,MEDICO,CHR_NAME,CHR_ENAME
    /*,REPLACE(dbo.RT_DataFormat(BIRTHDAY),'/','') BIRTHDAY*/
    FROM MEMBER 
    WHERE 1=1
    AND ACC_NO = @ACC_NO AND PSWD = @PSWD";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<ClamMember>(_sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 帳號身分證號碼
        /// </summary>
        /// <param name="IDN"></param>
        /// <returns></returns>
        public string ChkMemberIDN(string IDN)
        {
            var dictionary = new Dictionary<string, object> { { "@IDN", IDN.ToUpper() } };
            var parameters = new DynamicParameters(dictionary);
            var result = new List<TblMEMBER>();

            string alt_msg = "身分證號碼";
            if (IDN.Length == 8) { alt_msg = "統一編號"; }
            string err_msg = string.Format("{0}重複 \n", alt_msg);

            string _sql = @"SELECT IDN FROM MEMBER WHERE 1=1 AND UPPER(IDN) = @IDN";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                result = conn.Query<TblMEMBER>(_sql.ToString(), parameters).ToList();
                conn.Close();
                conn.Dispose();
            }

            return result.ToCount() > 0 ? err_msg : "";
        }

        /// <summary>
        /// 個人會員資料檢核-檢查所有欄位
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public string ChkMemberInfo(LoginDetailModel detail)
        {
            string Msg = "";

            if (detail.ACC_NO.TONotNullString() == "")
            {
                Msg += "帳號未填 \n";
            }
            if (Msg.Length > 1) { return Msg; }

            if (Regex.IsMatch(detail.ACC_NO, @"^[A-Za-z](1|2)\d{8}$"))
            {
                //Msg += "帳號不得為身份證格式 \n";
                Msg += "帳號不得為身份證編號/居留證號格式 \n";
            }

            bool pwdisMatch = Regex.IsMatch(detail.PSWD.TONotNullString(), @"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$");
            if (!detail.IsUpdata)
            {
                //新增檢核
                if (string.IsNullOrEmpty(detail.PSWD)) { Msg += "密碼 格式錯誤 \n"; }
                else if (!pwdisMatch) { Msg += "密碼格式錯誤 \n"; }
                if (detail.PSWD != detail.PSWD_CHK) { Msg += "確認密碼與密碼不相符 \n"; }
            }
            else
            {
                //修改檢核
                if (!detail.NoKeyPxasWrod)
                {
                    if (string.IsNullOrEmpty(detail.PSWD)) { Msg += "密碼 格式錯誤 \n"; }
                    else if (!pwdisMatch) { Msg += "密碼格式錯誤 \n"; }
                    if (detail.PSWD != detail.PSWD_CHK) { Msg += "確認密碼與密碼不相符 \n"; }
                }
            }

            //if (detail.IDN.Length != 10 || !CheckUtils.IsIdentity(detail.IDN))
            //{
            //    Msg += "身份證格式錯誤 \n";
            //}
            if (detail.IDN.Length != 10 || (!CheckUtils.IsIdentity(detail.IDN) && !CheckUtils.CheckResidentID(detail.IDN)))
            {
                Msg += "身份證編號/居留證號 格式錯誤 \n";
            }
            if (!Regex.IsMatch(detail.MAIL.TONotNullString(), @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$"))
            {
                Msg += "信箱 格式錯誤 \n";
            }
            if (!Regex.IsMatch(detail.NAME.TONotNullString(), @"^[\u4e00-\u9fa5･\s•]*$"))
            {
                Msg += "姓名 格式錯誤 \n";
            }
            //bool chk_ENAME = Regex.IsMatch(detail.ENAME.TONotNullString(), @"^[a-zA-Z0-9,.-_\u4e00-\u9fa5]");
            //bool chk_ENAME = Regex.IsMatch(detail.ENAME.TONotNullString(), @"^[A-Za-z]*$");
            //if (!chk_ENAME)
            //{
            //    Msg += "英文姓名 格式錯誤 \n";
            //}
            if (!Regex.IsMatch(detail.TEL.TONotNullString(), @"^(\d{2,4}-)(\d{6,8})?(#\d{1,6})?$"))
            {
                Msg += "電話號碼 格式錯誤 \n";
            }
            if (!string.IsNullOrEmpty(detail.MOBILE))
            {
                if (!Regex.IsMatch(detail.MOBILE.TONotNullString(), @"([0][0-9-]{9,11})"))
                {
                    Msg += "手機 格式錯誤 \n";
                }
            }
            if (string.IsNullOrEmpty(detail.ADDR_CODE))
            {
                Msg += "郵遞區號 格式錯誤 \n";
            }
            if (string.IsNullOrEmpty(detail.ADDR_DETAIL))
            {
                Msg += "地址 格式錯誤 \n";
            }
            if (Msg.Length > 1) { return Msg; }

            return Msg;
        }

        /// <summary>
        /// 公司會員資料檢核
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public string ChkMemberInfoC(LoginDetailModel detail)
        {
            string Msg = "";

            if (detail.ACC_NO.TONotNullString() == "")
            {
                Msg += "帳號未填 \n";
            }
            if (Msg.Length > 1) { return Msg; }

            if (Regex.IsMatch(detail.ACC_NO, @"^[A-Za-z](1|2)\d{8}$"))
            {
                Msg += "帳號不得為身份證格式 \n";
            }

            bool pwdisMatch = Regex.IsMatch(detail.PSWD.TONotNullString(), @"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$");
            if (!detail.IsUpdata)
            {
                //新增檢核
                if (string.IsNullOrEmpty(detail.PSWD)) { Msg += "密碼格式錯誤 \n"; }
                else if (!pwdisMatch) { Msg += "密碼格式錯誤 \n"; }
                if (detail.PSWD != detail.PSWD_CHK) { Msg += "確認密碼與密碼不相符 \n"; }
            }
            else
            {
                //修改檢核
                if (!detail.NoKeyPxasWrod)
                {
                    if (string.IsNullOrEmpty(detail.PSWD)) { Msg += "密碼格式錯誤 \n"; }
                    else if (!pwdisMatch) { Msg += "密碼格式錯誤 \n"; }
                    if (detail.PSWD != detail.PSWD_CHK) { Msg += "確認密碼與密碼不相符 \n"; }
                }
            }

            int i_IDN8 = 0;
            if (detail.IDN.Length != 8 || !Int32.TryParse(detail.IDN, out i_IDN8))
            {
                Msg += "統一編號格式錯誤 \n";
            }
            //if (detail.IDN.Length == 10 && !CheckUtils.IsIdentity(detail.IDN))
            //{
            //    Msg += "身份證格式錯誤 \n";
            //}
            if (!Regex.IsMatch(detail.MAIL.TONotNullString(), @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$"))
            {
                Msg += "信箱格式錯誤 \n";
            }
            //if (!Regex.IsMatch(detail.NAME.TONotNullString(), @"^[\u4e00-\u9fa5]*$"))
            //{
            //    Msg += "公司（商號）名稱中文格式錯誤 \n";
            //}
            //bool chk_ENAME = Regex.IsMatch(detail.ENAME.TONotNullString(), @"^[A-Za-z]*$");
            //if (!chk_ENAME)
            //{
            //    Msg += "公司（商號）名稱英文格式錯誤 \n";
            //}
            if (!Regex.IsMatch(detail.TEL.TONotNullString(), @"^(\d{2,4}-)(\d{6,8})?(#\d{1,6})?$"))
            {
                Msg += "電話號碼格式錯誤 \n";
            }
            if (!detail.FAX.TONotNullString().Equals("") && !Regex.IsMatch(detail.FAX.TONotNullString(), @"^(\d{2,4}-)(\d{6,8})?(#\d{1,6})?$"))
            {
                Msg += "傳真號碼格式錯誤 \n";
            }
            //if (detail.IDN.Length == 10 && !string.IsNullOrEmpty(detail.MOBILE))
            //{
            //    if (!Regex.IsMatch(detail.MOBILE.TONotNullString(), @"([0][0-9-]{9,11})"))
            //    {
            //        Msg += "手機格式錯誤 \n";
            //    }
            //}
            if (string.IsNullOrEmpty(detail.ADDR_CODE))
            {
                Msg += "郵遞區號格式錯誤 \n";
            }
            if (string.IsNullOrEmpty(detail.ADDR_DETAIL))
            {
                Msg += "地址格式錯誤 \n";
            }
            if (Msg.Length > 1) { return Msg; }

            return Msg;
        }


        /// <summary>
        /// 檢查所有欄位
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public string ChkpwdMemberInfo(LoginDetailModel detail)
        {
            string Msg = "";

            if (detail.ACC_NO.TONotNullString() == "")
            {
                Msg += "帳號未填 \n";
            }
            if (Msg.Length > 1) { return Msg; }

            //若帳PIN打對，代表同一人，不檢查重複
            IList<ClamMember> mem = ChkACC_NO2(detail.ACC_NO, detail.PSWD);
            if ((mem == null) || (mem.ToCount() == 0))
            {
                Msg = ChkACC_NO(detail.ACC_NO);
                if (Msg.Length > 1) { return Msg; }
            }


            string alt_msg = "身分證號碼";
            if (detail.IDN.Length == 8) { alt_msg = "統一編號"; }
            string err_msg = string.Format("{0}與資料比對有誤 \n", alt_msg);
            if ((mem != null) && (mem.ToCount() > 0))
            {
                foreach (var item in mem)
                {
                    if (!item.IDN.ToUpper().Equals(detail.IDN.ToUpper())) { Msg += err_msg; }
                }
            }
            else
            {
                //detail.IDN
                Msg = ChkMemberIDN(detail.IDN);
                if (Msg.Length > 1) { return Msg; }
            }

            if (Msg.Length > 1) { return Msg; }

            return Msg;
        }

        /// <summary>
        /// 儲存-會員資料修改
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public string SaveEditMemberInfo(LoginDetailModel detail)
        {

            string Msg = "";
            SessionModel sm = SessionModel.Get();
            if (sm.UserInfo == null || sm.UserInfo.Member == null)
            {
                //修改資料非同一組帳號
                Msg = "修改資料有誤!!";
                return Msg;
            }
            ClamMember mem = sm.UserInfo.Member;
            if (!mem.ACC_NO.Equals(detail.ACC_NO))
            {
                //修改資料非同一組帳號
                Msg = "修改資料有誤!!";
                return Msg;
            }

            if (!string.IsNullOrEmpty(detail.PSWD))
            {
                TblMEMBER_Log WHERE_LOG = new TblMEMBER_Log();
                WHERE_LOG.ACC_NO = detail.ACC_NO;
                var LOGS = GetRowList(WHERE_LOG).Where(x => x.PSWD.TONotNullString() != "").OrderByDescending(x => x.MODTIME).Take(3);
                if (LOGS != null && LOGS.Count() > 0)
                {
                    var cry_pswd = DataUtils.Crypt256(detail.PSWD);
                    foreach (var item in LOGS)
                    {
                        if (item.PSWD == cry_pswd)
                        {
                            logger.Warn("密碼不得與前三次相同");
                            //tran.Rollback();
                            return "密碼不得與前三次相同";
                        }
                    }
                }
            }
            //memwhere.PSWD = DataUtils.Crypt256(detail.PSWD);
            //bool fb_canUpdate = false;
            //if (base.GetRowList(memwhere).Count > 0) { fb_canUpdate = true; }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    this.Tran(conn, tran);
                    TblMEMBER memwhere = new TblMEMBER();
                    memwhere.ACC_NO = detail.ACC_NO;

                    // Update member
                    TblMEMBER data = new TblMEMBER();
                    data.InjectFrom(detail);
                    if (!string.IsNullOrEmpty(detail.PSWD))
                    {
                        data.PSWD = DataUtils.Crypt256(detail.PSWD);
                    }
                    data.MOBILE = detail.MOBILE.TONotNullString();
                    data.MAIL_MK = (detail.Mailmark ? null : "N");
                    data.ADDR = detail.ADDR_DETAIL;
                    data.CITY_CD = detail.ADDR_CODE;
                    data.TOWN_CD = detail.ADDR_CODE;
                    data.UPD_FUN_CD = "WEB-REG ";
                    //data.ADD_FUN_CD = "WEB-REG ";
                    data.BIRTHDAY = HelperUtil.TransToDateTime(detail.BIRTHDAY);
                    data.UPD_TIME = DateTime.Now;
                    //data.ADD_TIME = DateTime.Now;
                    base.Update(data, memwhere);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = ex.Message;
                    throw new Exception("SaveMemberInfo failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        public string SaveMemberInfo(LoginDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            //LoginDAO dao = new LoginDAO();
            string Msg = "";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    this.Tran(conn, tran);

                    TblMEMBER memwhere = new TblMEMBER();
                    memwhere.ACC_NO = detail.ACC_NO;
                    memwhere.PSWD = DataUtils.Crypt256(detail.PSWD);
                    bool fb_canUpdate = false;
                    if (base.GetRowList(memwhere).Count > 0) { fb_canUpdate = true; }

                    if (fb_canUpdate)
                    {
                        // Update member
                        TblMEMBER data = new TblMEMBER();
                        data.InjectFrom(detail);
                        data.PSWD = DataUtils.Crypt256(detail.PSWD);
                        data.ADDR = detail.ADDR_DETAIL;
                        data.CITY_CD = detail.ADDR_CODE;
                        data.TOWN_CD = detail.ADDR_CODE;
                        data.UPD_FUN_CD = "WEB-REG ";
                        //data.ADD_FUN_CD = "WEB-REG ";
                        data.BIRTHDAY = HelperUtil.TransToDateTime(detail.BIRTHDAY);
                        data.UPD_TIME = DateTime.Now;
                        //data.ADD_TIME = DateTime.Now;
                        if (!string.IsNullOrEmpty(detail.SERIALNO))
                        {
                            data.CARD_TYPE = detail.CARD_TYPE;
                            data.CARD_INFO = detail.CARD_INFO;
                            data.CARD_IDX = detail.CARD_IDX;
                            data.SERIALNO = detail.SERIALNO;
                        }
                        base.Update(data, memwhere);
                    }
                    else
                    {
                        TblMEMBER data = new TblMEMBER();
                        data.InjectFrom(detail);

                        data.PSWD = DataUtils.Crypt256(detail.PSWD);
                        data.ADDR = detail.ADDR_DETAIL;
                        data.CITY_CD = detail.ADDR_CODE;
                        data.TOWN_CD = detail.ADDR_CODE;
                        data.UPD_FUN_CD = "WEB-REG ";
                        data.ADD_FUN_CD = "WEB-REG ";
                        data.BIRTHDAY = HelperUtil.TransToDateTime(detail.BIRTHDAY);
                        data.UPD_TIME = DateTime.Now;
                        data.ADD_TIME = DateTime.Now;
                        if (!string.IsNullOrEmpty(detail.SERIALNO))
                        {
                            data.CARD_TYPE = detail.CARD_TYPE;
                            data.CARD_INFO = detail.CARD_INFO;
                            data.CARD_IDX = detail.CARD_IDX;
                            data.SERIALNO = detail.SERIALNO;
                        }
                        base.Insert(data);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    Msg = ex.Message;
                    throw new Exception("SaveMemberInfo failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return Msg;
        }

        #endregion

        #region 會員登入

        /// <summary>
        /// 使用者登入帳密檢核, 檢核結果以 LoginUserInfo 返回, 
        /// 應檢查 LoginUserInfo.LoginSuccess 判斷登入是否成功
        /// </summary>
        /// <param name="userNo">使用者ID</param>
        /// <param name="userPwd">使用者登入密碼</param>
        /// <returns></returns>
        public LoginUserInfo LoginValidate(string userNo, string userPwd_encry)
        {
            LoginUserInfo userInfo = new LoginUserInfo();
            userInfo.UserNo = userNo;
            userInfo.LoginSuccess = false;

            // 登入驗證方式: 1.一般帳密登入, 2.憑證登入
            string s_LoginAuth = "1";
            if (string.IsNullOrEmpty(userPwd_encry)) { s_LoginAuth = "2"; }

            var dictionary = new Dictionary<string, object>
                    {
                        { "@ACC_NO",userNo },{ "@PSWD",userPwd_encry }
                    };
            var parameters = new DynamicParameters(dictionary);

            // 取得使用者帳號資料
            TblMEMBER result = null;
            string _sql = @"select *
                            from member
                            where 1 = 1 and isnull(DEL_MK,'N')='N' ";
            //_sql += " and ACC_NO = '" + userNo + "'";
            //_sql += " and PSWD = '" + userPwd + "'";
            _sql += " and ACC_NO = @ACC_NO";
            if (!string.IsNullOrEmpty(userPwd_encry)) { _sql += " and PSWD = @PSWD"; }

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                try
                {
                    result = conn.QueryFirst<TblMEMBER>(_sql, parameters);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            // 登入訊息
            // 資料錯誤
            if (result == null)
            {
                var serviceTel = DataUtils.GetConfig("SERVICETEL"); // 系統操作服務諮詢電話
                //登入失敗
                userInfo.LoginErrMessage = "帳號或密碼錯誤! 帳號登入失敗若達五次將被鎖定，請諮詢系統操作服務諮詢電話：" + serviceTel;
                return userInfo;
            }

            // 資料正確
            ClamMember mb = new ClamMember();

            // 將資料塞入SESSION
            userInfo.LoginSuccess = true;

            mb.InjectFrom(result);

            if (userInfo.LoginSuccess)
            {
                //登入驗證方式: 1.一般帳密登入, 2.憑證登入
                userInfo.LoginAuth = s_LoginAuth;//"1";
                userInfo.Member = mb;
            }

            return userInfo;
        }

        /// <summary>
        /// 取得最後登入紀錄
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TblLOGIN_LOG GetLastLog(string userId)
        {

            var dictionary = new Dictionary<string, object>
                    {
                        { "@LOGIN_ID",userId }
                    };
            var parameters = new DynamicParameters(dictionary);

            TblLOGIN_LOG log = new TblLOGIN_LOG();

            string _sql = @"select top 1 *
                            from LOGIN_LOG
                            where 1 = 1";
            _sql += " and LOGIN_ID = @LOGIN_ID";
            _sql += " order by LOGIN_TIME desc";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                log = conn.Query<TblLOGIN_LOG>(_sql, parameters).FirstOrDefault();
                conn.Close();
                conn.Dispose();
            }

            return log;
        }

        /// <summary>
        /// 取得登入失敗次數
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TblLOGIN_LOG GetFalLog(string userId)
        {

            var dictionary = new Dictionary<string, object>
                    {
                        { "@LOGIN_ID",userId }
                    };
            var parameters = new DynamicParameters(dictionary);

            TblLOGIN_LOG log = new TblLOGIN_LOG();

            string _sql = @"select top 1 *
                            from LOGIN_LOG
                            where 1 = 1";
            _sql += " and LOGIN_ID = @LOGIN_ID";
            _sql += " order by LOGIN_TIME desc";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                log = conn.Query<TblLOGIN_LOG>(_sql, parameters).FirstOrDefault();
                conn.Close();
                conn.Dispose();
            }

            return log;
        }

        /// <summary>
        /// 使用者登入帳密檢核, 檢核結果以 LoginUserInfo 返回, 
        /// 應檢查 LoginUserInfo.LoginSuccess 判斷登入是否成功
        /// </summary>
        /// <param name="userNo">使用者ID</param>
        /// <param name="userPwd">使用者登入密碼</param>
        /// <returns></returns>
        public LoginUserInfo LoginInfo(string userNo)
        {
            LoginUserInfo userInfo = new LoginUserInfo();
            userInfo.UserNo = userNo;
            userInfo.LoginSuccess = true;

            var dictionary = new Dictionary<string, object>
                    {
                        { "@ACC_NO",userNo }
                    };
            var parameters = new DynamicParameters(dictionary);

            // 取得使用者帳號資料
            TblADMIN result = null;
            string _sql = @"select *
                            from Admin
                            where 1 = 1";
            _sql += " and ACC_NO = @ACC_NO";

            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                try
                {
                    result = conn.QueryFirst<TblADMIN>(_sql, parameters);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result = null;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            // 登入訊息

            if (result != null)
            {
                ClamAdmin ad = new ClamAdmin();

                // 將資料塞入SESSION
                userInfo.LoginSuccess = true;
                ad.InjectFrom(result);

                if (userInfo.LoginSuccess)
                {
                    userInfo.LoginAuth = "1";
                    userInfo.Admin = ad;
                }
            }

            return userInfo;
        }

        #endregion

        #region 忘記密碼
        /// <summary>
        /// 取得前台會員資料(忘記密碼)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public TblMEMBER GetForgetMember(MemberForgetModel model)
        {
            TblMEMBER where = new TblMEMBER();
            //where.DEL_MK = "N";
            where.ACC_NO = model.Account;
            where.IDN = model.Identity;
            where.MAIL = model.Mail;
            TblMEMBER result = new TblMEMBER();
            result = this.GetRow(where);
            return result;
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool UpdatePassword(TblMEMBER data, string NEW_PSWD)
        {
            string s_log1 = "";
            bool result = false;
            SessionModel sm = SessionModel.Get();
            if (data == null)
            {
                logger.Warn("修改密碼查無會員資料！");
                return result;
            }
            if (string.IsNullOrEmpty(data.ACC_NO))
            {
                logger.Warn("修改密碼查無會員帳號資料！");
                return result;
            }
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    this.Tran(conn, tran);
                    TblMEMBER where = new TblMEMBER();
                    where.ACC_NO = data.ACC_NO;
                    TblMEMBER updata = new TblMEMBER();
                    updata.ACC_NO = data.ACC_NO;
                    updata.IDN = data.IDN;
                    updata.PSWD = DataUtils.Crypt256(NEW_PSWD);
                    updata.UPD_TIME = DateTime.Now;
                    updata.UPD_FUN_CD = "WEB-FORGET";
                    updata.UPD_ACC = data.ACC_NO;
                    updata.DEL_MK = "N";

                    s_log1 += string.Format("where.ACC_NO： {0}\n", where.ACC_NO);
                    s_log1 += string.Format("updata.PSWD： {0}\n", updata.PSWD);
                    s_log1 += string.Format("updata.UPD_TIME： {0}\n", updata.UPD_TIME);
                    s_log1 += string.Format("updata.UPD_FUN_CD： {0}\n", updata.UPD_FUN_CD);
                    s_log1 += string.Format("updata.UPD_ACC： {0}\n", updata.UPD_ACC);

                    base.Update(updata, where);
                    tran.Commit();
                    result = true;
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    logger.Error(ex.Message, ex);
                    logger.Error("[ERROR LOG] \n" + s_log1);
                    tran.Rollback();
                    result = false;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
                return result;
            }
        }
        #endregion

        #region 問與答管理
        public QAViewModel GetQAList()
        {
            QAViewModel result = new QAViewModel();
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    TblQA where = new TblQA();
                    where.DEL_MK = "N";
                    result.grids = base.GetRowList<TblQA>(where);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("GetQAList failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return result;
        }
        #endregion

        #region 會員基本資料檢核
        public bool CheckMemberData(LoginUserInfo model)
        {
            var result = true;
            var msg = string.Empty;
            var member = model.Member;
            if (DataUtils.Crypt256(member.ACC_NO) == member.PSWD)
            {
                msg += "請更新密碼\n";
            }
            if (Convert.ToString(member.ACC_NO) == Convert.ToString(member.IDN))
            {
                msg += "帳號不得同為身份證字號或統一編號\n";
            }
            if (member.IDN.Length == 8)
            {
                // 公司(商號)帳號
                if (string.IsNullOrEmpty(member.NAME))
                {
                    msg += "請輸入公司名稱(中文)\n";
                }
                if (string.IsNullOrEmpty(member.ENAME))
                {
                    msg += "請輸入公司名稱(英文)\n";
                }
                if (string.IsNullOrEmpty(member.TEL))
                {
                    msg += "請輸入公司電話\n";
                }
                if (string.IsNullOrEmpty(member.CITY_CD) && string.IsNullOrEmpty(member.TOWN_CD))
                {
                    msg += "請輸入郵遞區號\n";
                }
                if (string.IsNullOrEmpty(member.ADDR))
                {
                    msg += "請輸入通訊地址(中文)\n";
                }
                if (string.IsNullOrEmpty(member.CNT_NAME))
                {
                    msg += "請輸入聯絡人姓名(中文)\n";
                }
                if (string.IsNullOrEmpty(member.CNT_ENAME))
                {
                    msg += "請輸入聯絡人姓名(英文)\n";
                }
            }
            else
            {
                // 個人帳號
                if (string.IsNullOrEmpty(member.NAME))
                {
                    msg += "請輸入中文姓名\n";
                }
                if (string.IsNullOrEmpty(member.ENAME))
                {
                    msg += "請輸入英文姓名\n";
                }
                if (string.IsNullOrEmpty(member.TEL))
                {
                    msg += "請輸入電話號碼\n";
                }
                if (string.IsNullOrEmpty(member.CITY_CD) && string.IsNullOrEmpty(member.TOWN_CD))
                {
                    msg += "請輸入郵遞區號\n";
                }
                if (string.IsNullOrEmpty(member.ADDR))
                {
                    msg += "請輸入通訊地址\n";
                }
                if (string.IsNullOrEmpty(member.SEX_CD))
                {
                    msg += "請輸入性別\n";
                }
                if (member.BIRTHDAY == null)
                {
                    msg += "請輸入出生年月日\n";
                }
            }
            if (!string.IsNullOrEmpty(msg))
            {
                result = false;
            }
            return result;
        }
        #endregion
    }
}
