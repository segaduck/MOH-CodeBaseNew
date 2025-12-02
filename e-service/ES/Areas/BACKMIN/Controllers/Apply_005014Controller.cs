using ES.Areas.Admin.Controllers;
using ES.Areas.BACKMIN.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Services;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Xceed.Words.NET;
using Apply_005014ViewModel = ES.Areas.BACKMIN.Models.Apply_005014ViewModel;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_005014Controller : BaseController
    {
        public ActionResult Index(string appid)
        {
            return RedirectToAction("Apply", new { id = "Entry", APP_ID = appid });
        }

        [HttpGet]
        public ActionResult Apply(string id)
        {
            if (string.IsNullOrEmpty(id)) { id = "Entry"; }

            FormValueProvider vp = new FormValueProvider(this.ControllerContext);
            QueryStringValueProvider qs = new QueryStringValueProvider(this.ControllerContext);
            Apply_005014ViewModel model = new Apply_005014ViewModel();
            ActionResult result = null;

            try
            {
                switch (id)
                {
                    case "Entry":   // 功能進入點
                        if (this.TryUpdateModel(model, qs))
                        {
                            model.GetApplyData(model.APP_ID);
                            model.ResetErrataField();
                            result = View("./Index", model);
                        }
                        break;

                    default:
                        result = HttpNotFound();
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error("B005014_Apply failed:" + ex.TONotNullString());
                result = HttpNotFound();
            }

            return result;
        }

        [HttpPost]
        public ActionResult ApplyPost(string id)
        {
            if (string.IsNullOrEmpty(id)) { return HttpNotFound(); }

            Apply_005014ViewModel model = new Apply_005014ViewModel();
            string viewName = null;
            ActionResult result = null;

            FormValueProvider vp = new FormValueProvider(this.ControllerContext);
            QueryStringValueProvider vp1 = new QueryStringValueProvider(this.ControllerContext);
            HttpFileCollectionValueProvider vp3 = new HttpFileCollectionValueProvider(this.ControllerContext);
            ValueProviderCollection collection = new ValueProviderCollection();
            collection.Add(vp);
            collection.Add(vp1);
            collection.Add(vp3);

            try
            {
                switch (id)
                {
                    case "Preview":
                        this.ModelState.Clear();
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.Preview();
                            model.Validate(ModelState);

                            if (ModelState.IsValid)
                            {
                                StringBuilder sb = ControllerContextHelper
                                    .RenderRazorPartialViewToString(this.ControllerContext, "CasePreview", model);
                                result = Json(new { status = true, data = sb.ToString() });
                            }
                            else
                            {
                                throw new ArgumentException();
                            }
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                        break;

                    case "Save":
                        this.ModelState.Clear();
                        if (this.TryUpdateModel(model, collection))
                        {
                            try
                            {
                                model.context = this.ControllerContext;
                                model.BeforeSave();
                                model.SaveApply(this.ModelState);
                                result = Json(new { status = true, msg = "" }); // 成功提示訊息
                            }
                            catch (Exception ex)
                            {
                                logger.Debug("B005014_ApplyPost failed:" + ex.TONotNullString());
                                result = Json(new { status = false, msg = "儲存失敗" }); // 成功提示訊息
                            }
                        }
                        break;

                    case "ApplyItemList":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetApplyItemList();
                            model.ResetErrataField();   // 清除補正欄位
                            result = Json(model.ApplyItems);
                        }
                        break;
                    case "ApplyItem2List":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetApplyItem2List();
                            model.ResetErrataField();   // 清除補正欄位
                            result = Json(model.ApplyItems2);
                        }
                        break;
                    case "EntryApplyItem":
                        Apply_005014_ItemExt item = new Apply_005014_ItemExt { ID = DateTime.Now.Ticks.ToString() };
                        model.ApplyItems.Add(item);
                        result = Json(model.ApplyItems);
                        break;
                    case "EntryApplyItem2":
                        Apply_005014_Item2Ext item2 = new Apply_005014_Item2Ext { ID = DateTime.Now.Ticks.ToString() };
                        model.ApplyItems2.Add(item2);
                        result = Json(model.ApplyItems2);
                        break;
                    case "GoodsUnitList":
                        var codeText0 = new ShareCodeListModel().GetGOODS_UNIT_IDList;
                        result = Json(codeText0);
                        break;
                    case "Vw_PACKList":
                        var codeText = new ShareCodeListModel().Getvw_PACKList;
                        result = Json(codeText);
                        break;
                    case "Vw_PackUnitList":
                        var codeText1 = new ShareCodeListModel().Getvw_PACK_UNITList;
                        result = Json(codeText1);
                        break;
                    case "CountryList":
                        var codeText2 = new ShareCodeListModel().CountryList;
                        result = Json(codeText2);
                        break;
                    case "ImportList":
                        var codeText4 = new ShareCodeListModel().ImportList;
                        result = Json(codeText4);
                        break;
                    case "Vw_DrugFormList":
                        var codeText3 = new ShareCodeListModel().Getvw_DRUG_FORMList;
                        result = Json(codeText3);
                        break;
                    case "NewApplyItem":
                        result = Json(new Apply_005014_ItemExt { ID = DateTime.Now.Ticks.ToString() });
                        break;
                    case "NewApplyItem2":
                        result = Json(new Apply_005014_Item2Ext { ID = DateTime.Now.Ticks.ToString() });
                        break;
                    case "CaseFileList":
                        if (this.TryUpdateModel(model, collection))
                        {
                            model.GetFileList();
                            result = Json(model.FileList);
                        }
                        break;

                    default:
                        result = HttpNotFound();
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                logger.Debug("B005014_ApplyPost failed:" + ex.TONotNullString());
                IList<object> errList = this.CollectionError(this.ModelState);
                result = Json(new { status = false, data = errList }); // 失敗提示訊息
            }
            catch (Exception ex)
            {
                logger.Debug("B005014_ApplyPost failed:" + ex.TONotNullString());
                result = HttpNotFound();
            }

            if (result == null) { result = View(viewName, model); }

            return result;
        }


        [DisplayName("Apply_001005_補件查詢")]
        public ActionResult AppDoc(string APP_ID)
        {
            Apply_005014ViewModel model = new Apply_005014ViewModel();
            if (!string.IsNullOrEmpty(APP_ID))
            {
                model.GetApplyData(APP_ID);
            }
            return View("Index", model);
        }

        /// <summary>
        /// 取得錯誤訊息
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public IList<object> CollectionError(ModelStateDictionary state)
        {
            IList<object> errList = new List<object>();
            if (!this.ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errItem = new
                    {
                        key = key,
                        msg = string.Join("；", ModelState[key].Errors.Select(x => x.ErrorMessage))
                    };
                    errList.Add(errItem);
                }
            }
            return errList;
        }

        /// <summary>
        /// 產壓縮檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void GetZIP(string APP_ID, string CASENAME)
        {
            #region 另存檔案至目錄

            // 判斷是否有資料夾
            if (!Directory.Exists(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP")))
            {
                Directory.CreateDirectory(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
            }

            ShareDAO dao = new ShareDAO();

            // 取檔案後排序
            Apply_005014_FILE fm = new Apply_005014_FILE();
            fm.APP_ID = APP_ID;
            var fmlst = dao.GetRowList(fm);
            var newfmlst = from a in fmlst
                           orderby a.CREATE_DATE descending
                           select a;
            // 不同檔案項目分別群組，取得最新的一筆下載
            var grpfmlst = newfmlst.GroupBy(x => new { x.FILE_ID }).ToList();
            foreach (var gfmlst in grpfmlst)
            {
                var fli = gfmlst.Where(x => x.FILE_NAME != null || x.FILE_NAME == "").OrderByDescending(x => x.CREATE_DATE).FirstOrDefault();
                // 紀錄FILE_NO 已避免重複
                string newFileName = "";
                int iPos = -1;
                if (!string.IsNullOrEmpty(fli.FILE_NAME))
                {
                    var fileRename = fli.FILE_NAME.Replace("/", "\\");
                    iPos = fileRename.LastIndexOf("\\");
                    newFileName = fli.FILE_NAME.Substring(iPos + 1, fli.FILE_NAME.Length - iPos - 1);

                    var FilePath = Server.MapPath(@"../../Template/" + APP_ID + "_ZIP") + "/" + newFileName;//item.SRC_FILENAME;
                    var dbyte = dao.sftpDownload(fli.FILE_NAME);
                    System.IO.File.WriteAllBytes(FilePath, dbyte);
                }
            }
            #endregion

            string zipFileName = CASENAME + "_" + APP_ID + ".zip";
            string[] filenames = Directory.GetFiles(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP/"));
            //byte[] buffer = new byte[409600];

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP/" + zipFileName))))
            {
                // 設定壓縮比
                zp.SetLevel(2);


                // 逐一將資料夾內的檔案抓出來壓縮，並寫入至目的檔(.ZIP)
                foreach (string filename in filenames)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(filename));
                    zp.PutNextEntry(entry);
                    using (FileStream fs = new FileStream(filename, FileMode.Open))
                    {
                        byte[] buffer = new byte[fs.Length];
                        int readLength;
                        do
                        {
                            readLength = fs.Read(buffer, 0, buffer.Length);
                            if (readLength > 0)
                            {
                                zp.Write(buffer, 0, readLength);
                            }
                        } while (readLength > 0);
                    }
                }

            }
            FileInfo file = new FileInfo(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP/" + zipFileName));
            // Clear the content of the response
            Response.ClearContent();

            // LINE1: Add the file name and attachment, which will force the open/cance/save dialog to show, to the header
            Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", file.Name));
            //Response.Headers["Content-Disposition"] = "attachment; filename=" + zipFileName;

            // Add the file size into the response header
            Response.AddHeader("Content-Length", file.Length.ToString());

            // Set the ContentType
            Response.ContentType = "application/zip";
            Response.TransmitFile(file.FullName);
            // End the response
            Response.End();
            //Response.Flush();
            //Response.Close();
            //// 刪除資料夾
            if (Directory.Exists(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP")))
            {
                string[] files = Directory.GetFiles(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
                string[] dirs = Directory.GetDirectories(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
                foreach (string item in files)
                {
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }
                Directory.Delete(Server.MapPath(@"../../Template/" + APP_ID + "_ZIP"));
            }
        }

        /// <summary>
        /// 005014_申請單套表"
        /// </summary>
        /// <param name="model"></param>
        [DisplayName("005014_申請單套表")]
        public void PreviewApplyForm1(Apply_005014ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string path = Server.MapPath("~/Sample/apply005014_1.docx");
            byte[] buffer = null;
            if (model.Apply.APP_TIME == null)
            {
                var where = new ApplyModel();
                where.APP_ID = model.Apply.APP_ID;
                var temp = dao.GetRow(where);
                model.Apply.APP_TIME = temp.APP_TIME;
            }
            
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    var cnt_phone = !string.IsNullOrEmpty(model.CNT_TEL_Zip) ? $"({model.CNT_TEL_Zip}){model.CNT_TEL_Phone}" : "";
                    var cnt_tel = !string.IsNullOrEmpty(model.CNT_TEL_Num) ? $"{cnt_phone}#{model.CNT_TEL_Num}" : cnt_phone;

                    //地址
                    TblZIPCODE zip = new TblZIPCODE();
                    zip.ZIP_CO = model.Apply.ADDR_ZIP;
                    var address = dao.GetRow(zip);
                    if (address != null && !string.IsNullOrEmpty(address.TOWNNM))
                    {
                        model.Apply.ADDR_ZIP_ADDR = address.CITYNM + address.TOWNNM;
                        model.Apply.ADDR_ZIP_DETAIL = model.Apply.ADDR_ZIP_DETAIL.TONotNullString().Replace(address.CITYNM + address.TOWNNM, "");
                    }

                    TblCODE_CD cc = new TblCODE_CD();
                    cc.CODE_KIND = "F1_PORT_2";
                    cc.CODE_PCD = "";
                    cc.CODE_CD = model.Detail.PRODUCTION_COUNTRY.TONotNullString();
                    var coo = dao.GetRow(cc).CODE_DESC.TONotNullString();
                    cc.CODE_CD = model.Detail.TRANSFER_COUNTRY.TONotNullString();
                    var sp = dao.GetRow(cc).CODE_DESC.TONotNullString();
                    cc.CODE_CD = model.Detail.SELL_COUNTRY.TONotNullString();
                    var cos = dao.GetRow(cc).CODE_DESC.TONotNullString();
                    cc.CODE_CD = model.Detail.TRANSFER_PORT.TONotNullString();
                    var spt = dao.GetRow(cc).CODE_DESC.TONotNullString();
                    // 替換文字
                    doc.ReplaceText("[$YEAR]", (((DateTime)model.Apply.APP_TIME.Value).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)model.Apply.APP_TIME.Value).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)model.Apply.APP_TIME.Value).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLICANT]", model.Apply.NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", model.Apply.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$AATO]", model.Apply.ADDR_ZIP.TONotNullString() + model.Apply.ADDR_ZIP_ADDR.TONotNullString() + model.Apply.ADDR_ZIP_DETAIL.TONotNullString() + "\n" + cnt_tel.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COO]", coo, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SP]", sp + "\n" + spt, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COS]", cos, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLYMAN]", $"{model.Apply.NAME} 線上申請案，案件編號:\n{model.Apply.APP_ID.TONotNullString()}", false, System.Text.RegularExpressions.RegexOptions.None);

                    // 動態表格處理
                    var itemlist = new List<Hashtable>();
                    var record = Convert.ToString(model.ApplyItemsJson);
                    var tempFinal = (JArray)JsonConvert.DeserializeObject(record);
                    var record2 = Convert.ToString(model.ApplyItems2Json);
                    var tempFinal2 = (JArray)JsonConvert.DeserializeObject(record2);
                    int i = 0;
                    foreach (var jitem in tempFinal)
                    {
                        Hashtable dataItem = new Hashtable();
                        i++;
                        for (int k = 0; k < 29; k++)
                        {
                            dataItem[((JProperty)jitem.ToList()[k]).Name] = ((JValue)jitem.ToList()[k].FirstOrDefault()).Value;
                        }

                        itemlist.Add(dataItem);
                    }
                    i = 0;
                    foreach (var jitem2 in tempFinal2)
                    {
                        Hashtable dataItem = new Hashtable();
                        i++;
                        for (int k = 0; k < 29; k++)
                        {
                            dataItem[((JProperty)jitem2.ToList()[k]).Name] = ((JValue)jitem2.ToList()[k].FirstOrDefault()).Value;
                        }

                        itemlist.Add(dataItem);
                    }
                    var tb = doc.AddTable(itemlist.ToCount() + 1, 5);
                    tb.Paragraphs[0].FontSize(11);
                    tb.Paragraphs[0].Font("標楷體");
                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Rows[0].Cells[0].Paragraphs[0].Append("⑦項次\n Item").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[1].Paragraphs[0].Append("⑧貨名、規格、廠牌及製造廠名稱\n Description of Commodities Spec.and Brand or Maker, etc.").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[2].Paragraphs[0].Append("⑨貨品分類稅則號列\n C.C.C.Code").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[3].Paragraphs[0].Append("⑩數量\n Q'ty").FontSize(11).Font("標楷體"); ;
                    tb.Rows[0].Cells[4].Paragraphs[0].Append("⑪單位\n Unit").FontSize(11).Font("標楷體"); ;
                    var q = 1;
                    foreach (var item in itemlist)
                    {
                        tb.Rows[q].Cells[0].Paragraphs[0].Append(q.TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[1].Paragraphs[0].Append(item["COMMODITIES"].TONotNullString()).FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[2].Paragraphs[0].Append("").FontSize(12).Font("標楷體");
                        tb.Rows[q].Cells[3].Paragraphs[0].Append(item["QTY"].TONotNullString()).FontSize(12).Font("標楷體");
                        TblCODE_CD un = new TblCODE_CD();
                        un.CODE_KIND = "F5_vw_PACK";//申請數量單位
                        un.CODE_PCD = "";
                        un.CODE_CD = item["UNIT"].TONotNullString();
                        var une = dao.GetRow(un).CODE_MEMO.TONotNullString();

                        TblCODE_CD un_unit = new TblCODE_CD();
                        un_unit.CODE_KIND = "F5_vw_PACK_UNIT"; //規格數量單位
                        un_unit.CODE_PCD = "";
                        un_unit.CODE_CD = item["SPECUNIT"].TONotNullString();
                        var une_unit = dao.GetRow(un_unit).CODE_DESC.TONotNullString();

                        tb.Rows[q].Cells[4].Paragraphs[0].Append(une.TONotNullString() + "\n" + item["SPECQTY"].TONotNullString() + une_unit.TONotNullString()).FontSize(12).Font("標楷體");
                        q++;
                    }
                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);

                    // 備註 替換文字
                    var firstRemark = model.Detail.Remark;
                    var repltext = string.Empty;
                    //REMARK1 YN // REMARK1_ITEM1_COMMENT
                    doc.ReplaceText("[$Remarks31-1]", firstRemark.checkboxR1 ?
                       $"█1.申請H01免查驗(報單影本，報單碼：{firstRemark.REMARK1_ITEM1_COMMENT.TONotNullString()})" :
                       $"□1.申請H01免查驗(報單影本，報單碼：_____)", false, System.Text.RegularExpressions.RegexOptions.None);
                    //REMARK1_ITEM2 1,2 //REMARK1_ITEM2_COMMENT
                    doc.ReplaceText("[$Remarks31-2]", firstRemark.REMARK1_ITEM2.TONotNullString() == "" ?
                        $"□新鮮品；□非中藥用途：_____" : firstRemark.REMARK1_ITEM2.TONotNullString() == "2" ?
                        $"□新鮮品；█非中藥用途：{firstRemark.REMARK1_ITEM2_COMMENT.TONotNullString()}" :
                        $"█新鮮品；□非中藥用途：_____", false, System.Text.RegularExpressions.RegexOptions.None);
                    //REMARK2 YN 
                    doc.ReplaceText("[$Remarks32]", firstRemark.checkboxR2 ? "█" : "□", false, System.Text.RegularExpressions.RegexOptions.None);
                    //REMARK3_1 YN
                    doc.ReplaceText("[$Remarks36-1]", firstRemark.checkboxR3 ?
                        $"█6.非中藥用途貨品進口：" :
                        $"□6.非中藥用途貨品進口：", false, System.Text.RegularExpressions.RegexOptions.None);
                    //REMARK3_2 1 //REMARK3_2_COMMENT
                    switch (firstRemark.REMARK3_2.TONotNullString())
                    {
                        case "1":
                            doc.ReplaceText("[$Remarks36-2]", $"█食品：{firstRemark.REMARK3_2_COMMENT.TONotNullString()} □研發：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            doc.ReplaceText("[$Remarks36-3]", $"□試製：_____ □其他：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            break;
                        case "2":
                            doc.ReplaceText("[$Remarks36-2]", $"□食品：_____ █研發：{firstRemark.REMARK3_3_COMMENT.TONotNullString()} ", false, System.Text.RegularExpressions.RegexOptions.None);
                            doc.ReplaceText("[$Remarks36-3]", $"□試製：_____ □其他：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            break;
                        case "3":
                            doc.ReplaceText("[$Remarks36-2]", $"□食品：_____ □研發：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            doc.ReplaceText("[$Remarks36-3]", $"█試製：{firstRemark.REMARK3_4_COMMENT.TONotNullString()} □其他：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            break;
                        case "4":
                            doc.ReplaceText("[$Remarks36-2]", $"□食品：_____ □研發：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            doc.ReplaceText("[$Remarks36-3]", $"□試製：_____ █其他：{firstRemark.REMARK3_5_COMMENT.TONotNullString()} ", false, System.Text.RegularExpressions.RegexOptions.None);
                            break;
                        default:
                            doc.ReplaceText("[$Remarks36-2]", $"□食品：_____ □研發：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            doc.ReplaceText("[$Remarks36-3]", $"□試製：_____ □其他：_____ ", false, System.Text.RegularExpressions.RegexOptions.None);
                            break;
                    }

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=貨品進口專案申請單.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// 005014_一般切結書套表"
        /// </summary>
        /// <param name="model"></param>
        [DisplayName("005014_申請單套表")]
        public void PreviewApplyForm2A(Apply_005014ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string path = Server.MapPath("~/Sample/apply005014_2.docx");
            byte[] buffer = null;
            if (model.Apply.APP_TIME == null)
            {
                var where = new ApplyModel();
                where.APP_ID = model.Apply.APP_ID;
                var temp = dao.GetRow(where);
                model.Apply.APP_TIME = temp.APP_TIME;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    TblCODE_CD cc = new TblCODE_CD();
                    cc.CODE_KIND = "F1_PORT_2";
                    cc.CODE_PCD = "";
                    cc.CODE_CD = model.Detail.TRANSFER_COUNTRY.TONotNullString();
                    var sp = dao.GetRow(cc).CODE_DESC.TONotNullString();
                    var cnt_phone = !string.IsNullOrEmpty(model.CNT_TEL_Zip) ? $"({model.CNT_TEL_Zip}){model.CNT_TEL_Phone}" : "";
                    var cnt_tel = !string.IsNullOrEmpty(model.CNT_TEL_Num) ? $"{cnt_phone}#{model.CNT_TEL_Num}" : cnt_phone;
                    //var aff1_imp = model.GetPortText(model.Detail.AFF1_IMPORT_COUNTRY.TONotNullString());
                    // 替換文字
                    doc.ReplaceText("[$YEAR]", (((DateTime)model.Apply.APP_TIME.Value).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)model.Apply.APP_TIME.Value).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)model.Apply.APP_TIME.Value).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SHIPPORT]", sp, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_NAME]", model.Apply.NAME, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_ADDR]", model.Apply.ADDR_ZIP_ADDR.TONotNullString() + model.Apply.ADDR_ZIP_DETAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CHR_NAME]", model.Apply.CHR_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_NAME]", model.Apply.CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_TEL]", cnt_tel, false, System.Text.RegularExpressions.RegexOptions.None);
                    // 動態表格處理
                    var itemlist = new List<Hashtable>();
                    var record = Convert.ToString(model.ApplyItemsJson);
                    var tempFinal = (JArray)JsonConvert.DeserializeObject(record);
                    int i = 0;
                    foreach (var jitem in tempFinal)
                    {
                        Hashtable dataItem = new Hashtable();
                        i++;
                        for (int k = 0; k < 35; k++)
                        {
                            dataItem[((JProperty)jitem.ToList()[k]).Name] = ((JValue)jitem.ToList()[k].FirstOrDefault()).Value;
                        }

                        itemlist.Add(dataItem);
                    }
                    var itemList = string.Empty;
                    foreach (var item in itemlist)
                    {
                        var itemtype = item["ITEM_TYPE"].TONotNullString();
                        //中藥材作為食品使用者(例如:靈芝) or 其他
                        if (itemtype == "3" || itemtype == "2")
                        {
                            //單位名稱
                            TblCODE_CD un = new TblCODE_CD();
                            un.CODE_KIND = "F5_vw_PACK";
                            un.CODE_PCD = "";
                            un.CODE_CD = item["UNIT"].TONotNullString();
                            var une = dao.GetRow(un).CODE_DESC.TONotNullString();
                            TblCODE_CD port = new TblCODE_CD();
                            port.CODE_KIND = "F5_vw_IMPORT";
                            port.CODE_PCD = "";
                            port.CODE_CD = item["AFF1_IMPORT_COUNTRY"].TONotNullString();
                            var portn = dao.GetRow(port).CODE_DESC.TONotNullString();
                          
                            //    人參丸       1瓶  ，報單號碼   CH1039500148  (進口關：基隆關)
                            itemList += $"{item["COMMODITIES"].TONotNullString()} {item["QTY"].TONotNullString()} {une}，";
                            itemList += $"報單號碼 {item["AFF1_SHEET_NO"].TONotNullString()} (進口關：{portn})";
                        }
                    }
                    doc.ReplaceText("[$ITEMLIST]", itemList, false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=貨品進口專案一般切結書.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }

        /// <summary>
        /// 005014_萃取物切結書套表"
        /// </summary>
        /// <param name="model"></param>
        [DisplayName("005014_申請單套表")]
        public void PreviewApplyForm2B(Apply_005014ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            string path = Server.MapPath("~/Sample/apply005014_3.docx");
            byte[] buffer = null;
            if (model.Apply.APP_TIME == null)
            {
                var where = new ApplyModel();
                where.APP_ID = model.Apply.APP_ID;
                var temp = dao.GetRow(where);
                model.Apply.APP_TIME = temp.APP_TIME;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    TblCODE_CD cc = new TblCODE_CD();
                    cc.CODE_KIND = "F1_PORT_2";
                    cc.CODE_PCD = "";
                    cc.CODE_CD = model.Detail.TRANSFER_COUNTRY.TONotNullString();
                    var sp = dao.GetRow(cc).CODE_DESC.TONotNullString();
                    var cnt_phone = !string.IsNullOrEmpty(model.CNT_TEL_Zip) ? $"({model.CNT_TEL_Zip}){model.CNT_TEL_Phone}" : "";
                    var cnt_tel = !string.IsNullOrEmpty(model.CNT_TEL_Num) ? $"{cnt_phone}#{model.CNT_TEL_Num}" : cnt_phone;
                    //var aff2_imp = model.GetPortText(model.Detail.AFF2_IMPORT_COUNTRY.TONotNullString());
                    // 替換文字
                    doc.ReplaceText("[$YEAR]", (((DateTime)model.Apply.APP_TIME.Value).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)model.Apply.APP_TIME.Value).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)model.Apply.APP_TIME.Value).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SHIPPORT]", sp, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_NAME]", model.Apply.NAME, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_ADDR]", model.Apply.ADDR_ZIP_ADDR.TONotNullString() + model.Apply.ADDR_ZIP_DETAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CHR_NAME]", model.Apply.CHR_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_NAME]", model.Apply.CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_TEL]", cnt_tel, false, System.Text.RegularExpressions.RegexOptions.None);
                    // 動態表格處理
                    var itemlist = new List<Hashtable>();
                    var record = Convert.ToString(model.ApplyItems2Json);
                    var tempFinal = (JArray)JsonConvert.DeserializeObject(record);
                    int i = 0;
                    foreach (var jitem in tempFinal)
                    {
                        Hashtable dataItem = new Hashtable();
                        i++;
                        for (int k = 0; k < 35; k++)
                        {
                            dataItem[((JProperty)jitem.ToList()[k]).Name] = ((JValue)jitem.ToList()[k].FirstOrDefault()).Value;
                        }

                        itemlist.Add(dataItem);
                    }
                    var itemList = string.Empty;
                    foreach (var item in itemlist)
                    {
                        var itemtype = item["ITEM_TYPE"].TONotNullString();
                        //中藥材萃取物作為食品原料者
                        if (itemtype == "1")
                        {
                            //單位名稱
                            TblCODE_CD un = new TblCODE_CD();
                            un.CODE_KIND = "F5_vw_PACK";
                            un.CODE_PCD = "";
                            un.CODE_CD = item["UNIT"].TONotNullString();
                            var une = dao.GetRow(un).CODE_MEMO.TONotNullString();
                            TblCODE_CD port = new TblCODE_CD();
                            port.CODE_KIND = "F5_vw_IMPORT";
                            port.CODE_PCD = "";
                            port.CODE_CD = item["AFF2_IMPORT_COUNTRY"].TONotNullString();
                            var portn = dao.GetRow(port).CODE_DESC.TONotNullString();
                            //    (貨品名稱)     (產品名稱自動帶入每公克含原中藥材OO共OO公克)  (貨品數量)  ，報單號碼 (進口關： )
                            var aff2 = $"每公克含原中藥材 {item["AFF2_AMOUNT_NAME"]} 共 {item["AFF2_AMOUNT"]} 公克) ";
                            itemList += $"{item["COMMODITIES"].TONotNullString()} ({aff2}) {item["QTY"].TONotNullString()} {une}，";
                            itemList += $"報單號碼 {item["AFF2_SHEET_NO"].TONotNullString()} (進口關：{portn})";
                        }
                    }
                    doc.ReplaceText("[$ITEMLIST]", itemList, false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "Application/msword";
            Response.AddHeader("Content-Disposition", "attachment;   filename=貨品進口專案萃取物切結書.docx");
            Response.BinaryWrite(buffer);
            Response.OutputStream.Flush();
            Response.OutputStream.Close();
            Response.Flush();
            Response.End();
        }
    }
}
