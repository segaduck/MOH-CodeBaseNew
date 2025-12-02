using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using System.Web.Mvc;
using log4net;
using EECOnline.DataLayers;
using EECOnline.Commons;
using EECOnline.Commons.Filter;
using EECOnline.Models;
using Turbo.Commons;
using EECOnline.Services;
using System.Web.Script.Serialization;
using System.Linq;
using System.IO;
using EECOnline.Models.Entities;
using Turbo.DataLayer;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.Zip;
using Omu.ValueInjecter;
using System.Security.Cryptography;
using NPOI.POIFS.Crypt;
using System.Net.Mail;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using EECOnline.Areas.A3.Models;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace EECOnline.Controllers
{

    /// <summary>
    /// 這個類集中放置一些 Ajax 動作會用的的下拉代碼清單控制 action
    /// </summary>
    [BypassAuthorize]
    public class AjaxController : EBaseController
    {
        protected static readonly ILog LOG = LogManager.GetLogger(typeof(MvcApplication));

        #region 傳回模組功能清單(多筆/根據SYSID)

        /// <summary>
        /// Ajax 取得系統對應的模組清單
        /// </summary>
        /// <param name="sysid">系統代號</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetModuleList(string sysid)
        {
            ShareCodeListModel model = new ShareCodeListModel();
            // 跟隨SYSID顯示資料
            var parms = new { sysid = sysid };
            var list = model.modules_list(parms);
            return MyCommonUtil.BuildOptionHtmlAjaxResult(list, "", "", "請選擇");
        }

        #endregion

        #region City 傳回城市項目

        /// <summary>
        /// 申辦項目
        /// </summary>
        [HttpPost]
        public ActionResult GetCity_list(string srv_cd)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();
            Hashtable parms = new Hashtable();
            parms["srv_cd"] = srv_cd;
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_city_parm, parms);
            list.Remove(list.Where(m => m.TEXT == null).FirstOrDefault());
            result.data = list;

            return Content(result.Serialize(), "application/json");
        }

        #endregion

        #region GetZIP_CO 傳回郵遞區號中文名稱(單筆)

        /// <summary>
        /// Ajax 傳回郵遞區號中文名稱(單筆)
        /// </summary>
        /// <param name="ZIP_CO">郵遞區號代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZIP_CO(string CODE)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();
            result.data = dao.GetZIP_COName(CODE);
            return Content(result.Serialize(), "application/json");
        }

        #endregion

        #region DynaminFILE 上傳檔案

        /// <summary>
        /// Ajax 上傳檔案/SystemUploadFile
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadFile(DynamicEFileGrid Upload)
        {
            SessionModel sm = SessionModel.Get();
            // 如果有任何檔案類型才做
            if (Request.Files.AllKeys.Any())
            {
                // 讀取指定的上傳檔案ID
                var httpPostedFile = Request.Files[0];
                // 副檔名
                var Extension = httpPostedFile.FileName.ToSplit('.').LastOrDefault();
                // 允許附檔名
                bool accepted = false;
                IList<AcceptFileType> acceptTypes = Upload.GetAcceptFileTypes();
                if (acceptTypes != null)
                {
                    string ext = Extension;
                    foreach (AcceptFileType type in acceptTypes)
                    {
                        if (type.Equals(ext)) accepted = true;
                    }
                }
                if (!accepted)
                {
                    Upload.ErrorMsg = "副檔名不允許，請檢查檔案後再重新上傳";
                }
                if (httpPostedFile.ContentLength > 10485760)
                {
                    Upload.ErrorMsg = "檔案大於10M，請檢查檔案後再重新上傳";
                }
                if (Upload.limitRow != null)
                {
                    if (Upload.fileGrid.ToCount() >= Upload.limitRow)
                    {
                        Upload.ErrorMsg = "僅能上傳" + Upload.limitRow + "筆檔案，若超過" + Upload.limitRow + "筆，請先刪除至少一筆在進行上傳 !";
                    }
                }

                if (Upload.ErrorMsg.TONotNullString() == "")
                {
                    // 真實有檔案，進行上傳
                    if (httpPostedFile != null && httpPostedFile.ContentLength != 0)
                    {
                        string Path = "~" + ConfigModel.UploadTempPath + "/" + Upload.peky1;
                        string PathDir = Server.MapPath(Path);
                        //string Path = ConfigModel.UploadTempPath + "/" + Upload.peky1;
                        //string PathDir = @"C:/euservice/"+ Upload.peky1+ Path;
                        string _filename = httpPostedFile.FileName.ToSplit('\\').Last();
                        string srcfilename = DateTime.Now.ToTwNowTime() + httpPostedFile.FileName.ToSplit('\\').Last().Replace("." + Extension, "").Replace("+", " ");
                        string extion = Extension;
                        string filename = Upload.pfilename;
                        if (!Directory.Exists(PathDir)) Directory.CreateDirectory(PathDir);

                        string FullPathDir = PathDir + "/" + srcfilename + "." + extion;
                        httpPostedFile.SaveAs(FullPathDir);

                        TblEFILE fileItem = new TblEFILE();
                        fileItem.srcfilename = srcfilename;
                        fileItem.filename = filename.TONotNullString() == "" ? srcfilename : filename;
                        fileItem.extion = extion;
                        fileItem.filesize = httpPostedFile.ContentLength;
                        fileItem.filepath = Path;
                        fileItem.peky1 = Upload.peky1;
                        fileItem.peky2 = Upload.peky2;
                        fileItem.peky3 = Upload.peky3;
                        fileItem.peky4 = Upload.peky4;
                        fileItem.modip = Request.UserHostAddress;
                        fileItem.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                        fileItem.moduser = sm.UserInfo.UserNo;
                        fileItem.modusername = sm.UserInfo.User.username;

                        // 模擬上傳的檔案內容
                        if (Upload.fileGrid.ToCount() == 0) Upload.fileGrid = new List<TblEFILE>();
                        Upload.fileGrid.Add(fileItem);
                    }
                }
            }

            //## 將結果回傳
            return PartialView("DynamicEFileGrid", Upload);
        }

        /// <summary>
        /// Ajax 刪除檔案/SystemUploadFile
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteFile(DynamicEFileGrid Upload, int idx)
        {
            if (Upload.fileGrid != null)
            {
                // 模擬上傳的檔案內容
                Upload.fileGrid.RemoveAt(idx);
            }

            // 將結果回傳
            return PartialView("DynamicEFileGrid", Upload);
        }

        #endregion 上傳檔案

        #region Get鄉鎮市區資料

        /// <summary>
        /// 郵遞區號
        /// </summary>
        [HttpPost]
        public ActionResult GetZipCo(string CODE)
        {

            var result = new AjaxResultStruct();
            var dao = new SHAREDAO();
            Hashtable parms = new Hashtable();
            parms["ZipCode"] = CODE;
            Hashtable list = new Hashtable();
            if (CODE.TONotNullString().Length == 3)
            {
                list = dao.GetZip3Detail(parms);
            }
            else
            {
                list = dao.GetZipDetail(parms);
            }

            result.data = list;

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 三碼郵遞區號
        /// </summary>
        [HttpPost]
        public ActionResult GetZipCo_Three(string CODE)
        {

            var result = new AjaxResultStruct();
            var dao = new SHAREDAO();
            Hashtable parms = new Hashtable();
            parms["ZipCode"] = CODE;
            Hashtable list = new Hashtable();
            list = dao.QueryZIP_CO_Three(parms);

            result.data = list;

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpPost]
        public ActionResult GetTownList(string CityCode)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();
            Hashtable parms = new Hashtable();
            parms["CityCode"] = CityCode;
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Zip_Town, parms);
            result.data = list;

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpPost]
        public ActionResult GetRoadList(string TownCode)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();
            Hashtable parms = new Hashtable();
            parms["TownCode"] = TownCode;
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Zip_Road, parms);
            result.data = list;

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// Ajax 傳回單位中文名稱
        /// </summary>
        /// <param name="UNIT">單位代碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetUNIT(string CODE)
        {
            var result = new AjaxResultStruct();
            var dao = new MyKeyMapDAO();
            result.data = dao.GetUNITName(CODE);
            return Content(result.Serialize(), "application/json");
        }

        #endregion

        #region 呼叫 API 用 (部端)

        /// <summary>
        /// [通用]
        /// 登入取 token 用
        /// <br/><br/>
        /// 備註：初期是只有亞東醫院在用，但後來新醫院加入，現在這隻 API 就是大家共用。
        /// </summary>
        [HttpPost]
        public ActionResult Api_A2_2_Login(EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_2_Login_ParamsModel model)
        {
            LOG.Debug("AjaxController.Api_A2_2_Login() Called.");
            var result = new AjaxResultStruct() { status = false, message = "" };
            var resultModel = new Utils.Hospital_FarEastern_Api.Api_A2_2_Login_ResultModel() { token = "" };
            if (model.user_name.TONotNullString() == "" || model.user_pwd.TONotNullString() == "")
            {
                result.message = "params is empty";
                LOG.Debug("AjaxController.Api_A2_2_Login() Return: params is empty.");
            }
            else
            {
                FrontDAO dao = new FrontDAO();
                var findUser = dao.GetRow(new TblEEC_Api_User() { user_name = model.user_name, user_pwd = model.user_pwd });
                if (findUser == null)
                {
                    result.message = "wrong user";
                    LOG.Debug("AjaxController.Api_A2_2_Login() Return: wrong user.");
                }
                else
                {
                    var tmpDate1 = DateTime.ParseExact(findUser.use_dates, "yyyyMMdd", null);
                    var tmpDate2 = DateTime.ParseExact(findUser.use_datee, "yyyyMMdd", null);
                    var checkDate = (DateTime.Now >= tmpDate1 && DateTime.Now <= tmpDate2);
                    if (!checkDate)
                    {
                        result.message = "user expired";
                        LOG.Debug("AjaxController.Api_A2_2_Login() Return: user expired.");
                    }
                    else
                    {
                        resultModel.token =
                            System.Guid.NewGuid().ToString("N") +
                            System.Guid.NewGuid().ToString("N") +
                            System.Guid.NewGuid().ToString("N");
                        result.status = true;
                        // 寫入 token 紀錄表
                        var resIdx = dao.Insert(new TblEEC_Api_Token()
                        {
                            user_name = model.user_name,
                            token = resultModel.token,
                            createtime = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        });
                        LOG.Debug("AjaxController.Api_A2_2_Login() Return: OK. Token: '" + resultModel.token + "'");
                    }
                }
            }
            result.data = resultModel;
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// [亞東醫院用]
        /// 匯入 SFTP 資料用
        /// </summary>
        //[HttpPost]
        [HttpPost, ValidateInput(false)]
        public ActionResult Api_A2_2(EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_2_ParamsModel model)
        {
            LOG.Debug("AjaxController.Api_A2_2() Called.");
            var result = new AjaxResultStruct() { status = false, message = "" };
            FrontDAO dao = new FrontDAO();

            #region 先檢查 Token 有效性

            bool tokenOK = false;
            var findToken = dao.GetRow(new TblEEC_Api_Token() { token = model.token });
            if (string.IsNullOrEmpty(model.token) || findToken == null)
            {
                result.message = "token error";
                LOG.Debug("AjaxController.Api_A2_2() Return: token error.");
            }
            else
            {
                var tmpDT = DateTime.ParseExact(findToken.createtime, "yyyyMMddHHmmss", null);
                var tmpDiff = (DateTime.Now - tmpDT).Minutes;
                if (tmpDiff <= 30) tokenOK = true;
                else
                {
                    result.message = "token error";
                    LOG.Debug("AjaxController.Api_A2_2() Return: token error.");
                }
            }
            if (!tokenOK) return Content(result.Serialize(), "application/json");

            #endregion

            var findData = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.caseNo }).Where(x => x.his_type.TONotNullString() != "").OrderBy(x => x.keyid).ToList();
            if (!string.IsNullOrEmpty(model.caseNo) && findData.ToCount() > 0)
            {
                // 建立回傳用資料集
                var resDatas = new List<EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_2_ResultModel>();
                // 有找到對應的訂單資料，才執行
                foreach (var row in model.data)
                {
                    // 新建各筆回傳資料
                    var tmpRes = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_2_ResultModel();
                    tmpRes.InjectFrom(row);
                    // 根據傳入的資料，去檢查是否實際存在
                    var findRow = findData.Where(x => x.his_type == row.ec_no).ToList();
                    if (findRow.ToCount() == 1)
                    {
                        tmpRes.ec_success = "成功";
                        tmpRes.ec_reason = "";
                        // 更新對方傳入的狀態，到 EEC_ApplyDetailPrice.ec_success_yn
                        ClearFieldMap cfmModel = new ClearFieldMap();
                        cfmModel.Add((TblEEC_ApplyDetailPrice x) => x.ec_success_yn);
                        cfmModel.Add((TblEEC_ApplyDetailPrice x) => x.ec_reason);
                        var resUpd = dao.Update(
                            new TblEEC_ApplyDetailPrice()
                            {
                                ec_success_yn = row.ec_success,
                                ec_reason = row.ec_reason,
                            },
                            new TblEEC_ApplyDetailPrice()
                            {
                                apply_no_sub = model.caseNo,
                                his_type = row.ec_no,
                            },
                            cfmModel
                        );
                        if (row.ec_fileBase64.TONotNullString() != "")
                        {
                            // 將 Base64 字符串轉換為字節數組
                            byte[] fileBytes = Convert.FromBase64String(row.ec_fileBase64);

                            var findSetupData = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_FarEastern_Api", del_mk = "N" });

                            // 指定存儲路徑和檔案名稱
                            string filePath = findSetupData.setup_val + findRow.FirstOrDefault().ec_fileName; // 替換 YourFileName 和擴展名

                            // 確保目錄存在
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            // 將字節數組寫入檔案
                            System.IO.File.WriteAllBytes(filePath, fileBytes);
                        }
                    }
                    else
                    {
                        tmpRes.ec_success = "失敗";
                        tmpRes.ec_reason = "data not found";
                    }
                    resDatas.Add(tmpRes);
                }
                result.data = resDatas;
                result.status = true;
                result.message = "";
                LOG.Debug("AjaxController.Api_A2_2() Return: OK.");

                #region 寄信通知
                // 取得該筆資料的 申請者
                var getUser = dao.GetRow(new TblEEC_User() { user_idno = findData.FirstOrDefault().user_idno });
                var getSucc = model.data.Where(x => x.ec_success.TONotNullString() == "成功" /*&& x.ec_reason.TONotNullString() == ""*/).ToList();
                // 有成功資料 且 有Email時
                if (getUser != null && getUser.user_email.TONotNullString() != "" && getSucc.Any())
                {
                    // 身分證隱碼處理
                    var tmpIDNStr = "";
                    try
                    {
                        var tmpIDN = getUser.user_idno.TONotNullString().ToCharArray();
                        for (int i = 4; i <= tmpIDN.Length - 3; i++) tmpIDN[i] = '*';
                        tmpIDNStr = new string(tmpIDN);
                    }
                    catch (Exception ex) { LOG.Debug("Ajax.Api_A2_2() Error: " + ex.TONotNullString()); }
                    // 信件組合
                    var strSubject = "衛生福利部民眾線上申辦電子病歷服務平台 電子病歷申辦進度通知";
                    var strBody = //getUser.user_name + " 您好：<br /><br />" +
                        "您好：<br /><br />" +
                        "衛生福利部民眾線上申辦電子病歷服務平台已收到院方回覆確認，並取得病歷資料：<br />" +
                        //"如要下載，請登入本系統並進入進度查詢頁面進行操作，謝謝！<br /><br />" +
                        "訂單編號：" + findData.FirstOrDefault().apply_no_sub + "<br />" +
                        "醫院名稱：" + findData.FirstOrDefault().hospital_name + "<br />" +
                        //"身分證字號：" + tmpIDNStr + "<br />" +
                        "生日：" + getUser.user_birthday + "<br />" +
                        "請登入線上申辦電子病歷服務平台，透過進度查詢頁面進行操作已取得電子病歷，謝謝！<br />" +
                        "<br />" +
                        "此為系統自動通知信，請勿直接回信！";
                    strBody = "<div style='font-size: 12pt;'>" + strBody + "</div>";
                    // 寄信
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, getUser.user_email, strSubject, strBody);
                    mailMessage.IsBodyHtml = true;
                    var t = CommonsServices.SendMail(mailMessage);
                    LOG.Debug("AjaxController.Api_A2_2() SendMail Success: " + (t.IsSuccess ? "Y" : "N") + ".");
                }
                #endregion
            }
            else
            {
                Hashtable resDatas = new Hashtable();
                resDatas.Add("error", "data not found");
                result.data = resDatas;
                result.message = "data not found";
                LOG.Debug("AjaxController.Api_A2_2() Return: data not found.");
            }
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// [中山醫院用]
        /// 匯入 SFTP 資料用
        /// </summary>
        //[HttpPost]
        [HttpPost, ValidateInput(false)]
        public ActionResult Api_A2_2_csh(EECOnline.Utils.Hospital_csh_Api.Api_A2_2_ParamsModel model)
        {
            LOG.Debug("AjaxController.Api_A2_2_csh() Called.");
            var result = new AjaxResultStruct() { status = false, message = "" };
            FrontDAO dao = new FrontDAO();

            #region 先檢查 Token 有效性

            bool tokenOK = false;
            var findToken = dao.GetRow(new TblEEC_Api_Token() { token = model.token });
            if (string.IsNullOrEmpty(model.token) || findToken == null)
            {
                result.message = "token error";
                LOG.Debug("AjaxController.Api_A2_2_csh() Return: token error.");
            }
            else
            {
                var tmpDT = DateTime.ParseExact(findToken.createtime, "yyyyMMddHHmmss", null);
                var tmpDiff = (DateTime.Now - tmpDT).Minutes;
                if (tmpDiff <= 30) tokenOK = true;
                else
                {
                    result.message = "token error";
                    LOG.Debug("AjaxController.Api_A2_2_csh() Return: token error.");
                }
            }
            if (!tokenOK) return Content(result.Serialize(), "application/json");

            #endregion

            var findData = dao.GetRowList(new TblEEC_ApplyDetailPrice() { caseNo = model.caseNo }).Where(x => x.his_type.TONotNullString() != "").OrderBy(x => x.keyid).ToList();
            if (!string.IsNullOrEmpty(model.caseNo) && findData.ToCount() > 0)
            {
                // 建立回傳用資料集
                var resDatas = new List<EECOnline.Utils.Hospital_csh_Api.Api_A2_2_ResultModel>();
                // 有找到對應的訂單資料，才執行
                foreach (var row in model.data)
                {
                    // 新建各筆回傳資料
                    var tmpRes = new EECOnline.Utils.Hospital_csh_Api.Api_A2_2_ResultModel();
                    tmpRes.InjectFrom(row);
                    // 根據傳入的資料，去檢查是否實際存在
                    var findRow = findData.Where(x => x.his_type == row.ec_no).ToList();
                    if (findRow.ToCount() == 1)
                    {
                        tmpRes.ec_success = "成功";
                        tmpRes.ec_reason = "";
                        // 更新對方傳入的狀態，到 EEC_ApplyDetailPrice.ec_success_yn
                        ClearFieldMap cfmModel = new ClearFieldMap();
                        cfmModel.Add((TblEEC_ApplyDetailPrice x) => x.ec_success_yn);
                        cfmModel.Add((TblEEC_ApplyDetailPrice x) => x.ec_reason);
                        var resUpd = dao.Update(
                            new TblEEC_ApplyDetailPrice()
                            {
                                ec_success_yn = row.ec_success,
                                ec_reason = row.ec_reason,
                            },
                            new TblEEC_ApplyDetailPrice()
                            {
                                caseNo = model.caseNo,
                                his_type = row.ec_no,
                            },
                            cfmModel
                        );
                        if (row.ec_fileBase64.TONotNullString() != "")
                        {
                            // 將 Base64 字符串轉換為字節數組
                            byte[] fileBytes = Convert.FromBase64String(row.ec_fileBase64);

                            var findSetupData = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_csh_Api", del_mk = "N" });

                            // 指定存儲路徑和檔案名稱
                            string filePath = findSetupData.setup_val + findRow.FirstOrDefault().ec_fileName; // 替換 YourFileName 和擴展名

                            // 確保目錄存在
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            // 將字節數組寫入檔案
                            System.IO.File.WriteAllBytes(filePath, fileBytes);
                        }
                    }
                    else
                    {
                        tmpRes.ec_success = "失敗";
                        tmpRes.ec_reason = "data not found";
                    }
                    resDatas.Add(tmpRes);
                }
                result.data = resDatas;
                result.status = true;
                result.message = "";
                LOG.Debug("AjaxController.Api_A2_2_csh() Return: OK.");

                #region 寄信通知
                // 取得該筆資料的 申請者
                var getUser = dao.GetRow(new TblEEC_User() { user_idno = findData.FirstOrDefault().user_idno });
                var getSucc = model.data.Where(x => x.ec_success.TONotNullString() == "成功" /*&& x.ec_reason.TONotNullString() == ""*/).ToList();
                // 有成功資料 且 有Email時
                if (getUser != null && getUser.user_email.TONotNullString() != "" && getSucc.Any())
                {
                    // 身分證隱碼處理
                    var tmpIDNStr = "";
                    try
                    {
                        var tmpIDN = getUser.user_idno.TONotNullString().ToCharArray();
                        for (int i = 4; i <= tmpIDN.Length - 3; i++) tmpIDN[i] = '*';
                        tmpIDNStr = new string(tmpIDN);
                    }
                    catch (Exception ex) { LOG.Debug("Ajax.Api_A2_2_csh() Error: " + ex.TONotNullString()); }
                    // 信件組合
                    var strSubject = "衛生福利部民眾線上申辦電子病歷服務平台 電子病歷申辦進度通知";
                    var strBody = //getUser.user_name + " 您好：<br /><br />" +
                        "您好：<br /><br />" +
                        "衛生福利部民眾線上申辦電子病歷服務平台已收到院方回覆確認，並取得病歷資料：<br />" +
                        //"如要下載，請登入本系統並進入進度查詢頁面進行操作，謝謝！<br /><br />" +
                        "訂單編號：" + findData.FirstOrDefault().apply_no_sub + "<br />" +
                        "醫院名稱：" + findData.FirstOrDefault().hospital_name + "<br />" +
                        //"身分證字號：" + tmpIDNStr + "<br />" +
                        "生日：" + getUser.user_birthday + "<br />" +
                        "請登入線上申辦電子病歷服務平台，透過進度查詢頁面進行操作已取得電子病歷，謝謝！<br />" +
                        "<br />" +
                        "此為系統自動通知信，請勿直接回信！";
                    strBody = "<div style='font-size: 12pt;'>" + strBody + "</div>";
                    // 寄信
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, getUser.user_email, strSubject, strBody);
                    mailMessage.IsBodyHtml = true;
                    var t = CommonsServices.SendMail(mailMessage);
                    LOG.Debug("AjaxController.Api_A2_2_csh() SendMail Success: " + (t.IsSuccess ? "Y" : "N") + ".");
                }
                #endregion
            }
            else
            {
                Hashtable resDatas = new Hashtable();
                resDatas.Add("error", "data not found");
                result.data = resDatas;
                result.message = "data not found";
                LOG.Debug("AjaxController.Api_A2_2_csh() Return: data not found.");
            }
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// [中國醫藥用]
        /// 匯入 SFTP 資料用
        /// </summary>
        //[HttpPost]
        [HttpPost, ValidateInput(false)]
        public ActionResult Api_A2_2_cmuh(EECOnline.Utils.Hospital_cmuh_Api.Api_A2_2_ParamsModel model)
        {
            LOG.Debug("AjaxController.Api_A2_2_cmuh() Called.");
            var result = new AjaxResultStruct() { status = false, message = "" };
            FrontDAO dao = new FrontDAO();

            #region 先檢查 Token 有效性

            bool tokenOK = false;
            var findToken = dao.GetRow(new TblEEC_Api_Token() { token = model.token });
            if (string.IsNullOrEmpty(model.token) || findToken == null)
            {
                result.message = "token error";
                LOG.Debug("AjaxController.Api_A2_2_cmuh() Return: token error.");
            }
            else
            {
                var tmpDT = DateTime.ParseExact(findToken.createtime, "yyyyMMddHHmmss", null);
                var tmpDiff = (DateTime.Now - tmpDT).Minutes;
                if (tmpDiff <= 30) tokenOK = true;
                else
                {
                    result.message = "token error";
                    LOG.Debug("AjaxController.Api_A2_2_cmuh() Return: token error.");
                }
            }
            if (!tokenOK) return Content(result.Serialize(), "application/json");

            #endregion

            var findData = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.caseNo }).Where(x => x.his_type.TONotNullString() != "").OrderBy(x => x.keyid).ToList();
            if (!string.IsNullOrEmpty(model.caseNo) && findData.ToCount() > 0)
            {
                // 建立回傳用資料集
                var resDatas = new List<EECOnline.Utils.Hospital_cmuh_Api.Api_A2_2_ResultModel>();
                // 有找到對應的訂單資料，才執行
                foreach (var row in model.data)
                {
                    // 新建各筆回傳資料
                    var tmpRes = new EECOnline.Utils.Hospital_cmuh_Api.Api_A2_2_ResultModel();
                    tmpRes.InjectFrom(row);
                    // 根據傳入的資料，去檢查是否實際存在
                    var findRow = findData.Where(x => x.his_type == row.ec_no).ToList();
                    if (findRow.ToCount() == 1)
                    {
                        tmpRes.ec_success = "成功";
                        tmpRes.ec_reason = "";
                        // 更新對方傳入的狀態，到 EEC_ApplyDetailPrice.ec_success_yn
                        ClearFieldMap cfmModel = new ClearFieldMap();
                        cfmModel.Add((TblEEC_ApplyDetailPrice x) => x.ec_success_yn);
                        cfmModel.Add((TblEEC_ApplyDetailPrice x) => x.ec_reason);
                        var resUpd = dao.Update(
                            new TblEEC_ApplyDetailPrice()
                            {
                                ec_success_yn = row.ec_success,
                                ec_reason = row.ec_reason,
                            },
                            new TblEEC_ApplyDetailPrice()
                            {
                                apply_no_sub = model.caseNo,
                                his_type = row.ec_no,
                            },
                            cfmModel
                        );
                        if (row.ec_fileBase64.TONotNullString() != "")
                        {
                            // 將 Base64 字符串轉換為字節數組
                            byte[] fileBytes = Convert.FromBase64String(row.ec_fileBase64);

                            var findSetupData = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_cmuh_Api", del_mk = "N" });

                            // 指定存儲路徑和檔案名稱
                            string filePath = findSetupData.setup_val + findRow.FirstOrDefault().ec_fileName; // 替換 YourFileName 和擴展名

                            // 確保目錄存在
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                            // 將字節數組寫入檔案
                            System.IO.File.WriteAllBytes(filePath, fileBytes);
                        }
                    }
                    else
                    {
                        tmpRes.ec_success = "失敗";
                        tmpRes.ec_reason = "data not found";
                    }
                    resDatas.Add(tmpRes);
                }
                result.data = resDatas;
                result.status = true;
                result.message = "";
                LOG.Debug("AjaxController.Api_A2_2_cmuh() Return: OK.");

                #region 寄信通知
                // 取得該筆資料的 申請者
                var getUser = dao.GetRow(new TblEEC_User() { user_idno = findData.FirstOrDefault().user_idno });
                var getSucc = model.data.Where(x => x.ec_success.TONotNullString() == "成功" /*&& x.ec_reason.TONotNullString() == ""*/).ToList();
                // 有成功資料 且 有Email時
                if (getUser != null && getUser.user_email.TONotNullString() != "" && getSucc.Any())
                {
                    // 身分證隱碼處理
                    var tmpIDNStr = "";
                    try
                    {
                        var tmpIDN = getUser.user_idno.TONotNullString().ToCharArray();
                        for (int i = 4; i <= tmpIDN.Length - 3; i++) tmpIDN[i] = '*';
                        tmpIDNStr = new string(tmpIDN);
                    }
                    catch (Exception ex) { LOG.Debug("Ajax.Api_A2_2_cmuh() Error: " + ex.TONotNullString()); }
                    // 信件組合
                    var strSubject = "衛生福利部民眾線上申辦電子病歷服務平台 電子病歷申辦進度通知";
                    var strBody = //getUser.user_name + " 您好：<br /><br />" +
                        "您好：<br /><br />" +
                        "衛生福利部民眾線上申辦電子病歷服務平台已收到院方回覆確認，並取得病歷資料：<br />" +
                        //"如要下載，請登入本系統並進入進度查詢頁面進行操作，謝謝！<br /><br />" +
                        "訂單編號：" + findData.FirstOrDefault().apply_no_sub + "<br />" +
                        "醫院名稱：" + findData.FirstOrDefault().hospital_name + "<br />" +
                        //"身分證字號：" + tmpIDNStr + "<br />" +
                        "生日：" + getUser.user_birthday + "<br />" +
                        "請登入線上申辦電子病歷服務平台，透過進度查詢頁面進行操作已取得電子病歷，謝謝！<br />" +
                        "<br />" +
                        "此為系統自動通知信，請勿直接回信！";
                    strBody = "<div style='font-size: 12pt;'>" + strBody + "</div>";
                    // 寄信
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, getUser.user_email, strSubject, strBody);
                    mailMessage.IsBodyHtml = true;
                    var t = CommonsServices.SendMail(mailMessage);
                    LOG.Debug("AjaxController.Api_A2_2_cmuh() SendMail Success: " + (t.IsSuccess ? "Y" : "N") + ".");
                }
                #endregion
            }
            else
            {
                Hashtable resDatas = new Hashtable();
                resDatas.Add("error", "data not found");
                result.data = resDatas;
                result.message = "data not found";
                LOG.Debug("AjaxController.Api_A2_2_cmuh() Return: data not found.");
            }
            return Content(result.Serialize(), "application/json");
        }





        #endregion

        #region 行動自然人憑證

        public static string ComputeSpChecksum(string payload, string aesKey)
        {
            // Step 1: SHA256_HEX(payload)
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(payloadBytes);
            string hexHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
            byte[] hexHashBytes = Encoding.UTF8.GetBytes(hexHash);

            // Step 2: AES-GCM encrypt
            byte[] keyBytes = Decode(aesKey);
            byte[] iv = new byte[12]; // 全0

            // 生成隨機字節並填充到 iv
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) { rng.GetBytes(iv); }

            byte[] output = new byte[hexHashBytes.Length + 16]; // 16 bytes tag

            var cipher = new GcmBlockCipher(new Org.BouncyCastle.Crypto.Engines.AesEngine());
            var parameters = new AeadParameters(new KeyParameter(keyBytes), 128, iv, null);

            cipher.Init(true, parameters);
            int len = cipher.ProcessBytes(hexHashBytes, 0, hexHashBytes.Length, output, 0);
            cipher.DoFinal(output, len); // append tag

            // Step 3: 拼接 IV (12 bytes) + cipher+tag
            byte[] result = new byte[iv.Length + output.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(output, 0, result, iv.Length, output.Length);

            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }

        [HttpPost]
        public ActionResult GetSPAPIWEB01(HomeViewModel model)
        {
            LOG.Debug("AjaxController.GetSPAPIWEB01() Called.");
            SessionModel sm = SessionModel.Get();
            var result = new AjaxResultStruct();
            FrontDAO dao = new FrontDAO();

            var errMsg = "";
            #region 檢查
            if (model.Login.user_name.TONotNullString() == "")
            {
                errMsg += "請輸入 姓名! \r\n";
            }
            if (model.Login.user_idno1.TONotNullString() == "")
            {
                errMsg += "請輸入 身分證字號! \r\n";
            }
            if (model.Login.user_birthday1.TONotNullString() == "")
            {
                errMsg += "請輸入 出生年月日! \r\n";
            }
            if (model.Login.ValidateCode2.TONotNullString() != sm.LoginValidateCode2)
            {
                errMsg += "驗證碼錯誤! \r\n";
            }
            #endregion

            if (errMsg == "")
            {
                Guid newGuid = Guid.NewGuid();
                var UUID = newGuid.ToString();

                EEC_SPAPIWEB01 sp01 = new EEC_SPAPIWEB01();
                sp01.transaction_id = UUID;
                sp01.sp_service_id = "20240110110945267658";  // 正式
                //sp01.sp_service_id = "20230926140227945383";  // 測試
                sp01.op_code = "SIGN";
                sp01.hint = "簽章提示訊息";

                sp01.sign_type = "PKCS#7";
                sp01.sign_data = "RE9DX0RJR0VTVF8xMjM0NTY3ODkw";
                sp01.tbs_encoding = "base64";
                sp01.hash_algorithm = "SHA256";

                //sp01.id_num = model.Login.user_idno1;
                //sp01.op_mode = "I-SCAN";
                //string payload = $"{sp01.transaction_id}{sp01.sp_service_id}{sp01.id_num}{sp01.op_code}{sp01.op_mode}{sp01.hint}{sp01.sign_data}";
                string payload = sp01.transaction_id + sp01.sp_service_id + sp01.op_code + sp01.hint + sp01.sign_data;

                var apiKey = "RqiGYfvkGTQIPOU7GthZFuHGp4skDW92Vw1nOxYUJTE";  // 正式
                //var apiKey = "bByI492ih1Y2tdyGVGQ4NzA0g0AhqawQsxPqmELgzfU";  // 測試
                sp01.sp_checksum = ComputeSpChecksum(payload, apiKey);

                // 把需要的欄位包起來 轉成 JSON 字串
                //sp01.sign_info = JsonConvert.SerializeObject(new
                //{
                //    sign_type = sp01.sign_type,
                //    sign_data = sp01.sign_data,
                //    tbs_encoding = sp01.tbs_encoding,
                //    hash_algorithm = sp01.hash_algorithm
                //});

                #region checksum加密 (目前不使用)
                /*
                var checksumO = sp01.transaction_id + sp01.sp_service_id + model.Login.user_idno1 + sp01.op_code + "I-SCAN" + sp01.hint + sp01.sign_data;
                //checksumO = "046b6c7f-0b8a-43b9-b35d-6489e6daee917b2c7f94-9f7b-481a-89a8-56b883dea695A987654321SIGNI-SCAN待簽署資料DOC_DIGEST_JLDKJFLKDSJFLDSLFJSDLFJLSKDFJLSFJLSFJLDSKFJLKSDFJLSFJLKDSFJLKSDFDSFSDFSDFDSFDSFF";
                string inputString = checksumO; // 要計算SHA-256哈希的輸入字符串

                // 將輸入字符串轉換為UTF-8編碼的字節數組
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
                string hashHex = "";
                // 創建SHA-256哈希算法的實例
                using (SHA256 sha256 = SHA256.Create())
                {
                    // 計算哈希值
                    byte[] hashBytes = sha256.ComputeHash(inputBytes);

                    // 將哈希值轉換為十六進制字符串
                    hashHex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                }

                //byte[] iv = Encoding.UTF8.GetBytes("1241c87f32fbe9effec8ac92");
                //byte[] ivBytes;
                //// 創建SHA-256哈希算法的實例
                //using (SHA256 sha256 = SHA256.Create())
                //{
                //    // 計算哈希值
                //    ivBytes = sha256.ComputeHash(iv);

                //}
                int GCM_IV_LENGTH = 12;
                var ivBytes = new byte[GCM_IV_LENGTH];


                //var apiKey = "RqiGYfvkGTQIPOU7GthZFuHGp4skDW92Vw1nOxYUJTE";  // 正式
                //var apiKey = "9LAkcNRrfEmgVCxLUybdtA0R5OEBqyz6sZajIsWDrnk";  // 測試

                // 进行 Base64Url 解码
                byte[] keyBytes = Decode(apiKey);
                //string resultText = "";
                //foreach (byte b in keyBytes)
                //{
                //    resultText +=b+"-";
                //}
                //sbyte[] AesKeyInSBytes = new sbyte[keyBytes.Length];
                //for (int i = 0; i < keyBytes.Length; i++)
                //{
                //    AesKeyInSBytes[i] = (sbyte)(keyBytes[i] & 0xFF);
                //}
                //string resultText1 = "";
                //// 打印转换后的 sbyte 数组
                //foreach (sbyte sb in AesKeyInSBytes)
                //{
                //    resultText1 += sb + ",";
                //}
                //var t = "";
                //byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
                //string resultEncode = Convert.ToBase64String(keyBytes);

                //var t = apiKey.Length;
                //byte[] keyBytes = Convert.FromBase64String(apiKey);

                //byte[] keyBytes = new byte[32]; // 256位密鑰
                //using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                //{
                //    rng.GetBytes(keyBytes);
                //}

                var dataBytes = Encoding.UTF8.GetBytes(hashHex);

                //SecretKeySpec sky = new SecretKeySpec(keyBytes, "AES");
                //Cipher cip = Cipher.GetInstance("AES/GCM/NoPadding");//算法/模式/補碼
                //cip.Init(Cipher.ENCRYPT_MODE, sky, new IvParameterSpec(ivBytes));

                byte[] cipherVal = Encrypt(dataBytes, keyBytes, ivBytes);

                var test2 = "";
                // 將哈希值轉換為十六進制字符串
                test2 = BitConverter.ToString(cipherVal).Replace("-", "").ToLower();
                //string resultText3 = "";
                //foreach (byte b in cipherVal)
                //{
                //    resultText3 += b + "-";
                //}
                //string test1 = Encode(cipherVal);

                byte[] result2 = new byte[ivBytes.Length + cipherVal.Length];

                Array.Copy(ivBytes, 0, result2, 0, ivBytes.Length);

                Array.Copy(cipherVal, 0, result2, ivBytes.Length, cipherVal.Length);

                var test1 = "";
                // 將哈希值轉換為十六進制字符串
                test1 = BitConverter.ToString(result2).Replace("-", "").ToLower();

                //var finalHEX = "";
                //using (SHA256 sha256 = SHA256.Create())
                //{
                //    // 計算哈希值
                //    byte[] hashBytes = sha256.ComputeHash(result2);
                //
                //    // 將哈希值轉換為十六進制字符串
                //    finalHEX = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                //
                //}

                //sp01.sp_checksum = test1;
                */
                #endregion

                sp01.time_limit = 60;
                sp01.fType = "1";
                sp01.CREATEDATE = DateTime.Now;
                sp01.IDNO = model.Login.user_idno1;
                sp01.BIRTHDAY = Convert.ToDateTime(model.Login.user_birthday1).ToString("yyyyMMdd");
                dao.Insert(sp01);

                result.data = sp01;
                result.status = true;
                result.message = "";
            }
            else
            {
                result.status = false;
                result.message = errMsg;
            }


            return Content(result.Serialize(), "application/json");
        }

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] nonce)
        {
            GcmBlockCipher gcm = new GcmBlockCipher(new AesFastEngine());
            AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, nonce);
            gcm.Init(true, parameters);

            byte[] ciphertext = new byte[gcm.GetOutputSize(plaintext.Length)];
            int len = gcm.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
            gcm.DoFinal(ciphertext, len);

            return ciphertext;
        }

        /// <summary>
        /// Encodes the specified byte array.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        public static string Encode(byte[] arg)
        {
            var s = Convert.ToBase64String(arg); // Standard base64 encoder

            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding

            return s;
        }

        /// <summary>
        /// Decodes the specified string.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Illegal base64url string!</exception>
        public static byte[] Decode(string arg)
        {
            var s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding

            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default: throw new Exception("Illegal base64url string!");
            }

            return Convert.FromBase64String(s); // Standard base64 decoder
        }

        #endregion

        /// <summary>
        /// 每天上午四點觸發<br />
        /// 去檢查資料庫資料，只要資料是 "已付款" 且 "尚未收到亞東醫院回覆病歷檔" 時，<br />
        /// 就寄信給亞東醫院 (代號：1131010011H)
        /// </summary>
        [HttpPost]
        public void CheckDataIsGot_Or_SendEmailTo_FarEastern()
        {
            LOG.Debug("AjaxController.CheckDataIsGot_Or_SendEmailToFarEastern() Called.");
            const string FEHospCode = "1131010011H";
            FrontDAO dao = new FrontDAO();
            Hashtable theParams = new Hashtable();
            theParams.Add("hospital_code", FEHospCode);
            var checkDatas = dao.QueryForListAll<TblEEC_ApplyDetailPrice>("Front.getDataIsNotGot", theParams).ToList();
            if (checkDatas.Any())
            {
                // 取得亞東醫院 Email
                var getHospEmail = dao.GetRow(new TblEEC_Hospital() { code = FEHospCode });
                if (getHospEmail == null || string.IsNullOrEmpty(getHospEmail.Email))
                    LOG.Debug("AjaxController.CheckDataIsGot_Or_SendEmailToFarEastern() The hospital email unset, Send process has been canceled.");
                else
                {
                    var strSubject = "衛生福利部民眾線上申辦電子病歷服務平台 電子病歷檔案未上傳通知 (" + DateTime.Now.ToString("yyyy-MM-dd") + ")";
                    var strBody = "";
                    var tmpRow = 1;
                    foreach (var row in checkDatas)
                    {
                        strBody = strBody + "<tr>";
                        strBody = strBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + tmpRow.ToString() + "</td>";
                        strBody = strBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + row.apply_no_sub + "</td>";
                        strBody = strBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + row.his_type + "</td>";
                        strBody = strBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + row.his_type_name + "</td>";
                        strBody = strBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + row.ec_date + "</td>";
                        strBody = strBody + "</tr>";
                        tmpRow++;
                    }
                    strBody = "<tr><th>Seq</th><th>caseNo</th><th>ec_no</th><th>ec_name</th><th>ec_date</th></tr>" + strBody;
                    strBody = "<table>" + strBody + "</table>";
                    strBody = "以下資料尚未收到電子病歷檔：<br /><br />" + strBody;
                    strBody = "<div style='font-size: 12pt;'>" + strBody + "</div>";
                    // 寄信
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, getHospEmail.Email, strSubject, strBody);
                    mailMessage.IsBodyHtml = true;
                    var t = CommonsServices.SendMail(mailMessage);
                    LOG.Debug("AjaxController.CheckDataIsGot_Or_SendEmailToFarEastern() SendMail Success: " + (t.IsSuccess ? "Y" : "N") + ".");
                }
            }
            LOG.Debug("AjaxController.CheckDataIsGot_Or_SendEmailToFarEastern() Count: " + checkDatas.ToCount().ToString() + " rows.");
            LOG.Debug("AjaxController.CheckDataIsGot_Or_SendEmailToFarEastern() Done.");
        }

        /// <summary>
        /// 每天上午四點觸發<br />
        /// 去檢查資料庫資料 EEC_ApplyDetailPrice 的 provide_status 為 '0' 的所有資料，<br />
        /// 如果該資料對應的 EEC_ApplyDetailPrice_ApiData 的 Report_HTML 無資料，則去 EEC 抓 base64 XML 回來產 HTML，<br />
        /// 如果有得到 HTML 後，則變更 EEC_ApplyDetailPrice 的 provide_status 狀態為 '1' 否則 '2'<br /><br />
        /// 只有檢查 EEC 資料！！<br />
        /// 跳過：<br />
        /// 亞東醫院 1131010011H  <br />
        /// 中山醫院 1317040011H  <br />
        /// 中國醫藥 1317050017H  <br />
        /// </summary>
        [HttpPost]
        public void CheckProvideDataStatus_Or_SendMailToEECHosp()
        {
            LOG.Debug("AjaxController.CheckProvideDataStatus_Or_SendMailToEECHosp() Called.");
            FrontDAO dao = new FrontDAO();
            string[] SkipHospCode = {  // 要跳過的 特定醫院
                "1131010011H",
                "1317040011H",
                "1317050017H",
            };
            var listApiData = dao.GetRowList(new TblEEC_ApplyDetailPrice_ApiData()).Select(x => x.master_keyid).ToArray();
            var tmpDataSet = dao.GetRowList(new TblEEC_ApplyDetailPrice() { provide_status = "0" })
                .Where(x => !SkipHospCode.Contains(x.hospital_code))  // 抓取 特定醫院以外的資料
                .Where(x => listApiData.Contains(x.keyid))  // 抓取 在 TblEEC_ApplyDetailPrice_ApiData 有資料的資料
                .ToList();
            if (tmpDataSet.Any())
            {
                foreach (var row in tmpDataSet)
                {
                    var getApiData = dao.GetRow(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = row.keyid });
                    if (getApiData == null) continue;
                    // 1. 取得 API Token
                    //var apiToken = EECOnline.Utils.Hospital_Common_Api.GetLoginToken(ConfigModel.LoginUser, ConfigModel.LoginPwd);
                    // 2. 取得 病歷檔案(base64) 然後存入 DB
                    //EECOnline.Utils.Hospital_Common_Api.GetQueryContent_SaveIntoDB(apiToken, getApiData.Guid, getApiData.PatientIdNo, getApiData.AccessionNum, getApiData.HospitalId, getApiData.TemplateId);
                    // 3. 抓出 DB 的 病歷檔案(base64) 然後轉成 HTML 然後存入 DB
                    EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML(row.his_type, getApiData.Guid, getApiData.PatientIdNo, getApiData.AccessionNum, getApiData.HospitalId, getApiData.TemplateId);
                    // 重抓一次資料，檢驗是否有取得 HTML
                    var getApiData2 = dao.GetRow(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = row.keyid });
                    if (getApiData2 == null) continue;
                    var tmpProvideStatus = (string.IsNullOrEmpty(getApiData2.Report_HTML) ? "2" : "1");
                    // 回填狀態
                    var resUpd = dao.Update(
                        new TblEEC_ApplyDetailPrice() { provide_status = tmpProvideStatus },
                        new TblEEC_ApplyDetailPrice() { keyid = row.keyid }
                    );
                }
            }

            #region 寄信，如果尚未取得 EEC base64 XML 檔案的話

            // 列出所有未取得病歷 XML 的清單
            var listNoHTML = dao.GetRowList(new TblEEC_ApplyDetailPrice() { provide_status = "2" }).ToList();
            if (listNoHTML.Any())
            {
                // 開始產生統計 apply_no_sub 用字串
                var listHosp = listNoHTML.Select(x => x.hospital_code).OrderBy(x => x).Distinct().ToList();
                var listBody = new List<string>();
                foreach (var codeHosp in listHosp)
                {
                    // 列出各筆醫院的 訂單編號及其他資料，彙總起來集合成一串
                    var tmpBody = "";
                    var tmpRows = listNoHTML.Where(x => x.hospital_code == codeHosp).OrderBy(x => x.apply_no_sub).ThenBy(x => x.his_type).ToList();
                    var tmpRow = 1;
                    foreach (var row in tmpRows)
                    {
                        tmpBody = tmpBody + "<tr>";
                        tmpBody = tmpBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + tmpRow.ToString() + "</td>";
                        tmpBody = tmpBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + row.apply_no_sub + "</td>";
                        tmpBody = tmpBody + "<td style='padding-left: 1rem; padding-right: 1rem;'>" + row.his_type_name + "</td>";
                        tmpBody = tmpBody + "</tr>";
                        tmpRow++;
                    }
                    listBody.Add(tmpBody);
                }
                // 整理好資料後，準備寄信
                for (var i = 0; i <= listHosp.ToCount() - 1; i++)
                {
                    var getHospEmail = dao.GetRow(new TblEEC_Hospital() { code = listHosp[i] });
                    if (getHospEmail == null) continue;
                    if (getHospEmail.Email.TONotNullString() == "") continue;
                    if (listBody[i].TONotNullString() == "") continue;
                    var strSubject = "衛生福利部民眾線上申辦電子病歷服務平台 電子病歷檔案未取得通知 (" + DateTime.Now.ToString("yyyy-MM-dd") + ")";
                    var strBody = listBody[i];
                    strBody = "<tr><th>流水號</th><th>訂單編號</th><th>病歷類別</th></tr>" + strBody;
                    strBody = "<table>" + strBody + "</table>";
                    strBody = getHospEmail.text + " 您好：<br />以下資料尚未取得電子病歷檔：<br /><br />" + strBody;
                    strBody = "<div style='font-size: 12pt;'>" + strBody + "</div>";
                    // 寄信
                    MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, getHospEmail.Email, strSubject, strBody);
                    mailMessage.IsBodyHtml = true;
                    var t = CommonsServices.SendMail(mailMessage);
                    LOG.Debug("AjaxController.CheckProvideDataStatus_Or_SendMailToEECHosp() SendMail Success: " + (t.IsSuccess ? "Y" : "N") + ".");
                }
            }

            #endregion

            LOG.Debug("AjaxController.CheckProvideDataStatus_Or_SendMailToEECHosp() Done.");
        }

        [HttpPost]
        public ActionResult SMTPTest(string email)
        {
            LOG.Debug("Ajax.SMTPTest() Call. email: " + email);
            try
            {
                if (string.IsNullOrEmpty(email)) return Content("NO");
                var strSubject = "Test";
                var strBody = "Test<br/>Test123";
                MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, email, strSubject, strBody);
                mailMessage.IsBodyHtml = true;
                var t = CommonsServices.SendMail(mailMessage);
                LOG.Debug("Ajax.SMTPTest() Result: " + t.IsSuccess.ToString());
                return Content("YES");
            }
            catch (Exception ex)
            {
                LOG.Debug("Ajax.SMTPTest() Error: " + ex.TONotNullString());
                return Content("NO");
            }
        }

        [HttpPost]
        public void ExportDat_GO(string hospital_code)
        {
            C101MExportDatModel model = new C101MExportDatModel();
            model.hospital_code = hospital_code;
            model.PayStatus_forSQL = " AND a.payed = 'Y' AND (c.is_request_payment <> 'Y' OR c.is_request_payment IS NULL) ";
            //model.ApplyDateS = DateTime.Now.ToString("yyyy/MM/dd");
            //model.ApplyDateE = DateTime.Now.ToString("yyyy/MM/dd");
            ModelState.Clear();
            A3DAO dao = new A3DAO();

            try
            {
                // 查資料
                model.Grid = dao.QueryForListAll<ExportDatGridModel>("A3.queryC101M_ExportDat", model);

                // 視情況增加
                if (model.hospital_code == "1131010011H")
                {
                    // 亞東
                    var GetSetups = new FrontDAO().QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);  // 取 DB 參數設定
                    var tmpMerchant = new FrontDAO().GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID_FE");       // 聯合特店代號
                    var tmpTerminal = new FrontDAO().GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID_FE");        // 聯合端末機代碼
                    LOG.Info("AJAX.ExportDat_Go() 亞東: " + GetSetups + "；" + tmpMerchant + "；" + tmpTerminal + "；");
                    // 生成 dat 檔
                    var DatFileList = CommonsServices.ExportDat(tmpMerchant, tmpTerminal, model.Grid);

                    // 輸出檔案
                    byte[] DatFile = DatFileList
                        .SelectMany(x => System.Text.Encoding.GetEncoding("big5").GetBytes(x + Environment.NewLine))
                        .ToArray();
                    var tmpObj = dao.GetRow(new TblSETUP() { setup_cd = "FOLDER_TEMPLATE_亞東", del_mk = "N" });
                    var filePath = $@"{tmpObj.setup_val}Pay\";
                    var fileName = $"{filePath}{tmpMerchant}.dat";
                    // 已存在檔案-刪除
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                        LOG.Info("AJAX.ExportDat_Go() Delete:" + fileName);
                    }

                    using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        fileStream.Write(DatFile, 0, DatFile.Length);
                    }

                    LOG.Info("AJAX.ExportDat_Go() ReBulidSuccess:" + fileName);

                    //執行bat
                    string fileuploadname = "FileUpload5亞東.bat";
                    string parameters = filePath;
                    CommonsServices.ProcessCmd(fileuploadname, parameters);

                    LOG.Info("AJAX.ExportDat_Go() ProcessCmd:" + parameters + fileuploadname);
                }
            }
            catch (Exception ex)
            {
                LOG.Error("A3.C101M.ExportDat_Go() Error: " + ex.TONotNullString());
            }

        }

        /// <summary>
        /// 處理請款回覆檔 排程
        /// </summary>
        /// <returns></returns>
        public void ProcessRequestFileDownloadSchedule(string hospital_code)
        {
            LOG.Debug("ProcessRequestFileDownloadSchedule start");
            A3DAO dao = new A3DAO();

            // 執行bat
            if (hospital_code == "1131010011H")
            {
                // 亞東
                string filename = "FileDownload5亞東.bat";
                var tmpObj = dao.GetRow(new TblSETUP() { setup_cd = "FOLDER_TEMPLATE_亞東", del_mk = "N" });
                string parameters = $@"{tmpObj.setup_val}Pay\";
                CommonsServices.ProcessCmd(filename, parameters);
            }

            LOG.Debug("ProcessRequestFileDownloadSchedule end");

        }

        [HttpPost]
        public void ImportDat_Go(string hospital_code)
        {
            C101MImportDatModel model = new C101MImportDatModel();
            try
            {
                model.Grid = new List<C101MImportDatGridModel>();
                A3DAO dao = new A3DAO();
                var directoryFiles = new FileInfo[0];
                var dt = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
                if (hospital_code == "1131010011H")
                {
                    // 亞東
                    var tmpObj = dao.GetRow(new TblSETUP() { setup_cd = "FOLDER_TEMPLATE_亞東", del_mk = "N" });
                    string parameters = $@"{tmpObj.setup_val}Pay\";
                    DirectoryInfo directoryInfo = new DirectoryInfo(parameters);
                    // GetFiles(string searchPattern): 加入指定的Pattern，比對檔案名稱
                    directoryFiles = directoryInfo.GetFiles("*.rsp");
                }

                if (directoryFiles.ToCount() > 0)
                {
                    var successCount = 0;
                    foreach (var TheFilePath in directoryFiles)
                    {
                        if (TheFilePath.Name.Contains(dt))
                        {
                            MemoryStream ms = new MemoryStream();
                            //Open file for Read\Write
                            FileStream fs = TheFilePath.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                            var tmpList = new List<string>();

                            using (StreamReader reader = new StreamReader(fs, Encoding.GetEncoding(950)))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    // do something with line
                                    tmpList.Add(line);
                                }
                                reader.Close();
                            }


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
                        }

                    }
                    LOG.Info("A3.C101M.ImportDat_Go() ImportData: " + "已上傳 " + model.Grid.ToCount().ToString() + " 筆<br/>" +
                  "共 " + successCount.ToString() + " 筆請款成功");
                }



            }
            catch (Exception ex)
            {
                LOG.Error("A3.C101M.ImportDat_Go() Error: " + ex.TONotNullString());
            }

        }
    }
}