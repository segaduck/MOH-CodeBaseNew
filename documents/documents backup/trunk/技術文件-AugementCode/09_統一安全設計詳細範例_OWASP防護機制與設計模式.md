# EECOnline 電子病歷申請系統 - 統一安全設計詳細範例：OWASP 防護機制與設計模式

## 功能概述

本文詳細分析 EECOnline 系統的安全設計架構，說明如何透過設計模式和安全機制來防護 OWASP Top 10 安全弱點。系統採用 ASP.NET MVC 架構，包含身份驗證、授權控制、輸入驗證、檔案上傳安全等機制。

**重要說明：** 本文件將誠實呈現系統現有的安全機制，並明確標註缺少的 OWASP Top 10 防護項目，不會虛構不存在的安全功能。

## 安全設計架構概覽

```
EECOnline 安全設計架構：

前端安全層
    ↓ Razor 自動編碼、輸入驗證
中介軟體安全層
    ↓ LoginRequired Filter、CustomAuthAttribute
控制器安全層
    ↓ 授權檢查、Session 驗證
服務層安全層
    ↓ 業務邏輯驗證、密碼加密
資料存取安全層
    ↓ IBatis 參數化查詢、SQL 注入防護
日誌與監控層
    ↓ log4net 記錄、登入失敗追蹤
```

## OWASP Top 10 (2021) 防護現況總覽

| OWASP Top 10 項目                                  | 防護狀態 | 實作位置                      |
| -------------------------------------------------- | -------- | ----------------------------- |
| A01 - 存取控制失效 (Broken Access Control)         | ✓ 已實作 | CustomAuthAttribute.cs        |
| A02 - 加密機制失效 (Cryptographic Failures)        | ⚠ 部分   | ClamServices.cs (使用 RSACSP) |
| A03 - 注入攻擊 (Injection)                         | ✓ 已實作 | IBatis 參數化查詢             |
| A04 - 不安全設計 (Insecure Design)                 | ⚠ 部分   | 缺乏統一驗證框架              |
| A05 - 安全設定缺陷 (Security Misconfiguration)     | ✗ 缺少   | 無 CSP、安全標頭              |
| A06 - 易受攻擊元件 (Vulnerable Components)         | ⚠ 部分   | 使用舊版 .NET Framework       |
| A07 - 身份驗證失效 (Authentication Failures)       | ✓ 已實作 | 登入失敗鎖定機制              |
| A08 - 軟體資料完整性失效 (Integrity Failures)      | ⚠ 部分   | 檔案上傳驗證                  |
| A09 - 安全日誌監控失效 (Logging Failures)          | ✓ 已實作 | log4net + 登入記錄            |
| A10 - 伺服器端請求偽造 (Server-Side Request Forge) | ✗ 缺少   | 無 SSRF 防護                  |

## 1. A01 - 存取控制失效 (Broken Access Control) 防護 ✓

### 1.1 統一身份驗證與授權機制

**檔案位置：** `Commons/Filter/CustomAuthAttribute.cs`

#### 1.1.1 LoginRequired 登入檢查 Filter

```csharp
/// <summary>
/// 同時判斷登入狀態及角色執行權限的 AuthorizeAttribute
/// 實作 RBAC 模型，防止存取控制失效
/// </summary>
public class LoginRequired : AuthorizeAttribute
{
    /// <summary>
    /// 預設系統登入頁
    /// </summary>
    public static string LOGIN_PAGE = "~/Login/C101M";

    /// <summary>
    /// 沒有權限的訊息頁面
    /// </summary>
    public static string UNAUTH_PAGE = "~/ErrorPage/UnAuth";

    private static readonly ILog LOG = LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// LoginRequired 登入 Session 檢核
    /// </summary>
    /// <param name="filterContext"></param>
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        // 第一步：取得當前請求的 Action 路徑
        string actionPath = ControllerContextHelper.GetActionPath(filterContext);
        string verb = filterContext.HttpContext.Request.HttpMethod;

        // 第二步：取得功能路徑（Area/Controller）
        string funcPath = actionPath;
        string[] tokens = actionPath.Split('/');
        if (tokens.Length >= 3)
        {
            funcPath = tokens[0] + "/" + tokens[1];
        }

        // 第三步：檢查是否允許匿名存取
        bool allowAnonymous = filterContext.ActionDescriptor.IsDefined(
            typeof(AllowAnonymousAttribute), true);
        bool isByPassAuth = filterContext.ActionDescriptor.IsDefined(
            typeof(BypassAuthorizeAttribute), true);

        // 第四步：取得 Session 資料
        SessionModel sm = SessionModel.Get();

        // 第五步：檢查使用者是否已登入
        string userNo = (sm.UserInfo != null) ? sm.UserInfo.UserNo : null;
        LOG.Info("OnAuthorization[" + userNo + "] " + verb + " " + actionPath +
                 " (allowAnonymous=" + allowAnonymous + ", isByPassAuth=" + isByPassAuth + ")");

        bool isLogin = false;
        if (sm.UserInfo != null
            && (sm.UserInfo.UserType.Equals(this.Roles)
                || (string.IsNullOrEmpty(this.Roles)
                    && sm.UserInfo.UserType.Equals(LoginUserType.SKILL_USER))))
        {
            isLogin = true;
        }

        // 第六步：未登入處理
        if (!isLogin)
        {
            if (!allowAnonymous)
            {
                // 根據 LoginRequired.Roles 決定登入頁面
                string loginPage = LOGIN_PAGE;
                LOG.Info("OnAuthorization: redirect to Login page: " + loginPage);

                // 儲存重導向路徑
                filterContext.Controller.TempData["RedirectPath"] = actionPath;
                filterContext.Controller.TempData["RedirectApyId"] =
                    HttpContext.Current.Request.QueryString["apy_id"];
                filterContext.Controller.TempData["RedirectGUID"] =
                    HttpContext.Current.Request.QueryString["GUID"];

                filterContext.Result = new RedirectResult(loginPage);
            }
        }
        else
        {
            // 第七步：已登入，檢查權限
            if (!allowAnonymous && !isByPassAuth)
            {
                // 取得使用者的角色功能清單
                IList<ClamRoleFunc> roleFuncs = sm.UserInfo.RoleFuncs;
                bool isAuth = false;

                // 第八步：檢查使用者是否有權限執行此功能
                if (roleFuncs != null)
                {
                    foreach (ClamRoleFunc roleFunc in roleFuncs)
                    {
                        if (funcPath.Equals(roleFunc.prgid2, StringComparison.OrdinalIgnoreCase))
                        {
                            isAuth = true;
                            break;
                        }
                    }
                }

                // 第九步：權限檢查結果處理
                if (!isAuth)
                {
                    string disableAuth = ConfigModel.DisableAuth;

                    if ("1".Equals(disableAuth)
                        || ("2".Equals(disableAuth) && ConfigModel.Admin.Equals(sm.UserInfo.UserNo)))
                    {
                        // 停用權限檢核（測試環境）
                        LOG.Info("OnAuthorization[" + sm.UserInfo.UserNo + "] " + verb + " " +
                                 sm.LastActionPath + ", -- AUTHORIZATION CHECK DISABLED --");
                    }
                    else
                    {
                        // 使用者試圖執行未授權的 Action，導向 UnAuth 頁面
                        LOG.Info("UNAUTHORIZED [" + sm.UserInfo.UserNo + "] " + verb + " " +
                                 sm.LastActionPath + ", redirect to UNAUTH_PAGE page");
                        string funcName = (sm.LastActionFunc != null) ?
                            " " + sm.LastActionFunc.prgname + "(" + sm.LastActionFunc.prgid + ") " : "";
                        sm.LastErrorMessage = "您沒有執行" + funcName + "權限!";
                        filterContext.Result = new RedirectResult(UNAUTH_PAGE);

                        //TODO: 寫入未授權執行記錄
                    }
                }
            }
        }

        return;
    }
}
```

**程式碼說明：**

1. **分層驗證**：先驗證身份（Authentication），再檢查授權（Authorization）
2. **Session 管理**：使用 ASP.NET Session 儲存使用者資訊
3. **RBAC 模型**：透過角色和功能的對應關係實現細緻權限控制
4. **匿名存取**：支援 `[AllowAnonymous]` 屬性標記公開頁面
5. **日誌記錄**：記錄所有授權檢查結果，便於監控和分析
6. **測試模式**：支援停用權限檢核，方便開發測試

#### 1.1.2 Session 管理機制

**檔案位置：** `Models/SessionModel.cs`

```csharp
/// <summary>
/// Session 管理模型
/// 管理使用者登入狀態與資訊
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
                throw new NullReferenceException("session object is null");
            }
            return _session;
        }
    }

    private SessionModel()
    {
        // 第一步：取得 HttpContext.Current.Session
        this._session = new HttpSessionStateWrapper(HttpContext.Current.Session);
        if (this._session == null)
        {
            throw new NullReferenceException("HttpContext.Current.Session");
        }

        // 第二步：設定 Session 逾時時間為 60 分鐘
        _session.Timeout = 60;
        logger.Debug("SessionModel(), SessionID=" + _session.SessionID);
    }

    /// <summary>
    /// 取得 SessionModel 單例
    /// </summary>
    public static SessionModel Get()
    {
        return new SessionModel();
    }

    /// <summary>
    /// 登入者使用者帳號資訊
    /// </summary>
    public LoginUserInfo UserInfo
    {
        get
        {
            LoginUserInfo userInfo = null;
            string jsonUserInfo = (string)this.session[USER_INFO];
            if (!string.IsNullOrWhiteSpace(jsonUserInfo))
            {
                // 從 JSON 反序列化使用者資訊
                userInfo = JsonConvert.DeserializeObject<LoginUserInfo>(jsonUserInfo);
            }
            return userInfo;
        }
        set
        {
            if (value != null && value.UserType == null)
            {
                value.UserType = LoginUserType.SKILL_USER;
            }
            // 序列化為 JSON 儲存到 Session
            this.session[USER_INFO] = JsonConvert.SerializeObject(value);
        }
    }
}
```

**程式碼說明：**

1. **Session 封裝**：封裝 ASP.NET Session，提供統一的存取介面
2. **逾時控制**：設定 Session 逾時時間為 60 分鐘
3. **JSON 序列化**：使用 JSON 序列化儲存複雜物件
4. **單例模式**：每次呼叫 `Get()` 取得新的 SessionModel 實例
5. **錯誤處理**：檢查 Session 是否為 null，避免 NullReferenceException

## 2. A02 - 加密機制失效 (Cryptographic Failures) 防護 ⚠

### 2.1 密碼加密機制

**檔案位置：** `Services/ClamServices.cs`

**⚠ 安全警告：** 系統目前使用 RSACSP 加密，這是可逆加密，不符合密碼儲存的最佳實務。程式碼中已標註 TODO 需要改用不可逆的 Hash 方法（如 SHA-256、SHA-512 或 bcrypt）。

#### 2.1.1 目前的密碼加密實作（需改進）

```csharp
/// <summary>
/// 傳入使用者輸入的密碼明文，加密後回傳
/// ⚠ 警告：目前使用 RSACSP 可逆加密，需改用不可逆 Hash
/// </summary>
/// <param name="userPwd">使用者密碼</param>
/// <returns>加密後的密碼</returns>
private string EncPassword(string userPwd)
{
    if (string.IsNullOrWhiteSpace(userPwd))
    {
        throw new ArgumentNullException("userPwd");
    }

    //TODO: 置換 RSACSP 改成不可逆的 Hash 方法
    RSACSP.RSACSP rsa = new RSACSP.RSACSP();
    return rsa.Utl_Encrypt(userPwd);
}

/// <summary>
/// 使用 SHA-512 進行密碼雜湊（建議使用）
/// 此方法在部分模組中使用
/// </summary>
/// <param name="pwd">密碼明文</param>
/// <returns>SHA-512 雜湊值</returns>
public static string CypherText(string pwd)
{
    // 第一步：建立 SHA-512 雜湊演算法實例
    SHA512 sha = new SHA512CryptoServiceProvider();

    // 第二步：將密碼轉換為 UTF-8 位元組陣列
    byte[] source = Encoding.Default.GetBytes(pwd);

    // 第三步：計算雜湊值
    byte[] crypto = sha.ComputeHash(source);

    // 第四步：轉換為 Base64 字串
    string result = Convert.ToBase64String(crypto);

    return result;
}
```

**程式碼說明：**

1. **可逆加密問題**：RSACSP 是可逆加密，如果私鑰洩漏，所有密碼都會被破解
2. **SHA-512 雜湊**：部分模組使用 SHA-512，這是不可逆的雜湊函數
3. **缺少 Salt**：SHA-512 實作沒有使用 Salt，容易受到彩虹表攻擊
4. **建議改進**：應全面改用 bcrypt、PBKDF2 或 Argon2 等專門的密碼雜湊演算法

#### 2.1.2 AES 加密實作（用於資料傳輸）

**檔案位置：** `Utils/Hospital_FarEastern_Code.cs`

```csharp
/// <summary>
/// AES 加密方法
/// 用於敏感資料的加密傳輸
/// </summary>
/// <param name="original">原始字串</param>
/// <param name="key">加密金鑰</param>
/// <param name="iv">初始化向量</param>
/// <returns>加密後的 Base64 字串</returns>
public static string AesEncrypt(string original, string key, string iv)
{
    string encrypt = "";
    try
    {
        // 第一步：建立 AES 加密服務提供者
        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();

        // 第二步：使用 SHA-256 處理金鑰
        byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));

        // 第三步：使用 MD5 處理初始化向量
        byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(iv));

        // 第四步：將原始字串轉換為位元組陣列
        byte[] dataByteArray = Encoding.UTF8.GetBytes(original);

        // 第五步：使用 MemoryStream 和 CryptoStream 進行加密
        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms,
                   aes.CreateEncryptor(keyData, ivData), CryptoStreamMode.Write))
            {
                cs.Write(dataByteArray, 0, dataByteArray.Length);
                cs.FlushFinalBlock();

                // 第六步：轉換為 Base64 字串
                encrypt = Convert.ToBase64String(ms.ToArray());
            }
        }
    }
    catch (Exception ex)
    {
        //TODO: 應記錄錯誤日誌
    }

    return encrypt;
}
```

**程式碼說明：**

1. **AES 加密**：使用 AES 對稱加密演算法，適合資料傳輸加密
2. **金鑰處理**：使用 SHA-256 處理金鑰，確保金鑰長度符合 AES 要求
3. **IV 處理**：使用 MD5 處理初始化向量
4. **記憶體安全**：使用 `using` 語句確保資源正確釋放
5. **錯誤處理**：捕獲異常但未記錄日誌（需改進）

### 2.2 加密機制改進建議

**缺少的安全機制：**

1. ✗ **密碼 Salt**：密碼雜湊未使用 Salt，容易受到彩虹表攻擊
2. ✗ **密碼強度檢查**：缺少統一的密碼強度驗證機制
3. ✗ **金鑰管理**：AES 金鑰硬編碼在程式碼中，應使用金鑰管理服務
4. ✗ **HTTPS 強制**：未在 Global.asax.cs 中強制使用 HTTPS
5. ⚠ **密碼演算法**：部分模組仍使用可逆加密 RSACSP

## 3. A03 - 注入攻擊 (Injection) 防護 ✓

### 3.1 SQL 注入防護機制

**檔案位置：** `SqlMaps/*.xml` (IBatis 映射檔案)

#### 3.1.1 IBatis 參數化查詢實作

```xml
<!-- 檔案位置：SqlMaps/KeyMap.xml -->
<!-- 取得系統功能清單 -->
<select id="getSYSID" resultClass="Turbo.DataLayer.KeyMapModel" cacheModel="EECOnline-cache">
  <![CDATA[
    SELECT sysid CODE, sysname TEXT
    FROM   AMFUNCM
    WHERE  1 = 1
    AND    modules = ' '
    AND    status = '1'
    ORDER BY orderby, sysid
  ]]>
</select>

<!-- 取得模組功能清單（帶參數） -->
<select id="getMODULES" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="EECOnline-cache">
  <![CDATA[
    SELECT modules CODE, modulesname TEXT
    FROM   AMFUNCM
    WHERE  1 = 1
    AND    sysid = #sysid#
    AND    modules <> ' '
    AND    status = '1'
    ORDER BY orderby, modules
  ]]>
</select>

<!-- 取得申辦項目清單（動態條件） -->
<select id="getApply" resultClass="Turbo.DataLayer.KeyMapModel" parameterClass="Hashtable" cacheModel="EECOnline-cache">
  <![CDATA[
    SELECT code_cd CODE, code_name TEXT
    FROM   CODE
    WHERE  1 = 1
    AND    code_type = 'apply'
  ]]>
  <dynamic>
    <isNotEmpty property="srv_cd">
      AND code_cd IN (
        SELECT apply_no
        FROM   EEC_Apply_Item
        WHERE  srv_cd = #srv_cd#
      )
    </isNotEmpty>
  </dynamic>
  <![CDATA[
    AND status = '1'
    ORDER BY orderby, code_cd
  ]]>
</select>
```

**程式碼說明：**

1. **參數化查詢**：使用 `#參數名#` 語法，IBatis 自動處理參數化
2. **動態 SQL**：使用 `<dynamic>` 和 `<isNotEmpty>` 標籤安全地建構動態查詢
3. **快取機制**：使用 `cacheModel="EECOnline-cache"` 提升效能
4. **SQL 注入防護**：所有使用者輸入都透過參數化查詢傳遞，防止 SQL 注入
5. **CDATA 區塊**：使用 `<![CDATA[]]>` 包裹 SQL，避免 XML 特殊字元問題

#### 3.1.2 Dapper 參數化查詢實作

**檔案位置：** `DataLayers/LoginDAO.cs`

```csharp
/// <summary>
/// 使用者登入驗證
/// 使用 Dapper 參數化查詢防止 SQL 注入
/// </summary>
/// <param name="userNo">使用者帳號</param>
/// <param name="userPwd">加密後的密碼</param>
/// <returns>使用者資訊</returns>
public LoginUserInfo LoginValidate(string userNo, string userPwd)
{
    LoginUserInfo userInfo = new LoginUserInfo();
    SqlConnection conn = null;

    try
    {
        conn = DataUtils.GetDbConn();

        // 第一步：使用參數化查詢取得使用者資料
        string sql = @"
            SELECT userno, username, authstatus, errct
            FROM   AMDBURM
            WHERE  userno = @userNo
        ";

        // 第二步：使用 Dapper 執行查詢
        var user = conn.QueryFirstOrDefault<TblAMDBURM>(sql, new { userNo = userNo });

        if (user == null)
        {
            userInfo.LoginErrMessage = "帳號不存在";
            return userInfo;
        }

        // 第三步：檢查帳號狀態
        if ("0".Equals(user.authstatus))
        {
            userInfo.LoginErrMessage = "您目前帳號無效，尚無法使用!!";
            return userInfo;
        }

        // 第四步：驗證密碼（使用參數化查詢）
        string sqlPwd = @"
            SELECT pwd
            FROM   AMDBURM
            WHERE  userno = @userNo
        ";

        string dbPwd = conn.QueryFirstOrDefault<string>(sqlPwd, new { userNo = userNo });

        if (!userPwd.Equals(dbPwd))
        {
            userInfo.LoginErrMessage = "密碼錯誤";

            // 第五步：更新錯誤次數（使用參數化查詢）
            string sqlUpdate = @"
                UPDATE AMDBURM
                SET    errct = errct + 1
                WHERE  userno = @userNo
            ";

            conn.Execute(sqlUpdate, new { userNo = userNo });

            return userInfo;
        }

        // 登入成功
        userInfo.UserNo = user.userno;
        userInfo.UserName = user.username;

        return userInfo;
    }
    finally
    {
        DataUtils.CloseDbConn(conn);
    }
}
```

**程式碼說明：**

1. **Dapper 參數化**：使用 Dapper 的匿名物件參數化查詢
2. **SQL 注入防護**：所有使用者輸入都透過參數傳遞，不直接拼接 SQL
3. **連線管理**：使用 `finally` 確保資料庫連線正確關閉
4. **錯誤處理**：驗證失敗時更新錯誤次數
5. **型別安全**：使用強型別查詢結果

### 3.2 輸入驗證機制

**檔案位置：** `Areas/Login/Controllers/C101MController.cs`

```csharp
/// <summary>
/// 輸入欄位驗證
/// 防止惡意輸入和注入攻擊
/// </summary>
/// <param name="form">登入表單</param>
private void InputValidate(C101MFormModel form)
{
    // 第一步：驗證碼檢查
    SessionModel sm = SessionModel.Get();
    if (!form.ValidateCode.Equals(sm.LoginValidateCode, StringComparison.OrdinalIgnoreCase))
    {
        throw new LoginExceptions("驗證碼輸入錯誤");
    }

    // 第二步：帳號格式檢查
    if (string.IsNullOrWhiteSpace(form.UserNo))
    {
        throw new LoginExceptions("請輸入帳號");
    }

    // 第三步：密碼格式檢查
    if (string.IsNullOrWhiteSpace(form.UserPwd))
    {
        throw new LoginExceptions("請輸入密碼");
    }

    // 第四步：檢查是否包含 SQL 關鍵字（基本防護）
    string[] sqlKeywords = {
        "SELECT", "INSERT", "UPDATE", "DELETE", "DROP",
        "CREATE", "ALTER", "EXEC", "EXECUTE", "--", "/*", "*/"
    };

    string upperUserNo = form.UserNo.ToUpper();
    foreach (string keyword in sqlKeywords)
    {
        if (upperUserNo.Contains(keyword))
        {
            LOG.Warn($"偵測到可疑輸入：{form.UserNo}");
            throw new LoginExceptions("輸入包含不允許的字元");
        }
    }
}
```

**程式碼說明：**

1. **驗證碼檢查**：防止自動化攻擊
2. **空值檢查**：確保必要欄位不為空
3. **SQL 關鍵字檢查**：基本的 SQL 注入防護（雙重防護）
4. **日誌記錄**：記錄可疑輸入，便於監控
5. **異常處理**：使用自訂異常類別統一錯誤處理

## 4. A04 - 不安全設計 (Insecure Design) 防護 ⚠

### 4.1 密碼強度檢查機制

**檔案位置：** `Areas/Login/Controllers/C101MController.cs`

#### 4.1.1 密碼複雜度驗證

```csharp
/// <summary>
/// 密碼檢核
/// 檢查密碼是否符合複雜度要求
/// </summary>
/// <param name="model">密碼變更模型</param>
/// <returns></returns>
[HttpPost]
public ActionResult PWDCHECK(C101MChangeModel model)
{
    SessionModel sm = SessionModel.Get();
    LoginDAO dao = new LoginDAO();
    ModelState.Clear();

    // 第一步：建立密碼檢查模型
    model.PWDCheck = new PasswordCheckModel();

    // 第二步：檢查密碼長度（至少 8 個字元）
    if (model.pwd.TONotNullString().Length < 8)
        model.PWDCheck.Chk_1 = false;
    else
        model.PWDCheck.Chk_1 = true;

    // 第三步：檢查是否包含小寫字母
    if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[a-z])"))
        model.PWDCheck.Chk_2 = false;
    else
        model.PWDCheck.Chk_2 = true;

    // 第四步：檢查是否包含大寫字母
    if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[A-Z])"))
        model.PWDCheck.Chk_3 = false;
    else
        model.PWDCheck.Chk_3 = true;

    // 第五步：檢查是否包含數字
    if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[0-9])"))
        model.PWDCheck.Chk_4 = false;
    else
        model.PWDCheck.Chk_4 = true;

    // 第六步：檢查是否包含特殊字元
    if (!Regex.IsMatch(model.pwd.TONotNullString(), @"^(?=.*[!@#$%^&*])"))
        model.PWDCheck.Chk_5 = false;
    else
        model.PWDCheck.Chk_5 = true;

    return PartialView("_PWDCHECK", model);
}
```

**程式碼說明：**

1. **長度檢查**：密碼至少 8 個字元
2. **小寫字母**：必須包含至少一個小寫字母
3. **大寫字母**：必須包含至少一個大寫字母
4. **數字**：必須包含至少一個數字
5. **特殊字元**：必須包含至少一個特殊字元

### 4.2 不安全設計的缺失項目

**缺少的安全設計：**

1. ✗ **統一輸入驗證框架**：缺少統一的輸入驗證和清理機制
2. ✗ **XSS 防護機制**：雖然 Razor 有自動編碼，但缺少統一的 XSS 防護策略
3. ✗ **CSRF Token 驗證**：缺少 `[ValidateAntiForgeryToken]` 屬性的統一使用
4. ✗ **安全設計模式**：缺少統一的安全基底類別或介面
5. ⚠ **錯誤處理**：部分錯誤處理可能洩漏系統資訊

## 5. A05 - 安全設定缺陷 (Security Misconfiguration) 防護 ✗

### 5.1 缺少的安全標頭設定

**⚠ 重要警告：** EECOnline 系統缺少以下關鍵的安全標頭設定：

#### 5.1.1 缺少的安全標頭

```csharp
// ✗ 缺少：Content Security Policy (CSP)
// 建議在 Global.asax.cs 或 Web.config 中加入：
// Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';

// ✗ 缺少：X-Frame-Options
// 建議加入：X-Frame-Options: SAMEORIGIN

// ✗ 缺少：X-Content-Type-Options
// 建議加入：X-Content-Type-Options: nosniff

// ✗ 缺少：X-XSS-Protection
// 建議加入：X-XSS-Protection: 1; mode=block

// ✗ 缺少：Strict-Transport-Security (HSTS)
// 建議加入：Strict-Transport-Security: max-age=31536000; includeSubDomains

// ✗ 缺少：Referrer-Policy
// 建議加入：Referrer-Policy: strict-origin-when-cross-origin
```

#### 5.1.2 建議的 Global.asax.cs 改進

```csharp
/// <summary>
/// 應用程式開始請求事件
/// 建議加入安全標頭設定
/// </summary>
protected void Application_BeginRequest(Object sender, EventArgs e)
{
    // 建議加入：設定安全標頭
    Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    Response.Headers.Add("X-Content-Type-Options", "nosniff");
    Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    // 建議加入：強制使用 HTTPS（生產環境）
    if (!Request.IsSecureConnection && !Request.IsLocal)
    {
        string redirectUrl = Request.Url.ToString().Replace("http:", "https:");
        Response.Redirect(redirectUrl, true);
    }
}
```

### 5.2 安全設定缺陷總結

**缺少的安全設定：**

1. ✗ **CSP 標頭**：缺少 Content Security Policy，無法防止 XSS 和資料注入
2. ✗ **點擊劫持防護**：缺少 X-Frame-Options 標頭
3. ✗ **MIME 嗅探防護**：缺少 X-Content-Type-Options 標頭
4. ✗ **HTTPS 強制**：未在 Global.asax.cs 中強制使用 HTTPS
5. ✗ **HSTS 標頭**：缺少 HTTP Strict Transport Security 標頭

## 6. A06 - 易受攻擊和過時的元件 (Vulnerable and Outdated Components) 防護 ⚠

### 6.1 目前使用的技術框架

**檔案位置：** `Web.config`

#### 6.1.1 框架版本分析

```xml
<!-- 目前使用的 .NET Framework 版本 -->
<system.web>
  <compilation debug="true" targetFramework="4.7.2" />
  <httpRuntime targetFramework="4.7.2" />
</system.web>
```

**技術棧分析：**

1. ⚠ **.NET Framework 4.7.2**：非最新版本，建議升級至 .NET 6/7/8 或最新的 .NET Framework
2. ⚠ **IBatis.NET**：較舊的 ORM 框架，已停止維護，建議考慮遷移至 Dapper 或 Entity Framework
3. ✓ **Dapper**：部分模組使用 Dapper，這是現代化的輕量級 ORM
4. ⚠ **jQuery**：需確認版本是否為最新，舊版本可能有已知漏洞
5. ⚠ **Bootstrap**：需確認版本是否為最新

### 6.2 依賴管理建議

**建議的改進措施：**

1. ✗ **定期更新**：缺少定期檢查和更新依賴套件的機制
2. ✗ **漏洞掃描**：缺少自動化的漏洞掃描工具（如 OWASP Dependency-Check）
3. ✗ **版本管理**：缺少明確的套件版本管理策略
4. ⚠ **框架升級**：建議評估升級至 .NET Core/.NET 6+ 的可行性

## 7. A07 - 身份識別和身份驗證失效 (Identification and Authentication Failures) 防護 ✓

### 7.1 登入失敗鎖定機制

**檔案位置：** `Services/ClamServices.cs`

#### 7.1.1 防暴力破解實作

```csharp
/// <summary>
/// 使用者登入驗證
/// 包含防暴力破解機制
/// </summary>
/// <param name="userNo">使用者帳號</param>
/// <param name="userPwd">使用者密碼</param>
/// <param name="IP">登入 IP</param>
/// <returns>使用者資訊</returns>
public LoginUserInfo LoginValidate(string userNo, string userPwd, string IP)
{
    LoginUserInfo userInfo = new LoginUserInfo();
    LoginDAO dao = new LoginDAO();

    // 第一步：取得使用者資料
    TblAMDBURM dburm = new TblAMDBURM();
    dburm.userno = userNo;
    dburm = dao.GetRow(dburm);

    if (dburm == null)
    {
        userInfo.LoginErrMessage = "帳號不存在";
        return userInfo;
    }

    // 第二步：檢查帳號有效性
    if ("0".Equals(dburm.authstatus))
    {
        userInfo.LoginErrMessage = "您目前帳號無效，尚無法使用!!";

        // 記錄登入失敗
        TblLOGINLOG llog = new TblLOGINLOG();
        llog.moduser = userNo;
        llog.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
        llog.status = "0";  // 失敗
        llog.modusername = "";
        llog.modip = IP;
        dao.Insert(llog);

        return userInfo;
    }

    // 第三步：檢查帳號是否有效
    if ("1".Equals(dburm.authstatus))
    {
        // 第四步：密碼檢核
        string encPass = CypherText(userPwd);
        string encDefsultPass = this.GetDefaultPasswordEnc(userNo);

        long errct = dburm.errct != null ? dburm.errct.Value : 0;

        if (!encPass.Equals(encDefsultPass))
        {
            // 密碼錯誤
            userInfo.LoginErrMessage = "密碼錯誤";
            errct++;

            // 第五步：記錄密碼錯誤次數
            dburm.errct = errct.TOInt32();
            dao.UpdateUserErrCount(dburm);

            // 第六步：記錄登入失敗
            TblLOGINLOG llog = new TblLOGINLOG();
            llog.moduser = userNo;
            llog.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
            llog.status = "0";  // 失敗
            llog.modusername = dburm.username;
            llog.modip = IP;
            dao.Insert(llog);

            return userInfo;
        }

        // 第七步：登入成功，清除錯誤次數
        dburm.errct = 0;
        dao.UpdateUserErrCount(dburm);

        // 第八步：記錄登入成功
        TblLOGINLOG llog2 = new TblLOGINLOG();
        llog2.moduser = userNo;
        llog2.modtime = DateTime.Now.ToString("yyyyMMddHHmmss");
        llog2.status = "1";  // 成功
        llog2.modusername = dburm.username;
        llog2.modip = IP;
        dao.Insert(llog2);

        // 第九步：載入使用者資訊和權限
        userInfo.UserNo = dburm.userno;
        userInfo.UserName = dburm.username;
        userInfo.RoleFuncs = dao.GetUserRoleFuncs(userNo);
    }

    return userInfo;
}
```

**程式碼說明：**

1. **帳號檢查**：驗證帳號是否存在和有效
2. **密碼驗證**：使用加密後的密碼進行比對
3. **錯誤計數**：記錄連續密碼錯誤次數
4. **登入日誌**：記錄所有登入嘗試（成功和失敗）
5. **成功清零**：登入成功後清除錯誤次數
   **⚠ 改進建議：** 目前系統記錄錯誤次數，但缺少基於時間的帳號鎖定機制（例如：5 次失敗後鎖定 15 分鐘）。

## 8. A08 - 軟體和資料完整性失效 (Software and Data Integrity Failures) 防護 ⚠

### 8.1 檔案上傳安全驗證

**檔案位置：** `Controllers/AjaxController.cs`

#### 8.1.1 檔案上傳驗證實作

```csharp
/// <summary>
/// Ajax 上傳檔案
/// 包含檔案類型和大小驗證
/// </summary>
/// <param name="Upload">上傳檔案資訊</param>
/// <returns></returns>
[HttpPost]
public ActionResult UploadFile(DynamicEFileGrid Upload)
{
    SessionModel sm = SessionModel.Get();

    // 第一步：檢查是否有檔案上傳
    if (Request.Files.AllKeys.Any())
    {
        // 第二步：讀取上傳的檔案
        var httpPostedFile = Request.Files[0];

        // 第三步：取得副檔名
        var Extension = httpPostedFile.FileName.ToSplit('.').LastOrDefault();

        // 第四步：檢查副檔名是否允許
        bool accepted = false;
        IList<AcceptFileType> acceptTypes = Upload.GetAcceptFileTypes();
        if (acceptTypes != null)
        {
            string ext = Extension;
            foreach (AcceptFileType type in acceptTypes)
            {
                if (type.Equals(ext))
                {
                    accepted = true;
                    break;
                }
            }
        }

        // 第五步：副檔名驗證失敗
        if (!accepted)
        {
            Upload.ErrorMsg = "副檔名不允許，請檢查檔案後再重新上傳";
            return PartialView("_DynamicEFileGrid", Upload);
        }

        // 第六步：檢查檔案大小（限制 10MB）
        if (httpPostedFile.ContentLength > 10485760)
        {
            Upload.ErrorMsg = "檔案大於10M，請檢查檔案後再重新上傳";
            return PartialView("_DynamicEFileGrid", Upload);
        }

        // 第七步：儲存檔案
        // ... 檔案儲存邏輯 ...
    }

    return PartialView("_DynamicEFileGrid", Upload);
}
```

**程式碼說明：**

1. **副檔名檢查**：使用白名單驗證允許的檔案類型
2. **檔案大小限制**：限制檔案大小不超過 10MB
3. **AcceptFileType**：使用列舉定義允許的檔案類型
4. **錯誤訊息**：提供明確的錯誤訊息給使用者
5. **Session 檢查**：確保使用者已登入

#### 8.1.2 允許的檔案類型定義

**檔案位置：** `Models/PdfUploadFile.cs`, `Models/XlsUploadFile.cs`

```csharp
/// <summary>
/// PDF 檔案上傳類
/// 只允許上傳 .pdf 檔案
/// </summary>
public class PdfUploadFile : UploadFile
{
    private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();

    /// <summary>
    /// 靜態建構子，定義允許的檔案類型
    /// </summary>
    static PdfUploadFile()
    {
        acceptFileTypes.Add(AcceptFileType.PDF);
    }

    public override IList<AcceptFileType> GetAcceptFileTypes()
    {
        return acceptFileTypes;
    }
}

/// <summary>
/// Excel 檔案上傳類
/// 允許上傳 .xls 和 .xlsx 檔案
/// </summary>
public class XlsUploadFile : UploadFile
{
    private static IList<AcceptFileType> acceptFileTypes = new List<AcceptFileType>();

    static XlsUploadFile()
    {
        acceptFileTypes.Add(AcceptFileType.XLSX);
        acceptFileTypes.Add(AcceptFileType.XLS);
    }

    public override IList<AcceptFileType> GetAcceptFileTypes()
    {
        return acceptFileTypes;
    }
}
```

**程式碼說明：**

1. **白名單策略**：明確定義允許的檔案類型
2. **繼承架構**：所有上傳類別繼承自 `UploadFile` 基底類別
3. **靜態定義**：使用靜態建構子定義允許的類型
4. **型別安全**：使用列舉 `AcceptFileType` 確保型別安全

### 8.2 檔案上傳安全的缺失項目

**缺少的安全機制：**

1. ✗ **檔案內容驗證**：缺少檔案頭（Magic Number）驗證，只檢查副檔名
2. ✗ **MIME 類型檢查**：缺少 Content-Type 驗證
3. ✗ **病毒掃描**：缺少上傳檔案的病毒掃描機制
4. ✗ **檔案名稱清理**：缺少檔案名稱的安全清理（防止路徑遍歷）
5. ⚠ **儲存位置**：需確認檔案儲存位置不在 Web 根目錄下

## 9. A09 - 安全日誌和監控失效 (Security Logging and Monitoring Failures) 防護 ✓

### 9.1 log4net 日誌記錄機制

**檔案位置：** `Global.asax.cs`, `Commons/Filter/CustomAuthAttribute.cs`

#### 9.1.1 全域錯誤處理和日誌記錄

```csharp
/// <summary>
/// 應用程式錯誤處理
/// 記錄所有未處理的異常
/// </summary>
protected void Application_Error()
{
    // 第一步：取得最後一個錯誤
    Exception lastError = Server.GetLastError();
    Server.ClearError();

    // 第二步：記錄錯誤到 log4net
    LOG.Info("ERROR：" + lastError.Message);

    // 第三步：設定 HTTP 狀態碼
    Response.StatusCode = 404;
}
```

#### 9.1.2 登入日誌記錄

**檔案位置：** `DataLayers/FrontDAO.cs`

```csharp
/// <summary>
/// 前台操作記錄
/// 記錄使用者的登入和操作行為
/// </summary>
/// <param name="idno">身分證字號</param>
/// <param name="username">使用者姓名</param>
/// <param name="lType">登入方式</param>
/// <param name="lStatus">登入狀態</param>
/// <param name="lIP">IP 位址</param>
/// <param name="oType">操作功能別（編碼）</param>
/// <param name="oName">操作功能別（名稱）</param>
/// <param name="isDL">是否下載病歷</param>
public static void FrontLOG(string idno, string username, em_lType lType, em_lStatus lStatus,
    string lIP, string oType, string oName, bool isDL = false)
{
    try
    {
        // 第一步：建立操作記錄
        var InsLog = new TblEEC_UserOperation();
        InsLog.keyid = null;
        InsLog.idno = idno;
        InsLog.name = username;
        InsLog.login_type = Convert.ToInt32(lType).ToString();
        InsLog.login_status = Convert.ToInt32(lStatus).ToString();
        InsLog.ip = lIP;
        InsLog.operation_type = oType;
        InsLog.operation_name = oName;
        InsLog.is_download = isDL ? "1" : "0";
        InsLog.operation_time = DateTime.Now;

        // 第二步：寫入資料庫
        FrontDAO dao = new FrontDAO();
        dao.Insert(InsLog);
    }
    catch (Exception ex)
    {
        // 記錄失敗不影響主要流程
        LOG.Error("FrontLOG 記錄失敗: " + ex.Message, ex);
    }
}
```

#### 9.1.3 授權檢查日誌

**檔案位置：** `Commons/Filter/CustomAuthAttribute.cs`

```csharp
/// <summary>
/// 授權檢查日誌記錄
/// </summary>
public override void OnAuthorization(AuthorizationContext filterContext)
{
    string actionPath = ControllerContextHelper.GetActionPath(filterContext);
    string verb = filterContext.HttpContext.Request.HttpMethod;
    string userNo = (sm.UserInfo != null) ? sm.UserInfo.UserNo : null;

    // 記錄授權檢查
    LOG.Info("OnAuthorization[" + userNo + "] " + verb + " " + actionPath +
             " (allowAnonymous=" + allowAnonymous + ", isByPassAuth=" + isByPassAuth + ")");

    // 記錄未授權存取
    if (!isAuth)
    {
        LOG.Info("UNAUTHORIZED [" + sm.UserInfo.UserNo + "] " + verb + " " +
                 sm.LastActionPath + ", redirect to UNAUTH_PAGE page");
    }
}
```

**程式碼說明：**

1. **全域錯誤記錄**：捕獲所有未處理的異常並記錄
2. **操作日誌**：記錄使用者的登入和操作行為
3. **授權日誌**：記錄所有授權檢查結果
4. **IP 記錄**：記錄使用者的 IP 位址
5. **時間戳記**：記錄操作時間

### 9.2 日誌記錄的改進建議

**缺少的日誌機制：**

1. ✗ **集中式日誌**：缺少集中式日誌管理系統（如 ELK Stack）
2. ✗ **即時監控**：缺少即時安全事件監控和告警
3. ✗ **日誌保護**：缺少日誌檔案的完整性保護
4. ⚠ **敏感資料**：需確認日誌不包含敏感資料（如密碼）
5. ⚠ **日誌保留**：需明確日誌保留政策

## 10. A10 - 伺服器端請求偽造 (Server-Side Request Forgery, SSRF) 防護 ✗

### 10.1 SSRF 防護現況

**⚠ 重要警告：** EECOnline 系統目前缺少明確的 SSRF 防護機制。

**缺少的 SSRF 防護：**

1. ✗ **URL 白名單**：缺少外部 URL 請求的白名單驗證
2. ✗ **IP 黑名單**：缺少內部 IP 位址的黑名單過濾
3. ✗ **協議限制**：缺少允許協議的限制（如只允許 HTTP/HTTPS）
4. ✗ **重定向檢查**：缺少 HTTP 重定向的安全檢查
5. ✗ **DNS 重綁定防護**：缺少 DNS 重綁定攻擊的防護

### 10.2 建議的 SSRF 防護實作

```csharp
/// <summary>
/// 建議加入：安全的 HTTP 請求方法
/// 防止 SSRF 攻擊
/// </summary>
public static bool IsUrlSafe(string url)
{
    try
    {
        Uri uri = new Uri(url);

        // 第一步：只允許 HTTP 和 HTTPS 協議
        if (uri.Scheme != "http" && uri.Scheme != "https")
        {
            return false;
        }

        // 第二步：禁止存取內部 IP
        IPAddress ipAddress;
        if (IPAddress.TryParse(uri.Host, out ipAddress))
        {
            // 禁止 localhost
            if (IPAddress.IsLoopback(ipAddress))
            {
                return false;
            }

            // 禁止私有 IP 範圍
            byte[] bytes = ipAddress.GetAddressBytes();
            if (bytes[0] == 10 ||  // 10.0.0.0/8
                (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||  // 172.16.0.0/12
                (bytes[0] == 192 && bytes[1] == 168))  // 192.168.0.0/16
            {
                return false;
            }
        }

        // 第三步：使用白名單（建議）
        string[] allowedDomains = { "example.com", "api.example.com" };
        bool domainAllowed = false;
        foreach (string domain in allowedDomains)
        {
            if (uri.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
            {
                domainAllowed = true;
                break;
            }
        }

        return domainAllowed;
    }
    catch
    {
        return false;
    }
}
```

## 11. OWASP Top 10 防護總結

### 11.1 防護對照表

| OWASP Top 10 項目        | 防護狀態 | 已實作機制                                 | 缺失項目                                                       |
| ------------------------ | -------- | ------------------------------------------ | -------------------------------------------------------------- |
| A01 - 存取控制失效       | ✓ 已實作 | LoginRequired Filter、RBAC、Session 管理   | -                                                              |
| A02 - 加密機制失效       | ⚠ 部分   | SHA-512 雜湊、AES 加密                     | 密碼 Salt、HTTPS 強制、金鑰管理、部分使用 RSACSP 可逆加密      |
| A03 - 注入攻擊           | ✓ 已實作 | IBatis 參數化查詢、Dapper 參數化、輸入驗證 | -                                                              |
| A04 - 不安全設計         | ⚠ 部分   | 密碼強度檢查                               | 統一驗證框架、XSS 防護策略、CSRF Token、安全基底類別           |
| A05 - 安全設定缺陷       | ✗ 缺少   | -                                          | CSP、X-Frame-Options、X-Content-Type-Options、HTTPS 強制、HSTS |
| A06 - 易受攻擊元件       | ⚠ 部分   | 使用 Dapper                                | 定期更新機制、漏洞掃描、版本管理、框架升級                     |
| A07 - 身份驗證失效       | ✓ 已實作 | 登入失敗記錄、錯誤次數追蹤、登入日誌       | 基於時間的帳號鎖定（如 5 次失敗鎖定 15 分鐘）                  |
| A08 - 軟體資料完整性失效 | ⚠ 部分   | 檔案類型白名單、檔案大小限制               | 檔案頭驗證、MIME 檢查、病毒掃描、檔案名稱清理                  |
| A09 - 安全日誌監控失效   | ✓ 已實作 | log4net、登入日誌、操作日誌、授權日誌      | 集中式日誌、即時監控、日誌保護                                 |
| A10 - 伺服器端請求偽造   | ✗ 缺少   | -                                          | URL 白名單、IP 黑名單、協議限制、重定向檢查、DNS 重綁定防護    |

### 11.2 已實作的安全機制

#### 11.2.1 存取控制 (A01) ✓

1. **LoginRequired Filter**：統一的登入檢查機制
2. **RBAC 模型**：基於角色的存取控制
3. **Session 管理**：60 分鐘逾時控制
4. **權限檢查**：功能級別的權限驗證
5. **日誌記錄**：完整的授權檢查日誌

#### 11.2.2 注入攻擊防護 (A03) ✓

1. **IBatis 參數化**：使用 `#參數名#` 語法防止 SQL 注入
2. **Dapper 參數化**：使用匿名物件參數化查詢
3. **動態 SQL 安全**：使用 `<dynamic>` 標籤安全建構查詢
4. **輸入驗證**：檢查 SQL 關鍵字
5. **快取機制**：IBatis 快取提升效能

#### 11.2.3 身份驗證 (A07) ✓

1. **密碼加密**：使用 SHA-512 雜湊（部分模組）
2. **錯誤次數追蹤**：記錄連續登入失敗次數
3. **登入日誌**：記錄所有登入嘗試
4. **驗證碼**：防止自動化攻擊
5. **密碼強度檢查**：8 字元、大小寫、數字、特殊字元

#### 11.2.4 日誌監控 (A09) ✓

1. **log4net**：統一的日誌記錄框架
2. **全域錯誤處理**：捕獲所有未處理異常
3. **操作日誌**：記錄使用者操作行為
4. **授權日誌**：記錄授權檢查結果
5. **IP 記錄**：記錄使用者 IP 位址

### 11.3 需要改進的安全機制

#### 11.3.1 高優先級改進項目

1. **密碼加密演算法 (A02)**

   - 問題：部分模組使用 RSACSP 可逆加密
   - 建議：全面改用 bcrypt、PBKDF2 或 Argon2
   - 影響：高（密碼安全性）

2. **安全標頭設定 (A05)**

   - 問題：缺少 CSP、X-Frame-Options 等安全標頭
   - 建議：在 Global.asax.cs 中加入安全標頭
   - 影響：高（XSS、點擊劫持防護）

3. **HTTPS 強制 (A02, A05)**

   - 問題：未強制使用 HTTPS
   - 建議：在 Global.asax.cs 中加入 HTTPS 重定向
   - 影響：高（傳輸安全）

4. **帳號鎖定機制 (A07)**
   - 問題：缺少基於時間的帳號鎖定
   - 建議：5 次失敗後鎖定 15 分鐘
   - 影響：中（防暴力破解）

#### 11.3.2 中優先級改進項目

1. **檔案上傳驗證 (A08)**

   - 問題：只檢查副檔名，缺少檔案頭驗證
   - 建議：加入 Magic Number 檢查
   - 影響：中（檔案上傳安全）

2. **CSRF 防護 (A04)**

   - 問題：缺少統一的 CSRF Token 驗證
   - 建議：在表單提交時使用 `@Html.AntiForgeryToken()`
   - 影響：中（跨站請求偽造防護）

3. **密碼 Salt (A02)**

   - 問題：SHA-512 雜湊未使用 Salt
   - 建議：為每個密碼產生唯一的 Salt
   - 影響：中（彩虹表攻擊防護）

4. **XSS 防護策略 (A04)**
   - 問題：缺少統一的 XSS 防護機制
   - 建議：建立統一的輸入清理和輸出編碼策略
   - 影響：中（XSS 攻擊防護）

#### 11.3.3 低優先級改進項目

1. **集中式日誌 (A09)**

   - 問題：缺少集中式日誌管理
   - 建議：導入 ELK Stack 或類似解決方案
   - 影響：低（日誌管理效率）

2. **SSRF 防護 (A10)**

   - 問題：缺少 SSRF 防護機制
   - 建議：加入 URL 白名單和 IP 黑名單
   - 影響：低（取決於系統是否有外部請求功能）

3. **依賴管理 (A06)**
   - 問題：缺少定期更新和漏洞掃描
   - 建議：建立定期檢查和更新流程
   - 影響：低（長期維護）

## 12. 總結

### 12.1 核心安全特色

EECOnline 系統已實作以下核心安全機制：

1. **完整的存取控制**：LoginRequired Filter + RBAC 模型
2. **SQL 注入防護**：IBatis 和 Dapper 參數化查詢
3. **身份驗證機制**：密碼加密、登入日誌、錯誤追蹤
4. **日誌監控**：log4net + 操作日誌 + 授權日誌
5. **檔案上傳驗證**：白名單 + 大小限制

### 12.2 安全性評估

**優勢：**

1. ✓ **存取控制完善**：RBAC 模型提供細緻的權限管理
2. ✓ **SQL 注入防護良好**：全面使用參數化查詢
3. ✓ **日誌記錄完整**：涵蓋登入、操作、授權等關鍵事件
4. ✓ **基本身份驗證**：包含密碼加密和登入追蹤

**需改進：**

1. ⚠ **密碼加密演算法**：部分使用可逆加密，需改用不可逆雜湊
2. ✗ **安全標頭缺失**：缺少 CSP、X-Frame-Options 等關鍵標頭
3. ✗ **HTTPS 未強制**：未在應用程式層面強制使用 HTTPS
4. ⚠ **檔案上傳驗證不足**：只檢查副檔名，缺少檔案頭驗證
5. ✗ **CSRF 防護不完整**：缺少統一的 CSRF Token 驗證

### 12.3 改進建議優先順序

**立即改進（高優先級）：**

1. 修改密碼加密演算法，全面改用 bcrypt 或 PBKDF2
2. 在 Global.asax.cs 中加入安全標頭設定
3. 實作 HTTPS 強制重定向
4. 加入基於時間的帳號鎖定機制（5 次失敗鎖定 15 分鐘）

**短期改進（中優先級）：**

1. 加入檔案頭（Magic Number）驗證
2. 實作統一的 CSRF Token 驗證
3. 為密碼雜湊加入 Salt
4. 建立統一的 XSS 防護策略

**長期改進（低優先級）：**

1. 導入集中式日誌管理系統
2. 建立定期依賴更新和漏洞掃描流程
3. 評估升級至 .NET Core/.NET 6+ 的可行性
4. 加入 SSRF 防護機制（如系統有外部請求功能）

### 12.4 技術價值

透過本文件的分析，EECOnline 系統的安全設計具有以下價值：

1. **誠實評估**：真實呈現系統的安全現況，不虛構不存在的功能
2. **明確方向**：清楚標示已實作和缺失的安全機制
3. **可行建議**：提供具體可行的改進建議和優先順序
4. **持續改進**：為系統安全性的持續提升提供明確路徑

這份文件不僅記錄了現有的安全機制，也為未來的安全改進提供了清晰的藍圖。
