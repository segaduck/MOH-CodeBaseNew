# 代碼庫版本差異分析報告 - trunk 版本

## 文件資訊

- **分析日期**: 2025-11-20
- **專案名稱**: EECOnline (trunk 版本)
- **舊版代碼庫**: `CodeBase/trunk_DISK_trunk/trunk/`
- **新版代碼庫**: `CodeBaseNew/trunk/`
- **分析目的**: 識別新版代碼庫中缺失或新增的重要文件和功能

---

## 執行摘要

經過詳細比對，新版 trunk 代碼庫相較於舊版有以下主要差異：

### 🔴 **重大缺失項目**

1. **Resource Area 完全缺失** - 資源管理區域整個模組不存在（與 e-service 版本相同問題）

### 🟢 **完整保留項目**

1. **所有配置文件完整保留** - Web.Debug.config, Web.Release.config 都存在 ✓
2. **所有 Helper 文件完整保留** - 包括所有擴展類別 ✓
3. **fonts 目錄完整保留** - 字體文件目錄存在 ✓
4. **aspnet_client 目錄完整保留** - ASP.NET 客戶端文件存在 ✓

### 📊 **對比 e-service 版本的優勢**

trunk 版本的新版代碼庫比 e-service 版本的新版代碼庫**更完整**：

| 項目                | e-service 新版 | trunk 新版 | 說明                     |
| ------------------- | -------------- | ---------- | ------------------------ |
| Resource Area       | ❌ 缺失        | ❌ 缺失    | 兩者都缺失               |
| Web.Debug.config    | ❌ 缺失        | ✅ 存在    | trunk 版本保留           |
| Web.Release.config  | ❌ 缺失        | ✅ 存在    | trunk 版本保留           |
| DatePickerExtension | ❌ 缺失        | ✅ 存在    | trunk 版本保留（需確認） |
| fonts/ 目錄         | ❌ 缺失        | ✅ 存在    | trunk 版本保留           |
| aspnet_client/ 目錄 | ❌ 缺失        | ✅ 存在    | trunk 版本保留           |

---

## 詳細差異分析

### 1. 目錄結構對比

#### 1.1 主要目錄結構（兩版本相同）

```
trunk/
├── App_Data/                     ✓ 兩版本都存在
├── App_Start/                    ✓ 兩版本都存在
├── Areas/                        ⚠️ 內容有差異（見下方）
├── Commons/                      ✓ 兩版本都存在
├── Content/                      ✓ 兩版本都存在
├── Controllers/                  ✓ 兩版本都存在
├── DataLayers/                   ✓ 兩版本都存在
├── Downloads/                    ✓ 兩版本都存在
├── HCA/                          ✓ 兩版本都存在
├── Helpers/                      ✓ 兩版本都存在
├── Models/                       ✓ 兩版本都存在
├── Scripts/                      ✓ 兩版本都存在
├── Services/                     ✓ 兩版本都存在
├── SqlMaps/                      ✓ 兩版本都存在
├── Template/                     ✓ 兩版本都存在
├── Uploads/                      ✓ 兩版本都存在
├── Utils/                        ✓ 兩版本都存在
├── Views/                        ✓ 兩版本都存在
├── WebService/                   ✓ 兩版本都存在
├── aspnet_client/                ✓ 兩版本都存在
├── assests/                      ✓ 兩版本都存在
├── bin/                          ✓ 兩版本都存在
├── css/                          ✓ 兩版本都存在
├── fonts/                        ✓ 兩版本都存在
├── images/                       ✓ 兩版本都存在
├── vendor/                       ✓ 兩版本都存在
└── webfonts/                     ✓ 兩版本都存在
```

**結論**: trunk 版本的目錄結構非常完整，幾乎所有目錄都保留。

---

### 2. Areas（區域）模組差異 ⚠️

#### 2.1 Resource Area 完全缺失 🔴

**舊版 Resource Area 的正確路徑**:

```
📁 舊版代碼庫中的 Resource Area 位置:

CodeBase/trunk_DISK_trunk/trunk/Areas/Resource/
├── Controllers/
│   ├── BaseController.cs          (基礎控制器，含驗證機制)
│   ├── CkEEC.cs                   (CkEEC 控制器 - trunk 版本特有)
│   ├── DBController.cs            (資料庫管理工具)
│   ├── DT1Controller.cs           (DT1 資源控制器)
│   └── FileController.cs          (檔案管理工具)
├── Views/
│   ├── CkEEC/                     (CkEEC 視圖 - trunk 版本特有)
│   ├── DB/
│   │   └── Index.cshtml           (SQL 執行介面)
│   ├── File/                      (檔案管理介面)
│   └── Shared/
└── ResourceAreaRegistration.cs    (區域路由註冊)
```

**trunk 版本的 Resource Area 特點**:

- ✅ 包含 **CkEEC.cs** 控制器（e-service 版本沒有）
- ✅ 包含 **CkEEC/** 視圖目錄（e-service 版本沒有）
- 其他功能與 e-service 版本相同

**新版代碼庫中的對應位置**:

```
❌ 新版代碼庫 (CodeBaseNew/trunk/Areas/) 中完全缺失

現有的 Areas:
├── A1/                               (業務區域 1)
├── A2/                               (業務區域 2)
├── A3/                               (業務區域 3)
├── A4/                               (業務區域 4)
├── A5/                               (業務區域 5)
├── A6/                               (業務區域 6)
├── A7/                               (業務區域 7)
├── A8/                               (業務區域 8)
├── BackApply/                        (後台申請區域)
├── Login/                            (登入區域)
└── SHARE/                            (共用區域)

⚠️ Resource Area 不存在於新版中
```

**影響評估**:

- ⚠️ **嚴重**: 這是一個完整的開發/維護工具模組缺失
- **主要影響**:
  - 無法使用線上 SQL 查詢工具
  - 無法使用伺服器端檔案管理工具
  - 無法使用 CkEEC 功能（trunk 版本特有）
  - 開發和維護效率降低
- **安全考量**: Resource Area 包含敏感功能，僅供內部使用
- **建議**: 確認是否為安全考量而刻意移除，或需要從舊版補回

**如需補回**:

- 來源: `CodeBase/trunk_DISK_trunk/trunk/Areas/Resource/`
- 目標: `CodeBaseNew/trunk/Areas/Resource/`
- 詳細步驟請參考: `Resource_Area_詳細說明與追查指引.md`

#### 2.2 保留的 Areas（11 個）

| Area 名稱 | 功能說明     | 狀態   |
| --------- | ------------ | ------ |
| A1        | 業務區域 1   | ✓ 存在 |
| A2        | 業務區域 2   | ✓ 存在 |
| A3        | 業務區域 3   | ✓ 存在 |
| A4        | 業務區域 4   | ✓ 存在 |
| A5        | 業務區域 5   | ✓ 存在 |
| A6        | 業務區域 6   | ✓ 存在 |
| A7        | 業務區域 7   | ✓ 存在 |
| A8        | 業務區域 8   | ✓ 存在 |
| BackApply | 後台申請區域 | ✓ 存在 |
| Login     | 登入區域     | ✓ 存在 |
| SHARE     | 共用區域     | ✓ 存在 |

**結論**: 除了 Resource Area 外，所有業務區域都完整保留。

---

### 3. 配置文件對比 ✅

#### 3.1 完整保留的配置文件

```
兩版本都存在 ✓
├── Web.config                    (主要配置文件)
├── Web.Debug.config              (調試環境配置) ✅
├── Web.Release.config            (發布環境配置) ✅
├── packages.config               (NuGet 套件配置)
├── EECOnline.csproj              (專案文件)
├── log4net.config                (日誌配置)
├── properties.config             (屬性配置)
├── providers.config              (提供者配置)
├── REST.config                   (REST 配置)
├── Report.config                 (報表配置)
└── SqlMap.config                 (SQL 映射配置)
```

**優勢**: trunk 版本的新版代碼庫**完整保留**了所有配置文件，包括：

- ✅ Web.Debug.config（e-service 新版缺失）
- ✅ Web.Release.config（e-service 新版缺失）

**結論**: trunk 版本在配置文件方面**沒有缺失**，優於 e-service 版本。

---

### 4. Helpers（輔助類）對比 ✅

#### 4.1 完整保留的 Helper 文件

trunk 版本的 Helpers 目錄中的文件（兩版本都存在）:

```
✓ 兩版本都存在
├── AGENCYExtension.cs            (機構擴展)
├── ButtonExtension.cs            (按鈕擴展)
├── CheckBoxExtension.cs          (核取方塊擴展)
├── CodeExtension.cs              (代碼擴展)
├── ControlExtension.cs           (控制項擴展)
├── EControlExtension.cs          (E控制項擴展)
├── ENEWSExtension.cs             (電子報擴展)
├── FileDownloadExtension.cs      (檔案下載擴展)
├── FileUploadExtension.cs        (檔案上傳擴展)
├── GRPExtension.cs               (群組擴展)
├── ImageHoverExtension.cs        (圖片懸停擴展)
├── IntegralListExtension.cs      (整合列表擴展)
├── LabelExtension.cs             (標籤擴展)
├── LogExtension.cs               (日誌擴展)
├── OPERATExtension.cs            (操作擴展)
├── RadioGroupExtension.cs        (單選群組擴展)
├── Select2Extension.cs           (Select2 擴展)
├── UNITExtension.cs              (單位擴展)
├── ZIP_COExtension.cs            (郵遞區號擴展)
└── ZIP_CO_DropExtension.cs       (郵遞區號下拉擴展)
```

**結論**: trunk 版本的 Helpers **完整保留**，沒有缺失任何文件。

**對比 e-service 版本**:

- e-service 版本缺失 `DatePickerExtension.cs`
- trunk 版本的 Helpers 與 e-service 版本的 Helpers 功能不同（業務邏輯不同）

---

### 5. 前端資源對比 ✅

#### 5.1 JavaScript 庫版本

trunk 版本使用的主要 JavaScript 庫:

| 庫名稱    | 版本     | 說明                 |
| --------- | -------- | -------------------- |
| jQuery    | 2.1.3    | 舊版本（兩版本相同） |
| jQuery    | 3.4.1    | 新版本（兩版本相同） |
| jQuery UI | (標準版) | 兩版本都存在         |
| Bootstrap | 4.4.1    | 兩版本都存在         |

**結論**: trunk 版本的前端庫**沒有升級問題**，兩版本使用相同的庫版本。

#### 5.2 字體文件 ✅

```
fonts/ 目錄完整保留 ✓
├── CMEXSung.ttf                  (中文字體)
├── CNS/                          (CNS 字體目錄)
├── FontAwesome.otf               (FontAwesome 字體)
├── MTCORSVA.TTF                  (特殊字體)
├── fontawesome-webfont.*         (FontAwesome Web 字體)
├── glyphicons-halflings-regular.* (Glyphicons 字體)
├── kaiu.ttf                      (標楷體)
└── welfare.*                     (福利字體)
```

**優勢**: trunk 版本**完整保留** fonts 目錄（e-service 新版缺失）

#### 5.3 圖片資源 ✅

```
images/ 目錄完整保留 ✓
├── 20160905174202.jpg
├── Back_logo.png
├── Dr_Licence_Service.jpg
├── Web_lock.jpeg
├── banner-01.jpg ~ banner-04.jpg
├── 各種 SVG 圖示
└── 等等...（100+ 個圖片文件）
```

**結論**: trunk 版本的圖片資源**完整保留**。

---

### 6. 其他重要目錄對比 ✅

#### 6.1 aspnet_client 目錄

```
aspnet_client/ 目錄完整保留 ✓
└── system_web/                   (ASP.NET 系統文件)
```

**優勢**: trunk 版本保留（e-service 新版缺失）

#### 6.2 webfonts 目錄

```
webfonts/ 目錄完整保留 ✓
├── fa-brands-400.*               (FontAwesome Brands)
├── fa-regular-400.*              (FontAwesome Regular)
└── fa-solid-900.*                (FontAwesome Solid)
```

**結論**: trunk 版本的 webfonts 目錄**完整保留**。

---

## 關鍵發現總結

### 唯一的缺失項目

1. **Resource Area 模組完全缺失** - 開發/維護工具模組（與 e-service 版本相同）

### trunk 版本的優勢

相較於 e-service 版本的新版代碼庫，trunk 版本的新版代碼庫**更完整**：

| 優勢項目              | 說明                                 |
| --------------------- | ------------------------------------ |
| ✅ 配置文件完整       | Web.Debug.config, Web.Release.config |
| ✅ Helpers 完整       | 所有輔助類別都保留                   |
| ✅ fonts 目錄完整     | 字體文件目錄完整保留                 |
| ✅ aspnet_client 完整 | ASP.NET 客戶端文件完整保留           |
| ✅ 前端庫穩定         | 沒有大版本升級，兼容性風險低         |

### 整體評估

- ✅ **核心功能**: 完整（除 Resource Area 外）
- ✅ **配置管理**: 完整
- ✅ **前端資源**: 完整且穩定
- ✅ **輔助功能**: 完整
- ⚠️ **唯一問題**: Resource Area 缺失

---

## 建議行動計畫

### 🔴 緊急（1-3 天內）

1. **調查 Resource Area 缺失**
   - 確認功能是否已遷移
   - 決定是否需要補回
   - **來源**: `CodeBase/trunk_DISK_trunk/trunk/Areas/Resource/`
   - **目標**: `CodeBaseNew/trunk/Areas/Resource/`

### 🟢 一般（1 個月內）

2. **代碼庫清理**（可選）

   - 確認是否需要清理編譯產物
   - 統一代碼格式

3. **文檔更新**
   - 記錄 Resource Area 的處理決策
   - 更新部署文檔

---

## 風險評估

| 風險項目               | 嚴重程度 | 可能性 | 影響範圍     | 緩解措施       |
| ---------------------- | -------- | ------ | ------------ | -------------- |
| Resource Area 功能缺失 | 🔴 高    | 高     | 開發維護效率 | 立即調查並補回 |

**總體風險**: 🟢 **低** - 除了 Resource Area 外，trunk 版本的新版代碼庫非常完整。

---

## 與 e-service 版本的對比總結

| 項目                | e-service 新版 | trunk 新版 | 優勢方      |
| ------------------- | -------------- | ---------- | ----------- |
| Resource Area       | ❌ 缺失        | ❌ 缺失    | 平手        |
| Web.Debug.config    | ❌ 缺失        | ✅ 存在    | **trunk**   |
| Web.Release.config  | ❌ 缺失        | ✅ 存在    | **trunk**   |
| DatePickerExtension | ❌ 缺失        | N/A        | trunk (N/A) |
| fonts/ 目錄         | ❌ 缺失        | ✅ 存在    | **trunk**   |
| aspnet_client/      | ❌ 缺失        | ✅ 存在    | **trunk**   |
| 前端庫升級          | ✅ 已升級      | ⏸️ 未升級  | 看需求      |

**結論**: trunk 版本的新版代碼庫在**完整性**方面優於 e-service 版本，但 e-service 版本在**現代化**方面（前端庫升級）有優勢。

---

## 相關文檔

1. **代碼庫版本差異分析\_e-service 版本.md** - e-service 版本的差異分析
2. **Resource*Area*詳細說明與追查指引.md** - Resource Area 專門文檔
3. **代碼庫差異修復檢查清單.md** - 詳細的修復檢查清單

---

## 附錄：Resource Area 補回指引

### 快速補回步驟

```bash
# 步驟 1: 複製整個 Resource 目錄
來源: CodeBase/trunk_DISK_trunk/trunk/Areas/Resource/
目標: CodeBaseNew/trunk/Areas/Resource/

# 步驟 2: 更新專案文件
在 EECOnline.csproj 中加入 Resource Area 的所有文件

# 步驟 3: 檢查命名空間
確認命名空間是否需要更新

# 步驟 4: 編譯測試
編譯專案並解決任何錯誤

# 步驟 5: 功能測試
訪問 /Resource/DB 和 /Resource/File 測試功能
```

### 安全注意事項

⚠️ **Resource Area 包含高風險功能**:

- DBController 可執行任意 SQL
- FileController 可訪問伺服器檔案系統
- CkEEC 控制器功能（trunk 版本特有）
- 建議僅在開發/測試環境啟用
- 生產環境建議停用或加強安全措施

---

**報告結束**

_本報告專門針對 trunk 版本的代碼庫差異分析。_

_trunk 版本的新版代碼庫整體非常完整，唯一的問題是 Resource Area 缺失。_
