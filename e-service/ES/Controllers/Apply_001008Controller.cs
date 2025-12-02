using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using ES.Utils;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ES.Controllers
{
    public class Apply_001008Controller : BaseController
    {
        //
        // GET: /Apply_001008/

        public static string s_SRV_NAME = "醫事人員或公共衛生師請領英文證明書";

        /// <summary>
        /// 顯示警語畫面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            Apply_001008FormModel model = new Apply_001008FormModel();
            //ActionResult rtn = View("Prompt", model.Form);
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt001008", model);
        }

        #region 新增申辦案件作業
        /// <summary>
        /// 新申辦案件(空白表單填寫頁)
        /// </summary>
        /// <returns></returns>
        [DisplayName("Apply_001008_申請")]
        [HttpGet]
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            Apply_001008FormModel model = new Apply_001008FormModel();
            ActionResult rtn = View("Index", model);

            model.APPLY_DATE = Commons.HelperUtil.DateTimeToString(DateTime.Now);
            //model.APPLY_DATE_TW = Commons.HelperUtil.TransTwToAdYear(model.APPLY_DATE);
            model.FlowMode = "1";

            if (sm == null || sm.UserInfo == null)
            {
                rtn = RedirectToAction("Index", "Login");
                return rtn;
            }

            var UsIn = sm.UserInfo.Member;

            if (UsIn != null)
            {
                //agree: 1:同意新增 /other:請先閱讀規章
                if (string.IsNullOrEmpty(agree)) { agree = "0"; }
                if (agree != null && !agree.Equals("1")) { return Prompt(); }

                #region 帶入帳號資訊
                ShareDAO dao = new ShareDAO();
                model.ACC_NO = UsIn.ACC_NO;
                model.SRV_ID = "001008";
                model.SRC_SRV_ID = "001008";
                model.UNIT_CD = dao.GetServiceUnitCD(model.SRV_ID);

                model.NAME = UsIn.NAME;
                model.IDN = UsIn.IDN;
                model.ENAME = UsIn.ENAME;
                model.TEL = UsIn.TEL;
                model.MOBILE = UsIn.MOBILE;
                model.EMAIL = UsIn.MAIL;

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = sm.UserInfo.Member.TOWN_CD;
                var address = dao.GetRow(zip);

                model.ADDR_ZIP = sm.UserInfo.Member.TOWN_CD;
                if (address != null && !string.IsNullOrEmpty(address.TOWNNM))
                {
                    model.ADDR_ZIP_ADDR = address.TOWNNM;
                    model.ADDR_ZIP_DETAIL = sm.UserInfo.Member.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }
                else
                {
                    model.ADDR_ZIP_ADDR = string.Empty;
                    model.ADDR_ZIP_DETAIL = sm.UserInfo.Member.ADDR;
                }

                //筆數
                model.RowCount = 1;
                model.MERGEYN = "N";
                #endregion
            }
            else
            {
                rtn = RedirectToAction("Index", "Login");
            }

            return rtn;
        }

        #region 註解
        ///// <summary>
        ///// 申辦流程
        ///// 1:填寫申報表件並上傳檔案 / 2:預覽申辦表件 / 3:繳費 / 4:完成申報
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult Apply(Apply_001008FormModel model)
        //{
        //    // 由登入資訊取得當前角色的檢定類別資訊
        //    SessionModel sm = SessionModel.Get();
        //    ActionResult rtn = View(model);
        //    if (model.MERGEYN == "Y")
        //    {
        //        foreach (var item in ModelState)
        //        {
        //            if (item.Key.ToLeft(5) == "FILE_")
        //            {
        //                item.Value.Errors.Clear();
        //            }
        //        }
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        //寄送地址判斷
        //        //上傳附件判斷
        //        var errormsg = model.FileSave();
        //        if (errormsg != "")
        //        {
        //            sm.LastErrorMessage = errormsg;
        //        }
        //        else
        //        {
        //            if (model.FlowMode == "1" || model.FlowMode == "2")
        //            {
        //                if (model.IsMode == "0")
        //                {
        //                    //新增
        //                    model.IsNew = true;
        //                    model.ME.IsReadOnly = false;
        //                    model.PR.IsReadOnly = false;
        //                    model.TRANS.IsReadOnly = false;
        //                    model.TRANSF.IsReadOnly = false;
        //                    //model.ATH.IsReadOnly = false;
        //                }
        //                if (model.IsMode == "1")
        //                {
        //                    //檢視
        //                    model.IsNew = false;
        //                    model.ME.IsReadOnly = true;
        //                    model.PR.IsReadOnly = true;
        //                    model.TRANS.IsReadOnly = true;
        //                    model.TRANSF.IsReadOnly = true;
        //                    //model.ATH.IsReadOnly = true;
        //                }
        //            }
        //            else if (model.FlowMode == "3")
        //            {
        //                //繳費
        //                model.IsNew = false;
        //                model.ME.IsReadOnly = true;
        //                model.PR.IsReadOnly = true;
        //                model.TRANS.IsReadOnly = true;
        //                model.TRANSF.IsReadOnly = true;
        //                //model.ATH.IsReadOnly = true;
        //                model.PAY_A_FEE = Int32.Parse(model.TOTAL_MEM); //應繳金額
        //            }
        //            else
        //            {
        //                //新增
        //                model.IsNew = true;
        //                model.ME.IsReadOnly = false;
        //                model.PR.IsReadOnly = false;
        //                model.TRANS.IsReadOnly = false;
        //                model.TRANSF.IsReadOnly = false;
        //                //model.ATH.IsReadOnly = false;
        //            }
        //        }
        //    }
        //    return View("Index", model);
        //}
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_001008FormModel model)
        {
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            //var idx = 1;
            //var blTransExists = false;
            //var blTransFExists = false;

            this.ValidateApplyForm(model);

            #region 註解
            //檢核是否有填國內寄送地址資料
            //if (model.TRANS != null)
            //{
            //    foreach (var item in model.TRANS.GoodsList)
            //    {
            //        if (!string.IsNullOrWhiteSpace(item.TRANS_ZIP) || !string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL) || item.TRANS_COPIES != null)
            //        {
            //            blTransExists = true;
            //            break;
            //        }
            //    }
            //}

            ////檢核是否有填國外寄送地址資料
            //if (model.TRANSF != null)
            //{
            //    foreach (var item in model.TRANSF.GoodsList)
            //    {
            //        if (!string.IsNullOrWhiteSpace(item.TRANSF_ADDR) || item.TRANSF_COPIES != null)
            //        {
            //            blTransFExists = true;
            //            break;
            //        }
            //    }
            //}


            //if (!blTransExists && !blTransFExists)
            //{
            //    ModelState.AddModelError("TRANS", "國內外寄送地址請擇一輸入");
            //}
            //else
            //{
            //    //檢核國內寄送地址資料
            //    if (model.TRANS != null)
            //    {
            //        foreach (var item in model.TRANS.GoodsList)
            //        {
            //            if ((string.IsNullOrWhiteSpace(item.TRANS_ZIP) && string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL)) && item.TRANS_COPIES != null)
            //            {
            //                ModelState.AddModelError("TRANS_ADDR_EMPTY_" + idx.ToSingle(), String.Format("[{0}]國內寄送地址 為必填欄位", idx.ToString()));
            //            } 
            //            else if ((!string.IsNullOrWhiteSpace(item.TRANS_ZIP) && string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL)) || (string.IsNullOrWhiteSpace(item.TRANS_ZIP) && !string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL)))
            //            {
            //                ModelState.AddModelError("TRANS_ADDR_EMPTY_" + idx.ToSingle(), String.Format("[{0}]國內寄送地址 尚未填寫完整", idx.ToString()));
            //            }

            //            if ((!string.IsNullOrWhiteSpace(item.TRANS_ZIP) || !string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL)) && item.TRANS_COPIES == null)
            //            {
            //                ModelState.AddModelError("TRANS_COPIES_EMPTY_" + idx.ToSingle(), String.Format("[{0}]國內寄送份數 為必填欄位", idx.ToString()));
            //            }

            //            if (item.TRANS_COPIES != null && item.TRANS_COPIES <= 0)
            //            {
            //                ModelState.AddModelError("TRANS_COPIES_" + idx.ToSingle(), String.Format("[{0}]國外寄送份數 請填寫大於0的件數", idx.ToString()));
            //            }

            //            idx++;
            //        }
            //    }

            //    //檢核國外寄送地址資料
            //    if (model.TRANSF != null)
            //    {
            //        idx = 1;
            //        foreach (var item in model.TRANSF.GoodsList)
            //        {
            //            if (string.IsNullOrWhiteSpace(item.TRANSF_ADDR) && item.TRANSF_COPIES != null)
            //            {
            //                ModelState.AddModelError("TRANSF_ADDR_EMPTY_" + idx.ToSingle(), String.Format("[{0}]國外寄送地址 為必填欄位", idx.ToString()));
            //            }

            //            if (!string.IsNullOrWhiteSpace(item.TRANSF_ADDR) && item.TRANSF_COPIES == null)
            //            {
            //                ModelState.AddModelError("TRANSF_COPIES_EMPTY" + idx.ToSingle(), String.Format("[{0}]國外寄送份數 為必填欄位", idx.ToString()));
            //            }

            //            if (item.TRANSF_COPIES != null && item.TRANSF_COPIES <= 0)
            //            {
            //                ModelState.AddModelError("TRANSF_COPIES_" + idx.ToSingle(), String.Format("[{0}]國外寄送份數 請填寫大於0的件數", idx.ToString()));
            //            }
            //            idx++;
            //        }
            //    }
            //}

            //string chkFileMsg = model.FileSave();
            //if (!string.IsNullOrEmpty(chkFileMsg))
            //{
            //    ModelState.AddModelError("FILEUPLOAD_MSG", chkFileMsg);
            //}
            #endregion

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                //ErrorMsg = model.FileSave(); //submit方式

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

        [DisplayName("Apply_001008_預覽")]
        [HttpPost]
        public ActionResult PreView(Apply_001008FormModel model)
        {
            ShareDAO dao = new ShareDAO();

            model.IsNew = false;
            model.IsMode = "1";
            model.FlowMode = "2";
            model.FileSave();

            //model.APPLY_DATE_TW = Commons.HelperUtil.TransToTwYear(model.APPLY_DATE);
            model.ME.IsReadOnly = true;
            model.PR.IsReadOnly = true;
            model.TRANS.IsReadOnly = true;
            model.TRANSF.IsReadOnly = true;

            return PartialView("PreView001008", model);
        }

        [DisplayName("Apply_001008_繳費")]
        [HttpPost]
        public ActionResult Pay(Apply_001008FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            int payFee = 0;
            int.TryParse(model.TOTAL_MEM, out payFee);

            model.PAY_A_FEE = payFee;
            model.PAY_METHOD = "";
            return PartialView("Pay", model);
        }

        /// <summary>
        /// 繳費金額計算方式範例
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PayMemo()
        {
            return View();
        }

        /// <summary>
        /// 線費-存檔並送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001008_完成申報")]
        [HttpPost]
        public ActionResult Save(Apply_001008FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("Index", model);

            string srvID = "001008";
            string srvName = "醫事人員或公共衛生師請領英文證明書";
            model.CLIENT_IP = GetClientIP();

            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            if (sm == null || sm.UserInfo == null)
            {
                sm.LastResultMessage = "操作已逾時，請重新登入";
                rtn = RedirectToAction("Index", "Login");
                return rtn;
            }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null)
            {
                sm.LastResultMessage = "操作已逾時，請重新登入";
                rtn = RedirectToAction("Index", "Login");
                return rtn;
            }

            #region 申請者收件內容(no use)
            //string custMailBody = "";
            //string appTimeY = "";
            //string appTimeM = "";
            //string appTimeD = "";
            //string appTime = HelperUtil.TransToTwYear(model.APPLY_DATE);
            //if (!string.IsNullOrEmpty(appTime))
            //{
            //    appTimeY = appTime.Substring(0, 3);
            //    appTimeM = appTime.Substring(4, 2);
            //    appTimeD = appTime.Substring(7, 2);
            //}

            //custMailBody = memberName + "，您好:<br/><br/>";
            //custMailBody += "您於民國" + appTimeY + "年" + appTimeM + "月" + appTimeD + "日申辦之" + srvName + "案件<br/>";
            //custMailBody += "申請編號：<a href='https://e-service.mohw.gov.tw//History/Show/" + model.APP_ID + "'>" + model.APP_ID + "</a>現在進度為'新收案件'<br/>";
            //custMailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
            //custMailBody += "衛生福利部醫事司敬上<br/><br/>";
            //custMailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
            //custMailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
            //custMailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
            //custMailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
            //custMailBody += "115204 臺北市南港區忠孝東路六段488號";
            #endregion

            switch (model.PAY_METHOD)
            {
                case "C": //信用卡線上刷卡（電子化政府網路付費服務）
                    //ApplyModel apply = dao.GetNewApplyEmptyRow(srvID, 4);
                    model.APP_ID = dao.GetApp_ID("001008");
                    //model.Apply = apply;
                    model.ISEC = DataUtils.GetConfig("PAY_EC_OPEN"); // Y 啟用

                    if (!dao.RunCreditCardTranx(model))
                    {
                        dao.DeleteApplyEmptyRow(model.APP_ID);
                        //sm.LastErrorMessage = model.ErrorMessage;
                        break;
                    }
                    else
                    {
                        //dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                        dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, srvName, srvID);
                    }

                    break;
                //匯票(抬頭：衛生福利部）
                case "D":
                    if (!dao.RunPayTranx(model))
                    {
                        model.ErrorCode = "-1";
                        model.ErrorMessage = "匯票繳費執行失敗!!";
                        break;
                    }
                    else
                    {
                        //dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                        dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, srvName, srvID);
                    }
                    model.ErrorCode = "0000";
                    break;
                //劃撥（見註一、註三）
                case "T":
                    if (!dao.RunPayTranx(model))
                    {
                        model.ErrorCode = "-1";
                        model.ErrorMessage = "劃撥繳費執行失敗!!";
                        break;
                    }
                    else
                    {
                        //dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                        dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, srvName, srvID);
                    }
                    model.ErrorCode = "0000";
                    break;
                //臨櫃（現金，見註二）
                case "B":
                    if (!dao.RunPayTranx(model))
                    {
                        model.ErrorCode = "-1";
                        model.ErrorMessage = "臨櫃繳費執行失敗!!";
                        break;
                    }
                    else
                    {
                        //dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                        dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, srvName, srvID);
                    }
                    model.ErrorCode = "0000";
                    break;
                //超商（見註五）
                case "S":
                    if (!dao.RunPayTranx(model))
                    {
                        model.ErrorCode = "-1";
                        model.ErrorMessage = "超商繳費執行失敗!!";
                        break;
                    }
                    else
                    {
                        //dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                        dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, srvName, srvID);
                    }
                    model.ErrorCode = "0000";
                    break;
            }

            return View("Save", model);
        }

        [DisplayName("Apply_001008_列印繳費單")]
        /// <summary>
        /// 列印繳費單
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult PayPDF(string id)
        {
            ApplyDAO dao = new ApplyDAO();
            return File(dao.ExportPayPDF(id), "application/pdf", "Pay" + id + ".pdf");
        }

        /// <summary>
        /// 申請表單資料檢核
        /// </summary>
        /// <param name="model"></param>
        public void ValidateApplyForm(Apply_001008FormModel model)
        {
            bool blMeExists = false;
            bool blPrExists = false;
            bool blTransExists = false;
            bool blTransFExists = false;
            //bool blFlag = false;
            int idx = 1;

            #region 申辦表件填寫
            if (!string.IsNullOrWhiteSpace(model.TEL_Zip) || !string.IsNullOrWhiteSpace(model.TEL_Phone) || !string.IsNullOrWhiteSpace(model.TEL_Num))
            {
                if (string.IsNullOrWhiteSpace(model.TEL_Zip) || string.IsNullOrWhiteSpace(model.TEL_Phone))
                {
                    ModelState.AddModelError("TEL_MSG", "電話 尚未填寫完整。");
                }
            }

            //選自訂& @後面未填值
            if (string.IsNullOrWhiteSpace(model.EMAIL_1) || (model.EMAIL_2 == "0" && string.IsNullOrWhiteSpace(model.EMAIL_3)))
            {
                ModelState.AddModelError("EMAIL_MSG", "EMAIL 尚未填寫完整。");
            }
            else if (!HelperUtil.IsEmail(model.EMAIL))
            {
                ModelState.AddModelError("EMAIL_MSG", "EMAIL 格式填寫錯誤。");
            }
            #endregion

            #region 醫事人員或公共衛生師/專科證書資料資料檢核
            //醫事人員證書grid
            if (model.ME != null && model.ME.GoodsList != null)
            {
                idx = 1;
                foreach (var item in model.ME.GoodsList)
                {
                    if (!string.IsNullOrWhiteSpace(item.ME_LIC_TYPE)
                        || !string.IsNullOrWhiteSpace(item.ME_LIC_NUM)
                        || !string.IsNullOrWhiteSpace(item.ME_ISSUE_DATE_AD)
                        || item.ME_COPIES != null
                        || !string.IsNullOrWhiteSpace(item.ME_TYPE_CD))
                    {
                        blMeExists = true;

                        if (string.IsNullOrWhiteSpace(item.ME_LIC_TYPE))
                        {
                            ModelState.AddModelError("ME_LIC_TYPE_" + idx.ToString(), string.Format("醫事人員或公共衛生師證書 第 {0} 筆 醫事人員證書 欄位是必要項。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.ME_LIC_NUM))
                        {
                            ModelState.AddModelError("ME_LIC_NUM_" + idx.ToString(), string.Format("醫事人員或公共衛生師證書 第 {0} 筆 證書字號 欄位是必要項。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.ME_ISSUE_DATE_AD))
                        {
                            ModelState.AddModelError("ME_ISSUE_DATE_AD_" + idx.ToString(), string.Format("醫事人員或公共衛生師證書 第 {0} 筆 核發日期 欄位是必要項。", idx.ToString()));
                        }

                        if (item.ME_COPIES == null)
                        {
                            ModelState.AddModelError("ME_COPIES_" + idx.ToString(), string.Format("醫事人員或公共衛生師證書 第 {0} 筆 申請份數 欄位是必要項。", idx.ToString()));
                        }
                        else if (item.ME_COPIES <= 0)
                        {
                            ModelState.AddModelError("ME_COPIES_" + idx.ToString(), string.Format("醫事人員或公共衛生師證書 第 {0} 筆 申請份數 必須輸入大於0的份數。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.ME_TYPE_CD))
                        {
                            ModelState.AddModelError("ME_TYPE_CD_" + idx.ToString(), string.Format("醫事人員或公共衛生師證書 第 {0} 筆 開立證明格式 欄位是必要項。", idx.ToString()));
                        }
                    }

                    idx++;
                }
            }

            //專科證書grid
            if (model.PR != null && model.PR.GoodsList != null)
            {
                idx = 1;
                foreach (var item in model.PR.GoodsList)
                {
                    if (!string.IsNullOrWhiteSpace(item.PR_LIC_TYPE)
                        || !string.IsNullOrWhiteSpace(item.PR_LIC_NUM)
                        || !string.IsNullOrWhiteSpace(item.PR_ISSUE_DATE_AD)
                        || !string.IsNullOrWhiteSpace(item.PR_EF_DATE_S_AD)
                        || !string.IsNullOrWhiteSpace(item.PR_EF_DATE_E_AD)
                        || item.PR_COPIES != null
                        || !string.IsNullOrWhiteSpace(item.PR_TYPE_CD))
                    {
                        blPrExists = true;

                        if (string.IsNullOrWhiteSpace(item.PR_LIC_TYPE))
                        {
                            ModelState.AddModelError("PR_LIC_TYPE_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 專科證書 欄位是必要項。", idx.ToString()));
                        }
                        else if (item.PR_LIC_TYPE.TONotNullString() == "C0700"
                           || item.PR_LIC_TYPE.TONotNullString() == "C0701"
                           || item.PR_LIC_TYPE.TONotNullString() == "C0900")
                        {
                            ModelState.AddModelError("PR_LIC_TYPE_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 專科證書 您所選擇的類型，尚未開放線上申辦。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.PR_LIC_NUM))
                        {
                            ModelState.AddModelError("PR_LIC_NUM_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 證書字號 欄位是必要項。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.PR_ISSUE_DATE_AD))
                        {
                            ModelState.AddModelError("PR_ISSUE_DATE_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 核發日期 欄位是必要項。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.PR_EF_DATE_S_AD))
                        {
                            ModelState.AddModelError("PR_EF_DATE_S_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 有效期間(起) 欄位是必要項。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.PR_EF_DATE_E_AD))
                        {
                            ModelState.AddModelError("PR_EF_DATE_E_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 有效期間(迄) 欄位是必要項。", idx.ToString()));
                        }

                        if (!string.IsNullOrWhiteSpace(item.PR_EF_DATE_S_AD) && !string.IsNullOrWhiteSpace(item.PR_EF_DATE_E_AD))
                        {
                            if (item.PR_EF_DATE_S_AD.CompareTo(item.PR_EF_DATE_E_AD) > 0)
                            {
                                ModelState.AddModelError("PR_EF_DATE_" + idx.ToSingle(), String.Format("專科證書 第 {0} 筆 有效期間 起日不得大於迄日。", idx.ToString()));
                            }
                        }

                        if (item.PR_COPIES == null)
                        {
                            ModelState.AddModelError("PR_COPIES_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 申請份數 欄位是必要項。", idx.ToString()));
                        }
                        else if (item.PR_COPIES <= 0)
                        {
                            ModelState.AddModelError("PR_COPIES_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 申請份數 必須輸入大於0的份數。", idx.ToString()));
                        }

                        if (string.IsNullOrWhiteSpace(item.PR_TYPE_CD))
                        {
                            ModelState.AddModelError("PR_TYPE_CD_" + idx.ToString(), string.Format("專科證書 第 {0} 筆 開立證明格式 欄位是必要項。", idx.ToString()));
                        }
                    }
                    idx++;
                }
            }

            if (!blMeExists && !blPrExists)
            {
                ModelState.AddModelError("CERT", "醫事人員或公共衛生師證書及專科證書 請至少擇一輸入一筆資料。");
            }
            #endregion

            #region 國內外寄送地址資料檢核
            //國內寄送地址grid
            if (model.TRANS != null && model.TRANS.GoodsList != null)
            {
                idx = 1;
                foreach (var item in model.TRANS.GoodsList)
                {
                    if (!string.IsNullOrWhiteSpace(item.TRANS_ZIP) || !string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL) || item.TRANS_COPIES != null)
                    {
                        blTransExists = true;
                        if (string.IsNullOrWhiteSpace(item.TRANS_ZIP) && string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL))
                        {
                            ModelState.AddModelError("TRANS_ADDR_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國內寄送地址 欄位是必要項。", idx.ToString()));
                        }
                        else if (string.IsNullOrWhiteSpace(item.TRANS_ZIP) || string.IsNullOrWhiteSpace(item.TRANS_ZIP_DETAIL))
                        {
                            ModelState.AddModelError("TRANS_ADDR_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國內寄送地址 尚未填寫完整", idx.ToString()));
                        }

                        if (item.TRANS_COPIES == null)
                        {
                            ModelState.AddModelError("TRANS_COPIES_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國外寄送份數 欄位是必要項。", idx.ToString()));
                        }
                        else if (item.TRANS_COPIES <= 0)
                        {
                            ModelState.AddModelError("TRANS_COPIES_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國外寄送份數 請填寫大於0的份數。", idx.ToString()));
                        }
                    }

                    idx++;
                }
            }

            //國外寄送地址grid
            if (model.TRANSF != null)
            {
                idx = 1;
                foreach (var item in model.TRANSF.GoodsList)
                {
                    if (!string.IsNullOrWhiteSpace(item.TRANSF_ADDR) || !string.IsNullOrWhiteSpace(item.TRANSF_UNITNAME) || item.TRANSF_COPIES != null)
                    {
                        blTransFExists = true;

                        if (string.IsNullOrWhiteSpace(item.TRANSF_ADDR))
                        {
                            ModelState.AddModelError("TRANSF_ADDR_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 國外寄送地址 欄位是必要項。", idx.ToString()));
                        }

                        //20201221 add [MOHES-890]醫事人員請領英文證明書-001008-(前台)申辦案件的國外寄送地址區塊增加一個機構名稱的欄位
                        if (string.IsNullOrWhiteSpace(item.TRANSF_UNITNAME))
                        {
                            ModelState.AddModelError("TRANSF_UNITNAME_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 機構名稱 欄位是必要項。", idx.ToString()));
                        }

                        if (item.TRANSF_COPIES == null)
                        {
                            ModelState.AddModelError("TRANSF_COPIES_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 國外寄送份數 欄位是必要項。", idx.ToString()));
                        }
                        else if (item.TRANSF_COPIES <= 0)
                        {
                            ModelState.AddModelError("TRANSF_COPIES_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 國外寄送份數 請填寫大於0的份數。", idx.ToString()));
                        }
                    }
                    idx++;
                }
            }

            if (!blTransExists && !blTransFExists)
            {
                ModelState.AddModelError("TRANS", "國內外寄送地址 請至少擇一輸入一筆資料。");
            }
            #endregion

            string chkFileMsg = model.FileSave();
            if (!string.IsNullOrEmpty(chkFileMsg))
            {
                ModelState.AddModelError("FILEUPLOAD_MSG", chkFileMsg);
            }
        }

        /// <summary>
        /// 繳費資料檢核
        /// </summary>
        /// <param name="model"></param>
        public void ValidatePayForm(Apply_001008FormModel model)
        {
            string[] payMethodAry = { "C", "T", "D", "B", "S" };

            if (string.IsNullOrEmpty(model.PAY_METHOD))
            {
                ModelState.AddModelError("PAYMETHOD_MSG", "請選擇 繳費方式");
            }
            else if (Array.IndexOf(payMethodAry, model.PAY_METHOD) < 0)
            {
                ModelState.AddModelError("PAYMETHOD_MSG", "繳費方式資料錯誤");
            }

            if ("C".Equals(model.PAY_METHOD))
            {
                if (string.IsNullOrEmpty(model.CARD_IDN))
                {
                    ModelState.AddModelError("CARD_IDN_MSG", "請輸入 持卡人身份證字號");
                }
                else
                {

                }
            }
        }

        [DisplayName("Apply_001008_完成申報")]
        [HttpPost]
        public ActionResult Save2(Apply_001008FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            //ActionResult rtn = View("Index", model);
            var result = new AjaxResultStruct();
            string errMsg = "";

            string srvID = "001008";
            string srvName = "醫事人員或公共衛生師請領英文證明書";
            model.CLIENT_IP = GetClientIP();

            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            this.ValidateApplyForm(model);
            this.ValidatePayForm(model);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                try
                {
                    #region 申請者收件內容
                    string custMailBody = "";
                    string appTimeY = "";
                    string appTimeM = "";
                    string appTimeD = "";
                    string appTime = HelperUtil.TransToTwYear(model.APPLY_DATE);
                    if (!string.IsNullOrEmpty(appTime))
                    {
                        appTimeY = appTime.Substring(0, 3);
                        appTimeM = appTime.Substring(4, 2);
                        appTimeD = appTime.Substring(7, 2);
                    }

                    custMailBody = memberName + "，您好:<br/><br/>";
                    custMailBody += "您於民國" + appTimeY + "年" + appTimeM + "月" + appTimeD + "日申辦之" + srvName + "案件<br/>";
                    custMailBody += "申請編號：<a href='https://e-service.mohw.gov.tw//History/Show/" + model.APP_ID + "'>" + model.APP_ID + "</a>現在進度為'新收案件'<br/>";
                    custMailBody += "特此通知。感謝您使用衛生福利部線上申辦系統<br/><br/>";
                    custMailBody += "衛生福利部醫事司敬上<br/><br/>";
                    custMailBody += "PS.本郵件係系統自動發信，請勿直接回信；如有問題，請逕向本部相關業務單位洽詢。<br><br>";
                    custMailBody += "※非移植目的承辦單位：食品藥物管理署藥品及新興生技藥品組(02)2787-8000<br>";
                    custMailBody += "115209 臺北市南港區昆陽街161-2號<br><br>";
                    custMailBody += "※移植目的承辦單位：衛生福利部醫事司(02)8590-6666<br>";
                    custMailBody += "115204 臺北市南港區忠孝東路六段488號";
                    #endregion

                    switch (model.PAY_METHOD)
                    {
                        case "C": //信用卡線上刷卡（電子化政府網路付費服務）
                            ApplyModel apply = dao.GetNewApplyEmptyRow(srvID, 4);

                            model.APP_ID = apply.APP_ID;
                            model.Apply = apply;

                            if (!dao.RunCreditCardTranx(model))
                            {
                                dao.DeleteApplyEmptyRow(model.APP_ID);
                                //sm.LastErrorMessage = model.ErrorMessage;
                                break;
                            }
                            else
                            {
                                dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                            }

                            break;
                        //匯票(抬頭：衛生福利部）
                        case "D":
                            if (!dao.RunPayTranx(model))
                            {
                                model.ErrorCode = "-1";
                                model.ErrorMessage = "匯票繳費執行失敗!!";
                                break;
                            }
                            else
                            {
                                dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                            }
                            model.ErrorCode = "0000";
                            break;
                        //劃撥（見註一、註三）
                        case "T":
                            if (!dao.RunPayTranx(model))
                            {
                                model.ErrorCode = "-1";
                                model.ErrorMessage = "劃撥繳費執行失敗!!";
                                break;
                            }
                            else
                            {
                                dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                            }
                            model.ErrorCode = "0000";
                            break;
                        //臨櫃（現金，見註二）
                        case "B":
                            if (!dao.RunPayTranx(model))
                            {
                                model.ErrorCode = "-1";
                                model.ErrorMessage = "臨櫃繳費執行失敗!!";
                                break;
                            }
                            else
                            {
                                dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                            }
                            model.ErrorCode = "0000";
                            break;
                        //超商（見註五）
                        case "S":
                            if (!dao.RunPayTranx(model))
                            {
                                model.ErrorCode = "-1";
                                model.ErrorMessage = "超商繳費執行失敗!!";
                                break;
                            }
                            else
                            {
                                dao.SendMail_New(memberName, memberEmail, model.APP_ID, srvName, srvID, custMailBody);
                            }
                            model.ErrorCode = "0000";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    result.status = false;
                    result.message = "儲存發生錯誤，請洽系統管理者";
                }
            }
            else
            {
                result.status = false;
                foreach (ModelState item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        errMsg += item.Errors[0].ErrorMessage + "\r\n";
                    }
                }
            }

            return Content(result.Serialize(), "application/json");
        }
        #endregion

        #region 補件查詢
        /// <summary>
        /// 補件查詢
        /// </summary>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001008FormModel model = new Apply_001008FormModel(APP_ID);
            ActionResult rtn = null;
            string srvID = "001008";

            try
            {
                if (sm.UserInfo == null || sm.UserInfo.Member == null)
                {
                    sm.LastResultMessage = "您尚未登入會員或無權限使用此功能 !";
                    rtn = RedirectToAction("Index", "Login");
                    return rtn;
                }

                var UsIn = sm.UserInfo.Member;

                // 案件基本資訊(apply)
                ApplyModel apply = new ApplyModel();
                apply.APP_ID = APP_ID;
                var applydata = dao.GetRow(apply);

                model.SRV_ID = srvID;
                if (applydata == null || applydata.APP_ID == null)
                {
                    sm.LastErrorMessage = "查無此案件!";// ex.Message;
                    return RedirectToAction("Index", "History");
                }

                //案件狀態名稱資料
                TblCODE_CD codewhere = new TblCODE_CD();
                codewhere.CODE_PCD = "2";
                codewhere.CODE_KIND = "F_CASE_STATUS";
                codewhere.CODE_CD = applydata.FLOW_CD;
                var codedata = dao.GetRow(codewhere);
                if (codedata != null)
                {
                    model.CODE_TEXT = codedata.CODE_DESC;
                }

                model.CODE_CD = applydata.FLOW_CD;
                model.APPLY_DATE = HelperUtil.DateTimeToString(applydata.APP_TIME);
                model.MAILBODY = applydata.MAILBODY;

                // 判斷是否為該案件申請人
                if (!applydata.ACC_NO.Equals(UsIn.ACC_NO))
                {
                    sm.LastErrorMessage = "非案件申請人無法瀏覽次案件 !";// ex.Message;
                    return RedirectToAction("Index", "Login");
                }

                // 基本資料（apply_001008）
                Apply_001008Model app = new Apply_001008Model();
                app.APP_ID = APP_ID;
                var appdata = dao.GetRow(app);

                // 載入表件資料
                #region 申辦表件填寫資料
                model.IDN = applydata.IDN;
                model.NAME = applydata.NAME;
                model.ENAME = applydata.ENAME;
                model.ENAME_ALIAS = appdata.ENAME_ALIAS;
                model.REMARK = appdata.REMARK;
                model.TOTAL_MEM = Convert.ToString(appdata.TOTAL_MEM);
                model.ADDR_ZIP = applydata.ADDR_CODE;
                model.ADDR_ZIP_DETAIL = applydata.ADDR;
                model.ADDR = applydata.ADDR;
                model.TEL = applydata.TEL;
                model.MOBILE = applydata.MOBILE;
                model.EMAIL = appdata.EMAIL;
                model.MERGEYN = appdata.MERGEYN;
                #endregion

                #region 醫事人員證書grid
                if (model.ME != null && model.ME.GoodsList != null && model.ME.GoodsList.Count > 0)
                {
                    foreach (var item in model.ME.GoodsList)
                    {
                        item.ME_ISSUE_DATE_AD = HelperUtil.DateTimeToString(item.ME_ISSUE_DATE);
                    }
                }
                else
                {
                    model.ME.APP_ID = APP_ID;
                    model.ME.GoodsList = new List<Apply_001008_MeViewModel>();
                    model.ME.GoodsList.Add(new Apply_001008_MeViewModel());
                }
                #endregion

                #region 專科證書grid
                if (model.PR != null && model.PR.GoodsList != null && model.PR.GoodsList.Count > 0)
                {
                    foreach (var item in model.PR.GoodsList)
                    {
                        item.PR_ISSUE_DATE_AD = HelperUtil.DateTimeToString(item.PR_ISSUE_DATE);
                        item.PR_EF_DATE_S_AD = HelperUtil.DateTimeToString(item.PR_EF_DATE_S);
                        item.PR_EF_DATE_E_AD = HelperUtil.DateTimeToString(item.PR_EF_DATE_E);
                    }
                }
                else
                {
                    model.PR.APP_ID = APP_ID;
                    model.PR.GoodsList = new List<Apply_001008_PrViewModel>();
                    model.PR.GoodsList.Add(new Apply_001008_PrViewModel());
                }
                #endregion

                #region 國內郵寄地址
                if (model.TRANS == null || model.TRANS.GoodsList == null || model.TRANS.GoodsList.Count == 0)
                {
                    model.TRANS.APP_ID = APP_ID;
                    model.TRANS.GoodsList = new List<Apply_001008_TransViewModel>();
                    model.TRANS.GoodsList.Add(new Apply_001008_TransViewModel());
                }
                #endregion

                #region 國外郵寄地址
                if (model.TRANSF == null || model.TRANSF.GoodsList == null || model.TRANSF.GoodsList.Count == 0)
                {
                    model.TRANSF.APP_ID = APP_ID;
                    model.TRANSF.GoodsList = new List<Apply_001008_TransFViewModel>();
                    model.TRANSF.GoodsList.Add(new Apply_001008_TransFViewModel());
                }
                #endregion

                #region 佐證附件 
                Apply_001008FileModel fileList = dao.GetFile_001008(APP_ID);

                Apply_FileModel filedata = null;
                int pos = 0;

                //繳費紀錄照片或pdf檔案
                model.HAS_OFILE_PAYRECORD = fileList.HAS_OFILE_PAYRECORD;
                model.FILE_PAYRECORD_TEXT = fileList.FILE_PAYRECORD_TEXT;
                //Apply_FileModel where = new Apply_FileModel();
                //where.APP_ID = APP_ID;
                //where.FILE_NO = 1;
                //filedata = dao.GetRow<Apply_FileModel>(where);

                //var fileNo1 = model.GetType().GetProperties().Where(m => m.Name == "FILE_PAYRECORD_FILENAME");
                //if (fileNo1.ToCount() > 0)
                //{
                //    var fileObj1 = fileNo1.FirstOrDefault();
                //    //fileObj1.SetValue(model, filedata.FILENAME);

                //    if (!string.IsNullOrWhiteSpace(filedata.FILENAME))
                //    {
                //        pos = filedata.FILENAME.LastIndexOf("\\");
                //        fileObj1.SetValue(model, filedata.FILENAME.Substring(pos + 1, filedata.FILENAME.Length - pos - 1));
                //    }
                //}

                //護照影本電子檔
                model.HAS_OFILE_PAYPASSPORT = fileList.HAS_OFILE_PAYPASSPORT;
                model.FILE_PASSPORT_TEXT = fileList.FILE_PASSPORT_TEXT;
                //where = new Apply_FileModel();
                //where.APP_ID = APP_ID;
                //where.FILE_NO = 2;
                //filedata = dao.GetRow<Apply_FileModel>(where);

                //var fileNo2 = model.GetType().GetProperties().Where(m => m.Name == "FILE_PASSPORT_FILENAME");
                //if (fileNo2.ToCount() > 0)
                //{
                //    var fileObj2 = fileNo2.FirstOrDefault();
                //    //fileObj2.SetValue(model, filedata.FILENAME);
                //    //if (fileList != null)
                //    //{
                //    //    fileObj2.SetValue(model, fileList.FILE_PASSPORT_TEXT);
                //    //}

                //    if (!string.IsNullOrWhiteSpace(filedata.FILENAME))
                //    {
                //        pos = filedata.FILENAME.LastIndexOf("\\");
                //        fileObj2.SetValue(model, filedata.FILENAME.Substring(pos + 1, filedata.FILENAME.Length - pos - 1));
                //    }
                //}


                //醫事人員/專科中文證書電子檔 grid
                model.ATHs = dao.GetAthFile_001008(APP_ID);
                #endregion

                #region 補件備註資料
                // 取回補件備註欄位
                TblAPPLY_NOTICE ntwhere = new TblAPPLY_NOTICE();
                ntwhere.APP_ID = APP_ID;
                ntwhere.ISADDYN = "N";
                var ntdata = dao.GetRowList(ntwhere);

                // 無動態欄位
                var ntLst = new List<string>();
                // 動態欄位(通常適用於檔案)
                var ntLstForList = new List<string>();
                int frequency = 0;
                bool blOpenAll = false;

                foreach (var item in ntdata)
                {
                    frequency = item.FREQUENCY.Value;
                    if (item.SRC_NO.TONotNullString() == "")
                    {
                        ntLst.Add(item.Field);
                    }
                    else
                    {
                        ntLstForList.Add(item.Field);
                    }

                    if (!blOpenAll)
                    {
                        blOpenAll = (item.Field.IndexOf("ALL") >= 0);
                    }
                }

                // 組成字串丟回前端跑JS
                model.FieldStr = string.Join(",", ntLst);
                model.FREQUENCY = frequency;
                #endregion

                ShareDAO shareDAO = new ShareDAO();
                bool blOpenApply = false; //是否開放編輯案件資料 true:開放 / false:不開放

                model.IsNotice = "N";
                //補件通知
                if (applydata.FLOW_CD.Equals("2"))
                {
                    if (shareDAO.CalculationDocDate(srvID, model.APP_ID))
                    {
                        //已過補件期限
                        sm.LastErrorMessage = "已過可補件時間，請聯絡承辦單位!!";
                    }
                    else
                    {
                        model.IsNotice = "Y";
                        if (blOpenAll) //勾其它
                        {
                            blOpenApply = true;
                        }
                    }
                }

                if (blOpenApply)
                {
                    model.IsNew = true;
                    model.TRANS.IsReadOnly = false;
                    model.TRANSF.IsReadOnly = false;
                }
                else
                {
                    model.IsNew = false;
                    model.TRANS.IsReadOnly = true;
                    model.TRANSF.IsReadOnly = true;
                }

                //不提供異動
                model.ME.IsReadOnly = true;
                model.PR.IsReadOnly = true;
                //不提供新增刪除
                //model.TRANS.IsNewOpen = false;
                //model.TRANS.IsDeleteOpen = false;
                //model.TRANSF.IsNewOpen = false;
                //model.TRANSF.IsDeleteOpen = false;

                rtn = View("AppDoc", model);
            }
            catch (Exception ex)
            {
                logger.Error("001008_AppDoc failed:" + ex.TONotNullString());
                sm.LastErrorMessage = "系統發生錯誤";//ex.Message;
                rtn = RedirectToAction("Index", "Login");
            }

            return rtn;
        }
        #endregion

        #region 補件存檔
        /// <summary>
        /// 補件存檔檢核
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_001008FormModel model)
        {
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            string[] fieldStrAry = null;

            if (!string.IsNullOrWhiteSpace(model.FieldStr))
            {
                fieldStrAry = model.FieldStr.Split(',');
            }

            ModelState.Clear();

            foreach (var item in fieldStrAry)
            {
                this.AppDocFormValidation(model, item);
            }

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                //ErrorMsg = model.FileSave(); //submit方式

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
        /// 補件儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Done(Apply_001008FormModel model)
        {
            ActionResult rtn = null;
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            string srvID = "001008";
            string srvName = "醫事人員或公共衛生師請領英文證明書";
            rtn = View("AppDoc", model);

            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;

            try
            {
                if (dao.SaveAppDoc001008(model))
                {
                    dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "醫事人員或公共衛生師請領英文證明書", "001008", "1");
                    //sm.LastResultMessage = "存檔成功!!";
                    //model.CODE_CD = "3";
                    //model.STATUS = "A";
                    rtn = Json(new { status = true, msg = "存檔成功", data = new Apply_001008FormModel { SRV_ID = srvID, APP_ID = model.APP_ID, CODE_CD = "3", STATUS = "A" } }); ;
                }
                else
                {
                    sm.LastErrorMessage = "存檔失敗!!";
                    //rtn = View("AppDoc", model);
                    rtn = Json(new { status = false, msg = "存檔失敗" });
                }
            }
            catch (Exception ex)
            {
                logger.Error("001008_Done failed:" + ex.TONotNullString());
                //sm.LastErrorMessage = "存檔失敗!!";
                rtn = Json(new { status = false, msg = "存檔失敗" });
            }

            return rtn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult DoneFinish(string status)
        {
            Apply_001008FormModel model = new Apply_001008FormModel();
            model.STATUS = status;
            model.CODE_CD = "3";

            return View("Save", model);
        }

        /// <summary>
        /// 補件資料檢核
        /// </summary>
        /// <param name="model"></param>
        public void AppDocFormValidation(Apply_001008FormModel model, string item)
        {
            var idx = 1;
            var blTransExists = false;
            var blTransFExists = false;

            bool blFile1 = false;
            bool blFile2 = false;
            string errMsg = "";

            if (item == "FILE_1" || item.IndexOf("ALL_") >= 0)
            {
                if (!blFile1)
                {
                    blFile1 = true;

                    if ((string.IsNullOrWhiteSpace(model.HAS_OFILE_PAYPASSPORT) || !"Y".Equals(model.HAS_OFILE_PAYPASSPORT)) && model.FILE_PASSPORT == null)
                    {
                        ModelState.AddModelError("FILE_PAYPASSPORT_CHECK", "請上傳護照影本電子檔");
                    }
                }
            }

            if (item == "FILE_2" || item.IndexOf("ALL_") >= 0)
            {
                if (!blFile2)
                {
                    blFile2 = true;
                    errMsg = model.chkAthFile();

                    if (!string.IsNullOrWhiteSpace(errMsg))
                    {
                        ModelState.AddModelError("ATH_CHECK", errMsg);
                    }
                }

            }

            if (item.IndexOf("ALL_") >= 0)
            {
                if (string.IsNullOrWhiteSpace(model.ENAME))
                {
                    ModelState.AddModelError("ENAME_CHECK", "申請人英文姓名 為必填欄位");
                }

                //if (string.IsNullOrWhiteSpace(model.ENAME_ALIAS))
                //{
                //    ModelState.AddModelError("ENAME_ALIAS_CHECK", "申請人英文別名 為必填欄位");
                //}

                if (string.IsNullOrWhiteSpace(model.ADDR_ZIP_ADDR) && string.IsNullOrWhiteSpace(model.ADDR_ZIP_DETAIL))
                {
                    ModelState.AddModelError("ADDR_CHECK", "通訊地址 為必填欄位");
                }
                else if (string.IsNullOrWhiteSpace(model.ADDR_ZIP_ADDR) && string.IsNullOrWhiteSpace(model.ADDR_ZIP_DETAIL))
                {
                    ModelState.AddModelError("ADDR_CHECK", "通訊地址 請填寫完整");
                }

                //if (string.IsNullOrWhiteSpace(model.TEL))
                //{
                //    ModelState.AddModelError("TEL_CHECK", "電話 為必填欄位");
                //}
                //20201221 電話改為非必填，有填時區碼及電話號碼為必填
                if (!string.IsNullOrWhiteSpace(model.TEL_Zip) || !string.IsNullOrWhiteSpace(model.TEL_Phone) || !string.IsNullOrWhiteSpace(model.TEL_Num))
                {
                    if (string.IsNullOrWhiteSpace(model.TEL_Zip) || string.IsNullOrWhiteSpace(model.TEL_Phone))
                    {
                        ModelState.AddModelError("TEL_MSG", "電話 尚未填寫完整。");
                    }
                }

                if (string.IsNullOrWhiteSpace(model.MOBILE))
                {
                    ModelState.AddModelError("MOBILE_CHECK", "行動電話 為必填欄位");
                }

                if (string.IsNullOrWhiteSpace(model.EMAIL_1) || (model.EMAIL_2 == "0" && string.IsNullOrWhiteSpace(model.EMAIL_3)))
                {
                    ModelState.AddModelError("EMAIL_CHECK", "EMAIL 尚未填寫完整。");
                }
                else if (!HelperUtil.IsEmail(model.EMAIL))
                {
                    ModelState.AddModelError("EMAIL_CHECK", "EMAIL 格式填寫錯誤。");
                }

                if (model.TRANS != null)
                {
                    idx = 1;
                    foreach (var item1 in model.TRANS.GoodsList)
                    {
                        if (!string.IsNullOrWhiteSpace(item1.TRANS_ZIP) || !string.IsNullOrWhiteSpace(item1.TRANS_ZIP_DETAIL) || item1.TRANS_COPIES != null)
                        {
                            blTransExists = true;

                            if (string.IsNullOrWhiteSpace(item1.TRANS_ZIP) && string.IsNullOrWhiteSpace(item1.TRANS_ZIP_DETAIL))
                            {
                                ModelState.AddModelError("TRANS_ADDR_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國內寄送地址 欄位是必要項。", idx.ToString()));
                            }
                            else if (string.IsNullOrWhiteSpace(item1.TRANS_ZIP) || string.IsNullOrWhiteSpace(item1.TRANS_ZIP_DETAIL))
                            {
                                ModelState.AddModelError("TRANS_ADDR_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國內寄送地址 尚未填寫完整", idx.ToString()));
                            }

                            if (item1.TRANS_COPIES == null)
                            {
                                ModelState.AddModelError("TRANS_COPIES_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國外寄送份數 欄位是必要項。", idx.ToString()));
                            }
                            else if (item1.TRANS_COPIES <= 0)
                            {
                                ModelState.AddModelError("TRANS_COPIES_" + idx.ToSingle(), String.Format("國內寄送地址 第 {0} 筆 國外寄送份數 請填寫大於0的件數。", idx.ToString()));
                            }
                        }

                        idx++;

                    }
                }

                //檢核是否有填國外寄送地址資料
                if (model.TRANSF != null)
                {
                    idx = 1;
                    foreach (var item2 in model.TRANSF.GoodsList)
                    {
                        if (!string.IsNullOrWhiteSpace(item2.TRANSF_ADDR) || item2.TRANSF_COPIES != null)
                        {
                            blTransFExists = true;

                            if (string.IsNullOrWhiteSpace(item2.TRANSF_ADDR))
                            {
                                ModelState.AddModelError("TRANSF_ADDR_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 國外寄送地址 欄位是必要項。", idx.ToString()));
                            }

                            //20201221 add [MOHES-890]醫事人員請領英文證明書-001008-(前台)申辦案件的國外寄送地址區塊增加一個機構名稱的欄位
                            if (string.IsNullOrWhiteSpace(item2.TRANSF_UNITNAME))
                            {
                                ModelState.AddModelError("TRANSF_UNITNAME_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 機構名稱 欄位是必要項。", idx.ToString()));
                            }

                            if (item2.TRANSF_COPIES == null)
                            {
                                ModelState.AddModelError("TRANSF_COPIES_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 國外寄送份數 欄位是必要項。", idx.ToString()));
                            }
                            else if (item2.TRANSF_COPIES <= 0)
                            {
                                ModelState.AddModelError("TRANSF_COPIES_" + idx.ToSingle(), String.Format("國外寄送地址 第 {0} 筆 國外寄送份數 請填寫大於0的件數。", idx.ToString()));
                            }
                        }

                        idx++;
                    }
                }

                if (!blTransExists && !blTransFExists)
                {
                    ModelState.AddModelError("TRANS", "國內外寄送地址請至少擇一輸入");
                }

                if (string.IsNullOrWhiteSpace(model.MERGEYN))
                {
                    ModelState.AddModelError("EMAIL_CHECK", "佐證文件採合併檔案 為必填欄位");
                }
            }
        }
        #endregion


    }
}
