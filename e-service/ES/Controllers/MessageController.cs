using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models;
using log4net;
using System.Data.SqlClient;
using ES.Action;
using System.Web.Configuration;
using System.IO;
using ES.Utils;

namespace ES.Controllers
{
    public class MessageController : BaseNoMemberController
    {
        /// <summary>
        /// 歷史消息查詢
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            MessageActionModel model = new MessageActionModel();
            model.NowPage = 1;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SetSearchCode(conn, 6);

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
            model.MESSAGE_TYPE = "";

            return View(model);
        }

        /// <summary>
        /// 歷史消息查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(MessageActionModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (ModelState.IsValid)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    SetSearchCode(conn, 6);

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

            return View(model);
        }

        /// <summary>
        /// 顯示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Show(int id)
        {
            MessageShowModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                MessageAction action = new MessageAction(conn);

                model = action.GetData(id);

                ViewBag.MessageFileFolder = DataUtils.GetConfig("FOLDER_MESSAGE_FILE");
                conn.Close();
                conn.Dispose();
            }

            return View(model);
        }

        //public ActionResult Down(string id)
        //{
        //    string filePath = DataUtils.GetConfig("FOLDER_MESSAGE_FILE") + Request.Params["file"].ToString();
        //    string fileName = Path.GetFileName(filePath);

        //    if (filePath.IndexOf("../") >= 0 || Path.GetExtension(fileName).ToLower().Equals(".ini"))
        //    {
        //        MessageBoxModel msg = new MessageBoxModel("Index", "Message", "參數異常");
        //        TempData["MessageBoxModel"] = msg;
        //        return RedirectToAction("Index", "MessageBox");
        //    }

        //    Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        //    return File(iStream, "application/unknown", fileName);
        //}

        public ActionResult Down(string id, string file)
        {
            string filePath = (@"C:\e-service\File\Message\").Replace('\\', '/');
            filePath += id + "/" +  file;
            string fileName = Path.GetFileName(filePath);

            Stream iStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(iStream, "application/unknown", fileName);
        }

    }
}
