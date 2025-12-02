using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Commons;
using ES.Services;
using System.ComponentModel;
using ES.DataLayers;
using Omu.ValueInjecter;
using System.Net.Mail;
using ES.Action;
using ES.Extensions;
using System.Data.SqlClient;
using ES.Action.Form;
using ES.Utils;

namespace ES.Controllers
{
    public class Apply_001010Controller : BaseController
    {
        public static string s_SRV_ID = "001010";
        public static string s_SRV_NAME = "醫事人員或公共衛生師證書影本申請書";

        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt001010");
        }

        public ActionResult Index(Apply_001010ViewModel model)
        {
            return View();
        }

        [DisplayName("Apply_001010_申請")]
        public ActionResult Apply(string agree)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001010ViewModel form = new Apply_001010ViewModel();

            ActionResult rtn = View("Index", form);

            if (sm == null || sm.UserInfo == null)
            {
                rtn = RedirectToAction("Index", "Login");
                return rtn;
            }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null)
            {
                rtn = RedirectToAction("Index", "Login");
                return rtn;
            }
            //agree: 1:同意新增 /other:請先閱讀規章
            if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            if (agree != null && !agree.Equals("1")) { return Prompt(); }


            #region UserInfo

            if (sm.UserInfo != null)
            {
                form.APPLY_NAME = sm.UserInfo.Member.NAME;
                form.APPLY_PID = sm.UserInfo.Member.IDN;
                form.BIRTHDAY_AC = HelperUtil.DateTimeToString(sm.UserInfo.Member.BIRTHDAY);
                form.MOBILE = sm.UserInfo.Member.MOBILE;

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = sm.UserInfo.Member.TOWN_CD;
                var address = dao.GetRow(zip);
                form.CITY_CODE = sm.UserInfo.Member.TOWN_CD;
                if (address != null && !string.IsNullOrEmpty(address.TOWNNM))
                {
                    form.CITY_TEXT = address.TOWNNM;
                    form.CITY_DETAIL = sm.UserInfo.Member.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }
                else
                {
                    form.CITY_TEXT = string.Empty;
                    form.CITY_DETAIL = sm.UserInfo.Member.ADDR;
                }

                // 電話
                if (sm.UserInfo.Member.TEL.TONotNullString().Trim() != "")
                {
                    string[] tel = sm.UserInfo.Member.TEL.TONotNullString().Split('-');
                    form.TEL_SEC = tel[0];
                    form.TEL_NO = tel[1].ToSplit('#')[0];

                    if (sm.UserInfo.Member.TEL.IndexOf('#') > 0)
                    {
                        form.TEL_EXT = sm.UserInfo.Member.TEL.Split('#')[1];
                    }
                }

                //Mail
                if (sm.UserInfo.Member.MAIL.TONotNullString().Trim() != "")
                {
                    form.MAIL_ACCOUNT = sm.UserInfo.Member.MAIL.Split('@')[0];
                    form.MAIL_DOMAIN = sm.UserInfo.Member.MAIL.Split('@')[1];

                    switch (sm.UserInfo.Member.MAIL.Split('@')[1])
                    {
                        case "gmail.com":
                            form.DOMAINList = "1";
                            break;
                        case "yahoo.com.tw":
                            form.DOMAINList = "2";
                            break;
                        case "outlook.com":
                            form.DOMAINList = "3";
                            break;
                        default:
                            form.DOMAINList = "0";
                            break;
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

            #endregion

            form.APPLY_DATE = HelperUtil.DateTimeToTwString(DateTime.Now);
            return View(form);
        }

        [HttpPost]
        public ActionResult ApplyAjax(Apply_001010ViewModel view)
        {
            var result = new AjaxResultStruct();
            var errmsg = string.Empty;
            if(view.COPIES.TOInt32() <= 0)
            {
                errmsg += "請輸入申請數量<br>";
            }
            if(view.TOTAL_MEM.TOInt32() <= 0)
            {
                errmsg += "請輸入申請數量<br>";
            }
            else if(view.TOTAL_MEM.TOInt32()/20 != view.COPIES.TOInt32())
            {
                errmsg += "請重新輸入申請數量，一份為20元計<br>";
            }
            if (string.IsNullOrEmpty(view.DIVISION))
            {
                errmsg += "請輸入證書類別<br>";
            }
            if (string.IsNullOrEmpty(view.ISSUE_DEPT))
            {
                errmsg += "請輸入核發單位<br>";
            }
            if (string.IsNullOrEmpty(view.LIC_CD) || string.IsNullOrEmpty(view.LIC_NUM) || string.IsNullOrEmpty(view.LIC_TYPE))
            {
                errmsg += "請輸入證書字號<br>";
            }
            if (string.IsNullOrEmpty(view.ISSUE_DATE_AC))
            {
                errmsg += "請輸入核發日期<br>";
            }
            if (string.IsNullOrEmpty(view.CITY_CODE)||string.IsNullOrEmpty(view.CITY_DETAIL))
            {
                errmsg += "請輸入通訊地址<br>";
            }
            if (string.IsNullOrEmpty(view.MOBILE)||string.IsNullOrEmpty(view.TEL))
            {
                errmsg += "請輸入行動電話/連絡電話<br>";
            }
            if (string.IsNullOrEmpty(view.MAIL))
            {
                errmsg += "請輸入EMAIL<br>";
            }
            if (string.IsNullOrEmpty(errmsg))
            {
                result.message = "";
                result.status = true;
            }else
            {
                result.message = errmsg;
                result.status = false;
            }
            
            return Content(result.Serialize(), "application/json");
        }

        [DisplayName("Apply_001010_預覽")]
        [HttpPost]
        public ActionResult PreView(Apply_001010ViewModel model)
        {
            ShareDAO dao = new ShareDAO();
            model.PreView = new Apply_001010ViewModel();
            model.PreView.InjectFrom(model);

            return PartialView("PreView001010", model);
        }

        [DisplayName("Apply_001010_繳費")]
        [HttpPost]
        public ActionResult Pay(Apply_001010ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            model.PreView = new Apply_001010ViewModel();
            model.PreView.InjectFrom(model);
            model.PAY_A_FEE = dao.GetApplyFee("001010").TOInt32() * model.COPIES.TOInt32();
            model.PAY_METHOD = "";
            return PartialView("Pay", model);
        }

        [DisplayName("Apply_001010_完成申報")]
        [HttpPost]
        public ActionResult Save(Apply_001010ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.APPLY_NAME) ? sm.UserInfo.Member.NAME : model.APPLY_NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;
            switch (model.PAY_METHOD)
            {
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
                        dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, "醫事人員或公共衛生師證書影本申請書", "001010");
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
                        dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, "醫事人員或公共衛生師證書影本申請書", "001010");
                    }
                    model.ErrorCode = "0000";
                    break;
            }

            return View("Save", model);
        }

        public ActionResult PayPDF(string id)
        {
            ApplyDAO dao = new ApplyDAO();
            return File(dao.ExportPayPDF(id), "application/pdf", "Pay" + id + ".pdf");
        }

        #region 補件查詢

        /// <summary>
        /// 補件查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001010_補件查詢")]
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001010ViewModel model = new Apply_001010ViewModel();
            model = new Apply_001010ViewModel();
            model.APP_ID = APP_ID;
            model = dao.QueryApply_001010(model);

            // 案件基本資訊
            ApplyModel apply = new ApplyModel();
            apply.APP_ID = APP_ID;
            var applyData = dao.GetRow(apply);

            try
            {
                var userInfo = sm.UserInfo.Member;

                // 判斷是否為該案件申請人
                if (applyData.ACC_NO == userInfo.ACC_NO)
                {
                    ShareDAO shareDAO = new ShareDAO();
                    model.IsNotice = "Y";
                    if (applyData.FLOW_CD == "2" && shareDAO.CalculationDocDate("001010", model.APP_ID))
                    {
                        sm.LastErrorMessage = "已過可補件時間，請聯絡承辦單位!!";
                        model.IsNotice = "N";
                    }

                    return View("AppDoc", model);
                }
                else
                {
                    throw new Exception("非案件申請人無法瀏覽次案件 !");
                }
            }
            catch (Exception ex)
            {
                if (model == null)
                {
                    sm.LastErrorMessage = "案件編號﹕" + APP_ID + "，尚未分派承辦人員，暫無法查詢!!";
                    return RedirectToAction("Index", "History");
                }
                else
                {
                    sm.LastErrorMessage = ex.Message;
                    return RedirectToAction("Index", "Login");
                }
            }
        }

        #endregion 

        #region 補件存檔

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001010_補件存檔")]
        [HttpPost]
        public ActionResult AppDocSave(Apply_001010ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            var memberName = string.IsNullOrWhiteSpace(model.APPLY_NAME) ? sm.UserInfo.Member.NAME : model.APPLY_NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;
            try
            {
                if (dao.SaveAppDoc001010(model))
                {
                    dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "醫事人員或公共衛生師證書影本申請書", "001010", "-1");
                    sm.LastResultMessage = "存檔成功!!";
                    model.Apply.FLOW_CD = "3";
                }
                else
                {
                    sm.LastErrorMessage = "存檔失敗!!";
                }
            }
            catch (Exception ex)
            {
                logger.Error("001010_AppDocSave failed:" + ex.TONotNullString());
                sm.LastErrorMessage = "存檔失敗!!";
            }

            return View("Done", model);
        }

        #endregion 
    }
}
