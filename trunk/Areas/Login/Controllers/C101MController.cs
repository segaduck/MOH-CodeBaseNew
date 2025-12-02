using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.Login.Models;
using EECOnline.Commons;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;

namespace EECOnline.Areas.Login.Controllers
{
    public class C101MController : Controller
    {
        protected static readonly ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Login
        [AllowAnonymous]
        public ActionResult Index()
        {
            C101MFormModel form = new C101MFormModel() { ThePage = "1" };
            ActionResult rtn = View(form);
            return rtn;
        }

        /// <summary>
        /// 使用者按下登入按鈕
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(C101MFormModel form)
        {
            ActionResult rtn;
            try
            {
                var IP = HttpContext.Request.UserHostAddress;
                // 系統管理邏輯
                ClamService service = new ClamService();

                // 檢查驗證碼及輸入欄位
                this.InputValidate(form);

                // 登入帳密檢核, 並取得使用者帳號及權限角色清單資料
                LoginUserInfo userInfo = null;
                if (form.ThePage == "1")
                {
                    userInfo = service.LoginValidate(form.UserNo, form.UserPwd, IP);
                }
                if (form.ThePage == "2")
                {
                    userInfo = service.LoginValidate_Hosp(form.AuthCode, form.AuthCode_Pwd, IP);
                    LoginDAO dao = new LoginDAO();
                    var tmpObj = dao.GetRow(new TblEEC_Hospital() { AuthCode = form.AuthCode });
                    if (tmpObj != null) userInfo.HospitalCode = tmpObj.code;
                }
                if (userInfo == null) return Redirect("~/Home/Index");
                userInfo.LoginIP = IP;
                userInfo.LoginTab = form.ThePage;

                // 登入失敗, 丟出錯誤訊息
                if (!userInfo.LoginSuccess)
                {
                    throw new LoginExceptions(userInfo.LoginErrMessage);
                }

                // 將登入者資訊保存在 SessionModel 中
                SessionModel sm = SessionModel.Get();
                sm.UserInfo = userInfo;

                // 取得帳號單位
                if (form.ThePage == "1")
                {
                    LoginDAO dao = new LoginDAO();
                    TblAMUROLE ar = new TblAMUROLE();
                    ar.userno = form.UserNo;
                    var ar_data = dao.GetRow(ar);
                    TblAMGRP ag = new TblAMGRP();
                    ag.grp_id = ar_data.grp_id;
                    var ag_data = dao.GetRow(ag);
                    sm.UserInfo.User.UNIT_NAME = ag_data.grpname;
                }
                if (form.ThePage == "2")
                {
                    LoginDAO dao = new LoginDAO();
                    TblAMUROLE_Hosp ar = new TblAMUROLE_Hosp();
                    ar.AuthCode = form.AuthCode;
                    var ar_data = dao.GetRow(ar);
                    TblAMGRP_Hosp ag = new TblAMGRP_Hosp();
                    ag.grp_id = ar_data.grp_id;
                    var ag_data = dao.GetRow(ag);
                    sm.UserInfo.User.UNIT_NAME = ag_data.grpname;
                }

                // 將登入者群組權限功能清單保存在 SessionModel 中
                if (form.ThePage == "1") sm.RoleFuncs = service.GetUserRoleFuncs(userInfo);
                if (form.ThePage == "2") sm.RoleFuncs = service.GetUserRoleFuncs_Hosp(userInfo);

                // 須變更密碼
                if (userInfo.ChangePwdRequired)
                {
                    if (form.ThePage == "1")
                    {
                        string msg = userInfo.LoginErrMessage;
                        sm.LastErrorMessage = string.IsNullOrEmpty(msg) ? "為了安全起見，請您先變更密碼再用本系統！" : msg;
                        sm.RedirectUrlAfterBlock = Url.Action("PasswordChange", "C101M", new { area = "Login", useCache = "2" });
                    }
                    if (form.ThePage == "2")
                    {
                        //string msg = userInfo.LoginErrMessage;
                        //sm.LastErrorMessage = string.IsNullOrEmpty(msg) ? "為了安全起見，請您先變更密碼再用本系統！" : msg;
                        //sm.RedirectUrlAfterBlock = Url.Action("PasswordChange", "C101M", new { area = "Login", useCache = "2" });
                    }
                    C101MFormModel model = new C101MFormModel();
                    model = form;
                    return View("Index", model);
                }
                else if (TempData["RedirectPath"] != null) // 重導
                {
                    var TPath = TempData["RedirectPath"].TONotNullString().ToSplit('/');

                    if (TempData["RedirectApyId"] != null || TempData["RedirectGUID"] != null)
                    {
                        var TApy_id = TempData["RedirectApyId"];
                        var TGUID = TempData["RedirectGUID"];

                        TempData["RedirectApyId"] = null;
                        TempData["RedirectGUID"] = null;
                        return RedirectToAction(TPath[2], TPath[1], new { area = TPath[0], apy_id = TApy_id, GUID = TGUID });

                    }
                    TempData["RedirectPath"] = null;

                    return RedirectToAction(TPath[2], TPath[1], new { area = TPath[0] });
                }

                rtn = RedirectToAction("Index", "C102M");
            }
            catch (LoginExceptions ex)
            {
                if (form.ThePage == "1") LOG.Info("Login(" + form.UserNo + ") Failed from " + Request.UserHostAddress + ": " + ex.Message);
                if (form.ThePage == "2") LOG.Info("Login(" + form.AuthCode + ") Failed from " + Request.UserHostAddress + ": " + ex.Message);
                // 清除不想要 Cache POST data 的欄位
                ModelState.Remove("form.ValidateCode");

                C101MFormModel model = new C101MFormModel();
                model.UserNo = form.UserNo;  // 帳號
                model.AuthCode = form.AuthCode;  // 醫院授權碼
                model.ThePage = form.ThePage;
                model.ErrorMessage = ex.Message;
                rtn = View("Index", model);
            }
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
            C101MChangeModel model = new C101MChangeModel();
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
                                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "Login" });
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
                        sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "Login" });
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Error("PasswordChange failed:" + ex.TONotNullString());
                sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "Login" });
            }


            return rtn;
        }

        /// <summary>
        /// 變更密碼儲存
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeSave(C101MChangeModel model)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
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
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "Login" });
                }
                else
                {
                    dao.SaveC101MForget(model, IP);
                    sm.LastResultMessage = "密碼變更成功，請重新登入 !";
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "Login" });
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
        public ActionResult Forget()
        {
            SessionModel sm = SessionModel.Get();
            C101MForgetModel model = new C101MForgetModel();

            return View("Forget", model);
        }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <returns></returns>
        public ActionResult Send(C101MForgetModel model)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            ActionResult rtn = View("Forget", model);
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
                        sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "Login", useCache = "0" });
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
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "Login", useCache = "0" });
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
        public ActionResult PWDCHECK(C101MChangeModel model)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
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

            //if (string.IsNullOrEmpty(model.pwd)) model.PWDCheck.Chk_7 = false;
            //else
            //{
            //    model.PWDCheck.Chk_7 = true;
            //    TblAMCHANGEPWD_LOG aml = new TblAMCHANGEPWD_LOG();
            //    aml.userno = model.userno;
            //    aml.status = "1";
            //    var amldata = dao.GetRowList(aml).OrderByDescending(m => m.modtime);
            //    var t = 0;
            //    if (amldata.ToCount() == 0)
            //    {
            //        model.PWDCheck.Chk_7 = true;
            //    }
            //    else
            //    {
            //        foreach (var item in amldata)
            //        {
            //            t++;
            //            if (t <= 3)
            //            {
            //                if (item.pwd.TONotNullString() == this.EncPassword(model.pwd.TONotNullString()))
            //                {
            //                    model.PWDCheck.Chk_7 = false;
            //                    break;
            //                }
            //            }
            //            else
            //            {
            //                model.PWDCheck.Chk_7 = true;
            //                break;
            //            }
            //        }
            //    }

            //}

            if (string.IsNullOrEmpty(model.pwd)) model.PWDCheck.Chk_8 = false;
            else if (model.pwd.TONotNullString() == model.userno.TONotNullString()) model.PWDCheck.Chk_8 = false;
            else model.PWDCheck.Chk_8 = true;

            return PartialView("Check", model);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            Session.Clear();
            Session.RemoveAll();
            return RedirectToAction("Index", "C101M", new { area = "Login" });
        }

        /// <summary>
        /// 圖型驗證碼轉語音撥放頁
        /// </summary>
        /// <returns></returns>
        public ActionResult VCodeAudio()
        {
            return View();
        }

        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCode()
        {
            EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(5);
            EECOnline.Models.SessionModel.Get().LoginValidateCode = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        /// <summary>
        /// 將當前的驗證碼轉成 Wav audio 輸出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCodeAudio()
        {
            string vCode = EECOnline.Models.SessionModel.Get().LoginValidateCode;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }

        /// <summary>
        /// 醫院 設定密碼頁
        /// </summary>
        /// <param name="Guid"></param>
        /// <returns></returns>
        public ActionResult HospPwdChange(string Guid)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            C101MHospPwdChangeModel model = new C101MHospPwdChangeModel();
            model.PWDCheck = new PasswordCheckModel() { Chk_1 = false, Chk_2 = false, Chk_3 = false, Chk_4 = false, Chk_5 = false, Chk_6 = false, };
            ActionResult rtn = View("HospPwdChange", model);
            try
            {
                if (string.IsNullOrEmpty(Guid))
                {
                    sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                    return Redirect("~/Home/Index");
                }
                // 找出該GUID下的資訊
                TblEEC_Hospital_CHANGEPWD_GUID ag = new TblEEC_Hospital_CHANGEPWD_GUID();
                ag.guid = Guid;
                ag.guidyn = "N";
                var agdata = dao.GetRow(ag);
                if (agdata != null)
                {
                    TblEEC_Hospital am = new TblEEC_Hospital();
                    am.code = agdata.hospital_code;
                    var amdata = dao.GetRow(am);
                    if (amdata != null)
                    {
                        if (DateTime.Now > ((DateTime)HelperUtil.TransTwLongToDateTime(agdata.modtime)).AddMinutes(15))
                        {
                            sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                            return Redirect("~/Home/Index");
                        }
                        else
                        {
                            model.guid = Guid;
                            model.code = amdata.code;
                            model.text = amdata.text;
                        }
                    }
                }
                else
                {
                    sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                    return Redirect("~/Home/Index");
                }
            }
            catch (Exception ex)
            {
                LOG.Error("HospPwdChange failed:" + ex.TONotNullString());
                sm.LastErrorMessage = "連結已失效，請重新申請忘記密碼 !";
                return Redirect("~/Home/Index");
            }
            return rtn;
        }

        [HttpPost]
        public ActionResult HospPwdChangeCHECK(C101MHospPwdChangeModel model)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            ShareCodeListModel sc = new ShareCodeListModel();
            ModelState.Clear();

            model.PWDCheck = new PasswordCheckModel();

            if (model.PWD.TONotNullString().Length < 8) model.PWDCheck.Chk_1 = false;
            else model.PWDCheck.Chk_1 = true;

            if (!Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*[a-z])")) model.PWDCheck.Chk_2 = false;
            else model.PWDCheck.Chk_2 = true;

            if (!Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*[A-Z])")) model.PWDCheck.Chk_3 = false;
            else model.PWDCheck.Chk_3 = true;

            if (!Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*\d)")) model.PWDCheck.Chk_4 = false;
            else model.PWDCheck.Chk_4 = true;

            if (string.IsNullOrEmpty(model.PWD)) model.PWDCheck.Chk_5 = false;
            else if (Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*[ ])")) model.PWDCheck.Chk_5 = false;
            else model.PWDCheck.Chk_5 = true;

            if (string.IsNullOrEmpty(model.PWD) || string.IsNullOrEmpty(model.PWD_REPEAT)) model.PWDCheck.Chk_6 = false;
            else if (model.PWD.TONotNullString() != model.PWD_REPEAT.TONotNullString()) model.PWDCheck.Chk_6 = false;
            else model.PWDCheck.Chk_6 = true;

            if (string.IsNullOrEmpty(model.PWD)) model.PWDCheck.Chk_8 = false;
            else if (model.PWD.TONotNullString() == model.code.TONotNullString()) model.PWDCheck.Chk_8 = false;
            else model.PWDCheck.Chk_8 = true;

            return PartialView("HospPwdCheck", model);
        }

        public ActionResult HospPwdChangeSave(C101MHospPwdChangeModel model)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            ActionResult rtn = View("HospPwdChange", model);
            var ErrMsg = "";
            if (string.IsNullOrEmpty(model.PWD) || string.IsNullOrEmpty(model.PWD_REPEAT)) ErrMsg = "請輸入密碼或重複密碼 ! \n";
            else
            {
                if (model.PWD.TONotNullString().Length < 8) ErrMsg = ErrMsg + "密碼長度至少8字元  \n";
                if (!Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*[a-z])")) ErrMsg = ErrMsg + "密碼需包含小寫英文字母  \n";
                if (!Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*[A-Z])")) ErrMsg = ErrMsg + "密碼需包含大寫英文字母 \n";
                if (!Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*\d)")) ErrMsg = ErrMsg + "密碼需包含數字  \n";
                if (Regex.IsMatch(model.PWD.TONotNullString(), @"^(?=.*[ ])")) ErrMsg = ErrMsg + "密碼不得含空白字元  \n";
                if (model.PWD.TONotNullString() != model.PWD_REPEAT.TONotNullString()) ErrMsg = ErrMsg + "密碼與重複密碼需相符  \n";
                if (model.PWD.TONotNullString() == model.code.TONotNullString()) ErrMsg = ErrMsg + "變更後密碼不得與醫院代號相同 \n";

                var tmpPwdList = dao.GetRowList(new TblEEC_Hospital_PWDLOG() { hospital_code = model.code, status = "1", })
                    .OrderByDescending(x => x.modtime)
                    .Take(3)
                    .ToList();
                if (tmpPwdList.Where(x => x.pwd == this.CypherText(model.PWD.TONotNullString())).Any())
                    ErrMsg = ErrMsg + "變更後密碼不得與前三次帳號密碼相同 \n";
            }
            if (ErrMsg == "")
            {
                ModelState.Clear();
                if (string.IsNullOrEmpty(model.guid))
                {
                    sm.LastResultMessage = "密碼變更失敗，請重新申請 !";
                    return Redirect("~/Home/Index");
                }
                else
                {
                    dao.SaveC101MHospPwd(model, HttpContext.Request.UserHostAddress);
                    sm.LastResultMessage = "密碼變更成功，請重新登入 !";
                    return Redirect("~/Home/Index");
                }
            }
            else
            {
                ErrMsg = "提醒您，變更密碼時須遵守以下條件 \n" + ErrMsg;
                model.ErrorMessage = ErrMsg;
                sm.LastErrorMessage = ErrMsg;
            }
            return rtn;
        }

        private void InputValidate(C101MFormModel form)
        {
            if (form.ThePage == "1")
            {
                if (string.IsNullOrEmpty(form.UserNo) || string.IsNullOrEmpty(form.UserPwd))
                {
                    LoginExceptions ex = new LoginExceptions("請輸入帳號及密碼 !!");
                    throw ex;
                }
            }
            if (form.ThePage == "2")
            {
                if (string.IsNullOrEmpty(form.AuthCode) || string.IsNullOrEmpty(form.AuthCode_Pwd))
                {
                    LoginExceptions ex = new LoginExceptions("請輸入醫院授權碼及醫院授權碼密碼 !!");
                    throw ex;
                }
            }
            if (string.IsNullOrEmpty(form.ValidateCode) || !form.ValidateCode.Equals(SessionModel.Get().LoginValidateCode))
            {
                LoginExceptions ex = new LoginExceptions("驗證碼輸入錯誤");
                throw ex;
            }
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
        private string CypherText(string originText)
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