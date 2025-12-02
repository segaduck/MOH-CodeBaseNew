using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ES.Models.ViewModels;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Omu.ValueInjecter;
using ES.Models;
using System.Net.Mail;
using System.IO;
using DocumentFormat.OpenXml.Bibliography;

namespace ES.Controllers
{
    public class Apply_011003Controller : BaseController
    {
        public static string s_SRV_ID = "011003";
        public static string s_SRV_NAME = "社會工作實務經驗及從事社會工作業務年資審查";

        #region 新增申辦案件

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Prompt()
        {
            ShareDAO dao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            Apply_011003ViewModel model = new Apply_011003ViewModel();

            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            string showText = "社會工作實務經驗年資審查";
            sm.LastErrorMessage = string.Format(s_msg_1A, /*s_SRV_NAME*/ showText);
            ViewBag.CanApply = "N"; //TODO

            var data = dao.GetRow<TblSERVICE_DATE>(new TblSERVICE_DATE { SRV_ID = "011003" });
            if (data != null)
            {
                if (DateTime.Now >= data.TIME_S && DateTime.Now < data.TIME_E.Value)
                {
                    ViewBag.CanApply = "Y";
                }
            }
            return View("Prompt011003", model.Form);
        }

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_011003FormModel model = new Apply_011003FormModel();
            ActionResult rtn = View("Index", model);
            model.ADD_TIME = HelperUtil.DateTimeToTwString(DateTime.Now);
            var UsIn = sm.UserInfo.Member;

            if (UsIn != null)
            {
                #region 帶入帳號資訊

                //帳號
                model.ACC_NO = UsIn.ACC_NO;
                // 姓名
                model.NAME = UsIn.NAME;
                // 電話
                var Tel = UsIn.TEL.Split('-');
                model.TEL = Tel[0];
                if (Tel.ToCount() > 1)
                {
                    model.TEL_0 = Tel[1];
                    if (model.TEL_0.IndexOf('#') > 0)
                    {
                        model.TEL_0 = Tel[1].Substring(0, Tel[1].IndexOf('#'));
                        model.TEL_1 = Tel[1].Split('#')[1];
                    }
                }
                   
                // 地址
                model.ADDR_CODE = UsIn.CITY_CD;
                model.ADDR_DETAIL = UsIn.ADDR;
                //行動
                model.MOBILE = UsIn.MOBILE;
                //Mail
                model.MAIL = UsIn.MAIL.Split('@')[0];
                model.MAIL_0 = "0";
                model.MAIL_1 = UsIn.MAIL.Split('@')[1];
                // 身分證
                model.IDN = UsIn.IDN;
                if (UsIn.IDN.TONotNullString()!= "")
                {
                    switch (UsIn.IDN.TONotNullString().Substring(1,1))
                    {
                        case "1":
                            model.SEX_CD = "M";
                            break;
                        case "2":
                            model.SEX_CD = "F";
                            break;
                        default:
                            break;
                    }
                }
                // 生日
                model.BIRTHDAY = HelperUtil.DateTimeToTwString((DateTime)UsIn.BIRTHDAY);
                #endregion
            }

            else rtn = RedirectToAction("Index", "Login");

            //agree: 1:同意新增 /other:請先閱讀規章
            if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            if (agree != null && !agree.Equals("1")) { return Prompt(); }

            return rtn;
        }

        /// <summary>
        /// 導預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_011003FormModel model)
        {
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = dao.ChkApply011003(model);
            if (ModelState.IsValid)
            {
                ModelState.Clear();

                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "";
                }
                else
                {
                    result.status = false;
                    result.message = ErrorMsg;
                }
            }
            else
            {
                result.status = false;
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg = ErrorMsg + item.Errors[0].ErrorMessage + "\r\n";
                    }
                }

                result.message = ErrorMsg;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PreView(Apply_011003FormModel model)
        {
            model.NOTICEDAY = model.NOTICEDAY_YEAR.TONotNullString() + model.NOTICEDAY_MONTH.TONotNullString();
            return PartialView("PreView011003", model);
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011003FormModel form)
        {
            // 由登入資訊取得當前角色的檢定類別資訊
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            switch (form.MAIL_0)
            {
                case "1":
                    form.MAIL_1 = "gmail.com";
                    break;
                case "2":
                    form.MAIL_1 = "yahoo.com.tw";
                    break;
                case "3":
                    form.MAIL_1 = "outlook.com";
                    break;
            }
            var email = $"{form.MAIL}@{form.MAIL_1}";
            var memberName = string.IsNullOrWhiteSpace(form.NAME) ? sm.UserInfo.Member.NAME : form.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(form.MAIL) ? sm.UserInfo.Member.MAIL : email;

            // 存檔
            form.APP_ID = dao.AppendApply011003(form);
            // 寄信
            dao.SendMail_New(memberName, memberEmail, form.APP_ID, "社會工作實務經驗年資審查", "011003");

            return Done("1");
        }

        #endregion

        #region 補件

        /// <summary>
        /// 補件畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_011003FormModel model = new Apply_011003FormModel();

            // 案件基本資訊
            APPLY_011003 app = new APPLY_011003();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            try
            {
                var UsIn = sm.UserInfo.Member;

                // 判斷是否為該案件申請人
                if (alydata.ACC_NO == UsIn.ACC_NO)
                {
                    // 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                    model = dao.GetDetail011003(APP_ID);
                    // 取回案件資料(可依個人方式決定帶值回來的方式)
                    #region 帶入帳號資訊
                    // 申請日期
                    model.ADD_TIME = ((DateTime)appdata.ADD_TIME).ToString("yyyy/MM/dd");

                    // 帳號
                    model.ACC_NO = alydata.ACC_NO;
                    // 單位名稱
                    model.NAME = appdata.NAME;
                    
                    // 地址
                    var addr = model.ADDR;
                    model.ADDR_CODE = model.ADDR_CODE;
                    TblZIPCODE zip = new TblZIPCODE();
                    zip.ZIP_CO = model.ADDR_CODE;
                    var getnam = dao.GetRow(zip);
                    if (getnam.CITYNM != null)
                    {
                        model.ADDR = getnam.ZIP_CO;
                        model.ADDR_DETAIL = addr.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                    }

                    //Mail
                    if (model.MAIL.TONotNullString().Trim() != "")
                    {
                        var mail = model.MAIL;
                        model.MAIL = mail.Split('@')[0];
                        model.MAIL_0 = "0";
                        model.MAIL_2 = "0";
                        model.MAIL_1 = mail.Split('@')[1];

                        switch (mail.Split('@')[1])
                        {
                            case "gmail.com":
                                model.MAIL_0 = "1";
                                model.MAIL_2 = "1";
                                model.MAIL_1 = "";
                                break;
                            case "yahoo.com.tw":
                                model.MAIL_0 = "2";
                                model.MAIL_2 = "2";
                                model.MAIL_1 = "";
                                break;
                            case "outlook.com":
                                model.MAIL_0 = "3";
                                model.MAIL_2 = "3";
                                model.MAIL_1 = "";
                                break;
                        }
                    }

                    if (model.GRADUATION.TONotNullString().Trim() != "")
                    {
                        var GRADUATION = model.GRADUATION;
                        model.GRADUATION_TEXT = GRADUATION.Substring(0, GRADUATION.Length - 2);
                        if (GRADUATION.SubstringTo(GRADUATION.Length - 2, 1) == "0")
                        {
                            model.GRADUATION_MONTH_TEXT = GRADUATION.SubstringTo(GRADUATION.Length - 1, 1);
                        }
                        else
                        {
                            model.GRADUATION_MONTH_TEXT = GRADUATION.SubstringTo(GRADUATION.Length - 2);
                        }

                    }
                    model.BIRTHDAY = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.BIRTHDAY));
                    #endregion
                    if (!string.IsNullOrEmpty(model.GRADUATION))
                    {
                        var GRADUATION = model.GRADUATION;
                        model.GRADUATION = GRADUATION.Substring(0, GRADUATION.Length - 2);
                        model.GRADUATION_MONTH = GRADUATION.SubstringTo(GRADUATION.Length - 2).TOInt32().TONotNullString();
                    }
                    if (!string.IsNullOrEmpty(model.NOTICEDAY))
                    {
                        var NOTICEDAY = model.NOTICEDAY;
                        model.NOTICEDAY_YEAR = NOTICEDAY.Substring(0, NOTICEDAY.Length - 2);
                        model.NOTICEDAY_MONTH = NOTICEDAY.SubstringTo(NOTICEDAY.Length - 2).TOInt32().TONotNullString();
                    }
                    if (alydata.FLOW_CD.TONotNullString() == "2")
                    {
                        // 取回補件備註欄位
                        TblAPPLY_NOTICE ntwhere = new TblAPPLY_NOTICE();
                        ntwhere.APP_ID = APP_ID;
                        ntwhere.ISADDYN = "N";
                        var ntdata = dao.GetRowList(ntwhere);

                        // 無動態欄位
                        var ntLst = new List<string>();
                        foreach (var item in ntdata)
                        {
                            ntLst.Add(item.Field);
                        }

                        model.FieldStr = string.Join(",", ntLst);
                    }
                    ShareDAO shareDAO = new ShareDAO();
                    model.APPSTATUS = alydata.FLOW_CD.TONotNullString() == "2" ? "1" : "0";
                    if (alydata.FLOW_CD == "2")
                    {
                        if (shareDAO.CalculationDocDate("011003", APP_ID)) model.APPSTATUS = "0";
                        else model.APPSTATUS = "1";
                    }
                    
                    return View("AppDoc", model);

                }
                else
                {
                    throw new Exception("非案件申請人無法瀏覽次案件 !");
                }
            }
            catch (Exception ex)
            {
                sm.LastErrorMessage = ex.Message;
                return RedirectToAction("Index", "Login");
            }

        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_011003FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            switch (model.MAIL_0)
            {
                case "1":
                    model.MAIL_1 = "gmail.com";
                    break;
                case "2":
                    model.MAIL_1 = "yahoo.com.tw";
                    break;
                case "3":
                    model.MAIL_1 = "outlook.com";
                    break;
            }
            var email = $"{model.MAIL}@{model.MAIL_1}";
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : email;
            
            model.NOTICEDAY = model.NOTICEDAY_YEAR.TONotNullString() + model.NOTICEDAY_MONTH.TONotNullString();
            // 存檔
            var count = dao.UpdateApply011003(model);
            // 寄信
            dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "社會工作實務經驗年資審查", "011003", count);

            return Done("2", count);

        }

        #endregion

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Done(string status, string Count = "")
        {
            SessionModel sm = SessionModel.Get();
            Apply_011003DoneModel model = new Apply_011003DoneModel();
            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }
    }
}
