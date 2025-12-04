@echo off
docker exec moh-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -d eservice_new -C -i /var/opt/mssql/scripts/query-servicetel.sql

