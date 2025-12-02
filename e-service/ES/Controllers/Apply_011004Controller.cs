using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using Omu.ValueInjecter;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ES.Controllers
{
    public class Apply_011004Controller : BaseController
    {
        public static string s_SRV_ID = "011004";
        public static string s_SRV_NAME = "社工師證書核發(英文)";

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
            return View("Prompt011004");
        }

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_011004FormModel model = new Apply_011004FormModel();
            ActionResult rtn = View("Index", model);
            var UsIn = sm.UserInfo.Member;

            if (UsIn != null)
            {
                #region 帶入帳號資訊
                //帳號
                model.ACC_NO = UsIn.ACC_NO;
                // 電話
                //model.C_TEL = UsIn.TEL;
                // 地址
                model.C_ZIPCODE = UsIn.CITY_CD;
                model.C_ADDR = UsIn.ADDR;
                //姓名
                model.NAME = UsIn.NAME;
                model.ENAME = UsIn.ENAME;
                //行動
                model.MOBILE = UsIn.MOBILE;
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
        public ActionResult Apply(Apply_011004FormModel model)
        {
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = dao.ChkApply011004(model);
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^([A-Z]+)(,)(\s+)([A-Z]+)(-)([A-Z]+)$");
                if (!reg.IsMatch(model.ENAME))
                {
                    ErrorMsg = "英文姓名必須全大寫英文，有「, 」及「-」";
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
        public ActionResult PreView(Apply_011004FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            return PartialView("PreView011004", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011004FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var app_id = string.Empty;
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            // 存檔
            app_id = dao.AppendApply011004(model);
            // 寄信
            dao.SendMail_New(memberName, memberEmail, app_id, "社工師證書核發（英文）", "011004");

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
            Apply_011004AppDocModel model = new Apply_011004AppDocModel();

            // 案件基本資訊
            TblAPPLY_011004 app = new TblAPPLY_011004();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            try
            {
                var UsIn = sm.UserInfo.Member;
                // 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                model = dao.GetFile_011004(APP_ID);

                #region 處理完資料帶出顯示
                model.InjectFrom(appdata);
                model.InjectFrom(alydata);

                model.SEX_CD = alydata.SEX_CD;
                model.BIRTHDAY_STR = HelperUtil.DateTimeToString(alydata.BIRTHDAY);
                model.APP_DATE = HelperUtil.TransToTwYear(alydata.ADD_TIME);
                model.EMAIL = appdata.EMAIL;
                model.C_TEL = appdata.C_TEL;
                model.H_TEL = appdata.H_TEL;

                // 通訊地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = appdata.C_ZIP;
                var zipdata = dao.GetRow(zip);
                // 地址
                model.C_ZIPCODE = appdata.C_ZIP;
                model.C_ZIPCODE_TEXT = zipdata.CITYNM + zipdata.TOWNNM;
                model.C_ADDR = appdata.C_ADDR;

                #endregion

                if (alydata != null && alydata.FLOW_CD == "2")
                {
                    // 取回補件備註欄位 開放修改狀態
                    TblAPPLY_NOTICE ntwhere = new TblAPPLY_NOTICE();
                    ntwhere.APP_ID = APP_ID;
                    ntwhere.ISADDYN = "N";
                    var ntdata = dao.GetRowList(ntwhere).ToList();
                    if (ntdata != null && ntdata.Count > 0)
                    {
                        var ngItem = string.Empty;
                        foreach (var item in ntdata)
                        {
                            ngItem += item.Field + ",";
                        }
                        model.NG_ITEM = ngItem.Remove(ngItem.Length - 1);
                    }
                }
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
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_011004AppDocModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            // 存檔
            var count = dao.UpdateApply011004(model);
            // 寄信
            dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "社工師證書核發（英文）", "011004", count);

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
            Apply_011004DoneModel model = new Apply_011004DoneModel();
            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }

        #endregion 
    }
}
