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
    public class Apply_001039Controller : BaseController
    {
        public ActionResult Index(string appid, string srvid)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            Apply_001039ViewModel model = new Apply_001039ViewModel();
            model.Form = new Apply_001039FormModel();
            model.Form.APP_ID = appid;
            model.Form = dao.QueryApply_001039(model.Form);

            if (model.Form != null)
            {
                if (model.Form.FLOW_CD == "2")
                {
                    model.Detail = dao.GetApplyNotice_001039(appid);
                }

                #region 調整資料後帶入欄位
                model.Form.APPTYPE = "醫師赴國外訓練英文保證函";

                //聯絡人電話
                if (!string.IsNullOrEmpty(model.Form.CONTACT_TEL))
                {
                    var tel_0 = model.Form.CONTACT_TEL.ToSplit('-')[0];
                    var tel_2 = (model.Form.CONTACT_TEL.ToSplit('#').ToCount() > 1) ? model.Form.CONTACT_TEL.ToSplit('#')[1] : "";
                    var tel_1 = (string.IsNullOrEmpty(tel_2)) ? model.Form.CONTACT_TEL.ToSplit('-')[1] : model.Form.CONTACT_TEL.ToSplit('-')[1].ToSplit('#')[0];
                    model.Form.CONTACT_TEL_0 = tel_0;
                    model.Form.CONTACT_TEL_1 = tel_1;
                    model.Form.CONTACT_TEL_2 = tel_2;
                }
                //聯絡人傳真
                if (!string.IsNullOrEmpty(model.Form.CONTACT_FAX))
                {
                    var fax_0 = model.Form.CONTACT_FAX.ToSplit('-')[0];
                    var fax_2 = (model.Form.CONTACT_FAX.ToSplit('#').ToCount() > 1) ? model.Form.CONTACT_FAX.ToSplit('#')[1] : "";
                    var fax_1 = (string.IsNullOrEmpty(fax_2)) ? model.Form.CONTACT_FAX.ToSplit('-')[1] : model.Form.CONTACT_FAX.ToSplit('-')[1].ToSplit('#')[0];
                    model.Form.CONTACT_FAX_0 = fax_0;
                    model.Form.CONTACT_FAX_1 = fax_1;
                    model.Form.CONTACT_FAX_2 = fax_2;
                }
                //Mail
                if (!string.IsNullOrEmpty(model.Form.E_MAIL))
                {
                    var email = model.Form.E_MAIL.ToSplit('@');
                    model.Form.E_MAIL_1 = email[0];
                    switch (email[1])
                    {
                        case "gmail.com":
                            model.Form.E_MAIL_2 = "1";
                            break;
                        case "yahoo.com.tw":
                            model.Form.E_MAIL_2 = "2";
                            break;
                        case "outlook.com":
                            model.Form.E_MAIL_2 = "3";
                            break;
                        default:
                            model.Form.E_MAIL_2 = "0";
                            model.Form.E_MAIL_3 = email[1];
                            break;
                    }
                    
                }
             
                model.Form.APPLY_DATE_STR = HelperUtil.TransToTwYear(model.Form.APPLY_DATE);
                model.Form.APP_EXT_DATE = HelperUtil.TransToTwYear(model.Form.APP_EXT);

                #endregion

                // 取檔案
                model.Form.FileList = dao.GetFileList_001039(model.Form.APP_ID);
            }

            return View("Index", model);
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001039ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            string ErrorMsg = "";
            result.message = "系統錯誤!";
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                ErrorMsg = dao.AppendApply001039(model);
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
