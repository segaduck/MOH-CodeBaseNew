using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CosmeticAdvertisingController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 9;
        }

        /// <summary>
        /// 化妝品廣告內容管理 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            CosmeticAdvertisingActionModel model = new CosmeticAdvertisingActionModel();
            model.NowPage = 1;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                CosmeticAdvertisingAction action = new CosmeticAdvertisingAction(conn);
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
        /// 化妝品廣告內容管理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(CosmeticAdvertisingActionModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Query"))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    CosmeticAdvertisingAction action = new CosmeticAdvertisingAction(conn);
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
                TempData["CosmeticAdvertisingQueryModel"] = model;
                return RedirectToAction(model.ActionType, "CosmeticAdvertising");
            }
            else if (model.ActionType.Equals("Delete"))
            {
                var isDelete = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    CosmeticAdvertisingAction action = new CosmeticAdvertisingAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.Delete(model))
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "刪除成功";
                        isDelete= true;
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
                    return RedirectToAction("Index", "CosmeticAdvertising");
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
        public ActionResult New(CosmeticAdvertisingEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    CosmeticAdvertisingAction action = new CosmeticAdvertisingAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.Insert(model))
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isInsert= true;
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
                    return RedirectToAction("Index", "CosmeticAdvertising");
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
            CosmeticAdvertisingActionModel tmpModel = (CosmeticAdvertisingActionModel)TempData["CosmeticAdvertisingQueryModel"];

            if (tmpModel == null)
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "CosmeticAdvertising");
            }

            CosmeticAdvertisingEditModel model = null;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                CosmeticAdvertisingAction action = new CosmeticAdvertisingAction(conn);
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
        public ActionResult Edit(CosmeticAdvertisingEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    CosmeticAdvertisingAction action = new CosmeticAdvertisingAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.Update(model))
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isUpdate= true;
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
                    return RedirectToAction("Index", "CosmeticAdvertising");
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
