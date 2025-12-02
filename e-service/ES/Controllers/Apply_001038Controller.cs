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
    public class Apply_001038Controller : BaseController
    {
        public static string s_SRV_ID = "001038";
        public static string s_SRV_NAME = "生殖細胞及胚胎輸入輸出";

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
            return View("Prompt001038");
        }

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_001038FormModel model = new Apply_001038FormModel();
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
                model.SRV_ID = "001038";
                model.SRC_SRV_ID = "001038";
                model.UNIT_CD = dao.GetServiceUnitCD(model.SRV_ID);
                model.IDN = UsIn.IDN;
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
        public ActionResult Apply(Apply_001038FormModel model)
        {
            // 由登入資訊取得當前角色的檢定類別資訊
            SessionModel sm = SessionModel.Get();
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
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\'\:\：]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            System.Text.RegularExpressions.Regex reg4 = new System.Text.RegularExpressions.Regex(@"^[\u4e00-\u9fa5]+$");
            System.Text.RegularExpressions.Regex reg5 = new System.Text.RegularExpressions.Regex(@"^[0-9\-\#]+$");
            if (!string.IsNullOrEmpty(model.Dest_ORG_EMAIL))
            {
                if (!reg3.IsMatch(model.Dest_ORG_EMAIL))
                {
                    ModelState.AddModelError("Dest_ORG_EMAIL", "請填入正確的E-MAIL格式 !");
                }                
            }
            if (!string.IsNullOrEmpty(model.Sell_ORG_EMAIL))
            {
                if (!reg3.IsMatch(model.Sell_ORG_EMAIL))
                {
                    ModelState.AddModelError("Sell_ORG_EMAIL", "請填入正確的E-MAIL格式 !");
                }
            }            
            if (!string.IsNullOrEmpty(model.TAX_ORG_EMAIL))
            {
                if (!reg3.IsMatch(model.TAX_ORG_EMAIL))
                {
                    ModelState.AddModelError("TAX_ORG_EMAIL", "請填入正確的委託人E-MAIL格式 !");
                }                
            }
            if (!string.IsNullOrEmpty(model.DATE_S_AD)&& !string.IsNullOrEmpty(model.DATE_E_AD))
            {
                if (HelperUtil.TransToDateTime(model.DATE_S_AD)> HelperUtil.TransToDateTime(model.DATE_E_AD))
                {
                    ModelState.AddModelError("DATE_S_AD", "起始日期不可以大於終止日期 !");
                }   
            }
            #region 其他資料檢核

            if (string.IsNullOrEmpty(model.TAX_ORG_NAME)&& string.IsNullOrEmpty(model.TAX_ORG_ENAME))
            {
                ModelState.AddModelError("TAX_ORG_NAME", "委託人姓名中文及英文請擇一填寫 !");
            }


            //if (string.IsNullOrEmpty(model.TAX_ORG_ZIP) && string.IsNullOrEmpty(model.TAX_ORG_EADDR))
            //{
            //    ModelState.AddModelError("TAX_ORG_NAME", "委託人地址中文及英文請擇一填寫 !");
            //}
            //else if (string.IsNullOrEmpty(model.TAX_ORG_EADDR))
            //{
            //    if (string.IsNullOrEmpty(model.TAX_ORG_ZIP) || string.IsNullOrEmpty(model.TAX_ORG_ADDR))
            //    {
            //        ModelState.AddModelError("TAX_ORG_NAME", "委託人地址中文請填寫完整 !");
            //    }
            //}
            if (string.IsNullOrEmpty(model.TAX_ORG_ADDR))
            {
                ModelState.AddModelError("TAX_ORG_ADDR", "請填入委託人地址 !");
            }

            if (model.DOC_TYP_01_SHOW)
            {
                if (string.IsNullOrEmpty(model.DOC_COD_01))
                {
                    ModelState.AddModelError("DOC_COD_01", "請填入檢附文件字號一 !");
                }
                if (string.IsNullOrEmpty(model.DOC_TXT_01))
                {
                    ModelState.AddModelError("DOC_TXT_01", "請填入檢附文件說明一 !");
                }
            }
            if (model.DOC_TYP_02_SHOW)
            {
                if (string.IsNullOrEmpty(model.DOC_COD_02))
                {
                    ModelState.AddModelError("DOC_COD_02", "請填入檢附文件字號二 !");
                }
                if (string.IsNullOrEmpty(model.DOC_TXT_02))
                {
                    ModelState.AddModelError("DOC_TXT_02", "請填入檢附文件說明二 !");
                }
            }
            if (model.DOC_TYP_03_SHOW)
            {
                if (string.IsNullOrEmpty(model.DOC_COD_03))
                {
                    ModelState.AddModelError("DOC_COD_03", "請填入檢附文件字號三 !");
                }
                if (string.IsNullOrEmpty(model.DOC_TXT_03))
                {
                    ModelState.AddModelError("DOC_TXT_03", "請填入檢附文件說明三 !");
                }
            }
            if (model.DOC_TYP_04_SHOW)
            {
                if (string.IsNullOrEmpty(model.DOC_COD_04))
                {
                    ModelState.AddModelError("DOC_COD_04", "請填入檢附文件字號四 !");
                }
                if (string.IsNullOrEmpty(model.DOC_TXT_04))
                {
                    ModelState.AddModelError("DOC_TXT_04", "請填入檢附文件說明四 !");
                }
            }
            if (model.DOC_TYP_05_SHOW)
            {
                if (string.IsNullOrEmpty(model.DOC_COD_05))
                {
                    ModelState.AddModelError("DOC_COD_05", "請填入檢附文件字號五 !");
                }
                if (string.IsNullOrEmpty(model.DOC_TXT_05))
                {
                    ModelState.AddModelError("DOC_TXT_05", "請填入檢附文件說明五 !");
                }
            }
            if (model.DOC_TYP_06_SHOW)
            {
                if (string.IsNullOrEmpty(model.DOC_COD_06))
                {
                    ModelState.AddModelError("DOC_COD_06", "請填入檢附文件字號六 !");
                }
                if (string.IsNullOrEmpty(model.DOC_TXT_06))
                {
                    ModelState.AddModelError("DOC_TXT_06", "請填入檢附文件說明六 !");
                }
            }
            #region 20201210 JIRA 787

            //if (model.DOC_TYP_07_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_07))
            //    {
            //        ModelState.AddModelError("DOC_COD_07", "請填入檢附文件字號七 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_07))
            //    {
            //        ModelState.AddModelError("DOC_TXT_07", "請填入檢附文件說明七 !");
            //    }
            //}
            //if (model.DOC_TYP_08_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_08))
            //    {
            //        ModelState.AddModelError("DOC_COD_08", "請填入檢附文件字號八 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_08))
            //    {
            //        ModelState.AddModelError("DOC_TXT_08", "請填入檢附文件說明八 !");
            //    }
            //}
            //if (model.DOC_TYP_09_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_09))
            //    {
            //        ModelState.AddModelError("DOC_COD_09", "請填入檢附文件字號九 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_09))
            //    {
            //        ModelState.AddModelError("DOC_TXT_09", "請填入檢附文件說明九 !");
            //    }
            //}
            //if (model.DOC_TYP_10_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_10))
            //    {
            //        ModelState.AddModelError("DOC_COD_10", "請填入檢附文件字號十 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_10))
            //    {
            //        ModelState.AddModelError("DOC_TXT_10", "請填入檢附文件說明十 !");
            //    }
            //}
            //if (model.DOC_TYP_11_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_11))
            //    {
            //        ModelState.AddModelError("DOC_COD_11", "請填入檢附文件字號十一 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_11))
            //    {
            //        ModelState.AddModelError("DOC_TXT_11", "請填入檢附文件說明十一 !");
            //    }
            //}
            //if (model.DOC_TYP_12_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_12))
            //    {
            //        ModelState.AddModelError("DOC_COD_12", "請填入檢附文件字號十二 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_12))
            //    {
            //        ModelState.AddModelError("DOC_TXT_12", "請填入檢附文件說明十二 !");
            //    }
            //}
            //if (model.DOC_TYP_13_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_13))
            //    {
            //        ModelState.AddModelError("DOC_COD_13", "請填入檢附文件字號十三 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_13))
            //    {
            //        ModelState.AddModelError("DOC_TXT_13", "請填入檢附文件說明十三 !");
            //    }
            //}
            //if (model.DOC_TYP_14_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_14))
            //    {
            //        ModelState.AddModelError("DOC_COD_14", "請填入檢附文件字號十四 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_14))
            //    {
            //        ModelState.AddModelError("DOC_TXT_14", "請填入檢附文件說明十四 !");
            //    }
            //}
            //if (model.DOC_TYP_15_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_15))
            //    {
            //        ModelState.AddModelError("DOC_COD_15", "請填入檢附文件字號十五 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_15))
            //    {
            //        ModelState.AddModelError("DOC_TXT_15", "請填入檢附文件說明十五 !");
            //    }
            //}
            //if (model.DOC_TYP_16_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_16))
            //    {
            //        ModelState.AddModelError("DOC_COD_16", "請填入檢附文件字號十六 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_16))
            //    {
            //        ModelState.AddModelError("DOC_TXT_16", "請填入檢附文件說明十六 !");
            //    }
            //}
            //if (model.DOC_TYP_17_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_17))
            //    {
            //        ModelState.AddModelError("DOC_COD_17", "請填入檢附文件字號十七 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_17))
            //    {
            //        ModelState.AddModelError("DOC_TXT_17", "請填入檢附文件說明十七 !");
            //    }
            //}
            //if (model.DOC_TYP_18_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_18))
            //    {
            //        ModelState.AddModelError("DOC_COD_18", "請填入檢附文件字號十八 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_18))
            //    {
            //        ModelState.AddModelError("DOC_TXT_18", "請填入檢附文件說明十八 !");
            //    }
            //}
            //if (model.DOC_TYP_19_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_19))
            //    {
            //        ModelState.AddModelError("DOC_COD_19", "請填入檢附文件字號十九 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_19))
            //    {
            //        ModelState.AddModelError("DOC_TXT_19", "請填入檢附文件說明十九 !");
            //    }
            //}
            //if (model.DOC_TYP_20_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_20))
            //    {
            //        ModelState.AddModelError("DOC_COD_20", "請填入檢附文件字號二十 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_20))
            //    {
            //        ModelState.AddModelError("DOC_TXT_20", "請填入檢附文件說明二十 !");
            //    }
            //}
            //if (model.DOC_TYP_21_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_21))
            //    {
            //        ModelState.AddModelError("DOC_COD_21", "請填入檢附文件字號二十一 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_21))
            //    {
            //        ModelState.AddModelError("DOC_TXT_21", "請填入檢附文件說明二十一 !");
            //    }
            //}
            //if (model.DOC_TYP_22_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_22))
            //    {
            //        ModelState.AddModelError("DOC_COD_22", "請填入檢附文件字號二十二 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_22))
            //    {
            //        ModelState.AddModelError("DOC_TXT_22", "請填入檢附文件說明二十二 !");
            //    }
            //}
            //if (model.DOC_TYP_23_SHOW)
            //{
            //    if (string.IsNullOrEmpty(model.DOC_COD_23))
            //    {
            //        ModelState.AddModelError("DOC_COD_23", "請填入檢附文件字號二十三 !");
            //    }
            //    if (string.IsNullOrEmpty(model.DOC_TXT_23))
            //    {
            //        ModelState.AddModelError("DOC_TXT_23", "請填入檢附文件說明二十三 !");
            //    }
            //}

            #endregion

            #endregion 其他資料檢核   

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                errormsg = model.FileSave();
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
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        errormsg = errormsg + item.Errors[0].ErrorMessage + "\n";
                    }
                }
                errormsg = errormsg + model.FileSave();
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
        public ActionResult Save(Apply_001038FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.TAX_ORG_NAME) ? sm.UserInfo.Member.NAME : model.TAX_ORG_NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.TAX_ORG_EMAIL) ? sm.UserInfo.Member.MAIL : model.TAX_ORG_EMAIL;
            ModelState.Clear();
            // 存檔
            dao.AppendApply001038(model);
            // 寄信
            dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, "生殖細胞及胚胎輸入輸出", "001038");
            //dao.SendMail_New(memberName, memberEmail, model.APP_ID, "生殖細胞及胚胎輸入輸出申請作業", "001038");


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
            Apply_001038FormModel model = new Apply_001038FormModel(APP_ID);

            try
            {
                var UsIn = sm.UserInfo.Member;

                // 案件基本資訊
                ApplyModel aly = new ApplyModel();
                aly.APP_ID = APP_ID;
                var alydata = dao.GetRow(aly);
                model.SRV_ID = "001038";
                model.APP_TIME = alydata.APP_TIME;


                // 判斷是否為該案件申請人
                if (alydata.ACC_NO == UsIn.ACC_NO)
                {
                    // 基本資料
                    Apply_001038Model app = new Apply_001038Model();
                    app.APP_ID = APP_ID;
                    var appdata = dao.GetRow(app);
                    model.IM_EXPORT = appdata.IM_EXPORT;
                    // 委託人資料
                    model.TAX_ORG_ID = appdata.TAX_ORG_ID;
                    model.TAX_ORG_NAME = appdata.TAX_ORG_NAME;
                    model.TAX_ORG_TID = appdata.TAX_ORG_TID;
                    //model.TAX_ORG_ZIP = appdata.TAX_ORG_ZIP; 沒有TAX_ORG_ZIP這個欄位
                    model.TAX_ORG_ADDR = appdata.TAX_ORG_ADDR;
                    model.TAX_ORG_EADDR = appdata.TAX_ORG_EADDR;
                    model.TAX_ORG_MAN = appdata.TAX_ORG_MAN;
                    model.TAX_ORG_TEL = appdata.TAX_ORG_TEL;
                    model.TAX_ORG_EMAIL = appdata.TAX_ORG_EMAIL;
                    model.TAX_ORG_FAX = appdata.TAX_ORG_FAX;
                    model.DATE_S = appdata.DATE_S;
                    model.DATE_E = appdata.DATE_E;
                    model.TRN_COUNTRY_ID = appdata.TRN_COUNTRY_ID;
                    model.TRN_PORT_ID = appdata.TRN_PORT_ID;
                    model.BEG_COUNTRY_ID = appdata.BEG_COUNTRY_ID;
                    model.BEG_PORT_ID = appdata.BEG_PORT_ID;

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


                    // 輸入
                    Apply_001038_DEST dest = new Apply_001038_DEST();
                    dest.APP_ID = APP_ID;
                    var destdata = dao.GetRow(dest);
                    model.Dest_ORG_UNITNAME = destdata.ORG_UNITNAME;
                    model.Dest_DEST_STATE_ID = appdata.DEST_STATE_ID;
                    model.Dest_ORG_NAME = destdata.ORG_NAME;
                    model.Dest_ORG_TEL = destdata.ORG_TEL;
                    model.Dest_ORG_EMAIL = destdata.ORG_EMAIL;
                    model.Dest_TAI_UNITNAME = destdata.TAI_UNITNAME;
                    model.Dest_LIC_NUM = destdata.LIC_NUM;
                    model.Dest_TAI_NAME = destdata.TAI_NAME;
                    model.Dest_TAI_TEL = destdata.TAI_TEL;
                    model.Dest_TAI_ADDR = destdata.TAI_ADDR;
                    model.Dest_TAI_EMAIL = destdata.TAI_EMAIL;
                    model.Dest_USE_MARK = appdata.USE_MARK;
                    model.Dest_A_NAME = destdata.A_NAME;
                    model.Dest_A_NUM1 = destdata.A_NUM1;
                    model.Dest_A_DATE = destdata.A_DATE;
                    model.Dest_B_NAME = destdata.B_NAME;
                    model.Dest_B_NUM1 = destdata.B_NUM1;
                    model.Dest_B_NUM2 = destdata.B_NUM2;
                    model.Dest_B_DATE = destdata.B_DATE;
                    model.Dest_C_NAME1 = destdata.C_NAME1;
                    model.Dest_C_NAME2 = destdata.C_NAME2;
                    model.Dest_C_NUM1 = destdata.C_NUM1;
                    model.Dest_C_DATE = destdata.C_DATE;
                    model.Dest_C_DAY = destdata.C_DAY;

                    // 輸出
                    Apply_001038_SELL sell = new Apply_001038_SELL();
                    sell.APP_ID = APP_ID;
                    var selldata = dao.GetRow(sell);
                    model.Sell_ORG_UNITNAME = selldata.ORG_UNITNAME;
                    model.Sell_ORG_NAME = selldata.ORG_NAME;
                    model.Sell_ORG_LIC_NUM = selldata.ORG_LIC_NUM;
                    model.Sell_ORG_TEL = selldata.ORG_TEL;
                    model.Sell_ORG_EMAIL = selldata.ORG_EMAIL;
                    model.Sell_SELL_STATE_ID = appdata.SELL_STATE_ID;
                    model.Sell_OTH_UNITNAME = selldata.OTH_UNITNAME;
                    model.Sell_OTH_TEL = selldata.OTH_TEL;
                    model.Sell_OTH_EMAIL = selldata.OTH_EMAIL;
                    //model.Sell_OTH_ZIP = selldata.OTH_ZIP;
                    model.Sell_OTH_ADDR = selldata.OTH_ADDR;
                    model.Sell_USE_MARK = appdata.USE_MARK;
                    model.Sell_A_NAME = selldata.A_NAME;
                    model.Sell_A_NUM1 = selldata.A_NUM1;
                    model.Sell_A_DATE = selldata.A_DATE;
                    model.Sell_B_NAME = selldata.B_NAME;
                    model.Sell_B_NUM1 = selldata.B_NUM1;
                    model.Sell_B_NUM2 = selldata.B_NUM2;
                    model.Sell_B_DATE = selldata.B_DATE;
                    model.Sell_C_NAME1 = selldata.C_NAME1;
                    model.Sell_C_NAME2 = selldata.C_NAME2;
                    model.Sell_C_NUM1 = selldata.C_NUM1;
                    model.Sell_C_DATE = selldata.C_DATE;
                    model.Sell_C_DAY = selldata.C_DAY;


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
            Apply_001038DoneModel model = new Apply_001038DoneModel();

            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }
    }
}
