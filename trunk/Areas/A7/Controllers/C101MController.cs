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
    public class C101MController : BaseController
    {
        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Index(C101MFormModel form)
        {
            A7DAO dao = new A7DAO();
            ActionResult rtn = View(form);
            
            if (HelperUtil.TransToDateTime(form.showdates, "") > HelperUtil.TransToDateTime(form.showdatee, ""))
            {
                ModelState.AddModelError("date", "上架日期起不得大於上架日期迄 。");
            }

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);

                // 查詢結果
                form.Grid = dao.QueryC101MGrid(form);

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
        /// 新增公告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C101MDetailModel model = new C101MDetailModel();
            // 設定Upload參數
            model.SetUploadParm();

            return View("Detail", model);
        }

        /// <summary>
        /// 編輯單位
        /// </summary>
        /// <param name="enews_id"></param>
        /// <returns></returns>
        public ActionResult Modify(string enews_id)
        {
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C101MDetailModel model = new C101MDetailModel();

            model.enews_id = enews_id;
            model = dao.GetRow(model);

            if (model == null)
            { sm.LastErrorMessage = "找不到指定的資料!"; model = new C101MDetailModel(); }
            model.IsNew = false;

            model.enews = model.newstype;
            // 設定Upload參數
            model.SetUploadParm();


            return View("Detail", model);
        }

        [HttpPost]
        public ActionResult Save(C101MDetailModel detail)
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            ActionResult rtn = View("Detail", detail);
            C101MDetailModel model = new C101MDetailModel();

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                // 檢核

                string ErrorMsg = dao.CheckC101M(detail);
                if (ErrorMsg == "")
                {
                    if (detail.IsNew)
                    {
                        dao.AppendC101MDetail(detail);
                        sm.LastResultMessage = "公告新增成功";
                    }
                    else
                    {
                        dao.UpdateC101MDetail(detail);
                        sm.LastResultMessage = "公告更新成功";
                    }
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A7", useCache = "2" });
                }
                else
                {
                    sm.LastErrorMessage = ErrorMsg;
                }
            }
            return rtn;
        }

        /// <summary>
        /// 刪除案件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(C101MDetailModel model)
        {

            if (string.IsNullOrEmpty(model.enews_id.TONotNullString()))
            {
                throw new ArgumentNullException("Detail.model.enews_id");
            }
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            dao.DeleteC101MDetail(model);

            sm.LastResultMessage = "該公告已刪除";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A7", seCache = "2" });

            return View("Detail", model);
        }

    }
}