# 代碼庫版本差異分析報告 - e-service 版本

## 文件資訊

- **分析日期**: 2025-11-20
- **專案名稱**: e-service (衛生福利部電子化服務平台)
- **舊版代碼庫**: `CodeBase/109_e-service/ES/`
- **新版代碼庫**: `CodeBaseNew/e-service/ES/`
- **分析目的**: 識別新版代碼庫中缺失或新增的重要文件和功能

---

## 執行摘要

經過詳細比對，新版 e-service 代碼庫相較於舊版有以下主要差異：

### 🔴 **重大缺失項目**

1. **Resource Area 完全缺失** - 資源管理區域整個模組不存在
2. **DatePickerExtension.cs 缺失** - 日期選擇器擴展功能
3. **部分配置文件缺失** - Web.Debug.config, Web.Release.config, Web-2021-rc1.config
4. **fonts 目錄缺失** - 字體文件目錄（根目錄）
5. **aspnet_client 目錄缺失** - ASP.NET 客戶端文件
6. **部分圖片文件缺失** - 多個橫幅和圖示文件

### 🟢 **新增項目**

1. **Bootstrap 4/5 相關文件** - 升級的 Bootstrap 框架文件
2. **jQuery UI 1.13.2** - 更新的 jQuery UI 版本
3. **obj 目錄** - 編譯輸出目錄（開發環境產物）
4. **ES.csproj.user** - Visual Studio 用戶配置文件
5. **Microsoft.mshtml.dll** - 新增的依賴庫
6. **新的 Bootstrap 模組化文件** - bootstrap-grid, bootstrap-reboot 等

---

## 詳細差異分析

### 1. 目錄結構差異

#### 1.1 缺失的主要目錄

| 目錄路徑          | 用途說明                 | 影響程度    |
| ----------------- | ------------------------ | ----------- |
| `Areas/Resource/` | 資源管理區域（完整模組） | ⚠️ **嚴重** |
| `fonts/`          | 字體文件目錄（根目錄）   | ⚠️ **中等** |
| `aspnet_client/`  | ASP.NET 客戶端系統文件   | ⚠️ **中等** |
| `Content/images/` | 內容圖片目錄             | ⚠️ **中等** |

**說明**:

- `fonts/` 目錄在舊版位於根目錄，新版可能已遷移到 `Content/fonts/`
- `Content/images/` 在新版中缺失，但主要圖片資源在 `Images/` 目錄中

#### 1.2 新增的目錄

| 目錄路徑       | 用途說明       | 備註                         |
| -------------- | -------------- | ---------------------------- |
| `obj/`         | 編譯輸出目錄   | 開發環境產物，通常不納入版控 |
| `Scripts/esm/` | ES Module 腳本 | 現代化 JavaScript 模組       |

---

### 2. 配置文件差異

#### 2.1 缺失的配置文件

```
舊版存在 ✓ | 新版缺失 ✗
├── Web.Debug.config          (調試環境配置)
├── Web.Release.config        (發布環境配置)
├── Web-2021-rc1.config       (2021版本配置)
└── ES.Publish.xml            (發布配置)
```

**影響**:

- 無法進行環境特定的配置轉換
- 部署到不同環境時可能出現配置問題
- 缺少發布配置可能導致部署流程中斷

**建議**:

- 從舊版複製 Web.Debug.config 和 Web.Release.config
- 確認 Web-2021-rc1.config 是否仍需要

#### 2.2 保留的配置文件

```
兩版本都存在 ✓
├── Web.config                (主要配置文件)
├── packages.config           (NuGet 套件配置)
└── ES.csproj                 (專案文件)
```

---

### 3. Areas（區域）模組差異

#### 3.1 Resource Area 完全缺失 ⚠️

**舊版 Resource Area 的正確路徑**:

```
📁 舊版代碼庫中的 Resource Area 位置:

CodeBase/109_e-service/ES/Areas/Resource/
├── Controllers/
│   ├── BaseController.cs          (基礎控制器，含驗證機制)
│   ├── DBController.cs            (資料庫管理工具)
│   ├── DT1Controller.cs           (DT1 資源控制器)
│   └── FileController.cs          (檔案管理工具)
├── Models/                        (資源模型)
├── Views/
│   ├── DB/
│   │   └── Index.cshtml           (SQL 執行介面)
│   ├── File/                      (檔案管理介面)
│   └── Shared/
└── ResourceAreaRegistration.cs    (區域路由註冊)
```

**新版代碼庫中的對應位置**:

```
❌ 新版代碼庫 (CodeBaseNew/e-service/ES/Areas/) 中完全缺失

現有的 Areas:
├── Admin/                            (管理員區域)
├── BACKMIN/                          (後台管理區域)
└── Schedule/                         (排程任務區域)

⚠️ Resource Area 不存在於新版中
```

**Resource Area 功能詳細說明**:

1. **DBController.cs** - 資料庫管理工具

   - 功能: 提供線上 SQL 查詢和執行介面
   - 用途: 開發/維護人員可直接執行 SQL 語句
   - 安全機制: 需要特殊驗證碼 (`turbo@13141806`)
   - 支援連線: 預設連線、醫事簽審連線
   - 操作模式: 查詢模式、異動模式

2. **FileController.cs** - 檔案管理工具

   - 功能: 伺服器端檔案管理
   - 支援操作:
     - 上傳檔案
     - 建立資料夾
     - 刪除檔案/資料夾
     - 下載壓縮檔
   - 用途: 管理系統檔案資源

3. **DT1Controller.cs** - DT1 資源控制器

   - 功能: 特定資源管理（需進一步確認用途）

4. **BaseController.cs** - 基礎控制器
   - 安全機制:
     - 僅允許本機 (localhost) 訪問
     - 或通過特定驗證
     - Session 驗證機制
   - 防護: 非授權訪問返回 404

**影響評估**:

- ⚠️ **嚴重**: 這是一個完整的開發/維護工具模組缺失
- **主要影響**:
  - 無法使用線上 SQL 查詢工具
  - 無法使用伺服器端檔案管理工具
  - 開發和維護效率降低
- **安全考量**: Resource Area 包含敏感功能，僅供內部使用
- **建議**: 確認是否為安全考量而刻意移除，或需要從舊版補回

**如需補回**:

- 來源: `CodeBase/109_e-service/ES/Areas/Resource/`
- 目標: `CodeBaseNew/e-service/ES/Areas/Resource/`
- 詳細步驟請參考: `Resource_Area_詳細說明與追查指引.md`

#### 3.2 保留的 Areas

| Area 名稱 | 功能說明     | 狀態   |
| --------- | ------------ | ------ |
| Admin     | 管理員功能區 | ✓ 存在 |
| BACKMIN   | 後台管理區   | ✓ 存在 |
| Schedule  | 排程任務區   | ✓ 存在 |

---

### 4. Helpers（輔助類）差異

#### 4.1 缺失的 Helper 文件

| 文件名稱                 | 功能說明       | 影響        |
| ------------------------ | -------------- | ----------- |
| `DatePickerExtension.cs` | 日期選擇器擴展 | ⚠️ **中等** |

**詳細說明**:

- 舊版有 `DatePickerExtension.cs`（標準日期選擇器）
- 新版只保留 `DatePickerACExtension.cs`（民國年日期選擇器）和 `DatePickerTWExtension.cs`（台灣日期選擇器）
- 可能影響使用西元年份的日期選擇功能

**建議**: 從舊版補回或確認是否有替代實現

#### 4.2 保留的 Helper 文件（24 個）

所有其他 Helper 文件均完整保留，包括：

- ABCNUMExtension.cs
- AddrExtension.cs
- CheckboxListExtension.cs
- ControlExtension.cs
- 等等...（共 24 個文件）

---

### 5. 前端資源差異

#### 5.1 JavaScript 庫版本變更

| 庫名稱    | 舊版版本 | 新版版本 | 變更類型 |
| --------- | -------- | -------- | -------- |
| jQuery    | 1.10.1   | 3.7.1    | ✓ 升級   |
| jQuery UI | 1.10.3   | 1.13.2   | ✓ 升級   |
| Bootstrap | 3.x      | 4.x/5.x  | ✓ 升級   |

**影響分析**:

- ✅ **正面**: 安全性和性能提升
- ⚠️ **風險**: 可能存在 API 不兼容問題
- 📝 **建議**: 需要進行完整的兼容性測試

---

## 關鍵發現總結

### 最嚴重的缺失

1. **Resource Area 模組完全缺失** - 開發/維護工具模組
2. **環境配置文件缺失** - Web.Debug.config, Web.Release.config

### 中等優先級缺失

1. **DatePickerExtension.cs** - 標準日期選擇器
2. **fonts 目錄** - 字體文件（可能已遷移）

### 積極改進

1. **前端框架現代化** - jQuery, jQuery UI, Bootstrap 全面升級
2. **TypeScript 支持** - 新增類型定義文件

---

## 建議行動計畫

### 🔴 緊急（1-3 天內）

1. **調查 Resource Area 缺失**

   - 確認功能是否已遷移
   - 決定是否需要補回

2. **補回關鍵配置文件**
   - 恢復 Web.Debug.config
   - 恢復 Web.Release.config

### 🟡 重要（1-2 週內）

3. **前端兼容性測試**

   - 測試 jQuery 3.7.1 兼容性
   - 測試 Bootstrap 4/5 兼容性

4. **補回缺失的 Helper**
   - 恢復 DatePickerExtension.cs
   - 測試日期選擇功能

---

## 風險評估

| 風險項目               | 嚴重程度 | 可能性 | 影響範圍     | 緩解措施       |
| ---------------------- | -------- | ------ | ------------ | -------------- |
| Resource Area 功能缺失 | 🔴 高    | 高     | 開發維護效率 | 立即調查並補回 |
| 前端庫不兼容           | 🟡 中    | 中     | 所有前端功能 | 完整測試       |
| 配置文件缺失           | 🟡 中    | 高     | 部署流程     | 補回配置文件   |
| DatePicker 功能異常    | 🟡 中    | 中     | 日期輸入功能 | 補回或替代實現 |

---

## 相關文檔

1. **代碼庫版本差異分析.md** - 原始完整分析報告（已整合到本文檔）
2. **代碼庫版本差異分析\_trunk 版本.md** - trunk 版本的差異分析
3. **Resource*Area*詳細說明與追查指引.md** - Resource Area 專門文檔
4. **代碼庫差異修復檢查清單.md** - 詳細的修復檢查清單

---

**報告結束**

_本報告專門針對 e-service 版本的代碼庫差異分析。_
