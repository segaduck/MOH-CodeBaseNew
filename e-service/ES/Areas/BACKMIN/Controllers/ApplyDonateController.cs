using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using WebUI.CustomClass;
using System.Web.Routing;

namespace ES.Areas.Admin.Controllers
{
    public class ApplyDonateController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 9;
        }
        /// <summary>
        /// 線上捐款專案
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            SessionModel sm = SessionModel.Get();
            ApplyDonateDAO dao = new ApplyDonateDAO();
            ApplyDonateViewModel model = new ApplyDonateViewModel();
            model.Grid = dao.GetApplyDonateGrid();
            return View("Index", model);
        }

        /// <summary>
        /// 編輯/新增賑災專戶
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(ApplyDonateViewModel model, string srv_id_donate)
        {
            if (model == null)
            {
                model = new ApplyDonateViewModel();
            }
            else if (model.Detail != null && !string.IsNullOrEmpty(model.Detail.SRV_ID_DONATE) && string.IsNullOrEmpty(srv_id_donate))
            {
                srv_id_donate = model.Detail.SRV_ID_DONATE;
            }
            if (!string.IsNullOrEmpty(srv_id_donate))
            {
                //讀取資料
                ApplyDonateDAO dao = new ApplyDonateDAO();
                model.Detail = dao.GetApplyDonate(srv_id_donate);
                if (model.Detail != null)
                {
                    model.Detail.FileList = dao.GetApplyDonateFile(srv_id_donate);
                }
            }

            return View("Edit", model);
        }

        public ActionResult SaveFile(ApplyDonateViewModel model)
        {
            ApplyDonateDAO dao = new ApplyDonateDAO();
            SessionModel sm = SessionModel.Get();
            // 上傳檔案 或是 刪除檔案
            if (model.File_1 == null || string.IsNullOrEmpty(model.Detail.SRV_ID_DONATE))
            {
                if (string.IsNullOrEmpty(model.Detail.SRV_ID_DONATE))
                {
                    sm.LastErrorMessage = "檔案上傳失敗，找不到賑災專戶";
                    return View("Index", model);
                }
                else
                {
                    sm.LastErrorMessage = "檔案上傳失敗，找不到檔案";
                }
                return RedirectToAction("Edit", "ApplyDonate", new { model, model.Detail.SRV_ID_DONATE });
            }
            else
            {
                var user = GetAccount();
                dao.UploadFileDonate(model, user);
                sm.LastResultMessage = "檔案上傳成功";
                return RedirectToAction("Edit", "ApplyDonate", new { model, model.Detail.SRV_ID_DONATE });
            }
        }

        /// <summary>
        /// 草稿儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveDraft(ApplyDonateViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDonateDAO dao = new ApplyDonateDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            //if (ModelState.IsValid)
            //{
            ModelState.Clear();
            ErrorMsg = dao.CheckDonate(model);
            if (ErrorMsg == "")
            {
                var user = GetAccount();
                TblAPPLY_DONATE where = new TblAPPLY_DONATE();
                var fmlst = dao.GetRowList(where);
                var newfmlst = from a in fmlst
                               orderby a.SRV_ID_DONATE descending
                               select a.SRV_ID_DONATE;
                var Msg = dao.AppendDonate(model, newfmlst.FirstOrDefault(), user, true);
                result.status = true;
                result.message = "存檔成功 !";
            }
            else
            {
                result.status = false;
                result.message = ErrorMsg;
            }
            //}

            return Content(result.Serialize(), "application/json");
        }
        /// <summary>
        /// 正式儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(ApplyDonateViewModel model)
        {
            ApplyDonateDAO dao = new ApplyDonateDAO();
            SessionModel sm = SessionModel.Get();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            //if (ModelState.IsValid)
            //{
            ModelState.Clear();
            ErrorMsg = dao.CheckDonate(model);
            if (ErrorMsg == "")
            {
                var user = GetAccount();
                TblAPPLY_DONATE where = new TblAPPLY_DONATE();
                var fmlst = dao.GetRowList(where);
                var newfmlst = from a in fmlst
                               orderby a.SRV_ID_DONATE descending
                               select a.SRV_ID_DONATE;
                var Msg = dao.AppendDonate(model, newfmlst.FirstOrDefault(), user, false);
                result.status = true;
                result.message = "存檔成功 !";
            }
            else
            {
                result.status = false;
                result.message = ErrorMsg;
            }
            //}

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 變更上下架狀態
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult OpenChange(string srv_id_donate, string isopen, string isdraft)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDonateDAO dao = new ApplyDonateDAO();
            var result = new AjaxResultStruct();
            if (!string.IsNullOrWhiteSpace(srv_id_donate))
            {
                if (isdraft == "Y")
                {
                    result.status = false;
                    result.message = "非正式儲存不得上架。";
                }
                else
                {
                    var user = GetAccount();
                    var Msg = dao.OpenChangeStatus(srv_id_donate, isopen, user);
                    result.status = true;
                    result.message = "存檔成功 !";
                }
            }
            else
            {
                result.status = false;
                result.message = "儲存失敗，請洽管理人員。";
            }

            return Content(result.Serialize(), "application/json");
        }


        /// <summary>
        /// 檔案刪除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult DelFile(string filestr)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDonateDAO dao = new ApplyDonateDAO();
            var result = new AjaxResultStruct();
            if (!string.IsNullOrWhiteSpace(filestr))
            {
                var user = GetAccount();
                var Msg = dao.DelFileDonate(filestr, user);
                result.status = true;
                result.message = "刪除成功 !";
            }
            else
            {
                result.status = false;
                result.message = "刪除失敗，請洽管理人員。";
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 捐款明細
        /// </summary>
        /// <param name="model"></param>
        /// <param name="srv_id_donate"></param>
        /// <returns></returns>
        public ActionResult ApplyDetail(ApplyDonateViewModel model, string srv_id_donate)
        {
            ApplyDonateDAO dao = new ApplyDonateDAO();
            model.GridDetail = dao.GetApplyDonateGridDetail(srv_id_donate);
            return View("DetailGrid", model);
        }

        /// <summary>
        /// 下載捐款明細
        /// </summary>
        /// <param name="model"></param>
        /// <param name="srv_id_donate"></param>
        /// <returns></returns>
        public ActionResult ExportDetail(string srv_id_donate)
        {
            ApplyDonateDAO dao = new ApplyDonateDAO();
            var list = dao.GetApplyDonateGridDetail(srv_id_donate);
            System.Data.DataTable dt = null;
            dt = dao.ExportDonateDetail(list);
            MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt);
            return File(ms, "application/unknown", "Download.xls");
        }
    }
}
