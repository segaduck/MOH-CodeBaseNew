using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Utils;
using System.IO;
using log4net;
using System.Data.SqlClient;
using ES.Action;
using ES.Models;
using ES.DataLayers;
using ES.Services;

namespace ES.Controllers
{
    /// <summary>
    /// BaseController 下載檔案免登入
    /// </summary>
    public class FileController : BaseNoMemberController
    {
        /// <summary>
        /// 案件申請
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult Apply(string path)
        {
            //logger.Debug("path: " + path);
            string message = "";
            path = DataUtils.FromBase64String(path);
            if (path == null)
            {
                message = "參數異常";
                MessageBoxModel msg = new MessageBoxModel(message, -1);
                TempData["MessageBoxModel"] = msg;
                return RedirectToAction("Index", "MessageBox");
            }

            try
            {
                if (path.IndexOf("\\") < 0 && path.IndexOf("/") < 0)
                {
                    return Redirect("http://203.65.100.178/admin/xml/" + path);
                    //return Redirect("http://203.65.100.178/BACKMIN/xml/" + path);
                }
                string filePath = DataUtils.GetConfig("FOLDER_APPLY_FILE") + path;
                string fileName = Path.GetFileName(filePath);

                Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return File(iStream, "application/unknown", HttpUtility.UrlEncode(fileName));
            }
            catch (DirectoryNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (FileNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (Exception e)
            {
                message = "下載失敗。";
                logger.Warn(e.Message, e);
            }

            if (message != "")
            {
                MessageBoxModel msg = new MessageBoxModel(message, -1);
                TempData["MessageBoxModel"] = msg;
            }
            return RedirectToAction("Index", "MessageBox");
        }

        /// <summary>
        /// 服務書表下載
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceFile(string id)
        {
            SessionModel sm = SessionModel.Get();

            string message = "";
            try
            {
                string folder = DataUtils.GetConfig("FOLDER_SERVICE_FILE");
                string fid = Request["fid"];

                string file = DataUtils.FromBase64String(Request["file"]);
                string url = DataUtils.FromBase64String(Request["url"]);
                if (file == null || url == null)
                {
                    message = "參數異常";
                    sm.LastErrorMessage = message;
                    return RedirectToAction("File", "Service", new { @id = id });
                }

                if (String.IsNullOrEmpty(file) && String.IsNullOrEmpty(url))
                {
                    message = "參數異常";
                    sm.LastErrorMessage = message;
                    return RedirectToAction("File", "Service", new { @id = id });
                }

                if (!String.IsNullOrEmpty(file) && (file.IndexOf("../") >= 0 || file.IndexOf("..\\") >= 0 || Path.GetExtension(file).ToLower().Equals(".ini")))
                {
                    message = "參數異常";
                    sm.LastErrorMessage = message;
                    return RedirectToAction("File", "Service", new { @id = id });
                }

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    ServiceAction action = new ServiceAction(conn, tran);

                    if (action.UpdateFileCount(id, Int32.Parse(fid)))
                    {
                        //logger.Debug("true");
                        tran.Commit();
                    }
                    else
                    {
                        //logger.Debug("false");
                        tran.Rollback();
                    }
                    conn.Close();
                    conn.Dispose();
                }

                //if (string.IsNullOrEmpty(file) && string.IsNullOrEmpty(url)) { return Redirect(url); }

                if (string.IsNullOrEmpty(file) && !string.IsNullOrEmpty(url)) { return Redirect(url); }

                if (!String.IsNullOrEmpty(file))
                {
                    //string s_log1 = "";
                    string filePath = folder + id + "\\" + file;
                    //s_log1 = string.Format("\n ##ServiceFile filePath: {0}", filePath);
                    //logger.Debug(s_log1);
                    string fileName = Path.GetFileName(filePath);
                    //s_log1 = string.Format("\n ##ServiceFile fileName: {0}", fileName);
                    //logger.Debug(s_log1);

                    if (!System.IO.File.Exists(filePath))
                    {
                        message = "下載失敗，檔案不存在";
                        sm.LastErrorMessage = message;
                        return RedirectToAction("File", "Service", new { @id = id });
                    }

                    Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    return File(iStream, "application/unknown", HttpUtility.UrlEncode(fileName));
                }

            }
            catch (DirectoryNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (FileNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (Exception e)
            {
                message = "下載失敗。";
                logger.Warn(e.Message, e);
            }

            TempData["tempMessage"] = message;
            sm.LastErrorMessage = message;
            return RedirectToAction("File", "Service", new { id = id });
        }

        /// <summary>
        /// 服務操作輔助說明下載
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceHelp(string id)
        {
            string message = "";
            try
            {
                string folder = DataUtils.GetConfig("FOLDER_SERVICE_HELP");
                string file = DataUtils.FromBase64String(Request["file"]);
                if (file == null)
                {
                    message = "參數異常";
                    TempData["tempMessage"] = message;
                    return RedirectToAction("Help", "Service", new { id = id });
                }
                if (file.IndexOf("../") >= 0 || file.IndexOf("..\\") >= 0 || Path.GetExtension(file).ToLower().Equals(".ini"))
                {
                    message = "參數異常";
                }
                else
                {
                    string filePath = folder + id + "\\" + file;
                    string fileName = Path.GetFileName(filePath);

                    Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    return File(iStream, "application/unknown", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (DirectoryNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (FileNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (Exception e)
            {
                message = "下載失敗。";
                logger.Warn(e.Message, e);
            }

            TempData["tempMessage"] = message;
            return RedirectToAction("Help", "Service", new { id = id });
        }

        /// <summary>
        /// 服務相關規範下載
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ServiceNorm(string id)
        {
            string message = "";
            try
            {
                string folder = DataUtils.GetConfig("FOLDER_SERVICE_NORM");
                string file = DataUtils.FromBase64String(Request["file"]);
                if (file == null)
                {
                    message = "參數異常";
                    TempData["tempMessage"] = message;
                    return RedirectToAction("Norm", "Service", new { id = id });
                }

                if (file.IndexOf("../") >= 0 || file.IndexOf("..\\") >= 0 || Path.GetExtension(file).ToLower().Equals(".ini"))
                {
                    message = "參數異常";
                }
                else
                {
                    string filePath = folder + id + "\\" + file;
                    string fileName = Path.GetFileName(filePath);
                    Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    return File(iStream, "application/unknown", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (DirectoryNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (FileNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (Exception e)
            {
                message = "下載失敗。";
                logger.Warn(e.Message, e);
            }

            TempData["tempMessage"] = message;
            return RedirectToAction("Norm", "Service", new { id = id });
        }

        /// <summary>
        /// 最新消息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Message(string id)
        {
            string message = "";
            try
            {
                string filePath = DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + Request.Params["file"].ToString();
                string file = DataUtils.FromBase64String(Request["file"]);

                if (file.IndexOf("../") >= 0 || file.IndexOf("..\\") >= 0 || Path.GetExtension(file).ToLower().Equals(".ini"))
                {
                    message = "參數異常";
                }
                else
                {
                    string fileName = Path.GetFileName(filePath);
                    Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    return File(iStream, "application/unknown", HttpUtility.UrlEncode(fileName));
                }
            }
            catch (DirectoryNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (FileNotFoundException e)
            {
                message = "下載失敗，檔案不存在。";
                logger.Warn(e.Message, e);
            }
            catch (Exception e)
            {
                message = "下載失敗。";
                logger.Warn(e.Message, e);
            }

            TempData["tempMessage"] = message;
            return RedirectToAction("Index", "Message", new { id = id });
        }

        public ActionResult DownloadFileFromPath(string filepath, string downloadType)
        {
            if (string.IsNullOrEmpty(filepath)) { return HttpNotFound(); }

            ActionResult result = HttpNotFound();
            ShareDAO dao = new ShareDAO();
            byte[] ba = null;
            string path = dao.GetServerLocalPath();
            string filename = null;
            string contentType = null;

            byte[] pathba = System.Convert.FromBase64String(filepath);
            string realpath = System.Text.Encoding.Default.GetString(pathba);

            try
            {
                string fullpath = (path + realpath).Replace("\\\\", "\\");
                using (FileStream fs = System.IO.File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    ba = new byte[fs.Length];
                    bs.Read(ba, 0, (int)fs.Length);
                }

                filename = Path.GetFileName(fullpath);
                var fileExt = Path.GetExtension(filename).ToLower();

                switch (fileExt)
                {
                    case ".pdf":
                        contentType = "application/pdf";
                        break;
                    case ".jpg":
                    case ".jpeg":
                        contentType = "image/jpeg";
                        break;
                    case ".bmp":
                        contentType = "image/bmp";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    default:
                        contentType = "application/octet-stream";
                        break;
                }

                switch (downloadType)
                {
                    case "inline":
                        this.Response.Headers["Content-Disposition"] = string.Format("inline; filename={0}", filename);
                        break;
                    default:
                        this.Response.Headers["Content-Disposition"] = string.Format("attachment; filename={0}", filename);
                        break;
                }

                result = File(ba, contentType);

            }
            catch (Exception ex)
            {
                logger.Error("DownloadFileFromPath failed:" + ex.TONotNullString());
                result = HttpNotFound();
            }

            return result;
        }

    }
}
