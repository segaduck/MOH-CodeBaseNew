using ES.Areas.BACKMIN.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ICSharpCode.SharpZipLib.Zip;
using Spire.Doc;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_005001Controller : BaseController
    {
        [DisplayName("005001_案件審理")]
        public ActionResult Index(string appid, string srvid)
        {
            BackApplyDAO dao = new BackApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            Apply_005001ViewModel form = new Apply_005001ViewModel(appid);

            form.Form = dao.QueryApply_005001(appid);

            //地址拆分
            form.Form.TAX_ORG_CITY_CODE = form.Form.MF_ADDR.Substring(0, 5);
            form.Form.TAX_ORG_CITY_DETAIL = form.Form.ADDR;
            TblZIPCODE zip = new TblZIPCODE();
            zip.ZIP_CO = form.Form.MF_ADDR.Substring(0, 5);
            var getnam = dao.GetRow(zip);
            if (getnam != null)
            {
                form.Form.TAX_ORG_CITY_DETAIL = form.Form.MF_ADDR.TONotNullString().Substring(5).Replace(getnam.CITYNM + getnam.TOWNNM, "");
                form.Form.TAX_ORG_CITY_TEXT = getnam.CITYNM + getnam.TOWNNM;
            }


            if (form.Form.CODE_CD == "2")
            {
                form.Detail = dao.GetApplyNotice_005001(appid);
                form.DI.IsReadOnly = true;
                form.PC.IsReadOnly = true;
            }

            ////案件是否過期
            form.Form.IS_CASE_LOCK = shareDao.CalculationDocDate("005001", appid);


            return View(form);
        }

        [DisplayName("005001_下載壓縮檔")]
        [HttpPost]
        public void GetZipFile(string app_id)
        {
            #region 另存檔案至目錄     
            FileStreamResult file;

            // 判斷是否有資料夾
            if (!Directory.Exists(Server.MapPath(@"../../Template/" + app_id + "_ZIP")))
            {
                Directory.CreateDirectory(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
            }

            BackApplyDAO dao = new BackApplyDAO();
            ShareDAO sharedao = new ShareDAO();

            // 取檔案後排序
            Apply_FileModel fm = new Apply_FileModel();
            fm.APP_ID = app_id;
            var fmlst = dao.GetRowList(fm);
            var newfmlst = from a in fmlst
                           orderby a.ADD_TIME, a.FILE_NO descending
                           select a;

            // 紀錄FILE_NO 已避免重複
            var i = 0;
            foreach (var item in newfmlst)
            {
                if (i != item.FILE_NO.TOInt32())
                {
                    i = item.FILE_NO.TOInt32();
                    var FilePath = Server.MapPath(@"../../Template/" + app_id + "_ZIP") + "/" + item.SRC_FILENAME;
                    var dbyte = sharedao.sftpDownload(item.FILENAME);
                    System.IO.File.WriteAllBytes(FilePath, dbyte);
                }
            }
            #endregion

            string[] filenames = Directory.GetFiles(Server.MapPath(@"../../Template/" + app_id + "_ZIP/"));
            byte[] buffer = new byte[4096];

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(@"../../Template/" + app_id + "_ZIP/中藥GMP廠證明文件(英文).ZIP"))))
            {
                // 設定壓縮比
                zp.SetLevel(0);

                // 逐一將資料夾內的檔案抓出來壓縮，並寫入至目的檔(.ZIP)
                foreach (string filename in filenames)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(filename));
                    zp.PutNextEntry(entry);
                }

            }

            Response.ContentType = "application/zip";
            Response.Headers["Content-Disposition"] = "attachment; filename=中藥GMP廠證明文件(英文).ZIP";
            Response.TransmitFile(Server.MapPath(@"../../Template/" + app_id + "_ZIP/中藥GMP廠證明文件(英文).ZIP"));
            Response.Flush();
            Response.Close();
            // 刪除資料夾
            if (Directory.Exists(Server.MapPath(@"../../Template/" + app_id + "_ZIP")))
            {
                string[] files = Directory.GetFiles(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
                string[] dirs = Directory.GetDirectories(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
                foreach (string item in files)
                {
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }

                Directory.Delete(Server.MapPath(@"../../Template/" + app_id + "_ZIP"));
            }
        }

        [DisplayName("005001_案件儲存")]
        [HttpPost]
        public ActionResult Save(Apply_005001ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if ((model.Form.CODE_CD == "4" || model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20") && string.IsNullOrEmpty(model.Form.MOHW_CASE_NO))
                {
                    ErrorMsg += "請取得公文文號。";
                    if (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE) || !model.Form.MOHW_CASE_DATE.Contains("/"))
                    {
                        ErrorMsg += "請輸入公文日期。";
                    }
                }
                else if (!string.IsNullOrEmpty(model.Form.MOHW_CASE_NO) && (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE) || !model.Form.MOHW_CASE_DATE.Contains("/")))
                {
                    ErrorMsg += "請輸入公文日期。";
                }

                if (ErrorMsg == "")
                {
                    model.Form.ADDR = model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL;
                    if (model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20")
                    {
                        //一般存檔
                        ErrorMsg += dao.AppendApply005001(model);
                    }
                    else
                    {
                        System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\'\""\:\：_]+$");
                        System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
                        System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
                        System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
                        System.Text.RegularExpressions.Regex reg4 = new System.Text.RegularExpressions.Regex(@"^[\u4e00-\u9fa5]+$");
                        System.Text.RegularExpressions.Regex reg5 = new System.Text.RegularExpressions.Regex(@"^[0-9\-\#]+$");

                        //個別欄位驗證
                        if (!string.IsNullOrEmpty(model.Form.PL_Num))
                        {
                            if (!reg2.IsMatch(model.Form.PL_Num))
                            {
                                ErrorMsg += "藥商許可執照字號(中文)請以數字填寫。\r\n";
                            }
                        }
                        //else
                        //{
                        //    ErrorMsg += "藥商許可執照字號(中文) 為必填欄位。\r\n";
                        //}

                        if (!string.IsNullOrEmpty(model.Form.PL_Num_E))
                        {
                            if (!reg2.IsMatch(model.Form.PL_Num_E))
                            {
                                ErrorMsg += "藥商許可執照字號(英文)請以數字填寫。\r\n";
                            }
                        }
                        //else
                        //{
                        //    ErrorMsg += "藥商許可執照字號(英文) 為必填欄位。\r\n";
                        //}

                        //if (string.IsNullOrEmpty(model.Form.MF_CNT_NAME))
                        //{
                        //    ErrorMsg += "製造廠名稱(中文) 為必填欄位。\r\n";
                        //}

                        if (!string.IsNullOrEmpty(model.Form.MF_CNT_NAME_E))
                        {
                            if (!reg.IsMatch(model.Form.MF_CNT_NAME_E))
                            {
                                ErrorMsg += "製造廠名稱(英文)請以英文填寫。\r\n";
                            }
                        }
                        //else
                        //{
                        //    ErrorMsg += "製造廠名稱(英文) 為必填欄位。\r\n";
                        //}

                        //if (string.IsNullOrEmpty(model.Form.TAX_ORG_CITY_CODE) ||
                        //    string.IsNullOrEmpty(model.Form.TAX_ORG_CITY_DETAIL) ||
                        //    string.IsNullOrEmpty(model.Form.TAX_ORG_CITY_TEXT))
                        //{
                        //    ErrorMsg += "製造廠地址(中文) 為必填欄位。\r\n";
                        //}

                        if (!string.IsNullOrEmpty(model.Form.MF_ADDR_E))
                        {
                            if (!reg.IsMatch(model.Form.MF_ADDR_E))
                            {
                                ErrorMsg += "製造廠地址(英文)請以英文填寫。\r\n";
                            }
                        }
                        //else
                        //{
                        //    ErrorMsg += "製造廠地址(英文) 為必填欄位。\r\n";
                        //}

                        //if (string.IsNullOrEmpty(model.Form.DRUG_NAME))
                        //{
                        //    ErrorMsg += "藥品名稱(中文) 為必填欄位。\r\n";
                        //}

                        if (!string.IsNullOrEmpty(model.Form.DRUG_NAME_E))
                        {
                            if (!reg.IsMatch(model.Form.DRUG_NAME_E))
                            {
                                ErrorMsg += "藥品名稱(英文)請以英文填寫。\r\n";
                            }
                        }

                        if (model.Form.IS_DRUG_ABROAD_CHECK)
                        {
                            if (!string.IsNullOrEmpty(model.Form.DRUG_ABROAD_NAME_E))
                            {
                                if (!reg.IsMatch(model.Form.DRUG_ABROAD_NAME_E))
                                {
                                    ErrorMsg += "外銷品名(英文)請以英文填寫。\r\n";
                                }
                            }

                            //if (string.IsNullOrEmpty(model.Form.DRUG_ABROAD_NAME_E))
                            //{
                            //    ErrorMsg += "外銷品名(英文) 為必填欄位。\r\n";
                            //}

                            //if (string.IsNullOrEmpty(model.Form.DRUG_ABROAD_NAME))
                            //{
                            //    ErrorMsg += "外銷品名(中文) 為必填欄位。\r\n";
                            //}
                        }

                        //if (string.IsNullOrEmpty(model.Form.DOSAGE_FORM))
                        //{
                        //    ErrorMsg += "劑型(中文) 為必填欄位。\r\n";
                        //}

                        if (!string.IsNullOrEmpty(model.Form.DOSAGE_FORM_E))
                        {
                            if (!reg.IsMatch(model.Form.DOSAGE_FORM_E))
                            {
                                ErrorMsg += "劑型(英文)請以英文填寫。\r\n";
                            }
                        }
                        //else
                        //{
                        //    ErrorMsg += "劑型(英文) 為必填欄位。\r\n";
                        //}

                        //if (string.IsNullOrEmpty(model.Form.ISSUE_DATE))
                        //{
                        //    ErrorMsg += "核准日期(中文) 為必填欄位。\r\n";
                        //}

                        //if (model.Form.IS_EXPIR_DATE_CHECK)
                        //{
                        //    if (string.IsNullOrEmpty(model.Form.EXPIR_DATE))
                        //    {
                        //        ErrorMsg += "有效日期(中文) 為必填欄位。\r\n";
                        //    }
                        //}

                        if (!string.IsNullOrEmpty(model.Form.MF_CONT))
                        {
                            if (model.Form.MF_CONT.Trim().Substring(model.Form.MF_CONT.Trim().Length - 1, 1) != "：")
                            {
                                ErrorMsg += "處方說明(中文)最後請以「：」結尾。\r\n";
                            }
                        }
                        //else
                        //{
                        //    ErrorMsg += "處方說明(中文) 為必填欄位。\r\n";
                        //}
                        if (!string.IsNullOrEmpty(model.Form.MF_CONT_E))
                        {
                            if (model.Form.MF_CONT_E.Trim().Substring(model.Form.MF_CONT_E.Trim().Length - 1, 1) != ":" && model.Form.MF_CONT_E.Trim().Substring(model.Form.MF_CONT_E.Trim().Length - 1, 1) != "：")
                            {
                                ErrorMsg += "處方說明(英文)最後請以「:」結尾。\r\n";
                            }

                            if (!reg.IsMatch(model.Form.MF_CONT_E))
                            {
                                ErrorMsg += "處方說明(英文)請以英文填寫。\r\n";
                            }
                        }
                        //else
                        //{
                        //    ErrorMsg += "處方說明(英文) 為必填欄位。\r\n";
                        //}

                        if (model.Form.IS_Concentrate_CHECK)
                        {
                            //if (string.IsNullOrEmpty(model.Form.PC_SCALE_1) || string.IsNullOrEmpty(model.Form.PC_SCALE_1E) ||
                            //    string.IsNullOrEmpty(model.Form.PC_SCALE_2E) || string.IsNullOrEmpty(model.Form.PC_SCALE_21) ||
                            //    string.IsNullOrEmpty(model.Form.PC_SCALE_22) || string.IsNullOrEmpty(model.Form.PC_SCALE_23) ||
                            //    string.IsNullOrEmpty(model.Form.PC_SCALE_24))
                            //{
                            //    ErrorMsg += "請輸入生藥與浸膏比例中文英文完整資訊。\r\n";
                            //}
                            //else
                            //{
                            //    if (!(reg1.IsMatch(model.Form.PC_SCALE_1) && reg1.IsMatch(model.Form.PC_SCALE_21) &&
                            //          reg1.IsMatch(model.Form.PC_SCALE_22) && reg1.IsMatch(model.Form.PC_SCALE_23) &&
                            //          reg1.IsMatch(model.Form.PC_SCALE_24)))
                            //    {
                            //        ErrorMsg += "請輸入生藥與浸膏比例中文英文請填寫數字。\r\n";
                            //    }
                            //}

                            //if (model.PC.GoodsList.Count == 1 && (string.IsNullOrEmpty(model.PC.GoodsList[0].PC_NAME)) || string.IsNullOrEmpty(model.PC.GoodsList[0].PC_ENAME))
                            //{
                            //    ErrorMsg += "請輸入賦形劑完整資訊。\r\n";
                            //}
                        }

                        if (model.Form.IS_INDIOCATION_CHECK)
                        {
                            //適應症(中文)
                            if (!string.IsNullOrEmpty(model.Form.INDIOCATION))
                            {
                                if (model.Form.INDIOCATION.Trim().Substring(model.Form.INDIOCATION.Trim().Length - 1, 1) != "。")
                                {
                                    ErrorMsg += "適應症(中文)最後請以「。」結尾。\r\n";
                                }
                            }
                            //else
                            //{
                            //    ErrorMsg += "勾選適應症時請填寫適應症(中文)資訊。\r\n";
                            //}

                            //適應症(英文)
                            if (!string.IsNullOrEmpty(model.Form.INDIOCATION_E))
                            {
                                if (model.Form.INDIOCATION_E.Trim().Substring(model.Form.INDIOCATION_E.Trim().Length - 1, 1) != ".")
                                {
                                    ErrorMsg += "適應症(英文)最後請以「.」結尾。\r\n";
                                }

                                if (!reg.IsMatch(model.Form.INDIOCATION_E))
                                {
                                    ErrorMsg += "適應症(英文)請以英文填寫。\r\n";
                                }
                            }
                            //else
                            //{
                            //    ErrorMsg += "勾選適應症時請填寫適應症(英文)資訊。\r\n";
                            //}
                        }

                        if (model.Form.IS_EFFICACY_CHECK)
                        {
                            //效能(中文)
                            if (!string.IsNullOrEmpty(model.Form.EFFICACY))
                            {
                                if (model.Form.EFFICACY.Trim().Substring(model.Form.EFFICACY.Trim().Length - 1, 1) != "。")
                                {
                                    ErrorMsg += "效能(中文)最後請以「。」結尾。\r\n";
                                }
                            }
                            //else
                            //{
                            //    ErrorMsg += "勾選效能時請填寫效能(中文)資訊。\r\n";
                            //}

                            //效能(英文)
                            if (!string.IsNullOrEmpty(model.Form.EFFICACY_E))
                            {
                                if (model.Form.EFFICACY_E.Trim().Substring(model.Form.EFFICACY_E.Trim().Length - 1, 1) != ".")
                                {
                                    ErrorMsg += "效能(英文)最後請以「.」結尾。\r\n";
                                }

                                if (!reg.IsMatch(model.Form.EFFICACY_E))
                                {
                                    ErrorMsg += "效能(英文)請以英文填寫。\r\n";
                                }
                            }
                            //else
                            //{
                            //    ErrorMsg += "勾選效能時請填寫效能(英文)資訊。\r\n";
                            //}
                        }

                        if (ErrorMsg == "")
                        {
                            //一般存檔
                            ErrorMsg += dao.AppendApply005001(model);
                            if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                            {
                                //補件存檔
                                ErrorMsg += dao.AppendApplyDoc005001(model);
                            }
                        }
                    }
                }

                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";

                    if (model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20")
                    {
                        dao.CaseFinishMail_005001(model);
                    }
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


        #region 申請表列印
        #region 舊版

        //[HttpPost]
        //[DisplayName("005001_申請單套表")]
        //public void PreviewApplyForm(Apply_005001ViewModel model)
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

        //        if (model.Form.IS_DRUG_ABROAD_CHECK)
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
        //            Spire.Doc.Fields.TextRange range1 = sTable[0, 0].AddParagraph().AppendText("製造廠名稱：" + model.Form.MF_CNT_NAME);
        //            range1.CharacterFormat.FontName = "新細明體";
        //            range1.CharacterFormat.FontSize = 8;

        //            //製造廠名稱(英文)
        //            Spire.Doc.Fields.TextRange range2 = sTable[1, 0].AddParagraph().AppendText("Manufacturer: " + model.Form.MF_CNT_NAME_E);
        //            range2.CharacterFormat.FontName = "Times New Roman";
        //            range2.CharacterFormat.FontSize = 8;

        //            //製造廠地址(中文)
        //            Spire.Doc.Fields.TextRange range3 = sTable[3, 0].AddParagraph().AppendText("製造廠地址：" + "中華民國台灣" + model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL);
        //            range3.CharacterFormat.FontName = "新細明體";
        //            range3.CharacterFormat.FontSize = 8;

        //            //製造廠地址(英文)
        //            Spire.Doc.Fields.TextRange range4 = sTable[4, 0].AddParagraph().AppendText("Manufacturing Plant Location: " + model.Form.MF_ADDR_E);
        //            range4.CharacterFormat.FontName = "Times New Roman";
        //            range4.CharacterFormat.FontSize = 8;

        //            //藥品名稱(中文)
        //            Spire.Doc.Fields.TextRange range5 = sTable[6, 0].AddParagraph().AppendText("藥品名稱：" + model.Form.DRUG_NAME);
        //            range5.CharacterFormat.FontName = "新細明體";
        //            range5.CharacterFormat.FontSize = 8;

        //            //藥品名稱(英文)
        //            Spire.Doc.Fields.TextRange range6 = sTable[7, 0].AddParagraph().AppendText("Product Name: " + model.Form.DRUG_NAME_E);
        //            range6.CharacterFormat.FontName = "Times New Roman";
        //            range6.CharacterFormat.FontSize = 8;

        //            //劑型(中文)
        //            Spire.Doc.Fields.TextRange range7 = sTable[6, 4].AddParagraph().AppendText("劑型：" + model.Form.DOSAGE_FORM);
        //            range7.CharacterFormat.FontName = "新細明體";
        //            range7.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range8 = sTable[6, 5].AddParagraph().AppendText(model.Form.DOSAGE_FORM);
        //            //range8.CharacterFormat.FontName = "新細明體";
        //            //range8.CharacterFormat.FontSize = 8;

        //            //劑型(英文)
        //            Spire.Doc.Fields.TextRange range9 = sTable[7, 4].AddParagraph().AppendText("Dosage Form: " + model.Form.DOSAGE_FORM_E);
        //            range9.CharacterFormat.FontName = "Times New Roman";
        //            range9.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range10 = sTable[7, 5].AddParagraph().AppendText(model.Form.DOSAGE_FORM_E);
        //            //range10.CharacterFormat.FontName = "Times New Roman";
        //            //range10.CharacterFormat.FontSize = 8;

        //            //外銷品名(中文)
        //            Spire.Doc.Fields.TextRange range23 = sTable[9, 0].AddParagraph().AppendText("外銷品名：" + model.Form.DRUG_ABROAD_NAME);
        //            range23.CharacterFormat.FontName = "新細明體";
        //            range23.CharacterFormat.FontSize = 8;

        //            //外銷品名(英文)
        //            Spire.Doc.Fields.TextRange range25 = sTable[10, 0].AddParagraph().AppendText("Export Name: " + model.Form.DRUG_ABROAD_NAME_E);
        //            range25.CharacterFormat.FontName = "Times New Roman";
        //            range25.CharacterFormat.FontSize = 8;

        //            if (model.Form.IS_EXPIR_DATE_CHECK)
        //            {
        //                //有效日期(中文)
        //                sTable.ApplyHorizontalMerge(9, 4, 5);
        //                var dateTW = model.Form.EXPIR_DATE_TW.Split('/');
        //                var dateIns = dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日";
        //                Spire.Doc.Fields.TextRange range15 = sTable[9, 4].AddParagraph().AppendText($"有效日期：{dateIns}");
        //                range15.CharacterFormat.FontName = "新細明體";
        //                range15.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range16 = sTable[9, 5].AddParagraph().AppendText(dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日");
        //                //range16.CharacterFormat.FontName = "新細明體";
        //                //range16.CharacterFormat.FontSize = 8;

        //                //有效日期(英文)
        //                sTable.ApplyHorizontalMerge(10, 4, 5);
        //                var dateInsE = Convert.ToDateTime(model.Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //                Spire.Doc.Fields.TextRange range17 = sTable[10, 4].AddParagraph().AppendText($"Date of Issue: {dateInsE}");
        //                range17.CharacterFormat.FontName = "Times New Roman";
        //                range17.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range18 = sTable[10, 5].AddParagraph().AppendText(Convert.ToDateTime(model.Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //                //range18.CharacterFormat.FontName = "Times New Roman";
        //                //range18.CharacterFormat.FontSize = 8;
        //            }

        //            //許可證字號(中文)
        //            sTable.ApplyHorizontalMerge(12, 0, 1);
        //            var plcdIns = model.Form.PL_CD_TEXT + "第" + model.Form.PL_Num + "號";
        //            Spire.Doc.Fields.TextRange range11 = sTable[12, 0].AddParagraph().AppendText($"許可證字號：{plcdIns}");
        //            range11.CharacterFormat.FontName = "新細明體";
        //            range11.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range12 = sTable[12, 1].AddParagraph().AppendText(model.Form.PL_CD_TEXT + "第" + model.Form.PL_Num + "號");
        //            //range12.CharacterFormat.FontName = "新細明體";
        //            //range12.CharacterFormat.FontSize = 8;

        //            //許可證字號(英文)
        //            //許可證字號(英文)
        //            sTable.ApplyHorizontalMerge(13, 0, 1);
        //            var plcdeIns = model.Form.PL_CD_E + "-" + model.Form.PL_Num_E;
        //            Spire.Doc.Fields.TextRange range13 = sTable[13, 0].AddParagraph().AppendText($"Registration Number: {plcdeIns}");
        //            range13.CharacterFormat.FontName = "Times New Roman";
        //            range13.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range14 = sTable[13, 1].AddParagraph().AppendText(model.Form.PL_CD_E + "-" + model.Form.PL_Num_E);
        //            //range14.CharacterFormat.FontName = "Times New Roman";
        //            //range14.CharacterFormat.FontSize = 8;

        //            //核准日期(中文)
        //            sTable.ApplyHorizontalMerge(12, 4, 5);
        //            var dateTW1 = model.Form.ISSUE_DATE_TW.Split('/');
        //            var dateTW1Ins = dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日";
        //            Spire.Doc.Fields.TextRange range19 = sTable[12, 4].AddParagraph().AppendText($"核准日期：{dateTW1Ins}");
        //            range19.CharacterFormat.FontName = "新細明體";
        //            range19.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range20 = sTable[12, 5].AddParagraph().AppendText(dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日");
        //            //range20.CharacterFormat.FontName = "新細明體";
        //            //range20.CharacterFormat.FontSize = 8;

        //            //核准日期(英文)
        //            sTable.ApplyHorizontalMerge(13, 4, 5);
        //            var issIns = Convert.ToDateTime(model.Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //            Spire.Doc.Fields.TextRange range21 = sTable[13, 4].AddParagraph().AppendText($"Date of Issue: {issIns}");
        //            range21.CharacterFormat.FontName = "Times New Roman";
        //            range21.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range22 = sTable[13, 5].AddParagraph().AppendText(Convert.ToDateTime(model.Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //            //range22.CharacterFormat.FontName = "Times New Roman";
        //            //range22.CharacterFormat.FontSize = 8;
        //        }
        //        else
        //        {
        //            //加入table
        //            Spire.Doc.Table sTable = s.AddTable(true);
        //            sTable.ResetCells(11, 5);
        //            //合併表格
        //            sTable.ApplyHorizontalMerge(0, 0, 4);
        //            sTable.ApplyHorizontalMerge(1, 0, 4);
        //            sTable.ApplyHorizontalMerge(2, 0, 4);
        //            sTable.ApplyHorizontalMerge(3, 0, 4);
        //            sTable.ApplyHorizontalMerge(4, 0, 4);
        //            sTable.ApplyHorizontalMerge(5, 0, 4);
        //            sTable.ApplyHorizontalMerge(6, 0, 2);
        //            sTable.ApplyHorizontalMerge(7, 0, 2);
        //            sTable.ApplyHorizontalMerge(8, 0, 4);
        //            //邊框隱藏
        //            Spire.Doc.Table table = (Spire.Doc.Table)s.Tables[0];
        //            table.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            table.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;

        //            //表格下邊框設定
        //            for (var i = 0; i < 5; i++)
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
        //            Spire.Doc.Fields.TextRange range1 = sTable[0, 0].AddParagraph().AppendText("製造廠名稱：" + model.Form.MF_CNT_NAME);
        //            range1.CharacterFormat.FontName = "新細明體";
        //            range1.CharacterFormat.FontSize = 8;

        //            //製造廠名稱(英文)
        //            Spire.Doc.Fields.TextRange range2 = sTable[1, 0].AddParagraph().AppendText("Manufacturer: " + model.Form.MF_CNT_NAME_E);
        //            range2.CharacterFormat.FontName = "Times New Roman";
        //            range2.CharacterFormat.FontSize = 8;

        //            //製造廠地址(中文)
        //            Spire.Doc.Fields.TextRange range3 = sTable[3, 0].AddParagraph().AppendText("製造廠地址：" + "中華民國台灣" + model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL);
        //            range3.CharacterFormat.FontName = "新細明體";
        //            range3.CharacterFormat.FontSize = 8;

        //            //製造廠地址(英文)
        //            Spire.Doc.Fields.TextRange range4 = sTable[4, 0].AddParagraph().AppendText("Manufacturing Plant Location: " + model.Form.MF_ADDR_E);
        //            range4.CharacterFormat.FontName = "Times New Roman";
        //            range4.CharacterFormat.FontSize = 8;

        //            //藥品名稱(中文)
        //            Spire.Doc.Fields.TextRange range5 = sTable[6, 0].AddParagraph().AppendText("藥品名稱：" + model.Form.DRUG_NAME);
        //            range5.CharacterFormat.FontName = "新細明體";
        //            range5.CharacterFormat.FontSize = 8;

        //            //藥品名稱(英文)
        //            Spire.Doc.Fields.TextRange range6 = sTable[7, 0].AddParagraph().AppendText("Product Name: " + model.Form.DRUG_NAME_E);
        //            range6.CharacterFormat.FontName = "Times New Roman";
        //            range6.CharacterFormat.FontSize = 8;

        //            //劑型(中文)
        //            sTable.ApplyHorizontalMerge(6, 3, 4);
        //            Spire.Doc.Fields.TextRange range7 = sTable[6, 3].AddParagraph().AppendText("劑型：" + model.Form.DOSAGE_FORM);
        //            range7.CharacterFormat.FontName = "新細明體";
        //            range7.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range8 = sTable[6, 5].AddParagraph().AppendText(model.Form.DOSAGE_FORM);
        //            //range8.CharacterFormat.FontName = "新細明體";
        //            //range8.CharacterFormat.FontSize = 8;

        //            //劑型(英文)
        //            sTable.ApplyHorizontalMerge(7, 3, 4);
        //            Spire.Doc.Fields.TextRange range9 = sTable[7, 3].AddParagraph().AppendText("Dosage Form: " + model.Form.DOSAGE_FORM_E);
        //            range9.CharacterFormat.FontName = "Times New Roman";
        //            range9.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range10 = sTable[7, 5].AddParagraph().AppendText(model.Form.DOSAGE_FORM_E);
        //            //range10.CharacterFormat.FontName = "Times New Roman";
        //            //range10.CharacterFormat.FontSize = 8;

        //            if (model.Form.IS_EXPIR_DATE_CHECK)
        //            {
        //                //許可證字號(中文)
        //                sTable.ApplyHorizontalMerge(9, 0, 1);
        //                var plcdIns = model.Form.PL_CD_TEXT + "第" + model.Form.PL_Num + "號";
        //                Spire.Doc.Fields.TextRange range11 = sTable[9, 0].AddParagraph().AppendText($"許可證字號：{plcdIns}");
        //                range11.CharacterFormat.FontName = "新細明體";
        //                range11.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range12 = sTable[9, 1].AddParagraph().AppendText(model.Form.PL_CD_TEXT + "第" + model.Form.PL_Num + "號");
        //                //range12.CharacterFormat.FontName = "新細明體";
        //                //range12.CharacterFormat.FontSize = 8;

        //                //許可證字號(英文)
        //                sTable.ApplyHorizontalMerge(10, 0, 1);
        //                var plcdeIns = model.Form.PL_CD_E + "-" + model.Form.PL_Num_E;
        //                Spire.Doc.Fields.TextRange range13 = sTable[10, 0].AddParagraph().AppendText($"Registration Number: {plcdeIns}");
        //                range13.CharacterFormat.FontName = "Times New Roman";
        //                range13.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range14 = sTable[10, 1].AddParagraph().AppendText(model.Form.PL_CD_E + "-" + model.Form.PL_Num_E);
        //                //range14.CharacterFormat.FontName = "Times New Roman";
        //                //range14.CharacterFormat.FontSize = 8;
        //            }
        //            else
        //            {
        //                //許可證字號(中文)
        //                sTable.ApplyHorizontalMerge(9, 0, 3);
        //                var plcdIns = model.Form.PL_CD_TEXT + "第" + model.Form.PL_Num + "號";
        //                Spire.Doc.Fields.TextRange range11 = sTable[9, 0].AddParagraph().AppendText($"許可證字號：{plcdIns}");
        //                range11.CharacterFormat.FontName = "新細明體";
        //                range11.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range12 = sTable[9, 1].AddParagraph().AppendText(model.Form.PL_CD_TEXT + "第" + model.Form.PL_Num + "號");
        //                //range12.CharacterFormat.FontName = "新細明體";
        //                //range12.CharacterFormat.FontSize = 8;

        //                //許可證字號(英文)
        //                sTable.ApplyHorizontalMerge(10, 0, 3);
        //                var plcdeIns = model.Form.PL_CD_E + "-" + model.Form.PL_Num_E;
        //                Spire.Doc.Fields.TextRange range13 = sTable[10, 0].AddParagraph().AppendText($"Registration Number: {plcdeIns}");
        //                range13.CharacterFormat.FontName = "Times New Roman";
        //                range13.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range14 = sTable[10, 1].AddParagraph().AppendText(model.Form.PL_CD_E + "-" + model.Form.PL_Num_E);
        //                //range14.CharacterFormat.FontName = "Times New Roman";
        //                //range14.CharacterFormat.FontSize = 8;
        //            }


        //            if (model.Form.IS_EXPIR_DATE_CHECK)
        //            {
        //                //有效日期(中文)
        //                sTable.ApplyHorizontalMerge(9, 2, 3);
        //                var dateTW = model.Form.EXPIR_DATE_TW.Split('/');
        //                var dateTwIns = dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日";
        //                Spire.Doc.Fields.TextRange range15 = sTable[9, 2].AddParagraph().AppendText($"有效日期：{dateTwIns}");
        //                range15.CharacterFormat.FontName = "新細明體";
        //                range15.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range16 = sTable[9, 3].AddParagraph().AppendText(dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日");
        //                //range16.CharacterFormat.FontName = "新細明體";
        //                //range16.CharacterFormat.FontSize = 8;

        //                //有效日期(英文)
        //                sTable.ApplyHorizontalMerge(10, 2, 3);
        //                var expIns = Convert.ToDateTime(model.Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //                Spire.Doc.Fields.TextRange range17 = sTable[10, 2].AddParagraph().AppendText($"Date of Issue: {expIns}");
        //                range17.CharacterFormat.FontName = "Times New Roman";
        //                range17.CharacterFormat.FontSize = 8;
        //                //Spire.Doc.Fields.TextRange range18 = sTable[10, 3].AddParagraph().AppendText(Convert.ToDateTime(model.Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //                //range18.CharacterFormat.FontName = "Times New Roman";
        //                //range18.CharacterFormat.FontSize = 8;
        //            }

        //            //核准日期(中文)
        //            sTable.ApplyHorizontalMerge(9, 4, 4);
        //            var dateTW1 = model.Form.ISSUE_DATE_TW.Split('/');
        //            var dateTW1Ins = dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日";
        //            Spire.Doc.Fields.TextRange range19 = sTable[9, 4].AddParagraph().AppendText($"核准日期：{dateTW1Ins}");
        //            range19.CharacterFormat.FontName = "新細明體";
        //            range19.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range20 = sTable[9, 5].AddParagraph().AppendText(dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日");
        //            //range20.CharacterFormat.FontName = "新細明體";
        //            //range20.CharacterFormat.FontSize = 8;

        //            //核准日期(英文)
        //            sTable.ApplyHorizontalMerge(10, 4, 4);
        //            var issIns = Convert.ToDateTime(model.Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        //            Spire.Doc.Fields.TextRange range21 = sTable[10, 4].AddParagraph().AppendText($"Date of Issue: {issIns}");
        //            range21.CharacterFormat.FontName = "Times New Roman";
        //            range21.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange range22 = sTable[10, 5].AddParagraph().AppendText(Convert.ToDateTime(model.Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
        //            //range22.CharacterFormat.FontName = "Times New Roman";
        //            //range22.CharacterFormat.FontSize = 8;
        //        }

        //        Paragraph para7 = s.AddParagraph();
        //        para7.AppendText("\r\n處  方：" + model.Form.MF_CONT);

        //        Paragraph para13 = s.AddParagraph();
        //        para13.AppendText("Formula: " + model.Form.MF_CONT_E);

        //        //加入table
        //        Spire.Doc.Table sTable1 = s.AddTable(true);
        //        var rowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.DI.GoodsList.Count) / 2));
        //        if (model.Form.IS_Concentrate_CHECK)
        //        {
        //            rowCount += Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.PC.GoodsList.Count) / 2)) + 2;
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

        //        for (var i = 0; i < Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.DI.GoodsList.Count) / 2)); i++)
        //        {
        //            if (i * 2 <= model.DI.GoodsList.Count)
        //            {
        //                Spire.Doc.Fields.TextRange rangeN1 =
        //                    sTable1[i, 0].AddParagraph().AppendText(model.DI.GoodsList[i].DI_NAME);
        //                rangeN1.CharacterFormat.FontName = "新細明體";
        //                rangeN1.CharacterFormat.FontSize = 8;

        //                Spire.Doc.Fields.TextRange rangeN2 =
        //                    sTable1[i, 1].AddParagraph().AppendText(model.DI.GoodsList[i].DI_ENAME);
        //                rangeN2.CharacterFormat.FontName = "Times New Roman";
        //                rangeN2.CharacterFormat.FontSize = 8;

        //                var parN3 = sTable1[i, 2].AddParagraph();
        //                parN3.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                Spire.Doc.Fields.TextRange rangeN3 =
        //                    parN3.AppendText(model.DI.GoodsList[i].DI_CONT);
        //                rangeN3.CharacterFormat.FontName = "Times New Roman";
        //                rangeN3.CharacterFormat.FontSize = 8;

        //                Spire.Doc.Fields.TextRange rangeN4 =
        //                    sTable1[i, 3].AddParagraph().AppendText(model.DI.GoodsList[i].DI_UNIT);
        //                rangeN4.CharacterFormat.FontName = "Times New Roman";
        //                rangeN4.CharacterFormat.FontSize = 8;

        //                if ((i + 1) * 2 <= model.DI.GoodsList.Count)
        //                {
        //                    Spire.Doc.Fields.TextRange rangeN5 =
        //                        sTable1[i, 4].AddParagraph().AppendText(model.DI.GoodsList[(i + 1) * 2].DI_NAME);
        //                    rangeN5.CharacterFormat.FontName = "新細明體";
        //                    rangeN5.CharacterFormat.FontSize = 8;

        //                    Spire.Doc.Fields.TextRange rangeN6 =
        //                        sTable1[i, 5].AddParagraph().AppendText(model.DI.GoodsList[(i + 1) * 2].DI_ENAME);
        //                    rangeN6.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN6.CharacterFormat.FontSize = 8;

        //                    var parN7 = sTable1[i, 6].AddParagraph();
        //                    parN7.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                    Spire.Doc.Fields.TextRange rangeN7 =
        //                        parN7.AppendText(model.DI.GoodsList[(i + 1) * 2].DI_CONT);
        //                    rangeN7.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN7.CharacterFormat.FontSize = 8;

        //                    Spire.Doc.Fields.TextRange rangeN8 =
        //                        sTable1[i, 7].AddParagraph().AppendText(model.DI.GoodsList[(i + 1) * 2].DI_UNIT);
        //                    rangeN8.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN8.CharacterFormat.FontSize = 8;
        //                }
        //            }
        //        }

        //        if (model.Form.IS_Concentrate_CHECK)
        //        {
        //            var rowCount1 = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.DI.GoodsList.Count) / 2));
        //            sTable1.ApplyHorizontalMerge(rowCount1, 0, 7);
        //            sTable1.ApplyHorizontalMerge(rowCount1 + 1, 0, 7);
        //            var rangeN9Text = "以上生藥製成浸膏" + model.Form.PC_SCALE_1 + model.Form.PC_SCALE_1E +
        //                              "(生藥與浸膏比例 " + model.Form.PC_SCALE_21 + ":" + model.Form.PC_SCALE_22 + " = " + model.Form.PC_SCALE_23 + ":" + model.Form.PC_SCALE_24 + ")";
        //            Spire.Doc.Fields.TextRange rangeN9 = sTable1[rowCount1, 0].AddParagraph().AppendText(rangeN9Text);
        //            rangeN9.CharacterFormat.FontName = "新細明體";
        //            rangeN9.CharacterFormat.FontSize = 8;
        //            rangeN9Text = "Above raw herbs equivalent to " + model.Form.PC_SCALE_1 + model.Form.PC_SCALE_2E +
        //                          " herbal extract.(Ratio of raw herbs and extract " + model.Form.PC_SCALE_21 + ":" + model.Form.PC_SCALE_22 + " = " + model.Form.PC_SCALE_23 + ":" + model.Form.PC_SCALE_24 + ")";
        //            Spire.Doc.Fields.TextRange rangeN10 = sTable1[rowCount1 + 1, 0].AddParagraph().AppendText(rangeN9Text);
        //            rangeN10.CharacterFormat.FontName = "Times New Roman";
        //            rangeN10.CharacterFormat.FontSize = 8;

        //            var j = 0;
        //            for (var i = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.DI.GoodsList.Count) / 2)) + 2; i < rowCount; i++)
        //            {
        //                if (j * 2 <= model.PC.GoodsList.Count)
        //                {
        //                    Spire.Doc.Fields.TextRange rangeN1 =
        //                        sTable1[i, 0].AddParagraph().AppendText(model.PC.GoodsList[j].PC_NAME);
        //                    rangeN1.CharacterFormat.FontName = "新細明體";
        //                    rangeN1.CharacterFormat.FontSize = 8;
        //                    Spire.Doc.Fields.TextRange rangeN2 =
        //                        sTable1[i, 1].AddParagraph().AppendText(model.PC.GoodsList[j].PC_ENAME);
        //                    rangeN2.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN2.CharacterFormat.FontSize = 8;

        //                    var parN3 = sTable1[i, 2].AddParagraph();
        //                    parN3.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                    Spire.Doc.Fields.TextRange rangeN3 =
        //                        parN3.AppendText(model.PC.GoodsList[j].PC_CONT);
        //                    rangeN3.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN3.CharacterFormat.FontSize = 8;

        //                    Spire.Doc.Fields.TextRange rangeN4 =
        //                        sTable1[i, 3].AddParagraph().AppendText(model.PC.GoodsList[j].PC_UNIT);
        //                    rangeN4.CharacterFormat.FontName = "Times New Roman";
        //                    rangeN4.CharacterFormat.FontSize = 8;
        //                    if ((j + 1) * 2 <= model.PC.GoodsList.Count)
        //                    {
        //                        Spire.Doc.Fields.TextRange rangeN5 =
        //                            sTable1[i, 4].AddParagraph().AppendText(model.PC.GoodsList[(j + 1) * 2].PC_NAME);
        //                        rangeN5.CharacterFormat.FontName = "新細明體";
        //                        rangeN5.CharacterFormat.FontSize = 8;
        //                        Spire.Doc.Fields.TextRange rangeN6 =
        //                            sTable1[i, 5].AddParagraph().AppendText(model.PC.GoodsList[(j + 1) * 2].PC_ENAME);
        //                        rangeN6.CharacterFormat.FontName = "Times New Roman";
        //                        rangeN6.CharacterFormat.FontSize = 8;

        //                        var parN7 = sTable1[i, 6].AddParagraph();
        //                        parN7.Format.HorizontalAlignment = HorizontalAlignment.Right;
        //                        Spire.Doc.Fields.TextRange rangeN7 =
        //                            parN7.AppendText(model.PC.GoodsList[(j + 1) * 2].PC_CONT);
        //                        rangeN7.CharacterFormat.FontName = "Times New Roman";
        //                        rangeN7.CharacterFormat.FontSize = 8;

        //                        Spire.Doc.Fields.TextRange rangeN8 =
        //                            sTable1[i, 7].AddParagraph().AppendText(model.PC.GoodsList[(j + 1) * 2].PC_UNIT);
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

        //        //Paragraph para8 = s.AddParagraph();
        //        //para8.AppendText("");

        //        Section section = doc.Sections[0];


        //        //適應症
        //        if (model.Form.IS_INDIOCATION_CHECK)
        //        {
        //            //// 畫適應症的表格
        //            //Spire.Doc.Table sTable2 = s.AddTable(true);
        //            //sTable2.ResetCells(2, 2);
        //            //sTable2.Rows[0].Cells[0].Width = 5;
        //            //sTable2.Rows[1].Cells[0].Width = 5;
        //            ////邊框隱藏
        //            //sTable2.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            //sTable2.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            //sTable2.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            //sTable2.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            //sTable2.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            //sTable2.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;
        //            //Spire.Doc.Fields.TextRange IIC = sTable2[0, 0].AddParagraph().AppendText("適應症：");
        //            //IIC.CharacterFormat.FontName = "新細明體";
        //            //IIC.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange IIC1 = sTable2[0, 1].AddParagraph().AppendText(model.Form.INDIOCATION);
        //            //IIC1.CharacterFormat.FontName = "新細明體";
        //            //IIC1.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange IIC2 = sTable2[1, 0].AddParagraph().AppendText("Indication(s): ");
        //            //IIC2.CharacterFormat.FontName = "新細明體";
        //            //IIC2.CharacterFormat.FontSize = 8;
        //            //Spire.Doc.Fields.TextRange IIC3 = sTable2[1, 1].AddParagraph().AppendText(model.Form.INDIOCATION_E);
        //            //IIC3.CharacterFormat.FontName = "新細明體";
        //            //IIC3.CharacterFormat.FontSize = 8;
        //            Paragraph para8 = s.AddParagraph();
        //            para8.AppendText("適應症：" + model.Form.INDIOCATION);
        //            para8.ApplyStyle("titleStyle5");
        //            //Paragraph para8 = section.Paragraphs[6];
        //            //para8.AppendText("適應症：" + model.Form.INDIOCATION);
        //            para8.Format.FirstLineIndent = 30;

        //            Paragraph para9 = s.AddParagraph();
        //            para9.AppendText("Indication(s): " + model.Form.INDIOCATION_E);
        //            para9.ApplyStyle("titleStyle6");
        //        }

        //        //效能
        //        if (model.Form.IS_EFFICACY_CHECK)
        //        {
        //            Paragraph para10 = s.AddParagraph();
        //            para10.AppendText("效能：" + model.Form.EFFICACY);
        //            para10.ApplyStyle("titleStyle5");

        //            Paragraph para11 = s.AddParagraph();
        //            para11.AppendText("Efficacy: " + model.Form.EFFICACY_E);
        //            para11.ApplyStyle("titleStyle6");
        //        }

        //        s.AddParagraph().AppendText("\r\n");

        //        // 簽章區塊
        //        Spire.Doc.Fields.TextBox tb = s.AddParagraph().AppendTextBox(170, 140);
        //        tb.Format.HorizontalAlignment = ShapeHorizontalAlignment.Right;
        //        tb.Format.LineStyle = TextBoxLineStyle.Simple;
        //        tb.Format.NoLine = true;
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
        //        para14.ApplyStyle("titleStyle6");

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
        public void PreviewApplyForm(Apply_005001ViewModel model)
        {
            ShareCodeListModel sc = new ShareCodeListModel();
            Apply_005001ViewModel fm = model;
            string path = "";

            #region 判斷勾選的值，決定用哪個套表

            // 外銷品名
            if (model.Form.IS_DRUG_ABROAD_CHECK)
            {
                // 有效日期
                if (model.Form.IS_EXPIR_DATE_CHECK)
                {
                    // 效能 適應症 濃縮劑
                    if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_0.docx");
                    }
                    // 效能 適應症
                    else if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_3.docx");
                    }
                    // 效能 
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_4.docx");
                    }
                    // 適應症
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_5.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_6.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_0_6.docx");
                    }
                }
                else
                {
                    // 效能 適應症 濃縮劑
                    if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_0.docx");
                    }
                    // 效能 適應症
                    else if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_3.docx");
                    }
                    // 效能 
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_4.docx");
                    }
                    // 適應症
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_5.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_6.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_0_1_6.docx");
                    }
                }
            }
            else
            {
                // 有效日期
                if (model.Form.IS_EXPIR_DATE_CHECK)
                {
                    // 效能 適應症 濃縮劑
                    if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_0.docx");
                    }
                    // 效能 適應症
                    else if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_3.docx");
                    }
                    // 效能 
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_4.docx");
                    }
                    // 適應症
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_5.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_6.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_1_6.docx");
                    }
                }
                else
                {
                    // 效能 適應症 濃縮劑
                    if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_0.docx");
                    }
                    // 效能 適應症
                    else if (model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_1.docx");
                    }
                    // 效能 濃縮劑
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_2.docx");
                    }
                    // 適應症 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_3.docx");
                    }
                    // 效能 
                    else if (model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_4.docx");
                    }
                    // 適應症
                    else if (!model.Form.IS_EFFICACY_CHECK && model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_5.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && model.Form.IS_Concentrate_CHECK)
                    {
                        path = Server.MapPath("~/Sample/apply005001_1_0_6.docx");
                    }
                    // 濃縮劑
                    else if (!model.Form.IS_EFFICACY_CHECK && !model.Form.IS_INDIOCATION_CHECK && !model.Form.IS_Concentrate_CHECK)
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
                    doc.ReplaceText("[$MF_CNT_NAME]", model.Form.MF_CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_CNT_NAME_E]", model.Form.MF_CNT_NAME_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$TAX_ORG_CITY]", "中華民國台灣" + model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_ADDR_E]", model.Form.MF_ADDR_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_NAME]", model.Form.DRUG_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_NAME_E]", model.Form.DRUG_NAME_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DOSAGE_FORM]", model.Form.DOSAGE_FORM.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DOSAGE_FORM_E]", model.Form.DOSAGE_FORM_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_ABROAD_NAME]", model.Form.DRUG_ABROAD_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DRUG_ABROAD_NAME_E]", model.Form.DRUG_ABROAD_NAME_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    if (model.Form.IS_EXPIR_DATE_CHECK)
                    {
                        var EdateTW1 = model.Form.EXPIR_DATE_TW.Split('/');
                        var EdateTW1Ins = EdateTW1[0] + "年" + EdateTW1[1] + "月" + EdateTW1[2] + "日";
                        doc.ReplaceText("[$EXPIR_DATE_TW]", EdateTW1Ins.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                        var EissIns = Convert.ToDateTime(model.Form.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                        doc.ReplaceText("[$EXPIR_DATE]", EissIns.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    }
                    doc.ReplaceText("[$PL_Num]", model.Form.PL_CD_TEXT.TONotNullString() + "第" + model.Form.PL_Num + "號".TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$PL_Num_E]", model.Form.PL_CD_E.TONotNullString() + "-" + model.Form.PL_Num_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    var dateTW1 = model.Form.ISSUE_DATE_TW.Split('/');
                    var dateTW1Ins = dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日";
                    doc.ReplaceText("[$ISSUE_DATE_TW]", dateTW1Ins.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    var issIns = Convert.ToDateTime(model.Form.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                    doc.ReplaceText("[$ISSUE_DATE]", issIns, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_CONT]", model.Form.MF_CONT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MF_CONT_E]", model.Form.MF_CONT_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$INDIOCATION]", model.Form.INDIOCATION.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$INDIOCATION_E]", model.Form.INDIOCATION_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$EFFICACY]", model.Form.EFFICACY.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$EFFICACY_E]", model.Form.EFFICACY_E.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    //doc.ReplaceText("[$ADDTABLE]", model.Form.MF_CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    Border border = new Border();
                    var rowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.DI.GoodsList.Count) / 2));
                    var tb = doc.AddTable(rowCount, 8);

                    if (model.Form.IS_Concentrate_CHECK)
                    {
                        var rowCount1 = rowCount + Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.PC.GoodsList.Count) / 2)) + 2;
                        tb = doc.AddTable(rowCount1, 8);
                    }

                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Design = TableDesign.None;
                    var ceiling = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.DI.GoodsList.Count) / 2));
                    for (var i = 0; i < ceiling; i++)
                    {
                        if (i <= model.DI.GoodsList.Count)
                        {
                            tb.Rows[i].Cells[0].Width = 100;
                            tb.Rows[i].Cells[1].Width = 200;
                            tb.Rows[i].Cells[2].Width = 50;
                            tb.Rows[i].Cells[3].Width = 50;
                            tb.Rows[i].Cells[0].Paragraphs[0].Append(model.DI.GoodsList[i].DI_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                            tb.Rows[i].Cells[0].Paragraphs[0].IndentationBefore = 26.5f;
                            tb.Rows[i].Cells[1].Paragraphs[0].Append(model.DI.GoodsList[i].DI_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                            tb.Rows[i].Cells[2].Paragraphs[0].Append(model.DI.GoodsList[i].DI_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                            tb.Rows[i].Cells[3].Paragraphs[0].Append(model.DI.GoodsList[i].DI_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;

                            if ((i + ceiling) < model.DI.GoodsList.Count)
                            {
                                tb.Rows[i].Cells[4].Width = 100;
                                tb.Rows[i].Cells[5].Width = 200;
                                tb.Rows[i].Cells[6].Width = 50;
                                tb.Rows[i].Cells[7].Width = 50;
                                tb.Rows[i].Cells[4].Paragraphs[0].Append(model.DI.GoodsList[(i + ceiling)].DI_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                                tb.Rows[i].Cells[5].Paragraphs[0].Append(model.DI.GoodsList[(i + ceiling)].DI_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                                tb.Rows[i].Cells[6].Paragraphs[0].Append(model.DI.GoodsList[(i + ceiling)].DI_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                                tb.Rows[i].Cells[7].Paragraphs[0].Append(model.DI.GoodsList[(i + ceiling)].DI_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                            }
                        }
                    }

                    if (model.Form.IS_Concentrate_CHECK)
                    {
                        var cnt = rowCount;
                        //rowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.PC.GoodsList.Count) / 2)) + 2;
                        //var tb1 = doc.AddTable(rowCount, 8);
                        //tb1.Alignment = Xceed.Document.NET.Alignment.center;
                        //tb1.Design = TableDesign.None;
                        tb.Rows[cnt].MergeCells(0, 7);
                        tb.Rows[cnt].Paragraphs[0].IndentationBefore = 26.5f;
                        tb.Rows[cnt + 1].MergeCells(0, 7);
                        tb.Rows[cnt + 1].Paragraphs[0].IndentationBefore = 26.5f;
                        tb.Rows[cnt].Cells[0].Paragraphs[0].Append("以上生藥製成浸膏" + model.Form.PC_SCALE_1 + model.Form.PC_SCALE_1E +
                           "(生藥與浸膏比例 " + model.Form.PC_SCALE_21 + ":" + model.Form.PC_SCALE_22 + "=" + model.Form.PC_SCALE_23 + ":" + model.Form.PC_SCALE_24 + ")").FontSize(8).Font("新細明體").Alignment = Alignment.left;
                        tb.Rows[cnt + 1].Cells[0].Paragraphs[0].Append("Above raw herbs equivalent to " + model.Form.PC_SCALE_1 + model.Form.PC_SCALE_1E +
                            " herbal extract. (Ratio of raw herbs and extract " + model.Form.PC_SCALE_21 + ":" + model.Form.PC_SCALE_22 + "=" + model.Form.PC_SCALE_23 + ":" + model.Form.PC_SCALE_24 + ")").FontSize(8).Font("新細明體").Alignment = Alignment.left;
                        var ceiling_pc = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(model.PC.GoodsList.Count) / 2));
                        for (var i = 0; i < ceiling_pc; i++)
                        {
                            if (i <= model.PC.GoodsList.Count)
                            {
                                tb.Rows[cnt + 2].Cells[0].Width = 100;
                                tb.Rows[cnt + 2].Cells[1].Width = 200;
                                tb.Rows[cnt + 2].Cells[2].Width = 50;
                                tb.Rows[cnt + 2].Cells[3].Width = 50;
                                tb.Rows[cnt + 2].Cells[0].Paragraphs[0].Append(model.PC.GoodsList[i].PC_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                                tb.Rows[cnt + 2].Cells[0].Paragraphs[0].IndentationBefore = 26.5f;
                                tb.Rows[cnt + 2].Cells[1].Paragraphs[0].Append(model.PC.GoodsList[i].PC_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                                tb.Rows[cnt + 2].Cells[2].Paragraphs[0].Append(model.PC.GoodsList[i].PC_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                                tb.Rows[cnt + 2].Cells[3].Paragraphs[0].Append(model.PC.GoodsList[i].PC_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;

                                if ((i + ceiling_pc) < model.PC.GoodsList.Count)
                                {
                                    tb.Rows[cnt + 2].Cells[4].Width = 100;
                                    tb.Rows[cnt + 2].Cells[5].Width = 200;
                                    tb.Rows[cnt + 2].Cells[6].Width = 50;
                                    tb.Rows[cnt + 2].Cells[7].Width = 50;
                                    tb.Rows[cnt + 2].Cells[4].Paragraphs[0].Append(model.PC.GoodsList[(i + ceiling_pc)].PC_NAME.TONotNullString()).FontSize(8).Font("新細明體").Alignment = Alignment.left;
                                    tb.Rows[cnt + 2].Cells[5].Paragraphs[0].Append(model.PC.GoodsList[(i + ceiling_pc)].PC_ENAME.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
                                    tb.Rows[cnt + 2].Cells[6].Paragraphs[0].Append(model.PC.GoodsList[(i + ceiling_pc)].PC_CONT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.right;
                                    tb.Rows[cnt + 2].Cells[7].Paragraphs[0].Append(model.PC.GoodsList[(i + ceiling_pc)].PC_UNIT.TONotNullString()).FontSize(8).Font("Times New Roman").Alignment = Alignment.left;
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

    }
}
