# 資料庫更新變更記錄

## 更新日期
2025-12-04

## 更新項目
更新系統操作服務諮詢電話

## 變更詳情

### 資料表: SETUP

| 欄位       | 舊值            | 新值            |
| ---------- | --------------- | --------------- |
| SETUP_CD   | SERVICETEL      | SERVICETEL      |
| SETUP_VAL  | (02)8590-6349   | (02)7730-7378   |
| UPD_TIME   | (舊時間)        | 2025-12-04      |
| UPD_FUN_CD | (舊值)          | DEPLOY-20251204 |
| UPD_ACC    | (舊值)          | SYSTEM          |

## 影響範圍

### 1. 登入失敗錯誤訊息
**位置**: `LoginController.cs` 第 828 行、`LoginDAO.cs` 第 605 行

**舊訊息**:
```
帳號或密碼錯誤! 帳號登入失敗若達五次將被鎖定，請諮詢系統操作服務諮詢電話：(02)8590-6349
```

**新訊息**:
```
帳號或密碼錯誤! 帳號登入失敗若達五次將被鎖定，請諮詢系統操作服務諮詢電話：(02)7730-7378
```

### 2. 帳號鎖定錯誤訊息
**位置**: `LoginController.cs` 第 914 行、`Areas/BACKMIN/Controllers/LoginController.cs` 第 108 行

**舊訊息**:
```
鎖定15分鐘，請諮詢系統操作服務諮詢電話：(02)8590-6349。
```

**新訊息**:
```
鎖定15分鐘，請諮詢系統操作服務諮詢電話：(02)7730-7378。
```

## 部署方式

### 開發環境 (已完成)
```powershell
# 已於 2025-12-04 14:43 執行
docker exec moh-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -d eservice_new -C -Q "UPDATE SETUP SET SETUP_VAL = '(02)7730-7378', UPD_TIME = GETDATE(), UPD_FUN_CD = 'DEPLOY-20251204', UPD_ACC = 'SYSTEM' WHERE SETUP_CD = 'SERVICETEL'"
```

### 正式環境
```sql
-- 執行 update-database-servicetel.sql
sqlcmd -S <ServerName> -d eservice_new -i update-database-servicetel.sql
```

## 驗證方式

### 1. 查詢資料庫
```sql
SELECT SETUP_CD, SETUP_DESC, SETUP_VAL 
FROM SETUP 
WHERE SETUP_CD = 'SERVICETEL';
```

**預期結果**: `SETUP_VAL = '(02)7730-7378'`

### 2. 測試登入錯誤訊息
1. 開啟登入頁面
2. 輸入錯誤的帳號密碼
3. 確認錯誤訊息顯示新電話號碼 `(02)7730-7378`

### 3. 測試帳號鎖定訊息
1. 連續輸入錯誤帳號密碼 5 次
2. 確認鎖定訊息顯示新電話號碼 `(02)7730-7378`

## 還原方式

如需還原，執行以下 SQL:

```sql
UPDATE SETUP 
SET SETUP_VAL = '(02)8590-6349',
    UPD_TIME = GETDATE(),
    UPD_FUN_CD = 'ROLLBACK-20251204',
    UPD_ACC = 'SYSTEM'
WHERE SETUP_CD = 'SERVICETEL';
```

## 注意事項

1. **應用程式快取**: 更新資料庫後需清除應用程式快取或重啟應用程式集區
2. **DataUtils.GetConfig()**: 此方法會快取 SETUP 表的設定值，需重啟應用程式才會重新讀取
3. **測試環境**: 建議先在測試環境驗證後再部署到正式環境
4. **備份**: 執行前請先備份 SETUP 表

## 相關檔案

- `update-database-servicetel.sql` - 資料庫更新腳本
- `e-service/check-servicetel.ps1` - 查詢與更新腳本 (開發環境用)
- `e-service/force-reload-app.ps1` - 強制重新載入應用程式腳本

