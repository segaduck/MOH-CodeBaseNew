using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Utils;
using log4net;
using ES.Action;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Services;
using ES.Models.ViewModels;
using ES.Helpers;
using System.Data;
using System.IO;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Net;
using Omu.ValueInjecter;
using Xceed.Words.NET;
using Xceed.Document.NET;
using DocumentFormat.OpenXml.EMMA;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace ES.Controllers
{
    public class AJAXController : BaseController
    {
        #region HELPER 控件

        #region GetCityTown 傳回郵遞區號中文名稱
        /// <summary>
        /// Ajax 傳回郵遞區號中文名稱
        /// </summary>
        /// <param name="CODE">郵遞區號</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCityTown(string CODE)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();
            KeyMapModel kmm = dao.GetCityTownName(CODE);
            result.data = (kmm != null ? kmm.TEXT : "");

            return Content(result.Serialize(), "application/json");
        }
        #endregion

        #region GetCityTown 傳回郵遞區號中文名稱
        /// <summary>
        /// Ajax 傳回郵遞區號中文名稱
        /// </summary>
        /// <param name="CODE">郵遞區號</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCityTownZip(string Address)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();
            KeyMapModel kmm = dao.GetCityTownZIP(Address);
            if (kmm != null)
            {
                result.status = true;
                result.data = kmm.CODE;
                result.message = kmm.TEXT;
            }
            else
            {
                result.status = false;
                result.message = "未有該地址的郵遞區號，請根據後方帶出地址自行查詢";
            }


            return Content(result.Serialize(), "application/json");
        }
        #endregion

        #endregion

        /// <summary>
        /// 取得鄉鎮市區下拉
        /// </summary>
        /// <param name="cityCode">縣市代碼</param>
        /// <returns></returns>
        public JsonResult GetTownList(string cityCode)
        {
            List<SelectListItem> list = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                list = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", cityCode, null, true);
                conn.Close();
                conn.Dispose();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得案件名稱下拉
        /// </summary>
        /// <param name="id">分類代碼</param>
        /// <returns></returns>
        public JsonResult GetServiceList(int id)
        {
            List<SelectListItem> list = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                list = CodeUtils.GetServiceList(conn, id, null, true);
                conn.Close();
                conn.Dispose();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsDate(string birthday)
        {
            return Json(CheckUtils.IsDate(birthday), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 是否為身分證號格式
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public JsonResult IsIdentity(string identity)
        {
            if (identity != null && identity.Length != 10)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(CheckUtils.IsIdentity(identity), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 檢查帳號是否存在
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public JsonResult CheckAccountExists(string account)
        {
            logger.Debug("CheckAccountExists: " + account);
            var result = false;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                MemberAction action = new MemberAction(conn);
                result = action.CheckAccountExists(account);
                conn.Close();
                conn.Dispose();
            }
            return Json(!result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 檢查身分證號是否存在
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public JsonResult CheckIdentityExists(string identity)
        {
            logger.Debug("CheckIdentityExists: " + identity);
            var result = false;
            if (identity != null && identity.Length != 10)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                MemberAction action = new MemberAction(conn);
                result = action.CheckIdentityExists(identity);
                conn.Close();
                conn.Dispose();
            }
            return Json(!result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCodeList(string id)
        {
            var result = new List<SelectListItem>();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                result = CodeUtils.GetCodeSelectList(conn, Request.Form["Code"].ToString(), id, null, true);
                conn.Close();
                conn.Dispose();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetF_LICENSE_CD_1(string id)
        {
            var result = new List<SelectListItem>();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                result = CodeUtils.GetCodeSelectList(conn, "F_LICENSE_CD_1", id, null, true);
                conn.Close();
                conn.Dispose();
            }
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetF_LICENSE_CD_2(string id)
        {
            var result = new List<SelectListItem>();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                result = CodeUtils.GetCodeSelectList(conn, "F_LICENSE_CD_2", id, null, true);
                conn.Close();
                conn.Dispose();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 岸口
        /// </summary>
        [HttpPost]
        public ActionResult GetHarborList(string port)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();

            IList<KeyMapModel> list = dao.GetHarborMapList(port);
            result.data = list;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 證書(專科醫師證書補（換）發)
        /// </summary>
        [HttpPost]
        public ActionResult GetLicNumList(string code_pcd)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();

            IList<KeyMapModel> list = dao.GetLicNumList(code_pcd);
            result.data = list;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 證書類別
        /// </summary>
        [HttpPost]
        public ActionResult GetLicNumCertList(string code_pcd)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();

            IList<KeyMapModel> list = dao.GetLicNumCertList(code_pcd);
            result.data = list;
            return Content(result.Serialize(), "application/json");
        }

        [HttpPost]
        public ActionResult GetLicNumCertMDList(string code_pcd)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();

            IList<KeyMapModel> list = dao.GetLicNumCertMDList(code_pcd);
            result.data = list;
            return Content(result.Serialize(), "application/json");
        }

        [HttpPost]
        public ActionResult GetCERTCATEMDList(string code_pcd)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();

            IList<KeyMapModel> list = dao.GetCERTCATEMDList(code_pcd);
            result.data = list;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 取得會員資料
        /// </summary>
        [HttpPost]
        public ActionResult GetMemberData()
        {
            var result = new AjaxResultStruct();
            SessionModel sm = SessionModel.Get();

            IList<KeyMapModel> list = new List<KeyMapModel>();

            if (sm.UserInfo != null)
            {
                KeyMapModel item = new KeyMapModel();
                item.CODE = "UserNo";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.UserNo) ? "" : sm.UserInfo.UserNo;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "NAME";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.NAME) ? "" : sm.UserInfo.Member.NAME;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "CNT_NAME";
                //item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.CNT_NAME) ? "" : sm.UserInfo.Member.CNT_NAME;
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.NAME) ? "" : sm.UserInfo.Member.NAME;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "MAIL";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.MAIL) ? "" : sm.UserInfo.Member.MAIL;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "TEL";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.TEL) ? "" : sm.UserInfo.Member.TEL;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "MOBILE";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.MOBILE) ? "" : sm.UserInfo.Member.MOBILE;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "CNT_TEL";
                //item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.CNT_TEL) ? "" : sm.UserInfo.Member.CNT_TEL;
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.TEL) ? "" : sm.UserInfo.Member.TEL;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "FAX";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.FAX) ? "" : sm.UserInfo.Member.FAX;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "CITY_CD";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.CITY_CD) ? "" : sm.UserInfo.Member.CITY_CD;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "TOWN_CD";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.TOWN_CD) ? "" : sm.UserInfo.Member.TOWN_CD;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "ADDR";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.ADDR) ? "" : sm.UserInfo.Member.ADDR;
                list.Add(item);

                item = new KeyMapModel();
                item.CODE = "MOBILE";
                item.TEXT = string.IsNullOrEmpty(sm.UserInfo.Member.MOBILE) ? "" : sm.UserInfo.Member.MOBILE;
                list.Add(item);
            }

            result.data = list;
            return Content(result.Serialize(), "application/json");
        }

        #region 下載檔案
        /// <summary>
        /// 下載檔案
        /// </summary>
        /// <returns></returns>
        public ActionResult DownLoadsFILE(string FILENAME, string APP_ID, string FILE_NO, string SRC_NO)
        {
            ShareDAO dao = new ShareDAO();
            byte[] dbyte = null;

            string srcFileName = "";
            string srvID = "";
            ApplyModel applywhere = new ApplyModel();
            applywhere.APP_ID = APP_ID;
            ApplyModel apply = dao.GetRow(applywhere);

            if (apply != null)
            {
                srvID = apply.SRV_ID;
            }

            if (srvID == "001008" && FILENAME.IndexOf("_ATH_UP_") > 0)
            {
                //20201105 add for 醫事人員或公共衛生師請領英文證明書-醫事人員或公共衛生師/專科中文證書電子檔(file from apply_001008_ath)
                Apply_001008_AthModel athwhere = new Apply_001008_AthModel();
                athwhere.APP_ID = APP_ID;
                athwhere.SRL_NO = FILE_NO.TOInt32();
                Apply_001008_AthModel athfile = dao.GetRow(athwhere);

                if (athfile != null && !string.IsNullOrWhiteSpace(athfile.ATH_UP))
                {
                    dbyte = dbyte = dao.sftpDownload(athfile.ATH_UP);
                    int pos = athfile.ATH_UP.LastIndexOf("\\");

                    srcFileName = (string.IsNullOrWhiteSpace(athfile.SRC_FILENAME) ? athfile.ATH_UP.Substring(pos + 1, athfile.ATH_UP.Length - pos - 1) : athfile.SRC_FILENAME);
                }

                string s_log1 = "";
                s_log1 += string.Format("\n #dbyte.Length:{0}", dbyte.Length);
                s_log1 += string.Format("\n #srcFileName:{0}", srcFileName);
                s_log1 += string.Format("\n #DownLoadsFILE(FILENAME:{0}, APP_ID:{1}, FILE_NO:{2}, SRC_NO:{3})", FILENAME, APP_ID, FILE_NO, SRC_NO);
                logger.Debug(s_log1);

                // 將結果回傳
                return File(dbyte, "application/octet-stream", srcFileName);
            }
            else
            {

                //file from apply_file
                Apply_File_LogModel fileWhere = new Apply_File_LogModel();
                fileWhere.APP_ID = APP_ID;
                fileWhere.FILE_NO = FILE_NO.TOInt32();
                //if (!string.IsNullOrEmpty(SRC_NO) && SRC_NO.TONotNullString() != "0")
                //{
                //    fileWhere.SRC_NO = SRC_NO.TOInt32();
                //}
                var files = dao.GetRowList(fileWhere);
                foreach (var item in files)
                {
                    var repName = item.FILENAME.ToUpper().Replace('/', '\\');
                    var itemNum = repName.LastIndexOf('\\');
                    var comName = FILENAME.ToUpper();
                    if (repName.Substring(itemNum + 1, repName.Length - itemNum - 1) == comName)
                    {
                        dbyte = dao.sftpDownload(item.FILENAME);
                    }
                }

                string s_log1 = "";
                s_log1 += string.Format("\n #dbyte.Length:{0}", dbyte.Length);
                s_log1 += string.Format("\n #DownLoadsFILE(FILENAME:{0}, APP_ID:{1}, FILE_NO:{2}, SRC_NO:{3})", FILENAME, APP_ID, FILE_NO, SRC_NO);
                logger.Debug(s_log1);

                // 將結果回傳
                return File(dbyte, "application/octet-stream", FILENAME);
            }
        }

        /// <summary>
        /// 下載尚未進入DB的檔案
        /// </summary>
        /// <returns></returns>
        public ActionResult DownLoadsFrontFILE(string FILENAME)
        {
            ShareDAO dao = new ShareDAO();
            var dbyte = dao.sftpDownload(FILENAME);

            // 將結果回傳
            return File(dbyte, "application/octet-stream", "檔案下載");
        }
        #endregion

        #region 取得公文文號
        /// <summary>
        /// 收文整合
        /// </summary>
        /// <param name="UNIT_CD"></param>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public ActionResult GetMOHW_CASE_NO_PostIncoming(string UNIT_CD, string APP_ID)
        {
            var result = new AjaxResultStruct();
            SessionModel sm = SessionModel.Get();
            ShareDAO dao = new ShareDAO();
            DocumentUtils du = new DocumentUtils();
            result.status = false;
            var myGuid = Guid.NewGuid();
            byte[] bArr = myGuid.ToByteArray();
            int autonum = Math.Abs(BitConverter.ToInt32(bArr, 0));
            var MOHW_CASE_NO = autonum.ToString();
            var smUsr = sm.UserInfo.Admin;

            try
            {
                var model_APP_ID = dao.GetApplyColumn(APP_ID, "");
                var srv_id = dao.GetApplyColumn(APP_ID, "SRV_ID");
                var unit_name = dao.GetApplyColumn(APP_ID, "UNIT_NAME");
                var address = dao.GetApplyColumn(APP_ID, "ADDRESS");
                var fax = dao.GetApplyColumn(APP_ID, "FAX");
                var mail = dao.GetApplyColumn(APP_ID, "MAIL");
                var name = dao.GetApplyColumn(APP_ID, "CNT_NAME");
                var tel = dao.GetApplyColumn(APP_ID, "TEL");
                var srvName = dao.GetServiceName(APP_ID.Substring(8, 6));
                var files = dao.GetFileGridListTop(APP_ID);
                if (srv_id == "005014")
                {
                    files = dao.GetFileGridListTop_005014(APP_ID);
                }
                var dimodel = new ES.Areas.Admin.Models.DocumentExportModel.DocumentModel()
                {
                    APP_ID = APP_ID,
                    ADDRESS = address,
                    SUBJECT = $"請惠准辦理。",
                    CAPTION1 = $"{srvName}線上申請{APP_ID}",
                    FAX = fax,
                    MAIL = mail,
                    NAME = name,
                    SRV_ID = srv_id,
                    TEL = tel,
                    UNIT_NAME = unit_name,
                };
                if (srv_id == "005013")
                {
                    dimodel = dao.GetDIColumn005013(APP_ID);
                }
                else if (srv_id == "005014")
                {
                    dimodel = dao.GetDIColumn005014(APP_ID);
                }
                var rDpno = string.Empty;
                var diUsr = dao.GetPRO_ACC(APP_ID);
                var epno = dao.GetEPNO(diUsr);
                switch (UNIT_CD)
                {

                    // 中醫藥司
                    case "7":
                        rDpno = "A10";
                        break;
                    // 醫事司
                    case "4":
                        rDpno = "A08";
                        break;
                    // 資訊室
                    default:
                        rDpno = "B06";
                        break;
                }
                #region 介接資料

                //產生公文DI檔
                int ranNum = new Random().Next(1, 99);
                var APP_ID_RAN = $"{APP_ID}{ranNum.ToString()}";
                var xml_content = new Areas.Admin.Controllers.DocumentExportController().GenerateDIXml(dimodel, files, "申請書", APP_ID_RAN);
                byte[] docx = null;
                switch (srv_id)
                {
                    case "005013":
                        this.ExportApplyDocx005013(APP_ID);
                        //切結書
                        this.ExportAffidavitDocx005013(APP_ID);
                        break;
                    case "005014":
                        this.ExportApplyDocx005014(APP_ID);
                        //切結書
                        //一般
                        this.ExportAffidavitDocx005014_1(APP_ID);
                        //萃取物
                        this.ExportAffidavitDocx005014_2(APP_ID);
                        break;
                }

                //產生的公文DI檔包成ZIP
                byte[] di_content_zip64 = du.GetDI_ZIP(APP_ID, APP_ID_RAN, docx, files, srv_id);
                logger.Debug("di_content_zip64:" + di_content_zip64);
                // 儲存DI傳送內容
                PostIncomingPOSTModel DBCPost = new PostIncomingPOSTModel();
                //DBCPost.SysID = $"{rDpno}-{A10_seq}";//單位代碼-流水號
                DBCPost.SysID = $"B06-001";
                DBCPost.VerifyCode = "BNA81CSCXQ";//驗證碼
                DBCPost.FileContent = di_content_zip64;//公文DI>ZIP>BASE64編碼
                DBCPost.Memo = APP_ID.TONotNullString();
                // 儲存資料庫
                DOC_DISaveModel saveData = new DOC_DISaveModel();
                saveData.APP_ID = APP_ID;
                saveData.di_content_b64 = di_content_zip64;
                saveData.di_filename = APP_ID_RAN;
                saveData.di_status = "公文DI";
                saveData.EmployeeCode = epno;
                saveData.FileContent = MOHW_CASE_NO;
                saveData.SysID = DBCPost.SysID;
                saveData.UnitCode = rDpno;
                saveData.VerifyCode = "";
                saveData.xml_content = xml_content;
                logger.Debug("InsertOFFICIAL_DOC_DI_PostIncoming:Start");
                dao.InsertOFFICIAL_DOC_DI(saveData);
                #endregion

                #region 介接資訊
                string byteArray = JsonConvert.SerializeObject(DBCPost);
                string APIURL = DataUtils.GetConfig("PostIncomingUrl");
                logger.Debug("webRequest.Create");
                var webRequest = WebRequest.Create(APIURL) as HttpWebRequest;
                logger.Debug($"APIURL:{APIURL}");
                webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                webRequest.ContentType = "application/json; charset=UTF-8";
                webRequest.Method = "POST";
                //webRequest.ContentLength = 0;
                webRequest.Accept = "*/*";
                //webRequest.Connection = "keep-alive"; //Keep-Alive 和 Close 可能未用此屬性設定。
                webRequest.KeepAlive = false;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                using (var reqStream = new StreamWriter(webRequest.GetRequestStream()))
                {
                    logger.Debug("reqStream.StreamWriter");
                    var temp = byteArray;
                    logger.Debug(temp);
                    reqStream.Write(temp);
                }
                #endregion

                // 是否開啟公文DI檔
                var isOpenDI = DataUtils.GetConfig("DOC_WEBSERVICE_DI_OPEN");
                if (isOpenDI.TONotNullString() == "Y")
                {
                    #region 介接回傳
                    logger.Debug("_PostIncoming:webRequest.GetResponse().GetResponseStream()");
                    using (var s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var contributorsAsJson = sr.ReadToEnd();
                            var JsonResult = JsonConvert.DeserializeObject<PostIncomingResultModel>(contributorsAsJson);
                            var DBCResult = new PostIncomingResultModel();
                            DBCResult.Success = JsonResult.Success;
                            DBCResult.Token = JsonResult.Token;
                            DBCResult.ErrorMsg = JsonResult.ErrorMsg;
                            logger.Debug($"_PostIncomingResultModel:{JsonResult.Success},{JsonResult.Token},{JsonResult.ErrorMsg}");
                            // 回寫回傳結果
                            saveData.Success = DBCResult.Success ? "1" : "0";
                            saveData.Token = DBCResult.Token;
                            saveData.ErrorMsg = DBCResult.ErrorMsg;
                            logger.Debug($"UpdateOFFICIAL_DOC_DI");
                            dao.UpdateOFFICIAL_DOC_DI(saveData);
                            dao.UpdateTOKEN(APP_ID, saveData.Token);
                            logger.Debug("Update Token to Apply:" + APP_ID + " token:" + saveData.Token);

                            if (JsonResult.Success)
                            {
                                result.status = true;
                                result.message = "收文整合服務時，流程尚未至公文系統總收文，無公文資訊，需待收文後方可查詢";
                            }
                            else
                            {
                                result.status = false;
                                result.message = $"收文失敗{saveData.ErrorMsg}，請聯絡系統管理員 !";
                                logger.Error(result.message);
                            }
                        }
                    }
                    logger.Debug("_PostIncoming:End");
                    #endregion 介接回傳
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                result.status = false;
                result.message = "收文失敗，請聯絡系統管理員 !";
            }
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 文稿整合(創文取號) 醫事司
        /// </summary>
        /// <param name="UNIT_CD"></param>
        /// <param name="APP_ID"></param>
        /// <returns></returns>
        public ActionResult GetMOHW_CASE_NO(string UNIT_CD, string APP_ID)
        {
            var result = new AjaxResultStruct();
            SessionModel sm = SessionModel.Get();
            ShareDAO dao = new ShareDAO();
            DocumentUtils du = new DocumentUtils();
            result.status = false;
            var myGuid = Guid.NewGuid();
            byte[] bArr = myGuid.ToByteArray();
            int autonum = Math.Abs(BitConverter.ToInt32(bArr, 0));
            var MOHW_CASE_NO = autonum.ToString();
            try
            {
                var rDpno = string.Empty;
                var diUsr = dao.GetPRO_ACC(APP_ID);
                var epno = dao.GetEPNO(diUsr);
                switch (UNIT_CD)
                {
                    // 中醫藥司
                    case "7":
                        rDpno = "A10";
                        break;
                    // 醫事司
                    case "4":
                        rDpno = "A08";
                        break;
                    // 資訊室
                    default:
                        rDpno = "B06";
                        break;
                }

                #region 介接資料
                byte[] docx = null;
                var smUsr = sm.UserInfo.Admin;
                var model_APP_ID = dao.GetApplyColumn(APP_ID, "");
                var srv_id = dao.GetApplyColumn(APP_ID, "SRV_ID");
                var unit_name = dao.GetApplyColumn(APP_ID, "UNIT_NAME");
                var address = dao.GetApplyColumn(APP_ID, "ADDRESS");
                var fax = dao.GetApplyColumn(APP_ID, "FAX");
                var mail = dao.GetApplyColumn(APP_ID, "MAIL");
                var name = dao.GetApplyColumn(APP_ID, "NAME");
                var tel = dao.GetApplyColumn(APP_ID, "TEL");
                var app_time = dao.GetApplyColumn(APP_ID, "APP_TIME");
                var srvName = dao.GetServiceName(APP_ID.Substring(8, 6));
                var files = dao.GetFileGridListTop(APP_ID);
                var dimodel = new ES.Areas.Admin.Models.DocumentExportModel.DocumentModel()
                {
                    APP_ID = APP_ID,
                    NAME = name,
                    SRV_ID = srv_id,
                    UNIT_NAME = unit_name,
                };
                switch (srv_id)
                {
                    case "001009":
                        var certcate001009 = dao.GetApplyColumn(APP_ID, "001009_APPLY_CERT_CATE_NAME");
                        //醫事人員或公共衛生師資格英文求證
                        dimodel.SUBJECT = $"有關{name}{certcate001009}申請醫事人員或公共衛生師資格英文求證一案，簽請核示。";
                        dimodel.CAPTION1 = $"依據本部「人民申請案件線上申辦系統」{app_time}{APP_ID}案號辦理。";
                        dimodel.CAPTION2 = "旨揭申請書及所附文件內容，與本部醫事管理系統之資料核對相符，依所請填復國外機構求證申請書。";
                        dimodel.CAPTION3 = "：陳請司長簽署國外機構求證申請書後，再以紙本郵寄至申請人指定收件地址。";
                        dimodel.CAPTION4 = "會辦單位：第二層決行 07-011"; //會辦單位
                        dimodel.CAPTION5 = app_time; // 年月日
                        break;
                    case "001037":
                        var certcate001037 = dao.GetApplyColumn(APP_ID, "001037_APPLY_CERT_CATE_NAME");
                        //醫事人員或公共衛生師請領無懲戒紀錄證明申請書
                        dimodel.SUBJECT = $"有關{name}{certcate001037}申請發給醫事人員無懲戒紀錄證明書一案，簽請核示。";
                        dimodel.CAPTION1 = $"依據{name}{certcate001037}{app_time}本部人民申請案件線上申辦系統案件編號{APP_ID}辦理。";
                        dimodel.CAPTION2 = "經查本部醫事人員管理系統，申請人符合醫事人員無懲戒紀錄證明書核發原則。";
                        dimodel.CAPTION3 = "：陳請司長簽署無懲戒紀錄證明書，再以紙本郵寄至指定收件地址。";
                        dimodel.CAPTION4 = "會辦單位：第二層決行 07-028"; //會辦單位
                        dimodel.CAPTION5 = app_time; // 年月日
                        break;
                    case "001039":
                        var certcate001039 = dao.GetApplyColumn(APP_ID, "001039_APPLY_CERT_CATE_NAME");
                        //醫師赴國外訓練英文保證函
                        dimodel.SUBJECT = $"有關{name}{certcate001039}申請發給英文保證函一案，簽請核示。";
                        dimodel.CAPTION1 = $"依據{name}{certcate001039}{app_time}本部人民申請案件線上申辦系統案件編號{APP_ID}辦理。";
                        dimodel.CAPTION2 = "本案醫師已依照英文保證函核發原則規定，繳交醫院保證函、申請人書面保證函、醫師證書影本、國外契約或接受文件、E.C.F.M.G.及格證書影本、國民身分證正反面影本、護照影本、個人執業發展規劃書等相關文件。";
                        dimodel.CAPTION3 = "：奉核後核發英文保證函，並函知外交部，另以電子郵件傳送英文保證函電子檔至美國ECFMG，擬附稿如後，併陳核示。";
                        dimodel.CAPTION4 = "會辦單位：第二層決行 07-028"; //會辦單位
                        dimodel.CAPTION5 = app_time; // 年月日
                        //段落 三
                        dimodel.CAPTION6 = "美國ECFMG公告可接受電子郵件方式傳遞英文保證函，為利申請人縮短簽證辦理時間，爰同時以電子郵件方式傳遞英文保證函電子掃描檔至美國ECFMG。";
                        break;
                }
                //產生公文DI檔
                int ranNum = new Random().Next(1, 99);
                var APP_ID_RAN = $"{APP_ID}{ranNum.ToString()}";
                var xml_content = new Areas.Admin.Controllers.DocumentExportController().GenerateDIXml_A08(dimodel, files, APP_ID_RAN);
                //產生的公文DI檔包成ZIP
                byte[] di_content_zip64 = du.GetDI_ZIP_A08(APP_ID, APP_ID_RAN, docx, files);
                logger.Debug("di_content_zip64:" + di_content_zip64);
                // 創文流水號
                var A10_seq = DataUtils.GetConfig("DraftByCreateSeqA10").PadLeft(3, '0');
                logger.Debug($"DraftByCreateSeqA10:{A10_seq}");
                // 儲存DI傳送內容
                DraftByCreatePOSTModel DBCPost = new DraftByCreatePOSTModel();
                //DBCPost.SysID = $"{rDpno}-{A10_seq}";//單位代碼-流水號
                DBCPost.SysID = $"B06-001";
                DBCPost.VerifyCode = "BNA81CSCXQ";//驗證碼
                                                  //DBCPost.UnitCode = rDpno;//單位代碼(部門)
                DBCPost.UnitCode = rDpno;
                DBCPost.EmployeeCode = epno;//承辦人資訊
                DBCPost.FileContent = di_content_zip64;//公文DI>ZIP>BASE64編碼
                                                       // 儲存資料庫
                DOC_DISaveModel saveData = new DOC_DISaveModel();
                saveData.APP_ID = APP_ID;
                saveData.di_content_b64 = di_content_zip64;
                saveData.di_filename = APP_ID_RAN;
                saveData.di_status = "公文DI";
                saveData.EmployeeCode = epno;
                saveData.FileContent = MOHW_CASE_NO;
                saveData.SysID = DBCPost.SysID;
                saveData.UnitCode = rDpno;
                saveData.VerifyCode = "";
                saveData.xml_content = xml_content;
                logger.Debug("InsertOFFICIAL_DOC_DI:Start");
                dao.InsertOFFICIAL_DOC_DI(saveData);
                #endregion

                #region 介接資訊
                string byteArray = JsonConvert.SerializeObject(DBCPost);
                string APIURL = DataUtils.GetConfig("DraftByCreateUrl"); //創文取號
                logger.Debug("webRequest.Create");
                var webRequest = WebRequest.Create(APIURL) as HttpWebRequest;
                logger.Debug($"APIURL:{APIURL}");
                webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                webRequest.ContentType = "application/json; charset=UTF-8";
                webRequest.Method = "POST";
                //webRequest.ContentLength = 0;
                webRequest.Accept = "*/*";
                //webRequest.Connection = "keep-alive"; //Keep-Alive 和 Close 可能未用此屬性設定。
                webRequest.KeepAlive = false;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                using (var reqStream = new StreamWriter(webRequest.GetRequestStream()))
                {
                    logger.Debug("reqStream.StreamWriter");
                    var temp = byteArray;
                    logger.Debug(temp);
                    reqStream.Write(temp);
                }
                #endregion

                // 是否開啟公文DI檔
                var isOpenDI = DataUtils.GetConfig("DOC_WEBSERVICE_DI_OPEN");
                if (isOpenDI.TONotNullString() == "Y")
                {
                    #region 介接回傳
                    logger.Debug("DraftByCreate:webRequest.GetResponse().GetResponseStream()");
                    using (var s = webRequest.GetResponse().GetResponseStream())
                    {
                        using (var sr = new StreamReader(s))
                        {
                            var contributorsAsJson = sr.ReadToEnd();
                            var JsonResult = JsonConvert.DeserializeObject<DraftByCreateResultModel>(contributorsAsJson);
                            var DBCResult = new DraftByCreateResultModel();
                            DBCResult.Success = JsonResult.Success;
                            DBCResult.Token = JsonResult.Token;
                            DBCResult.ObjectId = JsonResult.ObjectId;
                            DBCResult.ErrorMsg = JsonResult.ErrorMsg;
                            logger.Debug($"DraftByCreateResultModel:{JsonResult.Success},{JsonResult.Token},{JsonResult.ObjectId},{JsonResult.ErrorMsg}");
                            // 回寫回傳結果
                            saveData.Success = DBCResult.Success ? "1" : "0";
                            saveData.Token = DBCResult.Token;
                            saveData.ObjectId = DBCResult.ObjectId;
                            saveData.ErrorMsg = DBCResult.ErrorMsg;
                            logger.Debug($"UpdateOFFICIAL_DOC_DI");
                            dao.UpdateOFFICIAL_DOC_DI(saveData);
                            logger.Debug($"UpdateDraftByCreateSeqA10");
                            dao.UpdateDraftByCreateSeqA10(A10_seq);
                            if (JsonResult.Success)
                            {
                                MOHW_CASE_NO = DBCResult.ObjectId;
                                //取得公文時，紀錄取得時間
                                dao.InsertCaseNo(APP_ID, MOHW_CASE_NO);
                                result.status = true;
                                result.message = MOHW_CASE_NO;
                            }
                            else
                            {
                                result.status = false;
                                result.message = $"取號失敗{saveData.ErrorMsg}，請聯絡系統管理員 !";
                                logger.Error(result.message);
                            }
                        }
                    }
                    logger.Debug("DraftByCreate:End");
                    #endregion 介接回傳
                }

                #region 舊的公文取號
                //string url = DataUtils.GetConfig("DOC_WEBSERVICE_URL");
                //string act = DataUtils.GetConfig("DOC_WEBSERVICE_GET");
                //string[] args = new string[1];
                //args[0] = rDpno;
                //// 依據各司代碼取回公文文號，取得相關值中
                //object retdata = WebServiceHelper.InvokeWebService(url, "", act, args);
                //bool tFlag = (bool)retdata.GetType().GetField("tFlag").GetValue(retdata);
                //var tMsg = (string)retdata.GetType().GetField("tMsg").GetValue(retdata);
                //var dNo = (string)retdata.GetType().GetField("dNo").GetValue(retdata);

                //if (tFlag)
                //{
                //    logger.Info($"WebServiceHelper:Success.{tMsg}/{dNo}.");
                //    MOHW_CASE_NO = dNo;
                //    //取得公文時，紀錄取得時間
                //    dao.InsertCaseNo(APP_ID, MOHW_CASE_NO);

                //    // 判斷資料夾是否存在
                //    du.FileDI();
                //    // 是否開啟公文DI檔
                //    var isOpenDI = DataUtils.GetConfig("DOC_WEBSERVICE_DI_OPEN");
                //    if (isOpenDI.TONotNullString() == "Y")
                //    {
                //        // 傳送DI檔案 WebService di2wpf_dmc
                //        var smUsr = sm.UserInfo.Admin;
                //        var srvName = dao.GetServiceName(APP_ID.Substring(8, 6));
                //        //產生公文DI檔
                //        var xml_content = dao.CreateDI($"{smUsr.NAME} {smUsr.TEL}", "", smUsr.MAIL, $"{srvName}線上申請{APP_ID}");
                //        logger.Info($"xml_content:{xml_content}");
                //        var di_filename = $"{rDpno}.{smUsr.ACC_NO}.{dNo}.DI";
                //        logger.Info($"di_filename:{di_filename}");
                //        byte[] di_content_b64 = du.DownLoadXmlFileStream(xml_content, di_filename);
                //        logger.Info($"DownLoadXmlFileStream:{Convert.ToBase64String(di_content_b64)}");
                //        logger.Info($"srvName:{srvName},APP_ID:{APP_ID}");
                //        string act_DI = DataUtils.GetConfig("DOC_WEBSERVICE_DI");
                //        string[] args_DI = new string[2];
                //        args_DI[0] = di_filename;
                //        args_DI[1] = Convert.ToBase64String(di_content_b64);
                //        logger.Info($"InvokeWebService:{act_DI.TONotNullString()}");
                //        object retdata_DI = WebServiceHelper.InvokeWebService(url, "", act, args);
                //        var tMsg_DI = (string)retdata.GetType().GetField("string").GetValue(retdata);
                //        logger.Info($"InvokeWebService:{tMsg_DI}");
                //        // 儲存DI傳送內容
                //        dao.InsertOFFICIAL_DOC_DI(di_filename, di_content_b64, tMsg_DI, APP_ID, xml_content);
                //        logger.Info($"InsertOFFICIAL_DOC_DI:{di_filename}");
                //    }
                //    result.status = true;
                //    result.message = MOHW_CASE_NO;
                //}
                //else
                //{
                //    logger.Error("取號失敗，請聯絡系統管理員 !");
                //    result.status = false;
                //    result.message = "取號失敗，請聯絡系統管理員 !";
                //}
                #endregion

                if (true)
                {
                    logger.Info($"WebServiceHelper:Success..{MOHW_CASE_NO}.");
                    result.status = true;
                    result.message = MOHW_CASE_NO;
                }
                //else
                //{
                //    result.status = false;
                //    result.message = "取號失敗，請聯絡系統管理員 !";
                //}

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                result.status = false;
                result.message = "取號失敗，請聯絡系統管理員 !";
            }
            return Content(result.Serialize(), "application/json");
        }


        public ActionResult GetMOHW_Token(string APP_ID)
        {
            var result = new AjaxResultStruct();
            SessionModel sm = SessionModel.Get();
            ShareDAO dao = new ShareDAO();
            result.status = false;
            var tokenarry = dao.GetApplyColumn(APP_ID, "token");
            logger.Debug("GetMOHW_Token.getToken:" + tokenarry);
            if (string.IsNullOrEmpty(tokenarry))
            {
                result.status = false;
                result.message = "未取得公文Token，請先操作收文整合。";
                return Content(result.Serialize(), "application/json");
            }

            try
            {
                var rDpno = string.Empty;
                var diUsr = dao.GetPRO_ACC(APP_ID);
                var epno = dao.GetEPNO(diUsr);

                #region 介接資料

                // 儲存DI傳送內容
                DataQueryPOSTModel DBCPost = new DataQueryPOSTModel();
                //DBCPost.SysID = $"{rDpno}-{A10_seq}";//單位代碼-流水號
                DBCPost.SysID = $"B06-001";
                //DBCPost.VerifyCode = "XCDE45BCE4";//驗證碼
                DBCPost.VerifyCode = "BNA81CSCXQ";
                List<string> list = new List<string>();
                list.Add(tokenarry);
                DBCPost.TokenArray = list.ToArray();
                // 儲存資料庫
                DOC_DISaveModel saveData = new DOC_DISaveModel();
                saveData.APP_ID = APP_ID;
                saveData.di_status = "公文查詢";
                saveData.EmployeeCode = epno;
                saveData.SysID = DBCPost.SysID;
                saveData.UnitCode = rDpno;
                saveData.VerifyCode = "";
                saveData.xml_content = DBCPost.TokenArray.ToString();
                logger.Debug("InsertOFFICIAL_DOC_DI:Start");
                dao.InsertOFFICIAL_DOC_DI(saveData);
                #endregion
                logger.Debug($"DataQueryPOSTModel:{JsonConvert.SerializeObject(DBCPost)}");

                #region 介接資訊
                string byteArray = JsonConvert.SerializeObject(DBCPost);
                string APIURL = DataUtils.GetConfig("DataQueryUrl");
                logger.Debug("webRequest.Create");
                var webRequest = WebRequest.Create(APIURL) as HttpWebRequest;
                logger.Debug($"APIURL:{APIURL}");
                webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                webRequest.ContentType = "application/json; charset=UTF-8";
                webRequest.Method = "POST";
                //webRequest.ContentLength = 0;
                webRequest.Accept = "*/*";
                //webRequest.Connection = "keep-alive"; //Keep-Alive 和 Close 可能未用此屬性設定。
                webRequest.KeepAlive = false;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
                using (var reqStream = new StreamWriter(webRequest.GetRequestStream()))
                {
                    var temp = byteArray;
                    reqStream.Write(temp);
                }
                #endregion

                #region 介接回傳
                logger.Debug("DataQuery:webRequest.GetResponse().GetResponseStream()");
                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        var contributorsAsJson = sr.ReadToEnd();
                        logger.Debug("contributorsAsJson:" + contributorsAsJson);
                        var JsonResult = JsonConvert.DeserializeObject<DataQueryResultModel>(contributorsAsJson);
                        logger.Debug("JsonResult:" + JsonResult);
                        var DBCResult = new DataQueryResultModel();
                        DBCResult.Success = JsonResult.Success;
                        DBCResult.Data = JsonResult.Data;
                        logger.Debug("JsonResult.Data:" + JsonResult.Data);
                        DBCResult.ErrorMsg = JsonResult.ErrorMsg;
                        logger.Debug("JsonResult.ErrorMsg:" + JsonResult.ErrorMsg);
                        var DBResult = new DataQueryBasicModel();
                        if (string.IsNullOrEmpty(JsonResult.ErrorMsg) && JsonResult.Data != null && JsonResult.Data.Count() > 0)
                        {
                            logger.Debug("JsonResult.Data.Token:" + JsonResult.Data[0].Token);
                            DBResult.Token = JsonResult.Data[0].Token;
                            DBResult.ObjectId = JsonResult.Data[0].ObjectId;
                            logger.Debug($"DataQueryResultModel:{JsonResult.Success},{DBResult.Token},{DBResult.ObjectId},{JsonResult.ErrorMsg}");
                        }
                        else
                        {
                            logger.Debug("JsonResult.Data is null");
                            result.status = false;
                            result.message = "請於公文流程後取回公文文號。";
                            return Content(result.Serialize(), "application/json");
                        }
                        // 回寫回傳結果
                        saveData.Success = DBCResult.Success ? "1" : "0";
                        saveData.Token = DBResult.Token;
                        saveData.ObjectId = DBResult.ObjectId;
                        saveData.ErrorMsg = DBCResult.ErrorMsg;
                        logger.Debug($"UpdateOFFICIAL_DOC_DI");
                        dao.UpdateOFFICIAL_DOC_DI(saveData);
                        if (JsonResult.Success)
                        {
                            //取得公文時，紀錄取得時間
                            dao.InsertCaseNo(APP_ID, DBResult.ObjectId);
                            result.status = true;
                            result.message = DBResult.ObjectId;
                        }
                        else
                        {
                            result.status = false;
                            result.message = $"取號失敗{saveData.ErrorMsg}(token:{tokenarry})，請聯絡系統管理員 !";
                        }
                    }
                }
                logger.Debug("DataQuery:End");
                #endregion 介接回傳
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                result.status = false;
                result.message = "取號失敗，請聯絡系統管理員 !";
            }
            return Content(result.Serialize(), "application/json");
        }
        #endregion


        #region Apply_001038_GoodsModel
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow001038(GoodsDynamicGrid<Apply_001038_GoodsViewModel> Goods)
        {
            SessionModel sm = SessionModel.Get();
            if (Goods != null)
            {
                if (Goods.GoodsList != null)
                {
                    Apply_001038_GoodsViewModel temp = new Apply_001038_GoodsViewModel();
                    var i = 0;
                    foreach (var item in Goods.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                    }
                    temp.SRL_NO = i + 1;
                    Goods.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001038", Goods);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow001038(GoodsDynamicGrid<Apply_001038_GoodsViewModel> Goods, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (Goods != null)
            {
                if (Goods.GoodsList != null)
                {
                    if (Goods.GoodsList.ToCount() > 1)
                    {
                        Goods.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = Goods.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001038", Goods);
        }
        #endregion

        #region Apply_001035_GoodsModel
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow001035(GoodsDynamicGrid<Apply_001035_GoodsModel> Goods)
        {
            SessionModel sm = SessionModel.Get();
            if (Goods != null)
            {
                if (Goods.GoodsList != null)
                {
                    Apply_001035_GoodsModel temp = new Apply_001035_GoodsModel();
                    Goods.GoodsList.Add(temp);

                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001035", Goods);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow001035(GoodsDynamicGrid<Apply_001035_GoodsModel> Goods, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (Goods != null)
            {
                if (Goods.GoodsList != null)
                {
                    if (Goods.GoodsList.ToCount() > 1)
                    {
                        Goods.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = Goods.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001035", Goods);
        }
        #endregion

        #region Apply_005001_DIModel
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow005001(GoodsDynamicGrid<Apply_005001_DiViewModel> Di)
        {
            SessionModel sm = SessionModel.Get();
            if (Di != null)
            {
                if (Di.GoodsList != null)
                {
                    Apply_005001_DiViewModel temp = new Apply_005001_DiViewModel();
                    var i = 0;
                    foreach (var item in Di.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                        item.DI_CONT = item.DI_CONT.TONotNullString().Replace("+", " ");
                        item.DI_ENAME = item.DI_ENAME.TONotNullString().Replace("+", " ");
                        item.DI_NAME = item.DI_NAME.TONotNullString().Replace("+", " ");
                        item.DI_UNIT = item.DI_UNIT.TONotNullString().Replace("+", " ");
                    }
                    temp.SRL_NO = i + 1;
                    Di.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001", Di);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpGoodRow005001(GoodsDynamicGrid<Apply_005001_DiViewModel> Di, int idx)
        {
            SessionModel sm = SessionModel.Get();
            foreach (var item in Di.GoodsList)
            {
                item.DI_CONT = item.DI_CONT.TONotNullString().Replace("+", " ");
                item.DI_ENAME = item.DI_ENAME.TONotNullString().Replace("+", " ");
                item.DI_NAME = item.DI_NAME.TONotNullString().Replace("+", " ");
                item.DI_UNIT = item.DI_UNIT.TONotNullString().Replace("+", " ");
            }
            if (Di != null)
            {
                if (Di.GoodsList != null)
                {

                    var UpRows = Di.GoodsList.Where(m => m.SRL_NO < idx).OrderByDescending(m => m.SRL_NO).ToList();

                    if (UpRows.ToCount() > 0)
                    {
                        var Row = Di.GoodsList.Where(m => m.SRL_NO == idx).ToList().FirstOrDefault();
                        var UpRow = UpRows.FirstOrDefault();
                        Row.SRL_NO = UpRow.SRL_NO;
                        UpRow.SRL_NO = idx;
                    }
                    Di.GoodsList = Di.GoodsList.OrderBy(m => m.SRL_NO).ToList();
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001", Di);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DownGoodRow005001(GoodsDynamicGrid<Apply_005001_DiViewModel> Di, int idx)
        {
            SessionModel sm = SessionModel.Get();
            foreach (var item in Di.GoodsList)
            {
                item.DI_CONT = item.DI_CONT.TONotNullString().Replace("+", " ");
                item.DI_ENAME = item.DI_ENAME.TONotNullString().Replace("+", " ");
                item.DI_NAME = item.DI_NAME.TONotNullString().Replace("+", " ");
                item.DI_UNIT = item.DI_UNIT.TONotNullString().Replace("+", " ");
            }
            if (Di != null)
            {
                if (Di.GoodsList != null)
                {

                    var DownRows = Di.GoodsList.Where(m => m.SRL_NO > idx).OrderBy(m => m.SRL_NO).ToList();

                    if (DownRows.ToCount() > 0)
                    {
                        var Row = Di.GoodsList.Where(m => m.SRL_NO == idx).ToList().FirstOrDefault();
                        var DownRow = DownRows.FirstOrDefault();
                        Row.SRL_NO = DownRow.SRL_NO;
                        DownRow.SRL_NO = idx;
                    }
                    Di.GoodsList = Di.GoodsList.OrderBy(m => m.SRL_NO).ToList();
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001", Di);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow005001(GoodsDynamicGrid<Apply_005001_DiViewModel> Di, int idx)
        {
            SessionModel sm = SessionModel.Get();
            foreach (var item in Di.GoodsList)
            {
                item.DI_CONT = item.DI_CONT.TONotNullString().Replace("+", " ");
                item.DI_ENAME = item.DI_ENAME.TONotNullString().Replace("+", " ");
                item.DI_NAME = item.DI_NAME.TONotNullString().Replace("+", " ");
                item.DI_UNIT = item.DI_UNIT.TONotNullString().Replace("+", " ");
            }
            if (Di != null)
            {
                if (Di.GoodsList != null)
                {
                    if (Di.GoodsList.ToCount() > 1)
                    {
                        if (Di.GoodsList.ToCount() == idx)
                        {
                            Di.GoodsList.RemoveAt(idx - 1);
                        }
                        else
                        {
                            Di.GoodsList.RemoveAt(idx);
                        }
                    }
                    else
                    {
                        var good = Di.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001", Di);
        }
        #endregion

        #region Apply_005001_PCModel
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow005001Pc(GoodsDynamicGrid<Apply_005001_PcViewModel> Pc)
        {
            SessionModel sm = SessionModel.Get();
            if (Pc != null)
            {
                if (Pc.GoodsList != null)
                {
                    Apply_005001_PcViewModel temp = new Apply_005001_PcViewModel();
                    var i = 0;
                    foreach (var item in Pc.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                        item.PC_CONT = item.PC_CONT.TONotNullString().Replace("+", " ");
                        item.PC_ENAME = item.PC_ENAME.TONotNullString().Replace("+", " ");
                        item.PC_NAME = item.PC_NAME.TONotNullString().Replace("+", " ");
                        item.PC_UNIT = item.PC_UNIT.TONotNullString().Replace("+", " ");
                    }
                    temp.SRL_NO = i + 1;
                    Pc.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001Pc", Pc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpGoodRow005001Pc(GoodsDynamicGrid<Apply_005001_PcViewModel> Pc, int idx)
        {
            SessionModel sm = SessionModel.Get();
            foreach (var item in Pc.GoodsList)
            {
                item.PC_CONT = item.PC_CONT.TONotNullString().Replace("+", " ");
                item.PC_ENAME = item.PC_ENAME.TONotNullString().Replace("+", " ");
                item.PC_NAME = item.PC_NAME.TONotNullString().Replace("+", " ");
                item.PC_UNIT = item.PC_UNIT.TONotNullString().Replace("+", " ");

            }
            if (Pc != null)
            {
                if (Pc.GoodsList != null)
                {

                    var UpRows = Pc.GoodsList.Where(m => m.SRL_NO < idx).OrderByDescending(m => m.SRL_NO).ToList();

                    if (UpRows.ToCount() > 0)
                    {
                        var Row = Pc.GoodsList.Where(m => m.SRL_NO == idx).ToList().FirstOrDefault();
                        var UpRow = UpRows.FirstOrDefault();
                        Row.SRL_NO = UpRow.SRL_NO;
                        UpRow.SRL_NO = idx;
                    }
                    Pc.GoodsList = Pc.GoodsList.OrderBy(m => m.SRL_NO).ToList();
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001Pc", Pc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DownGoodRow005001Pc(GoodsDynamicGrid<Apply_005001_PcViewModel> Pc, int idx)
        {
            SessionModel sm = SessionModel.Get();
            foreach (var item in Pc.GoodsList)
            {
                item.PC_CONT = item.PC_CONT.TONotNullString().Replace("+", " ");
                item.PC_ENAME = item.PC_ENAME.TONotNullString().Replace("+", " ");
                item.PC_NAME = item.PC_NAME.TONotNullString().Replace("+", " ");
                item.PC_UNIT = item.PC_UNIT.TONotNullString().Replace("+", " ");

            }
            if (Pc != null)
            {
                if (Pc.GoodsList != null)
                {

                    var DownRows = Pc.GoodsList.Where(m => m.SRL_NO > idx).OrderBy(m => m.SRL_NO).ToList();

                    if (DownRows.ToCount() > 0)
                    {
                        var Row = Pc.GoodsList.Where(m => m.SRL_NO == idx).ToList().FirstOrDefault();
                        var DownRow = DownRows.FirstOrDefault();
                        Row.SRL_NO = DownRow.SRL_NO;
                        DownRow.SRL_NO = idx;
                    }
                    Pc.GoodsList = Pc.GoodsList.OrderBy(m => m.SRL_NO).ToList();
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001Pc", Pc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow005001Pc(GoodsDynamicGrid<Apply_005001_PcViewModel> Pc, int idx)
        {
            SessionModel sm = SessionModel.Get();
            foreach (var item in Pc.GoodsList)
            {
                item.PC_CONT = item.PC_CONT.TONotNullString().Replace("+", " ");
                item.PC_ENAME = item.PC_ENAME.TONotNullString().Replace("+", " ");
                item.PC_NAME = item.PC_NAME.TONotNullString().Replace("+", " ");
                item.PC_UNIT = item.PC_UNIT.TONotNullString().Replace("+", " ");

            }
            if (Pc != null)
            {
                if (Pc.GoodsList != null)
                {
                    if (Pc.GoodsList.ToCount() > 1)
                    {
                        if (Pc.GoodsList.ToCount() == idx)
                        {
                            Pc.GoodsList.RemoveAt(idx - 1);
                        }
                        else
                        {
                            Pc.GoodsList.RemoveAt(idx);
                        }
                    }
                    else
                    {
                        var good = Pc.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }


            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid005001Pc", Pc);
        }
        #endregion

        #region Apply_005003_F11Model

        //GoodsDynamicGrid005003_F11.cshtml
        string s_GoodsDynamicGrid005003_F11 = "GoodsDynamicGrid005003";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow005003(GoodsDynamicGrid<Apply_005003_F11ViewModel> F11)
        {
            GoodsDynamicGrid<Apply_005003_F11ViewModel> oGrid = F11;
            SessionModel sm = SessionModel.Get();
            if (oGrid != null && oGrid.GoodsList != null)
            {
                Apply_005003_F11ViewModel temp = new Apply_005003_F11ViewModel();
                int i = 0;
                foreach (var item in oGrid.GoodsList)
                {
                    i++;
                    item.SRL_NO = i;
                    item.F11_SCI_NM = item.F11_SCI_NM.TONotNullString().Replace("+", " ");
                    item.F11_SCI_NAME = item.F11_SCI_NAME.TONotNullString().Replace("+", " ");
                    item.F11_QUANTITY = item.F11_QUANTITY.TONotNullString().Replace("+", " ");
                    item.F11_UNIT = item.F11_UNIT.TONotNullString().Replace("+", " ");

                }
                temp.SRL_NO = i + 1;
                oGrid.GoodsList.Add(temp);
            }
            //## 將結果回傳
            return PartialView(s_GoodsDynamicGrid005003_F11, oGrid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpGoodRow005003(GoodsDynamicGrid<Apply_005003_F11ViewModel> F11, int idx)
        {
            GoodsDynamicGrid<Apply_005003_F11ViewModel> oGrid = F11;
            SessionModel sm = SessionModel.Get();
            foreach (var item in oGrid.GoodsList)
            {
                item.F11_SCI_NM = item.F11_SCI_NM.TONotNullString().Replace("+", " ");
                item.F11_SCI_NAME = item.F11_SCI_NAME.TONotNullString().Replace("+", " ");
                item.F11_QUANTITY = item.F11_QUANTITY.TONotNullString().Replace("+", " ");
                item.F11_UNIT = item.F11_UNIT.TONotNullString().Replace("+", " ");

            }
            if (oGrid == null || oGrid.GoodsList == null)
            {
                //## 將結果回傳
                return PartialView(s_GoodsDynamicGrid005003_F11, oGrid);
            }

            List<Apply_005003_F11ViewModel> UpRows = oGrid.GoodsList.Where(m => m.SRL_NO < idx).OrderByDescending(m => m.SRL_NO).ToList();
            if (UpRows.ToCount() > 0)
            {
                Apply_005003_F11ViewModel Row = oGrid.GoodsList.Where(m => m.SRL_NO == idx).ToList().FirstOrDefault();
                Apply_005003_F11ViewModel UpRow = UpRows.FirstOrDefault();
                Row.SRL_NO = UpRow.SRL_NO;
                UpRow.SRL_NO = idx;
            }
            oGrid.GoodsList = oGrid.GoodsList.OrderBy(m => m.SRL_NO).ToList();
            //## 將結果回傳
            return PartialView(s_GoodsDynamicGrid005003_F11, oGrid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DownGoodRow005003(GoodsDynamicGrid<Apply_005003_F11ViewModel> F11, int idx)
        {
            GoodsDynamicGrid<Apply_005003_F11ViewModel> oGrid = F11;
            SessionModel sm = SessionModel.Get();
            foreach (var item in oGrid.GoodsList)
            {
                item.F11_SCI_NM = item.F11_SCI_NM.TONotNullString().Replace("+", " ");
                item.F11_SCI_NAME = item.F11_SCI_NAME.TONotNullString().Replace("+", " ");
                item.F11_QUANTITY = item.F11_QUANTITY.TONotNullString().Replace("+", " ");
                item.F11_UNIT = item.F11_UNIT.TONotNullString().Replace("+", " ");

            }
            if (oGrid == null || oGrid.GoodsList == null)
            {
                //## 將結果回傳
                return PartialView(s_GoodsDynamicGrid005003_F11, oGrid);
            }


            List<Apply_005003_F11ViewModel> DownRows = oGrid.GoodsList.Where(m => m.SRL_NO > idx).OrderBy(m => m.SRL_NO).ToList();
            if (DownRows.ToCount() > 0)
            {
                Apply_005003_F11ViewModel Row = oGrid.GoodsList.Where(m => m.SRL_NO == idx).ToList().FirstOrDefault();
                Apply_005003_F11ViewModel DownRow = DownRows.FirstOrDefault();
                Row.SRL_NO = DownRow.SRL_NO;
                DownRow.SRL_NO = idx;
            }
            oGrid.GoodsList = oGrid.GoodsList.OrderBy(m => m.SRL_NO).ToList();
            //## 將結果回傳
            return PartialView(s_GoodsDynamicGrid005003_F11, oGrid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow005003(GoodsDynamicGrid<Apply_005003_F11ViewModel> F11, int idx)
        {
            GoodsDynamicGrid<Apply_005003_F11ViewModel> oGrid = F11;
            SessionModel sm = SessionModel.Get();
            foreach (var item in oGrid.GoodsList)
            {
                item.F11_SCI_NM = item.F11_SCI_NM.TONotNullString().Replace("+", " ");
                item.F11_SCI_NAME = item.F11_SCI_NAME.TONotNullString().Replace("+", " ");
                item.F11_QUANTITY = item.F11_QUANTITY.TONotNullString().Replace("+", " ");
                item.F11_UNIT = item.F11_UNIT.TONotNullString().Replace("+", " ");

            }
            if (oGrid == null || oGrid.GoodsList == null)
            {
                //## 將結果回傳
                return PartialView(s_GoodsDynamicGrid005003_F11, oGrid);
            }

            if (oGrid.GoodsList.ToCount() > 1)
            {
                if (oGrid.GoodsList.ToCount() == idx)
                {
                    oGrid.GoodsList.RemoveAt(idx - 1);
                }
                else
                {
                    oGrid.GoodsList.RemoveAt(idx);
                }
            }
            else
            {
                Apply_005003_F11ViewModel good = oGrid.GoodsList.FirstOrDefault();
                var goodPi = good.GetType().GetProperties();
                foreach (var pi in goodPi)
                {
                    pi.SetValue(good, null);
                }
            }
            //## 將結果回傳
            return PartialView(s_GoodsDynamicGrid005003_F11, oGrid);
        }
        #endregion

        #region Apply_010001_GoodsModel
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow010001(GoodsDynamicGrid<APPLY_010001_APPFILModel> APPFIL)
        {
            SessionModel sm = SessionModel.Get();
            if (APPFIL != null)
            {
                if (APPFIL.GoodsList != null)
                {
                    APPLY_010001_APPFILModel temp = new APPLY_010001_APPFILModel();
                    var i = 0;
                    foreach (var item in APPFIL.GoodsList)
                    {
                        i++;
                        item.SEQ_NO = i;
                    }
                    temp.SEQ_NO = i + 1;
                    APPFIL.GoodsList.Add(temp);
                }

            }

            // 將結果回傳
            return PartialView("GoodsDynamicGrid010001", APPFIL);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow010001(GoodsDynamicGrid<APPLY_010001_APPFILModel> APPFIL, int idx)
        {
            SessionModel sm = SessionModel.Get();

            if (APPFIL != null)
            {
                if (APPFIL.GoodsList != null)
                {
                    if (APPFIL.GoodsList.ToCount() > 1)
                    {
                        APPFIL.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = APPFIL.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid010001", APPFIL);
        }
        #endregion

        #region Apply_010001_GoodsModel
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow012001(GoodsDynamicGrid<APPLY_012001_APPFILModel> APPFIL)
        {
            SessionModel sm = SessionModel.Get();
            if (APPFIL != null)
            {
                if (APPFIL.GoodsList != null)
                {
                    APPLY_012001_APPFILModel temp = new APPLY_012001_APPFILModel();
                    var i = 0;
                    foreach (var item in APPFIL.GoodsList)
                    {
                        i++;
                        item.SEQ_NO = i;
                    }
                    temp.SEQ_NO = i + 1;
                    APPFIL.GoodsList.Add(temp);
                }

            }

            // 將結果回傳
            return PartialView("GoodsDynamicGrid012001", APPFIL);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow012001(GoodsDynamicGrid<APPLY_012001_APPFILModel> APPFIL, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (APPFIL != null)
            {
                if (APPFIL.GoodsList != null)
                {
                    if (APPFIL.GoodsList.ToCount() > 1)
                    {
                        APPFIL.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = APPFIL.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid012001", APPFIL);
        }
        #endregion

        #region 醫事人員證書 Apply_001008_MeGoodsModel
        /// <summary>
        /// 新增一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow001008Me(GoodsDynamicGrid<Apply_001008_MeViewModel> Me)
        {
            SessionModel sm = SessionModel.Get();
            if (Me != null)
            {
                if (Me.GoodsList != null)
                {
                    Apply_001008_MeViewModel temp = new Apply_001008_MeViewModel();
                    var i = 0;
                    foreach (var item in Me.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                    }
                    temp.SRL_NO = i + 1;
                    Me.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Me", Me);
        }

        /// <summary>
        /// 刪除一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow001008Me(GoodsDynamicGrid<Apply_001008_MeViewModel> Me, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (Me != null)
            {
                if (Me.GoodsList != null)
                {
                    if (Me.GoodsList.ToCount() > 1)
                    {
                        Me.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = Me.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Me", Me);
        }
        #endregion

        #region 專科證書 Apply_001008_PRGoodsModel
        /// <summary>
        /// 新增一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow001008Pr(GoodsDynamicGrid<Apply_001008_PrViewModel> Pr)
        {
            SessionModel sm = SessionModel.Get();
            if (Pr != null)
            {
                if (Pr.GoodsList != null)
                {
                    Apply_001008_PrViewModel temp = new Apply_001008_PrViewModel();
                    var i = 0;
                    foreach (var item in Pr.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                    }
                    temp.SRL_NO = i + 1;
                    Pr.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Pr", Pr);
        }

        /// <summary>
        /// 刪除一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow001008Pr(GoodsDynamicGrid<Apply_001008_PrViewModel> Pr, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (Pr != null)
            {
                if (Pr.GoodsList != null)
                {
                    if (Pr.GoodsList.ToCount() > 1)
                    {
                        Pr.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = Pr.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Pr", Pr);
        }
        #endregion

        #region 國內寄送地址 Apply_001008_TransGoodsModel
        /// <summary>
        /// 新增一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow001008Trans(GoodsDynamicGrid<Apply_001008_TransViewModel> Trans)
        {
            SessionModel sm = SessionModel.Get();
            if (Trans != null)
            {
                if (Trans.GoodsList != null)
                {
                    Apply_001008_TransViewModel temp = new Apply_001008_TransViewModel();
                    var i = 0;
                    foreach (var item in Trans.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                        item.TRANS_ADDR = item.TRANS_ADDR.TONotNullString().Replace("+", " ");
                    }
                    temp.SRL_NO = i + 1;
                    Trans.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Trans", Trans);
        }

        /// <summary>
        /// 刪除一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow001008Trans(GoodsDynamicGrid<Apply_001008_TransViewModel> Trans, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (Trans != null)
            {
                if (Trans.GoodsList != null)
                {
                    var i = 0;
                    foreach (var item in Trans.GoodsList)
                    {
                        i++;
                        item.TRANS_ADDR = item.TRANS_ADDR.TONotNullString().Replace("+", " ");
                    }
                    if (Trans.GoodsList.ToCount() > 1)
                    {
                        Trans.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = Trans.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Trans", Trans);
        }
        #endregion

        #region 國外寄送地址 Apply_001008_TransFGoodsModel
        /// <summary>
        /// 新增一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow001008TransF(GoodsDynamicGrid<Apply_001008_TransFViewModel> TransF)
        {
            SessionModel sm = SessionModel.Get();
            if (TransF != null)
            {
                if (TransF.GoodsList != null)
                {
                    Apply_001008_TransFViewModel temp = new Apply_001008_TransFViewModel();
                    var i = 0;
                    foreach (var item in TransF.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                        item.TRANSF_ADDR = item.TRANSF_ADDR.TONotNullString().Replace("+", " ");
                        item.TRANSF_UNITNAME = item.TRANSF_UNITNAME.TONotNullString().Replace("+", " ");
                    }
                    temp.SRL_NO = i + 1;
                    TransF.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008TransF", TransF);
        }

        /// <summary>
        /// 刪除一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow001008TransF(GoodsDynamicGrid<Apply_001008_TransFViewModel> TransF, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (TransF != null)
            {
                if (TransF.GoodsList != null)
                {
                    var i = 0;
                    foreach (var item in TransF.GoodsList)
                    {
                        i++;
                        item.TRANSF_ADDR = item.TRANSF_ADDR.TONotNullString().Replace("+", " ");
                        item.TRANSF_UNITNAME = item.TRANSF_UNITNAME.TONotNullString().Replace("+", " ");
                    }
                    if (TransF.GoodsList.ToCount() > 1)
                    {
                        TransF.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = TransF.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008TransF", TransF);
        }
        #endregion

        #region 醫事人員中文證書電子檔 Apply_001008_AthGoodsModel
        /// <summary>
        /// 新增一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult addGoodRow001008Ath(GoodsDynamicGrid<Apply_001008_AthViewModel> Ath)
        {
            SessionModel sm = SessionModel.Get();
            if (Ath != null)
            {
                if (Ath.GoodsList != null)
                {
                    Apply_001008_AthViewModel temp = new Apply_001008_AthViewModel();
                    var i = 0;
                    foreach (var item in Ath.GoodsList)
                    {
                        i++;
                        item.SRL_NO = i;
                    }
                    temp.SRL_NO = i + 1;
                    Ath.GoodsList.Add(temp);
                }

            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Ath", Ath);
        }

        /// <summary>
        /// 刪除一筆
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult deleteGoodRow001008Ath(GoodsDynamicGrid<Apply_001008_AthViewModel> Ath, int idx)
        {
            SessionModel sm = SessionModel.Get();
            if (Ath != null)
            {
                if (Ath.GoodsList != null)
                {
                    if (Ath.GoodsList.ToCount() > 1)
                    {
                        Ath.GoodsList.RemoveAt(idx);
                    }
                    else
                    {
                        var good = Ath.GoodsList.FirstOrDefault();
                        var goodPi = good.GetType().GetProperties();
                        foreach (var pi in goodPi)
                        {
                            pi.SetValue(good, null);
                        }
                    }
                }
            }

            //## 將結果回傳
            return PartialView("GoodsDynamicGrid001008Ath", Ath);
        }
        #endregion

        #region 前台首頁表件下載 線上申辦 進入判斷登入方式是否符合
        /// <summary>
        /// 前台首頁表件下載 線上申辦 進入判斷登入方式是否符合
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ActionResult ApplyLoginCheck(string serviceId)
        {
            SessionModel sm = SessionModel.Get();
            bool flag_can_anyUse = false;//不限制登入方式
            string LOGIN_TYPE_TEST = CodeUtils.Utl_GetConfigSet("LOGIN_TYPE_TEST");
            if (LOGIN_TYPE_TEST.Equals("Y")) { flag_can_anyUse = true; }

            ShareDAO dao = new ShareDAO();
            TblSERVICE where = new TblSERVICE();
            where.SRV_ID = serviceId;
            TblSERVICE data = dao.GetRow(where);
            if (data == null)
            {
                sm.LastErrorMessage = "查無此申辦項目";
                return RedirectToAction("Index", "Service");
            }

            if (sm.UserInfo == null || sm.UserInfo.Member == null)
            {
                sm.LastErrorMessage = "尚未設定登入資訊，請先登入";
                return RedirectToAction("Index", "Service");
            }
            ClamMember mem = sm.UserInfo.Member;
            string logTag = "";
            if (mem.IDN.Length == 10) { logTag = "N"; }
            if (mem.IDN.Length == 8) { logTag = "L"; }

            // 可登入方式
            string caType = data.CA_TYPE ?? "";
            string apTarget = data.APP_TARGET ?? ""; //N/L
            bool isCA = false;
            bool isCA2 = false;
            if (apTarget.Length > 0 && logTag.Length > 0)
            {
                if (apTarget.Contains(logTag)) { isCA2 = true; }
            }
            if (!string.IsNullOrWhiteSpace(caType))
            {
                IList<string> typList = caType.ToSplit(',');
                string loginType = sm.UserInfo.LoginAuth;
                foreach (string item in typList)
                {
                    string s_item = (item ?? "").Trim();
                    switch (s_item)
                    {
                        // 帳密登入
                        case "MEMBER":
                            if (loginType == "MEMBER") { isCA = true; }
                            break;
                        // 自然人
                        case "MOICA":
                            if (loginType == "MOICA") { isCA = true; }
                            break;
                        // 工商
                        case "MOEACA":
                            if (loginType == "MOEACA") { isCA = true; }
                            break;
                        // 醫事憑證
                        case "HCA0":
                            if (loginType == "HCA0") { isCA = true; }
                            break;
                        // 醫事憑證
                        case "HCA1":
                            if (loginType == "HCA1") { isCA = true; }
                            break;
                        // 數位身分證
                        case "NEWEID":
                            if (loginType == "NEWEID") { isCA = true; }
                            break;
                    }
                }
            }
            else
            {
                sm.LastErrorMessage = "申請方式未設定";
                return RedirectToAction("Index", "Service");
            }

            //不限制登入方式
            if (flag_can_anyUse && !isCA) { isCA = true; }
            if (!isCA)
            {
                sm.LastErrorMessage = "該申辦項目無法使用此登入方式，請重新登入。";
                return RedirectToAction("Index", "Service");
            }
            if (flag_can_anyUse && !isCA2) { isCA2 = true; }
            if (!isCA2)
            {
                if (logTag.Equals("N"))
                {
                    sm.LastErrorMessage = "此申辦項目「自然人會員」無法申請。";
                }
                else if (logTag.Equals("L"))
                {
                    sm.LastErrorMessage = "此申辦項目「法人會員」無法申請。";
                }
                else
                {
                    sm.LastErrorMessage = "此申辦項目「申辦資格不符」無法申請。";
                }
                return RedirectToAction("Index", "Service");
            }

            //if (isCA)
            return RedirectToAction("Apply", "APPLY_" + serviceId);

        }
        #endregion

        /// <summary>
        /// 處理進度(以單位)
        /// </summary>
        [HttpPost]
        public ActionResult GetFlowCDListByUnit(string unit_cd)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();

            IList<KeyMapModel> list = dao.GetFlowCDListByUnit(unit_cd);
            result.data = list;
            return Content(result.Serialize(), "application/json");
        }

        public void ExportApplyDocx005013(string app_id)
        {
            ES.Areas.BACKMIN.Models.Apply_005013ViewModel model = new Areas.BACKMIN.Models.Apply_005013ViewModel();
            BackApplyDAO dao = new BackApplyDAO();
            string path = Server.MapPath("~/Sample/apply005013_2.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (Xceed.Words.NET.DocX doc = Xceed.Words.NET.DocX.Load(path))
                {
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = app_id;
                    var applydata = dao.GetRow(apply);
                    APPLY_005013 ap005013 = new APPLY_005013();
                    ap005013.APP_ID = app_id;
                    var ap005013data = dao.GetRow(ap005013);
                    model.Form = dao.QueryApply_005013(app_id);

                    doc.ReplaceText("[$YEAR]", (((DateTime)applydata.APP_TIME).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)applydata.APP_TIME).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)applydata.APP_TIME).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLICANT]", applydata.NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", applydata.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$AATO]", applydata.ADDR.TONotNullString() + "\n" + model.Form.TEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COO]", ap005013data.ORIGIN_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SP]", ap005013data.SHIPPINGPORT_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COS]", ap005013data.SELLER_TEXT.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLYMAN]", applydata.NAME + "線上申請案\n案件編號:" + applydata.APP_ID.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);

                    var tb = doc.AddTable(model.Form.Item_005013.ToCount() + 1, 5);
                    tb.Paragraphs[0].FontSize(11);
                    tb.Paragraphs[0].Font("標楷體");
                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Rows[0].Cells[0].Paragraphs[0].Append("⑦項次\n Item").FontSize(11).Font("標楷體");
                    tb.Rows[0].Cells[1].Paragraphs[0].Append("⑧貨名、規格、廠牌及製造廠名稱\n Description of Commodities Spec.and Brand or Maker, etc.").FontSize(11).Font("標楷體");
                    tb.Rows[0].Cells[2].Paragraphs[0].Append("⑨貨品分類稅則號列\n C.C.C.Code").FontSize(11).Font("標楷體");
                    tb.Rows[0].Cells[3].Paragraphs[0].Append("⑩數量\n Q'ty").FontSize(11).Font("標楷體");
                    tb.Rows[0].Cells[4].Paragraphs[0].Append("⑪單位\n Unit").FontSize(11).Font("標楷體");
                    var q = 1;
                    if (model.Form != null && model.Form.Item_005013 != null)
                    {
                        foreach (var item in model.Form.Item_005013)
                        {
                            tb.Rows[q].Cells[0].Paragraphs[0].Append(q.TONotNullString()).FontSize(12).Font("標楷體");
                            tb.Rows[q].Cells[1].Paragraphs[0].Append(item.Commodities.TONotNullString()).FontSize(12).Font("標楷體");
                            tb.Rows[q].Cells[2].Paragraphs[0].Append("").FontSize(12).Font("標楷體");
                            tb.Rows[q].Cells[3].Paragraphs[0].Append(item.Qty.TONotNullString()).FontSize(12).Font("標楷體");
                            tb.Rows[q].Cells[4].Paragraphs[0].Append(item.Unit_TEXT.TONotNullString() + "\n" + item.SpecQty.TONotNullString() + item.SpecUnit_TEXT.TONotNullString()).FontSize(12).Font("標楷體");
                            q++;
                        }
                    }

                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
                string DownLoadPath = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH");//下載路徑  
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromStream(ms, Spire.Doc.FileFormat.Docx);
                document.SaveToFile(DownLoadPath + "DI\\" + "applypdf.pdf", Spire.Doc.FileFormat.PDF);
                //System.Diagnostics.Process.Start("applypdf.pdf");
            }
            //return buffer;
        }

        public void ExportApplyDocx005014(string app_id)
        {
            ES.Areas.BACKMIN.Models.Apply_005014ViewModel model = new Areas.BACKMIN.Models.Apply_005014ViewModel();

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
                using (Xceed.Words.NET.DocX doc = Xceed.Words.NET.DocX.Load(path))
                {
                    model.APP_ID = app_id;
                    model.GetApplyData(app_id);
                    model.ApplyItems = GetApplyItemList(app_id);
                    model.ApplyItems2 = GetApplyItem2List(app_id);

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
                    doc.ReplaceText("[$YEAR]", (((DateTime)model.Apply.APP_TIME).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)model.Apply.APP_TIME).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)model.Apply.APP_TIME).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLICANT]", model.Apply.NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", model.Apply.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$AATO]", model.Apply.ADDR_ZIP.TONotNullString() + model.Apply.ADDR_ZIP_ADDR.TONotNullString() + model.Apply.ADDR_ZIP_DETAIL.TONotNullString() + "\n" + cnt_tel.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COO]", coo, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SP]", sp + "\n" + spt, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$COS]", cos, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLYMAN]", $"{model.Apply.NAME} 線上申請案，案件編號:\n{model.APP_ID.TONotNullString()}", false, System.Text.RegularExpressions.RegexOptions.None);

                    // 動態表格處理
                    var itemlist = new List<System.Collections.Hashtable>();
                    var record = JsonConvert.SerializeObject(model.ApplyItems);
                    var tempFinal = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(record);
                    var record2 = JsonConvert.SerializeObject(model.ApplyItems2);
                    var tempFinal2 = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject(record2);
                    int i = 0;
                    if (tempFinal != null && tempFinal.Count() > 0)
                    {
                        foreach (var jitem in tempFinal)
                        {
                            System.Collections.Hashtable dataItem = new System.Collections.Hashtable();
                            i++;
                            for (int k = 0; k < 29; k++)
                            {
                                dataItem[((Newtonsoft.Json.Linq.JProperty)jitem.ToList()[k]).Name] = ((Newtonsoft.Json.Linq.JValue)jitem.ToList()[k].FirstOrDefault()).Value;
                            }

                            itemlist.Add(dataItem);
                        }
                    }

                    i = 0;
                    if (tempFinal2 != null && tempFinal2.Count() > 0)
                    {
                        foreach (var jitem2 in tempFinal2)
                        {
                            System.Collections.Hashtable dataItem = new System.Collections.Hashtable();
                            i++;
                            for (int k = 0; k < 29; k++)
                            {
                                dataItem[((Newtonsoft.Json.Linq.JProperty)jitem2.ToList()[k]).Name] = ((Newtonsoft.Json.Linq.JValue)jitem2.ToList()[k].FirstOrDefault()).Value;
                            }

                            itemlist.Add(dataItem);
                        }
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
                        un.CODE_KIND = "F5_vw_PACK"; //申請數量單位
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
                string DownLoadPath = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH");//下載路徑  
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromStream(ms, Spire.Doc.FileFormat.Docx);
                document.SaveToFile(DownLoadPath + "DI\\" + "applypdf.pdf", Spire.Doc.FileFormat.PDF);
            }
            //return buffer;
        }
        public List<Apply_005014_ItemExt> GetApplyItemList(string APP_ID)
        {
            ShareDAO dao = new ShareDAO();
            IList<Apply_005014_Item> itemList = dao.GetRowList<Apply_005014_Item>(new Apply_005014_Item { APP_ID = APP_ID });
            List<Apply_005014_ItemExt> extList = itemList.Where(x => x.ITEM_TYPE.TOInt32() > 1)
                .Select(x => new Apply_005014_ItemExt().InjectFrom(x) as Apply_005014_ItemExt).ToList();
            if (extList == null)
            {
                Apply_005014_ItemExt item = new Apply_005014_ItemExt { ID = DateTime.Now.Ticks.ToString() };
                extList.Add(item);
            }
            return extList;
        }
        public List<Apply_005014_Item2Ext> GetApplyItem2List(string APP_ID)
        {
            ShareDAO dao = new ShareDAO();
            IList<Apply_005014_Item> itemList = dao.GetRowList(new Apply_005014_Item { APP_ID = APP_ID });
            List<Apply_005014_Item2Ext> ext2List = itemList.Where(x => x.ITEM_TYPE.TOInt32() == 1)
                .Select(x => new Apply_005014_Item2Ext().InjectFrom(x) as Apply_005014_Item2Ext).ToList();
            if (ext2List == null)
            {
                Apply_005014_Item2Ext item = new Apply_005014_Item2Ext { ID = DateTime.Now.Ticks.ToString() };
                ext2List.Add(item);
            }
            return ext2List;
        }

        #region 切結書
        public void ExportAffidavitDocx005013(string app_id)
        {
            ES.Areas.BACKMIN.Models.Apply_005013ViewModel model = new Areas.BACKMIN.Models.Apply_005013ViewModel();
            BackApplyDAO dao = new BackApplyDAO();
            string path = Server.MapPath("~/Sample/apply005013_1.docx");
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Load(path))
                {
                    ApplyModel apply = new ApplyModel();
                    apply.APP_ID = app_id;
                    var applydata = dao.GetRow(apply);
                    APPLY_005013 ap005013 = new APPLY_005013();
                    ap005013.APP_ID = app_id;
                    var ap005013data = dao.GetRow(ap005013);
                    APPLY_005013_ITEM2 item2005013 = new APPLY_005013_ITEM2();
                    item2005013.APP_ID = app_id;
                    var item2data = dao.GetRowList(item2005013);

                    var APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                    var APPLY_TYPE2 = "□保健養生";
                    var APPLY_TYPE3 = "□其他：(請說明)";
                    switch (ap005013data.RADIOUSAGE.TONotNullString())
                    {
                        case "1":
                            APPLY_TYPE1 = "■疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "□保健養生";
                            APPLY_TYPE3 = "□其他：(請說明)";
                            break;
                        case "2":
                            APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "■保健養生";
                            APPLY_TYPE3 = "□其他：(請說明)";
                            break;
                        case "3":
                            APPLY_TYPE1 = "□疾病:請檢附醫療機構開立之診斷書，處方箋等醫療證明文件之影本。";
                            APPLY_TYPE2 = "□保健養生";
                            APPLY_TYPE3 = "■其他：(請說明)" + ap005013data.RADIOUSAGE_TEXT.TONotNullString();
                            break;
                        default:
                            break;
                    }

                    // 替換文字
                    doc.ReplaceText("[$YEAR]", (((DateTime)applydata.APP_TIME).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)applydata.APP_TIME).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)applydata.APP_TIME).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$NAME]", applydata.NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE1]", APPLY_TYPE1, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE2]", APPLY_TYPE2, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$APPLY_TYPE3]", APPLY_TYPE3, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$IDN]", applydata.IDN.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$PHONE]", applydata.TEL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$ADDR]", applydata.ADDR.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$NAME1]", applydata.NAME.TONotNullString() + "線上申請案，案件編號:" + applydata.APP_ID.TONotNullString().TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);


                    // 動態表格
                    var tb = doc.AddTable(item2data.ToCount() + 1, 4);
                    tb.Paragraphs[0].FontSize(11);
                    tb.Paragraphs[0].Font("標楷體");
                    tb.Alignment = Xceed.Document.NET.Alignment.center;
                    tb.Rows[0].Cells[0].Width = 50;
                    tb.Rows[0].Cells[1].Width = 120;
                    tb.Rows[0].Cells[2].Width = 160;
                    tb.Rows[0].Cells[3].Width = 160;
                    tb.Rows[0].Cells[0].Paragraphs[0].Append("").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[1].Paragraphs[0].Append("藥品名稱").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[2].Paragraphs[0].Append("用法").Font("標楷體").Alignment = Alignment.center;
                    tb.Rows[0].Cells[3].Paragraphs[0].Append("總數量").Font("標楷體").Alignment = Alignment.center;
                    var q = 1;
                    if (item2data != null)
                    {
                        foreach (var item in item2data)
                        {
                            tb.Rows[q].Cells[0].Width = 50;
                            tb.Rows[q].Cells[1].Width = 120;
                            tb.Rows[q].Cells[2].Width = 160;
                            tb.Rows[q].Cells[3].Width = 160;
                            tb.Rows[q].Cells[0].Paragraphs[0].Append(q.TONotNullString()).Font("標楷體").Alignment = Alignment.center;
                            tb.Rows[q].Cells[1].Paragraphs[0].Append(item.ITEMNAME.TONotNullString()).Font("標楷體");
                            tb.Rows[q].Cells[2].Paragraphs[0].Append(item.USAGE.TONotNullString()).Font("標楷體");
                            tb.Rows[q].Cells[3].Paragraphs[0].Append(item.ALLQTY.TONotNullString()).Font("標楷體");
                            q++;
                        }
                    }

                    doc.ReplaceTextWithObject("[$ADDTABLE]", tb, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
                string DownLoadPath = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH");//下載路徑  
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromStream(ms, Spire.Doc.FileFormat.Docx);
                document.SaveToFile(DownLoadPath + "DI\\" + "affidavit1.pdf", Spire.Doc.FileFormat.PDF);
            }
        }

        public void ExportAffidavitDocx005014_1(string app_id)
        {
            Apply_005014ViewModel model = new Apply_005014ViewModel();
            model.APP_ID = app_id;
            model.GetApplyData(app_id);
            model.ApplyItems = GetApplyItemList(app_id);
            model.ApplyItems2 = GetApplyItem2List(app_id);
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
                    doc.ReplaceText("[$YEAR]", (((DateTime)model.Apply.APP_TIME).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)model.Apply.APP_TIME).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)model.Apply.APP_TIME).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SHIPPORT]", sp, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_NAME]", model.Apply.NAME, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_ADDR]", model.Apply.ADDR_ZIP_ADDR.TONotNullString() + model.Apply.ADDR_ZIP_DETAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CHR_NAME]", model.Apply.CHR_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_NAME]", model.Apply.CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_TEL]", cnt_tel, false, System.Text.RegularExpressions.RegexOptions.None);
                    // 動態表格處理
                    var itemList = string.Empty;
                    if (model.ApplyItems != null)
                    {
                        foreach (var item in model.ApplyItems)
                        {
                            var itemtype = item.ITEM_TYPE.TONotNullString();
                            //中藥材作為食品使用者(例如:靈芝) or 其他
                            if (itemtype == "3" || itemtype == "2")
                            {
                                //單位名稱
                                TblCODE_CD un = new TblCODE_CD();
                                un.CODE_KIND = "F5_vw_PACK";
                                un.CODE_PCD = "";
                                un.CODE_CD = item.UNIT.TONotNullString();
                                var une = dao.GetRow(un).CODE_MEMO.TONotNullString();
                                TblCODE_CD port = new TblCODE_CD();
                                port.CODE_KIND = "F5_vw_IMPORT";
                                port.CODE_PCD = "";
                                port.CODE_CD = item.AFF1_IMPORT_COUNTRY.TONotNullString();
                                var portn = dao.GetRow(port).CODE_DESC.TONotNullString();
                                //    人參丸       1瓶  ，報單號碼   CH1039500148  (進口關：基隆關)
                                itemList += $"{item.COMMODITIES.TONotNullString()} {item.QTY.TONotNullString()} {une}，";
                                itemList += $"報單號碼 {item.AFF1_SHEET_NO.TONotNullString()} (進口關：{portn})，";
                            }
                        }
                    }
                    doc.ReplaceText("[$ITEMLIST]", itemList, false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
                string DownLoadPath = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH");//下載路徑  
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromStream(ms, Spire.Doc.FileFormat.Docx);
                document.SaveToFile(DownLoadPath + "DI\\" + "affidavit1.pdf", Spire.Doc.FileFormat.PDF);
            }
        }

        public void ExportAffidavitDocx005014_2(string app_id)
        {
            Apply_005014ViewModel model = new Apply_005014ViewModel();
            model.APP_ID = app_id;
            model.GetApplyData(app_id);
            model.ApplyItems = GetApplyItemList(app_id);
            model.ApplyItems2 = GetApplyItem2List(app_id);
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
                    doc.ReplaceText("[$YEAR]", (((DateTime)model.Apply.APP_TIME).Year - 1911).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$MONTH]", (((DateTime)model.Apply.APP_TIME).Month).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$DAY]", (((DateTime)model.Apply.APP_TIME).Day).TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$SHIPPORT]", sp, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_NAME]", model.Apply.NAME, false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CO_ADDR]", model.Apply.ADDR_ZIP_ADDR.TONotNullString() + model.Apply.ADDR_ZIP_DETAIL.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CHR_NAME]", model.Apply.CHR_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_NAME]", model.Apply.CNT_NAME.TONotNullString(), false, System.Text.RegularExpressions.RegexOptions.None);
                    doc.ReplaceText("[$CNT_TEL]", cnt_tel, false, System.Text.RegularExpressions.RegexOptions.None);
                    // 動態表格處理
                    var itemList = string.Empty;
                    if (model.ApplyItems2 != null)
                    {
                        foreach (var item in model.ApplyItems2)
                        {
                            var itemtype = item.ITEM_TYPE.TONotNullString();
                            //中藥材萃取物作為食品原料者
                            if (itemtype == "1")
                            {
                                //單位名稱
                                TblCODE_CD un = new TblCODE_CD();
                                un.CODE_KIND = "F5_vw_PACK";
                                un.CODE_PCD = "";
                                un.CODE_CD = item.UNIT.TONotNullString();
                                var une = dao.GetRow(un).CODE_DESC.TONotNullString();
                                TblCODE_CD port = new TblCODE_CD();
                                port.CODE_KIND = "F1_COUNTRY_1";
                                port.CODE_PCD = "";
                                port.CODE_CD = item.AFF2_IMPORT_COUNTRY.TONotNullString();
                                var portn = dao.GetRow(port).CODE_DESC.TONotNullString();
                                //    (貨品名稱)     (產品名稱自動帶入每公克含原中藥材OO共OO公克)  (貨品數量)  ，報單號碼 (進口關： )
                                var aff2 = $"每公克含原中藥材 {item.AFF2_AMOUNT_NAME} 共 {item.AFF2_AMOUNT} 公克) ";
                                itemList += $"{item.COMMODITIES.TONotNullString()} ({aff2}) {item.QTY.TONotNullString()} {une}，";
                                itemList += $"報單號碼 {item.AFF2_SHEET_NO.TONotNullString()} (進口關：{portn})，";
                            }
                        }
                    }
                    doc.ReplaceText("[$ITEMLIST]", itemList, false, System.Text.RegularExpressions.RegexOptions.None);

                    doc.SaveAs(ms);
                }
                buffer = ms.ToArray();
                string DownLoadPath = DataUtils.GetConfig("DOWNLOAD_DOCUMENT_PATH");//下載路徑  
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromStream(ms, Spire.Doc.FileFormat.Docx);
                document.SaveToFile(DownLoadPath + "DI\\" + "affidavit2.pdf", Spire.Doc.FileFormat.PDF);
            }

        }
        #endregion
    }
}


