# trunk 管理後台 - 正式環境部署說明

## 版本資訊

| 項目     | 說明                                            |
| -------- | ----------------------------------------------- |
| 版本     | 1.1                                             |
| 日期     | 2025-12-06                                      |
| 系統     | **trunk (管理後台)**                             |
| 修改項目 | 檔案上傳安全性強化 + Mock登入 + 管理功能修正 + 6碼郵遞區號支援 + 郵遞區號驗證優化 |

---

## 修改內容摘要

### 1. 檔案上傳安全性強化 (Phase 1) ⚠️ 安全性關鍵

#### 新增功能
- ✅ **集中式安全驗證工具** (`FileSecurityHelper.cs`) - **新增檔案**
- ✅ **MIME 類型驗證** (Content-Type 檢查)
- ✅ **Magic Bytes 驗證** (檔案頭真實性檢查)
- ✅ **危險副檔名黑名單** (12+ 類別封鎖)
- ✅ **雙重副檔名檢測** (防止 shell.aspx.pdf 攻擊)
- ✅ **CSRF Token 保護** (防跨站請求偽造)
- ✅ **UI 強化** (嚴格 JavaScript 驗證)

#### 安全防護層級

```
Layer 1: 客戶端 JavaScript 驗證 (change 事件 + 提交前驗證)
   ↓
Layer 2: 瀏覽器 CORS 政策 (跨域資源共享)
   ↓
Layer 3: CSRF Token 驗證 (ValidateAntiForgeryToken)
   ↓
Layer 4: FileSecurityHelper 伺服器驗證 (Magic Bytes + MIME + 黑名單)
   ↓
✅ 安全上傳成功
```

### 2. Mock 登入功能 (測試用)

- ✅ 新增 `MockLogin.cshtml` - 一般使用者 Mock 登入
- ✅ 新增 `MockLoginAdmin.cshtml` - 管理員 Mock 登入
- ✅ 修改 `HomeController.cs` - Mock 登入處理
- ⚠️ **正式環境必須透過 Web.config 停用 Mock 登入**

### 3. 6碼郵遞區號支援與驗證優化 (v1.1 更新)

- ✅ `MyKeyMapDAO.cs` - 支援 5 碼及 6 碼郵遞區號查詢
- ✅ `ZIPCODE6.cs` - 新增 6 碼郵遞區號 Entity - **新增檔案**
- ✅ `StaticCodeMap_TableName.cs` - 新增 ZIPCODE6 表名稱定義
- ✅ `AjaxController.cs` - 郵遞區號驗證優化，查無資料時不清空欄位
- ✅ `global.js` - 前端驗證優化，不再彈出警告訊息
- ⚠️ **需先執行資料庫 Migration** (建立 ZIPCODE6 表)

### 4. 管理後台功能修正

| 功能模組 | 修正內容 |
|---------|---------|
| A2/C102M | 病歷補件管理 - 視圖修正 |
| A3/C101M | 繳費明細管理 - 視圖修正 |
| A3/C104M | 繳費作業 Index1/Index2 修正 |
| A4/C102M | 業務管理 Index1/Index2 修正 |
| SqlMaps  | A2.xml, A5.xml, A6.xml SQL 修正 |
| 登入頁面 | Login.cshtml 修正 |
| 設定檔   | properties.config, Web.config 修正 |

---

## 部署檔案清單

### 核心安全檔案 (必要) ⚠️

| 檔案路徑 | 說明 | 狀態 |
|---------|------|:----:|
| `Commons/FileSecurityHelper.cs` | 集中式安全驗證工具 | **新增** |
| `Controllers/AjaxController.cs` | AJAX 上傳 + CSRF 保護 | 修改 |
| `Areas/A2/Controllers/C102MController.cs` | 病歷補上傳驗證 | 修改 |
| `Areas/A3/Controllers/C101MController.cs` | 繳費匯入驗證 | 修改 |

### 視圖檔案 (必要) - 需保持 UTF-8 BOM 編碼

| 檔案路徑 | 說明 | 編碼 |
|---------|------|:----:|
| `Views/Home/Login.cshtml` | 登入頁面 | UTF-8 BOM |
| `Views/Home/MockLogin.cshtml` | Mock 登入頁面 | **新增** |
| `Views/Home/MockLoginAdmin.cshtml` | Mock 管理員登入 | **新增** |
| `Areas/A2/Views/C102M/Index.cshtml` | 病歷補件列表 | UTF-8 BOM |
| `Areas/A2/Views/C102M/Upload.cshtml` | 病歷補件上傳 (安全強化) | UTF-8 BOM |
| `Areas/A3/Views/C101M/Index.cshtml` | 繳費明細列表 | UTF-8 BOM |
| `Areas/A3/Views/C101M/ImportDat.cshtml` | 繳費匯入 (安全強化) | UTF-8 BOM |
| `Areas/A3/Views/C104M/Index1.cshtml` | 繳費作業 1 | UTF-8 BOM |
| `Areas/A3/Views/C104M/Index2.cshtml` | 繳費作業 2 | UTF-8 BOM |
| `Areas/A4/Views/C102M/Index1.cshtml` | 業務管理 1 | UTF-8 BOM |
| `Areas/A4/Views/C102M/Index2.cshtml` | 業務管理 2 | UTF-8 BOM |
| `Views/Shared/EditorTemplates/DynamicEFileGrid.cshtml` | 動態上傳元件 (CSRF) | UTF-8 BOM |

### JavaScript 檔案 (必要)

| 檔案路徑 | 說明 |
|---------|------|
| `Scripts/global.js` | 全域 JS (ajaxUploadFile CSRF 支援 + 郵遞區號驗證優化) |

### SQL Maps (必要)

| 檔案路徑 | 說明 |
|---------|------|
| `SqlMaps/A2.xml` | A2 模組 SQL |
| `SqlMaps/A5.xml` | A5 模組 SQL |
| `SqlMaps/A6.xml` | A6 模組 SQL |

### 設定檔 (必要)

| 檔案路徑 | 說明 |
|---------|------|
| `Web.config` | 網站設定 |
| `properties.config` | 應用程式設定 |

### 專案檔 (必要)

| 檔案路徑 | 說明 |
|---------|------|
| `EECOnline.csproj` | 專案檔 (包含 FileSecurityHelper.cs) |

### Model 及 Entity 檔案 (必要)

| 檔案路徑 | 說明 | 狀態 |
|---------|------|:----:|
| `Models/Entities/ZIPCODE6.cs` | 6碼郵遞區號 Entity | **新增** |
| `Models/SessionModel.cs` | Session 模型 | 修改 |
| `Models/base/ConfigModel.cs` | 設定模型 | 修改 |
| `Areas/A2/Models/C101MViewModel.cs` | A2 C101M ViewModel | 修改 |
| `Areas/A2/Models/C102MViewModel.cs` | A2 C102M ViewModel | 修改 |

### DAO 及 Common 檔案 (必要)

| 檔案路徑 | 說明 | 狀態 |
|---------|------|:----:|
| `DataLayers/MyKeyMapDAO.cs` | 6碼郵遞區號查詢支援 | 修改 |
| `Commons/StaticCodeMap_TableName.cs` | 新增 ZIPCODE6 表名稱 | 修改 |

### 控制器檔案 (必要)

| 檔案路徑 | 說明 |
|---------|------|
| `Controllers/HomeController.cs` | 首頁控制器 (Mock 登入) |
| `Areas/A3/Controllers/C104MController.cs` | 繳費作業控制器 |
| `Areas/A4/Controllers/C102MController.cs` | 業務管理控制器 |

### 編譯輸出 (部署時產生)

| 檔案路徑 | 說明 |
|---------|------|
| `bin/EECOnline.dll` | 主要應用程式組件 |
| `bin/EECOnline.pdb` | 除錯符號檔 (選用) |

---

### 資料庫 Migration 腳本

| 檔案路徑 | 說明 |
|---------|------|
| `scripts/migration/001_create_zipcode6_table.sql` | 建立 ZIPCODE6 表 |
| `scripts/migration/003_verify_zipcode6.sql` | 驗證 ZIPCODE6 表 |
| `scripts/migration/004_rollback_zipcode6.sql` | 回滾 ZIPCODE6 表 |

---

## 部署方式

### ⚠️ 重要提醒

1. **UTF-8 BOM 編碼**: 所有 `.cshtml` 檔案必須保持 UTF-8 with BOM 編碼
2. **正式環境無網際網路**: 所有相依套件已包含在 `bin/` 目錄
3. **此為獨立部署**: 與 e-service (民眾端) 分開部署
4. **停用 Mock 登入**: 正式環境必須在 Web.config 中停用
5. **資料庫 Migration**: 部署前須先執行 `001_create_zipcode6_table.sql`

---

### 方式一：使用自動化腳本 (建議)

#### 執行步驟

```powershell
# 1. 以系統管理員身分開啟 PowerShell

# 2. 切換到部署目錄
cd F:\AITest\MOH-CodeBaseNew\deploy\trunk-20251206

# 3. 執行 Build 腳本 (本機開發環境)
.\build-release.ps1 -ProjectPath "..\..\trunk\EECOnline.csproj" -OutputPath ".\build-output"

# 4. 執行部署腳本 (正式環境伺服器)
.\deploy.ps1 `
    -SourcePath ".\build-output" `
    -TargetPath "D:\WebSite\trunk" `
    -BackupPath "D:\Backup\trunk" `
    -SiteName "trunk" `
    -AppPoolName "trunkPool"
```

---

### 方式二：手動部署

#### Step 1: 備份現有檔案

```powershell
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "D:\Backup\trunk\$timestamp"
$targetPath = "D:\WebSite\trunk"

# 建立備份目錄結構
$dirs = @(
    "Commons", "Controllers", "Models", "Models\base", "Scripts", "SqlMaps", "bin",
    "Areas\A2\Controllers", "Areas\A2\Models", "Areas\A2\Views\C102M",
    "Areas\A3\Controllers", "Areas\A3\Views\C101M", "Areas\A3\Views\C104M",
    "Areas\A4\Controllers", "Areas\A4\Views\C102M",
    "Views\Home", "Views\Shared\EditorTemplates"
)

foreach ($dir in $dirs) {
    New-Item -ItemType Directory -Path "$backupDir\$dir" -Force | Out-Null
}

# 備份檔案清單
$filesToBackup = @(
    "Commons\FileSecurityHelper.cs",
    "Controllers\HomeController.cs",
    "Controllers\AjaxController.cs",
    "Models\SessionModel.cs",
    "Models\base\ConfigModel.cs",
    "Scripts\global.js",
    "SqlMaps\A2.xml",
    "SqlMaps\A5.xml",
    "SqlMaps\A6.xml",
    "Web.config",
    "properties.config",
    "EECOnline.csproj",
    "Areas\A2\Controllers\C102MController.cs",
    "Areas\A2\Models\C101MViewModel.cs",
    "Areas\A2\Models\C102MViewModel.cs",
    "Areas\A2\Views\C102M\Index.cshtml",
    "Areas\A2\Views\C102M\Upload.cshtml",
    "Areas\A3\Controllers\C101MController.cs",
    "Areas\A3\Controllers\C104MController.cs",
    "Areas\A3\Views\C101M\Index.cshtml",
    "Areas\A3\Views\C101M\ImportDat.cshtml",
    "Areas\A3\Views\C104M\Index1.cshtml",
    "Areas\A3\Views\C104M\Index2.cshtml",
    "Areas\A4\Controllers\C102MController.cs",
    "Areas\A4\Views\C102M\Index1.cshtml",
    "Areas\A4\Views\C102M\Index2.cshtml",
    "Views\Home\Login.cshtml",
    "Views\Home\MockLogin.cshtml",
    "Views\Home\MockLoginAdmin.cshtml",
    "Views\Shared\EditorTemplates\DynamicEFileGrid.cshtml",
    "bin\EECOnline.dll",
    "bin\EECOnline.pdb"
)

foreach ($file in $filesToBackup) {
    $targetFile = Join-Path $targetPath $file
    if (Test-Path $targetFile) {
        $backupFile = Join-Path $backupDir $file
        Copy-Item -Path $targetFile -Destination $backupFile -Force
        Write-Host "已備份: $file" -ForegroundColor Cyan
    }
}

Write-Host "備份完成: $backupDir" -ForegroundColor Green
```

#### Step 2: 放置 app_offline.htm (暫停網站)

```powershell
$targetPath = "D:\WebSite\trunk"
$appOfflinePath = "$targetPath\app_offline.htm"

$content = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>系統維護中</title>
    <style>
        body { 
            font-family: "Microsoft JhengHei", Arial; 
            text-align: center; 
            padding: 50px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }
        .container {
            background: white;
            color: #333;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            max-width: 600px;
            margin: 0 auto;
        }
        h1 { color: #667eea; }
        .icon { font-size: 60px; margin-bottom: 20px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="icon">🔧</div>
        <h1>系統維護中</h1>
        <p>系統正在進行安全性更新，請稍候再試。</p>
        <p><strong>預計維護時間：5 分鐘</strong></p>
    </div>
</body>
</html>
"@

Set-Content -Path $appOfflinePath -Value $content -Encoding UTF8
Write-Host "網站已暫停" -ForegroundColor Yellow
Start-Sleep -Seconds 3
```

#### Step 3: 複製更新檔案

```powershell
$sourcePath = ".\source"  # 或本機 build 輸出目錄
$targetPath = "D:\WebSite\trunk"

# 複製所有檔案 (使用備份清單)
$filesToDeploy = @(
    @{ File = "Commons\FileSecurityHelper.cs"; IsNew = $true },
    @{ File = "Controllers\HomeController.cs"; IsNew = $false },
    @{ File = "Controllers\AjaxController.cs"; IsNew = $false },
    # ... (完整清單參考備份清單)
)

foreach ($item in $filesToDeploy) {
    $sourceFile = Join-Path $sourcePath $item.File
    $targetFile = Join-Path $targetPath $item.File
    
    if (Test-Path $sourceFile) {
        # 確保目標目錄存在
        $targetDir = Split-Path $targetFile -Parent
        if (!(Test-Path $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        
        Copy-Item -Path $sourceFile -Destination $targetFile -Force
        
        if ($item.IsNew) {
            Write-Host "[NEW] $($item.File)" -ForegroundColor Green
        } else {
            Write-Host "[MOD] $($item.File)" -ForegroundColor Cyan
        }
    }
}

Write-Host "檔案複製完成" -ForegroundColor Green
```

#### Step 4: 移除 app_offline.htm (恢復網站)

```powershell
Remove-Item "D:\WebSite\trunk\app_offline.htm" -Force
Write-Host "網站已恢復" -ForegroundColor Green
```

#### Step 5: 回收應用程式集區

```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name "trunkPool"
Write-Host "應用程式集區已回收" -ForegroundColor Green
```

---

## 還原 (Rollback)

### 使用還原腳本

```powershell
.\rollback.ps1 `
    -BackupDir "D:\Backup\trunk\20251206_143000" `
    -TargetPath "D:\WebSite\trunk" `
    -AppPoolName "trunkPool"
```

### 手動還原

```powershell
$backupDir = "D:\Backup\trunk\20251206_143000"
$targetPath = "D:\WebSite\trunk"

# 還原所有檔案
Get-ChildItem -Path $backupDir -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($backupDir.Length + 1)
    $targetFile = Join-Path $targetPath $relativePath
    
    $targetDir = Split-Path $targetFile -Parent
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    
    Copy-Item -Path $_.FullName -Destination $targetFile -Force
    Write-Host "還原: $relativePath" -ForegroundColor Cyan
}

Restart-WebAppPool -Name "trunkPool"
Write-Host "還原完成" -ForegroundColor Green
```

---

## 部署後驗證

### 1. 安全性測試 (關鍵!) ⚠️

使用測試帳號 `testadmin` / `Test@1234` 登入：

| 測試項目                   | 測試路徑                | 預期結果                |
| -------------------------- | ----------------------- | ----------------------- |
| **病歷補上傳 - 正常上傳**  | A2/C102M/Upload         | PDF/JPG 上傳成功        |
| **病歷補上傳 - 惡意檔案**  | 上傳 fake.pdf           | Magic Bytes 驗證失敗    |
| **病歷補上傳 - Webshell**  | 上傳 shell.aspx.pdf     | 雙重副檔名偵測攔截      |
| **繳費匯入 - 正常上傳**    | A3/C101M/ImportDat      | DAT 檔案匯入成功        |
| **繳費匯入 - 錯誤格式**    | 上傳 malware.exe        | 前端立即拒絕            |
| **CSRF 攻擊**              | 無 Token 的 AJAX 請求   | 400 Bad Request         |
| ***.* 繞過測試**           | 透過「所有檔案」選擇exe | 前端即時拒絕            |

### 2. 功能測試

| 測試項目     | 測試步驟                       | 預期結果         |
| ------------ | ------------------------------ | ---------------- |
| 登入功能     | 使用 testadmin 登入            | 登入成功         |
| 病歷補件     | A2/C102M 列表及上傳            | 正常運作         |
| 繳費明細     | A3/C101M 列表及匯入            | 正常運作         |
| 繳費作業     | A3/C104M Index1/Index2         | 正常運作         |
| 業務管理     | A4/C102M Index1/Index2         | 正常運作         |

### 3. 編碼檢查

```powershell
$files = @(
    "D:\WebSite\trunk\Areas\A2\Views\C102M\Upload.cshtml",
    "D:\WebSite\trunk\Areas\A3\Views\C101M\ImportDat.cshtml",
    "D:\WebSite\trunk\Views\Shared\EditorTemplates\DynamicEFileGrid.cshtml"
)

foreach ($file in $files) {
    $bytes = Get-Content -Path $file -Encoding Byte -TotalCount 3
    if ($bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191) {
        Write-Host "OK: $file" -ForegroundColor Green
    } else {
        Write-Host "ERROR: $file (Missing UTF-8 BOM!)" -ForegroundColor Red
    }
}
```

---

## 停機時間說明

| 部署方式         | 預估停機時間 | 說明                                     |
| ---------------- | ------------ | ---------------------------------------- |
| 自動化腳本       | **< 30 秒**  | 使用 app_offline.htm                     |
| 手動部署         | **< 2 分鐘** | 檔案較多                                 |
| 應用程式集區回收 | **< 5 秒**   | 僅回收集區                               |

---

## 安全性改善總結

### 多層防禦架構

```
┌─────────────────────────────────────────────────────────────┐
│ Layer 1: 客戶端 JavaScript 驗證                                │
│ ✅ 檔案選擇時立即檢查 (change 事件)                            │
│ ✅ 提交前再次檢查 (doSave 函數)                                │
│ → 攔截: 錯誤副檔名、超大檔案、*.* 繞過                          │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ Layer 2: 瀏覽器安全政策                                         │
│ ✅ CORS + Same-Origin Policy                                 │
│ → 攔截: 跨站請求、惡意網站攻擊                                  │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ Layer 3: CSRF Token 驗證                                       │
│ ✅ [ValidateAntiForgeryToken] 屬性                           │
│ → 攔截: 偽造請求、無 Token 請求                                 │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ Layer 4: FileSecurityHelper 伺服器驗證                          │
│ ✅ MIME 類型驗證                                              │
│ ✅ Magic Bytes 驗證 (檔案頭)                                  │
│ ✅ 危險副檔名黑名單 (12+ 類別)                                 │
│ ✅ 雙重副檔名檢測                                              │
│ → 攔截: 偽造檔案、Webshell、惡意程式                            │
└─────────────────────────────────────────────────────────────┘
                              ↓
                    ✅ 檔案安全上傳成功
```

### 已實作安全功能

| 功能                       | 狀態 | 測試狀態 |
| -------------------------- | :--: | :------: |
| MIME 類型驗證              |  ✅  | 待測試   |
| Magic Bytes 驗證           |  ✅  | 待測試   |
| 危險副檔名黑名單           |  ✅  | 待測試   |
| 雙重副檔名檢測             |  ✅  | 待測試   |
| CSRF Token 保護            |  ✅  | ✅已驗證 |
| *.* 選項防護               |  ✅  | ✅已實作 |

---

## 注意事項

### ⚠️ 正式環境停用 Mock 登入

在 `Web.config` 中確認以下設定：

```xml
<appSettings>
    <!-- 正式環境必須設為 false -->
    <add key="EnableMockLogin" value="false" />
</appSettings>
```

### ⚠️ UTF-8 BOM 編碼要求

所有 `.cshtml` 檔案必須保持 **UTF-8 with BOM** 編碼！

### ⚠️ MSBuild 路徑

使用 Visual Studio 2019 MSBuild：
```
C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe
```
⚠️ VS 2022 BuildTools 缺少 WebApplication targets，不可使用！

---

## 相關文件

- [檔案上傳安全性改善計畫](../../documents/plans/檔案上傳安全性改善計畫.md)
- [檔案上傳安全性實作說明](../../documents/plans/檔案上傳安全性實作說明.md)
- [Mock登入測試用戶清單](../../documents/bug-fix/Mock登入測試用戶清單.md)
- [AGENTS.md](../../AGENTS.md) - 開發規範

---

## 變更記錄

| 日期       | 版本 | 變更內容                                           |
| ---------- | :--: | -------------------------------------------------- |
| 2025-12-06 | 1.0  | 初版 - 檔案上傳安全性 + Mock登入 + 管理功能修正    |
| 2025-12-06 | 1.1  | 6碼郵遞區號支援 + 郵遞區號驗證優化                 |

---

_文件更新日期: 2025-12-06_  
_部署包版本: trunk-20251206 v1.1_  
_影響範圍: trunk (管理後台)_
