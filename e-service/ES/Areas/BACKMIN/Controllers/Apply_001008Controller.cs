using ES.Areas.Admin.Models;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Services;
using System.Web.Mvc;

namespace ES.Areas.Admin.Controllers
{
    public class Apply_001008Controller : BaseController
    {
        //
        // GET: /BACKMIN/Apply_001008/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="srvid"></param>
        /// <returns></returns>
        public ActionResult Index(string appid, string srvid)
        {
            var APP_ID = appid;
            BackApplyDAO dao = new BackApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001008FormModel model = new Apply_001008FormModel();
            ActionResult rtn = View(model);

            model = dao.QueryApply_001008(APP_ID);
            model.FileList = dao.GetFile_001008(APP_ID);

            model.IsNew = false;
            model.ME.IsReadOnly = true;
            model.PR.IsReadOnly = true;
            model.TRANS.IsReadOnly = true;
            model.TRANSF.IsReadOnly = true;
            if (!string.IsNullOrEmpty(model.PAY_EXT_TIME_AC))
            {
                model.PAY_MONEY = model.Apply.PAY_A_PAID;
            }
            rtn = View("Index", model);

            return rtn;
        }

        /// <summary>
        /// 套印申請書
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult PrintPDF(string id)
        {
            BackApplyDAO dao = new BackApplyDAO();
            logger.Debug("PrintPDF 001008_id:" + id);
            return File(dao.PrintPdf001008(id), "application/pdf", "Apply" + id + ".pdf");
        }

        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(Apply_001008FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            BackApplyDAO dao = new BackApplyDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            var ErrorMsg = "";

            ModelState.Clear();
            this.FormValidate(model);

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 存檔
                ErrorMsg = dao.SaveApply001008(model);

                if (ErrorMsg == "")
                {
                    result.status = true;
                    result.message = "存檔成功 !";
                }
                else { result.message = ErrorMsg; }
            }
            else
            {
                foreach (ModelState item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg += item.Errors[0].ErrorMessage + "\r\n";
                    }
                }

                result.message = ErrorMsg;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 繳費金額計算方式範例
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PayMemo()
        {
            return View();
        }

        /// <summary>
        /// 輸入檢核
        /// </summary>
        /// <param name="mstate"></param>
        /// <param name="model"></param>
        public void FormValidate(Apply_001008FormModel model)
        {
            if (string.IsNullOrWhiteSpace(model.APP_ID))
            {
                ModelState.AddModelError("APP_ID", "操作失敗，請聯絡系統管理員 !");
            }
            else
            {
                //繳費情形
                #region 繳費情形
                if (!string.IsNullOrWhiteSpace(model.PAY_EXT_TIME_AC))
                {
                    if (!model.IsPay)
                    {
                        ModelState.AddModelError("IsPay", "請勾選 是否已繳費!");
                    }
                }

                if (model.IsPay)
                {
                    if (string.IsNullOrWhiteSpace(model.PAY_EXT_TIME_AC))
                    {
                        ModelState.AddModelError("PAY_EXT_TIME_AC", "請輸入交易日期!");
                    }
                }
                #endregion

                //案件進度歷程
                #region 案件進度歷程
                if (string.IsNullOrWhiteSpace(model.FLOW_CD))
                {
                    ModelState.AddModelError("FLOW_CD", "請選擇案件狀態!");
                }
                else
                {
                    if (model.FLOW_CD == "2")
                    {
                        if (string.IsNullOrWhiteSpace(model.NoticeCheck))
                        {
                            ModelState.AddModelError("NOTICE_CHECK", "請至少選擇一種補件項目!");
                        }

                        if (!string.IsNullOrWhiteSpace(model.NoticeCheck) && string.IsNullOrWhiteSpace(model.Note))
                        {
                            ModelState.AddModelError("NOTE_CHECK", "請填寫補件內容!");
                        }
                    }
                    else if (model.FLOW_CD == "9")
                    {
                        if (string.IsNullOrWhiteSpace(model.Note))
                        {
                            ModelState.AddModelError("NOTE_CHECK", "請填寫備註!");
                        }
                    }
                    else if (model.FLOW_CD == "12")
                    {
                        if (string.IsNullOrWhiteSpace(model.MAIL_DATE_AD))
                        {
                            ModelState.AddModelError("MAIL_DATE_CHECK", "請輸入郵寄日期!");
                        }

                        if (string.IsNullOrWhiteSpace(model.MAIL_BARCODE))
                        {
                            ModelState.AddModelError("MAIL_BARCODE_CHECK", "請輸入掛號條碼!");
                        }
                    }
                }
                #endregion
            }
        }
    }
}
