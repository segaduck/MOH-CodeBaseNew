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

namespace ES.Controllers
{
    public class Apply_001039Controller : BaseController
    {
        public static string s_SRV_ID = "001039";
        public static string s_SRV_NAME = "醫師赴國外訓練英文保證函";

        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt001039");
        }

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_001039FormModel form = new Apply_001039FormModel();
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

            form.APPLY_DATE_STR = HelperUtil.DateTimeToTwString(DateTime.Now);
            form.CNAME = sm.UserInfo.Member.NAME;
            form.PID = sm.UserInfo.Member.IDN;
            form.BIRTHDAY_STR= HelperUtil.DateTimeToString(sm.UserInfo.Member.BIRTHDAY);
            form.ENAME = sm.UserInfo.Member.ENAME;
            form.GENDER = sm.UserInfo.Member.SEX_CD;

            return rtn;
        }

        /// <summary>
        /// 導預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_001039FormModel Form)
        {
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if (!string.IsNullOrEmpty(Form.FILE0_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE0_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE0_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE1_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE1_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE1_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE2_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE2_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE2_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE3_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE3_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE3_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE4_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE4_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE4_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE5_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE5_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE5_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE6_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE6_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE6_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE7_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE7_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE7_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE8_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE8_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE8_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
                if (!string.IsNullOrEmpty(Form.FILE9_TEXT))
                {
                    logger.Debug("Apply_001039.FileName:" + Form.FILE9_TEXT);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = Form.FILE9_TEXT.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg = "不支援的檔案格式！";
                    }
                }
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
        public ActionResult PreView(Apply_001039FormModel Form)
        {
            ApplyDAO dao = new ApplyDAO();

            switch (Form.E_MAIL_2)
            {
                case "1":
                    Form.E_MAIL_2 = "gmail.com";
                    break;
                case "2":
                    Form.E_MAIL_2 = "yahoo.com.tw";
                    break;
                case "3":
                    Form.E_MAIL_2 = "outlook.com";
                    break;
                default:
                    Form.E_MAIL_2 = "@" + Form.E_MAIL_3;
                    break;
            }
            Form.E_MAIL = Form.E_MAIL_1 + Form.E_MAIL_2;
            
            if (!string.IsNullOrEmpty(Form.CONTACT_TEL_0)&& !string.IsNullOrEmpty(Form.CONTACT_TEL_1))
            {
                Form.CONTACT_TEL_2 = (string.IsNullOrEmpty(Form.CONTACT_TEL_2)) ? "" : "#" + Form.CONTACT_TEL_2;
                Form.CONTACT_TEL = Form.CONTACT_TEL_0 + Form.CONTACT_TEL_1 + Form.CONTACT_TEL_2;
            }
            if (!string.IsNullOrEmpty(Form.CONTACT_FAX_0) && !string.IsNullOrEmpty(Form.CONTACT_FAX_1))
            {
                Form.CONTACT_FAX_2 = (string.IsNullOrEmpty(Form.CONTACT_FAX_2)) ? "" : "#" + Form.CONTACT_FAX_2;
                Form.CONTACT_FAX = Form.CONTACT_FAX_0.TONotNullString() + Form.CONTACT_FAX_1.TONotNullString() + Form.CONTACT_FAX_2.TONotNullString();
            }
             

            return PartialView("PreView001039", Form);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001039FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            var memberName = string.IsNullOrWhiteSpace(model.CNAME) ? sm.UserInfo.Member.NAME : model.CNAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.E_MAIL) ? sm.UserInfo.Member.MAIL : model.E_MAIL;
            //儲存案件申請
            model.APP_ID = dao.AppendApply001039(model);

            // 申請通知寄信
            dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, "醫師赴國外訓練英文保證函", "001039");

            return Done("1");
        }

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
            Apply_001039AppDocModel model = new Apply_001039AppDocModel();

            // 案件基本資訊
            ApplyModel where = new ApplyModel();
            where.APP_ID = APP_ID;
            var apdata = dao.GetRow(where);

            Apply_001039Model app = new Apply_001039Model();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            try
            {
                var UsIn = sm.UserInfo.Member;
                // 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                model = dao.GetFile_001039(APP_ID);

                // 取回案件資料(可依個人方式決定帶值回來的方式)
                #region 帶入帳號資訊
                model.APP_ID = APP_ID;
                model.InjectFrom(appdata);
                model.FLOW_CD = apdata.FLOW_CD;
                // 是否補件
                model.APPSTATUS = apdata.FLOW_CD;
                model.MAILBODY = apdata.MAILBODY;
                // 申請日期
                model.APPLY_DATE_STR = ((DateTime)appdata.APPLY_DATE).ToString("yyyy/MM/dd");
                if (!string.IsNullOrEmpty(apdata.CNT_TEL))
                {
                    // 電話
                    var tel_0 = apdata.CNT_TEL.ToSplit('-')[0];
                    var tel_2 = (apdata.CNT_TEL.ToSplit('#').ToCount() > 1) ? apdata.CNT_TEL.ToSplit('#')[1] : "";
                    var tel_1 = (string.IsNullOrEmpty(tel_2)) ? apdata.CNT_TEL.ToSplit('-')[1] : apdata.CNT_TEL.ToSplit('-')[1].ToSplit('#')[0];
                    model.CONTACT_TEL_0 = tel_0;
                    model.CONTACT_TEL_1 = tel_1;
                    model.CONTACT_TEL_2 = tel_2;
                }
                //聯絡人傳真
                if (!string.IsNullOrEmpty(appdata.CONTACT_FAX))
                {
                    var fax_0 = appdata.CONTACT_FAX.ToSplit('-')[0];
                    var fax_2 = (appdata.CONTACT_FAX.ToSplit('#').ToCount() >1) ? appdata.CONTACT_FAX.ToSplit('#')[1] : "";
                    var fax_1 = (string.IsNullOrEmpty(fax_2)) ? appdata.CONTACT_FAX.ToSplit('-')[1] : appdata.CONTACT_FAX.ToSplit('-')[1].ToSplit('#')[0];
                    model.CONTACT_FAX_0 = fax_0;
                    model.CONTACT_FAX_1 = fax_1;
                    model.CONTACT_FAX_2 = fax_2;
                }

                model.BIRTHDAY_STR = HelperUtil.DateTimeToString(model.BIRTHDAY);
                //EMAIL
                var email = appdata.E_MAIL.Split('@');
                model.E_MAIL_1 = email[0];
                switch (email[1])
                {
                    case "gmail.com":
                        model.E_MAIL_2 = "1";
                        break;
                    case "yahoo.com.tw":
                        model.E_MAIL_2 = "2";
                        break;
                    case "outlook.com":
                        model.E_MAIL_2 = "3";
                        break;
                    default:
                        model.E_MAIL_2 = "0";
                        model.E_MAIL_3 = email[1];
                        break;
                }

                model.IS_MERGE_FILE = appdata.IS_MERGE_FILE;

                #endregion

                // 取回補件備註欄位
                TblAPPLY_NOTICE ntwhere = new TblAPPLY_NOTICE();
                ntwhere.APP_ID = APP_ID;
                ntwhere.ISADDYN = "N";
                var ntdata = dao.GetRowList(ntwhere);

                // 無動態欄位
                var ntLst = new List<string>();
                // 動態欄位(通常適用於檔案)
                var ntLstForList = new List<string>();
                foreach (var item in ntdata)
                {
                    if (item.SRC_NO.TONotNullString() == "")
                    {
                        ntLst.Add(item.Field);
                    }
                    else
                    {
                        ntLstForList.Add(item.Field);
                    }

                }
                // 組成字串丟回前端跑JS
                model.FieldStr = string.Join(",", ntLst);

                return View("AppDoc", model);

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

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_001039AppDocModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.CNAME) ? sm.UserInfo.Member.NAME : model.CNAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.E_MAIL) ? sm.UserInfo.Member.MAIL : model.E_MAIL;
            // 存檔
            var count = dao.UpdateApply001039(model);
            // 寄信
            dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "醫師赴國外訓練英文保證函", "001039", count);

            return Done("2", count);

        }

        ///// <summary>
        ///// 完成
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        public ActionResult Done(string status, string Count = "")
        {
            SessionModel sm = SessionModel.Get();
            Apply_001039DoneModel model = new Apply_001039DoneModel();
            model.status = status;
            return View("Done", model);
        }

        #endregion 

    }
}
