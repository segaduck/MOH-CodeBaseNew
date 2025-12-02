using DocumentFormat.OpenXml.Office2010.ExcelAc;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using Omu.ValueInjecter;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_011010Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_011010ViewModel model = new Apply_011010ViewModel();
            string s_APP_NAME = "全國社會工作專業人員選拔推薦";

            model.Form = new Apply_011010FormModel();
            model.Form.APP_ID = appid;
            model.Form = dao.QueryApply_011010(model.Form);
            if (model.Form == null)
            {
                string s_log1 = "##Apply_011010Controller dao.QueryApply_011010(model.Form) is null";
                logger.Error(s_log1);
                ViewBag.tempMessage = "查詢資料有誤!";
                sm.LastErrorMessage = ViewBag.tempMessage;
                return View("Index", model);
            }
 
            model.Form.APPLY_DATE_TW = HelperUtil.DateTimeToTwString(model.Form.APPLY_DATE);
            model.Form.APP_NAME = s_APP_NAME;

            if (model.Form != null)
            {
                if (model.Form.FLOW_CD == "2")
                {
                    model.Detail = dao.GetApplyNotice_011010(appid);
                }
                // 取檔案
                model.Form.FileList = dao.GetFileList_011010(model.Form.APP_ID);
                model.Form.FileList.SRVLIST = new List<Apply_011010SRVLSTModel>();
                if(model.Form.FileList.FILE!=null && model.Form.FileList.FILE.Count > 0)
                {
                    foreach(var item in model.Form.FileList.FILE)
                    {
                        var insert_data = new Apply_011010SRVLSTModel();
                        insert_data.InjectFrom(item);
                        model.Form.FileList.SRVLIST.Add(insert_data);
                    }
                }
            }
            return View("Index", model);
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011010ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            // 檢核
            ErrorMsg = dao.CheckApply011010(model);

            if (ErrorMsg == "")
            {
                //存檔
                ErrorMsg = dao.AppendApply011010(model);
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
