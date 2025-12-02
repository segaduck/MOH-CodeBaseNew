using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class Apply_005004Controller : BaseController
    {
        public static string s_SRV_ID = "005004";
        public static string s_SRV_NAME = "中藥GMP廠證明文件(中文)";

        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt005004");
        }

        [DisplayName("005004_申請案件")]
        public ActionResult Apply(string agree)
        {
            ////agree: 1:同意新增 /other:請先閱讀規章
            //if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            //if (agree != null && !agree.Equals("1")) { return Prompt(); }

            Apply_005004FormModel Form = new Apply_005004FormModel();
            //Form.ISSUE_DATE = DateTime.Now.ToString("yyyy/MM/dd");
            Form.APP_TIME_TW = HelperUtil.DateTimeToTwString(DateTime.Now);
            Form.APP_ID = "";
            Form.MOHW_CASE_NO = "";
            Form.COUNTRY = "中華民國";

            return View("Index", Form);
        }

        [HttpPost]
        [DisplayName("005004_申請案件送出")]
        public ActionResult Apply(Apply_005004FormModel form)
        {
            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            string ErrorMsg = "";
            
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");

            if (ModelState.IsValid)
            {
                //共用欄位驗證
                ErrorMsg = dao.checkApply(form);
                //個別欄位驗證
                if (!string.IsNullOrEmpty(form.LIC_NUM))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(form.LIC_NUM.ToUpper(), @"^\(C\)[0-9]{7}$"))
                    {
                        ErrorMsg += "製造廠許可編號輸入錯誤\r\n";
                    }
                }

                if (string.IsNullOrEmpty(form.PL_CD))
                {
                    ErrorMsg += "請輸入藥商許可執照字號-字\r\n";
                }

                for (int i = 0; i < form.PL_Num.Length; i++)
                {
                    if (!Char.IsNumber(form.PL_Num, i))
                    {
                        ErrorMsg += "藥商許可執照字號請輸入數字!\r\n";
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(form.EMAIL))
                {
                    if (!reg3.IsMatch(form.EMAIL))
                    {
                        ErrorMsg += "請填入正確的Email格式 ! \r\n";
                    }
                }
                if (!string.IsNullOrEmpty(form.Name_File_1))
                {
                    logger.Debug("Apply_005004.FileName:" + form.Name_File_1);
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
                    logger.Debug("Apply_005004.FileName:" + form.Name_File_2);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = form.Name_File_2.ToSplit(".").LastOrDefault().ToLower();
                    if (!validExts.Contains(ext))
                    {
                        ErrorMsg += "不支援的檔案格式";
                    }
                }
                if (!string.IsNullOrEmpty(form.Name_File_3))
                {
                    logger.Debug("Apply_005004.FileName:" + form.Name_File_3);
                    // 允許的附檔名（全小寫）
                    var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                    // 取得副檔名並轉小寫
                    var ext = form.Name_File_3.ToSplit(".").LastOrDefault().ToLower();
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
        [DisplayName("005004_申請案件預覽送出")]
        public ActionResult PreView(Apply_005004FormModel Form)
        {
            if (!string.IsNullOrEmpty(Form.CON_ITEM))
            {
                Form.CON_ITEM = Form.CON_ITEM.Substring(0, Form.CON_ITEM.Length - 1);
                Form.CON_ITEM_CD = Form.CON_ITEM_CD.Substring(0, Form.CON_ITEM_CD.Length - 1);
            }

            if (!string.IsNullOrEmpty(Form.TRA_ITEM))
            {
                Form.TRA_ITEM = Form.TRA_ITEM.Substring(0, Form.TRA_ITEM.Length - 1);
                Form.TRA_ITEM_CD = Form.TRA_ITEM_CD.Substring(0, Form.TRA_ITEM_CD.Length - 1);
            }

            return View("PreView005004", Form);
        }

        [HttpPost]
        [DisplayName("005004_申請案件完成")]
        public ActionResult Save(Apply_005004FormModel Form)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            Form.APP_ID = dao.AppendApply005004(Form);
            var email = Form.EMAIL;

            string MailBody = "<table align=\"left\" style=\"width:90%;\">";
            MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
            MailBody += " <tr><td>貴公司申請中藥GMP廠證明文件(中文)(案號：" + Form.APP_ID + ")一案，「人民申請案線上申辦服務系統」已收件，尚待貴公司繳納規費，請提供申請者聯絡資訊、申請案類別、案件編號及規費金額等資料，連同欲繳納之匯票或現金，寄到「115204臺北市南港區忠孝東路六段488號 衛生福利部中醫藥司收」，敬請配合。</td></tr>";
            MailBody += " <tr><td><br /></td></tr>";
            MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
            MailBody += "</table>";

            dao.SendMail_New(Form.CNT_NAME, email, Form.APP_ID, "中藥GMP廠證明文件(中文)", "005004", MailBody);

            sm.LastResultMessage = "送出成功";
            return View("Save", Form);
        }

        [HttpPost]
        [DisplayName("005004_申請單套表")]
        public void PreviewApplyForm(Apply_005004FormModel Form)
        {
            string path = Server.MapPath("~/Sample/apply005004.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    //文號
                    doc.ReplaceText("$MOHW_CASE_NO_SELF", Form.MOHW_CASE_NO_SELF.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //申請日期
                    doc.ReplaceText("$APP_TIME_TW", "中華民國" + Form.APP_TIME_TW.Split('/')[0] + "年" + Form.APP_TIME_TW.Split('/')[1] + "月" + Form.APP_TIME_TW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);

                    //申請類別
                    if (Form.APPLY_TYPE == "新申請")
                    {
                        doc.ReplaceText("$APPLY_TYPE", "■新申請       □遺失補發      □汙損換發", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (Form.APPLY_TYPE == "遺失補發")
                    {
                        doc.ReplaceText("$APPLY_TYPE", "□新申請       ■遺失補發      □汙損換發", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (Form.APPLY_TYPE == "汙損換發")
                    {
                        doc.ReplaceText("$APPLY_TYPE", "□新申請       □遺失補發      ■汙損換發", false, System.Text.RegularExpressions.RegexOptions.None);
                    }

                    //製造廠名稱/藥廠名稱
                    doc.ReplaceText("$MF_CNT_NAME", Form.MF_CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //藥商許可執照字號
                    doc.ReplaceText("$PL_CD_TEXT", Form.PL_CD.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("$PL_Num", Form.PL_Num.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //製造廠名稱/藥廠地址
                    doc.ReplaceText("$TAX_ORG_CITY_CODE", Form.TAX_ORG_CITY_CODE.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("$TAX_ORG_CITY_TEXT", Form.TAX_ORG_CITY_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("$TAX_ORG_CITY_DETAIL", Form.TAX_ORG_CITY_DETAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //聯絡人
                    doc.ReplaceText("$CNT_NAME", Form.CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //var telall = Form.TEL_BEFORE + "-" + Form.TEL_AFTER;
                    //if (!string.IsNullOrEmpty(Form.TEL_Extension))
                    //{
                    //    telall += "#" + Form.TEL_Extension;
                    //}

                    //電話
                    doc.ReplaceText("$TELALL", Form.TEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //var faxall = "";
                    //if (!string.IsNullOrEmpty(Form.FAX_BEFORE))
                    //{
                    //    faxall = Form.FAX_BEFORE + "-" + Form.FAX_AFTER;
                    //    if (!string.IsNullOrEmpty(Form.FAX_Extension))
                    //    {
                    //        faxall += "#" + Form.FAX_Extension;
                    //    }
                    //}

                    //傳真
                    doc.ReplaceText("$FAXALL", Form.FAX.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    var emailall = "";
                    if (Form.EMAIL_ADDR == "0")
                    {
                        emailall = Form.EMAIL_BEFORE + "@" + Form.EMAIL_CUSTOM.TONotNullString().Replace("@", "");
                    }
                    else
                    {
                        emailall = Form.EMAIL_BEFORE + "@" + Form.EMAIL_ADDR_TEXT;
                    }

                    //EMAIL
                    doc.ReplaceText("$EMALALL", Form.EMAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //負責人身分證統一編號
                    doc.ReplaceText("$IDN", Form.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //負責人姓名
                    doc.ReplaceText("$CHR_NAME", Form.CHR_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //■製劑    □原料藥$PROCESS_TYPE
                    if (Form.CON_CHECK == "Y" && Form.TRA_CHECK == "Y")
                    {
                        doc.ReplaceText("$PROCESS_TYPE", "■製劑    ■原料藥", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (Form.CON_CHECK == "Y")
                    {
                        doc.ReplaceText("$PROCESS_TYPE", "■製劑    □原料藥", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else if (Form.TRA_CHECK == "Y")
                    {
                        doc.ReplaceText("$PROCESS_TYPE", "□製劑    ■原料藥", false, System.Text.RegularExpressions.RegexOptions.None);
                    }

                    //監製藥師
                    doc.ReplaceText("$PP_NAME", Form.PP_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //工廠登記證字號
                    doc.ReplaceText("$FRC_Num", Form.FRC_Num.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    //最近一次GMP查廠日期
                    doc.ReplaceText("$GMP_TIME_TW", "中華民國 " + Form.ISSUE_DATE_TW.Split('/')[0] + " 年 " + Form.ISSUE_DATE_TW.Split('/')[1] + " 月 " + Form.ISSUE_DATE_TW.Split('/')[2] + " 日 ", false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=中藥GMP廠證明文件申請單.doc");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        [DisplayName("005004_補件進入")]
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005004ViewModel model = new Apply_005004ViewModel();

            model.FormBack = dao.QueryApply_005004(APP_ID);
            model.Detail = dao.GetApplyNotice_005004(APP_ID);

            if (model.FormBack.CODE_CD == "2" || model.FormBack.CODE_CD == "4")
            {
                if (shareDao.CalculationDocDate("005004", APP_ID))
                {
                    //已過補件期限
                    return View("Detail005004", model);
                }
                else
                {
                    return View("AppDoc", model);
                }
            }
            else
            {
                return View("Detail005004", model);
            }
        }

        [HttpPost]
        [DisplayName("005004_補件驗證欄位")]
        public ActionResult DocSave(Apply_005004ViewModel model)
        {
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            if (string.IsNullOrEmpty(model.FormBack.LIC_NUM))
            {
                ErrorMsg += "製造廠許可編號 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.NAME))
            {
                ErrorMsg += "製造廠名稱 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.PL_CD))
            {
                ErrorMsg += "藥商許可執照字號 選擇錯誤。";
            }
            if (string.IsNullOrEmpty(model.FormBack.PL_Num))
            {
                ErrorMsg += "藥商許可執照字號 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_CODE))
            {
                ErrorMsg += "製造廠地址區碼 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_TEXT))
            {
                ErrorMsg += "製造廠地址區域 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_DETAIL))
            {
                ErrorMsg += "製造廠地址 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.CHR_NAME))
            {
                ErrorMsg += "負責人姓名 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.IDN))
            {
                ErrorMsg += "負責人身分證字號 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.PP_NAME))
            {
                ErrorMsg += "測試監製藥師 欄位是必要項。";
            }
            if (string.IsNullOrEmpty(model.FormBack.FRC_NUM))
            {
                ErrorMsg += "工廠登記證字號 欄位是必要項。";
            }
            if (model.FormBack.CON_CHECK == "N" && model.FormBack.TRA_CHECK == "N")
            {
                ErrorMsg += "請至少勾選一項產品類別。";
            }
            if (string.IsNullOrEmpty(model.FormBack.ISSUE_DATE))
            {
                ErrorMsg += "最近一次GMP查廠日期 欄位是必要項。";
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
        [DisplayName("005004_補件存檔")]
        public ActionResult DocFinish(Apply_005004ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = "";

            ErrorMsg = dao.AppendApplyDoc005004(model);
            Apply_005004FormModel Form = new Apply_005004FormModel();
            Form.DOCYN = "Y";
            Form.DOCCOUNT = dao.GetNoticeCount(model.FormBack.APP_ID).ToString();

            if (ErrorMsg == "" && model.FormBack.CODE_CD == "4")
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, "中藥GMP廠證明文件(中文)", "005004", Form.DOCCOUNT, ISBACK: true);
            }
            else if (ErrorMsg == "")
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, "中藥GMP廠證明文件(中文)", "005004", Form.DOCCOUNT);
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

        [HttpPost]
        public ActionResult getMemberData(Apply_005004FormModel Form)
        {
            SessionModel sm = SessionModel.Get();
            var result = new AjaxResultStruct();

            result.status = true;
            result.message = "成功";
            result.data = JsonConvert.SerializeObject(sm);

            return Content(result.Serialize(), "application/json");
        }
    }
}
