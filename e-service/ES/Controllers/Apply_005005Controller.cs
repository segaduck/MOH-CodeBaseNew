using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
using Xceed.Words.NET;
using System.Net.Mail;
using ES.Models;
using System.Reflection;
using ES.Models.ChineseMedicineAPI;

namespace ES.Controllers
{
    public class Apply_005005Controller : BaseController
    {
        public static string s_SRV_ID = "005005";
        public static string s_SRV_NAME = "中藥GMP廠證明文件(英文)";

        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt005005");
        }

        [DisplayName("005005_申請案件")]
        public ActionResult Apply(string agree)
        {
            ////agree: 1:同意新增 /other:請先閱讀規章
            //if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            //if (agree != null && !agree.Equals("1")) { return Prompt(); }

            Apply_005005FormModel Form = new Apply_005005FormModel();
            Form.APP_TIME_TW = HelperUtil.DateTimeToTwString(DateTime.Now);
            Form.PAYAMOUNT = 1500;
            Form.PAYCOUNT = "1";

            return View("Index", Form);
        }

        [HttpPost]
        [DisplayName("005005_申請案件送出")]
        public ActionResult Apply(Apply_005005FormModel form)
        {
            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\'\.\-\,\s\(\)]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            System.Text.RegularExpressions.Regex reg4 = new System.Text.RegularExpressions.Regex(@"^[\u4e00-\u9fa5]+$");
            System.Text.RegularExpressions.Regex reg5 = new System.Text.RegularExpressions.Regex(@"^[0-9\-\#]+$");

            string ErrorMsg = "";
            if (ModelState.IsValid)
            {
                //個別欄位驗證
                if (!string.IsNullOrEmpty(form.EMAIL))
                {
                    if (!reg3.IsMatch(form.EMAIL))
                    {
                        ErrorMsg += "請填入正確的EMAIL格式。\r\n";
                    }
                }
                if (!string.IsNullOrEmpty(form.CNT_NAME))
                {
                    if (!reg4.IsMatch(form.CNT_NAME))
                    {
                        ErrorMsg += "請填入中文姓名。\r\n";
                    }
                }
                if (!string.IsNullOrEmpty(form.NAME))
                {
                    if (!reg.IsMatch(form.NAME))
                    {
                        ErrorMsg += "公司名稱請以英文或數字填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.IMP_COUNTRY))
                {
                    if (form.IMP_COUNTRY.Trim().Substring(form.IMP_COUNTRY.Trim().Length - 1, 1) != ".")
                    {
                        ErrorMsg += "外銷國家最後請以「.」結尾。\r\n";
                    }

                    if (!reg.IsMatch(form.IMP_COUNTRY))
                    {
                        ErrorMsg += "外銷國家請以英文填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.LIC_NUM))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(form.LIC_NUM.ToUpper(), @"^\(C\)[0-9]{7}$"))
                    {
                        ErrorMsg += "製造廠許可編號輸入錯誤。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.MF_CNT_NAME))
                {
                    if (!reg.IsMatch(form.MF_CNT_NAME))
                    {
                        ErrorMsg += "製造廠名稱請以英文或數字填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.MF_ADDR))
                {
                    if (!reg.IsMatch(form.MF_ADDR))
                    {
                        ErrorMsg += "製造廠地址請以英文或數字填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.FAX_BEFORE) || !string.IsNullOrEmpty(form.FAX_AFTER) || !string.IsNullOrEmpty(form.FAX_Extension))
                {
                    if (!string.IsNullOrEmpty(form.FAX_AFTER) && string.IsNullOrEmpty(form.FAX_BEFORE))
                    {
                        ErrorMsg += "請輸入傳真區碼。\r\n";
                    }
                    else if (!string.IsNullOrEmpty(form.FAX_BEFORE) && string.IsNullOrEmpty(form.FAX_AFTER))
                    {
                        ErrorMsg += "請輸入傳真。\r\n";
                    }
                    else
                    {
                        if (!reg2.IsMatch(form.FAX_BEFORE))
                        {
                            ErrorMsg += "請輸入正確傳真區碼格式。\r\n";
                        }
                        if (!reg2.IsMatch(form.FAX_AFTER))
                        {
                            ErrorMsg += "請輸入正確傳真格式。\r\n";
                        }
                        if (!string.IsNullOrEmpty(form.FAX_Extension))
                        {
                            if (!reg2.IsMatch(form.FAX_Extension))
                            {
                                ErrorMsg += "請輸入正確傳真分機格式。\r\n";
                            }
                        }
                    }

                }

                if (!string.IsNullOrEmpty(form.TEL))
                {
                    if (!reg5.IsMatch(form.TEL))
                    {
                        ErrorMsg += "請輸入正確電話格式。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.Name_File_1))
                {
                    logger.Debug("Apply_005005.FileName:" + form.Name_File_1);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = form.Name_File_1.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }
                if (!string.IsNullOrEmpty(form.Name_File_2))
                {
                    logger.Debug("Apply_005005.FileName:" + form.Name_File_2);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = form.Name_File_2.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
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
        [DisplayName("005005_申請案件預覽送出")]
        public ActionResult PreView(Apply_005005FormModel Form)
        {
            if (Form.FAX_BEFORE.TONotNullString() != "")
            {
                Form.FAX = Form.FAX_Extension.TONotNullString().ToTrim() != "" ?
                Form.FAX_BEFORE.TONotNullString() + "-" + Form.FAX_AFTER.TONotNullString() + "#" + Form.FAX_Extension.TONotNullString() :
                Form.FAX_BEFORE.TONotNullString() + "-" + Form.FAX_AFTER.TONotNullString();
            }

            return View("PreView005005", Form);
        }

        [HttpPost]
        [DisplayName("005005_申請案件完成")]
        public ActionResult Save(Apply_005005FormModel Form)
        {
            try
            {
                ApplyDAO dao = new ApplyDAO();
                ShareDAO shareDao = new ShareDAO();
                SessionModel sm = SessionModel.Get();
                Form.APP_ID = dao.AppendApply005005(Form);

                string MailBody = "<table align=\"left\" style=\"width:90%;\">";
                MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
                MailBody += " <tr><td>貴公司申請中藥GMP廠證明文件(英文)(案號：" + Form.APP_ID + ")一案，「人民申請案線上申辦服務系統」已收件，尚待貴公司繳納規費，請提供申請者聯絡資訊、申請案類別、案件編號及規費金額等資料，連同欲繳納之匯票或現金，寄到「115204臺北市南港區忠孝東路六段488號 衛生福利部中醫藥司收」，敬請配合。</td></tr>";
                MailBody += " <tr><td><br /></td></tr>";
                MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
                MailBody += "</table>";

                dao.SendMail_New(Form.CNT_NAME, Form.EMAIL, Form.APP_ID, "中藥GMP廠證明文件(英文)", "005005", MailBody);

                sm.LastResultMessage = "送出成功";
                return View("Save", Form);
            }
            catch (Exception ex)
            {
                logger.Error("005005_申請案件失敗，原因：" + ex.Message, ex);
                throw new Exception(string.Format("005005_申請案件失敗，原因：{0}", ex.Message));
            }

        }

        [HttpPost]
        [DisplayName("005005_申請單套表")]
        public void PreviewApplyForm(Apply_005005FormModel Form)
        {
            string path = Server.MapPath("~/Sample/apply005005.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //申請日期
                    //doc.ReplaceText("$APP_TIME_TW", "中華民國" + Form.APP_TIME_TW.Split('/')[0] + "年" + Form.APP_TIME_TW.Split('/')[1] + "月" + Form.APP_TIME_TW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);

                    //外銷國家
                    doc.ReplaceText("$IMP_COUNTRY", Form.IMP_COUNTRY.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠名稱
                    doc.ReplaceText("$MF_CNT_NAME", Form.MF_CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠地址
                    doc.ReplaceText("$MF_ADDR", Form.MF_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠許可編號
                    doc.ReplaceText("$LIC_NUM", Form.LIC_NUM.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //黑色底線
                    Xceed.Document.NET.Formatting formatting = new Xceed.Document.NET.Formatting();
                    formatting.UnderlineColor = System.Drawing.Color.Black;
                    //白色底線
                    Xceed.Document.NET.Formatting formatting1 = new Xceed.Document.NET.Formatting();
                    formatting1.UnderlineColor = System.Drawing.Color.White;

                    //製造廠名稱
                    doc.ReplaceText("$ISSUE_DATE", Convert.ToDateTime(Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")), false, System.Text.RegularExpressions.RegexOptions.None, formatting);

                    var yearStr = ", " + Convert.ToDateTime(Form.ISSUE_DATE).Year;
                    doc.ReplaceText(yearStr, yearStr, false, System.Text.RegularExpressions.RegexOptions.None, formatting1);

                    //製造廠名稱
                    doc.ReplaceText("$EXPIR_DATE", Convert.ToDateTime(Form.EXPIR_DATE).ToString("MMM. dd, yyyy.", CultureInfo.CreateSpecificCulture("en-US")), false, System.Text.RegularExpressions.RegexOptions.None, formatting);

                    yearStr = ", " + Convert.ToDateTime(Form.EXPIR_DATE).Year;
                    doc.ReplaceText(yearStr, yearStr, false, System.Text.RegularExpressions.RegexOptions.None, formatting1);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=GMP英文證明書.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        [DisplayName("005005_補件進入")]
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005005ViewModel model = new Apply_005005ViewModel();

            model.FormBack = dao.QueryApply_005005(APP_ID);
            model.Detail = dao.GetApplyNotice_005005(APP_ID);

            if (model.FormBack.CODE_CD == "2" || model.FormBack.CODE_CD == "4")
            {
                if (shareDao.CalculationDocDate("005005", APP_ID))
                {
                    //已過補件期限
                    return View("Detail005005", model);
                }
                else
                {
                    return View("AppDoc", model);
                }
            }
            else
            {
                return View("Detail005005", model);
            }
        }

        [HttpPost]
        [DisplayName("005005_補件驗證欄位")]
        public ActionResult DocSave(Apply_005005ViewModel model)
        {
            var result = new AjaxResultStruct();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\'\.\-\,\s\(\)]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            System.Text.RegularExpressions.Regex reg4 = new System.Text.RegularExpressions.Regex(@"^[\u4e00-\u9fa5]+$");
            System.Text.RegularExpressions.Regex reg5 = new System.Text.RegularExpressions.Regex(@"^[0-9\-\#]+$");

            string ErrorMsg = "";
            if (string.IsNullOrEmpty(model.FormBack.IMP_COUNTRY))
            {
                ErrorMsg += "外銷國家 欄位是必要項。";
            }
            else
            {
                if (model.FormBack.IMP_COUNTRY.Trim().Substring(model.FormBack.IMP_COUNTRY.Trim().Length - 1, 1) != ".")
                {
                    ErrorMsg += "外銷國家最後請以「.」結尾。\r\n";
                }

                if (!reg.IsMatch(model.FormBack.IMP_COUNTRY))
                {
                    ErrorMsg += "外銷國家請以英文填寫。\r\n";
                }
            }

            if (string.IsNullOrEmpty(model.FormBack.LIC_NUM))
            {
                ErrorMsg += "製造廠許可編號 欄位是必要項。";
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(model.FormBack.LIC_NUM.ToUpper(), @"^\(C\)[0-9]{7}$"))
                {
                    ErrorMsg += "製造廠許可編號輸入錯誤。\r\n";
                }
            }

            if (string.IsNullOrEmpty(model.FormBack.MF_CNT_NAME))
            {
                ErrorMsg += "製造廠名稱 欄位是必要項。";
            }
            else
            {
                if (!reg.IsMatch(model.FormBack.MF_CNT_NAME))
                {
                    ErrorMsg += "製造廠名稱請以英文或數字填寫。\r\n";
                }
            }

            if (string.IsNullOrEmpty(model.FormBack.MF_ADDR))
            {
                ErrorMsg += "製造廠地址 欄位是必要項。";
            }
            else
            {
                if (!reg.IsMatch(model.FormBack.MF_ADDR))
                {
                    ErrorMsg += "製造廠地址請以英文或數字填寫。\r\n";
                }
            }

            if (string.IsNullOrEmpty(model.FormBack.ISSUE_DATE))
            {
                ErrorMsg += "最近一次GMP查廠日期 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.EXPIR_DATE))
            {
                ErrorMsg += "GMP有效日期 欄位是必要項。";
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
        [DisplayName("005005_補件存檔")]
        public ActionResult DocFinish(Apply_005005ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = "";

            ErrorMsg = dao.AppendApplyDoc005005(model);
            Apply_005005FormModel Form = new Apply_005005FormModel();
            Form.DOCYN = "Y";
            Form.PAYCOUNT = dao.GetNoticeCount(model.FormBack.APP_ID).ToString();

            if (ErrorMsg == "" && model.FormBack.CODE_CD == "4")
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, "中藥GMP廠證明文件(英文)", "005005", Form.PAYCOUNT, ISBACK: true);
            }
            else if (ErrorMsg == "")
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, "中藥GMP廠證明文件(英文)", "005005", Form.PAYCOUNT);
            }

            return View("Save", Form);
        }

        [HttpPost]
        public ActionResult getGMPData(Apply_005004FormModel Form)
        {
            var Data = new GMPAPI();
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();

            if (string.IsNullOrEmpty(Form.LIC_NUM))
            {
                result.status = false;
                result.message = "請輸入製造廠許可編號";
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(Form.LIC_NUM.ToUpper(), @"^\(C\)[0-9]{7}$"))
                {
                    result.status = false;
                    result.message = "製造廠許可編號格式輸入錯誤\r\n";
                }
                else
                {
                    //WebTestEnvir 測試環境 Y 啟用
                    if (ConfigModel.WebTestEnvir == "Y")
                    {
                        //介接
                        var temp = "{" +
                                   "\"GMPResp\": {" +
                                   "\"名稱\": \"順天堂藥廠股份有限公司\"," +
                                   "\"醫事機構代碼\": \"623105B224\"," +
                                   "\"郵遞區號\": \"40252\"," +
                                   "\"營業地址\": \"新北市新店區北新路3段207號3樓\"," +
                                   "\"負責人\": \"盧道隆\"," +
                                   "\"查廠日期\": \"2005/06/27\"," +
                                   "\"有效期限\": \"2020/12/25\"" +
                                   "},\"STATUS\": \"成功\"}";
                        Data = JsonConvert.DeserializeObject<GMPAPI>(temp);
                    }
                    else
                    {
                        APIDAO api = new APIDAO();
                        Data = api.GetGMPLicense(Form.LIC_NUM);
                    }

                    if (Data.STATUS == "成功")
                    {
                        //資料處理
                        Data.GMPResp = dao.SortAddr(Data.GMPResp);

                        result.status = true;
                        result.message = "成功";
                        result.data = Data.GMPResp;
                    }
                    else if (Data.STATUS == "失敗")
                    {
                        result.status = false;
                        result.message = "取得資料失敗";
                    }
                    else if (Data.STATUS == "查無資料")
                    {
                        result.status = false;
                        result.message = "查無資料";
                    }
                }
            }

            return Content(result.Serialize(), "application/json");
        }
    }
}
