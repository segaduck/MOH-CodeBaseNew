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
using Turbo.Commons;

namespace EECOnline.Areas.A6.Controllers
{
    public class C102MController : BaseController
    {
        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Index(C102MFormModel form)
        {
            A6DAO dao = new A6DAO();
            ActionResult rtn = View(form);
            SessionModel sm = SessionModel.Get();

            if (form.apy_time_st.TONotNullString() != "" && form.apy_time_ed.TONotNullString() != "")
            {
                if (HelperUtil.TransToDateTime(form.apy_time_st, "/") > ((DateTime)HelperUtil.TransToDateTime(form.apy_time_ed, "/")).AddDays(-1))
                {
                    ModelState.AddModelError("date", "操作日期起不得大於操作日期迄 。");
                }

            }


            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                // 查詢結果
                form.Grid = dao.QueryC102MGrid(form);

                // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0)
                {
                    rtn = PartialView("_GridRows", form);
                }
                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }


            return rtn;
        }


        
    }
}