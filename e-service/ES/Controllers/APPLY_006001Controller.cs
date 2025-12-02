using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using NPOI.SS.Formula.Functions;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Controllers
{
    public class APPLY_006001Controller : BaseController
    {
        public static string s_SRV_ID = "006001";
        public static string s_SRV_NAME = "國民年金爭議審議線上申辦";

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
            Apply_006001FormModel model = new Apply_006001FormModel();
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
                model.BIRTHDAY_STR = HelperUtil.DateTimeToTwString(UsIn.BIRTHDAY);
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
        public ActionResult Apply(Apply_006001FormModel model)
        {
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = dao.ChkApply006001(model);
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
        public ActionResult PreView(Apply_006001FormModel model)
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
        public ActionResult Save(Apply_006001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var app_id = string.Empty;
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            // 存檔
            app_id = dao.AppendApply006001(model);
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
            Apply_006001AppDocModel model = new Apply_006001AppDocModel();

            // 案件基本資訊
            TblAPPLY_006001 app = new TblAPPLY_006001();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            try
            {
                var UsIn = sm.UserInfo.Member;
                //取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                model = dao.GetFile_006001(APP_ID);
                model.SRVLIST = new List<Apply_006001SRVLSTModel>();
                if (model.FILE.Count > 0)
                {
                    foreach (var item in model.FILE)
                    {
                        var insert_data = new Apply_006001SRVLSTModel();
                        insert_data.InjectFrom(item);
                        insert_data.FILE_2_TEXT = item.SRC_FILENAME;
                        insert_data.FILE_NO = item.FILE_NO;
                        insert_data.SRC_NO = item.SRC_NO;
                        model.SRVLIST.Add(insert_data);
                    }
                }
                #region 處理完資料帶出顯示
                model.InjectFrom(appdata);
                model.InjectFrom(alydata);

                model.APP_DATE = HelperUtil.DateTimeToString(alydata.ADD_TIME);
                model.NAME = alydata.NAME;
                model.BIRTHDAY_STR = HelperUtil.DateTimeToTwString(alydata.BIRTHDAY);
                model.IDN = alydata.IDN;
                model.EMAIL = appdata.EMAIL;
                model.MOBILE = alydata.MOBILE;
                model.H_TEL = alydata.CNT_TEL;

                // 申請人通訊地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = alydata.ADDR_CODE;
                var zipdata = dao.GetRow(zip);
                model.C_ZIPCODE = alydata.ADDR_CODE;
                model.C_ZIPCODE_TEXT = zipdata.CITYNM + zipdata.TOWNNM;
                model.C_ADDR = alydata.ADDR;

                // 被保險人通訊地址
                TblZIPCODE zipr = new TblZIPCODE();
                zipr.ZIP_CO = appdata.R_ADDR_CODE;
                var zipdatar = dao.GetRow(zipr);
                model.R_ZIPCODE = appdata.R_ADDR_CODE;
                model.R_ZIPCODE_TEXT = zipdatar.CITYNM + zipdatar.TOWNNM;
                model.R_ADDR = appdata.R_ADDR;

                // 申請說明
                model.ISSAME = appdata.ISSAME;
                model.ISSAME_CHK = appdata.ISSAME == "Y" ? true : false;
                model.R_NAME = appdata.R_NAME;
                model.R_BIRTH_STR = HelperUtil.DateTimeToString(appdata.R_BIRTH);
                model.R_IDN = appdata.R_IDN;
                model.R_TEL = appdata.R_TEL;
                model.R_MOBILE = appdata.R_MOBILE;
                model.KINDTYPE = appdata.KINDTYPE;
                model.KIND1_CHK = appdata.KINDTYPE.Contains("1") ? true : false;
                model.KIND2_CHK = appdata.KINDTYPE.Contains("2") ? true : false;
                model.LIC_DATE_STR = HelperUtil.DateTimeToString(appdata.LIC_DATE);
                model.LIC_CD = appdata.LIC_CD;
                model.LIC_NUM = appdata.LIC_NUM;
                model.PAY_YEAR = appdata.PAY_YEAR;
                model.PAY_MONTH = appdata.PAY_MONTH;
                model.PAY_NUM = appdata.PAY_NUM;
                model.KNOW_DATE_STR = HelperUtil.DateTimeToString(appdata.KNOW_DATE);
                model.KNOW_MEMO = appdata.KNOW_MEMO;
                model.KNOW_FACT = appdata.KNOW_FACT;
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
            Apply_006001DoneModel model = new Apply_006001DoneModel();
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
            string path = Server.MapPath("~/Sample/apply006001.docx");
            var filesStr = string.Empty;
            byte[] buffer = null;
            Apply_006001AppDocModel model = new Apply_006001AppDocModel();
            model.APP_ID = APP_ID;

            // 案件基本資訊
            TblAPPLY_006001 app = new TblAPPLY_006001();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            try
            {
                //取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                model = dao.GetFile_006001(APP_ID);
                model.SRVLIST = new List<Apply_006001SRVLSTModel>();
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

                model.SEX_CD = alydata.SEX_CD;
                model.APP_DATE = HelperUtil.TransToTwYear(alydata.ADD_TIME);
                model.BIRTHDAY_STR = HelperUtil.TransToTwYear(alydata.BIRTHDAY);
                if (appdata.R_BIRTH != null)
                {
                    model.R_BIRTH_STR = HelperUtil.TransToTwYear(appdata.R_BIRTH);
                }
                else
                {
                    model.R_BIRTH_STR = string.Empty;
                }
                if (appdata.KNOW_DATE != null)
                {
                    model.KNOW_DATE_STR = HelperUtil.TransToTwYear(appdata.KNOW_DATE);
                }
                else
                {
                    model.KNOW_DATE_STR = string.Empty;
                }
                if (appdata.LIC_DATE != null)
                {
                    model.LIC_DATE_STR = HelperUtil.TransToTwYear(appdata.LIC_DATE);
                }
                else
                {
                    model.LIC_DATE_STR = string.Empty;
                }

                // 訴願人通訊地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = alydata.ADDR_CODE;
                var zipdata = dao.GetRow(zip);
                model.C_ZIPCODE = alydata.ADDR_CODE;
                model.C_ZIPCODE_TEXT = zipdata.CITYNM + zipdata.TOWNNM;
                model.C_ADDR = alydata.ADDR;

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
                    //申請人
                    doc.ReplaceText("$ANAME$", alydata.NAME.TONotNullString());
                    doc.ReplaceText("$BYEAR$", model.BIRTHDAY_STR.Split('/')[0]);
                    doc.ReplaceText("$BMONTH$", model.BIRTHDAY_STR.Split('/')[1]);
                    doc.ReplaceText("$BDAY$", model.BIRTHDAY_STR.Split('/')[2]);
                    doc.ReplaceText("$AIDN$", alydata.IDN.TONotNullString());
                    doc.ReplaceText("$AADDRCODE$", alydata.ADDR_CODE.TONotNullString() + " " + model.C_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$AADDR$", alydata.ADDR.TONotNullString());
                    doc.ReplaceText("$AMOBILE$", alydata.MOBILE.TONotNullString());
                    doc.ReplaceText("$ATEL$", alydata.CNT_TEL.TONotNullString());
                    //代理人
                    doc.ReplaceText("$ISSAME$", appdata.ISSAME == "Y" ? "█" : "□");
                    doc.ReplaceText("$RNAME$", appdata.R_NAME.TONotNullString());
                    if (!string.IsNullOrEmpty(model.R_BIRTH_STR))
                    {
                        doc.ReplaceText("$RYEAR$", model.R_BIRTH_STR.Split('/')[0]);
                        doc.ReplaceText("$RMONTH$", model.R_BIRTH_STR.Split('/')[1]);
                        doc.ReplaceText("$RDAY$", model.R_BIRTH_STR.Split('/')[2]);
                    }
                    else
                    {
                        doc.ReplaceText("$RYEAR$", "");
                        doc.ReplaceText("$RMONTH$", "");
                        doc.ReplaceText("$RDAY$", "");
                    }
                    doc.ReplaceText("$RIDN$", appdata.R_IDN.TONotNullString());
                    doc.ReplaceText("$RADDRCODE$", appdata.R_ADDR.TONotNullString() == "" ? "" : appdata.R_ADDR_CODE.TONotNullString() + " " + model.R_ZIPCODE_TEXT.TONotNullString());
                    doc.ReplaceText("$RADDR$", appdata.R_ADDR.TONotNullString());
                    doc.ReplaceText("$RMOBILE$", appdata.R_MOBILE.TONotNullString());
                    doc.ReplaceText("$RTEL$", appdata.R_TEL.TONotNullString());
                    //申請內容
                    doc.ReplaceText("$KIND1$", appdata.KINDTYPE.Contains("1") ? "█" : "□");

                    if (appdata.KINDTYPE.Contains("1"))
                    {
                        doc.ReplaceText("$LYEAR$", model.LIC_DATE_STR.Split('/')[0]);
                        doc.ReplaceText("$LMONTH$", model.LIC_DATE_STR.Split('/')[1]);
                        doc.ReplaceText("$LDAY$", model.LIC_DATE_STR.Split('/')[2]);
                        doc.ReplaceText("$LICCD$", appdata.LIC_CD.TONotNullString());
                        doc.ReplaceText("$LICNUM$", appdata.LIC_NUM.TONotNullString());
                    }
                    else
                    {
                        doc.ReplaceText("$LYEAR$", "");
                        doc.ReplaceText("$LMONTH$", "");
                        doc.ReplaceText("$LDAY$", "");
                        doc.ReplaceText("$LICCD$", "");
                        doc.ReplaceText("$LICNUM$", "");
                    }

                    doc.ReplaceText("$KIND2$", appdata.KINDTYPE.Contains("2") ? "█" : "□");
                    if (appdata.KINDTYPE.Contains("2"))
                    {
                        doc.ReplaceText("$PYEAR$", appdata.PAY_YEAR.TONotNullString());
                        doc.ReplaceText("$PMONTH$", appdata.PAY_MONTH.TONotNullString());
                        doc.ReplaceText("$PNUM$", appdata.PAY_NUM.TONotNullString());
                    }
                    else
                    {
                        doc.ReplaceText("$PYEAR$", "");
                        doc.ReplaceText("$PMONTH$", "");
                        doc.ReplaceText("$PNUM$", "");
                    }
                    if (appdata.KNOW_DATE != null)
                    {
                        doc.ReplaceText("$KYEAR$", model.KNOW_DATE_STR.Split('/')[0]);
                        doc.ReplaceText("$KMONTH$", model.KNOW_DATE_STR.Split('/')[1]);
                        doc.ReplaceText("$KDAY$", model.KNOW_DATE_STR.Split('/')[2]);
                    }
                    else
                    {
                        doc.ReplaceText("$KYEAR$", "");
                        doc.ReplaceText("$KMONTH$", "");
                        doc.ReplaceText("$KDAY$", "");
                    }

                    doc.ReplaceText("$KNOWMEMO$", appdata.KNOW_MEMO.TONotNullString());
                    doc.ReplaceText("$KNOWFACT$", appdata.KNOW_FACT.TONotNullString());
                    doc.ReplaceText("$FILES$", filesStr.TONotNullString());
                    doc.ReplaceText("$AYEAR$", model.APP_DATE.Split('/')[0]);
                    doc.ReplaceText("$AMONTH$", model.APP_DATE.Split('/')[1]);
                    doc.ReplaceText("$ADAY$", model.APP_DATE.Split('/')[2]);
                    doc.ReplaceText("$ANAME2$", alydata.NAME.TONotNullString());

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=國民年金爭議審議線上申辦書.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }
        #endregion
    }
}
