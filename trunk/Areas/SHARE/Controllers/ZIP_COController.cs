using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.SHARE.Models;
using EECOnline.Models;
using EECOnline.DataLayers;
using Turbo.Commons;
using EECOnline.Controllers;

namespace EECOnline.Areas.SHARE.Controllers
{
    public class ZIP_COController : EBaseController
    {
        // GET: SHARE/ZIP_CO
        [HttpGet]
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ZIP_COFormModel form = new ZIP_COFormModel();       
            return Index(form);
        }

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(ZIP_COFormModel form)
        {
            SessionModel sm = SessionModel.Get();
            SHAREDAO dao = new SHAREDAO();

            ActionResult rtn = View(form);

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 查詢結果
                form.Grid = dao.QueryZIP_CO(form);

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