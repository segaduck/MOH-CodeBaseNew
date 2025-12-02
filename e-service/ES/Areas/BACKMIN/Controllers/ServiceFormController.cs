using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceFormController :  BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 4;
        }

        /// <summary>
        /// 表單設定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Index(int? id)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (id != null)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    ServiceFormAction action = new ServiceFormAction(conn);
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
                ServiceNoticeAction action = new ServiceNoticeAction(conn);

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
        /// 表單設定 - 案件列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceList(int id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceFormAction action = new ServiceFormAction(conn);
                ViewBag.List = action.GetServiceList(id);
                ViewBag.Dict = action.GetCategoryById(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 表單設定 - 修改
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ActionResult Edit(string id)
        {
            ServiceFormEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceFormAction action = new ServiceFormAction(conn);
                model = action.GetEdit(id);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 表單設定 - 修改 (送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ServiceFormEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceFormAction action = new ServiceFormAction(conn, tran);
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
                if(isUpdate)
                {
                    return RedirectToAction("Index", new { id = model.CategoryId });
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

                ServiceFormAction action = new ServiceFormAction(conn);
                model = action.GetEdit(model.ServiceId);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }
    }
}
