using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using System.Web.Mvc;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_011001Controller : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="srvid"></param>
        /// <returns></returns>
        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_011001FormModel Form = new Apply_011001FormModel();
            Form = new Apply_011001FormModel();
            Form.APP_ID = appid;
            Form = dao.QueryApply_011001(Form);
            Form.IsNew = false;
            if (Form != null)
            {
                #region 調整資料後帶入欄位

                // 電話
                Form.ACC_TEL = Form.TEL;

                // 地址
                Form.ACC_ADDR_CODE = Form.ADDR_CODE;
                Form.ACC_ADDR = Form.ADDR;

                #endregion

                // 取檔案
                Form.fileMode = dao.GetFileList_011001(Form.APP_ID);
            }

            return View("Index", Form);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_011001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                // 檢核
                ErrorMsg = dao.CheckApply011001(model);
                if (ErrorMsg == "")
                {
                    // 存檔
                    ErrorMsg = dao.AppendApply011001(model);
                    if (ErrorMsg == "")
                    {
                        result.status = true;
                        result.message = "存檔成功 !";
                    }
                    else result.message = ErrorMsg;
                }
                else result.message = ErrorMsg;                
            }

            return Content(result.Serialize(), "application/json");
        }       
    }
}
