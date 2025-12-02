using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Action;
using ES.Models;
using System.Web.Configuration;
using System.IO;
using log4net;
using ES.Utils;

namespace ES.Controllers
{
    public class ServiceController : BaseNoMemberController
    {
        /// <summary>
        /// 服務項目
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SetSearchCode(conn, 3);

                ServiceAction action = new ServiceAction(conn);
                //列表-表件下載
                var vList = action.GetList();
                // 生殖細胞
                var Item001038 = vList.Where(x => x.ServiceId == "001038").FirstOrDefault();
                if(Item001038 != null && !string.IsNullOrEmpty(Item001038.ServiceId))
                {
                    var new001038 = Item001038;
                    new001038.UnitName = "國民健康署";
                    vList.Remove(Item001038);
                    vList.Add(new001038);
                }
                ViewBag.List = vList;
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        /// <summary>
        /// 申請須知
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Notice(string id)
        {
            ServiceNoticeModel model = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);
                model = action.GetNotice(id);
            }

            return View(model);
        }

        /// <summary>
        /// 書表下載
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult File(string id)
        {
            string s_log1 = "";
            s_log1 += string.Format(" ##File(string id):{0}", id);
            logger.Debug(s_log1);

            ViewBag.tempMessage = TempData["tempMessage"];

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);
                //List<ServiceFileModel>
                ViewBag.List = action.GetFileList(id);
            }

            return View("File");
        }

        /// <summary>
        /// 操作輔助說明
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Help(string id)
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);
                ViewBag.List = action.GetHelpList(id);
            }

            return View();
        }

        /// <summary>
        /// 相關規範
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Norm(string id)
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);
                ViewBag.List = action.GetNormList(id);
            }

            return View();
        }
    }
}
