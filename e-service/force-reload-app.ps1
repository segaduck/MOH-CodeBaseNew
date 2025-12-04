Write-Host "=== 強制重新載入應用程式 ===" -ForegroundColor Cyan

$appPath = "e-service\ES"
$webConfigPath = Join-Path $appPath "Web.config"
$tempAspNetPath = "$env:LOCALAPPDATA\Temp\Temporary ASP.NET Files"

Write-Host "Step 1: 清除 ASP.NET 編譯快取..." -ForegroundColor Yellow
if (Test-Path $tempAspNetPath) {
    Get-ChildItem $tempAspNetPath -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
    Write-Host "  ✓ 已清除" -ForegroundColor Green
} else {
    Write-Host "  - 目錄不存在" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Step 2: 建立 app_offline.htm 暫停應用程式..." -ForegroundColor Yellow
$appOfflinePath = Join-Path $appPath "app_offline.htm"
$appOfflineContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>系統更新中</title>
</head>
<body>
    <h1>系統更新中，請稍候...</h1>
</body>
</html>
"@
Set-Content -Path $appOfflinePath -Value $appOfflineContent -Encoding UTF8
Write-Host "  ✓ 已建立 app_offline.htm" -ForegroundColor Green

Write-Host ""
Write-Host "Step 3: 等待 3 秒讓應用程式卸載..." -ForegroundColor Yellow
Start-Sleep -Seconds 3
Write-Host "  ✓ 等待完成" -ForegroundColor Green

Write-Host ""
Write-Host "Step 4: 移除 app_offline.htm 恢復應用程式..." -ForegroundColor Yellow
Remove-Item $appOfflinePath -Force
Write-Host "  ✓ 已移除，應用程式正在重新載入" -ForegroundColor Green

Write-Host ""
Write-Host "Step 5: 觸發 Web.config 變更..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$webConfig = Get-Content $webConfigPath -Raw
Set-Content -Path $webConfigPath -Value $webConfig -NoNewline
Write-Host "  ✓ 已觸發 (修改 Web.config 時間: $timestamp)" -ForegroundColor Green

Write-Host ""
Write-Host "=== 應用程式已強制重新載入 ===" -ForegroundColor Green
Write-Host "資料庫中的 SERVICETEL 已更新為: (02)7730-7378" -ForegroundColor Cyan
Write-Host "請重新整理瀏覽器 (Ctrl+F5) 測試登入錯誤訊息" -ForegroundColor Cyan

