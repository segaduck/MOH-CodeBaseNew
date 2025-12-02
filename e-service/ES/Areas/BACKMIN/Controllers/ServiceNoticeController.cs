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
    /// <summary>
    /// 申請須知管理
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ServiceNoticeController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 4;
        }

        /// <summary>
        /// 申請須知管理
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
                    ServiceNoticeAction action = new ServiceNoticeAction(conn);
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

            this.SetVisitRecord("ServiceNotice", "Index", "申請須知管理");

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
        /// 申請須知管理 - 案件列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceList(int id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceNoticeAction action = new ServiceNoticeAction(conn);
                ViewBag.List = action.GetServiceList(id);
                ViewBag.Dict = action.GetCategoryById(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 申請須知管理 - 修改
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public ActionResult Edit(string id)
        {
            ServiceNoticeEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceNoticeAction action = new ServiceNoticeAction(conn);
                model = action.GetEdit(id);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 申請須知管理 - 修改 (送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ServiceNoticeEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceNoticeAction action = new ServiceNoticeAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    if (action.Update(model))
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "3";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = log_action.getServiceName(model.ServiceId);
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isUpdate = true;
                    }
                    else
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "3";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = "存檔失敗";
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isUpdate)
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

                ServiceNoticeAction action = new ServiceNoticeAction(conn);
                model = action.GetEdit(model.ServiceId);
            }

            return View(model);
        }
    }
}
