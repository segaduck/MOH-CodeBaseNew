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
    public class C104MController : BaseController
    {
        ///// <summary>
        /// 連結首頁
        /// </summary>
        /// <returns></returns>
        /// [HttpGet]
        /// public ActionResult Index()
        ///  {
        ///     A7DAO dao = new A7DAO();
        ///     C104MFormModel form = new C104MFormModel();
        ///     ActionResult rtn = View(form);

        ///      return View(form);
        /// }

        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Index(C104MFormModel form)
        {
            A7DAO dao = new A7DAO();
            ActionResult rtn = View(form);

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                // 查詢結果
                form.Grid = dao.QueryC104MGrid(form);

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
        /// 新增常見問題
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C104MDetailModel model = new C104MDetailModel();
            model.status = "1";

            return View("Detail", model);
        }

        /// <summary>
        /// 編輯常見問題
        /// </summary>
        /// <param name="Modify"></param>
        /// <returns></returns>
        public ActionResult Modify(string efaq_id)
        {
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C104MDetailModel model = new C104MDetailModel();

            model.efaq_id = efaq_id;
            model = dao.GetRow(model);
            

            if (model == null)
            { sm.LastErrorMessage = "找不到指定的資料!"; model = new C104MDetailModel(); }
            model.IsNew = false;
            model.code_name = model.faqtype;
            
            return View("Detail", model);
        }


        /// <summary>
        /// 儲存常見問題
        /// </summary>
        /// <param name="Save"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(C104MDetailModel detail)
        {
            // 由登入資訊取得當前角色的檢定類別資訊
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            ActionResult rtn = View("Detail", detail);

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                
                // 檢核

                string ErrorMsg = dao.CheckC104M(detail);
                if (ErrorMsg == "")
                {
                    if (detail.IsNew)
                    {
                        dao.AppendC104MDetail(detail);
                        sm.LastResultMessage = "問題新增成功";
                    }
                    else
                    {
                        dao.UpdateC104MDetail(detail);
                        sm.LastResultMessage = "問題更新成功";
                    }
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C104M", new { area = "A7", useCache = "2" });
                }
                else
                {
                    sm.LastErrorMessage = ErrorMsg;
                }
            }
            return rtn;
        }

        /// <summary>
        /// 刪除常見問題
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(C104MDetailModel model)
        {

            if (string.IsNullOrEmpty(model.efaq_id.TONotNullString()))
            {
                throw new ArgumentNullException("Detail.model.efaq_id");
            }
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            dao.DeleteC104MDetail(model);

            sm.LastResultMessage = "該問題已刪除";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C104M", new { area = "A7", useCache = "2" });

            return View("Detail", model);
        }
    }
}