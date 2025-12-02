using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Windows.Interop;
using Xceed.Words.NET;

namespace ES.Controllers
{
    public class Apply_011010Controller : BaseController
    {
        public static string s_SRV_ID = "011010";
        public static string s_SRV_NAME = "全國社會工作專業人員選拔推薦";
        /// <summary>
        /// 顯示警語畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Prompt()
        {
            ShareDAO dao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            Apply_011010ViewModel model = new Apply_011010ViewModel();
            //ActionResult rtn = View("Prompt", model.Form);
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);

            ViewBag.CanApply = "N"; //TODO

            var data = dao.GetRow<TblSERVICE_DATE>(new TblSERVICE_DATE { SRV_ID = "011010" });
            if (data != null)
            {
                if (DateTime.Now >= data.TIME_S && DateTime.Now < data.TIME_E.Value)
                {
                    ViewBag.CanApply = "Y";
                }
            }
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

            Apply_011010ViewModel model = new Apply_011010ViewModel();
            ActionResult rtn = View("Index", model.Form);

            //agree: 1:同意新增 /other:請先閱讀規章
            if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            if (agree != null && !agree.Equals("1")) { return Prompt(); }

            model.Form.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(model.Form.APP_TIME == null ? DateTime.Now : model.Form.APP_TIME);
            model.Form.EMAIL = mem.MAIL;
            model.Form.NAME = mem.NAME;
            model.Form.IDN = mem.IDN;
            model.Form.MOBILE = mem.MOBILE;
            // 單位資料
            model.Form.UNIT_TEL = mem.MOBILE;
            model.Form.UNIT_NAME = mem.NAME;
            model.Form.UNIT_TYPE = "1";

            model.Form.MERGEYN = "N";

            return rtn;
        }

        /// <summary>
        /// 導預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_011010FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            AjaxResultStruct result = new AjaxResultStruct();

            string ErrorMsg = "";
            bool flag_EMAILNG = false;

            #region 檢核
            if (string.IsNullOrWhiteSpace(model.EMAIL_0)) { flag_EMAILNG = true; }
            if (string.IsNullOrWhiteSpace(model.EMAIL_1) && string.IsNullOrWhiteSpace(model.EMAIL_2)) { flag_EMAILNG = true; }
            if (model.EMAIL_1 != null && model.EMAIL_1.Equals("0") && string.IsNullOrWhiteSpace(model.EMAIL_2)) { flag_EMAILNG = true; }
            if (flag_EMAILNG)
            {
                ModelState.AddModelError("EMAIL_ALL", "E-MAIL 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.UNIT_TYPE))
            {
                ModelState.AddModelError("UNIT_TYPE", "單位類型 為必填欄位");
            }
            else if (model.UNIT_TYPE == "2" && string.IsNullOrWhiteSpace(model.UNIT_SUBTYPE))
            {
                ModelState.AddModelError("UNIT_SUBTYPE", "單位類型(私部門) 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.UNIT_NAME))
            {
                ModelState.AddModelError("UNIT_NAME", "單位名稱 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.UNIT_DEPART))
            {
                ModelState.AddModelError("UNIT_DEPART", "單位連絡人局處/部門 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.UNIT_TITLE))
            {
                ModelState.AddModelError("UNIT_TITLE", "單位聯絡人職稱 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.UNIT_CNAME))
            {
                ModelState.AddModelError("UNIT_CNAME", "單位聯絡人姓名 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.UNIT_TEL))
            {
                ModelState.AddModelError("UNIT_TEL", "連絡電話 為必填欄位");
            }
            if (string.IsNullOrWhiteSpace(model.CNT_D))
            {
                 ModelState.AddModelError("CNT_D", "單位社工人員總數");
            }
            if (string.IsNullOrWhiteSpace(model.CNT_E))
            {
                ModelState.AddModelError("CNT_E", "績優社工獎推薦人數 為必填項目");
            }
            if (string.IsNullOrWhiteSpace(model.CNT_F))
            {
                ModelState.AddModelError("CNT_F", "績優社工督導獎推薦人數 為必填項目");
            }
            if (string.IsNullOrWhiteSpace(model.CNT_G))
            {
                ModelState.AddModelError("CNT_G", "資深敬業獎推薦人數 為必填項目");
            }
            if (string.IsNullOrWhiteSpace(model.CNT_H))
            {
                ModelState.AddModelError("CNT_H", "特殊貢獻獎推薦人數 為必填項目");
            }
            /*數量檢核*/
            if (!string.IsNullOrWhiteSpace(model.UNIT_TYPE))
            {
                //var sum_ab = Convert.ToInt32(model.CNT_A) + Convert.ToInt32(model.CNT_B);
                var sum_ef = Convert.ToInt32(model.CNT_E) + Convert.ToInt32(model.CNT_F);
                var sum_d = Convert.ToInt32(model.CNT_D);
                // 單位類型= 公部門或私部門
                if (model.UNIT_TYPE == "1" || model.UNIT_TYPE == "2")
                {
                    // 單位社工人員總數
                    if (sum_d <= 30)
                    {
                        if (sum_ef > 1)
                        {
                            ModelState.AddModelError("CNT_E", "績優社工獎推薦人數、績優社工督導獎推薦人數 加總不得超過1");
                        }
                    }
                    else if (sum_d > 30 && sum_d < 900)
                    {
                        if (sum_d % 30 == 0)
                        {
                            if (sum_ef > (sum_d / 30))
                            {
                                ModelState.AddModelError("CNT_E", "績優社工獎推薦人數、績優社工督導獎推薦人數 加總不得超過單位社工人員總數/30【無條件進位】");
                            }
                        }
                        else
                        {
                            if (sum_ef > (sum_d / 30) + 1)
                            {
                                ModelState.AddModelError("CNT_E", "績優社工獎推薦人數、績優社工督導獎推薦人數 加總不得超過單位社工人員總數/30【無條件進位】");
                            }
                        }
                    }
                    else if (sum_d >= 900)
                    {
                        if (sum_ef > 30)
                        {
                            ModelState.AddModelError("CNT_E", "績優社工獎推薦人數、績優社工督導獎推薦人數 加總不得超過30");
                        }
                    }
                }
                else if (model.UNIT_TYPE == "3")
                {
                    // 單位類型=醫事機構
                    // 單位社工人員總數
                    if (sum_d <= 15)
                    {
                        if (sum_ef > 1)
                        {
                            ModelState.AddModelError("CNT_E", "績優社工獎推薦人數、績優社工督導獎推薦人數 加總不得超過1");
                        }
                    }
                    else if (sum_d > 15)
                    {
                        if (sum_ef > 2)
                        {
                            ModelState.AddModelError("CNT_E", "績優社工獎推薦人數、績優社工督導獎推薦人數 加總不得超過2");
                        }
                    }
                }
            }
            //佐證文件檢核
            if (model.SRVLIST.Count < 1)
            {
                ModelState.AddModelError("FILE_3", "推薦表(PDF檔)至少上傳一個檔案");
            }
            else
            {
                //for (var i = 0; i < model.SRVLIST.Count; i++)
                //{
                //    if (string.IsNullOrEmpty(model.SRVLIST[i].FILE_3_TEXT))
                //    {
                //        ModelState.AddModelError("SRVLIST_" + i + "__FILE_3", "推薦表(PDF檔) 序號" + (i + 1) + " 為必填欄位");
                //    }
                //}
            }
            #endregion

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
        public ActionResult PreView(Apply_011010FormModel model)
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
        public ActionResult Save(Apply_011010FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            ApplyDAO dao = new ApplyDAO();
            //ShareDAO Sdao = new ShareDAO();

            string APP_ID = dao.GetApp_ID(s_SRV_ID);// "011010");
                                                    // 存檔
            dao.AppendApply011010(model, APP_ID);
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
            Apply_011010AppDocModel model = new Apply_011010AppDocModel();
            //Apply_011010DetailModel detail = new Apply_011010DetailModel();

            // 案件基本資訊
            Apply_011010Model app_SN_Where = new Apply_011010Model();
            app_SN_Where.APP_ID = APP_ID;
            Apply_011010Model app_SN_data = dao.GetRow(app_SN_Where);

            ApplyModel app_Where = new ApplyModel();
            app_Where.APP_ID = APP_ID;
            app_Where.SRV_ID = s_SRV_ID;
            ApplyModel app_data = dao.GetRow(app_Where);

            // 判斷是否為該案件申請人
            if (!mem.ACC_NO.Equals(app_data.ACC_NO))
            {
                sm.LastErrorMessage = "非案件申請人無法瀏覽次案件 !";// ex.Message;
                return RedirectToAction("Index", "History");
            }

            // 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
            model = dao.GetFile_011010(APP_ID);
            // 取回案件資料(可依個人方式決定帶值回來的方式)

            #region 帶入帳號資訊
            // 補件狀態-案件狀態 1 補件 0 非補件
            model.APPSTATUS = app_data.FLOW_CD.TONotNullString() == "2" ? "1" : "0";
            model.FLOW_CD = app_data.FLOW_CD;

            model.MAILBODY = app_data.MAILBODY;
            // 申請日期
            model.APP_TIME = app_data.APP_TIME;
            model.APP_TIME_TW = HelperUtil.DateTimeToTwString(app_data.APP_TIME);
            // 帳號
            model.ACC_NO = app_data.ACC_NO;
            // 姓名
            model.NAME = app_data.NAME;
            // 身分證字號
            model.IDN = app_data.IDN;
            // 手機
            model.MOBILE = app_SN_data.UNIT_TEL;
            // Mail
            model.EMAIL = app_SN_data.UNIT_EMAIL;
            //合併上傳
            model.MERGEYN = app_SN_data.MERGEYN;
            #endregion
            #region 帶入其他資料
            model.UNIT_TYPE = app_SN_data.UNIT_TYPE;
            model.UNIT_SUBTYPE = app_SN_data.UNIT_SUBTYPE;
            model.UNIT_NAME = app_SN_data.UNIT_NAME;
            model.UNIT_DEPART = app_SN_data.UNIT_DEPART;
            model.UNIT_TITLE = app_SN_data.UNIT_TITLE;
            model.UNIT_CNAME = app_SN_data.UNIT_CNAME;
            model.UNIT_TEL = app_SN_data.UNIT_TEL;
            model.CNT_TYPE = app_SN_data.CNT_TYPE;
            model.CNT_A = app_SN_data.CNT_A;
            model.CNT_B = app_SN_data.CNT_B;
            model.CNT_C = app_SN_data.CNT_C;
            model.CNT_D = app_SN_data.CNT_D;
            model.CNT_E = app_SN_data.CNT_E;
            model.CNT_F = app_SN_data.CNT_F;
            model.CNT_G = app_SN_data.CNT_G;
            model.CNT_H = app_SN_data.CNT_H;
            #endregion

            model.SRVLIST = new List<Apply_011010SRVLSTModel>();
            if (model.FILE.Count > 0)
            {
                foreach (var item in model.FILE)
                {
                    var insert_data = new Apply_011010SRVLSTModel();
                    insert_data.InjectFrom(item);
                    model.SRVLIST.Add(insert_data);
                }
            }

            // 取回補件備註欄位
            TblAPPLY_NOTICE ntwhere = new TblAPPLY_NOTICE();
            ntwhere.APP_ID = APP_ID;
            ntwhere.ISADDYN = "N";
            IList<TblAPPLY_NOTICE> ntdata = dao.GetRowList(ntwhere);

            // 無動態欄位
            List<string> ntLst = new List<string>();
            // 動態欄位(通常適用於檔案)
            List<string> ntLstForList = new List<string>();
            foreach (TblAPPLY_NOTICE item in ntdata)
            {
                ntLst.Add(item.Field);
                if (item.SRC_NO.TONotNullString().Equals("")) { ntLst.Add(item.Field); }
                if (!item.SRC_NO.TONotNullString().Equals("")) { ntLstForList.Add(item.Field); }
            }
            // 組成字串丟回前端跑JS
            //FILE_0 FILE_EXCEL FILE_PDF FILE_3 FILE_4 ALL_5 OTHER_6
            model.FieldStr = string.Join(",", ntLst);
            return View("AppDoc", model);
        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_011010AppDocModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            var memName = string.IsNullOrWhiteSpace(model.NAME) ? mem.NAME : model.NAME;
            var memEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? mem.MAIL : model.EMAIL;

            ApplyDAO dao = new ApplyDAO();
            // 存檔
            var count = dao.UpdateApply011010(model);
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

            Apply_011010DoneModel model = new Apply_011010DoneModel();
            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }
        #endregion
    }
}
