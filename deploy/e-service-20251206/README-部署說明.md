# e-service 民眾端 - 正式環境部署說明

## 版本資訊

| 項目     | 說明                                            |
| -------- | ----------------------------------------------- |
| 版本     | 1.2                                             |
| 日期     | 2025-12-06                                      |
| 系統     | **e-service (民眾端)**                           |
| 修改項目 | 新使用者註冊修正 + 郵遞區號查詢彈窗增強 + Mock登入 + 6碼郵遞區號完整資料 |

---

## 修改內容摘要

### 1. 個人會員電話號碼改為非必填

- **影響範圍**: 僅個人會員 (RPC=P)
- **公司會員**: 公司（商號）電話維持必填
- **修改檔案**: 
  - `Views/Login/New.cshtml` - 新使用者註冊頁面
  - `Views/Login/Edit1.cshtml` - 會員資料編輯頁面

### 2. 郵遞區號改為 6 碼

- **影響範圍**: 所有會員類型
- **前端驗證**: 輸入框改為接受 6 碼
- **後端驗證**: `MyKeyMapDAO.cs` 支援 5 碼及 6 碼查詢
- **資料庫**: 需建立 `ZIPCODE6` 表存放 6 碼資料
- **修改檔案**:
  - `DataLayers/MyKeyMapDAO.cs` - 根據長度自動選擇 ZIPCODE 或 ZIPCODE6 表

### 3. 郵遞區號查詢彈窗增強 (v1.1 新增)

- **功能**: 支援 5 碼 (舊制) 及 6 碼 (新制) 郵遞區號查詢切換
- **UI 改進**: 新增格式選擇 Radio Button (20%/40%/40% flexbox 佈局)
- **初始狀態**: 表格初始為空，點擊搜尋後載入資料
- **切換清空**: 切換格式時自動清空查詢結果
- **驗證優化**: 查無資料時不再顯示警告彈窗，不清空使用者輸入
- **修改檔案**:
  - `Controllers/AJAXController.cs` - 返回 "-" 而非空字串
  - `Controllers/Share/ZIP_COController.cs` - GET 請求返回空表格
  - `DataLayers/MyKeyMapDAO.cs` - 支援 5/6 碼查詢
  - `DataLayers/SHAREDAO.cs` - 根據格式選擇查詢來源表
  - `Helpers/AddrExtension.cs` - 移除 blockAlert 和欄位清空
  - `Models/Share/ZIP_COViewModel.cs` - 新增 ZIP_FORMAT 屬性
  - `Views/ZIP_CO/Index.cshtml` - 新增格式選擇 UI
  - `Views/ZIP_CO/_GridRows.cshtml` - 條件顯示街道名稱欄位

### 4. Mock 登入功能 (v1.1 新增) - 開發測試用

- **功能**: 提供開發環境快速登入測試功能
- **安全**: 僅在 `DevMode=1` 時啟用
- **修改檔案**:
  - `Controllers/MockLoginController.cs` - **新增**
  - `Views/MockLogin/Index.cshtml` - **新增**
  - `Web.config` - 新增 DevMode 設定

### 5. 其他小修改

- `Views/ServiceLst/Notice.cshtml` - 服務公告頁面調整
- `Views/Shared/_Footer.cshtml` - 頁尾修正

---

## 部署檔案清單

### DAO 及 Helper 檔案 (必要)

| 檔案路徑                          | 說明                      | 狀態        |
| --------------------------------- | ------------------------- | :---------: |
| `DataLayers/MyKeyMapDAO.cs`       | 郵遞區號查詢 (5碼+6碼)    | 修改        |
| `DataLayers/SHAREDAO.cs`          | ZIP_CO 查詢支援 5/6 碼    | 修改        |
| `Helpers/AddrExtension.cs`        | 地址控件驗證邏輯優化      | 修改        |

### Controller 檔案 (必要)

| 檔案路徑                              | 說明                      | 狀態        |
| ------------------------------------- | ------------------------- | :---------: |
| `Controllers/AJAXController.cs`       | 郵遞區號驗證 API          | 修改        |
| `Controllers/Share/ZIP_COController.cs` | ZIP_CO 查詢控制器       | 修改        |
| `Controllers/MockLoginController.cs`  | Mock 登入控制器           | **新增**    |

### Model 檔案 (必要)

| 檔案路徑                              | 說明                      | 狀態        |
| ------------------------------------- | ------------------------- | :---------: |
| `Models/Share/ZIP_COViewModel.cs`     | ZIP_CO 視圖模型           | 修改        |

### 視圖檔案 (必要) - 需保持 UTF-8 BOM 編碼

| 檔案路徑                          | 說明                      | 編碼要求    |
| --------------------------------- | ------------------------- | :---------: |
| `Views/Login/New.cshtml`          | 新使用者註冊頁面          | UTF-8 BOM   |
| `Views/Login/Edit1.cshtml`        | 會員資料編輯頁面          | UTF-8 BOM   |
| `Views/ServiceLst/Notice.cshtml`  | 服務公告頁面              | UTF-8 BOM   |
| `Views/Shared/_Footer.cshtml`     | 共用頁尾                  | UTF-8 BOM   |
| `Views/ZIP_CO/Index.cshtml`       | 郵遞區號查詢彈窗          | UTF-8 BOM   |
| `Views/ZIP_CO/_GridRows.cshtml`   | 郵遞區號查詢結果列        | UTF-8 BOM   |
| `Views/MockLogin/Index.cshtml`    | Mock 登入頁面 **(新增)**  | UTF-8 BOM   |

### 設定檔 (必要)

| 檔案路徑                          | 說明                      | 狀態        |
| --------------------------------- | ------------------------- | :---------: |
| `Web.config`                      | 網站設定 (DevMode 設定)   | 修改        |

### 部署工具 (建議)

| 檔案路徑                          | 說明                      |
| --------------------------------- | ------------------------- |
| `backup_app_offline.htm.template` | 維護頁面模板              |

### 編譯輸出 (部署時產生)

| 檔案路徑                          | 說明                      |
| --------------------------------- | ------------------------- |
| `bin/ES.dll`                      | 主要應用程式組件          |
| `bin/ES.pdb`                      | 除錯符號檔 (選用)         |

### 6碼郵遞區號資料匯入 (v1.2 新增)

| 檔案路徑                          | 說明                                    |
| --------------------------------- | --------------------------------------- |
| `insert_zipcode6_full.sql`        | SQL 匯入腳本 (79,861 筆)                |

> **來源**: 中華郵政 3+3 郵遞區號簿 (2504A + 2504B)，原始檔案保存於 `SQL-BAK/`

**欄位對應說明**:
| Excel 欄位 | 資料庫欄位 | 說明                     |
| ---------- | ---------- | ------------------------ |
| Col 0      | CITYNM     | 縣市 (如: 桃園市)        |
| Col 1      | TOWNNM     | 區域 (如: 蘆竹區)        |
| Col 2      | ZIP_CO     | 6碼郵遞區號 (如: 338116) |
| Col 3      | ROADNM     | 街道名稱 (如: 吉林路)    |
| Col 4      | SCOOP      | 門牌範圍 (如: 單 119號至 139號) |

---

## 部署前準備：確認 IIS 網站資訊

> **重要**: 這是接手案例的首次部署，需先確認正式環境的 IIS 設定。

### ⚠️ 必須使用 Windows PowerShell 5.1

`WebAdministration` 模組**不支援 PowerShell 7 (Core)**，必須使用 **Windows PowerShell 5.1**。

```powershell
# 方法 1: 從開始功能表開啟
# 搜尋 "Windows PowerShell" (不是 "PowerShell 7" 或 "pwsh")
# 右鍵 → 以系統管理員身分執行

# 方法 2: 從命令列啟動 Windows PowerShell 5.1
powershell.exe -NoProfile -ExecutionPolicy Bypass

# 方法 3: 確認 PowerShell 版本
$PSVersionTable.PSVersion
# 應顯示 5.1.xxxxx (不是 7.x)
```

### Step 1: 查詢 IIS 網站清單

以**系統管理員**身分開啟 **Windows PowerShell 5.1**，執行以下命令：

```powershell
# 載入 IIS 模組 (僅適用於 Windows PowerShell 5.1)
Import-Module WebAdministration

# 列出所有網站及其實體路徑
Get-Website | Select-Object Name, ID, State, PhysicalPath | Format-Table -AutoSize

# 輸出範例：
# Name         ID State   PhysicalPath
# ----         -- -----   ------------
# Default Web  1  Started C:\inetpub\wwwroot
# e-service    2  Started D:\WebSite\e-service
# trunk        3  Started D:\WebSite\trunk
```

### Step 2: 查詢特定網站的詳細資訊

```powershell
# 查詢 e-service 網站的完整設定
Get-Website -Name "e-service" | Format-List *

# 或使用模糊搜尋 (如果不確定網站名稱)
Get-Website | Where-Object { $_.Name -like "*service*" -or $_.Name -like "*eec*" } | Format-List Name, PhysicalPath, ApplicationPool
```

### Step 3: 查詢應用程式集區資訊

```powershell
# 列出所有應用程式集區
Get-IISAppPool | Select-Object Name, State, ManagedRuntimeVersion | Format-Table -AutoSize

# 查詢特定網站使用的應用程式集區
(Get-Website -Name "e-service").ApplicationPool

# 輸出範例: eServicePool
```

### Step 4: 確認網站綁定 (URL/Port)

```powershell
# 查詢網站綁定資訊
Get-WebBinding -Name "e-service" | Select-Object protocol, bindingInformation | Format-Table -AutoSize

# 輸出範例:
# protocol bindingInformation
# -------- ------------------
# http     *:80:e-service.mohw.gov.tw
# https    *:443:e-service.mohw.gov.tw
```

### Step 5: 驗證實體路徑存在

```powershell
# 取得網站實體路徑
$sitePath = (Get-Website -Name "e-service").PhysicalPath

# 檢查路徑是否存在
if (Test-Path $sitePath) {
    Write-Host "網站路徑存在: $sitePath" -ForegroundColor Green
    
    # 列出關鍵目錄結構
    Write-Host "`n目錄結構:"
    Get-ChildItem $sitePath -Directory | Select-Object Name
    
    # 確認 bin 目錄存在
    if (Test-Path "$sitePath\bin") {
        Write-Host "`nbin 目錄存在，目前的 ES.dll:" -ForegroundColor Green
        Get-ChildItem "$sitePath\bin\ES.dll" | Select-Object Name, LastWriteTime, Length
    }
} else {
    Write-Host "警告: 路徑不存在 - $sitePath" -ForegroundColor Red
}
```

### Step 6: 匯出網站設定資訊 (建議)

```powershell
# 匯出完整的 IIS 設定資訊供記錄
$outputFile = "C:\Temp\IIS-SiteInfo-$(Get-Date -Format 'yyyyMMdd').txt"

"=== IIS 網站設定資訊 ===" | Out-File $outputFile
"產生時間: $(Get-Date)" | Out-File $outputFile -Append
"" | Out-File $outputFile -Append

"--- 網站清單 ---" | Out-File $outputFile -Append
Get-Website | Format-Table Name, ID, State, PhysicalPath -AutoSize | Out-File $outputFile -Append

"--- 應用程式集區 ---" | Out-File $outputFile -Append
Get-IISAppPool | Format-Table Name, State, ManagedRuntimeVersion -AutoSize | Out-File $outputFile -Append

"--- e-service 詳細設定 ---" | Out-File $outputFile -Append
Get-Website -Name "e-service" | Format-List * | Out-File $outputFile -Append

Write-Host "設定已匯出至: $outputFile" -ForegroundColor Green
```

### 替代方案：使用 appcmd (適用於任何 PowerShell 版本)

如果無法使用 `WebAdministration` 模組，可以使用 IIS 內建的 `appcmd.exe`：

> **注意**: 此命令僅適用於**正式環境伺服器** (已安裝 IIS)。  
> 開發電腦如果沒有安裝 IIS，執行這些命令會沒有輸出或找不到檔案。

```powershell
# 確認 IIS 是否安裝
$appcmd = "$env:SystemRoot\System32\inetsrv\appcmd.exe"
if (!(Test-Path $appcmd)) {
    Write-Host "IIS 未安裝 - 請在正式環境伺服器上執行此命令" -ForegroundColor Yellow
} else {
    Write-Host "IIS 已安裝，appcmd 可用" -ForegroundColor Green
}

# 列出所有網站 (如果沒有輸出，表示沒有網站)
& $appcmd list site
# 輸出範例:
# SITE "Default Web Site" (id:1,bindings:http/*:80:,state:Started)
# SITE "e-service" (id:2,bindings:http/*:80:e-service.mohw.gov.tw,state:Started)

# 查詢特定網站詳細資訊 (請將 "e-service" 替換為實際網站名稱)
& $appcmd list site "e-service" /config

# 查詢網站的實體路徑
& $appcmd list vdir /app.name:"e-service/"
# 輸出範例:
# VDIR "e-service/" (physicalPath:D:\WebSite\e-service)
# 
# 如果沒有輸出，表示找不到該網站，請先執行 list site 確認正確的網站名稱

# 列出所有應用程式集區
& $appcmd list apppool
# 輸出範例:
# APPPOOL "DefaultAppPool" (MgdVersion:v4.0,MgdMode:Integrated,state:Started)
# APPPOOL "eServicePool" (MgdVersion:v4.0,MgdMode:Integrated,state:Started)

# 查詢網站使用的應用程式集區
& $appcmd list app "e-service/" /text:applicationPool
# 輸出範例: eServicePool
```

### 確認清單

在開始部署前，請確認以下資訊：

| 項目 | 確認值 | 範例 |
| ---- | ------ | ---- |
| 網站名稱 | __________ | e-service |
| 實體路徑 | __________ | D:\WebSite\e-service |
| 應用程式集區 | __________ | eServicePool |
| 目前 ES.dll 時間 | __________ | 2025/11/01 10:30 |

---

## 部署方式

### ⚠️ 重要提醒

1. **UTF-8 BOM 編碼**: 所有 `.cshtml` 檔案必須保持 UTF-8 with BOM 編碼
2. **正式環境無網際網路**: 所有相依套件必須包含在 `bin/` 目錄
3. **此為獨立部署**: 與 trunk (管理後台) 分開部署
4. **資料庫 Migration**: 需先建立 `ZIPCODE6` 表，請參考 trunk 部署包中的 Migration 腳本

---

### 方式一：使用自動化腳本 (建議)

#### 前置條件

1. 以**系統管理員**身分執行 PowerShell
2. 確認 IIS 網站名稱和應用程式集區名稱
3. 準備好編譯完成的 build 輸出檔案

#### 執行步驟

```powershell
# 1. 以系統管理員身分開啟 PowerShell

# 2. 切換到部署目錄
cd F:\AITest\MOH-CodeBaseNew\deploy\e-service-20251206

# 3. 執行 Build 腳本 (本機開發環境)
.\build-release.ps1 -ProjectPath "..\..\e-service\ES\ES.csproj" -OutputPath ".\build-output"

# 4. 執行部署腳本 (正式環境伺服器)
.\deploy.ps1 `
    -SourcePath ".\build-output" `
    -TargetPath "D:\WebSite\e-service" `
    -BackupPath "D:\Backup\e-service" `
    -SiteName "e-service" `
    -AppPoolName "eServicePool"
```

#### 參數說明

| 參數           | 說明                        | 預設值                |
| -------------- | --------------------------- | --------------------- |
| `-SourcePath`  | Build 輸出目錄 (必填)       | -                     |
| `-TargetPath`  | 正式環境網站目錄 (必填)     | -                     |
| `-BackupPath`  | 備份存放目錄                | `D:\Backup\e-service` |
| `-SiteName`    | IIS 網站名稱                | `e-service`           |
| `-AppPoolName` | 應用程式集區名稱            | `eServicePool`        |

---

### 方式二：手動部署

#### Step 1: 備份現有檔案

```powershell
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "D:\Backup\e-service\$timestamp"
$targetPath = "D:\WebSite\e-service"

# 建立備份目錄
New-Item -ItemType Directory -Path "$backupDir\Views\Login" -Force
New-Item -ItemType Directory -Path "$backupDir\Views\ServiceLst" -Force
New-Item -ItemType Directory -Path "$backupDir\Views\Shared" -Force
New-Item -ItemType Directory -Path "$backupDir\bin" -Force

# 備份檔案
Copy-Item "$targetPath\Views\Login\New.cshtml" "$backupDir\Views\Login\"
Copy-Item "$targetPath\Views\Login\Edit1.cshtml" "$backupDir\Views\Login\"
Copy-Item "$targetPath\Views\ServiceLst\Notice.cshtml" "$backupDir\Views\ServiceLst\"
Copy-Item "$targetPath\Views\Shared\_Footer.cshtml" "$backupDir\Views\Shared\"
Copy-Item "$targetPath\bin\ES.dll" "$backupDir\bin\"
Copy-Item "$targetPath\bin\ES.pdb" "$backupDir\bin\" -ErrorAction SilentlyContinue

Write-Host "備份完成: $backupDir" -ForegroundColor Green
```

#### Step 2: 放置 app_offline.htm (暫停網站)

```powershell
$targetPath = "D:\WebSite\e-service"
$templatePath = "$targetPath\backup_app_offline.htm.template"
$appOfflinePath = "$targetPath\app_offline.htm"

if (Test-Path $templatePath) {
    Copy-Item $templatePath $appOfflinePath
} else {
    # 建立預設維護頁面
    $content = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>系統維護中</title>
    <style>
        body { font-family: "Microsoft JhengHei", Arial, sans-serif; text-align: center; padding: 50px; }
        h1 { color: #333; }
        p { color: #666; }
    </style>
</head>
<body>
    <h1>系統維護中</h1>
    <p>系統正在進行更新維護，請稍候再試。</p>
    <p>預計維護時間：5 分鐘</p>
</body>
</html>
"@
    Set-Content -Path $appOfflinePath -Value $content -Encoding UTF8
}

Write-Host "網站已暫停" -ForegroundColor Yellow
Start-Sleep -Seconds 3
```

#### Step 3: 複製更新檔案

```powershell
$sourcePath = ".\source"  # 或本機 build 輸出目錄
$targetPath = "D:\WebSite\e-service"

# 複製視圖檔案
Copy-Item "$sourcePath\Views\Login\New.cshtml" "$targetPath\Views\Login\" -Force
Copy-Item "$sourcePath\Views\Login\Edit1.cshtml" "$targetPath\Views\Login\" -Force
Copy-Item "$sourcePath\Views\ServiceLst\Notice.cshtml" "$targetPath\Views\ServiceLst\" -Force
Copy-Item "$sourcePath\Views\Shared\_Footer.cshtml" "$targetPath\Views\Shared\" -Force

# 複製編譯輸出
Copy-Item "$sourcePath\bin\ES.dll" "$targetPath\bin\" -Force
Copy-Item "$sourcePath\bin\ES.pdb" "$targetPath\bin\" -Force -ErrorAction SilentlyContinue

Write-Host "檔案複製完成" -ForegroundColor Green
```

#### Step 4: 移除 app_offline.htm (恢復網站)

```powershell
Remove-Item "D:\WebSite\e-service\app_offline.htm" -Force
Write-Host "網站已恢復" -ForegroundColor Green
```

#### Step 5: 回收應用程式集區

```powershell
Import-Module WebAdministration
Restart-WebAppPool -Name "eServicePool"
Write-Host "應用程式集區已回收" -ForegroundColor Green
```

---

## 還原 (Rollback)

### 使用還原腳本

```powershell
.\rollback.ps1 `
    -BackupDir "D:\Backup\e-service\20251206_143000" `
    -TargetPath "D:\WebSite\e-service" `
    -AppPoolName "eServicePool"
```

### 手動還原

```powershell
$backupDir = "D:\Backup\e-service\20251206_143000"  # 替換為實際備份目錄
$targetPath = "D:\WebSite\e-service"

Copy-Item "$backupDir\Views\Login\*" "$targetPath\Views\Login\" -Force
Copy-Item "$backupDir\Views\ServiceLst\*" "$targetPath\Views\ServiceLst\" -Force
Copy-Item "$backupDir\Views\Shared\*" "$targetPath\Views\Shared\" -Force
Copy-Item "$backupDir\bin\*" "$targetPath\bin\" -Force

Restart-WebAppPool -Name "eServicePool"
Write-Host "還原完成" -ForegroundColor Green
```

---

## 部署後驗證

### 1. 功能測試

| 測試項目     | 測試步驟                       | 預期結果         |
| ------------ | ------------------------------ | ---------------- |
| 個人會員註冊 | 不填電話號碼，填寫其他必填欄位 | 可成功註冊       |
| 公司會員註冊 | 不填公司電話                   | 應顯示必填錯誤   |
| 郵遞區號輸入 | 輸入 6 碼郵遞區號              | 可正常輸入並儲存 |
| 會員資料編輯 | 個人會員清空電話欄位           | 可成功儲存       |

### 2. 頁面檢查

- [ ] `/Login/New?num=1&RPC=P` - 個人會員註冊頁面
  - 電話號碼欄位無紅色星號
  - 郵遞區號欄位顯示「郵遞區號 6 碼」

- [ ] `/Login/New?num=1&RPC=C` - 公司會員註冊頁面
  - 公司（商號）電話欄位有紅色星號
  - 郵遞區號欄位顯示「郵遞區號 6 碼」

- [ ] `/Login/Edit1` - 會員資料編輯頁面
  - 依會員類型顯示正確的必填標記

### 3. 編碼檢查

```powershell
# 確認 UTF-8 BOM 存在
$files = @(
    "D:\WebSite\e-service\Views\Login\New.cshtml",
    "D:\WebSite\e-service\Views\Login\Edit1.cshtml"
)

foreach ($file in $files) {
    $bytes = Get-Content -Path $file -Encoding Byte -TotalCount 3
    if ($bytes[0] -eq 239 -and $bytes[1] -eq 187 -and $bytes[2] -eq 191) {
        Write-Host "OK: $file (UTF-8 BOM)" -ForegroundColor Green
    } else {
        Write-Host "WARNING: $file (Missing BOM!)" -ForegroundColor Red
    }
}
```

---

## 停機時間說明

| 部署方式         | 預估停機時間 | 說明                                     |
| ---------------- | ------------ | ---------------------------------------- |
| 自動化腳本       | **< 30 秒**  | 使用 app_offline.htm，使用者看到維護頁面 |
| 手動部署         | **< 1 分鐘** | 依操作熟練度而定                         |
| 應用程式集區回收 | **< 5 秒**   | 僅回收集區，不影響其他網站               |

---

## 注意事項

### ⚠️ UTF-8 BOM 編碼要求 (關鍵!)

所有 `.cshtml` 檔案必須保持 **UTF-8 with BOM** 編碼，否則中文會顯示亂碼！

**檢查方法**:
```powershell
$bytes = Get-Content -Path "file.cshtml" -Encoding Byte -TotalCount 3
# 應顯示: 239 187 191 (即 EF BB BF)
```

**修復方法**:
```powershell
$content = Get-Content -Path "file.cshtml" -Raw -Encoding UTF8
$Utf8Bom = New-Object System.Text.UTF8Encoding $True
[System.IO.File]::WriteAllText("file.cshtml", $content, $Utf8Bom)
```

### ⚠️ 部署前確認

1. ✅ 確認已在測試環境完成測試
2. ✅ 確認有足夠的磁碟空間存放備份
3. ✅ 確認正式環境的 IIS 網站名稱和應用程式集區名稱
4. ✅ 建議在離峰時段執行部署

### ⚠️ 權限需求

- 需要**系統管理員**權限執行 PowerShell 腳本
- 需要 IIS 管理權限 (回收應用程式集區)
- 需要網站目錄的讀寫權限

---

## 相關文件

- [新使用者註冊修正計畫](../../documents/plans/新使用者註冊-修正計畫.md) - 詳細修改說明
- [郵遞區號6碼資料庫實施計畫](../../e-service/deploy-20251204/6碼郵遞區號資料庫實施計畫.md) - 資料庫相關

---

## 變更記錄

| 日期       | 版本 | 變更內容                                 |
| ---------- | :--: | ---------------------------------------- |
| 2025-12-06 | 1.0  | 初版 - 電話非必填 + 郵遞區號6碼          |
| 2025-12-06 | 1.1  | 郵遞區號查詢彈窗增強 + Mock 登入功能     |
| 2025-12-06 | 1.2  | 6碼郵遞區號完整資料 (79,861筆) + 修正欄位對應 |

---

_文件更新日期: 2025-12-06_  
_部署包版本: e-service-20251206 v1.2_  
_影響範圍: e-service (民眾端)_
