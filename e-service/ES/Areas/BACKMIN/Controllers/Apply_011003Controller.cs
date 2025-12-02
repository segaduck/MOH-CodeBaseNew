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
    public class Apply_011003Controller : BaseController
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
            Apply_011003ViewModel model = new Apply_011003ViewModel();
            model.Form = new Apply_011003FormModel();
            model.Form.APP_ID = appid;
            model.Form = dao.QueryApply_011003(model.Form);

            if (model.Form != null)
            {
                #region 調整資料後帶入欄位

                //// 電話
                //if (model.Form.TEL.TONotNullString().Trim() != "")
                //{
                //    var Tel = model.Form.TEL;
                //    var TelLst = model.Form.TEL.Split('-');
                //    if (TelLst.ToCount() == 2)
                //    {
                //        model.Form.TEL = TelLst[0];
                //        model.Form.TEL_0 = TelLst[1];

                //        if (TelLst[1].IndexOf('#') > 0)
                //        {
                //            model.Form.TEL_0 = TelLst[1].Substring(0, TelLst[1].IndexOf('#'));
                //            model.Form.TEL_1 = TelLst[1].Split('#')[1];
                //        }
                //    }
                //    else
                //    {
                //        model.Form.TEL = Tel.Substring(0, 2);
                //        model.Form.TEL_0 = Tel.SubstringTo(2);
                //        if (Tel.SubstringTo(2).IndexOf('#') > 0)
                //        {
                //            model.Form.TEL_0 = (Tel.SubstringTo(2)).Substring(0, (Tel.SubstringTo(2)).IndexOf('#'));
                //            model.Form.TEL_1 = (Tel.SubstringTo(2)).Split('#')[1];
                //        }
                //    }
                //}
                //// FAX                   
                //if (model.Form.FAX.TONotNullString().Trim() != "")
                //{
                //    var FAX = model.Form.FAX;
                //    var FAXLst = model.Form.TEL.Split('-');
                //    if (FAXLst.ToCount() == 2)
                //    {
                //        model.Form.FAX = FAXLst[0];
                //        model.Form.FAX_0 = FAXLst[1];

                //        if (FAXLst[1].IndexOf('#') > 0)
                //        {
                //            model.Form.FAX_0 = FAXLst[1].Substring(0, FAXLst[1].IndexOf('#'));
                //            model.Form.FAX_1 = FAXLst[1].Split('#')[1];
                //        }
                //    }
                //    else
                //    {
                //        model.Form.FAX = FAX.Substring(0, 2);
                //        model.Form.FAX_0 = FAX.SubstringTo(2);
                //        if (FAX.SubstringTo(2).IndexOf('#') > 0)
                //        {
                //            model.Form.FAX_0 = (FAX.SubstringTo(2)).Substring(0, (FAX.SubstringTo(2)).IndexOf('#'));
                //            model.Form.FAX_1 = (FAX.SubstringTo(2)).Split('#')[1];
                //        }
                //    }

                //}
                // 地址
                var addr = model.Form.ADDR;
                model.Form.ADDR = model.Form.ADDR_CODE;
                TblZIPCODE zip = new TblZIPCODE();
                zip.ZIP_CO = model.Form.ADDR_CODE;
                var getnam = dao.GetRow(zip);
                if (getnam.CITYNM != null)
                {
                    model.Form.ADDR = getnam.ZIP_CO;
                    model.Form.ADDR_DETAIL = addr.TONotNullString().Replace(getnam.CITYNM + getnam.TOWNNM, "");
                }

                //Mail
                if (model.Form.MAIL.TONotNullString().Trim() != "")
                {
                    model.Form.EMAIL = model.Form.MAIL;
                    var mail = model.Form.MAIL;
                    model.Form.MAIL = mail.Split('@')[0];
                    model.Form.MAIL_0 = "0";
                    model.Form.MAIL_1 = mail.Split('@')[1];

                    switch (mail.Split('@')[1])
                    {
                        case "gmail.com":
                            model.Form.MAIL_0 = "1";
                            model.Form.MAIL_1 = "";
                            break;
                        case "yahoo.com.tw":
                            model.Form.MAIL_0 = "2";
                            model.Form.MAIL_1 ="";
                            break;
                        case "outlook.com":
                            model.Form.MAIL_0 = "3";
                            model.Form.MAIL_1 = "";
                            break;
                    }
                }
                if (model.Form.GRADUATION.TONotNullString().Trim() != "")
                {
                    var GRADUATION = model.Form.GRADUATION;
                    model.Form.GRADUATION = GRADUATION.Substring(0, GRADUATION.Length - 2);
                    model.Form.GRADUATION_MONTH = GRADUATION.SubstringTo(GRADUATION.Length - 2);
                }
                if (model.Form.NOTICEDAY.TONotNullString().Trim() != "")
                {
                    var NOTICEDAY = model.Form.NOTICEDAY;
                    model.Form.NOTICEDAY_YEAR = NOTICEDAY.Substring(0, NOTICEDAY.Length - 2);
                    model.Form.NOTICEDAY_MONTH = NOTICEDAY.SubstringTo(NOTICEDAY.Length - 2);
                }
                model.Form.BIRTHDAY = HelperUtil.DateTimeToTwString(HelperUtil.TransToDateTime(model.Form.BIRTHDAY));
                #endregion
            }

            return View("Index", model);
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_011003ViewModel model)
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
                ErrorMsg = dao.CheckApply011003(model);
                if (ErrorMsg == "")
                {

                    // 存檔
                    ErrorMsg = dao.AppendApply011003(model);
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
