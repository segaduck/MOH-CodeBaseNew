using ES.Areas.Admin.Action;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;


namespace ES.Areas.Admin.Controllers
{
    public class ApplyDonateDetailController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 9;
        }
        /// <summary>
        /// 線上捐款明細查詢
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ApplyDonateDetailViewModel model = new ApplyDonateDetailViewModel();

            CaseAction action = new CaseAction();

            model.Grid = action.GetAPPLYDonateDetail(model, GetAccountModel());

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            model.NowPage = model.NowPage;
            model.TotalCount = action.GetTotalCount();
            model.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(ApplyDonateDetailViewModel model)
        {
            CaseAction action = new CaseAction();
            model.Grid = action.GetAPPLYDonateDetail(model, GetAccountModel());

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            model.NowPage = model.NowPage;
            model.TotalCount = action.GetTotalCount();
            model.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            return View(model);
        }
        /// <summary>
        /// 線上捐款明細
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(string app_id)
        {
            ApplyDonateDetailModifyModel model = new ApplyDonateDetailModifyModel();
            ApplyDonateDAO dao = new ApplyDonateDAO();
            try
            {
                model = dao.GetAPPLYDonateDetail(app_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                return Index();
            }
            return View("Edit", model);
        }

        /// <summary>
        /// 僅依據年度匯出CSV
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ExportCSV(ApplyDonateDetailViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            CaseAction action = new CaseAction();
            List<ApplyDonateCSVModel> list = new List<ApplyDonateCSVModel>();
            list = action.GetAPPLYDonateCSV(model);
            StringBuilder sb = new StringBuilder();
            try
            {
                // 表頭
                //sb.Append("DON_YR|DON_IDN|DON_NM|DON_AMT|DON_BAN|DON_KD|DONEE_NM");
                //sb.Append("\r\n");
                sb.Append("捐贈年度|捐贈者身分證統一編號|捐贈者姓名|捐款金額|受捐贈者統一編號(扣繳單位統一編號)|捐贈別|受捐贈者名稱");
                sb.Append("\r\n");
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        sb.Append($"{item.DON_YR}|{item.DON_IDN}|{item.DON_NM}|{item.DON_AMT}|{item.DON_BAN}|{item.DON_KD}|{item.DONEE_NM}");

                        //Append new line character.
                        sb.Append("\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex.Message, ex);
                return Index(model);
            }
            byte[] buffer = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(sb.ToString());
            Response.AppendHeader("Content-Disposition", "attachment;filename=\"線上捐款明細" + ".csv\"");
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            byte[] outBuffer = new byte[buffer.Length + 3];
            outBuffer[0] = (byte)0xEF;//有BOM,解决乱码
            outBuffer[1] = (byte)0xBB;
            outBuffer[2] = (byte)0xBF;
            Array.Copy(buffer, 0, outBuffer, 3, buffer.Length);
            return File(outBuffer, "application/ms-excel");
        }

        /// <summary>
        /// 線上捐款明細 儲存
        /// </summary>
        /// <returns></returns>
        public ActionResult Save(ApplyDonateDetailModifyModel model)
        {
            ApplyDonateDAO dao = new ApplyDonateDAO();
            SessionModel sm = SessionModel.Get();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            // 檢核
            ErrorMsg = dao.CheckDonateDetail(model);
            if (ErrorMsg == "")
            {
                ErrorMsg = dao.UpdateDonateDetail(model);
                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";
                }
                else result.message = ErrorMsg;
            }
            else result.message = ErrorMsg;

            return Content(result.Serialize(), "application/json");
        }
    }
}
