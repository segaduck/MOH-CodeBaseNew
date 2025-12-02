using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A3.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Omu.ValueInjecter;
using Turbo.Commons;
using OfficeOpenXml;

namespace EECOnline.Areas.A3.Controllers
{
    public class C103MController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Index(new C103MFormModel());
        }

        [HttpPost]
        public ActionResult Index(C103MFormModel model)
        {
            ModelState.Clear();
            ActionResult rtn = View("Index", model);
            A3DAO dao = new A3DAO();
            // 設定查詢分頁資訊
            dao.SetPageInfo(model.rid, model.p);
            // 查詢結果
            //model.Grid = dao.QueryC103M(model);
            // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
            if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows", model);
            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model, dao, "Index");
            return rtn;
        }

        [HttpPost]
        public ActionResult Export(C103MFormModel model)
        {
            return null;
        }
    }
}