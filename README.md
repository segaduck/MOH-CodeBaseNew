# MOH e-Service Systems - 衛生福利部線上服務系統

本倉庫包含兩個重要的ASP.NET MVC網頁應用系統，為台灣衛生福利部提供線上服務平台。

## 系統概覽 | System Overview

### 1. e-service - 人民申請案件線上申辦服務系統
**Ministry of Health Online Application Service Platform**

一個綜合性的政府電子化服務平台，提供民眾、醫療專業人員及機構線上申辦各類衛生福利相關業務。

#### 技術架構 | Technology Stack
- **框架**: ASP.NET MVC 4 (.NET Framework 4.8)
- **資料存取**: Dapper (主要) + Entity Framework 5.0
- **資料庫**: SQL Server
- **前端**: Bootstrap 4.5.3, jQuery 3.7.1, Vue.js 2.6.11
- **背景作業**: Hangfire 1.7.19
- **文件處理**: iTextSharp, NPOI, DocX, OpenXml
- **安全性**: AntiXSS, JWT, BouncyCastle加密

#### 主要功能 | Key Features

**前台服務 (40+ 申辦項目)**
- 🏥 **醫事司業務**: 醫事人員證書申請、英文證明、執業登記等
- 🌿 **中醫藥司業務**: 中醫藥相關證照申辦、GMP證明
- 💰 **國民年金業務**: 國民年金爭議審議
- 📋 **檔案應用業務**: 政府檔案調閱申請
- 👥 **社會工作業務**: 社工師證照申辦、繼續教育積分審查

**多元身分驗證**
- 自然人憑證 (Natural Person Certificate)
- 工商憑證 (Business Certificate)
- 醫事人員憑證 (Healthcare Professional Certificate)
- 數位身分證 (Digital ID)
- 一般帳號密碼 (Username/Password)

**金流整合**
- 我的E政府支付閘道
- ATM虛擬帳號繳費
- 信用卡線上繳費
- 多通路繳費對帳

**後台管理系統**
- 案件指派與流程管理
- 多層級審核機制
- 案件追蹤與通知
- 逾期案件管理
- 郵件自動通知

**系統整合**
- 醫審系統 (MDOD) 整合
- 我的E政府入口網API串接
- 外部金流服務整合
- SFTP檔案交換

**特殊服務**
- 化粧品產品登錄
- 捐款管理系統
- 防疫旅館繳費系統 (疫情相關)

#### 專案結構 | Project Structure
```
e-service/
├── ES/                     # 主要應用程式專案
│   ├── Areas/             # MVC Areas模組化架構
│   │   ├── BACKMIN/      # 後台管理區 (71個控制器)
│   │   ├── Admin/        # 行政管理區
│   │   └── Schedule/     # 排程任務區 (10個控制器)
│   ├── Controllers/      # 前台控制器 (63個控制器)
│   ├── Models/
│   │   ├── Entities/    # 資料庫實體模型 (160+檔案)
│   │   └── ViewModels/  # 視圖模型
│   ├── DataLayers/      # 資料存取層 (9個DAO)
│   ├── Action/          # 業務邏輯層 (13個action)
│   ├── Services/        # 服務層
│   ├── Utils/           # 工具類別 (20+工具)
│   ├── Views/           # Razor視圖
│   └── Web.config       # 主要配置檔
├── packages/            # NuGet套件
└── ES.sln              # 解決方案檔
```

#### 核心DAO類別 | Core DAOs
- **ApplyDAO.cs** (1.2MB) - 申辦案件資料存取
- **BackApplyDAO.cs** (933KB) - 後台案件處理
- **LoginDAO.cs** - 身分驗證
- **PayDAO.cs** - 金流處理
- **SHAREDAO.cs** (134KB) - 共用資料
- **APIDAO.cs** - API操作

#### 排程作業 | Scheduled Jobs
```
每小時執行: 醫審系統XML上傳
每日21:00: 我的E政府API管理
每日01:00: 逾期通知郵件
每日00:20: 繳費檔案產生
每日07:40: 繳費檔案處理
```

---

### 2. trunk (EECOnline) - 電子病歷服務平台申辦系統
**Electronic Medical Record E-Application System**

醫療服務電子申辦系統，提供醫療機構與政府單位間的電子病歷申辦、帳務計算及報表統計功能。

#### 技術架構 | Technology Stack
- **框架**: ASP.NET MVC 5.2.3 (.NET Framework 4.5.2)
- **資料存取**: iBATIS/MyBatis (主要) + Dapper
- **資料庫**: SQL Server (EEC_PD_DB)
- **前端**: Bootstrap 3.3.7, jQuery 2.1.3
- **背景作業**: Hangfire 1.7.18
- **文件處理**: EPPlus, NPOI, iTextSharp, OpenXml
- **框架庫**: Turbo.Lib (自訂框架)

#### 主要功能 | Key Features

**功能模組 (8個主要區域)**
- **A1** - 病歷設定管理 (Medical Record Configuration)
- **A2** - 申辦案件管理 (Application Case Management)
- **A3** - 帳務計算管理 (Billing/Account Calculation)
- **A4** - 報表統計管理 (Report Statistics)
- **A5** - 帳號管理 (Account Management)
- **A6** - 系統權限管理 (System Permission Management)
- **A7** - 病歷設定管理 (Medical Record Configuration)
- **A8** - 紀錄查詢 (Record Inquiry/Audit Logs)

**雙重登入機制**
- Tab 1: 帳號密碼驗證 (內部使用者)
- Tab 2: 醫院授權碼驗證 (外部醫療機構)
- 自然人憑證支援 (HCA整合)

**背景作業排程**
```
每日04:00: 資料狀態檢查與郵件通知
每日04:00: 資料驗證郵件
每日00:20: 帳務檔案匯出
每日07:40: 檔案下載處理
每日07:50: 資料匯入
```

**報表與匯出**
- 自訂報表引擎 (Report.config)
- 多格式匯出: PDF, Excel, Word
- REST API資料查詢支援

**檔案管理**
- 支援格式: PDF, Excel, MDB, TXT, JPG, Word
- FTP整合 (NAS伺服器)
- 檔案上傳/下載功能

#### 專案結構 | Project Structure
```
trunk/
├── EECOnline.csproj    # 主專案檔
├── Areas/              # 功能模組
│   ├── A1-A8/         # 8個業務模組
│   ├── Login/         # 登入驗證
│   ├── SHARE/         # 共用元件
│   └── BackApply/     # 後台申辦
├── Controllers/        # 根控制器
├── Models/
│   └── Entities/      # 資料實體 (60個檔案)
├── DataLayers/        # DAO資料存取層
├── Services/          # 業務邏輯服務
├── SqlMaps/           # iBATIS SQL對應檔
├── Commons/           # 共用工具與過濾器
├── Helpers/           # HTML輔助擴充
└── Views/             # Razor視圖
```

#### SQL對應檔 | SQL Mapping Files
- **SqlMap.config** - iBATIS主設定
- **A1.xml ~ A8.xml** - 各模組SQL查詢
- **Login.xml** - 身分驗證查詢
- **Report.xml** - 報表定義
- **REST.xml** - REST API定義

#### 資料存取層 | Data Access Layer
```
BaseDAO (繼承 Turbo.DataLayer.RowBaseDAO)
  ├── A1DAO ~ A8DAO  # 模組資料存取
  ├── FrontDAO       # 首頁資料
  ├── LoginDAO       # 身分驗證
  ├── SHAREDAO       # 共用元件
  └── TransLogDAO    # 交易記錄
```

#### 核心實體 | Key Entities
- **TblAPPLY** - 申辦案件
- **TblAMUROLE / TblAMUROLE_Hosp** - 使用者/醫院角色
- **TblAMGRP / TblAMGRP_Hosp** - 使用者/醫院群組
- **TblAMFUNCM** - 功能定義
- **TblEEC_Hospital** - 醫院主檔
- **TblCODE** - 代碼表

---

## 系統特色 | System Characteristics

### e-service 系統
- ✅ **多租戶架構**: 服務多個衛福部業務司
- ✅ **雙語介面**: 支援中英文
- ✅ **高安全性**: 憑證驗證、SSL/TLS、安全標頭
- ✅ **可擴展性**: 背景作業處理、排程任務
- ✅ **法規遵循**: 政府資安標準、稽核日誌
- ✅ **系統整合**: 串接多個政府系統

### trunk (EECOnline) 系統
- ✅ **雙租戶系統**: 內部政府使用者 + 外部醫療機構
- ✅ **嚴格稽核**: 交易日誌記錄
- ✅ **系統整合**: e-Training平台、醫院API
- ✅ **批次處理**: 帳務處理與資料同步
- ✅ **中文介面**: 繁體中文
- ✅ **政府醫療系統**: 服務退輔會等單位

---

## 開發環境設定 | Development Setup

### 系統需求 | Prerequisites
- Visual Studio 2019/2022
- .NET Framework 4.5.2+ / 4.8
- SQL Server 2016+
- IIS 8.0+

### 資料庫設定 | Database Configuration

#### e-service
```
Server: 127.0.0.1,1433 (Docker SQL Server 2022)
Database: eservice_new
Connection String Key: DefaultConnection
```

#### trunk (EECOnline)
```
Server: 172.16.25.38
Database: EEC_PD_DB
Connection String Key: connectionEUSERVICE
```

### 建置步驟 | Build Steps

1. **還原NuGet套件**
   ```bash
   nuget restore e-service/ES.sln
   nuget restore trunk/EECOnline.csproj
   ```

2. **更新連線字串**
   - e-service: 編輯 `ES/Web.config`
   - trunk: 編輯 `Web.config` 和 `SqlMap.config`

3. **編譯專案**
   ```bash
   msbuild e-service/ES.sln /p:Configuration=Release
   msbuild trunk/EECOnline.csproj /p:Configuration=Release
   ```

4. **部署至IIS**
   - e-service: 綁定至 HTTPS (需SSL憑證)
   - trunk: 設定應用程式集區 (.NET 4.x)

---

## Docker支援 | Docker Support

### SQL Server容器 (e-service)
```bash
docker-compose up -d
```

參見: `docker-compose.yml`

---

## 安全性注意事項 | Security Notes

### e-service
- 🔒 強制HTTPS (HSTS)
- 🔒 XSS防護標頭
- 🔒 CSRF防護 (AntiForgeryToken)
- 🔒 內容安全政策
- 🔒 密碼使用BouncyCastle加密
- 🔒 JWT令牌驗證

### trunk (EECOnline)
- 🔒 X-Frame-Options: SAMEORIGIN
- 🔒 安全Cookie (requireSSL)
- 🔒 RSACSP密碼加密
- 🔒 角色型存取控制 (RBAC)
- 🔒 交易稽核日誌

---

## 日誌與監控 | Logging & Monitoring

### e-service
- **log4net** 設定
- 日誌路徑: `C:\e-service\Logs\`
  - Web/ - 網頁日誌
  - ScheduleMdod/ - 醫審系統日誌
  - ScheduleSystem/ - 系統任務日誌
  - ScheduleCert/ - 憑證日誌
- 每日滾動檔案 (yyyyMMdd格式)

### trunk (EECOnline)
- **log4net.config** 設定
- Hangfire儀表板: `/hangfire`
- IP限制存取控制
- 資料庫交易日誌 (TblVISIT_RECORD)

---

## 背景作業管理 | Background Jobs

兩系統皆使用 **Hangfire** 進行背景作業排程:

### e-service
- 訪問: `https://{domain}/hangfire`
- IP白名單保護

### trunk (EECOnline)
- 訪問: `https://{domain}/hangfire`
- IP限制: 127.0.0.1, 192.168.*, 172.*
- 儀表板驗證過濾器

---

## 文檔資源 | Documentation

### 技術文件位置
```
documents/documents backup/
├── 109-e-service/                    # e-service技術文件
│   ├── 技術文件-AugementCode/       # 詳細技術規格
│   └── 技術文件-ClaudeCode/         # 系統架構說明
├── MITV2 sample document/            # 範例文件
├── temp/CodeBase/技術文件/          # 程式碼庫比較文件
└── SQL-SERVER-SETUP.md               # 資料庫安裝指南
```

### 主要技術文件
- **01_專案高階概述.md** - 專案概覽
- **02-11_全端功能詳細範例** - 各模組詳細說明
- **資料庫架構與CRUD操作詳細範例.md** - 資料存取模式
- **身份驗證與授權機制詳細範例.md** - 安全架構
- **OWASP防護機制與驗證.md** - 資安實作

---

## 相關連結 | Related Links

- **我的E政府**: https://www.gov.tw/
- **衛生福利部**: https://www.mohw.gov.tw/
- **Hangfire文件**: https://www.hangfire.io/

---

## 授權聲明 | License

本專案為台灣衛生福利部所有，僅供授權開發與維護使用。

**未經授權不得複製、修改或散布。**

---

## 維護資訊 | Maintenance

### 聯絡資訊
- 專案負責: 衛生福利部資訊處
- 技術支援: [技術支援團隊]

### 最後更新
- 文件更新日期: 2025-12-04
- e-service版本: 基於 .NET 4.8
- trunk版本: 基於 .NET 4.5.2

---

**注意**: 本README為技術文件，包含敏感系統資訊，請妥善保管。
