<#
.SYNOPSIS
    測試 app_offline.htm 機制
    Test app_offline.htm mechanism

.DESCRIPTION
    此腳本模擬 app_offline.htm 的放置與移除，用於測試部署流程。
    請以系統管理員身分執行。

.PARAMETER WebsitePath
    網站根目錄路徑

.PARAMETER DurationSeconds
    維護頁面顯示時間 (預設: 10 秒)

.EXAMPLE
    .\test-app-offline.ps1 -WebsitePath "F:\AITest\MOH-CodeBaseNew\e-service\ES"

.NOTES
    版本: 1.0
    建立日期: 2025-12-04
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$WebsitePath,
    [int]$DurationSeconds = 10
)

$ErrorActionPreference = "Stop"

function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[SUCCESS] $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "[WARNING] $Message" -ForegroundColor Yellow }

$appOfflinePath = Join-Path $WebsitePath "app_offline.htm"

Write-Info "=========================================="
Write-Info "測試 app_offline.htm 機制"
Write-Info "網站路徑: $WebsitePath"
Write-Info "=========================================="

# 檢查路徑是否存在
if (!(Test-Path $WebsitePath)) {
    Write-Warning "網站路徑不存在: $WebsitePath"
    exit 1
}

# Step 1: 放置 app_offline.htm
Write-Info ""
Write-Info "Step 1: 放置 app_offline.htm..."

$appOfflineTemplatePath = Join-Path $WebsitePath "backup_app_offline.htm.template"

# 優先使用網站目錄中的模板檔案
if (Test-Path $appOfflineTemplatePath) {
    Copy-Item -Path $appOfflineTemplatePath -Destination $appOfflinePath -Force
    Write-Success "app_offline.htm 已放置 (使用模板檔案)"
    Write-Info "模板來源: backup_app_offline.htm.template"
} else {
    Write-Warning "模板檔案不存在: backup_app_offline.htm.template"
    Write-Warning "請先建立模板檔案，或從版本庫取得"
    exit 1
}
Write-Info "檔案位置: $appOfflinePath"

# 提示使用者測試
Write-Info ""
Write-Warning "=========================================="
Write-Warning "現在請開啟瀏覽器訪問網站，您應該會看到維護頁面"
Write-Warning "測試網址: http://localhost:<port>/"
Write-Warning "=========================================="
Write-Info ""
Write-Info "維護頁面將在 $DurationSeconds 秒後移除..."
Write-Info ""

# 倒數計時
for ($i = $DurationSeconds; $i -gt 0; $i--) {
    Write-Host "`r剩餘時間: $i 秒..." -NoNewline
    Start-Sleep -Seconds 1
}
Write-Host ""

# Step 2: 移除 app_offline.htm
Write-Info ""
Write-Info "Step 2: 移除 app_offline.htm..."
Remove-Item -Path $appOfflinePath -Force
Write-Success "app_offline.htm 已移除"

Write-Info ""
Write-Info "=========================================="
Write-Success "測試完成！網站已恢復正常"
Write-Info "=========================================="

