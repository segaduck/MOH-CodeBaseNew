# EECOnline 系統資料庫連線說明文件

## 系統概述

**EECOnline** (民眾線上申辦電子病歷服務平台) 是衛生福利部的電子病歷申請管理系統，提供民眾線上申請電子病歷及醫院端管理功能。

| 項目          | 說明                                     |
| ------------- | ---------------------------------------- |
| **專案名稱**  | EECOnline                                |
| **專案 GUID** | `{8B0971ED-8D8F-44E2-B1F9-0A0FBD10B87F}` |
| **目標框架**  | .NET Framework 4.5.2                     |
| **MVC 版本**  | ASP.NET MVC 5.2.3                        |
| **ORM 框架**  | iBATIS.NET (MyBatis 前身)                |

---

## 資料庫連線架構

### 連線設定檔案

EECOnline 使用 **iBATIS.NET** 作為 ORM 框架，資料庫連線設定分散於多個設定檔：

```
Web.config                 ← 定義連線字串名稱
    ↓
properties.config          ← 定義連線字串內容
    ↓
providers.config           ← 定義資料庫提供者
    ↓
SqlMap.config              ← 整合連線設定與 SQL 映射
```

---

### 1. Web.config (連線字串定義)

```xml
<connectionStrings>
  <add name="connectionEUSERVICE" connectionString="Data Source=請輸入資料庫連線字串" />
  <add name="DefaultConnection" providerName="System.Data.SqlClient"
       connectionString="Data Source=請輸入資料庫連線字串" />
</connectionStrings>
```

| 連線名稱                | 說明                            |
| ----------------------- | ------------------------------- |
| **connectionEUSERVICE** | iBATIS.NET 使用的主要連線       |
| **DefaultConnection**   | ASP.NET Identity 或其他框架使用 |

⚠️ **注意：** 目前設定檔中的連線字串為佔位符，部署時需更新為實際資料庫位址。

---

### 2. properties.config (連線參數定義)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<settings>
  <add key="dbTypeSQLServer" value="SQLSERVER" />
  <add key="providerSQLClient4" value="SQLClient4.0" />
  <add key="connectionEUSERVICE" value="Data Source=請輸入資料庫連線字串"/>
</settings>
```

此檔案定義 iBATIS.NET 使用的變數，可在 `SqlMap.config` 中以 `${變數名}` 方式引用。

---

### 3. providers.config (資料庫提供者)

支援兩種資料庫提供者：

| 提供者名稱                | 說明                     | 狀態              |
| ------------------------- | ------------------------ | ----------------- |
| **SQLClient4.0**          | Microsoft SQL Server 4.0 | ✅ 使用中         |
| **ODP.NET Managed 4.121** | Oracle Database          | 🔸 已定義但未使用 |

---

### 4. SqlMap.config (SQL 映射設定)

```xml
<database>
  <provider name="${providerSQLClient4}"/>
  <dataSource name="EECOnline" connectionString="${connectionEUSERVICE}" />
</database>

<sqlMaps>
  <sqlMap resource="./SqlMaps/Commons.xml" />
  <sqlMap resource="./SqlMaps/Login.xml" />
  <sqlMap resource="./SqlMaps/A1.xml" />
  <!-- ... 其他 SQL 映射檔 -->
</sqlMaps>
```

---

## 資料存取層架構

### DAO 類別繼承關係

```
Turbo.DataLayer.RowBaseDAO (外部 DLL)
         ↓
    BaseDAO (本專案)
         ↓
   ┌─────┴─────┬────────┬────────┬────────┐
   ↓           ↓        ↓        ↓        ↓
LoginDAO    A1DAO    A2DAO  FrontDAO   ...
```

### BaseDAO 說明

```csharp
public class BaseDAO : RowBaseDAO
{
    // 以預設的 SqlMap config 連接資料庫
    public BaseDAO() : base()
    {
        base.PageSize = ConfigModel.DefaultPageSize;
        // 植入客制化資料異動記錄的功能
        base.SetExecuteTracert(new TransLogDAO());
    }

    // 以指定的 SqlMap config 連接資料庫
    public BaseDAO(string sqlMapConfig) : base(sqlMapConfig)
    {
        // ...
    }
}
```

---

## SQL 映射檔案對照表

| 檔案          | 用途           | 對應功能模組       |
| ------------- | -------------- | ------------------ |
| Commons.xml   | 基本 CRUD 操作 | 全系統共用         |
| Login.xml     | 登入認證相關   | 登入 (C101M/C102M) |
| A1.xml        | 病歷設定管理   | A1 區域            |
| A2.xml        | 申辦案件管理   | A2 區域            |
| A3.xml        | 帳務計算管理   | A3 區域            |
| A4.xml        | 報表統計管理   | A4 區域            |
| A5.xml        | 帳號管理       | A5 區域            |
| A6.xml        | 系統權限管理   | A6 區域            |
| A7.xml        | 病歷設定管理   | A7 區域            |
| A8.xml        | 紀錄查詢       | A8 區域            |
| BackApply.xml | 後台案件處理   | BackApply 區域     |
| Front.xml     | 首頁相關       | Home               |
| Report.xml    | 報表功能       | 報表模組           |
| KeyMap.xml    | 共用代碼對照   | 全系統共用         |
| SHARE.xml     | 共用對話框功能 | 全系統共用         |

---

## 密碼加密機制

系統使用 **SHA512** 加密演算法處理密碼：

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

⚠️ **注意：** 舊版使用 RSACSP 可逆加密，已逐步轉換為 SHA512 不可逆雜湊。

---

## 外部服務整合

### 1. 醫院端 Web Service

**中山醫學大學附設醫院病歷申請服務：**

```xml
<setting name="EECOnline_tw_org_csh_sysint_MedRecApply" serializeAs="String">
  <value>https://sysint.csh.org.tw/MedRecApply/MedRecApply.asmx</value>
</setting>
```

### 2. Hospital_Common_Api

系統透過 RESTful API 與各醫院系統介接：

- API 網址設定於資料庫 `SETUP` 資料表 (`setup_cd = 'Hospital_Common_Api'`)
- 登入憑證設定於 `Web.config` AppSettings：
  - `LoginUser`: API 登入帳號
  - `LoginPwd`: API 登入密碼

### 3. 訓練系統整合 (elite.etraining.gov.tw)

```xml
<setting name="EUSERVICE_tw_gov_etraining_elite_get_data">
  <value>https://elite.etraining.gov.tw/lpf_wsv/get_data.asmx</value>
</setting>
```

---

## 主要資料表

| 資料表       | 說明             |
| ------------ | ---------------- |
| AMDBURM      | 使用者帳號主檔   |
| AMGRP        | 權限群組         |
| AMUROLE      | 使用者角色對照   |
| AMFUNCM      | 功能選單         |
| AMGMAPM      | 群組功能權限對照 |
| EEC_Hospital | 醫院端帳號       |
| AMUROLE_Hosp | 醫院端使用者角色 |
| AMGRP_Hosp   | 醫院端權限群組   |
| LOGINLOG     | 登入紀錄         |
| VISIT_RECORD | 功能使用紀錄     |
| SETUP        | 系統設定參數     |

---

## 部署注意事項

### 連線字串更新

部署至正式環境時，需更新以下檔案：

1. **Web.config** - 更新 `connectionStrings` 區段
2. **properties.config** - 更新 `connectionEUSERVICE` 值

### 環境設定

`Web.config` 中的 AppSettings 需依環境調整：

| 設定項目         | 測試環境            | 正式環境   |
| ---------------- | ------------------- | ---------- |
| OnOrOff          | 0                   | 1          |
| HCAOnOff         | 0                   | 1          |
| OnOrOffData      | 0                   | 1          |
| level1OnOrOff    | 0                   | 1          |
| NetID            | 1 (內網) / 2 (外網) | 依實際環境 |
| DisableAuthorize | 1 (停用)            | 0 (啟用)   |

### Cookie 安全性

正式環境需啟用 SSL Cookie：

```xml
<httpCookies httpOnlyCookies="true" requireSSL="true" />
```

⚠️ **注意：** 本機測試需註解此設定，否則會造成驗證碼錯誤。

---

## 系統架構圖

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           EECOnline 系統架構                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────┐                   │
│  │   部端登入   │   │  醫院端登入  │   │  民眾前台   │                   │
│  │  (AMDBURM)  │   │(EEC_Hospital)│   │             │                   │
│  └──────┬──────┘   └──────┬──────┘   └──────┬──────┘                   │
│         │                 │                 │                           │
│         └────────────┬────┴─────────────────┘                           │
│                      ↓                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      ASP.NET MVC 5.2.3                          │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐        │   │
│  │  │   A1     │  │   A2     │  │   A3     │  │   A4     │   ...  │   │
│  │  │ 病歷設定 │  │ 案件管理 │  │ 帳務計算 │  │ 報表統計 │        │   │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘        │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                      ↓                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    DataLayers (DAO)                             │   │
│  │  BaseDAO ← RowBaseDAO (Turbo.DataLayer.dll)                    │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                      ↓                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    iBATIS.NET ORM                               │   │
│  │  SqlMap.config + properties.config + providers.config          │   │
│  │  SqlMaps/*.xml (SQL 映射檔)                                     │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                      ↓                                                  │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    SQL Server Database                          │   │
│  │  connectionEUSERVICE (主要資料庫)                               │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                    外部服務整合                                  │   │
│  │  • 中山醫學大學病歷服務 (Web Service)                           │   │
│  │  • Hospital_Common_Api (RESTful API)                            │   │
│  │  • 訓練系統 elite.etraining.gov.tw                              │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 與 e-Service 系統比較

| 項目         | EECOnline                         | e-Service (ES)               |
| ------------ | --------------------------------- | ---------------------------- |
| **用途**     | 電子病歷申請平台                  | 醫事線上申辦系統             |
| **ORM**      | iBATIS.NET                        | Dapper                       |
| **連線設定** | properties.config + SqlMap.config | Web.config connectionStrings |
| **密碼加密** | SHA512                            | SHA256                       |
| **外部整合** | 醫院 Web Service / RESTful API    | MDOD 醫事簽審 (SFTP)         |
| **連線數量** | 1 個 (connectionEUSERVICE)        | 3 個 (Default/MDOD/SFTP)     |

---

## 更新紀錄

| 日期       | 說明     |
| ---------- | -------- |
| 2025-11-28 | 初版建立 |
