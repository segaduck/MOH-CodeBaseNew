using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Utils;
using System.Web.Configuration;
using System.IO;
using log4net;
using System.Web.Routing;
using ES.DataLayers;
using ES.Services;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MessageController : BaseController
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
            //logger.Debug("##public ActionResult Index()");

            ViewBag.tempMessage = TempData["tempMessage"];

            MessageActionModel model = new MessageActionModel();
            model.NowPage = 1;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                MessageAction action = new MessageAction(conn);
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
        public ActionResult Index(MessageActionModel model)
        {
            //logger.Debug("##model.ActionType:" + DateTime.Today.ToString("yyyy/MM/dd HH:mm:ss"));

            ViewBag.tempMessage = TempData["tempMessage"];

            if (String.IsNullOrEmpty(model.ActionType) || model.ActionType.Equals("Query"))
            {
                //logger.Debug("##model.ActionType.Equals(\"Query\")");
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MessageAction action = new MessageAction(conn);

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
                TempData["MessageActionModel"] = model;
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
                    MessageAction action = new MessageAction(conn, tran);
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
                    return RedirectToAction("Index", "Message");
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
        public ActionResult New(MessageEditModel model)
        {
            if (ModelState.IsValid)
            {
                logger.Debug("Request.Files.Count: " + Request.Files.Count);
                // 檔案上傳
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MessageAction action = new MessageAction(conn);

                    if (Request.Files.Count > 2)
                    {
                        // 取得新增ID
                        var maxid = action.GetDataNewID();
                        string dirConfig = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE");
                        string dir = dirConfig + maxid.ToString() + "\\";
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
                    conn.Close();
                    conn.Dispose();
                }
                var isInsertSuccess = this.NewDataInsert(model);
                if (isInsertSuccess)
                {
                    TempData["tempMessage"] = "存檔成功";
                    return RedirectToAction("Index", "Message");
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

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ViewBag.CategoryList = CodeUtils.GetCodeCheckBoxList(conn, "MSG_CATE", "", model.Category);

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }
        public bool NewDataInsert(MessageEditModel model)
        {
            bool result = false;
            using (SqlConnection conn = GetConnection())
            {
                AccountModel account = GetAccountModel();
                model.UnitCode = Int32.Parse(account.UnitCode);

                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                MessageAction action = new MessageAction(conn, tran);
                model.UpdateAccount = GetAccount();

                bool flag_saveok = action.Insert(model);
                if (flag_saveok)
                {
                    tran.Commit();
                    if (model.SendMailMark)
                    {
                        ES.Utils.WebUtils.News_Mail_Send(model.MessageID.ToString());
                    }
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
            //ViewBag.tempMessage = TempData["tempMessage"];
            //MessageActionModel tmpModel = (MessageActionModel)TempData["MessageActionModel"];

            //if (tmpModel == null)
            //{
            //    TempData["tempMessage"] = "參數異常";
            //    return RedirectToAction("Index", "Message");
            //}

            MessageEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                MessageAction action = new MessageAction(conn);
                model = action.GetData(id);
                ViewBag.CategoryList = CodeUtils.GetCodeCheckBoxList(conn, "MSG_CATE", "", model.Category);
                ViewBag.MessageFileFolder = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + id + "\\";

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
            MessageActionModel model = new MessageActionModel();
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
                MessageAction action = new MessageAction(conn, tran);
                if (action.Delete(model))
                {
                    tran.Commit();
                    TempData["tempMessage"] = "刪除成功";
                    isDelete= true;
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
                return RedirectToAction("Index", "Message");
            }
            TempData["tempMessage"] = "刪除失敗";
            //return Json(new { status = false });
            return RedirectToAction("Index", "Message");
        }


        /// <summary>
        /// 修改-no use
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditM()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            MessageActionModel tmpModel = (MessageActionModel)TempData["MessageActionModel"];

            if (tmpModel == null)
            {
                TempData["tempMessage"] = "參數異常";
                return RedirectToAction("Index", "Message");
            }

            MessageEditModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                MessageAction action = new MessageAction(conn);
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
        public ActionResult Edit(MessageEditModel model)
        {
            ShareDAO dao = new ShareDAO();
            if (ModelState.IsValid)
            {
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    AccountModel account = GetAccountModel();
                    model.UnitCode = Int32.Parse(account.UnitCode);

                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    MessageAction action = new MessageAction(conn, tran);
                    model.UpdateAccount = GetAccount();

                    if (model.UpdateFile1 != null)
                    {
                        string dir = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + model.MessageID + "\\";
                        string filename = Path.GetFileName(model.UpdateFile1.FileName);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        model.UpdateFile1.SaveAs(dir + filename);
                        //var filename = dao.MSGPutFile(model.MessageID.ToString(), model.UpdateFile1,"1", "");
                        var FileList = filename.ToSplit('\\');
                        model.FileName1 = FileList[FileList.ToCount() - 1];
                    }
                    if (model.UpdateFile2 != null)
                    {
                        string dir = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + model.MessageID + "\\";
                        string filename = Path.GetFileName(model.UpdateFile2.FileName);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        model.UpdateFile2.SaveAs(dir + filename);
                        //var filename = dao.MSGPutFile(model.MessageID.ToString(), model.UpdateFile2, "2", "");
                        var FileList = filename.ToSplit('\\');
                        model.FileName2 = FileList[FileList.ToCount() - 1];
                    }
                    if (model.UpdateFile3 != null)
                    {
                        string dir = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + model.MessageID + "\\";
                        string filename = Path.GetFileName(model.UpdateFile3.FileName);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        model.UpdateFile3.SaveAs(dir + filename);
                        //var filename = dao.MSGPutFile(model.MessageID.ToString(), model.UpdateFile3, "3", "");
                        var FileList = filename.ToSplit('\\');
                        model.FileName3 = FileList[FileList.ToCount() - 1];
                    }

                    bool flag_saveok = action.Update(model);

                    if (flag_saveok)
                    {
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        if (model.SendMailMark)
                        {
                            ES.Utils.WebUtils.News_Mail_Send(model.MessageID.ToString());
                        }
                        isUpdate= true;
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
                    return RedirectToAction("Index", "Message");
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
                ViewBag.MessageFileFolder = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + model.MessageID.ToString() + "\\";

                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        public ActionResult Down(string id, string file)
        {
            string filePath = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE");
            filePath += id + "\\" + file;
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
            ES.Models.MessageShowModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ES.Action.MessageAction action = new ES.Action.MessageAction(conn);

                model = action.GetData(id);

                ViewBag.MessageFileFolder = ES.Utils.DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + id + "\\";
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }
    }
}
