using System.IO;
using System.Linq;
using System.Web.Mvc;
using EECOnline.Commons.Filter;
using EECOnline.DataLayers;
using EECOnline.Models;
using EECOnline.Services;
using EECOnline.Commons;
using System.Collections;
using System.Collections.Generic;
using EECOnline.Models.Entities;
using Omu.ValueInjecter;
using System;
using Turbo.Commons;
using HppApi;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using static EECOnline.Utils.Hospital_FarEastern_Code;
using System.Runtime.InteropServices;
using System.Net.Mail;
using Xceed.Words.NET;
using OpenXmlPowerTools;
using System.Drawing.Imaging;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using Turbo.ReportTK.PDF;
using iTextSharp.text.pdf;
using Newtonsoft.Json;

namespace EECOnline.Controllers
{
    /// <summary>
    /// 首頁
    /// </summary>
    public class HomeController : EBaseController
    {
        public ActionResult Index(string result, string transaction_id)
        {
            HomeViewModel model = new HomeViewModel();
            FrontDAO dao = new FrontDAO();
            model.News = new NewsModel();
            model.News.Grid = dao.GetHomeNews().Take(5).ToList();
            //result = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJ3d3cuZ3NwLmdvdi50dyIsInN1YiI6IjdhNzkwMjdmLTUxNDQtNGIxOS05Mzc3LTc5OWZlNzM2YzhjMyIsInJlc3VsdCI6InN1Y2Nlc3MiLCJpYXQiOjE3MjAxNDM0MDksImNrRmllbGRzIjpbIkNoZWNrTmhpU2VyaWFsIl19.qqBmCVcuaAuFY2pvV8sAMeXNsAq03CAWrXuQ6zK0NzKjH2dzw_bE5qGkUGGCDPrmZHaf3te1IgbYCv3BBLVI0LXQ3wkD6w5l0fUXll1XmZ27oP0O3zfFJpb5Dalw03FGO3XcJ7CCwUEudRVMJieLcZyIFlhvqLZRuB_smjfWIzPBkhGyguSK14Hk_zq9W4c_x7KpNyRAENkAjvczXo00GBZ-Z27RcGnFnOUHODv5CnVYyRK6FWeeDnmadXQ-XVLiDSwdDHH5T91bjQMUJEPcF8-Nx1FPafzr3h0maCZvWBbT1eEmCrYI1rpVCCG6AHAypQrzpMViH9ZYAdMw7S3XnQ";
            if (result.TONotNullString() != "")
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(result);
                var claims = token.Claims;

                if (claims.ToCount() > 0)
                {
                    var uuid = claims.Where(m => m.Type == "sub").ToList().FirstOrDefault().Value;
                    var resultR = claims.Where(m => m.Type == "result").ToList().FirstOrDefault().Value;
                    if (resultR.Contains("success"))
                    {
                        //FrontDAO dao = new FrontDAO();
                        // 如果該筆UUID已經驗證過，就排除該次驗證後續執行
                        EEC_NHICardVerify nhiCheck = new EEC_NHICardVerify();
                        nhiCheck.UUID = uuid;
                        var nhiList = dao.GetRowList(nhiCheck).Where(m => m.result.TONotNullString() == "").ToList();
                        if (nhiList.ToCount() > 0)
                        {

                            // 將健保資訊回來的資訊儲存起來
                            EEC_NHICardVerify nhiWhere = new EEC_NHICardVerify();
                            nhiWhere.UUID = uuid;
                            EEC_NHICardVerify nhi = new EEC_NHICardVerify();
                            nhi.result = resultR;
                            nhi.UPDATADATE = DateTime.Now;
                            dao.Update(nhi, nhiWhere);

                            var findUsers = dao.GetRowList(new EEC_NHICardVerify() { UUID = uuid.TONotNullString() });
                            if (findUsers.ToCount() > 0)
                            {
                                var findUser = findUsers.FirstOrDefault();
                                // 判斷fType = 1，跳申請
                                if (nhiList.FirstOrDefault().fType == "1")
                                {
                                    HomeViewModel NewModel = new HomeViewModel();
                                    NewModel.Login = new LoginModel();
                                    NewModel.Login.user_name = findUser.USERNAME;
                                    NewModel.Login.user_idno2 = findUser.IDNO;
                                    NewModel.Login.user_birthday2 = findUser.BIRTHDAY;
                                    NewModel.Login.user_email3 = findUser.EMAIL;

                                    // LOG
                                    FrontDAO.FrontLOG(findUser.IDNO, findUser.USERNAME, FrontDAO.em_lType.Login3, FrontDAO.em_lStatus.LoginSuccess,
                                        HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");

                                    return LoginForm3return(NewModel);
                                }
                                if (nhiList.FirstOrDefault().fType == "2")
                                {
                                    HomeViewModel NewModel = new HomeViewModel();
                                    NewModel.Search = new SearchModel();
                                    NewModel.Search.user_idno2 = findUser.IDNO;
                                    NewModel.Search.user_name = findUser.USERNAME;

                                    // LOG
                                    FrontDAO.FrontLOG(findUser.IDNO, findUser.USERNAME, FrontDAO.em_lType.Search3, FrontDAO.em_lStatus.LoginSuccess,
                                        HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");

                                    return SearchLoginForm3return(NewModel);
                                }
                            }
                        }
                    }
                }
                return RedirectToAction("Index", "Home");
            }
            // TWFIDO
            else if (transaction_id.TONotNullString() != "")
            {

                var uuid = transaction_id;

                //FrontDAO dao = new FrontDAO();
                // 如果該筆UUID已經驗證過，就排除該次驗證後續執行
                EEC_SPAPIWEB01 nhiCheck = new EEC_SPAPIWEB01();
                nhiCheck.transaction_id = uuid;
                var nhiList = dao.GetRowList(nhiCheck).ToList();
                if (nhiList.ToCount() > 0)
                {
                    var findUsers = nhiList;
                    if (findUsers.ToCount() > 0)
                    {
                        var findUser = findUsers.FirstOrDefault();
                        // 判斷fType = 1，跳申請

                        // 將健保資訊回來的資訊儲存起來
                        EEC_SPAPIWEB01 nhiWhere = new EEC_SPAPIWEB01();
                        nhiWhere.transaction_id = uuid;
                        EEC_SPAPIWEB01 nhi = new EEC_SPAPIWEB01();
                        nhi.UPDATADATE = DateTime.Now;
                        dao.Update(nhi, nhiWhere);

                        if (nhiList.FirstOrDefault().fType == "1")
                        {
                            HomeViewModel NewModel = new HomeViewModel();
                            NewModel.Login = new LoginModel();
                            NewModel.Login.user_name = findUser.IDNO;
                            NewModel.Login.user_idno1 = findUser.IDNO;
                            NewModel.Login.user_birthday1 = findUser.BIRTHDAY;

                            // LOG
                            FrontDAO.FrontLOG(findUser.IDNO, findUser.IDNO, FrontDAO.em_lType.Login2, FrontDAO.em_lStatus.LoginSuccess,
                                HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");

                            return LoginForm2return(NewModel);
                        }
                        if (nhiList.FirstOrDefault().fType == "2")
                        {
                            HomeViewModel NewModel = new HomeViewModel();
                            NewModel.Search = new SearchModel();
                            NewModel.Search.user_idno1 = findUser.IDNO;
                            NewModel.Search.user_name = findUser.IDNO;

                            // LOG
                            FrontDAO.FrontLOG(findUser.IDNO, findUser.IDNO, FrontDAO.em_lType.Search2, FrontDAO.em_lStatus.LoginSuccess,
                                HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");

                            return SearchLoginForm2return(NewModel);
                        }


                    }
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Index", model);
            }
        }

        [HttpGet]
        public ActionResult News()
        {
            NewsModel model = new NewsModel();
            return News(model);
        }

        public ActionResult News(NewsModel model)
        {
            FrontDAO dao = new FrontDAO();
            ActionResult rtn = View("News", model);
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                // 設定查詢分頁資訊
                dao.SetPageInfo(model.rid, model.p);
                // 查詢結果
                model.Grid = dao.GetHomeNews();
                // 有 result id 資訊, 分頁連結, 返回 GridRows Partial View 
                if (!string.IsNullOrEmpty(model.rid) && model.useCache == 0) rtn = PartialView("_GridRows_News", model);
                // 設定分頁元件(_PagingLink partial view)所需的資訊
                base.SetPagingParams(model, dao, "News");
            }
            return rtn;
        }

        public ActionResult NewsDetail(string NewsID)
        {
            SessionModel sm = SessionModel.Get();
            FrontDAO dao = new FrontDAO();
            var TheNewsID = -1;
            int.TryParse(NewsID, out TheNewsID);
            var findData = dao.GetRow(new TblENEWS() { enews_id = TheNewsID });
            if (findData == null)
            {
                sm.LastErrorMessage = "查無資料";
                return Redirect(Url.Action("News", "Home"));
            }
            else
            {
                NewsDetailModel model = new NewsDetailModel();
                model.InjectFrom(findData);
                model.Files = dao.GetRowList(new TblEFILE()
                {
                    peky1 = "ENEWS",
                    peky2 = model.enews_id.TONotNullString()
                });
                return View("NewsDetail", model);
            }
        }

        public ActionResult Login(string transaction_id)
        { // TWFIDO
            if (transaction_id.TONotNullString() != "")
            {
                FrontDAO dao = new FrontDAO();
                var uuid = transaction_id;

                //FrontDAO dao = new FrontDAO();
                // 如果該筆UUID已經驗證過，就排除該次驗證後續執行
                EEC_SPAPIWEB01 nhiCheck = new EEC_SPAPIWEB01();
                nhiCheck.transaction_id = uuid;
                var nhiList = dao.GetRowList(nhiCheck).ToList();
                if (nhiList.ToCount() > 0)
                {
                    var findUsers = nhiList;
                    if (findUsers.ToCount() > 0)
                    {
                        var findUser = findUsers.FirstOrDefault();
                        // 判斷fType = 1，跳申請

                        // 將健保資訊回來的資訊儲存起來
                        EEC_SPAPIWEB01 nhiWhere = new EEC_SPAPIWEB01();
                        nhiWhere.transaction_id = uuid;
                        EEC_SPAPIWEB01 nhi = new EEC_SPAPIWEB01();
                        nhi.UPDATADATE = DateTime.Now;
                        dao.Update(nhi, nhiWhere);

                        if (nhiList.FirstOrDefault().fType == "1")
                        {
                            HomeViewModel NewModel = new HomeViewModel();
                            NewModel.Login = new LoginModel();
                            NewModel.Login.user_name = findUser.IDNO;
                            NewModel.Login.user_idno1 = findUser.IDNO;
                            NewModel.Login.user_birthday1 = findUser.BIRTHDAY;

                            // LOG
                            FrontDAO.FrontLOG(findUser.IDNO, findUser.IDNO, FrontDAO.em_lType.Login2, FrontDAO.em_lStatus.LoginSuccess,
                                HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");

                            return LoginForm2return(NewModel);
                        }
                        if (nhiList.FirstOrDefault().fType == "2")
                        {
                            HomeViewModel NewModel = new HomeViewModel();
                            NewModel.Search = new SearchModel();
                            NewModel.Search.user_idno1 = findUser.IDNO;
                            NewModel.Search.user_name = findUser.IDNO;

                            // LOG
                            FrontDAO.FrontLOG(findUser.IDNO, findUser.IDNO, FrontDAO.em_lType.Search2, FrontDAO.em_lStatus.LoginSuccess,
                                HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");

                            return SearchLoginForm2return(NewModel);
                        }


                    }
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                HomeViewModel model = new HomeViewModel();
                model.Login = new LoginModel();
                model.ProcessStep = "1";
                model.UserLoginTab = "1";
                return View("Login", model);
            }

        }

        [HttpPost]
        public ActionResult LoginForm1(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "1";
            ModelState.Clear();
            // LOG
            FrontDAO.FrontLOG(model.Login.user_idno, model.Login.user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginTry,
                HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");
            // 以防有人打小寫字母，導致 API 傳到醫院時找不到病歷 - 2025.1.14
            model.Login.user_idno = model.Login.user_idno.TONotNullString().ToUpper();
            // 檢查
            #region 檢查
            if (model.Login.user_pincode.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 自然人憑證PIN碼!";
                FrontDAO.FrontLOG(model.Login.user_idno, model.Login.user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");
                return View("Login", model);
            }
            if (model.Login.user_name.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 姓名!";
                FrontDAO.FrontLOG(model.Login.user_idno, model.Login.user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");
                return View("Login", model);
            }
            if (model.Login.user_idno.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 身分證字號!";
                FrontDAO.FrontLOG(model.Login.user_idno, model.Login.user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");
                return View("Login", model);
            }
            if (model.Login.certData_subjectID.TONotNullString() == ""
             || model.Login.user_idno.Length != 10
             || model.Login.user_idno.ToRight(4) != model.Login.certData_subjectID)
            {
                sm.LastErrorMessage = "身分證字號與自然人憑證不符，請確認!";
                FrontDAO.FrontLOG(model.Login.user_idno, model.Login.user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");
                return View("Login", model);
            }
            if (model.Login.user_birthday.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 出生年月日!";
                FrontDAO.FrontLOG(model.Login.user_idno, model.Login.user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");
                return View("Login", model);
            }
            // 移到 LoginForm1_CheckValidateCode() 去做
            //if (model.Login.ValidateCode1.TONotNullString() != sm.LoginValidateCode1)
            //{
            //    sm.LastErrorMessage = "驗證碼錯誤!";
            //    return View("Login", model);
            //}
            if (model.Login.user_email1.TONotNullString() == "" || !CommonsServices.CheckEMail(model.Login.user_email1.TONotNullString()))
            {
                sm.LastErrorMessage = "電子郵件Email格式錯誤!";
                FrontDAO.FrontLOG(model.Login.user_idno, model.Login.user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");
                return View("Login", model);
            }
            FrontDAO dao = new FrontDAO();
            TblEEC_User euserWhere = new TblEEC_User();
            //euserWhere.user_pincode = model.Login.user_pincode;
            euserWhere.user_idno = model.Login.user_idno;
            TblEEC_User euser = new TblEEC_User();
            euser.user_name = model.Login.user_name;
            euser.user_idno = model.Login.user_idno;
            euser.user_birthday = Convert.ToDateTime(model.Login.user_birthday).ToString("yyyyMMdd");
            euser.user_pincode = model.Login.user_pincode;
            euser.user_email = model.Login.user_email1;
            dao.InsertOrUpdate(euser, euserWhere);

            var findUser = dao.GetRowList(new TblEEC_User() { user_idno = model.Login.user_idno.TONotNullString() });
            //    if (findUser.ToCount() != 1)
            //    {


            //        //sm.LastErrorMessage = "自然人憑證PIN碼 錯誤！登入失敗！";
            //        //return View("Login", model);
            //    }
            #endregion
            // OK
            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "1" };
            NewModel.LoginApply = new LoginApplyModel();
            NewModel.LoginApply.login_type = "1";
            this.ProcessStepNew(findUser.FirstOrDefault(), ref NewModel);  // 現在這邊一律都當 新申請

            // LOG
            FrontDAO.FrontLOG(findUser.FirstOrDefault().user_idno, findUser.FirstOrDefault().user_name, FrontDAO.em_lType.Login1, FrontDAO.em_lStatus.LoginSuccess,
                HttpContext.Request.UserHostAddress, "Home/LoginForm1", "自然人憑證登入");

            return View("Login", NewModel);
        }

        [HttpPost]
        public ActionResult LoginForm1_CheckValidateCode(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            if (model.Login.ValidateCode1.TONotNullString() != sm.LoginValidateCode1)
            {
                return Content("驗證碼錯誤！");
            }
            return Content("");
        }

        [HttpPost]
        public ActionResult LoginForm2(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "2";
            ModelState.Clear();
            // LOG
            FrontDAO.FrontLOG(model.Login.user_idno1, "", FrontDAO.em_lType.Login2, FrontDAO.em_lStatus.LoginTry,
                HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");
            // 以防有人打小寫字母，導致 API 傳到醫院時找不到病歷 - 2025.1.14
            model.Login.user_idno1 = model.Login.user_idno1.TONotNullString().ToUpper();
            // 檢查
            #region 檢查
            if (model.Login.user_name.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 姓名";
                FrontDAO.FrontLOG(model.Login.user_idno1, model.Login.user_name, FrontDAO.em_lType.Login2, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");
                return View("Login", model);
            }
            if (model.Login.user_idno1.TONotNullString() == "" || model.Login.user_birthday1.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 身分證字號 及 出生年月日";
                FrontDAO.FrontLOG(model.Login.user_idno1, model.Login.user_name, FrontDAO.em_lType.Login2, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");
                return View("Login", model);
            }

            FrontDAO dao = new FrontDAO();
            var findUser = dao.GetRowList(new TblEEC_User()
            {
                user_idno = model.Login.user_idno1.TONotNullString(),
                user_birthday = model.Login.user_birthday1.TONotNullString(),
            });
            if (findUser.ToCount() != 1)
            {
                sm.LastErrorMessage = "身分證字號 及 出生年月日 錯誤！登入失敗！";
                FrontDAO.FrontLOG(model.Login.user_idno1, model.Login.user_name, FrontDAO.em_lType.Login2, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");
                return View("Login", model);
            }
            if (findUser.ToCount() == 1 && findUser.FirstOrDefault().user_email != model.Login.user_email2)
            {
                sm.LastErrorMessage = "電子郵件Email 錯誤！登入失敗！";
                FrontDAO.FrontLOG(model.Login.user_idno1, model.Login.user_name, FrontDAO.em_lType.Login2, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm2", "行動自然人憑證登入(TW FidO)");
                return View("Login", model);
            }
            #endregion
            // OK
            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "2" };
            NewModel.LoginApply = new LoginApplyModel();
            NewModel.LoginApply.login_type = "2";
            this.ProcessStepNew(findUser.FirstOrDefault(), ref NewModel);  // 現在這邊一律都當 新申請
            return View("Login", NewModel);
        }

        [HttpPost]
        public ActionResult LoginForm2return(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "2";
            ModelState.Clear();
            // OK

            FrontDAO dao = new FrontDAO();
            TblEEC_User euserWhere = new TblEEC_User();
            euserWhere.user_idno = model.Login.user_idno1;
            TblEEC_User euser = new TblEEC_User();
            euser.user_name = model.Login.user_name;
            euser.user_idno = model.Login.user_idno1;
            euser.user_birthday = model.Login.user_birthday1;
            euser.user_email = model.Login.user_email2;
            dao.InsertOrUpdate(euser, euserWhere);


            var findUser = dao.GetRowList(new TblEEC_User() { user_idno = model.Login.user_idno1.TONotNullString() });

            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "3" };
            NewModel.LoginApply = new LoginApplyModel();
            NewModel.LoginApply.login_type = "2";
            this.ProcessStepNew(findUser.FirstOrDefault(), ref NewModel);  // 現在這邊一律都當 新申請
            return View("Login", NewModel);
        }

        [HttpPost]
        public ActionResult SearchLoginForm2return(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "2";
            ModelState.Clear();
            // OK

            FrontDAO dao = new FrontDAO();
            TblEEC_User euserWhere = new TblEEC_User();
            euserWhere.user_idno = model.Search.user_idno1;
            var euserList = dao.GetRowList(euserWhere);
            if (euserList.ToCount() == 0)
            {
                TblEEC_User euser = new TblEEC_User();
                euser.user_name = model.Search.user_name;
                euser.user_idno = model.Search.user_idno1;
                dao.InsertOrUpdate(euser, euserWhere);
            }



            var findUser = dao.GetRowList(new TblEEC_User() { user_idno = model.Search.user_idno1.TONotNullString() });

            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "2" };
            this.ProcessStepSearch(findUser.FirstOrDefault(), ref NewModel);  // 現在這邊一律都當 新申請
            return View("Search", NewModel);
        }

        [HttpPost]
        public ActionResult LoginForm3(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "3";
            ModelState.Clear();
            // LOG
            FrontDAO.FrontLOG(model.Login.user_idno2, "", FrontDAO.em_lType.Login3, FrontDAO.em_lStatus.LoginTry,
                HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");
            // 以防有人打小寫字母，導致 API 傳到醫院時找不到病歷 - 2025.1.14
            model.Login.user_idno2 = model.Login.user_idno2.TONotNullString().ToUpper();
            // 檢查
            #region 檢查
            if (model.Login.user_name.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 姓名!";
                FrontDAO.FrontLOG(model.Login.user_idno2, model.Login.user_name, FrontDAO.em_lType.Login3, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");
                return View("Login", model);
            }
            if (model.Login.user_idno2.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 身分證字號!";
                FrontDAO.FrontLOG(model.Login.user_idno2, model.Login.user_name, FrontDAO.em_lType.Login3, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");
                return View("Login", model);
            }
            if (model.Login.user_birthday2.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 出生年月日!";
                FrontDAO.FrontLOG(model.Login.user_idno2, model.Login.user_name, FrontDAO.em_lType.Login3, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");
                return View("Login", model);
            }
            if (model.Login.ValidateCode3.TONotNullString() != sm.LoginValidateCode3)
            {
                sm.LastErrorMessage = "驗證碼錯誤!";
                FrontDAO.FrontLOG(model.Login.user_idno2, model.Login.user_name, FrontDAO.em_lType.Login3, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");
                return View("Login", model);
            }
            if (model.Login.user_email3.TONotNullString() == "" || !CommonsServices.CheckEMail(model.Login.user_email3.TONotNullString()))
            {
                sm.LastErrorMessage = "電子郵件Email格式錯誤!";
                FrontDAO.FrontLOG(model.Login.user_idno2, model.Login.user_name, FrontDAO.em_lType.Login3, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/LoginForm3", "身分證字號＋健保卡");
                return View("Login", model);
            }
            #endregion
            // OK

            #region 健保卡加密

            Guid newGuid = Guid.NewGuid();
            var UUID = newGuid.ToString();
            string input = model.Login.user_idno2.TONotNullString() + ";" + UUID;
            byte[] asciiBytes = Encoding.ASCII.GetBytes(input);

            // Load the public key from .cer file
            string appDataFolderPath = Server.MapPath("~/APP_DATA");
            string certificateFilePath = Path.Combine(appDataFolderPath, "NDC.cer");
            //string certificateFilePath = "F:\\3.Work\\MOH_eu-service\\SA\\112\\健保卡網路註冊服務\\健保卡網路註冊服務_20230823\\NDC.cer"; // Replace with actual file path
            X509Certificate2 certificate = new X509Certificate2(certificateFilePath);

            // Get the RSA public key from the certificate
            RSACryptoServiceProvider rsaPublicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            // Encrypt using RSA
            byte[] encryptedData = RSAEncrypt(asciiBytes, rsaPublicKey.ExportParameters(false), false);
            //byte[] encryptedBytes = rsaPublicKey.Encrypt(asciiBytes, true);

            // Convert to Base64
            string encryptedBase64 = Convert.ToBase64String(encryptedData);

            // URL encode the Base64 string
            string encryptedUrlEncoded = HttpUtility.UrlEncode(encryptedBase64);

            var urlTest = "https://eecapply.mohw.gov.tw/";
            // URL encode the Base64 string
            string encryptedUrlEncodedUrl = HttpUtility.UrlEncode(urlTest);

            string postUrl = "https://www.cp.gov.tw/portal/PIIMVerify.aspx?checkFields=8&successUrl=" + encryptedUrlEncodedUrl + "&toVerify=" + encryptedUrlEncoded;
            #endregion

            FrontDAO dao = new FrontDAO();

            #region 將健保資訊儲存起來
            EEC_NHICardVerify nhi = new EEC_NHICardVerify();
            nhi.successURL = encryptedUrlEncodedUrl;
            nhi.toVerify = encryptedUrlEncoded;
            nhi.IDNO = model.Login.user_idno2;
            nhi.USERNAME = model.Login.user_name;
            nhi.BIRTHDAY = Convert.ToDateTime(model.Login.user_birthday2).ToString("yyyyMMdd");
            nhi.EMAIL = model.Login.user_email3;
            nhi.UUID = UUID;
            nhi.CREATEDATE = DateTime.Now;
            nhi.fType = "1";
            dao.Insert(nhi);
            #endregion

            //HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "3" };
            //NewModel.LoginApply = new LoginApplyModel();
            //NewModel.LoginApply.login_type = "3";
            //this.ProcessStepNew(findUser.FirstOrDefault(), ref NewModel);  // 現在這邊一律都當 新申請
            return Redirect(postUrl);
        }

        #region 健保卡加密


        //The key size to use maybe 1024/2048
        private const int _EncryptionKeySize = 2048;

        // The buffer size to decrypt per set
        private const int _DecryptionBufferSize = (_EncryptionKeySize / 8);

        //The buffer size to encrypt per set
        private const int _EncryptionBufferSize = _DecryptionBufferSize - 11;

        static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                //byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    ////Encrypt the passed byte array and specify OAEP padding.  
                    ////OAEP padding is only available on Microsoft Windows XP or
                    ////later.  
                    //encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                    //2012/10/19 rm 改用block
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[_EncryptionBufferSize];
                        int pos = 0;
                        int copyLength = buffer.Length;
                        while (true)
                        {
                            //Check if the bytes left to read is smaller than the buffer size, 
                            //then limit the buffer size to the number of bytes left

                            if (pos + copyLength > DataToEncrypt.Length)

                                copyLength = DataToEncrypt.Length - pos;

                            //Create a new buffer that has the correct size

                            buffer = new byte[copyLength];

                            //Copy as many bytes as the algorithm can handle at a time, 
                            //iterate until the whole input array is encoded

                            Array.Copy(DataToEncrypt, pos, buffer, 0, copyLength);

                            //Start from here in next iteration

                            pos += copyLength;

                            //Encrypt the data using the public key and add it to the memory buffer

                            //_DecryptionBufferSize is the size of the encrypted data

                            ms.Write(RSA.Encrypt(buffer, false), 0, _DecryptionBufferSize);

                            //Clear the content of the buffer, 
                            //otherwise we could end up copying the same data during the last iteration

                            Array.Clear(buffer, 0, copyLength);

                            //Check if we have reached the end, then exit

                            if (pos >= DataToEncrypt.Length)

                                break;
                        }
                        return ms.ToArray();
                    }
                }
                //return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }
        #endregion

        [HttpPost]
        public ActionResult LoginForm3return(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "3";
            ModelState.Clear();
            // OK

            FrontDAO dao = new FrontDAO();
            TblEEC_User euserWhere = new TblEEC_User();
            euserWhere.user_idno = model.Login.user_idno2;
            TblEEC_User euser = new TblEEC_User();
            euser.user_name = model.Login.user_name;
            euser.user_idno = model.Login.user_idno2;
            euser.user_birthday = model.Login.user_birthday2;
            euser.user_email = model.Login.user_email3;
            dao.InsertOrUpdate(euser, euserWhere);


            var findUser = dao.GetRowList(new TblEEC_User() { user_idno = model.Login.user_idno2.TONotNullString() });

            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "3" };
            NewModel.LoginApply = new LoginApplyModel();
            NewModel.LoginApply.login_type = "3";
            this.ProcessStepNew(findUser.FirstOrDefault(), ref NewModel);  // 現在這邊一律都當 新申請
            return View("Login", NewModel);
        }

        /// <summary>
        /// 現在 Home 這邊一律都當 新申請
        /// </summary>
        /// <param name="findUser"></param>
        /// <param name="model"></param>
        private void ProcessStepNew(TblEEC_User findUser, ref HomeViewModel model)
        {
            model.ProcessStep = "2";
            model.LoginApply.InjectFrom(findUser);
            var tmpDT = DateTime.Now;  // 把建立日跟案號的日期統一
            model.LoginApply.createdatetime = tmpDT.ToString("yyyy/MM/dd HH:mm:ss");
            model.LoginApply.apply_no = tmpDT.ToString("yyyyMMddHHmmssfff");
            model.LoginApply.keyid = null;
            // 以防有人打小寫字母，導致 API 傳到醫院時找不到病歷 - 2025.1.14
            model.LoginApply.user_idno = model.LoginApply.user_idno.TONotNullString().ToUpper();
        }

        private string GetApplyDetailNewSeqno(HomeViewModel model)
        {
            var tmpObj = model.LoginApply.ApplyDetail.OrderByDescending(x => x.apply_no_sub).FirstOrDefault();
            if (tmpObj == null) return "001";
            int tmpSeq = tmpObj.apply_no_sub.ToRight(3).TOInt32() + 1;
            return tmpSeq.ToString().PadLeft(3, '0');
        }

        /// <summary>
        /// 新增一筆 ApplyDetail
        /// </summary>
        [HttpPost]
        public ActionResult New_GridRows_ApplyDetail(HomeViewModel model)
        {
            ModelState.Clear();
            if (model.LoginApply.ApplyDetail == null) model.LoginApply.ApplyDetail = new List<LoginApplyDetailModel>();
            model.LoginApply.ApplyDetail.Add(new LoginApplyDetailModel()
            {
                apply_no = model.LoginApply.apply_no,
                apply_no_sub =
                    model.LoginApply.apply_no +
                    model.LoginApply.user_idno.ToRight(9) +
                    this.GetApplyDetailNewSeqno(model),
                user_idno = model.LoginApply.user_idno,
                user_birthday = model.LoginApply.user_birthday,
                pay_deadline =
                    DateTime.ParseExact(model.LoginApply.createdatetime, "yyyy/MM/dd HH:mm:ss", null)
                    .AddDays(3)
                    .ToString("yyyy/MM/dd HH:mm:ss"),
                payed = "N"
            });
            return PartialView("_GridRows_ApplyDetail", model);
        }

        /// <summary>
        /// 刪除一筆 ApplyDetail
        /// </summary>
        [HttpPost]
        public ActionResult Del_GridRows_ApplyDetail(HomeViewModel model)
        {
            ModelState.Clear();
            int tmpDelIdx = 0;
            if (int.TryParse(model.LoginApply.ApplyDetail_DelIdx.TONotNullString(), out tmpDelIdx))
            {
                model.LoginApply.ApplyDetail.RemoveAt(tmpDelIdx);
            }
            return PartialView("_GridRows_ApplyDetail", model);
        }

        /// <summary>
        /// 重新整理 ApplyDetail <br />
        /// 主要是刷新畫面右側，金額統計的顯示部份
        /// </summary>
        [HttpPost]
        public ActionResult Refresh_GridRows_ApplyDetail(HomeViewModel model)
        {
            ModelState.Clear();
            FrontDAO dao = new FrontDAO();
            ShareCodeListModel sclm = new ShareCodeListModel();
            try
            {
                if (model.LoginApply != null && model.LoginApply.ApplyDetail.ToCount() > 0)
                {
                    foreach (var row in model.LoginApply.ApplyDetail)
                    {
                        row.ApplyDetailPrice = new List<LoginApplyDetailPriceModel>();
                        row.hospital_name = "";
                        if (row.hospital_code.TONotNullString() != "")
                            row.hospital_name = sclm.Get_Hospital_list().Where(x => x.Value == row.hospital_code).FirstOrDefault().Text;
                        var tmpTypes = row.his_types.TONotNullString().Split(',').ToList();
                        if (tmpTypes.Any())
                        {
                            foreach (var theType in tmpTypes)
                            {
                                if (theType.TONotNullString() == "") continue;
                                var findHisType = row.HisTypes_List.Where(x => x.his_type == theType).FirstOrDefault();
                                if (findHisType == null) continue;
                                var newItem = new LoginApplyDetailPriceModel()
                                {
                                    apply_no = row.apply_no,
                                    apply_no_sub = row.apply_no_sub,
                                    user_idno = row.user_idno,
                                    hospital_code = row.hospital_code,
                                    hospital_name = row.hospital_name,
                                    his_type = theType,
                                    his_type_name = findHisType.his_type_name,
                                    price = findHisType.price,
                                    pay_deadline = row.pay_deadline,
                                    payed = "N",
                                    ec_date = findHisType.ec_date,
                                    ec_dateText = findHisType.ec_dateText,
                                    ec_note = findHisType.ec_note,
                                    ec_dept = findHisType.ec_dept,
                                    ec_doctor = findHisType.ec_doctor,
                                    ec_docType = findHisType.ec_docType,
                                    ec_system = findHisType.ec_system,
                                };
                                newItem.ApiData.InjectFrom(findHisType.ApiQueryIndexData);
                                newItem.ApiData.master_keyid = null;
                                newItem.ApiData.keyid = null;
                                row.ApplyDetailPrice.Add(newItem);
                            }
                        }
                        if (row.ApplyDetailPrice.ToCount() > 0)
                        {
                            // 固定項目
                            var commonTypes = dao.Get_EEC_CommonType();
                            foreach (var cType in commonTypes)
                            {
                                row.ApplyDetailPrice.Add(new LoginApplyDetailPriceModel()
                                {
                                    apply_no = row.apply_no,
                                    apply_no_sub = row.apply_no_sub,
                                    user_idno = row.user_idno,
                                    hospital_code = row.hospital_code,
                                    hospital_name = row.hospital_name,
                                    his_type = "",
                                    his_type_name = cType["type_name"].ToString(),
                                    price = cType["type_price"].TOInt32(),
                                    pay_deadline = row.pay_deadline,
                                    payed = "N",
                                });
                            }
                            #region 舊版 固定寫法 (已不使用)
                            /*
                            row.ApplyDetailPrice.Add(new LoginApplyDetailPriceModel()
                            {
                                apply_no = row.apply_no,
                                apply_no_sub = row.apply_no_sub,
                                user_idno = row.user_idno,
                                hospital_code = row.hospital_code,
                                hospital_name = row.hospital_name,
                                his_type = "",
                                his_type_name = "刷卡手續費",
                                price = 20,
                                pay_deadline = row.pay_deadline,
                                payed = "N",
                            });
                            row.ApplyDetailPrice.Add(new LoginApplyDetailPriceModel()
                            {
                                apply_no = row.apply_no,
                                apply_no_sub = row.apply_no_sub,
                                user_idno = row.user_idno,
                                hospital_code = row.hospital_code,
                                hospital_name = row.hospital_name,
                                his_type = "",
                                his_type_name = "平台手續費",
                                price = 20,
                                pay_deadline = row.pay_deadline,
                                payed = "N",
                            }); 
                            */
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("HomeController.Refresh_GridRows_ApplyDetail() Error: " + ex.Message);
            }
            return PartialView("_GridRows_ApplyDetail", model);
        }

        [HttpPost]
        public ActionResult LoginApplySave(HomeViewModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            ActionResult rtn = View("Login", model);
            // 檢查
            if (model.LoginApply.ApplyDetail.ToCount() <= 0) { sm.LastErrorMessage = "請至少新增一筆申請資料！"; return rtn; }
            string rtnMsg = "";
            List<string> tmpHospitalList = new List<string>();
            for (int i = 0; i <= model.LoginApply.ApplyDetail.ToCount() - 1; i++)
            {
                var row = model.LoginApply.ApplyDetail[i];
                if (row.hospital_code.TONotNullString() == "")
                    rtnMsg = rtnMsg + /*"項目：" + (i + 1).ToString() +*/ " 醫院名稱 是必填項目！<br/>";
                if (row.his_range1.TONotNullString() == "" || row.his_range2.TONotNullString() == "")
                    rtnMsg = rtnMsg + /*"項目：" + (i + 1).ToString() +*/ " 病歷時間區間 是必填項目！<br/>";
                if (row.his_types.TONotNullString() == "")
                    rtnMsg = rtnMsg + /*"項目：" + (i + 1).ToString() +*/ " 病歷類型 是必填項目！<br/>";
                if (tmpHospitalList.IndexOf(row.hospital_code) >= 0)
                    rtnMsg = rtnMsg + "項目：" + (i + 1).ToString() + "「" + row.hospital_name + "」已重複選取！<br/>";
                else
                    tmpHospitalList.Add(row.hospital_code);

                if (row.ApplyDetailPrice != null)
                {
                    // 資安 防竄改金額
                    var ckHisList = row.HisTypes_List;
                    foreach (var rowPri in row.ApplyDetailPrice)
                    {
                        var findHis = ckHisList.Where(x => x.his_type == rowPri.his_type).FirstOrDefault();
                        if (findHis != null) rowPri.price = findHis.price;
                    }
                }
            }
            if (rtnMsg != "") { sm.LastErrorMessage = rtnMsg; return rtn; }
            // OK 
            this.SaveLoginApply(ref model);  // 存入 DB
            model.ProcessStep = "4";
            return rtn;
        }

        /// <summary>
        /// 將使用者填的 LoginApply 資料存入資料庫 EEC_Apply <br/>
        /// 如果是新增 會回填 model.LoginApply.keyid <br/>
        /// 如果是修改 會先整批刪除舊明細，再進行新增 <br/>
        /// EEC_Apply (表頭) 一位使用者只會有一筆，除新增以外基本上不會異動它
        /// </summary>
        /// <param name="model"></param>
        private void SaveLoginApply(ref HomeViewModel model)
        {
            FrontDAO dao = new FrontDAO();
            try
            {
                dao.BeginTransaction();
                // 處理表頭 EEC_Apply
                if (model.LoginApply.keyid.TONotNullString() == "")
                {
                    // 檢查
                    if (dao.GetRowList(new TblEEC_Apply() { apply_no = model.LoginApply.apply_no }).Any())
                    {
                        // 理論上 keyid 空白進來，應該是新增
                        // 但卻有資料，那有可能是異常，或是按F5重新整理
                        // 那就把原本的砍了
                        int resDel = dao.Delete(new TblEEC_Apply() { apply_no = model.LoginApply.apply_no });
                    }
                    // 新增
                    TblEEC_Apply Ins = new TblEEC_Apply();
                    Ins.InjectFrom(model.LoginApply);
                    int resKey = dao.Insert(Ins);
                    model.LoginApply.keyid = resKey;
                }
                // 先整批刪除舊明細 EEC_ApplyDetail
                int resDel1 = dao.Delete(new TblEEC_ApplyDetail() { apply_no = model.LoginApply.apply_no, payed = "N" });
                // 先整批刪除舊明細 EEC_ApplyDetailPrice
                var keyDel2 = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no = model.LoginApply.apply_no, payed = "N" }).Select(x => x.keyid).ToList();
                int resDel2 = dao.Delete(new TblEEC_ApplyDetailPrice() { apply_no = model.LoginApply.apply_no, payed = "N" });
                // 先整批刪除舊明細 EEC_ApplyDetailPrice_ApiData
                foreach (var key in keyDel2)
                {
                    if (key == null) continue;
                    if (key <= 0) continue;
                    int resDel2ApiData = dao.Delete(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = key });
                }
                // 新增明細 EEC_ApplyDetail
                var tmpListDet = model.LoginApply.ApplyDetail.Where(x => x.payed == "N").ToList();
                if (tmpListDet.Any())
                {
                    foreach (var rowDet in tmpListDet)
                    {
                        // 新增之前，先檢查目前資料庫內，該筆資料是否被付款了
                        var tmpCheck = dao.GetRow(new TblEEC_ApplyDetail()
                        {
                            apply_no = rowDet.apply_no,
                            apply_no_sub = rowDet.apply_no_sub
                        });
                        if (tmpCheck != null && tmpCheck.payed == "Y")
                        {
                            int tmpI = model.LoginApply.ApplyDetail.IndexOf(rowDet);
                            model.LoginApply.ApplyDetail[tmpI].payed = "Y";
                            foreach (var pri in model.LoginApply.ApplyDetail[tmpI].ApplyDetailPrice) pri.payed = "Y";
                            continue;
                        }
                        // 去新增
                        TblEEC_ApplyDetail insDet = new TblEEC_ApplyDetail();
                        insDet.InjectFrom(rowDet);
                        int resDet = dao.Insert(insDet);
                        // 新增明細 EEC_ApplyDetailPrice
                        var tmpListPri = rowDet.ApplyDetailPrice.Where(x => x.payed != "Y").ToList();
                        if (tmpListPri.Any())
                        {
                            foreach (var rowPce in tmpListPri)
                            {
                                TblEEC_ApplyDetailPrice insPce = new TblEEC_ApplyDetailPrice();
                                insPce.InjectFrom(rowPce);
                                int resPce = dao.Insert(insPce);
                                // 新增完 EEC_ApplyDetailPrice 後，換新增其 附屬資料(EEC_ApplyDetailPrice_ApiData) 如果有的話
                                if (rowPce.ApiData != null
                                 && rowPce.ApiData.Guid.TONotNullString() != ""
                                 && rowPce.ApiData.PatientIdNo.TONotNullString() != ""
                                 && rowPce.ApiData.AccessionNum.TONotNullString() != ""
                                 && rowPce.ApiData.HospitalId.TONotNullString() != ""
                                 && rowPce.ApiData.TemplateId.TONotNullString() != "")
                                {
                                    TblEEC_ApplyDetailPrice_ApiData insApiData = new TblEEC_ApplyDetailPrice_ApiData();
                                    insApiData.InjectFrom(rowPce.ApiData);
                                    insApiData.master_keyid = resPce;
                                    insApiData.keyid = null;
                                    int resApiData = dao.Insert(insApiData);
                                }
                            }
                        }
                    }
                }
                dao.CommitTransaction();
            }
            catch (Exception ex)
            {
                dao.RollBackTransaction();
            }
        }

        private void ReGetDatas(ref HomeViewModel model)
        {
            FrontDAO dao = new FrontDAO();
            var dataApply = dao.GetRow(new TblEEC_Apply() { apply_no = model.LoginApply.apply_no });
            if (dataApply != null)
            {
                model.LoginApply = new LoginApplyModel();
                model.LoginApply.InjectFrom(dataApply);
                model.LoginApply.user_email = dao.GetRow(new TblEEC_User() { user_idno = model.LoginApply.user_idno }).user_email;
                // 查看看有沒有明細 EEC_ApplyDetail
                var dataApplyDetail = dao.GetRowList(new TblEEC_ApplyDetail() { apply_no = model.LoginApply.apply_no });
                if (dataApplyDetail.ToCount() > 0)
                {
                    model.LoginApply.ApplyDetail = new List<LoginApplyDetailModel>();
                    foreach (var row in dataApplyDetail)
                    {
                        var tmpItem = new LoginApplyDetailModel();
                        tmpItem.InjectFrom(row);
                        tmpItem.user_birthday = model.LoginApply.user_birthday;
                        // 查看看有沒有明細 EEC_ApplyDetailPrice
                        var dataApplyDetailPrice = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no = model.LoginApply.apply_no, apply_no_sub = row.apply_no_sub });
                        if (dataApplyDetailPrice.ToCount() > 0)
                        {
                            tmpItem.ApplyDetailPrice = new List<LoginApplyDetailPriceModel>();
                            foreach (var rowDet in dataApplyDetailPrice)
                            {
                                var tmpItemDet = new LoginApplyDetailPriceModel();
                                tmpItemDet.InjectFrom(rowDet);
                                // 加入該筆的 附屬資料(EEC_ApplyDetailPrice_ApiData) 如果有的話
                                var tmpItemDetApiData = dao.GetRow(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = rowDet.keyid });
                                if (tmpItemDetApiData != null)
                                {
                                    tmpItemDet.ApiData.InjectFrom(tmpItemDetApiData);
                                }
                                tmpItem.ApplyDetailPrice.Add(tmpItemDet);
                            }
                        }
                        model.LoginApply.ApplyDetail.Add(tmpItem);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult LoginApplyPayForm_Back(HomeViewModel model)
        {
            ModelState.Clear();
            this.ReGetDatas(ref model);
            model.ProcessStep = "3";
            return View("Login", model);
        }

        //[HttpPost]
        //public ActionResult Refresh_GridRows_ApplyDetail_Pay(HomeViewModel model)
        //{
        //    ModelState.Clear();
        //    FrontDAO dao = new FrontDAO();
        //    if (model.LoginApply.ApplyDetail_PayIdx.TONotNullString() != "" && model.LoginApply.ApplyDetail_PayIdx.TOInt32() >= 0)
        //    {
        //        var tmpDT = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        //        var tmpObj = model.LoginApply.ApplyDetail[model.LoginApply.ApplyDetail_PayIdx.TOInt32()];
        //        tmpObj.payed = "Y";
        //        tmpObj.payed_datetime = tmpDT;
        //        // 下面變更資料庫付費變數的部分，應該是要寫在付費程序那
        //        // 但為了測試，現在先暫時寫在這邊
        //        TblEEC_ApplyDetail whereDet = new TblEEC_ApplyDetail() { apply_no = tmpObj.apply_no, apply_no_sub = tmpObj.apply_no_sub };
        //        TblEEC_ApplyDetail updateDet = new TblEEC_ApplyDetail() { payed = "Y", payed_datetime = tmpDT };
        //        var resDet = dao.Update(updateDet, whereDet);
        //        foreach (var row in tmpObj.ApplyDetailPrice)
        //        {
        //            row.payed = "Y";
        //            row.payed_datetime = tmpDT;
        //        }
        //        // 下面變更資料庫付費變數的部分，應該是要寫在付費程序那
        //        // 但為了測試，現在先暫時寫在這邊
        //        TblEEC_ApplyDetailPrice wherePri = new TblEEC_ApplyDetailPrice() { apply_no = tmpObj.apply_no, apply_no_sub = tmpObj.apply_no_sub };
        //        TblEEC_ApplyDetailPrice updatePri = new TblEEC_ApplyDetailPrice() { payed = "Y", payed_datetime = tmpDT };
        //        var resPri = dao.Update(updatePri, wherePri);
        //    }
        //    return PartialView("_GridRows_ApplyDetail_Pay", model);
        //}

        //[HttpPost]
        //public ActionResult Refresh_GridRows_ApplyDetail_PayedList(HomeViewModel model)
        //{
        //    ModelState.Clear();
        //    return PartialView("_GridRows_ApplyDetail_PayedList", model);
        //}

        //[HttpPost]
        //public ActionResult CheckPayedOK(HomeViewModel model)
        //{
        //    ModelState.Clear();
        //    var Count1 = model.LoginApply.ApplyDetail.ToCount();
        //    var Count2 = model.LoginApply.ApplyDetail.Where(x => x.payed == "Y").ToCount();
        //    var Result = new AjaxResultStruct();
        //    Result.message = (Count1 == Count2) ? "Same" : "Diff";
        //    return Content(Result.Serialize(), "application/json");
        //}

        /// <summary>
        /// 聯合信用卡中心 SessionTransactionKey <br/>
        /// 去取得付款用的 SessionKey
        /// </summary>
        [HttpPost]
        public ActionResult Get_Pay_STKey_forLogin(HomeViewModel model)
        {
            ModelState.Clear();
            AjaxResultStruct Result = new AjaxResultStruct();
            FrontDAO dao = new FrontDAO();
            var ErrCode = "";
            var ErrMsg = "";
            var OnOff = ConfigModel.level1OnOrOff;
            if (OnOff != "1")
            {
                model.LoginApply.user_idno = "T202434361";  ///////
            }

            // 資安 防竄改金額
            foreach (var row in model.LoginApply.ApplyDetail)
            {
                var ckHisList = row.HisTypes_List;
                foreach (var rowPri in row.ApplyDetailPrice)
                {
                    var findHis = ckHisList.Where(x => x.his_type == rowPri.his_type).FirstOrDefault();
                    if (findHis != null) rowPri.price = findHis.price;
                }
            }

            // 去取得付款用的 SessionKey (聯合信用卡中心)
            var STKey = dao.Get_ApplyDetail_Pay_SessionTransactionKey_forLogin(model.LoginApply, ref ErrCode, ref ErrMsg);
            if (ErrCode == "00" && STKey != "")
            {
                Result.data = STKey;
                Result.status = true;
            }
            else
            {
                Result.message = "取得預約交易代碼失敗(" + ErrCode + ":" + ErrMsg + ")";
                Result.status = false;
            }

            // LOG
            FrontDAO.FrontLOG(model.LoginApply.user_idno, model.LoginApply.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32(), FrontDAO.em_lStatus.LoginSuccess,
                HttpContext.Request.UserHostAddress, "Home/Get_Pay_STKey_forLogin", "線上申辦-立即繳費");

            return Content(Result.Serialize(), "application/json");
        }

        [HttpPost]
        public ActionResult LoginApplyPayForm_Send(HomeViewModel model)
        {
            ModelState.Clear();
            model.ProcessStep = "5";

            // LOG
            FrontDAO.FrontLOG(model.LoginApply.user_idno, model.LoginApply.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32(), FrontDAO.em_lStatus.LoginSuccess,
                HttpContext.Request.UserHostAddress, "Home/LoginApplyPayForm_Send", "線上申辦-稍後繳費");

            return View("Login", model);
        }

        [HttpPost]
        public ActionResult LoginApplyDoneForm_Back(HomeViewModel model)
        {
            ModelState.Clear();
            this.ReGetDatas(ref model);
            model.ProcessStep = "4";
            return View("Login", model);
        }

        [HttpPost]
        public ActionResult Goto_SearchGridDetail(HomeViewModel model)
        {
            // 後來暫時改成，申請一次只能一間醫院，
            // 所以 ApplyDetail 理論上不存在多筆，這邊就直接 FirstOrDefault()
            var resModel = new HomeViewModel();
            resModel.SearchApply = new SearchApplyModel()
            {
                DetailApplyNo = model.LoginApply.ApplyDetail.FirstOrDefault().apply_no_sub,
                user_idno = model.LoginApply.user_idno,
                user_name = model.LoginApply.user_name,
                ActiveGridTab = "1",  // 隨便給個預設值
                Search1Filter = "3",  // 隨便給個預設值
            };
            return SearchGridDetail(resModel);
        }

        public ActionResult Search()
        {
            HomeViewModel model = new HomeViewModel();
            model.Search = new SearchModel();
            model.ProcessStep = "1";
            model.UserLoginTab = "1";
            return View("Search", model);
        }

        [HttpPost]
        public ActionResult SearchLoginForm1(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "1";
            ModelState.Clear();
            // LOG
            FrontDAO.FrontLOG(model.Search.user_pincode, "", FrontDAO.em_lType.Search1, FrontDAO.em_lStatus.LoginTry,
                HttpContext.Request.UserHostAddress, "Home/SearchLoginForm1", "自然人憑證登入");
            // 檢查
            #region 檢查
            if (model.Search.user_pincode.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 自然人憑證PIN碼";
                FrontDAO.FrontLOG(model.Search.user_pincode, "", FrontDAO.em_lType.Search1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm1", "自然人憑證登入");
                return View("Search", model);
            }
            if (model.Search.ValidateCode1 != sm.LoginValidateCode1)
            {
                sm.LastErrorMessage = "驗證碼輸入錯誤!";
                FrontDAO.FrontLOG(model.Search.user_pincode, "", FrontDAO.em_lType.Search1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm1", "自然人憑證登入");
                return View("Search", model);
            }
            FrontDAO dao = new FrontDAO();
            var findUser = dao.GetRowList(new TblEEC_User() { user_pincode = model.Search.user_pincode.TONotNullString() }).Where(m => m.user_idno.ToRight(4) == model.Search.user_idno4Last).ToList();

            if (findUser.ToCount() != 1)
            {
                sm.LastErrorMessage = "該使用者無申請資訊！";
                FrontDAO.FrontLOG(model.Search.user_pincode, "", FrontDAO.em_lType.Search1, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm1", "自然人憑證登入");
                return View("Search", model);
            }
            #endregion
            // OK
            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "1" };
            this.ProcessStepSearch(findUser.FirstOrDefault(), ref NewModel);

            // LOG
            FrontDAO.FrontLOG(findUser.FirstOrDefault().user_idno, findUser.FirstOrDefault().user_name, FrontDAO.em_lType.Search1, FrontDAO.em_lStatus.LoginSuccess,
                HttpContext.Request.UserHostAddress, "Home/SearchLoginForm1", "自然人憑證登入");

            return View("Search", NewModel);
        }

        [HttpPost]
        public ActionResult SearchLoginForm2(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "2";
            ModelState.Clear();
            // LOG
            FrontDAO.FrontLOG(model.Search.user_idno1, "", FrontDAO.em_lType.Search2, FrontDAO.em_lStatus.LoginTry,
                HttpContext.Request.UserHostAddress, "Home/SearchLoginForm2", "行動自然人憑證登入(TW FidO)");
            // 以防有人打小寫字母，導致 API 傳到醫院時找不到病歷 - 2025.1.14
            model.Search.user_idno1 = model.Search.user_idno1.TONotNullString().ToUpper();
            // 檢查
            #region 檢查
            if (model.Search.user_idno1.TONotNullString() == "" || model.Search.user_birthday_Y.TONotNullString() == "" || model.Search.user_birthday_M.TONotNullString() == "" || model.Search.user_birthday_D.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 身分證字號 及 出生年月日";
                FrontDAO.FrontLOG(model.Search.user_idno1, "", FrontDAO.em_lType.Search2, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm2", "行動自然人憑證登入(TW FidO)");
                return View("Search", model);
            }
            if (model.Search.ValidateCode2 != sm.LoginValidateCode2)
            {
                sm.LastErrorMessage = "驗證碼輸入錯誤!";
                FrontDAO.FrontLOG(model.Search.user_idno1, "", FrontDAO.em_lType.Search2, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm2", "行動自然人憑證登入(TW FidO)");
                return View("Search", model);
            }
            string BirthYMD = (model.Search.user_birthday_Y + 1911).ToString() + model.Search.user_birthday_M.TONotNullString().PadLeft(2, '0') + model.Search.user_birthday_D.TONotNullString().PadLeft(2, '0');
            FrontDAO dao = new FrontDAO();
            var findUser = dao.GetRowList(new TblEEC_User()
            {
                user_idno = model.Search.user_idno1.TONotNullString(),
                user_birthday = BirthYMD,
            });
            if (findUser.ToCount() != 1)
            {
                sm.LastErrorMessage = "身分證字號 及 出生年月日 錯誤！登入失敗！";
                FrontDAO.FrontLOG(model.Search.user_idno1, "", FrontDAO.em_lType.Search2, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm2", "行動自然人憑證登入(TW FidO)");
                return View("Search", model);
            }
            #endregion
            // OK
            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "2" };
            this.ProcessStepSearch(findUser.FirstOrDefault(), ref NewModel);
            // LOG
            FrontDAO.FrontLOG(findUser.FirstOrDefault().user_idno, findUser.FirstOrDefault().user_name, FrontDAO.em_lType.Search2, FrontDAO.em_lStatus.LoginSuccess,
                HttpContext.Request.UserHostAddress, "Home/SearchLoginForm2", "行動自然人憑證登入(TW FidO)");
            return View("Search", NewModel);
        }

        [HttpPost]
        public ActionResult SearchLoginForm3(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "3";
            ModelState.Clear();
            // LOG
            FrontDAO.FrontLOG(model.Search.user_idno2, model.Search.user_name, FrontDAO.em_lType.Search3, FrontDAO.em_lStatus.LoginTry,
                HttpContext.Request.UserHostAddress, "Home/SearchLoginForm3", "身分證字號＋健保卡");
            // 以防有人打小寫字母，導致 API 傳到醫院時找不到病歷 - 2025.1.14
            model.Search.user_idno2 = model.Search.user_idno2.TONotNullString().ToUpper();
            // 檢查
            #region 檢查
            if (model.Search.user_idno2.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請輸入 身分證字號";
                FrontDAO.FrontLOG(model.Search.user_idno2, model.Search.user_name, FrontDAO.em_lType.Search3, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm3", "身分證字號＋健保卡");
                return View("Search", model);
            }
            if (model.Search.ValidateCode3 != sm.LoginValidateCode3)
            {
                sm.LastErrorMessage = "驗證碼輸入錯誤!";
                FrontDAO.FrontLOG(model.Search.user_idno2, model.Search.user_name, FrontDAO.em_lType.Search3, FrontDAO.em_lStatus.LoginFailed, HttpContext.Request.UserHostAddress, "Home/SearchLoginForm3", "身分證字號＋健保卡");
                return View("Search", model);
            }

            #endregion

            #region 健保卡加密

            Guid newGuid = Guid.NewGuid();
            var UUID = newGuid.ToString();
            string input = model.Search.user_idno2.TONotNullString() + ";" + UUID;
            byte[] asciiBytes = Encoding.ASCII.GetBytes(input);

            // Load the public key from .cer file
            string appDataFolderPath = Server.MapPath("~/APP_DATA");
            string certificateFilePath = Path.Combine(appDataFolderPath, "NDC.cer");
            //string certificateFilePath = "F:\\3.Work\\MOH_eu-service\\SA\\112\\健保卡網路註冊服務\\健保卡網路註冊服務_20230823\\NDC.cer"; // Replace with actual file path
            X509Certificate2 certificate = new X509Certificate2(certificateFilePath);

            // Get the RSA public key from the certificate
            RSACryptoServiceProvider rsaPublicKey = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            // Encrypt using RSA
            byte[] encryptedData = RSAEncrypt(asciiBytes, rsaPublicKey.ExportParameters(false), false);
            //byte[] encryptedBytes = rsaPublicKey.Encrypt(asciiBytes, true);

            // Convert to Base64
            string encryptedBase64 = Convert.ToBase64String(encryptedData);

            // URL encode the Base64 string
            string encryptedUrlEncoded = HttpUtility.UrlEncode(encryptedBase64);

            var urlTest = "https://eecapply.mohw.gov.tw/";
            // URL encode the Base64 string
            string encryptedUrlEncodedUrl = HttpUtility.UrlEncode(urlTest);

            string postUrl = "https://www.cp.gov.tw/portal/PIIMVerify.aspx?checkFields=8&successUrl=" + encryptedUrlEncodedUrl + "&toVerify=" + encryptedUrlEncoded;
            #endregion

            FrontDAO dao = new FrontDAO();

            #region 將健保資訊儲存起來
            EEC_NHICardVerify nhi = new EEC_NHICardVerify();
            nhi.successURL = encryptedUrlEncodedUrl;
            nhi.toVerify = encryptedUrlEncoded;
            nhi.IDNO = model.Search.user_idno2;
            nhi.USERNAME = model.Search.user_name;
            nhi.UUID = UUID;
            nhi.CREATEDATE = DateTime.Now;
            nhi.fType = "2";
            dao.Insert(nhi);
            #endregion

            return Redirect(postUrl);

        }

        [HttpPost]
        public ActionResult SearchLoginForm3return(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            model.ProcessStep = "1";
            model.UserLoginTab = "3";
            ModelState.Clear();
            // OK

            FrontDAO dao = new FrontDAO();
            TblEEC_User euserWhere = new TblEEC_User();
            euserWhere.user_idno = model.Search.user_idno2;
            var euserList = dao.GetRowList(euserWhere);
            if (euserList.ToCount() == 0)
            {
                TblEEC_User euser = new TblEEC_User();
                euser.user_name = model.Search.user_name;
                euser.user_idno = model.Search.user_idno2;
                dao.InsertOrUpdate(euser, euserWhere);
            }



            var findUser = dao.GetRowList(new TblEEC_User() { user_idno = model.Search.user_idno2.TONotNullString() });

            HomeViewModel NewModel = new HomeViewModel() { UserLoginTab = "3" };
            this.ProcessStepSearch(findUser.FirstOrDefault(), ref NewModel);  // 現在這邊一律都當 新申請
            return View("Search", NewModel);
        }

        /// <summary>
        /// 去找 登入者申請的資料
        /// </summary>
        /// <param name="findUser"></param>
        /// <param name="model"></param>
        private void ProcessStepSearch(TblEEC_User findUser, ref HomeViewModel model)
        {
            model.ProcessStep = "2";
            model.SearchApply = new SearchApplyModel() { ActiveGridTab = "1" };
            model.SearchApply.InjectFrom(findUser);
            model.SearchApply.Search1Filter = "3";  // 預設搜尋條件 訂購區間：3個月
            model.SearchApply.Search2Filter = "";
            model.SearchApply.Search3Filter = "";
            FrontDAO dao = new FrontDAO();
            model.SearchApply.SearchGrid1 = dao.GetSearchApplyList("1", model.SearchApply);
            model.SearchApply.SearchGrid2 = dao.GetSearchApplyList("2", model.SearchApply);
            model.SearchApply.SearchGrid3 = dao.GetSearchApplyList("3", model.SearchApply);
            // 以防有人打小寫字母，導致 API 傳到醫院時找不到病歷 - 2025.1.14
            model.SearchApply.user_idno = model.SearchApply.user_idno.TONotNullString().ToUpper();
        }

        [HttpPost]
        public ActionResult Refresh_SearchGrid1(HomeViewModel model)
        {
            ModelState.Clear();
            FrontDAO dao = new FrontDAO();
            model.SearchApply.SearchGrid1 = dao.GetSearchApplyList("1", model.SearchApply);
            return PartialView("_GridRows_SearchGrid1", model);
        }

        [HttpPost]
        public ActionResult Refresh_SearchGrid2(HomeViewModel model)
        {
            ModelState.Clear();
            FrontDAO dao = new FrontDAO();
            model.SearchApply.SearchGrid2 = dao.GetSearchApplyList("2", model.SearchApply);
            return PartialView("_GridRows_SearchGrid2", model);
        }

        [HttpPost]
        public ActionResult Refresh_SearchGrid3(HomeViewModel model)
        {
            ModelState.Clear();
            FrontDAO dao = new FrontDAO();
            model.SearchApply.SearchGrid3 = dao.GetSearchApplyList("3", model.SearchApply);
            return PartialView("_GridRows_SearchGrid3", model);
        }

        [HttpPost]
        public ActionResult SearchGridDetail(HomeViewModel model)
        {
            // LOG
            FrontDAO.FrontLOG(model.SearchApply.user_idno, model.SearchApply.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32() + 3, FrontDAO.em_lStatus.LoginSuccess,
                HttpContext.Request.UserHostAddress, "Home/SearchGridDetail", "進度查詢-管理訂單");
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            if (model.SearchApply.DetailApplyNo.TONotNullString() == "")
            {
                sm.LastErrorMessage = "查無資料！";
                return View("Search", model);
            }
            // 先將列表頁資料記錄起來
            model.SearchApplyDetail = new SearchApplyDetailModel();
            model.SearchApplyDetail.InjectFrom(model.SearchApply);
            // 載入明細資料
            FrontDAO dao = new FrontDAO();
            var mainData = dao.GetRow(new TblEEC_ApplyDetail() { apply_no_sub = model.SearchApply.DetailApplyNo });
            if (mainData == null)
            {
                sm.LastErrorMessage = "查無資料！";
                return View("Search", model);
            }
            else
            {
                model.SearchApplyDetail.InjectFrom(mainData);
                // 取表頭的申請日
                var mainDataMst = dao.GetRow(new TblEEC_Apply() { apply_no = mainData.apply_no });
                model.SearchApplyDetail.createdatetime = mainDataMst.createdatetime;
                // 取明細的資料
                var mainDataPri = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.SearchApply.DetailApplyNo });
                if (mainDataPri.ToCount() > 0)
                {
                    model.SearchApplyDetail.DetailPrice = new List<SearchApplyDetailPriceModel>();
                    foreach (var row in mainDataPri)
                    {
                        var tmpObj = new SearchApplyDetailPriceModel();
                        tmpObj.InjectFrom(row);
                        model.SearchApplyDetail.DetailPrice.Add(tmpObj);
                    }
                }
            }
            model.ProcessStep = "3";
            return View("Search", model);
        }

        /// <summary>亞東用 繳費證明</summary>
        [HttpPost]
        public ActionResult SearchGridDetailPrint01(HomeViewModel model)
        {
            ModelState.Clear();

            SessionModel sm = SessionModel.Get();

            if (model.SearchApply.DetailApplyNo.TONotNullString() == "")
            {
                sm.LastErrorMessage = "查無資料！";
                return View("Search", model);
            }
            // 先將列表頁資料記錄起來
            model.SearchApplyDetail = new SearchApplyDetailModel();
            model.SearchApplyDetail.InjectFrom(model.SearchApply);
            //確認套印檔案是否存在
            var TemplatePath = Server.MapPath("~/App_Data/Temp/電子病歷服務平台繳費明細單_亞東.docx");
            if (!System.IO.File.Exists(TemplatePath)) { sm.LastErrorMessage = "格式檔遺失！"; return View("Search", model); }
            var tmpFolder = Server.MapPath("~/Uploads/WordTemplate/");
            if (!System.IO.File.Exists(tmpFolder)) { System.IO.Directory.CreateDirectory(tmpFolder); }
            // 載入明細資料
            FrontDAO dao = new FrontDAO();
            var mainData = dao.GetRow(new TblEEC_ApplyDetail() { apply_no_sub = model.SearchApply.DetailApplyNo });
            if (mainData == null)
            {
                sm.LastErrorMessage = "查無資料！";
                return View("Search", model);
            }
            else
            {
                model.SearchApplyDetail.InjectFrom(mainData);
                // 取表頭的申請日
                var mainDataMst = dao.GetRow(new TblEEC_Apply() { apply_no = mainData.apply_no });
                model.SearchApplyDetail.createdatetime = mainDataMst.createdatetime;
                // 取明細的資料
                var mainDataPri = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.SearchApply.DetailApplyNo });
                if (mainDataPri.ToCount() > 0)
                {
                    model.SearchApplyDetail.DetailPrice = new List<SearchApplyDetailPriceModel>();
                    foreach (var row in mainDataPri)
                    {
                        var tmpObj = new SearchApplyDetailPriceModel();
                        tmpObj.InjectFrom(row);
                        model.SearchApplyDetail.DetailPrice.Add(tmpObj);
                    }
                }



                var objects = new Hashtable();
                objects["APPLYNAME"] = mainDataMst.user_name;
                objects["BIRYEAR"] = mainDataMst.user_birthday.Substring(0, 4).TOInt32() - 1911;
                objects["BIRMONTH"] = mainDataMst.user_birthday.Substring(4, 2);
                objects["BIRDAYS"] = mainDataMst.user_birthday.Substring(6, 2);

                var payDate = Convert.ToDateTime(model.SearchApplyDetail.payed_datetime).ToString("yyyyMMdd");

                objects["PAYYEAR"] = payDate.Substring(0, 4).TOInt32() - 1911;
                objects["PAYMONTH"] = payDate.Substring(4, 2);
                objects["PAYDAYS"] = payDate.Substring(6, 2);

                var payMoney = model.SearchApplyDetail.DetailPrice.Select(m => m.price.TOInt32()).Sum();

                objects["PAYMONEY"] = payMoney.TOTranThousandString();

                var nowDate = DateTime.Now.ToString("yyyyMMdd");

                objects["CYEAR"] = nowDate.Substring(0, 4).TOInt32() - 1911;
                objects["CMONTH"] = nowDate.Substring(4, 2);
                objects["CDAYS"] = nowDate.Substring(6, 2);

                DocX document = DocX.Load(TemplatePath);
                foreach (var item in objects.Keys) document.ReplaceText("$" + item + "$", objects[item].TONotNullString());

                var pHtml = this.SaveAndConvertHTML(document, tmpFolder + model.SearchApplyDetail.apply_no + "tmp" + DateTime.Now.ToString("yyMMddHHmmssffffff"));

                // 轉換程序 - 將 HTML 轉換成 PDF
                byte[] arrBytes = this.Convert_Html2Pdf(pHtml);
                arrBytes = this.AddPasswordToPdf(arrBytes, model.SearchApplyDetail.user_idno, "Turbo@13141806");

                // LOG
                FrontDAO.FrontLOG(model.SearchApplyDetail.user_idno, model.SearchApplyDetail.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32() + 3, FrontDAO.em_lStatus.LoginSuccess,
                    HttpContext.Request.UserHostAddress, "Home/SearchGridDetailPrint01", "進度查詢-列印-電子病歷服務平台繳費證明_亞東");

                return File(arrBytes, "application/pdf", model.SearchApplyDetail.apply_no + DateTime.Now.ToString("yyyyMMddHHmmssffffff") + "電子病歷服務平台繳費證明_亞東.pdf");
            }
        }

        /// <summary>中山用 繳費證明</summary>
        [HttpPost]
        public ActionResult SearchGridDetailPrint02(HomeViewModel model)
        {
            ModelState.Clear();

            SessionModel sm = SessionModel.Get();

            if (model.SearchApply.DetailApplyNo.TONotNullString() == "")
            {
                sm.LastErrorMessage = "查無資料！";
                return View("Search", model);
            }
            // 先將列表頁資料記錄起來
            model.SearchApplyDetail = new SearchApplyDetailModel();
            model.SearchApplyDetail.InjectFrom(model.SearchApply);
            //確認套印檔案是否存在
            var TemplatePath = Server.MapPath("~/App_Data/Temp/電子病歷服務平台繳費明細單_中山.docx");
            if (!System.IO.File.Exists(TemplatePath)) { sm.LastErrorMessage = "格式檔遺失！"; return View("Search", model); }
            var tmpFolder = Server.MapPath("~/Uploads/WordTemplate/");
            if (!System.IO.File.Exists(tmpFolder)) { System.IO.Directory.CreateDirectory(tmpFolder); }
            // 載入明細資料
            FrontDAO dao = new FrontDAO();
            var mainData = dao.GetRow(new TblEEC_ApplyDetail() { apply_no_sub = model.SearchApply.DetailApplyNo });
            if (mainData == null)
            {
                sm.LastErrorMessage = "查無資料！";
                return View("Search", model);
            }
            else
            {
                model.SearchApplyDetail.InjectFrom(mainData);
                // 取表頭的申請日
                var mainDataMst = dao.GetRow(new TblEEC_Apply() { apply_no = mainData.apply_no });
                model.SearchApplyDetail.createdatetime = mainDataMst.createdatetime;
                // 取明細的資料
                var mainDataPri = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.SearchApply.DetailApplyNo });
                if (mainDataPri.ToCount() > 0)
                {
                    model.SearchApplyDetail.DetailPrice = new List<SearchApplyDetailPriceModel>();
                    foreach (var row in mainDataPri)
                    {
                        var tmpObj = new SearchApplyDetailPriceModel();
                        tmpObj.InjectFrom(row);
                        model.SearchApplyDetail.DetailPrice.Add(tmpObj);
                    }
                }



                var objects = new Hashtable();
                objects["APPLYNAME"] = mainDataMst.user_name;
                objects["BIRYEAR"] = mainDataMst.user_birthday.Substring(0, 4).TOInt32() - 1911;
                objects["BIRMONTH"] = mainDataMst.user_birthday.Substring(4, 2);
                objects["BIRDAYS"] = mainDataMst.user_birthday.Substring(6, 2);

                var payDate = Convert.ToDateTime(model.SearchApplyDetail.payed_datetime).ToString("yyyyMMdd");

                objects["PAYYEAR"] = payDate.Substring(0, 4).TOInt32() - 1911;
                objects["PAYMONTH"] = payDate.Substring(4, 2);
                objects["PAYDAYS"] = payDate.Substring(6, 2);

                var payMoney = model.SearchApplyDetail.DetailPrice.Select(m => m.price.TOInt32()).Sum();

                objects["PAYMONEY"] = payMoney.TOTranThousandString();

                var nowDate = DateTime.Now.ToString("yyyyMMdd");

                objects["CYEAR"] = nowDate.Substring(0, 4).TOInt32() - 1911;
                objects["CMONTH"] = nowDate.Substring(4, 2);
                objects["CDAYS"] = nowDate.Substring(6, 2);

                DocX document = DocX.Load(TemplatePath);
                foreach (var item in objects.Keys) document.ReplaceText("$" + item + "$", objects[item].TONotNullString());

                var pHtml = this.SaveAndConvertHTML(document, tmpFolder + model.SearchApplyDetail.apply_no + "tmp" + DateTime.Now.ToString("yyMMddHHmmssffffff"));

                // 轉換程序 - 將 HTML 轉換成 PDF
                byte[] arrBytes = this.Convert_Html2Pdf(pHtml);
                arrBytes = this.AddPasswordToPdf(arrBytes, model.SearchApplyDetail.user_idno, "Turbo@13141806");

                // LOG
                FrontDAO.FrontLOG(model.SearchApplyDetail.user_idno, model.SearchApplyDetail.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32() + 3, FrontDAO.em_lStatus.LoginSuccess,
                    HttpContext.Request.UserHostAddress, "Home/SearchGridDetailPrint02", "進度查詢-列印-電子病歷服務平台繳費證明_中山");

                return File(arrBytes, "application/pdf", model.SearchApplyDetail.apply_no + DateTime.Now.ToString("yyyyMMddHHmmssffffff") + "電子病歷服務平台繳費證明_中山.pdf");
            }
        }

        /// <summary>
        /// 傳入 DocX 物件後，轉成 HTML 字串回傳
        /// </summary>
        /// <param name="docx"></param>
        /// <param name="path"></param>
        /// <param name="QRStr"></param>
        /// <returns></returns>
        private string SaveAndConvertHTML(DocX docx, string path)
        {
            var wordPath = path + ".docx";
            var htmlPath = path + ".html";
            var pdf_Path = path + ".pdf";

            // 儲存成 Word 檔
            docx.SaveAs(wordPath);

            // 轉換程序 - 將 Word 檔 轉換成 Html
            var fileInfo = new FileInfo(wordPath);
            string fullFilePath = fileInfo.FullName;
            string htmlText = string.Empty;
            try
            {
                htmlText = ParseDOCX(fileInfo);
            }
            catch (OpenXmlPackageException e)
            {
                //if (e.ToString().Contains("Invalid Hyperlink"))
                //{
                //    using (FileStream fs = new FileStream(fullFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                //    {
                //        UriFixer.FixInvalidUri(fs, brokenUri => FixUri(brokenUri));
                //    }
                //    htmlText = ParseDOCX(fileInfo);
                //}
            }

            // 收據上 QRCode 的網址
            //var dataBase64 = Convert.ToBase64String(BarCodeUtils.GenerateQRCode(QRStr));
            //var imgStr = "<img alt=\"QRCode\" style=\"width: 90px;\" src=\"data:image/PNG;base64," + dataBase64 + "\" />";
            //htmlText = htmlText.Replace("QRCode-Here", imgStr);

            var writer = System.IO.File.CreateText(htmlPath);
            writer.WriteLine(htmlText);
            writer.Dispose();

            return htmlText;
        }

        private string ParseDOCX(FileInfo fileInfo)
        {
            try
            {
                byte[] byteArray = System.IO.File.ReadAllBytes(fileInfo.FullName);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    using (WordprocessingDocument wDoc = WordprocessingDocument.Open(memoryStream, true))
                    {
                        int imageCounter = 0;
                        var pageTitle = fileInfo.FullName;
                        var part = wDoc.CoreFilePropertiesPart;
                        if (part != null)
                            pageTitle = (string)part.GetXDocument().Descendants(DC.title).FirstOrDefault() ?? fileInfo.FullName;

                        WmlToHtmlConverterSettings settings = new WmlToHtmlConverterSettings()
                        {
                            AdditionalCss = "body { }",
                            PageTitle = pageTitle,
                            FabricateCssClasses = true,
                            CssClassPrefix = "pt-",
                            RestrictToSupportedLanguages = false,
                            RestrictToSupportedNumberingFormats = false,
                            ImageHandler = imageInfo =>
                            {
                                ++imageCounter;
                                string extension = imageInfo.ContentType.Split('/')[1].ToLower();
                                ImageFormat imageFormat = null;
                                if (extension == "png") imageFormat = ImageFormat.Png;
                                else if (extension == "gif") imageFormat = ImageFormat.Gif;
                                else if (extension == "bmp") imageFormat = ImageFormat.Bmp;
                                else if (extension == "jpeg") imageFormat = ImageFormat.Jpeg;
                                else if (extension == "tiff")
                                {
                                    extension = "gif";
                                    imageFormat = ImageFormat.Gif;
                                }
                                else if (extension == "x-wmf")
                                {
                                    extension = "wmf";
                                    imageFormat = ImageFormat.Wmf;
                                }

                                if (imageFormat == null) return null;

                                string base64 = null;
                                try
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        imageInfo.Bitmap.Save(ms, imageFormat);
                                        var ba = ms.ToArray();
                                        base64 = System.Convert.ToBase64String(ba);
                                    }
                                }
                                catch (System.Runtime.InteropServices.ExternalException) { return null; }

                                ImageFormat format = imageInfo.Bitmap.RawFormat;
                                ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);
                                string mimeType = codec.MimeType;

                                string imageSource = string.Format("data:{0};base64,{1}", mimeType, base64);

                                XElement img = new XElement(
                                    Xhtml.img,
                                    new XAttribute(NoNamespace.src, imageSource),
                                    imageInfo.ImgStyleAttribute,
                                    imageInfo.AltText != null ?
                                    new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                                return img;
                            }
                        };

                        XElement htmlElement = WmlToHtmlConverter.ConvertToHtml(wDoc, settings);
                        var html = new XDocument(new XDocumentType("html", null, null, null), htmlElement);
                        var htmlString = html.ToString(SaveOptions.DisableFormatting);
                        return htmlString;
                    }
                }
            }
            catch
            {
                return "File contains corrupt data";
            }
        }

        /// <summary>轉換程序 - 將 HTML 轉換成 PDF</summary>
        private byte[] Convert_Html2Pdf(string theHTMLText)
        {
            Turbo.ReportTK.PDF.HtmlPDFOptions options = new Turbo.ReportTK.PDF.HtmlPDFOptions();  // 轉 PDF 的頁面參數
            options.PageSize = System.Drawing.Printing.PaperKind.A4;
            //options.MarginTop = 0.85;
            //options.MarginBottom = 0.85;
            //options.MarginLeft = 0.5;
            //options.MarginRight = 0.5;
            //options.PageInfoFontSize = 12;
            var hPdf = new HtmlPDF().Convert("", theHTMLText, options);  // Html 轉 PDF
            return hPdf;
        }

        public byte[] AddPasswordToPdf(byte[] pdfBytes, string userPassword, string ownerPassword)
        {
            using (MemoryStream inputPdfStream = new MemoryStream(pdfBytes))
            using (MemoryStream outputPdfStream = new MemoryStream())
            {
                PdfReader reader = new PdfReader(inputPdfStream);
                PdfEncryptor.Encrypt(reader, outputPdfStream, true, userPassword, ownerPassword, PdfWriter.ALLOW_PRINTING);

                return outputPdfStream.ToArray();
            }
        }

        [HttpPost]
        public ActionResult SearchGridDelete(HomeViewModel model)
        {
            ModelState.Clear();
            SessionModel sm = SessionModel.Get();
            if (model.SearchApply.DeleteApplyNo.TONotNullString() == "")
            {
                sm.LastErrorMessage = "查無資料！";
                return View("Search", model);
            }
            FrontDAO dao = new FrontDAO();
            try
            {
                dao.BeginTransaction();
                var apply_no = model.SearchApply.DeleteApplyNo.ToLeft(17);
                var apply_no_sub = model.SearchApply.DeleteApplyNo;
                // 刪除明細
                var key1 = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = apply_no_sub }).Select(x => x.keyid).ToList();
                int res1 = dao.Delete(new TblEEC_ApplyDetailPrice() { apply_no_sub = apply_no_sub });
                int res2 = dao.Delete(new TblEEC_ApplyDetail() { apply_no_sub = apply_no_sub });
                foreach (var key in key1)
                {
                    if (key == null) continue;
                    if (key <= 0) continue;
                    int res1ApiData = dao.Delete(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = key });
                }
                // 查看是否還有其他明細資料
                var tmpList1 = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no = apply_no });
                var tmpList2 = dao.GetRowList(new TblEEC_ApplyDetail() { apply_no = apply_no });
                // 如果都沒有其他明細，則刪除表頭
                if (!tmpList1.Any() && !tmpList2.Any())
                {
                    int res3 = dao.Delete(new TblEEC_Apply() { apply_no = apply_no });
                }
                dao.CommitTransaction();

                // LOG
                FrontDAO.FrontLOG(model.SearchApply.user_idno, model.SearchApply.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32() + 3, FrontDAO.em_lStatus.LoginSuccess,
                    HttpContext.Request.UserHostAddress, "Home/SearchGridDelete", "進度查詢-刪除");
            }
            catch (Exception ex)
            {
                dao.RollBackTransaction();
                LOG.Error("SearchGridDelete failed:" + ex.TONotNullString());
                sm.LastErrorMessage = "刪除失敗！";
            }
            model.SearchApply.SearchGrid1 = dao.GetSearchApplyList("1", model.SearchApply);
            model.SearchApply.SearchGrid2 = dao.GetSearchApplyList("2", model.SearchApply);
            model.SearchApply.SearchGrid3 = dao.GetSearchApplyList("3", model.SearchApply);
            model.ProcessStep = "2";
            return View("Search", model);
        }

        /// <summary>
        /// 聯合信用卡中心 SessionTransactionKey <br/>
        /// 去取得付款用的 SessionKey
        /// </summary>
        [HttpPost]
        public ActionResult Get_Pay_STKey(HomeViewModel model)
        {
            ModelState.Clear();
            AjaxResultStruct Result = new AjaxResultStruct();
            FrontDAO dao = new FrontDAO();
            var ErrCode = "";
            var ErrMsg = "";
            var OnOff = ConfigModel.level1OnOrOff;
            if (OnOff != "1")
            {
                model.SearchApplyDetail.user_idno = "T202434361";  ///////
            }

            // 資安 防竄改金額
            var getData = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.SearchApplyDetail.apply_no_sub });
            foreach (var row in model.SearchApplyDetail.DetailPrice)
            {
                row.price = getData.Where(x => x.keyid == row.keyid).FirstOrDefault().price;
            }

            // 去取得付款用的 SessionKey (聯合信用卡中心)
            var STKey = dao.Get_ApplyDetail_Pay_SessionTransactionKey(model.SearchApplyDetail, ref ErrCode, ref ErrMsg);
            if (ErrCode == "00" && STKey != "")
            {
                Result.data = STKey;
                Result.status = true;
            }
            else
            {
                Result.message = "取得預約交易代碼失敗(" + ErrCode + ":" + ErrMsg + ")";
                Result.status = false;
            }

            // LOG
            FrontDAO.FrontLOG(model.SearchApplyDetail.user_idno, model.SearchApplyDetail.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32() + 3, FrontDAO.em_lStatus.LoginSuccess,
                HttpContext.Request.UserHostAddress, "Home/Get_Pay_STKey", "進度查詢-繳費");

            return Content(Result.Serialize(), "application/json");
        }

        /// <summary>
        /// 聯合信用卡中心 交易後結果回傳
        /// </summary>
        public ActionResult SuccessEEC(string Key)
        {
            ModelState.Clear();
            LOG.Debug("SuccessEEC Key: " + Key);
            SessionModel sm = SessionModel.Get();
            FrontDAO dao = new FrontDAO();
            // 取 DB 參數設定
            var GetSetups = dao.QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);
            // 去 API 抓資料
            ApiClient apiClient = new ApiClient();
            apiClient.setKEY(Key);
            apiClient.setURL(
                dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_DOMAINNAME"),  // 聯合正式或測試DomainName
                dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_REQUESTURL")   // 聯合正式或測試RequestUrl
            );
            int res = apiClient.postQuery();
            // 進 DB 查看看有無這筆訂單
            var findData = dao.GetRow(new TblEEC_ApplyDetail() { payed_orderid = apiClient.getORDERID(), payed_sessionkey = Key });
            if (findData == null)
            {
                sm.LastErrorMessage = "無對應之案件編號，請洽管理人員";
                return Redirect("~/Home/Index");
            }
            // 檢查 API Key 的特店與末端代號，是否與 DB 的參數相同
            var apiMerchant = apiClient.getMERCHANTID();
            var apiTerminal = apiClient.getTERMINALID();
            var tmpMerchant = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID");  // 聯合特店代號
            var tmpTerminal = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID");   // 聯合端末機代碼
            if (findData.hospital_code == "1131010011H")  // 亞東
            {
                tmpMerchant = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_MERCHANTID_FE");
                tmpTerminal = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_TRMINALID_FE");
            }
            // 檢查特店代號是否正確
            if (apiMerchant == tmpMerchant && apiTerminal == tmpTerminal)
            {
                // 檢查刷卡的結果 成功或失敗
                var resCode = apiClient.getRESPONSECODE();
                var resText = apiClient.getRESPONSEMSG();
                if (!resCode.Equals("00"))
                {
                    LOG.Debug("SuccessEEC Error: " + resText);
                    sm.LastErrorMessage = "繳費失敗：" + resText;
                    return Redirect("~/Home/Index");
                }
                // 刷卡成功
                var apply_no = findData.apply_no;
                var apply_no_sub = findData.apply_no_sub;
                var user_idno = findData.user_idno;
                var hospital_code = findData.hospital_code;
                var userInfo = dao.GetRow(new TblEEC_User() { user_idno = user_idno });
                try
                {
                    dao.BeginTransaction();
                    // 更新繳費狀態
                    var tmpDT = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    TblEEC_ApplyDetail whereDet = new TblEEC_ApplyDetail() { apply_no = apply_no, apply_no_sub = apply_no_sub };
                    TblEEC_ApplyDetail updateDet = new TblEEC_ApplyDetail() { payed = "Y", payed_datetime = tmpDT };
                    var resDet = dao.Update(updateDet, whereDet);
                    TblEEC_ApplyDetailPrice wherePri = new TblEEC_ApplyDetailPrice() { apply_no = apply_no, apply_no_sub = apply_no_sub };
                    TblEEC_ApplyDetailPrice updatePri = new TblEEC_ApplyDetailPrice() { payed = "Y", payed_datetime = tmpDT };
                    var resPri = dao.Update(updatePri, wherePri);
                    sm.LastResultMessage = "已付款";
                    // 如果是特殊醫院代號，則須在付款完當下，發送 API 給該醫院申請病歷
                    #region [特殊處理] 特殊醫院代號 - 亞東
                    // 找 當前醫院 是否為 特殊醫院 - 亞東
                    var HospApiList = dao.GetRowList(new TblEEC_Hospital_Api() { hospital_code = hospital_code, hospital_apikey = "A2", });
                    if (HospApiList.ToCount() == 1 && userInfo != null && hospital_code == "1131010011H")
                    {
                        // 準備 API 所需參數
                        var apiParamsData = new List<EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_1_ParamsModel>();
                        var getApplyPrice = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = apply_no_sub }).Where(x => x.his_type.TONotNullString() != "").OrderBy(x => x.keyid).ToList();
                        foreach (var row in getApplyPrice)
                        {
                            if (row.his_type.TONotNullString() == "") continue;
                            apiParamsData.Add(new EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_1_ParamsModel()
                            {
                                ec_no = row.his_type,
                                ec_name = row.his_type_name,
                                ec_price = row.price,
                                ec_date = row.ec_date,
                                ec_docType = row.ec_docType,
                                ec_system = row.ec_system,
                                ec_fileName = apply_no_sub + row.his_type,
                            });
                        }
                        // 傳給對方的 API
                        var apiResultData = EECOnline.Utils.Hospital_FarEastern_Api.Api_A2_1(
                            HospApiList.FirstOrDefault().hospital_domain,
                            userInfo.user_idno,
                            userInfo.user_birthday,
                            apply_no_sub,
                            apiParamsData
                        );
                        // API 回傳資料處理
                        var okSuccessNo = new List<string>();
                        foreach (var row in apiResultData)
                        {
                            if (row.ec_no.TONotNullString() == "") continue;
                            if (row.ec_success == "成功") okSuccessNo.Add(row.ec_no);
                            int tmpRes = dao.Update(
                                new TblEEC_ApplyDetailPrice()
                                {
                                    ec_fileName = apply_no_sub + row.ec_no,
                                    ec_success = (row.ec_success == "成功") ? "Y" : "N",
                                    ec_success_yn = "N",
                                },
                                new TblEEC_ApplyDetailPrice()
                                {
                                    apply_no_sub = apply_no_sub,
                                    his_type = row.ec_no
                                }
                            );
                            LOG.Debug(
                                "SuccessEEC call Api_A2_1 " +
                                "ec_fileName: '" + apply_no_sub + row.ec_no + "'; " +
                                "ec_success: '" + row.ec_success + "'; " +
                                "ec_reason: '" + row.ec_reason + "'; "
                            );
                        }
                        if (okSuccessNo.Any() && apiParamsData.Any() && okSuccessNo.ToCount() != apiParamsData.ToCount())
                        {
                            var dataHosp = dao.GetRow(new TblEEC_Hospital() { code = hospital_code });
                            if (dataHosp != null && dataHosp.Email.TONotNullString() != "")
                            {
                                #region 寄信通知對方，哪些是失敗的資料
                                var strSubject = "衛生福利部民眾線上申辦電子病歷服務平台 與醫院病歷請求發送失敗通知 (" + DateTime.Now.ToString("yyyy-MM-dd") + ")";
                                var strBody = "以下為請求發送失敗資料：<br /><br />" + "caseNo: " + apply_no_sub + "<br />data:<br />";
                                // 組合一下 body
                                var tmpStr = string.Join("|", okSuccessNo);
                                var tmpList = apiParamsData.Where(x => !tmpStr.Contains(x.ec_no)).ToList();
                                foreach (var row in tmpList)
                                {
                                    var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                                    tmpObj.InjectFrom(row);
                                    tmpStr = row.ec_no + " " + row.ec_name + EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj);
                                    strBody = strBody + tmpStr + "<br />";
                                }
                                strBody = strBody + "<br />再請聯絡資訊同仁協助以上述傳送病歷資料請求時異常，進行確認";
                                MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, dataHosp.Email, strSubject, strBody);
                                mailMessage.IsBodyHtml = true;
                                var t = CommonsServices.SendMail(mailMessage);
                                LOG.Debug("SuccessEEC Api_A2_1 SendMail to: " + dataHosp.Email + " | IsSuccess: " + (t.IsSuccess ? "Y" : "N") + ".");
                                #endregion
                            }
                        }
                    }
                    #endregion
                    dao.CommitTransaction();

                    // LOG
                    FrontDAO.FrontLOG(userInfo.user_idno, userInfo.user_name, FrontDAO.em_lType.None, FrontDAO.em_lStatus.LoginTry,
                        HttpContext.Request.UserHostAddress, "Home/SuccessEEC", "繳費完畢");
                }
                catch (Exception ex)
                {
                    dao.RollBackTransaction();
                    LOG.Debug("SuccessEEC Error: " + ex.Message);
                    sm.LastErrorMessage = "發生錯誤";
                    return Redirect("~/Home/Index");
                }
                // 一律都導到一個畫面去
                if (userInfo != null)
                {
                    var resModel = new HomeViewModel();
                    resModel.SuccessPage = new SuccessPageModel()
                    {
                        apply_no_sub = apply_no_sub,
                        hospital_code = findData.hospital_code,
                        hospital_name = findData.hospital_name,
                    };
                    return SuccessPage(resModel);
                }
            }
            else
            {
                sm.LastErrorMessage = "特店代碼或終端機代碼不相符";
            }
            return Redirect("~/Home/Index");
        }

        /// <summary>
        /// 中山醫院 合庫用 取 交易訂單編號                                            <br/><br/>
        /// 因為合庫的 交易訂單編號 信用卡交易，最大長度19位 而已                        <br/>
        /// 我們的 apply_no_sub 太長，要縮一下                                        <br/>
        /// 原格式是 yyyyMMddHHmmssfff + 身分證後9碼 + 流水編號(001)                   <br/>
        /// 現在改成 yyyyMMddHHmmssfff              + 流水編號( 01)                   <br/>
        /// 改成2碼流水號，這樣才剛好19位，到時候回來 SQL 再用 LIKE 找，應該可以          <br/>
        /// </summary>
        private string Get_lidm_Code(string apply_no, string apply_no_sub)
        {
            if (string.IsNullOrEmpty(apply_no)) return "";
            if (string.IsNullOrEmpty(apply_no_sub)) return "";
            var c1 = apply_no;  // yyyyMMddHHmmssfff
            var c2 = apply_no_sub.ToRight(2);  // 流水號末 2 碼
            return c1 + c2;
        }

        [HttpPost]
        public ActionResult LoginApplyPayForm_Send_csh(HomeViewModel model)
        {
            try
            {
                ModelState.Clear();
                var Result = new AjaxResultStruct();
                var ResultData = new Hashtable();
                var dao = new FrontDAO();
                var GetSetups = dao.QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);

                // 資安 防竄改金額 - 重抓一次金額更新
                foreach (var row in model.LoginApply.ApplyDetail)
                {
                    var ckHisList = row.HisTypes_List;
                    foreach (var rowPri in row.ApplyDetailPrice)
                    {
                        var findHis = ckHisList.Where(x => x.his_type == rowPri.his_type).FirstOrDefault();
                        if (findHis != null) rowPri.price = findHis.price;
                    }
                }

                // 金額
                // 現在一次申請只能一筆醫院 所以這邊直接用 FirstOrDefault()
                long purchAmt = 0;
                foreach (var row in model.LoginApply.ApplyDetail.FirstOrDefault().ApplyDetailPrice)
                {
                    purchAmt = purchAmt + (row.price ?? 0);
                }
                // 去取 交易訂單編號
                var lidm = this.Get_lidm_Code(
                    model.LoginApply.ApplyDetail.FirstOrDefault().apply_no,
                    model.LoginApply.ApplyDetail.FirstOrDefault().apply_no_sub
                );

                ResultData["MerchantID"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_MerchantID_CSH");
                ResultData["TerminalID"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_TerminalID_CSH");
                ResultData["merID"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_merID_CSH");
                ResultData["MerchantName"] = "中山醫學大學附設醫院";
                ResultData["purchAmt"] = purchAmt.ToString();
                ResultData["lidm"] = lidm;
                ResultData["AutoCap"] = "1";
                ResultData["AuthResURL"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_AuthResURL_CSH");

                Result.data = ResultData;
                Result.status = true;

                // LOG
                FrontDAO.FrontLOG(model.LoginApply.user_idno, model.LoginApply.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32(), FrontDAO.em_lStatus.LoginSuccess,
                    HttpContext.Request.UserHostAddress, "Home/LoginApplyPayForm_Send_csh", "線上申辦-立即繳費");

                LOG.Debug("LoginApplyPayForm_Send_csh() Result.data: " + JsonConvert.SerializeObject(ResultData));

                return Content(Result.Serialize(), "application/json");
            }
            catch (Exception ex)
            {
                LOG.Debug("LoginApplyPayForm_Send_csh() Error: " + ex.ToString());
                var Result = new AjaxResultStruct();
                Result.message = "錯誤！請連絡系統管理員！";
                Result.status = false;
                return Content(Result.Serialize(), "application/json");
            }
        }

        [HttpPost]
        public ActionResult SearchDetailPayForm_Send_csh(HomeViewModel model)
        {
            try
            {
                ModelState.Clear();
                var Result = new AjaxResultStruct();
                var ResultData = new Hashtable();
                var dao = new FrontDAO();
                var GetSetups = dao.QueryForListAll<Hashtable>("Front.get_ApplyDetail_Pay_SETUP", null);

                // 資安 防竄改金額 - 重抓一次金額更新
                var getData = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = model.SearchApplyDetail.apply_no_sub });
                foreach (var row in model.SearchApplyDetail.DetailPrice)
                {
                    row.price = getData.Where(x => x.keyid == row.keyid).FirstOrDefault().price;
                }

                // 金額
                long purchAmt = 0;
                foreach (var row in model.SearchApplyDetail.DetailPrice)
                {
                    purchAmt = purchAmt + (row.price ?? 0);
                }
                // 去取 交易訂單編號
                var lidm = this.Get_lidm_Code(
                    model.SearchApplyDetail.apply_no,
                    model.SearchApplyDetail.apply_no_sub
                );

                ResultData["MerchantID"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_MerchantID_CSH");
                ResultData["TerminalID"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_TerminalID_CSH");
                ResultData["merID"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_merID_CSH");
                ResultData["MerchantName"] = "中山醫學大學附設醫院";
                ResultData["purchAmt"] = purchAmt.ToString();
                ResultData["lidm"] = lidm;
                ResultData["AutoCap"] = "1";
                ResultData["AuthResURL"] = dao.GetDataFromHashtableList(GetSetups, "PAY_EEC_AuthResURL_CSH");

                Result.data = ResultData;
                Result.status = true;

                // LOG
                FrontDAO.FrontLOG(model.SearchApplyDetail.user_idno, model.SearchApplyDetail.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32(), FrontDAO.em_lStatus.LoginSuccess,
                    HttpContext.Request.UserHostAddress, "Home/SearchDetailPayForm_Send_csh", "線上申辦-立即繳費");

                LOG.Debug("SearchDetailPayForm_Send_csh() Result.data: " + JsonConvert.SerializeObject(ResultData));

                return Content(Result.Serialize(), "application/json");
            }
            catch (Exception ex)
            {
                LOG.Debug("SearchDetailPayForm_Send_csh() Error: " + ex.ToString());
                var Result = new AjaxResultStruct();
                Result.message = "錯誤！請連絡系統管理員！";
                Result.status = false;
                return Content(Result.Serialize(), "application/json");
            }
        }

        /// <summary>中山醫 合庫 授權交易輸出 API</summary>
        [HttpPost]
        public ActionResult Success_csh()
        {
            SessionModel sm = SessionModel.Get();
            FrontDAO dao = new FrontDAO();
            ActionResult rtn = RedirectToAction("Index", "Home");
            LOG.Debug("Success_csh() Begin.");
            try
            {
                // 讀取 傳入的參數資料 (Big-5 編碼)
                using (var reader = new StreamReader(Request.InputStream, Encoding.GetEncoding("big5")))
                {
                    // 取資料
                    // 然後從 URL 編碼傳成 big5
                    var org_body = reader.ReadToEnd();
                    var dec_body = HttpUtility.UrlDecode(org_body, Encoding.GetEncoding("big5"));

                    // 資料是這種格式 errcode=41&authCode=&authRespTime=20250509145814&...
                    // 可以用 ParseQueryString() 導入，然後把他轉成 Model 型態
                    var tmpModel = HttpUtility.ParseQueryString(dec_body);
                    var model = new Success_csh_AuthRespModel();
                    foreach (var item in model.GetType().GetProperties())
                    {
                        item.SetValue(model, tmpModel[item.Name]);
                    }

                    LOG.Debug("Success_csh() org_body:\r\n" + org_body);
                    LOG.Debug("Success_csh() dec_body:\r\n" + dec_body);
                    LOG.Debug("Success_csh() tmpModel:\r\n" + JsonConvert.SerializeObject(tmpModel));
                    LOG.Debug("Success_csh() model:\r\n" + JsonConvert.SerializeObject(model));

                    // 若信用卡交易 status = 0 且 authCode 非空值時，才能確認該筆交易成功授權
                    if (model.status.TONotNullString() == "0" && model.authCode.TONotNullString() != "")
                    {
                        // 成功
                        LOG.Debug("Success_csh() 交易結果：成功");
                        try
                        {
                            dao.BeginTransaction();

                            // 去查資料庫
                            var tmpPara = new Hashtable();
                            tmpPara["lidm"] = model.lidm.ToLeft(model.lidm.Length - 2) + "%" + model.lidm.ToRight(2);  // 訂單編號格式 詳見：Get_lidm_Code()
                            var findData = dao.QueryForListAll<Hashtable>("Front.get_EEC_ApplyDetailPrice_lidm", tmpPara);
                            LOG.Debug("Success_csh() findData [lidm] Key: " + tmpPara["lidm"] + ", Count: " + findData.ToCount().ToString());

                            // 有對應的訂單資料
                            if (findData.ToCount() == 1)
                            {
                                // 先檢查是否已繳款，以防使用者繳了之後又F5重整畫面或是手機平台
                                if (findData.FirstOrDefault()["payed"].TONotNullString() == "Y")
                                {
                                    LOG.Debug("Success_csh() findData [lidm] Key: " + tmpPara["lidm"] + ", 此訂單已繳費！時間：" + findData.FirstOrDefault()["payed_datetime"].TONotNullString());
                                    sm.LastResultMessage = "此訂單已繳費！時間：" + findData.FirstOrDefault()["payed_datetime"].TONotNullString();
                                    dao.RollBackTransaction();
                                    return RedirectToAction("Index", "Home");
                                }

                                // 準備基本資料
                                var apply_no = findData.FirstOrDefault()["apply_no"].TONotNullString();
                                var apply_no_sub = findData.FirstOrDefault()["apply_no_sub"].TONotNullString();
                                var user_idno = findData.FirstOrDefault()["user_idno"].TONotNullString();
                                var hospital_code = findData.FirstOrDefault()["hospital_code"].TONotNullString();
                                var hospital_name = findData.FirstOrDefault()["hospital_name"].TONotNullString();
                                var user_birthday = dao.GetRow(new TblEEC_Apply() { apply_no = apply_no, user_idno = user_idno }).user_birthday;
                                var caseNo = this.New_EEC_ApplyDetailPrice_caseNo(DateTime.Now.ToString("yyyyMMdd") + hospital_code) + "_" + model.pan.ToRight(4) + "_" + model.authCode;

                                // 更新繳費狀態
                                var tmpDT = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                                TblEEC_ApplyDetail whereDet = new TblEEC_ApplyDetail() { apply_no = apply_no, apply_no_sub = apply_no_sub };
                                TblEEC_ApplyDetail updateDet = new TblEEC_ApplyDetail() { payed = "Y", payed_datetime = tmpDT };
                                var resDet = dao.Update(updateDet, whereDet);
                                TblEEC_ApplyDetailPrice wherePri = new TblEEC_ApplyDetailPrice() { apply_no = apply_no, apply_no_sub = apply_no_sub };
                                TblEEC_ApplyDetailPrice updatePri = new TblEEC_ApplyDetailPrice() { payed = "Y", payed_datetime = tmpDT };
                                var resPri = dao.Update(updatePri, wherePri);
                                sm.LastResultMessage = "已付款";
                                LOG.Debug("Success_csh() 已付款 更新繳費狀態, caseNo: " + caseNo + ", resDet: " + resDet.ToString() + ", resPri: " + resPri.ToString());

                                // 發送 API 給該醫院申請病歷 - 中山醫
                                #region Call 中山醫 Api (A2-1)
                                // 準備 API 所需參數
                                var apiParamsData = new List<EECOnline.Utils.Hospital_csh_Api.Api_A2_1_ParamsModel>();
                                var getApplyPrice = dao.GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = apply_no_sub }).Where(x => x.his_type.TONotNullString() != "").OrderBy(x => x.keyid).ToList();
                                foreach (var row in getApplyPrice)
                                {
                                    if (row.his_type.TONotNullString() == "") continue;
                                    apiParamsData.Add(new EECOnline.Utils.Hospital_csh_Api.Api_A2_1_ParamsModel()
                                    {
                                        ec_no = row.his_type,
                                        ec_name = row.his_type_name,
                                        ec_price = row.price,
                                        ec_date = row.ec_date,
                                        ec_docType = row.ec_docType,
                                        ec_system = row.ec_system,
                                        ec_fileName = apply_no_sub + row.his_type,
                                    });
                                }
                                // Go
                                var apiResultData = EECOnline.Utils.Hospital_csh_Api.Api_A2_1(user_idno, user_birthday, caseNo, apiParamsData);
                                LOG.Debug("Success_csh() Api A2-1 傳入: " + apiParamsData.ToCount().ToString() + " 筆.");
                                LOG.Debug("Success_csh() Api A2-1 回傳: " + apiResultData.ToCount().ToString() + " 筆.");
                                // Api result
                                foreach (var row in apiResultData)
                                {
                                    if (row.ec_success != "成功") continue;
                                    var res = dao.Update(
                                        new TblEEC_ApplyDetailPrice()
                                        {
                                            ec_fileName = apply_no_sub + row.ec_no,
                                            ec_success = (row.ec_success == "成功") ? "Y" : "N",
                                            ec_success_yn = "N",
                                            caseNo = caseNo,
                                        },
                                        new TblEEC_ApplyDetailPrice()
                                        {
                                            apply_no_sub = apply_no_sub,
                                            his_type = row.ec_no
                                        }
                                    );
                                }
                                #endregion

                                // Goto 繳費成功畫面
                                var resModel = new HomeViewModel();
                                resModel.SuccessPage = new SuccessPageModel()
                                {
                                    apply_no_sub = apply_no_sub,
                                    hospital_code = hospital_code,
                                    hospital_name = hospital_name,
                                };
                                rtn = SuccessPage(resModel);
                            }
                            else
                            {
                                throw new Exception("訂單編號 " + model.lidm + " 查無資料，或對應超過一張單據");
                            }

                            dao.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            dao.RollBackTransaction();
                            LOG.Debug("Success_csh() Error: " + ex.ToString());
                            sm.LastErrorMessage = "執行失敗！<br/>訂單編號：" + model.lidm;
                        }
                    }
                    else
                    {
                        // 失敗
                        LOG.Debug("Success_csh() 交易結果：失敗");
                        sm.LastErrorMessage = "交易失敗！<br/>" + model.errDesc;
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("Success_csh() Error: " + ex.ToString());
                sm.LastErrorMessage = "執行失敗！";
            }
            LOG.Debug("Success_csh() End.");
            return rtn;
        }

        private string New_EEC_ApplyDetailPrice_caseNo(string caseNo_Key)
        {
            var para = new Hashtable();
            para["caseNo_Key"] = caseNo_Key;
            var getNo = new FrontDAO().QueryForObject<Hashtable>("Front.get_EEC_ApplyDetailPrice_caseNo", para);
            if (getNo == null || getNo["caseNo"].TONotNullString() == "")
            {
                return caseNo_Key + "001";
            }
            else
            {
                var cutNo = getNo["caseNo"].TONotNullString().Split('_');
                var nowNo = cutNo[0].ToRight(3);
                var intNo = nowNo.TOInt32() + 1;
                return caseNo_Key + intNo.ToString().PadLeft(3, '0');
            }
        }

        public ActionResult SuccessPage(HomeViewModel model)
        {
            return View("SuccessPage", model);
        }

        [HttpPost]
        public ActionResult SearchGridDetail_Back(HomeViewModel model)
        {
            // 返回列表頁，回復先前狀態
            ModelState.Clear();
            model.SearchApply = new SearchApplyModel();
            model.SearchApply.InjectFrom(model.SearchApplyDetail);
            // 重新刷新資料
            FrontDAO dao = new FrontDAO();
            model.SearchApply.SearchGrid1 = dao.GetSearchApplyList("1", model.SearchApply);
            model.SearchApply.SearchGrid2 = dao.GetSearchApplyList("2", model.SearchApply);
            model.SearchApply.SearchGrid3 = dao.GetSearchApplyList("3", model.SearchApply);
            model.ProcessStep = "2";
            return View("Search", model);
        }

        /// <summary>
        /// 圖型驗證碼轉語音撥放頁
        /// </summary>
        /// <returns></returns>
        public ActionResult VCodeAudio1()
        {
            return View();
        }

        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCode1()
        {
            EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(5);
            EECOnline.Models.SessionModel.Get().LoginValidateCode1 = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        /// <summary>
        /// 將當前的驗證碼轉成 Wav audio 輸出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCodeAudio1()
        {
            string vCode = EECOnline.Models.SessionModel.Get().LoginValidateCode1;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }

        /// <summary>
        /// 圖型驗證碼轉語音撥放頁
        /// </summary>
        /// <returns></returns>
        public ActionResult VCodeAudio2()
        {
            return View();
        }

        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCode2()
        {
            EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(5);
            EECOnline.Models.SessionModel.Get().LoginValidateCode2 = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        /// <summary>
        /// 將當前的驗證碼轉成 Wav audio 輸出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCodeAudio2()
        {
            string vCode = EECOnline.Models.SessionModel.Get().LoginValidateCode2;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }

        /// <summary>
        /// 圖型驗證碼轉語音撥放頁
        /// </summary>
        /// <returns></returns>
        public ActionResult VCodeAudio3()
        {
            return View();
        }

        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCode3()
        {
            EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(5);
            EECOnline.Models.SessionModel.Get().LoginValidateCode3 = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        /// <summary>
        /// 將當前的驗證碼轉成 Wav audio 輸出
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult GetValidateCodeAudio3()
        {
            string vCode = EECOnline.Models.SessionModel.Get().LoginValidateCode3;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }

        private void SetResponseCookie(string Value)
        {
            // 假設成功匯出，則回傳一個餅乾訊息 - by.Senya
            HttpCookie tmpCookie = new HttpCookie("CheckHasBeenDownloaded");
            tmpCookie.Value = Value;
            tmpCookie.Expires = DateTime.Now.AddSeconds(3);  // 3 秒後失效
            tmpCookie.HttpOnly = false;  // 設成 False 前面才抓得到
            Response.Cookies.Add(tmpCookie);
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        private static extern int FindMimeFromData(
            IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer,
            int cbSize,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
            int dwMimeFlags,
            out IntPtr ppwzMimeOut,
            int dwReserved
        );

        public string GetMimeFromData(byte[] data)
        {
            IntPtr mimetype = IntPtr.Zero;
            try
            {
                const int flags = 0x20; // FMFD_RETURNUPDATEDIMGMIMES
                int res = FindMimeFromData(IntPtr.Zero, null, data, data.Length, null, flags, out mimetype, 0);
                switch (res)
                {
                    case 0:
                        string mime = Marshal.PtrToStringUni(mimetype);
                        return mime;
                    // snip - error handling
                    // ...
                    default:
                        throw new Exception("Unexpected HRESULT " + res + " returned by FindMimeFromData (in urlmon.dll)");
                }
            }
            finally
            {
                if (mimetype != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(mimetype);
            }
        }

        /// <summary>下載病歷後 下載次數+1</summary>
        private void HisDownloadCountAdd(long? keyid, int? download_count, HomeViewModel model)
        {
            if (keyid == null || keyid <= 0) return;
            try
            {
                FrontDAO dao = new FrontDAO();
                var tmpCount = (download_count ?? 0) + 1;
                var res = dao.Update(
                    new TblEEC_ApplyDetailPrice() { download_count = tmpCount },
                    new TblEEC_ApplyDetailPrice() { keyid = keyid }
                );
                // LOG
                FrontDAO.FrontLOG(model.SearchApplyDetail.user_idno, model.SearchApplyDetail.user_name, (FrontDAO.em_lType)model.UserLoginTab.TOInt32() + 3, FrontDAO.em_lStatus.LoginSuccess,
                    HttpContext.Request.UserHostAddress, "Home/SearchGridDetail_HisView", "進度查詢-下載病歷", true);
            }
            catch (Exception ex)
            {
                LOG.Debug("Home.HisDownloadCountAdd() Error: " + ex.TONotNullString());
            }
        }

        [HttpPost]
        public ActionResult SearchGridDetail_HisView(HomeViewModel model)
        {
            SessionModel sm = SessionModel.Get();
            FrontDAO dao = new FrontDAO();
            ActionResult rtn = View("Search", model);
            ModelState.Clear();
            //SetResponseCookie("OK");
            // 由 HisView_keyid 去抓出，是哪一筆 EEC_ApplyDetailPrice 的明細
            var mainData = dao.GetRow(new TblEEC_ApplyDetailPrice() { keyid = model.SearchApplyDetail.HisView_keyid.TOInt64() });
            if (mainData == null) { sm.LastErrorMessage = "查無資料！"; return rtn; }
            // 檢查該筆資料，是否屬於 特殊醫院別
            if (mainData.hospital_code == "1131010011H")
            {
                // 是 亞東醫院
                var findHospApi = dao.GetRowList(new TblEEC_Hospital_Api() { hospital_code = mainData.hospital_code }).ToList();
                if (findHospApi.ToCount() > 0)
                {
                    if (mainData.ec_success_yn.TONotNullString() != "成功") { sm.LastErrorMessage = "尚未取得病歷！"; return rtn; }
                    var getSETUP = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_FarEastern_Api", del_mk = "N" });
                    if (getSETUP != null)
                    {
                        var hisPath = getSETUP.setup_val.TONotNullString() + mainData.ec_fileName.TONotNullString();
                        if (!System.IO.File.Exists(hisPath)) { sm.LastErrorMessage = "無法取得病歷！"; return rtn; }
                        Byte[] EncryptPDF = System.IO.File.ReadAllBytes(hisPath);
                        string Decryptstring = AesCrypto.AesDecrypt(Convert.ToBase64String(EncryptPDF), "pQ2azF4XY8R4BcgQ", "FekVt9gzVUMYdpKC");
                        byte[] DecryptPDFBYTE = Convert.FromBase64String(Decryptstring);
                        // 有下載的話，要回傳
                        var tmpList = findHospApi.Where(x => x.hospital_apikey == "A4").ToList();
                        if (tmpList.Any())
                        {
                            var getBirth = dao.GetRow(new TblEEC_User() { user_idno = mainData.user_idno });
                            var dataParams = new List<EECOnline.Utils.Hospital_FarEastern_Api.Api_A4_ParamsModel>();
                            dataParams.Add(new EECOnline.Utils.Hospital_FarEastern_Api.Api_A4_ParamsModel() { ec_no = mainData.his_type });
                            EECOnline.Utils.Hospital_FarEastern_Api.Api_A4(
                                tmpList.FirstOrDefault().hospital_domain,
                                mainData.user_idno,
                                getBirth.user_birthday,
                                mainData.apply_no_sub,
                                dataParams
                            );
                        }
                        this.HisDownloadCountAdd(mainData.keyid, mainData.download_count, model);
                        //pdf password :67890101 (測試用檔)
                        return File(DecryptPDFBYTE, "application/pdf", "病歷下載.pdf");
                    }
                }
            }
            else
            if (mainData.hospital_code == "1317040011H")
            {
                // 是 中山醫院
                if (mainData.ec_success_yn.TONotNullString() != "成功") { sm.LastErrorMessage = "尚未取得病歷！"; return rtn; }
                var getSETUP = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_csh_Api", del_mk = "N" });
                if (getSETUP != null)
                {
                    var hisPath = getSETUP.setup_val.TONotNullString() + mainData.ec_fileName.TONotNullString();
                    if (!System.IO.File.Exists(hisPath)) { sm.LastErrorMessage = "無法取得病歷！"; return rtn; }
                    Byte[] BytePDF = System.IO.File.ReadAllBytes(hisPath);
                    this.HisDownloadCountAdd(mainData.keyid, mainData.download_count, model);
                    return File(BytePDF, "application/pdf", "病歷下載.pdf");
                }
                return rtn;
            }
            else
            if (mainData.hospital_code == "1317050017H")
            {
                // 是 中國醫藥
            }
            else
            {
                // 是 一般 EEC
                var getApiData = dao.GetRow(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = mainData.keyid });
                if (getApiData == null) { sm.LastErrorMessage = "尚未取得病歷！"; return rtn; }
                if (getApiData.Report_HTML.TONotNullString() == "")
                {
                    if (mainData.provide_bin.TONotNullString() != "")
                    {
                        // 看看後台有沒有手動上傳檔案，有的話，就讓他下載
                        byte[] tmpBytes = Convert.FromBase64String(mainData.provide_bin);
                        var tmpContentType = this.GetMimeFromData(tmpBytes);
                        this.HisDownloadCountAdd(mainData.keyid, mainData.download_count, model);
                        return File(tmpBytes, tmpContentType, "病歷下載" + mainData.provide_ext);
                    }
                    else
                    {
                        // 如果無文件，則立刻去 EEC 抓一次
                        // 1. 取得 API Token
                        //var apiToken = EECOnline.Utils.Hospital_Common_Api.GetLoginToken(ConfigModel.LoginUser, ConfigModel.LoginPwd);
                        // 2. 取得 病歷檔案(base64) 然後存入 DB
                        //EECOnline.Utils.Hospital_Common_Api.GetQueryContent_SaveIntoDB(apiToken, getApiData.Guid, getApiData.PatientIdNo, getApiData.AccessionNum, getApiData.HospitalId, getApiData.TemplateId);
                        // 3. 抓出 DB 的 病歷檔案(base64) 然後轉成 HTML 然後存入 DB
                        EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML(mainData.his_type, getApiData.Guid, getApiData.PatientIdNo, getApiData.AccessionNum, getApiData.HospitalId, getApiData.TemplateId);
                        // 再驗一次
                        var getApiData2 = dao.GetRow(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = mainData.keyid });
                        if (getApiData2 == null) { sm.LastErrorMessage = "尚未取得病歷！"; return rtn; }
                        if (getApiData2.Report_HTML.TONotNullString() == "") { sm.LastErrorMessage = "尚未取得病歷！"; return rtn; }
                        else
                        {
                            var resUpd = dao.Update(
                                new TblEEC_ApplyDetailPrice() { provide_status = "1" },
                                new TblEEC_ApplyDetailPrice() { keyid = mainData.keyid }
                            );
                        }
                    }
                }
                // 回傳 去產 PDF
                model.TempDatas = new Hashtable();
                model.TempDatas.Add("RptId", "EEC_HTML_ReportTo_PDF");
                model.TempDatas.Add("Key", mainData.keyid.TONotNullString());
                this.HisDownloadCountAdd(mainData.keyid, mainData.download_count, model);
            }
            return rtn;
        }

        [HttpGet] public ActionResult QnA() { return View(); }

        [HttpGet] public ActionResult About() { return View(); }

        [HttpGet] public ActionResult Sitemap() { return View(); }

        [HttpGet]
        public ActionResult ContactUs(ContactUsModel model)
        {
            return View(model);
        }

        [HttpPost]
        public ActionResult ContactUs_Save(ContactUsModel model)
        {
            try
            {
                SessionModel sm = SessionModel.Get();
                string ErrMsg = "";
                if (string.IsNullOrEmpty(model.Tel)) ErrMsg = ErrMsg + "請輸入 聯絡電話！<br />";
                if (string.IsNullOrEmpty(model.Email)) ErrMsg = ErrMsg + "請輸入 聯絡信箱！<br />";
                if (string.IsNullOrEmpty(model.Type)) ErrMsg = ErrMsg + "請選擇 分類！<br />";
                if (string.IsNullOrEmpty(model.Text)) ErrMsg = ErrMsg + "請輸入 反應內容！<br />";
                if (string.IsNullOrEmpty(model.ValidateCode)) ErrMsg = ErrMsg + "請輸入 驗證碼！<br />";
                if (!model.ValidateCode.TONotNullString().Equals(sm.LoginValidateCode)) ErrMsg = ErrMsg + "驗證碼輸入錯誤！<br />";
                if (ErrMsg == "")
                {
                    ModelState.Clear();
                    // 新增進資料庫
                    var dao = new FrontDAO();
                    var ins = new TblContactUs();
                    ins.InjectFrom(model);
                    ins.keyid = null;
                    ins.Created = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    var res = dao.Insert(ins);
                    // 寄信給 CCCN@mohw.gov.tw, CCYU@mohw.gov.tw
                    var strSubject = "衛生福利部民眾線上申辦電子病歷服務平台 聯絡我們反映通知 (" + DateTime.Now.ToString("yyyy-MM-dd") + ")";
                    var strBody =
                        "聯絡我們反映通知：<br />" +
                        "<br />" +
                        "反映日期：" + ins.Created + "<br />" +
                        "反映序號：" + res.ToString() + "<br />" +
                        "<br />" +
                        "聯絡電話：" + ins.Tel + "<br />" +
                        "聯絡信箱：" + ins.Email + "<br />" +
                        "分　　類：" + ins.Type + "<br />" +
                        "反應內容：" + ins.Text + "<br />" +
                        "<br />" +
                        "此為系統自動通知信";
                    strBody = "<div style='font-size: 12pt;'>" + strBody + "</div>";
                    //MailMessage mailMessage = CommonsServices.NewMail(ConfigModel.MailSenderAddr, strSendTo, strSubject, strBody);
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress(ConfigModel.MailSenderAddr);
                    mailMessage.To.Add("CCCN@mohw.gov.tw");
                    mailMessage.To.Add("CCYU@mohw.gov.tw");
                    mailMessage.Subject = strSubject;
                    mailMessage.Body = strBody;
                    mailMessage.IsBodyHtml = true;
                    var t = CommonsServices.SendMail(mailMessage);
                    sm.LastResultMessage = "已成功寄出！";
                    return RedirectToAction("ContactUs");
                }
                else
                {
                    sm.LastErrorMessage = ErrMsg;
                    return View("ContactUs", model);
                }
            }
            catch (Exception ex)
            {
                LOG.Debug("ContactUs_Save() Error: " + ex.ToString());
                return RedirectToAction("ContactUs");
            }
        }

        public ActionResult ContactUs_VCodeAudio()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ContactUs_GetValidateCode()
        {
            EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(5);
            EECOnline.Models.SessionModel.Get().LoginValidateCode = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        [AllowAnonymous]
        public ActionResult ContactUs_GetValidateCodeAudio()
        {
            string vCode = EECOnline.Models.SessionModel.Get().LoginValidateCode;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                EECOnline.Commons.ValidateCode vc = new EECOnline.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }
    }
}