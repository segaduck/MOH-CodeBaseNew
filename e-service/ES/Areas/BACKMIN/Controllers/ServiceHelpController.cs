using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using System.Web.Configuration;
using System.IO;
using System.Web.Routing;
using ES.Utils;

namespace ES.Areas.Admin.Controllers
{
    /// <summary>
    /// 操作輔助說明管理
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ServiceHelpController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 4;
        }

        /// <summary>
        /// 操作輔助說明管理
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
                    ServiceHelpAction action = new ServiceHelpAction(conn);
                    ViewBag.ExpandId = action.GetCategoryById((int)id)["SC_PID"];

                    this.SetVisitRecord("ServiceHelp", "Index", "操作輔助說明管理");

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
                ServiceHelpAction action = new ServiceHelpAction(conn);

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
        /// 操作輔助說明管理 - 案件列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceList(int id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceHelpAction action = new ServiceHelpAction(conn);
                ViewBag.List = action.GetServiceList(id);
                ViewBag.Dict = action.GetCategoryById(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 操作輔助說明管理 - 列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult List(string id)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceHelpAction action = new ServiceHelpAction(conn);

                ViewBag.List = action.GetList(id);
                ViewBag.Dict = action.GetServiceName(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 操作輔助說明管理 - 新增
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult New(string id)
        {
            ServiceHelpNewModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceHelpAction action = new ServiceHelpAction(conn);
                model = action.GetNew(id);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 操作輔助說明管理 - 新增 (送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(ServiceHelpNewModel model)
        {
            if (ModelState.IsValid)
            {
                if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                {
                    string dir = DataUtils.GetConfig("FOLDER_SERVICE_HELP") + model.ServiceId + "\\";
                    string filename = Path.GetFileName(Request.Files[0].FileName);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    Request.Files[0].SaveAs(dir + filename);
                    model.FileName = filename;
                }
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceHelpAction action = new ServiceHelpAction(conn, tran);
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
                    return RedirectToAction("List", "ServiceHelp", new { @id = model.ServiceId });
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

                ServiceHelpAction action = new ServiceHelpAction(conn);
                model = action.GetNew(model.ServiceId);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 操作輔助說明管理 - 修改
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(string id, int fid)
        {
            ServiceHelpEditModel model = new ServiceHelpEditModel();
            model.ServiceId = id;
            model.FileId = fid;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceHelpAction action = new ServiceHelpAction(conn);
                model = action.GetEdit(model);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 操作輔助說明管理 - 修改 (送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ServiceHelpEditModel model)
        {
            if (ModelState.IsValid)
            {
                if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                {
                    string dir = DataUtils.GetConfig("FOLDER_SERVICE_HELP") + model.ServiceId + "\\";
                    string filename = Path.GetFileName(Request.Files[0].FileName);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    Request.Files[0].SaveAs(dir + filename);
                    model.FileName = filename;
                }
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceHelpAction action = new ServiceHelpAction(conn, tran);
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
                    return RedirectToAction("List", "ServiceHelp", new { @id = model.ServiceId });
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

                ServiceHelpAction action = new ServiceHelpAction(conn);
                model = action.GetEdit(model);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 操作輔助說明管理 - 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult Delete(string id, int fid)
        {
            ServiceHelpEditModel model = new ServiceHelpEditModel();
            model.ServiceId = id;
            model.FileId = fid;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                ServiceHelpAction action = new ServiceHelpAction(conn, tran);
                model.UpdateAccount = GetAccount();

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

            return RedirectToAction("List", "ServiceHelp", new { @id = model.ServiceId });
        }

        /// <summary>
        /// 操作輔助說明管理 - 檔案下載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult Download(string id, int fid)
        {
            ServiceHelpEditModel model = new ServiceHelpEditModel();
            model.ServiceId = id;
            model.FileId = fid;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceHelpAction action = new ServiceHelpAction(conn);
                model = action.GetEdit(model);
                conn.Close();
                conn.Dispose();
            }

            string filePath = DataUtils.GetConfig("FOLDER_SERVICE_HELP") + id + "\\" + model.FileName;
            //logger.Debug("filePath: " + filePath);
            string fileName = Path.GetFileName(filePath);
            Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(iStream, "application/unknown", fileName);
        }

    }
}
