using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A2.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Turbo.Commons;

namespace EECOnline.Areas.A2.Controllers
{
    public class C101MController : BaseController
    {
        public ActionResult Index(C101MFormModel Form)
        {
            SessionModel sm = SessionModel.Get();
            A2DAO dao = new A2DAO();
            ActionResult rtn = View(Form);
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(Form.rid, Form.p);
                // 醫院登入時，只能看得到自己的資料
                Form.HospCode = null;
                if (sm.UserInfo.LoginTab == "2" && sm.UserInfo.HospitalCode.TONotNullString() != "")
                {
                    Form.HospCode = sm.UserInfo.HospitalCode;
                }
                // 查詢結果
                Form.Grid = dao.QueryC101M(Form);
                // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
                if (!string.IsNullOrEmpty(Form.rid) && Form.useCache == 0) rtn = PartialView("_GridRows", Form);
                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(Form, dao, "Index");
            }
            return rtn;
        }

        public ActionResult Detail(string apply_no_sub)
        {
            A2DAO dao = new A2DAO();
            C101MDetailModel model = dao.DetailC101M(apply_no_sub);
            if (model == null) return new HttpNotFoundResult();
            model.DetailGrid = dao.DetailC101MGrid(apply_no_sub);
            return View("Detail", model);
        }
    }
}