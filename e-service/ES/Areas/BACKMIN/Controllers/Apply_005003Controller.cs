using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using Newtonsoft.Json;
using ES.Commons;
using ES.DataLayers;
using ES.Services;
using ES.Areas.BACKMIN;
using ES.Areas.BACKMIN.Models;
using ES.Models.Entities;
using ES.Models;
using ICSharpCode.SharpZipLib.Zip;
using System.Globalization;
using Xceed.Words.NET;
using Spire.Doc;
using Spire.Doc.Documents;
//using Apply_005003ViewModel = ES.Areas.BACKMIN.Models.Apply_005003ViewModel;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_005003Controller : BaseController
    {
        public static string s_SRV_ID = "005003";
        public static string s_SRV_NAME = "WHO格式之產銷證明書(英文)";
        public static string s_zip_NAME = "WHO格式之產銷證明書(英文).ZIP";

        [DisplayName("005003_案件審理")]
        public ActionResult Index(string appid, string srvid)
        {
            //BackApplyDAO dao = new BackApplyDAO();
            //ShareDAO shareDao = new ShareDAO();
            Apply_005003ViewModel form = new Apply_005003ViewModel(appid);
            BackApplyDAO dao = new BackApplyDAO();
            form.Form = dao.QueryApply_005003(appid);

            //地址拆分
            //form.Form.TAX_ORG_CITY_CODE = form.Form.MF_ADDR.Substring(0, 5);
            //form.Form.TAX_ORG_CITY_DETAIL = form.Form.ADDR;
            //TblZIPCODE zip = new TblZIPCODE();
            //zip.ZIP_CO = form.Form.MF_ADDR.Substring(0, 5);
            //var getnam = dao.GetRow(zip);
            //if (getnam != null)
            //{
            //    form.Form.TAX_ORG_CITY_DETAIL = form.Form.MF_ADDR.Substring(5).Replace(getnam.CITYNM + getnam.TOWNNM, "");
            //    form.Form.TAX_ORG_CITY_TEXT = getnam.CITYNM + getnam.TOWNNM;
            //}

            form.F11.IsReadOnly = false;
            form.Form.F11.IsReadOnly = true;
            if (form.Form.CODE_CD == "2")
            {
                form.Detail = dao.GetApplyNotice_005003(appid);
                form.F11.IsReadOnly = true;
                form.Form.F11.IsReadOnly = true;
                form.Form.F11.IsNewOpen = false;
                form.Form.F11.IsDeleteOpen = false;
            }

            ////案件是否過期
            ShareDAO shareDao = new ShareDAO();
            form.Form.IS_CASE_LOCK = shareDao.CalculationDocDate(s_SRV_ID, appid);
            return View(form);
        }

        [DisplayName("005003_下載壓縮檔")]
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

            string zip_name1 = string.Format(@"../../Template/{0}_ZIP/", app_id);
            string zip_name1b = string.Format(@"../../Template/{0}_ZIP", app_id);
            string zip_name2 = string.Format(@"../../Template/{0}_ZIP/{1}", app_id, s_zip_NAME);
            string s_attach1 = string.Format("attachment; filename={0}", s_zip_NAME);

            string[] filenames = Directory.GetFiles(Server.MapPath(zip_name1));
            byte[] buffer = new byte[4096];

            using (ZipOutputStream zp = new ZipOutputStream(System.IO.File.Create(Server.MapPath(zip_name2))))
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
            Response.Headers["Content-Disposition"] = s_attach1;
            Response.TransmitFile(Server.MapPath(zip_name2));
            Response.Flush();
            Response.Close();

            // 刪除資料夾
            if (Directory.Exists(Server.MapPath(zip_name1b)))
            {
                string[] files = Directory.GetFiles(Server.MapPath(zip_name1b));
                string[] dirs = Directory.GetDirectories(Server.MapPath(zip_name1b));
                foreach (string item in files)
                {
                    System.IO.File.SetAttributes(item, FileAttributes.Normal);
                    System.IO.File.Delete(item);
                }

                Directory.Delete(Server.MapPath(zip_name1b));
            }
        }

        /// <summary>
        /// 005003_案件儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("005003_案件儲存")]
        [HttpPost]
        public ActionResult Savebk(Apply_005003ViewModel model)
        {
            //logger.Debug("##ActionResult Savebk(Apply_005003ViewModel model)");
            //Apply_005003FormModel model
            //Apply_005003ViewModel model
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (!ModelState.IsValid)
            {
                result.status = false;
                foreach (ModelState item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg = ErrorMsg + item.Errors[0].ErrorMessage + "\r\n";
                    }
                }
                result.status = false;
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }

            ModelState.Clear();
            //0:結案(回函核准) 1:新收案件 2:申請資料待確認 3:審查中 4:申請案補件中 10:已收案，處理中 20:結案(歉難同意)
            if ((model.Form.CODE_CD == "4" || model.Form.CODE_CD == "0" || model.Form.CODE_CD == "20") && string.IsNullOrEmpty(model.Form.MOHW_CASE_NO))
            {
                ErrorMsg += "請取得公文文號。";
                if (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE))
                {
                    ErrorMsg += "請輸入公文日期。";
                }
            }
            else if (!string.IsNullOrEmpty(model.Form.MOHW_CASE_NO) && (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE) || !model.Form.MOHW_CASE_DATE.Contains("/")))
            {
                ErrorMsg += "請輸入公文日期。";
            }
            //if ((model.Form.CODE_CD == "2" && !string.IsNullOrEmpty(model.Form.MOHW_CASE_NO)))
            //{
            //    if (string.IsNullOrEmpty(model.Form.MOHW_CASE_DATE))
            //    {
            //        ErrorMsg += "請輸入公文日期。";
            //    }
            //}
            #region 檢核
            if (model.Form.F_1_1.Trim().Substring(model.Form.F_1_1.Trim().Length - 1, 1) != ":")
            {
                ErrorMsg += "1.1處方說明最後請以「:」結尾。\r\n";
            }

            #region 檢核
            if (model.Form.F_1_1.Trim().Substring(model.Form.F_1_1.Trim().Length - 1, 1) != ":")
            {
                ErrorMsg += "1.1處方說明最後請以「:」結尾。\r\n";
            }

            // 1.2本產品是否獲准在國內販售？
            if (model.Form.F_1_2.TONotNullString() == "Y")
            {
                //* 是否為委託製造？
                if (model.Form.F_2A_2_COMM.TONotNullString() == "Y")
                {
                    if (model.Form.F_2A_2.TONotNullString() == "")
                    {
                        ErrorMsg += "請填寫2A.2藥商名稱 。\r\n";
                    }
                    if (model.Form.F_2A_2_ADDR.TONotNullString() == "")
                    {
                        ErrorMsg += "請填寫2A.2藥商地址 。\r\n";
                    }
                }

                if (model.Form.F_2A_3.TONotNullString() == "")
                {
                    ErrorMsg += "請勾選2A.3藥品許可證持有者之類別 。\r\n";
                }

                model.Form.F_2B_2 = null;
                model.Form.F_2B_3 = null;
                model.Form.F_2B_3_REMARKS = null;
            }
            else
            {
                if (model.Form.F_2B_2.TONotNullString() == "")
                {
                    ErrorMsg += "請至少勾選一種2B.2申請者之類別 。\r\n";
                }
                if (model.Form.F_2B_3.TONotNullString() == "")
                {
                    ErrorMsg += "請選擇2B.3僅供外銷專用之原因 。\r\n";
                }

                // 選否不需要有值
                model.Form.F_2A_2_COMM = null;
                model.Form.F_2A_2 = null;
                model.Form.F_2A_2_ADDR = null;

                model.Form.F_2A_3 = null;
                model.Form.F_2A_4 = null;
                model.Form.F_2A_5 = null;
                model.Form.F_2A_6 = null;
            }
            #endregion
            #endregion


            if (ErrorMsg == "")
            {
                //model.Form.ADDR = model.Form.TAX_ORG_CITY_TEXT + model.Form.TAX_ORG_CITY_DETAIL;
                //一般存檔
                try
                {
                    //存檔
                    ErrorMsg += dao.AppendApply005003(model);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    ErrorMsg = ex.Message;
                    //throw;
                }

                if (ErrorMsg == "")
                {
                    if (model.Form.CODE_CD == "2" || model.Form.CODE_CD == "4")
                    {
                        try
                        {
                            //補件存檔
                            ErrorMsg += dao.AppendApplyDoc005003(model);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.Message, ex);
                            ErrorMsg = ex.Message;
                            //throw;
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
                    try
                    {
                        dao.CaseFinishMail_005003(model);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        ErrorMsg = ex.Message;
                        //throw;
                    }
                }
            }
            else
            {
                result.status = false;
                result.message = ErrorMsg;
            }
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 005003_申請單套表
        /// </summary>
        /// <param name="form"></param>
        [HttpPost]
        [DisplayName("005003_申請單套表")]
        public void PreviewApplyForm(Apply_005003FormModel form)
        {
            ShareCodeListModel sc = new ShareCodeListModel();
            Apply_005003FormModel fm = form;
            BackApplyDAO dao = new BackApplyDAO();
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
                    doc.ReplaceText("[$IMP_COUNTRY$]", fm.MF_ADDR.Replace(",", ", ").TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1$]", fm.F_1.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$F_1_DF$]", fm.F_1_DF.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    var str = "";
                    Apply_005003_F11Model F11M = new Apply_005003_F11Model();
                    F11M.APP_ID = form.APP_ID;
                    var F11MDATA = dao.GetRowList(F11M);
                    foreach (var item in F11MDATA)
                    {
                        if (item.F11_QUANTITY.TONotNullString() == "")
                        {
                            item.F11_QUANTITY = "0";
                        }
                        str += item.F11_SCI_NM + " " + item.F11_QUANTITY + " " + item.F11_UNIT + ", ";
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
                    doc.ReplaceText("[$F_2A_1_DATE_F$]", F_2A_1_DATE_US.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
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

        /// <summary>
        /// 取得介接資料
        /// </summary>
        /// <param name="Form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetInterfaceData(Apply_005003FormModel Form)
        {
            var result = new AjaxResultStruct();
            BackApplyDAO dao = new BackApplyDAO();

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
                        var Data = JsonConvert.DeserializeObject<Apply_005003RemoteDataModel>(temp);

                        if (Data.STATUS == "成功")
                        {
                            //資料處理
                            Data.WHOPMPE = dao.SortData005003(Data.WHOPMPE);

                            result.status = true;
                            result.message = "成功";
                            result.data = Data.WHOPMPE;
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
