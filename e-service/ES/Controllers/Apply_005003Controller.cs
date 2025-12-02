using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.ChineseMedicineAPI;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace ES.Controllers
{
    public class Apply_005003Controller : BaseController
    {
        public static string s_SRV_ID = "005003";
        public static string s_SRV_NAME = "WHO格式之產銷證明書(英文)";
        //public static string s_SRV_NAME2 = "產銷證明書";

        //[HttpGet]
        //public ActionResult Prompt()
        //{
        //    SessionModel sm = SessionModel.Get();
        //    string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後再，進入申辦頁面 !";
        //    sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
        //    return View("Prompt1");
        //}

        [DisplayName("005003_申請案件")]
        public ActionResult Apply(string agree)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            Apply_005003FormModel Form = new Apply_005003FormModel();

            ActionResult rtn = View("Index", Form);

            //agree: 1:同意新增 /other:請先閱讀規章
            //if (string.IsNullOrEmpty(agree)) { agree = "0"; }
            //if (agree != null && !agree.Equals("1")) { return Prompt(); }

            Form.APP_TIME_TW = HelperUtil.DateTimeToTwString(DateTime.Now);
            Form.PAYAMOUNT = 1500;
            Form.PAYCOUNT = "1";
            Form.COPIES = Convert.ToInt32(Form.PAYCOUNT);
            Form.IS_PREVIEW = false;

            Form.IDN = mem.IDN;
            Form.NAME = mem.NAME;
            Form.CNT_NAME = mem.CNT_NAME;
            //Form.BIRTHDAY = mem.BIRTHDAY;
            //Form.SEX_CD = mem.SEX_CD;
            // 電話
            if (mem.TEL != null)
            {
                Form.TEL = mem.TEL;
                Form.TEL_0 = mem.TEL.ToSplit('-').FirstOrDefault();
                Form.TEL_1 = mem.TEL.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                Form.TEL_2 = mem.TEL.ToSplit('-').LastOrDefault().ToSplit('#').LastOrDefault();
                if (Form.TEL_0 != null && Form.TEL_0.Equals(Form.TEL_1)) { Form.TEL_0 = ""; }
                if (Form.TEL_2 != null && Form.TEL_2.Equals(Form.TEL_1)) { Form.TEL_2 = ""; }
            }
            if (mem.FAX != null)
            {
                Form.FAX = mem.FAX;
                Form.FAX_0 = mem.FAX.ToSplit('-').FirstOrDefault();
                Form.FAX_1 = mem.FAX.ToSplit('-').LastOrDefault().ToSplit('#').FirstOrDefault();
                Form.FAX_2 = mem.FAX.ToSplit('-').LastOrDefault().ToSplit('#').LastOrDefault();
                if (Form.FAX_0 != null && Form.FAX_0.Equals(Form.TEL_1)) { Form.FAX_0 = ""; }
                if (Form.FAX_2 != null && Form.FAX_2.Equals(Form.TEL_1)) { Form.FAX_2 = ""; }
            }
            if (mem.MAIL != null)
            {
                Form.EMAIL = mem.MAIL;
                Form.EMAIL_0 = mem.MAIL.ToSplit('@').FirstOrDefault();
                Form.EMAIL_1 = "0";//0:自訂-GetMailDomainList 
                Form.EMAIL_2 = mem.MAIL.ToSplit('@').LastOrDefault();
            }
            return View("Index", Form);
        }

        [HttpPost]
        [DisplayName("005003_申請案件送出")]
        public ActionResult Apply(Apply_005003FormModel form)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\'\:]+$");//(英文)請以英文填寫
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");//請填寫數字
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");//請以數字填寫

            //string ErrorMsg = "";
            string s_COLNAME = "";
            //共用欄位驗證
            //ErrorMsg = dao.checkApply(form);
            //個別欄位驗證

            //申辦份數
            s_COLNAME = "申辦份數";
            if (!form.COPIES.HasValue) { ModelState.AddModelError("COPIES", string.Format("{0} 為必選欄位", s_COLNAME)); }
            //* 公司名稱
            if (string.IsNullOrEmpty(form.NAME)) ModelState.AddModelError("NAME", "公司名稱 為必填欄位");
            //* 聯絡人
            //if (string.IsNullOrEmpty(form.CNT_NAME)) ModelState.AddModelError("CNT_NAME", "聯絡人 為必填欄位");
            //* 電話
            s_COLNAME = "電話";
            if (string.IsNullOrEmpty(form.TEL_1)) ModelState.AddModelError("TEL_1", string.Format("{0} 為必填欄位", s_COLNAME));
            //00-0000#00

            //*E-MAIL
            s_COLNAME = "E-MAIL";
            bool flag_EMAILNG = false;
            if (string.IsNullOrWhiteSpace(form.EMAIL_0)) { flag_EMAILNG = true; }
            if (string.IsNullOrWhiteSpace(form.EMAIL_1) && string.IsNullOrWhiteSpace(form.EMAIL_2)) { flag_EMAILNG = true; }
            if (form.EMAIL_1 != null && form.EMAIL_1.Equals("0") && string.IsNullOrWhiteSpace(form.EMAIL_2)) { flag_EMAILNG = true; }
            if (flag_EMAILNG) { ModelState.AddModelError("EMAIL", "E-MAIL 為必填欄位"); }
            //* 外銷國家
            s_COLNAME = "外銷國家";
            if (string.IsNullOrEmpty(form.MF_ADDR)) ModelState.AddModelError("MF_ADDR", string.Format("{0} 為必填欄位", s_COLNAME));
            else if (form.MF_ADDR.IndexOf(".") < 0) { ModelState.AddModelError("MF_ADDR", string.Format("{0} 最後請以「.」結尾。", s_COLNAME)); }

            //* 2A.1藥品許可證字號
            s_COLNAME = "2A.1藥品許可證字號-字";
            if (string.IsNullOrEmpty(form.F_2A_1_WORD)) ModelState.AddModelError("F_2A_1_WORD", string.Format("{0} 為必選欄位", s_COLNAME));
            s_COLNAME = "2A.1藥品許可證字號-號";
            if (string.IsNullOrEmpty(form.F_2A_1_NUM)) ModelState.AddModelError("F_2A_1_NUM", string.Format("{0} 為必填欄位", s_COLNAME));

            //*劑型
            s_COLNAME = "劑型";
            if (string.IsNullOrEmpty(form.F_1_DF)) ModelState.AddModelError("F_1_DF", string.Format("{0} 為必填欄位", s_COLNAME));
            //* 1.1處方說明
            s_COLNAME = "1.1處方說明";
            if (!string.IsNullOrEmpty(form.F_1_1))
            {
                if (form.F_1_1.Trim().Substring(form.F_1_1.Trim().Length - 1, 1) != ":")
                {
                    ModelState.AddModelError("F_1_1", "1.1處方說明最後請以「:」結尾。");
                }
            }
            else ModelState.AddModelError("F_1_1", string.Format("{0} 為必填欄位", s_COLNAME));

            //* 1.3本產品是否有在國內販售？
            s_COLNAME = "1.3本產品是否有在國內販售？";
            if (string.IsNullOrEmpty(form.F_1_3)) ModelState.AddModelError("F_1_3", string.Format("{0} 為必選欄位", s_COLNAME));
            //*2A.1核准日期 
            s_COLNAME = "2A.1核准日期 ";
            if (!form.F_2A_1_DATE.HasValue) ModelState.AddModelError("F_2A_1_DATE", string.Format("{0} 為必填欄位", s_COLNAME));

            //* 2A.2/2A.3.1/2B.2.1製造廠地址
            s_COLNAME = "2A.2/2A.3.1/2B.2.1製造廠名稱";
            if (string.IsNullOrEmpty(form.F_2A_3_1_NAME)) ModelState.AddModelError("F_2A_3_1_NAME", string.Format("{0} 為必填欄位", s_COLNAME));

            //* 2A.2/2A.3.1/2B.2.1製造廠地址
            s_COLNAME = "2A.2/2A.3.1/2B.2.1製造廠地址";
            if (string.IsNullOrEmpty(form.F_2A_3_2_ADDR)) ModelState.AddModelError("F_2A_3_2_ADDR", string.Format("{0} 為必填欄位", s_COLNAME));


            // 1.2本產品是否獲准在國內販售？
            if (form.F_1_2.TONotNullString() == "Y")
            {
                //* 是否為委託製造？
                if (form.F_2A_2_COMM.TONotNullString() == "Y")
                {
                    if (form.F_2A_2.TONotNullString() == "")
                    {
                        ModelState.AddModelError("F_2A_2", "請填寫2A.2藥商名稱 。");
                    }
                    if (form.F_2A_2_ADDR.TONotNullString() == "")
                    {
                        ModelState.AddModelError("F_2A_2_ADDR", "請填寫2A.2藥商地址 。");
                    }
                }
                if (form.F_2A_3.TONotNullString() == "")
                {
                    ModelState.AddModelError("F_2A_3", "請勾選2A.3藥品許可證持有者之類別 。");
                }

                form.F_2B_2 = null;
                form.F_2B_3 = null;
                form.F_2B_3_REMARKS = null;

            }
            else
            {
                if (form.F_2B_2.TONotNullString() == "")
                {
                    ModelState.AddModelError("F_2B_2", "請勾選2B.2申請者之類別 。");
                }
                if (form.F_2B_3.TONotNullString() == "")
                {
                    ModelState.AddModelError("F_2B_3", "請選擇2B.3僅供外銷專用之原因 。");
                }

                // 選否不需要有值
                form.F_2A_2_COMM = null;
                form.F_2A_2 = null;
                form.F_2A_2_ADDR = null;

                form.F_2A_3 = null;
                form.F_2A_4 = null;
                form.F_2A_5 = null;
                form.F_2A_6 = null;
            }

            //* 3.製造廠是否定期接受本部之GMP查核？
            s_COLNAME = "3.製造廠是否定期接受本部之GMP查核？";
            if (string.IsNullOrEmpty(form.F_3_0)) ModelState.AddModelError("F_3_0", string.Format("{0} 為必選欄位", s_COLNAME));
            //*3.1接受定期查核之週期為何？
            s_COLNAME = "3.1接受定期查核之週期為何？";
            if (string.IsNullOrEmpty(form.F_3_1)) ModelState.AddModelError("F_3_1", string.Format("{0} 為必選欄位", s_COLNAME));
            //*3.2申請案藥品許可證之劑型，是否經過本部查核？
            s_COLNAME = "3.2申請案藥品許可證之劑型，是否經過本部查核？";
            if (string.IsNullOrEmpty(form.F_3_2)) ModelState.AddModelError("F_3_2", string.Format("{0} 為必選欄位", s_COLNAME));
            //*3.3申請案藥品許可證之製造設備及製程，是否符合WHO建議之GMP規範？
            s_COLNAME = "3.3申請案藥品許可證之製造設備及製程，是否符合WHO建議之GMP規範？";
            if (string.IsNullOrEmpty(form.F_3_3)) ModelState.AddModelError("F_3_3", string.Format("{0} 為必選欄位", s_COLNAME));
            //*4.申請者所提供之資訊，是否符合外銷對象對產品製造所有方面的標準？
            s_COLNAME = "4.申請者所提供之資訊，是否符合外銷對象對產品製造所有方面的標準？";
            if (string.IsNullOrEmpty(form.F_4)) ModelState.AddModelError("F_4", string.Format("{0} 為必選欄位", s_COLNAME));
            var i = 1;
            foreach (var item in form.F11.GoodsList)
            {
                if (string.IsNullOrEmpty(item.F11_SCI_NM))
                {
                    ModelState.AddModelError("F11_SCI_NM"+i.TONotNullString(), "成分內容_生藥名(" + i.TONotNullString() + ")為必選欄位");
                }
                i++;
            }
            // 確認上傳檔案
            if (form.MERGEYN.TONotNullString().Equals("Y"))
            {
                bool flag_Empty = false;
                if (string.IsNullOrEmpty(form.FILE_LICF_TEXT)
                        && string.IsNullOrEmpty(form.FILE_LICB_TEXT)
                        && string.IsNullOrEmpty(form.FILE_CHART_TEXT)) { flag_Empty = true; }
                if (flag_Empty) ModelState.AddModelError("FILE_ALL", "請至少上傳一個檔案 ");
            }
            if (!string.IsNullOrEmpty(form.FILE_LICF_TEXT))
            {
                logger.Debug("Apply_005003.FileName:" + form.FILE_LICF_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                // 取得副檔名並轉小寫
                var ext = form.FILE_LICF_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            if (!string.IsNullOrEmpty(form.FILE_LICB_TEXT))
            {
                logger.Debug("Apply_005003.FileName:" + form.FILE_LICB_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                // 取得副檔名並轉小寫
                var ext = form.FILE_LICB_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            if (!string.IsNullOrEmpty(form.FILE_CHART_TEXT))
            {
                logger.Debug("Apply_005003.FileName:" + form.FILE_CHART_TEXT);
                // 允許的附檔名（全小寫）
                var validExts = new[] { "pdf", "jpg", "jpeg", "bmp", "png", "gif", "tif", "zip" };
                // 取得副檔名並轉小寫
                var ext = form.FILE_CHART_TEXT.ToSplit(".").LastOrDefault().ToLower();
                if (!validExts.Contains(ext))
                {
                    ModelState.AddModelError("FILE_ALL", "不支援的檔案格式");
                }
            }
            //else
            //{
            //    s_COLNAME = "藥品許可證影本(正面)";
            //    if (string.IsNullOrEmpty(form.FILE_LICF_TEXT)) ModelState.AddModelError("FILE_LICF", string.Format("請上傳 {0} ", s_COLNAME));
            //    s_COLNAME = "藥品許可證影本(反面)";
            //    if (string.IsNullOrEmpty(form.FILE_LICB_TEXT)) ModelState.AddModelError("FILE_LICB", string.Format("請上傳 {0} ", s_COLNAME));
            //    s_COLNAME = "處方之中藥材中英文對照表。";
            //    if (string.IsNullOrEmpty(form.FILE_CHART_TEXT)) ModelState.AddModelError("FILE_CHART", string.Format("請上傳 {0} ", s_COLNAME));
            //}

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                result.status = true;
                result.message = "";
            }
            else
            {
                string ErrorMsg = "";
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

        [HttpPost]
        [DisplayName("005003_申請案件預覽送出")]
        public ActionResult PreView(Apply_005003FormModel Form)
        {
            Form.F11.IsReadOnly = true;
            //Form.PC.IsReadOnly = true;
            Form.IS_PREVIEW = true;

            return View("PreView1", Form);
        }

        [HttpPost]
        [DisplayName("005003_申請案件完成")]
        public ActionResult Save(Apply_005003FormModel Form)
        {
            //ShareDAO shareDao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            ApplyDAO dao = new ApplyDAO();
            //'SAVE'
            Form.APP_ID = dao.AppendApply005003(Form);

            string s_Mail_A1 = @"貴公司申請{0}(案號：{1})一案，「人民申請案線上申辦服務系統」已收件，尚待貴公司繳納規費，請提供申請者聯絡資訊、申請案類別、案件編號及規費金額等資料，連同欲繳納之匯票或現金，寄到「115204台北市南港區忠孝東路六段488號衛生福利部中醫藥司收」，敬請配合。";
            string s_Mail_1 = string.Format(s_Mail_A1, s_SRV_NAME, Form.APP_ID);

            string MailBody = "<table align=\"left\" style=\"width:90%;\">";
            MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
            MailBody += " <tr><td>";
            MailBody += s_Mail_1;
            MailBody += " </td></tr>";
            MailBody += " <tr><td><br /></td></tr>";
            MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
            MailBody += "</table>";

            dao.SendMail_New(Form.CNT_NAME, Form.EMAIL, Form.APP_ID, s_SRV_NAME, s_SRV_ID, MailBody);

            sm.LastResultMessage = "送出成功";
            return View("Save", Form);
        }

        [HttpPost]
        [DisplayName("005003_申請單套表")]
        public void PreviewApplyForm(Apply_005003FormModel form)
        {
            ShareCodeListModel sc = new ShareCodeListModel();
            Apply_005003FormModel fm = form;
            string path = Server.MapPath("~/Sample/apply005003.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    string F_1_2_YN = sc.Get_YesNo_TEXT(fm.F_1_2);
                    string F_1_3_YN = sc.Get_YesNo_TEXT(fm.F_1_3);
                    //申請日期
                    //doc.ReplaceText("$APP_TIME_TW", "中華民國" + Form.APP_TIME_TW.Split('/')[0] + "年" + Form.APP_TIME_TW.Split('/')[1] + "月" + Form.APP_TIME_TW.Split('/')[2] + "日", false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IMP_COUNTRY$]", fm.MF_ADDR.Replace(",",", ").TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1$]", fm.F_1.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_DF$]", fm.F_1_DF.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    var str = "";
                    foreach (var item in fm.F11.GoodsList)
                    {
                        if(item.F11_QUANTITY.TONotNullString() == "")
                        {
                            item.F11_QUANTITY = "0";
                        }
                        str += item.F11_SCI_NM.TONotNullString() + " " + item.F11_QUANTITY.TONotNullString() + " " + item.F11_UNIT.TONotNullString() + ", ";
                    }
                    if (str.Length > 0)
                    {
                        str = str.Remove(str.LastIndexOf(","), 2);// 去除最後一個,
                        str += ".";// 英文句點
                    }
                    doc.ReplaceText("[$F11$]", str, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_1$]", fm.F_1_1.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_2_F$]", F_1_2_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_3_F$]", F_1_3_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    string F_2A_1_NUM_WN = string.Format("License number: {0}-{1}", fm.F_2A_1_WORD, fm.F_2A_1_NUM);
                    string F_2A_1_DATE_US = sc.Get_DATE_US_TEXT(fm.F_2A_1_DATE);
                    doc.ReplaceText("[$F_2A_1_NUM$]", F_2A_1_NUM_WN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_2A_1_DATE_F$]",F_2A_1_DATE_US.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    if (fm.F_1_2.TONotNullString().Equals("Y"))
                    {
                        string F_2A_4_YN = sc.Get_YesNo_TEXT(fm.F_2A_4);
                        string F_2A_5_YN = sc.Get_YesNo_TEXT(fm.F_2A_5);
                        string F_2A_6_NAME = fm.F_2A_6_NAME.TONotNullString();
                        string F_2A_6_ADDR = fm.F_2A_6_ADDR.TONotNullString();
                        if (fm.F_2A_6.TONotNullString().Equals("Y"))
                        {
                            F_2A_6_NAME = "The same as the license holder.";
                            F_2A_6_ADDR = "";
                        }

                        string F_2A_2 = fm.F_2A_2_COMM.TONotNullString().Equals("Y") ? fm.F_2A_2.TONotNullString() : fm.F_2A_3_1_NAME.TONotNullString();
                        string F_2A_2_ADDR = fm.F_2A_2_COMM.TONotNullString().Equals("Y") ? fm.F_2A_2_ADDR.TONotNullString() : fm.F_2A_3_2_ADDR.TONotNullString();

                        doc.ReplaceText("[$F_2A_2$]", F_2A_2, false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_2_ADDR$]", F_2A_2_ADDR, false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3$]", fm.F_2A_3.TONotNullString() + ".", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_4_F$]", F_2A_4_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_5_F$]", F_2A_5_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_NAME$]", F_2A_6_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_ADDR$]", F_2A_6_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        var f2b3str = "";

                        doc.ReplaceText("[$F_2B_2$]", fm.F_2B_2.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_3$]", f2b3str.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                        doc.ReplaceText("[$F_2A_3_1_NAME$]", fm.F_2A_3_1_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_2_ADDR$]", fm.F_2A_3_2_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_3_REMARKS$]", fm.F_2B_3_REMARKS.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_6_NAME$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_6_ADDR$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    else
                    {
                        string F_2A_6_NAME = fm.F_2A_6_NAME.TONotNullString();
                        string F_2A_6_ADDR = fm.F_2A_6_ADDR.TONotNullString();
                        string F_2B_6_NAME = F_2A_6_NAME;
                        string F_2B_6_ADDR = F_2A_6_ADDR;
                        if (fm.F_2A_6.TONotNullString().Equals("Y"))
                        {
                            F_2A_6_NAME = "The same as the license holder.";
                            F_2A_6_ADDR = "";
                        }
                        doc.ReplaceText("[$F_2A_2$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_2_ADDR$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_4_F$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_5_F$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_NAME$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_6_ADDR$]", "", false, System.Text.RegularExpressions.RegexOptions.None);
                        fm.F_2B_1_NAME = fm.F_2A_6_NAME.TONotNullString();
                        fm.F_2B_2_ADDR = fm.F_2A_6_ADDR.TONotNullString();
                        string F_2B_3_Eng = sc.Get_PList2B3_ENG(fm.F_2B_3);
                        var f2b3str = "";
                        doc.ReplaceText("[$F_2B_1_NAME$]", fm.F_2B_1_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_2_ADDR$]", fm.F_2B_2_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_2$]", fm.F_2B_2.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                        switch (fm.F_2B_3.TONotNullString())
                        {
                            case "1":
                                f2b3str = "Not required.";
                                break;
                            case "2":
                                f2b3str = "Not requested.";
                                break;
                            case "3":
                                f2b3str = "Under  consideration.";
                                break;
                            case "4":
                                f2b3str = "Refused.";
                                break;
                            default:
                                break;
                        }
                        doc.ReplaceText("[$F_2B_3$]", f2b3str, false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_1_NAME$]", fm.F_2A_3_1_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_2_ADDR$]", fm.F_2A_3_2_ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2A_3_3_REMARKS$]", fm.F_2B_3_REMARKS.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_6_NAME$]", F_2B_6_NAME, false, System.Text.RegularExpressions.RegexOptions.None);
                        doc.ReplaceText("[$F_2B_6_ADDR$]", F_2B_6_ADDR, false, System.Text.RegularExpressions.RegexOptions.None);
                    }

                    string F_3_0_YN = sc.Get_YesNo_TEXT(fm.F_3_0);
                    string F_3_1 = string.Format("{0} years.", fm.F_3_1.TONotNullString());
                    string F_3_2_YN = sc.Get_YesNo_TEXT(fm.F_3_2);
                    string F_3_3_YN = sc.Get_YesNo_TEXT(fm.F_3_3);
                    string F_4_YN = sc.Get_YesNo_TEXT(fm.F_4);
                    doc.ReplaceText("[$F_3_F$]", F_3_0_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_3_1$]", F_3_1.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_3_2_F$]", F_3_2_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_3_3_F$]", F_3_3_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_4_F$]", F_4_YN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            string attach_1 = string.Format(@"attachment;  filename={0}.doc", s_SRV_NAME);
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", attach_1);
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        [DisplayName("005003_補件進入")]
        public ActionResult AppDoc(string APP_ID)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            ApplyDAO dao = new ApplyDAO();
            //Apply_011007DetailModel detail = new Apply_011007DetailModel();

            // 案件基本資訊
            ApplyModel app_Where = new ApplyModel();
            app_Where.APP_ID = APP_ID;
            app_Where.SRV_ID = s_SRV_ID;
            ApplyModel app_data = dao.GetRow(app_Where);

            // 案件基本資訊-2
            Apply_005003Model app_SN_Where = new Apply_005003Model();
            app_SN_Where.APP_ID = APP_ID;
            Apply_005003Model app_SN_data = dao.GetRow(app_SN_Where);

            // 判斷是否為該案件申請人
            if (!mem.ACC_NO.Equals(app_data.ACC_NO))
            {
                sm.LastErrorMessage = "非案件申請人無法瀏覽次案件 !";// ex.Message;
                return RedirectToAction("Index", "History");
            }

            ModelState.Clear();

            ShareDAO shareDao = new ShareDAO();
            Apply_005003ViewModel model = new Apply_005003ViewModel(APP_ID);

            // 帶入基本資料
            model.FormBack = dao.QueryApply_005003(APP_ID);

            // 附件檔 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
            Apply_005003AppDocModel file = new Apply_005003AppDocModel();
            file = dao.GetFile_005003(APP_ID);
            model.FormBack.FILE_LICF_TEXT = file.FILE_LICF_TEXT;
            model.FormBack.FILE_LICB_TEXT = file.FILE_LICB_TEXT;
            model.FormBack.FILE_CHART_TEXT = file.FILE_CHART_TEXT;

            // 補件狀態-案件狀態 1 補件 0 非補件
            // model.FormBack.APPSTATUS = app_data.FLOW_CD.TONotNullString() == "2" ? "1" : "0";
            //FLOW_CD  0:結案(回函核准) 1:新收案件 2:申請資料待確認 3:審查中 4:申請案補件中 10:已收案，處理中 20:結案(歉難同意) 
            model.FormBack.FLOW_CD = model.FormBack.CODE_CD;
            model.Detail = dao.GetApplyNotice_005003(APP_ID);

            if (model.FormBack.CODE_CD == "2" || model.FormBack.CODE_CD == "4")
            {
                //補件案件是否鎖定(true=>鎖定，false=>不鎖定)
                bool ret = shareDao.CalculationDocDate(s_SRV_ID, APP_ID);
                if (ret)
                {
                    //已過補件期限
                    model.F11.IsReadOnly = true;
                    model.FormBack.F11.IsReadOnly = true;
                    //model.PC.IsReadOnly = true;
                    return View("Detail1", model);
                }
                else
                {
                    model.F11.IsReadOnly = false;
                    model.FormBack.F11.IsReadOnly = false;
                    return View("AppDoc", model);
                }
            }
            else
            {
                model.F11.IsReadOnly = true;
                model.FormBack.F11.IsReadOnly = true;
                //model.PC.IsReadOnly = true;
                return View("Detail1", model);
            }
        }

        [HttpPost]
        [DisplayName("005003_補件驗證欄位")]
        public ActionResult DocSave(Apply_005003ViewModel model)
        {
            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\'\:_]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");

            ModelState.Clear();

            string ErrorMsg = "";

            #region 檢核
            if (model.FormBack.F_1_1.Trim().Substring(model.FormBack.F_1_1.Trim().Length - 1, 1) != ":" && model.FormBack.F_1_1.Trim().Substring(model.FormBack.F_1_1.Trim().Length - 1, 1) != "：")
            {
                ErrorMsg += "1.1處方說明最後請以「:」結尾。\r\n";
            }

            // 1.2本產品是否獲准在國內販售？
            if (model.FormBack.F_1_2.TONotNullString() == "Y")
            {
                //* 是否為委託製造？
                if (model.FormBack.F_2A_2_COMM.TONotNullString() == "Y")
                {
                    if (model.FormBack.F_2A_2.TONotNullString() == "")
                    {
                        ErrorMsg += "請填寫2A.2藥商名稱 。\r\n";
                    }
                    if (model.FormBack.F_2A_2_ADDR.TONotNullString() == "")
                    {
                        ErrorMsg += "請填寫2A.2藥商地址 。\r\n";
                    }
                }
                if (model.FormBack.F_2A_3.TONotNullString() == "")
                {
                    ErrorMsg = "請勾選2A.3藥品許可證持有者之類別 。\r\n";
                }

                model.FormBack.F_2B_2 = null;
                model.FormBack.F_2B_3 = null;
                model.FormBack.F_2B_3_REMARKS = null;

            }
            else
            {
                if (model.FormBack.F_2B_2.TONotNullString() == "")
                {
                    ErrorMsg += "請勾選2B.2申請者之類別 。\r\n";
                }
                if (model.FormBack.F_2B_3.TONotNullString() == "")
                {
                    ErrorMsg += "請選擇2B.3僅供外銷專用之原因 。\r\n";
                }

                // 選否不需要有值
                model.FormBack.F_2A_2_COMM = null;
                model.FormBack.F_2A_2 = null;
                model.FormBack.F_2A_2_ADDR = null;

                model.FormBack.F_2A_3 = null;
                model.FormBack.F_2A_4 = null;
                model.FormBack.F_2A_5 = null;
                model.FormBack.F_2A_6 = null;
            }
            if(model.F11.GoodsList != null && model.F11.GoodsList.Count() > 0)
            {
                var i = 1;
                foreach (var item in model.F11.GoodsList)
                {
                    if (string.IsNullOrEmpty(item.F11_SCI_NM))
                    {
                        ErrorMsg += "成分內容_生藥名(" + i.TONotNullString() + ")為必選欄位。\r\n";
                    }
                    i++;
                }
            }

            #endregion

            result.status = true;
            result.message = "";
            if (!ErrorMsg.Equals(""))
            {
                result.status = false;
                result.message = ErrorMsg;
            }
            return Content(result.Serialize(), "application/json");
        }

        [HttpPost]
        [DisplayName("005003_補件存檔")]
        public ActionResult DocFinish(Apply_005003ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (sm == null || sm.UserInfo == null) { return RedirectToAction("Index", "History"); }
            ClamMember mem = sm.UserInfo.Member;
            if (mem == null) { return RedirectToAction("Index", "History"); }

            ApplyDAO dao = new ApplyDAO();

            ModelState.Clear();
            string ErrorMsg = "";

            ErrorMsg = dao.AppendApplyDoc005003(model);

            if (model.FormBack.CODE_CD == "4")
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, s_SRV_NAME, s_SRV_ID, "1", ISBACK: true);
            }
            else
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, s_SRV_NAME, s_SRV_ID, "1");
            }

            Apply_005003FormModel Form = new Apply_005003FormModel();
            Form.DOCYN = "Y";
            Form.NAME = model.FormBack.NAME;
            Form.CNT_NAME = model.FormBack.CNT_NAME;
            Form.EMAIL = model.FormBack.EMAIL;
            //sm.LastResultMessage = "送出成功";
            return View("Save", Form);
        }

        /// <summary>
        /// 取得介接資料
        /// </summary>
        /// <param name="Form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInterfaceData(Apply_005003FormModel Form)
        {
            var Data = new SALEAPI();
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();

            if (string.IsNullOrEmpty(Form.F_2A_1_WORD) || string.IsNullOrEmpty(Form.F_2A_1_NUM))
            {
                result.status = false;
                result.message = "請輸入2A.1藥品許可證字號";
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(Form.F_2A_1_NUM, @"^[0-9]+$"))
                {
                    result.status = false;
                    result.message = "2A.1藥品許可證字號請以數字填寫。\r\n";
                }
                else
                {
                    try
                    {
                        //WebTestEnvir 測試環境 Y 啟用
                        if (ConfigModel.WebTestEnvir == "Y")
                        {
                            //介接
                            var temp = "{" +
                                       "\"SALEResp\": {" +
                                       "\"製造廠名稱\": \"台灣順安生物科技製藥有限公司大發廠\"," +
                                       "\"製造廠地址\": \"高雄市大寮區鳳林二路876號\"," +
                                       "\"中文品名\": \"$123$寶泰隆苑志牛黃清心丸（去麝香、去羚羊角、去犀角）\"," +
                                       "\"英文品名\": \"qwer\"," +
                                       "\"藥品劑型\": \"丸劑\"," +
                                       "\"發證日期\": \"2005/06/27\"," +
                                       "\"有效日期\": \"2020/09/30\"," +
                                       "\"處方說明\": \"每丸(3g)中含有：\"," +
                                       "\"效能\": \"\"," +
                                       "\"適應症\": \"\"," +
                                       "\"限制1\": \"外銷專用\"," +
                                       "\"限制2\": \"\"," +
                                       "\"限制3\": \"\"," +
                                       "\"限制4\": \"\"," +
                                       "\"外銷品名\": [" +
                                       "{" +
                                       "\"外銷中文品名\": \"q中文1\"," +
                                       "\"外銷英文品名\": \"qwe\"" +
                                       "}," +
                                       "{" +
                                       "\"外銷中文品名\": \"q中文2\"," +
                                       "\"外銷英文品名\": \"qwer\"" +
                                       "}" +
                                       "]," +
                                       "\"成分說明\": [" +
                                       "{" +
                                       "\"成分內容\": \"牛黃\"," +
                                       "\"份量\": \"51.3\"," +
                                       "\"單位\": \"mg\"" +
                                       "}," +
                                       "{" +
                                       "\"成分內容\": \"柴胡\"," +
                                       "\"份量\": \"51.3\"," +
                                       "\"單位\": \"mg\"" +
                                       "}," +
                                       "{" +
                                       "\"成分內容\": \"桔梗\"," +
                                       "\"份量\": \"51.3\"," +
                                       "\"單位\": \"mg\"" +
                                       "}" +
                                       "]" +
                                       "}," +
                                       "\"STATUS\": \"成功\"" +
                                       "}";
                            Data = JsonConvert.DeserializeObject<SALEAPI>(temp);
                        }
                        else
                        {
                            APIDAO api = new APIDAO();
                            Data = api.GetSALELicense(Form.F_2A_1_WORD, Form.F_2A_1_NUM);
                        }

                        if (Data.STATUS == "成功"|| Data.STATUS == "外銷專用")
                        {
                            //資料處理
                            Data.SALEResp = dao.SortData005003(Data.SALEResp);

                            result.status = true;
                            result.message = "成功";
                            result.data = Data.SALEResp;
                        }
                        //else if (Data.STATUS == "外銷專用")
                        //{
                        //    result.status = false;
                        //    result.message = "輸入之藥品許可證為外銷專用，請改為申請外銷證明書。";
                        //}
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
                    catch (Exception ex)
                    {
                        logger.Error("005003_GetInterfaceData failed:" + ex.TONotNullString());
                        result.status = false;
                        result.message = "取得資料失敗";
                    }

                }
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 取得介接資料
        /// </summary>
        /// <param name="Form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInterfaceDataAppDoc(Apply_005003ViewModel Form)
        {
            var Data = new SALEAPI();
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();

            if (string.IsNullOrEmpty(Form.FormBack.F_2A_1_WORD) || string.IsNullOrEmpty(Form.FormBack.F_2A_1_NUM))
            {
                result.status = false;
                result.message = "請輸入2A.1藥品許可證字號";
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(Form.FormBack.F_2A_1_NUM, @"^[0-9]+$"))
                {
                    result.status = false;
                    result.message = "2A.1藥品許可證字號請以數字填寫。\r\n";
                }
                else
                {
                    try
                    {
                        //WebTestEnvir 測試環境 Y 啟用
                        if (ConfigModel.WebTestEnvir == "Y")
                        {
                            //介接
                            var temp = "{" +
                                       "\"WHOPMPE\": {" +
                                       "\"製造廠名稱\": \"台灣順安生物科技製藥有限公司大發廠\"," +
                                       "\"製造廠地址\": \"高雄市大寮區鳳林二路876號\"," +
                                       "\"中文品名\": \"寶泰隆苑志牛黃清心丸（去麝香、去羚羊角、去犀角）\"," +
                                       "\"英文品名\": \"\"," +
                                       "\"藥品劑型\": \"丸劑\"," +
                                       "\"發證日期\": \"2005/06/27\"," +
                                       "\"有效日期\": \"2020/09/30\"," +
                                       "\"處方說明\": \"每丸(3g)中含有：\"," +
                                       "\"效能\": \"\"," +
                                       "\"適應症\": \"\"," +
                                       "\"限制1\": \"\"," +
                                       "\"限制2\": \"\"," +
                                       "\"限制3\": \"\"," +
                                       "\"限制4\": \"\"," +
                                       "\"外銷品名\": [" +
                                       "{" +
                                       "\"外銷中文品名\": \"中文1\"," +
                                       "\"外銷英文品名\": \"qwe\"" +
                                       "}," +
                                       "{" +
                                       "\"外銷中文品名\": \"中文2\"," +
                                       "\"外銷英文品名\": \"qwer\"" +
                                       "}" +
                                       "]," +
                                       "\"成分說明\": [" +
                                       "{" +
                                       "\"成分內容\": \"牛黃\"," +
                                       "\"份量\": \"51.3\"," +
                                       "\"單位\": \"mg\"" +
                                       "}," +
                                       "{" +
                                       "\"成分內容\": \"柴胡\"," +
                                       "\"份量\": \"51.3\"," +
                                       "\"單位\": \"mg\"" +
                                       "}," +
                                       "{" +
                                       "\"成分內容\": \"桔梗\"," +
                                       "\"份量\": \"51.3\"," +
                                       "\"單位\": \"mg\"" +
                                       "}" +
                                       "]" +
                                       "}," +
                                       "\"STATUS\": \"成功\"" +
                                       "}";
                            Data = JsonConvert.DeserializeObject<SALEAPI>(temp);
                        }
                        else
                        {
                            APIDAO api = new APIDAO();
                            Data = api.GetSALELicense(Form.FormBack.F_2A_1_WORD, Form.FormBack.F_2A_1_NUM);
                        }

                        if (Data.STATUS == "成功")
                        {
                            //資料處理
                            Data.SALEResp = dao.SortData005003(Data.SALEResp);

                            result.status = true;
                            result.message = "成功";
                            result.data = Data.SALEResp;
                        }
                        else if (Data.STATUS == "外銷專用")
                        {
                            result.status = false;
                            result.message = "輸入之藥品許可證為外銷專用，請改為申請外銷證明書。";
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
                    catch (Exception ex)
                    {
                        logger.Error("005003_GetInterfaceData failed:" + ex.TONotNullString());
                        result.status = false;
                        result.message = "取得資料失敗";
                    }

                }
            }

            return Content(result.Serialize(), "application/json");
        }
    }

}


