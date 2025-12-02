using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Utils;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using ES.Areas.Admin.Action;
using WebUI.CustomClass;

namespace ES.Areas.Admin.Controllers
{
    public class ExportController : BaseController
    {
        [HttpGet]
        public ActionResult Case1()
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ViewBag.List = CodeUtils.GetCase1List(conn, null, true);
                conn.Close();
                conn.Dispose();
            }

            this.SetVisitRecord("Export", "Case1", "社救司案件匯出");

            return View(new ExportCase1Models());
        }

        [HttpPost]
        public ActionResult Case1(ExportCase1Models model)
        {
            DataTable dt = null;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                ExportAction action = new ExportAction(conn);
                dt = action.GetCase1(model);
                conn.Close();
                conn.Dispose();
            }

            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt);
            return File(ms, "application/unknown", "Download.xls");
        }
    }
}
