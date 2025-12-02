using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models;
using ES.Models.Entities;
using ES.Models.ViewModels;
using ES.Commons;
using ES.Services;
using System.ComponentModel;
using ES.DataLayers;
using Omu.ValueInjecter;
using System.Net.Mail;

namespace ES.Controllers
{
    public class Apply_001034Controller : BaseController
    {

        public ActionResult Index(Apply_001034ViewModel model)
        {
            return View();
        }

        [DisplayName("Apply_001034_申請")]
        public ActionResult Apply()
        {
            SessionModel sm = SessionModel.Get();

            if (sm.UserInfo == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                Apply_001034ViewModel form = new Apply_001034ViewModel();
                form.APPLY_DATE = HelperUtil.DateTimeToTwString(DateTime.Now);
                form.APPLY_NAME = sm.UserInfo.Member.NAME;
                form.APPLY_PID = sm.UserInfo.Member.IDN;

                return View(form);
            }

        }

        [HttpPost]
        public ActionResult Apply(Apply_001034ViewModel View)
        {
            var result = new AjaxResultStruct();
            result.message = "";
            result.status = true;

            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\.\-\,\s\(\)\'\:\：]+$");
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[0-9\.]+$");
            System.Text.RegularExpressions.Regex reg2 = new System.Text.RegularExpressions.Regex(@"^[0-9]+$");
            System.Text.RegularExpressions.Regex reg3 = new System.Text.RegularExpressions.Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,10})+)$");
            System.Text.RegularExpressions.Regex reg4 = new System.Text.RegularExpressions.Regex(@"^[\u4e00-\u9fa5]+$");
            System.Text.RegularExpressions.Regex reg5 = new System.Text.RegularExpressions.Regex(@"^[0-9\-\#]+$");
            System.Text.RegularExpressions.Regex reg6 = new System.Text.RegularExpressions.Regex(@"^[A-Za-z]+$");

            #region 註解
            //if (!string.IsNullOrEmpty(View.TAX_ORG_NAME))
            //{
            //    if (!reg4.IsMatch(View.TAX_ORG_NAME))
            //    {
            //        result.message += "姓名/公司名稱中文請以中文填寫。\r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.TAX_ORG_ENAME))
            //{
            //    if (!reg6.IsMatch(View.TAX_ORG_ENAME))
            //    {
            //        result.message += "姓名/公司名稱英文請以英文填寫。\r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.MAIL))
            //{
            //    if (!reg3.IsMatch(View.MAIL))
            //    {
            //        result.message += "請填入正確的Email格式 ! \r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.FAX))
            //{
            //    if (View.FAX.Length>16)
            //    {
            //        result.message += "請輸入正確的傳真號碼 。\r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.DATE_S_AC)&& !string.IsNullOrEmpty(View.DATE_E_AC))
            //{
            //    if (HelperUtil.TransToDateTime(View.DATE_S_AC)> HelperUtil.TransToDateTime(View.DATE_E_AC))
            //    {
            //        result.message += "起始日期不得大於終止日期。\r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.SELL_NAME))
            //{
            //    if (!reg6.IsMatch(View.SELL_NAME))
            //    {
            //        result.message += "賣方英文名稱請以英文填寫 ! \r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.SELL_ADDR))
            //{
            //    if (!reg6.IsMatch(View.SELL_ADDR))
            //    {
            //        result.message += "賣方英文地址請以英文填寫 ! \r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.TAX_ORG_CITY_DETAIL))
            //{
            //    if (!reg4.IsMatch(View.TAX_ORG_CITY_DETAIL))
            //    {
            //        result.message += "聯絡地址中文請以中文填寫。\r\n";
            //    }
            //}

            //if (!string.IsNullOrEmpty(View.TAX_ORG_EADDR))
            //{
            //    if (!reg6.IsMatch(View.TAX_ORG_EADDR))
            //    {
            //        result.message += "聯絡地址英文請以英文填寫。\r\n";
            //    }
            //}
            #endregion

            if (result.message!="")
            {
                result.status = false;
            }

            return Content(result.Serialize(), "application/json");
        }

        [DisplayName("Apply_001034_預覽")]
        [HttpPost]
        public ActionResult PreView(Apply_001034ViewModel model)
        {
            ShareDAO dao = new ShareDAO();
            model.PreView = new Apply_001034ViewModel();
            model.PreView.InjectFrom(model);
            model.APPLY_DATE = HelperUtil.DateTimeToTwString(DateTime.Now);

            return PartialView("PreView001034", model);
        }

        [DisplayName("Apply_001034_完成申報")]
        public ActionResult Save(Apply_001034ViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            ShareDAO shareDao = new ShareDAO();
            var memberName = string.IsNullOrWhiteSpace(model.APPLY_NAME) ? sm.UserInfo.Member.NAME : model.APPLY_NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;
            dao.AppendApply001034(model);
            dao.SendMail_Proc(memberName, memberEmail, model.APP_ID, "危險性醫療儀器進口", "001034");
            //dao.SendMail_New(memberName, memberEmail, model.APP_ID, "危險性醫療儀器進口", "001034");

            return View("Save");
        }

        #region 補件查詢

        /// <summary>
        /// 補件查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001034_補件查詢")]
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001034ViewModel model = new Apply_001034ViewModel();
            model = new Apply_001034ViewModel();
            model.APP_ID = APP_ID;
            model = dao.QueryApply_001034(model);

            if (model.Apply.FLOW_CD == "2")
            {
                #region  案件基本資訊

                ApplyModel apply = new ApplyModel();
                apply.APP_ID = APP_ID;
                var applyData = dao.GetRow(apply);

                try
                {
                    var userInfo = sm.UserInfo.Member;

                    // 判斷是否為該案件申請人
                    if (applyData.ACC_NO == userInfo.ACC_NO)
                    {

                        // 取回補件備註欄位
                        TblAPPLY_NOTICE noticeWhere = new TblAPPLY_NOTICE();
                        noticeWhere.APP_ID = APP_ID;
                        noticeWhere.ISADDYN = "N";
                        var noticeData = dao.GetRowList(noticeWhere);

                        // 無動態欄位
                        var filedList = new List<string>();
                        // 動態欄位(通常適用於檔案)
                        var goodFieldList = new List<string>();

                        foreach (var item in noticeData)
                        {
                            if (item.BATCH_INDEX.TONotNullString() == "")
                            {
                                filedList.Add(item.Field);
                            }
                            else
                            {
                                var field = "";
                                if (item.BATCH_INDEX.TONotNullString() != "")
                                {
                                    field = "Goods_" + (item.BATCH_INDEX - 1).ToString() + "__" + (item.Field.EndsWith("_NAME") ? item.Field.TONotNullString().Replace("_NAME", "") : item.Field);
                                }
                                else
                                {
                                    field = "Goods_" + (item.BATCH_INDEX - 1).ToString() + "__" + item.Field;
                                }
                                goodFieldList.Add(field);
                            }

                        }

                        // 組成字串丟回前端跑JS
                        model.FieldList = string.Join(",", filedList);
                        model.GoodFieldList = string.Join(",", goodFieldList);

                        return View("AppDoc", model);
                    }
                    else
                    {
                        throw new Exception("非案件申請人無法瀏覽次案件 !");
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("Apply_001034_補件查詢__AppDoc failed:" + ex.TONotNullString());
                    return RedirectToAction("Index", "Login");
                }

                #endregion
            }
            else
            {
                return Detail(model.APP_ID);
            }

        }

        #endregion 

        #region 明細查詢

        /// <summary>
        /// 明細查詢
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001034_明細查詢")]
        public ActionResult Detail(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_001034ViewModel model = new Apply_001034ViewModel();
            model = new Apply_001034ViewModel();
            model.APP_ID = APP_ID;
            model = dao.QueryApply_001034(model);

            // 案件基本資訊
            ApplyModel apply = new ApplyModel();
            apply.APP_ID = APP_ID;
            var applyData = dao.GetRow(apply);

            try
            {
                var userInfo = sm.UserInfo.Member;

                // 判斷是否為該案件申請人
                if (applyData.ACC_NO == userInfo.ACC_NO)
                {

                    // 取回補件備註欄位
                    TblAPPLY_NOTICE noticeWhere = new TblAPPLY_NOTICE();
                    noticeWhere.APP_ID = APP_ID;
                    noticeWhere.ISADDYN = "N";
                    var noticeData = dao.GetRowList(noticeWhere);

                    // 無動態欄位
                    var filedList = new List<string>();
                    // 動態欄位(通常適用於檔案)
                    var goodFieldList = new List<string>();

                    foreach (var item in noticeData)
                    {
                        if (item.BATCH_INDEX.TONotNullString() == "")
                        {
                            filedList.Add(item.Field);
                        }
                        else
                        {
                            var field = "";
                            if (item.BATCH_INDEX.TONotNullString() != "")
                            {
                                field = "Goods_" + (item.BATCH_INDEX - 1).ToString() + "__" + (item.Field.EndsWith("_NAME") ? item.Field.TONotNullString().Replace("_NAME", "") : item.Field);
                            }
                            else
                            {
                                field = "Goods_" + (item.BATCH_INDEX - 1).ToString() + "__" + item.Field;
                            }
                            goodFieldList.Add(field);
                        }

                    }

                    // 組成字串丟回前端跑JS
                    model.FieldList = string.Join(",", filedList);
                    model.GoodFieldList = string.Join(",", goodFieldList);

                    return View("Detail001034", model);
                }
                else
                {
                    throw new Exception("非案件申請人無法瀏覽次案件 !");
                }
            }
            catch (Exception ex)
            {
                sm.LastErrorMessage = ex.Message;
                return RedirectToAction("Index", "Login");
            }
        }

        #endregion 

        #region 補件存檔

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [DisplayName("Apply_001034_補件存檔")]
        [HttpPost]
        public ActionResult AppDocSave(Apply_001034ViewModel model)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            dao.UpdateAddtionalDocApply_001034(model);

            TblAPPLY_NOTICE notice = new TblAPPLY_NOTICE();
            notice.APP_ID = model.APP_ID;
            model.AppDocCount = dao.GetRowList(notice).ToList().Count();
            model.STATUS = "A";
            var memberName = string.IsNullOrWhiteSpace(model.APPLY_NAME) ? sm.UserInfo.Member.NAME : model.APPLY_NAME;
            var memberEmail = string.IsNullOrWhiteSpace(model.MAIL) ? sm.UserInfo.Member.MAIL : model.MAIL;
            dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "危險性醫療儀器進口", "001034", model.AppDocCount.ToString());

            return View("Save", model);
        }

        #endregion 
    }
}
