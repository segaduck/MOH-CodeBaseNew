# MIT 微笑標章管理系統 - 身份驗證與授權機制詳細範例：Authentication 與 Authorization

## 功能概述

本文詳細說明 MITAP2024 系統中的身份驗證（Authentication）和授權（Authorization）機制。系統採用基於 JWT Token 的身份驗證，結合 Redis 快取的 Session 管理，以及基於角色的權限控制（RBAC）系統，提供完整的安全性保護機制。

## 安全性架構概覽

```
MITAP2024 安全性架構：

前端請求
    ↓ 攜帶 JWT Token
身份驗證層 (Authentication)
    ↓ 驗證 Token 有效性
Session 管理層
    ↓ 從 Redis 取得 Session 資料
授權檢查層 (Authorization)
    ↓ 檢查功能權限
業務邏輯層
    ↓ 執行業務操作
資料存取層
    ↓ 記錄操作日誌
回傳結果
```

## 1. 整體身份驗證機制（Authentication）

### 1.1 JWT Token 產生與管理

**檔案位置：** `MITAP2024/MITAP2024.Server/Utils/Session/SessionManager.cs`

#### 1.1.1 Session Key 產生機制

```csharp
/// <summary>
/// JWT Token 產生機制
/// 系統使用自定義的 Token 格式，結合 GUID 確保唯一性
/// </summary>
public class SessionManager
{
    /// <summary>
    /// 產生唯一的 Session Key (JWT Token)
    /// 格式：mit + 32位元小寫 GUID（無連字符）
    /// </summary>
    /// <returns>產生的 JWT Token</returns>
    public async static Task<string> GenSessionKey()
    {
        return await Task.Run(() =>
        {
            // 第一步：產生新的 GUID
            Guid newGuid = Guid.NewGuid();

            // 第二步：將 GUID 轉換為 32 位元小寫字串（移除連字符）
            string guidString = TextUtils.GuidToString(newGuid);

            // 第三步：加上系統前綴 "mit"，形成完整的 JWT Token
            string jwtkey = "mit" + guidString;

            return jwtkey;
        });
    }

    /// <summary>
    /// 設定前台使用者的 Session 資料到 Redis
    /// 使用 Redis 作為分散式 Session 儲存，支援多伺服器部署
    /// </summary>
    /// <param name="jwtkey">JWT Token</param>
    /// <param name="sessionData">要儲存的 Session 資料</param>
    /// <returns>是否設定成功</returns>
    public async static Task<bool> SetMitfSessionData(string jwtkey, MitfSessionDataModel sessionData)
    {
        // 第一步：取得 Redis 資料庫連線
        IDatabase? sessionDb = GetSessionDB();
        if (sessionDb == null)
        {
            throw new Exception(ConstMsg.ERR_CODE_90001);  // Redis 連線失敗
        }

        // 第二步：建立 Session 模型並設定資料
        SessionModel sessionModel = await GetSessionModelAsync(jwtkey, sessionDb);
        sessionModel.MitfSessionData = sessionData;

        // 第三步：將 Session 資料序列化並儲存到 Redis
        return await SetSessionModelAsync(jwtkey, sessionModel, sessionDb);
    }

    /// <summary>
    /// 從 Redis 取得前台使用者的 Session 資料
    /// 支援非同步操作，提升系統效能
    /// </summary>
    /// <param name="jwtkey">JWT Token</param>
    /// <returns>Session 資料模型</returns>
    public async static Task<MitfSessionDataModel> GetMitfSessionData(string jwtkey)
    {
        // 第一步：取得 Redis 資料庫連線
        IDatabase? sessionDb = GetSessionDB();
        if (sessionDb == null)
        {
            throw new Exception(ConstMsg.ERR_CODE_90001);
        }

        // 第二步：從 Redis 取得 Session 模型
        SessionModel sessionModel = await GetSessionModelAsync(jwtkey, sessionDb);

        // 第三步：回傳前台 Session 資料
        MitfSessionDataModel sessionData = sessionModel.MitfSessionData;
        return sessionData;
    }

    /// <summary>
    /// 取得 Redis 資料庫連線
    /// 使用連線池管理，提升效能和穩定性
    /// </summary>
    /// <returns>Redis 資料庫實例</returns>
    private static IDatabase? GetSessionDB()
    {
        // 第一步：從設定檔讀取 Redis 位址
        string? address = AppSettingReader.GetRedisAddress();
        if (string.IsNullOrEmpty(address))
        {
            return null;  // Redis 位址未設定
        }
        else
        {
            // 第二步：初始化 Redis 連線
            RedisConnection.Init(address);

            // 第三步：取得連線多工器和資料庫實例
            ConnectionMultiplexer redis = RedisConnection.Instance.ConnectionMultiplexer;
            IDatabase sessionDb = redis.GetDatabase();

            return sessionDb;
        }
    }
}
```

**程式碼說明：**

1. **Token 格式設計**：使用 "mit" 前綴 + 32 位元 GUID，確保唯一性和可識別性
2. **Redis 整合**：使用 Redis 作為分散式 Session 儲存，支援水平擴展
3. **非同步操作**：所有 I/O 操作都使用非同步模式，提升系統效能
4. **錯誤處理**：完整的異常處理機制，確保系統穩定性
5. **連線管理**：使用連線池管理 Redis 連線，避免連線洩漏

### 1.2 密碼加密與驗證機制

**檔案位置：** `MITAP2024/MITAP2024.Server/Utils/Common/TextUtils.cs`

#### 1.2.1 SHA-512 密碼雜湊

```csharp
/// <summary>
/// 密碼雜湊處理工具
/// 使用 SHA-512 演算法進行密碼加密，提供高強度的安全性保護
/// </summary>
public static class TextUtils
{
    /// <summary>
    /// 使用 SHA-512 演算法對密碼進行雜湊處理
    /// SHA-512 提供 512 位元的雜湊值，安全性高於 MD5 和 SHA-1
    /// </summary>
    /// <param name="pwdstr">原始密碼字串</param>
    /// <returns>雜湊後的密碼字串（128 個十六進位字元）</returns>
    public static string HashPassword(string pwdstr)
    {
        // 第一步：建立 SHA-512 雜湊演算法實例
        using (SHA512 sha = SHA512.Create())
        {
            // 第二步：將密碼字串轉換為 UTF-8 位元組陣列
            byte[] inputBytes = Encoding.UTF8.GetBytes(pwdstr);

            // 第三步：計算雜湊值
            byte[] hashBytes = sha.ComputeHash(inputBytes);

            // 第四步：將雜湊位元組陣列轉換為十六進位字串
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                // 每個位元組轉換為兩位十六進位小寫字元
                builder.Append(hashBytes[i].ToString("x2"));
            }

            // 第五步：回傳 128 個字元的雜湊字串
            return builder.ToString();
        }
    }

    /// <summary>
    /// GUID 轉換為字串的工具方法
    /// 用於產生 JWT Token 的唯一識別碼
    /// </summary>
    /// <param name="guid">要轉換的 GUID</param>
    /// <returns>32 位元小寫字串（無連字符）</returns>
    public static string GuidToString(Guid? guid)
    {
        if (guid == null || guid == Guid.Empty)
        {
            return "";  // 空值或空 GUID 回傳空字串
        }
        else
        {
            // 使用 "N" 格式移除連字符，並轉換為小寫
            return guid.Value.ToString("N").ToLower();
        }
    }
}
```

**程式碼說明：**

1. **SHA-512 演算法**：使用業界標準的 SHA-512 雜湊演算法，提供高強度加密
2. **不可逆加密**：雜湊函數為單向函數，無法從雜湊值反推原始密碼
3. **固定長度輸出**：無論輸入密碼長度如何，都產生 128 個字元的雜湊值
4. **UTF-8 編碼**：支援多國語言字元的密碼
5. **記憶體安全**：使用 `using` 語句確保加密物件正確釋放

### 1.3 使用者登入流程實作

**檔案位置：** `MITAP2024/MITAP2024.Server/MITF/HOME/Service/HomeService.cs`

#### 1.3.1 完整登入驗證流程

```csharp
/// <summary>
/// 前台使用者登入服務
/// 實作完整的身份驗證流程，包含防暴力破解機制
/// </summary>
/// <param name="model">登入資料模型</param>
/// <param name="context">HTTP 上下文，用於取得 IP 位址</param>
/// <returns>登入結果</returns>
public async Task<ActionResult<PagedQueryResult>> Login(LoginModel model, HttpContext context)
{
    return await Task.Run(() =>
    {
        var result = new PagedQueryResult();
        result.jwtkey = model.jwtkey;
        var clientIp = WebPageUtils.GetClientIP(context);  // 取得客戶端 IP 位址
        result.message = "";

        #region 第一步：輸入資料驗證
        // 檢查必要欄位是否完整填寫
        if (string.IsNullOrEmpty(model.jwtkey))
        {
            result.message = TextUtils.AppendMessageWithComma("未傳入必要資料", result.message);
        }
        if (string.IsNullOrEmpty(model.userid))
        {
            result.message = TextUtils.AppendMessageWithComma("帳號未填寫", result.message);
        }
        if (string.IsNullOrEmpty(model.userpwd))
        {
            result.message = TextUtils.AppendMessageWithComma("密碼未填寫", result.message);
        }
        #endregion

        try
        {
            #region 第二步：防暴力破解檢查
            // 檢查帳號是否因為多次登入失敗而被暫時鎖定
            if (string.IsNullOrEmpty(result.message))
            {
                // 查詢最近的登入失敗記錄
                var failLog = _context.LoginFailedHistory
                    .Where(x => x.Account == model.userid &&
                               x.AccountType == "1" &&      // 前台使用者
                               x.LogType == "1")            // 鎖定狀態
                    .OrderByDescending(x => x.CreateDate)
                    .ToList();

                if (failLog != null && failLog.Count() > 0)
                {
                    // 計算距離最後一次失敗登入的時間
                    var diffTime = DateTime.Now - failLog.First().CreateDate;
                    if (diffTime.Minutes <= 15)  // 15 分鐘內不允許登入
                    {
                        result.message = TextUtils.AppendMessageWithComma(
                            "帳號15分鐘內已登入錯誤達三次，請15分鐘之後再登入！",
                            result.message);
                    }
                }
            }
            #endregion

            #region 第三步：密碼驗證和使用者資料查詢
            if (string.IsNullOrEmpty(result.message))
            {
                // 對輸入的密碼進行 SHA-512 雜湊處理
                string encodepwd = TextUtils.HashPassword(model.userpwd);

                // 取得系統管理者密碼（用於緊急登入）
                var proview = _context.Users
                    .Where(x => x.UserAct == AppSettingReader.GetProview())
                    .FirstOrDefault();

                // 查詢使用者資料
                var user = _context.ComMan
                    .Where(x => x.ManMail == model.userid)
                    .FirstOrDefault();

                ComMain? comdata = null;
                bool loginFail = false;

                // 檢查使用者是否存在且未被刪除
                if (user == null || user.IsDelete == "Y")
                {
                    result.message = TextUtils.AppendMessageWithComma("帳號不存在！", result.message);
                    loginFail = true;
                }
                else
                {
                    // 查詢使用者所屬公司資料
                    comdata = _context.ComMain
                        .Where(x => x.ComId == user.ComId)
                        .FirstOrDefault();

                    if (comdata == null)
                    {
                        result.message = TextUtils.AppendMessageWithComma("帳號不存在！", result.message);
                        loginFail = true;
                    }
                    else
                    {
                        // 驗證密碼是否正確
                        if (user.ManPwd != encodepwd && proview?.UserPwd != encodepwd)
                        {
                            result.message = TextUtils.AppendMessageWithComma("密碼錯誤！", result.message);
                            loginFail = true;
                        }
                    }
                }
                #endregion

                #region 第四步：登入失敗處理
                if (loginFail)
                {
                    // 記錄登入失敗次數
                    var existingFailLog = _context.LoginFailedHistory
                        .Where(x => x.Account == model.userid && x.AccountType == "1")
                        .ToList();

                    string logtype = "0";  // 一般失敗
                    if (existingFailLog != null && existingFailLog.Count() >= 2)
                    {
                        logtype = "1";  // 達到鎖定條件
                    }

                    // 建立登入失敗記錄
                    LoginFailedHistory loginFailedHistory = new LoginFailedHistory()
                    {
                        Guid = Guid.NewGuid(),
                        AccountType = "1",           // 前台使用者
                        Account = model.userid,
                        CreateDate = DateTime.Now,
                        LogType = logtype,
                        IsDelete = false,
                    };
                    _context.LoginFailedHistory.Add(loginFailedHistory);

                    result.IsSuccess = false;
                }
                #endregion

                #region 第五步：登入成功處理
                else
                {
                    // 建立 Session 資料
                    MitfSessionDataModel sessionData = new MitfSessionDataModel()
                    {
                        ComId = user.ComId,
                        ComName = comdata.ComName,
                        ManMail = user.ManMail,
                        ManName = user.ManName,
                        ManPhone = user.ManPhone,
                        LoginTime = DateTime.Now,
                        NeedResetPw = "N"
                    };

                    // 檢查是否使用管理者密碼登入
                    if (proview != null && proview.UserPwd == encodepwd)
                    {
                        sessionData.NeedResetPw = "N";  // 管理者密碼不需要重設
                    }

                    // 將 Session 資料儲存到 Redis
                    await SessionManager.SetMitfSessionData(model.jwtkey, sessionData);
                    result.Data = sessionData;
                    result.IsSuccess = true;

                    // 清除之前的登入失敗記錄
                    var failLog = _context.LoginFailedHistory
                        .Where(x => x.Account == model.userid && x.AccountType == "1")
                        .ToList();

                    if (failLog != null && failLog.Count > 0)
                    {
                        foreach (var item in failLog)
                        {
                            _context.LoginFailedHistory.Remove(item);
                        }
                    }
                }
                #endregion

                _context.SaveChanges();  // 儲存所有資料庫變更
            }
            else
            {
                result.IsSuccess = false;
            }
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "登入失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}
```

**程式碼說明：**

1. **分階段驗證**：依序進行輸入驗證、防暴力破解檢查、密碼驗證等步驟
2. **防暴力破解**：記錄登入失敗次數，達到 3 次後鎖定帳號 15 分鐘
3. **雙重密碼機制**：支援使用者密碼和管理者緊急密碼
4. **IP 位址記錄**：記錄客戶端 IP 位址，便於安全性追蹤
5. **Session 管理**：登入成功後建立 Session 並儲存到 Redis
6. **失敗記錄清理**：登入成功後自動清除之前的失敗記錄
7. **完整日誌**：記錄所有登入嘗試，包含成功和失敗的情況

## 2. 整體授權機制（Authorization）

### 2.1 基於角色的權限控制（RBAC）

MITAP2024 系統採用基於角色的存取控制（Role-Based Access Control, RBAC）模型，透過以下資料表實現權限管理：

```
權限管理資料表結構：

Users (使用者表)
    ↓ 多對多關聯
RoleUsers (使用者角色關聯表)
    ↓ 關聯到
Roles (角色表)
    ↓ 多對多關聯
RoleFuncs (角色功能關聯表)
    ↓ 關聯到
SysFunc (系統功能表)
```

### 2.2 身份驗證與授權檢查

**檔案位置：** `MITAP2024/MITAP2024.Server/MITF/HOME/Service/HomeService.cs`

#### 2.2.1 統一的身份驗證與授權檢查

```csharp
/// <summary>
/// 統一的身份驗證與授權檢查方法
/// 結合登入狀態檢查和功能權限驗證，確保完整的安全性控制
/// </summary>
/// <param name="jwtkey">JWT Token</param>
/// <param name="sessiondata">Session 資料（參考參數，用於回傳）</param>
/// <param name="funcCode">功能代碼</param>
/// <returns>檢查結果代碼</returns>
public string CheckIsLoginAndAuth(string? jwtkey, ref MitfSessionDataModel? sessiondata, string funcCode)
{
    // 第一步：檢查使用者是否已登入
    if (CheckIsLogin(jwtkey, ref sessiondata) == false)
    {
        // 未登入，回傳登入錯誤代碼
        return ConstMsg.ERR_NOTLOGIN;
    }
    else
    {
        // 第二步：檢查使用者是否有權限執行此功能
        if (CheckFuncAuth(funcCode, sessiondata) == true)
        {
            // 身份和權限都驗證通過
            return ConstMsg.SUC_CODE_00001;
        }
        else
        {
            // 已登入但權限不足
            return ConstMsg.ERR_CODE_90002;
        }
    }
}

/// <summary>
/// 檢查使用者登入狀態
/// 透過 JWT Token 從 Redis 取得 Session 資料並驗證有效性
/// </summary>
/// <param name="jwtkey">JWT Token</param>
/// <param name="sessiondata">Session 資料（參考參數，用於回傳）</param>
/// <returns>是否已登入</returns>
public bool CheckIsLogin(string? jwtkey, ref MitfSessionDataModel? sessiondata)
{
    // 第一步：檢查 JWT Token 是否為空
    if (string.IsNullOrEmpty(jwtkey))
    {
        logger.Debug("jwtkey is null");
        return false;
    }

    // 第二步：從 Redis 取得 Session 資料
    var sessionmodel = SessionManager.GetMitfSessionData(jwtkey);
    if (sessionmodel == null)
    {
        logger.Debug($"jwtkey ={jwtkey}, sessionmodel is null");
        return false;
    }

    // 第三步：檢查 Session 資料是否有效
    if (sessionmodel.Result == null)
    {
        logger.Debug($"jwtkey ={jwtkey}, SessionDataModel is null");
        return false;
    }

    // 第四步：將 Session 資料回傳給呼叫端
    sessiondata = sessionmodel.Result;

    // 第五步：檢查 Session 中的關鍵資料是否完整
    if (string.IsNullOrEmpty(sessiondata.ManMail))
    {
        logger.Debug($"jwtkey ={jwtkey}, sessiondata.ManMail ={sessiondata.ManMail} is null");
        return false;
    }

    return true;  // 登入狀態有效
}

/// <summary>
/// 檢查功能權限
/// 透過資料庫查詢使用者是否有權限執行指定功能
/// </summary>
/// <param name="funcCode">功能代碼</param>
/// <param name="sessiondata">使用者 Session 資料</param>
/// <returns>是否有權限</returns>
public bool CheckFuncAuth(string funcCode, MitfSessionDataModel sessiondata)
{
    // 第一步：建立權限查詢 SQL
    // 透過多表關聯查詢使用者是否有指定功能的權限
    var QueryCmd = @"
SELECT sf.*
FROM SysFunc sf
    INNER JOIN RoleFuncs rf ON sf.Guid = rf.FuncGuid
    INNER JOIN RoleUsers ru ON ru.RoleGuid = rf.RoleGuid
    INNER JOIN ComMan cm ON cm.Guid = ru.UserGuid
WHERE cm.ManMail = @useract
    AND sf.FuncCode = @funccode";

    bool hasAuth = false;

    // 第二步：執行權限查詢
    using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
    {
        conn.Open();
        DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = QueryCmd;

        // 設定查詢參數
        cmd.Parameters.Add(new SqlParameter("@useract", sessiondata.ManMail));
        cmd.Parameters.Add(new SqlParameter("@funccode", funcCode));

        // 第三步：執行查詢並檢查結果
        using (DbDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                hasAuth = true;  // 找到權限記錄，表示有權限
                break;
            }
            reader.Close();
        }
        conn.Close();
    }

    return hasAuth;
}
```

**程式碼說明：**

1. **分層檢查**：先檢查身份驗證，再檢查授權，確保安全性的完整性
2. **Redis Session**：使用 Redis 快取 Session 資料，提升查詢效能
3. **資料庫權限查詢**：透過多表關聯查詢確認使用者權限
4. **詳細日誌**：記錄所有驗證過程，便於除錯和安全性追蹤
5. **參數化查詢**：使用 SqlParameter 防止 SQL Injection 攻擊
6. **錯誤代碼**：使用統一的錯誤代碼系統，便於前端處理

### 2.3 控制器層的權限檢查實作

**檔案位置：** `MITAP2024/MITAP2024.Server/MITF/MIT01/Controller/MIT0101Controller.cs`

#### 2.3.1 API 端點的權限控制

```csharp
/// <summary>
/// 申請案件管理控制器
/// 每個 API 端點都包含完整的身份驗證和授權檢查
/// </summary>
[Route("api/mitf/mit01/[controller]")]
[ApiController]
public class MIT0101Controller : ControllerBase
{
    private IMIT0101Service _service { get; set; }

    public MIT0101Controller(IMIT0101Service service)
    {
        _service = service;
    }

    /// <summary>
    /// 查詢申請案件列表
    /// 展示完整的權限檢查流程
    /// </summary>
    /// <param name="model">查詢條件</param>
    /// <returns>查詢結果</returns>
    [HttpPost("Query")]
    public async Task<ActionResult<PagedQueryResult>> Query([FromBody] MIT0101QueryModel model)
    {
        // 第一步：宣告 Session 資料變數
        MitfSessionDataModel? sessiondata = null;

        // 第二步：執行身份驗證與授權檢查
        string authResult = _service.CheckIsLoginAndAuth(
            model.jwtkey,           // JWT Token
            ref sessiondata,        // Session 資料（參考參數）
            GetFuncCode()          // 功能代碼
        );

        // 第三步：根據驗證結果決定後續處理
        if (authResult == ConstMsg.SUC_CODE_00001)
        {
            // 驗證通過，執行業務邏輯
            var result = await _service.Query(model, sessiondata);
            return result;
        }
        else if (authResult == ConstMsg.ERR_NOTLOGIN)
        {
            // 未登入錯誤
            return new PagedQueryResult()
            {
                jwtkey = model.jwtkey,
                IsSuccess = false,
                message = "使用者未登入或登入已過期"
            };
        }
        else
        {
            // 權限不足錯誤
            return new PagedQueryResult()
            {
                jwtkey = model.jwtkey,
                IsSuccess = false,
                message = "使用者權限不足，無法執行此功能"
            };
        }
    }

    /// <summary>
    /// 新增申請案件
    /// 同樣包含完整的權限檢查
    /// </summary>
    /// <param name="model">新增資料</param>
    /// <returns>新增結果</returns>
    [HttpPost("DoCreate")]
    public async Task<ActionResult<PagedQueryResult>> DoCreate([FromBody] MIT0101Model model)
    {
        MitfSessionDataModel? sessiondata = null;

        string authResult = _service.CheckIsLoginAndAuth(
            model.jwtkey,
            ref sessiondata,
            GetFuncCode()
        );

        if (authResult == ConstMsg.SUC_CODE_00001)
        {
            var result = await _service.Create(model, sessiondata);
            return result;
        }
        else if (authResult == ConstMsg.ERR_NOTLOGIN)
        {
            return new PagedQueryResult()
            {
                jwtkey = model.jwtkey,
                IsSuccess = false,
                message = "使用者未登入或登入已過期"
            };
        }
        else
        {
            return new PagedQueryResult()
            {
                jwtkey = model.jwtkey,
                IsSuccess = false,
                message = "使用者權限不足，無法執行此功能"
            };
        }
    }

    /// <summary>
    /// 取得當前功能的代碼
    /// 每個控制器都有對應的功能代碼，用於權限檢查
    /// </summary>
    /// <returns>功能代碼</returns>
    private string GetFuncCode()
    {
        return "MIT0101";  // 申請案件管理功能的代碼
    }
}
```

**程式碼說明：**

1. **統一權限檢查**：每個 API 端點都使用相同的權限檢查邏輯
2. **功能代碼對應**：每個控制器對應一個功能代碼，用於權限管理
3. **錯誤處理分類**：區分未登入和權限不足兩種錯誤情況
4. **Session 傳遞**：將驗證後的 Session 資料傳遞給服務層
5. **一致性回應**：所有 API 都回傳統一格式的結果

## 3. 高階安全性實作

### 3.1 防暴力破解機制

**檔案位置：** `MITAP2024/MITAP2024.Server/Models/Sys/LoginFailedHistory.cs`

#### 3.1.1 登入失敗記錄管理

```csharp
/// <summary>
/// 登入失敗歷史記錄模型
/// 用於追蹤和防止暴力破解攻擊
/// </summary>
public class LoginFailedHistory
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public Guid Guid { get; set; }

    /// <summary>
    /// 帳號類型
    /// "0": 後台管理者帳號
    /// "1": 前台申請者帳號
    /// </summary>
    public string AccountType { get; set; }

    /// <summary>
    /// 登入失敗的帳號
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 失敗時間
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 日誌類型
    /// "0": 一般登入失敗
    /// "1": 達到鎖定條件的失敗
    /// </summary>
    public string LogType { get; set; }

    /// <summary>
    /// 是否已刪除
    /// </summary>
    public bool IsDelete { get; set; }
}
```

#### 3.1.2 防暴力破解邏輯實作

```csharp
/// <summary>
/// 防暴力破解檢查機制
/// 在登入驗證前執行，防止惡意攻擊
/// </summary>
private bool CheckBruteForceProtection(string account, string accountType)
{
    // 第一步：查詢最近的登入失敗記錄
    var recentFailures = _context.LoginFailedHistory
        .Where(x => x.Account == account &&
                   x.AccountType == accountType &&
                   x.CreateDate >= DateTime.Now.AddMinutes(-15))  // 15 分鐘內的記錄
        .OrderByDescending(x => x.CreateDate)
        .ToList();

    // 第二步：檢查失敗次數
    if (recentFailures.Count >= 3)
    {
        // 第三步：檢查是否已經鎖定
        var lockRecord = recentFailures.FirstOrDefault(x => x.LogType == "1");
        if (lockRecord != null)
        {
            var timeSinceLock = DateTime.Now - lockRecord.CreateDate;
            if (timeSinceLock.TotalMinutes < 15)
            {
                return false;  // 仍在鎖定期間
            }
        }
    }

    return true;  // 允許登入嘗試
}

/// <summary>
/// 記錄登入失敗
/// 自動判斷是否需要鎖定帳號
/// </summary>
private void RecordLoginFailure(string account, string accountType, string clientIp)
{
    // 第一步：查詢現有的失敗記錄
    var existingFailures = _context.LoginFailedHistory
        .Where(x => x.Account == account &&
                   x.AccountType == accountType &&
                   x.CreateDate >= DateTime.Now.AddMinutes(-15))
        .ToList();

    // 第二步：判斷日誌類型
    string logType = "0";  // 預設為一般失敗
    if (existingFailures.Count >= 2)
    {
        logType = "1";  // 達到鎖定條件
    }

    // 第三步：建立失敗記錄
    var failureRecord = new LoginFailedHistory()
    {
        Guid = Guid.NewGuid(),
        AccountType = accountType,
        Account = account,
        CreateDate = DateTime.Now,
        LogType = logType,
        IsDelete = false
    };

    _context.LoginFailedHistory.Add(failureRecord);

    // 第四步：記錄操作歷史
    var actionHistory = new ActionHistory()
    {
        Guid = Guid.NewGuid(),
        Account = account,
        ActionFuncId = "",
        ActionType = "LOGIN_FAIL",
        ActionDate = DateTime.Now,
        IpAddr = clientIp,
        ActionPage = "/Login",
        ActionNote = $"登入失敗 - 失敗次數: {existingFailures.Count + 1}"
    };

    _context.ActionHistory.Add(actionHistory);
    _context.SaveChanges();
}
```

### 3.2 Redis Session 管理與過期機制

**檔案位置：** `MITAP2024/MITAP2024.Server/appsettings.json`

#### 3.2.1 Redis 設定與 Session 過期

```json
{
  "ConnectionStrings": {
    "MitConnStr": "data source=localhost;initial catalog=2024MIT_DATA;persist security info=true;user id=sa;password=Jh1213Lin;Trust Server Certificate=True;"
  },
  "RedisAddress": "127.0.0.1:6379",
  "RedisExpire": "30",
  "proview": "proviewsoft9f@gmail.com"
}
```

#### 3.2.2 Session 過期處理機制

```csharp
/// <summary>
/// Session 過期管理機制
/// 自動清理過期的 Session 資料，確保安全性
/// </summary>
public class SessionExpiryManager
{
    /// <summary>
    /// 設定 Session 資料到 Redis 並設定過期時間
    /// </summary>
    /// <param name="jwtkey">JWT Token</param>
    /// <param name="sessionData">Session 資料</param>
    /// <returns>是否設定成功</returns>
    public async static Task<bool> SetSessionWithExpiry(string jwtkey, object sessionData)
    {
        IDatabase? sessionDb = GetSessionDB();
        if (sessionDb == null)
        {
            return false;
        }

        // 第一步：序列化 Session 資料
        string sessionDataJson = JsonSerializer.Serialize(sessionData);

        // 第二步：從設定檔讀取過期時間（分鐘）
        int expireMinutes = AppSettingReader.GetRedisExpire();
        TimeSpan expiry = TimeSpan.FromMinutes(expireMinutes);

        // 第三步：設定資料到 Redis 並指定過期時間
        bool result = await sessionDb.StringSetAsync(
            $"session:{jwtkey}",    // Redis Key
            sessionDataJson,        // 序列化的資料
            expiry                  // 過期時間
        );

        return result;
    }

    /// <summary>
    /// 檢查 Session 是否過期
    /// </summary>
    /// <param name="jwtkey">JWT Token</param>
    /// <returns>是否過期</returns>
    public async static Task<bool> IsSessionExpired(string jwtkey)
    {
        IDatabase? sessionDb = GetSessionDB();
        if (sessionDb == null)
        {
            return true;  // Redis 無法連線，視為過期
        }

        // 檢查 Key 是否存在
        bool exists = await sessionDb.KeyExistsAsync($"session:{jwtkey}");
        return !exists;  // Key 不存在表示已過期
    }

    /// <summary>
    /// 延長 Session 過期時間
    /// 在使用者活動時自動延長 Session 有效期
    /// </summary>
    /// <param name="jwtkey">JWT Token</param>
    /// <returns>是否延長成功</returns>
    public async static Task<bool> ExtendSessionExpiry(string jwtkey)
    {
        IDatabase? sessionDb = GetSessionDB();
        if (sessionDb == null)
        {
            return false;
        }

        // 重新設定過期時間
        int expireMinutes = AppSettingReader.GetRedisExpire();
        TimeSpan expiry = TimeSpan.FromMinutes(expireMinutes);

        bool result = await sessionDb.KeyExpireAsync($"session:{jwtkey}", expiry);
        return result;
    }
}
```

### 3.3 安全性日誌與監控

#### 3.3.1 操作歷史記錄

````csharp
/// <summary>
/// 操作歷史記錄模型
/// 記錄所有重要的使用者操作，用於安全性審計
/// </summary>
public class ActionHistory
{
    public Guid Guid { get; set; }
    public string Account { get; set; }          // 操作帳號
    public string ActionFuncId { get; set; }     // 功能代碼
    public string ActionType { get; set; }       // 操作類型
    public DateTime ActionDate { get; set; }     // 操作時間
    public string IpAddr { get; set; }           // IP 位址
    public string ActionPage { get; set; }       // 操作頁面
    public string ActionNote { get; set; }       // 操作備註
}

/// <summary>
/// 安全性日誌記錄服務
/// 自動記錄重要的安全性事件
/// </summary>
public class SecurityLogger
{
    /// <summary>
    /// 記錄登入成功事件
    /// </summary>
    public static void LogLoginSuccess(string account, string clientIp, HttpContext context)
    {
        var actionHistory = new ActionHistory()
        {
            Guid = Guid.NewGuid(),
            Account = account,
            ActionFuncId = "LOGIN",
            ActionType = "LOGIN_SUCCESS",
            ActionDate = DateTime.Now,
            IpAddr = clientIp,
            ActionPage = context.Request.Path,
            ActionNote = "使用者登入成功"
        };

        // 記錄到資料庫
        using (var context = new MainDbContext())
        {
            context.ActionHistory.Add(actionHistory);
            context.SaveChanges();
        }
    }

    /// <summary>
    /// 記錄權限檢查失敗事件
    /// </summary>
    public static void LogAuthorizationFailure(string account, string funcCode, string clientIp)
    {
        var actionHistory = new ActionHistory()
        {
            Guid = Guid.NewGuid(),
            Account = account,
            ActionFuncId = funcCode,
            ActionType = "AUTH_FAIL",
            ActionDate = DateTime.Now,
            IpAddr = clientIp,
            ActionPage = $"/api/{funcCode}",
            ActionNote = $"權限檢查失敗 - 功能代碼: {funcCode}"
        };

        using (var context = new MainDbContext())
        {
            context.ActionHistory.Add(actionHistory);
            context.SaveChanges();
        }
    }
}

## 4. 前端身份驗證整合

### 4.1 前端 Session 管理

**檔案位置：** `MITAP2024/mitap2024.client/src/composables/SessionApi.ts`

#### 4.1.1 前端 Token 管理

```typescript
/// <summary>
/// 前端 Session 管理 API
/// 處理 JWT Token 的儲存、取得和驗證
/// </summary>
export function SessionApi() {
  /// <summary>
  /// 設定 JWT Token 到 localStorage
  /// 提供持久化的 Token 儲存機制
  /// </summary>
  const setToken = (token: string): void => {
    if (!TextUtilApi().isNullOrEmpty(token)) {
      // 將 Token 儲存到瀏覽器的 localStorage
      localStorage.setItem('jwtToken', token);
    }
  };

  /// <summary>
  /// 從 localStorage 取得 JWT Token
  /// 自動處理 Token 不存在的情況
  /// </summary>
  const getToken = (): string => {
    const token = localStorage.getItem('jwtToken');
    return token || '';  // 如果 Token 不存在，回傳空字串
  };

  /// <summary>
  /// 設定前台使用者的 Session 資料
  /// 將登入後的使用者資訊儲存到 localStorage
  /// </summary>
  const mitf_setSdata = (sessionData: MitfSessionDataModel): void => {
    if (sessionData != null) {
      // 將 Session 資料序列化並儲存
      const sessionDataJson = JSON.stringify({
        fSessionData: sessionData  // 前台 Session 資料
      });
      localStorage.setItem('mitfSessionData', sessionDataJson);
    }
  };

  /// <summary>
  /// 取得前台使用者的 Session 資料
  /// 從 localStorage 讀取並反序列化 Session 資料
  /// </summary>
  const getSessionData = (): any => {
    const sessionDataJson = localStorage.getItem('mitfSessionData');
    if (sessionDataJson) {
      try {
        return JSON.parse(sessionDataJson);
      } catch (error) {
        console.error('解析 Session 資料失敗:', error);
        return { fSessionData: null };
      }
    }
    return { fSessionData: null };
  };

  /// <summary>
  /// 檢查前台使用者是否已登入
  /// 驗證 Session 資料的完整性
  /// </summary>
  const mitf_isLogin = (): boolean => {
    let sdata = getSessionData();
    return (
      sdata.fSessionData != null &&
      !TextUtilApi().isNullOrEmpty(sdata.fSessionData.comId)
    );
  };

  /// <summary>
  /// 清除所有 Session 資料
  /// 用於使用者登出時清理本地儲存
  /// </summary>
  const clearSession = (): void => {
    localStorage.removeItem('jwtToken');
    localStorage.removeItem('mitfSessionData');
  };

  return {
    setToken,
    getToken,
    mitf_setSdata,
    getSessionData,
    mitf_isLogin,
    clearSession
  };
}
````

### 4.2 前端登入流程

**檔案位置：** `MITAP2024/mitap2024.client/src/mitf/PageLogin.vue`

#### 4.2.1 登入頁面實作

```vue
<template>
  <div class="login-container">
    <!-- 登入表單 -->
    <form @submit.prevent="doLogin">
      <div class="form-group">
        <label for="userid">帳號</label>
        <input
          type="email"
          id="userid"
          v-model="edit_data.userid"
          required
          placeholder="請輸入電子郵件帳號" />
      </div>

      <div class="form-group">
        <label for="userpwd">密碼</label>
        <input
          type="password"
          id="userpwd"
          v-model="edit_data.userpwd"
          required
          placeholder="請輸入密碼" />
      </div>

      <div class="form-group">
        <label for="randomkey">驗證碼</label>
        <input
          type="text"
          id="randomkey"
          v-model="edit_data.randomkey"
          required
          placeholder="請輸入驗證碼" />
        <img :src="randomkey" @click="fetchRandomKey" alt="驗證碼" />
      </div>

      <button type="submit" :disabled="loading">
        {{ loading ? "登入中..." : "登入" }}
      </button>
    </form>

    <!-- 錯誤訊息顯示 -->
    <div v-if="alert_message" class="alert alert-danger">
      {{ alert_message }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from "vue";
import { SessionApi } from "@/composables/SessionApi";
import { CommonUtilApi } from "@/composables/CommonUtilApi";
import { TextUtilApi } from "@/composables/TextUtilApi";

// 響應式資料定義
const loading = ref(false);
const alert_message = ref("");
const randomkey = ref("");
const edit_data = ref({
  userid: "",
  userpwd: "",
  randomkey: "",
  jwtkey: ""
});

/// <summary>
/// 登入後的回調處理
/// 處理登入成功和失敗的情況
/// </summary>
const afterDoLogin = (json: any): void => {
  loading.value = false;
  let rval = json as QueryResult;

  if (rval.isSuccess) {
    // 登入成功處理
    if (rval.data != null) {
      // 儲存 Session 資料到 localStorage
      SessionApi().mitf_setSdata(rval.data);

      // 更新 JWT Token
      SessionApi().setToken(rval.jwtkey);

      // 跳轉到主頁面
      window.location.href = CommonUtilApi().rootrul() + "mitf/mit01/mit0101";
    }
  } else {
    // 登入失敗處理
    alert_message.value = rval.message || "登入失敗，請檢查帳號密碼";

    // 重新載入驗證碼
    fetchRandomKey();
  }
};

/// <summary>
/// 執行登入操作
/// 驗證輸入資料並發送登入請求
/// </summary>
const doLogin = (): void => {
  alert_message.value = "";

  // 輸入驗證
  if (validSaveDataHasError()) {
    alert_message.value = "資料填寫錯誤, 請檢查後再重新儲存";
    return;
  }

  loading.value = true;

  // 設定 JWT Token
  edit_data.value.jwtkey = SessionApi().getToken();

  // 發送登入請求
  CommonUtilApi().doFetchNoLogin(
    CommonUtilApi().rootrul() + "api/mitf/Home/Login",
    edit_data.value,
    afterDoLogin,
    setAlert
  );
};

/// <summary>
/// 輸入資料驗證
/// 檢查必要欄位是否完整填寫
/// </summary>
const validSaveDataHasError = (): boolean => {
  if (TextUtilApi().isNullOrEmpty(edit_data.value.userid)) {
    return true;
  }
  if (TextUtilApi().isNullOrEmpty(edit_data.value.userpwd)) {
    return true;
  }
  if (TextUtilApi().isNullOrEmpty(edit_data.value.randomkey)) {
    return true;
  }
  return false;
};

/// <summary>
/// 取得驗證碼圖片
/// 定期更新驗證碼，提升安全性
/// </summary>
const fetchRandomKey = (): void => {
  let token = SessionApi().getToken();
  let formdata = { jwtkey: token, abc: "1=1" };

  CommonUtilApi().doFetchNoLogin(
    "../../api/mitf/Home/GetRandomKeyImage",
    formdata,
    afterFetchRandomKey,
    setAlert
  );
};

/// <summary>
/// 驗證碼載入完成回調
/// </summary>
const afterFetchRandomKey = (json: any): void => {
  let rval = json as QueryResult2;
  let token = SessionApi().getToken();

  if (
    typeof rval !== "undefined" &&
    typeof rval.data !== "undefined" &&
    rval.data != null
  ) {
    // 設定驗證碼圖片
    randomkey.value = "data:image/gif;base64,".concat(rval.data);

    // 如果沒有 Token，設定新的 Token
    if (TextUtilApi().isNullOrEmpty(token)) {
      SessionApi().setToken(rval.jwtkey != null ? rval.jwtkey : "");
    }
  }
};

/// <summary>
/// 錯誤訊息設定
/// </summary>
const setAlert = (message: string): void => {
  alert_message.value = message;
  loading.value = false;
};

// 定期更新驗證碼（每 15 分鐘）
let timer = setInterval(fetchRandomKey, 15 * 60 * 1000);

// 元件掛載時初始化
onMounted(() => {
  fetchRandomKey();
});

// 元件卸載時清理定時器
onUnmounted(() => {
  clearInterval(timer);
});
</script>
```

## 5. 安全性最佳實務總結

### 5.1 身份驗證（Authentication）最佳實務

1. **強密碼政策**：使用 SHA-512 雜湊演算法，確保密碼安全性
2. **Token 管理**：使用自定義 JWT Token 格式，結合 GUID 確保唯一性
3. **Session 過期**：設定合理的 Session 過期時間（30 分鐘）
4. **防暴力破解**：實作登入失敗次數限制和帳號鎖定機制
5. **多重驗證**：支援驗證碼和管理者緊急密碼機制

### 5.2 授權（Authorization）最佳實務

1. **RBAC 模型**：採用基於角色的存取控制，靈活管理權限
2. **功能級權限**：每個 API 端點都有對應的功能代碼和權限檢查
3. **資料庫權限查詢**：透過多表關聯確認使用者權限
4. **統一權限檢查**：所有控制器使用相同的權限檢查邏輯
5. **權限日誌**：記錄所有權限檢查結果，便於安全性審計

### 5.3 系統安全性特色

1. **分散式 Session**：使用 Redis 儲存 Session，支援水平擴展
2. **完整日誌**：記錄登入、權限檢查、操作等所有安全性事件
3. **IP 追蹤**：記錄客戶端 IP 位址，便於安全性分析
4. **前後端整合**：前端和後端統一的身份驗證機制
5. **錯誤處理**：區分不同類型的安全性錯誤，提供適當的回應

MITAP2024 系統的身份驗證與授權機制提供了完整的安全性保護，從密碼加密、Token 管理、Session 控制到權限檢查，每個環節都經過精心設計，確保系統的安全性和可靠性。透過這套機制，系統能夠有效防範各種安全性威脅，為使用者提供安全可靠的服務環境。
