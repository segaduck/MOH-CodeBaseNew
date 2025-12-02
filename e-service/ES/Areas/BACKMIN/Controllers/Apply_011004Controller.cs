using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Services;
using System.Web.Mvc;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_011004Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_011004FormModel model = new Apply_011004FormModel();
            model = dao.QueryApply_011004(appid);
            model.APP_ID = appid;
            if (model != null)
            {
                #region 調整資料後帶入欄位
                //日期轉民國年
                model.APP_DATE_STR = HelperUtil.TransToTwYear(model.APP_TIME);
                model.APP_EXT = HelperUtil.TransToTwYear(model.APP_EXT_DATE);
                model.BIRTHDAY_STR = HelperUtil.TransToTwYear(model.BIRTHDAY);
                #endregion
                // 取檔案
                model.FileList = dao.GetFileList_011004(appid);
            }
            return View("Index", model);
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_011004FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                ErrorMsg = dao.AppendApply011004(model);
                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";
                }
                else
                {
                    result.status = false;
                    result.message = ErrorMsg;
                }
            }
            else
            {
                result.status = false;
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg = ErrorMsg + item.Errors[0].ErrorMessage + "\r\n";
                    }
                }
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }
            return Content(result.Serialize(), "application/json");
        }       
    }
}
