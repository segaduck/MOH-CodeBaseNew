using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Turbo.DataLayer;
using EECOnline.Areas.Login.Models;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using EECOnline.Services;
using System.Collections;
using EECOnline.Models;
using Turbo.Commons;
using OfficeOpenXml;
using System.Net.Mail;
using System.Text;

namespace EECOnline.DataLayers
{
    public class LoginDAO : BaseDAO
    {
        #region 加密

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
        /// SHA512加密法
        /// </summary>
        /// <param name="originText">加密前字串（必須是明文密碼，請勿傳入加密之後的密碼）</param>
        /// <returns></returns>
        public string CypherText(string originText)
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
        #endregion

        #region LOGIN 登入畫面

        /// <summary>
        /// 以指定的 UserNO 取得使用者帳號資料
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public ClamUser GetUser(string userNo)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            return (ClamUser)base.QueryForObject("Login.getClamUser", parms);
        }

        public ClamUser GetUser_Hosp(string AuthCode)
        {
            Hashtable parms = new Hashtable();
            parms["AuthCode"] = AuthCode;
            return (ClamUser)base.QueryForObject("Login.getClamUser_Hosp", parms);
        }

        /// <summary>
        /// 更新登入密碼錯誤次數
        /// </summary>
        /// <param name="dburm"></param>
        public void UpdateUserErrCount(TblAMDBURM dburm)
        {
            TblAMDBURM where = new TblAMDBURM { userno = dburm.userno };
            TblAMDBURM upd = new TblAMDBURM { userno = dburm.userno, errct = dburm.errct, authstatus = dburm.authstatus };
            base.Update<TblAMDBURM>(upd, where, where);
        }

        public void UpdateUserErrCount_Hosp(TblAMDBURM dburm)
        {
            TblEEC_Hospital where = new TblEEC_Hospital { AuthCode = dburm.userno };
            TblEEC_Hospital upd = new TblEEC_Hospital { errct = dburm.errct, AuthStatus = dburm.authstatus };
            base.Update<TblEEC_Hospital>(upd, where, where);
        }

        /// <summary>
        /// 鎖定使用者帳號
        /// </summary>
        /// <param name="dburm"></param>
        public void UpdateUserAccountLock(TblAMDBURM dburm)
        {
            TblAMDBURM where = new TblAMDBURM { userno = dburm.userno };
            TblAMDBURM upd = new TblAMDBURM { userno = dburm.userno, authstatus = "8" };
            base.Update<TblAMDBURM>(upd, where, where);
        }

        /// <summary>
        /// 以指定的 UserNO 取得使用者單位群組角色資料
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public IList<ClamUserGroup> GetUserGroup(string userNo)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            return base.QueryForListAll<ClamUserGroup>("Login.getClamUserGroup", parms);
        }

        public IList<ClamUserGroup> GetUserGroup_Hosp(string AuthCode)
        {
            Hashtable parms = new Hashtable();
            parms["AuthCode"] = AuthCode;
            return base.QueryForListAll<ClamUserGroup>("Login.getClamUserGroup_Hosp", parms);
        }

        /// <summary>
        /// 取得角色群組權限功能清單
        /// </summary>
        /// <param name="examKind">檢定類別ID</param>
        /// <param name="role">角色群組ID</param>
        /// <param name="userNo">使用者ID</param>
        /// <param name="netID"></param>
        /// <returns></returns>
        public IList<ClamRoleFunc> GetRoleFuncs(string userNo)
        {
            Hashtable parms = new Hashtable();
            parms["USERNO"] = userNo;
            return base.QueryForListAll<ClamRoleFunc>("Login.getClamGroupFuncs", parms);
        }

        public IList<ClamRoleFunc> GetRoleFuncs_Hosp(string AuthCode)
        {
            Hashtable parms = new Hashtable();
            parms["AuthCode"] = AuthCode;
            return base.QueryForListAll<ClamRoleFunc>("Login.getClamGroupFuncs_Hosp", parms);
        }

        /// <summary>
        /// 更新密碼
        /// </summary>
        /// <param name="model"></param>
        public void SaveC101MChange(C101MChangeModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                // 更新 AMDBURM
                TblAMDBURM ad = new TblAMDBURM();
                ad.userno = model.userno;
                TblAMDBURM newad = new TblAMDBURM();
                newad.pwd = this.CypherText(model.pwd);  //newad.pwd = this.EncPassword(model.pwd);

                newad.authstatus = "1";
                newad.modip = sm.UserInfo.LoginIP.TONotNullString();
                newad.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                newad.moduserid = sm.UserInfo.UserNo.TONotNullString();
                newad.modusername = sm.UserInfo.User.username.TONotNullString();
                Update(newad, ad);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("SaveC101MChange failed:" + ex.Message, ex);
            }
        }

        #endregion

        #region 忘記密碼

        /// <summary>
        /// 檢核資訊是否相符
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckUserInfo(C101MForgetModel model)
        {
            string Msg = "";

            TblAMDBURM ad = new TblAMDBURM();
            ad.userno = model.userno;
            var addata = GetRow(ad);


            if (addata == null)
            {
                Msg = "查無此帳號，請確認填入的帳號是否正確 !";
            }
            else
            {
                if (addata.idno != model.idno || addata.email.ToUpper() != model.email.ToUpper())
                {
                    Msg = "身分證或信箱不符，請確認填入的正確的資料 !";
                }
            }

            return Msg;
        }

        /// <summary>
        /// 寄出驗證信
        /// </summary>
        /// <param name="detail"></param>
        public void SendResetMail(C101MForgetModel model, string IP)
        {
            SessionModel sm = SessionModel.Get();
            BeginTransaction();
            try
            {
                // 帳號基本資料
                TblAMDBURM ad = new TblAMDBURM();
                ad.userno = model.userno;
                var addata = GetRow(ad);

                // 產出驗證碼
                var gd = Guid.NewGuid().ToString("N");

                // 重複發驗證信狀態全部更新為 "Y" 
                TblAMCHANGEPWD_GUID where = new TblAMCHANGEPWD_GUID();
                where.userno = model.userno;
                TblAMCHANGEPWD_GUID newdata = new TblAMCHANGEPWD_GUID();
                newdata.guidyn = "Y";
                Update(newdata, where);

                // 紀錄驗證碼至資料表比對
                TblAMCHANGEPWD_GUID ag = new TblAMCHANGEPWD_GUID();
                ag.userno = model.userno;
                ag.guid = gd;
                ag.guidyn = "N";
                ag.modip = IP;
                ag.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                ag.moduserid = addata.userno.TONotNullString();
                ag.modusername = addata.username.TONotNullString();
                Insert(ag);

                //查詢網站網址
                string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                Uri uri = new Uri(webUrl);
                string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
                string appDocUrl = string.Format("{0}/Login/C101M/PasswordChange?Guid={1}", webDomain, gd);

                // 信件內容
                var body = addata.username + "您好：<br>";
                body += "我們已收到您於衛生福利部民眾線上申請電子病歷服務平台管理系統的重設密碼申請。<br>";
                body += "請開啟以下密碼重設連結，並依照螢幕上的指示重設您的密碼。<br>";
                body += "在電子郵件發送的 15 分鐘內，開啟電子郵件所附的重設密碼連結。<br><br><br>";
                body += "密碼重設連結：<br>";
                body += "<a target='_blank' href='" + appDocUrl + "'>" + appDocUrl + "</a><br><br>";
                body += "PS.這封電子郵件是由伺服器自動發送。請勿回覆。<br>";
                body += "如果您不是此電子郵件的指定收件人，請刪除此訊息。<br>";
                body += "<br><br><br><br>";
                body += "衛生福利部 民眾線上申辦電子病歷服務平台";


                // 寄信
                MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, model.email, "衛福部民眾線上申請電子病歷服務平台管理系統-忘記密碼通知信件", body);
                mailMessage.IsBodyHtml = true;
                var t = CommonsServices.SendMail(mailMessage);

                // 寄信LOG
                TblAMEMAILLOG_EMAIL maillog = new TblAMEMAILLOG_EMAIL();
                maillog.eservice_id = "Login";
                maillog.subject = "衛福部民眾線上申請電子病歷服務平台管理系統-忘記密碼通知信件";
                maillog.body = t.IsSuccess == true ? body : t.ResultText.TONotNullString();
                maillog.send_time = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                maillog.mail = model.email;
                maillog.mail_type = "1";
                maillog.modip = IP;
                maillog.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                maillog.moduser = addata.userno.TONotNullString();
                maillog.modusername = addata.username.TONotNullString();
                maillog.status = t.IsSuccess == true ? "1" : "0";

                Insert(maillog);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("SendResetMail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 更新密碼
        /// </summary>
        /// <param name="model"></param>
        public void SaveC101MForget(C101MChangeModel model, string IP)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                TblAMCHANGEPWD_GUID ag = new TblAMCHANGEPWD_GUID();
                ag.guid = model.guid;
                var agdata = GetRow(ag);

                TblAMDBURM ad = new TblAMDBURM();
                ad.userno = agdata.userno;
                var addata = GetRow(ad);

                // 更新 AMDBURM
                TblAMDBURM where = new TblAMDBURM();
                where.userno = agdata.userno;
                TblAMDBURM newdata = new TblAMDBURM();
                newdata.pwd = this.CypherText(model.pwd);  //newdata.pwd = this.EncPassword(model.pwd);
                newdata.authstatus = "1";
                newdata.modip = IP;
                newdata.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                newdata.moduserid = addata.userno.TONotNullString();
                newdata.modusername = addata.username.TONotNullString();
                Update(newdata, where);

                TblAMCHANGEPWD_LOG aml = new TblAMCHANGEPWD_LOG();
                aml.pwd = this.CypherText(model.pwd);  //aml.pwd = this.EncPassword(model.pwd);
                aml.userno = agdata.userno;
                aml.status = "1";
                aml.modip = IP;
                aml.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                aml.moduserid = addata.userno.TONotNullString();
                aml.modusername = addata.username.TONotNullString();
                Insert(aml);

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("SaveC101MForget failed:" + ex.Message, ex);
            }
        }

        public void SaveC101MHospPwd(C101MHospPwdChangeModel model, string IP)
        {
            SessionModel sm = SessionModel.Get();
            try
            {
                base.BeginTransaction();
                var tmpPWD = this.CypherText(model.PWD);
                TblEEC_Hospital where = new TblEEC_Hospital() { code = model.code };
                TblEEC_Hospital update = new TblEEC_Hospital() { PWD = tmpPWD };
                int res1 = base.Update(update, where);
                TblEEC_Hospital_CHANGEPWD_GUID where2 = new TblEEC_Hospital_CHANGEPWD_GUID() { guid = model.guid };
                TblEEC_Hospital_CHANGEPWD_GUID update2 = new TblEEC_Hospital_CHANGEPWD_GUID() { guidyn = "Y" };
                int res2 = base.Update(update2, where2);
                string userno = null;
                string username = null;
                if (sm != null && sm.UserInfo != null && sm.UserInfo.User != null)
                {
                    userno = sm.UserInfo.User.userno;
                    username = sm.UserInfo.User.username;
                }
                int res3 = base.Insert(new TblEEC_Hospital_PWDLOG()
                {
                    hospital_code = model.code,
                    pwd = tmpPWD,
                    moduserid = userno,
                    modusername = username,
                    modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now),
                    modip = IP,
                    status = "1",
                });

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("SaveC101MHospPwd failed:" + ex.Message, ex);
            }
        }

        #endregion

        #region 首頁

        public C102MViewModel queryHomeInfo()
        {
            SessionModel sm = SessionModel.Get();
            C102MViewModel Result = new C102MViewModel();
            // 申辦案件數
            var data1 = base.QueryForListAll<Hashtable>("Login.getHomeData1", null);
            // 待補上傳件數
            var data2 = base.QueryForListAll<Hashtable>("Login.getHomeData2", null);
            // 本月申辦預定收款金額
            var data3 = base.QueryForListAll<Hashtable>("Login.getHomeData3", null);
            // 醫院端登入，僅能看到自己的
            if (sm.UserInfo.LoginTab == "2")
            {
                data1 = data1.Where(x => x["AuthCode"].TONotNullString() == sm.UserInfo.UserNo).ToList();
                data2 = data2.Where(x => x["AuthCode"].TONotNullString() == sm.UserInfo.UserNo).ToList();
                data3 = data3.Where(x => x["AuthCode"].TONotNullString() == sm.UserInfo.UserNo).ToList();
            }
            Result.ApplyNoSubNum = data1.ToCount().ToString();
            Result.WaitUploadNum = data2.ToCount().ToString();
            Result.MonthMoneySum = data3.Sum(x => x["price"].TOInt32()).ToString();

            // 圖表類 - 只有部登入能看
            Result.YearApplyNoDatas = new List<string>() { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            Result.YearHisTypeDatas = new List<Hashtable>();
            if (sm.UserInfo.LoginTab == "1")
            {
                // 當年度申請情形
                var data4 = base.QueryForListAll<Hashtable>("Login.getHomeData4", null);
                for (var i = 1; i <= 12; i++)
                {
                    var tmpObj = data4.Where(x => x["TheMonth"].TONotNullString() == i.ToString().PadLeft(2, '0')).FirstOrDefault();
                    if (tmpObj != null) Result.YearApplyNoDatas[i - 1] = tmpObj["MonthCount"].TONotNullString();
                }
                // 當年度病歷單張類型申請情形
                var data5 = base.QueryForListAll<Hashtable>("Login.getHomeData5", null);
                if (data5.Any())
                {
                    var findMaxValue = data5.Max(x => x["value"].TOInt32());
                    foreach (var row in data5) { if (row["value"].TOInt32() == findMaxValue) { row.Add("selected", "true"); break; } }
                }
                Result.YearHisTypeDatas = data5;
            }

            // 最新消息
            if (Result.News == null) Result.News = new List<C102MNewsModel>();
            var tmpNews = base.QueryForListAll<NewsGridModel>("Front.getHomeNews", null).Take(5);
            foreach (var row in tmpNews)
            {
                var item = new C102MNewsModel();
                item.NewsID = row.enews_id;
                item.Type = row.newstype;
                item.Subject = row.subject;
                item.Date = row.showdates;
                item.IsTop = row.totop;
                Result.News.Add(item);
            }
            return Result;
        }

        #endregion

        /// <summary>
        /// 轉換RSACSP為SHA512(不可上版)
        /// </summary>
        /// <returns></returns>
        public void PlanToSHA512()
        {
            RSACSP.RSACSP rsa = new RSACSP.RSACSP();
            TblAMDBURM am = new TblAMDBURM();
            var amlist = this.GetRowList(am);
            foreach (var item in amlist)
            {
                if (item.userno != "turbo")
                {
                    var Opwd = rsa.Utl_Decrypt(item.pwd);

                    TblAMDBURM where = new TblAMDBURM();
                    where.userno = item.userno;
                    TblAMDBURM newdata = new TblAMDBURM();
                    newdata.pwd = this.CypherText(Opwd);

                    Update(newdata, where);
                }
            }
        }
    }
}