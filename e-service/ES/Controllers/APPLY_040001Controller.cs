using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using ES.Utils;
using NPOI.SS.Formula.Functions;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Controllers
{
    public class APPLY_040001Controller : BaseController
    {
        public static string s_SRV_ID = "040001";
        public static string s_SRV_NAME = "衛生福利部訴願案件";

        #region 新增申辦案件
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt");
        }

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_040001FormModel model = new Apply_040001FormModel();
            ActionResult rtn = View("Index", model);
            var UsIn = sm.UserInfo.Member;

            if (UsIn != null)
            {
                #region 帶入帳號資訊
                //帳號
                model.ACC_NO = UsIn.ACC_NO;
                // 電話
                model.H_TEL = UsIn.TEL;
                model.TEL = UsIn.TEL;
                model.MOBILE = UsIn.MOBILE;
                // 地址
                model.C_ZIPCODE = UsIn.CITY_CD;
                model.C_ADDR = UsIn.ADDR;
                //姓名
                model.NAME = UsIn.NAME;
                model.ENAME = UsIn.ENAME;
                //Mail
                model.EMAIL = UsIn.MAIL;
                //出生年月日
                model.IDN = UsIn.IDN;
                model.BIRTHDAY = UsIn.BIRTHDAY;
                model.BIRTHDAY_STR = HelperUtil.DateTimeToString(UsIn.BIRTHDAY);
                model.SEX_CD = UsIn.SEX_CD;
                model.APP_DATE = HelperUtil.DateTimeToTwString(DateTime.Now);
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
        public ActionResult Apply(Apply_040001FormModel model)
        {
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = dao.ChkApply040001(model);
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
        public ActionResult PreView(Apply_040001FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            return PartialView("PreView", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_040001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var app_id = string.Empty;
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            // 存檔
            app_id = dao.AppendApply040001(model);
            // 寄信
            dao.SendMail_New(memberName, memberEmail, app_id, s_SRV_NAME, s_SRV_ID);

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
            Apply_040001AppDocModel model = new Apply_040001AppDocModel();

            // 案件基本資訊
            TblAPPLY_040001 app = new TblAPPLY_040001();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            try
            {
                var UsIn = sm.UserInfo.Member;
                //取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                model = dao.GetFile_040001(APP_ID);
                model.SRVLIST = new List<Apply_040001SRVLSTModel>();
                if (model.FILE.Count > 0)
                {
                    foreach (var item in model.FILE)
                    {
                        var insert_data = new Apply_040001SRVLSTModel();
                        insert_data.InjectFrom(item);
                        insert_data.FILE_1_TEXT = item.SRC_FILENAME;
                        insert_data.FILE_NO = item.FILE_NO;
                        insert_data.SRC_NO = item.SRC_NO;
                        model.SRVLIST.Add(insert_data);
                    }
                }
                #region 處理完資料帶出顯示
                model.InjectFrom(appdata);
                model.InjectFrom(alydata);

                model.SEX_CD = alydata.SEX_CD;
                model.BIRTHDAY_STR = HelperUtil.DateTimeToString(alydata.BIRTHDAY);
                model.APP_DATE = HelperUtil.TransToTwYear(alydata.ADD_TIME);
                model.EMAIL = appdata.EMAIL;
                model.MOBILE = alydata.MOBILE;
                model.H_TEL = alydata.CNT_TEL;
                model.CHR_BIRTH_STR = HelperUtil.DateTimeToString(appdata.CHR_BIRTH);
                model.R_BIRTH_STR = HelperUtil.DateTimeToString(appdata.R_BIRTH);
                model.ORG_DATE = HelperUtil.DateTimeToString(appdata.ORG_DATE);

                // 訴願人通訊地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = alydata.ADDR_CODE;
                var zipdata = dao.GetRow(zip);
                model.C_ZIPCODE = alydata.ADDR_CODE;
                model.C_ZIPCODE_TEXT = zipdata.CITYNM + zipdata.TOWNNM;
                model.C_ADDR = alydata.ADDR;

                // 代表人通訊地址
                TblZIPCODE zipc = new TblZIPCODE();
                zipc.ZIP_CO = appdata.CHR_ADDR_CODE;
                var zipdatac = dao.GetRow(zipc);
                model.CHR_ZIPCODE = appdata.CHR_ADDR_CODE;
                model.CHR_ZIPCODE_TEXT = zipdatac.CITYNM + zipdatac.TOWNNM;
                model.CHR_ADDR = appdata.CHR_ADDR;

                // 代理人通訊地址
                TblZIPCODE zipr = new TblZIPCODE();
                zipr.ZIP_CO = appdata.R_ADDR_CODE;
                var zipdatar = dao.GetRow(zipr);
                model.R_ZIPCODE = appdata.R_ADDR_CODE;
                model.R_ZIPCODE_TEXT = zipdatar.CITYNM + zipdatar.TOWNNM;
                model.R_ADDR = appdata.R_ADDR;

                #endregion

                return View("AppDoc", model);
            }
            catch (Exception ex)
            {
                sm.LastErrorMessage = ex.Message;
                logger.Error(ex.Message, ex);
                return RedirectToAction("Index", "History");
            }
        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Done(string status, string Count = "")
        {
            SessionModel sm = SessionModel.Get();
            Apply_040001DoneModel model = new Apply_040001DoneModel();
            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }

        #endregion

        #region 套印申請書
        /// <summary>
        /// 套印申請書
        /// </summary>
        public void PrintDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            string path = Server.MapPath("~/Sample/apply040001.docx");
            var filesStr = string.Empty;
            byte[] buffer = null;
            Apply_040001AppDocModel model = new Apply_040001AppDocModel();
            model.APP_ID = APP_ID;

            // 案件基本資訊
            TblAPPLY_040001 app = new TblAPPLY_040001();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            try
            {
                //取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                model = dao.GetFile_040001(APP_ID);
                model.SRVLIST = new List<Apply_040001SRVLSTModel>();
                if (model.FILE.Count > 0)
                {
                    foreach (var item in model.FILE)
                    {
                        filesStr += item.SRC_FILENAME + ",";
                    }
                }
                #region 處理完資料帶出顯示
                model.InjectFrom(appdata);
                model.InjectFrom(alydata);

                model.BIRTHDAY_STR = HelperUtil.DateTimeToTwString(alydata.BIRTHDAY);
                model.APP_DATE = HelperUtil.TransToTwYear(alydata.ADD_TIME);
                model.CHR_BIRTH_STR = HelperUtil.DateTimeToTwString(appdata.CHR_BIRTH);
                model.R_BIRTH_STR = HelperUtil.DateTimeToTwString(appdata.R_BIRTH);
                model.ORG_DATE = HelperUtil.DateTimeToTwString(appdata.ORG_DATE);

                // 訴願人通訊地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = alydata.ADDR_CODE;
                var zipdata = dao.GetRow(zip);
                model.C_ZIPCODE = alydata.ADDR_CODE;
                model.C_ZIPCODE_TEXT = zipdata.CITYNM + zipdata.TOWNNM;
                model.C_ADDR = alydata.ADDR;

                // 代表人通訊地址
                TblZIPCODE zipc = new TblZIPCODE();
                zipc.ZIP_CO = appdata.CHR_ADDR_CODE;
                var zipdatac = dao.GetRow(zipc);
                model.CHR_ZIPCODE = appdata.CHR_ADDR_CODE;
                model.CHR_ZIPCODE_TEXT = zipdatac.CITYNM + zipdatac.TOWNNM;
                model.CHR_ADDR = appdata.CHR_ADDR;

                // 代理人通訊地址
                TblZIPCODE zipr = new TblZIPCODE();
                zipr.ZIP_CO = appdata.R_ADDR_CODE;
                var zipdatar = dao.GetRow(zipr);
                model.R_ZIPCODE = appdata.R_ADDR_CODE;
                model.R_ZIPCODE_TEXT = zipdatar.CITYNM + zipdatar.TOWNNM;
                model.R_ADDR = appdata.R_ADDR;

                #endregion

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //訴願人
                    doc.ReplaceText("$ANAME$", alydata.NAME.TONotNullString());
                    doc.ReplaceText("$ABIRTH$", model.BIRTHDAY_STR.TONotNullString());
                    doc.ReplaceText("$AIDN$", alydata.IDN.TONotNullString());
                    doc.ReplaceText("$AADDRCODE$", alydata.ADDR_CODE.TONotNullString() + " " + model.C_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$AADDR$", alydata.ADDR.TONotNullString());
                    doc.ReplaceText("$AMOBILE$", alydata.MOBILE.TONotNullString());
                    doc.ReplaceText("$ATEL$", alydata.CNT_TEL.TONotNullString());
                    //代表人
                    doc.ReplaceText("$CHRNAME$", appdata.CHR_NAME.TONotNullString());
                    doc.ReplaceText("$CHRBIRTH$", model.CHR_BIRTH_STR.TONotNullString());
                    doc.ReplaceText("$CHRIDN$", appdata.CHR_IDN.TONotNullString());
                    doc.ReplaceText("$CHRADDRCODE$", appdata.CHR_ADDR.TONotNullString() == "" ? "" : appdata.CHR_ADDR_CODE.TONotNullString() + " " + model.CHR_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$CHRADDR$", appdata.CHR_ADDR.TONotNullString());
                    doc.ReplaceText("$CHRMOBILE$", appdata.CHR_MOBILE.TONotNullString());
                    doc.ReplaceText("$CHRTEL$", appdata.CHR_TEL.TONotNullString());
                    //代理人
                    doc.ReplaceText("$RNAME$", appdata.R_NAME.TONotNullString());
                    doc.ReplaceText("$RBIRTH$", model.R_BIRTH_STR.TONotNullString());
                    doc.ReplaceText("$RIDN$", appdata.R_IDN.TONotNullString());
                    doc.ReplaceText("$RADDRCODE$", appdata.R_ADDR.TONotNullString() == "" ? "" : appdata.R_ADDR_CODE.TONotNullString() + " " + model.R_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$RADDR$", appdata.R_ADDR.TONotNullString());
                    doc.ReplaceText("$RMOBILE$", appdata.R_MOBILE.TONotNullString());
                    doc.ReplaceText("$RTEL$", appdata.R_TEL.TONotNullString());
                    //申請內容
                    doc.ReplaceText("$ORGNAME$", appdata.ORG_NAME.TONotNullString());
                    doc.ReplaceText("$ORGDATE$", model.ORG_DATE.TONotNullString());
                    doc.ReplaceText("$ORGMEMO$", appdata.ORG_MEMO.TONotNullString());
                    doc.ReplaceText("$ORGFACT$", appdata.ORG_FACT.TONotNullString());
                    doc.ReplaceText("$ORGNAME2$", appdata.ORG_NAME.TONotNullString());
                    doc.ReplaceText("$ANAME2$", alydata.NAME.TONotNullString());
                    doc.ReplaceText("$CHRNAME2$", appdata.CHR_NAME.TONotNullString());
                    doc.ReplaceText("$RNAME2$", appdata.R_NAME.TONotNullString());
                    doc.ReplaceText("$AYEAR$", model.APP_DATE.Split('/')[0]);
                    doc.ReplaceText("$AMONTH$", model.APP_DATE.Split('/')[1]);
                    doc.ReplaceText("$ADAY$", model.APP_DATE.Split('/')[2]);
                    doc.ReplaceText("$FILES$", filesStr.TONotNullString());
                    doc.ReplaceText("$AYEAR2$", model.APP_DATE.Split('/')[0]);
                    doc.ReplaceText("$AMONTH2$", model.APP_DATE.Split('/')[1]);
                    doc.ReplaceText("$ADAY2$", model.APP_DATE.Split('/')[2]);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=訴願申請書.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }
        #endregion
    }
}
