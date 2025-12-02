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
    public class ServiceApplyOpenController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceApplyOpenAction action = new ServiceApplyOpenAction(conn);

                ViewBag.List = action.GetList();
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit()
        {
            ServiceApplyOpenModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceApplyOpenAction action = new ServiceApplyOpenAction(conn);

                model = action.GetData("011010");

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ServiceApplyOpenModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    ServiceApplyOpenAction action = new ServiceApplyOpenAction(conn, tran);

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
                    return RedirectToAction("Index", "ServiceApplyOpen");
                }
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }

            return View();
        }

    }
}
