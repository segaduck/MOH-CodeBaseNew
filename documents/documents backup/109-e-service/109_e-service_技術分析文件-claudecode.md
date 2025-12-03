# 109 e-service 系統技術分析文件

**專案名稱**: 衛生福利部線上申辦服務平台 (e-service)
**分析日期**: 2025年10月8日
**文件版本**: v1.0

---

## 目錄

1. [系統概述](#1-系統概述)
2. [專案架構分析](#2-專案架構分析)
3. [認證與授權機制](#3-認證與授權機制)
4. [資料庫結構](#4-資料庫結構)
5. [業務邏輯分析](#5-業務邏輯分析)
6. [程式碼範例](#6-程式碼範例)
7. [安全機制](#7-安全機制)
8. [系統特色與建議](#8-系統特色與建議)

---

## 1. 系統概述

### 1.1 專案背景

本系統為衛生福利部提供民眾線上申辦各項業務的電子化服務平台,整合多個司別的申辦業務,提供便捷的線上申請、審核、繳費等功能。

### 1.2 使用者對象

- **民眾**: 透過自然人憑證、工商憑證、醫事人員憑證等方式登入,申辦各項業務
- **後台管理人員**: 衛福部各司別承辦人員,負責案件審核、管理
- **系統管理員**: 負責系統設定、帳號管理、權限管理

### 1.3 主要功能

- 線上申辦各項衛生福利業務 (40+ 種申辦項目)
- 案件審核與流程管理
- 線上繳費 (整合政府支付平台)
- 公文管理與通知
- 統計報表查詢
- 帳號權限管理

---

## 2. 專案架構分析

### 2.1 技術堆疊

#### 核心框架

| 技術項目 | 版本 | 說明 |
|---------|------|------|
| **開發框架** | ASP.NET MVC 4.0 | Microsoft 網頁應用程式框架 |
| **.NET Framework** | 4.5.2 | 應用程式執行環境 |
| **視圖引擎** | Razor 2.0 | 動態網頁生成引擎 |
| **ORM 框架** | Entity Framework 5.0 | 物件關聯對應框架 |
| **資料查詢** | Dapper 1.60.6 | 輕量級 ORM,用於高效能查詢 |

#### 主要第三方套件

| 套件名稱 | 版本 | 用途 |
|---------|------|------|
| **log4net** | 2.0.0 | 日誌記錄與管理 |
| **Newtonsoft.Json** | 10.0.3 | JSON 序列化與反序列化 |
| **NPOI** | 2.5.1 | Excel 檔案讀寫處理 |
| **iTextSharp** | 5.4.4 | PDF 文件生成與處理 |
| **DotNetZip** | 1.9.1.8 | 壓縮檔案處理 |
| **Hangfire** | 1.7.19 | 背景任務排程系統 |
| **DocX** | 1.7.0 | Word 文件處理 |
| **jose-jwt** | 3.0.0 | JWT Token 處理 |
| **AntiXSS** | 4.3.0 | 跨站腳本攻擊防護 |
| **Bootstrap** | 5.3.5 | 前端 UI 框架 |
| **jQuery** | 2.1.3 | JavaScript 函式庫 |
| **Font Awesome** | 5.6.3 | 圖示字型庫 |

### 2.2 專案結構

```
ES (主專案根目錄)
│
├── Controllers/                 # 前台控制器 (民眾使用)
│   ├── BaseController.cs       # 需登入的基底控制器
│   ├── BaseNoMemberController.cs  # 免登入的基底控制器
│   ├── LoginController.cs      # 登入處理控制器
│   ├── Apply_001xxx.cs        # 醫事司申辦控制器
│   ├── Apply_005xxx.cs        # 中醫藥司申辦控制器
│   ├── Apply_010xxx.cs        # 社工司申辦控制器
│   └── ...                     # 其他業務控制器
│
├── Areas/                       # MVC Areas 區域劃分
│   ├── BACKMIN/                # 後台管理區 (審核人員使用)
│   │   ├── Controllers/        # 後台控制器
│   │   ├── Action/             # 業務邏輯層
│   │   └── Views/              # 後台視圖
│   ├── Admin/                  # 管理區
│   ├── Resource/               # 資源管理區
│   └── Schedule/               # 排程任務區
│
├── Models/                      # 資料模型
│   ├── Entities/               # 資料庫實體類別 (150+ 個資料表對應)
│   ├── ViewModels/             # 視圖模型
│   ├── SessionModel.cs         # Session 管理模型
│   └── LoginUserInfo.cs        # 登入使用者資訊
│
├── DataLayers/                  # 資料存取層 (DAO)
│   ├── LoginDAO.cs             # 登入相關資料存取
│   ├── ApplyDAO.cs             # 申辦案件資料存取
│   └── ...                     # 其他資料存取物件
│
├── Services/                    # 服務層 (Business Logic)
│   ├── ClamService.cs          # 登入驗證服務
│   └── CommonsServices.cs      # 共用服務
│
├── Utils/                       # 工具類別
│   ├── DataUtils.cs            # 資料處理工具
│   ├── CheckUtils.cs           # 驗證工具
│   └── LogUtils.cs             # 日誌工具
│
├── Views/                       # Razor 視圖檔案
├── Content/                     # 靜態資源 (CSS, 圖片)
├── Scripts/                     # JavaScript 檔案
├── Web.config                   # 主要設定檔
└── Global.asax.cs               # 應用程式啟動設定

```

### 2.3 系統架構圖

```
┌─────────────────────────────────────────────────────────┐
│                      使用者介面層                          │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│   │  民眾前台     │  │  後台管理     │  │  系統管理     │  │
│   │  (前台)      │  │  (BACKMIN)   │  │  (Admin)     │  │
│   └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                    控制器層 (Controllers)                 │
│   ┌──────────────────────────────────────────────────┐  │
│   │  BaseController (權限驗證、Session 管理)          │  │
│   └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                  業務邏輯層 (Services/Action)             │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│   │  認證服務     │  │  申辦服務     │  │  共用服務     │  │
│   └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                  資料存取層 (DataLayers/DAO)              │
│   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│   │  LoginDAO    │  │  ApplyDAO    │  │  其他 DAO     │  │
│   └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                    資料庫層 (SQL Server)                  │
│   ┌──────────────────────────────────────────────────┐  │
│   │  e-service Database (主資料庫)                    │  │
│   │  - 會員資料、申辦案件、系統設定...               │  │
│   └──────────────────────────────────────────────────┘  │
│   ┌──────────────────────────────────────────────────┐  │
│   │  MDOD Database (外部資料庫)                       │  │
│   │  - 醫事人員資料、醫療機構資料                     │  │
│   └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## 3. 認證與授權機制

### 3.1 多元登入方式

本系統支援五種登入方式,以符合不同使用者的需求:

| 登入方式 | 代碼 | 使用對象 | 驗證機制 |
|---------|------|---------|---------|
| **帳號密碼登入** | MEMBER | 一般民眾 | 帳號 + 密碼 (SHA256 加密) |
| **自然人憑證** | MOICA | 個人 | 內政部自然人憑證 |
| **工商憑證** | MOEACA | 公司行號 | 經濟部工商憑證 |
| **醫事人員憑證** | HCA1 | 醫事人員 | 衛福部醫事人員憑證 |
| **數位身分證** | NEWEID | 個人 | 數位身分識別證 |

### 3.2 登入流程

#### 3.2.1 登入流程圖

```
開始
  ↓
使用者選擇登入方式
  ↓
輸入登入資訊 (帳密/憑證)
  ↓
輸入圖形驗證碼
  ↓
提交登入請求
  ↓
━━━━━━━━━━━━━━━━━━━━━━━━━━━
         系統驗證
━━━━━━━━━━━━━━━━━━━━━━━━━━━
  ↓
檢查驗證碼是否正確?
  │
  ├─ 否 → 顯示錯誤訊息 → 返回登入頁
  │
  └─ 是
      ↓
    查詢帳號是否存在?
      │
      ├─ 否 → 顯示「帳號不存在」→ 返回登入頁
      │
      └─ 是
          ↓
        檢查是否已被鎖定? (5次錯誤)
          │
          ├─ 是 → 檢查鎖定時間是否超過15分鐘?
          │        │
          │        ├─ 否 → 顯示「帳號已鎖定」→ 返回登入頁
          │        └─ 是 → 解除鎖定 → 繼續驗證
          │
          └─ 否
              ↓
            密碼/憑證驗證
              │
              ├─ 失敗 → 錯誤次數 +1 → 記錄登入失敗 LOG → 返回登入頁
              │
              └─ 成功
                  ↓
                載入使用者資訊至 Session
                  ↓
                重置錯誤次數為 0
                  ↓
                記錄登入成功 LOG
                  ↓
                導向首頁或原始請求頁面
                  ↓
                結束
```

#### 3.2.2 登入失敗鎖定機制

為防止暴力破解攻擊,系統實作以下保護機制:

- **失敗次數上限**: 連續 5 次密碼錯誤
- **鎖定時間**: 15 分鐘
- **記錄資料表**: `LOGIN_LOG`
  - `STATUS='S'`: 登入成功
  - `STATUS='A'`: 登入失敗
  - `FAIL_COUNT`: 連續失敗次數
  - `FAIL_TOTAL`: 累計失敗次數

### 3.3 Session 管理

#### 3.3.1 Session 設定

```xml
<!-- Web.config 設定 -->
<sessionState mode="StateServer"
              stateConnectionString="tcpip=127.0.0.1:42424"
              cookieless="false"
              timeout="30">
</sessionState>
```

**設定說明**:
- **mode**: `StateServer` (分散式 Session,支援負載平衡)
- **timeout**: 30 分鐘 (閒置逾時)
- **cookieless**: `false` (使用 Cookie 儲存 Session ID)

#### 3.3.2 登入逾時設定

```xml
<!-- 一般會員登入後逾時時間 (分鐘) -->
<add key="MemberLoginTimeout" value="60" />

<!-- 管理者登入後逾時時間 (分鐘) -->
<add key="AdminLoginTimeout" value="640" />
```

#### 3.3.3 Session 資料結構

**SessionModel 主要欄位**:

| 欄位名稱 | 型態 | 說明 |
|---------|------|------|
| `LoginValidateCode` | string | 登入驗證碼 (圖形驗證碼) |
| `UserInfo` | LoginUserInfo | 登入使用者完整資訊 |
| `RoleFuncs` | IList&lt;ClamRoleFunc&gt; | 使用者可存取的功能清單 |
| `LastActionFunc` | TblAMFUNCM | 最後執行的功能項目 |
| `LastErrorMessage` | string | 錯誤訊息 (自動清除機制) |
| `LastResultMessage` | string | 操作結果訊息 (自動清除機制) |

**LoginUserInfo 主要欄位**:

| 欄位名稱 | 型態 | 說明 |
|---------|------|------|
| `UserNo` | string | 帳號 |
| `Member` | ClamMember | 會員基本資料 |
| `Admin` | ClamAdmin | 管理員基本資料 (後台使用) |
| `LoginAuth` | string | 登入方式 (MEMBER/MOICA/MOEACA/HCA1/NEWEID) |
| `LoginIP` | string | 登入 IP 位址 |
| `LoginSuccess` | bool | 登入是否成功 |
| `LoginErrMessage` | string | 登入錯誤訊息 |

### 3.4 權限控管機制

#### 3.4.1 BaseController 權限檢核

所有需要登入的功能都繼承自 `BaseController`,在 `OnActionExecuting` 事件中自動檢核權限:

```csharp
protected override void OnActionExecuting(ActionExecutingContext filterContext)
{
    base.OnActionExecuting(filterContext);

    // 1. 檢查是否標註 AllowAnonymousAttribute (允許匿名存取)
    if (有 AllowAnonymousAttribute)
    {
        return; // 略過登入檢查
    }

    // 2. 檢查 Session 中是否有登入資訊
    SessionModel sm = SessionModel.Get();
    if (sm.UserInfo == null)
    {
        sm.LastErrorMessage = "請先登入帳號";
        filterContext.Result = Redirect("/Login/Index");
        return;
    }

    // 3. 權限檢核 (後台管理功能)
    // 前台申辦功能不需檢核權限
}
```

#### 3.4.2 後台權限管理 (BACKMIN Area)

後台管理系統使用「使用者-群組-功能」三層權限架構:

```
管理員帳號 (ADMIN)
    ↓ 指派至
群組 (AMGRP)
    ↓ 設定
功能權限 (AMGMAPM)
    ↓ 對應至
功能定義 (AMFUNCM)
```

**權限資料表結構**:

| 資料表 | 說明 | 關鍵欄位 |
|--------|------|----------|
| `ADMIN` | 管理員帳號資料表 | `ACC_NO` (帳號), `PSWD` (密碼), `NAME` (姓名) |
| `AMGRP` | 群組資料表 | `grp_id` (群組ID), `grpname` (群組名稱) |
| `AMUROLE` | 管理員-群組對應表 | `ACC_NO` (帳號), `grp_id` (群組ID) |
| `AMFUNCM` | 功能定義資料表 | `prgid` (功能代碼), `prgname` (功能名稱) |
| `AMGMAPM` | 群組-功能權限對應表 | `grp_id`, `prgid`, `prg_i`, `prg_u`, `prg_d`, `prg_q`, `prg_p` |

**權限標記說明**:
- `prg_i = 1`: 允許新增 (Insert)
- `prg_u = 1`: 允許修改 (Update)
- `prg_d = 1`: 允許刪除 (Delete)
- `prg_q = 1`: 允許查詢 (Query)
- `prg_p = 1`: 允許列印 (Print)

---

## 4. 資料庫結構

### 4.1 資料庫連線設定

#### 主要資料庫

```xml
<!-- Web.config -->
<add name="DefaultConnection"
     providerName="System.Data.SqlClient"
     connectionString="Data Source=sql2016.turbotech.com.tw;
                       Initial Catalog=e-service;
                       Persist Security Info=True;
                       User ID=eservice;
                       Password=eservice!QAZ" />
```

**資料庫資訊**:
- **資料庫類型**: Microsoft SQL Server 2016
- **資料庫名稱**: `e-service`
- **伺服器位址**: sql2016.turbotech.com.tw

#### 外部資料庫

```xml
<!-- MDOD 資料庫 (醫事人員資料) -->
<add name="MDODConnection"
     providerName="System.Data.SqlClient"
     connectionString="Data Source=203.65.109.238;
                       Initial Catalog=MDOD;
                       User ID=dohdbmg;
                       Password=doh10769#$" />
```

### 4.2 主要資料表結構

#### 4.2.1 會員管理相關資料表

##### MEMBER (會員資料表)

儲存民眾註冊帳號的基本資料。

| 欄位名稱 | 資料型態 | 說明 | 備註 |
|---------|---------|------|------|
| `ACC_NO` | varchar(50) | 帳號 | PK, 通常為身分證字號或統編 |
| `PSWD` | varchar(256) | 密碼 | SHA256 加密儲存 |
| `IDN` | varchar(20) | 身分證字號/統一編號 | |
| `SEX_CD` | varchar(1) | 性別代碼 | M=男, F=女 |
| `BIRTHDAY` | datetime | 生日 | |
| `NAME` | nvarchar(100) | 中文姓名 | |
| `ENAME` | nvarchar(200) | 英文姓名 | |
| `CNT_NAME` | nvarchar(100) | 聯絡人姓名 | |
| `TEL` | varchar(20) | 電話 | |
| `MOBILE` | varchar(20) | 手機 | |
| `MAIL` | varchar(200) | Email | |
| `CITY_CD` | varchar(10) | 縣市代碼 | |
| `TOWN_CD` | varchar(10) | 鄉鎮代碼 | |
| `ADDR` | nvarchar(500) | 地址 | |
| `CARD_TYPE` | varchar(10) | 憑證類型 | MOICA/MOEACA/HCA1/NEWEID |
| `CARD_INFO` | varchar(500) | 憑證資訊 | JSON 格式 |
| `SERIALNO` | varchar(100) | 憑證序號 | |
| `DEL_MK` | varchar(1) | 刪除註記 | Y=已刪除, N=正常 |
| `CREATE_TIME` | datetime | 建立時間 | |
| `UPDATE_TIME` | datetime | 更新時間 | |

##### LOGIN_LOG (登入記錄表)

記錄所有登入嘗試,包含成功與失敗。

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `LOGIN_ID` | varchar(50) | 登入帳號 |
| `LOGIN_TIME` | datetime | 登入時間 |
| `NAME` | nvarchar(100) | 姓名 |
| `IP_ADDR` | varchar(50) | IP 位址 |
| `STATUS` | varchar(1) | 狀態 (S=成功, A=失敗) |
| `FAIL_COUNT` | int | 連續失敗次數 |
| `FAIL_TOTAL` | int | 累計失敗次數 |
| `REMARK` | nvarchar(500) | 備註 |

#### 4.2.2 後台管理相關資料表

##### ADMIN (管理員資料表)

儲存後台管理人員帳號資料。

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `ACC_NO` | varchar(50) | 帳號 (PK) |
| `PSWD` | varchar(256) | 密碼 (SHA256 加密) |
| `NAME` | nvarchar(100) | 姓名 |
| `UNIT_CD` | varchar(20) | 單位代碼 |
| `LEVEL_NO` | int | 權限等級 |
| `EMAIL` | varchar(200) | Email |
| `TEL` | varchar(20) | 電話 |
| `STATUS` | varchar(1) | 狀態 (1=啟用, 0=停用) |

##### AMGRP (群組資料表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `grp_id` | varchar(20) | 群組ID (PK) |
| `grpname` | nvarchar(100) | 群組名稱 |
| `remark` | nvarchar(500) | 說明 |

##### AMUROLE (管理員-群組對應表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `ACC_NO` | varchar(50) | 管理員帳號 (FK) |
| `grp_id` | varchar(20) | 群組ID (FK) |

##### AMFUNCM (功能定義資料表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `prgid` | varchar(50) | 功能代碼 (PK) |
| `prgname` | nvarchar(100) | 功能名稱 |
| `sysid` | varchar(20) | 系統第一層 |
| `modules` | varchar(20) | 第二層 |
| `submodules` | varchar(20) | 第三層 |
| `prgorder` | int | 排序 |
| `showmenu` | varchar(1) | 是否顯示於選單 (1=是, 0=否) |

##### AMGMAPM (群組-功能權限對應表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `grp_id` | varchar(20) | 群組ID (FK) |
| `prgid` | varchar(50) | 功能代碼 (FK) |
| `prg_i` | varchar(1) | 新增權限 (1=允許, 0=不允許) |
| `prg_u` | varchar(1) | 修改權限 |
| `prg_d` | varchar(1) | 刪除權限 |
| `prg_q` | varchar(1) | 查詢權限 |
| `prg_p` | varchar(1) | 列印權限 |

#### 4.2.3 申辦業務相關資料表

##### APPLY (申辦案件主檔)

儲存所有申辦案件的基本資料。

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `APP_ID` | varchar(50) | 申辦編號 (PK) |
| `SRV_ID` | varchar(20) | 服務編號 (FK) |
| `ACC_NO` | varchar(50) | 申請人帳號 (FK) |
| `STATUS_CD` | varchar(10) | 狀態代碼 |
| `APP_TIME` | datetime | 申辦時間 |
| `APPR_TIME` | datetime | 審核時間 |
| `APPR_USER` | varchar(50) | 審核人員 |
| `APPR_OPINION` | nvarchar(1000) | 審核意見 |
| `PAY_STATUS` | varchar(10) | 繳費狀態 |
| `PAY_AMT` | decimal(18,2) | 繳費金額 |
| `PAY_TIME` | datetime | 繳費時間 |

**狀態代碼說明** (`STATUS_CD`):
- `01`: 待審核
- `02`: 補件中
- `03`: 審核通過
- `04`: 退件
- `05`: 已完成
- `09`: 已取消

##### APPLY_FILE (申辦附件檔案表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `FILE_ID` | varchar(50) | 檔案ID (PK) |
| `APP_ID` | varchar(50) | 申辦編號 (FK) |
| `FILE_TYPE` | varchar(20) | 檔案類型 |
| `FILE_NAME` | nvarchar(200) | 檔案名稱 |
| `FILE_PATH` | varchar(500) | 檔案路徑 |
| `FILE_SIZE` | bigint | 檔案大小 (Bytes) |
| `UPLOAD_TIME` | datetime | 上傳時間 |

#### 4.2.4 系統設定相關資料表

##### SETUP (系統參數設定表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `setup_cd` | varchar(50) | 設定代碼 (PK) |
| `setup_val` | nvarchar(1000) | 設定值 |
| `setup_memo` | nvarchar(500) | 說明 |

##### CODE (系統代碼表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `code_type` | varchar(20) | 代碼類別 |
| `code_no` | varchar(20) | 代碼 |
| `code_name` | nvarchar(200) | 名稱 |
| `code_order` | int | 排序 |

##### ZIPCODE (郵遞區號資料表)

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `zip_code` | varchar(10) | 郵遞區號 (PK) |
| `city` | nvarchar(50) | 縣市 |
| `area` | nvarchar(50) | 鄉鎮市區 |

#### 4.2.5 系統記錄相關資料表

##### SYS_TRANS_LOG (系統交易記錄表)

記錄所有資料異動軌跡。

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `LOG_ID` | bigint | 記錄ID (PK, Identity) |
| `TABLE_NAME` | varchar(100) | 資料表名稱 |
| `PK_VALUE` | varchar(200) | 主鍵值 |
| `ACTION_TYPE` | varchar(10) | 異動類型 (INSERT/UPDATE/DELETE) |
| `BEFORE_VALUE` | nvarchar(max) | 異動前值 (JSON) |
| `AFTER_VALUE` | nvarchar(max) | 異動後值 (JSON) |
| `MOD_USER` | varchar(50) | 異動人員 |
| `MOD_TIME` | datetime | 異動時間 |
| `MOD_IP` | varchar(50) | 異動IP |

##### VISIT_RECORD (操作記錄表)

記錄使用者操作紀錄。

| 欄位名稱 | 資料型態 | 說明 |
|---------|---------|------|
| `RECORD_ID` | bigint | 記錄ID (PK, Identity) |
| `ACC_NO` | varchar(50) | 操作帳號 |
| `CONTROL_NAME` | varchar(100) | 控制器名稱 |
| `ACTION_NAME` | varchar(100) | 動作名稱 |
| `APP_NAME` | varchar(200) | 功能名稱 |
| `TAG` | double | 時間戳記 |
| `IS_EXPIRED` | bit | 是否過期 |

### 4.3 資料庫 ER 關係圖

```
┌─────────────┐          ┌─────────────┐
│   MEMBER    │──────────│ LOGIN_LOG   │
│  (會員資料)  │ 1      * │  (登入記錄)  │
└─────────────┘          └─────────────┘
       │ 1
       │
       │ *
┌─────────────┐          ┌─────────────┐
│    APPLY    │──────────│ APPLY_FILE  │
│ (申辦案件)   │ 1      * │ (申辦附件)   │
└─────────────┘          └─────────────┘


┌─────────────┐          ┌─────────────┐          ┌─────────────┐
│    ADMIN    │──────────│  AMUROLE    │──────────│   AMGRP     │
│ (管理員帳號) │ 1      * │(帳號-群組)  │ *      1 │   (群組)     │
└─────────────┘          └─────────────┘          └─────────────┘
                                                          │ 1
                                                          │
                                                          │ *
                         ┌─────────────┐          ┌─────────────┐
                         │  AMFUNCM    │──────────│  AMGMAPM    │
                         │ (功能定義)   │ 1      * │(群組-功能)  │
                         └─────────────┘          └─────────────┘
```

---

## 5. 業務邏輯分析

### 5.1 前台申辦功能模組 (民眾使用)

#### 5.1.1 醫事司申辦 (Apply_001xxx)

| 控制器 | 申辦項目名稱 | 說明 |
|-------|------------|------|
| `Apply_001005Controller` | 醫師執業登記 | 醫師執業登記、變更、歇業 |
| `Apply_001007Controller` | 護理人員執業登記 | 護理師/護士執業登記 |
| `Apply_001008Controller` | 醫療機構設立許可 | 醫院/診所設立許可申請 |
| `Apply_001009Controller` | 醫療廣告申請 | 醫療廣告內容申請審核 |
| `Apply_001010Controller` | 診所設立許可 | 診所開業許可申請 |
| `Apply_001034Controller` | 醫事人員證照補發 | 證照遺失補發申請 |
| `Apply_001035Controller` | 醫療器材許可證 | 醫療器材許可證申請 |
| `Apply_001036Controller` | 藥商許可執照 | 藥商設立許可執照申請 |
| `Apply_001037Controller` | 藥品查驗登記 | 新藥查驗登記申請 |
| `Apply_001038Controller` | 化粧品製造工廠登記 | 化粧品工廠設立登記 |
| `Apply_001039Controller` | 食品業者登錄 | 食品業者登錄申請 |

#### 5.1.2 中醫藥司申辦 (Apply_005xxx)

| 控制器 | 申辦項目名稱 | 說明 |
|-------|------------|------|
| `Apply_005001Controller` | 中藥商設立許可 | 中藥商設立許可申請 |
| `Apply_005002Controller` | 中藥製造許可 | 中藥製造許可證申請 |
| `Apply_005003Controller` | 中藥輸入許可 | 中藥輸入許可證申請 |
| `Apply_005004Controller` | 中藥查驗登記 | 中藥新藥查驗登記 |
| `Apply_005005Controller` | 中藥藥品許可證 | 中藥製劑許可證申請 |
| `Apply_005013Controller` | 中醫診所開業 | 中醫診所開業許可 |

#### 5.1.3 社工司申辦 (Apply_010xxx, 011xxx)

| 控制器 | 申辦項目名稱 | 說明 |
|-------|------------|------|
| `Apply_010001Controller` | 社工師執業登記 | 社工師執業登記申請 |
| `Apply_010002Controller` | 社工人員證照 | 社工人員證照申請 |
| `Apply_011001Controller` | 低收入戶證明 | 低收入戶證明申請 |
| `Apply_011002Controller` | 身心障礙證明 | 身心障礙證明申請 |
| `Apply_011003Controller` | 老人福利補助 | 老人福利生活津貼 |
| `Apply_011004Controller` | 育兒津貼 | 育兒津貼補助申請 |
| `Apply_011005Controller` | 托育補助 | 托育服務費用補助 |
| `Apply_011006Controller` | 特殊境遇家庭扶助 | 特殊境遇家庭扶助金 |
| `Apply_011007Controller` | 身心障礙者日間照顧費用補助 | 日間照顧及住宿式照顧費用補助 |
| `Apply_011008Controller` | 身心障礙者生活補助 | 身心障礙者生活補助費 |
| `Apply_011009Controller` | 弱勢兒童及少年生活扶助 | 兒少生活扶助申請 |
| `Apply_011010Controller` | 特殊教育學生獎助學金 | 身心障礙學生獎助學金 |

#### 5.1.4 其他申辦功能

| 控制器 | 功能名稱 | 說明 |
|-------|---------|------|
| `ApplyDonateController` | 線上捐款 | 線上捐款功能,支援信用卡付款 |
| `FlyPayController` | 防疫旅館系統 | 防疫旅館入住與付款管理 |
| `CosmeticAdvertisingController` | 化粧品廣告申請 | 化粧品廣告內容審核 |
| `CosmeticIngredientsController` | 化粧品成分查詢 | 化粧品禁用成分查詢 |

### 5.2 後台管理功能模組 (BACKMIN Area)

#### 5.2.1 案件管理功能

| 控制器 | 功能名稱 | 說明 |
|-------|---------|------|
| `RecordController` | 案件查詢 | 查詢所有申辦案件 |
| `AssignController` | 案件分派 | 將案件分派給審核人員 |
| `Apply_*Controller` | 案件審核 | 對應各項申辦的審核功能 (40+ 個) |
| `MessageController` | 案件通知 | 寄送補件通知、審核結果通知 |
| `ExportController` | 案件匯出 | 匯出案件資料為 Excel |
| `DocumentExportController` | 公文匯出 | 匯出公文格式文件 |

#### 5.2.2 服務管理功能

| 控制器 | 功能名稱 | 說明 |
|-------|---------|------|
| `ServiceController` | 服務項目管理 | 新增/修改/刪除申辦服務項目 |
| `ServiceDateController` | 服務開放時間 | 設定服務開放/關閉時間 |
| `ServiceFormController` | 服務表單管理 | 管理申辦表單欄位 |
| `ServiceFileController` | 服務附件管理 | 管理應繳附件類型 |
| `ServiceRuleController` | 服務規則管理 | 設定申辦規則與限制 |
| `ServiceNoticeController` | 服務公告管理 | 管理服務相關公告 |
| `ServiceHelpController` | 服務說明管理 | 管理線上說明文件 |
| `ServiceAgreementController` | 服務條款管理 | 管理使用條款與隱私權聲明 |
| `ServiceNormController` | 服務規範管理 | 管理申辦規範文件 |

#### 5.2.3 繳費管理功能

| 控制器 | 功能名稱 | 說明 |
|-------|---------|------|
| `PayController` | 繳費管理 | 查詢繳費記錄、對帳 |
| `PayECController` | 電子支付管理 | 整合政府支付平台 |
| `FlyPayController` | 防疫旅館繳費 | 防疫旅館線上繳費 |
| `FlySwipeController` | 刷卡記錄 | 信用卡刷卡記錄查詢 |

#### 5.2.4 系統管理功能

| 控制器 | 功能名稱 | 說明 |
|-------|---------|------|
| `AccountController` | 帳號管理 | 管理後台管理員帳號 |
| `UnitController` | 單位管理 | 管理組織單位資料 |
| `SiteController` | 網站管理 | 網站參數設定 |
| `SetupController` | 系統設定 | 系統參數設定 |
| `QAController` | 問答管理 | 常見問題管理 |
| `MessageBackController` | 訊息管理 | 系統訊息管理 |

### 5.3 申辦流程範例

#### 5.3.1 醫師執業登記流程 (Apply_001005)

```
民眾端流程:
1. 登入系統 (憑證或帳密)
   ↓
2. 選擇「醫師執業登記」
   ↓
3. 填寫基本資料
   - 醫師姓名
   - 身分證字號
   - 醫師證書字號
   - 執業機構
   - 執業科別
   ↓
4. 上傳應繳附件
   - 醫師證書影本
   - 最近三個月內照片
   - 執業機構證明文件
   ↓
5. 確認送出
   ↓
6. 線上繳費 (政府支付平台)
   ↓
7. 取得申辦編號

━━━━━━━━━━━━━━━━━━━━━━━━━━━

後台審核流程:
1. 案件自動進入「待審核」狀態
   ↓
2. 主管分派案件給審核人員
   ↓
3. 審核人員登入後台
   ↓
4. 查看案件資料與附件
   ↓
5. 審核結果判定:

   ├─ 資料不齊 → 通知補件 → 民眾補件 → 重新審核
   │
   ├─ 不符規定 → 退件 → 寄送退件通知
   │
   └─ 審核通過 → 核發執業執照 → 寄送核准通知
```

#### 5.3.2 申辦案件狀態轉換圖

```
           ┌────────────┐
           │   已送出    │
           │ (STATUS=01) │
           └────────────┘
                  ↓
           ┌────────────┐
           │  待審核     │
           │ (STATUS=01) │
           └────────────┘
                  ↓
        ┌─────────┴─────────┐
        ↓                   ↓
┌────────────┐      ┌────────────┐
│   補件中    │      │  審核中     │
│(STATUS=02)  │      │(STATUS=01)  │
└────────────┘      └────────────┘
        ↓                   ↓
        └─────────┬─────────┘
                  ↓
        ┌─────────┴─────────┐
        ↓                   ↓
┌────────────┐      ┌────────────┐
│  審核通過   │      │    退件     │
│(STATUS=03)  │      │(STATUS=04)  │
└────────────┘      └────────────┘
        ↓
┌────────────┐
│   已完成    │
│(STATUS=05)  │
└────────────┘
```

---

## 6. 程式碼範例

### 6.1 登入驗證範例

#### 6.1.1 LoginController - 處理登入請求

**檔案位置**: `ES\Controllers\LoginController.cs`

```csharp
[HttpPost]
public ActionResult UserLogin(LoginFormModel form)
{
    var result = new AjaxResultStruct();

    try
    {
        string userId = form.UserNo;
        string userPwd_encry = DataUtils.Crypt256(form.UserPwd); // SHA256 加密

        // 驗證碼檢查
        SessionModel sm = SessionModel.Get();
        if (!form.ValidateCode.Equals(sm.LoginValidateCode))
        {
            result.message = "驗證碼輸入錯誤";
            return Content(result.Serialize(), "application/json");
        }

        // 檢查登入失敗鎖定
        LoginDAO dao = new LoginDAO();
        var loginlog = dao.GetLastLog(form.UserNo);
        if (loginlog != null && loginlog.STATUS == "A")
        {
            if (loginlog.FAIL_COUNT >= 5 &&
                DateTime.Now < loginlog.LOGIN_TIME?.AddMinutes(15))
            {
                throw new LoginExceptions("您的帳號因密碼錯誤超過5次已被鎖定15分鐘");
            }
        }

        // 登入驗證
        LoginUserInfo userInfo = dao.LoginValidate(userId, userPwd_encry);
        userInfo.LoginAuth = "MEMBER";
        userInfo.LoginIP = HttpContext.Request.UserHostAddress;

        if (!userInfo.LoginSuccess)
        {
            throw new LoginExceptions(userInfo.LoginErrMessage);
        }

        // 保存 Session
        sm.UserInfo = userInfo;

        // 記錄成功 LOG
        TblLOGIN_LOG llog = new TblLOGIN_LOG();
        llog.LOGIN_ID = userInfo.UserNo;
        llog.LOGIN_TIME = DateTime.Now;
        llog.NAME = userInfo.Member.NAME;
        llog.IP_ADDR = userInfo.LoginIP;
        llog.STATUS = "S";
        llog.FAIL_COUNT = 0;
        dao.Insert(llog);

        result.status = true;
        result.message = userInfo.Member.NAME + " 登入成功";

        return Content(result.Serialize(), "application/json");
    }
    catch (LoginExceptions ex)
    {
        result.status = false;
        result.message = ex.Message;
        return Content(result.Serialize(), "application/json");
    }
}
```

#### 6.1.2 LoginDAO - 登入驗證邏輯

**檔案位置**: `ES\DataLayers\LoginDAO.cs`

```csharp
public LoginUserInfo LoginValidate(string userNo, string userPwd_encry)
{
    LoginUserInfo userInfo = new LoginUserInfo();
    userInfo.UserNo = userNo;
    userInfo.LoginSuccess = false;

    var dictionary = new Dictionary<string, object>
    {
        { "@ACC_NO", userNo },
        { "@PSWD", userPwd_encry }
    };
    var parameters = new DynamicParameters(dictionary);

    // 使用 Dapper 查詢會員資料
    string _sql = @"SELECT * FROM MEMBER
                    WHERE 1=1 AND ISNULL(DEL_MK,'N')='N'
                    AND ACC_NO = @ACC_NO";

    if (!string.IsNullOrEmpty(userPwd_encry))
    {
        _sql += " AND PSWD = @PSWD";
    }

    using (SqlConnection conn = DataUtils.GetConnection())
    {
        TblMEMBER result = conn.QueryFirst<TblMEMBER>(_sql, parameters);

        if (result == null)
        {
            userInfo.LoginErrMessage = "帳號或密碼錯誤";

            // 記錄失敗 LOG
            RecordFailedLogin(userNo);

            return userInfo;
        }

        // 使用 ValueInjecter 進行物件映射
        ClamMember mb = new ClamMember();
        mb.InjectFrom(result);

        userInfo.LoginSuccess = true;
        userInfo.Member = mb;
    }

    return userInfo;
}

// 記錄登入失敗
private void RecordFailedLogin(string userNo)
{
    TblLOGIN_LOG llog = new TblLOGIN_LOG();
    llog.LOGIN_ID = userNo;
    llog.LOGIN_TIME = DateTime.Now;
    llog.STATUS = "A"; // 失敗
    llog.FAIL_COUNT = GetFailCount(userNo) + 1;

    Insert(llog);
}
```

#### 6.1.3 密碼加密處理

**檔案位置**: `ES\Utils\DataUtils.cs`

```csharp
/// <summary>
/// SHA256 加密
/// </summary>
public static string Crypt256(string originText)
{
    if (string.IsNullOrEmpty(originText)) return "";

    using (SHA256 sha256Hash = SHA256.Create())
    {
        // 將字串轉換為 byte array
        byte[] data = sha256Hash.ComputeHash(
            Encoding.UTF8.GetBytes(originText)
        );

        // 將 byte array 轉換為 16 進位字串
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString().ToUpper();
    }
}
```

### 6.2 資料查詢與分頁範例

#### 6.2.1 Controller - 查詢案件資料

**檔案位置**: `ES\Areas\BACKMIN\Controllers\RecordController.cs`

```csharp
public class RecordController : BaseController
{
    /// <summary>
    /// 顯示查詢頁面
    /// </summary>
    [HttpGet]
    public ActionResult Index()
    {
        RecordFormModel form = new RecordFormModel();

        // 預設查詢條件: 最近一個月
        form.APP_TIME_S = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd");
        form.APP_TIME_E = DateTime.Now.ToString("yyyy/MM/dd");

        return View(form);
    }

    /// <summary>
    /// 執行查詢 (POST)
    /// </summary>
    [HttpPost]
    public ActionResult Index(RecordFormModel form)
    {
        using (SqlConnection conn = GetConnection())
        {
            conn.Open();

            RecordAction action = new RecordAction(conn);

            // 設定查詢條件
            Hashtable parms = new Hashtable();
            if (!string.IsNullOrEmpty(form.APP_ID))
                parms["APP_ID"] = form.APP_ID;
            if (!string.IsNullOrEmpty(form.SRV_ID))
                parms["SRV_ID"] = form.SRV_ID;
            if (!string.IsNullOrEmpty(form.STATUS_CD))
                parms["STATUS_CD"] = form.STATUS_CD;
            if (!string.IsNullOrEmpty(form.APP_TIME_S))
                parms["APP_TIME_S"] = form.APP_TIME_S.Replace("/", "");
            if (!string.IsNullOrEmpty(form.APP_TIME_E))
                parms["APP_TIME_E"] = form.APP_TIME_E.Replace("/", "");

            // 設定分頁資訊
            action.SetPageInfo(form.rid, form.p);

            // 執行查詢
            var gridList = action.QueryApplyList(parms);

            form.grid = gridList;

            // 設定分頁參數
            SetPagingParams(form, action, "Index");

            conn.Close();
        }

        return View(form);
    }

    /// <summary>
    /// AJAX 分頁載入
    /// </summary>
    [HttpPost]
    public ActionResult LoadPage(RecordFormModel form)
    {
        // 從 Session 載入上次查詢條件
        if (!LoadCachedFormModel(form))
        {
            return RedirectToAction("Index");
        }

        using (SqlConnection conn = GetConnection())
        {
            conn.Open();

            RecordAction action = new RecordAction(conn);
            action.SetPageInfo(form.rid, form.p);

            // 重新執行查詢
            Hashtable parms = BuildQueryParams(form);
            var gridList = action.QueryApplyList(parms);

            form.grid = gridList;
            SetPagingParams(form, action, "Index");

            conn.Close();
        }

        // 返回部分視圖 (Grid)
        return PartialView("_GridPartial", form);
    }
}
```

#### 6.2.2 Action - 業務邏輯處理

**檔案位置**: `ES\Areas\BACKMIN\Action\RecordAction.cs`

```csharp
public class RecordAction : BaseAction
{
    public RecordAction(SqlConnection conn) : base(conn) { }

    /// <summary>
    /// 查詢申辦案件清單 (含分頁)
    /// </summary>
    public IList<ApplyGridModel> QueryApplyList(Hashtable parms)
    {
        // 使用 Dapper 執行分頁查詢
        string sql = @"
            SELECT
                a.APP_ID,
                a.SRV_ID,
                s.SRV_NAME,
                a.ACC_NO,
                m.NAME AS APPLY_NAME,
                a.STATUS_CD,
                c.code_name AS STATUS_NAME,
                a.APP_TIME,
                a.APPR_TIME,
                a.APPR_USER,
                a.PAY_STATUS,
                a.PAY_AMT
            FROM APPLY a
            LEFT JOIN SERVICE s ON s.SRV_ID = a.SRV_ID
            LEFT JOIN MEMBER m ON m.ACC_NO = a.ACC_NO
            LEFT JOIN CODE c ON c.code_type = 'STATUS' AND c.code_no = a.STATUS_CD
            WHERE 1=1
        ";

        var parameters = new DynamicParameters();

        if (parms.ContainsKey("APP_ID"))
        {
            sql += " AND a.APP_ID = @APP_ID";
            parameters.Add("APP_ID", parms["APP_ID"]);
        }
        if (parms.ContainsKey("SRV_ID"))
        {
            sql += " AND a.SRV_ID = @SRV_ID";
            parameters.Add("SRV_ID", parms["SRV_ID"]);
        }
        if (parms.ContainsKey("STATUS_CD"))
        {
            sql += " AND a.STATUS_CD = @STATUS_CD";
            parameters.Add("STATUS_CD", parms["STATUS_CD"]);
        }
        if (parms.ContainsKey("APP_TIME_S"))
        {
            sql += " AND a.APP_TIME >= @APP_TIME_S";
            parameters.Add("APP_TIME_S", parms["APP_TIME_S"]);
        }
        if (parms.ContainsKey("APP_TIME_E"))
        {
            sql += " AND a.APP_TIME <= @APP_TIME_E + '235959'";
            parameters.Add("APP_TIME_E", parms["APP_TIME_E"]);
        }

        sql += " ORDER BY a.APP_TIME DESC, a.APP_ID DESC";

        // 執行分頁查詢
        return QueryForList<ApplyGridModel>(sql, parameters);
    }
}
```

#### 6.2.3 BaseAction - 分頁處理

**檔案位置**: `ES\Areas\BACKMIN\Action\BaseAction.cs`

```csharp
public class BaseAction : RowBaseDAO
{
    public BaseAction(SqlConnection conn) : base(conn)
    {
        // 預設每頁 20 筆
        base.PageSize = 20;
    }

    /// <summary>
    /// 設定分頁資訊
    /// </summary>
    public void SetPageInfo(string resultId, int pageNo)
    {
        if (!string.IsNullOrEmpty(resultId))
        {
            base.ResultID = resultId;
        }

        base.PageNo = pageNo > 0 ? pageNo : 1;
    }

    /// <summary>
    /// 查詢清單 (自動分頁)
    /// </summary>
    protected IList<T> QueryForList<T>(string sql, DynamicParameters parms)
    {
        // 計算總筆數
        string countSql = "SELECT COUNT(*) FROM (" + sql + ") AS CountQuery";
        int totalRecords = Connection.ExecuteScalar<int>(countSql, parms);

        // 計算分頁資訊
        int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
        int offset = (PageNo - 1) * PageSize;

        // 設定分頁資訊
        PaginationInfo = new PaginationInfo
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            PageNo = PageNo,
            PageSize = PageSize
        };

        // 執行分頁查詢
        string pagedSql = sql +
            $" OFFSET {offset} ROWS FETCH NEXT {PageSize} ROWS ONLY";

        return Connection.Query<T>(pagedSql, parms).ToList();
    }
}
```

### 6.3 權限驗證範例

#### 6.3.1 BaseController - OnActionExecuting

**檔案位置**: `ES\Controllers\BaseController.cs`

```csharp
protected override void OnActionExecuting(ActionExecutingContext filterContext)
{
    base.OnActionExecuting(filterContext);

    string controllerName = filterContext.Controller.GetType().Name;
    string actionName = filterContext.ActionDescriptor.ActionName;

    // 檢查是否標註 AllowAnonymousAttribute
    var allowAnonymous = filterContext.ActionDescriptor
        .GetCustomAttributes(typeof(AllowAnonymousAttribute), true)
        .Length > 0;

    if (allowAnonymous)
    {
        return; // 允許匿名存取,略過登入檢查
    }

    // Session 驗證
    SessionModel sm = SessionModel.Get();
    try
    {
        if (sm.UserInfo == null)
        {
            sm.LastErrorMessage = "請先登入帳號";
            filterContext.Result = Redirect("/Login/Index");
            return;
        }

        // 記錄操作路徑 (用於操作記錄)
        sm.LastControllerName = controllerName;
        sm.LastActionName = actionName;
    }
    catch (Exception ex)
    {
        logger.Error("BaseController.OnActionExecuting 發生錯誤: " + ex.Message, ex);
        filterContext.Result = Redirect("/Login/Index");
    }
}
```

#### 6.3.2 後台權限檢核 (BACKMIN BaseController)

**檔案位置**: `ES\Areas\BACKMIN\Controllers\BaseController.cs`

```csharp
protected override void OnActionExecuting(ActionExecutingContext filterContext)
{
    base.OnActionExecuting(filterContext);

    string controllerName = filterContext.Controller.GetType().Name;
    string actionName = filterContext.ActionDescriptor.ActionName;

    SessionModel sm = SessionModel.Get();

    try
    {
        // 1. 檢查是否已登入
        if (sm.UserInfo == null || sm.UserInfo.Admin == null)
        {
            filterContext.Result = Redirect("/BACKMIN/Login");
            return;
        }

        // 2. 取得當前功能對應的權限設定
        string funcPath = GetFunctionPath(controllerName, actionName);
        var funcDef = GetFunctionDefinition(funcPath);

        if (funcDef == null)
        {
            // 功能未定義,視為可存取 (或可改為拒絕存取)
            return;
        }

        // 3. 檢查使用者群組是否有此功能權限
        bool hasPermission = CheckUserPermission(
            sm.UserInfo.Admin.ACC_NO,
            funcDef.prgid
        );

        if (!hasPermission)
        {
            sm.LastErrorMessage = "您沒有執行此功能的權限!";
            filterContext.Result = Redirect("/BACKMIN/Main/UnAuth");
            return;
        }

        // 4. 檢查操作權限 (新增/修改/刪除/查詢/列印)
        string operationType = GetOperationType(actionName);
        bool hasOperationPermission = CheckOperationPermission(
            sm.UserInfo.Admin.ACC_NO,
            funcDef.prgid,
            operationType
        );

        if (!hasOperationPermission)
        {
            sm.LastErrorMessage = $"您沒有{operationType}的權限!";
            filterContext.Result = Redirect("/BACKMIN/Main/UnAuth");
            return;
        }
    }
    catch (Exception ex)
    {
        logger.Error("權限檢核發生錯誤: " + ex.Message, ex);
        filterContext.Result = Redirect("/BACKMIN/Login");
    }
}

/// <summary>
/// 檢查使用者是否有功能權限
/// </summary>
private bool CheckUserPermission(string accNo, string prgid)
{
    using (SqlConnection conn = GetConnection())
    {
        string sql = @"
            SELECT COUNT(*) FROM AMGMAPM gm
            INNER JOIN AMUROLE ur ON ur.grp_id = gm.grp_id
            WHERE ur.ACC_NO = @ACC_NO
              AND gm.prgid = @prgid
              AND (gm.prg_i = '1' OR gm.prg_u = '1' OR gm.prg_d = '1'
                   OR gm.prg_q = '1' OR gm.prg_p = '1')
        ";

        int count = conn.ExecuteScalar<int>(sql, new { ACC_NO = accNo, prgid = prgid });
        return count > 0;
    }
}

/// <summary>
/// 檢查操作權限
/// </summary>
private bool CheckOperationPermission(string accNo, string prgid, string operationType)
{
    string permissionColumn = operationType switch
    {
        "新增" => "prg_i",
        "修改" => "prg_u",
        "刪除" => "prg_d",
        "查詢" => "prg_q",
        "列印" => "prg_p",
        _ => "prg_q" // 預設為查詢權限
    };

    using (SqlConnection conn = GetConnection())
    {
        string sql = $@"
            SELECT COUNT(*) FROM AMGMAPM gm
            INNER JOIN AMUROLE ur ON ur.grp_id = gm.grp_id
            WHERE ur.ACC_NO = @ACC_NO
              AND gm.prgid = @prgid
              AND gm.{permissionColumn} = '1'
        ";

        int count = conn.ExecuteScalar<int>(sql, new { ACC_NO = accNo, prgid = prgid });
        return count > 0;
    }
}
```

### 6.4 檔案上傳範例

**檔案位置**: `ES\Controllers\Apply_001005Controller.cs`

```csharp
/// <summary>
/// 上傳附件檔案
/// </summary>
[HttpPost]
public ActionResult UploadFile(HttpPostedFileBase file, string fileType)
{
    var result = new AjaxResultStruct();

    try
    {
        if (file == null || file.ContentLength == 0)
        {
            result.status = false;
            result.message = "請選擇檔案";
            return Json(result);
        }

        // 檢查檔案大小 (最大 10 MB)
        if (file.ContentLength > 10 * 1024 * 1024)
        {
            result.status = false;
            result.message = "檔案大小不得超過 10 MB";
            return Json(result);
        }

        // 檢查副檔名
        string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        string fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            result.status = false;
            result.message = "不支援的檔案格式,僅接受: " +
                string.Join(", ", allowedExtensions);
            return Json(result);
        }

        // 產生檔案唯一識別碼
        string fileId = Guid.NewGuid().ToString("N");
        string fileName = fileId + fileExtension;

        // 儲存路徑
        string uploadPath = Server.MapPath("~/Uploads");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        string filePath = Path.Combine(uploadPath, fileName);

        // 儲存檔案
        file.SaveAs(filePath);

        // 記錄檔案資訊至資料庫
        SessionModel sm = SessionModel.Get();
        using (SqlConnection conn = GetConnection())
        {
            conn.Open();

            TblAPPLY_FILE fileRecord = new TblAPPLY_FILE
            {
                FILE_ID = fileId,
                APP_ID = sm.CurrentAppId, // 暫存的申辦編號
                FILE_TYPE = fileType,
                FILE_NAME = file.FileName,
                FILE_PATH = "/Uploads/" + fileName,
                FILE_SIZE = file.ContentLength,
                UPLOAD_TIME = DateTime.Now
            };

            ApplyDAO dao = new ApplyDAO(conn);
            dao.Insert(fileRecord);

            conn.Close();
        }

        result.status = true;
        result.message = "檔案上傳成功";
        result.data = new {
            fileId = fileId,
            fileName = file.FileName,
            fileSize = file.ContentLength
        };

        return Json(result);
    }
    catch (Exception ex)
    {
        logger.Error("檔案上傳失敗: " + ex.Message, ex);
        result.status = false;
        result.message = "檔案上傳失敗: " + ex.Message;
        return Json(result);
    }
}
```

### 6.5 Excel 匯出範例

**檔案位置**: `ES\Areas\BACKMIN\Controllers\ExportController.cs`

```csharp
/// <summary>
/// 匯出申辦案件資料為 Excel
/// </summary>
public ActionResult ExportApplyToExcel(RecordFormModel form)
{
    try
    {
        using (SqlConnection conn = GetConnection())
        {
            conn.Open();

            RecordAction action = new RecordAction(conn);

            // 查詢所有資料 (不分頁)
            Hashtable parms = BuildQueryParams(form);
            var dataList = action.QueryApplyListAll(parms);

            // 使用 NPOI 建立 Excel
            IWorkbook workbook = new XSSFWorkbook(); // XLSX 格式
            ISheet sheet = workbook.CreateSheet("申辦案件資料");

            // 建立標題列
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("申辦編號");
            headerRow.CreateCell(1).SetCellValue("服務名稱");
            headerRow.CreateCell(2).SetCellValue("申請人");
            headerRow.CreateCell(3).SetCellValue("狀態");
            headerRow.CreateCell(4).SetCellValue("申辦時間");
            headerRow.CreateCell(5).SetCellValue("審核時間");
            headerRow.CreateCell(6).SetCellValue("審核人員");
            headerRow.CreateCell(7).SetCellValue("繳費金額");

            // 設定標題列樣式
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.Grey25.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            IFont headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            for (int i = 0; i <= 7; i++)
            {
                headerRow.GetCell(i).CellStyle = headerStyle;
            }

            // 填入資料列
            int rowIndex = 1;
            foreach (var item in dataList)
            {
                IRow dataRow = sheet.CreateRow(rowIndex);
                dataRow.CreateCell(0).SetCellValue(item.APP_ID);
                dataRow.CreateCell(1).SetCellValue(item.SRV_NAME);
                dataRow.CreateCell(2).SetCellValue(item.APPLY_NAME);
                dataRow.CreateCell(3).SetCellValue(item.STATUS_NAME);
                dataRow.CreateCell(4).SetCellValue(item.APP_TIME);
                dataRow.CreateCell(5).SetCellValue(item.APPR_TIME);
                dataRow.CreateCell(6).SetCellValue(item.APPR_USER);
                dataRow.CreateCell(7).SetCellValue(item.PAY_AMT?.ToString("N0"));

                rowIndex++;
            }

            // 自動調整欄位寬度
            for (int i = 0; i <= 7; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            conn.Close();

            // 輸出至 Browser
            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);

            string fileName = $"申辦案件資料_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
    catch (Exception ex)
    {
        logger.Error("匯出 Excel 失敗: " + ex.Message, ex);
        SessionModel.Get().LastErrorMessage = "匯出失敗: " + ex.Message;
        return RedirectToAction("Index", "Record");
    }
}
```

---

## 7. 安全機制

### 7.1 密碼安全

#### 7.1.1 密碼加密

- **加密演算法**: SHA256
- **加密位置**: 前端提交前即已加密,後端再次驗證
- **儲存格式**: 64 個字元的 16 進位字串

#### 7.1.2 密碼強度要求

```csharp
// 密碼規則驗證 (ES\Utils\CheckUtils.cs)
public static bool IsValidPassword(string password)
{
    // 至少 8 個字元
    if (password.Length < 8) return false;

    // 必須包含英文、數字、特殊符號
    string pattern = @"^(?=.*[!@#$%_-])(?=.*[A-Za-z])(?=.*[0-9])[A-Za-z0-9!@#$%_-]*$";
    return Regex.IsMatch(password, pattern);
}
```

**密碼規則**:
- 長度至少 8 個字元
- 必須包含英文字母 (大小寫不拘)
- 必須包含數字
- 必須包含特殊符號 (!@#$%_-)

#### 7.1.3 密碼變更機制

- 首次登入強制變更密碼
- 定期提醒變更密碼 (90 天)
- 密碼不得與前 3 次相同
- 密碼變更歷程記錄 (`AMCHANGEPWD_LOG`)

### 7.2 防護機制

#### 7.2.1 XSS (跨站腳本) 防護

```xml
<!-- Web.config -->
<system.web>
  <!-- 啟用請求驗證 -->
  <httpRuntime requestValidationMode="2.0" />
  <pages validateRequest="true" />
</system.web>

<system.webServer>
  <httpProtocol>
    <customHeaders>
      <!-- XSS 保護標頭 -->
      <add name="X-XSS-Protection" value="1; mode=block" />
      <add name="X-Content-Type-Options" value="nosniff" />
    </customHeaders>
  </httpProtocol>
</system.webServer>
```

**程式碼層級防護**:
```csharp
// View 中使用 @Html.Encode() 或 @Html.DisplayFor()
<p>@Html.Encode(Model.UserInput)</p>

// 或使用 AntiXSS Library
string safeHtml = Microsoft.Security.Application.Encoder.HtmlEncode(userInput);
```

#### 7.2.2 CSRF (跨站請求偽造) 防護

```csharp
// Controller
[HttpPost]
[ValidateAntiForgeryToken]
public ActionResult SubmitForm(FormModel form)
{
    // ...
}
```

```html
<!-- View -->
<form method="post">
    @Html.AntiForgeryToken()
    <!-- 表單欄位 -->
</form>
```

#### 7.2.3 SQL Injection 防護

**使用參數化查詢** (Dapper):
```csharp
// 安全的查詢方式
string sql = "SELECT * FROM MEMBER WHERE ACC_NO = @ACC_NO";
var result = conn.Query<TblMEMBER>(sql, new { ACC_NO = userNo });

// 錯誤的查詢方式 (禁止)
// string sql = $"SELECT * FROM MEMBER WHERE ACC_NO = '{userNo}'"; // 危險!
```

#### 7.2.4 路徑遍歷攻擊防護

```csharp
// Global.asax.cs - Application_BeginRequest
protected void Application_BeginRequest(object sender, EventArgs e)
{
    // 禁止直接存取特定資料夾
    Regex rx = new Regex(@"^(/[s|S]cripts|/[l|L]ogin|/[w|W]eb References|...)");
    if (rx.IsMatch(Request.Path))
    {
        Response.StatusCode = 404;
        Response.End();
        return;
    }
}
```

#### 7.2.5 HTTPS 強制轉向

```csharp
// Global.asax.cs - Application_BeginRequest
if (!Context.Request.IsSecureConnection)
{
    if (Request.ServerVariables["SERVER_PORT"].Contains("80"))
    {
        string redirectUrl = "https://" + Request.Url.Host + Request.Url.AbsolutePath;
        if (Request.QueryString.Count > 0)
        {
            redirectUrl += "?" + Request.QueryString.ToString();
        }
        Response.Redirect(redirectUrl);
    }
}
```

```xml
<!-- Web.config -->
<system.webServer>
  <httpProtocol>
    <customHeaders>
      <!-- HSTS 標頭 -->
      <add name="Strict-Transport-Security"
           value="max-age=31536000; includeSubDomains; preload" />
    </customHeaders>
  </httpProtocol>
</system.webServer>
```

### 7.3 Session 安全

#### 7.3.1 Cookie 安全設定

```csharp
// Global.asax.cs - Application_EndRequest
protected void Application_EndRequest()
{
    foreach (var item in Response.Cookies)
    {
        Response.Cookies[item.TONotNullString()].Secure = true;    // 僅 HTTPS 傳輸
        Response.Cookies[item.TONotNullString()].HttpOnly = true;   // 防止 JavaScript 存取
    }
}
```

```xml
<!-- Web.config -->
<system.web>
  <httpCookies httpOnlyCookies="true" requireSSL="true" sameSite="Strict" />
</system.web>
```

#### 7.3.2 Session Fixation 防護

```csharp
// 登入成功後重新產生 Session ID
public ActionResult UserLogin(LoginFormModel form)
{
    // ... 驗證成功 ...

    // 清除舊 Session
    Session.Clear();
    Session.Abandon();

    // 重新建立 Session
    SessionModel sm = SessionModel.Get();
    sm.UserInfo = userInfo;

    // ...
}
```

### 7.4 日誌與監控

#### 7.4.1 log4net 設定

```xml
<!-- Web.config -->
<log4net>
  <!-- Web 應用程式日誌 -->
  <appender name="WebLoggerAppender" type="log4net.Appender.RollingFileAppender">
    <param name="File" value="C:\e-service\Logs\Web\" />
    <param name="DatePattern" value="yyyy\\MM\\yyyyMMdd'.txt'" />
    <param name="StaticLogFileName" value="false" />
    <param name="AppendToFile" value="true" />
    <layout type="log4net.Layout.PatternLayout">
      <param name="ConversionPattern"
             value="%date [%thread] %-5level %logger - %message%newline" />
    </layout>
  </appender>

  <root>
    <level value="INFO" />
    <appender-ref ref="WebLoggerAppender" />
  </root>
</log4net>
```

#### 7.4.2 操作記錄

系統自動記錄以下操作:
- 登入/登出記錄 (`LOGIN_LOG`)
- 頁面瀏覽記錄 (`VISIT_RECORD`)
- 資料異動記錄 (`SYS_TRANS_LOG`)
- 錯誤日誌 (log4net)

```csharp
// BaseController.OnActionExecuted - 自動記錄操作
protected override void OnActionExecuted(ActionExecutedContext filterContext)
{
    base.OnActionExecuted(filterContext);

    SessionModel sm = SessionModel.Get();
    if (sm.UserInfo != null)
    {
        // 記錄操作至 VISIT_RECORD
        RecordVisit(
            sm.UserInfo.UserNo,
            sm.LastControllerName,
            sm.LastActionName
        );
    }
}
```

---

## 8. 系統特色與建議

### 8.1 系統特色

#### 8.1.1 多元憑證登入

系統整合五種憑證登入方式,提供使用者彈性選擇:
- 自然人憑證 (MOICA)
- 工商憑證 (MOEACA)
- 醫事人員憑證 (HCA1)
- 數位身分證 (NEWEID)
- 傳統帳號密碼

#### 8.1.2 完整的權限管理

採用「使用者-群組-功能」三層權限架構,支援細緻的權限控管:
- 功能層級權限
- 操作層級權限 (新增/修改/刪除/查詢/列印)
- 資料層級權限 (可擴充)

#### 8.1.3 整合政府支付平台

系統整合行政院「政府支付平台」,提供:
- 線上信用卡付款
- ATM 轉帳
- 超商代收
- 電子帳單

#### 8.1.4 豐富的申辦項目

整合衛福部多個司別的線上申辦業務:
- 醫事司: 20+ 項業務
- 中醫藥司: 10+ 項業務
- 社工司: 15+ 項業務

#### 8.1.5 完善的操作記錄

系統記錄所有重要操作,支援稽核追蹤:
- 登入/登出記錄
- 資料異動軌跡 (異動前後值比對)
- 操作路徑記錄
- 錯誤日誌

### 8.2 改善建議

#### 8.2.1 技術層面

**1. 升級至最新框架**
- 建議升級至 ASP.NET Core 6.0 或更新版本
- 優點:
  - 跨平台支援 (Linux, macOS)
  - 更好的效能
  - 現代化的開發體驗
  - 長期支援 (LTS)

**2. 前端框架現代化**
- 建議導入 Vue.js 3 或 React
- 優點:
  - 更好的使用者體驗
  - 元件化開發
  - 狀態管理
  - 支援 TypeScript

**3. API 化設計**
- 建議將業務邏輯封裝為 RESTful API
- 優點:
  - 前後端分離
  - 支援多端存取 (Web, Mobile App)
  - 易於整合第三方系統

**4. 資料庫效能優化**
- 建議檢視並優化資料庫索引
- 針對常用查詢建立適當的索引
- 考慮導入 Redis 快取熱門資料

#### 8.2.2 安全層面

**1. 密碼加密升級**
- 目前: SHA256
- 建議: bcrypt 或 Argon2
- 原因: SHA256 過於快速,易受暴力破解攻擊

```csharp
// 建議使用 bcrypt
using BCrypt.Net;

// 密碼加密
string hashedPassword = BCrypt.HashPassword(password);

// 密碼驗證
bool isValid = BCrypt.Verify(password, hashedPassword);
```

**2. 雙因子認證 (2FA)**
- 建議導入簡訊或 Email OTP
- 增加帳號安全性

**3. 憑證安全性提升**
- 建議加強憑證序號驗證
- 定期更新憑證黑名單

#### 8.2.3 使用者體驗

**1. 響應式設計 (RWD)**
- 建議優化手機版介面
- 支援觸控操作

**2. 進度儲存功能**
- 建議新增「儲存草稿」功能
- 避免使用者因逾時遺失填寫資料

**3. 智能表單驗證**
- 建議即時驗證輸入欄位
- 提供友善的錯誤提示

#### 8.2.4 維運層面

**1. 監控與告警**
- 建議導入 APM (Application Performance Monitoring)
- 工具推薦: Application Insights, New Relic

**2. 自動化部署**
- 建議導入 CI/CD Pipeline
- 工具推薦: Azure DevOps, Jenkins

**3. 備援機制**
- 建議建立異地備援機制
- 定期進行災難復原演練

### 8.3 系統優點總結

1. **完整的業務整合**: 整合多個司別的申辦業務於單一平台
2. **彈性的權限控管**: 支援細緻的權限設定,符合不同角色需求
3. **多元的登入方式**: 提供多種憑證登入,方便不同使用者
4. **完善的操作記錄**: 所有操作皆有記錄,支援稽核追蹤
5. **安全的機制設計**: 實作多層安全防護,保護使用者資料

---

## 附錄

### 附錄A: 檔案路徑對照表

| 功能模組 | 主要檔案位置 |
|---------|------------|
| 登入處理 | `ES\Controllers\LoginController.cs` |
| 前台基底控制器 | `ES\Controllers\BaseController.cs` |
| 後台基底控制器 | `ES\Areas\BACKMIN\Controllers\BaseController.cs` |
| Session 管理 | `ES\Models\SessionModel.cs` |
| 登入驗證服務 | `ES\Services\ClamService.cs` |
| 登入資料存取 | `ES\DataLayers\LoginDAO.cs` |
| 資料工具類別 | `ES\Utils\DataUtils.cs` |
| 驗證工具類別 | `ES\Utils\CheckUtils.cs` |
| 日誌工具類別 | `ES\Utils\LogUtils.cs` |
| 主要設定檔 | `ES\Web.config` |
| 應用程式啟動 | `ES\Global.asax.cs` |

### 附錄B: 資料表清單

#### 會員管理相關 (8 個資料表)
- MEMBER (會員資料)
- LOGIN_LOG (登入記錄)
- MEMBER_CHANGEPWD_LOG (密碼變更歷程)
- MEMBER_CERT (憑證資料)

#### 後台管理相關 (6 個資料表)
- ADMIN (管理員帳號)
- AMGRP (群組)
- AMUROLE (管理員-群組對應)
- AMFUNCM (功能定義)
- AMGMAPM (群組-功能權限對應)
- ADMIN_CHANGEPWD_LOG (管理員密碼變更歷程)

#### 申辦業務相關 (20+ 個資料表)
- APPLY (申辦案件主檔)
- APPLY_FILE (申辦附件檔案)
- APPLY_DETAIL (申辦明細)
- SERVICE (服務項目)
- ... (各業務專屬資料表)

#### 系統設定相關 (5 個資料表)
- SETUP (系統參數)
- CODE (系統代碼)
- ZIPCODE (郵遞區號)
- UNIT (單位)
- ENEWS (最新消息)

#### 系統記錄相關 (3 個資料表)
- SYS_TRANS_LOG (系統交易記錄)
- VISIT_RECORD (操作記錄)
- ERROR_LOG (錯誤日誌)

### 附錄C: 系統代碼表

#### 性別代碼 (SEX_CD)
- `M`: 男性
- `F`: 女性

#### 案件狀態代碼 (STATUS_CD)
- `01`: 待審核
- `02`: 補件中
- `03`: 審核通過
- `04`: 退件
- `05`: 已完成
- `09`: 已取消

#### 繳費狀態代碼 (PAY_STATUS)
- `01`: 未繳費
- `02`: 繳費中
- `03`: 已繳費
- `04`: 退費中
- `05`: 已退費

#### 憑證類型代碼 (CARD_TYPE)
- `MEMBER`: 帳號密碼
- `MOICA`: 自然人憑證
- `MOEACA`: 工商憑證
- `HCA1`: 醫事人員憑證
- `NEWEID`: 數位身分證

### 附錄D: 常見問題 FAQ

**Q1: 忘記密碼怎麼辦?**
A: 請點選登入頁面的「忘記密碼」連結,輸入註冊時的 Email,系統會寄送重設密碼連結至您的信箱。

**Q2: 為什麼無法登入?**
A: 可能的原因:
1. 帳號或密碼輸入錯誤
2. 帳號已被停用
3. 連續登入失敗 5 次,帳號已被鎖定 15 分鐘
4. Session 已過期

**Q3: 如何變更密碼?**
A: 登入後點選右上角帳號名稱,選擇「變更密碼」功能。

**Q4: 上傳檔案有什麼限制?**
A:
- 檔案大小限制: 10 MB
- 支援格式: PDF, JPG, JPEG, PNG, DOC, DOCX

**Q5: 案件審核需要多久時間?**
A: 一般案件約 7-14 個工作天,實際審核時間依案件複雜度而定。

---

**文件結束**

**編撰人員**: Claude AI
**審核人員**: (待填寫)
**版本歷程**:
- v1.0 (2025-10-08): 初版發佈
