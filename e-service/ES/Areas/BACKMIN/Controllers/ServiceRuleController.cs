using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Utils;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceRuleController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 4;
        }

        /// <summary>
        /// 案件稽催設定
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
                    ServiceRuleAction action = new ServiceRuleAction(conn);
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

            this.SetVisitRecord("ServiceRule", "Index", "案件稽催設定");

            return View();
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
                ServiceRuleAction action = new ServiceRuleAction(conn);

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
        /// 案件稽催設定 - 列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult List(int id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceRuleAction action = new ServiceRuleAction(conn);
                ViewBag.List = action.GetRuleList(id);
                ViewBag.Dict = action.GetCategoryById(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 預設稽催管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Setup()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            ServiceRuleEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceRuleAction action = new ServiceRuleAction(conn);
                model = action.GetSetup("000000");
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 預設稽催設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Setup(ServiceRuleEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceRuleAction action = new ServiceRuleAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    model.InitGet();
                    if (action.Update(model))
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isUpdate = true;
                    }
                    else
                    {
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isUpdate)
                {
                    return RedirectToAction("Index", "ServiceRule");
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

                ServiceRuleAction action = new ServiceRuleAction(conn);
                model = action.GetSetup(model.ServiceId);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 案件稽催設定 - 新增
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New(string id)
        {
            ServiceRuleEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceRuleAction action = new ServiceRuleAction(conn);
                model = action.GetNew(id);
                ViewBag.HeadList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, model.HeadAccount, true);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 案件稽催設定 - 新增 (存檔)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(ServiceRuleEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceRuleAction action = new ServiceRuleAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    model.InitGet();
                    if (action.Insert(model))
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isInsert = true;
                    }
                    else
                    {
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isInsert)
                {
                    return RedirectToAction("Index", "ServiceRule", new { id = model.CategoryId });
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

                ServiceRuleAction action = new ServiceRuleAction(conn);
                model = action.GetNew(model.ServiceId);
                ViewBag.HeadList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, model.HeadAccount, true);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 案件稽催設定 - 修改
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            ServiceRuleEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceRuleAction action = new ServiceRuleAction(conn);
                model = action.GetEdit(id);
                ViewBag.HeadList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, model.HeadAccount, true);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 案件稽催設定 - 修改 (存檔)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ServiceRuleEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceRuleAction action = new ServiceRuleAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    model.InitGet();
                    if (action.Update(model))
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isUpdate = true;
                    }
                    else
                    {
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isUpdate)
                {
                    return RedirectToAction("Index", "ServiceRule", new { id = model.CategoryId });
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

                ServiceRuleAction action = new ServiceRuleAction(conn);
                model = action.GetEdit(model.ServiceId);
                ViewBag.HeadList = CodeUtils.GetUnitAdminSelectList(conn, model.UnitCode, model.HeadAccount, true);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 案件稽催設定 - 刪除 (存檔)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(string id)
        {
            ServiceRuleEditModel model = new ServiceRuleEditModel();
            model.ServiceId = id.Split(',')[0];
            model.UpdateAccount = GetAccount();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                ServiceRuleAction action = new ServiceRuleAction(conn, tran);

                if (action.Delete(model))
                {
                    tran.Commit();
                    TempData["tempMessage"] = "刪除成功";
                }
                else
                {
                    tran.Rollback();
                    TempData["tempMessage"] = "刪除失敗";
                }
                conn.Close();
                conn.Dispose();
            }

            return RedirectToAction("Index", "ServiceRule", new { @id = id.Split(',')[1] });
        }
    }
}
