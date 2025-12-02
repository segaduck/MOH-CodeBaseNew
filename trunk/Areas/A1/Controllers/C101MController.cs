using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EECOnline.Areas.A1.Models;
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

namespace EECOnline.Areas.A1.Controllers
{
    public class C101MController : BaseController
    {
        /// <summary>
        /// 查詢帳號
        /// </summary>
        public ActionResult Index(C101MFormModel form)
        {
            A1DAO dao = new A1DAO();
            ActionResult rtn = View(form);
            SessionModel sm = SessionModel.Get();
            form.unit_cd = sm.UserInfo.User.unit_cd;

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(form.rid, form.p);
                // 查詢結果
                form.Grid = dao.QueryC101MGrid(form);
                // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View
                if (!string.IsNullOrEmpty(form.rid) && form.useCache == 0) rtn = PartialView("_GridRows", form);
                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(form, dao, "Index");
            }

            return rtn;
        }

        /// <summary>
        /// 新增帳號
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New()
        {
            A1DAO dao = new A1DAO();
            SessionModel sm = SessionModel.Get();
            C101MDetailModel model = new C101MDetailModel();
            return View("Detail", model);
        }

        /// <summary>
        /// 編輯帳號
        /// </summary>
        /// <param name="userno"></param>
        /// <returns></returns>
        public ActionResult Modify(string userno)
        {
            A1DAO dao = new A1DAO();
            SessionModel sm = SessionModel.Get();
            C101MDetailModel model = new C101MDetailModel();
            model = dao.QueryC101MDetail(userno);

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
            A1DAO dao = new A1DAO();
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
                        sm.LastResultMessage = "帳號新增成功";
                    }
                    else
                    {
                        dao.UpdateC101MDetail(detail);
                        sm.LastResultMessage = "帳號更新成功";
                    }
                    sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A1", useCache = "2" });
                }
                else
                {
                    sm.LastErrorMessage = ErrorMsg;
                }
            }
            return rtn;
        }

        /// <summary>
        /// 刪除帳號
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(C101MDetailModel model)
        {
            if (string.IsNullOrEmpty(model.code.TONotNullString()))
            {
                throw new ArgumentNullException("Detail.CODE");
            }
            SessionModel sm = SessionModel.Get();
            A1DAO dao = new A1DAO();
            dao.DeleteC101MDetail(model);

            sm.LastResultMessage = "該醫院資料已刪除";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A1", useCache = "2" });

            return View("Detail", model);
        }

        /// <summary>
        /// 獲得醫院授權碼
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetValidateCode(string code)
        {
            A1DAO dao = new A1DAO();
            SessionModel sm = SessionModel.Get();
            C101MDetailModel model = dao.QueryC101MDetail(code);

            if (model == null)
            {
                sm.LastErrorMessage = "找不到指定的資料!";
                return Json(new { success = false, message = "找不到指定的資料!" });
            }

            EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
            var vCode = vc.CreateValidateCode(6);

            TblEEC_Hospital where = new TblEEC_Hospital() { code = code };
            TblEEC_Hospital update = new TblEEC_Hospital()
            {
                AuthCode = vCode,
                AuthStatus = "1",  // 帳號啟用
                AuthDate = DateTime.Now.ToString("yyyyMMddHHmmss"),
            };
            int res1 = dao.Update(update, where);

            int res2 = dao.Insert(new TblAMUROLE_Hosp()
            {
                AuthCode = vCode,
                grp_id = 1,  // 預設 醫院群組
                moduser = sm.UserInfo.User.userno,
                modusername = sm.UserInfo.User.username,
                modtime = HelperUtil.DateTimeToLongTwString(DateTime.Now),
                modip = sm.UserInfo.LoginIP,
            });

            dao.C101MSendSetPwdMail(model.code, model.text, model.Email, vCode, sm.UserInfo.LoginIP);

            sm.LastResultMessage = "帳號更新成功";
            sm.RedirectUrlAfterBlock = Url.Action("Index", "C101M", new { area = "A1", useCache = "2" });

            return Json(new { success = true, message = sm.LastResultMessage });
        }

        [HttpPost]
        public ActionResult SendSetPwdMail(C101MDetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            A1DAO dao = new A1DAO();
            dao.C101MSendSetPwdMail(model.code, model.text, model.Email, model.AuthCode, sm.UserInfo.LoginIP);
            sm.LastResultMessage = "已寄送信件";
            return View("Detail", model);
        }
    }
}