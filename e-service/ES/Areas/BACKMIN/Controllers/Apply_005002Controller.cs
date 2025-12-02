using ES.Areas.Admin.Controllers;
using ES.Areas.BACKMIN.Models;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spire.Doc;
using Spire.Doc.Documents;
using System.Globalization;
using ES.Commons;
using Spire.Doc.Fields;
using ES.Services;

namespace ES.Areas.Admin.Controllers
{
    /// <summary>
    /// 後台外銷證明書
    /// </summary>
    public class Apply_005002Controller : BaseController
    {
        //
        // GET: /BACKMIN/Apply_005002/
        public ActionResult Index(string appid)
        {
            return RedirectToAction("Apply", new { id = "Entry", APP_ID = appid });
        }

        [HttpGet]
        public ActionResult Apply(string id)
        {
            FormValueProvider vp = new FormValueProvider(this.ControllerContext);
            QueryStringValueProvider qs = new QueryStringValueProvider(this.ControllerContext);
            Apply_005002ViewModel model = new Apply_005002ViewModel();
            ActionResult result = null;
            ValueProviderCollection collection = new ValueProviderCollection();
            collection.Add(vp);
            collection.Add(qs);

            try
            {
                switch (id)
                {
                    case "Entry":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetApplyData();
                            model.ResetErrataList();
                            result = View("./Index", model);
                        }
                        break;
                    case "ErrataGrid":  // 通知補件
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetApplyData();
                            result = View("./Index", model);
                        }
                        break;
                    case "LogGrid":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetApplyData();
                            result = View("./Index", model);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error("005002_Apply failed:" + ex.TONotNullString());
                result = HttpNotFound();
            }

            return result;
        }

        [HttpPost]
        public ActionResult ApplyPost(string id)
        {
            if (string.IsNullOrEmpty(id)) { id = "Entry"; }

            FormValueProvider vp = new FormValueProvider(this.ControllerContext);
            QueryStringValueProvider qs = new QueryStringValueProvider(this.ControllerContext);
            HttpFileCollectionValueProvider fp = new HttpFileCollectionValueProvider(this.ControllerContext);
            Apply_005002ViewModel model = new Apply_005002ViewModel();
            ActionResult result = null;
            ValueProviderCollection collection = new ValueProviderCollection();
            collection.Add(vp);
            collection.Add(qs);
            collection.Add(fp);

            try
            {
                switch (id)
                {
                    case "GetApplyData":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetApplyData();
                            if (!model.Apply.APP_TIME.HasValue)
                            {
                                model.Apply.APP_TIME = DateTime.Now;
                            }
                            result = Json(model);
                        }
                        break;
                    case "Save":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.BindData();
                            model.SaveApply(this.ModelState);
                            result = Json(new { status = true, message = "" });
                        }
                        break;
                    case "Export":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetApplyData();
                            this.PreviewApplyForm(model);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                result = Json(new { status = false, message = ex.Message });
            }

            return result;
        }

        public ActionResult ApplyDoc(string id)
        {

            if (string.IsNullOrEmpty(id)) { id = "Entry"; }

            FormValueProvider vp = new FormValueProvider(this.ControllerContext);
            QueryStringValueProvider qs = new QueryStringValueProvider(this.ControllerContext);
            Apply_005002ViewModel model = new Apply_005002ViewModel();
            ActionResult result = null;

            try
            {
                switch (id)
                {
                    case "Entry":   // 功能進入點
                        if (this.TryUpdateModel(model, qs))
                        {
                            model.GetApplyData();
                            string userNo = SessionModel.Get().UserInfo.UserNo;
                            // 判斷使用者是否為案件所有人
                            if (model.Apply != null && model.Apply.PRO_ACC == userNo)
                            {
                                result = View("Index", model);
                            }
                            else
                            {
                                result = RedirectToAction("Index", "Login");
                            }
                        }
                        break;

                    default:
                        result = HttpNotFound();
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error("005002_ApplyDoc failed:" + ex.TONotNullString());
                result = HttpNotFound();
            }

            return result;
        }

        public ActionResult DownloadFile(string APP_ID, int? FILE_NO)
        {
            if (string.IsNullOrEmpty(APP_ID) || !FILE_NO.HasValue || FILE_NO < 1 || FILE_NO > 5)
            {
                return HttpNotFound();
            }

            ShareDAO dao = new ShareDAO();
            byte[] ba = null;
            ActionResult result;
            string path = dao.GetServerLocalPath();
            string filename = null;
            string contentType = null;

            try
            {

                Apply_FileModel file = dao.GetRow(new Apply_FileModel { APP_ID = APP_ID, FILE_NO = FILE_NO });

                if (file != null)
                {
                    var fileExt = Path.GetExtension(file.FILENAME);

                    switch (fileExt)
                    {
                        case ".pdf":
                            contentType = "application/pdf";
                            break;
                        case ".jpg":
                        case ".jpeg":
                            contentType = "image/jpeg";
                            break;
                        case ".bmp":
                            contentType = "image/bmp";
                            break;
                        case ".png":
                            contentType = "image/png";
                            break;
                        case ".gif":
                            contentType = "image/gif";
                            break;
                        default:
                            contentType = "application/octet-stream";
                            break;
                    }

                    string fullpath = (path + file.FILENAME).TONotNullString().Replace("\\\\", "\\");
                    using (FileStream fs = System.IO.File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (BufferedStream bs = new BufferedStream(fs))
                    {
                        ba = new byte[fs.Length];
                        bs.Read(ba, 0, (int)fs.Length);
                    }

                    filename = Path.GetFileName(fullpath);
                }

                this.Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", filename);

                result = File(ba, contentType);
            }
            catch (Exception ex)
            {
                logger.Debug("005002_DownloadFile failed:" + ex.TONotNullString());
                result = HttpNotFound();
            }

            return result;
        }

        [HttpPost]
        public void PreviewApplyForm(Apply_005002ViewModel vm)
        {
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ShareCodeListModel scm = new ShareCodeListModel();
                IList<SelectListItem> pList = scm.PList;

                Document doc = new Document();
                Section s = doc.AddSection();
                Paragraph para1 = s.AddParagraph();
                para1.AppendText("中   華   民   國   衛   生   福   利   部");
                Paragraph para2 = s.AddParagraph();
                para2.AppendText("MINISTRY OF HEALTH AND WELFARE, THE EXECUTIVE YUAN" + "\r\n" + "REPUBLIC OF CHINA");
                Paragraph para3 = s.AddParagraph();
                para3.AppendText("Date:____________                                                                           No:____________");
                Paragraph para4 = s.AddParagraph();
                para4.AppendText("證  明  書");
                Paragraph para5 = s.AddParagraph();
                para5.AppendText("Certificate").CharacterFormat.UnderlineStyle = UnderlineStyle.Single;
                Paragraph para6 = s.AddParagraph();
                para6.AppendText("\r\n茲證明下述藥品經衛生福利部核准許可登記，准予外銷。");
                Paragraph para12 = s.AddParagraph();
                para12.AppendText("Ministry of Health and Welfare , The Executive Yuan of the Republic of China\r\n" +
                                  "hereby certifies that the product as described below is subject to its \r\n" +
                                  "jurisdiction and is legally approved for exportation.\r\n");

                if (!string.IsNullOrEmpty(vm.Detail.DRUG_ABROAD_NAME))
                {
                    //加入table
                    Spire.Doc.Table sTable = s.AddTable(true);
                    sTable.ResetCells(14, 6);
                    //合併表格
                    sTable.ApplyHorizontalMerge(0, 0, 5);
                    sTable.ApplyHorizontalMerge(1, 0, 5);
                    sTable.ApplyHorizontalMerge(2, 0, 5);
                    sTable.ApplyHorizontalMerge(3, 0, 5);
                    sTable.ApplyHorizontalMerge(4, 0, 5);
                    sTable.ApplyHorizontalMerge(5, 0, 5);

                    sTable.ApplyHorizontalMerge(6, 0, 3);
                    sTable.ApplyHorizontalMerge(7, 0, 3);
                    sTable.ApplyHorizontalMerge(8, 0, 3);
                    sTable.ApplyHorizontalMerge(9, 0, 3);
                    sTable.ApplyHorizontalMerge(10, 0, 3);
                    sTable.ApplyHorizontalMerge(11, 0, 3);
                    sTable.ApplyHorizontalMerge(12, 0, 3);
                    sTable.ApplyHorizontalMerge(13, 0, 3);
                    //邊框隱藏
                    Spire.Doc.Table table = (Spire.Doc.Table)s.Tables[0];
                    table.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;

                    //表格下邊框設定
                    for (var i = 0; i < 6; i++)
                    {
                        Spire.Doc.TableCell cell1 = sTable[1, i];
                        Spire.Doc.TableCell cell2 = sTable[4, i];
                        Spire.Doc.TableCell cell3 = sTable[7, i];
                        Spire.Doc.TableCell cell4 = sTable[10, i];
                        Spire.Doc.TableCell cell5 = sTable[13, i];
                        cell1.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                        cell2.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                        cell3.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                        cell4.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                        cell5.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                    }

                    //空行設定
                    Spire.Doc.Fields.TextRange rangeNull1 = sTable[2, 0].AddParagraph().AppendText(" ");
                    rangeNull1.CharacterFormat.FontSize = 8;
                    Spire.Doc.Fields.TextRange rangeNull2 = sTable[5, 0].AddParagraph().AppendText(" ");
                    rangeNull2.CharacterFormat.FontSize = 8;
                    Spire.Doc.Fields.TextRange rangeNull3 = sTable[8, 0].AddParagraph().AppendText(" ");
                    rangeNull3.CharacterFormat.FontSize = 8;
                    Spire.Doc.Fields.TextRange rangeNull4 = sTable[11, 0].AddParagraph().AppendText(" ");
                    rangeNull4.CharacterFormat.FontSize = 8;

                    //製造廠名稱(中文)
                    Paragraph paraCnt = sTable[0, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range1 = paraCnt.AppendText("製造廠名稱：" + vm.Detail.MF_CNT_NAME);
                    range1.CharacterFormat.FontName = "新細明體";
                    range1.CharacterFormat.FontSize = 8;
                    paraCnt.Format.FirstLineIndent = -48f;
                    paraCnt.Format.LeftIndent = 48f;

                    //製造廠名稱(英文)
                    Paragraph paraMeasure = sTable[1, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range2 = paraMeasure.AppendText("Manufacturer: " + vm.Detail.MF_CNT_NAME_E);
                    range2.CharacterFormat.FontName = "Times New Roman";
                    range2.CharacterFormat.FontSize = 8;
                    paraMeasure.Format.FirstLineIndent = -48f;   // 首行凸排
                    paraMeasure.Format.LeftIndent = 48f;         // 縮排

                    //製造廠地址(中文)
                    Paragraph paraCMeasure = sTable[3, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range3 = paraCMeasure.AppendText("製造廠地址：" + "中華民國台灣" + vm.Detail.MF_ADDR);
                    range3.CharacterFormat.FontName = "新細明體";
                    range3.CharacterFormat.FontSize = 8;
                    paraCMeasure.Format.FirstLineIndent = -48f;   // 首行凸排
                    paraCMeasure.Format.LeftIndent = 48f;         // 縮排

                    //製造廠地址(英文)
                    Paragraph paraLocation = sTable[4, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range4 = paraLocation.AppendText("Manufacturing Plant Location: " + vm.Detail.MF_ADDR_E);
                    range4.CharacterFormat.FontName = "Times New Roman";
                    range4.CharacterFormat.FontSize = 8;
                    paraLocation.Format.FirstLineIndent = -100f; // 首行凸排
                    paraLocation.Format.LeftIndent = 100f;       // 縮排

                    //藥品名稱(中文)
                    Paragraph paraDrug = sTable[6, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range5 = paraDrug.AppendText("藥品名稱：" + vm.Detail.DRUG_NAME);
                    range5.CharacterFormat.FontName = "新細明體";
                    range5.CharacterFormat.FontSize = 8;
                    paraDrug.Format.FirstLineIndent = -40.85f; // 首行凸排
                    paraDrug.Format.LeftIndent = 40.85f;       // 縮排

                    //藥品名稱(英文)
                    Paragraph paraProdName = sTable[7, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range6 = paraProdName.AppendText("Product Name: " + vm.Detail.DRUG_NAME_E);
                    range6.CharacterFormat.FontName = "Times New Roman";
                    range6.CharacterFormat.FontSize = 8;
                    paraProdName.Format.FirstLineIndent = -50f;
                    paraProdName.Format.LeftIndent = 50f;

                    sTable.ApplyHorizontalMerge(6, 4, 5);
                    //劑型(中文)
                    Paragraph paraCDosage = sTable[6, 4].AddParagraph();
                    Spire.Doc.Fields.TextRange range7 = paraCDosage.AppendText("劑型：" + vm.Detail.DOSAGE_FORM);
                    range7.CharacterFormat.FontName = "新細明體";
                    range7.CharacterFormat.FontSize = 8;
                    paraCDosage.Format.FirstLineIndent = -24.57f;
                    paraCDosage.Format.LeftIndent = 24.57f;

                    sTable.ApplyHorizontalMerge(7, 4, 5);
                    //劑型(英文)
                    Paragraph paraDosage = sTable[7, 4].AddParagraph();
                    Spire.Doc.Fields.TextRange range9 = paraDosage.AppendText("Dosage Form: " + vm.Detail.DOSAGE_FORM_E);
                    range9.CharacterFormat.FontName = "Times New Roman";
                    range9.CharacterFormat.FontSize = 8;
                    paraDosage.Format.FirstLineIndent = -47.42f;
                    paraDosage.Format.LeftIndent = 47.42f;

                    //外銷品名(中文)
                    Paragraph paraAbroad = sTable[9, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range23 = paraAbroad.AppendText("外銷品名：" + vm.Detail.DRUG_ABROAD_NAME);
                    range23.CharacterFormat.FontName = "新細明體";
                    range23.CharacterFormat.FontSize = 8;
                    paraAbroad.Format.FirstLineIndent = -39.42f;
                    paraAbroad.Format.LeftIndent = 39.42f;

                    //外銷品名(英文)
                    Paragraph paraExport = sTable[10, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range25 = paraExport.AppendText("Export Name: " + vm.Detail.DRUG_ABROAD_NAME_E);
                    range25.CharacterFormat.FontName = "Times New Roman";
                    range25.CharacterFormat.FontSize = 8;
                    paraExport.Format.FirstLineIndent = -47f;
                    paraExport.Format.LeftIndent = 47f;

                    if (vm.Detail.EXPIR_DATE.HasValue)
                    {
                        sTable.ApplyHorizontalMerge(9, 0, 3);
                        sTable.ApplyHorizontalMerge(9, 4, 5);

                        //有效日期(中文)
                        var dateTW = HelperUtil.TransToTwYear(vm.Detail.EXPIR_DATE).Split('/');
                        string strDateTw = dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日";
                        Spire.Doc.Fields.TextRange range15 = sTable[9, 4].AddParagraph().AppendText("有效日期：" + strDateTw);
                        range15.CharacterFormat.FontName = "新細明體";
                        range15.CharacterFormat.FontSize = 8;

                        sTable.ApplyHorizontalMerge(10, 0, 3);
                        sTable.ApplyHorizontalMerge(10, 4, 5);

                        //有效日期(英文)
                        string strDateEn = Convert.ToDateTime(vm.Detail.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
                        Spire.Doc.Fields.TextRange range17 = sTable[10, 4].AddParagraph().AppendText("Date of Issue: " + strDateEn);
                        range17.CharacterFormat.FontName = "Times New Roman";
                        range17.CharacterFormat.FontSize = 8;
                    }
                    else
                    {
                        sTable.ApplyHorizontalMerge(9, 0, 5);
                        sTable.ApplyHorizontalMerge(10, 0, 5);
                    }

                    //許可證字號(中文)
                    var codeItem = pList.Where(x => x.Value == vm.Detail.LIC_CD).FirstOrDefault();
                    string licText = string.Empty;
                    if (codeItem != null)
                    {
                        licText = codeItem.Text + "第" + vm.Detail.LIC_NUM + "號";
                    }
                    else
                    {
                        licText = vm.Detail.LIC_CD + "第" + vm.Detail.LIC_NUM + "號";
                    }
                    Spire.Doc.Fields.TextRange range11 = sTable[12, 0].AddParagraph().AppendText("許可證字號：" + licText);
                    range11.CharacterFormat.FontName = "新細明體";
                    range11.CharacterFormat.FontSize = 8;
                    // Spire.Doc.Fields.TextRange range12 = sTable[12, 1].AddParagraph().AppendText(vm.Detail.LIC_CD + "第" + vm.Detail.LIC_NUM + "號");
                    // range12.CharacterFormat.FontName = "新細明體";
                    // range12.CharacterFormat.FontSize = 8;

                    //許可證字號(英文)
                    string liccdEn = vm.Detail.LIC_CD_E + "-" + vm.Detail.LIC_NUM_E;
                    sTable[13, 0].Width = 100f;
                    Spire.Doc.Fields.TextRange range13 = sTable[13, 0].AddParagraph().AppendText("Registration Number: " + liccdEn);
                    range13.CharacterFormat.FontName = "Times New Roman";
                    range13.CharacterFormat.FontSize = 8;
                    //Spire.Doc.Fields.TextRange range14 = sTable[13, 1].AddParagraph().AppendText(vm.Detail.LIC_CD_E + "-" + vm.Detail.LIC_NUM_E);
                    //range14.CharacterFormat.FontName = "Times New Roman";
                    //range14.CharacterFormat.FontSize = 8;

                    sTable.ApplyHorizontalMerge(12, 4, 5);
                    //核准日期(中文)
                    var dateTW1 = HelperUtil.TransToTwYear(vm.Detail.ISSUE_DATE).Split('/');
                    string strDateTw1 = dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日";
                    Spire.Doc.Fields.TextRange range19 = sTable[12, 4].AddParagraph().AppendText("核准日期：" + strDateTw1);
                    range19.CharacterFormat.FontName = "新細明體";
                    range19.CharacterFormat.FontSize = 8;

                    sTable.ApplyHorizontalMerge(13, 4, 5);
                    //核准日期(英文)
                    string strIssueDate = (Convert.ToDateTime(vm.Detail.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
                    Spire.Doc.Fields.TextRange range21 = sTable[13, 4].AddParagraph().AppendText("Date of Issue: " + strIssueDate);
                    range21.CharacterFormat.FontName = "Times New Roman";
                    range21.CharacterFormat.FontSize = 8;
                }
                else
                {
                    //加入table
                    Spire.Doc.Table sTable = s.AddTable(true);
                    sTable.ResetCells(11, 6);
                    // IsTable.AutoFit(AutoFitBehaviorType.AutoFitToContents);

                    //合併表格
                    sTable.ApplyHorizontalMerge(0, 0, 5);
                    sTable.ApplyHorizontalMerge(1, 0, 5);
                    sTable.ApplyHorizontalMerge(2, 0, 5);
                    sTable.ApplyHorizontalMerge(3, 0, 5);
                    sTable.ApplyHorizontalMerge(4, 0, 5);
                    sTable.ApplyHorizontalMerge(5, 0, 5);
                    sTable.ApplyHorizontalMerge(6, 0, 3);
                    sTable.ApplyHorizontalMerge(6, 4, 5);
                    sTable.ApplyHorizontalMerge(7, 0, 3);
                    sTable.ApplyHorizontalMerge(7, 4, 5);
                    sTable.ApplyHorizontalMerge(8, 0, 5);

                    //sTable.ApplyHorizontalMerge(9, 0, 3);
                    //sTable.ApplyHorizontalMerge(9, 4, 5);
                    //sTable.ApplyHorizontalMerge(10, 0, 3);
                    //sTable.ApplyHorizontalMerge(10, 4, 5);

                    //邊框隱藏
                    Spire.Doc.Table table = (Spire.Doc.Table)s.Tables[0];
                    table.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
                    table.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;

                    //表格下邊框設定
                    for (var i = 0; i < 6; i++)
                    {
                        Spire.Doc.TableCell cell1 = sTable[1, i];
                        Spire.Doc.TableCell cell2 = sTable[4, i];
                        Spire.Doc.TableCell cell3 = sTable[7, i];
                        Spire.Doc.TableCell cell4 = sTable[10, i];
                        cell1.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                        cell2.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                        cell3.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                        cell4.CellFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.Single;
                    }

                    //空行設定
                    Spire.Doc.Fields.TextRange rangeNull1 = sTable[2, 0].AddParagraph().AppendText(" ");
                    rangeNull1.CharacterFormat.FontSize = 8;
                    Spire.Doc.Fields.TextRange rangeNull2 = sTable[5, 0].AddParagraph().AppendText(" ");
                    rangeNull2.CharacterFormat.FontSize = 8;
                    Spire.Doc.Fields.TextRange rangeNull3 = sTable[8, 0].AddParagraph().AppendText(" ");
                    rangeNull3.CharacterFormat.FontSize = 8;

                    //製造廠名稱(中文)
                    Paragraph paraCnt = sTable[0, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range1 = paraCnt.AppendText("製造廠名稱：" + vm.Detail.MF_CNT_NAME);
                    range1.CharacterFormat.FontName = "新細明體";
                    range1.CharacterFormat.FontSize = 8;
                    paraCnt.Format.FirstLineIndent = -48f; // 首行凸排
                    paraCnt.Format.LeftIndent = 48f;       // 縮排

                    //製造廠名稱(英文)
                    Paragraph paraMeasure = sTable[1, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range2 = paraMeasure.AppendText("Manufacturer: " + vm.Detail.MF_CNT_NAME_E);
                    range2.CharacterFormat.FontName = "Times New Roman";
                    range2.CharacterFormat.FontSize = 8;
                    paraMeasure.Format.FirstLineIndent = -48f; // 首行凸排
                    paraMeasure.Format.LeftIndent = 48f;       // 縮排

                    //製造廠地址(中文)
                    Paragraph paraCMeasure = sTable[3, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range3 = paraCMeasure.AppendText("製造廠地址：" + "中華民國台灣" + vm.Detail.MF_ADDR);
                    range3.CharacterFormat.FontName = "新細明體";
                    range3.CharacterFormat.FontSize = 8;
                    paraCMeasure.Format.FirstLineIndent = -48f; // 首行凸排
                    paraCMeasure.Format.LeftIndent = 48f;       // 縮排

                    //製造廠地址(英文)
                    Paragraph paraLocation = sTable[4, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range4 = paraLocation.AppendText("Manufacturing Plant Location: " + vm.Detail.MF_ADDR_E);
                    range4.CharacterFormat.FontName = "Times New Roman";
                    range4.CharacterFormat.FontSize = 8;
                    paraLocation.Format.FirstLineIndent = -100f; // 首行凸排
                    paraLocation.Format.LeftIndent = 100f;       // 縮排

                    //藥品名稱(中文)
                    Paragraph paraDrug = sTable[6, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range5 = paraDrug.AppendText("藥品名稱：" + vm.Detail.DRUG_NAME);
                    range5.CharacterFormat.FontName = "新細明體";
                    range5.CharacterFormat.FontSize = 8;
                    paraDrug.Format.FirstLineIndent = -40.85f; // 首行凸排
                    paraDrug.Format.LeftIndent = 40.85f;       // 縮排

                    //藥品名稱(英文)
                    Paragraph paraProdName = sTable[7, 0].AddParagraph();
                    Spire.Doc.Fields.TextRange range6 = paraProdName.AppendText("Product Name: " + vm.Detail.DRUG_NAME_E);
                    range6.CharacterFormat.FontName = "Times New Roman";
                    range6.CharacterFormat.FontSize = 8;
                    paraProdName.Format.FirstLineIndent = -50f;
                    paraProdName.Format.LeftIndent = 50f;

                    //劑型(中文)
                    Paragraph paraCDosage = sTable[6, 4].AddParagraph();
                    Spire.Doc.Fields.TextRange range7 = paraCDosage.AppendText("劑型：" + vm.Detail.DOSAGE_FORM);
                    range7.CharacterFormat.FontName = "新細明體";
                    range7.CharacterFormat.FontSize = 8;
                    paraCDosage.Format.FirstLineIndent = -24.57f;
                    paraCDosage.Format.LeftIndent = 24.57f;
                    //Spire.Doc.Fields.TextRange range8 = sTable[6, 5].AddParagraph().AppendText(vm.Detail.DOSAGE_FORM);
                    //range8.CharacterFormat.FontName = "新細明體";
                    //range8.CharacterFormat.FontSize = 8;

                    //劑型(英文)
                    Paragraph paraDosage = sTable[7, 4].AddParagraph();
                    Spire.Doc.Fields.TextRange range9 = paraDosage.AppendText("Dosage Form: " + vm.Detail.DOSAGE_FORM_E);
                    range9.CharacterFormat.FontName = "Times New Roman";
                    range9.CharacterFormat.FontSize = 8;
                    paraDosage.Format.FirstLineIndent = -47.42f;
                    paraDosage.Format.LeftIndent = 47.42f;
                    //Spire.Doc.Fields.TextRange range10 = sTable[7, 5].AddParagraph().AppendText(vm.Detail.DOSAGE_FORM_E);
                    //range10.CharacterFormat.FontName = "Times New Roman";
                    //range10.CharacterFormat.FontSize = 8;

                    //許可證字號(中文)
                    var codeItem = pList.Where(x => x.Value == vm.Detail.LIC_CD).FirstOrDefault();
                    string licText = string.Empty;
                    if (codeItem != null)
                    {
                        licText = codeItem.Text + "第" + vm.Detail.LIC_NUM + "號";
                    }
                    else
                    {
                        licText = vm.Detail.LIC_CD + "第" + vm.Detail.LIC_NUM + "號";
                    }

                    Spire.Doc.Fields.TextRange range11 = sTable[9, 0].AddParagraph().AppendText("許可證字號：" + licText);
                    range11.CharacterFormat.FontName = "新細明體";
                    range11.CharacterFormat.FontSize = 8;
                    //Spire.Doc.Fields.TextRange range12 = sTable[9, 1].AddParagraph().AppendText(vm.Detail.LIC_CD + "第" + vm.Detail.LIC_NUM + "號");
                    //range12.CharacterFormat.FontName = "新細明體";
                    //range12.CharacterFormat.FontSize = 8;

                    //許可證字號(英文)
                    string liccdEn = vm.Detail.LIC_CD_E + "-" + vm.Detail.LIC_NUM_E;
                    //foreach (TableRow row in sTable.Rows)
                    //{
                    //    row.Cells[0].Width = 100f;
                    //}
                    sTable[10, 0].Width = 100f;
                    Spire.Doc.Fields.TextRange range13 = sTable[10, 0].AddParagraph().AppendText("Registration Number: " + liccdEn);
                    range13.CharacterFormat.FontName = "Times New Roman";
                    range13.CharacterFormat.FontSize = 8;
                    
                    //Spire.Doc.Fields.TextRange range14 = sTable[10, 1].AddParagraph().AppendText(vm.Detail.LIC_CD_E + "-" + vm.Detail.LIC_NUM_E);
                    //range14.CharacterFormat.FontName = "Times New Roman";
                    //range14.CharacterFormat.FontSize = 8;

                    if (vm.Detail.EXPIR_DATE.HasValue)
                    {
                        sTable.ApplyHorizontalMerge(9, 0, 1);
                        sTable.ApplyHorizontalMerge(9, 2, 3);
                        sTable.ApplyHorizontalMerge(9, 4, 5);

                        //有效日期(中文)
                        var dateTW = HelperUtil.TransToTwYear(vm.Detail.EXPIR_DATE).Split('/');
                        string strDateTw = (dateTW[0] + "年" + dateTW[1] + "月" + dateTW[2] + "日");
                        Spire.Doc.Fields.TextRange range15 = sTable[9, 2].AddParagraph().AppendText("  有效日期：" + strDateTw);
                        range15.CharacterFormat.FontName = "新細明體";
                        range15.CharacterFormat.FontSize = 8;

                        sTable.ApplyHorizontalMerge(10, 0, 1);
                        sTable.ApplyHorizontalMerge(10, 2, 3);
                        sTable.ApplyHorizontalMerge(10, 4, 5);

                        //有效日期(英文)
                        string strExpirDate = (Convert.ToDateTime(vm.Detail.EXPIR_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
                        Spire.Doc.Fields.TextRange range17 = sTable[10, 2].AddParagraph().AppendText("  Date of Issue: " + strExpirDate);
                        range17.CharacterFormat.FontName = "Times New Roman";
                        range17.CharacterFormat.FontSize = 8;
                    }
                    else
                    {
                        sTable.ApplyHorizontalMerge(9, 0, 3);
                        sTable.ApplyHorizontalMerge(9, 4, 5);
                        sTable.ApplyHorizontalMerge(10, 0, 3);
                        sTable.ApplyHorizontalMerge(10, 4, 5);
                    }

                    //核准日期(中文)
                    var dateTW1 = HelperUtil.TransToTwYear(vm.Detail.ISSUE_DATE).Split('/');
                    string dateTw1 = (dateTW1[0] + "年" + dateTW1[1] + "月" + dateTW1[2] + "日");
                    Spire.Doc.Fields.TextRange range19 = sTable[9, 4].AddParagraph().AppendText("核准日期：" + dateTw1);
                    range19.CharacterFormat.FontName = "新細明體";
                    range19.CharacterFormat.FontSize = 8;


                    //核准日期(英文)
                    string strIssueDate = (Convert.ToDateTime(vm.Detail.ISSUE_DATE).ToString("MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US")));
                    Spire.Doc.Fields.TextRange range21 = sTable[10, 4].AddParagraph().AppendText("Date of Issue: " + strIssueDate);
                    range21.CharacterFormat.FontName = "Times New Roman";
                    range21.CharacterFormat.FontSize = 8;
                }

                Paragraph para7 = s.AddParagraph();
                para7.AppendText("\r\n處  方：" + vm.Detail.MF_CONT);

                Paragraph para13 = s.AddParagraph();
                para13.AppendText("Formula: " + vm.Detail.MF_CONT_E);

                //加入table
                Spire.Doc.Table sTable1 = s.AddTable(true);
                var rowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(vm.IngredientList.Count) / 2));
                rowCount += Convert.ToInt32(Math.Ceiling(Convert.ToDouble(vm.ExcipientList.Count) / 2)) + 2;

                sTable1.TableFormat.LeftIndent = 28.57f;

                sTable1.ResetCells(rowCount, 8);

                int diRowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(vm.IngredientList.Count) / 2));
                int pcRowCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(vm.ExcipientList.Count) / 2));

                //邊框隱藏
                Spire.Doc.Table table1 = (Spire.Doc.Table)s.Tables[1];
                table1.TableFormat.Borders.Top.BorderType = Spire.Doc.Documents.BorderStyle.None;
                table1.TableFormat.Borders.Left.BorderType = Spire.Doc.Documents.BorderStyle.None;
                table1.TableFormat.Borders.Right.BorderType = Spire.Doc.Documents.BorderStyle.None;
                table1.TableFormat.Borders.Bottom.BorderType = Spire.Doc.Documents.BorderStyle.None;
                table1.TableFormat.Borders.Horizontal.BorderType = Spire.Doc.Documents.BorderStyle.None;
                table1.TableFormat.Borders.Vertical.BorderType = Spire.Doc.Documents.BorderStyle.None;

                for (var i = 0; i < Convert.ToInt32(Math.Ceiling(Convert.ToDouble(vm.IngredientList.Count) / 2)); i++)
                {
                    if (vm.IngredientList != null && i < vm.IngredientList.Count)
                    {
                        Spire.Doc.Fields.TextRange rangeN1 =
                            sTable1[i, 0].AddParagraph().AppendText(vm.IngredientList[i].DI_NAME);
                        rangeN1.CharacterFormat.FontName = "新細明體";
                        rangeN1.CharacterFormat.FontSize = 8;

                        Spire.Doc.Fields.TextRange rangeN2 =
                            sTable1[i, 1].AddParagraph().AppendText(vm.IngredientList[i].DI_ENAME);
                        rangeN2.CharacterFormat.FontName = "Times New Roman";
                        rangeN2.CharacterFormat.FontSize = 8;

                        var parN3 = sTable1[i, 2].AddParagraph();
                        parN3.Format.HorizontalAlignment = HorizontalAlignment.Right;
                        Spire.Doc.Fields.TextRange rangeN3 =
                            parN3.AppendText(vm.IngredientList[i].DI_CONT);
                        rangeN3.CharacterFormat.FontName = "Times New Roman";
                        rangeN3.CharacterFormat.FontSize = 8;

                        Spire.Doc.Fields.TextRange rangeN4 =
                            sTable1[i, 3].AddParagraph().AppendText(vm.IngredientList[i].DI_UNIT);
                        rangeN4.CharacterFormat.FontName = "Times New Roman";
                        rangeN4.CharacterFormat.FontSize = 8;

                        if ((diRowCount + i) < vm.IngredientList.Count)
                        {
                            Spire.Doc.Fields.TextRange rangeN5 =
                                sTable1[i, 4].AddParagraph().AppendText(vm.IngredientList[diRowCount + i].DI_NAME);
                            rangeN5.CharacterFormat.FontName = "新細明體";
                            rangeN5.CharacterFormat.FontSize = 8;

                            Spire.Doc.Fields.TextRange rangeN6 =
                                sTable1[i, 5].AddParagraph().AppendText(vm.IngredientList[diRowCount + i].DI_ENAME);
                            rangeN6.CharacterFormat.FontName = "Times New Roman";
                            rangeN6.CharacterFormat.FontSize = 8;

                            var parN7 = sTable1[i, 6].AddParagraph();
                            parN7.Format.HorizontalAlignment = HorizontalAlignment.Right;
                            Spire.Doc.Fields.TextRange rangeN7 =
                                parN7.AppendText(vm.IngredientList[diRowCount + i].DI_CONT);
                            rangeN7.CharacterFormat.FontName = "Times New Roman";
                            rangeN7.CharacterFormat.FontSize = 8;

                            Spire.Doc.Fields.TextRange rangeN8 =
                                sTable1[i, 7].AddParagraph().AppendText(vm.IngredientList[diRowCount + i].DI_UNIT);
                            rangeN8.CharacterFormat.FontName = "Times New Roman";
                            rangeN8.CharacterFormat.FontSize = 8;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(vm.Detail.PC_SCALE_1))
                {
                    var rowCount1 = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(vm.IngredientList.Count) / 2));
                    sTable1.ApplyHorizontalMerge(rowCount1, 0, 7);
                    sTable1.ApplyHorizontalMerge(rowCount1 + 1, 0, 7);

                    Paragraph parScale = sTable1[rowCount1, 0].AddParagraph();

                    Spire.Doc.Fields.TextRange rangeScale3 = parScale.AppendText("以上生藥製成浸膏");
                    rangeScale3.CharacterFormat.FontName = "新細明體";
                    rangeScale3.CharacterFormat.FontSize = 8;

                    Spire.Doc.Fields.TextRange rangeScale1 = parScale.AppendText(vm.Detail.PC_SCALE_1 + vm.Detail.PC_SCALE_1E);
                    rangeScale1.CharacterFormat.FontName = "Times New Roman";
                    rangeScale1.CharacterFormat.FontSize = 8;

                    Spire.Doc.Fields.TextRange rangeScale31 = parScale.AppendText(" (生藥與浸膏比例");
                    rangeScale31.CharacterFormat.FontName = "新細明體";
                    rangeScale31.CharacterFormat.FontSize = 8;

                    Spire.Doc.Fields.TextRange rangeScale2 = parScale.AppendText(vm.Detail.PC_SCALE_21 + ":" + vm.Detail.PC_SCALE_22 + "=" + vm.Detail.PC_SCALE_23 + ":" + vm.Detail.PC_SCALE_24);
                    rangeScale2.CharacterFormat.FontName = "Times New Roman";
                    rangeScale2.CharacterFormat.FontSize = 8;

                    Spire.Doc.Fields.TextRange rangeScale32 = parScale.AppendText(")");
                    rangeScale32.CharacterFormat.FontName = "新細明體";
                    rangeScale32.CharacterFormat.FontSize = 8;

                    //var rangeN9Text = "以上生藥製成浸膏" + vm.Detail.PC_SCALE_1 + vm.Detail.PC_SCALE_1E +
                    //  "(生藥與浸膏比例" + vm.Detail.PC_SCALE_21 + " : " + vm.Detail.PC_SCALE_22 + " = " + vm.Detail.PC_SCALE_23 + " : " + vm.Detail.PC_SCALE_24 + ")";

                    //Spire.Doc.Fields.TextRange rangeN9 = sTable1[rowCount1, 0].AddParagraph().AppendText(rangeN9Text);
                    //rangeN9.CharacterFormat.FontName = "新細明體";
                    //rangeN9.CharacterFormat.FontSize = 8;

                    var rangeN9Text = "Above raw herbs equivalent to " + vm.Detail.PC_SCALE_1 + vm.Detail.PC_SCALE_2E +
                                  " herbal extract.(Ratio of raw herbs and extract " + vm.Detail.PC_SCALE_21 + ":" + vm.Detail.PC_SCALE_22 + "=" + vm.Detail.PC_SCALE_23 + ":" + vm.Detail.PC_SCALE_24 + ")";

                    Spire.Doc.Fields.TextRange rangeN10 = sTable1[rowCount1 + 1, 0].AddParagraph().AppendText(rangeN9Text);
                    rangeN10.CharacterFormat.FontName = "Times New Roman";
                    rangeN10.CharacterFormat.FontSize = 8;

                    var j = 0;
                    for (var i = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(vm.IngredientList.Count) / 2)) + 2; i < rowCount; i++)
                    {
                        if (vm.ExcipientList != null && j < vm.ExcipientList.Count)
                        {
                            Spire.Doc.Fields.TextRange rangeN1 =
                                sTable1[i, 0].AddParagraph().AppendText(vm.ExcipientList[j].PC_NAME);
                            rangeN1.CharacterFormat.FontName = "新細明體";
                            rangeN1.CharacterFormat.FontSize = 8;

                            Spire.Doc.Fields.TextRange rangeN2 =
                                sTable1[i, 1].AddParagraph().AppendText(vm.ExcipientList[j].PC_ENAME);
                            rangeN2.CharacterFormat.FontName = "Times New Roman";
                            rangeN2.CharacterFormat.FontSize = 8;

                            var parN3 = sTable1[i, 2].AddParagraph();
                            parN3.Format.HorizontalAlignment = HorizontalAlignment.Right;
                            Spire.Doc.Fields.TextRange rangeN3 =
                                parN3.AppendText(vm.ExcipientList[j].PC_CONT);
                            rangeN3.CharacterFormat.FontName = "Times New Roman";
                            rangeN3.CharacterFormat.FontSize = 8;

                            Spire.Doc.Fields.TextRange rangeN4 =
                                sTable1[i, 3].AddParagraph().AppendText(vm.ExcipientList[j].PC_UNIT);
                            rangeN4.CharacterFormat.FontName = "Times New Roman";
                            rangeN4.CharacterFormat.FontSize = 8;

                            if ((pcRowCount + j) < vm.ExcipientList.Count)
                            {
                                Spire.Doc.Fields.TextRange rangeN5 =
                                    sTable1[i, 4].AddParagraph().AppendText(vm.ExcipientList[pcRowCount + j].PC_NAME);
                                rangeN5.CharacterFormat.FontName = "新細明體";
                                rangeN5.CharacterFormat.FontSize = 8;

                                Spire.Doc.Fields.TextRange rangeN6 =
                                    sTable1[i, 5].AddParagraph().AppendText(vm.ExcipientList[pcRowCount + j].PC_ENAME);
                                rangeN6.CharacterFormat.FontName = "Times New Roman";
                                rangeN6.CharacterFormat.FontSize = 8;

                                var parN7 = sTable1[i, 6].AddParagraph();
                                parN7.Format.HorizontalAlignment = HorizontalAlignment.Right;
                                Spire.Doc.Fields.TextRange rangeN7 =
                                    parN7.AppendText(vm.ExcipientList[pcRowCount + j].PC_CONT);
                                rangeN7.CharacterFormat.FontName = "Times New Roman";
                                rangeN7.CharacterFormat.FontSize = 8;

                                Spire.Doc.Fields.TextRange rangeN8 =
                                    sTable1[i, 7].AddParagraph().AppendText(vm.ExcipientList[pcRowCount + j].PC_UNIT);
                                rangeN8.CharacterFormat.FontName = "Times New Roman";
                                rangeN8.CharacterFormat.FontSize = 8;
                            }
                        }

                        j++;
                    }
                }

                ParagraphStyle style1 = new ParagraphStyle(doc);
                style1.Name = "titleStyle";
                style1.CharacterFormat.FontName = "新細明體";
                style1.CharacterFormat.FontSize = 12;
                doc.Styles.Add(style1);
                para1.ApplyStyle("titleStyle");
                para1.Format.HorizontalAlignment = HorizontalAlignment.Center;

                ParagraphStyle style2 = new ParagraphStyle(doc);
                style2.Name = "titleStyle2";
                style2.CharacterFormat.FontName = "Times New Roman";
                style2.CharacterFormat.FontSize = 12;
                doc.Styles.Add(style2);
                para2.ApplyStyle("titleStyle2");
                para2.Format.HorizontalAlignment = HorizontalAlignment.Center;

                ParagraphStyle style3 = new ParagraphStyle(doc);
                style3.Name = "titleStyle3";
                style3.CharacterFormat.Bold = true;
                style3.CharacterFormat.FontName = "新細明體";
                style3.CharacterFormat.FontSize = 12;
                doc.Styles.Add(style3);
                para4.ApplyStyle("titleStyle3");
                para4.Format.HorizontalAlignment = HorizontalAlignment.Center;

                ParagraphStyle style4 = new ParagraphStyle(doc);
                style4.Name = "titleStyle4";
                style4.CharacterFormat.Bold = true;
                style4.CharacterFormat.FontName = "Times New Roman";
                style4.CharacterFormat.FontSize = 12;
                doc.Styles.Add(style4);
                para5.ApplyStyle("titleStyle4");
                para5.Format.HorizontalAlignment = HorizontalAlignment.Center;

                ParagraphStyle style5 = new ParagraphStyle(doc);
                style5.Name = "titleStyle5";
                style5.CharacterFormat.FontName = "新細明體";
                style5.CharacterFormat.FontSize = 8;
                doc.Styles.Add(style5);
                para6.ApplyStyle("titleStyle5");
                para7.ApplyStyle("titleStyle5");

                ParagraphStyle style6 = new ParagraphStyle(doc);
                style6.Name = "titleStyle6";
                style6.CharacterFormat.FontName = "Times New Roman";
                style6.CharacterFormat.FontSize = 8;
                doc.Styles.Add(style6);
                para12.ApplyStyle("titleStyle6");
                para13.ApplyStyle("titleStyle6");

                ParagraphStyle style7 = new ParagraphStyle(doc);
                style7.Name = "titleStyle7";
                style7.CharacterFormat.FontName = "Times New Roman";
                style7.CharacterFormat.FontSize = 8;
                style7.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
                doc.Styles.Add(style7);

                // s.AddParagraph().AppendText("\n");

                //適應症
                if (!string.IsNullOrEmpty(vm.Detail.INDIOCATION))
                {
                    Paragraph para8 = s.AddParagraph();
                    para8.AppendText("\r\n適應症：" + vm.Detail.INDIOCATION);
                    para8.ApplyStyle("titleStyle5");
                    para8.Format.FirstLineIndent = -32.57f;
                    para8.Format.LeftIndent = 32.57f;

                    Paragraph para9 = s.AddParagraph();
                    para9.AppendText("Indication(s): " + vm.Detail.INDIOCATION_E);
                    para9.ApplyStyle("titleStyle6");

                    // 文字內容對齊標題  
                    para9.Format.FirstLineIndent = -42.8f;   // 首行凸排
                    para9.Format.LeftIndent = 42.8f;         // 縮排
                }

                // 效能
                if (!string.IsNullOrEmpty(vm.Detail.EFFICACY))
                {
                    Paragraph para10 = s.AddParagraph();
                    para10.AppendText("\r\n效能：" + vm.Detail.EFFICACY);
                    para10.ApplyStyle("titleStyle5");
                    para10.Format.FirstLineIndent = -24f;
                    para10.Format.LeftIndent = 24f;

                    Paragraph para11 = s.AddParagraph();
                    para11.AppendText("Efficacy: " + vm.Detail.EFFICACY_E);
                    para11.ApplyStyle("titleStyle6");

                    // 文字內容對齊標題  
                    para11.Format.FirstLineIndent = -28.57f;   // 首行凸排
                    para11.Format.LeftIndent = 28.57f;         // 縮排
                }

                s.AddParagraph().AppendText("\r\n");

                // 簽章區塊
                TextBox tb = s.AddParagraph().AppendTextBox(220, 120);
                tb.Format.HorizontalAlignment = ShapeHorizontalAlignment.Right;
                tb.Format.LineStyle = TextBoxLineStyle.Simple;
                tb.Format.NoLine = true;
                tb.Format.VerticalAlignment = ShapeVerticalAlignment.Bottom;
                tb.Format.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
                tb.Format.TextWrappingType = TextWrappingType.Right;


                Paragraph para14 = tb.Body.AddParagraph();
                TextRange tr = para14.AppendText("\r\nSigned by______________________________\r\n" +
                                  "                 Yi-Tsau Huang, M.D., Ph.D.\r\n" +
                                  "                 Director General\r\n" +
                                  "                 Department of Chinese Medicine and Pharmacy\r\n" +
                                  "                 on behalf of Shih - Chung Chen, D.D.S.\r\n" +
                                  "                 Minister\r\n" +
                                  "                 Ministry of Health and Welfare\r\n" +
                                  "                 The Executive Yuan, R.O.C.");
                tr.CharacterFormat.FontSize = 8;
                para14.Format.HorizontalAlignment = HorizontalAlignment.Left;


                Section sec = doc.Sections[0];
                sec.PageSetup.PageSize = PageSize.A4;

                sec.PageSetup.Margins.Top = 71.88f;
                sec.PageSetup.Margins.Bottom = 71.88f;
                sec.PageSetup.Margins.Left = 90.12f;
                sec.PageSetup.Margins.Right = 90.12f;

                //string path = Server.MapPath("~/Sample/apply005001.docx");
                //doc.SaveToFile(path, FileFormat.Docx2013);
                doc.SaveToStream(ms, FileFormat.Docx2013);
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment; filename=外銷證明書.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

    }
}
