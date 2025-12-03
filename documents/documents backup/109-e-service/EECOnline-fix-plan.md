# EECOnline 系統修復計畫

## 優先級說明

| 等級      | 說明                       | 建議時間       |
| --------- | -------------------------- | -------------- |
| 🔴 **P0** | 嚴重安全漏洞，必須立即修復 | 1-2 天         |
| 🟠 **P1** | 安全風險，應盡快修復       | 1 週內         |
| 🟡 **P2** | 品質問題，建議修復         | 2-4 週內       |
| 🟢 **P3** | 優化建議                   | 可排入版本計畫 |

---

## 🔴 P0 - 嚴重問題 (立即修復)

### 1. 權限檢核被停用

**問題位置**: `Web.config` 第 82 行

```xml
<add key="DisableAuthorize" value="1" />
```

**風險**: 所有權限檢核被 bypass，任何登入使用者可存取所有功能

**修復方式**:

```xml
<add key="DisableAuthorize" value="0" />
```

**修復時間**: 5 分鐘

---

### 2. 敏感資訊外洩 - 設定檔中有明文密碼

**問題位置**: `bin/EECOnline.dll.config` 第 19-20 行

```xml
<add name="connectionEUSERVICE" connectionString="...Password=EUservice110..." />
```

**問題位置**: `Web.config` 第 153-154 行

```xml
<add key="LoginUser" value="A21030000I" />
<add key="LoginPwd" value="I0023A" />
```

**風險**: 資料庫密碼、API 帳密明文儲存

**修復方式**:

1. 使用環境變數或 Azure Key Vault
2. 或至少加密 connection string

**修復時間**: 2-4 小時

---

### 3. 錯誤訊息顯示過多資訊

**問題位置**: `Web.config` 第 90 行

```xml
<add key="ErrorPageShowExeption" value="2" />
```

**風險**: 顯示完整 Exception 及 Stacktrace，洩漏系統內部資訊

**修復方式**:

```xml
<add key="ErrorPageShowExeption" value="0" />
```

**修復時間**: 5 分鐘

---

## 🟠 P1 - 安全風險 (1 週內修復)

### 4. SQL Injection 風險

**問題位置**: `SqlMaps/A1.xml` 第 54-63 行

```sql
SET @WHERE = @WHERE + ' and city like ''%' + @WHERE_CITY + '%'''
```

**風險**: 動態 SQL 拼接，可能導致 SQL Injection

**修復方式**: 使用 iBATIS 參數化查詢

```xml
<isNotEmpty property="city">
  AND city LIKE '%' + #city# + '%'
</isNotEmpty>
```

**影響範圍**: 檢查所有 `SqlMaps/*.xml` 中的動態 SQL

**修復時間**: 1-2 天

---

### 5. Content-Security-Policy 被註解

**問題位置**: `Web.config` 第 191 行

```xml
<!-- <add name="Content-Security-Policy" value="..." />-->
```

**風險**: 無法防止 XSS 及資料注入攻擊

**修復方式**: 啟用並正確設定 CSP Header

**修復時間**: 2-4 小時

---

### 6. Debug 模式開啟

**問題位置**: `Web.config` 第 162 行

```xml
<compilation debug="true" targetFramework="4.5.2" />
```

**風險**: 效能降低、記憶體增加、洩漏除錯資訊

**修復方式**:

```xml
<compilation debug="false" targetFramework="4.5.2" />
```

**修復時間**: 5 分鐘

---

## 🟡 P2 - 品質問題 (2-4 週內修復)

### 7. 測試帳號殘留

**問題位置**: `Web.config` 第 72-74 行 (被註解但仍存在)

```xml
<!-- <add key="StressTestMode" value="Y" /> -->
<!-- <add key="StressTestUserNo" value="johnsoncj" /> -->
```

**修復方式**: 完全移除測試相關設定

---

### 8. Session 狀態管理被註解

**問題位置**: `Web.config` 第 172-175 行

```xml
<!--<sessionState mode="StateServer" ... />-->
```

**風險**: 使用預設 InProc 模式，無法支援 Web Farm

**修復方式**: 根據部署環境啟用適當的 Session 狀態模式

---

### 9. 硬編碼內網 IP

**問題位置**: `Web.config` 第 93, 116 行

```xml
<add key="BallotUrl" value="http://172.28.11.92/Ballot/..." />
<add key="FtpNasServer" value="ftp://172.28.100.39/..." />
```

**修復方式**: 改用 SETUP 資料表或環境變數管理

---

## 🟢 P3 - 優化建議

### 10. 框架升級

| 項目           | 目前版本 | 建議版本          |
| -------------- | -------- | ----------------- |
| .NET Framework | 4.5.2    | 4.8 或 .NET 6+    |
| MVC            | 5.2.3    | 5.2.9             |
| iBATIS.NET     | (已過時) | Dapper 或 EF Core |

### 11. 安全性增強建議

- 加入 CSRF Token 驗證
- 實作密碼有效期限
- 加入帳號鎖定機制 (已有 errct 欄位但未完整實作)
- 使用 HTTPS 強制重導向

---

## 快速修復指南 (30 分鐘內可完成)

### Step 1: 修改 Web.config (5 分鐘)

```xml
<!-- 第 82 行: 啟用權限檢核 -->
<add key="DisableAuthorize" value="0" />

<!-- 第 90 行: 關閉錯誤詳細資訊 -->
<add key="ErrorPageShowExeption" value="0" />

<!-- 第 162 行: 關閉 Debug 模式 -->
<compilation debug="false" targetFramework="4.5.2" />
```

### Step 2: 啟用 CSP Header (10 分鐘)

```xml
<!-- 第 191 行附近，取消註解並調整 -->
<add name="Content-Security-Policy"
     value="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self';" />
```

### Step 3: 移除敏感資訊 (15 分鐘)

1. 將 `bin/EECOnline.dll.config` 從版控移除
2. 將 `Web.config` 中的密碼設定移至環境變數或外部設定

---

## 修復檢查清單

| #   | 項目                         | 優先級 | 狀態 |
| --- | ---------------------------- | ------ | ---- |
| 1   | DisableAuthorize 設為 0      | P0     | ☐    |
| 2   | 移除明文密碼                 | P0     | ☐    |
| 3   | ErrorPageShowExeption 設為 0 | P0     | ☐    |
| 4   | 修復 SQL Injection (SqlMaps) | P1     | ☐    |
| 5   | 啟用 Content-Security-Policy | P1     | ☐    |
| 6   | Debug 模式設為 false         | P1     | ☐    |
| 7   | 移除測試帳號設定             | P2     | ☐    |
| 8   | 設定 Session State           | P2     | ☐    |
| 9   | 移除硬編碼 IP                | P2     | ☐    |
| 10  | 框架升級評估                 | P3     | ☐    |

---

## 預估工時

| 優先級 | 項目數 | 預估工時 |
| ------ | ------ | -------- |
| P0     | 3 項   | 4 小時   |
| P1     | 3 項   | 2 天     |
| P2     | 3 項   | 1 週     |
| P3     | 2 項   | 依規劃   |

**最小可行修復 (P0 + P1)**: 約 3 個工作天

---

## 更新紀錄

| 日期       | 版本 | 說明         |
| ---------- | ---- | ------------ |
| 2025-11-28 | 1.0  | 初版修復計畫 |
