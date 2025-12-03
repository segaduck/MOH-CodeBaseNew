# 衛福部人民線上申辦系統 - 身份驗證與授權機制詳細範例：Authentication 與 Authorization

## 功能概述

本文詳細說明 e-service 系統中的身份驗證（Authentication）和授權（Authorization）機制。系統採用 ASP.NET Forms Authentication，支援多種登入方式（帳號密碼、自然人憑證、工商憑證、醫事人員憑證、數位身分證），並結合 Session 管理和角色權限控制，提供完整的安全性保護機制。

## 安全性架構概覽

```
e-service 安全性架構：

使用者請求
    ↓
登入方式選擇
    ├─→ 帳號密碼登入
    ├─→ 自然人憑證 (MOICA)
    ├─→ 工商憑證 (MOEACA)
    ├─→ 醫事人員憑證 (HCA)
    └─→ 數位身分證 (NEWEID)
    ↓
身份驗證 (Authentication)
    ↓ 驗證成功
Forms Authentication Ticket
    ↓ 建立 Cookie
Session 管理
    ↓ 儲存使用者資訊
授權檢查 (Authorization)
    ↓ 檢查角色權限
業務邏輯執行
    ↓
回傳結果
```

## 1. 身份驗證機制（Authentication）

### 1.1 多種登入方式

**檔案位置：** `ES/Controllers/LoginController.cs`

#### 1.1.1 憑證登入流程

```csharp
/// <summary>
/// 憑證登入處理
/// 支援自然人憑證、工商憑證、醫事人員憑證、數位身分證
/// </summary>
/// <param name="model">登入模型</param>
/// <returns>登入結果</returns>
[HttpPost]
public ActionResult Index(LoginViewModel model)
{
    LoginDAO dao = new LoginDAO();
    SessionModel sm = SessionModel.Get();
    string s_log1 = "(憑證登入失敗)";

    // 檢查 PIN 碼驗證是否通過
    if (model.Hide_PinVerify.Equals("Ok") &&
        !string.IsNullOrEmpty(model.Hide_cadata) &&
        !string.IsNullOrEmpty(model.Hide_enccert))
    {
        s_log1 = "\n (憑證登入成功)";
        logger.Debug(s_log1);

        Dictionary<string, object> item = new Dictionary<string, object>();
        MemberModel member = null;

        // 驗證憑證資料
        using (SqlConnection conn = DataUtils.GetConnection())
        {
            DataUtils.OpenDbConn(conn);
            MemberAction action = new MemberAction(conn);

            // 準備憑證驗證參數
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("CAData", (model.Hide_cadata ?? ""));
            args.Add("EncCert", (model.Hide_enccert ?? ""));
            args.Add("loginType", (model.Hide_loginType ?? ""));

            // 檢查憑證資料並取得會員資訊
            member = action.CheckCAData(args, ref item);

            conn.Close();
            conn.Dispose();
        }

        // 檢查錯誤訊息
        string message = sm.LastErrorMessage;
        if (!string.IsNullOrEmpty(message))
        {
            sm.LastErrorMessage = message;
            return View("Index", model);
        }

        // 系統未找到會員資料，導向註冊頁面
        if (member == null)
        {
            return IndexReg(model);
        }

        // 建立登入使用者資訊
        LoginUserInfo userInfo = new LoginUserInfo();

        // 登入驗證，取得使用者帳號及權限角色清單資料
        userInfo = dao.LoginValidate(member.Account, null);
        userInfo.LoginIP = HttpContext.Request.UserHostAddress;

        // 設定登入方式
        // 1:自然人憑證 2:工商憑證 3:醫事人員憑證 4:數位身分證
        string s_cardtype = "MEMBER";
        if (model.Hide_loginType.Equals("1")) { s_cardtype = "MOICA"; }
        if (model.Hide_loginType.Equals("2")) { s_cardtype = "MOEACA"; }
        if (model.Hide_loginType.Equals("3")) { s_cardtype = "HCA1"; }
        if (model.Hide_loginType.Equals("4")) { s_cardtype = "NEWEID"; }

        userInfo.LoginAuth = s_cardtype;

        // 將登入者資訊保存在 SessionModel 中
        sm.UserInfo = userInfo;

        // 記錄登入統計
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
                tran.Rollback();
                logger.Error("更新登入統計失敗", ex);
            }
            finally
            {
                conn.Close();
            }
        }

        // 導向會員首頁
        return RedirectToAction("Index", "Home");
    }

    return View("Index", model);
}
```

**程式碼說明：**

1. **PIN 碼驗證**：前端使用憑證元件驗證 PIN 碼
2. **憑證資料檢查**：驗證憑證有效性並取得會員資訊
3. **多種憑證支援**：支援 MOICA、MOEACA、HCA、NEWEID
4. **自動註冊**：未找到會員資料時導向註冊頁面
5. **登入統計**：記錄各種登入方式的使用次數
6. **Session 管理**：將使用者資訊儲存在 Session 中

#### 1.1.2 帳號密碼登入

**檔案位置：** `ES/DataLayers/LoginDAO.cs`

```csharp
/// <summary>
/// 帳號密碼登入驗證
/// </summary>
/// <param name="userNo">帳號</param>
/// <param name="userPwd_encry">密碼（已加密）</param>
/// <returns>登入使用者資訊</returns>
public LoginUserInfo LoginValidate(string userNo, string userPwd_encry)
{
    LoginUserInfo userInfo = new LoginUserInfo();
    userInfo.UserNo = userNo;
    userInfo.LoginSuccess = false;

    // 準備查詢參數
    var dictionary = new Dictionary<string, object>
    {
        { "@ACC_NO", userNo },
        { "@PSWD", userPwd_encry }
    };
    var parameters = new DynamicParameters(dictionary);

    // 查詢會員資料
    TblMEMBER result = null;
    string _sql = @"
        SELECT *
        FROM MEMBER
        WHERE 1 = 1
            AND ISNULL(DEL_MK,'N') = 'N'
            AND ACC_NO = @ACC_NO";

    if (!string.IsNullOrEmpty(userPwd_encry))
    {
        _sql += " AND PSWD = @PSWD";
    }

    using (SqlConnection conn = DataUtils.GetConnection())
    {
        try
        {
            result = conn.QueryFirst<TblMEMBER>(_sql, parameters);
        }
        catch (Exception ex)
        {
            logger.Warn(ex.Message, ex);
            result = null;
        }
        finally
        {
            conn.Close();
            conn.Dispose();
        }
    }

    // 驗證失敗
    if (result == null)
    {
        var serviceTel = DataUtils.GetConfig("SERVICETEL");
        userInfo.LoginErrMessage =
            "帳號或密碼錯誤! 帳號登入失敗若達五次將被鎖定，請諮詢系統操作服務諮詢電話：" + serviceTel;
        return userInfo;
    }

    // 檢查帳號狀態
    if (result.STATUS_CD == "L")
    {
        userInfo.LoginErrMessage = "您的帳號已被鎖定，請聯絡系統管理員";
        return userInfo;
    }

    if (result.STATUS_CD == "S")
    {
        userInfo.LoginErrMessage = "您的帳號已被停用，請聯絡系統管理員";
        return userInfo;
    }

    // 登入成功
    userInfo.LoginSuccess = true;
    userInfo.Member = new ClamMember();
    userInfo.Member.InjectFrom(result);

    return userInfo;
}
```

**程式碼說明：**

1. **參數化查詢**：使用 Dapper 參數化查詢防止 SQL Injection
2. **密碼加密**：密碼已在前端或控制器加密
3. **帳號狀態檢查**：檢查帳號是否被鎖定或停用
4. **錯誤訊息**：提供明確的錯誤訊息
5. **物件對應**：使用 ValueInjecter 進行物件對應

### 1.2 Forms Authentication 實作

#### 1.2.1 建立 Authentication Ticket

**檔案位置：** `ES/Areas/BACKMIN/Controllers/LoginController.cs`

```csharp
/// <summary>
/// 建立 Forms Authentication Ticket
/// 用於管理者登入
/// </summary>
/// <param name="account">帳號</param>
/// <param name="roles">角色列表</param>
/// <param name="isPersistent">是否持久化 Cookie</param>
private void CreateAuthTicket(string account, List<string> roles, bool isPersistent)
{
    // 取得登入逾時設定
    int timeout = Int32.Parse(DataUtils.GetConfig("LOGIN_TIMEOUT"));

    // 準備使用者資料
    Dictionary<string, string> userData = new Dictionary<string, string>();
    userData.Add("Id", account);
    userData.Add("AdminAccount", account);
    userData.Add("Roles", DataUtils.StringArrayToString(roles.ToArray(), ","));

    // 建立 Forms Authentication Ticket
    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
        1,                                              // 版本
        userData["Id"],                                 // 使用者名稱
        DateTime.Now,                                   // 發行時間
        DateTime.Now.AddMinutes(timeout),               // 過期時間
        isPersistent,                                   // 是否持久化
        DataUtils.DictionaryToJsonString(userData)     // 使用者資料（JSON 格式）
    );

    // 加密 Ticket
    string encTicket = FormsAuthentication.Encrypt(ticket);

    // 建立或更新 Cookie
    HttpCookie cookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
    if (cookie == null)
    {
        cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
    }

    // 設定 Cookie 過期時間
    if (isPersistent)
    {
        cookie.Expires = ticket.Expiration;
    }

    // 設定 Cookie 值並加入回應
    cookie.Value = encTicket;
    HttpContext.Response.AppendCookie(cookie);
}
```

**程式碼說明：**

1. **FormsAuthenticationTicket**：ASP.NET Forms 驗證票證
2. **使用者資料**：將角色等資訊儲存在 UserData 中
3. **加密處理**：使用 FormsAuthentication.Encrypt 加密票證
4. **Cookie 管理**：將加密後的票證儲存在 Cookie 中
5. **持久化選項**：支援記住我功能

#### 1.2.2 驗證 Authentication Ticket

**檔案位置：** `ES/Global.asax.cs`

```csharp
/// <summary>
/// 應用程式驗證請求事件
/// 從 Forms Authentication Ticket 中提取角色資訊
/// </summary>
protected void Application_AuthenticateRequest(Object sender, EventArgs e)
{
    // 檢查使用者是否已驗證
    if (User != null && User.Identity.IsAuthenticated && User.Identity is FormsIdentity)
    {
        // 取得 Forms Identity
        FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
        FormsAuthenticationTicket ticket = id.Ticket;

        // 從 Ticket 中取得使用者資料
        Dictionary<string, string> userData =
            DataUtils.JsonStringToDictionary(ticket.UserData);

        if (userData != null && userData.ContainsKey("Roles"))
        {
            // 提取角色資訊
            string[] roles = userData["Roles"].Split(',');

            // 建立 GenericPrincipal 並設定角色
            HttpContext.Current.User = new GenericPrincipal(id, roles);
        }
    }
}
```

**程式碼說明：**

1. **全域事件**：在每個請求時執行
2. **Ticket 解析**：從 Cookie 中解析 Forms Authentication Ticket
3. **角色提取**：從 UserData 中提取角色資訊
4. **Principal 設定**：建立 GenericPrincipal 並設定角色
5. **授權基礎**：為後續的授權檢查提供基礎

## 2. Session 管理

### 2.1 SessionModel 設計

**檔案位置：** `ES/Models/SessionModel.cs`

#### 2.1.1 Session 模型定義

```csharp
/// <summary>
/// Session 管理模型
/// 封裝 ASP.NET Session 操作
/// </summary>
public class SessionModel
{
    protected static readonly ILog logger = LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

    /// <summary>
    /// 私有建構子，防止外部直接建立實例
    /// </summary>
    private SessionModel()
    {
        if (HttpContext.Current != null)
        {
            this._session = new HttpSessionStateWrapper(HttpContext.Current.Session);
            if (this._session == null)
            {
                throw new NullReferenceException("HttpContext.Current.Session");
            }

            // 設定 Session 逾時時間（分鐘）
            _session.Timeout = 20;
            logger.Debug("SessionModel(), SessionID=" + _session.SessionID);
        }
        else
        {
            logger.Info("SessionModel(), HttpContext.Current is null");
        }
    }

    /// <summary>
    /// 取得 SessionModel 實例
    /// </summary>
    public static SessionModel Get()
    {
        return new SessionModel();
    }

    // Session 鍵值常數定義
    private static readonly string USER_INFO = "SYS.LOGIN.USER";
    private static readonly string VALIDATE_CODE = "SYS.LOGIN.VALIDATECODE";
    private static readonly string LAST_ERROR_MESSAGE = "USER.LAST_ERROR_MESSAGE";
    private static readonly string LAST_RESULT_MESSAGE = "USER.LAST_RESULT_MESSAGE";

    /// <summary>
    /// 登入者使用者帳號資訊
    /// </summary>
    public LoginUserInfo UserInfo
    {
        get
        {
            LoginUserInfo userInfo = null;
            if (this.session != null)
            {
                string jsonUserInfo = (string)this.session[USER_INFO];
                if (!string.IsNullOrWhiteSpace(jsonUserInfo))
                {
                    // 從 JSON 反序列化使用者資訊
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
            // 序列化為 JSON 儲存
            this.session[USER_INFO] = JsonConvert.SerializeObject(value);
        }
    }

    /// <summary>
    /// 使用者登入驗證碼
    /// </summary>
    public string LoginValidateCode
    {
        get { return (string)this.session[VALIDATE_CODE]; }
        set { this.session[VALIDATE_CODE] = value; }
    }

    /// <summary>
    /// 最後被記錄的錯誤訊息
    /// 讀取後自動清除
    /// </summary>
    public string LastErrorMessage
    {
        get
        {
            string message = (string)this.session[LAST_ERROR_MESSAGE];
            this.session[LAST_ERROR_MESSAGE] = string.Empty;
            return (string.IsNullOrEmpty(message) ? string.Empty :
                message.Replace("\n", "<br/>").Replace("'", "\""));
        }
        set { this.session[LAST_ERROR_MESSAGE] = value; }
    }

    /// <summary>
    /// 最後被記錄的結果訊息
    /// 讀取後自動清除
    /// </summary>
    public string LastResultMessage
    {
        get
        {
            string message = (string)this.session[LAST_RESULT_MESSAGE];
            this.session[LAST_RESULT_MESSAGE] = string.Empty;
            return (string.IsNullOrEmpty(message) ? string.Empty :
                message.Replace("\n", "<br/>").Replace("'", "\""));
        }
        set { this.session[LAST_RESULT_MESSAGE] = value; }
    }
}
```

**程式碼說明：**

1. **單例模式**：使用靜態方法 Get() 取得實例
2. **JSON 序列化**：使用 JSON 儲存複雜物件
3. **自動清除**：錯誤訊息讀取後自動清除
4. **逾時設定**：設定 Session 逾時時間
5. **日誌記錄**：記錄 Session 建立和操作

## 3. 授權機制（Authorization）

### 3.1 BaseController 登入檢查

**檔案位置：** `ES/Controllers/BaseController.cs`

#### 3.1.1 取得登入使用者資訊

```csharp
/// <summary>
/// 基礎控制器
/// 提供共用的資料庫連線和使用者資訊取得方法
/// </summary>
public class BaseController : Controller
{
    protected static readonly ILog logger = LogUtils.GetLogger();

    /// <summary>
    /// 取得目前登入的帳號
    /// </summary>
    /// <returns>帳號</returns>
    protected string GetAccount()
    {
        if (Request.IsAuthenticated)
        {
            // 從 Forms Authentication Ticket 取得使用者資料
            FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
            FormsAuthenticationTicket ticket = id.Ticket;
            Dictionary<string, string> userData =
                DataUtils.JsonStringToDictionary(ticket.UserData);

            // 優先取得會員帳號
            if (userData.ContainsKey("MemberAccount"))
            {
                return userData["MemberAccount"];
            }
        }
        else
        {
            // 測試環境支援
            if (!String.IsNullOrEmpty(Request["TestAccount"]))
            {
                return Request["TestAccount"];
            }
        }

        return "";
    }

    /// <summary>
    /// 取得使用者完整資料
    /// </summary>
    /// <returns>使用者資料字典</returns>
    protected Dictionary<string, string> GetUserData()
    {
        if (Request.IsAuthenticated)
        {
            FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
            FormsAuthenticationTicket ticket = id.Ticket;
            Dictionary<string, string> userData =
                DataUtils.JsonStringToDictionary(ticket.UserData);
            return userData;
        }
        return null;
    }

    /// <summary>
    /// 取得資料庫連線
    /// </summary>
    protected SqlConnection GetConnection()
    {
        string connectionString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        SqlConnection conn = new SqlConnection(connectionString);
        return conn;
    }
}
```

**程式碼說明：**

1. **Forms Identity**：從 Forms Authentication 取得使用者身份
2. **UserData 解析**：從 Ticket 的 UserData 取得使用者資訊
3. **測試支援**：開發環境支援測試帳號
4. **共用方法**：提供給所有繼承的控制器使用

### 3.2 角色授權檢查

#### 3.2.1 使用 Authorize 屬性

```csharp
/// <summary>
/// 管理者專區控制器
/// 使用 Authorize 屬性限制只有 Admin 角色可存取
/// </summary>
[Authorize(Roles = "Admin")]
public class ReportController : BaseController
{
    /// <summary>
    /// 申辦統計報表
    /// 只有管理員可存取
    /// </summary>
    [HttpGet]
    public ActionResult CaseSum()
    {
        CaseSumModel model = new CaseSumModel();
        model.Sdate = DateTime.Now.ToString("yyyy/MM/dd");
        model.Fdate = DateTime.Now.ToString("yyyy/MM/dd");

        // 記錄訪問記錄
        this.SetVisitRecord("Report", "CaseSum", "申辦統計");

        return View(model);
    }
}
```

**程式碼說明：**

1. **Authorize 屬性**：ASP.NET MVC 內建的授權屬性
2. **角色限制**：限制只有特定角色可存取
3. **自動檢查**：框架自動檢查使用者角色
4. **未授權處理**：未授權時自動導向登入頁面

#### 3.2.2 程式碼中的權限檢查

```csharp
/// <summary>
/// 檢查使用者是否有特定權限
/// </summary>
/// <param name="permission">權限代碼</param>
/// <returns>是否有權限</returns>
protected bool CheckPermission(string permission)
{
    SessionModel sm = SessionModel.Get();

    // 檢查是否已登入
    if (sm.UserInfo == null)
    {
        return false;
    }

    // 檢查是否為管理員
    if (User.IsInRole("Admin"))
    {
        return true;
    }

    // 檢查特定權限
    // 這裡可以從資料庫查詢使用者的權限清單
    using (SqlConnection conn = GetConnection())
    {
        conn.Open();

        string sql = @"
            SELECT COUNT(*)
            FROM ADMIN_MENU AM
                INNER JOIN ADMIN A ON AM.ACC_NO = A.ACC_NO
            WHERE A.ACC_NO = @ACC_NO
                AND AM.MENU_CD = @PERMISSION
                AND A.DEL_MK = 'N'
                AND AM.DEL_MK = 'N'";

        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ACC_NO", sm.UserInfo.UserNo);
        cmd.Parameters.AddWithValue("@PERMISSION", permission);

        int count = (int)cmd.ExecuteScalar();
        conn.Close();

        return count > 0;
    }
}
```

**程式碼說明：**

1. **Session 檢查**：先檢查使用者是否已登入
2. **管理員特權**：管理員擁有所有權限
3. **資料庫查詢**：從資料庫查詢使用者權限
4. **彈性控制**：可在程式碼中靈活控制權限

## 4. 登出機制

### 4.1 登出處理

**檔案位置：** `ES/Areas/BACKMIN/Controllers/LogoutController.cs`

```csharp
/// <summary>
/// 登出控制器
/// </summary>
public class LogoutController : BaseController
{
    /// <summary>
    /// 登出處理
    /// </summary>
    [HttpGet]
    public ActionResult Index()
    {
        try
        {
            // 取得使用者資料
            Dictionary<string, string> userData = GetUserData();

            if (userData != null && userData.ContainsKey("AdminAccount"))
            {
                // 管理員登出，切換回一般會員身份
                List<string> roles = new List<string>();
                roles.Add("Member");

                // 移除管理員帳號
                userData.Remove("AdminAccount");
                userData["Roles"] = DataUtils.StringArrayToString(roles.ToArray(), ",");

                // 重新建立 Authentication Ticket
                bool isPersistent = DataUtils.GetConfig("LOGIN_PERSISTENT_MK").Equals("Y");
                int timeout = Int32.Parse(DataUtils.GetConfig("LOGIN_TIMEOUT"));

                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1,
                    userData["Id"],
                    DateTime.Now,
                    DateTime.Now.AddMinutes(timeout),
                    isPersistent,
                    DataUtils.DictionaryToJsonString(userData)
                );

                string encTicket = FormsAuthentication.Encrypt(ticket);

                HttpCookie cookie = HttpContext.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie == null)
                {
                    cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
                }

                if (isPersistent)
                {
                    cookie.Expires = ticket.Expiration;
                }

                cookie.Value = encTicket;
                HttpContext.Response.AppendCookie(cookie);

                // 導向會員首頁
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            else
            {
                // 一般會員登出
                // 清除 Session
                SessionModel sm = SessionModel.Get();
                sm.UserInfo = null;

                // 登出 Forms Authentication
                FormsAuthentication.SignOut();

                // 導向登入頁面
                return RedirectToAction("Index", "Login", new { area = "" });
            }
        }
        catch (Exception ex)
        {
            logger.Error("登出失敗", ex);
            return RedirectToAction("Index", "Login", new { area = "" });
        }
    }
}
```

**程式碼說明：**

1. **雙重身份**：支援管理員切換回會員身份
2. **Session 清除**：清除 Session 中的使用者資訊
3. **Cookie 清除**：使用 FormsAuthentication.SignOut() 清除 Cookie
4. **安全導向**：登出後導向登入頁面

## 5. 安全性最佳實務

### 5.1 密碼安全

```csharp
/// <summary>
/// 密碼加密
/// 使用 SHA256 雜湊演算法
/// </summary>
public static string EncryptPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        // 將密碼轉換為位元組陣列
        byte[] bytes = Encoding.UTF8.GetBytes(password);

        // 計算雜湊值
        byte[] hash = sha256.ComputeHash(bytes);

        // 轉換為十六進位字串
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            result.Append(hash[i].ToString("x2"));
        }

        return result.ToString();
    }
}
```

### 5.2 防止暴力破解

```csharp
/// <summary>
/// 檢查登入失敗次數
/// 達到上限時鎖定帳號
/// </summary>
public bool CheckLoginFailedCount(string account)
{
    using (SqlConnection conn = DataUtils.GetConnection())
    {
        conn.Open();

        // 查詢最近 15 分鐘內的登入失敗次數
        string sql = @"
            SELECT COUNT(*)
            FROM LOGIN_FAILED_LOG
            WHERE ACC_NO = @ACC_NO
                AND FAILED_TIME >= DATEADD(MINUTE, -15, GETDATE())";

        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ACC_NO", account);

        int failedCount = (int)cmd.ExecuteScalar();

        // 失敗次數達到 5 次，鎖定帳號
        if (failedCount >= 5)
        {
            string updateSql = @"
                UPDATE MEMBER
                SET STATUS_CD = 'L',
                    LOCK_TIME = GETDATE()
                WHERE ACC_NO = @ACC_NO";

            SqlCommand updateCmd = new SqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@ACC_NO", account);
            updateCmd.ExecuteNonQuery();

            conn.Close();
            return false;
        }

        conn.Close();
        return true;
    }
}
```

### 5.3 Session 逾時處理

```csharp
/// <summary>
/// 檢查 Session 是否逾時
/// </summary>
public ActionResult CheckSessionTimeout()
{
    SessionModel sm = SessionModel.Get();

    if (sm.UserInfo == null)
    {
        // Session 已逾時，導向登入頁面
        return RedirectToAction("Index", "Login");
    }

    // Session 有效，繼續處理
    return null;
}
```

### 5.4 HTTPS 強制使用

**Web.config 設定：**

```xml
<system.web>
    <authentication mode="Forms">
        <forms
            loginUrl="~/Login/Index"
            timeout="20"
            slidingExpiration="true"
            requireSSL="true"
            cookieless="UseCookies"
            protection="All" />
    </authentication>
</system.web>
```

**程式碼說明：**

1. **requireSSL**：要求使用 HTTPS
2. **protection="All"**：加密和驗證 Cookie
3. **slidingExpiration**：滑動過期時間
4. **cookieless="UseCookies"**：使用 Cookie 儲存票證

## 6. 總結

e-service 系統的身份驗證與授權機制提供了完整的安全性保護：

### 6.1 身份驗證特色

- **多種登入方式**：支援帳號密碼、憑證登入
- **Forms Authentication**：使用 ASP.NET 內建驗證機制
- **Session 管理**：完整的 Session 生命週期管理
- **安全加密**：密碼加密、Cookie 加密

### 6.2 授權機制特色

- **角色授權**：基於角色的存取控制
- **Authorize 屬性**：宣告式授權檢查
- **程式碼檢查**：彈性的程式碼授權檢查
- **權限管理**：資料庫驅動的權限管理

### 6.3 安全性特色

- **防暴力破解**：登入失敗次數限制
- **Session 逾時**：自動逾時保護
- **HTTPS 強制**：強制使用 HTTPS
- **日誌記錄**：完整的操作日誌

這套身份驗證與授權機制為系統提供了多層次的安全性保護，確保只有授權的使用者才能存取相應的功能。
