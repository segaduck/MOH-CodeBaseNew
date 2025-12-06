using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A4.Models;
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

namespace EECOnline.Areas.A4.Controllers
{
    public class C102MController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string Idx = "1")
        {
            return (Idx == "2") ?
                Index2(new C102MForm2Model()) :
                Index1(new C102MForm1Model());
        }

        [HttpPost]
        public ActionResult Index1(C102MForm1Model model)
        {
            ModelState.Clear();
            ActionResult rtn = View("Index1", model);
            A4DAO dao = new A4DAO();
            // 設定查詢分頁資訊
            dao.SetPageInfo(model.rid, model.p);
            // 查詢結果
            //model.Form1Grid = dao.QueryC102M_1(model);
            // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
            if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows1", model);
            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model, dao, "Index1");
            return rtn;
        }

        [HttpPost]
        public ActionResult Index2(C102MForm2Model model)
        {
            ModelState.Clear();
            ActionResult rtn = View("Index2", model);
            A4DAO dao = new A4DAO();
            // 設定查詢分頁資訊
            dao.SetPageInfo(model.rid, model.p);
            // 查詢結果
            //model.Form2Grid = dao.QueryC102M_2(model);
            // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
            if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows2", model);
            // 設定分頁元件(_PagingLink partial view)所需的資訊
            base.SetPagingParams(model, dao, "Index2");
            return rtn;
        }

        [HttpPost]
        public ActionResult Export1(C102MForm1Model model)
        {
            return null;
        }

        [HttpPost]
        public ActionResult Export2(C102MForm2Model model)
        {
            return null;
        }
    }
}