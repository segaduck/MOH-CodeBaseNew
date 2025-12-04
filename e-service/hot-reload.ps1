Write-Host "=== ASP.NET 熱重載 ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: 建立 app_offline.htm 暫停應用程式
Write-Host "Step 1: 暫停應用程式..." -ForegroundColor Yellow
$appOfflineContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Updating...</title>
</head>
<body>
    <h1>System Updating...</h1>
    <p>Please wait...</p>
</body>
</html>
"@

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appOfflinePath = Join-Path $scriptDir "ES\app_offline.htm"

Set-Content -Path $appOfflinePath -Value $appOfflineContent -Encoding UTF8
Write-Host "  ✓ 應用程式已暫停" -ForegroundColor Green

# Step 2: 等待 2 秒讓 ASP.NET 卸載應用程式
Write-Host ""
Write-Host "Step 2: 等待應用程式卸載..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Write-Host "  ✓ 應用程式已卸載" -ForegroundColor Green

# Step 3: 移除 app_offline.htm 恢復應用程式
Write-Host ""
Write-Host "Step 3: 恢復應用程式..." -ForegroundColor Yellow
Remove-Item $appOfflinePath -Force
Write-Host "  ✓ 應用程式已恢復，變更已載入" -ForegroundColor Green

Write-Host ""
Write-Host "=== 熱重載完成 ===" -ForegroundColor Green
Write-Host "請重新整理瀏覽器查看變更" -ForegroundColor Cyan

