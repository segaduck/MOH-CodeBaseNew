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
    public class C103MController : BaseController
    {
        /// <summary>
        /// 聯絡我們查詢
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(C103MFormModel form)
        {
            A7DAO dao = new A7DAO();
            ActionResult rtn = View(form);

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                // 查詢結果
                form.Grid = dao.QueryC103MGrid(form);

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
        /// 新增聯絡我們
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C103MDetailModel model = new C103MDetailModel();

            model.status = "1";
            return View("Detail", model);
        }

        /// <summary>
        /// 編輯聯絡我們
        /// </summary>
        /// <param name="Modify"></param>
        /// <returns></returns>
        public ActionResult Modify(string con_id)
        {
            A7DAO dao = new A7DAO();
            SessionModel sm = SessionModel.Get();
            C103MDetailModel model = new C103MDetailModel();
            
            model.con_id = con_id;            
            model = dao.GetRow(model);
            
            if (model == null)
            { sm.LastErrorMessage = "找不到指定的資料!"; model = new C103MDetailModel(); }
            model.IsNew = false;
            model.con_name = model.con_cd;


            
            return View("Detail", model);
        }

        /// <summary>
        /// 儲存聯絡資訊
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(C103MDetailModel detail)
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            ActionResult rtn = View("Detail", detail);

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                // 檢核
                string ErrorMsg = dao.CheckC103M(detail);

                if (ErrorMsg == "")
                {
                    if (detail.IsNew)
                    {
                        dao.AppendC103MDetail(detail);
                        sm.LastResultMessage = "聯絡資訊新增成功";
                    }
                    else
                    {
                        dao.UpdateC103MDetail(detail);
                        sm.LastResultMessage = "聯絡資訊更新成功";
                    }
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C103M", new { area = "A7", useCache = "2" });
                }
                else
                {
                    sm.LastErrorMessage = ErrorMsg;
                }
            }
            return rtn;
        }

        /// <summary>
        /// 刪除聯絡我們
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(C103MDetailModel model)
        {
            if (string.IsNullOrEmpty(model.con_id.TONotNullString()))
            {
                throw new ArgumentNullException("Detail.model.con_id");
            }
            SessionModel sm = SessionModel.Get();
            A7DAO dao = new A7DAO();
            dao.DeleteC103MDetail(model);

            sm.LastResultMessage = "該聯絡資訊已刪除";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C103M", new { area = "A7", useCache = "2" });

            return View("Detail", model);
        }
    }
}