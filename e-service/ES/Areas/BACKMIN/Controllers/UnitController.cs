using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;
using log4net;
using System.Web.Routing;
using NPOI.OpenXmlFormats.Dml.ChartDrawing;

namespace ES.Areas.Admin.Controllers
{
    public class UnitController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 2;
        }

        /// <summary>
        /// 單位管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                UnitAction action = new UnitAction(conn);
                ViewBag.List = action.GetList();
                conn.Close();
                conn.Dispose();
            }

            this.SetVisitRecord("Unit", "Index", "單位管理");

            return View();
        }

        /// <summary>
        /// 單位管理 (送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(UnitModel model)
        {

            if (ModelState.IsValid)
            {
                TempData["UnitModel"] = model;
                if (model.ActionType.Equals("N"))
                {
                    return RedirectToAction("New", "Unit");
                }
                else if (model.ActionType.Equals("E"))
                {
                    return RedirectToAction("Edit", "Unit");
                }
                else if (model.ActionType.Equals("D"))
                {
                    using (SqlConnection conn = GetConnection())
                    {
                        conn.Open();
                        SqlTransaction tran = conn.BeginTransaction();

                        UnitAction action = new UnitAction(conn, tran);

                        if (action.Delete(model))
                        {
                            tran.Commit();
                            ViewBag.tempMessage = "刪除成功";
                        }
                        else
                        {
                            tran.Rollback();
                            ViewBag.tempMessage = "刪除失敗";
                        }
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                UnitAction action = new UnitAction(conn);
                ViewBag.List = action.GetList();
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 單位管理 (新增)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            UnitModel tmpModel = (UnitModel)TempData["UnitModel"];

            if (tmpModel == null)
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "Unit");
            }

            UnitEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                UnitAction action = new UnitAction(conn);
                if (tmpModel.UnitCode == 0)
                {
                    model = action.GetRootUnit();
                }
                else
                {
                    model = action.GetParentUnit(tmpModel.UnitCode);
                }
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 單位管理 (新增 - 送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(UnitEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    UnitAction action = new UnitAction(conn, tran);
                    model.UpdateAccount = GetAccount();
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
                    return RedirectToAction("Index", "Unit");
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View();
        }

        /// <summary>
        /// 單位管理 (修改)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit()
        {
            UnitModel tmpModel = (UnitModel)TempData["UnitModel"];

            if (tmpModel == null)
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "Unit");
            }

            UnitEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                UnitAction action = new UnitAction(conn);
                model = action.GetUnit(tmpModel.UnitCode);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 單位管理 (修改 - 送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(UnitEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    UnitAction action = new UnitAction(conn, tran);
                    model.UpdateAccount = GetAccount();
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
                    return RedirectToAction("Index", "Unit");
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View();
        }
    }
}
