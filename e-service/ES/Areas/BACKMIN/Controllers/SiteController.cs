using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SiteController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 3;
        }

        /// <summary>
        /// 相關網站連結
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SiteAction action = new SiteAction(conn);
                ViewBag.List = action.GetList();
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            SiteEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SiteAction action = new SiteAction(conn);
                model = action.GetNewModel();
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(SiteEditModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    SiteAction action = new SiteAction(conn, tran);
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
                    return RedirectToAction("Index", "Site");
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(int id)
        {
            SiteEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SiteAction action = new SiteAction(conn);
                model = action.GetData(id);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(SiteEditModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    SiteAction action = new SiteAction(conn, tran);
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
                    return RedirectToAction("Index", "Site");
                }
                ViewBag.tempMessage = "存檔失敗";
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View(model);
        }

        public ActionResult Delete(int id)
        {

            SiteEditModel model = new SiteEditModel();
            model.SiteId = id;
            model.UpdateAccount = GetAccount();
            var isDelete = false;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                SiteAction action = new SiteAction(conn, tran);
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
                return RedirectToAction("Index", "Site");
            }
            TempData["tempMessage"] = "刪除失敗";

            return RedirectToAction("Index", "Site");
        }
    }
}
