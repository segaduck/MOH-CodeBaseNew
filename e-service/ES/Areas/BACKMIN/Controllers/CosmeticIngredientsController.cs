using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CosmeticIngredientsController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 9;
        }

        /// <summary>
        /// 一般化妝品成分管理 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            CosmeticIngredientsActionModel model = new CosmeticIngredientsActionModel();
            model.NowPage = 1;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                CosmeticIngredientsAction action = new CosmeticIngredientsAction(conn);
                ViewBag.List = action.GetList(model);

                double pageSize = action.GetPageSize();
                double totalCount = action.GetTotalCount();

                ViewBag.NowPage = model.NowPage;
                ViewBag.TotalCount = action.GetTotalCount();
                ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 一般化妝品成分管理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(CosmeticIngredientsActionModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Query"))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    CosmeticIngredientsAction action = new CosmeticIngredientsAction(conn);
                    ViewBag.List = action.GetList(model);

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
                TempData["CosmeticIngredientsQueryModel"] = model;
                return RedirectToAction(model.ActionType, "CosmeticIngredients");
            }
            else if (model.ActionType.Equals("Delete"))
            {
                var isDelete = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    CosmeticIngredientsAction action = new CosmeticIngredientsAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.Delete(model))
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "刪除成功";
                        isDelete = true;
                    }
                    else
                    {
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isDelete)
                {
                    return RedirectToAction("Index", "CosmeticIngredients");
                }
                TempData["tempMessage"] = "刪除失敗";
            }

            return View(model);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            return View();
        }

        /// <summary>
        /// 新增 - 送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(CosmeticIngredientsEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    CosmeticIngredientsAction action = new CosmeticIngredientsAction(conn, tran);
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
                    return RedirectToAction("Index", "CosmeticIngredients");
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
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            CosmeticIngredientsActionModel tmpModel = (CosmeticIngredientsActionModel)TempData["CosmeticIngredientsQueryModel"];

            if (tmpModel == null)
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "CosmeticIngredients");
            }

            CosmeticIngredientsEditModel model = null;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                CosmeticIngredientsAction action = new CosmeticIngredientsAction(conn);
                model = action.GetData(tmpModel.ActionId);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 修改 - 送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(CosmeticIngredientsEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    CosmeticIngredientsAction action = new CosmeticIngredientsAction(conn, tran);
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
                    return RedirectToAction("Index", "CosmeticIngredients");
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View(model);
        }
    }
}
