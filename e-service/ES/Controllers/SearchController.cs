using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Models;
using ES.Action;

namespace ES.Controllers
{
    public class SearchController : BaseController
    {
        /// <summary>
        /// 案件搜尋
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            
            return View();
        }

        /// <summary>
        /// 案件搜尋
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(SearchModel model)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SearchAction action = new SearchAction(conn);
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
        /// 案件搜尋
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchAction()
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            SearchModel model = new SearchModel();
            model.NowPage = 1;

            return View(model);
        }
    }
}
