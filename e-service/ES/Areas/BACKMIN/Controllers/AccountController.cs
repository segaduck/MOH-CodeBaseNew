using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Utils;
using Newtonsoft.Json;
using System.Web.Routing;
using log4net;
using System.IO;
using GemBox.Spreadsheet;
using ES.Areas.BACKMIN.Utils;
using System.Data;
using WebUI.CustomClass;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 1;
        }

        [HttpPost]
        public ActionResult downloadReport(AccountQueryModel model, string Report_type)  //GetExcel
        {
            AccountReport temp = new AccountReport(model);
            if (!temp.Processing())
            {
                TempData["tempMessage"] = temp.Error;
                return RedirectToAction("BACKMIN", "Account");
            }

            string sFileName = HttpUtility.UrlEncode("使用者帳號清單." + Report_type);

            switch (Report_type)
            {
                case "xls":
                    var tempfile = File(FileToStream(temp.FilePath + ".xls"), "application/unknown", sFileName); //從檔讀取 FilePath

                    System.IO.File.Delete(temp.FilePath + ".xls");//刪掉暫存
                    System.IO.File.Delete(temp.FilePath + ".ods"); //刪掉暫存

                    return tempfile;

                case "ods":
                    var tempods = File(FileToStream(temp.FilePath + ".ods"), "application/vnd.oasis.opendocument.spreadsheet", sFileName);

                    System.IO.File.Delete(temp.FilePath + ".xls");//刪掉暫存
                    System.IO.File.Delete(temp.FilePath + ".ods"); //刪掉暫存

                    return tempods;

                default:
                    TempData["tempMessage"] = "未偵測到匯出格式!";
                    return RedirectToAction("BACKMIN", "Account");
            }
        }

        /// <summary>
        /// 從檔讀取 Stream
        /// </summary>
        public Stream FileToStream(string filePath)
        {
            // 打開檔
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 讀取檔的 byte[]
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 轉換成 Stream
            Stream stream = new MemoryStream(bytes);
            return stream;
        }


        /// <summary>
        /// 管理帳號 - 列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            AccountQueryModel model = new AccountQueryModel();
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                model.NowPage = 1;

                AccountModel accountModel = GetAccountModel();
                if (accountModel == null || accountModel.Scope == 1)
                {
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);
                }
                else
                {
                    model.UnitCode = accountModel.ScopeUnitCode.ToString();
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, accountModel.ScopeUnitCode, (string)null, false);
                }

                AccountAction action = new AccountAction(conn);
                ViewBag.List = action.GetList(model);

                double pageSize = action.GetPageSize();
                double totalCount = action.GetTotalCount();

                ViewBag.NowPage = model.NowPage;
                ViewBag.TotalCount = action.GetTotalCount();
                ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

                conn.Close();
                conn.Dispose();
            }

            this.SetVisitRecord("Account", "Index", "管理帳號列表");

            return View(model);
        }




        /// <summary>管理帳號 - 列表</summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(AccountQueryModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Query"))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    AccountAction action = new AccountAction(conn);
                    ViewBag.List = action.GetList(model);

                    AccountModel accountModel = GetAccountModel();
                    if (accountModel == null || accountModel.Scope == 1)
                    {
                        ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);
                    }
                    else
                    {
                        ViewBag.UnitList = CodeUtils.GetUnitList(conn, accountModel.ScopeUnitCode, (string)null, false);
                    }

                    double pageSize = action.GetPageSize();
                    double totalCount = action.GetTotalCount();

                    ViewBag.NowPage = model.NowPage;
                    ViewBag.TotalCount = action.GetTotalCount();
                    ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

                    conn.Close();
                    conn.Dispose();
                }
            }
            else if (model.ActionType.Equals("Edit"))
            {
                TempData["ActionId"] = model.ActionId;
                return RedirectToAction("Edit", "Account");
            }
            else if (model.ActionType.Equals("Delete"))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    AccountAction action = new AccountAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.Delete(model)) //停用帳號
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        return RedirectToAction("Index", "Account");
                    }
                    else
                    {
                        tran.Rollback();
                    }

                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);
                    //Dictionary<string, string[]> d = CodeUtils.GetLevelItemList(conn);
                    //ViewBag.LevelCode = d["CODE"];
                    //ViewBag.LevelText = d["TEXT"];

                    ViewBag.List = action.GetList(model);

                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);

                    double pageSize = action.GetPageSize();
                    double totalCount = action.GetTotalCount();

                    ViewBag.NowPage = model.NowPage;
                    ViewBag.TotalCount = action.GetTotalCount();
                    ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

                    conn.Close();
                    conn.Dispose();
                }
            }

            return View(model);
        }

        /// <summary>
        /// 管理帳號 - 新增
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            AccountNewModel model = new AccountNewModel();
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                AccountModel accountModel = GetAccountModel();
                if (accountModel == null || accountModel.Scope == 1)
                {
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);
                }
                else
                {
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, accountModel.ScopeUnitCode, (string)null, false);
                }
                conn.Close();
                conn.Dispose();
            }

            this.SetVisitRecord("Account", "New", "新增管理帳號");

            return View(model);
        }

        /// <summary>
        /// 管理帳號 - 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult New(AccountNewModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    AccountAction action = new AccountAction(conn);
                    // 查詢是否已有重複帳號
                    var isRep = action.AccountExists(model.Account);
                    if (isRep)
                    {
                        AccountModel accountModel = GetAccountModel();
                        if (accountModel == null || accountModel.Scope == 1)
                        {
                            ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);
                        }
                        else
                        {
                            ViewBag.UnitList = CodeUtils.GetUnitList(conn, accountModel.ScopeUnitCode, (string)null, false);
                        }
                        ViewBag.tempMessage = "存檔失敗，帳號重複。";
                        return View(model);
                    }
                    conn.Close();
                    conn.Dispose();
                }

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    AccountAction action = new AccountAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.Insert(model))
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "1";
                        log_model.TX_TYPE = 1;
                        log_model.TX_DESC = model.Name;
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        return RedirectToAction("Index", "Account");
                    }
                    else
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "1";
                        log_model.TX_TYPE = 1;
                        log_model.TX_DESC = "存檔失敗";
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View(model);
        }

        /// <summary>
        /// 管理帳號 - 修改
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit()
        {
            string actionId = (string)TempData["ActionId"];

            if (String.IsNullOrEmpty(actionId))
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "Account");
            }

            AccountEditModel model = new AccountEditModel();
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                AccountModel accountModel = GetAccountModel();
                if (accountModel == null || accountModel.Scope == 1)
                {
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);
                }
                else
                {
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, accountModel.ScopeUnitCode, (string)null, false);
                }

                ViewBag.Scope = accountModel.Scope;

                //Dictionary<string, string[]> d = CodeUtils.GetLevelItemList(conn);
                //ViewBag.LevelCode = d["CODE"];
                //ViewBag.LevelText = d["TEXT"];

                AccountAction action = new AccountAction(conn);
                model = action.GetAccount(actionId);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 管理帳號 (修改 - 送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(AccountEditModel model)
        {
            //logger.Debug("Level: " + model.Level);
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    AccountAction action = new AccountAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.UpdateLevel(model) && action.Update(model))
                    {
                        //Log
                        AccountModel upd_Model = action.GetAccountModel(GetAccount());
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "1";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = model.Name;
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        return RedirectToAction("Index", "Account");
                    }
                    else
                    {
                        //Log
                        AccountModel upd_Model = action.GetAccountModel(GetAccount());
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "1";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = "存檔失敗";
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Rollback();
                    }
                    AccountModel accountModel = GetAccountModel();
                    if (accountModel == null || accountModel.Scope == 1)
                    {
                        ViewBag.UnitList = CodeUtils.GetUnitList(conn, true);
                    }
                    else
                    {
                        ViewBag.UnitList = CodeUtils.GetUnitList(conn, accountModel.ScopeUnitCode, (string)null, false);
                    }
                    Dictionary<string, string[]> d = CodeUtils.GetLevelItemList(conn);
                    ViewBag.LevelCode = d["CODE"];
                    ViewBag.LevelText = d["TEXT"];

                    conn.Close();
                    conn.Dispose();
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View(model);
        }

        /// <summary>
        /// 重設會員密碼 - 列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Reset()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ViewBag.QueryTypeList = CodeUtils.GetCodeSelectList(conn, "RST_PSWD", "", null, false);
                conn.Close();
                conn.Dispose();
            }

            this.SetVisitRecord("Account", "Reset", "重設會員密碼");

            return View();
        }

        /// <summary>
        /// 重設會員密碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Reset(MemberQueryModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Q"))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    ViewBag.QueryTypeList = CodeUtils.GetCodeSelectList(conn, "RST_PSWD", "", null, false);

                    AccountAction action = new AccountAction(conn);
                    ViewBag.List = action.GetMemberList(model);

                    conn.Close();
                    conn.Dispose();
                }
            }
            else if (model.ActionType.Equals("R"))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    AccountAction action = new AccountAction(conn, tran);
                    model.UpdateAccount = GetAccount();

                    if (action.ResetMemberPassword(model))
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "1";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = model.QueryId;
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        TempData["tempMessage"] = "重設密碼成功，重設之密碼與帳號相同";
                        tran.Commit();
                    }
                    else
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "1";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = "重設密碼失敗";
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        TempData["tempMessage"] = "重設密碼失敗";
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                return RedirectToAction("Reset", "Account");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult NewADSreach(string account)
        {
            AccountNewJsonModel anm = new AccountNewJsonModel();
            ES.Utils.ADUtils adu = new ES.Utils.ADUtils();
            Map map = adu.SearchAD(account);
            if (map != null)
            {
                anm.result = "true";
                anm.Account = map.GetString("samaccountname");
                anm.Mail = map.GetString("mail");
                anm.Name = map.GetString("name");
                anm.Tel = map.GetString("telephonenumber");
            }
            else
            {
                anm.result = "false";
                anm.Account = adu.Errmessage;
            }
            return Json(anm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult LevelList(string id)
        {
            List<Dictionary<String, Object>> list = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                AccountAction action = new AccountAction(conn);
                list = action.GetLevelList(id);
                conn.Close();
                conn.Dispose();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}
