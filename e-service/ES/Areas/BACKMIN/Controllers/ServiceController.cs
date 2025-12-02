using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Utils;
using ES.Areas.Admin.Models;
using log4net;
using System.Web.Configuration;
using System.IO;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 4;
        }

        /// <summary>
        /// 案件管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int? id)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (id != null)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    ServiceAction action = new ServiceAction(conn);
                    ViewBag.ExpandId = action.GetCategoryById((int)id)["SC_PID"];
                    ViewBag.ListId = (int)id;
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                ViewBag.ExpandId = 0;
                ViewBag.ListId = 0;
            }

            AccountModel accountModel = GetAccountModel();
            ViewBag.IsBaseNew = (accountModel != null && accountModel.Scope == 1);

            this.SetVisitRecord("Service", "Index", "案件管理");

            return View();
        }

        /// <summary>
        /// 依分類ID取得服務列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceList(int id)
        {
            ServiceBatchModel model = new ServiceBatchModel();
            model.ServiceId = id;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);
                ViewBag.List = action.GetServiceList(id);
                ViewBag.Dict = action.GetCategoryById(id);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 依父分類ID取得分類列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCategory(int? id)
        {
            List<Dictionary<String, Object>> list = null;

            int parentId = 0;

            if (id != null)
            {
                parentId = (int)id;
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);

                AccountModel accountModel = GetAccountModel();
                if (parentId == 0 && accountModel != null && accountModel.Scope != 1)
                {
                    list = action.GetCategoryList(parentId, accountModel.ServiceUnitCode);
                }
                else
                {
                    list = action.GetCategoryList(parentId, -1);
                }
                conn.Close();
                conn.Dispose();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新增分類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CategoryNew(int? id)
        {

            ServiceCategoryModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);

                if (id == null)
                {
                    model = action.GetRootCategoryName();
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, false);
                }
                else
                {
                    model = action.GetParentCategoryName((int)id);
                }
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 新增分類 - 送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CategoryNew(ServiceCategoryModel model)
        {

            Dictionary<String, Object> item = new Dictionary<string, object>();

            if (ModelState.IsValid)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceAction action = new ServiceAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.InsertCategory(model))
                    {
                        tran.Commit();
                        item.Add("message", "存檔成功");
                        item.Add("status", true);
                    }
                    else
                    {
                        tran.Rollback();
                        item.Add("message", "存檔失敗");
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                item.Add("message", "欄位驗證失敗");
            }

            if (!item.ContainsKey("status"))
            {
                item.Add("status", false);
            }

            // return Json(item, JsonRequestBehavior.AllowGet);
            ViewBag.tempMessage = TempData["tempMessage"];

            ViewBag.ExpandId = 0;
            ViewBag.ListId = 0;

            AccountModel accountModel = GetAccountModel();
            ViewBag.IsBaseNew = (accountModel != null && accountModel.Scope == 1);

            this.SetVisitRecord("Service", "Index", "案件管理");

            return View("index");
        }

        /// <summary>
        /// 修改分類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CategoryEdit(int id)
        {
            ServiceCategoryModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);

                model = action.GetCategory(id);
                if (model.ParentId == 0)
                {
                    ViewBag.UnitList = CodeUtils.GetUnitList(conn, false);
                }
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 修改分類 - 送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CategoryEdit(ServiceCategoryModel model)
        {
            Dictionary<String, Object> item = new Dictionary<string, object>();

            if (ModelState.IsValid)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceAction action = new ServiceAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.UpdateCategory(model))
                    {
                        tran.Commit();
                        item.Add("message", "存檔成功");
                        item.Add("status", true);
                    }
                    else
                    {
                        tran.Rollback();
                        item.Add("message", "存檔失敗");
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                item.Add("message", "欄位驗證失敗");
            }

            if (!item.ContainsKey("status"))
            {
                item.Add("status", false);
            }

            // return Json(item, JsonRequestBehavior.AllowGet);
            ViewBag.tempMessage = TempData["tempMessage"];

            ViewBag.ExpandId = 0;
            ViewBag.ListId = 0;

            AccountModel accountModel = GetAccountModel();
            ViewBag.IsBaseNew = (accountModel != null && accountModel.Scope == 1);

            this.SetVisitRecord("Service", "Index", "案件管理");

            return View("index");
        }

        /// <summary>
        /// 刪除分類
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CategoryDelete(int id)
        {
            ServiceCategoryModel model = new ServiceCategoryModel();
            model.CategoryId = id;

            Dictionary<String, Object> item = new Dictionary<string, object>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                ServiceAction action = new ServiceAction(conn, tran);
                model.UpdateAccount = GetAccount();
                if (action.DeleteCategory(model))
                {
                    tran.Commit();
                    item.Add("message", "刪除成功");
                    item.Add("status", true);
                }
                else
                {
                    tran.Rollback();
                    item.Add("message", "刪除失敗");
                }
                conn.Close();
                conn.Dispose();
            }

            if (!item.ContainsKey("status"))
            {
                item.Add("status", false);
            }

            // return Json(item, JsonRequestBehavior.AllowGet);
            ViewBag.tempMessage = TempData["tempMessage"];

            ViewBag.ExpandId = 0;
            ViewBag.ListId = 0;

            AccountModel accountModel = GetAccountModel();
            ViewBag.IsBaseNew = (accountModel != null && accountModel.Scope == 1);

            this.SetVisitRecord("Service", "Index", "案件管理");

            return View("index");
        }

        /// <summary>
        /// 新增案件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ServiceNew(int id)
        {
            ServiceEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceAction action = new ServiceAction(conn);
                model = action.GetParentServiceName(id);

                ViewBag.UnitList = CodeUtils.GetUnitList(conn, false);
                ViewBag.PayMethodList = CodeUtils.GetCodeCheckBoxList(conn, "PAY_METHOD", "", model.PayMethod);
                ViewBag.PayDeadlineList = CodeUtils.GetCodeSelectList(conn, "PAY_DEADLINE", "", model.PayDeadline, false);
                ViewBag.ArchiveCodeList = CodeUtils.GetMCaseClassSelectList(conn, model.UnitSCode, model.ArchiveCode, true);
                ViewBag.LoginTypeList = CodeUtils.GetCodeCheckBoxList(conn, "LOGIN_TYPE", "", model.LoginType);
                ViewBag.ApplyTargetList = CodeUtils.GetCodeCheckBoxList(conn, "APP_TARGET", "", model.ApplyTarget);
                ViewBag.FixUnitList = CodeUtils.GetUnitList(conn, model.UnitCode, model.FixUnitCode.ToString(), false);
                ViewBag.PageMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 143, model.PageMakerId, false);
                ViewBag.FileMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 144, model.FileMakerId, false);
                ViewBag.AssignUnitList = CodeUtils.GetUnitList(conn, model.UnitCode, model.AssignUnitCode, false);
                ViewBag.PayAccountList = CodeUtils.GetPayAccountList(conn, model.PayAccount, true);
                ViewBag.SharedList = CodeUtils.GetSharedList(conn, model.FormID, true);
                ViewBag.CaseFixUserList = CodeUtils.GetUnitACCList(conn, model.UnitCode, model.CaseMakerIds, false);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 新增案件 - 送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ServiceNew(ServiceEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceAction action = new ServiceAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    model.SetModel();
                    if (action.InsertService(model))
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "2";
                        log_model.TX_TYPE = 1;
                        log_model.TX_DESC = model.Name;
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isInsert = true;
                    }
                    else
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "2";
                        log_model.TX_TYPE = 1;
                        log_model.TX_DESC = "存檔失敗";
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isInsert)
                {
                    return RedirectToAction("Index", "Service", new { id = model.CategoryId });
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ViewBag.UnitList = CodeUtils.GetUnitList(conn, false);
                ViewBag.PayMethodList = CodeUtils.GetCodeCheckBoxList(conn, "PAY_METHOD", "", model.PayMethod);
                ViewBag.PayDeadlineList = CodeUtils.GetCodeSelectList(conn, "PAY_DEADLINE", "", model.PayDeadline, false);
                ViewBag.ArchiveCodeList = CodeUtils.GetMCaseClassSelectList(conn, model.UnitSCode, model.ArchiveCode, true);
                ViewBag.LoginTypeList = CodeUtils.GetCodeCheckBoxList(conn, "LOGIN_TYPE", "", model.LoginType);
                ViewBag.ApplyTargetList = CodeUtils.GetCodeCheckBoxList(conn, "APP_TARGET", "", model.ApplyTarget);
                ViewBag.FixUnitList = CodeUtils.GetUnitList(conn, model.UnitCode, model.FixUnitCode.ToString(), false);
                ViewBag.PageMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 143, model.PageMakerId, false);
                ViewBag.FileMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 144, model.FileMakerId, false);
                ViewBag.PayAccountList = CodeUtils.GetPayAccountList(conn, model.PayAccount, true);
                ViewBag.SharedList = CodeUtils.GetSharedList(conn, model.FormID, true);
                ViewBag.CaseFixUserList = CodeUtils.GetUnitACCList(conn, model.UnitCode, model.CaseMakerIds, false);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 修改案件
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ServiceEdit(string id)
        {
            ServiceEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceAction action = new ServiceAction(conn);
                model = action.GetService(id);

                ViewBag.UnitList = CodeUtils.GetUnitList(conn, false);
                ViewBag.PayMethodList = CodeUtils.GetCodeCheckBoxList(conn, "PAY_METHOD", "", model.PayMethod);
                ViewBag.PayDeadlineList = CodeUtils.GetCodeSelectList(conn, "PAY_DEADLINE", "", model.PayDeadline, false);
                ViewBag.ArchiveCodeList = CodeUtils.GetMCaseClassSelectList(conn, model.UnitSCode, model.ArchiveCode, true);
                ViewBag.LoginTypeList = CodeUtils.GetCodeCheckBoxList(conn, "LOGIN_TYPE", "", model.LoginType);
                ViewBag.ApplyTargetList = CodeUtils.GetCodeCheckBoxList(conn, "APP_TARGET", "", model.ApplyTarget);
                ViewBag.FixUnitList = CodeUtils.GetUnitList(conn, model.UnitCode, model.FixUnitCode.ToString(), false);
                ViewBag.PageMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 143, model.PageMakerId, false);  // 申請須知管理者
                ViewBag.FileMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 144, model.FileMakerId, false);  // 書表下載管理者
                ViewBag.AssignUnitList = CodeUtils.GetUnitList(conn, model.UnitCode, model.AssignUnitCode, false);
                ViewBag.PayAccountList = CodeUtils.GetPayAccountList(conn, model.PayAccount, true);
                ViewBag.SharedList = CodeUtils.GetSharedList(conn, model.FormID, true);
                ViewBag.CaseFixUserList = CodeUtils.GetUnitACCList(conn, model.UnitCode, model.CaseMakerIds, false);

                logger.Debug("test: " + model.FormID);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 修改案件 - 送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ServiceEdit(ServiceEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceAction action = new ServiceAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    model.SetModel();
                    if (action.UpdateService(model))
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "2";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = model.Name;
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isInsert = true;
                    }
                    else
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "2";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = "存檔失敗";
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isInsert)
                {
                    return RedirectToAction("Index", "Service", new { id = model.CategoryId });
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ViewBag.UnitList = CodeUtils.GetUnitList(conn, false);
                ViewBag.PayMethodList = CodeUtils.GetCodeCheckBoxList(conn, "PAY_METHOD", "", model.PayMethod);
                ViewBag.PayDeadlineList = CodeUtils.GetCodeSelectList(conn, "PAY_DEADLINE", "", model.PayDeadline, false);
                ViewBag.ArchiveCodeList = CodeUtils.GetMCaseClassSelectList(conn, model.UnitSCode, model.ArchiveCode, true);
                ViewBag.LoginTypeList = CodeUtils.GetCodeCheckBoxList(conn, "LOGIN_TYPE", "", model.LoginType);
                ViewBag.ApplyTargetList = CodeUtils.GetCodeCheckBoxList(conn, "APP_TARGET", "", model.ApplyTarget);
                ViewBag.FixUnitList = CodeUtils.GetUnitList(conn, model.UnitCode, model.FixUnitCode.ToString(), false);
                ViewBag.PageMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 143, model.PageMakerId, false);
                ViewBag.FileMakerList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, 144, model.FileMakerId, false);
                ViewBag.AssignUnitList = CodeUtils.GetUnitList(conn, model.UnitCode, model.AssignUnitCode, false);
                ViewBag.PayAccountList = CodeUtils.GetPayAccountList(conn, model.PayAccount, true);
                ViewBag.SharedList = CodeUtils.GetSharedList(conn, model.FormID, true);
                ViewBag.CaseFixUserList = CodeUtils.GetUnitACCList(conn, model.UnitCode, model.CaseMakerIds, false);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 批次修改案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ServiceBatchUpdate(ServiceBatchModel model)
        {
            Dictionary<String, Object> item = new Dictionary<string, object>();
            /*
            logger.Debug("ServiceBatchUpdate");
            logger.Debug("Type: " + model.Type);
            logger.Debug("ActionId: " + model.ActionId.Count());
            logger.Debug("ServiceId: " + model.ServiceId);
            */
            item.Add("id", model.ServiceId);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                ServiceAction action = new ServiceAction(conn, tran);
                model.UpdateAccount = GetAccount();
                if (action.BatchUpdateService(model))
                {
                    //Log
                    AccountModel upd_Model = GetAccountModel();
                    UtilityAction log_action = new UtilityAction(conn, tran);
                    UtilityModel log_model = new UtilityModel();
                    log_model.TX_CATE_CD = "2";
                    if (model.Type.Equals("D"))
                    {
                        log_model.TX_TYPE = 3;
                    }
                    else
                    {
                        log_model.TX_TYPE = 2;
                    }
                    log_action.InsertServiceBatch(log_model, upd_Model, model.ActionId);
                    //log end;
                    tran.Commit();
                    item.Add("message", "批次異動成功");
                    item.Add("status", true);
                }
                else
                {
                    //Log
                    AccountModel upd_Model = GetAccountModel();
                    UtilityAction log_action = new UtilityAction(conn, tran);
                    UtilityModel log_model = new UtilityModel();
                    log_model.TX_CATE_CD = "2";
                    log_model.TX_TYPE = 2;
                    log_model.TX_DESC = "批次異動失敗";
                    log_action.Insert(log_model, upd_Model);
                    //log end;
                    tran.Rollback();
                    item.Add("message", "批次異動失敗");
                }
                conn.Close();
                conn.Dispose();
            }

            if (!item.ContainsKey("status"))
            {
                item.Add("status", false);
            }

            // return Json(item, JsonRequestBehavior.AllowGet);
            ViewBag.tempMessage = TempData["tempMessage"];

            ViewBag.ExpandId = 0;
            ViewBag.ListId = 0;

            AccountModel accountModel = GetAccountModel();
            ViewBag.IsBaseNew = (accountModel != null && accountModel.Scope == 1);

            this.SetVisitRecord("Service", "Index", "案件管理");

            return View("Index");
        }
    }
}
