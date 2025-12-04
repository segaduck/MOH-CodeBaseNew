#Requires -RunAsAdministrator
<#
.SYNOPSIS
    還原部署 - 將網站還原到之前的版本
    Rollback deployment - restore website to previous version

.DESCRIPTION
    此腳本從備份目錄還原檔案到正式環境

.PARAMETER BackupDir
    備份目錄路徑 (包含時間戳記的完整路徑)

.PARAMETER TargetPath
    目標部署路徑 (正式環境網站目錄)

.PARAMETER AppPoolName
    應用程式集區名稱 (預設: eServicePool)

.EXAMPLE
    .\rollback-deployment.ps1 -BackupDir "D:\Backup\e-service\20251204_143000" -TargetPath "D:\WebSite\e-service"

.NOTES
    版本: 1.0
    建立日期: 2025-12-04
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$BackupDir,
    [Parameter(Mandatory=$true)]
    [string]$TargetPath,
    [string]$AppPoolName = "eServicePool"
)

$ErrorActionPreference = "Stop"

function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[SUCCESS] $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "[WARNING] $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

Write-Info "=========================================="
Write-Info "開始還原部署"
Write-Info "備份來源: $BackupDir"
Write-Info "還原目標: $TargetPath"
Write-Info "=========================================="

if (!(Test-Path $BackupDir)) {
    Write-Error "備份目錄不存在: $BackupDir"
    exit 1
}

try {
    # 放置 app_offline.htm
    Write-Info "暫停網站..."
    $appOfflinePath = Join-Path $TargetPath "app_offline.htm"
    $appOfflineContent = @"
<!DOCTYPE html>
<html>
<head><meta charset="utf-8"><title>系統維護中</title></head>
<body><h1>系統維護中，請稍候...</h1></body>
</html>
"@
    Set-Content -Path $appOfflinePath -Value $appOfflineContent -Encoding UTF8
    Start-Sleep -Seconds 3

    # 還原檔案
    Write-Info "還原檔案..."
    $filesToRestore = Get-ChildItem -Path $BackupDir -Recurse -File
    foreach ($file in $filesToRestore) {
        $relativePath = $file.FullName.Replace($BackupDir, "").TrimStart("\")
        $targetFile = Join-Path $TargetPath $relativePath
        Copy-Item -Path $file.FullName -Destination $targetFile -Force
        Write-Info "  已還原: $relativePath"
    }

    # 移除 app_offline.htm
    Write-Info "恢復網站..."
    Remove-Item -Path $appOfflinePath -Force -ErrorAction SilentlyContinue

    # 回收應用程式集區
    Write-Info "回收應用程式集區..."
    Import-Module WebAdministration -ErrorAction SilentlyContinue
    if (Get-Command Restart-WebAppPool -ErrorAction SilentlyContinue) {
        Restart-WebAppPool -Name $AppPoolName
    } else {
        & "C:\Windows\System32\inetsrv\appcmd.exe" recycle apppool /apppool.name:$AppPoolName
    }

    Write-Success "還原完成！"
}
catch {
    Write-Error "還原失敗: $_"
    # 確保網站恢復
    $appOfflinePath = Join-Path $TargetPath "app_offline.htm"
    Remove-Item -Path $appOfflinePath -Force -ErrorAction SilentlyContinue
    exit 1
}

