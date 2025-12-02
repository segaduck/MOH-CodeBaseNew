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
using ES.DataLayers;
using ES.Services;

namespace ES.Areas.Admin.Controllers
{
    /// <summary>
    /// 書表下載管理
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ServiceFileController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 4;
        }

        /// <summary>
        /// 書表下載管理
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
                    ServiceFileAction action = new ServiceFileAction(conn);
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

            this.SetVisitRecord("ServiceFile", "Index", "書表下載管理");

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
                ServiceFileAction action = new ServiceFileAction(conn);

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
        /// 書表下載管理 - 案件列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceList(int id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceFileAction action = new ServiceFileAction(conn);
                ViewBag.List = action.GetServiceList(id);
                ViewBag.Dict = action.GetCategoryById(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 書表下載管理 - 列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult List(string id)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceFileAction action = new ServiceFileAction(conn);

                ViewBag.List = action.GetList(id);
                ViewBag.Dict = action.GetServiceName(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 書表下載管理 - 新增
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult New(string id)
        {
            ServiceFileNewModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceFileAction action = new ServiceFileAction(conn);
                model = action.GetNew(id);
                ViewBag.FileTypeList = CodeUtils.GetCodeSelectList(conn, "FILE_TYPE_CD", "", model.FileType, false);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 書表下載管理 - 新增 (送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult New(ServiceFileNewModel model)
        {
            ShareDAO dao = new ShareDAO();
            if (ModelState.IsValid)
            {
                //if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                //{
                //    string dir = DataUtils.GetConfig("FOLDER_SERVICE_FILE") + model.ServiceId + "\\";
                //    string filename = Path.GetFileName(Request.Files[0].FileName);
                //    if (!Directory.Exists(dir))
                //    {
                //        Directory.CreateDirectory(dir);
                //    }
                //    Request.Files[0].SaveAs(dir + filename);
                //    model.FileName = filename;
                //}
                var isInsert = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceFileAction action = new ServiceFileAction(conn, tran);
                    model.UpdateAccount = GetAccount();

                    if (model.UpdateFile != null)
                    {
                        string dir = DataUtils.GetConfig("FOLDER_SERVICE_FILE") + model.ServiceId + "\\";
                        string filename = Path.GetFileName(model.UpdateFile.FileName);
                        //var filename = dao.SFPutFile(model.ServiceId.ToString(), model.UpdateFile, model.Seq.ToString(), "");
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        model.UpdateFile.SaveAs(dir + filename);
                        var FileList = filename.ToSplit('\\');
                        model.FileName = FileList[FileList.ToCount() - 1];
                    }

                    if (action.Insert(model))
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "4";
                        log_model.TX_TYPE = 1;
                        log_model.TX_DESC = "案件名稱:" + log_action.getServiceName(model.ServiceId) + "<br>檔案名稱:" + model.FileName + "<br>檔案title:" + model.Title + "<br>順序:" + model.Seq;
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Commit();
                        TempData["tempMessage"] = "存檔成功";
                        isInsert = true;
                    }
                    else
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "4";
                        log_model.TX_TYPE = 1;
                        log_model.TX_DESC = "存檔失敗";
                        log_action.Insert(log_model, upd_Model);
                        //log end;
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }
                if (isInsert)
                {
                    return RedirectToAction("List", "ServiceFile", new { @id = model.ServiceId });
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

                ServiceFileAction action = new ServiceFileAction(conn);
                model = action.GetNew(model.ServiceId);
                ViewBag.FileTypeList = CodeUtils.GetCodeSelectList(conn, "FILE_TYPE_CD", "", model.FileType, false);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 書表下載管理 - 修改
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(string id, int fid)
        {
            ServiceFileEditModel model = new ServiceFileEditModel();
            model.ServiceId = id;
            model.FileId = fid;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceFileAction action = new ServiceFileAction(conn);
                model = action.GetEdit(model);
                ViewBag.FileTypeList = CodeUtils.GetCodeSelectList(conn, "FILE_TYPE_CD", "", model.FileType, false);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 書表下載管理 - 修改 (送出)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(ServiceFileEditModel model)
        {
            ShareDAO dao = new ShareDAO();
            if (ModelState.IsValid)
            {
                //if (Request.Files.Count > 0 && !String.IsNullOrEmpty(Request.Files[0].FileName))
                //{
                //    string dir = DataUtils.GetConfig("FOLDER_SERVICE_FILE") + model.ServiceId + "\\";
                //    string filename = Path.GetFileName(Request.Files[0].FileName);
                //    if (!Directory.Exists(dir))
                //    {
                //        Directory.CreateDirectory(dir);
                //    }
                //    Request.Files[0].SaveAs(dir + filename);
                //    model.FileName = filename;
                //}
                var isUpdate = false;
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    ServiceFileAction action = new ServiceFileAction(conn, tran);
                    model.UpdateAccount = GetAccount();

                    if (model.UpdateFile != null)
                    {
                        string dir = DataUtils.GetConfig("FOLDER_SERVICE_FILE") + model.ServiceId + "\\";
                        string filename = Path.GetFileName(model.UpdateFile.FileName);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        model.UpdateFile.SaveAs(dir + filename);
                        //var filename = dao.SFPutFile(model.ServiceId.ToString(), model.UpdateFile, model.Seq.ToString(), "");
                        var FileList = filename.ToSplit('\\');
                        model.FileName = FileList[FileList.ToCount() - 1];
                    }

                    if (action.Update(model))
                    {
                        //Log
                        AccountModel upd_Model = GetAccountModel();
                        UtilityAction log_action = new UtilityAction(conn, tran);
                        UtilityModel log_model = new UtilityModel();
                        log_model.TX_CATE_CD = "4";
                        log_model.TX_TYPE = 2;
                        log_model.TX_DESC = "案件名稱:" + log_action.getServiceName(model.ServiceId) + "<br>檔案名稱:" + model.FileName + "<br>檔案title:" + model.Title + "<br>順序:" + model.Seq;
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
                        log_model.TX_CATE_CD = "4";
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
                    return RedirectToAction("List", "ServiceFile", new { @id = model.ServiceId });
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

                ServiceFileAction action = new ServiceFileAction(conn);
                model = action.GetEdit(model);
                ViewBag.FileTypeList = CodeUtils.GetCodeSelectList(conn, "FILE_TYPE_CD", "", model.FileType, false);
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        /// <summary>
        /// 書表下載管理 - 刪除
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult Delete(string id, int fid)
        {
            ServiceFileEditModel model = new ServiceFileEditModel();
            model.ServiceId = id;
            model.FileId = fid;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                ServiceFileAction action = new ServiceFileAction(conn, tran);
                model.UpdateAccount = GetAccount();

                if (action.Delete(model))
                {
                    //Log
                    AccountModel upd_Model = GetAccountModel();
                    UtilityAction log_action = new UtilityAction(conn, tran);
                    UtilityModel log_model = new UtilityModel();
                    log_model.TX_CATE_CD = "4";
                    log_model.TX_TYPE = 3;
                    log_model.TX_DESC = "案件編號:" + model.ServiceId + "<br>檔案名稱:" + model.FileName + "<br>";
                    log_action.Insert(log_model, upd_Model);
                    //log end;
                    tran.Commit();
                    TempData["tempMessage"] = "刪除成功";
                }
                else
                {
                    //Log
                    AccountModel upd_Model = GetAccountModel();
                    UtilityAction log_action = new UtilityAction(conn, tran);
                    UtilityModel log_model = new UtilityModel();
                    log_model.TX_CATE_CD = "4";
                    log_model.TX_TYPE = 3;
                    log_model.TX_DESC = "刪除失敗";
                    log_action.Insert(log_model, upd_Model);
                    //log end;
                    tran.Rollback();
                    TempData["tempMessage"] = "刪除失敗";
                }
                conn.Close();
                conn.Dispose();
            }

            return RedirectToAction("List", "ServiceFile", new { @id = model.ServiceId });
        }

        /// <summary>
        /// 書表下載管理 - 檔案下載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult Download(string id, int fid)
        {
            ServiceFileEditModel model = new ServiceFileEditModel();
            model.ServiceId = id;
            model.FileId = fid;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ServiceFileAction action = new ServiceFileAction(conn);
                model = action.GetEdit(model);
                conn.Close();
                conn.Dispose();
            }

            //string s_log1 = "";
            //string filePath =  DataUtils.GetConfig("FOLDER_SERVICE_FILE") + id + "\\" + model.FileName;
            //s_log1 += string.Format("\n ##Download model.FileName: {0}", model.FileName);
            //s_log1 += string.Format("\n ##Download filePath: {0}", filePath);
            //logger.Debug(s_log1);
            //logger.Debug("filePath: " + filePath);
            //s_log1 = ""; //string s_log1 = "";
            //s_log1 += string.Format("\n ##Download fileName: {0}", fileName);
            //logger.Debug(s_log1);

            //string filePath = string.Format("{0},{1}\\{2}", DataUtils.GetConfig("FOLDER_SERVICE_FILE"), id, model.FileName);
            string filePath = string.Format("{0}{1}\\{2}", DataUtils.GetConfig("FOLDER_SERVICE_FILE"), id, model.FileName);//20210107 fix 下載檔案失敗問題
            string fileName = Path.GetFileName(filePath);

            //if (!System.IO.File.Exists(fileName))
            if (!System.IO.File.Exists(filePath))//20210107 fix 下載檔案失敗問題
            {
                TempData["tempMessage"] = "下載失敗，檔案不存在";
                //ViewBag.tempMessage = TempData["tempMessage"];
                return RedirectToAction("List", "ServiceFile", new { @id = model.ServiceId });
            }

            Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(iStream, "application/unknown", fileName);
        }

        /// <summary>
        /// 下載
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult Down(string id, string file)
        {
            //string filePath = (@"C:\e-service\File\ServiceFile\").Replace('\\', '/');
            String filePath = DataUtils.GetConfig("FOLDER_SERVICE_FILE").Replace('\\', '/'); //20210111 fix 修改下載附件發生找不到檔案的問題
            filePath += id + "/" + file;
            string fileName = Path.GetFileName(filePath);

            Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(iStream, "application/unknown", fileName);
        }
    }
}
