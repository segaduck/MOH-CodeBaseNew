using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;
using ES.Utils;
using System.Data.SqlClient;
using ES.Areas.Admin.Action;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApplyTempController : BaseController
    {
        
        public ActionResult Index()
        {
            int nowPage = 1;

            if (!String.IsNullOrEmpty(Request["NowPage"]))
            {
                nowPage = Convert.ToInt32(Request["NowPage"]);
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ApplyTempAction action = new ApplyTempAction(conn);
                ViewBag.List = action.GetList(nowPage);

                double pageSize = action.GetPageSize();
                double totalCount = action.GetTotalCount();

                ViewBag.NowPage = nowPage;
                ViewBag.TotalCount = action.GetTotalCount();
                ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

                conn.Close();
                conn.Dispose();
            }

            return View();
        }

        public ActionResult Show(string id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ApplyTempAction action = new ApplyTempAction(conn);
                ViewBag.Item = action.GetTempData(id);
                conn.Close();
                conn.Dispose();
            }

            return View();
        }
    }
}
