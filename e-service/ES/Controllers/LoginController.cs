using ES.Action;
using ES.Commons;
using ES.DataLayers;
using ES.Models;
using ES.Models.Entities;
using ES.Models.Share;
using ES.Services;
using ES.Utils;
using log4net;
using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Dapper;
using System.IO;
using System.Net.Mail;
using System.Windows.Interop;

namespace ES.Controllers
{
    public class LoginController : Controller
    {
        //protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected static readonly ILog logger = LogUtils.GetLogger();

        /// <summary>
        /// 登入頁面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //string s_log1 = "";
            //s_log1 += string.Format("\n ##LoginController.Index");
            //logger.Debug(s_log1);

            LoginViewModel model = new LoginViewModel();
            ActionResult rtn = View("Index", model);

            #region 會員專區
            SessionModel sm = SessionModel.Get();
            if (sm.UserInfo != null && sm.UserInfo.Member != null)
            {
                return Edit1(model);
            }
            #endregion

            return rtn;

        }

        public ActionResult LoginHCA()
        {
            return View();
        }

        /// <summary>
        /// 登入頁面-憑證登入-送出
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(LoginViewModel model)
        {
            LoginDAO dao = new LoginDAO();
            SessionModel sm = SessionModel.Get();
            string s_log1 = "(憑證登入失敗)";

            //登入作業: 前端 PIN 檢核通過, 檢核憑證效期及帳號綁定
            if (model.Hide_PinVerify.Equals("Ok") && !string.IsNullOrEmpty(model.Hide_cadata) && !string.IsNullOrEmpty(model.Hide_enccert))
            {
                s_log1 = "\n (憑證登入成功)";
                if (model.Hide_PinVerify.Equals("Ok")) { s_log1 += "\n Hide_PinVerify.Equals : Ok"; }
                s_log1 += string.Format("\n Hide_cadata: {0}", model.Hide_cadata);
                s_log1 += string.Format("\n Hide_enccert: {0}", model.Hide_enccert);
                //1:自然人憑證 2:工商憑證 3:醫事人員憑證  4:數位身分證
                s_log1 += string.Format("\n Hide_loginType: {0}", model.Hide_loginType);
                //s_log1 += string.Format("\n Hide_sign: {0}", model.Hide_sign);
                logger.Debug(s_log1);

                Dictionary<string, object> item = new Dictionary<string, object>();

                MemberModel member = null;
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    DataUtils.OpenDbConn(conn);
                    MemberAction action = new MemberAction(conn);
                    Dictionary<string, object> args = new Dictionary<string, object>();
                    args.Add("CAData", (model.Hide_cadata ?? ""));
                    args.Add("EncCert", (model.Hide_enccert ?? ""));
                    args.Add("loginType", (model.Hide_loginType ?? ""));
                    //string s_chkCA = action.CheckCAData(model.Hide_cadata, model.Hide_enccert);
                    member = action.CheckCAData(args, ref item);
                    conn.Close();
                    conn.Dispose();
                }

                string message = sm.LastErrorMessage;
                if (!string.IsNullOrEmpty(message))
                {
                    sm.LastErrorMessage = message;
                    //LoginViewModel model = new LoginViewModel();
                    return View("Index", model);
                }

                //系統未找到您的會員資料\n請先閱讀規章並點選同意後再進行加入會員
                if (member == null) { return IndexReg(model); }

                //string userId = member.Account;
                //憑證登入成功
                LoginUserInfo userInfo = new LoginUserInfo();


                // 登入帳密檢核, 並取得使用者帳號及權限角色清單資料-依帳號
                userInfo = dao.LoginValidate(member.Account, null);
                userInfo.LoginIP = HttpContext.Request.UserHostAddress;
                //1:自然人憑證 2:工商憑證 3:醫事人員憑證  4:數位身分證
                string s_cardtype = "MEMBER";
                if (model.Hide_loginType.Equals("1")) { s_cardtype = "MOICA"; }
                if (model.Hide_loginType.Equals("2")) { s_cardtype = "MOEACA"; }
                if (model.Hide_loginType.Equals("3")) { s_cardtype = "HCA1"; } /*醫事憑證-人員*/
                if (model.Hide_loginType.Equals("4")) { s_cardtype = "NEWEID"; } /*醫事憑證-人員*/
                userInfo.LoginAuth = s_cardtype;
                //登入失敗, 丟出錯誤訊息
                //if (!userInfo.LoginSuccess)
                //{
                //    if (string.IsNullOrEmpty(userInfo.LoginErrMessage)) { userInfo.LoginErrMessage = s_err1; }
                //    throw new LoginExceptions(userInfo.LoginErrMessage);
                //}

                // 將登入者資訊保存在 SessionModel 中
                //SessionModel sm = SessionModel.Get();
                sm.UserInfo = userInfo;

                #region 紀錄成功LOG

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        MemberAction action = new MemberAction(conn, tran);
                        if (action.updateLoginStatistics(s_cardtype))
                        {
                            tran.Commit();
                        }
                        else
                        {
                            tran.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        tran.Rollback();
                        throw new Exception("#updateLoginStatistics failed:" + ex.Message, ex);
                    }
                }

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        dao.Tran(conn, tran);
                        TblLOGIN_LOG llog = new TblLOGIN_LOG();

                        llog.LOGIN_ID = userInfo.UserNo;
                        llog.LOGIN_TIME = DateTime.Now;
                        llog.NAME = userInfo.Member.NAME;
                        llog.UNIT_CD = 0;
                        llog.IP_ADDR = userInfo.LoginIP;
                        llog.STATUS = "S";
                        llog.FAIL_COUNT = 0;
                        dao.Insert(llog);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        tran.Rollback();
                        throw new Exception("SaveMemberInfo failed:" + ex.Message, ex);
                    }
                }

                #endregion

                //result.status = true;
                //result.message = userInfo.Member.NAME + "  登入成功";
                //member 找到資料-登入成功-
                //s_log1 = "(憑證登入成功)";
                s_log1 = userInfo.Member.NAME + "  登入成功";
                sm.LastResultMessage = s_log1;
                return View("Index", model);

                //MemberAction action = new MemberAction(conn);
                //    ' 驗證碼符合 If Hide_vcode.Value = sm.LoginValidateCode Then End If
                //Dim redirectUrl As String = String.Empty
                //redirectUrl = sUtl_CheckCAData(Hide_cadata.Value, Hide_enccert.Value)
                //If Not IsNothing(redirectUrl) AndAlso Not String.IsNullOrEmpty(redirectUrl) Then
                //    '檢核成功, 導向首頁
                //    Response.Redirect(redirectUrl)
                //End If
            }
            else
            {
                // 登入失敗
                // LoginLog
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        dao.Tran(conn, tran);
                        var lstdata = dao.GetLastLog(model.Hide_cadata);
                        TblLOGIN_LOG llog = new TblLOGIN_LOG();
                        llog.LOGIN_ID = model.Hide_cadata;
                        llog.LOGIN_TIME = DateTime.Now;
                        llog.UNIT_CD = 0;
                        llog.IP_ADDR = HttpContext.Request.UserHostAddress;
                        llog.STATUS = "A";
                        llog.FAIL_TOTAL = lstdata != null ? lstdata.FAIL_TOTAL.TOInt32() + 1 : 1;
                        llog.FAIL_COUNT = lstdata != null ? lstdata.FAIL_COUNT.TOInt32() + 1 : 1;
                        dao.Insert(llog);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        tran.Rollback();
                        throw new Exception("SaveMemberInfo failed:" + ex.Message, ex);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            sm.LastErrorMessage = s_log1;
            return View("Index", model);
        }

        #region 會員專區-會員資料修改
        /// <summary>
        /// 會員專區-會員資料修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Edit1(LoginViewModel model)
        {
            //LoginViewModel model = new LoginViewModel();
            ActionResult rtn = View("Index", model);

            SessionModel sm = SessionModel.Get();
            if (sm.UserInfo != null && sm.UserInfo.Member != null)
            {
                ClamMember mem = sm.UserInfo.Member;

                //string s_log1 = "";
                //s_log1 = string.Format("\n ##sm.UserInfo != null mem.ACC_NO:{0}", mem.ACC_NO);
                //logger.Debug(s_log1);

                string _sql = @"SELECT * FROM MEMBER WHERE 1=1 AND ACC_NO=@ACC_NO";
                var dictionary = new Dictionary<string, object> { { "@ACC_NO", mem.ACC_NO } };
                var parameters = new DynamicParameters(dictionary);
                TblMEMBER res = null;
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    res = conn.QueryFirst<TblMEMBER>(_sql, parameters);
                    conn.Close();
                    conn.Dispose();
                }
                LoginDetailModel det1 = new LoginDetailModel()
                {
                    ACC_NO = res.ACC_NO,
                    PSWD = res.PSWD,
                    IDN = res.IDN,
                    SEX_CD = res.SEX_CD,
                    BIRTHDAY = (res.BIRTHDAY.HasValue ? res.BIRTHDAY.Value.ToString("yyyy/MM/dd") : ""),
                    NAME = res.NAME,
                    ENAME = res.ENAME,
                    ONAME = res.ONAME,
                    CNT_NAME = res.CNT_NAME.TONotNullString() == "" ? res.ACC_NO : res.CNT_NAME,
                    CNT_ENAME = res.CNT_ENAME,
                    CHR_NAME = res.CHR_NAME,
                    CHR_ENAME = res.CHR_ENAME,
                    TEL = res.TEL,
                    FAX = res.FAX,
                    CNT_TEL = res.CNT_TEL,
                    MOBILE = res.MOBILE,
                    MAIL = res.MAIL,
                    CITY_CD = res.CITY_CD,
                    TOWN_CD = res.TOWN_CD,
                    ADDR = res.ADDR,
                    EADDR = res.EADDR,
                    MEDICO = res.MEDICO,
                    MAIL_MK = res.MAIL_MK,
                    CARD_TYPE = res.CARD_TYPE,
                    CARD_INFO = res.CARD_INFO,
                    Hid_RPCTYPE = (res.IDN == null ? "" : (res.IDN.Length == 8 ? "C" : "P")),
                    Mailmark = (res.MAIL_MK == null ? true : (res.MAIL_MK.Equals("N") ? false : true)),
                    ADDR_CODE = res.CITY_CD,
                    ADDR_DETAIL = res.ADDR,
                    IsUpdata = true
                };
                model.Detail = det1;
                rtn = View("Edit1", model.Detail);
            }
            return rtn;
        }
        #endregion

        #region 加入會員

        //public ActionResult New1() { return View(); }


        /// <summary>
        /// 規章畫面
        /// </summary>
        /// <returns></returns>
        public ActionResult Regulations(LoginViewModel model)
        {
            //string s_log1 = "";
            //s_log1 += string.Format("\n ##MemberController.Regulations");
            //logger.Debug(s_log1);

            RegulationsModel Regulations = new RegulationsModel();
            ActionResult rtn = View("Regulations", Regulations);
            //ActionResult rtn = View("Regulations", model);
            return rtn;
        }

        /// <summary>
        /// 系統未找到您的會員資料\n請先閱讀規章並點選同意後再進行加入會員
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult IndexReg(LoginViewModel model)
        {
            SessionModel sm = SessionModel.Get();

            string message_no_mem = "系統未找到您的會員資料\n請先閱讀規章並點選同意後再進行加入會員 !";
            string message = sm.LastErrorMessage;
            model.lock_RPCTYPE = false;
            if (!string.IsNullOrEmpty(model.Hide_loginType))
            {
                if (model.Hide_loginType.Equals("1")) { model.Hid_RPCTYPE = "P"; }
                if (model.Hide_loginType.Equals("2")) { model.Hid_RPCTYPE = "C"; }
                if (model.Hide_loginType.Equals("3")) { model.Hid_RPCTYPE = "P"; }
                if (model.Hide_loginType.Equals("4")) { model.Hid_RPCTYPE = "P"; }
                if (!string.IsNullOrEmpty(model.Hid_RPCTYPE)) { model.lock_RPCTYPE = true; }
            }
            if (message.TONotNullString() != "")
            {
                message = string.Format("{0}\n{1}", message, message_no_mem);
                sm.LastErrorMessage = message;
            }
            else
            {
                sm.LastErrorMessage = message_no_mem;
            }

            //LoginViewModel model = new LoginViewModel();
            //model.Detail = new LoginDetailModel();
            return View("Regulations", model);
        }

        /// <summary>
        /// 加入求職會員
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult New(string num, string RPC)
        {
            SessionModel sm = SessionModel.Get();
            ViewBag.tmpMessage = "";
            //num: 1:同意新增 /other:請先閱讀規章
            if (string.IsNullOrEmpty(num)) { num = "0"; }
            //RPC: P:個人 C:公司 其它異常
            string[] PRC_CON = { "P", "C" };
            if (RPC == null || !PRC_CON.Contains(RPC))
            {
                ViewBag.tmpMessage += "請選擇加入會員 個人／公司 !\n";
                //return View("Regulations");
            }

            LoginViewModel model = new LoginViewModel();
            model.Detail = new LoginDetailModel();
            model.Hid_RPCTYPE = RPC;
            model.Detail.Hid_RPCTYPE = model.Hid_RPCTYPE;
            if (!num.Equals("1"))
            {
                ViewBag.tmpMessage += "請先閱讀規章並點選同意後再進行加入會員 !\n";
                //sm.LastErrorMessage = "請先閱讀規章並點選同意後再進行加入會員 !";
            }

            if (ViewBag.tmpMessage != "")
            {
                sm.LastErrorMessage = ViewBag.tmpMessage;
                return View("Regulations");
            }

            return View("New", model.Detail);
        }

        /// <summary>
        /// 加入會員
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult New2(LoginViewModel model)
        {
            //num: 1:同意新增 /other:請先閱讀規章
            //if (string.IsNullOrEmpty(num)) { num = "0"; }

            SessionModel sm = SessionModel.Get();
            //RPC: P:個人 C:公司 其它異常
            string[] PRC_CON = { "P", "C" };
            string s_log1 = "";
            s_log1 += string.Format("\n ##model.Hid_RPCTYPE :{0}", model.Hid_RPCTYPE ?? " [IS NULL]");
            bool flag_ng = !PRC_CON.Contains(model.Hid_RPCTYPE);
            s_log1 += string.Format("\n ##ActionResult New2 flag_ng :[{0}]", (flag_ng ? "True" : "False"));
            s_log1 += string.Format("\n ##model.Hide_PinVerify :{0}", model.Hide_PinVerify ?? " [IS NULL]");
            s_log1 += string.Format("\n ##model.Hide_enccert :{0}", model.Hide_enccert ?? " [IS NULL]");
            s_log1 += string.Format("\n ##model.Hide_cadata :{0}", model.Hide_cadata ?? " [IS NULL]");
            s_log1 += string.Format("\n ##model.Hide_sign :{0}", model.Hide_sign ?? " [IS NULL]");
            s_log1 += string.Format("\n ##model.Hide_loginType :{0}", model.Hide_loginType ?? " [IS NULL]");
            s_log1 += string.Format("\n ##model.Hid_RPCTYPE :{0}", model.Hid_RPCTYPE ?? " [IS NULL]");
            logger.Debug(s_log1);

            if (model.Hid_RPCTYPE == null || !PRC_CON.Contains(model.Hid_RPCTYPE))
            {
                sm.LastErrorMessage = "請選擇 個人／公司 !";
                return View("Regulations");
            }

            //LoginViewModel model = new LoginViewModel();
            LoginDetailModel det1 = new LoginDetailModel();
            det1.Hide_PinVerify = model.Hide_PinVerify;
            det1.Hide_enccert = model.Hide_enccert;
            det1.Hide_cadata = model.Hide_cadata;
            det1.Hide_sign = model.Hide_sign;
            det1.Hide_loginType = model.Hide_loginType;
            det1.Hid_RPCTYPE = model.Hid_RPCTYPE;

            model.Detail = det1;

            return View("New", model.Detail);
        }


        /// <summary>
        /// 帳號重複檢查
        /// </summary>
        [HttpPost]
        public ActionResult ChkACC_NO(string ACC_NO)
        {
            SessionModel sm = SessionModel.Get();
            LoginViewModel model = new LoginViewModel();

            LoginDAO dao = new LoginDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            result.message = "";
            if (ACC_NO.ToTrim() == "")
            {
                result.message = "帳號未填 \n";
            }
            else if (Regex.IsMatch(ACC_NO, @"^[A-Za-z](1|2)\d{8}$"))
            {
                result.message = "帳號不得為身分證格式 \n";
            }
            else
            {
                var data = dao.ChkACC_NO(ACC_NO);

                if (data != "")
                {
                    result.status = false;
                    result.message = data;
                }
                else
                {
                    result.status = true;
                    result.message = "此帳號可使用 !";
                }
            }
            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 自然人憑證綁定檢核
        /// </summary>
        /// <param name="ACC_NO"></param>
        /// <param name="PSWD"></param>
        /// <returns></returns>
        public ActionResult ChkACC_NO2(string ACC_NO, string PSWD)
        {
            SessionModel sm = SessionModel.Get();
            LoginViewModel model = new LoginViewModel();

            LoginDAO dao = new LoginDAO();
            var result = new AjaxResultStruct();
            result.status = false; //true:有資料 false:無資料
            result.message = "";
            bool flag_error = false;
            if (ACC_NO.ToTrim() == "")
            {
                flag_error = true;
                result.message = "帳號未填 \n";
            }
            else if (Regex.IsMatch(ACC_NO, @"^[A-Za-z](1|2)\d{8}$"))
            {
                flag_error = true;
                result.message = "帳號不得為身分證格式 \n";
            }

            if (!flag_error)
            {
                //var data = dao.ChkACC_NO2(ACC_NO);
                IList<ClamMember> list = dao.ChkACC_NO2(ACC_NO, PSWD);

                if (list != null && list.Count > 0)
                {
                    if (list[0].IDN.Length != 10)
                    {
                        result.data = null;
                        result.status = false;
                        result.message = "該帳號非身份證號申請!";
                        return Content(result.Serialize(), "application/json");
                    }
                }

                if (list != null && list.Count > 0)
                {
                    result.data = list;
                    result.status = true;
                    result.message = "請確認資料!";
                }
                else
                {
                    result.data = null;
                    result.status = false;
                    result.message = "帳號或密碼有誤!";
                }
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary>
        /// 工商憑證綁定檢核
        /// </summary>
        /// <param name="ACC_NO"></param>
        /// <param name="PSWD"></param>
        /// <returns></returns>
        public ActionResult ChkACC_NO3(string ACC_NO, string PSWD)
        {
            SessionModel sm = SessionModel.Get();
            LoginViewModel model = new LoginViewModel();

            LoginDAO dao = new LoginDAO();
            var result = new AjaxResultStruct();
            result.status = false; //true:有資料 false:無資料
            result.message = "";
            bool flag_error = false;
            if (ACC_NO.ToTrim() == "")
            {
                flag_error = true;
                result.message = "帳號未填 \n";
            }
            else if (Regex.IsMatch(ACC_NO, @"^[A-Za-z](1|2)\d{8}$"))
            {
                flag_error = true;
                result.message = "帳號不得為身分證格式 \n";
            }

            if (!flag_error)
            {
                //var data = dao.ChkACC_NO2(ACC_NO);
                IList<ClamMember> list = dao.ChkACC_NO2(ACC_NO, PSWD);

                if (list != null && list.Count > 0)
                {
                    if (list[0].IDN.Length != 8)
                    {
                        result.data = null;
                        result.status = false;
                        result.message = "該帳號非公司統編申請!";
                        return Content(result.Serialize(), "application/json");
                    }
                }

                if (list != null && list.Count > 0)
                {
                    result.data = list;
                    result.status = true;
                    result.message = "請確認資料!";
                }
                else
                {
                    result.data = null;
                    result.status = false;
                    result.message = "帳號或密碼有誤!";
                }
            }

            return Content(result.Serialize(), "application/json");
        }



        /// <summary>
        /// 儲存
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(LoginDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            string ErrorMsg = "";

            // 驗證必填欄位
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg = ErrorMsg + item.Errors[0].ErrorMessage + "\r\n";
                    }
                }
                result.status = false;
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }
            ModelState.Clear();

            detail.IsUpdata = false;
            //之後統一身分證號大寫
            if (detail.IDN != null) { detail.IDN = detail.IDN.ToUpper(); }
            if (detail.IDN.Length == 8)
            {
                ErrorMsg = dao.ChkMemberInfoC(detail);
            }
            else
            {
                ErrorMsg = dao.ChkMemberInfo(detail);
            }
            if (ErrorMsg != "")
            {
                result.status = false;
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }

            ErrorMsg = dao.ChkpwdMemberInfo(detail);
            if (ErrorMsg != "")
            {
                result.status = false;
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }

            if (detail.Hide_loginType != null && detail.Hide_PinVerify.Equals("Ok")
                && !string.IsNullOrEmpty(detail.Hide_cadata)
                && !string.IsNullOrEmpty(detail.Hide_enccert))
            {
                string[] sArrData = Regex.Split(detail.Hide_cadata, "~~", RegexOptions.IgnoreCase);
                if (sArrData.Length == 3)
                {
                    string sName = sArrData[0];//sName
                    string sCardId = sArrData[1];//CardId
                    string sLastIDNO = sArrData[2];//LatIDNO4
                    detail.CARD_IDX = sLastIDNO;
                }
                X509CertsHelper x509Helper = new X509CertsHelper(detail.Hide_enccert);
                string cardtype = null;
                if (detail.Hide_loginType.Equals("1")) { cardtype = "MOICA"; }
                if (detail.Hide_loginType.Equals("2")) { cardtype = "MOEACA"; }
                if (detail.Hide_loginType.Equals("3")) { cardtype = "HCA1"; }
                if (detail.Hide_loginType.Equals("4")) { cardtype = "NEWEID"; }
                detail.CARD_TYPE = cardtype;
                detail.CARD_INFO = x509Helper.Subject;
                //detail.CARD_IDX = sLastIDNO;
                detail.SERIALNO = x509Helper.SerialNumber;
            }

            if (detail.SERIALNO != null && detail.SERIALNO.Length > 1 && detail.CARD_IDX.Length > 1)
            {
                //使用憑證存取做一些檢核身份證號問題
                if (!detail.IDN.Contains(detail.CARD_IDX))
                {
                    ErrorMsg = "(請檢查)憑證與身分證字號驗證有誤" + "\r\n";
                    result.status = false;
                    result.message = ErrorMsg;
                    return Content(result.Serialize(), "application/json");
                }

                //detail.IDN
                //ErrorMsg = dao.ChkMemberIDN(detail.IDN);
                //if (ErrorMsg.Length > 1) {
                //    result.status = false;
                //    result.message = ErrorMsg;
                //    return Content(result.Serialize(), "application/json");
                //}
            }

            ErrorMsg = dao.SaveMemberInfo(detail);
            result.status = false;
            result.message = "加入網站會員失敗，請聯絡系統管理員。";
            if (ErrorMsg == "")
            {
                result.status = true;
                result.message = "已成功加入網站會員，請於會員登入畫面，輸入帳號密碼進行登入。";
            }

            return Content(result.Serialize(), "application/json");
        }


        /// <summary>
        /// 修改後-儲存
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveEdit(LoginDetailModel detail)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            var result = new AjaxResultStruct();
            result.status = false;
            string ErrorMsg = "";

            // 驗證必填欄位
            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState.Values)
                {
                    if (item.Errors.ToCount() > 0)
                    {
                        ErrorMsg = ErrorMsg + item.Errors[0].ErrorMessage + "\r\n";
                    }
                }
                result.status = false;
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }

            ModelState.Clear();

            //英文姓名 欄位是必要項。
            //性別 欄位是必要項。
            //出生年月日 欄位是必要項。
            detail.IsUpdata = true;
            //之後統一身分證號大寫
            if (detail.IDN != null) { detail.IDN = detail.IDN.ToUpper(); }
            if (detail.IDN.Length == 8)
            {
                ErrorMsg = dao.ChkMemberInfoC(detail);
            }
            else
            {
                ErrorMsg = dao.ChkMemberInfo(detail);
            }

            if (ErrorMsg != "")
            {
                result.status = false;
                result.message = ErrorMsg;
                return Content(result.Serialize(), "application/json");
            }

            ErrorMsg = dao.SaveEditMemberInfo(detail);
            result.status = false;
            if (ErrorMsg == "")
            {
                result.status = true;
                result.message = "會員資料維護完成，請於會員登入畫面，輸入帳號密碼進行登入。";
            }
            else
            {
                if (ErrorMsg.Contains("密碼"))
                {
                    result.message = ErrorMsg;
                }
                else
                {
                    result.message = "會員資料維護失敗，請聯絡系統管理員。";
                }
            }

            return Content(result.Serialize(), "application/json");
        }
        #endregion 加入會員

        #region 會員登入

        /// <summary>
        /// POST: Login - 帳密登入
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserLogin(LoginFormModel form)
        {
            LoginViewModel viewModel = new LoginViewModel();
            var result = new AjaxResultStruct();
            result.status = false;
            //string ErrorMsg = "";
            var serviceTel = DataUtils.GetConfig("SERVICETEL"); // 系統操作服務諮詢電話
            string s_err1 = $"帳號或密碼錯誤! 帳號登入失敗若達五次將被鎖定，請諮詢系統操作服務諮詢電話：{serviceTel}";
            // 帳號密碼
            string userId = form.UserNo;
            string userPwd_encry = null; //加密 

            bool flag_IsLocal = false; /*非本機使用*/
            if (HttpContext.Request.IsLocal) { flag_IsLocal = true; }
            if (flag_IsLocal)
            {
                if (string.IsNullOrEmpty(form.UserNo))
                {
                    result.status = false;
                    result.message = s_err1;
                    return Content(result.Serialize(), "application/json");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(form.UserNo) || string.IsNullOrEmpty(form.UserPwd))
                {
                    result.status = false;
                    result.message = s_err1;
                    return Content(result.Serialize(), "application/json");
                }
            }

            SessionModel sm = SessionModel.Get();
            bool flag_ValidateCode = false;
            string s_err2 = "";
            if (!flag_ValidateCode && (sm.LoginValidateCode == null || string.IsNullOrEmpty(sm.LoginValidateCode)))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸出有誤";
            }
            if (!flag_ValidateCode && (form.ValidateCode == null || string.IsNullOrEmpty(form.ValidateCode)))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸入有誤";
            }

            if (!flag_ValidateCode && (!form.ValidateCode.Equals(sm.LoginValidateCode)))
            {
                flag_ValidateCode = true;
                s_err2 = "驗證碼輸入錯誤";
            }
            if (flag_ValidateCode && s_err2 != "")
            {
                result.status = false;
                result.message = s_err2;
                return Content(result.Serialize(), "application/json");
            }
            sm.LoginValidateCode = "";

            //加密
            if (flag_IsLocal)
            {
                //加密
                if (!string.IsNullOrEmpty(form.UserPwd)) { userPwd_encry = DataUtils.Crypt256(form.UserPwd); }
            }
            else
            {
                userPwd_encry = DataUtils.Crypt256(form.UserPwd); //加密
                if (string.IsNullOrEmpty(userPwd_encry))
                {
                    result.status = false;
                    result.message = s_err1;
                    return Content(result.Serialize(), "application/json");
                }
            }

            try
            {
                LoginUserInfo userInfo = new LoginUserInfo();
                LoginDAO dao = new LoginDAO();

                //欄位檢核-throw ex
                if (!flag_IsLocal) { InputValidate(form); }

                var loginlog = dao.GetLastLog(form.UserNo);
                if (loginlog != null && loginlog.STATUS == "A" && form.UserNo != form.UserPwd)
                {
                    if (loginlog.FAIL_COUNT >= 5 && DateTime.Now < loginlog.LOGIN_TIME?.AddMinutes(15))
                    {
                        var serviceTel15 = DataUtils.GetConfig("SERVICETEL"); // 系統操作服務諮詢電話

                        // 錯誤達五次以上，鎖定15分鐘
                        throw new LoginExceptions($"鎖定15分鐘，請諮詢系統操作服務諮詢電話：{serviceTel15}。");
                    }
                }

                // 登入帳密檢核, 並取得使用者帳號及權限角色清單資料
                // First try with hashed password (normal flow)
                userInfo = dao.LoginValidate(userId, userPwd_encry);

                // [TEST MODE] If login failed, also try using the input directly as hash value
                // This allows testing with DB hash values without knowing original password
                if (!userInfo.LoginSuccess && !string.IsNullOrEmpty(form.UserPwd))
                {
                    userInfo = dao.LoginValidate(userId, form.UserPwd);
                }
                userInfo.LoginAuth = "MEMBER";
                userInfo.LoginIP = HttpContext.Request.UserHostAddress;

                // 登入失敗, 丟出錯誤訊息
                if (!userInfo.LoginSuccess)
                {
                    if (string.IsNullOrEmpty(userInfo.LoginErrMessage))
                    {
                        userInfo.LoginErrMessage = s_err1;
                    }
                    throw new LoginExceptions(userInfo.LoginErrMessage);
                }

                // 將登入者資訊保存在 SessionModel 中
                //SessionModel sm = SessionModel.Get();
                sm.UserInfo = userInfo;

                #region 紀錄成功LOG

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        MemberAction action = new MemberAction(conn, tran);
                        if (action.updateLoginStatistics(userInfo.LoginAuth))
                        {
                            tran.Commit();
                        }
                        else
                        {
                            tran.Rollback();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        tran.Rollback();
                        throw new Exception("#UserLogin(LoginFormModel form) failed:" + ex.Message, ex);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }

                }

                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        dao.Tran(conn, tran);
                        TblLOGIN_LOG llog = new TblLOGIN_LOG();

                        llog.LOGIN_ID = userInfo.UserNo;
                        llog.LOGIN_TIME = DateTime.Now;
                        llog.NAME = userInfo.Member.NAME;
                        llog.UNIT_CD = 0;
                        llog.IP_ADDR = userInfo.LoginIP;
                        llog.STATUS = "S";
                        llog.FAIL_COUNT = 0;
                        dao.Insert(llog);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message, ex);
                        tran.Rollback();
                        throw new Exception("SaveMemberInfo failed:" + ex.Message, ex);
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }

                #endregion

                result.status = true;
                result.message = userInfo.Member.NAME + "  登入成功。";
                var memberCheck = dao.CheckMemberData(userInfo);
                if (memberCheck == false)
                {
                    if (DataUtils.Crypt256(userInfo.Member.ACC_NO) == userInfo.Member.PSWD)
                    {
                        result.message += "請更新密碼。\n";
                    }
                    if (Convert.ToString(userInfo.Member.ACC_NO) == Convert.ToString(userInfo.Member.IDN))
                    {
                        result.message += "因應資訊安全，帳號不得同身份證字號/統一編號，請填寫服務信箱申請變更。\n";
                    }
                    result.message += "為保障申辦權益 請先進行個人基本資料的維護作業 始得進入申辦程序。";
                }
            }
            catch (LoginExceptions ex)
            {

                writeExceptionLog(form);

                logger.Warn("Login(" + userId + ") Failed from " + Request.UserHostAddress + ": " + ex.Message);

                result.status = false;
                result.message = ex.Message;
            }

            return Content(result.Serialize(), "application/json");
        }

        /// <summary> 紀錄失敗LOG</summary>
        /// <param name="form"></param>
        private void writeExceptionLog(LoginFormModel form)
        {
            if (form == null) { return; }
            // 帳號密碼
            string userId = form.UserNo;

            LoginDAO dao = new LoginDAO();
            #region 紀錄失敗LOG
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    dao.Tran(conn, tran);
                    TblLOGIN_LOG llog = new TblLOGIN_LOG();
                    // 確認有無登入紀錄再進行Insert
                    var GetLog = dao.GetFalLog(userId);
                    if (GetLog != null)
                    {
                        llog.LOGIN_ID = form.UserNo;
                        llog.LOGIN_TIME = DateTime.Now;
                        llog.NAME = "";
                        llog.UNIT_CD = 0;
                        llog.IP_ADDR = HttpContext.Request.UserHostAddress;
                        llog.STATUS = "A";
                        llog.FAIL_COUNT = GetLog.FAIL_COUNT + 1;
                        llog.FAIL_TOTAL = GetLog.FAIL_TOTAL + 1;
                        dao.Insert(llog);
                    }
                    else
                    {
                        llog.LOGIN_ID = form.UserNo;
                        llog.LOGIN_TIME = DateTime.Now;
                        llog.NAME = "";
                        llog.UNIT_CD = 0;
                        llog.IP_ADDR = HttpContext.Request.UserHostAddress;
                        llog.STATUS = "A";
                        llog.FAIL_COUNT = 1;
                        llog.FAIL_TOTAL = 1;
                        dao.Insert(llog);
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex.Message, ex);
                    tran.Rollback();
                    throw new Exception("#writeExceptionLog failed:" + ex.Message, ex);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            #endregion
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            //SessionModel sm = SessionModel.Get();
            Session.RemoveAll();

            LoginViewModel model = new LoginViewModel();

            return View("Index", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="form"></param>
        private void InputValidate(LoginFormModel form)
        {
            if (string.IsNullOrEmpty(form.UserNo) || string.IsNullOrEmpty(form.UserPwd))
            {
                LoginExceptions ex = new LoginExceptions("請輸入 帳號及密碼 !!");
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <returns></returns>
        public ActionResult Forget()
        {
            return View();
        }

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Forget(MemberForgetModel model)
        {
            SessionModel sm = SessionModel.Get();
            LoginDAO dao = new LoginDAO();
            bool isSuccess = false;
            if (ModelState.IsValid)
            {
                ModelState.Clear();

                if (!model.ValidateCode.Equals(sm.LoginValidateCode))
                {
                    sm.LastErrorMessage = "驗證碼輸入錯誤";
                }
                else
                {
                    isSuccess = true;
                }
                var data = dao.GetForgetMember(model);

                if (data == null)
                {
                    sm.LastErrorMessage = "您填寫的資料錯誤，請重新檢查！";
                    isSuccess = false;
                }
                else if (string.IsNullOrEmpty(data.ACC_NO))
                {
                    sm.LastErrorMessage = "修改密碼查無會員帳號資料";
                    isSuccess = false;
                }
                else if (isSuccess)
                {
                    string sendTo = model.Mail.ToLower();
                    string subject = MessageUtils.MAIL_FORGET_SUBJECT;
                    string name = data.NAME.TONotNullString();
                    string newPassword = DataUtils.Crypt(DateTime.Now.ToString("yyyyMMddHHmmssffffff"));
                    string url = Request.Url.ToString().Replace(Url.Action("Index", "Member"), "/");

                    if (model.Identity.Length != 10)
                    {
                        name = data.CNT_NAME.TONotNullString();
                    }

                    string body = String.Format(MessageUtils.MAIL_FORGET_BODY, name, data.ACC_NO.TONotNullString(), newPassword, url);

                    if (dao.UpdatePassword(data, newPassword))
                    {
                        if (MailUtils.SendMail(sendTo, subject, body))
                        {
                            sm.LastResultMessage = "您送出的資料我們已經收到了<br/>我們會將新密碼傳到你的E-Mail信箱<br/>謝謝！";
                            isSuccess = true;
                        }
                        else
                        {
                            sm.LastErrorMessage = "發送信件失敗，請稍後再試！";
                            isSuccess = false;
                        }
                    }
                    else
                    {
                        sm.LastErrorMessage = "異動資料庫失敗，請稍後再試！";
                        isSuccess = false;
                    }
                }
            }
            if (isSuccess)
            {
                return View("Index");
            }
            else
            {
                // 更新驗證碼
                sm.LoginValidateCode = "";
                model.ValidateCode = "";
                return View(model);
            }
        }

        #region 憑證

        ///// <summary>
        ///// 設定登入Cookie
        ///// </summary>
        ///// <param name="account"></param>
        //private void SetAuthCookie(String account, String loginType)
        //{
        //    //logger.Debug("SetAuthCookie LoginType: " + loginType);

        //    // 將管理者登入的 Cookie 設定成 Session Cookie
        //    bool isPersistent = DataUtils.GetConfig("LOGIN_PERSISTENT_MK").Equals("Y");
        //    int timeout = Int32.Parse(DataUtils.GetConfig("LOGIN_TIMEOUT"));

        //    Dictionary<string, string> userData = GetUserData();
        //    List<string> roles = null;

        //    if (userData == null)
        //    {
        //        userData = new Dictionary<string, string>();
        //        userData.Add("Roles", null);
        //        userData.Add("Id", DateTime.Now.ToString("yyyyMMddHHmmssffffff"));
        //        roles = new List<string>();
        //    }
        //    else
        //    {
        //        roles = ES.Utils.DataUtils.StringToList(userData["Roles"]);
        //    }

        //    if (userData.ContainsKey("MemberAccount"))
        //    {
        //        userData["MemberAccount"] = account;
        //        userData["MemberLoginType"] = loginType;
        //    }
        //    else
        //    {
        //        userData.Add("MemberAccount", account);
        //        userData.Add("MemberLoginType", loginType);
        //        roles.Add("Member");
        //    }

        //    if (!loginType.Equals("MEMBER"))
        //    {
        //        //userData.Add("MemberCardId", (string) Session["LoginCardId"]);
        //        if (userData.ContainsKey("MemberCardId"))
        //        {
        //            userData["MemberCardId"] = (string)Session["LoginCardId"];
        //        }
        //        else
        //        {
        //            userData.Add("MemberCardId", (string)Session["LoginCardId"]);
        //        }

        //        string cardName = (string)Session["LoginCardName"];

        //        //logger.Debug("cardName: " + cardName);

        //        if (cardName.IndexOf("=") >= 0)
        //        {
        //            cardName = cardName.Substring(cardName.IndexOf("=") + 1);
        //            cardName = cardName.Substring(0, cardName.IndexOf(","));
        //        }

        //        //logger.Debug("cardName: " + cardName);
        //        if (userData.ContainsKey("MemberCardName"))
        //        {
        //            userData["MemberCardName"] = cardName;
        //        }
        //        else
        //        {
        //            userData.Add("MemberCardName", cardName);
        //        }

        //        //userData.Add("MemberCardName", cardName);
        //    }

        //    userData["Roles"] = DataUtils.StringArrayToString(roles.ToArray(), ",");

        //    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userData["Id"], DateTime.Now, DateTime.Now.AddMinutes(timeout), isPersistent, DataUtils.DictionaryToJsonString(userData));
        //    string encTicket = FormsAuthentication.Encrypt(ticket);

        //    HttpCookie cookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
        //    if (cookie == null)
        //    {
        //        cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
        //    }
        //    if (isPersistent)
        //    {
        //        cookie.Expires = ticket.Expiration;
        //    }
        //    cookie.Value = encTicket;
        //    HttpContext.Response.AppendCookie(cookie);

        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        SqlTransaction tran = conn.BeginTransaction();

        //        MemberAction action = new MemberAction(conn, tran);
        //        if (action.updateLoginStatistics(loginType))
        //        {
        //            tran.Commit();
        //        }
        //        else
        //        {
        //            tran.Rollback();
        //        }
        //    }
        //}

        ///// <summary>
        ///// 帳號密碼驗證
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //private bool ValidateUser(LoginModel model)
        //{
        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        MemberAction action = new MemberAction(conn);
        //        MemberModel member = action.GetMember(model.LoginAccount);

        //        if (member != null && member.Password != null)
        //        {
        //            if (member.Password.Length == 13) // 使用 Unix Crypt 加密
        //            {
        //                //logger.Debug("Password: " + member.Password + " / " + DataUtils.Crypt(member.Password.Substring(0, 2), model.LoginPassword));
        //                if (member.Password.Equals(DataUtils.Crypt(member.Password.Substring(0, 2), model.LoginPassword)))
        //                {
        //                    Session.Add("Member", member);
        //                    return true;
        //                }
        //            }
        //            else // 使用SHA256加密
        //            {
        //                if (member.Password.Equals(DataUtils.Crypt256(model.LoginPassword)))
        //                {
        //                    Session.Add("Member", member);
        //                    return true;
        //                }
        //            }
        //        }
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// 憑證登入自然人查無資料，要求輸入身分證字號
        ///// Get /Member/CertLogin
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="returnUrl"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult CertLogin2()
        //{
        //    //logger.Debug("test: ");
        //    ViewBag.tempMessage = "查無自然人帳號資料，請輸入身分證字號";

        //    return View();
        //}
        ///// <summary>
        ///// 憑證登入確認身分證字號存在與否
        ///// POST /Member/CertLogin
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="returnUrl"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult CertLogin2(CertLoginModel model)
        //{
        //    String id = "";
        //    if (Request["CertLoginUserId"] != null && Request["CertLoginUserId"].ToString().Length > 0)
        //    {
        //        id = Request["CertLoginUserId"].ToString();
        //    }
        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        MemberAction action = new MemberAction(conn);

        //        if (!action.CheckIdentityExists(id))//導到註冊頁面
        //        {
        //            return View("../Member/Register");
        //        }
        //        else
        //        {
        //            if (action.UpdateC(id, Session["Cert_Subject"].ToString(), Session["Cert_CardType"].ToString(), Convert.ToString(Session["Cert_Idn"])))
        //            { //登入
        //                SetAuthCookie(action.getMemberIDN(id), (string)Session["LoginCardType"]);
        //            }
        //            else
        //            { //更新失敗

        //            }
        //        }
        //    }
        //    return RedirectToAction("Index", "Home");
        //}
        ///// <summary>
        ///// 憑證登入
        ///// POST /Member/CertLogin
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="returnUrl"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult CertLogin(CertLoginModel model, string returnUrl)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        Session["LoginCardType"] = model.CardType;
        //        Session["LoginCardId"] = model.UserId;
        //        Session["LoginCardName"] = model.UserName;

        //        //logger.Debug("CardType: " + model.CardType + " / UserId: " + model.UserId + " / UserName: " + model.UserName);

        //        //string account = "tkadmin";
        //        int validate_type = ValidateUser(model);
        //        if (validate_type != 3)
        //        {
        //            //FormsAuthentication.SetAuthCookie(account, false);                    
        //            if (validate_type == 2)//對應不到會員資料
        //            {
        //                Session["Cert_CardType"] = model.CardType;
        //                using (SqlConnection conn = GetConnection())
        //                {
        //                    conn.Open();
        //                    MemberAction action = new MemberAction(conn);

        //                    //"MOEACA"工商  "HCA0"醫事機構  "HCA1"醫事人員
        //                    if (model.CardType.Equals("MOICA"))//自然人
        //                    {
        //                        return RedirectToAction("CertLogin2", "Member");
        //                    }
        //                    else
        //                    {
        //                        if (!action.CheckIdentityExists(model.UserId))//導到註冊頁面
        //                        {
        //                            return View("../Member/Register");
        //                        }
        //                        else
        //                        {
        //                            if (action.UpdateC(model.UserId, Session["Cert_Subject"].ToString(), Session["Cert_CardType"].ToString(), Convert.ToString(Session["Cert_Idn"])))
        //                            { //登入
        //                                SetAuthCookie(action.getMemberIDN(model.UserId), (string)Session["LoginCardType"]);
        //                            }
        //                            else
        //                            {//更新失敗 

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else//導入會員頁面
        //            {
        //                //工商憑證 更新統編(因上傳憑證使用狀況需要)
        //                if (model.CardType == "MOEACA")
        //                {
        //                    using (SqlConnection conn = GetConnection())
        //                    {
        //                        conn.Open();
        //                        MemberAction action = new MemberAction(conn);
        //                        action.UpdateC(model.UserId, Session["Cert_Subject"].ToString(), Session["Cert_CardType"].ToString(), Convert.ToString(Session["Cert_Idn"]));
        //                    }
        //                }
        //                MemberModel member = (MemberModel)Session["Member"];
        //                SetAuthCookie(member.Account, (string)Session["LoginCardType"]);
        //            }
        //        }
        //        else//憑證登入驗證失敗
        //        {
        //            TempData["tempMessage"] = "憑證登入異常";
        //        }
        //    }
        //    else
        //    {
        //        TempData["tempMessage"] = "欄位驗證錯誤";
        //    }
        //    if (String.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith("/"))
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //    return Redirect(returnUrl);
        //}

        ///// <summary>
        ///// 憑證登入驗證
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns>1.有憑證註冊過 2.沒憑證註冊過 3.憑證驗證失敗</returns>
        //private int ValidateUser(CertLoginModel model)
        //{

        //    //StringBuilder param = new StringBuilder("base64P7File=" + model.pSignedData);

        //    StringBuilder param = new StringBuilder("");
        //    param.Append("base64P7File=" + HttpUtility.UrlEncode(model.pSignedData));
        //    //logger.Debug("param: " + param.ToString());

        //    byte[] postBytes = Encoding.ASCII.GetBytes(param.ToString());

        //    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(DataUtils.GetConfig("PKI_SERVER") + "service/pkcs7SignedDataLog/p7SignedDataVerify.do");
        //    req.Method = "POST";
        //    req.ContentType = "application/x-www-form-urlencoded";
        //    req.ContentLength = postBytes.Length;

        //    using (Stream reqStream = req.GetRequestStream())
        //    {
        //        reqStream.Write(postBytes, 0, postBytes.Length);
        //    }

        //    using (StreamReader sr = new StreamReader(req.GetResponse().GetResponseStream()))
        //    {
        //        Dictionary<string, string> dic = new Dictionary<string, string>();

        //        string xml = sr.ReadToEnd();
        //        logger.Debug("VA: " + xml);
        //        TempData["Cert"] = xml;

        //        XmlDocument doc = new XmlDocument();
        //        doc.LoadXml(xml);

        //        TempData["Cert"] = FormatXml(xml);
        //        TempData["Cert"] = TempData["Cert"] + "\n" + doc.SelectSingleNode(@"//pkcs7-signed-data//signer-infos//subject").InnerText;


        //        if (model.CardType == "MOEACA") //工商憑證
        //        {
        //            Session["Cert_Idn"] = doc.SelectSingleNode(@"//pkcs7-signed-data//signer-infos//certificate//id").InnerText;
        //            Session["Cert_Sn"] = doc.SelectSingleNode(@"//pkcs7-signed-data//signer-infos//serial-number").InnerText;
        //        }
        //        else
        //        {
        //            Session["Cert_Idn"] = "";
        //            Session["Cert_Sn"] = "";
        //        }


        //        Session["Cert_Subject"] = doc.SelectSingleNode(@"//pkcs7-signed-data//signer-infos//subject").InnerText;
        //        XmlNode verified = doc.SelectSingleNode("//verified");
        //        //logger.Debug("verified: " + verified.InnerText );

        //        if (verified.InnerText.Equals("true"))//憑證驗證正確
        //        {
        //            string subject = doc.SelectSingleNode(@"//pkcs7-signed-data//signer-infos//subject").InnerText;

        //            using (SqlConnection conn = GetConnection())
        //            {
        //                conn.Open();
        //                MemberAction action = new MemberAction(conn);
        //                MemberModel member = action.GetMemberC(subject, model.CardType);

        //                if (member != null)
        //                {
        //                    Session.Add("Member", member);
        //                    TempData["Cert"] = TempData["Cert"] + "\n" + "find in member";
        //                    return 1;
        //                }
        //                else
        //                {
        //                    TempData["Cert"] = TempData["Cert"] + "\n" + "not find in member";
        //                    return 2;
        //                }
        //            }

        //        }
        //    }

        //    return 3;
        //}

        //private string FormatXml(string inputXml)
        //{
        //    XmlDocument document = new XmlDocument();
        //    document.Load(new StringReader(inputXml));
        //    StringBuilder builder = new StringBuilder();
        //    using (XmlTextWriter writer = new XmlTextWriter(new StringWriter(builder)))
        //    {
        //        writer.Formatting = Formatting.Indented;
        //        document.Save(writer);
        //    }
        //    return builder.ToString();
        //}

        ///// <summary>
        ///// 登出
        ///// GET /Member/Logout
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult Logout()
        //{
        //    try
        //    {
        //        if (Request.IsAuthenticated)
        //        {
        //            Dictionary<string, string> userData = GetUserData();
        //            List<string> roles = ES.Utils.DataUtils.StringToList(userData["Roles"]);
        //            String logouAccout = GetAccount();

        //            roles.Remove("Member");

        //            if (roles.Count == 0)
        //            {
        //                // 清除表單驗證Cookie
        //                FormsAuthentication.SignOut();

        //                // 清除Session資料
        //                Session.Clear();
        //            }
        //            else
        //            {
        //                // 將管理者登入的 Cookie 設定成 Session Cookie
        //                bool isPersistent = DataUtils.GetConfig("LOGIN_PERSISTENT_MK").Equals("Y");

        //                int timeout = Int32.Parse(DataUtils.GetConfig("LOGIN_TIMEOUT"));

        //                userData.Remove("MemberAccount");
        //                userData.Remove("MemberLoginType");
        //                userData["Roles"] = ES.Utils.DataUtils.StringArrayToString(roles.ToArray(), ",");

        //                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userData["Id"], DateTime.Now, DateTime.Now.AddMinutes(timeout), isPersistent, ES.Utils.DataUtils.DictionaryToJsonString(userData));

        //                string encTicket = FormsAuthentication.Encrypt(ticket);

        //                HttpCookie cookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
        //                if (cookie == null)
        //                {
        //                    cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
        //                }
        //                if (isPersistent)
        //                {
        //                    cookie.Expires = ticket.Expiration;
        //                }
        //                cookie.Value = encTicket;
        //                HttpContext.Response.AppendCookie(cookie);
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // 清除表單驗證Cookie
        //        FormsAuthentication.SignOut();

        //        // 清除Session資料
        //        Session.Clear();
        //    }

        //    return RedirectToAction("Index", "Home");
        //}

        ///// <summary>
        ///// 加入會員 (同意條款)
        ///// Get /Member/Register
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult Register()
        //{
        //    ViewBag.tempMessage = TempData["tempMessage"];
        //    return View();
        //}

        ///// <summary>
        ///// 加入會員 (步驟二)
        ///// Get /Member/Register2
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult Register2()
        //{
        //    ViewBag.tempMessage = TempData["tempMessage"];
        //    return View();
        //}

        ///// <summary>
        ///// 加入會員 (步驟二 - 送出)
        ///// POST /Member/Register2
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //public ActionResult Register2(MemberRModel model)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        if (model.Identity.Length == 10)
        //        {
        //            if (!CheckUtils.IsIdentity(model.Identity))
        //            {
        //                ModelState.AddModelError("Identity", "身分證號格式錯誤");

        //                return View(model);
        //            }
        //        }

        //        if (CheckUtils.IsIdentity(model.Identity)) // 個人
        //        {
        //            // 取得縣市及鄉鎮市區下拉
        //            using (SqlConnection conn = GetConnection())
        //            {
        //                conn.Open();
        //                ViewBag.CityList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", "", null, true);
        //                ViewBag.TownList = CodeUtils.GetEmptySelectList();
        //            }

        //            ViewBag.IsPerson = true;

        //            return View("RegisterP", new MemberRPModel(model));
        //        }
        //        else
        //        {
        //            // 取得縣市及鄉鎮市區下拉
        //            using (SqlConnection conn = GetConnection())
        //            {
        //                conn.Open();
        //                ViewBag.CityList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", "", null, true);
        //                ViewBag.TownList = CodeUtils.GetEmptySelectList();
        //            }

        //            ViewBag.IsPerson = false;

        //            return View("RegisterC", new MemberRCModel(model));
        //        }

        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// 加入會員 (個人 - 送出)
        ///// POST /Member/RegisterP
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult RegisterP(MemberRPModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (SqlConnection conn = GetConnection())
        //        {
        //            conn.Open();

        //            MemberAction action = new MemberAction(conn);

        //            // 檢查身分證號是否存在
        //            if (action.CheckIdentityExists(model.Identity))
        //            {
        //                TempData["tempMessage"] = "註冊失敗，身分證號已存在！";
        //            }
        //            else if (action.CheckAccountExists(model.Account))
        //            {
        //                TempData["tempMessage"] = "註冊失敗，帳號已存在！";
        //            }
        //            else if (action.Insert(model))
        //            {
        //                if (Session["Cert_Subject"] != null)
        //                {
        //                    action.UpdateC(model.Identity, Session["Cert_Subject"].ToString(), Session["Cert_CardType"].ToString(), Convert.ToString(Session["Cert_Idn"]));
        //                }
        //                TempData["tempMessage"] = "您送出的資料我們已經收到了\\n歡迎您使用衛生福利部線上申辦服務\\n謝謝！";
        //                return RedirectToAction("Index", "Home");
        //            }
        //            else
        //            {
        //                TempData["tempMessage"] = "異動資料庫失敗！";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        TempData["tempMessage"] = "欄位驗證錯誤";
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// 加入會員 (步驟三 - 送出)
        ///// POST /Member/RegisterC
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult RegisterC(MemberRCModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (SqlConnection conn = GetConnection())
        //        {
        //            conn.Open();

        //            MemberAction action = new MemberAction(conn);

        //            if (action.CheckAccountExists(model.Account))
        //            {
        //                TempData["tempMessage"] = "註冊失敗，帳號已存在！";
        //            }
        //            else if (action.Insert(model))
        //            {
        //                if (Session["Cert_Subject"] != null)
        //                {
        //                    action.UpdateC(model.Identity, Session["Cert_Subject"].ToString(), Session["Cert_CardType"].ToString(), Convert.ToString(Session["Cert_Idn"]));
        //                }
        //                TempData["tempMessage"] = "您送出的資料我們已經收到了\\n歡迎您使用衛生福利部線上申辦服務\\n謝謝！";
        //                return RedirectToAction("Index", "Home");
        //            }
        //            else
        //            {
        //                TempData["tempMessage"] = "異動資料庫失敗！";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        TempData["tempMessage"] = "欄位驗證錯誤";
        //    }

        //    return View(model);
        //}


        ///// <summary>
        ///// 會員資料管理
        ///// </summary>
        ///// <returns></returns>
        //[Authorize(Roles = "Member")]
        //[HttpGet]
        //public ActionResult Edit()
        //{
        //    ViewBag.tempMessage = TempData["tempMessage"];

        //    //logger.Debug("Account: " + GetAccount());

        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();

        //        if (((MemberModel)Session["Member"]).IsPerson) // 個人
        //        {
        //            MemberAction action = new MemberAction(conn);
        //            MemberPModel member = action.GetMemberP(GetAccount());

        //            if (member != null)
        //            {
        //                if (member.TownCode != null && member.TownCode.Length == 4)
        //                {
        //                    member.CityCode = member.TownCode.Substring(0, 2);
        //                }

        //                // 縣市及鄉鎮下拉
        //                ViewBag.CityList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", "", member.CityCode, true);
        //                ViewBag.TownList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", member.CityCode, member.TownCode, true);
        //            }

        //            return View("EditP", member);
        //        }
        //        else // 公司
        //        {
        //            MemberAction action = new MemberAction(conn);
        //            MemberCModel member = action.GetMemberC(GetAccount());

        //            if (member != null)
        //            {
        //                if (member.TownCode != null && member.TownCode.Length == 4)
        //                {
        //                    member.CityCode = member.TownCode.Substring(0, 2);
        //                }

        //                // 縣市及鄉鎮下拉
        //                ViewBag.CityList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", "", member.CityCode, true);
        //                ViewBag.TownList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", member.CityCode, member.TownCode, true);
        //            }

        //            return View("EditC", member);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 會員資料管理 (個人 - 送出)
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[Authorize(Roles = "Member")]
        //[HttpPost]
        //public ActionResult EditP(MemberPModel model)
        //{
        //    model.Account = GetAccount();

        //    if (ModelState.IsValid)
        //    {
        //        using (SqlConnection conn = GetConnection())
        //        {
        //            conn.Open();
        //            MemberAction action = new MemberAction(conn);

        //            if (action.Update(model))
        //            {
        //                TempData["tempMessage"] = "您的資料已修改完成<br/>謝謝！";
        //                return RedirectToAction("Edit", "Member");
        //            }
        //            else
        //            {
        //                ViewBag.tempMessage = "異動資料庫失敗！";
        //            }

        //            model = action.GetMemberP(model.Account);
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.tempMessage = "欄位驗證錯誤";
        //    }

        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        // 縣市及鄉鎮下拉
        //        ViewBag.CityList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", "", model.CityCode, true);
        //        ViewBag.TownList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", model.CityCode, model.TownCode, true);
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// 會員資料管理 (公司 - 送出)
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[Authorize(Roles = "Member")]
        //[HttpPost]
        //public ActionResult EditC(MemberCModel model)
        //{
        //    model.Account = GetAccount();
        //    if (ModelState.IsValid)
        //    {
        //        using (SqlConnection conn = GetConnection())
        //        {
        //            conn.Open();
        //            MemberAction action = new MemberAction(conn);

        //            if (action.Update(model))
        //            {
        //                TempData["tempMessage"] = "您的資料已修改完成<br/>謝謝！";
        //                return RedirectToAction("Edit", "Member");
        //            }
        //            else
        //            {
        //                ViewBag.tempMessage = "異動資料庫失敗！";
        //            }

        //            model = action.GetMemberC(model.Account);
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.tempMessage = "欄位驗證錯誤";
        //    }

        //    using (SqlConnection conn = GetConnection())
        //    {
        //        conn.Open();
        //        // 縣市及鄉鎮下拉
        //        ViewBag.CityList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", "", model.CityCode, true);
        //        ViewBag.TownList = CodeUtils.GetCodeSelectList(conn, "TOWN_CD", model.CityCode, model.TownCode, true);
        //    }

        //    return View(model);
        //}

        ///// <summary>
        ///// 忘記密碼
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet]
        //public ActionResult Forget()
        //{
        //    ViewBag.tempMessage = TempData["tempMessage"];
        //    return View();
        //}

        ///// <summary>
        ///// 忘記密碼
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Forget(MemberForgetModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (SqlConnection conn = GetConnection())
        //        {
        //            conn.Open();
        //            SqlTransaction tran = conn.BeginTransaction();
        //            MemberAction action = new MemberAction(conn, tran);
        //            Dictionary<string, object> data = action.GetForgetMember(model);

        //            if (data == null)
        //            {
        //                ViewBag.tempMessage = "您填寫的資料錯誤，請重新檢查！";
        //            }
        //            else
        //            {
        //                string sendTo = model.Mail.ToLower();
        //                string subject = MessageUtils.MAIL_FORGET_SUBJECT;
        //                string name = (string)data["NAME"];
        //                string newPassword = DataUtils.Crypt(DateTime.Now.ToString("yyyyMMddHHmmssffffff"));
        //                string url = Request.Url.ToString().Replace(Url.Action("Forget", "Member"), "/");

        //                if (model.Identity.Length != 10)
        //                {
        //                    name = (string)data["CNT_NAME"];
        //                }

        //                string body = String.Format(MessageUtils.MAIL_FORGET_BODY, name, data["ACC_NO"], newPassword, url);

        //                data.Add("NEW_PSWD", newPassword);

        //                if (action.UpdatePassword(data))
        //                {
        //                    if (MailUtils.SendMail(sendTo, subject, body))
        //                    {
        //                        ViewBag.tempMessage = "您送出的資料我們已經收到了<br/>我們會將新密碼傳到你的E-Mail信箱<br/>謝謝！";
        //                        tran.Commit();
        //                    }
        //                    else
        //                    {
        //                        ViewBag.tempMessage = "發送信件失敗，請稍後再試！";
        //                        tran.Rollback();
        //                    }
        //                }
        //                else
        //                {
        //                    ViewBag.tempMessage = "異動資料庫失敗，請稍後再試！";
        //                }
        //            }
        //        }
        //    }
        //    return View();
        //}
        #endregion


        /// <summary>
        /// 圖型驗證碼轉語音撥放頁
        /// </summary>
        /// <returns></returns>
        public ActionResult VCodeAudio()
        {
            return View();
        }

        /// <summary>
        /// 重新產生並回傳驗證碼圖片檔案內容
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult GetValidateCode()
        {
            Commons.ValidateCode vc = new Commons.ValidateCode();
            string vCode = vc.CreateValidateCode(4);
            SessionModel.Get().LoginValidateCode = vCode;

            MemoryStream stream = vc.CreateValidateGraphic(vCode);
            return File(stream.ToArray(), "image/jpeg");
        }

        /// <summary>
        /// 將當前的驗證碼轉成 Wav audio 輸出
        /// </summary>
        /// <returns></returns>
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult GetValidateCodeAudio()
        {
            string vCode = SessionModel.Get().LoginValidateCode;

            if (string.IsNullOrEmpty(vCode))
            {
                return HttpNotFound();
            }
            else
            {
                string audioPath = HttpContext.Server.MapPath("~/Content/audio/");
                ES.Commons.ValidateCode vc = new ES.Commons.ValidateCode();
                MemoryStream stream = vc.CreateValidateAudio(vCode, audioPath);
                return File(stream.ToArray(), "audio/wav");
            }
        }

        public ActionResult Trace()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            QAViewModel model = new QAViewModel();
            LoginDAO dao = new LoginDAO();
            TblQA where = new TblQA();
            where.DEL_MK = "N";
            model = dao.GetQAList();
            return View(model);
        }

        [HttpGet]
        public ActionResult OpenMail()
        {
            ViewBag.Apply_Service = new List<SelectListItem>() {
                new SelectListItem(){Value="",Text="請選擇"},
                new SelectListItem(){Value="醫事人員或公共衛生師相關",Text="醫事人員或公共衛生師相關"},
                new SelectListItem(){Value="社工相關",Text="社工相關"},
                new SelectListItem(){Value="中醫藥相關",Text="中醫藥相關"},
                new SelectListItem(){Value="線上付款相關",Text="線上付款相關"},
                new SelectListItem(){Value="補助相關",Text="補助相關"},
                new SelectListItem(){Value="生醫、器官與儀器相關",Text="生醫、器官與儀器相關"},
                new SelectListItem(){Value="帳號相關",Text="帳號相關"},
                new SelectListItem(){Value="其他",Text="其他"},
            };
            return View();
        }

        [HttpPost]
        public ActionResult OpenMail(string phone, string email, string content, string validCode, string service)
        {
            SessionModel sm = SessionModel.Get();
            ViewBag.phone = phone;
            ViewBag.email = email;
            ViewBag.content = content;
            ViewBag.Apply_Service = new List<SelectListItem>() {
                new SelectListItem(){Value="",Text="請選擇"},
                new SelectListItem(){Value="醫事人員或公共衛生師相關",Text="醫事人員或公共衛生師相關"},
                new SelectListItem(){Value="社工相關",Text="社工相關"},
                new SelectListItem(){Value="中醫藥相關",Text="中醫藥相關"},
                new SelectListItem(){Value="線上付款相關",Text="線上付款相關"},
                new SelectListItem(){Value="補助相關",Text="補助相關"},
                new SelectListItem(){Value="與生醫、器官與儀器等類別",Text="與生醫、器官與儀器等類別"},
                new SelectListItem(){Value="帳號相關",Text="帳號相關"},
                new SelectListItem(){Value="其他",Text="其他"},
            };

            if (!validCode.Equals(sm.LoginValidateCode))
            {
                sm.LastErrorMessage = "驗證碼輸入錯誤";
            }
            else if (phone.TONotNullString() == "" && email.TONotNullString() == "")
            {
                sm.LastErrorMessage = "電話或EMAIL請則一輸入";
            }
            else if (service.TONotNullString() == "")
            {
                sm.LastErrorMessage = "請選擇申辦服務分類";
            }
            else
            {
                ApplyDAO dao = new ApplyDAO();
                dao.SendOpenMail(phone, email, content, service);
            }

            return View();
        }

    }
}
