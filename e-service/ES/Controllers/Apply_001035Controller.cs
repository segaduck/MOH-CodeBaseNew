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

namespace ES.Controllers
{
    public class Apply_001035Controller : BaseController
    {
        public static string s_SRV_ID = "001035";
        public static string s_SRV_NAME = "非感染性人體器官、組織及細胞進出口";

        #region 新增申辦案件

        [HttpGet]
        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt001035");
        }

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_001035FormModel model = new Apply_001035FormModel();
            ActionResult rtn = View("Index", model);
            model.ADD_TIME = DateTime.Now;
            var UsIn = sm.UserInfo.Member;
            model.IM_EXPORT = "0";
            model.APP_TIME = DateTime.Now;
            if (UsIn != null)
            {
                #region 帶入帳號資訊
                ShareDAO dao = new ShareDAO();
                model.ACC_NO = UsIn.ACC_NO;
                model.SRV_ID = "001035";
                model.SRC_SRV_ID = "001035";
                model.UNIT_CD = dao.GetServiceUnitCD(model.SRV_ID);
                model.NAME = UsIn.NAME;
                model.IDN = UsIn.IDN;

                model.TAX_ORG_MAN = !string.IsNullOrWhiteSpace(UsIn.CNT_NAME)? UsIn.CNT_NAME : UsIn.NAME;
                model.TAX_ORG_TEL = UsIn.TEL;
                model.TAX_ORG_EMAIL = UsIn.MAIL;
                model.TAX_ORG_FAX = UsIn.FAX;

                model.TAX_ORG_ID = UsIn.IDN;
                model.TAX_ORG_NAME = UsIn.NAME;
                model.TAX_ORG_ENAME = UsIn.ENAME;
                #endregion
            }

            else rtn = RedirectToAction("Index", "Login");

            ////agree: 1:同意新增 /other:請先閱讀規章
            //if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            //if (agree != null && !agree.Equals("1")) { return Prompt(); }

            return rtn;
        }

        /// <summary>
        /// 預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_001035FormModel model)
        {
            // 由登入資訊取得當前角色的檢定類別資訊
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            ActionResult rtn = View(model);
            var errormsg = string.Empty;
            if (model.IM_EXPORT == "0")
            {
                foreach (var item in ModelState)
                {
                    if (item.Key.ToLeft(5) == "Sell_")
                    {
                        item.Value.Errors.Clear();
                    }
                }
            }
            if (model.IM_EXPORT == "1")
            {
                foreach (var item in ModelState)
                {
                    if (item.Key.ToLeft(5) == "Dest_")
                    {
                        item.Value.Errors.Clear();
                    }
                }
            }
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                errormsg = model.FileSave();
                errormsg += dao.Check001035(model);
                if (errormsg != "")
                {
                    sm.LastErrorMessage = errormsg;
                }
                else
                {
                    if (model.IsMode == "1")
                    {
                        model.IsNew = false;
                        model.GOODS.IsReadOnly = true;
                    }
                    if (model.IsMode == "0")
                    {
                        model.IsNew = true;
                        model.GOODS.IsReadOnly = false;
                    }
                }
            }
            else
            {
                errormsg = errormsg + model.FileSave();
                errormsg = errormsg + dao.Check001035(model);
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        errormsg = errormsg + item.Errors[0].ErrorMessage + "\n";
                    }
                }               
                sm.LastErrorMessage = errormsg;
            }
            rtn = View("Index", model);
            return rtn;
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_001035FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.TAX_ORG_EMAIL) ? sm.UserInfo.Member.MAIL : model.TAX_ORG_EMAIL;
            ModelState.Clear();
            // 存檔
            dao.AppendApply001035(model);
            // 寄信

            dao.SendMail_New(memberName, memberEmail, model.APP_ID, "非感染性人體器官、組織及細胞進出口", "001035");


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
            Apply_001035FormModel model = new Apply_001035FormModel(APP_ID);

            try
            {
                var UsIn = sm.UserInfo.Member;

                // 案件基本資訊
                ApplyModel aly = new ApplyModel();
                aly.APP_ID = APP_ID;
                var alydata = dao.GetRow(aly);
                model.SRV_ID = "001035";
                model.APP_TIME = alydata.APP_TIME;
                model.NAME = alydata.NAME;
                model.IDN = alydata.IDN;

                // 判斷是否為該案件申請人
                if (alydata.ACC_NO == UsIn.ACC_NO)
                {
                    // 基本資料
                    Apply_001035Model app = new Apply_001035Model();
                    app.APP_ID = APP_ID;
                    var appdata = dao.GetRow(app);

                    // 納稅義務人資料
                    model.TAX_ORG_ID = appdata.TAX_ORG_ID;
                    model.TAX_ORG_NAME = appdata.TAX_ORG_NAME;
                    model.TAX_ORG_ENAME = appdata.TAX_ORG_ENAME;
                    //model.TAX_ORG_ZIP = appdata.TAX_ORG_ZIP; 沒有TAX_ORG_ZIP這個欄位
                    model.TAX_ORG_ADDR = appdata.TAX_ORG_ADDR;
                    model.TAX_ORG_EADDR = appdata.TAX_ORG_EADDR;
                    model.TAX_ORG_MAN = appdata.TAX_ORG_MAN;
                    model.TAX_ORG_TEL = appdata.TAX_ORG_TEL;
                    model.TAX_ORG_EMAIL = appdata.TAX_ORG_EMAIL;
                    model.TAX_ORG_FAX = appdata.TAX_ORG_FAX;
                    model.IM_EXPORT = appdata.IM_EXPORT;
                    model.DATE_S = appdata.DATE_S;
                    model.DATE_E = appdata.DATE_E;
                    if (model.IM_EXPORT == "0")
                    {
                        model.Dest_DEST_STATE_ID = appdata.DEST_STATE_ID;
                        model.Dest_SELL_STATE_ID = appdata.SELL_STATE_ID;
                        model.Dest_BEG_COUNTRY_ID = appdata.BEG_COUNTRY_ID;
                        model.Dest_BEG_PORT_ID = appdata.BEG_PORT_ID;
                        model.Dest_SELL_NAME = appdata.SELL_NAME;
                        model.Dest_SELL_ADDR = appdata.SELL_ADDR;
                    }
                    if (model.IM_EXPORT == "1")
                    {
                        model.Sell_DEST_STATE_ID = appdata.DEST_STATE_ID;
                        model.Sell_SELL_STATE_ID = appdata.SELL_STATE_ID;
                        model.Sell_TRN_COUNTRY_ID = appdata.TRN_COUNTRY_ID;
                        model.Sell_TRN_PORT_ID = appdata.TRN_PORT_ID;
                        model.Sell_BEG_COUNTRY_ID = appdata.BEG_COUNTRY_ID;
                        model.Sell_BEG_PORT_ID = appdata.BEG_PORT_ID;
                        model.Sell_SELL_NAME = appdata.SELL_NAME;
                        model.Sell_SELL_ADDR = appdata.SELL_ADDR;
                    }



                    // 其他資料
                    model.APP_USE_ID = appdata.APP_USE_ID;
                    model.CONF_TYPE_ID = appdata.CONF_TYPE_ID;
                    for (var k = 1; k <= 23; k++)
                    {
                        var kNo = k.TONotNullString().PadLeft(2, '0');
                        // 檢附文件類型
                        var MOD_DOC_TYP = model.GetType().GetProperties().Where(m => m.Name == "DOC_TYP_" + kNo);
                        var APP_DOC_TYP = appdata.GetType().GetProperties().Where(m => m.Name == "DOC_TYP_" + kNo);
                        if (MOD_DOC_TYP.ToCount() > 0 && APP_DOC_TYP.ToCount() > 0)
                        {
                            var ModObj = MOD_DOC_TYP.FirstOrDefault();
                            var AppObj = APP_DOC_TYP.FirstOrDefault();
                            var AppVal = AppObj.GetValue(appdata);
                            ModObj.SetValue(model, AppVal);
                        }
                        // 檢附文件字號
                        var MOD_DOC_COD = model.GetType().GetProperties().Where(m => m.Name == "DOC_COD_" + kNo);
                        var APP_DOC_COD = appdata.GetType().GetProperties().Where(m => m.Name == "DOC_COD_" + kNo);
                        if (MOD_DOC_COD.ToCount() > 0 && APP_DOC_COD.ToCount() > 0)
                        {
                            var ModObj = MOD_DOC_COD.FirstOrDefault();
                            var AppObj = APP_DOC_COD.FirstOrDefault();
                            var AppVal = AppObj.GetValue(appdata);
                            ModObj.SetValue(model, AppVal);
                        }
                        // 檢附文件說明
                        var MOD_DOC_TXT = model.GetType().GetProperties().Where(m => m.Name == "DOC_TXT_" + kNo);
                        var APP_DOC_TXT = appdata.GetType().GetProperties().Where(m => m.Name == "DOC_TXT_" + kNo);
                        if (MOD_DOC_TXT.ToCount() > 0 && APP_DOC_TXT.ToCount() > 0)
                        {
                            var ModObj = MOD_DOC_TXT.FirstOrDefault();
                            var AppObj = APP_DOC_TXT.FirstOrDefault();
                            var AppVal = AppObj.GetValue(appdata);
                            ModObj.SetValue(model, AppVal);
                        }
                    }

                    // 檔案
                    for (var k = 1; k <= 23; k++)
                    {
                        Apply_FileModel fileWhere = new Apply_FileModel();
                        fileWhere.APP_ID = APP_ID;
                        fileWhere.FILE_NO = k;
                        var filedata = dao.GetRow(fileWhere);


                        var kNo = k.TONotNullString().PadLeft(2, '0');
                        var fileNo = model.GetType().GetProperties().Where(m => m.Name == "DOC_FILE_" + kNo + "_FILENAME");
                        if (fileNo.ToCount() > 0)
                        {
                            var fileObj = fileNo.FirstOrDefault();
                            fileObj.SetValue(model, filedata.FILENAME);
                        }
                    }

                    model.IsNew = false;
                    model.GOODS.IsReadOnly = true;

                    return View("Detail", model);

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
        #endregion


        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Done(string status, string Count = "")
        {
            SessionModel sm = SessionModel.Get();
            Apply_001035DoneModel model = new Apply_001035DoneModel();

            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }
    }
}
