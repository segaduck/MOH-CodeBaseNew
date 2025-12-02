# EECOnline 系統完整分析報告

## 1. 系統概述

### 1.1 基本資訊

| 項目          | 說明                                     |
| ------------- | ---------------------------------------- |
| **系統名稱**  | EECOnline (民眾線上申辦電子病歷服務平台) |
| **專案 GUID** | `{8B0971ED-8D8F-44E2-B1F9-0A0FBD10B87F}` |
| **目標框架**  | .NET Framework 4.5.2                     |
| **MVC 版本**  | ASP.NET MVC 5.2.3                        |
| **ORM 框架**  | iBATIS.NET (MyBatis 前身)                |
| **背景排程**  | Hangfire                                 |
| **日誌框架**  | log4net                                  |

### 1.2 系統用途

EECOnline 是衛生福利部建置的 **電子病歷申請管理系統**，提供以下主要功能：

- 民眾可線上申請並取得電子病歷
- 醫院端可管理病歷申請案件
- 衛福部後台可進行系統管理與統計

---

## 2. 系統架構

### 2.1 目錄結構

```
trunk/
├── Areas/                    # MVC 區域 (功能模組)
│   ├── A1/                   # 醫院管理模組
│   ├── A2/                   # 案件管理模組
│   ├── A3/                   # 帳務計算模組
│   ├── A4/                   # 報表統計模組
│   ├── A5/                   # 帳號管理模組
│   ├── A6/                   # 系統權限模組
│   ├── A7/                   # 病歷設定模組
│   ├── A8/                   # 紀錄查詢模組
│   ├── BackApply/            # 後台案件處理
│   ├── Login/                # 登入模組
│   └── SHARE/                # 共用模組
├── Controllers/              # 全域控制器
├── DataLayers/               # 資料存取層 (DAO)
├── Models/                   # 資料模型
│   └── Entities/             # 資料庫實體
├── Services/                 # 業務邏輯服務
├── Utils/                    # 工具類別
├── SqlMaps/                  # iBATIS SQL 對映檔
├── Views/                    # 視圖
├── HCA/                      # 健保卡認證元件
└── Uploads/                  # 上傳檔案目錄
    └── XSLTemplate/          # XML 轉 HTML 樣式表
```

### 2.2 MVC 區域功能對照

| 區域          | 功能說明             | 主要控制器                       |
| ------------- | -------------------- | -------------------------------- |
| **Login**     | 登入/登出/忘記密碼   | C101MController, C102MController |
| **A1**        | 醫院基本資料管理     | C101MController, C102MController |
| **A2**        | 電子病歷申請案件管理 | C101MController, C102MController |
| **A3**        | 帳務計算與費用管理   | C101M~C104MController            |
| **A4**        | 報表統計與查詢       | C101M~C103MController            |
| **A5**        | 使用者帳號管理       | -                                |
| **A6**        | 系統權限設定         | -                                |
| **A7**        | 電子病歷設定管理     | -                                |
| **A8**        | 操作紀錄查詢         | -                                |
| **BackApply** | 後台案件處理         | -                                |
| **SHARE**     | 共用功能模組         | -                                |

---

## 3. 資料庫架構

### 3.1 連線設定

- **設定檔**: `properties.config` + `SqlMap.config`
- **資料庫類型**: SQL Server
- **連線名稱**: `connectionEUSERVICE`

### 3.2 主要資料表

#### 帳號權限相關

| 資料表         | 說明             |
| -------------- | ---------------- |
| `AMDBURM`      | 使用者帳號主檔   |
| `AMGRP`        | 權限群組         |
| `AMUROLE`      | 使用者角色對照   |
| `AMFUNCM`      | 功能選單         |
| `AMGMAPM`      | 群組功能權限對照 |
| `EEC_Hospital` | 醫院端帳號       |
| `AMUROLE_Hosp` | 醫院端使用者角色 |
| `AMGRP_Hosp`   | 醫院端權限群組   |

#### 電子病歷相關

| 資料表                         | 說明             |
| ------------------------------ | ---------------- |
| `EEC_Apply`                    | 電子病歷申請主檔 |
| `EEC_ApplyDetail`              | 申請明細         |
| `EEC_ApplyDetailPrice`         | 申請費用明細     |
| `EEC_ApplyDetailPrice_ApiData` | API 回傳資料暫存 |
| `EEC_Hospital_Api`             | 醫院 API 設定    |
| `EEC_Hospital_SetPrice`        | 醫院收費設定     |

#### 系統管理相關

| 資料表         | 說明         |
| -------------- | ------------ |
| `LOGINLOG`     | 登入紀錄     |
| `VISIT_RECORD` | 功能使用紀錄 |
| `SETUP`        | 系統設定參數 |
| `ENEWS`        | 公告消息     |
| `EFAQ`         | 常見問題     |
| `MAILLOG`      | 郵件發送紀錄 |

---

## 4. 認證與安全機制

### 4.1 登入方式

系統支援多種登入方式：

| 登入方式       | 說明           | 實作位置                      |
| -------------- | -------------- | ----------------------------- |
| **帳號密碼**   | 後台管理員登入 | `LoginController`             |
| **醫院授權碼** | 醫院端登入     | `LoginController` (ThePage=2) |
| **健保卡**     | 民眾身分驗證   | `HomeController` + HCA 元件   |
| **TW FidO**    | 行動自然人憑證 | `HomeController`              |

### 4.2 密碼加密方式

系統使用 **SHA-512 + Base64** 進行密碼加密：

```csharp
public static string CypherText(string originText)
{
    using (SHA512CryptoServiceProvider sha512 = new SHA512CryptoServiceProvider())
    {
        byte[] dataToHash = Encoding.UTF8.GetBytes(originText);
        byte[] hashvalue = sha512.ComputeHash(dataToHash);
        return Convert.ToBase64String(hashvalue);
    }
}
```

### 4.3 密碼規則

- 長度至少 8 字元
- 必須包含大寫英文字母
- 必須包含小寫英文字母
- 必須包含數字
- 不得含空白字元
- 不得與帳號相同
- 不得與前三次密碼相同

---

## 5. 外部整合

### 5.1 醫院 API 整合

系統與多家醫院進行電子病歷資料交換：

| 醫院                 | 代碼          | 介接方式            | 實作檔案                     |
| -------------------- | ------------- | ------------------- | ---------------------------- |
| 亞東紀念醫院         | `1131010011H` | RESTful API + Token | `Hospital_FarEastern_Api.cs` |
| 中山醫學大學附設醫院 | `1317040011H` | SOAP Web Service    | `Hospital_csh_Api.cs`        |
| 中國醫藥大學附設醫院 | `1317050017H` | (待實作)            | `Hospital_cmuh_Api.cs`       |
| 其他醫院             | -             | 共用 RESTful API    | `Hospital_Common_Api.cs`     |

### 5.2 API 功能說明

#### 共用 API (`Hospital_Common_Api.cs`)

| API                 | 方法                | 用途                  |
| ------------------- | ------------------- | --------------------- |
| `/api/login`        | `GetLoginToken()`   | 登入取得 Token        |
| `/api/queryindex`   | `GetQueryIndex()`   | 查詢可申請病歷列表    |
| `/api/querycontent` | `GetQueryContent()` | 下載病歷內容 (Base64) |

#### 亞東醫院 API (`Hospital_FarEastern_Api.cs`)

| API      | 方法         | 用途                   |
| -------- | ------------ | ---------------------- |
| A1 API   | `Api_A1()`   | 查詢可申請病歷類型列表 |
| A2-1 API | `Api_A2_1()` | 送出病歷申請           |
| A4 API   | `Api_A4()`   | 確認病歷已下載         |

### 5.3 健保卡驗證

系統透過健保署 API 進行健保卡身分驗證：

- **回傳格式**: JWT Token
- **驗證欄位**: `sub` (UUID), `result` (驗證結果)
- **驗證成功**: `result` 包含 "success"

### 5.4 TW FidO 行動自然人憑證

支援透過 `transaction_id` 進行行動自然人憑證驗證登入。

---

## 6. 核心控制器說明

### 6.1 HomeController

主要處理民眾前台功能：

- `Index()` - 首頁與登入結果處理
- `LoginForm1()` - 自然人憑證登入
- `LoginForm2()` - TW FidO 登入
- `LoginForm3()` - 健保卡登入
- `SearchLoginForm()` - 申請進度查詢登入

### 6.2 AjaxController

處理 AJAX 非同步請求與背景排程任務：

- `CheckDataIsGot_Or_SendEmailTo_FarEastern()` - 每日 4:00 檢查亞東醫院病歷狀態

### 6.3 Login/C101MController

後台登入控制器：

- `Login()` - 處理登入驗證
- `PasswordChange()` - 變更密碼
- `Forget()` / `Send()` - 忘記密碼處理
- `HospPwdChange()` - 醫院端密碼變更

---

## 7. 資料存取層 (DAO)

系統使用 iBATIS.NET 進行資料存取：

| DAO 類別       | 對應 SqlMap   | 用途     |
| -------------- | ------------- | -------- |
| `LoginDAO`     | Login.xml     | 登入相關 |
| `FrontDAO`     | Front.xml     | 前台功能 |
| `A1DAO`        | A1.xml        | 醫院管理 |
| `A2DAO`        | A2.xml        | 案件管理 |
| `A3DAO`        | A3.xml        | 帳務管理 |
| `A4DAO`        | A4.xml        | 報表統計 |
| `A5DAO`        | A5.xml        | 帳號管理 |
| `A6DAO`        | A6.xml        | 權限管理 |
| `A7DAO`        | A7.xml        | 病歷設定 |
| `A8DAO`        | A8.xml        | 紀錄查詢 |
| `BackApplyDAO` | BackApply.xml | 後台案件 |
| `SHAREDAO`     | SHARE.xml     | 共用功能 |

---

## 8. 背景排程 (Hangfire)

### 8.1 排程設定

系統使用 Hangfire 處理背景排程任務：

```csharp
// HangfireBootstrapper.cs
GlobalConfiguration.Configuration.UseSqlServerStorage("connectionEUSERVICE");
```

### 8.2 排程任務

| 任務                                       | 執行時間   | 說明                     |
| ------------------------------------------ | ---------- | ------------------------ |
| `CheckDataIsGot_Or_SendEmailTo_FarEastern` | 每日 04:00 | 檢查亞東醫院病歷回覆狀態 |

---

## 9. 電子病歷處理流程

### 9.1 申請流程

```
民眾登入 → 選擇醫院 → 查詢可申請病歷 (A1 API)
    ↓
選擇病歷項目 → 確認申請 → 線上付款
    ↓
系統送出申請 (A2 API) → 等待醫院回覆
    ↓
民眾下載病歷 → 確認下載 (A4 API)
```

### 9.2 病歷格式轉換

系統支援將 XML 格式病歷透過 XSLT 轉換為 HTML：

| 病歷類型代碼 | 說明     | XSL 樣式表                            |
| ------------ | -------- | ------------------------------------- |
| `121`        | 門診病歷 | `121ClinicalMedical門診病歷.xsl`      |
| `114`        | 門診用藥 | `114OutPatientMedication門診用藥.xsl` |
| `113`        | 血液檢驗 | `113BloodTest血液.xsl`                |
| `004`        | 醫療影像 | `ImageReport醫療影像.xsl`             |
| `115`        | 出院病摘 | `115Discharge出院病摘.xsl`            |

---

## 10. 安全性設定

### 10.1 TLS 設定

強制使用 TLS 1.2 進行安全連線：

```csharp
ServicePointManager.SecurityProtocol =
    SecurityProtocolType.Ssl3 |
    SecurityProtocolType.Tls |
    SecurityProtocolType.Tls11 |
    SecurityProtocolType.Tls12;
```

### 10.2 權限控制

系統透過以下機制進行權限控制：

1. **Session 管理**: `SessionModel` 儲存登入者資訊
2. **角色權限**: `ClamRoleFunc` 定義功能權限
3. **群組管理**: `AMGRP` / `AMGRP_Hosp` 定義群組

### 10.3 Web.config 安全設定

```xml
<!-- 權限檢核開關 -->
<add key="DisableAuthorize" value="1" />

<!-- 錯誤訊息顯示等級 -->
<add key="ErrorPageShowExeption" value="2" />
```

---

## 11. 部署注意事項

### 11.1 必要設定

1. 更新 `properties.config` 中的資料庫連線字串
2. 更新 `Web.config` 中的 `connectionStrings`
3. 設定 `SETUP` 資料表中的系統參數
4. 設定醫院 API 連線資訊

### 11.2 環境變數

| 設定項                | 說明                      | 位置         |
| --------------------- | ------------------------- | ------------ |
| `NetID`               | 網路環境 (1=內網, 2=外網) | Web.config   |
| `UpLoadFile`          | 上傳檔案路徑              | Web.config   |
| `TempPath`            | 暫存檔案路徑              | Web.config   |
| `Hospital_Common_Api` | 共用 API 網址             | SETUP 資料表 |

---

## 12. 技術元件與相依套件

### 12.1 主要 NuGet 套件

| 套件名稱          | 用途         |
| ----------------- | ------------ |
| `iBatisNet`       | ORM 資料存取 |
| `Hangfire`        | 背景排程     |
| `log4net`         | 日誌記錄     |
| `Newtonsoft.Json` | JSON 處理    |
| `iTextSharp`      | PDF 處理     |
| `EPPlus`          | Excel 處理   |
| `HtmlAgilityPack` | HTML 解析    |
| `JWT`             | Token 處理   |

### 12.2 自訂元件

| 元件     | 說明           |
| -------- | -------------- |
| `HCA`    | 健保卡認證元件 |
| `HppApi` | 線上付款元件   |
| `RSACSP` | RSA 加密元件   |
| `Turbo`  | 通用工具函式庫 |

---

## 13. 與 e-Service 系統比較

| 項目         | EECOnline                         | e-Service                    |
| ------------ | --------------------------------- | ---------------------------- |
| **用途**     | 電子病歷申請平台                  | 醫事線上申辦系統             |
| **ORM 框架** | iBATIS.NET                        | Dapper                       |
| **連線設定** | properties.config + SqlMap.config | Web.config connectionStrings |
| **密碼加密** | SHA-512                           | SHA-256                      |
| **外部整合** | 醫院 API / 健保署                 | MDOD 醫事簽審 (SFTP)         |
| **認證方式** | 帳密 / 健保卡 / TW FidO           | 帳密 / AD                    |

---

## 14. 更新紀錄

| 日期       | 版本 | 說明             |
| ---------- | ---- | ---------------- |
| 2025-11-28 | 1.0  | 初版系統分析報告 |
