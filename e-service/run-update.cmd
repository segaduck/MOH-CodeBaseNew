@echo off
echo === 更新 SERVICETEL 設定值 ===
docker exec moh-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -d eservice_new -C -i /var/opt/mssql/scripts/update-servicetel.sql
echo.
echo === 更新完成 ===

