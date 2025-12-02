using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RecordController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 8;
        }

        [HttpGet]
        public ActionResult SysInOutLog()
        {
            LoginRecordQueryModel model = new LoginRecordQueryModel();
            model.NowPage = 1;
            RecordAction action = new RecordAction();
            ViewBag.List = action.GetLOGIN_LOG(model);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            this.SetVisitRecord("Record", "SysInOutLog", "查詢登出入紀錄");

            return View(model);
        }

        [HttpPost]
        public ActionResult SysInOutLog(LoginRecordQueryModel model)
        {
            RecordAction action = new RecordAction();
            ViewBag.List = action.GetLOGIN_LOG(model);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }

        [HttpGet]
        public ActionResult SysInOutErrLog()
        {
            LoginRecordQueryModel model = new LoginRecordQueryModel();
            model.NowPage = 1;
            model.ErrTimes = 1;
            RecordAction action = new RecordAction();
            ViewBag.List = action.GetLOGIN_LOGERR(model);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            this.SetVisitRecord("Record", "SysInOutErrLog", "查詢異常登入紀錄");

            return View(model);
        }

        [HttpPost]
        public ActionResult SysInOutErrLog(LoginRecordQueryModel model)
        {
            RecordAction action = new RecordAction();
            ViewBag.List = action.GetLOGIN_LOGERR(model);

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }

        [HttpGet]
        public ActionResult ModifyLog()
        {
            ModifyLogModel model = new ModifyLogModel();
            model.NowPage = 1;
            model.TIME_S = DateTime.Now.ToString("yyyy/MM/dd");
            model.TIME_F = DateTime.Now.ToString("yyyy/MM/dd");
            ViewBag.NowPage = 1;
            ViewBag.TotalCount = 0;
            ViewBag.TotalPage = 0;
            return View(model);
        }

        [HttpPost]
        public ActionResult ModifyLog(ModifyLogModel model)
        {
            RecordAction action = new RecordAction();
            ViewBag.List = action.ModifySearchQuery(model);
            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();
            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);
            return View(model);
        }

        // GET: /Admin/Record/
        [HttpGet]
        public ActionResult MailLog()
        {
            MailLogModel model = new MailLogModel();
            model.NowPage = 1;
            model.TIME_S = DateTime.Now.ToString("yyyy/MM/dd");
            model.TIME_F = DateTime.Now.ToString("yyyy/MM/dd");
            model.ORDER_FIELD = "send_time";
            model.ORDER_BY = "desc";
            ViewBag.NowPage = 1;
            ViewBag.TotalCount = 0;
            ViewBag.TotalPage = 0;

            this.SetVisitRecord("Record", "MailLog", "信件查詢");

            return View(model);
        }

        [HttpPost]
        public ActionResult MailLog(MailLogModel model)
        {
            RecordAction action = new RecordAction();
            if (!String.IsNullOrEmpty(model.MAIL))
                model.MAIL = model.MAIL.Trim();
            ViewBag.List = action.MailSearchQuery(model);
            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();
            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);
            return View(model);
        }
    }
}
