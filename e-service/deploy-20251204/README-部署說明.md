# 新使用者註冊修正 - 正式環境部署說明

## 版本資訊

| 項目     | 說明                                                                                                  |
| -------- | ----------------------------------------------------------------------------------------------------- |
| 版本     | 1.3                                                                                                   |
| 日期     | 2025-12-04                                                                                            |
| 修改項目 | 個人會員電話號碼非必填、郵遞區號改為 6 碼、頁尾及聯絡窗口改為柏通股份有限公司、更新資料庫服務諮詢電話 |

---

## 修改內容摘要

### 1. 個人會員電話號碼改為非必填

- **影響範圍**: 僅個人會員 (RPC=P)
- **公司會員**: 公司（商號）電話維持必填

### 2. 郵遞區號改為 6 碼

- **影響範圍**: 所有會員類型
- **資料庫**: 已確認 `ZIPCODE.ZIP_CO` 欄位為 `nvarchar(10)`，無需修改

### 3. 頁尾新增維護廠商資訊

- **新增內容**: `維護廠商: 柏通股份有限公司    服務專線: 02-7730-7378`
- **位置**: 頁尾「衛生福利部郵遞區號」那行的上方
- **影響範圍**: 所有使用 `_Footer.cshtml` 的頁面

### 4. 聯絡窗口改為柏通股份有限公司

- **修改內容**: 聯絡窗口從「衛生福利部」改為「柏通股份有限公司」
- **影響範圍**: 服務項目說明頁面 (`/Servicelst/notice`)

### 5. 更新資料庫系統操作服務諮詢電話

- **修改內容**: 資料庫 `SETUP` 表中 `SERVICETEL` 設定值從 `(02)8590-6349` 改為 `(02)7730-7378`
- **影響範圍**:
  - 登入失敗錯誤訊息：「帳號或密碼錯誤! 帳號登入失敗若達五次將被鎖定，請諮詢系統操作服務諮詢電話：(02)7730-7378」
  - 帳號鎖定錯誤訊息：「鎖定 15 分鐘，請諮詢系統操作服務諮詢電話：(02)7730-7378。」
- **部署方式**: 執行 SQL 腳本 `update-database-servicetel.sql`

---

## 部署檔案清單

### 本部署套件結構

```
e-service/deploy-20251204/
├── README-部署說明.md                  ← 本說明文件
├── backup_app_offline.htm.template     ← 維護頁面模板
├── build-release.ps1                   ← 編譯腳本
├── deploy-user-registration-fix.ps1    ← 部署腳本
├── rollback-deployment.ps1             ← 還原腳本
├── test-app-offline.ps1                ← 測試腳本
├── update-database-servicetel.sql      ← 資料庫更新腳本 (新增)
└── source/
    ├── bin/
    │   ├── ES.dll                      ← 主要應用程式組件 (需編譯)
    │   └── ES.pdb                      ← 除錯符號檔 (選用)
    └── Views/
        ├── Login/
        │   ├── New.cshtml              ← 新使用者註冊頁面 (已修改)
        │   └── Edit1.cshtml            ← 會員資料編輯頁面 (已修改)
        ├── ServiceLst/
        │   └── Notice.cshtml           ← 服務項目說明頁面 (已修改)
        └── Shared/
            └── _Footer.cshtml          ← 頁尾 (已修改)
```

### 需要部署的檔案

| 檔案路徑                                | 說明                      |
| --------------------------------------- | ------------------------- |
| `source/bin/ES.dll`                     | 主要應用程式組件          |
| `source/bin/ES.pdb`                     | 除錯符號檔 (選用)         |
| `source/Views/Login/New.cshtml`         | 新使用者註冊頁面          |
| `source/Views/Login/Edit1.cshtml`       | 會員資料編輯頁面          |
| `source/Views/ServiceLst/Notice.cshtml` | 服務項目說明頁面          |
| `source/Views/Shared/_Footer.cshtml`    | 頁尾共用元件              |
| `update-database-servicetel.sql`        | 資料庫更新腳本 **(新增)** |
| `backup_app_offline.htm.template`       | 維護頁面模板              |

> **注意**: 本次修改涉及 C# 程式碼 (Model 驗證屬性、DAO 驗證邏輯)，需要編譯 DLL。
> 請先執行 `build-release.ps1` 編譯專案，將產生的 `bin/ES.dll` 複製到 `source/bin/` 目錄。

### 已修改的 C# 檔案 (需編譯)

| 檔案路徑                      | 修改內容                                                           |
| ----------------------------- | ------------------------------------------------------------------ |
| `ES/DataLayers/LoginDAO.cs`   | 個人會員電話驗證邏輯改為非必填                                     |
| `ES/Models/LoginViewModel.cs` | `LoginDetailModel.TEL` 移除 `[Required]`                           |
| `ES/Models/MemberModels.cs`   | `PersonalMember.Tel`、`PersonalMemberForApp.Tel` 移除 `[Required]` |

---

## 部署方式

### 方式一：使用自動化腳本 (建議)

此方式使用 `app_offline.htm` 優雅地暫停網站，並回收應用程式集區，**無需重啟整個系統**。

#### 前置條件

1. 以**系統管理員**身分執行 PowerShell
2. 確認 IIS 網站名稱和應用程式集區名稱
3. 準備好編譯完成的 build 輸出檔案

#### 執行步驟

```powershell
# 1. 以系統管理員身分開啟 PowerShell

# 2. 切換到 deploy 目錄
cd C:\path\to\deploy

# 3. 執行部署腳本
.\deploy-user-registration-fix.ps1 `
    -SourcePath "C:\Build\e-service" `
    -TargetPath "D:\WebSite\e-service" `
    -BackupPath "D:\Backup\e-service" `
    -SiteName "e-service" `
    -AppPoolName "eServicePool"
```

#### 參數說明

| 參數           | 說明                    | 預設值                |
| -------------- | ----------------------- | --------------------- |
| `-SourcePath`  | Build 輸出目錄 (必填)   | -                     |
| `-TargetPath`  | 正式環境網站目錄 (必填) | -                     |
| `-BackupPath`  | 備份存放目錄            | `D:\Backup\e-service` |
| `-SiteName`    | IIS 網站名稱            | `e-service`           |
| `-AppPoolName` | 應用程式集區名稱        | `eServicePool`        |

---

### 方式二：手動部署

如果無法使用自動化腳本，請依照以下步驟手動部署：

#### Step 1: 備份現有檔案

```powershell
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "D:\Backup\e-service\$timestamp"
$targetPath = "D:\WebSite\e-service"

# 建立備份目錄
New-Item -ItemType Directory -Path "$backupDir\Views\Login" -Force
New-Item -ItemType Directory -Path "$backupDir\bin" -Force

# 備份檔案
Copy-Item "$targetPath\Views\Login\New.cshtml" "$backupDir\Views\Login\"
Copy-Item "$targetPath\Views\Login\Edit1.cshtml" "$backupDir\Views\Login\"
Copy-Item "$targetPath\bin\ES.dll" "$backupDir\bin\"
Copy-Item "$targetPath\bin\ES.pdb" "$backupDir\bin\"
```

#### Step 2: 放置 app_offline.htm (暫停網站)

使用網站目錄中的模板檔案：

```powershell
# 複製模板檔案啟用維護模式
Copy-Item "$targetPath\backup_app_offline.htm.template" "$targetPath\app_offline.htm"
```

> **注意**: `backup_app_offline.htm.template` 已包含在版本庫中，部署時會自動使用此模板。

#### Step 3: 複製更新檔案

```powershell
$sourcePath = "C:\Build\e-service"
$targetPath = "D:\WebSite\e-service"

Copy-Item "$sourcePath\Views\Login\New.cshtml" "$targetPath\Views\Login\" -Force
Copy-Item "$sourcePath\Views\Login\Edit1.cshtml" "$targetPath\Views\Login\" -Force
Copy-Item "$sourcePath\bin\ES.dll" "$targetPath\bin\" -Force
Copy-Item "$sourcePath\bin\ES.pdb" "$targetPath\bin\" -Force
```

#### Step 4: 更新資料庫設定值

```powershell
# 使用 SQL Server Management Studio 或 sqlcmd 執行 SQL 腳本
sqlcmd -S <ServerName> -d eservice_new -i update-database-servicetel.sql

# 或手動執行以下 SQL:
# UPDATE SETUP
# SET SETUP_VAL = '(02)7730-7378',
#     UPD_TIME = GETDATE(),
#     UPD_FUN_CD = 'DEPLOY-20251204',
#     UPD_ACC = 'SYSTEM'
# WHERE SETUP_CD = 'SERVICETEL';
```

#### Step 5: 移除 app_offline.htm (恢復網站)

```powershell
Remove-Item "D:\WebSite\e-service\app_offline.htm" -Force
```

#### Step 6: 回收應用程式集區

```powershell
# 方法 1: 使用 PowerShell WebAdministration 模組
Import-Module WebAdministration
Restart-WebAppPool -Name "eServicePool"

# 方法 2: 使用 appcmd
C:\Windows\System32\inetsrv\appcmd.exe recycle apppool /apppool.name:eServicePool
```

---

## 還原 (Rollback)

如果部署後發現問題，可使用以下方式還原：

### 使用還原腳本

```powershell
.\rollback-deployment.ps1 `
    -BackupDir "D:\Backup\e-service\20251204_143000" `
    -TargetPath "D:\WebSite\e-service" `
    -AppPoolName "eServicePool"
```

### 手動還原

```powershell
$backupDir = "D:\Backup\e-service\20251204_143000"  # 替換為實際備份目錄
$targetPath = "D:\WebSite\e-service"

Copy-Item "$backupDir\Views\Login\*" "$targetPath\Views\Login\" -Force
Copy-Item "$backupDir\bin\*" "$targetPath\bin\" -Force

# 回收應用程式集區
Restart-WebAppPool -Name "eServicePool"
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

- [ ] `/Servicelst/notice` - 服務項目說明頁面

  - 聯絡窗口顯示「柏通股份有限公司」

- [ ] 任何頁面的頁尾
  - 顯示「維護廠商: 柏通股份有限公司 服務專線: 02-7730-7378」

### 3. 資料庫設定驗證

```sql
-- 驗證 SERVICETEL 設定值
SELECT SETUP_CD, SETUP_DESC, SETUP_VAL
FROM SETUP
WHERE SETUP_CD = 'SERVICETEL';

-- 預期結果: SETUP_VAL = '(02)7730-7378'
```

### 4. 登入錯誤訊息測試

- [ ] 輸入錯誤的帳號密碼
  - 應顯示：「帳號或密碼錯誤! 帳號登入失敗若達五次將被鎖定，請諮詢系統操作服務諮詢電話：(02)7730-7378」
- [ ] 連續錯誤 5 次後
  - 應顯示：「鎖定 15 分鐘，請諮詢系統操作服務諮詢電話：(02)7730-7378。」

---

## 停機時間說明

| 部署方式         | 預估停機時間 | 說明                                     |
| ---------------- | ------------ | ---------------------------------------- |
| 自動化腳本       | **< 30 秒**  | 使用 app_offline.htm，使用者看到維護頁面 |
| 手動部署         | **< 1 分鐘** | 依操作熟練度而定                         |
| 應用程式集區回收 | **< 5 秒**   | 僅回收集區，不影響其他網站               |

### 為何不需要重啟系統？

1. **app_offline.htm 機制**: IIS 偵測到此檔案會優雅地停止接受新請求
2. **應用程式集區回收**: 僅重啟該網站的工作程序，不影響：
   - 其他 IIS 網站
   - Windows 系統服務
   - 資料庫連線 (會重新建立)
3. **View 檔案 (.cshtml)**: Razor View 會在下次請求時自動重新編譯

---

## 注意事項

### ⚠️ 部署前確認

1. 確認已在測試環境完成測試
2. 確認有足夠的磁碟空間存放備份
3. 確認正式環境的 IIS 網站名稱和應用程式集區名稱
4. 建議在離峰時段執行部署

### ⚠️ 權限需求

- 需要**系統管理員**權限執行 PowerShell 腳本
- 需要 IIS 管理權限 (回收應用程式集區)
- 需要網站目錄的讀寫權限

### ⚠️ 緊急聯絡

如果部署過程中遇到問題：

1. 立即執行還原腳本恢復舊版本
2. 確認 app_offline.htm 已被移除
3. 聯繫系統管理員

---

## 維護頁面模板

網站目錄中包含 `backup_app_offline.htm.template` 檔案，用於部署時的維護頁面。

### 檔案命名說明

| 檔名                              | 用途                           |
| --------------------------------- | ------------------------------ |
| `backup_app_offline.htm.template` | 模板檔案，不會觸發維護模式     |
| `app_offline.htm`                 | 啟用維護模式（部署時自動建立） |

### 為何使用 prefix + suffix 命名？

- **prefix `backup_`**: 明確標示這是備份/模板檔案
- **suffix `.template`**: 避免被 `.gitignore` 的 `*.bak` 規則排除
- **雙重保護**: 確保不會意外觸發維護模式

### 手動啟用/停用維護模式

```powershell
# 啟用維護模式
Copy-Item "backup_app_offline.htm.template" "app_offline.htm"

# 停用維護模式
Remove-Item "app_offline.htm"
```

---

_文件建立日期: 2025-12-04_
