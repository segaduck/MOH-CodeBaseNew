using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ES.Controllers
{
    public class AJAX2Controller : BaseNoMemberController
    {
        /// <summary>
        /// 根據傳入代碼 返回清單選項(用於 Select 的 Options HTML)
        /// </summary>
        /// <param name="UNIT_CD"></param>
        /// <param name="VIL"></param>
        /// <returns></returns>
        public ActionResult VillageByServiceName(string LST_ID, string VIL)
        {
            //JsonResult
            //string s_log1 = "";
            //s_log1 += string.Format("\n ##VillageByServiceName LST_ID:{0} ", LST_ID);
            //s_log1 += string.Format("\t VIL:{0} ", VIL);
            //logger.Debug(s_log1);

            if (!string.IsNullOrEmpty(VIL)) { ViewBag.Selected = VIL; }
            //PartialView("_SelectOptions", QueryList("queryServiceList", UNIT_CD));
            //List<SelectListItem> list = CodeUtils.GetServiceLst1(LST_ID, VIL);
            List<SelectListItem> list = null;
            using (SqlConnection conn = GetConnection())
            {
                DataUtils.OpenDbConn(conn);
                list = CodeUtils.GetServiceLst3(conn, LST_ID, VIL);
                DataUtils.CloseDbConn(conn);
            }
            return PartialView("_SelectOptions", list);
            //return Json(list, JsonRequestBehavior.AllowGet);
        }


    }
}
