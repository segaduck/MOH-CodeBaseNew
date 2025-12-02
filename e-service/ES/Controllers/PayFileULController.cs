using System;
using System.Web.Mvc;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;

namespace ES.Controllers
{
    public class PayFileULController : BaseController
    {
        #region 上傳繳費資料檔案
        public ActionResult Index(string Up_PayFileStr)
        {
            SessionModel sm = SessionModel.Get();
            if (string.IsNullOrWhiteSpace(Up_PayFileStr)) { sm.LastErrorMessage = "您無權限使用此功能 !"; return null; }
            var strArr = Up_PayFileStr.Split('-');
            ULPayFileFormModel model = new ULPayFileFormModel();
            model.APP_ID = strArr[0];
            model.SRV_ID = strArr[1];
            model.FILE_NO = strArr[2];
            return View("PayFileUL", model);
        }

        public ActionResult PayFileULSave(ULPayFileFormModel model)
        {
            var result = new AjaxResultStruct();
            ShareDAO dao = new ShareDAO();
            SessionModel sm = SessionModel.Get();
            model.ERRMSG = "";
            if (sm.UserInfo == null || sm.UserInfo.Member == null)
            {
                model.ERRMSG += "請重新登入系統<br/>";
                ModelState.AddModelError("LOGIN_ERR", "請重新登入系統");
            }
            else if (model.ATTACH_FILE == null)
            {
                model.ERRMSG += "請至少上傳一個檔案<br/>";
                ModelState.AddModelError("ATTACH_FILE", "請至少上傳一個檔案");
            }
            else if (model.ATTACH_FILE.ContentLength == 0)
            {
                model.ERRMSG += "請至少上傳一個檔案<br/>";
                ModelState.AddModelError("ATTACH_FILE", "請至少上傳一個檔案");
            }
            ////避免使用者上傳到別的申請案件
            //if (dao.GetRow(new ApplyModel() { APP_ID = model.APP_ID, SRV_ID = model.SRV_ID }).ADD_ACC != sm.UserInfo.Member.ACC_NO)
            //{
            //    model.ERRMSG += "您無權限上傳該檔案<br/>";
            //    ModelState.AddModelError("MEMBER_ERR", "您無權限上傳該檔案");
            //}
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                try
                {
                    //更新APPLY_FILE Model
                    dao.SavePayFile(model);
                    ApplyModel whereApply = new ApplyModel();
                    whereApply.APP_ID = model.APP_ID;
                    var data = dao.GetApplyData(whereApply);
                    if (data != null)
                    {
                        if (!(model.SRV_ID == "005004") && data.FLOW_CD == "2")
                        {
                            //更新補件狀態
                            dao.UpdateApplyNotice(model.APP_ID, model.SRV_ID);
                            var memberName = string.IsNullOrWhiteSpace(model.NAME) ? sm.UserInfo.Member.NAME : model.NAME;
                            var memberEmail = string.IsNullOrWhiteSpace(model.EMAIL) ? sm.UserInfo.Member.MAIL : model.EMAIL;
                            dao.SendMail_Update(memberName, memberEmail, model.APP_ID, model.SRV_ID, "1");
                        }
                    }

                    model.STATUS = "Y";
                    sm.LastResultMessage = "成功上傳繳費紀錄檔案!!";
                }
                catch (Exception ex)
                {
                    logger.Error("PayFileUL failed:" + ex.Message, ex);
                    sm.LastResultMessage = "上傳失敗，您無權限上傳該案件繳費檔案!!";
                }
            }

            return View("PayFileUL", model);
        }
        #endregion 上傳繳費資料檔案

    }
}
