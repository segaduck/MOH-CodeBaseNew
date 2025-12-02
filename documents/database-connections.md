# 資料庫連線說明文件

## 概述

本系統 (e-Service 線上申辦系統) 使用多個資料庫連線，定義於 `Web.config` 檔案中。本文件說明各連線的用途、使用方式及部署注意事項。

---

## 連線字串清單

### 1. DefaultConnection (主要資料庫)

```xml
<add name="DefaultConnection" 
     connectionString="Data Source=127.0.0.1,1433;Initial Catalog=eservice_new;User ID=sa;Password=YourStrong!Passw0rd;Connect Timeout=60" 
     providerName="System.Data.SqlClient" />
```

| 項目 | 說明 |
|------|------|
| **用途** | e-Service 系統主要資料庫，儲存所有申請案件、會員資料、系統設定等 |
| **資料庫名稱** | `eservice_new` |
| **存取模式** | **讀寫 (Read-Write)** |
| **使用位置** | 全系統 DAO、Controller、Action |

**主要資料表：**
- `MEMBER` - 會員帳號資料
- `APPLY` - 申請案件主檔
- `APPLY_XXXXXX` - 各類申請案件明細
- `SETUP` - 系統設定
- `ADMIN` - 後台管理員
- `CODE` - 代碼對照表

---

### 2. MDODConnection (醫事簽審資料庫)

```xml
<add name="MDODConnection" 
     providerName="System.Data.SqlClient" 
     connectionString="Data Source=203.65.109.238;Initial Catalog=MDOD;User ID=dohdbmg;Password=doh10769#$" />
```

| 項目 | 說明 |
|------|------|
| **用途** | 醫事人員/器材簽審系統 (Ministry of Health 醫事簽審) |
| **資料庫名稱** | `MDOD` |
| **伺服器位址** | `203.65.109.238` (衛福部外部伺服器) |
| **存取模式** | **僅供開發工具查詢 (限定用途)** |

⚠️ **重要說明：此連線指向外部政府伺服器，需在衛福部內部網路或 VPN 環境下才可存取。**

---

### 3. SFTPConnection (SFTP 檔案傳輸)

```xml
<add name="SFTPConnection" 
     connectionString="" 
     providerName="" />
```

| 項目 | 說明 |
|------|------|
| **用途** | SFTP 檔案傳輸設定 (佔位符) |
| **狀態** | 未使用，實際 SFTP 設定於 `SETUP` 資料表或 `Web.config` 的 AppSettings |

---

## MDODConnection 詳細說明

### 使用位置

#### 1. DBController (開發工具)

**路徑：** `Areas/Resource/Controllers/DBController.cs`  
**網址：** `/Resource/DB?Validate=turbo@13141806`

此為開發人員專用的 SQL 查詢工具，可選擇使用 `DefaultConnection` 或 `MDODConnection` 執行查詢。

**功能：**
- 執行任意 SQL 查詢 (SELECT, INSERT, UPDATE, DELETE)
- 透過「是否異動資料庫」核取方塊控制是否提交變更

```csharp
// DBController.cs 關鍵程式碼
if (Request["isModify"].Equals("true"))
{
    tran.Commit();  // 提交變更
}
else
{
    tran.Rollback();  // 唯讀模式，回復變更
}
```

#### 2. MDOD 排程作業 (注意：非直接使用 MDODConnection)

**路徑：** 
- `Areas/Schedule/Controllers/MdodController.cs`
- `Areas/Schedule/Action/MdodAction.cs`

⚠️ **重要澄清：** 雖然檔名包含 "Mdod"，但此排程作業**不直接連接 MDOD 資料庫**！

**實際運作流程：**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  e-Service DB   │ → │   產生 XML 檔案   │ → │   SFTP 上傳     │
│ (DefaultConn)   │    │  (APP110~112)   │    │  至 MDOD 系統   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         ↓
   更新 TO_MIS_MK = 'Y'
```

**涉及資料表：**
- `APPLY_001034` - 醫事器材輸入許可證申請
- `APPLY_001035` - 醫事器材輸入同意書申請  
- `APPLY_001038` - 醫事器材輸入查驗申請

---

## 資料庫操作權限總覽

| 元件 | 資料庫 | 操作權限 |
|------|--------|----------|
| **所有業務邏輯** | DefaultConnection | 讀寫 |
| **DBController** (開發工具) | DefaultConnection | 讀寫 |
| **DBController** (開發工具) | MDODConnection | 讀寫 (需勾選) |
| **MdodAction** (排程) | DefaultConnection | 讀取 + 更新旗標 |

---

## 部署注意事項

### 正式環境部署

1. **DefaultConnection**
   - 更新 `Data Source` 為正式 SQL Server 位址
   - 更新帳號密碼為正式環境憑證
   - 確認防火牆開放連線

2. **MDODConnection**
   - 此連線需在衛福部內網環境才可使用
   - 確認 IP `203.65.109.238` 可從應用程式伺服器存取
   - 若無法存取，DBController 的「醫事簽審」選項將無法使用

3. **SFTP 設定**
   - MDOD 資料傳輸使用 SFTP，非直接資料庫連線
   - 相關設定項目於 `Web.config` AppSettings：
     - `SCHEDULE_MDOD_FTP_SERVER`
     - `SCHEDULE_MDOD_FTP_ACCOUNT`
     - `SCHEDULE_MDOD_FTP_PASSWORD`
     - `SCHEDULE_MDOD_XML_PATH`

### 本機開發環境

- `DefaultConnection` 可使用本機 Docker SQL Server
- `MDODConnection` 通常無法存取 (需 VPN)
- MDOD 排程功能在本機測試時可能因 SFTP 無法連線而失敗

---

## 相關程式碼位置

| 檔案 | 說明 |
|------|------|
| `Web.config` | 連線字串定義 |
| `Utils/DataUtils.cs` | `GetConnection()` 方法 |
| `Controllers/BaseController.cs` | 基礎連線取得 |
| `Areas/Resource/Controllers/BaseController.cs` | Resource 區域連線取得 |
| `Areas/Resource/Controllers/DBController.cs` | SQL 查詢開發工具 |
| `Areas/Schedule/Controllers/MdodController.cs` | MDOD 排程控制器 |
| `Areas/Schedule/Action/MdodAction.cs` | MDOD 資料存取層 |

---

## 更新紀錄

| 日期 | 說明 |
|------|------|
| 2025-11-28 | 初版建立 |

