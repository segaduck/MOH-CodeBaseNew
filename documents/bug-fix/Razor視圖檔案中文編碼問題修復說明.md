# Razor 視圖檔案中文編碼問題修復說明

> **文件版本**: 1.0  
> **建立日期**: 2025/12/05  
> **專案**: EECOnline 民眾線上申辦電子病歷服務平台管理系統

---

## 一、問題描述

### 1.1 現象

在 `/A3/C101M` 和 `/A3/C104M` 頁面中，中文字元顯示為亂碼，例如：

| 原本顯示 | 正確內容 |
|----------|----------|
| `??亥岷` | 送出查詢 |
| `皜?‵` | 清除重填 |
| `?臬鞈?` | 匯出報表 |
| `?臬隢狡瑼?` | 匯出請款檔 |
| `銝隢狡瑼?` | 匯入請款檔 |
| `?亥岷蝯?` | 查詢結果 |
| `閮蝺刻?` | 訂單編號 |
| `?唾?鈭?` | 申請人 |
| `?唾??恍` | 申請醫院 |

### 1.2 影響範圍

- `trunk/Areas/A3/Views/C101M/Index.cshtml`
- `trunk/Areas/A3/Views/C104M/Index1.cshtml`
- `trunk/Areas/A3/Views/C104M/Index2.cshtml`

---

## 二、根本原因分析

### 2.1 編碼問題的本質

此問題源於 **檔案編碼格式不正確**。ASP.NET Razor 視圖引擎在解析 `.cshtml` 檔案時，需要正確識別檔案的字元編碼。

### 2.2 UTF-8 與 UTF-8 BOM 的差異

| 編碼格式 | 檔案開頭位元組 | 說明 |
|----------|----------------|------|
| UTF-8 (無 BOM) | 直接是內容 | 某些系統可能無法正確識別為 UTF-8 |
| UTF-8 (有 BOM) | `EF BB BF` | 明確標示此檔案為 UTF-8 編碼 |

**BOM (Byte Order Mark)** 是一個位於檔案開頭的特殊標記，用於告訴程式此檔案使用何種編碼。

### 2.3 為什麼會發生編碼錯誤？

1. **檔案編輯工具問題**：
   - 某些文字編輯器（如 Notepad++、VS Code）預設儲存為 UTF-8 無 BOM
   - Windows 記事本在舊版本中預設使用 ANSI 編碼
   - Git 或版本控制系統在某些情況下可能改變檔案編碼

2. **檔案複製或轉換**：
   - 從其他系統複製檔案時，編碼可能被轉換
   - FTP 傳輸時若使用文字模式，可能導致編碼轉換

3. **開發工具自動處理**：
   - 某些 IDE 在儲存時可能自動轉換編碼
   - 程式碼格式化工具可能改變檔案編碼

### 2.4 ASP.NET Razor 引擎的編碼處理

ASP.NET Razor 視圖引擎處理 `.cshtml` 檔案時：

1. **有 BOM 的 UTF-8 檔案**：引擎可以立即識別編碼，正確解析中文
2. **無 BOM 的 UTF-8 檔案**：引擎可能誤判為系統預設編碼（如 Windows-1252 或 Big5），導致亂碼
3. **其他編碼**：若檔案實際是 Big5 或 GB2312，但被當作 UTF-8 讀取，也會產生亂碼

### 2.5 亂碼產生的技術原理

以「送出查詢」為例：

```
正確的 UTF-8 編碼: E9 80 81 E5 87 BA E6 9F A5 E8 A9 A2
```

當這些位元組被錯誤地以其他編碼（如 Big5 或 Windows-1252）解讀時：
- 多位元組字元被拆開解讀
- 產生無意義的字元組合
- 顯示為 `??亥岷` 等亂碼

---

## 三、修復方法

### 3.1 修復步驟

1. **重新撰寫檔案內容**：使用正確的 UTF-8 編碼重新寫入檔案
2. **加入 UTF-8 BOM**：在檔案開頭加入 `EF BB BF` 位元組標記

### 3.2 PowerShell 加入 BOM 的方法

```powershell
# 讀取檔案內容
$content = Get-Content -Path "檔案路徑.cshtml" -Raw -Encoding UTF8

# 建立包含 BOM 的 UTF-8 編碼器
$Utf8BomEncoding = New-Object System.Text.UTF8Encoding $True

# 使用 BOM 編碼寫入檔案
[System.IO.File]::WriteAllText("檔案路徑.cshtml", $content, $Utf8BomEncoding)
```

### 3.3 驗證檔案是否有 BOM

```powershell
# 讀取檔案前 3 個位元組
Get-Content -Path "檔案路徑.cshtml" -Encoding Byte -TotalCount 3 | 
    ForEach-Object { '{0:X2}' -f $_ }

# 正確結果應顯示: EF BB BF
```

---

## 四、預防措施

### 4.1 Visual Studio 設定

1. 開啟 Visual Studio
2. 工具 → 選項 → 文字編輯器 → 進階
3. 確認「儲存文件時不使用 UTF-8 簽章」選項為 **未勾選**

### 4.2 VS Code 設定

在 `settings.json` 中加入：

```json
{
    "files.encoding": "utf8bom",
    "[razor]": {
        "files.encoding": "utf8bom"
    },
    "[csharp]": {
        "files.encoding": "utf8bom"
    }
}
```

### 4.3 Git 設定

在 `.gitattributes` 中加入：

```
*.cshtml text eol=crlf encoding=utf-8-bom
*.cs text eol=crlf encoding=utf-8-bom
```

### 4.4 開發規範建議

1. **統一編輯器設定**：團隊成員使用相同的編碼設定
2. **定期檢查**：使用腳本定期檢查 `.cshtml` 檔案的編碼
3. **程式碼審查**：在 PR 審查時注意編碼問題
4. **自動化檢測**：在 CI/CD 流程中加入編碼檢查

---

## 五、修復紀錄

### 5.1 已修復檔案清單

| 檔案路徑 | 修復日期 | 說明 |
|----------|----------|------|
| `Areas/A3/Views/C101M/Index.cshtml` | 2025/12/05 | 重寫檔案並加入 UTF-8 BOM |
| `Areas/A3/Views/C104M/Index1.cshtml` | 2025/12/05 | 重寫檔案並加入 UTF-8 BOM |
| `Areas/A3/Views/C104M/Index2.cshtml` | 2025/12/05 | 重寫檔案並加入 UTF-8 BOM |

### 5.2 修復後驗證

1. ✅ `/A3/C101M` 頁面中文正常顯示
2. ✅ `/A3/C104M` 頁面中文正常顯示
3. ✅ 按鈕文字（送出查詢、清除重填等）正確
4. ✅ 表格標題（訂單編號、申請人等）正確
5. ✅ 分頁標籤（依申請案件明細、依醫院彙總統計）正確

---

## 六、檢測腳本

### 6.1 批次檢查所有 cshtml 檔案編碼

```powershell
# 檢查所有 cshtml 檔案是否有 UTF-8 BOM
$files = Get-ChildItem -Path "trunk" -Filter "*.cshtml" -Recurse

foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $hasBOM = ($bytes.Length -ge 3) -and 
              ($bytes[0] -eq 0xEF) -and 
              ($bytes[1] -eq 0xBB) -and 
              ($bytes[2] -eq 0xBF)
    
    if (-not $hasBOM) {
        Write-Host "缺少 BOM: $($file.FullName)" -ForegroundColor Yellow
    }
}
```

### 6.2 批次修復缺少 BOM 的檔案

```powershell
# 為所有缺少 BOM 的 cshtml 檔案加入 BOM
$files = Get-ChildItem -Path "trunk" -Filter "*.cshtml" -Recurse

foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $hasBOM = ($bytes.Length -ge 3) -and 
              ($bytes[0] -eq 0xEF) -and 
              ($bytes[1] -eq 0xBB) -and 
              ($bytes[2] -eq 0xBF)
    
    if (-not $hasBOM) {
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        $Utf8BomEncoding = New-Object System.Text.UTF8Encoding $True
        [System.IO.File]::WriteAllText($file.FullName, $content, $Utf8BomEncoding)
        Write-Host "已修復: $($file.FullName)" -ForegroundColor Green
    }
}
```

---

## 七、相關資源

- [Microsoft Docs: Razor syntax reference](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/razor)
- [Unicode BOM FAQ](https://unicode.org/faq/utf_bom.html)
- [Character encoding in .NET](https://docs.microsoft.com/en-us/dotnet/standard/base-types/character-encoding)

---

## 八、結論

Razor 視圖檔案的中文編碼問題是 ASP.NET MVC 開發中常見的問題。關鍵要點：

1. **始終使用 UTF-8 with BOM** 儲存包含中文的 `.cshtml` 檔案
2. **統一開發環境設定**，確保團隊成員使用相同的編碼配置
3. **建立自動化檢查機制**，在開發流程中及早發現編碼問題
4. **文件化編碼規範**，讓新進成員了解專案的編碼要求

透過這些措施，可以有效預防和快速修復中文編碼問題。
