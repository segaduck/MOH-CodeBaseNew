using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using System.Web.Routing;
using ES.Utils;
using System.Data.SqlClient;
using ES.Services;
using ES.Models;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AssignController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 5;

        }

        /// <summary>
        /// 分文處理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            AssignModel model = new AssignModel();
            model.NowPage = 1;
            model.IS_HOME_PAGE = "N";
            AssignAction action = new AssignAction();
            ViewBag.List = action.GetAPPLY(model, GetAccountModel());
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ViewBag.CaseTypeList = action.GetCaseTypeList(GetAccountModel(), conn);
                conn.Close();
                conn.Dispose();
            }

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            this.SetVisitRecord("Assign", "Index", "分文處理");


            return View(model);
        }

        /// <summary>
        /// 分文處理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CaseSearch(string service_id, string flow_cd)
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            AssignModel model = new AssignModel();
            model.NowPage = 1;
            model.SRV_ID = service_id;
            model.CaseType = service_id;
            model.FLOW_CD = flow_cd;
            model.IS_HOME_PAGE = "Y";

            AssignAction action = new AssignAction();
            ViewBag.List = action.SearchAPPLY(model, GetAccountModel());
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ViewBag.CaseTypeList = action.GetCaseTypeList(GetAccountModel(), conn);
                conn.Close();
                conn.Dispose();
            }

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View("Index", model);
        }

        [HttpPost]
        public ActionResult Index(AssignModel model)
        {
            AssignAction action = new AssignAction();
            ViewBag.List = action.GetAPPLY(model, GetAccountModel());
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ViewBag.CaseTypeList = action.GetCaseTypeList(GetAccountModel(), conn);
                conn.Close();
                conn.Dispose();
            }

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }

        [HttpPost]
        public JsonResult GetAdminList(string serviceId)
        {
            List<Dictionary<string, object>> list = null;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                AssignAction action = new AssignAction(conn);
                list = action.GetAdminList(serviceId);
                conn.Close();
                conn.Dispose();
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BatchAssign()
        {
            Dictionary<string, object> item = new Dictionary<string, object>();

            try
            {
                string serviceId = Request["caseType"];
                string[] admin = Request["caseAccount"].Split('/');

                Dictionary<string, object> args = new Dictionary<string, object>();
                args.Add("UPD_ACC", GetAccount()); // 分文者帳號
                args.Add("PRO_ACC", admin[0]);
                args.Add("PRO_UNIT_CD", Int32.Parse(admin[1]));
                args.Add("UPD_FUN_CD", "ADM-ASSIGN");
                args.Add("SRV_ID", serviceId);

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();

                    AssignAction action = new AssignAction(conn, tran);

                    List<Dictionary<string, object>> list = action.GetBatchList(args);

                    action.BatchUpdateApply(args);
                    tran.Commit();
                    try
                    {
                        foreach (Dictionary<string, object> item2 in list)
                        {
                            WebUtils.Dispatch_Mail_Send(Convert.ToString(item2["APP_ID"]));
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.Message, e);
                    }

                    item.Add("status", true);
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception e)
            {
                item.Add("status", false);
                logger.Warn(e.Message, e);
            }

            return Json(item, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(string id, string srv_id, string is_home_page)
        {
            AssignAction action = new AssignAction();
            AssignEditModel model = action.GetAPPLY(id);
            model.SRV_ID = srv_id;
            model.IS_HOME_PAGE = is_home_page;
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(AssignEditModel model)
        {
            AssignAction action = new AssignAction();
            if (action.UpdateAPPLY(model, this.GetAccount()))
            {
                TempData["tempMessage"] = "分文完成!";
                WebUtils.Dispatch_Mail_Send(model.APP_ID);
            }
            else
            {
                TempData["tempMessage"] = "分文失敗!";
            }

            if (model.IS_HOME_PAGE == "Y")
            {
                return CaseSearch(model.SRV_ID, model.FLOW_CD);
            }
            else
            {
                return RedirectToAction("Index", "Assign");
            }

        }
    }
}
