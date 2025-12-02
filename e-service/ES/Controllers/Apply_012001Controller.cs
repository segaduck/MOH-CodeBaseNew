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
using ES.Utils;

namespace ES.Controllers
{
    public class Apply_012001Controller : BaseController
    {
        #region 新增申辦案件

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply()
        {
            SessionModel sm = SessionModel.Get();
            Apply_012001FormModel model = new Apply_012001FormModel();
            ActionResult rtn = View("Index", model);
            model.APP_TIME = DateTime.Now;
            var UsIn = sm.UserInfo.Member;
            model.A_AGENT = "0";
            model.APP_ROLE = "0";
            if (UsIn != null)
            {
                #region 帶入帳號資訊
                ShareDAO dao = new ShareDAO();
                model.ACC_NO = UsIn.ACC_NO;
                model.SRV_ID = "012001";
                model.SRC_SRV_ID = "012001";
                model.UNIT_CD = dao.GetServiceUnitCD(model.SRV_ID);
                model.IDN = UsIn.IDN;
                model.NAME = UsIn.NAME;
                model.ADDR = UsIn.CITY_CD;
                model.ADDR_DETAIL = UsIn.ADDR;
                model.TEL = UsIn.TEL;
                model.MAIL = UsIn.MAIL;
                model.BIRTHDAY = UsIn.BIRTHDAY;
                #endregion
            }

            else rtn = RedirectToAction("Index", "Login");

            return rtn;
        }

        /// <summary>
        /// 預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_012001FormModel model)
        {
            // 由登入資訊取得當前角色的檢定類別資訊
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View(model);
            var errormsg = string.Empty;
            if (model.APP_ROLE == "0")
            {
                // 自辦時將代理人欄位檢核拔除
                foreach (var item in ModelState)
                {
                    if (item.Key.ToLeft(5) == "LPIN_")
                    {
                        item.Value.Errors.Clear();
                    }
                }
                foreach (var item in ModelState)
                {
                    if (item.Key.ToLeft(5) == "NPIN_")
                    {
                        item.Value.Errors.Clear();
                    }
                }
            }
            else
            {
                // 代辦時判斷自然人或法人
                if (model.A_AGENT == "0")
                {
                    foreach (var item in ModelState)
                    {
                        if (item.Key.ToLeft(5) == "LPIN_")
                        {
                            item.Value.Errors.Clear();
                        }
                    }
                }
                if (model.A_AGENT == "1")
                {
                    foreach (var item in ModelState)
                    {
                        if (item.Key.ToLeft(5) == "NPIN_")
                        {
                            item.Value.Errors.Clear();
                        }
                    }
                }
            }
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                errormsg = model.FileSave();
                errormsg += model.APPFILSave();
                errormsg += model.EMAILSave();
                if (errormsg != "")
                {
                    sm.LastErrorMessage = errormsg;
                }
                else
                {
                    if (model.IsMode == "1")
                    {
                        model.IsNew = false;
                        model.APPFIL.IsReadOnly = true;
                    }
                    if (model.IsMode == "0")
                    {
                        model.IsNew = true;
                        model.APPFIL.IsReadOnly = false;
                    }
                }
            }
            else
            {
                //ModelState.Clear();
                errormsg = model.FileSave();
                errormsg += model.APPFILSave();
                errormsg += model.EMAILSave();
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
        public ActionResult Save(Apply_012001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;
            ModelState.Clear();
            // 存檔
            dao.AppendApply012001(model);
            // 寄信
            dao.SendMail_New(memberName, memberEmail, model.APP_ID, "檔案應用", "012001");


            return Done("1");
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
            Apply_012001DoneModel model = new Apply_012001DoneModel();

            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }

        /// <summary>
        /// 補件畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_012001DocModel model = new Apply_012001DocModel(APP_ID);

            // 案件基本資訊
            TblAPPLY_012001 app = new TblAPPLY_012001();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            Apply_FileModel applyFile = new Apply_FileModel();
            applyFile.APP_ID = APP_ID;
            var applyFileData = dao.GetRowList(applyFile);

            TblAPPLY_012001_CHK applyChk = new TblAPPLY_012001_CHK();
            applyChk.APP_ID = APP_ID;
            var applyChkData = dao.GetRowList(applyChk);

            try
            {
                var UsIn = sm.UserInfo.Member;

                // 判斷是否為該案件申請人
                if (alydata.ACC_NO == UsIn.ACC_NO)
                {
                    #region 案件內容

                    model.ADD_TIME = appdata.ADD_TIME?.ToString("yyyy/MM/dd");
                    model.APP_ROLE = appdata.APP_ROLE;
                    model.NAME = alydata.NAME;
                    model.BIRTHDAY = alydata.BIRTHDAY;
                    model.IDN = alydata.IDN;

                    // 地址
                    model.TAX_ORG_CITY_CODE = alydata.ADDR_CODE;
                    model.TAX_ORG_CITY_DETAIL = alydata.ADDR;
                    TblZIPCODE zip = new TblZIPCODE();
                    zip.ZIP_CO = alydata.ADDR_CODE;
                    var getnam = dao.GetRow(zip);
                    if (getnam != null)
                    {
                        model.TAX_ORG_CITY_DETAIL = alydata.ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                        model.TAX_ORG_CITY_TEXT = getnam.CITYNM + getnam.TOWNNM;
                    }

                    getnam.CITYNM = null;

                    model.TEL = alydata.TEL;
                    model.MAIL = appdata.MAIL;
                    model.A_AGENT = appdata.A_AGENT;
                    //model.APP_REASON = appdata.APP_REASON;
                    foreach (var item in applyChkData)
                    {
                        if (item.TYPE == "0")
                        {
                            model.CHECK_NO_LIST = model.CHECK_NO_LIST.TONotNullString() + item.CHECKNO + ",";
                        }
                    }

                    model.CHECK_NO_LIST = model.CHECK_NO_LIST.Trim(new Char[] { ' ', ',' });
                    var note = applyChkData.Where(m => m.TYPE == "0" && m.CHECKNO == "8").Count();
                    if (note > 0)
                    {
                        model.CHECK_NO_NOTE = applyChkData.Where(m => m.TYPE == "0" && m.CHECKNO == "8").FirstOrDefault().NOTE;
                    }

                    //代理申請抓取代理人資料
                    if (appdata.APP_ROLE == "1")
                    {
                        if (appdata.A_AGENT == "0")
                        {
                            //自然人
                            model.E_NAME = appdata.E_NAME;
                            model.AE_RELATION = appdata.AE_RELATION;
                            model.E_BIRTHDAY = appdata.E_BIRTHDAY;
                            model.E_IDN = appdata.E_IDN;

                            // 地址
                            model.E_TAX_ORG_CITY_CODE = appdata.E_ADDR_CODE;
                            model.E_TAX_ORG_CITY_DETAIL = appdata.E_ADDR;
                            zip = new TblZIPCODE();
                            zip.ZIP_CO = appdata.E_ADDR_CODE;
                            getnam = dao.GetRow(zip);
                            if (getnam != null)
                            {
                                model.E_TAX_ORG_CITY_DETAIL = appdata.E_ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                                model.E_TAX_ORG_CITY_TEXT = getnam.CITYNM + getnam.TOWNNM;
                            }

                            model.E_TEL = appdata.E_TEL;
                            model.E_MAIL = appdata.E_MAIL;
                            var temp = applyFileData.Where(m => m.FILE_NO.ToString() == "1").ToList();
                            if (temp.Count > 0)
                            {
                                var itemFile = temp.LastOrDefault();
                                if (!string.IsNullOrEmpty(itemFile.FILENAME))
                                {
                                    var tempFilename = itemFile.FILENAME.Split('/');
                                    if (tempFilename.Length > 0)
                                    {
                                        model.FILE_01 = tempFilename[3].ToString() + "," + APP_ID + ",1,0";
                                    }
                                }
                            }
                        }
                        else
                        {
                            //法人
                            model.E_UNIT_NAME = appdata.E_UNIT_NAME;
                            // 地址
                            model.E_UNIT_TAX_ORG_CITY_CODE = appdata.E_UNIT_ADDR_CODE;
                            model.E_UNIT_TAX_ORG_CITY_DETAIL = appdata.E_UNIT_ADDR;
                            zip = new TblZIPCODE();
                            zip.ZIP_CO = appdata.E_UNIT_ADDR_CODE;
                            getnam = dao.GetRow(zip);
                            if (getnam != null)
                            {
                                model.E_UNIT_TAX_ORG_CITY_DETAIL = appdata.E_UNIT_ADDR.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                                model.E_UNIT_TAX_ORG_CITY_TEXT = getnam.CITYNM + getnam.TOWNNM;
                            }

                            var temp = applyFileData.Where(m => m.FILE_NO.ToString() == "2").ToList();
                            if (temp.Count > 0)
                            {
                                var itemFile = temp.LastOrDefault();
                                if (!string.IsNullOrEmpty(itemFile.FILENAME))
                                {
                                    var tempFilename = itemFile.FILENAME.Split('/');
                                    if (tempFilename.Length > 0)
                                    {
                                        model.FILE_02 = tempFilename[3].ToString() + "," + APP_ID + ",2,0";
                                    }
                                }
                            }
                        }
                    }

                    // 申請日期
                    model.ADD_TIME = ((DateTime)appdata.ADD_TIME).ToString("yyyy/MM/dd");
                    // 帳號
                    model.ACC_NO = alydata.ACC_NO;
                    // 單位名稱
                    model.NAME = appdata.NAME;

                    model.APP_ID = APP_ID;

                    //動態欄位CHK加入
                    for (var i = 0; i < model.APPFIL.GoodsList.Count; i++)
                    {
                        var chkList = applyChkData
                            .Where(m => m.TYPE == "1" && m.SEQ_NO == model.APPFIL.GoodsList[i].SEQ_NO).ToList();
                        foreach (var item in chkList)
                        {
                            model.APPFIL.GoodsList[i].CHECKNO_Lst = model.APPFIL.GoodsList[i].CHECKNO_Lst.TONotNullString() + item.CHECKNO + ",";
                        }
                        model.APPFIL.GoodsList[i].CHECKNO_Lst = model.APPFIL.GoodsList[i].CHECKNO_Lst.Trim(new Char[] { ' ', ',' });
                    }

                    model.FLOW_CD = alydata.FLOW_CD;
                    model.DOC_ITEM = "N";
                    model.APPFIL.IsReadOnly = true;
                    if (alydata.FLOW_CD == "2")
                    {
                        model.MAILBODY = alydata.MAILBODY;
                        model.DOC_FILE = "";
                        TblAPPLY_NOTICE notice = new TblAPPLY_NOTICE();
                        notice.APP_ID = APP_ID;
                        var noticedata = dao.GetRowList(notice);
                        foreach (var item in noticedata)
                        {
                            if (item.Field == "FILE_1")
                            {
                                model.DOC_FILE = item.Field;
                            }

                            if (item.Field == "FILE_2")
                            {
                                model.DOC_FILE = item.Field;
                            }

                            if (item.Field == "ALL_3")
                            {
                                model.DOC_ITEM = "Y";
                                model.APPFIL.IsReadOnly = false;
                            }
                        }
                    }
                    #endregion
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
        /// 補件驗證
        /// </summary>
        public ActionResult DocSave(Apply_012001DocModel form)
        {
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            // 檔案驗證
            //ErrorMsg += form.DocFileSave();
            //欄位驗證
            if (string.IsNullOrEmpty(form.TAX_ORG_CITY_CODE) || string.IsNullOrEmpty(form.TAX_ORG_CITY_TEXT) ||
                string.IsNullOrEmpty(form.TAX_ORG_CITY_DETAIL))
            {
                ErrorMsg += "地址 為必填欄位\n";
            }

            if (form.APP_ROLE == "1")
            {
                //自然人
                if (form.A_AGENT == "0")
                {
                    if (string.IsNullOrWhiteSpace(form.newFILE_01_TEXT) && form.FILE_01 == null)
                    {
                        ErrorMsg += "代理上傳委任書電子檔 為必填欄位\n";
                    }
                    if (string.IsNullOrEmpty(form.E_NAME))
                    {
                        ErrorMsg += "自然人姓名 為必填欄位\n";
                    }

                    if (string.IsNullOrEmpty(form.AE_RELATION))
                    {
                        ErrorMsg += "自然人與申請人關係 為必填欄位\n";
                    }

                    if (string.IsNullOrEmpty(form.E_BIRTHDAY_AD))
                    {
                        ErrorMsg += "自然人出生年月日 為必填欄位\n";
                    }

                    if (string.IsNullOrEmpty(form.E_IDN))
                    {
                        ErrorMsg += "自然人身分證明文件字號 為必填欄位\n";
                    }
                    else
                    {
                        if (!CheckUtils.IsIdentity(form.E_IDN))
                        {
                            ErrorMsg += "自然人身分證明文件字號 格式不正確\n";
                        }
                    }

                    if (string.IsNullOrEmpty(form.E_TEL))
                    {
                        ErrorMsg += "自然人電話 為必填欄位\n";
                    }

                    if (string.IsNullOrEmpty(form.E_MAIL))
                    {
                        ErrorMsg += "自然人E-MAIL 為必填欄位\n";
                    }
                    else
                    {
                        if (!reg3.IsMatch(form.E_MAIL))
                        {
                            ErrorMsg += "自然人E-MAIL 格式不正確\n";
                        }
                    }

                    if (string.IsNullOrEmpty(form.E_TAX_ORG_CITY_CODE) ||
                        string.IsNullOrEmpty(form.E_TAX_ORG_CITY_TEXT) ||
                        string.IsNullOrEmpty(form.E_TAX_ORG_CITY_DETAIL))
                    {
                        ErrorMsg += "自然人地址 為必填欄位\n";
                    }
                }
                //法人
                if (form.A_AGENT == "1")
                {
                    if (string.IsNullOrWhiteSpace(form.newFILE_02_TEXT) && form.FILE_02 == null)
                    {
                        ErrorMsg += "代理<法人>上傳登記證影本 為必填欄位\n";
                    }
                    if (string.IsNullOrEmpty(form.E_UNIT_NAME))
                    {
                        ErrorMsg += "法人、團體、事務所或營業所名稱 為必填欄位\n";
                    }

                    if (string.IsNullOrEmpty(form.E_UNIT_TAX_ORG_CITY_CODE) ||
                        string.IsNullOrEmpty(form.E_UNIT_TAX_ORG_CITY_TEXT) ||
                        string.IsNullOrEmpty(form.E_UNIT_TAX_ORG_CITY_DETAIL))
                    {
                        ErrorMsg += "管理人或代表人地址 為必填欄位\n";
                    }
                }
            }

            // 申請資料 檢核
            if (string.IsNullOrEmpty(form.IDN))
            {
                ErrorMsg += "身分證明文件字號 為必填欄位\n";
            }
            else
            {
                if (!CheckUtils.IsIdentity(form.IDN))
                {
                    ErrorMsg += "身分證明文件字號 格式不正確\n";
                }
            }
            if (string.IsNullOrEmpty(form.MAIL))
            {
                ErrorMsg += "E-MAIL 為必填欄位\n";
            }
            else
            {
                if (!reg3.IsMatch(form.MAIL))
                {
                    ErrorMsg += "E-MAIL 格式不正確\n";
                }
            }


            //動態表格驗證
            for (var i = 0; i < form.APPFIL.GoodsList.Count; i++)
            {
                if (string.IsNullOrEmpty(form.APPFIL.GoodsList[i].FILENUM))
                {
                    ErrorMsg += "序號" + (i + 1) + "_檔號及文號 為必填欄位\n";
                }

                //if (string.IsNullOrEmpty(Form.APPFIL.GoodsList[i].FILENAME))
                //{
                //    ErrorMsg += "序號" + (i + 1) + "_檔案名稱或內容要旨 為必填欄位\n";
                //}

                if (string.IsNullOrEmpty(form.APPFIL.GoodsList[i].NUMCNT))
                {
                    ErrorMsg += "序號" + (i + 1) + "_件數 為必填欄位\n";
                }

                if (string.IsNullOrEmpty(form.APPFIL.GoodsList[i].CHECKNO_Lst))
                {
                    ErrorMsg += "序號" + (i + 1) + "_申請項目 為必填欄位\n";
                }
            }

            //if (string.IsNullOrEmpty(Form.APP_REASON))
            //{
            //    ErrorMsg += "使用檔卷原件事由 為必填欄位\n";
            //}

            if (string.IsNullOrEmpty(form.CHECK_NO_LIST))
            {
                ErrorMsg += "申請目的及用途 為必填欄位\n";
            }

            var ChkItem = form.CHECK_NO_LIST.Split(',');
            for (var i = 0; i < ChkItem.Length; i++)
            {
                if (ChkItem[i] == "8")
                {
                    if (string.IsNullOrEmpty(form.CHECK_NO_NOTE))
                    {
                        ErrorMsg += "申請目的及用途(其他) 為必填欄位\n";
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(ErrorMsg))
            {
                result.status = true;
                result.message = "";
            }
            else
            {
                result.status = false;
                result.message = ErrorMsg;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_012001DocModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;

            // 存檔
            dao.UpdateApply012001(model);
            // 寄信
            dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "檔案應用申請", "012001", "0");

            return Done("2", "0");

        }
    }
}
