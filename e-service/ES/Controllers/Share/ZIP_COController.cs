using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models.Share;
using ES.Models;
using ES.Controllers;
using ES.DataLayers;
using ES.Services;

namespace ES.Controllers.Share
{
    public class ZIP_COController : BaseNoMemberController
    {
        // GET: SHARE/ZIP_CO
        [HttpGet]
        public ActionResult Index()
        {
            ZIP_COFormModel form = new ZIP_COFormModel();
            form.NowPage = 0;

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
            ActionResult rtn = View(form);
            ShareDAO dao = new ShareDAO();

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if (form.CITYNM.TONotNullString() != "")
                {
                    form.CITYNM = form.CITYNM.Replace("台", "臺");
                }
                // 查詢結果
                form.Grid = dao.QueryZIP_CO(form);
                // 分頁
                double pageSize = dao.GetPageSize();
                double totalCount = dao.GetTotalCount();

                form.TotalCount = dao.GetTotalCount();
                form.TotalPage = (int)Math.Ceiling(totalCount / pageSize);
            }

            return rtn;
        }
    }
}