using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A6.Models;
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

namespace EECOnline.Areas.A6.Controllers
{
    public class C104MController : BaseController
    {
        public ActionResult Index(C104MFormModel form)
        {
            ModelState.Clear();
            A6DAO dao = new A6DAO();
            form.Grid = dao.QueryC104M(form);
            return View(form);
        }

        [HttpGet]
        public ActionResult New()
        {
            C104MDetailModel model = new C104MDetailModel();
            model.IsNew = true;
            return View("Detail", model);
        }

        [HttpGet]
        public ActionResult Modify(string keyid)
        {
            A6DAO dao = new A6DAO();
            SessionModel sm = SessionModel.Get();
            C104MDetailModel model = new C104MDetailModel();
            model = dao.QueryC104MDetail(keyid);
            if (model == null)
            {
                sm.LastErrorMessage = "找不到指定的資料!";
                model = new C104MDetailModel();
            }
            model.IsNew = false;
            return View("Detail", model);
        }

        [HttpPost]
        public ActionResult Save(C104MDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            A6DAO dao = new A6DAO();
            ActionResult rtn = View("Detail", detail);

            var tryInt = 0;
            if (!int.TryParse(detail.type_price, out tryInt)) ModelState.AddModelError("", "請輸入正確的金額！");

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                TblEEC_CommonType where = new TblEEC_CommonType();
                where.keyid = detail.keyid;

                TblEEC_CommonType ins_upd = new TblEEC_CommonType();
                ins_upd.InjectFrom(detail);

                if (detail.IsNew)
                    dao.Insert(ins_upd);
                else
                    dao.Update(ins_upd, where);

                sm.LastResultMessage = "已儲存";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "C104M", new { area = "A6", useCache = "2" });
            }
            return rtn;
        }

        [HttpPost]
        public ActionResult Delete(C104MDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            A6DAO dao = new A6DAO();
            ActionResult rtn = View("Detail", detail);

            var tmpObj = dao.GetRow(new TblEEC_CommonType() { keyid = detail.keyid });
            if (tmpObj == null || detail.keyid == null || detail.keyid.TOInt32() <= 0) ModelState.AddModelError("", "錯誤！查無資料！");

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                var res = dao.Delete(new TblEEC_CommonType() { keyid = detail.keyid });

                sm.LastResultMessage = "已刪除";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "C104M", new { area = "A6", useCache = "2" });
            }
            return rtn;
        }
    }
}