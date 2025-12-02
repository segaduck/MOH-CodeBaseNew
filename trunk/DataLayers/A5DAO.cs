using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Areas.A5.Models;
using EECOnline.Services;
using EECOnline.Commons;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using System;
using Turbo.Commons;
using System.Web;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace EECOnline.DataLayers
{
    public class A5DAO : BaseDAO
    {
        #region 共用

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

        #region C101M

        /// <summary>
        /// 查詢帳號
        /// </summary>
        public IList<C101MGridModel> QueryC101MGrid(C101MFormModel parm)
        {
            return base.QueryForList<C101MGridModel>("A5.queryC101MGrid", parm);
        }

        /// <summary>
        /// 查詢帳號_匯出Excel
        /// </summary>
        public IList<C101MGridAllModel> QueryC101MGridAll(C101MFormModel parm)
        {
            return base.QueryForListAll<C101MGridAllModel>("A5.queryC101MGridAll", parm);
        }

        /// <summary>
        /// 查詢群組權限一覽_匯出Excel
        /// </summary>
        public IList<C101MGmapmGridModel> QueryC101MGmapmGrid(C101MFormModel parm)
        {
            return base.QueryForListAll<C101MGmapmGridModel>("A5.queryC101MGmapmGrid", parm);
        }

        /// <summary>
        /// 查詢帳號明細
        /// </summary>
        public C101MDetailModel QueryC101MDetail(string userno)
        {
            Hashtable parm = new Hashtable();
            parm["userno"] = userno;
            return base.QueryForObject<C101MDetailModel>("A5.queryC101MDetail", parm);
        }

        /// <summary>
        /// 查詢功能群組
        /// </summary>
        public IList<C101MGrpGridModel> QueryC101MGrp(C101MGrpModel parm)
        {
            return base.QueryForListAll<C101MGrpGridModel>("A5.queryC101MGrp", parm);
        }

        /// <summary>
        /// 檢查函數
        /// </summary>
        /// <param name="Grid"></param>
        /// <returns></returns>
        public string CheckC101M(C101MGrpModel model)
        {
            // 全部都要檢核項目
            var GridY = model.Grid.Where(m => m.Is_Check).ToList();
            if (GridY.ToCount() == 0)
            {
                return "請至少勾選一筆群組資料後離開";
            }
            else
            {
                TblAMDBURM ad = new TblAMDBURM();
                ad.userno = model.userno;
                var addata = GetRow(ad);

                if (addata != null)
                {
                    if (addata.unit_cd != "00" && addata.unit_cd != "01")
                    {
                        foreach (var item in GridY)
                        {
                            TblAMGRP gp = new TblAMGRP();
                            gp.grp_id = item.grp_id;
                            var gpdata = GetRow(gp);

                            if (gpdata != null)
                            {
                                if (gpdata.unit_cd != addata.unit_cd)
                                {
                                    return "選擇的群組必須與單位相符。";
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// 新增/修改 帳號檢核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckC101M(C101MDetailModel model)
        {
            string msg = "";

            // 帳號重複
            TblAMDBURM ad = new TblAMDBURM();
            ad.userno = model.userno;
            var addata = GetRow(ad);
            if (model.IsNew)
            {
                if (addata != null)
                {
                    msg += "帳號重複，請重新輸入 ! \n";
                }

                if (model.userno.TONotNullString() == "")
                {
                    msg += "請輸入帳號 ! \n";
                }
                else
                {
                    if (model.userno.Length != model.userno.ToTrim().Length)
                    {
                        msg += "帳號不可填入空格，請重新輸入  ! \n";
                    }
                }

            }

            // 20230217 信箱檢核收到回報 @mail.XXXXX.gov.tw 無法通過檢核，故將上限調整為5
            // 20230217 信箱檢核收到回報 @mail.XXXXXXX.gov.tw 無法通過檢核，故將上限調整為10
            //Regex mailreg = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Regex mailreg = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            if (!mailreg.IsMatch(model.email))
            {
                msg += "請填入正確的信箱格式  ! \n";
            }

            if (model.authdates.TOInt32() < model.modtime.TOInt32())
            {
                msg += "啟用日期不得小於申請日期  ! \n";
            }

            return msg;
        }

        ///// <summary>
        ///// 新增帳號
        ///// </summary>
        ///// <param name="model"></param>
        //public void AppendC101MDetail(C101MDetailModel model)
        //{
        //    SessionModel sm = SessionModel.Get();
        //    //整批交易管理
        //    BeginTransaction();
        //    try
        //    {
        //        //新增 AMDBURM
        //        TblAMDBURM ad = new TblAMDBURM();
        //        ad.InjectFrom(model);
        //        ad.pwd = this.CypherText(model.userno);   //ad.pwd = this.EncPassword(model.userno);
        //        ad.del_mk = "N";
        //        ad.errct = 0;
        //        ad.modip = sm.UserInfo.LoginIP.TONotNullString();
        //        ad.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
        //        ad.moduserid = sm.UserInfo.UserNo.TONotNullString();
        //        ad.modusername = sm.UserInfo.User.username.TONotNullString();
        //        ad.del_mk = "N";
        //        //ad.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
        //        //ad.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
        //        //ad.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";
        //        Insert(ad);

        //        SendResetMail(model, sm.UserInfo.LoginIP);

        //        CommitTransaction();
        //    }
        //    catch (Exception ex)
        //    {
        //        base.RollBackTransaction();
        //        throw new Exception("AppendC101MDetail failed:" + ex.Message, ex);
        //    }
        //}

        /// <summary>
        /// 更新帳號
        /// </summary>
        /// <param name="model"></param>
        public void UpdateC101MDetail(C101MDetailModel model)
        {
            //整批交易管理
            base.BeginTransaction();
            try
            {
                //更新 AMDBURM 資料表
                TblAMDBURM where = new TblAMDBURM();
                where.userno = model.userno;
                var getdata = GetRow(where);
                TblAMDBURM newdata = new TblAMDBURM();
                newdata.InjectFrom(model);
                newdata.authstatus = getdata.authstatus == "2" ? "2" : model.authstatus;
                //newdata.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
                //newdata.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
                //newdata.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";

                base.Update(newdata, where);
                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("UpdateC101MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 刪除帳號
        /// </summary>
        /// <param name="detail"></param>
        public void DeleteC101MDetail(C101MDetailModel model)
        {
            //整批交易管理
            BeginTransaction();
            try
            {
                TblAMDBURM where = new TblAMDBURM();
                where.userno = model.userno;
                Delete(where);
                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("DeleteC101MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 新增帳號
        /// </summary>
        /// <param name="model"></param>
        public void SaveC101MGrp(C101MGrpModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            BeginTransaction();
            try
            {
                // 刪除該帳號所有群組資料
                TblAMUROLE where = new TblAMUROLE();
                where.userno = model.userno;
                base.Delete(where);

                // 將勾選群組資料新增進入AMUROLE
                var GridY = model.Grid.Where(m => m.IsCheck == "1").ToList();
                foreach (var item in GridY)
                {
                    TblAMUROLE newar = new TblAMUROLE();
                    newar.userno = model.userno;
                    newar.grp_id = item.grp_id.TOInt32();
                    newar.modip = sm.UserInfo.LoginIP.TONotNullString();
                    newar.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                    newar.moduser = sm.UserInfo.UserNo.TONotNullString();
                    newar.modusername = sm.UserInfo.User.username.TONotNullString();

                    base.Insert(newar);
                }

                CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("SaveC101MGrp failed:" + ex.Message, ex);
            }
        }

        #endregion

        /// <summary>
        /// 寄出驗證信
        /// </summary>
        /// <param name="detail"></param>
        public void SendResetMail(C102MFormModel model, string IP)
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
                body += "我們已收到您衛生福利部民眾線上申請電子病歷服務平台管理系統的密碼變更申請。<br>";
                body += "開啟以下密碼重設連結，並依照螢幕上的指示重設您的密碼。<br>";
                body += "密碼重設連結：<br>";
                body += "<a target='_blank' href='" + appDocUrl + "'>" + appDocUrl + "</a><br><br>";
                body += "PS.這封電子郵件是由伺服器自動發送。請勿回覆。<br>";
                body += "如果您不是此電子郵件的指定收件人，請刪除此訊息。<br>";
                body += "<br><br><br><br>";
                body += "衛福部民眾線上申請電子病歷服務平台管理系統-密碼變更通知信件";

                // 寄信
                MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, model.email, "衛福部民眾線上申請電子病歷服務平台管理系統-密碼變更通知信件", body);
                mailMessage.IsBodyHtml = true;
                var t = CommonsServices.SendMail(mailMessage);

                // 寄信LOG
                TblAMEMAILLOG_EMAIL maillog = new TblAMEMAILLOG_EMAIL();
                maillog.eservice_id = "Login";
                maillog.subject = "衛福部民眾線上申請電子病歷服務平台管理系統-密碼變更通知信件";
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
        public void SaveC102MForget(C102MChangeModel model, string IP)
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
                throw new Exception("SaveC102MForget failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 檢核資訊是否相符
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckUserInfo(C102MFormModel model)
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
    }
}