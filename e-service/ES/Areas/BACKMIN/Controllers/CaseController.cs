using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ES.Areas.Admin.Models;
using ES.Areas.Admin.Action;
using System.Web.Routing;
using ES.Utils;
using Ionic.Zip;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using WebUI.CustomClass;
using Omu.ValueInjecter;
using ES.Services;
using ES.Commons;
using ES.Models;
using ES.DataLayers;
using System.Net.Mail;
using DocumentFormat.OpenXml.Bibliography;

namespace ES.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CaseController : BaseController
    {
        protected override void Initialize(RequestContext rc)
        {
            base.Initialize(rc);
            ViewBag.zTreeExpandNodeId = 5;
        }

        /// <summary>
        /// 案件處理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.tempMessage = TempData["tempMessage"];
            CaseModel model = new CaseModel();

            model.FLOW_CD = "";
            model.OderByCol = "APP_ID";
            model.SortAZ = "DESC";
            model.CLOSE_MK = "N";
            model.UNIT_CD = GetAccountModel().ScopeUnitCode.ToString();

            CaseAction action = new CaseAction();

            ViewBag.List = action.GetAPPLY(model, GetAccountModel());
            // 單位選單
            ViewBag.UnitList = action.GetAccountUnit(model.UNIT_CD);
            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            //ViewBag.FLOW_CDList = action.GetFLOW_CD(this.GetAccount(), true);//權限?
            // 案件項目
            ViewBag.Apply_Item = action.GetApply_Item(model.UNIT_CD);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ViewBag.LIC_CDList = CodeUtils.GetCodeSelectList(conn, "F5_LIC_CD", "", "", true);
                conn.Close();
                conn.Dispose();
            }

            string unitCode = GetAccountModel().UnitCode;
            int scope = GetAccountModel().Scope;

            ViewBag.ShowAdv5 = false;
            if (scope == 1 || unitCode.Equals("7"))
            {
                ViewBag.ShowAdv5 = true;
            }

            this.SetVisitRecord("Case", "Index", "案件處理");

            //暫存查詢條件
            SessionModel sm = SessionModel.Get();
            ES.Models.CaseQryModel caseInfo = new ES.Models.CaseQryModel();

            if (sm.CaseApply == null)
            {
                sm.CaseApply = new CaseQryModel();
                caseInfo.InjectFrom(model);
                sm.CaseApply = caseInfo;
            }
            else
            {
                model.InjectFrom(sm.CaseApply);
                caseInfo.InjectFrom(model);
                sm.CaseApply = caseInfo;
                return Index(model);
            }

            return View(model);
        }

        ///// <summary>
        ///// 案件處理
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult CaseSearch(string service_id, string is_home_page, string flow_cd)
        //{
        //    SessionModel sm = SessionModel.Get();

        //    ViewBag.tempMessage = TempData["tempMessage"];
        //    CaseModel model = new CaseModel();
        //    model.FLOW_CD = flow_cd;
        //    model.OderByCol = "APP_ID";
        //    model.SortAZ = "DESC";
        //    model.CLOSE_MK = "N";
        //    model.Apply_Item = service_id;
        //    model.IS_HOME_PAGE = is_home_page;
        //    model.UNIT_CD = GetAccountModel().ScopeUnitCode.ToString();
        //    model.FLOW_CD_ITEM = flow_cd;
        //    CaseAction action = new CaseAction();

        //    ViewBag.List = action.GetAPPLY(model, GetAccountModel());
        //    // 單位選單
        //    ViewBag.UnitList = action.GetAccountUnit(model.UNIT_CD);

        //    double pageSize = action.GetPageSize();
        //    double totalCount = action.GetTotalCount();

        //    ViewBag.NowPage = model.NowPage;
        //    ViewBag.TotalCount = action.GetTotalCount();
        //    ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

        //    ViewBag.FLOW_CDList = action.GetFLOW_CD(this.GetAccount(), true);//權限?

        //    // 案件項目
        //    ViewBag.Apply_Item = action.GetApply_Item(model.UNIT_CD);

        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        ViewBag.LIC_CDList = CodeUtils.GetCodeSelectList(conn, "F5_LIC_CD", "", "", true);
        //        conn.Close();
        //        conn.Dispose();
        //    }

        //    string unitCode = GetAccountModel().UnitCode;
        //    int scope = GetAccountModel().Scope;

        //    if (scope == 1 || unitCode.Equals("7"))
        //    {
        //        ViewBag.ShowAdv5 = true;
        //    }
        //    else
        //    {
        //        ViewBag.ShowAdv5 = false;
        //    }

        //    ModelState.Clear();

        //    return View("Index", model);
        //}

        [HttpPost]
        public ActionResult Index(CaseModel model)
        {
            CaseAction action = new CaseAction();
            model.APP_TIME_BEGIN = model.APP_TIME_BEGIN_AD.TONotNullString() == "" ? null : HelperUtil.TransToDateTime(model.APP_TIME_BEGIN_AD);
            model.APP_TIME_END = model.APP_TIME_END_AD.TONotNullString() == "" ? null : HelperUtil.TransToDateTime(model.APP_TIME_END_AD);
            ViewBag.List = action.GetAPPLY(model, GetAccountModel());

            double pageSize = action.GetPageSize();
            double totalCount = action.GetTotalCount();

            ViewBag.NowPage = model.NowPage;
            ViewBag.TotalCount = action.GetTotalCount();
            ViewBag.TotalPage = (int)Math.Ceiling(totalCount / pageSize);

            //ViewBag.FLOW_CDList = action.GetFLOW_CD(this.GetAccount(), true);//權限?

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                ViewBag.LIC_CDList = CodeUtils.GetCodeSelectList(conn, "F5_LIC_CD", "", model.LIC_CD, true);
                conn.Close();
                conn.Dispose();
            }

            string unitCode = GetAccountModel().UnitCode;
            int scope = GetAccountModel().Scope;

            if (scope == 1 || unitCode.Equals("7"))
            {
                ViewBag.ShowAdv5 = true;
            }
            else
            {
                ViewBag.ShowAdv5 = false;
            }
            ModelState.Clear();
            // 單位清單
            var unit_cd = GetAccountModel().ScopeUnitCode.ToString();
            // 單位選單
            ViewBag.UnitList = action.GetAccountUnit(unit_cd);
            // 案件項目
            ViewBag.Apply_Item = action.GetApply_Item(unit_cd);

            //暫存查詢條件
            SessionModel sm = SessionModel.Get();
            ES.Models.CaseQryModel caseInfo = new ES.Models.CaseQryModel();
            caseInfo.InjectFrom(model);
            sm.CaseApply = caseInfo;

            return View(model);
        }

        /// <summary>
        /// 內容修改
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit1(string id)
        {
            CaseAction action = new CaseAction();
            CaseQueryModel model = action.GetAPPLY(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit1(CaseQueryModel model)
        {
            CaseAction action = new CaseAction();
            switch (model.ActionType)
            {
                case "SAVE":
                    if (action.UpdateAPPLYEdit1(model))
                    {
                        //Log
                        using (SqlConnection conn = GetConnection())
                        {
                            conn.Open();
                            SqlTransaction tran = conn.BeginTransaction();
                            AccountModel upd_Model = GetAccountModel();
                            UtilityAction log_action = new UtilityAction(conn, tran);
                            UtilityModel log_model = new UtilityModel();
                            log_model.TX_CATE_CD = "5";
                            log_model.TX_TYPE = 5;
                            log_model.TX_DESC = "申辦編號:" + model.APP_ID + " 修改案件內容";
                            log_action.Insert(log_model, upd_Model);
                            tran.Commit();
                            conn.Close();
                            conn.Dispose();
                        }
                        //log end;
                        TempData["tempMessage"] = "申辦編號:" + model.APP_ID + " 修改案件內容";
                    }
                    else
                    {
                        TempData["tempMessage"] = "修改案件內容失敗!";
                    }
                    //等繳費計算確認
                    break;
                case "DISPATCH":
                    if (action.UpdateAPPLYRePatch(model.APP_ID))
                    {
                        //Log
                        using (SqlConnection conn = GetConnection())
                        {
                            conn.Open();
                            SqlTransaction tran = conn.BeginTransaction();
                            AccountModel upd_Model = GetAccountModel();
                            UtilityAction log_action = new UtilityAction(conn, tran);
                            UtilityModel log_model = new UtilityModel();
                            log_model.TX_CATE_CD = "5";
                            log_model.TX_TYPE = 4;
                            log_model.TX_DESC = "申辦編號:" + model.APP_ID + " 退回重新分文";
                            log_action.Insert(log_model, upd_Model);
                            tran.Commit();
                            conn.Close();
                            conn.Dispose();
                        }
                        //log end;
                        TempData["tempMessage"] = "申辦編號:" + model.APP_ID + " 退回重新分文";
                    }
                    else
                    {
                        TempData["tempMessage"] = "重新分文失敗!";
                    }
                    break;
                case "COUNT":
                    Calculatefee c = new Calculatefee();
                    model = c.CalculateFee(model);
                    return View(model);
                    //break;
            }

            return RedirectToAction("Index", "Case");
        }


        [HttpGet]
        public ActionResult Edit2(string id)
        {
            CaseAction action = new CaseAction();
            CaseQueryModel model = action.GetAPPLY(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit2(CaseQueryModel model)
        {

            String recommTex = Request["recommTex"] == null ? "" : Request["recommTex"].ToString();
            String MailSendDate = Request["MailSendDate"] == null ? "" : Request["MailSendDate"].ToString();
            String RegisteredNum = Request["RegisteredNum"] == null ? "" : Request["RegisteredNum"].ToString();

            CaseAction action = new CaseAction();
            String flowName = action.GetFLOWCDNAME(model.APP_ID, model.FLOW_CD);
            Map dataMap = action.GetCONFIRM_DATA(model.APP_ID, model.UNIT_CD.Value.ToString(), model.FLOW_CD);
            String msg = "";
            switch (model.ActionType)
            {
                case "SAVE":
                    if (action.UpdateAPPLYEdit2(model, dataMap))
                    {
                        if (dataMap.GetString("CLOSE_MK").Equals("Y"))
                        {
                            if (model.CASE_BACK_MK)
                            {
                                msg += "案件狀態有選擇退件 退件日期:" + DateTime.Now.ToString("yyyy/MM/dd") + " <br>";
                                if (model.PAY_BACK_MK)
                                {
                                    if (model.PAY_A_PAID > 0)
                                    {
                                        msg += " 案件有退款";
                                    }
                                }
                            }
                        }
                        //Log
                        using (SqlConnection conn = GetConnection())
                        {
                            conn.Open();
                            SqlTransaction tran = conn.BeginTransaction();
                            AccountModel upd_Model = GetAccountModel();
                            UtilityAction log_action = new UtilityAction(conn, tran);
                            UtilityModel log_model = new UtilityModel();
                            log_model.TX_CATE_CD = "5";
                            log_model.TX_TYPE = 6;
                            log_model.TX_DESC = "申辦編號:" + model.APP_ID + " 異動處理進度，修改為:" + flowName;
                            log_action.Insert(log_model, upd_Model);
                            tran.Commit();
                            conn.Close();
                            conn.Dispose();
                        }
                        //log end;
                        TempData["tempMessage"] = msg + "申辦編號:" + model.APP_ID + " 異動處理進度，修改為:" + flowName;
                        String ClientUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/";
                        if (model.FLOW_CD.Equals("12") || model.FLOW_CD.Equals("13"))
                        {
                            DateTime dt = new DateTime();
                            if (!String.IsNullOrEmpty(MailSendDate))
                            {
                                dt = Convert.ToDateTime(MailSendDate);
                                recommTex = "已於" + dt.Month + "月" + dt.Day + "日寄出，掛號條碼：" + RegisteredNum;
                            }
                            else
                            {
                                recommTex = "";
                            }

                            WebUtils.Apply_Mail_Send(model.APP_ID, ClientUrl, recommTex, 3);
                        }
                        else if (model.FLOW_CD.Equals("02"))
                        {
                            if (model.APP_ID.Substring(8, 6).Equals("001008"))
                            {
                                recommTex = recommTex.Replace("1.", "<br>1.");
                                recommTex = recommTex.Replace("2.", "<br>2.");
                                recommTex = recommTex.Replace("3.", "<br>3.");
                                recommTex = recommTex.Replace("4.", "<br>4.");
                            }
                            WebUtils.Apply_Mail_Send(model.APP_ID, ClientUrl, recommTex, 2);
                        }
                        else if (model.FLOW_CD.Equals("09"))
                        {
                            if (recommTex.Length > 1000)
                                recommTex = recommTex.Substring(1, 1000);
                            WebUtils.Apply_Mail_Send(model.APP_ID, ClientUrl, recommTex, 1);
                        }
                        else if (model.FLOW_CD.Equals("20"))
                        {
                            if (recommTex.Length > 1000)
                                recommTex = recommTex.Substring(1, 1000);
                            WebUtils.Apply_Mail_Send(model.APP_ID, ClientUrl, recommTex, 1);
                        }
                        else
                        {
                            WebUtils.Apply_Mail_Send(model.APP_ID, ClientUrl, recommTex, 0);
                        }
                    }
                    else
                    {

                        TempData["tempMessage"] = "重新分文失敗!";
                    }
                    break;
                case "DISPATCH":
                    if (action.UpdateAPPLYRePatch(model.APP_ID))
                    {
                        //Log
                        using (SqlConnection conn = GetConnection())
                        {
                            conn.Open();
                            SqlTransaction tran = conn.BeginTransaction();
                            AccountModel upd_Model = GetAccountModel();
                            UtilityAction log_action = new UtilityAction(conn, tran);
                            UtilityModel log_model = new UtilityModel();
                            log_model.TX_CATE_CD = "5";
                            log_model.TX_TYPE = 4;
                            log_model.TX_DESC = "申辦編號:" + model.APP_ID + " 退回重新分文";
                            log_action.Insert(log_model, upd_Model);
                            tran.Commit();
                            conn.Close();
                            conn.Dispose();
                        }
                        //log end;
                        TempData["tempMessage"] = "申辦編號:" + model.APP_ID + " 退回重新分文";
                    }
                    else
                    {

                        TempData["tempMessage"] = "重新分文失敗!";
                    }
                    break;
                case "D":
                    String path = Request.PhysicalApplicationPath;
                    XMLToDOCUtils xtd = new XMLToDOCUtils();
                    //model.SRV_ID = "001008";
                    //model.APP_ID = "201310300010080006";

                    if (model.SRV_ID.Equals("001037"))
                    {

                        String xml = xtd.InitFileXML(path, model.SRV_ID, model.APP_ID);
                        Map map = xtd.GetAPPLY_001037Data(model.APP_ID);
                        xml = xtd.DoReplaceMark(xml, model.SRV_ID, map);
                        System.IO.Stream ms = xtd.GenerateStreamFromString(xml);
                        return File(ms, "application/msword", "Download.doc");
                    }
                    else if (model.SRV_ID.Equals("001008"))
                    {
                        ZipFile zip = new ZipFile();
                        if (xtd.GetAPPLY_001008Type("ME", model.APP_ID, "1") == 0)
                        {
                            String xml = xtd.InitFileXML(path, model.SRV_ID + "_ME_1", model.APP_ID);
                            List<Map> li = xtd.GetAPPLY_001008ME(model.APP_ID);
                            foreach (Map map in li)
                            {
                                xml = xtd.DoReplaceMark(xml, model.SRV_ID + "_ME_1", map);
                                MemoryStream ms = (MemoryStream)xtd.GenerateStreamFromString(xml);
                                zip.AddEntry(model.APP_ID + "_ME_1.doc", ms.ToArray());
                                ms.Dispose();
                                ms.Close();
                            }
                        }
                        else if (xtd.GetAPPLY_001008Type("ME", model.APP_ID, "2") == 0)
                        {
                            String xml = xtd.InitFileXML(path, model.SRV_ID + "_ME_2", model.APP_ID);
                            List<Map> li = xtd.GetAPPLY_001008ME(model.APP_ID);
                            int i = 1;
                            foreach (Map map in li)
                            {
                                xml = xtd.DoReplaceMark(xml, model.SRV_ID + "_ME_2_" + i, map);
                                i++;
                                if (i == 3)
                                    break;
                            }
                            xml = xml.Replace("$A09$", "");
                            xml = xml.Replace("$A10$", "");
                            xml = xml.Replace("$A11$", "");
                            xml = xml.Replace("$A12$", "");
                            xml = xml.Replace("$A13$", "");
                            xml = xml.Replace("$A14$", "");
                            MemoryStream ms = (MemoryStream)xtd.GenerateStreamFromString(xml);
                            zip.AddEntry(model.APP_ID + "_ME_2.doc", ms.ToArray());
                            ms.Dispose();
                            ms.Close();
                        }
                        if (xtd.GetAPPLY_001008Type("PR", model.APP_ID, "1") == 0)
                        {
                            List<Map> li = xtd.GetAPPLY_001008ME(model.APP_ID);
                            List<Map> li_PR = xtd.GetAPPLY_001008PR(model.APP_ID);
                            foreach (Map map in li)
                            {
                                foreach (Map pr_map in li_PR)
                                {
                                    String xml = xtd.InitFileXML(path, model.SRV_ID + "_PR_1", model.APP_ID);
                                    if ((map.GetString("LIC_TYPE").Equals("F") && !pr_map.GetString("LIC_TYPE").Equals("A0302") && !pr_map.GetString("LIC_TYPE").Equals("A0201"))
                                        || (!map.GetString("LIC_TYPE").Equals("F") && (pr_map.GetString("LIC_TYPE").Equals("A0302") || pr_map.GetString("LIC_TYPE").Equals("A0201"))))
                                    {
                                        //DO NOTHINH.....
                                    }
                                    else
                                    {
                                        xml = xtd.DoReplaceMark2(xml, map, pr_map);
                                        MemoryStream ms = (MemoryStream)xtd.GenerateStreamFromString(xml);
                                        zip.AddEntry(model.APP_ID + "_PR_1.doc", ms.ToArray());
                                        ms.Dispose();
                                        ms.Close();
                                    }

                                }
                            }
                        }
                        else if (xtd.GetAPPLY_001008Type("PR", model.APP_ID, "2") == 0)
                        {
                            List<Map> li = xtd.GetAPPLY_001008ME(model.APP_ID);
                            List<Map> li_PR = xtd.GetAPPLY_001008PR(model.APP_ID);
                            String xml = xtd.InitFileXML(path, model.SRV_ID + "_PR_1", model.APP_ID);
                            foreach (Map map in li)
                            {
                                int i = 1;
                                foreach (Map pr_map in li_PR)
                                {
                                    if ((map.GetString("LIC_TYPE").Equals("F") && !pr_map.GetString("LIC_TYPE").Equals("A0302") && !pr_map.GetString("LIC_TYPE").Equals("A0201"))
                                        || (!map.GetString("LIC_TYPE").Equals("F") && (pr_map.GetString("LIC_TYPE").Equals("A0302") || pr_map.GetString("LIC_TYPE").Equals("A0201"))))
                                    {
                                        //DO NOTHINH.....
                                    }
                                    else
                                    {
                                        xml = xtd.DoReplaceMark3(xml, i, map, pr_map);
                                        i++;
                                        if (i == 2)
                                            break;
                                    }

                                }
                            }
                            xml = xml.Replace("$A14$", "");
                            xml = xml.Replace("$A15$", "");
                            xml = xml.Replace("$A16$", "");
                            xml = xml.Replace("$A17$", "");
                            MemoryStream ms = (MemoryStream)xtd.GenerateStreamFromString(xml);
                            zip.AddEntry(model.APP_ID + "_PR_2.doc", ms.ToArray());
                            ms.Dispose();
                            ms.Close();
                        }
                        //FileStream f = System.IO.File.Create("C:\\Download.zip");
                        MemoryStream ms1 = new MemoryStream();
                        zip.Save(ms1);
                        zip.Dispose();

                        ms1.Seek(0, SeekOrigin.Begin);
                        ms1.Flush();

                        return File(ms1, "application/zip", "Download.zip");
                    }

                    break;
            }

            return RedirectToAction("Index", "Case");
        }

        public ActionResult ExportExcel(CaseModel model)
        {
            try
            {
                DataTable dt = null;
                CaseAction action = new CaseAction();
                dt = action.GetAPPLYEXCEL(model, GetAccountModel());

                MemoryStream ms = ReportUtils.RenderDataTableToExcel(dt) as MemoryStream;
                string FileName = System.DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls";
                return File(ms, "application/unknown", FileName);
            }
            catch
            {
                TempData["tempMessage"] = "匯出資料發生錯誤！";
                return RedirectToAction("Index");
            }

        }

        public ActionResult DocumentView(string applyId, string srvId)
        {

            CaseAction action = new CaseAction();
            ES.Models.LeadModel.DocumentFormat model = action.GetDocumentData(srvId, applyId);
            return View(model);
        }

        /// <summary>
        /// 內容修改
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail(string appid, string srvid)
        {
            CaseDetailAction action = new CaseDetailAction();
            DetailViewModel model_base = action.GetAPPLY(appid);

            switch (srvid)
            {
                #region 醫事司
                case "001037":  //醫事人員請領無懲戒紀錄證明申請書
                    return RedirectToAction("Index", "Apply_001037", new { appid = appid, srvid = srvid });
                case "001038":  //生殖細胞及胚胎輸入輸出申請作業
                    return RedirectToAction("Index", "Apply_001038", new { appid = appid, srvid = srvid });
                case "001035": //非感染性人體器官、組織及細胞進出口申請作業
                    return RedirectToAction("Index", "Apply_001035", new { appid = appid, srvid = srvid });
                case "001034": //危險性醫療儀器進口申請作業
                    return RedirectToAction("Index", "Apply_001034", new { appid = appid, srvid = srvid });
                case "001005": //醫事人員證書補(換)發
                    return RedirectToAction("Index", "Apply_001005", new { appid = appid, srvid = srvid });
                case "001007": //專科醫師證書補(換)發
                    return RedirectToAction("Index", "Apply_001007", new { appid = appid, srvid = srvid });
                case "001008": //醫事人員請領英文證明書
                    return RedirectToAction("Index", "Apply_001008", new { appid = appid, srvid = srvid });
                case "001009": //醫事人員資格英文求證
                    return RedirectToAction("Index", "Apply_001009", new { appid = appid, srvid = srvid });
                case "001039": //醫師赴國外訓練英文保證函
                    return RedirectToAction("Index", "Apply_001039", new { appid = appid, srvid = srvid });
                #endregion 醫事司

                #region 中醫藥司
                case "005001": //產銷證明書
                    return RedirectToAction("Index", "Apply_005001", new { appid = appid, srvid = srvid });
                case "005002": //外銷證明書
                    return RedirectToAction("Index", "Apply_005002", new { appid = appid, srvid = srvid });
                case "005003": //WHO格式之產銷證明書(英文)
                    return RedirectToAction("Index", "Apply_005003", new { appid = appid, srvid = srvid });
                case "005004": //中藥GMP廠證明文件(中文)
                    return RedirectToAction("Index", "Apply_005004", new { appid = appid, srvid = srvid });
                case "005005": //中藥GMP廠證明文件(英文)
                    return RedirectToAction("Index", "Apply_005005", new { appid = appid, srvid = srvid });
                case "005013": //少量自用中藥樣品申請
                    return RedirectToAction("Index", "Apply_005013", new { appid = appid, srvid = srvid });
                case "005014": //貨品進口專案申請
                    return RedirectToAction("Index", "Apply_005014", new { appid = appid, srvid = srvid });
                #endregion 中醫藥司

                #region 社工司
                case "011001": //志願服務計畫核備
                    return RedirectToAction("Index", "Apply_011001", new { appid = appid, srvid = srvid });
                case "社會工作證書核發(中文)": //社會工作證書核發(中文)
                    break;
                case "011004": //社會工作證書核發(英文)
                    return RedirectToAction("Index", "Apply_011004", new { appid = appid, srvid = srvid });
                case "社會工作證書換(補)發": //社會工作證書換(補)發
                    break;
                case "011003": //社會工作實務經驗年資審查
                    return RedirectToAction("Index", "Apply_011003", new { appid = appid, srvid = srvid });
                case "011002": //專科社會工作證書核發
                    return RedirectToAction("Index", "Apply_011002", new { appid = appid, srvid = srvid });
                case "專科社會工作證書換(補)發": //專科社會工作證書換(補)發
                    break;
                case "011005": //專科社會工作證書補發(遺失或污損)
                    return RedirectToAction("Index", "Apply_011005", new { appid = appid, srvid = srvid });
                case "011006": //專科社會工作師證書換發（更名）
                    return RedirectToAction("Index", "Apply_011006", new { appid = appid, srvid = srvid });
                #endregion 社工司

                #region 國健署
                case "010001": // 檔案應用申請
                    return RedirectToAction("Index", "Apply_010001", new { appid = appid, srvid = srvid });
                case "低收入戶及中低收入戶之體外受精": //低收入戶及中低收入戶之體外受精
                    break;
                #endregion 國健署

                #region 其他
                case "檔案應用申請": //檔案應用申請
                    break;
                case "001036": //專科護理師證書補(換)發
                    return RedirectToAction("Index", "Apply_001036", new { appid = appid, srvid = srvid });
                case "007001": //信用卡線上捐款
                    break;
                #endregion 其他

                default:
                    if (srvid.Length == 6)
                    {
                        //長度為6試著找尋可能control
                        string s_ctl = string.Format("Apply_{0}", srvid);
                        return RedirectToAction("Index", s_ctl, new { appid = appid, srvid = srvid });
                    }
                    break;
            }
            return RedirectToAction("Index", "Case");
        }

        /// <summary>
        /// 補件已關閉通知給承辦人
        /// </summary>
        /// <returns></returns>
        public void SendMailOverdue()
        {
            logger.Debug("補件已關閉通知給承辦人");
            var list = new List<string>();
            ShareDAO dao = new ShareDAO();
            list = dao.GetFNoticeAPPID005();
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    var data = item.ToSplit(',');
                    var APP_ID = data[0];
                    var SRV_ID = data[0].Substring(8, 6);
                    var add_time = data[1];
                    var docday = dao.GetFNoticeDays(SRV_ID);
                    var doctype = dao.GetFNoticeDaysType(SRV_ID);
                    var dateline = dao.GetFNoticeDateLine(docday, doctype, Convert.ToDateTime(add_time));
                    var doctypeText = doctype == "10" ? "工作天" : "日曆天";
                    logger.Debug($"APP_ID:{APP_ID}, ADD_TIME:{add_time}, docDay:{docday}, doctype:{doctype}");
                    logger.Debug("dateline:" + dateline);
                    if (dateline == DateTime.Today.AddDays(-1))
                    {
                        // 截止日期 等於 昨天
                        // 寄信通知關閉補件
                        var pro_acc = dao.GetPRO_ACC(APP_ID);
                        var mail = dao.GetPRO_ACCMail(pro_acc);
                        logger.Debug($"寄信通知:{APP_ID}, 收信人:{pro_acc}, 信箱:{mail}");
                        if (string.IsNullOrEmpty(mail))
                        {
                            mail = "預設信箱";
                        }
                        var subject = $"案件編號{APP_ID}，申請人{docday}{doctypeText}內未於前台補件通知";
                        var MailBody = $"申辦案件:案件編號{APP_ID}，申請人{docday}{doctypeText}內未於前台補件，已關閉補件功能。";
                        dao.SendMail_Overdue(subject, MailBody, mail, SRV_ID);

                    }
                }
            }
            logger.Debug("補件已關閉通知寄信完畢。");
        }

        /// <summary>
        /// 退回分文
        /// </summary>
        /// <returns></returns>
        public ActionResult BackAssign(string appid, string srvid)
        {
            try
            {
                Dictionary<string, object> args = new Dictionary<string, object>();
                args.Add("UPD_ACC", GetAccount()); // 分文者帳號
                args.Add("UPD_FUN_CD", "ADM-ASSIGN");
                args.Add("APP_ID", appid);

                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction(); // 開啟交易

                    try
                    {
                        CaseDetailAction action = new CaseDetailAction(conn, tran);
                        action.UpdateBackAssignApply(appid, GetAccount(), tran); // 傳遞交易物件

                        tran.Commit(); // 提交交易
                        logger.Warn("Transaction committed successfully.");
                        TempData["tempMessage"] = "退回分文成功!";
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback(); // 回滾交易
                        logger.Warn("Transaction rolled back due to error.", ex);
                        TempData["tempMessage"] = ex.Message;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["tempMessage"] = ex.Message;
                logger.Warn(ex.Message, ex);
            }

            return RedirectToAction("Index");
        }
    }
}
