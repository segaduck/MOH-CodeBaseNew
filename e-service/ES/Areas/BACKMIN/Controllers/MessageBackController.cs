using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Utils;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MessageBackController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 2;
        }

        /// <summary>
        /// 最新消息管理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            MessageBackActionModel model = new MessageBackActionModel();
            model.NowPage = 1;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                MessageBackAction action = new MessageBackAction(conn);
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
        public ActionResult Index(MessageBackActionModel model)
        {
            //logger.Debug("##model.ActionType:" + DateTime.Today.ToString("yyyy/MM/dd HH:mm:ss"));

            ViewBag.tempMessage = TempData["tempMessage"];

            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Query"))
            {
                //logger.Debug("##model.ActionType.Equals(\"Query\")");
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MessageBackAction action = new MessageBackAction(conn);

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
                TempData["MessageBackActionModel"] = model;
                return RedirectToAction(model.ActionType, "Message");
            }
            else if (model.ActionType.Equals("Delete"))
            {
                model.UpdateAccount = GetAccount();
                var isDelete = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    MessageBackAction action = new MessageBackAction(conn, tran);
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
                    return RedirectToAction("Index", "MessageBack");
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
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ViewBag.CategoryList = CodeUtils.GetCodeCheckBoxList(conn, "MSG_CATE", "", null);

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
        public ActionResult New(MessageBackEditModel model)
        {
            if (ModelState.IsValid)
            {

                logger.Debug("Request.Files.Count: " + Request.Files.Count);


                if (Request.Files.Count > 2)
                {
                    //string folder = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string dir = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE");
                    string filename = null;

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (!String.IsNullOrEmpty(Request.Files[0].FileName))
                    {
                        filename = Path.GetFileName(Request.Files[0].FileName);
                        Request.Files[0].SaveAs(dir + filename);
                        model.FileName1 = filename;
                    }

                    if (!String.IsNullOrEmpty(Request.Files[1].FileName))
                    {
                        filename = Path.GetFileName(Request.Files[1].FileName);
                        Request.Files[1].SaveAs(dir + filename);
                        model.FileName2 = filename;
                    }

                    if (!String.IsNullOrEmpty(Request.Files[2].FileName))
                    {
                        filename = Path.GetFileName(Request.Files[2].FileName);
                        Request.Files[2].SaveAs(dir + filename);
                        model.FileName3 = filename;
                    }
                }

                using (SqlConnection conn = GetConnection())
                {
                    AccountModel account = GetAccountModel();
                    model.UnitCode = Int32.Parse(account.UnitCode);

                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    MessageBackAction action = new MessageBackAction(conn, tran);
                    model.UpdateAccount = GetAccount();

                    bool flag_saveok = action.Insert(model);

                    if (flag_saveok)
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        return RedirectToAction("Index", "MessageBack");
                    }
                    else
                    {
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
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

                ViewBag.CategoryList = CodeUtils.GetCodeCheckBoxList(conn, "MSG_CATE", "", model.Category);

                conn.Close();
                conn.Dispose();
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
            //ViewBag.tempMessage = TempData["tempMessage"];
            //MessageBackActionModel tmpModel = (MessageBackActionModel)TempData["MessageBackActionModel"];

            //if (tmpModel == null)
            //{
            //    TempData["tempMessage"] = "參數異常";
            //    return RedirectToAction("Index", "MessageBack");
            //}

            MessageBackEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                MessageBackAction action = new MessageBackAction(conn);
                model = action.GetData(id);
                ViewBag.CategoryList = CodeUtils.GetCodeCheckBoxList(conn, "MSG_CATE", "", model.Category);
                ViewBag.MessageFileFolder = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE");

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
            MessageBackActionModel model = new MessageBackActionModel();
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
                MessageBackAction action = new MessageBackAction(conn, tran);
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
                return RedirectToAction("Index", "MessageBack");
            }
            TempData["tempMessage"] = "刪除失敗";
            //return Json(new { status = false });
            return RedirectToAction("Index", "MessageBack");
        }

        /// <summary>
        /// 修改-no use
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditM()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            MessageBackActionModel tmpModel = (MessageBackActionModel)TempData["MessageBackActionModel"];

            if (tmpModel == null)
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "MessageBack");
            }

            MessageBackEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                MessageBackAction action = new MessageBackAction(conn);
                model = action.GetData(tmpModel.ActionId);
                ViewBag.CategoryList = CodeUtils.GetCodeCheckBoxList(conn, "MSG_CATE", "", model.Category);
                ViewBag.MessageFileFolder = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE");

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
        public ActionResult Edit(MessageBackEditModel model)
        {
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    AccountModel account = GetAccountModel();
                    model.UnitCode = Int32.Parse(account.UnitCode);

                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    MessageBackAction action = new MessageBackAction(conn, tran);
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
                    return RedirectToAction("Index", "MessageBack");
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

                ViewBag.CategoryList = CodeUtils.GetCodeCheckBoxList(conn, "MSG_CATE", "", model.Category);
                ViewBag.MessageFileFolder = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE");
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        public ActionResult Down(string id)
        {
            string filePath = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + Request.Params["file"].ToString();
            string fileName = Path.GetFileName(filePath);
            Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(iStream, "application/unknown", fileName);
        }

        /// <summary>
        /// 顯示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Show(int id)
        {
            MessageBackShowModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                MessageBackAction action = new MessageBackAction(conn);

                model = action.GetDataToShow(id);

                ViewBag.MessageFileFolder = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE");

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }
    }
}
