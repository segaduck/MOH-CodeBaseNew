using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SetupController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SetupAction action = new SetupAction(conn);

                ViewBag.List = action.GetList();
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        [HttpGet]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(SetupModel model)
        {
            if (ModelState.IsValid)
            {
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    SetupAction action = new SetupAction(conn, tran);

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
                        ViewBag.tempMessage = "存檔失敗";
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isInsert)
                {
                    return RedirectToAction("Index", "Setup");
                }
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            SetupModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SetupAction action = new SetupAction(conn);

                model = action.GetData(id);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(SetupModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    SetupAction action = new SetupAction(conn, tran);

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
                        ViewBag.tempMessage = "存檔失敗";
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isUpdate)
                {
                    return RedirectToAction("Index", "Setup");
                }
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View();
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            SetupModel model = new SetupModel();
            model.SetupCode = id;
            model.UpdateAccount = GetAccount();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                SetupAction action = new SetupAction(conn, tran);

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

            return RedirectToAction("Index");
        }

        public ActionResult Reset()
        {
            ES.Utils.DataUtils.ClearConfig();

            TempData["tempMessage"] = "重設成功";

            return RedirectToAction("Index");
        }
    }
}
