using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Areas.A1.Models;
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
    public class A1DAO : BaseDAO
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
            return base.QueryForList<C101MGridModel>("A1.queryC101MGrid", parm);
        }

        /// <summary>
        /// 查詢帳號明細
        /// </summary>
        public C101MDetailModel QueryC101MDetail(string code)
        {
            Hashtable parm = new Hashtable();
            parm["code"] = code;
            return base.QueryForObject<C101MDetailModel>("A1.queryC101MDetail", parm);
        }


        /// <summary>
        /// 新增/修改 帳號檢核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckC101M(C101MDetailModel model)
        {
            string msg = "";
            TblEEC_Hospital ad = new TblEEC_Hospital();

            if (model.IsNew)
            {
                // 帳號重複
                ad.code = model.code;
                var addata = GetRow(ad);

                if (addata != null)
                {
                    msg += "醫院代碼重複，請重新輸入 ! \n";
                }

                //if (model.code.TONotNullString() == "")
                //{
                //    msg += "請輸入醫院授權碼 ! \n";
                //}
                //else
                //{
                //    if (model.code.Length != model.code.ToTrim().Length)
                //    {
                //        msg += "醫院授權碼不可填入空格，請重新輸入  ! \n";
                //    }
                //}

            }
            else
            {
                if (model.AuthCode.TONotNullString() != "")
                {
                    var addata = GetRowList(new TblEEC_Hospital() { AuthCode = model.AuthCode }).Where(x => x.code != model.code).ToList();
                    if (addata.Any()) msg += "醫院授權碼重複，請重新生成 ! \n";
                }
                //ad.AuthCode = model.AuthCode;
                //var addata = GetRow(ad);
                //if (addata != null)
                //{
                //    msg += "醫院授權碼重複，請重新生成 ! \n";
                //}
            }

            return msg;
        }

        /// <summary>
        /// 新增帳號
        /// </summary>
        /// <param name="model"></param>
        public void AppendC101MDetail(C101MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            //整批交易管理
            try
            {
                base.BeginTransaction();
                //新增 AMDBURM
                TblEEC_Hospital ad = new TblEEC_Hospital();
                ad.InjectFrom(model);
                //long tmpLong = 0;
                //if (long.TryParse(model.orderby, out tmpLong)) ad.orderby = tmpLong; else ad.orderby = 0;

                //ad.code = model.code;   //ad.pwd = this.EncPassword(model.userno);
                //ad.city = model.city;
                //ad.text = model.text;
                //ad.orderby = model.orderby;
                //ad.cityName = model.cityName;
                //ad.AuthCode = model.AuthCode;

                //ad.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
                //ad.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
                //ad.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";
                int res = base.Insert(ad);

                //SendResetMail(model, sm.UserInfo.LoginIP);

                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("AppendC101MDetail failed:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 更新帳號
        /// </summary>
        /// <param name="model"></param>
        public void UpdateC101MDetail(C101MDetailModel model)
        {
            //整批交易管理
            try
            {
                base.BeginTransaction();
                TblEEC_Hospital where = new TblEEC_Hospital();
                where.code = model.code;
                //var getdata = GetRow(where);
                TblEEC_Hospital newdata = new TblEEC_Hospital();
                newdata.InjectFrom(model);
                //newdata.authstatus = getdata.authstatus == "2" ? "2" : model.authstatus;
                //newdata.login_yn = model.loginstatus.Contains("1") ? "1" : "0";
                //newdata.sso_yn = model.loginstatus.Contains("2") ? "1" : "0";
                //newdata.doc_yn = model.loginstatus.Contains("3") ? "1" : "0";

                int res = base.Update(newdata, where);
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
            try
            {
                base.BeginTransaction();
                TblEEC_Hospital where = new TblEEC_Hospital();
                where.code = model.code;
                base.Delete(where);
                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("DeleteC101MDetail failed:" + ex.Message, ex);
            }
        }

        public void C101MSendSetPwdMail(string HospCode, string HospName, string Email, string AuthCode, string IP)
        {
            SessionModel sm = SessionModel.Get();
            try
            {
                base.BeginTransaction();
                // 產出驗證碼
                var gd = Guid.NewGuid().ToString("N");
                // 重複發驗證信狀態全部更新為 "Y" 
                TblEEC_Hospital_CHANGEPWD_GUID where = new TblEEC_Hospital_CHANGEPWD_GUID();
                where.hospital_code = HospCode;
                TblEEC_Hospital_CHANGEPWD_GUID newdata = new TblEEC_Hospital_CHANGEPWD_GUID();
                newdata.guidyn = "Y";
                int res1 = Update(newdata, where);
                // 紀錄驗證碼至資料表比對
                TblEEC_Hospital_CHANGEPWD_GUID ag = new TblEEC_Hospital_CHANGEPWD_GUID();
                ag.hospital_code = HospCode;
                ag.guid = gd;
                ag.guidyn = "N";
                ag.modip = IP;
                ag.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                ag.moduserid = sm.UserInfo.User.userno;
                ag.modusername = sm.UserInfo.User.username;
                int res2 = Insert(ag);
                // 查詢網站網址
                string webUrl = HttpContext.Current.Request.Url.AbsoluteUri;
                Uri uri = new Uri(webUrl);
                string webDomain = uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.IsDefaultPort ? "" : ":" + uri.Port);
                string appDocUrl = string.Format("{0}/Login/C101M/HospPwdChange?Guid={1}", webDomain, gd);
                // 信件內容
                var body = HospName + "您好：<br>";
                body += "我們已收到您於衛生福利部民眾線上申請電子病歷服務平台管理系統的重設密碼申請。<br>";
                body += "醫院授權碼為：" + AuthCode + "<br>";
                body += "請開啟以下密碼重設連結，並依照螢幕上的指示重設您的密碼。<br>";
                body += "在電子郵件發送的 15 分鐘內，開啟電子郵件所附的重設密碼連結。<br><br><br>";
                body += "密碼重設連結：<br>";
                body += "<a target='_blank' href='" + appDocUrl + "'>" + appDocUrl + "</a><br><br>";
                body += "PS.這封電子郵件是由伺服器自動發送。請勿回覆。<br>";
                body += "如果您不是此電子郵件的指定收件人，請刪除此訊息。<br>";
                body += "<br><br><br><br>";
                body += "衛生福利部 民眾線上申辦電子病歷服務平台";
                // 寄信
                MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, Email, "衛福部民眾線上申請電子病歷服務平台管理系統-重設密碼申請通知信件", body);
                mailMessage.IsBodyHtml = true;
                var t = CommonsServices.SendMail(mailMessage);
                // 寄信LOG
                TblAMEMAILLOG_EMAIL maillog = new TblAMEMAILLOG_EMAIL();
                maillog.eservice_id = "A1/C101M";
                maillog.subject = "衛福部民眾線上申請電子病歷服務平台管理系統-重設密碼申請通知信件";
                maillog.body = t.IsSuccess == true ? body : t.ResultText.TONotNullString();
                maillog.send_time = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                maillog.mail = Email;
                maillog.mail_type = "1";
                maillog.modip = IP;
                maillog.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                maillog.moduser = sm.UserInfo.User.userno;
                maillog.modusername = sm.UserInfo.User.username;
                maillog.status = t.IsSuccess == true ? "1" : "0";
                int res3 = Insert(maillog);
                base.CommitTransaction();
            }
            catch (Exception ex)
            {
                base.RollBackTransaction();
                throw new Exception("C101MSendSetPwdMail failed:" + ex.Message, ex);
            }
        }

        #endregion

        #region C102M

        public IList<C102MGridModel> QueryC102M(C102MFormModel parm)
        {
            return base.QueryForListAll<C102MGridModel>("A1.queryC102M", parm);
        }

        #endregion
    }
}