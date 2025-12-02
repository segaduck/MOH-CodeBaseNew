using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Controllers
{
    public class Apply_011008Controller : BaseController
    {

        public static string s_SRV_ID = "011008";
        public static string s_SRV_NAME = "社工師證書換發（更名或汙損）";
        //public static string s_doc_Name = "社會工作師證書申請書.doc";

        /// <summary>
        /// 顯示警語畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            Apply_011008ViewModel model = new Apply_011008ViewModel();
            //ActionResult rtn = View("Prompt", model.Form);
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt", model.Form);
        }


        #region 新增申辦案件
        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            Apply_011008ViewModel model = new Apply_011008ViewModel();
            ActionResult rtn = View("Index", model.Form);

            //agree: 1:同意新增 /other:請先閱讀規章
            if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            if (agree != null && !agree.Equals("1")) { return Prompt(); }

            //社工師證書換發（更名或汙損） （1:更名或2:汙損）
            model.Form.APPLY_TYPE = "1";
            model.Form.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(model.Form.APP_TIME == null ? DateTime.Now : model.Form.APP_TIME);
            model.Form.EMAIL = mem.MAIL;

            model.Form.NAME = mem.NAME;
            model.Form.IDN = mem.IDN;
            model.Form.BIRTHDAY = mem.BIRTHDAY;
            model.Form.SEX_CD = mem.SEX_CD;

            //if (!string.IsNullOrEmpty(mem.TEL))
            //{
            //    // 電話(公)
            //    model.Form.W_TEL = mem.TEL;
            //    model.Form.W_TEL_0 = mem.TEL.ToSplit('-').FirstOrDefault();
            //    model.Form.W_TEL_1 = mem.TEL.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
            //    model.Form.W_TEL_2 = (mem.TEL.Contains('#')) ? mem.TEL.ToSplit('-').LastOrDefault().ToSplit('#').LastOrDefault() : "";
            //    // 電話(宅)
            //    model.Form.H_TEL = mem.TEL;
            //    model.Form.H_TEL_0 = mem.TEL.ToSplit('-').FirstOrDefault();
            //    model.Form.H_TEL_1 = mem.TEL.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
            //    model.Form.H_TEL_2 = (mem.TEL.Contains('#')) ? mem.TEL.ToSplit('-').LastOrDefault().ToSplit('#').LastOrDefault() : "";
            //}

            model.Form.MOBILE = mem.MOBILE;

            model.Form.C_ZIPCODE = mem.CITY_CD;
            model.Form.C_ADDR = mem.ADDR;

            model.Form.MERGEYN = "N";
            model.Form.H_EQUAL = "N";

            return rtn;
        }

        /// <summary>
        /// 導預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_011008FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            AjaxResultStruct result = new AjaxResultStruct();
            //ApplyDAO dao = new ApplyDAO();

            string ErrorMsg = "";
            bool flag_EMAILNG = false;
            if (string.IsNullOrWhiteSpace(model.EMAIL_0)) { flag_EMAILNG = true; }
            if (string.IsNullOrWhiteSpace(model.EMAIL_1) && string.IsNullOrWhiteSpace(model.EMAIL_2)) { flag_EMAILNG = true; }
            if (model.EMAIL_1 != null && model.EMAIL_1.Equals("0") && string.IsNullOrWhiteSpace(model.EMAIL_2)) { flag_EMAILNG = true; }
            if (flag_EMAILNG)
            {
                ModelState.AddModelError("EMAIL_ALL", "E-MAIL 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.H_TEL_1) && string.IsNullOrWhiteSpace(model.W_TEL_1) && string.IsNullOrWhiteSpace(model.MOBILE))
            {
                ModelState.AddModelError("TEL_MOBILE", "電話(公)、電話(宅)、行動電話請擇一填寫");
            }
            if (string.IsNullOrEmpty(model.SEX_CD))
            {
                ModelState.AddModelError("SEX_CD", "性別為必填欄位");
            }
            if (string.IsNullOrEmpty(model.APPLY_TYPE))
            {
                ModelState.AddModelError("APPLY_TYPE", "申請類別為必選欄位");
            }
            if (model.APPLY_TYPE != null && model.APPLY_TYPE.Equals("1"))
            {
                if (string.IsNullOrEmpty(model.CHG_NAME))
                {
                    ModelState.AddModelError("CHR_NAME", "申請類別為更名 姓名(更正前) 必填欄位");
                }
                else if (model.CHG_NAME != null && model.NAME != null && model.CHG_NAME.Equals(model.NAME))
                {
                    ModelState.AddModelError("CHR_NAME", "申請類別為更名 姓名(更正前) 不可等同 姓名");
                }
            }
            if (!string.IsNullOrEmpty(model.FILE_HOUSEHOLD_TEXT))
            {
                logger.Debug("Apply_011008.FileName:" + model.FILE_HOUSEHOLD_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                // 取得副檔名並轉小寫
                var ext = model.FILE_HOUSEHOLD_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            if (!string.IsNullOrEmpty(model.FILE_IDNF_TEXT))
            {
                logger.Debug("Apply_011008.FileName:" + model.FILE_IDNF_TEXT);
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
                logger.Debug("Apply_011008.FileName:" + model.FILE_IDNB_TEXT);
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
                logger.Debug("Apply_011008.FileName:" + model.FILE_PHOTO_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                // 取得副檔名並轉小寫
                var ext = model.FILE_PHOTO_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            if (!string.IsNullOrEmpty(model.FILE_PASSCOPY_TEXT))
            {
                logger.Debug("Apply_011008.FileName:" + model.FILE_PASSCOPY_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                // 取得副檔名並轉小寫
                var ext = model.FILE_PASSCOPY_TEXT.ToSplit(".").LastOrDefault().ToLower();
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
                foreach (ModelState item in ModelState.Values)
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
        public ActionResult PreView(Apply_011008FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }
            //ApplyDAO dao = new ApplyDAO();

            return PartialView("PreView", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011008FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            ApplyDAO dao = new ApplyDAO();
            //ShareDAO Sdao = new ShareDAO();

            string APP_ID = dao.GetApp_ID(s_SRV_ID);// "011008");
            // 存檔
            dao.AppendApply011008(model, APP_ID);
            // 寄信
            dao.SendMail_New(mem.NAME, mem.MAIL, APP_ID, s_SRV_NAME, s_SRV_ID);

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
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            ApplyDAO dao = new ApplyDAO();
            Apply_011008AppDocModel model = new Apply_011008AppDocModel();
            //Apply_011008DetailModel detail = new Apply_011008DetailModel();

            // 案件基本資訊
            Apply_011008Model app_SN_Where = new Apply_011008Model();
            app_SN_Where.APP_ID = APP_ID;
            Apply_011008Model app_SN_data = dao.GetRow(app_SN_Where);

            ApplyModel app_Where = new ApplyModel();
            app_Where.APP_ID = APP_ID;
            app_Where.SRV_ID = s_SRV_ID;//"011002";
            ApplyModel app_data = dao.GetRow(app_Where);

            // 判斷是否為該案件申請人
            if (!mem.ACC_NO.Equals(app_data.ACC_NO))
            {
                sm.LastErrorMessage = "非案件申請人無法瀏覽次案件 !";// ex.Message;
                return RedirectToAction("Index", "History");
            }

            // 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
            model = dao.GetFile_011008(APP_ID);
            // 取回案件資料(可依個人方式決定帶值回來的方式)

            #region 帶入帳號資訊
            // 補件狀態-案件狀態 1 補件 0 非補件
            model.APPSTATUS = app_data.FLOW_CD.TONotNullString() == "2" ? "1" : "0";
            model.FLOW_CD = app_data.FLOW_CD;

            model.MAILBODY = app_data.MAILBODY;
            // 申請日期
            model.APP_TIME = app_data.APP_TIME;
            model.APP_TIME_TW = HelperUtil.DateTimeToTwString(app_data.APP_TIME);
            // 申請日期
            model.APPLY_DATE = app_SN_data.APPLY_DATE;
            model.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(app_SN_data.APPLY_DATE);
            //[申請類別]（1:更名或2:汙損）
            model.APPLY_TYPE = app_SN_data.APPLY_TYPE;
            // 帳號
            model.ACC_NO = app_data.ACC_NO;
            // 姓名
            model.NAME = app_data.NAME;
            // 更正姓名
            model.CHG_NAME = app_SN_data.CHG_NAME;
            // 出生年月日
            model.BIRTHDAY = app_data.BIRTHDAY;
            // 性別
            model.SEX_CD = app_data.SEX_CD;
            // 身分證字號
            model.IDN = app_data.IDN;
            // 電話
            model.W_TEL = app_SN_data.W_TEL;
            if (!string.IsNullOrEmpty(app_SN_data.W_TEL))
            {
                model.W_TEL_0 = app_SN_data.W_TEL.ToSplit('-').FirstOrDefault();
                model.W_TEL_1 = app_SN_data.W_TEL.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                model.W_TEL_2 = (app_SN_data.W_TEL.Contains('#')) ? app_SN_data.W_TEL.ToSplit('-').LastOrDefault().ToSplit('#').LastOrDefault() : "";
            }
            model.H_TEL = app_SN_data.H_TEL;
            if (!string.IsNullOrEmpty(app_SN_data.H_TEL))
            {
                model.H_TEL_0 = app_SN_data.H_TEL.ToSplit('-').FirstOrDefault();
                model.H_TEL_1 = app_SN_data.H_TEL.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                model.H_TEL_2 = (app_SN_data.H_TEL.Contains('#')) ? app_SN_data.H_TEL.ToSplit('-').LastOrDefault().ToSplit('#').LastOrDefault() : "";
            }
            // 行動手機
            // 手機
            model.MOBILE = app_SN_data.MOBILE;
            // 地址
            model.C_ZIPCODE = app_SN_data.C_ZIPCODE;
            model.C_ADDR = app_SN_data.C_ADDR;
            model.H_ZIPCODE = app_SN_data.H_ZIPCODE;
            model.H_ADDR = app_SN_data.H_ADDR;
            model.H_EQUAL = app_SN_data.H_EQUAL;
            // 行動
            //if (string.IsNullOrEmpty(model.MOBILE)) { model.MOBILE = app_data.MOBILE; }
            // Mail
            model.EMAIL = app_SN_data.EMAIL;
            //考試年度
            model.TEST_YEAR = app_SN_data.TEST_YEAR;
            //考試名稱類科
            model.TEST_CATEGORY = app_SN_data.TEST_CATEGORY;
            //合併上傳
            model.MERGEYN = app_SN_data.MERGEYN;
            #endregion

            // 取回補件備註欄位
            TblAPPLY_NOTICE ntwhere = new TblAPPLY_NOTICE();
            ntwhere.APP_ID = APP_ID;
            ntwhere.ISADDYN = "N";
            IList<TblAPPLY_NOTICE> ntdata = dao.GetRowList(ntwhere);

            // 無動態欄位
            List<string> ntLst = new List<string>();
            // 動態欄位(通常適用於檔案)
            //List<string> ntLstForList = new List<string>();
            foreach (TblAPPLY_NOTICE item in ntdata)
            {
                ntLst.Add(item.Field);
                //if (item.SRC_NO.TONotNullString().Equals("")) { ntLst.Add(item.Field); }
                //if (!item.SRC_NO.TONotNullString().Equals("")) { ntLstForList.Add(item.Field); }
            }
            // 組成字串丟回前端跑JS
            //FILE_0 FILE_1 FILE_2 FILE_3 FILE_4 ALL_5 OTHER_6
            model.FieldStr = string.Join(",", ntLst);

            return View("AppDoc", model);


        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_011008AppDocModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            var memName = string.IsNullOrWhiteSpace(model.NAME) ? mem.NAME : model.NAME;
            var memEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? mem.MAIL : model.EMAIL;

            ApplyDAO dao = new ApplyDAO();
            // 存檔
            var count = dao.UpdateApply011008(model);
            // 寄信
            dao.SendMail_Update(memName, memEmail, model.APP_ID, s_SRV_NAME, s_SRV_ID, count);

            return Done("2", count);

        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Done(string status, string Count = "")
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            Apply_011008DoneModel model = new Apply_011008DoneModel();
            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }

        #endregion

        //[HttpPost]
        //[DisplayName("011008_申請單套表")]
        //public void PreviewApplyForm(Apply_011008FormModel Form)
        //{
        //    //□ ■
        //    string path = Server.MapPath("~/Sample/apply011008.docx");
        //    byte[] buffer = null;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (DocX doc = DocX.Load(path))
        //        {
        //            //申請日期
        //            //doc.ReplaceText("$APP_TIME_TW$", "中華民國" + Form.APP_TIME_SHOW.Split('/')[0] + "年" + Form.APP_TIME_SHOW.Split('/')[1] + "月" + Form.APP_TIME_SHOW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);

        //            //性別
        //            switch (Form.SEX_CD)
        //            {
        //                case "M":
        //                    doc.ReplaceText("$SEX_TYPE1$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SEX_TYPE2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                case "F":
        //                    doc.ReplaceText("$SEX_TYPE1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SEX_TYPE2$", "■", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //                default:
        //                    doc.ReplaceText("$SEX_TYPE1$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    doc.ReplaceText("$SEX_TYPE2$", "□", false, System.Text.RegularExpressions.RegexOptions.None);
        //                    break;
        //            }

        //            //姓名
        //            doc.ReplaceText("$NAME$", Form.NAME, false, System.Text.RegularExpressions.RegexOptions.None);

        //            //出生年月日
        //            doc.ReplaceText("$BIRTHDAY_TW$", Form.BIRTHDAY_TW, false, System.Text.RegularExpressions.RegexOptions.None);
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
        //            //考試年度
        //            doc.ReplaceText("$TEST_YEAR$", Form.TEST_YEAR, false, System.Text.RegularExpressions.RegexOptions.None);
        //            //考試名稱類科
        //            doc.ReplaceText("$TEST_CATEGORY$", Form.TEST_CATEGORY, false, System.Text.RegularExpressions.RegexOptions.None);

        //            doc.SaveAs(ms);
        //        }
        //        buffer = ms.ToArray();
        //    }

        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    Response.ContentType = "Application/msword";
        //    Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", s_doc_Name));
        //    Response.BinaryWrite(buffer);
        //    Response.OutputStream.Flush();
        //    Response.OutputStream.Close();
        //    Response.Flush();
        //    Response.End();
        //}
    }
}
