using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Omu.ValueInjecter;
using ES.Models;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_001036Controller : BaseController
    {

        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_001036ViewModel model = new Apply_001036ViewModel();
            model = new Apply_001036ViewModel();
            model.APP_ID = appid;
            model = dao.QueryApply_001036(model);
            //已繳費
            if(model.PAY_EXT_TIME_AC!= null)
            {
                //繳費金額
                model.PAY_MONEY = model.Apply.PAY_A_PAID;
            }
            return View("Index", model);
        }

        public ActionResult PrintPDF(string id)
        {
            BackApplyDAO dao = new BackApplyDAO();
            return File(dao.PrintPdf001036(id), "application/pdf", "Apply" + id + ".pdf");
        } 

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001036ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";

            if (ModelState.IsValid)
            {
                ModelState.Clear();

                if (model.Apply.FLOW_CD=="51")
                {
                    // 移轉醫事司
                    ErrorMsg = dao.SaveApply_001036_TO_001005(model);
                }
                else if(model.Apply.FLOW_CD == "52")
                {
                    // 返回重新分文
                    ErrorMsg = dao.SaveApply_001036ToAsign(model);
                }
                else
                {
                    ErrorMsg = dao.SaveApply_001036(model);
                }

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

            return Content(result.Serialize(), "application/json");
        }
    }
}
