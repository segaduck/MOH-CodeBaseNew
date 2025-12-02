using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using ES.Models.ViewModels;
using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using Omu.ValueInjecter;
using ES.Models;
using System.Net.Mail;
using System.IO;

namespace ES.Controllers
{
    public class Apply_011001Controller : BaseController
    {
        #region 新增申辦案件

        /// <summary>
        /// 空白表單畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Apply()
        {
            SessionModel sm = SessionModel.Get();
            Apply_011001FormModel model = new Apply_011001FormModel();
            ActionResult rtn = View("Index", model);
            model.APP_TIME = DateTime.Now;
            var UsIn = sm.UserInfo.Member;

            if (UsIn != null)
            {
                #region 帶入帳號資訊

                //帳號
                model.ACC_NO = UsIn.ACC_NO;
                //單位名稱
                model.ACC_NAM = UsIn.NAME;
                // 電話
                model.ACC_TEL = UsIn.TEL;
                // 地址
                model.ACC_ADDR_CODE = UsIn.CITY_CD;
                model.ACC_ADDR = UsIn.ADDR;
                //姓名
                model.ADM_NAM = UsIn.CNT_NAME;
                //行動
                model.ADM_MOBILE = UsIn.MOBILE;
                //Mail
                model.ADM_MAIL = UsIn.MAIL;

                //計畫序號
                model.SRV_ID = "011001";
                model.SRC_SRV_ID = "011001";
                #endregion
                model.MERGEYN = "N";
            }

            else rtn = RedirectToAction("Index", "Login");

            return rtn;
        }


        /// <summary>
        /// 導預覽畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Apply(Apply_011001FormModel model)
        {
            // 由登入資訊取得當前角色的檢定類別資訊
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View(model);

            if (model.MERGEYN == "Y")
            {
                foreach (var item in ModelState)
                {
                    if (item.Key.ToLeft(5) == "FILE_")
                    {
                        item.Value.Errors.Clear();
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var errormsg = model.FileSave();
                if (errormsg != "")
                {
                    sm.LastErrorMessage = errormsg;
                }
                else
                {
                    if (model.IsMode == "1")
                    {
                        model.IsNew = false;
                    }
                    if (model.IsMode == "0")
                    {
                        model.IsNew = true;
                    }
                }
            }

            rtn = View("Index", model);
            return rtn;
        }


        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(Apply_011001FormModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.ADM_NAM) ? sm.UserInfo.Member.NAME : model.ADM_NAM;
            var memberEmail = string.IsNullOrWhiteSpace(model.ADM_MAIL) ? sm.UserInfo.Member.MAIL : model.ADM_MAIL;
            ModelState.Clear();
            // 存檔
            var appid = dao.AppendApply011001(model);
            // 寄信
            dao.SendMail_New(memberName, memberEmail, appid, "志願服務計畫核備", "011001", ISSEND: true);

            return Done("1");
        }

        #endregion

        #region 補件

        /// <summary>
        /// 補件畫面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult AppDoc(string APP_ID)
        {
            ApplyDAO dao = new ApplyDAO();
            SessionModel sm = SessionModel.Get();
            Apply_011001DetailModel model = new Apply_011001DetailModel();

            // 案件基本資訊
            APPLY_011001 app = new APPLY_011001();
            app.APP_ID = APP_ID;
            var appdata = dao.GetRow(app);

            ApplyModel aly = new ApplyModel();
            aly.APP_ID = APP_ID;
            var alydata = dao.GetRow(aly);

            try
            {
                var UsIn = sm.UserInfo.Member;

                // 判斷是否為該案件申請人
                if (appdata.ACC_NO == UsIn.ACC_NO)
                {
                    #region 案件內容
                    // 取檔案(可依個人方式決定帶值回來的方式，建議用SQL)
                    model = dao.GetFile011001(APP_ID);
                    // 取回案件資料(可依個人方式決定帶值回來的方式)
                    #region 帶入帳號資訊
                    model.IsNew = false;
                    model.APP_TIME = DateTime.Now;
                    // 申請日期
                    model.ADD_TIME = appdata.ADD_TIME;
                    // 帳號
                    model.ACC_NO = appdata.ACC_NO;
                    // 單位名稱
                    model.ACC_NAM = appdata.ACC_NAM;
                    // 電話
                    model.ACC_TEL = appdata.ACC_TEL;
                    // 地址
                    model.ACC_ADDR_CODE = appdata.ACC_ADDR_CODE;
                    model.ACC_ADDR = appdata.ACC_ADDR;
                    // 姓名
                    model.ADM_NAM = appdata.ADM_NAM;
                    // 行動
                    model.ADM_MOBILE = appdata.ADM_MOBILE;
                    // Mail
                    model.ADM_MAIL = appdata.ADM_MAIL;
                    // 運用單位發文日期
                    model.ACC_SDATE = appdata.ACC_SDATE.HasValue ? appdata.ACC_SDATE.Value.ToString("yyyy/MM/dd") : null;
                    // 運用單位發文字號
                    model.ACC_NUM = appdata.ACC_NUM;
                    model.MERGEYN = appdata.MERGEYN;

                    model.MAILBODY = alydata.MAILBODY;

                    //計畫序號
                    model.SRV_ID = "011001";
                    model.SRC_SRV_ID = "011001";


                    #endregion
                    model.APPSTATUS = alydata.FLOW_CD.TONotNullString() == "2" ? "1" : "0";
                    if (alydata != null && alydata.FLOW_CD == "2")
                    {
                        // 取回補件備註欄位
                        TblAPPLY_NOTICE ntwhere = new TblAPPLY_NOTICE();
                        ntwhere.APP_ID = APP_ID;
                        ntwhere.ISADDYN = "N";
                        var ntdata = dao.GetRowList(ntwhere);
                        //if (ntdata.Where(m=>m.Field_NAME=="全部").ToList().ToCount()>0)
                        //{
                        //    model.IsNew = true;
                        //}
                        // 無動態欄位
                        var ntLst = new List<string>();
                        foreach (var item in ntdata)
                        {
                            ntLst.Add(item.Field);
                        }
                        // 組成字串丟回前端跑JS
                        model.FieldStr = string.Join(",", ntLst);
                    }
                    #endregion
                    return View("Detail", model);
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

        /// <summary>
        /// 補件存檔
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult SaveAppDoc(Apply_011001DetailModel model)
        {
            SessionModel sm = SessionModel.Get();
            ApplyDAO dao = new ApplyDAO();
            var memberName = string.IsNullOrWhiteSpace(model.ADM_NAM) ? sm.UserInfo.Member.NAME : model.ADM_NAM;
            var memberEmail = string.IsNullOrWhiteSpace(model.ADM_MAIL) ? sm.UserInfo.Member.MAIL : model.ADM_MAIL;
            ModelState.Clear();
            // 存檔
            var count = dao.UpdateApply011001(model);
            // 寄信
            dao.SendMail_Update(memberName, memberEmail, model.APP_ID, "志願服務計畫核備", "011001", count, ISSEND: true);

            return Done("2", count);

        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Done(string status, string Count = "")
        {
            SessionModel sm = SessionModel.Get();
            Apply_011001DoneModel model = new Apply_011001DoneModel();
            model.status = status.TONotNullString();
            model.Count = Count.TONotNullString();

            return View("Done", model);
        }

        #endregion 
    }
}
