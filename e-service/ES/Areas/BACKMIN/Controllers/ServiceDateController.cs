using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Utils;
using ES.DataLayers;
using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceDateController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 3;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ServiceDateActionModel model = new ServiceDateActionModel();
            model.ActionType = "Query";
            Index(model);
            return View(model);
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(ServiceDateActionModel model)
        {
            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Query"))
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    ServiceDateAction action = new ServiceDateAction(conn);

                    ViewBag.List = action.GetList(model);

                    //double pageSize = action.GetPageSize();
                    //double totalCount = action.GetTotalCount();

                    //ViewBag.NowPage = model.NowPage;
                    //ViewBag.TotalCount = action.GetTotalCount();
                    //ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

                    conn.Close();
                    conn.Dispose();
                }
            }
            else if (model.ActionType.Equals("Edit"))
            {
                TempData["ServiceDateActionModel"] = model;
                return RedirectToAction(model.ActionType, "ServiceDate");
            }
            else if (model.ActionType.Equals("Delete"))
            {
                model.UpdateAccount = GetAccount();
                var isDelete = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceDateAction action = new ServiceDateAction(conn, tran);
                    if (action.Delete(model))
                    {
                        tran.Commit();
                        TempData["tempServiceDate"] = "刪除成功";
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
                    return RedirectToAction("Index", "ServiceDate");
                }
                TempData["tempServiceDate"] = "刪除失敗";
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
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(ServiceDateEditModel model)
        {
            if (ModelState.IsValid)
            {
                logger.Debug("Request.Files.Count: " + Request.Files.Count);
                // 檔案上傳
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    ServiceDateAction action = new ServiceDateAction(conn);

                    conn.Close();
                    conn.Dispose();
                }
                var isInsertSuccess = this.NewDataInsert(model);
                if (isInsertSuccess)
                {
                    TempData["tempServiceDate"] = "存檔成功";
                    return RedirectToAction("Index", "ServiceDate");
                }
                else
                {
                    ViewBag.tempServiceDate = "存檔失敗";
                }
            }
            else
            {
                ViewBag.tempServiceDate = "欄位驗證失敗";
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }
        public bool NewDataInsert(ServiceDateEditModel model)
        {
            bool result = false;
            using (SqlConnection conn = GetConnection())
            {
                AccountModel account = GetAccountModel();

                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                ServiceDateAction action = new ServiceDateAction(conn, tran);
                model.UpdateAccount = GetAccount();

                bool flag_saveok = action.Insert(model);
                if (flag_saveok)
                {
                    tran.Commit();
                    result = true;
                }
                else
                {
                    tran.Rollback();
                }
                conn.Close();
                conn.Dispose();
            }

            return result;
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(int id)
        {
            ServiceDateEditModel model = null;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceDateAction action = new ServiceDateAction(conn);
                model = action.GetData(id);
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
        public ActionResult Edit(ServiceDateEditModel model)
        {
            ShareDAO dao = new ShareDAO();
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    AccountModel account = GetAccountModel();
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceDateAction action = new ServiceDateAction(conn, tran);
                    model.UpdateAccount = GetAccount();
                    bool flag_saveok = action.Update(model);
                    if (flag_saveok)
                    {
                        tran.Commit();
                        TempData["tempServiceDate"] = "存檔成功";
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
                    return RedirectToAction("Index", "ServiceDate");
                }
                ViewBag.tempServiceDate = "存檔失敗";
            }
            else
            {
                ViewBag.tempServiceDate = "欄位驗證失敗";
            }
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }
    }
}
