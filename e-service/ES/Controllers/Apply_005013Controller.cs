using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models.ViewModels;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Xceed.Words.NET;
using Xceed.Document.NET;
using ES.Models;

namespace ES.Controllers
{
    public class Apply_005013Controller : BaseController
    {
        public static string s_SRV_ID = "005013";
        public static string s_SRV_NAME = "民眾少量自用中藥貨品進口";

        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt005013");
        }

        [DisplayName("005013_申請案件")]
        public ActionResult Apply(string agree)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_005013FormModel Form = new Apply_005013FormModel();
            ActionResult rtn = View("Index", Form);

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

            Form.APP_TIME_TW = HelperUtil.DateTimeToTwString(DateTime.Now);

            #region 取得USERIFNO
            if (sm.UserInfo != null)
            {
                Form.NAME = sm.UserInfo.Member.NAME;
                Form.IDN = sm.UserInfo.Member.IDN;
                Form.MOBILE = sm.UserInfo.Member.MOBILE;

                //地址
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = sm.UserInfo.Member.TOWN_CD;
                var address = dao.GetRow(zip);
                Form.TAX_ORG_CITY_CODE = sm.UserInfo.Member.TOWN_CD;
                if (address != null)
                {
                    Form.TAX_ORG_CITY_TEXT = address.TOWNNM;
                    Form.TAX_ORG_CITY_DETAIL = sm.UserInfo.Member.ADDR.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                }

                // 電話
                if (sm.UserInfo.Member.TEL.TONotNullString().Trim() != "")
                {
                    string[] tel = sm.UserInfo.Member.TEL.TONotNullString().Split('-');
                    Form.TEL_BEFORE = tel[0];
                    Form.TEL_AFTER = tel[1].ToSplit('#')[0];

                    if (sm.UserInfo.Member.TEL.IndexOf('#') > 0)
                    {
                        Form.TEL_Extension = sm.UserInfo.Member.TEL.Split('#')[1];
                    }
                }

                Form.EMAIL = sm.UserInfo.Member.MAIL;
            }
            #endregion

            return View("Index", Form);
        }

        [HttpPost]
        [DisplayName("005013_申請案件欄位驗證")]
        public ActionResult Apply(Apply_005013FormModel Form)
        {
            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            string ErrorMsg = "";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\'\:\：]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");

            if (ModelState.IsValid)
            {
                if (!reg3.IsMatch(Form.EMAIL))
                {
                    ErrorMsg += "請填入正確的Email格式 ! \r\n";
                }
                if (Form.CSEE_TYPE == "Y")
                {
                    //事前申請
                }
                else
                {
                    ////補辦案件:
                    //if (string.IsNullOrEmpty(Form.File_1_Name))
                    //{
                    //    ErrorMsg += "國際包裹招領單或海關補辦驗關通關手續通知書之正面影本 為必要附件檔。\r\n";
                    //}

                    //if (string.IsNullOrEmpty(Form.File_2_Name))
                    //{
                    //    ErrorMsg += "國際包裹招領單或海關補辦驗關通關手續通知書之反面影本 為必要附件檔。\r\n";
                    //}
                }

                //申請項目欄位驗證
                foreach (var item in Form.newApplyItem)
                {
                    if (string.IsNullOrEmpty(item.PorcType))
                    {
                        ErrorMsg += "申請項目項次 " + item.ItemNum + " 產品類別 為必填欄位。\r\n";
                    }

                    if (string.IsNullOrEmpty(item.Commodities))
                    {
                        ErrorMsg += "申請項目項次 " + item.ItemNum + " 貨名 為必填欄位。\r\n";
                    }

                    if (string.IsNullOrEmpty(item.CommodType))
                    {
                        ErrorMsg += "申請項目項次 " + item.CommodType + " 劑型 為必填欄位。\r\n";
                    }

                    if (string.IsNullOrEmpty(item.Qty))
                    {
                        ErrorMsg += "申請項目項次 " + item.ItemNum + " 申請數量 為必填欄位。\r\n";
                    }

                    if (string.IsNullOrEmpty(item.Unit))
                    {
                        ErrorMsg += "申請項目項次 " + item.ItemNum + " 申請數量單位 為必填欄位。\r\n";
                    }

                    if (string.IsNullOrEmpty(item.SpecQty))
                    {
                        ErrorMsg += "申請項目項次 " + item.ItemNum + " 規格數量 為必填欄位。\r\n";
                    }

                    if (string.IsNullOrEmpty(item.SpecUnit))
                    {
                        ErrorMsg += "申請項目項次 " + item.ItemNum + " 規格數量單位 為必填欄位。\r\n";
                    }

                }

                //民眾輸入自用中藥切結書申請項目
                foreach (var item in Form.ApplyItem2)
                {
                    if (string.IsNullOrEmpty(item.Usage))
                    {
                        ErrorMsg += "民眾輸入自用中藥切結書申請項目項次 " + item.ItemNum + " 用法 為必填欄位。\r\n";
                    }
                    // 總數量
                }

                if (Form.RADIOUSAGE == "3")
                {
                    if (string.IsNullOrEmpty(Form.RADIOUSAGE_TEXT))
                    {
                        ErrorMsg += "個人用途其他說明 為必填欄位。\r\n";
                    }
                }

                // 佐證文件檔案上傳
                if (Form.RADIOYN == "N")
                {
                    if (string.IsNullOrEmpty(Form.File_4_Name))
                    {
                        ErrorMsg += "佐證文件檔案：產品外盒、仿單、說明書 為必要欄位。\r\n";
                    }
                    if(string.IsNullOrEmpty(Form.File_5_Name))
                    {
                        ErrorMsg += "佐證文件檔案：身分證影本(正面) 為必要欄位。\r\n";
                    }
                    if (string.IsNullOrEmpty(Form.File_6_Name))
                    {
                        ErrorMsg += "佐證文件檔案：身分證影本(反面) 為必要欄位。\r\n";
                    }
                }
                else if (Form.RADIOYN == "Y")
                {
                    if (string.IsNullOrEmpty(Form.File_1_Name) && string.IsNullOrEmpty(Form.File_2_Name) && string.IsNullOrEmpty(Form.File_3_Name) && string.IsNullOrEmpty(Form.File_4_Name) && string.IsNullOrEmpty(Form.File_5_Name) && string.IsNullOrEmpty(Form.File_6_Name))
                    {
                        ErrorMsg += "佐證文件檔案上傳 為必要欄位。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(Form.File_1_Name))
                {
                    logger.Debug("Apply_005013.FileName:" + Form.File_1_Name);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip","doc","docx","odt","odf", "ods","xls","xlsx","ppt","pptx" };
                    // 取得副檔名並轉小寫
                    var ext = Form.File_1_Name.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }
                if (!string.IsNullOrEmpty(Form.File_2_Name))
                {
                    logger.Debug("Apply_005013.FileName:" + Form.File_2_Name);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                    // 取得副檔名並轉小寫
                    var ext = Form.File_2_Name.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }
                if (!string.IsNullOrEmpty(Form.File_3_Name))
                {
                    logger.Debug("Apply_005013.FileName:" + Form.File_3_Name);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                    // 取得副檔名並轉小寫
                    var ext = Form.File_3_Name.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }
                if (!string.IsNullOrEmpty(Form.File_4_Name))
                {
                    logger.Debug("Apply_005013.FileName:" + Form.File_4_Name);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                    // 取得副檔名並轉小寫
                    var ext = Form.File_4_Name.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }
                if (!string.IsNullOrEmpty(Form.File_5_Name))
                {
                    logger.Debug("Apply_005013.FileName:" + Form.File_5_Name);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                    // 取得副檔名並轉小寫
                    var ext = Form.File_5_Name.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }
                if (!string.IsNullOrEmpty(Form.File_6_Name))
                {
                    logger.Debug("Apply_005013.FileName:" + Form.File_6_Name);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                    // 取得副檔名並轉小寫
                    var ext = Form.File_6_Name.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }

                if(Form.ApplyFile!=null && Form.ApplyFile.Count > 0)
                {
                    foreach(var item in Form.ApplyFile)
                    {
                        if (!string.IsNullOrEmpty(item.FileName_TEXT))
                        {
                            // 允許的附檔名（全小寫）
                            var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip", "doc", "docx", "odt", "odf", "ods", "xls", "xlsx", "ppt", "pptx" };
                            // 取得副檔名並轉小寫
                            var ext = item.FileName_TEXT.ToSplit(".").LastOrDefault().ToLower();
                            if (!validExts.Contains(ext))
                            {
                                ErrorMsg += "不支援的檔案格式";
                            }
                        }
                    }
                }

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

        [HttpPost]
        [DisplayName("005013_申請案件預覽送出")]
        public ActionResult PreView(Apply_005013FormModel Form)
        {
            Form.ADDR = Form.TAX_ORG_CITY_TEXT + Form.TAX_ORG_CITY_DETAIL;

            return View("PreView005013", Form);
        }

        [HttpPost]
        [DisplayName("005013_申請案件完成")]
        public ActionResult Save(Apply_005013FormModel Form)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            Form.APP_ID = dao.AppendApply005013(Form);

            string MailBody = "<table align=\"left\" style=\"width:90%;\">";
            MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
            MailBody += " <tr><td>貴公司申請民眾少量自用中藥貨品進口(案號：" + Form.APP_ID + ")一案，「人民申請案線上申辦服務系統」已完成系統資料填答及上傳程序，將儘速辦理您的申請案件，謝謝。</td></tr>";
            MailBody += " <tr><td><br /></td></tr>";
            MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
            MailBody += "</table>";

            dao.SendMail_New(Form.NAME, Form.EMAIL, Form.APP_ID, "民眾少量自用中藥貨品進口", "005013", MailBody);

            sm.LastResultMessage = "送出成功";
            return View("Save", Form);
        }

        [DisplayName("005013_補件進入")]
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005013ViewModel model = new Apply_005013ViewModel();

            model.FormBack = dao.QueryApply_005013(APP_ID);
            if (model.FormBack.TEL.TONotNullString().ToTrim() != "")
            {
                var Tel = model.FormBack.TEL.Split('-');
                model.FormBack.TEL_BEFORE = Tel[0];
                if (Tel.ToCount() > 1)
                {
                    model.FormBack.TEL_AFTER = Tel[1];
                    if (model.FormBack.TEL_AFTER.IndexOf('#') > 0)
                    {
                        model.FormBack.TEL_AFTER = Tel[1].Substring(0, Tel[1].IndexOf('#'));
                        model.FormBack.TEL_Extension = Tel[1].Split('#')[1];
                    }
                }

            }
            if (model.FormBack.EMAIL.TONotNullString().Trim() != "")
            {
                var mail = model.FormBack.EMAIL;
                model.FormBack.EMAIL = mail.Split('@')[0];
                model.FormBack.EMAIL_0 = "0";
                model.FormBack.EMAIL_2 = "0";
                model.FormBack.EMAIL_1 = mail.Split('@')[1];

                switch (mail.Split('@')[1])
                {
                    case "gmail.com":
                        model.FormBack.EMAIL_0 = "1";
                        model.FormBack.EMAIL_2 = "1";
                        model.FormBack.EMAIL_1 = "";
                        break;
                    case "yahoo.com.tw":
                        model.FormBack.EMAIL_0 = "2";
                        model.FormBack.EMAIL_2 = "2";
                        model.FormBack.EMAIL_1 = "";
                        break;
                    case "outlook.com":
                        model.FormBack.EMAIL_0 = "3";
                        model.FormBack.EMAIL_2 = "3";
                        model.FormBack.EMAIL_1 = "";
                        break;
                }
            }
            model.Detail = dao.GetApplyNotice_005013(APP_ID);

            model.FormFile = dao.QueryApplyFileList_005013(APP_ID);


            if (model.FormBack.CODE_CD == "2" || model.FormBack.CODE_CD == "4")
            {
                if (shareDao.CalculationDocDate("005013", APP_ID))
                {
                    //已過補件期限
                    return View("Detail005013", model);
                }
                else
                {
                    if (model.Detail.FormFile.ToCount() > 0)
                    {
                        if (model.FormFile.ToCount() == 0)
                        {
                            ApplyFileItem4Model f4 = new ApplyFileItem4Model();
                            model.FormFile.Add(f4);
                        }
                    }

                    return View("AppDoc", model);
                }
            }
            else
            {
                return View("Detail005013", model);
            }
        }

        [HttpPost]
        [DisplayName("005013_補件驗證欄位")]
        public ActionResult DocSave(Apply_005013ViewModel model)
        {
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_CODE))
            {
                ErrorMsg += "地址區碼 為必填欄位。";
            }

            if (string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_TEXT))
            {
                ErrorMsg += "地址區域 為必填欄位。";
            }

            if (string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_DETAIL))
            {
                ErrorMsg += "地址 為必填欄位。";
            }

            if (string.IsNullOrEmpty(model.FormBack.TEL_BEFORE) || string.IsNullOrEmpty(model.FormBack.TEL_AFTER))
            {
                ErrorMsg += "電話Tel. No. 為必填欄位。";
            }

            if(string.IsNullOrEmpty(model.FormBack.MOBILE))
            {
                ErrorMsg += "行動電話 為必要欄位。";
            }
            //if (model.FormBack.CSEE_TYPE == "Y")
            //{
            //    //事前申請
            //}
            //else
            //{
            //    //補辦案件:
            //    if (string.IsNullOrEmpty(model.FormBack.File_1_Name))
            //    {
            //        ErrorMsg += "國際包裹招領單或海關補辦驗關通關手續通知書之正面影本 為必要附件檔。\r\n";
            //    }

            //    if (string.IsNullOrEmpty(model.FormBack.File_2_Name))
            //    {
            //        ErrorMsg += "國際包裹招領單或海關補辦驗關通關手續通知書之反面影本 為必要附件檔。\r\n";
            //    }
            //}
            //申請項目欄位驗證
            foreach (var item in model.FormBack.FormApply005013Item1)
            {
                if (string.IsNullOrEmpty(item.PorcType))
                {
                    ErrorMsg += "申請項目項次 " + item.ItemNum + " 產品類別 為必填欄位。\r\n";
                }

                if (string.IsNullOrEmpty(item.Commodities))
                {
                    ErrorMsg += "申請項目項次 " + item.ItemNum + " 貨名 為必填欄位。\r\n";
                }

                if (string.IsNullOrEmpty(item.CommodType))
                {
                    ErrorMsg += "申請項目項次 " + item.CommodType + " 劑型 為必填欄位。\r\n";
                }

                if (string.IsNullOrEmpty(item.Qty))
                {
                    ErrorMsg += "申請項目項次 " + item.ItemNum + " 申請數量 為必填欄位。\r\n";
                }

                if (string.IsNullOrEmpty(item.Unit))
                {
                    ErrorMsg += "申請項目項次 " + item.ItemNum + " 申請數量單位 為必填欄位。\r\n";
                }

                if (string.IsNullOrEmpty(item.SpecQty))
                {
                    ErrorMsg += "申請項目項次 " + item.ItemNum + " 規格數量 為必填欄位。\r\n";
                }

                if (string.IsNullOrEmpty(item.SpecUnit))
                {
                    ErrorMsg += "申請項目項次 " + item.ItemNum + " 規格數量單位 為必填欄位。\r\n";
                }

            }

            //民眾輸入自用中藥切結書申請項目
            foreach (var item in model.FormBack.FormApply005013Item2)
            {
                if (string.IsNullOrEmpty(item.Usage))
                {
                    ErrorMsg += "民眾輸入自用中藥切結書申請項目項次 " + item.ItemNum + " 用法 為必填欄位。\r\n";
                }
                // 總數量
            }

            //個人用途

            if (model.FormBack.RADIOUSAGE == "3")
            {
                if (string.IsNullOrEmpty(model.FormBack.RADIOUSAGE_TEXT))
                {
                    ErrorMsg += "個人用途其他說明 為必填欄位。\r\n";
                }
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

            return Content(result.Serialize(), "application/json");
        }

        [HttpPost]
        [DisplayName("005013_補件存檔")]
        public ActionResult DocFinish(Apply_005013ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            string ErrorMsg = "";

            model.Detail = dao.GetApplyNotice_005013(model.FormBack.APP_ID);
            ErrorMsg = dao.AppendApplyDoc005013(model);

            Apply_005013FormModel Form = new Apply_005013FormModel();
            Form.DOCYN = "Y";
            Form.DOCCOUNT = dao.GetNoticeCount(model.FormBack.APP_ID).ToString();

            if (ErrorMsg == "")
            {
                dao.SendMail_Update(model.FormBack.NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, "民眾少量自用中藥貨品進口", "005013", Form.DOCCOUNT);
                return View("Save", Form);
            }
            else
            {
                sm.LastErrorMessage = ErrorMsg;
                return AppDoc(model.Form.APP_ID);
            }
        }

        /// <summary>
        /// 005013_申請單套表"
        /// </summary>
        /// <param name="model"></param>
        [DisplayName("005013_申請單套表")]
        public void PreviewApplyForm_1(Apply_005013FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string path = Server.MapPath("~/Sample/apply005013_1.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    var APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                    var APPLY_TYPE2 = "□保健養生";
                    var APPLY_TYPE3 = "□其他：(請說明)";
                    switch (model.RADIOUSAGE.TONotNullString())
                    {
                        case "1":
                            APPLY_TYPE1 = "■疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "□保健養生";
                            APPLY_TYPE3 = "□其他：(請說明)";
                            break;
                        case "2":
                            APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "■保健養生";
                            APPLY_TYPE3 = "□其他：(請說明)";
                            break;
                        case "3":
                            APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "□保健養生";
                            APPLY_TYPE3 = "■其他：(請說明)" + model.RADIOUSAGE_TEXT.TONotNullString();
                            break;
                        default:
                            break;
                    }

                    // 替換文字
                    doc.ReplaceText("[$YEAR]", ((DateTime.Now.Year) - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (DateTime.Now.Month).TONotNullString().Length < 2 ? "0" + (DateTime.Now.Month).TONotNullString() : (DateTime.Now.Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (DateTime.Now.Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$NAME]", model.NAME, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE1]", APPLY_TYPE1, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE2]", APPLY_TYPE2, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE3]", APPLY_TYPE3, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", model.IDN, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$PHONE]", model.TEL, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$ADDR]", model.TAX_ORG_CITY_TEXT + model.TAX_ORG_CITY_DETAIL, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$NAME1]", model.NAME + "線上申請案，案件編號:" + model.APP_ID.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    // 動態表格
                    var tb = doc.AddTable(model.ApplyItem2.ToCount() + 1, 4);
                    tb.Paragraphs[0].FontSize(11);
                    tb.Paragraphs[0].Font("標楷體");
                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Rows[0].Cells[0].Paragraphs[0].Append("").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[1].Paragraphs[0].Append("藥品名稱").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[2].Paragraphs[0].Append("用法").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[3].Paragraphs[0].Append("總數量").Font("標楷體").Alignment = Alignment.center;
                    var q = 1;
                    foreach (var item in model.ApplyItem2)
                    {
                        tb.Rows[q].Cells[0].Paragraphs[0].Append(q.TONotNullString()).Font("標楷體").Alignment = Alignment.center;
                        tb.Rows[q].Cells[1].Paragraphs[0].Append(item.ItemName.TONotNullString()).Font("標楷體");
                        tb.Rows[q].Cells[2].Paragraphs[0].Append(item.Usage.TONotNullString()).Font("標楷體");
                        var allQty = $"共{item.ItemQty}{item.ItemQtyUnit}，每{item.ItemQtyUnit2}{item.ItemSpecQty}{item.ItemSpecQtyUnit}";
                        tb.Rows[q].Cells[3].Paragraphs[0].Append(allQty.TONotNullString()).Font("標楷體");
                        q++;
                    }
                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=個人少量自用貨品進口切結書.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// 005013_申請單套表"
        /// </summary>
        /// <param name="model"></param>
        [DisplayName("005013_申請單套表")]
        public void PreviewApplyForm_2(Apply_005013FormModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string path = Server.MapPath("~/Sample/apply005013_2.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    doc.ReplaceText("[$YEAR]", ((DateTime.Now.Year) - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (DateTime.Now.Month).TONotNullString().Length < 2 ? "0" + (DateTime.Now.Month).TONotNullString() : (DateTime.Now.Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (DateTime.Now.Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLICANT]", model.NAME, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", model.IDN, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$AATO]", model.TAX_ORG_CITY_TEXT + model.TAX_ORG_CITY_DETAIL + "\n" + model.TEL, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COO]", model.ProductionCountry_TEXT, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SP]", model.ShippingPort_TEXT, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COS]", model.SellerCountry_TEXT, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLYMAN]", model.NAME + "線上申請案\n案件編號:" + model.APP_ID.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    var tb = doc.AddTable(model.newApplyItem.ToCount() + 1, 5);
                    tb.Paragraphs[0].FontSize(11);
                    tb.Paragraphs[0].Font("標楷體");
                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Rows[0].Cells[0].Paragraphs[0].Append("⑦項次\n Item").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[1].Paragraphs[0].Append("⑧貨名、規格、廠牌及製造廠名稱\n Description of Commodities Spec.and Brand or Maker, etc.").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[2].Paragraphs[0].Append("⑨貨品分類稅則號列\n C.C.C.Code").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[3].Paragraphs[0].Append("⑩數量\n Q'ty").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[4].Paragraphs[0].Append("⑪單位\n Unit").FontSize(11).Font("標楷體"); ;
                    var q = 1;
                    foreach (var item in model.newApplyItem)
                    {
                        tb.Rows[q].Cells[0].Paragraphs[0].Append(q.TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[1].Paragraphs[0].Append(item.Commodities.TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[2].Paragraphs[0].Append("").FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[3].Paragraphs[0].Append(item.Qty.TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[4].Paragraphs[0].Append(item.Unit_TEXT.TONotNullString() + "\n" + item.SpecQty.TONotNullString() + item.SpecUnit_TEXT.TONotNullString()).FontSize(12).Font("標楷體");
                        q++;
                    }
                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=民眾少量自用中藥貨品進口申請單.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }
    }
}
