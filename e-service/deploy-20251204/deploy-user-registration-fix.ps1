#Requires -RunAsAdministrator
<#
.SYNOPSIS
    部署「新使用者註冊修正」更新到正式環境
    Deploy "User Registration Fix" update to production environment

.DESCRIPTION
    此腳本執行以下操作：
    1. 備份現有檔案
    2. 使用 app_offline.htm 優雅地暫停網站
    3. 複製更新檔案
    4. 移除 app_offline.htm 恢復網站
    5. 回收應用程式集區（非重啟整個系統）

.PARAMETER SiteName
    IIS 網站名稱 (預設: e-service)

.PARAMETER AppPoolName
    應用程式集區名稱 (預設: eServicePool)

.PARAMETER SourcePath
    來源檔案路徑 (本機 build 輸出目錄)

.PARAMETER TargetPath
    目標部署路徑 (正式環境網站目錄)

.PARAMETER BackupPath
    備份檔案存放路徑

.EXAMPLE
    .\deploy-user-registration-fix.ps1 -SourcePath "C:\Build\e-service" -TargetPath "D:\WebSite\e-service" -BackupPath "D:\Backup"

.NOTES
    版本: 1.1
    建立日期: 2025-12-04
    修改項目: 個人會員電話號碼非必填、郵遞區號6碼、頁尾新增維護廠商資訊
#>

param(
    [string]$SiteName = "e-service",
    [string]$AppPoolName = "eServicePool",
    [Parameter(Mandatory=$true)]
    [string]$SourcePath,
    [Parameter(Mandatory=$true)]
    [string]$TargetPath,
    [string]$BackupPath = "D:\Backup\e-service"
)

# 設定錯誤處理
$ErrorActionPreference = "Stop"

# 顏色輸出函數
function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[SUCCESS] $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "[WARNING] $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

# 開始部署
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = Join-Path $BackupPath $timestamp

Write-Info "=========================================="
Write-Info "開始部署 - 新使用者註冊修正"
Write-Info "時間: $(Get-Date)"
Write-Info "=========================================="

# 要更新的檔案清單
$filesToDeploy = @(
    @{ Source = "bin\ES.dll"; Description = "主要應用程式組件" },
    @{ Source = "bin\ES.pdb"; Description = "除錯符號檔" },
    @{ Source = "Views\Login\New.cshtml"; Description = "新使用者註冊頁面" },
    @{ Source = "Views\Login\Edit1.cshtml"; Description = "會員資料編輯頁面" },
    @{ Source = "Views\ServiceLst\Notice.cshtml"; Description = "服務項目說明頁面" },
    @{ Source = "Views\Shared\_Footer.cshtml"; Description = "頁尾共用元件" }
)

try {
    # Step 1: 建立備份目錄
    Write-Info "Step 1: 建立備份目錄..."
    if (!(Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $backupDir "bin") -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $backupDir "Views\Login") -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $backupDir "Views\ServiceLst") -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $backupDir "Views\Shared") -Force | Out-Null
    }
    Write-Success "備份目錄已建立: $backupDir"

    # Step 2: 備份現有檔案
    Write-Info "Step 2: 備份現有檔案..."
    foreach ($file in $filesToDeploy) {
        $targetFile = Join-Path $TargetPath $file.Source
        if (Test-Path $targetFile) {
            $backupFile = Join-Path $backupDir $file.Source
            Copy-Item -Path $targetFile -Destination $backupFile -Force
            Write-Info "  已備份: $($file.Source)"
        }
    }
    Write-Success "備份完成"

    # Step 3: 放置 app_offline.htm (優雅停止網站)
    Write-Info "Step 3: 暫停網站 (使用 app_offline.htm)..."
    $appOfflinePath = Join-Path $TargetPath "app_offline.htm"
    $appOfflineTemplatePath = Join-Path $TargetPath "backup_app_offline.htm.template"

    # 優先使用網站目錄中的模板檔案
    if (Test-Path $appOfflineTemplatePath) {
        Copy-Item -Path $appOfflineTemplatePath -Destination $appOfflinePath -Force
        Write-Info "  使用模板檔案: backup_app_offline.htm.template"
    } else {
        # 如果模板不存在，使用內建內容
        Write-Warning "  模板檔案不存在，使用內建維護頁面"
        $appOfflineContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>系統維護中</title>
</head>
<body>
    <h1>系統維護中</h1>
    <p>系統正在進行更新維護，請稍候再試。</p>
</body>
</html>
"@
        Set-Content -Path $appOfflinePath -Value $appOfflineContent -Encoding UTF8
    }
    Write-Success "網站已暫停"
    
    # 等待現有請求完成
    Start-Sleep -Seconds 3

    # Step 4: 複製更新檔案
    Write-Info "Step 4: 部署更新檔案..."
    foreach ($file in $filesToDeploy) {
        $sourceFile = Join-Path $SourcePath $file.Source
        $targetFile = Join-Path $TargetPath $file.Source
        if (Test-Path $sourceFile) {
            Copy-Item -Path $sourceFile -Destination $targetFile -Force
            Write-Success "  已部署: $($file.Description) ($($file.Source))"
        } else {
            Write-Warning "  來源檔案不存在: $sourceFile"
        }
    }

    # Step 5: 移除 app_offline.htm (恢復網站)
    Write-Info "Step 5: 恢復網站..."
    Remove-Item -Path $appOfflinePath -Force -ErrorAction SilentlyContinue
    Write-Success "網站已恢復"

    # Step 6: 回收應用程式集區 (非重啟系統)
    Write-Info "Step 6: 回收應用程式集區..."
    Import-Module WebAdministration -ErrorAction SilentlyContinue
    if (Get-Command Restart-WebAppPool -ErrorAction SilentlyContinue) {
        Restart-WebAppPool -Name $AppPoolName
        Write-Success "應用程式集區已回收: $AppPoolName"
    } else {
        # 使用 appcmd 作為備援
        $appcmd = "C:\Windows\System32\inetsrv\appcmd.exe"
        & $appcmd recycle apppool /apppool.name:$AppPoolName
        Write-Success "應用程式集區已回收 (使用 appcmd): $AppPoolName"
    }

    Write-Info "=========================================="
    Write-Success "部署完成！"
    Write-Info "備份位置: $backupDir"
    Write-Info "完成時間: $(Get-Date)"
    Write-Info "=========================================="
}
catch {
    Write-Error "部署失敗: $_"

    # 確保移除 app_offline.htm
    $appOfflinePath = Join-Path $TargetPath "app_offline.htm"
    if (Test-Path $appOfflinePath) {
        Remove-Item -Path $appOfflinePath -Force -ErrorAction SilentlyContinue
        Write-Warning "已移除 app_offline.htm，網站已恢復"
    }

    Write-Warning "如需還原，請執行: .\rollback-deployment.ps1 -BackupDir '$backupDir' -TargetPath '$TargetPath'"
    exit 1
}

