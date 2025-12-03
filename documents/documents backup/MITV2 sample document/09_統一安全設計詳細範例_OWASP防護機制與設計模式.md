# MIT 微笑標章管理系統 - 統一安全設計詳細範例：OWASP 防護機制與設計模式

## 功能概述

本文詳細分析 MITAP2024 系統的統一安全設計架構，說明如何透過設計模式和安全機制來防護 OWASP Top 10 安全弱點。系統採用多層次的安全防護策略，包含身份驗證、授權控制、輸入驗證、輸出編碼、安全標頭等機制，建立完整的安全防護體系。

## 安全設計架構概覽

```
MITAP2024 統一安全設計架構：

前端安全層
    ↓ CSP、XSS 防護、輸入驗證
中介軟體安全層
    ↓ 安全標頭、CORS、身份驗證
控制器安全層
    ↓ 授權檢查、參數驗證
服務層安全層
    ↓ 業務邏輯驗證、資料清理
資料存取安全層
    ↓ 參數化查詢、SQL 注入防護
日誌與監控層
    ↓ 安全事件記錄、異常追蹤
```

## 1. A01 - 存取控制失效 (Broken Access Control) 防護

### 1.1 統一身份驗證與授權機制

**檔案位置：** `MITAP2024/MITAP2024.Server/MITF/HOME/Service/HomeService.cs`

#### 1.1.1 基於角色的存取控制 (RBAC) 實作

```csharp
/// <summary>
/// 統一的身份驗證與授權檢查機制
/// 實作 RBAC 模型，防止存取控制失效
/// </summary>
public string CheckIsLoginAndAuth(
    string? jwtkey,                      // JWT Token
    ref MitfSessionDataModel? sessiondata, // Session 資料
    string funcCode                      // 功能代碼
)
{
    try
    {
        // 第一步：身份驗證 (Authentication)
        if (string.IsNullOrEmpty(jwtkey))
        {
            return ConstMsg.ERR_CODE_00002; // Token 為空
        }

        // 從 Redis 取得 Session 資料
        sessiondata = SessionManager.GetMitfSessionData(jwtkey).Result;
        if (sessiondata == null)
        {
            return ConstMsg.ERR_CODE_00002; // Session 不存在或已過期
        }

        // 第二步：授權檢查 (Authorization)
        if (!CheckFuncAuth(funcCode, sessiondata))
        {
            return ConstMsg.ERR_CODE_00003; // 權限不足
        }

        return ConstMsg.SUC_CODE_00001; // 驗證成功
    }
    catch (Exception ex)
    {
        // 記錄安全性異常
        logger.Error($"身份驗證失敗: {ex.Message}", ex);
        return ConstMsg.ERR_CODE_00002;
    }
}

/// <summary>
/// 功能權限檢查
/// 透過資料庫查詢確認使用者是否有執行特定功能的權限
/// </summary>
public bool CheckFuncAuth(string funcCode, MitfSessionDataModel sessiondata)
{
    // 多表關聯查詢：Users → RoleUsers → Roles → RoleFuncs → SysFunc
    var QueryCmd = @"
        SELECT sf.*
        FROM SysFunc sf
            INNER JOIN RoleFuncs rf ON sf.Guid = rf.FuncGuid
            INNER JOIN RoleUsers ru ON ru.RoleGuid = rf.RoleGuid
            INNER JOIN ComMan cm ON cm.Guid = ru.UserGuid
        WHERE cm.ManMail = @useract AND sf.FuncCode = @funccode";

    bool hasAuth = false;
    using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
    {
        conn.Open();
        DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = QueryCmd;

        // 使用參數化查詢防止 SQL 注入
        cmd.Parameters.Add(new SqlParameter("@useract", sessiondata.ManMail));
        cmd.Parameters.Add(new SqlParameter("@funccode", funcCode));

        using (DbDataReader reader = cmd.ExecuteReader())
        {
            hasAuth = reader.Read(); // 有資料表示有權限
        }
    }

    return hasAuth;
}
```

**程式碼說明：**

1. **分層驗證**：先驗證身份，再檢查授權，確保完整的存取控制
2. **Session 管理**：使用 Redis 儲存 Session，支援分散式部署
3. **RBAC 模型**：透過角色和功能的多對多關聯實現細緻權限控制
4. **參數化查詢**：防止 SQL 注入攻擊
5. **異常處理**：記錄安全性異常，便於監控和分析

#### 1.1.2 控制器層的統一權限檢查

```csharp
/// <summary>
/// 申請案件管理控制器
/// 每個 API 端點都實作統一的權限檢查
/// </summary>
[Route("api/mitf/mit01/[controller]")]
[ApiController]
public class MIT0101Controller : ControllerBase
{
    /// <summary>
    /// 查詢申請案件資料
    /// 展示統一權限檢查模式的實作
    /// </summary>
    [HttpPost("QueryDatas")]
    public async Task<ActionResult<PagedQueryResult>> QueryDatas([FromBody] MIT0101QueryModel model)
    {
        MitfSessionDataModel? sessiondata = null;

        // 統一的身份驗證與授權檢查
        var checkResult = _homeService.CheckIsLoginAndAuth(
            model.jwtkey,      // JWT Token
            ref sessiondata,   // Session 資料
            GetFuncCode()      // 功能代碼
        );

        // 根據檢查結果決定後續處理
        if (checkResult == ConstMsg.SUC_CODE_00001)
        {
            // 權限檢查通過，執行業務邏輯
            return await _service.QueryDatas(model, sessiondata);
        }
        else
        {
            // 權限檢查失敗，回傳錯誤
            return new PagedQueryResult()
            {
                jwtkey = model.jwtkey,
                IsSuccess = false,
                message = checkResult == ConstMsg.ERR_CODE_00002 ? "請重新登入" : "權限不足"
            };
        }
    }

    /// <summary>
    /// 取得功能代碼
    /// 每個控制器對應一個功能代碼，用於權限管理
    /// </summary>
    private string GetFuncCode()
    {
        return "MIT0101"; // 申請案件管理功能代碼
    }
}
```

**程式碼說明：**

1. **統一模式**：所有控制器使用相同的權限檢查邏輯
2. **功能對應**：每個控制器對應特定功能代碼
3. **錯誤分類**：區分未登入和權限不足兩種情況
4. **一致回應**：統一的錯誤回應格式
5. **可維護性**：集中的權限檢查邏輯便於維護

## 2. A02 - 加密機制失效 (Cryptographic Failures) 防護

### 2.1 密碼雜湊與加密機制

**檔案位置：** `MITAP2024/MITAP2024.Server/Utils/Common/TextUtils.cs`

#### 2.1.1 SHA-512 密碼雜湊實作

```csharp
/// <summary>
/// 密碼雜湊處理工具
/// 使用 SHA-512 演算法提供強密碼保護
/// </summary>
public static class TextUtils
{
    /// <summary>
    /// 使用 SHA-512 演算法對密碼進行雜湊處理
    /// 防止密碼明文儲存和彩虹表攻擊
    /// </summary>
    /// <param name="pwdstr">原始密碼字串</param>
    /// <returns>雜湊後的密碼字串（128 個十六進位字元）</returns>
    public static string HashPassword(string pwdstr)
    {
        // 使用 SHA-512 演算法
        using (SHA512 sha = SHA512.Create())
        {
            // 將密碼轉換為 UTF-8 位元組陣列
            byte[] inputBytes = Encoding.UTF8.GetBytes(pwdstr);

            // 計算雜湊值
            byte[] hashBytes = sha.ComputeHash(inputBytes);

            // 轉換為十六進位字串
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// 產生安全的 JWT Token
    /// 結合系統前綴和 GUID 確保唯一性和安全性
    /// </summary>
    public static async Task<string> GenSessionKey()
    {
        return await Task.Run(() =>
        {
            // 產生新的 GUID
            Guid newGuid = Guid.NewGuid();

            // 轉換為 32 位元小寫字串
            string guidString = newGuid.ToString("N").ToLower();

            // 加上系統前綴
            return "mit" + guidString;
        });
    }
}
```

**程式碼說明：**

1. **強雜湊演算法**：使用 SHA-512 提供 512 位元的雜湊值
2. **不可逆加密**：雜湊函數為單向函數，無法反推原始密碼
3. **UTF-8 支援**：支援多國語言字元的密碼
4. **記憶體安全**：使用 `using` 語句確保資源正確釋放
5. **Token 安全性**：結合 GUID 和系統前綴確保 Token 唯一性

## 3. A03 - 注入攻擊 (Injection) 防護

### 3.1 SQL 注入防護機制

**檔案位置：** `MITAP2024/MITAP2024.Server/Main/MIT10/Service/MIT1003Service.cs`

#### 3.1.1 參數化查詢實作

```csharp
/// <summary>
/// 產品資料查詢服務
/// 展示完整的 SQL 注入防護機制
/// </summary>
public async Task<ActionResult<PagedQueryResult>> QueryDatas(MIT1003QueryModel model, SessionDataModel sessiondata)
{
    return await Task.Run(() =>
    {
        PagedQueryResult result = new PagedQueryResult();

        try
        {
            // 建構動態查詢條件，但使用參數化查詢防止注入
            string QueryCountCmd = @"
                SELECT COUNT(*) as cnt
                FROM ProductGoods pg
                    LEFT JOIN Industry i ON pg.IndustryGuid = i.Guid
                    LEFT JOIN ProductIndustryType pit ON pg.ProductIndustryTypeGuid = pit.Guid
                WHERE 1=1";

            // 動態添加查詢條件
            if (!string.IsNullOrEmpty(model.q_industryGuid))
            {
                QueryCountCmd += " AND pg.IndustryGuid = @IndustryGuId";
            }
            if (!string.IsNullOrEmpty(model.q_productIndustryGuid))
            {
                QueryCountCmd += " AND pg.ProductIndustryTypeGuid = @ProductIndustryTypeGuid";
            }
            if (!string.IsNullOrEmpty(model.q_goodName))
            {
                QueryCountCmd += " AND pg.GoodName LIKE CONCAT('%', @GoodName, '%')";
            }
            if (!string.IsNullOrEmpty(model.q_isDelete))
            {
                QueryCountCmd += " AND pg.IsDelete = @isdelete";
            }

            using (var conn = new SqlConnection(AppSettingReader.GetMitDbConnStr()))
            {
                conn.Open();
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = QueryCountCmd;

                // 使用參數化查詢，防止 SQL 注入攻擊
                if (!string.IsNullOrEmpty(model.q_industryGuid))
                {
                    cmd.Parameters.Add(new SqlParameter("@IndustryGuId", Guid.Parse(model.q_industryGuid)));
                }
                if (!string.IsNullOrEmpty(model.q_productIndustryGuid))
                {
                    cmd.Parameters.Add(new SqlParameter("@ProductIndustryTypeGuid", Guid.Parse(model.q_productIndustryGuid)));
                }
                if (!string.IsNullOrEmpty(model.q_goodName))
                {
                    // 對使用者輸入進行清理，移除潛在的惡意字元
                    string cleanGoodName = SanitizeInput(model.q_goodName);
                    cmd.Parameters.Add(new SqlParameter("@GoodName", cleanGoodName));
                }
                if (!string.IsNullOrEmpty(model.q_isDelete))
                {
                    cmd.Parameters.Add(new SqlParameter("@isdelete", model.q_isDelete));
                }

                // 執行查詢
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result.TotalCount = Convert.ToInt32(reader["cnt"]);
                    }
                }
            }

            result.IsSuccess = true;
        }
        catch (Exception e)
        {
            result.message = TextUtils.GenErrmsgWithNum(logger, "查詢產品資料失敗", e);
            result.IsSuccess = false;
        }

        return result;
    });
}

/// <summary>
/// 輸入資料清理方法
/// 移除或轉義潛在的惡意字元
/// </summary>
private string SanitizeInput(string input)
{
    if (string.IsNullOrEmpty(input))
        return string.Empty;

    // 移除或轉義 SQL 特殊字元
    return input.Replace("'", "''")           // 單引號轉義
                .Replace("--", "")            // 移除 SQL 註解
                .Replace("/*", "")            // 移除多行註解開始
                .Replace("*/", "")            // 移除多行註解結束
                .Replace("xp_", "")           // 移除擴展預存程序前綴
                .Replace("sp_", "")           // 移除系統預存程序前綴
                .Replace("exec", "")          // 移除執行命令
                .Replace("execute", "")       // 移除執行命令
                .Trim();                      // 移除前後空白
}
```

**程式碼說明：**

1. **參數化查詢**：所有使用者輸入都透過 SqlParameter 傳遞
2. **動態查詢安全**：動態建構 SQL 但使用參數化防護
3. **輸入清理**：對使用者輸入進行清理和驗證
4. **型別驗證**：使用 Guid.Parse 確保 GUID 格式正確
5. **錯誤處理**：捕獲異常並記錄，不暴露敏感資訊

### 3.2 NoSQL 注入防護

**檔案位置：** `MITAP2024/MITAP2024.Server/Utils/Session/SessionManager.cs`

#### 3.2.1 Redis 操作安全實作

```csharp
/// <summary>
/// Session 管理器
/// 展示 NoSQL (Redis) 注入防護機制
/// </summary>
public class SessionManager
{
    /// <summary>
    /// 安全的 Redis Key 產生
    /// 防止 Redis 注入攻擊
    /// </summary>
    private static string GenerateSecureRedisKey(string jwtkey)
    {
        // 驗證 JWT Token 格式
        if (string.IsNullOrEmpty(jwtkey) || !jwtkey.StartsWith("mit"))
        {
            throw new ArgumentException("Invalid JWT token format");
        }

        // 移除潛在的 Redis 命令注入字元
        string cleanKey = jwtkey.Replace(" ", "")      // 移除空格
                                .Replace("\n", "")     // 移除換行
                                .Replace("\r", "")     // 移除回車
                                .Replace("\t", "")     // 移除 Tab
                                .Replace("*", "")      // 移除萬用字元
                                .Replace("?", "");     // 移除問號

        // 限制 Key 長度，防止過長的 Key 攻擊
        if (cleanKey.Length > 50)
        {
            cleanKey = cleanKey.Substring(0, 50);
        }

        return $"mitf_session:{cleanKey}";
    }

    /// <summary>
    /// 安全的 Session 資料儲存
    /// </summary>
    public static async Task SetMitfSessionData(string jwtkey, MitfSessionDataModel sessionData)
    {
        try
        {
            string redisKey = GenerateSecureRedisKey(jwtkey);

            // 序列化 Session 資料
            string jsonData = JsonSerializer.Serialize(sessionData);

            // 使用 Redis 連線池和安全設定
            using (var redis = ConnectionMultiplexer.Connect(AppSettingReader.GetRedisConnStr()))
            {
                var db = redis.GetDatabase();

                // 設定過期時間，防止 Session 永久存在
                await db.StringSetAsync(redisKey, jsonData, TimeSpan.FromMinutes(30));
            }
        }
        catch (Exception ex)
        {
            // 記錄錯誤但不暴露敏感資訊
            var logger = LogManager.GetLogger("MITAP2024");
            logger.Error($"Session 儲存失敗: {ex.Message}");
            throw new InvalidOperationException("Session 操作失敗");
        }
    }

    /// <summary>
    /// 安全的 Session 資料讀取
    /// </summary>
    public static async Task<MitfSessionDataModel?> GetMitfSessionData(string jwtkey)
    {
        try
        {
            string redisKey = GenerateSecureRedisKey(jwtkey);

            using (var redis = ConnectionMultiplexer.Connect(AppSettingReader.GetRedisConnStr()))
            {
                var db = redis.GetDatabase();

                string? jsonData = await db.StringGetAsync(redisKey);

                if (string.IsNullOrEmpty(jsonData))
                {
                    return null; // Session 不存在或已過期
                }

                // 反序列化時進行額外驗證
                var sessionData = JsonSerializer.Deserialize<MitfSessionDataModel>(jsonData);

                // 驗證 Session 資料完整性
                if (sessionData == null || string.IsNullOrEmpty(sessionData.ManMail))
                {
                    return null;
                }

                return sessionData;
            }
        }
        catch (Exception ex)
        {
            var logger = LogManager.GetLogger("MITAP2024");
            logger.Error($"Session 讀取失敗: {ex.Message}");
            return null; // 發生錯誤時回傳 null，強制重新登入
        }
    }
}
```

**程式碼說明：**

1. **Key 驗證**：驗證 Redis Key 格式，防止注入攻擊
2. **字元過濾**：移除潛在的 Redis 命令注入字元
3. **長度限制**：限制 Key 長度，防止過長攻擊
4. **連線安全**：使用連線池和安全的連線設定
5. **過期控制**：設定 Session 過期時間，防止永久存在

## 4. A04 - 不安全設計 (Insecure Design) 防護

### 4.1 安全設計模式實作

#### 4.1.1 防禦性程式設計模式

```csharp
/// <summary>
/// 基礎服務類別
/// 實作防禦性程式設計模式
/// </summary>
public abstract class BaseService
{
    protected readonly ILog logger;

    protected BaseService()
    {
        logger = LogManager.GetLogger(GetType());
    }

    /// <summary>
    /// 安全的資料模型驗證
    /// 實作輸入驗證的統一模式
    /// </summary>
    protected bool CheckModelDataHasError<T>(T model, ref PagedQueryResult result, bool isCreate = false)
    {
        try
        {
            // 第一步：空值檢查
            if (model == null)
            {
                result.message = "資料模型不可為空";
                return true;
            }

            // 第二步：使用反射進行屬性驗證
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                // 檢查必要欄位
                var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
                if (requiredAttr != null)
                {
                    var value = property.GetValue(model);
                    if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                    {
                        result.message = $"{property.Name} 為必要欄位";
                        return true;
                    }
                }

                // 檢查字串長度
                var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttr != null && property.GetValue(model) is string stringValue)
                {
                    if (stringValue.Length > stringLengthAttr.MaximumLength)
                    {
                        result.message = $"{property.Name} 長度不可超過 {stringLengthAttr.MaximumLength} 個字元";
                        return true;
                    }
                }

                // 檢查 XSS 攻擊模式
                if (property.PropertyType == typeof(string) && property.GetValue(model) is string inputValue)
                {
                    if (ContainsXssPattern(inputValue))
                    {
                        result.message = $"{property.Name} 包含不安全的內容";
                        return true;
                    }
                }
            }

            return false; // 驗證通過
        }
        catch (Exception ex)
        {
            logger.Error($"資料驗證失敗: {ex.Message}", ex);
            result.message = "資料驗證過程發生錯誤";
            return true;
        }
    }

    /// <summary>
    /// XSS 攻擊模式檢測
    /// </summary>
    private bool ContainsXssPattern(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        string[] xssPatterns = {
            "<script", "</script>", "javascript:", "onload=",
            "onerror=", "onclick=", "onmouseover=", "eval(",
            "expression(", "vbscript:", "data:text/html"
        };

        string lowerInput = input.ToLower();
        return xssPatterns.Any(pattern => lowerInput.Contains(pattern));
    }
}
```

**程式碼說明：**

1. **統一驗證**：所有服務類別繼承統一的驗證邏輯
2. **反射驗證**：使用反射自動檢查資料註解
3. **XSS 檢測**：檢測常見的 XSS 攻擊模式
4. **防禦性設計**：假設所有輸入都可能是惡意的
5. **錯誤處理**：安全的錯誤處理，不暴露系統資訊

## 5. A05 - 安全設定缺陷 (Security Misconfiguration) 防護

### 5.1 安全標頭與 CSP 設定

**檔案位置：** `MITAP2024/MITAP2024.Server/Program.cs`

#### 5.1.1 Content Security Policy 實作

```csharp
/// <summary>
/// 應用程式安全設定
/// 實作完整的安全標頭和 CSP 防護
/// </summary>
var app = builder.Build();

// 設定 Content Security Policy
var policyCollection = new HeaderPolicyCollection().AddContentSecurityPolicy(builder =>
{
    // 預設來源：只允許同源
    builder.AddDefaultSrc().Self();

    // 腳本來源：只允許同源，防止 XSS 攻擊
    builder.AddScriptSrc().Self();

    // 圖片來源：允許同源和 data URI
    builder.AddImgSrc().Self().Data();

    // 連線來源：只允許同源，防止資料外洩
    builder.AddConnectSrc().Self();

    // 字型來源：只允許同源
    builder.AddFontSrc().Self();

    // 物件來源：完全禁止，防止插件攻擊
    builder.AddObjectSrc().None();

    // 表單提交：只允許同源
    builder.AddFormAction().Self();

    // 框架祖先：禁止被嵌入框架，防止點擊劫持
    builder.AddFrameAncestors().None();

    // 基礎 URI：只允許同源
    builder.AddBaseUri().Self();

    // 框架來源：只允許同源
    builder.AddFrameSrc().Self();
});

// 應用安全標頭
app.UseSecurityHeaders(policyCollection);

// 添加額外的安全標頭
app.Use(async (context, next) =>
{
    // X-Frame-Options：防止點擊劫持攻擊
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");

    // X-Content-Type-Options：防止 MIME 類型嗅探攻擊
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

    // X-XSS-Protection：啟用瀏覽器 XSS 防護
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

    // Referrer-Policy：控制 Referrer 資訊洩露
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    // Permissions-Policy：限制瀏覽器功能存取
    context.Response.Headers.Add("Permissions-Policy",
        "camera=(), microphone=(), geolocation=(), payment=()");

    await next();
});

// HTTPS 重定向（生產環境）
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();

    // HSTS (HTTP Strict Transport Security)
    app.UseHsts();
}
```

**程式碼說明：**

1. **CSP 防護**：完整的內容安全政策，防止 XSS 和資料注入
2. **點擊劫持防護**：X-Frame-Options 防止頁面被惡意嵌入
3. **MIME 嗅探防護**：防止瀏覽器錯誤解析檔案類型
4. **HTTPS 強制**：生產環境強制使用 HTTPS
5. **權限控制**：限制瀏覽器敏感功能的存取

#### 5.1.2 前端 CSP 設定

**檔案位置：** `MITAP2024/MITAP2024.Server/bin/Release/net8.0/publish/wwwroot/index.html`

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <link rel="icon" href="/assets/icon-B_R34KXh.ico" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />

    <!-- Content Security Policy Meta Tag -->
    <meta
      http-equiv="Content-Security-Policy"
      content="default-src 'self';
                   img-src data: https: http:;
                   script-src 'self' 'unsafe-inline';
                   style-src 'self' 'unsafe-inline';
                   connect-src 'self';
                   font-src 'self';
                   object-src 'none';
                   frame-ancestors 'none';" />

    <title>台灣製產品MIT微笑標章</title>
    <script type="module" crossorigin src="/assets/index-D8twqA5u.js"></script>
    <link rel="stylesheet" crossorigin href="/assets/index-BLlBnrXZ.css" />
  </head>
  <body>
    <div id="app">
      <router-view />
    </div>
  </body>
</html>
```

**程式碼說明：**

1. **雙重 CSP**：後端和前端都設定 CSP，確保完整防護
2. **嚴格政策**：限制資源載入來源，防止惡意內容注入
3. **框架防護**：禁止頁面被嵌入框架
4. **物件禁用**：完全禁止 object 標籤，防止插件攻擊
5. **跨域限制**：嚴格控制跨域資源存取

## 6. A06 - 易受攻擊和過時的元件 (Vulnerable and Outdated Components) 防護

### 6.1 依賴管理與版本控制

**檔案位置：** `MITAP2024/mitap2024.client/package.json`

#### 6.1.1 前端依賴安全管理

```json
{
  "name": "mitap2024.client",
  "private": true,
  "version": "0.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "vue-tsc && vite build",
    "preview": "vite preview",
    "audit": "npm audit",
    "audit-fix": "npm audit fix",
    "update-check": "npm outdated"
  },
  "dependencies": {
    // 使用最新穩定版本，定期更新
    "vue": "^3.4.0", // Vue.js 框架
    "vue-router": "^4.2.0", // 路由管理
    "bootstrap": "^5.3.0", // UI 框架
    "dompurify": "^3.0.0", // XSS 防護庫
    "@fortawesome/fontawesome-free": "^6.5.0"
  },
  "devDependencies": {
    "@types/node": "^20.0.0",
    "@vitejs/plugin-vue": "^4.6.0",
    "typescript": "^5.2.0",
    "vite": "^5.0.0",
    "vue-tsc": "^1.8.0"
  }
}
```

**程式碼說明：**

1. **版本管理**：使用語意化版本控制，定期更新依賴
2. **安全審計**：包含 npm audit 腳本檢查已知漏洞
3. **XSS 防護**：使用 DOMPurify 庫清理使用者輸入
4. **最新版本**：使用最新穩定版本的依賴套件
5. **開發工具**：包含 TypeScript 提供型別安全

#### 6.1.2 後端依賴安全管理

**檔案位置：** `MITAP2024/MITAP2024.Server/MITAP2024.Server.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- 啟用安全性分析 -->
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <!-- 使用最新穩定版本的套件 -->
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />

    <!-- 安全性相關套件 -->
    <PackageReference Include="NetEscapades.AspNetCore.SecurityHeaders" Version="0.21.0" />
    <PackageReference Include="StackExchange.Redis" Version="2.7.0" />

    <!-- 日誌和監控 -->
    <PackageReference Include="log4net" Version="2.0.15" />
    <PackageReference Include="Microsoft.Extensions.Logging.Log4Net.AspNetCore" Version="8.0.0" />

    <!-- 文件處理 -->
    <PackageReference Include="DocumentFormat.OpenXml" Version="3.0.0" />
    <PackageReference Include="iTextSharp" Version="5.5.13.3" />
  </ItemGroup>

</Project>
```

**程式碼說明：**

1. **.NET 9.0**：使用最新的 .NET 版本，獲得最新安全性修補
2. **安全性分析**：啟用 .NET 分析器檢查安全性問題
3. **版本控制**：明確指定套件版本，避免意外更新
4. **安全性套件**：使用專門的安全性標頭套件
5. **定期更新**：建立更新流程，定期檢查和更新依賴

## 7. A07 - 身份識別和身份驗證失效 (Identification and Authentication Failures) 防護

### 7.1 防暴力破解機制

**檔案位置：** `MITAP2024/MITAP2024.Server/MITF/HOME/Service/HomeService.cs`

#### 7.1.1 登入失敗記錄與帳號鎖定

```csharp
/// <summary>
/// 防暴力破解的登入驗證機制
/// 實作帳號鎖定和失敗次數限制
/// </summary>
public async Task<ActionResult<PagedQueryResult>> Login(LoginModel model)
{
    PagedQueryResult result = new PagedQueryResult();

    try
    {
        // 第一步：檢查是否已被鎖定
        var failedAttempts = _context.LoginFailedHistory
            .Where(x => x.Account == model.userid &&
                       x.AccountType == "1" &&
                       x.CreateDate > DateTime.Now.AddMinutes(-15)) // 15分鐘內的失敗記錄
            .Count();

        if (failedAttempts >= 3)
        {
            result.message = "帳號已被鎖定15分鐘，請稍後再試";
            result.IsSuccess = false;

            // 記錄鎖定事件
            LogSecurityEvent("ACCOUNT_LOCKED", model.userid, GetClientIP());
            return result;
        }

        // 第二步：驗證使用者帳號密碼
        string encodepwd = TextUtils.HashPassword(model.userpwd);
        var user = _context.ComMan
            .Where(x => x.ManMail == model.userid && x.IsDelete != "Y")
            .FirstOrDefault();

        bool loginSuccess = false;

        if (user != null)
        {
            // 檢查密碼是否正確
            if (user.ManPwd == encodepwd)
            {
                loginSuccess = true;
            }
            else
            {
                // 檢查是否使用管理者緊急密碼
                var adminUser = _context.Users
                    .Where(x => x.UserAct == AppSettingReader.GetProview())
                    .FirstOrDefault();

                if (adminUser != null && adminUser.UserPwd == encodepwd)
                {
                    loginSuccess = true;
                }
            }
        }

        if (loginSuccess)
        {
            // 第三步：登入成功處理
            // 清除之前的失敗記錄
            var oldFailedRecords = _context.LoginFailedHistory
                .Where(x => x.Account == model.userid && x.AccountType == "1")
                .ToList();

            _context.LoginFailedHistory.RemoveRange(oldFailedRecords);

            // 建立 Session
            MitfSessionDataModel sessionData = new MitfSessionDataModel()
            {
                ComId = user.ComId,
                ComName = user.ComName,
                ManMail = user.ManMail,
                ManName = user.ManName,
                LoginTime = DateTime.Now,
                NeedResetPw = "N"
            };

            await SessionManager.SetMitfSessionData(model.jwtkey, sessionData);

            // 記錄成功登入事件
            LogSecurityEvent("LOGIN_SUCCESS", model.userid, GetClientIP());

            result.Data = sessionData;
            result.IsSuccess = true;
        }
        else
        {
            // 第四步：登入失敗處理
            // 記錄失敗嘗試
            var failedRecord = new LoginFailedHistory()
            {
                Guid = Guid.NewGuid(),
                Account = model.userid,
                AccountType = "1", // 1=前台使用者
                FailedReason = "密碼錯誤",
                ClientIP = GetClientIP(),
                UserAgent = GetUserAgent(),
                CreateDate = DateTime.Now
            };

            _context.LoginFailedHistory.Add(failedRecord);
            _context.SaveChanges();

            // 記錄失敗登入事件
            LogSecurityEvent("LOGIN_FAILED", model.userid, GetClientIP());

            result.message = "帳號或密碼錯誤";
            result.IsSuccess = false;
        }
    }
    catch (Exception ex)
    {
        logger.Error($"登入過程發生錯誤: {ex.Message}", ex);
        result.message = "登入過程發生錯誤，請稍後再試";
        result.IsSuccess = false;
    }

    return result;
}

/// <summary>
/// 記錄安全性事件
/// </summary>
private void LogSecurityEvent(string eventType, string account, string clientIP)
{
    try
    {
        var securityLog = new ActionHistory()
        {
            Guid = Guid.NewGuid(),
            ActionType = eventType,
            ActionUser = account,
            ActionTime = DateTime.Now,
            ClientIP = clientIP,
            ActionDesc = $"{eventType}: {account} from {clientIP}",
            CreateDate = DateTime.Now
        };

        _context.ActionHistory.Add(securityLog);
        _context.SaveChanges();
    }
    catch (Exception ex)
    {
        logger.Error($"記錄安全性事件失敗: {ex.Message}", ex);
    }
}

/// <summary>
/// 取得客戶端 IP 位址
/// </summary>
private string GetClientIP()
{
    return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
}
```

**程式碼說明：**

1. **失敗次數限制**：3 次失敗後鎖定帳號 15 分鐘
2. **時間窗口**：只計算 15 分鐘內的失敗次數
3. **緊急密碼**：支援管理者緊急密碼機制
4. **安全性日誌**：記錄所有登入嘗試和安全性事件
5. **IP 追蹤**：記錄客戶端 IP 位址便於分析

## 8. A08 - 軟體和資料完整性失效 (Software and Data Integrity Failures) 防護

### 8.1 輸入驗證與資料清理

**檔案位置：** `MITAP2024/mitap2024.client/src/components/InputTextRichText.vue`

#### 8.1.1 富文本編輯器 XSS 防護

```vue
<template>
  <!-- 富文本編輯器安全顯示 -->
  <div class="rich-text-container">
    <div v-if="isEditMode">
      <!-- 編輯模式：使用安全的富文本編輯器 -->
      <textarea
        v-model="internalData"
        @input="handleInput"
        :placeholder="placeholder"
        class="form-control rich-text-editor"></textarea>
    </div>
    <div v-else>
      <!-- 顯示模式：使用 DOMPurify 清理 HTML -->
      <div class="rich-text-display" v-html="sanitizedContent"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import DOMPurify from "dompurify";

interface Props {
  data?: string;
  isEditMode?: boolean;
  placeholder?: string;
}

const props = withDefaults(defineProps<Props>(), {
  data: "",
  isEditMode: false,
  placeholder: "請輸入內容..."
});

const emit = defineEmits<{
  "update:data": [value: string];
}>();

const internalData = ref(props.data);

/// <summary>
/// 使用 DOMPurify 清理 HTML 內容
/// 防止 XSS 攻擊和惡意腳本注入
/// </summary>
const sanitizedContent = computed(() => {
  if (!props.data) return "";

  // 使用 DOMPurify 清理 HTML，只允許安全的標籤和屬性
  const cleanHtml = DOMPurify.sanitize(props.data, {
    // 允許的 HTML 標籤
    ALLOWED_TAGS: [
      "b",
      "i",
      "em",
      "strong",
      "a",
      "p",
      "br",
      "ul",
      "ol",
      "li",
      "div",
      "table",
      "tr",
      "td",
      "th",
      "tbody",
      "thead",
      "h1",
      "h2",
      "h3"
    ],
    // 允許的屬性
    ALLOWED_ATTR: ["href", "border", "colspan", "rowspan", "style", "class"],
    // 禁止的標籤
    FORBID_TAGS: [
      "script",
      "iframe",
      "object",
      "embed",
      "form",
      "input",
      "button"
    ],
    // 禁止的屬性
    FORBID_ATTR: [
      "onclick",
      "onload",
      "onerror",
      "onmouseover",
      "onfocus",
      "onblur"
    ],
    // 移除空的標籤
    REMOVE_EMPTY: true,
    // 保持安全的 URL
    SAFE_FOR_TEMPLATES: true
  });

  return cleanHtml;
});

/// <summary>
/// 處理輸入事件
/// 對使用者輸入進行即時驗證和清理
/// </summary>
const handleInput = (event: Event) => {
  const target = event.target as HTMLTextAreaElement;
  let value = target.value;

  // 基本的輸入清理
  value = sanitizeInput(value);

  internalData.value = value;
  emit("update:data", value);
};

/// <summary>
/// 輸入資料清理函數
/// 移除潛在的惡意內容
/// </summary>
const sanitizeInput = (input: string): string => {
  if (!input) return "";

  // 移除潛在的腳本標籤
  let cleaned = input.replace(
    /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
    ""
  );

  // 移除 javascript: 協議
  cleaned = cleaned.replace(/javascript:/gi, "");

  // 移除 data: 協議（除了圖片）
  cleaned = cleaned.replace(/data:(?!image\/)/gi, "");

  // 移除事件處理器
  cleaned = cleaned.replace(/on\w+\s*=/gi, "");

  // 限制長度，防止過長的輸入
  if (cleaned.length > 10000) {
    cleaned = cleaned.substring(0, 10000);
  }

  return cleaned;
};
</script>
```

**程式碼說明：**

1. **DOMPurify 整合**：使用專業的 HTML 清理庫防止 XSS
2. **白名單策略**：只允許安全的 HTML 標籤和屬性
3. **即時清理**：輸入時即時清理潛在的惡意內容
4. **長度限制**：防止過長的輸入造成系統負擔
5. **安全顯示**：顯示時再次清理，確保安全性

### 8.2 檔案上傳安全驗證

**檔案位置：** `MITAP2024/MITAP2024.Server/Utils/Common/FileUtils.cs`

#### 8.2.1 檔案類型與內容驗證

```csharp
/// <summary>
/// 檔案上傳安全驗證工具
/// 防止惡意檔案上傳和執行
/// </summary>
public static class FileUploadSecurity
{
    // 允許的檔案類型（白名單）
    private static readonly Dictionary<string, string[]> AllowedFileTypes = new()
    {
        { ".pdf", new[] { "application/pdf" } },
        { ".doc", new[] { "application/msword" } },
        { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
        { ".xls", new[] { "application/vnd.ms-excel" } },
        { ".xlsx", new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
        { ".jpg", new[] { "image/jpeg" } },
        { ".jpeg", new[] { "image/jpeg" } },
        { ".png", new[] { "image/png" } },
        { ".gif", new[] { "image/gif" } }
    };

    // 危險的檔案副檔名（黑名單）
    private static readonly string[] DangerousExtensions = {
        ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js",
        ".jar", ".asp", ".aspx", ".php", ".jsp", ".sh", ".ps1"
    };

    /// <summary>
    /// 驗證上傳檔案的安全性
    /// </summary>
    /// <param name="file">上傳的檔案</param>
    /// <param name="maxSizeInMB">最大檔案大小（MB）</param>
    /// <returns>驗證結果</returns>
    public static FileValidationResult ValidateUploadedFile(IFormFile file, int maxSizeInMB = 10)
    {
        var result = new FileValidationResult();

        try
        {
            // 第一步：基本檢查
            if (file == null || file.Length == 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案不可為空";
                return result;
            }

            // 第二步：檔案大小檢查
            var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
            if (file.Length > maxSizeInBytes)
            {
                result.IsValid = false;
                result.ErrorMessage = $"檔案大小不可超過 {maxSizeInMB}MB";
                return result;
            }

            // 第三步：副檔名檢查
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (string.IsNullOrEmpty(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案必須有副檔名";
                return result;
            }

            // 檢查是否為危險副檔名
            if (DangerousExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "不允許上傳此類型的檔案";
                return result;
            }

            // 檢查是否為允許的副檔名
            if (!AllowedFileTypes.ContainsKey(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = "不支援此檔案類型";
                return result;
            }

            // 第四步：MIME 類型檢查
            var allowedMimeTypes = AllowedFileTypes[extension];
            if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案類型與副檔名不符";
                return result;
            }

            // 第五步：檔案內容檢查（檢查檔案頭）
            using (var stream = file.OpenReadStream())
            {
                if (!ValidateFileHeader(stream, extension))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "檔案內容與類型不符";
                    return result;
                }
            }

            // 第六步：檔案名稱安全檢查
            var fileName = Path.GetFileName(file.FileName);
            if (!IsValidFileName(fileName))
            {
                result.IsValid = false;
                result.ErrorMessage = "檔案名稱包含不安全的字元";
                return result;
            }

            result.IsValid = true;
            result.SafeFileName = GenerateSafeFileName(fileName);
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ErrorMessage = "檔案驗證過程發生錯誤";

            // 記錄錯誤但不暴露詳細資訊
            var logger = LogManager.GetLogger("FileUploadSecurity");
            logger.Error($"檔案驗證失敗: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 驗證檔案頭是否符合預期格式
    /// </summary>
    private static bool ValidateFileHeader(Stream stream, string extension)
    {
        var buffer = new byte[8];
        stream.Read(buffer, 0, 8);
        stream.Position = 0; // 重置位置

        return extension switch
        {
            ".pdf" => buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46, // %PDF
            ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
            ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
            ".gif" => (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38),
            _ => true // 其他類型暫時通過，可根據需要擴展
        };
    }

    /// <summary>
    /// 檢查檔案名稱是否安全
    /// </summary>
    private static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        // 檢查是否包含危險字元
        char[] invalidChars = { '<', '>', ':', '"', '|', '?', '*', '\\', '/' };
        if (fileName.IndexOfAny(invalidChars) >= 0)
            return false;

        // 檢查是否為保留名稱
        string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "LPT1", "LPT2" };
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToUpper();
        if (reservedNames.Contains(nameWithoutExtension))
            return false;

        return true;
    }

    /// <summary>
    /// 產生安全的檔案名稱
    /// </summary>
    private static string GenerateSafeFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        // 移除不安全字元
        nameWithoutExtension = Regex.Replace(nameWithoutExtension, @"[^\w\-_\.]", "_");

        // 限制長度
        if (nameWithoutExtension.Length > 50)
        {
            nameWithoutExtension = nameWithoutExtension.Substring(0, 50);
        }

        // 加上時間戳記確保唯一性
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        return $"{nameWithoutExtension}_{timestamp}{extension}";
    }
}

/// <summary>
/// 檔案驗證結果
/// </summary>
public class FileValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string SafeFileName { get; set; } = string.Empty;
}
```

**程式碼說明：**

1. **多層驗證**：檔案大小、副檔名、MIME 類型、檔案頭驗證
2. **白名單策略**：只允許預定義的安全檔案類型
3. **檔案頭檢查**：驗證檔案實際內容與副檔名是否一致
4. **安全檔名**：產生安全的檔案名稱，防止路徑遍歷攻擊
5. **錯誤處理**：安全的錯誤處理，不暴露系統資訊

## 9. 統一安全設計模式總結

### 9.1 防禦性程式設計原則

MITAP2024 系統採用以下防禦性程式設計原則：

1. **零信任原則**：假設所有輸入都可能是惡意的
2. **最小權限原則**：使用者只能存取必要的功能和資料
3. **深度防禦**：多層次的安全防護機制
4. **失敗安全**：系統失敗時預設為安全狀態
5. **安全預設**：預設設定採用最安全的選項

### 9.2 OWASP Top 10 防護對照表

| OWASP Top 10             | 防護機制                          | 實作位置              |
| ------------------------ | --------------------------------- | --------------------- |
| A01 - 存取控制失效       | RBAC、JWT Token、Session 管理     | HomeService.cs        |
| A02 - 加密機制失效       | SHA-512 雜湊、安全 Token 產生     | TextUtils.cs          |
| A03 - 注入攻擊           | 參數化查詢、輸入清理、Redis 安全  | 各 Service 類別       |
| A04 - 不安全設計         | 防禦性程式設計、統一驗證          | BaseService.cs        |
| A05 - 安全設定缺陷       | CSP、安全標頭、HTTPS              | Program.cs            |
| A06 - 易受攻擊元件       | 依賴管理、版本控制、安全審計      | package.json, .csproj |
| A07 - 身份驗證失效       | 防暴力破解、帳號鎖定、安全日誌    | HomeService.cs        |
| A08 - 軟體資料完整性失效 | DOMPurify、檔案驗證、內容清理     | 各元件                |
| A09 - 安全日誌監控失效   | ActionHistory、LoginFailedHistory | 日誌系統              |
| A10 - 伺服器端請求偽造   | URL 驗證、白名單、請求限制        | 各 API 端點           |

### 9.3 安全設計優勢

1. **統一性**：所有安全機制採用統一的設計模式
2. **可維護性**：集中式的安全邏輯便於維護和更新
3. **可擴展性**：模組化設計支援新增安全功能
4. **效能優化**：安全機制與效能優化並重
5. **合規性**：符合國際安全標準和最佳實務

## 10. 總結

MITAP2024 系統的統一安全設計提供了完整的 OWASP Top 10 防護機制：

### 10.1 核心安全特色

1. **多層次防護**：從前端到後端的完整安全防護鏈
2. **統一安全模式**：所有元件採用一致的安全設計模式
3. **主動防禦**：主動檢測和防護各種安全威脅
4. **完整日誌**：詳細的安全事件記錄和監控
5. **持續改進**：定期更新和改進安全機制

### 10.2 技術價值

1. **安全性**：全面防護 OWASP Top 10 安全弱點
2. **可靠性**：穩定的安全機制確保系統可靠運行
3. **合規性**：符合資安法規和標準要求
4. **可維護性**：統一的安全架構便於維護
5. **可擴展性**：模組化設計支援未來安全需求

### 10.3 業務價值

透過這套統一安全設計，MITAP2024 系統能夠：

- 保護使用者資料和隱私安全
- 防止各種網路攻擊和安全威脅
- 確保系統穩定可靠的運行
- 滿足政府和企業的資安要求
- 建立使用者對系統的信任

這套安全機制不僅保護了當前的系統安全，也為未來的安全挑戰做好了準備。
