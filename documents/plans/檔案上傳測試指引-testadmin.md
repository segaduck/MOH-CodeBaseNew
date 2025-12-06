# 檔案上傳測試指引 - testadmin 用戶

> **建立日期**: 2025-12-06  
> **測試用戶**: testadmin  
> **測試環境**: http://localhost:8080  
> **目的**: 提供完整的檔案上傳功能測試流程

---

## 一、測試資料概要

### 已建立的測試資料

✅ **10 個測試案件已建立完成**

| 項目 | 數量 | 說明 |
|-----|:----:|------|
| **測試案件** | 10 個 | 編號 001-010 |
| **可上傳項目** | 約 25 筆 | 各種病歷類型 |
| **測試病患** | 1 位 | ID: A123456789 |

### 測試病患資訊

| 欄位 | 內容 |
|-----|------|
| **身分證字號** | A123456789 |
| **姓名** | 測試病患 |
| **生日** | 1990/01/01 |
| **醫院** | 亞東紀念醫院 |

### 病歷類型分布

**案件 1-5** (每案 2-3 筆病歷):
- 門診病歷 (01)
- 住院病歷 (02)
- X光報告 (04) - 僅奇數案件

**案件 6-10** (每案 2-3 筆病歷):
- 門診病歷 (01)
- 檢驗報告 (03)
- X光報告 (04) - 僅奇數案件

---

## 二、測試前準備

### 1. 確認測試環境

```bash
# 檢查 SQL Server 容器狀態
docker ps | findstr sqlserver

# 應顯示: moh-sqlserver ... Up ... 0.0.0.0:1433->1433/tcp
```

### 2. 準備測試檔案

建議準備以下測試檔案：

| 檔案類型 | 用途 | 大小建議 |
|---------|------|---------|
| **test.pdf** | 正常 PDF 上傳測試 | < 5MB |
| **test.jpg** | 正常圖片上傳測試 | < 2MB |
| **test-large.pdf** | 大檔案測試 (應拒絕) | > 20MB |
| **test.txt** | 錯誤格式測試 (應拒絕) | 任意 |
| **fake.pdf** | 偽造副檔名測試 | 將 .txt 改名為 .pdf |

### 3. 登入後台系統

1. 開啟瀏覽器訪問: `http://localhost:8080`
2. 使用以下帳號登入:
   - **帳號**: `testadmin`
   - **密碼**: `Test@1234`

---

## 三、測試步驟

### 測試案例 1: 正常上傳流程

#### 步驟 1: 進入病歷補上傳頁面

```
URL: http://localhost:8080/A2/C102M/Index
```

#### 步驟 2: 搜尋測試病患

在搜尋欄位輸入:
- **身分證字號**: `A123456789`
- 點擊「查詢」按鈕

**預期結果**: 顯示約 25 筆待上傳的病歷記錄

#### 步驟 3: 上傳病歷檔案

1. 在任一記錄列點擊「上傳檔案」按鈕
2. 彈出上傳視窗
3. 點擊「選擇檔案」
4. 選擇 `test.pdf` 或 `test.jpg`
5. 點擊「儲存」按鈕

**預期結果**: 
- ✅ 上傳成功
- ✅ 視窗自動關閉
- ✅ 列表頁顯示「已上傳」狀態

#### 步驟 4: 驗證資料庫

```sql
-- 查詢上傳記錄
SELECT 
    apply_no_sub AS '申請編號',
    his_type_name AS '病歷類型',
    CASE WHEN provide_bin IS NULL THEN '未上傳' ELSE '已上傳' END AS '狀態',
    LEN(provide_bin) AS 'Base64長度',
    provide_ext AS '副檔名',
    provide_datetime AS '上傳時間'
FROM EEC_ApplyDetailPrice
WHERE user_idno = 'A123456789'
    AND provide_bin IS NOT NULL
ORDER BY provide_datetime DESC
```

**預期結果**: 
- `provide_bin` 欄位包含 Base64 編碼內容
- `provide_ext` 顯示副檔名 (如 `.pdf`, `.jpg`)
- `provide_datetime` 顯示上傳時間

---

### 測試案例 2: 副檔名驗證測試

#### 2.1 上傳不允許的副檔名

**測試檔案**: `test.txt` (純文字檔)

**操作**:
1. 點擊「上傳檔案」
2. 選擇 `test.txt`

**預期結果**: 
- ❌ 前端 JavaScript 立即提示錯誤
- ❌ 訊息: 「請選擇正確的檔案格式！ (.PDF,.JPG,.BMP,.PNG,.GIF,.TIF)」
- ❌ 檔案選擇框被清空

#### 2.2 偽造副檔名攻擊測試

**準備**:
```bash
# 建立測試檔案
echo "This is a text file" > test.txt
# 重新命名為 .pdf
copy test.txt fake.pdf
```

**操作**:
1. 點擊「上傳檔案」
2. 選擇 `fake.pdf`
3. 點擊「儲存」

**預期結果 (目前狀態)**:
- ⚠️ **可能成功上傳** (安全漏洞)
- ⚠️ 因為缺乏 Magic Bytes 驗證

**預期結果 (修復後)**:
- ❌ 應被後端攔截
- ❌ 提示: 「檔案類型不符」

---

### 測試案例 3: 檔案大小限制測試

#### 3.1 上傳超大檔案

**測試檔案**: > 20MB 的 PDF

**操作**:
1. 點擊「上傳檔案」
2. 選擇大於 20MB 的檔案

**預期結果**:
- ❌ 前端 JavaScript 立即提示錯誤
- ❌ 訊息: 「檔案大小以 20MB 為限！」
- ❌ 檔案選擇框被清空

#### 3.2 邊界值測試

**測試檔案**: 
- 19.9MB (應成功)
- 20.0MB (應成功)
- 20.1MB (應失敗)

---

### 測試案例 4: 多次上傳同一案件

**目的**: 測試是否可覆蓋已上傳的檔案

**操作**:
1. 上傳 `test1.pdf` 到案件 001 的門診病歷
2. 再次點擊同一記錄的「上傳檔案」
3. 上傳 `test2.pdf`

**預期結果**:
- ✅ 第二次上傳成功
- ✅ `provide_bin` 被覆蓋為新檔案的 Base64
- ✅ `provide_datetime` 更新為最新時間

---

### 測試案例 5: 不同病歷類型上傳

**操作**: 依序上傳不同類型病歷

| 案件編號 | 病歷類型 | 測試檔案 |
|---------|---------|---------|
| 001 | 門診病歷 | test1.pdf |
| 001 | 住院病歷 | test2.pdf |
| 001 | X光報告 | test-xray.jpg |

**預期結果**:
- ✅ 每種類型都能獨立上傳
- ✅ 互不影響

---

## 四、測試資料管理

### 重置測試資料 (清空已上傳檔案)

當你想重新測試上傳功能時，執行以下命令:

```bash
# 進入專案目錄
cd F:\AITest\MOH-CodeBaseNew

# 複製 SQL 腳本到容器
docker cp scripts/reset-upload-test-cases.sql moh-sqlserver:/tmp/reset.sql

# 執行重置腳本
docker exec moh-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -d EEC_PD_DB -C -i /tmp/reset.sql
```

**腳本功能**:
- 清空 `provide_bin` (Base64 檔案內容)
- 清空 `provide_ext` (副檔名)
- 清空 `provide_datetime` (上傳時間)
- 重置 `provide_status` 為 '2' (待上傳)
- 清空 `download_count` (下載次數)

**執行後**: 所有 25 筆記錄恢復為「待上傳」狀態

---

## 五、進階測試 (安全測試)

⚠️ **以下為安全漏洞驗證測試，僅供開發環境使用**

### 5.1 Webshell 上傳測試 (模擬攻擊)

**目的**: 驗證是否能上傳可執行的惡意檔案

**準備**:
```aspx
<!-- test-webshell.aspx -->
<%@ Page Language="C#" %>
<% Response.Write("Webshell Test"); %>
```

**操作**:
1. 將 `test-webshell.aspx` 重新命名為 `test-webshell.pdf`
2. 嘗試上傳

**預期結果 (目前)**:
- ⚠️ 可能成功上傳 (因為只檢查副檔名)
- ⚠️ Base64 儲存至資料庫

**預期結果 (修復後)**:
- ❌ 後端 Magic Bytes 驗證失敗
- ❌ 拒絕上傳

### 5.2 雙重副檔名測試

**測試檔案**: `shell.aspx.pdf`

**操作**: 嘗試上傳

**預期結果 (目前)**:
- ✅ 前端檢查 `.pdf` 通過
- ⚠️ 後端儲存可能有風險

**預期結果 (修復後)**:
- ❌ 偵測到雙重副檔名
- ❌ 拒絕上傳

---

## 六、測試檢查清單

### 功能測試

- [ ] 正常上傳 PDF 檔案
- [ ] 正常上傳 JPG 檔案
- [ ] 正常上傳 PNG 檔案
- [ ] 上傳後資料庫正確儲存 Base64
- [ ] 可重複上傳覆蓋檔案
- [ ] 不同病歷類型獨立上傳

### 驗證測試

- [ ] 拒絕 .txt 檔案
- [ ] 拒絕 .exe 檔案
- [ ] 拒絕 .aspx 檔案
- [ ] 拒絕超過 20MB 檔案
- [ ] 檔案大小邊界值測試

### 安全測試

- [ ] 偽造副檔名 (.txt → .pdf) - **應被攔截但目前可能通過**
- [ ] 雙重副檔名 (shell.aspx.pdf) - **需驗證**
- [ ] Webshell 上傳測試 - **應被攔截但目前可能通過**
- [ ] SQL Injection 測試 (檔名特殊字元)
- [ ] Path Traversal (檔名包含 ../)

### 效能測試

- [ ] 上傳 1MB 檔案 (應 < 3 秒)
- [ ] 上傳 10MB 檔案 (應 < 10 秒)
- [ ] 上傳 20MB 檔案 (應 < 20 秒)
- [ ] 連續上傳 10 個檔案

---

## 七、常見問題排除

### Q1: 找不到測試病患資料

**解決方法**:
```bash
# 重新執行建立測試資料腳本
docker cp scripts/create-10-upload-test-cases-for-testadmin.sql moh-sqlserver:/tmp/create.sql
docker exec moh-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -d EEC_PD_DB -C -i /tmp/create.sql
```

### Q2: 上傳後找不到記錄

**檢查**:
1. 確認登入的是 `testadmin` 帳號
2. 搜尋條件輸入 `A123456789`
3. 檢查 `provide_status` 是否為 '2' (待上傳)

### Q3: 上傳成功但無法重新測試

**解決方法**:
執行重置腳本 (參見第四節)

### Q4: 彈出視窗無法開啟

**可能原因**:
- 瀏覽器阻擋彈出視窗
- JavaScript 錯誤

**解決方法**:
1. 允許瀏覽器彈出視窗
2. 檢查瀏覽器 Console (F12) 是否有錯誤訊息

---

## 八、測試報告範本

### 測試記錄表

| 測試編號 | 測試項目 | 測試檔案 | 預期結果 | 實際結果 | 狀態 | 備註 |
|:-------:|---------|---------|---------|---------|:----:|------|
| TC-01 | 正常上傳 PDF | test.pdf (2MB) | 成功 | 成功 | ✅ | - |
| TC-02 | 拒絕 TXT | test.txt | 拒絕 | 拒絕 | ✅ | 前端攔截 |
| TC-03 | 偽造副檔名 | fake.pdf (實為 txt) | 拒絕 | **成功** | ❌ | **安全漏洞** |
| TC-04 | 超大檔案 | large.pdf (25MB) | 拒絕 | 拒絕 | ✅ | 前端攔截 |
| ... | ... | ... | ... | ... | ... | ... |

---

## 九、參考資料

### 相關文件

- [檔案上傳安全性改善計畫](./檔案上傳安全性改善計畫.md)
- [檔案上傳功能清單與測試指引](./檔案上傳功能清單與測試指引.md)

### SQL 腳本

- 建立測試資料: `scripts/create-10-upload-test-cases-for-testadmin.sql`
- 重置測試資料: `scripts/reset-upload-test-cases.sql`

### 測試環境資訊

| 項目 | 資訊 |
|-----|------|
| 應用程式 URL | http://localhost:8080 |
| SQL Server 容器 | moh-sqlserver |
| 資料庫 | EEC_PD_DB |
| 測試帳號 | testadmin / Test@1234 |
| 測試病患 | A123456789 (測試病患) |

---

_文件建立日期: 2025-12-06_  
_最後更新: 2025-12-06_  
_測試狀態: ✅ 測試資料已建立，可開始測試_
