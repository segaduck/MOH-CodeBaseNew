using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ES.Action;

namespace ES.Controllers
{
    public class SiteController : BaseNoMemberController
    {
        /// <summary>
        /// 相關連結
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                SetSearchCode(conn, 5);

                SiteAction action = new SiteAction(conn);
                ViewBag.List = action.GetList();
                conn.Close();
                conn.Dispose();
            }

            return View();
        }

    }
}
