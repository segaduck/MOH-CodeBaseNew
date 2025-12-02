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
using System.Net.Mail;
using ES.Models;
using System.Reflection;
using System.Web.UI.WebControls;
using Spire.License;
using System.Drawing;
using System.Globalization;
using ES.Models.ChineseMedicineAPI;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace ES.Controllers
{
    public class Apply_005001Controller : BaseController
    {
        public static string s_SRV_ID = "005001";
        public static string s_SRV_NAME = "產銷證明書";

        [HttpGet]
        public ActionResult Prompt()
        {
            SessionModel sm = SessionModel.Get();
            string s_msg_1A = "請先閱讀 「{0}說明事項」點選同意後，再進入申辦頁面 !";
            sm.LastErrorMessage = string.Format(s_msg_1A, s_SRV_NAME);
            return View("Prompt005001");
        }

        [DisplayName("005001_申請案件")]
        public ActionResult Apply(string agree)
        {
            Apply_005001FormModel Form = new Apply_005001FormModel();
            SessionModel sm = SessionModel.Get();
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
            Form.PAYAMOUNT = 1500;
            Form.PAYCOUNT = "1";
            Form.IS_PREVIEW = false;


            return View("Index", Form);
        }

        [HttpPost]
        [DisplayName("005001_申請案件送出")]
        public ActionResult Apply(Apply_005001FormModel form)
        {
            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            string ErrorMsg = "";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\""\'\:\：\“\”]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            System.Text.RegularExpressions.Regex reg4 = new System.Text.RegularExpressions.Regex(@"^[\u4e00-\u9fa5]+$");
            System.Text.RegularExpressions.Regex reg5 = new System.Text.RegularExpressions.Regex(@"^[0-9\-\#]+$");

            if (ModelState.IsValid)
            {
                //共用欄位驗證
                ErrorMsg = dao.checkApply(form);
                //個別欄位驗證
                if (!string.IsNullOrEmpty(form.PL_Num))
                {
                    if (!reg2.IsMatch(form.PL_Num))
                    {
                        ErrorMsg += "藥品許可執照字號(中文)請以數字填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.PL_Num_E))
                {
                    if (!reg2.IsMatch(form.PL_Num_E))
                    {
                        ErrorMsg += "藥品許可執照字號(英文)請以數字填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.MF_CNT_NAME_E))
                {
                    if (!reg.IsMatch(form.MF_CNT_NAME_E))
                    {
                        ErrorMsg += "製造廠名稱(英文)請以英文填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.MF_ADDR_E))
                {
                    if (!reg.IsMatch(form.MF_ADDR_E))
                    {
                        ErrorMsg += "製造廠地址(英文)請以英文填寫。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.DRUG_NAME_E))
                {
                    if (!reg.IsMatch(form.DRUG_NAME_E))
                    {
                        ErrorMsg += "藥品名稱(英文)請以英文填寫。\r\n";
                    }
                }

                if (form.IS_DRUG_ABROAD_CHECK)
                {
                    if (!string.IsNullOrEmpty(form.DRUG_ABROAD_NAME_E))
                    {
                        if (!reg.IsMatch(form.DRUG_ABROAD_NAME_E))
                        {
                            ErrorMsg += "外銷品名(英文)請以英文填寫。\r\n";
                        }
                    }

                    if (string.IsNullOrEmpty(form.DRUG_ABROAD_NAME_E) && string.IsNullOrEmpty(form.DRUG_ABROAD_NAME))
                    {
                        ErrorMsg += "外銷名品(中文/英文)欄位需擇一填入。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.DOSAGE_FORM_E))
                {
                    if (!reg.IsMatch(form.DOSAGE_FORM_E))
                    {
                        ErrorMsg += "劑型(英文)請以英文填寫。\r\n";
                    }
                }

                if (form.IS_EXPIR_DATE_CHECK)
                {
                    if (string.IsNullOrEmpty(form.EXPIR_DATE))
                    {
                        ErrorMsg += "有效日期(中文) 為必填欄位。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.MF_CONT))
                {
                    if (form.MF_CONT.Trim().Substring(form.MF_CONT.Trim().Length - 1, 1) != "：")
                    {
                        ErrorMsg += "處方說明(中文)最後請以「：」結尾。\r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.EMAIL))
                {
                    if (!reg3.IsMatch(form.EMAIL))
                    {
                        ErrorMsg += "請填入正確的Email格式 ! \r\n";
                    }
                }

                if (!string.IsNullOrEmpty(form.MF_CONT_E))
                {
                    if (form.MF_CONT_E.Trim().Substring(form.MF_CONT_E.Trim().Length - 1, 1) != ":" && form.MF_CONT_E.Trim().Substring(form.MF_CONT_E.Trim().Length - 1, 1) != "：")
                    {
                        ErrorMsg += "處方說明(英文)最後請以「:」結尾。\r\n";
                    }

                    if (!reg.IsMatch(form.MF_CONT_E))
                    {
                        ErrorMsg += "處方說明(英文)請以英文填寫。\r\n";
                    }
                }

                if (form.IS_Concentrate_CHECK)
                {
                    if (string.IsNullOrEmpty(form.PC_SCALE_1) || string.IsNullOrEmpty(form.PC_SCALE_1E) ||
                        string.IsNullOrEmpty(form.PC_SCALE_21) || string.IsNullOrEmpty(form.PC_SCALE_22) ||
                        string.IsNullOrEmpty(form.PC_SCALE_23) || string.IsNullOrEmpty(form.PC_SCALE_24))
                    {
                        ErrorMsg += "請輸入生藥與浸膏比例中文英文完整資訊。\r\n";
                    }
                    else
                    {
                        if (!(reg1.IsMatch(form.PC_SCALE_1) && reg1.IsMatch(form.PC_SCALE_21) &&
                              reg1.IsMatch(form.PC_SCALE_22) && reg1.IsMatch(form.PC_SCALE_23) &&
                              reg1.IsMatch(form.PC_SCALE_24)))
                        {
                            ErrorMsg += "請輸入生藥與浸膏比例中文英文請填寫數字。\r\n";
                        }
                    }

                    if (form.PC.GoodsList.Count == 1 && (string.IsNullOrEmpty(form.PC.GoodsList[0].PC_NAME)) || string.IsNullOrEmpty(form.PC.GoodsList[0].PC_ENAME))
                    {
                        ErrorMsg += "請輸入賦形劑完整資訊。\r\n";
                    }
                }

                if (form.IS_INDIOCATION_CHECK)
                {
                    //適應症(中文)
                    if (!string.IsNullOrEmpty(form.INDIOCATION))
                    {
                        if (form.INDIOCATION.Trim().Substring(form.INDIOCATION.Trim().Length - 1, 1) != "。")
                        {
                            ErrorMsg += "適應症(中文)最後請以「。」結尾。\r\n";
                        }
                    }
                    else
                    {
                        ErrorMsg += "勾選適應症時請填寫適應症(中文)資訊。\r\n";
                    }

                    //適應症(英文)
                    if (!string.IsNullOrEmpty(form.INDIOCATION_E))
                    {
                        if (form.INDIOCATION_E.Trim().Substring(form.INDIOCATION_E.Trim().Length - 1, 1) != ".")
                        {
                            ErrorMsg += "適應症(英文)最後請以「.」結尾。\r\n";
                        }

                        if (!reg.IsMatch(form.INDIOCATION_E))
                        {
                            ErrorMsg += "適應症(英文)請以英文填寫。\r\n";
                        }
                    }
                    else
                    {
                        ErrorMsg += "勾選適應症時請填寫適應症(英文)資訊。\r\n";
                    }
                }

                if (form.IS_EFFICACY_CHECK)
                {
                    //效能(中文)
                    if (!string.IsNullOrEmpty(form.EFFICACY))
                    {
                        if (form.EFFICACY.Trim().Substring(form.EFFICACY.Trim().Length - 1, 1) != "。")
                        {
                            ErrorMsg += "效能(中文)最後請以「。」結尾。\r\n";
                        }
                    }
                    else
                    {
                        ErrorMsg += "勾選效能時請填寫效能(中文)資訊。\r\n";
                    }

                    //效能(英文)
                    if (!string.IsNullOrEmpty(form.EFFICACY_E))
                    {
                        if (form.EFFICACY_E.Trim().Substring(form.EFFICACY_E.Trim().Length - 1, 1) != ".")
                        {
                            ErrorMsg += "效能(英文)最後請以「.」結尾。\r\n";
                        }

                        if (!reg.IsMatch(form.EFFICACY_E))
                        {
                            ErrorMsg += "效能(英文)請以英文填寫。\r\n";
                        }
                    }
                    else
                    {
                        ErrorMsg += "勾選效能時請填寫效能(英文)資訊。\r\n";
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

                if (form.RADIOYN == "Y")
                {
                    if (string.IsNullOrEmpty(form.Name_File_1) && string.IsNullOrEmpty(form.Name_File_2) &&
                        string.IsNullOrEmpty(form.Name_File_3))
                    {
                        ErrorMsg += "請至少上傳一筆檔案!\r\n";
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
        [DisplayName("005001_申請案件預覽送出")]
        public ActionResult PreView(Apply_005001FormModel Form)
        {
            Form.IsMode = "1";
            Form.FileSave();
            Form.DI.IsReadOnly = true;
            Form.PC.IsReadOnly = true;
            Form.IS_PREVIEW = true;
            Form.FAX = Form.FAX_Extension.TONotNullString().ToTrim() == "" ? Form.FAX_BEFORE + "-" + Form.FAX_AFTER : Form.FAX_BEFORE + "-" + Form.FAX_AFTER + "#" + Form.FAX_Extension;
            return View("PreView005001", Form);
        }

        [HttpPost]
        [DisplayName("005001_申請案件完成")]
        public ActionResult Save(Apply_005001FormModel Form)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            Form.APP_ID = dao.AppendApply005001(Form);

            string MailBody = "<table align=\"left\" style=\"width:90%;\">";
            MailBody += " <tr><th align=\"left\">敬啟者:</th></tr>";
            MailBody += " <tr><td>貴公司申請產銷證明書(案號：" + Form.APP_ID + ")一案，「人民申請案線上申辦服務系統」已收件，尚待貴公司繳納規費，請提供申請者聯絡資訊、申請案類別、案件編號及規費金額等資料，連同欲繳納之匯票或現金，寄到「115204臺北市南港區忠孝東路六段488號 衛生福利部中醫藥司收」，敬請配合。</td></tr>";
            MailBody += " <tr><td><br /></td></tr>";
            MailBody += " <tr><td>衛生福利部 中醫藥司</td></tr>";
            MailBody += "</table>";

            dao.SendMail_New(Form.CNT_NAME, Form.EMAIL, Form.APP_ID, "產銷證明書", "005001", MailBody);

            sm.LastResultMessage = "送出成功";
            return View("Save", Form);
        }

        #region 申請表套印

        #region 舊版

        //[HttpPost]
        //[DisplayName("005001_申請單套表")]
        //public void PreviewApplyForm(Apply_005001FormModel Form)
        //{
        //    byte[] buffer = null;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        Document doc = new Document();
        //        Section s = doc.AddSection();
        //        Paragraph para1 = s.AddParagraph();
        //        para1.AppendText("中   華   民   國   衛   生   福   利   部");
        //        Paragraph para2 = s.AddParagraph();
        //        para2.AppendText("MINISTRY OF HEALTH AND WELFARE, THE EXECUTIVE YUAN" + "\r\n" + "REPUBLIC OF CHINA");
        //        Paragraph para3 = s.AddParagraph();
        //        para3.AppendText("Date:____________                                                                           No:____________");
        //        Paragraph para4 = s.AddParagraph();
        //        para4.AppendText("證  明  書");
        //        Paragraph para5 = s.AddParagraph();
        //        para5.AppendText("Certificate").CharacterFormat.UnderlineStyle = UnderlineStyle.Single;
        //        Paragraph para6 = s.AddParagraph();
        //        para6.AppendText("\r\n茲證明下述藥品經衛生福利部核准許可登記，准予產銷。\r\n該藥品之製造廠亦經評定已實施經濟及能源部與衛生福利部聯合公布推行之\r\n" +
        //                         "藥品優良製造規範，並接受定期與不定期之稽查。");
        //        Paragraph para12 = s.AddParagraph();
        //        para12.AppendText("Ministry of Health and Welfare, The Executive Yuan of the Republic of China hereby certifies\r\n" +
        //                          "that the product as described below is subject to its jurisdiction and is legally\r\n" +
        //                          "approved for distribution within the Republic of China.\r\n" +
        //                          "It is also certified that the manufacturing establishment has been in compliance\r\n" +
        //                          "with the requirements for Good Manufacturing Practices as jointly promulgated by\r\n" +
        //                          "the Ministry of Economic and Energy Affairs and Ministry of Health and Welfare, The Executive Yuan,\r\n" +
        //                          "R.O.C.and is subject to inspections at appropriate intervals.\r\n");

        //        if (Form.IS_DRUG_ABROAD_CHECK)
        //        {
        //            //加入table
        //            Spire.Doc.Table sTable = s.AddTable(true);
        //            sTable.ResetCells(14, 6);
        //            //合併表格
        //            sTable.ApplyHorizontalMerge(0, 0, 5);
        //            sTable.ApplyHorizontalMerge(1, 0, 5);
        //            sTable.ApplyHorizontalMerge(2, 0, 5);
        //            sTable.ApplyHorizontalMerge(3, 0, 5);
        //            sTable.ApplyHorizontalMerge(4, 0, 5);
        //            sTable.ApplyHorizontalMerge(5, 0, 5);

        //            sTable.ApplyHorizontalMerge(6, 0, 3);
        //            sTable.ApplyHorizontalMerge(7, 0, 3);
        //            sTable.ApplyHorizontalMerge(8, 0, 3);
        //            sTable.ApplyHorizontalMerge(9, 0, 3);
        //            sTable.ApplyHorizontalMerge(10, 0, 3);
        //            sTable.ApplyHorizontalMerge(11, 0, 3);
        //            sTable.ApplyHorizontalMerge(12, 0, 3);
        //            sTable.ApplyHorizontalMerge(13, 0, 3);
        //            //邊框隱藏
        //            Spire.Doc.Table table = (Spire.Doc.Table)s.Tables[0];
        //            table.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;

        //            //表格下邊框設定
        //            for (var i = 0; i < 6; i++)
        //            {
        //                Spire.Doc.TableCell cell1 = sTable[1, i];
        //                Spire.Doc.TableCell cell2 = sTable[4, i];
        //                Spire.Doc.TableCell cell3 = sTable[7, i];
        //                Spire.Doc.TableCell cell4 = sTable[10, i];
        //                Spire.Doc.TableCell cell5 = sTable[13, i];
        //                cell1.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //                cell2.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //                cell3.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //                cell4.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //                cell5.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //            }

        //            //空行設定
        //            Spire.Doc.Fields.TextRange rangeNull1 = sTable[2, 0].AddParagraph().AppendText(" ");
        //            rangeNull1.CharacterFormat.FontSize = 8;
        //            Spire.Doc.Fields.TextRange rangeNull2 = sTable[5, 0].AddParagraph().AppendText(" ");
        //            rangeNull2.CharacterFormat.FontSize = 8;
        //            Spire.Doc.Fields.TextRange rangeNull3 = sTable[8, 0].AddParagraph().AppendText(" ");
        //            rangeNull3.CharacterFormat.FontSize = 8;
        //            Spire.Doc.Fields.TextRange rangeNull4 = sTable[11, 0].AddParagraph().AppendText(" ");
        //            rangeNull4.CharacterFormat.FontSize = 8;

        //            //製造廠名稱(中文)
        //            Spire.Doc.Fields.TextRange range1 = sTable[0, 0].AddParagraph().AppendText("製造廠名稱：" + Form.MF_CNT_NAME);
        //            range1.CharacterFormat.FontName = "新細明體";
        //            range1.CharacterFormat.FontSize = 8;

        //            //製造廠名稱(英文)
        //            Spire.Doc.Fields.TextRange range2 = sTable[1, 0].AddParagraph().AppendText("Manufacturer: " + Form.MF_CNT_NAME_E);
        //            range2.CharacterFormat.FontName = "Times New Roman";
        //            range2.CharacterFormat.FontSize = 8;

        //            //製造廠地址(中文)
        //            Spire.Doc.Fields.TextRange range3 = sTable[3, 0].AddParagraph().AppendText("製造廠地址：" + "中華民國台灣" + Form.TAX_ORG_CITY_TEXT + Form.TAX_ORG_CITY_DETAIL);
        //            range3.CharacterFormat.FontName = "新細明體";
        //            range3.CharacterFormat.FontSize = 8;

        //            //製造廠地址(英文)
        //            Spire.Doc.Fields.TextRange range4 = sTable[4, 0].AddParagraph().AppendText("Manufacturing Plant Location: " + Form.MF_ADDR_E);
        //            range4.CharacterFormat.FontName = "Times New Roman";
        //            range4.CharacterFormat.FontSize = 8;

        //            //藥品名稱(中文)
        //            Spire.Doc.Fields.TextRange range5 = sTable[6, 0].AddParagraph().AppendText("藥品名稱：" + Form.DRUG_NAME);
        //            range5.CharacterFormat.FontName = "新細明體";
        //            range5.CharacterFormat.FontSize = 8;

        //            //藥品名稱(英文)
        //            Spire.Doc.Fields.TextRange range6 = sTable[7, 0].AddParagraph().AppendText("Product Name: " + Form.DRUG_NAME_E);
        //            range6.CharacterFormat.FontName = "Times New Roman";
        //            range6.CharacterFormat.FontSize = 8;

        //            //劑型(中文)
        //            Spire.Doc.Fields.TextRange range7 = sTable[6, 4].AddParagraph().AppendText("劑型：" + Form.DOSAGE_FORM);
        //            range7.CharacterFormat.FontName = "新細明體";
        //            range7.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range8 = sTable[6, 5].AddParagraph().AppendText(Form.DOSAGE_FORM);
        //            //range8.CharacterFormat.FontName = "新細明體";
        //            //range8.CharacterFormat.FontSize = 8;

        //            //劑型(英文)
        //            Spire.Doc.Fields.TextRange range9 = sTable[7, 4].AddParagraph().AppendText("Dosage Form: " + Form.DOSAGE_FORM_E);
        //            range9.CharacterFormat.FontName = "Times New Roman";
        //            range9.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range10 = sTable[7, 5].AddParagraph().AppendText(Form.DOSAGE_FORM_E);
        //            //range10.CharacterFormat.FontName = "Times New Roman";
        //            //range10.CharacterFormat.FontSize = 8;

        //            //外銷品名(中文)
        //            Spire.Doc.Fields.TextRange range23 = sTable[9, 0].AddParagraph().AppendText("外銷品名：" + Form.DRUG_ABROAD_NAME);
        //            range23.CharacterFormat.FontName = "新細明體";
        //            range23.CharacterFormat.FontSize = 8;

        //            //外銷品名(英文)
        //            Spire.Doc.Fields.TextRange range25 = sTable[10, 0].AddParagraph().AppendText("Export Name: " + Form.DRUG_ABROAD_NAME_E);
        //            range25.CharacterFormat.FontName = "Times New Roman";
        //            range25.CharacterFormat.FontSize = 8;

        //            if (Form.IS_EXPIR_DATE_CHECK)
        //            {
        //                //有效日期(中文)
        //                // 合併欄位
        //                sTable.ApplyHorizontalMerge(9, 4, 5);
        //                var dateTW = Form.EXPIR_DATE_TW.Split('/');
        //                var dateIns = dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日";
        //                Spire.Doc.Fields.TextRange range15 = sTable[9, 4].AddParagraph().AppendText($"有效日期：{dateIns}");
        //                range15.CharacterFormat.FontName = "新細明體";
        //                range15.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range16 = sTable[9, 5].AddParagraph().AppendText(dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日");
        //                //range16.CharacterFormat.FontName = "新細明體";
        //                //range16.CharacterFormat.FontSize = 8;

        //                //有效日期(英文)
        //                sTable.ApplyHorizontalMerge(10, 4, 5);
        //                var dateInsE = Convert.ToDateTime(Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //                Spire.Doc.Fields.TextRange range17 = sTable[10, 4].AddParagraph().AppendText($"Date of Issue:{dateInsE}");
        //                range17.CharacterFormat.FontName = "Times New Roman";
        //                range17.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range18 = sTable[10, 5].AddParagraph().AppendText(Convert.ToDateTime(Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //                //range18.CharacterFormat.FontName = "Times New Roman";
        //                //range18.CharacterFormat.FontSize = 8;
        //            }

        //            //許可證字號(中文)
        //            sTable.ApplyHorizontalMerge(12, 0, 1);
        //            var plcdIns = Form.PL_CD_TEXT + "第" + Form.PL_Num + "號";
        //            Spire.Doc.Fields.TextRange range11 = sTable[12, 0].AddParagraph().AppendText($"許可證字號：{plcdIns}");
        //            range11.CharacterFormat.FontName = "新細明體";
        //            range11.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range12 = sTable[12, 1].AddParagraph().AppendText(Form.PL_CD_TEXT + "第" + Form.PL_Num + "號");
        //            //range12.CharacterFormat.FontName = "新細明體";
        //            //range12.CharacterFormat.FontSize = 8;

        //            //許可證字號(英文)
        //            sTable.ApplyHorizontalMerge(13, 0, 1);
        //            var plcdeIns = Form.PL_CD_E + "-" + Form.PL_Num_E;
        //            Spire.Doc.Fields.TextRange range13 = sTable[13, 0].AddParagraph().AppendText($"Registration Number:{plcdeIns}");
        //            range13.CharacterFormat.FontName = "Times New Roman";
        //            range13.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range14 = sTable[13, 1].AddParagraph().AppendText(Form.PL_CD_E + "-" + Form.PL_Num_E);
        //            //range14.CharacterFormat.FontName = "Times New Roman";
        //            //range14.CharacterFormat.FontSize = 8;

        //            //核准日期(中文)
        //            sTable.ApplyHorizontalMerge(12, 4, 5);
        //            var dateTW1 = Form.ISSUE_DATE_TW.Split('/');
        //            var dateTW1Ins = dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日";
        //            Spire.Doc.Fields.TextRange range19 = sTable[12, 4].AddParagraph().AppendText($"核准日期：{dateTW1Ins}");
        //            range19.CharacterFormat.FontName = "新細明體";
        //            range19.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range20 = sTable[12, 5].AddParagraph().AppendText(dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日");
        //            //range20.CharacterFormat.FontName = "新細明體";
        //            //range20.CharacterFormat.FontSize = 8;

        //            //核准日期(英文)
        //            sTable.ApplyHorizontalMerge(13, 4, 5);
        //            var issIns = Convert.ToDateTime(Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //            Spire.Doc.Fields.TextRange range21 = sTable[13, 4].AddParagraph().AppendText($"Date of Issue:{issIns}");
        //            range21.CharacterFormat.FontName = "Times New Roman";
        //            range21.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range22 = sTable[13, 5].AddParagraph().AppendText(Convert.ToDateTime(Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //            //range22.CharacterFormat.FontName = "Times New Roman";
        //            //range22.CharacterFormat.FontSize = 8;
        //        }
        //        else
        //        {
        //            //加入table
        //            Spire.Doc.Table sTable = s.AddTable(true);
        //            sTable.ResetCells(11, 6);
        //            //合併表格
        //            sTable.ApplyHorizontalMerge(0, 0, 5);
        //            sTable.ApplyHorizontalMerge(1, 0, 5);
        //            sTable.ApplyHorizontalMerge(2, 0, 5);
        //            sTable.ApplyHorizontalMerge(3, 0, 5);
        //            sTable.ApplyHorizontalMerge(4, 0, 5);
        //            sTable.ApplyHorizontalMerge(5, 0, 5);
        //            sTable.ApplyHorizontalMerge(6, 0, 3);
        //            sTable.ApplyHorizontalMerge(7, 0, 3);
        //            sTable.ApplyHorizontalMerge(8, 0, 5);
        //            //邊框隱藏
        //            Spire.Doc.Table table = (Spire.Doc.Table)s.Tables[0];
        //            table.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;

        //            //表格下邊框設定
        //            for (var i = 0; i < 6; i++)
        //            {
        //                Spire.Doc.TableCell cell1 = sTable[1, i];
        //                Spire.Doc.TableCell cell2 = sTable[4, i];
        //                Spire.Doc.TableCell cell3 = sTable[7, i];
        //                Spire.Doc.TableCell cell4 = sTable[10, i];
        //                cell1.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //                cell2.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //                cell3.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //                cell4.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
        //            }

        //            //空行設定
        //            Spire.Doc.Fields.TextRange rangeNull1 = sTable[2, 0].AddParagraph().AppendText(" ");
        //            rangeNull1.CharacterFormat.FontSize = 8;
        //            Spire.Doc.Fields.TextRange rangeNull2 = sTable[5, 0].AddParagraph().AppendText(" ");
        //            rangeNull2.CharacterFormat.FontSize = 8;
        //            Spire.Doc.Fields.TextRange rangeNull3 = sTable[8, 0].AddParagraph().AppendText(" ");
        //            rangeNull3.CharacterFormat.FontSize = 8;

        //            //製造廠名稱(中文)
        //            Spire.Doc.Fields.TextRange range1 = sTable[0, 0].AddParagraph().AppendText("製造廠名稱：" + Form.MF_CNT_NAME);
        //            range1.CharacterFormat.FontName = "新細明體";
        //            range1.CharacterFormat.FontSize = 8;

        //            //製造廠名稱(英文)
        //            Spire.Doc.Fields.TextRange range2 = sTable[1, 0].AddParagraph().AppendText("Manufacturer:" + Form.MF_CNT_NAME_E);
        //            range2.CharacterFormat.FontName = "Times New Roman";
        //            range2.CharacterFormat.FontSize = 8;

        //            //製造廠地址(中文)
        //            Spire.Doc.Fields.TextRange range3 = sTable[3, 0].AddParagraph().AppendText("製造廠地址：" + "中華民國台灣" + Form.TAX_ORG_CITY_TEXT + Form.TAX_ORG_CITY_DETAIL);
        //            range3.CharacterFormat.FontName = "新細明體";
        //            range3.CharacterFormat.FontSize = 8;

        //            //製造廠地址(英文)
        //            Spire.Doc.Fields.TextRange range4 = sTable[4, 0].AddParagraph().AppendText("Manufacturing Plant Location:" + Form.MF_ADDR_E);
        //            range4.CharacterFormat.FontName = "Times New Roman";
        //            range4.CharacterFormat.FontSize = 8;

        //            //藥品名稱(中文)
        //            Spire.Doc.Fields.TextRange range5 = sTable[6, 0].AddParagraph().AppendText("藥品名稱：" + Form.DRUG_NAME);
        //            range5.CharacterFormat.FontName = "新細明體";
        //            range5.CharacterFormat.FontSize = 8;

        //            //藥品名稱(英文)
        //            Spire.Doc.Fields.TextRange range6 = sTable[7, 0].AddParagraph().AppendText("Product Name:" + Form.DRUG_NAME_E);
        //            range6.CharacterFormat.FontName = "Times New Roman";
        //            range6.CharacterFormat.FontSize = 8;

        //            //劑型(中文)
        //            Spire.Doc.Fields.TextRange range7 = sTable[6, 4].AddParagraph().AppendText("劑型：");
        //            range7.CharacterFormat.FontName = "新細明體";
        //            range7.CharacterFormat.FontSize = 8;
        //            Spire.Doc.Fields.TextRange range8 = sTable[6, 5].AddParagraph().AppendText(Form.DOSAGE_FORM);
        //            range8.CharacterFormat.FontName = "新細明體";
        //            range8.CharacterFormat.FontSize = 8;

        //            //劑型(英文)
        //            Spire.Doc.Fields.TextRange range9 = sTable[7, 4].AddParagraph().AppendText("Dosage Form:");
        //            range9.CharacterFormat.FontName = "Times New Roman";
        //            range9.CharacterFormat.FontSize = 8;
        //            Spire.Doc.Fields.TextRange range10 = sTable[7, 5].AddParagraph().AppendText(Form.DOSAGE_FORM_E);
        //            range10.CharacterFormat.FontName = "Times New Roman";
        //            range10.CharacterFormat.FontSize = 8;

        //            if (Form.IS_EXPIR_DATE_CHECK)
        //            {
        //                //許可證字號(中文)
        //                sTable.ApplyHorizontalMerge(9, 0, 1);
        //                var plcdIns = Form.PL_CD_TEXT + "第" + Form.PL_Num + "號";
        //                Spire.Doc.Fields.TextRange range11 = sTable[9, 0].AddParagraph().AppendText($"許可證字號：{plcdIns}");
        //                range11.CharacterFormat.FontName = "新細明體";
        //                range11.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range12 = sTable[9, 1].AddParagraph().AppendText(Form.PL_CD_TEXT + "第" + Form.PL_Num + "號");
        //                //range12.CharacterFormat.FontName = "新細明體";
        //                //range12.CharacterFormat.FontSize = 8;

        //                //許可證字號(英文)
        //                sTable.ApplyHorizontalMerge(10, 0, 1);
        //                var plcdeIns = Form.PL_CD_E + "-" + Form.PL_Num_E;
        //                Spire.Doc.Fields.TextRange range13 = sTable[10, 0].AddParagraph().AppendText($"Registration Number:{plcdeIns}");
        //                range13.CharacterFormat.FontName = "Times New Roman";
        //                range13.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range14 = sTable[10, 1].AddParagraph().AppendText(Form.PL_CD_E + "-" + Form.PL_Num_E);
        //                //range14.CharacterFormat.FontName = "Times New Roman";
        //                //range14.CharacterFormat.FontSize = 8;
        //            }
        //            else
        //            {
        //                //許可證字號(中文)
        //                sTable.ApplyHorizontalMerge(9, 0, 3);
        //                var plcdIns = Form.PL_CD_TEXT + "第" + Form.PL_Num + "號";
        //                Spire.Doc.Fields.TextRange range11 = sTable[9, 0].AddParagraph().AppendText($"許可證字號：{plcdIns}");
        //                range11.CharacterFormat.FontName = "新細明體";
        //                range11.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range12 = sTable[9, 1].AddParagraph().AppendText(Form.PL_CD_TEXT + "第" + Form.PL_Num + "號");
        //                //range12.CharacterFormat.FontName = "新細明體";
        //                //range12.CharacterFormat.FontSize = 8;

        //                //許可證字號(英文)
        //                sTable.ApplyHorizontalMerge(10, 0, 3);
        //                var plcdeIns = Form.PL_CD_E + "-" + Form.PL_Num_E;
        //                Spire.Doc.Fields.TextRange range13 = sTable[10, 0].AddParagraph().AppendText($"Registration Number:{plcdeIns}");
        //                range13.CharacterFormat.FontName = "Times New Roman";
        //                range13.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range14 = sTable[10, 1].AddParagraph().AppendText(Form.PL_CD_E + "-" + Form.PL_Num_E);
        //                //range14.CharacterFormat.FontName = "Times New Roman";
        //                //range14.CharacterFormat.FontSize = 8;
        //            }
        //            if (Form.IS_EXPIR_DATE_CHECK)
        //            {
        //                //有效日期(中文)
        //                sTable.ApplyHorizontalMerge(9, 2, 3);
        //                var dateTW = Form.EXPIR_DATE_TW.Split('/');
        //                var dateTwIns = dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日";
        //                Spire.Doc.Fields.TextRange range15 = sTable[9, 2].AddParagraph().AppendText($"有效日期：{dateTwIns}");
        //                range15.CharacterFormat.FontName = "新細明體";
        //                range15.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range16 = sTable[9, 3].AddParagraph().AppendText(dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日");
        //                //range16.CharacterFormat.FontName = "新細明體";
        //                //range16.CharacterFormat.FontSize = 8;

        //                //有效日期(英文)
        //                sTable.ApplyHorizontalMerge(10, 2, 3);
        //                var expIns = Convert.ToDateTime(Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //                Spire.Doc.Fields.TextRange range17 = sTable[10, 2].AddParagraph().AppendText($"Date of Issue:{expIns}");
        //                range17.CharacterFormat.FontName = "Times New Roman";
        //                range17.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range18 = sTable[10, 3].AddParagraph().AppendText(Convert.ToDateTime(Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //                //range18.CharacterFormat.FontName = "Times New Roman";
        //                //range18.CharacterFormat.FontSize = 8;
        //            }

        //            //核准日期(中文)
        //            sTable.ApplyHorizontalMerge(9, 4, 5);
        //            var dateTW1 = Form.ISSUE_DATE_TW.Split('/');
        //            var dateTW1Ins = dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日";
        //            Spire.Doc.Fields.TextRange range19 = sTable[9, 4].AddParagraph().AppendText($"核准日期：{dateTW1Ins}");
        //            range19.CharacterFormat.FontName = "新細明體";
        //            range19.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range20 = sTable[9, 5].AddParagraph().AppendText(dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日");
        //            //range20.CharacterFormat.FontName = "新細明體";
        //            //range20.CharacterFormat.FontSize = 8;

        //            //核准日期(英文)
        //            sTable.ApplyHorizontalMerge(10, 4, 5);
        //            var issIns = Convert.ToDateTime(Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //            Spire.Doc.Fields.TextRange range21 = sTable[10, 4].AddParagraph().AppendText($"Date of Issue:{issIns}");
        //            range21.CharacterFormat.FontName = "Times New Roman";
        //            range21.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range22 = sTable[10, 5].AddParagraph().AppendText(Convert.ToDateTime(Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //            //range22.CharacterFormat.FontName = "Times New Roman";
        //            //range22.CharacterFormat.FontSize = 8;
        //        }

        //        Paragraph para7 = s.AddParagraph();
        //        para7.AppendText("\r\n處  方：" + Form.MF_CONT);

        //        Paragraph para13 = s.AddParagraph();
        //        para13.AppendText("Formula:" + Form.MF_CONT_E);

        //        //加入table
        //        Spire.Doc.Table sTable1 = s.AddTable(true);
        //        var rowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.DI.GoodsList.Count) / 2));
        //        if (Form.IS_Concentrate_CHECK)
        //        {
        //            rowCount += Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.PC.GoodsList.Count) / 2)) + 2;
        //        }

        //        sTable1.ResetCells(rowCount, 8);
        //        //邊框隱藏
        //        Spire.Doc.Table table1 = (Spire.Doc.Table)s.Tables[1];
        //        table1.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //        table1.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //        table1.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //        table1.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //        table1.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //        table1.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;

        //        var halfCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.DI.GoodsList.Count) / 2));
        //        for (var i = 0; i < halfCount; i++)
        //        {
        //            if (i * 2 <= Form.DI.GoodsList.Count)
        //            {
        //                Spire.Doc.Fields.TextRange rangeN1 =
        //                    sTable1[i, 0].AddParagraph().AppendText(Form.DI.GoodsList[i].DI_NAME);
        //                rangeN1.CharacterFormat.FontName = "新細明體";
        //                rangeN1.CharacterFormat.FontSize = 8;

        //                Spire.Doc.Fields.TextRange rangeN2 =
        //                    sTable1[i, 1].AddParagraph().AppendText(Form.DI.GoodsList[i].DI_ENAME);
        //                rangeN2.CharacterFormat.FontName = "Times New Roman";
        //                rangeN2.CharacterFormat.FontSize = 8;

        //                var parN3 = sTable1[i, 2].AddParagraph();
        //                parN3.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                Spire.Doc.Fields.TextRange rangeN3 =
        //                    parN3.AppendText(Form.DI.GoodsList[i].DI_CONT);
        //                rangeN3.CharacterFormat.FontName = "Times New Roman";
        //                rangeN3.CharacterFormat.FontSize = 8;

        //                Spire.Doc.Fields.TextRange rangeN4 =
        //                    sTable1[i, 3].AddParagraph().AppendText(Form.DI.GoodsList[i].DI_UNIT);
        //                rangeN4.CharacterFormat.FontName = "Times New Roman";
        //                rangeN4.CharacterFormat.FontSize = 8;
        //                if ((i + 1) * 2 <= Form.DI.GoodsList.Count)
        //                {
        //                    Spire.Doc.Fields.TextRange rangeN5 =
        //                        sTable1[i, 4].AddParagraph().AppendText(Form.DI.GoodsList[halfCount + i].DI_NAME);
        //                    rangeN5.CharacterFormat.FontName = "新細明體";
        //                    rangeN5.CharacterFormat.FontSize = 8;

        //                    Spire.Doc.Fields.TextRange rangeN6 =
        //                        sTable1[i, 5].AddParagraph().AppendText(Form.DI.GoodsList[halfCount + i].DI_ENAME);
        //                    rangeN6.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN6.CharacterFormat.FontSize = 8;

        //                    var parN7 = sTable1[i, 6].AddParagraph();
        //                    parN7.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                    Spire.Doc.Fields.TextRange rangeN7 =
        //                        parN7.AppendText(Form.DI.GoodsList[halfCount + i].DI_CONT);
        //                    rangeN7.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN7.CharacterFormat.FontSize = 8;

        //                    Spire.Doc.Fields.TextRange rangeN8 =
        //                        sTable1[i, 7].AddParagraph().AppendText(Form.DI.GoodsList[halfCount + i].DI_UNIT);
        //                    rangeN8.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN8.CharacterFormat.FontSize = 8;
        //                }
        //            }
        //        }

        //        if (Form.IS_Concentrate_CHECK)
        //        {
        //            var rowCount1 = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.DI.GoodsList.Count) / 2));
        //            sTable1.ApplyHorizontalMerge(rowCount1, 0, 7);
        //            sTable1.ApplyHorizontalMerge(rowCount1 + 1, 0, 7);
        //            var rangeN9Text = "以上生藥製成浸膏" + Form.PC_SCALE_1 + Form.PC_SCALE_1E +
        //                              "(生藥與浸膏比例 " + Form.PC_SCALE_21 + " : " + Form.PC_SCALE_22 + " = " + Form.PC_SCALE_23 + " : " + Form.PC_SCALE_24 + ")";
        //            Spire.Doc.Fields.TextRange rangeN9 = sTable1[rowCount1, 0].AddParagraph().AppendText(rangeN9Text);
        //            rangeN9.CharacterFormat.FontName = "新細明體";
        //            rangeN9.CharacterFormat.FontSize = 8;
        //            rangeN9Text = "Above raw herbs equivalent to " + Form.PC_SCALE_1 + Form.PC_SCALE_2E +
        //                          " herbal extract.(Ratio of raw herbs and extract " + Form.PC_SCALE_21 + " : " + Form.PC_SCALE_22 + " = " + Form.PC_SCALE_23 + " : " + Form.PC_SCALE_24 + ")";
        //            Spire.Doc.Fields.TextRange rangeN10 = sTable1[rowCount1 + 1, 0].AddParagraph().AppendText(rangeN9Text);
        //            rangeN10.CharacterFormat.FontName = "Times New Roman";
        //            rangeN10.CharacterFormat.FontSize = 8;

        //            var j = 0;
        //            for (var i = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.DI.GoodsList.Count) / 2)) + 2; i < rowCount; i++)
        //            {
        //                halfCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.PC.GoodsList.Count) / 2));
        //                if (j * 2 <= Form.PC.GoodsList.Count)
        //                {
        //                    Spire.Doc.Fields.TextRange rangeN1 =
        //                        sTable1[i, 0].AddParagraph().AppendText(Form.PC.GoodsList[j].PC_NAME);
        //                    rangeN1.CharacterFormat.FontName = "新細明體";
        //                    rangeN1.CharacterFormat.FontSize = 8;

        //                    Spire.Doc.Fields.TextRange rangeN2 =
        //                        sTable1[i, 1].AddParagraph().AppendText(Form.PC.GoodsList[j].PC_ENAME);
        //                    rangeN2.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN2.CharacterFormat.FontSize = 8;

        //                    var parN3 = sTable1[i, 2].AddParagraph();
        //                    parN3.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                    Spire.Doc.Fields.TextRange rangeN3 =
        //                        parN3.AppendText(Form.PC.GoodsList[j].PC_CONT);
        //                    rangeN3.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN3.CharacterFormat.FontSize = 8;

        //                    Spire.Doc.Fields.TextRange rangeN4 =
        //                        sTable1[i, 3].AddParagraph().AppendText(Form.PC.GoodsList[j].PC_UNIT);
        //                    rangeN4.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN4.CharacterFormat.FontSize = 8;

        //                    if ((j + 1) * 2 <= Form.PC.GoodsList.Count)
        //                    {
        //                        Spire.Doc.Fields.TextRange rangeN5 =
        //                            sTable1[i, 4].AddParagraph().AppendText(Form.PC.GoodsList[halfCount + j].PC_NAME);
        //                        rangeN5.CharacterFormat.FontName = "新細明體";
        //                        rangeN5.CharacterFormat.FontSize = 8;

        //                        Spire.Doc.Fields.TextRange rangeN6 =
        //                            sTable1[i, 5].AddParagraph().AppendText(Form.PC.GoodsList[halfCount + j].PC_ENAME);
        //                        rangeN6.CharacterFormat.FontName = "Times New Roman";
        //                        rangeN6.CharacterFormat.FontSize = 8;

        //                        var parN7 = sTable1[i, 6].AddParagraph();
        //                        parN7.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                        Spire.Doc.Fields.TextRange rangeN7 =
        //                            parN7.AppendText(Form.PC.GoodsList[halfCount + j].PC_CONT);
        //                        rangeN7.CharacterFormat.FontName = "Times New Roman";
        //                        rangeN7.CharacterFormat.FontSize = 8;

        //                        Spire.Doc.Fields.TextRange rangeN8 =
        //                            sTable1[i, 7].AddParagraph().AppendText(Form.PC.GoodsList[halfCount + j].PC_UNIT);
        //                        rangeN8.CharacterFormat.FontName = "Times New Roman";
        //                        rangeN8.CharacterFormat.FontSize = 8;
        //                    }
        //                }

        //                j++;
        //            }
        //        }

        //        ParagraphStyle style1 = new ParagraphStyle(doc);
        //        style1.Name = "titleStyle";
        //        style1.CharacterFormat.FontName = "新細明體";
        //        style1.CharacterFormat.FontSize = 12;
        //        doc.Styles.Add(style1);
        //        para1.ApplyStyle("titleStyle");
        //        para1.Format.HorizontalAlignment = HorizontalAlignment.Center;

        //        ParagraphStyle style2 = new ParagraphStyle(doc);
        //        style2.Name = "titleStyle2";
        //        style2.CharacterFormat.FontName = "Times New Roman";
        //        style2.CharacterFormat.FontSize = 12;
        //        doc.Styles.Add(style2);
        //        para2.ApplyStyle("titleStyle2");
        //        para2.Format.HorizontalAlignment = HorizontalAlignment.Center;

        //        ParagraphStyle style3 = new ParagraphStyle(doc);
        //        style3.Name = "titleStyle3";
        //        style3.CharacterFormat.Bold = true;
        //        style3.CharacterFormat.FontName = "新細明體";
        //        style3.CharacterFormat.FontSize = 12;
        //        doc.Styles.Add(style3);
        //        para4.ApplyStyle("titleStyle3");
        //        para4.Format.HorizontalAlignment = HorizontalAlignment.Center;

        //        ParagraphStyle style4 = new ParagraphStyle(doc);
        //        style4.Name = "titleStyle4";
        //        style4.CharacterFormat.Bold = true;
        //        style4.CharacterFormat.FontName = "Times New Roman";
        //        style4.CharacterFormat.FontSize = 12;
        //        doc.Styles.Add(style4);
        //        para5.ApplyStyle("titleStyle4");
        //        para5.Format.HorizontalAlignment = HorizontalAlignment.Center;

        //        ParagraphStyle style5 = new ParagraphStyle(doc);
        //        style5.Name = "titleStyle5";
        //        style5.CharacterFormat.FontName = "新細明體";
        //        style5.CharacterFormat.FontSize = 8;
        //        doc.Styles.Add(style5);
        //        para6.ApplyStyle("titleStyle5");
        //        para7.ApplyStyle("titleStyle5");

        //        ParagraphStyle style6 = new ParagraphStyle(doc);
        //        style6.Name = "titleStyle6";
        //        style6.CharacterFormat.FontName = "Times New Roman";
        //        style6.CharacterFormat.FontSize = 8;
        //        doc.Styles.Add(style6);
        //        para12.ApplyStyle("titleStyle6");
        //        para13.ApplyStyle("titleStyle6");

        //        //適應症
        //        if (Form.IS_INDIOCATION_CHECK)
        //        {
        //            Paragraph para8 = s.AddParagraph();
        //            para8.AppendText("適應症：" + Form.INDIOCATION);
        //            para8.ApplyStyle("titleStyle5");

        //            Paragraph para9 = s.AddParagraph();
        //            para9.AppendText("Indication(s):" + Form.INDIOCATION_E);
        //            para9.ApplyStyle("titleStyle6");
        //        }

        //        //效能
        //        if (Form.IS_EFFICACY_CHECK)
        //        {
        //            Paragraph para10 = s.AddParagraph();
        //            para10.AppendText("效能：" + Form.EFFICACY);
        //            para10.ApplyStyle("titleStyle5");

        //            Paragraph para11 = s.AddParagraph();
        //            para11.AppendText("Efficacy:" + Form.EFFICACY_E);
        //            para11.ApplyStyle("titleStyle6");
        //        }

        //        s.AddParagraph().AppendText("\r\n");

        //        // 簽章區塊
        //        Spire.Doc.Fields.TextBox tb = s.AddParagraph().AppendTextBox(170, 140);
        //        tb.Format.HorizontalAlignment = ShapeHorizontalAlignment.Right;
        //        tb.Format.LineStyle = TextBoxLineStyle.Simple;
        //        tb.Format.NoLine = true;
        //        tb.Format.VerticalAlignment = ShapeVerticalAlignment.Bottom;
        //        tb.Format.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
        //        tb.Format.TextWrappingType = TextWrappingType.Right;

        //        Paragraph para14 = tb.Body.AddParagraph();
        //        Spire.Doc.Fields.TextRange tr = para14.AppendText("\r\n\r\n\r\nSigned by______________________________\r\n" +
        //                          "Yi-Tsau Huang, M.D., Ph.D.\r\n" +
        //                          "Director General\r\n" +
        //                          "Department of Chinese Medicine and Pharmacy\r\n" +
        //                          "on behalf of Shih - Chung Chen, D.D.S.\r\n" +
        //                          "Minister\r\n" +
        //                          "Ministry of Health and Welfare\r\n" +
        //                          "The Executive Yuan, R.O.C.");
        //        tr.CharacterFormat.FontSize = 8;
        //        para14.Format.HorizontalAlignment = HorizontalAlignment.Left;

        //        //Paragraph para14 = s.AddParagraph();
        //        //para14.AppendText("\r\n\r\n\r\n\r\nSigned by______________________________\r\n" +
        //        //                  "Yi-Tsau Huang, M.D., Ph.D.\r\n" +
        //        //                  "Director General\r\n" +
        //        //                  "Department of Chinese Medicine and Pharmacy\r\n" +
        //        //                  "on behalf of Shih - Chung Chen, D.D.S.\r\n" +
        //        //                  "Minister\r\n" +
        //        //                  "Ministry of Health and Welfare\r\n" +
        //        //                  "The Executive Yuan, R.O.C.");
        //        //para14.ApplyStyle("titleStyle6");


        //        Section sec = doc.Sections[0];
        //        sec.PageSetup.PageSize = PageSize.A4;

        //        sec.PageSetup.Margins.Top = 71.88f;
        //        sec.PageSetup.Margins.Bottom = 71.88f;
        //        sec.PageSetup.Margins.Left = 90.12f;
        //        sec.PageSetup.Margins.Right = 90.12f;

        //        #region Docx 套表
        //        //using (DocX doc = DocX.Load(path))
        //        //{
        //        //    //製造廠名稱(英文)
        //        //    doc.ReplaceText("$MF_CNT_NAME_E", Form.MF_CNT_NAME_E, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //製造廠名稱(中文)
        //        //    doc.ReplaceText("$MF_CNT_NAME", Form.MF_CNT_NAME, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //製造廠地址(英文)
        //        //    doc.ReplaceText("$MF_ADDR_E", Form.MF_ADDR_E, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //製造廠地址(中文)
        //        //    doc.ReplaceText("$MF_ADDR", "中華民國台灣" + Form.TAX_ORG_CITY_TEXT + Form.TAX_ORG_CITY_DETAIL, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //藥品名稱(英文)
        //        //    doc.ReplaceText("$DRUG_NAME_E", string.IsNullOrEmpty(Form.DRUG_NAME_E) ? "" : Form.DRUG_NAME_E, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //藥品名稱(中文)
        //        //    doc.ReplaceText("$DRUG_NAME", Form.DRUG_NAME, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //劑型(英文)
        //        //    doc.ReplaceText("$DOSAGE_FORM_E", Form.DOSAGE_FORM_E, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //劑型(中文)
        //        //    doc.ReplaceText("$DOSAGE_FORM", Form.DOSAGE_FORM, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    if (Form.IS_DRUG_ABROAD_CHECK)
        //        //    {
        //        //        //外銷品名(英文)
        //        //        doc.ReplaceText("$DRUG_ABROAD_NAME_E", Form.DRUG_ABROAD_NAME_E, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //        //外銷品名(中文)
        //        //        doc.ReplaceText("$DRUG_ABROAD_NAME", Form.DRUG_ABROAD_NAME, false, System.Text.RegularExpressions.RegexOptions.None);
        //        //    }
        //        //    else
        //        //    {
        //        //        //外銷品名(英文)
        //        //        doc.ReplaceText("$DRUG_ABROAD_NAME_E", "", false, System.Text.RegularExpressions.RegexOptions.None);

        //        //        //外銷品名(中文)
        //        //        doc.ReplaceText("$DRUG_ABROAD_NAME", "", false, System.Text.RegularExpressions.RegexOptions.None);
        //        //    }

        //        //    if (Form.IS_EXPIR_DATE_CHECK)
        //        //    {
        //        //        //有效日期(英文)
        //        //        doc.ReplaceText("$EXPIR_DATE_AD", Form.EXPIR_DATE, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //        //有效日期(中文)
        //        //        doc.ReplaceText("$EXPIR_DATE", Form.EXPIR_DATE, false, System.Text.RegularExpressions.RegexOptions.None);
        //        //    }
        //        //    else
        //        //    {
        //        //        //有效日期(英文)
        //        //        doc.ReplaceText("$EXPIR_DATE_AD", "", false, System.Text.RegularExpressions.RegexOptions.None);

        //        //        //有效日期(中文)
        //        //        doc.ReplaceText("$EXPIR_DATE", "", false, System.Text.RegularExpressions.RegexOptions.None);
        //        //    }

        //        //    //藥商許可執照字號(英文)
        //        //    doc.ReplaceText("$LIC_NUM_E", Form.PL_CD_E + "-" + Form.PL_Num_E, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //藥商許可執照字號(中文)
        //        //    doc.ReplaceText("$LIC_NUM", Form.PL_CD_TEXT + "第" + Form.PL_Num + "號", false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //核准日期(英文)
        //        //    doc.ReplaceText("$ISSUE_DATE_AD", Form.ISSUE_DATE, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //核准日期(中文)
        //        //    doc.ReplaceText("$ISSUE_DATE", Form.ISSUE_DATE, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //處方說明(英文)
        //        //    doc.ReplaceText("$MF_CONT_E", Form.MF_CONT_E, false, System.Text.RegularExpressions.RegexOptions.None);

        //        //    //處方說明(中文)
        //        //    doc.ReplaceText("$MF_CONT", Form.MF_CONT, false, System.Text.RegularExpressions.RegexOptions.None);


        //        //    doc.SaveAs(ms);
        //        //}
        //        #endregion

        //        //string path = Server.MapPath("~/Sample/apply005001.docx");
        //        //doc.SaveToFile(path, FileFormat.Docx2013);
        //        doc.SaveToStream(ms, FileFormat.Docx2013);
        //        buffer = ms.ToArray();
        //    }

        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    Response.ContentType = "Application/msword";
        //    Response.AddHeader("Content-Disposition", "attachment;   filename=產銷證明書.docx");
        //    Response.BinaryWrite(buffer);
        //    Response.OutputStream.Flush();
        //    Response.OutputStream.Close();
        //    Response.Flush();
        //    Response.End();
        //}

        #endregion

        #region 新版
        /// <summary>
        /// 005003_申請單套表
        /// </summary>
        /// <param name="form"></param>
        [HttpPost]
        [DisplayName("005001_申請單套表")]
        public void PreviewApplyForm(Apply_005001FormModel Form)
        {
            ShareCodeListModel sc = new ShareCodeListModel();
            Apply_005001FormModel fm = Form;
            string path = "";

            #region 判斷勾選的值，決定用哪個套表

            // 外銷品名
            if (Form.IS_DRUG_ABROAD_CHECK)
            {
                // 有效日期
                if (Form.IS_EXPIR_DATE_CHECK)
                {
                    // 效能 適應症 濃縮劑
                    if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_0.docx");
                    }
                    // 效能 適應症
                    else if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_3.docx");
                    }
                    // 效能 
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_4.docx");
                    }
                    // 適應症
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_5.docx");
                    }
                    // 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_6.docx");
                    }
                    // 
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_6.docx");
                    }
                }
                else
                {
                    // 效能 適應症 濃縮劑
                    if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_0.docx");
                    }
                    // 效能 適應症
                    else if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_3.docx");
                    }
                    // 效能 
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_4.docx");
                    }
                    // 適應症
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_5.docx");
                    }
                    // 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_6.docx");
                    }
                    // 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_6.docx");
                    }
                }
            }
            else
            {
                // 有效日期
                if (Form.IS_EXPIR_DATE_CHECK)
                {
                    // 效能 適應症 濃縮劑
                    if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_0.docx");
                    }
                    // 效能 適應症
                    else if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_3.docx");
                    }
                    // 效能 
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_4.docx");
                    }
                    // 適應症
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_5.docx");
                    }
                    // 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_6.docx");
                    }
                    // 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_6.docx");
                    }
                }
                else
                {
                    // 效能 適應症 濃縮劑
                    if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_0.docx");
                    }
                    // 效能 適應症
                    else if (Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_3.docx");
                    }
                    // 效能 
                    else if (Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_4.docx");
                    }
                    // 適應症
                    else if (!Form.IS_EFFICACY_CHECK && Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_5.docx");
                    }
                    // 濃縮劑
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_6.docx");
                    }
                    // 
                    else if (!Form.IS_EFFICACY_CHECK && !Form.IS_INDIOCATION_CHECK && !Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_6.docx");
                    }
                }
            }

            #endregion

            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    doc.ReplaceText("[$MF_CNT_NAME]", Form.MF_CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_CNT_NAME_E]", Form.MF_CNT_NAME_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$TAX_ORG_CITY]", "中華民國台灣" + Form.TAX_ORG_CITY_TEXT + Form.TAX_ORG_CITY_DETAIL, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_ADDR_E]", Form.MF_ADDR_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_NAME]", Form.DRUG_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_NAME_E]", Form.DRUG_NAME_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DOSAGE_FORM]", Form.DOSAGE_FORM.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DOSAGE_FORM_E]", Form.DOSAGE_FORM_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_ABROAD_NAME]", Form.DRUG_ABROAD_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_ABROAD_NAME_E]", Form.DRUG_ABROAD_NAME_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    if (Form.IS_EXPIR_DATE_CHECK)
                    {
                        var EdateTW1 = Form.EXPIR_DATE_TW.Split('/');
                        var EdateTW1Ins = EdateTW1[0] + "年" + EdateTW1[1] + "月" + EdateTW1[2] + "日";
                        doc.ReplaceText("[$EXPIR_DATE_TW]", EdateTW1Ins.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        var EissIns = Convert.ToDateTime(Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                        doc.ReplaceText("[$EXPIR_DATE]", EissIns.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    doc.ReplaceText("[$PL_Num]", Form.PL_CD_TEXT.TONotNullString() + "第" + Form.PL_Num + "號".TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$PL_Num_E]", Form.PL_CD_E.TONotNullString() + "-" + Form.PL_Num_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    var dateTW1 = Form.ISSUE_DATE_TW.Split('/');
                    var dateTW1Ins = dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日";
                    doc.ReplaceText("[$ISSUE_DATE_TW]", dateTW1Ins.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    var issIns = Convert.ToDateTime(Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                    doc.ReplaceText("[$ISSUE_DATE]", issIns, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_CONT]", Form.MF_CONT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_CONT_E]", Form.MF_CONT_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$INDIOCATION]", Form.INDIOCATION.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$INDIOCATION_E]", Form.INDIOCATION_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$EFFICACY]", Form.EFFICACY.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$EFFICACY_E]", Form.EFFICACY_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //doc.ReplaceText("[$ADDTABLE]", Form.MF_CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);


                    Border border = new Border();
                    var rowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.DI.GoodsList.Count) / 2));
                    var tb = doc.AddTable(rowCount, 8);
                    tb.IndentFromLeft = 28.57f;
                    if (Form.IS_Concentrate_CHECK)
                    {
                        var rowCount1 = rowCount + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.PC.GoodsList.Count) / 2)) + 2;
                        tb = doc.AddTable(rowCount1, 8);
                    }

                    tb.Alignment = Xceed.Document.NET.Alignment.left;
                    tb.Design = TableDesign.None;
                    var ceiling = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.DI.GoodsList.Count) / 2));
                    for (var i = 0; i < ceiling; i++)
                    {
                        if (i <= Form.DI.GoodsList.Count)
                        {
                            tb.Rows[i].Cells[0].Width = 100;
                            tb.Rows[i].Cells[1].Width = 200;
                            tb.Rows[i].Cells[2].Width = 50;
                            tb.Rows[i].Cells[3].Width = 50;
                            tb.Rows[i].Cells[0].Paragraphs[0].Append(Form.DI.GoodsList[i].DI_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                            tb.Rows[i].Cells[0].Paragraphs[0].IndentationBefore = 26.5f;
                            tb.Rows[i].Cells[1].Paragraphs[0].Append(Form.DI.GoodsList[i].DI_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                            tb.Rows[i].Cells[2].Paragraphs[0].Append(Form.DI.GoodsList[i].DI_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                            tb.Rows[i].Cells[3].Paragraphs[0].Append(Form.DI.GoodsList[i].DI_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;

                            if ((i + ceiling) < Form.DI.GoodsList.Count)
                            {
                                tb.Rows[i].Cells[4].Width = 100;
                                tb.Rows[i].Cells[5].Width = 200;
                                tb.Rows[i].Cells[6].Width = 50;
                                tb.Rows[i].Cells[7].Width = 50;
                                tb.Rows[i].Cells[4].Paragraphs[0].Append(Form.DI.GoodsList[(i + ceiling)].DI_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                                tb.Rows[i].Cells[5].Paragraphs[0].Append(Form.DI.GoodsList[(i + ceiling)].DI_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                                tb.Rows[i].Cells[6].Paragraphs[0].Append(Form.DI.GoodsList[(i + ceiling)].DI_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                                tb.Rows[i].Cells[7].Paragraphs[0].Append(Form.DI.GoodsList[(i + ceiling)].DI_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                            }
                        }
                    }


                    if (Form.IS_Concentrate_CHECK)
                    {
                        var cnt = rowCount;
                        //rowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.PC.GoodsList.Count) / 2)) + 2;
                        //var tb1 = doc.AddTable(rowCount, 8);
                        //tb1.Alignment = Xceed.Document.NET.Alignment.center;
                        //tb1.Design = TableDesign.None;
                        tb.Rows[cnt].MergeCells(0, 7);
                        tb.Rows[cnt].Paragraphs[0].IndentationBefore = 26.5f;
                        tb.Rows[cnt + 1].MergeCells(0, 7);
                        tb.Rows[cnt + 1].Paragraphs[0].IndentationBefore = 26.5f;
                        tb.Rows[cnt].Cells[0].Paragraphs[0].Append("以上生藥製成浸膏" + Form.PC_SCALE_1 + Form.PC_SCALE_1E +
                           "(生藥與浸膏比例 " + Form.PC_SCALE_21 + ":" + Form.PC_SCALE_22 + "=" + Form.PC_SCALE_23 + ":" + Form.PC_SCALE_24 + ")").FontSize(8).Font("新細明體").Alignment = Alignment.left;
                        tb.Rows[cnt + 1].Cells[0].Paragraphs[0].Append("Above raw herbs equivalent to " + Form.PC_SCALE_1 + Form.PC_SCALE_1E +
                            " herbal extract. (Ratio of raw herbs and extract " + Form.PC_SCALE_21 + ":" + Form.PC_SCALE_22 + "=" + Form.PC_SCALE_23 + ":" + Form.PC_SCALE_24 + ")").FontSize(8).Font("新細明體").Alignment = Alignment.left;
                        var ceiling_pc = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Form.PC.GoodsList.Count) / 2));
                        for (var i = 0; i < ceiling_pc; i++)
                        {
                            if (i <= Form.PC.GoodsList.Count)
                            {
                                tb.Rows[cnt + 2].Cells[0].Width = 100;
                                tb.Rows[cnt + 2].Cells[1].Width = 200;
                                tb.Rows[cnt + 2].Cells[2].Width = 50;
                                tb.Rows[cnt + 2].Cells[3].Width = 50;
                                tb.Rows[cnt + 2].Cells[0].Paragraphs[0].Append(Form.PC.GoodsList[i].PC_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                                tb.Rows[cnt + 2].Cells[0].Paragraphs[0].IndentationBefore = 26.5f;
                                tb.Rows[cnt + 2].Cells[1].Paragraphs[0].Append(Form.PC.GoodsList[i].PC_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                                tb.Rows[cnt + 2].Cells[2].Paragraphs[0].Append(Form.PC.GoodsList[i].PC_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                                tb.Rows[cnt + 2].Cells[3].Paragraphs[0].Append(Form.PC.GoodsList[i].PC_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;

                                if ((i + ceiling_pc) < Form.PC.GoodsList.Count)
                                {
                                    tb.Rows[cnt + 2].Cells[4].Width = 100;
                                    tb.Rows[cnt + 2].Cells[5].Width = 200;
                                    tb.Rows[cnt + 2].Cells[6].Width = 50;
                                    tb.Rows[cnt + 2].Cells[7].Width = 50;
                                    tb.Rows[cnt + 2].Cells[4].Paragraphs[0].Append(Form.PC.GoodsList[(i + ceiling_pc)].PC_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                                    tb.Rows[cnt + 2].Cells[5].Paragraphs[0].Append(Form.PC.GoodsList[(i + ceiling_pc)].PC_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                                    tb.Rows[cnt + 2].Cells[6].Paragraphs[0].Append(Form.PC.GoodsList[(i + ceiling_pc)].PC_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                                    tb.Rows[cnt + 2].Cells[7].Paragraphs[0].Append(Form.PC.GoodsList[(i + ceiling_pc)].PC_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                                }
                                cnt++;
                            }
                        }
                        //doc.ReplaceTextWithObject("[$ADDTABLE1]", tb1, false, System.Text.RegularExpressions.RegexOptions.None);
                    }

                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);


                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            string attach_1 = string.Format(@"attachment;  filename={0}.doc", "產銷證明書");
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
        #endregion

        #endregion

        [DisplayName("005001_補件進入")]
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005001ViewModel model = new Apply_005001ViewModel(APP_ID);

            model.FormBack = dao.QueryApply_005001(APP_ID);
            model.Detail = dao.GetApplyNotice_005001(APP_ID);

            if (model.FormBack.CODE_CD == "2" || model.FormBack.CODE_CD == "4")
            {
                if (shareDao.CalculationDocDate("005001", APP_ID))
                {
                    //已過補件期限
                    model.DI.IsReadOnly = true;
                    model.PC.IsReadOnly = true;

                    return View("Detail005001", model);
                }
                else
                {
                    if (model.Detail.INGREDIENT_CONTENT == null)
                    {
                        model.Detail.INGREDIENT_CONTENT = string.Empty;
                        model.DI.IsReadOnly = true;
                    }
                    else
                    {
                        model.DI.IsDeleteOpen = true;
                        model.DI.IsNewOpen = true;
                        model.DI.IsReadOnly = false;
                    }

                    if (model.Detail.EXCIPIENT == null)
                    {
                        model.Detail.EXCIPIENT = string.Empty;
                        model.PC.IsReadOnly = true;
                    }
                    else
                    {
                        model.PC.IsDeleteOpen = true;
                        model.PC.IsNewOpen = true;
                        model.PC.IsReadOnly = false;
                    }

                    return View("AppDoc", model);
                }
            }
            else
            {
                model.DI.IsReadOnly = true;
                model.PC.IsReadOnly = true;

                return View("Detail005001", model);
            }
        }

        [HttpPost]
        [DisplayName("005001_補件驗證欄位")]
        public ActionResult DocSave(Apply_005001ViewModel model)
        {
            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\""\'\:\：\“\”]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            System.Text.RegularExpressions.Regex reg4 = new System.Text.RegularExpressions.Regex(@"^[\u4e00-\u9fa5]+$");
            System.Text.RegularExpressions.Regex reg5 = new System.Text.RegularExpressions.Regex(@"^[0-9\-\#]+$");
            string ErrorMsg = "";

            //個別欄位驗證
            if (!string.IsNullOrEmpty(model.FormBack.PL_Num))
            {
                if (!reg2.IsMatch(model.FormBack.PL_Num))
                {
                    ErrorMsg += "藥商許可執照字號(中文)請以數字填寫。\r\n";
                }
            }
            else
            {
                ErrorMsg += "藥商許可執照字號(中文) 為必填欄位。\r\n";
            }

            if (!string.IsNullOrEmpty(model.FormBack.PL_Num_E))
            {
                if (!reg2.IsMatch(model.FormBack.PL_Num_E))
                {
                    ErrorMsg += "藥商許可執照字號(英文)請以數字填寫。\r\n";
                }
            }
            else
            {
                ErrorMsg += "藥商許可執照字號(英文) 為必填欄位。\r\n";
            }

            if (string.IsNullOrEmpty(model.FormBack.MF_CNT_NAME))
            {
                ErrorMsg += "製造廠名稱(中文) 為必填欄位。\r\n";
            }

            if (!string.IsNullOrEmpty(model.FormBack.MF_CNT_NAME_E))
            {
                if (!reg.IsMatch(model.FormBack.MF_CNT_NAME_E))
                {
                    ErrorMsg += "製造廠名稱(英文)請以英文填寫。\r\n";
                }
            }
            else
            {
                ErrorMsg += "製造廠名稱(英文) 為必填欄位。\r\n";
            }

            if (string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_CODE) ||
                string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_DETAIL) ||
                string.IsNullOrEmpty(model.FormBack.TAX_ORG_CITY_TEXT))
            {
                ErrorMsg += "製造廠地址(中文) 為必填欄位。\r\n";
            }

            if (!string.IsNullOrEmpty(model.FormBack.MF_ADDR_E))
            {
                if (!reg.IsMatch(model.FormBack.MF_ADDR_E))
                {
                    ErrorMsg += "製造廠地址(英文)請以英文填寫。\r\n";
                }
            }
            else
            {
                ErrorMsg += "製造廠地址(英文) 為必填欄位。\r\n";
            }

            if (string.IsNullOrEmpty(model.FormBack.DRUG_NAME))
            {
                ErrorMsg += "藥品名稱(中文) 為必填欄位。\r\n";
            }

            if (!string.IsNullOrEmpty(model.FormBack.DRUG_NAME_E))
            {
                if (!reg.IsMatch(model.FormBack.DRUG_NAME_E))
                {
                    ErrorMsg += "藥品名稱(英文)請以英文填寫。\r\n";
                }
            }

            if (model.FormBack.IS_DRUG_ABROAD_CHECK)
            {
                if (!string.IsNullOrEmpty(model.FormBack.DRUG_ABROAD_NAME_E))
                {
                    if (!reg.IsMatch(model.FormBack.DRUG_ABROAD_NAME_E))
                    {
                        ErrorMsg += "外銷品名(英文)請以英文填寫。\r\n";
                    }
                }

                if (string.IsNullOrEmpty(model.FormBack.DRUG_ABROAD_NAME_E) && string.IsNullOrEmpty(model.FormBack.DRUG_ABROAD_NAME))
                {
                    ErrorMsg += "外銷名品(中文/英文)欄位需擇一填入。\r\n";
                }
            }

            if (string.IsNullOrEmpty(model.FormBack.DOSAGE_FORM))
            {
                ErrorMsg += "劑型(中文) 為必填欄位。\r\n";
            }

            if (!string.IsNullOrEmpty(model.FormBack.DOSAGE_FORM_E))
            {
                if (!reg.IsMatch(model.FormBack.DOSAGE_FORM_E))
                {
                    ErrorMsg += "劑型(英文)請以英文填寫。\r\n";
                }
            }
            else
            {
                ErrorMsg += "劑型(英文) 為必填欄位。\r\n";
            }

            if (string.IsNullOrEmpty(model.FormBack.ISSUE_DATE))
            {
                ErrorMsg += "核准日期(中文) 為必填欄位。\r\n";
            }

            if (model.FormBack.IS_EXPIR_DATE_CHECK)
            {
                if (string.IsNullOrEmpty(model.FormBack.EXPIR_DATE))
                {
                    ErrorMsg += "有效日期(中文) 為必填欄位。\r\n";
                }
            }

            if (!string.IsNullOrEmpty(model.FormBack.MF_CONT))
            {
                if (model.FormBack.MF_CONT.Trim().Substring(model.FormBack.MF_CONT.Trim().Length - 1, 1) != "：")
                {
                    ErrorMsg += "處方說明(中文)最後請以「：」結尾。\r\n";
                }
            }
            else
            {
                ErrorMsg += "處方說明(中文) 為必填欄位。\r\n";
            }
            if (!string.IsNullOrEmpty(model.FormBack.MF_CONT_E))
            {
                if (model.FormBack.MF_CONT_E.Trim().Substring(model.FormBack.MF_CONT_E.Trim().Length - 1, 1) != ":" && model.FormBack.MF_CONT_E.Trim().Substring(model.FormBack.MF_CONT_E.Trim().Length - 1, 1) != "：")
                {
                    ErrorMsg += "處方說明(英文)最後請以「:」結尾。\r\n";
                }

                if (!reg.IsMatch(model.FormBack.MF_CONT_E))
                {
                    ErrorMsg += "處方說明(英文)請以英文填寫。\r\n";
                }
            }
            else
            {
                ErrorMsg += "處方說明(英文) 為必填欄位。\r\n";
            }

            if (model.FormBack.IS_Concentrate_CHECK)
            {
                if (string.IsNullOrEmpty(model.FormBack.PC_SCALE_1) || string.IsNullOrEmpty(model.FormBack.PC_SCALE_1E) ||
                    string.IsNullOrEmpty(model.FormBack.PC_SCALE_21) || string.IsNullOrEmpty(model.FormBack.PC_SCALE_22) ||
                    string.IsNullOrEmpty(model.FormBack.PC_SCALE_23) || string.IsNullOrEmpty(model.FormBack.PC_SCALE_24))
                {
                    ErrorMsg += "請輸入生藥與浸膏比例中文英文完整資訊。\r\n";
                }
                else
                {
                    if (!(reg1.IsMatch(model.FormBack.PC_SCALE_1) && reg1.IsMatch(model.FormBack.PC_SCALE_21) &&
                          reg1.IsMatch(model.FormBack.PC_SCALE_22) && reg1.IsMatch(model.FormBack.PC_SCALE_23) &&
                          reg1.IsMatch(model.FormBack.PC_SCALE_24)))
                    {
                        ErrorMsg += "請輸入生藥與浸膏比例中文英文請填寫數字。\r\n";
                    }
                }

                if (model.PC.GoodsList.Count == 1 && (string.IsNullOrEmpty(model.PC.GoodsList[0].PC_NAME)) || string.IsNullOrEmpty(model.PC.GoodsList[0].PC_ENAME))
                {
                    ErrorMsg += "請輸入賦形劑完整資訊。\r\n";
                }
            }

            if (model.FormBack.IS_INDIOCATION_CHECK)
            {
                //適應症(中文)
                if (!string.IsNullOrEmpty(model.FormBack.INDIOCATION))
                {
                    if (model.FormBack.INDIOCATION.Trim().Substring(model.FormBack.INDIOCATION.Trim().Length - 1, 1) != "。")
                    {
                        ErrorMsg += "適應症(中文)最後請以「。」結尾。\r\n";
                    }
                }
                else
                {
                    ErrorMsg += "勾選適應症時請填寫適應症(中文)資訊。\r\n";
                }

                //適應症(英文)
                if (!string.IsNullOrEmpty(model.FormBack.INDIOCATION_E))
                {
                    if (model.FormBack.INDIOCATION_E.Trim().Substring(model.FormBack.INDIOCATION_E.Trim().Length - 1, 1) != ".")
                    {
                        ErrorMsg += "適應症(英文)最後請以「.」結尾。\r\n";
                    }

                    if (!reg.IsMatch(model.FormBack.INDIOCATION_E))
                    {
                        ErrorMsg += "適應症(英文)請以英文填寫。\r\n";
                    }
                }
                else
                {
                    ErrorMsg += "勾選適應症時請填寫適應症(英文)資訊。\r\n";
                }
            }

            if (model.FormBack.IS_EFFICACY_CHECK)
            {
                //效能(中文)
                if (!string.IsNullOrEmpty(model.FormBack.EFFICACY))
                {
                    if (model.FormBack.EFFICACY.Trim().Substring(model.FormBack.EFFICACY.Trim().Length - 1, 1) != "。")
                    {
                        ErrorMsg += "效能(中文)最後請以「。」結尾。\r\n";
                    }
                }
                else
                {
                    ErrorMsg += "勾選效能時請填寫效能(中文)資訊。\r\n";
                }

                //效能(英文)
                if (!string.IsNullOrEmpty(model.FormBack.EFFICACY_E))
                {
                    if (model.FormBack.EFFICACY_E.Trim().Substring(model.FormBack.EFFICACY_E.Trim().Length - 1, 1) != ".")
                    {
                        ErrorMsg += "效能(英文)最後請以「.」結尾。\r\n";
                    }

                    if (!reg.IsMatch(model.FormBack.EFFICACY_E))
                    {
                        ErrorMsg += "效能(英文)請以英文填寫。\r\n";
                    }
                }
                else
                {
                    ErrorMsg += "勾選效能時請填寫效能(英文)資訊。\r\n";
                }
            }

            if (!string.IsNullOrEmpty(model.FormBack.FAX_BEFORE) || !string.IsNullOrEmpty(model.FormBack.FAX_AFTER) || !string.IsNullOrEmpty(model.FormBack.FAX_Extension))
            {
                if (!string.IsNullOrEmpty(model.FormBack.FAX_AFTER) && string.IsNullOrEmpty(model.FormBack.FAX_BEFORE))
                {
                    ErrorMsg += "請輸入傳真區碼。\r\n";
                }
                else
                {
                    if (!reg2.IsMatch(model.FormBack.FAX_BEFORE))
                    {
                        ErrorMsg += "請輸入正確傳真區碼格式。\r\n";
                    }
                    if (!reg2.IsMatch(model.FormBack.FAX_AFTER))
                    {
                        ErrorMsg += "請輸入正確傳真格式。\r\n";
                    }
                    if (!string.IsNullOrEmpty(model.FormBack.FAX_Extension))
                    {
                        if (!reg2.IsMatch(model.FormBack.FAX_Extension))
                        {
                            ErrorMsg += "請輸入正確傳真分機格式。\r\n";
                        }
                    }
                }

            }

            if (!string.IsNullOrEmpty(model.FormBack.TEL))
            {
                if (!reg5.IsMatch(model.FormBack.TEL))
                {
                    ErrorMsg += "請輸入正確電話格式。\r\n";
                }
            }

            if (model.FormBack.RADIOYN == "Y")
            {
                if (model.FormBack.File_1 == null && model.FormBack.File_2 == null && model.FormBack.File_3 == null)
                {
                    ErrorMsg += "請至少上傳一筆檔案!\r\n";
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
        [DisplayName("005001_補件存檔")]
        public ActionResult DocFinish(Apply_005001ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string ErrorMsg = "";


            ErrorMsg = dao.AppendApplyDoc005001(model);
            if (model.FormBack.CODE_CD == "4")
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, "產銷證明書", "005001", "1", ISBACK: true);
            }
            else
            {
                dao.SendMail_Update(model.FormBack.CNT_NAME, model.FormBack.EMAIL, model.FormBack.APP_ID, "產銷證明書", "005001", "1");
            }

            Apply_005001FormModel Form = new Apply_005001FormModel();
            Form.DOCYN = "Y";

            return View("Save", Form);
        }

        [HttpPost]
        public ActionResult getGMPData(Apply_005001FormModel Form)
        {
            var Data = new SALEAPI();
            var result = new AjaxResultStruct();
            ApplyDAO dao = new ApplyDAO();

            if (string.IsNullOrEmpty(Form.PL_Num))
            {
                result.status = false;
                result.message = "請輸入藥品許可執照字號";
            }
            else
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(Form.PL_Num, @"^[0-9]+$"))
                {
                    result.status = false;
                    result.message = "藥品許可執照字號(中文)請以數字填寫。\r\n";
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
                                       "\"外銷中文品名\": \"外銷品名(中文)1\"," +
                                       "\"外銷英文品名\": \"\"" +
                                       "}," +
                                       "{" +
                                       "\"外銷中文品名\": \"外銷品名(中文)2\"," +
                                       "\"外銷英文品名\": \"\"" +
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
                            Data = api.GetSALELicense(Form.PL_CD, Form.PL_Num);
                        }
                        if (Data.STATUS == "成功")
                        {
                            //資料處理
                            Data.SALEResp = dao.SortData005001(Data.SALEResp);

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
                        logger.Error("005001_GetInterfaceData failed:" + ex.TONotNullString());
                        result.status = false;
                        result.message = "取得資料失敗";
                    }
                }
            }

            return Content(result.Serialize(), "application/json");
        }
    }
}
