using ES.Action;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using System;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace ES.Controllers
{
    public class ServiceLstController : BaseNoMemberController
    {
        /// <summary>
        /// 申辦服務
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(string ACT_TYPE, string LST_ID)
        {
            WebDAO dao = new WebDAO();
            ServiceLstViewModel model = new ServiceLstViewModel();
            // 預設顯示醫事司相關申辦案件
            model.ACT_TYPE = "1";
            model.LST_ID = 1;
            // 案件返回重新輸入查詢狀態
            if (!string.IsNullOrWhiteSpace(ACT_TYPE) && !string.IsNullOrWhiteSpace(LST_ID))
            {
                model.ACT_TYPE = ACT_TYPE;
                model.LST_ID = Convert.ToInt32(LST_ID);
            }
            model.Grid = dao.GetServiceLst(model);
            ActionResult rtn = View("Index", model);
            return rtn;
        }

        /// <summary>
        /// 服務項目
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string act_type, int? id)
        {
            int i_id = (id ?? 0);

            //string s_log1 = "";
            //s_log1 += string.Format("\n  act_type:{0}", act_type);
            //s_log1 += string.Format("\n  i_id:{0}", i_id);
            //logger.Debug(s_log1);

            ServiceLstViewModel model = new ServiceLstViewModel();
            model.ACT_TYPE = (!string.IsNullOrEmpty(act_type) ? act_type : "1");
            model.LST_ID = i_id;
            //s_log1 = ""; //string s_log1 = "";
            //s_log1 += string.Format("\n  model.active_type:{0}", model.active_type);
            //logger.Debug(s_log1);
            //SessionModel sm = SessionModel.Get();

            WebDAO dao = new WebDAO();

            model.Grid = dao.GetServiceLst(model);

            //return PartialView("_GridRows", model);
            return View(model);
        }

        /// <summary>
        /// 歷史消息查詢-LIST
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult IndexXX(MessageActionModel model)
        {
            ViewBag.tempMessage = TempData["tempMessage"];

            if (ModelState.IsValid)
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    SetSearchCode(conn, 6);

                    MessageAction action = new MessageAction(conn);

                    ViewBag.List = action.GetList(model);

                    double pageSize = action.GetPageSize();
                    double totalCount = action.GetTotalCount();

                    ViewBag.NowPage = model.NowPage;
                    ViewBag.TotalCount = action.GetTotalCount();
                    ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);
                    conn.Close();
                    conn.Dispose();
                }
            }

            return View(model);
        }

        /// <summary>
        /// 申請須知
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Notice(string id,string ACT_TYPE, string LST_ID)
        {
            ServiceNoticeModel model = null;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ServiceAction action = new ServiceAction(conn);
                model = action.GetNotice(id);
                conn.Close();
                conn.Dispose();
            }
            model.ACT_TYPE = ACT_TYPE;
            model.LST_ID = LST_ID;
            return View(model);
        }
    }
}
