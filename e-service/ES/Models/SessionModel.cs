using ES.DataLayers;
using ES.Services;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Web.Routing;

namespace ES.Models
{
    public class SessionModel
    {
        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private HttpSessionStateBase _session;

        private HttpSessionStateBase session
        {
            get
            {
                if (_session == null)
                {
                    logger.Info("session object is null");
                }
                return _session;
            }
        }

        private SessionModel()
        {
            if(HttpContext.Current != null)
            {
                this._session = new HttpSessionStateWrapper(HttpContext.Current.Session);
                if (this._session == null)
                {
                    throw new NullReferenceException("HttpContext.Current.Session");
                }

                _session.Timeout = 20;
                logger.Debug("SessionModel(), SessionID=" + _session.SessionID);
            }
            else
            {
                logger.Info("SessionModel(), HttpContext.Current is null");
            }
        }

        /// <summary>
        /// 取得/建立 Login SessionModel 
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static SessionModel Get()
        {
            return new SessionModel();
        }


        [Obsolete("use SessionModel.get() instead")]
        public static SessionModel Get(HttpSessionStateBase session)
        {
            return new SessionModel();
        }

        private static readonly string CASE_APP = "CASE.APPLY";

        private static readonly string VALIDATE_CODE = "SYS.LOGIN.VALIDATECODE";
        private static readonly string USER_INFO = "SYS.LOGIN.USER";
        private static readonly string CUR_ROLE = "SYS.LOGIN.ROLE";
        private static readonly string CUR_ROLE_FUNCTION = "SYS.LOGIN.ROLE.FUNCTION";
        private static readonly string LAST_ACTION_FUNC = "SYS.MENU.LAST_ACTION_FUNC";
        private static readonly string LAST_ACTION_PATH = "SYS.MENU.LAST_ACTION_PATH";
        private static readonly string LAST_ACTION_NAME = "SYS.MENU.LAST_ACTION_NAME";
        private static readonly string BREADCRUMB_PATH = "SYS.MENU.BREADCRUMB_PATH";
        private static readonly string BREADCRUMB_PATH_STORE = "SYS.MENU.BREADCRUMB_PATH_STORE";

        private static readonly string LAST_UPDATE_MODTIME = "USER.LAST_UPDATE_MODTIME";
        private static readonly string LAST_ERROR_MESSAGE = "USER.LAST_ERROR_MESSAGE";
        private static readonly string LAST_RESULT_MESSAGE = "USER.LAST_RESULT_MESSAGE";
        //private static readonly string CLOSE_AFTER_DIALOG = "USER.CLOSE_AFTER_DIALOG";
        private static readonly string REDIRECT_AFTER_BLOCK = "USER.REDIRECT_AFTER_BLOCK";

        /// <summary>使用 HTTP Get 方式導向指定網址</summary>
        private static readonly string REDIRECT_AFTER_BLOCK_2 = "USER.REDIRECT_AFTER_BLOCK_2";

        private static readonly string STATUS_ID = "STATUS.STATUS_ID";

        /// <summary>
        /// 使用者登入驗證碼
        /// </summary>
        public string LoginValidateCode
        {
            get { return (string)this.session[VALIDATE_CODE]; }
            set { this.session[VALIDATE_CODE] = value; }
        }

        /// <summary>
        /// 暫時儲存時間標記-MODTIME
        /// </summary>
        public string LastMODTIME
        {
            get
            {
                //return (string)this.session[LAST_UPDATE_MODTIME];
                string message = (string)this.session[LAST_UPDATE_MODTIME];
                this.session[LAST_UPDATE_MODTIME] = null;
                return (string.IsNullOrEmpty(message) ? null : message);
            }
            set { this.session[LAST_UPDATE_MODTIME] = value; }
        }

        #region 登入者使用資訊

        /// <summary>
        /// 登入者使用者帳號資訊
        /// </summary>
        public LoginUserInfo UserInfo
        {
            get
            {
                LoginUserInfo userInfo = null;
                if(this.session != null)
                {
                    string jsonUserInfo = (string)this.session[USER_INFO];
                    if (!string.IsNullOrWhiteSpace(jsonUserInfo))
                    {
                        userInfo = JsonConvert.DeserializeObject<LoginUserInfo>(jsonUserInfo);
                    }
                }
                return userInfo;
            }
            set
            {
                if (value != null && value.UserType == null)
                {
                    value.UserType = LoginUserType.SKILL_USER;
                }
                this.session[USER_INFO] = JsonConvert.SerializeObject(value);
            }
        }

        #endregion

        #region Case查詢欄位資訊

        /// <summary>
        /// 登入者使用者帳號資訊
        /// </summary>
        public CaseQryModel CaseApply
        {
            get
            {
                CaseQryModel caseInfo = null;
                if (this.session != null)
                {
                    string jsonCaseInfo = (string)this.session[CASE_APP];
                    if (!string.IsNullOrWhiteSpace(jsonCaseInfo))
                    {
                        caseInfo = JsonConvert.DeserializeObject<CaseQryModel>(jsonCaseInfo);
                    }
                }
                return caseInfo;
            }
            set
            {
                
                this.session[CASE_APP] = JsonConvert.SerializeObject(value);
            }
        }

        #endregion


        #region 錯誤訊息及導向

        /// <summary>
        /// 最後被記錄的應用功能錯誤提示訊息, 設定這個值, 在下一個頁面中會觸發 blockAlert() 顯示這個訊息,
        /// 每次這個訊息被讀取後會自動清除, 確保這個訊息只會在一個頁面中被觸發.
        /// </summary>
        public string LastErrorMessage
        {
            get
            {
                string message = (string)this.session[LAST_ERROR_MESSAGE];
                this.session[LAST_ERROR_MESSAGE] = string.Empty;
                return (string.IsNullOrEmpty(message) ? string.Empty : message.Replace("\n", "<br/>").Replace("'", "\""));
            }
            set { this.session[LAST_ERROR_MESSAGE] = value; }
        }

        /// <summary>
        /// 最後被記錄的應用功能操作結果提示訊息, 設定這個值, 在下一個頁面中會觸發 blockResult() 顯示這個訊息,
        /// 每次這個訊息被讀取後會自動清除, 確保這個訊息只會在一個頁面中被觸發.
        /// </summary>
        public string LastResultMessage
        {
            get
            {
                string message = (string)this.session[LAST_RESULT_MESSAGE];
                this.session[LAST_RESULT_MESSAGE] = string.Empty;
                return (string.IsNullOrEmpty(message) ? string.Empty : message.Replace("\n", "<br/>").Replace("'", "\""));
            }
            set { this.session[LAST_RESULT_MESSAGE] = value; }
        }

        /// <summary>
        /// 配合 LastResultMessage 運作，若這個屬性不為空，則在前端 blockResult() 訊息確認後，
        /// 會以 HTTP POST 方式重導至指定 URL。POST 參數可以用 ?parm1=value1&amp;parm2=value2 的方式傳入
        /// </summary>
        public string RedirectUrlAfterBlock
        {
            get
            {
                string url = (string)this.session[REDIRECT_AFTER_BLOCK];
                this.session[REDIRECT_AFTER_BLOCK] = string.Empty;
                return url;
            }
            set { this.session[REDIRECT_AFTER_BLOCK] = value; }
        }

        /// <summary>
        /// 配合 LastResultMessage、LastErrorMessage 運作，若這個屬性不為空，則在前端 blockResult() 訊息確認後， 
        /// 會以 HTTP GET 方式重導至指定 URL。若要傳遞參數時給目的網頁時，可以使用 ?parm1=value1&amp;parm2=value2 方式來設定。
        /// <para>注意！！！ 一律優先使用 RedirectUrlAfterBlock 屬性（使用 HTTP POST 方式）, 除非遇到無法克服的困難才使用這個GET模式。</para>
        /// </summary>
        public string RedirectUrlAfterBlockViaGet
        {
            get
            {
                string url = (string)this.session[REDIRECT_AFTER_BLOCK_2];
                this.session[REDIRECT_AFTER_BLOCK_2] = string.Empty;
                return url;
            }
            set { this.session[REDIRECT_AFTER_BLOCK_2] = value; }
        }

        #endregion               

        // 取得控制器名稱 110.01.28 Alan
        public string WhereController
        {
            get
            {
                string url = "";
                try
                {
                    url = RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current)).Values.Values.FirstOrDefault().TONotNullString();
                }
                catch (Exception ex)
                {
                    logger.Error("session_WhereController failed:" + ex.TONotNullString());
                }
                
                return url;
            }
            set { this.session["Controller"] = value; }
        }

        /// <summary>
        /// 檢測是否分案檢視畫面(唯讀)
        /// </summary>
        public string statusId
        {
            get
            {
                string message = (string)this.session[STATUS_ID];
                this.session[STATUS_ID] = string.Empty;
                return (string.IsNullOrEmpty(message) ? string.Empty : message);
            }
            set { this.session[STATUS_ID] = value; }
        }
    }
}