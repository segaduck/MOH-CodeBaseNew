# Resource Area 詳細說明與追查指引

**文件版本**: 1.0  
**建立日期**: 2025-11-20  
**狀態**: 🔴 新版代碼庫中缺失

---

## 📍 位置資訊

### 舊版代碼庫位置（已確認存在）

#### 位置 1: 主要版本

```
路徑: CodeBase/109_e-service/ES/Areas/Resource/

完整結構:
CodeBase/109_e-service/ES/Areas/Resource/
├── Controllers/
│   ├── BaseController.cs          (基礎控制器，含安全驗證)
│   ├── DBController.cs            (資料庫管理工具)
│   ├── DT1Controller.cs           (DT1 資源控制器)
│   └── FileController.cs          (檔案管理工具)
├── Models/                        (資源模型 - 可能為空)
├── Views/
│   ├── DB/
│   │   └── Index.cshtml           (SQL 執行介面)
│   ├── File/
│   │   └── Index.cshtml           (檔案管理介面)
│   ├── Shared/
│   │   └── _Layout.cshtml         (共用版面)
│   ├── Web.config                 (視圖配置)
│   └── _ViewStart.cshtml          (視圖啟動)
└── ResourceAreaRegistration.cs    (MVC Area 路由註冊)
```

#### 位置 2: Trunk 版本

```
路徑: CodeBase/trunk_DISK_trunk/trunk/Areas/Resource/

額外包含:
└── Controllers/
    └── CkEEC.cs                   (額外的控制器)
└── Views/
    └── CkEEC/                     (對應視圖)
```

### 新版代碼庫位置（目前不存在）

```
❌ 不存在: CodeBaseNew/e-service/ES/Areas/Resource/

現有的 Areas:
CodeBaseNew/e-service/ES/Areas/
├── Admin/                         ✓ 存在
├── BACKMIN/                       ✓ 存在
└── Schedule/                      ✓ 存在

⚠️ Resource/ 目錄完全缺失
```

### 如需補回，應該複製到的位置

```
目標路徑: CodeBaseNew/e-service/ES/Areas/Resource/

建議來源: CodeBase/109_e-service/ES/Areas/Resource/
(因為這是較新的主要版本)
```

---

## 🔧 功能詳細說明

### 1. BaseController.cs

**命名空間**: `ES.Areas.Resource.Controllers`

**主要功能**:

- 提供 Resource Area 所有控制器的基礎類別
- 實現安全驗證機制

**安全機制**:

```csharp
protected override void OnActionExecuting(ActionExecutingContext filterContext)
{
    // 僅允許本機訪問或通過驗證的來源
    if (!(Request.IsLocal || IsValidateFrom()))
    {
        filterContext.Result = new HttpStatusCodeResult(404);
    }
}
```

**驗證方法**:

```csharp
protected bool Validate()
{
    // 檢查驗證碼: turbo@13141806
    if (Request["Validate"] != null &&
        Request["Validate"].Equals("turbo@13141806"))
    {
        Session.Add("UtilsValidate", "Y");
    }

    // 檢查 Session 驗證狀態
    if (Session["UtilsValidate"] != null &&
        Session["UtilsValidate"].Equals("Y"))
    {
        return true;
    }

    return false;
}
```

**訪問方式**:

1. 從 localhost 訪問（自動允許）
2. 使用驗證碼: `?Validate=turbo@13141806`

---

### 2. DBController.cs

**路由**: `/Resource/DB`

**主要功能**: 線上 SQL 查詢和執行工具

**功能特性**:

- ✅ 執行 SELECT 查詢
- ✅ 執行 INSERT/UPDATE/DELETE 異動（需勾選選項）
- ✅ 支援多個資料庫連線
- ✅ 顯示查詢結果
- ✅ 錯誤處理和訊息顯示

**支援的連線**:

1. `DefaultConnection` - 預設連線
2. `MDODConnection` - 醫事簽審連線

**使用介面**:

```html
表單欄位: - 連線選擇: 下拉選單 (預設/醫事簽審) - SQL 語句: 文字區域 (可輸入多行
SQL) - 異動選項: 核取方塊 (是否異動資料庫) - 送出按鈕: 執行 SQL
```

**安全考量**:

- ⚠️ 可執行任意 SQL，風險極高
- ⚠️ 僅應在開發/測試環境使用
- ⚠️ 生產環境建議完全移除或嚴格限制

**使用範例**:

```
訪問: http://localhost/Resource/DB?Validate=turbo@13141806
輸入: SELECT TOP 10 * FROM TblMEMBER
執行: 點擊送出
結果: 顯示查詢結果表格
```

---

### 3. FileController.cs

**路由**: `/Resource/File`

**主要功能**: 伺服器端檔案管理工具

**支援操作**:

1. **上傳檔案** (`UpdateFile`)

   - 支援多檔案上傳
   - 自動處理檔案儲存

2. **建立資料夾** (`CreateFolder`)

   - ActionType: CreateFolder
   - 建立新目錄

3. **刪除檔案/資料夾** (`DeleteList`)

   - ActionType: DeleteList
   - 批次刪除

4. **下載壓縮檔** (`GetZip`)
   - ActionType: GetZip
   - 打包下載

**使用介面**:

- 檔案瀏覽器
- 上傳表單
- 操作按鈕（新增/刪除/下載）

**安全考量**:

- ⚠️ 可訪問伺服器檔案系統
- ⚠️ 需要嚴格的路徑驗證
- ⚠️ 建議限制可訪問的目錄範圍

---

### 4. DT1Controller.cs

**路由**: `/Resource/DT1`

**主要功能**: DT1 資源管理

**說明**:

- 具體功能需要進一步確認
- 可能與特定業務資源相關

---

## 🔍 追查指引

### 步驟 1: 確認是否刻意移除

**檢查清單**:

- [ ] 查看專案變更記錄 (Git log)
- [ ] 檢查需求文件是否有說明
- [ ] 詢問專案負責人
- [ ] 確認生產環境是否使用

**可能的原因**:

1. **安全考量**: Resource Area 包含敏感功能，可能因安全政策而移除
2. **環境分離**: 可能僅在開發環境保留，生產環境移除
3. **功能遷移**: 功能可能已遷移到其他模組
4. **意外遺漏**: 可能在代碼整理時意外遺漏

### 步驟 2: 搜索替代實現

**搜索關鍵字**:

```
在 CodeBaseNew 中搜索:
- "DBController" (排除 Resource Area)
- "FileController" (排除 Resource Area)
- "SQL" + "execute" 或 "query"
- "file" + "upload" 或 "manage"
```

**檢查位置**:

- [ ] `Areas/BACKMIN/Controllers/` - 後台管理可能包含類似功能
- [ ] `Controllers/` - 主控制器目錄
- [ ] `Areas/Admin/` - 管理員區域

### 步驟 3: 決定處理方案

#### 方案 A: 功能已遷移

**行動**:

- 記錄新位置
- 更新文檔
- 通知團隊

#### 方案 B: 功能缺失且需要

**行動**:

- 從舊版複製 Resource Area
- 更新命名空間和配置
- 進行完整測試
- 更新部署文檔

#### 方案 C: 功能不再需要

**行動**:

- 記錄移除原因
- 歸檔舊版代碼
- 更新文檔說明

---

## 🔄 補回步驟（方案 B）

### 前置準備

1. **備份新版代碼庫**

   ```bash
   # 建議先建立 Git 分支
   git checkout -b feature/add-resource-area
   ```

2. **確認依賴項**
   - 檢查 Resource Area 是否依賴特定 NuGet 套件
   - 確認所有引用的類別在新版中存在

### 執行步驟

#### 步驟 1: 複製目錄

```bash
來源: CodeBase/109_e-service/ES/Areas/Resource/
目標: CodeBaseNew/e-service/ES/Areas/Resource/

# 複製整個目錄，包含所有子目錄和文件
```

#### 步驟 2: 更新專案文件 (ES.csproj)

在 `<ItemGroup>` 中加入:

```xml
<Compile Include="Areas\Resource\Controllers\BaseController.cs" />
<Compile Include="Areas\Resource\Controllers\DBController.cs" />
<Compile Include="Areas\Resource\Controllers\DT1Controller.cs" />
<Compile Include="Areas\Resource\Controllers\FileController.cs" />
<Compile Include="Areas\Resource\ResourceAreaRegistration.cs" />

<Content Include="Areas\Resource\Views\DB\Index.cshtml" />
<Content Include="Areas\Resource\Views\File\Index.cshtml" />
<Content Include="Areas\Resource\Views\Shared\_Layout.cshtml" />
<Content Include="Areas\Resource\Views\Web.config" />
<Content Include="Areas\Resource\Views\_ViewStart.cshtml" />
```

#### 步驟 3: 檢查命名空間

**檢查項目**:

```csharp
// 舊版可能使用:
namespace EUSERVICE.Areas.Resource.Controllers
namespace EECOnline.Areas.Resource.Controllers

// 新版應該使用:
namespace ES.Areas.Resource.Controllers
```

**需要更新的文件**:

- [ ] BaseController.cs
- [ ] DBController.cs
- [ ] DT1Controller.cs
- [ ] FileController.cs
- [ ] ResourceAreaRegistration.cs

#### 步驟 4: 更新安全設定

**修改驗證碼** (BaseController.cs):

```csharp
// 不要使用預設的 turbo@13141806
// 改用更安全的驗證碼
if (Request["Validate"] != null &&
    Request["Validate"].Equals("YOUR_SECURE_CODE_HERE"))
{
    Session.Add("UtilsValidate", "Y");
}
```

**加入 IP 限制** (可選):

```csharp
protected bool IsValidateFrom()
{
    string clientIP = Request.UserHostAddress;
    string[] allowedIPs = { "127.0.0.1", "::1", "YOUR_IP_HERE" };
    return allowedIPs.Contains(clientIP);
}
```

#### 步驟 5: 編譯測試

```bash
# 使用 Visual Studio 或 MSBuild
msbuild ES.csproj /p:Configuration=Debug

# 檢查編譯結果
# 解決任何編譯錯誤
```

#### 步驟 6: 功能測試

**測試清單**:

1. **路由測試**:

   ```
   訪問: http://localhost:PORT/Resource/DB
   預期: 顯示驗證頁面或 404（如未驗證）

   訪問: http://localhost:PORT/Resource/DB?Validate=YOUR_CODE
   預期: 顯示 SQL 執行介面
   ```

2. **DBController 測試**:

   ```sql
   -- 測試查詢
   SELECT TOP 5 * FROM TblMEMBER

   -- 測試異動（測試環境）
   UPDATE TblMEMBER SET UpdateDate = GETDATE() WHERE MemberId = 'TEST'
   ```

3. **FileController 測試**:
   - 上傳測試檔案
   - 建立測試資料夾
   - 刪除測試檔案
   - 下載壓縮檔

#### 步驟 7: 環境配置

**Web.config 設定** (可選):

```xml
<appSettings>
  <!-- 控制 Resource Area 是否啟用 -->
  <add key="ResourceArea:Enabled" value="true" />

  <!-- 允許的 IP 清單 -->
  <add key="ResourceArea:AllowedIPs" value="127.0.0.1,::1" />
</appSettings>
```

**環境別設定**:

- 開發環境: 啟用
- 測試環境: 啟用
- 預生產環境: 視需求決定
- 生產環境: **建議停用**

---

## ⚠️ 安全注意事項

### 高風險功能

1. **DBController - SQL 執行**

   - 🔴 **風險等級**: 極高
   - 可執行任意 SQL 語句
   - 可能導致資料洩漏或損毀
   - **建議**: 僅在開發/測試環境啟用

2. **FileController - 檔案管理**
   - 🟡 **風險等級**: 高
   - 可訪問伺服器檔案系統
   - 可能導致敏感檔案洩漏
   - **建議**: 限制可訪問的目錄範圍

### 安全加強措施

#### 1. 強化驗證機制

```csharp
// 使用更複雜的驗證
// 考慮使用 JWT 或其他 Token 機制
// 加入時間限制（Session timeout）
// 記錄所有訪問日誌
```

#### 2. IP 白名單

```csharp
// 僅允許特定 IP 訪問
// 可設定在 Web.config 中
// 支援 IP 範圍設定
```

#### 3. 操作日誌

```csharp
// 記錄所有 SQL 執行
// 記錄所有檔案操作
// 包含時間、使用者、IP、操作內容
// 定期審查日誌
```

#### 4. 環境隔離

```csharp
#if DEBUG
    // 僅在 Debug 模式啟用
#else
    // Release 模式自動停用
    return new HttpStatusCodeResult(404);
#endif
```

### 部署檢查清單

**生產環境部署前**:

- [ ] 確認是否真的需要 Resource Area
- [ ] 如需要，確認已修改預設驗證碼
- [ ] 確認已設定 IP 白名單
- [ ] 確認已加入操作日誌
- [ ] 確認已進行安全測試
- [ ] 確認已取得安全團隊批准
- [ ] 確認已更新部署文檔

**建議的生產環境配置**:

```xml
<appSettings>
  <!-- 生產環境建議停用 -->
  <add key="ResourceArea:Enabled" value="false" />
</appSettings>
```

---

## 📝 決策記錄範本

```markdown
## Resource Area 處理決策

**決策日期**: YYYY-MM-DD
**決策人**: [姓名/職位]
**參與討論**: [團隊成員]

### 調查結果

- [ ] 功能已遷移到: ******\_\_\_******
- [ ] 功能缺失
- [ ] 功能不再需要

### 選擇的方案

- [ ] 方案 A: 更新文檔說明遷移位置
- [ ] 方案 B: 從舊版補回 Resource Area
- [ ] 方案 C: 記錄移除原因並歸檔

### 理由說明

[詳細說明選擇此方案的原因]

### 執行計畫

[如選擇方案 B，列出執行步驟和時程]

### 安全考量

[說明安全措施和風險控管]

### 後續追蹤

- [ ] 更新技術文檔
- [ ] 通知相關團隊
- [ ] 更新部署流程
- [ ] 進行安全審查

**簽核**: ******\_\_\_******
**日期**: ******\_\_\_******
```

---

## 📚 相關文檔

1. **代碼庫版本差異分析.md** - 完整的差異分析報告
2. **代碼庫差異修復檢查清單.md** - 詳細的修復檢查清單
3. **Codebase_Comparison_Summary.md** - 英文摘要版本

---

## 🔗 快速連結

**舊版 Resource Area 位置**:

- `CodeBase/109_e-service/ES/Areas/Resource/`
- `CodeBase/trunk_DISK_trunk/trunk/Areas/Resource/`

**新版應該在的位置**:

- `CodeBaseNew/e-service/ES/Areas/Resource/` ❌ (目前不存在)

**訪問 URL** (補回後):

- SQL 工具: `http://localhost:PORT/Resource/DB?Validate=YOUR_CODE`
- 檔案管理: `http://localhost:PORT/Resource/File?Validate=YOUR_CODE`

---

**文件結束**

_本文件提供 Resource Area 的完整說明和追查指引，請根據實際情況進行調整。_
