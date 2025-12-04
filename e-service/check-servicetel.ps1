Write-Host "=== 更新 SERVICETEL 設定值 ===" -ForegroundColor Cyan

# 更新電話號碼
$updateQuery = "UPDATE SETUP SET SETUP_VAL = '(02)7730-7378', UPD_TIME = GETDATE(), UPD_FUN_CD = 'DEPLOY-20251204', UPD_ACC = 'SYSTEM' WHERE SETUP_CD = 'SERVICETEL'"
docker exec moh-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -d eservice_new -C -Q $updateQuery

Write-Host ""
Write-Host "=== 驗證更新結果 ===" -ForegroundColor Cyan

# 查詢更新後的結果
$selectQuery = "SELECT SETUP_CD, SETUP_DESC, SETUP_VAL FROM SETUP WHERE SETUP_CD = 'SERVICETEL'"
docker exec moh-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -d eservice_new -C -Q $selectQuery

