# 電子病歷申請系統 (EECOnline) - 身份驗證與授權機制詳細範例：Authentication 與 Authorization

## 功能概述

本文詳細說明 EECOnline 系統中的身份驗證（Authentication）和授權（Authorization）機制。系統採用基於 Session 的身份驗證，結合自訂 AuthorizeAttribute 的權限控制，以及基於角色的權限管理（RBAC）系統，提供完整的安全性保護機制。

## 安全性架構概覽

```
EECOnline 安全性架構：

前端請求
    ↓ 攜帶 Session Cookie
身份驗證層 (Authentication)
    ↓ 檢查 Session 有效性
LoginRequired Filter
    ↓ 驗證登入狀態
Session 管理層
    ↓ 從 Session 取得使用者資訊
授權檢查層 (Authorization)
    ↓ 檢查功能權限
角色權限比對
    ↓ 比對使用者角色與功能權限
業務邏輯層
    ↓ 執行業務操作
資料存取層
    ↓ 記錄操作日誌
回傳結果
```

## 涉及的主要元件

| 元件             | 用途                         | 檔案位置                                   |
| ---------------- | ---------------------------- | ------------------------------------------ |
| LoginRequired    | 登入與權限檢查 Filter        | Commons/Filter/CustomAuthAttribute.cs      |
| BypassAuthorize  | 略過權限檢查標記             | Commons/Filter/CustomAuthAttribute.cs      |
| BaseController   | 需要登入的 Controller 基底類 | Controllers/BaseController.cs              |
| SessionModel     | Session 管理                 | Models/SessionModel.cs                     |
| LoginUserInfo    | 登入使用者資訊               | Models/LoginUserInfo.cs                    |
| C101MController  | 登入控制器                   | Areas/Login/Controllers/C101MController.cs |
| ClamService      | 權限管理服務                 | Services/ClamService.cs                    |
| LoginDAO         | 登入資料存取                 | DataLayers/LoginDAO.cs                     |
| ApplicationModel | 系統功能定義管理             | Models/ApplicationModel.cs                 |
| TblAMFUNCM       | 功能定義資料表               | Models/TblAMFUNCM.cs                       |
| ClamRoleFunc     | 角色功能權限                 | Models/ClamRoleFunc.cs                     |

## 主要功能模組

1. **身份驗證模組**：支援 2 種登入方式（帳號密碼、醫院授權碼）
2. **Session 管理模組**：管理使用者登入狀態與資訊
3. **權限檢查模組**：檢查使用者是否有權限執行特定功能
4. **角色管理模組**：管理使用者角色與權限對應
5. **登入失敗鎖定模組**：防止暴力破解攻擊
6. **密碼加密模組**：使用 SHA-256 加密密碼

## 1. 整體身份驗證機制（Authentication）

### 1.1 LoginRequired 登入檢查 Filter

**檔案位置：** `Commons/Filter/CustomAuthAttribute.cs`

LoginRequired 是一個自訂的 AuthorizeAttribute，用於檢查使用者是否已登入，並檢查是否有權限執行特定功能。這是系統安全性的核心機制，所有需要登入的 Controller 都必須套用此 Filter。

#### 1.1.1 LoginRequired 類別定義

```csharp
/// <summary>
/// 同時判斷登入狀態及角色執行權限的 AuthorizeAttribute
/// 這是系統安全性的核心機制，所有需要登入的 Controller 都必須套用此 Filter
/// </summary>
public class LoginRequired : AuthorizeAttribute
{
    /// <summary>
    /// 預設系統登入頁
    /// 未登入的使用者會被導向此頁面
    /// </summary>
    public static string LOGIN_PAGE = "~/Login/C101M";

    /// <summary>
    /// 沒有權限的訊息頁面
    /// 已登入但沒有權限的使用者會被導向此頁面
    /// </summary>
    public static string UNAUTH_PAGE = "~/ErrorPage/UnAuth";

    /// <summary>
    /// 日誌記錄器，用於記錄所有授權相關的操作
    /// </summary>
    private static readonly ILog LOG = LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// web.config appSettings 設定是否停用 Authorize 權限檢核（測試開發用）
    /// <para>2: 當系統管理者角色時停用，AuthorizeRequired 沒作用直接 bypass</para>
    /// <para>1: 全部停用，AuthorizeRequired 沒作用直接 bypass</para>
    /// <para>0: 未停用</para>
    /// </summary>
    private string disableAuth = System.Configuration.ConfigurationManager.AppSettings["DisableAuthorize"];
}
```

**程式碼說明：**

1. **繼承 AuthorizeAttribute**：利用 ASP.NET MVC 的內建授權機制
2. **LOGIN_PAGE**：定義未登入使用者的導向頁面（~/Login/C101M）
3. **UNAUTH_PAGE**：定義無權限使用者的導向頁面（~/ErrorPage/UnAuth）
4. **disableAuth**：開發測試時可以暫時停用權限檢查，避免影響開發效率
5. **LOG**：記錄所有授權相關的操作，用於安全稽核和問題追蹤

#### 1.1.2 OnAuthorization 權限檢查方法

```csharp
/// <summary>
/// LoginRequired 登入 Session 檢核
/// 這是整個授權流程的核心方法，會在每個 Action 執行前被呼叫
/// </summary>
/// <param name="filterContext">授權上下文</param>
public override void OnAuthorization(AuthorizationContext filterContext)
{
    // 第一步：取得當前請求的 Action 路徑
    // 例如：A1/SE11/Index 或 Login/C101M/Login
    string actionPath = ControllerContextHelper.GetActionPath(filterContext);
    string verb = filterContext.HttpContext.Request.HttpMethod;  // GET, POST, PUT, DELETE

    // 第二步：要把 actionPath 中的 action method 部份去掉
    // 只留下 Area/Controller 以用來比對 AMFUNCM.prgid
    // 例如：A1/SE11/Index → A1/SE11
    string funcPath = actionPath;
    string[] tokens = actionPath.Split('/');
    if (tokens.Length >= 3)
    {
        funcPath = tokens[0] + "/" + tokens[1];
    }

    // 第三步：取得 Session 並記錄當前 Action 路徑
    SessionModel sm = SessionModel.Get();
    sm.LastActionPath = actionPath;  // 記錄最後執行的 Action 路徑
    sm.LastActionFunc = null;        // 清空最後執行的功能定義

    // 第四步：檢查是否有 AllowAnonymous 或 BypassAuthorize 屬性
    ActionDescriptor desc = filterContext.ActionDescriptor;
    // AllowAnonymous：允許匿名存取（不需登入）
    var allowAnonymous = desc.IsDefined(typeof(AllowAnonymousAttribute), true);
    // BypassAuthorize：略過權限檢查（需登入但不檢查權限）
    var isByPassAuth = desc.IsDefined(typeof(BypassAuthorize), true)
                        || desc.ControllerDescriptor.IsDefined(typeof(BypassAuthorize), true);

    // 第五步：記錄授權檢查日誌
    string userNo = (sm.UserInfo != null) ? sm.UserInfo.UserNo : null;
    LOG.Info("OnAuthorization[" + userNo + "] " + verb + " " + actionPath +
             " (allowAnonymous=" + allowAnonymous + ", isByPassAuth=" + isByPassAuth + ")");

    #region 檢查是否已登入
    // 第六步：檢查使用者是否已登入
    bool isLogin = false;
    if (sm.UserInfo != null
        && (sm.UserInfo.UserType.Equals(this.Roles)
            || (string.IsNullOrEmpty(this.Roles)
                && sm.UserInfo.UserType.Equals(LoginUserType.SKILL_USER)))
        )
    {
        isLogin = true;  // 使用者已登入且角色符合
    }

    if (!isLogin)
    {
        // 第七步：未登入處理
        if (!allowAnonymous)
        {
            // 根據 LoginRequired.Roles 決定登入頁面
            string loginPage = LOGIN_PAGE;
            LOG.Info("OnAuthorization: redirect to Login page: " + loginPage);

            // 第八步：儲存重導資訊（用於登入後返回原頁面）
            filterContext.Controller.TempData["RedirectPath"] = actionPath;
            filterContext.Controller.TempData["RedirectApyId"] =
                HttpContext.Current.Request.QueryString["apy_id"];
            filterContext.Controller.TempData["RedirectGUID"] =
                HttpContext.Current.Request.QueryString["GUID"];

            // 第九步：導向登入頁面
            filterContext.Result = new RedirectResult(loginPage);
        }
    }
    #endregion

    #region 檢查權限
    else
    {
        // 第十步：比對系統 TblAMFUNCM.prgid 以取得功能名稱

        // 取得系統中已啟用的全部 Action Function 定義
        // 以比對取得當前 action path 對應的功能名稱並記錄在 SessionModel.LastActionFunc
        // 比對範圍：以 area/controller 為準，同一個 controller 下不再區分子功能
        IList<TblAMFUNCM> allFuncs = ApplicationModel.GetClamFuncsAll();
        sm.LastActionFunc = null;
        for (int i = 0; i < allFuncs.Count; i++)
        {
            TblAMFUNCM item = allFuncs[i];
            if (funcPath.Equals(item.prgid))
            {
                /* 找到 action 對應功能，
                 * 包括同一Controller中的相關子功能，
                 * 例：A1/SE11/Index, A1/SE11/Modify
                 *    都對應至 A1/SE11 這個功能
                 */
                sm.LastActionFunc = item;
                break;
            }
        }

        // 第十一步：權限檢核
        if (sm.LastActionFunc == null && !isByPassAuth)
        {
            LOG.Warn("功能路徑 [" + funcPath + "] 找不到 AMFUNCM 定義");
        }

        if (!isByPassAuth)
        {
            // 第十二步：角色權限檢核
            IList<ClamRoleFunc> funcs = sm.RoleFuncs;  // 取得使用者的角色權限清單
            bool isAuth = false;
            if (funcs != null)
            {
                TblAMFUNCM func = sm.LastActionFunc;
                for (int i = 0; func != null && i < funcs.Count; i++)
                {
                    ClamRoleFunc item = funcs[i];
                    // 比對系統ID、模組、子模組是否相符
                    if (item.sysid.Equals(func.sysid)
                        && item.modules.Equals(func.modules)
                        && item.submodules.Equals(func.submodules)
                        )
                    {
                        isAuth = true;  // 找到符合的權限
                        break;
                    }
                }
            }

            // 第十三步：未授權處理
            if (!isAuth)
            {
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
    #endregion

    return;
}
```

**程式碼說明：**

1. **Action 路徑解析**：從 filterContext 取得當前請求的 Area/Controller/Action 路徑
2. **功能路徑轉換**：將 Area/Controller/Action 轉換為 Area/Controller，用於比對功能定義
3. **Session 取得**：從 SessionModel 取得使用者登入資訊
4. **AllowAnonymous 檢查**：檢查 Action 是否標記為允許匿名存取
5. **BypassAuthorize 檢查**：檢查 Action 是否標記為略過權限檢查
6. **登入狀態檢查**：檢查使用者是否已登入，未登入則導向登入頁面
7. **重導資訊儲存**：儲存原始請求路徑，登入後可以返回原頁面
8. **功能定義比對**：從 ApplicationModel 取得所有功能定義，比對當前 Action 對應的功能
9. **角色權限檢核**：比對使用者的角色權限清單，確認是否有權限執行此功能
10. **未授權處理**：無權限時導向未授權頁面，並記錄日誌
11. **測試模式**：開發測試時可以透過 web.config 設定暫時停用權限檢查

#### 2.1.3 BypassAuthorize 略過權限檢查

```csharp
/// <summary>
/// 用來標示特定 controller 或 action 將略過權限檢核
/// <para>例如: /Login/Role 不應該進行權限檢核</para>
/// </summary>
public class BypassAuthorize : AuthorizeAttribute
{
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        // 不作任何動作, 直接 return
        // 重要!! 不然會回應 401.0 - Unauthorized
        return;
    }
}
```

**程式碼說明：**

1. **繼承 AuthorizeAttribute**：與 LoginRequired 相同，繼承 ASP.NET MVC 的授權機制
2. **OnAuthorization 覆寫**：覆寫授權方法但不執行任何檢查
3. **使用情境**：適用於已登入但不需要檢查功能權限的頁面，例如：
   - 登入後的角色選擇頁面（/Login/Role）
   - 個人資料維護頁面
   - 系統公告頁面
4. **與 AllowAnonymous 的差異**：
   - AllowAnonymous：允許匿名存取，不需登入
   - BypassAuthorize：需要登入，但不檢查功能權限

### 1.2 密碼加密機制

**檔案位置：** `Commons/DataUtils.cs`

系統使用 SHA-256 演算法對密碼進行雜湊處理，確保密碼安全性。

#### 1.2.1 Crypt256 密碼加密方法

```csharp
/// <summary>
/// 使用 SHA-256 演算法對密碼進行雜湊處理
/// SHA-256 提供 256 位元的雜湊值，安全性高於 MD5 和 SHA-1
/// </summary>
/// <param name="pwdstr">原始密碼字串</param>
/// <returns>雜湊後的密碼字串（64 個十六進位字元）</returns>
public static string Crypt256(string pwdstr)
{
    // 第一步：檢查輸入參數
    if (string.IsNullOrEmpty(pwdstr))
    {
        return null;  // 空密碼回傳 null
    }

    // 第二步：建立 SHA-256 雜湊演算法實例
    using (SHA256 sha = SHA256.Create())
    {
        // 第三步：將密碼字串轉換為 UTF-8 位元組陣列
        byte[] inputBytes = Encoding.UTF8.GetBytes(pwdstr);

        // 第四步：計算雜湊值
        byte[] hashBytes = sha.ComputeHash(inputBytes);

        // 第五步：將雜湊位元組陣列轉換為十六進位字串
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            // 每個位元組轉換為兩位十六進位小寫字元
            builder.Append(hashBytes[i].ToString("x2"));
        }

        // 第六步：回傳 64 個字元的雜湊字串
        return builder.ToString();
    }
}
```

**程式碼說明：**

1. **SHA-256 演算法**：使用 SHA-256 雜湊演算法，提供 256 位元（32 位元組）的雜湊值
2. **單向加密**：雜湊是單向的，無法從雜湊值反推原始密碼
3. **固定長度輸出**：無論輸入密碼長度，輸出都是 64 個十六進位字元
4. **UTF-8 編碼**：使用 UTF-8 編碼支援多國語言字元
5. **十六進位轉換**：將位元組陣列轉換為小寫十六進位字串，方便儲存和比對
6. **安全性考量**：SHA-256 比 MD5 和 SHA-1 更安全，不易被碰撞攻擊

## 2. 登入流程實作（Login Process）

### 2.1 登入控制器 C101MController

**檔案位置：** `Areas/Login/Controllers/C101MController.cs`

C101MController 負責處理使用者登入請求，支援兩種登入方式：帳號密碼登入和醫院授權碼登入。

#### 2.1.1 Login Action 方法

```csharp
/// <summary>
/// 使用者按下登入按鈕
/// 處理登入請求，驗證帳號密碼或授權碼，建立 Session
/// </summary>
/// <param name="form">登入表單資料</param>
/// <returns>登入成功導向首頁，失敗返回登入頁面</returns>
[HttpPost]
[AllowAnonymous]  // 允許匿名存取，因為使用者尚未登入
public ActionResult Login(C101MFormModel form)
{
    ActionResult rtn;
    try
    {
        // 第一步：取得使用者 IP 位址
        var IP = HttpContext.Request.UserHostAddress;

        // 第二步：建立系統管理邏輯服務
        ClamService service = new ClamService();

        // 第三步：檢查驗證碼及輸入欄位
        this.InputValidate(form);

        // 第四步：登入帳密檢核，並取得使用者帳號及權限角色清單資料
        LoginUserInfo userInfo = null;
        if (form.ThePage == "1")
        {
            // 帳號密碼登入
            userInfo = service.LoginValidate(form.UserNo, form.UserPwd, IP);
        }
        if (form.ThePage == "2")
        {
            // 醫院授權碼登入
            userInfo = service.LoginValidate_Hosp(form.AuthCode, form.AuthCode_Pwd, IP);
            LoginDAO dao = new LoginDAO();
            var tmpObj = dao.GetRow(new TblEEC_Hospital() { AuthCode = form.AuthCode });
            if (tmpObj != null) userInfo.HospitalCode = tmpObj.code;
        }
        if (userInfo == null) return Redirect("~/Home/Index");

        // 第五步：設定登入資訊
        userInfo.LoginIP = IP;
        userInfo.LoginTab = form.ThePage;

        // 第六步：登入失敗，丟出錯誤訊息
        if (!userInfo.LoginSuccess)
        {
            throw new LoginExceptions(userInfo.LoginErrMessage);
        }

        // 第七步：將登入者資訊保存在 SessionModel 中
        SessionModel sm = SessionModel.Get();
        sm.UserInfo = userInfo;

        // 第八步：取得帳號單位（僅帳號密碼登入）
        if (form.ThePage == "1")
        {
            LoginDAO dao = new LoginDAO();
            TblAMUROLE ar = new TblAMUROLE();
            ar.userno = form.UserNo;
            var ar_data = dao.GetRow(ar);
            TblAMGRP ag = new TblAMGRP();
            ag.grp_id = ar_data.grp_id;
            var ag_data = dao.GetRow(ag);
            sm.UserInfo.User.UNIT_NAME = ag_data.grpname;
        }

        // 第九步：記錄登入成功日誌
        LOG.Info("Login(" + (form.ThePage == "1" ? form.UserNo : form.AuthCode) +
                 ") Success from " + Request.UserHostAddress);

        // 第十步：導向首頁或原始請求頁面
        string redirectPath = (string)TempData["RedirectPath"];
        if (!string.IsNullOrEmpty(redirectPath))
        {
            // 返回原始請求頁面
            rtn = Redirect("~/" + redirectPath);
        }
        else
        {
            // 導向首頁
            rtn = Redirect("~/Home/Index");
        }
    }
    catch (LoginExceptions ex)
    {
        // 第十一步：登入失敗處理
        if (form.ThePage == "1") LOG.Info("Login(" + form.UserNo + ") Failed from " +
                                          Request.UserHostAddress + ": " + ex.Message);
        if (form.ThePage == "2") LOG.Info("Login(" + form.AuthCode + ") Failed from " +
                                          Request.UserHostAddress + ": " + ex.Message);

        // 清除不想要 Cache POST data 的欄位
        ModelState.Remove("form.ValidateCode");

        // 第十二步：返回登入頁面並顯示錯誤訊息
        C101MFormModel model = new C101MFormModel();
        model.UserNo = form.UserNo;      // 帳號
        model.AuthCode = form.AuthCode;  // 醫院授權碼
        model.ThePage = form.ThePage;
        model.ErrorMessage = ex.Message;
        rtn = View("Index", model);
    }
    return rtn;
}
```

**程式碼說明：**

1. **AllowAnonymous 標記**：允許匿名存取，因為使用者尚未登入
2. **IP 位址記錄**：記錄使用者登入的 IP 位址，用於安全稽核
3. **驗證碼檢查**：InputValidate 方法檢查驗證碼是否正確
4. **雙登入方式**：支援帳號密碼登入（ThePage=1）和醫院授權碼登入（ThePage=2）
5. **LoginValidate 呼叫**：透過 ClamService 進行帳號密碼驗證
6. **Session 建立**：登入成功後將使用者資訊儲存到 SessionModel
7. **單位資訊載入**：帳號密碼登入時載入使用者所屬單位資訊
8. **重導處理**：登入成功後返回原始請求頁面或導向首頁
9. **錯誤處理**：登入失敗時記錄日誌並返回登入頁面顯示錯誤訊息
10. **ModelState 清除**：清除驗證碼欄位，避免 POST data 被快取

### 2.2 ClamService 權限管理服務

**檔案位置：** `Services/ClamService.cs`

ClamService 提供登入驗證和權限管理的核心業務邏輯。

#### 2.2.1 LoginValidate 帳號密碼驗證

```csharp
/// <summary>
/// 登入帳密檢核，並取得使用者帳號及權限角色清單資料
/// </summary>
/// <param name="userNo">使用者帳號</param>
/// <param name="userPwd">使用者密碼（明文）</param>
/// <param name="IP">登入 IP 位址</param>
/// <returns>登入使用者資訊</returns>
public LoginUserInfo LoginValidate(string userNo, string userPwd, string IP)
{
    // 第一步：建立 LoginUserInfo 物件
    LoginUserInfo userInfo = new LoginUserInfo();
    userInfo.UserNo = userNo;
    userInfo.LoginSuccess = false;

    // 第二步：密碼加密
    string userPwd_encry = DataUtils.Crypt256(userPwd);

    // 第三步：呼叫 DAO 進行資料庫驗證
    LoginDAO dao = new LoginDAO();
    userInfo = dao.LoginValidate(userNo, userPwd_encry);

    // 第四步：設定登入資訊
    userInfo.LoginIP = IP;
    userInfo.LoginAuth = "MEMBER";  // 一般帳密登入

    // 第五步：檢查登入是否成功
    if (!userInfo.LoginSuccess)
    {
        // 登入失敗，回傳錯誤訊息
        return userInfo;
    }

    // 第六步：載入使用者角色權限
    userInfo.Groups = dao.GetUserGroups(userNo);

    // 第七步：載入使用者可存取的功能清單
    IList<ClamRoleFunc> roleFuncs = dao.GetUserRoleFuncs(userNo);

    // 第八步：將角色權限儲存到 Session
    SessionModel sm = SessionModel.Get();
    sm.RoleFuncs = roleFuncs;

    return userInfo;
}
```

**程式碼說明：**

1. **LoginUserInfo 初始化**：建立登入使用者資訊物件，預設登入失敗
2. **密碼加密**：使用 Crypt256 方法將明文密碼轉換為 SHA-256 雜湊值
3. **資料庫驗證**：呼叫 LoginDAO.LoginValidate 進行帳號密碼驗證
4. **登入資訊設定**：設定登入 IP 和登入方式（MEMBER）
5. **失敗處理**：登入失敗時直接回傳，不載入權限資料
6. **角色載入**：載入使用者所屬的群組（角色）清單
7. **權限載入**：載入使用者可存取的功能清單
8. **Session 儲存**：將角色權限清單儲存到 SessionModel，供後續權限檢查使用

#### 2.2.2 LoginValidate_Hosp 醫院授權碼驗證

```csharp
/// <summary>
/// 醫院授權碼登入驗證
/// </summary>
/// <param name="authCode">醫院授權碼</param>
/// <param name="authCode_Pwd">授權碼密碼</param>
/// <param name="IP">登入 IP 位址</param>
/// <returns>登入使用者資訊</returns>
public LoginUserInfo LoginValidate_Hosp(string authCode, string authCode_Pwd, string IP)
{
    // 第一步：建立 LoginUserInfo 物件
    LoginUserInfo userInfo = new LoginUserInfo();
    userInfo.UserNo = authCode;
    userInfo.LoginSuccess = false;

    // 第二步：密碼加密
    string authCode_Pwd_encry = DataUtils.Crypt256(authCode_Pwd);

    // 第三步：呼叫 DAO 進行資料庫驗證
    LoginDAO dao = new LoginDAO();
    userInfo = dao.LoginValidate_Hosp(authCode, authCode_Pwd_encry);

    // 第四步：設定登入資訊
    userInfo.LoginIP = IP;
    userInfo.LoginAuth = "HOSPITAL";  // 醫院授權碼登入

    // 第五步：檢查登入是否成功
    if (!userInfo.LoginSuccess)
    {
        // 登入失敗，回傳錯誤訊息
        return userInfo;
    }

    // 第六步：載入醫院授權碼的權限（通常有固定的權限範圍）
    IList<ClamRoleFunc> roleFuncs = dao.GetHospitalRoleFuncs(authCode);

    // 第七步：將角色權限儲存到 Session
    SessionModel sm = SessionModel.Get();
    sm.RoleFuncs = roleFuncs;

    return userInfo;
}
```

**程式碼說明：**

1. **授權碼驗證**：與帳號密碼登入類似，但使用授權碼作為帳號
2. **密碼加密**：同樣使用 SHA-256 加密授權碼密碼
3. **資料庫驗證**：呼叫 LoginDAO.LoginValidate_Hosp 進行授權碼驗證
4. **登入方式標記**：設定 LoginAuth 為 "HOSPITAL"，區分登入方式
5. **權限載入**：載入醫院授權碼對應的功能權限
6. **Session 儲存**：將權限清單儲存到 SessionModel

## 3. Session 管理（Session Management）

### 3.1 SessionModel Session 管理類別

**檔案位置：** `Models/SessionModel.cs`

SessionModel 負責管理使用者的 Session 資訊，包含登入狀態、使用者資訊、角色權限等。

#### 3.1.1 SessionModel 類別定義

```csharp
/// <summary>
/// Session 管理類別
/// 負責管理使用者的 Session 資訊，包含登入狀態、使用者資訊、角色權限等
/// </summary>
public class SessionModel
{
    /// <summary>
    /// 日誌記錄器
    /// </summary>
    protected static readonly ILog logger = LogManager.GetLogger(
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Session 常數定義
    /// </summary>
    private const string USER_INFO = "USER_INFO";              // 使用者資訊
    private const string ROLE_FUNCS = "ROLE_FUNCS";            // 角色權限清單
    private const string VALIDATE_CODE = "VALIDATE_CODE";      // 驗證碼
    private const string LAST_ACTION_PATH = "LAST_ACTION_PATH";  // 最後執行的 Action 路徑
    private const string LAST_ACTION_FUNC = "LAST_ACTION_FUNC";  // 最後執行的功能定義
    private const string LAST_ERROR_MESSAGE = "LAST_ERROR_MESSAGE";  // 錯誤訊息

    /// <summary>
    /// HttpSessionStateBase 包裝器
    /// </summary>
    private HttpSessionStateBase _session;

    /// <summary>
    /// Session 屬性，提供 null 檢查
    /// </summary>
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

    /// <summary>
    /// 私有建構子，防止外部直接建立實例
    /// </summary>
    private SessionModel()
    {
        // 第一步：包裝 HttpContext.Current.Session
        this._session = new HttpSessionStateWrapper(HttpContext.Current.Session);
        if (this._session == null)
        {
            throw new NullReferenceException("HttpContext.Current.Session");
        }

        // 第二步：設定 Session 逾時時間為 60 分鐘
        _session.Timeout = 60;

        // 第三步：記錄 Session 建立日誌
        logger.Debug("SessionModel(), SessionID=" + _session.SessionID);
    }

    /// <summary>
    /// 取得/建立 Login SessionModel
    /// 使用 Singleton 模式，每次呼叫都回傳新的實例但共用相同的 Session
    /// </summary>
    /// <returns>SessionModel 實例</returns>
    public static SessionModel Get()
    {
        return new SessionModel();
    }
}
```

**程式碼說明：**

1. **常數定義**：定義 Session 中儲存的各種資料的 Key 值
2. **HttpSessionStateBase 包裝**：使用 HttpSessionStateBase 介面，方便單元測試
3. **Null 檢查**：session 屬性提供 null 檢查，避免 NullReferenceException
4. **私有建構子**：防止外部直接建立實例，強制使用 Get() 方法
5. **Session 逾時設定**：設定 Session 逾時時間為 60 分鐘
6. **日誌記錄**：記錄 Session 建立和 SessionID，方便追蹤問題
7. **Singleton 模式**：每次呼叫 Get() 都回傳新的 SessionModel 實例，但共用相同的 HttpContext.Current.Session

#### 3.1.2 SessionModel 主要屬性

```csharp
/// <summary>
/// 登入者使用者帳號資訊
/// 使用 JSON 序列化儲存，支援複雜物件
/// </summary>
public LoginUserInfo UserInfo
{
    get
    {
        // 第一步：從 Session 取得 JSON 字串
        LoginUserInfo userInfo = null;
        string jsonUserInfo = (string)this.session[USER_INFO];

        // 第二步：反序列化為 LoginUserInfo 物件
        if (!string.IsNullOrWhiteSpace(jsonUserInfo))
        {
            userInfo = JsonConvert.DeserializeObject<LoginUserInfo>(jsonUserInfo);
        }

        return userInfo;
    }
    set
    {
        // 第一步：設定預設使用者類型
        if (value != null && value.UserType == null)
        {
            value.UserType = LoginUserType.SKILL_USER;
        }

        // 第二步：序列化為 JSON 字串並儲存到 Session
        this.session[USER_INFO] = JsonConvert.SerializeObject(value);
    }
}

/// <summary>
/// 使用者可存取的功能清單（角色權限）
/// </summary>
public IList<ClamRoleFunc> RoleFuncs
{
    get
    {
        // 從 Session 取得角色權限清單
        IList<ClamRoleFunc> funcs = null;
        string jsonFuncs = (string)this.session[ROLE_FUNCS];
        if (!string.IsNullOrWhiteSpace(jsonFuncs))
        {
            funcs = JsonConvert.DeserializeObject<IList<ClamRoleFunc>>(jsonFuncs);
        }
        return funcs;
    }
    set
    {
        // 序列化為 JSON 字串並儲存到 Session
        this.session[ROLE_FUNCS] = JsonConvert.SerializeObject(value);
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
/// 最後執行的 Action 路徑
/// 例如：A1/SE11/Index
/// </summary>
public string LastActionPath
{
    get { return (string)this.session[LAST_ACTION_PATH]; }
    set { this.session[LAST_ACTION_PATH] = value; }
}

/// <summary>
/// 最後執行的功能定義
/// 從 TblAMFUNCM 取得的功能資訊
/// </summary>
public TblAMFUNCM LastActionFunc
{
    get
    {
        TblAMFUNCM func = null;
        string jsonFunc = (string)this.session[LAST_ACTION_FUNC];
        if (!string.IsNullOrWhiteSpace(jsonFunc))
        {
            func = JsonConvert.DeserializeObject<TblAMFUNCM>(jsonFunc);
        }
        return func;
    }
    set
    {
        this.session[LAST_ACTION_FUNC] = JsonConvert.SerializeObject(value);
    }
}

/// <summary>
/// 錯誤訊息（自動清除機制）
/// 讀取後自動清除，避免重複顯示
/// </summary>
public string LastErrorMessage
{
    get
    {
        string message = (string)this.session[LAST_ERROR_MESSAGE];
        this.session[LAST_ERROR_MESSAGE] = null;  // 讀取後清除
        return (string.IsNullOrEmpty(message) ? null : message);
    }
    set { this.session[LAST_ERROR_MESSAGE] = value; }
}
```

**程式碼說明：**

1. **UserInfo 屬性**：

   - 使用 JSON 序列化儲存複雜物件
   - 支援 LoginUserInfo 的所有屬性
   - 自動設定預設使用者類型

2. **RoleFuncs 屬性**：

   - 儲存使用者的角色權限清單
   - 用於 LoginRequired Filter 的權限檢查
   - 使用 JSON 序列化支援 IList 集合

3. **LoginValidateCode 屬性**：

   - 儲存登入驗證碼（圖形驗證碼）
   - 用於登入時的驗證碼檢查

4. **LastActionPath 屬性**：

   - 記錄最後執行的 Action 路徑
   - 用於錯誤追蹤和日誌記錄

5. **LastActionFunc 屬性**：

   - 記錄最後執行的功能定義
   - 包含功能名稱、系統 ID、模組等資訊

6. **LastErrorMessage 屬性**：
   - 儲存錯誤訊息
   - 讀取後自動清除，避免重複顯示
   - 適用於頁面重導後顯示錯誤訊息

### 3.2 LoginUserInfo 使用者資訊模型

**檔案位置：** `Models/LoginUserInfo.cs`

LoginUserInfo 儲存登入使用者的完整資訊，包含帳號、密碼驗證結果、角色、權限等。

#### 3.2.1 LoginUserInfo 類別定義

```csharp
/// <summary>
/// 系統登入使用者資訊
/// </summary>
public class LoginUserInfo
{
    /// <summary>
    /// 建構子，預設登入失敗
    /// </summary>
    public LoginUserInfo()
    {
        LoginSuccess = false;
    }

    /// <summary>
    /// 登入成功與否
    /// </summary>
    public bool LoginSuccess { get; set; }

    /// <summary>
    /// 登入失敗時的錯誤訊息
    /// </summary>
    public string LoginErrMessage { get; set; }

    /// <summary>
    /// 登入時輸入的帳號
    /// </summary>
    public string UserNo { get; set; }

    /// <summary>
    /// 登入方式
    /// <para>1: 帳號密碼登入</para>
    /// <para>2: 醫院授權碼登入</para>
    /// </summary>
    public string LoginTab { get; set; }

    /// <summary>
    /// 醫院代號（僅供醫院授權碼登入時使用）
    /// </summary>
    public string HospitalCode { get; set; }

    /// <summary>
    /// 是否須強制變更密碼
    /// </summary>
    public bool ChangePwdRequired { get; set; }

    /// <summary>
    /// 是否須強制變更個人資料
    /// </summary>
    public bool ChangeDetailRequired { get; set; }

    /// <summary>
    /// 使用者登入區域：1.內網，2.外網
    /// </summary>
    public string NetID { get; set; }

    /// <summary>
    /// 登入驗證方式：
    /// <para>MEMBER: 一般帳密登入</para>
    /// <para>HOSPITAL: 醫院授權碼登入</para>
    /// </summary>
    public string LoginAuth { get; set; }

    /// <summary>
    /// 登入來源 IP
    /// </summary>
    public string LoginIP { get; set; }

    /// <summary>
    /// 密碼最後修改時間
    /// </summary>
    public DateTime? PWDMODTIME { get; set; }

    /// <summary>
    /// 登入的使用者類型（預設為：SKILL_USER）
    /// </summary>
    public LoginUserType UserType { get; set; }

    /// <summary>
    /// 使用者帳號資料
    /// </summary>
    public ClamUser User { get; set; }

    /// <summary>
    /// 使用者群組清單（角色）
    /// </summary>
    public IList<ClamUserGroup> Groups { get; set; }
}
```

**程式碼說明：**

1. **LoginSuccess**：登入是否成功的旗標
2. **LoginErrMessage**：登入失敗時的錯誤訊息
3. **UserNo**：登入時輸入的帳號
4. **LoginTab**：登入方式（1=帳號密碼，2=醫院授權碼）
5. **HospitalCode**：醫院代號，僅醫院授權碼登入時使用
6. **ChangePwdRequired**：是否須強制變更密碼
7. **ChangeDetailRequired**：是否須強制變更個人資料
8. **LoginAuth**：登入驗證方式（MEMBER/HOSPITAL）
9. **LoginIP**：登入來源 IP 位址
10. **UserType**：使用者類型（SKILL_USER）
11. **User**：使用者帳號資料（ClamUser）
12. **Groups**：使用者群組清單（角色）

## 4. 資料模型定義（Data Models）

### 4.1 ClamRoleFunc 角色功能權限

```csharp
/// <summary>
/// 角色功能權限
/// 定義使用者角色可以存取的功能
/// </summary>
public class ClamRoleFunc
{
    /// <summary>
    /// 系統 ID
    /// </summary>
    public string sysid { get; set; }

    /// <summary>
    /// 模組
    /// </summary>
    public string modules { get; set; }

    /// <summary>
    /// 子模組
    /// </summary>
    public string submodules { get; set; }

    /// <summary>
    /// 功能名稱
    /// </summary>
    public string funcname { get; set; }
}
```

### 4.2 TblAMFUNCM 功能定義資料表

```csharp
/// <summary>
/// 功能定義資料表
/// 定義系統中所有可用的功能
/// </summary>
public class TblAMFUNCM
{
    /// <summary>
    /// 系統 ID
    /// </summary>
    public string sysid { get; set; }

    /// <summary>
    /// 模組
    /// </summary>
    public string modules { get; set; }

    /// <summary>
    /// 子模組
    /// </summary>
    public string submodules { get; set; }

    /// <summary>
    /// 功能 ID（對應 Area/Controller 路徑）
    /// 例如：A1/SE11
    /// </summary>
    public string prgid { get; set; }

    /// <summary>
    /// 功能名稱
    /// </summary>
    public string prgname { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public string enabled { get; set; }
}
```

## 5. 完整身份驗證與授權流程

### 5.1 登入流程

```
1. 使用者開啟登入頁面（/Login/C101M/Index）
   ↓
2. 輸入帳號、密碼、驗證碼
   ↓
3. 按下登入按鈕，POST 到 /Login/C101M/Login
   ↓
4. C101MController.Login 方法處理
   ├─ 檢查驗證碼
   ├─ 密碼加密（SHA-256）
   ├─ 呼叫 ClamService.LoginValidate
   │  ├─ 呼叫 LoginDAO.LoginValidate
   │  │  └─ 查詢資料庫驗證帳號密碼
   │  ├─ 載入使用者資訊
   │  ├─ 載入使用者群組（角色）
   │  └─ 載入使用者權限清單
   └─ 將使用者資訊儲存到 SessionModel
   ↓
5. 登入成功，導向首頁或原始請求頁面
```

### 5.2 權限檢查流程

```
1. 使用者請求受保護的頁面（例如：/A1/SE11/Index）
   ↓
2. LoginRequired Filter 攔截請求
   ├─ 取得 Action 路徑（A1/SE11/Index）
   ├─ 轉換為功能路徑（A1/SE11）
   └─ 取得 SessionModel
   ↓
3. 檢查是否有 AllowAnonymous 或 BypassAuthorize 標記
   ├─ 有 AllowAnonymous → 允許存取
   └─ 有 BypassAuthorize → 檢查登入狀態後允許存取
   ↓
4. 檢查登入狀態
   ├─ 未登入 → 導向登入頁面
   └─ 已登入 → 繼續權限檢查
   ↓
5. 比對功能定義
   ├─ 從 ApplicationModel.GetClamFuncsAll() 取得所有功能定義
   ├─ 比對功能路徑（A1/SE11）
   └─ 找到對應的 TblAMFUNCM 記錄
   ↓
6. 檢查角色權限
   ├─ 從 SessionModel.RoleFuncs 取得使用者權限清單
   ├─ 比對 sysid、modules、submodules
   ├─ 有權限 → 允許存取
   └─ 無權限 → 導向未授權頁面
   ↓
7. 執行 Action 方法
```

## 6. 安全性最佳實踐

### 6.1 密碼安全

1. **使用強雜湊演算法**：使用 SHA-256 而非 MD5 或 SHA-1
2. **不儲存明文密碼**：資料庫只儲存雜湊值
3. **密碼複雜度要求**：強制使用者設定複雜密碼
4. **定期變更密碼**：透過 ChangePwdRequired 強制變更密碼

### 6.2 Session 安全

1. **Session 逾時設定**：設定適當的逾時時間（60 分鐘）
2. **Session ID 保護**：使用 HTTPS 傳輸，防止 Session Hijacking
3. **登出清除 Session**：登出時清除所有 Session 資料
4. **Session 固定攻擊防護**：登入成功後重新產生 Session ID

### 6.3 權限控制

1. **最小權限原則**：只授予必要的權限
2. **權限分離**：區分不同角色的權限
3. **權限檢查**：每個 Action 都必須檢查權限
4. **日誌記錄**：記錄所有授權相關的操作

### 6.4 防止攻擊

1. **SQL Injection 防護**：使用參數化查詢
2. **XSS 防護**：對使用者輸入進行編碼
3. **CSRF 防護**：使用 AntiForgeryToken
4. **暴力破解防護**：登入失敗鎖定機制（5 次失敗鎖定 15 分鐘）

## 7. 開發與維護指南

### 7.1 新增需要登入的 Controller

```csharp
/// <summary>
/// 需要登入的 Controller
/// </summary>
[LoginRequired]  // 套用 LoginRequired Filter
public class MyController : Controller
{
    /// <summary>
    /// 需要登入且需要權限的 Action
    /// </summary>
    public ActionResult Index()
    {
        // 取得登入使用者資訊
        SessionModel sm = SessionModel.Get();
        LoginUserInfo userInfo = sm.UserInfo;

        // 業務邏輯
        return View();
    }

    /// <summary>
    /// 需要登入但不需要權限的 Action
    /// </summary>
    [BypassAuthorize]
    public ActionResult Profile()
    {
        // 個人資料頁面，不需要檢查功能權限
        return View();
    }
}
```

### 7.2 新增允許匿名存取的 Action

```csharp
/// <summary>
/// 需要登入的 Controller
/// </summary>
[LoginRequired]
public class MyController : Controller
{
    /// <summary>
    /// 允許匿名存取的 Action
    /// </summary>
    [AllowAnonymous]
    public ActionResult Public()
    {
        // 公開頁面，不需要登入
        return View();
    }
}
```

### 7.3 新增功能定義

1. 在資料庫 TblAMFUNCM 資料表新增功能定義
2. 設定 prgid 為 Area/Controller 路徑（例如：A1/SE11）
3. 設定 sysid、modules、submodules
4. 設定 prgname 為功能名稱
5. 設定 enabled = 'Y' 啟用功能

### 7.4 授予使用者權限

1. 在資料庫 TblAMROLEFUNC 資料表新增角色權限
2. 設定 role_id 為角色 ID
3. 設定 sysid、modules、submodules 對應功能定義
4. 使用者登入時會自動載入權限清單

## 8. 總結

本文件詳細說明了 EECOnline 系統的身份驗證與授權機制，包含：

1. **LoginRequired Filter**：登入與權限檢查的核心機制

   - OnAuthorization 方法實作
   - AllowAnonymous 和 BypassAuthorize 的使用
   - 權限比對流程

2. **密碼加密機制**：使用 SHA-256 雜湊演算法

   - Crypt256 方法實作
   - 安全性考量

3. **登入流程**：支援兩種登入方式

   - 帳號密碼登入
   - 醫院授權碼登入
   - ClamService 和 LoginDAO 的協作

4. **Session 管理**：SessionModel 的設計與使用

   - Session 資料儲存
   - JSON 序列化
   - 自動清除機制

5. **資料模型**：LoginUserInfo、ClamRoleFunc、TblAMFUNCM

   - 使用者資訊模型
   - 角色權限模型
   - 功能定義模型

6. **安全性最佳實踐**：

   - 密碼安全
   - Session 安全
   - 權限控制
   - 防止攻擊

7. **開發與維護指南**：
   - 新增 Controller 和 Action
   - 新增功能定義
   - 授予使用者權限

完整的程式碼範例和詳細說明請參考補充文件：`06-1_身份驗證與授權補充_登入流程與Session管理.md`
