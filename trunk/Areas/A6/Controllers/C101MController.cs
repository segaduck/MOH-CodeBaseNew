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
    public class C101MController : BaseController
    {
        ///// <summary>
        ///// 群組首頁
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult Index()
        //{
        //    A6DAO dao = new A6DAO();
        //    C101MFormModel form = new C101MFormModel();
        //    ActionResult rtn = View(form);

        //    return View(form);
        //}

        /// <summary>
        /// 查詢
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult Index(C101MFormModel form)
        {
            A6DAO dao = new A6DAO();
            ActionResult rtn = View(form);
            SessionModel sm = SessionModel.Get();
            form.unit_cd = sm.UserInfo.User.unit_cd;

            // 檢核權限
            TblAMUROLE tr = new TblAMUROLE();
            tr.userno = sm.UserInfo.UserNo;
            var trlist = dao.GetRowList(tr);

            if (trlist.ToCount() == 1)
            {
                TblAMGRP tg = new TblAMGRP();
                tg.grp_id = trlist.FirstOrDefault().grp_id;
                var tgdata = dao.GetRow(tg);
                if (tgdata.grpname.Contains("案件處理"))
                {
                    ModelState.AddModelError("Authority", "您的權限不足 !");
                }
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
        /// 新增
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            A6DAO dao = new A6DAO();
            SessionModel sm = SessionModel.Get();
        
            C101MDetailModel model = new C101MDetailModel();
            // 預設啟用
            model.unit_cd = sm.UserInfo.User.unit_cd;
            model.grp_status = "1";

            return View("Detail", model);
        }

        /// <summary>
        /// 編輯群組
        /// </summary>
        /// <param name="grp_id"></param>
        /// <returns></returns>
        public ActionResult Modify(string grp_id)
        {
            A6DAO dao = new A6DAO();
            SessionModel sm = SessionModel.Get();
            C101MDetailModel model = new C101MDetailModel();
            model.grp_id = grp_id;           
            model = dao.GetRow(model);
            model.hdgrpname = model.grpname;
            if (model == null)
            { sm.LastErrorMessage = "找不到指定的資料!"; model = new C101MDetailModel(); }
            model.IsNew = false;

            return View("Detail", model);
        }

        /// <summary>
        /// 儲存群組
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(C101MDetailModel detail)
        {
            // 由登入資訊取得當前角色的檢定類別資訊s
            SessionModel sm = SessionModel.Get();
            A6DAO dao = new A6DAO();
            ActionResult rtn = View("Detail", detail);
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
                        sm.LastResultMessage = "群組新增成功";
                    }
                    else
                    {
                        dao.UpdateC101MDetail(detail);
                        sm.LastResultMessage = "群組更新成功";
                    }
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A6", useCache = "2" });
                }
                else
                {
                    sm.LastErrorMessage = ErrorMsg;
                }
            }

            return rtn;
        }

        /// <summary>
        /// 刪除群組
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(C101MDetailModel model)
        {
            if (string.IsNullOrEmpty(model.grp_id.TONotNullString()))
            {
                throw new ArgumentNullException("Detail.GRPID");
            }
            SessionModel sm = SessionModel.Get();
            A6DAO dao = new A6DAO();
            dao.DeleteC101MDetail(model);

            sm.LastResultMessage = "群組已刪除";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A6", useCache = "2" });

            return View("Detail", model);
        }

        /// <summary>
        /// 編輯權限
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>      
        [HttpGet]
        public ActionResult Set(string grp_id, string grpname)
        {
            // 由登入資訊取得當前角色的檢定類別資訊
            SessionModel sm = SessionModel.Get();
            C101MFuncmModel model = new C101MFuncmModel();
            model.grp_id = grp_id;
            model.grpname = grpname;

            return Set(model);
        }

        /// <summary>
        /// 編輯權限查詢
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Set(C101MFuncmModel model)
        {
            A6DAO dao = new A6DAO();
            ActionResult rtn = View(model);
            ModelState.Clear(); 
            model.Grid = dao.QueryC101MSetGrid(model);

            return rtn;
        }

        /// <summary>
        /// 儲存權限
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetSave(C101MFuncmModel model)
        {
            SessionModel sm = SessionModel.Get();
            ModelState.Clear();
            if (model.Grid.Count > 0)
            {
                //判斷已選取項目內的資訊是否完整
                foreach (C101MFuncmGridModel item in model.Grid)
                {
                    if (string.IsNullOrEmpty(item.grp_id.TONotNullString()))
                    {
                        throw new ArgumentNullException("C101MFuncmGridModel.grpid");
                    }

                    if (string.IsNullOrEmpty(item.sysid.TONotNullString()))
                    {
                        throw new ArgumentNullException("C101MFuncmGridModel.sys_id");
                    }

                    if (string.IsNullOrEmpty(item.prgid.TONotNullString()))
                    {
                        throw new ArgumentNullException("C101MFuncmGridModel.prgid");
                    }

                    //解決PKEY不得為null
                    if (string.IsNullOrEmpty(item.modules)) item.modules = "";
                    if (string.IsNullOrEmpty(item.submodules)) item.submodules = "";

                    item.modip = sm.UserInfo.LoginIP;
                    item.modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now);
                    item.moduser = sm.UserInfo.UserNo;
                    item.modusername = sm.UserInfo.User.username;
                }
                ///執行
                A6DAO dao = new A6DAO();
                dao.UpdateORAppendC101MSetGrid(model.Grid);
                sm.LastResultMessage = "權限設定已儲存";
                sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A6", useCache = "2" });
            }

            return View("Set", model);
        }

    }
}