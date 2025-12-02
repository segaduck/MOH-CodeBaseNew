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
using ES.DataLayers;

namespace ES.Areas.Admin.Controllers
{
    public class QAController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 3;
        }

        /// <summary>
        /// QA管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            //logger.Debug("##public ActionResult Index()");

            ViewBag.tempMessage = TempData["tempMessage"];

            QAActionModel model = new QAActionModel();
            model.NowPage = 1;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                QAAction action = new QAAction(conn);
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
        /// 最新消息管理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(QAActionModel model)
        {
            //logger.Debug("##model.ActionType:" + DateTime.Today.ToString("yyyy/MM/dd HH:mm:ss"));

            ViewBag.tempMessage = TempData["tempMessage"];

            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Query"))
            {
                //logger.Debug("##model.ActionType.Equals(\"Query\")");
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    QAAction action = new QAAction(conn);

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
                TempData["QAActionModel"] = model;
                return RedirectToAction(model.ActionType, "QA");
            }
            else if (model.ActionType.Equals("Delete"))
            {
                model.UpdateAccount = GetAccount();
                var isDelete = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    QAAction action = new QAAction(conn, tran);
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
                    return RedirectToAction("Index", "QA");
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
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(QAEditModel model)
        {
            if (ModelState.IsValid)
            {

                var isInsertSuccess = this.NewDataInsert(model);
                if (isInsertSuccess)
                {
                    TempData["tempMessage"] = "存檔成功";
                    return RedirectToAction("Index", "QA");
                }
                else
                {
                    ViewBag.tempMessage = "存檔失敗";
                }
            }
            else
            {
                ViewBag.tempMessage = "欄位驗證失敗";
            }
            return View(model);
        }
        public bool NewDataInsert(QAEditModel model)
        {
            bool result = false;
            using (SqlConnection conn = GetConnection())
            {
                AccountModel account = GetAccountModel();
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                QAAction action = new QAAction(conn, tran);
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
            QAEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                QAAction action = new QAAction(conn);
                model = action.GetData(id);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(string id)
        {
            QAActionModel model = new QAActionModel();
            if (id == null) { return View(model); }

            Int32 i_id = 0;
            bool success = Int32.TryParse(id, out i_id);
            if (!success) { return View(model); }

            model.UpdateAccount = GetAccount();
            model.ActionId = i_id;

            var isDelete = false;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                QAAction action = new QAAction(conn, tran);
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
                return RedirectToAction("Index", "QA");
            }
            TempData["tempMessage"] = "刪除失敗";
            return RedirectToAction("Index", "QA");
        }


        /// <summary>
        /// 修改-no use
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditM()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            QAActionModel tmpModel = (QAActionModel)TempData["QAActionModel"];

            if (tmpModel == null)
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "QA");
            }

            QAEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                QAAction action = new QAAction(conn);
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
        public ActionResult Edit(QAEditModel model)
        {
            ShareDAO dao = new ShareDAO();
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    QAAction action = new QAAction(conn, tran);
                    model.UpdateAccount = GetAccount();

                    bool flag_saveok = action.Update(model);

                    if (flag_saveok)
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
                    return RedirectToAction("Index", "QA");
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
