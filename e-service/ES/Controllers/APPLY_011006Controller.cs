using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ES.Controllers
{
    /// <summary>
    /// 專科社會工作師證書換發（更名）
    /// </summary>
    public class Apply_011006Controller : BaseController
    {
        #region 前導說明頁
        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            Apply_011006ViewModel model = new Apply_011006ViewModel();
            //ActionResult rtn = View("Prompt", model.Form);
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, "專科社會工作師證書換發（更名或污損）");
            return View("Prompt", model.Form);
        }

        #endregion

        #region 新增申辦案件
        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_011006ViewModel model = new Apply_011006ViewModel();
            ActionResult rtn = View("Index", model.Form);
            if (sm == null || sm.UserInfo == null)
            {
                rtn = RedirectToAction("Index", "Login");
            }
            else
            {
                var UsIn = sm.UserInfo.Member;
                if (UsIn != null)
                {
                    //agree: 1:同意新增 /other:請先閱讀規章
                    if (string.IsNullOrEmpty(agree)) { agree = "0"; }
                    if (agree != null && !agree.Equals("1")) { return Prompt(); }

                    if (model.Form.APP_TIME == null)
                        model.Form.APP_TIME_SHOW = HelperUtil.DateTimeToTwString(DateTime.Now);
                    else
                        model.Form.APP_TIME_SHOW = HelperUtil.DateTimeToTwString(model.Form.APP_TIME);
                    model.Form.APPLY_TYPE = "3";
                    model.Form.NAME = UsIn.NAME;
                    model.Form.IDN = UsIn.IDN;
                    model.Form.BIRTHDAY = UsIn.BIRTHDAY;
                    model.Form.SEX_CD = UsIn.SEX_CD;
                    // 電話
                    var Tel = UsIn.TEL.Split('-');
                    model.Form.H_TEL_0 = Tel[0];
                    if (Tel.ToCount() > 1)
                    {
                        model.Form.H_TEL_1 = Tel[1];
                        if (model.Form.H_TEL_1.IndexOf('#') > 0)
                        {
                            model.Form.H_TEL_1 = Tel[1].Substring(0, Tel[1].IndexOf('#'));
                            model.Form.H_TEL_2 = Tel[1].Split('#')[1];
                        }
                    }
                    // 電話
                    model.Form.W_TEL_0 = Tel[0];
                    if (Tel.ToCount() > 1)
                    {
                        model.Form.W_TEL_1 = Tel[1];
                        if (model.Form.W_TEL_1.IndexOf('#') > 0)
                        {
                            model.Form.W_TEL_1 = Tel[1].Substring(0, Tel[1].IndexOf('#'));
                            model.Form.W_TEL_2 = Tel[1].Split('#')[1];
                        }
                    }
                    model.Form.C_ZIPCODE = UsIn.CITY_CD;
                    model.Form.C_ADDR = UsIn.ADDR;
                    model.Form.MOBILE = UsIn.MOBILE;
                    model.Form.EMAIL = UsIn.MAIL;
                    model.Form.MERGEYN = "N";
                    model.Form.H_EQUAL = "N";
                }
                else
                {
                    rtn = RedirectToAction("Index", "Login");
                }
            }
            return rtn;
        }

        /// <summary>
        /// 導預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_011006FormModel model)
        {
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            string ErrorMsg = "";
            if (string.IsNullOrWhiteSpace(model.H_TEL_0) && string.IsNullOrWhiteSpace(model.H_TEL_1)
              && string.IsNullOrWhiteSpace(model.W_TEL_0) && string.IsNullOrWhiteSpace(model.W_TEL_1)
              && string.IsNullOrWhiteSpace(model.MOBILE))
            {
                ModelState.AddModelError("TEL_MOBILE", "電話(公)、電話(宅)、行動電話請擇一填寫");
            }
            if (!string.IsNullOrWhiteSpace(model.H_TEL_0) || !string.IsNullOrWhiteSpace(model.H_TEL_1))
            {
                if (string.IsNullOrWhiteSpace(model.H_TEL_0) || string.IsNullOrWhiteSpace(model.H_TEL_1))
                {
                    ModelState.AddModelError("H_TEL_0", "電話(公)請填寫完整");
                }
            }
            if (!string.IsNullOrWhiteSpace(model.W_TEL_0) || !string.IsNullOrWhiteSpace(model.W_TEL_1))
            {
                if (string.IsNullOrWhiteSpace(model.W_TEL_0) || string.IsNullOrWhiteSpace(model.W_TEL_1))
                {
                    ModelState.AddModelError("W_TEL_0", "電話(宅)請填寫完整");
                }
            }
            if (string.IsNullOrEmpty(model.SEX_CD))
            {
                ModelState.AddModelError("SEX_CD", "性別為必填欄位");
            }

            if (string.IsNullOrEmpty(model.EMAIL_0))
            {
                ModelState.AddModelError("EMAIL_0", "E-MAIL帳號名稱為必填欄位");
            }

            if (string.IsNullOrEmpty(model.EMAIL_2))
            {
                ModelState.AddModelError("EMAIL_2", "E-MAIL帳號網域為必填欄位");
            }
            if (!string.IsNullOrWhiteSpace(model.EMAIL_0) && !string.IsNullOrWhiteSpace(model.EMAIL_2))
            {
                var mail = $"{model.EMAIL_0}@{model.EMAIL_2}";
                if (!reg3.IsMatch(mail))
                {
                    ModelState.AddModelError("EMAIL_2", "請填入正確的Email格式");
                }
            }
            // 合併檔案
            if (model.MERGEYN == "Y")
            {
                if (string.IsNullOrWhiteSpace(model.FILE_IDNF_TEXT) && string.IsNullOrWhiteSpace(model.FILE_IDNB_TEXT)
                    && string.IsNullOrWhiteSpace(model.FILE_PHOTO_TEXT) && string.IsNullOrWhiteSpace(model.FILE_HOUSEHOLD_TEXT))
                {
                    ModelState.AddModelError("FILE_IDNF", "請至少上傳一個檔案");
                }
            }
            else
            {
                // 必傳檔案
                if (model.FILE_IDNF_TEXT == null)
                {
                    ModelState.AddModelError("FILE_IDNF", "請上傳身分證正面影本");
                }
                if (model.FILE_IDNB_TEXT == null)
                {
                    ModelState.AddModelError("FILE_IDNB", "請上傳身分證反面影本");
                }
                if (model.FILE_PHOTO_TEXT == null)
                {
                    ModelState.AddModelError("FILE_PHOTO", "請上傳照片(規格應同護照照片)");
                }
                if (model.APPLY_TYPE == "3" && model.FILE_HOUSEHOLD_TEXT == null)
                {
                    // 更名 要上傳戶口名簿
                    ModelState.AddModelError("FILE_HOUSEHOLD", "請上傳戶籍謄本或戶口名簿影本");
                }
            }


            if (!string.IsNullOrEmpty(model.FILE_IDNF_TEXT))
            {
                logger.Debug("Apply_011006.FileName:" + model.FILE_IDNF_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                // 取得副檔名並轉小寫
                var ext = model.FILE_IDNF_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            if (!string.IsNullOrEmpty(model.FILE_IDNB_TEXT))
            {
                logger.Debug("Apply_011006.FileName:" + model.FILE_IDNB_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                // 取得副檔名並轉小寫
                var ext = model.FILE_IDNB_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            if (!string.IsNullOrEmpty(model.FILE_PHOTO_TEXT))
            {
                logger.Debug("Apply_011006.FileName:" + model.FILE_PHOTO_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                // 取得副檔名並轉小寫
                var ext = model.FILE_PHOTO_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            if (!string.IsNullOrEmpty(model.FILE_HOUSEHOLD_TEXT))
            {
                logger.Debug("Apply_011006.FileName:" + model.FILE_HOUSEHOLD_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                // 取得副檔名並轉小寫
                var ext = model.FILE_HOUSEHOLD_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }

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
        public ActionResult PreView(Apply_011006FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();

            return PartialView("PreView011006", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011006FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            ShareDAO Sdao = new ShareDAO();
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            var APP_ID = dao.GetApp_ID("011006");
            // 存檔
            dao.AppendApply011006(model, APP_ID);
            // 寄信
            dao.SendMail_New(memberName, memberEmail, APP_ID, "專科社會工作師證書換發（更名或污損）", "011006", ISSEND: true);

            return Done("1");
        }
        #endregion 新增申辦案件

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
            Apply_011006AppDocModel model = new Apply_011006AppDocModel();
            //Apply_011006DetailModel detail = new Apply_011006DetailModel();

            // 案件基本資訊
            Apply_011006Model app_011006 = new Apply_011006Model();
            app_011006.APP_ID = APP_ID;
            var app_011006data = dao.GetRow(app_011006);

            ApplyModel app = new ApplyModel();
            app.APP_ID = APP_ID;
            app.SRV_ID = "011006";
            var appdata = dao.GetRow(app);

            try
            {
                var UsIn = sm.UserInfo.Member;

                // 判斷是否為該案件申請人
                if (appdata.ACC_NO == UsIn.ACC_NO)
                {
                    // 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                    model = dao.GetFile_011006(APP_ID);
                    // 取回案件資料(可依個人方式決定帶值回來的方式)
                    #region 帶入帳號資訊
                    // 補件狀態
                    model.APPSTATUS = appdata.FLOW_CD.TONotNullString() == "2" ? "1" : "0";
                    model.FLOW_CD = appdata.FLOW_CD;
                    model.MAILBODY = appdata.MAILBODY;
                    // 申請日期
                    model.APP_TIME = appdata.APP_TIME;
                    model.APP_TIME_SHOW = HelperUtil.DateTimeToTwString(appdata.APP_TIME);
                    // 帳號
                    model.ACC_NO = appdata.ACC_NO;
                    //申請用途
                    model.APPLY_TYPE = app_011006data.APPLY_TYPE;
                    //專科類別
                    model.SPECIALIST_TYPE = app_011006data.SPECIALIST_TYPE;
                    // 姓名
                    model.NAME = appdata.NAME;
                    //出生年月日
                    model.BIRTHDAY = appdata.BIRTHDAY;
                    //性別
                    model.SEX_CD = appdata.SEX_CD;
                    //身分證字號
                    model.IDN = appdata.IDN;
                    // 電話
                    model.W_TEL = app_011006data.W_TEL;
                    if (!string.IsNullOrEmpty(app_011006data.W_TEL))
                    {
                        model.W_TEL_0 = app_011006data.W_TEL.ToSplit('-').FirstOrDefault();
                        model.W_TEL_1 = app_011006data.W_TEL.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                        if (model.W_TEL.IndexOf('#') > 0)
                        {
                            model.W_TEL_2 = app_011006data.W_TEL.ToSplit('#').LastOrDefault();
                        }
                    }
                    model.H_TEL = app_011006data.H_TEL;
                    if (!string.IsNullOrEmpty(app_011006data.H_TEL))
                    {
                        model.H_TEL_0 = app_011006data.H_TEL.ToSplit('-').FirstOrDefault();
                        model.H_TEL_1 = app_011006data.H_TEL.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                        if (model.H_TEL.IndexOf('#') > 0)
                        {
                            model.H_TEL_2 = app_011006data.H_TEL.ToSplit('#').LastOrDefault();
                        }
                    }
                    //手機
                    model.MOBILE = appdata.MOBILE;
                    // 地址
                    model.C_ZIPCODE = app_011006data.C_ZIPCODE;
                    model.C_ADDR = app_011006data.C_ADDR;
                    model.H_ZIPCODE = app_011006data.H_ZIPCODE;
                    model.H_ADDR = app_011006data.H_ADDR;
                    model.H_EQUAL = app_011006data.H_EQUAL;
                    // 行動
                    model.MOBILE = appdata.MOBILE;
                    // Mail
                    model.EMAIL = app_011006data.EMAIL;
                    // 執業處所
                    model.PRACTICE_PLACE = app_011006data.PRACTICE_PLACE;
                    //考試年度
                    model.TEST_YEAR = app_011006data.TEST_YEAR;
                    //合併上傳
                    model.MERGEYN = app_011006data.MERGEYN;
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
        public ActionResult SaveAppDoc(Apply_011006AppDocModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            #region 檢核
            string ErrorMsg = "";
            if (string.IsNullOrWhiteSpace(model.H_TEL_0) && string.IsNullOrWhiteSpace(model.H_TEL_1)
                && string.IsNullOrWhiteSpace(model.W_TEL_0) && string.IsNullOrWhiteSpace(model.W_TEL_1)
                && string.IsNullOrWhiteSpace(model.MOBILE))
            {
                ModelState.AddModelError("TEL_MOBILE", "電話(公)、電話(宅)、行動電話請擇一填寫");
            }
            if (!string.IsNullOrWhiteSpace(model.H_TEL_0) || !string.IsNullOrWhiteSpace(model.H_TEL_1))
            {
                if (string.IsNullOrWhiteSpace(model.H_TEL_0) || string.IsNullOrWhiteSpace(model.H_TEL_1))
                {
                    ModelState.AddModelError("H_TEL_0", "電話(公)請填寫完整");
                }
            }
            if (!string.IsNullOrWhiteSpace(model.W_TEL_0) || !string.IsNullOrWhiteSpace(model.W_TEL_1))
            {
                if (string.IsNullOrWhiteSpace(model.W_TEL_0) || string.IsNullOrWhiteSpace(model.W_TEL_1))
                {
                    ModelState.AddModelError("W_TEL_0", "電話(宅)請填寫完整");
                }
            }
            if (string.IsNullOrEmpty(model.SEX_CD))
            {
                ModelState.AddModelError("SEX_CD", "性別為必填欄位");
            }

            if (string.IsNullOrEmpty(model.EMAIL_0))
            {
                ModelState.AddModelError("EMAIL_0", "E-MAIL帳號名稱為必填欄位");
            }

            if (string.IsNullOrEmpty(model.EMAIL_2))
            {
                ModelState.AddModelError("EMAIL_2", "E-MAIL帳號網域為必填欄位");
            }

            System.Text.RegularExpressions.Regex mailReg = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");

            if (!mailReg.IsMatch(model.EMAIL_0 + "@" + model.EMAIL_2))
            {
                ModelState.AddModelError("EMAIL", "請輸入正確的E-MAIL格式 !");
            }
            #endregion

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 存檔
                var count = dao.UpdateApply011006(model);
                // 寄信
                dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "專科社會工作師證書換發（更名或污損）", "011006", count, ISSEND: true);

                return Done("2", count);
            }
            else
            {
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg = ErrorMsg + item.Errors[0].ErrorMessage + "\n";
                    }
                }
                sm.LastErrorMessage = ErrorMsg;
            }
            return View("AppDoc", model);         
        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Done(string status, string Count = "")
        {
            SessionModel sm = SessionModel.Get();
            Apply_011006DoneModel model = new Apply_011006DoneModel();
            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }

        #endregion

        #region 申請單套表
        //[HttpPost]
        //[DisplayName("011006_申請單套表")]
        //public void PreviewApplyForm(Apply_011006FormModel Form)
        //{
        //    //□ ■
        //    string path = Server.MapPath("~/Sample/apply011006.docx");
        //    byte[] buffer = null;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (DocX doc = DocX.Load(path))
        //        {
        //            //申請日期
        //            doc.ReplaceText("$APP_TIME_TW$", "中華民國" + Form.APP_TIME_SHOW.Split('/')[0] + "年" + Form.APP_TIME_SHOW.Split('/')[1] + "月" + Form.APP_TIME_SHOW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);

        //            //申請類別
        //            switch (Form.APPLY_TYPE)
        //            {
        //                case "1":
        //                    doc.ReplaceText("$APPLY_TYPE1$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                case "2":
        //                    doc.ReplaceText("$APPLY_TYPE1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE2$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                case "3":
        //                    doc.ReplaceText("$APPLY_TYPE1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE3$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                default:
        //                    doc.ReplaceText("$APPLY_TYPE1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$APPLY_TYPE3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //            }

        //            //專科類別
        //            switch (Form.SPECIALIST_TYPE)
        //            {
        //                case "1":
        //                    doc.ReplaceText("$SPECIALIST1$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST4$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST5$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                case "2":
        //                    doc.ReplaceText("$SPECIALIST1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST2$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST4$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST5$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                case "3":
        //                    doc.ReplaceText("$SPECIALIST1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST3$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST4$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST5$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                case "4":
        //                    doc.ReplaceText("$SPECIALIST1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST4$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST5$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                case "5":
        //                    doc.ReplaceText("$SPECIALIST1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST4$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST5$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                default:
        //                    doc.ReplaceText("$SPECIALIST1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST3$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST4$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SPECIALIST5$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //            }

        //            //姓名
        //            doc.ReplaceText("$NAME$", Form.NAME, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //出生年月日
        //            doc.ReplaceText("$BIRTHDAY_TW$", Form.BIRTHDAY_AD_TW, false, System.Text.RegularExpressions.RegexOptions.None);
        //            //身分證字號
        //            doc.ReplaceText("$IDN$", Form.IDN, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //電話
        //            doc.ReplaceText("$W_TEL$", Form.W_TEL, false, System.Text.RegularExpressions.RegexOptions.None);
        //            doc.ReplaceText("$H_TEL$", Form.H_TEL, false, System.Text.RegularExpressions.RegexOptions.None);
        //            //手機
        //            doc.ReplaceText("$MOBILE$", Form.MOBILE, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //通訊地址
        //            doc.ReplaceText("$C_ZIPCODE$", Form.C_ZIPCODE, false, System.Text.RegularExpressions.RegexOptions.None);
        //            doc.ReplaceText("$C_ADDR$", Form.C_ADDR, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //戶籍地址
        //            doc.ReplaceText("$H_ZIPCODE$", Form.H_ZIPCODE, false, System.Text.RegularExpressions.RegexOptions.None);
        //            doc.ReplaceText("$H_ADDR$", Form.H_ADDR, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //EMAIL
        //            doc.ReplaceText("$EMAIL$", Form.EMAIL, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //執業處所
        //            doc.ReplaceText("$PRACTICE_PLACE$", Form.PRACTICE_PLACE, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //考試年度
        //            doc.ReplaceText("$TEST_YEAR$", Form.TEST_YEAR, false, System.Text.RegularExpressions.RegexOptions.None);

        //            doc.SaveAs(ms);
        //        }
        //        buffer = ms.ToArray();
        //    }

        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    Response.ContentType = "Application/msword";
        //    Response.AddHeader("Content-Disposition", "attachment;   filename=專科社會工作師核發申請書.doc");
        //    Response.BinaryWrite(buffer);
        //    Response.OutputStream.Flush();
        //    Response.OutputStream.Close();
        //    Response.Flush();
        //    Response.End();
        //}
        #endregion
    }
}
