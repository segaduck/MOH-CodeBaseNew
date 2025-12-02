using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Commons;
using ES.Services;
using System.ComponentModel;
using ES.DataLayers;
using Omu.ValueInjecter;
using System.Net.Mail;
using ES.Action;
using ES.Extensions;
using System.Data.SqlClient;
using ES.Action.Form;
using ES.Utils;

namespace ES.Controllers
{
    public class Apply_001009Controller : BaseController
    {
        public static string s_SRV_ID = "001009";
        public static string s_SRV_NAME = "醫事人員或公共衛生師資格英文求證";

        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt001009");
        }

        public ActionResult Index(Apply_001009ViewModel model)
        {
            return View();
        }

        [DisplayName("Apply_001009_申請")]
        public ActionResult Apply(string agree)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001009ViewModel form = new Apply_001009ViewModel();

            ActionResult rtn = View("Index", form);

            if (sm == null || sm.UserInfo == null)
            {
                rtn = RedirectToAction("Index", "Login");
                return rtn;
            }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null)
            {
                rtn = RedirectToAction("Index", "Login");
                return rtn;
            }
            //agree: 1:同意新增 /other:請先閱讀規章
            if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            if (agree != null && !agree.Equals("1")) { return Prompt(); }

            #region UserInfo

            if (sm.UserInfo != null)
            {
                form.APPLY_NAME = sm.UserInfo.Member.NAME;
                form.APPLY_PID = sm.UserInfo.Member.IDN;
                form.BIRTHDAY_AC = HelperUtil.DateTimeToString(sm.UserInfo.Member.BIRTHDAY);
                form.Apply = new ApplyModel();
                form.Apply.ENAME = sm.UserInfo.Member.ENAME;
                //20201222-MOHES876拿掉
                //form.MOBILE = sm.UserInfo.Member.MOBILE;

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = sm.UserInfo.Member.TOWN_CD;
                var address = dao.GetRow(zip);
                form.CITY_CODE = sm.UserInfo.Member.TOWN_CD;
                if (address != null && !string.IsNullOrEmpty(address.TOWNNM))
                {
                    form.CITY_TEXT = address.TOWNNM;
                    form.CITY_DETAIL = sm.UserInfo.Member.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }
                else
                {
                    form.CITY_TEXT = string.Empty;
                    form.CITY_DETAIL = sm.UserInfo.Member.ADDR;
                }

                //20201222-MOHES876拿掉
                // 電話
                //string[] tel = sm.UserInfo.Member.TEL.Split('-');
                //if (sm.UserInfo.Member.TEL.TONotNullString().Trim() != "")
                //{
                //    form.TEL_SEC = tel[0];
                //    form.TEL_NO = tel[1].ToSplit('#')[0];

                //    if (sm.UserInfo.Member.TEL.IndexOf('#') > 0)
                //    {
                //        form.TEL_EXT = sm.UserInfo.Member.TEL.Split('#')[1];
                //    }
                //}

                //20201222-MOHES876拿掉
                //Mail
                //if (sm.UserInfo.Member.MAIL.TONotNullString().Trim() != "")
                //{
                //    form.MAIL_ACCOUNT = sm.UserInfo.Member.MAIL.Split('@')[0];
                //    form.MAIL_DOMAIN = sm.UserInfo.Member.MAIL.Split('@')[1];

                //    switch (sm.UserInfo.Member.MAIL.Split('@')[1])
                //    {
                //        case "gmail.com":
                //            form.DOMAINList = "1";
                //            break;
                //        case "yahoo.com.tw":
                //            form.DOMAINList = "2";
                //            break;
                //        case "outlook.com":
                //            form.DOMAINList = "3";
                //            break;
                //        default:
                //            form.DOMAINList = "0";
                //            break;
                //    }
                //}
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            #endregion

            form.APPLY_DATE = HelperUtil.DateTimeToTwString(DateTime.Now);
            return View(form);
        }

        [HttpPost]
        public ActionResult ApplyAjax(Apply_001009ViewModel view)
        {
            var result = new AjaxResultStruct();
            result.message = "";
            result.status = true;

            if (view.IS_MERGE_FILE == "Y")
            {
                //合併時至少上傳一筆資料
                if (string.IsNullOrEmpty(view.FILE1_NAME) &&
                    string.IsNullOrEmpty(view.FILE2_NAME) &&
                    string.IsNullOrEmpty(view.FILE3_NAME) &&
                    string.IsNullOrEmpty(view.FILE4_NAME) &&
                    string.IsNullOrEmpty(view.FILE5_NAME))
                {
                    result.message = "請至少上傳一筆資料 ! \r\n";
                    result.status = false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(view.FILE1_NAME))
                {
                    result.message += "請上傳護照影本 ! \r\n";
                    result.status = false;
                }
                else
                {
                    if (view.FILE1_NAME != null)
                    {
                        logger.Debug("Apply_001009.FileName:" + view.FILE1_NAME);
                        // 允許的附檔名（全小寫）
                        var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif" };
                        // 取得副檔名並轉小寫
                        var ext = view.FILE1_NAME.ToSplit(".").LastOrDefault().ToLower();
                        if (!validExts.Contains(ext))
                        {
                            result.message += "不支援的檔案格式！";
                            result.status = false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(view.FILE2_NAME))
                {
                    result.message += "請上傳醫事人員或公共衛生師中文證書影本 ! \r\n";
                    result.status = false;
                }
                else
                {
                    if (view.FILE2_NAME != null)
                    {
                        logger.Debug("Apply_001009.FileName:" + view.FILE2_NAME);
                        // 允許的附檔名（全小寫）
                        var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif" };
                        // 取得副檔名並轉小寫
                        var ext = view.FILE2_NAME.ToSplit(".").LastOrDefault().ToLower();
                        if (!validExts.Contains(ext))
                        {
                            result.message += "不支援的檔案格式！";
                            result.status = false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(view.FILE3_NAME))
                {
                    result.message += "請上傳考照時之畢業證書影本 ! \r\n";
                    result.status = false;
                }
                else
                {
                    if (view.FILE3_NAME != null)
                    {
                        logger.Debug("Apply_001009.FileName:" + view.FILE3_NAME);
                        // 允許的附檔名（全小寫）
                        var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif" };
                        // 取得副檔名並轉小寫
                        var ext = view.FILE3_NAME.ToSplit(".").LastOrDefault().ToLower();
                        if (!validExts.Contains(ext))
                        {
                            result.message += "不支援的檔案格式！";
                            result.status = false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(view.FILE4_NAME))
                {
                    result.message += "請上傳考試及格證書影本 ! \r\n";
                    result.status = false;
                }
                else
                {
                    if (view.FILE4_NAME != null)
                    {
                        logger.Debug("Apply_001009.FileName:" + view.FILE4_NAME);
                        // 允許的附檔名（全小寫）
                        var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif" };
                        // 取得副檔名並轉小寫
                        var ext = view.FILE4_NAME.ToSplit(".").LastOrDefault().ToLower();
                        if (!validExts.Contains(ext))
                        {
                            result.message += "不支援的檔案格式！";
                            result.status = false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(view.FILE5_NAME))
                {
                    result.message += "請上傳對方機構求證表格 ! \r\n";
                    result.status = false;
                }
                else
                {
                    if (view.FILE5_NAME != null)
                    {
                        logger.Debug("Apply_001009.FileName:" + view.FILE5_NAME);
                        // 允許的附檔名（全小寫）
                        var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif" };
                        // 取得副檔名並轉小寫
                        var ext = view.FILE5_NAME.ToSplit(".").LastOrDefault().ToLower();
                        if (!validExts.Contains(ext))
                        {
                            result.message += "不支援的檔案格式！";
                            result.status = false;
                        }
                    }
                }
            }
            return Content(result.Serialize(), "application/json");
        }

        [DisplayName("Apply_001009_預覽")]
        [HttpPost]
        public ActionResult PreView(Apply_001009ViewModel model)
        {
            return PartialView("PreView001009", model);
        }


        [DisplayName("Apply_001009_完成申報")]
        [HttpPost]
        public ActionResult Save(Apply_001009ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.APPLY_NAME) ? sm.UserInfo.Member.NAME : model.APPLY_NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;
            dao.AppendApply001009(model);
            dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, "醫事人員或公共衛生師資格英文求證", "001009");
            //dao.SendMail_New(memberName, memberEmail, model.APP_ID, "醫事人員資格英文求證", "001009");

            return View("Save", model);
        }

        #region 補件查詢

        /// <summary>
        /// 補件查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001009_補件查詢")]
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001009ViewModel model = new Apply_001009ViewModel();
            model = new Apply_001009ViewModel();
            model.APP_ID = APP_ID;
            model = dao.QueryApply_001009(model);

            // 案件基本資訊
            ApplyModel apply = new ApplyModel();
            apply.APP_ID = APP_ID;
            var applyData = dao.GetRow(apply);

            try
            {
                var userInfo = sm.UserInfo.Member;

                // 判斷是否為該案件申請人
                if (applyData.ACC_NO == userInfo.ACC_NO)
                {
                    ShareDAO shareDAO = new ShareDAO();
                    model.IsNotice = "Y";
                    if (applyData.FLOW_CD == "2" && shareDAO.CalculationDocDate("001009", model.APP_ID))
                    {
                        sm.LastErrorMessage = "已過可補件時間，請聯絡承辦單位!!";
                        model.IsNotice = "N";
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
                if (model == null)
                {
                    sm.LastErrorMessage = "案件編號﹕" + APP_ID + "，尚未分派承辦人員，暫無法查詢!!";
                    return RedirectToAction("Index", "History");
                }
                else
                {
                    sm.LastErrorMessage = ex.Message;
                    return RedirectToAction("Index", "Login");
                }
            }
        }

        #endregion 

        #region 補件存檔

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001009_補件存檔")]
        [HttpPost]
        public ActionResult AppDocSave(Apply_001009ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            var memberName = string.IsNullOrWhiteSpace(model.APPLY_NAME) ? sm.UserInfo.Member.NAME : model.APPLY_NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;
            try
            {

                if (dao.SaveAppDoc001009(model))
                {
                    dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "醫事人員或公共衛生師資格英文求證", "001009", "-1");
                    sm.LastResultMessage = "存檔成功!!";
                    model.Apply.FLOW_CD = "3";
                }
                else
                {
                    sm.LastErrorMessage = "存檔失敗!!";
                }
            }
            catch (Exception ex)
            {
                logger.Error("001009AppDocSave failed:" + ex.TONotNullString());
                sm.LastErrorMessage = "存檔失敗!!";
            }

            return View("Done", model);
        }

        #endregion 
    }
}
