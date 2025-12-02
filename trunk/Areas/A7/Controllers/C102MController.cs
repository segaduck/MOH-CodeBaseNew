using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A7.Models;
using EECOnline.Commons;
using EECOnline.Controllers;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Models.Entities;
using EECOnline.Services;
using log4net;
using Turbo.Commons;

namespace EECOnline.Areas.A7.Controllers
{
    public class C102MController : BaseController
    {
        ///// <summary>
        /// 連結首頁
        /// </summary>
        /// <returns></returns>
        /// [HttpGet]
        /// public ActionResult Index()
        ///  {
        ///     A7DAO dao = new A7DAO();
        ///     C102MFormModel form = new C102MFormModel();
        ///     ActionResult rtn = View(form);

        ///      return View(form);
        /// }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Index(C102MFormModel form)
        {
            A7DAO dao = new A7DAO();
            ActionResult rtn = View(form);

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

        /// <summary>
        /// 新增連結
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C102MDetailModel model = new C102MDetailModel();
            // 設定Upload參數
            model.SetUploadParm();

            return View("Detail", model);
        }

        /// <summary>
        /// 編輯連結
        /// </summary>
        /// <param name="elinks"></param>
        /// <returns></returns>
        public ActionResult Modify(int elinks_id)
        {
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C102MDetailModel model = new C102MDetailModel();
            model.elinks_id = elinks_id.TONotNullString();
            model = dao.GetRow(model);
            if (model == null)
            { sm.LastErrorMessage = "找不到指定的資料!"; model = new C102MDetailModel(); }
            model.IsNew = false;

            // 設定Upload參數
            model.SetUploadParm();

            return View("Detail", model);
        }

        [HttpPost]
        public ActionResult Save(C102MDetailModel detail)
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            ActionResult rtn = View("Detail", detail);
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                
                // 檢核

                string ErrorMsg = dao.CheckC102M(detail);
                if (ErrorMsg == "")
                {
                    if (detail.IsNew)
                    {
                        dao.AppendC102MDetail(detail);
                        sm.LastResultMessage = "連結新增成功";
                    }
                    else
                    {
                        dao.UpdateC102MDetail(detail);
                        sm.LastResultMessage = "連結更新成功";
                    }
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "A7", useCache = "2" });
                }
                else
                {
                    sm.LastErrorMessage = ErrorMsg;
                }
            }
            return rtn;
        }

        /// <summary>
        /// 刪除連結
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(C102MDetailModel model)
        {
            if (string.IsNullOrEmpty(model.elinks_id.TONotNullString()))
            {
                throw new ArgumentNullException("Detail.model.elinks_id");
            }
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            dao.DeleteC102MDetail(model);

            sm.LastResultMessage = "該連結已刪除";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C102M", new { area = "A7", useCache = "2" });

            return View("Detail", model);
        }
    }
}