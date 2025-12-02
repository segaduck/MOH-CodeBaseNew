using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A5.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;

namespace EECOnline.Areas.A5.Controllers
{
    public class C102MController : BaseController
    {
        public ActionResult Index(C102MFormModel form)
        {
            ActionResult rtn = View(form);
            SessionModel sm = SessionModel.Get();

            return rtn;
        }


        /// <summary>
        /// 變更密碼
        /// </summary>
        /// <returns></returns>
        public ActionResult PasswordChange(string Guid)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            C102MChangeModel model = new C102MChangeModel();
            model.PWDCheck = new PasswordCheckModel();
            model.PWDCheck.Chk_1 = false;
            model.PWDCheck.Chk_2 = false;
            model.PWDCheck.Chk_3 = false;
            model.PWDCheck.Chk_4 = false;
            model.PWDCheck.Chk_5 = false;
            model.PWDCheck.Chk_6 = false;
            ActionResult rtn = View("Change", model);
            try
            {
                if (string.IsNullOrEmpty(Guid))
                {
                    string userno = sm.UserInfo.User.userno;
                    model.userno = userno;
                }
                else
                {
                    // 找出該GUID下的資訊
                    TblAMCHANGEPWD_GUID ag = new TblAMCHANGEPWD_GUID();
                    ag.guid = Guid;
                    ag.guidyn = "N";
                    var agdata = dao.GetRow(ag);

                    if (agdata != null)
                    {
                        TblAMDBURM am = new TblAMDBURM();
                        am.userno = agdata.userno;
                        var amdata = dao.GetRow(am);

                        if (amdata != null)
                        {
                            if (amdata.authstatus == "2")
                            {
                                model.guid = Guid;
                                model.userno = agdata.userno;
                            }
                            else
                            {
                                if (DateTime.Now > ((DateTime)HelperUtil.TransTwLongToDateTime(agdata.modtime)).AddMinutes(15))
                                {
                                    sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "Login" });
                                }
                                else
                                {
                                    model.guid = Guid;
                                    model.userno = agdata.userno;
                                }
                            }
                        }
                    }
                    else
                    {
                        sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                        sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "Login" });
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Error("PasswordChange failed:" + ex.TONotNullString());
                sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "Login" });
            }


            return rtn;
        }

        /// <summary>
        /// 變更密碼儲存
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeSave(C102MChangeModel model)
        {
            SessionModel sm = SessionModel.Get();
            A5DAO dao = new A5DAO();
            ActionResult rtn = View("Change", model);
            var IP = HttpContext.Request.UserHostAddress;
            string msg = "";

            TblAMDBURM ad = new TblAMDBURM();
            ad.userno = model.userno;
            var addata = dao.GetRow(ad);

            if (string.IsNullOrEmpty(model.pwd) || string.IsNullOrEmpty(model.pwd_REPEAT))
            {
                msg = "請輸入密碼或重複密碼 ! \n";
            }
            else
            {
                if (model.pwd.TONotNullString().Length < 8)
                {
                    msg += "密碼長度至少8字元  \n";
                }

                if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[a-z])"))
                {
                    msg += "密碼需包含小寫英文字母  \n";
                }

                if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[A-Z])"))
                {
                    msg += "密碼需包含大寫英文字母 \n";
                }

                if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*\d)"))
                {
                    msg += "密碼需包含數字  \n";
                }

                if (Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[ ])"))
                {
                    msg += "密碼不得含空白字元  \n";
                }

                if (model.pwd.TONotNullString() != model.pwd_REPEAT.TONotNullString())
                {
                    msg += "密碼與重複密碼需相符  \n";
                }

                if (model.pwd.TONotNullString() == model.userno.TONotNullString())
                {
                    msg += "變更後密碼不得與帳號相同 \n";
                }

                TblAMCHANGEPWD_LOG aml = new TblAMCHANGEPWD_LOG();
                aml.userno = model.userno;
                aml.status = "1";
                var amldata = dao.GetRowList(aml).OrderByDescending(m => m.modtime);
                var t = 0;
                foreach (var item in amldata)
                {
                    t++;
                    if (t <= 3)
                    {
                        //if (item.pwd.TONotNullString() == this.EncPassword(model.pwd.TONotNullString()))
                        if (item.pwd.TONotNullString() == this.CypherText(model.pwd.TONotNullString()))
                        {
                            msg += "變更後密碼不得與前三次帳號密碼相同 \n";
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (msg != "")
                {
                    msg = "提醒您，變更密碼時須遵守以下條件 \n" + msg;
                }
            }

            if (msg == "")
            {
                ModelState.Clear();

                if (string.IsNullOrEmpty(model.guid))
                {

                    sm.LastResultMessage = "密碼變更失敗，請重新申請 !";
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "Login" });
                }
                else
                {
                    dao.SaveC102MForget(model, IP);
                    sm.LastResultMessage = "密碼變更成功，請重新登入 !";
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "Login" });
                }

            }
            else
            {
                model.ErrorMessage = msg;
                sm.LastErrorMessage = msg;
            }

            return rtn;
        }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <returns></returns>
        public ActionResult Send(C102MFormModel model)
        {
            SessionModel sm = SessionModel.Get();
            A5DAO dao = new A5DAO();
            ActionResult rtn = View("Index", model);
            var IP = HttpContext.Request.UserHostAddress;
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                try
                {
                    string Msg = dao.CheckUserInfo(model);

                    if (Msg == "")
                    {
                        dao.SendResetMail(model, IP);
                        sm.LastResultMessage = "驗證信已寄出請至信箱收取信件 !";
                        sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "A5", useCache = "0" });
                    }
                    else
                    {
                        sm.LastErrorMessage = Msg;
                    }

                }
                catch (Exception ex)
                {
                    LOG.Error("ForgetUserPWD failed:" + ex.TONotNullString());
                    sm.LastErrorMessage = "驗證失敗，請聯絡系統管理員 !";
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "Login", useCache = "0" });
                }
            }

            return rtn;
        }


        /// <summary>
        /// 密碼檢核
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PWDCHECK(C102MChangeModel model)
        {
            SessionModel sm = SessionModel.Get();
            A5DAO dao = new A5DAO();
            ShareCodeListModel sc = new ShareCodeListModel();
            ModelState.Clear();

            TblAMDBURM ad = new TblAMDBURM();
            ad.userno = model.userno;
            var addata = dao.GetRow(ad);


            model.PWDCheck = new PasswordCheckModel();

            if (model.pwd.TONotNullString().Length < 8) model.PWDCheck.Chk_1 = false;
            else model.PWDCheck.Chk_1 = true;

            if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[a-z])")) model.PWDCheck.Chk_2 = false;
            else model.PWDCheck.Chk_2 = true;

            if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[A-Z])")) model.PWDCheck.Chk_3 = false;
            else model.PWDCheck.Chk_3 = true;

            if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*\d)")) model.PWDCheck.Chk_4 = false;
            else model.PWDCheck.Chk_4 = true;

            if (string.IsNullOrEmpty(model.pwd)) model.PWDCheck.Chk_5 = false;
            else if (Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[ ])")) model.PWDCheck.Chk_5 = false;
            else model.PWDCheck.Chk_5 = true;

            if (string.IsNullOrEmpty(model.pwd) || string.IsNullOrEmpty(model.pwd_REPEAT)) model.PWDCheck.Chk_6 = false;
            else if (model.pwd.TONotNullString() != model.pwd_REPEAT.TONotNullString()) model.PWDCheck.Chk_6 = false;
            else model.PWDCheck.Chk_6 = true;

            if (string.IsNullOrEmpty(model.pwd)) model.PWDCheck.Chk_8 = false;
            else if (model.pwd.TONotNullString() == model.userno.TONotNullString()) model.PWDCheck.Chk_8 = false;
            else model.PWDCheck.Chk_8 = true;

            return PartialView("Check", model);
        }

        /// <summary>
        /// 20230531 已停用RSACSP
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
    }
}