using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;
using System.Web.Routing;
using ES.Utils;
using System.IO;
using WebUI.CustomClass;
using System.Data.SqlClient;
using ES.DataLayers;
using ES.Models;
using System.Text;
using System.Diagnostics;
using DocumentFormat.OpenXml.Vml;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using DocumentFormat.OpenXml.Bibliography;
using NPOI.SS.Formula.Functions;
using Microsoft.Office.Interop.Word;
using System.Net.Sockets;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PayECController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 7;
        }

        // GET: /Admin/PayEC/
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            PayECModel model = new PayECModel();
            model.queryModel.APP_TIME_BEGIN = DateTime.Now;
            model.queryModel.APP_TIME_END = DateTime.Now;
            model.queryModel.NowPage = 1;
            ViewBag.NowPage = 1;
            ViewBag.TotalCount = 0;
            ViewBag.TotalPage = 0;

            this.SetVisitRecord("PayEC", "Index", "信用卡請款");

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(PayECModel model)
        {
            String rspStatus = String.IsNullOrEmpty(Request.Form["sel_RSP_STATUS"]) ? "" : Request.Form["sel_RSP_STATUS"];
            PayAction action = new PayAction();
            model.queryModel.RSP_STATUS = rspStatus;
            logger.Debug("PayECModel.Index For Query:" + model.submit_type);
            if (model.submit_type == "2")
            {
                Export(model);
            }

            ViewBag.List = action.MaintainQuery(model.queryModel);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.queryModel.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }

        /// <summary>
        /// 匯出請款檔
        /// </summary>
        /// <param name="model"></param>
        public void Export(PayECModel model)
        {
            //登錄950 big5
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var big5 = Encoding.GetEncoding(950);
            //logger.Debug($"Name : {big5.EncodingName} , CodePage :{big5.CodePage}");
            //Console.ReadKey();
            //顯示所有Encoding
            //var encodings = Encoding.GetEncodings();
            //foreach (var t in encodings)
            //{
            //    logger.Debug($"DisplayName:{t.DisplayName}, codePage: {t.CodePage}");
            //}

            PayAction action = new PayAction();
            SessionModel sm = SessionModel.Get();
            var sumPayMoney = 0;
            var cnt = 0;
            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.queryModel.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);
            try
            {
                var memStream = new MemoryStream();
                var merchant = DataUtils.GetConfig("PAY_EC_MERCHANTID");
                var dt = DateTime.Now.ToString("yyyyMMdd");
                var dtRan = new Random().Next(999999999).ToString().PadLeft(10, '0');
                var listStr = string.Empty;
                var sumStr = string.Empty;
                var headerEmpty = "".PadLeft(216, ' ');
                var txtList = new List<string>();
                var filePath = $@"{DataUtils.GetConfig("FOLDER_TEMPLATE")}\Pay\";
                var fileName = $"{filePath}{merchant}.dat";
                // 請款明細
                logger.Debug("Export ViewBag.List: exportECDetailList");
                ViewBag.List = action.exportECDetailList(model.queryModel);

                if (ViewBag.List != null && ViewBag.List.Count > 0)
                {
                    // 已存在檔案-刪除
                    logger.Debug("匯出請款檔  Export ViewBag.List");
                    if (System.IO.File.Exists(fileName))
                    {
                        logger.Debug("匯出請款檔  檔案-刪除");
                        System.IO.File.Delete(fileName);
                    }

                    foreach (var item in ViewBag.List)
                    {
                        // 請款總金額
                        sumPayMoney += item.GetInt("PAY_MONEY");
                        // 請款明細
                        var listItem = $"{item.GetString("A_MERCHANTID")}{item.GetString("B_TRMINALID")}{item.GetString("C_ECORDERID")}{item.GetString("D_EMPTY")}{item.GetString("E_MOENY")}{item.GetString("F_AUTHNO")}{item.GetString("G_AUTHCODE")}{item.GetString("H_AUTHDATE")}{item.GetString("I_PRIVATE")}{item.GetString("J_EMPTY")}{item.GetString("K_EMPTY_RETURN")}";
                        txtList.Add(listItem);
                    }
                    listStr = Convert.ToString(ViewBag.List.Count).PadLeft(12, '0');
                    sumStr = Convert.ToString(sumPayMoney).PadLeft(12, '0');
                    logger.Debug("Export StreamWriter start");
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName, false, Encoding.GetEncoding(950)))
                    {
                        // 檔頭
                        writer.WriteLine($"H{merchant}{dt}{dtRan}{listStr}+{sumStr}{headerEmpty}");
                        //以 Hex Code 0x0D 0x0A 結尾(換行符號)
                        //請款明細
                        foreach (var item in txtList)
                        {
                            writer.WriteLine(item);
                            //以 Hex Code 0x0D 0x0A 結尾(換行符號)
                        }
                    }
                    logger.Debug("Export StreamWriter end");
                    //var sw = new StreamWriter(filePath+"big5.dat", false, Encoding.GetEncoding(950));
                    //// 檔頭
                    //sw.WriteLine($"H{merchant}{dt}{dtRan}{listStr}+{sumStr}{headerEmpty}");
                    ////以 Hex Code 0x0D 0x0A 結尾(換行符號)
                    ////請款明細
                    //foreach (var item in txtList)
                    //{
                    //    sw.WriteLine(item);
                    //    //以 Hex Code 0x0D 0x0A 結尾(換行符號)
                    //}
                    //sw.Close();

                    // check hexcode
                    FileStream fsOpen = new FileStream(fileName, FileMode.Open);
                    logger.Debug("Export check hexcode");
                    int hexIn;
                    string hex = string.Empty;
                    for (int i = 0; (hexIn = fsOpen.ReadByte()) != -1; i++)
                    {
                        hex += string.Format("{0:X2}", hexIn);
                    }
                    byte[] bodyDefaultByte = Encoding.Default.GetBytes(hex);//將字串轉為byte[]
                    byte[] bodyEncodingByte = Encoding.Convert(Encoding.Default, Encoding.GetEncoding(950), bodyDefaultByte);//進行轉碼,參數1,來源編碼,參數二,目標編碼,參數三,欲編碼變數
                    logger.Debug("fsOpen.Dispose");
                    fsOpen.Dispose();
                    logger.Debug($"PAY_EC.txt({dt}{dtRan}):{hex}");

                    ////var t = Encoding.GetEncoding(950).GetString(bodyEncodingByte);
                    //FileStream fs = new FileStream(fileName, FileMode.Create);
                    //fs.Write(bodyEncodingByte, 0, bodyEncodingByte.Length);
                    //fs.Close();

                    // 文字編碼
                    //Response.ContentEncoding = Encoding.GetEncoding(950);
                    logger.Debug("Response Clear");
                    Response.Clear();
                    Response.AddHeader("content-disposition", $"attachment; filename={merchant}.dat");
                    Response.ContentType = "text/html";
                    Response.Charset = "UTF-8";
                    logger.Debug("Response WriteFile");
                    Response.WriteFile(fileName);
                    Response.End();
                    logger.Debug("Response End");
                }
                else
                {
                    logger.Warn("PayECModel查無可請款資料");
                    sm.LastErrorMessage = "查無可請款資料!!";
                }

            }
            catch (Exception ex)
            {
                logger.Error("exportECDetailList:" + ex.Message, ex);
                sm.LastErrorMessage = "匯出失敗!!";
            }
            //return View("Index", model);
        }

        /// <summary>
        /// 匯入請款回覆檔
        /// </summary>
        /// <param name="Text_UploadFile"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PayFileUpload(HttpPostedFileBase Text_UploadFile)
        {
            var msg = string.Empty;
            if (Text_UploadFile != null)
            {
                MemoryStream ms = new MemoryStream();

                try
                {
                    if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                    {
                        DateTime now = DateTime.Now;

                        string dir = DataUtils.GetConfig("FOLDER_TEMPLATE") + "PAY" + "\\";
                        string filename = System.IO.Path.GetFileName(Request.Files[0].FileName);

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        Request.Files[0].SaveAs(dir + now.ToString("yyyyMMddHHmmss") + "_" + filename);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn(e.Message, e);
                    TempData["tempMessage"] = "上傳失敗!!";
                }

                try
                {
                    List<RspModel> rspList = new List<RspModel>();
                    List<string> rowlist = new List<string>();

                    using (StreamReader reader = new StreamReader(Text_UploadFile.InputStream, Encoding.GetEncoding(950)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // do something with line
                            rowlist.Add(line);
                        }
                        reader.Close();
                    }
                    if (rowlist.Count > 0 && rowlist != null)
                    {
                        foreach (var row in rowlist)
                        {
                            RspModel rsp = new RspModel();

                            rsp.A_001_010_10 = row.Substring(0, 10);
                            rsp.B_011_018_8 = row.Substring(10, 8);
                            rsp.C_019_058_40 = row.Substring(18, 40);
                            rsp.D_059_077_19 = row.Substring(58, 19);
                            rsp.E_078_085_8 = row.Substring(77, 8);
                            rsp.F_086_093_8 = row.Substring(85, 8);
                            rsp.G_094_095_2 = row.Substring(93, 2);
                            rsp.H_096_103_8 = row.Substring(95, 8);
                            rsp.I_104_119_16 = row.Substring(103, 16);
                            rsp.J_120_159_40 = row.Substring(119, 20); //卡人資訊
                            rsp.K_160_165_6 = row.Substring(139, 6);
                            rsp.L_166_168_3 = row.Substring(145, 3);
                            rsp.M_169_184_16 = row.Substring(148, 12); //請款成功
                            rsp.N_185_190_6 = row.Substring(160, 6);
                            rsp.O_191_191_1 = row.Substring(166, 1);
                            rsp.P_192_193_2 = row.Substring(167, 2);
                            rsp.Q_194_201_8 = row.Substring(169, 8);
                            rsp.R_202_209_8 = row.Substring(177, 8);
                            rsp.S_210_215_6 = row.Substring(185, 6);
                            rsp.T_216_223_8 = row.Substring(191, 8);
                            rsp.U_224_224_1 = row.Substring(199, 1);
                            rsp.V_225_232_8 = row.Substring(200, 8);
                            rsp.W_233_242_10 = row.Substring(208, 10);
                            rsp.X_243_250_8 = row.Substring(218, 8);
                            rsp.Y_251_251_1 = row.Substring(226, 1);
                            rsp.Z_252_252_1 = row.Substring(227, 1);
                            rsp.ZA_253_253_1 = row.Substring(228, 1);
                            //rsp.ZB_254_270_17 = row.Substring(233, 17);
                            rspList.Add(rsp);
                        }

                        using (SqlConnection conn = GetConnection())
                        {
                            conn.Open();
                            SqlTransaction tran = conn.BeginTransaction();

                            PayAction action = new PayAction(conn, tran);
                            PayDAO dao = new PayDAO();
                            int count = action.UpdateECDetailFile(rspList, GetAccount());

                            if (count > 0)
                            {
                                tran.Commit();
                            }
                            else
                            {
                                tran.Rollback();
                            }

                            logger.Debug("請款檔回傳總共" + rowlist.Count + "。匯入成功，總共 " + rspList.Count + " 筆，成功匯入 " + count + "筆。");
                            TempData["tempMessage"] = "匯入成功，總共 " + rowlist.Count + " 筆，成功匯入 " + count + "筆。";
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    TempData["tempMessage"] = "匯入失敗!!";
                }
                finally
                {
                    ms.Dispose();
                    ms.Close();
                }
            }
            return RedirectToAction("Index", "PayEC", new PayECModel());
        }

        /// <summary>
        /// 匯出
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportRspList(PayECModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["tempMessage"] = "查詢驗證有誤!!";
                ViewBag.tempMessage = TempData["tempMessage"];
                return Index();
            }

            PayAction action = new PayAction();
            System.Data.DataTable dt = action.CreateExcelPayEC(form);

            // 產生 Excel 資料流。
            MemoryStream ms = new MemoryStream();
            ms = ReportUtils.RenderDataTableToExcelPayEC(dt);

            return File(ms, "application/unknown", $"PayECList_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xls");
        }

        #region 檔案請款
        /// <summary>
        /// 產生請款檔 排程
        /// </summary>
        public void RequestFileUploadSchedule()
        {
            //登錄950 big5
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var big5 = Encoding.GetEncoding(950);
            logger.Debug($"Name : {big5.EncodingName} , CodePage :{big5.CodePage}");
            //Console.ReadKey();
            //顯示所有Encoding
            //var encodings = Encoding.GetEncodings();
            //foreach (var t in encodings)
            //{
            //    logger.Debug($"DisplayName:{t.DisplayName}, codePage: {t.CodePage}");
            //}

            PayAction action = new PayAction();
            var sumPayMoney = 0;
            var merchant = DataUtils.GetConfig("PAY_EC_MERCHANTID");
            var dt = DateTime.Now.ToString("yyyyMMdd");
            var dtRan = new Random().Next(999999999).ToString().PadLeft(10, '0');
            var listStr = string.Empty;
            var sumStr = string.Empty;
            var headerEmpty = "".PadLeft(216, ' ');
            var txtList = new List<string>();
            var filePath = $@"{DataUtils.GetConfig("FOLDER_TEMPLATE")}Pay\";
            var fileName = $"{filePath}{merchant}.dat";
            PayECModel model = new PayECModel();
            model.queryModel.APP_TIME_BEGIN = DateTime.Today.AddDays(-1);
            model.queryModel.APP_TIME_END = DateTime.Today;
            // 請款明細
            ViewBag.List = action.exportECDetailList(model.queryModel);

            if (ViewBag.List != null && ViewBag.List.Count > 0)
            {
                // 已存在檔案-刪除
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                foreach (var item in ViewBag.List)
                {
                    // 請款總金額
                    sumPayMoney += item.GetInt("PAY_MONEY");
                    // 請款明細
                    var listItem = $"{item.GetString("A_MERCHANTID")}{item.GetString("B_TRMINALID")}{item.GetString("C_ECORDERID")}{item.GetString("D_EMPTY")}{item.GetString("E_MOENY")}{item.GetString("F_AUTHNO")}{item.GetString("G_AUTHCODE")}{item.GetString("H_AUTHDATE")}{item.GetString("I_PRIVATE")}{item.GetString("J_EMPTY")}{item.GetString("K_EMPTY_RETURN")}";
                    txtList.Add(listItem);
                }
                listStr = Convert.ToString(ViewBag.List.Count).PadLeft(12, '0');
                sumStr = Convert.ToString(sumPayMoney).PadLeft(12, '0');

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName, false, Encoding.GetEncoding(950)))
                {
                    // 檔頭
                    writer.WriteLine($"H{merchant}{dt}{dtRan}{listStr}+{sumStr}{headerEmpty}");
                    //以 Hex Code 0x0D 0x0A 結尾(換行符號)
                    //請款明細
                    foreach (var item in txtList)
                    {
                        writer.WriteLine(item);
                        //以 Hex Code 0x0D 0x0A 結尾(換行符號)
                    }
                }
                // check hexcode
                FileStream fsOpen = new FileStream(fileName, FileMode.Open);
                int hexIn;
                string hex = string.Empty;
                for (int i = 0; (hexIn = fsOpen.ReadByte()) != -1; i++)
                {
                    hex += string.Format("{0:X2}", hexIn);
                }
                byte[] bodyDefaultByte = Encoding.Default.GetBytes(hex);//將字串轉為byte[]
                byte[] bodyEncodingByte = Encoding.Convert(Encoding.Default, Encoding.GetEncoding(950), bodyDefaultByte);//進行轉碼,參數1,來源編碼,參數二,目標編碼,參數三,欲編碼變數
                fsOpen.Dispose();
                logger.Debug($"PAY_EC.txt({dt}{dtRan}):{hex}");
                logger.Debug("RequestFileUploadSchedule start");
                //執行bat
                string filename = "FileUpload5.bat";
                string parameters = $@"{DataUtils.GetConfig("FOLDER_TEMPLATE")}Pay\";
                ProcessCmd(filename, parameters);
                logger.Debug("RequestFileUploadSchedule end");
            }
            else
            {
                logger.Warn("PayECModel查無可請款資料");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="parameters"></param>
        public void ProcessCmd(string filename, string parameters)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + parameters + filename);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
        }
        /// <summary>
        /// 處理請款回覆檔 排程
        /// </summary>
        /// <returns></returns>
        public void ProcessRequestFileDownloadSchedule()
        {
            logger.Debug("ProcessRequestFileDownloadSchedule start");
            //執行bat
            string filename = "FileDownload5.bat";
            string parameters = $@"{DataUtils.GetConfig("FOLDER_TEMPLATE")}Pay\";
            ProcessCmd(filename, parameters);
            logger.Debug("ProcessRequestFileDownloadSchedule end");

        }
        /// <summary>
        /// 請款回附檔排程回寫資料表
        /// </summary>
        public void ECFileReadLineSchedule()
        {
            string parameters = $@"{DataUtils.GetConfig("FOLDER_TEMPLATE")}Pay\";
            DirectoryInfo directoryInfo = new DirectoryInfo(parameters);
            // GetFiles(string searchPattern): 加入指定的Pattern，比對檔案名稱
            var directoryFiles = directoryInfo.GetFiles("*.rsp");
            var dt = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");

            foreach (var file in directoryFiles)
            {
                // Name: 檔案名稱
                Console.WriteLine(file.Name);
                if (file.Name.Contains(dt))
                {
                    MemoryStream ms = new MemoryStream();
                    try
                    {
                        //Open file for Read\Write
                        FileStream fs = file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                        List<RspModel> rspList = new List<RspModel>();
                        List<string> rowlist = new List<string>();

                        using (StreamReader reader = new StreamReader(fs, Encoding.GetEncoding(950)))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                // do something with line
                                rowlist.Add(line);
                            }
                            reader.Close();
                        }
                        if (rowlist.Count > 0 && rowlist != null)
                        {
                            foreach (var row in rowlist)
                            {
                                RspModel rsp = new RspModel();

                                rsp.A_001_010_10 = row.Substring(0, 10);
                                rsp.B_011_018_8 = row.Substring(10, 8);
                                rsp.C_019_058_40 = row.Substring(18, 40);
                                rsp.D_059_077_19 = row.Substring(58, 19);
                                rsp.E_078_085_8 = row.Substring(77, 8);
                                rsp.F_086_093_8 = row.Substring(85, 8);
                                rsp.G_094_095_2 = row.Substring(93, 2);
                                rsp.H_096_103_8 = row.Substring(95, 8);
                                rsp.I_104_119_16 = row.Substring(103, 16);
                                rsp.J_120_159_40 = row.Substring(119, 20); //卡人資訊
                                rsp.K_160_165_6 = row.Substring(139, 6);
                                rsp.L_166_168_3 = row.Substring(145, 3);
                                rsp.M_169_184_16 = row.Substring(148, 12); //請款成功
                                rsp.N_185_190_6 = row.Substring(160, 6);
                                rsp.O_191_191_1 = row.Substring(166, 1);
                                rsp.P_192_193_2 = row.Substring(167, 2);
                                rsp.Q_194_201_8 = row.Substring(169, 8);
                                rsp.R_202_209_8 = row.Substring(177, 8);
                                rsp.S_210_215_6 = row.Substring(185, 6);
                                rsp.T_216_223_8 = row.Substring(191, 8);
                                rsp.U_224_224_1 = row.Substring(199, 1);
                                rsp.V_225_232_8 = row.Substring(200, 8);
                                rsp.W_233_242_10 = row.Substring(208, 10);
                                rsp.X_243_250_8 = row.Substring(218, 8);
                                rsp.Y_251_251_1 = row.Substring(226, 1);
                                rsp.Z_252_252_1 = row.Substring(227, 1);
                                rsp.ZA_253_253_1 = row.Substring(228, 1);
                                //rsp.ZB_254_270_17 = row.Substring(233, 17);
                                rspList.Add(rsp);
                            }
                            if (rspList.Count > 0)
                            {
                                using (SqlConnection conn = GetConnection())
                                {
                                    conn.Open();
                                    SqlTransaction tran = conn.BeginTransaction();

                                    PayAction action = new PayAction(conn, tran);
                                    PayDAO dao = new PayDAO();
                                    logger.Debug("UpdateECDetailFile");
                                    int count = action.UpdateECDetailFile(rspList, "system");

                                    if (count > 0)
                                    {
                                        logger.Debug("Commit");
                                        tran.Commit();
                                    }
                                    else
                                    {
                                        logger.Debug("Rollback");
                                        tran.Rollback();
                                    }
                                    logger.Debug("請款檔回傳總共" + rowlist.Count + "。匯入成功，總共 " + rspList.Count + " 筆，成功匯入 " + count + "筆。");
                                    conn.Close();
                                    conn.Dispose();
                                }
                            }
                            else
                            {
                                logger.Debug("請款回覆檔無資料");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                    }
                    finally
                    {
                        ms.Dispose();
                        ms.Close();
                    }
                }
            }
        }
        #endregion
    }
}
