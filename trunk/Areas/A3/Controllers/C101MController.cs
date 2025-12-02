using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A3.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;
using OfficeOpenXml;
using System.Collections;
using HppApi;

namespace EECOnline.Areas.A3.Controllers
{
    public class C101MController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Index(new C101MFormModel());
        }

        [HttpPost]
        public ActionResult Index(C101MFormModel model)
        {
            ModelState.Clear();
            ActionResult rtn = View("Index", model);
            A3DAO dao = new A3DAO();
            // 設定查詢分頁資訊
            dao.SetPageInfo(model.rid, model.p);
            // 查詢結果
            model.Grid = dao.QueryC101M(model);
            // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
            if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows", model);
            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model, dao, "Index");
            return rtn;
        }

        [HttpPost]
        public ActionResult Export(C101MFormModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("Index1", model);
            A3DAO dao = new A3DAO();
            model.Grid = dao.QueryC101MAll(model);
            if (model.Grid.ToCount() <= 0) { sm.LastResultMessage = "查無資料"; }
            else
            {
                #region 固定欄位
                string[] DisplayArr = {
                    "訂單編號",
                    "申請人",
                    "申請醫院",
                    "申請日",
                    "病歷期間起",
                    "病歷期間迄",
                    "病歷產製費",
                    "繳費狀態",
                };
                string[] FieldArr = {
                    "apply_no_sub",
                    "user_name",
                    "hospital_name",
                    "createdatetime_Text",
                    "his_range1_Text",
                    "his_range2_Text",
                    "price_Text",
                    "payed_Text",
                };
                #endregion
                // 建立 Excel 物件
                ExcelPackage objExcel = new ExcelPackage();
                ExcelWorksheet objSheet = objExcel.Workbook.Worksheets.Add("申請案件費用明細表");
                // 一些暫存變數
                var iRow = 1;
                var iCol = 1;
                // 產生 標題列 (固定欄位)
                foreach (var col in DisplayArr)
                {
                    objSheet.Cells[iRow, iCol].Value = col;
                    iCol++;
                }
                // 產生 標題列 (動態欄位)
                iRow = 2;
                iCol = 1;
                foreach (var row in model.Grid)
                {
                    foreach (var col in FieldArr)
                    {
                        objSheet.Cells[iRow, iCol].Value = row.GetPiValue(col).TONotNullString();
                        iCol++;
                    }
                    iRow++;
                    iCol = 1;
                }
                // 後續處理
                objSheet.Cells.AutoFitColumns(10, 35);
                objSheet.Cells.Style.Font.Name = "新細明體";
                objSheet.Cells.Style.Font.Size = 12;
                //sm.LastResultMessage = "已匯出";
                LOG.Debug("A3C101M Export1() Called. Export record: " + model.Grid.ToCount().ToString() + " rows.");
                return File(objExcel.GetAsByteArray(), HelperUtil.XLSXContentType, "申請案件費用明細表.xlsx");
            }
            return rtn;
        }

        [HttpGet]
        public ActionResult ExportDat()
        {
            ModelState.Clear();
            return View("ExportDat", new C101MExportDatModel());
        }

        [HttpPost]
        public ActionResult ExportDat_Go(C101MExportDatModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            A3DAO dao = new A3DAO();
            ActionResult rtn = View("ExportDat", model);
            if (string.IsNullOrEmpty(model.hospital_code)) ModelState.AddModelError("", "請選擇醫院名稱");
            if (string.IsNullOrEmpty(model.PayStatus)) ModelState.AddModelError("", "請選擇繳費情形");
            if (ModelState.IsValid)
            {
                try
                {
                    // 特別條件組合
                    model.PayStatus_forSQL = null;
                    switch (model.PayStatus)
                    {
                        case "Y": model.PayStatus_forSQL = " AND a.payed = 'Y' "; break;               // 已繳費
                        case "N": model.PayStatus_forSQL = " AND a.payed = 'N' "; break;               // 未繳費
                        case "D": model.PayStatus_forSQL = " AND c.is_request_payment = 'Y' "; break;  // 請款成功
                    }
                    // 查資料
                    model.Grid = dao.QueryForListAll<ExportDatGridModel>("A3.queryC101M_ExportDat", model);
                    if (model.Grid.ToCount() <= 0)
                    {
                        sm.LastErrorMessage = "查無資料";
                        return rtn;
                    }
                    // 視情況增加
                    if (model.hospital_code == "1131010011H")
                    {
                        // 亞東
                        var GetSetups = new FrontDAO().QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);  // 取 DB 參數設定
                        var tmpMerchant = new FrontDAO().GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID_FE");       // 聯合特店代號
                        var tmpTerminal = new FrontDAO().GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID_FE");        // 聯合端末機代碼

                        // 生成 dat 檔
                        var DatFileList = ExportDat(tmpMerchant, tmpTerminal, model.Grid);

                        // 輸出檔案
                        byte[] DatFile = DatFileList
                            .SelectMany(x => System.Text.Encoding.UTF8.GetBytes(x + Environment.NewLine))
                            .ToArray();
                        return File(DatFile, HelperUtil.TEXTContentType, HttpUtility.UrlPathEncode(tmpMerchant + ".dat"));
                    }
                    else
                    {
                        sm.LastErrorMessage = "查無對應醫院格式";
                    }
                }
                catch (Exception ex)
                {
                    LOG.Debug("A3.C101M.ExportDat_Go() Error: " + ex.TONotNullString());
                    sm.LastErrorMessage = "錯誤，匯出失敗！";
                }
            }
            return rtn;
        }

        private void SetResponseCookie(string Value)
        {
            // 假設成功匯出，則回傳一個餅乾訊息 - by.Senya
            HttpCookie tmpCookie = new HttpCookie("CheckHasBeenDownloaded");
            tmpCookie.Value = Value;
            tmpCookie.Expires = DateTime.Now.AddSeconds(3);  // 3 秒後失效
            tmpCookie.HttpOnly = false;  // 設成 False 前面才抓得到
            Response.Cookies.Add(tmpCookie);
        }

        public static List<string> ExportDat(string Merchant, string Terminal, IList<ExportDatGridModel> Grid)
        {
            if (string.IsNullOrEmpty(Merchant)) return null;
            if (string.IsNullOrEmpty(Terminal)) return null;
            var resFile = new List<string>();
            // 檔頭資料
            var a1 = "H";                                                           // HEADER   長度 1
            var a2 = DateTime.Now.ToString("yyyyMMdd");                             // 送檔日期  長度 8
            var a3 = DateTime.Now.ToString("HHmmssffff");                           // 序號      長度 10
            var a4 = Grid.ToCount().ToString().PadLeft(12, '0');                    // 總筆數    長度 12
            var a5 = "+";                                                           // 金額正負號 長度 1
            var a6 = Grid.Sum(x => x.price.TOInt64()).ToString().PadLeft(12, '0');  // 總金額    長度 12
            var a7 = "".PadLeft(216, ' ');                                          // 空白      長度 216
            resFile.Add(a1 + Merchant + a2 + a3 + a4 + a5 + a6 + a7);
            // 明細資料
            foreach (var row in Grid)
            {
                if (string.IsNullOrEmpty(row.payed_orderid) || string.IsNullOrEmpty(row.payed_sessionkey)) continue;
                var b1 = row.payed_orderid.TONotNullString().PadRight(40, ' ');     // 訂單編號  長度 40
                var b2 = "".PadLeft(19, ' ');                                       // 空白      長度 19
                var b3 = row.price.TONotNullString().PadLeft(8, '0');               // 交易金額  長度 8
                var b4 = row.payed_approvecode.TONotNullString().PadRight(8, ' ');  // 授權碼    長度 8
                var b5 = "02";                                                      // 交易碼    長度 2
                var b6 = row.payed_transdate.TONotNullString();                     // 交易日期  長度 8
                if (string.IsNullOrEmpty(row.payed_approvecode) || string.IsNullOrEmpty(row.payed_transdate))
                {
                    var obj = ReGetNCCCApiDatas(row.payed_sessionkey);
                    if (obj == null || string.IsNullOrEmpty(obj.getAPPROVECODE())) continue;
                    b4 = obj.getAPPROVECODE().TONotNullString().PadRight(8, ' ');
                    b6 = obj.getTRANSDATE();
                }
                var b7 = "".PadLeft(16, ' ');   // 使用者自訂欄位 長度 16
                var b8 = "".PadLeft(20, '　');  // 卡人資訊       長度 20個全形空白
                var b9 = "".PadLeft(111, ' ');  // 其他欄位補滿
                resFile.Add(Merchant + Terminal + b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8 + b9);
            }
            return resFile;
        }

        private static ApiClient ReGetNCCCApiDatas(string Key)
        {
            if (Key.TONotNullString() == "") return null;
            // 取 DB 參數設定
            var dao = new FrontDAO();
            var GetSetups = dao.QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);
            // 去 API 抓資料
            ApiClient apiClient = new ApiClient();
            apiClient.setKEY(Key);
            apiClient.setURL(
                dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_DOMAINNAME"),  // 聯合正式或測試DomainName
                dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_REQUESTURL")   // 聯合正式或測試RequestUrl
            );
            int res = apiClient.postQuery();
            return apiClient;
        }

        [HttpGet]
        public ActionResult ImportDat()
        {
            ModelState.Clear();
            return View("ImportDat", new C101MImportDatModel());
        }

        private string isFileCheckOK(HttpPostedFileBase YourFile)
        {
            string Result = "";
            const string acceptType = ".rsp";
            if (YourFile == null || YourFile.ContentLength.TOInt32() <= 0) return "請選擇上傳檔案！<br/ >";
            if (!acceptType.Contains(Path.GetExtension(YourFile.FileName).ToLower())) Result = Result + "請選擇正確的檔案格式！ (" + acceptType + ")<br/ >";
            if (YourFile.ContentLength.TOInt32() > (20 * 1024 * 1024)) Result = Result + "檔案大小以 20MB 為限！<br/ >";
            return Result;
        }

        [HttpPost]
        public ActionResult ImportDat_Go(C101MImportDatModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("ImportDat", model);
            var ErrMsg = this.isFileCheckOK(model.UploadFILE);
            if (ErrMsg != "") sm.LastErrorMessage = ErrMsg;
            else
            {
                try
                {
                    model.Grid = new List<C101MImportDatGridModel>();

                    // 上傳檔案
                    var RSP_Path = Server.MapPath(@"~\Uploads\RSP_Files\");
                    System.IO.Directory.CreateDirectory(RSP_Path);
                    var TheFilePath = RSP_Path + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + model.UploadFILE.FileName;
                    model.UploadFILE.SaveAs(TheFilePath);

                    // 讀取檔案
                    var tmpList = new List<string>();
                    using (StreamReader reader = new StreamReader(TheFilePath/*, System.Text.Encoding.GetEncoding(950)*/))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            tmpList.Add(line);
                        }
                        reader.Close();
                    }

                    var successCount = 0;
                    if (tmpList.ToCount() > 0)
                    {
                        // 處理檔案
                        foreach (var row in tmpList)
                        {
                            var obj = new C101MImportDatGridModel();
                            obj.A_001_010_10 = row.Substring(0, 10);
                            obj.B_011_018_8 = row.Substring(10, 8);
                            obj.C_019_058_40 = row.Substring(18, 40);
                            obj.D_059_077_19 = row.Substring(58, 19);
                            obj.E_078_085_8 = row.Substring(77, 8);
                            obj.F_086_093_8 = row.Substring(85, 8);
                            obj.G_094_095_2 = row.Substring(93, 2);
                            obj.H_096_103_8 = row.Substring(95, 8);
                            obj.I_104_119_16 = row.Substring(103, 16);
                            obj.J_120_159_40 = row.Substring(119, 20); //卡人資訊
                            obj.K_160_165_6 = row.Substring(139, 6);
                            obj.L_166_168_3 = row.Substring(145, 3);
                            obj.M_169_184_16 = row.Substring(148, 12); //請款成功
                            obj.N_185_190_6 = row.Substring(160, 6);
                            obj.O_191_191_1 = row.Substring(166, 1);
                            obj.P_192_193_2 = row.Substring(167, 2);
                            obj.Q_194_201_8 = row.Substring(169, 8);
                            obj.R_202_209_8 = row.Substring(177, 8);
                            obj.S_210_215_6 = row.Substring(185, 6);
                            obj.T_216_223_8 = row.Substring(191, 8);
                            obj.U_224_224_1 = row.Substring(199, 1);
                            obj.V_225_232_8 = row.Substring(200, 8);
                            obj.W_233_242_10 = row.Substring(208, 10);
                            obj.X_243_250_8 = row.Substring(218, 8);
                            obj.Y_251_251_1 = row.Substring(226, 1);
                            obj.Z_252_252_1 = row.Substring(227, 1);
                            obj.ZA_253_253_1 = row.Substring(228, 1);
                            //obj.ZB_254_270_17 = row.Substring(233, 17);
                            model.Grid.Add(obj);
                        }

                        // 存資料庫
                        A3DAO dao = new A3DAO();
                        foreach (var row in model.Grid)
                        {
                            var orderid = row.C_019_058_40.Trim();
                            var rsp_code = row.L_166_168_3.Trim();
                            var rsp_name = row.M_169_184_16.Trim();
                            if (string.IsNullOrEmpty(orderid) || string.IsNullOrEmpty(rsp_code) || string.IsNullOrEmpty(rsp_name)) continue;
                            var rsp_status = (rsp_code == "00" && rsp_name == "請款成功");
                            if (rsp_status) successCount++;
                            dao.C101M_SaveRspStatus(orderid, rsp_status);
                        }
                    }
                    sm.LastResultMessage =
                        "已上傳 " + model.Grid.ToCount().ToString() + " 筆<br/>" +
                        "共 " + successCount.ToString() + " 筆請款成功";
                }
                catch (Exception ex)
                {
                    LOG.Debug("A3.C101M.ImportDat_Go() Error: " + ex.TONotNullString());
                    sm.LastErrorMessage = "上傳失敗";
                }
            }
            return rtn;
        }
    }
}